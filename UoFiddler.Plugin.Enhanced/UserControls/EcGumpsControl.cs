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
    public sealed class EcGumpsControl : UserControl, IEcEditor
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

        private readonly ToolStripComboBox _sourceBox;
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripComboBox _sizeBox;
        private readonly ToolStripButton _saveButton;
        private readonly EcThumbnailGrid _grid;
        private readonly EcPreviewPanel _preview;
        private readonly Label _info;
        private readonly ToolStripStatusLabel _statusLabel;

        private EcClient _client;
        private bool _loaded;
        private Source _source;
        private EcUopPackage _package;
        private List<(int Id, EcUopEntry Entry)> _items = new List<(int, EcUopEntry)>();

        public EcGumpsControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _sourceBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
            _sourceBox.Items.AddRange(_sources.Cast<object>().ToArray());
            _sourceBox.SelectedIndexChanged += (s, e) => { if (_loaded) { LoadSource(); } };
            _searchBox = new ToolStripTextBox { Width = 90, ToolTipText = "Gump id – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Search(); } };
            _sizeBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 60 };
            _sizeBox.Items.AddRange(new object[] { "64", "96", "128", "192" });
            _sizeBox.SelectedIndex = 2;
            _sizeBox.SelectedIndexChanged += (s, e) => _grid.CellSize = int.Parse((string)_sizeBox.SelectedItem, CultureInfo.InvariantCulture);
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(_sourceBox);
            toolStrip.Items.Add(new ToolStripLabel("Id:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Search", null, (s, e) => Search()));
            toolStrip.Items.Add(new ToolStripLabel("Size:"));
            toolStrip.Items.Add(_sizeBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Replace...", null, (s, e) => Replace()));
            toolStrip.Items.Add(new ToolStripButton("Insert...", null, (s, e) => Insert()));
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => RemoveSelected()));
            var exportButton = new ToolStripDropDownButton("Export");
            exportButton.DropDownItems.Add("Selected as PNG...", null, (s, e) => ExportSelected());
            exportButton.DropDownItems.Add("All as PNG to folder...", null, (s, e) => ExportAll());
            toolStrip.Items.Add(exportButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _grid = new EcThumbnailGrid { Dock = DockStyle.Fill, CellSize = 128 };
            _grid.ImageRequested += index => index < _items.Count ? Decode(_items[index].Entry, out _) : null;
            _grid.LabelRequested += index => index < _items.Count ? _items[index].Id + (_items[index].Entry.IsModified ? "*" : string.Empty) : string.Empty;
            _grid.SelectionChanged += (s, e) => ShowSelected();

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill };
            _info = new Label { Dock = DockStyle.Top, Height = 50, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };
            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(_preview);
            right.Controls.Add(_info);

            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
            split.Panel1.Controls.Add(_grid);
            split.Panel2.Controls.Add(right);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = Math.Max(300, split.Width - 420); } catch (InvalidOperationException) { } };

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
            _loaded = false;
            _package = null;
            _items = new List<(int, EcUopEntry)>();
            _grid.ItemCount = 0;
            _preview.Image = null;
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
            if (_sourceBox.SelectedIndex < 0)
            {
                _sourceBox.SelectedIndex = 0;
            }
            _loaded = true;
            LoadSource();
        }

        private void LoadSource()
        {
            _source = _sourceBox.SelectedItem as Source;
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
                    _grid.ItemCount = 0;
                    _statusLabel.Text = $"{_source.File} was not found.";
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
            _saveButton.Enabled = _sources.Any(s => _client.IsOpen(s.File) && (_client.GetPackage(s.File)?.IsDirty ?? false));
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
            _grid.ItemCount = _items.Count;
            _statusLabel.Text = $"{_source.File}: {_items.Count} gumps.";
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

        private (int Id, EcUopEntry Entry)? Selected => _grid.SelectedIndex >= 0 && _grid.SelectedIndex < _items.Count ? _items[_grid.SelectedIndex] : ((int, EcUopEntry)?)null;

        private void ShowSelected()
        {
            var sel = Selected;
            if (sel == null)
            {
                _preview.Image = null;
                _info.Text = string.Empty;
                return;
            }
            try
            {
                _preview.Image = Decode(sel.Value.Entry, out string info);
                _info.Text = $"Gump {sel.Value.Id} (0x{sel.Value.Id:X})\r\n{sel.Value.Entry.Name}\r\n{info ?? "empty"}";
            }
            catch (Exception ex)
            {
                _preview.Image = null;
                _info.Text = ex.Message;
            }
        }

        private void Search()
        {
            if (!EcDialogs.TryParseNumber(_searchBox.Text, out long id))
            {
                return;
            }
            int index = _items.FindIndex(i => i.Id >= id);
            if (index >= 0)
            {
                _grid.SelectedIndex = index;
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
                _grid.InvalidateItem(_grid.SelectedIndex);
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
                _searchBox.Text = id.ToString(CultureInfo.InvariantCulture);
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
            int index = _grid.SelectedIndex;
            _package.Remove(sel.Value.Entry);
            BuildItems();
            _grid.SelectedIndex = Math.Min(index, _items.Count - 1);
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
                            _statusLabel.Text = $"Exporting {done}...";
                            Application.DoEvents();
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                _statusLabel.Text = $"{done} gumps exported.";
            }
        }

        private void Save(bool saveAs)
        {
            var packages = _sources.Where(s => _client != null && _client.IsOpen(s.File)).Select(s => _client.GetPackage(s.File));
            if (EcPackageSaver.Save(this, packages, saveAs))
            {
                _saveButton.Enabled = false;
                _grid.ClearCache();
                _grid.Invalidate();
                _statusLabel.Text = "Saved.";
            }
        }
    }
}

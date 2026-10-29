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
using UoFiddler.Plugin.Enhanced.EcUltima.Multis;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for MultiCollection.uop (houses, boats, ...): list, parts table, isometric preview with the EC item images.
    /// </summary>
    public sealed class EcMultisControl : UserControl, IEcEditor
    {
        public const string FileName = "MultiCollection.uop";

        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly DataGridView _parts;
        private readonly EcPreviewPanel _preview;
        private readonly NumericUpDown _maxZBox;
        private readonly ToolStripStatusLabel _statusLabel;

        private EcClient _client;
        private EcUopPackage _package;
        private bool _loaded;
        private readonly List<(int Id, EcUopEntry Entry)> _multis = new List<(int, EcUopEntry)>();
        private EcMulti _current;
        private EcUopEntry _currentEntry;
        private bool _filling;
        private readonly Dictionary<uint, Bitmap> _images = new Dictionary<uint, Bitmap>();

        public EcMultisControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _searchBox = new ToolStripTextBox { Width = 90, ToolTipText = "Multi id – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Search(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Id:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Search", null, (s, e) => Search()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("New / copy...", null, (s, e) => AddMulti()));
            toolStrip.Items.Add(new ToolStripButton("Delete", null, (s, e) => DeleteMulti()));
            var txtButton = new ToolStripDropDownButton("Text");
            txtButton.DropDownItems.Add("Export parts (item x y z flags)...", null, (s, e) => ExportText());
            txtButton.DropDownItems.Add("Import parts (item x y z flags)...", null, (s, e) => ImportText());
            toolStrip.Items.Add(txtButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _list.Columns.Add("Id", 60, HorizontalAlignment.Right);
            _list.Columns.Add("Hex", 60);
            _list.Columns.Add("Size", 70, HorizontalAlignment.Right);
            _list.SelectedIndexChanged += (s, e) => ShowSelected();

            _parts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersWidth = 24
            };
            foreach (string c in new[] { "Item", "X", "Y", "Z", "Flags", "Clilocs" })
            {
                _parts.Columns.Add(c, c);
            }
            _parts.CellEndEdit += (s, e) => PartsChanged();
            _parts.UserDeletedRow += (s, e) => PartsChanged();

            _maxZBox = new NumericUpDown { Minimum = -128, Maximum = 512, Value = 512, Width = 60 };
            _maxZBox.ValueChanged += (s, e) => DrawPreview();
            var previewTools = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            previewTools.Controls.Add(new Label { Text = "Max z:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            previewTools.Controls.Add(_maxZBox);
            previewTools.Controls.Add(new Label { Text = "Ctrl + mouse wheel = zoom", AutoSize = true, Margin = new Padding(10, 8, 0, 0) });

            _preview = new EcPreviewPanel { Dock = DockStyle.Fill };
            var previewPanel = new Panel { Dock = DockStyle.Fill };
            previewPanel.Controls.Add(_preview);
            previewPanel.Controls.Add(previewTools);

            var rightSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            rightSplit.Panel1.Controls.Add(previewPanel);
            rightSplit.Panel2.Controls.Add(_parts);
            rightSplit.HandleCreated += (s, e) => { try { rightSplit.SplitterDistance = Math.Max(200, rightSplit.Height * 3 / 5); } catch (InvalidOperationException) { } };

            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(rightSplit);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = 220; } catch (InvalidOperationException) { } };

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
            _multis.Clear();
            _list.Items.Clear();
            _parts.Rows.Clear();
            _preview.Image = null;
            ClearImages();
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
            _package.Changed += (s, e) => _saveButton.Enabled = _package.IsDirty;
            FillList();
        }

        private void FillList()
        {
            _multis.Clear();
            foreach (EcUopEntry e in _package.Entries)
            {
                if (e.Name != null && e.Name.StartsWith("build/multicollection/", StringComparison.OrdinalIgnoreCase) && EcClient.TryGetIdFromName(e.Name, out int id))
                {
                    _multis.Add((id, e));
                }
            }
            _multis.Sort((a, b) => a.Id.CompareTo(b.Id));

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (var (id, entry) in _multis)
            {
                var item = new ListViewItem(id.ToString(CultureInfo.InvariantCulture)) { Tag = entry };
                item.SubItems.Add($"0x{id:X}");
                item.SubItems.Add(EcImageHelper.FormatSize(entry.DecompressedSize));
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            _statusLabel.Text = $"{_multis.Count} multis (housing.bin and other entries: UOP Browser).";
        }

        private void Search()
        {
            if (!EcDialogs.TryParseNumber(_searchBox.Text, out long id))
            {
                return;
            }
            foreach (ListViewItem item in _list.Items)
            {
                if (item.Text == id.ToString(CultureInfo.InvariantCulture))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }
            }
        }

        private void ShowSelected()
        {
            if (_list.SelectedItems.Count != 1)
            {
                return;
            }
            try
            {
                _currentEntry = (EcUopEntry)_list.SelectedItems[0].Tag;
                _current = EcMulti.Read(_package.ReadData(_currentEntry));
                FillParts();
                DrawPreview();
                var b = _current.Bounds();
                _statusLabel.Text = $"Multi {_current.Id}: {_current.Parts.Count} parts, x {b.MinX}..{b.MaxX}, y {b.MinY}..{b.MaxY}, z {b.MinZ}..{b.MaxZ}";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = ex.Message;
            }
        }

        private void FillParts()
        {
            _filling = true;
            _parts.Rows.Clear();
            foreach (EcMultiPart p in _current.Parts)
            {
                _parts.Rows.Add($"0x{p.ItemId:X4}", p.X, p.Y, p.Z, p.Flags, string.Join(" ", p.Clilocs));
            }
            _filling = false;
        }

        private void PartsChanged()
        {
            if (_filling || _current == null)
            {
                return;
            }
            try
            {
                var parts = new List<EcMultiPart>();
                foreach (DataGridViewRow row in _parts.Rows)
                {
                    if (row.IsNewRow || row.Cells[0].Value == null)
                    {
                        continue;
                    }
                    var p = new EcMultiPart
                    {
                        ItemId = (ushort)Num(row.Cells[0].Value),
                        X = (short)Num(row.Cells[1].Value),
                        Y = (short)Num(row.Cells[2].Value),
                        Z = (short)Num(row.Cells[3].Value),
                        Flags = (ushort)Num(row.Cells[4].Value)
                    };
                    foreach (string c in (row.Cells[5].Value?.ToString() ?? string.Empty).Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        p.Clilocs.Add((uint)Num(c));
                    }
                    parts.Add(p);
                }
                _current.Parts.Clear();
                _current.Parts.AddRange(parts);
                _package.SetData(_currentEntry, _current.Write());
                _saveButton.Enabled = true;
                DrawPreview();
            }
            catch (FormatException ex)
            {
                EcDialogs.ShowError(this, "Invalid number in the parts table.", ex);
            }
        }

        private static long Num(object value)
        {
            string text = value?.ToString() ?? "0";
            if (!EcDialogs.TryParseNumber(text, out long v))
            {
                throw new FormatException(text);
            }
            return v;
        }

        private Bitmap ItemImage(uint id)
        {
            if (_images.TryGetValue(id, out Bitmap b))
            {
                return b;
            }
            byte[] data = _client.GetItemImage(id, out _);
            b = data != null ? EcImageHelper.TryDecode(data, out _) : null;
            _images[id] = b;
            return b;
        }

        private void ClearImages()
        {
            foreach (Bitmap b in _images.Values)
            {
                b?.Dispose();
            }
            _images.Clear();
        }

        /// <summary>
        /// Isometric drawing: tile 44 x 44, 4 pixels per z, parts sorted by x + y, then z.
        /// </summary>
        private void DrawPreview()
        {
            if (_current == null || _current.Parts.Count == 0)
            {
                _preview.Image = null;
                return;
            }

            const int half = 22;
            int maxZ = (int)_maxZBox.Value;
            List<EcMultiPart> parts = _current.Parts.Where(p => p.Z <= maxZ).OrderBy(p => p.X + p.Y).ThenBy(p => p.Z).ToList();
            if (parts.Count == 0)
            {
                _preview.Image = null;
                return;
            }

            // bounds of all images
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            var placed = new List<(Bitmap Image, int X, int Y)>();
            foreach (EcMultiPart p in parts)
            {
                Bitmap img = ItemImage(p.ItemId);
                int w = img?.Width ?? 44, h = img?.Height ?? 44;
                int cx = (p.X - p.Y) * half;
                int cy = ((p.X + p.Y) * half) - (p.Z * 4);
                int x = cx - (w / 2);
                int y = cy + half - h;
                placed.Add((img, x, y));
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x + w);
                maxY = Math.Max(maxY, y + h);
            }

            int width = Math.Min(8000, maxX - minX + 20), height = Math.Min(8000, maxY - minY + 20);
            var canvas = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (Graphics g = Graphics.FromImage(canvas))
            {
                foreach (var (img, x, y) in placed)
                {
                    if (img != null)
                    {
                        g.DrawImageUnscaled(img, x - minX + 10, y - minY + 10);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.Red, x - minX + 10, y - minY + 10, 44, 44);
                    }
                }
            }
            _preview.Image = canvas;
        }

        private void AddMulti()
        {
            if (_package == null)
            {
                return;
            }
            int next = _multis.Count == 0 ? 0 : _multis.Max(m => m.Id) + 1;
            string text = EcDialogs.Prompt(this, "New multi", "Id (the selected multi is copied, nothing selected = empty):", next.ToString(CultureInfo.InvariantCulture));
            if (text == null || !EcDialogs.TryParseNumber(text, out long id) || id < 0)
            {
                return;
            }
            string name = string.Format(CultureInfo.InvariantCulture, EcMulti.NamePattern, id);
            if (_package.Contains(name) && !EcDialogs.Confirm(this, $"Multi {id} exists already. Replace it?"))
            {
                return;
            }
            var multi = new EcMulti { Id = (int)id };
            if (_current != null)
            {
                multi.Parts.AddRange(_current.Parts.Select(p => p.Clone()));
            }
            _package.AddOrReplace(name, multi.Write(), EcUopCompression.Zlib);
            _saveButton.Enabled = true;
            FillList();
            _searchBox.Text = id.ToString(CultureInfo.InvariantCulture);
            Search();
        }

        private void DeleteMulti()
        {
            if (_currentEntry == null || _list.SelectedItems.Count != 1 || !EcDialogs.Confirm(this, $"Delete multi {_current?.Id}?"))
            {
                return;
            }
            _package.Remove(_currentEntry);
            _current = null;
            _currentEntry = null;
            _parts.Rows.Clear();
            _preview.Image = null;
            _saveButton.Enabled = true;
            FillList();
        }

        private void ExportText()
        {
            if (_current == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "Text|*.txt", FileName = $"multi_{_current.Id}.txt" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var sb = new StringBuilder();
                    foreach (EcMultiPart p in _current.Parts)
                    {
                        sb.AppendLine($"0x{p.ItemId:X4} {p.X} {p.Y} {p.Z} {p.Flags}");
                    }
                    File.WriteAllText(dialog.FileName, sb.ToString());
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
                try
                {
                    var parts = new List<EcMultiPart>();
                    foreach (string line in File.ReadAllLines(dialog.FileName))
                    {
                        string[] v = line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        if (v.Length < 4)
                        {
                            continue;
                        }
                        parts.Add(new EcMultiPart
                        {
                            ItemId = (ushort)Num(v[0]),
                            X = (short)Num(v[1]),
                            Y = (short)Num(v[2]),
                            Z = (short)Num(v[3]),
                            Flags = v.Length > 4 ? (ushort)Num(v[4]) : (ushort)1
                        });
                    }
                    _current.Parts.Clear();
                    _current.Parts.AddRange(parts);
                    _package.SetData(_currentEntry, _current.Write());
                    _saveButton.Enabled = true;
                    FillParts();
                    DrawPreview();
                }
                catch (FormatException ex)
                {
                    EcDialogs.ShowError(this, "Invalid line in the file.", ex);
                }
            }
        }

        private void Save(bool saveAs)
        {
            if (_package != null && EcPackageSaver.Save(this, new[] { _package }, saveAs))
            {
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearImages();
            }
            base.Dispose(disposing);
        }
    }
}

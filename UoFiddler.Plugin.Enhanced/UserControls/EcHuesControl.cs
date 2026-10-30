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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Hues;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Editor for Hues.uop: the color ramps (256 x 2 BMP per hue) and the names.
    /// </summary>
    public sealed class EcHuesControl : UserControl, IEcEditor
    {
        public const string FileName = "Hues.uop";

        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly PictureBox _ramp;
        private readonly TextBox _nameBox;
        private readonly Label _info;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly ImageList _images = new ImageList { ImageSize = new Size(128, 14), ColorDepth = ColorDepth.Depth32Bit };

        private EcClient _client;
        private EcHues _hues;
        private bool _loaded;
        private List<EcHue> _view = new List<EcHue>();
        private readonly Dictionary<int, Bitmap> _rampCache = new Dictionary<int, Bitmap>();

        public EcHuesControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _searchBox = new ToolStripTextBox { Width = 150, ToolTipText = "Number or part of the name – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Import ramp...", null, (s, e) => ImportRamp()) { ToolTipText = "Replace the color ramp with an image (best 256 x 2)" });
            toolStrip.Items.Add(new ToolStripButton("Export ramp...", null, (s, e) => ExportRamp()));
            toolStrip.Items.Add(new ToolStripButton("Add copy...", null, (s, e) => AddCopy()) { ToolTipText = "New hue as copy of the selected one" });
            toolStrip.Items.Add(new ToolStripButton("Delete", null, (s, e) => DeleteHue()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false, OwnerDraw = true };
            _list.Columns.Add("Hue", 60, HorizontalAlignment.Right);
            _list.Columns.Add("Ramp", 140);
            _list.Columns.Add("Name", 300);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.DrawColumnHeader += (s, e) => e.DrawDefault = true;
            _list.DrawSubItem += DrawSubItem;
            _list.SelectedIndexChanged += (s, e) => ShowSelected();

            _ramp = new PictureBox { Dock = DockStyle.Top, Height = 60, SizeMode = PictureBoxSizeMode.StretchImage, BorderStyle = BorderStyle.FixedSingle };
            _info = new Label { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };
            _nameBox = new TextBox { Width = 220 };
            var nameButton = new Button { Text = "Set name", AutoSize = true };
            nameButton.Click += (s, e) => SetName();
            var namePanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34 };
            namePanel.Controls.Add(new Label { Text = "Name:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            namePanel.Controls.Add(_nameBox);
            namePanel.Controls.Add(nameButton);

            var details = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
            details.Controls.Add(namePanel);
            details.Controls.Add(_ramp);
            details.Controls.Add(_info);

            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(details);
            split.HandleCreated += (s, e) => { try { split.SplitterDistance = 560; } catch (InvalidOperationException) { } };

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
            _hues = null;
            ClearCache();
            _view = new List<EcHue>();
            _list.VirtualListSize = 0;
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

            EcUopPackage package = _client.GetPackage(FileName);
            if (package == null)
            {
                _statusLabel.Text = $"{FileName} was not found.";
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                _hues = new EcHues(package);
                package.Changed += (s, e) => _saveButton.Enabled = package.IsDirty;
                ApplyFilter();
                _statusLabel.Text = $"{_hues.Count} hues.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Hues.uop can't be read.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearCache()
        {
            foreach (Bitmap b in _rampCache.Values)
            {
                b.Dispose();
            }
            _rampCache.Clear();
        }

        private void ApplyFilter()
        {
            if (_hues == null)
            {
                return;
            }
            string filter = _searchBox.Text.Trim();
            int jump = -1;
            List<EcHue> all = _hues.Hues.ToList();
            if (filter.Length > 0 && int.TryParse(filter, out int number))
            {
                _view = all;
                jump = _view.FindIndex(h => h.Number >= number);
            }
            else if (filter.Length > 0)
            {
                _view = all.Where(h => h.Name != null && h.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            else
            {
                _view = all;
            }
            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
            if (jump >= 0)
            {
                _list.SelectedIndices.Clear();
                _list.SelectedIndices.Add(jump);
                _list.EnsureVisible(jump);
            }
        }

        private Bitmap Ramp(EcHue hue)
        {
            if (_rampCache.TryGetValue(hue.Number, out Bitmap b))
            {
                return b;
            }
            try
            {
                byte[] px = _hues.GetRamp(hue, out int w, out int h);
                b = EcImageHelper.ToBitmap(px, w, h);
            }
            catch (Exception)
            {
                b = new Bitmap(1, 1);
            }
            _rampCache[hue.Number] = b;
            return b;
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            EcHue hue = _view[e.ItemIndex];
            var item = new ListViewItem(hue.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(string.Empty);
            item.SubItems.Add(hue.Name ?? string.Empty);
            e.Item = item;
        }

        private void DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != 1 || e.ItemIndex >= _view.Count)
            {
                e.DrawDefault = true;
                return;
            }
            e.DrawBackground();
            Bitmap ramp = Ramp(_view[e.ItemIndex]);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.DrawImage(ramp, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, e.Bounds.Width - 4, e.Bounds.Height - 4), new Rectangle(0, 0, ramp.Width, 1), GraphicsUnit.Pixel);
        }

        private EcHue Selected => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                _ramp.Image = null;
                return;
            }
            _ramp.Image = Ramp(hue);
            _nameBox.Text = hue.Name ?? string.Empty;
            _info.Text = $"Hue {hue.Number}  {hue.Entry.Name}\r\n{_ramp.Image.Width} x {_ramp.Image.Height}" + (hue.Entry.IsModified ? "  (changed)" : string.Empty);
        }

        private void SetName()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            _hues.SetName(hue, _nameBox.Text);
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        private void RefreshHue(EcHue hue)
        {
            if (_rampCache.TryGetValue(hue.Number, out Bitmap b))
            {
                b.Dispose();
                _rampCache.Remove(hue.Number);
            }
            _saveButton.Enabled = true;
            _list.Invalidate();
            ShowSelected();
        }

        private void ImportRamp()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "Images (*.bmp;*.png;*.gif;*.tga;*.dds)|*.bmp;*.png;*.gif;*.tga;*.dds|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    using (Bitmap image = EcImageHelper.LoadImageFile(dialog.FileName))
                    {
                        byte[] px = EcImageHelper.ToBgra(image, out int w, out int h);
                        _hues.SetRamp(hue, px, w, h);
                    }
                    RefreshHue(hue);
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "Import failed.", ex);
                }
            }
        }

        private void ExportRamp()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "PNG|*.png|BMP|*.bmp", FileName = $"hue{hue.Number:D4}.png" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Ramp(hue).Save(dialog.FileName, dialog.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
                }
            }
        }

        private void AddCopy()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            int next = _hues.Hues.Max(h => h.Number) + 1;
            string text = EcDialogs.Prompt(this, "Add hue", "Number of the new hue:", next.ToString(CultureInfo.InvariantCulture));
            if (text == null || !int.TryParse(text, out int number) || number <= 0 || number > 9999)
            {
                return;
            }
            if (_hues.Get(number) != null && !EcDialogs.Confirm(this, $"Hue {number} exists already. Replace it?"))
            {
                return;
            }
            byte[] px = _hues.GetRamp(hue, out int w, out int h);
            EcHue added = _hues.Add(number, px, w, h, hue.Name);
            ApplyFilter();
            int index = _view.IndexOf(added);
            if (index >= 0)
            {
                _list.SelectedIndices.Clear();
                _list.SelectedIndices.Add(index);
                _list.EnsureVisible(index);
            }
            _saveButton.Enabled = true;
        }

        private void DeleteHue()
        {
            EcHue hue = Selected;
            if (hue == null || !EcDialogs.Confirm(this, $"Delete hue {hue.Number}?"))
            {
                return;
            }
            _hues.Remove(hue);
            ApplyFilter();
            _saveButton.Enabled = true;
        }

        private void Save(bool saveAs)
        {
            if (_hues == null)
            {
                return;
            }
            _hues.Apply();
            if (EcPackageSaver.Save(this, new[] { _hues.Package }, saveAs))
            {
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCache();
                _images.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

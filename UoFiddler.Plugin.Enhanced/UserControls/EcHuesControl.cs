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
    public sealed partial class EcHuesControl : UserControl, IEcEditor
    {
        public const string FileName = "Hues.uop";

        private readonly ImageList _images = new ImageList { ImageSize = new Size(128, 14), ColorDepth = ColorDepth.Depth32Bit };

        private EcClient _client;
        private EcHues _hues;
        private bool _loaded;
        private List<EcHue> _view = new List<EcHue>();
        private readonly Dictionary<int, Bitmap> _rampCache = new Dictionary<int, Bitmap>();

        public EcHuesControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Disposed += OnControlDisposed;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _hues = null;
            ClearCache();
            _view = new List<EcHue>();
            listViewHues.VirtualListSize = 0;
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

            EcUopPackage package = _client.GetPackage(FileName);
            if (package == null)
            {
                toolStripStatusLabel.Text = $"{FileName} was not found.";
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                _hues = new EcHues(package);
                package.Changed += (s, e) => toolStripButtonSave.Enabled = package.IsDirty;
                ApplyFilter();
                toolStripStatusLabel.Text = $"{_hues.Count} hues.";
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
            string filter = toolStripTextBoxSearch.Text.Trim();
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
            listViewHues.VirtualListSize = _view.Count;
            listViewHues.Invalidate();
            if (jump >= 0)
            {
                listViewHues.SelectedIndices.Clear();
                listViewHues.SelectedIndices.Add(jump);
                listViewHues.EnsureVisible(jump);
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

        private EcHue Selected => listViewHues.SelectedIndices.Count == 1 && listViewHues.SelectedIndices[0] < _view.Count ? _view[listViewHues.SelectedIndices[0]] : null;

        private void ShowSelected()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                pictureBoxRamp.Image = null;
                return;
            }
            pictureBoxRamp.Image = Ramp(hue);
            textBoxName.Text = hue.Name ?? string.Empty;
            labelInfo.Text = $"Hue {hue.Number}  {hue.Entry.Name}\r\n{pictureBoxRamp.Image.Width} x {pictureBoxRamp.Image.Height}" + (hue.Entry.IsModified ? "  (changed)" : string.Empty);
        }

        private void SetName()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            _hues.SetName(hue, textBoxName.Text);
            toolStripButtonSave.Enabled = true;
            listViewHues.Invalidate();
        }

        private void RefreshHue(EcHue hue)
        {
            if (_rampCache.TryGetValue(hue.Number, out Bitmap b))
            {
                b.Dispose();
                _rampCache.Remove(hue.Number);
            }
            toolStripButtonSave.Enabled = true;
            listViewHues.Invalidate();
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
                listViewHues.SelectedIndices.Clear();
                listViewHues.SelectedIndices.Add(index);
                listViewHues.EnsureVisible(index);
            }
            toolStripButtonSave.Enabled = true;
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
            toolStripButtonSave.Enabled = true;
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
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = "Saved.";
            }
        }

        private void OnControlDisposed(object sender, EventArgs e)
        {
            ClearCache();
            _images.Dispose();
        }

        // ==================================================================== event handlers (designer)

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        }

        private void OnClickApply(object sender, EventArgs e) => ApplyFilter();

        private void OnClickImportRamp(object sender, EventArgs e) => ImportRamp();

        private void OnClickExportRamp(object sender, EventArgs e) => ExportRamp();

        private void OnClickAddCopy(object sender, EventArgs e) => AddCopy();

        private void OnClickDelete(object sender, EventArgs e) => DeleteHue();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) => e.DrawDefault = true;

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnClickSetName(object sender, EventArgs e) => SetName();

    }
}

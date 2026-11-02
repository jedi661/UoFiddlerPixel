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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Animations;
using UoFiddler.Plugin.Enhanced.EcUltima.Gumps;
using UoFiddler.Plugin.Enhanced.EcUltima.Hues;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Hues.uop: editor (ramps, 32 classic colors, names), compare with the classic hues.mul,
    /// converter in both directions (huesmul2uop, improved) and help.
    /// </summary>
    public sealed partial class EcHuesControl : UserControl, IEcEditor
    {
        public const string FileName = "Hues.uop";

        private const string OutputSetting = "Hues.Output";
        private const string BackupSetting = "Hues.BackupFolder";
        private const string BackupEnabledSetting = "Hues.Backup";
        private const string ClassicSetting = "Hues.Classic";

        private const int CompareColumnClassic = 1;
        private const int CompareColumnEc = 2;

        private EcClient _client;
        private EcHues _hues;
        private bool _loaded;
        private bool _busy;
        private bool _updating;
        private List<EcHue> _view = new List<EcHue>();
        private readonly Dictionary<int, Bitmap> _rampCache = new Dictionary<int, Bitmap>();
        private readonly Dictionary<int, Bitmap> _classicRampCache = new Dictionary<int, Bitmap>();
        private readonly Dictionary<int, HueDecodeResult> _colorsCache = new Dictionary<int, HueDecodeResult>();

        private ClassicHueFile _classic;
        private string _classicPath;
        private bool _classicDirty;

        private List<HueCompareRow> _compareRows = new List<HueCompareRow>();
        private List<HueCompareRow> _compareView = new List<HueCompareRow>();
        private readonly Dictionary<ColumnHeader, int> _columnWidths = new Dictionary<ColumnHeader, int>();

        private readonly EcBackup _backup = new EcBackup("Hues");
        private int _hoverColor = -1;

        public EcHuesControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Disposed += OnControlDisposed;

            // runtime only: defaults and remembered settings
            _updating = true;
            comboBoxStyle.SelectedIndex = 0;
            comboBoxScale.SelectedIndex = (int)HueColorScale.Shift;
            comboBoxPreview.SelectedIndex = 0;
            toolStripComboBoxFilter.SelectedIndex = 1;
            textBoxOutput.Text = EcSettings.Get(OutputSetting) ?? string.Empty;
            textBoxBackup.Text = EcSettings.Get(BackupSetting) ?? string.Empty;
            checkBoxBackup.Checked = EcSettings.Get(BackupEnabledSetting) != "0";
            foreach (ColumnHeader column in listViewCompare.Columns)
            {
                _columnWidths[column] = column.Width;
            }
            _updating = false;
            ApplyStyleUi();
            UpdateTargetUi();
            ShowHelp();
        }

        // ==================================================================== loading

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _hues = null;
            _sprites?.Dispose();
            _sprites = null;
            ClearCaches();
            _view = new List<EcHue>();
            listViewHues.VirtualListSize = 0;
            _compareRows = new List<HueCompareRow>();
            _compareView = new List<HueCompareRow>();
            listViewCompare.VirtualListSize = 0;
            toolStripButtonSave.Enabled = false;
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_classic == null)
            {
                LoadDefaultClassic();
            }
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
                package.Changed += (s, e) => UpdateSaveButtons();
                ApplyFilter();
                toolStripStatusLabel.Text = $"{_hues.Count} hues. Ramps: {_hues.Options}.";
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

        /// <summary>The remembered hues.mul or the one of the classic client loaded in UOFiddler.</summary>
        private void LoadDefaultClassic()
        {
            string path = EcSettings.Get(ClassicSetting);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                try
                {
                    path = Ultima.Files.GetFilePath(ClassicHueFile.FileName);
                }
                catch (Exception)
                {
                    path = null;
                }
            }
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                LoadClassic(path, false);
            }
        }

        private void LoadClassic(string path, bool showErrors)
        {
            try
            {
                _classic = ClassicHueFile.Load(path);
                _classicPath = path;
                _classicDirty = false;
                _classicRampCache.Values.ToList().ForEach(b => b.Dispose());
                _classicRampCache.Clear();
                EcSettings.Set(ClassicSetting, path);
                toolStripLabelClassic.Text = $"Classic: {path} ({_classic.Count} hues)";
                toolStripLabelClassic.ToolTipText = path;
                UpdateSaveButtons();
                if (IsHandleCreated)
                {
                    listViewHues.Invalidate();
                    ShowSelected();
                }
            }
            catch (Exception ex)
            {
                if (showErrors)
                {
                    EcDialogs.ShowError(this, "hues.mul can't be read.", ex);
                }
            }
        }

        private void UpdateSaveButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateSaveButtons));
                return;
            }
            toolStripButtonSave.Enabled = _hues != null && (_hues.Package.IsDirty || _hues.HasPendingChanges);
            toolStripButtonSaveClassic.Enabled = _classic != null && _classicDirty;
            toolStripButtonSaveClassicAs.Enabled = _classic != null;
            toolStripLabelClassic.Text = _classic == null ? "Classic: (not loaded)" : $"Classic: {_classicPath ?? "(new)"} ({_classic.Count} hues)" + (_classicDirty ? " *" : string.Empty);
        }

        private void ClearCaches()
        {
            foreach (Bitmap b in _rampCache.Values.Concat(_classicRampCache.Values))
            {
                b.Dispose();
            }
            _rampCache.Clear();
            _classicRampCache.Clear();
            _colorsCache.Clear();
        }

        private void ForgetHue(int number)
        {
            if (_rampCache.TryGetValue(number, out Bitmap b))
            {
                b.Dispose();
                _rampCache.Remove(number);
            }
            if (_classicRampCache.TryGetValue(number, out Bitmap c))
            {
                c.Dispose();
                _classicRampCache.Remove(number);
            }
            _colorsCache.Remove(number);
        }

        // ==================================================================== editor: list

        private void ApplyFilter()
        {
            if (_hues == null)
            {
                return;
            }
            string filter = toolStripTextBoxSearch.Text.Trim();
            int jump = -1;
            List<EcHue> all = _hues.Hues.ToList();
            if (filter.Length > 0 && EcDialogs.TryParseNumber(filter, out long number))
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
                SelectHue(_view[jump].Number);
            }
        }

        private void SelectHue(int number)
        {
            int index = _view.FindIndex(h => h.Number == number);
            if (index < 0)
            {
                return;
            }
            listViewHues.SelectedIndices.Clear();
            listViewHues.SelectedIndices.Add(index);
            listViewHues.EnsureVisible(index);
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

        private Bitmap ClassicRamp(ClassicHue hue)
        {
            if (_classicRampCache.TryGetValue(hue.Number, out Bitmap b))
            {
                return b;
            }
            HueRenderOptions options = _hues?.Options ?? HueRenderOptions.OriginalClient;
            b = EcImageHelper.ToBitmap(EcHueRender.RenderRampPixels(hue.Colors, options), EcHueRender.RampWidth, 2);
            _classicRampCache[hue.Number] = b;
            return b;
        }

        /// <summary>The classic colors of an EC hue (cached on the UI thread; nothing is calculated while a job runs).</summary>
        private HueDecodeResult Colors(EcHue hue)
        {
            if (_colorsCache.TryGetValue(hue.Number, out HueDecodeResult result))
            {
                return result;
            }
            if (_busy || _hues == null)
            {
                return null;
            }
            try
            {
                result = _hues.GetColors(hue);
            }
            catch (Exception)
            {
                result = null;
            }
            _colorsCache[hue.Number] = result;
            return result;
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
            HueDecodeResult colors = Colors(hue);
            item.SubItems.Add(colors == null ? "..." : HueDecodeResult.Text(colors.Quality));
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
            DrawRamp(e.Graphics, Ramp(_view[e.ItemIndex]), e.Bounds);
        }

        private static void DrawRamp(Graphics g, Bitmap ramp, Rectangle bounds)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(ramp, new Rectangle(bounds.X + 2, bounds.Y + 2, bounds.Width - 4, bounds.Height - 4), new Rectangle(0, 0, ramp.Width, 1), GraphicsUnit.Pixel);
        }

        private EcHue Selected => listViewHues.SelectedIndices.Count >= 1 && listViewHues.SelectedIndices[0] < _view.Count ? _view[listViewHues.SelectedIndices[0]] : null;

        private List<EcHue> SelectedHues()
        {
            var list = new List<EcHue>();
            foreach (int index in listViewHues.SelectedIndices)
            {
                if (index < _view.Count)
                {
                    list.Add(_view[index]);
                }
            }
            return list;
        }

        private void ShowSelected()
        {
            EcHue hue = Selected;
            panelColors.Invalidate();
            if (hue == null || _hues == null)
            {
                pictureBoxRamp.Image = null;
                labelInfo.Text = string.Empty;
                return;
            }
            pictureBoxRamp.Image = Ramp(hue);
            textBoxName.Text = hue.Name ?? string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"Hue {hue.Number}  {hue.Entry.Name}");
            sb.AppendLine($"{pictureBoxRamp.Image.Width} x {pictureBoxRamp.Image.Height}" + (hue.Entry.IsModified ? "  (changed)" : string.Empty));
            HueDecodeResult colors = Colors(hue);
            if (colors != null)
            {
                sb.AppendLine($"Classic colors: {HueDecodeResult.Text(colors.Quality)}" + (colors.MaxDiff > 0 ? $" (max difference {colors.MaxDiff})" : string.Empty));
            }
            sb.AppendLine($"Ramps: {_hues.Options}");
            sb.AppendLine("hues.dds row: " + (hue.Number <= EcHues.MaxAtlasHue ? (hue.Number % EcHues.DdsSize).ToString(CultureInfo.InvariantCulture) + $", column {hue.Number / EcHues.DdsSize}" : "none (number too high)"));
            ClassicHue classic = _classic?.Get(hue.Number);
            if (classic == null)
            {
                sb.AppendLine("Classic: " + (_classic == null ? "hues.mul not loaded" : "not in hues.mul"));
            }
            else if (colors != null)
            {
                int diff = Enumerable.Range(0, 32).Count(i => (classic.Colors[i] & 0x7FFF) != (colors.Colors[i] & 0x7FFF));
                sb.AppendLine($"Classic: \"{classic.Name}\", " + (diff == 0 ? "same colors" : $"{diff} colors differ"));
            }
            labelInfo.Text = sb.ToString();
            ShowHuePreview();
        }

        // ==================================================================== editor: preview and gradient

        private EcItemSprites _sprites;

        /// <summary>The image to hue: item (EC image), gump (EC legacy gump or classic) or the first frame of a body.</summary>
        private byte[] PreviewSource(out int width, out int height)
        {
            width = height = 0;
            if (_client == null || !EcDialogs.TryParseNumber(textBoxPreviewId.Text.Trim(), out long id) || id < 0)
            {
                return null;
            }
            switch (comboBoxPreview.SelectedIndex)
            {
                case 1:
                {
                    byte[] data = _client.GetPackage("gumpartLegacyMUL.uop")?.ReadData(string.Format(CultureInfo.InvariantCulture, "build/gumpartlegacymul/{0:D8}.tga", id));
                    if (data != null && EcLegacyGump.IsLegacyGump(data))
                    {
                        byte[] px = EcLegacyGump.Decode(data, out width, out height);
                        if (width > 0)
                        {
                            return px;
                        }
                    }
                    try
                    {
                        Bitmap cached = Ultima.Gumps.IsValidIndex((int)id) ? Ultima.Gumps.GetGump((int)id) : null;
                        if (cached != null)
                        {
                            using (var copy = new Bitmap(cached))
                            {
                                return EcImageHelper.ToBgra(copy, out width, out height);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // no classic client
                    }
                    return null;
                }
                case 2:
                {
                    foreach (string file in new[] { "AnimationFrame1.uop", "AnimationFrame2.uop", "AnimationFrame3.uop", "AnimationFrame4.uop", "AnimationFrame5.uop", "AnimationFrame6.uop" })
                    {
                        EcUopPackage package = _client.GetPackage(file);
                        for (int action = 0; package != null && action < 35; action++)
                        {
                            byte[] data = package.ReadData(string.Format(CultureInfo.InvariantCulture, EcClient.AnimationFramePattern, id, action));
                            if (data == null || !EcAnimation.IsAnimation(data))
                            {
                                continue;
                            }
                            EcAnimation animation = EcAnimation.Read(data);
                            EcAnimationFrame frame = animation.Frames.FirstOrDefault(f => !f.IsEmpty);
                            if (frame != null)
                            {
                                width = frame.Width;
                                height = frame.Height;
                                return animation.ToBgra(frame);
                            }
                        }
                    }
                    return null;
                }
                default:
                {
                    _sprites ??= new EcItemSprites(_client);
                    Bitmap image = _sprites.Get((uint)id, false).Image;
                    return image == null ? null : EcImageHelper.ToBgra(image, out width, out height);
                }
            }
        }

        private void ShowHuePreview()
        {
            EcHue hue = Selected;
            HueDecodeResult colors = hue == null ? null : Colors(hue);
            Image old = pictureBoxPreview.Image;
            pictureBoxPreview.Image = null;
            old?.Dispose();
            if (colors == null)
            {
                return;
            }
            try
            {
                byte[] px = PreviewSource(out int w, out int h);
                if (px == null || w <= 0 || h <= 0)
                {
                    return;
                }
                EcHueRender.ApplyHue(px, colors.Colors, _hues.Options.Scale, checkBoxPartial.Checked);
                pictureBoxPreview.Image = EcImageHelper.ToBitmap(px, w, h);
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = "Preview: " + ex.Message;
            }
        }

        /// <summary>New colors for the selected hue: a gradient from a dark (color 0) to a light color (color 31).</summary>
        private void Gradient()
        {
            EcHue hue = Selected;
            HueDecodeResult colors = hue == null ? null : Colors(hue);
            if (colors == null)
            {
                return;
            }
            Color start, end;
            using (var dialog = new ColorDialog { Color = ToColor(colors.Colors[0]), FullOpen = true })
            {
                MessageBox.Show(this, "First choose the dark color (color 0), then the light color (color 31).", "Gradient", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                start = dialog.Color;
                dialog.Color = ToColor(colors.Colors[31]);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                end = dialog.Color;
            }
            ushort[] gradient = EcHueRender.Gradient(start.R, start.G, start.B, end.R, end.G, end.B, _hues.Options.Scale);
            _hues.SetColors(hue, gradient);
            RefreshHue(hue);
            toolStripStatusLabel.Text = $"Hue {hue.Number}: gradient written (bitmap, hues.dds and overview).";
        }

        private void RefreshHue(EcHue hue)
        {
            ForgetHue(hue.Number);
            UpdateSaveButtons();
            listViewHues.Invalidate();
            ShowSelected();
        }

        private void SetName()
        {
            EcHue hue = Selected;
            if (hue == null)
            {
                return;
            }
            _hues.SetName(hue, textBoxName.Text);
            UpdateSaveButtons();
            listViewHues.Invalidate();
        }

        // ==================================================================== editor: the 32 colors

        private Rectangle ColorCell(int index)
        {
            Size size = panelColors.ClientSize;
            int w = Math.Max(1, size.Width / 16);
            int h = Math.Max(1, size.Height / 2);
            return new Rectangle((index % 16) * w, (index / 16) * h, w, h);
        }

        private int ColorAt(Point p)
        {
            for (int i = 0; i < 32; i++)
            {
                if (ColorCell(i).Contains(p))
                {
                    return i;
                }
            }
            return -1;
        }

        private Color ToColor(ushort c)
        {
            HueColorScale scale = _hues?.Options.Scale ?? HueColorScale.Shift;
            return Color.FromArgb(EcHueRender.To8(ClassicHue.Red(c), scale), EcHueRender.To8(ClassicHue.Green(c), scale), EcHueRender.To8(ClassicHue.Blue(c), scale));
        }

        private void PaintColors(Graphics g)
        {
            EcHue hue = Selected;
            HueDecodeResult colors = hue == null ? null : Colors(hue);
            if (colors == null)
            {
                return;
            }
            ClassicHue classic = _classic?.Get(hue.Number);
            for (int i = 0; i < 32; i++)
            {
                Rectangle r = ColorCell(i);
                using (var brush = new SolidBrush(ToColor(colors.Colors[i])))
                {
                    g.FillRectangle(brush, r);
                }
                g.DrawRectangle(i == _hoverColor ? Pens.White : Pens.Black, r.X, r.Y, r.Width - 1, r.Height - 1);
                if (classic != null && (classic.Colors[i] & 0x7FFF) != (colors.Colors[i] & 0x7FFF))
                {
                    // small mark: the classic color is different
                    using (var brush = new SolidBrush(ToColor(classic.Colors[i])))
                    {
                        g.FillRectangle(brush, r.Right - 9, r.Bottom - 9, 7, 7);
                    }
                    g.DrawRectangle(Pens.Red, r.Right - 9, r.Bottom - 9, 7, 7);
                }
            }
        }

        private void EditColor(int index)
        {
            EcHue hue = Selected;
            HueDecodeResult colors = hue == null ? null : Colors(hue);
            if (colors == null || index < 0)
            {
                return;
            }
            using (var dialog = new ColorDialog { Color = ToColor(colors.Colors[index]), FullOpen = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                HueColorScale scale = _hues.Options.Scale;
                var changed = (ushort[])colors.Colors.Clone();
                changed[index] = ClassicHue.Make(EcHueRender.To5(dialog.Color.R, scale), EcHueRender.To5(dialog.Color.G, scale), EcHueRender.To5(dialog.Color.B, scale));
                _hues.SetColors(hue, changed);
                RefreshHue(hue);
                toolStripStatusLabel.Text = $"Hue {hue.Number}: color {index} changed (bitmap, hues.dds and overview).";
            }
        }

        private void ShowColorTip(Point p)
        {
            int index = ColorAt(p);
            if (index == _hoverColor)
            {
                return;
            }
            _hoverColor = index;
            panelColors.Invalidate();
            EcHue hue = Selected;
            HueDecodeResult colors = hue == null ? null : Colors(hue);
            if (index < 0 || colors == null)
            {
                toolTip.SetToolTip(panelColors, null);
                return;
            }
            ushort c = colors.Colors[index];
            string text = $"Color {index}: R {ClassicHue.Red(c)} G {ClassicHue.Green(c)} B {ClassicHue.Blue(c)}  (0x{c:X4})";
            ClassicHue classic = _classic?.Get(hue.Number);
            if (classic != null && (classic.Colors[index] & 0x7FFF) != (c & 0x7FFF))
            {
                ushort k = classic.Colors[index];
                text += $"\r\nClassic: R {ClassicHue.Red(k)} G {ClassicHue.Green(k)} B {ClassicHue.Blue(k)}  (0x{k:X4})";
            }
            toolTip.SetToolTip(panelColors, text + "\r\nClick to change");
        }

        // ==================================================================== editor: actions

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
            int next = _hues.MaxNumber + 1;
            string text = EcDialogs.Prompt(this, "Add hue", "Number of the new hue:", next.ToString(CultureInfo.InvariantCulture));
            if (text == null || !int.TryParse(text, out int number) || number <= 0 || number > 9999)
            {
                return;
            }
            if (_hues.Get(number) != null && !EcDialogs.Confirm(this, $"Hue {number} exists already. Replace it?"))
            {
                return;
            }
            HueDecodeResult colors = Colors(hue);
            EcHue added;
            if (colors != null && colors.Quality <= HueDecodeQuality.Verified)
            {
                added = _hues.Add(number, colors.Colors, hue.Name, _hues.GetStoredClassic(hue.Number));
            }
            else
            {
                byte[] px = _hues.GetRamp(hue, out int w, out int h);
                added = _hues.Add(number, px, w, h, hue.Name);
            }
            ForgetHue(number);
            ApplyFilter();
            SelectHue(added.Number);
            UpdateSaveButtons();
        }

        private void DeleteHues()
        {
            List<EcHue> hues = SelectedHues();
            if (hues.Count == 0 || !EcDialogs.Confirm(this, hues.Count == 1 ? $"Delete hue {hues[0].Number}?" : $"Delete {hues.Count} hues?"))
            {
                return;
            }
            foreach (EcHue hue in hues)
            {
                _hues.Remove(hue);
                ForgetHue(hue.Number);
            }
            ApplyFilter();
            UpdateSaveButtons();
        }

        /// <summary>Draws the selected EC hues from the classic colors.</summary>
        private void ColorsFromClassic()
        {
            if (!NeedClassic())
            {
                return;
            }
            int count = 0;
            foreach (EcHue hue in SelectedHues())
            {
                ClassicHue classic = _classic.Get(hue.Number);
                if (classic == null)
                {
                    continue;
                }
                _hues.SetColors(hue, classic.Colors, classic);
                ForgetHue(hue.Number);
                count++;
            }
            listViewHues.Invalidate();
            ShowSelected();
            UpdateSaveButtons();
            toolStripStatusLabel.Text = $"{count} hues drawn from hues.mul.";
        }

        /// <summary>Writes the colors of the selected EC hues into the classic file.</summary>
        private void ColorsToClassic()
        {
            if (!NeedClassic())
            {
                return;
            }
            var only = new HashSet<int>(SelectedHues().Select(h => h.Number));
            if (only.Count == 0)
            {
                return;
            }
            TransferToClassic(only, false);
        }

        private bool NeedClassic()
        {
            if (_classic != null)
            {
                return true;
            }
            OpenClassic();
            return _classic != null;
        }

        // ==================================================================== compare

        private async void RunCompare(int select = 0)
        {
            if (_hues == null || !NeedClassic())
            {
                return;
            }
            ClassicHueFile classic = _classic;
            await Run("Compare", progress =>
            {
                List<HueCompareRow> rows = EcHuesConverter.Compare(classic, _hues, progress);
                var report = new HueReport();
                report.Line($"{rows.Count} hues: " + string.Join(", ", rows.GroupBy(r => r.Status).OrderBy(g => g.Key).Select(g => $"{g.Count()} {StatusName(g.Key)}")));
                BeginInvoke(new Action(() =>
                {
                    _compareRows = rows;
                    _colorsCache.Clear();
                    foreach (HueCompareRow row in rows.Where(r => r.Ec != null && r.Decoded != null))
                    {
                        _colorsCache[row.Number] = row.Decoded;
                    }
                    ApplyCompareFilter();
                    if (select > 0)
                    {
                        SelectCompareRow(select);
                    }
                }));
                return report;
            }, false);
        }

        private static string StatusName(HueCompareStatus status) => status switch
        {
            HueCompareStatus.Same => "same",
            HueCompareStatus.ColorsDiffer => "colors differ",
            HueCompareStatus.NameDiffers => "only the name differs",
            HueCompareStatus.OnlyClassic => "only in classic",
            _ => "only in EC"
        };

        private void ApplyCompareFilter()
        {
            int filter = toolStripComboBoxFilter.SelectedIndex;
            _compareView = _compareRows.Where(r => filter switch
            {
                1 => r.Status != HueCompareStatus.Same,
                2 => r.Status == HueCompareStatus.ColorsDiffer,
                3 => r.NameDiffers || r.Status == HueCompareStatus.NameDiffers,
                4 => r.Status == HueCompareStatus.OnlyClassic,
                5 => r.Status == HueCompareStatus.OnlyEc,
                6 => r.Decoded != null && r.Decoded.Quality > HueDecodeQuality.Verified,
                _ => true
            }).ToList();
            listViewCompare.VirtualListSize = _compareView.Count;
            listViewCompare.Invalidate();
            labelCompareInfo.Text = _compareRows.Count == 0 ? "Open the classic hues.mul and press Compare."
                : $"{_compareView.Count} of {_compareRows.Count} hues shown. " + string.Join(", ", _compareRows.GroupBy(r => r.Status).OrderBy(g => g.Key).Select(g => $"{g.Count()} {StatusName(g.Key)}"));
        }

        private void SelectCompareRow(int number)
        {
            int index = _compareView.FindIndex(r => r.Number == number);
            if (index < 0 && toolStripComboBoxFilter.SelectedIndex != 0)
            {
                toolStripComboBoxFilter.SelectedIndex = 0;
                index = _compareView.FindIndex(r => r.Number == number);
            }
            if (index >= 0)
            {
                listViewCompare.SelectedIndices.Clear();
                listViewCompare.SelectedIndices.Add(index);
                listViewCompare.EnsureVisible(index);
            }
        }

        private void RetrieveCompareItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _compareView.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            HueCompareRow row = _compareView[e.ItemIndex];
            var item = new ListViewItem(row.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(string.Empty);
            item.SubItems.Add(string.Empty);
            item.SubItems.Add(row.StatusText);
            item.SubItems.Add(row.Decoded == null ? string.Empty : HueDecodeResult.Text(row.Decoded.Quality));
            item.SubItems.Add(row.Classic?.Name ?? string.Empty);
            item.SubItems.Add(row.Ec?.Name ?? string.Empty);
            if (row.Status != HueCompareStatus.Same)
            {
                item.ForeColor = row.Status == HueCompareStatus.ColorsDiffer ? Color.DarkRed : Color.DarkBlue;
            }
            e.Item = item;
        }

        private void DrawCompareSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if ((e.ColumnIndex != CompareColumnClassic && e.ColumnIndex != CompareColumnEc) || e.ItemIndex >= _compareView.Count)
            {
                e.DrawDefault = true;
                return;
            }
            e.DrawBackground();
            if (e.Bounds.Width < 8)
            {
                return;
            }
            HueCompareRow row = _compareView[e.ItemIndex];
            if (e.ColumnIndex == CompareColumnClassic && row.Classic != null)
            {
                DrawRamp(e.Graphics, ClassicRamp(row.Classic), e.Bounds);
            }
            else if (e.ColumnIndex == CompareColumnEc && row.Ec != null)
            {
                DrawRamp(e.Graphics, Ramp(row.Ec), e.Bounds);
            }
        }

        private HueCompareRow SelectedCompareRow => listViewCompare.SelectedIndices.Count >= 1 && listViewCompare.SelectedIndices[0] < _compareView.Count ? _compareView[listViewCompare.SelectedIndices[0]] : null;

        private HashSet<int> MarkedCompareNumbers()
        {
            var set = new HashSet<int>();
            foreach (int index in listViewCompare.SelectedIndices)
            {
                if (index < _compareView.Count)
                {
                    set.Add(_compareView[index].Number);
                }
            }
            return set;
        }

        private void ShowCompareDetails()
        {
            HueCompareRow row = SelectedCompareRow;
            if (row == null)
            {
                ApplyCompareFilter();
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Hue {row.Number}: {row.StatusText}" + (listViewCompare.SelectedIndices.Count > 1 ? $"   ({listViewCompare.SelectedIndices.Count} marked)" : string.Empty));
            if (row.Classic != null && row.Decoded != null)
            {
                var diffs = Enumerable.Range(0, 32).Where(i => (row.Classic.Colors[i] & 0x7FFF) != (row.Decoded.Colors[i] & 0x7FFF)).ToList();
                if (diffs.Count > 0)
                {
                    sb.AppendLine("Color  classic  EC");
                    sb.AppendLine(string.Join("   ", diffs.Take(8).Select(i => $"{i,2}: {row.Classic.Colors[i]:X4} {row.Decoded.Colors[i]:X4}")) + (diffs.Count > 8 ? " ..." : string.Empty));
                }
            }
            if (row.NameDiffers)
            {
                sb.AppendLine($"Name: classic \"{row.Classic?.Name}\"  EC \"{row.Ec?.Name}\"");
            }
            labelCompareInfo.Text = sb.ToString();
        }

        private void ShowColumns()
        {
            bool classic = toolStripButtonShowClassic.Checked;
            bool ec = toolStripButtonShowEc.Checked;
            SetColumn(columnHeaderCompareClassic, classic);
            SetColumn(columnHeaderCompareClassicName, classic);
            SetColumn(columnHeaderCompareEc, ec);
            SetColumn(columnHeaderCompareEcName, ec);
            SetColumn(columnHeaderCompareQuality, ec);
        }

        private void SetColumn(ColumnHeader column, bool visible) => column.Width = visible ? _columnWidths[column] : 0;

        private void TransferToEc(HashSet<int> numbers)
        {
            if (_hues == null || !NeedClassic() || numbers.Count == 0)
            {
                return;
            }
            var options = new ClassicToEcOptions { Only = numbers, Names = true, OnlyChanged = false, StoreExact = checkBoxToEcExact.Checked };
            HueReport report = EcHuesConverter.ClassicToEc(_classic, _hues, options);
            foreach (int n in numbers)
            {
                ForgetHue(n);
            }
            Log("=== Classic → EC (marked)\r\n" + report);
            ApplyFilter();
            UpdateSaveButtons();
            toolStripStatusLabel.Text = $"{report.Written} hues written into Hues.uop – \"Save Hues.uop\" to keep them.";
            if (_compareRows.Count > 0)
            {
                RunCompare(numbers.Min());
            }
        }

        private void TransferToClassic(HashSet<int> numbers, bool compareAfter)
        {
            if (_hues == null || !NeedClassic() || numbers.Count == 0)
            {
                return;
            }
            var options = new EcToClassicOptions { Only = numbers, Names = true, KeepTable = true };
            _classic = EcHuesConverter.EcToClassic(_hues, _classic, options, out HueReport report);
            _classicDirty = true;
            foreach (int n in numbers)
            {
                ForgetHue(n);
            }
            Log("=== EC → Classic (marked)\r\n" + report);
            UpdateSaveButtons();
            listViewHues.Invalidate();
            ShowSelected();
            toolStripStatusLabel.Text = $"{report.Written} hues written into hues.mul – \"Save hues.mul\" to keep them.";
            if (compareAfter || _compareRows.Count > 0)
            {
                RunCompare(numbers.Min());
            }
        }

        // ==================================================================== converter

        private HueRenderOptions StyleOptions()
        {
            HueRampStyle style = comboBoxStyle.SelectedIndex == 0 || comboBoxStyle.SelectedIndex == 2 ? HueRampStyle.Linear : HueRampStyle.Blurred;
            return new HueRenderOptions
            {
                BmpStyle = style,
                DdsStyle = style,
                BmpBlur = (int)numericUpDownBmpBlur.Value,
                DdsBlur = (int)numericUpDownDdsBlur.Value,
                Scale = (HueColorScale)Math.Max(0, comboBoxScale.SelectedIndex)
            };
        }

        private void ApplyStyleUi()
        {
            if (_updating)
            {
                return;
            }
            _updating = true;
            switch (comboBoxStyle.SelectedIndex)
            {
                case 0:
                    comboBoxScale.SelectedIndex = (int)HueColorScale.Shift;
                    break;
                case 1:
                    numericUpDownBmpBlur.Value = 4;
                    numericUpDownDdsBlur.Value = 6;
                    comboBoxScale.SelectedIndex = (int)HueColorScale.Exact;
                    break;
            }
            bool custom = comboBoxStyle.SelectedIndex >= 2;
            bool blurred = comboBoxStyle.SelectedIndex == 1 || comboBoxStyle.SelectedIndex == 3;
            numericUpDownBmpBlur.Enabled = custom && blurred;
            numericUpDownDdsBlur.Enabled = custom && blurred;
            comboBoxScale.Enabled = custom;
            _updating = false;
        }

        private void UpdateTargetUi()
        {
            bool isNew = radioButtonToEcNew.Checked;
            flowLayoutPanelStyle.Enabled = isNew;
            checkBoxToEcOnlyChanged.Enabled = !isNew;
            toolTip.SetToolTip(flowLayoutPanelStyle, isNew ? null : "The loaded Hues.uop keeps its own style: " + (_hues?.Options.ToString() ?? "-"));
        }

        private T Selection<T>(T options, CheckBox range, NumericUpDown from, NumericUpDown to) where T : HueSelection
        {
            if (range.Checked)
            {
                options.First = (int)Math.Min(from.Value, to.Value);
                options.Last = (int)Math.Max(from.Value, to.Value);
            }
            return options;
        }

        private string OutputFolder()
        {
            string folder = textBoxOutput.Text.Trim();
            if (string.IsNullOrEmpty(folder))
            {
                using (var dialog = new FolderBrowserDialog { Description = "Output folder" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return null;
                    }
                    folder = dialog.SelectedPath;
                    textBoxOutput.Text = folder;
                }
            }
            Directory.CreateDirectory(folder);
            EcSettings.Set(OutputSetting, folder);
            return folder;
        }

        private void PrepareBackup()
        {
            _backup.Enabled = checkBoxBackup.Checked;
            _backup.Folder = textBoxBackup.Text.Trim();
        }

        /// <summary>Writes a file with backup of the old one and copy of the new one.</summary>
        private void WriteWithBackup(string path, Action<string> write, HueReport report)
        {
            string old = _backup.BackupExisting(path);
            if (old != null)
            {
                report.Line($"Backup: {old}");
            }
            write(path);
            string copy = _backup.CopyNew(path);
            report.Line($"Written: {path}" + (copy != null ? $"  (copy: {copy})" : string.Empty));
        }

        private async void ConvertToEc()
        {
            if (!NeedClassic())
            {
                return;
            }
            ClassicToEcOptions options = Selection(new ClassicToEcOptions
            {
                Names = checkBoxToEcNames.Checked,
                OnlyChanged = checkBoxToEcOnlyChanged.Checked,
                SkipEmpty = checkBoxToEcSkipEmpty.Checked,
                StoreExact = checkBoxToEcExact.Checked
            }, checkBoxToEcRange, numericUpDownToEcFrom, numericUpDownToEcTo);
            ClassicHueFile classic = _classic;
            PrepareBackup();

            if (radioButtonToEcLoaded.Checked)
            {
                if (_hues == null)
                {
                    EcDialogs.ShowError(this, "No Hues.uop is loaded. Choose \"Create a new Hues.uop\".");
                    return;
                }
                await Run("Classic → EC (loaded Hues.uop)", progress => EcHuesConverter.ClassicToEc(classic, _hues, options, progress), true);
                ClearCaches();
                ApplyFilter();
                UpdateSaveButtons();
                toolStripStatusLabel.Text = "Hues.uop changed – \"Save Hues.uop\" writes it.";
                return;
            }

            string folder = OutputFolder();
            if (folder == null)
            {
                return;
            }
            string path = Path.Combine(folder, FileName);
            if (_client != null && string.Equals(Path.GetFullPath(path), Path.GetFullPath(Path.Combine(_client.Folder, FileName)), StringComparison.OrdinalIgnoreCase))
            {
                EcDialogs.ShowError(this, "The output folder is the client folder. Use \"Update the loaded Hues.uop\" for that, or choose another folder.");
                return;
            }
            HueRenderOptions style = StyleOptions();
            options.OnlyChanged = false;
            await Run("Classic → EC (new Hues.uop)", progress =>
            {
                EcHues hues = EcHuesConverter.CreatePackage(style);
                HueReport report = EcHuesConverter.ClassicToEc(classic, hues, options, progress);
                WriteWithBackup(path, p => hues.Package.Save(p, false), report);
                hues.Package.Dispose();
                return report;
            }, false);
        }

        private async void ConvertToClassic()
        {
            if (_hues == null)
            {
                EcDialogs.ShowError(this, "No Hues.uop is loaded.");
                return;
            }
            EcToClassicOptions options = Selection(new EcToClassicOptions
            {
                Names = checkBoxToClassicNames.Checked,
                KeepTable = checkBoxToClassicKeepTable.Checked
            }, checkBoxToClassicRange, numericUpDownToClassicFrom, numericUpDownToClassicTo);
            PrepareBackup();

            if (radioButtonToClassicLoaded.Checked)
            {
                if (!NeedClassic())
                {
                    return;
                }
                ClassicHueFile baseFile = _classic;
                ClassicHueFile result = null;
                await Run("EC → Classic (loaded hues.mul)", progress =>
                {
                    result = EcHuesConverter.EcToClassic(_hues, baseFile, options, out HueReport report, progress);
                    return report;
                }, true);
                if (result != null)
                {
                    _classic = result;
                    _classicDirty = true;
                    ClearCaches();
                    listViewHues.Invalidate();
                    UpdateSaveButtons();
                    toolStripStatusLabel.Text = "hues.mul changed – \"Save hues.mul\" writes it.";
                }
                return;
            }

            string folder = OutputFolder();
            if (folder == null)
            {
                return;
            }
            string path = Path.Combine(folder, ClassicHueFile.FileName);
            await Run("EC → Classic (new hues.mul)", progress =>
            {
                ClassicHueFile result = EcHuesConverter.EcToClassic(_hues, null, options, out HueReport report, progress);
                WriteWithBackup(path, p => File.WriteAllBytes(p, result.ToBytes()), report);
                return report;
            }, true);
        }

        private async void Analyze()
        {
            if (_hues == null)
            {
                return;
            }
            await Run("Analyze Hues.uop", progress => EcHuesConverter.Analyze(_hues, progress), true);
            ClearCaches();
            listViewHues.Invalidate();
        }

        private async void RebuildAtlas()
        {
            if (_hues == null || !EcDialogs.Confirm(this, "Draw hues.dds and the overview again from all hue bitmaps?\r\n(\"Save Hues.uop\" writes it afterwards.)"))
            {
                return;
            }
            await Run("Rebuild hues.dds + overview", progress =>
            {
                _hues.RebuildAtlases();
                progress.Report(70);
                _hues.Apply();
                var report = new HueReport();
                report.Line($"hues.dds and the overview were drawn again from {_hues.Count} bitmaps ({_hues.Options}).");
                return report;
            }, true);
            UpdateSaveButtons();
            toolStripStatusLabel.Text = "hues.dds rebuilt – \"Save Hues.uop\" writes it.";
        }

        // ==================================================================== saving

        private void Save(bool saveAs)
        {
            if (_hues == null)
            {
                return;
            }
            _hues.Apply();
            PrepareBackup();
            string path = _hues.Package.FilePath;
            string old = saveAs ? null : _backup.BackupExisting(path);
            if (EcPackageSaver.Save(this, new[] { _hues.Package }, saveAs))
            {
                string copy = saveAs ? null : _backup.CopyNew(path);
                toolStripStatusLabel.Text = "Saved." + (old != null ? $" Backup: {Path.GetDirectoryName(Path.GetDirectoryName(old))}" : string.Empty);
                if (old != null || copy != null)
                {
                    Log($"Hues.uop saved. Backup old: {old}  new: {copy}");
                }
            }
            UpdateSaveButtons();
        }

        private void OpenClassic()
        {
            using (var dialog = new OpenFileDialog { Filter = "hues.mul|hues*.mul|All files (*.*)|*.*", FileName = ClassicHueFile.FileName })
            {
                if (!string.IsNullOrEmpty(_classicPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(_classicPath);
                }
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (_classicDirty && !EcDialogs.Confirm(this, "The loaded hues.mul has unsaved changes. Discard them?"))
                    {
                        return;
                    }
                    LoadClassic(dialog.FileName, true);
                    _compareRows = new List<HueCompareRow>();
                    ApplyCompareFilter();
                }
            }
        }

        private void SaveClassic(bool saveAs)
        {
            if (_classic == null)
            {
                return;
            }
            string path = _classicPath;
            if (saveAs || string.IsNullOrEmpty(path))
            {
                using (var dialog = new SaveFileDialog { Filter = "hues.mul|*.mul", FileName = ClassicHueFile.FileName })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    path = dialog.FileName;
                }
            }
            else if (!EcDialogs.Confirm(this, $"Save {path}?" + (checkBoxBackup.Checked ? "\r\nThe old file is copied into the backup folder." : string.Empty)))
            {
                return;
            }
            try
            {
                PrepareBackup();
                var report = new HueReport();
                WriteWithBackup(path, p => File.WriteAllBytes(p, _classic.ToBytes()), report);
                Log("=== Save hues.mul\r\n" + report);
                _classicPath = path;
                _classicDirty = false;
                EcSettings.Set(ClassicSetting, path);
                ReloadFiddlerHues(path);
                UpdateSaveButtons();
                toolStripStatusLabel.Text = $"Saved {path}.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "hues.mul can't be written.", ex);
            }
        }

        /// <summary>When the file is the hues.mul of the classic client loaded in UOFiddler, UOFiddler reads it again.</summary>
        private void ReloadFiddlerHues(string path)
        {
            try
            {
                string fiddler = Ultima.Files.GetFilePath(ClassicHueFile.FileName);
                if (fiddler != null && string.Equals(Path.GetFullPath(fiddler), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
                {
                    Ultima.Hues.Initialize();
                    Log("The hues of UOFiddler (classic client) were read again.");
                }
            }
            catch (Exception)
            {
                // UOFiddler has no classic client loaded
            }
        }

        // ==================================================================== jobs / log

        private void Log(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(Log), text);
                return;
            }
            textBoxLog.AppendText(text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine).TrimEnd() + Environment.NewLine);
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            tabControl.Enabled = !busy;
            toolStrip.Enabled = !busy;
            if (!busy)
            {
                toolStripProgressBar.Value = 0;
            }
        }

        /// <summary>Runs a job in the background with log, progress and error message.</summary>
        private async Task Run(string title, Func<IProgress<int>, HueReport> job, bool showLog)
        {
            var progress = new Progress<int>(p => toolStripProgressBar.Value = Math.Max(0, Math.Min(100, p)));
            SetBusy(true);
            toolStripStatusLabel.Text = title + "...";
            Log($"=== {title} – {DateTime.Now:HH:mm:ss}");
            try
            {
                HueReport report = await Task.Run(() => job(progress));
                Log(report.ToString());
                toolStripStatusLabel.Text = $"{title}: done" + (report.Problems > 0 ? $", {report.Problems} problems (see Converter log)" : string.Empty);
                if (showLog)
                {
                    tabControl.SelectedTab = tabPageConvert;
                }
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
                toolStripStatusLabel.Text = $"{title}: error";
                EcDialogs.ShowError(this, $"{title} failed.", ex);
            }
            finally
            {
                SetBusy(false);
                UpdateSaveButtons();
            }
        }

        private void ShowHelp()
        {
            richTextBoxHelp.Text = radioButtonHelpGerman.Checked ? HuesHelp.German : HuesHelp.English;
        }

        private void OnControlDisposed(object sender, EventArgs e)
        {
            _sprites?.Dispose();
            ClearCaches();
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

        private void OnClickDelete(object sender, EventArgs e) => DeleteHues();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnClickOpenClassic(object sender, EventArgs e) => OpenClassic();

        private void OnClickSaveClassic(object sender, EventArgs e) => SaveClassic(false);

        private void OnClickSaveClassicAs(object sender, EventArgs e) => SaveClassic(true);

        private void OnTabChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPageConvert)
            {
                UpdateTargetUi();
            }
        }

        private void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e) => e.DrawDefault = true;

        private void OnListSelectedIndexChanged(object sender, EventArgs e) => ShowSelected();

        private void OnListSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e) => ShowSelected();

        private void OnClickSetName(object sender, EventArgs e) => SetName();

        private void OnClickFromClassic(object sender, EventArgs e) => ColorsFromClassic();

        private void OnClickToClassic(object sender, EventArgs e) => ColorsToClassic();

        private void OnClickShowInCompare(object sender, EventArgs e)
        {
            EcHue hue = Selected;
            tabControl.SelectedTab = tabPageCompare;
            if (hue == null)
            {
                return;
            }
            if (_compareRows.Count == 0)
            {
                RunCompare(hue.Number);
            }
            else
            {
                SelectCompareRow(hue.Number);
            }
        }

        private void OnPaintColors(object sender, PaintEventArgs e) => PaintColors(e.Graphics);

        private void OnClickColors(object sender, MouseEventArgs e) => EditColor(ColorAt(e.Location));

        private void OnMoveColors(object sender, MouseEventArgs e) => ShowColorTip(e.Location);

        private void OnResizeColors(object sender, EventArgs e) => panelColors.Invalidate();

        private void OnClickCompare(object sender, EventArgs e) => RunCompare();

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                ApplyCompareFilter();
            }
        }

        private void OnShowColumnsChanged(object sender, EventArgs e) => ShowColumns();

        private void OnClickMarkAll(object sender, EventArgs e)
        {
            if (_compareView.Count > 0)
            {
                listViewCompare.Focus();
                listViewCompare.VirtualListSize = _compareView.Count;
                for (int i = 0; i < _compareView.Count; i++)
                {
                    listViewCompare.SelectedIndices.Add(i);
                }
            }
        }

        private void OnClickToEcMarked(object sender, EventArgs e) => TransferToEc(MarkedCompareNumbers());

        private void OnClickToClassicMarked(object sender, EventArgs e) => TransferToClassic(MarkedCompareNumbers(), true);

        private void OnCompareSelectedIndexChanged(object sender, EventArgs e) => ShowCompareDetails();

        private void OnClickShowInEditor(object sender, EventArgs e)
        {
            HueCompareRow row = SelectedCompareRow;
            if (row?.Ec == null)
            {
                return;
            }
            toolStripTextBoxSearch.Text = string.Empty;
            ApplyFilter();
            tabControl.SelectedTab = tabPageEditor;
            SelectHue(row.Number);
        }

        private void OnTargetChanged(object sender, EventArgs e) => UpdateTargetUi();

        private void OnStyleChanged(object sender, EventArgs e) => ApplyStyleUi();

        private void OnClickConvertToEc(object sender, EventArgs e) => ConvertToEc();

        private void OnClickConvertToClassic(object sender, EventArgs e) => ConvertToClassic();

        private void OnFolderLeave(object sender, EventArgs e)
        {
            EcSettings.Set(OutputSetting, textBoxOutput.Text.Trim());
            EcSettings.Set(BackupSetting, textBoxBackup.Text.Trim());
        }

        private void OnClickBrowseOutput(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Output folder", SelectedPath = textBoxOutput.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxOutput.Text = dialog.SelectedPath;
                    EcSettings.Set(OutputSetting, dialog.SelectedPath);
                }
            }
        }

        private void OnBackupChanged(object sender, EventArgs e)
        {
            textBoxBackup.Enabled = checkBoxBackup.Checked;
            buttonBackup.Enabled = checkBoxBackup.Checked;
            if (!_updating)
            {
                EcSettings.Set(BackupEnabledSetting, checkBoxBackup.Checked ? "1" : "0");
            }
        }

        private void OnClickBrowseBackup(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Backup folder", SelectedPath = textBoxBackup.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxBackup.Text = dialog.SelectedPath;
                    EcSettings.Set(BackupSetting, dialog.SelectedPath);
                }
            }
        }

        private void OnClickOpenBackup(object sender, EventArgs e)
        {
            PrepareBackup();
            string reference = _hues?.Package.FilePath ?? _classicPath ?? Path.Combine(textBoxOutput.Text.Trim(), FileName);
            string folder = _backup.FolderFor(reference);
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(this, $"No backup yet:\r\n{folder}", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
        }

        private void OnClickAnalyze(object sender, EventArgs e) => Analyze();

        private void OnClickRebuildAtlas(object sender, EventArgs e) => RebuildAtlas();

        private void OnHelpLanguageChanged(object sender, EventArgs e) => ShowHelp();

        private void OnClickGradient(object sender, EventArgs e) => Gradient();

        private void OnPreviewChanged(object sender, EventArgs e) => ShowHuePreview();

        private void OnPreviewIdKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ShowHuePreview();
            }
        }
    }
}

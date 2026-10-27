/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer.Build
{
    /// <summary>
    /// Takes the statics of a map area as building: preview, choose the items, then save as file
    /// (all formats), into the library, into multi.mul or as ServUO script / decoration.
    /// </summary>
    public partial class AreaExportForm : Form
    {
        private readonly MapDefinition _definition;
        private readonly Map _map;
        private readonly Rectangle _area;
        private readonly List<MapStatic> _all;
        private readonly ArtPixelCache _art = new ArtPixelCache();
        private readonly int _groundMin;
        private readonly MultiExportFormat[] _formats = (MultiExportFormat[])Enum.GetValues(typeof(MultiExportFormat));
        private bool _filling;

        /// <summary>Only for the designer</summary>
        public AreaExportForm()
        {
            InitializeComponent();
        }

        /// <param name="session">edit session of a region viewer (includes changes in memory) or null</param>
        public AreaExportForm(MapDefinition definition, Rectangle area, RegionEditSession session)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _definition = definition;
            _map = session?.Map ?? definition.GetMap();
            area.Intersect(new Rectangle(0, 0, _map.Width, _map.Height));
            _area = area;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // a session only reads here (statics of the file + changes in memory)
                RegionEditSession reader = session ?? new RegionEditSession(_map);
                _all = reader.GetStaticsInArea(area.Left, area.Top, area.Right - 1, area.Bottom - 1);

                _groundMin = int.MaxValue;
                for (int x = area.Left; x < area.Right; x++)
                {
                    for (int y = area.Top; y < area.Bottom; y++)
                    {
                        _groundMin = Math.Min(_groundMin, _map.Tiles.GetLandTile(x, y).Z);
                    }
                }

                if (_groundMin == int.MaxValue)
                {
                    _groundMin = 0;
                }

                _map.Tiles.CloseStreams();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            Text = $"Export area - {definition.Name}  {area.Left},{area.Top} - {area.Right - 1},{area.Bottom - 1}";
            labelArea.Text = $"{definition.Name}: {area.Left},{area.Top} - {area.Right - 1},{area.Bottom - 1}  ({area.Width} x {area.Height}), " +
                             $"{_all.Count} statics, lowest ground z {_groundMin}";
            textBoxName.Text = $"{definition.Name} {area.Left},{area.Top}";
            textBoxCategory.Text = "From map";
            textBoxAuthor.Text = Environment.UserName;

            foreach (MultiExportFormat f in _formats)
            {
                comboBoxFormat.Items.Add(MultiFormats.FormatName(f));
            }

            _filling = true;
            comboBoxFormat.SelectedIndex = 0;
            comboBoxOrigin.SelectedIndex = 0;
            comboBoxBaseZ.SelectedIndex = 0;
            numericBaseZ.Enabled = false;
            _filling = false;

            FillItems();
            UpdatePreview();
        }

        #region Items

        private void FillItems()
        {
            _filling = true;
            listViewItems.BeginUpdate();
            try
            {
                listViewItems.Items.Clear();
                foreach (IGrouping<ushort, MapStatic> group in _all.GroupBy(s => s.Id).OrderByDescending(g => g.Count()))
                {
                    ushort id = group.Key;
                    ItemData data = id < TileData.ItemTable.Length ? TileData.ItemTable[id] : default;
                    var item = new ListViewItem($"0x{id:X4}")
                    {
                        Tag = id,
                        Checked = true
                    };
                    item.SubItems.Add(data.Name ?? string.Empty);
                    item.SubItems.Add(group.Count().ToString());
                    item.SubItems.Add($"{group.Min(s => s.Z)}..{group.Max(s => s.Z)}");
                    item.SubItems.Add(FlagText(data.Flags));
                    if (BuildPlacement.IsPlant(id))
                    {
                        item.ForeColor = Color.DarkGreen;
                    }

                    listViewItems.Items.Add(item);
                }
            }
            finally
            {
                listViewItems.EndUpdate();
                _filling = false;
            }
        }

        private static string FlagText(TileFlag flags)
        {
            TileFlag[] shown = { TileFlag.Wall, TileFlag.Roof, TileFlag.Surface, TileFlag.Door, TileFlag.Window, TileFlag.Foliage, TileFlag.Bridge, TileFlag.LightSource };
            return string.Join(", ", shown.Where(f => (flags & f) != 0));
        }

        private void SetChecked(Func<ushort, bool> check)
        {
            _filling = true;
            listViewItems.BeginUpdate();
            foreach (ListViewItem item in listViewItems.Items)
            {
                item.Checked = check((ushort)item.Tag);
            }

            listViewItems.EndUpdate();
            _filling = false;
            RequestPreview();
        }

        private void OnClickAll(object sender, EventArgs e) => SetChecked(_ => true);

        private void OnClickNone(object sender, EventArgs e) => SetChecked(_ => false);

        private void OnClickNoPlants(object sender, EventArgs e) => SetChecked(id => !BuildPlacement.IsPlant(id) && IsCheckedId(id));

        private void OnClickOnlyPlants(object sender, EventArgs e) => SetChecked(BuildPlacement.IsPlant);

        private bool IsCheckedId(ushort id)
        {
            return listViewItems.Items.Cast<ListViewItem>().Any(i => (ushort)i.Tag == id && i.Checked);
        }

        private void OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (!_filling)
            {
                RequestPreview();
            }
        }

        #endregion

        #region Building

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_filling)
            {
                return;
            }

            numericBaseZ.Enabled = comboBoxBaseZ.SelectedIndex == 3;
            RequestPreview();
        }

        private void OnFormatChanged(object sender, EventArgs e)
        {
        }

        /// <summary>Statics with the current filter (absolute map positions)</summary>
        private List<MapStatic> SelectedStatics()
        {
            var ids = new HashSet<ushort>(listViewItems.Items.Cast<ListViewItem>().Where(i => i.Checked).Select(i => (ushort)i.Tag));
            int minZ = (int)numericMinZ.Value, maxZ = (int)numericMaxZ.Value;

            IEnumerable<MapStatic> query = _all.Where(s => ids.Contains(s.Id) && s.Z >= minZ && s.Z <= maxZ);
            if (checkBoxNoDraw.Checked)
            {
                query = query.Where(s => !IsoRenderer.IsNoDrawStatic(s.Id) && Art.IsValidStatic(s.Id));
            }

            List<MapStatic> list = query.ToList();
            if (checkBoxDuplicates.Checked)
            {
                list = list.GroupBy(s => (s.X, s.Y, s.Z, s.Id, s.Hue)).Select(g => g.First()).ToList();
            }

            return list;
        }

        /// <summary>The selected statics as building (relative coordinates)</summary>
        private MultiTemplate BuildTemplate()
        {
            List<MapStatic> statics = SelectedStatics();
            var t = new MultiTemplate
            {
                Name = string.IsNullOrWhiteSpace(textBoxName.Text) ? "Building" : textBoxName.Text.Trim(),
                Category = textBoxCategory.Text.Trim(),
                Author = textBoxAuthor.Text.Trim(),
                Source = $"{_definition.Name} {_area.Left},{_area.Top}"
            };

            if (statics.Count == 0)
            {
                return t;
            }

            int originX = _area.Left, originY = _area.Top;
            if (comboBoxOrigin.SelectedIndex == 1)
            {
                originX = (statics.Min(s => s.X) + statics.Max(s => s.X)) / 2;
                originY = (statics.Min(s => s.Y) + statics.Max(s => s.Y)) / 2;
            }

            int baseZ = comboBoxBaseZ.SelectedIndex switch
            {
                1 => statics.Min(s => s.Z),
                2 => 0,
                3 => (int)numericBaseZ.Value,
                _ => _groundMin
            };

            foreach (MapStatic s in statics)
            {
                t.Components.Add(new MultiComponent(s.Id, s.X - originX, s.Y - originY, s.Z - baseZ, (ushort)s.Hue, 1));
            }

            return t;
        }

        #endregion

        #region Preview

        private void RequestPreview()
        {
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void OnPreviewTimer(object sender, EventArgs e)
        {
            previewTimer.Stop();
            UpdatePreview();
        }

        private void OnPreviewResize(object sender, EventArgs e)
        {
            RequestPreview();
        }

        private void UpdatePreview()
        {
            if (_all == null)
            {
                return;
            }

            MultiTemplate t = BuildTemplate();
            Image old = pictureBoxPreview.Image;
            pictureBoxPreview.Image = pictureBoxPreview.Width > 8 && pictureBoxPreview.Height > 8
                ? MultiPreview.Thumbnail(t, _art, pictureBoxPreview.ClientSize, pictureBoxPreview.BackColor)
                : null;
            old?.Dispose();

            labelSummary.Text = t.Components.Count == 0
                ? "Nothing selected."
                : $"{t.Components.Count} parts, {t.Width} x {t.Height}, z {t.MinZ}..{t.MaxZ}" +
                  (t.HasHues ? ", with hues" : string.Empty) +
                  (t.MaxZ - t.MinZ > 255 ? "  (higher than 255 - can not be placed completely)" : string.Empty);
        }

        #endregion

        #region Targets

        private MultiTemplate TemplateOrWarn()
        {
            MultiTemplate t = BuildTemplate();
            if (t.Components.Count == 0)
            {
                MessageBox.Show("No statics selected (check the items, the z range and the filters).", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            return t;
        }

        private string FileName(string extension)
        {
            string name = textBoxName.Text.Trim();
            if (name.Length == 0)
            {
                name = "building";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name + extension;
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            MultiTemplate t = TemplateOrWarn();
            if (t == null)
            {
                return;
            }

            MultiExportFormat format = _formats[Math.Max(0, comboBoxFormat.SelectedIndex)];
            string filter = MultiFormats.ExportFilter(format);
            using (var dialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = FileName(filter.Split('|')[1].TrimStart('*').Split(';')[0]),
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    MultiFormats.Export(t, dialog.FileName, format);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool keepsHue = format == MultiExportFormat.TxtHex || format == MultiExportFormat.TxtDecimal || format == MultiExportFormat.UoaBinary;
                MessageBox.Show($"Saved {t.Components.Count} parts to\n{dialog.FileName}" +
                                (t.HasHues && !keepsHue ? "\n\nNote: this format has no hues, the colors are lost." : string.Empty),
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickLibrary(object sender, EventArgs e)
        {
            MultiTemplate t = TemplateOrWarn();
            if (t == null)
            {
                return;
            }

            string file = MultiLibraryLocation.CurrentFile;
            try
            {
                using (var library = new MultiLibrary(file))
                {
                    MultiLibraryEntry entry = library.Add(t);
                    library.Flush();
                    MessageBox.Show($"\"{t.Name}\" stored in the library as #{entry.Id} ({t.Components.Count} parts).\n\n{library.DataFile}",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The library could not be written:\n{file}\n\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClickMulti(object sender, EventArgs e)
        {
            MultiTemplate t = TemplateOrWarn();
            if (t != null)
            {
                MultiBrowserForm.SaveToMultiMul(this, t);
            }
        }

        private void OnClickAddon(object sender, EventArgs e)
        {
            MultiTemplate t = TemplateOrWarn();
            if (t == null)
            {
                return;
            }

            string className = InputDialog.Show(this, "ServUO addon", "Class name:", ServUOExport.ClassName(t.Name), false);
            if (string.IsNullOrWhiteSpace(className))
            {
                return;
            }

            className = ServUOExport.ClassName(className.Replace("Addon", string.Empty));
            using (var dialog = new SaveFileDialog { Filter = "C# script (*.cs)|*.cs", FileName = className + ".cs", InitialDirectory = Options.OutputPath })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                File.WriteAllText(dialog.FileName, ServUOExport.AddonScript(t, className), new UTF8Encoding(false));
                MessageBox.Show($"Script saved:\n{dialog.FileName}\n\nCopy it into the Scripts folder of the server and add it in game with [add {className.Substring(0, className.Length - 5)}Deed.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickDecoration(object sender, EventArgs e)
        {
            List<MapStatic> statics = SelectedStatics();
            if (statics.Count == 0)
            {
                MessageBox.Show("No statics selected.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog { Filter = "Decoration (*.cfg)|*.cfg", FileName = FileName(".cfg"), InitialDirectory = Options.OutputPath })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string comment = $"{_definition.Name} (map {_definition.Id}) {_area.Left},{_area.Top} - {_area.Right - 1},{_area.Bottom - 1}";
                File.WriteAllText(dialog.FileName, ServUOExport.DecorationCfg(statics, comment), new UTF8Encoding(false));
                MessageBox.Show($"{statics.Count} statics saved:\n{dialog.FileName}\n\nThe positions are absolute map coordinates. Copy the file into Data/Decoration/<map> of the server.",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnClickMultiScript(object sender, EventArgs e)
        {
            MultiTemplate t = TemplateOrWarn();
            if (t == null)
            {
                return;
            }

            var form = new Forms.MultiScript { IndexImageText = ServUOExport.MultiScriptText(t) };
            form.Show(this);
        }

        #endregion
    }
}

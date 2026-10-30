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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Imaging;
using UoFiddler.Plugin.Enhanced.EcUltima.Maps;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Map editor of the Enhanced Client: facetN.uop / facetNx.uop (64 x 64 tile sectors with land, blend layers and statics),
    /// overview from the radar image facet{N:D2}.dds.
    /// </summary>
    public sealed partial class EcMapControl : UserControl, IEcEditor
    {
        // ------------------------------------------------------------------ fields

        private EcClient _client;
        private bool _loaded;
        private EcFacet _facet;
        private byte[] _radar;
        private int _sectorX = -1, _sectorY = -1;
        private EcSector _sector;
        private EcMapTile _clipboard;
        private readonly Dictionary<string, EcFacet> _facets = new Dictionary<string, EcFacet>(StringComparer.OrdinalIgnoreCase);

        public EcMapControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            // runtime only: defaults and the tile events of the map panels
            toolStripComboBoxZoom.SelectedIndex = 2;
            overviewPanel.TileClicked += (x, y) => SelectSector(x / EcSector.Size, y / EcSector.Size, x % EcSector.Size, y % EcSector.Size);
            sectorPanel.TileClicked += (x, y) => ShowTile(x, y);
            sectorPanel.TileHover += OnSectorTileHover;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _facet = null;
            _facets.Clear();
            _sector = null;
            sectorPanel.Sector = null;
            overviewPanel.SetImage(null, 1f);
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

            toolStripComboBoxFacet.Items.Clear();
            foreach (string file in _client.GetUopFiles().Select(Path.GetFileName).Where(EcFacet.IsFacetFile))
            {
                toolStripComboBoxFacet.Items.Add(file);
            }
            if (toolStripComboBoxFacet.Items.Count == 0)
            {
                toolStripStatusLabel.Text = "No facet*.uop files found.";
                return;
            }
            toolStripComboBoxFacet.SelectedIndex = 0;
            OpenFacet();
        }

        private void OpenFacet()
        {
            if (toolStripComboBoxFacet.SelectedItem is not string file)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                if (!_facets.TryGetValue(file, out EcFacet facet))
                {
                    EcUopPackage package = _client.GetPackage(file);
                    facet = new EcFacet(package, file);
                    _facets[file] = facet;
                    package.Changed += (s, e) => toolStripButtonSave.Enabled = _facets.Values.Any(f => f.IsModified || f.Package.IsDirty);
                }
                _facet = facet;
                LoadOverview();
                _sector = null;
                sectorPanel.Sector = null;
                sectorPanel.Invalidate();
                toolStripStatusLabel.Text = $"{file}: map {facet.Width} x {facet.Height}, {facet.SectorsX} x {facet.SectorsY} sectors" +
                                    (_radar == null ? $" – no radar image facet{facet.FacetId:D2}.dds found" : string.Empty);
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, $"{file} can't be read.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadOverview()
        {
            string path = EcFacet.FindRadarImage(_client.Folder, _facet.FacetId);
            _radar = path != null ? File.ReadAllBytes(path) : null;
            int level = toolStripComboBoxZoom.SelectedIndex;
            if (_radar != null)
            {
                DdsImage image = DdsImage.DecodeMip(_radar, level);
                float scale = (float)_facet.Width / image.Width;
                overviewPanel.SetImage(EcImageHelper.ToBitmap(image), scale);
            }
            else
            {
                // plain grid when there is no radar image
                int scale = 1 << (level + 2);
                var bitmap = new Bitmap(Math.Max(1, _facet.Width / scale), Math.Max(1, _facet.Height / scale));
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.FromArgb(30, 40, 60));
                    using (var pen = new Pen(Color.FromArgb(60, 80, 110)))
                    {
                        for (int x = 0; x < _facet.SectorsX; x++)
                        {
                            g.DrawLine(pen, x * 64 / scale, 0, x * 64 / scale, bitmap.Height);
                        }
                        for (int y = 0; y < _facet.SectorsY; y++)
                        {
                            g.DrawLine(pen, 0, y * 64 / scale, bitmap.Width, y * 64 / scale);
                        }
                    }
                }
                overviewPanel.SetImage(bitmap, scale);
            }
            if (_sectorX >= 0)
            {
                overviewPanel.Selection = new Rectangle(_sectorX * 64, _sectorY * 64, 64, 64);
            }
        }

        private void GoTo()
        {
            string[] parts = toolStripTextBoxGoTo.Text.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (_facet == null || parts.Length < 2 || !int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
            {
                return;
            }
            SelectSector(x / 64, y / 64, x % 64, y % 64);
            overviewPanel.ScrollTo(x, y);
        }

        private void SelectSector(int sx, int sy, int tileX, int tileY)
        {
            if (_facet == null)
            {
                return;
            }

            try
            {
                EcSector sector = _facet.GetSector(sx, sy);
                _sectorX = sx;
                _sectorY = sy;
                _sector = sector;
                sectorPanel.Sector = sector;
                sectorPanel.Colors = _radar != null ? DdsImage.DecodeRegion(_radar, sx * 64, sy * 64, 64, 64).Pixels : null;
                sectorPanel.Selected = new Point(tileX, tileY);
                sectorPanel.Invalidate();
                overviewPanel.Selection = new Rectangle(sx * 64, sy * 64, 64, 64);
                overviewPanel.Invalidate();
                if (sector == null)
                {
                    toolStripStatusLabel.Text = $"Sector {sx},{sy} doesn't exist in {_facet.FileName}.";
                    return;
                }
                ShowTile(tileX, tileY);
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The sector can't be read.", ex);
            }
        }

        private EcMapTile CurrentTile => _sector != null && sectorPanel.Selected.X >= 0 ? _sector[sectorPanel.Selected.X, sectorPanel.Selected.Y] : null;

        private void ShowTile(int x, int y)
        {
            EcMapTile tile = _sector?[x, y];
            listViewLayers.Items.Clear();
            listViewStatics.Items.Clear();
            previewPanelStatic.Image = null;
            if (tile == null)
            {
                return;
            }

            labelTileInfo.Text = $"Tile {(_sectorX * 64) + x}, {(_sectorY * 64) + y}  (sector {_sector.Index}: {x},{y})\r\n" +
                             $"land 0x{tile.LandId:X4}  z {tile.Z}";
            numericUpDownZ.Value = tile.Z;
            textBoxLand.Text = $"0x{tile.LandId:X4}";

            foreach (EcLandLayer l in tile.Layers)
            {
                var item = new ListViewItem(l.Blend.ToString()) { Tag = l };
                item.SubItems.Add(l.Z.ToString());
                item.SubItems.Add($"0x{l.LandId:X4}");
                listViewLayers.Items.Add(item);
            }
            foreach (EcStatic st in tile.Statics)
            {
                var item = new ListViewItem($"0x{st.ItemId:X4}") { Tag = st };
                item.SubItems.Add(st.Z.ToString());
                item.SubItems.Add(st.Hue.ToString());
                listViewStatics.Items.Add(item);
            }
            if (listViewStatics.Items.Count > 0)
            {
                listViewStatics.Items[0].Selected = true;
            }
        }

        private void ShowStaticImage()
        {
            previewPanelStatic.Image = null;
            if (listViewStatics.SelectedItems.Count != 1 || _client == null)
            {
                return;
            }
            var st = (EcStatic)listViewStatics.SelectedItems[0].Tag;
            byte[] data = _client.GetItemImage(st.ItemId, out _);
            previewPanelStatic.Image = data != null ? EcImageHelper.TryDecode(data, out _) : null;
        }

        private void Changed()
        {
            if (_sector == null)
            {
                return;
            }
            _facet.MarkModified(_sector);
            toolStripButtonSave.Enabled = true;
            sectorPanel.Invalidate();
            ShowTile(sectorPanel.Selected.X, sectorPanel.Selected.Y);
        }

        private void ApplyLand()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || !EcDialogs.TryParseNumber(textBoxLand.Text, out long land) || land < 0 || land > ushort.MaxValue)
            {
                return;
            }
            tile.Z = (sbyte)numericUpDownZ.Value;
            tile.LandId = (ushort)land;
            Changed();
        }

        private void PasteTile()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || _clipboard == null)
            {
                return;
            }
            EcMapTile copy = _clipboard.Clone();
            tile.Z = copy.Z;
            tile.LandId = copy.LandId;
            tile.Layers.Clear();
            tile.Layers.AddRange(copy.Layers);
            tile.Statics.Clear();
            tile.Statics.AddRange(copy.Statics);
            Changed();
        }

        private void RemoveLayer()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || listViewLayers.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in listViewLayers.SelectedItems)
            {
                tile.Layers.Remove((EcLandLayer)item.Tag);
            }
            Changed();
        }

        private bool AskStatic(string title, EcStatic st)
        {
            string[] values = EcDialogs.Prompt(this, title, new[] { "Item id:", "z:", "Hue:" },
                new[] { $"0x{st.ItemId:X4}", st.Z.ToString(CultureInfo.InvariantCulture), st.Hue.ToString(CultureInfo.InvariantCulture) });
            if (values == null || !EcDialogs.TryParseNumber(values[0], out long id) || !EcDialogs.TryParseNumber(values[1], out long z) || !EcDialogs.TryParseNumber(values[2], out long hue))
            {
                return false;
            }
            st.ItemId = (uint)id;
            st.Z = (sbyte)Math.Max(sbyte.MinValue, Math.Min(sbyte.MaxValue, z));
            st.Hue = (uint)hue;
            return true;
        }

        private void AddStatic()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null)
            {
                return;
            }
            var st = new EcStatic { Z = tile.Z };
            if (AskStatic("Add static", st))
            {
                tile.Statics.Add(st);
                Changed();
            }
        }

        private void EditStatic()
        {
            if (CurrentTile == null || listViewStatics.SelectedItems.Count != 1)
            {
                return;
            }
            if (AskStatic("Edit static", (EcStatic)listViewStatics.SelectedItems[0].Tag))
            {
                Changed();
            }
        }

        private void RemoveStatic()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || listViewStatics.SelectedItems.Count != 1)
            {
                return;
            }
            tile.Statics.Remove((EcStatic)listViewStatics.SelectedItems[0].Tag);
            Changed();
        }

        private void ClearSectorStatics()
        {
            if (_sector == null || !EcDialogs.Confirm(this, $"Remove all statics of sector {_sector.Index}?"))
            {
                return;
            }
            foreach (EcMapTile t in _sector.Tiles)
            {
                t.Statics.Clear();
            }
            Changed();
        }

        private void ExportSector()
        {
            if (_sector == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { Filter = "Sector (*.bin)|*.bin", FileName = $"facet{_facet.FacetId:D2}_sector{_sector.Index:D8}.bin" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, _sector.Write());
                }
            }
        }

        private void ImportSector()
        {
            if (_sector == null)
            {
                return;
            }
            using (var dialog = new OpenFileDialog { Filter = "Sector (*.bin)|*.bin|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    EcSector imported = EcSector.Read(File.ReadAllBytes(dialog.FileName));
                    for (int i = 0; i < EcSector.TileCount; i++)
                    {
                        _sector.Tiles[i] = imported.Tiles[i];
                    }
                    Changed();
                }
                catch (Exception ex)
                {
                    EcDialogs.ShowError(this, "The file is no valid sector.", ex);
                }
            }
        }

        private void Save(bool saveAs)
        {
            foreach (EcFacet f in _facets.Values)
            {
                f.Apply();
            }
            if (EcPackageSaver.Save(this, _facets.Values.Select(f => f.Package), saveAs))
            {
                toolStripButtonSave.Enabled = false;
                toolStripStatusLabel.Text = "Saved.";
            }
        }

        // ==================================================================== event handlers (designer)

        private void OnFacetChanged(object sender, EventArgs e)
        {
            if (_loaded)
            {
                OpenFacet();
            }
        }

        private void OnZoomChanged(object sender, EventArgs e)
        {
            if (_facet != null)
            {
                LoadOverview();
            }
        }

        private void OnGoToKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GoTo();
            }
        }

        private void OnShadingChanged(object sender, EventArgs e)
        {
            sectorPanel.Shading = toolStripButtonShading.Checked;
            sectorPanel.Invalidate();
        }

        private void OnClickExportSector(object sender, EventArgs e) => ExportSector();

        private void OnClickImportSector(object sender, EventArgs e) => ImportSector();

        private void OnClickClearStatics(object sender, EventArgs e) => ClearSectorStatics();

        private void OnClickSave(object sender, EventArgs e) => Save(false);

        private void OnClickSaveAs(object sender, EventArgs e) => Save(true);

        private void OnSectorTileHover(int x, int y)
        {
            if (_sector != null)
            {
                EcMapTile t = _sector[x, y];
                toolStripStatusLabel.Text = $"Tile {(_sectorX * 64) + x}, {(_sectorY * 64) + y}   land 0x{t.LandId:X4}  z {t.Z}  layers {t.Layers.Count}  statics {t.Statics.Count}";
            }
        }

        private void OnClickApplyLand(object sender, EventArgs e) => ApplyLand();

        private void OnClickCopyTile(object sender, EventArgs e)
        {
            if (CurrentTile != null)
            {
                _clipboard = CurrentTile.Clone();
                toolStripStatusLabel.Text = "Tile copied.";
            }
        }

        private void OnClickPasteTile(object sender, EventArgs e) => PasteTile();

        private void OnClickRemoveLayer(object sender, EventArgs e) => RemoveLayer();

        private void OnStaticSelectedIndexChanged(object sender, EventArgs e) => ShowStaticImage();

        private void OnClickAddStatic(object sender, EventArgs e) => AddStatic();

        private void OnClickEditStatic(object sender, EventArgs e) => EditStatic();

        private void OnClickRemoveStatic(object sender, EventArgs e) => RemoveStatic();

    }
}

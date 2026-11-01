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
using UoFiddler.Plugin.Enhanced.EcUltima.TileArt;
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
        private Rectangle _region = Rectangle.Empty;
        private EcItemSprites _sprites;
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
            sectorPanel.MarkChanged += OnSectorMarkChanged;
            overviewPanel.MarkChanged += OnOverviewMarkChanged;
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _facet = null;
            _sprites?.Clear();
            _sprites = null;
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
                ClearMark();
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
            _sprites ??= new EcItemSprites(_client);
            EcItemSprite sprite = _sprites.Get(st.ItemId, false);
            // the panel disposes its image: give it a copy
            previewPanelStatic.Image = sprite.Image != null ? new Bitmap(sprite.Image) : null;
            toolStripStatusLabel.Text = $"0x{st.ItemId:X4} {sprite.Name}: {sprite.Source ?? "no image"}";
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

        // ------------------------------------------------------------------ marked area / region viewer

        /// <summary>Most rows of the "Marked area" list (the export always takes all statics).</summary>
        private const int MaxMarkedRows = 5000;

        private void ClearMark()
        {
            _region = Rectangle.Empty;
            sectorPanel.Mark = Rectangle.Empty;
            overviewPanel.Mark = Rectangle.Empty;
            sectorPanel.Invalidate();
            overviewPanel.Invalidate();
            listViewMarked.Items.Clear();
            labelMarkedInfo.Text = "Check \"Mark area\" and drag a rectangle on the sector or the overview.";
            labelMark.Text = string.Empty;
        }

        private void SetRegion(Rectangle region)
        {
            if (_facet == null)
            {
                return;
            }
            _region = _facet.Clip(region);
            overviewPanel.Mark = _region;
            overviewPanel.Invalidate();
            if (_sectorX >= 0)
            {
                // the part of the mark inside the shown sector
                var sectorArea = new Rectangle(_sectorX * EcSector.Size, _sectorY * EcSector.Size, EcSector.Size, EcSector.Size);
                Rectangle inside = Rectangle.Intersect(sectorArea, _region);
                sectorPanel.Mark = inside.Width > 0 ? new Rectangle(inside.X - sectorArea.X, inside.Y - sectorArea.Y, inside.Width, inside.Height) : Rectangle.Empty;
                sectorPanel.Invalidate();
            }
            ShowMarked();
            tabControlInspector.SelectedTab = tabPageMarked;
        }

        private void ShowMarked()
        {
            listViewMarked.BeginUpdate();
            listViewMarked.Items.Clear();
            if (_facet == null || _region.Width <= 0)
            {
                listViewMarked.EndUpdate();
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                List<(int X, int Y, EcStatic Static)> statics = _facet.GetStatics(_region);
                foreach (var (x, y, st) in statics.Take(MaxMarkedRows))
                {
                    var item = new ListViewItem(x.ToString(CultureInfo.InvariantCulture)) { Tag = (x, y, st) };
                    item.SubItems.Add(y.ToString(CultureInfo.InvariantCulture));
                    item.SubItems.Add($"0x{st.ItemId:X4}");
                    item.SubItems.Add(ItemName(st.ItemId));
                    item.SubItems.Add(st.Z.ToString(CultureInfo.InvariantCulture));
                    item.SubItems.Add(st.Hue.ToString(CultureInfo.InvariantCulture));
                    listViewMarked.Items.Add(item);
                }
                string area = $"{_region.X},{_region.Y} – {_region.Right - 1},{_region.Bottom - 1} ({_region.Width} x {_region.Height})";
                labelMark.Text = area;
                labelMarkedInfo.Text = $"Area {area}\r\n{statics.Count} statics" + (statics.Count > MaxMarkedRows ? $" (the first {MaxMarkedRows} are listed)" : string.Empty);
            }
            finally
            {
                listViewMarked.EndUpdate();
                Cursor = Cursors.Default;
            }
        }

        private string ItemName(uint id)
        {
            try
            {
                TileArtEntry tile = _client?.TileArtIfLoaded?.Get(id)?.Entry;
                return tile != null && _client.HasStringDictionary ? _client.StringDictionary.Get(tile.NameIndex) ?? string.Empty : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>The marked area, else the shown sector.</summary>
        private Rectangle RegionOrSector =>
            _region.Width > 0 ? _region
            : _sectorX >= 0 ? new Rectangle(_sectorX * EcSector.Size, _sectorY * EcSector.Size, EcSector.Size, EcSector.Size)
            : Rectangle.Empty;

        /// <summary>Called after statics were changed outside the sector editor (marked area, region viewer).</summary>
        private void StaticsChanged()
        {
            toolStripButtonSave.Enabled = true;
            if (_sectorX >= 0)
            {
                // the sector may have been read again by the facet
                _sector = _facet.GetSector(_sectorX, _sectorY);
                sectorPanel.Sector = _sector;
                sectorPanel.Invalidate();
                if (sectorPanel.Selected.X >= 0)
                {
                    ShowTile(sectorPanel.Selected.X, sectorPanel.Selected.Y);
                }
            }
            if (_region.Width > 0)
            {
                ShowMarked();
            }
        }

        private void ViewRegion()
        {
            Rectangle area = RegionOrSector;
            if (_facet == null || area.Width <= 0)
            {
                EcDialogs.ShowError(this, "Mark an area or select a sector first.");
                return;
            }
            if (area.Width > Forms.EcRegionViewerForm.MaxAreaLegacy || area.Height > Forms.EcRegionViewerForm.MaxAreaLegacy)
            {
                if (!EcDialogs.Confirm(this, $"The area is {area.Width} x {area.Height} tiles. The viewer shows up to {Forms.EcRegionViewerForm.MaxAreaEc} x {Forms.EcRegionViewerForm.MaxAreaEc} tiles " +
                                             $"with EC images ({Forms.EcRegionViewerForm.MaxAreaLegacy} with legacy images) from the upper left corner; import / export use the whole area. Continue?"))
                {
                    return;
                }
            }
            var form = new Forms.EcRegionViewerForm(_client, _facet, area, _radar, StaticsChanged, () => Save(false));
            form.Show(this);
        }

        private IEnumerable<(int X, int Y, EcStatic Static)> SelectedMarked =>
            listViewMarked.SelectedItems.Cast<ListViewItem>().Select(i => ((int X, int Y, EcStatic Static))i.Tag);

        private void RemoveMarked()
        {
            var selected = SelectedMarked.ToList();
            if (selected.Count == 0 || !EcDialogs.Confirm(this, $"Remove {selected.Count} statics?"))
            {
                return;
            }
            foreach (var (x, y, st) in selected)
            {
                _facet.RemoveStatic(x, y, st);
            }
            StaticsChanged();
        }

        private void ClearMarkedStatics()
        {
            if (_region.Width <= 0 || !EcDialogs.Confirm(this, $"Remove all statics of the marked area ({_region.Width} x {_region.Height} tiles)?"))
            {
                return;
            }
            int removed = _facet.ClearStatics(_region);
            StaticsChanged();
            toolStripStatusLabel.Text = $"{removed} statics removed.";
        }

        private void ImportMarked()
        {
            Rectangle area = RegionOrSector;
            if (_facet == null || area.Width <= 0)
            {
                EcDialogs.ShowError(this, "Mark an area or select a sector first (the import is placed there).");
                return;
            }
            Point tile = _sectorX >= 0 && sectorPanel.Selected.X >= 0 && _region.Width <= 0
                ? new Point((_sectorX * EcSector.Size) + sectorPanel.Selected.X, (_sectorY * EcSector.Size) + sectorPanel.Selected.Y)
                : new Point(-1, -1);
            string message = EcStaticsUi.Import(this, _facet, area, tile, out Rectangle target);
            if (message != null)
            {
                if (_region.Width > 0)
                {
                    SetRegion(Rectangle.Union(_region, target));
                }
                StaticsChanged();
                toolStripStatusLabel.Text = message;
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


        private void OnMarkModeChanged(object sender, EventArgs e)
        {
            sectorPanel.MarkMode = checkBoxMark.Checked;
            overviewPanel.MarkMode = checkBoxMark.Checked;
            sectorPanel.Cursor = checkBoxMark.Checked ? Cursors.Cross : Cursors.Default;
            overviewPanel.Cursor = checkBoxMark.Checked ? Cursors.Cross : Cursors.Default;
            toolStripStatusLabel.Text = checkBoxMark.Checked ? "Drag a rectangle on the sector or on the overview." : string.Empty;
        }

        private void OnSectorMarkChanged(Rectangle mark)
        {
            if (_sectorX >= 0 && mark.Width > 0)
            {
                SetRegion(new Rectangle((_sectorX * EcSector.Size) + mark.X, (_sectorY * EcSector.Size) + mark.Y, mark.Width, mark.Height));
            }
        }

        private void OnOverviewMarkChanged(Rectangle mark)
        {
            if (mark.Width > 0)
            {
                SetRegion(mark);
            }
        }

        private void OnClickClearMark(object sender, EventArgs e) => ClearMark();

        private void OnClickViewRegion(object sender, EventArgs e) => ViewRegion();

        private void OnMarkedDoubleClick(object sender, EventArgs e)
        {
            var first = SelectedMarked.FirstOrDefault();
            if (first.Static == null)
            {
                return;
            }
            SelectSector(first.X / EcSector.Size, first.Y / EcSector.Size, first.X % EcSector.Size, first.Y % EcSector.Size);
            tabControlInspector.SelectedTab = tabPageTile;
            foreach (ListViewItem item in listViewStatics.Items)
            {
                item.Selected = ReferenceEquals(item.Tag, first.Static);
            }
        }

        private void OnClickRemoveMarked(object sender, EventArgs e) => RemoveMarked();

        private void OnClickClearMarkedStatics(object sender, EventArgs e) => ClearMarkedStatics();

        private void OnClickImportMarked(object sender, EventArgs e) => ImportMarked();

        private void OnClickExportMarked(object sender, EventArgs e)
        {
            Rectangle area = RegionOrSector;
            if (_facet != null && area.Width > 0 && Enum.TryParse((sender as ToolStripItem)?.Tag as string, out EcStaticsFormat format))
            {
                string message = EcStaticsUi.Export(this, _facet, area, format);
                if (message != null)
                {
                    toolStripStatusLabel.Text = message;
                }
            }
        }
    }
}

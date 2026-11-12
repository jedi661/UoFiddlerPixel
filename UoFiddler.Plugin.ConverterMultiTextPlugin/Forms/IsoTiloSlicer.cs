// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.IsoSlicer;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Cuts a large picture into UO land tile diamonds (44 × 44, every second row shifted) or straight rectangles.
    /// Views: the picture with the cut grid, the cut tiles put back together (with gaps), the tiles as land on a 3D / 2D map
    /// and all tiles. Output as files (+ layout.html / .csv / grid.png) or directly into art.mul.
    /// </summary>
    public partial class IsoTiloSlicer : Form
    {
        private readonly Timer _recutTimer = new Timer { Interval = 200 };
        private Bitmap _source;           // the picture that is cut (after mirror / rotate / scale)
        private Bitmap _original;         // as loaded
        private string _sourceName = "image";
        private List<SliceCell> _cells = new List<SliceCell>();
        private SliceCell _selected;
        private Color _gridColor = Color.FromArgb(200, 0, 255, 0);
        private Color _fillColor = Color.Black;
        private bool _assembledDirty = true, _mapDirty = true, _tilesDirty = true;
        private Bitmap _assembled, _mapImage;
        private TerrainScene _scene;
        private List<TerrainKind> _kinds = new List<TerrainKind>();
        private Dictionary<Point, SliceCell> _byMap = new Dictionary<Point, SliceCell>();
        private TerrainRenderOptions _mapOptions = new TerrainRenderOptions();
        private bool _loadingPreset;

        private static string TempDirectory => Path.Combine(Application.StartupPath, "tempGrafic");

        public IsoTiloSlicer()
        {
            InitializeComponent();
            // the designer does not store SelectedIndex
            foreach (ComboBox c in new[] { comboMode, comboPreset, comboOutside, comboFormat, comboMapView, comboHeights })
            {
                if (c.SelectedIndex < 0 && c.Items.Count > 0)
                {
                    c.SelectedIndex = 0;
                }
            }
            txtFolder.Text = TempDirectory;
            _recutTimer.Tick += (s, e) =>
            {
                _recutTimer.Stop();
                Recut();
            };
            gridTiles.CellSize = new Size(70, 84);
            gridTiles.ImageProvider = i => i >= 0 && i < _cells.Count ? _cells[i].Image : null;
            gridTiles.LabelProvider = i => i >= 0 && i < _cells.Count ? (_cells[i].FileNumber >= 0 ? $"#{_cells[i].FileNumber}" : $"{_cells[i].Column}/{_cells[i].Row}") : string.Empty;
            gridTiles.IsMarked = i => i >= 0 && i < _cells.Count && _cells[i].Empty;
            gridTiles.ItemActivated += (s, e) => { tabViews.SelectedTab = tab2D; FocusSelected(); };
            UpdateCutInfo();
            SetMessage("Open or paste a picture. Iso = UO land tiles (44 × 44 diamonds). F1 = help.");
        }

        #region Helpers

        private void SetMessage(string text) => statusMessage.Text = text;

        private SliceSettings ReadSettings() => new SliceSettings
        {
            Mode = comboMode.SelectedIndex == 1 ? SliceMode.Straight : SliceMode.IsoLand,
            TileWidth = (int)numTileWidth.Value,
            TileHeight = (int)numTileHeight.Value,
            GridX = (int)numGridX.Value,
            GridY = (int)numGridY.Value,
            Outside = (OutsideFill)Math.Max(0, comboOutside.SelectedIndex),
            FillColor = _fillColor,
            SkipEmpty = chkSkipEmpty.Checked
        };

        private SliceMode Mode => comboMode.SelectedIndex == 1 ? SliceMode.Straight : SliceMode.IsoLand;

        private bool CheckImage()
        {
            if (_source != null)
            {
                return true;
            }
            SetMessage("Open or paste a picture first.");
            return false;
        }

        private void UpdateCutInfo()
        {
            int w = (int)numTileWidth.Value, h = (int)numTileHeight.Value;
            string text = Mode == SliceMode.IsoLand
                ? $"Diamonds {w} × {h}, rows every {h / 2} px, every second row shifted by {w / 2} px." + (w == 44 && h == 44 ? " = UO land tiles." : " (UO land tiles are 44 × 44.)")
                : $"Rectangles {w} × {h}.";
            if (_source != null)
            {
                int used = _cells.Count(c => !c.Empty);
                text += $"\n{_cells.Count} tiles, {used} with content" + (_cells.Count > used ? $", {_cells.Count - used} empty" : string.Empty) + ".";
            }
            lblCutInfo.Text = text;
        }

        private void UpdateImageInfo()
        {
            lblImageInfo.Text = _source == null ? "No image." : $"{_sourceName}: {_source.Width} × {_source.Height} px" +
                (_original != null && _original.Size != _source.Size ? $" (original {_original.Width} × {_original.Height})" : string.Empty);
        }

        #endregion

        #region Image

        private void SetSource(Bitmap bmp, string name, bool isOriginal)
        {
            if (bmp == null)
            {
                return;
            }
            if (isOriginal)
            {
                _original?.Dispose();
                _original = CutterImaging.To32Bpp(bmp);
                _sourceName = name;
            }
            Bitmap old = _source;
            _source = CutterImaging.To32Bpp(bmp);
            if (!ReferenceEquals(bmp, _original))
            {
                bmp.Dispose();
            }
            view2D.Image = _source;
            old?.Dispose();
            UpdateImageInfo();
            Recut();
            view2D.FitImage();
        }

        private void TsOpen_Click(object sender, EventArgs e)
        {
            using (var d = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Picture to cut" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    SetSource(CutterImaging.LoadBitmap(d.FileName), Path.GetFileNameWithoutExtension(d.FileName), true);
                    SetMessage("Loaded " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The picture could not be loaded:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void TsPaste_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = CutterImaging.GetClipboardImage();
                if (bmp == null)
                {
                    SetMessage("The clipboard contains no picture.");
                    return;
                }
                SetSource(bmp, "clipboard", true);
                SetMessage("Picture pasted.");
            }
            catch (Exception ex)
            {
                SetMessage("Clipboard: " + ex.Message);
            }
        }

        private void Transform(RotateFlipType type, string message)
        {
            if (!CheckImage())
            {
                return;
            }
            Bitmap copy = CutterImaging.To32Bpp(_source);
            copy.RotateFlip(type);
            SetSource(copy, _sourceName, false);
            SetMessage(message);
        }

        private void BtnMirror_Click(object sender, EventArgs e) => Transform(RotateFlipType.RotateNoneFlipX, "Mirrored – the cut uses the mirrored picture now.");

        private void BtnRotate_Click(object sender, EventArgs e) => Transform(RotateFlipType.Rotate90FlipNone, "Rotated by 90°.");

        private void BtnUndoImage_Click(object sender, EventArgs e)
        {
            if (_original != null)
            {
                SetSource(_original, _sourceName, false);
                SetMessage("Back to the original picture.");
            }
        }

        private void BtnScale_Click(object sender, EventArgs e)
        {
            if (!CheckImage())
            {
                return;
            }
            double f = (double)numScale.Value / 100.0;
            SetSource(IsoSlicerCore.Scale(_source, (int)Math.Round(_source.Width * f), (int)Math.Round(_source.Height * f)), _sourceName, false);
            SetMessage($"Scaled to {numScale.Value}%.");
        }

        private void BtnFit_Click(object sender, EventArgs e)
        {
            if (!CheckImage())
            {
                return;
            }
            int w = (int)numTileWidth.Value * (int)numFitTiles.Value;
            int h = (int)Math.Round(_source.Height * (w / (double)_source.Width));
            SetSource(IsoSlicerCore.Scale(_source, w, h), _sourceName, false);
            SetMessage($"Scaled to {w} × {h} = {numFitTiles.Value} tiles wide.");
        }

        private void BtnFillColor_Click(object sender, EventArgs e)
        {
            using (var d = new ColorDialog { Color = _fillColor, FullOpen = true })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _fillColor = d.Color;
                    btnFillColor.BackColor = d.Color;
                    comboOutside.SelectedIndex = 2;
                    ScheduleRecut();
                }
            }
        }

        #endregion

        #region Cutting

        private void Settings_Changed(object sender, EventArgs e)
        {
            if (sender == numTileWidth || sender == numTileHeight)
            {
                if (!_loadingPreset)
                {
                    comboPreset.SelectedIndex = comboPreset.Items.Count - 1; // custom
                }
            }
            UpdateCutInfo();
            ScheduleRecut();
        }

        private void ComboPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size = comboPreset.SelectedIndex switch
            {
                0 => 44,
                1 => 64,
                2 => 128,
                3 => 44,
                _ => -1
            };
            if (size < 0)
            {
                return;
            }
            _loadingPreset = true;
            numTileWidth.Value = size;
            numTileHeight.Value = size;
            if (comboPreset.SelectedIndex == 0)
            {
                comboMode.SelectedIndex = 0;
            }
            else
            {
                comboMode.SelectedIndex = 1;
            }
            _loadingPreset = false;
            ScheduleRecut();
        }

        private void ScheduleRecut()
        {
            _recutTimer.Stop();
            _recutTimer.Start();
        }

        private void Recut()
        {
            if (_source == null)
            {
                view2D.Invalidate();
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (SliceCell c in _cells)
                {
                    c.Image?.Dispose();
                }
                SliceSettings s = ReadSettings();
                _cells = IsoSlicerCore.Layout(_source.Size, s);
                IsoSlicerCore.Cut(_source, _cells, s);
                _selected = null;
                _assembledDirty = _mapDirty = _tilesDirty = true;
                UpdateCutInfo();
                view2D.Invalidate();
                RefreshVisibleView();
                groupArt.Enabled = true;
                lblArtInfo.Text = Mode == SliceMode.IsoLand && s.TileWidth == 44 && s.TileHeight == 44
                    ? $"{_cells.Count(c => !c.Empty || !chkSkipEmpty.Checked)} tiles → land tiles from the ID."
                    : "Land tiles must be 44 × 44 iso – write them as items, or choose the UO land preset.";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Views

        private void TabViews_SelectedIndexChanged(object sender, EventArgs e) => RefreshVisibleView();

        private void View_Changed(object sender, EventArgs e)
        {
            if (sender == chkShowGrid || sender == chkShowNumbers || sender == chkMarkEmpty)
            {
                view2D.Invalidate();
                return;
            }
            if (sender == numGap || sender == chkOutlines || sender == chkAsmNumbers || sender == chkHideEmpty)
            {
                _assembledDirty = true;
                _mapDirty |= sender == chkHideEmpty;
            }
            else
            {
                _mapDirty = true;
            }
            RefreshVisibleView();
        }

        private void RefreshVisibleView()
        {
            if (tabViews.SelectedTab == tabAssembled && _assembledDirty)
            {
                Bitmap old = _assembled;
                _assembled = _cells.Count == 0 ? null : IsoSlicerCore.Assemble(_cells, Mode, (int)numGap.Value, chkOutlines.Checked, chkAsmNumbers.Checked, chkHideEmpty.Checked);
                viewAssembled.Image = _assembled;
                old?.Dispose();
                _assembledDirty = false;
            }
            else if (tabViews.SelectedTab == tabMap && _mapDirty)
            {
                BuildMap();
            }
            else if (tabViews.SelectedTab == tabTiles && _tilesDirty)
            {
                gridTiles.Items = Enumerable.Range(0, _cells.Count).Where(i => !chkHideEmpty.Checked || !_cells[i].Empty).ToList();
                _tilesDirty = false;
            }
            tsZoomLabel.Text = CurrentView()?.ZoomText ?? string.Empty;
        }

        private void BuildMap()
        {
            foreach (TerrainKind k in _kinds)
            {
                k.Dispose();
            }
            _kinds.Clear();
            Bitmap old = _mapImage;
            _mapImage = null;
            if (_cells.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    _scene = IsoSlicerCore.BuildScene(_cells, Mode, (IsoSlicerCore.HeightSource)Math.Max(0, comboHeights.SelectedIndex), (int)numMaxZ.Value,
                        chkHideEmpty.Checked, out _kinds, out _byMap);
                    _mapOptions = new TerrainRenderOptions
                    {
                        Isometric = comboMapView.SelectedIndex == 0,
                        Grid = chkMapGrid.Checked,
                        Light = chkMapLight.Checked,
                        Heights = chkMapHeights.Checked,
                        ArtForFlat = Mode == SliceMode.IsoLand,
                        CellSize2D = Math.Max(16, Math.Min(64, (int)numTileWidth.Value)),
                        Highlight = _selected != null ? _selected.Map : new Point(-1, -1)
                    };
                    _mapImage = TerrainRenderer.Render(_scene, _kinds, _mapOptions);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            viewMap.Image = _mapImage;
            old?.Dispose();
            _mapDirty = false;
        }

        private ZoomView CurrentView() =>
            tabViews.SelectedTab == tabAssembled ? viewAssembled : tabViews.SelectedTab == tabMap ? viewMap : tabViews.SelectedTab == tab2D ? view2D : null;

        private void View_ZoomChanged(object sender, EventArgs e) => tsZoomLabel.Text = CurrentView()?.ZoomText ?? string.Empty;

        private void TsZoomIn_Click(object sender, EventArgs e) => CurrentView()?.ZoomStep(1);

        private void TsZoomOut_Click(object sender, EventArgs e) => CurrentView()?.ZoomStep(-1);

        private void TsFit_Click(object sender, EventArgs e) => CurrentView()?.FitImage();

        /// <summary>The cut grid over the picture: outlines, numbers, empty tiles, hovered / selected tile.</summary>
        private void View2D_PaintOverlay(object sender, ZoomPaintEventArgs e)
        {
            if (_cells.Count == 0)
            {
                return;
            }
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            SliceMode mode = Mode;
            using (var gridPen = new Pen(_gridColor, 1f))
            using (var emptyBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(90, 255, 80, 80), Color.Transparent))
            using (var font = new Font("Segoe UI", 7.5f))
            {
                foreach (SliceCell c in _cells)
                {
                    PointF[] pts = IsoSlicerCore.Outline(c, mode);
                    e.ImageToScreen.TransformPoints(pts);
                    if (chkMarkEmpty.Checked && c.Empty)
                    {
                        g.FillPolygon(emptyBrush, pts);
                    }
                    if (chkShowGrid.Checked)
                    {
                        g.DrawPolygon(gridPen, pts);
                    }
                    if (chkShowNumbers.Checked && e.Zoom >= 0.75f)
                    {
                        float cx = pts.Average(p => p.X), cy = pts.Average(p => p.Y);
                        string t = c.FileNumber >= 0 ? c.FileNumber.ToString() : $"{c.Column},{c.Row}";
                        TextRenderer.DrawText(g, t, font, new Point((int)cx - 12, (int)cy - 7), Color.Yellow);
                    }
                }
                if (_selected != null)
                {
                    PointF[] pts = IsoSlicerCore.Outline(_selected, mode);
                    e.ImageToScreen.TransformPoints(pts);
                    using (var sel = new Pen(Color.Yellow, 2.5f))
                    {
                        g.DrawPolygon(sel, pts);
                    }
                }
            }
        }

        private void View_HoverPixel(object sender, Point p)
        {
            if (p.X < 0)
            {
                statusPos.Text = string.Empty;
                return;
            }
            string text = $"X {p.X}  Y {p.Y}";
            if (sender == view2D && _source != null)
            {
                Color col = _source.GetPixel(p.X, p.Y);
                text += $"  #{col.R:X2}{col.G:X2}{col.B:X2}";
                SliceCell c = IsoSlicerCore.HitTest(_cells, Mode, p);
                if (c != null)
                {
                    text += $"  ·  tile {c.Column}/{c.Row}, map {c.Map.X}/{c.Map.Y}" + (c.Empty ? " (empty)" : string.Empty);
                }
            }
            statusPos.Text = text;
        }

        private void View2D_ImageClick(object sender, MouseEventArgs e)
        {
            PointF ip = view2D.ToImage(e.Location);
            Select(IsoSlicerCore.HitTest(_cells, Mode, new Point((int)Math.Floor(ip.X), (int)Math.Floor(ip.Y))));
        }

        private void ViewMap_HoverPixel(object sender, Point p)
        {
            if (p.X < 0 || _scene == null)
            {
                statusPos.Text = string.Empty;
                return;
            }
            Point cell = TerrainRenderer.HitTest(_scene, _mapOptions, p);
            statusPos.Text = _byMap.TryGetValue(cell, out SliceCell c)
                ? $"map {cell.X}/{cell.Y}  ·  tile {c.Column}/{c.Row}  z {_scene.GetZ(cell.X, cell.Y)}"
                : cell.X >= 0 ? $"map {cell.X}/{cell.Y}" : string.Empty;
        }

        private void ViewMap_ImageClick(object sender, MouseEventArgs e)
        {
            if (_scene == null)
            {
                return;
            }
            PointF ip = viewMap.ToImage(e.Location);
            Point cell = TerrainRenderer.HitTest(_scene, _mapOptions, new Point((int)ip.X, (int)ip.Y));
            if (_byMap.TryGetValue(cell, out SliceCell c))
            {
                Select(c);
                _mapDirty = true;
                PointF center = viewMap.ToImage(new Point(viewMap.Width / 2, viewMap.Height / 2));
                BuildMap();
                viewMap.CenterOn(center);
            }
        }

        private void GridTiles_SelectionChanged(object sender, EventArgs e)
        {
            int i = gridTiles.SelectedItem;
            if (i >= 0 && i < _cells.Count)
            {
                Select(_cells[i], false);
            }
        }

        private void Select(SliceCell c, bool syncGrid = true)
        {
            _selected = c;
            view2D.Invalidate();
            if (c == null)
            {
                return;
            }
            if (syncGrid)
            {
                gridTiles.SelectItem(c.Index);
            }
            SetMessage($"Tile {c.Column}/{c.Row} · image {c.Bounds.X}, {c.Bounds.Y} · map {c.Map.X}/{c.Map.Y}" + (c.Empty ? " · empty" : string.Empty) +
                       (c.FileNumber >= 0 ? $" · file #{c.FileNumber}" : string.Empty) + "  (Ctrl+C copies it)");
        }

        private void FocusSelected()
        {
            if (_selected != null)
            {
                view2D.CenterOn(new PointF(_selected.Bounds.X + _selected.Bounds.Width / 2f, _selected.Bounds.Y + _selected.Bounds.Height / 2f));
            }
        }

        private void BtnGridColor_Click(object sender, EventArgs e)
        {
            using (var d = new ColorDialog { Color = Color.FromArgb(255, _gridColor), FullOpen = true })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _gridColor = Color.FromArgb(200, d.Color);
                    view2D.Invalidate();
                }
            }
        }

        #endregion

        #region Output

        private void BtnFolder_Click(object sender, EventArgs e)
        {
            using (var d = new FolderBrowserDialog { SelectedPath = Directory.Exists(txtFolder.Text) ? txtFolder.Text : TempDirectory })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    txtFolder.Text = d.SelectedPath;
                }
            }
        }

        private void TsSlice_Click(object sender, EventArgs e)
        {
            if (!CheckImage())
            {
                return;
            }
            Recut();
            string folder = string.IsNullOrWhiteSpace(txtFolder.Text) ? TempDirectory : txtFolder.Text.Trim();
            (ImageFormat format, string ext) = comboFormat.SelectedIndex switch
            {
                1 => (ImageFormat.Png, "png"),
                2 => (ImageFormat.Tiff, "tiff"),
                _ => (ImageFormat.Bmp, "bmp")
            };
            var o = new IsoSlicerCore.OutputSettings
            {
                Folder = folder,
                FileNameFormat = string.IsNullOrWhiteSpace(txtFileName.Text) ? "{0}" : txtFileName.Text.Trim(),
                StartNumber = (int)numStart.Value,
                Format = format,
                Extension = ext,
                Html = chkHtml.Checked,
                Csv = chkCsv.Checked,
                GridImage = chkGridPng.Checked
            };
            try
            {
                Cursor = Cursors.WaitCursor;
                int n = IsoSlicerCore.Save(_cells, ReadSettings(), o);
                _assembledDirty = _tilesDirty = true;
                RefreshVisibleView();
                view2D.Invalidate();
                SetMessage($"{n} tiles written into {folder}" + (o.Html ? " (+ layout.html)" : string.Empty) + ".");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void TsOpenFolder_Click(object sender, EventArgs e)
        {
            string folder = string.IsNullOrWhiteSpace(txtFolder.Text) ? TempDirectory : txtFolder.Text.Trim();
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                SetMessage("Cannot open the folder: " + ex.Message);
            }
        }

        private void TsDeleteTemp_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(TempDirectory))
            {
                SetMessage("tempGrafic does not exist yet.");
                return;
            }
            string[] files = Directory.GetFiles(TempDirectory);
            if (files.Length == 0)
            {
                SetMessage("tempGrafic is empty.");
                return;
            }
            if (MessageBox.Show(this, $"Delete {files.Length} file(s) in tempGrafic?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            int errors = 0;
            foreach (string f in files)
            {
                try
                {
                    File.Delete(f);
                }
                catch (Exception)
                {
                    errors++;
                }
            }
            SetMessage(errors == 0 ? $"{files.Length} files deleted." : $"Deleted with {errors} error(s) – some files are in use.");
        }

        private void TsCopyTile_Click(object sender, EventArgs e)
        {
            if (_selected?.Image == null)
            {
                SetMessage("Click a tile first.");
                return;
            }
            try
            {
                CutterImaging.CopyToClipboard(_selected.Image);
                SetMessage($"Tile {_selected.Column}/{_selected.Row} copied.");
            }
            catch (Exception ex)
            {
                SetMessage("Copy failed: " + ex.Message);
            }
        }

        #endregion

        #region art.mul

        private ArtKind ArtKindSelected => radioLand.Checked ? ArtKind.Land : ArtKind.Item;

        private List<SliceCell> TilesForArt() => _cells.Where(c => c.Image != null && (!chkSkipEmpty.Checked || !c.Empty)).ToList();

        private bool CheckArt()
        {
            if (!CheckImage())
            {
                return false;
            }
            if (!CutterArt.IsAvailable)
            {
                SetMessage("art.mul is not loaded.");
                return false;
            }
            if (ArtKindSelected == ArtKind.Land && (Mode != SliceMode.IsoLand || numTileWidth.Value != 44 || numTileHeight.Value != 44))
            {
                MessageBox.Show(this, "Land tiles must be 44 × 44 iso diamonds. Choose the preset \"UO land tile 44 × 44\" or write them as items.", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
            return true;
        }

        private void BtnFreeArt_Click(object sender, EventArgs e)
        {
            if (!CheckArt())
            {
                return;
            }
            int count = TilesForArt().Count;
            int id = CutterArt.FindFreeBlock(ArtKindSelected, (int)numArtId.Value, count);
            if (id < 0)
            {
                SetMessage($"No {count} free IDs in a row found.");
                return;
            }
            numArtId.Value = id;
            SetMessage($"{count} free IDs from 0x{id:X4}.");
        }

        private void BtnWriteArt_Click(object sender, EventArgs e)
        {
            if (!CheckArt())
            {
                return;
            }
            List<SliceCell> tiles = TilesForArt();
            int start = (int)numArtId.Value;
            if (MessageBox.Show(this, $"Write {tiles.Count} tiles as {(ArtKindSelected == ArtKind.Land ? "land tiles" : "items")} from 0x{start:X4}" +
                                      (chkSkipUsed.Checked ? " (used IDs are skipped)" : " (used IDs are replaced!)") + "?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                List<int> ids = CutterArt.WriteMany(ArtKindSelected, start, tiles.Select(c => c.Image).ToList(), chkSkipUsed.Checked, this);
                for (int i = 0; i < tiles.Count && i < ids.Count; i++)
                {
                    tiles[i].FileNumber = ids[i];
                }
                _assembledDirty = _tilesDirty = true;
                view2D.Invalidate();
                RefreshVisibleView();
                SetMessage(ids.Count == 0 ? "Nothing written." : $"{ids.Count} tiles written: 0x{ids.First():X4} … 0x{ids.Last():X4}. Numbers in the views = IDs now. Save art.mul to keep them.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BtnSaveArt_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, $"Save artidx.mul and art.mul into:\n{UoFiddler.Controls.Classes.Options.OutputPath}", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                SetMessage("art.mul saved in " + CutterArt.Save());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Keyboard, help, closing

        private void TsHelp_Click(object sender, EventArgs e)
        {
            using (var help = new IsoTiloSlicerHelp())
            {
                help.ShowDialog(this);
            }
        }

        private bool IsTextInputFocused()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is UpDownBase || c?.Parent is UpDownBase || c is ComboBox;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1: TsHelp_Click(this, EventArgs.Empty); return true;
                case Keys.Control | Keys.O: TsOpen_Click(this, EventArgs.Empty); return true;
                case Keys.Control | Keys.S: TsSlice_Click(this, EventArgs.Empty); return true;
            }
            if (!IsTextInputFocused())
            {
                switch (keyData)
                {
                    case Keys.Control | Keys.V: TsPaste_Click(this, EventArgs.Empty); return true;
                    case Keys.Control | Keys.C: TsCopyTile_Click(this, EventArgs.Empty); return true;
                    case Keys.Oemplus:
                    case Keys.Add: TsZoomIn_Click(this, EventArgs.Empty); return true;
                    case Keys.OemMinus:
                    case Keys.Subtract: TsZoomOut_Click(this, EventArgs.Empty); return true;
                    case Keys.D0:
                    case Keys.NumPad0: TsFit_Click(this, EventArgs.Empty); return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _recutTimer.Dispose();
            foreach (SliceCell c in _cells)
            {
                c.Image?.Dispose();
            }
            foreach (TerrainKind k in _kinds)
            {
                k.Dispose();
            }
            _source?.Dispose();
            _original?.Dispose();
            _assembled?.Dispose();
            _mapImage?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion
    }
}

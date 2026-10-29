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
    public sealed class EcMapControl : UserControl, IEcEditor
    {
        // ------------------------------------------------------------------ overview panel

        private sealed class OverviewPanel : ScrollableControl
        {
            public Bitmap Image;
            public float TileScale = 1f;    // tiles per pixel of the image
            public Rectangle Selection;     // in tiles
            public event Action<int, int> TileClicked;

            public OverviewPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
                AutoScroll = true;
                BackColor = Color.Black;
            }

            public void SetImage(Bitmap image, float scale)
            {
                Image?.Dispose();
                Image = image;
                TileScale = scale;
                AutoScrollMinSize = image?.Size ?? Size.Empty;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);
                if (Image == null)
                {
                    return;
                }
                e.Graphics.DrawImageUnscaled(Image, AutoScrollPosition.X, AutoScrollPosition.Y);
                if (Selection.Width > 0)
                {
                    var r = new Rectangle(
                        (int)(Selection.X / TileScale) + AutoScrollPosition.X,
                        (int)(Selection.Y / TileScale) + AutoScrollPosition.Y,
                        Math.Max(3, (int)(Selection.Width / TileScale)),
                        Math.Max(3, (int)(Selection.Height / TileScale)));
                    using (var pen = new Pen(Color.Red, 2))
                    {
                        e.Graphics.DrawRectangle(pen, r);
                    }
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (Image == null)
                {
                    return;
                }
                int x = (int)((e.X - AutoScrollPosition.X) * TileScale);
                int y = (int)((e.Y - AutoScrollPosition.Y) * TileScale);
                TileClicked?.Invoke(x, y);
            }

            public void ScrollTo(int tileX, int tileY)
            {
                if (Image == null)
                {
                    return;
                }
                int px = (int)(tileX / TileScale) - (ClientSize.Width / 2);
                int py = (int)(tileY / TileScale) - (ClientSize.Height / 2);
                AutoScrollPosition = new Point(Math.Max(0, px), Math.Max(0, py));
                Invalidate();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Image?.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        // ------------------------------------------------------------------ sector panel

        private sealed class SectorPanel : Control
        {
            public EcSector Sector;
            public byte[] Colors;           // BGRA 64 x 64 from the radar image (may be null)
            public int CellSize = 9;
            public Point Selected { get; set; } = new Point(-1, -1);
            public bool Shading = true;
            public event Action<int, int> TileClicked;
            public event Action<int, int> TileHover;

            public SectorPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
                BackColor = Color.DimGray;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                if (Sector == null)
                {
                    TextRenderer.DrawText(g, "No sector (click on the map).", Font, new Point(10, 10), Color.White);
                    return;
                }

                using (var brush = new SolidBrush(Color.Black))
                using (var staticBrush = new SolidBrush(Color.FromArgb(220, 255, 60, 30)))
                {
                    for (int x = 0; x < EcSector.Size; x++)
                    {
                        for (int y = 0; y < EcSector.Size; y++)
                        {
                            EcMapTile tile = Sector[x, y];
                            Color c = TileColor(x, y, tile);
                            brush.Color = c;
                            g.FillRectangle(brush, x * CellSize, y * CellSize, CellSize, CellSize);
                            if (tile.Statics.Count > 0 && CellSize >= 4)
                            {
                                int s = Math.Max(2, CellSize / 3);
                                g.FillRectangle(staticBrush, (x * CellSize) + ((CellSize - s) / 2), (y * CellSize) + ((CellSize - s) / 2), s, s);
                            }
                        }
                    }
                }

                if (Selected.X >= 0)
                {
                    using (var pen = new Pen(Color.Yellow, 2))
                    {
                        g.DrawRectangle(pen, Selected.X * CellSize, Selected.Y * CellSize, CellSize, CellSize);
                    }
                }
            }

            private Color TileColor(int x, int y, EcMapTile tile)
            {
                int r, gr, b;
                if (Colors != null)
                {
                    int o = ((y * EcSector.Size) + x) * 4;
                    b = Colors[o];
                    gr = Colors[o + 1];
                    r = Colors[o + 2];
                }
                else
                {
                    // no radar image: a color per land id
                    int h = (int)((tile.LandId * 2654435761u) >> 8);
                    r = 60 + (h & 0x7F);
                    gr = 60 + ((h >> 7) & 0x7F);
                    b = 60 + ((h >> 14) & 0x7F);
                }

                if (Shading)
                {
                    // lighter = higher
                    double f = 1.0 + (tile.Z / 160.0);
                    r = Math.Max(0, Math.Min(255, (int)(r * f)));
                    gr = Math.Max(0, Math.Min(255, (int)(gr * f)));
                    b = Math.Max(0, Math.Min(255, (int)(b * f)));
                }
                return Color.FromArgb(r, gr, b);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                Focus();
                int x = e.X / CellSize, y = e.Y / CellSize;
                if (Sector != null && x >= 0 && y >= 0 && x < EcSector.Size && y < EcSector.Size)
                {
                    Selected = new Point(x, y);
                    Invalidate();
                    TileClicked?.Invoke(x, y);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                int x = e.X / CellSize, y = e.Y / CellSize;
                if (x >= 0 && y >= 0 && x < EcSector.Size && y < EcSector.Size)
                {
                    TileHover?.Invoke(x, y);
                }
            }

            protected override bool IsInputKey(Keys keyData)
            {
                Keys k = keyData & Keys.KeyCode;
                return k == Keys.Left || k == Keys.Right || k == Keys.Up || k == Keys.Down || base.IsInputKey(keyData);
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                if (Sector == null || Selected.X < 0)
                {
                    return;
                }
                int x = Selected.X, y = Selected.Y;
                switch (e.KeyCode)
                {
                    case Keys.Left: x--; break;
                    case Keys.Right: x++; break;
                    case Keys.Up: y--; break;
                    case Keys.Down: y++; break;
                    default: return;
                }
                x = Math.Max(0, Math.Min(63, x));
                y = Math.Max(0, Math.Min(63, y));
                Selected = new Point(x, y);
                Invalidate();
                TileClicked?.Invoke(x, y);
            }
        }

        // ------------------------------------------------------------------ fields

        private readonly ToolStripComboBox _facetBox;
        private readonly ToolStripComboBox _zoomBox;
        private readonly ToolStripTextBox _gotoBox;
        private readonly ToolStripButton _shadingButton;
        private readonly ToolStripButton _saveButton;
        private readonly OverviewPanel _overview;
        private readonly SectorPanel _sectorPanel;
        private readonly Label _tileInfo;
        private readonly NumericUpDown _zBox;
        private readonly TextBox _landBox;
        private readonly ListView _layerList;
        private readonly ListView _staticList;
        private readonly EcPreviewPanel _staticPreview;
        private readonly ToolStripStatusLabel _statusLabel;

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
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _facetBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140, ToolTipText = "Map file" };
            _facetBox.SelectedIndexChanged += (s, e) => { if (_loaded) { OpenFacet(); } };
            _zoomBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70, ToolTipText = "Size of the overview" };
            _zoomBox.Items.AddRange(new object[] { "1:1", "1:2", "1:4", "1:8" });
            _zoomBox.SelectedIndex = 2;
            _zoomBox.SelectedIndexChanged += (s, e) => { if (_facet != null) { LoadOverview(); } };
            _gotoBox = new ToolStripTextBox { Width = 90, ToolTipText = "x,y – Enter" };
            _gotoBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; GoTo(); } };
            _shadingButton = new ToolStripButton("Height shading") { CheckOnClick = true, Checked = true };
            _shadingButton.CheckedChanged += (s, e) => { _sectorPanel.Shading = _shadingButton.Checked; _sectorPanel.Invalidate(); };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(new ToolStripLabel("Map:"));
            toolStrip.Items.Add(_facetBox);
            toolStrip.Items.Add(new ToolStripLabel("Overview:"));
            toolStrip.Items.Add(_zoomBox);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripLabel("Go to x,y:"));
            toolStrip.Items.Add(_gotoBox);
            toolStrip.Items.Add(_shadingButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            var sectorButton = new ToolStripDropDownButton("Sector");
            sectorButton.DropDownItems.Add("Export sector (.bin)...", null, (s, e) => ExportSector());
            sectorButton.DropDownItems.Add("Import sector (.bin)...", null, (s, e) => ImportSector());
            sectorButton.DropDownItems.Add(new ToolStripSeparator());
            sectorButton.DropDownItems.Add("Remove all statics of the sector", null, (s, e) => ClearSectorStatics());
            toolStrip.Items.Add(sectorButton);
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _overview = new OverviewPanel { Dock = DockStyle.Fill };
            _overview.TileClicked += (x, y) => SelectSector(x / EcSector.Size, y / EcSector.Size, x % EcSector.Size, y % EcSector.Size);

            _sectorPanel = new SectorPanel { Dock = DockStyle.Fill };
            _sectorPanel.TileClicked += (x, y) => ShowTile(x, y);
            _sectorPanel.TileHover += (x, y) =>
            {
                if (_sector != null)
                {
                    EcMapTile t = _sector[x, y];
                    _statusLabel.Text = $"Tile {(_sectorX * 64) + x}, {(_sectorY * 64) + y}   land 0x{t.LandId:X4}  z {t.Z}  layers {t.Layers.Count}  statics {t.Statics.Count}";
                }
            };
            var sectorScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            _sectorPanel.Dock = DockStyle.None;
            _sectorPanel.Size = new Size(64 * 9, 64 * 9);
            sectorScroll.Controls.Add(_sectorPanel);

            // ---------------- tile inspector
            _tileInfo = new Label { Dock = DockStyle.Top, Height = 40, Padding = new Padding(4), Font = EcDialogs.MonoFont(8.5f) };
            _zBox = new NumericUpDown { Minimum = -128, Maximum = 127, Width = 60 };
            _landBox = new TextBox { Width = 70 };
            var applyButton = new Button { Text = "Apply", Width = 60 };
            applyButton.Click += (s, e) => ApplyLand();
            var landPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34 };
            landPanel.Controls.Add(new Label { Text = "z:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) });
            landPanel.Controls.Add(_zBox);
            landPanel.Controls.Add(new Label { Text = "Land:", AutoSize = true, Margin = new Padding(6, 8, 0, 0) });
            landPanel.Controls.Add(_landBox);
            landPanel.Controls.Add(applyButton);

            var tileButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            AddButton(tileButtons, "Copy tile", (s, e) => { if (CurrentTile != null) { _clipboard = CurrentTile.Clone(); _statusLabel.Text = "Tile copied."; } });
            AddButton(tileButtons, "Paste tile", (s, e) => PasteTile());

            _layerList = new ListView { Dock = DockStyle.Top, Height = 90, View = View.Details, FullRowSelect = true, HideSelection = false };
            _layerList.Columns.Add("Blend", 50);
            _layerList.Columns.Add("z", 40);
            _layerList.Columns.Add("Land", 70);
            var layerButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            AddButton(layerButtons, "Remove layer", (s, e) => RemoveLayer());

            _staticList = new ListView { Dock = DockStyle.Top, Height = 140, View = View.Details, FullRowSelect = true, HideSelection = false, MultiSelect = false };
            _staticList.Columns.Add("Item", 70);
            _staticList.Columns.Add("z", 40);
            _staticList.Columns.Add("Hue", 60);
            _staticList.SelectedIndexChanged += (s, e) => ShowStaticImage();
            _staticList.DoubleClick += (s, e) => EditStatic();
            var staticButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            AddButton(staticButtons, "Add...", (s, e) => AddStatic());
            AddButton(staticButtons, "Edit...", (s, e) => EditStatic());
            AddButton(staticButtons, "Remove", (s, e) => RemoveStatic());

            _staticPreview = new EcPreviewPanel { Dock = DockStyle.Fill };

            var inspector = new Panel { Dock = DockStyle.Fill };
            inspector.Controls.Add(_staticPreview);
            inspector.Controls.Add(staticButtons);
            inspector.Controls.Add(_staticList);
            inspector.Controls.Add(new Label { Text = "Statics", Dock = DockStyle.Top, Height = 18 });
            inspector.Controls.Add(layerButtons);
            inspector.Controls.Add(_layerList);
            inspector.Controls.Add(new Label { Text = "Blend layers", Dock = DockStyle.Top, Height = 18 });
            inspector.Controls.Add(tileButtons);
            inspector.Controls.Add(landPanel);
            inspector.Controls.Add(_tileInfo);

            var rightSplit = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
            rightSplit.Panel1.Controls.Add(sectorScroll);
            rightSplit.Panel2.Controls.Add(inspector);
            rightSplit.HandleCreated += (s, e) => { try { rightSplit.SplitterDistance = Math.Max(300, rightSplit.Width - 260); } catch (InvalidOperationException) { } };

            var mainSplit = new SplitContainer { Dock = DockStyle.Fill };
            mainSplit.Panel1.Controls.Add(_overview);
            mainSplit.Panel2.Controls.Add(rightSplit);
            mainSplit.HandleCreated += (s, e) => { try { mainSplit.SplitterDistance = Math.Max(300, mainSplit.Width / 2 - 100); } catch (InvalidOperationException) { } };

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(mainSplit);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        private static void AddButton(Control parent, string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            parent.Controls.Add(b);
        }

        public void SetClient(EcClient client)
        {
            _client = client;
            _loaded = false;
            _facet = null;
            _facets.Clear();
            _sector = null;
            _sectorPanel.Sector = null;
            _overview.SetImage(null, 1f);
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

            _facetBox.Items.Clear();
            foreach (string file in _client.GetUopFiles().Select(Path.GetFileName).Where(EcFacet.IsFacetFile))
            {
                _facetBox.Items.Add(file);
            }
            if (_facetBox.Items.Count == 0)
            {
                _statusLabel.Text = "No facet*.uop files found.";
                return;
            }
            _facetBox.SelectedIndex = 0;
            OpenFacet();
        }

        private void OpenFacet()
        {
            if (_facetBox.SelectedItem is not string file)
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
                    package.Changed += (s, e) => _saveButton.Enabled = _facets.Values.Any(f => f.IsModified || f.Package.IsDirty);
                }
                _facet = facet;
                LoadOverview();
                _sector = null;
                _sectorPanel.Sector = null;
                _sectorPanel.Invalidate();
                _statusLabel.Text = $"{file}: map {facet.Width} x {facet.Height}, {facet.SectorsX} x {facet.SectorsY} sectors" +
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
            int level = _zoomBox.SelectedIndex;
            if (_radar != null)
            {
                DdsImage image = DdsImage.DecodeMip(_radar, level);
                float scale = (float)_facet.Width / image.Width;
                _overview.SetImage(EcImageHelper.ToBitmap(image), scale);
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
                _overview.SetImage(bitmap, scale);
            }
            if (_sectorX >= 0)
            {
                _overview.Selection = new Rectangle(_sectorX * 64, _sectorY * 64, 64, 64);
            }
        }

        private void GoTo()
        {
            string[] parts = _gotoBox.Text.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (_facet == null || parts.Length < 2 || !int.TryParse(parts[0], out int x) || !int.TryParse(parts[1], out int y))
            {
                return;
            }
            SelectSector(x / 64, y / 64, x % 64, y % 64);
            _overview.ScrollTo(x, y);
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
                _sectorPanel.Sector = sector;
                _sectorPanel.Colors = _radar != null ? DdsImage.DecodeRegion(_radar, sx * 64, sy * 64, 64, 64).Pixels : null;
                _sectorPanel.Selected = new Point(tileX, tileY);
                _sectorPanel.Invalidate();
                _overview.Selection = new Rectangle(sx * 64, sy * 64, 64, 64);
                _overview.Invalidate();
                if (sector == null)
                {
                    _statusLabel.Text = $"Sector {sx},{sy} doesn't exist in {_facet.FileName}.";
                    return;
                }
                ShowTile(tileX, tileY);
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The sector can't be read.", ex);
            }
        }

        private EcMapTile CurrentTile => _sector != null && _sectorPanel.Selected.X >= 0 ? _sector[_sectorPanel.Selected.X, _sectorPanel.Selected.Y] : null;

        private void ShowTile(int x, int y)
        {
            EcMapTile tile = _sector?[x, y];
            _layerList.Items.Clear();
            _staticList.Items.Clear();
            _staticPreview.Image = null;
            if (tile == null)
            {
                return;
            }

            _tileInfo.Text = $"Tile {(_sectorX * 64) + x}, {(_sectorY * 64) + y}  (sector {_sector.Index}: {x},{y})\r\n" +
                             $"land 0x{tile.LandId:X4}  z {tile.Z}";
            _zBox.Value = tile.Z;
            _landBox.Text = $"0x{tile.LandId:X4}";

            foreach (EcLandLayer l in tile.Layers)
            {
                var item = new ListViewItem(l.Blend.ToString()) { Tag = l };
                item.SubItems.Add(l.Z.ToString());
                item.SubItems.Add($"0x{l.LandId:X4}");
                _layerList.Items.Add(item);
            }
            foreach (EcStatic st in tile.Statics)
            {
                var item = new ListViewItem($"0x{st.ItemId:X4}") { Tag = st };
                item.SubItems.Add(st.Z.ToString());
                item.SubItems.Add(st.Hue.ToString());
                _staticList.Items.Add(item);
            }
            if (_staticList.Items.Count > 0)
            {
                _staticList.Items[0].Selected = true;
            }
        }

        private void ShowStaticImage()
        {
            _staticPreview.Image = null;
            if (_staticList.SelectedItems.Count != 1 || _client == null)
            {
                return;
            }
            var st = (EcStatic)_staticList.SelectedItems[0].Tag;
            byte[] data = _client.GetItemImage(st.ItemId, out _);
            _staticPreview.Image = data != null ? EcImageHelper.TryDecode(data, out _) : null;
        }

        private void Changed()
        {
            if (_sector == null)
            {
                return;
            }
            _facet.MarkModified(_sector);
            _saveButton.Enabled = true;
            _sectorPanel.Invalidate();
            ShowTile(_sectorPanel.Selected.X, _sectorPanel.Selected.Y);
        }

        private void ApplyLand()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || !EcDialogs.TryParseNumber(_landBox.Text, out long land) || land < 0 || land > ushort.MaxValue)
            {
                return;
            }
            tile.Z = (sbyte)_zBox.Value;
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
            if (tile == null || _layerList.SelectedItems.Count == 0)
            {
                return;
            }
            foreach (ListViewItem item in _layerList.SelectedItems)
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
            if (CurrentTile == null || _staticList.SelectedItems.Count != 1)
            {
                return;
            }
            if (AskStatic("Edit static", (EcStatic)_staticList.SelectedItems[0].Tag))
            {
                Changed();
            }
        }

        private void RemoveStatic()
        {
            EcMapTile tile = CurrentTile;
            if (tile == null || _staticList.SelectedItems.Count != 1)
            {
                return;
            }
            tile.Statics.Remove((EcStatic)_staticList.SelectedItems[0].Tag);
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
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }
    }
}

/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareMapControl : UserControl
    {
        // ─────────────────────────────────────────────────────────────────
        // Fields
        // ─────────────────────────────────────────────────────────────────
        private bool _loaded;
        private bool _moving;
        private Point _movingPoint;
        private Point _currentPoint;
        private Map _currentMap;
        private Map _originalMap;
        private int _currentMapId;
        private Bitmap _map;
        private static double _zoom = 1;

        // Block-level diff set – O(1) lookup to skip non-diff blocks in OnPaint
        private HashSet<Point> _diffBlockSet = new HashSet<Point>();
        // Tile-level diff set – computed ONCE in CalculateDiffsAsync, looked up in O(1) per pixel in OnPaint.
        // Eliminates repeated GetLandTile/GetStaticTiles calls during every paint frame.
        private HashSet<Point> _diffTileSet = new HashSet<Point>();
        private List<Point> _diffBlocks = new List<Point>();
        private int _currentDiffIndex = -1;

        // [3] Version token – discard results from superseded async runs
        private int _diffVersion;
        private bool _calculatingDiffs;

        // [4] Tooltip cache
        private string _lastDiff = string.Empty;

        // ─────────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────────
        public CompareMapControl()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint,
                true);

            pictureBox.MouseWheel += PictureBox_MouseWheel;
        }

        // ─────────────────────────────────────────────────────────────────
        // Load
        // ─────────────────────────────────────────────────────────────────
        private void OnLoad(object sender, EventArgs e)
        {
            _currentMap = Map.Custom;
            _originalMap = Map.Felucca;

            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;

            showDifferencesToolStripMenuItem.Checked = true;
            showMap1ToolStripMenuItem.Checked = true;
            showMap2ToolStripMenuItem.Checked = false;

            SetScrollBarValues();
            ChangeMapNames();
            UpdateZoomLabel();

            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            if (!_loaded)
            {
                ControlEvents.MapDiffChangeEvent += OnMapDiffChangeEvent;
                ControlEvents.MapNameChangeEvent += OnMapNameChangeEvent;
                ControlEvents.MapSizeChangeEvent += OnMapSizeChangeEvent;
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }
            _loaded = true;
        }

        // ─────────────────────────────────────────────────────────────────
        // Event relay handlers
        // ─────────────────────────────────────────────────────────────────
        private void OnMapDiffChangeEvent()
        {
            _ = CalculateDiffsAsync();
            pictureBox.Invalidate();
        }

        private void OnMapNameChangeEvent() => ChangeMapNames();
        private void OnMapSizeChangeEvent() => InternalUpdate();
        private void OnFilePathChangeEvent() => InternalUpdate();

        private void InternalUpdate()
        {
            SetScrollBarValues();
            if (_currentMap != null)
                ChangeMap();
            pictureBox.Invalidate();
        }

        private void ChangeMapNames()
        {
            if (!_loaded) { return; }
            feluccaToolStripMenuItem.Text = Options.MapNames[0];
            trammelToolStripMenuItem.Text = Options.MapNames[1];
            ilshenarToolStripMenuItem.Text = Options.MapNames[2];
            malasToolStripMenuItem.Text = Options.MapNames[3];
            tokunoToolStripMenuItem.Text = Options.MapNames[4];
            terMurToolStripMenuItem.Text = Options.MapNames[5];
        }

        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────
        private static int Round(int x) => (x >> 3) << 3;

        private void UpdateZoomLabel()
        {
            ZoomLabel.Text = "Zoom:";
            toolStripZoomTextBox.Text = _zoom.ToString("G");
        }

        // [2] O(1) block-level diff check (used for tooltip in OnMouseMove)
        private bool BlockDiff(int x, int y) => _diffBlockSet.Contains(new Point(x, y));

        // ─────────────────────────────────────────────────────────────────
        // Mouse events
        // ─────────────────────────────────────────────────────────────────
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _moving = true;
                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;
                Cursor = Cursors.Hand;
            }
            else
            {
                _moving = false;
                Cursor = Cursors.Default;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_originalMap == null) { return; }

            int worldX = Math.Min(_originalMap.Width - 1, (int)(e.X / _zoom) + Round(hScrollBar.Value));
            int worldY = Math.Min(_originalMap.Height - 1, (int)(e.Y / _zoom) + Round(vScrollBar.Value));

            CoordsLabel.Text = $"Coords: {worldX}, {worldY}";

            if (_moving)
            {
                toolTip1.RemoveAll();

                int deltaX = (int)(-(e.X - _movingPoint.X) / _zoom);
                int deltaY = (int)(-(e.Y - _movingPoint.Y) / _zoom);

                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;

                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));

                pictureBox.Invalidate();
                return;
            }

            string diff = string.Empty;

            if (_zoom >= 2 && _currentMap != null)
            {
                if (BlockDiff(worldX >> 3, worldY >> 3))
                {
                    // [4] StringBuilder – no repeated string concatenation
                    var sb = new StringBuilder();

                    Tile customTile = _currentMap.Tiles.GetLandTile(worldX, worldY);
                    Tile origTile = _originalMap.Tiles.GetLandTile(worldX, worldY);

                    if (customTile.Id != origTile.Id || customTile.Z != origTile.Z)
                    {
                        sb.AppendLine("Tile:");
                        sb.AppendLine($"0x{origTile.Id:X} {origTile.Z} → 0x{customTile.Id:X} {customTile.Z}");
                    }

                    HuedTile[] customStatics = _currentMap.Tiles.GetStaticTiles(worldX, worldY);
                    HuedTile[] origStatics = _originalMap.Tiles.GetStaticTiles(worldX, worldY);

                    if (customStatics.Length != origStatics.Length)
                    {
                        sb.AppendLine("Statics:");
                        sb.AppendLine("orig:");
                        foreach (HuedTile t in origStatics)
                            sb.AppendLine($"0x{t.Id:X} {t.Z} {t.Hue}");
                        sb.AppendLine("new:");
                        foreach (HuedTile t in customStatics)
                            sb.AppendLine($"0x{t.Id:X} {t.Z} {t.Hue}");
                    }
                    else
                    {
                        bool headerWritten = false;
                        for (int i = 0; i < customStatics.Length; i++)
                        {
                            if (customStatics[i].Id != origStatics[i].Id
                             || customStatics[i].Z != origStatics[i].Z
                             || customStatics[i].Hue != origStatics[i].Hue)
                            {
                                if (!headerWritten)
                                {
                                    sb.AppendLine("Statics diff:");
                                    headerWritten = true;
                                }
                                sb.AppendLine(
                                    $"0x{origStatics[i].Id:X} {origStatics[i].Z} {origStatics[i].Hue}"
                                  + $" → 0x{customStatics[i].Id:X} {customStatics[i].Z} {customStatics[i].Hue}");
                            }
                        }
                    }

                    diff = sb.ToString();
                }

                if (diff != _lastDiff)
                {
                    _lastDiff = diff;
                    toolTip1.SetToolTip(pictureBox, diff);
                    pictureBox.Invalidate();
                }
            }

            // Mark-Diff patch tooltip
            if ((_zoom < 2) || !markDiffToolStripMenuItem.Checked || !string.IsNullOrEmpty(diff))
                return;

            Map drawMap = showMap1ToolStripMenuItem.Checked ? _originalMap : _currentMap;
            if (drawMap == null) { return; }

            var sb2 = new StringBuilder();

            if (drawMap.Tiles.Patch.LandBlocksCount > 0
             && drawMap.Tiles.Patch.IsLandBlockPatched(worldX >> 3, worldY >> 3))
            {
                Tile patchTile = drawMap.Tiles.Patch.GetLandTile(worldX, worldY);
                Tile baseTile = drawMap.Tiles.GetLandTile(worldX, worldY, false);
                sb2.AppendLine("Tile:");
                sb2.AppendLine($"0x{baseTile.Id:X} {baseTile.Z} → 0x{patchTile.Id:X} {patchTile.Z}");
            }

            if (drawMap.Tiles.Patch.StaticBlocksCount > 0
             && drawMap.Tiles.Patch.IsStaticBlockPatched(worldX >> 3, worldY >> 3))
            {
                HuedTile[] patchStatics = drawMap.Tiles.Patch.GetStaticTiles(worldX, worldY);
                HuedTile[] baseStatics = drawMap.Tiles.GetStaticTiles(worldX, worldY, false);
                sb2.AppendLine("Statics:");
                sb2.AppendLine("orig:");
                foreach (HuedTile t in baseStatics)
                    sb2.AppendLine($"0x{t.Id:X} {t.Z} {t.Hue}");
                sb2.AppendLine("patch:");
                foreach (HuedTile t in patchStatics)
                    sb2.AppendLine($"0x{t.Id:X} {t.Z} {t.Hue}");
            }

            diff = sb2.ToString();
            if (diff != _lastDiff)
            {
                _lastDiff = diff;
                toolTip1.SetToolTip(pictureBox, diff);
                pictureBox.Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _moving = false;
            Cursor = Cursors.Default;
        }

        // ─────────────────────────────────────────────────────────────────
        // Paint
        // ─────────────────────────────────────────────────────────────────
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (!_loaded) { return; }

            // Do NOT dispose _map here – it may still be referenced by a previous frame.
            // We create a local bitmap and only assign it to _map after everything succeeds.
            Bitmap newMap;

            Map renderMap = (!showMap1ToolStripMenuItem.Checked && _currentMap != null)
                ? _currentMap
                : _originalMap;

            if (renderMap == null) { return; }

            // [7] Clamp start first, then count – never exceed map bounds
            int mapBlocksW = renderMap.Width >> 3;
            int mapBlocksH = renderMap.Height >> 3;

            int blockStartX = Math.Max(0, Math.Min(hScrollBar.Value >> 3, mapBlocksW - 1));
            int blockStartY = Math.Max(0, Math.Min(vScrollBar.Value >> 3, mapBlocksH - 1));

            int rawCountX = Math.Max(1, (int)((e.ClipRectangle.Width / _zoom) + 8) >> 3);
            int rawCountY = Math.Max(1, (int)((e.ClipRectangle.Height / _zoom) + 8) >> 3);

            int blockCountX = Math.Max(1, Math.Min(rawCountX, mapBlocksW - blockStartX));
            int blockCountY = Math.Max(1, Math.Min(rawCountY, mapBlocksH - blockStartY));

            try
            {
                newMap = renderMap.GetImage(blockStartX, blockStartY, blockCountX, blockCountY, true);
            }
            catch (ArgumentOutOfRangeException)
            {
                e.Graphics.Clear(Color.FromArgb(30, 30, 30));
                return;
            }

            // ── Diff overlay – nur die wirklich veränderten Tiles werden rot markiert ──
            if (_currentMap != null && showDifferencesToolStripMenuItem.Checked && _diffTileSet.Count > 0)
            {
                int maxx = Math.Min(blockStartX + blockCountX, mapBlocksW);
                int maxy = Math.Min(blockStartY + blockCountY, mapBlocksH);

                using (Graphics mapg = Graphics.FromImage(newMap))
                using (var pathFill = new GraphicsPath())
                using (var pathOutline = new GraphicsPath())
                {
                    int gx = 0;
                    for (int x = blockStartX; x < maxx; x++, gx += 8)
                    {
                        int gy = 0;
                        for (int y = blockStartY; y < maxy; y++, gy += 8)
                        {
                            if (!_diffBlockSet.Contains(new Point(x, y))) { continue; }

                            if (_zoom >= 1)
                            {
                                // Zoom normal/nah: nur exakt die geänderten Tiles einfärben
                                for (int xb = 0; xb < 8; xb++)
                                    for (int yb = 0; yb < 8; yb++)
                                        if (_diffTileSet.Contains(new Point(x * 8 + xb, y * 8 + yb)))
                                            pathFill.AddRectangle(new Rectangle(gx + xb, gy + yb, 1, 1));
                            }
                            else
                            {
                                // Rausgezoomt: Block-Umriss damit Diffs sichtbar bleiben
                                pathOutline.AddRectangle(new Rectangle(gx, gy, 8, 8));
                            }
                        }
                    }

                    if (pathFill.PointCount > 0)
                        mapg.FillPath(Brushes.Red, pathFill);

                    if (pathOutline.PointCount > 0)
                        using (var pen = new Pen(Color.Red, 1f))
                            mapg.DrawPath(pen, pathOutline);
                }
            }

            // ── Mark Diff (patch overlay, Azure) ──────────────────────────
            if (markDiffToolStripMenuItem.Checked)
            {
                Map drawMap = showMap1ToolStripMenuItem.Checked ? _originalMap : _currentMap;
                if (drawMap != null)
                {
                    int patchCount = drawMap.Tiles.Patch.LandBlocksCount
                                   + drawMap.Tiles.Patch.StaticBlocksCount;
                    if (patchCount > 0)
                    {
                        int maxX = Math.Min(blockStartX + blockCountX, drawMap.Width >> 3);
                        int maxY = Math.Min(blockStartY + blockCountY, drawMap.Height >> 3);

                        using (Graphics g = Graphics.FromImage(newMap))
                        using (var pFill = new GraphicsPath())
                        using (var pEdgeTop = new GraphicsPath())
                        using (var pEdgeLeft = new GraphicsPath())
                        {
                            int gx = 0;
                            for (int x = blockStartX; x < maxX; x++, gx += 8)
                            {
                                int gy = 0;
                                for (int y = blockStartY; y < maxY; y++, gy += 8)
                                {
                                    if (drawMap.Tiles.Patch.IsLandBlockPatched(x, y)
                                     || drawMap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                    {
                                        pFill.AddRectangle(new Rectangle(gx, gy, 8, 8));
                                        pEdgeTop.AddRectangle(new Rectangle(gx, 0, 8, 2));
                                        pEdgeLeft.AddRectangle(new Rectangle(0, gy, 2, 8));
                                    }
                                }
                            }
                            g.FillPath(Brushes.Azure, pFill);
                            g.FillPath(Brushes.Azure, pEdgeTop);
                            g.FillPath(Brushes.Azure, pEdgeLeft);
                        }
                    }
                }
            }

            ZoomMap(ref newMap);

            // Atomically replace _map and dispose the old one
            Bitmap oldMap = _map;
            _map = newMap;
            oldMap?.Dispose();

            e.Graphics.DrawImageUnscaledAndClipped(_map, e.ClipRectangle);
        }


        // ─────────────────────────────────────────────────────────────────
        // Zoom helpers
        // ─────────────────────────────────────────────────────────────────
        private void ZoomMap(ref Bitmap bmp0)
        {
            var bmp1 = new Bitmap((int)(bmp0.Width * _zoom), (int)(bmp0.Height * _zoom));
            using (Graphics g = Graphics.FromImage(bmp1))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(bmp0, new Rectangle(0, 0, bmp1.Width, bmp1.Height));
            }
            bmp0.Dispose();
            bmp0 = bmp1;
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (!_loaded) { return; }
            ChangeScrollBar();
            pictureBox.Invalidate();
        }

        private void ChangeScrollBar()
        {
            Map refMap = (!showMap1ToolStripMenuItem.Checked && _currentMap != null)
                ? _currentMap
                : _originalMap;

            if (refMap == null) { return; }

            int zoomExtra = (int)(_zoom >= 1 ? 40 * _zoom : 40 / _zoom);

            // [7] Direct Math.Max(0,...) – no separate clamp step needed
            hScrollBar.Maximum = Math.Max(0, Round(
                refMap.Width
                - Round((int)(pictureBox.ClientSize.Width / _zoom) - 8)
                + zoomExtra));

            vScrollBar.Maximum = Math.Max(0, Round(
                refMap.Height
                - Round((int)(pictureBox.ClientSize.Height / _zoom) - 8)
                + zoomExtra));
        }

        private void SetScrollBarValues()
        {
            hScrollBar.Minimum = 0;
            vScrollBar.Minimum = 0;
            ChangeScrollBar();
            hScrollBar.LargeChange = 40;
            hScrollBar.SmallChange = 8;
            hScrollBar.Value = 0;
            vScrollBar.LargeChange = 40;
            vScrollBar.SmallChange = 8;
            vScrollBar.Value = 0;
        }

        private void OnZoomPlus(object sender, EventArgs e)
        {
            if (_zoom < 18) { _zoom *= 2; DoZoom(); }
        }

        private void OnZoomMinus(object sender, EventArgs e)
        {
            if (_zoom > 0.25) { _zoom /= 2; DoZoom(); }
        }

        private void OnZoomTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Return) { return; }
            e.Handled = true;
            if (double.TryParse(toolStripZoomTextBox.Text,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double val))
            {
                _zoom = Math.Max(0.25, Math.Min(18, val));
                DoZoom();
            }
            else
            {
                toolStripZoomTextBox.Text = _zoom.ToString("G");
            }
        }

        private void DoZoom()
        {
            ChangeScrollBar();   // Maximum gets updated first
            UpdateZoomLabel();

            Point mouse = pictureBox.PointToClient(MousePosition);
            // Center the view on the mouse position after zoom; clamp to updated Maximum
            int x = Round(Math.Max(0, Math.Min(
                (int)(mouse.X / _zoom + hScrollBar.Value) - (int)(pictureBox.ClientSize.Width / _zoom / 2),
                hScrollBar.Maximum)));
            int y = Round(Math.Max(0, Math.Min(
                (int)(mouse.Y / _zoom + vScrollBar.Value) - (int)(pictureBox.ClientSize.Height / _zoom / 2),
                vScrollBar.Maximum)));

            hScrollBar.Value = Math.Min(x, hScrollBar.Maximum);
            vScrollBar.Value = Math.Min(y, vScrollBar.Maximum);
            pictureBox.Invalidate();
        }

        // ─────────────────────────────────────────────────────────────────
        // Context menu
        // ─────────────────────────────────────────────────────────────────
        private void OnOpeningContext(object sender, CancelEventArgs e)
        {
            _currentPoint = pictureBox.PointToClient(MousePosition);
            _currentPoint.X = (int)(_currentPoint.X / _zoom) + hScrollBar.Value;
            _currentPoint.Y = (int)(_currentPoint.Y / _zoom) + vScrollBar.Value;
        }

        // ─────────────────────────────────────────────────────────────────
        // Load / ChangeMap
        // ─────────────────────────────────────────────────────────────────
        private void OnClickBrowseLoc(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the map files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                    toolStripTextBox1.Text = dialog.SelectedPath;
            }
        }

        private void OnClickLoad(object sender, EventArgs e) => ChangeMap();

        private void ChangeMap()
        {
            SetScrollBarValues();
            string path = toolStripTextBox1.Text;

            if (Directory.Exists(path))
            {
                _currentMap = Map.Custom = new Map(
                    path,
                    _originalMap.FileIndex,
                    _currentMapId,
                    _originalMap.Width,
                    _originalMap.Height);
            }
            else
            {
                MessageBox.Show("Please select a valid directory.", "No directory",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _ = CalculateDiffsAsync();
            pictureBox.Invalidate();
        }

        // ─────────────────────────────────────────────────────────────────
        // Map selection
        // ─────────────────────────────────────────────────────────────────
        private void ResetCheckedMap()
        {
            feluccaToolStripMenuItem.Checked = false;
            trammelToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
        }

        private void OnClickChangeFelucca(object sender, EventArgs e)
        {
            if (feluccaToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            feluccaToolStripMenuItem.Checked = true;
            _originalMap = Map.Felucca; _currentMapId = 0;
            ChangeMap();
        }
        private void OnClickChangeTrammel(object sender, EventArgs e)
        {
            if (trammelToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            trammelToolStripMenuItem.Checked = true;
            _originalMap = Map.Trammel; _currentMapId = 1;
            ChangeMap();
        }
        private void OnClickChangeIlshenar(object sender, EventArgs e)
        {
            if (ilshenarToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            ilshenarToolStripMenuItem.Checked = true;
            _originalMap = Map.Ilshenar; _currentMapId = 2;
            ChangeMap();
        }
        private void OnClickChangeMalas(object sender, EventArgs e)
        {
            if (malasToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            malasToolStripMenuItem.Checked = true;
            _originalMap = Map.Malas; _currentMapId = 3;
            ChangeMap();
        }
        private void OnClickChangeTokuno(object sender, EventArgs e)
        {
            if (tokunoToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            tokunoToolStripMenuItem.Checked = true;
            _originalMap = Map.Tokuno; _currentMapId = 4;
            ChangeMap();
        }
        private void OnClickChangeTerMur(object sender, EventArgs e)
        {
            if (terMurToolStripMenuItem.Checked) { return; }
            ResetCheckedMap();
            terMurToolStripMenuItem.Checked = true;
            _originalMap = Map.TerMur; _currentMapId = 5;
            ChangeMap();
        }

        private void OnClickShowDiff(object sender, EventArgs e) => pictureBox.Invalidate();
        private void OnClickShowMap2(object sender, EventArgs e)
        {
            if (showMap2ToolStripMenuItem.Checked || _currentMap == null) { return; }
            showMap1ToolStripMenuItem.Checked = false;
            showMap2ToolStripMenuItem.Checked = true;
            pictureBox.Invalidate();
        }
        private void OnClickShowMap1(object sender, EventArgs e)
        {
            if (showMap1ToolStripMenuItem.Checked) { return; }
            showMap2ToolStripMenuItem.Checked = false;
            showMap1ToolStripMenuItem.Checked = true;
            pictureBox.Invalidate();
        }
        private void OnClickMarkDiff(object sender, EventArgs e) => pictureBox.Invalidate();

        // ─────────────────────────────────────────────────────────────────
        // [1][2][3] CalculateDiffs – async, versioned, HashSet-based
        // ─────────────────────────────────────────────────────────────────
        private async Task CalculateDiffsAsync()
        {
            if (_currentMap == null || _originalMap == null)
            {
                _diffBlockSet = new HashSet<Point>();
                _diffTileSet = new HashSet<Point>();
                _diffBlocks = new List<Point>();
                _calculatingDiffs = false;
                SetLoadingState(false);
                UpdateDiffStats(0);
                NotifyDiffListDialog();
                return;
            }

            _calculatingDiffs = true;
            SetLoadingState(true);

            // [3] Increment version – any in-flight run with an older version will discard its result
            int version = Interlocked.Increment(ref _diffVersion);

            // [1] Clamp to the smaller map so neither map is accessed out-of-range
            int width = Math.Min(_currentMap.Width, _originalMap.Width) >> 3;
            int height = Math.Min(_currentMap.Height, _originalMap.Height) >> 3;

            // Capture field refs – background threads must not access 'this' fields directly
            Map curMap = _currentMap;
            Map origMap = _originalMap;

            (HashSet<Point> blockSet, HashSet<Point> tileSet) diffSet;
            List<Point> diffBlocks;

            try
            {
                (diffSet, diffBlocks) = await Task.Run(() =>
                {
                    var blockSet = new HashSet<Point>();
                    var tileSet = new HashSet<Point>();
                    var db = new List<Point>();

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            Tile[] customTiles = curMap.Tiles.GetLandBlock(x, y);
                            Tile[] origTiles = origMap.Tiles.GetLandBlock(x, y);
                            HuedTile[][][] customStatics = curMap.Tiles.GetStaticBlock(x, y);
                            HuedTile[][][] origStatics = origMap.Tiles.GetStaticBlock(x, y);

                            bool blockDiffers = false;

                            for (int xb = 0; xb < 8; xb++)
                            {
                                for (int yb = 0; yb < 8; yb++)
                                {
                                    int idx = ((yb & 7) << 3) + (xb & 7);
                                    bool tileDiffers = false;

                                    if (customTiles[idx].Id != origTiles[idx].Id
                                     || customTiles[idx].Z != origTiles[idx].Z)
                                    {
                                        tileDiffers = true;
                                    }
                                    else if (customStatics[xb][yb].Length != origStatics[xb][yb].Length)
                                    {
                                        tileDiffers = true;
                                    }
                                    else
                                    {
                                        for (int i = 0; i < customStatics[xb][yb].Length; i++)
                                        {
                                            if (customStatics[xb][yb][i].Id != origStatics[xb][yb][i].Id
                                             || customStatics[xb][yb][i].Z != origStatics[xb][yb][i].Z
                                             || customStatics[xb][yb][i].Hue != origStatics[xb][yb][i].Hue)
                                            {
                                                tileDiffers = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (tileDiffers)
                                    {
                                        tileSet.Add(new Point(x * 8 + xb, y * 8 + yb));
                                        blockDiffers = true;
                                    }
                                }
                            }

                            if (blockDiffers)
                            {
                                var pt = new Point(x, y);
                                blockSet.Add(pt);
                                db.Add(pt);
                            }
                        }
                    }

                    return ((blockSet, tileSet), db);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CalculateDiffsAsync failed: {ex}");
                _calculatingDiffs = false;
                SetLoadingState(false);
                return;
            }

            // [3] A newer run has started – discard this result; let the newer run own the UI state
            if (version != _diffVersion) { return; }

            _calculatingDiffs = false;
            SetLoadingState(false);

            _diffBlockSet = diffSet.blockSet;
            _diffTileSet = diffSet.tileSet;
            _diffBlocks = diffBlocks;
            _currentDiffIndex = -1;

            UpdateDiffStats(_diffBlocks.Count);
            NotifyDiffListDialog();
            pictureBox.Invalidate();
        }

        private void SetLoadingState(bool loading)
        {
            toolStripProgressBar.Visible = loading;
            toolStripLabelProgress.Visible = loading;
            toolStripButton2.Enabled = !loading;

            bool hasDiffs = _diffBlocks != null && _diffBlocks.Count > 0;
            toolStripButtonNextDiff.Enabled = !loading && hasDiffs;
            toolStripButtonPrevDiff.Enabled = !loading && hasDiffs;
        }

        private void UpdateDiffStats(int count)
        {
            if (count == 0)
            {
                toolStripLabelDiffStats.Text = "Diffs: –";
                toolStripLabelDiffStats.ForeColor = Color.FromArgb(255, 180, 60);
                toolStripButtonNextDiff.Enabled = false;
                toolStripButtonPrevDiff.Enabled = false;
            }
            else
            {
                toolStripLabelDiffStats.Text = $"Diffs: {count} block{(count == 1 ? "" : "s")}";
                toolStripLabelDiffStats.ForeColor = Color.FromArgb(80, 220, 80);
                toolStripButtonNextDiff.Enabled = true;
                toolStripButtonPrevDiff.Enabled = true;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Next / Prev Diff navigation
        // ─────────────────────────────────────────────────────────────────
        private void OnClickNextDiff(object sender, EventArgs e)
        {
            if (_diffBlocks == null || _diffBlocks.Count == 0) { return; }
            _currentDiffIndex = (_currentDiffIndex + 1) % _diffBlocks.Count;
            JumpToDiffBlock(_diffBlocks[_currentDiffIndex]);
        }

        private void OnClickPrevDiff(object sender, EventArgs e)
        {
            if (_diffBlocks == null || _diffBlocks.Count == 0) { return; }
            if (--_currentDiffIndex < 0) _currentDiffIndex = _diffBlocks.Count - 1;
            JumpToDiffBlock(_diffBlocks[_currentDiffIndex]);
        }

        private void JumpToDiffBlock(Point blockPos)
        {
            int worldX = blockPos.X * 8;
            int worldY = blockPos.Y * 8;

            int targetX = Round(Math.Max(0, worldX - (int)(pictureBox.ClientSize.Width / _zoom / 2)));
            int targetY = Round(Math.Max(0, worldY - (int)(pictureBox.ClientSize.Height / _zoom / 2)));

            hScrollBar.Value = Math.Min(targetX, hScrollBar.Maximum);
            vScrollBar.Value = Math.Min(targetY, vScrollBar.Maximum);

            toolStripLabelDiffStats.Text =
                $"Diffs: {_diffBlocks.Count} blocks  [{_currentDiffIndex + 1}/{_diffBlocks.Count}]"
                + $"  @ {worldX},{worldY}";

            pictureBox.Invalidate();
        }

        // ─────────────────────────────────────────────────────────────────
        // Export diff list
        // ─────────────────────────────────────────────────────────────────
        private void OnClickExportDiff(object sender, EventArgs e)
        {
            if (_diffBlocks == null || _diffBlocks.Count == 0)
            {
                MessageBox.Show("No differences available to export.\nLoad a map first.",
                    "Export Diff", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Diff List";
                sfd.Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.FileName = "map_diff.csv";

                if (sfd.ShowDialog() != DialogResult.OK) { return; }

                try
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("BlockX,BlockY,WorldX,WorldY");
                        foreach (Point pt in _diffBlocks)
                            writer.WriteLine($"{pt.X},{pt.Y},{pt.X * 8},{pt.Y * 8}");
                    }

                    MessageBox.Show(
                        $"Exported {_diffBlocks.Count} diff-blocks to:\n{sfd.FileName}",
                        "Export complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // [6] Screenshot – InvokePaint (reliable with DoubleBuffer)
        // ─────────────────────────────────────────────────────────────────
        private void OnClickScreenshot(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Save Screenshot";
                sfd.Filter = "PNG Image (*.png)|*.png|Bitmap (*.bmp)|*.bmp";
                sfd.FileName = "map_screenshot.png";

                if (sfd.ShowDialog() != DialogResult.OK) { return; }

                try
                {
                    int w = Math.Max(1, pictureBox.ClientSize.Width);
                    int h = Math.Max(1, pictureBox.ClientSize.Height);

                    using (var bmp = new Bitmap(w, h))
                    using (var g = Graphics.FromImage(bmp))
                    {
                        // InvokePaint must be called on the owning UserControl (this),
                        // not on pictureBox directly – PictureBox inherits it as protected.
                        using (var pe = new PaintEventArgs(g, new Rectangle(0, 0, w, h)))
                        {
                            OnPaint(pictureBox, pe);
                        }

                        var fmt = sfd.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                            ? System.Drawing.Imaging.ImageFormat.Bmp
                            : System.Drawing.Imaging.ImageFormat.Png;

                        bmp.Save(sfd.FileName, fmt);
                    }

                    MessageBox.Show($"Screenshot saved:\n{sfd.FileName}",
                        "Screenshot saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Screenshot failed:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Scroll + Mouse wheel
        // ─────────────────────────────────────────────────────────────────
        private void HandleScroll(object sender, ScrollEventArgs e) => pictureBox.Invalidate();

        private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) OnZoomPlus(sender, e);
            else if (e.Delta < 0) OnZoomMinus(sender, e);
        }

        // ─────────────────────────────────────────────────────────────────
        // [9] Diff List Dialog
        // ─────────────────────────────────────────────────────────────────
        private DiffListDialog _diffListDialog;

        private void OnClickDiffList(object sender, EventArgs e)
        {
            // Reuse open dialog if still visible; otherwise create fresh
            if (_diffListDialog != null && !_diffListDialog.IsDisposed)
            {
                _diffListDialog.BringToFront();
                return;
            }

            _diffListDialog = new DiffListDialog(_diffBlocks, JumpToDiffBlock);
            _diffListDialog.Show(this);
        }

        // Called after every CalculateDiffsAsync completes – keep dialog in sync
        private void NotifyDiffListDialog()
        {
            if (_diffListDialog != null && !_diffListDialog.IsDisposed)
                _diffListDialog.RefreshList(_diffBlocks);
        }
    }

    // =========================================================================
    // DiffListDialog – schlankes Koordinatenfenster
    // =========================================================================
    internal sealed class DiffListDialog : Form
    {
        private ListBox _list;
        private Label _lblCount;
        private Button _btnJump;
        private Button _btnCopy;
        private List<Point> _blocks;
        private readonly Action<Point> _jump;

        public DiffListDialog(List<Point> diffBlocks, Action<Point> jump)
        {
            _jump = jump;

            Text = "Diff Coordinates";
            Size = new Size(340, 460);
            MinimumSize = new Size(280, 260);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            _lblCount = new Label
            {
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(180, 180, 180),
                Padding = new Padding(4, 0, 0, 0)
            };

            _list = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None
            };
            _list.DoubleClick += (s, ev) => DoJump();

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 34,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(4, 3, 4, 3),
                FlowDirection = FlowDirection.LeftToRight
            };

            _btnJump = Btn("▶ Jump");
            _btnJump.Click += (s, ev) => DoJump();

            _btnCopy = Btn("Copy All");
            _btnCopy.Click += (s, ev) => DoCopyAll();

            panel.Controls.Add(_btnJump);
            panel.Controls.Add(_btnCopy);

            Controls.Add(_list);
            Controls.Add(_lblCount);
            Controls.Add(panel);

            RefreshList(diffBlocks);
        }

        private static Button Btn(string text) => new Button
        {
            Text = text,
            Height = 26,
            Width = 90,
            BackColor = Color.FromArgb(62, 62, 66),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5f)
        };

        public void RefreshList(List<Point> diffBlocks)
        {
            if (InvokeRequired) { Invoke(new Action(() => RefreshList(diffBlocks))); return; }

            _blocks = diffBlocks;
            _list.BeginUpdate();
            _list.Items.Clear();
            if (diffBlocks != null)
            {
                foreach (Point p in diffBlocks)
                    _list.Items.Add($"Block ({p.X,5}, {p.Y,5})  →  World ({p.X * 8,6}, {p.Y * 8,6})");
            }
            _list.EndUpdate();

            int n = diffBlocks?.Count ?? 0;
            _lblCount.Text = n == 0 ? "  Keine Unterschiede." : $"  {n} Diff-Block{(n == 1 ? "" : "s")}  –  Doppelklick = Jump";
            _btnCopy.Enabled = n > 0;
            _btnJump.Enabled = n > 0;
        }

        private void DoJump()
        {
            if (_list.SelectedIndex < 0 || _blocks == null) return;
            _jump?.Invoke(_blocks[_list.SelectedIndex]);
        }

        private void DoCopyAll()
        {
            if (_blocks == null || _blocks.Count == 0) return;
            var sb = new StringBuilder();
            sb.AppendLine("BlockX\tBlockY\tWorldX\tWorldY");
            foreach (Point p in _blocks)
                sb.AppendLine($"{p.X}\t{p.Y}\t{p.X * 8}\t{p.Y * 8}");
            try
            {
                Clipboard.SetText(sb.ToString().TrimEnd());
                _lblCount.Text = $"  ✓ {_blocks.Count} Einträge kopiert!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kopieren fehlgeschlagen:\n{ex.Message}", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.MapViewer;

namespace UoFiddler.Controls.UserControls
{
    public partial class MapControl : UserControl
    {
        #region [ Region (marked area) ]
        // marked area in map coordinates (inclusive, Width/Height >= 1)
        private Rectangle _region = Rectangle.Empty;
        private int _regionMapId = -1;
        private bool _hasRegion;
        // true while the "Mark" button is active
        private bool _isDrawingRectangle;
        // true while the mouse is dragged in mark mode
        private bool _regionDragging;
        private Point _regionAnchor;

        /// <summary>
        /// Maximum size of a region that is opened in the region viewer
        /// </summary>
        public const int MaxViewerRegionSize = 512;
        #endregion

        #region [ MapControl ]
        public MapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            if (!Files.CacheData)
            {
                PreloadMap.Visible = false;
            }

            ProgressBar.Visible = false;
            _refMarker = this;
            panel1.Visible = false;

            pictureBox.MouseWheel += OnMouseWheel;
        }
        #endregion

        private static MapControl _refMarker;
        public static double Zoom = 1;

        // reused render buffer (no new bitmaps on every paint)
        private Bitmap _mapBuffer;
        private MapDefinition _currentDef;
        private bool _syncWithClient;
        private int _clientX;
        private int _clientY;
        private int _clientZ;
        private int _clientMap;
        private Point _currentPoint;
        private bool _moving;
        private Point _movingPoint;
        private bool _renderingZoom;

        // marker groups by map id
        private readonly Dictionary<int, TreeNode> _markerGroups = new Dictionary<int, TreeNode>();

        private int HScrollBar => hScrollBar.Value;
        private int VScrollBar => vScrollBar.Value;
        public Map CurrentMap { get; private set; }

        /// <summary>
        /// Definition (id, name, files) of the shown map
        /// </summary>
        public MapDefinition CurrentDefinition => _currentDef;

        private int _currentMapId => _currentDef?.Id ?? 0;

        private static bool _loaded;

        #region [ class RectangleDrawnEventArgs ]
        // Delegate definition = coordinates
        public delegate void RectangleDrawnEventHandler(object sender, RectangleDrawnEventArgs e);

        // Event definition = coordinates
        public event RectangleDrawnEventHandler RectangleDrawn;

        // For coordinates
        public class RectangleDrawnEventArgs : EventArgs
        {
            public Point StartPoint { get; set; }
            public Point EndPoint { get; set; }
        }
        #endregion

        #region [ Reload ]
        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            Zoom = 1;
            _moving = false;
            OnLoad(this, EventArgs.Empty);
        }
        #endregion

        #region [ OnLoad ]
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            MapRegistryStore.EnsureLoaded();

            if (!_loaded)
            {
                // markers are only read once, a reload must not drop markers that were added in this session
                LoadMapOverlays();
            }

            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            RebuildMapLists();
            SetCurrentMap(MapRegistry.GetById(0) ?? MapRegistry.Definitions.First(), false);
            PreloadMap.Visible = Files.CacheData && !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);

            ZoomLabel.Text = $"Zoom: {Zoom}";
            SetScrollBarValues();
            Refresh();
            pictureBox.Invalidate();
            Cursor.Current = Cursors.Default;

            if (!_loaded)
            {
                ControlEvents.MapDiffChangeEvent += OnMapDiffChangeEvent;
                ControlEvents.MapNameChangeEvent += OnMapNameChangeEvent;
                ControlEvents.MapSizeChangeEvent += OnMapSizeChangeEvent;
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                MapRegistry.Changed += OnMapRegistryChanged;
            }
            _loaded = true;
        }
        #endregion

        #region [ OnMapDiffChangeEvent ]
        private void OnMapDiffChangeEvent()
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnMapNameChangeEvent ]
        private void OnMapNameChangeEvent()
        {
            // names were changed in the options dialog
            MapRegistryStore.SyncNamesFromOptions();
            ChangeMapNames();
        }
        #endregion

        #region [ OnMapSizeChangeEvent ]
        private void OnMapSizeChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ OnFilePathChangeEvent ]
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region [ OnMapRegistryChanged ]
        private void OnMapRegistryChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            MapRegistryStore.SyncNamesToOptions();
            RebuildMapLists();

            // the shown map was removed or hidden -> back to the first map
            MapDefinition def = _currentDef == null ? null : MapRegistry.GetById(_currentDef.Id);
            if (def == null || !def.Visible)
            {
                def = MapRegistry.Definitions.FirstOrDefault(d => d.Visible) ?? MapRegistry.Definitions.First();
            }

            SetCurrentMap(def, true);
        }
        #endregion

        #region [ RefreshMap ]
        public void RefreshMap()
        {
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Drops the render cache of the current map and redraws (after map/statics were changed in memory).
        /// </summary>
        public void RefreshMapAfterEdit()
        {
            CurrentMap?.ResetCache();
            PreloadMap.Visible = Files.CacheData && CurrentMap != null && !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }
        #endregion

        #region [ ChangeMapNames / RebuildMapLists ]
        /// <summary>
        /// Changes the Names of maps
        /// </summary>
        private void ChangeMapNames()
        {
            if (!_loaded)
            {
                return;
            }

            RebuildMapLists();
        }

        /// <summary>
        /// Builds the map entries of the context menu, the "Maps" toolbar menu and the marker groups
        /// from <see cref="MapRegistry"/>.
        /// </summary>
        private void RebuildMapLists()
        {
            // context menu: map entries between toolStripSeparator2 and toolStripSeparator1
            int start = contextMenuStrip1.Items.IndexOf(toolStripSeparator2) + 1;
            while (contextMenuStrip1.Items.Count > start && contextMenuStrip1.Items[start] != toolStripSeparator1)
            {
                ToolStripItem old = contextMenuStrip1.Items[start];
                contextMenuStrip1.Items.RemoveAt(start);
                old.Dispose();
            }

            foreach (ToolStripItem item in toolStripDropDownButtonMaps.DropDownItems.Cast<ToolStripItem>().ToList())
            {
                toolStripDropDownButtonMaps.DropDownItems.Remove(item);
                item.Dispose();
            }

            int insertAt = start;
            foreach (MapDefinition def in MapRegistry.Definitions.Where(d => d.Visible))
            {
                contextMenuStrip1.Items.Insert(insertAt++, CreateMapMenuItem(def));
                toolStripDropDownButtonMaps.DropDownItems.Add(CreateMapMenuItem(def));
            }

            toolStripDropDownButtonMaps.DropDownItems.Add(new ToolStripSeparator());
            var manage = new ToolStripMenuItem("Manage Maps...", null, OnClickManageMaps)
            {
                ToolTipText = "Add, edit, hide or remove maps (also maps from other directories)"
            };
            toolStripDropDownButtonMaps.DropDownItems.Add(manage);

            UpdateMarkerGroups();
            UpdateCheckedMap();
        }

        private ToolStripMenuItem CreateMapMenuItem(MapDefinition def)
        {
            bool exists = def.FilesExist();
            var item = new ToolStripMenuItem(exists ? def.Name : $"{def.Name} (missing)")
            {
                Tag = def.Id,
                ForeColor = exists ? SystemColors.ControlText : SystemColors.GrayText,
                ToolTipText = !exists
                    ? $"map{def.FileIndex}.mul / map{def.FileIndex}LegacyMUL.uop was not found.\nAdd your own map files with \"Manage Maps...\"."
                    : def.UsesCustomPath
                        ? $"ID {def.Id}, map{def.FileIndex} ({def.Width}x{def.Height})\n{def.CustomPath}"
                        : $"ID {def.Id}, map{def.FileIndex} ({def.Width}x{def.Height})"
            };
            item.Click += OnClickChangeMap;
            return item;
        }

        private void UpdateCheckedMap()
        {
            IEnumerable<ToolStripMenuItem> items = contextMenuStrip1.Items.OfType<ToolStripMenuItem>()
                .Concat(toolStripDropDownButtonMaps.DropDownItems.OfType<ToolStripMenuItem>());

            foreach (ToolStripMenuItem item in items)
            {
                if (item.Tag is int id)
                {
                    item.Checked = _currentDef != null && id == _currentDef.Id;
                }
            }

            toolStripDropDownButtonMaps.Text = _currentDef == null ? "Maps" : $"Map: {_currentDef.Name}";
        }
        #endregion

        #region [ HandleScroll ]
        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region [ public static int Round ]
        public static int Round(int x)
        {
            return (x >> 3) << 3;
        }
        #endregion

        #region [ Map buffer ]
        private void DisposeMapBuffer()
        {
            _mapBuffer?.Dispose();
            _mapBuffer = null;
        }

        private Bitmap GetMapBuffer(int width, int height)
        {
            if (_mapBuffer == null || _mapBuffer.Width < width || _mapBuffer.Height < height)
            {
                DisposeMapBuffer();
                _mapBuffer = new Bitmap(width, height, PixelFormat.Format16bppRgb555);
            }

            return _mapBuffer;
        }
        #endregion

        #region [ Coordinates ]
        /// <summary>
        /// Converts a position of the picture box into map coordinates (same rounding as the drawn map).
        /// </summary>
        private Point ScreenToMap(Point p)
        {
            int x = (int)(p.X / Zoom) + Round(hScrollBar.Value);
            int y = (int)(p.Y / Zoom) + Round(vScrollBar.Value);
            if (CurrentMap != null)
            {
                x = Math.Max(0, Math.Min(CurrentMap.Width - 1, x));
                y = Math.Max(0, Math.Min(CurrentMap.Height - 1, y));
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Converts map coordinates into a position of the picture box.
        /// </summary>
        private PointF MapToScreen(int x, int y)
        {
            return new PointF((float)((x - Round(hScrollBar.Value)) * Zoom), (float)((y - Round(vScrollBar.Value)) * Zoom));
        }
        #endregion

        #region [ SetScrollBarValues ]
        private void SetScrollBarValues()
        {
            vScrollBar.Minimum = 0;
            hScrollBar.Minimum = 0;
            ChangeScrollBar();
            hScrollBar.LargeChange = 40;
            hScrollBar.SmallChange = 8;
            vScrollBar.LargeChange = 40;
            vScrollBar.SmallChange = 8;
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
        }
        #endregion

        #region [ ChangeScrollBar ]
        private void ChangeScrollBar()
        {
            if (PreloadWorker.IsBusy || CurrentMap == null)
            {
                return;
            }

            hScrollBar.Maximum = CurrentMap.Width;
            hScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Width / Zoom) - 8);
            if (Zoom >= 1)
            {
                hScrollBar.Maximum += (int)(40 * Zoom);
            }
            else if (Zoom < 1)
            {
                hScrollBar.Maximum += (int)(40 / Zoom);
            }

            hScrollBar.Maximum = Math.Max(0, Round(hScrollBar.Maximum));
            vScrollBar.Maximum = CurrentMap.Height;
            vScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Height / Zoom) - 8);
            if (Zoom >= 1)
            {
                vScrollBar.Maximum += (int)(40 * Zoom);
            }
            else if (Zoom < 1)
            {
                vScrollBar.Maximum += (int)(40 / Zoom);
            }

            vScrollBar.Maximum = Math.Max(0, Round(vScrollBar.Maximum));
        }
        #endregion

        #region [ OnResize ]
        private void OnResize(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (PreloadWorker.IsBusy)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            ChangeScrollBar();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ ChangeMap ]
        private void ChangeMap()
        {
            PreloadMap.Visible = Files.CacheData && !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            SetScrollBarValues();
            pictureBox.Invalidate();
        }

        private void OnClickChangeMap(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem { Tag: int id })
            {
                return;
            }

            if (_currentDef != null && _currentDef.Id == id)
            {
                return;
            }

            MapDefinition def = MapRegistry.GetById(id);
            if (def == null)
            {
                return;
            }

            // map files missing (e.g. Forell/Dragon/Intermediate World are not part of the standard client)
            if (!def.FilesExist() && !AskForMissingMap(def))
            {
                return;
            }

            SetCurrentMap(def, true);
        }

        /// <summary>
        /// Tells the user that the files of a map are missing and offers the map manager.
        /// Returns true if the (empty) map should be shown anyway.
        /// </summary>
        private bool AskForMissingMap(MapDefinition def)
        {
            string where = def.UsesCustomPath ? def.CustomPath : (string.IsNullOrEmpty(Files.RootDir) ? "the client directory" : Files.RootDir);
            string text =
                $"The files of the map \"{def.Name}\" were not found:\n" +
                $"map{def.FileIndex}.mul or map{def.FileIndex}LegacyMUL.uop (and staidx{def.FileIndex}.mul / statics{def.FileIndex}.mul)\n" +
                $"in {where}\n\n" +
                (def.IsBuiltIn && def.Id >= 6
                    ? "This map is not part of the standard Ultima Online client. You can add your own map files (also from another directory) or hide this entry.\n\n"
                    : "You can add the map files from another directory or hide this entry.\n\n") +
                "Yes = open \"Manage Maps\"\nNo = show the map anyway (empty)\nCancel = do nothing";

            DialogResult result = MessageBox.Show(text, "Map not found", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                OnClickManageMaps(this, EventArgs.Empty);
                return false;
            }

            return result == DialogResult.No;
        }

        /// <summary>
        /// Shows the given map
        /// </summary>
        public void SetCurrentMap(MapDefinition def, bool resetView)
        {
            if (def == null)
            {
                return;
            }

            _currentDef = def;
            CurrentMap = def.GetMap();
            UpdateCheckedMap();

            MessageLabel.ForeColor = Color.Red;
            MessageLabel.Text = def.FilesExist() ? string.Empty : $"map{def.FileIndex} not found!";

            if (resetView)
            {
                ChangeMap();
            }
        }

        /// <summary>
        /// Switches to the map with the given id (client or marker map id). Returns false if the map is unknown.
        /// </summary>
        private bool SwitchMap(int mapId)
        {
            MapDefinition def = MapRegistry.GetById(mapId);
            if (def == null)
            {
                return false;
            }

            SetCurrentMap(def, false);
            return true;
        }
        #endregion

        #region [ OnMouseDown ]
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (PreloadWorker.IsBusy || e.Button != MouseButtons.Left)
            {
                return;
            }

            // Check that we are in drawing mode
            if (_isDrawingRectangle)
            {
                // Save the starting point
                _regionAnchor = ScreenToMap(e.Location);
                _regionDragging = true;
                _region = new Rectangle(_regionAnchor, new Size(1, 1));
                _regionMapId = _currentMapId;
                pictureBox.Invalidate();
            }
            else
            {
                // Moving the map
                _moving = true;
                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;
                Cursor = Cursors.Hand;
            }
        }
        #endregion

        #region [ OnMouseUp ]
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            _moving = false;
            Cursor = _isDrawingRectangle ? Cursors.Cross : Cursors.Default;

            if (!_regionDragging)
            {
                return;
            }

            // Finish drawing the rectangle
            _regionDragging = false;
            UpdateRegion(ScreenToMap(e.Location));
            _hasRegion = true;

            SetMarkMode(false);
            UpdateRegionButtons();
            pictureBox.Invalidate();

            Point start = _region.Location;
            Point end = new Point(_region.Right - 1, _region.Bottom - 1);

            // Trigger the RectangleDrawn event = coordinates (start = upper left, end = lower right)
            RectangleDrawn?.Invoke(this, new RectangleDrawnEventArgs { StartPoint = start, EndPoint = end });

            MessageBox.Show($"Upper left corner: {start}\nBottom right corner: {end}\nSize: {_region.Width} x {_region.Height}",
                "Coordinates of the rectangle", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region [ OnMouseMove ]
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentMap == null)
            {
                return;
            }

            Point mapPoint = ScreenToMap(e.Location);
            CoordsLabel.Text = $"Coords: {mapPoint.X},{mapPoint.Y}";

            // Check that we are in drawing mode and the left mouse button is pressed
            if (_regionDragging && e.Button == MouseButtons.Left)
            {
                UpdateRegion(mapPoint);
                MessageLabel.ForeColor = Color.DarkBlue;
                MessageLabel.Text = $"Region: {_region.Width} x {_region.Height}";

                // Redraw PictureBox
                pictureBox.Invalidate();
                return;
            }

            // Moving the map
            if (!_moving)
            {
                return;
            }

            int deltaX = (int)(-1 * (e.X - _movingPoint.X) / Zoom);
            int deltaY = (int)(-1 * (e.Y - _movingPoint.Y) / Zoom);

            _movingPoint.X = e.X;
            _movingPoint.Y = e.Y;

            hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
            vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));

            pictureBox.Invalidate();
        }

        private void OnMouseLeaveMap(object sender, EventArgs e)
        {
            if (!_regionDragging && !_moving)
            {
                return;
            }

            _moving = false;
            Cursor = _isDrawingRectangle ? Cursors.Cross : Cursors.Default;
        }

        private void UpdateRegion(Point current)
        {
            int x1 = Math.Min(_regionAnchor.X, current.X);
            int y1 = Math.Min(_regionAnchor.Y, current.Y);
            int x2 = Math.Max(_regionAnchor.X, current.X);
            int y2 = Math.Max(_regionAnchor.Y, current.Y);
            _region = new Rectangle(x1, y1, (x2 - x1) + 1, (y2 - y1) + 1);
        }
        #endregion

        #region [ OnClick_ShowClientLoc ]
        private void OnClick_ShowClientLoc(object sender, EventArgs e)
        {
            _syncWithClient = !_syncWithClient;
        }
        #endregion

        #region [ OnClick_GotoClientLoc ]
        private void OnClick_GotoClientLoc(object sender, EventArgs e)
        {
            int x = 0;
            int y = 0;
            int z = 0;
            int mapClient = 0;
            if (!Client.Running)
            {
                return;
            }

            Client.Calibrate();
            if (!Client.FindLocation(ref x, ref y, ref z, ref mapClient))
            {
                return;
            }

            if (_currentMapId != mapClient && !SwitchMap(mapClient))
            {
                MessageLabel.Text = $"Client map {mapClient} is not in the map list";
                return;
            }

            _clientX = x;
            _clientY = y;
            _clientZ = z;
            _clientMap = mapClient;
            SetScrollBarValues();
            CenterOn(x, y);
            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{MapRegistry.GetName(mapClient)}";
        }
        #endregion

        #region [ CenterOn ]
        /// <summary>
        /// Scrolls the map so that the given position is in the center
        /// </summary>
        public void CenterOn(int x, int y)
        {
            hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, (int)(x - (pictureBox.ClientSize.Width / Zoom / 2))));
            vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, (int)(y - (pictureBox.ClientSize.Height / Zoom / 2))));
            pictureBox.Invalidate();
        }
        #endregion

        #region [ SyncClientTimer ]
        private void SyncClientTimer(object sender, EventArgs e)
        {
            if (!_syncWithClient)
            {
                return;
            }

            int x = 0;
            int y = 0;
            int z = 0;
            int mapClient = 0;
            string mapName = "";
            if (Client.Running)
            {
                Client.Calibrate();
                if (Client.FindLocation(ref x, ref y, ref z, ref mapClient))
                {
                    if (_clientX == x && _clientY == y && _clientZ == z && _clientMap == mapClient)
                    {
                        return;
                    }

                    _clientX = x;
                    _clientY = y;
                    _clientZ = z;
                    _clientMap = mapClient;
                    mapName = MapRegistry.GetName(mapClient);
                }
            }

            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{mapName}";
            pictureBox.Invalidate();
        }
        #endregion

        #region [ GetMapInfo ]
        private void GetMapInfo(object sender, EventArgs e)
        {
            new MapDetailsForm(CurrentMap, _currentPoint).Show();
        }
        #endregion

        #region [ OnOpenContext ]
        private void OnOpenContext(object sender, CancelEventArgs e)  // Save for GetMapInfo
        {
            _currentPoint = ScreenToMap(pictureBox.PointToClient(MousePosition));
        }
        #endregion

        #region [ OnContextClosed ]
        private void OnContextClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnDropDownClosed ]
        private void OnDropDownClosed(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnMouseWheel ]
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (_renderingZoom)
            {
                return;
            }

            //Needed to update current position of the cursor
            OnOpenContext(sender, null);

            //Scrolling goes up
            if (e.Delta > 0)
            {
                OnZoomPlus(sender, null);
            }
            //Scrolling goes down
            else
            {
                OnZoomMinus(sender, null);
            }
        }
        #endregion

        #region [ Zoom ]

        private const double MaxZoom = 16; // Here you can set the zoom factor  4 or 8 or 16 or 32...
        private const double MinZoom = 0.25;

        private void OnZoomMinus(object sender, EventArgs e)
        {
            if (Zoom / 2 < MinZoom)
            {
                return;
            }

            Zoom /= 2;
            DoZoom();
        }

        private void OnZoomPlus(object sender, EventArgs e)
        {
            if (Zoom * 2 > MaxZoom)
            {
                return;
            }

            Zoom *= 2;
            DoZoom();
        }

        private void DoZoom()
        {
            if (_renderingZoom)
            {
                return;
            }

            _renderingZoom = true;
            ChangeScrollBar();
            ZoomLabel.Text = $"Zoom: {Zoom}";
            int x = Math.Max(0, _currentPoint.X - ((int)(pictureBox.ClientSize.Width / Zoom) / 2));
            int y = Math.Max(0, _currentPoint.Y - ((int)(pictureBox.ClientSize.Height / Zoom) / 2));
            x = Math.Min(x, hScrollBar.Maximum);
            y = Math.Min(y, vScrollBar.Maximum);
            hScrollBar.Value = Round(x);
            vScrollBar.Value = Round(y);
            pictureBox.Invalidate();
            _renderingZoom = false;
        }
        #endregion

        #region [ OnPaint ]
        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (PreloadWorker.IsBusy)
            {
                e.Graphics.DrawString("Preloading map. Please wait...", SystemFonts.DefaultFont, Brushes.Black, 60, 60);
                return;
            }

            if (CurrentMap == null)
            {
                return;
            }

            // always render the whole visible area starting at the rounded scroll position
            // (the clip rectangle can be only a part of the picture box)
            Size client = pictureBox.ClientSize;
            int blockWidth = ((int)(client.Width / Zoom) >> 3) + 2;
            int blockHeight = ((int)(client.Height / Zoom) >> 3) + 2;

            Bitmap buffer = GetMapBuffer(blockWidth << 3, blockHeight << 3);
            try
            {
                CurrentMap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3, blockWidth, blockHeight, buffer,
                    showStaticsToolStripMenuItem1.Checked);
            }
            catch (Exception ex)
            {
                // broken or missing map files must not crash the application
                e.Graphics.Clear(Color.Black);
                e.Graphics.DrawString($"The map \"{_currentDef?.Name}\" can not be read:\n{ex.Message}\n\nCheck the files with \"Maps > Manage Maps...\".",
                    SystemFonts.DefaultFont, Brushes.White, 20, 20);
                return;
            }

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(buffer,
                new Rectangle(0, 0, (int)((blockWidth << 3) * Zoom), (int)((blockHeight << 3) * Zoom)),
                0, 0, blockWidth << 3, blockHeight << 3, GraphicsUnit.Pixel);
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Default;

            if (showCenterCrossToolStripMenuItem1.Checked)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                using (Pen pen = new Pen(brush))
                {
                    int x = Round(pictureBox.Width / 2);
                    int y = Round(pictureBox.Height / 2);

                    e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                    e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);
                }
            }

            if (showClientCrossToolStripMenuItem.Checked && Client.Running)
            {
                if (_clientX > hScrollBar.Value &&
                    _clientX < hScrollBar.Value + (client.Width / Zoom) &&
                    _clientY > vScrollBar.Value &&
                    _clientY < vScrollBar.Value + (client.Height / Zoom) &&
                    _clientMap == _currentMapId)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(180, Color.Yellow)))
                    using (Pen pen = new Pen(brush))
                    {
                        int x = (int)((_clientX - Round(hScrollBar.Value)) * Zoom);
                        int y = (int)((_clientY - Round(vScrollBar.Value)) * Zoom);

                        e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                        e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);

                        e.Graphics.DrawEllipse(pen, x - 2, y - 2, 2 * 2, 2 * 2);
                    }
                }
            }

            if (showMarkersToolStripMenuItem.Checked && _markerGroups.TryGetValue(_currentMapId, out TreeNode group))
            {
                var bounds = new Rectangle(Point.Empty, client);
                foreach (TreeNode obj in group.Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    if (o.IsVisible(bounds, _currentMapId, Round(HScrollBar), Round(VScrollBar), Zoom))
                    {
                        o.Draw(e.Graphics, Round(HScrollBar), Round(VScrollBar), Zoom, CurrentMap.Width);
                    }
                }
            }

            // marked region (stays visible until it is cleared)
            if ((_hasRegion || _regionDragging) && _regionMapId == _currentMapId)
            {
                DrawRegion(e.Graphics);
            }
        }

        private void DrawRegion(Graphics g)
        {
            PointF p1 = MapToScreen(_region.Left, _region.Top);
            PointF p2 = MapToScreen(_region.Right, _region.Bottom);
            var rect = RectangleF.FromLTRB(p1.X, p1.Y, Math.Max(p2.X, p1.X + 1), Math.Max(p2.Y, p1.Y + 1));

            using (var fill = new SolidBrush(Color.FromArgb(40, Color.Red)))
            using (var pen = new Pen(Color.Red, 2) { DashStyle = _regionDragging ? DashStyle.Dash : DashStyle.Solid })
            {
                g.FillRectangle(fill, rect);
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }

            string text = $"{_region.Width} x {_region.Height}";
            SizeF size = g.MeasureString(text, Control.DefaultFont);
            using (var back = new SolidBrush(Color.FromArgb(160, Color.Black)))
            {
                g.FillRectangle(back, rect.X, rect.Y - size.Height - 1, size.Width, size.Height);
            }

            g.DrawString(text, Control.DefaultFont, Brushes.White, rect.X, rect.Y - size.Height - 1);
        }
        #endregion

        #region [ OnKeyDownGoto ]
        private void OnKeyDownGoto(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            string line = TextBoxGoto.Text.Trim();
            if (line.Length > 0)
            {
                string[] args = line.Split(' ');
                if (args.Length != 2)
                {
                    args = line.Split(',');
                }

                if (args.Length == 2 && int.TryParse(args[0].Trim(), out int x) && int.TryParse(args[1].Trim(), out int y))
                {
                    if (x >= 0 && y >= 0 && x < CurrentMap.Width && y < CurrentMap.Height)
                    {
                        contextMenuStrip1.Close();
                        CenterOn(x, y);
                    }
                }
            }
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnClickSendClient ]
        private void OnClickSendClient(object sender, EventArgs e)
        {
            if (!Client.Running)
            {
                return;
            }

            int x = Round((int)(pictureBox.Width / Zoom / 2));
            int y = Round((int)(pictureBox.Height / Zoom / 2));
            x += Round(hScrollBar.Value);
            y += Round(vScrollBar.Value);
            SendCharTo(x, y);
        }
        #endregion

        #region [ OnClickSendClientToPos ]
        private void OnClickSendClientToPos(object sender, EventArgs e)
        {
            if (Client.Running)
            {
                SendCharTo(_currentPoint.X, _currentPoint.Y);
            }
        }
        #endregion

        #region [ SendCharTo ]
        private void SendCharTo(int x, int y)
        {
            string format = "{0} " + Options.MapArgs;
            int z = CurrentMap.Tiles.GetLandTile(x, y).Z;
            Client.SendText(string.Format(format, Options.MapCmd, x, y, z, _currentMapId, _currentDef?.Name ?? string.Empty));
        }
        #endregion

        #region [ ExtractMapBmp ]
        private void ExtractMapBmp(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Bmp);
        }
        #endregion

        #region [ ExtractMapTiff ]
        private void ExtractMapTiff(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Tiff);
        }
        #endregion

        #region [ ExtractMapJpg ]
        private void ExtractMapJpg(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Jpeg);
        }
        #endregion

        #region [ ExtractMapPng ]
        private void ExtractMapPng(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Png);
        }
        #endregion

        #region [ ExtractMapImage ]

        private void ExtractMapImage(ImageFormat imageFormat)
        {
            Cursor.Current = Cursors.WaitCursor;

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string name = MakeValidFileName(_currentDef?.Name ?? $"Map{_currentMapId}");
            string fileName = Path.Combine(Options.OutputPath, $"{name}.{fileExtension}");

            try
            {
                using (Bitmap extract = CurrentMap.GetImage(0, 0, CurrentMap.Width >> 3, CurrentMap.Height >> 3, showStaticsToolStripMenuItem1.Checked))
                {
                    if (showMarkersToolStripMenuItem.Checked && _markerGroups.TryGetValue(_currentMapId, out TreeNode group))
                    {
                        using (Graphics g = Graphics.FromImage(extract))
                        {
                            foreach (TreeNode obj in group.Nodes)
                            {
                                OverlayObject o = (OverlayObject)obj.Tag;
                                if (o.Visible)
                                {
                                    // whole map: no scroll offset and no zoom
                                    o.Draw(g, 0, 0, 1, CurrentMap.Width);
                                }
                            }
                        }
                    }

                    extract.Save(fileName, imageFormat);
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            MessageBox.Show($"Map saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private static string MakeValidFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }
        #endregion

        #region [ MapMarkerForm ]

        private MapMarkerForm _mapMarkerForm;

        private void OnClickInsertMarker(object sender, EventArgs e)
        {
            if (_mapMarkerForm?.IsDisposed == false)
            {
                return;
            }

            _mapMarkerForm = new MapMarkerForm(AddOverlay, _currentPoint.X, _currentPoint.Y, _currentMapId)
            {
                TopMost = true
            };

            // Set the MapControl instance
            _mapMarkerForm.SetMapControl(this);

            _mapMarkerForm.Show();
        }
        #endregion

        #region [ AddOverlay ]
        public static void AddOverlay(int x, int y, int mapId, Color color, string text)
        {
            if (_refMarker == null || mapId < 0)
            {
                return;
            }

            TreeNode group = _refMarker.GetOrCreateMarkerGroup(mapId);
            OverlayCursor o = new OverlayCursor(new Point(x, y), mapId, text, color);
            TreeNode node = new TreeNode(text) { Tag = o };
            group.Nodes.Add(node);
            _refMarker.pictureBox.Invalidate();
        }
        #endregion

        #region [ Marker groups ]
        private TreeNode GetOrCreateMarkerGroup(int mapId)
        {
            if (_markerGroups.TryGetValue(mapId, out TreeNode group))
            {
                return group;
            }

            group = new TreeNode(MapRegistry.GetName(mapId)) { Tag = mapId };
            _markerGroups[mapId] = group;

            // keep the groups ordered by map id
            int index = 0;
            while (index < OverlayObjectTree.Nodes.Count && OverlayObjectTree.Nodes[index].Tag is int other && other < mapId)
            {
                index++;
            }

            OverlayObjectTree.Nodes.Insert(index, group);
            return group;
        }

        private void UpdateMarkerGroups()
        {
            OverlayObjectTree.BeginUpdate();
            try
            {
                foreach (MapDefinition def in MapRegistry.Definitions)
                {
                    GetOrCreateMarkerGroup(def.Id).Text = def.Name;
                }

                // groups of removed maps are kept as long as they have markers
                foreach (KeyValuePair<int, TreeNode> pair in _markerGroups.ToList())
                {
                    if (MapRegistry.GetById(pair.Key) != null)
                    {
                        continue;
                    }

                    if (pair.Value.Nodes.Count == 0)
                    {
                        pair.Value.Remove();
                        _markerGroups.Remove(pair.Key);
                    }
                    else
                    {
                        pair.Value.Text = $"Map {pair.Key} (not in map list)";
                    }
                }
            }
            finally
            {
                OverlayObjectTree.EndUpdate();
            }

            OverlayObjectTree.Invalidate();
        }
        #endregion

        #region [ LoadMapOverlays ]
        public void LoadMapOverlays()
        {
            OverlayObjectTree.BeginUpdate();
            try
            {
                foreach (TreeNode group in _markerGroups.Values)
                {
                    foreach (TreeNode node in group.Nodes)
                    {
                        (node.Tag as IDisposable)?.Dispose();
                    }
                }

                OverlayObjectTree.Nodes.Clear();
                _markerGroups.Clear();

                AddOverlayGroups();

                string fileName = Path.Combine(Options.AppDataPath, "MapOverlays.xml");
                if (!File.Exists(fileName))
                {
                    return;
                }

                XmlDocument dom = new XmlDocument();
                dom.Load(fileName);
                XmlElement xOptions = dom["Overlays"];
                var markers = xOptions?.SelectNodes("Marker");
                if (markers == null)
                {
                    return;
                }

                foreach (XmlElement element in markers)
                {
                    if (!int.TryParse(element.GetAttribute("x"), out int x) ||
                        !int.TryParse(element.GetAttribute("y"), out int y) ||
                        !int.TryParse(element.GetAttribute("map"), out int m) ||
                        !int.TryParse(element.GetAttribute("color"), out int c) ||
                        m < 0)
                    {
                        continue;
                    }

                    string text = element.GetAttribute("text");

                    // markers of unknown maps are kept (and saved again)
                    OverlayCursor o = new OverlayCursor(new Point(x, y), m, text, Color.FromArgb(c));
                    TreeNode node = new TreeNode(text) { Tag = o };
                    GetOrCreateMarkerGroup(m).Nodes.Add(node);
                }
            }
            finally
            {
                OverlayObjectTree.EndUpdate();
            }

            if (_loaded)
            {
                UpdateMarkerGroups();
                pictureBox.Invalidate();
            }
        }
        #endregion

        #region [ AddOverlayGroups ]
        private void AddOverlayGroups()
        {
            foreach (MapDefinition def in MapRegistry.Definitions)
            {
                GetOrCreateMarkerGroup(def.Id);
            }
        }
        #endregion

        #region [ SaveMapOverlays ]
        public static void SaveMapOverlays()
        {
            if (!_loaded || _refMarker == null)
            {
                return;
            }

            string filepath = Options.AppDataPath;
            string fileName = Path.Combine(filepath, "MapOverlays.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Overlays");

            foreach (TreeNode group in _refMarker.OverlayObjectTree.Nodes)
            {
                foreach (TreeNode obj in group.Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    XmlElement elem = dom.CreateElement("Marker");
                    o.Save(elem);
                    sr.AppendChild(elem);
                }
            }

            dom.AppendChild(sr);

            // also save an empty list, otherwise removed markers come back after a restart
            dom.Save(fileName);
        }
        #endregion

        #region [ OnClickPreloadMap ]
        private void OnClickPreloadMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = CurrentMap.Width >> 3;
            ProgressBar.Step = 1;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            PreloadWorker.RunWorkerAsync(new object[] { CurrentMap, showStaticsToolStripMenuItem1.Checked });
        }
        #endregion

        #region [ PreLoadDoWork ]
        private void PreLoadDoWork(object sender, DoWorkEventArgs e)
        {
            Map workMap = (Map)((object[])e.Argument)[0];
            bool statics = (bool)((object[])e.Argument)[1];
            int width = workMap.Width >> 3;
            int height = workMap.Height >> 3;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    workMap.PreloadRenderedBlock(x, y, statics);
                }

                // one report per column (one per block floods the UI thread)
                PreloadWorker.ReportProgress(x);
            }
        }
        #endregion

        #region [ PreLoadProgressChanged ]
        private void PreLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }
        #endregion

        #region [ PreLoadCompleted ]
        private void PreLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
            PreloadMap.Visible = false;
            ChangeScrollBar();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnDoubleClickMarker ]
        private void OnDoubleClickMarker(object sender, TreeNodeMouseClickEventArgs e)
        {
            OnClickGotoMarker(this, null);
        }
        #endregion

        #region [ OnClickGotoMarker ]
        private void OnClickGotoMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            if (_currentMapId != o.DefMap && !SwitchMap(o.DefMap))
            {
                MessageLabel.Text = $"Map {o.DefMap} is not in the map list";
                return;
            }

            SetScrollBarValues();
            CenterOn(o.Loc.X, o.Loc.Y);
        }
        #endregion

        #region [ OnClickRemoveMarker ]
        private void OnClickRemoveMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            (OverlayObjectTree.SelectedNode.Tag as IDisposable)?.Dispose();
            OverlayObjectTree.SelectedNode.Remove();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnClickSwitchVisible ]
        private void OnClickSwitchVisible(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            o.Visible = !o.Visible;
            OverlayObjectTree.SelectedNode.ForeColor = !o.Visible ? Color.Red : Color.Black;

            OverlayObjectTree.Invalidate();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnChangeView ]
        private void OnChangeView(object sender, EventArgs e)
        {
            PreloadMap.Visible = Files.CacheData && !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnClickDefragStatics ]
        private void OnClickDefragStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                CurrentMap, CurrentMap.Width, CurrentMap.Height, false);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickDefragRemoveStatics ]
        private void OnClickDefragRemoveStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                CurrentMap, CurrentMap.Width, CurrentMap.Height, true);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnResizeMap ]
        private void OnResizeMap(object sender, EventArgs e)
        {
            // the zoom chosen by the user is kept, only the scroll range is updated
            if (PreloadWorker.IsBusy || !_loaded)
            {
                return;
            }

            ChangeScrollBar();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ ReWriteMap ]
        private void OnClickRewriteMap(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.RewriteMap(Options.OutputPath, CurrentMap);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Map saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickReportInvisStatics ]
        private void OnClickReportInvisStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CurrentMap.ReportInvisStatics(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickReportInvalidMapIDs ]
        private void OnClickReportInvalidMapIDs(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CurrentMap.ReportInvalidMapIDs(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region [ OnClickCopy ]

        private MapReplaceForm _showForm;
        private void OnClickCopy(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new MapReplaceForm(CurrentMap, this) // Pass 'this' as mapControl
            {
                TopMost = true
            };
            _showForm.Show();
        }
        #endregion

        #region [ OnClickInsertDiffData ]

        private MapDiffInsertForm _showFormMapDiff;
        private void OnClickInsertDiffData(object sender, EventArgs e)
        {
            if (_showFormMapDiff?.IsDisposed == false)
            {
                return;
            }

            _showFormMapDiff = new MapDiffInsertForm(CurrentMap)
            {
                TopMost = true
            };
            _showFormMapDiff.Show();
        }
        #endregion

        #region [ OnClickStaticImport ]
        private void OnClickStaticImport(object sender, EventArgs e)
        {
            string path;
            using (OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select WSC Static file to import",
                Multiselect = false,
                CheckFileExists = true
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                path = dialog.FileName;
            }

            StaticImport(path);
        }
        #endregion

        #region [ StaticImport ]
        private void StaticImport(string filename)
        {
            StaticTile newTile = new StaticTile
            {
                Id = 0xFFFF,
                Hue = 0
            };

            int blockY = 0;
            int blockX = 0;

            using (StreamReader ip = new StreamReader(filename))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                    {
                        continue;
                    }

                    try
                    {
                        if (line.StartsWith("SECTION WORLDITEM"))
                        {
                            if (newTile.Id != 0xFFFF)
                            {
                                CurrentMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
                                blockX = blockY = 0;
                            }
                            newTile = new StaticTile
                            {
                                Id = 0xFFFF,
                                Hue = 0
                            };
                        }
                        else if (line.StartsWith("ID"))
                        {
                            line = line.Remove(0, 2).Trim();
                            newTile.Id = Art.GetLegalItemId(Convert.ToUInt16(line));
                        }
                        else if (line.StartsWith("X"))
                        {
                            line = line.Remove(0, 1).Trim();
                            int x = Convert.ToInt32(line);
                            blockX = x >> 3;
                            x &= 0x7;
                            newTile.X = (byte)x;
                        }
                        else if (line.StartsWith("Y"))
                        {
                            line = line.Remove(0, 1).Trim();
                            int y = Convert.ToInt32(line);
                            blockY = y >> 3;
                            y &= 0x7;
                            newTile.Y = (byte)y;
                        }
                        else if (line.StartsWith("Z"))
                        {
                            line = line.Remove(0, 1).Trim();
                            newTile.Z = Convert.ToSByte(line);
                        }
                        else if (line.StartsWith("COLOR"))
                        {
                            line = line.Remove(0, 5).Trim();
                            newTile.Hue = Convert.ToInt16(line);
                        }
                    }
                    catch
                    {
                        // TODO: add logging?
                        // ignored
                    }
                }
            }

            if (newTile.Id != 0xFFFF)
            {
                CurrentMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
            }

            MessageBox.Show("Done", "Freeze Static", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            CurrentMap.ResetCache();
            pictureBox.Invalidate();
        }
        #endregion

        #region [ OnClickMeltStatics ]
        private MapMeltStaticsForm _showMeltStaticsForm;
        private void OnClickMeltStatics(object sender, EventArgs e)
        {
            if (_showMeltStaticsForm?.IsDisposed == false)
            {
                return;
            }

            _showMeltStaticsForm = new MapMeltStaticsForm(RefreshMap, CurrentMap)
            {
                TopMost = true
            };
            _showMeltStaticsForm.Show();
        }
        #endregion

        #region [ OnClickClearStatics ]
        private MapClearStaticsForm _showClearStaticsForm;
        private void OnClickClearStatics(object sender, EventArgs e)
        {
            if (_showClearStaticsForm?.IsDisposed == false)
            {
                return;
            }

            _showClearStaticsForm = new MapClearStaticsForm(RefreshMap, CurrentMap)
            {
                TopMost = true
            };
            _showClearStaticsForm.Show();
        }
        #endregion

        #region [ OnClickReplaceTiles ]
        private MapReplaceTilesForm _showMapReplaceTilesForm;
        private void OnClickReplaceTiles(object sender, EventArgs e)
        {
            if (_showMapReplaceTilesForm?.IsDisposed == false)
            {
                return;
            }

            _showMapReplaceTilesForm = new MapReplaceTilesForm(CurrentMap)
            {
                TopMost = true
            };
            _showMapReplaceTilesForm.Show();
        }
        #endregion

        #region [ toolStripButtonMarkRegion ]
        private void toolStripButtonMarkRegion_Click(object sender, EventArgs e)
        {
            // Switch drawing mode (the button is a toggle button)
            SetMarkMode(toolStripButtonMarkRegion.Checked);
        }

        private void SetMarkMode(bool active)
        {
            _isDrawingRectangle = active;
            toolStripButtonMarkRegion.Checked = active;
            pictureBox.Cursor = active ? Cursors.Cross : Cursors.Default;

            if (active)
            {
                MessageLabel.ForeColor = Color.DarkBlue;
                MessageLabel.Text = "Drag a rectangle on the map";
            }
            else if (!_regionDragging)
            {
                MessageLabel.ForeColor = Color.Red;
                MessageLabel.Text = string.Empty;
            }
        }

        private void UpdateRegionButtons()
        {
            toolStripButtonViewRegion.Enabled = _hasRegion;
            toolStripButtonClearRegion.Enabled = _hasRegion;
            toolStripButtonViewRegion.ToolTipText = _hasRegion
                ? $"Open region {_region.X},{_region.Y} - {_region.Right - 1},{_region.Bottom - 1} ({_region.Width} x {_region.Height}) in the region viewer"
                : "Mark a region first";
        }

        private void OnClickClearRegion(object sender, EventArgs e)
        {
            _hasRegion = false;
            _regionDragging = false;
            _region = Rectangle.Empty;
            UpdateRegionButtons();
            pictureBox.Invalidate();
        }

        /// <summary>
        /// The marked region (map coordinates) or Rectangle.Empty
        /// </summary>
        public Rectangle MarkedRegion => _hasRegion ? _region : Rectangle.Empty;
        #endregion

        #region [ Region viewer ]
        private void OnClickViewRegion(object sender, EventArgs e)
        {
            if (!_hasRegion)
            {
                MessageBox.Show("Please mark a region first.", "Region Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MapDefinition def = MapRegistry.GetById(_regionMapId) ?? _currentDef;
            OpenRegionViewer(def, _region);
        }

        private void OnClickViewAreaHere(object sender, EventArgs e)
        {
            const int size = 64;
            var area = new Rectangle(_currentPoint.X - (size / 2), _currentPoint.Y - (size / 2), size, size);
            OpenRegionViewer(_currentDef, area);
        }

        private void OpenRegionViewer(MapDefinition def, Rectangle area)
        {
            if (def == null)
            {
                return;
            }

            Map map = def.GetMap();
            area.Intersect(new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            if (area.Width > MaxViewerRegionSize || area.Height > MaxViewerRegionSize)
            {
                DialogResult result = MessageBox.Show(
                    $"The region is {area.Width} x {area.Height} tiles. Only {MaxViewerRegionSize} x {MaxViewerRegionSize} tiles (from the upper left corner) are opened.\n\nContinue?",
                    "Region Viewer", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK)
                {
                    return;
                }

                area.Width = Math.Min(area.Width, MaxViewerRegionSize);
                area.Height = Math.Min(area.Height, MaxViewerRegionSize);
            }

            var form = new MapRegionViewerForm(def, area, this);
            form.Show();
        }
        #endregion

        #region [ Manage maps ]
        private MapManagerForm _mapManagerForm;

        private void OnClickManageMaps(object sender, EventArgs e)
        {
            if (_mapManagerForm?.IsDisposed == false)
            {
                _mapManagerForm.Activate();
                return;
            }

            _mapManagerForm = new MapManagerForm();
            _mapManagerForm.Show();
        }
        #endregion

        #region [ mapCordinateToolStripMenuItem ]
        private void mapCordinateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CoordsLabel.Text))
            {
                Clipboard.SetText(CoordsLabel.Text);
                MessageBox.Show("Coordinates have been copied to the clipboard.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No coordinates available to copy.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
    }

    #region [ class OverlayObject ]
    public class OverlayObject
    {
        public virtual bool IsVisible(Rectangle bounds, int m, int hScrollBar, int vScrollBar, double zoom) { return false; }
        public virtual void Draw(Graphics g, int roundedHScrollbar, int roundedVScrollbar, double zoom, int width) { }
        public virtual void Save(XmlElement elem) { }
        public override string ToString() { return string.Empty; }

        public bool Visible { get; set; }
        public Point Loc { get; protected set; }
        public int DefMap { get; protected set; }
    }
    #endregion

    #region [ class OverlayCursor ]
    public class OverlayCursor : OverlayObject, IDisposable
    {
        private readonly string _text;
        private readonly Color _col;
        private readonly Pen _pen;
        private readonly Brush _brush;
        // one background brush per marker (a shared static brush was replaced/disposed by other markers)
        private readonly Brush _background;

        public OverlayCursor(Point location, int m, string t, Color c)
        {
            Loc = location;
            DefMap = m;
            _text = t;
            _col = c;
            Visible = true;
            _brush = new SolidBrush(_col);
            _pen = new Pen(_brush);
            _background = new SolidBrush(Color.FromArgb(100, Color.White));
        }
        #endregion

        #region [ public override bool IsVisible ]
        public override bool IsVisible(Rectangle bounds, int m, int hScrollBar, int vScrollBar, double zoom)
        {
            if (!Visible)
            {
                return false;
            }

            if (DefMap != m)
            {
                return false;
            }

            return Loc.X >= hScrollBar &&
                Loc.X < hScrollBar + (bounds.Width / zoom) &&
                Loc.Y >= vScrollBar &&
                Loc.Y < vScrollBar + (bounds.Height / zoom);
        }
        #endregion

        #region [ public override void Draw ]
        public override void Draw(Graphics g, int roundedHScrollbar, int roundedVScrollbar, double zoom, int width)
        {
            int x = (int)((Loc.X - roundedHScrollbar) * zoom);
            int y = (int)((Loc.Y - roundedVScrollbar) * zoom);
            g.DrawLine(_pen, x - 4, y, x + 4, y);
            g.DrawLine(_pen, x, y - 4, x, y + 4);
            g.DrawEllipse(_pen, x - 2, y - 2, 2 * 2, 2 * 2);
            SizeF tSize = g.MeasureString(_text, Control.DefaultFont);
            int xStr = Loc.X + tSize.Width > width ? x - (int)tSize.Width - 6 : x + 6;
            g.FillRectangle(_background, xStr, y - tSize.Height, tSize.Width, tSize.Height);
            g.DrawString(_text, Control.DefaultFont, Brushes.Black, xStr, y - tSize.Height);
        }
        #endregion

        #region [ public override void Save ]
        public override void Save(XmlElement elem)
        {
            elem.SetAttribute("x", Loc.X.ToString());
            elem.SetAttribute("y", Loc.Y.ToString());
            elem.SetAttribute("map", DefMap.ToString());
            elem.SetAttribute("color", _col.ToArgb().ToString());
            elem.SetAttribute("text", _text);
        }
        #endregion

        #region [ public override string ]
        public override string ToString()
        {
            return _text;
        }
        #endregion

        #region [ public void Dispose ]
        public void Dispose()
        {
            _pen?.Dispose();
            _brush?.Dispose();
            _background?.Dispose();
        }
        #endregion
    }
}

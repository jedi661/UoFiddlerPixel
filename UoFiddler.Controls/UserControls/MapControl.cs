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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class MapControl : UserControl
    {
        // starting point of the rectangle
        private Point rectStartPoint;
        private Point rectEndPoint;
        // the rectrangle that is drawn
        private Rectangle rect = Rectangle.Empty;
        // Declaration of the isDrawingRectangle variable
        private bool isDrawingRectangle = false;

        #region MapControl
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

            //maps
            Options.MapNames = new string[] { "Map.Felucca", "Map.Trammel", "Map.Ilshenar", "Map.Malas", "Map.Tokuno", "Map.TerMur", "Map.Forell" }; // Namen festlegen für neue Karten.
        
        
        }
        #endregion

        private static MapControl _refMarker;
        public static double Zoom = 1;

        private Bitmap _map;
        private int _currentMapId;
        private bool _syncWithClient;
        private int _clientX;
        private int _clientY;
        private int _clientZ;
        private int _clientMap;
        private Point _currentPoint;
        private bool _moving;
        private Point _movingPoint;
        private bool _renderingZoom;

        private int HScrollBar => hScrollBar.Value;
        private int VScrollBar => vScrollBar.Value;
        public Map CurrentMap { get; private set; }

        private static bool _loaded;

        //OnResizeMap
        private bool isResizing = false;
        private int targetZoomLevel = 0;
        private const int ZoomTimerInterval = 10;

        #region RectangleDrawnEventArgs = coordinates
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

        #region Reload
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

        #region OnLoad
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            LoadMapOverlays();
            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            CurrentMap = Map.Felucca;
            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            forellToolStripMenuItem.Checked = false; //new Map Forell
            PreloadMap.Visible = true;
            ChangeMapNames();
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
            }
            _loaded = true;
        }
        #endregion

        #region OnMapDiffChangeEvent
        private void OnMapDiffChangeEvent()
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region OnMapNameChangeEvent
        private void OnMapNameChangeEvent()
        {
            ChangeMapNames();
        }
        #endregion

        #region OnMapSizeChangeEvent
        private void OnMapSizeChangeEvent()
        {
            Reload();
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region RefreshMap
        public void RefreshMap()
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region ChangeMapNames
        /// <summary>
        /// Changes the Names of maps
        /// </summary>        
        private void ChangeMapNames()
        {
            if (!_loaded)
            {
                return;
            }

            // Create lists of ToolStripMenuItems and nodes that correspond to the cards
            ToolStripMenuItem[] mapMenuItems = { feluccaToolStripMenuItem, trammelToolStripMenuItem, ilshenarToolStripMenuItem, malasToolStripMenuItem, tokunoToolStripMenuItem, terMurToolStripMenuItem, forellToolStripMenuItem };
            TreeNode[] mapNodes = { OverlayObjectTree.Nodes[0], OverlayObjectTree.Nodes[1], OverlayObjectTree.Nodes[2], OverlayObjectTree.Nodes[3], OverlayObjectTree.Nodes[4], OverlayObjectTree.Nodes[5], OverlayObjectTree.Nodes[6] };

            // Update the texts of the ToolStripMenuItems and nodes
            for (int i = 0; i < Options.MapNames.Length; i++)
            {
                mapMenuItems[i].Text = Options.MapNames[i];
                mapNodes[i].Text = Options.MapNames[i];
            }

            OverlayObjectTree.Invalidate();
        }
        #endregion

        #region HandleScroll
        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region public static int Round
        public static int Round(int x)
        {
            return (x >> 3) << 3;
        }
        #endregion

        #region ZoomMap
        private void ZoomMap(ref Bitmap bmp0)
        {
            Bitmap bmp1 = new Bitmap((int)(_map.Width * Zoom), (int)(_map.Height * Zoom));
            Graphics graph = Graphics.FromImage(bmp1);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            graph.PixelOffsetMode = PixelOffsetMode.Half;
            graph.DrawImage(bmp0, new Rectangle(0, 0, bmp1.Width, bmp1.Height));
            graph.Dispose();
            bmp0 = bmp1;
        }
        #endregion

        #region SetScrollBarValues
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

        #region ChangeScrollBar
        private void ChangeScrollBar()
        {
            if (PreloadWorker.IsBusy)
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

        #region OnResize
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

        #region ChangeMap
        private void ChangeMap()
        {
            PreloadMap.Visible = !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            SetScrollBarValues();
            pictureBox.Invalidate();
        }
        #endregion

        #region ResetCheckedMap
        private void ResetCheckedMap()
        {
            feluccaToolStripMenuItem.Checked = false;
            trammelToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
            forellToolStripMenuItem.Checked = false;
        }
        #endregion

        #region ChangeMapFelucca
        private void ChangeMapFelucca(object sender, EventArgs e)
        {
            if (feluccaToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            feluccaToolStripMenuItem.Checked = true;
            CurrentMap = Map.Felucca;
            _currentMapId = 0;
            ChangeMap();
        }
        #endregion

        #region ChangeMapTrammel
        private void ChangeMapTrammel(object sender, EventArgs e)
        {
            if (trammelToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            trammelToolStripMenuItem.Checked = true;
            CurrentMap = Map.Trammel;
            _currentMapId = 1;
            ChangeMap();
        }
        #endregion

        #region ChangeMapIlshenar
        private void ChangeMapIlshenar(object sender, EventArgs e)
        {
            if (ilshenarToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            ilshenarToolStripMenuItem.Checked = true;
            CurrentMap = Map.Ilshenar;
            _currentMapId = 2;
            ChangeMap();
        }
        #endregion

        #region ChangeMapMalas
        private void ChangeMapMalas(object sender, EventArgs e)
        {
            if (malasToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            malasToolStripMenuItem.Checked = true;
            CurrentMap = Map.Malas;
            _currentMapId = 3;
            ChangeMap();
        }
        #endregion

        #region ChangeMapTokuno
        private void ChangeMapTokuno(object sender, EventArgs e)
        {
            if (tokunoToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            tokunoToolStripMenuItem.Checked = true;
            CurrentMap = Map.Tokuno;
            _currentMapId = 4;
            ChangeMap();
        }
        #endregion

        #region ChangeMapTerMur
        private void ChangeMapTerMur(object sender, EventArgs e)
        {
            if (terMurToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            terMurToolStripMenuItem.Checked = true;
            CurrentMap = Map.TerMur;
            _currentMapId = 5;
            ChangeMap();
        }
        #endregion

        #region ChangeMapForell
        private void ChangeMapForell(object sender, EventArgs e) //New Map
        {
            if (forellToolStripMenuItem.Checked)
            {
                return;
            }
            ResetCheckedMap();
            forellToolStripMenuItem.Checked = true;
            CurrentMap = Map.Forell;
            _currentMapId = 6;
            ChangeMap();
        }
        #endregion

        #region OnMouseDown
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            // Check that we are in drawing mode
            if (isDrawingRectangle)
            {
                // Save the starting point                              
                rectStartPoint = new Point((int)((e.X / Zoom) + hScrollBar.Value), (int)((e.Y / Zoom) + vScrollBar.Value));
                rect.Location = e.Location; // Set rect.Location to the position of the mouse click relative to the Picture

            }
            else
            {
                // Moving the map
                if (PreloadWorker.IsBusy)
                {
                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    _moving = true;
                    _movingPoint.X = e.X;
                    _movingPoint.Y = e.Y;
                    Cursor = Cursors.Hand;
                }
            }
        }
        #endregion

        #region OnMouseUp
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            _moving = false;
            Cursor = Cursors.Default;

            // Check that we are in drawing mode
            if (isDrawingRectangle)
            {
                // Finish drawing the rectangle and display a MessageBox with the coordinates
                rectEndPoint = new Point((int)((e.X / Zoom) + hScrollBar.Value), (int)((e.Y / Zoom) + vScrollBar.Value));
                isDrawingRectangle = false;

                // Trigger the RectangleDrawn event = coordinates
                RectangleDrawn?.Invoke(this, new RectangleDrawnEventArgs { StartPoint = rectStartPoint, EndPoint = rectEndPoint });

                MessageBox.Show($"Upper left corner: {rectStartPoint}\nBottom right corner: {rectEndPoint}", "Coordinates of the rectangle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region OnMouseMove
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // Check that we are in drawing mode and the left mouse button is pressed
            if (isDrawingRectangle && e.Button == MouseButtons.Left)
            {
                // Update the endpoint
                //rectEndPoint = new Point((int)(e.X / Zoom) + Round(hScrollBar.Value), (int)(e.Y / Zoom) + Round(vScrollBar.Value));
                rectEndPoint = new Point((int)((e.X / Zoom) + hScrollBar.Value), (int)((e.Y / Zoom) + vScrollBar.Value));

                // Update the size and position of the rectangle
                int x = Math.Min(rectStartPoint.X, rectEndPoint.X) - hScrollBar.Value;
                int y = Math.Min(rectStartPoint.Y, rectEndPoint.Y) - vScrollBar.Value;
                int width = Math.Abs(rectStartPoint.X - rectEndPoint.X);
                int height = Math.Abs(rectStartPoint.Y - rectEndPoint.Y);
                rect = new Rectangle(x, y, width, height);

                // Redraw PictureBox
                pictureBox.Invalidate();
            }
            else
            {
                // Moving the map

                int xDelta = Math.Min(CurrentMap.Width, (int)(e.X / Zoom) + Round(hScrollBar.Value));
                int yDelta = Math.Min(CurrentMap.Height, (int)(e.Y / Zoom) + Round(vScrollBar.Value));

                CoordsLabel.Text = $"Coords: {xDelta},{yDelta}";

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
        }
        #endregion

        #region OnClick_ShowClientLoc
        private void OnClick_ShowClientLoc(object sender, EventArgs e)
        {
            _syncWithClient = !_syncWithClient;
        }
        #endregion

        #region OnClick_GotoClientLoc
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

            if (_currentMapId != mapClient)
            {
                ResetCheckedMap();
                SwitchMap(mapClient);
                _currentMapId = mapClient;
            }
            _clientX = x;
            _clientY = y;
            _clientZ = z;
            _clientMap = mapClient;
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, x - (pictureBox.Right / Zoom / 2));
            vScrollBar.Value = (int)Math.Max(0, y - (pictureBox.Bottom / Zoom / 2));
            pictureBox.Invalidate();
            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{Options.MapNames[mapClient]}";
        }
        #endregion

        #region SwitchMap        
        private void SwitchMap(int mapId)
        {
            // Create lists of ToolStripMenuItems and cards that correspond to the cards
            ToolStripMenuItem[] mapMenuItems = { feluccaToolStripMenuItem, trammelToolStripMenuItem, ilshenarToolStripMenuItem, malasToolStripMenuItem, tokunoToolStripMenuItem, terMurToolStripMenuItem, forellToolStripMenuItem };
            Map[] maps = { Map.Felucca, Map.Trammel, Map.Ilshenar, Map.Malas, Map.Tokuno, Map.TerMur, Map.Forell };

            // Make sure the mapId is in the valid range
            if (mapId >= 0 && mapId < mapMenuItems.Length)
            {
                mapMenuItems[mapId].Checked = true;
                CurrentMap = maps[mapId];
            }
        }
        #endregion

        #region SyncClientTimer
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
                    mapName = Options.MapNames[mapClient];
                }
            }

            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{mapName}";
            pictureBox.Invalidate();
        }
        #endregion

        #region  GetMapInfo
        private void GetMapInfo(object sender, EventArgs e)
        {
            new MapDetailsForm(CurrentMap, _currentPoint).Show();
        }
        #endregion

        #region OnOpenContext
        private void OnOpenContext(object sender, CancelEventArgs e)  // Save for GetMapInfo
        {
            _currentPoint = pictureBox.PointToClient(MousePosition);
            _currentPoint.X = (int)(_currentPoint.X / Zoom);
            _currentPoint.Y = (int)(_currentPoint.Y / Zoom);
            _currentPoint.X += Round(hScrollBar.Value);
            _currentPoint.Y += Round(vScrollBar.Value);
        }
        #endregion

        #region OnContextClosed
        private void OnContextClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region OnDropDownClosed
        private void OnDropDownClosed(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }
        #endregion

        #region OnMouseWheel
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

        #region Zoom

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

        #region OnPaint
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

            _map = CurrentMap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3,
                (int)((e.ClipRectangle.Width / Zoom) + 8) >> 3, (int)((e.ClipRectangle.Height / Zoom) + 8) >> 3,
                showStaticsToolStripMenuItem1.Checked);
            ZoomMap(ref _map);
            e.Graphics.DrawImageUnscaledAndClipped(_map, e.ClipRectangle);

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
                    _clientX < hScrollBar.Value + (e.ClipRectangle.Width / Zoom) &&
                    _clientY > vScrollBar.Value &&
                    _clientY < vScrollBar.Value + (e.ClipRectangle.Height / Zoom) &&
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

            if (OverlayObjectTree.Nodes.Count <= 0 || !showMarkersToolStripMenuItem.Checked)
            {
                return;
            }

            if (_currentMapId < OverlayObjectTree.Nodes.Count)
            {
                foreach (TreeNode obj in OverlayObjectTree.Nodes[_currentMapId].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    if (o.IsVisible(e.ClipRectangle, _currentMapId, HScrollBar, VScrollBar, Zoom))
                    {
                        o.Draw(e.Graphics, Round(HScrollBar), Round(VScrollBar), Zoom, CurrentMap.Width);
                    }
                }
            }


            // Check if we should draw a rectangle
            if (isDrawingRectangle)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.ScaleTransform((float)Zoom, (float)Zoom);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }
        #endregion

        #region OnKeyDownGoto
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

                if (args.Length == 2 && int.TryParse(args[0], out int x) && int.TryParse(args[1], out int y))
                {
                    if (x >= 0 && y >= 0 && x <= CurrentMap.Width && x <= CurrentMap.Height)
                    {
                        contextMenuStrip1.Close();
                        hScrollBar.Value = (int)Math.Max(0, x - (pictureBox.Right / Zoom / 2));
                        vScrollBar.Value = (int)Math.Max(0, y - (pictureBox.Bottom / Zoom / 2));
                    }
                }
            }
            pictureBox.Invalidate();
        }
        #endregion

        #region OnClickSendClient
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

        #region OnClickSendClientToPos
        private void OnClickSendClientToPos(object sender, EventArgs e)
        {
            if (Client.Running)
            {
                SendCharTo(_currentPoint.X, _currentPoint.Y);
            }
        }
        #endregion

        #region SendCharTo
        private void SendCharTo(int x, int y)
        {
            string format = "{0} " + Options.MapArgs;
            int z = CurrentMap.Tiles.GetLandTile(x, y).Z;
            Client.SendText(string.Format(format, Options.MapCmd, x, y, z, _currentMapId, Options.MapNames[_currentMapId]));
        }
        #endregion

        #region ExtractMapBmp
        private void ExtractMapBmp(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Bmp);
        }
        #endregion

        #region ExtractMapTiff
        private void ExtractMapTiff(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Tiff);
        }
        #endregion

        #region ExtractMapJpg
        private void ExtractMapJpg(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Jpeg);
        }
        #endregion

        #region ExtractMapPng
        private void ExtractMapPng(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Png);
        }
        #endregion

        #region ExtractMapImage       

        private void ExtractMapImage(ImageFormat imageFormat)
        {
            Cursor.Current = Cursors.WaitCursor;

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            // Verify that Options.MapNames has enough elements before attempting to access it.
            if (_currentMapId < Options.MapNames.Length)
            {
                string fileName = Path.Combine(Options.OutputPath, $"{Options.MapNames[_currentMapId]}.{fileExtension}");

                try
                {
                    Bitmap extract = CurrentMap.GetImage(0, 0, CurrentMap.Width >> 3, CurrentMap.Height >> 3, showStaticsToolStripMenuItem1.Checked);

                    if (showMarkersToolStripMenuItem.Checked)
                    {
                        Graphics g = Graphics.FromImage(extract);
                        // Check that OverlayObjectTree.Nodes has enough nodes before attempting to access it.
                        if (_currentMapId < OverlayObjectTree.Nodes.Count)
                        {
                            foreach (TreeNode obj in OverlayObjectTree.Nodes[_currentMapId].Nodes)
                            {
                                OverlayObject o = (OverlayObject)obj.Tag;
                                if (o.Visible)
                                {
                                    o.Draw(g, Round(HScrollBar), Round(VScrollBar), Zoom, CurrentMap.Width);
                                }
                            }
                        }
                        g.Save();
                    }
                    extract.Save(fileName, imageFormat);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                MessageBox.Show($"Map saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region MapMarkerForm
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

        #region AddOverlay
        public static void AddOverlay(int x, int y, int mapId, Color color, string text)
        {
            // If the node for the map doesn't already exist, create it
            while (_refMarker.OverlayObjectTree.Nodes.Count <= mapId)
            {
                TreeNode newNode = new TreeNode(Options.MapNames[_refMarker.OverlayObjectTree.Nodes.Count])
                {
                    Tag = _refMarker.OverlayObjectTree.Nodes.Count
                };
                _refMarker.OverlayObjectTree.Nodes.Add(newNode);
            }

            OverlayCursor o = new OverlayCursor(new Point(x, y), mapId, text, color);
            TreeNode node = new TreeNode(text) { Tag = o };
            _refMarker.OverlayObjectTree.Nodes[mapId].Nodes.Add(node);
            _refMarker.pictureBox.Invalidate();            
        }
        #endregion

        #region LoadMapOverlays
        public void LoadMapOverlays()
        {
            OverlayObjectTree.BeginUpdate();
            try
            {
                OverlayObjectTree.Nodes.Clear();

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
                    int x = int.Parse(element.GetAttribute("x"));
                    int y = int.Parse(element.GetAttribute("y"));
                    int m = int.Parse(element.GetAttribute("map"));
                    int c = int.Parse(element.GetAttribute("color"));
                    string text = element.GetAttribute("text");

                    if (m < OverlayObjectTree.Nodes.Count)
                    {
                        OverlayCursor o = new OverlayCursor(new Point(x, y), m, text, Color.FromArgb(c));
                        TreeNode node = new TreeNode(text) { Tag = o };
                        OverlayObjectTree.Nodes[m].Nodes.Add(node);
                    }
                }
            }
            finally
            {
                OverlayObjectTree.EndUpdate();
            }
        }
        #endregion

        #region AddOverlayGroups        
        private void AddOverlayGroups()
        {
            for (int i = 0; i < Options.MapNames.Length; i++)
            {
                TreeNode node = new TreeNode(Options.MapNames[i])
                {
                    Tag = i
                };
                OverlayObjectTree.Nodes.Add(node);
            }
        }
        #endregion  

        #region SaveMapOverlays
        public static void SaveMapOverlays()
        {
            if (!_loaded)
            {
                return;
            }

            string filepath = Options.AppDataPath;
            string fileName = Path.Combine(filepath, "MapOverlays.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Overlays");
            bool entries = false;

            // Increase the upper limit of the loop to include the new card
            for (int i = 0; i < _refMarker.OverlayObjectTree.Nodes.Count; ++i)
            {
                // Check that _refMarker.OverlayObjectTree.Nodes has enough nodes before attempting to access it.
                if (i < _refMarker.OverlayObjectTree.Nodes.Count)
                {
                    foreach (TreeNode obj in _refMarker.OverlayObjectTree.Nodes[i].Nodes)
                    {
                        OverlayObject o = (OverlayObject)obj.Tag;
                        XmlElement elem = dom.CreateElement("Marker");
                        o.Save(elem);
                        sr.AppendChild(elem);
                        entries = true;
                    }
                }
            }

            dom.AppendChild(sr);

            if (entries)
            {
                dom.Save(fileName);
            }
        }
        #endregion

        #region OnClickPreloadMap
        private void OnClickPreloadMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = (CurrentMap.Width >> 3) * (CurrentMap.Height >> 3);
            ProgressBar.Step = 1;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            PreloadWorker.RunWorkerAsync(new object[] { CurrentMap, showStaticsToolStripMenuItem1.Checked });
        }
        #endregion

        #region PreLoadDoWork
        private void PreLoadDoWork(object sender, DoWorkEventArgs e)
        {
            //Ultima.Map workmap = (Ultima.Map)((object[])e.Argument)[0]; // TODO: unused variable?
            bool statics = (bool)((object[])e.Argument)[1];
            int width = CurrentMap.Width >> 3;
            int height = CurrentMap.Height >> 3;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    CurrentMap.PreloadRenderedBlock(x, y, statics);
                    PreloadWorker.ReportProgress(1);
                }
            }
        }
        #endregion

        #region PreLoadProgressChanged
        private void PreLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }
        #endregion

        #region PreLoadCompleted
        private void PreLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
            PreloadMap.Visible = false;
            pictureBox.Invalidate();
        }
        #endregion

        #region OnDoubleClickMarker
        private void OnDoubleClickMarker(object sender, TreeNodeMouseClickEventArgs e)
        {
            OnClickGotoMarker(this, null);
        }
        #endregion

        #region OnClickGotoMarker
        private void OnClickGotoMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            if (_currentMapId != o.DefMap)
            {
                ResetCheckedMap();
                SwitchMap(o.DefMap);
                _currentMapId = o.DefMap;
            }
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, o.Loc.X - (pictureBox.Right / Zoom / 2));
            vScrollBar.Value = (int)Math.Max(0, o.Loc.Y - (pictureBox.Bottom / Zoom / 2));
            pictureBox.Invalidate();
        }
        #endregion

        #region OnClickRemoveMarker
        private void OnClickRemoveMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObjectTree.SelectedNode.Remove();
            pictureBox.Invalidate();
        }
        #endregion

        #region OnClickSwitchVisible
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

        #region OnChangeView
        private void OnChangeView(object sender, EventArgs e)
        {
            PreloadMap.Visible = !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }
        #endregion

        #region OnClickDefragStatics
        private void OnClickDefragStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                CurrentMap, CurrentMap.Width, CurrentMap.Height, false);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickDefragRemoveStatics
        private void OnClickDefragRemoveStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                CurrentMap, CurrentMap.Width, CurrentMap.Height, true);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnResizeMap
        private void OnResizeMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            // Make sure that the method is not called by multiple events at the same time
            if (isResizing)
            {
                return;
            }

            isResizing = true;

            // Calculate the target zoom level based on the size of the PictureBox
            var targetWidth = pictureBox.Width;
            var targetZoom = 1;

            while (targetWidth > CurrentMap.Width)
            {
                targetZoom++;
                targetWidth >>= 1;
            }

            targetZoomLevel = targetZoom;

            // Start a timer to gradually zoom
            Timer zoomTimer = new Timer();
            zoomTimer.Interval = ZoomTimerInterval;
            zoomTimer.Tick += PerformZoomStep;
            zoomTimer.Start();
        }
        private void PerformZoomStep(object sender, EventArgs e)
        {
            // Zoom gradually until the desired zoom level is reached
            if (Zoom < targetZoomLevel)
            {
                Zoom++;
            }
            else if (Zoom > targetZoomLevel)
            {
                Zoom--;
            }
            else
            {
                // When the desired zoom level is reached, stop the timer
                Timer zoomTimer = (Timer)sender;
                try
                {
                    zoomTimer.Stop();
                    isResizing = false;

                    // Update the ScrollBars and the PictureBox
                    ChangeScrollBar();
                    pictureBox.Invalidate();
                }
                finally
                {
                    zoomTimer.Dispose();
                }
            }
        }
        #endregion

        #region ReWriteMap
        private void OnClickRewriteMap(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.RewriteMap(Options.OutputPath,
                _currentMapId, CurrentMap.Width, CurrentMap.Height);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Map saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickReportInvisStatics
        private void OnClickReportInvisStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CurrentMap.ReportInvisStatics(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickReportInvalidMapIDs
        private void OnClickReportInvalidMapIDs(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            CurrentMap.ReportInvalidMapIDs(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        private MapReplaceForm _showForm;

        #region OnClickCopy
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

        private MapDiffInsertForm _showFormMapDiff;

        #region OnClickInsertDiffData
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

        #region OnClickStaticImport
        private void OnClickStaticImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select WSC Static file to import",
                Multiselect = false,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }

            string path = dialog.FileName;
            dialog.Dispose();
            StaticImport(path);
        }
        #endregion

        #region StaticImport
        private void StaticImport(string filename)
        {
            StreamReader ip = new StreamReader(filename);

            string line;
            StaticTile newTile = new StaticTile
            {
                Id = 0xFFFF,
                Hue = 0
            };

            int blockY = 0;
            int blockX = 0;
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
                        line = line.Remove(0, 2);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Id = Art.GetLegalItemId(Convert.ToUInt16(line));
                    }
                    else if (line.StartsWith("X"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        int x = Convert.ToInt32(line);
                        blockX = x >> 3;
                        x &= 0x7;
                        newTile.X = (byte)x;
                    }
                    else if (line.StartsWith("Y"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        int y = Convert.ToInt32(line);
                        blockY = y >> 3;
                        y &= 0x7;
                        newTile.Y = (byte)y;
                    }
                    else if (line.StartsWith("Z"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Z = Convert.ToSByte(line);
                    }
                    else if (line.StartsWith("COLOR"))
                    {
                        line = line.Remove(0, 5);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Hue = Convert.ToInt16(line);
                    }
                }
                catch
                {
                    // TODO: add logging?
                    // ignored
                }
            }
            if (newTile.Id != 0xFFFF)
            {
                CurrentMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
            }

            ip.Close();

            MessageBox.Show("Done", "Freeze Static", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            CurrentMap.ResetCache();
            pictureBox.Invalidate();
        }
        #endregion

        private MapMeltStaticsForm _showMeltStaticsForm;

        #region OnClickMeltStatics
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

        private MapClearStaticsForm _showClearStaticsForm;

        #region OnClickClearStatics
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

        private MapReplaceTilesForm _showMapReplaceTilesForm;

        #region OnClickReplaceTiles
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

        #region toolStripButtonMarkRegion
        private void toolStripButtonMarkRegion_Click(object sender, EventArgs e)
        {
            // Switch drawing mode
            isDrawingRectangle = !isDrawingRectangle;
        }
        #endregion

        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            // Überprüfen Sie, ob der Mausklick innerhalb des Rechtecks erfolgt
            /*if (rect.Contains(e.Location))
            {
                // Berechnen Sie die Koordinaten der oberen linken und unteren rechten Ecke
                Point topLeft = new Point(rectStartPoint.X, rectStartPoint.Y);
                Point bottomRight = new Point(rectEndPoint.X, rectEndPoint.Y);

                // Zeigen Sie eine MessageBox mit den Koordinaten an
                MessageBox.Show($"Obere linke Ecke: {topLeft}\nUntere rechte Ecke: {bottomRight}", "Koordinaten des Rechtecks", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }*/
        }
    }

    #region class OverlayObject
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

    #region class OverlayCursor
    public class OverlayCursor : OverlayObject, IDisposable
    {
        private readonly string _text;
        private readonly Color _col;
        private readonly Pen _pen;
        private readonly Brush _brush;
        private static Brush _background;

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

        #region public override bool IsVisible
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

            return Loc.X > hScrollBar &&
                Loc.X < hScrollBar + (bounds.Width / zoom) &&
                Loc.Y > vScrollBar &&
                Loc.Y < vScrollBar + (bounds.Height / zoom);
        }
        #endregion

        #region public override void Draw
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

        #region public override void Save
        public override void Save(XmlElement elem)
        {
            elem.SetAttribute("x", Loc.X.ToString());
            elem.SetAttribute("y", Loc.Y.ToString());
            elem.SetAttribute("map", DefMap.ToString());
            elem.SetAttribute("color", _col.ToArgb().ToString());
            elem.SetAttribute("text", _text);
        }
        #endregion

        #region public override string
        public override string ToString()
        {
            return _text;
        }
        #endregion

        #region public void Dispose
        public void Dispose()
        {
            _pen?.Dispose();
            _brush?.Dispose();
            _background?.Dispose();
        }
        #endregion
    }
}

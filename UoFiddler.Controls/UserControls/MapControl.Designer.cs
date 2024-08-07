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

namespace UoFiddler.Controls.UserControls
{
    partial class MapControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            statusStrip = new System.Windows.Forms.StatusStrip();
            CoordsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ClientLocLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ZoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
            pictureBox = new System.Windows.Forms.PictureBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            zoomToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            getMapInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertMarkerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            TextBoxGoto = new System.Windows.Forms.ToolStripTextBox();
            sendClientToPosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            feluccaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            trammelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ilshenarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            malasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tokunoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            terMurToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            forellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dragonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            intermediateWorldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            extractMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mapCordinateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hScrollBar = new System.Windows.Forms.HScrollBar();
            vScrollBar = new System.Windows.Forms.VScrollBar();
            timer1 = new System.Windows.Forms.Timer(components);
            PreloadWorker = new System.ComponentModel.BackgroundWorker();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            gotoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            switchVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panel1 = new System.Windows.Forms.Panel();
            OverlayObjectTree = new System.Windows.Forms.TreeView();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            showStaticsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            showCenterCrossToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            showMarkersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showClientCrossToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            showClientLocToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            gotoClientLocToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            sendClientToCenterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            PreloadMap = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            defragStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            defragAndRemoveDuplicatesStToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            meltStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearStaticsinMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reportStaticsUnderMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            rewriteMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertDiffDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            replaceTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripButtonMarkRegion = new System.Windows.Forms.ToolStripButton();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            collapsibleSplitter2 = new CollapsibleSplitter();
            collapsibleSplitter1 = new CollapsibleSplitter();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            contextMenuStrip1.SuspendLayout();
            contextMenuStrip2.SuspendLayout();
            panel1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { CoordsLabel, ClientLocLabel, ZoomLabel });
            statusStrip.Location = new System.Drawing.Point(0, 356);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStrip.Size = new System.Drawing.Size(736, 33);
            statusStrip.TabIndex = 0;
            statusStrip.Text = "statusStrip1";
            // 
            // CoordsLabel
            // 
            CoordsLabel.AutoSize = false;
            CoordsLabel.Name = "CoordsLabel";
            CoordsLabel.Size = new System.Drawing.Size(120, 28);
            CoordsLabel.Text = "Coords: 0,0";
            CoordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ClientLocLabel
            // 
            ClientLocLabel.AutoSize = false;
            ClientLocLabel.Name = "ClientLocLabel";
            ClientLocLabel.Size = new System.Drawing.Size(200, 28);
            ClientLocLabel.Text = "ClientLoc: 0,0";
            ClientLocLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ZoomLabel
            // 
            ZoomLabel.AutoSize = false;
            ZoomLabel.Name = "ZoomLabel";
            ZoomLabel.Size = new System.Drawing.Size(100, 28);
            ZoomLabel.Text = "Zoom: ";
            ZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox
            // 
            pictureBox.ContextMenuStrip = contextMenuStrip1;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(0, 36);
            pictureBox.Margin = new System.Windows.Forms.Padding(0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(478, 303);
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            pictureBox.Paint += OnPaint;
            pictureBox.MouseClick += pictureBox_MouseClick;
            pictureBox.MouseDown += OnMouseDown;
            pictureBox.MouseMove += OnMouseMove;
            pictureBox.MouseUp += OnMouseUp;
            pictureBox.Resize += OnResizeMap;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { zoomToolStripMenuItem, zoomToolStripMenuItem1, getMapInfoToolStripMenuItem, insertMarkerToolStripMenuItem, toolStripSeparator4, gotoToolStripMenuItem, sendClientToPosToolStripMenuItem, mapCordinateToolStripMenuItem, toolStripSeparator2, feluccaToolStripMenuItem, trammelToolStripMenuItem, ilshenarToolStripMenuItem, malasToolStripMenuItem, tokunoToolStripMenuItem, terMurToolStripMenuItem, forellToolStripMenuItem, dragonToolStripMenuItem, intermediateWorldToolStripMenuItem, toolStripSeparator1, extractMapToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(209, 418);
            contextMenuStrip1.Closed += OnContextClosed;
            contextMenuStrip1.Opening += OnOpenContext;
            // 
            // zoomToolStripMenuItem
            // 
            zoomToolStripMenuItem.Image = Properties.Resources.zoomplus;
            zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            zoomToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            zoomToolStripMenuItem.Text = "+Zoom";
            zoomToolStripMenuItem.Click += OnZoomPlus;
            // 
            // zoomToolStripMenuItem1
            // 
            zoomToolStripMenuItem1.Image = Properties.Resources.zoomminus;
            zoomToolStripMenuItem1.Name = "zoomToolStripMenuItem1";
            zoomToolStripMenuItem1.Size = new System.Drawing.Size(208, 22);
            zoomToolStripMenuItem1.Text = "-Zoom";
            zoomToolStripMenuItem1.Click += OnZoomMinus;
            // 
            // getMapInfoToolStripMenuItem
            // 
            getMapInfoToolStripMenuItem.Image = Properties.Resources.Map;
            getMapInfoToolStripMenuItem.Name = "getMapInfoToolStripMenuItem";
            getMapInfoToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            getMapInfoToolStripMenuItem.Text = "GetMapInfo";
            getMapInfoToolStripMenuItem.ToolTipText = "\"Provides information about the map, including the tiles, textures, and statics at each position.";
            getMapInfoToolStripMenuItem.Click += GetMapInfo;
            // 
            // insertMarkerToolStripMenuItem
            // 
            insertMarkerToolStripMenuItem.Name = "insertMarkerToolStripMenuItem";
            insertMarkerToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            insertMarkerToolStripMenuItem.Text = "Insert Marker";
            insertMarkerToolStripMenuItem.Click += OnClickInsertMarker;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(205, 6);
            // 
            // gotoToolStripMenuItem
            // 
            gotoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { TextBoxGoto });
            gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
            gotoToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            gotoToolStripMenuItem.Text = "Goto...";
            gotoToolStripMenuItem.DropDownClosed += OnDropDownClosed;
            // 
            // TextBoxGoto
            // 
            TextBoxGoto.Name = "TextBoxGoto";
            TextBoxGoto.Size = new System.Drawing.Size(100, 23);
            TextBoxGoto.KeyDown += OnKeyDownGoto;
            // 
            // sendClientToPosToolStripMenuItem
            // 
            sendClientToPosToolStripMenuItem.Name = "sendClientToPosToolStripMenuItem";
            sendClientToPosToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            sendClientToPosToolStripMenuItem.Text = "Send Client To Pos";
            sendClientToPosToolStripMenuItem.Click += OnClickSendClientToPos;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(205, 6);
            // 
            // feluccaToolStripMenuItem
            // 
            feluccaToolStripMenuItem.Name = "feluccaToolStripMenuItem";
            feluccaToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            feluccaToolStripMenuItem.Text = "Felucca";
            feluccaToolStripMenuItem.Click += ChangeMapFelucca;
            // 
            // trammelToolStripMenuItem
            // 
            trammelToolStripMenuItem.Name = "trammelToolStripMenuItem";
            trammelToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            trammelToolStripMenuItem.Text = "Trammel";
            trammelToolStripMenuItem.Click += ChangeMapTrammel;
            // 
            // ilshenarToolStripMenuItem
            // 
            ilshenarToolStripMenuItem.Name = "ilshenarToolStripMenuItem";
            ilshenarToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            ilshenarToolStripMenuItem.Text = "Ilshenar";
            ilshenarToolStripMenuItem.Click += ChangeMapIlshenar;
            // 
            // malasToolStripMenuItem
            // 
            malasToolStripMenuItem.Name = "malasToolStripMenuItem";
            malasToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            malasToolStripMenuItem.Text = "Malas";
            malasToolStripMenuItem.Click += ChangeMapMalas;
            // 
            // tokunoToolStripMenuItem
            // 
            tokunoToolStripMenuItem.Name = "tokunoToolStripMenuItem";
            tokunoToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            tokunoToolStripMenuItem.Text = "Tokuno";
            tokunoToolStripMenuItem.Click += ChangeMapTokuno;
            // 
            // terMurToolStripMenuItem
            // 
            terMurToolStripMenuItem.Name = "terMurToolStripMenuItem";
            terMurToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            terMurToolStripMenuItem.Text = "TerMur";
            terMurToolStripMenuItem.Click += ChangeMapTerMur;
            // 
            // forellToolStripMenuItem
            // 
            forellToolStripMenuItem.Name = "forellToolStripMenuItem";
            forellToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            forellToolStripMenuItem.Tag = "6";
            forellToolStripMenuItem.Text = "Forell";
            forellToolStripMenuItem.Click += ChangeMapForell;
            // 
            // dragonToolStripMenuItem
            // 
            dragonToolStripMenuItem.Name = "dragonToolStripMenuItem";
            dragonToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            dragonToolStripMenuItem.Tag = "7";
            dragonToolStripMenuItem.Text = "Dragon";
            dragonToolStripMenuItem.Click += ChangeMapDragon;
            // 
            // intermediateWorldToolStripMenuItem
            // 
            intermediateWorldToolStripMenuItem.Name = "intermediateWorldToolStripMenuItem";
            intermediateWorldToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            intermediateWorldToolStripMenuItem.Tag = "8";
            intermediateWorldToolStripMenuItem.Text = "Intermediate World";
            intermediateWorldToolStripMenuItem.Click += ChangeIntermediateWorld;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(205, 6);
            // 
            // extractMapToolStripMenuItem
            // 
            extractMapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            extractMapToolStripMenuItem.Image = Properties.Resources.tokuno_map;
            extractMapToolStripMenuItem.Name = "extractMapToolStripMenuItem";
            extractMapToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            extractMapToolStripMenuItem.Text = "Extract Map..";
            extractMapToolStripMenuItem.DropDownClosed += OnDropDownClosed;
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += ExtractMapBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += ExtractMapTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += ExtractMapJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += ExtractMapPng;
            // 
            // mapCordinateToolStripMenuItem
            // 
            mapCordinateToolStripMenuItem.Name = "mapCordinateToolStripMenuItem";
            mapCordinateToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            mapCordinateToolStripMenuItem.Text = "Map Cordinate Clipboard";
            mapCordinateToolStripMenuItem.Click += mapCordinateToolStripMenuItem_Click;
            // 
            // hScrollBar
            // 
            hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            hScrollBar.Location = new System.Drawing.Point(0, 339);
            hScrollBar.Name = "hScrollBar";
            hScrollBar.Size = new System.Drawing.Size(478, 17);
            hScrollBar.TabIndex = 2;
            hScrollBar.Scroll += HandleScroll;
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            vScrollBar.Location = new System.Drawing.Point(478, 36);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new System.Drawing.Size(17, 320);
            vScrollBar.TabIndex = 3;
            vScrollBar.Scroll += HandleScroll;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 2000;
            timer1.Tick += SyncClientTimer;
            // 
            // PreloadWorker
            // 
            PreloadWorker.WorkerReportsProgress = true;
            PreloadWorker.DoWork += PreLoadDoWork;
            PreloadWorker.ProgressChanged += PreLoadProgressChanged;
            PreloadWorker.RunWorkerCompleted += PreLoadCompleted;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { gotoToolStripMenuItem1, removeToolStripMenuItem, switchVisibleToolStripMenuItem });
            contextMenuStrip2.Name = "contextMenuStrip2";
            contextMenuStrip2.Size = new System.Drawing.Size(157, 70);
            contextMenuStrip2.Closed += OnContextClosed;
            // 
            // gotoToolStripMenuItem1
            // 
            gotoToolStripMenuItem1.Name = "gotoToolStripMenuItem1";
            gotoToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            gotoToolStripMenuItem1.Text = "Goto";
            gotoToolStripMenuItem1.Click += OnClickGotoMarker;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemoveMarker;
            // 
            // switchVisibleToolStripMenuItem
            // 
            switchVisibleToolStripMenuItem.Name = "switchVisibleToolStripMenuItem";
            switchVisibleToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            switchVisibleToolStripMenuItem.Text = "Switch Visibility";
            switchVisibleToolStripMenuItem.Click += OnClickSwitchVisible;
            // 
            // panel1
            // 
            panel1.Controls.Add(OverlayObjectTree);
            panel1.Dock = System.Windows.Forms.DockStyle.Right;
            panel1.Location = new System.Drawing.Point(503, 36);
            panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(233, 320);
            panel1.TabIndex = 5;
            // 
            // OverlayObjectTree
            // 
            OverlayObjectTree.ContextMenuStrip = contextMenuStrip2;
            OverlayObjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            OverlayObjectTree.Location = new System.Drawing.Point(0, 0);
            OverlayObjectTree.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OverlayObjectTree.Name = "OverlayObjectTree";
            OverlayObjectTree.Size = new System.Drawing.Size(233, 320);
            OverlayObjectTree.TabIndex = 5;
            OverlayObjectTree.NodeMouseDoubleClick += OnDoubleClickMarker;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripDropDownButton1, toolStripDropDownButton2, ProgressBar, PreloadMap, toolStripDropDownButton3, toolStripButtonMarkRegion });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(736, 28);
            toolStrip1.TabIndex = 7;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showStaticsToolStripMenuItem1, showCenterCrossToolStripMenuItem1, showMarkersToolStripMenuItem, showClientCrossToolStripMenuItem });
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(45, 25);
            toolStripDropDownButton1.Text = "View";
            toolStripDropDownButton1.DropDownClosed += OnDropDownClosed;
            // 
            // showStaticsToolStripMenuItem1
            // 
            showStaticsToolStripMenuItem1.Checked = true;
            showStaticsToolStripMenuItem1.CheckOnClick = true;
            showStaticsToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            showStaticsToolStripMenuItem1.Name = "showStaticsToolStripMenuItem1";
            showStaticsToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            showStaticsToolStripMenuItem1.Text = "Show Statics";
            showStaticsToolStripMenuItem1.Click += OnChangeView;
            // 
            // showCenterCrossToolStripMenuItem1
            // 
            showCenterCrossToolStripMenuItem1.Checked = true;
            showCenterCrossToolStripMenuItem1.CheckOnClick = true;
            showCenterCrossToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            showCenterCrossToolStripMenuItem1.Name = "showCenterCrossToolStripMenuItem1";
            showCenterCrossToolStripMenuItem1.Size = new System.Drawing.Size(173, 22);
            showCenterCrossToolStripMenuItem1.Text = "Show Center Cross";
            showCenterCrossToolStripMenuItem1.Click += OnChangeView;
            // 
            // showMarkersToolStripMenuItem
            // 
            showMarkersToolStripMenuItem.Checked = true;
            showMarkersToolStripMenuItem.CheckOnClick = true;
            showMarkersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            showMarkersToolStripMenuItem.Name = "showMarkersToolStripMenuItem";
            showMarkersToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            showMarkersToolStripMenuItem.Text = "Show Markers";
            showMarkersToolStripMenuItem.Click += OnChangeView;
            // 
            // showClientCrossToolStripMenuItem
            // 
            showClientCrossToolStripMenuItem.Checked = true;
            showClientCrossToolStripMenuItem.CheckOnClick = true;
            showClientCrossToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            showClientCrossToolStripMenuItem.Name = "showClientCrossToolStripMenuItem";
            showClientCrossToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            showClientCrossToolStripMenuItem.Text = "Show Client Cross";
            // 
            // toolStripDropDownButton2
            // 
            toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { showClientLocToolStripMenuItem1, toolStripSeparator5, gotoClientLocToolStripMenuItem1, sendClientToCenterToolStripMenuItem });
            toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            toolStripDropDownButton2.Size = new System.Drawing.Size(94, 25);
            toolStripDropDownButton2.Text = "Client Interact";
            toolStripDropDownButton2.DropDownClosed += OnDropDownClosed;
            // 
            // showClientLocToolStripMenuItem1
            // 
            showClientLocToolStripMenuItem1.CheckOnClick = true;
            showClientLocToolStripMenuItem1.Name = "showClientLocToolStripMenuItem1";
            showClientLocToolStripMenuItem1.Size = new System.Drawing.Size(186, 22);
            showClientLocToolStripMenuItem1.Text = "Show Client Loc";
            showClientLocToolStripMenuItem1.Click += OnClick_ShowClientLoc;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(183, 6);
            // 
            // gotoClientLocToolStripMenuItem1
            // 
            gotoClientLocToolStripMenuItem1.Name = "gotoClientLocToolStripMenuItem1";
            gotoClientLocToolStripMenuItem1.Size = new System.Drawing.Size(186, 22);
            gotoClientLocToolStripMenuItem1.Text = "Goto Client Loc";
            gotoClientLocToolStripMenuItem1.Click += OnClick_GotoClientLoc;
            // 
            // sendClientToCenterToolStripMenuItem
            // 
            sendClientToCenterToolStripMenuItem.Name = "sendClientToCenterToolStripMenuItem";
            sendClientToCenterToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            sendClientToCenterToolStripMenuItem.Text = "Send Client to Center";
            sendClientToCenterToolStripMenuItem.Click += OnClickSendClient;
            // 
            // ProgressBar
            // 
            ProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(117, 25);
            // 
            // PreloadMap
            // 
            PreloadMap.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            PreloadMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            PreloadMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            PreloadMap.Name = "PreloadMap";
            PreloadMap.Size = new System.Drawing.Size(78, 25);
            PreloadMap.Text = "Preload Map";
            PreloadMap.Click += OnClickPreloadMap;
            // 
            // toolStripDropDownButton3
            // 
            toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { defragStaticsToolStripMenuItem, defragAndRemoveDuplicatesStToolStripMenuItem, importStaticsToolStripMenuItem, meltStaticsToolStripMenuItem, clearStaticsinMemoryToolStripMenuItem, reportStaticsUnderMapToolStripMenuItem, toolStripMenuItem1, toolStripSeparator3, rewriteMapToolStripMenuItem, toolStripSeparator6, copyToolStripMenuItem, insertDiffDataToolStripMenuItem, toolStripSeparator7, replaceTilesToolStripMenuItem });
            toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            toolStripDropDownButton3.Size = new System.Drawing.Size(45, 25);
            toolStripDropDownButton3.Text = "Misc";
            toolStripDropDownButton3.DropDownClosed += OnDropDownClosed;
            // 
            // defragStaticsToolStripMenuItem
            // 
            defragStaticsToolStripMenuItem.Name = "defragStaticsToolStripMenuItem";
            defragStaticsToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            defragStaticsToolStripMenuItem.Text = "Defrag Statics";
            defragStaticsToolStripMenuItem.Click += OnClickDefragStatics;
            // 
            // defragAndRemoveDuplicatesStToolStripMenuItem
            // 
            defragAndRemoveDuplicatesStToolStripMenuItem.Name = "defragAndRemoveDuplicatesStToolStripMenuItem";
            defragAndRemoveDuplicatesStToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            defragAndRemoveDuplicatesStToolStripMenuItem.Text = "Defrag and Remove Duplicates Statics";
            defragAndRemoveDuplicatesStToolStripMenuItem.Click += OnClickDefragRemoveStatics;
            // 
            // importStaticsToolStripMenuItem
            // 
            importStaticsToolStripMenuItem.Name = "importStaticsToolStripMenuItem";
            importStaticsToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            importStaticsToolStripMenuItem.Text = "Freeze Statics.. (in Memory)";
            importStaticsToolStripMenuItem.Click += OnClickStaticImport;
            // 
            // meltStaticsToolStripMenuItem
            // 
            meltStaticsToolStripMenuItem.Name = "meltStaticsToolStripMenuItem";
            meltStaticsToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            meltStaticsToolStripMenuItem.Text = "Melt Statics.. (in Memory)";
            meltStaticsToolStripMenuItem.ToolTipText = "Clears a block of statics from memory. Also generates an Export File of the items removed.";
            meltStaticsToolStripMenuItem.Click += OnClickMeltStatics;
            // 
            // clearStaticsinMemoryToolStripMenuItem
            // 
            clearStaticsinMemoryToolStripMenuItem.Name = "clearStaticsinMemoryToolStripMenuItem";
            clearStaticsinMemoryToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            clearStaticsinMemoryToolStripMenuItem.Text = "Clear Statics..(in Memory)";
            clearStaticsinMemoryToolStripMenuItem.ToolTipText = "Clears a block of statics from memory. Unlike the Melt Statics, this does not create an export file of the static items removed.";
            clearStaticsinMemoryToolStripMenuItem.Click += OnClickClearStatics;
            // 
            // reportStaticsUnderMapToolStripMenuItem
            // 
            reportStaticsUnderMapToolStripMenuItem.Name = "reportStaticsUnderMapToolStripMenuItem";
            reportStaticsUnderMapToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            reportStaticsUnderMapToolStripMenuItem.Text = "Report Statics below Map (possible invisible)";
            reportStaticsUnderMapToolStripMenuItem.Click += OnClickReportInvisStatics;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(308, 22);
            toolStripMenuItem1.Text = "Report Invalid Map IDs";
            toolStripMenuItem1.Click += OnClickReportInvalidMapIDs;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(305, 6);
            // 
            // rewriteMapToolStripMenuItem
            // 
            rewriteMapToolStripMenuItem.Image = Properties.Resources.iishenar_map;
            rewriteMapToolStripMenuItem.Name = "rewriteMapToolStripMenuItem";
            rewriteMapToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            rewriteMapToolStripMenuItem.Text = "Rewrite Map";
            rewriteMapToolStripMenuItem.ToolTipText = "Creates a new map from an old map with the corresponding size.";
            rewriteMapToolStripMenuItem.Click += OnClickRewriteMap;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(305, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = Properties.Resources.Zeichnen;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            copyToolStripMenuItem.Text = "Map and Statics Copy...";
            copyToolStripMenuItem.Click += OnClickCopy;
            // 
            // insertDiffDataToolStripMenuItem
            // 
            insertDiffDataToolStripMenuItem.Name = "insertDiffDataToolStripMenuItem";
            insertDiffDataToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            insertDiffDataToolStripMenuItem.Text = "Diff to Map Copy...";
            insertDiffDataToolStripMenuItem.Click += OnClickInsertDiffData;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(305, 6);
            // 
            // replaceTilesToolStripMenuItem
            // 
            replaceTilesToolStripMenuItem.Image = Properties.Resources.Map;
            replaceTilesToolStripMenuItem.Name = "replaceTilesToolStripMenuItem";
            replaceTilesToolStripMenuItem.Size = new System.Drawing.Size(308, 22);
            replaceTilesToolStripMenuItem.Text = "Replace Tiles..";
            replaceTilesToolStripMenuItem.Click += OnClickReplaceTiles;
            // 
            // toolStripButtonMarkRegion
            // 
            toolStripButtonMarkRegion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonMarkRegion.Image = Properties.Resources.mapcordinate;
            toolStripButtonMarkRegion.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonMarkRegion.Name = "toolStripButtonMarkRegion";
            toolStripButtonMarkRegion.Size = new System.Drawing.Size(23, 25);
            toolStripButtonMarkRegion.Text = "Mark";
            toolStripButtonMarkRegion.ToolTipText = "Marks the area with coordinates";
            toolStripButtonMarkRegion.Click += toolStripButtonMarkRegion_Click;
            // 
            // collapsibleSplitter2
            // 
            collapsibleSplitter2.AnimationDelay = 20;
            collapsibleSplitter2.AnimationStep = 20;
            collapsibleSplitter2.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            collapsibleSplitter2.ControlToHide = panel1;
            collapsibleSplitter2.Dock = System.Windows.Forms.DockStyle.Right;
            collapsibleSplitter2.ExpandParentForm = false;
            collapsibleSplitter2.Location = new System.Drawing.Point(495, 36);
            collapsibleSplitter2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            collapsibleSplitter2.Name = "collapsibleSplitter2";
            collapsibleSplitter2.Size = new System.Drawing.Size(8, 320);
            collapsibleSplitter2.TabIndex = 8;
            collapsibleSplitter2.TabStop = false;
            toolTip1.SetToolTip(collapsibleSplitter2, "Click to Show/Hide Marker list");
            collapsibleSplitter2.UseAnimations = true;
            collapsibleSplitter2.VisualStyle = VisualStyles.DoubleDots;
            // 
            // collapsibleSplitter1
            // 
            collapsibleSplitter1.AnimationDelay = 20;
            collapsibleSplitter1.AnimationStep = 20;
            collapsibleSplitter1.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            collapsibleSplitter1.ControlToHide = toolStrip1;
            collapsibleSplitter1.Dock = System.Windows.Forms.DockStyle.Top;
            collapsibleSplitter1.ExpandParentForm = false;
            collapsibleSplitter1.Location = new System.Drawing.Point(0, 28);
            collapsibleSplitter1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            collapsibleSplitter1.Name = "collapsibleSplitter1";
            collapsibleSplitter1.Size = new System.Drawing.Size(736, 8);
            collapsibleSplitter1.TabIndex = 6;
            collapsibleSplitter1.TabStop = false;
            toolTip1.SetToolTip(collapsibleSplitter1, "Click To Show/Hide Toolbar");
            collapsibleSplitter1.UseAnimations = false;
            collapsibleSplitter1.VisualStyle = VisualStyles.DoubleDots;
            // 
            // MapControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(pictureBox);
            Controls.Add(hScrollBar);
            Controls.Add(vScrollBar);
            Controls.Add(collapsibleSplitter2);
            Controls.Add(panel1);
            Controls.Add(collapsibleSplitter1);
            Controls.Add(toolStrip1);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(0);
            Name = "MapControl";
            Size = new System.Drawing.Size(736, 389);
            Load += OnLoad;
            SizeChanged += OnResize;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            contextMenuStrip2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearStaticsinMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel ClientLocLabel;
        private UoFiddler.Controls.UserControls.CollapsibleSplitter collapsibleSplitter1;
        private UoFiddler.Controls.UserControls.CollapsibleSplitter collapsibleSplitter2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripStatusLabel CoordsLabel;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defragAndRemoveDuplicatesStToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defragStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem feluccaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem getMapInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoClientLocToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem1;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.ToolStripMenuItem ilshenarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertDiffDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertMarkerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem malasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meltStaticsToolStripMenuItem;
        private System.Windows.Forms.TreeView OverlayObjectTree;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStripButton PreloadMap;
        private System.ComponentModel.BackgroundWorker PreloadWorker;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportStaticsUnderMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rewriteMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendClientToCenterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendClientToPosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCenterCrossToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showClientCrossToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showClientLocToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showMarkersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showStaticsToolStripMenuItem1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripMenuItem switchVisibleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem terMurToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox TextBoxGoto;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem tokunoToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem trammelToolStripMenuItem;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ToolStripStatusLabel ZoomLabel;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton toolStripButtonMarkRegion;
        private System.Windows.Forms.ToolStripMenuItem forellToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dragonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem intermediateWorldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapCordinateToolStripMenuItem;
    }
}

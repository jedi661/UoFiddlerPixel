namespace UoFiddler.Controls.MapViewer
{
    partial class MapRegionViewerForm
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
            if (disposing)
            {
                components?.Dispose();
                DisposeResources();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            toolStripMain = new System.Windows.Forms.ToolStrip();
            toolStripButtonHelp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFit = new System.Windows.Forms.ToolStripButton();
            toolStripLabelZoom = new System.Windows.Forms.ToolStripLabel();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonLand = new System.Windows.Forms.ToolStripButton();
            toolStripButtonStatics = new System.Windows.Forms.ToolStripButton();
            toolStripButtonStretch = new System.Windows.Forms.ToolStripButton();
            toolStripButtonHeight = new System.Windows.Forms.ToolStripButton();
            toolStripButtonGrid = new System.Windows.Forms.ToolStripButton();
            toolStripButtonIssueOverlay = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonCheck = new System.Windows.Forms.ToolStripButton();
            toolStripButtonFixAll = new System.Windows.Forms.ToolStripButton();
            toolStripButtonUndo = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonReload = new System.Windows.Forms.ToolStripButton();
            toolStripButtonShowOnMap = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            isoView = new IsoViewControl();
            contextMenuView = new System.Windows.Forms.ContextMenuStrip(components);
            markErrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyCoordinatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            showOnMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tabControlSide = new System.Windows.Forms.TabControl();
            tabPageTile = new System.Windows.Forms.TabPage();
            listViewTile = new System.Windows.Forms.ListView();
            columnTileKind = new System.Windows.Forms.ColumnHeader();
            columnTileId = new System.Windows.Forms.ColumnHeader();
            columnTileName = new System.Windows.Forms.ColumnHeader();
            columnTileZ = new System.Windows.Forms.ColumnHeader();
            columnTileHeight = new System.Windows.Forms.ColumnHeader();
            columnTileHue = new System.Windows.Forms.ColumnHeader();
            columnTileFlags = new System.Windows.Forms.ColumnHeader();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            labelCellHeader = new System.Windows.Forms.Label();
            flowPanelTileButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonRemoveStatic = new System.Windows.Forms.Button();
            buttonZUp = new System.Windows.Forms.Button();
            buttonZDown = new System.Windows.Forms.Button();
            buttonMarkIssue = new System.Windows.Forms.Button();
            tabPageIssues = new System.Windows.Forms.TabPage();
            listViewIssues = new System.Windows.Forms.ListView();
            columnIssueStatus = new System.Windows.Forms.ColumnHeader();
            columnIssueSeverity = new System.Windows.Forms.ColumnHeader();
            columnIssueType = new System.Windows.Forms.ColumnHeader();
            columnIssueX = new System.Windows.Forms.ColumnHeader();
            columnIssueY = new System.Windows.Forms.ColumnHeader();
            columnIssueZ = new System.Windows.Forms.ColumnHeader();
            columnIssueId = new System.Windows.Forms.ColumnHeader();
            columnIssueDescription = new System.Windows.Forms.ColumnHeader();
            columnIssueNote = new System.Windows.Forms.ColumnHeader();
            panelIssueTop = new System.Windows.Forms.TableLayoutPanel();
            labelIssueFilter = new System.Windows.Forms.Label();
            comboBoxIssueFilter = new System.Windows.Forms.ComboBox();
            labelIssueSummary = new System.Windows.Forms.Label();
            flowPanelIssueButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonFixChecked = new System.Windows.Forms.Button();
            buttonCheckFixable = new System.Windows.Forms.Button();
            buttonIgnore = new System.Windows.Forms.Button();
            buttonReopen = new System.Windows.Forms.Button();
            buttonNote = new System.Windows.Forms.Button();
            buttonDeleteIssue = new System.Windows.Forms.Button();
            buttonExportCsv = new System.Windows.Forms.Button();
            buttonSaveProtocol = new System.Windows.Forms.Button();
            buttonLoadProtocol = new System.Windows.Forms.Button();
            tabPageView = new System.Windows.Forms.TabPage();
            tableLayoutView = new System.Windows.Forms.TableLayoutPanel();
            groupBoxDisplay = new System.Windows.Forms.GroupBox();
            tableLayoutDisplay = new System.Windows.Forms.TableLayoutPanel();
            labelMaxZ = new System.Windows.Forms.Label();
            trackBarMaxZ = new System.Windows.Forms.TrackBar();
            checkBoxNoDraw = new System.Windows.Forms.CheckBox();
            checkBoxLighting = new System.Windows.Forms.CheckBox();
            groupBoxCheck = new System.Windows.Forms.GroupBox();
            tableLayoutCheck = new System.Windows.Forms.TableLayoutPanel();
            checkBoxCheckIds = new System.Windows.Forms.CheckBox();
            checkBoxCheckDuplicates = new System.Windows.Forms.CheckBox();
            checkBoxCheckBelow = new System.Windows.Forms.CheckBox();
            checkBoxCheckSteep = new System.Windows.Forms.CheckBox();
            numericSteep = new System.Windows.Forms.NumericUpDown();
            checkBoxCheckFloating = new System.Windows.Forms.CheckBox();
            numericFloating = new System.Windows.Forms.NumericUpDown();
            groupBoxCache = new System.Windows.Forms.GroupBox();
            tableLayoutCache = new System.Windows.Forms.TableLayoutPanel();
            labelCacheMb = new System.Windows.Forms.Label();
            numericCacheMb = new System.Windows.Forms.NumericUpDown();
            checkBoxDiskCache = new System.Windows.Forms.CheckBox();
            buttonClearCache = new System.Windows.Forms.Button();
            labelCacheInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusLabelCell = new System.Windows.Forms.ToolStripStatusLabel();
            statusLabelRender = new System.Windows.Forms.ToolStripStatusLabel();
            statusLabelChanges = new System.Windows.Forms.ToolStripStatusLabel();
            toolTip = new System.Windows.Forms.ToolTip(components);
            tabPageBuild = new System.Windows.Forms.TabPage();
            panelBuildScroll = new System.Windows.Forms.Panel();
            tableLayoutBuild = new System.Windows.Forms.TableLayoutPanel();
            groupBoxMulti = new System.Windows.Forms.GroupBox();
            tableLayoutMulti = new System.Windows.Forms.TableLayoutPanel();
            labelTemplate = new System.Windows.Forms.Label();
            flowPanelMulti = new System.Windows.Forms.FlowLayoutPanel();
            buttonOpenBrowser = new System.Windows.Forms.Button();
            buttonImportTemplate = new System.Windows.Forms.Button();
            buttonNormalizeZ = new System.Windows.Forms.Button();
            buttonClearTemplate = new System.Windows.Forms.Button();
            groupBoxPlace = new System.Windows.Forms.GroupBox();
            tableLayoutPlace = new System.Windows.Forms.TableLayoutPanel();
            labelAnchor = new System.Windows.Forms.Label();
            comboBoxAnchor = new System.Windows.Forms.ComboBox();
            labelHeightMode = new System.Windows.Forms.Label();
            comboBoxHeightMode = new System.Windows.Forms.ComboBox();
            labelZValue = new System.Windows.Forms.Label();
            numericZValue = new System.Windows.Forms.NumericUpDown();
            checkBoxInvisible = new System.Windows.Forms.CheckBox();
            checkBoxFlatten = new System.Windows.Forms.CheckBox();
            labelObstacles = new System.Windows.Forms.Label();
            comboBoxObstacles = new System.Windows.Forms.ComboBox();
            labelPlacement = new System.Windows.Forms.Label();
            listBoxChecks = new System.Windows.Forms.ListBox();
            listViewObstacles = new System.Windows.Forms.ListView();
            columnObsX = new System.Windows.Forms.ColumnHeader();
            columnObsY = new System.Windows.Forms.ColumnHeader();
            columnObsZ = new System.Windows.Forms.ColumnHeader();
            columnObsId = new System.Windows.Forms.ColumnHeader();
            columnObsName = new System.Windows.Forms.ColumnHeader();
            flowPanelPlace = new System.Windows.Forms.FlowLayoutPanel();
            buttonPlaceMode = new System.Windows.Forms.Button();
            buttonPlaceNow = new System.Windows.Forms.Button();
            buttonCancelPlace = new System.Windows.Forms.Button();
            labelKeys = new System.Windows.Forms.Label();
            groupBoxArea = new System.Windows.Forms.GroupBox();
            tableLayoutArea = new System.Windows.Forms.TableLayoutPanel();
            buttonSelectArea = new System.Windows.Forms.Button();
            labelArea = new System.Windows.Forms.Label();
            labelAreaZ = new System.Windows.Forms.Label();
            flowPanelAreaZ = new System.Windows.Forms.FlowLayoutPanel();
            numericAreaMinZ = new System.Windows.Forms.NumericUpDown();
            labelAreaTo = new System.Windows.Forms.Label();
            numericAreaMaxZ = new System.Windows.Forms.NumericUpDown();
            labelAreaFilter = new System.Windows.Forms.Label();
            comboBoxAreaFilter = new System.Windows.Forms.ComboBox();
            flowPanelArea = new System.Windows.Forms.FlowLayoutPanel();
            buttonAreaRemove = new System.Windows.Forms.Button();
            buttonAreaFlatten = new System.Windows.Forms.Button();
            numericFlattenZ = new System.Windows.Forms.NumericUpDown();
            buttonAreaExport = new System.Windows.Forms.Button();
            buttonAreaLibrary = new System.Windows.Forms.Button();
            buttonAreaMulti = new System.Windows.Forms.Button();
            toolStripButtonUndoLast = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRedo = new System.Windows.Forms.ToolStripButton();
            toolStripButtonBackups = new System.Windows.Forms.ToolStripButton();
            toolStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            contextMenuView.SuspendLayout();
            tabControlSide.SuspendLayout();
            tabPageTile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            flowPanelTileButtons.SuspendLayout();
            tabPageIssues.SuspendLayout();
            panelIssueTop.SuspendLayout();
            flowPanelIssueButtons.SuspendLayout();
            tabPageView.SuspendLayout();
            tableLayoutView.SuspendLayout();
            groupBoxDisplay.SuspendLayout();
            tableLayoutDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMaxZ).BeginInit();
            groupBoxCheck.SuspendLayout();
            tableLayoutCheck.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericSteep).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericFloating).BeginInit();
            groupBoxCache.SuspendLayout();
            tableLayoutCache.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericCacheMb).BeginInit();
            tabPageBuild.SuspendLayout();
            panelBuildScroll.SuspendLayout();
            tableLayoutBuild.SuspendLayout();
            groupBoxMulti.SuspendLayout();
            tableLayoutMulti.SuspendLayout();
            flowPanelMulti.SuspendLayout();
            groupBoxPlace.SuspendLayout();
            tableLayoutPlace.SuspendLayout();
            flowPanelPlace.SuspendLayout();
            groupBoxArea.SuspendLayout();
            tableLayoutArea.SuspendLayout();
            flowPanelAreaZ.SuspendLayout();
            flowPanelArea.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericZValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericAreaMinZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericAreaMaxZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericFlattenZ).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStripMain
            //
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonZoomIn, toolStripButtonZoomOut, toolStripButtonFit, toolStripLabelZoom, toolStripSeparator1, toolStripButtonLand, toolStripButtonStatics, toolStripButtonStretch, toolStripButtonHeight, toolStripButtonGrid, toolStripButtonIssueOverlay, toolStripSeparator2, toolStripButtonCheck, toolStripButtonFixAll, toolStripButtonUndoLast, toolStripButtonRedo, toolStripButtonUndo, toolStripButtonSave, toolStripButtonBackups, toolStripSeparator3, toolStripButtonReload, toolStripButtonShowOnMap, toolStripButtonHelp });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripMain.Size = new System.Drawing.Size(1264, 29);
            toolStripMain.TabIndex = 0;
            //
            // toolStripButtonHelp
            //
            toolStripButtonHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButtonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonHelp.Name = "toolStripButtonHelp";
            toolStripButtonHelp.Size = new System.Drawing.Size(60, 22);
            toolStripButtonHelp.Text = "Help (F1)";
            toolStripButtonHelp.ToolTipText = "Explanation of all functions";
            toolStripButtonHelp.Click += OnClickHelp;
            //
            // toolStripButtonZoomIn
            //
            toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonZoomIn.Image = Properties.Resources.zoomplus;
            toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            toolStripButtonZoomIn.Size = new System.Drawing.Size(23, 22);
            toolStripButtonZoomIn.Text = "Zoom in";
            toolStripButtonZoomIn.Click += OnClickZoomIn;
            //
            // toolStripButtonZoomOut
            //
            toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonZoomOut.Image = Properties.Resources.zoomminus;
            toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            toolStripButtonZoomOut.Size = new System.Drawing.Size(23, 22);
            toolStripButtonZoomOut.Text = "Zoom out";
            toolStripButtonZoomOut.Click += OnClickZoomOut;
            //
            // toolStripButtonFit
            //
            toolStripButtonFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFit.Name = "toolStripButtonFit";
            toolStripButtonFit.Size = new System.Drawing.Size(27, 22);
            toolStripButtonFit.Text = "Fit";
            toolStripButtonFit.ToolTipText = "Show the whole region";
            toolStripButtonFit.Click += OnClickFit;
            //
            // toolStripLabelZoom
            //
            toolStripLabelZoom.AutoSize = false;
            toolStripLabelZoom.Name = "toolStripLabelZoom";
            toolStripLabelZoom.Size = new System.Drawing.Size(70, 22);
            toolStripLabelZoom.Text = "100%";
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonLand
            //
            toolStripButtonLand.Checked = true;
            toolStripButtonLand.CheckOnClick = true;
            toolStripButtonLand.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonLand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonLand.Name = "toolStripButtonLand";
            toolStripButtonLand.Size = new System.Drawing.Size(37, 22);
            toolStripButtonLand.Text = "Land";
            toolStripButtonLand.ToolTipText = "Show land tiles";
            toolStripButtonLand.CheckedChanged += OnRenderOptionChanged;
            //
            // toolStripButtonStatics
            //
            toolStripButtonStatics.Checked = true;
            toolStripButtonStatics.CheckOnClick = true;
            toolStripButtonStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonStatics.Name = "toolStripButtonStatics";
            toolStripButtonStatics.Size = new System.Drawing.Size(45, 22);
            toolStripButtonStatics.Text = "Statics";
            toolStripButtonStatics.ToolTipText = "Show statics";
            toolStripButtonStatics.CheckedChanged += OnRenderOptionChanged;
            //
            // toolStripButtonStretch
            //
            toolStripButtonStretch.Checked = true;
            toolStripButtonStretch.CheckOnClick = true;
            toolStripButtonStretch.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonStretch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonStretch.Name = "toolStripButtonStretch";
            toolStripButtonStretch.Size = new System.Drawing.Size(39, 22);
            toolStripButtonStretch.Text = "Hills";
            toolStripButtonStretch.ToolTipText = "Draw uneven land with stretched textures like the client";
            toolStripButtonStretch.CheckedChanged += OnRenderOptionChanged;
            //
            // toolStripButtonHeight
            //
            toolStripButtonHeight.CheckOnClick = true;
            toolStripButtonHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonHeight.Name = "toolStripButtonHeight";
            toolStripButtonHeight.Size = new System.Drawing.Size(48, 22);
            toolStripButtonHeight.Text = "Height";
            toolStripButtonHeight.ToolTipText = "Color the land by its height";
            toolStripButtonHeight.CheckedChanged += OnRenderOptionChanged;
            //
            // toolStripButtonGrid
            //
            toolStripButtonGrid.CheckOnClick = true;
            toolStripButtonGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonGrid.Name = "toolStripButtonGrid";
            toolStripButtonGrid.Size = new System.Drawing.Size(33, 22);
            toolStripButtonGrid.Text = "Grid";
            toolStripButtonGrid.ToolTipText = "Show the tile grid";
            toolStripButtonGrid.CheckedChanged += OnOverlayOptionChanged;
            //
            // toolStripButtonIssueOverlay
            //
            toolStripButtonIssueOverlay.Checked = true;
            toolStripButtonIssueOverlay.CheckOnClick = true;
            toolStripButtonIssueOverlay.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonIssueOverlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonIssueOverlay.Name = "toolStripButtonIssueOverlay";
            toolStripButtonIssueOverlay.Size = new System.Drawing.Size(45, 22);
            toolStripButtonIssueOverlay.Text = "Errors";
            toolStripButtonIssueOverlay.ToolTipText = "Mark the found errors in the view";
            toolStripButtonIssueOverlay.CheckedChanged += OnOverlayOptionChanged;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonCheck
            //
            toolStripButtonCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonCheck.Name = "toolStripButtonCheck";
            toolStripButtonCheck.Size = new System.Drawing.Size(84, 22);
            toolStripButtonCheck.Text = "Check region";
            toolStripButtonCheck.ToolTipText = "Search the region for errors";
            toolStripButtonCheck.Click += OnClickCheck;
            //
            // toolStripButtonFixAll
            //
            toolStripButtonFixAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonFixAll.Name = "toolStripButtonFixAll";
            toolStripButtonFixAll.Size = new System.Drawing.Size(73, 22);
            toolStripButtonFixAll.Text = "Fix all (safe)";
            toolStripButtonFixAll.ToolTipText = "Fixes all open errors with a safe repair (invalid ids, duplicates, statics below land)";
            toolStripButtonFixAll.Click += OnClickFixAll;
            //
            // toolStripButtonUndo
            //
            toolStripButtonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonUndo.Enabled = false;
            toolStripButtonUndo.Name = "toolStripButtonUndo";
            toolStripButtonUndo.Size = new System.Drawing.Size(58, 22);
            toolStripButtonUndo.Text = "Undo all";
            toolStripButtonUndo.ToolTipText = "Restores all changes made in this window";
            toolStripButtonUndo.Click += OnClickUndo;
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Image = Properties.Resources.save;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(51, 22);
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.ToolTipText = "Writes map/statics files into the output directory";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            //
            // toolStripButtonReload
            //
            toolStripButtonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReload.Name = "toolStripButtonReload";
            toolStripButtonReload.Size = new System.Drawing.Size(47, 22);
            toolStripButtonReload.Text = "Reload";
            toolStripButtonReload.ToolTipText = "Reads the region again (e.g. after changes in other tools)";
            toolStripButtonReload.Click += OnClickReload;
            //
            // toolStripButtonShowOnMap
            //
            toolStripButtonShowOnMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonShowOnMap.Name = "toolStripButtonShowOnMap";
            toolStripButtonShowOnMap.Size = new System.Drawing.Size(82, 22);
            toolStripButtonShowOnMap.Text = "Show on map";
            toolStripButtonShowOnMap.ToolTipText = "Centers the map tab on the selected position";
            toolStripButtonShowOnMap.Click += OnClickShowOnMap;
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainerMain.Location = new System.Drawing.Point(0, 29);
            splitContainerMain.Name = "splitContainerMain";
            //
            // splitContainerMain.Panel1
            //
            splitContainerMain.Panel1.Controls.Add(isoView);
            //
            // splitContainerMain.Panel2
            //
            splitContainerMain.Panel2.Controls.Add(tabControlSide);
            splitContainerMain.Size = new System.Drawing.Size(1264, 710);
            splitContainerMain.SplitterDistance = 820;
            splitContainerMain.TabIndex = 1;
            //
            // isoView
            //
            isoView.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            isoView.ContextMenuStrip = contextMenuView;
            isoView.Dock = System.Windows.Forms.DockStyle.Fill;
            isoView.Location = new System.Drawing.Point(0, 0);
            isoView.Name = "isoView";
            isoView.Size = new System.Drawing.Size(820, 710);
            isoView.TabIndex = 0;
            isoView.CellClicked += OnCellClicked;
            isoView.HoverChanged += OnHoverChanged;
            isoView.ZoomChanged += OnZoomChanged;
            isoView.RenderProgress += OnRenderProgress;
            //
            // contextMenuView
            //
            contextMenuView.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { markErrorToolStripMenuItem, removeStaticToolStripMenuItem, copyCoordinatesToolStripMenuItem, showOnMapToolStripMenuItem });
            contextMenuView.Name = "contextMenuView";
            contextMenuView.Size = new System.Drawing.Size(220, 92);
            contextMenuView.Opening += OnContextOpening;
            //
            // markErrorToolStripMenuItem
            //
            markErrorToolStripMenuItem.Name = "markErrorToolStripMenuItem";
            markErrorToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            markErrorToolStripMenuItem.Text = "Mark error here...";
            markErrorToolStripMenuItem.Click += OnClickMarkErrorHere;
            //
            // removeStaticToolStripMenuItem
            //
            removeStaticToolStripMenuItem.Name = "removeStaticToolStripMenuItem";
            removeStaticToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            removeStaticToolStripMenuItem.Text = "Remove this static";
            removeStaticToolStripMenuItem.Click += OnClickRemoveStaticHere;
            //
            // copyCoordinatesToolStripMenuItem
            //
            copyCoordinatesToolStripMenuItem.Name = "copyCoordinatesToolStripMenuItem";
            copyCoordinatesToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            copyCoordinatesToolStripMenuItem.Text = "Copy coordinates";
            copyCoordinatesToolStripMenuItem.Click += OnClickCopyCoordinates;
            //
            // showOnMapToolStripMenuItem
            //
            showOnMapToolStripMenuItem.Name = "showOnMapToolStripMenuItem";
            showOnMapToolStripMenuItem.Size = new System.Drawing.Size(219, 22);
            showOnMapToolStripMenuItem.Text = "Show on map";
            showOnMapToolStripMenuItem.Click += OnClickShowOnMap;
            //
            // tabControlSide
            //
            tabControlSide.Controls.Add(tabPageTile);
            tabControlSide.Controls.Add(tabPageIssues);
            tabControlSide.Controls.Add(tabPageBuild);
            tabControlSide.Controls.Add(tabPageView);
            tabControlSide.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlSide.Location = new System.Drawing.Point(0, 0);
            tabControlSide.Name = "tabControlSide";
            tabControlSide.SelectedIndex = 0;
            tabControlSide.Size = new System.Drawing.Size(440, 710);
            tabControlSide.TabIndex = 0;
            //
            // tabPageTile
            //
            tabPageTile.Controls.Add(listViewTile);
            tabPageTile.Controls.Add(pictureBoxPreview);
            tabPageTile.Controls.Add(labelCellHeader);
            tabPageTile.Controls.Add(flowPanelTileButtons);
            tabPageTile.Location = new System.Drawing.Point(4, 24);
            tabPageTile.Name = "tabPageTile";
            tabPageTile.Padding = new System.Windows.Forms.Padding(6);
            tabPageTile.Size = new System.Drawing.Size(432, 682);
            tabPageTile.TabIndex = 0;
            tabPageTile.Text = "Tile";
            tabPageTile.UseVisualStyleBackColor = true;
            //
            // listViewTile
            //
            listViewTile.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnTileKind, columnTileId, columnTileName, columnTileZ, columnTileHeight, columnTileHue, columnTileFlags });
            listViewTile.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewTile.FullRowSelect = true;
            listViewTile.GridLines = true;
            listViewTile.HideSelection = false;
            listViewTile.Location = new System.Drawing.Point(6, 190);
            listViewTile.MultiSelect = false;
            listViewTile.Name = "listViewTile";
            listViewTile.Size = new System.Drawing.Size(420, 426);
            listViewTile.TabIndex = 2;
            listViewTile.UseCompatibleStateImageBehavior = false;
            listViewTile.View = System.Windows.Forms.View.Details;
            listViewTile.SelectedIndexChanged += OnTileSelectionChanged;
            //
            // columnTileKind
            //
            columnTileKind.Text = "Kind";
            columnTileKind.Width = 50;
            //
            // columnTileId
            //
            columnTileId.Text = "ID";
            columnTileId.Width = 58;
            //
            // columnTileName
            //
            columnTileName.Text = "Name";
            columnTileName.Width = 110;
            //
            // columnTileZ
            //
            columnTileZ.Text = "Z";
            columnTileZ.Width = 40;
            //
            // columnTileHeight
            //
            columnTileHeight.Text = "H";
            columnTileHeight.Width = 32;
            //
            // columnTileHue
            //
            columnTileHue.Text = "Hue";
            columnTileHue.Width = 45;
            //
            // columnTileFlags
            //
            columnTileFlags.Text = "Flags";
            columnTileFlags.Width = 160;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 46);
            pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Top;
            pictureBoxPreview.Location = new System.Drawing.Point(6, 36);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(420, 154);
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabIndex = 1;
            pictureBoxPreview.TabStop = false;
            //
            // labelCellHeader
            //
            labelCellHeader.Dock = System.Windows.Forms.DockStyle.Top;
            labelCellHeader.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            labelCellHeader.Location = new System.Drawing.Point(6, 6);
            labelCellHeader.Name = "labelCellHeader";
            labelCellHeader.Size = new System.Drawing.Size(420, 30);
            labelCellHeader.TabIndex = 0;
            labelCellHeader.Text = "Click on a tile in the view";
            labelCellHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // flowPanelTileButtons
            //
            flowPanelTileButtons.AutoSize = true;
            flowPanelTileButtons.Controls.Add(buttonRemoveStatic);
            flowPanelTileButtons.Controls.Add(buttonZUp);
            flowPanelTileButtons.Controls.Add(buttonZDown);
            flowPanelTileButtons.Controls.Add(buttonMarkIssue);
            flowPanelTileButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowPanelTileButtons.Location = new System.Drawing.Point(6, 616);
            flowPanelTileButtons.Name = "flowPanelTileButtons";
            flowPanelTileButtons.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            flowPanelTileButtons.Size = new System.Drawing.Size(420, 60);
            flowPanelTileButtons.TabIndex = 3;
            //
            // buttonRemoveStatic
            //
            buttonRemoveStatic.AutoSize = true;
            buttonRemoveStatic.Enabled = false;
            buttonRemoveStatic.Name = "buttonRemoveStatic";
            buttonRemoveStatic.TabIndex = 0;
            buttonRemoveStatic.Text = "Remove static";
            buttonRemoveStatic.UseVisualStyleBackColor = true;
            buttonRemoveStatic.Click += OnClickRemoveStatic;
            //
            // buttonZUp
            //
            buttonZUp.AutoSize = true;
            buttonZUp.Enabled = false;
            buttonZUp.Name = "buttonZUp";
            buttonZUp.TabIndex = 1;
            buttonZUp.Text = "Z +1";
            toolTip.SetToolTip(buttonZUp, "Raises the selected entry (Shift: +5)");
            buttonZUp.UseVisualStyleBackColor = true;
            buttonZUp.Click += OnClickZUp;
            //
            // buttonZDown
            //
            buttonZDown.AutoSize = true;
            buttonZDown.Enabled = false;
            buttonZDown.Name = "buttonZDown";
            buttonZDown.TabIndex = 2;
            buttonZDown.Text = "Z -1";
            toolTip.SetToolTip(buttonZDown, "Lowers the selected entry (Shift: -5)");
            buttonZDown.UseVisualStyleBackColor = true;
            buttonZDown.Click += OnClickZDown;
            //
            // buttonMarkIssue
            //
            buttonMarkIssue.AutoSize = true;
            buttonMarkIssue.Enabled = false;
            buttonMarkIssue.Name = "buttonMarkIssue";
            buttonMarkIssue.TabIndex = 3;
            buttonMarkIssue.Text = "Mark error...";
            toolTip.SetToolTip(buttonMarkIssue, "Adds this position to the protocol");
            buttonMarkIssue.UseVisualStyleBackColor = true;
            buttonMarkIssue.Click += OnClickMarkIssue;
            //
            // tabPageIssues
            //
            tabPageIssues.Controls.Add(listViewIssues);
            tabPageIssues.Controls.Add(panelIssueTop);
            tabPageIssues.Controls.Add(flowPanelIssueButtons);
            tabPageIssues.Location = new System.Drawing.Point(4, 24);
            tabPageIssues.Name = "tabPageIssues";
            tabPageIssues.Padding = new System.Windows.Forms.Padding(6);
            tabPageIssues.Size = new System.Drawing.Size(432, 682);
            tabPageIssues.TabIndex = 1;
            tabPageIssues.Text = "Errors / Protocol";
            tabPageIssues.UseVisualStyleBackColor = true;
            //
            // listViewIssues
            //
            listViewIssues.CheckBoxes = true;
            listViewIssues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnIssueStatus, columnIssueSeverity, columnIssueType, columnIssueX, columnIssueY, columnIssueZ, columnIssueId, columnIssueDescription, columnIssueNote });
            listViewIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewIssues.FullRowSelect = true;
            listViewIssues.GridLines = true;
            listViewIssues.HideSelection = false;
            listViewIssues.Location = new System.Drawing.Point(6, 62);
            listViewIssues.Name = "listViewIssues";
            listViewIssues.ShowItemToolTips = true;
            listViewIssues.Size = new System.Drawing.Size(420, 494);
            listViewIssues.TabIndex = 1;
            listViewIssues.UseCompatibleStateImageBehavior = false;
            listViewIssues.View = System.Windows.Forms.View.Details;
            listViewIssues.SelectedIndexChanged += OnIssueSelectionChanged;
            listViewIssues.DoubleClick += OnIssueDoubleClick;
            //
            // columnIssueStatus
            //
            columnIssueStatus.Text = "Status";
            columnIssueStatus.Width = 70;
            //
            // columnIssueSeverity
            //
            columnIssueSeverity.Text = "Level";
            columnIssueSeverity.Width = 60;
            //
            // columnIssueType
            //
            columnIssueType.Text = "Type";
            columnIssueType.Width = 110;
            //
            // columnIssueX
            //
            columnIssueX.Text = "X";
            columnIssueX.Width = 45;
            //
            // columnIssueY
            //
            columnIssueY.Text = "Y";
            columnIssueY.Width = 45;
            //
            // columnIssueZ
            //
            columnIssueZ.Text = "Z";
            columnIssueZ.Width = 35;
            //
            // columnIssueId
            //
            columnIssueId.Text = "ID";
            columnIssueId.Width = 55;
            //
            // columnIssueDescription
            //
            columnIssueDescription.Text = "Description";
            columnIssueDescription.Width = 250;
            //
            // columnIssueNote
            //
            columnIssueNote.Text = "Note";
            columnIssueNote.Width = 150;
            //
            // panelIssueTop
            //
            panelIssueTop.AutoSize = true;
            panelIssueTop.ColumnCount = 3;
            panelIssueTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelIssueTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelIssueTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelIssueTop.Controls.Add(labelIssueFilter, 0, 0);
            panelIssueTop.Controls.Add(comboBoxIssueFilter, 1, 0);
            panelIssueTop.Controls.Add(labelIssueSummary, 2, 0);
            panelIssueTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelIssueTop.Location = new System.Drawing.Point(6, 6);
            panelIssueTop.Name = "panelIssueTop";
            panelIssueTop.RowCount = 1;
            panelIssueTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelIssueTop.Size = new System.Drawing.Size(420, 29);
            panelIssueTop.TabIndex = 0;
            //
            // labelIssueFilter
            //
            labelIssueFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelIssueFilter.AutoSize = true;
            labelIssueFilter.Name = "labelIssueFilter";
            labelIssueFilter.Text = "Show:";
            //
            // comboBoxIssueFilter
            //
            comboBoxIssueFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxIssueFilter.Items.AddRange(new object[] { "Open", "All", "Errors only", "Fixed", "Ignored", "Marked by hand" });
            comboBoxIssueFilter.Name = "comboBoxIssueFilter";
            comboBoxIssueFilter.Size = new System.Drawing.Size(120, 23);
            comboBoxIssueFilter.TabIndex = 0;
            comboBoxIssueFilter.SelectedIndexChanged += OnIssueFilterChanged;
            //
            // labelIssueSummary
            //
            labelIssueSummary.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelIssueSummary.AutoSize = true;
            labelIssueSummary.Name = "labelIssueSummary";
            labelIssueSummary.Text = "-";
            //
            // flowPanelIssueButtons
            //
            flowPanelIssueButtons.AutoSize = true;
            flowPanelIssueButtons.Controls.Add(buttonFixChecked);
            flowPanelIssueButtons.Controls.Add(buttonCheckFixable);
            flowPanelIssueButtons.Controls.Add(buttonIgnore);
            flowPanelIssueButtons.Controls.Add(buttonReopen);
            flowPanelIssueButtons.Controls.Add(buttonNote);
            flowPanelIssueButtons.Controls.Add(buttonDeleteIssue);
            flowPanelIssueButtons.Controls.Add(buttonExportCsv);
            flowPanelIssueButtons.Controls.Add(buttonSaveProtocol);
            flowPanelIssueButtons.Controls.Add(buttonLoadProtocol);
            flowPanelIssueButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowPanelIssueButtons.Location = new System.Drawing.Point(6, 556);
            flowPanelIssueButtons.Name = "flowPanelIssueButtons";
            flowPanelIssueButtons.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            flowPanelIssueButtons.Size = new System.Drawing.Size(420, 120);
            flowPanelIssueButtons.TabIndex = 2;
            //
            // buttonFixChecked
            //
            buttonFixChecked.AutoSize = true;
            buttonFixChecked.Name = "buttonFixChecked";
            buttonFixChecked.TabIndex = 0;
            buttonFixChecked.Text = "Fix checked";
            toolTip.SetToolTip(buttonFixChecked, "Repairs all checked entries (in memory, save afterwards)");
            buttonFixChecked.UseVisualStyleBackColor = true;
            buttonFixChecked.Click += OnClickFixChecked;
            //
            // buttonCheckFixable
            //
            buttonCheckFixable.AutoSize = true;
            buttonCheckFixable.Name = "buttonCheckFixable";
            buttonCheckFixable.TabIndex = 1;
            buttonCheckFixable.Text = "Check fixable";
            toolTip.SetToolTip(buttonCheckFixable, "Checks all open entries that can be repaired automatically");
            buttonCheckFixable.UseVisualStyleBackColor = true;
            buttonCheckFixable.Click += OnClickCheckFixable;
            //
            // buttonIgnore
            //
            buttonIgnore.AutoSize = true;
            buttonIgnore.Name = "buttonIgnore";
            buttonIgnore.TabIndex = 2;
            buttonIgnore.Text = "Ignore";
            buttonIgnore.UseVisualStyleBackColor = true;
            buttonIgnore.Click += OnClickIgnore;
            //
            // buttonReopen
            //
            buttonReopen.AutoSize = true;
            buttonReopen.Name = "buttonReopen";
            buttonReopen.TabIndex = 3;
            buttonReopen.Text = "Reopen";
            buttonReopen.UseVisualStyleBackColor = true;
            buttonReopen.Click += OnClickReopen;
            //
            // buttonNote
            //
            buttonNote.AutoSize = true;
            buttonNote.Name = "buttonNote";
            buttonNote.TabIndex = 4;
            buttonNote.Text = "Note...";
            buttonNote.UseVisualStyleBackColor = true;
            buttonNote.Click += OnClickNote;
            //
            // buttonDeleteIssue
            //
            buttonDeleteIssue.AutoSize = true;
            buttonDeleteIssue.Name = "buttonDeleteIssue";
            buttonDeleteIssue.TabIndex = 5;
            buttonDeleteIssue.Text = "Delete";
            toolTip.SetToolTip(buttonDeleteIssue, "Removes the selected entries from the protocol");
            buttonDeleteIssue.UseVisualStyleBackColor = true;
            buttonDeleteIssue.Click += OnClickDeleteIssue;
            //
            // buttonExportCsv
            //
            buttonExportCsv.AutoSize = true;
            buttonExportCsv.Name = "buttonExportCsv";
            buttonExportCsv.TabIndex = 6;
            buttonExportCsv.Text = "Export CSV...";
            buttonExportCsv.UseVisualStyleBackColor = true;
            buttonExportCsv.Click += OnClickExportCsv;
            //
            // buttonSaveProtocol
            //
            buttonSaveProtocol.AutoSize = true;
            buttonSaveProtocol.Name = "buttonSaveProtocol";
            buttonSaveProtocol.TabIndex = 7;
            buttonSaveProtocol.Text = "Save protocol...";
            buttonSaveProtocol.UseVisualStyleBackColor = true;
            buttonSaveProtocol.Click += OnClickSaveProtocol;
            //
            // buttonLoadProtocol
            //
            buttonLoadProtocol.AutoSize = true;
            buttonLoadProtocol.Name = "buttonLoadProtocol";
            buttonLoadProtocol.TabIndex = 8;
            buttonLoadProtocol.Text = "Load protocol...";
            buttonLoadProtocol.UseVisualStyleBackColor = true;
            buttonLoadProtocol.Click += OnClickLoadProtocol;
            //
            // tabPageView
            //
            tabPageView.AutoScroll = true;
            tabPageView.Controls.Add(tableLayoutView);
            tabPageView.Location = new System.Drawing.Point(4, 24);
            tabPageView.Name = "tabPageView";
            tabPageView.Padding = new System.Windows.Forms.Padding(6);
            tabPageView.Size = new System.Drawing.Size(432, 682);
            tabPageView.TabIndex = 2;
            tabPageView.Text = "Settings";
            tabPageView.UseVisualStyleBackColor = true;
            //
            // tableLayoutView
            //
            tableLayoutView.AutoSize = true;
            tableLayoutView.ColumnCount = 1;
            tableLayoutView.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutView.Controls.Add(groupBoxDisplay, 0, 0);
            tableLayoutView.Controls.Add(groupBoxCheck, 0, 1);
            tableLayoutView.Controls.Add(groupBoxCache, 0, 2);
            tableLayoutView.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayoutView.Location = new System.Drawing.Point(6, 6);
            tableLayoutView.Name = "tableLayoutView";
            tableLayoutView.RowCount = 3;
            tableLayoutView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutView.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutView.Size = new System.Drawing.Size(420, 500);
            tableLayoutView.TabIndex = 0;
            //
            // groupBoxDisplay
            //
            groupBoxDisplay.AutoSize = true;
            groupBoxDisplay.Controls.Add(tableLayoutDisplay);
            groupBoxDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxDisplay.Name = "groupBoxDisplay";
            groupBoxDisplay.TabIndex = 0;
            groupBoxDisplay.TabStop = false;
            groupBoxDisplay.Text = "Display";
            //
            // tableLayoutDisplay
            //
            tableLayoutDisplay.AutoSize = true;
            tableLayoutDisplay.ColumnCount = 1;
            tableLayoutDisplay.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutDisplay.Controls.Add(labelMaxZ, 0, 0);
            tableLayoutDisplay.Controls.Add(trackBarMaxZ, 0, 1);
            tableLayoutDisplay.Controls.Add(checkBoxNoDraw, 0, 2);
            tableLayoutDisplay.Controls.Add(checkBoxLighting, 0, 3);
            tableLayoutDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutDisplay.Name = "tableLayoutDisplay";
            tableLayoutDisplay.RowCount = 4;
            tableLayoutDisplay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDisplay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDisplay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDisplay.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutDisplay.TabIndex = 0;
            //
            // labelMaxZ
            //
            labelMaxZ.AutoSize = true;
            labelMaxZ.Name = "labelMaxZ";
            labelMaxZ.Text = "Hide statics above z: 127 (all visible)";
            //
            // trackBarMaxZ
            //
            trackBarMaxZ.Dock = System.Windows.Forms.DockStyle.Fill;
            trackBarMaxZ.LargeChange = 10;
            trackBarMaxZ.Maximum = 127;
            trackBarMaxZ.Minimum = -128;
            trackBarMaxZ.Name = "trackBarMaxZ";
            trackBarMaxZ.TabIndex = 1;
            trackBarMaxZ.TickFrequency = 16;
            trackBarMaxZ.Value = 127;
            toolTip.SetToolTip(trackBarMaxZ, "Hides roofs and upper floors to look into buildings");
            trackBarMaxZ.Scroll += OnMaxZScroll;
            trackBarMaxZ.MouseUp += OnMaxZChanged;
            trackBarMaxZ.KeyUp += OnMaxZChanged;
            //
            // checkBoxNoDraw
            //
            checkBoxNoDraw.AutoSize = true;
            checkBoxNoDraw.Name = "checkBoxNoDraw";
            checkBoxNoDraw.TabIndex = 2;
            checkBoxNoDraw.Text = "Show nodraw tiles";
            checkBoxNoDraw.UseVisualStyleBackColor = true;
            checkBoxNoDraw.CheckedChanged += OnRenderOptionChanged;
            //
            // checkBoxLighting
            //
            checkBoxLighting.AutoSize = true;
            checkBoxLighting.Checked = true;
            checkBoxLighting.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLighting.Name = "checkBoxLighting";
            checkBoxLighting.TabIndex = 3;
            checkBoxLighting.Text = "Light on hills";
            checkBoxLighting.UseVisualStyleBackColor = true;
            checkBoxLighting.CheckedChanged += OnRenderOptionChanged;
            //
            // groupBoxCheck
            //
            groupBoxCheck.AutoSize = true;
            groupBoxCheck.Controls.Add(tableLayoutCheck);
            groupBoxCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxCheck.Name = "groupBoxCheck";
            groupBoxCheck.TabIndex = 1;
            groupBoxCheck.TabStop = false;
            groupBoxCheck.Text = "Error check";
            //
            // tableLayoutCheck
            //
            tableLayoutCheck.AutoSize = true;
            tableLayoutCheck.ColumnCount = 2;
            tableLayoutCheck.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutCheck.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.Controls.Add(checkBoxCheckIds, 0, 0);
            tableLayoutCheck.Controls.Add(checkBoxCheckDuplicates, 0, 1);
            tableLayoutCheck.Controls.Add(checkBoxCheckBelow, 0, 2);
            tableLayoutCheck.Controls.Add(checkBoxCheckSteep, 0, 3);
            tableLayoutCheck.Controls.Add(numericSteep, 1, 3);
            tableLayoutCheck.Controls.Add(checkBoxCheckFloating, 0, 4);
            tableLayoutCheck.Controls.Add(numericFloating, 1, 4);
            tableLayoutCheck.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutCheck.Name = "tableLayoutCheck";
            tableLayoutCheck.RowCount = 5;
            tableLayoutCheck.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCheck.TabIndex = 0;
            //
            // checkBoxCheckIds
            //
            checkBoxCheckIds.AutoSize = true;
            checkBoxCheckIds.Checked = true;
            checkBoxCheckIds.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCheckIds.Name = "checkBoxCheckIds";
            checkBoxCheckIds.TabIndex = 0;
            checkBoxCheckIds.Text = "Invalid land / static ids";
            checkBoxCheckIds.UseVisualStyleBackColor = true;
            //
            // checkBoxCheckDuplicates
            //
            checkBoxCheckDuplicates.AutoSize = true;
            checkBoxCheckDuplicates.Checked = true;
            checkBoxCheckDuplicates.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCheckDuplicates.Name = "checkBoxCheckDuplicates";
            checkBoxCheckDuplicates.TabIndex = 1;
            checkBoxCheckDuplicates.Text = "Duplicate statics";
            checkBoxCheckDuplicates.UseVisualStyleBackColor = true;
            //
            // checkBoxCheckBelow
            //
            checkBoxCheckBelow.AutoSize = true;
            checkBoxCheckBelow.Checked = true;
            checkBoxCheckBelow.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCheckBelow.Name = "checkBoxCheckBelow";
            checkBoxCheckBelow.TabIndex = 2;
            checkBoxCheckBelow.Text = "Statics below land (invisible)";
            checkBoxCheckBelow.UseVisualStyleBackColor = true;
            //
            // checkBoxCheckSteep
            //
            checkBoxCheckSteep.AutoSize = true;
            checkBoxCheckSteep.Checked = true;
            checkBoxCheckSteep.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxCheckSteep.Name = "checkBoxCheckSteep";
            checkBoxCheckSteep.TabIndex = 3;
            checkBoxCheckSteep.Text = "Steep land, more than z:";
            checkBoxCheckSteep.UseVisualStyleBackColor = true;
            //
            // numericSteep
            //
            numericSteep.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericSteep.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericSteep.Name = "numericSteep";
            numericSteep.Size = new System.Drawing.Size(60, 23);
            numericSteep.TabIndex = 4;
            numericSteep.Value = new decimal(new int[] { 30, 0, 0, 0 });
            //
            // checkBoxCheckFloating
            //
            checkBoxCheckFloating.AutoSize = true;
            checkBoxCheckFloating.Name = "checkBoxCheckFloating";
            checkBoxCheckFloating.TabIndex = 5;
            checkBoxCheckFloating.Text = "Floating statics, gap more than z:";
            toolTip.SetToolTip(checkBoxCheckFloating, "Many hits on upper floors, use it as a hint only");
            checkBoxCheckFloating.UseVisualStyleBackColor = true;
            //
            // numericFloating
            //
            numericFloating.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericFloating.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericFloating.Name = "numericFloating";
            numericFloating.Size = new System.Drawing.Size(60, 23);
            numericFloating.TabIndex = 6;
            numericFloating.Value = new decimal(new int[] { 10, 0, 0, 0 });
            //
            // groupBoxCache
            //
            groupBoxCache.AutoSize = true;
            groupBoxCache.Controls.Add(tableLayoutCache);
            groupBoxCache.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxCache.Name = "groupBoxCache";
            groupBoxCache.TabIndex = 2;
            groupBoxCache.TabStop = false;
            groupBoxCache.Text = "Cache";
            //
            // tableLayoutCache
            //
            tableLayoutCache.AutoSize = true;
            tableLayoutCache.ColumnCount = 2;
            tableLayoutCache.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutCache.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCache.Controls.Add(labelCacheMb, 0, 0);
            tableLayoutCache.Controls.Add(numericCacheMb, 1, 0);
            tableLayoutCache.Controls.Add(checkBoxDiskCache, 0, 1);
            tableLayoutCache.Controls.Add(buttonClearCache, 1, 1);
            tableLayoutCache.Controls.Add(labelCacheInfo, 0, 2);
            tableLayoutCache.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutCache.Name = "tableLayoutCache";
            tableLayoutCache.RowCount = 3;
            tableLayoutCache.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCache.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCache.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutCache.TabIndex = 0;
            //
            // labelCacheMb
            //
            labelCacheMb.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelCacheMb.AutoSize = true;
            labelCacheMb.Name = "labelCacheMb";
            labelCacheMb.Text = "Memory for rendered images (MB):";
            //
            // numericCacheMb
            //
            numericCacheMb.Increment = new decimal(new int[] { 64, 0, 0, 0 });
            numericCacheMb.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
            numericCacheMb.Minimum = new decimal(new int[] { 32, 0, 0, 0 });
            numericCacheMb.Name = "numericCacheMb";
            numericCacheMb.Size = new System.Drawing.Size(70, 23);
            numericCacheMb.TabIndex = 1;
            numericCacheMb.Value = new decimal(new int[] { 256, 0, 0, 0 });
            numericCacheMb.ValueChanged += OnCacheOptionChanged;
            //
            // checkBoxDiskCache
            //
            checkBoxDiskCache.AutoSize = true;
            checkBoxDiskCache.Name = "checkBoxDiskCache";
            checkBoxDiskCache.TabIndex = 2;
            checkBoxDiskCache.Text = "Swap out to temp directory";
            toolTip.SetToolTip(checkBoxDiskCache, "Images removed from memory are kept in the temp directory (deleted when the window is closed)");
            checkBoxDiskCache.UseVisualStyleBackColor = true;
            checkBoxDiskCache.CheckedChanged += OnCacheOptionChanged;
            //
            // buttonClearCache
            //
            buttonClearCache.AutoSize = true;
            buttonClearCache.Name = "buttonClearCache";
            buttonClearCache.TabIndex = 3;
            buttonClearCache.Text = "Clear";
            buttonClearCache.UseVisualStyleBackColor = true;
            buttonClearCache.Click += OnClickClearCache;
            //
            // labelCacheInfo
            //
            labelCacheInfo.AutoSize = true;
            tableLayoutCache.SetColumnSpan(labelCacheInfo, 2);
            labelCacheInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelCacheInfo.Name = "labelCacheInfo";
            labelCacheInfo.Text = "-";
            // 
            // tabPageBuild
            // 
            tabPageBuild.Controls.Add(panelBuildScroll);
            tabPageBuild.Name = "tabPageBuild";
            tabPageBuild.Padding = new System.Windows.Forms.Padding(4);
            tabPageBuild.Text = "Build";
            tabPageBuild.UseVisualStyleBackColor = true;
            // 
            // panelBuildScroll
            // 
            panelBuildScroll.AutoScroll = true;
            panelBuildScroll.Controls.Add(tableLayoutBuild);
            panelBuildScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            panelBuildScroll.Name = "panelBuildScroll";
            // 
            // tableLayoutBuild
            // 
            tableLayoutBuild.AutoSize = true;
            tableLayoutBuild.ColumnCount = 1;
            tableLayoutBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutBuild.Controls.Add(groupBoxMulti, 0, 0);
            tableLayoutBuild.Controls.Add(groupBoxPlace, 0, 1);
            tableLayoutBuild.Controls.Add(groupBoxArea, 0, 2);
            tableLayoutBuild.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayoutBuild.Name = "tableLayoutBuild";
            tableLayoutBuild.RowCount = 3;
            tableLayoutBuild.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutBuild.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutBuild.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // groupBoxMulti
            // 
            groupBoxMulti.AutoSize = true;
            groupBoxMulti.Controls.Add(tableLayoutMulti);
            groupBoxMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxMulti.Name = "groupBoxMulti";
            groupBoxMulti.TabStop = false;
            groupBoxMulti.Text = "Building";
            // 
            // tableLayoutMulti
            // 
            tableLayoutMulti.AutoSize = true;
            tableLayoutMulti.ColumnCount = 1;
            tableLayoutMulti.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutMulti.Controls.Add(labelTemplate, 0, 0);
            tableLayoutMulti.Controls.Add(flowPanelMulti, 0, 1);
            tableLayoutMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutMulti.Name = "tableLayoutMulti";
            tableLayoutMulti.RowCount = 2;
            tableLayoutMulti.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutMulti.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // labelTemplate
            // 
            labelTemplate.AutoSize = true;
            labelTemplate.MaximumSize = new System.Drawing.Size(400, 0);
            labelTemplate.Name = "labelTemplate";
            labelTemplate.Text = "No building selected. Open the multi window or import a file.";
            // 
            // flowPanelMulti
            // 
            flowPanelMulti.AutoSize = true;
            flowPanelMulti.Controls.Add(buttonOpenBrowser);
            flowPanelMulti.Controls.Add(buttonImportTemplate);
            flowPanelMulti.Controls.Add(buttonNormalizeZ);
            flowPanelMulti.Controls.Add(buttonClearTemplate);
            flowPanelMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelMulti.Name = "flowPanelMulti";
            // 
            // buttonOpenBrowser
            // 
            buttonOpenBrowser.AutoSize = true;
            buttonOpenBrowser.Name = "buttonOpenBrowser";
            buttonOpenBrowser.Text = "Multis / library...";
            buttonOpenBrowser.UseVisualStyleBackColor = true;
            buttonOpenBrowser.Click += OnClickOpenBrowser;
            toolTip.SetToolTip(buttonOpenBrowser, "Client multis (multi.mul) and the own building library");
            // 
            // buttonImportTemplate
            // 
            buttonImportTemplate.AutoSize = true;
            buttonImportTemplate.Name = "buttonImportTemplate";
            buttonImportTemplate.Text = "Import file...";
            buttonImportTemplate.UseVisualStyleBackColor = true;
            buttonImportTemplate.Click += OnClickImportTemplate;
            toolTip.SetToolTip(buttonImportTemplate, "TXT (decimal/hex), UOA, WSC, CSV");
            // 
            // buttonNormalizeZ
            // 
            buttonNormalizeZ.AutoSize = true;
            buttonNormalizeZ.Name = "buttonNormalizeZ";
            buttonNormalizeZ.Text = "Normalize Z";
            buttonNormalizeZ.UseVisualStyleBackColor = true;
            buttonNormalizeZ.Click += OnClickNormalizeZ;
            buttonNormalizeZ.Enabled = false;
            toolTip.SetToolTip(buttonNormalizeZ, "Moves the building so that the lowest part is at z 0");
            // 
            // buttonClearTemplate
            // 
            buttonClearTemplate.AutoSize = true;
            buttonClearTemplate.Name = "buttonClearTemplate";
            buttonClearTemplate.Text = "Clear";
            buttonClearTemplate.UseVisualStyleBackColor = true;
            buttonClearTemplate.Click += OnClickClearTemplate;
            buttonClearTemplate.Enabled = false;
            // 
            // groupBoxPlace
            // 
            groupBoxPlace.AutoSize = true;
            groupBoxPlace.Controls.Add(tableLayoutPlace);
            groupBoxPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxPlace.Name = "groupBoxPlace";
            groupBoxPlace.TabStop = false;
            groupBoxPlace.Text = "Place";
            // 
            // tableLayoutPlace
            // 
            tableLayoutPlace.AutoSize = true;
            tableLayoutPlace.ColumnCount = 2;
            tableLayoutPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPlace.Controls.Add(labelAnchor, 0, 0);
            tableLayoutPlace.Controls.Add(comboBoxAnchor, 1, 0);
            tableLayoutPlace.Controls.Add(labelHeightMode, 0, 1);
            tableLayoutPlace.Controls.Add(comboBoxHeightMode, 1, 1);
            tableLayoutPlace.Controls.Add(labelZValue, 0, 2);
            tableLayoutPlace.Controls.Add(numericZValue, 1, 2);
            tableLayoutPlace.Controls.Add(checkBoxInvisible, 0, 3);
            tableLayoutPlace.SetColumnSpan(checkBoxInvisible, 2);
            tableLayoutPlace.Controls.Add(checkBoxFlatten, 0, 4);
            tableLayoutPlace.SetColumnSpan(checkBoxFlatten, 2);
            tableLayoutPlace.Controls.Add(labelObstacles, 0, 5);
            tableLayoutPlace.Controls.Add(comboBoxObstacles, 1, 5);
            tableLayoutPlace.Controls.Add(labelPlacement, 0, 6);
            tableLayoutPlace.SetColumnSpan(labelPlacement, 2);
            tableLayoutPlace.Controls.Add(listBoxChecks, 0, 7);
            tableLayoutPlace.SetColumnSpan(listBoxChecks, 2);
            tableLayoutPlace.Controls.Add(listViewObstacles, 0, 8);
            tableLayoutPlace.SetColumnSpan(listViewObstacles, 2);
            tableLayoutPlace.Controls.Add(flowPanelPlace, 0, 9);
            tableLayoutPlace.SetColumnSpan(flowPanelPlace, 2);
            tableLayoutPlace.Controls.Add(labelKeys, 0, 10);
            tableLayoutPlace.SetColumnSpan(labelKeys, 2);
            tableLayoutPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPlace.Name = "tableLayoutPlace";
            tableLayoutPlace.RowCount = 11;
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // labelAnchor
            // 
            labelAnchor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelAnchor.AutoSize = true;
            labelAnchor.Name = "labelAnchor";
            labelAnchor.Text = "Anchor:";
            // 
            // comboBoxAnchor
            // 
            comboBoxAnchor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxAnchor.Items.AddRange(new object[] { "Center", "Upper left corner", "Origin of the template" });
            comboBoxAnchor.Name = "comboBoxAnchor";
            comboBoxAnchor.Size = new System.Drawing.Size(170, 23);
            comboBoxAnchor.SelectedIndexChanged += OnPlacementOptionChanged;
            // 
            // labelHeightMode
            // 
            labelHeightMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelHeightMode.AutoSize = true;
            labelHeightMode.Name = "labelHeightMode";
            labelHeightMode.Text = "Height:";
            // 
            // comboBoxHeightMode
            // 
            comboBoxHeightMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxHeightMode.Items.AddRange(new object[] { "Ground at anchor + offset", "Lowest ground + offset", "Highest ground + offset", "Average ground + offset", "Fixed z" });
            comboBoxHeightMode.Name = "comboBoxHeightMode";
            comboBoxHeightMode.Size = new System.Drawing.Size(170, 23);
            comboBoxHeightMode.SelectedIndexChanged += OnPlacementOptionChanged;
            // 
            // labelZValue
            // 
            labelZValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelZValue.AutoSize = true;
            labelZValue.Name = "labelZValue";
            labelZValue.Text = "Z offset / fixed z:";
            // 
            // numericZValue
            // 
            numericZValue.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericZValue.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericZValue.Name = "numericZValue";
            numericZValue.Size = new System.Drawing.Size(60, 23);
            numericZValue.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numericZValue.ValueChanged += OnPlacementOptionChanged;
            // 
            // checkBoxInvisible
            // 
            checkBoxInvisible.AutoSize = true;
            checkBoxInvisible.Name = "checkBoxInvisible";
            checkBoxInvisible.Text = "Include invisible parts (flag 0)";
            checkBoxInvisible.UseVisualStyleBackColor = true;
            checkBoxInvisible.CheckedChanged += OnPlacementOptionChanged;
            // 
            // checkBoxFlatten
            // 
            checkBoxFlatten.AutoSize = true;
            checkBoxFlatten.Name = "checkBoxFlatten";
            checkBoxFlatten.Text = "Flatten the ground below the building to the base z";
            checkBoxFlatten.UseVisualStyleBackColor = true;
            checkBoxFlatten.CheckedChanged += OnPlacementOptionChanged;
            // 
            // labelObstacles
            // 
            labelObstacles.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelObstacles.AutoSize = true;
            labelObstacles.Name = "labelObstacles";
            labelObstacles.Text = "Remove in the way:";
            // 
            // comboBoxObstacles
            // 
            comboBoxObstacles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxObstacles.Items.AddRange(new object[] { "All statics", "Only trees, plants, rocks", "Nothing (choose in the list)" });
            comboBoxObstacles.Name = "comboBoxObstacles";
            comboBoxObstacles.Size = new System.Drawing.Size(170, 23);
            comboBoxObstacles.SelectedIndexChanged += OnObstacleFilterChanged;
            // 
            // labelPlacement
            // 
            labelPlacement.AutoSize = true;
            labelPlacement.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelPlacement.Name = "labelPlacement";
            labelPlacement.Text = "-";
            // 
            // listBoxChecks
            // 
            listBoxChecks.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxChecks.HorizontalScrollbar = true;
            listBoxChecks.IntegralHeight = false;
            listBoxChecks.Name = "listBoxChecks";
            listBoxChecks.Size = new System.Drawing.Size(380, 64);
            // 
            // listViewObstacles
            // 
            listViewObstacles.CheckBoxes = true;
            listViewObstacles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnObsX, columnObsY, columnObsZ, columnObsId, columnObsName });
            listViewObstacles.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewObstacles.FullRowSelect = true;
            listViewObstacles.GridLines = true;
            listViewObstacles.Name = "listViewObstacles";
            listViewObstacles.Size = new System.Drawing.Size(380, 140);
            listViewObstacles.UseCompatibleStateImageBehavior = false;
            listViewObstacles.View = System.Windows.Forms.View.Details;
            toolTip.SetToolTip(listViewObstacles, "Statics in the way. Checked entries are removed when the building is placed.");
            // 
            // columnObsX
            // 
            columnObsX.Text = "X";
            columnObsX.Width = 45;
            // 
            // columnObsY
            // 
            columnObsY.Text = "Y";
            columnObsY.Width = 45;
            // 
            // columnObsZ
            // 
            columnObsZ.Text = "Z";
            columnObsZ.Width = 35;
            // 
            // columnObsId
            // 
            columnObsId.Text = "ID";
            columnObsId.Width = 55;
            // 
            // columnObsName
            // 
            columnObsName.Text = "Name";
            columnObsName.Width = 170;
            // 
            // flowPanelPlace
            // 
            flowPanelPlace.AutoSize = true;
            flowPanelPlace.Controls.Add(buttonPlaceMode);
            flowPanelPlace.Controls.Add(buttonPlaceNow);
            flowPanelPlace.Controls.Add(buttonCancelPlace);
            flowPanelPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelPlace.Name = "flowPanelPlace";
            // 
            // buttonPlaceMode
            // 
            buttonPlaceMode.AutoSize = true;
            buttonPlaceMode.Name = "buttonPlaceMode";
            buttonPlaceMode.Text = "Start placing";
            buttonPlaceMode.UseVisualStyleBackColor = true;
            buttonPlaceMode.Click += OnClickPlaceMode;
            buttonPlaceMode.Enabled = false;
            toolTip.SetToolTip(buttonPlaceMode, "The building follows the mouse, a click fixes the position");
            // 
            // buttonPlaceNow
            // 
            buttonPlaceNow.AutoSize = true;
            buttonPlaceNow.Name = "buttonPlaceNow";
            buttonPlaceNow.Text = "Place (Enter)";
            buttonPlaceNow.UseVisualStyleBackColor = true;
            buttonPlaceNow.Click += OnClickPlaceNow;
            buttonPlaceNow.Enabled = false;
            toolTip.SetToolTip(buttonPlaceNow, "Writes the building into the map (in memory, undo with \"Undo\")");
            // 
            // buttonCancelPlace
            // 
            buttonCancelPlace.AutoSize = true;
            buttonCancelPlace.Name = "buttonCancelPlace";
            buttonCancelPlace.Text = "Cancel (Esc)";
            buttonCancelPlace.UseVisualStyleBackColor = true;
            buttonCancelPlace.Click += OnClickCancelPlace;
            buttonCancelPlace.Enabled = false;
            // 
            // labelKeys
            // 
            labelKeys.AutoSize = true;
            labelKeys.ForeColor = System.Drawing.SystemColors.GrayText;
            labelKeys.MaximumSize = new System.Drawing.Size(400, 0);
            labelKeys.Name = "labelKeys";
            labelKeys.Text = "Mouse: move, click: fix the position (click again to follow the mouse). Arrows: X / Y. Page up / down: Z (Shift = 5). Enter: place, Esc: cancel.";
            // 
            // groupBoxArea
            // 
            groupBoxArea.AutoSize = true;
            groupBoxArea.Controls.Add(tableLayoutArea);
            groupBoxArea.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxArea.Name = "groupBoxArea";
            groupBoxArea.TabStop = false;
            groupBoxArea.Text = "Area of the map";
            // 
            // tableLayoutArea
            // 
            tableLayoutArea.AutoSize = true;
            tableLayoutArea.ColumnCount = 2;
            tableLayoutArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutArea.Controls.Add(buttonSelectArea, 0, 0);
            tableLayoutArea.Controls.Add(labelArea, 1, 0);
            tableLayoutArea.Controls.Add(labelAreaZ, 0, 1);
            tableLayoutArea.Controls.Add(flowPanelAreaZ, 1, 1);
            tableLayoutArea.Controls.Add(labelAreaFilter, 0, 2);
            tableLayoutArea.Controls.Add(comboBoxAreaFilter, 1, 2);
            tableLayoutArea.Controls.Add(flowPanelArea, 0, 3);
            tableLayoutArea.SetColumnSpan(flowPanelArea, 2);
            tableLayoutArea.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutArea.Name = "tableLayoutArea";
            tableLayoutArea.RowCount = 4;
            tableLayoutArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // 
            // buttonSelectArea
            // 
            buttonSelectArea.AutoSize = true;
            buttonSelectArea.Name = "buttonSelectArea";
            buttonSelectArea.Text = "Select area";
            buttonSelectArea.UseVisualStyleBackColor = true;
            buttonSelectArea.Click += OnClickSelectArea;
            toolTip.SetToolTip(buttonSelectArea, "Drag a rectangle in the view");
            // 
            // labelArea
            // 
            labelArea.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelArea.AutoSize = true;
            labelArea.Name = "labelArea";
            labelArea.Text = "no area selected";
            // 
            // labelAreaZ
            // 
            labelAreaZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelAreaZ.AutoSize = true;
            labelAreaZ.Name = "labelAreaZ";
            labelAreaZ.Text = "Statics z:";
            // 
            // flowPanelAreaZ
            // 
            flowPanelAreaZ.AutoSize = true;
            flowPanelAreaZ.Controls.Add(numericAreaMinZ);
            flowPanelAreaZ.Controls.Add(labelAreaTo);
            flowPanelAreaZ.Controls.Add(numericAreaMaxZ);
            flowPanelAreaZ.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelAreaZ.Name = "flowPanelAreaZ";
            // 
            // numericAreaMinZ
            // 
            numericAreaMinZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericAreaMinZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericAreaMinZ.Name = "numericAreaMinZ";
            numericAreaMinZ.Size = new System.Drawing.Size(60, 23);
            numericAreaMinZ.Value = new decimal(new int[] { 128, 0, 0, -2147483648 });
            // 
            // labelAreaTo
            // 
            labelAreaTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelAreaTo.AutoSize = true;
            labelAreaTo.Name = "labelAreaTo";
            labelAreaTo.Text = "to";
            // 
            // numericAreaMaxZ
            // 
            numericAreaMaxZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericAreaMaxZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericAreaMaxZ.Name = "numericAreaMaxZ";
            numericAreaMaxZ.Size = new System.Drawing.Size(60, 23);
            numericAreaMaxZ.Value = new decimal(new int[] { 127, 0, 0, 0 });
            // 
            // labelAreaFilter
            // 
            labelAreaFilter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelAreaFilter.AutoSize = true;
            labelAreaFilter.Name = "labelAreaFilter";
            labelAreaFilter.Text = "Statics:";
            // 
            // comboBoxAreaFilter
            // 
            comboBoxAreaFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxAreaFilter.Items.AddRange(new object[] { "All statics", "Only trees, plants, rocks" });
            comboBoxAreaFilter.Name = "comboBoxAreaFilter";
            comboBoxAreaFilter.Size = new System.Drawing.Size(170, 23);
            comboBoxAreaFilter.SelectedIndexChanged += OnAreaFilterChanged;
            // 
            // flowPanelArea
            // 
            flowPanelArea.AutoSize = true;
            flowPanelArea.Controls.Add(buttonAreaRemove);
            flowPanelArea.Controls.Add(buttonAreaFlatten);
            flowPanelArea.Controls.Add(numericFlattenZ);
            flowPanelArea.Controls.Add(buttonAreaExport);
            flowPanelArea.Controls.Add(buttonAreaLibrary);
            flowPanelArea.Controls.Add(buttonAreaMulti);
            flowPanelArea.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelArea.Name = "flowPanelArea";
            // 
            // buttonAreaRemove
            // 
            buttonAreaRemove.AutoSize = true;
            buttonAreaRemove.Name = "buttonAreaRemove";
            buttonAreaRemove.Text = "Remove statics";
            buttonAreaRemove.UseVisualStyleBackColor = true;
            buttonAreaRemove.Click += OnClickAreaRemove;
            buttonAreaRemove.Enabled = false;
            toolTip.SetToolTip(buttonAreaRemove, "Removes the statics of the area (z range and filter)");
            // 
            // buttonAreaFlatten
            // 
            buttonAreaFlatten.AutoSize = true;
            buttonAreaFlatten.Name = "buttonAreaFlatten";
            buttonAreaFlatten.Text = "Flatten land to z:";
            buttonAreaFlatten.UseVisualStyleBackColor = true;
            buttonAreaFlatten.Click += OnClickAreaFlatten;
            buttonAreaFlatten.Enabled = false;
            // 
            // numericFlattenZ
            // 
            numericFlattenZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericFlattenZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericFlattenZ.Name = "numericFlattenZ";
            numericFlattenZ.Size = new System.Drawing.Size(60, 23);
            numericFlattenZ.Value = new decimal(new int[] { 0, 0, 0, 0 });
            // 
            // buttonAreaExport
            // 
            buttonAreaExport.AutoSize = true;
            buttonAreaExport.Name = "buttonAreaExport";
            buttonAreaExport.Text = "Export / script...";
            buttonAreaExport.UseVisualStyleBackColor = true;
            buttonAreaExport.Click += OnClickAreaExport;
            buttonAreaExport.Enabled = false;
            toolTip.SetToolTip(buttonAreaExport, "Preview, choose the items, then file (TXT, UOA, ...), library, multi.mul or ServUO script");
            // 
            // buttonAreaLibrary
            // 
            buttonAreaLibrary.AutoSize = true;
            buttonAreaLibrary.Name = "buttonAreaLibrary";
            buttonAreaLibrary.Text = "To library...";
            buttonAreaLibrary.UseVisualStyleBackColor = true;
            buttonAreaLibrary.Click += OnClickAreaLibrary;
            buttonAreaLibrary.Enabled = false;
            toolTip.SetToolTip(buttonAreaLibrary, "Stores the statics of the area in the building library");
            // 
            // buttonAreaMulti
            // 
            buttonAreaMulti.AutoSize = true;
            buttonAreaMulti.Name = "buttonAreaMulti";
            buttonAreaMulti.Text = "To multi.mul...";
            buttonAreaMulti.UseVisualStyleBackColor = true;
            buttonAreaMulti.Click += OnClickAreaMulti;
            buttonAreaMulti.Enabled = false;
            toolTip.SetToolTip(buttonAreaMulti, "Stores the statics of the area as entry of multi.mul");
            //
            // toolStripButtonRedo
            //
            toolStripButtonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRedo.Enabled = false;
            toolStripButtonRedo.Name = "toolStripButtonRedo";
            toolStripButtonRedo.Size = new System.Drawing.Size(38, 22);
            toolStripButtonRedo.Text = "Redo";
            toolStripButtonRedo.ToolTipText = "Redo (Ctrl+Y)";
            toolStripButtonRedo.Click += OnClickRedo;
            // 
            // toolStripButtonUndoLast
            // 
            toolStripButtonUndoLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonUndoLast.Enabled = false;
            toolStripButtonUndoLast.Name = "toolStripButtonUndoLast";
            toolStripButtonUndoLast.Text = "Undo";
            toolStripButtonUndoLast.ToolTipText = "Undo the last change (Ctrl+Z)";
            toolStripButtonUndoLast.Click += OnClickUndoLast;
            // 
            // toolStripButtonBackups
            // 
            toolStripButtonBackups.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonBackups.Name = "toolStripButtonBackups";
            toolStripButtonBackups.Text = "Backups...";
            toolStripButtonBackups.ToolTipText = "Restore map files from a backup";
            toolStripButtonBackups.Click += OnClickBackups;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusLabelCell, statusLabelRender, statusLabelChanges });
            statusStrip.Location = new System.Drawing.Point(0, 739);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1264, 22);
            statusStrip.TabIndex = 2;
            //
            // statusLabelCell
            //
            statusLabelCell.AutoSize = false;
            statusLabelCell.Name = "statusLabelCell";
            statusLabelCell.Size = new System.Drawing.Size(360, 17);
            statusLabelCell.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusLabelRender
            //
            statusLabelRender.Name = "statusLabelRender";
            statusLabelRender.Size = new System.Drawing.Size(745, 17);
            statusLabelRender.Spring = true;
            statusLabelRender.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // statusLabelChanges
            //
            statusLabelChanges.ForeColor = System.Drawing.Color.DarkRed;
            statusLabelChanges.Name = "statusLabelChanges";
            statusLabelChanges.Size = new System.Drawing.Size(144, 17);
            //
            // MapRegionViewerForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1264, 761);
            Controls.Add(splitContainerMain);
            Controls.Add(toolStripMain);
            Controls.Add(statusStrip);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(900, 500);
            Name = "MapRegionViewerForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Region Viewer";
            FormClosing += OnFormClosing;
            Shown += OnShown;
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            contextMenuView.ResumeLayout(false);
            tabControlSide.ResumeLayout(false);
            tabPageTile.ResumeLayout(false);
            tabPageTile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            flowPanelTileButtons.ResumeLayout(false);
            flowPanelTileButtons.PerformLayout();
            tabPageIssues.ResumeLayout(false);
            tabPageIssues.PerformLayout();
            panelIssueTop.ResumeLayout(false);
            panelIssueTop.PerformLayout();
            flowPanelIssueButtons.ResumeLayout(false);
            flowPanelIssueButtons.PerformLayout();
            tabPageView.ResumeLayout(false);
            tabPageView.PerformLayout();
            tableLayoutView.ResumeLayout(false);
            tableLayoutView.PerformLayout();
            groupBoxDisplay.ResumeLayout(false);
            groupBoxDisplay.PerformLayout();
            tableLayoutDisplay.ResumeLayout(false);
            tableLayoutDisplay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarMaxZ).EndInit();
            groupBoxCheck.ResumeLayout(false);
            groupBoxCheck.PerformLayout();
            tableLayoutCheck.ResumeLayout(false);
            tableLayoutCheck.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericSteep).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericFloating).EndInit();
            groupBoxCache.ResumeLayout(false);
            groupBoxCache.PerformLayout();
            tableLayoutCache.ResumeLayout(false);
            tableLayoutCache.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericCacheMb).EndInit();
            tabPageBuild.ResumeLayout(false);
            tabPageBuild.PerformLayout();
            panelBuildScroll.ResumeLayout(false);
            panelBuildScroll.PerformLayout();
            tableLayoutBuild.ResumeLayout(false);
            tableLayoutBuild.PerformLayout();
            groupBoxMulti.ResumeLayout(false);
            groupBoxMulti.PerformLayout();
            tableLayoutMulti.ResumeLayout(false);
            tableLayoutMulti.PerformLayout();
            flowPanelMulti.ResumeLayout(false);
            flowPanelMulti.PerformLayout();
            groupBoxPlace.ResumeLayout(false);
            groupBoxPlace.PerformLayout();
            tableLayoutPlace.ResumeLayout(false);
            tableLayoutPlace.PerformLayout();
            flowPanelPlace.ResumeLayout(false);
            flowPanelPlace.PerformLayout();
            groupBoxArea.ResumeLayout(false);
            groupBoxArea.PerformLayout();
            tableLayoutArea.ResumeLayout(false);
            tableLayoutArea.PerformLayout();
            flowPanelAreaZ.ResumeLayout(false);
            flowPanelAreaZ.PerformLayout();
            flowPanelArea.ResumeLayout(false);
            flowPanelArea.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericZValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericAreaMinZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericAreaMaxZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericFlattenZ).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonHelp;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButtonFit;
        private System.Windows.Forms.ToolStripLabel toolStripLabelZoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonLand;
        private System.Windows.Forms.ToolStripButton toolStripButtonStatics;
        private System.Windows.Forms.ToolStripButton toolStripButtonStretch;
        private System.Windows.Forms.ToolStripButton toolStripButtonHeight;
        private System.Windows.Forms.ToolStripButton toolStripButtonGrid;
        private System.Windows.Forms.ToolStripButton toolStripButtonIssueOverlay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonCheck;
        private System.Windows.Forms.ToolStripButton toolStripButtonFixAll;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndo;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonReload;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowOnMap;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private IsoViewControl isoView;
        private System.Windows.Forms.ContextMenuStrip contextMenuView;
        private System.Windows.Forms.ToolStripMenuItem markErrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCoordinatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnMapToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControlSide;
        private System.Windows.Forms.TabPage tabPageTile;
        private System.Windows.Forms.ListView listViewTile;
        private System.Windows.Forms.ColumnHeader columnTileKind;
        private System.Windows.Forms.ColumnHeader columnTileId;
        private System.Windows.Forms.ColumnHeader columnTileName;
        private System.Windows.Forms.ColumnHeader columnTileZ;
        private System.Windows.Forms.ColumnHeader columnTileHeight;
        private System.Windows.Forms.ColumnHeader columnTileHue;
        private System.Windows.Forms.ColumnHeader columnTileFlags;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelCellHeader;
        private System.Windows.Forms.FlowLayoutPanel flowPanelTileButtons;
        private System.Windows.Forms.Button buttonRemoveStatic;
        private System.Windows.Forms.Button buttonZUp;
        private System.Windows.Forms.Button buttonZDown;
        private System.Windows.Forms.Button buttonMarkIssue;
        private System.Windows.Forms.TabPage tabPageIssues;
        private System.Windows.Forms.ListView listViewIssues;
        private System.Windows.Forms.ColumnHeader columnIssueStatus;
        private System.Windows.Forms.ColumnHeader columnIssueSeverity;
        private System.Windows.Forms.ColumnHeader columnIssueType;
        private System.Windows.Forms.ColumnHeader columnIssueX;
        private System.Windows.Forms.ColumnHeader columnIssueY;
        private System.Windows.Forms.ColumnHeader columnIssueZ;
        private System.Windows.Forms.ColumnHeader columnIssueId;
        private System.Windows.Forms.ColumnHeader columnIssueDescription;
        private System.Windows.Forms.ColumnHeader columnIssueNote;
        private System.Windows.Forms.TableLayoutPanel panelIssueTop;
        private System.Windows.Forms.Label labelIssueFilter;
        private System.Windows.Forms.ComboBox comboBoxIssueFilter;
        private System.Windows.Forms.Label labelIssueSummary;
        private System.Windows.Forms.FlowLayoutPanel flowPanelIssueButtons;
        private System.Windows.Forms.Button buttonFixChecked;
        private System.Windows.Forms.Button buttonCheckFixable;
        private System.Windows.Forms.Button buttonIgnore;
        private System.Windows.Forms.Button buttonReopen;
        private System.Windows.Forms.Button buttonNote;
        private System.Windows.Forms.Button buttonDeleteIssue;
        private System.Windows.Forms.Button buttonExportCsv;
        private System.Windows.Forms.Button buttonSaveProtocol;
        private System.Windows.Forms.Button buttonLoadProtocol;
        private System.Windows.Forms.TabPage tabPageView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutView;
        private System.Windows.Forms.GroupBox groupBoxDisplay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutDisplay;
        private System.Windows.Forms.Label labelMaxZ;
        private System.Windows.Forms.TrackBar trackBarMaxZ;
        private System.Windows.Forms.CheckBox checkBoxNoDraw;
        private System.Windows.Forms.CheckBox checkBoxLighting;
        private System.Windows.Forms.GroupBox groupBoxCheck;
        private System.Windows.Forms.TableLayoutPanel tableLayoutCheck;
        private System.Windows.Forms.CheckBox checkBoxCheckIds;
        private System.Windows.Forms.CheckBox checkBoxCheckDuplicates;
        private System.Windows.Forms.CheckBox checkBoxCheckBelow;
        private System.Windows.Forms.CheckBox checkBoxCheckSteep;
        private System.Windows.Forms.NumericUpDown numericSteep;
        private System.Windows.Forms.CheckBox checkBoxCheckFloating;
        private System.Windows.Forms.NumericUpDown numericFloating;
        private System.Windows.Forms.GroupBox groupBoxCache;
        private System.Windows.Forms.TableLayoutPanel tableLayoutCache;
        private System.Windows.Forms.Label labelCacheMb;
        private System.Windows.Forms.NumericUpDown numericCacheMb;
        private System.Windows.Forms.CheckBox checkBoxDiskCache;
        private System.Windows.Forms.Button buttonClearCache;
        private System.Windows.Forms.Label labelCacheInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelCell;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelRender;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelChanges;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TabPage tabPageBuild;
        private System.Windows.Forms.Panel panelBuildScroll;
        private System.Windows.Forms.TableLayoutPanel tableLayoutBuild;
        private System.Windows.Forms.GroupBox groupBoxMulti;
        private System.Windows.Forms.TableLayoutPanel tableLayoutMulti;
        private System.Windows.Forms.Label labelTemplate;
        private System.Windows.Forms.FlowLayoutPanel flowPanelMulti;
        private System.Windows.Forms.Button buttonOpenBrowser;
        private System.Windows.Forms.Button buttonImportTemplate;
        private System.Windows.Forms.Button buttonNormalizeZ;
        private System.Windows.Forms.Button buttonClearTemplate;
        private System.Windows.Forms.GroupBox groupBoxPlace;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPlace;
        private System.Windows.Forms.Label labelAnchor;
        private System.Windows.Forms.ComboBox comboBoxAnchor;
        private System.Windows.Forms.Label labelHeightMode;
        private System.Windows.Forms.ComboBox comboBoxHeightMode;
        private System.Windows.Forms.Label labelZValue;
        private System.Windows.Forms.NumericUpDown numericZValue;
        private System.Windows.Forms.CheckBox checkBoxInvisible;
        private System.Windows.Forms.CheckBox checkBoxFlatten;
        private System.Windows.Forms.Label labelObstacles;
        private System.Windows.Forms.ComboBox comboBoxObstacles;
        private System.Windows.Forms.Label labelPlacement;
        private System.Windows.Forms.ListBox listBoxChecks;
        private System.Windows.Forms.ListView listViewObstacles;
        private System.Windows.Forms.ColumnHeader columnObsX;
        private System.Windows.Forms.ColumnHeader columnObsY;
        private System.Windows.Forms.ColumnHeader columnObsZ;
        private System.Windows.Forms.ColumnHeader columnObsId;
        private System.Windows.Forms.ColumnHeader columnObsName;
        private System.Windows.Forms.FlowLayoutPanel flowPanelPlace;
        private System.Windows.Forms.Button buttonPlaceMode;
        private System.Windows.Forms.Button buttonPlaceNow;
        private System.Windows.Forms.Button buttonCancelPlace;
        private System.Windows.Forms.Label labelKeys;
        private System.Windows.Forms.GroupBox groupBoxArea;
        private System.Windows.Forms.TableLayoutPanel tableLayoutArea;
        private System.Windows.Forms.Button buttonSelectArea;
        private System.Windows.Forms.Label labelArea;
        private System.Windows.Forms.Label labelAreaZ;
        private System.Windows.Forms.FlowLayoutPanel flowPanelAreaZ;
        private System.Windows.Forms.NumericUpDown numericAreaMinZ;
        private System.Windows.Forms.Label labelAreaTo;
        private System.Windows.Forms.NumericUpDown numericAreaMaxZ;
        private System.Windows.Forms.Label labelAreaFilter;
        private System.Windows.Forms.ComboBox comboBoxAreaFilter;
        private System.Windows.Forms.FlowLayoutPanel flowPanelArea;
        private System.Windows.Forms.Button buttonAreaRemove;
        private System.Windows.Forms.Button buttonAreaFlatten;
        private System.Windows.Forms.NumericUpDown numericFlattenZ;
        private System.Windows.Forms.Button buttonAreaExport;
        private System.Windows.Forms.Button buttonAreaLibrary;
        private System.Windows.Forms.Button buttonAreaMulti;
        private System.Windows.Forms.ToolStripButton toolStripButtonUndoLast;
        private System.Windows.Forms.ToolStripButton toolStripButtonRedo;
        private System.Windows.Forms.ToolStripButton toolStripButtonBackups;
    }
}

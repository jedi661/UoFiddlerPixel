/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    partial class EcMapControl
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
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripLabelMap = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxFacet = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelOverview = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxZoom = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelGoTo = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxGoTo = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonShading = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownSector = new System.Windows.Forms.ToolStripDropDownButton();
            exportSectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importSectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorSector = new System.Windows.Forms.ToolStripSeparator();
            clearStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            overviewPanel = new EcMapOverviewPanel();
            splitContainerRight = new System.Windows.Forms.SplitContainer();
            panelSectorScroll = new System.Windows.Forms.Panel();
            sectorPanel = new EcMapSectorPanel();
            contextMenuStripSector = new System.Windows.Forms.ContextMenuStrip(components);
            copyTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            addStaticMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            exportSectorMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importSectorMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelInspector = new System.Windows.Forms.Panel();
            previewPanelStatic = new EcPreviewPanel();
            flowLayoutPanelStaticButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonAddStatic = new System.Windows.Forms.Button();
            buttonEditStatic = new System.Windows.Forms.Button();
            buttonRemoveStatic = new System.Windows.Forms.Button();
            listViewStatics = new System.Windows.Forms.ListView();
            columnHeaderStaticItem = new System.Windows.Forms.ColumnHeader();
            columnHeaderStaticZ = new System.Windows.Forms.ColumnHeader();
            columnHeaderStaticHue = new System.Windows.Forms.ColumnHeader();
            contextMenuStripStatics = new System.Windows.Forms.ContextMenuStrip(components);
            addStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            labelStatics = new System.Windows.Forms.Label();
            flowLayoutPanelLayerButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonRemoveLayer = new System.Windows.Forms.Button();
            listViewLayers = new System.Windows.Forms.ListView();
            columnHeaderBlend = new System.Windows.Forms.ColumnHeader();
            columnHeaderLayerZ = new System.Windows.Forms.ColumnHeader();
            columnHeaderLayerLand = new System.Windows.Forms.ColumnHeader();
            labelLayers = new System.Windows.Forms.Label();
            flowLayoutPanelTileButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonCopyTile = new System.Windows.Forms.Button();
            buttonPasteTile = new System.Windows.Forms.Button();
            flowLayoutPanelLand = new System.Windows.Forms.FlowLayoutPanel();
            labelZ = new System.Windows.Forms.Label();
            numericUpDownZ = new System.Windows.Forms.NumericUpDown();
            labelLand = new System.Windows.Forms.Label();
            textBoxLand = new System.Windows.Forms.TextBox();
            buttonApplyLand = new System.Windows.Forms.Button();
            labelTileInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
            panelSectorScroll.SuspendLayout();
            contextMenuStripSector.SuspendLayout();
            panelInspector.SuspendLayout();
            flowLayoutPanelStaticButtons.SuspendLayout();
            contextMenuStripStatics.SuspendLayout();
            flowLayoutPanelLayerButtons.SuspendLayout();
            flowLayoutPanelTileButtons.SuspendLayout();
            flowLayoutPanelLand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownZ).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelMap, toolStripComboBoxFacet, toolStripLabelOverview, toolStripComboBoxZoom, toolStripSeparator1, toolStripLabelGoTo, toolStripTextBoxGoTo, toolStripButtonShading, toolStripSeparator2, toolStripDropDownSector, toolStripSeparator3, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelMap
            //
            toolStripLabelMap.Name = "toolStripLabelMap";
            toolStripLabelMap.Text = "Map:";
            //
            // toolStripComboBoxFacet
            //
            toolStripComboBoxFacet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFacet.Name = "toolStripComboBoxFacet";
            toolStripComboBoxFacet.Size = new System.Drawing.Size(140, 25);
            toolStripComboBoxFacet.ToolTipText = "Map file";
            toolStripComboBoxFacet.SelectedIndexChanged += OnFacetChanged;
            //
            // toolStripLabelOverview
            //
            toolStripLabelOverview.Name = "toolStripLabelOverview";
            toolStripLabelOverview.Text = "Overview:";
            //
            // toolStripComboBoxZoom
            //
            toolStripComboBoxZoom.Items.AddRange(new object[] { "1:1", "1:2", "1:4", "1:8" });
            toolStripComboBoxZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxZoom.Name = "toolStripComboBoxZoom";
            toolStripComboBoxZoom.Size = new System.Drawing.Size(70, 25);
            toolStripComboBoxZoom.ToolTipText = "Size of the overview";
            toolStripComboBoxZoom.SelectedIndexChanged += OnZoomChanged;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripLabelGoTo
            //
            toolStripLabelGoTo.Name = "toolStripLabelGoTo";
            toolStripLabelGoTo.Text = "Go to x,y:";
            //
            // toolStripTextBoxGoTo
            //
            toolStripTextBoxGoTo.Name = "toolStripTextBoxGoTo";
            toolStripTextBoxGoTo.Size = new System.Drawing.Size(90, 25);
            toolStripTextBoxGoTo.ToolTipText = "x,y – Enter";
            toolStripTextBoxGoTo.KeyDown += OnGoToKeyDown;
            //
            // toolStripButtonShading
            //
            toolStripButtonShading.CheckOnClick = true;
            toolStripButtonShading.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonShading.Checked = true;
            toolStripButtonShading.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonShading.Name = "toolStripButtonShading";
            toolStripButtonShading.Text = "Height shading";
            toolStripButtonShading.CheckedChanged += OnShadingChanged;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripDropDownSector
            //
            toolStripDropDownSector.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportSectorToolStripMenuItem, importSectorToolStripMenuItem, toolStripSeparatorSector, clearStaticsToolStripMenuItem });
            toolStripDropDownSector.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownSector.Name = "toolStripDropDownSector";
            toolStripDropDownSector.Text = "Sector";
            //
            // exportSectorToolStripMenuItem
            //
            exportSectorToolStripMenuItem.Name = "exportSectorToolStripMenuItem";
            exportSectorToolStripMenuItem.Text = "Export sector (.bin)...";
            exportSectorToolStripMenuItem.Click += OnClickExportSector;
            //
            // importSectorToolStripMenuItem
            //
            importSectorToolStripMenuItem.Name = "importSectorToolStripMenuItem";
            importSectorToolStripMenuItem.Text = "Import sector (.bin)...";
            importSectorToolStripMenuItem.Click += OnClickImportSector;
            //
            // toolStripSeparatorSector
            //
            toolStripSeparatorSector.Name = "toolStripSeparatorSector";
            //
            // clearStaticsToolStripMenuItem
            //
            clearStaticsToolStripMenuItem.Name = "clearStaticsToolStripMenuItem";
            clearStaticsToolStripMenuItem.Text = "Remove all statics of the sector";
            clearStaticsToolStripMenuItem.Click += OnClickClearStatics;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // splitContainerMain
            //
            splitContainerMain.Panel1.Controls.Add(overviewPanel);
            splitContainerMain.Panel2.Controls.Add(splitContainerRight);
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Size = new System.Drawing.Size(900, 450);
            splitContainerMain.SplitterDistance = 350;
            splitContainerMain.TabIndex = 1;
            //
            // overviewPanel
            //
            overviewPanel.AutoScroll = true;
            overviewPanel.BackColor = System.Drawing.Color.Black;
            overviewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            overviewPanel.Name = "overviewPanel";
            overviewPanel.TabIndex = 0;
            //
            // splitContainerRight
            //
            splitContainerRight.Panel1.Controls.Add(panelSectorScroll);
            splitContainerRight.Panel2.Controls.Add(panelInspector);
            splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainerRight.Name = "splitContainerRight";
            splitContainerRight.Size = new System.Drawing.Size(546, 450);
            splitContainerRight.SplitterDistance = 280;
            splitContainerRight.TabIndex = 1;
            //
            // panelSectorScroll
            //
            panelSectorScroll.Controls.Add(sectorPanel);
            panelSectorScroll.AutoScroll = true;
            panelSectorScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            panelSectorScroll.Name = "panelSectorScroll";
            panelSectorScroll.TabIndex = 0;
            //
            // sectorPanel
            //
            sectorPanel.ContextMenuStrip = contextMenuStripSector;
            sectorPanel.Location = new System.Drawing.Point(0, 0);
            sectorPanel.Name = "sectorPanel";
            sectorPanel.Size = new System.Drawing.Size(576, 576);
            sectorPanel.TabIndex = 0;
            //
            // contextMenuStripSector
            //
            contextMenuStripSector.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyTileToolStripMenuItem, pasteTileToolStripMenuItem, toolStripSeparatorMenu1, addStaticMenuToolStripMenuItem, toolStripSeparatorMenu2, exportSectorMenuToolStripMenuItem, importSectorMenuToolStripMenuItem, toolStripSeparatorMenu3, saveToolStripMenuItem });
            contextMenuStripSector.Name = "contextMenuStripSector";
            //
            // copyTileToolStripMenuItem
            //
            copyTileToolStripMenuItem.Name = "copyTileToolStripMenuItem";
            copyTileToolStripMenuItem.Text = "Copy tile";
            copyTileToolStripMenuItem.Click += OnClickCopyTile;
            //
            // pasteTileToolStripMenuItem
            //
            pasteTileToolStripMenuItem.Name = "pasteTileToolStripMenuItem";
            pasteTileToolStripMenuItem.Text = "Paste tile";
            pasteTileToolStripMenuItem.Click += OnClickPasteTile;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // addStaticMenuToolStripMenuItem
            //
            addStaticMenuToolStripMenuItem.Name = "addStaticMenuToolStripMenuItem";
            addStaticMenuToolStripMenuItem.Text = "Add static...";
            addStaticMenuToolStripMenuItem.Click += OnClickAddStatic;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // exportSectorMenuToolStripMenuItem
            //
            exportSectorMenuToolStripMenuItem.Name = "exportSectorMenuToolStripMenuItem";
            exportSectorMenuToolStripMenuItem.Text = "Export sector (.bin)...";
            exportSectorMenuToolStripMenuItem.Click += OnClickExportSector;
            //
            // importSectorMenuToolStripMenuItem
            //
            importSectorMenuToolStripMenuItem.Name = "importSectorMenuToolStripMenuItem";
            importSectorMenuToolStripMenuItem.Text = "Import sector (.bin)...";
            importSectorMenuToolStripMenuItem.Click += OnClickImportSector;
            //
            // toolStripSeparatorMenu3
            //
            toolStripSeparatorMenu3.Name = "toolStripSeparatorMenu3";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelInspector
            //
            panelInspector.Controls.Add(previewPanelStatic);
            panelInspector.Controls.Add(flowLayoutPanelStaticButtons);
            panelInspector.Controls.Add(listViewStatics);
            panelInspector.Controls.Add(labelStatics);
            panelInspector.Controls.Add(flowLayoutPanelLayerButtons);
            panelInspector.Controls.Add(listViewLayers);
            panelInspector.Controls.Add(labelLayers);
            panelInspector.Controls.Add(flowLayoutPanelTileButtons);
            panelInspector.Controls.Add(flowLayoutPanelLand);
            panelInspector.Controls.Add(labelTileInfo);
            panelInspector.Dock = System.Windows.Forms.DockStyle.Fill;
            panelInspector.Name = "panelInspector";
            panelInspector.TabIndex = 0;
            //
            // previewPanelStatic
            //
            previewPanelStatic.AutoScroll = true;
            previewPanelStatic.BackColor = System.Drawing.Color.White;
            previewPanelStatic.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanelStatic.Name = "previewPanelStatic";
            previewPanelStatic.TabIndex = 9;
            //
            // flowLayoutPanelStaticButtons
            //
            flowLayoutPanelStaticButtons.Controls.Add(buttonAddStatic);
            flowLayoutPanelStaticButtons.Controls.Add(buttonEditStatic);
            flowLayoutPanelStaticButtons.Controls.Add(buttonRemoveStatic);
            flowLayoutPanelStaticButtons.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelStaticButtons.Name = "flowLayoutPanelStaticButtons";
            flowLayoutPanelStaticButtons.Size = new System.Drawing.Size(256, 32);
            flowLayoutPanelStaticButtons.TabIndex = 8;
            //
            // buttonAddStatic
            //
            buttonAddStatic.AutoSize = true;
            buttonAddStatic.Name = "buttonAddStatic";
            buttonAddStatic.TabIndex = 0;
            buttonAddStatic.Text = "Add...";
            buttonAddStatic.UseVisualStyleBackColor = true;
            buttonAddStatic.Click += OnClickAddStatic;
            //
            // buttonEditStatic
            //
            buttonEditStatic.AutoSize = true;
            buttonEditStatic.Name = "buttonEditStatic";
            buttonEditStatic.TabIndex = 1;
            buttonEditStatic.Text = "Edit...";
            buttonEditStatic.UseVisualStyleBackColor = true;
            buttonEditStatic.Click += OnClickEditStatic;
            //
            // buttonRemoveStatic
            //
            buttonRemoveStatic.AutoSize = true;
            buttonRemoveStatic.Name = "buttonRemoveStatic";
            buttonRemoveStatic.TabIndex = 2;
            buttonRemoveStatic.Text = "Remove";
            buttonRemoveStatic.UseVisualStyleBackColor = true;
            buttonRemoveStatic.Click += OnClickRemoveStatic;
            //
            // listViewStatics
            //
            listViewStatics.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderStaticItem, columnHeaderStaticZ, columnHeaderStaticHue });
            listViewStatics.ContextMenuStrip = contextMenuStripStatics;
            listViewStatics.Dock = System.Windows.Forms.DockStyle.Top;
            listViewStatics.FullRowSelect = true;
            listViewStatics.HideSelection = false;
            listViewStatics.MultiSelect = false;
            listViewStatics.Name = "listViewStatics";
            listViewStatics.Size = new System.Drawing.Size(256, 140);
            listViewStatics.TabIndex = 7;
            listViewStatics.UseCompatibleStateImageBehavior = false;
            listViewStatics.View = System.Windows.Forms.View.Details;
            listViewStatics.SelectedIndexChanged += OnStaticSelectedIndexChanged;
            listViewStatics.DoubleClick += OnClickEditStatic;
            //
            // columnHeaderStaticItem
            //
            columnHeaderStaticItem.Text = "Item";
            columnHeaderStaticItem.Width = 70;
            //
            // columnHeaderStaticZ
            //
            columnHeaderStaticZ.Text = "z";
            columnHeaderStaticZ.Width = 40;
            //
            // columnHeaderStaticHue
            //
            columnHeaderStaticHue.Text = "Hue";
            columnHeaderStaticHue.Width = 60;
            //
            // contextMenuStripStatics
            //
            contextMenuStripStatics.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addStaticToolStripMenuItem, editStaticToolStripMenuItem, removeStaticToolStripMenuItem });
            contextMenuStripStatics.Name = "contextMenuStripStatics";
            //
            // addStaticToolStripMenuItem
            //
            addStaticToolStripMenuItem.Name = "addStaticToolStripMenuItem";
            addStaticToolStripMenuItem.Text = "Add...";
            addStaticToolStripMenuItem.Click += OnClickAddStatic;
            //
            // editStaticToolStripMenuItem
            //
            editStaticToolStripMenuItem.Name = "editStaticToolStripMenuItem";
            editStaticToolStripMenuItem.Text = "Edit...";
            editStaticToolStripMenuItem.Click += OnClickEditStatic;
            //
            // removeStaticToolStripMenuItem
            //
            removeStaticToolStripMenuItem.Name = "removeStaticToolStripMenuItem";
            removeStaticToolStripMenuItem.Text = "Remove";
            removeStaticToolStripMenuItem.Click += OnClickRemoveStatic;
            //
            // labelStatics
            //
            labelStatics.Dock = System.Windows.Forms.DockStyle.Top;
            labelStatics.Name = "labelStatics";
            labelStatics.Size = new System.Drawing.Size(256, 18);
            labelStatics.Text = "Statics";
            //
            // flowLayoutPanelLayerButtons
            //
            flowLayoutPanelLayerButtons.Controls.Add(buttonRemoveLayer);
            flowLayoutPanelLayerButtons.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelLayerButtons.Name = "flowLayoutPanelLayerButtons";
            flowLayoutPanelLayerButtons.Size = new System.Drawing.Size(256, 32);
            flowLayoutPanelLayerButtons.TabIndex = 5;
            //
            // buttonRemoveLayer
            //
            buttonRemoveLayer.AutoSize = true;
            buttonRemoveLayer.Name = "buttonRemoveLayer";
            buttonRemoveLayer.TabIndex = 0;
            buttonRemoveLayer.Text = "Remove layer";
            buttonRemoveLayer.UseVisualStyleBackColor = true;
            buttonRemoveLayer.Click += OnClickRemoveLayer;
            //
            // listViewLayers
            //
            listViewLayers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderBlend, columnHeaderLayerZ, columnHeaderLayerLand });
            listViewLayers.Dock = System.Windows.Forms.DockStyle.Top;
            listViewLayers.FullRowSelect = true;
            listViewLayers.HideSelection = false;
            listViewLayers.Name = "listViewLayers";
            listViewLayers.Size = new System.Drawing.Size(256, 90);
            listViewLayers.TabIndex = 4;
            listViewLayers.UseCompatibleStateImageBehavior = false;
            listViewLayers.View = System.Windows.Forms.View.Details;
            //
            // columnHeaderBlend
            //
            columnHeaderBlend.Text = "Blend";
            columnHeaderBlend.Width = 50;
            //
            // columnHeaderLayerZ
            //
            columnHeaderLayerZ.Text = "z";
            columnHeaderLayerZ.Width = 40;
            //
            // columnHeaderLayerLand
            //
            columnHeaderLayerLand.Text = "Land";
            columnHeaderLayerLand.Width = 70;
            //
            // labelLayers
            //
            labelLayers.Dock = System.Windows.Forms.DockStyle.Top;
            labelLayers.Name = "labelLayers";
            labelLayers.Size = new System.Drawing.Size(256, 18);
            labelLayers.Text = "Blend layers";
            //
            // flowLayoutPanelTileButtons
            //
            flowLayoutPanelTileButtons.Controls.Add(buttonCopyTile);
            flowLayoutPanelTileButtons.Controls.Add(buttonPasteTile);
            flowLayoutPanelTileButtons.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelTileButtons.Name = "flowLayoutPanelTileButtons";
            flowLayoutPanelTileButtons.Size = new System.Drawing.Size(256, 32);
            flowLayoutPanelTileButtons.TabIndex = 2;
            //
            // buttonCopyTile
            //
            buttonCopyTile.AutoSize = true;
            buttonCopyTile.Name = "buttonCopyTile";
            buttonCopyTile.TabIndex = 0;
            buttonCopyTile.Text = "Copy tile";
            buttonCopyTile.UseVisualStyleBackColor = true;
            buttonCopyTile.Click += OnClickCopyTile;
            //
            // buttonPasteTile
            //
            buttonPasteTile.AutoSize = true;
            buttonPasteTile.Name = "buttonPasteTile";
            buttonPasteTile.TabIndex = 1;
            buttonPasteTile.Text = "Paste tile";
            buttonPasteTile.UseVisualStyleBackColor = true;
            buttonPasteTile.Click += OnClickPasteTile;
            //
            // flowLayoutPanelLand
            //
            flowLayoutPanelLand.Controls.Add(labelZ);
            flowLayoutPanelLand.Controls.Add(numericUpDownZ);
            flowLayoutPanelLand.Controls.Add(labelLand);
            flowLayoutPanelLand.Controls.Add(textBoxLand);
            flowLayoutPanelLand.Controls.Add(buttonApplyLand);
            flowLayoutPanelLand.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelLand.Name = "flowLayoutPanelLand";
            flowLayoutPanelLand.Size = new System.Drawing.Size(256, 34);
            flowLayoutPanelLand.TabIndex = 1;
            //
            // labelZ
            //
            labelZ.AutoSize = true;
            labelZ.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            labelZ.Name = "labelZ";
            labelZ.Text = "z:";
            //
            // numericUpDownZ
            //
            numericUpDownZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numericUpDownZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numericUpDownZ.Name = "numericUpDownZ";
            numericUpDownZ.Size = new System.Drawing.Size(60, 23);
            numericUpDownZ.TabIndex = 1;
            //
            // labelLand
            //
            labelLand.AutoSize = true;
            labelLand.Margin = new System.Windows.Forms.Padding(6, 8, 0, 0);
            labelLand.Name = "labelLand";
            labelLand.Text = "Land:";
            //
            // textBoxLand
            //
            textBoxLand.Name = "textBoxLand";
            textBoxLand.Size = new System.Drawing.Size(70, 23);
            textBoxLand.TabIndex = 3;
            //
            // buttonApplyLand
            //
            buttonApplyLand.Name = "buttonApplyLand";
            buttonApplyLand.Size = new System.Drawing.Size(60, 23);
            buttonApplyLand.TabIndex = 4;
            buttonApplyLand.Text = "Apply";
            buttonApplyLand.UseVisualStyleBackColor = true;
            buttonApplyLand.Click += OnClickApplyLand;
            //
            // labelTileInfo
            //
            labelTileInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelTileInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelTileInfo.Name = "labelTileInfo";
            labelTileInfo.Padding = new System.Windows.Forms.Padding(4);
            labelTileInfo.Size = new System.Drawing.Size(256, 40);
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 2;
            //
            // toolStripStatusLabel
            //
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Spring = true;
            toolStripStatusLabel.Text = "Choose the Enhanced Client folder on top.";
            toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // EcMapControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcMapControl";
            Size = new System.Drawing.Size(1000, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
            panelSectorScroll.ResumeLayout(false);
            panelSectorScroll.PerformLayout();
            contextMenuStripSector.ResumeLayout(false);
            panelInspector.ResumeLayout(false);
            panelInspector.PerformLayout();
            flowLayoutPanelStaticButtons.ResumeLayout(false);
            flowLayoutPanelStaticButtons.PerformLayout();
            contextMenuStripStatics.ResumeLayout(false);
            flowLayoutPanelLayerButtons.ResumeLayout(false);
            flowLayoutPanelLayerButtons.PerformLayout();
            flowLayoutPanelTileButtons.ResumeLayout(false);
            flowLayoutPanelTileButtons.PerformLayout();
            flowLayoutPanelLand.ResumeLayout(false);
            flowLayoutPanelLand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownZ).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelMap;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFacet;
        private System.Windows.Forms.ToolStripLabel toolStripLabelOverview;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxZoom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelGoTo;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxGoTo;
        private System.Windows.Forms.ToolStripButton toolStripButtonShading;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownSector;
        private System.Windows.Forms.ToolStripMenuItem exportSectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSector;
        private System.Windows.Forms.ToolStripMenuItem clearStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private EcMapOverviewPanel overviewPanel;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.Panel panelSectorScroll;
        private EcMapSectorPanel sectorPanel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSector;
        private System.Windows.Forms.ToolStripMenuItem copyTileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteTileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem addStaticMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem exportSectorMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importSectorMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelInspector;
        private EcPreviewPanel previewPanelStatic;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelStaticButtons;
        private System.Windows.Forms.Button buttonAddStatic;
        private System.Windows.Forms.Button buttonEditStatic;
        private System.Windows.Forms.Button buttonRemoveStatic;
        private System.Windows.Forms.ListView listViewStatics;
        private System.Windows.Forms.ColumnHeader columnHeaderStaticItem;
        private System.Windows.Forms.ColumnHeader columnHeaderStaticZ;
        private System.Windows.Forms.ColumnHeader columnHeaderStaticHue;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripStatics;
        private System.Windows.Forms.ToolStripMenuItem addStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeStaticToolStripMenuItem;
        private System.Windows.Forms.Label labelStatics;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLayerButtons;
        private System.Windows.Forms.Button buttonRemoveLayer;
        private System.Windows.Forms.ListView listViewLayers;
        private System.Windows.Forms.ColumnHeader columnHeaderBlend;
        private System.Windows.Forms.ColumnHeader columnHeaderLayerZ;
        private System.Windows.Forms.ColumnHeader columnHeaderLayerLand;
        private System.Windows.Forms.Label labelLayers;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTileButtons;
        private System.Windows.Forms.Button buttonCopyTile;
        private System.Windows.Forms.Button buttonPasteTile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLand;
        private System.Windows.Forms.Label labelZ;
        private System.Windows.Forms.NumericUpDown numericUpDownZ;
        private System.Windows.Forms.Label labelLand;
        private System.Windows.Forms.TextBox textBoxLand;
        private System.Windows.Forms.Button buttonApplyLand;
        private System.Windows.Forms.Label labelTileInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

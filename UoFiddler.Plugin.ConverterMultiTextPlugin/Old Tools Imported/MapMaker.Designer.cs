// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class MapMaker
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapMaker));
            menuStrip = new System.Windows.Forms.MenuStrip();
            miFile = new System.Windows.Forms.ToolStripMenuItem();
            miOpenProject = new System.Windows.Forms.ToolStripMenuItem();
            miLoadMap = new System.Windows.Forms.ToolStripMenuItem();
            miSaveMap = new System.Windows.Forms.ToolStripMenuItem();
            miSep1 = new System.Windows.Forms.ToolStripSeparator();
            miOpenEditor = new System.Windows.Forms.ToolStripMenuItem();
            miSep2 = new System.Windows.Forms.ToolStripSeparator();
            miClose = new System.Windows.Forms.ToolStripMenuItem();
            miTools = new System.Windows.Forms.ToolStripMenuItem();
            createDataIndexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            createStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            xMLHexSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            transitionEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            miSep3 = new System.Windows.Forms.ToolStripSeparator();
            miExportPalettes = new System.Windows.Forms.ToolStripMenuItem();
            miHelp = new System.Windows.Forms.ToolStripMenuItem();
            miManual = new System.Windows.Forms.ToolStripMenuItem();
            groupProject = new System.Windows.Forms.GroupBox();
            tableProject = new System.Windows.Forms.TableLayoutPanel();
            lblProject = new System.Windows.Forms.Label();
            txtProject = new System.Windows.Forms.TextBox();
            btnBrowseProject = new System.Windows.Forms.Button();
            btnOpenProjectFolder = new System.Windows.Forms.Button();
            lblTerrainFile = new System.Windows.Forms.Label();
            txtTerrainFile = new System.Windows.Forms.TextBox();
            lblMapSize = new System.Windows.Forms.Label();
            cbMapSize = new System.Windows.Forms.ComboBox();
            lblSize = new System.Windows.Forms.Label();
            flowSize = new System.Windows.Forms.FlowLayoutPanel();
            numWidth = new System.Windows.Forms.NumericUpDown();
            lblSizeX = new System.Windows.Forms.Label();
            numHeight = new System.Windows.Forms.NumericUpDown();
            lblMapNumber = new System.Windows.Forms.Label();
            numMapNumber = new System.Windows.Forms.NumericUpDown();
            lblAltitudeFile = new System.Windows.Forms.Label();
            txtAltitudeFile = new System.Windows.Forms.TextBox();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            progressBar = new System.Windows.Forms.ToolStripProgressBar();
            splitMain = new System.Windows.Forms.SplitContainer();
            splitWork = new System.Windows.Forms.SplitContainer();
            groupPreview = new System.Windows.Forms.GroupBox();
            picPreview = new System.Windows.Forms.PictureBox();
            tablePreviewBottom = new System.Windows.Forms.TableLayoutPanel();
            flowPreviewMode = new System.Windows.Forms.FlowLayoutPanel();
            rbPrevTerrain = new System.Windows.Forms.RadioButton();
            rbPrevAltitude = new System.Windows.Forms.RadioButton();
            rbPrevRelief = new System.Windows.Forms.RadioButton();
            lblMapInfo = new System.Windows.Forms.Label();
            btnEditMap = new System.Windows.Forms.Button();
            btnSaveMap = new System.Windows.Forms.Button();
            btnLoadMap = new System.Windows.Forms.Button();
            btnSaveMapAs = new System.Windows.Forms.Button();
            tabControl = new System.Windows.Forms.TabControl();
            tabBlank = new System.Windows.Forms.TabPage();
            tableBlank = new System.Windows.Forms.TableLayoutPanel();
            lblBlankInfo = new System.Windows.Forms.Label();
            lblDefaultTerrain = new System.Windows.Forms.Label();
            cbDefaultTerrain = new System.Windows.Forms.ComboBox();
            chkBlankTerrainAltitude = new System.Windows.Forms.CheckBox();
            lblBlankAltitude = new System.Windows.Forms.Label();
            numBlankAltitude = new System.Windows.Forms.NumericUpDown();
            chkBlankDungeon = new System.Windows.Forms.CheckBox();
            btnCreateBlank = new System.Windows.Forms.Button();
            tabGenerator = new System.Windows.Forms.TabPage();
            splitGenerator = new System.Windows.Forms.SplitContainer();
            propertyGenerator = new System.Windows.Forms.PropertyGrid();
            tableGenRight = new System.Windows.Forms.TableLayoutPanel();
            groupBiomes = new System.Windows.Forms.GroupBox();
            gridBiomes = new System.Windows.Forms.DataGridView();
            flowBiomeButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnAddBiome = new System.Windows.Forms.Button();
            btnRemoveBiome = new System.Windows.Forms.Button();
            btnBiomeUp = new System.Windows.Forms.Button();
            btnBiomeDown = new System.Windows.Forms.Button();
            groupSketch = new System.Windows.Forms.GroupBox();
            tableSketch = new System.Windows.Forms.TableLayoutPanel();
            lblSketchInfo = new System.Windows.Forms.Label();
            btnLoadSketch = new System.Windows.Forms.Button();
            btnClearSketch = new System.Windows.Forms.Button();
            lblHeightTemplateInfo = new System.Windows.Forms.Label();
            btnLoadHeightTemplate = new System.Windows.Forms.Button();
            btnClearHeightTemplate = new System.Windows.Forms.Button();
            lblSketchHelp = new System.Windows.Forms.Label();
            groupPresets = new System.Windows.Forms.GroupBox();
            flowPresets = new System.Windows.Forms.FlowLayoutPanel();
            btnRandomSeed = new System.Windows.Forms.Button();
            btnPresetLoad = new System.Windows.Forms.Button();
            btnPresetSave = new System.Windows.Forms.Button();
            btnPresetDefaults = new System.Windows.Forms.Button();
            groupGenerate = new System.Windows.Forms.GroupBox();
            flowGenerate = new System.Windows.Forms.FlowLayoutPanel();
            btnPreview = new System.Windows.Forms.Button();
            btnGenerate = new System.Windows.Forms.Button();
            chkRepairAfterGenerate = new System.Windows.Forms.CheckBox();
            chkSaveAfterGenerate = new System.Windows.Forms.CheckBox();
            tabTemplates = new System.Windows.Forms.TabPage();
            tableTemplateTop = new System.Windows.Forms.TableLayoutPanel();
            lblTemplateFolder = new System.Windows.Forms.Label();
            txtTemplateFolder = new System.Windows.Forms.TextBox();
            btnTemplateBrowse = new System.Windows.Forms.Button();
            btnTemplateRefresh = new System.Windows.Forms.Button();
            btnTemplateOpenFolder = new System.Windows.Forms.Button();
            splitTemplates = new System.Windows.Forms.SplitContainer();
            listTemplates = new System.Windows.Forms.ListView();
            tableTemplateRight = new System.Windows.Forms.TableLayoutPanel();
            picTemplate = new System.Windows.Forms.PictureBox();
            lblTemplateInfo = new System.Windows.Forms.Label();
            lblTemplateUse = new System.Windows.Forms.Label();
            flowTemplateUse = new System.Windows.Forms.FlowLayoutPanel();
            rbTplConvert = new System.Windows.Forms.RadioButton();
            rbTplSketch = new System.Windows.Forms.RadioButton();
            rbTplHeight = new System.Windows.Forms.RadioButton();
            rbTplStamp = new System.Windows.Forms.RadioButton();
            lblScaleMode = new System.Windows.Forms.Label();
            cbScaleMode = new System.Windows.Forms.ComboBox();
            lblTemplateFill = new System.Windows.Forms.Label();
            cbTemplateFill = new System.Windows.Forms.ComboBox();
            chkNaturalOnly = new System.Windows.Forms.CheckBox();
            btnTemplateApply = new System.Windows.Forms.Button();
            lblTemplateHelp = new System.Windows.Forms.Label();
            tabHeights = new System.Windows.Forms.TabPage();
            tableHeights = new System.Windows.Forms.TableLayoutPanel();
            lblHeightsInfo = new System.Windows.Forms.Label();
            lblFilterType = new System.Windows.Forms.Label();
            cbFilterType = new System.Windows.Forms.ComboBox();
            lblFilterTerrain = new System.Windows.Forms.Label();
            cbFilterTerrain = new System.Windows.Forms.ComboBox();
            lblOffset = new System.Windows.Forms.Label();
            numOffset = new System.Windows.Forms.NumericUpDown();
            btnOffset = new System.Windows.Forms.Button();
            lblScale = new System.Windows.Forms.Label();
            numScale = new System.Windows.Forms.NumericUpDown();
            btnScale = new System.Windows.Forms.Button();
            lblSmooth = new System.Windows.Forms.Label();
            numSmooth = new System.Windows.Forms.NumericUpDown();
            btnSmooth = new System.Windows.Forms.Button();
            lblRoughen = new System.Windows.Forms.Label();
            numRoughAmp = new System.Windows.Forms.NumericUpDown();
            numRoughSize = new System.Windows.Forms.NumericUpDown();
            btnRoughen = new System.Windows.Forms.Button();
            lblClamp = new System.Windows.Forms.Label();
            numClampMin = new System.Windows.Forms.NumericUpDown();
            numClampMax = new System.Windows.Forms.NumericUpDown();
            btnClamp = new System.Windows.Forms.Button();
            lblProfile = new System.Windows.Forms.Label();
            numProfileSlope = new System.Windows.Forms.NumericUpDown();
            numProfileMax = new System.Windows.Forms.NumericUpDown();
            btnProfile = new System.Windows.Forms.Button();
            lblProfileTerrain = new System.Windows.Forms.Label();
            cbProfileTerrain = new System.Windows.Forms.ComboBox();
            lblResetAltitude = new System.Windows.Forms.Label();
            btnResetAltitude = new System.Windows.Forms.Button();
            btnHeightUndo = new System.Windows.Forms.Button();
            lblHeightStats = new System.Windows.Forms.Label();
            tabBuild = new System.Windows.Forms.TabPage();
            tableBuild = new System.Windows.Forms.TableLayoutPanel();
            lblBuildInfo = new System.Windows.Forms.Label();
            lblOutputFolder = new System.Windows.Forms.Label();
            txtOutputFolder = new System.Windows.Forms.TextBox();
            btnOutputBrowse = new System.Windows.Forms.Button();
            chkRandomStatics = new System.Windows.Forms.CheckBox();
            chkImportStatics = new System.Windows.Forms.CheckBox();
            txtImportFolder = new System.Windows.Forms.TextBox();
            btnImportBrowse = new System.Windows.Forms.Button();
            lblCompileSeed = new System.Windows.Forms.Label();
            numCompileSeed = new System.Windows.Forms.NumericUpDown();
            chkRepairBeforeBuild = new System.Windows.Forms.CheckBox();
            flowBuildButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCheckTransitions = new System.Windows.Forms.Button();
            btnRepairTransitions = new System.Windows.Forms.Button();
            btnBuildMul = new System.Windows.Forms.Button();
            lblBuildHelp = new System.Windows.Forms.Label();
            tabPageDataViewer = new System.Windows.Forms.TabPage();
            MainMenu1 = new System.Windows.Forms.MenuStrip();
            MenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            terrainACTToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            terrainACOToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            MenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            AltitudeACTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            AltitudeACOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tableDataViewer = new System.Windows.Forms.TableLayoutPanel();
            lbTerrainList = new System.Windows.Forms.Label();
            lbTileProperties = new System.Windows.Forms.Label();
            ListBox1 = new System.Windows.Forms.ListBox();
            PropertyGrid1 = new System.Windows.Forms.PropertyGrid();
            PictureBox1 = new System.Windows.Forms.PictureBox();
            tabHelp = new System.Windows.Forms.TabPage();
            rtbHelp = new System.Windows.Forms.RichTextBox();
            groupLog = new System.Windows.Forms.GroupBox();
            txtLog = new System.Windows.Forms.TextBox();
            flowLogButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnClearLog = new System.Windows.Forms.Button();
            btnSaveLog = new System.Windows.Forms.Button();
            toolTip = new System.Windows.Forms.ToolTip(components);
            menuStrip.SuspendLayout();
            groupProject.SuspendLayout();
            tableProject.SuspendLayout();
            flowSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMapNumber).BeginInit();
            statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitWork).BeginInit();
            splitWork.Panel1.SuspendLayout();
            splitWork.Panel2.SuspendLayout();
            splitWork.SuspendLayout();
            groupPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            tablePreviewBottom.SuspendLayout();
            flowPreviewMode.SuspendLayout();
            tabControl.SuspendLayout();
            tabBlank.SuspendLayout();
            tableBlank.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBlankAltitude).BeginInit();
            tabGenerator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitGenerator).BeginInit();
            splitGenerator.Panel1.SuspendLayout();
            splitGenerator.Panel2.SuspendLayout();
            splitGenerator.SuspendLayout();
            tableGenRight.SuspendLayout();
            groupBiomes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridBiomes).BeginInit();
            flowBiomeButtons.SuspendLayout();
            groupSketch.SuspendLayout();
            tableSketch.SuspendLayout();
            groupPresets.SuspendLayout();
            flowPresets.SuspendLayout();
            groupGenerate.SuspendLayout();
            flowGenerate.SuspendLayout();
            tabTemplates.SuspendLayout();
            tableTemplateTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitTemplates).BeginInit();
            splitTemplates.Panel1.SuspendLayout();
            splitTemplates.Panel2.SuspendLayout();
            splitTemplates.SuspendLayout();
            tableTemplateRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picTemplate).BeginInit();
            flowTemplateUse.SuspendLayout();
            tabHeights.SuspendLayout();
            tableHeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSmooth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRoughAmp).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRoughSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numClampMin).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numClampMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numProfileSlope).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numProfileMax).BeginInit();
            tabBuild.SuspendLayout();
            tableBuild.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCompileSeed).BeginInit();
            flowBuildButtons.SuspendLayout();
            tabPageDataViewer.SuspendLayout();
            MainMenu1.SuspendLayout();
            tableDataViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            tabHelp.SuspendLayout();
            groupLog.SuspendLayout();
            flowLogButtons.SuspendLayout();
            SuspendLayout();
            //
            // menuStrip
            //
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miFile, miTools, miHelp });
            menuStrip.Dock = System.Windows.Forms.DockStyle.Top;
            menuStrip.Text = "menuStrip";
            menuStrip.Name = "menuStrip";
            //
            // miFile
            //
            miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miOpenProject, miLoadMap, miSaveMap, miSep1, miOpenEditor, miSep2, miClose });
            miFile.Text = "&File";
            miFile.Name = "miFile";
            //
            // miOpenProject
            //
            miOpenProject.Text = "Open project folder...";
            miOpenProject.Name = "miOpenProject";
            miOpenProject.Click += BtnBrowseProject_Click;
            //
            // miLoadMap
            //
            miLoadMap.Text = "Load map from project";
            miLoadMap.Name = "miLoadMap";
            miLoadMap.Click += BtnLoadMap_Click;
            //
            // miSaveMap
            //
            miSaveMap.Text = "Save map to project";
            miSaveMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            miSaveMap.Name = "miSaveMap";
            miSaveMap.Click += BtnSaveMap_Click;
            //
            // miSep1
            //
            miSep1.Name = "miSep1";
            //
            // miOpenEditor
            //
            miOpenEditor.Text = "Open map editor...";
            miOpenEditor.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            miOpenEditor.Name = "miOpenEditor";
            miOpenEditor.Click += BtnEditMap_Click;
            //
            // miSep2
            //
            miSep2.Name = "miSep2";
            //
            // miClose
            //
            miClose.Text = "Close";
            miClose.Name = "miClose";
            miClose.Click += MiClose_Click;
            //
            // miTools
            //
            miTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { createDataIndexToolStripMenuItem, createStaticToolStripMenuItem, xMLHexSearchToolStripMenuItem, transitionEditorToolStripMenuItem, miSep3, miExportPalettes });
            miTools.Text = "&Tools";
            miTools.Name = "miTools";
            //
            // createDataIndexToolStripMenuItem
            //
            createDataIndexToolStripMenuItem.Text = "Create Data Index";
            createDataIndexToolStripMenuItem.Name = "createDataIndexToolStripMenuItem";
            createDataIndexToolStripMenuItem.Click += createDataIndexToolStripMenuItem_Click;
            //
            // createStaticToolStripMenuItem
            //
            createStaticToolStripMenuItem.Text = "Create Static";
            createStaticToolStripMenuItem.Name = "createStaticToolStripMenuItem";
            createStaticToolStripMenuItem.Click += createStaticToolStripMenuItem_Click;
            //
            // xMLHexSearchToolStripMenuItem
            //
            xMLHexSearchToolStripMenuItem.Text = "XML Hex Search";
            xMLHexSearchToolStripMenuItem.Name = "xMLHexSearchToolStripMenuItem";
            xMLHexSearchToolStripMenuItem.Click += xMLHexSearchToolStripMenuItem_Click;
            //
            // transitionEditorToolStripMenuItem
            //
            transitionEditorToolStripMenuItem.Text = "Transition Editor";
            transitionEditorToolStripMenuItem.Name = "transitionEditorToolStripMenuItem";
            transitionEditorToolStripMenuItem.Click += transitionEditorToolStripMenuItem_Click;
            //
            // miSep3
            //
            miSep3.Name = "miSep3";
            //
            // miExportPalettes
            //
            miExportPalettes.Text = "Export palettes for Photoshop (ACT / ACO)";
            miExportPalettes.Name = "miExportPalettes";
            miExportPalettes.Click += MiExportPalettes_Click;
            //
            // miHelp
            //
            miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miManual });
            miHelp.Text = "&Help";
            miHelp.Name = "miHelp";
            //
            // miManual
            //
            miManual.Text = "Manual (German)";
            miManual.ShortcutKeys = System.Windows.Forms.Keys.F1;
            miManual.Name = "miManual";
            miManual.Click += MiManual_Click;
            //
            // groupProject
            //
            groupProject.Controls.Add(tableProject);
            groupProject.Text = "Project";
            groupProject.Dock = System.Windows.Forms.DockStyle.Top;
            groupProject.Padding = new System.Windows.Forms.Padding(6);
            groupProject.AutoSize = true;
            groupProject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupProject.Name = "groupProject";
            //
            // tableProject
            //
            tableProject.Controls.Add(lblProject, 0, 0);
            tableProject.Controls.Add(txtProject, 1, 0);
            tableProject.SetColumnSpan(txtProject, 3);
            tableProject.Controls.Add(btnBrowseProject, 4, 0);
            tableProject.Controls.Add(btnOpenProjectFolder, 5, 0);
            tableProject.Controls.Add(lblTerrainFile, 6, 0);
            tableProject.Controls.Add(txtTerrainFile, 7, 0);
            tableProject.Controls.Add(lblMapSize, 0, 1);
            tableProject.Controls.Add(cbMapSize, 1, 1);
            tableProject.Controls.Add(lblSize, 2, 1);
            tableProject.Controls.Add(flowSize, 3, 1);
            tableProject.Controls.Add(lblMapNumber, 4, 1);
            tableProject.Controls.Add(numMapNumber, 5, 1);
            tableProject.Controls.Add(lblAltitudeFile, 6, 1);
            tableProject.Controls.Add(txtAltitudeFile, 7, 1);
            tableProject.ColumnCount = 8;
            tableProject.RowCount = 2;
            tableProject.Dock = System.Windows.Forms.DockStyle.Fill;
            tableProject.Padding = new System.Windows.Forms.Padding(6);
            tableProject.AutoSize = true;
            tableProject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableProject.Padding = new System.Windows.Forms.Padding(2);
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableProject.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableProject.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableProject.Name = "tableProject";
            //
            // lblProject
            //
            lblProject.Text = "Project folder:";
            lblProject.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProject.AutoSize = true;
            lblProject.Name = "lblProject";
            //
            // txtProject
            //
            txtProject.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtProject.Name = "txtProject";
            //
            // btnBrowseProject
            //
            btnBrowseProject.Text = "Browse...";
            btnBrowseProject.Size = new System.Drawing.Size(94, 28);
            btnBrowseProject.UseVisualStyleBackColor = true;
            btnBrowseProject.Name = "btnBrowseProject";
            btnBrowseProject.Click += BtnBrowseProject_Click;
            //
            // btnOpenProjectFolder
            //
            btnOpenProjectFolder.Text = "Open folder";
            btnOpenProjectFolder.Size = new System.Drawing.Size(94, 28);
            btnOpenProjectFolder.UseVisualStyleBackColor = true;
            btnOpenProjectFolder.Name = "btnOpenProjectFolder";
            btnOpenProjectFolder.Click += BtnOpenProjectFolder_Click;
            //
            // lblTerrainFile
            //
            lblTerrainFile.Text = "Terrain image:";
            lblTerrainFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTerrainFile.AutoSize = true;
            lblTerrainFile.Name = "lblTerrainFile";
            //
            // txtTerrainFile
            //
            txtTerrainFile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTerrainFile.Text = "Terrain.bmp";
            txtTerrainFile.Name = "txtTerrainFile";
            //
            // lblMapSize
            //
            lblMapSize.Text = "Map size:";
            lblMapSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMapSize.AutoSize = true;
            lblMapSize.Name = "lblMapSize";
            //
            // cbMapSize
            //
            cbMapSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbMapSize.Size = new System.Drawing.Size(300, 23);
            cbMapSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbMapSize.Name = "cbMapSize";
            cbMapSize.SelectedIndexChanged += CbMapSize_SelectedIndexChanged;
            //
            // lblSize
            //
            lblSize.Text = "Tiles:";
            lblSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSize.AutoSize = true;
            lblSize.Name = "lblSize";
            //
            // flowSize
            //
            flowSize.Controls.Add(numWidth);
            flowSize.Controls.Add(lblSizeX);
            flowSize.Controls.Add(numHeight);
            flowSize.AutoSize = true;
            flowSize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowSize.WrapContents = true;
            flowSize.Margin = new System.Windows.Forms.Padding(0);
            flowSize.Name = "flowSize";
            //
            // numWidth
            //
            numWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Maximum = new decimal(new int[] { 32760, 0, 0, 0 });
            numWidth.Value = new decimal(new int[] { 6144, 0, 0, 0 });
            numWidth.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Size = new System.Drawing.Size(72, 23);
            numWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numWidth.Name = "numWidth";
            //
            // lblSizeX
            //
            lblSizeX.Text = "x";
            lblSizeX.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSizeX.AutoSize = true;
            lblSizeX.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            lblSizeX.Name = "lblSizeX";
            //
            // numHeight
            //
            numHeight.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Maximum = new decimal(new int[] { 32760, 0, 0, 0 });
            numHeight.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            numHeight.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Size = new System.Drawing.Size(72, 23);
            numHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numHeight.Name = "numHeight";
            //
            // lblMapNumber
            //
            lblMapNumber.Text = "Map # (mul):";
            lblMapNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMapNumber.AutoSize = true;
            lblMapNumber.Name = "lblMapNumber";
            //
            // numMapNumber
            //
            numMapNumber.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numMapNumber.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numMapNumber.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numMapNumber.Increment = 1;
            numMapNumber.Size = new System.Drawing.Size(70, 23);
            numMapNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numMapNumber.Name = "numMapNumber";
            //
            // lblAltitudeFile
            //
            lblAltitudeFile.Text = "Altitude image:";
            lblAltitudeFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblAltitudeFile.AutoSize = true;
            lblAltitudeFile.Name = "lblAltitudeFile";
            //
            // txtAltitudeFile
            //
            txtAltitudeFile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtAltitudeFile.Text = "Altitude.bmp";
            txtAltitudeFile.Name = "txtAltitudeFile";
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus, progressBar });
            statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            statusStrip.Name = "statusStrip";
            //
            // lblStatus
            //
            lblStatus.Spring = true;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatus.Name = "lblStatus";
            //
            // progressBar
            //
            progressBar.Size = new System.Drawing.Size(220, 16);
            progressBar.Name = "progressBar";
            //
            // splitMain
            //
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitMain.Size = new System.Drawing.Size(1260, 760);
            splitMain.SplitterDistance = 560;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitMain.Name = "splitMain";
            splitMain.Panel1.Controls.Add(splitWork);
            splitMain.Panel2.Controls.Add(groupLog);
            //
            // splitWork
            //
            splitWork.Dock = System.Windows.Forms.DockStyle.Fill;
            splitWork.Size = new System.Drawing.Size(1260, 560);
            splitWork.SplitterDistance = 820;
            splitWork.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitWork.Name = "splitWork";
            splitWork.Panel1.Controls.Add(tabControl);
            splitWork.Panel2.Controls.Add(groupPreview);
            //
            // groupPreview
            //
            groupPreview.Controls.Add(picPreview);
            groupPreview.Controls.Add(tablePreviewBottom);
            groupPreview.Text = "Current map";
            groupPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            groupPreview.Padding = new System.Windows.Forms.Padding(6);
            groupPreview.Name = "groupPreview";
            //
            // picPreview
            //
            picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picPreview.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
            picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPreview.TabStop = false;
            picPreview.Name = "picPreview";
            picPreview.DoubleClick += BtnEditMap_Click;
            //
            // tablePreviewBottom
            //
            tablePreviewBottom.Controls.Add(flowPreviewMode, 0, 0);
            tablePreviewBottom.SetColumnSpan(flowPreviewMode, 2);
            tablePreviewBottom.Controls.Add(lblMapInfo, 0, 1);
            tablePreviewBottom.SetColumnSpan(lblMapInfo, 2);
            tablePreviewBottom.Controls.Add(btnEditMap, 0, 2);
            tablePreviewBottom.Controls.Add(btnSaveMap, 1, 2);
            tablePreviewBottom.Controls.Add(btnLoadMap, 0, 3);
            tablePreviewBottom.Controls.Add(btnSaveMapAs, 1, 3);
            tablePreviewBottom.ColumnCount = 2;
            tablePreviewBottom.RowCount = 4;
            tablePreviewBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            tablePreviewBottom.Padding = new System.Windows.Forms.Padding(6);
            tablePreviewBottom.AutoSize = true;
            tablePreviewBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tablePreviewBottom.Padding = new System.Windows.Forms.Padding(0);
            tablePreviewBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tablePreviewBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.Name = "tablePreviewBottom";
            //
            // flowPreviewMode
            //
            flowPreviewMode.Controls.Add(rbPrevTerrain);
            flowPreviewMode.Controls.Add(rbPrevAltitude);
            flowPreviewMode.Controls.Add(rbPrevRelief);
            flowPreviewMode.AutoSize = true;
            flowPreviewMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowPreviewMode.WrapContents = true;
            flowPreviewMode.Name = "flowPreviewMode";
            //
            // rbPrevTerrain
            //
            rbPrevTerrain.Text = "Terrain";
            rbPrevTerrain.AutoSize = true;
            rbPrevTerrain.Checked = true;
            rbPrevTerrain.Name = "rbPrevTerrain";
            rbPrevTerrain.CheckedChanged += PreviewMode_CheckedChanged;
            //
            // rbPrevAltitude
            //
            rbPrevAltitude.Text = "Altitude";
            rbPrevAltitude.AutoSize = true;
            rbPrevAltitude.Name = "rbPrevAltitude";
            rbPrevAltitude.CheckedChanged += PreviewMode_CheckedChanged;
            //
            // rbPrevRelief
            //
            rbPrevRelief.Text = "Relief";
            rbPrevRelief.AutoSize = true;
            rbPrevRelief.Name = "rbPrevRelief";
            rbPrevRelief.CheckedChanged += PreviewMode_CheckedChanged;
            //
            // lblMapInfo
            //
            lblMapInfo.Text = "No map loaded.";
            lblMapInfo.AutoSize = true;
            lblMapInfo.MaximumSize = new System.Drawing.Size(380, 0);
            lblMapInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            lblMapInfo.Name = "lblMapInfo";
            //
            // btnEditMap
            //
            btnEditMap.Text = "Open in editor";
            btnEditMap.Size = new System.Drawing.Size(170, 28);
            btnEditMap.UseVisualStyleBackColor = true;
            btnEditMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnEditMap.Name = "btnEditMap";
            btnEditMap.Click += BtnEditMap_Click;
            //
            // btnSaveMap
            //
            btnSaveMap.Text = "Save to project";
            btnSaveMap.Size = new System.Drawing.Size(170, 28);
            btnSaveMap.UseVisualStyleBackColor = true;
            btnSaveMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnSaveMap.Name = "btnSaveMap";
            btnSaveMap.Click += BtnSaveMap_Click;
            //
            // btnLoadMap
            //
            btnLoadMap.Text = "Load from project";
            btnLoadMap.Size = new System.Drawing.Size(170, 28);
            btnLoadMap.UseVisualStyleBackColor = true;
            btnLoadMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnLoadMap.Name = "btnLoadMap";
            btnLoadMap.Click += BtnLoadMap_Click;
            //
            // btnSaveMapAs
            //
            btnSaveMapAs.Text = "Export as PNG...";
            btnSaveMapAs.Size = new System.Drawing.Size(170, 28);
            btnSaveMapAs.UseVisualStyleBackColor = true;
            btnSaveMapAs.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnSaveMapAs.Name = "btnSaveMapAs";
            btnSaveMapAs.Click += BtnExportPng_Click;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabBlank);
            tabControl.Controls.Add(tabGenerator);
            tabControl.Controls.Add(tabTemplates);
            tabControl.Controls.Add(tabHeights);
            tabControl.Controls.Add(tabBuild);
            tabControl.Controls.Add(tabPageDataViewer);
            tabControl.Controls.Add(tabHelp);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Padding = new System.Drawing.Point(12, 5);
            tabControl.Name = "tabControl";
            //
            // tabBlank
            //
            tabBlank.Controls.Add(tableBlank);
            tabBlank.Text = "1. Blank map";
            tabBlank.Padding = new System.Windows.Forms.Padding(8);
            tabBlank.UseVisualStyleBackColor = true;
            tabBlank.Name = "tabBlank";
            //
            // tableBlank
            //
            tableBlank.Controls.Add(lblBlankInfo, 0, 0);
            tableBlank.SetColumnSpan(lblBlankInfo, 2);
            tableBlank.Controls.Add(lblDefaultTerrain, 0, 1);
            tableBlank.Controls.Add(cbDefaultTerrain, 1, 1);
            tableBlank.Controls.Add(chkBlankTerrainAltitude, 0, 2);
            tableBlank.SetColumnSpan(chkBlankTerrainAltitude, 2);
            tableBlank.Controls.Add(lblBlankAltitude, 0, 3);
            tableBlank.Controls.Add(numBlankAltitude, 1, 3);
            tableBlank.Controls.Add(chkBlankDungeon, 0, 4);
            tableBlank.SetColumnSpan(chkBlankDungeon, 2);
            tableBlank.Controls.Add(btnCreateBlank, 0, 5);
            tableBlank.ColumnCount = 2;
            tableBlank.RowCount = 6;
            tableBlank.Dock = System.Windows.Forms.DockStyle.Top;
            tableBlank.Padding = new System.Windows.Forms.Padding(6);
            tableBlank.AutoSize = true;
            tableBlank.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableBlank.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            tableBlank.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.Name = "tableBlank";
            //
            // lblBlankInfo
            //
            lblBlankInfo.Text = "Creates an empty map (Terrain.bmp + Altitude.bmp) with the size of the project bar. Paint it in the editor or use it as a base for templates.";
            lblBlankInfo.AutoSize = true;
            lblBlankInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblBlankInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            lblBlankInfo.Name = "lblBlankInfo";
            //
            // lblDefaultTerrain
            //
            lblDefaultTerrain.Text = "Fill terrain:";
            lblDefaultTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDefaultTerrain.AutoSize = true;
            lblDefaultTerrain.Name = "lblDefaultTerrain";
            //
            // cbDefaultTerrain
            //
            cbDefaultTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbDefaultTerrain.Size = new System.Drawing.Size(300, 23);
            cbDefaultTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbDefaultTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbDefaultTerrain.DropDownHeight = 400;
            cbDefaultTerrain.IntegralHeight = false;
            cbDefaultTerrain.Name = "cbDefaultTerrain";
            cbDefaultTerrain.DrawItem += UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit.TerrainChoice.DrawItem;
            //
            // chkBlankTerrainAltitude
            //
            chkBlankTerrainAltitude.Text = "Use the default altitude of the terrain";
            chkBlankTerrainAltitude.AutoSize = true;
            chkBlankTerrainAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBlankTerrainAltitude.Checked = true;
            chkBlankTerrainAltitude.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBlankTerrainAltitude.Name = "chkBlankTerrainAltitude";
            chkBlankTerrainAltitude.CheckedChanged += ChkBlankTerrainAltitude_CheckedChanged;
            //
            // lblBlankAltitude
            //
            lblBlankAltitude.Text = "Altitude (z):";
            lblBlankAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblBlankAltitude.AutoSize = true;
            lblBlankAltitude.Name = "lblBlankAltitude";
            //
            // numBlankAltitude
            //
            numBlankAltitude.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numBlankAltitude.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numBlankAltitude.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numBlankAltitude.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numBlankAltitude.Size = new System.Drawing.Size(90, 23);
            numBlankAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numBlankAltitude.Enabled = false;
            numBlankAltitude.Name = "numBlankAltitude";
            //
            // chkBlankDungeon
            //
            chkBlankDungeon.Text = "Fill the dungeon area (x >= dungeon start of the map size) with Blank";
            chkBlankDungeon.AutoSize = true;
            chkBlankDungeon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBlankDungeon.Checked = true;
            chkBlankDungeon.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBlankDungeon.Name = "chkBlankDungeon";
            //
            // btnCreateBlank
            //
            btnCreateBlank.Text = "Create blank map";
            btnCreateBlank.Size = new System.Drawing.Size(200, 28);
            btnCreateBlank.UseVisualStyleBackColor = true;
            btnCreateBlank.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            btnCreateBlank.Name = "btnCreateBlank";
            btnCreateBlank.Click += BtnCreateBlank_Click;
            //
            // tabGenerator
            //
            tabGenerator.Controls.Add(splitGenerator);
            tabGenerator.Text = "2. Generator";
            tabGenerator.Padding = new System.Windows.Forms.Padding(6);
            tabGenerator.UseVisualStyleBackColor = true;
            tabGenerator.Name = "tabGenerator";
            //
            // splitGenerator
            //
            splitGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            splitGenerator.Size = new System.Drawing.Size(800, 500);
            splitGenerator.SplitterDistance = 360;
            splitGenerator.Name = "splitGenerator";
            splitGenerator.Panel1.Controls.Add(propertyGenerator);
            splitGenerator.Panel2.Controls.Add(tableGenRight);
            //
            // propertyGenerator
            //
            propertyGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGenerator.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propertyGenerator.ToolbarVisible = false;
            propertyGenerator.Name = "propertyGenerator";
            //
            // tableGenRight
            //
            tableGenRight.Controls.Add(groupBiomes, 0, 0);
            tableGenRight.Controls.Add(groupSketch, 0, 1);
            tableGenRight.Controls.Add(groupPresets, 0, 2);
            tableGenRight.Controls.Add(groupGenerate, 0, 3);
            tableGenRight.ColumnCount = 1;
            tableGenRight.RowCount = 4;
            tableGenRight.Dock = System.Windows.Forms.DockStyle.Fill;
            tableGenRight.Padding = new System.Windows.Forms.Padding(6);
            tableGenRight.Padding = new System.Windows.Forms.Padding(0);
            tableGenRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.Name = "tableGenRight";
            //
            // groupBiomes
            //
            groupBiomes.Controls.Add(gridBiomes);
            groupBiomes.Controls.Add(flowBiomeButtons);
            groupBiomes.Text = "Biomes (random terrain patches on the land, applied from top to bottom)";
            groupBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBiomes.Padding = new System.Windows.Forms.Padding(6);
            groupBiomes.Name = "groupBiomes";
            //
            // gridBiomes
            //
            gridBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridBiomes.AllowUserToAddRows = false;
            gridBiomes.AllowUserToResizeRows = false;
            gridBiomes.AutoGenerateColumns = false;
            gridBiomes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gridBiomes.RowHeadersWidth = 24;
            gridBiomes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gridBiomes.BackgroundColor = System.Drawing.SystemColors.Window;
            gridBiomes.Name = "gridBiomes";
            gridBiomes.DataError += GridBiomes_DataError;
            gridBiomes.CurrentCellDirtyStateChanged += GridBiomes_CurrentCellDirtyStateChanged;
            //
            // flowBiomeButtons
            //
            flowBiomeButtons.Controls.Add(btnAddBiome);
            flowBiomeButtons.Controls.Add(btnRemoveBiome);
            flowBiomeButtons.Controls.Add(btnBiomeUp);
            flowBiomeButtons.Controls.Add(btnBiomeDown);
            flowBiomeButtons.AutoSize = true;
            flowBiomeButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowBiomeButtons.WrapContents = true;
            flowBiomeButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowBiomeButtons.Name = "flowBiomeButtons";
            //
            // btnAddBiome
            //
            btnAddBiome.Text = "Add biome";
            btnAddBiome.Size = new System.Drawing.Size(110, 28);
            btnAddBiome.UseVisualStyleBackColor = true;
            btnAddBiome.Name = "btnAddBiome";
            btnAddBiome.Click += BtnAddBiome_Click;
            //
            // btnRemoveBiome
            //
            btnRemoveBiome.Text = "Remove";
            btnRemoveBiome.Size = new System.Drawing.Size(90, 28);
            btnRemoveBiome.UseVisualStyleBackColor = true;
            btnRemoveBiome.Name = "btnRemoveBiome";
            btnRemoveBiome.Click += BtnRemoveBiome_Click;
            //
            // btnBiomeUp
            //
            btnBiomeUp.Text = "Up";
            btnBiomeUp.Size = new System.Drawing.Size(60, 28);
            btnBiomeUp.UseVisualStyleBackColor = true;
            btnBiomeUp.Name = "btnBiomeUp";
            btnBiomeUp.Click += BtnBiomeUp_Click;
            //
            // btnBiomeDown
            //
            btnBiomeDown.Text = "Down";
            btnBiomeDown.Size = new System.Drawing.Size(60, 28);
            btnBiomeDown.UseVisualStyleBackColor = true;
            btnBiomeDown.Name = "btnBiomeDown";
            btnBiomeDown.Click += BtnBiomeDown_Click;
            //
            // groupSketch
            //
            groupSketch.Controls.Add(tableSketch);
            groupSketch.Text = "Sketch / height map (optional)";
            groupSketch.Dock = System.Windows.Forms.DockStyle.Top;
            groupSketch.Padding = new System.Windows.Forms.Padding(6);
            groupSketch.AutoSize = true;
            groupSketch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupSketch.Name = "groupSketch";
            //
            // tableSketch
            //
            tableSketch.Controls.Add(lblSketchInfo, 0, 0);
            tableSketch.Controls.Add(btnLoadSketch, 1, 0);
            tableSketch.Controls.Add(btnClearSketch, 2, 0);
            tableSketch.Controls.Add(lblHeightTemplateInfo, 0, 1);
            tableSketch.Controls.Add(btnLoadHeightTemplate, 1, 1);
            tableSketch.Controls.Add(btnClearHeightTemplate, 2, 1);
            tableSketch.Controls.Add(lblSketchHelp, 0, 2);
            tableSketch.SetColumnSpan(lblSketchHelp, 3);
            tableSketch.ColumnCount = 3;
            tableSketch.RowCount = 3;
            tableSketch.Dock = System.Windows.Forms.DockStyle.Top;
            tableSketch.Padding = new System.Windows.Forms.Padding(6);
            tableSketch.AutoSize = true;
            tableSketch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableSketch.Padding = new System.Windows.Forms.Padding(0);
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.Name = "tableSketch";
            //
            // lblSketchInfo
            //
            lblSketchInfo.Text = "Sketch: none";
            lblSketchInfo.AutoSize = true;
            lblSketchInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSketchInfo.Name = "lblSketchInfo";
            //
            // btnLoadSketch
            //
            btnLoadSketch.Text = "Load sketch...";
            btnLoadSketch.Size = new System.Drawing.Size(140, 28);
            btnLoadSketch.UseVisualStyleBackColor = true;
            btnLoadSketch.Name = "btnLoadSketch";
            btnLoadSketch.Click += BtnLoadSketch_Click;
            //
            // btnClearSketch
            //
            btnClearSketch.Text = "Clear";
            btnClearSketch.Size = new System.Drawing.Size(70, 28);
            btnClearSketch.UseVisualStyleBackColor = true;
            btnClearSketch.Name = "btnClearSketch";
            btnClearSketch.Click += BtnClearSketch_Click;
            //
            // lblHeightTemplateInfo
            //
            lblHeightTemplateInfo.Text = "Height map: none";
            lblHeightTemplateInfo.AutoSize = true;
            lblHeightTemplateInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblHeightTemplateInfo.Name = "lblHeightTemplateInfo";
            //
            // btnLoadHeightTemplate
            //
            btnLoadHeightTemplate.Text = "Load height map...";
            btnLoadHeightTemplate.Size = new System.Drawing.Size(140, 28);
            btnLoadHeightTemplate.UseVisualStyleBackColor = true;
            btnLoadHeightTemplate.Name = "btnLoadHeightTemplate";
            btnLoadHeightTemplate.Click += BtnLoadHeightTemplate_Click;
            //
            // btnClearHeightTemplate
            //
            btnClearHeightTemplate.Text = "Clear";
            btnClearHeightTemplate.Size = new System.Drawing.Size(70, 28);
            btnClearHeightTemplate.UseVisualStyleBackColor = true;
            btnClearHeightTemplate.Name = "btnClearHeightTemplate";
            btnClearHeightTemplate.Click += BtnClearHeightTemplate_Click;
            //
            // lblSketchHelp
            //
            lblSketchHelp.Text = "Sketch = image painted with terrain colours (transparent / black / [00] No Draw = generator decides). Height map = grey scale image (dark = sea, bright = mountains). Both are scaled to the map size.";
            lblSketchHelp.AutoSize = true;
            lblSketchHelp.MaximumSize = new System.Drawing.Size(440, 0);
            lblSketchHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblSketchHelp.Name = "lblSketchHelp";
            //
            // groupPresets
            //
            groupPresets.Controls.Add(flowPresets);
            groupPresets.Text = "Presets";
            groupPresets.Dock = System.Windows.Forms.DockStyle.Top;
            groupPresets.Padding = new System.Windows.Forms.Padding(6);
            groupPresets.AutoSize = true;
            groupPresets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupPresets.Name = "groupPresets";
            //
            // flowPresets
            //
            flowPresets.Controls.Add(btnRandomSeed);
            flowPresets.Controls.Add(btnPresetLoad);
            flowPresets.Controls.Add(btnPresetSave);
            flowPresets.Controls.Add(btnPresetDefaults);
            flowPresets.AutoSize = true;
            flowPresets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowPresets.WrapContents = true;
            flowPresets.Dock = System.Windows.Forms.DockStyle.Top;
            flowPresets.Name = "flowPresets";
            //
            // btnRandomSeed
            //
            btnRandomSeed.Text = "New random seed";
            btnRandomSeed.Size = new System.Drawing.Size(130, 28);
            btnRandomSeed.UseVisualStyleBackColor = true;
            btnRandomSeed.Name = "btnRandomSeed";
            btnRandomSeed.Click += BtnRandomSeed_Click;
            //
            // btnPresetLoad
            //
            btnPresetLoad.Text = "Load preset...";
            btnPresetLoad.Size = new System.Drawing.Size(110, 28);
            btnPresetLoad.UseVisualStyleBackColor = true;
            btnPresetLoad.Name = "btnPresetLoad";
            btnPresetLoad.Click += BtnPresetLoad_Click;
            //
            // btnPresetSave
            //
            btnPresetSave.Text = "Save preset...";
            btnPresetSave.Size = new System.Drawing.Size(110, 28);
            btnPresetSave.UseVisualStyleBackColor = true;
            btnPresetSave.Name = "btnPresetSave";
            btnPresetSave.Click += BtnPresetSave_Click;
            //
            // btnPresetDefaults
            //
            btnPresetDefaults.Text = "Defaults";
            btnPresetDefaults.Size = new System.Drawing.Size(80, 28);
            btnPresetDefaults.UseVisualStyleBackColor = true;
            btnPresetDefaults.Name = "btnPresetDefaults";
            btnPresetDefaults.Click += BtnPresetDefaults_Click;
            //
            // groupGenerate
            //
            groupGenerate.Controls.Add(flowGenerate);
            groupGenerate.Text = "Generate";
            groupGenerate.Dock = System.Windows.Forms.DockStyle.Top;
            groupGenerate.Padding = new System.Windows.Forms.Padding(6);
            groupGenerate.AutoSize = true;
            groupGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupGenerate.Name = "groupGenerate";
            //
            // flowGenerate
            //
            flowGenerate.Controls.Add(btnPreview);
            flowGenerate.Controls.Add(btnGenerate);
            flowGenerate.Controls.Add(chkRepairAfterGenerate);
            flowGenerate.Controls.Add(chkSaveAfterGenerate);
            flowGenerate.AutoSize = true;
            flowGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowGenerate.WrapContents = true;
            flowGenerate.Dock = System.Windows.Forms.DockStyle.Top;
            flowGenerate.Name = "flowGenerate";
            //
            // btnPreview
            //
            btnPreview.Text = "Quick preview";
            btnPreview.Size = new System.Drawing.Size(130, 28);
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Name = "btnPreview";
            btnPreview.Click += BtnPreview_Click;
            //
            // btnGenerate
            //
            btnGenerate.Text = "Generate full map";
            btnGenerate.Size = new System.Drawing.Size(150, 28);
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Click += BtnGenerate_Click;
            //
            // chkRepairAfterGenerate
            //
            chkRepairAfterGenerate.Text = "Repair transitions";
            chkRepairAfterGenerate.AutoSize = true;
            chkRepairAfterGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRepairAfterGenerate.Checked = true;
            chkRepairAfterGenerate.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRepairAfterGenerate.Name = "chkRepairAfterGenerate";
            //
            // chkSaveAfterGenerate
            //
            chkSaveAfterGenerate.Text = "Save to project";
            chkSaveAfterGenerate.AutoSize = true;
            chkSaveAfterGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkSaveAfterGenerate.Checked = true;
            chkSaveAfterGenerate.CheckState = System.Windows.Forms.CheckState.Checked;
            chkSaveAfterGenerate.Name = "chkSaveAfterGenerate";
            //
            // tabTemplates
            //
            tabTemplates.Controls.Add(splitTemplates);
            tabTemplates.Controls.Add(tableTemplateTop);
            tabTemplates.Text = "3. Templates";
            tabTemplates.Padding = new System.Windows.Forms.Padding(6);
            tabTemplates.UseVisualStyleBackColor = true;
            tabTemplates.Name = "tabTemplates";
            //
            // tableTemplateTop
            //
            tableTemplateTop.Controls.Add(lblTemplateFolder, 0, 0);
            tableTemplateTop.Controls.Add(txtTemplateFolder, 1, 0);
            tableTemplateTop.Controls.Add(btnTemplateBrowse, 2, 0);
            tableTemplateTop.Controls.Add(btnTemplateRefresh, 3, 0);
            tableTemplateTop.Controls.Add(btnTemplateOpenFolder, 4, 0);
            tableTemplateTop.ColumnCount = 5;
            tableTemplateTop.RowCount = 1;
            tableTemplateTop.Dock = System.Windows.Forms.DockStyle.Top;
            tableTemplateTop.Padding = new System.Windows.Forms.Padding(6);
            tableTemplateTop.AutoSize = true;
            tableTemplateTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableTemplateTop.Padding = new System.Windows.Forms.Padding(0);
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateTop.Name = "tableTemplateTop";
            //
            // lblTemplateFolder
            //
            lblTemplateFolder.Text = "Template folder:";
            lblTemplateFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateFolder.AutoSize = true;
            lblTemplateFolder.Name = "lblTemplateFolder";
            //
            // txtTemplateFolder
            //
            txtTemplateFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTemplateFolder.Name = "txtTemplateFolder";
            //
            // btnTemplateBrowse
            //
            btnTemplateBrowse.Text = "Browse...";
            btnTemplateBrowse.Size = new System.Drawing.Size(90, 28);
            btnTemplateBrowse.UseVisualStyleBackColor = true;
            btnTemplateBrowse.Name = "btnTemplateBrowse";
            btnTemplateBrowse.Click += BtnTemplateBrowse_Click;
            //
            // btnTemplateRefresh
            //
            btnTemplateRefresh.Text = "Refresh";
            btnTemplateRefresh.Size = new System.Drawing.Size(80, 28);
            btnTemplateRefresh.UseVisualStyleBackColor = true;
            btnTemplateRefresh.Name = "btnTemplateRefresh";
            btnTemplateRefresh.Click += BtnTemplateRefresh_Click;
            //
            // btnTemplateOpenFolder
            //
            btnTemplateOpenFolder.Text = "Open folder";
            btnTemplateOpenFolder.Size = new System.Drawing.Size(90, 28);
            btnTemplateOpenFolder.UseVisualStyleBackColor = true;
            btnTemplateOpenFolder.Name = "btnTemplateOpenFolder";
            btnTemplateOpenFolder.Click += BtnTemplateOpenFolder_Click;
            //
            // splitTemplates
            //
            splitTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            splitTemplates.Size = new System.Drawing.Size(800, 480);
            splitTemplates.SplitterDistance = 420;
            splitTemplates.Name = "splitTemplates";
            splitTemplates.Panel1.Controls.Add(listTemplates);
            splitTemplates.Panel2.Controls.Add(tableTemplateRight);
            //
            // listTemplates
            //
            listTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            listTemplates.HideSelection = false;
            listTemplates.MultiSelect = false;
            listTemplates.View = System.Windows.Forms.View.LargeIcon;
            listTemplates.Name = "listTemplates";
            listTemplates.SelectedIndexChanged += ListTemplates_SelectedIndexChanged;
            listTemplates.DoubleClick += BtnTemplateApply_Click;
            //
            // tableTemplateRight
            //
            tableTemplateRight.Controls.Add(picTemplate, 0, 0);
            tableTemplateRight.SetColumnSpan(picTemplate, 2);
            tableTemplateRight.Controls.Add(lblTemplateInfo, 0, 1);
            tableTemplateRight.SetColumnSpan(lblTemplateInfo, 2);
            tableTemplateRight.Controls.Add(lblTemplateUse, 0, 2);
            tableTemplateRight.Controls.Add(flowTemplateUse, 1, 2);
            tableTemplateRight.Controls.Add(lblScaleMode, 0, 3);
            tableTemplateRight.Controls.Add(cbScaleMode, 1, 3);
            tableTemplateRight.Controls.Add(lblTemplateFill, 0, 4);
            tableTemplateRight.Controls.Add(cbTemplateFill, 1, 4);
            tableTemplateRight.Controls.Add(chkNaturalOnly, 0, 5);
            tableTemplateRight.SetColumnSpan(chkNaturalOnly, 2);
            tableTemplateRight.Controls.Add(btnTemplateApply, 0, 6);
            tableTemplateRight.SetColumnSpan(btnTemplateApply, 2);
            tableTemplateRight.Controls.Add(lblTemplateHelp, 0, 7);
            tableTemplateRight.SetColumnSpan(lblTemplateHelp, 2);
            tableTemplateRight.ColumnCount = 2;
            tableTemplateRight.RowCount = 9;
            tableTemplateRight.Dock = System.Windows.Forms.DockStyle.Fill;
            tableTemplateRight.Padding = new System.Windows.Forms.Padding(6);
            tableTemplateRight.Padding = new System.Windows.Forms.Padding(4);
            tableTemplateRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            tableTemplateRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.Name = "tableTemplateRight";
            //
            // picTemplate
            //
            picTemplate.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
            picTemplate.Size = new System.Drawing.Size(360, 200);
            picTemplate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            picTemplate.MinimumSize = new System.Drawing.Size(100, 180);
            picTemplate.TabStop = false;
            picTemplate.Name = "picTemplate";
            //
            // lblTemplateInfo
            //
            lblTemplateInfo.Text = "Select an image.";
            lblTemplateInfo.AutoSize = true;
            lblTemplateInfo.MaximumSize = new System.Drawing.Size(420, 0);
            lblTemplateInfo.Name = "lblTemplateInfo";
            //
            // lblTemplateUse
            //
            lblTemplateUse.Text = "Use as:";
            lblTemplateUse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateUse.AutoSize = true;
            lblTemplateUse.Name = "lblTemplateUse";
            //
            // flowTemplateUse
            //
            flowTemplateUse.Controls.Add(rbTplConvert);
            flowTemplateUse.Controls.Add(rbTplSketch);
            flowTemplateUse.Controls.Add(rbTplHeight);
            flowTemplateUse.Controls.Add(rbTplStamp);
            flowTemplateUse.AutoSize = true;
            flowTemplateUse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowTemplateUse.WrapContents = true;
            flowTemplateUse.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowTemplateUse.Name = "flowTemplateUse";
            //
            // rbTplConvert
            //
            rbTplConvert.Text = "Map (terrain colours -> Terrain.bmp + Altitude.bmp)";
            rbTplConvert.AutoSize = true;
            rbTplConvert.Checked = true;
            rbTplConvert.Name = "rbTplConvert";
            //
            // rbTplSketch
            //
            rbTplSketch.Text = "Sketch for the generator";
            rbTplSketch.AutoSize = true;
            rbTplSketch.Name = "rbTplSketch";
            //
            // rbTplHeight
            //
            rbTplHeight.Text = "Height map for the generator (grey scale)";
            rbTplHeight.AutoSize = true;
            rbTplHeight.Name = "rbTplHeight";
            //
            // rbTplStamp
            //
            rbTplStamp.Text = "Stamp pattern in the editor";
            rbTplStamp.AutoSize = true;
            rbTplStamp.Name = "rbTplStamp";
            //
            // lblScaleMode
            //
            lblScaleMode.Text = "Scaling:";
            lblScaleMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblScaleMode.AutoSize = true;
            lblScaleMode.Name = "lblScaleMode";
            //
            // cbScaleMode
            //
            cbScaleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbScaleMode.Size = new System.Drawing.Size(200, 23);
            cbScaleMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbScaleMode.Name = "cbScaleMode";
            //
            // lblTemplateFill
            //
            lblTemplateFill.Text = "Fill / No Draw:";
            lblTemplateFill.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateFill.AutoSize = true;
            lblTemplateFill.Name = "lblTemplateFill";
            //
            // cbTemplateFill
            //
            cbTemplateFill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbTemplateFill.Size = new System.Drawing.Size(260, 23);
            cbTemplateFill.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbTemplateFill.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbTemplateFill.DropDownHeight = 400;
            cbTemplateFill.IntegralHeight = false;
            cbTemplateFill.Name = "cbTemplateFill";
            cbTemplateFill.DrawItem += UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit.TerrainChoice.DrawItem;
            //
            // chkNaturalOnly
            //
            chkNaturalOnly.Text = "Only natural terrains (better for photos / drawings)";
            chkNaturalOnly.AutoSize = true;
            chkNaturalOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkNaturalOnly.Name = "chkNaturalOnly";
            //
            // btnTemplateApply
            //
            btnTemplateApply.Text = "Apply";
            btnTemplateApply.Size = new System.Drawing.Size(160, 28);
            btnTemplateApply.UseVisualStyleBackColor = true;
            btnTemplateApply.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnTemplateApply.Name = "btnTemplateApply";
            btnTemplateApply.Click += BtnTemplateApply_Click;
            //
            // lblTemplateHelp
            //
            lblTemplateHelp.Text = "Put PNG / BMP / JPG / GIF images into the folder (sub folders too). Images in the colours of Terrain.xml are converted 1:1, other colours are matched to the nearest terrain. Transparent pixels = No Draw.";
            lblTemplateHelp.AutoSize = true;
            lblTemplateHelp.MaximumSize = new System.Drawing.Size(420, 0);
            lblTemplateHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblTemplateHelp.Name = "lblTemplateHelp";
            //
            // tabHeights
            //
            tabHeights.Controls.Add(tableHeights);
            tabHeights.Text = "4. Heights";
            tabHeights.Padding = new System.Windows.Forms.Padding(8);
            tabHeights.UseVisualStyleBackColor = true;
            tabHeights.Name = "tabHeights";
            //
            // tableHeights
            //
            tableHeights.Controls.Add(lblHeightsInfo, 0, 0);
            tableHeights.SetColumnSpan(lblHeightsInfo, 5);
            tableHeights.Controls.Add(lblFilterType, 0, 1);
            tableHeights.Controls.Add(cbFilterType, 1, 1);
            tableHeights.SetColumnSpan(cbFilterType, 3);
            tableHeights.Controls.Add(lblFilterTerrain, 0, 2);
            tableHeights.Controls.Add(cbFilterTerrain, 1, 2);
            tableHeights.SetColumnSpan(cbFilterTerrain, 3);
            tableHeights.Controls.Add(lblOffset, 0, 3);
            tableHeights.Controls.Add(numOffset, 1, 3);
            tableHeights.Controls.Add(btnOffset, 3, 3);
            tableHeights.Controls.Add(lblScale, 0, 4);
            tableHeights.Controls.Add(numScale, 1, 4);
            tableHeights.Controls.Add(btnScale, 3, 4);
            tableHeights.Controls.Add(lblSmooth, 0, 5);
            tableHeights.Controls.Add(numSmooth, 1, 5);
            tableHeights.Controls.Add(btnSmooth, 3, 5);
            tableHeights.Controls.Add(lblRoughen, 0, 6);
            tableHeights.Controls.Add(numRoughAmp, 1, 6);
            tableHeights.Controls.Add(numRoughSize, 2, 6);
            tableHeights.Controls.Add(btnRoughen, 3, 6);
            tableHeights.Controls.Add(lblClamp, 0, 7);
            tableHeights.Controls.Add(numClampMin, 1, 7);
            tableHeights.Controls.Add(numClampMax, 2, 7);
            tableHeights.Controls.Add(btnClamp, 3, 7);
            tableHeights.Controls.Add(lblProfile, 0, 8);
            tableHeights.Controls.Add(numProfileSlope, 1, 8);
            tableHeights.Controls.Add(numProfileMax, 2, 8);
            tableHeights.Controls.Add(btnProfile, 3, 8);
            tableHeights.Controls.Add(lblProfileTerrain, 0, 9);
            tableHeights.Controls.Add(cbProfileTerrain, 1, 9);
            tableHeights.SetColumnSpan(cbProfileTerrain, 3);
            tableHeights.Controls.Add(lblResetAltitude, 0, 10);
            tableHeights.Controls.Add(btnResetAltitude, 3, 10);
            tableHeights.Controls.Add(btnHeightUndo, 0, 11);
            tableHeights.Controls.Add(lblHeightStats, 0, 12);
            tableHeights.SetColumnSpan(lblHeightStats, 5);
            tableHeights.ColumnCount = 5;
            tableHeights.RowCount = 13;
            tableHeights.Dock = System.Windows.Forms.DockStyle.Top;
            tableHeights.Padding = new System.Windows.Forms.Padding(6);
            tableHeights.AutoSize = true;
            tableHeights.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableHeights.Name = "tableHeights";
            //
            // lblHeightsInfo
            //
            lblHeightsInfo.Text = "Changes the altitude of the current map. Heights are stored in steps of 5 (see Altitude.xml), the type (water / land / mountain) is kept.";
            lblHeightsInfo.AutoSize = true;
            lblHeightsInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblHeightsInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            lblHeightsInfo.Name = "lblHeightsInfo";
            //
            // lblFilterType
            //
            lblFilterType.Text = "Only altitude type:";
            lblFilterType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFilterType.AutoSize = true;
            lblFilterType.Name = "lblFilterType";
            //
            // cbFilterType
            //
            cbFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFilterType.Size = new System.Drawing.Size(150, 23);
            cbFilterType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbFilterType.Name = "cbFilterType";
            //
            // lblFilterTerrain
            //
            lblFilterTerrain.Text = "Only terrain:";
            lblFilterTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFilterTerrain.AutoSize = true;
            lblFilterTerrain.Name = "lblFilterTerrain";
            //
            // cbFilterTerrain
            //
            cbFilterTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFilterTerrain.Size = new System.Drawing.Size(300, 23);
            cbFilterTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbFilterTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbFilterTerrain.DropDownHeight = 400;
            cbFilterTerrain.IntegralHeight = false;
            cbFilterTerrain.Name = "cbFilterTerrain";
            cbFilterTerrain.DrawItem += UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit.TerrainChoice.DrawItem;
            //
            // lblOffset
            //
            lblOffset.Text = "Raise / lower by (z):";
            lblOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblOffset.AutoSize = true;
            lblOffset.Name = "lblOffset";
            //
            // numOffset
            //
            numOffset.Minimum = new decimal(new int[] { 200, 0, 0, int.MinValue });
            numOffset.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numOffset.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numOffset.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numOffset.Size = new System.Drawing.Size(90, 23);
            numOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numOffset.Name = "numOffset";
            //
            // btnOffset
            //
            btnOffset.Text = "Apply";
            btnOffset.Size = new System.Drawing.Size(90, 28);
            btnOffset.UseVisualStyleBackColor = true;
            btnOffset.Name = "btnOffset";
            btnOffset.Click += BtnOffset_Click;
            //
            // lblScale
            //
            lblScale.Text = "Multiply height by:";
            lblScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblScale.AutoSize = true;
            lblScale.Name = "lblScale";
            //
            // numScale
            //
            numScale.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numScale.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScale.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            numScale.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numScale.Size = new System.Drawing.Size(90, 23);
            numScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numScale.DecimalPlaces = 2;
            numScale.Name = "numScale";
            //
            // btnScale
            //
            btnScale.Text = "Apply";
            btnScale.Size = new System.Drawing.Size(90, 28);
            btnScale.UseVisualStyleBackColor = true;
            btnScale.Name = "btnScale";
            btnScale.Click += BtnScale_Click;
            //
            // lblSmooth
            //
            lblSmooth.Text = "Smooth (passes):";
            lblSmooth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSmooth.AutoSize = true;
            lblSmooth.Name = "lblSmooth";
            //
            // numSmooth
            //
            numSmooth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSmooth.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numSmooth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numSmooth.Increment = 1;
            numSmooth.Size = new System.Drawing.Size(90, 23);
            numSmooth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numSmooth.Name = "numSmooth";
            //
            // btnSmooth
            //
            btnSmooth.Text = "Apply";
            btnSmooth.Size = new System.Drawing.Size(90, 28);
            btnSmooth.UseVisualStyleBackColor = true;
            btnSmooth.Name = "btnSmooth";
            btnSmooth.Click += BtnSmooth_Click;
            //
            // lblRoughen
            //
            lblRoughen.Text = "Roughen (z / size):";
            lblRoughen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblRoughen.AutoSize = true;
            lblRoughen.Name = "lblRoughen";
            //
            // numRoughAmp
            //
            numRoughAmp.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRoughAmp.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numRoughAmp.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numRoughAmp.Increment = 1;
            numRoughAmp.Size = new System.Drawing.Size(90, 23);
            numRoughAmp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numRoughAmp.Name = "numRoughAmp";
            //
            // numRoughSize
            //
            numRoughSize.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numRoughSize.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numRoughSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numRoughSize.Increment = 1;
            numRoughSize.Size = new System.Drawing.Size(90, 23);
            numRoughSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numRoughSize.Name = "numRoughSize";
            //
            // btnRoughen
            //
            btnRoughen.Text = "Apply";
            btnRoughen.Size = new System.Drawing.Size(90, 28);
            btnRoughen.UseVisualStyleBackColor = true;
            btnRoughen.Name = "btnRoughen";
            btnRoughen.Click += BtnRoughen_Click;
            //
            // lblClamp
            //
            lblClamp.Text = "Limit to min / max (z):";
            lblClamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblClamp.AutoSize = true;
            lblClamp.Name = "lblClamp";
            //
            // numClampMin
            //
            numClampMin.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numClampMin.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numClampMin.Value = new decimal(new int[] { 5, 0, 0, int.MinValue });
            numClampMin.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numClampMin.Size = new System.Drawing.Size(90, 23);
            numClampMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numClampMin.Name = "numClampMin";
            //
            // numClampMax
            //
            numClampMax.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numClampMax.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numClampMax.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numClampMax.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numClampMax.Size = new System.Drawing.Size(90, 23);
            numClampMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numClampMax.Name = "numClampMax";
            //
            // btnClamp
            //
            btnClamp.Text = "Apply";
            btnClamp.Size = new System.Drawing.Size(90, 28);
            btnClamp.UseVisualStyleBackColor = true;
            btnClamp.Name = "btnClamp";
            btnClamp.Click += BtnClamp_Click;
            //
            // lblProfile
            //
            lblProfile.Text = "Mountain profile (slope / max z):";
            lblProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProfile.AutoSize = true;
            lblProfile.Name = "lblProfile";
            //
            // numProfileSlope
            //
            numProfileSlope.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numProfileSlope.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numProfileSlope.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numProfileSlope.Increment = 1;
            numProfileSlope.Size = new System.Drawing.Size(90, 23);
            numProfileSlope.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numProfileSlope.Name = "numProfileSlope";
            //
            // numProfileMax
            //
            numProfileMax.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numProfileMax.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numProfileMax.Value = new decimal(new int[] { 70, 0, 0, 0 });
            numProfileMax.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numProfileMax.Size = new System.Drawing.Size(90, 23);
            numProfileMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numProfileMax.Name = "numProfileMax";
            //
            // btnProfile
            //
            btnProfile.Text = "Apply";
            btnProfile.Size = new System.Drawing.Size(90, 28);
            btnProfile.UseVisualStyleBackColor = true;
            btnProfile.Name = "btnProfile";
            btnProfile.Click += BtnProfile_Click;
            //
            // lblProfileTerrain
            //
            lblProfileTerrain.Text = "Mountain terrain:";
            lblProfileTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProfileTerrain.AutoSize = true;
            lblProfileTerrain.Name = "lblProfileTerrain";
            //
            // cbProfileTerrain
            //
            cbProfileTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbProfileTerrain.Size = new System.Drawing.Size(300, 23);
            cbProfileTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbProfileTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbProfileTerrain.DropDownHeight = 400;
            cbProfileTerrain.IntegralHeight = false;
            cbProfileTerrain.Name = "cbProfileTerrain";
            cbProfileTerrain.DrawItem += UoFiddler.Plugin.ConverterMultiTextPlugin.MapMakerKit.TerrainChoice.DrawItem;
            //
            // lblResetAltitude
            //
            lblResetAltitude.Text = "Altitude from terrain defaults:";
            lblResetAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblResetAltitude.AutoSize = true;
            lblResetAltitude.Name = "lblResetAltitude";
            //
            // btnResetAltitude
            //
            btnResetAltitude.Text = "Apply";
            btnResetAltitude.Size = new System.Drawing.Size(90, 28);
            btnResetAltitude.UseVisualStyleBackColor = true;
            btnResetAltitude.Name = "btnResetAltitude";
            btnResetAltitude.Click += BtnResetAltitude_Click;
            //
            // btnHeightUndo
            //
            btnHeightUndo.Text = "Undo last height change";
            btnHeightUndo.Size = new System.Drawing.Size(190, 28);
            btnHeightUndo.UseVisualStyleBackColor = true;
            btnHeightUndo.Enabled = false;
            btnHeightUndo.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            btnHeightUndo.Name = "btnHeightUndo";
            btnHeightUndo.Click += BtnHeightUndo_Click;
            //
            // lblHeightStats
            //
            lblHeightStats.Text = "";
            lblHeightStats.AutoSize = true;
            lblHeightStats.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            lblHeightStats.Name = "lblHeightStats";
            //
            // tabBuild
            //
            tabBuild.Controls.Add(tableBuild);
            tabBuild.Text = "5. Build MUL";
            tabBuild.Padding = new System.Windows.Forms.Padding(8);
            tabBuild.UseVisualStyleBackColor = true;
            tabBuild.Name = "tabBuild";
            //
            // tableBuild
            //
            tableBuild.Controls.Add(lblBuildInfo, 0, 0);
            tableBuild.SetColumnSpan(lblBuildInfo, 3);
            tableBuild.Controls.Add(lblOutputFolder, 0, 1);
            tableBuild.Controls.Add(txtOutputFolder, 1, 1);
            tableBuild.Controls.Add(btnOutputBrowse, 2, 1);
            tableBuild.Controls.Add(chkRandomStatics, 0, 2);
            tableBuild.SetColumnSpan(chkRandomStatics, 3);
            tableBuild.Controls.Add(chkImportStatics, 0, 3);
            tableBuild.Controls.Add(txtImportFolder, 1, 3);
            tableBuild.Controls.Add(btnImportBrowse, 2, 3);
            tableBuild.Controls.Add(lblCompileSeed, 0, 4);
            tableBuild.Controls.Add(numCompileSeed, 1, 4);
            tableBuild.Controls.Add(chkRepairBeforeBuild, 0, 5);
            tableBuild.SetColumnSpan(chkRepairBeforeBuild, 3);
            tableBuild.Controls.Add(flowBuildButtons, 0, 6);
            tableBuild.SetColumnSpan(flowBuildButtons, 3);
            tableBuild.Controls.Add(lblBuildHelp, 0, 7);
            tableBuild.SetColumnSpan(lblBuildHelp, 3);
            tableBuild.ColumnCount = 3;
            tableBuild.RowCount = 10;
            tableBuild.Dock = System.Windows.Forms.DockStyle.Top;
            tableBuild.Padding = new System.Windows.Forms.Padding(6);
            tableBuild.AutoSize = true;
            tableBuild.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBuild.Name = "tableBuild";
            //
            // lblBuildInfo
            //
            lblBuildInfo.Text = "Builds Map#.mul, StaIdx#.mul and Statics#.mul from the current map (# = \"Map # (mul)\" in the project bar). Transitions from Data/Transitions, random statics from Data/Statics.";
            lblBuildInfo.AutoSize = true;
            lblBuildInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblBuildInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            lblBuildInfo.Name = "lblBuildInfo";
            //
            // lblOutputFolder
            //
            lblOutputFolder.Text = "Output folder:";
            lblOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Name = "lblOutputFolder";
            //
            // txtOutputFolder
            //
            txtOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtOutputFolder.PlaceholderText = "(project folder)";
            txtOutputFolder.Name = "txtOutputFolder";
            //
            // btnOutputBrowse
            //
            btnOutputBrowse.Text = "Browse...";
            btnOutputBrowse.Size = new System.Drawing.Size(90, 28);
            btnOutputBrowse.UseVisualStyleBackColor = true;
            btnOutputBrowse.Name = "btnOutputBrowse";
            btnOutputBrowse.Click += BtnOutputBrowse_Click;
            //
            // chkRandomStatics
            //
            chkRandomStatics.Text = "Random statics (trees, rocks, ... from Data/Statics)";
            chkRandomStatics.AutoSize = true;
            chkRandomStatics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRandomStatics.Checked = true;
            chkRandomStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRandomStatics.Name = "chkRandomStatics";
            //
            // chkImportStatics
            //
            chkImportStatics.Text = "Import static XML files from folder:";
            chkImportStatics.AutoSize = true;
            chkImportStatics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkImportStatics.Checked = true;
            chkImportStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            chkImportStatics.Name = "chkImportStatics";
            //
            // txtImportFolder
            //
            txtImportFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtImportFolder.PlaceholderText = "(project folder)\\Import Files";
            txtImportFolder.Name = "txtImportFolder";
            //
            // btnImportBrowse
            //
            btnImportBrowse.Text = "Browse...";
            btnImportBrowse.Size = new System.Drawing.Size(90, 28);
            btnImportBrowse.UseVisualStyleBackColor = true;
            btnImportBrowse.Name = "btnImportBrowse";
            btnImportBrowse.Click += BtnImportBrowse_Click;
            //
            // lblCompileSeed
            //
            lblCompileSeed.Text = "Random seed (0 = random):";
            lblCompileSeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCompileSeed.AutoSize = true;
            lblCompileSeed.Name = "lblCompileSeed";
            //
            // numCompileSeed
            //
            numCompileSeed.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numCompileSeed.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCompileSeed.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numCompileSeed.Increment = 1;
            numCompileSeed.Size = new System.Drawing.Size(120, 23);
            numCompileSeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numCompileSeed.Name = "numCompileSeed";
            //
            // chkRepairBeforeBuild
            //
            chkRepairBeforeBuild.Text = "Repair transitions before building";
            chkRepairBeforeBuild.AutoSize = true;
            chkRepairBeforeBuild.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRepairBeforeBuild.Name = "chkRepairBeforeBuild";
            //
            // flowBuildButtons
            //
            flowBuildButtons.Controls.Add(btnCheckTransitions);
            flowBuildButtons.Controls.Add(btnRepairTransitions);
            flowBuildButtons.Controls.Add(btnBuildMul);
            flowBuildButtons.AutoSize = true;
            flowBuildButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowBuildButtons.WrapContents = true;
            flowBuildButtons.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            flowBuildButtons.Name = "flowBuildButtons";
            //
            // btnCheckTransitions
            //
            btnCheckTransitions.Text = "Check transitions";
            btnCheckTransitions.Size = new System.Drawing.Size(150, 28);
            btnCheckTransitions.UseVisualStyleBackColor = true;
            btnCheckTransitions.Name = "btnCheckTransitions";
            btnCheckTransitions.Click += BtnCheckTransitions_Click;
            //
            // btnRepairTransitions
            //
            btnRepairTransitions.Text = "Repair transitions";
            btnRepairTransitions.Size = new System.Drawing.Size(150, 28);
            btnRepairTransitions.UseVisualStyleBackColor = true;
            btnRepairTransitions.Name = "btnRepairTransitions";
            btnRepairTransitions.Click += BtnRepairTransitions_Click;
            //
            // btnBuildMul
            //
            btnBuildMul.Text = "Build MUL files";
            btnBuildMul.Size = new System.Drawing.Size(170, 28);
            btnBuildMul.UseVisualStyleBackColor = true;
            btnBuildMul.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnBuildMul.Name = "btnBuildMul";
            btnBuildMul.Click += BtnBuildMul_Click;
            //
            // lblBuildHelp
            //
            lblBuildHelp.Text = "Check transitions lists terrain borders without transition (hard edges in the client). Repair fixes single tiles and thin strips automatically. Copy the mul files into your client / server folder to test the map.";
            lblBuildHelp.AutoSize = true;
            lblBuildHelp.MaximumSize = new System.Drawing.Size(700, 0);
            lblBuildHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblBuildHelp.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            lblBuildHelp.Name = "lblBuildHelp";
            //
            // tabPageDataViewer
            //
            tabPageDataViewer.Controls.Add(tableDataViewer);
            tabPageDataViewer.Controls.Add(MainMenu1);
            tabPageDataViewer.Text = "6. Data viewer";
            tabPageDataViewer.Padding = new System.Windows.Forms.Padding(6);
            tabPageDataViewer.UseVisualStyleBackColor = true;
            tabPageDataViewer.Name = "tabPageDataViewer";
            //
            // MainMenu1
            //
            MainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItem1, MenuItem2 });
            MainMenu1.Dock = System.Windows.Forms.DockStyle.Top;
            MainMenu1.Text = "MainMenu1";
            MainMenu1.Name = "MainMenu1";
            //
            // MenuItem1
            //
            MenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, exportToolStripMenuItem });
            MenuItem1.Text = "Terrain";
            MenuItem1.Name = "MenuItem1";
            //
            // loadToolStripMenuItem
            //
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Text = "Save Terrain.xml";
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            //
            // exportToolStripMenuItem
            //
            exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { terrainACTToolStripMenuItem1, terrainACOToolStripMenuItem1 });
            exportToolStripMenuItem.Text = "Export";
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            //
            // terrainACTToolStripMenuItem1
            //
            terrainACTToolStripMenuItem1.Text = "Terrain.ACT";
            terrainACTToolStripMenuItem1.Name = "terrainACTToolStripMenuItem1";
            terrainACTToolStripMenuItem1.Click += terrainACTToolStripMenuItem1_Click;
            //
            // terrainACOToolStripMenuItem1
            //
            terrainACOToolStripMenuItem1.Text = "Terrain.ACO";
            terrainACOToolStripMenuItem1.Name = "terrainACOToolStripMenuItem1";
            terrainACOToolStripMenuItem1.Click += terrainACOToolStripMenuItem1_Click;
            //
            // MenuItem2
            //
            MenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem1, saveToolStripMenuItem1, exportToolStripMenuItem1 });
            MenuItem2.Text = "Altitude";
            MenuItem2.Name = "MenuItem2";
            //
            // loadToolStripMenuItem1
            //
            loadToolStripMenuItem1.Text = "Load";
            loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            loadToolStripMenuItem1.Click += loadToolStripMenuItem1_Click;
            //
            // saveToolStripMenuItem1
            //
            saveToolStripMenuItem1.Text = "Save Altitude.xml";
            saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            saveToolStripMenuItem1.Click += saveToolStripMenuItem1_Click;
            //
            // exportToolStripMenuItem1
            //
            exportToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { AltitudeACTToolStripMenuItem, AltitudeACOToolStripMenuItem });
            exportToolStripMenuItem1.Text = "Export";
            exportToolStripMenuItem1.Name = "exportToolStripMenuItem1";
            //
            // AltitudeACTToolStripMenuItem
            //
            AltitudeACTToolStripMenuItem.Text = "Altitude.ACT";
            AltitudeACTToolStripMenuItem.Name = "AltitudeACTToolStripMenuItem";
            AltitudeACTToolStripMenuItem.Click += AltitudeACTToolStripMenuItem_Click;
            //
            // AltitudeACOToolStripMenuItem
            //
            AltitudeACOToolStripMenuItem.Text = "Altitude.ACO";
            AltitudeACOToolStripMenuItem.Name = "AltitudeACOToolStripMenuItem";
            AltitudeACOToolStripMenuItem.Click += AltitudeACOToolStripMenuItem_Click;
            //
            // tableDataViewer
            //
            tableDataViewer.Controls.Add(lbTerrainList, 0, 0);
            tableDataViewer.Controls.Add(lbTileProperties, 1, 0);
            tableDataViewer.Controls.Add(ListBox1, 0, 1);
            tableDataViewer.SetRowSpan(ListBox1, 2);
            tableDataViewer.Controls.Add(PropertyGrid1, 1, 1);
            tableDataViewer.Controls.Add(PictureBox1, 1, 2);
            tableDataViewer.ColumnCount = 2;
            tableDataViewer.RowCount = 3;
            tableDataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            tableDataViewer.Padding = new System.Windows.Forms.Padding(6);
            tableDataViewer.Padding = new System.Windows.Forms.Padding(0);
            tableDataViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableDataViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDataViewer.Name = "tableDataViewer";
            //
            // lbTerrainList
            //
            lbTerrainList.Text = "Terrain List";
            lbTerrainList.AutoSize = true;
            lbTerrainList.Name = "lbTerrainList";
            //
            // lbTileProperties
            //
            lbTileProperties.Text = "Tile Properties";
            lbTileProperties.AutoSize = true;
            lbTileProperties.Name = "lbTileProperties";
            //
            // ListBox1
            //
            ListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            ListBox1.IntegralHeight = false;
            ListBox1.Name = "ListBox1";
            ListBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            //
            // PropertyGrid1
            //
            PropertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            PropertyGrid1.ToolbarVisible = false;
            PropertyGrid1.Name = "PropertyGrid1";
            //
            // PictureBox1
            //
            PictureBox1.Size = new System.Drawing.Size(120, 120);
            PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            PictureBox1.TabStop = false;
            PictureBox1.Name = "PictureBox1";
            //
            // tabHelp
            //
            tabHelp.Controls.Add(rtbHelp);
            tabHelp.Text = "7. Help";
            tabHelp.Padding = new System.Windows.Forms.Padding(6);
            tabHelp.UseVisualStyleBackColor = true;
            tabHelp.Name = "tabHelp";
            //
            // rtbHelp
            //
            rtbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbHelp.ReadOnly = true;
            rtbHelp.BackColor = System.Drawing.SystemColors.Window;
            rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbHelp.Font = new System.Drawing.Font("Segoe UI", 10F);
            rtbHelp.Name = "rtbHelp";
            //
            // groupLog
            //
            groupLog.Controls.Add(txtLog);
            groupLog.Controls.Add(flowLogButtons);
            groupLog.Text = "Log";
            groupLog.Dock = System.Windows.Forms.DockStyle.Fill;
            groupLog.Padding = new System.Windows.Forms.Padding(6);
            groupLog.Name = "groupLog";
            //
            // txtLog
            //
            txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            txtLog.BackColor = System.Drawing.SystemColors.Window;
            txtLog.WordWrap = false;
            txtLog.Name = "txtLog";
            //
            // flowLogButtons
            //
            flowLogButtons.Controls.Add(btnCancel);
            flowLogButtons.Controls.Add(btnClearLog);
            flowLogButtons.Controls.Add(btnSaveLog);
            flowLogButtons.AutoSize = true;
            flowLogButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLogButtons.WrapContents = true;
            flowLogButtons.Dock = System.Windows.Forms.DockStyle.Right;
            flowLogButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLogButtons.WrapContents = false;
            flowLogButtons.Name = "flowLogButtons";
            //
            // btnCancel
            //
            btnCancel.Text = "Cancel";
            btnCancel.Size = new System.Drawing.Size(110, 28);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Enabled = false;
            btnCancel.Name = "btnCancel";
            btnCancel.Click += BtnCancel_Click;
            //
            // btnClearLog
            //
            btnClearLog.Text = "Clear log";
            btnClearLog.Size = new System.Drawing.Size(110, 28);
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Click += BtnClearLog_Click;
            //
            // btnSaveLog
            //
            btnSaveLog.Text = "Save log...";
            btnSaveLog.Size = new System.Drawing.Size(110, 28);
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Click += BtnSaveLog_Click;
            //
            // toolTip
            //
            toolTip.AutoPopDelay = 8000;
            toolTip.InitialDelay = 400;
            toolTip.ReshowDelay = 200;
            //
            // MapMaker
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1260, 880);
            Controls.Add(splitMain);
            Controls.Add(statusStrip);
            Controls.Add(groupProject);
            Controls.Add(menuStrip);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            MinimumSize = new System.Drawing.Size(1080, 740);
            Name = "MapMaker";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Map Maker";
            FormClosing += MapMaker_FormClosing;
            FormClosed += MapMaker_FormClosed;
            Load += MapMaker_Load;
            flowLogButtons.ResumeLayout(false);
            flowLogButtons.PerformLayout();
            groupLog.ResumeLayout(false);
            groupLog.PerformLayout();
            tabHelp.ResumeLayout(false);
            tabHelp.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            tableDataViewer.ResumeLayout(false);
            tableDataViewer.PerformLayout();
            MainMenu1.ResumeLayout(false);
            MainMenu1.PerformLayout();
            tabPageDataViewer.ResumeLayout(false);
            tabPageDataViewer.PerformLayout();
            flowBuildButtons.ResumeLayout(false);
            flowBuildButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCompileSeed).EndInit();
            tableBuild.ResumeLayout(false);
            tableBuild.PerformLayout();
            tabBuild.ResumeLayout(false);
            tabBuild.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numProfileMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numProfileSlope).EndInit();
            ((System.ComponentModel.ISupportInitialize)numClampMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numClampMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoughSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoughAmp).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSmooth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOffset).EndInit();
            tableHeights.ResumeLayout(false);
            tableHeights.PerformLayout();
            tabHeights.ResumeLayout(false);
            tabHeights.PerformLayout();
            flowTemplateUse.ResumeLayout(false);
            flowTemplateUse.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picTemplate).EndInit();
            tableTemplateRight.ResumeLayout(false);
            tableTemplateRight.PerformLayout();
            splitTemplates.Panel1.ResumeLayout(false);
            splitTemplates.Panel2.ResumeLayout(false);
            splitTemplates.Panel1.PerformLayout();
            splitTemplates.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitTemplates).EndInit();
            splitTemplates.ResumeLayout(false);
            splitTemplates.PerformLayout();
            tableTemplateTop.ResumeLayout(false);
            tableTemplateTop.PerformLayout();
            tabTemplates.ResumeLayout(false);
            tabTemplates.PerformLayout();
            flowGenerate.ResumeLayout(false);
            flowGenerate.PerformLayout();
            groupGenerate.ResumeLayout(false);
            groupGenerate.PerformLayout();
            flowPresets.ResumeLayout(false);
            flowPresets.PerformLayout();
            groupPresets.ResumeLayout(false);
            groupPresets.PerformLayout();
            tableSketch.ResumeLayout(false);
            tableSketch.PerformLayout();
            groupSketch.ResumeLayout(false);
            groupSketch.PerformLayout();
            flowBiomeButtons.ResumeLayout(false);
            flowBiomeButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridBiomes).EndInit();
            groupBiomes.ResumeLayout(false);
            groupBiomes.PerformLayout();
            tableGenRight.ResumeLayout(false);
            tableGenRight.PerformLayout();
            splitGenerator.Panel1.ResumeLayout(false);
            splitGenerator.Panel2.ResumeLayout(false);
            splitGenerator.Panel1.PerformLayout();
            splitGenerator.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitGenerator).EndInit();
            splitGenerator.ResumeLayout(false);
            splitGenerator.PerformLayout();
            tabGenerator.ResumeLayout(false);
            tabGenerator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBlankAltitude).EndInit();
            tableBlank.ResumeLayout(false);
            tableBlank.PerformLayout();
            tabBlank.ResumeLayout(false);
            tabBlank.PerformLayout();
            tabControl.ResumeLayout(false);
            tabControl.PerformLayout();
            flowPreviewMode.ResumeLayout(false);
            flowPreviewMode.PerformLayout();
            tablePreviewBottom.ResumeLayout(false);
            tablePreviewBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            groupPreview.ResumeLayout(false);
            groupPreview.PerformLayout();
            splitWork.Panel1.ResumeLayout(false);
            splitWork.Panel2.ResumeLayout(false);
            splitWork.Panel1.PerformLayout();
            splitWork.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitWork).EndInit();
            splitWork.ResumeLayout(false);
            splitWork.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            splitMain.Panel1.PerformLayout();
            splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            splitMain.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMapNumber).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            flowSize.ResumeLayout(false);
            flowSize.PerformLayout();
            tableProject.ResumeLayout(false);
            tableProject.PerformLayout();
            groupProject.ResumeLayout(false);
            groupProject.PerformLayout();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miOpenProject;
        private System.Windows.Forms.ToolStripMenuItem miLoadMap;
        private System.Windows.Forms.ToolStripMenuItem miSaveMap;
        private System.Windows.Forms.ToolStripSeparator miSep1;
        private System.Windows.Forms.ToolStripMenuItem miOpenEditor;
        private System.Windows.Forms.ToolStripSeparator miSep2;
        private System.Windows.Forms.ToolStripMenuItem miClose;
        private System.Windows.Forms.ToolStripMenuItem miTools;
        private System.Windows.Forms.ToolStripMenuItem createDataIndexToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xMLHexSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem transitionEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator miSep3;
        private System.Windows.Forms.ToolStripMenuItem miExportPalettes;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miManual;
        private System.Windows.Forms.GroupBox groupProject;
        private System.Windows.Forms.TableLayoutPanel tableProject;
        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.TextBox txtProject;
        private System.Windows.Forms.Button btnBrowseProject;
        private System.Windows.Forms.Button btnOpenProjectFolder;
        private System.Windows.Forms.Label lblTerrainFile;
        private System.Windows.Forms.TextBox txtTerrainFile;
        private System.Windows.Forms.Label lblMapSize;
        private System.Windows.Forms.ComboBox cbMapSize;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.FlowLayoutPanel flowSize;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.Label lblSizeX;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.Label lblMapNumber;
        private System.Windows.Forms.NumericUpDown numMapNumber;
        private System.Windows.Forms.Label lblAltitudeFile;
        private System.Windows.Forms.TextBox txtAltitudeFile;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.SplitContainer splitWork;
        private System.Windows.Forms.GroupBox groupPreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.TableLayoutPanel tablePreviewBottom;
        private System.Windows.Forms.FlowLayoutPanel flowPreviewMode;
        private System.Windows.Forms.RadioButton rbPrevTerrain;
        private System.Windows.Forms.RadioButton rbPrevAltitude;
        private System.Windows.Forms.RadioButton rbPrevRelief;
        private System.Windows.Forms.Label lblMapInfo;
        private System.Windows.Forms.Button btnEditMap;
        private System.Windows.Forms.Button btnSaveMap;
        private System.Windows.Forms.Button btnLoadMap;
        private System.Windows.Forms.Button btnSaveMapAs;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBlank;
        private System.Windows.Forms.TableLayoutPanel tableBlank;
        private System.Windows.Forms.Label lblBlankInfo;
        private System.Windows.Forms.Label lblDefaultTerrain;
        private System.Windows.Forms.ComboBox cbDefaultTerrain;
        private System.Windows.Forms.CheckBox chkBlankTerrainAltitude;
        private System.Windows.Forms.Label lblBlankAltitude;
        private System.Windows.Forms.NumericUpDown numBlankAltitude;
        private System.Windows.Forms.CheckBox chkBlankDungeon;
        private System.Windows.Forms.Button btnCreateBlank;
        private System.Windows.Forms.TabPage tabGenerator;
        private System.Windows.Forms.SplitContainer splitGenerator;
        private System.Windows.Forms.PropertyGrid propertyGenerator;
        private System.Windows.Forms.TableLayoutPanel tableGenRight;
        private System.Windows.Forms.GroupBox groupBiomes;
        private System.Windows.Forms.DataGridView gridBiomes;
        private System.Windows.Forms.FlowLayoutPanel flowBiomeButtons;
        private System.Windows.Forms.Button btnAddBiome;
        private System.Windows.Forms.Button btnRemoveBiome;
        private System.Windows.Forms.Button btnBiomeUp;
        private System.Windows.Forms.Button btnBiomeDown;
        private System.Windows.Forms.GroupBox groupSketch;
        private System.Windows.Forms.TableLayoutPanel tableSketch;
        private System.Windows.Forms.Label lblSketchInfo;
        private System.Windows.Forms.Button btnLoadSketch;
        private System.Windows.Forms.Button btnClearSketch;
        private System.Windows.Forms.Label lblHeightTemplateInfo;
        private System.Windows.Forms.Button btnLoadHeightTemplate;
        private System.Windows.Forms.Button btnClearHeightTemplate;
        private System.Windows.Forms.Label lblSketchHelp;
        private System.Windows.Forms.GroupBox groupPresets;
        private System.Windows.Forms.FlowLayoutPanel flowPresets;
        private System.Windows.Forms.Button btnRandomSeed;
        private System.Windows.Forms.Button btnPresetLoad;
        private System.Windows.Forms.Button btnPresetSave;
        private System.Windows.Forms.Button btnPresetDefaults;
        private System.Windows.Forms.GroupBox groupGenerate;
        private System.Windows.Forms.FlowLayoutPanel flowGenerate;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.CheckBox chkRepairAfterGenerate;
        private System.Windows.Forms.CheckBox chkSaveAfterGenerate;
        private System.Windows.Forms.TabPage tabTemplates;
        private System.Windows.Forms.TableLayoutPanel tableTemplateTop;
        private System.Windows.Forms.Label lblTemplateFolder;
        private System.Windows.Forms.TextBox txtTemplateFolder;
        private System.Windows.Forms.Button btnTemplateBrowse;
        private System.Windows.Forms.Button btnTemplateRefresh;
        private System.Windows.Forms.Button btnTemplateOpenFolder;
        private System.Windows.Forms.SplitContainer splitTemplates;
        private System.Windows.Forms.ListView listTemplates;
        private System.Windows.Forms.TableLayoutPanel tableTemplateRight;
        private System.Windows.Forms.PictureBox picTemplate;
        private System.Windows.Forms.Label lblTemplateInfo;
        private System.Windows.Forms.Label lblTemplateUse;
        private System.Windows.Forms.FlowLayoutPanel flowTemplateUse;
        private System.Windows.Forms.RadioButton rbTplConvert;
        private System.Windows.Forms.RadioButton rbTplSketch;
        private System.Windows.Forms.RadioButton rbTplHeight;
        private System.Windows.Forms.RadioButton rbTplStamp;
        private System.Windows.Forms.Label lblScaleMode;
        private System.Windows.Forms.ComboBox cbScaleMode;
        private System.Windows.Forms.Label lblTemplateFill;
        private System.Windows.Forms.ComboBox cbTemplateFill;
        private System.Windows.Forms.CheckBox chkNaturalOnly;
        private System.Windows.Forms.Button btnTemplateApply;
        private System.Windows.Forms.Label lblTemplateHelp;
        private System.Windows.Forms.TabPage tabHeights;
        private System.Windows.Forms.TableLayoutPanel tableHeights;
        private System.Windows.Forms.Label lblHeightsInfo;
        private System.Windows.Forms.Label lblFilterType;
        private System.Windows.Forms.ComboBox cbFilterType;
        private System.Windows.Forms.Label lblFilterTerrain;
        private System.Windows.Forms.ComboBox cbFilterTerrain;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.NumericUpDown numOffset;
        private System.Windows.Forms.Button btnOffset;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Button btnScale;
        private System.Windows.Forms.Label lblSmooth;
        private System.Windows.Forms.NumericUpDown numSmooth;
        private System.Windows.Forms.Button btnSmooth;
        private System.Windows.Forms.Label lblRoughen;
        private System.Windows.Forms.NumericUpDown numRoughAmp;
        private System.Windows.Forms.NumericUpDown numRoughSize;
        private System.Windows.Forms.Button btnRoughen;
        private System.Windows.Forms.Label lblClamp;
        private System.Windows.Forms.NumericUpDown numClampMin;
        private System.Windows.Forms.NumericUpDown numClampMax;
        private System.Windows.Forms.Button btnClamp;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.NumericUpDown numProfileSlope;
        private System.Windows.Forms.NumericUpDown numProfileMax;
        private System.Windows.Forms.Button btnProfile;
        private System.Windows.Forms.Label lblProfileTerrain;
        private System.Windows.Forms.ComboBox cbProfileTerrain;
        private System.Windows.Forms.Label lblResetAltitude;
        private System.Windows.Forms.Button btnResetAltitude;
        private System.Windows.Forms.Button btnHeightUndo;
        private System.Windows.Forms.Label lblHeightStats;
        private System.Windows.Forms.TabPage tabBuild;
        private System.Windows.Forms.TableLayoutPanel tableBuild;
        private System.Windows.Forms.Label lblBuildInfo;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Button btnOutputBrowse;
        private System.Windows.Forms.CheckBox chkRandomStatics;
        private System.Windows.Forms.CheckBox chkImportStatics;
        private System.Windows.Forms.TextBox txtImportFolder;
        private System.Windows.Forms.Button btnImportBrowse;
        private System.Windows.Forms.Label lblCompileSeed;
        private System.Windows.Forms.NumericUpDown numCompileSeed;
        private System.Windows.Forms.CheckBox chkRepairBeforeBuild;
        private System.Windows.Forms.FlowLayoutPanel flowBuildButtons;
        private System.Windows.Forms.Button btnCheckTransitions;
        private System.Windows.Forms.Button btnRepairTransitions;
        private System.Windows.Forms.Button btnBuildMul;
        private System.Windows.Forms.Label lblBuildHelp;
        private System.Windows.Forms.TabPage tabPageDataViewer;
        private System.Windows.Forms.MenuStrip MainMenu1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem1;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem terrainACTToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem terrainACOToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem2;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem AltitudeACTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AltitudeACOToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableDataViewer;
        private System.Windows.Forms.Label lbTerrainList;
        private System.Windows.Forms.Label lbTileProperties;
        private System.Windows.Forms.ListBox ListBox1;
        private System.Windows.Forms.PropertyGrid PropertyGrid1;
        private System.Windows.Forms.PictureBox PictureBox1;
        private System.Windows.Forms.TabPage tabHelp;
        private System.Windows.Forms.RichTextBox rtbHelp;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.FlowLayoutPanel flowLogButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnSaveLog;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

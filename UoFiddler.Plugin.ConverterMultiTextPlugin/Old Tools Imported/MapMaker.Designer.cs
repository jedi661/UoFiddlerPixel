// /***************************************************************************
//  *
//  * $Author: Nikodemus
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
            tableTemplateTop = new System.Windows.Forms.TableLayoutPanel();
            lblTemplateFolder = new System.Windows.Forms.Label();
            txtTemplateFolder = new System.Windows.Forms.TextBox();
            btnTemplateBrowse = new System.Windows.Forms.Button();
            btnTemplateRefresh = new System.Windows.Forms.Button();
            btnTemplateOpenFolder = new System.Windows.Forms.Button();
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
            tableDataViewer = new System.Windows.Forms.TableLayoutPanel();
            lbTerrainList = new System.Windows.Forms.Label();
            lbTileProperties = new System.Windows.Forms.Label();
            ListBox1 = new System.Windows.Forms.ListBox();
            PropertyGrid1 = new System.Windows.Forms.PropertyGrid();
            PictureBox1 = new System.Windows.Forms.PictureBox();
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
            tabHelp = new System.Windows.Forms.TabPage();
            rtbHelp = new System.Windows.Forms.RichTextBox();
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
            ((System.ComponentModel.ISupportInitialize)splitTemplates).BeginInit();
            splitTemplates.Panel1.SuspendLayout();
            splitTemplates.Panel2.SuspendLayout();
            splitTemplates.SuspendLayout();
            tableTemplateRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picTemplate).BeginInit();
            flowTemplateUse.SuspendLayout();
            tableTemplateTop.SuspendLayout();
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
            tableDataViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            MainMenu1.SuspendLayout();
            tabHelp.SuspendLayout();
            groupPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            tablePreviewBottom.SuspendLayout();
            flowPreviewMode.SuspendLayout();
            groupLog.SuspendLayout();
            flowLogButtons.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { miFile, miTools, miHelp });
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(1260, 24);
            menuStrip.TabIndex = 3;
            menuStrip.Text = "menuStrip";
            // 
            // miFile
            // 
            miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miOpenProject, miLoadMap, miSaveMap, miSep1, miOpenEditor, miSep2, miClose });
            miFile.Name = "miFile";
            miFile.Size = new System.Drawing.Size(37, 20);
            miFile.Text = "&File";
            // 
            // miOpenProject
            // 
            miOpenProject.Name = "miOpenProject";
            miOpenProject.Size = new System.Drawing.Size(221, 22);
            miOpenProject.Text = "Open project folder...";
            miOpenProject.Click += BtnBrowseProject_Click;
            // 
            // miLoadMap
            // 
            miLoadMap.Name = "miLoadMap";
            miLoadMap.Size = new System.Drawing.Size(221, 22);
            miLoadMap.Text = "Load map from project";
            miLoadMap.Click += BtnLoadMap_Click;
            // 
            // miSaveMap
            // 
            miSaveMap.Name = "miSaveMap";
            miSaveMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            miSaveMap.Size = new System.Drawing.Size(221, 22);
            miSaveMap.Text = "Save map to project";
            miSaveMap.Click += BtnSaveMap_Click;
            // 
            // miSep1
            // 
            miSep1.Name = "miSep1";
            miSep1.Size = new System.Drawing.Size(218, 6);
            // 
            // miOpenEditor
            // 
            miOpenEditor.Name = "miOpenEditor";
            miOpenEditor.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E;
            miOpenEditor.Size = new System.Drawing.Size(221, 22);
            miOpenEditor.Text = "Open map editor...";
            miOpenEditor.Click += BtnEditMap_Click;
            // 
            // miSep2
            // 
            miSep2.Name = "miSep2";
            miSep2.Size = new System.Drawing.Size(218, 6);
            // 
            // miClose
            // 
            miClose.Name = "miClose";
            miClose.Size = new System.Drawing.Size(221, 22);
            miClose.Text = "Close";
            miClose.Click += MiClose_Click;
            // 
            // miTools
            // 
            miTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { createDataIndexToolStripMenuItem, createStaticToolStripMenuItem, xMLHexSearchToolStripMenuItem, transitionEditorToolStripMenuItem, miSep3, miExportPalettes });
            miTools.Name = "miTools";
            miTools.Size = new System.Drawing.Size(46, 20);
            miTools.Text = "&Tools";
            // 
            // createDataIndexToolStripMenuItem
            // 
            createDataIndexToolStripMenuItem.Name = "createDataIndexToolStripMenuItem";
            createDataIndexToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            createDataIndexToolStripMenuItem.Text = "Create Data Index";
            createDataIndexToolStripMenuItem.Click += createDataIndexToolStripMenuItem_Click;
            // 
            // createStaticToolStripMenuItem
            // 
            createStaticToolStripMenuItem.Name = "createStaticToolStripMenuItem";
            createStaticToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            createStaticToolStripMenuItem.Text = "Create Static";
            createStaticToolStripMenuItem.Click += createStaticToolStripMenuItem_Click;
            // 
            // xMLHexSearchToolStripMenuItem
            // 
            xMLHexSearchToolStripMenuItem.Name = "xMLHexSearchToolStripMenuItem";
            xMLHexSearchToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            xMLHexSearchToolStripMenuItem.Text = "XML Hex Search";
            xMLHexSearchToolStripMenuItem.Click += xMLHexSearchToolStripMenuItem_Click;
            // 
            // transitionEditorToolStripMenuItem
            // 
            transitionEditorToolStripMenuItem.Name = "transitionEditorToolStripMenuItem";
            transitionEditorToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
            transitionEditorToolStripMenuItem.Text = "Transition Editor";
            transitionEditorToolStripMenuItem.Click += transitionEditorToolStripMenuItem_Click;
            // 
            // miSep3
            // 
            miSep3.Name = "miSep3";
            miSep3.Size = new System.Drawing.Size(297, 6);
            // 
            // miExportPalettes
            // 
            miExportPalettes.Name = "miExportPalettes";
            miExportPalettes.Size = new System.Drawing.Size(300, 22);
            miExportPalettes.Text = "Export palettes for Photoshop (ACT / ACO)";
            miExportPalettes.Click += MiExportPalettes_Click;
            // 
            // miHelp
            // 
            miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { miManual });
            miHelp.Name = "miHelp";
            miHelp.Size = new System.Drawing.Size(44, 20);
            miHelp.Text = "&Help";
            // 
            // miManual
            // 
            miManual.Name = "miManual";
            miManual.ShortcutKeys = System.Windows.Forms.Keys.F1;
            miManual.Size = new System.Drawing.Size(186, 22);
            miManual.Text = "Manual (German)";
            miManual.Click += MiManual_Click;
            // 
            // groupProject
            // 
            groupProject.AutoSize = true;
            groupProject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupProject.Controls.Add(tableProject);
            groupProject.Dock = System.Windows.Forms.DockStyle.Top;
            groupProject.Location = new System.Drawing.Point(0, 24);
            groupProject.Name = "groupProject";
            groupProject.Padding = new System.Windows.Forms.Padding(6);
            groupProject.Size = new System.Drawing.Size(1260, 95);
            groupProject.TabIndex = 2;
            groupProject.TabStop = false;
            groupProject.Text = "Project";
            // 
            // tableProject
            // 
            tableProject.AutoSize = true;
            tableProject.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableProject.ColumnCount = 8;
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            tableProject.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableProject.Controls.Add(lblProject, 0, 0);
            tableProject.Controls.Add(txtProject, 1, 0);
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
            tableProject.Dock = System.Windows.Forms.DockStyle.Fill;
            tableProject.Location = new System.Drawing.Point(6, 22);
            tableProject.Name = "tableProject";
            tableProject.Padding = new System.Windows.Forms.Padding(2);
            tableProject.RowCount = 2;
            tableProject.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableProject.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableProject.Size = new System.Drawing.Size(1248, 67);
            tableProject.TabIndex = 0;
            // 
            // lblProject
            // 
            lblProject.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProject.AutoSize = true;
            lblProject.Location = new System.Drawing.Point(5, 11);
            lblProject.Name = "lblProject";
            lblProject.Size = new System.Drawing.Size(81, 15);
            lblProject.TabIndex = 0;
            lblProject.Text = "Project folder:";
            // 
            // txtProject
            // 
            txtProject.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tableProject.SetColumnSpan(txtProject, 3);
            txtProject.Location = new System.Drawing.Point(110, 7);
            txtProject.Name = "txtProject";
            txtProject.Size = new System.Drawing.Size(490, 23);
            txtProject.TabIndex = 1;
            // 
            // btnBrowseProject
            // 
            btnBrowseProject.Location = new System.Drawing.Point(606, 5);
            btnBrowseProject.Name = "btnBrowseProject";
            btnBrowseProject.Size = new System.Drawing.Size(94, 28);
            btnBrowseProject.TabIndex = 2;
            btnBrowseProject.Text = "Browse...";
            btnBrowseProject.UseVisualStyleBackColor = true;
            btnBrowseProject.Click += BtnBrowseProject_Click;
            // 
            // btnOpenProjectFolder
            // 
            btnOpenProjectFolder.Location = new System.Drawing.Point(706, 5);
            btnOpenProjectFolder.Name = "btnOpenProjectFolder";
            btnOpenProjectFolder.Size = new System.Drawing.Size(94, 28);
            btnOpenProjectFolder.TabIndex = 3;
            btnOpenProjectFolder.Text = "Open folder";
            btnOpenProjectFolder.UseVisualStyleBackColor = true;
            btnOpenProjectFolder.Click += BtnOpenProjectFolder_Click;
            // 
            // lblTerrainFile
            // 
            lblTerrainFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTerrainFile.AutoSize = true;
            lblTerrainFile.Location = new System.Drawing.Point(806, 11);
            lblTerrainFile.Name = "lblTerrainFile";
            lblTerrainFile.Size = new System.Drawing.Size(81, 15);
            lblTerrainFile.TabIndex = 4;
            lblTerrainFile.Text = "Terrain image:";
            // 
            // txtTerrainFile
            // 
            txtTerrainFile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTerrainFile.Location = new System.Drawing.Point(916, 7);
            txtTerrainFile.Name = "txtTerrainFile";
            txtTerrainFile.Size = new System.Drawing.Size(327, 23);
            txtTerrainFile.TabIndex = 5;
            txtTerrainFile.Text = "Terrain.bmp";
            // 
            // lblMapSize
            // 
            lblMapSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMapSize.AutoSize = true;
            lblMapSize.Location = new System.Drawing.Point(5, 43);
            lblMapSize.Name = "lblMapSize";
            lblMapSize.Size = new System.Drawing.Size(56, 15);
            lblMapSize.TabIndex = 6;
            lblMapSize.Text = "Map size:";
            // 
            // cbMapSize
            // 
            cbMapSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbMapSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbMapSize.Location = new System.Drawing.Point(110, 39);
            cbMapSize.Name = "cbMapSize";
            cbMapSize.Size = new System.Drawing.Size(265, 23);
            cbMapSize.TabIndex = 7;
            cbMapSize.SelectedIndexChanged += CbMapSize_SelectedIndexChanged;
            // 
            // lblSize
            // 
            lblSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSize.AutoSize = true;
            lblSize.Location = new System.Drawing.Point(381, 43);
            lblSize.Name = "lblSize";
            lblSize.Size = new System.Drawing.Size(33, 15);
            lblSize.TabIndex = 8;
            lblSize.Text = "Tiles:";
            // 
            // flowSize
            // 
            flowSize.AutoSize = true;
            flowSize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowSize.Controls.Add(numWidth);
            flowSize.Controls.Add(lblSizeX);
            flowSize.Controls.Add(numHeight);
            flowSize.Location = new System.Drawing.Point(423, 36);
            flowSize.Margin = new System.Windows.Forms.Padding(0);
            flowSize.Name = "flowSize";
            flowSize.Size = new System.Drawing.Size(169, 29);
            flowSize.TabIndex = 9;
            // 
            // numWidth
            // 
            numWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numWidth.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Location = new System.Drawing.Point(3, 3);
            numWidth.Maximum = new decimal(new int[] { 32760, 0, 0, 0 });
            numWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numWidth.Name = "numWidth";
            numWidth.Size = new System.Drawing.Size(72, 23);
            numWidth.TabIndex = 0;
            numWidth.Value = new decimal(new int[] { 6144, 0, 0, 0 });
            // 
            // lblSizeX
            // 
            lblSizeX.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSizeX.AutoSize = true;
            lblSizeX.Location = new System.Drawing.Point(78, 10);
            lblSizeX.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            lblSizeX.Name = "lblSizeX";
            lblSizeX.Size = new System.Drawing.Size(13, 15);
            lblSizeX.TabIndex = 1;
            lblSizeX.Text = "x";
            // 
            // numHeight
            // 
            numHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numHeight.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Location = new System.Drawing.Point(94, 3);
            numHeight.Maximum = new decimal(new int[] { 32760, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numHeight.Name = "numHeight";
            numHeight.Size = new System.Drawing.Size(72, 23);
            numHeight.TabIndex = 2;
            numHeight.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            // 
            // lblMapNumber
            // 
            lblMapNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblMapNumber.AutoSize = true;
            lblMapNumber.Location = new System.Drawing.Point(606, 43);
            lblMapNumber.Name = "lblMapNumber";
            lblMapNumber.Size = new System.Drawing.Size(76, 15);
            lblMapNumber.TabIndex = 10;
            lblMapNumber.Text = "Map # (mul):";
            // 
            // numMapNumber
            // 
            numMapNumber.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numMapNumber.Location = new System.Drawing.Point(706, 39);
            numMapNumber.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numMapNumber.Name = "numMapNumber";
            numMapNumber.Size = new System.Drawing.Size(70, 23);
            numMapNumber.TabIndex = 11;
            // 
            // lblAltitudeFile
            // 
            lblAltitudeFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblAltitudeFile.AutoSize = true;
            lblAltitudeFile.Location = new System.Drawing.Point(806, 43);
            lblAltitudeFile.Name = "lblAltitudeFile";
            lblAltitudeFile.Size = new System.Drawing.Size(88, 15);
            lblAltitudeFile.TabIndex = 12;
            lblAltitudeFile.Text = "Altitude image:";
            // 
            // txtAltitudeFile
            // 
            txtAltitudeFile.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtAltitudeFile.Location = new System.Drawing.Point(916, 39);
            txtAltitudeFile.Name = "txtAltitudeFile";
            txtAltitudeFile.Size = new System.Drawing.Size(327, 23);
            txtAltitudeFile.TabIndex = 13;
            txtAltitudeFile.Text = "Altitude.bmp";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus, progressBar });
            statusStrip.Location = new System.Drawing.Point(0, 858);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1260, 22);
            statusStrip.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(1023, 17);
            lblStatus.Spring = true;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            progressBar.Name = "progressBar";
            progressBar.Size = new System.Drawing.Size(220, 16);
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitMain.Location = new System.Drawing.Point(0, 119);
            splitMain.Name = "splitMain";
            splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(splitWork);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(groupLog);
            splitMain.Size = new System.Drawing.Size(1260, 739);
            splitMain.SplitterDistance = 539;
            splitMain.TabIndex = 0;
            // 
            // splitWork
            // 
            splitWork.Dock = System.Windows.Forms.DockStyle.Fill;
            splitWork.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitWork.Location = new System.Drawing.Point(0, 0);
            splitWork.Name = "splitWork";
            // 
            // splitWork.Panel1
            // 
            splitWork.Panel1.Controls.Add(tabControl);
            // 
            // splitWork.Panel2
            // 
            splitWork.Panel2.Controls.Add(groupPreview);
            splitWork.Size = new System.Drawing.Size(1260, 539);
            splitWork.SplitterDistance = 820;
            splitWork.TabIndex = 0;
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
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.Padding = new System.Drawing.Point(12, 5);
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(820, 539);
            tabControl.TabIndex = 0;
            // 
            // tabBlank
            // 
            tabBlank.Controls.Add(tableBlank);
            tabBlank.Location = new System.Drawing.Point(4, 28);
            tabBlank.Name = "tabBlank";
            tabBlank.Padding = new System.Windows.Forms.Padding(8);
            tabBlank.Size = new System.Drawing.Size(812, 507);
            tabBlank.TabIndex = 0;
            tabBlank.Text = "1. Blank map";
            tabBlank.UseVisualStyleBackColor = true;
            // 
            // tableBlank
            // 
            tableBlank.AutoSize = true;
            tableBlank.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableBlank.ColumnCount = 2;
            tableBlank.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            tableBlank.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBlank.Controls.Add(lblBlankInfo, 0, 0);
            tableBlank.Controls.Add(lblDefaultTerrain, 0, 1);
            tableBlank.Controls.Add(cbDefaultTerrain, 1, 1);
            tableBlank.Controls.Add(chkBlankTerrainAltitude, 0, 2);
            tableBlank.Controls.Add(lblBlankAltitude, 0, 3);
            tableBlank.Controls.Add(numBlankAltitude, 1, 3);
            tableBlank.Controls.Add(chkBlankDungeon, 0, 4);
            tableBlank.Controls.Add(btnCreateBlank, 0, 5);
            tableBlank.Dock = System.Windows.Forms.DockStyle.Top;
            tableBlank.Location = new System.Drawing.Point(8, 8);
            tableBlank.Name = "tableBlank";
            tableBlank.Padding = new System.Windows.Forms.Padding(6);
            tableBlank.RowCount = 6;
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableBlank.Size = new System.Drawing.Size(796, 208);
            tableBlank.TabIndex = 0;
            // 
            // lblBlankInfo
            // 
            lblBlankInfo.AutoSize = true;
            tableBlank.SetColumnSpan(lblBlankInfo, 2);
            lblBlankInfo.Location = new System.Drawing.Point(9, 9);
            lblBlankInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 12);
            lblBlankInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblBlankInfo.Name = "lblBlankInfo";
            lblBlankInfo.Size = new System.Drawing.Size(672, 30);
            lblBlankInfo.TabIndex = 0;
            lblBlankInfo.Text = "Creates an empty map (Terrain.bmp + Altitude.bmp) with the size of the project bar. Paint it in the editor or use it as a base for templates.";
            // 
            // lblDefaultTerrain
            // 
            lblDefaultTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblDefaultTerrain.AutoSize = true;
            lblDefaultTerrain.Location = new System.Drawing.Point(9, 58);
            lblDefaultTerrain.Name = "lblDefaultTerrain";
            lblDefaultTerrain.Size = new System.Drawing.Size(62, 15);
            lblDefaultTerrain.TabIndex = 1;
            lblDefaultTerrain.Text = "Fill terrain:";
            // 
            // cbDefaultTerrain
            // 
            cbDefaultTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbDefaultTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbDefaultTerrain.DropDownHeight = 400;
            cbDefaultTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbDefaultTerrain.IntegralHeight = false;
            cbDefaultTerrain.Location = new System.Drawing.Point(219, 54);
            cbDefaultTerrain.Name = "cbDefaultTerrain";
            cbDefaultTerrain.Size = new System.Drawing.Size(300, 24);
            cbDefaultTerrain.TabIndex = 2;
            // 
            // chkBlankTerrainAltitude
            // 
            chkBlankTerrainAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBlankTerrainAltitude.AutoSize = true;
            chkBlankTerrainAltitude.Checked = true;
            chkBlankTerrainAltitude.CheckState = System.Windows.Forms.CheckState.Checked;
            tableBlank.SetColumnSpan(chkBlankTerrainAltitude, 2);
            chkBlankTerrainAltitude.Location = new System.Drawing.Point(9, 83);
            chkBlankTerrainAltitude.Name = "chkBlankTerrainAltitude";
            chkBlankTerrainAltitude.Size = new System.Drawing.Size(219, 19);
            chkBlankTerrainAltitude.TabIndex = 3;
            chkBlankTerrainAltitude.Text = "Use the default altitude of the terrain";
            chkBlankTerrainAltitude.CheckedChanged += ChkBlankTerrainAltitude_CheckedChanged;
            // 
            // lblBlankAltitude
            // 
            lblBlankAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblBlankAltitude.AutoSize = true;
            lblBlankAltitude.Location = new System.Drawing.Point(9, 112);
            lblBlankAltitude.Name = "lblBlankAltitude";
            lblBlankAltitude.Size = new System.Drawing.Size(68, 15);
            lblBlankAltitude.TabIndex = 4;
            lblBlankAltitude.Text = "Altitude (z):";
            // 
            // numBlankAltitude
            // 
            numBlankAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numBlankAltitude.Enabled = false;
            numBlankAltitude.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numBlankAltitude.Location = new System.Drawing.Point(219, 108);
            numBlankAltitude.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numBlankAltitude.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numBlankAltitude.Name = "numBlankAltitude";
            numBlankAltitude.Size = new System.Drawing.Size(90, 23);
            numBlankAltitude.TabIndex = 5;
            // 
            // chkBlankDungeon
            // 
            chkBlankDungeon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBlankDungeon.AutoSize = true;
            chkBlankDungeon.Checked = true;
            chkBlankDungeon.CheckState = System.Windows.Forms.CheckState.Checked;
            tableBlank.SetColumnSpan(chkBlankDungeon, 2);
            chkBlankDungeon.Location = new System.Drawing.Point(9, 137);
            chkBlankDungeon.Name = "chkBlankDungeon";
            chkBlankDungeon.Size = new System.Drawing.Size(391, 19);
            chkBlankDungeon.TabIndex = 6;
            chkBlankDungeon.Text = "Fill the dungeon area (x >= dungeon start of the map size) with Blank";
            // 
            // btnCreateBlank
            // 
            btnCreateBlank.Location = new System.Drawing.Point(9, 171);
            btnCreateBlank.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            btnCreateBlank.Name = "btnCreateBlank";
            btnCreateBlank.Size = new System.Drawing.Size(200, 28);
            btnCreateBlank.TabIndex = 7;
            btnCreateBlank.Text = "Create blank map";
            btnCreateBlank.UseVisualStyleBackColor = true;
            btnCreateBlank.Click += BtnCreateBlank_Click;
            // 
            // tabGenerator
            // 
            tabGenerator.Controls.Add(splitGenerator);
            tabGenerator.Location = new System.Drawing.Point(4, 28);
            tabGenerator.Name = "tabGenerator";
            tabGenerator.Padding = new System.Windows.Forms.Padding(6);
            tabGenerator.Size = new System.Drawing.Size(192, 68);
            tabGenerator.TabIndex = 1;
            tabGenerator.Text = "2. Generator";
            tabGenerator.UseVisualStyleBackColor = true;
            // 
            // splitGenerator
            // 
            splitGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            splitGenerator.Location = new System.Drawing.Point(6, 6);
            splitGenerator.Name = "splitGenerator";
            // 
            // splitGenerator.Panel1
            // 
            splitGenerator.Panel1.Controls.Add(propertyGenerator);
            // 
            // splitGenerator.Panel2
            // 
            splitGenerator.Panel2.Controls.Add(tableGenRight);
            splitGenerator.Size = new System.Drawing.Size(180, 56);
            splitGenerator.SplitterDistance = 81;
            splitGenerator.TabIndex = 0;
            // 
            // propertyGenerator
            // 
            propertyGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGenerator.Location = new System.Drawing.Point(0, 0);
            propertyGenerator.Name = "propertyGenerator";
            propertyGenerator.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propertyGenerator.Size = new System.Drawing.Size(81, 56);
            propertyGenerator.TabIndex = 0;
            propertyGenerator.ToolbarVisible = false;
            // 
            // tableGenRight
            // 
            tableGenRight.ColumnCount = 1;
            tableGenRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableGenRight.Controls.Add(groupBiomes, 0, 0);
            tableGenRight.Controls.Add(groupSketch, 0, 1);
            tableGenRight.Controls.Add(groupPresets, 0, 2);
            tableGenRight.Controls.Add(groupGenerate, 0, 3);
            tableGenRight.Dock = System.Windows.Forms.DockStyle.Fill;
            tableGenRight.Location = new System.Drawing.Point(0, 0);
            tableGenRight.Name = "tableGenRight";
            tableGenRight.RowCount = 4;
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableGenRight.Size = new System.Drawing.Size(95, 56);
            tableGenRight.TabIndex = 0;
            // 
            // groupBiomes
            // 
            groupBiomes.Controls.Add(gridBiomes);
            groupBiomes.Controls.Add(flowBiomeButtons);
            groupBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBiomes.Location = new System.Drawing.Point(3, 3);
            groupBiomes.Name = "groupBiomes";
            groupBiomes.Padding = new System.Windows.Forms.Padding(6);
            groupBiomes.Size = new System.Drawing.Size(89, 1);
            groupBiomes.TabIndex = 0;
            groupBiomes.TabStop = false;
            groupBiomes.Text = "Biomes (random terrain patches on the land, applied from top to bottom)";
            // 
            // gridBiomes
            // 
            gridBiomes.AllowUserToAddRows = false;
            gridBiomes.AllowUserToResizeRows = false;
            gridBiomes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            gridBiomes.BackgroundColor = System.Drawing.SystemColors.Window;
            gridBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridBiomes.Location = new System.Drawing.Point(6, 22);
            gridBiomes.Name = "gridBiomes";
            gridBiomes.RowHeadersWidth = 24;
            gridBiomes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            gridBiomes.Size = new System.Drawing.Size(77, 0);
            gridBiomes.TabIndex = 0;
            gridBiomes.CurrentCellDirtyStateChanged += GridBiomes_CurrentCellDirtyStateChanged;
            gridBiomes.DataError += GridBiomes_DataError;
            // 
            // flowBiomeButtons
            // 
            flowBiomeButtons.AutoSize = true;
            flowBiomeButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowBiomeButtons.Controls.Add(btnAddBiome);
            flowBiomeButtons.Controls.Add(btnRemoveBiome);
            flowBiomeButtons.Controls.Add(btnBiomeUp);
            flowBiomeButtons.Controls.Add(btnBiomeDown);
            flowBiomeButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowBiomeButtons.Location = new System.Drawing.Point(6, -114);
            flowBiomeButtons.Name = "flowBiomeButtons";
            flowBiomeButtons.Size = new System.Drawing.Size(77, 136);
            flowBiomeButtons.TabIndex = 1;
            // 
            // btnAddBiome
            // 
            btnAddBiome.Location = new System.Drawing.Point(3, 3);
            btnAddBiome.Name = "btnAddBiome";
            btnAddBiome.Size = new System.Drawing.Size(110, 28);
            btnAddBiome.TabIndex = 0;
            btnAddBiome.Text = "Add biome";
            btnAddBiome.UseVisualStyleBackColor = true;
            btnAddBiome.Click += BtnAddBiome_Click;
            // 
            // btnRemoveBiome
            // 
            btnRemoveBiome.Location = new System.Drawing.Point(3, 37);
            btnRemoveBiome.Name = "btnRemoveBiome";
            btnRemoveBiome.Size = new System.Drawing.Size(90, 28);
            btnRemoveBiome.TabIndex = 1;
            btnRemoveBiome.Text = "Remove";
            btnRemoveBiome.UseVisualStyleBackColor = true;
            btnRemoveBiome.Click += BtnRemoveBiome_Click;
            // 
            // btnBiomeUp
            // 
            btnBiomeUp.Location = new System.Drawing.Point(3, 71);
            btnBiomeUp.Name = "btnBiomeUp";
            btnBiomeUp.Size = new System.Drawing.Size(60, 28);
            btnBiomeUp.TabIndex = 2;
            btnBiomeUp.Text = "Up";
            btnBiomeUp.UseVisualStyleBackColor = true;
            btnBiomeUp.Click += BtnBiomeUp_Click;
            // 
            // btnBiomeDown
            // 
            btnBiomeDown.Location = new System.Drawing.Point(3, 105);
            btnBiomeDown.Name = "btnBiomeDown";
            btnBiomeDown.Size = new System.Drawing.Size(60, 28);
            btnBiomeDown.TabIndex = 3;
            btnBiomeDown.Text = "Down";
            btnBiomeDown.UseVisualStyleBackColor = true;
            btnBiomeDown.Click += BtnBiomeDown_Click;
            // 
            // groupSketch
            // 
            groupSketch.AutoSize = true;
            groupSketch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupSketch.Controls.Add(tableSketch);
            groupSketch.Dock = System.Windows.Forms.DockStyle.Top;
            groupSketch.Location = new System.Drawing.Point(3, -224);
            groupSketch.Name = "groupSketch";
            groupSketch.Padding = new System.Windows.Forms.Padding(6);
            groupSketch.Size = new System.Drawing.Size(89, 141);
            groupSketch.TabIndex = 1;
            groupSketch.TabStop = false;
            groupSketch.Text = "Sketch / height map (optional)";
            // 
            // tableSketch
            // 
            tableSketch.AutoSize = true;
            tableSketch.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableSketch.ColumnCount = 3;
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableSketch.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableSketch.Controls.Add(lblSketchInfo, 0, 0);
            tableSketch.Controls.Add(btnLoadSketch, 1, 0);
            tableSketch.Controls.Add(btnClearSketch, 2, 0);
            tableSketch.Controls.Add(lblHeightTemplateInfo, 0, 1);
            tableSketch.Controls.Add(btnLoadHeightTemplate, 1, 1);
            tableSketch.Controls.Add(btnClearHeightTemplate, 2, 1);
            tableSketch.Controls.Add(lblSketchHelp, 0, 2);
            tableSketch.Dock = System.Windows.Forms.DockStyle.Top;
            tableSketch.Location = new System.Drawing.Point(6, 22);
            tableSketch.Name = "tableSketch";
            tableSketch.RowCount = 3;
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableSketch.Size = new System.Drawing.Size(77, 383);
            tableSketch.TabIndex = 0;
            // 
            // lblSketchInfo
            // 
            lblSketchInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSketchInfo.AutoSize = true;
            lblSketchInfo.Location = new System.Drawing.Point(3, 9);
            lblSketchInfo.Name = "lblSketchInfo";
            lblSketchInfo.Size = new System.Drawing.Size(1, 15);
            lblSketchInfo.TabIndex = 0;
            lblSketchInfo.Text = "Sketch: none";
            // 
            // btnLoadSketch
            // 
            btnLoadSketch.Location = new System.Drawing.Point(-142, 3);
            btnLoadSketch.Name = "btnLoadSketch";
            btnLoadSketch.Size = new System.Drawing.Size(140, 28);
            btnLoadSketch.TabIndex = 1;
            btnLoadSketch.Text = "Load sketch...";
            btnLoadSketch.UseVisualStyleBackColor = true;
            btnLoadSketch.Click += BtnLoadSketch_Click;
            // 
            // btnClearSketch
            // 
            btnClearSketch.Location = new System.Drawing.Point(4, 3);
            btnClearSketch.Name = "btnClearSketch";
            btnClearSketch.Size = new System.Drawing.Size(70, 28);
            btnClearSketch.TabIndex = 2;
            btnClearSketch.Text = "Clear";
            btnClearSketch.UseVisualStyleBackColor = true;
            btnClearSketch.Click += BtnClearSketch_Click;
            // 
            // lblHeightTemplateInfo
            // 
            lblHeightTemplateInfo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblHeightTemplateInfo.AutoSize = true;
            lblHeightTemplateInfo.Location = new System.Drawing.Point(3, 43);
            lblHeightTemplateInfo.Name = "lblHeightTemplateInfo";
            lblHeightTemplateInfo.Size = new System.Drawing.Size(1, 15);
            lblHeightTemplateInfo.TabIndex = 3;
            lblHeightTemplateInfo.Text = "Height map: none";
            // 
            // btnLoadHeightTemplate
            // 
            btnLoadHeightTemplate.Location = new System.Drawing.Point(-142, 37);
            btnLoadHeightTemplate.Name = "btnLoadHeightTemplate";
            btnLoadHeightTemplate.Size = new System.Drawing.Size(140, 28);
            btnLoadHeightTemplate.TabIndex = 4;
            btnLoadHeightTemplate.Text = "Load height map...";
            btnLoadHeightTemplate.UseVisualStyleBackColor = true;
            btnLoadHeightTemplate.Click += BtnLoadHeightTemplate_Click;
            // 
            // btnClearHeightTemplate
            // 
            btnClearHeightTemplate.Location = new System.Drawing.Point(4, 37);
            btnClearHeightTemplate.Name = "btnClearHeightTemplate";
            btnClearHeightTemplate.Size = new System.Drawing.Size(70, 28);
            btnClearHeightTemplate.TabIndex = 5;
            btnClearHeightTemplate.Text = "Clear";
            btnClearHeightTemplate.UseVisualStyleBackColor = true;
            btnClearHeightTemplate.Click += BtnClearHeightTemplate_Click;
            // 
            // lblSketchHelp
            // 
            lblSketchHelp.AutoSize = true;
            tableSketch.SetColumnSpan(lblSketchHelp, 3);
            lblSketchHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblSketchHelp.Location = new System.Drawing.Point(3, 68);
            lblSketchHelp.MaximumSize = new System.Drawing.Size(440, 0);
            lblSketchHelp.Name = "lblSketchHelp";
            lblSketchHelp.Size = new System.Drawing.Size(71, 315);
            lblSketchHelp.TabIndex = 6;
            lblSketchHelp.Text = "Sketch = image painted with terrain colours (transparent / black / [00] No Draw = generator decides). Height map = grey scale image (dark = sea, bright = mountains). Both are scaled to the map size.";
            // 
            // groupPresets
            // 
            groupPresets.AutoSize = true;
            groupPresets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupPresets.Controls.Add(flowPresets);
            groupPresets.Dock = System.Windows.Forms.DockStyle.Top;
            groupPresets.Location = new System.Drawing.Point(3, -77);
            groupPresets.Name = "groupPresets";
            groupPresets.Padding = new System.Windows.Forms.Padding(6);
            groupPresets.Size = new System.Drawing.Size(89, 62);
            groupPresets.TabIndex = 2;
            groupPresets.TabStop = false;
            groupPresets.Text = "Presets";
            // 
            // flowPresets
            // 
            flowPresets.AutoSize = true;
            flowPresets.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowPresets.Controls.Add(btnRandomSeed);
            flowPresets.Controls.Add(btnPresetLoad);
            flowPresets.Controls.Add(btnPresetSave);
            flowPresets.Controls.Add(btnPresetDefaults);
            flowPresets.Dock = System.Windows.Forms.DockStyle.Top;
            flowPresets.Location = new System.Drawing.Point(6, 22);
            flowPresets.Name = "flowPresets";
            flowPresets.Size = new System.Drawing.Size(77, 136);
            flowPresets.TabIndex = 0;
            // 
            // btnRandomSeed
            // 
            btnRandomSeed.Location = new System.Drawing.Point(3, 3);
            btnRandomSeed.Name = "btnRandomSeed";
            btnRandomSeed.Size = new System.Drawing.Size(130, 28);
            btnRandomSeed.TabIndex = 0;
            btnRandomSeed.Text = "New random seed";
            btnRandomSeed.UseVisualStyleBackColor = true;
            btnRandomSeed.Click += BtnRandomSeed_Click;
            // 
            // btnPresetLoad
            // 
            btnPresetLoad.Location = new System.Drawing.Point(3, 37);
            btnPresetLoad.Name = "btnPresetLoad";
            btnPresetLoad.Size = new System.Drawing.Size(110, 28);
            btnPresetLoad.TabIndex = 1;
            btnPresetLoad.Text = "Load preset...";
            btnPresetLoad.UseVisualStyleBackColor = true;
            btnPresetLoad.Click += BtnPresetLoad_Click;
            // 
            // btnPresetSave
            // 
            btnPresetSave.Location = new System.Drawing.Point(3, 71);
            btnPresetSave.Name = "btnPresetSave";
            btnPresetSave.Size = new System.Drawing.Size(110, 28);
            btnPresetSave.TabIndex = 2;
            btnPresetSave.Text = "Save preset...";
            btnPresetSave.UseVisualStyleBackColor = true;
            btnPresetSave.Click += BtnPresetSave_Click;
            // 
            // btnPresetDefaults
            // 
            btnPresetDefaults.Location = new System.Drawing.Point(3, 105);
            btnPresetDefaults.Name = "btnPresetDefaults";
            btnPresetDefaults.Size = new System.Drawing.Size(80, 28);
            btnPresetDefaults.TabIndex = 3;
            btnPresetDefaults.Text = "Defaults";
            btnPresetDefaults.UseVisualStyleBackColor = true;
            btnPresetDefaults.Click += BtnPresetDefaults_Click;
            // 
            // groupGenerate
            // 
            groupGenerate.AutoSize = true;
            groupGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupGenerate.Controls.Add(flowGenerate);
            groupGenerate.Dock = System.Windows.Forms.DockStyle.Top;
            groupGenerate.Location = new System.Drawing.Point(3, -9);
            groupGenerate.Name = "groupGenerate";
            groupGenerate.Padding = new System.Windows.Forms.Padding(6);
            groupGenerate.Size = new System.Drawing.Size(89, 62);
            groupGenerate.TabIndex = 3;
            groupGenerate.TabStop = false;
            groupGenerate.Text = "Generate";
            // 
            // flowGenerate
            // 
            flowGenerate.AutoSize = true;
            flowGenerate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowGenerate.Controls.Add(btnPreview);
            flowGenerate.Controls.Add(btnGenerate);
            flowGenerate.Controls.Add(chkRepairAfterGenerate);
            flowGenerate.Controls.Add(chkSaveAfterGenerate);
            flowGenerate.Dock = System.Windows.Forms.DockStyle.Top;
            flowGenerate.Location = new System.Drawing.Point(6, 22);
            flowGenerate.Name = "flowGenerate";
            flowGenerate.Size = new System.Drawing.Size(77, 118);
            flowGenerate.TabIndex = 0;
            // 
            // btnPreview
            // 
            btnPreview.Location = new System.Drawing.Point(3, 3);
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new System.Drawing.Size(130, 28);
            btnPreview.TabIndex = 0;
            btnPreview.Text = "Quick preview";
            btnPreview.UseVisualStyleBackColor = true;
            btnPreview.Click += BtnPreview_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnGenerate.Location = new System.Drawing.Point(3, 37);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(150, 28);
            btnGenerate.TabIndex = 1;
            btnGenerate.Text = "Generate full map";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += BtnGenerate_Click;
            // 
            // chkRepairAfterGenerate
            // 
            chkRepairAfterGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRepairAfterGenerate.AutoSize = true;
            chkRepairAfterGenerate.Checked = true;
            chkRepairAfterGenerate.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRepairAfterGenerate.Location = new System.Drawing.Point(3, 71);
            chkRepairAfterGenerate.Name = "chkRepairAfterGenerate";
            chkRepairAfterGenerate.Size = new System.Drawing.Size(117, 19);
            chkRepairAfterGenerate.TabIndex = 2;
            chkRepairAfterGenerate.Text = "Repair transitions";
            // 
            // chkSaveAfterGenerate
            // 
            chkSaveAfterGenerate.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkSaveAfterGenerate.AutoSize = true;
            chkSaveAfterGenerate.Checked = true;
            chkSaveAfterGenerate.CheckState = System.Windows.Forms.CheckState.Checked;
            chkSaveAfterGenerate.Location = new System.Drawing.Point(3, 96);
            chkSaveAfterGenerate.Name = "chkSaveAfterGenerate";
            chkSaveAfterGenerate.Size = new System.Drawing.Size(104, 19);
            chkSaveAfterGenerate.TabIndex = 3;
            chkSaveAfterGenerate.Text = "Save to project";
            // 
            // tabTemplates
            // 
            tabTemplates.Controls.Add(splitTemplates);
            tabTemplates.Controls.Add(tableTemplateTop);
            tabTemplates.Location = new System.Drawing.Point(4, 28);
            tabTemplates.Name = "tabTemplates";
            tabTemplates.Padding = new System.Windows.Forms.Padding(6);
            tabTemplates.Size = new System.Drawing.Size(192, 68);
            tabTemplates.TabIndex = 2;
            tabTemplates.Text = "3. Templates";
            tabTemplates.UseVisualStyleBackColor = true;
            // 
            // splitTemplates
            // 
            splitTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            splitTemplates.Location = new System.Drawing.Point(6, 40);
            splitTemplates.Name = "splitTemplates";
            // 
            // splitTemplates.Panel1
            // 
            splitTemplates.Panel1.Controls.Add(listTemplates);
            // 
            // splitTemplates.Panel2
            // 
            splitTemplates.Panel2.Controls.Add(tableTemplateRight);
            splitTemplates.Size = new System.Drawing.Size(180, 22);
            splitTemplates.SplitterDistance = 94;
            splitTemplates.TabIndex = 0;
            // 
            // listTemplates
            // 
            listTemplates.Dock = System.Windows.Forms.DockStyle.Fill;
            listTemplates.Location = new System.Drawing.Point(0, 0);
            listTemplates.MultiSelect = false;
            listTemplates.Name = "listTemplates";
            listTemplates.Size = new System.Drawing.Size(94, 22);
            listTemplates.TabIndex = 0;
            listTemplates.UseCompatibleStateImageBehavior = false;
            listTemplates.SelectedIndexChanged += ListTemplates_SelectedIndexChanged;
            listTemplates.DoubleClick += BtnTemplateApply_Click;
            // 
            // tableTemplateRight
            // 
            tableTemplateRight.ColumnCount = 2;
            tableTemplateRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            tableTemplateRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateRight.Controls.Add(picTemplate, 0, 0);
            tableTemplateRight.Controls.Add(lblTemplateInfo, 0, 1);
            tableTemplateRight.Controls.Add(lblTemplateUse, 0, 2);
            tableTemplateRight.Controls.Add(flowTemplateUse, 1, 2);
            tableTemplateRight.Controls.Add(lblScaleMode, 0, 3);
            tableTemplateRight.Controls.Add(cbScaleMode, 1, 3);
            tableTemplateRight.Controls.Add(lblTemplateFill, 0, 4);
            tableTemplateRight.Controls.Add(cbTemplateFill, 1, 4);
            tableTemplateRight.Controls.Add(chkNaturalOnly, 0, 5);
            tableTemplateRight.Controls.Add(btnTemplateApply, 0, 6);
            tableTemplateRight.Controls.Add(lblTemplateHelp, 0, 7);
            tableTemplateRight.Dock = System.Windows.Forms.DockStyle.Fill;
            tableTemplateRight.Location = new System.Drawing.Point(0, 0);
            tableTemplateRight.Name = "tableTemplateRight";
            tableTemplateRight.Padding = new System.Windows.Forms.Padding(4);
            tableTemplateRight.RowCount = 9;
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateRight.Size = new System.Drawing.Size(82, 22);
            tableTemplateRight.TabIndex = 0;
            // 
            // picTemplate
            // 
            picTemplate.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
            tableTemplateRight.SetColumnSpan(picTemplate, 2);
            picTemplate.Dock = System.Windows.Forms.DockStyle.Fill;
            picTemplate.Location = new System.Drawing.Point(7, 7);
            picTemplate.MinimumSize = new System.Drawing.Size(100, 180);
            picTemplate.Name = "picTemplate";
            picTemplate.Size = new System.Drawing.Size(100, 180);
            picTemplate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picTemplate.TabIndex = 0;
            picTemplate.TabStop = false;
            // 
            // lblTemplateInfo
            // 
            lblTemplateInfo.AutoSize = true;
            tableTemplateRight.SetColumnSpan(lblTemplateInfo, 2);
            lblTemplateInfo.Location = new System.Drawing.Point(7, -565);
            lblTemplateInfo.MaximumSize = new System.Drawing.Size(420, 0);
            lblTemplateInfo.Name = "lblTemplateInfo";
            lblTemplateInfo.Size = new System.Drawing.Size(57, 30);
            lblTemplateInfo.TabIndex = 1;
            lblTemplateInfo.Text = "Select an image.";
            // 
            // lblTemplateUse
            // 
            lblTemplateUse.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateUse.AutoSize = true;
            lblTemplateUse.Location = new System.Drawing.Point(7, -490);
            lblTemplateUse.Name = "lblTemplateUse";
            lblTemplateUse.Size = new System.Drawing.Size(43, 15);
            lblTemplateUse.TabIndex = 2;
            lblTemplateUse.Text = "Use as:";
            // 
            // flowTemplateUse
            // 
            flowTemplateUse.AutoSize = true;
            flowTemplateUse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowTemplateUse.Controls.Add(rbTplConvert);
            flowTemplateUse.Controls.Add(rbTplSketch);
            flowTemplateUse.Controls.Add(rbTplHeight);
            flowTemplateUse.Controls.Add(rbTplStamp);
            flowTemplateUse.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowTemplateUse.Location = new System.Drawing.Point(127, -532);
            flowTemplateUse.Name = "flowTemplateUse";
            flowTemplateUse.Size = new System.Drawing.Size(1, 100);
            flowTemplateUse.TabIndex = 3;
            // 
            // rbTplConvert
            // 
            rbTplConvert.AutoSize = true;
            rbTplConvert.Checked = true;
            rbTplConvert.Location = new System.Drawing.Point(3, 3);
            rbTplConvert.Name = "rbTplConvert";
            rbTplConvert.Size = new System.Drawing.Size(302, 19);
            rbTplConvert.TabIndex = 0;
            rbTplConvert.TabStop = true;
            rbTplConvert.Text = "Map (terrain colours -> Terrain.bmp + Altitude.bmp)";
            // 
            // rbTplSketch
            // 
            rbTplSketch.AutoSize = true;
            rbTplSketch.Location = new System.Drawing.Point(3, 28);
            rbTplSketch.Name = "rbTplSketch";
            rbTplSketch.Size = new System.Drawing.Size(152, 19);
            rbTplSketch.TabIndex = 1;
            rbTplSketch.Text = "Sketch for the generator";
            // 
            // rbTplHeight
            // 
            rbTplHeight.AutoSize = true;
            rbTplHeight.Location = new System.Drawing.Point(3, 53);
            rbTplHeight.Name = "rbTplHeight";
            rbTplHeight.Size = new System.Drawing.Size(243, 19);
            rbTplHeight.TabIndex = 2;
            rbTplHeight.Text = "Height map for the generator (grey scale)";
            // 
            // rbTplStamp
            // 
            rbTplStamp.AutoSize = true;
            rbTplStamp.Location = new System.Drawing.Point(3, 78);
            rbTplStamp.Name = "rbTplStamp";
            rbTplStamp.Size = new System.Drawing.Size(167, 19);
            rbTplStamp.TabIndex = 3;
            rbTplStamp.Text = "Stamp pattern in the editor";
            // 
            // lblScaleMode
            // 
            lblScaleMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblScaleMode.AutoSize = true;
            lblScaleMode.Location = new System.Drawing.Point(7, -422);
            lblScaleMode.Name = "lblScaleMode";
            lblScaleMode.Size = new System.Drawing.Size(48, 15);
            lblScaleMode.TabIndex = 4;
            lblScaleMode.Text = "Scaling:";
            // 
            // cbScaleMode
            // 
            cbScaleMode.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbScaleMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbScaleMode.Location = new System.Drawing.Point(127, -426);
            cbScaleMode.Name = "cbScaleMode";
            cbScaleMode.Size = new System.Drawing.Size(1, 23);
            cbScaleMode.TabIndex = 5;
            // 
            // lblTemplateFill
            // 
            lblTemplateFill.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateFill.AutoSize = true;
            lblTemplateFill.Location = new System.Drawing.Point(7, -393);
            lblTemplateFill.Name = "lblTemplateFill";
            lblTemplateFill.Size = new System.Drawing.Size(82, 15);
            lblTemplateFill.TabIndex = 6;
            lblTemplateFill.Text = "Fill / No Draw:";
            // 
            // cbTemplateFill
            // 
            cbTemplateFill.Anchor = System.Windows.Forms.AnchorStyles.Left;
            cbTemplateFill.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbTemplateFill.DropDownHeight = 400;
            cbTemplateFill.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbTemplateFill.IntegralHeight = false;
            cbTemplateFill.Location = new System.Drawing.Point(127, -397);
            cbTemplateFill.Name = "cbTemplateFill";
            cbTemplateFill.Size = new System.Drawing.Size(1, 24);
            cbTemplateFill.TabIndex = 7;
            // 
            // chkNaturalOnly
            // 
            chkNaturalOnly.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkNaturalOnly.AutoSize = true;
            tableTemplateRight.SetColumnSpan(chkNaturalOnly, 2);
            chkNaturalOnly.Location = new System.Drawing.Point(7, -368);
            chkNaturalOnly.Name = "chkNaturalOnly";
            chkNaturalOnly.Size = new System.Drawing.Size(68, 19);
            chkNaturalOnly.TabIndex = 8;
            chkNaturalOnly.Text = "Only natural terrains (better for photos / drawings)";
            // 
            // btnTemplateApply
            // 
            tableTemplateRight.SetColumnSpan(btnTemplateApply, 2);
            btnTemplateApply.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnTemplateApply.Location = new System.Drawing.Point(7, -343);
            btnTemplateApply.Name = "btnTemplateApply";
            btnTemplateApply.Size = new System.Drawing.Size(68, 28);
            btnTemplateApply.TabIndex = 9;
            btnTemplateApply.Text = "Apply";
            btnTemplateApply.UseVisualStyleBackColor = true;
            btnTemplateApply.Click += BtnTemplateApply_Click;
            // 
            // lblTemplateHelp
            // 
            lblTemplateHelp.AutoSize = true;
            tableTemplateRight.SetColumnSpan(lblTemplateHelp, 2);
            lblTemplateHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblTemplateHelp.Location = new System.Drawing.Point(7, -312);
            lblTemplateHelp.MaximumSize = new System.Drawing.Size(420, 0);
            lblTemplateHelp.Name = "lblTemplateHelp";
            lblTemplateHelp.Size = new System.Drawing.Size(68, 330);
            lblTemplateHelp.TabIndex = 10;
            lblTemplateHelp.Text = resources.GetString("lblTemplateHelp.Text");
            // 
            // tableTemplateTop
            // 
            tableTemplateTop.AutoSize = true;
            tableTemplateTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableTemplateTop.ColumnCount = 5;
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableTemplateTop.Controls.Add(lblTemplateFolder, 0, 0);
            tableTemplateTop.Controls.Add(txtTemplateFolder, 1, 0);
            tableTemplateTop.Controls.Add(btnTemplateBrowse, 2, 0);
            tableTemplateTop.Controls.Add(btnTemplateRefresh, 3, 0);
            tableTemplateTop.Controls.Add(btnTemplateOpenFolder, 4, 0);
            tableTemplateTop.Dock = System.Windows.Forms.DockStyle.Top;
            tableTemplateTop.Location = new System.Drawing.Point(6, 6);
            tableTemplateTop.Name = "tableTemplateTop";
            tableTemplateTop.RowCount = 1;
            tableTemplateTop.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableTemplateTop.Size = new System.Drawing.Size(180, 34);
            tableTemplateTop.TabIndex = 1;
            // 
            // lblTemplateFolder
            // 
            lblTemplateFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTemplateFolder.AutoSize = true;
            lblTemplateFolder.Location = new System.Drawing.Point(3, 9);
            lblTemplateFolder.Name = "lblTemplateFolder";
            lblTemplateFolder.Size = new System.Drawing.Size(92, 15);
            lblTemplateFolder.TabIndex = 0;
            lblTemplateFolder.Text = "Template folder:";
            // 
            // txtTemplateFolder
            // 
            txtTemplateFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTemplateFolder.Location = new System.Drawing.Point(103, 5);
            txtTemplateFolder.Name = "txtTemplateFolder";
            txtTemplateFolder.Size = new System.Drawing.Size(1, 23);
            txtTemplateFolder.TabIndex = 1;
            // 
            // btnTemplateBrowse
            // 
            btnTemplateBrowse.Location = new System.Drawing.Point(-95, 3);
            btnTemplateBrowse.Name = "btnTemplateBrowse";
            btnTemplateBrowse.Size = new System.Drawing.Size(90, 28);
            btnTemplateBrowse.TabIndex = 2;
            btnTemplateBrowse.Text = "Browse...";
            btnTemplateBrowse.UseVisualStyleBackColor = true;
            btnTemplateBrowse.Click += BtnTemplateBrowse_Click;
            // 
            // btnTemplateRefresh
            // 
            btnTemplateRefresh.Location = new System.Drawing.Point(1, 3);
            btnTemplateRefresh.Name = "btnTemplateRefresh";
            btnTemplateRefresh.Size = new System.Drawing.Size(80, 28);
            btnTemplateRefresh.TabIndex = 3;
            btnTemplateRefresh.Text = "Refresh";
            btnTemplateRefresh.UseVisualStyleBackColor = true;
            btnTemplateRefresh.Click += BtnTemplateRefresh_Click;
            // 
            // btnTemplateOpenFolder
            // 
            btnTemplateOpenFolder.Location = new System.Drawing.Point(87, 3);
            btnTemplateOpenFolder.Name = "btnTemplateOpenFolder";
            btnTemplateOpenFolder.Size = new System.Drawing.Size(90, 28);
            btnTemplateOpenFolder.TabIndex = 4;
            btnTemplateOpenFolder.Text = "Open folder";
            btnTemplateOpenFolder.UseVisualStyleBackColor = true;
            btnTemplateOpenFolder.Click += BtnTemplateOpenFolder_Click;
            // 
            // tabHeights
            // 
            tabHeights.Controls.Add(tableHeights);
            tabHeights.Location = new System.Drawing.Point(4, 28);
            tabHeights.Name = "tabHeights";
            tabHeights.Padding = new System.Windows.Forms.Padding(8);
            tabHeights.Size = new System.Drawing.Size(192, 68);
            tabHeights.TabIndex = 3;
            tabHeights.Text = "4. Heights";
            tabHeights.UseVisualStyleBackColor = true;
            // 
            // tableHeights
            // 
            tableHeights.AutoSize = true;
            tableHeights.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableHeights.ColumnCount = 5;
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableHeights.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableHeights.Controls.Add(lblHeightsInfo, 0, 0);
            tableHeights.Controls.Add(lblFilterType, 0, 1);
            tableHeights.Controls.Add(cbFilterType, 1, 1);
            tableHeights.Controls.Add(lblFilterTerrain, 0, 2);
            tableHeights.Controls.Add(cbFilterTerrain, 1, 2);
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
            tableHeights.Controls.Add(lblResetAltitude, 0, 10);
            tableHeights.Controls.Add(btnResetAltitude, 3, 10);
            tableHeights.Controls.Add(btnHeightUndo, 0, 11);
            tableHeights.Controls.Add(lblHeightStats, 0, 12);
            tableHeights.Dock = System.Windows.Forms.DockStyle.Top;
            tableHeights.Location = new System.Drawing.Point(8, 8);
            tableHeights.Name = "tableHeights";
            tableHeights.Padding = new System.Windows.Forms.Padding(6);
            tableHeights.RowCount = 13;
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
            tableHeights.Size = new System.Drawing.Size(176, 513);
            tableHeights.TabIndex = 0;
            // 
            // lblHeightsInfo
            // 
            lblHeightsInfo.AutoSize = true;
            tableHeights.SetColumnSpan(lblHeightsInfo, 5);
            lblHeightsInfo.Location = new System.Drawing.Point(9, 9);
            lblHeightsInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            lblHeightsInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblHeightsInfo.Name = "lblHeightsInfo";
            lblHeightsInfo.Size = new System.Drawing.Size(155, 90);
            lblHeightsInfo.TabIndex = 0;
            lblHeightsInfo.Text = "Changes the altitude of the current map. Heights are stored in steps of 5 (see Altitude.xml), the type (water / land / mountain) is kept.";
            // 
            // lblFilterType
            // 
            lblFilterType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFilterType.AutoSize = true;
            lblFilterType.Location = new System.Drawing.Point(9, 116);
            lblFilterType.Name = "lblFilterType";
            lblFilterType.Size = new System.Drawing.Size(104, 15);
            lblFilterType.TabIndex = 1;
            lblFilterType.Text = "Only altitude type:";
            // 
            // cbFilterType
            // 
            cbFilterType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tableHeights.SetColumnSpan(cbFilterType, 3);
            cbFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFilterType.Location = new System.Drawing.Point(209, 112);
            cbFilterType.Name = "cbFilterType";
            cbFilterType.Size = new System.Drawing.Size(150, 23);
            cbFilterType.TabIndex = 2;
            // 
            // lblFilterTerrain
            // 
            lblFilterTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFilterTerrain.AutoSize = true;
            lblFilterTerrain.Location = new System.Drawing.Point(9, 145);
            lblFilterTerrain.Name = "lblFilterTerrain";
            lblFilterTerrain.Size = new System.Drawing.Size(72, 15);
            lblFilterTerrain.TabIndex = 3;
            lblFilterTerrain.Text = "Only terrain:";
            // 
            // cbFilterTerrain
            // 
            cbFilterTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tableHeights.SetColumnSpan(cbFilterTerrain, 3);
            cbFilterTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbFilterTerrain.DropDownHeight = 400;
            cbFilterTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFilterTerrain.IntegralHeight = false;
            cbFilterTerrain.Location = new System.Drawing.Point(209, 141);
            cbFilterTerrain.Name = "cbFilterTerrain";
            cbFilterTerrain.Size = new System.Drawing.Size(300, 24);
            cbFilterTerrain.TabIndex = 4;
            // 
            // lblOffset
            // 
            lblOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblOffset.AutoSize = true;
            lblOffset.Location = new System.Drawing.Point(9, 176);
            lblOffset.Name = "lblOffset";
            lblOffset.Size = new System.Drawing.Size(109, 15);
            lblOffset.TabIndex = 5;
            lblOffset.Text = "Raise / lower by (z):";
            // 
            // numOffset
            // 
            numOffset.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numOffset.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numOffset.Location = new System.Drawing.Point(209, 172);
            numOffset.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numOffset.Minimum = new decimal(new int[] { 200, 0, 0, int.MinValue });
            numOffset.Name = "numOffset";
            numOffset.Size = new System.Drawing.Size(90, 23);
            numOffset.TabIndex = 6;
            numOffset.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // btnOffset
            // 
            btnOffset.Location = new System.Drawing.Point(401, 170);
            btnOffset.Name = "btnOffset";
            btnOffset.Size = new System.Drawing.Size(90, 28);
            btnOffset.TabIndex = 7;
            btnOffset.Text = "Apply";
            btnOffset.UseVisualStyleBackColor = true;
            btnOffset.Click += BtnOffset_Click;
            // 
            // lblScale
            // 
            lblScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(9, 210);
            lblScale.Name = "lblScale";
            lblScale.Size = new System.Drawing.Size(107, 15);
            lblScale.TabIndex = 8;
            lblScale.Text = "Multiply height by:";
            // 
            // numScale
            // 
            numScale.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numScale.DecimalPlaces = 2;
            numScale.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numScale.Location = new System.Drawing.Point(209, 206);
            numScale.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numScale.Name = "numScale";
            numScale.Size = new System.Drawing.Size(90, 23);
            numScale.TabIndex = 9;
            numScale.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            // 
            // btnScale
            // 
            btnScale.Location = new System.Drawing.Point(401, 204);
            btnScale.Name = "btnScale";
            btnScale.Size = new System.Drawing.Size(90, 28);
            btnScale.TabIndex = 10;
            btnScale.Text = "Apply";
            btnScale.UseVisualStyleBackColor = true;
            btnScale.Click += BtnScale_Click;
            // 
            // lblSmooth
            // 
            lblSmooth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblSmooth.AutoSize = true;
            lblSmooth.Location = new System.Drawing.Point(9, 244);
            lblSmooth.Name = "lblSmooth";
            lblSmooth.Size = new System.Drawing.Size(97, 15);
            lblSmooth.TabIndex = 11;
            lblSmooth.Text = "Smooth (passes):";
            // 
            // numSmooth
            // 
            numSmooth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numSmooth.Location = new System.Drawing.Point(209, 240);
            numSmooth.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numSmooth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSmooth.Name = "numSmooth";
            numSmooth.Size = new System.Drawing.Size(90, 23);
            numSmooth.TabIndex = 12;
            numSmooth.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // btnSmooth
            // 
            btnSmooth.Location = new System.Drawing.Point(401, 238);
            btnSmooth.Name = "btnSmooth";
            btnSmooth.Size = new System.Drawing.Size(90, 28);
            btnSmooth.TabIndex = 13;
            btnSmooth.Text = "Apply";
            btnSmooth.UseVisualStyleBackColor = true;
            btnSmooth.Click += BtnSmooth_Click;
            // 
            // lblRoughen
            // 
            lblRoughen.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblRoughen.AutoSize = true;
            lblRoughen.Location = new System.Drawing.Point(9, 278);
            lblRoughen.Name = "lblRoughen";
            lblRoughen.Size = new System.Drawing.Size(104, 15);
            lblRoughen.TabIndex = 14;
            lblRoughen.Text = "Roughen (z / size):";
            // 
            // numRoughAmp
            // 
            numRoughAmp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numRoughAmp.Location = new System.Drawing.Point(209, 274);
            numRoughAmp.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            numRoughAmp.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRoughAmp.Name = "numRoughAmp";
            numRoughAmp.Size = new System.Drawing.Size(90, 23);
            numRoughAmp.TabIndex = 15;
            numRoughAmp.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // numRoughSize
            // 
            numRoughSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numRoughSize.Location = new System.Drawing.Point(305, 274);
            numRoughSize.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numRoughSize.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numRoughSize.Name = "numRoughSize";
            numRoughSize.Size = new System.Drawing.Size(90, 23);
            numRoughSize.TabIndex = 16;
            numRoughSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // btnRoughen
            // 
            btnRoughen.Location = new System.Drawing.Point(401, 272);
            btnRoughen.Name = "btnRoughen";
            btnRoughen.Size = new System.Drawing.Size(90, 28);
            btnRoughen.TabIndex = 17;
            btnRoughen.Text = "Apply";
            btnRoughen.UseVisualStyleBackColor = true;
            btnRoughen.Click += BtnRoughen_Click;
            // 
            // lblClamp
            // 
            lblClamp.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblClamp.AutoSize = true;
            lblClamp.Location = new System.Drawing.Point(9, 312);
            lblClamp.Name = "lblClamp";
            lblClamp.Size = new System.Drawing.Size(125, 15);
            lblClamp.TabIndex = 18;
            lblClamp.Text = "Limit to min / max (z):";
            // 
            // numClampMin
            // 
            numClampMin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numClampMin.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numClampMin.Location = new System.Drawing.Point(209, 308);
            numClampMin.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numClampMin.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numClampMin.Name = "numClampMin";
            numClampMin.Size = new System.Drawing.Size(90, 23);
            numClampMin.TabIndex = 19;
            numClampMin.Value = new decimal(new int[] { 5, 0, 0, int.MinValue });
            // 
            // numClampMax
            // 
            numClampMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numClampMax.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numClampMax.Location = new System.Drawing.Point(305, 308);
            numClampMax.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numClampMax.Minimum = new decimal(new int[] { 120, 0, 0, int.MinValue });
            numClampMax.Name = "numClampMax";
            numClampMax.Size = new System.Drawing.Size(90, 23);
            numClampMax.TabIndex = 20;
            numClampMax.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnClamp
            // 
            btnClamp.Location = new System.Drawing.Point(401, 306);
            btnClamp.Name = "btnClamp";
            btnClamp.Size = new System.Drawing.Size(90, 28);
            btnClamp.TabIndex = 21;
            btnClamp.Text = "Apply";
            btnClamp.UseVisualStyleBackColor = true;
            btnClamp.Click += BtnClamp_Click;
            // 
            // lblProfile
            // 
            lblProfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProfile.AutoSize = true;
            lblProfile.Location = new System.Drawing.Point(9, 346);
            lblProfile.Name = "lblProfile";
            lblProfile.Size = new System.Drawing.Size(180, 15);
            lblProfile.TabIndex = 22;
            lblProfile.Text = "Mountain profile (slope / max z):";
            // 
            // numProfileSlope
            // 
            numProfileSlope.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numProfileSlope.Location = new System.Drawing.Point(209, 342);
            numProfileSlope.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numProfileSlope.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numProfileSlope.Name = "numProfileSlope";
            numProfileSlope.Size = new System.Drawing.Size(90, 23);
            numProfileSlope.TabIndex = 23;
            numProfileSlope.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // numProfileMax
            // 
            numProfileMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numProfileMax.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            numProfileMax.Location = new System.Drawing.Point(305, 342);
            numProfileMax.Maximum = new decimal(new int[] { 115, 0, 0, 0 });
            numProfileMax.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numProfileMax.Name = "numProfileMax";
            numProfileMax.Size = new System.Drawing.Size(90, 23);
            numProfileMax.TabIndex = 24;
            numProfileMax.Value = new decimal(new int[] { 70, 0, 0, 0 });
            // 
            // btnProfile
            // 
            btnProfile.Location = new System.Drawing.Point(401, 340);
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new System.Drawing.Size(90, 28);
            btnProfile.TabIndex = 25;
            btnProfile.Text = "Apply";
            btnProfile.UseVisualStyleBackColor = true;
            btnProfile.Click += BtnProfile_Click;
            // 
            // lblProfileTerrain
            // 
            lblProfileTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblProfileTerrain.AutoSize = true;
            lblProfileTerrain.Location = new System.Drawing.Point(9, 378);
            lblProfileTerrain.Name = "lblProfileTerrain";
            lblProfileTerrain.Size = new System.Drawing.Size(99, 15);
            lblProfileTerrain.TabIndex = 26;
            lblProfileTerrain.Text = "Mountain terrain:";
            // 
            // cbProfileTerrain
            // 
            cbProfileTerrain.Anchor = System.Windows.Forms.AnchorStyles.Left;
            tableHeights.SetColumnSpan(cbProfileTerrain, 3);
            cbProfileTerrain.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cbProfileTerrain.DropDownHeight = 400;
            cbProfileTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbProfileTerrain.IntegralHeight = false;
            cbProfileTerrain.Location = new System.Drawing.Point(209, 374);
            cbProfileTerrain.Name = "cbProfileTerrain";
            cbProfileTerrain.Size = new System.Drawing.Size(300, 24);
            cbProfileTerrain.TabIndex = 27;
            // 
            // lblResetAltitude
            // 
            lblResetAltitude.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblResetAltitude.AutoSize = true;
            lblResetAltitude.Location = new System.Drawing.Point(9, 409);
            lblResetAltitude.Name = "lblResetAltitude";
            lblResetAltitude.Size = new System.Drawing.Size(163, 15);
            lblResetAltitude.TabIndex = 28;
            lblResetAltitude.Text = "Altitude from terrain defaults:";
            // 
            // btnResetAltitude
            // 
            btnResetAltitude.Location = new System.Drawing.Point(401, 403);
            btnResetAltitude.Name = "btnResetAltitude";
            btnResetAltitude.Size = new System.Drawing.Size(90, 28);
            btnResetAltitude.TabIndex = 29;
            btnResetAltitude.Text = "Apply";
            btnResetAltitude.UseVisualStyleBackColor = true;
            btnResetAltitude.Click += BtnResetAltitude_Click;
            // 
            // btnHeightUndo
            // 
            btnHeightUndo.Enabled = false;
            btnHeightUndo.Location = new System.Drawing.Point(9, 446);
            btnHeightUndo.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            btnHeightUndo.Name = "btnHeightUndo";
            btnHeightUndo.Size = new System.Drawing.Size(190, 28);
            btnHeightUndo.TabIndex = 30;
            btnHeightUndo.Text = "Undo last height change";
            btnHeightUndo.UseVisualStyleBackColor = true;
            btnHeightUndo.Click += BtnHeightUndo_Click;
            // 
            // lblHeightStats
            // 
            lblHeightStats.AutoSize = true;
            tableHeights.SetColumnSpan(lblHeightStats, 5);
            lblHeightStats.Location = new System.Drawing.Point(9, 489);
            lblHeightStats.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            lblHeightStats.Name = "lblHeightStats";
            lblHeightStats.Size = new System.Drawing.Size(0, 15);
            lblHeightStats.TabIndex = 31;
            // 
            // tabBuild
            // 
            tabBuild.Controls.Add(tableBuild);
            tabBuild.Location = new System.Drawing.Point(4, 28);
            tabBuild.Name = "tabBuild";
            tabBuild.Padding = new System.Windows.Forms.Padding(8);
            tabBuild.Size = new System.Drawing.Size(192, 68);
            tabBuild.TabIndex = 4;
            tabBuild.Text = "5. Build MUL";
            tabBuild.UseVisualStyleBackColor = true;
            // 
            // tableBuild
            // 
            tableBuild.AutoSize = true;
            tableBuild.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableBuild.ColumnCount = 3;
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 260F));
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableBuild.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableBuild.Controls.Add(lblBuildInfo, 0, 0);
            tableBuild.Controls.Add(lblOutputFolder, 0, 1);
            tableBuild.Controls.Add(txtOutputFolder, 1, 1);
            tableBuild.Controls.Add(btnOutputBrowse, 2, 1);
            tableBuild.Controls.Add(chkRandomStatics, 0, 2);
            tableBuild.Controls.Add(chkImportStatics, 0, 3);
            tableBuild.Controls.Add(txtImportFolder, 1, 3);
            tableBuild.Controls.Add(btnImportBrowse, 2, 3);
            tableBuild.Controls.Add(lblCompileSeed, 0, 4);
            tableBuild.Controls.Add(numCompileSeed, 1, 4);
            tableBuild.Controls.Add(chkRepairBeforeBuild, 0, 5);
            tableBuild.Controls.Add(flowBuildButtons, 0, 6);
            tableBuild.Controls.Add(lblBuildHelp, 0, 7);
            tableBuild.Dock = System.Windows.Forms.DockStyle.Top;
            tableBuild.Location = new System.Drawing.Point(8, 8);
            tableBuild.Name = "tableBuild";
            tableBuild.Padding = new System.Windows.Forms.Padding(6);
            tableBuild.RowCount = 10;
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
            tableBuild.Size = new System.Drawing.Size(176, 541);
            tableBuild.TabIndex = 0;
            // 
            // lblBuildInfo
            // 
            lblBuildInfo.AutoSize = true;
            tableBuild.SetColumnSpan(lblBuildInfo, 3);
            lblBuildInfo.Location = new System.Drawing.Point(9, 9);
            lblBuildInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 10);
            lblBuildInfo.MaximumSize = new System.Drawing.Size(700, 0);
            lblBuildInfo.Name = "lblBuildInfo";
            lblBuildInfo.Size = new System.Drawing.Size(144, 120);
            lblBuildInfo.TabIndex = 0;
            lblBuildInfo.Text = "Builds Map#.mul, StaIdx#.mul and Statics#.mul from the current map (# = \"Map # (mul)\" in the project bar). Transitions from Data/Transitions, random statics from Data/Statics.";
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(9, 148);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(82, 15);
            lblOutputFolder.TabIndex = 1;
            lblOutputFolder.Text = "Output folder:";
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtOutputFolder.Location = new System.Drawing.Point(269, 144);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.PlaceholderText = "(project folder)";
            txtOutputFolder.Size = new System.Drawing.Size(1, 23);
            txtOutputFolder.TabIndex = 2;
            // 
            // btnOutputBrowse
            // 
            btnOutputBrowse.Location = new System.Drawing.Point(77, 142);
            btnOutputBrowse.Name = "btnOutputBrowse";
            btnOutputBrowse.Size = new System.Drawing.Size(90, 28);
            btnOutputBrowse.TabIndex = 3;
            btnOutputBrowse.Text = "Browse...";
            btnOutputBrowse.UseVisualStyleBackColor = true;
            btnOutputBrowse.Click += BtnOutputBrowse_Click;
            // 
            // chkRandomStatics
            // 
            chkRandomStatics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRandomStatics.AutoSize = true;
            chkRandomStatics.Checked = true;
            chkRandomStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            tableBuild.SetColumnSpan(chkRandomStatics, 3);
            chkRandomStatics.Location = new System.Drawing.Point(9, 176);
            chkRandomStatics.Name = "chkRandomStatics";
            chkRandomStatics.Size = new System.Drawing.Size(158, 19);
            chkRandomStatics.TabIndex = 4;
            chkRandomStatics.Text = "Random statics (trees, rocks, ... from Data/Statics)";
            // 
            // chkImportStatics
            // 
            chkImportStatics.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkImportStatics.AutoSize = true;
            chkImportStatics.Checked = true;
            chkImportStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            chkImportStatics.Location = new System.Drawing.Point(9, 205);
            chkImportStatics.Name = "chkImportStatics";
            chkImportStatics.Size = new System.Drawing.Size(210, 19);
            chkImportStatics.TabIndex = 5;
            chkImportStatics.Text = "Import static XML files from folder:";
            // 
            // txtImportFolder
            // 
            txtImportFolder.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtImportFolder.Location = new System.Drawing.Point(269, 203);
            txtImportFolder.Name = "txtImportFolder";
            txtImportFolder.PlaceholderText = "(project folder)\\Import Files";
            txtImportFolder.Size = new System.Drawing.Size(1, 23);
            txtImportFolder.TabIndex = 6;
            // 
            // btnImportBrowse
            // 
            btnImportBrowse.Location = new System.Drawing.Point(77, 201);
            btnImportBrowse.Name = "btnImportBrowse";
            btnImportBrowse.Size = new System.Drawing.Size(90, 28);
            btnImportBrowse.TabIndex = 7;
            btnImportBrowse.Text = "Browse...";
            btnImportBrowse.UseVisualStyleBackColor = true;
            btnImportBrowse.Click += BtnImportBrowse_Click;
            // 
            // lblCompileSeed
            // 
            lblCompileSeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCompileSeed.AutoSize = true;
            lblCompileSeed.Location = new System.Drawing.Point(9, 239);
            lblCompileSeed.Name = "lblCompileSeed";
            lblCompileSeed.Size = new System.Drawing.Size(155, 15);
            lblCompileSeed.TabIndex = 8;
            lblCompileSeed.Text = "Random seed (0 = random):";
            // 
            // numCompileSeed
            // 
            numCompileSeed.Anchor = System.Windows.Forms.AnchorStyles.Left;
            numCompileSeed.Location = new System.Drawing.Point(269, 235);
            numCompileSeed.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
            numCompileSeed.Name = "numCompileSeed";
            numCompileSeed.Size = new System.Drawing.Size(1, 23);
            numCompileSeed.TabIndex = 9;
            // 
            // chkRepairBeforeBuild
            // 
            chkRepairBeforeBuild.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkRepairBeforeBuild.AutoSize = true;
            tableBuild.SetColumnSpan(chkRepairBeforeBuild, 3);
            chkRepairBeforeBuild.Location = new System.Drawing.Point(9, 264);
            chkRepairBeforeBuild.Name = "chkRepairBeforeBuild";
            chkRepairBeforeBuild.Size = new System.Drawing.Size(158, 19);
            chkRepairBeforeBuild.TabIndex = 10;
            chkRepairBeforeBuild.Text = "Repair transitions before building";
            // 
            // flowBuildButtons
            // 
            flowBuildButtons.AutoSize = true;
            flowBuildButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableBuild.SetColumnSpan(flowBuildButtons, 3);
            flowBuildButtons.Controls.Add(btnCheckTransitions);
            flowBuildButtons.Controls.Add(btnRepairTransitions);
            flowBuildButtons.Controls.Add(btnBuildMul);
            flowBuildButtons.Location = new System.Drawing.Point(6, 298);
            flowBuildButtons.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            flowBuildButtons.Name = "flowBuildButtons";
            flowBuildButtons.Size = new System.Drawing.Size(164, 102);
            flowBuildButtons.TabIndex = 11;
            // 
            // btnCheckTransitions
            // 
            btnCheckTransitions.Location = new System.Drawing.Point(3, 3);
            btnCheckTransitions.Name = "btnCheckTransitions";
            btnCheckTransitions.Size = new System.Drawing.Size(150, 28);
            btnCheckTransitions.TabIndex = 0;
            btnCheckTransitions.Text = "Check transitions";
            btnCheckTransitions.UseVisualStyleBackColor = true;
            btnCheckTransitions.Click += BtnCheckTransitions_Click;
            // 
            // btnRepairTransitions
            // 
            btnRepairTransitions.Location = new System.Drawing.Point(3, 37);
            btnRepairTransitions.Name = "btnRepairTransitions";
            btnRepairTransitions.Size = new System.Drawing.Size(150, 28);
            btnRepairTransitions.TabIndex = 1;
            btnRepairTransitions.Text = "Repair transitions";
            btnRepairTransitions.UseVisualStyleBackColor = true;
            btnRepairTransitions.Click += BtnRepairTransitions_Click;
            // 
            // btnBuildMul
            // 
            btnBuildMul.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnBuildMul.Location = new System.Drawing.Point(3, 71);
            btnBuildMul.Name = "btnBuildMul";
            btnBuildMul.Size = new System.Drawing.Size(170, 28);
            btnBuildMul.TabIndex = 2;
            btnBuildMul.Text = "Build MUL files";
            btnBuildMul.UseVisualStyleBackColor = true;
            btnBuildMul.Click += BtnBuildMul_Click;
            // 
            // lblBuildHelp
            // 
            lblBuildHelp.AutoSize = true;
            tableBuild.SetColumnSpan(lblBuildHelp, 3);
            lblBuildHelp.ForeColor = System.Drawing.SystemColors.GrayText;
            lblBuildHelp.Location = new System.Drawing.Point(9, 412);
            lblBuildHelp.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            lblBuildHelp.MaximumSize = new System.Drawing.Size(700, 0);
            lblBuildHelp.Name = "lblBuildHelp";
            lblBuildHelp.Size = new System.Drawing.Size(158, 120);
            lblBuildHelp.TabIndex = 12;
            lblBuildHelp.Text = resources.GetString("lblBuildHelp.Text");
            // 
            // tabPageDataViewer
            // 
            tabPageDataViewer.Controls.Add(tableDataViewer);
            tabPageDataViewer.Controls.Add(MainMenu1);
            tabPageDataViewer.Location = new System.Drawing.Point(4, 28);
            tabPageDataViewer.Name = "tabPageDataViewer";
            tabPageDataViewer.Padding = new System.Windows.Forms.Padding(6);
            tabPageDataViewer.Size = new System.Drawing.Size(192, 68);
            tabPageDataViewer.TabIndex = 5;
            tabPageDataViewer.Text = "6. Data viewer";
            tabPageDataViewer.UseVisualStyleBackColor = true;
            // 
            // tableDataViewer
            // 
            tableDataViewer.ColumnCount = 2;
            tableDataViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableDataViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableDataViewer.Controls.Add(lbTerrainList, 0, 0);
            tableDataViewer.Controls.Add(lbTileProperties, 1, 0);
            tableDataViewer.Controls.Add(ListBox1, 0, 1);
            tableDataViewer.Controls.Add(PropertyGrid1, 1, 1);
            tableDataViewer.Controls.Add(PictureBox1, 1, 2);
            tableDataViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            tableDataViewer.Location = new System.Drawing.Point(6, 30);
            tableDataViewer.Name = "tableDataViewer";
            tableDataViewer.RowCount = 3;
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableDataViewer.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableDataViewer.Size = new System.Drawing.Size(180, 32);
            tableDataViewer.TabIndex = 0;
            // 
            // lbTerrainList
            // 
            lbTerrainList.AutoSize = true;
            lbTerrainList.Location = new System.Drawing.Point(3, 0);
            lbTerrainList.Name = "lbTerrainList";
            lbTerrainList.Size = new System.Drawing.Size(63, 15);
            lbTerrainList.TabIndex = 0;
            lbTerrainList.Text = "Terrain List";
            // 
            // lbTileProperties
            // 
            lbTileProperties.AutoSize = true;
            lbTileProperties.Location = new System.Drawing.Point(84, 0);
            lbTileProperties.Name = "lbTileProperties";
            lbTileProperties.Size = new System.Drawing.Size(81, 15);
            lbTileProperties.TabIndex = 1;
            lbTileProperties.Text = "Tile Properties";
            // 
            // ListBox1
            // 
            ListBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            ListBox1.IntegralHeight = false;
            ListBox1.ItemHeight = 15;
            ListBox1.Location = new System.Drawing.Point(3, 18);
            ListBox1.Name = "ListBox1";
            tableDataViewer.SetRowSpan(ListBox1, 2);
            ListBox1.Size = new System.Drawing.Size(75, 11);
            ListBox1.TabIndex = 2;
            ListBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
            // 
            // PropertyGrid1
            // 
            PropertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            PropertyGrid1.Location = new System.Drawing.Point(84, 18);
            PropertyGrid1.Name = "PropertyGrid1";
            PropertyGrid1.Size = new System.Drawing.Size(93, 1);
            PropertyGrid1.TabIndex = 3;
            PropertyGrid1.ToolbarVisible = false;
            // 
            // PictureBox1
            // 
            PictureBox1.Location = new System.Drawing.Point(84, -91);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new System.Drawing.Size(93, 120);
            PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            PictureBox1.TabIndex = 4;
            PictureBox1.TabStop = false;
            // 
            // MainMenu1
            // 
            MainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { MenuItem1, MenuItem2 });
            MainMenu1.Location = new System.Drawing.Point(6, 6);
            MainMenu1.Name = "MainMenu1";
            MainMenu1.Size = new System.Drawing.Size(180, 24);
            MainMenu1.TabIndex = 1;
            MainMenu1.Text = "MainMenu1";
            // 
            // MenuItem1
            // 
            MenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, exportToolStripMenuItem });
            MenuItem1.Name = "MenuItem1";
            MenuItem1.Size = new System.Drawing.Size(54, 20);
            MenuItem1.Text = "Terrain";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            saveToolStripMenuItem.Text = "Save Terrain.xml";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { terrainACTToolStripMenuItem1, terrainACOToolStripMenuItem1 });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // terrainACTToolStripMenuItem1
            // 
            terrainACTToolStripMenuItem1.Name = "terrainACTToolStripMenuItem1";
            terrainACTToolStripMenuItem1.Size = new System.Drawing.Size(137, 22);
            terrainACTToolStripMenuItem1.Text = "Terrain.ACT";
            terrainACTToolStripMenuItem1.Click += terrainACTToolStripMenuItem1_Click;
            // 
            // terrainACOToolStripMenuItem1
            // 
            terrainACOToolStripMenuItem1.Name = "terrainACOToolStripMenuItem1";
            terrainACOToolStripMenuItem1.Size = new System.Drawing.Size(137, 22);
            terrainACOToolStripMenuItem1.Text = "Terrain.ACO";
            terrainACOToolStripMenuItem1.Click += terrainACOToolStripMenuItem1_Click;
            // 
            // MenuItem2
            // 
            MenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem1, saveToolStripMenuItem1, exportToolStripMenuItem1 });
            MenuItem2.Name = "MenuItem2";
            MenuItem2.Size = new System.Drawing.Size(61, 20);
            MenuItem2.Text = "Altitude";
            // 
            // loadToolStripMenuItem1
            // 
            loadToolStripMenuItem1.Name = "loadToolStripMenuItem1";
            loadToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            loadToolStripMenuItem1.Text = "Load";
            loadToolStripMenuItem1.Click += loadToolStripMenuItem1_Click;
            // 
            // saveToolStripMenuItem1
            // 
            saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            saveToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            saveToolStripMenuItem1.Text = "Save Altitude.xml";
            saveToolStripMenuItem1.Click += saveToolStripMenuItem1_Click;
            // 
            // exportToolStripMenuItem1
            // 
            exportToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { AltitudeACTToolStripMenuItem, AltitudeACOToolStripMenuItem });
            exportToolStripMenuItem1.Name = "exportToolStripMenuItem1";
            exportToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            exportToolStripMenuItem1.Text = "Export";
            // 
            // AltitudeACTToolStripMenuItem
            // 
            AltitudeACTToolStripMenuItem.Name = "AltitudeACTToolStripMenuItem";
            AltitudeACTToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            AltitudeACTToolStripMenuItem.Text = "Altitude.ACT";
            AltitudeACTToolStripMenuItem.Click += AltitudeACTToolStripMenuItem_Click;
            // 
            // AltitudeACOToolStripMenuItem
            // 
            AltitudeACOToolStripMenuItem.Name = "AltitudeACOToolStripMenuItem";
            AltitudeACOToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            AltitudeACOToolStripMenuItem.Text = "Altitude.ACO";
            AltitudeACOToolStripMenuItem.Click += AltitudeACOToolStripMenuItem_Click;
            // 
            // tabHelp
            // 
            tabHelp.Controls.Add(rtbHelp);
            tabHelp.Location = new System.Drawing.Point(4, 28);
            tabHelp.Name = "tabHelp";
            tabHelp.Padding = new System.Windows.Forms.Padding(6);
            tabHelp.Size = new System.Drawing.Size(192, 68);
            tabHelp.TabIndex = 6;
            tabHelp.Text = "7. Help";
            tabHelp.UseVisualStyleBackColor = true;
            // 
            // rtbHelp
            // 
            rtbHelp.BackColor = System.Drawing.SystemColors.Window;
            rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbHelp.Font = new System.Drawing.Font("Segoe UI", 10F);
            rtbHelp.Location = new System.Drawing.Point(6, 6);
            rtbHelp.Name = "rtbHelp";
            rtbHelp.ReadOnly = true;
            rtbHelp.Size = new System.Drawing.Size(180, 56);
            rtbHelp.TabIndex = 0;
            rtbHelp.Text = "";
            // 
            // groupPreview
            // 
            groupPreview.Controls.Add(picPreview);
            groupPreview.Controls.Add(tablePreviewBottom);
            groupPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            groupPreview.Location = new System.Drawing.Point(0, 0);
            groupPreview.Name = "groupPreview";
            groupPreview.Padding = new System.Windows.Forms.Padding(6);
            groupPreview.Size = new System.Drawing.Size(436, 539);
            groupPreview.TabIndex = 0;
            groupPreview.TabStop = false;
            groupPreview.Text = "Current map";
            // 
            // picPreview
            // 
            picPreview.BackColor = System.Drawing.Color.FromArgb(32, 32, 36);
            picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picPreview.Location = new System.Drawing.Point(6, 22);
            picPreview.Name = "picPreview";
            picPreview.Size = new System.Drawing.Size(424, 388);
            picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            picPreview.DoubleClick += BtnEditMap_Click;
            // 
            // tablePreviewBottom
            // 
            tablePreviewBottom.AutoSize = true;
            tablePreviewBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tablePreviewBottom.ColumnCount = 2;
            tablePreviewBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tablePreviewBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tablePreviewBottom.Controls.Add(flowPreviewMode, 0, 0);
            tablePreviewBottom.Controls.Add(lblMapInfo, 0, 1);
            tablePreviewBottom.Controls.Add(btnEditMap, 0, 2);
            tablePreviewBottom.Controls.Add(btnSaveMap, 1, 2);
            tablePreviewBottom.Controls.Add(btnLoadMap, 0, 3);
            tablePreviewBottom.Controls.Add(btnSaveMapAs, 1, 3);
            tablePreviewBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            tablePreviewBottom.Location = new System.Drawing.Point(6, 410);
            tablePreviewBottom.Name = "tablePreviewBottom";
            tablePreviewBottom.RowCount = 4;
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tablePreviewBottom.Size = new System.Drawing.Size(424, 123);
            tablePreviewBottom.TabIndex = 1;
            // 
            // flowPreviewMode
            // 
            flowPreviewMode.AutoSize = true;
            flowPreviewMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tablePreviewBottom.SetColumnSpan(flowPreviewMode, 2);
            flowPreviewMode.Controls.Add(rbPrevTerrain);
            flowPreviewMode.Controls.Add(rbPrevAltitude);
            flowPreviewMode.Controls.Add(rbPrevRelief);
            flowPreviewMode.Location = new System.Drawing.Point(3, 3);
            flowPreviewMode.Name = "flowPreviewMode";
            flowPreviewMode.Size = new System.Drawing.Size(199, 25);
            flowPreviewMode.TabIndex = 0;
            // 
            // rbPrevTerrain
            // 
            rbPrevTerrain.AutoSize = true;
            rbPrevTerrain.Checked = true;
            rbPrevTerrain.Location = new System.Drawing.Point(3, 3);
            rbPrevTerrain.Name = "rbPrevTerrain";
            rbPrevTerrain.Size = new System.Drawing.Size(60, 19);
            rbPrevTerrain.TabIndex = 0;
            rbPrevTerrain.TabStop = true;
            rbPrevTerrain.Text = "Terrain";
            rbPrevTerrain.CheckedChanged += PreviewMode_CheckedChanged;
            // 
            // rbPrevAltitude
            // 
            rbPrevAltitude.AutoSize = true;
            rbPrevAltitude.Location = new System.Drawing.Point(69, 3);
            rbPrevAltitude.Name = "rbPrevAltitude";
            rbPrevAltitude.Size = new System.Drawing.Size(67, 19);
            rbPrevAltitude.TabIndex = 1;
            rbPrevAltitude.Text = "Altitude";
            rbPrevAltitude.CheckedChanged += PreviewMode_CheckedChanged;
            // 
            // rbPrevRelief
            // 
            rbPrevRelief.AutoSize = true;
            rbPrevRelief.Location = new System.Drawing.Point(142, 3);
            rbPrevRelief.Name = "rbPrevRelief";
            rbPrevRelief.Size = new System.Drawing.Size(54, 19);
            rbPrevRelief.TabIndex = 2;
            rbPrevRelief.Text = "Relief";
            rbPrevRelief.CheckedChanged += PreviewMode_CheckedChanged;
            // 
            // lblMapInfo
            // 
            lblMapInfo.AutoSize = true;
            tablePreviewBottom.SetColumnSpan(lblMapInfo, 2);
            lblMapInfo.Location = new System.Drawing.Point(3, 34);
            lblMapInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            lblMapInfo.MaximumSize = new System.Drawing.Size(380, 0);
            lblMapInfo.Name = "lblMapInfo";
            lblMapInfo.Size = new System.Drawing.Size(92, 15);
            lblMapInfo.TabIndex = 1;
            lblMapInfo.Text = "No map loaded.";
            // 
            // btnEditMap
            // 
            btnEditMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnEditMap.Location = new System.Drawing.Point(3, 58);
            btnEditMap.Name = "btnEditMap";
            btnEditMap.Size = new System.Drawing.Size(206, 28);
            btnEditMap.TabIndex = 2;
            btnEditMap.Text = "Open in editor";
            btnEditMap.UseVisualStyleBackColor = true;
            btnEditMap.Click += BtnEditMap_Click;
            // 
            // btnSaveMap
            // 
            btnSaveMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnSaveMap.Location = new System.Drawing.Point(215, 58);
            btnSaveMap.Name = "btnSaveMap";
            btnSaveMap.Size = new System.Drawing.Size(206, 28);
            btnSaveMap.TabIndex = 3;
            btnSaveMap.Text = "Save to project";
            btnSaveMap.UseVisualStyleBackColor = true;
            btnSaveMap.Click += BtnSaveMap_Click;
            // 
            // btnLoadMap
            // 
            btnLoadMap.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnLoadMap.Location = new System.Drawing.Point(3, 92);
            btnLoadMap.Name = "btnLoadMap";
            btnLoadMap.Size = new System.Drawing.Size(206, 28);
            btnLoadMap.TabIndex = 4;
            btnLoadMap.Text = "Load from project";
            btnLoadMap.UseVisualStyleBackColor = true;
            btnLoadMap.Click += BtnLoadMap_Click;
            // 
            // btnSaveMapAs
            // 
            btnSaveMapAs.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            btnSaveMapAs.Location = new System.Drawing.Point(215, 92);
            btnSaveMapAs.Name = "btnSaveMapAs";
            btnSaveMapAs.Size = new System.Drawing.Size(206, 28);
            btnSaveMapAs.TabIndex = 5;
            btnSaveMapAs.Text = "Export as PNG...";
            btnSaveMapAs.UseVisualStyleBackColor = true;
            btnSaveMapAs.Click += BtnExportPng_Click;
            // 
            // groupLog
            // 
            groupLog.Controls.Add(txtLog);
            groupLog.Controls.Add(flowLogButtons);
            groupLog.Dock = System.Windows.Forms.DockStyle.Fill;
            groupLog.Location = new System.Drawing.Point(0, 0);
            groupLog.Name = "groupLog";
            groupLog.Padding = new System.Windows.Forms.Padding(6);
            groupLog.Size = new System.Drawing.Size(1260, 196);
            groupLog.TabIndex = 0;
            groupLog.TabStop = false;
            groupLog.Text = "Log";
            // 
            // txtLog
            // 
            txtLog.BackColor = System.Drawing.SystemColors.Window;
            txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            txtLog.Font = new System.Drawing.Font("Consolas", 9F);
            txtLog.Location = new System.Drawing.Point(6, 22);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.Size = new System.Drawing.Size(1132, 168);
            txtLog.TabIndex = 0;
            txtLog.WordWrap = false;
            // 
            // flowLogButtons
            // 
            flowLogButtons.AutoSize = true;
            flowLogButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLogButtons.Controls.Add(btnCancel);
            flowLogButtons.Controls.Add(btnClearLog);
            flowLogButtons.Controls.Add(btnSaveLog);
            flowLogButtons.Dock = System.Windows.Forms.DockStyle.Right;
            flowLogButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLogButtons.Location = new System.Drawing.Point(1138, 22);
            flowLogButtons.Name = "flowLogButtons";
            flowLogButtons.Size = new System.Drawing.Size(116, 168);
            flowLogButtons.TabIndex = 1;
            flowLogButtons.WrapContents = false;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(3, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(110, 28);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new System.Drawing.Point(3, 37);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new System.Drawing.Size(110, 28);
            btnClearLog.TabIndex = 1;
            btnClearLog.Text = "Clear log";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += BtnClearLog_Click;
            // 
            // btnSaveLog
            // 
            btnSaveLog.Location = new System.Drawing.Point(3, 71);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new System.Drawing.Size(110, 28);
            btnSaveLog.TabIndex = 2;
            btnSaveLog.Text = "Save log...";
            btnSaveLog.UseVisualStyleBackColor = true;
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
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            groupProject.ResumeLayout(false);
            groupProject.PerformLayout();
            tableProject.ResumeLayout(false);
            tableProject.PerformLayout();
            flowSize.ResumeLayout(false);
            flowSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMapNumber).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            splitWork.Panel1.ResumeLayout(false);
            splitWork.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitWork).EndInit();
            splitWork.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabBlank.ResumeLayout(false);
            tabBlank.PerformLayout();
            tableBlank.ResumeLayout(false);
            tableBlank.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numBlankAltitude).EndInit();
            tabGenerator.ResumeLayout(false);
            splitGenerator.Panel1.ResumeLayout(false);
            splitGenerator.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitGenerator).EndInit();
            splitGenerator.ResumeLayout(false);
            tableGenRight.ResumeLayout(false);
            tableGenRight.PerformLayout();
            groupBiomes.ResumeLayout(false);
            groupBiomes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridBiomes).EndInit();
            flowBiomeButtons.ResumeLayout(false);
            groupSketch.ResumeLayout(false);
            groupSketch.PerformLayout();
            tableSketch.ResumeLayout(false);
            tableSketch.PerformLayout();
            groupPresets.ResumeLayout(false);
            groupPresets.PerformLayout();
            flowPresets.ResumeLayout(false);
            groupGenerate.ResumeLayout(false);
            groupGenerate.PerformLayout();
            flowGenerate.ResumeLayout(false);
            flowGenerate.PerformLayout();
            tabTemplates.ResumeLayout(false);
            tabTemplates.PerformLayout();
            splitTemplates.Panel1.ResumeLayout(false);
            splitTemplates.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitTemplates).EndInit();
            splitTemplates.ResumeLayout(false);
            tableTemplateRight.ResumeLayout(false);
            tableTemplateRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picTemplate).EndInit();
            flowTemplateUse.ResumeLayout(false);
            flowTemplateUse.PerformLayout();
            tableTemplateTop.ResumeLayout(false);
            tableTemplateTop.PerformLayout();
            tabHeights.ResumeLayout(false);
            tabHeights.PerformLayout();
            tableHeights.ResumeLayout(false);
            tableHeights.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSmooth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoughAmp).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoughSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numClampMin).EndInit();
            ((System.ComponentModel.ISupportInitialize)numClampMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)numProfileSlope).EndInit();
            ((System.ComponentModel.ISupportInitialize)numProfileMax).EndInit();
            tabBuild.ResumeLayout(false);
            tabBuild.PerformLayout();
            tableBuild.ResumeLayout(false);
            tableBuild.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCompileSeed).EndInit();
            flowBuildButtons.ResumeLayout(false);
            tabPageDataViewer.ResumeLayout(false);
            tabPageDataViewer.PerformLayout();
            tableDataViewer.ResumeLayout(false);
            tableDataViewer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            MainMenu1.ResumeLayout(false);
            MainMenu1.PerformLayout();
            tabHelp.ResumeLayout(false);
            groupPreview.ResumeLayout(false);
            groupPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            tablePreviewBottom.ResumeLayout(false);
            tablePreviewBottom.PerformLayout();
            flowPreviewMode.ResumeLayout(false);
            flowPreviewMode.PerformLayout();
            groupLog.ResumeLayout(false);
            groupLog.PerformLayout();
            flowLogButtons.ResumeLayout(false);
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

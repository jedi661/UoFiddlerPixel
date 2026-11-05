namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    partial class DragonMapStudioForm
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
            menuStrip = new System.Windows.Forms.MenuStrip();
            mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuNewMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuOpenMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuSaveMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuSaveMapAs = new System.Windows.Forms.ToolStripMenuItem();
            mnuFileSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuExportPng = new System.Windows.Forms.ToolStripMenuItem();
            mnuImportMul = new System.Windows.Forms.ToolStripMenuItem();
            mnuFileSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuRecent = new System.Windows.Forms.ToolStripMenuItem();
            mnuFileSep3 = new System.Windows.Forms.ToolStripSeparator();
            mnuClose = new System.Windows.Forms.ToolStripMenuItem();
            mnuEdit = new System.Windows.Forms.ToolStripMenuItem();
            mnuUndo = new System.Windows.Forms.ToolStripMenuItem();
            mnuRedo = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuCut = new System.Windows.Forms.ToolStripMenuItem();
            mnuPaste = new System.Windows.Forms.ToolStripMenuItem();
            mnuDeleteSelection = new System.Windows.Forms.ToolStripMenuItem();
            mnuFillSelection = new System.Windows.Forms.ToolStripMenuItem();
            mnuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            mnuEditSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuCheckMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuFixMap = new System.Windows.Forms.ToolStripMenuItem();
            mnuRules = new System.Windows.Forms.ToolStripMenuItem();
            mnuLoadScripts = new System.Windows.Forms.ToolStripMenuItem();
            mnuLoadJson = new System.Windows.Forms.ToolStripMenuItem();
            mnuSaveJson = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportScripts = new System.Windows.Forms.ToolStripMenuItem();
            mnuRulesSep1 = new System.Windows.Forms.ToolStripSeparator();
            mnuImportPalette = new System.Windows.Forms.ToolStripMenuItem();
            mnuExportPalette = new System.Windows.Forms.ToolStripMenuItem();
            mnuCreateColorTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuRulesSep2 = new System.Windows.Forms.ToolStripSeparator();
            mnuValidate = new System.Windows.Forms.ToolStripMenuItem();
            mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            mnuHelpTopics = new System.Windows.Forms.ToolStripMenuItem();
            mnuOpenSettingsFolder = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            lblPosition = new System.Windows.Forms.ToolStripStatusLabel();
            lblZoom = new System.Windows.Forms.ToolStripStatusLabel();
            progressBar = new System.Windows.Forms.ToolStripProgressBar();
            lblCancel = new System.Windows.Forms.ToolStripStatusLabel();
            tabMain = new System.Windows.Forms.TabControl();
            tabColors = new System.Windows.Forms.TabPage();
            tabEditor = new System.Windows.Forms.TabPage();
            tabCgi = new System.Windows.Forms.TabPage();
            tabGenerator = new System.Windows.Forms.TabPage();
            tabRules = new System.Windows.Forms.TabPage();
            tabBuild = new System.Windows.Forms.TabPage();
            tabSettings = new System.Windows.Forms.TabPage();
            tabHelp = new System.Windows.Forms.TabPage();
            splitColors = new System.Windows.Forms.SplitContainer();
            panelColorTableScroll = new System.Windows.Forms.Panel();
            colorTableView = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.ColorTableView();
            panelColorTableTools = new System.Windows.Forms.FlowLayoutPanel();
            lblCtZoom = new System.Windows.Forms.Label();
            numColorTableZoom = new System.Windows.Forms.NumericUpDown();
            btnLoadColorTable = new System.Windows.Forms.Button();
            btnRenderColorTable = new System.Windows.Forms.Button();
            lblCopyFormat = new System.Windows.Forms.Label();
            cmbCopyFormat = new System.Windows.Forms.ComboBox();
            splitColorRight = new System.Windows.Forms.SplitContainer();
            grpColorInfo = new System.Windows.Forms.GroupBox();
            paletteGrid = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.PaletteGridView();
            panelColorInfo = new System.Windows.Forms.Panel();
            panelColorSwatch = new System.Windows.Forms.Panel();
            lblColorAddress = new System.Windows.Forms.Label();
            lblColorDetails = new System.Windows.Forms.Label();
            picTilePreview = new System.Windows.Forms.PictureBox();
            flowColorButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCopyColor = new System.Windows.Forms.Button();
            btnUseAsPrimary = new System.Windows.Forms.Button();
            btnUseAsSecondary = new System.Windows.Forms.Button();
            btnUseAsLand = new System.Windows.Forms.Button();
            btnUseAsWater = new System.Windows.Forms.Button();
            btnNewColor = new System.Windows.Forms.Button();
            btnEditRgb = new System.Windows.Forms.Button();
            propColorEntry = new System.Windows.Forms.PropertyGrid();
            splitEditor = new System.Windows.Forms.SplitContainer();
            grpPalette = new System.Windows.Forms.GroupBox();
            mapNavigator = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapNavigator();
            grpLibrary = new System.Windows.Forms.GroupBox();
            lstMaps = new System.Windows.Forms.ListView();
            colMapName = new System.Windows.Forms.ColumnHeader();
            colMapSize = new System.Windows.Forms.ColumnHeader();
            colMapDate = new System.Windows.Forms.ColumnHeader();
            flowLibraryButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnLibOpen = new System.Windows.Forms.Button();
            btnLibNew = new System.Windows.Forms.Button();
            btnLibDuplicate = new System.Windows.Forms.Button();
            btnLibRename = new System.Windows.Forms.Button();
            btnLibDelete = new System.Windows.Forms.Button();
            btnLibRefresh = new System.Windows.Forms.Button();
            editorPalette = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.PaletteGridView();
            panelBrushColors = new System.Windows.Forms.Panel();
            panelPrimarySwatch = new System.Windows.Forms.Panel();
            lblPrimary = new System.Windows.Forms.Label();
            panelSecondarySwatch = new System.Windows.Forms.Panel();
            lblSecondary = new System.Windows.Forms.Label();
            mapCanvas = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapCanvas();
            toolStripEditor = new System.Windows.Forms.ToolStrip();
            tsbBrush = new System.Windows.Forms.ToolStripButton();
            tsbEraser = new System.Windows.Forms.ToolStripButton();
            tsbSpray = new System.Windows.Forms.ToolStripButton();
            tsbLine = new System.Windows.Forms.ToolStripButton();
            tsbRect = new System.Windows.Forms.ToolStripButton();
            tsbRectFill = new System.Windows.Forms.ToolStripButton();
            tsbEllipse = new System.Windows.Forms.ToolStripButton();
            tsbEllipseFill = new System.Windows.Forms.ToolStripButton();
            tsbFill = new System.Windows.Forms.ToolStripButton();
            tsbPicker = new System.Windows.Forms.ToolStripButton();
            tsbRaise = new System.Windows.Forms.ToolStripButton();
            tsbLower = new System.Windows.Forms.ToolStripButton();
            tsbSelect = new System.Windows.Forms.ToolStripButton();
            tsbPan = new System.Windows.Forms.ToolStripButton();
            tssEd1 = new System.Windows.Forms.ToolStripSeparator();
            tslBrushSize = new System.Windows.Forms.ToolStripLabel();
            tscBrushSize = new System.Windows.Forms.ToolStripComboBox();
            tsbSquare = new System.Windows.Forms.ToolStripButton();
            tssEd2 = new System.Windows.Forms.ToolStripSeparator();
            tsbZoomIn = new System.Windows.Forms.ToolStripButton();
            tsbZoomOut = new System.Windows.Forms.ToolStripButton();
            tsbZoomFit = new System.Windows.Forms.ToolStripButton();
            tsbZoom100 = new System.Windows.Forms.ToolStripButton();
            tsbGrid = new System.Windows.Forms.ToolStripButton();
            tsbHeights = new System.Windows.Forms.ToolStripButton();
            tssEd3 = new System.Windows.Forms.ToolStripSeparator();
            tsbUndo = new System.Windows.Forms.ToolStripButton();
            tsbRedo = new System.Windows.Forms.ToolStripButton();
            tssEd4 = new System.Windows.Forms.ToolStripSeparator();
            tsbCgiHere = new System.Windows.Forms.ToolStripButton();
            splitCgi = new System.Windows.Forms.SplitContainer();
            isoView = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.IsoMapView();
            tabCgiSide = new System.Windows.Forms.TabControl();
            grpCgiGoto = new System.Windows.Forms.GroupBox();
            cgiNavigator = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapNavigator();
            lblCgiX = new System.Windows.Forms.Label();
            numCgiX = new System.Windows.Forms.NumericUpDown();
            lblCgiY = new System.Windows.Forms.Label();
            numCgiY = new System.Windows.Forms.NumericUpDown();
            btnCgiGoto = new System.Windows.Forms.Button();
            btnCgiToSelection = new System.Windows.Forms.Button();
            tabCgiItems = new System.Windows.Forms.TabPage();
            tabCgiMulti = new System.Windows.Forms.TabPage();
            tabCgiLand = new System.Windows.Forms.TabPage();
            tabCgiSelection = new System.Windows.Forms.TabPage();
            tabCgiProblems = new System.Windows.Forms.TabPage();
            lstCgiItems = new System.Windows.Forms.ListView();
            colCgiItemId = new System.Windows.Forms.ColumnHeader();
            colCgiItemName = new System.Windows.Forms.ColumnHeader();
            txtCgiFilter = new System.Windows.Forms.TextBox();
            panelCgiItem = new System.Windows.Forms.Panel();
            picCgiItem = new System.Windows.Forms.PictureBox();
            lblCgiItemId = new System.Windows.Forms.Label();
            txtCgiItemId = new System.Windows.Forms.TextBox();
            lblCgiHue = new System.Windows.Forms.Label();
            numCgiHue = new System.Windows.Forms.NumericUpDown();
            panelCgiHueSwatch = new System.Windows.Forms.Panel();
            lblCgiZ = new System.Windows.Forms.Label();
            numCgiZOffset = new System.Windows.Forms.NumericUpDown();
            chkCgiStack = new System.Windows.Forms.CheckBox();
            lblCgiItemInfo = new System.Windows.Forms.Label();
            lstCgiLand = new System.Windows.Forms.ListView();
            colCgiLandId = new System.Windows.Forms.ColumnHeader();
            colCgiLandName = new System.Windows.Forms.ColumnHeader();
            txtCgiLandFilter = new System.Windows.Forms.TextBox();
            panelCgiLand = new System.Windows.Forms.Panel();
            picCgiLandArt = new System.Windows.Forms.PictureBox();
            picCgiLandTex = new System.Windows.Forms.PictureBox();
            lblCgiLandId = new System.Windows.Forms.Label();
            txtCgiLandId = new System.Windows.Forms.TextBox();
            lblCgiRadius = new System.Windows.Forms.Label();
            numCgiRadius = new System.Windows.Forms.NumericUpDown();
            lblCgiStep = new System.Windows.Forms.Label();
            numCgiStep = new System.Windows.Forms.NumericUpDown();
            lblCgiTarget = new System.Windows.Forms.Label();
            numCgiTarget = new System.Windows.Forms.NumericUpDown();
            lblCgiLandInfo = new System.Windows.Forms.Label();
            picCgiMulti = new System.Windows.Forms.PictureBox();
            panelCgiMultiTop = new System.Windows.Forms.Panel();
            lblCgiMultiId = new System.Windows.Forms.Label();
            numCgiMultiId = new System.Windows.Forms.NumericUpDown();
            chkCgiMultiInvisible = new System.Windows.Forms.CheckBox();
            lblCgiMultiInfo = new System.Windows.Forms.Label();
            propCgiSelection = new System.Windows.Forms.PropertyGrid();
            lblCgiSelInfo = new System.Windows.Forms.Label();
            lstCgiProblems = new System.Windows.Forms.ListBox();
            panelCgiProblemsTop = new System.Windows.Forms.Panel();
            lblCgiCliff = new System.Windows.Forms.Label();
            numCgiCliff = new System.Windows.Forms.NumericUpDown();
            btnCgiScan = new System.Windows.Forms.Button();
            btnCgiRemoveDup = new System.Windows.Forms.Button();
            btnCgiSaveLog = new System.Windows.Forms.Button();
            toolStripCgiLand = new System.Windows.Forms.ToolStrip();
            tslCgiLandTools = new System.Windows.Forms.ToolStripLabel();
            tsbCgiLandSet = new System.Windows.Forms.ToolStripButton();
            tsbCgiRaise = new System.Windows.Forms.ToolStripButton();
            tsbCgiLower = new System.Windows.Forms.ToolStripButton();
            tsbCgiSetHeight = new System.Windows.Forms.ToolStripButton();
            tsbCgiSmooth = new System.Windows.Forms.ToolStripButton();
            tsbCgiArea = new System.Windows.Forms.ToolStripButton();
            toolStripCgi = new System.Windows.Forms.ToolStrip();
            tsbCgiBuild = new System.Windows.Forms.ToolStripButton();
            tsbCgiLoad = new System.Windows.Forms.ToolStripButton();
            tsbCgiSave = new System.Windows.Forms.ToolStripButton();
            tsbCgiSaveAs = new System.Windows.Forms.ToolStripButton();
            tssCgi1 = new System.Windows.Forms.ToolStripSeparator();
            tsbCgiSelect = new System.Windows.Forms.ToolStripButton();
            tsbCgiPlace = new System.Windows.Forms.ToolStripButton();
            tsbCgiMulti = new System.Windows.Forms.ToolStripButton();
            tsbCgiDelete = new System.Windows.Forms.ToolStripButton();
            tsbCgiHue = new System.Windows.Forms.ToolStripButton();
            tssCgi2 = new System.Windows.Forms.ToolStripSeparator();
            tsbCgiUndo = new System.Windows.Forms.ToolStripButton();
            tsbCgiRedo = new System.Windows.Forms.ToolStripButton();
            tssCgi3 = new System.Windows.Forms.ToolStripSeparator();
            tsbCgiZoomIn = new System.Windows.Forms.ToolStripButton();
            tsbCgiZoomOut = new System.Windows.Forms.ToolStripButton();
            tsbCgiGrid = new System.Windows.Forms.ToolStripButton();
            tsbCgiHeights = new System.Windows.Forms.ToolStripButton();
            tsbCgiStatics = new System.Windows.Forms.ToolStripButton();
            tsbCgiProblems = new System.Windows.Forms.ToolStripButton();
            tslCgiMaxZ = new System.Windows.Forms.ToolStripLabel();
            tscCgiMaxZ = new System.Windows.Forms.ToolStripComboBox();
            splitGenerator = new System.Windows.Forms.SplitContainer();
            tabGenOptions = new System.Windows.Forms.TabControl();
            tabGenSettings = new System.Windows.Forms.TabPage();
            tabGenBiomes = new System.Windows.Forms.TabPage();
            tabOutline = new System.Windows.Forms.TabPage();
            propGenerator = new System.Windows.Forms.PropertyGrid();
            flowGenButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnGenPreview = new System.Windows.Forms.Button();
            btnGenNewSeed = new System.Windows.Forms.Button();
            btnGenToEditor = new System.Windows.Forms.Button();
            btnGenSavePreset = new System.Windows.Forms.Button();
            btnGenLoadPreset = new System.Windows.Forms.Button();
            gridBiomes = new System.Windows.Forms.DataGridView();
            flowBiomeButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnBiomesDefault = new System.Windows.Forms.Button();
            lblBiomeHint = new System.Windows.Forms.Label();
            propOutline = new System.Windows.Forms.PropertyGrid();
            flowOutlineButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnOutlineLoad = new System.Windows.Forms.Button();
            btnOutlinePreview = new System.Windows.Forms.Button();
            btnOutlineApply = new System.Windows.Forms.Button();
            btnOutlineAsMask = new System.Windows.Forms.Button();
            btnMaskFromMap = new System.Windows.Forms.Button();
            btnMaskClear = new System.Windows.Forms.Button();
            picGenPreview = new System.Windows.Forms.PictureBox();
            lblGenInfo = new System.Windows.Forms.Label();
            tabRulesInner = new System.Windows.Forms.TabControl();
            panelRulesTop = new System.Windows.Forms.FlowLayoutPanel();
            lblRuleSetInfo = new System.Windows.Forms.Label();
            btnRulesReload = new System.Windows.Forms.Button();
            btnRulesSaveJson = new System.Windows.Forms.Button();
            btnRulesExport = new System.Windows.Forms.Button();
            btnRulesValidate = new System.Windows.Forms.Button();
            tabRuleGroups = new System.Windows.Forms.TabPage();
            tabRulePalette = new System.Windows.Forms.TabPage();
            tabRuleTransitions = new System.Windows.Forms.TabPage();
            tabRuleItems = new System.Windows.Forms.TabPage();
            tabRuleUneven = new System.Windows.Forms.TabPage();
            tabRuleFixes = new System.Windows.Forms.TabPage();
            tabRuleAliases = new System.Windows.Forms.TabPage();
            tabRuleWarnings = new System.Windows.Forms.TabPage();
            gridGroups = new System.Windows.Forms.DataGridView();
            gridPaletteEntries = new System.Windows.Forms.DataGridView();
            splitTransitions = new System.Windows.Forms.SplitContainer();
            gridTransitions = new System.Windows.Forms.DataGridView();
            gridPatterns = new System.Windows.Forms.DataGridView();
            lblPatternFile = new System.Windows.Forms.Label();
            splitItems = new System.Windows.Forms.SplitContainer();
            gridItemRules = new System.Windows.Forms.DataGridView();
            gridItemGroups = new System.Windows.Forms.DataGridView();
            lblItemFile = new System.Windows.Forms.Label();
            gridUneven = new System.Windows.Forms.DataGridView();
            gridFixes = new System.Windows.Forms.DataGridView();
            gridAliases = new System.Windows.Forms.DataGridView();
            txtRuleWarnings = new System.Windows.Forms.TextBox();
            splitBuild = new System.Windows.Forms.SplitContainer();
            propBuild = new System.Windows.Forms.PropertyGrid();
            flowBuildButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnBuildCurrent = new System.Windows.Forms.Button();
            btnBuildFromFile = new System.Windows.Forms.Button();
            btnOpenOutput = new System.Windows.Forms.Button();
            btnBuildCancel = new System.Windows.Forms.Button();
            btnCheckMap = new System.Windows.Forms.Button();
            btnFixMap = new System.Windows.Forms.Button();
            btnOpenLog = new System.Windows.Forms.Button();
            grpM2B = new System.Windows.Forms.GroupBox();
            lblM2BFile = new System.Windows.Forms.Label();
            txtM2BFile = new System.Windows.Forms.TextBox();
            btnM2BBrowse = new System.Windows.Forms.Button();
            lblM2BSize = new System.Windows.Forms.Label();
            cmbM2BSize = new System.Windows.Forms.ComboBox();
            numM2BWidth = new System.Windows.Forms.NumericUpDown();
            lblM2BX = new System.Windows.Forms.Label();
            numM2BHeight = new System.Windows.Forms.NumericUpDown();
            lblM2BMapNo = new System.Windows.Forms.Label();
            numM2BMapNo = new System.Windows.Forms.NumericUpDown();
            btnM2BConvert = new System.Windows.Forms.Button();
            txtBuildLog = new System.Windows.Forms.TextBox();
            propSettings = new System.Windows.Forms.PropertyGrid();
            flowSettingsButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnSettingsSave = new System.Windows.Forms.Button();
            btnSettingsApply = new System.Windows.Forms.Button();
            btnOpenProjectFolder = new System.Windows.Forms.Button();
            btnOpenOutputFolder = new System.Windows.Forms.Button();
            btnOpenScriptFolder = new System.Windows.Forms.Button();
            txtHelp = new System.Windows.Forms.TextBox();
            panelHelpTop = new System.Windows.Forms.Panel();
            lblHelpLanguage = new System.Windows.Forms.Label();
            cmbHelpLanguage = new System.Windows.Forms.ComboBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            timerThumbnail = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)splitColors).BeginInit();
            splitColors.Panel1.SuspendLayout();
            splitColors.Panel2.SuspendLayout();
            splitColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitColorRight).BeginInit();
            splitColorRight.Panel1.SuspendLayout();
            splitColorRight.Panel2.SuspendLayout();
            splitColorRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitEditor).BeginInit();
            splitEditor.Panel1.SuspendLayout();
            splitEditor.Panel2.SuspendLayout();
            splitEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitCgi).BeginInit();
            splitCgi.Panel1.SuspendLayout();
            splitCgi.Panel2.SuspendLayout();
            splitCgi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitGenerator).BeginInit();
            splitGenerator.Panel1.SuspendLayout();
            splitGenerator.Panel2.SuspendLayout();
            splitGenerator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitTransitions).BeginInit();
            splitTransitions.Panel1.SuspendLayout();
            splitTransitions.Panel2.SuspendLayout();
            splitTransitions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitItems).BeginInit();
            splitItems.Panel1.SuspendLayout();
            splitItems.Panel2.SuspendLayout();
            splitItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitBuild).BeginInit();
            splitBuild.Panel1.SuspendLayout();
            splitBuild.Panel2.SuspendLayout();
            splitBuild.SuspendLayout();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            tabMain.SuspendLayout();
            tabColors.SuspendLayout();
            tabEditor.SuspendLayout();
            tabCgi.SuspendLayout();
            tabGenerator.SuspendLayout();
            tabRules.SuspendLayout();
            tabBuild.SuspendLayout();
            tabSettings.SuspendLayout();
            tabHelp.SuspendLayout();
            panelColorTableScroll.SuspendLayout();
            panelColorTableTools.SuspendLayout();
            grpColorInfo.SuspendLayout();
            panelColorInfo.SuspendLayout();
            panelColorSwatch.SuspendLayout();
            flowColorButtons.SuspendLayout();
            grpPalette.SuspendLayout();
            grpLibrary.SuspendLayout();
            flowLibraryButtons.SuspendLayout();
            panelBrushColors.SuspendLayout();
            panelPrimarySwatch.SuspendLayout();
            panelSecondarySwatch.SuspendLayout();
            toolStripEditor.SuspendLayout();
            tabCgiSide.SuspendLayout();
            grpCgiGoto.SuspendLayout();
            tabCgiItems.SuspendLayout();
            tabCgiMulti.SuspendLayout();
            tabCgiLand.SuspendLayout();
            tabCgiSelection.SuspendLayout();
            tabCgiProblems.SuspendLayout();
            panelCgiItem.SuspendLayout();
            panelCgiHueSwatch.SuspendLayout();
            panelCgiLand.SuspendLayout();
            panelCgiMultiTop.SuspendLayout();
            panelCgiProblemsTop.SuspendLayout();
            toolStripCgiLand.SuspendLayout();
            toolStripCgi.SuspendLayout();
            tabGenOptions.SuspendLayout();
            tabGenSettings.SuspendLayout();
            tabGenBiomes.SuspendLayout();
            tabOutline.SuspendLayout();
            flowGenButtons.SuspendLayout();
            flowBiomeButtons.SuspendLayout();
            flowOutlineButtons.SuspendLayout();
            tabRulesInner.SuspendLayout();
            panelRulesTop.SuspendLayout();
            tabRuleGroups.SuspendLayout();
            tabRulePalette.SuspendLayout();
            tabRuleTransitions.SuspendLayout();
            tabRuleItems.SuspendLayout();
            tabRuleUneven.SuspendLayout();
            tabRuleFixes.SuspendLayout();
            tabRuleAliases.SuspendLayout();
            tabRuleWarnings.SuspendLayout();
            flowBuildButtons.SuspendLayout();
            grpM2B.SuspendLayout();
            flowSettingsButtons.SuspendLayout();
            panelHelpTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numColorTableZoom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picTilePreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCgiItem).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiZOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCgiLandArt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCgiLandTex).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiStep).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiTarget).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCgiMulti).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiMultiId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCgiCliff).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridBiomes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picGenPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridGroups).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPaletteEntries).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridTransitions).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridPatterns).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridItemRules).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridItemGroups).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridUneven).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridFixes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridAliases).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numM2BWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numM2BHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numM2BMapNo).BeginInit();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuFile, mnuEdit, mnuRules, mnuHelp });
            menuStrip.Dock = System.Windows.Forms.DockStyle.Top;
            menuStrip.Name = "menuStrip";
            // 
            // mnuFile
            // 
            mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuNewMap, mnuOpenMap, mnuSaveMap, mnuSaveMapAs, mnuFileSep1, mnuExportPng, mnuImportMul, mnuFileSep2, mnuRecent, mnuFileSep3, mnuClose });
            mnuFile.Text = "&Datei";
            mnuFile.Name = "mnuFile";
            // 
            // mnuNewMap
            // 
            mnuNewMap.Text = "&Neue Karte…";
            mnuNewMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            mnuNewMap.Name = "mnuNewMap";
            mnuNewMap.Click += MnuNewMap_Click;
            // 
            // mnuOpenMap
            // 
            mnuOpenMap.Text = "Karte ö&ffnen…";
            mnuOpenMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            mnuOpenMap.Name = "mnuOpenMap";
            mnuOpenMap.Click += MnuOpenMap_Click;
            // 
            // mnuSaveMap
            // 
            mnuSaveMap.Text = "&Speichern";
            mnuSaveMap.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            mnuSaveMap.Name = "mnuSaveMap";
            mnuSaveMap.Click += MnuSaveMap_Click;
            // 
            // mnuSaveMapAs
            // 
            mnuSaveMapAs.Text = "Speichern &unter…";
            mnuSaveMapAs.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.S;
            mnuSaveMapAs.Name = "mnuSaveMapAs";
            mnuSaveMapAs.Click += MnuSaveMapAs_Click;
            // 
            // mnuFileSep1
            // 
            mnuFileSep1.Name = "mnuFileSep1";
            // 
            // mnuExportPng
            // 
            mnuExportPng.Text = "Als &PNG exportieren…";
            mnuExportPng.Name = "mnuExportPng";
            mnuExportPng.Click += MnuExportPng_Click;
            // 
            // mnuImportMul
            // 
            mnuImportMul.Text = "MUL / UOP einlesen (→ BMP)…";
            mnuImportMul.Name = "mnuImportMul";
            mnuImportMul.Click += MnuImportMul_Click;
            // 
            // mnuFileSep2
            // 
            mnuFileSep2.Name = "mnuFileSep2";
            // 
            // mnuRecent
            // 
            mnuRecent.Text = "&Zuletzt geöffnet";
            mnuRecent.Name = "mnuRecent";
            // 
            // mnuFileSep3
            // 
            mnuFileSep3.Name = "mnuFileSep3";
            // 
            // mnuClose
            // 
            mnuClose.Text = "S&chließen";
            mnuClose.Name = "mnuClose";
            mnuClose.Click += MnuClose_Click;
            // 
            // mnuEdit
            // 
            mnuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuUndo, mnuRedo, mnuEditSep1, mnuCopy, mnuCut, mnuPaste, mnuDeleteSelection, mnuFillSelection, mnuSelectAll, mnuEditSep2, mnuCheckMap, mnuFixMap });
            mnuEdit.Text = "&Bearbeiten";
            mnuEdit.Name = "mnuEdit";
            // 
            // mnuUndo
            // 
            mnuUndo.Text = "&Rückgängig";
            mnuUndo.ShortcutKeyDisplayString = "Strg+Z";
            mnuUndo.Name = "mnuUndo";
            mnuUndo.Click += MnuUndo_Click;
            // 
            // mnuRedo
            // 
            mnuRedo.Text = "&Wiederholen";
            mnuRedo.ShortcutKeyDisplayString = "Strg+Y";
            mnuRedo.Name = "mnuRedo";
            mnuRedo.Click += MnuRedo_Click;
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Name = "mnuEditSep1";
            // 
            // mnuCopy
            // 
            mnuCopy.Text = "&Kopieren";
            mnuCopy.ShortcutKeyDisplayString = "Strg+C";
            mnuCopy.Name = "mnuCopy";
            mnuCopy.Click += MnuCopy_Click;
            // 
            // mnuCut
            // 
            mnuCut.Text = "&Ausschneiden";
            mnuCut.ShortcutKeyDisplayString = "Strg+X";
            mnuCut.Name = "mnuCut";
            mnuCut.Click += MnuCut_Click;
            // 
            // mnuPaste
            // 
            mnuPaste.Text = "&Einfügen (Stempel)";
            mnuPaste.ShortcutKeyDisplayString = "Strg+V";
            mnuPaste.Name = "mnuPaste";
            mnuPaste.Click += MnuPaste_Click;
            // 
            // mnuDeleteSelection
            // 
            mnuDeleteSelection.Text = "Auswahl &löschen (Farbe 2)";
            mnuDeleteSelection.ShortcutKeyDisplayString = "Entf";
            mnuDeleteSelection.Name = "mnuDeleteSelection";
            mnuDeleteSelection.Click += MnuDeleteSelection_Click;
            // 
            // mnuFillSelection
            // 
            mnuFillSelection.Text = "Auswahl mit Farbe 1 &füllen";
            mnuFillSelection.ShortcutKeyDisplayString = "Strg+F";
            mnuFillSelection.Name = "mnuFillSelection";
            mnuFillSelection.Click += MnuFillSelection_Click;
            // 
            // mnuSelectAll
            // 
            mnuSelectAll.Text = "Alles a&uswählen";
            mnuSelectAll.ShortcutKeyDisplayString = "Strg+A";
            mnuSelectAll.Name = "mnuSelectAll";
            mnuSelectAll.Click += MnuSelectAll_Click;
            // 
            // mnuEditSep2
            // 
            mnuEditSep2.Name = "mnuEditSep2";
            // 
            // mnuCheckMap
            // 
            mnuCheckMap.Text = "Karte &prüfen (Log)…";
            mnuCheckMap.Name = "mnuCheckMap";
            mnuCheckMap.Click += BtnCheckMap_Click;
            // 
            // mnuFixMap
            // 
            mnuFixMap.Text = "Karte automatisch &reparieren";
            mnuFixMap.Name = "mnuFixMap";
            mnuFixMap.Click += BtnFixMap_Click;
            // 
            // mnuRules
            // 
            mnuRules.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuLoadScripts, mnuLoadJson, mnuSaveJson, mnuExportScripts, mnuRulesSep1, mnuImportPalette, mnuExportPalette, mnuCreateColorTable, mnuRulesSep2, mnuValidate });
            mnuRules.Text = "&Regeln";
            mnuRules.Name = "mnuRules";
            // 
            // mnuLoadScripts
            // 
            mnuLoadScripts.Text = "Dragon-&Skripte laden…";
            mnuLoadScripts.Name = "mnuLoadScripts";
            mnuLoadScripts.Click += MnuLoadScripts_Click;
            // 
            // mnuLoadJson
            // 
            mnuLoadJson.Text = "Regelsatz (JSON) &laden…";
            mnuLoadJson.Name = "mnuLoadJson";
            mnuLoadJson.Click += MnuLoadJson_Click;
            // 
            // mnuSaveJson
            // 
            mnuSaveJson.Text = "Regelsatz (JSON) &speichern…";
            mnuSaveJson.Name = "mnuSaveJson";
            mnuSaveJson.Click += MnuSaveJson_Click;
            // 
            // mnuExportScripts
            // 
            mnuExportScripts.Text = "Als Dragon-Skripte &exportieren…";
            mnuExportScripts.Name = "mnuExportScripts";
            mnuExportScripts.Click += MnuExportScripts_Click;
            // 
            // mnuRulesSep1
            // 
            mnuRulesSep1.Name = "mnuRulesSep1";
            // 
            // mnuImportPalette
            // 
            mnuImportPalette.Text = "Palette &importieren…";
            mnuImportPalette.Name = "mnuImportPalette";
            mnuImportPalette.Click += MnuImportPalette_Click;
            // 
            // mnuExportPalette
            // 
            mnuExportPalette.Text = "Palette e&xportieren (ACT / PAL / ACO / GPL)…";
            mnuExportPalette.Name = "mnuExportPalette";
            mnuExportPalette.Click += MnuExportPalette_Click;
            // 
            // mnuCreateColorTable
            // 
            mnuCreateColorTable.Text = "Farbtabelle als &Bild speichern…";
            mnuCreateColorTable.Name = "mnuCreateColorTable";
            mnuCreateColorTable.Click += MnuCreateColorTable_Click;
            // 
            // mnuRulesSep2
            // 
            mnuRulesSep2.Name = "mnuRulesSep2";
            // 
            // mnuValidate
            // 
            mnuValidate.Text = "Regelsatz &prüfen";
            mnuValidate.Name = "mnuValidate";
            mnuValidate.Click += MnuValidate_Click;
            // 
            // mnuHelp
            // 
            mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { mnuHelpTopics, mnuOpenSettingsFolder });
            mnuHelp.Text = "&Hilfe";
            mnuHelp.Name = "mnuHelp";
            // 
            // mnuHelpTopics
            // 
            mnuHelpTopics.Text = "&Anleitung";
            mnuHelpTopics.ShortcutKeys = System.Windows.Forms.Keys.F1;
            mnuHelpTopics.Name = "mnuHelpTopics";
            mnuHelpTopics.Click += MnuHelpTopics_Click;
            // 
            // mnuOpenSettingsFolder
            // 
            mnuOpenSettingsFolder.Text = "Einstellungs-Ordner öffnen";
            mnuOpenSettingsFolder.Name = "mnuOpenSettingsFolder";
            mnuOpenSettingsFolder.Click += MnuOpenSettingsFolder_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus, lblPosition, lblZoom, progressBar, lblCancel });
            statusStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            statusStrip.Name = "statusStrip";
            // 
            // lblStatus
            // 
            lblStatus.Spring = true;
            lblStatus.Text = "Bereit";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblStatus.Name = "lblStatus";
            // 
            // lblPosition
            // 
            lblPosition.AutoSize = false;
            lblPosition.Size = new System.Drawing.Size(330, 17);
            lblPosition.Text = "-";
            lblPosition.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblPosition.Name = "lblPosition";
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = false;
            lblZoom.Size = new System.Drawing.Size(90, 17);
            lblZoom.Text = "Zoom";
            lblZoom.Name = "lblZoom";
            // 
            // progressBar
            // 
            progressBar.Size = new System.Drawing.Size(160, 16);
            progressBar.Visible = false;
            progressBar.Name = "progressBar";
            // 
            // lblCancel
            // 
            lblCancel.IsLink = true;
            lblCancel.Text = "Abbrechen";
            lblCancel.Visible = false;
            lblCancel.Name = "lblCancel";
            lblCancel.Click += LblCancel_Click;
            // 
            // tabMain
            // 
            tabMain.Controls.Add(tabColors);
            tabMain.Controls.Add(tabEditor);
            tabMain.Controls.Add(tabCgi);
            tabMain.Controls.Add(tabGenerator);
            tabMain.Controls.Add(tabRules);
            tabMain.Controls.Add(tabBuild);
            tabMain.Controls.Add(tabSettings);
            tabMain.Controls.Add(tabHelp);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Name = "tabMain";
            tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
            // 
            // tabColors
            // 
            tabColors.Controls.Add(splitColors);
            tabColors.Text = "Farbtabelle";
            tabColors.UseVisualStyleBackColor = true;
            tabColors.Padding = new System.Windows.Forms.Padding(3);
            tabColors.Name = "tabColors";
            tabColors.Location = new System.Drawing.Point(4, 24);
            tabColors.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabEditor
            // 
            tabEditor.Controls.Add(splitEditor);
            tabEditor.Controls.Add(toolStripEditor);
            tabEditor.Text = "Karten-Editor";
            tabEditor.UseVisualStyleBackColor = true;
            tabEditor.Padding = new System.Windows.Forms.Padding(3);
            tabEditor.Name = "tabEditor";
            tabEditor.Location = new System.Drawing.Point(4, 24);
            tabEditor.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabCgi
            // 
            tabCgi.Controls.Add(splitCgi);
            tabCgi.Controls.Add(toolStripCgiLand);
            tabCgi.Controls.Add(toolStripCgi);
            tabCgi.Text = "CGI-Ansicht";
            tabCgi.UseVisualStyleBackColor = true;
            tabCgi.Padding = new System.Windows.Forms.Padding(3);
            tabCgi.Name = "tabCgi";
            tabCgi.Location = new System.Drawing.Point(4, 24);
            tabCgi.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabGenerator
            // 
            tabGenerator.Controls.Add(splitGenerator);
            tabGenerator.Text = "Generator & Umriss";
            tabGenerator.UseVisualStyleBackColor = true;
            tabGenerator.Padding = new System.Windows.Forms.Padding(3);
            tabGenerator.Name = "tabGenerator";
            tabGenerator.Location = new System.Drawing.Point(4, 24);
            tabGenerator.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRules
            // 
            tabRules.Controls.Add(tabRulesInner);
            tabRules.Controls.Add(panelRulesTop);
            tabRules.Text = "Regeln & Skripte";
            tabRules.UseVisualStyleBackColor = true;
            tabRules.Padding = new System.Windows.Forms.Padding(3);
            tabRules.Name = "tabRules";
            tabRules.Location = new System.Drawing.Point(4, 24);
            tabRules.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabBuild
            // 
            tabBuild.Controls.Add(splitBuild);
            tabBuild.Text = "MUL / UOP erzeugen";
            tabBuild.UseVisualStyleBackColor = true;
            tabBuild.Padding = new System.Windows.Forms.Padding(3);
            tabBuild.Name = "tabBuild";
            tabBuild.Location = new System.Drawing.Point(4, 24);
            tabBuild.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabSettings
            // 
            tabSettings.Controls.Add(propSettings);
            tabSettings.Controls.Add(flowSettingsButtons);
            tabSettings.Text = "Einstellungen";
            tabSettings.UseVisualStyleBackColor = true;
            tabSettings.Padding = new System.Windows.Forms.Padding(3);
            tabSettings.Name = "tabSettings";
            tabSettings.Location = new System.Drawing.Point(4, 24);
            tabSettings.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabHelp
            // 
            tabHelp.Controls.Add(txtHelp);
            tabHelp.Controls.Add(panelHelpTop);
            tabHelp.Text = "Hilfe";
            tabHelp.UseVisualStyleBackColor = true;
            tabHelp.Padding = new System.Windows.Forms.Padding(3);
            tabHelp.Name = "tabHelp";
            tabHelp.Location = new System.Drawing.Point(4, 24);
            tabHelp.Size = new System.Drawing.Size(1176, 640);
            // 
            // splitColors
            // 
            splitColors.Panel1.Controls.Add(panelColorTableScroll);
            splitColors.Panel1.Controls.Add(panelColorTableTools);
            splitColors.Panel2.Controls.Add(splitColorRight);
            splitColors.Dock = System.Windows.Forms.DockStyle.Fill;
            splitColors.Orientation = System.Windows.Forms.Orientation.Vertical;
            splitColors.Size = new System.Drawing.Size(1170, 634);
            splitColors.SplitterDistance = 640;
            splitColors.Name = "splitColors";
            // 
            // panelColorTableScroll
            // 
            panelColorTableScroll.Controls.Add(colorTableView);
            panelColorTableScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            panelColorTableScroll.AutoScroll = true;
            panelColorTableScroll.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            panelColorTableScroll.Name = "panelColorTableScroll";
            // 
            // colorTableView
            // 
            colorTableView.Location = new System.Drawing.Point(0, 0);
            colorTableView.Size = new System.Drawing.Size(532, 600);
            colorTableView.Zoom = 2;
            colorTableView.Name = "colorTableView";
            colorTableView.IndexClicked += ColorTableView_IndexClicked;
            colorTableView.IndexHovered += ColorTableView_IndexHovered;
            // 
            // panelColorTableTools
            // 
            panelColorTableTools.Controls.Add(lblCtZoom);
            panelColorTableTools.Controls.Add(numColorTableZoom);
            panelColorTableTools.Controls.Add(btnLoadColorTable);
            panelColorTableTools.Controls.Add(btnRenderColorTable);
            panelColorTableTools.Controls.Add(lblCopyFormat);
            panelColorTableTools.Controls.Add(cmbCopyFormat);
            panelColorTableTools.Dock = System.Windows.Forms.DockStyle.Top;
            panelColorTableTools.Size = new System.Drawing.Size(640, 36);
            panelColorTableTools.Padding = new System.Windows.Forms.Padding(2);
            panelColorTableTools.Name = "panelColorTableTools";
            // 
            // lblCtZoom
            // 
            lblCtZoom.AutoSize = true;
            lblCtZoom.Location = new System.Drawing.Point(0, 0);
            lblCtZoom.Text = "Zoom:";
            lblCtZoom.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            lblCtZoom.Name = "lblCtZoom";
            // 
            // numColorTableZoom
            // 
            numColorTableZoom.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            numColorTableZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numColorTableZoom.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numColorTableZoom.Size = new System.Drawing.Size(48, 23);
            numColorTableZoom.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            numColorTableZoom.Name = "numColorTableZoom";
            numColorTableZoom.ValueChanged += NumColorTableZoom_ValueChanged;
            // 
            // btnLoadColorTable
            // 
            btnLoadColorTable.Text = "Farbtabelle laden…";
            btnLoadColorTable.UseVisualStyleBackColor = true;
            btnLoadColorTable.AutoSize = true;
            btnLoadColorTable.Margin = new System.Windows.Forms.Padding(3);
            btnLoadColorTable.Name = "btnLoadColorTable";
            btnLoadColorTable.Click += BtnLoadColorTable_Click;
            // 
            // btnRenderColorTable
            // 
            btnRenderColorTable.Text = "Aus Regeln erzeugen";
            btnRenderColorTable.UseVisualStyleBackColor = true;
            btnRenderColorTable.AutoSize = true;
            btnRenderColorTable.Margin = new System.Windows.Forms.Padding(3);
            btnRenderColorTable.Name = "btnRenderColorTable";
            btnRenderColorTable.Click += BtnRenderColorTable_Click;
            // 
            // lblCopyFormat
            // 
            lblCopyFormat.AutoSize = true;
            lblCopyFormat.Location = new System.Drawing.Point(0, 0);
            lblCopyFormat.Text = "Klick kopiert:";
            lblCopyFormat.Margin = new System.Windows.Forms.Padding(10, 8, 0, 0);
            lblCopyFormat.Name = "lblCopyFormat";
            // 
            // cmbCopyFormat
            // 
            cmbCopyFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbCopyFormat.Size = new System.Drawing.Size(170, 23);
            cmbCopyFormat.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            cmbCopyFormat.Name = "cmbCopyFormat";
            cmbCopyFormat.SelectedIndexChanged += CmbCopyFormat_SelectedIndexChanged;
            // 
            // splitColorRight
            // 
            splitColorRight.Panel1.Controls.Add(grpColorInfo);
            splitColorRight.Panel1.Controls.Add(paletteGrid);
            splitColorRight.Panel2.Controls.Add(propColorEntry);
            splitColorRight.Dock = System.Windows.Forms.DockStyle.Fill;
            splitColorRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitColorRight.Size = new System.Drawing.Size(1170, 634);
            splitColorRight.SplitterDistance = 360;
            splitColorRight.Name = "splitColorRight";
            // 
            // grpColorInfo
            // 
            grpColorInfo.Controls.Add(panelColorInfo);
            grpColorInfo.Controls.Add(flowColorButtons);
            grpColorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            grpColorInfo.Text = "Ausgewählte Farbe";
            grpColorInfo.Name = "grpColorInfo";
            // 
            // paletteGrid
            // 
            paletteGrid.Dock = System.Windows.Forms.DockStyle.Left;
            paletteGrid.Size = new System.Drawing.Size(340, 340);
            paletteGrid.Name = "paletteGrid";
            paletteGrid.IndexClicked += PaletteGrid_IndexClicked;
            // 
            // panelColorInfo
            // 
            panelColorInfo.Controls.Add(panelColorSwatch);
            panelColorInfo.Controls.Add(lblColorAddress);
            panelColorInfo.Controls.Add(lblColorDetails);
            panelColorInfo.Controls.Add(picTilePreview);
            panelColorInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            panelColorInfo.Name = "panelColorInfo";
            // 
            // panelColorSwatch
            // 
            panelColorSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColorSwatch.Location = new System.Drawing.Point(8, 6);
            panelColorSwatch.Size = new System.Drawing.Size(56, 56);
            panelColorSwatch.Name = "panelColorSwatch";
            // 
            // lblColorAddress
            // 
            lblColorAddress.AutoSize = true;
            lblColorAddress.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            lblColorAddress.Location = new System.Drawing.Point(72, 4);
            lblColorAddress.Text = "--";
            lblColorAddress.Name = "lblColorAddress";
            // 
            // lblColorDetails
            // 
            lblColorDetails.Location = new System.Drawing.Point(72, 36);
            lblColorDetails.Size = new System.Drawing.Size(250, 150);
            lblColorDetails.Text = "Klicke auf eine Farbe in der Farbtabelle.";
            lblColorDetails.Name = "lblColorDetails";
            // 
            // picTilePreview
            // 
            picTilePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picTilePreview.Location = new System.Drawing.Point(8, 70);
            picTilePreview.Size = new System.Drawing.Size(56, 56);
            picTilePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picTilePreview.Name = "picTilePreview";
            // 
            // flowColorButtons
            // 
            flowColorButtons.Controls.Add(btnCopyColor);
            flowColorButtons.Controls.Add(btnUseAsPrimary);
            flowColorButtons.Controls.Add(btnUseAsSecondary);
            flowColorButtons.Controls.Add(btnUseAsLand);
            flowColorButtons.Controls.Add(btnUseAsWater);
            flowColorButtons.Controls.Add(btnNewColor);
            flowColorButtons.Controls.Add(btnEditRgb);
            flowColorButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowColorButtons.Size = new System.Drawing.Size(200, 98);
            flowColorButtons.Padding = new System.Windows.Forms.Padding(3);
            flowColorButtons.Name = "flowColorButtons";
            // 
            // btnCopyColor
            // 
            btnCopyColor.Text = "Adresse kopieren";
            btnCopyColor.UseVisualStyleBackColor = true;
            btnCopyColor.AutoSize = true;
            btnCopyColor.Margin = new System.Windows.Forms.Padding(3);
            btnCopyColor.Name = "btnCopyColor";
            btnCopyColor.Click += BtnCopyColor_Click;
            // 
            // btnUseAsPrimary
            // 
            btnUseAsPrimary.Text = "Als Farbe 1";
            btnUseAsPrimary.UseVisualStyleBackColor = true;
            btnUseAsPrimary.AutoSize = true;
            btnUseAsPrimary.Margin = new System.Windows.Forms.Padding(3);
            btnUseAsPrimary.Name = "btnUseAsPrimary";
            btnUseAsPrimary.Click += BtnUseAsPrimary_Click;
            // 
            // btnUseAsSecondary
            // 
            btnUseAsSecondary.Text = "Als Farbe 2";
            btnUseAsSecondary.UseVisualStyleBackColor = true;
            btnUseAsSecondary.AutoSize = true;
            btnUseAsSecondary.Margin = new System.Windows.Forms.Padding(3);
            btnUseAsSecondary.Name = "btnUseAsSecondary";
            btnUseAsSecondary.Click += BtnUseAsSecondary_Click;
            // 
            // btnUseAsLand
            // 
            btnUseAsLand.Text = "Als Umriss-Land";
            btnUseAsLand.UseVisualStyleBackColor = true;
            btnUseAsLand.AutoSize = true;
            btnUseAsLand.Margin = new System.Windows.Forms.Padding(3);
            btnUseAsLand.Name = "btnUseAsLand";
            btnUseAsLand.Click += BtnUseAsLand_Click;
            // 
            // btnUseAsWater
            // 
            btnUseAsWater.Text = "Als Umriss-Wasser";
            btnUseAsWater.UseVisualStyleBackColor = true;
            btnUseAsWater.AutoSize = true;
            btnUseAsWater.Margin = new System.Windows.Forms.Padding(3);
            btnUseAsWater.Name = "btnUseAsWater";
            btnUseAsWater.Click += BtnUseAsWater_Click;
            // 
            // btnNewColor
            // 
            btnNewColor.Text = "Neue Farbe definieren…";
            btnNewColor.UseVisualStyleBackColor = true;
            btnNewColor.AutoSize = true;
            btnNewColor.Margin = new System.Windows.Forms.Padding(3);
            btnNewColor.Name = "btnNewColor";
            btnNewColor.Click += BtnNewColor_Click;
            // 
            // btnEditRgb
            // 
            btnEditRgb.Text = "RGB ändern…";
            btnEditRgb.UseVisualStyleBackColor = true;
            btnEditRgb.AutoSize = true;
            btnEditRgb.Margin = new System.Windows.Forms.Padding(3);
            btnEditRgb.Name = "btnEditRgb";
            btnEditRgb.Click += BtnEditRgb_Click;
            // 
            // propColorEntry
            // 
            propColorEntry.Dock = System.Windows.Forms.DockStyle.Fill;
            propColorEntry.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propColorEntry.Name = "propColorEntry";
            propColorEntry.PropertyValueChanged += PropColorEntry_PropertyValueChanged;
            // 
            // splitEditor
            // 
            splitEditor.Panel1.Controls.Add(grpPalette);
            splitEditor.Panel1.Controls.Add(mapNavigator);
            splitEditor.Panel1.Controls.Add(grpLibrary);
            splitEditor.Panel2.Controls.Add(mapCanvas);
            splitEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            splitEditor.Orientation = System.Windows.Forms.Orientation.Vertical;
            splitEditor.Size = new System.Drawing.Size(1170, 634);
            splitEditor.SplitterDistance = 300;
            splitEditor.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitEditor.Name = "splitEditor";
            // 
            // grpPalette
            // 
            grpPalette.Controls.Add(editorPalette);
            grpPalette.Controls.Add(panelBrushColors);
            grpPalette.Dock = System.Windows.Forms.DockStyle.Fill;
            grpPalette.Text = "Farben (links = 1, rechts = 2)";
            grpPalette.Name = "grpPalette";
            // 
            // mapNavigator
            // 
            mapNavigator.Dock = System.Windows.Forms.DockStyle.Bottom;
            mapNavigator.Size = new System.Drawing.Size(300, 190);
            mapNavigator.Name = "mapNavigator";
            mapNavigator.NavigateTo += MapNavigator_NavigateTo;
            // 
            // grpLibrary
            // 
            grpLibrary.Controls.Add(lstMaps);
            grpLibrary.Controls.Add(flowLibraryButtons);
            grpLibrary.Dock = System.Windows.Forms.DockStyle.Top;
            grpLibrary.Size = new System.Drawing.Size(300, 250);
            grpLibrary.Text = "Karten (Projekt-Ordner)";
            grpLibrary.Name = "grpLibrary";
            // 
            // lstMaps
            // 
            lstMaps.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colMapName, colMapSize, colMapDate });
            lstMaps.Dock = System.Windows.Forms.DockStyle.Fill;
            lstMaps.FullRowSelect = true;
            lstMaps.HideSelection = false;
            lstMaps.MultiSelect = false;
            lstMaps.View = System.Windows.Forms.View.Details;
            lstMaps.Name = "lstMaps";
            lstMaps.DoubleClick += BtnLibOpen_Click;
            // 
            // colMapName
            // 
            colMapName.Text = "Name";
            colMapName.Width = 130;
            // 
            // colMapSize
            // 
            colMapSize.Text = "Größe";
            colMapSize.Width = 80;
            // 
            // colMapDate
            // 
            colMapDate.Text = "Geändert";
            colMapDate.Width = 110;
            // 
            // flowLibraryButtons
            // 
            flowLibraryButtons.Controls.Add(btnLibOpen);
            flowLibraryButtons.Controls.Add(btnLibNew);
            flowLibraryButtons.Controls.Add(btnLibDuplicate);
            flowLibraryButtons.Controls.Add(btnLibRename);
            flowLibraryButtons.Controls.Add(btnLibDelete);
            flowLibraryButtons.Controls.Add(btnLibRefresh);
            flowLibraryButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLibraryButtons.Size = new System.Drawing.Size(200, 64);
            flowLibraryButtons.Padding = new System.Windows.Forms.Padding(3);
            flowLibraryButtons.Name = "flowLibraryButtons";
            // 
            // btnLibOpen
            // 
            btnLibOpen.Text = "Öffnen";
            btnLibOpen.UseVisualStyleBackColor = true;
            btnLibOpen.AutoSize = true;
            btnLibOpen.Margin = new System.Windows.Forms.Padding(3);
            btnLibOpen.Name = "btnLibOpen";
            btnLibOpen.Click += BtnLibOpen_Click;
            // 
            // btnLibNew
            // 
            btnLibNew.Text = "Neu…";
            btnLibNew.UseVisualStyleBackColor = true;
            btnLibNew.AutoSize = true;
            btnLibNew.Margin = new System.Windows.Forms.Padding(3);
            btnLibNew.Name = "btnLibNew";
            btnLibNew.Click += MnuNewMap_Click;
            // 
            // btnLibDuplicate
            // 
            btnLibDuplicate.Text = "Duplizieren";
            btnLibDuplicate.UseVisualStyleBackColor = true;
            btnLibDuplicate.AutoSize = true;
            btnLibDuplicate.Margin = new System.Windows.Forms.Padding(3);
            btnLibDuplicate.Name = "btnLibDuplicate";
            btnLibDuplicate.Click += BtnLibDuplicate_Click;
            // 
            // btnLibRename
            // 
            btnLibRename.Text = "Umbenennen";
            btnLibRename.UseVisualStyleBackColor = true;
            btnLibRename.AutoSize = true;
            btnLibRename.Margin = new System.Windows.Forms.Padding(3);
            btnLibRename.Name = "btnLibRename";
            btnLibRename.Click += BtnLibRename_Click;
            // 
            // btnLibDelete
            // 
            btnLibDelete.Text = "Löschen";
            btnLibDelete.UseVisualStyleBackColor = true;
            btnLibDelete.AutoSize = true;
            btnLibDelete.Margin = new System.Windows.Forms.Padding(3);
            btnLibDelete.Name = "btnLibDelete";
            btnLibDelete.Click += BtnLibDelete_Click;
            // 
            // btnLibRefresh
            // 
            btnLibRefresh.Text = "Aktualisieren";
            btnLibRefresh.UseVisualStyleBackColor = true;
            btnLibRefresh.AutoSize = true;
            btnLibRefresh.Margin = new System.Windows.Forms.Padding(3);
            btnLibRefresh.Name = "btnLibRefresh";
            btnLibRefresh.Click += BtnLibRefresh_Click;
            // 
            // editorPalette
            // 
            editorPalette.Dock = System.Windows.Forms.DockStyle.Fill;
            editorPalette.Name = "editorPalette";
            editorPalette.IndexClicked += EditorPalette_IndexClicked;
            // 
            // panelBrushColors
            // 
            panelBrushColors.Controls.Add(panelPrimarySwatch);
            panelBrushColors.Controls.Add(lblPrimary);
            panelBrushColors.Controls.Add(panelSecondarySwatch);
            panelBrushColors.Controls.Add(lblSecondary);
            panelBrushColors.Dock = System.Windows.Forms.DockStyle.Top;
            panelBrushColors.Size = new System.Drawing.Size(290, 46);
            panelBrushColors.Name = "panelBrushColors";
            // 
            // panelPrimarySwatch
            // 
            panelPrimarySwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelPrimarySwatch.Location = new System.Drawing.Point(4, 4);
            panelPrimarySwatch.Size = new System.Drawing.Size(34, 34);
            panelPrimarySwatch.Name = "panelPrimarySwatch";
            // 
            // lblPrimary
            // 
            lblPrimary.Location = new System.Drawing.Point(42, 2);
            lblPrimary.Size = new System.Drawing.Size(100, 40);
            lblPrimary.Text = "1: -";
            lblPrimary.Name = "lblPrimary";
            // 
            // panelSecondarySwatch
            // 
            panelSecondarySwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelSecondarySwatch.Location = new System.Drawing.Point(146, 4);
            panelSecondarySwatch.Size = new System.Drawing.Size(34, 34);
            panelSecondarySwatch.Name = "panelSecondarySwatch";
            // 
            // lblSecondary
            // 
            lblSecondary.Location = new System.Drawing.Point(184, 2);
            lblSecondary.Size = new System.Drawing.Size(100, 40);
            lblSecondary.Text = "2: -";
            lblSecondary.Name = "lblSecondary";
            // 
            // mapCanvas
            // 
            mapCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            mapCanvas.Name = "mapCanvas";
            mapCanvas.CursorMoved += MapCanvas_CursorMoved;
            mapCanvas.ColorPicked += MapCanvas_ColorPicked;
            mapCanvas.ViewChanged += MapCanvas_ViewChanged;
            mapCanvas.MapEdited += MapCanvas_MapEdited;
            // 
            // toolStripEditor
            // 
            toolStripEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbBrush, tsbEraser, tsbSpray, tsbLine, tsbRect, tsbRectFill, tsbEllipse, tsbEllipseFill, tsbFill, tsbPicker, tsbRaise, tsbLower, tsbSelect, tsbPan, tssEd1, tslBrushSize, tscBrushSize, tsbSquare, tssEd2, tsbZoomIn, tsbZoomOut, tsbZoomFit, tsbZoom100, tsbGrid, tsbHeights, tssEd3, tsbUndo, tsbRedo, tssEd4, tsbCgiHere });
            toolStripEditor.Dock = System.Windows.Forms.DockStyle.Top;
            toolStripEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripEditor.Name = "toolStripEditor";
            // 
            // tsbBrush
            // 
            tsbBrush.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbBrush.Text = "Pinsel";
            tsbBrush.ToolTipText = "Pinsel (B)";
            tsbBrush.Name = "tsbBrush";
            tsbBrush.Click += ToolButton_Click;
            // 
            // tsbEraser
            // 
            tsbEraser.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbEraser.Text = "Radierer";
            tsbEraser.ToolTipText = "Malt mit Farbe 2 (E)";
            tsbEraser.Name = "tsbEraser";
            tsbEraser.Click += ToolButton_Click;
            // 
            // tsbSpray
            // 
            tsbSpray.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbSpray.Text = "Sprühen";
            tsbSpray.ToolTipText = "Zufällige Punkte – gut für Mischungen";
            tsbSpray.Name = "tsbSpray";
            tsbSpray.Click += ToolButton_Click;
            // 
            // tsbLine
            // 
            tsbLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbLine.Text = "Linie";
            tsbLine.ToolTipText = "Linie (L)";
            tsbLine.Name = "tsbLine";
            tsbLine.Click += ToolButton_Click;
            // 
            // tsbRect
            // 
            tsbRect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbRect.Text = "Rechteck";
            tsbRect.Name = "tsbRect";
            tsbRect.Click += ToolButton_Click;
            // 
            // tsbRectFill
            // 
            tsbRectFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbRectFill.Text = "Rechteck ■";
            tsbRectFill.ToolTipText = "Gefülltes Rechteck (R)";
            tsbRectFill.Name = "tsbRectFill";
            tsbRectFill.Click += ToolButton_Click;
            // 
            // tsbEllipse
            // 
            tsbEllipse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbEllipse.Text = "Ellipse";
            tsbEllipse.Name = "tsbEllipse";
            tsbEllipse.Click += ToolButton_Click;
            // 
            // tsbEllipseFill
            // 
            tsbEllipseFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbEllipseFill.Text = "Ellipse ●";
            tsbEllipseFill.ToolTipText = "Gefüllte Ellipse";
            tsbEllipseFill.Name = "tsbEllipseFill";
            tsbEllipseFill.Click += ToolButton_Click;
            // 
            // tsbFill
            // 
            tsbFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbFill.Text = "Füllen";
            tsbFill.ToolTipText = "Fläche füllen (F)";
            tsbFill.Name = "tsbFill";
            tsbFill.Click += ToolButton_Click;
            // 
            // tsbPicker
            // 
            tsbPicker.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbPicker.Text = "Pipette";
            tsbPicker.ToolTipText = "Farbe aufnehmen (I)";
            tsbPicker.Name = "tsbPicker";
            tsbPicker.Click += ToolButton_Click;
            // 
            // tsbRaise
            // 
            tsbRaise.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbRaise.Text = "Höhe +";
            tsbRaise.ToolTipText = "Nächste höhere Farbe der Gruppe (rechte Maustaste: tiefer)";
            tsbRaise.Name = "tsbRaise";
            tsbRaise.Click += ToolButton_Click;
            // 
            // tsbLower
            // 
            tsbLower.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbLower.Text = "Höhe −";
            tsbLower.ToolTipText = "Nächste tiefere Farbe der Gruppe";
            tsbLower.Name = "tsbLower";
            tsbLower.Click += ToolButton_Click;
            // 
            // tsbSelect
            // 
            tsbSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbSelect.Text = "Auswahl";
            tsbSelect.ToolTipText = "Rechteck auswählen – Kopieren / Einfügen / Löschen (S)";
            tsbSelect.Name = "tsbSelect";
            tsbSelect.Click += ToolButton_Click;
            // 
            // tsbPan
            // 
            tsbPan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbPan.Text = "Hand";
            tsbPan.ToolTipText = "Verschieben (auch mittlere Maustaste oder Leertaste)";
            tsbPan.Name = "tsbPan";
            tsbPan.Click += ToolButton_Click;
            // 
            // tssEd1
            // 
            tssEd1.Name = "tssEd1";
            // 
            // tslBrushSize
            // 
            tslBrushSize.Text = "Größe:";
            tslBrushSize.Name = "tslBrushSize";
            // 
            // tscBrushSize
            // 
            tscBrushSize.Size = new System.Drawing.Size(60, 25);
            tscBrushSize.Name = "tscBrushSize";
            tscBrushSize.SelectedIndexChanged += TscBrushSize_Changed;
            tscBrushSize.TextChanged += TscBrushSize_Changed;
            // 
            // tsbSquare
            // 
            tsbSquare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbSquare.Text = "Eckig";
            tsbSquare.ToolTipText = "Eckiger Pinsel";
            tsbSquare.CheckOnClick = true;
            tsbSquare.Name = "tsbSquare";
            tsbSquare.Click += TsbSquare_Click;
            // 
            // tssEd2
            // 
            tssEd2.Name = "tssEd2";
            // 
            // tsbZoomIn
            // 
            tsbZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbZoomIn.Text = "Zoom +";
            tsbZoomIn.Name = "tsbZoomIn";
            tsbZoomIn.Click += TsbZoomIn_Click;
            // 
            // tsbZoomOut
            // 
            tsbZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbZoomOut.Text = "Zoom −";
            tsbZoomOut.Name = "tsbZoomOut";
            tsbZoomOut.Click += TsbZoomOut_Click;
            // 
            // tsbZoomFit
            // 
            tsbZoomFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbZoomFit.Text = "Ganz";
            tsbZoomFit.ToolTipText = "Ganze Karte anzeigen";
            tsbZoomFit.Name = "tsbZoomFit";
            tsbZoomFit.Click += TsbZoomFit_Click;
            // 
            // tsbZoom100
            // 
            tsbZoom100.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbZoom100.Text = "1:1";
            tsbZoom100.Name = "tsbZoom100";
            tsbZoom100.Click += TsbZoom100_Click;
            // 
            // tsbGrid
            // 
            tsbGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbGrid.Text = "Raster";
            tsbGrid.ToolTipText = "8×8-Blockraster (bei starkem Zoom)";
            tsbGrid.CheckOnClick = true;
            tsbGrid.Name = "tsbGrid";
            tsbGrid.Click += TsbGrid_Click;
            // 
            // tsbHeights
            // 
            tsbHeights.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbHeights.Text = "Höhen";
            tsbHeights.ToolTipText = "Höhe (Z) jeder Farbe einblenden (ab Zoom 12)";
            tsbHeights.CheckOnClick = true;
            tsbHeights.Name = "tsbHeights";
            tsbHeights.Click += TsbHeights_Click;
            // 
            // tssEd3
            // 
            tssEd3.Name = "tssEd3";
            // 
            // tsbUndo
            // 
            tsbUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbUndo.Text = "Rückgängig";
            tsbUndo.Name = "tsbUndo";
            tsbUndo.Click += MnuUndo_Click;
            // 
            // tsbRedo
            // 
            tsbRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbRedo.Text = "Wiederholen";
            tsbRedo.Name = "tsbRedo";
            tsbRedo.Click += MnuRedo_Click;
            // 
            // tssEd4
            // 
            tssEd4.Name = "tssEd4";
            // 
            // tsbCgiHere
            // 
            tsbCgiHere.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiHere.Text = "CGI-Ansicht hier";
            tsbCgiHere.ToolTipText = "Diese Stelle im CGI-Modus (Client-Ansicht) anzeigen";
            tsbCgiHere.Name = "tsbCgiHere";
            tsbCgiHere.Click += TsbCgiHere_Click;
            // 
            // splitCgi
            // 
            splitCgi.Panel1.Controls.Add(isoView);
            splitCgi.Panel2.Controls.Add(tabCgiSide);
            splitCgi.Panel2.Controls.Add(grpCgiGoto);
            splitCgi.Panel2.Controls.Add(cgiNavigator);
            splitCgi.Dock = System.Windows.Forms.DockStyle.Fill;
            splitCgi.Orientation = System.Windows.Forms.Orientation.Vertical;
            splitCgi.Size = new System.Drawing.Size(1170, 634);
            splitCgi.SplitterDistance = 860;
            splitCgi.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitCgi.Name = "splitCgi";
            // 
            // isoView
            // 
            isoView.Dock = System.Windows.Forms.DockStyle.Fill;
            isoView.Name = "isoView";
            isoView.HoverChanged += IsoView_HoverChanged;
            isoView.SelectionChanged += IsoView_SelectionChanged;
            isoView.MapChanged += IsoView_MapChanged;
            isoView.ViewChanged += IsoView_ViewChanged;
            isoView.ItemPicked += IsoView_ItemPicked;
            isoView.Message += IsoView_Message;
            // 
            // tabCgiSide
            // 
            tabCgiSide.Controls.Add(tabCgiItems);
            tabCgiSide.Controls.Add(tabCgiMulti);
            tabCgiSide.Controls.Add(tabCgiLand);
            tabCgiSide.Controls.Add(tabCgiSelection);
            tabCgiSide.Controls.Add(tabCgiProblems);
            tabCgiSide.Dock = System.Windows.Forms.DockStyle.Fill;
            tabCgiSide.Name = "tabCgiSide";
            // 
            // grpCgiGoto
            // 
            grpCgiGoto.Controls.Add(lblCgiX);
            grpCgiGoto.Controls.Add(numCgiX);
            grpCgiGoto.Controls.Add(lblCgiY);
            grpCgiGoto.Controls.Add(numCgiY);
            grpCgiGoto.Controls.Add(btnCgiGoto);
            grpCgiGoto.Controls.Add(btnCgiToSelection);
            grpCgiGoto.Dock = System.Windows.Forms.DockStyle.Top;
            grpCgiGoto.Size = new System.Drawing.Size(300, 80);
            grpCgiGoto.Text = "Gehe zu";
            grpCgiGoto.Name = "grpCgiGoto";
            // 
            // cgiNavigator
            // 
            cgiNavigator.Dock = System.Windows.Forms.DockStyle.Top;
            cgiNavigator.Size = new System.Drawing.Size(300, 220);
            cgiNavigator.Name = "cgiNavigator";
            cgiNavigator.NavigateTo += CgiNavigator_NavigateTo;
            // 
            // lblCgiX
            // 
            lblCgiX.AutoSize = true;
            lblCgiX.Location = new System.Drawing.Point(6, 24);
            lblCgiX.Text = "X:";
            lblCgiX.Name = "lblCgiX";
            // 
            // numCgiX
            // 
            numCgiX.Location = new System.Drawing.Point(24, 21);
            numCgiX.Maximum = new decimal(new int[] { 16383, 0, 0, 0 });
            numCgiX.Size = new System.Drawing.Size(64, 23);
            numCgiX.Name = "numCgiX";
            // 
            // lblCgiY
            // 
            lblCgiY.AutoSize = true;
            lblCgiY.Location = new System.Drawing.Point(94, 24);
            lblCgiY.Text = "Y:";
            lblCgiY.Name = "lblCgiY";
            // 
            // numCgiY
            // 
            numCgiY.Location = new System.Drawing.Point(112, 21);
            numCgiY.Maximum = new decimal(new int[] { 16383, 0, 0, 0 });
            numCgiY.Size = new System.Drawing.Size(64, 23);
            numCgiY.Name = "numCgiY";
            // 
            // btnCgiGoto
            // 
            btnCgiGoto.Location = new System.Drawing.Point(182, 20);
            btnCgiGoto.Size = new System.Drawing.Size(60, 25);
            btnCgiGoto.Text = "Gehe";
            btnCgiGoto.UseVisualStyleBackColor = true;
            btnCgiGoto.Name = "btnCgiGoto";
            btnCgiGoto.Click += BtnCgiGoto_Click;
            // 
            // btnCgiToSelection
            // 
            btnCgiToSelection.Location = new System.Drawing.Point(6, 48);
            btnCgiToSelection.Size = new System.Drawing.Size(236, 25);
            btnCgiToSelection.Text = "Zur Auswahl im Karten-Editor";
            btnCgiToSelection.UseVisualStyleBackColor = true;
            btnCgiToSelection.Name = "btnCgiToSelection";
            btnCgiToSelection.Click += BtnCgiToSelection_Click;
            // 
            // tabCgiItems
            // 
            tabCgiItems.Controls.Add(lstCgiItems);
            tabCgiItems.Controls.Add(txtCgiFilter);
            tabCgiItems.Controls.Add(panelCgiItem);
            tabCgiItems.Text = "Items";
            tabCgiItems.UseVisualStyleBackColor = true;
            tabCgiItems.Padding = new System.Windows.Forms.Padding(3);
            tabCgiItems.Name = "tabCgiItems";
            tabCgiItems.Location = new System.Drawing.Point(4, 24);
            tabCgiItems.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabCgiMulti
            // 
            tabCgiMulti.Controls.Add(picCgiMulti);
            tabCgiMulti.Controls.Add(panelCgiMultiTop);
            tabCgiMulti.Text = "Multis";
            tabCgiMulti.UseVisualStyleBackColor = true;
            tabCgiMulti.Padding = new System.Windows.Forms.Padding(3);
            tabCgiMulti.Name = "tabCgiMulti";
            tabCgiMulti.Location = new System.Drawing.Point(4, 24);
            tabCgiMulti.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabCgiLand
            // 
            tabCgiLand.Controls.Add(lstCgiLand);
            tabCgiLand.Controls.Add(txtCgiLandFilter);
            tabCgiLand.Controls.Add(panelCgiLand);
            tabCgiLand.Text = "Land";
            tabCgiLand.UseVisualStyleBackColor = true;
            tabCgiLand.Padding = new System.Windows.Forms.Padding(3);
            tabCgiLand.Name = "tabCgiLand";
            tabCgiLand.Location = new System.Drawing.Point(4, 24);
            tabCgiLand.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabCgiSelection
            // 
            tabCgiSelection.Controls.Add(propCgiSelection);
            tabCgiSelection.Controls.Add(lblCgiSelInfo);
            tabCgiSelection.Text = "Auswahl";
            tabCgiSelection.UseVisualStyleBackColor = true;
            tabCgiSelection.Padding = new System.Windows.Forms.Padding(3);
            tabCgiSelection.Name = "tabCgiSelection";
            tabCgiSelection.Location = new System.Drawing.Point(4, 24);
            tabCgiSelection.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabCgiProblems
            // 
            tabCgiProblems.Controls.Add(lstCgiProblems);
            tabCgiProblems.Controls.Add(panelCgiProblemsTop);
            tabCgiProblems.Text = "Probleme";
            tabCgiProblems.UseVisualStyleBackColor = true;
            tabCgiProblems.Padding = new System.Windows.Forms.Padding(3);
            tabCgiProblems.Name = "tabCgiProblems";
            tabCgiProblems.Location = new System.Drawing.Point(4, 24);
            tabCgiProblems.Size = new System.Drawing.Size(1176, 640);
            // 
            // lstCgiItems
            // 
            lstCgiItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colCgiItemId, colCgiItemName });
            lstCgiItems.Dock = System.Windows.Forms.DockStyle.Fill;
            lstCgiItems.FullRowSelect = true;
            lstCgiItems.HideSelection = false;
            lstCgiItems.MultiSelect = false;
            lstCgiItems.VirtualMode = true;
            lstCgiItems.View = System.Windows.Forms.View.Details;
            lstCgiItems.Name = "lstCgiItems";
            lstCgiItems.RetrieveVirtualItem += LstCgiItems_RetrieveVirtualItem;
            lstCgiItems.SelectedIndexChanged += LstCgiItems_SelectedIndexChanged;
            // 
            // colCgiItemId
            // 
            colCgiItemId.Text = "ID";
            colCgiItemId.Width = 60;
            // 
            // colCgiItemName
            // 
            colCgiItemName.Text = "Name";
            colCgiItemName.Width = 190;
            // 
            // txtCgiFilter
            // 
            txtCgiFilter.Dock = System.Windows.Forms.DockStyle.Top;
            txtCgiFilter.PlaceholderText = "Suche: Name oder ID (hex)";
            txtCgiFilter.Name = "txtCgiFilter";
            txtCgiFilter.TextChanged += TxtCgiFilter_TextChanged;
            // 
            // panelCgiItem
            // 
            panelCgiItem.Controls.Add(picCgiItem);
            panelCgiItem.Controls.Add(lblCgiItemId);
            panelCgiItem.Controls.Add(txtCgiItemId);
            panelCgiItem.Controls.Add(lblCgiHue);
            panelCgiItem.Controls.Add(numCgiHue);
            panelCgiItem.Controls.Add(panelCgiHueSwatch);
            panelCgiItem.Controls.Add(lblCgiZ);
            panelCgiItem.Controls.Add(numCgiZOffset);
            panelCgiItem.Controls.Add(chkCgiStack);
            panelCgiItem.Controls.Add(lblCgiItemInfo);
            panelCgiItem.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelCgiItem.Size = new System.Drawing.Size(290, 170);
            panelCgiItem.Name = "panelCgiItem";
            // 
            // picCgiItem
            // 
            picCgiItem.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picCgiItem.Location = new System.Drawing.Point(4, 4);
            picCgiItem.Size = new System.Drawing.Size(96, 120);
            picCgiItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picCgiItem.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picCgiItem.Name = "picCgiItem";
            // 
            // lblCgiItemId
            // 
            lblCgiItemId.AutoSize = true;
            lblCgiItemId.Location = new System.Drawing.Point(108, 8);
            lblCgiItemId.Text = "Item (hex):";
            lblCgiItemId.Name = "lblCgiItemId";
            // 
            // txtCgiItemId
            // 
            txtCgiItemId.Location = new System.Drawing.Point(184, 4);
            txtCgiItemId.Size = new System.Drawing.Size(70, 23);
            txtCgiItemId.Text = "0e75";
            txtCgiItemId.Name = "txtCgiItemId";
            txtCgiItemId.TextChanged += CgiPlaceSettings_Changed;
            // 
            // lblCgiHue
            // 
            lblCgiHue.AutoSize = true;
            lblCgiHue.Location = new System.Drawing.Point(108, 37);
            lblCgiHue.Text = "Hue:";
            lblCgiHue.Name = "lblCgiHue";
            // 
            // numCgiHue
            // 
            numCgiHue.Location = new System.Drawing.Point(184, 33);
            numCgiHue.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numCgiHue.Size = new System.Drawing.Size(70, 23);
            numCgiHue.Name = "numCgiHue";
            numCgiHue.ValueChanged += CgiPlaceSettings_Changed;
            // 
            // panelCgiHueSwatch
            // 
            panelCgiHueSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelCgiHueSwatch.Location = new System.Drawing.Point(258, 33);
            panelCgiHueSwatch.Size = new System.Drawing.Size(24, 23);
            panelCgiHueSwatch.Name = "panelCgiHueSwatch";
            // 
            // lblCgiZ
            // 
            lblCgiZ.AutoSize = true;
            lblCgiZ.Location = new System.Drawing.Point(108, 66);
            lblCgiZ.Text = "Z + :";
            lblCgiZ.Name = "lblCgiZ";
            // 
            // numCgiZOffset
            // 
            numCgiZOffset.Location = new System.Drawing.Point(184, 62);
            numCgiZOffset.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numCgiZOffset.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numCgiZOffset.Size = new System.Drawing.Size(70, 23);
            numCgiZOffset.Name = "numCgiZOffset";
            numCgiZOffset.ValueChanged += CgiPlaceSettings_Changed;
            // 
            // chkCgiStack
            // 
            chkCgiStack.AutoSize = true;
            chkCgiStack.Location = new System.Drawing.Point(108, 92);
            chkCgiStack.Text = "auf Statics stapeln";
            chkCgiStack.Name = "chkCgiStack";
            chkCgiStack.CheckedChanged += CgiPlaceSettings_Changed;
            // 
            // lblCgiItemInfo
            // 
            lblCgiItemInfo.Location = new System.Drawing.Point(4, 128);
            lblCgiItemInfo.Size = new System.Drawing.Size(280, 38);
            lblCgiItemInfo.Text = "-";
            lblCgiItemInfo.Name = "lblCgiItemInfo";
            // 
            // lstCgiLand
            // 
            lstCgiLand.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colCgiLandId, colCgiLandName });
            lstCgiLand.Dock = System.Windows.Forms.DockStyle.Fill;
            lstCgiLand.FullRowSelect = true;
            lstCgiLand.HideSelection = false;
            lstCgiLand.MultiSelect = false;
            lstCgiLand.VirtualMode = true;
            lstCgiLand.View = System.Windows.Forms.View.Details;
            lstCgiLand.Name = "lstCgiLand";
            lstCgiLand.RetrieveVirtualItem += LstCgiLand_RetrieveVirtualItem;
            lstCgiLand.SelectedIndexChanged += LstCgiLand_SelectedIndexChanged;
            // 
            // colCgiLandId
            // 
            colCgiLandId.Text = "ID";
            colCgiLandId.Width = 60;
            // 
            // colCgiLandName
            // 
            colCgiLandName.Text = "Name";
            colCgiLandName.Width = 190;
            // 
            // txtCgiLandFilter
            // 
            txtCgiLandFilter.Dock = System.Windows.Forms.DockStyle.Top;
            txtCgiLandFilter.PlaceholderText = "Suche: Name oder ID (hex)";
            txtCgiLandFilter.Name = "txtCgiLandFilter";
            txtCgiLandFilter.TextChanged += TxtCgiLandFilter_TextChanged;
            // 
            // panelCgiLand
            // 
            panelCgiLand.Controls.Add(picCgiLandArt);
            panelCgiLand.Controls.Add(picCgiLandTex);
            panelCgiLand.Controls.Add(lblCgiLandId);
            panelCgiLand.Controls.Add(txtCgiLandId);
            panelCgiLand.Controls.Add(lblCgiRadius);
            panelCgiLand.Controls.Add(numCgiRadius);
            panelCgiLand.Controls.Add(lblCgiStep);
            panelCgiLand.Controls.Add(numCgiStep);
            panelCgiLand.Controls.Add(lblCgiTarget);
            panelCgiLand.Controls.Add(numCgiTarget);
            panelCgiLand.Controls.Add(lblCgiLandInfo);
            panelCgiLand.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelCgiLand.Size = new System.Drawing.Size(290, 196);
            panelCgiLand.Name = "panelCgiLand";
            // 
            // picCgiLandArt
            // 
            picCgiLandArt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picCgiLandArt.Location = new System.Drawing.Point(4, 4);
            picCgiLandArt.Size = new System.Drawing.Size(60, 60);
            picCgiLandArt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picCgiLandArt.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picCgiLandArt.Name = "picCgiLandArt";
            // 
            // picCgiLandTex
            // 
            picCgiLandTex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picCgiLandTex.Location = new System.Drawing.Point(68, 4);
            picCgiLandTex.Size = new System.Drawing.Size(60, 60);
            picCgiLandTex.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picCgiLandTex.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picCgiLandTex.Name = "picCgiLandTex";
            // 
            // lblCgiLandId
            // 
            lblCgiLandId.AutoSize = true;
            lblCgiLandId.Location = new System.Drawing.Point(136, 8);
            lblCgiLandId.Text = "Land (hex):";
            lblCgiLandId.Name = "lblCgiLandId";
            // 
            // txtCgiLandId
            // 
            txtCgiLandId.Location = new System.Drawing.Point(206, 4);
            txtCgiLandId.Size = new System.Drawing.Size(70, 23);
            txtCgiLandId.Text = "0003";
            txtCgiLandId.Name = "txtCgiLandId";
            txtCgiLandId.TextChanged += CgiLandSettings_Changed;
            // 
            // lblCgiRadius
            // 
            lblCgiRadius.AutoSize = true;
            lblCgiRadius.Location = new System.Drawing.Point(136, 37);
            lblCgiRadius.Text = "Radius:";
            lblCgiRadius.Name = "lblCgiRadius";
            // 
            // numCgiRadius
            // 
            numCgiRadius.Location = new System.Drawing.Point(206, 33);
            numCgiRadius.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numCgiRadius.Size = new System.Drawing.Size(70, 23);
            numCgiRadius.Name = "numCgiRadius";
            numCgiRadius.ValueChanged += CgiLandSettings_Changed;
            // 
            // lblCgiStep
            // 
            lblCgiStep.AutoSize = true;
            lblCgiStep.Location = new System.Drawing.Point(4, 74);
            lblCgiStep.Text = "Höhe ± Schritt:";
            lblCgiStep.Name = "lblCgiStep";
            // 
            // numCgiStep
            // 
            numCgiStep.Location = new System.Drawing.Point(110, 70);
            numCgiStep.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numCgiStep.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCgiStep.Size = new System.Drawing.Size(60, 23);
            numCgiStep.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numCgiStep.Name = "numCgiStep";
            numCgiStep.ValueChanged += CgiLandSettings_Changed;
            // 
            // lblCgiTarget
            // 
            lblCgiTarget.AutoSize = true;
            lblCgiTarget.Location = new System.Drawing.Point(176, 74);
            lblCgiTarget.Text = "Höhe = :";
            lblCgiTarget.Name = "lblCgiTarget";
            // 
            // numCgiTarget
            // 
            numCgiTarget.Location = new System.Drawing.Point(230, 70);
            numCgiTarget.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            numCgiTarget.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            numCgiTarget.Size = new System.Drawing.Size(52, 23);
            numCgiTarget.Name = "numCgiTarget";
            numCgiTarget.ValueChanged += CgiLandSettings_Changed;
            // 
            // lblCgiLandInfo
            // 
            lblCgiLandInfo.Location = new System.Drawing.Point(4, 100);
            lblCgiLandInfo.Size = new System.Drawing.Size(280, 92);
            lblCgiLandInfo.Text = "Werkzeuge 'Land setzen', 'Höhe +/−/=', 'Glätten' oben. Ziehen mit gedrückter Maus malt; ein Zug = ein Rückgängig-Schritt. Rechtsklick nimmt Land-ID und Höhe auf.";
            lblCgiLandInfo.Name = "lblCgiLandInfo";
            // 
            // picCgiMulti
            // 
            picCgiMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            picCgiMulti.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picCgiMulti.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picCgiMulti.Name = "picCgiMulti";
            // 
            // panelCgiMultiTop
            // 
            panelCgiMultiTop.Controls.Add(lblCgiMultiId);
            panelCgiMultiTop.Controls.Add(numCgiMultiId);
            panelCgiMultiTop.Controls.Add(chkCgiMultiInvisible);
            panelCgiMultiTop.Controls.Add(lblCgiMultiInfo);
            panelCgiMultiTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelCgiMultiTop.Size = new System.Drawing.Size(290, 92);
            panelCgiMultiTop.Name = "panelCgiMultiTop";
            // 
            // lblCgiMultiId
            // 
            lblCgiMultiId.AutoSize = true;
            lblCgiMultiId.Location = new System.Drawing.Point(4, 8);
            lblCgiMultiId.Text = "Multi-ID (hex):";
            lblCgiMultiId.Name = "lblCgiMultiId";
            // 
            // numCgiMultiId
            // 
            numCgiMultiId.Hexadecimal = true;
            numCgiMultiId.Location = new System.Drawing.Point(100, 4);
            numCgiMultiId.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numCgiMultiId.Size = new System.Drawing.Size(80, 23);
            numCgiMultiId.Name = "numCgiMultiId";
            numCgiMultiId.ValueChanged += NumCgiMultiId_ValueChanged;
            // 
            // chkCgiMultiInvisible
            // 
            chkCgiMultiInvisible.AutoSize = true;
            chkCgiMultiInvisible.Location = new System.Drawing.Point(4, 34);
            chkCgiMultiInvisible.Text = "unsichtbare Teile mitsetzen";
            chkCgiMultiInvisible.Name = "chkCgiMultiInvisible";
            chkCgiMultiInvisible.CheckedChanged += CgiPlaceSettings_Changed;
            // 
            // lblCgiMultiInfo
            // 
            lblCgiMultiInfo.Location = new System.Drawing.Point(4, 58);
            lblCgiMultiInfo.Size = new System.Drawing.Size(280, 32);
            lblCgiMultiInfo.Text = "Werkzeug 'Multi setzen' wählen und in die Karte klicken.";
            lblCgiMultiInfo.Name = "lblCgiMultiInfo";
            // 
            // propCgiSelection
            // 
            propCgiSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            propCgiSelection.ToolbarVisible = false;
            propCgiSelection.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            propCgiSelection.Name = "propCgiSelection";
            propCgiSelection.PropertyValueChanged += PropCgiSelection_PropertyValueChanged;
            // 
            // lblCgiSelInfo
            // 
            lblCgiSelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            lblCgiSelInfo.Size = new System.Drawing.Size(290, 96);
            lblCgiSelInfo.Text = "Nichts ausgewählt.";
            lblCgiSelInfo.Padding = new System.Windows.Forms.Padding(3);
            lblCgiSelInfo.Name = "lblCgiSelInfo";
            // 
            // lstCgiProblems
            // 
            lstCgiProblems.Dock = System.Windows.Forms.DockStyle.Fill;
            lstCgiProblems.HorizontalScrollbar = true;
            lstCgiProblems.IntegralHeight = false;
            lstCgiProblems.Name = "lstCgiProblems";
            lstCgiProblems.DoubleClick += LstCgiProblems_DoubleClick;
            lstCgiProblems.SelectedIndexChanged += LstCgiProblems_DoubleClick;
            // 
            // panelCgiProblemsTop
            // 
            panelCgiProblemsTop.Controls.Add(lblCgiCliff);
            panelCgiProblemsTop.Controls.Add(numCgiCliff);
            panelCgiProblemsTop.Controls.Add(btnCgiScan);
            panelCgiProblemsTop.Controls.Add(btnCgiRemoveDup);
            panelCgiProblemsTop.Controls.Add(btnCgiSaveLog);
            panelCgiProblemsTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelCgiProblemsTop.Size = new System.Drawing.Size(290, 92);
            panelCgiProblemsTop.Name = "panelCgiProblemsTop";
            // 
            // lblCgiCliff
            // 
            lblCgiCliff.AutoSize = true;
            lblCgiCliff.Location = new System.Drawing.Point(4, 8);
            lblCgiCliff.Text = "Steilkante ab z:";
            lblCgiCliff.Name = "lblCgiCliff";
            // 
            // numCgiCliff
            // 
            numCgiCliff.Location = new System.Drawing.Point(110, 4);
            numCgiCliff.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numCgiCliff.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCgiCliff.Size = new System.Drawing.Size(60, 23);
            numCgiCliff.Value = new decimal(new int[] { 40, 0, 0, 0 });
            numCgiCliff.Name = "numCgiCliff";
            // 
            // btnCgiScan
            // 
            btnCgiScan.Location = new System.Drawing.Point(4, 31);
            btnCgiScan.Size = new System.Drawing.Size(166, 25);
            btnCgiScan.Text = "Ganze Karte prüfen";
            btnCgiScan.UseVisualStyleBackColor = true;
            btnCgiScan.Name = "btnCgiScan";
            btnCgiScan.Click += BtnCgiScan_Click;
            // 
            // btnCgiRemoveDup
            // 
            btnCgiRemoveDup.Location = new System.Drawing.Point(4, 60);
            btnCgiRemoveDup.Size = new System.Drawing.Size(130, 25);
            btnCgiRemoveDup.Text = "Doppelte entfernen";
            btnCgiRemoveDup.UseVisualStyleBackColor = true;
            btnCgiRemoveDup.Name = "btnCgiRemoveDup";
            btnCgiRemoveDup.Click += BtnCgiRemoveDup_Click;
            // 
            // btnCgiSaveLog
            // 
            btnCgiSaveLog.Location = new System.Drawing.Point(140, 60);
            btnCgiSaveLog.Size = new System.Drawing.Size(110, 25);
            btnCgiSaveLog.Text = "Log speichern";
            btnCgiSaveLog.UseVisualStyleBackColor = true;
            btnCgiSaveLog.Name = "btnCgiSaveLog";
            btnCgiSaveLog.Click += BtnCgiSaveLog_Click;
            // 
            // toolStripCgiLand
            // 
            toolStripCgiLand.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tslCgiLandTools, tsbCgiLandSet, tsbCgiRaise, tsbCgiLower, tsbCgiSetHeight, tsbCgiSmooth, tsbCgiArea });
            toolStripCgiLand.Dock = System.Windows.Forms.DockStyle.Top;
            toolStripCgiLand.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripCgiLand.Name = "toolStripCgiLand";
            // 
            // tslCgiLandTools
            // 
            tslCgiLandTools.Text = "Land:";
            tslCgiLandTools.Name = "tslCgiLandTools";
            // 
            // tsbCgiLandSet
            // 
            tsbCgiLandSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiLandSet.Text = "Land setzen";
            tsbCgiLandSet.ToolTipText = "Land-Tile / Textur (Reiter Land) malen – Rechtsklick nimmt Land-ID und Höhe auf";
            tsbCgiLandSet.Name = "tsbCgiLandSet";
            tsbCgiLandSet.Click += CgiTool_Click;
            // 
            // tsbCgiRaise
            // 
            tsbCgiRaise.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiRaise.Text = "Höhe +";
            tsbCgiRaise.ToolTipText = "Land anheben (Schritt und Radius rechts im Reiter Land)";
            tsbCgiRaise.Name = "tsbCgiRaise";
            tsbCgiRaise.Click += CgiTool_Click;
            // 
            // tsbCgiLower
            // 
            tsbCgiLower.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiLower.Text = "Höhe −";
            tsbCgiLower.ToolTipText = "Land absenken";
            tsbCgiLower.Name = "tsbCgiLower";
            tsbCgiLower.Click += CgiTool_Click;
            // 
            // tsbCgiSetHeight
            // 
            tsbCgiSetHeight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiSetHeight.Text = "Höhe =";
            tsbCgiSetHeight.ToolTipText = "Land auf eine feste Höhe setzen";
            tsbCgiSetHeight.Name = "tsbCgiSetHeight";
            tsbCgiSetHeight.Click += CgiTool_Click;
            // 
            // tsbCgiSmooth
            // 
            tsbCgiSmooth.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiSmooth.Text = "Glätten";
            tsbCgiSmooth.ToolTipText = "Land-Höhen glätten";
            tsbCgiSmooth.Name = "tsbCgiSmooth";
            tsbCgiSmooth.Click += CgiTool_Click;
            // 
            // tsbCgiArea
            // 
            tsbCgiArea.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiArea.Text = "Bereich";
            tsbCgiArea.ToolTipText = "Bereich aufziehen: Strg+C / Strg+V (Land + Statics), Entf = Statics löschen, Bild↑/↓ = heben / senken";
            tsbCgiArea.Name = "tsbCgiArea";
            tsbCgiArea.Click += CgiTool_Click;
            // 
            // toolStripCgi
            // 
            toolStripCgi.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbCgiBuild, tsbCgiLoad, tsbCgiSave, tsbCgiSaveAs, tssCgi1, tsbCgiSelect, tsbCgiPlace, tsbCgiMulti, tsbCgiDelete, tsbCgiHue, tssCgi2, tsbCgiUndo, tsbCgiRedo, tssCgi3, tsbCgiZoomIn, tsbCgiZoomOut, tsbCgiGrid, tsbCgiHeights, tsbCgiStatics, tsbCgiProblems, tslCgiMaxZ, tscCgiMaxZ });
            toolStripCgi.Dock = System.Windows.Forms.DockStyle.Top;
            toolStripCgi.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripCgi.Name = "toolStripCgi";
            // 
            // tsbCgiBuild
            // 
            tsbCgiBuild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiBuild.Text = "Aus Editor-Karte bauen";
            tsbCgiBuild.ToolTipText = "Baut die Karte aus dem Editor im Speicher (Übergänge, Items …) und zeigt sie an";
            tsbCgiBuild.Name = "tsbCgiBuild";
            tsbCgiBuild.Click += TsbCgiBuild_Click;
            // 
            // tsbCgiLoad
            // 
            tsbCgiLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiLoad.Text = "MUL / UOP laden…";
            tsbCgiLoad.ToolTipText = "map#.mul / map#LegacyMUL.uop + staidx/statics aus dem gleichen Ordner";
            tsbCgiLoad.Name = "tsbCgiLoad";
            tsbCgiLoad.Click += TsbCgiLoad_Click;
            // 
            // tsbCgiSave
            // 
            tsbCgiSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiSave.Text = "Speichern";
            tsbCgiSave.ToolTipText = "Statics (und bei gebauter Karte auch die Map) speichern – alte Dateien bekommen .bak";
            tsbCgiSave.Name = "tsbCgiSave";
            tsbCgiSave.Click += TsbCgiSave_Click;
            // 
            // tsbCgiSaveAs
            // 
            tsbCgiSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiSaveAs.Text = "Speichern unter…";
            tsbCgiSaveAs.Name = "tsbCgiSaveAs";
            tsbCgiSaveAs.Click += TsbCgiSaveAs_Click;
            // 
            // tssCgi1
            // 
            tssCgi1.Name = "tssCgi1";
            // 
            // tsbCgiSelect
            // 
            tsbCgiSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiSelect.Text = "Auswahl";
            tsbCgiSelect.ToolTipText = "Klick = auswählen, Strg+Klick = dazu, Umschalt+Klick = alle auf dem Tile";
            tsbCgiSelect.Name = "tsbCgiSelect";
            tsbCgiSelect.Click += CgiTool_Click;
            // 
            // tsbCgiPlace
            // 
            tsbCgiPlace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiPlace.Text = "Static setzen";
            tsbCgiPlace.ToolTipText = "Gewähltes Item (rechts) setzen";
            tsbCgiPlace.Name = "tsbCgiPlace";
            tsbCgiPlace.Click += CgiTool_Click;
            // 
            // tsbCgiMulti
            // 
            tsbCgiMulti.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiMulti.Text = "Multi setzen";
            tsbCgiMulti.ToolTipText = "Gewähltes Multi (rechts) als Statics setzen";
            tsbCgiMulti.Name = "tsbCgiMulti";
            tsbCgiMulti.Click += CgiTool_Click;
            // 
            // tsbCgiDelete
            // 
            tsbCgiDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiDelete.Text = "Löschen";
            tsbCgiDelete.Name = "tsbCgiDelete";
            tsbCgiDelete.Click += CgiTool_Click;
            // 
            // tsbCgiHue
            // 
            tsbCgiHue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiHue.Text = "Färben";
            tsbCgiHue.ToolTipText = "Klick färbt das Static mit der Hue (rechts)";
            tsbCgiHue.Name = "tsbCgiHue";
            tsbCgiHue.Click += CgiTool_Click;
            // 
            // tssCgi2
            // 
            tssCgi2.Name = "tssCgi2";
            // 
            // tsbCgiUndo
            // 
            tsbCgiUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiUndo.Text = "Rückgängig";
            tsbCgiUndo.Name = "tsbCgiUndo";
            tsbCgiUndo.Click += TsbCgiUndo_Click;
            // 
            // tsbCgiRedo
            // 
            tsbCgiRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiRedo.Text = "Wiederholen";
            tsbCgiRedo.Name = "tsbCgiRedo";
            tsbCgiRedo.Click += TsbCgiRedo_Click;
            // 
            // tssCgi3
            // 
            tssCgi3.Name = "tssCgi3";
            // 
            // tsbCgiZoomIn
            // 
            tsbCgiZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiZoomIn.Text = "Zoom +";
            tsbCgiZoomIn.Name = "tsbCgiZoomIn";
            tsbCgiZoomIn.Click += TsbCgiZoomIn_Click;
            // 
            // tsbCgiZoomOut
            // 
            tsbCgiZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiZoomOut.Text = "Zoom −";
            tsbCgiZoomOut.Name = "tsbCgiZoomOut";
            tsbCgiZoomOut.Click += TsbCgiZoomOut_Click;
            // 
            // tsbCgiGrid
            // 
            tsbCgiGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiGrid.Text = "Raster";
            tsbCgiGrid.CheckOnClick = true;
            tsbCgiGrid.Name = "tsbCgiGrid";
            tsbCgiGrid.Click += TsbCgiView_Click;
            // 
            // tsbCgiHeights
            // 
            tsbCgiHeights.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiHeights.Text = "Höhen";
            tsbCgiHeights.ToolTipText = "Land-Höhe jedes Tiles einblenden";
            tsbCgiHeights.CheckOnClick = true;
            tsbCgiHeights.Name = "tsbCgiHeights";
            tsbCgiHeights.Click += TsbCgiView_Click;
            // 
            // tsbCgiStatics
            // 
            tsbCgiStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiStatics.Text = "Statics";
            tsbCgiStatics.ToolTipText = "Statics anzeigen";
            tsbCgiStatics.CheckOnClick = true;
            tsbCgiStatics.Name = "tsbCgiStatics";
            tsbCgiStatics.Click += TsbCgiView_Click;
            // 
            // tsbCgiProblems
            // 
            tsbCgiProblems.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbCgiProblems.Text = "Probleme";
            tsbCgiProblems.ToolTipText = "Gefundene Probleme rot markieren";
            tsbCgiProblems.CheckOnClick = true;
            tsbCgiProblems.Name = "tsbCgiProblems";
            tsbCgiProblems.Click += TsbCgiView_Click;
            // 
            // tslCgiMaxZ
            // 
            tslCgiMaxZ.Text = "Max Z:";
            tslCgiMaxZ.Name = "tslCgiMaxZ";
            // 
            // tscCgiMaxZ
            // 
            tscCgiMaxZ.Size = new System.Drawing.Size(56, 25);
            tscCgiMaxZ.Name = "tscCgiMaxZ";
            tscCgiMaxZ.SelectedIndexChanged += TscCgiMaxZ_Changed;
            tscCgiMaxZ.TextChanged += TscCgiMaxZ_Changed;
            // 
            // splitGenerator
            // 
            splitGenerator.Panel1.Controls.Add(tabGenOptions);
            splitGenerator.Panel2.Controls.Add(picGenPreview);
            splitGenerator.Panel2.Controls.Add(lblGenInfo);
            splitGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            splitGenerator.Orientation = System.Windows.Forms.Orientation.Vertical;
            splitGenerator.Size = new System.Drawing.Size(1170, 634);
            splitGenerator.SplitterDistance = 440;
            splitGenerator.Name = "splitGenerator";
            // 
            // tabGenOptions
            // 
            tabGenOptions.Controls.Add(tabGenSettings);
            tabGenOptions.Controls.Add(tabGenBiomes);
            tabGenOptions.Controls.Add(tabOutline);
            tabGenOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            tabGenOptions.Name = "tabGenOptions";
            // 
            // tabGenSettings
            // 
            tabGenSettings.Controls.Add(propGenerator);
            tabGenSettings.Controls.Add(flowGenButtons);
            tabGenSettings.Text = "Generator";
            tabGenSettings.UseVisualStyleBackColor = true;
            tabGenSettings.Padding = new System.Windows.Forms.Padding(3);
            tabGenSettings.Name = "tabGenSettings";
            tabGenSettings.Location = new System.Drawing.Point(4, 24);
            tabGenSettings.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabGenBiomes
            // 
            tabGenBiomes.Controls.Add(gridBiomes);
            tabGenBiomes.Controls.Add(flowBiomeButtons);
            tabGenBiomes.Text = "Biome";
            tabGenBiomes.UseVisualStyleBackColor = true;
            tabGenBiomes.Padding = new System.Windows.Forms.Padding(3);
            tabGenBiomes.Name = "tabGenBiomes";
            tabGenBiomes.Location = new System.Drawing.Point(4, 24);
            tabGenBiomes.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabOutline
            // 
            tabOutline.Controls.Add(propOutline);
            tabOutline.Controls.Add(flowOutlineButtons);
            tabOutline.Text = "Umriss-Bild";
            tabOutline.UseVisualStyleBackColor = true;
            tabOutline.Padding = new System.Windows.Forms.Padding(3);
            tabOutline.Name = "tabOutline";
            tabOutline.Location = new System.Drawing.Point(4, 24);
            tabOutline.Size = new System.Drawing.Size(1176, 640);
            // 
            // propGenerator
            // 
            propGenerator.Dock = System.Windows.Forms.DockStyle.Fill;
            propGenerator.ToolbarVisible = true;
            propGenerator.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propGenerator.Name = "propGenerator";
            // 
            // flowGenButtons
            // 
            flowGenButtons.Controls.Add(btnGenPreview);
            flowGenButtons.Controls.Add(btnGenNewSeed);
            flowGenButtons.Controls.Add(btnGenToEditor);
            flowGenButtons.Controls.Add(btnGenSavePreset);
            flowGenButtons.Controls.Add(btnGenLoadPreset);
            flowGenButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowGenButtons.Size = new System.Drawing.Size(200, 70);
            flowGenButtons.Padding = new System.Windows.Forms.Padding(3);
            flowGenButtons.Name = "flowGenButtons";
            // 
            // btnGenPreview
            // 
            btnGenPreview.Text = "Vorschau";
            btnGenPreview.UseVisualStyleBackColor = true;
            btnGenPreview.AutoSize = true;
            btnGenPreview.Margin = new System.Windows.Forms.Padding(3);
            btnGenPreview.Name = "btnGenPreview";
            btnGenPreview.Click += BtnGenPreview_Click;
            // 
            // btnGenNewSeed
            // 
            btnGenNewSeed.Text = "Neuer Seed + Vorschau";
            btnGenNewSeed.UseVisualStyleBackColor = true;
            btnGenNewSeed.AutoSize = true;
            btnGenNewSeed.Margin = new System.Windows.Forms.Padding(3);
            btnGenNewSeed.Name = "btnGenNewSeed";
            btnGenNewSeed.Click += BtnGenNewSeed_Click;
            // 
            // btnGenToEditor
            // 
            btnGenToEditor.Text = "Karte erzeugen → Editor";
            btnGenToEditor.UseVisualStyleBackColor = true;
            btnGenToEditor.AutoSize = true;
            btnGenToEditor.Margin = new System.Windows.Forms.Padding(3);
            btnGenToEditor.Name = "btnGenToEditor";
            btnGenToEditor.Click += BtnGenToEditor_Click;
            // 
            // btnGenSavePreset
            // 
            btnGenSavePreset.Text = "Preset speichern…";
            btnGenSavePreset.UseVisualStyleBackColor = true;
            btnGenSavePreset.AutoSize = true;
            btnGenSavePreset.Margin = new System.Windows.Forms.Padding(3);
            btnGenSavePreset.Name = "btnGenSavePreset";
            btnGenSavePreset.Click += BtnGenSavePreset_Click;
            // 
            // btnGenLoadPreset
            // 
            btnGenLoadPreset.Text = "Preset laden…";
            btnGenLoadPreset.UseVisualStyleBackColor = true;
            btnGenLoadPreset.AutoSize = true;
            btnGenLoadPreset.Margin = new System.Windows.Forms.Padding(3);
            btnGenLoadPreset.Name = "btnGenLoadPreset";
            btnGenLoadPreset.Click += BtnGenLoadPreset_Click;
            // 
            // gridBiomes
            // 
            gridBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridBiomes.AllowUserToAddRows = true;
            gridBiomes.AllowUserToDeleteRows = true;
            gridBiomes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridBiomes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridBiomes.RowHeadersWidth = 28;
            gridBiomes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridBiomes.Name = "gridBiomes";
            gridBiomes.DataError += Grid_DataError;
            // 
            // flowBiomeButtons
            // 
            flowBiomeButtons.Controls.Add(btnBiomesDefault);
            flowBiomeButtons.Controls.Add(lblBiomeHint);
            flowBiomeButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowBiomeButtons.Size = new System.Drawing.Size(200, 38);
            flowBiomeButtons.Padding = new System.Windows.Forms.Padding(3);
            flowBiomeButtons.Name = "flowBiomeButtons";
            // 
            // btnBiomesDefault
            // 
            btnBiomesDefault.Text = "Standard-Biome";
            btnBiomesDefault.UseVisualStyleBackColor = true;
            btnBiomesDefault.AutoSize = true;
            btnBiomesDefault.Margin = new System.Windows.Forms.Padding(3);
            btnBiomesDefault.Name = "btnBiomesDefault";
            btnBiomesDefault.Click += BtnBiomesDefault_Click;
            // 
            // lblBiomeHint
            // 
            lblBiomeHint.AutoSize = true;
            lblBiomeHint.Margin = new System.Windows.Forms.Padding(6, 8, 3, 0);
            lblBiomeHint.Text = "Reihenfolge = Priorität. Temperatur/Feuchte 0..1, Gruppe dezimal.";
            lblBiomeHint.Name = "lblBiomeHint";
            // 
            // propOutline
            // 
            propOutline.Dock = System.Windows.Forms.DockStyle.Fill;
            propOutline.ToolbarVisible = true;
            propOutline.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propOutline.Name = "propOutline";
            // 
            // flowOutlineButtons
            // 
            flowOutlineButtons.Controls.Add(btnOutlineLoad);
            flowOutlineButtons.Controls.Add(btnOutlinePreview);
            flowOutlineButtons.Controls.Add(btnOutlineApply);
            flowOutlineButtons.Controls.Add(btnOutlineAsMask);
            flowOutlineButtons.Controls.Add(btnMaskFromMap);
            flowOutlineButtons.Controls.Add(btnMaskClear);
            flowOutlineButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowOutlineButtons.Size = new System.Drawing.Size(200, 100);
            flowOutlineButtons.Padding = new System.Windows.Forms.Padding(3);
            flowOutlineButtons.Name = "flowOutlineButtons";
            // 
            // btnOutlineLoad
            // 
            btnOutlineLoad.Text = "Bild laden…";
            btnOutlineLoad.UseVisualStyleBackColor = true;
            btnOutlineLoad.AutoSize = true;
            btnOutlineLoad.Margin = new System.Windows.Forms.Padding(3);
            btnOutlineLoad.Name = "btnOutlineLoad";
            btnOutlineLoad.Click += BtnOutlineLoad_Click;
            // 
            // btnOutlinePreview
            // 
            btnOutlinePreview.Text = "Vorschau";
            btnOutlinePreview.UseVisualStyleBackColor = true;
            btnOutlinePreview.AutoSize = true;
            btnOutlinePreview.Margin = new System.Windows.Forms.Padding(3);
            btnOutlinePreview.Name = "btnOutlinePreview";
            btnOutlinePreview.Click += BtnOutlinePreview_Click;
            // 
            // btnOutlineApply
            // 
            btnOutlineApply.Text = "In Karte zeichnen";
            btnOutlineApply.UseVisualStyleBackColor = true;
            btnOutlineApply.AutoSize = true;
            btnOutlineApply.Margin = new System.Windows.Forms.Padding(3);
            btnOutlineApply.Name = "btnOutlineApply";
            btnOutlineApply.Click += BtnOutlineApply_Click;
            // 
            // btnOutlineAsMask
            // 
            btnOutlineAsMask.Text = "Als Maske für Generator";
            btnOutlineAsMask.UseVisualStyleBackColor = true;
            btnOutlineAsMask.AutoSize = true;
            btnOutlineAsMask.Margin = new System.Windows.Forms.Padding(3);
            btnOutlineAsMask.Name = "btnOutlineAsMask";
            btnOutlineAsMask.Click += BtnOutlineAsMask_Click;
            // 
            // btnMaskFromMap
            // 
            btnMaskFromMap.Text = "Maske aus aktueller Karte";
            btnMaskFromMap.UseVisualStyleBackColor = true;
            btnMaskFromMap.AutoSize = true;
            btnMaskFromMap.Margin = new System.Windows.Forms.Padding(3);
            btnMaskFromMap.Name = "btnMaskFromMap";
            btnMaskFromMap.Click += BtnMaskFromMap_Click;
            // 
            // btnMaskClear
            // 
            btnMaskClear.Text = "Maske löschen";
            btnMaskClear.UseVisualStyleBackColor = true;
            btnMaskClear.AutoSize = true;
            btnMaskClear.Margin = new System.Windows.Forms.Padding(3);
            btnMaskClear.Name = "btnMaskClear";
            btnMaskClear.Click += BtnMaskClear_Click;
            // 
            // picGenPreview
            // 
            picGenPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picGenPreview.BackColor = System.Drawing.Color.FromArgb(30, 30, 34);
            picGenPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picGenPreview.Name = "picGenPreview";
            // 
            // lblGenInfo
            // 
            lblGenInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblGenInfo.Size = new System.Drawing.Size(400, 44);
            lblGenInfo.Text = "Vorschau: Einstellungen links wählen und 'Vorschau' klicken.";
            lblGenInfo.Padding = new System.Windows.Forms.Padding(4);
            lblGenInfo.Name = "lblGenInfo";
            // 
            // tabRulesInner
            // 
            tabRulesInner.Controls.Add(tabRuleGroups);
            tabRulesInner.Controls.Add(tabRulePalette);
            tabRulesInner.Controls.Add(tabRuleTransitions);
            tabRulesInner.Controls.Add(tabRuleItems);
            tabRulesInner.Controls.Add(tabRuleUneven);
            tabRulesInner.Controls.Add(tabRuleFixes);
            tabRulesInner.Controls.Add(tabRuleAliases);
            tabRulesInner.Controls.Add(tabRuleWarnings);
            tabRulesInner.Dock = System.Windows.Forms.DockStyle.Fill;
            tabRulesInner.Name = "tabRulesInner";
            // 
            // panelRulesTop
            // 
            panelRulesTop.Controls.Add(lblRuleSetInfo);
            panelRulesTop.Controls.Add(btnRulesReload);
            panelRulesTop.Controls.Add(btnRulesSaveJson);
            panelRulesTop.Controls.Add(btnRulesExport);
            panelRulesTop.Controls.Add(btnRulesValidate);
            panelRulesTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelRulesTop.Size = new System.Drawing.Size(800, 36);
            panelRulesTop.Padding = new System.Windows.Forms.Padding(2);
            panelRulesTop.Name = "panelRulesTop";
            // 
            // lblRuleSetInfo
            // 
            lblRuleSetInfo.AutoSize = true;
            lblRuleSetInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblRuleSetInfo.Margin = new System.Windows.Forms.Padding(3, 8, 12, 0);
            lblRuleSetInfo.Text = "Regelsatz";
            lblRuleSetInfo.Name = "lblRuleSetInfo";
            // 
            // btnRulesReload
            // 
            btnRulesReload.Text = "Neu laden";
            btnRulesReload.UseVisualStyleBackColor = true;
            btnRulesReload.AutoSize = true;
            btnRulesReload.Margin = new System.Windows.Forms.Padding(3);
            btnRulesReload.Name = "btnRulesReload";
            btnRulesReload.Click += BtnRulesReload_Click;
            // 
            // btnRulesSaveJson
            // 
            btnRulesSaveJson.Text = "JSON speichern…";
            btnRulesSaveJson.UseVisualStyleBackColor = true;
            btnRulesSaveJson.AutoSize = true;
            btnRulesSaveJson.Margin = new System.Windows.Forms.Padding(3);
            btnRulesSaveJson.Name = "btnRulesSaveJson";
            btnRulesSaveJson.Click += MnuSaveJson_Click;
            // 
            // btnRulesExport
            // 
            btnRulesExport.Text = "Als Dragon-Skripte exportieren…";
            btnRulesExport.UseVisualStyleBackColor = true;
            btnRulesExport.AutoSize = true;
            btnRulesExport.Margin = new System.Windows.Forms.Padding(3);
            btnRulesExport.Name = "btnRulesExport";
            btnRulesExport.Click += MnuExportScripts_Click;
            // 
            // btnRulesValidate
            // 
            btnRulesValidate.Text = "Prüfen";
            btnRulesValidate.UseVisualStyleBackColor = true;
            btnRulesValidate.AutoSize = true;
            btnRulesValidate.Margin = new System.Windows.Forms.Padding(3);
            btnRulesValidate.Name = "btnRulesValidate";
            btnRulesValidate.Click += MnuValidate_Click;
            // 
            // tabRuleGroups
            // 
            tabRuleGroups.Controls.Add(gridGroups);
            tabRuleGroups.Text = "Gruppen";
            tabRuleGroups.UseVisualStyleBackColor = true;
            tabRuleGroups.Padding = new System.Windows.Forms.Padding(3);
            tabRuleGroups.Name = "tabRuleGroups";
            tabRuleGroups.Location = new System.Drawing.Point(4, 24);
            tabRuleGroups.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRulePalette
            // 
            tabRulePalette.Controls.Add(gridPaletteEntries);
            tabRulePalette.Text = "Farben (maptrans)";
            tabRulePalette.UseVisualStyleBackColor = true;
            tabRulePalette.Padding = new System.Windows.Forms.Padding(3);
            tabRulePalette.Name = "tabRulePalette";
            tabRulePalette.Location = new System.Drawing.Point(4, 24);
            tabRulePalette.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleTransitions
            // 
            tabRuleTransitions.Controls.Add(splitTransitions);
            tabRuleTransitions.Text = "Übergänge";
            tabRuleTransitions.UseVisualStyleBackColor = true;
            tabRuleTransitions.Padding = new System.Windows.Forms.Padding(3);
            tabRuleTransitions.Name = "tabRuleTransitions";
            tabRuleTransitions.Location = new System.Drawing.Point(4, 24);
            tabRuleTransitions.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleItems
            // 
            tabRuleItems.Controls.Add(splitItems);
            tabRuleItems.Text = "Items";
            tabRuleItems.UseVisualStyleBackColor = true;
            tabRuleItems.Padding = new System.Windows.Forms.Padding(3);
            tabRuleItems.Name = "tabRuleItems";
            tabRuleItems.Location = new System.Drawing.Point(4, 24);
            tabRuleItems.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleUneven
            // 
            tabRuleUneven.Controls.Add(gridUneven);
            tabRuleUneven.Text = "Unebenheit";
            tabRuleUneven.UseVisualStyleBackColor = true;
            tabRuleUneven.Padding = new System.Windows.Forms.Padding(3);
            tabRuleUneven.Name = "tabRuleUneven";
            tabRuleUneven.Location = new System.Drawing.Point(4, 24);
            tabRuleUneven.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleFixes
            // 
            tabRuleFixes.Controls.Add(gridFixes);
            tabRuleFixes.Text = "Fix-Tabelle";
            tabRuleFixes.UseVisualStyleBackColor = true;
            tabRuleFixes.Padding = new System.Windows.Forms.Padding(3);
            tabRuleFixes.Name = "tabRuleFixes";
            tabRuleFixes.Location = new System.Drawing.Point(4, 24);
            tabRuleFixes.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleAliases
            // 
            tabRuleAliases.Controls.Add(gridAliases);
            tabRuleAliases.Text = "Farb-Aliase";
            tabRuleAliases.UseVisualStyleBackColor = true;
            tabRuleAliases.Padding = new System.Windows.Forms.Padding(3);
            tabRuleAliases.Name = "tabRuleAliases";
            tabRuleAliases.Location = new System.Drawing.Point(4, 24);
            tabRuleAliases.Size = new System.Drawing.Size(1176, 640);
            // 
            // tabRuleWarnings
            // 
            tabRuleWarnings.Controls.Add(txtRuleWarnings);
            tabRuleWarnings.Text = "Hinweise";
            tabRuleWarnings.UseVisualStyleBackColor = true;
            tabRuleWarnings.Padding = new System.Windows.Forms.Padding(3);
            tabRuleWarnings.Name = "tabRuleWarnings";
            tabRuleWarnings.Location = new System.Drawing.Point(4, 24);
            tabRuleWarnings.Size = new System.Drawing.Size(1176, 640);
            // 
            // gridGroups
            // 
            gridGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            gridGroups.AllowUserToAddRows = true;
            gridGroups.AllowUserToDeleteRows = true;
            gridGroups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridGroups.RowHeadersWidth = 28;
            gridGroups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridGroups.Name = "gridGroups";
            gridGroups.DataError += Grid_DataError;
            // 
            // gridPaletteEntries
            // 
            gridPaletteEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPaletteEntries.AllowUserToAddRows = false;
            gridPaletteEntries.AllowUserToDeleteRows = true;
            gridPaletteEntries.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridPaletteEntries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridPaletteEntries.RowHeadersWidth = 28;
            gridPaletteEntries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridPaletteEntries.Name = "gridPaletteEntries";
            gridPaletteEntries.DataError += Grid_DataError;
            // 
            // splitTransitions
            // 
            splitTransitions.Panel1.Controls.Add(gridTransitions);
            splitTransitions.Panel2.Controls.Add(gridPatterns);
            splitTransitions.Panel2.Controls.Add(lblPatternFile);
            splitTransitions.Dock = System.Windows.Forms.DockStyle.Fill;
            splitTransitions.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitTransitions.Size = new System.Drawing.Size(1170, 634);
            splitTransitions.SplitterDistance = 250;
            splitTransitions.Name = "splitTransitions";
            // 
            // gridTransitions
            // 
            gridTransitions.Dock = System.Windows.Forms.DockStyle.Fill;
            gridTransitions.AllowUserToAddRows = true;
            gridTransitions.AllowUserToDeleteRows = true;
            gridTransitions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridTransitions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridTransitions.RowHeadersWidth = 28;
            gridTransitions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridTransitions.Name = "gridTransitions";
            gridTransitions.DataError += Grid_DataError;
            // 
            // gridPatterns
            // 
            gridPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
            gridPatterns.AllowUserToAddRows = true;
            gridPatterns.AllowUserToDeleteRows = true;
            gridPatterns.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridPatterns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridPatterns.RowHeadersWidth = 28;
            gridPatterns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridPatterns.Name = "gridPatterns";
            gridPatterns.DataError += Grid_DataError;
            // 
            // lblPatternFile
            // 
            lblPatternFile.Dock = System.Windows.Forms.DockStyle.Top;
            lblPatternFile.Size = new System.Drawing.Size(400, 22);
            lblPatternFile.Text = "Muster der gewählten Übergangsdatei";
            lblPatternFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblPatternFile.Name = "lblPatternFile";
            // 
            // splitItems
            // 
            splitItems.Panel1.Controls.Add(gridItemRules);
            splitItems.Panel2.Controls.Add(gridItemGroups);
            splitItems.Panel2.Controls.Add(lblItemFile);
            splitItems.Dock = System.Windows.Forms.DockStyle.Fill;
            splitItems.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitItems.Size = new System.Drawing.Size(1170, 634);
            splitItems.SplitterDistance = 220;
            splitItems.Name = "splitItems";
            // 
            // gridItemRules
            // 
            gridItemRules.Dock = System.Windows.Forms.DockStyle.Fill;
            gridItemRules.AllowUserToAddRows = true;
            gridItemRules.AllowUserToDeleteRows = true;
            gridItemRules.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridItemRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridItemRules.RowHeadersWidth = 28;
            gridItemRules.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridItemRules.Name = "gridItemRules";
            gridItemRules.DataError += Grid_DataError;
            // 
            // gridItemGroups
            // 
            gridItemGroups.Dock = System.Windows.Forms.DockStyle.Fill;
            gridItemGroups.AllowUserToAddRows = true;
            gridItemGroups.AllowUserToDeleteRows = true;
            gridItemGroups.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridItemGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridItemGroups.RowHeadersWidth = 28;
            gridItemGroups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridItemGroups.Name = "gridItemGroups";
            gridItemGroups.DataError += Grid_DataError;
            // 
            // lblItemFile
            // 
            lblItemFile.Dock = System.Windows.Forms.DockStyle.Top;
            lblItemFile.Size = new System.Drawing.Size(400, 22);
            lblItemFile.Text = "Item-Gruppen der gewählten Datei (Statics: id z hue dx dy; …)";
            lblItemFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblItemFile.Name = "lblItemFile";
            // 
            // gridUneven
            // 
            gridUneven.Dock = System.Windows.Forms.DockStyle.Fill;
            gridUneven.AllowUserToAddRows = true;
            gridUneven.AllowUserToDeleteRows = true;
            gridUneven.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridUneven.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridUneven.RowHeadersWidth = 28;
            gridUneven.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridUneven.Name = "gridUneven";
            gridUneven.DataError += Grid_DataError;
            // 
            // gridFixes
            // 
            gridFixes.Dock = System.Windows.Forms.DockStyle.Fill;
            gridFixes.AllowUserToAddRows = true;
            gridFixes.AllowUserToDeleteRows = true;
            gridFixes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridFixes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridFixes.RowHeadersWidth = 28;
            gridFixes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridFixes.Name = "gridFixes";
            gridFixes.DataError += Grid_DataError;
            // 
            // gridAliases
            // 
            gridAliases.Dock = System.Windows.Forms.DockStyle.Fill;
            gridAliases.AllowUserToAddRows = true;
            gridAliases.AllowUserToDeleteRows = true;
            gridAliases.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            gridAliases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridAliases.RowHeadersWidth = 28;
            gridAliases.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.RowHeaderSelect;
            gridAliases.Name = "gridAliases";
            gridAliases.DataError += Grid_DataError;
            // 
            // txtRuleWarnings
            // 
            txtRuleWarnings.Dock = System.Windows.Forms.DockStyle.Fill;
            txtRuleWarnings.Multiline = true;
            txtRuleWarnings.ReadOnly = true;
            txtRuleWarnings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtRuleWarnings.WordWrap = false;
            txtRuleWarnings.Name = "txtRuleWarnings";
            // 
            // splitBuild
            // 
            splitBuild.Panel1.Controls.Add(propBuild);
            splitBuild.Panel1.Controls.Add(flowBuildButtons);
            splitBuild.Panel1.Controls.Add(grpM2B);
            splitBuild.Panel2.Controls.Add(txtBuildLog);
            splitBuild.Dock = System.Windows.Forms.DockStyle.Fill;
            splitBuild.Orientation = System.Windows.Forms.Orientation.Vertical;
            splitBuild.Size = new System.Drawing.Size(1170, 634);
            splitBuild.SplitterDistance = 470;
            splitBuild.Name = "splitBuild";
            // 
            // propBuild
            // 
            propBuild.Dock = System.Windows.Forms.DockStyle.Fill;
            propBuild.ToolbarVisible = true;
            propBuild.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propBuild.Name = "propBuild";
            // 
            // flowBuildButtons
            // 
            flowBuildButtons.Controls.Add(btnBuildCurrent);
            flowBuildButtons.Controls.Add(btnBuildFromFile);
            flowBuildButtons.Controls.Add(btnOpenOutput);
            flowBuildButtons.Controls.Add(btnBuildCancel);
            flowBuildButtons.Controls.Add(btnCheckMap);
            flowBuildButtons.Controls.Add(btnFixMap);
            flowBuildButtons.Controls.Add(btnOpenLog);
            flowBuildButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowBuildButtons.Size = new System.Drawing.Size(200, 100);
            flowBuildButtons.Padding = new System.Windows.Forms.Padding(3);
            flowBuildButtons.Name = "flowBuildButtons";
            // 
            // btnBuildCurrent
            // 
            btnBuildCurrent.Text = "Aktuelle Karte → MUL / UOP";
            btnBuildCurrent.UseVisualStyleBackColor = true;
            btnBuildCurrent.AutoSize = true;
            btnBuildCurrent.Margin = new System.Windows.Forms.Padding(3);
            btnBuildCurrent.Name = "btnBuildCurrent";
            btnBuildCurrent.Click += BtnBuildCurrent_Click;
            // 
            // btnBuildFromFile
            // 
            btnBuildFromFile.Text = "BMP-Datei → MUL / UOP…";
            btnBuildFromFile.UseVisualStyleBackColor = true;
            btnBuildFromFile.AutoSize = true;
            btnBuildFromFile.Margin = new System.Windows.Forms.Padding(3);
            btnBuildFromFile.Name = "btnBuildFromFile";
            btnBuildFromFile.Click += BtnBuildFromFile_Click;
            // 
            // btnOpenOutput
            // 
            btnOpenOutput.Text = "Ausgabe-Ordner öffnen";
            btnOpenOutput.UseVisualStyleBackColor = true;
            btnOpenOutput.AutoSize = true;
            btnOpenOutput.Margin = new System.Windows.Forms.Padding(3);
            btnOpenOutput.Name = "btnOpenOutput";
            btnOpenOutput.Click += BtnOpenOutput_Click;
            // 
            // btnBuildCancel
            // 
            btnBuildCancel.Text = "Abbrechen";
            btnBuildCancel.UseVisualStyleBackColor = true;
            btnBuildCancel.AutoSize = true;
            btnBuildCancel.Margin = new System.Windows.Forms.Padding(3);
            btnBuildCancel.Name = "btnBuildCancel";
            btnBuildCancel.Click += LblCancel_Click;
            // 
            // btnCheckMap
            // 
            btnCheckMap.Text = "Karte prüfen (Log)";
            btnCheckMap.UseVisualStyleBackColor = true;
            btnCheckMap.AutoSize = true;
            btnCheckMap.Margin = new System.Windows.Forms.Padding(3);
            btnCheckMap.Name = "btnCheckMap";
            btnCheckMap.Click += BtnCheckMap_Click;
            // 
            // btnFixMap
            // 
            btnFixMap.Text = "Karte reparieren";
            btnFixMap.UseVisualStyleBackColor = true;
            btnFixMap.AutoSize = true;
            btnFixMap.Margin = new System.Windows.Forms.Padding(3);
            btnFixMap.Name = "btnFixMap";
            btnFixMap.Click += BtnFixMap_Click;
            // 
            // btnOpenLog
            // 
            btnOpenLog.Text = "Log öffnen";
            btnOpenLog.UseVisualStyleBackColor = true;
            btnOpenLog.AutoSize = true;
            btnOpenLog.Margin = new System.Windows.Forms.Padding(3);
            btnOpenLog.Name = "btnOpenLog";
            btnOpenLog.Click += BtnOpenLog_Click;
            // 
            // grpM2B
            // 
            grpM2B.Controls.Add(lblM2BFile);
            grpM2B.Controls.Add(txtM2BFile);
            grpM2B.Controls.Add(btnM2BBrowse);
            grpM2B.Controls.Add(lblM2BSize);
            grpM2B.Controls.Add(cmbM2BSize);
            grpM2B.Controls.Add(numM2BWidth);
            grpM2B.Controls.Add(lblM2BX);
            grpM2B.Controls.Add(numM2BHeight);
            grpM2B.Controls.Add(lblM2BMapNo);
            grpM2B.Controls.Add(numM2BMapNo);
            grpM2B.Controls.Add(btnM2BConvert);
            grpM2B.Dock = System.Windows.Forms.DockStyle.Bottom;
            grpM2B.Size = new System.Drawing.Size(470, 132);
            grpM2B.Text = "MUL / UOP → BMP (bestehende Karte einlesen, wie Dragon_m2b)";
            grpM2B.Name = "grpM2B";
            // 
            // lblM2BFile
            // 
            lblM2BFile.AutoSize = true;
            lblM2BFile.Location = new System.Drawing.Point(8, 26);
            lblM2BFile.Text = "Datei:";
            lblM2BFile.Name = "lblM2BFile";
            // 
            // txtM2BFile
            // 
            txtM2BFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtM2BFile.Location = new System.Drawing.Point(60, 22);
            txtM2BFile.Size = new System.Drawing.Size(330, 23);
            txtM2BFile.Name = "txtM2BFile";
            // 
            // btnM2BBrowse
            // 
            btnM2BBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnM2BBrowse.Location = new System.Drawing.Point(396, 21);
            btnM2BBrowse.Size = new System.Drawing.Size(66, 25);
            btnM2BBrowse.Text = "…";
            btnM2BBrowse.UseVisualStyleBackColor = true;
            btnM2BBrowse.Name = "btnM2BBrowse";
            btnM2BBrowse.Click += BtnM2BBrowse_Click;
            // 
            // lblM2BSize
            // 
            lblM2BSize.AutoSize = true;
            lblM2BSize.Location = new System.Drawing.Point(8, 57);
            lblM2BSize.Text = "Größe:";
            lblM2BSize.Name = "lblM2BSize";
            // 
            // cmbM2BSize
            // 
            cmbM2BSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbM2BSize.Location = new System.Drawing.Point(60, 53);
            cmbM2BSize.Size = new System.Drawing.Size(190, 23);
            cmbM2BSize.Name = "cmbM2BSize";
            cmbM2BSize.SelectedIndexChanged += CmbM2BSize_SelectedIndexChanged;
            // 
            // numM2BWidth
            // 
            numM2BWidth.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numM2BWidth.Location = new System.Drawing.Point(258, 53);
            numM2BWidth.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
            numM2BWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numM2BWidth.Size = new System.Drawing.Size(64, 23);
            numM2BWidth.Value = new decimal(new int[] { 7168, 0, 0, 0 });
            numM2BWidth.Name = "numM2BWidth";
            // 
            // lblM2BX
            // 
            lblM2BX.AutoSize = true;
            lblM2BX.Location = new System.Drawing.Point(324, 57);
            lblM2BX.Text = "×";
            lblM2BX.Name = "lblM2BX";
            // 
            // numM2BHeight
            // 
            numM2BHeight.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            numM2BHeight.Location = new System.Drawing.Point(340, 53);
            numM2BHeight.Maximum = new decimal(new int[] { 16384, 0, 0, 0 });
            numM2BHeight.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numM2BHeight.Size = new System.Drawing.Size(64, 23);
            numM2BHeight.Value = new decimal(new int[] { 4096, 0, 0, 0 });
            numM2BHeight.Name = "numM2BHeight";
            // 
            // lblM2BMapNo
            // 
            lblM2BMapNo.AutoSize = true;
            lblM2BMapNo.Location = new System.Drawing.Point(8, 92);
            lblM2BMapNo.Text = "Karte # (UOP):";
            lblM2BMapNo.Name = "lblM2BMapNo";
            // 
            // numM2BMapNo
            // 
            numM2BMapNo.Location = new System.Drawing.Point(100, 88);
            numM2BMapNo.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
            numM2BMapNo.Size = new System.Drawing.Size(44, 23);
            numM2BMapNo.Name = "numM2BMapNo";
            // 
            // btnM2BConvert
            // 
            btnM2BConvert.Location = new System.Drawing.Point(258, 86);
            btnM2BConvert.Size = new System.Drawing.Size(204, 27);
            btnM2BConvert.Text = "Einlesen → Editor";
            btnM2BConvert.UseVisualStyleBackColor = true;
            btnM2BConvert.Name = "btnM2BConvert";
            btnM2BConvert.Click += BtnM2BConvert_Click;
            // 
            // txtBuildLog
            // 
            txtBuildLog.Dock = System.Windows.Forms.DockStyle.Fill;
            txtBuildLog.Font = new System.Drawing.Font("Consolas", 9F);
            txtBuildLog.Multiline = true;
            txtBuildLog.ReadOnly = true;
            txtBuildLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtBuildLog.WordWrap = false;
            txtBuildLog.Name = "txtBuildLog";
            // 
            // propSettings
            // 
            propSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            propSettings.ToolbarVisible = true;
            propSettings.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propSettings.Name = "propSettings";
            // 
            // flowSettingsButtons
            // 
            flowSettingsButtons.Controls.Add(btnSettingsSave);
            flowSettingsButtons.Controls.Add(btnSettingsApply);
            flowSettingsButtons.Controls.Add(btnOpenProjectFolder);
            flowSettingsButtons.Controls.Add(btnOpenOutputFolder);
            flowSettingsButtons.Controls.Add(btnOpenScriptFolder);
            flowSettingsButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowSettingsButtons.Size = new System.Drawing.Size(200, 40);
            flowSettingsButtons.Padding = new System.Windows.Forms.Padding(3);
            flowSettingsButtons.Name = "flowSettingsButtons";
            // 
            // btnSettingsSave
            // 
            btnSettingsSave.Text = "Speichern";
            btnSettingsSave.UseVisualStyleBackColor = true;
            btnSettingsSave.AutoSize = true;
            btnSettingsSave.Margin = new System.Windows.Forms.Padding(3);
            btnSettingsSave.Name = "btnSettingsSave";
            btnSettingsSave.Click += BtnSettingsSave_Click;
            // 
            // btnSettingsApply
            // 
            btnSettingsApply.Text = "Speichern + Regeln neu laden";
            btnSettingsApply.UseVisualStyleBackColor = true;
            btnSettingsApply.AutoSize = true;
            btnSettingsApply.Margin = new System.Windows.Forms.Padding(3);
            btnSettingsApply.Name = "btnSettingsApply";
            btnSettingsApply.Click += BtnSettingsApply_Click;
            // 
            // btnOpenProjectFolder
            // 
            btnOpenProjectFolder.Text = "Projekt-Ordner öffnen";
            btnOpenProjectFolder.UseVisualStyleBackColor = true;
            btnOpenProjectFolder.AutoSize = true;
            btnOpenProjectFolder.Margin = new System.Windows.Forms.Padding(3);
            btnOpenProjectFolder.Name = "btnOpenProjectFolder";
            btnOpenProjectFolder.Click += BtnOpenProjectFolder_Click;
            // 
            // btnOpenOutputFolder
            // 
            btnOpenOutputFolder.Text = "Ausgabe-Ordner öffnen";
            btnOpenOutputFolder.UseVisualStyleBackColor = true;
            btnOpenOutputFolder.AutoSize = true;
            btnOpenOutputFolder.Margin = new System.Windows.Forms.Padding(3);
            btnOpenOutputFolder.Name = "btnOpenOutputFolder";
            btnOpenOutputFolder.Click += BtnOpenOutput_Click;
            // 
            // btnOpenScriptFolder
            // 
            btnOpenScriptFolder.Text = "Skript-Ordner öffnen";
            btnOpenScriptFolder.UseVisualStyleBackColor = true;
            btnOpenScriptFolder.AutoSize = true;
            btnOpenScriptFolder.Margin = new System.Windows.Forms.Padding(3);
            btnOpenScriptFolder.Name = "btnOpenScriptFolder";
            btnOpenScriptFolder.Click += BtnOpenScriptFolder_Click;
            // 
            // txtHelp
            // 
            txtHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            txtHelp.Font = new System.Drawing.Font("Consolas", 9.75F);
            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtHelp.BackColor = System.Drawing.SystemColors.Window;
            txtHelp.Name = "txtHelp";
            // 
            // panelHelpTop
            // 
            panelHelpTop.Controls.Add(lblHelpLanguage);
            panelHelpTop.Controls.Add(cmbHelpLanguage);
            panelHelpTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelHelpTop.Size = new System.Drawing.Size(800, 32);
            panelHelpTop.Name = "panelHelpTop";
            // 
            // lblHelpLanguage
            // 
            lblHelpLanguage.AutoSize = true;
            lblHelpLanguage.Location = new System.Drawing.Point(6, 8);
            lblHelpLanguage.Text = "Sprache / Language:";
            lblHelpLanguage.Name = "lblHelpLanguage";
            // 
            // cmbHelpLanguage
            // 
            cmbHelpLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbHelpLanguage.Location = new System.Drawing.Point(130, 4);
            cmbHelpLanguage.Size = new System.Drawing.Size(140, 23);
            cmbHelpLanguage.Name = "cmbHelpLanguage";
            cmbHelpLanguage.SelectedIndexChanged += CmbHelpLanguage_SelectedIndexChanged;
            // 
            // toolTip
            // 
            // 
            // timerThumbnail
            // 
            timerThumbnail.Interval = 1500;
            timerThumbnail.Tick += TimerThumbnail_Tick;
            // 
            // DragonMapStudioForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1184, 761);
            Controls.Add(tabMain);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            KeyPreview = true;
            MainMenuStrip = menuStrip;
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "DragonMapStudioForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Dragon Map Studio";
            FormClosing += DragonMapStudioForm_FormClosing;
            Load += DragonMapStudioForm_Load;
            KeyDown += DragonMapStudioForm_KeyDown;
            ((System.ComponentModel.ISupportInitialize)numColorTableZoom).EndInit();
            ((System.ComponentModel.ISupportInitialize)picTilePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiY).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCgiItem).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiZOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCgiLandArt).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCgiLandTex).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiStep).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiTarget).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCgiMulti).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiMultiId).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCgiCliff).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridBiomes).EndInit();
            ((System.ComponentModel.ISupportInitialize)picGenPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridGroups).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPaletteEntries).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridTransitions).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridPatterns).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridItemRules).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridItemGroups).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridUneven).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridFixes).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridAliases).EndInit();
            ((System.ComponentModel.ISupportInitialize)numM2BWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numM2BHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numM2BMapNo).EndInit();
            panelHelpTop.ResumeLayout(false);
            panelHelpTop.PerformLayout();
            flowSettingsButtons.ResumeLayout(false);
            flowSettingsButtons.PerformLayout();
            grpM2B.ResumeLayout(false);
            grpM2B.PerformLayout();
            flowBuildButtons.ResumeLayout(false);
            flowBuildButtons.PerformLayout();
            tabRuleWarnings.ResumeLayout(false);
            tabRuleWarnings.PerformLayout();
            tabRuleAliases.ResumeLayout(false);
            tabRuleAliases.PerformLayout();
            tabRuleFixes.ResumeLayout(false);
            tabRuleFixes.PerformLayout();
            tabRuleUneven.ResumeLayout(false);
            tabRuleUneven.PerformLayout();
            tabRuleItems.ResumeLayout(false);
            tabRuleItems.PerformLayout();
            tabRuleTransitions.ResumeLayout(false);
            tabRuleTransitions.PerformLayout();
            tabRulePalette.ResumeLayout(false);
            tabRulePalette.PerformLayout();
            tabRuleGroups.ResumeLayout(false);
            tabRuleGroups.PerformLayout();
            panelRulesTop.ResumeLayout(false);
            panelRulesTop.PerformLayout();
            tabRulesInner.ResumeLayout(false);
            flowOutlineButtons.ResumeLayout(false);
            flowOutlineButtons.PerformLayout();
            flowBiomeButtons.ResumeLayout(false);
            flowBiomeButtons.PerformLayout();
            flowGenButtons.ResumeLayout(false);
            flowGenButtons.PerformLayout();
            tabOutline.ResumeLayout(false);
            tabOutline.PerformLayout();
            tabGenBiomes.ResumeLayout(false);
            tabGenBiomes.PerformLayout();
            tabGenSettings.ResumeLayout(false);
            tabGenSettings.PerformLayout();
            tabGenOptions.ResumeLayout(false);
            toolStripCgi.ResumeLayout(false);
            toolStripCgi.PerformLayout();
            toolStripCgiLand.ResumeLayout(false);
            toolStripCgiLand.PerformLayout();
            panelCgiProblemsTop.ResumeLayout(false);
            panelCgiProblemsTop.PerformLayout();
            panelCgiMultiTop.ResumeLayout(false);
            panelCgiMultiTop.PerformLayout();
            panelCgiLand.ResumeLayout(false);
            panelCgiLand.PerformLayout();
            panelCgiHueSwatch.ResumeLayout(false);
            panelCgiHueSwatch.PerformLayout();
            panelCgiItem.ResumeLayout(false);
            panelCgiItem.PerformLayout();
            tabCgiProblems.ResumeLayout(false);
            tabCgiProblems.PerformLayout();
            tabCgiSelection.ResumeLayout(false);
            tabCgiSelection.PerformLayout();
            tabCgiLand.ResumeLayout(false);
            tabCgiLand.PerformLayout();
            tabCgiMulti.ResumeLayout(false);
            tabCgiMulti.PerformLayout();
            tabCgiItems.ResumeLayout(false);
            tabCgiItems.PerformLayout();
            grpCgiGoto.ResumeLayout(false);
            grpCgiGoto.PerformLayout();
            tabCgiSide.ResumeLayout(false);
            toolStripEditor.ResumeLayout(false);
            toolStripEditor.PerformLayout();
            panelSecondarySwatch.ResumeLayout(false);
            panelSecondarySwatch.PerformLayout();
            panelPrimarySwatch.ResumeLayout(false);
            panelPrimarySwatch.PerformLayout();
            panelBrushColors.ResumeLayout(false);
            panelBrushColors.PerformLayout();
            flowLibraryButtons.ResumeLayout(false);
            flowLibraryButtons.PerformLayout();
            grpLibrary.ResumeLayout(false);
            grpLibrary.PerformLayout();
            grpPalette.ResumeLayout(false);
            grpPalette.PerformLayout();
            flowColorButtons.ResumeLayout(false);
            flowColorButtons.PerformLayout();
            panelColorSwatch.ResumeLayout(false);
            panelColorSwatch.PerformLayout();
            panelColorInfo.ResumeLayout(false);
            panelColorInfo.PerformLayout();
            grpColorInfo.ResumeLayout(false);
            grpColorInfo.PerformLayout();
            panelColorTableTools.ResumeLayout(false);
            panelColorTableTools.PerformLayout();
            panelColorTableScroll.ResumeLayout(false);
            panelColorTableScroll.PerformLayout();
            tabHelp.ResumeLayout(false);
            tabHelp.PerformLayout();
            tabSettings.ResumeLayout(false);
            tabSettings.PerformLayout();
            tabBuild.ResumeLayout(false);
            tabBuild.PerformLayout();
            tabRules.ResumeLayout(false);
            tabRules.PerformLayout();
            tabGenerator.ResumeLayout(false);
            tabGenerator.PerformLayout();
            tabCgi.ResumeLayout(false);
            tabCgi.PerformLayout();
            tabEditor.ResumeLayout(false);
            tabEditor.PerformLayout();
            tabColors.ResumeLayout(false);
            tabColors.PerformLayout();
            tabMain.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            splitBuild.Panel1.ResumeLayout(false);
            splitBuild.Panel1.PerformLayout();
            splitBuild.Panel2.ResumeLayout(false);
            splitBuild.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitBuild).EndInit();
            splitBuild.ResumeLayout(false);
            splitItems.Panel1.ResumeLayout(false);
            splitItems.Panel1.PerformLayout();
            splitItems.Panel2.ResumeLayout(false);
            splitItems.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitItems).EndInit();
            splitItems.ResumeLayout(false);
            splitTransitions.Panel1.ResumeLayout(false);
            splitTransitions.Panel1.PerformLayout();
            splitTransitions.Panel2.ResumeLayout(false);
            splitTransitions.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitTransitions).EndInit();
            splitTransitions.ResumeLayout(false);
            splitGenerator.Panel1.ResumeLayout(false);
            splitGenerator.Panel1.PerformLayout();
            splitGenerator.Panel2.ResumeLayout(false);
            splitGenerator.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitGenerator).EndInit();
            splitGenerator.ResumeLayout(false);
            splitCgi.Panel1.ResumeLayout(false);
            splitCgi.Panel1.PerformLayout();
            splitCgi.Panel2.ResumeLayout(false);
            splitCgi.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitCgi).EndInit();
            splitCgi.ResumeLayout(false);
            splitEditor.Panel1.ResumeLayout(false);
            splitEditor.Panel1.PerformLayout();
            splitEditor.Panel2.ResumeLayout(false);
            splitEditor.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitEditor).EndInit();
            splitEditor.ResumeLayout(false);
            splitColorRight.Panel1.ResumeLayout(false);
            splitColorRight.Panel1.PerformLayout();
            splitColorRight.Panel2.ResumeLayout(false);
            splitColorRight.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitColorRight).EndInit();
            splitColorRight.ResumeLayout(false);
            splitColors.Panel1.ResumeLayout(false);
            splitColors.Panel1.PerformLayout();
            splitColors.Panel2.ResumeLayout(false);
            splitColors.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitColors).EndInit();
            splitColors.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuNewMap;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenMap;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveMap;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveMapAs;
        private System.Windows.Forms.ToolStripSeparator mnuFileSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuExportPng;
        private System.Windows.Forms.ToolStripMenuItem mnuImportMul;
        private System.Windows.Forms.ToolStripSeparator mnuFileSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuRecent;
        private System.Windows.Forms.ToolStripSeparator mnuFileSep3;
        private System.Windows.Forms.ToolStripMenuItem mnuClose;
        private System.Windows.Forms.ToolStripMenuItem mnuEdit;
        private System.Windows.Forms.ToolStripMenuItem mnuUndo;
        private System.Windows.Forms.ToolStripMenuItem mnuRedo;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuCut;
        private System.Windows.Forms.ToolStripMenuItem mnuPaste;
        private System.Windows.Forms.ToolStripMenuItem mnuDeleteSelection;
        private System.Windows.Forms.ToolStripMenuItem mnuFillSelection;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectAll;
        private System.Windows.Forms.ToolStripSeparator mnuEditSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuCheckMap;
        private System.Windows.Forms.ToolStripMenuItem mnuFixMap;
        private System.Windows.Forms.ToolStripMenuItem mnuRules;
        private System.Windows.Forms.ToolStripMenuItem mnuLoadScripts;
        private System.Windows.Forms.ToolStripMenuItem mnuLoadJson;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveJson;
        private System.Windows.Forms.ToolStripMenuItem mnuExportScripts;
        private System.Windows.Forms.ToolStripSeparator mnuRulesSep1;
        private System.Windows.Forms.ToolStripMenuItem mnuImportPalette;
        private System.Windows.Forms.ToolStripMenuItem mnuExportPalette;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateColorTable;
        private System.Windows.Forms.ToolStripSeparator mnuRulesSep2;
        private System.Windows.Forms.ToolStripMenuItem mnuValidate;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.ToolStripMenuItem mnuHelpTopics;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenSettingsFolder;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblPosition;
        private System.Windows.Forms.ToolStripStatusLabel lblZoom;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel lblCancel;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabColors;
        private System.Windows.Forms.TabPage tabEditor;
        private System.Windows.Forms.TabPage tabCgi;
        private System.Windows.Forms.TabPage tabGenerator;
        private System.Windows.Forms.TabPage tabRules;
        private System.Windows.Forms.TabPage tabBuild;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabHelp;
        private System.Windows.Forms.SplitContainer splitColors;
        private System.Windows.Forms.Panel panelColorTableScroll;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.ColorTableView colorTableView;
        private System.Windows.Forms.FlowLayoutPanel panelColorTableTools;
        private System.Windows.Forms.Label lblCtZoom;
        private System.Windows.Forms.NumericUpDown numColorTableZoom;
        private System.Windows.Forms.Button btnLoadColorTable;
        private System.Windows.Forms.Button btnRenderColorTable;
        private System.Windows.Forms.Label lblCopyFormat;
        private System.Windows.Forms.ComboBox cmbCopyFormat;
        private System.Windows.Forms.SplitContainer splitColorRight;
        private System.Windows.Forms.GroupBox grpColorInfo;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.PaletteGridView paletteGrid;
        private System.Windows.Forms.Panel panelColorInfo;
        private System.Windows.Forms.Panel panelColorSwatch;
        private System.Windows.Forms.Label lblColorAddress;
        private System.Windows.Forms.Label lblColorDetails;
        private System.Windows.Forms.PictureBox picTilePreview;
        private System.Windows.Forms.FlowLayoutPanel flowColorButtons;
        private System.Windows.Forms.Button btnCopyColor;
        private System.Windows.Forms.Button btnUseAsPrimary;
        private System.Windows.Forms.Button btnUseAsSecondary;
        private System.Windows.Forms.Button btnUseAsLand;
        private System.Windows.Forms.Button btnUseAsWater;
        private System.Windows.Forms.Button btnNewColor;
        private System.Windows.Forms.Button btnEditRgb;
        private System.Windows.Forms.PropertyGrid propColorEntry;
        private System.Windows.Forms.SplitContainer splitEditor;
        private System.Windows.Forms.GroupBox grpPalette;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapNavigator mapNavigator;
        private System.Windows.Forms.GroupBox grpLibrary;
        private System.Windows.Forms.ListView lstMaps;
        private System.Windows.Forms.ColumnHeader colMapName;
        private System.Windows.Forms.ColumnHeader colMapSize;
        private System.Windows.Forms.ColumnHeader colMapDate;
        private System.Windows.Forms.FlowLayoutPanel flowLibraryButtons;
        private System.Windows.Forms.Button btnLibOpen;
        private System.Windows.Forms.Button btnLibNew;
        private System.Windows.Forms.Button btnLibDuplicate;
        private System.Windows.Forms.Button btnLibRename;
        private System.Windows.Forms.Button btnLibDelete;
        private System.Windows.Forms.Button btnLibRefresh;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.PaletteGridView editorPalette;
        private System.Windows.Forms.Panel panelBrushColors;
        private System.Windows.Forms.Panel panelPrimarySwatch;
        private System.Windows.Forms.Label lblPrimary;
        private System.Windows.Forms.Panel panelSecondarySwatch;
        private System.Windows.Forms.Label lblSecondary;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapCanvas mapCanvas;
        private System.Windows.Forms.ToolStrip toolStripEditor;
        private System.Windows.Forms.ToolStripButton tsbBrush;
        private System.Windows.Forms.ToolStripButton tsbEraser;
        private System.Windows.Forms.ToolStripButton tsbSpray;
        private System.Windows.Forms.ToolStripButton tsbLine;
        private System.Windows.Forms.ToolStripButton tsbRect;
        private System.Windows.Forms.ToolStripButton tsbRectFill;
        private System.Windows.Forms.ToolStripButton tsbEllipse;
        private System.Windows.Forms.ToolStripButton tsbEllipseFill;
        private System.Windows.Forms.ToolStripButton tsbFill;
        private System.Windows.Forms.ToolStripButton tsbPicker;
        private System.Windows.Forms.ToolStripButton tsbRaise;
        private System.Windows.Forms.ToolStripButton tsbLower;
        private System.Windows.Forms.ToolStripButton tsbSelect;
        private System.Windows.Forms.ToolStripButton tsbPan;
        private System.Windows.Forms.ToolStripSeparator tssEd1;
        private System.Windows.Forms.ToolStripLabel tslBrushSize;
        private System.Windows.Forms.ToolStripComboBox tscBrushSize;
        private System.Windows.Forms.ToolStripButton tsbSquare;
        private System.Windows.Forms.ToolStripSeparator tssEd2;
        private System.Windows.Forms.ToolStripButton tsbZoomIn;
        private System.Windows.Forms.ToolStripButton tsbZoomOut;
        private System.Windows.Forms.ToolStripButton tsbZoomFit;
        private System.Windows.Forms.ToolStripButton tsbZoom100;
        private System.Windows.Forms.ToolStripButton tsbGrid;
        private System.Windows.Forms.ToolStripButton tsbHeights;
        private System.Windows.Forms.ToolStripSeparator tssEd3;
        private System.Windows.Forms.ToolStripButton tsbUndo;
        private System.Windows.Forms.ToolStripButton tsbRedo;
        private System.Windows.Forms.ToolStripSeparator tssEd4;
        private System.Windows.Forms.ToolStripButton tsbCgiHere;
        private System.Windows.Forms.SplitContainer splitCgi;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.IsoMapView isoView;
        private System.Windows.Forms.TabControl tabCgiSide;
        private System.Windows.Forms.GroupBox grpCgiGoto;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.MapNavigator cgiNavigator;
        private System.Windows.Forms.Label lblCgiX;
        private System.Windows.Forms.NumericUpDown numCgiX;
        private System.Windows.Forms.Label lblCgiY;
        private System.Windows.Forms.NumericUpDown numCgiY;
        private System.Windows.Forms.Button btnCgiGoto;
        private System.Windows.Forms.Button btnCgiToSelection;
        private System.Windows.Forms.TabPage tabCgiItems;
        private System.Windows.Forms.TabPage tabCgiMulti;
        private System.Windows.Forms.TabPage tabCgiLand;
        private System.Windows.Forms.TabPage tabCgiSelection;
        private System.Windows.Forms.TabPage tabCgiProblems;
        private System.Windows.Forms.ListView lstCgiItems;
        private System.Windows.Forms.ColumnHeader colCgiItemId;
        private System.Windows.Forms.ColumnHeader colCgiItemName;
        private System.Windows.Forms.TextBox txtCgiFilter;
        private System.Windows.Forms.Panel panelCgiItem;
        private System.Windows.Forms.PictureBox picCgiItem;
        private System.Windows.Forms.Label lblCgiItemId;
        private System.Windows.Forms.TextBox txtCgiItemId;
        private System.Windows.Forms.Label lblCgiHue;
        private System.Windows.Forms.NumericUpDown numCgiHue;
        private System.Windows.Forms.Panel panelCgiHueSwatch;
        private System.Windows.Forms.Label lblCgiZ;
        private System.Windows.Forms.NumericUpDown numCgiZOffset;
        private System.Windows.Forms.CheckBox chkCgiStack;
        private System.Windows.Forms.Label lblCgiItemInfo;
        private System.Windows.Forms.ListView lstCgiLand;
        private System.Windows.Forms.ColumnHeader colCgiLandId;
        private System.Windows.Forms.ColumnHeader colCgiLandName;
        private System.Windows.Forms.TextBox txtCgiLandFilter;
        private System.Windows.Forms.Panel panelCgiLand;
        private System.Windows.Forms.PictureBox picCgiLandArt;
        private System.Windows.Forms.PictureBox picCgiLandTex;
        private System.Windows.Forms.Label lblCgiLandId;
        private System.Windows.Forms.TextBox txtCgiLandId;
        private System.Windows.Forms.Label lblCgiRadius;
        private System.Windows.Forms.NumericUpDown numCgiRadius;
        private System.Windows.Forms.Label lblCgiStep;
        private System.Windows.Forms.NumericUpDown numCgiStep;
        private System.Windows.Forms.Label lblCgiTarget;
        private System.Windows.Forms.NumericUpDown numCgiTarget;
        private System.Windows.Forms.Label lblCgiLandInfo;
        private System.Windows.Forms.PictureBox picCgiMulti;
        private System.Windows.Forms.Panel panelCgiMultiTop;
        private System.Windows.Forms.Label lblCgiMultiId;
        private System.Windows.Forms.NumericUpDown numCgiMultiId;
        private System.Windows.Forms.CheckBox chkCgiMultiInvisible;
        private System.Windows.Forms.Label lblCgiMultiInfo;
        private System.Windows.Forms.PropertyGrid propCgiSelection;
        private System.Windows.Forms.Label lblCgiSelInfo;
        private System.Windows.Forms.ListBox lstCgiProblems;
        private System.Windows.Forms.Panel panelCgiProblemsTop;
        private System.Windows.Forms.Label lblCgiCliff;
        private System.Windows.Forms.NumericUpDown numCgiCliff;
        private System.Windows.Forms.Button btnCgiScan;
        private System.Windows.Forms.Button btnCgiRemoveDup;
        private System.Windows.Forms.Button btnCgiSaveLog;
        private System.Windows.Forms.ToolStrip toolStripCgiLand;
        private System.Windows.Forms.ToolStripLabel tslCgiLandTools;
        private System.Windows.Forms.ToolStripButton tsbCgiLandSet;
        private System.Windows.Forms.ToolStripButton tsbCgiRaise;
        private System.Windows.Forms.ToolStripButton tsbCgiLower;
        private System.Windows.Forms.ToolStripButton tsbCgiSetHeight;
        private System.Windows.Forms.ToolStripButton tsbCgiSmooth;
        private System.Windows.Forms.ToolStripButton tsbCgiArea;
        private System.Windows.Forms.ToolStrip toolStripCgi;
        private System.Windows.Forms.ToolStripButton tsbCgiBuild;
        private System.Windows.Forms.ToolStripButton tsbCgiLoad;
        private System.Windows.Forms.ToolStripButton tsbCgiSave;
        private System.Windows.Forms.ToolStripButton tsbCgiSaveAs;
        private System.Windows.Forms.ToolStripSeparator tssCgi1;
        private System.Windows.Forms.ToolStripButton tsbCgiSelect;
        private System.Windows.Forms.ToolStripButton tsbCgiPlace;
        private System.Windows.Forms.ToolStripButton tsbCgiMulti;
        private System.Windows.Forms.ToolStripButton tsbCgiDelete;
        private System.Windows.Forms.ToolStripButton tsbCgiHue;
        private System.Windows.Forms.ToolStripSeparator tssCgi2;
        private System.Windows.Forms.ToolStripButton tsbCgiUndo;
        private System.Windows.Forms.ToolStripButton tsbCgiRedo;
        private System.Windows.Forms.ToolStripSeparator tssCgi3;
        private System.Windows.Forms.ToolStripButton tsbCgiZoomIn;
        private System.Windows.Forms.ToolStripButton tsbCgiZoomOut;
        private System.Windows.Forms.ToolStripButton tsbCgiGrid;
        private System.Windows.Forms.ToolStripButton tsbCgiHeights;
        private System.Windows.Forms.ToolStripButton tsbCgiStatics;
        private System.Windows.Forms.ToolStripButton tsbCgiProblems;
        private System.Windows.Forms.ToolStripLabel tslCgiMaxZ;
        private System.Windows.Forms.ToolStripComboBox tscCgiMaxZ;
        private System.Windows.Forms.SplitContainer splitGenerator;
        private System.Windows.Forms.TabControl tabGenOptions;
        private System.Windows.Forms.TabPage tabGenSettings;
        private System.Windows.Forms.TabPage tabGenBiomes;
        private System.Windows.Forms.TabPage tabOutline;
        private System.Windows.Forms.PropertyGrid propGenerator;
        private System.Windows.Forms.FlowLayoutPanel flowGenButtons;
        private System.Windows.Forms.Button btnGenPreview;
        private System.Windows.Forms.Button btnGenNewSeed;
        private System.Windows.Forms.Button btnGenToEditor;
        private System.Windows.Forms.Button btnGenSavePreset;
        private System.Windows.Forms.Button btnGenLoadPreset;
        private System.Windows.Forms.DataGridView gridBiomes;
        private System.Windows.Forms.FlowLayoutPanel flowBiomeButtons;
        private System.Windows.Forms.Button btnBiomesDefault;
        private System.Windows.Forms.Label lblBiomeHint;
        private System.Windows.Forms.PropertyGrid propOutline;
        private System.Windows.Forms.FlowLayoutPanel flowOutlineButtons;
        private System.Windows.Forms.Button btnOutlineLoad;
        private System.Windows.Forms.Button btnOutlinePreview;
        private System.Windows.Forms.Button btnOutlineApply;
        private System.Windows.Forms.Button btnOutlineAsMask;
        private System.Windows.Forms.Button btnMaskFromMap;
        private System.Windows.Forms.Button btnMaskClear;
        private System.Windows.Forms.PictureBox picGenPreview;
        private System.Windows.Forms.Label lblGenInfo;
        private System.Windows.Forms.TabControl tabRulesInner;
        private System.Windows.Forms.FlowLayoutPanel panelRulesTop;
        private System.Windows.Forms.Label lblRuleSetInfo;
        private System.Windows.Forms.Button btnRulesReload;
        private System.Windows.Forms.Button btnRulesSaveJson;
        private System.Windows.Forms.Button btnRulesExport;
        private System.Windows.Forms.Button btnRulesValidate;
        private System.Windows.Forms.TabPage tabRuleGroups;
        private System.Windows.Forms.TabPage tabRulePalette;
        private System.Windows.Forms.TabPage tabRuleTransitions;
        private System.Windows.Forms.TabPage tabRuleItems;
        private System.Windows.Forms.TabPage tabRuleUneven;
        private System.Windows.Forms.TabPage tabRuleFixes;
        private System.Windows.Forms.TabPage tabRuleAliases;
        private System.Windows.Forms.TabPage tabRuleWarnings;
        private System.Windows.Forms.DataGridView gridGroups;
        private System.Windows.Forms.DataGridView gridPaletteEntries;
        private System.Windows.Forms.SplitContainer splitTransitions;
        private System.Windows.Forms.DataGridView gridTransitions;
        private System.Windows.Forms.DataGridView gridPatterns;
        private System.Windows.Forms.Label lblPatternFile;
        private System.Windows.Forms.SplitContainer splitItems;
        private System.Windows.Forms.DataGridView gridItemRules;
        private System.Windows.Forms.DataGridView gridItemGroups;
        private System.Windows.Forms.Label lblItemFile;
        private System.Windows.Forms.DataGridView gridUneven;
        private System.Windows.Forms.DataGridView gridFixes;
        private System.Windows.Forms.DataGridView gridAliases;
        private System.Windows.Forms.TextBox txtRuleWarnings;
        private System.Windows.Forms.SplitContainer splitBuild;
        private System.Windows.Forms.PropertyGrid propBuild;
        private System.Windows.Forms.FlowLayoutPanel flowBuildButtons;
        private System.Windows.Forms.Button btnBuildCurrent;
        private System.Windows.Forms.Button btnBuildFromFile;
        private System.Windows.Forms.Button btnOpenOutput;
        private System.Windows.Forms.Button btnBuildCancel;
        private System.Windows.Forms.Button btnCheckMap;
        private System.Windows.Forms.Button btnFixMap;
        private System.Windows.Forms.Button btnOpenLog;
        private System.Windows.Forms.GroupBox grpM2B;
        private System.Windows.Forms.Label lblM2BFile;
        private System.Windows.Forms.TextBox txtM2BFile;
        private System.Windows.Forms.Button btnM2BBrowse;
        private System.Windows.Forms.Label lblM2BSize;
        private System.Windows.Forms.ComboBox cmbM2BSize;
        private System.Windows.Forms.NumericUpDown numM2BWidth;
        private System.Windows.Forms.Label lblM2BX;
        private System.Windows.Forms.NumericUpDown numM2BHeight;
        private System.Windows.Forms.Label lblM2BMapNo;
        private System.Windows.Forms.NumericUpDown numM2BMapNo;
        private System.Windows.Forms.Button btnM2BConvert;
        private System.Windows.Forms.TextBox txtBuildLog;
        private System.Windows.Forms.PropertyGrid propSettings;
        private System.Windows.Forms.FlowLayoutPanel flowSettingsButtons;
        private System.Windows.Forms.Button btnSettingsSave;
        private System.Windows.Forms.Button btnSettingsApply;
        private System.Windows.Forms.Button btnOpenProjectFolder;
        private System.Windows.Forms.Button btnOpenOutputFolder;
        private System.Windows.Forms.Button btnOpenScriptFolder;
        private System.Windows.Forms.TextBox txtHelp;
        private System.Windows.Forms.Panel panelHelpTop;
        private System.Windows.Forms.Label lblHelpLanguage;
        private System.Windows.Forms.ComboBox cmbHelpLanguage;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer timerThumbnail;
    }
}

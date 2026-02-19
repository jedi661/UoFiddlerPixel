namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class ARTMulIDXCreator
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();

            // Existing tabs
            this.tabPageCreateMuls = new System.Windows.Forms.TabPage();
            this.tabPageReadMuls = new System.Windows.Forms.TabPage();
            this.tabPageTileData = new System.Windows.Forms.TabPage();
            this.tabPageReadOut = new System.Windows.Forms.TabPage();
            this.tabPageTexturen = new System.Windows.Forms.TabPage();
            this.tabPageRadarColor = new System.Windows.Forms.TabPage();
            this.tabPagePalette = new System.Windows.Forms.TabPage();
            this.tabPageAnimation = new System.Windows.Forms.TabPage();
            this.tabPageArtmul = new System.Windows.Forms.TabPage();
            this.tabPageSound = new System.Windows.Forms.TabPage();
            this.tabPageGump = new System.Windows.Forms.TabPage();

            // New tabs
            this.tabPageHues = new System.Windows.Forms.TabPage();
            this.tabPageMap = new System.Windows.Forms.TabPage();
            this.tabPageMulti = new System.Windows.Forms.TabPage();
            this.tabPageSkills = new System.Windows.Forms.TabPage();
            this.tabPageValidator = new System.Windows.Forms.TabPage();
            this.tabPageIdxPatcher = new System.Windows.Forms.TabPage();
            this.tabPageBatch = new System.Windows.Forms.TabPage();
            this.tabPageHexViewer = new System.Windows.Forms.TabPage();

            // ── Tab 1: Create Muls controls ──────────────────────────────────
            this.grpCreateMulsDir = new System.Windows.Forms.GroupBox();
            this.BtFileOrder = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblDirInfo = new System.Windows.Forms.Label();
            this.grpCreateMulsCount = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.lblCountHint = new System.Windows.Forms.Label();
            this.grpCreateMulsButtons = new System.Windows.Forms.GroupBox();
            this.BtCreateARTIDXMul = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Ulong = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_uint = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Int = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Ushort = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Short = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Byte = new System.Windows.Forms.Button();
            this.BtCreateARTIDXMul_Sbyte = new System.Windows.Forms.Button();
            this.lblButtonsNote = new System.Windows.Forms.Label();
            this.grpRename = new System.Windows.Forms.GroupBox();
            this.ComboBoxMuls = new System.Windows.Forms.ComboBox();
            this.lblRenameHint = new System.Windows.Forms.Label();
            this.grpCreateOutput = new System.Windows.Forms.GroupBox();
            this.lbCreatedMul = new System.Windows.Forms.Label();

            // ── Tab 2: Read Muls controls ────────────────────────────────────
            this.grpReadMulsActions = new System.Windows.Forms.GroupBox();
            this.BtnCountEntries = new System.Windows.Forms.Button();
            this.lblEntryCount = new System.Windows.Forms.Label();
            this.grpReadMulsResult = new System.Windows.Forms.GroupBox();
            this.BtnShowInfo = new System.Windows.Forms.Button();
            this.textBoxInfo = new System.Windows.Forms.TextBox();
            this.grpReadSingle = new System.Windows.Forms.GroupBox();
            this.lblIndexHint = new System.Windows.Forms.Label();
            this.textBoxIndex = new System.Windows.Forms.TextBox();
            this.BtnReadArtIdx = new System.Windows.Forms.Button();

            // ── Tab 3: TileData controls ─────────────────────────────────────
            this.grpTileDataDir = new System.Windows.Forms.GroupBox();
            this.btnTileDataBrowse = new System.Windows.Forms.Button();
            this.tbDirTileData = new System.Windows.Forms.TextBox();
            this.grpTileDataConfig = new System.Windows.Forms.GroupBox();
            this.lblLandGroupsLbl = new System.Windows.Forms.Label();
            this.tblandTileGroups = new System.Windows.Forms.TextBox();
            this.lblLandGroupsHint = new System.Windows.Forms.Label();
            this.lblStaticGroupsLbl = new System.Windows.Forms.Label();
            this.tbstaticTileGroups = new System.Windows.Forms.TextBox();
            this.lblStaticGroupsHint = new System.Windows.Forms.Label();
            this.BtCreateTiledata = new System.Windows.Forms.Button();
            this.grpTileDataQuick = new System.Windows.Forms.GroupBox();
            this.BtCreateTiledataEmpty = new System.Windows.Forms.Button();
            this.BtCreateTiledataEmpty2 = new System.Windows.Forms.Button();
            this.BtCreateSimpleTiledata = new System.Windows.Forms.Button();
            this.grpTileDataRead = new System.Windows.Forms.GroupBox();
            this.BtTiledatainfo = new System.Windows.Forms.Button();
            this.BtnCountTileDataEntries = new System.Windows.Forms.Button();
            this.lblTileDataEntryCount = new System.Windows.Forms.Label();
            this.BtReadTileFlags = new System.Windows.Forms.Button();
            this.grpTileDataIndex = new System.Windows.Forms.GroupBox();
            this.lblTiledataIndexHint = new System.Windows.Forms.Label();
            this.textBoxTiledataIndex = new System.Windows.Forms.TextBox();
            this.BtReadIndexTiledata = new System.Windows.Forms.Button();
            this.BtReadLandTile = new System.Windows.Forms.Button();
            this.BtReadStaticTile = new System.Windows.Forms.Button();
            this.BtSelectDirectory = new System.Windows.Forms.Button();
            this.grpTileDataOutput = new System.Windows.Forms.GroupBox();
            this.lbTileDataCreate = new System.Windows.Forms.Label();
            this.checkBoxTileData = new System.Windows.Forms.CheckBox();

            // ── Tab 4: ReadOut controls ──────────────────────────────────────
            this.grpReadOutActions = new System.Windows.Forms.GroupBox();
            this.ButtonReadTileData = new System.Windows.Forms.Button();
            this.ButtonReadLandTileData = new System.Windows.Forms.Button();
            this.ButtonReadStaticTileData = new System.Windows.Forms.Button();
            this.listViewTileData = new System.Windows.Forms.ListView();
            this.lblSelectedEntry = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.grpReadOutInfo = new System.Windows.Forms.GroupBox();
            this.lblReadOutIdxLbl = new System.Windows.Forms.Label();
            this.textBoxTileDataInfo = new System.Windows.Forms.TextBox();

            // ── Tab 5: Texturen controls ─────────────────────────────────────
            this.grpTexConfig = new System.Windows.Forms.GroupBox();
            this.lblTexCountLbl = new System.Windows.Forms.Label();
            this.tbIndexCountTexture = new System.Windows.Forms.TextBox();
            this.lblTexCountHint = new System.Windows.Forms.Label();
            this.checkBoxTexture = new System.Windows.Forms.CheckBox();
            this.grpTexActions = new System.Windows.Forms.GroupBox();
            this.BtCreateTextur = new System.Windows.Forms.Button();
            this.BtCreateIndexes = new System.Windows.Forms.Button();
            this.grpTexOutput = new System.Windows.Forms.GroupBox();
            this.lbTextureCount = new System.Windows.Forms.Label();
            this.tbIndexCount = new System.Windows.Forms.Label();

            // ── Tab 6: RadarColor controls ───────────────────────────────────
            this.grpRadarConfig = new System.Windows.Forms.GroupBox();
            this.lblRadarCountLbl = new System.Windows.Forms.Label();
            this.indexCountTextBox = new System.Windows.Forms.TextBox();
            this.lblRadarCountHint = new System.Windows.Forms.Label();
            this.grpRadarActions = new System.Windows.Forms.GroupBox();
            this.CreateFileButtonRadarColor = new System.Windows.Forms.Button();
            this.grpRadarOutput = new System.Windows.Forms.GroupBox();
            this.lbRadarColor = new System.Windows.Forms.Label();

            // ── Tab 7: Palette controls ──────────────────────────────────────
            this.grpPaletteCreate = new System.Windows.Forms.GroupBox();
            this.BtCreatePalette = new System.Windows.Forms.Button();
            this.lbCreatePalette = new System.Windows.Forms.Label();
            this.BtCreatePaletteFull = new System.Windows.Forms.Button();
            this.lbCreateColorPalette = new System.Windows.Forms.Label();
            this.grpPaletteLoad = new System.Windows.Forms.GroupBox();
            this.LoadPaletteButton = new System.Windows.Forms.Button();
            this.lblPalettePreview = new System.Windows.Forms.Label();
            this.pictureBoxPalette = new System.Windows.Forms.PictureBox();
            this.grpPaletteValues = new System.Windows.Forms.GroupBox();
            this.textBoxRgbValues = new System.Windows.Forms.TextBox();

            // ── Tab 8: Animation controls ────────────────────────────────────
            this.grpAnimSource = new System.Windows.Forms.GroupBox();
            this.tbfilename = new System.Windows.Forms.TextBox();
            this.BtnBrowse = new System.Windows.Forms.Button();
            this.grpAnimOutput = new System.Windows.Forms.GroupBox();
            this.txtOutputDirectory = new System.Windows.Forms.TextBox();
            this.BtnSetOutputDirectory = new System.Windows.Forms.Button();
            this.lblAnimSuffixLbl = new System.Windows.Forms.Label();
            this.txtOutputFilename = new System.Windows.Forms.TextBox();
            this.grpAnimCreature = new System.Windows.Forms.GroupBox();
            this.lblOrigIDHint = new System.Windows.Forms.Label();
            this.txtOrigCreatureID = new System.Windows.Forms.TextBox();
            this.lblHexWarning = new System.Windows.Forms.Label();
            this.lblCopyCountHint = new System.Windows.Forms.Label();
            this.txtNewCreatureID = new System.Windows.Forms.TextBox();
            this.panelCheckbox = new System.Windows.Forms.Panel();
            this.lbCopys = new System.Windows.Forms.Label();
            this.chkLowDetail = new System.Windows.Forms.CheckBox();
            this.chkHighDetail = new System.Windows.Forms.CheckBox();
            this.chkHuman = new System.Windows.Forms.CheckBox();
            this.grpAnimActions = new System.Windows.Forms.GroupBox();
            this.BtnNewAnimIDXFiles = new System.Windows.Forms.Button();
            this.BtnProcessClickOld = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.grpAnimInfo = new System.Windows.Forms.GroupBox();
            this.ReadAnimIdx = new System.Windows.Forms.Button();
            this.btnCountIndices = new System.Windows.Forms.Button();
            this.BtnLoadAnimationMulData = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.grpAnimLog = new System.Windows.Forms.GroupBox();
            this.tbProcessAminidx = new System.Windows.Forms.TextBox();
            this.lblNewIdCount = new System.Windows.Forms.Label();

            // ── Tab 9: Artmul controls ───────────────────────────────────────
            this.grpArtCreate = new System.Windows.Forms.GroupBox();
            this.lblArtCreateHint = new System.Windows.Forms.Label();
            this.BtnCreateArtIdx100K = new System.Windows.Forms.Button();
            this.BtnCreateArtIdx150K = new System.Windows.Forms.Button();
            this.BtnCreateArtIdx200K = new System.Windows.Forms.Button();
            this.BtnCreateArtIdx250K = new System.Windows.Forms.Button();
            this.BtnCreateArtIdx500K = new System.Windows.Forms.Button();
            this.BtnCreateArtIdx = new System.Windows.Forms.Button();
            this.grpArtCustom = new System.Windows.Forms.GroupBox();
            this.lblArtCustomHint = new System.Windows.Forms.Label();
            this.tbxNewIndex = new System.Windows.Forms.TextBox();
            this.Button2 = new System.Windows.Forms.Button();
            this.lblOldVersionHint = new System.Windows.Forms.Label();
            this.BtCreateOldVersionArtidx = new System.Windows.Forms.Button();
            this.grpArtSplit = new System.Windows.Forms.GroupBox();
            this.lblArtSplitHint = new System.Windows.Forms.Label();
            this.lblArtSplitTotalLbl = new System.Windows.Forms.Label();
            this.tbxNewIndex2 = new System.Windows.Forms.TextBox();
            this.lbArtsCount = new System.Windows.Forms.Label();
            this.tbxArtsCount = new System.Windows.Forms.TextBox();
            this.lbLandTilesCount = new System.Windows.Forms.Label();
            this.tbxLandTilesCount = new System.Windows.Forms.TextBox();
            this.Button3 = new System.Windows.Forms.Button();
            this.grpArtRead = new System.Windows.Forms.GroupBox();
            this.lblArtReadHint = new System.Windows.Forms.Label();
            this.ReadArtmul = new System.Windows.Forms.Button();
            this.ReadArtmul2 = new System.Windows.Forms.Button();
            this.lblIndexCount = new System.Windows.Forms.Label();
            this.grpArtLog = new System.Windows.Forms.GroupBox();
            this.infoARTIDXMULID = new System.Windows.Forms.TextBox();

            // ── Tab 10: Sound controls ───────────────────────────────────────
            this.grpSoundConfig = new System.Windows.Forms.GroupBox();
            this.lblSoundCountLbl = new System.Windows.Forms.Label();
            this.SoundIDXMul = new System.Windows.Forms.TextBox();
            this.lblSoundCountHint = new System.Windows.Forms.Label();
            this.grpSoundActions = new System.Windows.Forms.GroupBox();
            this.CreateOrgSoundMul = new System.Windows.Forms.Button();
            this.ReadIndexSize = new System.Windows.Forms.Button();
            this.grpSoundOutput = new System.Windows.Forms.GroupBox();
            this.IndexSizeLabel = new System.Windows.Forms.Label();

            // ── Tab 11: Gump controls ────────────────────────────────────────
            this.grpGumpConfig = new System.Windows.Forms.GroupBox();
            this.lblGumpCountLbl = new System.Windows.Forms.Label();
            this.IndexSizeTextBox = new System.Windows.Forms.TextBox();
            this.lblGumpCountHint = new System.Windows.Forms.Label();
            this.grpGumpActions = new System.Windows.Forms.GroupBox();
            this.CreateGumpButton = new System.Windows.Forms.Button();
            this.ReadGumpButton = new System.Windows.Forms.Button();
            this.grpGumpOutput = new System.Windows.Forms.GroupBox();
            this.gumpLabel = new System.Windows.Forms.Label();

            // ── Tab 12: Hues controls ────────────────────────────────────────
            this.grpHuesActions = new System.Windows.Forms.GroupBox();
            this.BtnCreateHues = new System.Windows.Forms.Button();
            this.BtnReadHues = new System.Windows.Forms.Button();
            this.grpHuesOutput = new System.Windows.Forms.GroupBox();
            this.lblHuesOutput = new System.Windows.Forms.Label();

            // ── Tab 13: Map/Statics controls ─────────────────────────────────
            this.grpMapConfig = new System.Windows.Forms.GroupBox();
            this.lblMapSizeComboLbl = new System.Windows.Forms.Label();
            this.comboMapSize = new System.Windows.Forms.ComboBox();
            this.lblMapWidthLbl = new System.Windows.Forms.Label();
            this.tbMapWidth = new System.Windows.Forms.TextBox();
            this.lblMapHeightLbl = new System.Windows.Forms.Label();
            this.tbMapHeight = new System.Windows.Forms.TextBox();
            this.lblMapIndexLbl = new System.Windows.Forms.Label();
            this.tbMapIndex = new System.Windows.Forms.TextBox();
            this.lblMapSizeInfo = new System.Windows.Forms.Label();
            this.grpMapActions = new System.Windows.Forms.GroupBox();
            this.BtnCreateMap = new System.Windows.Forms.Button();
            this.BtnCreateStatics = new System.Windows.Forms.Button();
            this.BtnCreateMapAndStatics = new System.Windows.Forms.Button();
            this.grpMapOutput = new System.Windows.Forms.GroupBox();
            this.lblMapOutput = new System.Windows.Forms.Label();

            // ── Tab 14: Multi controls ───────────────────────────────────────
            this.grpMultiConfig = new System.Windows.Forms.GroupBox();
            this.lblMultiCountLbl = new System.Windows.Forms.Label();
            this.tbMultiCount = new System.Windows.Forms.TextBox();
            this.lblMultiIndexLbl = new System.Windows.Forms.Label();
            this.tbMultiIndex = new System.Windows.Forms.TextBox();
            this.checkBoxMultiHS = new System.Windows.Forms.CheckBox();
            this.grpMultiActions = new System.Windows.Forms.GroupBox();
            this.BtnCreateMulti = new System.Windows.Forms.Button();
            this.BtnReadMulti = new System.Windows.Forms.Button();
            this.grpMultiOutput = new System.Windows.Forms.GroupBox();
            this.lblMultiOutput = new System.Windows.Forms.Label();

            // ── Tab 15: Skills controls ──────────────────────────────────────
            this.grpSkillsConfig = new System.Windows.Forms.GroupBox();
            this.lblSkillCountLbl = new System.Windows.Forms.Label();
            this.tbSkillCount = new System.Windows.Forms.TextBox();
            this.grpSkillsActions = new System.Windows.Forms.GroupBox();
            this.BtnCreateDefaultSkills = new System.Windows.Forms.Button();
            this.BtnCreateEmptySkills = new System.Windows.Forms.Button();
            this.BtnReadSkills = new System.Windows.Forms.Button();
            this.grpSkillsOutput = new System.Windows.Forms.GroupBox();
            this.lblSkillsOutput = new System.Windows.Forms.Label();
            this.textBoxSkillsInfo = new System.Windows.Forms.TextBox();

            // ── Tab 16: Validator controls ───────────────────────────────────
            this.grpValidatorActions = new System.Windows.Forms.GroupBox();
            this.BtnValidate = new System.Windows.Forms.Button();
            this.BtnCompareDirectories = new System.Windows.Forms.Button();
            this.lblValidatorStatus = new System.Windows.Forms.Label();
            this.grpValidatorOutput = new System.Windows.Forms.GroupBox();
            this.textBoxValidatorOutput = new System.Windows.Forms.TextBox();

            // ── Tab 17: IDX Patcher controls ─────────────────────────────────
            this.grpPatcherFile = new System.Windows.Forms.GroupBox();
            this.lblPatchIdxLbl = new System.Windows.Forms.Label();
            this.tbPatchIdxPath = new System.Windows.Forms.TextBox();
            this.BtnPatchBrowseIdx = new System.Windows.Forms.Button();
            this.grpPatcherEdit = new System.Windows.Forms.GroupBox();
            this.lblPatchIndexLbl = new System.Windows.Forms.Label();
            this.tbPatchIndex = new System.Windows.Forms.TextBox();
            this.lblPatchLookupLbl = new System.Windows.Forms.Label();
            this.tbPatchLookup = new System.Windows.Forms.TextBox();
            this.lblPatchSizeLbl = new System.Windows.Forms.Label();
            this.tbPatchSize = new System.Windows.Forms.TextBox();
            this.lblPatchUnknownLbl = new System.Windows.Forms.Label();
            this.tbPatchUnknown = new System.Windows.Forms.TextBox();
            this.BtnPatchEntry = new System.Windows.Forms.Button();
            this.BtnClearEntry = new System.Windows.Forms.Button();
            this.grpPatcherRange = new System.Windows.Forms.GroupBox();
            this.lblPatchRangeFromLbl = new System.Windows.Forms.Label();
            this.tbPatchRangeFrom = new System.Windows.Forms.TextBox();
            this.lblPatchRangeCountLbl = new System.Windows.Forms.Label();
            this.tbPatchRangeCount = new System.Windows.Forms.TextBox();
            this.BtnReadRange = new System.Windows.Forms.Button();
            this.grpPatcherOutput = new System.Windows.Forms.GroupBox();
            this.textBoxPatcherOutput = new System.Windows.Forms.TextBox();

            // ── Tab 18: Batch Setup controls ─────────────────────────────────
            this.grpBatchConfig = new System.Windows.Forms.GroupBox();
            this.lblBatchMapWLbl = new System.Windows.Forms.Label();
            this.tbBatchMapW = new System.Windows.Forms.TextBox();
            this.lblBatchMapHLbl = new System.Windows.Forms.Label();
            this.tbBatchMapH = new System.Windows.Forms.TextBox();
            this.lblBatchMapIdxLbl = new System.Windows.Forms.Label();
            this.tbBatchMapIdx = new System.Windows.Forms.TextBox();
            this.lblBatchArtLbl = new System.Windows.Forms.Label();
            this.tbBatchArtCount = new System.Windows.Forms.TextBox();
            this.lblBatchSoundLbl = new System.Windows.Forms.Label();
            this.tbBatchSoundCount = new System.Windows.Forms.TextBox();
            this.lblBatchGumpLbl = new System.Windows.Forms.Label();
            this.tbBatchGumpCount = new System.Windows.Forms.TextBox();
            this.lblBatchMultiLbl = new System.Windows.Forms.Label();
            this.tbBatchMultiCount = new System.Windows.Forms.TextBox();
            this.lblBatchTileLandLbl = new System.Windows.Forms.Label();
            this.tbBatchTileLand = new System.Windows.Forms.TextBox();
            this.lblBatchTileStaticLbl = new System.Windows.Forms.Label();
            this.tbBatchTileStatic = new System.Windows.Forms.TextBox();
            this.lblBatchSkillCountLbl = new System.Windows.Forms.Label();
            this.tbBatchSkillCount = new System.Windows.Forms.TextBox();
            this.checkBoxBatchSkills = new System.Windows.Forms.CheckBox();
            this.checkBoxBatchDefaultSkills = new System.Windows.Forms.CheckBox();
            this.grpBatchActions = new System.Windows.Forms.GroupBox();
            this.btnBatchCreate = new System.Windows.Forms.Button();
            this.lblBatchStatus = new System.Windows.Forms.Label();
            this.grpBatchLog = new System.Windows.Forms.GroupBox();
            this.textBoxBatchLog = new System.Windows.Forms.TextBox();

            // ── Tab 19: Hex Viewer controls ──────────────────────────────────
            this.grpHexFile = new System.Windows.Forms.GroupBox();
            this.lblHexFilePathLbl = new System.Windows.Forms.Label();
            this.tbHexFilePath = new System.Windows.Forms.TextBox();
            this.BtnHexBrowse = new System.Windows.Forms.Button();
            this.lblHexFileInfo = new System.Windows.Forms.Label();
            this.grpHexRead = new System.Windows.Forms.GroupBox();
            this.lblHexOffsetLbl = new System.Windows.Forms.Label();
            this.tbHexOffset = new System.Windows.Forms.TextBox();
            this.lblHexLengthLbl = new System.Windows.Forms.Label();
            this.tbHexLength = new System.Windows.Forms.TextBox();
            this.BtnHexRead = new System.Windows.Forms.Button();
            this.BtnHexFileInfo = new System.Windows.Forms.Button();
            this.grpHexSearch = new System.Windows.Forms.GroupBox();
            this.lblHexPatternLbl = new System.Windows.Forms.Label();
            this.tbHexPattern = new System.Windows.Forms.TextBox();
            this.BtnHexSearch = new System.Windows.Forms.Button();
            this.grpHexOutput = new System.Windows.Forms.GroupBox();
            this.textBoxHexOutput = new System.Windows.Forms.TextBox();

            // Dialogs
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();

            this.tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPalette)).BeginInit();
            this.SuspendLayout();

            // ════════════════════════════════════════════════════════════════
            // FORM
            // ════════════════════════════════════════════════════════════════
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 700);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ARTMulIDXCreator";
            this.Text = "ARTMulIDXCreator  -  UO MUL/IDX Tool  v2.0";
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);

            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.tsStatusLabel });
            this.statusStrip1.Location = new System.Drawing.Point(0, 674);
            this.statusStrip1.Size = new System.Drawing.Size(960, 22);
            this.tsStatusLabel.Text = "Bereit.";

            this.tabControl1.Location = new System.Drawing.Point(3, 3);
            this.tabControl1.Size = new System.Drawing.Size(954, 668);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.TabIndex = 0;
            this.tabControl1.Controls.Add(this.tabPageCreateMuls);
            this.tabControl1.Controls.Add(this.tabPageReadMuls);
            this.tabControl1.Controls.Add(this.tabPageTileData);
            this.tabControl1.Controls.Add(this.tabPageReadOut);
            this.tabControl1.Controls.Add(this.tabPageTexturen);
            this.tabControl1.Controls.Add(this.tabPageRadarColor);
            this.tabControl1.Controls.Add(this.tabPagePalette);
            this.tabControl1.Controls.Add(this.tabPageAnimation);
            this.tabControl1.Controls.Add(this.tabPageArtmul);
            this.tabControl1.Controls.Add(this.tabPageSound);
            this.tabControl1.Controls.Add(this.tabPageGump);
            this.tabControl1.Controls.Add(this.tabPageHues);
            this.tabControl1.Controls.Add(this.tabPageMap);
            this.tabControl1.Controls.Add(this.tabPageMulti);
            this.tabControl1.Controls.Add(this.tabPageSkills);
            this.tabControl1.Controls.Add(this.tabPageValidator);
            this.tabControl1.Controls.Add(this.tabPageIdxPatcher);
            this.tabControl1.Controls.Add(this.tabPageBatch);
            this.tabControl1.Controls.Add(this.tabPageHexViewer);

            // ════════════════════════════════════════════════════════════════
            // TAB 1 – CREATE MULS
            // ════════════════════════════════════════════════════════════════
            this.tabPageCreateMuls.Text = "Create Muls";
            this.tabPageCreateMuls.Size = new System.Drawing.Size(946, 638);
            this.tabPageCreateMuls.TabIndex = 0;
            this.tabPageCreateMuls.UseVisualStyleBackColor = true;

            this.grpCreateMulsDir.Text = "1. Zielverzeichnis waehlen";
            this.grpCreateMulsDir.Location = new System.Drawing.Point(10, 10);
            this.grpCreateMulsDir.Size = new System.Drawing.Size(920, 65);
            this.grpCreateMulsDir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCreateMulsDir.TabIndex = 0;
            this.tabPageCreateMuls.Controls.Add(this.grpCreateMulsDir);

            this.BtFileOrder.Text = "Verzeichnis waehlen ...";
            this.BtFileOrder.Location = new System.Drawing.Point(12, 25);
            this.BtFileOrder.Size = new System.Drawing.Size(185, 26);
            this.BtFileOrder.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtFileOrder.TabIndex = 0;
            this.BtFileOrder.Click += new System.EventHandler(this.BtFileOrder_Click);
            this.grpCreateMulsDir.Controls.Add(this.BtFileOrder);

            this.textBox1.Location = new System.Drawing.Point(205, 27);
            this.textBox1.Size = new System.Drawing.Size(560, 23);
            this.textBox1.ReadOnly = true;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBox1.TabIndex = 1;
            this.grpCreateMulsDir.Controls.Add(this.textBox1);

            this.lblDirInfo.Text = "Zielordner fuer artidx.MUL + art.MUL";
            this.lblDirInfo.Location = new System.Drawing.Point(775, 30);
            this.lblDirInfo.AutoSize = true;
            this.lblDirInfo.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblDirInfo.ForeColor = System.Drawing.Color.Gray;
            this.lblDirInfo.TabIndex = 2;
            this.grpCreateMulsDir.Controls.Add(this.lblDirInfo);

            this.grpCreateMulsCount.Text = "2. Eintragszahl festlegen";
            this.grpCreateMulsCount.Location = new System.Drawing.Point(10, 83);
            this.grpCreateMulsCount.Size = new System.Drawing.Size(920, 65);
            this.grpCreateMulsCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCreateMulsCount.TabIndex = 1;
            this.tabPageCreateMuls.Controls.Add(this.grpCreateMulsCount);

            this.textBox2.Text = "81884";
            this.textBox2.Location = new System.Drawing.Point(12, 28);
            this.textBox2.Size = new System.Drawing.Size(130, 23);
            this.textBox2.Font = new System.Drawing.Font("Consolas", 10F);
            this.textBox2.TabIndex = 0;
            this.grpCreateMulsCount.Controls.Add(this.textBox2);

            this.lblCountHint.Text = "Hex-Eingabe moeglich (z.B. 0x14F9C)  |  Standard Original-UO = 81884";
            this.lblCountHint.Location = new System.Drawing.Point(155, 31);
            this.lblCountHint.AutoSize = true;
            this.lblCountHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblCountHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblCountHint.TabIndex = 1;
            this.grpCreateMulsCount.Controls.Add(this.lblCountHint);

            this.grpCreateMulsButtons.Text = "3. Erstellen  (alle Buttons erzeugen dasselbe Legacy-Format)";
            this.grpCreateMulsButtons.Location = new System.Drawing.Point(10, 156);
            this.grpCreateMulsButtons.Size = new System.Drawing.Size(560, 195);
            this.grpCreateMulsButtons.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCreateMulsButtons.TabIndex = 2;
            this.tabPageCreateMuls.Controls.Add(this.grpCreateMulsButtons);

            this.BtCreateARTIDXMul.Text = "Create (long)";
            this.BtCreateARTIDXMul.Location = new System.Drawing.Point(12, 25);
            this.BtCreateARTIDXMul.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul.TabIndex = 0;
            this.BtCreateARTIDXMul.Click += new System.EventHandler(this.BtCreateARTIDXMul_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul);

            this.BtCreateARTIDXMul_Ulong.Text = "Create (ulong)";
            this.BtCreateARTIDXMul_Ulong.Location = new System.Drawing.Point(170, 25);
            this.BtCreateARTIDXMul_Ulong.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Ulong.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Ulong.TabIndex = 1;
            this.BtCreateARTIDXMul_Ulong.Click += new System.EventHandler(this.BtCreateARTIDXMul_Ulong_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Ulong);

            this.BtCreateARTIDXMul_uint.Text = "Create (uint)";
            this.BtCreateARTIDXMul_uint.Location = new System.Drawing.Point(328, 25);
            this.BtCreateARTIDXMul_uint.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_uint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_uint.TabIndex = 2;
            this.BtCreateARTIDXMul_uint.Click += new System.EventHandler(this.BtCreateARTIDXMul_uint_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_uint);

            this.BtCreateARTIDXMul_Int.Text = "Create (int)";
            this.BtCreateARTIDXMul_Int.Location = new System.Drawing.Point(12, 61);
            this.BtCreateARTIDXMul_Int.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Int.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Int.TabIndex = 3;
            this.BtCreateARTIDXMul_Int.Click += new System.EventHandler(this.BtCreateARTIDXMul_Int_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Int);

            this.BtCreateARTIDXMul_Ushort.Text = "Create (ushort)";
            this.BtCreateARTIDXMul_Ushort.Location = new System.Drawing.Point(170, 61);
            this.BtCreateARTIDXMul_Ushort.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Ushort.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Ushort.TabIndex = 4;
            this.BtCreateARTIDXMul_Ushort.Click += new System.EventHandler(this.BtCreateARTIDXMul_Ushort_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Ushort);

            this.BtCreateARTIDXMul_Short.Text = "Create (short)";
            this.BtCreateARTIDXMul_Short.Location = new System.Drawing.Point(328, 61);
            this.BtCreateARTIDXMul_Short.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Short.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Short.TabIndex = 5;
            this.BtCreateARTIDXMul_Short.Click += new System.EventHandler(this.BtCreateARTIDXMul_Short_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Short);

            this.BtCreateARTIDXMul_Byte.Text = "Create (byte)";
            this.BtCreateARTIDXMul_Byte.Location = new System.Drawing.Point(12, 97);
            this.BtCreateARTIDXMul_Byte.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Byte.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Byte.TabIndex = 6;
            this.BtCreateARTIDXMul_Byte.Click += new System.EventHandler(this.BtCreateARTIDXMul_Byte_Click);
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Byte);

            this.BtCreateARTIDXMul_Sbyte.Text = "Create (sbyte)";
            this.BtCreateARTIDXMul_Sbyte.Location = new System.Drawing.Point(170, 97);
            this.BtCreateARTIDXMul_Sbyte.Size = new System.Drawing.Size(150, 28);
            this.BtCreateARTIDXMul_Sbyte.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateARTIDXMul_Sbyte.TabIndex = 7;
            this.grpCreateMulsButtons.Controls.Add(this.BtCreateARTIDXMul_Sbyte);

            this.lblButtonsNote.Text = "Alle 8 Buttons erzeugen identische Dateien im Legacy-IDX-Format. Die Typ-Bezeichnungen sind historisch.";
            this.lblButtonsNote.Location = new System.Drawing.Point(12, 140);
            this.lblButtonsNote.Size = new System.Drawing.Size(530, 42);
            this.lblButtonsNote.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblButtonsNote.ForeColor = System.Drawing.Color.DimGray;
            this.lblButtonsNote.TabIndex = 8;
            this.grpCreateMulsButtons.Controls.Add(this.lblButtonsNote);

            this.grpRename.Text = "4. Optional: Umbenennen";
            this.grpRename.Location = new System.Drawing.Point(580, 156);
            this.grpRename.Size = new System.Drawing.Size(350, 120);
            this.grpRename.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpRename.TabIndex = 3;
            this.tabPageCreateMuls.Controls.Add(this.grpRename);

            this.ComboBoxMuls.Items.AddRange(new object[] { "Texture" });
            this.ComboBoxMuls.Location = new System.Drawing.Point(12, 28);
            this.ComboBoxMuls.Size = new System.Drawing.Size(200, 23);
            this.ComboBoxMuls.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ComboBoxMuls.TabIndex = 0;
            this.ComboBoxMuls.SelectedIndexChanged += new System.EventHandler(this.ComboBoxMuls_SelectedIndexChanged);
            this.grpRename.Controls.Add(this.ComboBoxMuls);

            this.lblRenameHint.Text = "Texture: art.mul/artidx.mul wird umbenannt\r\nin texmaps.mul/texidx.mul";
            this.lblRenameHint.Location = new System.Drawing.Point(12, 56);
            this.lblRenameHint.AutoSize = true;
            this.lblRenameHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblRenameHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblRenameHint.TabIndex = 1;
            this.grpRename.Controls.Add(this.lblRenameHint);

            this.grpCreateOutput.Text = "Ausgabe";
            this.grpCreateOutput.Location = new System.Drawing.Point(10, 360);
            this.grpCreateOutput.Size = new System.Drawing.Size(920, 65);
            this.grpCreateOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpCreateOutput.TabIndex = 4;
            this.tabPageCreateMuls.Controls.Add(this.grpCreateOutput);

            this.lbCreatedMul.Text = "-";
            this.lbCreatedMul.Location = new System.Drawing.Point(12, 22);
            this.lbCreatedMul.Size = new System.Drawing.Size(890, 36);
            this.lbCreatedMul.Font = new System.Drawing.Font("Consolas", 9F);
            this.lbCreatedMul.ForeColor = System.Drawing.Color.DarkGreen;
            this.lbCreatedMul.TabIndex = 0;
            this.grpCreateOutput.Controls.Add(this.lbCreatedMul);

            // ════════════════════════════════════════════════════════════════
            // TAB 2 – READ MULS
            // ════════════════════════════════════════════════════════════════
            this.tabPageReadMuls.Text = "Read Muls";
            this.tabPageReadMuls.Size = new System.Drawing.Size(946, 638);
            this.tabPageReadMuls.TabIndex = 1;
            this.tabPageReadMuls.UseVisualStyleBackColor = true;

            this.grpReadMulsActions.Text = "Aktionen";
            this.grpReadMulsActions.Location = new System.Drawing.Point(10, 10);
            this.grpReadMulsActions.Size = new System.Drawing.Size(920, 70);
            this.grpReadMulsActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpReadMulsActions.TabIndex = 0;
            this.tabPageReadMuls.Controls.Add(this.grpReadMulsActions);

            this.BtnCountEntries.Text = "Eintraege zaehlen";
            this.BtnCountEntries.Location = new System.Drawing.Point(12, 28);
            this.BtnCountEntries.Size = new System.Drawing.Size(155, 28);
            this.BtnCountEntries.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCountEntries.TabIndex = 0;
            this.BtnCountEntries.Click += new System.EventHandler(this.BtnCountEntries_Click);
            this.grpReadMulsActions.Controls.Add(this.BtnCountEntries);

            this.lblEntryCount.Text = "-";
            this.lblEntryCount.Location = new System.Drawing.Point(180, 33);
            this.lblEntryCount.AutoSize = true;
            this.lblEntryCount.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblEntryCount.ForeColor = System.Drawing.Color.Navy;
            this.lblEntryCount.TabIndex = 1;
            this.grpReadMulsActions.Controls.Add(this.lblEntryCount);

            this.grpReadMulsResult.Text = "Alle Eintraege lesen (max. 2000 Zeilen)";
            this.grpReadMulsResult.Location = new System.Drawing.Point(10, 88);
            this.grpReadMulsResult.Size = new System.Drawing.Size(920, 360);
            this.grpReadMulsResult.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpReadMulsResult.TabIndex = 1;
            this.tabPageReadMuls.Controls.Add(this.grpReadMulsResult);

            this.BtnShowInfo.Text = "Index gesamt lesen";
            this.BtnShowInfo.Location = new System.Drawing.Point(12, 26);
            this.BtnShowInfo.Size = new System.Drawing.Size(175, 28);
            this.BtnShowInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnShowInfo.TabIndex = 0;
            this.BtnShowInfo.Click += new System.EventHandler(this.BtnShowInfo_Click);
            this.grpReadMulsResult.Controls.Add(this.BtnShowInfo);

            this.textBoxInfo.Location = new System.Drawing.Point(12, 62);
            this.textBoxInfo.Size = new System.Drawing.Size(890, 285);
            this.textBoxInfo.Multiline = true;
            this.textBoxInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxInfo.ReadOnly = true;
            this.textBoxInfo.TabIndex = 1;
            this.grpReadMulsResult.Controls.Add(this.textBoxInfo);

            this.grpReadSingle.Text = "Einzelnen Eintrag lesen";
            this.grpReadSingle.Location = new System.Drawing.Point(10, 456);
            this.grpReadSingle.Size = new System.Drawing.Size(920, 65);
            this.grpReadSingle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpReadSingle.TabIndex = 2;
            this.tabPageReadMuls.Controls.Add(this.grpReadSingle);

            this.lblIndexHint.Text = "Index-Nr:";
            this.lblIndexHint.Location = new System.Drawing.Point(12, 32);
            this.lblIndexHint.AutoSize = true;
            this.lblIndexHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblIndexHint.TabIndex = 0;
            this.grpReadSingle.Controls.Add(this.lblIndexHint);

            this.textBoxIndex.Text = "1";
            this.textBoxIndex.Location = new System.Drawing.Point(75, 29);
            this.textBoxIndex.Size = new System.Drawing.Size(70, 23);
            this.textBoxIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.textBoxIndex.TabIndex = 1;
            this.grpReadSingle.Controls.Add(this.textBoxIndex);

            this.BtnReadArtIdx.Text = "Eintrag anzeigen";
            this.BtnReadArtIdx.Location = new System.Drawing.Point(160, 27);
            this.BtnReadArtIdx.Size = new System.Drawing.Size(160, 28);
            this.BtnReadArtIdx.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnReadArtIdx.TabIndex = 2;
            this.BtnReadArtIdx.Click += new System.EventHandler(this.BtnReadArtIdx2_Click);
            this.grpReadSingle.Controls.Add(this.BtnReadArtIdx);

            // ════════════════════════════════════════════════════════════════
            // TAB 3 – TILEDATA
            // ════════════════════════════════════════════════════════════════
            this.tabPageTileData.Text = "TileData";
            this.tabPageTileData.Size = new System.Drawing.Size(946, 638);
            this.tabPageTileData.TabIndex = 2;
            this.tabPageTileData.UseVisualStyleBackColor = true;

            this.grpTileDataDir.Text = "Verzeichnis";
            this.grpTileDataDir.Location = new System.Drawing.Point(10, 10);
            this.grpTileDataDir.Size = new System.Drawing.Size(920, 58);
            this.grpTileDataDir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataDir.TabIndex = 0;
            this.tabPageTileData.Controls.Add(this.grpTileDataDir);

            this.btnTileDataBrowse.Text = "Waehlen ...";
            this.btnTileDataBrowse.Location = new System.Drawing.Point(12, 22);
            this.btnTileDataBrowse.Size = new System.Drawing.Size(120, 26);
            this.btnTileDataBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnTileDataBrowse.TabIndex = 0;
            this.btnTileDataBrowse.Click += new System.EventHandler(this.BtCreateTiledata_Click);
            this.grpTileDataDir.Controls.Add(this.btnTileDataBrowse);

            this.tbDirTileData.Location = new System.Drawing.Point(142, 24);
            this.tbDirTileData.Size = new System.Drawing.Size(560, 23);
            this.tbDirTileData.ReadOnly = true;
            this.tbDirTileData.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbDirTileData.TabIndex = 1;
            this.grpTileDataDir.Controls.Add(this.tbDirTileData);

            this.grpTileDataConfig.Text = "Konfiguration  -  Gruppen x 32 = Tile-Eintraege";
            this.grpTileDataConfig.Location = new System.Drawing.Point(10, 76);
            this.grpTileDataConfig.Size = new System.Drawing.Size(560, 125);
            this.grpTileDataConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataConfig.TabIndex = 1;
            this.tabPageTileData.Controls.Add(this.grpTileDataConfig);

            this.lblLandGroupsLbl.Text = "Land-Gruppen:";
            this.lblLandGroupsLbl.Location = new System.Drawing.Point(12, 28);
            this.lblLandGroupsLbl.AutoSize = true;
            this.lblLandGroupsLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLandGroupsLbl.TabIndex = 0;
            this.grpTileDataConfig.Controls.Add(this.lblLandGroupsLbl);

            this.tblandTileGroups.Text = "512";
            this.tblandTileGroups.Location = new System.Drawing.Point(140, 25);
            this.tblandTileGroups.Size = new System.Drawing.Size(90, 23);
            this.tblandTileGroups.Font = new System.Drawing.Font("Consolas", 10F);
            this.tblandTileGroups.TabIndex = 1;
            this.grpTileDataConfig.Controls.Add(this.tblandTileGroups);

            this.lblLandGroupsHint.Text = "Standard: 512 x 32 = 16.384 Land-Tiles";
            this.lblLandGroupsHint.Location = new System.Drawing.Point(245, 28);
            this.lblLandGroupsHint.AutoSize = true;
            this.lblLandGroupsHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLandGroupsHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblLandGroupsHint.TabIndex = 2;
            this.grpTileDataConfig.Controls.Add(this.lblLandGroupsHint);

            this.lblStaticGroupsLbl.Text = "Static-Gruppen:";
            this.lblStaticGroupsLbl.Location = new System.Drawing.Point(12, 62);
            this.lblStaticGroupsLbl.AutoSize = true;
            this.lblStaticGroupsLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStaticGroupsLbl.TabIndex = 3;
            this.grpTileDataConfig.Controls.Add(this.lblStaticGroupsLbl);

            this.tbstaticTileGroups.Text = "2048";
            this.tbstaticTileGroups.Location = new System.Drawing.Point(140, 59);
            this.tbstaticTileGroups.Size = new System.Drawing.Size(90, 23);
            this.tbstaticTileGroups.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbstaticTileGroups.TabIndex = 4;
            this.grpTileDataConfig.Controls.Add(this.tbstaticTileGroups);

            this.lblStaticGroupsHint.Text = "Standard: 2048 x 32 = 65.536 Static-Tiles";
            this.lblStaticGroupsHint.Location = new System.Drawing.Point(245, 62);
            this.lblStaticGroupsHint.AutoSize = true;
            this.lblStaticGroupsHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblStaticGroupsHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblStaticGroupsHint.TabIndex = 5;
            this.grpTileDataConfig.Controls.Add(this.lblStaticGroupsHint);

            this.BtCreateTiledata.Text = "Tiledata.mul erstellen";
            this.BtCreateTiledata.Location = new System.Drawing.Point(12, 90);
            this.BtCreateTiledata.Size = new System.Drawing.Size(200, 28);
            this.BtCreateTiledata.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtCreateTiledata.TabIndex = 6;
            this.BtCreateTiledata.Click += new System.EventHandler(this.BtCreateTiledata_Click);
            this.grpTileDataConfig.Controls.Add(this.BtCreateTiledata);

            this.grpTileDataQuick.Text = "Schnell-Erstellung (Standard-Werte)";
            this.grpTileDataQuick.Location = new System.Drawing.Point(580, 76);
            this.grpTileDataQuick.Size = new System.Drawing.Size(350, 125);
            this.grpTileDataQuick.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataQuick.TabIndex = 2;
            this.tabPageTileData.Controls.Add(this.grpTileDataQuick);

            this.BtCreateTiledataEmpty.Text = "Standard leer  (512 / 2048)";
            this.BtCreateTiledataEmpty.Location = new System.Drawing.Point(10, 22);
            this.BtCreateTiledataEmpty.Size = new System.Drawing.Size(325, 26);
            this.BtCreateTiledataEmpty.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateTiledataEmpty.TabIndex = 0;
            this.BtCreateTiledataEmpty.Click += new System.EventHandler(this.BtCreateTiledataEmpty_Click);
            this.grpTileDataQuick.Controls.Add(this.BtCreateTiledataEmpty);

            this.BtCreateTiledataEmpty2.Text = "Komplett leer (Minimal)";
            this.BtCreateTiledataEmpty2.Location = new System.Drawing.Point(10, 55);
            this.BtCreateTiledataEmpty2.Size = new System.Drawing.Size(325, 26);
            this.BtCreateTiledataEmpty2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateTiledataEmpty2.TabIndex = 1;
            this.BtCreateTiledataEmpty2.Click += new System.EventHandler(this.BtCreateTiledataEmpty2_Click);
            this.grpTileDataQuick.Controls.Add(this.BtCreateTiledataEmpty2);

            this.BtCreateSimpleTiledata.Text = "Simple Tiledata";
            this.BtCreateSimpleTiledata.Location = new System.Drawing.Point(10, 88);
            this.BtCreateSimpleTiledata.Size = new System.Drawing.Size(325, 26);
            this.BtCreateSimpleTiledata.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateSimpleTiledata.TabIndex = 2;
            this.BtCreateSimpleTiledata.Click += new System.EventHandler(this.BtCreateSimpleTiledata_Click);
            this.grpTileDataQuick.Controls.Add(this.BtCreateSimpleTiledata);

            this.grpTileDataRead.Text = "Tiledata.mul lesen";
            this.grpTileDataRead.Location = new System.Drawing.Point(10, 209);
            this.grpTileDataRead.Size = new System.Drawing.Size(920, 80);
            this.grpTileDataRead.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataRead.TabIndex = 3;
            this.tabPageTileData.Controls.Add(this.grpTileDataRead);

            this.BtTiledatainfo.Text = "Zusammenfassung";
            this.BtTiledatainfo.Location = new System.Drawing.Point(12, 28);
            this.BtTiledatainfo.Size = new System.Drawing.Size(155, 28);
            this.BtTiledatainfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtTiledatainfo.TabIndex = 0;
            this.BtTiledatainfo.Click += new System.EventHandler(this.BtTiledatainfo_Click);
            this.grpTileDataRead.Controls.Add(this.BtTiledatainfo);

            this.BtnCountTileDataEntries.Text = "Eintraege zaehlen";
            this.BtnCountTileDataEntries.Location = new System.Drawing.Point(180, 28);
            this.BtnCountTileDataEntries.Size = new System.Drawing.Size(150, 28);
            this.BtnCountTileDataEntries.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCountTileDataEntries.TabIndex = 1;
            this.BtnCountTileDataEntries.Click += new System.EventHandler(this.BtnCountTileDataEntries_Click);
            this.grpTileDataRead.Controls.Add(this.BtnCountTileDataEntries);

            this.lblTileDataEntryCount.Text = "-";
            this.lblTileDataEntryCount.Location = new System.Drawing.Point(345, 33);
            this.lblTileDataEntryCount.AutoSize = true;
            this.lblTileDataEntryCount.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblTileDataEntryCount.ForeColor = System.Drawing.Color.Navy;
            this.lblTileDataEntryCount.TabIndex = 2;
            this.grpTileDataRead.Controls.Add(this.lblTileDataEntryCount);

            this.BtReadTileFlags.Text = "Flag-Namen anzeigen";
            this.BtReadTileFlags.Location = new System.Drawing.Point(700, 28);
            this.BtReadTileFlags.Size = new System.Drawing.Size(175, 28);
            this.BtReadTileFlags.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtReadTileFlags.TabIndex = 3;
            this.BtReadTileFlags.Click += new System.EventHandler(this.BtReadTileFlags_Click);
            this.grpTileDataRead.Controls.Add(this.BtReadTileFlags);

            this.grpTileDataIndex.Text = "Einzeleintrag nach Index lesen";
            this.grpTileDataIndex.Location = new System.Drawing.Point(10, 297);
            this.grpTileDataIndex.Size = new System.Drawing.Size(920, 90);
            this.grpTileDataIndex.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataIndex.TabIndex = 4;
            this.tabPageTileData.Controls.Add(this.grpTileDataIndex);

            this.lblTiledataIndexHint.Text = "Index-Nr:";
            this.lblTiledataIndexHint.Location = new System.Drawing.Point(12, 36);
            this.lblTiledataIndexHint.AutoSize = true;
            this.lblTiledataIndexHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTiledataIndexHint.TabIndex = 0;
            this.grpTileDataIndex.Controls.Add(this.lblTiledataIndexHint);

            this.textBoxTiledataIndex.Location = new System.Drawing.Point(78, 33);
            this.textBoxTiledataIndex.Size = new System.Drawing.Size(110, 23);
            this.textBoxTiledataIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.textBoxTiledataIndex.TabIndex = 1;
            this.grpTileDataIndex.Controls.Add(this.textBoxTiledataIndex);

            this.BtReadIndexTiledata.Text = "Index lesen";
            this.BtReadIndexTiledata.Location = new System.Drawing.Point(205, 30);
            this.BtReadIndexTiledata.Size = new System.Drawing.Size(120, 28);
            this.BtReadIndexTiledata.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtReadIndexTiledata.TabIndex = 2;
            this.BtReadIndexTiledata.Click += new System.EventHandler(this.BtReadIndexTiledata_Click);
            this.grpTileDataIndex.Controls.Add(this.BtReadIndexTiledata);

            this.BtReadLandTile.Text = "Land-Tile";
            this.BtReadLandTile.Location = new System.Drawing.Point(335, 30);
            this.BtReadLandTile.Size = new System.Drawing.Size(120, 28);
            this.BtReadLandTile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtReadLandTile.TabIndex = 3;
            this.BtReadLandTile.Click += new System.EventHandler(this.BtReadLandTile_Click);
            this.grpTileDataIndex.Controls.Add(this.BtReadLandTile);

            this.BtReadStaticTile.Text = "Static-Tile";
            this.BtReadStaticTile.Location = new System.Drawing.Point(465, 30);
            this.BtReadStaticTile.Size = new System.Drawing.Size(120, 28);
            this.BtReadStaticTile.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtReadStaticTile.TabIndex = 4;
            this.BtReadStaticTile.Click += new System.EventHandler(this.BtReadStaticTile_Click);
            this.grpTileDataIndex.Controls.Add(this.BtReadStaticTile);

            this.BtSelectDirectory.Text = "Hex-Rohdaten";
            this.BtSelectDirectory.Location = new System.Drawing.Point(595, 30);
            this.BtSelectDirectory.Size = new System.Drawing.Size(120, 28);
            this.BtSelectDirectory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtSelectDirectory.TabIndex = 5;
            this.BtSelectDirectory.Click += new System.EventHandler(this.BtTReadHexAndSelectDirectory_Click);
            this.grpTileDataIndex.Controls.Add(this.BtSelectDirectory);

            this.grpTileDataOutput.Text = "Ausgabe";
            this.grpTileDataOutput.Location = new System.Drawing.Point(10, 395);
            this.grpTileDataOutput.Size = new System.Drawing.Size(920, 80);
            this.grpTileDataOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTileDataOutput.TabIndex = 5;
            this.tabPageTileData.Controls.Add(this.grpTileDataOutput);

            this.lbTileDataCreate.Text = "-";
            this.lbTileDataCreate.Location = new System.Drawing.Point(12, 22);
            this.lbTileDataCreate.Size = new System.Drawing.Size(890, 50);
            this.lbTileDataCreate.Font = new System.Drawing.Font("Consolas", 9F);
            this.lbTileDataCreate.ForeColor = System.Drawing.Color.DarkGreen;
            this.lbTileDataCreate.TabIndex = 0;
            this.grpTileDataOutput.Controls.Add(this.lbTileDataCreate);

            this.checkBoxTileData.Text = "Original";
            this.checkBoxTileData.Location = new System.Drawing.Point(10, 482);
            this.checkBoxTileData.AutoSize = true;
            this.checkBoxTileData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxTileData.TabIndex = 6;
            this.tabPageTileData.Controls.Add(this.checkBoxTileData);

            // ════════════════════════════════════════════════════════════════
            // TAB 4 – READOUT
            // ════════════════════════════════════════════════════════════════
            this.tabPageReadOut.Text = "ReadOut";
            this.tabPageReadOut.Size = new System.Drawing.Size(946, 638);
            this.tabPageReadOut.TabIndex = 3;
            this.tabPageReadOut.UseVisualStyleBackColor = true;

            this.grpReadOutActions.Text = "Tiledata.mul laden  (50 Eintraege je Klick | Leertaste = weitere 50)";
            this.grpReadOutActions.Location = new System.Drawing.Point(10, 10);
            this.grpReadOutActions.Size = new System.Drawing.Size(560, 62);
            this.grpReadOutActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpReadOutActions.TabIndex = 0;
            this.tabPageReadOut.Controls.Add(this.grpReadOutActions);

            this.ButtonReadTileData.Text = "Hex-Positionen";
            this.ButtonReadTileData.Location = new System.Drawing.Point(12, 26);
            this.ButtonReadTileData.Size = new System.Drawing.Size(145, 26);
            this.ButtonReadTileData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ButtonReadTileData.TabIndex = 0;
            this.ButtonReadTileData.Click += new System.EventHandler(this.ButtonReadTileData_Click);
            this.grpReadOutActions.Controls.Add(this.ButtonReadTileData);

            this.ButtonReadLandTileData.Text = "Land-Tiles";
            this.ButtonReadLandTileData.Location = new System.Drawing.Point(165, 26);
            this.ButtonReadLandTileData.Size = new System.Drawing.Size(145, 26);
            this.ButtonReadLandTileData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ButtonReadLandTileData.TabIndex = 1;
            this.ButtonReadLandTileData.Click += new System.EventHandler(this.ButtonReadLandTileData_Click);
            this.grpReadOutActions.Controls.Add(this.ButtonReadLandTileData);

            this.ButtonReadStaticTileData.Text = "Static-Tiles";
            this.ButtonReadStaticTileData.Location = new System.Drawing.Point(318, 26);
            this.ButtonReadStaticTileData.Size = new System.Drawing.Size(145, 26);
            this.ButtonReadStaticTileData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ButtonReadStaticTileData.TabIndex = 2;
            this.ButtonReadStaticTileData.Click += new System.EventHandler(this.ButtonReadStaticTileData_Click);
            this.grpReadOutActions.Controls.Add(this.ButtonReadStaticTileData);

            this.listViewTileData.Location = new System.Drawing.Point(10, 80);
            this.listViewTileData.Size = new System.Drawing.Size(560, 355);
            this.listViewTileData.View = System.Windows.Forms.View.Details;
            this.listViewTileData.FullRowSelect = true;
            this.listViewTileData.GridLines = true;
            this.listViewTileData.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.listViewTileData.TabIndex = 1;
            this.listViewTileData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TiledataHex_KeyDown);
            this.listViewTileData.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ListViewTileData_MouseClick);
            this.tabPageReadOut.Controls.Add(this.listViewTileData);

            this.lblSelectedEntry.Text = "Ausgewaehlter Eintrag:";
            this.lblSelectedEntry.Location = new System.Drawing.Point(10, 440);
            this.lblSelectedEntry.AutoSize = true;
            this.lblSelectedEntry.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSelectedEntry.TabIndex = 2;
            this.tabPageReadOut.Controls.Add(this.lblSelectedEntry);

            this.textBoxOutput.Location = new System.Drawing.Point(10, 460);
            this.textBoxOutput.Size = new System.Drawing.Size(560, 80);
            this.textBoxOutput.Multiline = true;
            this.textBoxOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.TabIndex = 3;
            this.tabPageReadOut.Controls.Add(this.textBoxOutput);

            this.grpReadOutInfo.Text = "Detail-Ansicht";
            this.grpReadOutInfo.Location = new System.Drawing.Point(580, 10);
            this.grpReadOutInfo.Size = new System.Drawing.Size(356, 530);
            this.grpReadOutInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpReadOutInfo.TabIndex = 4;
            this.tabPageReadOut.Controls.Add(this.grpReadOutInfo);

            this.lblReadOutIdxLbl.Text = "Index:";
            this.lblReadOutIdxLbl.Location = new System.Drawing.Point(10, 26);
            this.lblReadOutIdxLbl.AutoSize = true;
            this.lblReadOutIdxLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblReadOutIdxLbl.TabIndex = 0;
            this.grpReadOutInfo.Controls.Add(this.lblReadOutIdxLbl);

            this.textBoxTileDataInfo.Location = new System.Drawing.Point(10, 290);
            this.textBoxTileDataInfo.Size = new System.Drawing.Size(330, 225);
            this.textBoxTileDataInfo.Multiline = true;
            this.textBoxTileDataInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxTileDataInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxTileDataInfo.ReadOnly = true;
            this.textBoxTileDataInfo.TabIndex = 8;
            this.grpReadOutInfo.Controls.Add(this.textBoxTileDataInfo);

            // ════════════════════════════════════════════════════════════════
            // TAB 5 – TEXTUREN
            // ════════════════════════════════════════════════════════════════
            this.tabPageTexturen.Text = "Texturen";
            this.tabPageTexturen.Size = new System.Drawing.Size(946, 638);
            this.tabPageTexturen.TabIndex = 4;
            this.tabPageTexturen.UseVisualStyleBackColor = true;

            this.grpTexConfig.Text = "Konfiguration";
            this.grpTexConfig.Location = new System.Drawing.Point(10, 10);
            this.grpTexConfig.Size = new System.Drawing.Size(920, 95);
            this.grpTexConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTexConfig.TabIndex = 0;
            this.tabPageTexturen.Controls.Add(this.grpTexConfig);

            this.lblTexCountLbl.Text = "Anzahl Index-Eintraege:";
            this.lblTexCountLbl.Location = new System.Drawing.Point(12, 30);
            this.lblTexCountLbl.AutoSize = true;
            this.lblTexCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblTexCountLbl.TabIndex = 0;
            this.grpTexConfig.Controls.Add(this.lblTexCountLbl);

            this.tbIndexCountTexture.Text = "16383";
            this.tbIndexCountTexture.Location = new System.Drawing.Point(185, 27);
            this.tbIndexCountTexture.Size = new System.Drawing.Size(110, 23);
            this.tbIndexCountTexture.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbIndexCountTexture.TabIndex = 1;
            this.grpTexConfig.Controls.Add(this.tbIndexCountTexture);

            this.lblTexCountHint.Text = "Standard: 16383 Eintraege.";
            this.lblTexCountHint.Location = new System.Drawing.Point(310, 30);
            this.lblTexCountHint.AutoSize = true;
            this.lblTexCountHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblTexCountHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblTexCountHint.TabIndex = 2;
            this.grpTexConfig.Controls.Add(this.lblTexCountHint);

            this.checkBoxTexture.Text = "Nur 2 Bilder am Anfang (Rest = schwarze Platzhalter)";
            this.checkBoxTexture.Checked = true;
            this.checkBoxTexture.Location = new System.Drawing.Point(12, 60);
            this.checkBoxTexture.AutoSize = true;
            this.checkBoxTexture.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxTexture.TabIndex = 3;
            this.grpTexConfig.Controls.Add(this.checkBoxTexture);

            this.grpTexActions.Text = "Aktionen";
            this.grpTexActions.Location = new System.Drawing.Point(10, 113);
            this.grpTexActions.Size = new System.Drawing.Size(920, 75);
            this.grpTexActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTexActions.TabIndex = 1;
            this.tabPageTexturen.Controls.Add(this.grpTexActions);

            this.BtCreateTextur.Text = "TexMaps.mul + TexIdx.mul erstellen";
            this.BtCreateTextur.Location = new System.Drawing.Point(12, 28);
            this.BtCreateTextur.Size = new System.Drawing.Size(280, 30);
            this.BtCreateTextur.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtCreateTextur.TabIndex = 0;
            this.BtCreateTextur.Click += new System.EventHandler(this.BtCreateTextur_Click);
            this.grpTexActions.Controls.Add(this.BtCreateTextur);

            this.BtCreateIndexes.Text = "Nur leere TexIdx.mul erstellen";
            this.BtCreateIndexes.Location = new System.Drawing.Point(310, 28);
            this.BtCreateIndexes.Size = new System.Drawing.Size(255, 30);
            this.BtCreateIndexes.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreateIndexes.TabIndex = 1;
            this.BtCreateIndexes.Click += new System.EventHandler(this.BtCreateIndexes_Click);
            this.grpTexActions.Controls.Add(this.BtCreateIndexes);

            this.grpTexOutput.Text = "Ausgabe";
            this.grpTexOutput.Location = new System.Drawing.Point(10, 196);
            this.grpTexOutput.Size = new System.Drawing.Size(920, 65);
            this.grpTexOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpTexOutput.TabIndex = 2;
            this.tabPageTexturen.Controls.Add(this.grpTexOutput);

            this.lbTextureCount.Text = "-";
            this.lbTextureCount.Location = new System.Drawing.Point(12, 22);
            this.lbTextureCount.AutoSize = true;
            this.lbTextureCount.Font = new System.Drawing.Font("Consolas", 9F);
            this.lbTextureCount.ForeColor = System.Drawing.Color.DarkGreen;
            this.lbTextureCount.TabIndex = 0;
            this.grpTexOutput.Controls.Add(this.lbTextureCount);

            this.tbIndexCount.Text = "";
            this.tbIndexCount.Location = new System.Drawing.Point(12, 40);
            this.tbIndexCount.AutoSize = true;
            this.tbIndexCount.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbIndexCount.ForeColor = System.Drawing.Color.Navy;
            this.tbIndexCount.TabIndex = 1;
            this.grpTexOutput.Controls.Add(this.tbIndexCount);

            // ════════════════════════════════════════════════════════════════
            // TAB 6 – RADARCOLOR
            // ════════════════════════════════════════════════════════════════
            this.tabPageRadarColor.Text = "RadarColor";
            this.tabPageRadarColor.Size = new System.Drawing.Size(946, 638);
            this.tabPageRadarColor.TabIndex = 5;
            this.tabPageRadarColor.UseVisualStyleBackColor = true;

            this.grpRadarConfig.Text = "Konfiguration  -  radarcol.mul";
            this.grpRadarConfig.Location = new System.Drawing.Point(10, 10);
            this.grpRadarConfig.Size = new System.Drawing.Size(920, 105);
            this.grpRadarConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpRadarConfig.TabIndex = 0;
            this.tabPageRadarColor.Controls.Add(this.grpRadarConfig);

            this.lblRadarCountLbl.Text = "Anzahl Farb-Eintraege:";
            this.lblRadarCountLbl.Location = new System.Drawing.Point(12, 32);
            this.lblRadarCountLbl.AutoSize = true;
            this.lblRadarCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRadarCountLbl.TabIndex = 0;
            this.grpRadarConfig.Controls.Add(this.lblRadarCountLbl);

            this.indexCountTextBox.Text = "82374";
            this.indexCountTextBox.Location = new System.Drawing.Point(180, 29);
            this.indexCountTextBox.Size = new System.Drawing.Size(120, 23);
            this.indexCountTextBox.Font = new System.Drawing.Font("Consolas", 10F);
            this.indexCountTextBox.TabIndex = 1;
            this.grpRadarConfig.Controls.Add(this.indexCountTextBox);

            this.lblRadarCountHint.Text = "Jeder Eintrag = 2 Byte (16-bit RGB555). Standard UO: 82374 Eintraege";
            this.lblRadarCountHint.Location = new System.Drawing.Point(315, 32);
            this.lblRadarCountHint.AutoSize = true;
            this.lblRadarCountHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblRadarCountHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblRadarCountHint.TabIndex = 2;
            this.grpRadarConfig.Controls.Add(this.lblRadarCountHint);

            this.grpRadarActions.Text = "Aktion";
            this.grpRadarActions.Location = new System.Drawing.Point(10, 123);
            this.grpRadarActions.Size = new System.Drawing.Size(920, 65);
            this.grpRadarActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpRadarActions.TabIndex = 1;
            this.tabPageRadarColor.Controls.Add(this.grpRadarActions);

            this.CreateFileButtonRadarColor.Text = "radarcol.mul erstellen";
            this.CreateFileButtonRadarColor.Location = new System.Drawing.Point(12, 22);
            this.CreateFileButtonRadarColor.Size = new System.Drawing.Size(220, 30);
            this.CreateFileButtonRadarColor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.CreateFileButtonRadarColor.TabIndex = 0;
            this.CreateFileButtonRadarColor.Click += new System.EventHandler(this.CreateFileButtonRadarColor_Click);
            this.grpRadarActions.Controls.Add(this.CreateFileButtonRadarColor);

            this.grpRadarOutput.Text = "Ausgabe";
            this.grpRadarOutput.Location = new System.Drawing.Point(10, 196);
            this.grpRadarOutput.Size = new System.Drawing.Size(920, 65);
            this.grpRadarOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpRadarOutput.TabIndex = 2;
            this.tabPageRadarColor.Controls.Add(this.grpRadarOutput);

            this.lbRadarColor.Text = "-";
            this.lbRadarColor.Location = new System.Drawing.Point(12, 22);
            this.lbRadarColor.AutoSize = true;
            this.lbRadarColor.Font = new System.Drawing.Font("Consolas", 9F);
            this.lbRadarColor.ForeColor = System.Drawing.Color.DarkGreen;
            this.lbRadarColor.TabIndex = 0;
            this.grpRadarOutput.Controls.Add(this.lbRadarColor);

            // ════════════════════════════════════════════════════════════════
            // TAB 7 – PALETTE
            // ════════════════════════════════════════════════════════════════
            this.tabPagePalette.Text = "Palette";
            this.tabPagePalette.Size = new System.Drawing.Size(946, 638);
            this.tabPagePalette.TabIndex = 6;
            this.tabPagePalette.UseVisualStyleBackColor = true;

            this.grpPaletteCreate.Text = "Palette erstellen  -  256 Farben";
            this.grpPaletteCreate.Location = new System.Drawing.Point(10, 10);
            this.grpPaletteCreate.Size = new System.Drawing.Size(920, 95);
            this.grpPaletteCreate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPaletteCreate.TabIndex = 0;
            this.tabPagePalette.Controls.Add(this.grpPaletteCreate);

            this.BtCreatePalette.Text = "Graustufen-Palette";
            this.BtCreatePalette.Location = new System.Drawing.Point(12, 25);
            this.BtCreatePalette.Size = new System.Drawing.Size(180, 30);
            this.BtCreatePalette.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreatePalette.TabIndex = 0;
            this.BtCreatePalette.Click += new System.EventHandler(this.BtCreatePalette_Click);
            this.grpPaletteCreate.Controls.Add(this.BtCreatePalette);

            this.lbCreatePalette.Text = "256 Graustufen  (#000 bis #FFF)";
            this.lbCreatePalette.Location = new System.Drawing.Point(200, 31);
            this.lbCreatePalette.AutoSize = true;
            this.lbCreatePalette.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbCreatePalette.ForeColor = System.Drawing.Color.DimGray;
            this.lbCreatePalette.TabIndex = 1;
            this.grpPaletteCreate.Controls.Add(this.lbCreatePalette);

            this.BtCreatePaletteFull.Text = "UO-Standard-Palette";
            this.BtCreatePaletteFull.Location = new System.Drawing.Point(12, 60);
            this.BtCreatePaletteFull.Size = new System.Drawing.Size(180, 30);
            this.BtCreatePaletteFull.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtCreatePaletteFull.TabIndex = 2;
            this.BtCreatePaletteFull.Click += new System.EventHandler(this.BtCreatePaletteFull_Click);
            this.grpPaletteCreate.Controls.Add(this.BtCreatePaletteFull);

            this.lbCreateColorPalette.Text = "UO-definierte Farben + Graustufenauffuellung bis 256";
            this.lbCreateColorPalette.Location = new System.Drawing.Point(200, 66);
            this.lbCreateColorPalette.AutoSize = true;
            this.lbCreateColorPalette.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbCreateColorPalette.ForeColor = System.Drawing.Color.DimGray;
            this.lbCreateColorPalette.TabIndex = 3;
            this.grpPaletteCreate.Controls.Add(this.lbCreateColorPalette);

            this.grpPaletteLoad.Text = "Palette laden und vorschauen";
            this.grpPaletteLoad.Location = new System.Drawing.Point(10, 113);
            this.grpPaletteLoad.Size = new System.Drawing.Size(920, 195);
            this.grpPaletteLoad.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPaletteLoad.TabIndex = 1;
            this.tabPagePalette.Controls.Add(this.grpPaletteLoad);

            this.LoadPaletteButton.Text = "Palette.mul laden ...";
            this.LoadPaletteButton.Location = new System.Drawing.Point(12, 26);
            this.LoadPaletteButton.Size = new System.Drawing.Size(200, 30);
            this.LoadPaletteButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.LoadPaletteButton.TabIndex = 0;
            this.LoadPaletteButton.Click += new System.EventHandler(this.LoadPaletteButton_Click);
            this.grpPaletteLoad.Controls.Add(this.LoadPaletteButton);

            this.lblPalettePreview.Text = "Vorschau (256 Farben):";
            this.lblPalettePreview.Location = new System.Drawing.Point(12, 65);
            this.lblPalettePreview.AutoSize = true;
            this.lblPalettePreview.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPalettePreview.TabIndex = 1;
            this.grpPaletteLoad.Controls.Add(this.lblPalettePreview);

            this.pictureBoxPalette.Location = new System.Drawing.Point(12, 83);
            this.pictureBoxPalette.Size = new System.Drawing.Size(890, 80);
            this.pictureBoxPalette.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPalette.TabIndex = 2;
            this.pictureBoxPalette.TabStop = false;
            this.grpPaletteLoad.Controls.Add(this.pictureBoxPalette);

            this.grpPaletteValues.Text = "RGB-Werte aller 256 Farben";
            this.grpPaletteValues.Location = new System.Drawing.Point(10, 316);
            this.grpPaletteValues.Size = new System.Drawing.Size(920, 230);
            this.grpPaletteValues.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPaletteValues.TabIndex = 2;
            this.tabPagePalette.Controls.Add(this.grpPaletteValues);

            this.textBoxRgbValues.Location = new System.Drawing.Point(12, 24);
            this.textBoxRgbValues.Size = new System.Drawing.Size(890, 195);
            this.textBoxRgbValues.Multiline = true;
            this.textBoxRgbValues.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxRgbValues.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxRgbValues.ReadOnly = true;
            this.textBoxRgbValues.TabIndex = 0;
            this.grpPaletteValues.Controls.Add(this.textBoxRgbValues);

            // ════════════════════════════════════════════════════════════════
            // TAB 8 – ANIMATION
            // ════════════════════════════════════════════════════════════════
            this.tabPageAnimation.Text = "Animation";
            this.tabPageAnimation.Size = new System.Drawing.Size(946, 638);
            this.tabPageAnimation.TabIndex = 7;
            this.tabPageAnimation.UseVisualStyleBackColor = true;

            this.grpAnimSource.Text = "1. Quell-Datei  (Anim.idx)";
            this.grpAnimSource.Location = new System.Drawing.Point(10, 10);
            this.grpAnimSource.Size = new System.Drawing.Size(460, 62);
            this.grpAnimSource.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimSource.TabIndex = 0;
            this.tabPageAnimation.Controls.Add(this.grpAnimSource);

            this.tbfilename.Location = new System.Drawing.Point(12, 28);
            this.tbfilename.Size = new System.Drawing.Size(315, 23);
            this.tbfilename.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbfilename.TabIndex = 0;
            this.grpAnimSource.Controls.Add(this.tbfilename);

            this.BtnBrowse.Text = "laden ...";
            this.BtnBrowse.Location = new System.Drawing.Point(340, 25);
            this.BtnBrowse.Size = new System.Drawing.Size(108, 26);
            this.BtnBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnBrowse.TabIndex = 1;
            this.BtnBrowse.Click += new System.EventHandler(this.BtnBrowseClick);
            this.grpAnimSource.Controls.Add(this.BtnBrowse);

            this.grpAnimOutput.Text = "2. Ziel-Verzeichnis und Dateiname";
            this.grpAnimOutput.Location = new System.Drawing.Point(480, 10);
            this.grpAnimOutput.Size = new System.Drawing.Size(456, 62);
            this.grpAnimOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimOutput.TabIndex = 1;
            this.tabPageAnimation.Controls.Add(this.grpAnimOutput);

            this.txtOutputDirectory.Location = new System.Drawing.Point(12, 28);
            this.txtOutputDirectory.Size = new System.Drawing.Size(220, 23);
            this.txtOutputDirectory.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOutputDirectory.TabIndex = 0;
            this.grpAnimOutput.Controls.Add(this.txtOutputDirectory);

            this.BtnSetOutputDirectory.Text = "waehlen";
            this.BtnSetOutputDirectory.Location = new System.Drawing.Point(240, 25);
            this.BtnSetOutputDirectory.Size = new System.Drawing.Size(90, 26);
            this.BtnSetOutputDirectory.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnSetOutputDirectory.TabIndex = 1;
            this.BtnSetOutputDirectory.Click += new System.EventHandler(this.BtnSetOutputDirectoryClick);
            this.grpAnimOutput.Controls.Add(this.BtnSetOutputDirectory);

            this.lblAnimSuffixLbl.Text = "Suffix:";
            this.lblAnimSuffixLbl.Location = new System.Drawing.Point(340, 31);
            this.lblAnimSuffixLbl.AutoSize = true;
            this.lblAnimSuffixLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblAnimSuffixLbl.TabIndex = 2;
            this.grpAnimOutput.Controls.Add(this.lblAnimSuffixLbl);

            this.txtOutputFilename.Location = new System.Drawing.Point(385, 28);
            this.txtOutputFilename.Size = new System.Drawing.Size(60, 23);
            this.txtOutputFilename.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtOutputFilename.TabIndex = 3;
            this.grpAnimOutput.Controls.Add(this.txtOutputFilename);

            this.grpAnimCreature.Text = "3. Creature-Einstellungen";
            this.grpAnimCreature.Location = new System.Drawing.Point(10, 80);
            this.grpAnimCreature.Size = new System.Drawing.Size(460, 145);
            this.grpAnimCreature.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimCreature.TabIndex = 2;
            this.tabPageAnimation.Controls.Add(this.grpAnimCreature);

            this.lblOrigIDHint.Text = "Quell-Creature-ID (Hex):";
            this.lblOrigIDHint.Location = new System.Drawing.Point(12, 30);
            this.lblOrigIDHint.AutoSize = true;
            this.lblOrigIDHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblOrigIDHint.TabIndex = 0;
            this.grpAnimCreature.Controls.Add(this.lblOrigIDHint);

            this.txtOrigCreatureID.Location = new System.Drawing.Point(200, 27);
            this.txtOrigCreatureID.Size = new System.Drawing.Size(100, 23);
            this.txtOrigCreatureID.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtOrigCreatureID.TabIndex = 1;
            this.grpAnimCreature.Controls.Add(this.txtOrigCreatureID);

            this.lblHexWarning.Text = "Achtung: Hex-Eingabe! z.B. 0A = ID 10";
            this.lblHexWarning.Location = new System.Drawing.Point(12, 55);
            this.lblHexWarning.AutoSize = true;
            this.lblHexWarning.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblHexWarning.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblHexWarning.TabIndex = 2;
            this.grpAnimCreature.Controls.Add(this.lblHexWarning);

            this.lblCopyCountHint.Text = "Anzahl Kopien (oder Checkbox):";
            this.lblCopyCountHint.Location = new System.Drawing.Point(12, 80);
            this.lblCopyCountHint.AutoSize = true;
            this.lblCopyCountHint.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblCopyCountHint.TabIndex = 3;
            this.grpAnimCreature.Controls.Add(this.lblCopyCountHint);

            this.txtNewCreatureID.Location = new System.Drawing.Point(220, 77);
            this.txtNewCreatureID.Size = new System.Drawing.Size(100, 23);
            this.txtNewCreatureID.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtNewCreatureID.TabIndex = 4;
            this.grpAnimCreature.Controls.Add(this.txtNewCreatureID);

            this.panelCheckbox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCheckbox.Location = new System.Drawing.Point(12, 108);
            this.panelCheckbox.Size = new System.Drawing.Size(435, 28);
            this.panelCheckbox.BackColor = System.Drawing.Color.AliceBlue;
            this.panelCheckbox.TabIndex = 5;
            this.grpAnimCreature.Controls.Add(this.panelCheckbox);

            this.lbCopys.Text = "Schnellwahl:";
            this.lbCopys.Location = new System.Drawing.Point(5, 6);
            this.lbCopys.AutoSize = true;
            this.lbCopys.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lbCopys.TabIndex = 0;
            this.panelCheckbox.Controls.Add(this.lbCopys);

            this.chkLowDetail.Text = "LowDetail x65";
            this.chkLowDetail.Location = new System.Drawing.Point(90, 5);
            this.chkLowDetail.AutoSize = true;
            this.chkLowDetail.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkLowDetail.TabIndex = 1;
            this.panelCheckbox.Controls.Add(this.chkLowDetail);

            this.chkHighDetail.Text = "HighDetail x110";
            this.chkHighDetail.Location = new System.Drawing.Point(200, 5);
            this.chkHighDetail.AutoSize = true;
            this.chkHighDetail.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkHighDetail.TabIndex = 2;
            this.panelCheckbox.Controls.Add(this.chkHighDetail);

            this.chkHuman.Text = "Human x175";
            this.chkHuman.Location = new System.Drawing.Point(320, 5);
            this.chkHuman.AutoSize = true;
            this.chkHuman.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.chkHuman.TabIndex = 3;
            this.panelCheckbox.Controls.Add(this.chkHuman);

            this.grpAnimActions.Text = "4. Aktionen";
            this.grpAnimActions.Location = new System.Drawing.Point(480, 80);
            this.grpAnimActions.Size = new System.Drawing.Size(456, 145);
            this.grpAnimActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimActions.TabIndex = 3;
            this.tabPageAnimation.Controls.Add(this.grpAnimActions);

            this.BtnNewAnimIDXFiles.Text = "Anim IDX erstellen (neu)";
            this.BtnNewAnimIDXFiles.Location = new System.Drawing.Point(10, 25);
            this.BtnNewAnimIDXFiles.Size = new System.Drawing.Size(260, 28);
            this.BtnNewAnimIDXFiles.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnNewAnimIDXFiles.TabIndex = 0;
            this.BtnNewAnimIDXFiles.Click += new System.EventHandler(this.BtnProcessClick);
            this.grpAnimActions.Controls.Add(this.BtnNewAnimIDXFiles);

            this.BtnProcessClickOld.Text = "Anim IDX erstellen (Alt)";
            this.BtnProcessClickOld.Location = new System.Drawing.Point(10, 61);
            this.BtnProcessClickOld.Size = new System.Drawing.Size(260, 28);
            this.BtnProcessClickOld.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnProcessClickOld.TabIndex = 1;
            this.BtnProcessClickOld.Click += new System.EventHandler(this.BtnProcessClickOldVersion);
            this.grpAnimActions.Controls.Add(this.BtnProcessClickOld);

            this.Button1.Text = "Leere anim.mul erstellen";
            this.Button1.Location = new System.Drawing.Point(10, 97);
            this.Button1.Size = new System.Drawing.Size(260, 28);
            this.Button1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Button1.TabIndex = 2;
            this.Button1.Click += new System.EventHandler(this.BtnSingleEmptyAnimMul_Click);
            this.grpAnimActions.Controls.Add(this.Button1);

            this.grpAnimInfo.Text = "Anim.idx lesen und analysieren";
            this.grpAnimInfo.Location = new System.Drawing.Point(10, 235);
            this.grpAnimInfo.Size = new System.Drawing.Size(460, 100);
            this.grpAnimInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimInfo.TabIndex = 4;
            this.tabPageAnimation.Controls.Add(this.grpAnimInfo);

            this.ReadAnimIdx.Text = "Eintraege anzeigen";
            this.ReadAnimIdx.Location = new System.Drawing.Point(12, 28);
            this.ReadAnimIdx.Size = new System.Drawing.Size(175, 28);
            this.ReadAnimIdx.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReadAnimIdx.TabIndex = 0;
            this.ReadAnimIdx.Click += new System.EventHandler(this.ReadAnimIdx_Click);
            this.grpAnimInfo.Controls.Add(this.ReadAnimIdx);

            this.btnCountIndices.Text = "Eintraege zaehlen";
            this.btnCountIndices.Location = new System.Drawing.Point(200, 28);
            this.btnCountIndices.Size = new System.Drawing.Size(150, 28);
            this.btnCountIndices.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.btnCountIndices.TabIndex = 1;
            this.btnCountIndices.Click += new System.EventHandler(this.BtnCountIndices_Click);
            this.grpAnimInfo.Controls.Add(this.btnCountIndices);

            this.BtnLoadAnimationMulData.Text = "Anim.mul + .idx laden";
            this.BtnLoadAnimationMulData.Location = new System.Drawing.Point(12, 62);
            this.BtnLoadAnimationMulData.Size = new System.Drawing.Size(200, 28);
            this.BtnLoadAnimationMulData.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnLoadAnimationMulData.TabIndex = 2;
            this.BtnLoadAnimationMulData.Click += new System.EventHandler(this.BtnLoadAnimationMulData_Click);
            this.grpAnimInfo.Controls.Add(this.BtnLoadAnimationMulData);

            this.txtData.Location = new System.Drawing.Point(222, 62);
            this.txtData.Size = new System.Drawing.Size(225, 28);
            this.txtData.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtData.ReadOnly = true;
            this.txtData.TabIndex = 3;
            this.grpAnimInfo.Controls.Add(this.txtData);

            this.grpAnimLog.Text = "Log / Ausgabe";
            this.grpAnimLog.Location = new System.Drawing.Point(10, 343);
            this.grpAnimLog.Size = new System.Drawing.Size(926, 250);
            this.grpAnimLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpAnimLog.TabIndex = 5;
            this.tabPageAnimation.Controls.Add(this.grpAnimLog);

            this.tbProcessAminidx.Location = new System.Drawing.Point(12, 24);
            this.tbProcessAminidx.Size = new System.Drawing.Size(800, 215);
            this.tbProcessAminidx.Multiline = true;
            this.tbProcessAminidx.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbProcessAminidx.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.tbProcessAminidx.ReadOnly = true;
            this.tbProcessAminidx.TabIndex = 0;
            this.grpAnimLog.Controls.Add(this.tbProcessAminidx);

            this.lblNewIdCount.Text = "Erstellte IDs: 0";
            this.lblNewIdCount.Location = new System.Drawing.Point(824, 24);
            this.lblNewIdCount.AutoSize = true;
            this.lblNewIdCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblNewIdCount.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblNewIdCount.TabIndex = 1;
            this.grpAnimLog.Controls.Add(this.lblNewIdCount);

            // ════════════════════════════════════════════════════════════════
            // TAB 9 – ARTMUL
            // ════════════════════════════════════════════════════════════════
            this.tabPageArtmul.Text = "Artmul";
            this.tabPageArtmul.Size = new System.Drawing.Size(946, 638);
            this.tabPageArtmul.TabIndex = 8;
            this.tabPageArtmul.UseVisualStyleBackColor = true;

            this.grpArtCreate.Text = "Schnell-Erstellung  -  vordefinierte Groessen";
            this.grpArtCreate.Location = new System.Drawing.Point(10, 10);
            this.grpArtCreate.Size = new System.Drawing.Size(295, 260);
            this.grpArtCreate.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpArtCreate.TabIndex = 0;
            this.tabPageArtmul.Controls.Add(this.grpArtCreate);

            this.lblArtCreateHint.Text = "Speichern-Dialog oeffnet sich automatisch.\r\nartidx.mul + art.mul werden erstellt.";
            this.lblArtCreateHint.Location = new System.Drawing.Point(12, 22);
            this.lblArtCreateHint.AutoSize = true;
            this.lblArtCreateHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblArtCreateHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblArtCreateHint.TabIndex = 0;
            this.grpArtCreate.Controls.Add(this.lblArtCreateHint);

            this.BtnCreateArtIdx100K.Text = "100.000 Eintraege";
            this.BtnCreateArtIdx100K.Location = new System.Drawing.Point(12, 55);
            this.BtnCreateArtIdx100K.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx100K.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx100K.TabIndex = 1;
            this.BtnCreateArtIdx100K.Click += new System.EventHandler(this.BtnCreateArtIdx100K_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx100K);

            this.BtnCreateArtIdx150K.Text = "150.000 Eintraege";
            this.BtnCreateArtIdx150K.Location = new System.Drawing.Point(12, 89);
            this.BtnCreateArtIdx150K.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx150K.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx150K.TabIndex = 2;
            this.BtnCreateArtIdx150K.Click += new System.EventHandler(this.BtnCreateArtIdx150K_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx150K);

            this.BtnCreateArtIdx200K.Text = "200.000 Eintraege";
            this.BtnCreateArtIdx200K.Location = new System.Drawing.Point(12, 123);
            this.BtnCreateArtIdx200K.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx200K.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx200K.TabIndex = 3;
            this.BtnCreateArtIdx200K.Click += new System.EventHandler(this.BtnCreateArtIdx200K_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx200K);

            this.BtnCreateArtIdx250K.Text = "250.000 Eintraege";
            this.BtnCreateArtIdx250K.Location = new System.Drawing.Point(12, 157);
            this.BtnCreateArtIdx250K.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx250K.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx250K.TabIndex = 4;
            this.BtnCreateArtIdx250K.Click += new System.EventHandler(this.BtnCreateArtIdx250K_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx250K);

            this.BtnCreateArtIdx500K.Text = "500.000 Eintraege";
            this.BtnCreateArtIdx500K.Location = new System.Drawing.Point(12, 191);
            this.BtnCreateArtIdx500K.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx500K.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx500K.TabIndex = 5;
            this.BtnCreateArtIdx500K.Click += new System.EventHandler(this.BtnCreateArtIdx500K_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx500K);

            this.BtnCreateArtIdx.Text = "1.000.000 Eintraege";
            this.BtnCreateArtIdx.Location = new System.Drawing.Point(12, 225);
            this.BtnCreateArtIdx.Size = new System.Drawing.Size(265, 28);
            this.BtnCreateArtIdx.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateArtIdx.TabIndex = 6;
            this.BtnCreateArtIdx.Click += new System.EventHandler(this.BtnCreateArtIdx_Click);
            this.grpArtCreate.Controls.Add(this.BtnCreateArtIdx);

            this.grpArtCustom.Text = "Freie Groesse eingeben";
            this.grpArtCustom.Location = new System.Drawing.Point(315, 10);
            this.grpArtCustom.Size = new System.Drawing.Size(275, 155);
            this.grpArtCustom.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpArtCustom.TabIndex = 1;
            this.tabPageArtmul.Controls.Add(this.grpArtCustom);

            this.lblArtCustomHint.Text = "Eintragszahl (Dezimal oder Hex 0x...):";
            this.lblArtCustomHint.Location = new System.Drawing.Point(12, 25);
            this.lblArtCustomHint.AutoSize = true;
            this.lblArtCustomHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblArtCustomHint.TabIndex = 0;
            this.grpArtCustom.Controls.Add(this.lblArtCustomHint);

            this.tbxNewIndex.Text = "65500";
            this.tbxNewIndex.Location = new System.Drawing.Point(12, 46);
            this.tbxNewIndex.Size = new System.Drawing.Size(150, 23);
            this.tbxNewIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbxNewIndex.TabIndex = 1;
            this.grpArtCustom.Controls.Add(this.tbxNewIndex);

            this.Button2.Text = "Erstellen";
            this.Button2.Location = new System.Drawing.Point(12, 76);
            this.Button2.Size = new System.Drawing.Size(150, 28);
            this.Button2.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Button2.TabIndex = 2;
            this.Button2.Click += new System.EventHandler(this.BtnCreateNewArtidx);
            this.grpArtCustom.Controls.Add(this.Button2);

            this.lblOldVersionHint.Text = "Alt-Format (2003-Stil):";
            this.lblOldVersionHint.Location = new System.Drawing.Point(12, 112);
            this.lblOldVersionHint.AutoSize = true;
            this.lblOldVersionHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblOldVersionHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblOldVersionHint.TabIndex = 3;
            this.grpArtCustom.Controls.Add(this.lblOldVersionHint);

            this.BtCreateOldVersionArtidx.Text = "Old Variant 2003";
            this.BtCreateOldVersionArtidx.Location = new System.Drawing.Point(12, 128);
            this.BtCreateOldVersionArtidx.Size = new System.Drawing.Size(150, 22);
            this.BtCreateOldVersionArtidx.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.BtCreateOldVersionArtidx.TabIndex = 4;
            this.BtCreateOldVersionArtidx.Click += new System.EventHandler(this.BtnCreateOldVersionArtidx);
            this.grpArtCustom.Controls.Add(this.BtCreateOldVersionArtidx);

            this.grpArtSplit.Text = "Arts-Anteil + LandTiles-Anteil definieren";
            this.grpArtSplit.Location = new System.Drawing.Point(315, 173);
            this.grpArtSplit.Size = new System.Drawing.Size(390, 140);
            this.grpArtSplit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpArtSplit.TabIndex = 2;
            this.tabPageArtmul.Controls.Add(this.grpArtSplit);

            this.lblArtSplitHint.Text = "Bedingung: ArtsCount + LandTilesCount = Index (Gesamt)";
            this.lblArtSplitHint.Location = new System.Drawing.Point(12, 22);
            this.lblArtSplitHint.AutoSize = true;
            this.lblArtSplitHint.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblArtSplitHint.ForeColor = System.Drawing.Color.OrangeRed;
            this.lblArtSplitHint.TabIndex = 0;
            this.grpArtSplit.Controls.Add(this.lblArtSplitHint);

            this.lblArtSplitTotalLbl.Text = "Gesamt-Index:";
            this.lblArtSplitTotalLbl.Location = new System.Drawing.Point(12, 48);
            this.lblArtSplitTotalLbl.AutoSize = true;
            this.lblArtSplitTotalLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblArtSplitTotalLbl.TabIndex = 1;
            this.grpArtSplit.Controls.Add(this.lblArtSplitTotalLbl);

            this.tbxNewIndex2.Text = "100000";
            this.tbxNewIndex2.Location = new System.Drawing.Point(12, 66);
            this.tbxNewIndex2.Size = new System.Drawing.Size(105, 23);
            this.tbxNewIndex2.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbxNewIndex2.TabIndex = 2;
            this.grpArtSplit.Controls.Add(this.tbxNewIndex2);

            this.lbArtsCount.Text = "ArtsCount:";
            this.lbArtsCount.Location = new System.Drawing.Point(125, 48);
            this.lbArtsCount.AutoSize = true;
            this.lbArtsCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbArtsCount.TabIndex = 3;
            this.grpArtSplit.Controls.Add(this.lbArtsCount);

            this.tbxArtsCount.Text = "70000";
            this.tbxArtsCount.Location = new System.Drawing.Point(125, 66);
            this.tbxArtsCount.Size = new System.Drawing.Size(105, 23);
            this.tbxArtsCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbxArtsCount.TabIndex = 4;
            this.grpArtSplit.Controls.Add(this.tbxArtsCount);

            this.lbLandTilesCount.Text = "LandTiles:";
            this.lbLandTilesCount.Location = new System.Drawing.Point(240, 48);
            this.lbLandTilesCount.AutoSize = true;
            this.lbLandTilesCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lbLandTilesCount.TabIndex = 5;
            this.grpArtSplit.Controls.Add(this.lbLandTilesCount);

            this.tbxLandTilesCount.Text = "30000";
            this.tbxLandTilesCount.Location = new System.Drawing.Point(240, 66);
            this.tbxLandTilesCount.Size = new System.Drawing.Size(105, 23);
            this.tbxLandTilesCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbxLandTilesCount.TabIndex = 6;
            this.grpArtSplit.Controls.Add(this.tbxLandTilesCount);

            this.Button3.Text = "Erstellen";
            this.Button3.Location = new System.Drawing.Point(12, 100);
            this.Button3.Size = new System.Drawing.Size(150, 28);
            this.Button3.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.Button3.TabIndex = 7;
            this.Button3.Click += new System.EventHandler(this.BtnCreateNewArtidx2);
            this.grpArtSplit.Controls.Add(this.Button3);

            this.grpArtRead.Text = "Artidx.mul lesen";
            this.grpArtRead.Location = new System.Drawing.Point(615, 10);
            this.grpArtRead.Size = new System.Drawing.Size(320, 130);
            this.grpArtRead.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpArtRead.TabIndex = 3;
            this.tabPageArtmul.Controls.Add(this.grpArtRead);

            this.lblArtReadHint.Text = "Datei waehlen, Info erscheint im Log:";
            this.lblArtReadHint.Location = new System.Drawing.Point(12, 22);
            this.lblArtReadHint.AutoSize = true;
            this.lblArtReadHint.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblArtReadHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblArtReadHint.TabIndex = 0;
            this.grpArtRead.Controls.Add(this.lblArtReadHint);

            this.ReadArtmul.Text = "Zusammenfassung";
            this.ReadArtmul.Location = new System.Drawing.Point(12, 42);
            this.ReadArtmul.Size = new System.Drawing.Size(290, 28);
            this.ReadArtmul.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReadArtmul.TabIndex = 1;
            this.ReadArtmul.Click += new System.EventHandler(this.BtnReadArtIdx_Click);
            this.grpArtRead.Controls.Add(this.ReadArtmul);

            this.ReadArtmul2.Text = "Detailliste (300 Zeilen)";
            this.ReadArtmul2.Location = new System.Drawing.Point(12, 76);
            this.ReadArtmul2.Size = new System.Drawing.Size(290, 28);
            this.ReadArtmul2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReadArtmul2.TabIndex = 2;
            this.ReadArtmul2.Click += new System.EventHandler(this.ReadArtmul2_Click);
            this.grpArtRead.Controls.Add(this.ReadArtmul2);

            this.lblIndexCount.Text = "-";
            this.lblIndexCount.Location = new System.Drawing.Point(615, 148);
            this.lblIndexCount.AutoSize = true;
            this.lblIndexCount.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblIndexCount.ForeColor = System.Drawing.Color.Navy;
            this.lblIndexCount.TabIndex = 4;
            this.tabPageArtmul.Controls.Add(this.lblIndexCount);

            this.grpArtLog.Text = "Log / Ausgabe";
            this.grpArtLog.Location = new System.Drawing.Point(10, 322);
            this.grpArtLog.Size = new System.Drawing.Size(926, 280);
            this.grpArtLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpArtLog.TabIndex = 5;
            this.tabPageArtmul.Controls.Add(this.grpArtLog);

            this.infoARTIDXMULID.Location = new System.Drawing.Point(12, 24);
            this.infoARTIDXMULID.Size = new System.Drawing.Size(898, 245);
            this.infoARTIDXMULID.Multiline = true;
            this.infoARTIDXMULID.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.infoARTIDXMULID.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.infoARTIDXMULID.ReadOnly = true;
            this.infoARTIDXMULID.TabIndex = 0;
            this.grpArtLog.Controls.Add(this.infoARTIDXMULID);

            // ════════════════════════════════════════════════════════════════
            // TAB 10 – SOUND
            // ════════════════════════════════════════════════════════════════
            this.tabPageSound.Text = "Sound";
            this.tabPageSound.Size = new System.Drawing.Size(946, 638);
            this.tabPageSound.TabIndex = 9;
            this.tabPageSound.UseVisualStyleBackColor = true;

            this.grpSoundConfig.Text = "Konfiguration  -  SoundIdx.mul + Sound.mul";
            this.grpSoundConfig.Location = new System.Drawing.Point(10, 10);
            this.grpSoundConfig.Size = new System.Drawing.Size(920, 115);
            this.grpSoundConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSoundConfig.TabIndex = 0;
            this.tabPageSound.Controls.Add(this.grpSoundConfig);

            this.lblSoundCountLbl.Text = "Anzahl Sound-Slots:";
            this.lblSoundCountLbl.Location = new System.Drawing.Point(12, 35);
            this.lblSoundCountLbl.AutoSize = true;
            this.lblSoundCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSoundCountLbl.TabIndex = 0;
            this.grpSoundConfig.Controls.Add(this.lblSoundCountLbl);

            this.SoundIDXMul.Text = "4095";
            this.SoundIDXMul.Location = new System.Drawing.Point(165, 32);
            this.SoundIDXMul.Size = new System.Drawing.Size(110, 23);
            this.SoundIDXMul.Font = new System.Drawing.Font("Consolas", 10F);
            this.SoundIDXMul.TabIndex = 1;
            this.grpSoundConfig.Controls.Add(this.SoundIDXMul);

            this.lblSoundCountHint.Text = "Standard UO: 4095 Slots\r\nJeder Slot = 12 Byte Index + 1024 Byte Platzhalter-Ton";
            this.lblSoundCountHint.Location = new System.Drawing.Point(290, 28);
            this.lblSoundCountHint.AutoSize = true;
            this.lblSoundCountHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblSoundCountHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblSoundCountHint.TabIndex = 2;
            this.grpSoundConfig.Controls.Add(this.lblSoundCountHint);

            this.grpSoundActions.Text = "Aktionen";
            this.grpSoundActions.Location = new System.Drawing.Point(10, 133);
            this.grpSoundActions.Size = new System.Drawing.Size(920, 70);
            this.grpSoundActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSoundActions.TabIndex = 1;
            this.tabPageSound.Controls.Add(this.grpSoundActions);

            this.CreateOrgSoundMul.Text = "SoundIdx.mul + Sound.mul erstellen";
            this.CreateOrgSoundMul.Location = new System.Drawing.Point(12, 22);
            this.CreateOrgSoundMul.Size = new System.Drawing.Size(300, 30);
            this.CreateOrgSoundMul.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.CreateOrgSoundMul.TabIndex = 0;
            this.CreateOrgSoundMul.Click += new System.EventHandler(this.CreateOrgSoundMul_Click);
            this.grpSoundActions.Controls.Add(this.CreateOrgSoundMul);

            this.ReadIndexSize.Text = "Eintraege zaehlen";
            this.ReadIndexSize.Location = new System.Drawing.Point(325, 22);
            this.ReadIndexSize.Size = new System.Drawing.Size(160, 30);
            this.ReadIndexSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReadIndexSize.TabIndex = 1;
            this.ReadIndexSize.Click += new System.EventHandler(this.ReadIndexSize_Click);
            this.grpSoundActions.Controls.Add(this.ReadIndexSize);

            this.grpSoundOutput.Text = "Ausgabe";
            this.grpSoundOutput.Location = new System.Drawing.Point(10, 211);
            this.grpSoundOutput.Size = new System.Drawing.Size(920, 60);
            this.grpSoundOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSoundOutput.TabIndex = 2;
            this.tabPageSound.Controls.Add(this.grpSoundOutput);

            this.IndexSizeLabel.Text = "-";
            this.IndexSizeLabel.Location = new System.Drawing.Point(12, 22);
            this.IndexSizeLabel.AutoSize = true;
            this.IndexSizeLabel.Font = new System.Drawing.Font("Consolas", 9F);
            this.IndexSizeLabel.ForeColor = System.Drawing.Color.DarkGreen;
            this.IndexSizeLabel.TabIndex = 0;
            this.grpSoundOutput.Controls.Add(this.IndexSizeLabel);

            // ════════════════════════════════════════════════════════════════
            // TAB 11 – GUMP
            // ════════════════════════════════════════════════════════════════
            this.tabPageGump.Text = "Gump";
            this.tabPageGump.Size = new System.Drawing.Size(946, 638);
            this.tabPageGump.TabIndex = 10;
            this.tabPageGump.UseVisualStyleBackColor = true;

            this.grpGumpConfig.Text = "Konfiguration  -  GUMPIDX.MUL + GUMPART.MUL";
            this.grpGumpConfig.Location = new System.Drawing.Point(10, 10);
            this.grpGumpConfig.Size = new System.Drawing.Size(920, 115);
            this.grpGumpConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpGumpConfig.TabIndex = 0;
            this.tabPageGump.Controls.Add(this.grpGumpConfig);

            this.lblGumpCountLbl.Text = "Anzahl Gump-Eintraege:";
            this.lblGumpCountLbl.Location = new System.Drawing.Point(12, 35);
            this.lblGumpCountLbl.AutoSize = true;
            this.lblGumpCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblGumpCountLbl.TabIndex = 0;
            this.grpGumpConfig.Controls.Add(this.lblGumpCountLbl);

            this.IndexSizeTextBox.Text = "65535";
            this.IndexSizeTextBox.Location = new System.Drawing.Point(185, 32);
            this.IndexSizeTextBox.Size = new System.Drawing.Size(110, 23);
            this.IndexSizeTextBox.Font = new System.Drawing.Font("Consolas", 10F);
            this.IndexSizeTextBox.TabIndex = 1;
            this.grpGumpConfig.Controls.Add(this.IndexSizeTextBox);

            this.lblGumpCountHint.Text = "Standard UO: 65535 Eintraege\r\nJeder Eintrag = 12 Byte in GUMPIDX.MUL.";
            this.lblGumpCountHint.Location = new System.Drawing.Point(310, 28);
            this.lblGumpCountHint.AutoSize = true;
            this.lblGumpCountHint.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblGumpCountHint.ForeColor = System.Drawing.Color.DimGray;
            this.lblGumpCountHint.TabIndex = 2;
            this.grpGumpConfig.Controls.Add(this.lblGumpCountHint);

            this.grpGumpActions.Text = "Aktionen";
            this.grpGumpActions.Location = new System.Drawing.Point(10, 133);
            this.grpGumpActions.Size = new System.Drawing.Size(920, 70);
            this.grpGumpActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpGumpActions.TabIndex = 1;
            this.tabPageGump.Controls.Add(this.grpGumpActions);

            this.CreateGumpButton.Text = "GUMPIDX.MUL + GUMPART.MUL erstellen";
            this.CreateGumpButton.Location = new System.Drawing.Point(12, 22);
            this.CreateGumpButton.Size = new System.Drawing.Size(330, 30);
            this.CreateGumpButton.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.CreateGumpButton.TabIndex = 0;
            this.CreateGumpButton.Click += new System.EventHandler(this.CreateGumpButton_Click);
            this.grpGumpActions.Controls.Add(this.CreateGumpButton);

            this.ReadGumpButton.Text = "Eintraege zaehlen";
            this.ReadGumpButton.Location = new System.Drawing.Point(355, 22);
            this.ReadGumpButton.Size = new System.Drawing.Size(160, 30);
            this.ReadGumpButton.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReadGumpButton.TabIndex = 1;
            this.ReadGumpButton.Click += new System.EventHandler(this.ReadGumpButton_Click);
            this.grpGumpActions.Controls.Add(this.ReadGumpButton);

            this.grpGumpOutput.Text = "Ausgabe";
            this.grpGumpOutput.Location = new System.Drawing.Point(10, 211);
            this.grpGumpOutput.Size = new System.Drawing.Size(920, 60);
            this.grpGumpOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpGumpOutput.TabIndex = 2;
            this.tabPageGump.Controls.Add(this.grpGumpOutput);

            this.gumpLabel.Text = "-";
            this.gumpLabel.Location = new System.Drawing.Point(12, 22);
            this.gumpLabel.AutoSize = true;
            this.gumpLabel.Font = new System.Drawing.Font("Consolas", 9F);
            this.gumpLabel.ForeColor = System.Drawing.Color.DarkGreen;
            this.gumpLabel.TabIndex = 0;
            this.grpGumpOutput.Controls.Add(this.gumpLabel);

            // ════════════════════════════════════════════════════════════════
            // TAB 12 – HUES
            // ════════════════════════════════════════════════════════════════
            this.tabPageHues.Text = "Hues";
            this.tabPageHues.Size = new System.Drawing.Size(946, 638);
            this.tabPageHues.TabIndex = 11;
            this.tabPageHues.UseVisualStyleBackColor = true;

            this.grpHuesActions.Text = "Aktionen  -  hues.mul  (3000 x 88 Byte Farbpaletten)";
            this.grpHuesActions.Location = new System.Drawing.Point(10, 10);
            this.grpHuesActions.Size = new System.Drawing.Size(920, 75);
            this.grpHuesActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHuesActions.TabIndex = 0;
            this.tabPageHues.Controls.Add(this.grpHuesActions);

            this.BtnCreateHues.Text = "Leere hues.mul erstellen (3000 Eintraege)";
            this.BtnCreateHues.Location = new System.Drawing.Point(12, 28);
            this.BtnCreateHues.Size = new System.Drawing.Size(320, 30);
            this.BtnCreateHues.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnCreateHues.TabIndex = 0;
            this.BtnCreateHues.Click += new System.EventHandler(this.BtnCreateHues_Click);
            this.grpHuesActions.Controls.Add(this.BtnCreateHues);

            this.BtnReadHues.Text = "hues.mul lesen";
            this.BtnReadHues.Location = new System.Drawing.Point(345, 28);
            this.BtnReadHues.Size = new System.Drawing.Size(180, 30);
            this.BtnReadHues.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnReadHues.TabIndex = 1;
            this.BtnReadHues.Click += new System.EventHandler(this.BtnReadHues_Click);
            this.grpHuesActions.Controls.Add(this.BtnReadHues);

            this.grpHuesOutput.Text = "Ausgabe";
            this.grpHuesOutput.Location = new System.Drawing.Point(10, 93);
            this.grpHuesOutput.Size = new System.Drawing.Size(920, 510);
            this.grpHuesOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHuesOutput.TabIndex = 1;
            this.tabPageHues.Controls.Add(this.grpHuesOutput);

            this.lblHuesOutput.Text = "-";
            this.lblHuesOutput.Location = new System.Drawing.Point(12, 22);
            this.lblHuesOutput.Size = new System.Drawing.Size(890, 475);
            this.lblHuesOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblHuesOutput.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblHuesOutput.TabIndex = 0;
            this.grpHuesOutput.Controls.Add(this.lblHuesOutput);

            // ════════════════════════════════════════════════════════════════
            // TAB 13 – MAP / STATICS
            // ════════════════════════════════════════════════════════════════
            this.tabPageMap.Text = "Map/Statics";
            this.tabPageMap.Size = new System.Drawing.Size(946, 638);
            this.tabPageMap.TabIndex = 12;
            this.tabPageMap.UseVisualStyleBackColor = true;

            this.grpMapConfig.Text = "Kartenkonfiguration";
            this.grpMapConfig.Location = new System.Drawing.Point(10, 10);
            this.grpMapConfig.Size = new System.Drawing.Size(920, 175);
            this.grpMapConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMapConfig.TabIndex = 0;
            this.tabPageMap.Controls.Add(this.grpMapConfig);

            this.lblMapSizeComboLbl.Text = "Voreinstellung:";
            this.lblMapSizeComboLbl.Location = new System.Drawing.Point(12, 30);
            this.lblMapSizeComboLbl.AutoSize = true;
            this.lblMapSizeComboLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMapSizeComboLbl.TabIndex = 0;
            this.grpMapConfig.Controls.Add(this.lblMapSizeComboLbl);

            this.comboMapSize.Location = new System.Drawing.Point(110, 27);
            this.comboMapSize.Size = new System.Drawing.Size(340, 23);
            this.comboMapSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.comboMapSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMapSize.TabIndex = 1;
            this.comboMapSize.SelectedIndexChanged += new System.EventHandler(this.ComboMapSize_SelectedIndexChanged);
            this.grpMapConfig.Controls.Add(this.comboMapSize);

            this.lblMapWidthLbl.Text = "Breite (Tiles):";
            this.lblMapWidthLbl.Location = new System.Drawing.Point(12, 65);
            this.lblMapWidthLbl.AutoSize = true;
            this.lblMapWidthLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMapWidthLbl.TabIndex = 2;
            this.grpMapConfig.Controls.Add(this.lblMapWidthLbl);

            this.tbMapWidth.Text = "7168";
            this.tbMapWidth.Location = new System.Drawing.Point(110, 62);
            this.tbMapWidth.Size = new System.Drawing.Size(110, 23);
            this.tbMapWidth.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbMapWidth.TabIndex = 3;
            this.tbMapWidth.TextChanged += new System.EventHandler(this.TbMapWidth_TextChanged);
            this.grpMapConfig.Controls.Add(this.tbMapWidth);

            this.lblMapHeightLbl.Text = "Hoehe (Tiles):";
            this.lblMapHeightLbl.Location = new System.Drawing.Point(240, 65);
            this.lblMapHeightLbl.AutoSize = true;
            this.lblMapHeightLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMapHeightLbl.TabIndex = 4;
            this.grpMapConfig.Controls.Add(this.lblMapHeightLbl);

            this.tbMapHeight.Text = "4096";
            this.tbMapHeight.Location = new System.Drawing.Point(345, 62);
            this.tbMapHeight.Size = new System.Drawing.Size(110, 23);
            this.tbMapHeight.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbMapHeight.TabIndex = 5;
            this.tbMapHeight.TextChanged += new System.EventHandler(this.TbMapWidth_TextChanged);
            this.grpMapConfig.Controls.Add(this.tbMapHeight);

            this.lblMapIndexLbl.Text = "Karten-Index:";
            this.lblMapIndexLbl.Location = new System.Drawing.Point(480, 65);
            this.lblMapIndexLbl.AutoSize = true;
            this.lblMapIndexLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMapIndexLbl.TabIndex = 6;
            this.grpMapConfig.Controls.Add(this.lblMapIndexLbl);

            this.tbMapIndex.Text = "0";
            this.tbMapIndex.Location = new System.Drawing.Point(580, 62);
            this.tbMapIndex.Size = new System.Drawing.Size(60, 23);
            this.tbMapIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbMapIndex.TabIndex = 7;
            this.grpMapConfig.Controls.Add(this.tbMapIndex);

            this.lblMapSizeInfo.Text = "-";
            this.lblMapSizeInfo.Location = new System.Drawing.Point(12, 100);
            this.lblMapSizeInfo.Size = new System.Drawing.Size(890, 60);
            this.lblMapSizeInfo.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblMapSizeInfo.ForeColor = System.Drawing.Color.Navy;
            this.lblMapSizeInfo.TabIndex = 8;
            this.grpMapConfig.Controls.Add(this.lblMapSizeInfo);

            this.grpMapActions.Text = "Aktionen";
            this.grpMapActions.Location = new System.Drawing.Point(10, 193);
            this.grpMapActions.Size = new System.Drawing.Size(920, 75);
            this.grpMapActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMapActions.TabIndex = 1;
            this.tabPageMap.Controls.Add(this.grpMapActions);

            this.BtnCreateMap.Text = "Nur map*.mul erstellen";
            this.BtnCreateMap.Location = new System.Drawing.Point(12, 28);
            this.BtnCreateMap.Size = new System.Drawing.Size(220, 30);
            this.BtnCreateMap.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnCreateMap.TabIndex = 0;
            this.BtnCreateMap.Click += new System.EventHandler(this.BtnCreateMap_Click);
            this.grpMapActions.Controls.Add(this.BtnCreateMap);

            this.BtnCreateStatics.Text = "Nur statics*.mul + staidx*.mul";
            this.BtnCreateStatics.Location = new System.Drawing.Point(245, 28);
            this.BtnCreateStatics.Size = new System.Drawing.Size(255, 30);
            this.BtnCreateStatics.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateStatics.TabIndex = 1;
            this.BtnCreateStatics.Click += new System.EventHandler(this.BtnCreateStatics_Click);
            this.grpMapActions.Controls.Add(this.BtnCreateStatics);

            this.BtnCreateMapAndStatics.Text = "Map + Statics zusammen erstellen";
            this.BtnCreateMapAndStatics.Location = new System.Drawing.Point(515, 28);
            this.BtnCreateMapAndStatics.Size = new System.Drawing.Size(295, 30);
            this.BtnCreateMapAndStatics.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateMapAndStatics.TabIndex = 2;
            this.BtnCreateMapAndStatics.Click += new System.EventHandler(this.BtnCreateMapAndStatics_Click);
            this.grpMapActions.Controls.Add(this.BtnCreateMapAndStatics);

            this.grpMapOutput.Text = "Ausgabe";
            this.grpMapOutput.Location = new System.Drawing.Point(10, 276);
            this.grpMapOutput.Size = new System.Drawing.Size(920, 330);
            this.grpMapOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMapOutput.TabIndex = 2;
            this.tabPageMap.Controls.Add(this.grpMapOutput);

            this.lblMapOutput.Text = "-";
            this.lblMapOutput.Location = new System.Drawing.Point(12, 22);
            this.lblMapOutput.Size = new System.Drawing.Size(890, 295);
            this.lblMapOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblMapOutput.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblMapOutput.TabIndex = 0;
            this.grpMapOutput.Controls.Add(this.lblMapOutput);

            // ════════════════════════════════════════════════════════════════
            // TAB 14 – MULTI
            // ════════════════════════════════════════════════════════════════
            this.tabPageMulti.Text = "Multi";
            this.tabPageMulti.Size = new System.Drawing.Size(946, 638);
            this.tabPageMulti.TabIndex = 13;
            this.tabPageMulti.UseVisualStyleBackColor = true;

            this.grpMultiConfig.Text = "Konfiguration  -  multi.mul + multi.idx";
            this.grpMultiConfig.Location = new System.Drawing.Point(10, 10);
            this.grpMultiConfig.Size = new System.Drawing.Size(920, 110);
            this.grpMultiConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMultiConfig.TabIndex = 0;
            this.tabPageMulti.Controls.Add(this.grpMultiConfig);

            this.lblMultiCountLbl.Text = "Anzahl Multi-Eintraege:";
            this.lblMultiCountLbl.Location = new System.Drawing.Point(12, 30);
            this.lblMultiCountLbl.AutoSize = true;
            this.lblMultiCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMultiCountLbl.TabIndex = 0;
            this.grpMultiConfig.Controls.Add(this.lblMultiCountLbl);

            this.tbMultiCount.Text = "5000";
            this.tbMultiCount.Location = new System.Drawing.Point(190, 27);
            this.tbMultiCount.Size = new System.Drawing.Size(110, 23);
            this.tbMultiCount.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbMultiCount.TabIndex = 1;
            this.grpMultiConfig.Controls.Add(this.tbMultiCount);

            this.lblMultiIndexLbl.Text = "Lese-Index:";
            this.lblMultiIndexLbl.Location = new System.Drawing.Point(12, 65);
            this.lblMultiIndexLbl.AutoSize = true;
            this.lblMultiIndexLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblMultiIndexLbl.TabIndex = 2;
            this.grpMultiConfig.Controls.Add(this.lblMultiIndexLbl);

            this.tbMultiIndex.Text = "0";
            this.tbMultiIndex.Location = new System.Drawing.Point(100, 62);
            this.tbMultiIndex.Size = new System.Drawing.Size(90, 23);
            this.tbMultiIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbMultiIndex.TabIndex = 3;
            this.grpMultiConfig.Controls.Add(this.tbMultiIndex);

            this.checkBoxMultiHS.Text = "HighSeas-Format (16 Byte pro Tile statt 12)";
            this.checkBoxMultiHS.Location = new System.Drawing.Point(210, 63);
            this.checkBoxMultiHS.AutoSize = true;
            this.checkBoxMultiHS.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxMultiHS.TabIndex = 4;
            this.grpMultiConfig.Controls.Add(this.checkBoxMultiHS);

            this.grpMultiActions.Text = "Aktionen";
            this.grpMultiActions.Location = new System.Drawing.Point(10, 128);
            this.grpMultiActions.Size = new System.Drawing.Size(920, 75);
            this.grpMultiActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMultiActions.TabIndex = 1;
            this.tabPageMulti.Controls.Add(this.grpMultiActions);

            this.BtnCreateMulti.Text = "multi.mul + multi.idx erstellen";
            this.BtnCreateMulti.Location = new System.Drawing.Point(12, 28);
            this.BtnCreateMulti.Size = new System.Drawing.Size(280, 30);
            this.BtnCreateMulti.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnCreateMulti.TabIndex = 0;
            this.BtnCreateMulti.Click += new System.EventHandler(this.BtnCreateMulti_Click);
            this.grpMultiActions.Controls.Add(this.BtnCreateMulti);

            this.BtnReadMulti.Text = "Multi-Eintrag lesen";
            this.BtnReadMulti.Location = new System.Drawing.Point(305, 28);
            this.BtnReadMulti.Size = new System.Drawing.Size(200, 30);
            this.BtnReadMulti.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnReadMulti.TabIndex = 1;
            this.BtnReadMulti.Click += new System.EventHandler(this.BtnReadMulti_Click);
            this.grpMultiActions.Controls.Add(this.BtnReadMulti);

            this.grpMultiOutput.Text = "Ausgabe";
            this.grpMultiOutput.Location = new System.Drawing.Point(10, 211);
            this.grpMultiOutput.Size = new System.Drawing.Size(920, 400);
            this.grpMultiOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpMultiOutput.TabIndex = 2;
            this.tabPageMulti.Controls.Add(this.grpMultiOutput);

            this.lblMultiOutput.Text = "-";
            this.lblMultiOutput.Location = new System.Drawing.Point(12, 22);
            this.lblMultiOutput.Size = new System.Drawing.Size(890, 365);
            this.lblMultiOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblMultiOutput.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblMultiOutput.TabIndex = 0;
            this.grpMultiOutput.Controls.Add(this.lblMultiOutput);

            // ════════════════════════════════════════════════════════════════
            // TAB 15 – SKILLS
            // ════════════════════════════════════════════════════════════════
            this.tabPageSkills.Text = "Skills";
            this.tabPageSkills.Size = new System.Drawing.Size(946, 638);
            this.tabPageSkills.TabIndex = 14;
            this.tabPageSkills.UseVisualStyleBackColor = true;

            this.grpSkillsConfig.Text = "Konfiguration  -  skills.mul + skills.idx";
            this.grpSkillsConfig.Location = new System.Drawing.Point(10, 10);
            this.grpSkillsConfig.Size = new System.Drawing.Size(920, 75);
            this.grpSkillsConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSkillsConfig.TabIndex = 0;
            this.tabPageSkills.Controls.Add(this.grpSkillsConfig);

            this.lblSkillCountLbl.Text = "Anzahl Skills (leere Variante):";
            this.lblSkillCountLbl.Location = new System.Drawing.Point(12, 32);
            this.lblSkillCountLbl.AutoSize = true;
            this.lblSkillCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSkillCountLbl.TabIndex = 0;
            this.grpSkillsConfig.Controls.Add(this.lblSkillCountLbl);

            this.tbSkillCount.Text = "58";
            this.tbSkillCount.Location = new System.Drawing.Point(245, 29);
            this.tbSkillCount.Size = new System.Drawing.Size(90, 23);
            this.tbSkillCount.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbSkillCount.TabIndex = 1;
            this.grpSkillsConfig.Controls.Add(this.tbSkillCount);

            this.grpSkillsActions.Text = "Aktionen";
            this.grpSkillsActions.Location = new System.Drawing.Point(10, 93);
            this.grpSkillsActions.Size = new System.Drawing.Size(920, 75);
            this.grpSkillsActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSkillsActions.TabIndex = 1;
            this.tabPageSkills.Controls.Add(this.grpSkillsActions);

            this.BtnCreateDefaultSkills.Text = "Standard-Skills erstellen (58 UO-Skills)";
            this.BtnCreateDefaultSkills.Location = new System.Drawing.Point(12, 28);
            this.BtnCreateDefaultSkills.Size = new System.Drawing.Size(310, 30);
            this.BtnCreateDefaultSkills.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnCreateDefaultSkills.TabIndex = 0;
            this.BtnCreateDefaultSkills.Click += new System.EventHandler(this.BtnCreateDefaultSkills_Click);
            this.grpSkillsActions.Controls.Add(this.BtnCreateDefaultSkills);

            this.BtnCreateEmptySkills.Text = "Leere Skills erstellen";
            this.BtnCreateEmptySkills.Location = new System.Drawing.Point(335, 28);
            this.BtnCreateEmptySkills.Size = new System.Drawing.Size(200, 30);
            this.BtnCreateEmptySkills.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCreateEmptySkills.TabIndex = 1;
            this.BtnCreateEmptySkills.Click += new System.EventHandler(this.BtnCreateEmptySkills_Click);
            this.grpSkillsActions.Controls.Add(this.BtnCreateEmptySkills);

            this.BtnReadSkills.Text = "Skills lesen";
            this.BtnReadSkills.Location = new System.Drawing.Point(548, 28);
            this.BtnReadSkills.Size = new System.Drawing.Size(160, 30);
            this.BtnReadSkills.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnReadSkills.TabIndex = 2;
            this.BtnReadSkills.Click += new System.EventHandler(this.BtnReadSkills_Click);
            this.grpSkillsActions.Controls.Add(this.BtnReadSkills);

            this.grpSkillsOutput.Text = "Ausgabe";
            this.grpSkillsOutput.Location = new System.Drawing.Point(10, 176);
            this.grpSkillsOutput.Size = new System.Drawing.Size(920, 435);
            this.grpSkillsOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpSkillsOutput.TabIndex = 2;
            this.tabPageSkills.Controls.Add(this.grpSkillsOutput);

            this.lblSkillsOutput.Text = "-";
            this.lblSkillsOutput.Location = new System.Drawing.Point(12, 22);
            this.lblSkillsOutput.AutoSize = true;
            this.lblSkillsOutput.Font = new System.Drawing.Font("Consolas", 9F);
            this.lblSkillsOutput.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblSkillsOutput.TabIndex = 0;
            this.grpSkillsOutput.Controls.Add(this.lblSkillsOutput);

            this.textBoxSkillsInfo.Location = new System.Drawing.Point(12, 50);
            this.textBoxSkillsInfo.Size = new System.Drawing.Size(890, 370);
            this.textBoxSkillsInfo.Multiline = true;
            this.textBoxSkillsInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSkillsInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxSkillsInfo.ReadOnly = true;
            this.textBoxSkillsInfo.TabIndex = 1;
            this.grpSkillsOutput.Controls.Add(this.textBoxSkillsInfo);

            // ════════════════════════════════════════════════════════════════
            // TAB 16 – VALIDATOR
            // ════════════════════════════════════════════════════════════════
            this.tabPageValidator.Text = "Validator";
            this.tabPageValidator.Size = new System.Drawing.Size(946, 638);
            this.tabPageValidator.TabIndex = 15;
            this.tabPageValidator.UseVisualStyleBackColor = true;

            this.grpValidatorActions.Text = "IDX <-> MUL Konsistenz-Pruefung";
            this.grpValidatorActions.Location = new System.Drawing.Point(10, 10);
            this.grpValidatorActions.Size = new System.Drawing.Size(920, 80);
            this.grpValidatorActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpValidatorActions.TabIndex = 0;
            this.tabPageValidator.Controls.Add(this.grpValidatorActions);

            this.BtnValidate.Text = "IDX + MUL validieren";
            this.BtnValidate.Location = new System.Drawing.Point(12, 28);
            this.BtnValidate.Size = new System.Drawing.Size(220, 30);
            this.BtnValidate.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnValidate.TabIndex = 0;
            this.BtnValidate.Click += new System.EventHandler(this.BtnValidate_Click);
            this.grpValidatorActions.Controls.Add(this.BtnValidate);

            this.BtnCompareDirectories.Text = "Zwei Verzeichnisse vergleichen";
            this.BtnCompareDirectories.Location = new System.Drawing.Point(245, 28);
            this.BtnCompareDirectories.Size = new System.Drawing.Size(270, 30);
            this.BtnCompareDirectories.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnCompareDirectories.TabIndex = 1;
            this.BtnCompareDirectories.Click += new System.EventHandler(this.BtnCompareDirectories_Click);
            this.grpValidatorActions.Controls.Add(this.BtnCompareDirectories);

            this.lblValidatorStatus.Text = "-";
            this.lblValidatorStatus.Location = new System.Drawing.Point(530, 33);
            this.lblValidatorStatus.AutoSize = true;
            this.lblValidatorStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblValidatorStatus.ForeColor = System.Drawing.Color.Navy;
            this.lblValidatorStatus.TabIndex = 2;
            this.grpValidatorActions.Controls.Add(this.lblValidatorStatus);

            this.grpValidatorOutput.Text = "Ergebnis";
            this.grpValidatorOutput.Location = new System.Drawing.Point(10, 98);
            this.grpValidatorOutput.Size = new System.Drawing.Size(920, 512);
            this.grpValidatorOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpValidatorOutput.TabIndex = 1;
            this.tabPageValidator.Controls.Add(this.grpValidatorOutput);

            this.textBoxValidatorOutput.Location = new System.Drawing.Point(12, 24);
            this.textBoxValidatorOutput.Size = new System.Drawing.Size(890, 478);
            this.textBoxValidatorOutput.Multiline = true;
            this.textBoxValidatorOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxValidatorOutput.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxValidatorOutput.ReadOnly = true;
            this.textBoxValidatorOutput.TabIndex = 0;
            this.grpValidatorOutput.Controls.Add(this.textBoxValidatorOutput);

            // ════════════════════════════════════════════════════════════════
            // TAB 17 – IDX PATCHER
            // ════════════════════════════════════════════════════════════════
            this.tabPageIdxPatcher.Text = "IDX Patcher";
            this.tabPageIdxPatcher.Size = new System.Drawing.Size(946, 638);
            this.tabPageIdxPatcher.TabIndex = 16;
            this.tabPageIdxPatcher.UseVisualStyleBackColor = true;

            this.grpPatcherFile.Text = "IDX-Datei";
            this.grpPatcherFile.Location = new System.Drawing.Point(10, 10);
            this.grpPatcherFile.Size = new System.Drawing.Size(920, 65);
            this.grpPatcherFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPatcherFile.TabIndex = 0;
            this.tabPageIdxPatcher.Controls.Add(this.grpPatcherFile);

            this.lblPatchIdxLbl.Text = "IDX-Datei:";
            this.lblPatchIdxLbl.Location = new System.Drawing.Point(12, 30);
            this.lblPatchIdxLbl.AutoSize = true;
            this.lblPatchIdxLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchIdxLbl.TabIndex = 0;
            this.grpPatcherFile.Controls.Add(this.lblPatchIdxLbl);

            this.tbPatchIdxPath.Location = new System.Drawing.Point(85, 27);
            this.tbPatchIdxPath.Size = new System.Drawing.Size(680, 23);
            this.tbPatchIdxPath.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbPatchIdxPath.TabIndex = 1;
            this.grpPatcherFile.Controls.Add(this.tbPatchIdxPath);

            this.BtnPatchBrowseIdx.Text = "...";
            this.BtnPatchBrowseIdx.Location = new System.Drawing.Point(775, 25);
            this.BtnPatchBrowseIdx.Size = new System.Drawing.Size(130, 26);
            this.BtnPatchBrowseIdx.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnPatchBrowseIdx.TabIndex = 2;
            this.BtnPatchBrowseIdx.Click += new System.EventHandler(this.BtnPatchBrowseIdx_Click);
            this.grpPatcherFile.Controls.Add(this.BtnPatchBrowseIdx);

            this.grpPatcherEdit.Text = "Eintrag bearbeiten";
            this.grpPatcherEdit.Location = new System.Drawing.Point(10, 83);
            this.grpPatcherEdit.Size = new System.Drawing.Size(920, 100);
            this.grpPatcherEdit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPatcherEdit.TabIndex = 1;
            this.tabPageIdxPatcher.Controls.Add(this.grpPatcherEdit);

            this.lblPatchIndexLbl.Text = "Index:";
            this.lblPatchIndexLbl.Location = new System.Drawing.Point(12, 30);
            this.lblPatchIndexLbl.AutoSize = true;
            this.lblPatchIndexLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchIndexLbl.TabIndex = 0;
            this.grpPatcherEdit.Controls.Add(this.lblPatchIndexLbl);

            this.tbPatchIndex.Text = "0";
            this.tbPatchIndex.Location = new System.Drawing.Point(60, 27);
            this.tbPatchIndex.Size = new System.Drawing.Size(90, 23);
            this.tbPatchIndex.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchIndex.TabIndex = 1;
            this.grpPatcherEdit.Controls.Add(this.tbPatchIndex);

            this.lblPatchLookupLbl.Text = "Lookup:";
            this.lblPatchLookupLbl.Location = new System.Drawing.Point(170, 30);
            this.lblPatchLookupLbl.AutoSize = true;
            this.lblPatchLookupLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchLookupLbl.TabIndex = 2;
            this.grpPatcherEdit.Controls.Add(this.lblPatchLookupLbl);

            this.tbPatchLookup.Text = "0x0";
            this.tbPatchLookup.Location = new System.Drawing.Point(230, 27);
            this.tbPatchLookup.Size = new System.Drawing.Size(130, 23);
            this.tbPatchLookup.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchLookup.TabIndex = 3;
            this.grpPatcherEdit.Controls.Add(this.tbPatchLookup);

            this.lblPatchSizeLbl.Text = "Size:";
            this.lblPatchSizeLbl.Location = new System.Drawing.Point(380, 30);
            this.lblPatchSizeLbl.AutoSize = true;
            this.lblPatchSizeLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchSizeLbl.TabIndex = 4;
            this.grpPatcherEdit.Controls.Add(this.lblPatchSizeLbl);

            this.tbPatchSize.Text = "0";
            this.tbPatchSize.Location = new System.Drawing.Point(420, 27);
            this.tbPatchSize.Size = new System.Drawing.Size(110, 23);
            this.tbPatchSize.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchSize.TabIndex = 5;
            this.grpPatcherEdit.Controls.Add(this.tbPatchSize);

            this.lblPatchUnknownLbl.Text = "Unknown:";
            this.lblPatchUnknownLbl.Location = new System.Drawing.Point(550, 30);
            this.lblPatchUnknownLbl.AutoSize = true;
            this.lblPatchUnknownLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchUnknownLbl.TabIndex = 6;
            this.grpPatcherEdit.Controls.Add(this.lblPatchUnknownLbl);

            this.tbPatchUnknown.Text = "0";
            this.tbPatchUnknown.Location = new System.Drawing.Point(625, 27);
            this.tbPatchUnknown.Size = new System.Drawing.Size(100, 23);
            this.tbPatchUnknown.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchUnknown.TabIndex = 7;
            this.grpPatcherEdit.Controls.Add(this.tbPatchUnknown);

            this.BtnPatchEntry.Text = "Eintrag patchen";
            this.BtnPatchEntry.Location = new System.Drawing.Point(12, 58);
            this.BtnPatchEntry.Size = new System.Drawing.Size(180, 28);
            this.BtnPatchEntry.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnPatchEntry.TabIndex = 8;
            this.BtnPatchEntry.Click += new System.EventHandler(this.BtnPatchEntry_Click);
            this.grpPatcherEdit.Controls.Add(this.BtnPatchEntry);

            this.BtnClearEntry.Text = "Eintrag leeren";
            this.BtnClearEntry.Location = new System.Drawing.Point(205, 58);
            this.BtnClearEntry.Size = new System.Drawing.Size(160, 28);
            this.BtnClearEntry.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnClearEntry.TabIndex = 9;
            this.BtnClearEntry.Click += new System.EventHandler(this.BtnClearEntry_Click);
            this.grpPatcherEdit.Controls.Add(this.BtnClearEntry);

            this.grpPatcherRange.Text = "Eintraege-Bereich lesen";
            this.grpPatcherRange.Location = new System.Drawing.Point(10, 191);
            this.grpPatcherRange.Size = new System.Drawing.Size(920, 75);
            this.grpPatcherRange.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPatcherRange.TabIndex = 2;
            this.tabPageIdxPatcher.Controls.Add(this.grpPatcherRange);

            this.lblPatchRangeFromLbl.Text = "Von Index:";
            this.lblPatchRangeFromLbl.Location = new System.Drawing.Point(12, 30);
            this.lblPatchRangeFromLbl.AutoSize = true;
            this.lblPatchRangeFromLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchRangeFromLbl.TabIndex = 0;
            this.grpPatcherRange.Controls.Add(this.lblPatchRangeFromLbl);

            this.tbPatchRangeFrom.Text = "0";
            this.tbPatchRangeFrom.Location = new System.Drawing.Point(90, 27);
            this.tbPatchRangeFrom.Size = new System.Drawing.Size(90, 23);
            this.tbPatchRangeFrom.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchRangeFrom.TabIndex = 1;
            this.grpPatcherRange.Controls.Add(this.tbPatchRangeFrom);

            this.lblPatchRangeCountLbl.Text = "Anzahl:";
            this.lblPatchRangeCountLbl.Location = new System.Drawing.Point(200, 30);
            this.lblPatchRangeCountLbl.AutoSize = true;
            this.lblPatchRangeCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblPatchRangeCountLbl.TabIndex = 2;
            this.grpPatcherRange.Controls.Add(this.lblPatchRangeCountLbl);

            this.tbPatchRangeCount.Text = "20";
            this.tbPatchRangeCount.Location = new System.Drawing.Point(255, 27);
            this.tbPatchRangeCount.Size = new System.Drawing.Size(90, 23);
            this.tbPatchRangeCount.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbPatchRangeCount.TabIndex = 3;
            this.grpPatcherRange.Controls.Add(this.tbPatchRangeCount);

            this.BtnReadRange.Text = "Bereich anzeigen";
            this.BtnReadRange.Location = new System.Drawing.Point(360, 25);
            this.BtnReadRange.Size = new System.Drawing.Size(180, 28);
            this.BtnReadRange.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnReadRange.TabIndex = 4;
            this.BtnReadRange.Click += new System.EventHandler(this.BtnReadRange_Click);
            this.grpPatcherRange.Controls.Add(this.BtnReadRange);

            this.grpPatcherOutput.Text = "Ausgabe";
            this.grpPatcherOutput.Location = new System.Drawing.Point(10, 274);
            this.grpPatcherOutput.Size = new System.Drawing.Size(920, 337);
            this.grpPatcherOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpPatcherOutput.TabIndex = 3;
            this.tabPageIdxPatcher.Controls.Add(this.grpPatcherOutput);

            this.textBoxPatcherOutput.Location = new System.Drawing.Point(12, 24);
            this.textBoxPatcherOutput.Size = new System.Drawing.Size(890, 300);
            this.textBoxPatcherOutput.Multiline = true;
            this.textBoxPatcherOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxPatcherOutput.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxPatcherOutput.ReadOnly = true;
            this.textBoxPatcherOutput.TabIndex = 0;
            this.grpPatcherOutput.Controls.Add(this.textBoxPatcherOutput);

            // ════════════════════════════════════════════════════════════════
            // TAB 18 – BATCH SETUP
            // ════════════════════════════════════════════════════════════════
            this.tabPageBatch.Text = "Batch Setup";
            this.tabPageBatch.Size = new System.Drawing.Size(946, 638);
            this.tabPageBatch.TabIndex = 17;
            this.tabPageBatch.UseVisualStyleBackColor = true;

            this.grpBatchConfig.Text = "Konfiguration  -  alle Shard-Dateien auf einmal erstellen";
            this.grpBatchConfig.Location = new System.Drawing.Point(10, 10);
            this.grpBatchConfig.Size = new System.Drawing.Size(920, 280);
            this.grpBatchConfig.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpBatchConfig.TabIndex = 0;
            this.tabPageBatch.Controls.Add(this.grpBatchConfig);

            // Row 1
            this.lblBatchMapWLbl.Text = "Map Breite:";
            this.lblBatchMapWLbl.Location = new System.Drawing.Point(12, 28);
            this.lblBatchMapWLbl.AutoSize = true;
            this.lblBatchMapWLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchMapWLbl.TabIndex = 0;
            this.grpBatchConfig.Controls.Add(this.lblBatchMapWLbl);

            this.tbBatchMapW.Text = "7168";
            this.tbBatchMapW.Location = new System.Drawing.Point(100, 25);
            this.tbBatchMapW.Size = new System.Drawing.Size(90, 23);
            this.tbBatchMapW.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchMapW.TabIndex = 1;
            this.grpBatchConfig.Controls.Add(this.tbBatchMapW);

            this.lblBatchMapHLbl.Text = "Hoehe:";
            this.lblBatchMapHLbl.Location = new System.Drawing.Point(200, 28);
            this.lblBatchMapHLbl.AutoSize = true;
            this.lblBatchMapHLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchMapHLbl.TabIndex = 2;
            this.grpBatchConfig.Controls.Add(this.lblBatchMapHLbl);

            this.tbBatchMapH.Text = "4096";
            this.tbBatchMapH.Location = new System.Drawing.Point(255, 25);
            this.tbBatchMapH.Size = new System.Drawing.Size(90, 23);
            this.tbBatchMapH.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchMapH.TabIndex = 3;
            this.grpBatchConfig.Controls.Add(this.tbBatchMapH);

            this.lblBatchMapIdxLbl.Text = "Map-Index:";
            this.lblBatchMapIdxLbl.Location = new System.Drawing.Point(360, 28);
            this.lblBatchMapIdxLbl.AutoSize = true;
            this.lblBatchMapIdxLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchMapIdxLbl.TabIndex = 4;
            this.grpBatchConfig.Controls.Add(this.lblBatchMapIdxLbl);

            this.tbBatchMapIdx.Text = "0";
            this.tbBatchMapIdx.Location = new System.Drawing.Point(450, 25);
            this.tbBatchMapIdx.Size = new System.Drawing.Size(60, 23);
            this.tbBatchMapIdx.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchMapIdx.TabIndex = 5;
            this.grpBatchConfig.Controls.Add(this.tbBatchMapIdx);

            // Row 2
            this.lblBatchArtLbl.Text = "Art-Eintraege:";
            this.lblBatchArtLbl.Location = new System.Drawing.Point(12, 62);
            this.lblBatchArtLbl.AutoSize = true;
            this.lblBatchArtLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchArtLbl.TabIndex = 6;
            this.grpBatchConfig.Controls.Add(this.lblBatchArtLbl);

            this.tbBatchArtCount.Text = "81884";
            this.tbBatchArtCount.Location = new System.Drawing.Point(110, 59);
            this.tbBatchArtCount.Size = new System.Drawing.Size(90, 23);
            this.tbBatchArtCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchArtCount.TabIndex = 7;
            this.grpBatchConfig.Controls.Add(this.tbBatchArtCount);

            this.lblBatchSoundLbl.Text = "Sound-Slots:";
            this.lblBatchSoundLbl.Location = new System.Drawing.Point(215, 62);
            this.lblBatchSoundLbl.AutoSize = true;
            this.lblBatchSoundLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchSoundLbl.TabIndex = 8;
            this.grpBatchConfig.Controls.Add(this.lblBatchSoundLbl);

            this.tbBatchSoundCount.Text = "4095";
            this.tbBatchSoundCount.Location = new System.Drawing.Point(315, 59);
            this.tbBatchSoundCount.Size = new System.Drawing.Size(90, 23);
            this.tbBatchSoundCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchSoundCount.TabIndex = 9;
            this.grpBatchConfig.Controls.Add(this.tbBatchSoundCount);

            this.lblBatchGumpLbl.Text = "Gump-Eintraege:";
            this.lblBatchGumpLbl.Location = new System.Drawing.Point(420, 62);
            this.lblBatchGumpLbl.AutoSize = true;
            this.lblBatchGumpLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchGumpLbl.TabIndex = 10;
            this.grpBatchConfig.Controls.Add(this.lblBatchGumpLbl);

            this.tbBatchGumpCount.Text = "65535";
            this.tbBatchGumpCount.Location = new System.Drawing.Point(535, 59);
            this.tbBatchGumpCount.Size = new System.Drawing.Size(90, 23);
            this.tbBatchGumpCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchGumpCount.TabIndex = 11;
            this.grpBatchConfig.Controls.Add(this.tbBatchGumpCount);

            this.lblBatchMultiLbl.Text = "Multi-Eintraege:";
            this.lblBatchMultiLbl.Location = new System.Drawing.Point(640, 62);
            this.lblBatchMultiLbl.AutoSize = true;
            this.lblBatchMultiLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchMultiLbl.TabIndex = 12;
            this.grpBatchConfig.Controls.Add(this.lblBatchMultiLbl);

            this.tbBatchMultiCount.Text = "5000";
            this.tbBatchMultiCount.Location = new System.Drawing.Point(750, 59);
            this.tbBatchMultiCount.Size = new System.Drawing.Size(80, 23);
            this.tbBatchMultiCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchMultiCount.TabIndex = 13;
            this.grpBatchConfig.Controls.Add(this.tbBatchMultiCount);

            // Row 3
            this.lblBatchTileLandLbl.Text = "TileData Land-Gruppen:";
            this.lblBatchTileLandLbl.Location = new System.Drawing.Point(12, 97);
            this.lblBatchTileLandLbl.AutoSize = true;
            this.lblBatchTileLandLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchTileLandLbl.TabIndex = 14;
            this.grpBatchConfig.Controls.Add(this.lblBatchTileLandLbl);

            this.tbBatchTileLand.Text = "512";
            this.tbBatchTileLand.Location = new System.Drawing.Point(195, 94);
            this.tbBatchTileLand.Size = new System.Drawing.Size(90, 23);
            this.tbBatchTileLand.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchTileLand.TabIndex = 15;
            this.grpBatchConfig.Controls.Add(this.tbBatchTileLand);

            this.lblBatchTileStaticLbl.Text = "Static-Gruppen:";
            this.lblBatchTileStaticLbl.Location = new System.Drawing.Point(300, 97);
            this.lblBatchTileStaticLbl.AutoSize = true;
            this.lblBatchTileStaticLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchTileStaticLbl.TabIndex = 16;
            this.grpBatchConfig.Controls.Add(this.lblBatchTileStaticLbl);

            this.tbBatchTileStatic.Text = "2048";
            this.tbBatchTileStatic.Location = new System.Drawing.Point(415, 94);
            this.tbBatchTileStatic.Size = new System.Drawing.Size(90, 23);
            this.tbBatchTileStatic.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchTileStatic.TabIndex = 17;
            this.grpBatchConfig.Controls.Add(this.tbBatchTileStatic);

            // Row 4 – Skills
            this.checkBoxBatchSkills.Text = "Skills erstellen";
            this.checkBoxBatchSkills.Checked = true;
            this.checkBoxBatchSkills.Location = new System.Drawing.Point(12, 130);
            this.checkBoxBatchSkills.AutoSize = true;
            this.checkBoxBatchSkills.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxBatchSkills.TabIndex = 18;
            this.grpBatchConfig.Controls.Add(this.checkBoxBatchSkills);

            this.checkBoxBatchDefaultSkills.Text = "Standard-Skills (58 UO-Skills)";
            this.checkBoxBatchDefaultSkills.Checked = true;
            this.checkBoxBatchDefaultSkills.Location = new System.Drawing.Point(155, 130);
            this.checkBoxBatchDefaultSkills.AutoSize = true;
            this.checkBoxBatchDefaultSkills.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.checkBoxBatchDefaultSkills.TabIndex = 19;
            this.grpBatchConfig.Controls.Add(this.checkBoxBatchDefaultSkills);

            this.lblBatchSkillCountLbl.Text = "Skill-Anzahl (leer):";
            this.lblBatchSkillCountLbl.Location = new System.Drawing.Point(400, 131);
            this.lblBatchSkillCountLbl.AutoSize = true;
            this.lblBatchSkillCountLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblBatchSkillCountLbl.TabIndex = 20;
            this.grpBatchConfig.Controls.Add(this.lblBatchSkillCountLbl);

            this.tbBatchSkillCount.Text = "58";
            this.tbBatchSkillCount.Location = new System.Drawing.Point(520, 128);
            this.tbBatchSkillCount.Size = new System.Drawing.Size(70, 23);
            this.tbBatchSkillCount.Font = new System.Drawing.Font("Consolas", 9.5F);
            this.tbBatchSkillCount.TabIndex = 21;
            this.grpBatchConfig.Controls.Add(this.tbBatchSkillCount);

            this.grpBatchActions.Text = "Ausfuehren";
            this.grpBatchActions.Location = new System.Drawing.Point(10, 298);
            this.grpBatchActions.Size = new System.Drawing.Size(920, 70);
            this.grpBatchActions.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpBatchActions.TabIndex = 1;
            this.tabPageBatch.Controls.Add(this.grpBatchActions);

            this.btnBatchCreate.Text = "ALLE DATEIEN ERSTELLEN";
            this.btnBatchCreate.Location = new System.Drawing.Point(12, 22);
            this.btnBatchCreate.Size = new System.Drawing.Size(300, 35);
            this.btnBatchCreate.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnBatchCreate.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            this.btnBatchCreate.ForeColor = System.Drawing.Color.White;
            this.btnBatchCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBatchCreate.TabIndex = 0;
            this.btnBatchCreate.Click += new System.EventHandler(this.BtnBatchCreate_Click);
            this.grpBatchActions.Controls.Add(this.btnBatchCreate);

            this.lblBatchStatus.Text = "-";
            this.lblBatchStatus.Location = new System.Drawing.Point(325, 30);
            this.lblBatchStatus.AutoSize = true;
            this.lblBatchStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBatchStatus.ForeColor = System.Drawing.Color.Navy;
            this.lblBatchStatus.TabIndex = 1;
            this.grpBatchActions.Controls.Add(this.lblBatchStatus);

            this.grpBatchLog.Text = "Fortschritts-Log";
            this.grpBatchLog.Location = new System.Drawing.Point(10, 376);
            this.grpBatchLog.Size = new System.Drawing.Size(920, 235);
            this.grpBatchLog.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpBatchLog.TabIndex = 2;
            this.tabPageBatch.Controls.Add(this.grpBatchLog);

            this.textBoxBatchLog.Location = new System.Drawing.Point(12, 24);
            this.textBoxBatchLog.Size = new System.Drawing.Size(890, 200);
            this.textBoxBatchLog.Multiline = true;
            this.textBoxBatchLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxBatchLog.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxBatchLog.ReadOnly = true;
            this.textBoxBatchLog.TabIndex = 0;
            this.grpBatchLog.Controls.Add(this.textBoxBatchLog);

            // ════════════════════════════════════════════════════════════════
            // TAB 19 – HEX VIEWER
            // ════════════════════════════════════════════════════════════════
            this.tabPageHexViewer.Text = "Hex Viewer";
            this.tabPageHexViewer.Size = new System.Drawing.Size(946, 638);
            this.tabPageHexViewer.TabIndex = 18;
            this.tabPageHexViewer.UseVisualStyleBackColor = true;

            this.grpHexFile.Text = "Datei waehlen";
            this.grpHexFile.Location = new System.Drawing.Point(10, 10);
            this.grpHexFile.Size = new System.Drawing.Size(920, 90);
            this.grpHexFile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHexFile.TabIndex = 0;
            this.tabPageHexViewer.Controls.Add(this.grpHexFile);

            this.lblHexFilePathLbl.Text = "Datei:";
            this.lblHexFilePathLbl.Location = new System.Drawing.Point(12, 28);
            this.lblHexFilePathLbl.AutoSize = true;
            this.lblHexFilePathLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHexFilePathLbl.TabIndex = 0;
            this.grpHexFile.Controls.Add(this.lblHexFilePathLbl);

            this.tbHexFilePath.Location = new System.Drawing.Point(55, 25);
            this.tbHexFilePath.Size = new System.Drawing.Size(720, 23);
            this.tbHexFilePath.Font = new System.Drawing.Font("Consolas", 9F);
            this.tbHexFilePath.TabIndex = 1;
            this.grpHexFile.Controls.Add(this.tbHexFilePath);

            this.BtnHexBrowse.Text = "Oeffnen ...";
            this.BtnHexBrowse.Location = new System.Drawing.Point(785, 23);
            this.BtnHexBrowse.Size = new System.Drawing.Size(120, 26);
            this.BtnHexBrowse.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnHexBrowse.TabIndex = 2;
            this.BtnHexBrowse.Click += new System.EventHandler(this.BtnHexBrowse_Click);
            this.grpHexFile.Controls.Add(this.BtnHexBrowse);

            this.lblHexFileInfo.Text = "-";
            this.lblHexFileInfo.Location = new System.Drawing.Point(12, 58);
            this.lblHexFileInfo.Size = new System.Drawing.Size(890, 22);
            this.lblHexFileInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.lblHexFileInfo.ForeColor = System.Drawing.Color.Navy;
            this.lblHexFileInfo.TabIndex = 3;
            this.grpHexFile.Controls.Add(this.lblHexFileInfo);

            this.grpHexRead.Text = "Hex lesen";
            this.grpHexRead.Location = new System.Drawing.Point(10, 108);
            this.grpHexRead.Size = new System.Drawing.Size(920, 75);
            this.grpHexRead.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHexRead.TabIndex = 1;
            this.tabPageHexViewer.Controls.Add(this.grpHexRead);

            this.lblHexOffsetLbl.Text = "Offset (0x...):";
            this.lblHexOffsetLbl.Location = new System.Drawing.Point(12, 30);
            this.lblHexOffsetLbl.AutoSize = true;
            this.lblHexOffsetLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHexOffsetLbl.TabIndex = 0;
            this.grpHexRead.Controls.Add(this.lblHexOffsetLbl);

            this.tbHexOffset.Text = "0x0";
            this.tbHexOffset.Location = new System.Drawing.Point(105, 27);
            this.tbHexOffset.Size = new System.Drawing.Size(130, 23);
            this.tbHexOffset.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbHexOffset.TabIndex = 1;
            this.grpHexRead.Controls.Add(this.tbHexOffset);

            this.lblHexLengthLbl.Text = "Laenge (Bytes):";
            this.lblHexLengthLbl.Location = new System.Drawing.Point(255, 30);
            this.lblHexLengthLbl.AutoSize = true;
            this.lblHexLengthLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHexLengthLbl.TabIndex = 2;
            this.grpHexRead.Controls.Add(this.lblHexLengthLbl);

            this.tbHexLength.Text = "256";
            this.tbHexLength.Location = new System.Drawing.Point(360, 27);
            this.tbHexLength.Size = new System.Drawing.Size(100, 23);
            this.tbHexLength.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbHexLength.TabIndex = 3;
            this.grpHexRead.Controls.Add(this.tbHexLength);

            this.BtnHexRead.Text = "Hex lesen";
            this.BtnHexRead.Location = new System.Drawing.Point(475, 25);
            this.BtnHexRead.Size = new System.Drawing.Size(140, 28);
            this.BtnHexRead.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnHexRead.TabIndex = 4;
            this.BtnHexRead.Click += new System.EventHandler(this.BtnHexRead_Click);
            this.grpHexRead.Controls.Add(this.BtnHexRead);

            this.BtnHexFileInfo.Text = "Datei-Info";
            this.BtnHexFileInfo.Location = new System.Drawing.Point(625, 25);
            this.BtnHexFileInfo.Size = new System.Drawing.Size(140, 28);
            this.BtnHexFileInfo.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BtnHexFileInfo.TabIndex = 5;
            this.BtnHexFileInfo.Click += new System.EventHandler(this.BtnHexFileInfo_Click);
            this.grpHexRead.Controls.Add(this.BtnHexFileInfo);

            this.grpHexSearch.Text = "Byte-Pattern suchen";
            this.grpHexSearch.Location = new System.Drawing.Point(10, 191);
            this.grpHexSearch.Size = new System.Drawing.Size(920, 70);
            this.grpHexSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHexSearch.TabIndex = 2;
            this.tabPageHexViewer.Controls.Add(this.grpHexSearch);

            this.lblHexPatternLbl.Text = "Hex-Pattern (z.B. FF00AB):";
            this.lblHexPatternLbl.Location = new System.Drawing.Point(12, 28);
            this.lblHexPatternLbl.AutoSize = true;
            this.lblHexPatternLbl.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHexPatternLbl.TabIndex = 0;
            this.grpHexSearch.Controls.Add(this.lblHexPatternLbl);

            this.tbHexPattern.Location = new System.Drawing.Point(215, 25);
            this.tbHexPattern.Size = new System.Drawing.Size(250, 23);
            this.tbHexPattern.Font = new System.Drawing.Font("Consolas", 10F);
            this.tbHexPattern.TabIndex = 1;
            this.grpHexSearch.Controls.Add(this.tbHexPattern);

            this.BtnHexSearch.Text = "Suchen";
            this.BtnHexSearch.Location = new System.Drawing.Point(480, 23);
            this.BtnHexSearch.Size = new System.Drawing.Size(140, 28);
            this.BtnHexSearch.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.BtnHexSearch.TabIndex = 2;
            this.BtnHexSearch.Click += new System.EventHandler(this.BtnHexSearch_Click);
            this.grpHexSearch.Controls.Add(this.BtnHexSearch);

            this.grpHexOutput.Text = "Ausgabe";
            this.grpHexOutput.Location = new System.Drawing.Point(10, 269);
            this.grpHexOutput.Size = new System.Drawing.Size(920, 342);
            this.grpHexOutput.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.grpHexOutput.TabIndex = 3;
            this.tabPageHexViewer.Controls.Add(this.grpHexOutput);

            this.textBoxHexOutput.Location = new System.Drawing.Point(12, 24);
            this.textBoxHexOutput.Size = new System.Drawing.Size(890, 306);
            this.textBoxHexOutput.Multiline = true;
            this.textBoxHexOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxHexOutput.Font = new System.Drawing.Font("Consolas", 8.5F);
            this.textBoxHexOutput.ReadOnly = true;
            this.textBoxHexOutput.TabIndex = 0;
            this.grpHexOutput.Controls.Add(this.textBoxHexOutput);

            // ════════════════════════════════════════════════════════════════
            // RESUME
            // ════════════════════════════════════════════════════════════════
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPalette)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // ── Field Declarations ───────────────────────────────────────────────
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;

        // Existing tabs
        private System.Windows.Forms.TabPage tabPageCreateMuls;
        private System.Windows.Forms.TabPage tabPageReadMuls;
        private System.Windows.Forms.TabPage tabPageTileData;
        private System.Windows.Forms.TabPage tabPageReadOut;
        private System.Windows.Forms.TabPage tabPageTexturen;
        private System.Windows.Forms.TabPage tabPageRadarColor;
        private System.Windows.Forms.TabPage tabPagePalette;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.TabPage tabPageArtmul;
        private System.Windows.Forms.TabPage tabPageSound;
        private System.Windows.Forms.TabPage tabPageGump;

        // New tabs
        private System.Windows.Forms.TabPage tabPageHues;
        private System.Windows.Forms.TabPage tabPageMap;
        private System.Windows.Forms.TabPage tabPageMulti;
        private System.Windows.Forms.TabPage tabPageSkills;
        private System.Windows.Forms.TabPage tabPageValidator;
        private System.Windows.Forms.TabPage tabPageIdxPatcher;
        private System.Windows.Forms.TabPage tabPageBatch;
        private System.Windows.Forms.TabPage tabPageHexViewer;

        // Tab 1
        private System.Windows.Forms.GroupBox grpCreateMulsDir, grpCreateMulsCount, grpCreateMulsButtons, grpRename, grpCreateOutput;
        private System.Windows.Forms.TextBox textBox1, textBox2;
        private System.Windows.Forms.Button BtFileOrder;
        private System.Windows.Forms.Label lblDirInfo, lblCountHint, lblButtonsNote, lblRenameHint;
        private System.Windows.Forms.Button BtCreateARTIDXMul, BtCreateARTIDXMul_uint, BtCreateARTIDXMul_Int;
        private System.Windows.Forms.Button BtCreateARTIDXMul_Ushort, BtCreateARTIDXMul_Short, BtCreateARTIDXMul_Byte;
        private System.Windows.Forms.Button BtCreateARTIDXMul_Ulong, BtCreateARTIDXMul_Sbyte;
        private System.Windows.Forms.ComboBox ComboBoxMuls;
        private System.Windows.Forms.Label lbCreatedMul;

        // Tab 2
        private System.Windows.Forms.GroupBox grpReadMulsActions, grpReadMulsResult, grpReadSingle;
        private System.Windows.Forms.Button BtnCountEntries, BtnShowInfo, BtnReadArtIdx;
        private System.Windows.Forms.Label lblEntryCount, lblIndexHint;
        private System.Windows.Forms.TextBox textBoxInfo, textBoxIndex;

        // Tab 3
        private System.Windows.Forms.GroupBox grpTileDataDir, grpTileDataConfig, grpTileDataQuick;
        private System.Windows.Forms.GroupBox grpTileDataRead, grpTileDataIndex, grpTileDataOutput;
        private System.Windows.Forms.TextBox tbDirTileData, tblandTileGroups, tbstaticTileGroups;
        private System.Windows.Forms.Button btnTileDataBrowse, BtCreateTiledata;
        private System.Windows.Forms.Label lblLandGroupsLbl, lblLandGroupsHint, lblStaticGroupsLbl, lblStaticGroupsHint;
        private System.Windows.Forms.Button BtCreateTiledataEmpty, BtCreateTiledataEmpty2, BtCreateSimpleTiledata;
        private System.Windows.Forms.Button BtTiledatainfo, BtnCountTileDataEntries, BtReadIndexTiledata;
        private System.Windows.Forms.Button BtReadLandTile, BtReadStaticTile, BtReadTileFlags, BtSelectDirectory;
        private System.Windows.Forms.Label lblTileDataEntryCount, lblTiledataIndexHint;
        private System.Windows.Forms.TextBox textBoxTiledataIndex;
        private System.Windows.Forms.Label lbTileDataCreate;
        private System.Windows.Forms.CheckBox checkBoxTileData;

        // Tab 4
        private System.Windows.Forms.GroupBox grpReadOutActions, grpReadOutInfo;
        private System.Windows.Forms.Button ButtonReadTileData, ButtonReadLandTileData, ButtonReadStaticTileData;
        private System.Windows.Forms.Label lblSelectedEntry, lblReadOutIdxLbl;
        private System.Windows.Forms.ListView listViewTileData;
        private System.Windows.Forms.TextBox textBoxOutput, textBoxTileDataInfo;

        // Tab 5
        private System.Windows.Forms.GroupBox grpTexConfig, grpTexActions, grpTexOutput;
        private System.Windows.Forms.Label lblTexCountLbl;
        private System.Windows.Forms.TextBox tbIndexCountTexture;
        private System.Windows.Forms.Label lblTexCountHint, lbTextureCount, tbIndexCount;
        private System.Windows.Forms.CheckBox checkBoxTexture;
        private System.Windows.Forms.Button BtCreateTextur, BtCreateIndexes;

        // Tab 6
        private System.Windows.Forms.GroupBox grpRadarConfig, grpRadarActions, grpRadarOutput;
        private System.Windows.Forms.Label lblRadarCountLbl;
        private System.Windows.Forms.TextBox indexCountTextBox;
        private System.Windows.Forms.Label lblRadarCountHint, lbRadarColor;
        private System.Windows.Forms.Button CreateFileButtonRadarColor;

        // Tab 7
        private System.Windows.Forms.GroupBox grpPaletteCreate, grpPaletteLoad, grpPaletteValues;
        private System.Windows.Forms.Button BtCreatePalette, BtCreatePaletteFull, LoadPaletteButton;
        private System.Windows.Forms.Label lbCreatePalette, lbCreateColorPalette, lblPalettePreview;
        private System.Windows.Forms.PictureBox pictureBoxPalette;
        private System.Windows.Forms.TextBox textBoxRgbValues;

        // Tab 8
        private System.Windows.Forms.GroupBox grpAnimSource, grpAnimOutput, grpAnimCreature;
        private System.Windows.Forms.GroupBox grpAnimActions, grpAnimInfo, grpAnimLog;
        private System.Windows.Forms.TextBox tbfilename, txtOutputDirectory, txtOutputFilename;
        private System.Windows.Forms.Button BtnBrowse, BtnSetOutputDirectory;
        private System.Windows.Forms.Label lblAnimSuffixLbl, lblOrigIDHint, lblHexWarning, lblCopyCountHint;
        private System.Windows.Forms.TextBox txtOrigCreatureID, txtNewCreatureID;
        private System.Windows.Forms.Panel panelCheckbox;
        private System.Windows.Forms.Label lbCopys;
        private System.Windows.Forms.CheckBox chkHighDetail, chkLowDetail, chkHuman;
        private System.Windows.Forms.Button BtnNewAnimIDXFiles, BtnProcessClickOld, Button1;
        private System.Windows.Forms.Button ReadAnimIdx, btnCountIndices, BtnLoadAnimationMulData;
        private System.Windows.Forms.TextBox txtData, tbProcessAminidx;
        private System.Windows.Forms.Label lblNewIdCount;

        // Tab 9
        private System.Windows.Forms.GroupBox grpArtCreate, grpArtCustom, grpArtSplit, grpArtRead, grpArtLog;
        private System.Windows.Forms.Button BtnCreateArtIdx, BtnCreateArtIdx100K, BtnCreateArtIdx150K;
        private System.Windows.Forms.Button BtnCreateArtIdx200K, BtnCreateArtIdx250K, BtnCreateArtIdx500K;
        private System.Windows.Forms.TextBox tbxNewIndex, tbxNewIndex2, tbxArtsCount, tbxLandTilesCount;
        private System.Windows.Forms.Button Button2, Button3, BtCreateOldVersionArtidx;
        private System.Windows.Forms.Label lblArtCreateHint, lblArtCustomHint, lblOldVersionHint;
        private System.Windows.Forms.Label lblArtSplitHint, lblArtSplitTotalLbl, lbArtsCount, lbLandTilesCount, lblArtReadHint;
        private System.Windows.Forms.Button ReadArtmul, ReadArtmul2;
        private System.Windows.Forms.Label lblIndexCount;
        private System.Windows.Forms.TextBox infoARTIDXMULID;

        // Tab 10
        private System.Windows.Forms.GroupBox grpSoundConfig, grpSoundActions, grpSoundOutput;
        private System.Windows.Forms.Label lblSoundCountLbl;
        private System.Windows.Forms.TextBox SoundIDXMul;
        private System.Windows.Forms.Label lblSoundCountHint, IndexSizeLabel;
        private System.Windows.Forms.Button CreateOrgSoundMul, ReadIndexSize;

        // Tab 11
        private System.Windows.Forms.GroupBox grpGumpConfig, grpGumpActions, grpGumpOutput;
        private System.Windows.Forms.Label lblGumpCountLbl;
        private System.Windows.Forms.TextBox IndexSizeTextBox;
        private System.Windows.Forms.Label lblGumpCountHint, gumpLabel;
        private System.Windows.Forms.Button CreateGumpButton, ReadGumpButton;

        // Tab 12 – Hues
        private System.Windows.Forms.GroupBox grpHuesActions, grpHuesOutput;
        private System.Windows.Forms.Button BtnCreateHues, BtnReadHues;
        private System.Windows.Forms.Label lblHuesOutput;

        // Tab 13 – Map/Statics
        private System.Windows.Forms.GroupBox grpMapConfig, grpMapActions, grpMapOutput;
        private System.Windows.Forms.Label lblMapSizeComboLbl;
        private System.Windows.Forms.ComboBox comboMapSize;
        private System.Windows.Forms.Label lblMapWidthLbl;
        private System.Windows.Forms.TextBox tbMapWidth;
        private System.Windows.Forms.Label lblMapHeightLbl;
        private System.Windows.Forms.TextBox tbMapHeight;
        private System.Windows.Forms.Label lblMapIndexLbl;
        private System.Windows.Forms.TextBox tbMapIndex;
        private System.Windows.Forms.Label lblMapSizeInfo, lblMapOutput;
        private System.Windows.Forms.Button BtnCreateMap, BtnCreateStatics, BtnCreateMapAndStatics;

        // Tab 14 – Multi
        private System.Windows.Forms.GroupBox grpMultiConfig, grpMultiActions, grpMultiOutput;
        private System.Windows.Forms.Label lblMultiCountLbl;
        private System.Windows.Forms.TextBox tbMultiCount;
        private System.Windows.Forms.Label lblMultiIndexLbl;
        private System.Windows.Forms.TextBox tbMultiIndex;
        private System.Windows.Forms.CheckBox checkBoxMultiHS;
        private System.Windows.Forms.Button BtnCreateMulti, BtnReadMulti;
        private System.Windows.Forms.Label lblMultiOutput;

        // Tab 15 – Skills
        private System.Windows.Forms.GroupBox grpSkillsConfig, grpSkillsActions, grpSkillsOutput;
        private System.Windows.Forms.Label lblSkillCountLbl;
        private System.Windows.Forms.TextBox tbSkillCount;
        private System.Windows.Forms.Button BtnCreateDefaultSkills, BtnCreateEmptySkills, BtnReadSkills;
        private System.Windows.Forms.Label lblSkillsOutput;
        private System.Windows.Forms.TextBox textBoxSkillsInfo;

        // Tab 16 – Validator
        private System.Windows.Forms.GroupBox grpValidatorActions, grpValidatorOutput;
        private System.Windows.Forms.Button BtnValidate, BtnCompareDirectories;
        private System.Windows.Forms.Label lblValidatorStatus;
        private System.Windows.Forms.TextBox textBoxValidatorOutput;

        // Tab 17 – IDX Patcher
        private System.Windows.Forms.GroupBox grpPatcherFile, grpPatcherEdit, grpPatcherRange, grpPatcherOutput;
        private System.Windows.Forms.Label lblPatchIdxLbl;
        private System.Windows.Forms.TextBox tbPatchIdxPath;
        private System.Windows.Forms.Button BtnPatchBrowseIdx;
        private System.Windows.Forms.Label lblPatchIndexLbl;
        private System.Windows.Forms.TextBox tbPatchIndex;
        private System.Windows.Forms.Label lblPatchLookupLbl;
        private System.Windows.Forms.TextBox tbPatchLookup;
        private System.Windows.Forms.Label lblPatchSizeLbl;
        private System.Windows.Forms.TextBox tbPatchSize;
        private System.Windows.Forms.Label lblPatchUnknownLbl;
        private System.Windows.Forms.TextBox tbPatchUnknown;
        private System.Windows.Forms.Button BtnPatchEntry, BtnClearEntry;
        private System.Windows.Forms.Label lblPatchRangeFromLbl;
        private System.Windows.Forms.TextBox tbPatchRangeFrom;
        private System.Windows.Forms.Label lblPatchRangeCountLbl;
        private System.Windows.Forms.TextBox tbPatchRangeCount;
        private System.Windows.Forms.Button BtnReadRange;
        private System.Windows.Forms.TextBox textBoxPatcherOutput;

        // Tab 18 – Batch Setup
        private System.Windows.Forms.GroupBox grpBatchConfig, grpBatchActions, grpBatchLog;
        private System.Windows.Forms.Label lblBatchMapWLbl;
        private System.Windows.Forms.TextBox tbBatchMapW;
        private System.Windows.Forms.Label lblBatchMapHLbl;
        private System.Windows.Forms.TextBox tbBatchMapH;
        private System.Windows.Forms.Label lblBatchMapIdxLbl;
        private System.Windows.Forms.TextBox tbBatchMapIdx;
        private System.Windows.Forms.Label lblBatchArtLbl;
        private System.Windows.Forms.TextBox tbBatchArtCount;
        private System.Windows.Forms.Label lblBatchSoundLbl;
        private System.Windows.Forms.TextBox tbBatchSoundCount;
        private System.Windows.Forms.Label lblBatchGumpLbl;
        private System.Windows.Forms.TextBox tbBatchGumpCount;
        private System.Windows.Forms.Label lblBatchMultiLbl;
        private System.Windows.Forms.TextBox tbBatchMultiCount;
        private System.Windows.Forms.Label lblBatchTileLandLbl;
        private System.Windows.Forms.TextBox tbBatchTileLand;
        private System.Windows.Forms.Label lblBatchTileStaticLbl;
        private System.Windows.Forms.TextBox tbBatchTileStatic;
        private System.Windows.Forms.Label lblBatchSkillCountLbl;
        private System.Windows.Forms.TextBox tbBatchSkillCount;
        private System.Windows.Forms.CheckBox checkBoxBatchSkills, checkBoxBatchDefaultSkills;
        private System.Windows.Forms.Button btnBatchCreate;
        private System.Windows.Forms.Label lblBatchStatus;
        private System.Windows.Forms.TextBox textBoxBatchLog;

        // Tab 19 – Hex Viewer
        private System.Windows.Forms.GroupBox grpHexFile, grpHexRead, grpHexSearch, grpHexOutput;
        private System.Windows.Forms.Label lblHexFilePathLbl;
        private System.Windows.Forms.TextBox tbHexFilePath;
        private System.Windows.Forms.Button BtnHexBrowse;
        private System.Windows.Forms.Label lblHexFileInfo;
        private System.Windows.Forms.Label lblHexOffsetLbl;
        private System.Windows.Forms.TextBox tbHexOffset;
        private System.Windows.Forms.Label lblHexLengthLbl;
        private System.Windows.Forms.TextBox tbHexLength;
        private System.Windows.Forms.Button BtnHexRead, BtnHexFileInfo;
        private System.Windows.Forms.Label lblHexPatternLbl;
        private System.Windows.Forms.TextBox tbHexPattern;
        private System.Windows.Forms.Button BtnHexSearch;
        private System.Windows.Forms.TextBox textBoxHexOutput;
    }
}
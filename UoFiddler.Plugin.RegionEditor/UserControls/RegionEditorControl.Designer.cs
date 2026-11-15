namespace UoFiddler.Plugin.RegionEditor.UserControls
{
    partial class RegionEditorControl
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
                UnhookEvents();
                components?.Dispose();
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
            this.components = new System.ComponentModel.Container();
            this._splitMain = new System.Windows.Forms.SplitContainer();
            this._tree = new System.Windows.Forms.TreeView();
            this._searchPanel = new System.Windows.Forms.Panel();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._toolTree = new System.Windows.Forms.ToolStrip();
            this._btnAddRegion = new System.Windows.Forms.ToolStripButton();
            this._btnAddChild = new System.Windows.Forms.ToolStripButton();
            this._btnDuplicate = new System.Windows.Forms.ToolStripButton();
            this._btnDeleteRegion = new System.Windows.Forms.ToolStripButton();
            this._sepTree1 = new System.Windows.Forms.ToolStripSeparator();
            this._btnMoveUp = new System.Windows.Forms.ToolStripButton();
            this._btnMoveDown = new System.Windows.Forms.ToolStripButton();
            this._sepTree2 = new System.Windows.Forms.ToolStripSeparator();
            this._btnExpandAll = new System.Windows.Forms.ToolStripButton();
            this._btnCollapseAll = new System.Windows.Forms.ToolStripButton();
            this._splitRight = new System.Windows.Forms.SplitContainer();
            this._mapView = new UoFiddler.Plugin.RegionEditor.UserControls.RegionMapView();
            this._tabs = new System.Windows.Forms.TabControl();
            this._tabProps = new System.Windows.Forms.TabPage();
            this._tblProps = new System.Windows.Forms.TableLayoutPanel();
            this._lblPName = new System.Windows.Forms.Label();
            this._txtName = new System.Windows.Forms.TextBox();
            this._lblPType = new System.Windows.Forms.Label();
            this._cmbType = new System.Windows.Forms.ComboBox();
            this._lblPPriority = new System.Windows.Forms.Label();
            this._numPriority = new System.Windows.Forms.NumericUpDown();
            this._lblPMusic = new System.Windows.Forms.Label();
            this._cmbMusic = new System.Windows.Forms.ComboBox();
            this._lblPZ = new System.Windows.Forms.Label();
            this._pnlZ = new System.Windows.Forms.FlowLayoutPanel();
            this._chkZ = new System.Windows.Forms.CheckBox();
            this._numZMin = new System.Windows.Forms.NumericUpDown();
            this._numZMax = new System.Windows.Forms.NumericUpDown();
            this._lblPGo = new System.Windows.Forms.Label();
            this._pnlGo = new System.Windows.Forms.FlowLayoutPanel();
            this._chkGo = new System.Windows.Forms.CheckBox();
            this._numGoX = new System.Windows.Forms.NumericUpDown();
            this._numGoY = new System.Windows.Forms.NumericUpDown();
            this._numGoZ = new System.Windows.Forms.NumericUpDown();
            this._btnPickGo = new System.Windows.Forms.Button();
            this._lblPEntrance = new System.Windows.Forms.Label();
            this._pnlEntrance = new System.Windows.Forms.FlowLayoutPanel();
            this._chkEntrance = new System.Windows.Forms.CheckBox();
            this._numEnX = new System.Windows.Forms.NumericUpDown();
            this._numEnY = new System.Windows.Forms.NumericUpDown();
            this._numEnZ = new System.Windows.Forms.NumericUpDown();
            this._btnPickEntrance = new System.Windows.Forms.Button();
            this._cmbEnMap = new System.Windows.Forms.ComboBox();
            this._lblPGuards = new System.Windows.Forms.Label();
            this._pnlGuards = new System.Windows.Forms.FlowLayoutPanel();
            this._chkGuardsOff = new System.Windows.Forms.CheckBox();
            this._txtGuardsType = new System.Windows.Forms.TextBox();
            this._lblPRune = new System.Windows.Forms.Label();
            this._txtRune = new System.Windows.Forms.TextBox();
            this._lblPOptions = new System.Windows.Forms.Label();
            this._pnlOptions = new System.Windows.Forms.FlowLayoutPanel();
            this._chkSmartNoHousing = new System.Windows.Forms.CheckBox();
            this._chkLogoutDelay = new System.Windows.Forms.CheckBox();
            this._chkHidden = new System.Windows.Forms.CheckBox();
            this._chkLocked = new System.Windows.Forms.CheckBox();
            this._lblPColor = new System.Windows.Forms.Label();
            this._pnlColor = new System.Windows.Forms.FlowLayoutPanel();
            this._btnColor = new System.Windows.Forms.Button();
            this._btnColorReset = new System.Windows.Forms.Button();
            this._lblPNotes = new System.Windows.Forms.Label();
            this._txtNotes = new System.Windows.Forms.TextBox();
            this._lblPExtra = new System.Windows.Forms.Label();
            this._gridProps = new System.Windows.Forms.DataGridView();
            this._colKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this._lblRegionInfo = new System.Windows.Forms.Label();
            this._tabRects = new System.Windows.Forms.TabPage();
            this._listRects = new System.Windows.Forms.ListView();
            this._colRNo = new System.Windows.Forms.ColumnHeader();
            this._colRX = new System.Windows.Forms.ColumnHeader();
            this._colRY = new System.Windows.Forms.ColumnHeader();
            this._colRW = new System.Windows.Forms.ColumnHeader();
            this._colRH = new System.Windows.Forms.ColumnHeader();
            this._colRZ = new System.Windows.Forms.ColumnHeader();
            this._pnlRectEdit = new System.Windows.Forms.TableLayoutPanel();
            this._lblRX = new System.Windows.Forms.Label();
            this._numRX = new System.Windows.Forms.NumericUpDown();
            this._lblRY = new System.Windows.Forms.Label();
            this._numRY = new System.Windows.Forms.NumericUpDown();
            this._lblRW = new System.Windows.Forms.Label();
            this._numRW = new System.Windows.Forms.NumericUpDown();
            this._lblRH = new System.Windows.Forms.Label();
            this._numRH = new System.Windows.Forms.NumericUpDown();
            this._chkRZ = new System.Windows.Forms.CheckBox();
            this._numRZ1 = new System.Windows.Forms.NumericUpDown();
            this._lblRZTo = new System.Windows.Forms.Label();
            this._numRZ2 = new System.Windows.Forms.NumericUpDown();
            this._toolRects = new System.Windows.Forms.ToolStrip();
            this._btnRectAdd = new System.Windows.Forms.ToolStripButton();
            this._btnRectApply = new System.Windows.Forms.ToolStripButton();
            this._btnRectDelete = new System.Windows.Forms.ToolStripButton();
            this._sepRects = new System.Windows.Forms.ToolStripSeparator();
            this._btnRectOptimize = new System.Windows.Forms.ToolStripButton();
            this._btnPolyToRects = new System.Windows.Forms.ToolStripButton();
            this._tabCheck = new System.Windows.Forms.TabPage();
            this._listIssues = new System.Windows.Forms.ListView();
            this._colISev = new System.Windows.Forms.ColumnHeader();
            this._colIRegion = new System.Windows.Forms.ColumnHeader();
            this._colIMsg = new System.Windows.Forms.ColumnHeader();
            this._toolCheck = new System.Windows.Forms.ToolStrip();
            this._btnRunCheck = new System.Windows.Forms.ToolStripButton();
            this._btnFixSelected = new System.Windows.Forms.ToolStripButton();
            this._btnFixAll = new System.Windows.Forms.ToolStripButton();
            this._lblCheckSummary = new System.Windows.Forms.ToolStripLabel();
            this._tabHistory = new System.Windows.Forms.TabPage();
            this._listHistory = new System.Windows.Forms.ListBox();
            this._toolHistory = new System.Windows.Forms.ToolStrip();
            this._btnHistUndo = new System.Windows.Forms.ToolStripButton();
            this._btnHistRedo = new System.Windows.Forms.ToolStripButton();
            this._btnHistJump = new System.Windows.Forms.ToolStripButton();
            this._btnHistClear = new System.Windows.Forms.ToolStripButton();
            this._status = new System.Windows.Forms.StatusStrip();
            this._lblPos = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblTile = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblHover = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblClient = new System.Windows.Forms.ToolStripStatusLabel();
            this._lblCount = new System.Windows.Forms.ToolStripStatusLabel();
            this._toolTools = new System.Windows.Forms.ToolStrip();
            this._btnToolSelect = new System.Windows.Forms.ToolStripButton();
            this._btnToolPan = new System.Windows.Forms.ToolStripButton();
            this._btnToolRect = new System.Windows.Forms.ToolStripButton();
            this._btnToolPolygon = new System.Windows.Forms.ToolStripButton();
            this._btnToolCircle = new System.Windows.Forms.ToolStripButton();
            this._btnToolFill = new System.Windows.Forms.ToolStripButton();
            this._btnToolGo = new System.Windows.Forms.ToolStripButton();
            this._btnToolEntrance = new System.Windows.Forms.ToolStripButton();
            this._sepTools1 = new System.Windows.Forms.ToolStripSeparator();
            this._lblSnap = new System.Windows.Forms.ToolStripLabel();
            this._cmbSnap = new System.Windows.Forms.ToolStripComboBox();
            this._sepTools2 = new System.Windows.Forms.ToolStripSeparator();
            this._btnZoomIn = new System.Windows.Forms.ToolStripButton();
            this._btnZoomOut = new System.Windows.Forms.ToolStripButton();
            this._btnZoomFit = new System.Windows.Forms.ToolStripButton();
            this._lblZoom = new System.Windows.Forms.ToolStripLabel();
            this._sepTools3 = new System.Windows.Forms.ToolStripSeparator();
            this._ddView = new System.Windows.Forms.ToolStripDropDownButton();
            this._miViewStatics = new System.Windows.Forms.ToolStripMenuItem();
            this._miViewGrid = new System.Windows.Forms.ToolStripMenuItem();
            this._miViewLabels = new System.Windows.Forms.ToolStripMenuItem();
            this._miViewMinimap = new System.Windows.Forms.ToolStripMenuItem();
            this._miViewClient = new System.Windows.Forms.ToolStripMenuItem();
            this._sepView = new System.Windows.Forms.ToolStripSeparator();
            this._miViewRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this._sepTools4 = new System.Windows.Forms.ToolStripSeparator();
            this._btnClientGoto = new System.Windows.Forms.ToolStripButton();
            this._btnSyncClient = new System.Windows.Forms.ToolStripButton();
            this._toolMain = new System.Windows.Forms.ToolStrip();
            this._ddFile = new System.Windows.Forms.ToolStripDropDownButton();
            this._miNew = new System.Windows.Forms.ToolStripMenuItem();
            this._miOpen = new System.Windows.Forms.ToolStripMenuItem();
            this._miRecent = new System.Windows.Forms.ToolStripMenuItem();
            this._miSave = new System.Windows.Forms.ToolStripMenuItem();
            this._miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this._sepF1 = new System.Windows.Forms.ToolStripSeparator();
            this._miImport = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportPol = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportPolFolder = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportCsv = new System.Windows.Forms.ToolStripMenuItem();
            this._miImportProject = new System.Windows.Forms.ToolStripMenuItem();
            this._miExport = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpPolRegions = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpPolJustice = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpPolMusic = new System.Windows.Forms.ToolStripMenuItem();
            this._sepE1 = new System.Windows.Forms.ToolStripSeparator();
            this._miExpCsv = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpTxt = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpHtml = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpPng = new System.Windows.Forms.ToolStripMenuItem();
            this._miExpCSharp = new System.Windows.Forms.ToolStripMenuItem();
            this._sepE2 = new System.Windows.Forms.ToolStripSeparator();
            this._miExpAll = new System.Windows.Forms.ToolStripMenuItem();
            this._miOpenExportDir = new System.Windows.Forms.ToolStripMenuItem();
            this._sepF2 = new System.Windows.Forms.ToolStripSeparator();
            this._miLoadServer = new System.Windows.Forms.ToolStripMenuItem();
            this._miLoadServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miLoadSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miLoadPol = new System.Windows.Forms.ToolStripMenuItem();
            this._miSaveServer = new System.Windows.Forms.ToolStripMenuItem();
            this._miSaveServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miSaveSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miSavePol = new System.Windows.Forms.ToolStripMenuItem();
            this._sepF3 = new System.Windows.Forms.ToolStripSeparator();
            this._miSettings = new System.Windows.Forms.ToolStripMenuItem();
            this._btnUndo = new System.Windows.Forms.ToolStripButton();
            this._btnRedo = new System.Windows.Forms.ToolStripButton();
            this._sepMain1 = new System.Windows.Forms.ToolStripSeparator();
            this._lblMap = new System.Windows.Forms.ToolStripLabel();
            this._cmbMap = new System.Windows.Forms.ToolStripComboBox();
            this._sepMain2 = new System.Windows.Forms.ToolStripSeparator();
            this._ddServer = new System.Windows.Forms.ToolStripDropDownButton();
            this._miSrvLoadServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miSrvLoadSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miSrvLoadPol = new System.Windows.Forms.ToolStripMenuItem();
            this._sepSrv1 = new System.Windows.Forms.ToolStripSeparator();
            this._miSrvSaveServUo = new System.Windows.Forms.ToolStripMenuItem();
            this._miSrvSaveSphere = new System.Windows.Forms.ToolStripMenuItem();
            this._miSrvSavePol = new System.Windows.Forms.ToolStripMenuItem();
            this._sepSrv2 = new System.Windows.Forms.ToolStripSeparator();
            this._miSrvSettings = new System.Windows.Forms.ToolStripMenuItem();
            this._sepMain3 = new System.Windows.Forms.ToolStripSeparator();
            this._btnCheck = new System.Windows.Forms.ToolStripButton();
            this._btnSettings = new System.Windows.Forms.ToolStripButton();
            this._btnHelp = new System.Windows.Forms.ToolStripButton();
            this._btnTheme = new System.Windows.Forms.ToolStripButton();
            this._cmbLang = new System.Windows.Forms.ToolStripComboBox();
            this._lblLang = new System.Windows.Forms.ToolStripLabel();
            this._ctxTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxAdd = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxAddChild = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxDuplicate = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxRename = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxDelete = new System.Windows.Forms.ToolStripMenuItem();
            this._sepCtx1 = new System.Windows.Forms.ToolStripSeparator();
            this._ctxZoomTo = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxSendGo = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxCopyXml = new System.Windows.Forms.ToolStripMenuItem();
            this._sepCtx2 = new System.Windows.Forms.ToolStripSeparator();
            this._ctxOptimize = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxPolyToRects = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxToTop = new System.Windows.Forms.ToolStripMenuItem();
            this._sepCtx3 = new System.Windows.Forms.ToolStripSeparator();
            this._ctxHide = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxLock = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxMap = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._ctxMapSelect = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxMapSetGo = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxMapSetEntrance = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxMapDeleteRect = new System.Windows.Forms.ToolStripMenuItem();
            this._sepMapCtx = new System.Windows.Forms.ToolStripSeparator();
            this._ctxMapSendChar = new System.Windows.Forms.ToolStripMenuItem();
            this._ctxMapCopyPos = new System.Windows.Forms.ToolStripMenuItem();
            this._clientTimer = new System.Windows.Forms.Timer(this.components);
            this._toolTip = new System.Windows.Forms.ToolTip(this.components);
            this._colorDialog = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).BeginInit();
            this._splitMain.Panel1.SuspendLayout();
            this._splitMain.Panel2.SuspendLayout();
            this._splitMain.SuspendLayout();
            this._searchPanel.SuspendLayout();
            this._toolTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._splitRight)).BeginInit();
            this._splitRight.Panel1.SuspendLayout();
            this._splitRight.Panel2.SuspendLayout();
            this._splitRight.SuspendLayout();
            this._tabs.SuspendLayout();
            this._tabProps.SuspendLayout();
            this._tblProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPriority)).BeginInit();
            this._pnlZ.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numZMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numZMax)).BeginInit();
            this._pnlGo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numGoX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGoY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGoZ)).BeginInit();
            this._pnlEntrance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numEnX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numEnY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numEnZ)).BeginInit();
            this._pnlGuards.SuspendLayout();
            this._pnlOptions.SuspendLayout();
            this._pnlColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridProps)).BeginInit();
            this._tabRects.SuspendLayout();
            this._pnlRectEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numRX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRZ1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRZ2)).BeginInit();
            this._toolRects.SuspendLayout();
            this._tabCheck.SuspendLayout();
            this._toolCheck.SuspendLayout();
            this._tabHistory.SuspendLayout();
            this._toolHistory.SuspendLayout();
            this._status.SuspendLayout();
            this._toolTools.SuspendLayout();
            this._toolMain.SuspendLayout();
            this._ctxTree.SuspendLayout();
            this._ctxMap.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitMain
            // 
            // 
            // _splitMain.Panel1
            // 
            this._splitMain.Panel1.Controls.Add(this._tree);
            this._splitMain.Panel1.Controls.Add(this._searchPanel);
            this._splitMain.Panel1.Controls.Add(this._toolTree);
            // 
            // _splitMain.Panel2
            // 
            this._splitMain.Panel2.Controls.Add(this._splitRight);
            this._splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitMain.Location = new System.Drawing.Point(0, 54);
            this._splitMain.Name = "_splitMain";
            this._splitMain.Size = new System.Drawing.Size(1280, 682);
            this._splitMain.SplitterDistance = 270;
            this._splitMain.SplitterWidth = 5;
            this._splitMain.TabIndex = 0;
            // 
            // _tree
            // 
            this._tree.AllowDrop = true;
            this._tree.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._tree.ContextMenuStrip = this._ctxTree;
            this._tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tree.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this._tree.FullRowSelect = true;
            this._tree.HideSelection = false;
            this._tree.ItemHeight = 22;
            this._tree.LabelEdit = true;
            this._tree.Name = "_tree";
            this._tree.ShowNodeToolTips = true;
            this._tree.Size = new System.Drawing.Size(270, 597);
            this._tree.TabIndex = 1;
            this._tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnTreeAfterSelect);
            this._tree.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnTreeAfterLabelEdit);
            this._tree.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.OnTreeItemDrag);
            this._tree.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnTreeDragDrop);
            this._tree.DragOver += new System.Windows.Forms.DragEventHandler(this.OnTreeDragOver);
            this._tree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnTreeNodeDoubleClick);
            this._tree.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyDown);
            // 
            // _searchPanel
            // 
            this._searchPanel.Controls.Add(this._txtSearch);
            this._searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._searchPanel.Name = "_searchPanel";
            this._searchPanel.Padding = new System.Windows.Forms.Padding(4);
            this._searchPanel.Size = new System.Drawing.Size(270, 31);
            this._searchPanel.TabIndex = 2;
            // 
            // _txtSearch
            // 
            this._txtSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.PlaceholderText = "Search region…";
            this._txtSearch.Size = new System.Drawing.Size(262, 23);
            this._txtSearch.TabIndex = 3;
            this._txtSearch.TextChanged += new System.EventHandler(this.OnSearchChanged);
            // 
            // _toolTree
            // 
            this._toolTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnAddRegion,
            this._btnAddChild,
            this._btnDuplicate,
            this._btnDeleteRegion,
            this._sepTree1,
            this._btnMoveUp,
            this._btnMoveDown,
            this._sepTree2,
            this._btnExpandAll,
            this._btnCollapseAll});
            this._toolTree.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolTree.Name = "_toolTree";
            this._toolTree.Size = new System.Drawing.Size(270, 25);
            this._toolTree.TabIndex = 4;
            // 
            // _btnAddRegion
            // 
            this._btnAddRegion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnAddRegion.Name = "_btnAddRegion";
            this._btnAddRegion.Text = "＋ Region";
            this._btnAddRegion.ToolTipText = "New region (top level)";
            this._btnAddRegion.Click += new System.EventHandler(this.OnClickAddRegion);
            // 
            // _btnAddChild
            // 
            this._btnAddChild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnAddChild.Name = "_btnAddChild";
            this._btnAddChild.Text = "＋ Child";
            this._btnAddChild.ToolTipText = "New region inside the selected region";
            this._btnAddChild.Click += new System.EventHandler(this.OnClickAddChild);
            // 
            // _btnDuplicate
            // 
            this._btnDuplicate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnDuplicate.Name = "_btnDuplicate";
            this._btnDuplicate.Text = "⧉";
            this._btnDuplicate.ToolTipText = "Duplicate region";
            this._btnDuplicate.Click += new System.EventHandler(this.OnClickDuplicate);
            // 
            // _btnDeleteRegion
            // 
            this._btnDeleteRegion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnDeleteRegion.Name = "_btnDeleteRegion";
            this._btnDeleteRegion.Text = "✕";
            this._btnDeleteRegion.ToolTipText = "Delete region";
            this._btnDeleteRegion.Click += new System.EventHandler(this.OnClickDeleteRegion);
            // 
            // _sepTree1
            // 
            this._sepTree1.Name = "_sepTree1";
            // 
            // _btnMoveUp
            // 
            this._btnMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnMoveUp.Name = "_btnMoveUp";
            this._btnMoveUp.Text = "▲";
            this._btnMoveUp.ToolTipText = "Move up";
            this._btnMoveUp.Click += new System.EventHandler(this.OnClickMoveUp);
            // 
            // _btnMoveDown
            // 
            this._btnMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnMoveDown.Name = "_btnMoveDown";
            this._btnMoveDown.Text = "▼";
            this._btnMoveDown.ToolTipText = "Move down";
            this._btnMoveDown.Click += new System.EventHandler(this.OnClickMoveDown);
            // 
            // _sepTree2
            // 
            this._sepTree2.Name = "_sepTree2";
            // 
            // _btnExpandAll
            // 
            this._btnExpandAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnExpandAll.Name = "_btnExpandAll";
            this._btnExpandAll.Text = "⊞";
            this._btnExpandAll.ToolTipText = "Expand all";
            this._btnExpandAll.Click += new System.EventHandler(this.OnClickExpandAll);
            // 
            // _btnCollapseAll
            // 
            this._btnCollapseAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnCollapseAll.Name = "_btnCollapseAll";
            this._btnCollapseAll.Text = "⊟";
            this._btnCollapseAll.ToolTipText = "Collapse all";
            this._btnCollapseAll.Click += new System.EventHandler(this.OnClickCollapseAll);
            // 
            // _splitRight
            // 
            // 
            // _splitRight.Panel1
            // 
            this._splitRight.Panel1.Controls.Add(this._mapView);
            // 
            // _splitRight.Panel2
            // 
            this._splitRight.Panel2.Controls.Add(this._tabs);
            this._splitRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this._splitRight.Name = "_splitRight";
            this._splitRight.Size = new System.Drawing.Size(1005, 682);
            this._splitRight.SplitterDistance = 655;
            this._splitRight.SplitterWidth = 5;
            this._splitRight.TabIndex = 5;
            // 
            // _mapView
            // 
            this._mapView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this._mapView.ContextMenuStrip = this._ctxMap;
            this._mapView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mapView.Name = "_mapView";
            this._mapView.Size = new System.Drawing.Size(655, 682);
            this._mapView.TabIndex = 6;
            this._mapView.ShapeCreated += new System.EventHandler<UoFiddler.Plugin.RegionEditor.UserControls.ShapeEventArgs>(this.OnMapShapeCreated);
            this._mapView.PointPicked += new System.EventHandler<UoFiddler.Plugin.RegionEditor.UserControls.ShapeEventArgs>(this.OnMapPointPicked);
            this._mapView.RegionPicked += new System.EventHandler<UoFiddler.Plugin.RegionEditor.UserControls.RegionPickEventArgs>(this.OnMapRegionPicked);
            this._mapView.CursorMoved += new System.EventHandler<UoFiddler.Plugin.RegionEditor.UserControls.RegionPickEventArgs>(this.OnMapCursorMoved);
            this._mapView.EditStarting += new System.EventHandler(this.OnMapEditStarting);
            this._mapView.EditFinished += new System.EventHandler(this.OnMapEditFinished);
            this._mapView.ViewChanged += new System.EventHandler(this.OnMapViewChanged);
            // 
            // _tabs
            // 
            this._tabs.Controls.Add(this._tabProps);
            this._tabs.Controls.Add(this._tabRects);
            this._tabs.Controls.Add(this._tabCheck);
            this._tabs.Controls.Add(this._tabHistory);
            this._tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tabs.Name = "_tabs";
            this._tabs.Size = new System.Drawing.Size(345, 682);
            this._tabs.TabIndex = 7;
            this._tabs.SelectedIndexChanged += new System.EventHandler(this.OnTabChanged);
            // 
            // _tabProps
            // 
            this._tabProps.Controls.Add(this._tblProps);
            this._tabProps.AutoScroll = true;
            this._tabProps.Name = "_tabProps";
            this._tabProps.Padding = new System.Windows.Forms.Padding(3);
            this._tabProps.TabIndex = 8;
            this._tabProps.Text = "Properties";
            this._tabProps.UseVisualStyleBackColor = true;
            // 
            // _tblProps
            // 
            this._tblProps.Controls.Add(this._lblPName, 0, 0);
            this._tblProps.Controls.Add(this._txtName, 1, 0);
            this._tblProps.Controls.Add(this._lblPType, 0, 1);
            this._tblProps.Controls.Add(this._cmbType, 1, 1);
            this._tblProps.Controls.Add(this._lblPPriority, 0, 2);
            this._tblProps.Controls.Add(this._numPriority, 1, 2);
            this._tblProps.Controls.Add(this._lblPMusic, 0, 3);
            this._tblProps.Controls.Add(this._cmbMusic, 1, 3);
            this._tblProps.Controls.Add(this._lblPZ, 0, 4);
            this._tblProps.Controls.Add(this._pnlZ, 1, 4);
            this._tblProps.Controls.Add(this._lblPGo, 0, 5);
            this._tblProps.Controls.Add(this._pnlGo, 1, 5);
            this._tblProps.Controls.Add(this._lblPEntrance, 0, 6);
            this._tblProps.Controls.Add(this._pnlEntrance, 1, 6);
            this._tblProps.Controls.Add(this._lblPGuards, 0, 7);
            this._tblProps.Controls.Add(this._pnlGuards, 1, 7);
            this._tblProps.Controls.Add(this._lblPRune, 0, 8);
            this._tblProps.Controls.Add(this._txtRune, 1, 8);
            this._tblProps.Controls.Add(this._lblPOptions, 0, 9);
            this._tblProps.Controls.Add(this._pnlOptions, 1, 9);
            this._tblProps.Controls.Add(this._lblPColor, 0, 10);
            this._tblProps.Controls.Add(this._pnlColor, 1, 10);
            this._tblProps.Controls.Add(this._lblPNotes, 0, 11);
            this._tblProps.Controls.Add(this._txtNotes, 1, 11);
            this._tblProps.Controls.Add(this._lblPExtra, 0, 12);
            this._tblProps.Controls.Add(this._gridProps, 1, 12);
            this._tblProps.Controls.Add(this._lblRegionInfo, 1, 13);
            this._tblProps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._tblProps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tblProps.AutoSize = true;
            this._tblProps.ColumnCount = 2;
            this._tblProps.Dock = System.Windows.Forms.DockStyle.Top;
            this._tblProps.Name = "_tblProps";
            this._tblProps.Padding = new System.Windows.Forms.Padding(2);
            this._tblProps.RowCount = 14;
            this._tblProps.TabIndex = 9;
            // 
            // _lblPName
            // 
            this._lblPName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPName.AutoSize = true;
            this._lblPName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPName.Name = "_lblPName";
            this._lblPName.Text = "Name:";
            // 
            // _txtName
            // 
            this._txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtName.Name = "_txtName";
            this._txtName.TabIndex = 10;
            this._txtName.Validated += new System.EventHandler(this.OnNameValidated);
            this._txtName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPropertyTextKeyDown);
            // 
            // _lblPType
            // 
            this._lblPType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPType.AutoSize = true;
            this._lblPType.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPType.Name = "_lblPType";
            this._lblPType.Text = "Type:";
            // 
            // _cmbType
            // 
            this._cmbType.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbType.FormattingEnabled = true;
            this._cmbType.Name = "_cmbType";
            this._cmbType.TabIndex = 11;
            this._cmbType.SelectedIndexChanged += new System.EventHandler(this.OnTypeChanged);
            this._cmbType.Validated += new System.EventHandler(this.OnTypeChanged);
            // 
            // _lblPPriority
            // 
            this._lblPPriority.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPPriority.AutoSize = true;
            this._lblPPriority.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPPriority.Name = "_lblPPriority";
            this._lblPPriority.Text = "Priority:";
            // 
            // _numPriority
            // 
            this._numPriority.Maximum = new decimal(new int[] { 255, 0, 0, 0});
            this._numPriority.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numPriority.Name = "_numPriority";
            this._numPriority.Size = new System.Drawing.Size(70, 23);
            this._numPriority.TabIndex = 12;
            this._numPriority.Value = new decimal(new int[] { 50, 0, 0, 0});
            this._numPriority.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPMusic
            // 
            this._lblPMusic.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPMusic.AutoSize = true;
            this._lblPMusic.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPMusic.Name = "_lblPMusic";
            this._lblPMusic.Text = "Music:";
            // 
            // _cmbMusic
            // 
            this._cmbMusic.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbMusic.FormattingEnabled = true;
            this._cmbMusic.Name = "_cmbMusic";
            this._cmbMusic.TabIndex = 13;
            this._cmbMusic.SelectedIndexChanged += new System.EventHandler(this.OnMusicChanged);
            this._cmbMusic.Validated += new System.EventHandler(this.OnMusicChanged);
            // 
            // _lblPZ
            // 
            this._lblPZ.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPZ.AutoSize = true;
            this._lblPZ.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPZ.Name = "_lblPZ";
            this._lblPZ.Text = "Z range:";
            // 
            // _pnlZ
            // 
            this._pnlZ.Controls.Add(this._chkZ);
            this._pnlZ.Controls.Add(this._numZMin);
            this._pnlZ.Controls.Add(this._numZMax);
            this._pnlZ.AutoSize = true;
            this._pnlZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlZ.Margin = new System.Windows.Forms.Padding(0);
            this._pnlZ.Name = "_pnlZ";
            this._pnlZ.TabIndex = 14;
            this._pnlZ.WrapContents = true;
            // 
            // _chkZ
            // 
            this._chkZ.AutoSize = true;
            this._chkZ.Margin = new System.Windows.Forms.Padding(3, 5, 0, 3);
            this._chkZ.Name = "_chkZ";
            this._chkZ.TabIndex = 15;
            this._chkZ.UseVisualStyleBackColor = true;
            this._chkZ.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numZMin
            // 
            this._numZMin.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numZMin.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numZMin.Name = "_numZMin";
            this._numZMin.Size = new System.Drawing.Size(60, 23);
            this._numZMin.TabIndex = 16;
            this._numZMin.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numZMax
            // 
            this._numZMax.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numZMax.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numZMax.Name = "_numZMax";
            this._numZMax.Size = new System.Drawing.Size(60, 23);
            this._numZMax.TabIndex = 17;
            this._numZMax.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPGo
            // 
            this._lblPGo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPGo.AutoSize = true;
            this._lblPGo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPGo.Name = "_lblPGo";
            this._lblPGo.Text = "Go:";
            // 
            // _pnlGo
            // 
            this._pnlGo.Controls.Add(this._chkGo);
            this._pnlGo.Controls.Add(this._numGoX);
            this._pnlGo.Controls.Add(this._numGoY);
            this._pnlGo.Controls.Add(this._numGoZ);
            this._pnlGo.Controls.Add(this._btnPickGo);
            this._pnlGo.AutoSize = true;
            this._pnlGo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlGo.Margin = new System.Windows.Forms.Padding(0);
            this._pnlGo.Name = "_pnlGo";
            this._pnlGo.TabIndex = 18;
            this._pnlGo.WrapContents = true;
            // 
            // _chkGo
            // 
            this._chkGo.AutoSize = true;
            this._chkGo.Margin = new System.Windows.Forms.Padding(3, 5, 0, 3);
            this._chkGo.Name = "_chkGo";
            this._chkGo.TabIndex = 19;
            this._chkGo.UseVisualStyleBackColor = true;
            this._chkGo.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numGoX
            // 
            this._numGoX.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numGoX.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numGoX.Name = "_numGoX";
            this._numGoX.Size = new System.Drawing.Size(62, 23);
            this._numGoX.TabIndex = 20;
            this._numGoX.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numGoY
            // 
            this._numGoY.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numGoY.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numGoY.Name = "_numGoY";
            this._numGoY.Size = new System.Drawing.Size(62, 23);
            this._numGoY.TabIndex = 21;
            this._numGoY.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numGoZ
            // 
            this._numGoZ.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numGoZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numGoZ.Name = "_numGoZ";
            this._numGoZ.Size = new System.Drawing.Size(50, 23);
            this._numGoZ.TabIndex = 22;
            this._numGoZ.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _btnPickGo
            // 
            this._btnPickGo.Name = "_btnPickGo";
            this._btnPickGo.Size = new System.Drawing.Size(28, 23);
            this._btnPickGo.TabIndex = 23;
            this._btnPickGo.Text = "⌖";
            this._btnPickGo.UseVisualStyleBackColor = true;
            this._btnPickGo.Click += new System.EventHandler(this.OnClickPickGo);
            // 
            // _lblPEntrance
            // 
            this._lblPEntrance.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPEntrance.AutoSize = true;
            this._lblPEntrance.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPEntrance.Name = "_lblPEntrance";
            this._lblPEntrance.Text = "Entrance:";
            // 
            // _pnlEntrance
            // 
            this._pnlEntrance.Controls.Add(this._chkEntrance);
            this._pnlEntrance.Controls.Add(this._numEnX);
            this._pnlEntrance.Controls.Add(this._numEnY);
            this._pnlEntrance.Controls.Add(this._numEnZ);
            this._pnlEntrance.Controls.Add(this._btnPickEntrance);
            this._pnlEntrance.Controls.Add(this._cmbEnMap);
            this._pnlEntrance.AutoSize = true;
            this._pnlEntrance.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlEntrance.Margin = new System.Windows.Forms.Padding(0);
            this._pnlEntrance.Name = "_pnlEntrance";
            this._pnlEntrance.TabIndex = 24;
            this._pnlEntrance.WrapContents = true;
            // 
            // _chkEntrance
            // 
            this._chkEntrance.AutoSize = true;
            this._chkEntrance.Margin = new System.Windows.Forms.Padding(3, 5, 0, 3);
            this._chkEntrance.Name = "_chkEntrance";
            this._chkEntrance.TabIndex = 25;
            this._chkEntrance.UseVisualStyleBackColor = true;
            this._chkEntrance.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numEnX
            // 
            this._numEnX.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numEnX.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numEnX.Name = "_numEnX";
            this._numEnX.Size = new System.Drawing.Size(62, 23);
            this._numEnX.TabIndex = 26;
            this._numEnX.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numEnY
            // 
            this._numEnY.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numEnY.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numEnY.Name = "_numEnY";
            this._numEnY.Size = new System.Drawing.Size(62, 23);
            this._numEnY.TabIndex = 27;
            this._numEnY.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _numEnZ
            // 
            this._numEnZ.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numEnZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numEnZ.Name = "_numEnZ";
            this._numEnZ.Size = new System.Drawing.Size(50, 23);
            this._numEnZ.TabIndex = 28;
            this._numEnZ.ValueChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _btnPickEntrance
            // 
            this._btnPickEntrance.Name = "_btnPickEntrance";
            this._btnPickEntrance.Size = new System.Drawing.Size(28, 23);
            this._btnPickEntrance.TabIndex = 29;
            this._btnPickEntrance.Text = "⌖";
            this._btnPickEntrance.UseVisualStyleBackColor = true;
            this._btnPickEntrance.Click += new System.EventHandler(this.OnClickPickEntrance);
            // 
            // _cmbEnMap
            // 
            this._cmbEnMap.FormattingEnabled = true;
            this._cmbEnMap.Name = "_cmbEnMap";
            this._cmbEnMap.Size = new System.Drawing.Size(110, 23);
            this._cmbEnMap.TabIndex = 30;
            this._cmbEnMap.SelectedIndexChanged += new System.EventHandler(this.OnPropertyValueChanged);
            this._cmbEnMap.Validated += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPGuards
            // 
            this._lblPGuards.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPGuards.AutoSize = true;
            this._lblPGuards.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPGuards.Name = "_lblPGuards";
            this._lblPGuards.Text = "Guards:";
            // 
            // _pnlGuards
            // 
            this._pnlGuards.Controls.Add(this._chkGuardsOff);
            this._pnlGuards.Controls.Add(this._txtGuardsType);
            this._pnlGuards.AutoSize = true;
            this._pnlGuards.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlGuards.Margin = new System.Windows.Forms.Padding(0);
            this._pnlGuards.Name = "_pnlGuards";
            this._pnlGuards.TabIndex = 31;
            this._pnlGuards.WrapContents = true;
            // 
            // _chkGuardsOff
            // 
            this._chkGuardsOff.AutoSize = true;
            this._chkGuardsOff.Name = "_chkGuardsOff";
            this._chkGuardsOff.TabIndex = 32;
            this._chkGuardsOff.Text = "disabled";
            this._chkGuardsOff.UseVisualStyleBackColor = true;
            this._chkGuardsOff.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _txtGuardsType
            // 
            this._txtGuardsType.Name = "_txtGuardsType";
            this._txtGuardsType.PlaceholderText = "guard type";
            this._txtGuardsType.Size = new System.Drawing.Size(130, 23);
            this._txtGuardsType.TabIndex = 33;
            this._txtGuardsType.Validated += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPRune
            // 
            this._lblPRune.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPRune.AutoSize = true;
            this._lblPRune.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPRune.Name = "_lblPRune";
            this._lblPRune.Text = "Rune:";
            // 
            // _txtRune
            // 
            this._txtRune.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtRune.Name = "_txtRune";
            this._txtRune.TabIndex = 34;
            this._txtRune.Validated += new System.EventHandler(this.OnPropertyValueChanged);
            this._txtRune.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnPropertyTextKeyDown);
            // 
            // _lblPOptions
            // 
            this._lblPOptions.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPOptions.AutoSize = true;
            this._lblPOptions.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPOptions.Name = "_lblPOptions";
            this._lblPOptions.Text = "Options:";
            // 
            // _pnlOptions
            // 
            this._pnlOptions.Controls.Add(this._chkSmartNoHousing);
            this._pnlOptions.Controls.Add(this._chkLogoutDelay);
            this._pnlOptions.Controls.Add(this._chkHidden);
            this._pnlOptions.Controls.Add(this._chkLocked);
            this._pnlOptions.AutoSize = true;
            this._pnlOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlOptions.Margin = new System.Windows.Forms.Padding(0);
            this._pnlOptions.Name = "_pnlOptions";
            this._pnlOptions.TabIndex = 35;
            this._pnlOptions.WrapContents = true;
            // 
            // _chkSmartNoHousing
            // 
            this._chkSmartNoHousing.AutoSize = true;
            this._chkSmartNoHousing.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkSmartNoHousing.Name = "_chkSmartNoHousing";
            this._chkSmartNoHousing.TabIndex = 36;
            this._chkSmartNoHousing.Text = "smartNoHousing";
            this._chkSmartNoHousing.ThreeState = true;
            this._chkSmartNoHousing.UseVisualStyleBackColor = true;
            this._chkSmartNoHousing.CheckStateChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _chkLogoutDelay
            // 
            this._chkLogoutDelay.AutoSize = true;
            this._chkLogoutDelay.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this._chkLogoutDelay.Name = "_chkLogoutDelay";
            this._chkLogoutDelay.TabIndex = 37;
            this._chkLogoutDelay.Text = "logoutDelay";
            this._chkLogoutDelay.ThreeState = true;
            this._chkLogoutDelay.UseVisualStyleBackColor = true;
            this._chkLogoutDelay.CheckStateChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _chkHidden
            // 
            this._chkHidden.AutoSize = true;
            this._chkHidden.Name = "_chkHidden";
            this._chkHidden.TabIndex = 38;
            this._chkHidden.Text = "hidden";
            this._chkHidden.UseVisualStyleBackColor = true;
            this._chkHidden.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _chkLocked
            // 
            this._chkLocked.AutoSize = true;
            this._chkLocked.Name = "_chkLocked";
            this._chkLocked.TabIndex = 39;
            this._chkLocked.Text = "locked";
            this._chkLocked.UseVisualStyleBackColor = true;
            this._chkLocked.CheckedChanged += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPColor
            // 
            this._lblPColor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPColor.AutoSize = true;
            this._lblPColor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPColor.Name = "_lblPColor";
            this._lblPColor.Text = "Colour:";
            // 
            // _pnlColor
            // 
            this._pnlColor.Controls.Add(this._btnColor);
            this._pnlColor.Controls.Add(this._btnColorReset);
            this._pnlColor.AutoSize = true;
            this._pnlColor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlColor.Margin = new System.Windows.Forms.Padding(0);
            this._pnlColor.Name = "_pnlColor";
            this._pnlColor.TabIndex = 40;
            this._pnlColor.WrapContents = true;
            // 
            // _btnColor
            // 
            this._btnColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnColor.Name = "_btnColor";
            this._btnColor.Size = new System.Drawing.Size(60, 23);
            this._btnColor.TabIndex = 41;
            this._btnColor.UseVisualStyleBackColor = false;
            this._btnColor.Click += new System.EventHandler(this.OnClickColor);
            // 
            // _btnColorReset
            // 
            this._btnColorReset.AutoSize = true;
            this._btnColorReset.Name = "_btnColorReset";
            this._btnColorReset.TabIndex = 42;
            this._btnColorReset.Text = "by type";
            this._btnColorReset.UseVisualStyleBackColor = true;
            this._btnColorReset.Click += new System.EventHandler(this.OnClickColorReset);
            // 
            // _lblPNotes
            // 
            this._lblPNotes.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPNotes.AutoSize = true;
            this._lblPNotes.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPNotes.Name = "_lblPNotes";
            this._lblPNotes.Text = "Notes:";
            // 
            // _txtNotes
            // 
            this._txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtNotes.Multiline = true;
            this._txtNotes.Name = "_txtNotes";
            this._txtNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._txtNotes.Size = new System.Drawing.Size(230, 54);
            this._txtNotes.TabIndex = 43;
            this._txtNotes.Validated += new System.EventHandler(this.OnPropertyValueChanged);
            // 
            // _lblPExtra
            // 
            this._lblPExtra.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPExtra.AutoSize = true;
            this._lblPExtra.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPExtra.Name = "_lblPExtra";
            this._lblPExtra.Text = "Server keys:";
            // 
            // _gridProps
            // 
            this._gridProps.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._colKey,
            this._colValue});
            this._gridProps.AllowUserToResizeRows = false;
            this._gridProps.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._gridProps.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._gridProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridProps.Name = "_gridProps";
            this._gridProps.RowHeadersVisible = false;
            this._gridProps.Size = new System.Drawing.Size(230, 120);
            this._gridProps.TabIndex = 44;
            this._gridProps.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.OnGridPropsEdited);
            this._gridProps.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.OnGridPropsRowDeleted);
            // 
            // _colKey
            // 
            this._colKey.FillWeight = 40F;
            this._colKey.HeaderText = "Key";
            this._colKey.Name = "_colKey";
            // 
            // _colValue
            // 
            this._colValue.FillWeight = 60F;
            this._colValue.HeaderText = "Value";
            this._colValue.Name = "_colValue";
            // 
            // _lblRegionInfo
            // 
            this._lblRegionInfo.AutoSize = true;
            this._lblRegionInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblRegionInfo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRegionInfo.Name = "_lblRegionInfo";
            this._lblRegionInfo.Text = "-";
            // 
            // _tabRects
            // 
            this._tabRects.Controls.Add(this._listRects);
            this._tabRects.Controls.Add(this._pnlRectEdit);
            this._tabRects.Controls.Add(this._toolRects);
            this._tabRects.Name = "_tabRects";
            this._tabRects.Padding = new System.Windows.Forms.Padding(3);
            this._tabRects.TabIndex = 45;
            this._tabRects.Text = "Areas";
            this._tabRects.UseVisualStyleBackColor = true;
            // 
            // _listRects
            // 
            this._listRects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colRNo,
            this._colRX,
            this._colRY,
            this._colRW,
            this._colRH,
            this._colRZ});
            this._listRects.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listRects.FullRowSelect = true;
            this._listRects.GridLines = true;
            this._listRects.HideSelection = false;
            this._listRects.MultiSelect = false;
            this._listRects.Name = "_listRects";
            this._listRects.TabIndex = 46;
            this._listRects.UseCompatibleStateImageBehavior = false;
            this._listRects.View = System.Windows.Forms.View.Details;
            this._listRects.SelectedIndexChanged += new System.EventHandler(this.OnRectSelected);
            this._listRects.DoubleClick += new System.EventHandler(this.OnRectDoubleClick);
            // 
            // _colRNo
            // 
            this._colRNo.Text = "#";
            this._colRNo.Width = 34;
            // 
            // _colRX
            // 
            this._colRX.Text = "X";
            this._colRX.Width = 52;
            // 
            // _colRY
            // 
            this._colRY.Text = "Y";
            this._colRY.Width = 52;
            // 
            // _colRW
            // 
            this._colRW.Text = "Width";
            this._colRW.Width = 52;
            // 
            // _colRH
            // 
            this._colRH.Text = "Height";
            this._colRH.Width = 52;
            // 
            // _colRZ
            // 
            this._colRZ.Text = "Z";
            this._colRZ.Width = 70;
            // 
            // _pnlRectEdit
            // 
            this._pnlRectEdit.Controls.Add(this._lblRX, 0, 0);
            this._pnlRectEdit.Controls.Add(this._numRX, 1, 0);
            this._pnlRectEdit.Controls.Add(this._lblRY, 2, 0);
            this._pnlRectEdit.Controls.Add(this._numRY, 3, 0);
            this._pnlRectEdit.Controls.Add(this._lblRW, 0, 1);
            this._pnlRectEdit.Controls.Add(this._numRW, 1, 1);
            this._pnlRectEdit.Controls.Add(this._lblRH, 2, 1);
            this._pnlRectEdit.Controls.Add(this._numRH, 3, 1);
            this._pnlRectEdit.Controls.Add(this._chkRZ, 0, 2);
            this._pnlRectEdit.Controls.Add(this._numRZ1, 1, 2);
            this._pnlRectEdit.Controls.Add(this._lblRZTo, 2, 2);
            this._pnlRectEdit.Controls.Add(this._numRZ2, 3, 2);
            this._pnlRectEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlRectEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._pnlRectEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlRectEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this._pnlRectEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlRectEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlRectEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlRectEdit.AutoSize = true;
            this._pnlRectEdit.ColumnCount = 4;
            this._pnlRectEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._pnlRectEdit.Name = "_pnlRectEdit";
            this._pnlRectEdit.RowCount = 3;
            this._pnlRectEdit.TabIndex = 47;
            // 
            // _lblRX
            // 
            this._lblRX.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRX.AutoSize = true;
            this._lblRX.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRX.Name = "_lblRX";
            this._lblRX.Text = "X:";
            // 
            // _numRX
            // 
            this._numRX.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numRX.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numRX.Name = "_numRX";
            this._numRX.Size = new System.Drawing.Size(70, 23);
            this._numRX.TabIndex = 48;
            // 
            // _lblRY
            // 
            this._lblRY.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRY.AutoSize = true;
            this._lblRY.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRY.Name = "_lblRY";
            this._lblRY.Text = "Y:";
            // 
            // _numRY
            // 
            this._numRY.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numRY.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numRY.Name = "_numRY";
            this._numRY.Size = new System.Drawing.Size(70, 23);
            this._numRY.TabIndex = 49;
            // 
            // _lblRW
            // 
            this._lblRW.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRW.AutoSize = true;
            this._lblRW.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRW.Name = "_lblRW";
            this._lblRW.Text = "Width:";
            // 
            // _numRW
            // 
            this._numRW.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numRW.Minimum = new decimal(new int[] { 1, 0, 0, 0});
            this._numRW.Name = "_numRW";
            this._numRW.Size = new System.Drawing.Size(70, 23);
            this._numRW.TabIndex = 50;
            this._numRW.Value = new decimal(new int[] { 1, 0, 0, 0});
            // 
            // _lblRH
            // 
            this._lblRH.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRH.AutoSize = true;
            this._lblRH.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRH.Name = "_lblRH";
            this._lblRH.Text = "Height:";
            // 
            // _numRH
            // 
            this._numRH.Maximum = new decimal(new int[] { 65535, 0, 0, 0});
            this._numRH.Minimum = new decimal(new int[] { 1, 0, 0, 0});
            this._numRH.Name = "_numRH";
            this._numRH.Size = new System.Drawing.Size(70, 23);
            this._numRH.TabIndex = 51;
            this._numRH.Value = new decimal(new int[] { 1, 0, 0, 0});
            // 
            // _chkRZ
            // 
            this._chkRZ.AutoSize = true;
            this._chkRZ.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this._chkRZ.Name = "_chkRZ";
            this._chkRZ.TabIndex = 52;
            this._chkRZ.Text = "Z:";
            this._chkRZ.UseVisualStyleBackColor = true;
            // 
            // _numRZ1
            // 
            this._numRZ1.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numRZ1.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numRZ1.Name = "_numRZ1";
            this._numRZ1.Size = new System.Drawing.Size(70, 23);
            this._numRZ1.TabIndex = 53;
            // 
            // _lblRZTo
            // 
            this._lblRZTo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRZTo.AutoSize = true;
            this._lblRZTo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRZTo.Name = "_lblRZTo";
            this._lblRZTo.Text = "…";
            // 
            // _numRZ2
            // 
            this._numRZ2.Maximum = new decimal(new int[] { 127, 0, 0, 0});
            this._numRZ2.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648});
            this._numRZ2.Name = "_numRZ2";
            this._numRZ2.Size = new System.Drawing.Size(70, 23);
            this._numRZ2.TabIndex = 54;
            // 
            // _toolRects
            // 
            this._toolRects.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnRectAdd,
            this._btnRectApply,
            this._btnRectDelete,
            this._sepRects,
            this._btnRectOptimize,
            this._btnPolyToRects});
            this._toolRects.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._toolRects.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolRects.Name = "_toolRects";
            this._toolRects.TabIndex = 55;
            // 
            // _btnRectAdd
            // 
            this._btnRectAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRectAdd.Name = "_btnRectAdd";
            this._btnRectAdd.Text = "＋ Add";
            this._btnRectAdd.ToolTipText = "Add the values below as new rectangle";
            this._btnRectAdd.Click += new System.EventHandler(this.OnClickRectAdd);
            // 
            // _btnRectApply
            // 
            this._btnRectApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRectApply.Name = "_btnRectApply";
            this._btnRectApply.Text = "✓ Apply";
            this._btnRectApply.ToolTipText = "Apply the values below to the selected rectangle";
            this._btnRectApply.Click += new System.EventHandler(this.OnClickRectApply);
            // 
            // _btnRectDelete
            // 
            this._btnRectDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRectDelete.Name = "_btnRectDelete";
            this._btnRectDelete.Text = "✕";
            this._btnRectDelete.ToolTipText = "Delete the selected rectangle / polygon";
            this._btnRectDelete.Click += new System.EventHandler(this.OnClickRectDelete);
            // 
            // _sepRects
            // 
            this._sepRects.Name = "_sepRects";
            // 
            // _btnRectOptimize
            // 
            this._btnRectOptimize.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRectOptimize.Name = "_btnRectOptimize";
            this._btnRectOptimize.Text = "⧈ Optimize";
            this._btnRectOptimize.ToolTipText = "Merge rectangles into as few as possible";
            this._btnRectOptimize.Click += new System.EventHandler(this.OnClickRectOptimize);
            // 
            // _btnPolyToRects
            // 
            this._btnPolyToRects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnPolyToRects.Name = "_btnPolyToRects";
            this._btnPolyToRects.Text = "▱→▭";
            this._btnPolyToRects.ToolTipText = "Convert polygons into rectangles";
            this._btnPolyToRects.Click += new System.EventHandler(this.OnClickPolyToRects);
            // 
            // _tabCheck
            // 
            this._tabCheck.Controls.Add(this._listIssues);
            this._tabCheck.Controls.Add(this._toolCheck);
            this._tabCheck.Name = "_tabCheck";
            this._tabCheck.Padding = new System.Windows.Forms.Padding(3);
            this._tabCheck.TabIndex = 56;
            this._tabCheck.Text = "Check";
            this._tabCheck.UseVisualStyleBackColor = true;
            // 
            // _listIssues
            // 
            this._listIssues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colISev,
            this._colIRegion,
            this._colIMsg});
            this._listIssues.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listIssues.FullRowSelect = true;
            this._listIssues.HideSelection = false;
            this._listIssues.Name = "_listIssues";
            this._listIssues.ShowItemToolTips = true;
            this._listIssues.TabIndex = 57;
            this._listIssues.UseCompatibleStateImageBehavior = false;
            this._listIssues.View = System.Windows.Forms.View.Details;
            this._listIssues.DoubleClick += new System.EventHandler(this.OnIssueActivated);
            this._listIssues.SelectedIndexChanged += new System.EventHandler(this.OnIssueSelected);
            // 
            // _colISev
            // 
            this._colISev.Text = "!";
            this._colISev.Width = 30;
            // 
            // _colIRegion
            // 
            this._colIRegion.Text = "Region";
            this._colIRegion.Width = 100;
            // 
            // _colIMsg
            // 
            this._colIMsg.Text = "Problem";
            this._colIMsg.Width = 400;
            // 
            // _toolCheck
            // 
            this._toolCheck.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnRunCheck,
            this._btnFixSelected,
            this._btnFixAll,
            this._lblCheckSummary});
            this._toolCheck.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolCheck.Name = "_toolCheck";
            this._toolCheck.TabIndex = 58;
            // 
            // _btnRunCheck
            // 
            this._btnRunCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRunCheck.Name = "_btnRunCheck";
            this._btnRunCheck.Text = "▶ Check";
            this._btnRunCheck.Click += new System.EventHandler(this.OnClickRunCheck);
            // 
            // _btnFixSelected
            // 
            this._btnFixSelected.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnFixSelected.Name = "_btnFixSelected";
            this._btnFixSelected.Text = "Fix";
            this._btnFixSelected.ToolTipText = "Fix the selected problems automatically";
            this._btnFixSelected.Click += new System.EventHandler(this.OnClickFixSelected);
            // 
            // _btnFixAll
            // 
            this._btnFixAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnFixAll.Name = "_btnFixAll";
            this._btnFixAll.Text = "Fix all";
            this._btnFixAll.ToolTipText = "Fix all problems that can be fixed automatically";
            this._btnFixAll.Click += new System.EventHandler(this.OnClickFixAll);
            // 
            // _lblCheckSummary
            // 
            this._lblCheckSummary.Name = "_lblCheckSummary";
            this._lblCheckSummary.Text = "-";
            // 
            // _tabHistory
            // 
            this._tabHistory.Controls.Add(this._listHistory);
            this._tabHistory.Controls.Add(this._toolHistory);
            this._tabHistory.Name = "_tabHistory";
            this._tabHistory.Padding = new System.Windows.Forms.Padding(3);
            this._tabHistory.TabIndex = 59;
            this._tabHistory.Text = "History";
            this._tabHistory.UseVisualStyleBackColor = true;
            // 
            // _listHistory
            // 
            this._listHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listHistory.FormattingEnabled = true;
            this._listHistory.IntegralHeight = false;
            this._listHistory.ItemHeight = 15;
            this._listHistory.Name = "_listHistory";
            this._listHistory.TabIndex = 60;
            this._listHistory.DoubleClick += new System.EventHandler(this.OnClickHistoryJump);
            // 
            // _toolHistory
            // 
            this._toolHistory.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnHistUndo,
            this._btnHistRedo,
            this._btnHistJump,
            this._btnHistClear});
            this._toolHistory.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolHistory.Name = "_toolHistory";
            this._toolHistory.TabIndex = 61;
            // 
            // _btnHistUndo
            // 
            this._btnHistUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHistUndo.Name = "_btnHistUndo";
            this._btnHistUndo.Text = "↶";
            this._btnHistUndo.ToolTipText = "Undo";
            this._btnHistUndo.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // _btnHistRedo
            // 
            this._btnHistRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHistRedo.Name = "_btnHistRedo";
            this._btnHistRedo.Text = "↷";
            this._btnHistRedo.ToolTipText = "Redo";
            this._btnHistRedo.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // _btnHistJump
            // 
            this._btnHistJump.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHistJump.Name = "_btnHistJump";
            this._btnHistJump.Text = "Back to here";
            this._btnHistJump.ToolTipText = "Undo all steps up to the selected entry";
            this._btnHistJump.Click += new System.EventHandler(this.OnClickHistoryJump);
            // 
            // _btnHistClear
            // 
            this._btnHistClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHistClear.Name = "_btnHistClear";
            this._btnHistClear.Text = "Clear";
            this._btnHistClear.Click += new System.EventHandler(this.OnClickHistoryClear);
            // 
            // _status
            // 
            this._status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblPos,
            this._lblTile,
            this._lblHover,
            this._lblMessage,
            this._lblClient,
            this._lblCount});
            this._status.Location = new System.Drawing.Point(0, 736);
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(1280, 24);
            this._status.TabIndex = 62;
            // 
            // _lblPos
            // 
            this._lblPos.AutoSize = false;
            this._lblPos.Name = "_lblPos";
            this._lblPos.Size = new System.Drawing.Size(110, 19);
            this._lblPos.Text = "-";
            this._lblPos.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblTile
            // 
            this._lblTile.AutoSize = false;
            this._lblTile.Name = "_lblTile";
            this._lblTile.Size = new System.Drawing.Size(130, 19);
            this._lblTile.Text = "-";
            this._lblTile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblHover
            // 
            this._lblHover.Name = "_lblHover";
            this._lblHover.Text = "";
            // 
            // _lblMessage
            // 
            this._lblMessage.Name = "_lblMessage";
            this._lblMessage.Spring = true;
            this._lblMessage.Text = "";
            this._lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _lblClient
            // 
            this._lblClient.Name = "_lblClient";
            this._lblClient.Text = "";
            // 
            // _lblCount
            // 
            this._lblCount.Name = "_lblCount";
            this._lblCount.Text = "0";
            // 
            // _toolTools
            // 
            this._toolTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnToolSelect,
            this._btnToolPan,
            this._btnToolRect,
            this._btnToolPolygon,
            this._btnToolCircle,
            this._btnToolFill,
            this._btnToolGo,
            this._btnToolEntrance,
            this._sepTools1,
            this._lblSnap,
            this._cmbSnap,
            this._sepTools2,
            this._btnZoomIn,
            this._btnZoomOut,
            this._btnZoomFit,
            this._lblZoom,
            this._sepTools3,
            this._ddView,
            this._sepTools4,
            this._btnClientGoto,
            this._btnSyncClient});
            this._toolTools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolTools.Location = new System.Drawing.Point(0, 27);
            this._toolTools.Name = "_toolTools";
            this._toolTools.Size = new System.Drawing.Size(1280, 27);
            this._toolTools.TabIndex = 63;
            // 
            // _btnToolSelect
            // 
            this._btnToolSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolSelect.Name = "_btnToolSelect";
            this._btnToolSelect.Text = "➚ Select";
            this._btnToolSelect.ToolTipText = "Select, move and resize (Shift = move the whole region, Alt = next region below)";
            this._btnToolSelect.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolPan
            // 
            this._btnToolPan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolPan.Name = "_btnToolPan";
            this._btnToolPan.Text = "✋ Pan";
            this._btnToolPan.ToolTipText = "Move the map (also: middle mouse button)";
            this._btnToolPan.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolRect
            // 
            this._btnToolRect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolRect.Name = "_btnToolRect";
            this._btnToolRect.Text = "▭ Rectangle";
            this._btnToolRect.ToolTipText = "Draw a rectangle (Shift = add to the selected region, Alt = cut out)";
            this._btnToolRect.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolPolygon
            // 
            this._btnToolPolygon.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolPolygon.Name = "_btnToolPolygon";
            this._btnToolPolygon.Text = "▱ Polygon";
            this._btnToolPolygon.ToolTipText = "Click the corners, double click / Enter = finish, Backspace = remove last point";
            this._btnToolPolygon.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolCircle
            // 
            this._btnToolCircle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolCircle.Name = "_btnToolCircle";
            this._btnToolCircle.Text = "◯ Circle";
            this._btnToolCircle.ToolTipText = "Drag from the center";
            this._btnToolCircle.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolFill
            // 
            this._btnToolFill.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolFill.Name = "_btnToolFill";
            this._btnToolFill.Text = "▦ Fill";
            this._btnToolFill.ToolTipText = "Fill the connected land of the same kind (e.g. an island, a lake)";
            this._btnToolFill.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolGo
            // 
            this._btnToolGo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolGo.Name = "_btnToolGo";
            this._btnToolGo.Text = "G Go";
            this._btnToolGo.ToolTipText = "Set the go location of the selected region";
            this._btnToolGo.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _btnToolEntrance
            // 
            this._btnToolEntrance.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnToolEntrance.Name = "_btnToolEntrance";
            this._btnToolEntrance.Text = "E Entrance";
            this._btnToolEntrance.ToolTipText = "Set the entrance of the selected region";
            this._btnToolEntrance.Click += new System.EventHandler(this.OnClickTool);
            // 
            // _sepTools1
            // 
            this._sepTools1.Name = "_sepTools1";
            // 
            // _lblSnap
            // 
            this._lblSnap.Name = "_lblSnap";
            this._lblSnap.Text = "Snap:";
            // 
            // _cmbSnap
            // 
            this._cmbSnap.Items.AddRange(new object[] {
            "1",
            "2",
            "4",
            "8",
            "16"});
            this._cmbSnap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSnap.Name = "_cmbSnap";
            this._cmbSnap.Size = new System.Drawing.Size(55, 27);
            this._cmbSnap.SelectedIndexChanged += new System.EventHandler(this.OnSnapChanged);
            // 
            // _sepTools2
            // 
            this._sepTools2.Name = "_sepTools2";
            // 
            // _btnZoomIn
            // 
            this._btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnZoomIn.Name = "_btnZoomIn";
            this._btnZoomIn.Text = "＋";
            this._btnZoomIn.ToolTipText = "Zoom in (mouse wheel)";
            this._btnZoomIn.Click += new System.EventHandler(this.OnClickZoomIn);
            // 
            // _btnZoomOut
            // 
            this._btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnZoomOut.Name = "_btnZoomOut";
            this._btnZoomOut.Text = "－";
            this._btnZoomOut.ToolTipText = "Zoom out";
            this._btnZoomOut.Click += new System.EventHandler(this.OnClickZoomOut);
            // 
            // _btnZoomFit
            // 
            this._btnZoomFit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnZoomFit.Name = "_btnZoomFit";
            this._btnZoomFit.Text = "⤢";
            this._btnZoomFit.ToolTipText = "Show the selected region";
            this._btnZoomFit.Click += new System.EventHandler(this.OnClickZoomFit);
            // 
            // _lblZoom
            // 
            this._lblZoom.Name = "_lblZoom";
            this._lblZoom.Text = "100%";
            // 
            // _sepTools3
            // 
            this._sepTools3.Name = "_sepTools3";
            // 
            // _ddView
            // 
            this._ddView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miViewStatics,
            this._miViewGrid,
            this._miViewLabels,
            this._miViewMinimap,
            this._miViewClient,
            this._sepView,
            this._miViewRefresh});
            this._ddView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._ddView.Name = "_ddView";
            this._ddView.Text = "View";
            // 
            // _miViewStatics
            // 
            this._miViewStatics.CheckOnClick = true;
            this._miViewStatics.Name = "_miViewStatics";
            this._miViewStatics.Text = "Statics";
            this._miViewStatics.Click += new System.EventHandler(this.OnClickViewOption);
            // 
            // _miViewGrid
            // 
            this._miViewGrid.CheckOnClick = true;
            this._miViewGrid.Name = "_miViewGrid";
            this._miViewGrid.Text = "Grid";
            this._miViewGrid.Click += new System.EventHandler(this.OnClickViewOption);
            // 
            // _miViewLabels
            // 
            this._miViewLabels.CheckOnClick = true;
            this._miViewLabels.Name = "_miViewLabels";
            this._miViewLabels.Text = "Names";
            this._miViewLabels.Click += new System.EventHandler(this.OnClickViewOption);
            // 
            // _miViewMinimap
            // 
            this._miViewMinimap.CheckOnClick = true;
            this._miViewMinimap.Name = "_miViewMinimap";
            this._miViewMinimap.Text = "Minimap";
            this._miViewMinimap.Click += new System.EventHandler(this.OnClickViewOption);
            // 
            // _miViewClient
            // 
            this._miViewClient.CheckOnClick = true;
            this._miViewClient.Name = "_miViewClient";
            this._miViewClient.Text = "Client position";
            this._miViewClient.Click += new System.EventHandler(this.OnClickViewOption);
            // 
            // _sepView
            // 
            this._sepView.Name = "_sepView";
            // 
            // _miViewRefresh
            // 
            this._miViewRefresh.Name = "_miViewRefresh";
            this._miViewRefresh.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this._miViewRefresh.Text = "Reload map";
            this._miViewRefresh.Click += new System.EventHandler(this.OnClickViewRefresh);
            // 
            // _sepTools4
            // 
            this._sepTools4.Name = "_sepTools4";
            // 
            // _btnClientGoto
            // 
            this._btnClientGoto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnClientGoto.Name = "_btnClientGoto";
            this._btnClientGoto.Text = "⌖ Client";
            this._btnClientGoto.ToolTipText = "Show the position of the running client";
            this._btnClientGoto.Click += new System.EventHandler(this.OnClickClientGoto);
            // 
            // _btnSyncClient
            // 
            this._btnSyncClient.CheckOnClick = true;
            this._btnSyncClient.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSyncClient.Name = "_btnSyncClient";
            this._btnSyncClient.Text = "⟳ Follow";
            this._btnSyncClient.ToolTipText = "Follow the client position";
            this._btnSyncClient.Click += new System.EventHandler(this.OnClickSyncClient);
            // 
            // _toolMain
            // 
            this._toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ddFile,
            this._btnUndo,
            this._btnRedo,
            this._sepMain1,
            this._lblMap,
            this._cmbMap,
            this._sepMain2,
            this._ddServer,
            this._sepMain3,
            this._btnCheck,
            this._btnSettings,
            this._btnHelp,
            this._btnTheme,
            this._cmbLang,
            this._lblLang});
            this._toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._toolMain.Location = new System.Drawing.Point(0, 0);
            this._toolMain.Name = "_toolMain";
            this._toolMain.Size = new System.Drawing.Size(1280, 27);
            this._toolMain.TabIndex = 64;
            // 
            // _ddFile
            // 
            this._ddFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miNew,
            this._miOpen,
            this._miRecent,
            this._miSave,
            this._miSaveAs,
            this._sepF1,
            this._miImport,
            this._miExport,
            this._sepF2,
            this._miLoadServer,
            this._miSaveServer,
            this._sepF3,
            this._miSettings});
            this._ddFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._ddFile.Name = "_ddFile";
            this._ddFile.Text = "File";
            // 
            // _miNew
            // 
            this._miNew.Name = "_miNew";
            this._miNew.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            this._miNew.Text = "New project";
            this._miNew.Click += new System.EventHandler(this.OnClickNew);
            // 
            // _miOpen
            // 
            this._miOpen.Name = "_miOpen";
            this._miOpen.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            this._miOpen.Text = "Open project…";
            this._miOpen.Click += new System.EventHandler(this.OnClickOpen);
            // 
            // _miRecent
            // 
            this._miRecent.Name = "_miRecent";
            this._miRecent.Text = "Recent files";
            // 
            // _miSave
            // 
            this._miSave.Name = "_miSave";
            this._miSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            this._miSave.Text = "Save project";
            this._miSave.Click += new System.EventHandler(this.OnClickSave);
            // 
            // _miSaveAs
            // 
            this._miSaveAs.Name = "_miSaveAs";
            this._miSaveAs.Text = "Save project as…";
            this._miSaveAs.Click += new System.EventHandler(this.OnClickSaveAs);
            // 
            // _sepF1
            // 
            this._sepF1.Name = "_sepF1";
            // 
            // _miImport
            // 
            this._miImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miImportServUo,
            this._miImportSphere,
            this._miImportPol,
            this._miImportPolFolder,
            this._miImportCsv,
            this._miImportProject});
            this._miImport.Name = "_miImport";
            this._miImport.Text = "Import";
            // 
            // _miImportServUo
            // 
            this._miImportServUo.Name = "_miImportServUo";
            this._miImportServUo.Text = "ServUO / RunUO Regions.xml…";
            this._miImportServUo.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miImportSphere
            // 
            this._miImportSphere.Name = "_miImportSphere";
            this._miImportSphere.Text = "Sphere areas (.scp)…";
            this._miImportSphere.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miImportPol
            // 
            this._miImportPol.Name = "_miImportPol";
            this._miImportPol.Text = "POL region file (.cfg)…";
            this._miImportPol.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miImportPolFolder
            // 
            this._miImportPolFolder.Name = "_miImportPolFolder";
            this._miImportPolFolder.Text = "POL regions folder…";
            this._miImportPolFolder.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miImportCsv
            // 
            this._miImportCsv.Name = "_miImportCsv";
            this._miImportCsv.Text = "CSV…";
            this._miImportCsv.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miImportProject
            // 
            this._miImportProject.Name = "_miImportProject";
            this._miImportProject.Text = "Region project (add)…";
            this._miImportProject.Click += new System.EventHandler(this.OnClickImport);
            // 
            // _miExport
            // 
            this._miExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miExpServUo,
            this._miExpSphere,
            this._miExpPolRegions,
            this._miExpPolJustice,
            this._miExpPolMusic,
            this._sepE1,
            this._miExpCsv,
            this._miExpTxt,
            this._miExpHtml,
            this._miExpPng,
            this._miExpCSharp,
            this._sepE2,
            this._miExpAll,
            this._miOpenExportDir});
            this._miExport.Name = "_miExport";
            this._miExport.Text = "Export";
            // 
            // _miExpServUo
            // 
            this._miExpServUo.Name = "_miExpServUo";
            this._miExpServUo.Text = "ServUO / RunUO Regions.xml…";
            this._miExpServUo.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpSphere
            // 
            this._miExpSphere.Name = "_miExpSphere";
            this._miExpSphere.Text = "Sphere areas (.scp)…";
            this._miExpSphere.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpPolRegions
            // 
            this._miExpPolRegions.Name = "_miExpPolRegions";
            this._miExpPolRegions.Text = "POL regions.cfg…";
            this._miExpPolRegions.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpPolJustice
            // 
            this._miExpPolJustice.Name = "_miExpPolJustice";
            this._miExpPolJustice.Text = "POL justice.cfg…";
            this._miExpPolJustice.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpPolMusic
            // 
            this._miExpPolMusic.Name = "_miExpPolMusic";
            this._miExpPolMusic.Text = "POL music.cfg…";
            this._miExpPolMusic.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _sepE1
            // 
            this._sepE1.Name = "_sepE1";
            // 
            // _miExpCsv
            // 
            this._miExpCsv.Name = "_miExpCsv";
            this._miExpCsv.Text = "CSV table…";
            this._miExpCsv.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpTxt
            // 
            this._miExpTxt.Name = "_miExpTxt";
            this._miExpTxt.Text = "Text overview…";
            this._miExpTxt.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpHtml
            // 
            this._miExpHtml.Name = "_miExpHtml";
            this._miExpHtml.Text = "HTML overview…";
            this._miExpHtml.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpPng
            // 
            this._miExpPng.Name = "_miExpPng";
            this._miExpPng.Text = "Map image with regions (PNG)…";
            this._miExpPng.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _miExpCSharp
            // 
            this._miExpCSharp.Name = "_miExpCSharp";
            this._miExpCSharp.Text = "C# classes of own region types…";
            this._miExpCSharp.Click += new System.EventHandler(this.OnClickExport);
            // 
            // _sepE2
            // 
            this._sepE2.Name = "_sepE2";
            // 
            // _miExpAll
            // 
            this._miExpAll.Name = "_miExpAll";
            this._miExpAll.Text = "Everything into the export folder";
            this._miExpAll.Click += new System.EventHandler(this.OnClickExportAll);
            // 
            // _miOpenExportDir
            // 
            this._miOpenExportDir.Name = "_miOpenExportDir";
            this._miOpenExportDir.Text = "Open export folder";
            this._miOpenExportDir.Click += new System.EventHandler(this.OnClickOpenExportDir);
            // 
            // _sepF2
            // 
            this._sepF2.Name = "_sepF2";
            // 
            // _miLoadServer
            // 
            this._miLoadServer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miLoadServUo,
            this._miLoadSphere,
            this._miLoadPol});
            this._miLoadServer.Name = "_miLoadServer";
            this._miLoadServer.Text = "Load from server";
            // 
            // _miLoadServUo
            // 
            this._miLoadServUo.Name = "_miLoadServUo";
            this._miLoadServUo.Text = "ServUO (Data\\Regions.xml)";
            this._miLoadServUo.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _miLoadSphere
            // 
            this._miLoadSphere.Name = "_miLoadSphere";
            this._miLoadSphere.Text = "Sphere (areas file)";
            this._miLoadSphere.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _miLoadPol
            // 
            this._miLoadPol.Name = "_miLoadPol";
            this._miLoadPol.Text = "POL (regions folder)";
            this._miLoadPol.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _miSaveServer
            // 
            this._miSaveServer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miSaveServUo,
            this._miSaveSphere,
            this._miSavePol});
            this._miSaveServer.Name = "_miSaveServer";
            this._miSaveServer.Text = "Save into server";
            // 
            // _miSaveServUo
            // 
            this._miSaveServUo.Name = "_miSaveServUo";
            this._miSaveServUo.Text = "ServUO (Data\\Regions.xml)";
            this._miSaveServUo.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _miSaveSphere
            // 
            this._miSaveSphere.Name = "_miSaveSphere";
            this._miSaveSphere.Text = "Sphere (areas file)";
            this._miSaveSphere.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _miSavePol
            // 
            this._miSavePol.Name = "_miSavePol";
            this._miSavePol.Text = "POL (regions folder)";
            this._miSavePol.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _sepF3
            // 
            this._sepF3.Name = "_sepF3";
            // 
            // _miSettings
            // 
            this._miSettings.Name = "_miSettings";
            this._miSettings.Text = "Settings…";
            this._miSettings.Click += new System.EventHandler(this.OnClickSettings);
            // 
            // _btnUndo
            // 
            this._btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnUndo.Name = "_btnUndo";
            this._btnUndo.Text = "↶ Undo";
            this._btnUndo.ToolTipText = "Undo (Ctrl+Z)";
            this._btnUndo.Click += new System.EventHandler(this.OnClickUndo);
            // 
            // _btnRedo
            // 
            this._btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRedo.Name = "_btnRedo";
            this._btnRedo.Text = "↷ Redo";
            this._btnRedo.ToolTipText = "Redo (Ctrl+Y)";
            this._btnRedo.Click += new System.EventHandler(this.OnClickRedo);
            // 
            // _sepMain1
            // 
            this._sepMain1.Name = "_sepMain1";
            // 
            // _lblMap
            // 
            this._lblMap.Name = "_lblMap";
            this._lblMap.Text = "Map:";
            // 
            // _cmbMap
            // 
            this._cmbMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbMap.Name = "_cmbMap";
            this._cmbMap.Size = new System.Drawing.Size(160, 27);
            this._cmbMap.SelectedIndexChanged += new System.EventHandler(this.OnMapSelected);
            // 
            // _sepMain2
            // 
            this._sepMain2.Name = "_sepMain2";
            // 
            // _ddServer
            // 
            this._ddServer.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miSrvLoadServUo,
            this._miSrvLoadSphere,
            this._miSrvLoadPol,
            this._sepSrv1,
            this._miSrvSaveServUo,
            this._miSrvSaveSphere,
            this._miSrvSavePol,
            this._sepSrv2,
            this._miSrvSettings});
            this._ddServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._ddServer.Name = "_ddServer";
            this._ddServer.Text = "⇅ Server";
            this._ddServer.ToolTipText = "Load the regions of your server or save them into it";
            this._ddServer.DropDownOpening += new System.EventHandler(this.OnServerMenuOpening);
            // 
            // _miSrvLoadServUo
            // 
            this._miSrvLoadServUo.Name = "_miSrvLoadServUo";
            this._miSrvLoadServUo.Text = "Load from ServUO";
            this._miSrvLoadServUo.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _miSrvLoadSphere
            // 
            this._miSrvLoadSphere.Name = "_miSrvLoadSphere";
            this._miSrvLoadSphere.Text = "Load from Sphere";
            this._miSrvLoadSphere.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _miSrvLoadPol
            // 
            this._miSrvLoadPol.Name = "_miSrvLoadPol";
            this._miSrvLoadPol.Text = "Load from POL";
            this._miSrvLoadPol.Click += new System.EventHandler(this.OnClickLoadServer);
            // 
            // _sepSrv1
            // 
            this._sepSrv1.Name = "_sepSrv1";
            // 
            // _miSrvSaveServUo
            // 
            this._miSrvSaveServUo.Name = "_miSrvSaveServUo";
            this._miSrvSaveServUo.Text = "Save into ServUO";
            this._miSrvSaveServUo.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _miSrvSaveSphere
            // 
            this._miSrvSaveSphere.Name = "_miSrvSaveSphere";
            this._miSrvSaveSphere.Text = "Save into Sphere";
            this._miSrvSaveSphere.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _miSrvSavePol
            // 
            this._miSrvSavePol.Name = "_miSrvSavePol";
            this._miSrvSavePol.Text = "Save into POL";
            this._miSrvSavePol.Click += new System.EventHandler(this.OnClickSaveServer);
            // 
            // _sepSrv2
            // 
            this._sepSrv2.Name = "_sepSrv2";
            // 
            // _miSrvSettings
            // 
            this._miSrvSettings.Name = "_miSrvSettings";
            this._miSrvSettings.Text = "Server folders…";
            this._miSrvSettings.Click += new System.EventHandler(this.OnClickSettings);
            // 
            // _sepMain3
            // 
            this._sepMain3.Name = "_sepMain3";
            // 
            // _btnCheck
            // 
            this._btnCheck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnCheck.Name = "_btnCheck";
            this._btnCheck.Text = "✔ Check";
            this._btnCheck.ToolTipText = "Check all regions";
            this._btnCheck.Click += new System.EventHandler(this.OnClickRunCheck);
            // 
            // _btnSettings
            // 
            this._btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSettings.Name = "_btnSettings";
            this._btnSettings.Text = "⚙ Settings";
            this._btnSettings.ToolTipText = "Server folders, export, design, language";
            this._btnSettings.Click += new System.EventHandler(this.OnClickSettings);
            // 
            // _btnHelp
            // 
            this._btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHelp.Name = "_btnHelp";
            this._btnHelp.Text = "? Help";
            this._btnHelp.Click += new System.EventHandler(this.OnClickHelp);
            // 
            // _btnTheme
            // 
            this._btnTheme.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._btnTheme.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnTheme.Name = "_btnTheme";
            this._btnTheme.Text = "◐ Dark";
            this._btnTheme.ToolTipText = "Switch between light and dark design";
            this._btnTheme.Click += new System.EventHandler(this.OnClickTheme);
            // 
            // _cmbLang
            // 
            this._cmbLang.Items.AddRange(new object[] {
            "Deutsch",
            "English",
            "Français"});
            this._cmbLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbLang.Name = "_cmbLang";
            this._cmbLang.Size = new System.Drawing.Size(100, 27);
            this._cmbLang.SelectedIndexChanged += new System.EventHandler(this.OnLanguageSelected);
            // 
            // _lblLang
            // 
            this._lblLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblLang.Name = "_lblLang";
            this._lblLang.Text = "Language:";
            // 
            // _ctxTree
            // 
            this._ctxTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ctxAdd,
            this._ctxAddChild,
            this._ctxDuplicate,
            this._ctxRename,
            this._ctxDelete,
            this._sepCtx1,
            this._ctxZoomTo,
            this._ctxSendGo,
            this._ctxCopyXml,
            this._sepCtx2,
            this._ctxOptimize,
            this._ctxPolyToRects,
            this._ctxToTop,
            this._sepCtx3,
            this._ctxHide,
            this._ctxLock});
            this._ctxTree.Name = "_ctxTree";
            this._ctxTree.Opening += new System.ComponentModel.CancelEventHandler(this.OnTreeMenuOpening);
            // 
            // _ctxAdd
            // 
            this._ctxAdd.Name = "_ctxAdd";
            this._ctxAdd.Text = "New region";
            this._ctxAdd.Click += new System.EventHandler(this.OnClickAddRegion);
            // 
            // _ctxAddChild
            // 
            this._ctxAddChild.Name = "_ctxAddChild";
            this._ctxAddChild.Text = "New child region";
            this._ctxAddChild.Click += new System.EventHandler(this.OnClickAddChild);
            // 
            // _ctxDuplicate
            // 
            this._ctxDuplicate.Name = "_ctxDuplicate";
            this._ctxDuplicate.Text = "Duplicate";
            this._ctxDuplicate.Click += new System.EventHandler(this.OnClickDuplicate);
            // 
            // _ctxRename
            // 
            this._ctxRename.Name = "_ctxRename";
            this._ctxRename.Text = "Rename";
            this._ctxRename.Click += new System.EventHandler(this.OnClickRename);
            // 
            // _ctxDelete
            // 
            this._ctxDelete.Name = "_ctxDelete";
            this._ctxDelete.Text = "Delete";
            this._ctxDelete.Click += new System.EventHandler(this.OnClickDeleteRegion);
            // 
            // _sepCtx1
            // 
            this._sepCtx1.Name = "_sepCtx1";
            // 
            // _ctxZoomTo
            // 
            this._ctxZoomTo.Name = "_ctxZoomTo";
            this._ctxZoomTo.Text = "Show on map";
            this._ctxZoomTo.Click += new System.EventHandler(this.OnClickZoomFit);
            // 
            // _ctxSendGo
            // 
            this._ctxSendGo.Name = "_ctxSendGo";
            this._ctxSendGo.Text = "Send client to the go location";
            this._ctxSendGo.Click += new System.EventHandler(this.OnClickSendGo);
            // 
            // _ctxCopyXml
            // 
            this._ctxCopyXml.Name = "_ctxCopyXml";
            this._ctxCopyXml.Text = "Copy as ServUO XML";
            this._ctxCopyXml.Click += new System.EventHandler(this.OnClickCopyXml);
            // 
            // _sepCtx2
            // 
            this._sepCtx2.Name = "_sepCtx2";
            // 
            // _ctxOptimize
            // 
            this._ctxOptimize.Name = "_ctxOptimize";
            this._ctxOptimize.Text = "Optimize rectangles";
            this._ctxOptimize.Click += new System.EventHandler(this.OnClickRectOptimize);
            // 
            // _ctxPolyToRects
            // 
            this._ctxPolyToRects.Name = "_ctxPolyToRects";
            this._ctxPolyToRects.Text = "Polygons → rectangles";
            this._ctxPolyToRects.Click += new System.EventHandler(this.OnClickPolyToRects);
            // 
            // _ctxToTop
            // 
            this._ctxToTop.Name = "_ctxToTop";
            this._ctxToTop.Text = "Make top level region";
            this._ctxToTop.Click += new System.EventHandler(this.OnClickToTop);
            // 
            // _sepCtx3
            // 
            this._sepCtx3.Name = "_sepCtx3";
            // 
            // _ctxHide
            // 
            this._ctxHide.Name = "_ctxHide";
            this._ctxHide.Text = "Hidden";
            this._ctxHide.Click += new System.EventHandler(this.OnClickToggleHidden);
            // 
            // _ctxLock
            // 
            this._ctxLock.Name = "_ctxLock";
            this._ctxLock.Text = "Locked";
            this._ctxLock.Click += new System.EventHandler(this.OnClickToggleLocked);
            // 
            // _ctxMap
            // 
            this._ctxMap.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._ctxMapSelect,
            this._ctxMapSetGo,
            this._ctxMapSetEntrance,
            this._ctxMapDeleteRect,
            this._sepMapCtx,
            this._ctxMapSendChar,
            this._ctxMapCopyPos});
            this._ctxMap.Name = "_ctxMap";
            this._ctxMap.Opening += new System.ComponentModel.CancelEventHandler(this.OnMapMenuOpening);
            // 
            // _ctxMapSelect
            // 
            this._ctxMapSelect.Name = "_ctxMapSelect";
            this._ctxMapSelect.Text = "Select region here";
            this._ctxMapSelect.Click += new System.EventHandler(this.OnClickMapSelect);
            // 
            // _ctxMapSetGo
            // 
            this._ctxMapSetGo.Name = "_ctxMapSetGo";
            this._ctxMapSetGo.Text = "Set go location here";
            this._ctxMapSetGo.Click += new System.EventHandler(this.OnClickMapSetGo);
            // 
            // _ctxMapSetEntrance
            // 
            this._ctxMapSetEntrance.Name = "_ctxMapSetEntrance";
            this._ctxMapSetEntrance.Text = "Set entrance here";
            this._ctxMapSetEntrance.Click += new System.EventHandler(this.OnClickMapSetEntrance);
            // 
            // _ctxMapDeleteRect
            // 
            this._ctxMapDeleteRect.Name = "_ctxMapDeleteRect";
            this._ctxMapDeleteRect.Text = "Delete area here";
            this._ctxMapDeleteRect.Click += new System.EventHandler(this.OnClickMapDeleteRect);
            // 
            // _sepMapCtx
            // 
            this._sepMapCtx.Name = "_sepMapCtx";
            // 
            // _ctxMapSendChar
            // 
            this._ctxMapSendChar.Name = "_ctxMapSendChar";
            this._ctxMapSendChar.Text = "Send client here";
            this._ctxMapSendChar.Click += new System.EventHandler(this.OnClickMapSendChar);
            // 
            // _ctxMapCopyPos
            // 
            this._ctxMapCopyPos.Name = "_ctxMapCopyPos";
            this._ctxMapCopyPos.Text = "Copy position";
            this._ctxMapCopyPos.Click += new System.EventHandler(this.OnClickMapCopyPos);
            // 
            // _clientTimer
            // 
            this._clientTimer.Interval = 1000;
            this._clientTimer.Tick += new System.EventHandler(this.OnClientTimer);
            // 
            // _toolTip
            // 
            // 
            // _colorDialog
            // 
            this._colorDialog.FullOpen = true;
            // 
            // RegionEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Size = new System.Drawing.Size(1280, 760);
            this.Controls.Add(this._splitMain);
            this.Controls.Add(this._status);
            this.Controls.Add(this._toolTools);
            this.Controls.Add(this._toolMain);
            this.Name = "RegionEditorControl";
            this.Load += new System.EventHandler(this.OnControlLoad);
            this._splitMain.Panel1.ResumeLayout(false);
            this._splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitMain)).EndInit();
            this._splitMain.ResumeLayout(false);
            this._searchPanel.ResumeLayout(false);
            this._searchPanel.PerformLayout();
            this._toolTree.ResumeLayout(false);
            this._toolTree.PerformLayout();
            this._splitRight.Panel1.ResumeLayout(false);
            this._splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitRight)).EndInit();
            this._splitRight.ResumeLayout(false);
            this._tabs.ResumeLayout(false);
            this._tabProps.ResumeLayout(false);
            this._tabProps.PerformLayout();
            this._tblProps.ResumeLayout(false);
            this._tblProps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPriority)).EndInit();
            this._pnlZ.ResumeLayout(false);
            this._pnlZ.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numZMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numZMax)).EndInit();
            this._pnlGo.ResumeLayout(false);
            this._pnlGo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numGoX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGoY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numGoZ)).EndInit();
            this._pnlEntrance.ResumeLayout(false);
            this._pnlEntrance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numEnX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numEnY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numEnZ)).EndInit();
            this._pnlGuards.ResumeLayout(false);
            this._pnlGuards.PerformLayout();
            this._pnlOptions.ResumeLayout(false);
            this._pnlOptions.PerformLayout();
            this._pnlColor.ResumeLayout(false);
            this._pnlColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._gridProps)).EndInit();
            this._tabRects.ResumeLayout(false);
            this._tabRects.PerformLayout();
            this._pnlRectEdit.ResumeLayout(false);
            this._pnlRectEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numRX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRZ1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numRZ2)).EndInit();
            this._toolRects.ResumeLayout(false);
            this._toolRects.PerformLayout();
            this._tabCheck.ResumeLayout(false);
            this._tabCheck.PerformLayout();
            this._toolCheck.ResumeLayout(false);
            this._toolCheck.PerformLayout();
            this._tabHistory.ResumeLayout(false);
            this._tabHistory.PerformLayout();
            this._toolHistory.ResumeLayout(false);
            this._toolHistory.PerformLayout();
            this._status.ResumeLayout(false);
            this._status.PerformLayout();
            this._toolTools.ResumeLayout(false);
            this._toolTools.PerformLayout();
            this._toolMain.ResumeLayout(false);
            this._toolMain.PerformLayout();
            this._ctxTree.ResumeLayout(false);
            this._ctxMap.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitMain;
        private System.Windows.Forms.TreeView _tree;
        private System.Windows.Forms.Panel _searchPanel;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.ToolStrip _toolTree;
        private System.Windows.Forms.ToolStripButton _btnAddRegion;
        private System.Windows.Forms.ToolStripButton _btnAddChild;
        private System.Windows.Forms.ToolStripButton _btnDuplicate;
        private System.Windows.Forms.ToolStripButton _btnDeleteRegion;
        private System.Windows.Forms.ToolStripSeparator _sepTree1;
        private System.Windows.Forms.ToolStripButton _btnMoveUp;
        private System.Windows.Forms.ToolStripButton _btnMoveDown;
        private System.Windows.Forms.ToolStripSeparator _sepTree2;
        private System.Windows.Forms.ToolStripButton _btnExpandAll;
        private System.Windows.Forms.ToolStripButton _btnCollapseAll;
        private System.Windows.Forms.SplitContainer _splitRight;
        private UoFiddler.Plugin.RegionEditor.UserControls.RegionMapView _mapView;
        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tabProps;
        private System.Windows.Forms.TableLayoutPanel _tblProps;
        private System.Windows.Forms.Label _lblPName;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Label _lblPType;
        private System.Windows.Forms.ComboBox _cmbType;
        private System.Windows.Forms.Label _lblPPriority;
        private System.Windows.Forms.NumericUpDown _numPriority;
        private System.Windows.Forms.Label _lblPMusic;
        private System.Windows.Forms.ComboBox _cmbMusic;
        private System.Windows.Forms.Label _lblPZ;
        private System.Windows.Forms.FlowLayoutPanel _pnlZ;
        private System.Windows.Forms.CheckBox _chkZ;
        private System.Windows.Forms.NumericUpDown _numZMin;
        private System.Windows.Forms.NumericUpDown _numZMax;
        private System.Windows.Forms.Label _lblPGo;
        private System.Windows.Forms.FlowLayoutPanel _pnlGo;
        private System.Windows.Forms.CheckBox _chkGo;
        private System.Windows.Forms.NumericUpDown _numGoX;
        private System.Windows.Forms.NumericUpDown _numGoY;
        private System.Windows.Forms.NumericUpDown _numGoZ;
        private System.Windows.Forms.Button _btnPickGo;
        private System.Windows.Forms.Label _lblPEntrance;
        private System.Windows.Forms.FlowLayoutPanel _pnlEntrance;
        private System.Windows.Forms.CheckBox _chkEntrance;
        private System.Windows.Forms.NumericUpDown _numEnX;
        private System.Windows.Forms.NumericUpDown _numEnY;
        private System.Windows.Forms.NumericUpDown _numEnZ;
        private System.Windows.Forms.Button _btnPickEntrance;
        private System.Windows.Forms.ComboBox _cmbEnMap;
        private System.Windows.Forms.Label _lblPGuards;
        private System.Windows.Forms.FlowLayoutPanel _pnlGuards;
        private System.Windows.Forms.CheckBox _chkGuardsOff;
        private System.Windows.Forms.TextBox _txtGuardsType;
        private System.Windows.Forms.Label _lblPRune;
        private System.Windows.Forms.TextBox _txtRune;
        private System.Windows.Forms.Label _lblPOptions;
        private System.Windows.Forms.FlowLayoutPanel _pnlOptions;
        private System.Windows.Forms.CheckBox _chkSmartNoHousing;
        private System.Windows.Forms.CheckBox _chkLogoutDelay;
        private System.Windows.Forms.CheckBox _chkHidden;
        private System.Windows.Forms.CheckBox _chkLocked;
        private System.Windows.Forms.Label _lblPColor;
        private System.Windows.Forms.FlowLayoutPanel _pnlColor;
        private System.Windows.Forms.Button _btnColor;
        private System.Windows.Forms.Button _btnColorReset;
        private System.Windows.Forms.Label _lblPNotes;
        private System.Windows.Forms.TextBox _txtNotes;
        private System.Windows.Forms.Label _lblPExtra;
        private System.Windows.Forms.DataGridView _gridProps;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn _colValue;
        private System.Windows.Forms.Label _lblRegionInfo;
        private System.Windows.Forms.TabPage _tabRects;
        private System.Windows.Forms.ListView _listRects;
        private System.Windows.Forms.ColumnHeader _colRNo;
        private System.Windows.Forms.ColumnHeader _colRX;
        private System.Windows.Forms.ColumnHeader _colRY;
        private System.Windows.Forms.ColumnHeader _colRW;
        private System.Windows.Forms.ColumnHeader _colRH;
        private System.Windows.Forms.ColumnHeader _colRZ;
        private System.Windows.Forms.TableLayoutPanel _pnlRectEdit;
        private System.Windows.Forms.Label _lblRX;
        private System.Windows.Forms.NumericUpDown _numRX;
        private System.Windows.Forms.Label _lblRY;
        private System.Windows.Forms.NumericUpDown _numRY;
        private System.Windows.Forms.Label _lblRW;
        private System.Windows.Forms.NumericUpDown _numRW;
        private System.Windows.Forms.Label _lblRH;
        private System.Windows.Forms.NumericUpDown _numRH;
        private System.Windows.Forms.CheckBox _chkRZ;
        private System.Windows.Forms.NumericUpDown _numRZ1;
        private System.Windows.Forms.Label _lblRZTo;
        private System.Windows.Forms.NumericUpDown _numRZ2;
        private System.Windows.Forms.ToolStrip _toolRects;
        private System.Windows.Forms.ToolStripButton _btnRectAdd;
        private System.Windows.Forms.ToolStripButton _btnRectApply;
        private System.Windows.Forms.ToolStripButton _btnRectDelete;
        private System.Windows.Forms.ToolStripSeparator _sepRects;
        private System.Windows.Forms.ToolStripButton _btnRectOptimize;
        private System.Windows.Forms.ToolStripButton _btnPolyToRects;
        private System.Windows.Forms.TabPage _tabCheck;
        private System.Windows.Forms.ListView _listIssues;
        private System.Windows.Forms.ColumnHeader _colISev;
        private System.Windows.Forms.ColumnHeader _colIRegion;
        private System.Windows.Forms.ColumnHeader _colIMsg;
        private System.Windows.Forms.ToolStrip _toolCheck;
        private System.Windows.Forms.ToolStripButton _btnRunCheck;
        private System.Windows.Forms.ToolStripButton _btnFixSelected;
        private System.Windows.Forms.ToolStripButton _btnFixAll;
        private System.Windows.Forms.ToolStripLabel _lblCheckSummary;
        private System.Windows.Forms.TabPage _tabHistory;
        private System.Windows.Forms.ListBox _listHistory;
        private System.Windows.Forms.ToolStrip _toolHistory;
        private System.Windows.Forms.ToolStripButton _btnHistUndo;
        private System.Windows.Forms.ToolStripButton _btnHistRedo;
        private System.Windows.Forms.ToolStripButton _btnHistJump;
        private System.Windows.Forms.ToolStripButton _btnHistClear;
        private System.Windows.Forms.StatusStrip _status;
        private System.Windows.Forms.ToolStripStatusLabel _lblPos;
        private System.Windows.Forms.ToolStripStatusLabel _lblTile;
        private System.Windows.Forms.ToolStripStatusLabel _lblHover;
        private System.Windows.Forms.ToolStripStatusLabel _lblMessage;
        private System.Windows.Forms.ToolStripStatusLabel _lblClient;
        private System.Windows.Forms.ToolStripStatusLabel _lblCount;
        private System.Windows.Forms.ToolStrip _toolTools;
        private System.Windows.Forms.ToolStripButton _btnToolSelect;
        private System.Windows.Forms.ToolStripButton _btnToolPan;
        private System.Windows.Forms.ToolStripButton _btnToolRect;
        private System.Windows.Forms.ToolStripButton _btnToolPolygon;
        private System.Windows.Forms.ToolStripButton _btnToolCircle;
        private System.Windows.Forms.ToolStripButton _btnToolFill;
        private System.Windows.Forms.ToolStripButton _btnToolGo;
        private System.Windows.Forms.ToolStripButton _btnToolEntrance;
        private System.Windows.Forms.ToolStripSeparator _sepTools1;
        private System.Windows.Forms.ToolStripLabel _lblSnap;
        private System.Windows.Forms.ToolStripComboBox _cmbSnap;
        private System.Windows.Forms.ToolStripSeparator _sepTools2;
        private System.Windows.Forms.ToolStripButton _btnZoomIn;
        private System.Windows.Forms.ToolStripButton _btnZoomOut;
        private System.Windows.Forms.ToolStripButton _btnZoomFit;
        private System.Windows.Forms.ToolStripLabel _lblZoom;
        private System.Windows.Forms.ToolStripSeparator _sepTools3;
        private System.Windows.Forms.ToolStripDropDownButton _ddView;
        private System.Windows.Forms.ToolStripMenuItem _miViewStatics;
        private System.Windows.Forms.ToolStripMenuItem _miViewGrid;
        private System.Windows.Forms.ToolStripMenuItem _miViewLabels;
        private System.Windows.Forms.ToolStripMenuItem _miViewMinimap;
        private System.Windows.Forms.ToolStripMenuItem _miViewClient;
        private System.Windows.Forms.ToolStripSeparator _sepView;
        private System.Windows.Forms.ToolStripMenuItem _miViewRefresh;
        private System.Windows.Forms.ToolStripSeparator _sepTools4;
        private System.Windows.Forms.ToolStripButton _btnClientGoto;
        private System.Windows.Forms.ToolStripButton _btnSyncClient;
        private System.Windows.Forms.ToolStrip _toolMain;
        private System.Windows.Forms.ToolStripDropDownButton _ddFile;
        private System.Windows.Forms.ToolStripMenuItem _miNew;
        private System.Windows.Forms.ToolStripMenuItem _miOpen;
        private System.Windows.Forms.ToolStripMenuItem _miRecent;
        private System.Windows.Forms.ToolStripMenuItem _miSave;
        private System.Windows.Forms.ToolStripMenuItem _miSaveAs;
        private System.Windows.Forms.ToolStripSeparator _sepF1;
        private System.Windows.Forms.ToolStripMenuItem _miImport;
        private System.Windows.Forms.ToolStripMenuItem _miImportServUo;
        private System.Windows.Forms.ToolStripMenuItem _miImportSphere;
        private System.Windows.Forms.ToolStripMenuItem _miImportPol;
        private System.Windows.Forms.ToolStripMenuItem _miImportPolFolder;
        private System.Windows.Forms.ToolStripMenuItem _miImportCsv;
        private System.Windows.Forms.ToolStripMenuItem _miImportProject;
        private System.Windows.Forms.ToolStripMenuItem _miExport;
        private System.Windows.Forms.ToolStripMenuItem _miExpServUo;
        private System.Windows.Forms.ToolStripMenuItem _miExpSphere;
        private System.Windows.Forms.ToolStripMenuItem _miExpPolRegions;
        private System.Windows.Forms.ToolStripMenuItem _miExpPolJustice;
        private System.Windows.Forms.ToolStripMenuItem _miExpPolMusic;
        private System.Windows.Forms.ToolStripSeparator _sepE1;
        private System.Windows.Forms.ToolStripMenuItem _miExpCsv;
        private System.Windows.Forms.ToolStripMenuItem _miExpTxt;
        private System.Windows.Forms.ToolStripMenuItem _miExpHtml;
        private System.Windows.Forms.ToolStripMenuItem _miExpPng;
        private System.Windows.Forms.ToolStripMenuItem _miExpCSharp;
        private System.Windows.Forms.ToolStripSeparator _sepE2;
        private System.Windows.Forms.ToolStripMenuItem _miExpAll;
        private System.Windows.Forms.ToolStripMenuItem _miOpenExportDir;
        private System.Windows.Forms.ToolStripSeparator _sepF2;
        private System.Windows.Forms.ToolStripMenuItem _miLoadServer;
        private System.Windows.Forms.ToolStripMenuItem _miLoadServUo;
        private System.Windows.Forms.ToolStripMenuItem _miLoadSphere;
        private System.Windows.Forms.ToolStripMenuItem _miLoadPol;
        private System.Windows.Forms.ToolStripMenuItem _miSaveServer;
        private System.Windows.Forms.ToolStripMenuItem _miSaveServUo;
        private System.Windows.Forms.ToolStripMenuItem _miSaveSphere;
        private System.Windows.Forms.ToolStripMenuItem _miSavePol;
        private System.Windows.Forms.ToolStripSeparator _sepF3;
        private System.Windows.Forms.ToolStripMenuItem _miSettings;
        private System.Windows.Forms.ToolStripButton _btnUndo;
        private System.Windows.Forms.ToolStripButton _btnRedo;
        private System.Windows.Forms.ToolStripSeparator _sepMain1;
        private System.Windows.Forms.ToolStripLabel _lblMap;
        private System.Windows.Forms.ToolStripComboBox _cmbMap;
        private System.Windows.Forms.ToolStripSeparator _sepMain2;
        private System.Windows.Forms.ToolStripDropDownButton _ddServer;
        private System.Windows.Forms.ToolStripMenuItem _miSrvLoadServUo;
        private System.Windows.Forms.ToolStripMenuItem _miSrvLoadSphere;
        private System.Windows.Forms.ToolStripMenuItem _miSrvLoadPol;
        private System.Windows.Forms.ToolStripSeparator _sepSrv1;
        private System.Windows.Forms.ToolStripMenuItem _miSrvSaveServUo;
        private System.Windows.Forms.ToolStripMenuItem _miSrvSaveSphere;
        private System.Windows.Forms.ToolStripMenuItem _miSrvSavePol;
        private System.Windows.Forms.ToolStripSeparator _sepSrv2;
        private System.Windows.Forms.ToolStripMenuItem _miSrvSettings;
        private System.Windows.Forms.ToolStripSeparator _sepMain3;
        private System.Windows.Forms.ToolStripButton _btnCheck;
        private System.Windows.Forms.ToolStripButton _btnSettings;
        private System.Windows.Forms.ToolStripButton _btnHelp;
        private System.Windows.Forms.ToolStripButton _btnTheme;
        private System.Windows.Forms.ToolStripComboBox _cmbLang;
        private System.Windows.Forms.ToolStripLabel _lblLang;
        private System.Windows.Forms.ContextMenuStrip _ctxTree;
        private System.Windows.Forms.ToolStripMenuItem _ctxAdd;
        private System.Windows.Forms.ToolStripMenuItem _ctxAddChild;
        private System.Windows.Forms.ToolStripMenuItem _ctxDuplicate;
        private System.Windows.Forms.ToolStripMenuItem _ctxRename;
        private System.Windows.Forms.ToolStripMenuItem _ctxDelete;
        private System.Windows.Forms.ToolStripSeparator _sepCtx1;
        private System.Windows.Forms.ToolStripMenuItem _ctxZoomTo;
        private System.Windows.Forms.ToolStripMenuItem _ctxSendGo;
        private System.Windows.Forms.ToolStripMenuItem _ctxCopyXml;
        private System.Windows.Forms.ToolStripSeparator _sepCtx2;
        private System.Windows.Forms.ToolStripMenuItem _ctxOptimize;
        private System.Windows.Forms.ToolStripMenuItem _ctxPolyToRects;
        private System.Windows.Forms.ToolStripMenuItem _ctxToTop;
        private System.Windows.Forms.ToolStripSeparator _sepCtx3;
        private System.Windows.Forms.ToolStripMenuItem _ctxHide;
        private System.Windows.Forms.ToolStripMenuItem _ctxLock;
        private System.Windows.Forms.ContextMenuStrip _ctxMap;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapSelect;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapSetGo;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapSetEntrance;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapDeleteRect;
        private System.Windows.Forms.ToolStripSeparator _sepMapCtx;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapSendChar;
        private System.Windows.Forms.ToolStripMenuItem _ctxMapCopyPos;
        private System.Windows.Forms.Timer _clientTimer;
        private System.Windows.Forms.ToolTip _toolTip;
        private System.Windows.Forms.ColorDialog _colorDialog;
    }
}

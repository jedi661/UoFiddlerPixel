namespace UoFiddler.Plugin.ClientCheck.UserControls
{
    partial class ClientCheckControl
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
                Classes.Loc.LanguageChanged -= OnLanguageChanged;
                _cts?.Cancel();
                _boldFont.Dispose();
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
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this._btnStart = new System.Windows.Forms.ToolStripButton();
            this._btnCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._ddChecks = new System.Windows.Forms.ToolStripDropDownButton();
            this.checkArtTileDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkLandTilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkEquipmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBodiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkRadarColMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkMultisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkStaticsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkClilocMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._checkAll = new System.Windows.Forms.ToolStripMenuItem();
            this._checkNone = new System.Windows.Forms.ToolStripMenuItem();
            this._btnHelp = new System.Windows.Forms.ToolStripButton();
            this._cmbLang = new System.Windows.Forms.ToolStripComboBox();
            this._lblLang = new System.Windows.Forms.ToolStripLabel();
            this.toolStripFilter = new System.Windows.Forms.ToolStrip();
            this._lblShow = new System.Windows.Forms.ToolStripLabel();
            this._cmbSeverity = new System.Windows.Forms.ToolStripComboBox();
            this._cmbCategory = new System.Windows.Forms.ToolStripComboBox();
            this._lblSearch = new System.Windows.Forms.ToolStripLabel();
            this._txtSearch = new System.Windows.Forms.ToolStripTextBox();
            this._btnHideFixed = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._btnFixAll = new System.Windows.Forms.ToolStripButton();
            this._btnSaveRadar = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._ddExport = new System.Windows.Forms.ToolStripDropDownButton();
            this._exportHtml = new System.Windows.Forms.ToolStripMenuItem();
            this._exportCsv = new System.Windows.Forms.ToolStripMenuItem();
            this._exportTxt = new System.Windows.Forms.ToolStripMenuItem();
            this._summary = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this._list = new System.Windows.Forms.ListView();
            this.columnSeverity = new System.Windows.Forms.ColumnHeader();
            this.columnCategory = new System.Windows.Forms.ColumnHeader();
            this.columnId = new System.Windows.Forms.ColumnHeader();
            this.columnMessage = new System.Windows.Forms.ColumnHeader();
            this.detailsPanel = new System.Windows.Forms.Panel();
            this._preview = new System.Windows.Forms.PictureBox();
            this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnFix = new System.Windows.Forms.Button();
            this._btnCopyId = new System.Windows.Forms.Button();
            this._btnCopyLine = new System.Windows.Forms.Button();
            this._detailFix = new System.Windows.Forms.Label();
            this._detailHint = new System.Windows.Forms.Label();
            this._detailHintTitle = new System.Windows.Forms.Label();
            this._detailMessage = new System.Windows.Forms.Label();
            this._detailTitle = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this._progress = new System.Windows.Forms.ToolStripProgressBar();
            this._status = new System.Windows.Forms.ToolStripStatusLabel();
            this._shown = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripMain.SuspendLayout();
            this.toolStripFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.detailsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._preview)).BeginInit();
            this.buttonsPanel.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // toolStripMain
            //
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnStart,
            this._btnCancel,
            this.toolStripSeparator1,
            this._ddChecks,
            this._btnHelp,
            this._cmbLang,
            this._lblLang});
            this.toolStripMain.Location = new System.Drawing.Point(0, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripMain.Size = new System.Drawing.Size(1000, 27);
            this.toolStripMain.TabIndex = 0;
            //
            // _btnStart
            //
            this._btnStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnStart.Name = "_btnStart";
            this._btnStart.Size = new System.Drawing.Size(115, 20);
            this._btnStart.Text = "▶ Prüfung starten";
            this._btnStart.Click += new System.EventHandler(this.OnClickStart);
            //
            // _btnCancel
            //
            this._btnCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnCancel.Enabled = false;
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(77, 20);
            this._btnCancel.Text = "■ Abbrechen";
            this._btnCancel.Click += new System.EventHandler(this.OnClickCancel);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            //
            // _ddChecks
            //
            this._ddChecks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._ddChecks.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkArtTileDataMenuItem,
            this.checkLandTilesMenuItem,
            this.checkEquipmentMenuItem,
            this.checkBodiesMenuItem,
            this.checkRadarColMenuItem,
            this.checkMultisMenuItem,
            this.checkStaticsMenuItem,
            this.checkClilocMenuItem,
            this.toolStripSeparator2,
            this._checkAll,
            this._checkNone});
            this._ddChecks.Name = "_ddChecks";
            this._ddChecks.Size = new System.Drawing.Size(76, 20);
            this._ddChecks.Text = "Prüfungen";
            //
            // checkArtTileDataMenuItem
            //
            this.checkArtTileDataMenuItem.Checked = true;
            this.checkArtTileDataMenuItem.CheckOnClick = true;
            this.checkArtTileDataMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkArtTileDataMenuItem.Name = "checkArtTileDataMenuItem";
            this.checkArtTileDataMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkArtTileDataMenuItem.Text = "Art ↔ tiledata";
            this.checkArtTileDataMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkLandTilesMenuItem
            //
            this.checkLandTilesMenuItem.Checked = true;
            this.checkLandTilesMenuItem.CheckOnClick = true;
            this.checkLandTilesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkLandTilesMenuItem.Name = "checkLandTilesMenuItem";
            this.checkLandTilesMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkLandTilesMenuItem.Text = "Land-Tiles";
            this.checkLandTilesMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkEquipmentMenuItem
            //
            this.checkEquipmentMenuItem.Checked = true;
            this.checkEquipmentMenuItem.CheckOnClick = true;
            this.checkEquipmentMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEquipmentMenuItem.Name = "checkEquipmentMenuItem";
            this.checkEquipmentMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkEquipmentMenuItem.Text = "Ausrüstung";
            this.checkEquipmentMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkBodiesMenuItem
            //
            this.checkBodiesMenuItem.Checked = true;
            this.checkBodiesMenuItem.CheckOnClick = true;
            this.checkBodiesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBodiesMenuItem.Name = "checkBodiesMenuItem";
            this.checkBodiesMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkBodiesMenuItem.Text = "Bodies / Animationen";
            this.checkBodiesMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkRadarColMenuItem
            //
            this.checkRadarColMenuItem.Checked = true;
            this.checkRadarColMenuItem.CheckOnClick = true;
            this.checkRadarColMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkRadarColMenuItem.Name = "checkRadarColMenuItem";
            this.checkRadarColMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkRadarColMenuItem.Text = "radarcol (Kartenfarben)";
            this.checkRadarColMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkMultisMenuItem
            //
            this.checkMultisMenuItem.Checked = true;
            this.checkMultisMenuItem.CheckOnClick = true;
            this.checkMultisMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMultisMenuItem.Name = "checkMultisMenuItem";
            this.checkMultisMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkMultisMenuItem.Text = "Multis";
            this.checkMultisMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkStaticsMenuItem
            //
            this.checkStaticsMenuItem.Checked = true;
            this.checkStaticsMenuItem.CheckOnClick = true;
            this.checkStaticsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkStaticsMenuItem.Name = "checkStaticsMenuItem";
            this.checkStaticsMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkStaticsMenuItem.Text = "Statics auf Karten";
            this.checkStaticsMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // checkClilocMenuItem
            //
            this.checkClilocMenuItem.Checked = true;
            this.checkClilocMenuItem.CheckOnClick = true;
            this.checkClilocMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkClilocMenuItem.Name = "checkClilocMenuItem";
            this.checkClilocMenuItem.Size = new System.Drawing.Size(220, 22);
            this.checkClilocMenuItem.Text = "Cliloc (Namen)";
            this.checkClilocMenuItem.CheckedChanged += new System.EventHandler(this.OnCheckItemChanged);
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(217, 6);
            //
            // _checkAll
            //
            this._checkAll.Name = "_checkAll";
            this._checkAll.Size = new System.Drawing.Size(220, 22);
            this._checkAll.Text = "Alle auswählen";
            this._checkAll.Click += new System.EventHandler(this.OnClickCheckAll);
            //
            // _checkNone
            //
            this._checkNone.Name = "_checkNone";
            this._checkNone.Size = new System.Drawing.Size(220, 22);
            this._checkNone.Text = "Keine auswählen";
            this._checkNone.Click += new System.EventHandler(this.OnClickCheckNone);
            //
            // _btnHelp
            //
            this._btnHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHelp.Name = "_btnHelp";
            this._btnHelp.Size = new System.Drawing.Size(45, 20);
            this._btnHelp.Text = "? Hilfe";
            this._btnHelp.Click += new System.EventHandler(this.OnClickHelp);
            //
            // _cmbLang
            //
            this._cmbLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._cmbLang.AutoSize = false;
            this._cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbLang.Items.AddRange(new object[] {
            "Deutsch",
            "English",
            "Français"});
            this._cmbLang.Name = "_cmbLang";
            this._cmbLang.Size = new System.Drawing.Size(95, 23);
            this._cmbLang.SelectedIndexChanged += new System.EventHandler(this.OnLanguageSelected);
            //
            // _lblLang
            //
            this._lblLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblLang.Name = "_lblLang";
            this._lblLang.Size = new System.Drawing.Size(52, 20);
            this._lblLang.Text = "Sprache:";
            //
            // toolStripFilter
            //
            this.toolStripFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblShow,
            this._cmbSeverity,
            this._cmbCategory,
            this._lblSearch,
            this._txtSearch,
            this._btnHideFixed,
            this.toolStripSeparator3,
            this._btnFixAll,
            this._btnSaveRadar,
            this.toolStripSeparator4,
            this._ddExport});
            this.toolStripFilter.Location = new System.Drawing.Point(0, 27);
            this.toolStripFilter.Name = "toolStripFilter";
            this.toolStripFilter.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.toolStripFilter.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripFilter.Size = new System.Drawing.Size(1000, 27);
            this.toolStripFilter.TabIndex = 1;
            //
            // _lblShow
            //
            this._lblShow.Name = "_lblShow";
            this._lblShow.Size = new System.Drawing.Size(60, 20);
            this._lblShow.Text = "Anzeigen:";
            //
            // _cmbSeverity
            //
            this._cmbSeverity.AutoSize = false;
            this._cmbSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSeverity.Name = "_cmbSeverity";
            this._cmbSeverity.Size = new System.Drawing.Size(120, 23);
            this._cmbSeverity.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // _cmbCategory
            //
            this._cmbCategory.AutoSize = false;
            this._cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbCategory.Name = "_cmbCategory";
            this._cmbCategory.Size = new System.Drawing.Size(170, 23);
            this._cmbCategory.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // _lblSearch
            //
            this._lblSearch.Name = "_lblSearch";
            this._lblSearch.Size = new System.Drawing.Size(41, 20);
            this._lblSearch.Text = "Suche:";
            //
            // _txtSearch
            //
            this._txtSearch.AutoSize = false;
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(160, 23);
            this._txtSearch.TextChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // _btnHideFixed
            //
            this._btnHideFixed.CheckOnClick = true;
            this._btnHideFixed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnHideFixed.Name = "_btnHideFixed";
            this._btnHideFixed.Size = new System.Drawing.Size(122, 20);
            this._btnHideFixed.Text = "Behobene ausblenden";
            this._btnHideFixed.CheckedChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // toolStripSeparator3
            //
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            //
            // _btnFixAll
            //
            this._btnFixAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnFixAll.Name = "_btnFixAll";
            this._btnFixAll.Size = new System.Drawing.Size(145, 20);
            this._btnFixAll.Text = "Alle behebbaren beheben…";
            this._btnFixAll.Click += new System.EventHandler(this.OnClickFixAll);
            //
            // _btnSaveRadar
            //
            this._btnSaveRadar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSaveRadar.Enabled = false;
            this._btnSaveRadar.Name = "_btnSaveRadar";
            this._btnSaveRadar.Size = new System.Drawing.Size(132, 20);
            this._btnSaveRadar.Text = "radarcol.mul speichern";
            this._btnSaveRadar.Click += new System.EventHandler(this.OnClickSaveRadar);
            //
            // toolStripSeparator4
            //
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            //
            // _ddExport
            //
            this._ddExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._ddExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._exportHtml,
            this._exportCsv,
            this._exportTxt});
            this._ddExport.Enabled = false;
            this._ddExport.Name = "_ddExport";
            this._ddExport.Size = new System.Drawing.Size(122, 20);
            this._ddExport.Text = "Bericht exportieren";
            //
            // _exportHtml
            //
            this._exportHtml.Name = "_exportHtml";
            this._exportHtml.Size = new System.Drawing.Size(180, 22);
            this._exportHtml.Text = "HTML (Webseite)…";
            this._exportHtml.Click += new System.EventHandler(this.OnClickExportHtml);
            //
            // _exportCsv
            //
            this._exportCsv.Name = "_exportCsv";
            this._exportCsv.Size = new System.Drawing.Size(180, 22);
            this._exportCsv.Text = "CSV (Tabelle)…";
            this._exportCsv.Click += new System.EventHandler(this.OnClickExportCsv);
            //
            // _exportTxt
            //
            this._exportTxt.Name = "_exportTxt";
            this._exportTxt.Size = new System.Drawing.Size(180, 22);
            this._exportTxt.Text = "Text…";
            this._exportTxt.Click += new System.EventHandler(this.OnClickExportTxt);
            //
            // _summary
            //
            this._summary.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(248)))));
            this._summary.Dock = System.Windows.Forms.DockStyle.Top;
            this._summary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._summary.Location = new System.Drawing.Point(0, 54);
            this._summary.Name = "_summary";
            this._summary.Padding = new System.Windows.Forms.Padding(8, 5, 0, 0);
            this._summary.Size = new System.Drawing.Size(1000, 26);
            this._summary.TabIndex = 2;
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 80);
            this.splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this._list);
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.detailsPanel);
            this.splitContainer.Size = new System.Drawing.Size(1000, 498);
            this.splitContainer.SplitterDistance = 580;
            this.splitContainer.SplitterWidth = 5;
            this.splitContainer.TabIndex = 3;
            //
            // _list
            //
            this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnSeverity,
            this.columnCategory,
            this.columnId,
            this.columnMessage});
            this._list.Dock = System.Windows.Forms.DockStyle.Fill;
            this._list.FullRowSelect = true;
            this._list.HideSelection = false;
            this._list.Location = new System.Drawing.Point(0, 0);
            this._list.Name = "_list";
            this._list.Size = new System.Drawing.Size(580, 498);
            this._list.TabIndex = 0;
            this._list.UseCompatibleStateImageBehavior = false;
            this._list.View = System.Windows.Forms.View.Details;
            this._list.VirtualMode = true;
            this._list.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnColumnClick);
            this._list.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.OnRetrieveItem);
            this._list.SelectedIndexChanged += new System.EventHandler(this.OnListSelectionChanged);
            this._list.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnListKeyDown);
            //
            // columnSeverity
            //
            this.columnSeverity.Text = "Stufe";
            this.columnSeverity.Width = 90;
            //
            // columnCategory
            //
            this.columnCategory.Text = "Bereich";
            this.columnCategory.Width = 170;
            //
            // columnId
            //
            this.columnId.Text = "ID";
            this.columnId.Width = 90;
            //
            // columnMessage
            //
            this.columnMessage.Text = "Beschreibung";
            this.columnMessage.Width = 700;
            //
            // detailsPanel
            //
            this.detailsPanel.AutoScroll = true;
            this.detailsPanel.BackColor = System.Drawing.Color.White;
            this.detailsPanel.Controls.Add(this._preview);
            this.detailsPanel.Controls.Add(this.buttonsPanel);
            this.detailsPanel.Controls.Add(this._detailFix);
            this.detailsPanel.Controls.Add(this._detailHint);
            this.detailsPanel.Controls.Add(this._detailHintTitle);
            this.detailsPanel.Controls.Add(this._detailMessage);
            this.detailsPanel.Controls.Add(this._detailTitle);
            this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsPanel.Location = new System.Drawing.Point(0, 0);
            this.detailsPanel.Name = "detailsPanel";
            this.detailsPanel.Padding = new System.Windows.Forms.Padding(10);
            this.detailsPanel.Size = new System.Drawing.Size(415, 498);
            this.detailsPanel.TabIndex = 0;
            this.detailsPanel.Resize += new System.EventHandler(this.OnDetailsResize);
            //
            // _preview
            //
            this._preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(44)))));
            this._preview.Dock = System.Windows.Forms.DockStyle.Top;
            this._preview.Location = new System.Drawing.Point(10, 210);
            this._preview.Name = "_preview";
            this._preview.Size = new System.Drawing.Size(395, 220);
            this._preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._preview.TabIndex = 6;
            this._preview.TabStop = false;
            //
            // buttonsPanel
            //
            this.buttonsPanel.AutoSize = true;
            this.buttonsPanel.Controls.Add(this._btnFix);
            this.buttonsPanel.Controls.Add(this._btnCopyId);
            this.buttonsPanel.Controls.Add(this._btnCopyLine);
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonsPanel.Location = new System.Drawing.Point(10, 179);
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Size = new System.Drawing.Size(395, 31);
            this.buttonsPanel.TabIndex = 5;
            //
            // _btnFix
            //
            this._btnFix.AutoSize = true;
            this._btnFix.Location = new System.Drawing.Point(3, 3);
            this._btnFix.Name = "_btnFix";
            this._btnFix.Size = new System.Drawing.Size(75, 25);
            this._btnFix.TabIndex = 0;
            this._btnFix.Text = "Beheben";
            this._btnFix.UseVisualStyleBackColor = true;
            this._btnFix.Click += new System.EventHandler(this.OnClickFix);
            //
            // _btnCopyId
            //
            this._btnCopyId.AutoSize = true;
            this._btnCopyId.Location = new System.Drawing.Point(84, 3);
            this._btnCopyId.Name = "_btnCopyId";
            this._btnCopyId.Size = new System.Drawing.Size(80, 25);
            this._btnCopyId.TabIndex = 1;
            this._btnCopyId.Text = "ID kopieren";
            this._btnCopyId.UseVisualStyleBackColor = true;
            this._btnCopyId.Click += new System.EventHandler(this.OnClickCopyId);
            //
            // _btnCopyLine
            //
            this._btnCopyLine.AutoSize = true;
            this._btnCopyLine.Location = new System.Drawing.Point(170, 3);
            this._btnCopyLine.Name = "_btnCopyLine";
            this._btnCopyLine.Size = new System.Drawing.Size(95, 25);
            this._btnCopyLine.TabIndex = 2;
            this._btnCopyLine.Text = "Zeile kopieren";
            this._btnCopyLine.UseVisualStyleBackColor = true;
            this._btnCopyLine.Click += new System.EventHandler(this.OnClickCopyLine);
            //
            // _detailFix
            //
            this._detailFix.AutoSize = true;
            this._detailFix.Dock = System.Windows.Forms.DockStyle.Top;
            this._detailFix.ForeColor = System.Drawing.Color.DarkGreen;
            this._detailFix.Location = new System.Drawing.Point(10, 156);
            this._detailFix.MaximumSize = new System.Drawing.Size(380, 0);
            this._detailFix.Name = "_detailFix";
            this._detailFix.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this._detailFix.Size = new System.Drawing.Size(0, 23);
            this._detailFix.TabIndex = 4;
            //
            // _detailHint
            //
            this._detailHint.AutoSize = true;
            this._detailHint.Dock = System.Windows.Forms.DockStyle.Top;
            this._detailHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(70)))), ((int)(((byte)(120)))));
            this._detailHint.Location = new System.Drawing.Point(10, 131);
            this._detailHint.MaximumSize = new System.Drawing.Size(380, 0);
            this._detailHint.Name = "_detailHint";
            this._detailHint.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this._detailHint.Size = new System.Drawing.Size(0, 25);
            this._detailHint.TabIndex = 3;
            //
            // _detailHintTitle
            //
            this._detailHintTitle.AutoSize = true;
            this._detailHintTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._detailHintTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._detailHintTitle.Location = new System.Drawing.Point(10, 110);
            this._detailHintTitle.Name = "_detailHintTitle";
            this._detailHintTitle.Padding = new System.Windows.Forms.Padding(0, 4, 0, 2);
            this._detailHintTitle.Size = new System.Drawing.Size(79, 21);
            this._detailHintTitle.TabIndex = 2;
            this._detailHintTitle.Text = "So beheben:";
            //
            // _detailMessage
            //
            this._detailMessage.AutoSize = true;
            this._detailMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this._detailMessage.Location = new System.Drawing.Point(10, 38);
            this._detailMessage.MaximumSize = new System.Drawing.Size(380, 0);
            this._detailMessage.Name = "_detailMessage";
            this._detailMessage.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this._detailMessage.Size = new System.Drawing.Size(0, 25);
            this._detailMessage.TabIndex = 1;
            //
            // _detailTitle
            //
            this._detailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this._detailTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this._detailTitle.Location = new System.Drawing.Point(10, 10);
            this._detailTitle.Name = "_detailTitle";
            this._detailTitle.Size = new System.Drawing.Size(395, 28);
            this._detailTitle.TabIndex = 0;
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._progress,
            this._status,
            this._shown});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 4;
            //
            // _progress
            //
            this._progress.Name = "_progress";
            this._progress.Size = new System.Drawing.Size(180, 16);
            this._progress.Visible = false;
            //
            // _status
            //
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(985, 17);
            this._status.Spring = true;
            this._status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _shown
            //
            this._shown.ForeColor = System.Drawing.Color.DimGray;
            this._shown.Name = "_shown";
            this._shown.Size = new System.Drawing.Size(0, 17);
            //
            // ClientCheckControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this._summary);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStripFilter);
            this.Controls.Add(this.toolStripMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ClientCheckControl";
            this.Size = new System.Drawing.Size(1000, 600);
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.toolStripFilter.ResumeLayout(false);
            this.toolStripFilter.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.detailsPanel.ResumeLayout(false);
            this.detailsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._preview)).EndInit();
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton _btnStart;
        private System.Windows.Forms.ToolStripButton _btnCancel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton _ddChecks;
        private System.Windows.Forms.ToolStripMenuItem checkArtTileDataMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkLandTilesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkEquipmentMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkBodiesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkRadarColMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkMultisMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkStaticsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkClilocMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _checkAll;
        private System.Windows.Forms.ToolStripMenuItem _checkNone;
        private System.Windows.Forms.ToolStripButton _btnHelp;
        private System.Windows.Forms.ToolStripComboBox _cmbLang;
        private System.Windows.Forms.ToolStripLabel _lblLang;
        private System.Windows.Forms.ToolStrip toolStripFilter;
        private System.Windows.Forms.ToolStripLabel _lblShow;
        private System.Windows.Forms.ToolStripComboBox _cmbSeverity;
        private System.Windows.Forms.ToolStripComboBox _cmbCategory;
        private System.Windows.Forms.ToolStripLabel _lblSearch;
        private System.Windows.Forms.ToolStripTextBox _txtSearch;
        private System.Windows.Forms.ToolStripButton _btnHideFixed;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton _btnFixAll;
        private System.Windows.Forms.ToolStripButton _btnSaveRadar;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton _ddExport;
        private System.Windows.Forms.ToolStripMenuItem _exportHtml;
        private System.Windows.Forms.ToolStripMenuItem _exportCsv;
        private System.Windows.Forms.ToolStripMenuItem _exportTxt;
        private System.Windows.Forms.Label _summary;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnSeverity;
        private System.Windows.Forms.ColumnHeader columnCategory;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnMessage;
        private System.Windows.Forms.Panel detailsPanel;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.FlowLayoutPanel buttonsPanel;
        private System.Windows.Forms.Button _btnFix;
        private System.Windows.Forms.Button _btnCopyId;
        private System.Windows.Forms.Button _btnCopyLine;
        private System.Windows.Forms.Label _detailFix;
        private System.Windows.Forms.Label _detailHint;
        private System.Windows.Forms.Label _detailHintTitle;
        private System.Windows.Forms.Label _detailMessage;
        private System.Windows.Forms.Label _detailTitle;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar _progress;
        private System.Windows.Forms.ToolStripStatusLabel _status;
        private System.Windows.Forms.ToolStripStatusLabel _shown;
    }
}

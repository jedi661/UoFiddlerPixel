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
            toolStripMain = new System.Windows.Forms.ToolStrip();
            _btnStart = new System.Windows.Forms.ToolStripButton();
            _btnCancel = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _ddChecks = new System.Windows.Forms.ToolStripDropDownButton();
            checkArtTileDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkLandTilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkEquipmentMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkBodiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkRadarColMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkMultisMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkStaticsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkClilocMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            _checkAll = new System.Windows.Forms.ToolStripMenuItem();
            _checkNone = new System.Windows.Forms.ToolStripMenuItem();
            _btnHelp = new System.Windows.Forms.ToolStripButton();
            _cmbLang = new System.Windows.Forms.ToolStripComboBox();
            _lblLang = new System.Windows.Forms.ToolStripLabel();
            toolStripFilter = new System.Windows.Forms.ToolStrip();
            _lblShow = new System.Windows.Forms.ToolStripLabel();
            _cmbSeverity = new System.Windows.Forms.ToolStripComboBox();
            _cmbCategory = new System.Windows.Forms.ToolStripComboBox();
            _lblSearch = new System.Windows.Forms.ToolStripLabel();
            _txtSearch = new System.Windows.Forms.ToolStripTextBox();
            _btnHideFixed = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            _btnFixAll = new System.Windows.Forms.ToolStripButton();
            _btnSaveRadar = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            _ddExport = new System.Windows.Forms.ToolStripDropDownButton();
            _exportHtml = new System.Windows.Forms.ToolStripMenuItem();
            _exportCsv = new System.Windows.Forms.ToolStripMenuItem();
            _exportTxt = new System.Windows.Forms.ToolStripMenuItem();
            _summary = new System.Windows.Forms.Label();
            splitContainer = new System.Windows.Forms.SplitContainer();
            _list = new System.Windows.Forms.ListView();
            columnSeverity = new System.Windows.Forms.ColumnHeader();
            columnCategory = new System.Windows.Forms.ColumnHeader();
            columnId = new System.Windows.Forms.ColumnHeader();
            columnMessage = new System.Windows.Forms.ColumnHeader();
            detailsPanel = new System.Windows.Forms.Panel();
            _preview = new System.Windows.Forms.PictureBox();
            buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            _btnFix = new System.Windows.Forms.Button();
            _btnCopyId = new System.Windows.Forms.Button();
            _btnCopyLine = new System.Windows.Forms.Button();
            _detailFix = new System.Windows.Forms.Label();
            _detailHint = new System.Windows.Forms.Label();
            _detailHintTitle = new System.Windows.Forms.Label();
            _detailMessage = new System.Windows.Forms.Label();
            _detailTitle = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            _progress = new System.Windows.Forms.ToolStripProgressBar();
            _status = new System.Windows.Forms.ToolStripStatusLabel();
            _shown = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripMain.SuspendLayout();
            toolStripFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            detailsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            buttonsPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripMain
            // 
            toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _btnStart, _btnCancel, toolStripSeparator1, _ddChecks, _btnHelp, _cmbLang, _lblLang });
            toolStripMain.Location = new System.Drawing.Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStripMain.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripMain.Size = new System.Drawing.Size(1000, 27);
            toolStripMain.TabIndex = 0;
            // 
            // _btnStart
            // 
            _btnStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _btnStart.Name = "_btnStart";
            _btnStart.Size = new System.Drawing.Size(113, 20);
            _btnStart.Text = "▶ Prüfung starten";
            _btnStart.Click += OnClickStart;
            // 
            // _btnCancel
            // 
            _btnCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnCancel.Enabled = false;
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new System.Drawing.Size(82, 20);
            _btnCancel.Text = "■ Abbrechen";
            _btnCancel.Click += OnClickCancel;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // _ddChecks
            // 
            _ddChecks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _ddChecks.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { checkArtTileDataMenuItem, checkLandTilesMenuItem, checkEquipmentMenuItem, checkBodiesMenuItem, checkRadarColMenuItem, checkMultisMenuItem, checkStaticsMenuItem, checkClilocMenuItem, toolStripSeparator2, _checkAll, _checkNone });
            _ddChecks.Name = "_ddChecks";
            _ddChecks.Size = new System.Drawing.Size(76, 20);
            _ddChecks.Text = "Prüfungen";
            // 
            // checkArtTileDataMenuItem
            // 
            checkArtTileDataMenuItem.Checked = true;
            checkArtTileDataMenuItem.CheckOnClick = true;
            checkArtTileDataMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkArtTileDataMenuItem.Name = "checkArtTileDataMenuItem";
            checkArtTileDataMenuItem.Size = new System.Drawing.Size(196, 22);
            checkArtTileDataMenuItem.Text = "Art ↔ tiledata";
            checkArtTileDataMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkLandTilesMenuItem
            // 
            checkLandTilesMenuItem.Checked = true;
            checkLandTilesMenuItem.CheckOnClick = true;
            checkLandTilesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkLandTilesMenuItem.Name = "checkLandTilesMenuItem";
            checkLandTilesMenuItem.Size = new System.Drawing.Size(196, 22);
            checkLandTilesMenuItem.Text = "Land-Tiles";
            checkLandTilesMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkEquipmentMenuItem
            // 
            checkEquipmentMenuItem.Checked = true;
            checkEquipmentMenuItem.CheckOnClick = true;
            checkEquipmentMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkEquipmentMenuItem.Name = "checkEquipmentMenuItem";
            checkEquipmentMenuItem.Size = new System.Drawing.Size(196, 22);
            checkEquipmentMenuItem.Text = "Ausrüstung";
            checkEquipmentMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkBodiesMenuItem
            // 
            checkBodiesMenuItem.Checked = true;
            checkBodiesMenuItem.CheckOnClick = true;
            checkBodiesMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBodiesMenuItem.Name = "checkBodiesMenuItem";
            checkBodiesMenuItem.Size = new System.Drawing.Size(196, 22);
            checkBodiesMenuItem.Text = "Bodies / Animationen";
            checkBodiesMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkRadarColMenuItem
            // 
            checkRadarColMenuItem.Checked = true;
            checkRadarColMenuItem.CheckOnClick = true;
            checkRadarColMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkRadarColMenuItem.Name = "checkRadarColMenuItem";
            checkRadarColMenuItem.Size = new System.Drawing.Size(196, 22);
            checkRadarColMenuItem.Text = "radarcol (Kartenfarben)";
            checkRadarColMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkMultisMenuItem
            // 
            checkMultisMenuItem.Checked = true;
            checkMultisMenuItem.CheckOnClick = true;
            checkMultisMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkMultisMenuItem.Name = "checkMultisMenuItem";
            checkMultisMenuItem.Size = new System.Drawing.Size(196, 22);
            checkMultisMenuItem.Text = "Multis";
            checkMultisMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkStaticsMenuItem
            // 
            checkStaticsMenuItem.Checked = true;
            checkStaticsMenuItem.CheckOnClick = true;
            checkStaticsMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkStaticsMenuItem.Name = "checkStaticsMenuItem";
            checkStaticsMenuItem.Size = new System.Drawing.Size(196, 22);
            checkStaticsMenuItem.Text = "Statics auf Karten";
            checkStaticsMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // checkClilocMenuItem
            // 
            checkClilocMenuItem.Checked = true;
            checkClilocMenuItem.CheckOnClick = true;
            checkClilocMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            checkClilocMenuItem.Name = "checkClilocMenuItem";
            checkClilocMenuItem.Size = new System.Drawing.Size(196, 22);
            checkClilocMenuItem.Text = "Cliloc (Namen)";
            checkClilocMenuItem.CheckedChanged += OnCheckItemChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // _checkAll
            // 
            _checkAll.Name = "_checkAll";
            _checkAll.Size = new System.Drawing.Size(196, 22);
            _checkAll.Text = "Alle auswählen";
            _checkAll.Click += OnClickCheckAll;
            // 
            // _checkNone
            // 
            _checkNone.Name = "_checkNone";
            _checkNone.Size = new System.Drawing.Size(196, 22);
            _checkNone.Text = "Keine auswählen";
            _checkNone.Click += OnClickCheckNone;
            // 
            // _btnHelp
            // 
            _btnHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnHelp.Name = "_btnHelp";
            _btnHelp.Size = new System.Drawing.Size(44, 20);
            _btnHelp.Text = "? Hilfe";
            _btnHelp.Click += OnClickHelp;
            // 
            // _cmbLang
            // 
            _cmbLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _cmbLang.AutoSize = false;
            _cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbLang.Items.AddRange(new object[] { "Deutsch", "English", "Français" });
            _cmbLang.Name = "_cmbLang";
            _cmbLang.Size = new System.Drawing.Size(95, 23);
            _cmbLang.SelectedIndexChanged += OnLanguageSelected;
            // 
            // _lblLang
            // 
            _lblLang.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _lblLang.Name = "_lblLang";
            _lblLang.Size = new System.Drawing.Size(52, 20);
            _lblLang.Text = "Sprache:";
            // 
            // toolStripFilter
            // 
            toolStripFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _lblShow, _cmbSeverity, _cmbCategory, _lblSearch, _txtSearch, _btnHideFixed, toolStripSeparator3, _btnFixAll, _btnSaveRadar, toolStripSeparator4, _ddExport });
            toolStripFilter.Location = new System.Drawing.Point(0, 27);
            toolStripFilter.Name = "toolStripFilter";
            toolStripFilter.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            toolStripFilter.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripFilter.Size = new System.Drawing.Size(1000, 27);
            toolStripFilter.TabIndex = 1;
            // 
            // _lblShow
            // 
            _lblShow.Name = "_lblShow";
            _lblShow.Size = new System.Drawing.Size(59, 20);
            _lblShow.Text = "Anzeigen:";
            // 
            // _cmbSeverity
            // 
            _cmbSeverity.AutoSize = false;
            _cmbSeverity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbSeverity.Name = "_cmbSeverity";
            _cmbSeverity.Size = new System.Drawing.Size(120, 23);
            _cmbSeverity.SelectedIndexChanged += OnFilterChanged;
            // 
            // _cmbCategory
            // 
            _cmbCategory.AutoSize = false;
            _cmbCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbCategory.Name = "_cmbCategory";
            _cmbCategory.Size = new System.Drawing.Size(170, 23);
            _cmbCategory.SelectedIndexChanged += OnFilterChanged;
            // 
            // _lblSearch
            // 
            _lblSearch.Name = "_lblSearch";
            _lblSearch.Size = new System.Drawing.Size(42, 20);
            _lblSearch.Text = "Suche:";
            // 
            // _txtSearch
            // 
            _txtSearch.AutoSize = false;
            _txtSearch.Name = "_txtSearch";
            _txtSearch.Size = new System.Drawing.Size(160, 23);
            _txtSearch.TextChanged += OnFilterChanged;
            // 
            // _btnHideFixed
            // 
            _btnHideFixed.CheckOnClick = true;
            _btnHideFixed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnHideFixed.Name = "_btnHideFixed";
            _btnHideFixed.Size = new System.Drawing.Size(128, 20);
            _btnHideFixed.Text = "Behobene ausblenden";
            _btnHideFixed.CheckedChanged += OnFilterChanged;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 23);
            // 
            // _btnFixAll
            // 
            _btnFixAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnFixAll.Name = "_btnFixAll";
            _btnFixAll.Size = new System.Drawing.Size(155, 20);
            _btnFixAll.Text = "Alle behebbaren beheben…";
            _btnFixAll.Click += OnClickFixAll;
            // 
            // _btnSaveRadar
            // 
            _btnSaveRadar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _btnSaveRadar.Enabled = false;
            _btnSaveRadar.Name = "_btnSaveRadar";
            _btnSaveRadar.Size = new System.Drawing.Size(132, 19);
            _btnSaveRadar.Text = "radarcol.mul speichern";
            _btnSaveRadar.Click += OnClickSaveRadar;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 23);
            // 
            // _ddExport
            // 
            _ddExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _ddExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { _exportHtml, _exportCsv, _exportTxt });
            _ddExport.Enabled = false;
            _ddExport.Name = "_ddExport";
            _ddExport.Size = new System.Drawing.Size(120, 19);
            _ddExport.Text = "Bericht exportieren";
            // 
            // _exportHtml
            // 
            _exportHtml.Name = "_exportHtml";
            _exportHtml.Size = new System.Drawing.Size(174, 22);
            _exportHtml.Text = "HTML (Webseite)…";
            _exportHtml.Click += OnClickExportHtml;
            // 
            // _exportCsv
            // 
            _exportCsv.Name = "_exportCsv";
            _exportCsv.Size = new System.Drawing.Size(174, 22);
            _exportCsv.Text = "CSV (Tabelle)…";
            _exportCsv.Click += OnClickExportCsv;
            // 
            // _exportTxt
            // 
            _exportTxt.Name = "_exportTxt";
            _exportTxt.Size = new System.Drawing.Size(174, 22);
            _exportTxt.Text = "Text…";
            _exportTxt.Click += OnClickExportTxt;
            // 
            // _summary
            // 
            _summary.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);
            _summary.Dock = System.Windows.Forms.DockStyle.Top;
            _summary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _summary.Location = new System.Drawing.Point(0, 54);
            _summary.Name = "_summary";
            _summary.Padding = new System.Windows.Forms.Padding(8, 5, 0, 0);
            _summary.Size = new System.Drawing.Size(1000, 26);
            _summary.TabIndex = 2;
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 80);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(_list);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(detailsPanel);
            splitContainer.Size = new System.Drawing.Size(1000, 498);
            splitContainer.SplitterDistance = 580;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 3;
            // 
            // _list
            // 
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnSeverity, columnCategory, columnId, columnMessage });
            _list.Dock = System.Windows.Forms.DockStyle.Fill;
            _list.FullRowSelect = true;
            _list.Location = new System.Drawing.Point(0, 0);
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(580, 498);
            _list.TabIndex = 0;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.VirtualMode = true;
            _list.ColumnClick += OnColumnClick;
            _list.RetrieveVirtualItem += OnRetrieveItem;
            _list.SelectedIndexChanged += OnListSelectionChanged;
            _list.KeyDown += OnListKeyDown;
            // 
            // columnSeverity
            // 
            columnSeverity.Text = "Stufe";
            columnSeverity.Width = 90;
            // 
            // columnCategory
            // 
            columnCategory.Text = "Bereich";
            columnCategory.Width = 170;
            // 
            // columnId
            // 
            columnId.Text = "ID";
            columnId.Width = 90;
            // 
            // columnMessage
            // 
            columnMessage.Text = "Beschreibung";
            columnMessage.Width = 700;
            // 
            // detailsPanel
            // 
            detailsPanel.AutoScroll = true;
            detailsPanel.BackColor = System.Drawing.Color.White;
            detailsPanel.Controls.Add(_preview);
            detailsPanel.Controls.Add(buttonsPanel);
            detailsPanel.Controls.Add(_detailFix);
            detailsPanel.Controls.Add(_detailHint);
            detailsPanel.Controls.Add(_detailHintTitle);
            detailsPanel.Controls.Add(_detailMessage);
            detailsPanel.Controls.Add(_detailTitle);
            detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            detailsPanel.Location = new System.Drawing.Point(0, 0);
            detailsPanel.Name = "detailsPanel";
            detailsPanel.Padding = new System.Windows.Forms.Padding(10);
            detailsPanel.Size = new System.Drawing.Size(415, 498);
            detailsPanel.TabIndex = 0;
            detailsPanel.Resize += OnDetailsResize;
            // 
            // _preview
            // 
            _preview.BackColor = System.Drawing.Color.FromArgb(40, 40, 44);
            _preview.Dock = System.Windows.Forms.DockStyle.Top;
            _preview.Location = new System.Drawing.Point(10, 163);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(395, 220);
            _preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            _preview.TabIndex = 6;
            _preview.TabStop = false;
            // 
            // buttonsPanel
            // 
            buttonsPanel.AutoSize = true;
            buttonsPanel.Controls.Add(_btnFix);
            buttonsPanel.Controls.Add(_btnCopyId);
            buttonsPanel.Controls.Add(_btnCopyLine);
            buttonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            buttonsPanel.Location = new System.Drawing.Point(10, 132);
            buttonsPanel.Name = "buttonsPanel";
            buttonsPanel.Size = new System.Drawing.Size(395, 31);
            buttonsPanel.TabIndex = 5;
            // 
            // _btnFix
            // 
            _btnFix.AutoSize = true;
            _btnFix.Location = new System.Drawing.Point(3, 3);
            _btnFix.Name = "_btnFix";
            _btnFix.Size = new System.Drawing.Size(75, 25);
            _btnFix.TabIndex = 0;
            _btnFix.Text = "Beheben";
            _btnFix.UseVisualStyleBackColor = true;
            _btnFix.Click += OnClickFix;
            // 
            // _btnCopyId
            // 
            _btnCopyId.AutoSize = true;
            _btnCopyId.Location = new System.Drawing.Point(84, 3);
            _btnCopyId.Name = "_btnCopyId";
            _btnCopyId.Size = new System.Drawing.Size(80, 25);
            _btnCopyId.TabIndex = 1;
            _btnCopyId.Text = "ID kopieren";
            _btnCopyId.UseVisualStyleBackColor = true;
            _btnCopyId.Click += OnClickCopyId;
            // 
            // _btnCopyLine
            // 
            _btnCopyLine.AutoSize = true;
            _btnCopyLine.Location = new System.Drawing.Point(170, 3);
            _btnCopyLine.Name = "_btnCopyLine";
            _btnCopyLine.Size = new System.Drawing.Size(95, 25);
            _btnCopyLine.TabIndex = 2;
            _btnCopyLine.Text = "Zeile kopieren";
            _btnCopyLine.UseVisualStyleBackColor = true;
            _btnCopyLine.Click += OnClickCopyLine;
            // 
            // _detailFix
            // 
            _detailFix.AutoSize = true;
            _detailFix.Dock = System.Windows.Forms.DockStyle.Top;
            _detailFix.ForeColor = System.Drawing.Color.DarkGreen;
            _detailFix.Location = new System.Drawing.Point(10, 109);
            _detailFix.MaximumSize = new System.Drawing.Size(380, 0);
            _detailFix.Name = "_detailFix";
            _detailFix.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            _detailFix.Size = new System.Drawing.Size(0, 23);
            _detailFix.TabIndex = 4;
            // 
            // _detailHint
            // 
            _detailHint.AutoSize = true;
            _detailHint.Dock = System.Windows.Forms.DockStyle.Top;
            _detailHint.ForeColor = System.Drawing.Color.FromArgb(40, 70, 120);
            _detailHint.Location = new System.Drawing.Point(10, 84);
            _detailHint.MaximumSize = new System.Drawing.Size(380, 0);
            _detailHint.Name = "_detailHint";
            _detailHint.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            _detailHint.Size = new System.Drawing.Size(0, 25);
            _detailHint.TabIndex = 3;
            // 
            // _detailHintTitle
            // 
            _detailHintTitle.AutoSize = true;
            _detailHintTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _detailHintTitle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _detailHintTitle.Location = new System.Drawing.Point(10, 63);
            _detailHintTitle.Name = "_detailHintTitle";
            _detailHintTitle.Padding = new System.Windows.Forms.Padding(0, 4, 0, 2);
            _detailHintTitle.Size = new System.Drawing.Size(76, 21);
            _detailHintTitle.TabIndex = 2;
            _detailHintTitle.Text = "So beheben:";
            // 
            // _detailMessage
            // 
            _detailMessage.AutoSize = true;
            _detailMessage.Dock = System.Windows.Forms.DockStyle.Top;
            _detailMessage.Location = new System.Drawing.Point(10, 38);
            _detailMessage.MaximumSize = new System.Drawing.Size(380, 0);
            _detailMessage.Name = "_detailMessage";
            _detailMessage.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            _detailMessage.Size = new System.Drawing.Size(0, 25);
            _detailMessage.TabIndex = 1;
            // 
            // _detailTitle
            // 
            _detailTitle.Dock = System.Windows.Forms.DockStyle.Top;
            _detailTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            _detailTitle.Location = new System.Drawing.Point(10, 10);
            _detailTitle.Name = "_detailTitle";
            _detailTitle.Size = new System.Drawing.Size(395, 28);
            _detailTitle.TabIndex = 0;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _progress, _status, _shown });
            statusStrip.Location = new System.Drawing.Point(0, 578);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1000, 22);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 4;
            // 
            // _progress
            // 
            _progress.Name = "_progress";
            _progress.Size = new System.Drawing.Size(180, 16);
            _progress.Visible = false;
            // 
            // _status
            // 
            _status.Name = "_status";
            _status.Size = new System.Drawing.Size(985, 17);
            _status.Spring = true;
            _status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _shown
            // 
            _shown.ForeColor = System.Drawing.Color.DimGray;
            _shown.Name = "_shown";
            _shown.Size = new System.Drawing.Size(0, 17);
            // 
            // ClientCheckControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(_summary);
            Controls.Add(statusStrip);
            Controls.Add(toolStripFilter);
            Controls.Add(toolStripMain);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            Name = "ClientCheckControl";
            Size = new System.Drawing.Size(1000, 600);
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            toolStripFilter.ResumeLayout(false);
            toolStripFilter.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            detailsPanel.ResumeLayout(false);
            detailsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            buttonsPanel.ResumeLayout(false);
            buttonsPanel.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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

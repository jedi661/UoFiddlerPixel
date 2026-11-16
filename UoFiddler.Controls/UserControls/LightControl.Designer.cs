namespace UoFiddler.Controls.UserControls
{
    partial class LightControl
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeViewLights = new System.Windows.Forms.TreeView();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this._lblInfo = new System.Windows.Forms.Label();
            this._pnlTest = new System.Windows.Forms.TableLayoutPanel();
            this._lblLand = new System.Windows.Forms.Label();
            this._picLand = new System.Windows.Forms.PictureBox();
            this._txtLand = new System.Windows.Forms.TextBox();
            this._btnLandPrev = new System.Windows.Forms.Button();
            this._btnLandNext = new System.Windows.Forms.Button();
            this._btnLandPick = new System.Windows.Forms.Button();
            this._lblItem = new System.Windows.Forms.Label();
            this._picItem = new System.Windows.Forms.PictureBox();
            this._txtItem = new System.Windows.Forms.TextBox();
            this._btnItemPrev = new System.Windows.Forms.Button();
            this._btnItemNext = new System.Windows.Forms.Button();
            this._btnItemPick = new System.Windows.Forms.Button();
            this._chkIgPreview = new System.Windows.Forms.CheckBox();
            this._chkOnlyLightItems = new System.Windows.Forms.CheckBox();
            this._pnlOptions = new System.Windows.Forms.FlowLayoutPanel();
            this._lblDark = new System.Windows.Forms.Label();
            this._numDark = new System.Windows.Forms.NumericUpDown();
            this._lblLightX = new System.Windows.Forms.Label();
            this._numLightX = new System.Windows.Forms.NumericUpDown();
            this._lblLightY = new System.Windows.Forms.Label();
            this._numLightY = new System.Windows.Forms.NumericUpDown();
            this._lblZoom = new System.Windows.Forms.Label();
            this._cmbZoom = new System.Windows.Forms.ComboBox();
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this._lblSearch = new System.Windows.Forms.ToolStripLabel();
            this._txtSearch = new System.Windows.Forms.ToolStripTextBox();
            this._sep1 = new System.Windows.Forms.ToolStripSeparator();
            this._btnOverview = new System.Windows.Forms.ToolStripButton();
            this._btnNew = new System.Windows.Forms.ToolStripButton();
            this._btnEdit = new System.Windows.Forms.ToolStripButton();
            this._sep2 = new System.Windows.Forms.ToolStripSeparator();
            this._btnSave = new System.Windows.Forms.ToolStripButton();
            this._sep3 = new System.Windows.Forms.ToolStripSeparator();
            this._lblEntries = new System.Windows.Forms.ToolStripLabel();
            this._txtCount = new System.Windows.Forms.ToolStripTextBox();
            this._btnSetCount = new System.Windows.Forms.ToolStripButton();
            this._lblCount = new System.Windows.Forms.ToolStripLabel();
            this.treeViewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBoxInsertImport = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.iGPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundLandTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lightTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.copyPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this._pnlTest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._picLand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._picItem)).BeginInit();
            this._pnlOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDark)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLightX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLightY)).BeginInit();
            this.toolStripMain.SuspendLayout();
            this.treeViewContextMenuStrip.SuspendLayout();
            this.previewContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeViewLights);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.pictureBoxPreview);
            this.splitContainer.Panel2.Controls.Add(this._lblInfo);
            this.splitContainer.Panel2.Controls.Add(this._pnlTest);
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Location = new System.Drawing.Point(0, 25);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Size = new System.Drawing.Size(900, 535);
            this.splitContainer.SplitterDistance = 230;
            this.splitContainer.TabIndex = 0;
            // 
            // treeViewLights
            // 
            this.treeViewLights.ContextMenuStrip = this.treeViewContextMenuStrip;
            this.treeViewLights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLights.HideSelection = false;
            this.treeViewLights.LabelEdit = true;
            this.treeViewLights.Name = "treeViewLights";
            this.treeViewLights.ShowLines = false;
            this.treeViewLights.ShowRootLines = false;
            this.treeViewLights.TabIndex = 1;
            this.treeViewLights.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelect);
            this.treeViewLights.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnBeforeLabelEdit);
            this.treeViewLights.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.OnAfterLabelEdit);
            this.treeViewLights.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnTreeKeyDown);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BackColor = System.Drawing.Color.Black;
            this.pictureBoxPreview.ContextMenuStrip = this.previewContextMenuStrip;
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxPreview.TabIndex = 2;
            this.pictureBoxPreview.TabStop = false;
            this.pictureBoxPreview.SizeChanged += new System.EventHandler(this.OnPictureSizeChanged);
            // 
            // _lblInfo
            // 
            this._lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._lblInfo.Name = "_lblInfo";
            this._lblInfo.Padding = new System.Windows.Forms.Padding(6, 4, 6, 4);
            this._lblInfo.Size = new System.Drawing.Size(666, 24);
            this._lblInfo.Text = "-";
            // 
            // _pnlTest
            // 
            this._pnlTest.Controls.Add(this._lblLand, 0, 0);
            this._pnlTest.Controls.Add(this._picLand, 1, 0);
            this._pnlTest.Controls.Add(this._txtLand, 2, 0);
            this._pnlTest.Controls.Add(this._btnLandPrev, 3, 0);
            this._pnlTest.Controls.Add(this._btnLandNext, 4, 0);
            this._pnlTest.Controls.Add(this._btnLandPick, 5, 0);
            this._pnlTest.Controls.Add(this._lblItem, 0, 1);
            this._pnlTest.Controls.Add(this._picItem, 1, 1);
            this._pnlTest.Controls.Add(this._txtItem, 2, 1);
            this._pnlTest.Controls.Add(this._btnItemPrev, 3, 1);
            this._pnlTest.Controls.Add(this._btnItemNext, 4, 1);
            this._pnlTest.Controls.Add(this._btnItemPick, 5, 1);
            this._pnlTest.Controls.Add(this._chkIgPreview, 6, 0);
            this._pnlTest.Controls.Add(this._chkOnlyLightItems, 6, 1);
            this._pnlTest.Controls.Add(this._pnlOptions, 0, 2);
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._pnlTest.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._pnlTest.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlTest.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlTest.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._pnlTest.AutoSize = true;
            this._pnlTest.ColumnCount = 8;
            this._pnlTest.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._pnlTest.Name = "_pnlTest";
            this._pnlTest.Padding = new System.Windows.Forms.Padding(4);
            this._pnlTest.RowCount = 3;
            this._pnlTest.TabIndex = 3;
            // 
            // _lblLand
            // 
            this._lblLand.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblLand.AutoSize = true;
            this._lblLand.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblLand.Name = "_lblLand";
            this._lblLand.Text = "Ground (land tile):";
            // 
            // _picLand
            // 
            this._picLand.BackColor = System.Drawing.Color.Black;
            this._picLand.Name = "_picLand";
            this._picLand.Size = new System.Drawing.Size(44, 44);
            this._picLand.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._picLand.TabIndex = 4;
            this._picLand.TabStop = false;
            // 
            // _txtLand
            // 
            this._txtLand.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._txtLand.Name = "_txtLand";
            this._txtLand.Size = new System.Drawing.Size(70, 23);
            this._txtLand.TabIndex = 5;
            this._txtLand.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnLandTextKeyDown);
            this._txtLand.Leave += new System.EventHandler(this.OnLandTextLeave);
            // 
            // _btnLandPrev
            // 
            this._btnLandPrev.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnLandPrev.AutoSize = true;
            this._btnLandPrev.Name = "_btnLandPrev";
            this._btnLandPrev.TabIndex = 6;
            this._btnLandPrev.Text = "◀";
            this._btnLandPrev.UseVisualStyleBackColor = true;
            this._btnLandPrev.Click += new System.EventHandler(this.OnClickLandPrev);
            // 
            // _btnLandNext
            // 
            this._btnLandNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnLandNext.AutoSize = true;
            this._btnLandNext.Name = "_btnLandNext";
            this._btnLandNext.TabIndex = 7;
            this._btnLandNext.Text = "▶";
            this._btnLandNext.UseVisualStyleBackColor = true;
            this._btnLandNext.Click += new System.EventHandler(this.OnClickLandNext);
            // 
            // _btnLandPick
            // 
            this._btnLandPick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnLandPick.AutoSize = true;
            this._btnLandPick.Name = "_btnLandPick";
            this._btnLandPick.TabIndex = 8;
            this._btnLandPick.Text = "▦ Overview…";
            this._btnLandPick.UseVisualStyleBackColor = true;
            this._btnLandPick.Click += new System.EventHandler(this.OnClickLandPick);
            // 
            // _lblItem
            // 
            this._lblItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblItem.AutoSize = true;
            this._lblItem.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblItem.Name = "_lblItem";
            this._lblItem.Text = "Light source (item):";
            // 
            // _picItem
            // 
            this._picItem.BackColor = System.Drawing.Color.Black;
            this._picItem.Name = "_picItem";
            this._picItem.Size = new System.Drawing.Size(44, 44);
            this._picItem.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._picItem.TabIndex = 9;
            this._picItem.TabStop = false;
            // 
            // _txtItem
            // 
            this._txtItem.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._txtItem.Name = "_txtItem";
            this._txtItem.Size = new System.Drawing.Size(70, 23);
            this._txtItem.TabIndex = 10;
            this._txtItem.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnItemTextKeyDown);
            this._txtItem.Leave += new System.EventHandler(this.OnItemTextLeave);
            // 
            // _btnItemPrev
            // 
            this._btnItemPrev.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnItemPrev.AutoSize = true;
            this._btnItemPrev.Name = "_btnItemPrev";
            this._btnItemPrev.TabIndex = 11;
            this._btnItemPrev.Text = "◀";
            this._btnItemPrev.UseVisualStyleBackColor = true;
            this._btnItemPrev.Click += new System.EventHandler(this.OnClickItemPrev);
            // 
            // _btnItemNext
            // 
            this._btnItemNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnItemNext.AutoSize = true;
            this._btnItemNext.Name = "_btnItemNext";
            this._btnItemNext.TabIndex = 12;
            this._btnItemNext.Text = "▶";
            this._btnItemNext.UseVisualStyleBackColor = true;
            this._btnItemNext.Click += new System.EventHandler(this.OnClickItemNext);
            // 
            // _btnItemPick
            // 
            this._btnItemPick.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._btnItemPick.AutoSize = true;
            this._btnItemPick.Name = "_btnItemPick";
            this._btnItemPick.TabIndex = 13;
            this._btnItemPick.Text = "▦ Overview…";
            this._btnItemPick.UseVisualStyleBackColor = true;
            this._btnItemPick.Click += new System.EventHandler(this.OnClickItemPick);
            // 
            // _chkIgPreview
            // 
            this._chkIgPreview.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._chkIgPreview.AutoSize = true;
            this._chkIgPreview.Name = "_chkIgPreview";
            this._chkIgPreview.TabIndex = 14;
            this._chkIgPreview.Text = "In-game preview";
            this._chkIgPreview.UseVisualStyleBackColor = true;
            this._chkIgPreview.CheckedChanged += new System.EventHandler(this.OnPreviewOptionChanged);
            // 
            // _chkOnlyLightItems
            // 
            this._chkOnlyLightItems.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._chkOnlyLightItems.AutoSize = true;
            this._chkOnlyLightItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkOnlyLightItems.Checked = true;
            this._chkOnlyLightItems.Name = "_chkOnlyLightItems";
            this._chkOnlyLightItems.TabIndex = 15;
            this._chkOnlyLightItems.Text = "◀ ▶ only light sources";
            this._chkOnlyLightItems.UseVisualStyleBackColor = true;
            // 
            // _pnlOptions
            // 
            this._pnlOptions.Controls.Add(this._lblDark);
            this._pnlOptions.Controls.Add(this._numDark);
            this._pnlOptions.Controls.Add(this._lblLightX);
            this._pnlOptions.Controls.Add(this._numLightX);
            this._pnlOptions.Controls.Add(this._lblLightY);
            this._pnlOptions.Controls.Add(this._numLightY);
            this._pnlOptions.Controls.Add(this._lblZoom);
            this._pnlOptions.Controls.Add(this._cmbZoom);
            this._pnlTest.SetColumnSpan(this._pnlOptions, 8);
            this._pnlOptions.AutoSize = true;
            this._pnlOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pnlOptions.Margin = new System.Windows.Forms.Padding(0);
            this._pnlOptions.Name = "_pnlOptions";
            this._pnlOptions.TabIndex = 16;
            // 
            // _lblDark
            // 
            this._lblDark.AutoSize = true;
            this._lblDark.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._lblDark.Name = "_lblDark";
            this._lblDark.Text = "Darkness %:";
            // 
            // _numDark
            // 
            this._numDark.Maximum = new decimal(new int[] { 100, 0, 0, 0});
            this._numDark.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numDark.Name = "_numDark";
            this._numDark.Size = new System.Drawing.Size(55, 23);
            this._numDark.TabIndex = 17;
            this._numDark.Value = new decimal(new int[] { 70, 0, 0, 0});
            this._numDark.ValueChanged += new System.EventHandler(this.OnPreviewOptionChanged);
            // 
            // _lblLightX
            // 
            this._lblLightX.AutoSize = true;
            this._lblLightX.Margin = new System.Windows.Forms.Padding(12, 7, 3, 3);
            this._lblLightX.Name = "_lblLightX";
            this._lblLightX.Text = "Light offset X:";
            // 
            // _numLightX
            // 
            this._numLightX.Maximum = new decimal(new int[] { 200, 0, 0, 0});
            this._numLightX.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648});
            this._numLightX.Name = "_numLightX";
            this._numLightX.Size = new System.Drawing.Size(55, 23);
            this._numLightX.TabIndex = 18;
            this._numLightX.ValueChanged += new System.EventHandler(this.OnPreviewOptionChanged);
            // 
            // _lblLightY
            // 
            this._lblLightY.AutoSize = true;
            this._lblLightY.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._lblLightY.Name = "_lblLightY";
            this._lblLightY.Text = "Y:";
            // 
            // _numLightY
            // 
            this._numLightY.Maximum = new decimal(new int[] { 200, 0, 0, 0});
            this._numLightY.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648});
            this._numLightY.Name = "_numLightY";
            this._numLightY.Size = new System.Drawing.Size(55, 23);
            this._numLightY.TabIndex = 19;
            this._numLightY.Value = new decimal(new int[] { 30, 0, 0, 0});
            this._numLightY.ValueChanged += new System.EventHandler(this.OnPreviewOptionChanged);
            // 
            // _lblZoom
            // 
            this._lblZoom.AutoSize = true;
            this._lblZoom.Margin = new System.Windows.Forms.Padding(12, 7, 3, 3);
            this._lblZoom.Name = "_lblZoom";
            this._lblZoom.Text = "Zoom:";
            // 
            // _cmbZoom
            // 
            this._cmbZoom.Items.AddRange(new object[] {
            "100%",
            "200%",
            "300%",
            "Fit"});
            this._cmbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbZoom.Name = "_cmbZoom";
            this._cmbZoom.Size = new System.Drawing.Size(70, 23);
            this._cmbZoom.TabIndex = 20;
            this._cmbZoom.SelectedIndexChanged += new System.EventHandler(this.OnPreviewOptionChanged);
            // 
            // toolStripMain
            // 
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._lblSearch,
            this._txtSearch,
            this._sep1,
            this._btnOverview,
            this._btnNew,
            this._btnEdit,
            this._sep2,
            this._btnSave,
            this._sep3,
            this._lblEntries,
            this._txtCount,
            this._btnSetCount,
            this._lblCount});
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.Location = new System.Drawing.Point(0, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Size = new System.Drawing.Size(900, 25);
            this.toolStripMain.TabIndex = 21;
            // 
            // _lblSearch
            // 
            this._lblSearch.Name = "_lblSearch";
            this._lblSearch.Text = "Search:";
            // 
            // _txtSearch
            // 
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.Size = new System.Drawing.Size(150, 25);
            this._txtSearch.ToolTipText = "ID (12 or 0x0C) or name";
            this._txtSearch.TextChanged += new System.EventHandler(this.OnSearchChanged);
            // 
            // _sep1
            // 
            this._sep1.Name = "_sep1";
            // 
            // _btnOverview
            // 
            this._btnOverview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnOverview.Name = "_btnOverview";
            this._btnOverview.Text = "▦ Overview";
            this._btnOverview.ToolTipText = "All lights as pictures – double click selects";
            this._btnOverview.Click += new System.EventHandler(this.OnClickOverview);
            // 
            // _btnNew
            // 
            this._btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnNew.Name = "_btnNew";
            this._btnNew.Text = "✚ New light…";
            this._btnNew.ToolTipText = "Create a new light in the light designer";
            this._btnNew.Click += new System.EventHandler(this.OnClickNewLight);
            // 
            // _btnEdit
            // 
            this._btnEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnEdit.Name = "_btnEdit";
            this._btnEdit.Text = "✎ Edit…";
            this._btnEdit.ToolTipText = "Edit the selected light in the light designer (mirror, rotate, size …)";
            this._btnEdit.Click += new System.EventHandler(this.OnClickEditLight);
            // 
            // _sep2
            // 
            this._sep2.Name = "_sep2";
            // 
            // _btnSave
            // 
            this._btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSave.Name = "_btnSave";
            this._btnSave.Text = "Save light.mul";
            this._btnSave.ToolTipText = "Write light.mul / lightidx.mul into the output folder";
            this._btnSave.Click += new System.EventHandler(this.OnClickSave);
            // 
            // _sep3
            // 
            this._sep3.Name = "_sep3";
            // 
            // _lblEntries
            // 
            this._lblEntries.Name = "_lblEntries";
            this._lblEntries.Text = "Entries:";
            // 
            // _txtCount
            // 
            this._txtCount.Name = "_txtCount";
            this._txtCount.Size = new System.Drawing.Size(55, 25);
            this._txtCount.ToolTipText = "Number of entries in lightidx.mul (classic files: 100)";
            // 
            // _btnSetCount
            // 
            this._btnSetCount.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnSetCount.Name = "_btnSetCount";
            this._btnSetCount.Text = "Set";
            this._btnSetCount.ToolTipText = "Extend (or shorten) the light files to this number of entries";
            this._btnSetCount.Click += new System.EventHandler(this.OnClickSetCount);
            // 
            // _lblCount
            // 
            this._lblCount.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblCount.Name = "_lblCount";
            this._lblCount.Text = "-";
            // 
            // treeViewContextMenuStrip
            // 
            this.treeViewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportImageToolStripMenuItem,
            this.toolStripSeparator1,
            this.renameToolStripMenuItem,
            this.copyIdToolStripMenuItem,
            this.toolStripSeparator6,
            this.removeToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.insertAtToolStripMenuItem,
            this.toolStripSeparator2,
            this.copyToolStripMenuItem,
            this.importToolStripMenuItem,
            this.toolStripSeparator3,
            this.editToolStripMenuItem,
            this.sizeToolStripMenuItem,
            this.toolStripSeparator4,
            this.saveToolStripMenuItem});
            this.treeViewContextMenuStrip.Name = "treeViewContextMenuStrip";
            this.treeViewContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.OnTreeMenuOpening);
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asPngToolStripMenuItem,
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem,
            this.toolStripSeparator5,
            this.exportAllToolStripMenuItem});
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Text = "Export image";
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Text = "As PNG";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportPng);
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Text = "As BMP";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Text = "As TIFF";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Text = "As JPG";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportJpg);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Text = "All lights as PNG into a folder…";
            this.exportAllToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportAll);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Text = "Name… (F2)";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.OnClickRename);
            // 
            // copyIdToolStripMenuItem
            // 
            this.copyIdToolStripMenuItem.Name = "copyIdToolStripMenuItem";
            this.copyIdToolStripMenuItem.Text = "Copy ID";
            this.copyIdToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopyId);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Text = "Replace from file…";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // insertAtToolStripMenuItem
            // 
            this.insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertText});
            this.insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            this.insertAtToolStripMenuItem.Text = "Insert at ID (file)";
            // 
            // InsertText
            // 
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(100, 23);
            this.InsertText.ToolTipText = "ID, then Enter";
            this.InsertText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownInsert);
            this.InsertText.TextChanged += new System.EventHandler(this.OnTextChangedInsert);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Text = "Copy image";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBoxInsertImport});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Text = "Import from clipboard (at ID)";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.OnClickImportClipboard);
            // 
            // toolStripTextBoxInsertImport
            // 
            this.toolStripTextBoxInsertImport.Name = "toolStripTextBoxInsertImport";
            this.toolStripTextBoxInsertImport.Size = new System.Drawing.Size(100, 23);
            this.toolStripTextBoxInsertImport.ToolTipText = "ID (empty = selected light), then Enter";
            this.toolStripTextBoxInsertImport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownImportClipboard);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Text = "Edit in light designer…";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.OnClickEditLight);
            // 
            // sizeToolStripMenuItem
            // 
            this.sizeToolStripMenuItem.Name = "sizeToolStripMenuItem";
            this.sizeToolStripMenuItem.Text = "Size / mirror / rotate…";
            this.sizeToolStripMenuItem.Click += new System.EventHandler(this.OnClickEditLight);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Text = "Save light.mul";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnClickSave);
            // 
            // previewContextMenuStrip
            // 
            this.previewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.iGPreviewToolStripMenuItem,
            this.backgroundLandTileToolStripMenuItem,
            this.lightTileToolStripMenuItem,
            this.toolStripSeparator7,
            this.copyPreviewToolStripMenuItem,
            this.savePreviewToolStripMenuItem});
            this.previewContextMenuStrip.Name = "previewContextMenuStrip";
            // 
            // iGPreviewToolStripMenuItem
            // 
            this.iGPreviewToolStripMenuItem.Name = "iGPreviewToolStripMenuItem";
            this.iGPreviewToolStripMenuItem.Text = "In-game preview";
            this.iGPreviewToolStripMenuItem.Click += new System.EventHandler(this.OnClickIgPreview);
            // 
            // backgroundLandTileToolStripMenuItem
            // 
            this.backgroundLandTileToolStripMenuItem.Name = "backgroundLandTileToolStripMenuItem";
            this.backgroundLandTileToolStripMenuItem.Text = "Choose ground (land tile)…";
            this.backgroundLandTileToolStripMenuItem.Click += new System.EventHandler(this.OnClickLandPick);
            // 
            // lightTileToolStripMenuItem
            // 
            this.lightTileToolStripMenuItem.Name = "lightTileToolStripMenuItem";
            this.lightTileToolStripMenuItem.Text = "Choose light source (item)…";
            this.lightTileToolStripMenuItem.Click += new System.EventHandler(this.OnClickItemPick);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            // 
            // copyPreviewToolStripMenuItem
            // 
            this.copyPreviewToolStripMenuItem.Name = "copyPreviewToolStripMenuItem";
            this.copyPreviewToolStripMenuItem.Text = "Copy preview";
            this.copyPreviewToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopyPreview);
            // 
            // savePreviewToolStripMenuItem
            // 
            this.savePreviewToolStripMenuItem.Name = "savePreviewToolStripMenuItem";
            this.savePreviewToolStripMenuItem.Text = "Save preview as PNG…";
            this.savePreviewToolStripMenuItem.Click += new System.EventHandler(this.OnClickSavePreview);
            // 
            // LightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Size = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.toolStripMain);
            this.Name = "LightControl";
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this._pnlTest.ResumeLayout(false);
            this._pnlTest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._picLand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._picItem)).EndInit();
            this._pnlOptions.ResumeLayout(false);
            this._pnlOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDark)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLightX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numLightY)).EndInit();
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.treeViewContextMenuStrip.ResumeLayout(false);
            this.previewContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeViewLights;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.TableLayoutPanel _pnlTest;
        private System.Windows.Forms.Label _lblLand;
        private System.Windows.Forms.PictureBox _picLand;
        private System.Windows.Forms.TextBox _txtLand;
        private System.Windows.Forms.Button _btnLandPrev;
        private System.Windows.Forms.Button _btnLandNext;
        private System.Windows.Forms.Button _btnLandPick;
        private System.Windows.Forms.Label _lblItem;
        private System.Windows.Forms.PictureBox _picItem;
        private System.Windows.Forms.TextBox _txtItem;
        private System.Windows.Forms.Button _btnItemPrev;
        private System.Windows.Forms.Button _btnItemNext;
        private System.Windows.Forms.Button _btnItemPick;
        private System.Windows.Forms.CheckBox _chkIgPreview;
        private System.Windows.Forms.CheckBox _chkOnlyLightItems;
        private System.Windows.Forms.FlowLayoutPanel _pnlOptions;
        private System.Windows.Forms.Label _lblDark;
        private System.Windows.Forms.NumericUpDown _numDark;
        private System.Windows.Forms.Label _lblLightX;
        private System.Windows.Forms.NumericUpDown _numLightX;
        private System.Windows.Forms.Label _lblLightY;
        private System.Windows.Forms.NumericUpDown _numLightY;
        private System.Windows.Forms.Label _lblZoom;
        private System.Windows.Forms.ComboBox _cmbZoom;
        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripLabel _lblSearch;
        private System.Windows.Forms.ToolStripTextBox _txtSearch;
        private System.Windows.Forms.ToolStripSeparator _sep1;
        private System.Windows.Forms.ToolStripButton _btnOverview;
        private System.Windows.Forms.ToolStripButton _btnNew;
        private System.Windows.Forms.ToolStripButton _btnEdit;
        private System.Windows.Forms.ToolStripSeparator _sep2;
        private System.Windows.Forms.ToolStripButton _btnSave;
        private System.Windows.Forms.ToolStripSeparator _sep3;
        private System.Windows.Forms.ToolStripLabel _lblEntries;
        private System.Windows.Forms.ToolStripTextBox _txtCount;
        private System.Windows.Forms.ToolStripButton _btnSetCount;
        private System.Windows.Forms.ToolStripLabel _lblCount;
        private System.Windows.Forms.ContextMenuStrip treeViewContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxInsertImport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip previewContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem iGPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundLandTileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lightTileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem copyPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePreviewToolStripMenuItem;
    }
}

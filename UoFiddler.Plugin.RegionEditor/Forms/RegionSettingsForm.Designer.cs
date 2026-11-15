namespace UoFiddler.Plugin.RegionEditor.Forms
{
    partial class RegionSettingsForm
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
            this.components = new System.ComponentModel.Container();
            this._grid = new System.Windows.Forms.TableLayoutPanel();
            this._lblHeadServer = new System.Windows.Forms.Label();
            this._lblServUo = new System.Windows.Forms.Label();
            this._txtServUo = new System.Windows.Forms.TextBox();
            this._btnBrowseServUo = new System.Windows.Forms.Button();
            this._btnLoadServUo = new System.Windows.Forms.Button();
            this._lblSphere = new System.Windows.Forms.Label();
            this._txtSphere = new System.Windows.Forms.TextBox();
            this._btnBrowseSphere = new System.Windows.Forms.Button();
            this._btnLoadSphere = new System.Windows.Forms.Button();
            this._lblPol = new System.Windows.Forms.Label();
            this._txtPol = new System.Windows.Forms.TextBox();
            this._btnBrowsePol = new System.Windows.Forms.Button();
            this._btnLoadPol = new System.Windows.Forms.Button();
            this._lblExport = new System.Windows.Forms.Label();
            this._txtExport = new System.Windows.Forms.TextBox();
            this._btnBrowseExport = new System.Windows.Forms.Button();
            this._lblServerState = new System.Windows.Forms.Label();
            this._lblStartup = new System.Windows.Forms.Label();
            this._cmbStartup = new System.Windows.Forms.ComboBox();
            this._lblHeadExport = new System.Windows.Forms.Label();
            this._lblMerge = new System.Windows.Forms.Label();
            this._cmbMerge = new System.Windows.Forms.ComboBox();
            this._lblRectFormat = new System.Windows.Forms.Label();
            this._cmbRectFormat = new System.Windows.Forms.ComboBox();
            this._lblSphereEvents = new System.Windows.Forms.Label();
            this._txtSphereEvents = new System.Windows.Forms.TextBox();
            this._chkSkipHidden = new System.Windows.Forms.CheckBox();
            this._chkPolTexts = new System.Windows.Forms.CheckBox();
            this._lblHeadEditor = new System.Windows.Forms.Label();
            this._lblLanguage = new System.Windows.Forms.Label();
            this._cmbLanguage = new System.Windows.Forms.ComboBox();
            this._lblTheme = new System.Windows.Forms.Label();
            this._cmbTheme = new System.Windows.Forms.ComboBox();
            this._lblPriority = new System.Windows.Forms.Label();
            this._numPriority = new System.Windows.Forms.NumericUpDown();
            this._lblAlpha = new System.Windows.Forms.Label();
            this._numAlpha = new System.Windows.Forms.NumericUpDown();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._btnOpenExport = new System.Windows.Forms.Button();
            this._folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this._fileDialog = new System.Windows.Forms.OpenFileDialog();
            this._grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPriority)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._numAlpha)).BeginInit();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _grid
            // 
            this._grid.Controls.Add(this._lblHeadServer, 0, 0);
            this._grid.Controls.Add(this._lblServUo, 0, 1);
            this._grid.Controls.Add(this._txtServUo, 1, 1);
            this._grid.Controls.Add(this._btnBrowseServUo, 2, 1);
            this._grid.Controls.Add(this._btnLoadServUo, 3, 1);
            this._grid.Controls.Add(this._lblSphere, 0, 2);
            this._grid.Controls.Add(this._txtSphere, 1, 2);
            this._grid.Controls.Add(this._btnBrowseSphere, 2, 2);
            this._grid.Controls.Add(this._btnLoadSphere, 3, 2);
            this._grid.Controls.Add(this._lblPol, 0, 3);
            this._grid.Controls.Add(this._txtPol, 1, 3);
            this._grid.Controls.Add(this._btnBrowsePol, 2, 3);
            this._grid.Controls.Add(this._btnLoadPol, 3, 3);
            this._grid.Controls.Add(this._lblExport, 0, 4);
            this._grid.Controls.Add(this._txtExport, 1, 4);
            this._grid.Controls.Add(this._btnBrowseExport, 2, 4);
            this._grid.Controls.Add(this._lblServerState, 1, 5);
            this._grid.Controls.Add(this._lblStartup, 0, 6);
            this._grid.Controls.Add(this._cmbStartup, 1, 6);
            this._grid.Controls.Add(this._lblHeadExport, 0, 7);
            this._grid.Controls.Add(this._lblMerge, 0, 8);
            this._grid.Controls.Add(this._cmbMerge, 1, 8);
            this._grid.Controls.Add(this._lblRectFormat, 0, 9);
            this._grid.Controls.Add(this._cmbRectFormat, 1, 9);
            this._grid.Controls.Add(this._lblSphereEvents, 0, 10);
            this._grid.Controls.Add(this._txtSphereEvents, 1, 10);
            this._grid.Controls.Add(this._chkSkipHidden, 1, 11);
            this._grid.Controls.Add(this._chkPolTexts, 1, 12);
            this._grid.Controls.Add(this._lblHeadEditor, 0, 13);
            this._grid.Controls.Add(this._lblLanguage, 0, 14);
            this._grid.Controls.Add(this._cmbLanguage, 1, 14);
            this._grid.Controls.Add(this._lblTheme, 0, 15);
            this._grid.Controls.Add(this._cmbTheme, 1, 15);
            this._grid.Controls.Add(this._lblPriority, 0, 16);
            this._grid.Controls.Add(this._numPriority, 1, 16);
            this._grid.Controls.Add(this._lblAlpha, 0, 17);
            this._grid.Controls.Add(this._numAlpha, 1, 17);
            this._grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._grid.AutoSize = true;
            this._grid.ColumnCount = 4;
            this._grid.Dock = System.Windows.Forms.DockStyle.Top;
            this._grid.Name = "_grid";
            this._grid.Padding = new System.Windows.Forms.Padding(10);
            this._grid.RowCount = 19;
            this._grid.TabIndex = 0;
            // 
            // _lblHeadServer
            // 
            this._grid.SetColumnSpan(this._lblHeadServer, 4);
            this._lblHeadServer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblHeadServer.AutoSize = true;
            this._lblHeadServer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this._lblHeadServer.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblHeadServer.Name = "_lblHeadServer";
            this._lblHeadServer.Text = "Server folders";
            // 
            // _lblServUo
            // 
            this._lblServUo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblServUo.AutoSize = true;
            this._lblServUo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblServUo.Name = "_lblServUo";
            this._lblServUo.Text = "ServUO / RunUO folder:";
            // 
            // _txtServUo
            // 
            this._txtServUo.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtServUo.Name = "_txtServUo";
            this._txtServUo.PlaceholderText = "e.g. C:\\ServUO  (Data\\Regions.xml)";
            this._txtServUo.Size = new System.Drawing.Size(380, 23);
            this._txtServUo.TabIndex = 1;
            this._txtServUo.TextChanged += new System.EventHandler(this.OnPathChanged);
            // 
            // _btnBrowseServUo
            // 
            this._btnBrowseServUo.AutoSize = true;
            this._btnBrowseServUo.Name = "_btnBrowseServUo";
            this._btnBrowseServUo.TabIndex = 2;
            this._btnBrowseServUo.Text = "…";
            this._btnBrowseServUo.UseVisualStyleBackColor = true;
            this._btnBrowseServUo.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // _btnLoadServUo
            // 
            this._btnLoadServUo.AutoSize = true;
            this._btnLoadServUo.Name = "_btnLoadServUo";
            this._btnLoadServUo.TabIndex = 3;
            this._btnLoadServUo.Text = "Load";
            this._btnLoadServUo.UseVisualStyleBackColor = true;
            this._btnLoadServUo.Click += new System.EventHandler(this.OnClickLoadNow);
            // 
            // _lblSphere
            // 
            this._lblSphere.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblSphere.AutoSize = true;
            this._lblSphere.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblSphere.Name = "_lblSphere";
            this._lblSphere.Text = "Sphere areas file:";
            // 
            // _txtSphere
            // 
            this._txtSphere.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtSphere.Name = "_txtSphere";
            this._txtSphere.PlaceholderText = "e.g. C:\\Sphere\\scripts\\maps\\map0\\map0_areas.scp";
            this._txtSphere.Size = new System.Drawing.Size(380, 23);
            this._txtSphere.TabIndex = 4;
            this._txtSphere.TextChanged += new System.EventHandler(this.OnPathChanged);
            // 
            // _btnBrowseSphere
            // 
            this._btnBrowseSphere.AutoSize = true;
            this._btnBrowseSphere.Name = "_btnBrowseSphere";
            this._btnBrowseSphere.TabIndex = 5;
            this._btnBrowseSphere.Text = "…";
            this._btnBrowseSphere.UseVisualStyleBackColor = true;
            this._btnBrowseSphere.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // _btnLoadSphere
            // 
            this._btnLoadSphere.AutoSize = true;
            this._btnLoadSphere.Name = "_btnLoadSphere";
            this._btnLoadSphere.TabIndex = 6;
            this._btnLoadSphere.Text = "Load";
            this._btnLoadSphere.UseVisualStyleBackColor = true;
            this._btnLoadSphere.Click += new System.EventHandler(this.OnClickLoadNow);
            // 
            // _lblPol
            // 
            this._lblPol.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPol.AutoSize = true;
            this._lblPol.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPol.Name = "_lblPol";
            this._lblPol.Text = "POL regions folder:";
            // 
            // _txtPol
            // 
            this._txtPol.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtPol.Name = "_txtPol";
            this._txtPol.PlaceholderText = "e.g. C:\\pol\\regions";
            this._txtPol.Size = new System.Drawing.Size(380, 23);
            this._txtPol.TabIndex = 7;
            this._txtPol.TextChanged += new System.EventHandler(this.OnPathChanged);
            // 
            // _btnBrowsePol
            // 
            this._btnBrowsePol.AutoSize = true;
            this._btnBrowsePol.Name = "_btnBrowsePol";
            this._btnBrowsePol.TabIndex = 8;
            this._btnBrowsePol.Text = "…";
            this._btnBrowsePol.UseVisualStyleBackColor = true;
            this._btnBrowsePol.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // _btnLoadPol
            // 
            this._btnLoadPol.AutoSize = true;
            this._btnLoadPol.Name = "_btnLoadPol";
            this._btnLoadPol.TabIndex = 9;
            this._btnLoadPol.Text = "Load";
            this._btnLoadPol.UseVisualStyleBackColor = true;
            this._btnLoadPol.Click += new System.EventHandler(this.OnClickLoadNow);
            // 
            // _lblExport
            // 
            this._lblExport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblExport.AutoSize = true;
            this._lblExport.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblExport.Name = "_lblExport";
            this._lblExport.Text = "Export folder:";
            // 
            // _txtExport
            // 
            this._txtExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtExport.Name = "_txtExport";
            this._txtExport.PlaceholderText = "copy the files from here to your server";
            this._txtExport.Size = new System.Drawing.Size(380, 23);
            this._txtExport.TabIndex = 10;
            this._txtExport.TextChanged += new System.EventHandler(this.OnPathChanged);
            // 
            // _btnBrowseExport
            // 
            this._btnBrowseExport.AutoSize = true;
            this._btnBrowseExport.Name = "_btnBrowseExport";
            this._btnBrowseExport.TabIndex = 11;
            this._btnBrowseExport.Text = "…";
            this._btnBrowseExport.UseVisualStyleBackColor = true;
            this._btnBrowseExport.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // _lblServerState
            // 
            this._grid.SetColumnSpan(this._lblServerState, 3);
            this._lblServerState.AutoSize = true;
            this._lblServerState.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblServerState.Name = "_lblServerState";
            this._lblServerState.Text = "-";
            // 
            // _lblStartup
            // 
            this._lblStartup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblStartup.AutoSize = true;
            this._lblStartup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblStartup.Name = "_lblStartup";
            this._lblStartup.Text = "On start:";
            // 
            // _cmbStartup
            // 
            this._cmbStartup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbStartup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbStartup.Name = "_cmbStartup";
            this._cmbStartup.TabIndex = 12;
            // 
            // _lblHeadExport
            // 
            this._grid.SetColumnSpan(this._lblHeadExport, 4);
            this._lblHeadExport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblHeadExport.AutoSize = true;
            this._lblHeadExport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this._lblHeadExport.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblHeadExport.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this._lblHeadExport.Name = "_lblHeadExport";
            this._lblHeadExport.Text = "Export";
            // 
            // _lblMerge
            // 
            this._lblMerge.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblMerge.AutoSize = true;
            this._lblMerge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblMerge.Name = "_lblMerge";
            this._lblMerge.Text = "Existing Regions.xml:";
            // 
            // _cmbMerge
            // 
            this._cmbMerge.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbMerge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbMerge.Name = "_cmbMerge";
            this._cmbMerge.TabIndex = 13;
            // 
            // _lblRectFormat
            // 
            this._lblRectFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblRectFormat.AutoSize = true;
            this._lblRectFormat.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblRectFormat.Name = "_lblRectFormat";
            this._lblRectFormat.Text = "Rectangles:";
            // 
            // _cmbRectFormat
            // 
            this._cmbRectFormat.Items.AddRange(new object[] {
            "x / y / width / height",
            "x1 / y1 / x2 / y2"});
            this._cmbRectFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this._cmbRectFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbRectFormat.Name = "_cmbRectFormat";
            this._cmbRectFormat.TabIndex = 14;
            // 
            // _lblSphereEvents
            // 
            this._lblSphereEvents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblSphereEvents.AutoSize = true;
            this._lblSphereEvents.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblSphereEvents.Name = "_lblSphereEvents";
            this._lblSphereEvents.Text = "Sphere EVENTS:";
            // 
            // _txtSphereEvents
            // 
            this._txtSphereEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtSphereEvents.Name = "_txtSphereEvents";
            this._txtSphereEvents.TabIndex = 15;
            // 
            // _chkSkipHidden
            // 
            this._chkSkipHidden.AutoSize = true;
            this._chkSkipHidden.Name = "_chkSkipHidden";
            this._chkSkipHidden.TabIndex = 16;
            this._chkSkipHidden.Text = "Leave out hidden regions";
            this._chkSkipHidden.UseVisualStyleBackColor = true;
            // 
            // _chkPolTexts
            // 
            this._chkPolTexts.AutoSize = true;
            this._chkPolTexts.Name = "_chkPolTexts";
            this._chkPolTexts.TabIndex = 17;
            this._chkPolTexts.Text = "POL: guard texts (EnterText/LeaveText)";
            this._chkPolTexts.UseVisualStyleBackColor = true;
            // 
            // _lblHeadEditor
            // 
            this._grid.SetColumnSpan(this._lblHeadEditor, 4);
            this._lblHeadEditor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblHeadEditor.AutoSize = true;
            this._lblHeadEditor.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this._lblHeadEditor.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblHeadEditor.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this._lblHeadEditor.Name = "_lblHeadEditor";
            this._lblHeadEditor.Text = "Editor";
            // 
            // _lblLanguage
            // 
            this._lblLanguage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblLanguage.AutoSize = true;
            this._lblLanguage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblLanguage.Name = "_lblLanguage";
            this._lblLanguage.Text = "Language:";
            // 
            // _cmbLanguage
            // 
            this._cmbLanguage.Items.AddRange(new object[] {
            "Deutsch",
            "English",
            "Français"});
            this._cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbLanguage.Name = "_cmbLanguage";
            this._cmbLanguage.Size = new System.Drawing.Size(160, 23);
            this._cmbLanguage.TabIndex = 18;
            // 
            // _lblTheme
            // 
            this._lblTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblTheme.AutoSize = true;
            this._lblTheme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblTheme.Name = "_lblTheme";
            this._lblTheme.Text = "Design:";
            // 
            // _cmbTheme
            // 
            this._cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbTheme.Name = "_cmbTheme";
            this._cmbTheme.Size = new System.Drawing.Size(160, 23);
            this._cmbTheme.TabIndex = 19;
            // 
            // _lblPriority
            // 
            this._lblPriority.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblPriority.AutoSize = true;
            this._lblPriority.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblPriority.Name = "_lblPriority";
            this._lblPriority.Text = "Priority of new regions:";
            // 
            // _numPriority
            // 
            this._numPriority.Maximum = new decimal(new int[] { 255, 0, 0, 0});
            this._numPriority.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numPriority.Name = "_numPriority";
            this._numPriority.Size = new System.Drawing.Size(80, 23);
            this._numPriority.TabIndex = 20;
            this._numPriority.Value = new decimal(new int[] { 50, 0, 0, 0});
            // 
            // _lblAlpha
            // 
            this._lblAlpha.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblAlpha.AutoSize = true;
            this._lblAlpha.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._lblAlpha.Name = "_lblAlpha";
            this._lblAlpha.Text = "Fill opacity:";
            // 
            // _numAlpha
            // 
            this._numAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0});
            this._numAlpha.Minimum = new decimal(new int[] { 0, 0, 0, 0});
            this._numAlpha.Name = "_numAlpha";
            this._numAlpha.Size = new System.Drawing.Size(80, 23);
            this._numAlpha.TabIndex = 21;
            this._numAlpha.Value = new decimal(new int[] { 70, 0, 0, 0});
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnCancel);
            this._bottom.Controls.Add(this._btnOk);
            this._bottom.Controls.Add(this._btnOpenExport);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(8);
            this._bottom.TabIndex = 22;
            // 
            // _btnCancel
            // 
            this._btnCancel.AutoSize = true;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(90, 27);
            this._btnCancel.TabIndex = 23;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.AutoSize = true;
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(90, 27);
            this._btnOk.TabIndex = 24;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.OnClickOk);
            // 
            // _btnOpenExport
            // 
            this._btnOpenExport.AutoSize = true;
            this._btnOpenExport.Name = "_btnOpenExport";
            this._btnOpenExport.TabIndex = 25;
            this._btnOpenExport.Text = "Open export folder";
            this._btnOpenExport.UseVisualStyleBackColor = true;
            this._btnOpenExport.Click += new System.EventHandler(this.OnClickOpenExport);
            // 
            // _folderDialog
            // 
            // 
            // _fileDialog
            // 
            this._fileDialog.CheckFileExists = false;
            this._fileDialog.Filter = "Sphere scripts (*.scp)|*.scp|All files (*.*)|*.*";
            // 
            // RegionSettingsForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(700, 620);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Region editor – settings";
            this.Controls.Add(this._grid);
            this.Controls.Add(this._bottom);
            this.Name = "RegionSettingsForm";
            this._grid.ResumeLayout(false);
            this._grid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numPriority)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._numAlpha)).EndInit();
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _grid;
        private System.Windows.Forms.Label _lblHeadServer;
        private System.Windows.Forms.Label _lblServUo;
        private System.Windows.Forms.TextBox _txtServUo;
        private System.Windows.Forms.Button _btnBrowseServUo;
        private System.Windows.Forms.Button _btnLoadServUo;
        private System.Windows.Forms.Label _lblSphere;
        private System.Windows.Forms.TextBox _txtSphere;
        private System.Windows.Forms.Button _btnBrowseSphere;
        private System.Windows.Forms.Button _btnLoadSphere;
        private System.Windows.Forms.Label _lblPol;
        private System.Windows.Forms.TextBox _txtPol;
        private System.Windows.Forms.Button _btnBrowsePol;
        private System.Windows.Forms.Button _btnLoadPol;
        private System.Windows.Forms.Label _lblExport;
        private System.Windows.Forms.TextBox _txtExport;
        private System.Windows.Forms.Button _btnBrowseExport;
        private System.Windows.Forms.Label _lblServerState;
        private System.Windows.Forms.Label _lblStartup;
        private System.Windows.Forms.ComboBox _cmbStartup;
        private System.Windows.Forms.Label _lblHeadExport;
        private System.Windows.Forms.Label _lblMerge;
        private System.Windows.Forms.ComboBox _cmbMerge;
        private System.Windows.Forms.Label _lblRectFormat;
        private System.Windows.Forms.ComboBox _cmbRectFormat;
        private System.Windows.Forms.Label _lblSphereEvents;
        private System.Windows.Forms.TextBox _txtSphereEvents;
        private System.Windows.Forms.CheckBox _chkSkipHidden;
        private System.Windows.Forms.CheckBox _chkPolTexts;
        private System.Windows.Forms.Label _lblHeadEditor;
        private System.Windows.Forms.Label _lblLanguage;
        private System.Windows.Forms.ComboBox _cmbLanguage;
        private System.Windows.Forms.Label _lblTheme;
        private System.Windows.Forms.ComboBox _cmbTheme;
        private System.Windows.Forms.Label _lblPriority;
        private System.Windows.Forms.NumericUpDown _numPriority;
        private System.Windows.Forms.Label _lblAlpha;
        private System.Windows.Forms.NumericUpDown _numAlpha;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Button _btnOpenExport;
        private System.Windows.Forms.FolderBrowserDialog _folderDialog;
        private System.Windows.Forms.OpenFileDialog _fileDialog;
    }
}

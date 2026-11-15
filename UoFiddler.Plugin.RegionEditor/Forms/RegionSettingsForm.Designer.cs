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
            _grid = new System.Windows.Forms.TableLayoutPanel();
            _lblHeadServer = new System.Windows.Forms.Label();
            _lblServUo = new System.Windows.Forms.Label();
            _txtServUo = new System.Windows.Forms.TextBox();
            _btnBrowseServUo = new System.Windows.Forms.Button();
            _btnLoadServUo = new System.Windows.Forms.Button();
            _lblSphere = new System.Windows.Forms.Label();
            _txtSphere = new System.Windows.Forms.TextBox();
            _btnBrowseSphere = new System.Windows.Forms.Button();
            _btnLoadSphere = new System.Windows.Forms.Button();
            _lblPol = new System.Windows.Forms.Label();
            _txtPol = new System.Windows.Forms.TextBox();
            _btnBrowsePol = new System.Windows.Forms.Button();
            _btnLoadPol = new System.Windows.Forms.Button();
            _lblExport = new System.Windows.Forms.Label();
            _txtExport = new System.Windows.Forms.TextBox();
            _btnBrowseExport = new System.Windows.Forms.Button();
            _lblServerState = new System.Windows.Forms.Label();
            _lblStartup = new System.Windows.Forms.Label();
            _cmbStartup = new System.Windows.Forms.ComboBox();
            _lblHeadExport = new System.Windows.Forms.Label();
            _lblMerge = new System.Windows.Forms.Label();
            _cmbMerge = new System.Windows.Forms.ComboBox();
            _lblRectFormat = new System.Windows.Forms.Label();
            _cmbRectFormat = new System.Windows.Forms.ComboBox();
            _lblSphereEvents = new System.Windows.Forms.Label();
            _txtSphereEvents = new System.Windows.Forms.TextBox();
            _chkSkipHidden = new System.Windows.Forms.CheckBox();
            _chkPolTexts = new System.Windows.Forms.CheckBox();
            _lblHeadEditor = new System.Windows.Forms.Label();
            _lblLanguage = new System.Windows.Forms.Label();
            _cmbLanguage = new System.Windows.Forms.ComboBox();
            _lblTheme = new System.Windows.Forms.Label();
            _cmbTheme = new System.Windows.Forms.ComboBox();
            _lblPriority = new System.Windows.Forms.Label();
            _numPriority = new System.Windows.Forms.NumericUpDown();
            _lblAlpha = new System.Windows.Forms.Label();
            _numAlpha = new System.Windows.Forms.NumericUpDown();
            _bottom = new System.Windows.Forms.FlowLayoutPanel();
            _btnCancel = new System.Windows.Forms.Button();
            _btnOk = new System.Windows.Forms.Button();
            _btnOpenExport = new System.Windows.Forms.Button();
            _folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            _fileDialog = new System.Windows.Forms.OpenFileDialog();
            _grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numPriority).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_numAlpha).BeginInit();
            _bottom.SuspendLayout();
            SuspendLayout();
            // 
            // _grid
            // 
            _grid.AutoSize = true;
            _grid.ColumnCount = 4;
            _grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            _grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            _grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            _grid.Controls.Add(_lblHeadServer, 0, 0);
            _grid.Controls.Add(_lblServUo, 0, 1);
            _grid.Controls.Add(_txtServUo, 1, 1);
            _grid.Controls.Add(_btnBrowseServUo, 2, 1);
            _grid.Controls.Add(_btnLoadServUo, 3, 1);
            _grid.Controls.Add(_lblSphere, 0, 2);
            _grid.Controls.Add(_txtSphere, 1, 2);
            _grid.Controls.Add(_btnBrowseSphere, 2, 2);
            _grid.Controls.Add(_btnLoadSphere, 3, 2);
            _grid.Controls.Add(_lblPol, 0, 3);
            _grid.Controls.Add(_txtPol, 1, 3);
            _grid.Controls.Add(_btnBrowsePol, 2, 3);
            _grid.Controls.Add(_btnLoadPol, 3, 3);
            _grid.Controls.Add(_lblExport, 0, 4);
            _grid.Controls.Add(_txtExport, 1, 4);
            _grid.Controls.Add(_btnBrowseExport, 2, 4);
            _grid.Controls.Add(_lblServerState, 1, 5);
            _grid.Controls.Add(_lblStartup, 0, 6);
            _grid.Controls.Add(_cmbStartup, 1, 6);
            _grid.Controls.Add(_lblHeadExport, 0, 7);
            _grid.Controls.Add(_lblMerge, 0, 8);
            _grid.Controls.Add(_cmbMerge, 1, 8);
            _grid.Controls.Add(_lblRectFormat, 0, 9);
            _grid.Controls.Add(_cmbRectFormat, 1, 9);
            _grid.Controls.Add(_lblSphereEvents, 0, 10);
            _grid.Controls.Add(_txtSphereEvents, 1, 10);
            _grid.Controls.Add(_chkSkipHidden, 1, 11);
            _grid.Controls.Add(_chkPolTexts, 1, 12);
            _grid.Controls.Add(_lblHeadEditor, 0, 13);
            _grid.Controls.Add(_lblLanguage, 0, 14);
            _grid.Controls.Add(_cmbLanguage, 1, 14);
            _grid.Controls.Add(_lblTheme, 0, 15);
            _grid.Controls.Add(_cmbTheme, 1, 15);
            _grid.Controls.Add(_lblPriority, 0, 16);
            _grid.Controls.Add(_numPriority, 1, 16);
            _grid.Controls.Add(_lblAlpha, 0, 17);
            _grid.Controls.Add(_numAlpha, 1, 17);
            _grid.Dock = System.Windows.Forms.DockStyle.Top;
            _grid.Location = new System.Drawing.Point(0, 0);
            _grid.Name = "_grid";
            _grid.Padding = new System.Windows.Forms.Padding(10);
            _grid.RowCount = 19;
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _grid.Size = new System.Drawing.Size(715, 531);
            _grid.TabIndex = 0;
            // 
            // _lblHeadServer
            // 
            _lblHeadServer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblHeadServer.AutoSize = true;
            _grid.SetColumnSpan(_lblHeadServer, 4);
            _lblHeadServer.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            _lblHeadServer.Location = new System.Drawing.Point(13, 16);
            _lblHeadServer.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblHeadServer.Name = "_lblHeadServer";
            _lblHeadServer.Size = new System.Drawing.Size(94, 17);
            _lblHeadServer.TabIndex = 0;
            _lblHeadServer.Text = "Server folders";
            // 
            // _lblServUo
            // 
            _lblServUo.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblServUo.AutoSize = true;
            _lblServUo.Location = new System.Drawing.Point(13, 45);
            _lblServUo.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblServUo.Name = "_lblServUo";
            _lblServUo.Size = new System.Drawing.Size(132, 15);
            _lblServUo.TabIndex = 1;
            _lblServUo.Text = "ServUO / RunUO folder:";
            // 
            // _txtServUo
            // 
            _txtServUo.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtServUo.Location = new System.Drawing.Point(151, 39);
            _txtServUo.Name = "_txtServUo";
            _txtServUo.PlaceholderText = "e.g. C:\\ServUO  (Data\\Regions.xml)";
            _txtServUo.Size = new System.Drawing.Size(389, 23);
            _txtServUo.TabIndex = 1;
            _txtServUo.TextChanged += OnPathChanged;
            // 
            // _btnBrowseServUo
            // 
            _btnBrowseServUo.AutoSize = true;
            _btnBrowseServUo.Location = new System.Drawing.Point(546, 39);
            _btnBrowseServUo.Name = "_btnBrowseServUo";
            _btnBrowseServUo.Size = new System.Drawing.Size(75, 25);
            _btnBrowseServUo.TabIndex = 2;
            _btnBrowseServUo.Text = "…";
            _btnBrowseServUo.UseVisualStyleBackColor = true;
            _btnBrowseServUo.Click += OnClickBrowse;
            // 
            // _btnLoadServUo
            // 
            _btnLoadServUo.AutoSize = true;
            _btnLoadServUo.Location = new System.Drawing.Point(627, 39);
            _btnLoadServUo.Name = "_btnLoadServUo";
            _btnLoadServUo.Size = new System.Drawing.Size(75, 25);
            _btnLoadServUo.TabIndex = 3;
            _btnLoadServUo.Text = "Load";
            _btnLoadServUo.UseVisualStyleBackColor = true;
            _btnLoadServUo.Click += OnClickLoadNow;
            // 
            // _lblSphere
            // 
            _lblSphere.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblSphere.AutoSize = true;
            _lblSphere.Location = new System.Drawing.Point(13, 76);
            _lblSphere.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblSphere.Name = "_lblSphere";
            _lblSphere.Size = new System.Drawing.Size(95, 15);
            _lblSphere.TabIndex = 4;
            _lblSphere.Text = "Sphere areas file:";
            // 
            // _txtSphere
            // 
            _txtSphere.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtSphere.Location = new System.Drawing.Point(151, 70);
            _txtSphere.Name = "_txtSphere";
            _txtSphere.PlaceholderText = "e.g. C:\\Sphere\\scripts\\maps\\map0\\map0_areas.scp";
            _txtSphere.Size = new System.Drawing.Size(389, 23);
            _txtSphere.TabIndex = 4;
            _txtSphere.TextChanged += OnPathChanged;
            // 
            // _btnBrowseSphere
            // 
            _btnBrowseSphere.AutoSize = true;
            _btnBrowseSphere.Location = new System.Drawing.Point(546, 70);
            _btnBrowseSphere.Name = "_btnBrowseSphere";
            _btnBrowseSphere.Size = new System.Drawing.Size(75, 25);
            _btnBrowseSphere.TabIndex = 5;
            _btnBrowseSphere.Text = "…";
            _btnBrowseSphere.UseVisualStyleBackColor = true;
            _btnBrowseSphere.Click += OnClickBrowse;
            // 
            // _btnLoadSphere
            // 
            _btnLoadSphere.AutoSize = true;
            _btnLoadSphere.Location = new System.Drawing.Point(627, 70);
            _btnLoadSphere.Name = "_btnLoadSphere";
            _btnLoadSphere.Size = new System.Drawing.Size(75, 25);
            _btnLoadSphere.TabIndex = 6;
            _btnLoadSphere.Text = "Load";
            _btnLoadSphere.UseVisualStyleBackColor = true;
            _btnLoadSphere.Click += OnClickLoadNow;
            // 
            // _lblPol
            // 
            _lblPol.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblPol.AutoSize = true;
            _lblPol.Location = new System.Drawing.Point(13, 107);
            _lblPol.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblPol.Name = "_lblPol";
            _lblPol.Size = new System.Drawing.Size(108, 15);
            _lblPol.TabIndex = 7;
            _lblPol.Text = "POL regions folder:";
            // 
            // _txtPol
            // 
            _txtPol.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtPol.Location = new System.Drawing.Point(151, 101);
            _txtPol.Name = "_txtPol";
            _txtPol.PlaceholderText = "e.g. C:\\pol\\regions";
            _txtPol.Size = new System.Drawing.Size(389, 23);
            _txtPol.TabIndex = 7;
            _txtPol.TextChanged += OnPathChanged;
            // 
            // _btnBrowsePol
            // 
            _btnBrowsePol.AutoSize = true;
            _btnBrowsePol.Location = new System.Drawing.Point(546, 101);
            _btnBrowsePol.Name = "_btnBrowsePol";
            _btnBrowsePol.Size = new System.Drawing.Size(75, 25);
            _btnBrowsePol.TabIndex = 8;
            _btnBrowsePol.Text = "…";
            _btnBrowsePol.UseVisualStyleBackColor = true;
            _btnBrowsePol.Click += OnClickBrowse;
            // 
            // _btnLoadPol
            // 
            _btnLoadPol.AutoSize = true;
            _btnLoadPol.Location = new System.Drawing.Point(627, 101);
            _btnLoadPol.Name = "_btnLoadPol";
            _btnLoadPol.Size = new System.Drawing.Size(75, 25);
            _btnLoadPol.TabIndex = 9;
            _btnLoadPol.Text = "Load";
            _btnLoadPol.UseVisualStyleBackColor = true;
            _btnLoadPol.Click += OnClickLoadNow;
            // 
            // _lblExport
            // 
            _lblExport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblExport.AutoSize = true;
            _lblExport.Location = new System.Drawing.Point(13, 138);
            _lblExport.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblExport.Name = "_lblExport";
            _lblExport.Size = new System.Drawing.Size(78, 15);
            _lblExport.TabIndex = 10;
            _lblExport.Text = "Export folder:";
            // 
            // _txtExport
            // 
            _txtExport.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtExport.Location = new System.Drawing.Point(151, 132);
            _txtExport.Name = "_txtExport";
            _txtExport.PlaceholderText = "copy the files from here to your server";
            _txtExport.Size = new System.Drawing.Size(389, 23);
            _txtExport.TabIndex = 10;
            _txtExport.TextChanged += OnPathChanged;
            // 
            // _btnBrowseExport
            // 
            _btnBrowseExport.AutoSize = true;
            _btnBrowseExport.Location = new System.Drawing.Point(546, 132);
            _btnBrowseExport.Name = "_btnBrowseExport";
            _btnBrowseExport.Size = new System.Drawing.Size(75, 25);
            _btnBrowseExport.TabIndex = 11;
            _btnBrowseExport.Text = "…";
            _btnBrowseExport.UseVisualStyleBackColor = true;
            _btnBrowseExport.Click += OnClickBrowse;
            // 
            // _lblServerState
            // 
            _lblServerState.AutoSize = true;
            _grid.SetColumnSpan(_lblServerState, 3);
            _lblServerState.ForeColor = System.Drawing.SystemColors.GrayText;
            _lblServerState.Location = new System.Drawing.Point(151, 160);
            _lblServerState.Name = "_lblServerState";
            _lblServerState.Size = new System.Drawing.Size(12, 15);
            _lblServerState.TabIndex = 12;
            _lblServerState.Text = "-";
            // 
            // _lblStartup
            // 
            _lblStartup.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblStartup.AutoSize = true;
            _lblStartup.Location = new System.Drawing.Point(13, 183);
            _lblStartup.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblStartup.Name = "_lblStartup";
            _lblStartup.Size = new System.Drawing.Size(52, 15);
            _lblStartup.TabIndex = 13;
            _lblStartup.Text = "On start:";
            // 
            // _cmbStartup
            // 
            _cmbStartup.Dock = System.Windows.Forms.DockStyle.Fill;
            _cmbStartup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbStartup.Location = new System.Drawing.Point(151, 178);
            _cmbStartup.Name = "_cmbStartup";
            _cmbStartup.Size = new System.Drawing.Size(389, 23);
            _cmbStartup.TabIndex = 12;
            // 
            // _lblHeadExport
            // 
            _lblHeadExport.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblHeadExport.AutoSize = true;
            _grid.SetColumnSpan(_lblHeadExport, 4);
            _lblHeadExport.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            _lblHeadExport.Location = new System.Drawing.Point(13, 216);
            _lblHeadExport.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            _lblHeadExport.Name = "_lblHeadExport";
            _lblHeadExport.Size = new System.Drawing.Size(48, 17);
            _lblHeadExport.TabIndex = 14;
            _lblHeadExport.Text = "Export";
            // 
            // _lblMerge
            // 
            _lblMerge.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblMerge.AutoSize = true;
            _lblMerge.Location = new System.Drawing.Point(13, 244);
            _lblMerge.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblMerge.Name = "_lblMerge";
            _lblMerge.Size = new System.Drawing.Size(119, 15);
            _lblMerge.TabIndex = 15;
            _lblMerge.Text = "Existing Regions.xml:";
            // 
            // _cmbMerge
            // 
            _cmbMerge.Dock = System.Windows.Forms.DockStyle.Fill;
            _cmbMerge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbMerge.Location = new System.Drawing.Point(151, 239);
            _cmbMerge.Name = "_cmbMerge";
            _cmbMerge.Size = new System.Drawing.Size(389, 23);
            _cmbMerge.TabIndex = 13;
            // 
            // _lblRectFormat
            // 
            _lblRectFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblRectFormat.AutoSize = true;
            _lblRectFormat.Location = new System.Drawing.Point(13, 273);
            _lblRectFormat.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblRectFormat.Name = "_lblRectFormat";
            _lblRectFormat.Size = new System.Drawing.Size(67, 15);
            _lblRectFormat.TabIndex = 16;
            _lblRectFormat.Text = "Rectangles:";
            // 
            // _cmbRectFormat
            // 
            _cmbRectFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            _cmbRectFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbRectFormat.Items.AddRange(new object[] { "x / y / width / height", "x1 / y1 / x2 / y2" });
            _cmbRectFormat.Location = new System.Drawing.Point(151, 268);
            _cmbRectFormat.Name = "_cmbRectFormat";
            _cmbRectFormat.Size = new System.Drawing.Size(389, 23);
            _cmbRectFormat.TabIndex = 14;
            // 
            // _lblSphereEvents
            // 
            _lblSphereEvents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblSphereEvents.AutoSize = true;
            _lblSphereEvents.Location = new System.Drawing.Point(13, 302);
            _lblSphereEvents.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblSphereEvents.Name = "_lblSphereEvents";
            _lblSphereEvents.Size = new System.Drawing.Size(89, 15);
            _lblSphereEvents.TabIndex = 17;
            _lblSphereEvents.Text = "Sphere EVENTS:";
            // 
            // _txtSphereEvents
            // 
            _txtSphereEvents.Dock = System.Windows.Forms.DockStyle.Fill;
            _txtSphereEvents.Location = new System.Drawing.Point(151, 297);
            _txtSphereEvents.Name = "_txtSphereEvents";
            _txtSphereEvents.Size = new System.Drawing.Size(389, 23);
            _txtSphereEvents.TabIndex = 15;
            // 
            // _chkSkipHidden
            // 
            _chkSkipHidden.AutoSize = true;
            _chkSkipHidden.Location = new System.Drawing.Point(151, 326);
            _chkSkipHidden.Name = "_chkSkipHidden";
            _chkSkipHidden.Size = new System.Drawing.Size(159, 19);
            _chkSkipHidden.TabIndex = 16;
            _chkSkipHidden.Text = "Leave out hidden regions";
            _chkSkipHidden.UseVisualStyleBackColor = true;
            // 
            // _chkPolTexts
            // 
            _chkPolTexts.AutoSize = true;
            _chkPolTexts.Location = new System.Drawing.Point(151, 351);
            _chkPolTexts.Name = "_chkPolTexts";
            _chkPolTexts.Size = new System.Drawing.Size(228, 19);
            _chkPolTexts.TabIndex = 17;
            _chkPolTexts.Text = "POL: guard texts (EnterText/LeaveText)";
            _chkPolTexts.UseVisualStyleBackColor = true;
            // 
            // _lblHeadEditor
            // 
            _lblHeadEditor.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblHeadEditor.AutoSize = true;
            _grid.SetColumnSpan(_lblHeadEditor, 4);
            _lblHeadEditor.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            _lblHeadEditor.Location = new System.Drawing.Point(13, 385);
            _lblHeadEditor.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            _lblHeadEditor.Name = "_lblHeadEditor";
            _lblHeadEditor.Size = new System.Drawing.Size(45, 17);
            _lblHeadEditor.TabIndex = 18;
            _lblHeadEditor.Text = "Editor";
            // 
            // _lblLanguage
            // 
            _lblLanguage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblLanguage.AutoSize = true;
            _lblLanguage.Location = new System.Drawing.Point(13, 413);
            _lblLanguage.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblLanguage.Name = "_lblLanguage";
            _lblLanguage.Size = new System.Drawing.Size(62, 15);
            _lblLanguage.TabIndex = 19;
            _lblLanguage.Text = "Language:";
            // 
            // _cmbLanguage
            // 
            _cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbLanguage.Items.AddRange(new object[] { "Deutsch", "English", "Français" });
            _cmbLanguage.Location = new System.Drawing.Point(151, 408);
            _cmbLanguage.Name = "_cmbLanguage";
            _cmbLanguage.Size = new System.Drawing.Size(160, 23);
            _cmbLanguage.TabIndex = 18;
            // 
            // _lblTheme
            // 
            _lblTheme.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblTheme.AutoSize = true;
            _lblTheme.Location = new System.Drawing.Point(13, 442);
            _lblTheme.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblTheme.Name = "_lblTheme";
            _lblTheme.Size = new System.Drawing.Size(46, 15);
            _lblTheme.TabIndex = 20;
            _lblTheme.Text = "Design:";
            // 
            // _cmbTheme
            // 
            _cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbTheme.Location = new System.Drawing.Point(151, 437);
            _cmbTheme.Name = "_cmbTheme";
            _cmbTheme.Size = new System.Drawing.Size(160, 23);
            _cmbTheme.TabIndex = 19;
            // 
            // _lblPriority
            // 
            _lblPriority.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblPriority.AutoSize = true;
            _lblPriority.Location = new System.Drawing.Point(13, 471);
            _lblPriority.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblPriority.Name = "_lblPriority";
            _lblPriority.Size = new System.Drawing.Size(129, 15);
            _lblPriority.TabIndex = 21;
            _lblPriority.Text = "Priority of new regions:";
            // 
            // _numPriority
            // 
            _numPriority.Location = new System.Drawing.Point(151, 466);
            _numPriority.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _numPriority.Name = "_numPriority";
            _numPriority.Size = new System.Drawing.Size(80, 23);
            _numPriority.TabIndex = 20;
            _numPriority.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // _lblAlpha
            // 
            _lblAlpha.Anchor = System.Windows.Forms.AnchorStyles.Left;
            _lblAlpha.AutoSize = true;
            _lblAlpha.Location = new System.Drawing.Point(13, 500);
            _lblAlpha.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            _lblAlpha.Name = "_lblAlpha";
            _lblAlpha.Size = new System.Drawing.Size(67, 15);
            _lblAlpha.TabIndex = 22;
            _lblAlpha.Text = "Fill opacity:";
            // 
            // _numAlpha
            // 
            _numAlpha.Location = new System.Drawing.Point(151, 495);
            _numAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _numAlpha.Name = "_numAlpha";
            _numAlpha.Size = new System.Drawing.Size(80, 23);
            _numAlpha.TabIndex = 21;
            _numAlpha.Value = new decimal(new int[] { 70, 0, 0, 0 });
            // 
            // _bottom
            // 
            _bottom.AutoSize = true;
            _bottom.Controls.Add(_btnCancel);
            _bottom.Controls.Add(_btnOk);
            _bottom.Controls.Add(_btnOpenExport);
            _bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            _bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            _bottom.Location = new System.Drawing.Point(0, 639);
            _bottom.Name = "_bottom";
            _bottom.Padding = new System.Windows.Forms.Padding(8);
            _bottom.Size = new System.Drawing.Size(715, 49);
            _bottom.TabIndex = 22;
            // 
            // _btnCancel
            // 
            _btnCancel.AutoSize = true;
            _btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _btnCancel.Location = new System.Drawing.Point(606, 11);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new System.Drawing.Size(90, 27);
            _btnCancel.TabIndex = 23;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            _btnOk.AutoSize = true;
            _btnOk.Location = new System.Drawing.Point(510, 11);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new System.Drawing.Size(90, 27);
            _btnOk.TabIndex = 24;
            _btnOk.Text = "OK";
            _btnOk.UseVisualStyleBackColor = true;
            _btnOk.Click += OnClickOk;
            // 
            // _btnOpenExport
            // 
            _btnOpenExport.AutoSize = true;
            _btnOpenExport.Location = new System.Drawing.Point(387, 11);
            _btnOpenExport.Name = "_btnOpenExport";
            _btnOpenExport.Size = new System.Drawing.Size(117, 25);
            _btnOpenExport.TabIndex = 25;
            _btnOpenExport.Text = "Open export folder";
            _btnOpenExport.UseVisualStyleBackColor = true;
            _btnOpenExport.Click += OnClickOpenExport;
            // 
            // _fileDialog
            // 
            _fileDialog.CheckFileExists = false;
            _fileDialog.Filter = "Sphere scripts (*.scp)|*.scp|All files (*.*)|*.*";
            // 
            // RegionSettingsForm
            // 
            AcceptButton = _btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _btnCancel;
            ClientSize = new System.Drawing.Size(715, 688);
            Controls.Add(_grid);
            Controls.Add(_bottom);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RegionSettingsForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Region editor – settings";
            _grid.ResumeLayout(false);
            _grid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numPriority).EndInit();
            ((System.ComponentModel.ISupportInitialize)_numAlpha).EndInit();
            _bottom.ResumeLayout(false);
            _bottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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

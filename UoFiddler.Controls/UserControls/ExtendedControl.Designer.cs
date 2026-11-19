/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Controls.UserControls
{
    partial class ExtendedControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._tips = new System.Windows.Forms.ToolTip(this.components);
            this._rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this._introLabel = new System.Windows.Forms.Label();
            this._sizeGroup = new System.Windows.Forms.GroupBox();
            this._sizeOuter = new System.Windows.Forms.TableLayoutPanel();
            this._sizeGrid = new System.Windows.Forms.TableLayoutPanel();
            this._head0 = new System.Windows.Forms.Label();
            this._head1 = new System.Windows.Forms.Label();
            this._head2 = new System.Windows.Forms.Label();
            this._head3 = new System.Windows.Forms.Label();
            this._head4 = new System.Windows.Forms.Label();
            this._head5 = new System.Windows.Forms.Label();
            this._head6 = new System.Windows.Forms.Label();
            this._useItems = new System.Windows.Forms.CheckBox();
            this._titleItems = new System.Windows.Forms.Label();
            this._sizeItems = new System.Windows.Forms.NumericUpDown();
            this._nowItems = new System.Windows.Forms.Label();
            this._rangeItems = new System.Windows.Forms.Label();
            this._classicItems = new System.Windows.Forms.Label();
            this._memoryItems = new System.Windows.Forms.Label();
            this._maxItems = new System.Windows.Forms.Button();
            this._useLand = new System.Windows.Forms.CheckBox();
            this._titleLand = new System.Windows.Forms.Label();
            this._sizeLand = new System.Windows.Forms.NumericUpDown();
            this._nowLand = new System.Windows.Forms.Label();
            this._rangeLand = new System.Windows.Forms.Label();
            this._classicLand = new System.Windows.Forms.Label();
            this._memoryLand = new System.Windows.Forms.Label();
            this._maxLand = new System.Windows.Forms.Button();
            this._useTextures = new System.Windows.Forms.CheckBox();
            this._titleTextures = new System.Windows.Forms.Label();
            this._sizeTextures = new System.Windows.Forms.NumericUpDown();
            this._nowTextures = new System.Windows.Forms.Label();
            this._rangeTextures = new System.Windows.Forms.Label();
            this._classicTextures = new System.Windows.Forms.Label();
            this._memoryTextures = new System.Windows.Forms.Label();
            this._maxTextures = new System.Windows.Forms.Button();
            this._useHues = new System.Windows.Forms.CheckBox();
            this._titleHues = new System.Windows.Forms.Label();
            this._sizeHues = new System.Windows.Forms.NumericUpDown();
            this._nowHues = new System.Windows.Forms.Label();
            this._rangeHues = new System.Windows.Forms.Label();
            this._classicHues = new System.Windows.Forms.Label();
            this._memoryHues = new System.Windows.Forms.Label();
            this._maxHues = new System.Windows.Forms.Button();
            this._commonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._commonLabel = new System.Windows.Forms.Label();
            this._common = new System.Windows.Forms.NumericUpDown();
            this._applyButton = new System.Windows.Forms.Button();
            this._presetsLabel = new System.Windows.Forms.Label();
            this._presetClassic = new System.Windows.Forms.Button();
            this._preset64k = new System.Windows.Forms.Button();
            this._preset1m = new System.Windows.Forms.Button();
            this._presetMax = new System.Windows.Forms.Button();
            this._presetCurrent = new System.Windows.Forms.Button();
            this._totalLabel = new System.Windows.Forms.Label();
            this._actionGroup = new System.Windows.Forms.GroupBox();
            this._actionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            this._createButton = new System.Windows.Forms.Button();
            this._backupFirst = new System.Windows.Forms.CheckBox();
            this._saveRightAway = new System.Windows.Forms.CheckBox();
            this._saveAllButton = new System.Windows.Forms.Button();
            this._loadFolderButton = new System.Windows.Forms.Button();
            this._loadOutputButton = new System.Windows.Forms.Button();
            this._copyToClientButton = new System.Windows.Forms.Button();
            this._backupButton = new System.Windows.Forms.Button();
            this._openOutputButton = new System.Windows.Forms.Button();
            this._openUoButton = new System.Windows.Forms.Button();
            this._openBackupButton = new System.Windows.Forms.Button();
            this._detailsButton = new System.Windows.Forms.Button();
            this._statusLabel = new System.Windows.Forms.Label();
            this._bottomTabs = new System.Windows.Forms.TabControl();
            this._checkPage = new System.Windows.Forms.TabPage();
            this._patchPage = new System.Windows.Forms.TabPage();
            this._checkLayout = new System.Windows.Forms.TableLayoutPanel();
            this._checkTop = new System.Windows.Forms.FlowLayoutPanel();
            this._checkLabel = new System.Windows.Forms.Label();
            this._checkWhere = new System.Windows.Forms.ComboBox();
            this._runButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._showLabel = new System.Windows.Forms.Label();
            this._severityFilter = new System.Windows.Forms.ComboBox();
            this._checkStatus = new System.Windows.Forms.Label();
            this._issuesList = new System.Windows.Forms.ListView();
            this._colSeverity = new System.Windows.Forms.ColumnHeader();
            this._colArea = new System.Windows.Forms.ColumnHeader();
            this._colId = new System.Windows.Forms.ColumnHeader();
            this._colFile = new System.Windows.Forms.ColumnHeader();
            this._colProblem = new System.Windows.Forms.ColumnHeader();
            this._colRepair = new System.Windows.Forms.ColumnHeader();
            this._issuesMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._menuGoTo = new System.Windows.Forms.ToolStripMenuItem();
            this._menuRepair = new System.Windows.Forms.ToolStripMenuItem();
            this._menuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this._checkBottom = new System.Windows.Forms.FlowLayoutPanel();
            this._repairSelectedButton = new System.Windows.Forms.Button();
            this._repairAllButton = new System.Windows.Forms.Button();
            this._goToButton = new System.Windows.Forms.Button();
            this._copyReportButton = new System.Windows.Forms.Button();
            this._saveReportButton = new System.Windows.Forms.Button();
            this._checkHint = new System.Windows.Forms.Label();
            this._patchLayout = new System.Windows.Forms.TableLayoutPanel();
            this._patchTop = new System.Windows.Forms.FlowLayoutPanel();
            this._patchInfo = new System.Windows.Forms.Label();
            this._patchItems = new System.Windows.Forms.CheckBox();
            this._patchLand = new System.Windows.Forms.CheckBox();
            this._patchTextures = new System.Windows.Forms.CheckBox();
            this._patchHues = new System.Windows.Forms.CheckBox();
            this._patchLastSlot = new System.Windows.Forms.CheckBox();
            this._patchRunButton = new System.Windows.Forms.Button();
            this._patchUndoButton = new System.Windows.Forms.Button();
            this._patchOpenLogButton = new System.Windows.Forms.Button();
            this._patchCopyLogButton = new System.Windows.Forms.Button();
            this._patchStatus = new System.Windows.Forms.Label();
            this._patchList = new System.Windows.Forms.ListView();
            this._colPKind = new System.Windows.Forms.ColumnHeader();
            this._colPSource = new System.Windows.Forms.ColumnHeader();
            this._colPTarget = new System.Windows.Forms.ColumnHeader();
            this._colPMemory = new System.Windows.Forms.ColumnHeader();
            this._colPDisk = new System.Windows.Forms.ColumnHeader();
            this._colPReload = new System.Windows.Forms.ColumnHeader();
            this._colPResult = new System.Windows.Forms.ColumnHeader();
            this._patchBottom = new System.Windows.Forms.FlowLayoutPanel();
            this._patchGoGraphic = new System.Windows.Forms.Button();
            this._patchGoTileData = new System.Windows.Forms.Button();
            this._patchGoRadar = new System.Windows.Forms.Button();
            this._patchHint = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._sizeItems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeLand)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeTextures)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeHues)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._common)).BeginInit();
            this._rootLayout.SuspendLayout();
            this._sizeGroup.SuspendLayout();
            this._sizeOuter.SuspendLayout();
            this._sizeGrid.SuspendLayout();
            this._commonPanel.SuspendLayout();
            this._actionGroup.SuspendLayout();
            this._actionsFlow.SuspendLayout();
            this._bottomTabs.SuspendLayout();
            this._checkPage.SuspendLayout();
            this._patchPage.SuspendLayout();
            this._checkLayout.SuspendLayout();
            this._checkTop.SuspendLayout();
            this._issuesMenu.SuspendLayout();
            this._checkBottom.SuspendLayout();
            this._patchLayout.SuspendLayout();
            this._patchTop.SuspendLayout();
            this._patchBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _introLabel
            // 
            this._introLabel.AutoSize = true;
            this._introLabel.Text = "Extended mul files: the classic files (art.mul, tiledata.mul, texmaps.mul, hues.mul, radarcol.mul) stay unchanged for the old client. Each extension starts where the classic file ends and holds up to 16,777,216 entries for the new client. Choose the sizes, create everything with one click, check the files for errors and run a patch test.";
            this._introLabel.Name = "_introLabel";
            this._introLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._introLabel.MaximumSize = new System.Drawing.Size(1200, 0);
            this._introLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            // 
            // _head0
            // 
            this._head0.AutoSize = true;
            this._head0.Text = "Use";
            this._head0.Name = "_head0";
            this._head0.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head0.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head0.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head1
            // 
            this._head1.AutoSize = true;
            this._head1.Text = "Extension";
            this._head1.Name = "_head1";
            this._head1.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head1.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head2
            // 
            this._head2.AutoSize = true;
            this._head2.Text = "New size (slots)";
            this._head2.Name = "_head2";
            this._head2.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head2.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head3
            // 
            this._head3.AutoSize = true;
            this._head3.Text = "Now";
            this._head3.Name = "_head3";
            this._head3.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head3.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head4
            // 
            this._head4.AutoSize = true;
            this._head4.Text = "Ids with the new size";
            this._head4.Name = "_head4";
            this._head4.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head4.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head5
            // 
            this._head5.AutoSize = true;
            this._head5.Text = "Old client / new client";
            this._head5.Name = "_head5";
            this._head5.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head5.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head5.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _head6
            // 
            this._head6.AutoSize = true;
            this._head6.Text = "Memory";
            this._head6.Name = "_head6";
            this._head6.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._head6.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._head6.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            // 
            // _useItems
            // 
            this._useItems.AutoSize = true;
            this._useItems.Text = "";
            this._useItems.Name = "_useItems";
            this._useItems.Checked = true;
            this._useItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this._useItems.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._useItems.UseVisualStyleBackColor = true;
            this._useItems.CheckedChanged += new System.EventHandler(this.OnUseChanged);
            // 
            // _titleItems
            // 
            this._titleItems.AutoSize = true;
            this._titleItems.Text = "Items (art + tiledata)";
            this._titleItems.Name = "_titleItems";
            this._titleItems.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _sizeItems
            // 
            this._sizeItems.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            this._sizeItems.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._sizeItems.Value = new decimal(new int[] { 1048576, 0, 0, 0 });
            this._sizeItems.ThousandsSeparator = true;
            this._sizeItems.Width = 120;
            this._sizeItems.Name = "_sizeItems";
            this._sizeItems.ValueChanged += new System.EventHandler(this.OnSizeChanged);
            // 
            // _nowItems
            // 
            this._nowItems.AutoSize = true;
            this._nowItems.Text = "-";
            this._nowItems.Name = "_nowItems";
            this._nowItems.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._nowItems.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _rangeItems
            // 
            this._rangeItems.AutoSize = true;
            this._rangeItems.Text = "-";
            this._rangeItems.Name = "_rangeItems";
            this._rangeItems.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._rangeItems.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _classicItems
            // 
            this._classicItems.AutoSize = true;
            this._classicItems.Text = "-";
            this._classicItems.Name = "_classicItems";
            this._classicItems.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._classicItems.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _memoryItems
            // 
            this._memoryItems.AutoSize = true;
            this._memoryItems.Text = "-";
            this._memoryItems.Name = "_memoryItems";
            this._memoryItems.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._memoryItems.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _maxItems
            // 
            this._maxItems.AutoSize = true;
            this._maxItems.Text = "Max";
            this._maxItems.Name = "_maxItems";
            this._maxItems.UseVisualStyleBackColor = true;
            this._maxItems.Click += new System.EventHandler(this.OnMaxClick);
            // 
            // _useLand
            // 
            this._useLand.AutoSize = true;
            this._useLand.Text = "";
            this._useLand.Name = "_useLand";
            this._useLand.Checked = true;
            this._useLand.CheckState = System.Windows.Forms.CheckState.Checked;
            this._useLand.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._useLand.UseVisualStyleBackColor = true;
            this._useLand.CheckedChanged += new System.EventHandler(this.OnUseChanged);
            // 
            // _titleLand
            // 
            this._titleLand.AutoSize = true;
            this._titleLand.Text = "Land tiles (art + tiledata)";
            this._titleLand.Name = "_titleLand";
            this._titleLand.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _sizeLand
            // 
            this._sizeLand.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            this._sizeLand.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._sizeLand.Value = new decimal(new int[] { 1048576, 0, 0, 0 });
            this._sizeLand.ThousandsSeparator = true;
            this._sizeLand.Width = 120;
            this._sizeLand.Name = "_sizeLand";
            this._sizeLand.ValueChanged += new System.EventHandler(this.OnSizeChanged);
            // 
            // _nowLand
            // 
            this._nowLand.AutoSize = true;
            this._nowLand.Text = "-";
            this._nowLand.Name = "_nowLand";
            this._nowLand.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._nowLand.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _rangeLand
            // 
            this._rangeLand.AutoSize = true;
            this._rangeLand.Text = "-";
            this._rangeLand.Name = "_rangeLand";
            this._rangeLand.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._rangeLand.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _classicLand
            // 
            this._classicLand.AutoSize = true;
            this._classicLand.Text = "-";
            this._classicLand.Name = "_classicLand";
            this._classicLand.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._classicLand.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _memoryLand
            // 
            this._memoryLand.AutoSize = true;
            this._memoryLand.Text = "-";
            this._memoryLand.Name = "_memoryLand";
            this._memoryLand.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._memoryLand.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _maxLand
            // 
            this._maxLand.AutoSize = true;
            this._maxLand.Text = "Max";
            this._maxLand.Name = "_maxLand";
            this._maxLand.UseVisualStyleBackColor = true;
            this._maxLand.Click += new System.EventHandler(this.OnMaxClick);
            // 
            // _useTextures
            // 
            this._useTextures.AutoSize = true;
            this._useTextures.Text = "";
            this._useTextures.Name = "_useTextures";
            this._useTextures.Checked = true;
            this._useTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this._useTextures.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._useTextures.UseVisualStyleBackColor = true;
            this._useTextures.CheckedChanged += new System.EventHandler(this.OnUseChanged);
            // 
            // _titleTextures
            // 
            this._titleTextures.AutoSize = true;
            this._titleTextures.Text = "Textures";
            this._titleTextures.Name = "_titleTextures";
            this._titleTextures.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _sizeTextures
            // 
            this._sizeTextures.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            this._sizeTextures.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._sizeTextures.Value = new decimal(new int[] { 1048576, 0, 0, 0 });
            this._sizeTextures.ThousandsSeparator = true;
            this._sizeTextures.Width = 120;
            this._sizeTextures.Name = "_sizeTextures";
            this._sizeTextures.ValueChanged += new System.EventHandler(this.OnSizeChanged);
            // 
            // _nowTextures
            // 
            this._nowTextures.AutoSize = true;
            this._nowTextures.Text = "-";
            this._nowTextures.Name = "_nowTextures";
            this._nowTextures.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._nowTextures.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _rangeTextures
            // 
            this._rangeTextures.AutoSize = true;
            this._rangeTextures.Text = "-";
            this._rangeTextures.Name = "_rangeTextures";
            this._rangeTextures.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._rangeTextures.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _classicTextures
            // 
            this._classicTextures.AutoSize = true;
            this._classicTextures.Text = "-";
            this._classicTextures.Name = "_classicTextures";
            this._classicTextures.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._classicTextures.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _memoryTextures
            // 
            this._memoryTextures.AutoSize = true;
            this._memoryTextures.Text = "-";
            this._memoryTextures.Name = "_memoryTextures";
            this._memoryTextures.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._memoryTextures.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _maxTextures
            // 
            this._maxTextures.AutoSize = true;
            this._maxTextures.Text = "Max";
            this._maxTextures.Name = "_maxTextures";
            this._maxTextures.UseVisualStyleBackColor = true;
            this._maxTextures.Click += new System.EventHandler(this.OnMaxClick);
            // 
            // _useHues
            // 
            this._useHues.AutoSize = true;
            this._useHues.Text = "";
            this._useHues.Name = "_useHues";
            this._useHues.Checked = true;
            this._useHues.CheckState = System.Windows.Forms.CheckState.Checked;
            this._useHues.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._useHues.UseVisualStyleBackColor = true;
            this._useHues.CheckedChanged += new System.EventHandler(this.OnUseChanged);
            // 
            // _titleHues
            // 
            this._titleHues.AutoSize = true;
            this._titleHues.Text = "Hues";
            this._titleHues.Name = "_titleHues";
            this._titleHues.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _sizeHues
            // 
            this._sizeHues.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            this._sizeHues.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._sizeHues.Value = new decimal(new int[] { 1048576, 0, 0, 0 });
            this._sizeHues.ThousandsSeparator = true;
            this._sizeHues.Width = 120;
            this._sizeHues.Name = "_sizeHues";
            this._sizeHues.ValueChanged += new System.EventHandler(this.OnSizeChanged);
            // 
            // _nowHues
            // 
            this._nowHues.AutoSize = true;
            this._nowHues.Text = "-";
            this._nowHues.Name = "_nowHues";
            this._nowHues.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._nowHues.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _rangeHues
            // 
            this._rangeHues.AutoSize = true;
            this._rangeHues.Text = "-";
            this._rangeHues.Name = "_rangeHues";
            this._rangeHues.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._rangeHues.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _classicHues
            // 
            this._classicHues.AutoSize = true;
            this._classicHues.Text = "-";
            this._classicHues.Name = "_classicHues";
            this._classicHues.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._classicHues.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _memoryHues
            // 
            this._memoryHues.AutoSize = true;
            this._memoryHues.Text = "-";
            this._memoryHues.Name = "_memoryHues";
            this._memoryHues.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._memoryHues.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _maxHues
            // 
            this._maxHues.AutoSize = true;
            this._maxHues.Text = "Max";
            this._maxHues.Name = "_maxHues";
            this._maxHues.UseVisualStyleBackColor = true;
            this._maxHues.Click += new System.EventHandler(this.OnMaxClick);
            // 
            // _commonLabel
            // 
            this._commonLabel.AutoSize = true;
            this._commonLabel.Text = "Same size for all checked:";
            this._commonLabel.Name = "_commonLabel";
            this._commonLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _common
            // 
            this._common.Maximum = new decimal(new int[] { 16777216, 0, 0, 0 });
            this._common.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this._common.Value = new decimal(new int[] { 1048576, 0, 0, 0 });
            this._common.Increment = new decimal(new int[] { 65536, 0, 0, 0 });
            this._common.ThousandsSeparator = true;
            this._common.Width = 120;
            this._common.Name = "_common";
            // 
            // _applyButton
            // 
            this._applyButton.AutoSize = true;
            this._applyButton.Text = "Apply";
            this._applyButton.Name = "_applyButton";
            this._applyButton.UseVisualStyleBackColor = true;
            this._applyButton.Click += new System.EventHandler(this.OnApplyCommon);
            // 
            // _presetsLabel
            // 
            this._presetsLabel.AutoSize = true;
            this._presetsLabel.Text = "Presets:";
            this._presetsLabel.Name = "_presetsLabel";
            this._presetsLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._presetsLabel.Margin = new System.Windows.Forms.Padding(16, 7, 3, 3);
            // 
            // _presetClassic
            // 
            this._presetClassic.AutoSize = true;
            this._presetClassic.Text = "Old-client limits";
            this._presetClassic.Name = "_presetClassic";
            this._presetClassic.UseVisualStyleBackColor = true;
            this._presetClassic.Click += new System.EventHandler(this.OnPresetClassic);
            // 
            // _preset64k
            // 
            this._preset64k.AutoSize = true;
            this._preset64k.Text = "65,536";
            this._preset64k.Name = "_preset64k";
            this._preset64k.UseVisualStyleBackColor = true;
            this._preset64k.Click += new System.EventHandler(this.OnPreset64k);
            // 
            // _preset1m
            // 
            this._preset1m.AutoSize = true;
            this._preset1m.Text = "1,048,576";
            this._preset1m.Name = "_preset1m";
            this._preset1m.UseVisualStyleBackColor = true;
            this._preset1m.Click += new System.EventHandler(this.OnPreset1m);
            // 
            // _presetMax
            // 
            this._presetMax.AutoSize = true;
            this._presetMax.Text = "16,777,216 (max)";
            this._presetMax.Name = "_presetMax";
            this._presetMax.UseVisualStyleBackColor = true;
            this._presetMax.Click += new System.EventHandler(this.OnPresetMax);
            // 
            // _presetCurrent
            // 
            this._presetCurrent.AutoSize = true;
            this._presetCurrent.Text = "Current sizes";
            this._presetCurrent.Name = "_presetCurrent";
            this._presetCurrent.UseVisualStyleBackColor = true;
            this._presetCurrent.Click += new System.EventHandler(this.OnPresetCurrent);
            // 
            // _totalLabel
            // 
            this._totalLabel.AutoSize = true;
            this._totalLabel.Text = "";
            this._totalLabel.Name = "_totalLabel";
            this._totalLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._totalLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._totalLabel.Margin = new System.Windows.Forms.Padding(16, 7, 3, 3);
            // 
            // _createButton
            // 
            this._createButton.AutoSize = true;
            this._createButton.Text = "Create / resize checked  (one click)";
            this._createButton.Name = "_createButton";
            this._createButton.UseVisualStyleBackColor = true;
            this._createButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._createButton.BackColor = System.Drawing.Color.FromArgb(220, 240, 220);
            this._createButton.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this._createButton.Click += new System.EventHandler(this.OnCreateClick);
            // 
            // _backupFirst
            // 
            this._backupFirst.AutoSize = true;
            this._backupFirst.Text = "Backup first";
            this._backupFirst.Name = "_backupFirst";
            this._backupFirst.Checked = true;
            this._backupFirst.CheckState = System.Windows.Forms.CheckState.Checked;
            this._backupFirst.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._backupFirst.UseVisualStyleBackColor = true;
            // 
            // _saveRightAway
            // 
            this._saveRightAway.AutoSize = true;
            this._saveRightAway.Text = "Save the files right away";
            this._saveRightAway.Name = "_saveRightAway";
            this._saveRightAway.Checked = true;
            this._saveRightAway.CheckState = System.Windows.Forms.CheckState.Checked;
            this._saveRightAway.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._saveRightAway.UseVisualStyleBackColor = true;
            // 
            // _saveAllButton
            // 
            this._saveAllButton.AutoSize = true;
            this._saveAllButton.Text = "Save all extensions";
            this._saveAllButton.Name = "_saveAllButton";
            this._saveAllButton.UseVisualStyleBackColor = true;
            this._saveAllButton.Click += new System.EventHandler(this.OnSaveAllClick);
            // 
            // _loadFolderButton
            // 
            this._loadFolderButton.AutoSize = true;
            this._loadFolderButton.Text = "Load from folder...";
            this._loadFolderButton.Name = "_loadFolderButton";
            this._loadFolderButton.UseVisualStyleBackColor = true;
            this._loadFolderButton.Click += new System.EventHandler(this.OnLoadFolderClick);
            // 
            // _loadOutputButton
            // 
            this._loadOutputButton.AutoSize = true;
            this._loadOutputButton.Text = "Load from output folder";
            this._loadOutputButton.Name = "_loadOutputButton";
            this._loadOutputButton.UseVisualStyleBackColor = true;
            this._loadOutputButton.Click += new System.EventHandler(this.OnLoadOutputClick);
            // 
            // _copyToClientButton
            // 
            this._copyToClientButton.AutoSize = true;
            this._copyToClientButton.Text = "Copy output -> Ultima Online folder";
            this._copyToClientButton.Name = "_copyToClientButton";
            this._copyToClientButton.UseVisualStyleBackColor = true;
            this._copyToClientButton.Click += new System.EventHandler(this.OnCopyToClientClick);
            // 
            // _backupButton
            // 
            this._backupButton.AutoSize = true;
            this._backupButton.Text = "Backup now";
            this._backupButton.Name = "_backupButton";
            this._backupButton.UseVisualStyleBackColor = true;
            this._backupButton.Click += new System.EventHandler(this.OnBackupClick);
            // 
            // _openOutputButton
            // 
            this._openOutputButton.AutoSize = true;
            this._openOutputButton.Text = "Open output folder";
            this._openOutputButton.Name = "_openOutputButton";
            this._openOutputButton.UseVisualStyleBackColor = true;
            this._openOutputButton.Click += new System.EventHandler(this.OnOpenOutputClick);
            // 
            // _openUoButton
            // 
            this._openUoButton.AutoSize = true;
            this._openUoButton.Text = "Open UO folder";
            this._openUoButton.Name = "_openUoButton";
            this._openUoButton.UseVisualStyleBackColor = true;
            this._openUoButton.Click += new System.EventHandler(this.OnOpenUoClick);
            // 
            // _openBackupButton
            // 
            this._openBackupButton.AutoSize = true;
            this._openBackupButton.Text = "Open backup folder";
            this._openBackupButton.Name = "_openBackupButton";
            this._openBackupButton.UseVisualStyleBackColor = true;
            this._openBackupButton.Click += new System.EventHandler(this.OnOpenBackupClick);
            // 
            // _detailsButton
            // 
            this._detailsButton.AutoSize = true;
            this._detailsButton.Text = "Details window...";
            this._detailsButton.Name = "_detailsButton";
            this._detailsButton.UseVisualStyleBackColor = true;
            this._detailsButton.Click += new System.EventHandler(this.OnDetailsClick);
            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Text = "";
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._statusLabel.ForeColor = System.Drawing.Color.DimGray;
            this._statusLabel.MaximumSize = new System.Drawing.Size(1200, 0);
            // 
            // _checkLabel
            // 
            this._checkLabel.AutoSize = true;
            this._checkLabel.Text = "Check:";
            this._checkLabel.Name = "_checkLabel";
            this._checkLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _checkWhere
            // 
            this._checkWhere.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._checkWhere.Width = 420;
            this._checkWhere.Name = "_checkWhere";
            // 
            // _runButton
            // 
            this._runButton.AutoSize = true;
            this._runButton.Text = "Run check";
            this._runButton.Name = "_runButton";
            this._runButton.UseVisualStyleBackColor = true;
            this._runButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._runButton.Click += new System.EventHandler(this.OnRunCheckClick);
            // 
            // _cancelButton
            // 
            this._cancelButton.AutoSize = true;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Enabled = false;
            this._cancelButton.Click += new System.EventHandler(this.OnCancelCheckClick);
            // 
            // _showLabel
            // 
            this._showLabel.AutoSize = true;
            this._showLabel.Text = "Show:";
            this._showLabel.Name = "_showLabel";
            this._showLabel.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _severityFilter
            // 
            this._severityFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._severityFilter.Width = 140;
            this._severityFilter.Name = "_severityFilter";
            this._severityFilter.Items.AddRange(new object[] { "Everything", "Warnings + errors", "Errors only", "Repairable only" });
            this._severityFilter.SelectedIndexChanged += new System.EventHandler(this.OnSeverityFilterChanged);
            // 
            // _checkStatus
            // 
            this._checkStatus.AutoSize = true;
            this._checkStatus.Text = "";
            this._checkStatus.Name = "_checkStatus";
            this._checkStatus.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            // 
            // _colSeverity
            // 
            this._colSeverity.Text = "Severity";
            this._colSeverity.Width = 70;
            // 
            // _colArea
            // 
            this._colArea.Text = "Area";
            this._colArea.Width = 85;
            // 
            // _colId
            // 
            this._colId.Text = "Id";
            this._colId.Width = 90;
            // 
            // _colFile
            // 
            this._colFile.Text = "File";
            this._colFile.Width = 150;
            // 
            // _colProblem
            // 
            this._colProblem.Text = "Problem";
            this._colProblem.Width = 620;
            // 
            // _colRepair
            // 
            this._colRepair.Text = "Repair";
            this._colRepair.Width = 120;
            // 
            // _issuesList
            // 
            this._issuesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._issuesList.View = System.Windows.Forms.View.Details;
            this._issuesList.FullRowSelect = true;
            this._issuesList.HideSelection = false;
            this._issuesList.MultiSelect = true;
            this._issuesList.Name = "_issuesList";
            this._issuesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._colSeverity, this._colArea, this._colId, this._colFile, this._colProblem, this._colRepair });
            this._issuesList.DoubleClick += new System.EventHandler(this.OnIssueDoubleClick);
            // 
            // _menuGoTo
            // 
            this._menuGoTo.Text = "Go to (Items / LandTiles / TileData)";
            this._menuGoTo.Name = "_menuGoTo";
            this._menuGoTo.Click += new System.EventHandler(this.OnIssueDoubleClick);
            // 
            // _menuRepair
            // 
            this._menuRepair.Text = "Repair selected";
            this._menuRepair.Name = "_menuRepair";
            this._menuRepair.Click += new System.EventHandler(this.OnRepairSelectedClick);
            // 
            // _menuCopy
            // 
            this._menuCopy.Text = "Copy selected lines";
            this._menuCopy.Name = "_menuCopy";
            this._menuCopy.Click += new System.EventHandler(this.OnCopyIssuesClick);
            // 
            // _issuesMenu
            // 
            this._issuesMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._menuGoTo, this._menuRepair, this._menuCopy });
            this._issuesMenu.Name = "_issuesMenu";
            this._issuesList.ContextMenuStrip = this._issuesMenu;
            // 
            // _repairSelectedButton
            // 
            this._repairSelectedButton.AutoSize = true;
            this._repairSelectedButton.Text = "Repair selected";
            this._repairSelectedButton.Name = "_repairSelectedButton";
            this._repairSelectedButton.UseVisualStyleBackColor = true;
            this._repairSelectedButton.Click += new System.EventHandler(this.OnRepairSelectedClick);
            // 
            // _repairAllButton
            // 
            this._repairAllButton.AutoSize = true;
            this._repairAllButton.Text = "Repair all repairable";
            this._repairAllButton.Name = "_repairAllButton";
            this._repairAllButton.UseVisualStyleBackColor = true;
            this._repairAllButton.Click += new System.EventHandler(this.OnRepairAllClick);
            // 
            // _goToButton
            // 
            this._goToButton.AutoSize = true;
            this._goToButton.Text = "Go to";
            this._goToButton.Name = "_goToButton";
            this._goToButton.UseVisualStyleBackColor = true;
            this._goToButton.Click += new System.EventHandler(this.OnIssueDoubleClick);
            // 
            // _copyReportButton
            // 
            this._copyReportButton.AutoSize = true;
            this._copyReportButton.Text = "Copy report";
            this._copyReportButton.Name = "_copyReportButton";
            this._copyReportButton.UseVisualStyleBackColor = true;
            this._copyReportButton.Click += new System.EventHandler(this.OnCopyReportClick);
            // 
            // _saveReportButton
            // 
            this._saveReportButton.AutoSize = true;
            this._saveReportButton.Text = "Save report...";
            this._saveReportButton.Name = "_saveReportButton";
            this._saveReportButton.UseVisualStyleBackColor = true;
            this._saveReportButton.Click += new System.EventHandler(this.OnSaveReportClick);
            // 
            // _checkHint
            // 
            this._checkHint.AutoSize = true;
            this._checkHint.Text = "Repairs change the loaded data - save afterwards. Double click a line to jump to the id.";
            this._checkHint.Name = "_checkHint";
            this._checkHint.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._checkHint.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _patchInfo
            // 
            this._patchInfo.AutoSize = true;
            this._patchInfo.Text = "Copies a real classic entry into a free slot of every extension (and the last slot), checks it in memory, writes a test copy into <output>\\PatchTest\\<date> and reads it back from disk. The loaded files are not changed. Everything is logged in patchtest.log.";
            this._patchInfo.Name = "_patchInfo";
            this._patchInfo.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._patchInfo.MaximumSize = new System.Drawing.Size(1150, 0);
            this._patchInfo.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            // 
            // _patchItems
            // 
            this._patchItems.AutoSize = true;
            this._patchItems.Text = "Items";
            this._patchItems.Name = "_patchItems";
            this._patchItems.Checked = true;
            this._patchItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this._patchItems.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._patchItems.UseVisualStyleBackColor = true;
            // 
            // _patchLand
            // 
            this._patchLand.AutoSize = true;
            this._patchLand.Text = "Land tiles";
            this._patchLand.Name = "_patchLand";
            this._patchLand.Checked = true;
            this._patchLand.CheckState = System.Windows.Forms.CheckState.Checked;
            this._patchLand.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._patchLand.UseVisualStyleBackColor = true;
            // 
            // _patchTextures
            // 
            this._patchTextures.AutoSize = true;
            this._patchTextures.Text = "Textures";
            this._patchTextures.Name = "_patchTextures";
            this._patchTextures.Checked = true;
            this._patchTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this._patchTextures.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._patchTextures.UseVisualStyleBackColor = true;
            // 
            // _patchHues
            // 
            this._patchHues.AutoSize = true;
            this._patchHues.Text = "Hues";
            this._patchHues.Name = "_patchHues";
            this._patchHues.Checked = true;
            this._patchHues.CheckState = System.Windows.Forms.CheckState.Checked;
            this._patchHues.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._patchHues.UseVisualStyleBackColor = true;
            // 
            // _patchLastSlot
            // 
            this._patchLastSlot.AutoSize = true;
            this._patchLastSlot.Text = "Also the last slot";
            this._patchLastSlot.Name = "_patchLastSlot";
            this._patchLastSlot.Checked = true;
            this._patchLastSlot.CheckState = System.Windows.Forms.CheckState.Checked;
            this._patchLastSlot.Margin = new System.Windows.Forms.Padding(6, 7, 3, 3);
            this._patchLastSlot.UseVisualStyleBackColor = true;
            // 
            // _patchRunButton
            // 
            this._patchRunButton.AutoSize = true;
            this._patchRunButton.Text = "Run patch test";
            this._patchRunButton.Name = "_patchRunButton";
            this._patchRunButton.UseVisualStyleBackColor = true;
            this._patchRunButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._patchRunButton.BackColor = System.Drawing.Color.FromArgb(220, 230, 250);
            this._patchRunButton.Click += new System.EventHandler(this.OnPatchRunClick);
            // 
            // _patchUndoButton
            // 
            this._patchUndoButton.AutoSize = true;
            this._patchUndoButton.Text = "Remove test entries again";
            this._patchUndoButton.Name = "_patchUndoButton";
            this._patchUndoButton.UseVisualStyleBackColor = true;
            this._patchUndoButton.Enabled = false;
            this._patchUndoButton.Click += new System.EventHandler(this.OnPatchUndoClick);
            // 
            // _patchOpenLogButton
            // 
            this._patchOpenLogButton.AutoSize = true;
            this._patchOpenLogButton.Text = "Open log";
            this._patchOpenLogButton.Name = "_patchOpenLogButton";
            this._patchOpenLogButton.UseVisualStyleBackColor = true;
            this._patchOpenLogButton.Enabled = false;
            this._patchOpenLogButton.Click += new System.EventHandler(this.OnPatchOpenLogClick);
            // 
            // _patchCopyLogButton
            // 
            this._patchCopyLogButton.AutoSize = true;
            this._patchCopyLogButton.Text = "Copy log";
            this._patchCopyLogButton.Name = "_patchCopyLogButton";
            this._patchCopyLogButton.UseVisualStyleBackColor = true;
            this._patchCopyLogButton.Enabled = false;
            this._patchCopyLogButton.Click += new System.EventHandler(this.OnPatchCopyLogClick);
            // 
            // _patchStatus
            // 
            this._patchStatus.AutoSize = true;
            this._patchStatus.Text = "";
            this._patchStatus.Name = "_patchStatus";
            this._patchStatus.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._patchStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // _colPKind
            // 
            this._colPKind.Text = "Extension";
            this._colPKind.Width = 110;
            // 
            // _colPSource
            // 
            this._colPSource.Text = "Copied from";
            this._colPSource.Width = 100;
            // 
            // _colPTarget
            // 
            this._colPTarget.Text = "Patched into";
            this._colPTarget.Width = 110;
            // 
            // _colPMemory
            // 
            this._colPMemory.Text = "Memory";
            this._colPMemory.Width = 70;
            // 
            // _colPDisk
            // 
            this._colPDisk.Text = "Disk";
            this._colPDisk.Width = 70;
            // 
            // _colPReload
            // 
            this._colPReload.Text = "Read back";
            this._colPReload.Width = 80;
            // 
            // _colPResult
            // 
            this._colPResult.Text = "Result";
            this._colPResult.Width = 520;
            // 
            // _patchList
            // 
            this._patchList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._patchList.View = System.Windows.Forms.View.Details;
            this._patchList.FullRowSelect = true;
            this._patchList.HideSelection = false;
            this._patchList.MultiSelect = true;
            this._patchList.Name = "_patchList";
            this._patchList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._colPKind, this._colPSource, this._colPTarget, this._colPMemory, this._colPDisk, this._colPReload, this._colPResult });
            this._patchList.DoubleClick += new System.EventHandler(this.OnPatchGoGraphicClick);
            // 
            // _patchGoGraphic
            // 
            this._patchGoGraphic.AutoSize = true;
            this._patchGoGraphic.Text = "Show graphic / hue";
            this._patchGoGraphic.Name = "_patchGoGraphic";
            this._patchGoGraphic.UseVisualStyleBackColor = true;
            this._patchGoGraphic.Click += new System.EventHandler(this.OnPatchGoGraphicClick);
            // 
            // _patchGoTileData
            // 
            this._patchGoTileData.AutoSize = true;
            this._patchGoTileData.Text = "Show in TileData";
            this._patchGoTileData.Name = "_patchGoTileData";
            this._patchGoTileData.UseVisualStyleBackColor = true;
            this._patchGoTileData.Click += new System.EventHandler(this.OnPatchGoTileDataClick);
            // 
            // _patchGoRadar
            // 
            this._patchGoRadar.AutoSize = true;
            this._patchGoRadar.Text = "Show in RadarColor";
            this._patchGoRadar.Name = "_patchGoRadar";
            this._patchGoRadar.UseVisualStyleBackColor = true;
            this._patchGoRadar.Click += new System.EventHandler(this.OnPatchGoRadarClick);
            // 
            // _patchHint
            // 
            this._patchHint.AutoSize = true;
            this._patchHint.Text = "Click: the entry is marked in its tabs (graphic / hue, TileData, RadarColor). Double click: open the tab.";
            this._patchHint.Name = "_patchHint";
            this._patchHint.Margin = new System.Windows.Forms.Padding(3, 7, 12, 3);
            this._patchHint.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _rootLayout
            // 
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.RowCount = 4;
            this._rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.Controls.Add(this._introLabel, 0, 0);
            this._rootLayout.Controls.Add(this._sizeGroup, 0, 1);
            this._rootLayout.Controls.Add(this._actionGroup, 0, 2);
            this._rootLayout.Controls.Add(this._bottomTabs, 0, 3);
            // 
            // _sizeGroup
            // 
            this._sizeGroup.Text = "1. Sizes";
            this._sizeGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sizeGroup.AutoSize = true;
            this._sizeGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._sizeGroup.Name = "_sizeGroup";
            this._sizeGroup.Controls.Add(this._sizeOuter);
            // 
            // _sizeOuter
            // 
            this._sizeOuter.ColumnCount = 1;
            this._sizeOuter.RowCount = 2;
            this._sizeOuter.Dock = System.Windows.Forms.DockStyle.Top;
            this._sizeOuter.AutoSize = true;
            this._sizeOuter.Name = "_sizeOuter";
            this._sizeOuter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeOuter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeOuter.Controls.Add(this._sizeGrid, 0, 0);
            this._sizeOuter.Controls.Add(this._commonPanel, 0, 1);
            // 
            // _sizeGrid
            // 
            this._sizeGrid.ColumnCount = 8;
            this._sizeGrid.RowCount = 5;
            this._sizeGrid.AutoSize = true;
            this._sizeGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this._sizeGrid.Name = "_sizeGrid";
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._sizeGrid.Controls.Add(this._head0, 0, 0);
            this._sizeGrid.Controls.Add(this._head1, 1, 0);
            this._sizeGrid.Controls.Add(this._head2, 2, 0);
            this._sizeGrid.Controls.Add(this._head3, 3, 0);
            this._sizeGrid.Controls.Add(this._head4, 4, 0);
            this._sizeGrid.Controls.Add(this._head5, 5, 0);
            this._sizeGrid.Controls.Add(this._head6, 6, 0);
            this._sizeGrid.Controls.Add(this._useItems, 0, 1);
            this._sizeGrid.Controls.Add(this._titleItems, 1, 1);
            this._sizeGrid.Controls.Add(this._sizeItems, 2, 1);
            this._sizeGrid.Controls.Add(this._nowItems, 3, 1);
            this._sizeGrid.Controls.Add(this._rangeItems, 4, 1);
            this._sizeGrid.Controls.Add(this._classicItems, 5, 1);
            this._sizeGrid.Controls.Add(this._memoryItems, 6, 1);
            this._sizeGrid.Controls.Add(this._maxItems, 7, 1);
            this._sizeGrid.Controls.Add(this._useLand, 0, 2);
            this._sizeGrid.Controls.Add(this._titleLand, 1, 2);
            this._sizeGrid.Controls.Add(this._sizeLand, 2, 2);
            this._sizeGrid.Controls.Add(this._nowLand, 3, 2);
            this._sizeGrid.Controls.Add(this._rangeLand, 4, 2);
            this._sizeGrid.Controls.Add(this._classicLand, 5, 2);
            this._sizeGrid.Controls.Add(this._memoryLand, 6, 2);
            this._sizeGrid.Controls.Add(this._maxLand, 7, 2);
            this._sizeGrid.Controls.Add(this._useTextures, 0, 3);
            this._sizeGrid.Controls.Add(this._titleTextures, 1, 3);
            this._sizeGrid.Controls.Add(this._sizeTextures, 2, 3);
            this._sizeGrid.Controls.Add(this._nowTextures, 3, 3);
            this._sizeGrid.Controls.Add(this._rangeTextures, 4, 3);
            this._sizeGrid.Controls.Add(this._classicTextures, 5, 3);
            this._sizeGrid.Controls.Add(this._memoryTextures, 6, 3);
            this._sizeGrid.Controls.Add(this._maxTextures, 7, 3);
            this._sizeGrid.Controls.Add(this._useHues, 0, 4);
            this._sizeGrid.Controls.Add(this._titleHues, 1, 4);
            this._sizeGrid.Controls.Add(this._sizeHues, 2, 4);
            this._sizeGrid.Controls.Add(this._nowHues, 3, 4);
            this._sizeGrid.Controls.Add(this._rangeHues, 4, 4);
            this._sizeGrid.Controls.Add(this._classicHues, 5, 4);
            this._sizeGrid.Controls.Add(this._memoryHues, 6, 4);
            this._sizeGrid.Controls.Add(this._maxHues, 7, 4);
            // 
            // _commonPanel
            // 
            this._commonPanel.AutoSize = true;
            this._commonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._commonPanel.WrapContents = true;
            this._commonPanel.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this._commonPanel.Name = "_commonPanel";
            this._commonPanel.Controls.Add(this._commonLabel);
            this._commonPanel.Controls.Add(this._common);
            this._commonPanel.Controls.Add(this._applyButton);
            this._commonPanel.Controls.Add(this._presetsLabel);
            this._commonPanel.Controls.Add(this._presetClassic);
            this._commonPanel.Controls.Add(this._preset64k);
            this._commonPanel.Controls.Add(this._preset1m);
            this._commonPanel.Controls.Add(this._presetMax);
            this._commonPanel.Controls.Add(this._presetCurrent);
            this._commonPanel.Controls.Add(this._totalLabel);
            // 
            // _actionGroup
            // 
            this._actionGroup.Text = "2. Create / save / load";
            this._actionGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._actionGroup.AutoSize = true;
            this._actionGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._actionGroup.Name = "_actionGroup";
            this._actionGroup.Controls.Add(this._actionsFlow);
            // 
            // _actionsFlow
            // 
            this._actionsFlow.AutoSize = true;
            this._actionsFlow.Dock = System.Windows.Forms.DockStyle.Top;
            this._actionsFlow.WrapContents = true;
            this._actionsFlow.Name = "_actionsFlow";
            this._actionsFlow.Controls.Add(this._createButton);
            this._actionsFlow.Controls.Add(this._backupFirst);
            this._actionsFlow.Controls.Add(this._saveRightAway);
            this._actionsFlow.Controls.Add(this._saveAllButton);
            this._actionsFlow.Controls.Add(this._loadFolderButton);
            this._actionsFlow.Controls.Add(this._loadOutputButton);
            this._actionsFlow.Controls.Add(this._copyToClientButton);
            this._actionsFlow.Controls.Add(this._backupButton);
            this._actionsFlow.Controls.Add(this._openOutputButton);
            this._actionsFlow.Controls.Add(this._openUoButton);
            this._actionsFlow.Controls.Add(this._openBackupButton);
            this._actionsFlow.Controls.Add(this._detailsButton);
            this._actionsFlow.Controls.Add(this._statusLabel);
            this._actionsFlow.SetFlowBreak(this._saveRightAway, true);
            this._actionsFlow.SetFlowBreak(this._detailsButton, true);
            // 
            // _bottomTabs
            // 
            this._bottomTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._bottomTabs.MinimumSize = new System.Drawing.Size(0, 260);
            this._bottomTabs.Name = "_bottomTabs";
            this._bottomTabs.SelectedIndex = 0;
            this._bottomTabs.Controls.Add(this._checkPage);
            this._bottomTabs.Controls.Add(this._patchPage);
            // 
            // _checkPage
            // 
            this._checkPage.Text = "3. Error search";
            this._checkPage.Name = "_checkPage";
            this._checkPage.UseVisualStyleBackColor = true;
            this._checkPage.Padding = new System.Windows.Forms.Padding(3);
            this._checkPage.Controls.Add(this._checkLayout);
            // 
            // _patchPage
            // 
            this._patchPage.Text = "4. Patch test";
            this._patchPage.Name = "_patchPage";
            this._patchPage.UseVisualStyleBackColor = true;
            this._patchPage.Padding = new System.Windows.Forms.Padding(3);
            this._patchPage.Controls.Add(this._patchLayout);
            // 
            // _checkLayout
            // 
            this._checkLayout.ColumnCount = 1;
            this._checkLayout.RowCount = 3;
            this._checkLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._checkLayout.Name = "_checkLayout";
            this._checkLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._checkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._checkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._checkLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._checkLayout.Controls.Add(this._checkTop, 0, 0);
            this._checkLayout.Controls.Add(this._issuesList, 0, 1);
            this._checkLayout.Controls.Add(this._checkBottom, 0, 2);
            // 
            // _patchLayout
            // 
            this._patchLayout.ColumnCount = 1;
            this._patchLayout.RowCount = 3;
            this._patchLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._patchLayout.Name = "_patchLayout";
            this._patchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._patchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._patchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._patchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._patchLayout.Controls.Add(this._patchTop, 0, 0);
            this._patchLayout.Controls.Add(this._patchList, 0, 1);
            this._patchLayout.Controls.Add(this._patchBottom, 0, 2);
            // 
            // _checkTop
            // 
            this._checkTop.AutoSize = true;
            this._checkTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._checkTop.WrapContents = true;
            this._checkTop.Name = "_checkTop";
            // 
            // _checkBottom
            // 
            this._checkBottom.AutoSize = true;
            this._checkBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this._checkBottom.WrapContents = true;
            this._checkBottom.Name = "_checkBottom";
            // 
            // _patchTop
            // 
            this._patchTop.AutoSize = true;
            this._patchTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._patchTop.WrapContents = true;
            this._patchTop.Name = "_patchTop";
            // 
            // _patchBottom
            // 
            this._patchBottom.AutoSize = true;
            this._patchBottom.Dock = System.Windows.Forms.DockStyle.Top;
            this._patchBottom.WrapContents = true;
            this._patchBottom.Name = "_patchBottom";
            this._checkTop.Controls.Add(this._checkLabel);
            this._checkTop.Controls.Add(this._checkWhere);
            this._checkTop.Controls.Add(this._runButton);
            this._checkTop.Controls.Add(this._cancelButton);
            this._checkTop.Controls.Add(this._showLabel);
            this._checkTop.Controls.Add(this._severityFilter);
            this._checkTop.Controls.Add(this._checkStatus);
            this._checkBottom.Controls.Add(this._repairSelectedButton);
            this._checkBottom.Controls.Add(this._repairAllButton);
            this._checkBottom.Controls.Add(this._goToButton);
            this._checkBottom.Controls.Add(this._copyReportButton);
            this._checkBottom.Controls.Add(this._saveReportButton);
            this._checkBottom.Controls.Add(this._checkHint);
            this._patchTop.Controls.Add(this._patchInfo);
            this._patchTop.Controls.Add(this._patchItems);
            this._patchTop.Controls.Add(this._patchLand);
            this._patchTop.Controls.Add(this._patchTextures);
            this._patchTop.Controls.Add(this._patchHues);
            this._patchTop.Controls.Add(this._patchLastSlot);
            this._patchTop.Controls.Add(this._patchRunButton);
            this._patchTop.Controls.Add(this._patchUndoButton);
            this._patchTop.Controls.Add(this._patchOpenLogButton);
            this._patchTop.Controls.Add(this._patchCopyLogButton);
            this._patchTop.Controls.Add(this._patchStatus);
            this._patchTop.SetFlowBreak(this._patchInfo, true);
            this._patchList.SelectedIndexChanged += new System.EventHandler(this.OnPatchSelectionChanged);
            this._patchList.MultiSelect = false;
            this._patchBottom.Controls.Add(this._patchGoGraphic);
            this._patchBottom.Controls.Add(this._patchGoTileData);
            this._patchBottom.Controls.Add(this._patchGoRadar);
            this._patchBottom.Controls.Add(this._patchHint);
            // 
            // ExtendedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this._rootLayout);
            this.Name = "ExtendedControl";
            this.Size = new System.Drawing.Size(1200, 760);
            ((System.ComponentModel.ISupportInitialize)(this._sizeItems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeLand)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeTextures)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._sizeHues)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._common)).EndInit();
            this._patchBottom.ResumeLayout(false);
            this._patchBottom.PerformLayout();
            this._patchTop.ResumeLayout(false);
            this._patchTop.PerformLayout();
            this._patchLayout.ResumeLayout(false);
            this._patchLayout.PerformLayout();
            this._checkBottom.ResumeLayout(false);
            this._checkBottom.PerformLayout();
            this._issuesMenu.ResumeLayout(false);
            this._checkTop.ResumeLayout(false);
            this._checkTop.PerformLayout();
            this._checkLayout.ResumeLayout(false);
            this._checkLayout.PerformLayout();
            this._patchPage.ResumeLayout(false);
            this._patchPage.PerformLayout();
            this._checkPage.ResumeLayout(false);
            this._checkPage.PerformLayout();
            this._bottomTabs.ResumeLayout(false);
            this._actionsFlow.ResumeLayout(false);
            this._actionsFlow.PerformLayout();
            this._actionGroup.ResumeLayout(false);
            this._actionGroup.PerformLayout();
            this._commonPanel.ResumeLayout(false);
            this._commonPanel.PerformLayout();
            this._sizeGrid.ResumeLayout(false);
            this._sizeGrid.PerformLayout();
            this._sizeOuter.ResumeLayout(false);
            this._sizeOuter.PerformLayout();
            this._sizeGroup.ResumeLayout(false);
            this._sizeGroup.PerformLayout();
            this._rootLayout.ResumeLayout(false);
            this._rootLayout.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip _tips;
        private System.Windows.Forms.TableLayoutPanel _rootLayout;
        private System.Windows.Forms.Label _introLabel;
        private System.Windows.Forms.GroupBox _sizeGroup;
        private System.Windows.Forms.TableLayoutPanel _sizeOuter;
        private System.Windows.Forms.TableLayoutPanel _sizeGrid;
        private System.Windows.Forms.Label _head0;
        private System.Windows.Forms.Label _head1;
        private System.Windows.Forms.Label _head2;
        private System.Windows.Forms.Label _head3;
        private System.Windows.Forms.Label _head4;
        private System.Windows.Forms.Label _head5;
        private System.Windows.Forms.Label _head6;
        private System.Windows.Forms.CheckBox _useItems;
        private System.Windows.Forms.Label _titleItems;
        private System.Windows.Forms.NumericUpDown _sizeItems;
        private System.Windows.Forms.Label _nowItems;
        private System.Windows.Forms.Label _rangeItems;
        private System.Windows.Forms.Label _classicItems;
        private System.Windows.Forms.Label _memoryItems;
        private System.Windows.Forms.Button _maxItems;
        private System.Windows.Forms.CheckBox _useLand;
        private System.Windows.Forms.Label _titleLand;
        private System.Windows.Forms.NumericUpDown _sizeLand;
        private System.Windows.Forms.Label _nowLand;
        private System.Windows.Forms.Label _rangeLand;
        private System.Windows.Forms.Label _classicLand;
        private System.Windows.Forms.Label _memoryLand;
        private System.Windows.Forms.Button _maxLand;
        private System.Windows.Forms.CheckBox _useTextures;
        private System.Windows.Forms.Label _titleTextures;
        private System.Windows.Forms.NumericUpDown _sizeTextures;
        private System.Windows.Forms.Label _nowTextures;
        private System.Windows.Forms.Label _rangeTextures;
        private System.Windows.Forms.Label _classicTextures;
        private System.Windows.Forms.Label _memoryTextures;
        private System.Windows.Forms.Button _maxTextures;
        private System.Windows.Forms.CheckBox _useHues;
        private System.Windows.Forms.Label _titleHues;
        private System.Windows.Forms.NumericUpDown _sizeHues;
        private System.Windows.Forms.Label _nowHues;
        private System.Windows.Forms.Label _rangeHues;
        private System.Windows.Forms.Label _classicHues;
        private System.Windows.Forms.Label _memoryHues;
        private System.Windows.Forms.Button _maxHues;
        private System.Windows.Forms.FlowLayoutPanel _commonPanel;
        private System.Windows.Forms.Label _commonLabel;
        private System.Windows.Forms.NumericUpDown _common;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.Label _presetsLabel;
        private System.Windows.Forms.Button _presetClassic;
        private System.Windows.Forms.Button _preset64k;
        private System.Windows.Forms.Button _preset1m;
        private System.Windows.Forms.Button _presetMax;
        private System.Windows.Forms.Button _presetCurrent;
        private System.Windows.Forms.Label _totalLabel;
        private System.Windows.Forms.GroupBox _actionGroup;
        private System.Windows.Forms.FlowLayoutPanel _actionsFlow;
        private System.Windows.Forms.Button _createButton;
        private System.Windows.Forms.CheckBox _backupFirst;
        private System.Windows.Forms.CheckBox _saveRightAway;
        private System.Windows.Forms.Button _saveAllButton;
        private System.Windows.Forms.Button _loadFolderButton;
        private System.Windows.Forms.Button _loadOutputButton;
        private System.Windows.Forms.Button _copyToClientButton;
        private System.Windows.Forms.Button _backupButton;
        private System.Windows.Forms.Button _openOutputButton;
        private System.Windows.Forms.Button _openUoButton;
        private System.Windows.Forms.Button _openBackupButton;
        private System.Windows.Forms.Button _detailsButton;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.TabControl _bottomTabs;
        private System.Windows.Forms.TabPage _checkPage;
        private System.Windows.Forms.TabPage _patchPage;
        private System.Windows.Forms.TableLayoutPanel _checkLayout;
        private System.Windows.Forms.FlowLayoutPanel _checkTop;
        private System.Windows.Forms.Label _checkLabel;
        private System.Windows.Forms.ComboBox _checkWhere;
        private System.Windows.Forms.Button _runButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _showLabel;
        private System.Windows.Forms.ComboBox _severityFilter;
        private System.Windows.Forms.Label _checkStatus;
        private System.Windows.Forms.ListView _issuesList;
        private System.Windows.Forms.ColumnHeader _colSeverity;
        private System.Windows.Forms.ColumnHeader _colArea;
        private System.Windows.Forms.ColumnHeader _colId;
        private System.Windows.Forms.ColumnHeader _colFile;
        private System.Windows.Forms.ColumnHeader _colProblem;
        private System.Windows.Forms.ColumnHeader _colRepair;
        private System.Windows.Forms.ContextMenuStrip _issuesMenu;
        private System.Windows.Forms.ToolStripMenuItem _menuGoTo;
        private System.Windows.Forms.ToolStripMenuItem _menuRepair;
        private System.Windows.Forms.ToolStripMenuItem _menuCopy;
        private System.Windows.Forms.FlowLayoutPanel _checkBottom;
        private System.Windows.Forms.Button _repairSelectedButton;
        private System.Windows.Forms.Button _repairAllButton;
        private System.Windows.Forms.Button _goToButton;
        private System.Windows.Forms.Button _copyReportButton;
        private System.Windows.Forms.Button _saveReportButton;
        private System.Windows.Forms.Label _checkHint;
        private System.Windows.Forms.TableLayoutPanel _patchLayout;
        private System.Windows.Forms.FlowLayoutPanel _patchTop;
        private System.Windows.Forms.Label _patchInfo;
        private System.Windows.Forms.CheckBox _patchItems;
        private System.Windows.Forms.CheckBox _patchLand;
        private System.Windows.Forms.CheckBox _patchTextures;
        private System.Windows.Forms.CheckBox _patchHues;
        private System.Windows.Forms.CheckBox _patchLastSlot;
        private System.Windows.Forms.Button _patchRunButton;
        private System.Windows.Forms.Button _patchUndoButton;
        private System.Windows.Forms.Button _patchOpenLogButton;
        private System.Windows.Forms.Button _patchCopyLogButton;
        private System.Windows.Forms.Label _patchStatus;
        private System.Windows.Forms.ListView _patchList;
        private System.Windows.Forms.ColumnHeader _colPKind;
        private System.Windows.Forms.ColumnHeader _colPSource;
        private System.Windows.Forms.ColumnHeader _colPTarget;
        private System.Windows.Forms.ColumnHeader _colPMemory;
        private System.Windows.Forms.ColumnHeader _colPDisk;
        private System.Windows.Forms.ColumnHeader _colPReload;
        private System.Windows.Forms.ColumnHeader _colPResult;
        private System.Windows.Forms.FlowLayoutPanel _patchBottom;
        private System.Windows.Forms.Button _patchGoGraphic;
        private System.Windows.Forms.Button _patchGoTileData;
        private System.Windows.Forms.Button _patchGoRadar;
        private System.Windows.Forms.Label _patchHint;
    }
}

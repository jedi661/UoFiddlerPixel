/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Forms
{
    partial class PathSettingsForm
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
                DisposeImages();
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
            this._dirLabel = new System.Windows.Forms.Label();
            this._dirBox = new System.Windows.Forms.ComboBox();
            this._browseButton = new System.Windows.Forms.Button();
            this._applyButton = new System.Windows.Forms.Button();
            this._registryButton = new System.Windows.Forms.Button();
            this._openDirButton = new System.Windows.Forms.Button();
            this._favLabel = new System.Windows.Forms.Label();
            this._favNameBox = new System.Windows.Forms.TextBox();
            this._saveFavButton = new System.Windows.Forms.Button();
            this._removeFavButton = new System.Windows.Forms.Button();
            this._resetCheck = new System.Windows.Forms.CheckBox();
            this._clientInfo = new System.Windows.Forms.Label();
            this._dirFlow = new System.Windows.Forms.FlowLayoutPanel();
            this._dirGroup = new System.Windows.Forms.GroupBox();
            this._extList = new System.Windows.Forms.ListView();
            this._xcFile = new System.Windows.Forms.ColumnHeader();
            this._xcKind = new System.Windows.Forms.ColumnHeader();
            this._xcState = new System.Windows.Forms.ColumnHeader();
            this._xcFolder = new System.Windows.Forms.ColumnHeader();
            this._xcSize = new System.Windows.Forms.ColumnHeader();
            this._xcSlots = new System.Windows.Forms.ColumnHeader();
            this._extSearchButton = new System.Windows.Forms.Button();
            this._extFolderButton = new System.Windows.Forms.Button();
            this._extOutputButton = new System.Windows.Forms.Button();
            this._extTabButton = new System.Windows.Forms.Button();
            this._extStatus = new System.Windows.Forms.Label();
            this._extButtons = new System.Windows.Forms.FlowLayoutPanel();
            this._extLayout = new System.Windows.Forms.TableLayoutPanel();
            this._extGroup = new System.Windows.Forms.GroupBox();
            this._filterLabel = new System.Windows.Forms.Label();
            this._filterBox = new System.Windows.Forms.TextBox();
            this._showLabel = new System.Windows.Forms.Label();
            this._showBox = new System.Windows.Forms.ComboBox();
            this._countLabel = new System.Windows.Forms.Label();
            this._filesTop = new System.Windows.Forms.FlowLayoutPanel();
            this._fileList = new System.Windows.Forms.ListView();
            this._fcFile = new System.Windows.Forms.ColumnHeader();
            this._fcState = new System.Windows.Forms.ColumnHeader();
            this._fcPath = new System.Windows.Forms.ColumnHeader();
            this._fcSize = new System.Windows.Forms.ColumnHeader();
            this._fcDate = new System.Windows.Forms.ColumnHeader();
            this._fileMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._menuChoose = new System.Windows.Forms.ToolStripMenuItem();
            this._menuFromFolder = new System.Windows.Forms.ToolStripMenuItem();
            this._menuClear = new System.Windows.Forms.ToolStripMenuItem();
            this._menuOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this._menuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this._filesLayout = new System.Windows.Forms.TableLayoutPanel();
            this._filesGroup = new System.Windows.Forms.GroupBox();
            this._reloadButton = new System.Windows.Forms.Button();
            this._backupButton = new System.Windows.Forms.Button();
            this._closeButton = new System.Windows.Forms.Button();
            this._statusLabel = new System.Windows.Forms.Label();
            this._bottomFlow = new System.Windows.Forms.FlowLayoutPanel();
            this._rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this._tips = new System.Windows.Forms.ToolTip(this.components);
            this._dirFlow.SuspendLayout();
            this._dirGroup.SuspendLayout();
            this._extButtons.SuspendLayout();
            this._extLayout.SuspendLayout();
            this._extGroup.SuspendLayout();
            this._filesTop.SuspendLayout();
            this._fileMenu.SuspendLayout();
            this._filesLayout.SuspendLayout();
            this._filesGroup.SuspendLayout();
            this._bottomFlow.SuspendLayout();
            this._rootLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // _dirLabel
            // 
            this._dirLabel.AutoSize = true;
            this._dirLabel.Name = "_dirLabel";
            this._dirLabel.Text = "Folder:";
            this._dirLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _dirBox
            // 
            this._dirBox.Name = "_dirBox";
            this._dirBox.Width = 560;
            this._dirBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._dirBox.Items.AddRange(new object[] {  });
            this._dirBox.SelectedIndexChanged += new System.EventHandler(this.OnDirSelected);
            this._dirBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this._dirBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnDirKeyDown);
            // 
            // _browseButton
            // 
            this._browseButton.AutoSize = true;
            this._browseButton.Name = "_browseButton";
            this._browseButton.Text = "Browse...";
            this._browseButton.UseVisualStyleBackColor = true;
            this._browseButton.Click += new System.EventHandler(this.OnBrowse);
            // 
            // _applyButton
            // 
            this._applyButton.AutoSize = true;
            this._applyButton.Name = "_applyButton";
            this._applyButton.Text = "Use this folder";
            this._applyButton.UseVisualStyleBackColor = true;
            this._applyButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._applyButton.BackColor = System.Drawing.Color.FromArgb(220, 240, 220);
            this._applyButton.Click += new System.EventHandler(this.OnApply);
            // 
            // _registryButton
            // 
            this._registryButton.AutoSize = true;
            this._registryButton.Name = "_registryButton";
            this._registryButton.Text = "Detect (registry)";
            this._registryButton.UseVisualStyleBackColor = true;
            this._registryButton.Click += new System.EventHandler(this.OnDetectRegistry);
            // 
            // _openDirButton
            // 
            this._openDirButton.AutoSize = true;
            this._openDirButton.Name = "_openDirButton";
            this._openDirButton.Text = "Open";
            this._openDirButton.UseVisualStyleBackColor = true;
            this._openDirButton.Click += new System.EventHandler(this.OnOpenDir);
            // 
            // _favLabel
            // 
            this._favLabel.AutoSize = true;
            this._favLabel.Name = "_favLabel";
            this._favLabel.Text = "Quick list - name:";
            this._favLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _favNameBox
            // 
            this._favNameBox.Name = "_favNameBox";
            this._favNameBox.Width = 200;
            this._favNameBox.Text = "";
            // 
            // _saveFavButton
            // 
            this._saveFavButton.AutoSize = true;
            this._saveFavButton.Name = "_saveFavButton";
            this._saveFavButton.Text = "Save to quick list";
            this._saveFavButton.UseVisualStyleBackColor = true;
            this._saveFavButton.Click += new System.EventHandler(this.OnSaveFavourite);
            // 
            // _removeFavButton
            // 
            this._removeFavButton.AutoSize = true;
            this._removeFavButton.Name = "_removeFavButton";
            this._removeFavButton.Text = "Remove from list";
            this._removeFavButton.UseVisualStyleBackColor = true;
            this._removeFavButton.Click += new System.EventHandler(this.OnRemoveFavourite);
            // 
            // _resetCheck
            // 
            this._resetCheck.AutoSize = true;
            this._resetCheck.Name = "_resetCheck";
            this._resetCheck.Text = "When switching: point all files to the new folder";
            this._resetCheck.Checked = true;
            this._resetCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this._resetCheck.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this._resetCheck.UseVisualStyleBackColor = true;
            // 
            // _clientInfo
            // 
            this._clientInfo.AutoSize = true;
            this._clientInfo.Name = "_clientInfo";
            this._clientInfo.Text = "";
            this._clientInfo.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._clientInfo.MaximumSize = new System.Drawing.Size(1150, 0);
            this._clientInfo.Font = new System.Drawing.Font("Consolas", 9F);
            // 
            // _dirFlow
            // 
            this._dirFlow.Name = "_dirFlow";
            this._dirFlow.AutoSize = true;
            this._dirFlow.Dock = System.Windows.Forms.DockStyle.Top;
            this._dirFlow.WrapContents = true;
            this._dirFlow.Controls.Add(this._dirLabel);
            this._dirFlow.Controls.Add(this._dirBox);
            this._dirFlow.Controls.Add(this._browseButton);
            this._dirFlow.Controls.Add(this._applyButton);
            this._dirFlow.Controls.Add(this._registryButton);
            this._dirFlow.Controls.Add(this._openDirButton);
            this._dirFlow.Controls.Add(this._favLabel);
            this._dirFlow.Controls.Add(this._favNameBox);
            this._dirFlow.Controls.Add(this._saveFavButton);
            this._dirFlow.Controls.Add(this._removeFavButton);
            this._dirFlow.Controls.Add(this._resetCheck);
            this._dirFlow.Controls.Add(this._clientInfo);
            this._dirFlow.SetFlowBreak(this._openDirButton, true);
            this._dirFlow.SetFlowBreak(this._resetCheck, true);
            // 
            // _dirGroup
            // 
            this._dirGroup.Name = "_dirGroup";
            this._dirGroup.Text = "1. Ultima Online folder";
            this._dirGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dirGroup.AutoSize = true;
            this._dirGroup.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._dirGroup.Controls.Add(this._dirFlow);
            // 
            // _xcFile
            // 
            this._xcFile.Text = "File";
            this._xcFile.Width = 160;
            // 
            // _xcKind
            // 
            this._xcKind.Text = "Extension";
            this._xcKind.Width = 150;
            // 
            // _xcState
            // 
            this._xcState.Text = "State";
            this._xcState.Width = 170;
            // 
            // _xcFolder
            // 
            this._xcFolder.Text = "Folder";
            this._xcFolder.Width = 380;
            // 
            // _xcSize
            // 
            this._xcSize.Text = "Size";
            this._xcSize.Width = 100;
            // 
            // _xcSlots
            // 
            this._xcSlots.Text = "Slots";
            this._xcSlots.Width = 110;
            // 
            // _extList
            // 
            this._extList.Name = "_extList";
            this._extList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._extList.View = System.Windows.Forms.View.Details;
            this._extList.FullRowSelect = true;
            this._extList.HideSelection = false;
            this._extList.MultiSelect = true;
            this._extList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._xcFile, this._xcKind, this._xcState, this._xcFolder, this._xcSize, this._xcSlots });
            this._extList.DoubleClick += new System.EventHandler(this.OnExtOpenFolder);
            this._extList.MultiSelect = false;
            this._extList.Height = 200;
            // 
            // _extSearchButton
            // 
            this._extSearchButton.AutoSize = true;
            this._extSearchButton.Name = "_extSearchButton";
            this._extSearchButton.Text = "Search again + load";
            this._extSearchButton.UseVisualStyleBackColor = true;
            this._extSearchButton.Click += new System.EventHandler(this.OnExtSearch);
            // 
            // _extFolderButton
            // 
            this._extFolderButton.AutoSize = true;
            this._extFolderButton.Name = "_extFolderButton";
            this._extFolderButton.Text = "Load from folder...";
            this._extFolderButton.UseVisualStyleBackColor = true;
            this._extFolderButton.Click += new System.EventHandler(this.OnExtFolder);
            // 
            // _extOutputButton
            // 
            this._extOutputButton.AutoSize = true;
            this._extOutputButton.Name = "_extOutputButton";
            this._extOutputButton.Text = "Load from output folder";
            this._extOutputButton.UseVisualStyleBackColor = true;
            this._extOutputButton.Click += new System.EventHandler(this.OnExtOutput);
            // 
            // _extTabButton
            // 
            this._extTabButton.AutoSize = true;
            this._extTabButton.Name = "_extTabButton";
            this._extTabButton.Text = "Open the Extended tab";
            this._extTabButton.UseVisualStyleBackColor = true;
            this._extTabButton.Click += new System.EventHandler(this.OnExtTab);
            // 
            // _extStatus
            // 
            this._extStatus.AutoSize = true;
            this._extStatus.Name = "_extStatus";
            this._extStatus.Text = "";
            this._extStatus.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._extStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            // 
            // _extButtons
            // 
            this._extButtons.Name = "_extButtons";
            this._extButtons.AutoSize = true;
            this._extButtons.Dock = System.Windows.Forms.DockStyle.Top;
            this._extButtons.WrapContents = true;
            this._extButtons.Controls.Add(this._extSearchButton);
            this._extButtons.Controls.Add(this._extFolderButton);
            this._extButtons.Controls.Add(this._extOutputButton);
            this._extButtons.Controls.Add(this._extTabButton);
            this._extButtons.Controls.Add(this._extStatus);
            // 
            // _extLayout
            // 
            this._extLayout.Name = "_extLayout";
            this._extLayout.ColumnCount = 1;
            this._extLayout.RowCount = 2;
            this._extLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._extLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._extLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this._extLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._extLayout.Controls.Add(this._extList, 0, 0);
            this._extLayout.Controls.Add(this._extButtons, 0, 1);
            // 
            // _extGroup
            // 
            this._extGroup.Name = "_extGroup";
            this._extGroup.Text = "2. UOFiddler extension files (found automatically)";
            this._extGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._extGroup.Height = 270;
            this._extGroup.Controls.Add(this._extLayout);
            // 
            // _filterLabel
            // 
            this._filterLabel.AutoSize = true;
            this._filterLabel.Name = "_filterLabel";
            this._filterLabel.Text = "Filter:";
            this._filterLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _filterBox
            // 
            this._filterBox.Name = "_filterBox";
            this._filterBox.Width = 160;
            this._filterBox.Text = "";
            this._filterBox.TextChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // _showLabel
            // 
            this._showLabel.AutoSize = true;
            this._showLabel.Name = "_showLabel";
            this._showLabel.Text = "Show:";
            this._showLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            // 
            // _showBox
            // 
            this._showBox.Name = "_showBox";
            this._showBox.Width = 230;
            this._showBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._showBox.Items.AddRange(new object[] { "All files", "Found", "Missing", "Not in the Ultima Online folder", "UOFiddler extension files" });
            this._showBox.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // _countLabel
            // 
            this._countLabel.AutoSize = true;
            this._countLabel.Name = "_countLabel";
            this._countLabel.Text = "";
            this._countLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._countLabel.ForeColor = System.Drawing.Color.DimGray;
            // 
            // _filesTop
            // 
            this._filesTop.Name = "_filesTop";
            this._filesTop.AutoSize = true;
            this._filesTop.Dock = System.Windows.Forms.DockStyle.Top;
            this._filesTop.WrapContents = true;
            this._filesTop.Controls.Add(this._filterLabel);
            this._filesTop.Controls.Add(this._filterBox);
            this._filesTop.Controls.Add(this._showLabel);
            this._filesTop.Controls.Add(this._showBox);
            this._filesTop.Controls.Add(this._countLabel);
            // 
            // _fcFile
            // 
            this._fcFile.Text = "File";
            this._fcFile.Width = 170;
            // 
            // _fcState
            // 
            this._fcState.Text = "State";
            this._fcState.Width = 120;
            // 
            // _fcPath
            // 
            this._fcPath.Text = "Path";
            this._fcPath.Width = 560;
            // 
            // _fcSize
            // 
            this._fcSize.Text = "Size";
            this._fcSize.Width = 100;
            // 
            // _fcDate
            // 
            this._fcDate.Text = "Modified";
            this._fcDate.Width = 130;
            // 
            // _fileList
            // 
            this._fileList.Name = "_fileList";
            this._fileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileList.View = System.Windows.Forms.View.Details;
            this._fileList.FullRowSelect = true;
            this._fileList.HideSelection = false;
            this._fileList.MultiSelect = true;
            this._fileList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this._fcFile, this._fcState, this._fcPath, this._fcSize, this._fcDate });
            this._fileList.DoubleClick += new System.EventHandler(this.OnChooseFile);
            // 
            // _menuChoose
            // 
            this._menuChoose.Name = "_menuChoose";
            this._menuChoose.Text = "Choose file...";
            this._menuChoose.Click += new System.EventHandler(this.OnChooseFile);
            // 
            // _menuFromFolder
            // 
            this._menuFromFolder.Name = "_menuFromFolder";
            this._menuFromFolder.Text = "Take all files from a folder...";
            this._menuFromFolder.Click += new System.EventHandler(this.OnTakeFolder);
            // 
            // _menuClear
            // 
            this._menuClear.Name = "_menuClear";
            this._menuClear.Text = "Clear the path (file not used)";
            this._menuClear.Click += new System.EventHandler(this.OnClearPath);
            // 
            // _menuOpenFolder
            // 
            this._menuOpenFolder.Name = "_menuOpenFolder";
            this._menuOpenFolder.Text = "Open its folder";
            this._menuOpenFolder.Click += new System.EventHandler(this.OnOpenFileFolder);
            // 
            // _menuCopy
            // 
            this._menuCopy.Name = "_menuCopy";
            this._menuCopy.Text = "Copy the path";
            this._menuCopy.Click += new System.EventHandler(this.OnCopyPath);
            // 
            // _fileMenu
            // 
            this._fileMenu.Name = "_fileMenu";
            this._fileMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this._menuChoose, this._menuFromFolder, this._menuClear, this._menuOpenFolder, this._menuCopy });
            this._fileList.ContextMenuStrip = this._fileMenu;
            // 
            // _filesLayout
            // 
            this._filesLayout.Name = "_filesLayout";
            this._filesLayout.ColumnCount = 1;
            this._filesLayout.RowCount = 2;
            this._filesLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._filesLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._filesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._filesLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._filesLayout.Controls.Add(this._filesTop, 0, 0);
            this._filesLayout.Controls.Add(this._fileList, 0, 1);
            // 
            // _filesGroup
            // 
            this._filesGroup.Name = "_filesGroup";
            this._filesGroup.Text = "3. All client files (double click: choose a file)";
            this._filesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this._filesGroup.Controls.Add(this._filesLayout);
            // 
            // _reloadButton
            // 
            this._reloadButton.AutoSize = true;
            this._reloadButton.Name = "_reloadButton";
            this._reloadButton.Text = "Reload all files now";
            this._reloadButton.UseVisualStyleBackColor = true;
            this._reloadButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._reloadButton.Click += new System.EventHandler(this.OnReloadFiles);
            // 
            // _backupButton
            // 
            this._backupButton.AutoSize = true;
            this._backupButton.Name = "_backupButton";
            this._backupButton.Text = "Copy the whole folder (backup)...";
            this._backupButton.UseVisualStyleBackColor = true;
            this._backupButton.Click += new System.EventHandler(this.OnBackupFolder);
            // 
            // _closeButton
            // 
            this._closeButton.AutoSize = true;
            this._closeButton.Name = "_closeButton";
            this._closeButton.Text = "Close";
            this._closeButton.UseVisualStyleBackColor = true;
            this._closeButton.Click += new System.EventHandler(this.OnClose);
            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Text = "";
            this._statusLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._statusLabel.MaximumSize = new System.Drawing.Size(900, 0);
            // 
            // _bottomFlow
            // 
            this._bottomFlow.Name = "_bottomFlow";
            this._bottomFlow.AutoSize = true;
            this._bottomFlow.Dock = System.Windows.Forms.DockStyle.Top;
            this._bottomFlow.WrapContents = true;
            this._bottomFlow.Controls.Add(this._reloadButton);
            this._bottomFlow.Controls.Add(this._backupButton);
            this._bottomFlow.Controls.Add(this._closeButton);
            this._bottomFlow.Controls.Add(this._statusLabel);
            // 
            // _rootLayout
            // 
            this._rootLayout.Name = "_rootLayout";
            this._rootLayout.ColumnCount = 1;
            this._rootLayout.RowCount = 4;
            this._rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 280F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this._rootLayout.Controls.Add(this._dirGroup, 0, 0);
            this._rootLayout.Controls.Add(this._extGroup, 0, 1);
            this._rootLayout.Controls.Add(this._filesGroup, 0, 2);
            this._rootLayout.Controls.Add(this._bottomFlow, 0, 3);
            this._rootLayout.Padding = new System.Windows.Forms.Padding(6);
            // 
            // PathSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 860);
            this.Controls.Add(this._rootLayout);
            this.Name = "PathSettingsForm";
            this.Text = "Path Settings";
            this.MinimumSize = new System.Drawing.Size(900, 620);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this._rootLayout.ResumeLayout(false);
            this._rootLayout.PerformLayout();
            this._bottomFlow.ResumeLayout(false);
            this._bottomFlow.PerformLayout();
            this._filesGroup.ResumeLayout(false);
            this._filesGroup.PerformLayout();
            this._filesLayout.ResumeLayout(false);
            this._filesLayout.PerformLayout();
            this._fileMenu.ResumeLayout(false);
            this._fileMenu.PerformLayout();
            this._filesTop.ResumeLayout(false);
            this._filesTop.PerformLayout();
            this._extGroup.ResumeLayout(false);
            this._extGroup.PerformLayout();
            this._extLayout.ResumeLayout(false);
            this._extLayout.PerformLayout();
            this._extButtons.ResumeLayout(false);
            this._extButtons.PerformLayout();
            this._dirGroup.ResumeLayout(false);
            this._dirGroup.PerformLayout();
            this._dirFlow.ResumeLayout(false);
            this._dirFlow.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label _dirLabel;
        private System.Windows.Forms.ComboBox _dirBox;
        private System.Windows.Forms.Button _browseButton;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.Button _registryButton;
        private System.Windows.Forms.Button _openDirButton;
        private System.Windows.Forms.Label _favLabel;
        private System.Windows.Forms.TextBox _favNameBox;
        private System.Windows.Forms.Button _saveFavButton;
        private System.Windows.Forms.Button _removeFavButton;
        private System.Windows.Forms.CheckBox _resetCheck;
        private System.Windows.Forms.Label _clientInfo;
        private System.Windows.Forms.FlowLayoutPanel _dirFlow;
        private System.Windows.Forms.GroupBox _dirGroup;
        private System.Windows.Forms.ListView _extList;
        private System.Windows.Forms.ColumnHeader _xcFile;
        private System.Windows.Forms.ColumnHeader _xcKind;
        private System.Windows.Forms.ColumnHeader _xcState;
        private System.Windows.Forms.ColumnHeader _xcFolder;
        private System.Windows.Forms.ColumnHeader _xcSize;
        private System.Windows.Forms.ColumnHeader _xcSlots;
        private System.Windows.Forms.Button _extSearchButton;
        private System.Windows.Forms.Button _extFolderButton;
        private System.Windows.Forms.Button _extOutputButton;
        private System.Windows.Forms.Button _extTabButton;
        private System.Windows.Forms.Label _extStatus;
        private System.Windows.Forms.FlowLayoutPanel _extButtons;
        private System.Windows.Forms.TableLayoutPanel _extLayout;
        private System.Windows.Forms.GroupBox _extGroup;
        private System.Windows.Forms.Label _filterLabel;
        private System.Windows.Forms.TextBox _filterBox;
        private System.Windows.Forms.Label _showLabel;
        private System.Windows.Forms.ComboBox _showBox;
        private System.Windows.Forms.Label _countLabel;
        private System.Windows.Forms.FlowLayoutPanel _filesTop;
        private System.Windows.Forms.ListView _fileList;
        private System.Windows.Forms.ColumnHeader _fcFile;
        private System.Windows.Forms.ColumnHeader _fcState;
        private System.Windows.Forms.ColumnHeader _fcPath;
        private System.Windows.Forms.ColumnHeader _fcSize;
        private System.Windows.Forms.ColumnHeader _fcDate;
        private System.Windows.Forms.ContextMenuStrip _fileMenu;
        private System.Windows.Forms.ToolStripMenuItem _menuChoose;
        private System.Windows.Forms.ToolStripMenuItem _menuFromFolder;
        private System.Windows.Forms.ToolStripMenuItem _menuClear;
        private System.Windows.Forms.ToolStripMenuItem _menuOpenFolder;
        private System.Windows.Forms.ToolStripMenuItem _menuCopy;
        private System.Windows.Forms.TableLayoutPanel _filesLayout;
        private System.Windows.Forms.GroupBox _filesGroup;
        private System.Windows.Forms.Button _reloadButton;
        private System.Windows.Forms.Button _backupButton;
        private System.Windows.Forms.Button _closeButton;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.FlowLayoutPanel _bottomFlow;
        private System.Windows.Forms.TableLayoutPanel _rootLayout;
        private System.Windows.Forms.ToolTip _tips;
    }
}

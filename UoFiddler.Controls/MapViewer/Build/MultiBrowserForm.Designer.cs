namespace UoFiddler.Controls.MapViewer.Build
{
    partial class MultiBrowserForm
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
                DisposeResources();
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
            splitContainer = new System.Windows.Forms.SplitContainer();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageClient = new System.Windows.Forms.TabPage();
            listViewClient = new System.Windows.Forms.ListView();
            columnClientId = new System.Windows.Forms.ColumnHeader();
            columnClientName = new System.Windows.Forms.ColumnHeader();
            columnClientParts = new System.Windows.Forms.ColumnHeader();
            columnClientSize = new System.Windows.Forms.ColumnHeader();
            panelClientTop = new System.Windows.Forms.TableLayoutPanel();
            labelClientSearch = new System.Windows.Forms.Label();
            textBoxClientSearch = new System.Windows.Forms.TextBox();
            tabPageLibrary = new System.Windows.Forms.TabPage();
            listViewLibrary = new System.Windows.Forms.ListView();
            columnLibId = new System.Windows.Forms.ColumnHeader();
            columnLibName = new System.Windows.Forms.ColumnHeader();
            columnLibCategory = new System.Windows.Forms.ColumnHeader();
            columnLibParts = new System.Windows.Forms.ColumnHeader();
            columnLibSize = new System.Windows.Forms.ColumnHeader();
            columnLibAuthor = new System.Windows.Forms.ColumnHeader();
            panelLibraryTop = new System.Windows.Forms.TableLayoutPanel();
            labelLibrarySearch = new System.Windows.Forms.Label();
            textBoxLibrarySearch = new System.Windows.Forms.TextBox();
            labelCategory = new System.Windows.Forms.Label();
            comboBoxCategory = new System.Windows.Forms.ComboBox();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            labelInfo = new System.Windows.Forms.Label();
            flowPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonPlace = new System.Windows.Forms.Button();
            buttonAddToLibrary = new System.Windows.Forms.Button();
            buttonExport = new System.Windows.Forms.Button();
            buttonSaveToMulti = new System.Windows.Forms.Button();
            buttonEdit = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            groupBoxImport = new System.Windows.Forms.GroupBox();
            flowPanelImport = new System.Windows.Forms.FlowLayoutPanel();
            labelTxtIds = new System.Windows.Forms.Label();
            comboBoxTxtIds = new System.Windows.Forms.ComboBox();
            buttonImportFiles = new System.Windows.Forms.Button();
            buttonImportFolder = new System.Windows.Forms.Button();
            buttonCompact = new System.Windows.Forms.Button();
            buttonLibraryFile = new System.Windows.Forms.Button();
            buttonMoveLibrary = new System.Windows.Forms.Button();
            buttonOpenLibraryFolder = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            searchTimer = new System.Windows.Forms.Timer(components);
            toolTip = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageClient.SuspendLayout();
            panelClientTop.SuspendLayout();
            tabPageLibrary.SuspendLayout();
            panelLibraryTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            flowPanelButtons.SuspendLayout();
            groupBoxImport.SuspendLayout();
            flowPanelImport.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // splitContainer
            //
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            splitContainer.Panel1.Controls.Add(tabControl);
            //
            // splitContainer.Panel2
            //
            splitContainer.Panel2.Controls.Add(pictureBoxPreview);
            splitContainer.Panel2.Controls.Add(labelInfo);
            splitContainer.Panel2.Controls.Add(flowPanelButtons);
            splitContainer.Panel2.Controls.Add(groupBoxImport);
            splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(4);
            splitContainer.Size = new System.Drawing.Size(1084, 639);
            splitContainer.SplitterDistance = 560;
            splitContainer.TabIndex = 0;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPageClient);
            tabControl.Controls.Add(tabPageLibrary);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += OnTabChanged;
            //
            // tabPageClient
            //
            tabPageClient.Controls.Add(listViewClient);
            tabPageClient.Controls.Add(panelClientTop);
            tabPageClient.Name = "tabPageClient";
            tabPageClient.Padding = new System.Windows.Forms.Padding(4);
            tabPageClient.TabIndex = 0;
            tabPageClient.Text = "Client multis (multi.mul)";
            tabPageClient.UseVisualStyleBackColor = true;
            //
            // listViewClient
            //
            listViewClient.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnClientId, columnClientName, columnClientParts, columnClientSize });
            listViewClient.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewClient.FullRowSelect = true;
            listViewClient.GridLines = true;
            listViewClient.HideSelection = false;
            listViewClient.MultiSelect = false;
            listViewClient.Name = "listViewClient";
            listViewClient.TabIndex = 1;
            listViewClient.UseCompatibleStateImageBehavior = false;
            listViewClient.View = System.Windows.Forms.View.Details;
            listViewClient.VirtualMode = true;
            listViewClient.RetrieveVirtualItem += OnRetrieveClientItem;
            listViewClient.SelectedIndexChanged += OnSelectionChanged;
            listViewClient.DoubleClick += OnClickPlace;
            //
            // columnClientId
            //
            columnClientId.Text = "ID";
            columnClientId.Width = 90;
            //
            // columnClientName
            //
            columnClientName.Text = "Name";
            columnClientName.Width = 250;
            //
            // columnClientParts
            //
            columnClientParts.Text = "Parts";
            columnClientParts.Width = 60;
            //
            // columnClientSize
            //
            columnClientSize.Text = "Size";
            columnClientSize.Width = 70;
            //
            // panelClientTop
            //
            panelClientTop.AutoSize = true;
            panelClientTop.ColumnCount = 2;
            panelClientTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelClientTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelClientTop.Controls.Add(labelClientSearch, 0, 0);
            panelClientTop.Controls.Add(textBoxClientSearch, 1, 0);
            panelClientTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelClientTop.Name = "panelClientTop";
            panelClientTop.RowCount = 1;
            panelClientTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelClientTop.TabIndex = 0;
            //
            // labelClientSearch
            //
            labelClientSearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelClientSearch.AutoSize = true;
            labelClientSearch.Name = "labelClientSearch";
            labelClientSearch.Text = "Search (name or id):";
            //
            // textBoxClientSearch
            //
            textBoxClientSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxClientSearch.Name = "textBoxClientSearch";
            textBoxClientSearch.TabIndex = 0;
            textBoxClientSearch.TextChanged += OnSearchChanged;
            //
            // tabPageLibrary
            //
            tabPageLibrary.Controls.Add(listViewLibrary);
            tabPageLibrary.Controls.Add(panelLibraryTop);
            tabPageLibrary.Name = "tabPageLibrary";
            tabPageLibrary.Padding = new System.Windows.Forms.Padding(4);
            tabPageLibrary.TabIndex = 1;
            tabPageLibrary.Text = "Library";
            tabPageLibrary.UseVisualStyleBackColor = true;
            //
            // listViewLibrary
            //
            listViewLibrary.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnLibId, columnLibName, columnLibCategory, columnLibParts, columnLibSize, columnLibAuthor });
            listViewLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewLibrary.FullRowSelect = true;
            listViewLibrary.GridLines = true;
            listViewLibrary.HideSelection = false;
            listViewLibrary.Name = "listViewLibrary";
            listViewLibrary.TabIndex = 1;
            listViewLibrary.UseCompatibleStateImageBehavior = false;
            listViewLibrary.View = System.Windows.Forms.View.Details;
            listViewLibrary.VirtualMode = true;
            listViewLibrary.RetrieveVirtualItem += OnRetrieveLibraryItem;
            listViewLibrary.SelectedIndexChanged += OnSelectionChanged;
            listViewLibrary.VirtualItemsSelectionRangeChanged += OnSelectionRangeChanged;
            listViewLibrary.DoubleClick += OnClickPlace;
            //
            // columnLibId
            //
            columnLibId.Text = "ID";
            columnLibId.Width = 60;
            //
            // columnLibName
            //
            columnLibName.Text = "Name";
            columnLibName.Width = 180;
            //
            // columnLibCategory
            //
            columnLibCategory.Text = "Category";
            columnLibCategory.Width = 100;
            //
            // columnLibParts
            //
            columnLibParts.Text = "Parts";
            columnLibParts.Width = 55;
            //
            // columnLibSize
            //
            columnLibSize.Text = "Size";
            columnLibSize.Width = 65;
            //
            // columnLibAuthor
            //
            columnLibAuthor.Text = "Author";
            columnLibAuthor.Width = 80;
            //
            // panelLibraryTop
            //
            panelLibraryTop.AutoSize = true;
            panelLibraryTop.ColumnCount = 4;
            panelLibraryTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelLibraryTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelLibraryTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelLibraryTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            panelLibraryTop.Controls.Add(labelLibrarySearch, 0, 0);
            panelLibraryTop.Controls.Add(textBoxLibrarySearch, 1, 0);
            panelLibraryTop.Controls.Add(labelCategory, 2, 0);
            panelLibraryTop.Controls.Add(comboBoxCategory, 3, 0);
            panelLibraryTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelLibraryTop.Name = "panelLibraryTop";
            panelLibraryTop.RowCount = 1;
            panelLibraryTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelLibraryTop.TabIndex = 0;
            //
            // labelLibrarySearch
            //
            labelLibrarySearch.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelLibrarySearch.AutoSize = true;
            labelLibrarySearch.Name = "labelLibrarySearch";
            labelLibrarySearch.Text = "Search:";
            //
            // textBoxLibrarySearch
            //
            textBoxLibrarySearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxLibrarySearch.Name = "textBoxLibrarySearch";
            textBoxLibrarySearch.TabIndex = 0;
            toolTip.SetToolTip(textBoxLibrarySearch, "Name, author, category or #id");
            textBoxLibrarySearch.TextChanged += OnSearchChanged;
            //
            // labelCategory
            //
            labelCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelCategory.AutoSize = true;
            labelCategory.Name = "labelCategory";
            labelCategory.Text = "Category:";
            //
            // comboBoxCategory
            //
            comboBoxCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxCategory.Name = "comboBoxCategory";
            comboBoxCategory.TabIndex = 1;
            comboBoxCategory.SelectedIndexChanged += OnSearchChanged;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.FromArgb(24, 24, 28);
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.Resize += OnPreviewResize;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            labelInfo.Size = new System.Drawing.Size(510, 52);
            labelInfo.TabIndex = 1;
            labelInfo.Text = "-";
            //
            // flowPanelButtons
            //
            flowPanelButtons.AutoSize = true;
            flowPanelButtons.Controls.Add(buttonPlace);
            flowPanelButtons.Controls.Add(buttonAddToLibrary);
            flowPanelButtons.Controls.Add(buttonExport);
            flowPanelButtons.Controls.Add(buttonSaveToMulti);
            flowPanelButtons.Controls.Add(buttonEdit);
            flowPanelButtons.Controls.Add(buttonDelete);
            flowPanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowPanelButtons.Name = "flowPanelButtons";
            flowPanelButtons.TabIndex = 2;
            //
            // buttonPlace
            //
            buttonPlace.AutoSize = true;
            buttonPlace.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            buttonPlace.Name = "buttonPlace";
            buttonPlace.TabIndex = 0;
            buttonPlace.Text = "Place on map";
            toolTip.SetToolTip(buttonPlace, "Places the building in the region viewer (double click works too)");
            buttonPlace.UseVisualStyleBackColor = true;
            buttonPlace.Click += OnClickPlace;
            //
            // buttonAddToLibrary
            //
            buttonAddToLibrary.AutoSize = true;
            buttonAddToLibrary.Name = "buttonAddToLibrary";
            buttonAddToLibrary.TabIndex = 1;
            buttonAddToLibrary.Text = "Copy to library";
            buttonAddToLibrary.UseVisualStyleBackColor = true;
            buttonAddToLibrary.Click += OnClickAddToLibrary;
            //
            // buttonExport
            //
            buttonExport.AutoSize = true;
            buttonExport.Name = "buttonExport";
            buttonExport.TabIndex = 2;
            buttonExport.Text = "Export...";
            buttonExport.UseVisualStyleBackColor = true;
            buttonExport.Click += OnClickExport;
            //
            // buttonSaveToMulti
            //
            buttonSaveToMulti.AutoSize = true;
            buttonSaveToMulti.Name = "buttonSaveToMulti";
            buttonSaveToMulti.TabIndex = 3;
            buttonSaveToMulti.Text = "Save into multi.mul...";
            toolTip.SetToolTip(buttonSaveToMulti, "Writes the building as entry into multi.mul/multi.idx (output directory)");
            buttonSaveToMulti.UseVisualStyleBackColor = true;
            buttonSaveToMulti.Click += OnClickSaveToMulti;
            //
            // buttonEdit
            //
            buttonEdit.AutoSize = true;
            buttonEdit.Name = "buttonEdit";
            buttonEdit.TabIndex = 4;
            buttonEdit.Text = "Name / category...";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += OnClickEdit;
            //
            // buttonDelete
            //
            buttonDelete.AutoSize = true;
            buttonDelete.Name = "buttonDelete";
            buttonDelete.TabIndex = 5;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += OnClickDelete;
            //
            // groupBoxImport
            //
            groupBoxImport.AutoSize = true;
            groupBoxImport.Controls.Add(flowPanelImport);
            groupBoxImport.Dock = System.Windows.Forms.DockStyle.Bottom;
            groupBoxImport.Name = "groupBoxImport";
            groupBoxImport.TabIndex = 3;
            groupBoxImport.TabStop = false;
            groupBoxImport.Text = "Library";
            //
            // flowPanelImport
            //
            flowPanelImport.AutoSize = true;
            flowPanelImport.Controls.Add(labelTxtIds);
            flowPanelImport.Controls.Add(comboBoxTxtIds);
            flowPanelImport.Controls.Add(buttonImportFiles);
            flowPanelImport.Controls.Add(buttonImportFolder);
            flowPanelImport.Controls.Add(buttonCompact);
            flowPanelImport.Controls.Add(buttonLibraryFile);
            flowPanelImport.Controls.Add(buttonMoveLibrary);
            flowPanelImport.Controls.Add(buttonOpenLibraryFolder);
            flowPanelImport.Dock = System.Windows.Forms.DockStyle.Fill;
            flowPanelImport.Name = "flowPanelImport";
            flowPanelImport.TabIndex = 0;
            //
            // labelTxtIds
            //
            labelTxtIds.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTxtIds.AutoSize = true;
            labelTxtIds.Name = "labelTxtIds";
            labelTxtIds.Text = "TXT ids:";
            //
            // comboBoxTxtIds
            //
            comboBoxTxtIds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxTxtIds.Items.AddRange(new object[] { "Auto", "Hex", "Decimal" });
            comboBoxTxtIds.Name = "comboBoxTxtIds";
            comboBoxTxtIds.Size = new System.Drawing.Size(80, 23);
            comboBoxTxtIds.TabIndex = 0;
            toolTip.SetToolTip(comboBoxTxtIds, "Auto: \"0x\" or letters A-F = hex, otherwise decimal");
            //
            // buttonImportFiles
            //
            buttonImportFiles.AutoSize = true;
            buttonImportFiles.Name = "buttonImportFiles";
            buttonImportFiles.TabIndex = 1;
            buttonImportFiles.Text = "Import files...";
            buttonImportFiles.UseVisualStyleBackColor = true;
            buttonImportFiles.Click += OnClickImportFiles;
            //
            // buttonImportFolder
            //
            buttonImportFolder.AutoSize = true;
            buttonImportFolder.Name = "buttonImportFolder";
            buttonImportFolder.TabIndex = 2;
            buttonImportFolder.Text = "Import folder...";
            toolTip.SetToolTip(buttonImportFolder, "Imports all building files of a folder and its sub folders (sub folder = category)");
            buttonImportFolder.UseVisualStyleBackColor = true;
            buttonImportFolder.Click += OnClickImportFolder;
            //
            // buttonCompact
            //
            buttonCompact.AutoSize = true;
            buttonCompact.Name = "buttonCompact";
            buttonCompact.TabIndex = 3;
            buttonCompact.Text = "Compact";
            toolTip.SetToolTip(buttonCompact, "Removes deleted entries from the library file");
            buttonCompact.UseVisualStyleBackColor = true;
            buttonCompact.Click += OnClickCompact;
            //
            // buttonLibraryFile
            //
            buttonLibraryFile.AutoSize = true;
            buttonLibraryFile.Name = "buttonLibraryFile";
            buttonLibraryFile.TabIndex = 4;
            buttonLibraryFile.Text = "Library file...";
            toolTip.SetToolTip(buttonLibraryFile, "Open or create another library file");
            buttonLibraryFile.UseVisualStyleBackColor = true;
            buttonLibraryFile.Click += OnClickLibraryFile;
            //
            // buttonMoveLibrary
            //
            buttonMoveLibrary.AutoSize = true;
            buttonMoveLibrary.Name = "buttonMoveLibrary";
            buttonMoveLibrary.TabIndex = 5;
            buttonMoveLibrary.Text = "Move library...";
            toolTip.SetToolTip(buttonMoveLibrary, "Moves the library files into another folder");
            buttonMoveLibrary.UseVisualStyleBackColor = true;
            buttonMoveLibrary.Click += OnClickMoveLibrary;
            //
            // buttonOpenLibraryFolder
            //
            buttonOpenLibraryFolder.AutoSize = true;
            buttonOpenLibraryFolder.Name = "buttonOpenLibraryFolder";
            buttonOpenLibraryFolder.TabIndex = 6;
            buttonOpenLibraryFolder.Text = "Open folder";
            buttonOpenLibraryFolder.UseVisualStyleBackColor = true;
            buttonOpenLibraryFolder.Click += OnClickOpenLibraryFolder;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statusLabel });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 1;
            //
            // statusLabel
            //
            statusLabel.Name = "statusLabel";
            statusLabel.Spring = true;
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // searchTimer
            //
            searchTimer.Interval = 300;
            searchTimer.Tick += OnSearchTimer;
            //
            // MultiBrowserForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1084, 661);
            Controls.Add(splitContainer);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(800, 450);
            Name = "MultiBrowserForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Multis / Building library";
            Activated += OnActivated;
            FormClosed += OnFormClosed;
            Shown += OnShown;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageClient.ResumeLayout(false);
            tabPageClient.PerformLayout();
            panelClientTop.ResumeLayout(false);
            panelClientTop.PerformLayout();
            tabPageLibrary.ResumeLayout(false);
            tabPageLibrary.PerformLayout();
            panelLibraryTop.ResumeLayout(false);
            panelLibraryTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            flowPanelButtons.ResumeLayout(false);
            flowPanelButtons.PerformLayout();
            groupBoxImport.ResumeLayout(false);
            groupBoxImport.PerformLayout();
            flowPanelImport.ResumeLayout(false);
            flowPanelImport.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageClient;
        private System.Windows.Forms.ListView listViewClient;
        private System.Windows.Forms.ColumnHeader columnClientId;
        private System.Windows.Forms.ColumnHeader columnClientName;
        private System.Windows.Forms.ColumnHeader columnClientParts;
        private System.Windows.Forms.ColumnHeader columnClientSize;
        private System.Windows.Forms.TableLayoutPanel panelClientTop;
        private System.Windows.Forms.Label labelClientSearch;
        private System.Windows.Forms.TextBox textBoxClientSearch;
        private System.Windows.Forms.TabPage tabPageLibrary;
        private System.Windows.Forms.ListView listViewLibrary;
        private System.Windows.Forms.ColumnHeader columnLibId;
        private System.Windows.Forms.ColumnHeader columnLibName;
        private System.Windows.Forms.ColumnHeader columnLibCategory;
        private System.Windows.Forms.ColumnHeader columnLibParts;
        private System.Windows.Forms.ColumnHeader columnLibSize;
        private System.Windows.Forms.ColumnHeader columnLibAuthor;
        private System.Windows.Forms.TableLayoutPanel panelLibraryTop;
        private System.Windows.Forms.Label labelLibrarySearch;
        private System.Windows.Forms.TextBox textBoxLibrarySearch;
        private System.Windows.Forms.Label labelCategory;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.FlowLayoutPanel flowPanelButtons;
        private System.Windows.Forms.Button buttonPlace;
        private System.Windows.Forms.Button buttonAddToLibrary;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonSaveToMulti;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.GroupBox groupBoxImport;
        private System.Windows.Forms.FlowLayoutPanel flowPanelImport;
        private System.Windows.Forms.Label labelTxtIds;
        private System.Windows.Forms.ComboBox comboBoxTxtIds;
        private System.Windows.Forms.Button buttonImportFiles;
        private System.Windows.Forms.Button buttonImportFolder;
        private System.Windows.Forms.Button buttonCompact;
        private System.Windows.Forms.Button buttonLibraryFile;
        private System.Windows.Forms.Button buttonMoveLibrary;
        private System.Windows.Forms.Button buttonOpenLibraryFolder;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Timer searchTimer;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

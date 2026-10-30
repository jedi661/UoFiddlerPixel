/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    partial class EcMobileAnimationControl
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
            components = new System.ComponentModel.Container();
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripLabelBody = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportFramePngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportBinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripDropDownImport = new System.Windows.Forms.ToolStripDropDownButton();
            importFramePngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importPngFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorImport = new System.Windows.Forms.ToolStripSeparator();
            importBinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addBinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            splitContainerLists = new System.Windows.Forms.SplitContainer();
            listViewBodies = new System.Windows.Forms.ListView();
            columnHeaderBody = new System.Windows.Forms.ColumnHeader();
            columnHeaderBodyHex = new System.Windows.Forms.ColumnHeader();
            columnHeaderActions = new System.Windows.Forms.ColumnHeader();
            columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            contextMenuStripBodies = new System.Windows.Forms.ContextMenuStrip(components);
            addBinMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorBodies = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            listViewActions = new System.Windows.Forms.ListView();
            columnHeaderAction = new System.Windows.Forms.ColumnHeader();
            columnHeaderActionSize = new System.Windows.Forms.ColumnHeader();
            contextMenuStripActions = new System.Windows.Forms.ContextMenuStrip(components);
            exportPngMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportBinMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorActions = new System.Windows.Forms.ToolStripSeparator();
            importBinMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importPngFolderMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainerFrames = new System.Windows.Forms.SplitContainer();
            listViewFrames = new System.Windows.Forms.ListView();
            columnHeaderFrameNumber = new System.Windows.Forms.ColumnHeader();
            columnHeaderDir = new System.Windows.Forms.ColumnHeader();
            columnHeaderFrameSize = new System.Windows.Forms.ColumnHeader();
            columnHeaderPosition = new System.Windows.Forms.ColumnHeader();
            contextMenuStripFrames = new System.Windows.Forms.ContextMenuStrip(components);
            exportFramePngMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importFramePngMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelPreview = new System.Windows.Forms.Panel();
            previewPanel = new EcPreviewPanel();
            labelInfo = new System.Windows.Forms.Label();
            flowLayoutPanelControls = new System.Windows.Forms.FlowLayoutPanel();
            tableLayoutPanelCompass = new System.Windows.Forms.TableLayoutPanel();
            buttonViewUp = new System.Windows.Forms.Button();
            buttonViewUpRight = new System.Windows.Forms.Button();
            buttonViewRight = new System.Windows.Forms.Button();
            buttonViewDownRight = new System.Windows.Forms.Button();
            buttonViewDown = new System.Windows.Forms.Button();
            buttonViewDownLeft = new System.Windows.Forms.Button();
            buttonViewLeft = new System.Windows.Forms.Button();
            buttonViewUpLeft = new System.Windows.Forms.Button();
            buttonTurn = new System.Windows.Forms.Button();
            flowLayoutPanelOptions = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanelLine = new System.Windows.Forms.FlowLayoutPanel();
            labelStoredDir = new System.Windows.Forms.Label();
            numericUpDownDirection = new System.Windows.Forms.NumericUpDown();
            labelFramesPerDir = new System.Windows.Forms.Label();
            numericUpDownPerDirection = new System.Windows.Forms.NumericUpDown();
            labelMs = new System.Windows.Forms.Label();
            numericUpDownSpeed = new System.Windows.Forms.NumericUpDown();
            buttonPlay = new System.Windows.Forms.Button();
            checkBoxTurn = new System.Windows.Forms.CheckBox();
            checkBoxGround = new System.Windows.Forms.CheckBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            timerPlay = new System.Windows.Forms.Timer(components);
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLists).BeginInit();
            splitContainerLists.Panel1.SuspendLayout();
            splitContainerLists.Panel2.SuspendLayout();
            splitContainerLists.SuspendLayout();
            contextMenuStripBodies.SuspendLayout();
            contextMenuStripActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerFrames).BeginInit();
            splitContainerFrames.Panel1.SuspendLayout();
            splitContainerFrames.Panel2.SuspendLayout();
            splitContainerFrames.SuspendLayout();
            contextMenuStripFrames.SuspendLayout();
            panelPreview.SuspendLayout();
            flowLayoutPanelControls.SuspendLayout();
            tableLayoutPanelCompass.SuspendLayout();
            flowLayoutPanelOptions.SuspendLayout();
            flowLayoutPanelLine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDirection).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPerDirection).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownSpeed).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelBody, toolStripTextBoxSearch, toolStripButtonSearch, toolStripSeparator1, toolStripDropDownExport, toolStripDropDownImport, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelBody
            //
            toolStripLabelBody.Name = "toolStripLabelBody";
            toolStripLabelBody.Text = "Body:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(90, 25);
            toolStripTextBoxSearch.ToolTipText = "Body id – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonSearch
            //
            toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSearch.Name = "toolStripButtonSearch";
            toolStripButtonSearch.Text = "Search";
            toolStripButtonSearch.Click += OnClickSearch;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripDropDownExport
            //
            toolStripDropDownExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportPngToolStripMenuItem, exportFramePngToolStripMenuItem, exportBinToolStripMenuItem });
            toolStripDropDownExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownExport.Name = "toolStripDropDownExport";
            toolStripDropDownExport.Text = "Export";
            //
            // exportPngToolStripMenuItem
            //
            exportPngToolStripMenuItem.Name = "exportPngToolStripMenuItem";
            exportPngToolStripMenuItem.Text = "Frames of this action as PNG...";
            exportPngToolStripMenuItem.Click += OnClickExportPng;
            //
            // exportFramePngToolStripMenuItem
            //
            exportFramePngToolStripMenuItem.Name = "exportFramePngToolStripMenuItem";
            exportFramePngToolStripMenuItem.Text = "Selected frame as PNG...";
            exportFramePngToolStripMenuItem.Click += OnClickExportFramePng;
            //
            // exportBinToolStripMenuItem
            //
            exportBinToolStripMenuItem.Name = "exportBinToolStripMenuItem";
            exportBinToolStripMenuItem.Text = "Action as .bin (AMOU)...";
            exportBinToolStripMenuItem.Click += OnClickExportBin;
            //
            // toolStripDropDownImport
            //
            toolStripDropDownImport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { importFramePngToolStripMenuItem, importPngFolderToolStripMenuItem, toolStripSeparatorImport, importBinToolStripMenuItem, addBinToolStripMenuItem });
            toolStripDropDownImport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownImport.Name = "toolStripDropDownImport";
            toolStripDropDownImport.Text = "Import";
            //
            // importFramePngToolStripMenuItem
            //
            importFramePngToolStripMenuItem.Name = "importFramePngToolStripMenuItem";
            importFramePngToolStripMenuItem.Text = "Replace selected frame with PNG...";
            importFramePngToolStripMenuItem.Click += OnClickImportFramePng;
            //
            // importPngFolderToolStripMenuItem
            //
            importPngFolderToolStripMenuItem.Name = "importPngFolderToolStripMenuItem";
            importPngFolderToolStripMenuItem.Text = "Replace frames from PNG folder (exported names)...";
            importPngFolderToolStripMenuItem.Click += OnClickImportPngFolder;
            //
            // toolStripSeparatorImport
            //
            toolStripSeparatorImport.Name = "toolStripSeparatorImport";
            //
            // importBinToolStripMenuItem
            //
            importBinToolStripMenuItem.Name = "importBinToolStripMenuItem";
            importBinToolStripMenuItem.Text = "Replace action with .bin (AMOU)...";
            importBinToolStripMenuItem.Click += OnClickImportBin;
            //
            // addBinToolStripMenuItem
            //
            addBinToolStripMenuItem.Name = "addBinToolStripMenuItem";
            addBinToolStripMenuItem.Text = "Add .bin as new body / action...";
            addBinToolStripMenuItem.Click += OnClickAddBin;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // splitContainerMain
            //
            splitContainerMain.Panel1.Controls.Add(splitContainerLists);
            splitContainerMain.Panel2.Controls.Add(splitContainerFrames);
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Size = new System.Drawing.Size(1100, 550);
            splitContainerMain.SplitterDistance = 440;
            splitContainerMain.TabIndex = 1;
            //
            // splitContainerLists
            //
            splitContainerLists.Panel1.Controls.Add(listViewBodies);
            splitContainerLists.Panel2.Controls.Add(listViewActions);
            splitContainerLists.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerLists.Name = "splitContainerLists";
            splitContainerLists.Size = new System.Drawing.Size(440, 550);
            splitContainerLists.SplitterDistance = 300;
            splitContainerLists.TabIndex = 1;
            //
            // listViewBodies
            //
            listViewBodies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderBody, columnHeaderBodyHex, columnHeaderActions, columnHeaderFile });
            listViewBodies.ContextMenuStrip = contextMenuStripBodies;
            listViewBodies.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewBodies.FullRowSelect = true;
            listViewBodies.HideSelection = false;
            listViewBodies.MultiSelect = false;
            listViewBodies.Name = "listViewBodies";
            listViewBodies.TabIndex = 0;
            listViewBodies.UseCompatibleStateImageBehavior = false;
            listViewBodies.View = System.Windows.Forms.View.Details;
            listViewBodies.VirtualMode = true;
            listViewBodies.RetrieveVirtualItem += RetrieveBody;
            listViewBodies.SelectedIndexChanged += OnBodySelectedIndexChanged;
            //
            // columnHeaderBody
            //
            columnHeaderBody.Text = "Body";
            columnHeaderBody.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderBody.Width = 60;
            //
            // columnHeaderBodyHex
            //
            columnHeaderBodyHex.Text = "Hex";
            columnHeaderBodyHex.Width = 55;
            //
            // columnHeaderActions
            //
            columnHeaderActions.Text = "Actions";
            columnHeaderActions.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderActions.Width = 55;
            //
            // columnHeaderFile
            //
            columnHeaderFile.Text = "File";
            columnHeaderFile.Width = 120;
            //
            // contextMenuStripBodies
            //
            contextMenuStripBodies.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addBinMenuToolStripMenuItem, toolStripSeparatorBodies, saveToolStripMenuItem });
            contextMenuStripBodies.Name = "contextMenuStripBodies";
            //
            // addBinMenuToolStripMenuItem
            //
            addBinMenuToolStripMenuItem.Name = "addBinMenuToolStripMenuItem";
            addBinMenuToolStripMenuItem.Text = "Add .bin as new body / action...";
            addBinMenuToolStripMenuItem.Click += OnClickAddBin;
            //
            // toolStripSeparatorBodies
            //
            toolStripSeparatorBodies.Name = "toolStripSeparatorBodies";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // listViewActions
            //
            listViewActions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderAction, columnHeaderActionSize });
            listViewActions.ContextMenuStrip = contextMenuStripActions;
            listViewActions.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewActions.FullRowSelect = true;
            listViewActions.HideSelection = false;
            listViewActions.MultiSelect = false;
            listViewActions.Name = "listViewActions";
            listViewActions.TabIndex = 0;
            listViewActions.UseCompatibleStateImageBehavior = false;
            listViewActions.View = System.Windows.Forms.View.Details;
            listViewActions.SelectedIndexChanged += OnActionSelectedIndexChanged;
            //
            // columnHeaderAction
            //
            columnHeaderAction.Text = "Action";
            columnHeaderAction.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderAction.Width = 55;
            //
            // columnHeaderActionSize
            //
            columnHeaderActionSize.Text = "Size";
            columnHeaderActionSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderActionSize.Width = 70;
            //
            // contextMenuStripActions
            //
            contextMenuStripActions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exportPngMenuToolStripMenuItem, exportBinMenuToolStripMenuItem, toolStripSeparatorActions, importBinMenuToolStripMenuItem, importPngFolderMenuToolStripMenuItem });
            contextMenuStripActions.Name = "contextMenuStripActions";
            //
            // exportPngMenuToolStripMenuItem
            //
            exportPngMenuToolStripMenuItem.Name = "exportPngMenuToolStripMenuItem";
            exportPngMenuToolStripMenuItem.Text = "Export frames as PNG...";
            exportPngMenuToolStripMenuItem.Click += OnClickExportPng;
            //
            // exportBinMenuToolStripMenuItem
            //
            exportBinMenuToolStripMenuItem.Name = "exportBinMenuToolStripMenuItem";
            exportBinMenuToolStripMenuItem.Text = "Export as .bin (AMOU)...";
            exportBinMenuToolStripMenuItem.Click += OnClickExportBin;
            //
            // toolStripSeparatorActions
            //
            toolStripSeparatorActions.Name = "toolStripSeparatorActions";
            //
            // importBinMenuToolStripMenuItem
            //
            importBinMenuToolStripMenuItem.Name = "importBinMenuToolStripMenuItem";
            importBinMenuToolStripMenuItem.Text = "Replace with .bin (AMOU)...";
            importBinMenuToolStripMenuItem.Click += OnClickImportBin;
            //
            // importPngFolderMenuToolStripMenuItem
            //
            importPngFolderMenuToolStripMenuItem.Name = "importPngFolderMenuToolStripMenuItem";
            importPngFolderMenuToolStripMenuItem.Text = "Replace frames from PNG folder...";
            importPngFolderMenuToolStripMenuItem.Click += OnClickImportPngFolder;
            //
            // splitContainerFrames
            //
            splitContainerFrames.Panel1.Controls.Add(listViewFrames);
            splitContainerFrames.Panel2.Controls.Add(panelPreview);
            splitContainerFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerFrames.Name = "splitContainerFrames";
            splitContainerFrames.Size = new System.Drawing.Size(656, 550);
            splitContainerFrames.SplitterDistance = 250;
            splitContainerFrames.TabIndex = 1;
            //
            // listViewFrames
            //
            listViewFrames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderFrameNumber, columnHeaderDir, columnHeaderFrameSize, columnHeaderPosition });
            listViewFrames.ContextMenuStrip = contextMenuStripFrames;
            listViewFrames.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewFrames.FullRowSelect = true;
            listViewFrames.HideSelection = false;
            listViewFrames.MultiSelect = false;
            listViewFrames.Name = "listViewFrames";
            listViewFrames.TabIndex = 0;
            listViewFrames.UseCompatibleStateImageBehavior = false;
            listViewFrames.View = System.Windows.Forms.View.Details;
            listViewFrames.SelectedIndexChanged += OnFrameSelectedIndexChanged;
            //
            // columnHeaderFrameNumber
            //
            columnHeaderFrameNumber.Text = "#";
            columnHeaderFrameNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderFrameNumber.Width = 40;
            //
            // columnHeaderDir
            //
            columnHeaderDir.Text = "Dir";
            columnHeaderDir.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderDir.Width = 35;
            //
            // columnHeaderFrameSize
            //
            columnHeaderFrameSize.Text = "Size";
            columnHeaderFrameSize.Width = 70;
            //
            // columnHeaderPosition
            //
            columnHeaderPosition.Text = "Position";
            columnHeaderPosition.Width = 90;
            //
            // contextMenuStripFrames
            //
            contextMenuStripFrames.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exportFramePngMenuToolStripMenuItem, importFramePngMenuToolStripMenuItem });
            contextMenuStripFrames.Name = "contextMenuStripFrames";
            //
            // exportFramePngMenuToolStripMenuItem
            //
            exportFramePngMenuToolStripMenuItem.Name = "exportFramePngMenuToolStripMenuItem";
            exportFramePngMenuToolStripMenuItem.Text = "Export frame as PNG...";
            exportFramePngMenuToolStripMenuItem.Click += OnClickExportFramePng;
            //
            // importFramePngMenuToolStripMenuItem
            //
            importFramePngMenuToolStripMenuItem.Name = "importFramePngMenuToolStripMenuItem";
            importFramePngMenuToolStripMenuItem.Text = "Replace frame with PNG...";
            importFramePngMenuToolStripMenuItem.Click += OnClickImportFramePng;
            //
            // panelPreview
            //
            panelPreview.Controls.Add(previewPanel);
            panelPreview.Controls.Add(labelInfo);
            panelPreview.Controls.Add(flowLayoutPanelControls);
            panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPreview.Name = "panelPreview";
            panelPreview.TabIndex = 0;
            //
            // previewPanel
            //
            previewPanel.AutoScroll = true;
            previewPanel.BackColor = System.Drawing.Color.White;
            previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanel.Name = "previewPanel";
            previewPanel.TabIndex = 2;
            previewPanel.TabStop = true;
            previewPanel.Zoom = 2F;
            previewPanel.MouseDown += OnPreviewMouseDown;
            previewPanel.PreviewKeyDown += OnPreviewPreviewKeyDown;
            previewPanel.KeyDown += OnPreviewKeyDown;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(400, 36);
            //
            // flowLayoutPanelControls
            //
            flowLayoutPanelControls.Controls.Add(tableLayoutPanelCompass);
            flowLayoutPanelControls.Controls.Add(flowLayoutPanelOptions);
            flowLayoutPanelControls.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelControls.Name = "flowLayoutPanelControls";
            flowLayoutPanelControls.Padding = new System.Windows.Forms.Padding(2);
            flowLayoutPanelControls.Size = new System.Drawing.Size(400, 104);
            flowLayoutPanelControls.TabIndex = 0;
            //
            // tableLayoutPanelCompass
            //
            tableLayoutPanelCompass.Controls.Add(buttonViewUpLeft, 0, 0);
            tableLayoutPanelCompass.Controls.Add(buttonViewUp, 1, 0);
            tableLayoutPanelCompass.Controls.Add(buttonViewUpRight, 2, 0);
            tableLayoutPanelCompass.Controls.Add(buttonViewLeft, 0, 1);
            tableLayoutPanelCompass.Controls.Add(buttonTurn, 1, 1);
            tableLayoutPanelCompass.Controls.Add(buttonViewRight, 2, 1);
            tableLayoutPanelCompass.Controls.Add(buttonViewDownLeft, 0, 2);
            tableLayoutPanelCompass.Controls.Add(buttonViewDown, 1, 2);
            tableLayoutPanelCompass.Controls.Add(buttonViewDownRight, 2, 2);
            tableLayoutPanelCompass.ColumnCount = 3;
            tableLayoutPanelCompass.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            tableLayoutPanelCompass.Name = "tableLayoutPanelCompass";
            tableLayoutPanelCompass.RowCount = 3;
            tableLayoutPanelCompass.Size = new System.Drawing.Size(96, 96);
            tableLayoutPanelCompass.TabIndex = 0;
            tableLayoutPanelCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            tableLayoutPanelCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            //
            // buttonViewUp
            //
            buttonViewUp.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewUp.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewUp.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewUp.Name = "buttonViewUp";
            buttonViewUp.TabIndex = 0;
            buttonViewUp.Tag = "0";
            buttonViewUp.Text = "↑";
            buttonViewUp.UseVisualStyleBackColor = true;
            buttonViewUp.Click += OnClickView;
            //
            // buttonViewUpRight
            //
            buttonViewUpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewUpRight.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewUpRight.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewUpRight.Name = "buttonViewUpRight";
            buttonViewUpRight.TabIndex = 1;
            buttonViewUpRight.Tag = "1";
            buttonViewUpRight.Text = "↗";
            buttonViewUpRight.UseVisualStyleBackColor = true;
            buttonViewUpRight.Click += OnClickView;
            //
            // buttonViewRight
            //
            buttonViewRight.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewRight.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewRight.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewRight.Name = "buttonViewRight";
            buttonViewRight.TabIndex = 2;
            buttonViewRight.Tag = "2";
            buttonViewRight.Text = "→";
            buttonViewRight.UseVisualStyleBackColor = true;
            buttonViewRight.Click += OnClickView;
            //
            // buttonViewDownRight
            //
            buttonViewDownRight.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewDownRight.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewDownRight.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewDownRight.Name = "buttonViewDownRight";
            buttonViewDownRight.TabIndex = 3;
            buttonViewDownRight.Tag = "3";
            buttonViewDownRight.Text = "↘";
            buttonViewDownRight.UseVisualStyleBackColor = true;
            buttonViewDownRight.Click += OnClickView;
            //
            // buttonViewDown
            //
            buttonViewDown.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewDown.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewDown.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewDown.Name = "buttonViewDown";
            buttonViewDown.TabIndex = 4;
            buttonViewDown.Tag = "4";
            buttonViewDown.Text = "↓";
            buttonViewDown.UseVisualStyleBackColor = true;
            buttonViewDown.Click += OnClickView;
            //
            // buttonViewDownLeft
            //
            buttonViewDownLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewDownLeft.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewDownLeft.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewDownLeft.Name = "buttonViewDownLeft";
            buttonViewDownLeft.TabIndex = 5;
            buttonViewDownLeft.Tag = "5";
            buttonViewDownLeft.Text = "↙";
            buttonViewDownLeft.UseVisualStyleBackColor = true;
            buttonViewDownLeft.Click += OnClickView;
            //
            // buttonViewLeft
            //
            buttonViewLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewLeft.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewLeft.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewLeft.Name = "buttonViewLeft";
            buttonViewLeft.TabIndex = 6;
            buttonViewLeft.Tag = "6";
            buttonViewLeft.Text = "←";
            buttonViewLeft.UseVisualStyleBackColor = true;
            buttonViewLeft.Click += OnClickView;
            //
            // buttonViewUpLeft
            //
            buttonViewUpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonViewUpLeft.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonViewUpLeft.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonViewUpLeft.Name = "buttonViewUpLeft";
            buttonViewUpLeft.TabIndex = 7;
            buttonViewUpLeft.Tag = "7";
            buttonViewUpLeft.Text = "↖";
            buttonViewUpLeft.UseVisualStyleBackColor = true;
            buttonViewUpLeft.Click += OnClickView;
            //
            // buttonTurn
            //
            buttonTurn.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonTurn.Font = new System.Drawing.Font("Segoe UI", 11F);
            buttonTurn.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            buttonTurn.Name = "buttonTurn";
            buttonTurn.TabIndex = 8;
            buttonTurn.Text = "⟳";
            buttonTurn.UseVisualStyleBackColor = true;
            buttonTurn.MouseUp += OnTurnMouseUp;
            //
            // flowLayoutPanelOptions
            //
            flowLayoutPanelOptions.Controls.Add(flowLayoutPanelLine);
            flowLayoutPanelOptions.Controls.Add(checkBoxTurn);
            flowLayoutPanelOptions.Controls.Add(checkBoxGround);
            flowLayoutPanelOptions.AutoSize = true;
            flowLayoutPanelOptions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelOptions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelOptions.Name = "flowLayoutPanelOptions";
            flowLayoutPanelOptions.TabIndex = 1;
            flowLayoutPanelOptions.WrapContents = false;
            //
            // flowLayoutPanelLine
            //
            flowLayoutPanelLine.Controls.Add(labelStoredDir);
            flowLayoutPanelLine.Controls.Add(numericUpDownDirection);
            flowLayoutPanelLine.Controls.Add(labelFramesPerDir);
            flowLayoutPanelLine.Controls.Add(numericUpDownPerDirection);
            flowLayoutPanelLine.Controls.Add(labelMs);
            flowLayoutPanelLine.Controls.Add(numericUpDownSpeed);
            flowLayoutPanelLine.Controls.Add(buttonPlay);
            flowLayoutPanelLine.AutoSize = true;
            flowLayoutPanelLine.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            flowLayoutPanelLine.Name = "flowLayoutPanelLine";
            flowLayoutPanelLine.TabIndex = 0;
            flowLayoutPanelLine.WrapContents = false;
            //
            // labelStoredDir
            //
            labelStoredDir.AutoSize = true;
            labelStoredDir.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            labelStoredDir.Name = "labelStoredDir";
            labelStoredDir.Text = "Stored dir:";
            //
            // numericUpDownDirection
            //
            numericUpDownDirection.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            numericUpDownDirection.Name = "numericUpDownDirection";
            numericUpDownDirection.Size = new System.Drawing.Size(45, 23);
            numericUpDownDirection.TabIndex = 1;
            numericUpDownDirection.ValueChanged += OnDirectionChanged;
            //
            // labelFramesPerDir
            //
            labelFramesPerDir.AutoSize = true;
            labelFramesPerDir.Margin = new System.Windows.Forms.Padding(6, 8, 0, 0);
            labelFramesPerDir.Name = "labelFramesPerDir";
            labelFramesPerDir.Text = "Frames/dir:";
            //
            // numericUpDownPerDirection
            //
            numericUpDownPerDirection.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numericUpDownPerDirection.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownPerDirection.Name = "numericUpDownPerDirection";
            numericUpDownPerDirection.Size = new System.Drawing.Size(50, 23);
            numericUpDownPerDirection.TabIndex = 3;
            numericUpDownPerDirection.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownPerDirection.ValueChanged += OnPerDirectionChanged;
            //
            // labelMs
            //
            labelMs.AutoSize = true;
            labelMs.Margin = new System.Windows.Forms.Padding(6, 8, 0, 0);
            labelMs.Name = "labelMs";
            labelMs.Text = "ms:";
            //
            // numericUpDownSpeed
            //
            numericUpDownSpeed.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownSpeed.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numericUpDownSpeed.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numericUpDownSpeed.Name = "numericUpDownSpeed";
            numericUpDownSpeed.Size = new System.Drawing.Size(60, 23);
            numericUpDownSpeed.TabIndex = 5;
            numericUpDownSpeed.Value = new decimal(new int[] { 120, 0, 0, 0 });
            numericUpDownSpeed.ValueChanged += OnSpeedChanged;
            //
            // buttonPlay
            //
            buttonPlay.Name = "buttonPlay";
            buttonPlay.Size = new System.Drawing.Size(60, 23);
            buttonPlay.TabIndex = 6;
            buttonPlay.Text = "Play";
            buttonPlay.UseVisualStyleBackColor = true;
            buttonPlay.Click += OnClickPlay;
            //
            // checkBoxTurn
            //
            checkBoxTurn.AutoSize = true;
            checkBoxTurn.Margin = new System.Windows.Forms.Padding(3, 4, 0, 0);
            checkBoxTurn.Name = "checkBoxTurn";
            checkBoxTurn.TabIndex = 1;
            checkBoxTurn.Text = "Turn around while playing";
            checkBoxTurn.UseVisualStyleBackColor = true;
            //
            // checkBoxGround
            //
            checkBoxGround.AutoSize = true;
            checkBoxGround.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxGround.Checked = true;
            checkBoxGround.Margin = new System.Windows.Forms.Padding(3, 4, 0, 0);
            checkBoxGround.Name = "checkBoxGround";
            checkBoxGround.TabIndex = 2;
            checkBoxGround.Text = "Ground tile";
            checkBoxGround.UseVisualStyleBackColor = true;
            checkBoxGround.CheckedChanged += OnGroundChanged;
            //
            // toolTip
            //
            toolTip.SetToolTip(buttonTurn, "Turn right (right click: turn left). Keys: Left / Right in the preview");
            //
            // timerPlay
            //
            timerPlay.Interval = 120;
            timerPlay.Tick += OnTimerTick;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 2;
            //
            // toolStripStatusLabel
            //
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Spring = true;
            toolStripStatusLabel.Text = "Choose the Enhanced Client folder on top.";
            toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // EcMobileAnimationControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcMobileAnimationControl";
            Size = new System.Drawing.Size(1100, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerLists.Panel1.ResumeLayout(false);
            splitContainerLists.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLists).EndInit();
            splitContainerLists.ResumeLayout(false);
            contextMenuStripBodies.ResumeLayout(false);
            contextMenuStripActions.ResumeLayout(false);
            splitContainerFrames.Panel1.ResumeLayout(false);
            splitContainerFrames.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerFrames).EndInit();
            splitContainerFrames.ResumeLayout(false);
            contextMenuStripFrames.ResumeLayout(false);
            panelPreview.ResumeLayout(false);
            panelPreview.PerformLayout();
            flowLayoutPanelControls.ResumeLayout(false);
            flowLayoutPanelControls.PerformLayout();
            tableLayoutPanelCompass.ResumeLayout(false);
            tableLayoutPanelCompass.PerformLayout();
            flowLayoutPanelOptions.ResumeLayout(false);
            flowLayoutPanelOptions.PerformLayout();
            flowLayoutPanelLine.ResumeLayout(false);
            flowLayoutPanelLine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDirection).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownPerDirection).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownSpeed).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelBody;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownExport;
        private System.Windows.Forms.ToolStripMenuItem exportPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFramePngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBinToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownImport;
        private System.Windows.Forms.ToolStripMenuItem importFramePngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importPngFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorImport;
        private System.Windows.Forms.ToolStripMenuItem importBinToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBinToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLists;
        private System.Windows.Forms.ListView listViewBodies;
        private System.Windows.Forms.ColumnHeader columnHeaderBody;
        private System.Windows.Forms.ColumnHeader columnHeaderBodyHex;
        private System.Windows.Forms.ColumnHeader columnHeaderActions;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripBodies;
        private System.Windows.Forms.ToolStripMenuItem addBinMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorBodies;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ListView listViewActions;
        private System.Windows.Forms.ColumnHeader columnHeaderAction;
        private System.Windows.Forms.ColumnHeader columnHeaderActionSize;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripActions;
        private System.Windows.Forms.ToolStripMenuItem exportPngMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBinMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorActions;
        private System.Windows.Forms.ToolStripMenuItem importBinMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importPngFolderMenuToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerFrames;
        private System.Windows.Forms.ListView listViewFrames;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderDir;
        private System.Windows.Forms.ColumnHeader columnHeaderFrameSize;
        private System.Windows.Forms.ColumnHeader columnHeaderPosition;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFrames;
        private System.Windows.Forms.ToolStripMenuItem exportFramePngMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFramePngMenuToolStripMenuItem;
        private System.Windows.Forms.Panel panelPreview;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelControls;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelCompass;
        private System.Windows.Forms.Button buttonViewUp;
        private System.Windows.Forms.Button buttonViewUpRight;
        private System.Windows.Forms.Button buttonViewRight;
        private System.Windows.Forms.Button buttonViewDownRight;
        private System.Windows.Forms.Button buttonViewDown;
        private System.Windows.Forms.Button buttonViewDownLeft;
        private System.Windows.Forms.Button buttonViewLeft;
        private System.Windows.Forms.Button buttonViewUpLeft;
        private System.Windows.Forms.Button buttonTurn;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelOptions;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLine;
        private System.Windows.Forms.Label labelStoredDir;
        private System.Windows.Forms.NumericUpDown numericUpDownDirection;
        private System.Windows.Forms.Label labelFramesPerDir;
        private System.Windows.Forms.NumericUpDown numericUpDownPerDirection;
        private System.Windows.Forms.Label labelMs;
        private System.Windows.Forms.NumericUpDown numericUpDownSpeed;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.CheckBox checkBoxTurn;
        private System.Windows.Forms.CheckBox checkBoxGround;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer timerPlay;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

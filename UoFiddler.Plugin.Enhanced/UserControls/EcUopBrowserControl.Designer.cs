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
    partial class EcUopBrowserControl
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
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            groupBoxFiles = new System.Windows.Forms.GroupBox();
            listBoxFiles = new System.Windows.Forms.ListBox();
            panelRight = new System.Windows.Forms.Panel();
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripLabelFolder = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxFolder = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonResolveNames = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainerList = new System.Windows.Forms.SplitContainer();
            listViewEntries = new System.Windows.Forms.ListView();
            columnHeaderNumber = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderStored = new System.Windows.Forms.ColumnHeader();
            columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            columnHeaderPacked = new System.Windows.Forms.ColumnHeader();
            contextMenuStripEntries = new System.Windows.Forms.ContextMenuStrip(components);
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            exportSelectedMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            copyNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyHashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelPreview = new System.Windows.Forms.Panel();
            previewPanel = new EcPreviewPanel();
            textBoxHex = new System.Windows.Forms.TextBox();
            labelInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            groupBoxFiles.SuspendLayout();
            panelRight.SuspendLayout();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerList).BeginInit();
            splitContainerList.Panel1.SuspendLayout();
            splitContainerList.Panel2.SuspendLayout();
            splitContainerList.SuspendLayout();
            contextMenuStripEntries.SuspendLayout();
            panelPreview.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // splitContainerMain
            //
            splitContainerMain.Panel1.Controls.Add(groupBoxFiles);
            splitContainerMain.Panel2.Controls.Add(panelRight);
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Size = new System.Drawing.Size(900, 450);
            splitContainerMain.SplitterDistance = 220;
            splitContainerMain.TabIndex = 1;
            //
            // groupBoxFiles
            //
            groupBoxFiles.Controls.Add(listBoxFiles);
            groupBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxFiles.Name = "groupBoxFiles";
            groupBoxFiles.TabIndex = 0;
            groupBoxFiles.TabStop = false;
            groupBoxFiles.Text = "UOP files";
            //
            // listBoxFiles
            //
            listBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxFiles.IntegralHeight = false;
            listBoxFiles.Name = "listBoxFiles";
            listBoxFiles.TabIndex = 0;
            listBoxFiles.SelectedIndexChanged += OnFileSelectedIndexChanged;
            //
            // panelRight
            //
            panelRight.Controls.Add(splitContainerList);
            panelRight.Controls.Add(toolStrip);
            panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRight.Name = "panelRight";
            panelRight.TabIndex = 0;
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelFolder, toolStripComboBoxFolder, toolStripLabelFilter, toolStripTextBoxFilter, toolStripButtonApply, toolStripButtonOpen, toolStripSeparator1, toolStripButtonResolveNames, toolStripSeparator2, toolStripDropDownExport, toolStripButtonReplace, toolStripButtonAdd, toolStripButtonRemove, toolStripSeparator3, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelFolder
            //
            toolStripLabelFolder.Name = "toolStripLabelFolder";
            toolStripLabelFolder.Text = "Folder:";
            //
            // toolStripComboBoxFolder
            //
            toolStripComboBoxFolder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFolder.MaxDropDownItems = 30;
            toolStripComboBoxFolder.Name = "toolStripComboBoxFolder";
            toolStripComboBoxFolder.Size = new System.Drawing.Size(260, 25);
            toolStripComboBoxFolder.ToolTipText = "Show only one folder";
            toolStripComboBoxFolder.SelectedIndexChanged += OnFolderChanged;
            //
            // toolStripLabelFilter
            //
            toolStripLabelFilter.Name = "toolStripLabelFilter";
            toolStripLabelFilter.Text = "Filter:";
            //
            // toolStripTextBoxFilter
            //
            toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            toolStripTextBoxFilter.Size = new System.Drawing.Size(160, 25);
            toolStripTextBoxFilter.ToolTipText = "Filter by name or hash – Enter";
            toolStripTextBoxFilter.KeyDown += OnFilterKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripButtonOpen
            //
            toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOpen.Name = "toolStripButtonOpen";
            toolStripButtonOpen.Text = "Open";
            toolStripButtonOpen.ToolTipText = "Open the selected entry with the Windows program for its type (play mp3/wav, edit xml/lua, ...)";
            toolStripButtonOpen.Click += OnClickOpen;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonResolveNames
            //
            toolStripButtonResolveNames.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonResolveNames.Name = "toolStripButtonResolveNames";
            toolStripButtonResolveNames.Text = "Resolve names...";
            toolStripButtonResolveNames.ToolTipText = "Find the file names of the entries with a name pattern like build/worldart/{0:D8}.dds";
            toolStripButtonResolveNames.Click += OnClickResolveNames;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripDropDownExport
            //
            toolStripDropDownExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportSelectedToolStripMenuItem, exportAllToolStripMenuItem, exportListToolStripMenuItem });
            toolStripDropDownExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownExport.Name = "toolStripDropDownExport";
            toolStripDropDownExport.Text = "Export";
            //
            // exportSelectedToolStripMenuItem
            //
            exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
            exportSelectedToolStripMenuItem.Text = "Selected entries...";
            exportSelectedToolStripMenuItem.Click += OnClickExportSelected;
            //
            // exportAllToolStripMenuItem
            //
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Text = "All entries...";
            exportAllToolStripMenuItem.Click += OnClickExportAll;
            //
            // exportListToolStripMenuItem
            //
            exportListToolStripMenuItem.Name = "exportListToolStripMenuItem";
            exportListToolStripMenuItem.Text = "Entry list (csv)...";
            exportListToolStripMenuItem.Click += OnClickExportList;
            //
            // toolStripButtonReplace
            //
            toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReplace.Name = "toolStripButtonReplace";
            toolStripButtonReplace.Text = "Replace...";
            toolStripButtonReplace.ToolTipText = "Replace the data of the selected entry with a file";
            toolStripButtonReplace.Click += OnClickReplace;
            //
            // toolStripButtonAdd
            //
            toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAdd.Name = "toolStripButtonAdd";
            toolStripButtonAdd.Text = "Add...";
            toolStripButtonAdd.ToolTipText = "Add a file under a packed name";
            toolStripButtonAdd.Click += OnClickAdd;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Enabled = false;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save file";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // splitContainerList
            //
            splitContainerList.Panel1.Controls.Add(listViewEntries);
            splitContainerList.Panel2.Controls.Add(panelPreview);
            splitContainerList.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerList.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainerList.Name = "splitContainerList";
            splitContainerList.Size = new System.Drawing.Size(680, 450);
            splitContainerList.SplitterDistance = 300;
            splitContainerList.TabIndex = 1;
            //
            // listViewEntries
            //
            listViewEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderNumber, columnHeaderName, columnHeaderStored, columnHeaderSize, columnHeaderPacked });
            listViewEntries.ContextMenuStrip = contextMenuStripEntries;
            listViewEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewEntries.FullRowSelect = true;
            listViewEntries.HideSelection = false;
            listViewEntries.Name = "listViewEntries";
            listViewEntries.TabIndex = 0;
            listViewEntries.UseCompatibleStateImageBehavior = false;
            listViewEntries.View = System.Windows.Forms.View.Details;
            listViewEntries.VirtualMode = true;
            listViewEntries.RetrieveVirtualItem += RetrieveItem;
            listViewEntries.SelectedIndexChanged += OnEntrySelectedIndexChanged;
            //
            // columnHeaderNumber
            //
            columnHeaderNumber.Text = "#";
            columnHeaderNumber.Width = 60;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name / hash";
            columnHeaderName.Width = 280;
            //
            // columnHeaderStored
            //
            columnHeaderStored.Text = "Stored";
            columnHeaderStored.Width = 70;
            //
            // columnHeaderSize
            //
            columnHeaderSize.Text = "Size";
            columnHeaderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderSize.Width = 80;
            //
            // columnHeaderPacked
            //
            columnHeaderPacked.Text = "Packed";
            columnHeaderPacked.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderPacked.Width = 80;
            //
            // contextMenuStripEntries
            //
            contextMenuStripEntries.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, toolStripSeparatorMenu1, exportSelectedMenuToolStripMenuItem, replaceToolStripMenuItem, addToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu2, copyNameToolStripMenuItem, copyHashToolStripMenuItem, toolStripSeparatorMenu3, saveToolStripMenuItem });
            contextMenuStripEntries.Name = "contextMenuStripEntries";
            //
            // openToolStripMenuItem
            //
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Text = "Open with Windows";
            openToolStripMenuItem.Click += OnClickOpen;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // exportSelectedMenuToolStripMenuItem
            //
            exportSelectedMenuToolStripMenuItem.Name = "exportSelectedMenuToolStripMenuItem";
            exportSelectedMenuToolStripMenuItem.Text = "Export selected...";
            exportSelectedMenuToolStripMenuItem.Click += OnClickExportSelected;
            //
            // replaceToolStripMenuItem
            //
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Text = "Replace...";
            replaceToolStripMenuItem.Click += OnClickReplace;
            //
            // addToolStripMenuItem
            //
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Text = "Add...";
            addToolStripMenuItem.Click += OnClickAdd;
            //
            // removeToolStripMenuItem
            //
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // copyNameToolStripMenuItem
            //
            copyNameToolStripMenuItem.Name = "copyNameToolStripMenuItem";
            copyNameToolStripMenuItem.Text = "Copy name";
            copyNameToolStripMenuItem.Click += OnClickCopyName;
            //
            // copyHashToolStripMenuItem
            //
            copyHashToolStripMenuItem.Name = "copyHashToolStripMenuItem";
            copyHashToolStripMenuItem.Text = "Copy hash";
            copyHashToolStripMenuItem.Click += OnClickCopyHash;
            //
            // toolStripSeparatorMenu3
            //
            toolStripSeparatorMenu3.Name = "toolStripSeparatorMenu3";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save file";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelPreview
            //
            panelPreview.Controls.Add(previewPanel);
            panelPreview.Controls.Add(textBoxHex);
            panelPreview.Controls.Add(labelInfo);
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
            previewPanel.TabIndex = 0;
            //
            // textBoxHex
            //
            textBoxHex.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxHex.Font = new System.Drawing.Font("Consolas", 8.5F);
            textBoxHex.Multiline = true;
            textBoxHex.Name = "textBoxHex";
            textBoxHex.ReadOnly = true;
            textBoxHex.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxHex.TabIndex = 1;
            textBoxHex.Visible = false;
            textBoxHex.WordWrap = false;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(370, 64);
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
            // EcUopBrowserControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(statusStrip);
            Name = "EcUopBrowserControl";
            Size = new System.Drawing.Size(900, 500);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            groupBoxFiles.ResumeLayout(false);
            groupBoxFiles.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerList.Panel1.ResumeLayout(false);
            splitContainerList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerList).EndInit();
            splitContainerList.ResumeLayout(false);
            contextMenuStripEntries.ResumeLayout(false);
            panelPreview.ResumeLayout(false);
            panelPreview.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.GroupBox groupBoxFiles;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFolder;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFolder;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFilter;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonResolveNames;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownExport;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportListToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainerList;
        private System.Windows.Forms.ListView listViewEntries;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderStored;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderPacked;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripEntries;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem copyNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyHashToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelPreview;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.TextBox textBoxHex;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

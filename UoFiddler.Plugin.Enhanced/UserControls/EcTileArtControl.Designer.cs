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
    partial class EcTileArtControl
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
            toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripComboBoxRange = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonDuplicate = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownEntry = new System.Windows.Forms.ToolStripDropDownButton();
            exportEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorEntry = new System.Windows.Forms.ToolStripSeparator();
            exportCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            listViewTiles = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderHex = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderFlags = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            duplicateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            exportEntryMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importEntryMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            copyIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainerDetail = new System.Windows.Forms.SplitContainer();
            propertyGrid = new System.Windows.Forms.PropertyGrid();
            tabControlDetail = new System.Windows.Forms.TabControl();
            tabPageTextures = new System.Windows.Forms.TabPage();
            previewPanelTexture = new EcPreviewPanel();
            labelTextureInfo = new System.Windows.Forms.Label();
            listViewTextures = new System.Windows.Forms.ListView();
            columnHeaderSlot = new System.Windows.Forms.ColumnHeader();
            columnHeaderImage = new System.Windows.Forms.ColumnHeader();
            columnHeaderIndex = new System.Windows.Forms.ColumnHeader();
            toolStripTextures = new System.Windows.Forms.ToolStrip();
            toolStripButtonChangeTexture = new System.Windows.Forms.ToolStripButton();
            toolStripButtonShowInArt = new System.Windows.Forms.ToolStripButton();
            tabPageFlags = new System.Windows.Forms.TabPage();
            checkedListBoxFlags = new System.Windows.Forms.CheckedListBox();
            tabPageFlags2 = new System.Windows.Forms.TabPage();
            checkedListBoxFlags2 = new System.Windows.Forms.CheckedListBox();
            labelFlags2 = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).BeginInit();
            splitContainerDetail.Panel1.SuspendLayout();
            splitContainerDetail.Panel2.SuspendLayout();
            splitContainerDetail.SuspendLayout();
            tabControlDetail.SuspendLayout();
            tabPageTextures.SuspendLayout();
            toolStripTextures.SuspendLayout();
            tabPageFlags.SuspendLayout();
            tabPageFlags2.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxFilter, toolStripButtonApply, toolStripComboBoxRange, toolStripSeparator1, toolStripButtonDuplicate, toolStripButtonRemove, toolStripDropDownEntry, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Search:";
            //
            // toolStripTextBoxFilter
            //
            toolStripTextBoxFilter.Name = "toolStripTextBoxFilter";
            toolStripTextBoxFilter.Size = new System.Drawing.Size(160, 25);
            toolStripTextBoxFilter.ToolTipText = "Id (decimal / 0x hex) or part of the name – Enter";
            toolStripTextBoxFilter.KeyDown += OnFilterKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripComboBoxRange
            //
            toolStripComboBoxRange.Items.AddRange(new object[] { "All entries", "0x0000 - 0x3FFF", "0x4000 - 0xFFFF", ">= 0x10000", "EC only (>= 10000000)", "Modified", "Id differs from file" });
            toolStripComboBoxRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxRange.Name = "toolStripComboBoxRange";
            toolStripComboBoxRange.Size = new System.Drawing.Size(150, 25);
            toolStripComboBoxRange.SelectedIndexChanged += OnRangeChanged;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonDuplicate
            //
            toolStripButtonDuplicate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonDuplicate.Name = "toolStripButtonDuplicate";
            toolStripButtonDuplicate.Text = "Duplicate...";
            toolStripButtonDuplicate.ToolTipText = "Create a new entry as copy of the selected one";
            toolStripButtonDuplicate.Click += OnClickDuplicate;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripDropDownEntry
            //
            toolStripDropDownEntry.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportEntryToolStripMenuItem, importEntryToolStripMenuItem, toolStripSeparatorEntry, exportCsvToolStripMenuItem });
            toolStripDropDownEntry.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownEntry.Name = "toolStripDropDownEntry";
            toolStripDropDownEntry.Text = "Entry";
            //
            // exportEntryToolStripMenuItem
            //
            exportEntryToolStripMenuItem.Name = "exportEntryToolStripMenuItem";
            exportEntryToolStripMenuItem.Text = "Export entry (.bin)...";
            exportEntryToolStripMenuItem.Click += OnClickExportEntry;
            //
            // importEntryToolStripMenuItem
            //
            importEntryToolStripMenuItem.Name = "importEntryToolStripMenuItem";
            importEntryToolStripMenuItem.Text = "Import entry (.bin) into selected...";
            importEntryToolStripMenuItem.Click += OnClickImportEntry;
            //
            // toolStripSeparatorEntry
            //
            toolStripSeparatorEntry.Name = "toolStripSeparatorEntry";
            //
            // exportCsvToolStripMenuItem
            //
            exportCsvToolStripMenuItem.Name = "exportCsvToolStripMenuItem";
            exportCsvToolStripMenuItem.Text = "Export list (csv)...";
            exportCsvToolStripMenuItem.Click += OnClickExportCsv;
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
            toolStripButtonSave.ToolTipText = "Save tileart.uop (and string_dictionary.uop if names were added)";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripButtonSaveAs
            //
            toolStripButtonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSaveAs.Name = "toolStripButtonSaveAs";
            toolStripButtonSaveAs.Text = "Save as...";
            toolStripButtonSaveAs.ToolTipText = "Write tileart.uop (and string_dictionary.uop) into another folder";
            toolStripButtonSaveAs.Click += OnClickSaveAs;
            //
            // splitContainerMain
            //
            splitContainerMain.Panel1.Controls.Add(listViewTiles);
            splitContainerMain.Panel2.Controls.Add(splitContainerDetail);
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Size = new System.Drawing.Size(900, 450);
            splitContainerMain.SplitterDistance = 350;
            splitContainerMain.TabIndex = 1;
            //
            // listViewTiles
            //
            listViewTiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderHex, columnHeaderName, columnHeaderFlags });
            listViewTiles.ContextMenuStrip = contextMenuStripList;
            listViewTiles.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewTiles.FullRowSelect = true;
            listViewTiles.HideSelection = false;
            listViewTiles.MultiSelect = false;
            listViewTiles.Name = "listViewTiles";
            listViewTiles.TabIndex = 0;
            listViewTiles.UseCompatibleStateImageBehavior = false;
            listViewTiles.View = System.Windows.Forms.View.Details;
            listViewTiles.VirtualMode = true;
            listViewTiles.RetrieveVirtualItem += RetrieveItem;
            listViewTiles.SelectedIndexChanged += OnListSelectedIndexChanged;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderId.Width = 70;
            //
            // columnHeaderHex
            //
            columnHeaderHex.Text = "Hex";
            columnHeaderHex.Width = 70;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 200;
            //
            // columnHeaderFlags
            //
            columnHeaderFlags.Text = "Flags";
            columnHeaderFlags.Width = 220;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { duplicateToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu1, exportEntryMenuToolStripMenuItem, importEntryMenuToolStripMenuItem, toolStripSeparatorMenu2, copyIdToolStripMenuItem, copyNameToolStripMenuItem, toolStripSeparatorMenu3, saveToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // duplicateToolStripMenuItem
            //
            duplicateToolStripMenuItem.Name = "duplicateToolStripMenuItem";
            duplicateToolStripMenuItem.Text = "Duplicate...";
            duplicateToolStripMenuItem.Click += OnClickDuplicate;
            //
            // removeToolStripMenuItem
            //
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
            //
            // exportEntryMenuToolStripMenuItem
            //
            exportEntryMenuToolStripMenuItem.Name = "exportEntryMenuToolStripMenuItem";
            exportEntryMenuToolStripMenuItem.Text = "Export entry (.bin)...";
            exportEntryMenuToolStripMenuItem.Click += OnClickExportEntry;
            //
            // importEntryMenuToolStripMenuItem
            //
            importEntryMenuToolStripMenuItem.Name = "importEntryMenuToolStripMenuItem";
            importEntryMenuToolStripMenuItem.Text = "Import entry (.bin)...";
            importEntryMenuToolStripMenuItem.Click += OnClickImportEntry;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // copyIdToolStripMenuItem
            //
            copyIdToolStripMenuItem.Name = "copyIdToolStripMenuItem";
            copyIdToolStripMenuItem.Text = "Copy id (hex)";
            copyIdToolStripMenuItem.Click += OnClickCopyId;
            //
            // copyNameToolStripMenuItem
            //
            copyNameToolStripMenuItem.Name = "copyNameToolStripMenuItem";
            copyNameToolStripMenuItem.Text = "Copy name";
            copyNameToolStripMenuItem.Click += OnClickCopyName;
            //
            // toolStripSeparatorMenu3
            //
            toolStripSeparatorMenu3.Name = "toolStripSeparatorMenu3";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // splitContainerDetail
            //
            splitContainerDetail.Panel1.Controls.Add(propertyGrid);
            splitContainerDetail.Panel2.Controls.Add(tabControlDetail);
            splitContainerDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerDetail.Name = "splitContainerDetail";
            splitContainerDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            splitContainerDetail.Size = new System.Drawing.Size(540, 450);
            splitContainerDetail.SplitterDistance = 220;
            splitContainerDetail.TabIndex = 1;
            //
            // propertyGrid
            //
            propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            propertyGrid.Name = "propertyGrid";
            propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            propertyGrid.TabIndex = 0;
            propertyGrid.ToolbarVisible = false;
            propertyGrid.PropertyValueChanged += OnPropertyValueChanged;
            //
            // tabControlDetail
            //
            tabControlDetail.Controls.Add(tabPageTextures);
            tabControlDetail.Controls.Add(tabPageFlags);
            tabControlDetail.Controls.Add(tabPageFlags2);
            tabControlDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlDetail.Name = "tabControlDetail";
            tabControlDetail.SelectedIndex = 0;
            tabControlDetail.TabIndex = 0;
            //
            // tabPageTextures
            //
            tabPageTextures.Controls.Add(previewPanelTexture);
            tabPageTextures.Controls.Add(labelTextureInfo);
            tabPageTextures.Controls.Add(listViewTextures);
            tabPageTextures.Controls.Add(toolStripTextures);
            tabPageTextures.Location = new System.Drawing.Point(4, 24);
            tabPageTextures.Name = "tabPageTextures";
            tabPageTextures.Padding = new System.Windows.Forms.Padding(3);
            tabPageTextures.Size = new System.Drawing.Size(500, 300);
            tabPageTextures.TabIndex = 0;
            tabPageTextures.Text = "Textures";
            tabPageTextures.UseVisualStyleBackColor = true;
            //
            // previewPanelTexture
            //
            previewPanelTexture.AutoScroll = true;
            previewPanelTexture.BackColor = System.Drawing.Color.White;
            previewPanelTexture.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanelTexture.Name = "previewPanelTexture";
            previewPanelTexture.TabIndex = 3;
            //
            // labelTextureInfo
            //
            labelTextureInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelTextureInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelTextureInfo.Name = "labelTextureInfo";
            labelTextureInfo.Padding = new System.Windows.Forms.Padding(4);
            labelTextureInfo.Size = new System.Drawing.Size(494, 44);
            //
            // listViewTextures
            //
            listViewTextures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderSlot, columnHeaderImage, columnHeaderIndex });
            listViewTextures.Dock = System.Windows.Forms.DockStyle.Top;
            listViewTextures.FullRowSelect = true;
            listViewTextures.HideSelection = false;
            listViewTextures.MultiSelect = false;
            listViewTextures.Name = "listViewTextures";
            listViewTextures.Size = new System.Drawing.Size(494, 150);
            listViewTextures.TabIndex = 1;
            listViewTextures.UseCompatibleStateImageBehavior = false;
            listViewTextures.View = System.Windows.Forms.View.Details;
            listViewTextures.SelectedIndexChanged += OnTextureSelectedIndexChanged;
            listViewTextures.DoubleClick += OnClickChangeTexture;
            //
            // columnHeaderSlot
            //
            columnHeaderSlot.Text = "Slot";
            columnHeaderSlot.Width = 40;
            //
            // columnHeaderImage
            //
            columnHeaderImage.Text = "Image (string dictionary)";
            columnHeaderImage.Width = 300;
            //
            // columnHeaderIndex
            //
            columnHeaderIndex.Text = "Index";
            columnHeaderIndex.Width = 70;
            //
            // toolStripTextures
            //
            toolStripTextures.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonChangeTexture, toolStripButtonShowInArt });
            toolStripTextures.Dock = System.Windows.Forms.DockStyle.Top;
            toolStripTextures.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripTextures.Name = "toolStripTextures";
            toolStripTextures.TabIndex = 0;
            //
            // toolStripButtonChangeTexture
            //
            toolStripButtonChangeTexture.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonChangeTexture.Name = "toolStripButtonChangeTexture";
            toolStripButtonChangeTexture.Text = "Change image...";
            toolStripButtonChangeTexture.ToolTipText = "Point the reference to another image path, e.g. Data\\WorldArt\\00012345_name.tga";
            toolStripButtonChangeTexture.Click += OnClickChangeTexture;
            //
            // toolStripButtonShowInArt
            //
            toolStripButtonShowInArt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonShowInArt.Name = "toolStripButtonShowInArt";
            toolStripButtonShowInArt.Text = "Show in Art tab";
            toolStripButtonShowInArt.Click += OnClickShowInArt;
            //
            // tabPageFlags
            //
            tabPageFlags.Controls.Add(checkedListBoxFlags);
            tabPageFlags.Location = new System.Drawing.Point(4, 24);
            tabPageFlags.Name = "tabPageFlags";
            tabPageFlags.Padding = new System.Windows.Forms.Padding(3);
            tabPageFlags.Size = new System.Drawing.Size(500, 300);
            tabPageFlags.TabIndex = 1;
            tabPageFlags.Text = "Flags (EC)";
            tabPageFlags.UseVisualStyleBackColor = true;
            //
            // checkedListBoxFlags
            //
            checkedListBoxFlags.CheckOnClick = true;
            checkedListBoxFlags.ColumnWidth = 130;
            checkedListBoxFlags.Dock = System.Windows.Forms.DockStyle.Fill;
            checkedListBoxFlags.IntegralHeight = false;
            checkedListBoxFlags.MultiColumn = true;
            checkedListBoxFlags.Name = "checkedListBoxFlags";
            checkedListBoxFlags.TabIndex = 0;
            checkedListBoxFlags.ItemCheck += FlagChecked;
            //
            // tabPageFlags2
            //
            tabPageFlags2.Controls.Add(checkedListBoxFlags2);
            tabPageFlags2.Controls.Add(labelFlags2);
            tabPageFlags2.Location = new System.Drawing.Point(4, 24);
            tabPageFlags2.Name = "tabPageFlags2";
            tabPageFlags2.Padding = new System.Windows.Forms.Padding(3);
            tabPageFlags2.Size = new System.Drawing.Size(500, 300);
            tabPageFlags2.TabIndex = 2;
            tabPageFlags2.Text = "Flags 2 (classic)";
            tabPageFlags2.UseVisualStyleBackColor = true;
            //
            // checkedListBoxFlags2
            //
            checkedListBoxFlags2.CheckOnClick = true;
            checkedListBoxFlags2.ColumnWidth = 130;
            checkedListBoxFlags2.Dock = System.Windows.Forms.DockStyle.Fill;
            checkedListBoxFlags2.IntegralHeight = false;
            checkedListBoxFlags2.MultiColumn = true;
            checkedListBoxFlags2.Name = "checkedListBoxFlags2";
            checkedListBoxFlags2.TabIndex = 0;
            checkedListBoxFlags2.ItemCheck += Flag2Checked;
            //
            // labelFlags2
            //
            labelFlags2.Dock = System.Windows.Forms.DockStyle.Top;
            labelFlags2.Name = "labelFlags2";
            labelFlags2.Size = new System.Drawing.Size(494, 32);
            labelFlags2.Text = "Second flag field: same bits as the flags of the classic tiledata.mul (in 2/3 of the entries both fields are equal).";
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
            // EcTileArtControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcTileArtControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            contextMenuStripList.ResumeLayout(false);
            splitContainerDetail.Panel1.ResumeLayout(false);
            splitContainerDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerDetail).EndInit();
            splitContainerDetail.ResumeLayout(false);
            tabControlDetail.ResumeLayout(false);
            tabPageTextures.ResumeLayout(false);
            tabPageTextures.PerformLayout();
            toolStripTextures.ResumeLayout(false);
            toolStripTextures.PerformLayout();
            tabPageFlags.ResumeLayout(false);
            tabPageFlags.PerformLayout();
            tabPageFlags2.ResumeLayout(false);
            tabPageFlags2.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxRange;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonDuplicate;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownEntry;
        private System.Windows.Forms.ToolStripMenuItem exportEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorEntry;
        private System.Windows.Forms.ToolStripMenuItem exportCsvToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.ListView listViewTiles;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderHex;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFlags;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem duplicateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem exportEntryMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importEntryMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem copyIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyNameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerDetail;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.TabControl tabControlDetail;
        private System.Windows.Forms.TabPage tabPageTextures;
        private EcPreviewPanel previewPanelTexture;
        private System.Windows.Forms.Label labelTextureInfo;
        private System.Windows.Forms.ListView listViewTextures;
        private System.Windows.Forms.ColumnHeader columnHeaderSlot;
        private System.Windows.Forms.ColumnHeader columnHeaderImage;
        private System.Windows.Forms.ColumnHeader columnHeaderIndex;
        private System.Windows.Forms.ToolStrip toolStripTextures;
        private System.Windows.Forms.ToolStripButton toolStripButtonChangeTexture;
        private System.Windows.Forms.ToolStripButton toolStripButtonShowInArt;
        private System.Windows.Forms.TabPage tabPageFlags;
        private System.Windows.Forms.CheckedListBox checkedListBoxFlags;
        private System.Windows.Forms.TabPage tabPageFlags2;
        private System.Windows.Forms.CheckedListBox checkedListBoxFlags2;
        private System.Windows.Forms.Label labelFlags2;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

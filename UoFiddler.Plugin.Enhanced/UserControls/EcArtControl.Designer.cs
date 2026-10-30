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
    partial class EcArtControl
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
            toolStripLabelSource = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxSource = new System.Windows.Forms.ToolStripComboBox();
            toolStripComboBoxFile = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelId = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelSize = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxSize = new System.Windows.Forms.ToolStripComboBox();
            toolStripButtonUnnamed = new System.Windows.Forms.ToolStripButton();
            toolStripButtonScanIds = new System.Windows.Forms.ToolStripButton();
            toolStripEdit = new System.Windows.Forms.ToolStrip();
            toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            toolStripButtonInsert = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelImportAs = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxFormat = new System.Windows.Forms.ToolStripComboBox();
            toolStripButtonMipmaps = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportDdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorExport = new System.Windows.Forms.ToolStripSeparator();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            thumbnailGrid = new EcThumbnailGrid();
            contextMenuStripGrid = new System.Windows.Forms.ContextMenuStrip(components);
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportPngMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportDdsMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelRight = new System.Windows.Forms.Panel();
            previewPanel = new EcPreviewPanel();
            toolStripZoom = new System.Windows.Forms.ToolStrip();
            toolStripButtonZoomOut = new System.Windows.Forms.ToolStripButton();
            toolStripButtonZoom1 = new System.Windows.Forms.ToolStripButton();
            toolStripButtonZoomIn = new System.Windows.Forms.ToolStripButton();
            toolStripLabelZoomHint = new System.Windows.Forms.ToolStripLabel();
            labelInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            toolStripEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripGrid.SuspendLayout();
            panelRight.SuspendLayout();
            toolStripZoom.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSource, toolStripComboBoxSource, toolStripComboBoxFile, toolStripSeparator1, toolStripLabelId, toolStripTextBoxSearch, toolStripButtonSearch, toolStripSeparator2, toolStripLabelSize, toolStripComboBoxSize, toolStripButtonUnnamed, toolStripButtonScanIds });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelSource
            //
            toolStripLabelSource.Name = "toolStripLabelSource";
            toolStripLabelSource.Text = "Source:";
            //
            // toolStripComboBoxSource
            //
            toolStripComboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxSource.Name = "toolStripComboBoxSource";
            toolStripComboBoxSource.Size = new System.Drawing.Size(230, 25);
            toolStripComboBoxSource.ToolTipText = "Kind of image";
            toolStripComboBoxSource.SelectedIndexChanged += OnSourceChanged;
            //
            // toolStripComboBoxFile
            //
            toolStripComboBoxFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFile.Name = "toolStripComboBoxFile";
            toolStripComboBoxFile.Size = new System.Drawing.Size(160, 25);
            toolStripComboBoxFile.ToolTipText = "UOP file the images are read from";
            toolStripComboBoxFile.SelectedIndexChanged += OnFileChanged;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripLabelId
            //
            toolStripLabelId.Name = "toolStripLabelId";
            toolStripLabelId.Text = "Id:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(90, 25);
            toolStripTextBoxSearch.ToolTipText = "Id (decimal or 0x hex) – Enter to search";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonSearch
            //
            toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSearch.Name = "toolStripButtonSearch";
            toolStripButtonSearch.Text = "Search";
            toolStripButtonSearch.Click += OnClickSearch;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripLabelSize
            //
            toolStripLabelSize.Name = "toolStripLabelSize";
            toolStripLabelSize.Text = "Size:";
            //
            // toolStripComboBoxSize
            //
            toolStripComboBoxSize.Items.AddRange(new object[] { "48", "64", "96", "128", "192" });
            toolStripComboBoxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxSize.Name = "toolStripComboBoxSize";
            toolStripComboBoxSize.Size = new System.Drawing.Size(60, 25);
            toolStripComboBoxSize.ToolTipText = "Thumbnail size";
            toolStripComboBoxSize.SelectedIndexChanged += OnSizeChanged;
            //
            // toolStripButtonUnnamed
            //
            toolStripButtonUnnamed.CheckOnClick = true;
            toolStripButtonUnnamed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonUnnamed.Name = "toolStripButtonUnnamed";
            toolStripButtonUnnamed.Text = "Unnamed";
            toolStripButtonUnnamed.ToolTipText = "Also show entries whose name (id) is unknown";
            toolStripButtonUnnamed.CheckedChanged += OnUnnamedChanged;
            //
            // toolStripButtonScanIds
            //
            toolStripButtonScanIds.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonScanIds.Name = "toolStripButtonScanIds";
            toolStripButtonScanIds.Text = "Scan ids...";
            toolStripButtonScanIds.ToolTipText = "Search an additional id range for images";
            toolStripButtonScanIds.Click += OnClickScanIds;
            //
            // toolStripEdit
            //
            toolStripEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonReplace, toolStripButtonInsert, toolStripButtonRemove, toolStripSeparator3, toolStripLabelImportAs, toolStripComboBoxFormat, toolStripButtonMipmaps, toolStripSeparator4, toolStripDropDownExport, toolStripSeparator5, toolStripButtonSave, toolStripButtonSaveAs });
            toolStripEdit.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripEdit.Location = new System.Drawing.Point(0, 25);
            toolStripEdit.Name = "toolStripEdit";
            toolStripEdit.TabIndex = 1;
            //
            // toolStripButtonReplace
            //
            toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReplace.Name = "toolStripButtonReplace";
            toolStripButtonReplace.Text = "Replace...";
            toolStripButtonReplace.ToolTipText = "Replace the selected image with a picture (png, bmp, gif, jpg, tif, dds)";
            toolStripButtonReplace.Click += OnClickReplace;
            //
            // toolStripButtonInsert
            //
            toolStripButtonInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonInsert.Name = "toolStripButtonInsert";
            toolStripButtonInsert.Text = "Insert...";
            toolStripButtonInsert.ToolTipText = "Add an image with a new id";
            toolStripButtonInsert.Click += OnClickInsert;
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
            // toolStripLabelImportAs
            //
            toolStripLabelImportAs.Name = "toolStripLabelImportAs";
            toolStripLabelImportAs.Text = "Import as:";
            //
            // toolStripComboBoxFormat
            //
            toolStripComboBoxFormat.Items.AddRange(new object[] { "Same as original", "DXT5", "DXT1", "DXT3", "ARGB 32 bit" });
            toolStripComboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFormat.Name = "toolStripComboBoxFormat";
            toolStripComboBoxFormat.Size = new System.Drawing.Size(120, 25);
            toolStripComboBoxFormat.ToolTipText = "DDS format used when importing images";
            //
            // toolStripButtonMipmaps
            //
            toolStripButtonMipmaps.CheckOnClick = true;
            toolStripButtonMipmaps.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonMipmaps.Checked = true;
            toolStripButtonMipmaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonMipmaps.Name = "toolStripButtonMipmaps";
            toolStripButtonMipmaps.Text = "Mipmaps";
            toolStripButtonMipmaps.ToolTipText = "Generate mipmaps when the original image has them";
            //
            // toolStripSeparator4
            //
            toolStripSeparator4.Name = "toolStripSeparator4";
            //
            // toolStripDropDownExport
            //
            toolStripDropDownExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportPngToolStripMenuItem, exportBmpToolStripMenuItem, exportDdsToolStripMenuItem, toolStripSeparatorExport, exportAllToolStripMenuItem });
            toolStripDropDownExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownExport.Name = "toolStripDropDownExport";
            toolStripDropDownExport.Text = "Export";
            //
            // exportPngToolStripMenuItem
            //
            exportPngToolStripMenuItem.Name = "exportPngToolStripMenuItem";
            exportPngToolStripMenuItem.Text = "Selected as PNG...";
            exportPngToolStripMenuItem.Click += OnClickExportPng;
            //
            // exportBmpToolStripMenuItem
            //
            exportBmpToolStripMenuItem.Name = "exportBmpToolStripMenuItem";
            exportBmpToolStripMenuItem.Text = "Selected as BMP...";
            exportBmpToolStripMenuItem.Click += OnClickExportBmp;
            //
            // exportDdsToolStripMenuItem
            //
            exportDdsToolStripMenuItem.Name = "exportDdsToolStripMenuItem";
            exportDdsToolStripMenuItem.Text = "Selected as original data (DDS / TGA)...";
            exportDdsToolStripMenuItem.Click += OnClickExportDds;
            //
            // toolStripSeparatorExport
            //
            toolStripSeparatorExport.Name = "toolStripSeparatorExport";
            //
            // exportAllToolStripMenuItem
            //
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Text = "All as PNG to folder...";
            exportAllToolStripMenuItem.Click += OnClickExportAll;
            //
            // toolStripSeparator5
            //
            toolStripSeparator5.Name = "toolStripSeparator5";
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
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(thumbnailGrid);
            splitContainer.Panel2.Controls.Add(panelRight);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 540;
            splitContainer.TabIndex = 1;
            //
            // thumbnailGrid
            //
            thumbnailGrid.ContextMenuStrip = contextMenuStripGrid;
            thumbnailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            thumbnailGrid.Name = "thumbnailGrid";
            thumbnailGrid.TabIndex = 0;
            //
            // contextMenuStripGrid
            //
            contextMenuStripGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { replaceToolStripMenuItem, insertToolStripMenuItem, exportPngMenuToolStripMenuItem, exportDdsMenuToolStripMenuItem, copyIdToolStripMenuItem, toolStripSeparatorMenu1, removeToolStripMenuItem, toolStripSeparatorMenu2, saveToolStripMenuItem });
            contextMenuStripGrid.Name = "contextMenuStripGrid";
            //
            // replaceToolStripMenuItem
            //
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Text = "Replace...";
            replaceToolStripMenuItem.Click += OnClickReplace;
            //
            // insertToolStripMenuItem
            //
            insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            insertToolStripMenuItem.Text = "Insert...";
            insertToolStripMenuItem.Click += OnClickInsert;
            //
            // exportPngMenuToolStripMenuItem
            //
            exportPngMenuToolStripMenuItem.Name = "exportPngMenuToolStripMenuItem";
            exportPngMenuToolStripMenuItem.Text = "Export as PNG...";
            exportPngMenuToolStripMenuItem.Click += OnClickExportPng;
            //
            // exportDdsMenuToolStripMenuItem
            //
            exportDdsMenuToolStripMenuItem.Name = "exportDdsMenuToolStripMenuItem";
            exportDdsMenuToolStripMenuItem.Text = "Export DDS...";
            exportDdsMenuToolStripMenuItem.Click += OnClickExportDds;
            //
            // copyIdToolStripMenuItem
            //
            copyIdToolStripMenuItem.Name = "copyIdToolStripMenuItem";
            copyIdToolStripMenuItem.Text = "Copy id";
            copyIdToolStripMenuItem.Click += OnClickCopyId;
            //
            // toolStripSeparatorMenu1
            //
            toolStripSeparatorMenu1.Name = "toolStripSeparatorMenu1";
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
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save file";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelRight
            //
            panelRight.Controls.Add(previewPanel);
            panelRight.Controls.Add(toolStripZoom);
            panelRight.Controls.Add(labelInfo);
            panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRight.Name = "panelRight";
            panelRight.TabIndex = 0;
            //
            // previewPanel
            //
            previewPanel.AutoScroll = true;
            previewPanel.BackColor = System.Drawing.Color.White;
            previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanel.Name = "previewPanel";
            previewPanel.TabIndex = 0;
            //
            // toolStripZoom
            //
            toolStripZoom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonZoomOut, toolStripButtonZoom1, toolStripButtonZoomIn, toolStripLabelZoomHint });
            toolStripZoom.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStripZoom.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripZoom.Name = "toolStripZoom";
            toolStripZoom.TabIndex = 1;
            //
            // toolStripButtonZoomOut
            //
            toolStripButtonZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonZoomOut.Name = "toolStripButtonZoomOut";
            toolStripButtonZoomOut.Text = "−";
            toolStripButtonZoomOut.Click += OnClickZoomOut;
            //
            // toolStripButtonZoom1
            //
            toolStripButtonZoom1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonZoom1.Name = "toolStripButtonZoom1";
            toolStripButtonZoom1.Text = "1:1";
            toolStripButtonZoom1.Click += OnClickZoom1;
            //
            // toolStripButtonZoomIn
            //
            toolStripButtonZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonZoomIn.Name = "toolStripButtonZoomIn";
            toolStripButtonZoomIn.Text = "+";
            toolStripButtonZoomIn.Click += OnClickZoomIn;
            //
            // toolStripLabelZoomHint
            //
            toolStripLabelZoomHint.Name = "toolStripLabelZoomHint";
            toolStripLabelZoomHint.Text = "Ctrl + mouse wheel = zoom";
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(350, 92);
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
            // EcArtControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStripEdit);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcArtControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            toolStripEdit.ResumeLayout(false);
            toolStripEdit.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripGrid.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            toolStripZoom.ResumeLayout(false);
            toolStripZoom.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSource;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSource;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelId;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSize;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSize;
        private System.Windows.Forms.ToolStripButton toolStripButtonUnnamed;
        private System.Windows.Forms.ToolStripButton toolStripButtonScanIds;
        private System.Windows.Forms.ToolStrip toolStripEdit;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonInsert;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabelImportAs;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFormat;
        private System.Windows.Forms.ToolStripButton toolStripButtonMipmaps;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownExport;
        private System.Windows.Forms.ToolStripMenuItem exportPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorExport;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainer;
        private EcThumbnailGrid thumbnailGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGrid;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportPngMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportDdsMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelRight;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.ToolStrip toolStripZoom;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomOut;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoom1;
        private System.Windows.Forms.ToolStripButton toolStripButtonZoomIn;
        private System.Windows.Forms.ToolStripLabel toolStripLabelZoomHint;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

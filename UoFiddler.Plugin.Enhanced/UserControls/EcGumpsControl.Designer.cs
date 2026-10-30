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
    partial class EcGumpsControl
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
            toolStripComboBoxSource = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelId = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            toolStripLabelSize = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxSize = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            toolStripButtonInsert = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            thumbnailGrid = new EcThumbnailGrid();
            contextMenuStripGrid = new System.Windows.Forms.ContextMenuStrip(components);
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            exportSelectedMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportAllMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelRight = new System.Windows.Forms.Panel();
            previewPanel = new EcPreviewPanel();
            labelInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripGrid.SuspendLayout();
            panelRight.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripComboBoxSource, toolStripLabelId, toolStripTextBoxSearch, toolStripButtonSearch, toolStripLabelSize, toolStripComboBoxSize, toolStripSeparator1, toolStripButtonReplace, toolStripButtonInsert, toolStripButtonRemove, toolStripDropDownExport, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripComboBoxSource
            //
            toolStripComboBoxSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxSource.Name = "toolStripComboBoxSource";
            toolStripComboBoxSource.Size = new System.Drawing.Size(240, 25);
            toolStripComboBoxSource.SelectedIndexChanged += OnSourceChanged;
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
            toolStripTextBoxSearch.ToolTipText = "Gump id – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonSearch
            //
            toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSearch.Name = "toolStripButtonSearch";
            toolStripButtonSearch.Text = "Search";
            toolStripButtonSearch.Click += OnClickSearch;
            //
            // toolStripLabelSize
            //
            toolStripLabelSize.Name = "toolStripLabelSize";
            toolStripLabelSize.Text = "Size:";
            //
            // toolStripComboBoxSize
            //
            toolStripComboBoxSize.Items.AddRange(new object[] { "64", "96", "128", "192" });
            toolStripComboBoxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxSize.Name = "toolStripComboBoxSize";
            toolStripComboBoxSize.Size = new System.Drawing.Size(60, 25);
            toolStripComboBoxSize.SelectedIndexChanged += OnSizeChanged;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonReplace
            //
            toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReplace.Name = "toolStripButtonReplace";
            toolStripButtonReplace.Text = "Replace...";
            toolStripButtonReplace.Click += OnClickReplace;
            //
            // toolStripButtonInsert
            //
            toolStripButtonInsert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonInsert.Name = "toolStripButtonInsert";
            toolStripButtonInsert.Text = "Insert...";
            toolStripButtonInsert.Click += OnClickInsert;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripDropDownExport
            //
            toolStripDropDownExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportSelectedToolStripMenuItem, exportAllToolStripMenuItem });
            toolStripDropDownExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownExport.Name = "toolStripDropDownExport";
            toolStripDropDownExport.Text = "Export";
            //
            // exportSelectedToolStripMenuItem
            //
            exportSelectedToolStripMenuItem.Name = "exportSelectedToolStripMenuItem";
            exportSelectedToolStripMenuItem.Text = "Selected as PNG...";
            exportSelectedToolStripMenuItem.Click += OnClickExportSelected;
            //
            // exportAllToolStripMenuItem
            //
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Text = "All as PNG to folder...";
            exportAllToolStripMenuItem.Click += OnClickExportAll;
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
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(thumbnailGrid);
            splitContainer.Panel2.Controls.Add(panelRight);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 480;
            splitContainer.TabIndex = 1;
            //
            // thumbnailGrid
            //
            thumbnailGrid.CellSize = 128;
            thumbnailGrid.ContextMenuStrip = contextMenuStripGrid;
            thumbnailGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            thumbnailGrid.Name = "thumbnailGrid";
            thumbnailGrid.TabIndex = 0;
            //
            // contextMenuStripGrid
            //
            contextMenuStripGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { replaceToolStripMenuItem, insertToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu1, exportSelectedMenuToolStripMenuItem, exportAllMenuToolStripMenuItem, toolStripSeparatorMenu2, saveToolStripMenuItem });
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
            // exportSelectedMenuToolStripMenuItem
            //
            exportSelectedMenuToolStripMenuItem.Name = "exportSelectedMenuToolStripMenuItem";
            exportSelectedMenuToolStripMenuItem.Text = "Export as PNG...";
            exportSelectedMenuToolStripMenuItem.Click += OnClickExportSelected;
            //
            // exportAllMenuToolStripMenuItem
            //
            exportAllMenuToolStripMenuItem.Name = "exportAllMenuToolStripMenuItem";
            exportAllMenuToolStripMenuItem.Text = "Export all as PNG...";
            exportAllMenuToolStripMenuItem.Click += OnClickExportAll;
            //
            // toolStripSeparatorMenu2
            //
            toolStripSeparatorMenu2.Name = "toolStripSeparatorMenu2";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelRight
            //
            panelRight.Controls.Add(previewPanel);
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
            previewPanel.TabIndex = 1;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(400, 50);
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
            // EcGumpsControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcGumpsControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripGrid.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSource;
        private System.Windows.Forms.ToolStripLabel toolStripLabelId;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSize;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxSize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonInsert;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownExport;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainer;
        private EcThumbnailGrid thumbnailGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripGrid;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem exportSelectedMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelRight;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

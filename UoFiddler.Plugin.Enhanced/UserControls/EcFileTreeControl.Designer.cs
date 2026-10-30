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
    partial class EcFileTreeControl
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
            toolStripLabelFile = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxFile = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelFilter = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxFilter = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripButtonReplace = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            treeViewFiles = new System.Windows.Forms.TreeView();
            contextMenuStripTree = new System.Windows.Forms.ContextMenuStrip(components);
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu1 = new System.Windows.Forms.ToolStripSeparator();
            exportFileMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportFolderMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu2 = new System.Windows.Forms.ToolStripSeparator();
            copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelRight = new System.Windows.Forms.Panel();
            textBoxText = new System.Windows.Forms.TextBox();
            previewPanel = new EcPreviewPanel();
            buttonApply = new System.Windows.Forms.Button();
            labelInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripTree.SuspendLayout();
            panelRight.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelFile, toolStripComboBoxFile, toolStripLabelFilter, toolStripTextBoxFilter, toolStripButtonApply, toolStripSeparator1, toolStripButtonOpen, toolStripDropDownExport, toolStripButtonReplace, toolStripButtonAdd, toolStripButtonRemove, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelFile
            //
            toolStripLabelFile.Name = "toolStripLabelFile";
            toolStripLabelFile.Text = "File:";
            //
            // toolStripComboBoxFile
            //
            toolStripComboBoxFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxFile.Name = "toolStripComboBoxFile";
            toolStripComboBoxFile.Size = new System.Drawing.Size(180, 25);
            toolStripComboBoxFile.SelectedIndexChanged += OnFileChanged;
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
            toolStripTextBoxFilter.ToolTipText = "Part of the path – Enter";
            toolStripTextBoxFilter.KeyDown += OnFilterKeyDown;
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply";
            toolStripButtonApply.Click += OnClickApply;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonOpen
            //
            toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOpen.Name = "toolStripButtonOpen";
            toolStripButtonOpen.Text = "Open";
            toolStripButtonOpen.ToolTipText = "Open with the Windows program";
            toolStripButtonOpen.Click += OnClickOpen;
            //
            // toolStripDropDownExport
            //
            toolStripDropDownExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportFileToolStripMenuItem, exportFolderToolStripMenuItem });
            toolStripDropDownExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownExport.Name = "toolStripDropDownExport";
            toolStripDropDownExport.Text = "Export";
            //
            // exportFileToolStripMenuItem
            //
            exportFileToolStripMenuItem.Name = "exportFileToolStripMenuItem";
            exportFileToolStripMenuItem.Text = "Selected file...";
            exportFileToolStripMenuItem.Click += OnClickExportFile;
            //
            // exportFolderToolStripMenuItem
            //
            exportFolderToolStripMenuItem.Name = "exportFolderToolStripMenuItem";
            exportFolderToolStripMenuItem.Text = "Selected folder (with sub folders)...";
            exportFolderToolStripMenuItem.Click += OnClickExportFolder;
            //
            // toolStripButtonReplace
            //
            toolStripButtonReplace.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonReplace.Name = "toolStripButtonReplace";
            toolStripButtonReplace.Text = "Replace...";
            toolStripButtonReplace.Click += OnClickReplace;
            //
            // toolStripButtonAdd
            //
            toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAdd.Name = "toolStripButtonAdd";
            toolStripButtonAdd.Text = "Add...";
            toolStripButtonAdd.Click += OnClickAdd;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove";
            toolStripButtonRemove.Click += OnClickRemove;
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
            splitContainer.Panel1.Controls.Add(treeViewFiles);
            splitContainer.Panel2.Controls.Add(panelRight);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 360;
            splitContainer.TabIndex = 1;
            //
            // treeViewFiles
            //
            treeViewFiles.ContextMenuStrip = contextMenuStripTree;
            treeViewFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewFiles.HideSelection = false;
            treeViewFiles.Name = "treeViewFiles";
            treeViewFiles.TabIndex = 0;
            treeViewFiles.AfterSelect += OnAfterSelect;
            treeViewFiles.NodeMouseClick += OnNodeMouseClick;
            //
            // contextMenuStripTree
            //
            contextMenuStripTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, toolStripSeparatorMenu1, exportFileMenuToolStripMenuItem, exportFolderMenuToolStripMenuItem, replaceToolStripMenuItem, addToolStripMenuItem, removeToolStripMenuItem, toolStripSeparatorMenu2, copyPathToolStripMenuItem, saveToolStripMenuItem });
            contextMenuStripTree.Name = "contextMenuStripTree";
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
            // exportFileMenuToolStripMenuItem
            //
            exportFileMenuToolStripMenuItem.Name = "exportFileMenuToolStripMenuItem";
            exportFileMenuToolStripMenuItem.Text = "Export file...";
            exportFileMenuToolStripMenuItem.Click += OnClickExportFile;
            //
            // exportFolderMenuToolStripMenuItem
            //
            exportFolderMenuToolStripMenuItem.Name = "exportFolderMenuToolStripMenuItem";
            exportFolderMenuToolStripMenuItem.Text = "Export folder...";
            exportFolderMenuToolStripMenuItem.Click += OnClickExportFolder;
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
            // copyPathToolStripMenuItem
            //
            copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            copyPathToolStripMenuItem.Text = "Copy path";
            copyPathToolStripMenuItem.Click += OnClickCopyPath;
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelRight
            //
            panelRight.Controls.Add(textBoxText);
            panelRight.Controls.Add(previewPanel);
            panelRight.Controls.Add(buttonApply);
            panelRight.Controls.Add(labelInfo);
            panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRight.Name = "panelRight";
            panelRight.TabIndex = 0;
            //
            // textBoxText
            //
            textBoxText.AcceptsTab = true;
            textBoxText.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxText.Font = new System.Drawing.Font("Consolas", 9F);
            textBoxText.MaxLength = 0;
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBoxText.TabIndex = 0;
            textBoxText.Visible = false;
            textBoxText.WordWrap = false;
            textBoxText.TextChanged += OnTextChanged;
            //
            // previewPanel
            //
            previewPanel.AutoScroll = true;
            previewPanel.BackColor = System.Drawing.Color.White;
            previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanel.Name = "previewPanel";
            previewPanel.TabIndex = 1;
            previewPanel.Visible = false;
            //
            // buttonApply
            //
            buttonApply.Dock = System.Windows.Forms.DockStyle.Bottom;
            buttonApply.Enabled = false;
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(400, 28);
            buttonApply.TabIndex = 2;
            buttonApply.Text = "Apply text changes";
            buttonApply.UseVisualStyleBackColor = true;
            buttonApply.Click += OnClickApplyText;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(400, 36);
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
            // EcFileTreeControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcFileTreeControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripTree.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFile;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxFile;
        private System.Windows.Forms.ToolStripLabel toolStripLabelFilter;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFilter;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownExport;
        private System.Windows.Forms.ToolStripMenuItem exportFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonReplace;
        private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeViewFiles;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTree;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu1;
        private System.Windows.Forms.ToolStripMenuItem exportFileMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportFolderMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu2;
        private System.Windows.Forms.ToolStripMenuItem copyPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.TextBox textBoxText;
        private EcPreviewPanel previewPanel;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

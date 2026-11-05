namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    partial class ScriptEditorForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtEditor = new System.Windows.Forms.TextBox();
            treeFiles = new System.Windows.Forms.TreeView();
            panelInfo = new System.Windows.Forms.Panel();
            lblLineInfo = new System.Windows.Forms.Label();
            picPattern = new System.Windows.Forms.PictureBox();
            flowLineTiles = new System.Windows.Forms.FlowLayoutPanel();
            txtHelp = new System.Windows.Forms.TextBox();
            toolStrip = new System.Windows.Forms.ToolStrip();
            tsbSave = new System.Windows.Forms.ToolStripButton();
            tsbReloadFile = new System.Windows.Forms.ToolStripButton();
            tsbOpenFolder = new System.Windows.Forms.ToolStripButton();
            tssSep1 = new System.Windows.Forms.ToolStripSeparator();
            tslSearch = new System.Windows.Forms.ToolStripLabel();
            tstSearch = new System.Windows.Forms.ToolStripTextBox();
            tsbFindNext = new System.Windows.Forms.ToolStripButton();
            tssSep2 = new System.Windows.Forms.ToolStripSeparator();
            tslFile = new System.Windows.Forms.ToolStripLabel();
            ((System.ComponentModel.ISupportInitialize)picPattern).BeginInit();
            panelInfo.SuspendLayout();
            flowLineTiles.SuspendLayout();
            toolStrip.SuspendLayout();
            SuspendLayout();
            //
            // txtEditor
            //
            txtEditor.AcceptsTab = true;
            txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            txtEditor.Font = new System.Drawing.Font("Consolas", 10F);
            txtEditor.HideSelection = false;
            txtEditor.Multiline = true;
            txtEditor.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtEditor.WordWrap = false;
            txtEditor.Name = "txtEditor";
            txtEditor.TabIndex = 0;
            txtEditor.TextChanged += TxtEditor_TextChanged;
            txtEditor.KeyUp += TxtEditor_CaretMoved;
            txtEditor.MouseUp += TxtEditor_CaretMoved;
            //
            // treeFiles
            //
            treeFiles.Dock = System.Windows.Forms.DockStyle.Left;
            treeFiles.HideSelection = false;
            treeFiles.Size = new System.Drawing.Size(230, 500);
            treeFiles.Name = "treeFiles";
            treeFiles.TabIndex = 1;
            treeFiles.AfterSelect += TreeFiles_AfterSelect;
            //
            // panelInfo
            //
            panelInfo.Controls.Add(lblLineInfo);
            panelInfo.Controls.Add(picPattern);
            panelInfo.Controls.Add(flowLineTiles);
            panelInfo.AutoScroll = true;
            panelInfo.BackColor = System.Drawing.Color.FromArgb(36, 36, 40);
            panelInfo.Dock = System.Windows.Forms.DockStyle.Right;
            panelInfo.Size = new System.Drawing.Size(330, 500);
            panelInfo.Name = "panelInfo";
            panelInfo.TabIndex = 2;
            //
            // lblLineInfo
            //
            lblLineInfo.ForeColor = System.Drawing.Color.Gainsboro;
            lblLineInfo.Location = new System.Drawing.Point(8, 8);
            lblLineInfo.Size = new System.Drawing.Size(310, 150);
            lblLineInfo.Text = "-";
            lblLineInfo.Name = "lblLineInfo";
            lblLineInfo.TabIndex = 3;
            //
            // picPattern
            //
            picPattern.Location = new System.Drawing.Point(8, 160);
            picPattern.Size = new System.Drawing.Size(96, 96);
            picPattern.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPattern.Name = "picPattern";
            picPattern.TabIndex = 4;
            //
            // flowLineTiles
            //
            flowLineTiles.AutoScroll = true;
            flowLineTiles.Location = new System.Drawing.Point(8, 262);
            flowLineTiles.Size = new System.Drawing.Size(310, 300);
            flowLineTiles.Name = "flowLineTiles";
            flowLineTiles.TabIndex = 5;
            //
            // txtHelp
            //
            txtHelp.BackColor = System.Drawing.Color.FromArgb(250, 248, 235);
            txtHelp.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtHelp.Size = new System.Drawing.Size(1180, 150);
            txtHelp.Name = "txtHelp";
            txtHelp.TabIndex = 6;
            //
            // toolStrip
            //
            toolStrip.Items.Add(tsbSave);
            toolStrip.Items.Add(tsbReloadFile);
            toolStrip.Items.Add(tsbOpenFolder);
            toolStrip.Items.Add(tssSep1);
            toolStrip.Items.Add(tslSearch);
            toolStrip.Items.Add(tstSearch);
            toolStrip.Items.Add(tsbFindNext);
            toolStrip.Items.Add(tssSep2);
            toolStrip.Items.Add(tslFile);
            toolStrip.Dock = System.Windows.Forms.DockStyle.Top;
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 7;
            //
            // tsbSave
            //
            tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbSave.Text = "Speichern (Strg+S)";
            tsbSave.Name = "tsbSave";
            tsbSave.Click += TsbSave_Click;
            //
            // tsbReloadFile
            //
            tsbReloadFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbReloadFile.Text = "Datei neu laden";
            tsbReloadFile.Name = "tsbReloadFile";
            tsbReloadFile.Click += TsbReloadFile_Click;
            //
            // tsbOpenFolder
            //
            tsbOpenFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbOpenFolder.Text = "Ordner öffnen";
            tsbOpenFolder.Name = "tsbOpenFolder";
            tsbOpenFolder.Click += TsbOpenFolder_Click;
            //
            // tssSep1
            //
            tssSep1.Name = "tssSep1";
            //
            // tslSearch
            //
            tslSearch.Text = "Suchen:";
            tslSearch.Name = "tslSearch";
            //
            // tstSearch
            //
            tstSearch.Size = new System.Drawing.Size(160, 25);
            tstSearch.Name = "tstSearch";
            tstSearch.KeyDown += TstSearch_KeyDown;
            //
            // tsbFindNext
            //
            tsbFindNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tsbFindNext.Text = "Weiter";
            tsbFindNext.Name = "tsbFindNext";
            tsbFindNext.Click += TsbFindNext_Click;
            //
            // tssSep2
            //
            tssSep2.Name = "tssSep2";
            //
            // tslFile
            //
            tslFile.Text = "-";
            tslFile.Name = "tslFile";
            //
            // ScriptEditorForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1180, 760);
            Controls.Add(txtEditor);
            Controls.Add(treeFiles);
            Controls.Add(panelInfo);
            Controls.Add(txtHelp);
            Controls.Add(toolStrip);
            MinimumSize = new System.Drawing.Size(800, 500);
            KeyPreview = true;
            Name = "ScriptEditorForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Skript-Editor (Dragon-Skripte)";
            ((System.ComponentModel.ISupportInitialize)picPattern).EndInit();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            flowLineTiles.ResumeLayout(false);
            flowLineTiles.PerformLayout();
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtEditor;
        private System.Windows.Forms.TreeView treeFiles;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label lblLineInfo;
        private System.Windows.Forms.PictureBox picPattern;
        private System.Windows.Forms.FlowLayoutPanel flowLineTiles;
        private System.Windows.Forms.TextBox txtHelp;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripButton tsbReloadFile;
        private System.Windows.Forms.ToolStripButton tsbOpenFolder;
        private System.Windows.Forms.ToolStripSeparator tssSep1;
        private System.Windows.Forms.ToolStripLabel tslSearch;
        private System.Windows.Forms.ToolStripTextBox tstSearch;
        private System.Windows.Forms.ToolStripButton tsbFindNext;
        private System.Windows.Forms.ToolStripSeparator tssSep2;
        private System.Windows.Forms.ToolStripLabel tslFile;
    }
}

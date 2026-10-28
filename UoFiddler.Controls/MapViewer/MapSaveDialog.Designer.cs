namespace UoFiddler.Controls.MapViewer
{
    partial class MapSaveDialog
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
            labelInfo = new System.Windows.Forms.Label();
            groupBoxTarget = new System.Windows.Forms.GroupBox();
            tableLayoutTarget = new System.Windows.Forms.TableLayoutPanel();
            radioOutput = new System.Windows.Forms.RadioButton();
            labelOutputPath = new System.Windows.Forms.Label();
            radioOriginal = new System.Windows.Forms.RadioButton();
            labelOriginalPath = new System.Windows.Forms.Label();
            radioOther = new System.Windows.Forms.RadioButton();
            panelOther = new System.Windows.Forms.TableLayoutPanel();
            textBoxOther = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            labelWarning = new System.Windows.Forms.Label();
            groupBoxFiles = new System.Windows.Forms.GroupBox();
            tableLayoutFiles = new System.Windows.Forms.TableLayoutPanel();
            checkBoxLand = new System.Windows.Forms.CheckBox();
            checkBoxStatics = new System.Windows.Forms.CheckBox();
            checkBoxCompleteSet = new System.Windows.Forms.CheckBox();
            labelIndex = new System.Windows.Forms.Label();
            numericIndex = new System.Windows.Forms.NumericUpDown();
            labelFileNames = new System.Windows.Forms.Label();
            checkBoxBackup = new System.Windows.Forms.CheckBox();
            labelBackup = new System.Windows.Forms.Label();
            checkBoxProtocol = new System.Windows.Forms.CheckBox();
            panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonCancel = new System.Windows.Forms.Button();
            buttonSave = new System.Windows.Forms.Button();
            buttonRestore = new System.Windows.Forms.Button();
            groupBoxTarget.SuspendLayout();
            tableLayoutTarget.SuspendLayout();
            panelOther.SuspendLayout();
            panelButtons.SuspendLayout();
            groupBoxFiles.SuspendLayout();
            tableLayoutFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericIndex).BeginInit();
            SuspendLayout();
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Location = new System.Drawing.Point(10, 10);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(584, 58);
            labelInfo.TabIndex = 0;
            labelInfo.Text = "-";
            //
            // groupBoxTarget
            //
            groupBoxTarget.AutoSize = true;
            groupBoxTarget.Controls.Add(tableLayoutTarget);
            groupBoxTarget.Dock = System.Windows.Forms.DockStyle.Top;
            groupBoxTarget.Location = new System.Drawing.Point(10, 68);
            groupBoxTarget.Name = "groupBoxTarget";
            groupBoxTarget.Padding = new System.Windows.Forms.Padding(8);
            groupBoxTarget.Size = new System.Drawing.Size(584, 170);
            groupBoxTarget.TabIndex = 1;
            groupBoxTarget.TabStop = false;
            groupBoxTarget.Text = "Save into";
            //
            // tableLayoutTarget
            //
            tableLayoutTarget.AutoSize = true;
            tableLayoutTarget.ColumnCount = 1;
            tableLayoutTarget.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutTarget.Controls.Add(radioOutput, 0, 0);
            tableLayoutTarget.Controls.Add(labelOutputPath, 0, 1);
            tableLayoutTarget.Controls.Add(radioOriginal, 0, 2);
            tableLayoutTarget.Controls.Add(labelOriginalPath, 0, 3);
            tableLayoutTarget.Controls.Add(radioOther, 0, 4);
            tableLayoutTarget.Controls.Add(panelOther, 0, 5);
            tableLayoutTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutTarget.Name = "tableLayoutTarget";
            tableLayoutTarget.RowCount = 6;
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutTarget.TabIndex = 0;
            //
            // radioOutput
            //
            radioOutput.AutoSize = true;
            radioOutput.Checked = true;
            radioOutput.Name = "radioOutput";
            radioOutput.TabIndex = 0;
            radioOutput.TabStop = true;
            radioOutput.Text = "Output directory (recommended, the client files stay unchanged)";
            radioOutput.UseVisualStyleBackColor = true;
            radioOutput.CheckedChanged += OnTargetChanged;
            //
            // labelOutputPath
            //
            labelOutputPath.AutoSize = true;
            labelOutputPath.ForeColor = System.Drawing.SystemColors.GrayText;
            labelOutputPath.Margin = new System.Windows.Forms.Padding(20, 0, 3, 6);
            labelOutputPath.Name = "labelOutputPath";
            labelOutputPath.Text = "-";
            //
            // radioOriginal
            //
            radioOriginal.AutoSize = true;
            radioOriginal.Name = "radioOriginal";
            radioOriginal.TabIndex = 1;
            radioOriginal.Text = "Directory of the loaded map files (replaces the original files)";
            radioOriginal.UseVisualStyleBackColor = true;
            radioOriginal.CheckedChanged += OnTargetChanged;
            //
            // labelOriginalPath
            //
            labelOriginalPath.AutoSize = true;
            labelOriginalPath.ForeColor = System.Drawing.SystemColors.GrayText;
            labelOriginalPath.Margin = new System.Windows.Forms.Padding(20, 0, 3, 6);
            labelOriginalPath.Name = "labelOriginalPath";
            labelOriginalPath.Text = "-";
            //
            // radioOther
            //
            radioOther.AutoSize = true;
            radioOther.Name = "radioOther";
            radioOther.TabIndex = 2;
            radioOther.Text = "Other directory:";
            radioOther.UseVisualStyleBackColor = true;
            radioOther.CheckedChanged += OnTargetChanged;
            //
            // panelOther
            //
            panelOther.AutoSize = true;
            panelOther.ColumnCount = 2;
            panelOther.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelOther.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelOther.Controls.Add(textBoxOther, 0, 0);
            panelOther.Controls.Add(buttonBrowse, 1, 0);
            panelOther.Dock = System.Windows.Forms.DockStyle.Fill;
            panelOther.Margin = new System.Windows.Forms.Padding(17, 0, 3, 0);
            panelOther.Name = "panelOther";
            panelOther.RowCount = 1;
            panelOther.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelOther.TabIndex = 3;
            //
            // textBoxOther
            //
            textBoxOther.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxOther.Enabled = false;
            textBoxOther.Name = "textBoxOther";
            textBoxOther.TabIndex = 0;
            textBoxOther.TextChanged += OnTargetChanged;
            //
            // buttonBrowse
            //
            buttonBrowse.AutoSize = true;
            buttonBrowse.Enabled = false;
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += OnClickBrowse;
            //
            // groupBoxFiles
            //
            groupBoxFiles.AutoSize = true;
            groupBoxFiles.Controls.Add(tableLayoutFiles);
            groupBoxFiles.Dock = System.Windows.Forms.DockStyle.Top;
            groupBoxFiles.Name = "groupBoxFiles";
            groupBoxFiles.Padding = new System.Windows.Forms.Padding(8);
            groupBoxFiles.TabStop = false;
            groupBoxFiles.Text = "Files";
            //
            // tableLayoutFiles
            //
            tableLayoutFiles.AutoSize = true;
            tableLayoutFiles.ColumnCount = 2;
            tableLayoutFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFiles.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutFiles.Controls.Add(checkBoxLand, 0, 0);
            tableLayoutFiles.SetColumnSpan(checkBoxLand, 2);
            tableLayoutFiles.Controls.Add(checkBoxStatics, 0, 1);
            tableLayoutFiles.SetColumnSpan(checkBoxStatics, 2);
            tableLayoutFiles.Controls.Add(checkBoxCompleteSet, 0, 2);
            tableLayoutFiles.SetColumnSpan(checkBoxCompleteSet, 2);
            tableLayoutFiles.Controls.Add(labelIndex, 0, 3);
            tableLayoutFiles.Controls.Add(numericIndex, 1, 3);
            tableLayoutFiles.Controls.Add(labelFileNames, 0, 4);
            tableLayoutFiles.SetColumnSpan(labelFileNames, 2);
            tableLayoutFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutFiles.Name = "tableLayoutFiles";
            tableLayoutFiles.RowCount = 5;
            tableLayoutFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutFiles.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            //
            // checkBoxLand
            //
            checkBoxLand.AutoSize = true;
            checkBoxLand.Checked = true;
            checkBoxLand.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLand.Name = "checkBoxLand";
            checkBoxLand.Text = "Land (map.mul)";
            checkBoxLand.UseVisualStyleBackColor = true;
            checkBoxLand.CheckedChanged += OnTargetChanged;
            //
            // checkBoxStatics
            //
            checkBoxStatics.AutoSize = true;
            checkBoxStatics.Checked = true;
            checkBoxStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxStatics.Name = "checkBoxStatics";
            checkBoxStatics.Text = "Statics (staidx.mul + statics.mul)";
            checkBoxStatics.UseVisualStyleBackColor = true;
            checkBoxStatics.CheckedChanged += OnTargetChanged;
            //
            // checkBoxCompleteSet
            //
            checkBoxCompleteSet.AutoSize = true;
            checkBoxCompleteSet.Name = "checkBoxCompleteSet";
            checkBoxCompleteSet.Text = "Complete set: also write files that were not changed (for a manual exchange)";
            checkBoxCompleteSet.UseVisualStyleBackColor = true;
            checkBoxCompleteSet.CheckedChanged += OnTargetChanged;
            //
            // labelIndex
            //
            labelIndex.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelIndex.AutoSize = true;
            labelIndex.Name = "labelIndex";
            labelIndex.Text = "File number (mapN):";
            //
            // numericIndex
            //
            numericIndex.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericIndex.Name = "numericIndex";
            numericIndex.Size = new System.Drawing.Size(60, 23);
            numericIndex.ValueChanged += OnTargetChanged;
            //
            // labelFileNames
            //
            labelFileNames.AutoSize = true;
            labelFileNames.ForeColor = System.Drawing.SystemColors.GrayText;
            labelFileNames.Name = "labelFileNames";
            labelFileNames.Text = "-";
            //
            // labelWarning
            //
            labelWarning.Dock = System.Windows.Forms.DockStyle.Top;
            labelWarning.ForeColor = System.Drawing.Color.Firebrick;
            labelWarning.Location = new System.Drawing.Point(10, 238);
            labelWarning.Name = "labelWarning";
            labelWarning.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            labelWarning.Size = new System.Drawing.Size(584, 52);
            labelWarning.TabIndex = 2;
            //
            // checkBoxBackup
            //
            checkBoxBackup.AutoSize = true;
            checkBoxBackup.Checked = true;
            checkBoxBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBackup.Dock = System.Windows.Forms.DockStyle.Top;
            checkBoxBackup.Location = new System.Drawing.Point(10, 290);
            checkBoxBackup.Name = "checkBoxBackup";
            checkBoxBackup.TabIndex = 3;
            checkBoxBackup.Text = "Create a backup of the files that are replaced (can be restored later)";
            checkBoxBackup.UseVisualStyleBackColor = true;
            checkBoxBackup.CheckedChanged += OnTargetChanged;
            //
            // labelBackup
            //
            labelBackup.Dock = System.Windows.Forms.DockStyle.Top;
            labelBackup.ForeColor = System.Drawing.SystemColors.GrayText;
            labelBackup.Location = new System.Drawing.Point(10, 309);
            labelBackup.Name = "labelBackup";
            labelBackup.Padding = new System.Windows.Forms.Padding(17, 0, 0, 0);
            labelBackup.Size = new System.Drawing.Size(584, 34);
            labelBackup.TabIndex = 4;
            //
            // checkBoxProtocol
            //
            checkBoxProtocol.AutoSize = true;
            checkBoxProtocol.Checked = true;
            checkBoxProtocol.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxProtocol.Dock = System.Windows.Forms.DockStyle.Top;
            checkBoxProtocol.Location = new System.Drawing.Point(10, 343);
            checkBoxProtocol.Name = "checkBoxProtocol";
            checkBoxProtocol.TabIndex = 5;
            checkBoxProtocol.Text = "Save the error protocol of the region next to the files";
            checkBoxProtocol.UseVisualStyleBackColor = true;
            //
            // panelButtons
            //
            panelButtons.AutoSize = true;
            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Controls.Add(buttonSave);
            panelButtons.Controls.Add(buttonRestore);
            panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelButtons.Location = new System.Drawing.Point(10, 384);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new System.Drawing.Size(584, 35);
            panelButtons.TabIndex = 6;
            //
            // buttonCancel
            //
            buttonCancel.AutoSize = true;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Name = "buttonCancel";
            buttonCancel.TabIndex = 0;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonSave
            //
            buttonSave.AutoSize = true;
            buttonSave.Name = "buttonSave";
            buttonSave.TabIndex = 1;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            buttonSave.Click += OnClickSave;
            //
            // buttonRestore
            //
            buttonRestore.AutoSize = true;
            buttonRestore.Name = "buttonRestore";
            buttonRestore.TabIndex = 2;
            buttonRestore.Text = "Restore backup...";
            buttonRestore.UseVisualStyleBackColor = true;
            buttonRestore.Click += OnClickRestore;
            //
            // MapSaveDialog
            //
            AcceptButton = buttonSave;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(640, 600);
            Controls.Add(checkBoxProtocol);
            Controls.Add(labelBackup);
            Controls.Add(checkBoxBackup);
            Controls.Add(labelWarning);
            Controls.Add(groupBoxFiles);
            Controls.Add(groupBoxTarget);
            Controls.Add(labelInfo);
            Controls.Add(panelButtons);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MapSaveDialog";
            Padding = new System.Windows.Forms.Padding(10);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Save map changes";
            groupBoxTarget.ResumeLayout(false);
            groupBoxTarget.PerformLayout();
            tableLayoutTarget.ResumeLayout(false);
            tableLayoutTarget.PerformLayout();
            panelOther.ResumeLayout(false);
            panelOther.PerformLayout();
            panelButtons.ResumeLayout(false);
            panelButtons.PerformLayout();
            tableLayoutFiles.ResumeLayout(false);
            tableLayoutFiles.PerformLayout();
            groupBoxFiles.ResumeLayout(false);
            groupBoxFiles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericIndex).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.GroupBox groupBoxTarget;
        private System.Windows.Forms.TableLayoutPanel tableLayoutTarget;
        private System.Windows.Forms.RadioButton radioOutput;
        private System.Windows.Forms.Label labelOutputPath;
        private System.Windows.Forms.RadioButton radioOriginal;
        private System.Windows.Forms.Label labelOriginalPath;
        private System.Windows.Forms.RadioButton radioOther;
        private System.Windows.Forms.TableLayoutPanel panelOther;
        private System.Windows.Forms.TextBox textBoxOther;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.GroupBox groupBoxFiles;
        private System.Windows.Forms.TableLayoutPanel tableLayoutFiles;
        private System.Windows.Forms.CheckBox checkBoxLand;
        private System.Windows.Forms.CheckBox checkBoxStatics;
        private System.Windows.Forms.CheckBox checkBoxCompleteSet;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.NumericUpDown numericIndex;
        private System.Windows.Forms.Label labelFileNames;
        private System.Windows.Forms.CheckBox checkBoxBackup;
        private System.Windows.Forms.Label labelBackup;
        private System.Windows.Forms.CheckBox checkBoxProtocol;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonRestore;
    }
}

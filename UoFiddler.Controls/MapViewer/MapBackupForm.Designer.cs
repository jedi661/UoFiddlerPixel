namespace UoFiddler.Controls.MapViewer
{
    partial class MapBackupForm
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
            panelTop = new System.Windows.Forms.TableLayoutPanel();
            labelDirectory = new System.Windows.Forms.Label();
            comboBoxDirectory = new System.Windows.Forms.ComboBox();
            buttonBrowse = new System.Windows.Forms.Button();
            listViewBackups = new System.Windows.Forms.ListView();
            columnDate = new System.Windows.Forms.ColumnHeader();
            columnMap = new System.Windows.Forms.ColumnHeader();
            columnFiles = new System.Windows.Forms.ColumnHeader();
            columnComment = new System.Windows.Forms.ColumnHeader();
            checkBoxBackupCurrent = new System.Windows.Forms.CheckBox();
            panelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonClose = new System.Windows.Forms.Button();
            buttonRestore = new System.Windows.Forms.Button();
            buttonDelete = new System.Windows.Forms.Button();
            buttonOpenFolder = new System.Windows.Forms.Button();
            panelTop.SuspendLayout();
            panelButtons.SuspendLayout();
            SuspendLayout();
            //
            // panelTop
            //
            panelTop.AutoSize = true;
            panelTop.ColumnCount = 3;
            panelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            panelTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            panelTop.Controls.Add(labelDirectory, 0, 0);
            panelTop.Controls.Add(comboBoxDirectory, 1, 0);
            panelTop.Controls.Add(buttonBrowse, 2, 0);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Name = "panelTop";
            panelTop.RowCount = 1;
            panelTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            panelTop.TabIndex = 0;
            //
            // labelDirectory
            //
            labelDirectory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelDirectory.AutoSize = true;
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Text = "Map directory:";
            //
            // comboBoxDirectory
            //
            comboBoxDirectory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboBoxDirectory.Name = "comboBoxDirectory";
            comboBoxDirectory.TabIndex = 0;
            comboBoxDirectory.SelectedIndexChanged += OnDirectoryChanged;
            comboBoxDirectory.Leave += OnDirectoryChanged;
            //
            // buttonBrowse
            //
            buttonBrowse.AutoSize = true;
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += OnClickBrowse;
            //
            // listViewBackups
            //
            listViewBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnDate, columnMap, columnFiles, columnComment });
            listViewBackups.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewBackups.FullRowSelect = true;
            listViewBackups.GridLines = true;
            listViewBackups.HideSelection = false;
            listViewBackups.MultiSelect = false;
            listViewBackups.Name = "listViewBackups";
            listViewBackups.TabIndex = 1;
            listViewBackups.UseCompatibleStateImageBehavior = false;
            listViewBackups.View = System.Windows.Forms.View.Details;
            listViewBackups.SelectedIndexChanged += OnSelectionChanged;
            //
            // columnDate
            //
            columnDate.Text = "Date";
            columnDate.Width = 140;
            //
            // columnMap
            //
            columnMap.Text = "Map";
            columnMap.Width = 140;
            //
            // columnFiles
            //
            columnFiles.Text = "Files";
            columnFiles.Width = 240;
            //
            // columnComment
            //
            columnComment.Text = "Comment";
            columnComment.Width = 120;
            //
            // checkBoxBackupCurrent
            //
            checkBoxBackupCurrent.AutoSize = true;
            checkBoxBackupCurrent.Checked = true;
            checkBoxBackupCurrent.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBackupCurrent.Dock = System.Windows.Forms.DockStyle.Bottom;
            checkBoxBackupCurrent.Name = "checkBoxBackupCurrent";
            checkBoxBackupCurrent.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            checkBoxBackupCurrent.TabIndex = 2;
            checkBoxBackupCurrent.Text = "Back up the current files before restoring";
            checkBoxBackupCurrent.UseVisualStyleBackColor = true;
            //
            // panelButtons
            //
            panelButtons.AutoSize = true;
            panelButtons.Controls.Add(buttonClose);
            panelButtons.Controls.Add(buttonRestore);
            panelButtons.Controls.Add(buttonDelete);
            panelButtons.Controls.Add(buttonOpenFolder);
            panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            panelButtons.Name = "panelButtons";
            panelButtons.TabIndex = 3;
            //
            // buttonClose
            //
            buttonClose.AutoSize = true;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonClose.Name = "buttonClose";
            buttonClose.TabIndex = 0;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // buttonRestore
            //
            buttonRestore.AutoSize = true;
            buttonRestore.Enabled = false;
            buttonRestore.Name = "buttonRestore";
            buttonRestore.TabIndex = 1;
            buttonRestore.Text = "Restore";
            buttonRestore.UseVisualStyleBackColor = true;
            buttonRestore.Click += OnClickRestore;
            //
            // buttonDelete
            //
            buttonDelete.AutoSize = true;
            buttonDelete.Enabled = false;
            buttonDelete.Name = "buttonDelete";
            buttonDelete.TabIndex = 2;
            buttonDelete.Text = "Delete backup";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += OnClickDelete;
            //
            // buttonOpenFolder
            //
            buttonOpenFolder.AutoSize = true;
            buttonOpenFolder.Enabled = false;
            buttonOpenFolder.Name = "buttonOpenFolder";
            buttonOpenFolder.TabIndex = 3;
            buttonOpenFolder.Text = "Open folder";
            buttonOpenFolder.UseVisualStyleBackColor = true;
            buttonOpenFolder.Click += OnClickOpenFolder;
            //
            // MapBackupForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(700, 400);
            Controls.Add(listViewBackups);
            Controls.Add(panelTop);
            Controls.Add(checkBoxBackupCurrent);
            Controls.Add(panelButtons);
            MinimumSize = new System.Drawing.Size(560, 300);
            Name = "MapBackupForm";
            Padding = new System.Windows.Forms.Padding(8);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Map backups";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelButtons.ResumeLayout(false);
            panelButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel panelTop;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.ComboBox comboBoxDirectory;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ListView listViewBackups;
        private System.Windows.Forms.ColumnHeader columnDate;
        private System.Windows.Forms.ColumnHeader columnMap;
        private System.Windows.Forms.ColumnHeader columnFiles;
        private System.Windows.Forms.ColumnHeader columnComment;
        private System.Windows.Forms.CheckBox checkBoxBackupCurrent;
        private System.Windows.Forms.FlowLayoutPanel panelButtons;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRestore;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Button buttonOpenFolder;
    }
}

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
    partial class EcPatchControl
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
            toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonAddChanges = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAddFile = new System.Windows.Forms.ToolStripButton();
            toolStripButtonRemove = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            tabControl = new System.Windows.Forms.TabControl();
            tabPagePatch = new System.Windows.Forms.TabPage();
            tableLayoutPanelInfo = new System.Windows.Forms.TableLayoutPanel();
            labelTitle = new System.Windows.Forms.Label();
            textBoxTitle = new System.Windows.Forms.TextBox();
            labelAuthor = new System.Windows.Forms.Label();
            textBoxAuthor = new System.Windows.Forms.TextBox();
            labelDescription = new System.Windows.Forms.Label();
            textBoxDescription = new System.Windows.Forms.TextBox();
            checkBoxBackup = new System.Windows.Forms.CheckBox();
            listViewEntries = new System.Windows.Forms.ListView();
            columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderAction = new System.Windows.Forms.ColumnHeader();
            columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            tabPageHelp = new System.Windows.Forms.TabPage();
            flowLayoutPanelHelp = new System.Windows.Forms.FlowLayoutPanel();
            radioButtonHelpEnglish = new System.Windows.Forms.RadioButton();
            radioButtonHelpGerman = new System.Windows.Forms.RadioButton();
            richTextBoxHelp = new System.Windows.Forms.RichTextBox();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            toolStrip.SuspendLayout();
            tabControl.SuspendLayout();
            tabPagePatch.SuspendLayout();
            tableLayoutPanelInfo.SuspendLayout();
            tabPageHelp.SuspendLayout();
            flowLayoutPanelHelp.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonNew, toolStripButtonOpen, toolStripButtonSave, toolStripSeparator1, toolStripButtonAddChanges, toolStripButtonAddFile, toolStripButtonRemove, toolStripSeparator2, toolStripButtonApply });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripButtonNew
            //
            toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonNew.Name = "toolStripButtonNew";
            toolStripButtonNew.Text = "New";
            toolStripButtonNew.Click += OnClickNew;
            //
            // toolStripButtonOpen
            //
            toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOpen.Name = "toolStripButtonOpen";
            toolStripButtonOpen.Text = "Open...";
            toolStripButtonOpen.Click += OnClickOpen;
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save as...";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonAddChanges
            //
            toolStripButtonAddChanges.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAddChanges.Name = "toolStripButtonAddChanges";
            toolStripButtonAddChanges.Text = "Add changes of the client";
            toolStripButtonAddChanges.ToolTipText = "Every new, changed and removed entry of the open UOP files (save or keep them unsaved)";
            toolStripButtonAddChanges.Click += OnClickAddChanges;
            //
            // toolStripButtonAddFile
            //
            toolStripButtonAddFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAddFile.Name = "toolStripButtonAddFile";
            toolStripButtonAddFile.Text = "Add file...";
            toolStripButtonAddFile.ToolTipText = "A file from disk as entry of a UOP file (e.g. a DDS as build/worldart/00012345.dds)";
            toolStripButtonAddFile.Click += OnClickAddFile;
            //
            // toolStripButtonRemove
            //
            toolStripButtonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRemove.Name = "toolStripButtonRemove";
            toolStripButtonRemove.Text = "Remove from patch";
            toolStripButtonRemove.Click += OnClickRemove;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonApply
            //
            toolStripButtonApply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonApply.Name = "toolStripButtonApply";
            toolStripButtonApply.Text = "Apply to a client...";
            toolStripButtonApply.ToolTipText = "Writes the patch into the UOP files of a client folder (with backup)";
            toolStripButtonApply.Click += OnClickApply;
            //
            // tabControl
            //
            tabControl.Controls.Add(tabPagePatch);
            tabControl.Controls.Add(tabPageHelp);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.TabIndex = 1;
            //
            // tabPagePatch
            //
            tabPagePatch.Controls.Add(listViewEntries);
            tabPagePatch.Controls.Add(tableLayoutPanelInfo);
            tabPagePatch.Location = new System.Drawing.Point(4, 24);
            tabPagePatch.Name = "tabPagePatch";
            tabPagePatch.Padding = new System.Windows.Forms.Padding(3);
            tabPagePatch.Size = new System.Drawing.Size(500, 300);
            tabPagePatch.TabIndex = 0;
            tabPagePatch.Text = "Patch";
            tabPagePatch.UseVisualStyleBackColor = true;
            //
            // tableLayoutPanelInfo
            //
            tableLayoutPanelInfo.Controls.Add(labelTitle, 0, 0);
            tableLayoutPanelInfo.Controls.Add(textBoxTitle, 1, 0);
            tableLayoutPanelInfo.Controls.Add(labelAuthor, 0, 1);
            tableLayoutPanelInfo.Controls.Add(textBoxAuthor, 1, 1);
            tableLayoutPanelInfo.Controls.Add(labelDescription, 0, 2);
            tableLayoutPanelInfo.Controls.Add(textBoxDescription, 1, 2);
            tableLayoutPanelInfo.Controls.Add(checkBoxBackup, 1, 3);
            tableLayoutPanelInfo.ColumnCount = 2;
            tableLayoutPanelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            tableLayoutPanelInfo.Name = "tableLayoutPanelInfo";
            tableLayoutPanelInfo.RowCount = 4;
            tableLayoutPanelInfo.Size = new System.Drawing.Size(900, 150);
            tableLayoutPanelInfo.TabIndex = 0;
            tableLayoutPanelInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelInfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableLayoutPanelInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableLayoutPanelInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelInfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            //
            // labelTitle
            //
            labelTitle.AutoSize = true;
            labelTitle.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelTitle.Name = "labelTitle";
            labelTitle.Text = "Title:";
            //
            // textBoxTitle
            //
            textBoxTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxTitle.Name = "textBoxTitle";
            textBoxTitle.TabIndex = 1;
            textBoxTitle.TextChanged += OnInfoChanged;
            //
            // labelAuthor
            //
            labelAuthor.AutoSize = true;
            labelAuthor.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Text = "Author:";
            //
            // textBoxAuthor
            //
            textBoxAuthor.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxAuthor.Name = "textBoxAuthor";
            textBoxAuthor.TabIndex = 3;
            textBoxAuthor.TextChanged += OnInfoChanged;
            //
            // labelDescription
            //
            labelDescription.AutoSize = true;
            labelDescription.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelDescription.Name = "labelDescription";
            labelDescription.Text = "Description:";
            //
            // textBoxDescription
            //
            textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxDescription.Multiline = true;
            textBoxDescription.Name = "textBoxDescription";
            textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxDescription.TabIndex = 5;
            textBoxDescription.TextChanged += OnInfoChanged;
            //
            // checkBoxBackup
            //
            checkBoxBackup.AutoSize = true;
            checkBoxBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBackup.Checked = true;
            checkBoxBackup.Name = "checkBoxBackup";
            checkBoxBackup.TabIndex = 6;
            checkBoxBackup.Text = "Backup when applying (old + new copy in \"UoFiddlerBackups\\\\Patches\" next to the files)";
            checkBoxBackup.UseVisualStyleBackColor = true;
            //
            // listViewEntries
            //
            listViewEntries.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderFile, columnHeaderName, columnHeaderAction, columnHeaderSize });
            listViewEntries.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewEntries.FullRowSelect = true;
            listViewEntries.HideSelection = false;
            listViewEntries.Name = "listViewEntries";
            listViewEntries.TabIndex = 1;
            listViewEntries.UseCompatibleStateImageBehavior = false;
            listViewEntries.View = System.Windows.Forms.View.Details;
            listViewEntries.KeyDown += OnEntriesKeyDown;
            //
            // columnHeaderFile
            //
            columnHeaderFile.Text = "UOP file";
            columnHeaderFile.Width = 170;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Entry";
            columnHeaderName.Width = 380;
            //
            // columnHeaderAction
            //
            columnHeaderAction.Text = "Action";
            columnHeaderAction.Width = 90;
            //
            // columnHeaderSize
            //
            columnHeaderSize.Text = "Size";
            columnHeaderSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderSize.Width = 100;
            //
            // tabPageHelp
            //
            tabPageHelp.Controls.Add(richTextBoxHelp);
            tabPageHelp.Controls.Add(flowLayoutPanelHelp);
            tabPageHelp.Location = new System.Drawing.Point(4, 24);
            tabPageHelp.Name = "tabPageHelp";
            tabPageHelp.Padding = new System.Windows.Forms.Padding(3);
            tabPageHelp.Size = new System.Drawing.Size(500, 300);
            tabPageHelp.TabIndex = 1;
            tabPageHelp.Text = "Help";
            tabPageHelp.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelHelp
            //
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpEnglish);
            flowLayoutPanelHelp.Controls.Add(radioButtonHelpGerman);
            flowLayoutPanelHelp.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelHelp.Name = "flowLayoutPanelHelp";
            flowLayoutPanelHelp.Size = new System.Drawing.Size(500, 30);
            //
            // radioButtonHelpEnglish
            //
            radioButtonHelpEnglish.AutoSize = true;
            radioButtonHelpEnglish.Checked = true;
            radioButtonHelpEnglish.Name = "radioButtonHelpEnglish";
            radioButtonHelpEnglish.TabStop = true;
            radioButtonHelpEnglish.Text = "English";
            radioButtonHelpEnglish.UseVisualStyleBackColor = true;
            radioButtonHelpEnglish.CheckedChanged += OnHelpLanguageChanged;
            //
            // radioButtonHelpGerman
            //
            radioButtonHelpGerman.AutoSize = true;
            radioButtonHelpGerman.Name = "radioButtonHelpGerman";
            radioButtonHelpGerman.Text = "Deutsch";
            radioButtonHelpGerman.UseVisualStyleBackColor = true;
            radioButtonHelpGerman.CheckedChanged += OnHelpLanguageChanged;
            //
            // richTextBoxHelp
            //
            richTextBoxHelp.BackColor = System.Drawing.Color.White;
            richTextBoxHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxHelp.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxHelp.Name = "richTextBoxHelp";
            richTextBoxHelp.ReadOnly = true;
            richTextBoxHelp.WordWrap = false;
            //
            // statusStrip
            //
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel, toolStripProgressBar });
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
            // toolStripProgressBar
            //
            toolStripProgressBar.Name = "toolStripProgressBar";
            toolStripProgressBar.Size = new System.Drawing.Size(160, 16);
            //
            // EcPatchControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcPatchControl";
            Size = new System.Drawing.Size(950, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            tabControl.ResumeLayout(false);
            tabPagePatch.ResumeLayout(false);
            tabPagePatch.PerformLayout();
            tableLayoutPanelInfo.ResumeLayout(false);
            tableLayoutPanelInfo.PerformLayout();
            tabPageHelp.ResumeLayout(false);
            tabPageHelp.PerformLayout();
            flowLayoutPanelHelp.ResumeLayout(false);
            flowLayoutPanelHelp.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddChanges;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddFile;
        private System.Windows.Forms.ToolStripButton toolStripButtonRemove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPagePatch;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelInfo;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.TextBox textBoxTitle;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textBoxAuthor;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textBoxDescription;
        private System.Windows.Forms.CheckBox checkBoxBackup;
        private System.Windows.Forms.ListView listViewEntries;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderAction;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.TabPage tabPageHelp;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelHelp;
        private System.Windows.Forms.RadioButton radioButtonHelpEnglish;
        private System.Windows.Forms.RadioButton radioButtonHelpGerman;
        private System.Windows.Forms.RichTextBox richTextBoxHelp;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
    }
}

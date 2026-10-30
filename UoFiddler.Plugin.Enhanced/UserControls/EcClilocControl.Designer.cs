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
    partial class EcClilocControl
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
            toolStripLabelLanguage = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxLanguage = new System.Windows.Forms.ToolStripComboBox();
            toolStripLabelSearch = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownImportExport = new System.Windows.Forms.ToolStripDropDownButton();
            exportTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            listViewCliloc = new System.Windows.Forms.ListView();
            columnHeaderNumber = new System.Windows.Forms.ColumnHeader();
            columnHeaderFlag = new System.Windows.Forms.ColumnHeader();
            columnHeaderText = new System.Windows.Forms.ColumnHeader();
            panelEdit = new System.Windows.Forms.Panel();
            textBoxText = new System.Windows.Forms.TextBox();
            buttonApplyText = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            panelEdit.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelLanguage, toolStripComboBoxLanguage, toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonApply, toolStripSeparator1, toolStripButtonAdd, toolStripButtonDelete, toolStripDropDownImportExport, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelLanguage
            //
            toolStripLabelLanguage.Name = "toolStripLabelLanguage";
            toolStripLabelLanguage.Text = "Language:";
            //
            // toolStripComboBoxLanguage
            //
            toolStripComboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxLanguage.Name = "toolStripComboBoxLanguage";
            toolStripComboBoxLanguage.Size = new System.Drawing.Size(180, 25);
            toolStripComboBoxLanguage.SelectedIndexChanged += OnLanguageChanged;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Search:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(200, 25);
            toolStripTextBoxSearch.ToolTipText = "Number or part of the text – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
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
            // toolStripButtonAdd
            //
            toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAdd.Name = "toolStripButtonAdd";
            toolStripButtonAdd.Text = "Add...";
            toolStripButtonAdd.Click += OnClickAdd;
            //
            // toolStripButtonDelete
            //
            toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonDelete.Name = "toolStripButtonDelete";
            toolStripButtonDelete.Text = "Delete";
            toolStripButtonDelete.Click += OnClickDelete;
            //
            // toolStripDropDownImportExport
            //
            toolStripDropDownImportExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportTextToolStripMenuItem, importTextToolStripMenuItem });
            toolStripDropDownImportExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownImportExport.Name = "toolStripDropDownImportExport";
            toolStripDropDownImportExport.Text = "Import / Export";
            //
            // exportTextToolStripMenuItem
            //
            exportTextToolStripMenuItem.Name = "exportTextToolStripMenuItem";
            exportTextToolStripMenuItem.Text = "Export language as text (id <tab> text)...";
            exportTextToolStripMenuItem.Click += OnClickExportText;
            //
            // importTextToolStripMenuItem
            //
            importTextToolStripMenuItem.Name = "importTextToolStripMenuItem";
            importTextToolStripMenuItem.Text = "Import text (id <tab> text, adds / changes)...";
            importTextToolStripMenuItem.Click += OnClickImportText;
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
            // listViewCliloc
            //
            listViewCliloc.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderNumber, columnHeaderFlag, columnHeaderText });
            listViewCliloc.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewCliloc.FullRowSelect = true;
            listViewCliloc.HideSelection = false;
            listViewCliloc.MultiSelect = false;
            listViewCliloc.Name = "listViewCliloc";
            listViewCliloc.TabIndex = 1;
            listViewCliloc.UseCompatibleStateImageBehavior = false;
            listViewCliloc.View = System.Windows.Forms.View.Details;
            listViewCliloc.VirtualMode = true;
            listViewCliloc.RetrieveVirtualItem += RetrieveItem;
            listViewCliloc.SelectedIndexChanged += OnListSelectedIndexChanged;
            //
            // columnHeaderNumber
            //
            columnHeaderNumber.Text = "Number";
            columnHeaderNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderNumber.Width = 90;
            //
            // columnHeaderFlag
            //
            columnHeaderFlag.Text = "Flag";
            columnHeaderFlag.Width = 40;
            //
            // columnHeaderText
            //
            columnHeaderText.Text = "Text";
            columnHeaderText.Width = 800;
            //
            // panelEdit
            //
            panelEdit.Controls.Add(textBoxText);
            panelEdit.Controls.Add(buttonApplyText);
            panelEdit.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelEdit.Name = "panelEdit";
            panelEdit.Size = new System.Drawing.Size(900, 80);
            panelEdit.TabIndex = 3;
            //
            // textBoxText
            //
            textBoxText.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxText.Multiline = true;
            textBoxText.Name = "textBoxText";
            textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxText.TabIndex = 0;
            //
            // buttonApplyText
            //
            buttonApplyText.Dock = System.Windows.Forms.DockStyle.Right;
            buttonApplyText.Name = "buttonApplyText";
            buttonApplyText.Size = new System.Drawing.Size(100, 80);
            buttonApplyText.TabIndex = 1;
            buttonApplyText.Text = "Apply text";
            buttonApplyText.UseVisualStyleBackColor = true;
            buttonApplyText.Click += OnClickApplyText;
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
            // EcClilocControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(listViewCliloc);
            Controls.Add(panelEdit);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcClilocControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            panelEdit.ResumeLayout(false);
            panelEdit.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelLanguage;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxLanguage;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownImportExport;
        private System.Windows.Forms.ToolStripMenuItem exportTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.ListView listViewCliloc;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.ColumnHeader columnHeaderFlag;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.Panel panelEdit;
        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.Button buttonApplyText;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

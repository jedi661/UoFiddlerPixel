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
    partial class EcStringsControl
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
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonAdd = new System.Windows.Forms.ToolStripButton();
            toolStripButtonEdit = new System.Windows.Forms.ToolStripButton();
            toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            listViewStrings = new System.Windows.Forms.ListView();
            columnHeaderIndex = new System.Windows.Forms.ColumnHeader();
            columnHeaderText = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            copyTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyIndexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxFilter, toolStripButtonApply, toolStripSeparator1, toolStripButtonAdd, toolStripButtonEdit, toolStripButtonExport, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
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
            toolStripTextBoxFilter.Size = new System.Drawing.Size(220, 25);
            toolStripTextBoxFilter.ToolTipText = "Part of the text, or #index – Enter";
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
            // toolStripButtonAdd
            //
            toolStripButtonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAdd.Name = "toolStripButtonAdd";
            toolStripButtonAdd.Text = "Add...";
            toolStripButtonAdd.Click += OnClickAdd;
            //
            // toolStripButtonEdit
            //
            toolStripButtonEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonEdit.Name = "toolStripButtonEdit";
            toolStripButtonEdit.Text = "Edit...";
            toolStripButtonEdit.ToolTipText = "Change the text (every tileart entry using it changes too)";
            toolStripButtonEdit.Click += OnClickEdit;
            //
            // toolStripButtonExport
            //
            toolStripButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonExport.Name = "toolStripButtonExport";
            toolStripButtonExport.Text = "Export (txt)...";
            toolStripButtonExport.Click += OnClickExport;
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
            // listViewStrings
            //
            listViewStrings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderIndex, columnHeaderText });
            listViewStrings.ContextMenuStrip = contextMenuStripList;
            listViewStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewStrings.FullRowSelect = true;
            listViewStrings.HideSelection = false;
            listViewStrings.MultiSelect = false;
            listViewStrings.Name = "listViewStrings";
            listViewStrings.TabIndex = 1;
            listViewStrings.UseCompatibleStateImageBehavior = false;
            listViewStrings.View = System.Windows.Forms.View.Details;
            listViewStrings.VirtualMode = true;
            listViewStrings.RetrieveVirtualItem += RetrieveItem;
            listViewStrings.DoubleClick += OnClickEdit;
            //
            // columnHeaderIndex
            //
            columnHeaderIndex.Text = "Index";
            columnHeaderIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderIndex.Width = 80;
            //
            // columnHeaderText
            //
            columnHeaderText.Text = "Text";
            columnHeaderText.Width = 700;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyTextToolStripMenuItem, copyIndexToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // copyTextToolStripMenuItem
            //
            copyTextToolStripMenuItem.Name = "copyTextToolStripMenuItem";
            copyTextToolStripMenuItem.Text = "Copy text";
            copyTextToolStripMenuItem.Click += OnClickCopyText;
            //
            // copyIndexToolStripMenuItem
            //
            copyIndexToolStripMenuItem.Name = "copyIndexToolStripMenuItem";
            copyIndexToolStripMenuItem.Text = "Copy index";
            copyIndexToolStripMenuItem.Click += OnClickCopyIndex;
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
            // EcStringsControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(listViewStrings);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcStringsControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            contextMenuStripList.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAdd;
        private System.Windows.Forms.ToolStripButton toolStripButtonEdit;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.ListView listViewStrings;
        private System.Windows.Forms.ColumnHeader columnHeaderIndex;
        private System.Windows.Forms.ColumnHeader columnHeaderText;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem copyTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyIndexToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

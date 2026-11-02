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
    partial class EcSearchControl
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
            toolStripLabelKind = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxKind = new System.Windows.Forms.ToolStripComboBox();
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            toolStripButtonCancel = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonMaps = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            listViewResults = new System.Windows.Forms.ListView();
            columnHeaderArea = new System.Windows.Forms.ColumnHeader();
            columnHeaderWhere = new System.Windows.Forms.ColumnHeader();
            columnHeaderDetail = new System.Windows.Forms.ColumnHeader();
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            goToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelPreview = new System.Windows.Forms.Panel();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            labelPreview = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStrip.SuspendLayout();
            panelPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelKind, toolStripComboBoxKind, toolStripTextBoxSearch, toolStripButtonSearch, toolStripButtonCancel, toolStripSeparator1, toolStripButtonMaps });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelKind
            //
            toolStripLabelKind.Name = "toolStripLabelKind";
            toolStripLabelKind.Text = "Search:";
            //
            // toolStripComboBoxKind
            //
            toolStripComboBoxKind.Items.AddRange(new object[] { "Item id", "Gump id", "Hue", "Multi id", "Name / text" });
            toolStripComboBoxKind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxKind.Name = "toolStripComboBoxKind";
            toolStripComboBoxKind.Size = new System.Drawing.Size(110, 25);
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(160, 25);
            toolStripTextBoxSearch.ToolTipText = "Id (decimal or 0x hex) or text – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonSearch
            //
            toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSearch.Name = "toolStripButtonSearch";
            toolStripButtonSearch.Text = "Search";
            toolStripButtonSearch.Click += OnClickSearch;
            //
            // toolStripButtonCancel
            //
            toolStripButtonCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonCancel.Enabled = false;
            toolStripButtonCancel.Name = "toolStripButtonCancel";
            toolStripButtonCancel.Text = "Cancel";
            toolStripButtonCancel.Click += OnClickCancel;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonMaps
            //
            toolStripButtonMaps.CheckOnClick = true;
            toolStripButtonMaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonMaps.Name = "toolStripButtonMaps";
            toolStripButtonMaps.Text = "Search the maps too";
            toolStripButtonMaps.ToolTipText = "Item and hue: every static of every facet (takes a while)";
            //
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(listViewResults);
            splitContainer.Panel2.Controls.Add(panelPreview);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 700;
            splitContainer.TabIndex = 1;
            //
            // listViewResults
            //
            listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderArea, columnHeaderWhere, columnHeaderDetail });
            listViewResults.ContextMenuStrip = contextMenuStrip;
            listViewResults.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewResults.FullRowSelect = true;
            listViewResults.HideSelection = false;
            listViewResults.MultiSelect = false;
            listViewResults.Name = "listViewResults";
            listViewResults.TabIndex = 0;
            listViewResults.UseCompatibleStateImageBehavior = false;
            listViewResults.View = System.Windows.Forms.View.Details;
            listViewResults.DoubleClick += OnClickGoTo;
            listViewResults.SelectedIndexChanged += OnResultSelected;
            //
            // columnHeaderArea
            //
            columnHeaderArea.Text = "Area";
            columnHeaderArea.Width = 110;
            //
            // columnHeaderWhere
            //
            columnHeaderWhere.Text = "Where";
            columnHeaderWhere.Width = 230;
            //
            // columnHeaderDetail
            //
            columnHeaderDetail.Text = "Details";
            columnHeaderDetail.Width = 340;
            //
            // contextMenuStrip
            //
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { goToToolStripMenuItem, copyToolStripMenuItem, exportToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            //
            // goToToolStripMenuItem
            //
            goToToolStripMenuItem.Name = "goToToolStripMenuItem";
            goToToolStripMenuItem.Text = "Show in its tab";
            goToToolStripMenuItem.Click += OnClickGoTo;
            //
            // copyToolStripMenuItem
            //
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Text = "Copy line";
            copyToolStripMenuItem.Click += OnClickCopy;
            //
            // exportToolStripMenuItem
            //
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Text = "Save all results...";
            exportToolStripMenuItem.Click += OnClickExport;
            //
            // panelPreview
            //
            panelPreview.Controls.Add(pictureBoxPreview);
            panelPreview.Controls.Add(labelPreview);
            panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPreview.Name = "panelPreview";
            panelPreview.Padding = new System.Windows.Forms.Padding(6);
            panelPreview.TabIndex = 0;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.White;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabStop = false;
            //
            // labelPreview
            //
            labelPreview.Dock = System.Windows.Forms.DockStyle.Top;
            labelPreview.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelPreview.Name = "labelPreview";
            labelPreview.Padding = new System.Windows.Forms.Padding(4);
            labelPreview.Size = new System.Drawing.Size(280, 70);
            labelPreview.Text = "Double click a result to show it in its tab.";
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
            // EcSearchControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcSearchControl";
            Size = new System.Drawing.Size(950, 600);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStrip.ResumeLayout(false);
            panelPreview.ResumeLayout(false);
            panelPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelKind;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxKind;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonCancel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonMaps;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewResults;
        private System.Windows.Forms.ColumnHeader columnHeaderArea;
        private System.Windows.Forms.ColumnHeader columnHeaderWhere;
        private System.Windows.Forms.ColumnHeader columnHeaderDetail;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem goToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
    }
}

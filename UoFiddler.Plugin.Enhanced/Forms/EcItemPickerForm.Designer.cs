/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Enhanced.Forms
{
    partial class EcItemPickerForm
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
            toolStripTextBoxSearch = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonSearch = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonOnlyEc = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            listViewItems = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderHex = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            labelInfo = new System.Windows.Forms.Label();
            flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonCancel = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            flowLayoutPanelButtons.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonSearch, toolStripSeparator1, toolStripButtonOnlyEc });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelSearch
            //
            toolStripLabelSearch.Name = "toolStripLabelSearch";
            toolStripLabelSearch.Text = "Id or name:";
            //
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(180, 25);
            toolStripTextBoxSearch.ToolTipText = "Id (decimal or 0x hex) jumps to it, text filters by name – Enter";
            toolStripTextBoxSearch.KeyDown += OnSearchKeyDown;
            //
            // toolStripButtonSearch
            //
            toolStripButtonSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSearch.Name = "toolStripButtonSearch";
            toolStripButtonSearch.Text = "Search";
            toolStripButtonSearch.Click += OnClickSearch;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripButtonOnlyEc
            //
            toolStripButtonOnlyEc.CheckOnClick = true;
            toolStripButtonOnlyEc.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonOnlyEc.Name = "toolStripButtonOnlyEc";
            toolStripButtonOnlyEc.Text = "Only with EC image";
            toolStripButtonOnlyEc.CheckedChanged += OnClickSearch;
            //
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(listViewItems);
            splitContainer.Panel2.Controls.Add(pictureBoxPreview);
            splitContainer.Panel2.Controls.Add(labelInfo);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(640, 420);
            splitContainer.SplitterDistance = 380;
            splitContainer.TabIndex = 1;
            //
            // listViewItems
            //
            listViewItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderHex, columnHeaderName });
            listViewItems.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewItems.FullRowSelect = true;
            listViewItems.HideSelection = false;
            listViewItems.MultiSelect = false;
            listViewItems.Name = "listViewItems";
            listViewItems.TabIndex = 0;
            listViewItems.UseCompatibleStateImageBehavior = false;
            listViewItems.View = System.Windows.Forms.View.Details;
            listViewItems.VirtualMode = true;
            listViewItems.RetrieveVirtualItem += RetrieveItem;
            listViewItems.SelectedIndexChanged += OnSelectionChanged;
            listViewItems.DoubleClick += OnClickOk;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderId.Width = 60;
            //
            // columnHeaderHex
            //
            columnHeaderHex.Text = "Hex";
            columnHeaderHex.Width = 70;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 220;
            //
            // pictureBoxPreview
            //
            pictureBoxPreview.BackColor = System.Drawing.Color.White;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabStop = false;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(250, 48);
            //
            // flowLayoutPanelButtons
            //
            flowLayoutPanelButtons.Controls.Add(buttonCancel);
            flowLayoutPanelButtons.Controls.Add(buttonOk);
            flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            flowLayoutPanelButtons.Padding = new System.Windows.Forms.Padding(6);
            flowLayoutPanelButtons.Size = new System.Drawing.Size(640, 42);
            flowLayoutPanelButtons.TabIndex = 2;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(90, 27);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonOk
            //
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(90, 27);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "Place";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += OnClickOk;
            //
            // EcItemPickerForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(flowLayoutPanelButtons);
            Controls.Add(toolStrip);
            Name = "EcItemPickerForm";
            AcceptButton = buttonOk;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(640, 500);
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Choose an item";
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            flowLayoutPanelButtons.ResumeLayout(false);
            flowLayoutPanelButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonSearch;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonOnlyEc;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewItems;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderHex;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
    }
}

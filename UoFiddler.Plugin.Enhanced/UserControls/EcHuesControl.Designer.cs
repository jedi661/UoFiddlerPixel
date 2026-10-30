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
    partial class EcHuesControl
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
            toolStripButtonApply = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonImportRamp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonExportRamp = new System.Windows.Forms.ToolStripButton();
            toolStripButtonAddCopy = new System.Windows.Forms.ToolStripButton();
            toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonSaveAs = new System.Windows.Forms.ToolStripButton();
            splitContainer = new System.Windows.Forms.SplitContainer();
            listViewHues = new System.Windows.Forms.ListView();
            columnHeaderHue = new System.Windows.Forms.ColumnHeader();
            columnHeaderRamp = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            contextMenuStripList = new System.Windows.Forms.ContextMenuStrip(components);
            importRampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportRampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addCopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelDetails = new System.Windows.Forms.Panel();
            flowLayoutPanelName = new System.Windows.Forms.FlowLayoutPanel();
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            buttonSetName = new System.Windows.Forms.Button();
            pictureBoxRamp = new System.Windows.Forms.PictureBox();
            labelInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripList.SuspendLayout();
            panelDetails.SuspendLayout();
            flowLayoutPanelName.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRamp).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelSearch, toolStripTextBoxSearch, toolStripButtonApply, toolStripSeparator1, toolStripButtonImportRamp, toolStripButtonExportRamp, toolStripButtonAddCopy, toolStripButtonDelete, toolStripSeparator2, toolStripButtonSave, toolStripButtonSaveAs });
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
            // toolStripTextBoxSearch
            //
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new System.Drawing.Size(150, 25);
            toolStripTextBoxSearch.ToolTipText = "Number or part of the name – Enter";
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
            // toolStripButtonImportRamp
            //
            toolStripButtonImportRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonImportRamp.Name = "toolStripButtonImportRamp";
            toolStripButtonImportRamp.Text = "Import ramp...";
            toolStripButtonImportRamp.ToolTipText = "Replace the color ramp with an image (best 256 x 2)";
            toolStripButtonImportRamp.Click += OnClickImportRamp;
            //
            // toolStripButtonExportRamp
            //
            toolStripButtonExportRamp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonExportRamp.Name = "toolStripButtonExportRamp";
            toolStripButtonExportRamp.Text = "Export ramp...";
            toolStripButtonExportRamp.Click += OnClickExportRamp;
            //
            // toolStripButtonAddCopy
            //
            toolStripButtonAddCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonAddCopy.Name = "toolStripButtonAddCopy";
            toolStripButtonAddCopy.Text = "Add copy...";
            toolStripButtonAddCopy.ToolTipText = "New hue as copy of the selected one";
            toolStripButtonAddCopy.Click += OnClickAddCopy;
            //
            // toolStripButtonDelete
            //
            toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonDelete.Name = "toolStripButtonDelete";
            toolStripButtonDelete.Text = "Delete";
            toolStripButtonDelete.Click += OnClickDelete;
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
            splitContainer.Panel1.Controls.Add(listViewHues);
            splitContainer.Panel2.Controls.Add(panelDetails);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(900, 450);
            splitContainer.SplitterDistance = 560;
            splitContainer.TabIndex = 1;
            //
            // listViewHues
            //
            listViewHues.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderHue, columnHeaderRamp, columnHeaderName });
            listViewHues.ContextMenuStrip = contextMenuStripList;
            listViewHues.Dock = System.Windows.Forms.DockStyle.Fill;
            listViewHues.FullRowSelect = true;
            listViewHues.HideSelection = false;
            listViewHues.MultiSelect = false;
            listViewHues.Name = "listViewHues";
            listViewHues.OwnerDraw = true;
            listViewHues.TabIndex = 0;
            listViewHues.UseCompatibleStateImageBehavior = false;
            listViewHues.View = System.Windows.Forms.View.Details;
            listViewHues.VirtualMode = true;
            listViewHues.RetrieveVirtualItem += RetrieveItem;
            listViewHues.DrawColumnHeader += OnDrawColumnHeader;
            listViewHues.DrawSubItem += DrawSubItem;
            listViewHues.SelectedIndexChanged += OnListSelectedIndexChanged;
            //
            // columnHeaderHue
            //
            columnHeaderHue.Text = "Hue";
            columnHeaderHue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderHue.Width = 60;
            //
            // columnHeaderRamp
            //
            columnHeaderRamp.Text = "Ramp";
            columnHeaderRamp.Width = 140;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 300;
            //
            // contextMenuStripList
            //
            contextMenuStripList.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { importRampToolStripMenuItem, exportRampToolStripMenuItem, addCopyToolStripMenuItem, deleteToolStripMenuItem, toolStripSeparatorMenu, saveToolStripMenuItem });
            contextMenuStripList.Name = "contextMenuStripList";
            //
            // importRampToolStripMenuItem
            //
            importRampToolStripMenuItem.Name = "importRampToolStripMenuItem";
            importRampToolStripMenuItem.Text = "Import ramp...";
            importRampToolStripMenuItem.Click += OnClickImportRamp;
            //
            // exportRampToolStripMenuItem
            //
            exportRampToolStripMenuItem.Name = "exportRampToolStripMenuItem";
            exportRampToolStripMenuItem.Text = "Export ramp...";
            exportRampToolStripMenuItem.Click += OnClickExportRamp;
            //
            // addCopyToolStripMenuItem
            //
            addCopyToolStripMenuItem.Name = "addCopyToolStripMenuItem";
            addCopyToolStripMenuItem.Text = "Add copy...";
            addCopyToolStripMenuItem.Click += OnClickAddCopy;
            //
            // deleteToolStripMenuItem
            //
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += OnClickDelete;
            //
            // toolStripSeparatorMenu
            //
            toolStripSeparatorMenu.Name = "toolStripSeparatorMenu";
            //
            // saveToolStripMenuItem
            //
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            //
            // panelDetails
            //
            panelDetails.Controls.Add(flowLayoutPanelName);
            panelDetails.Controls.Add(pictureBoxRamp);
            panelDetails.Controls.Add(labelInfo);
            panelDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            panelDetails.Name = "panelDetails";
            panelDetails.Padding = new System.Windows.Forms.Padding(6);
            panelDetails.TabIndex = 0;
            //
            // flowLayoutPanelName
            //
            flowLayoutPanelName.Controls.Add(labelName);
            flowLayoutPanelName.Controls.Add(textBoxName);
            flowLayoutPanelName.Controls.Add(buttonSetName);
            flowLayoutPanelName.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelName.Name = "flowLayoutPanelName";
            flowLayoutPanelName.Size = new System.Drawing.Size(320, 34);
            flowLayoutPanelName.TabIndex = 2;
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Margin = new System.Windows.Forms.Padding(3, 8, 0, 0);
            labelName.Name = "labelName";
            labelName.Text = "Name:";
            //
            // textBoxName
            //
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new System.Drawing.Size(220, 23);
            textBoxName.TabIndex = 1;
            //
            // buttonSetName
            //
            buttonSetName.AutoSize = true;
            buttonSetName.Name = "buttonSetName";
            buttonSetName.TabIndex = 2;
            buttonSetName.Text = "Set name";
            buttonSetName.UseVisualStyleBackColor = true;
            buttonSetName.Click += OnClickSetName;
            //
            // pictureBoxRamp
            //
            pictureBoxRamp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxRamp.Dock = System.Windows.Forms.DockStyle.Top;
            pictureBoxRamp.Name = "pictureBoxRamp";
            pictureBoxRamp.Size = new System.Drawing.Size(320, 60);
            pictureBoxRamp.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBoxRamp.TabStop = false;
            //
            // labelInfo
            //
            labelInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelInfo.Name = "labelInfo";
            labelInfo.Padding = new System.Windows.Forms.Padding(4);
            labelInfo.Size = new System.Drawing.Size(320, 36);
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
            // EcHuesControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcHuesControl";
            Size = new System.Drawing.Size(900, 500);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripList.ResumeLayout(false);
            panelDetails.ResumeLayout(false);
            panelDetails.PerformLayout();
            flowLayoutPanelName.ResumeLayout(false);
            flowLayoutPanelName.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxRamp).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelSearch;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearch;
        private System.Windows.Forms.ToolStripButton toolStripButtonApply;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButtonImportRamp;
        private System.Windows.Forms.ToolStripButton toolStripButtonExportRamp;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddCopy;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonSaveAs;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ListView listViewHues;
        private System.Windows.Forms.ColumnHeader columnHeaderHue;
        private System.Windows.Forms.ColumnHeader columnHeaderRamp;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripList;
        private System.Windows.Forms.ToolStripMenuItem importRampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addCopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panelDetails;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button buttonSetName;
        private System.Windows.Forms.PictureBox pictureBoxRamp;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

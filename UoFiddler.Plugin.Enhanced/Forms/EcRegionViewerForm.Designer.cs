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
    partial class EcRegionViewerForm
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
            toolStripLabelArt = new System.Windows.Forms.ToolStripLabel();
            toolStripComboBoxArt = new System.Windows.Forms.ToolStripComboBox();
            toolStripButtonLand = new System.Windows.Forms.ToolStripButton();
            toolStripButtonLandArt = new System.Windows.Forms.ToolStripButton();
            toolStripButtonStatics = new System.Windows.Forms.ToolStripButton();
            toolStripButtonGrid = new System.Windows.Forms.ToolStripButton();
            toolStripLabelMaxZ = new System.Windows.Forms.ToolStripLabel();
            toolStripTextBoxMaxZ = new System.Windows.Forms.ToolStripTextBox();
            toolStripButtonRefresh = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownStatics = new System.Windows.Forms.ToolStripDropDownButton();
            importStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportMultiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorStatics = new System.Windows.Forms.ToolStripSeparator();
            clearStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorStatics2 = new System.Windows.Forms.ToolStripSeparator();
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabelHint = new System.Windows.Forms.ToolStripLabel();
            splitContainer = new System.Windows.Forms.SplitContainer();
            previewPanel = new UoFiddler.Plugin.Enhanced.UserControls.EcPreviewPanel();
            contextMenuStripPreview = new System.Windows.Forms.ContextMenuStrip(components);
            addStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeStaticToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorMenu = new System.Windows.Forms.ToolStripSeparator();
            copyIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelTile = new System.Windows.Forms.Panel();
            pictureBoxStatic = new System.Windows.Forms.PictureBox();
            flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonAdd = new System.Windows.Forms.Button();
            buttonEdit = new System.Windows.Forms.Button();
            buttonRemove = new System.Windows.Forms.Button();
            listViewTileStatics = new System.Windows.Forms.ListView();
            columnHeaderItem = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderZ = new System.Windows.Forms.ColumnHeader();
            columnHeaderHue = new System.Windows.Forms.ColumnHeader();
            labelTileInfo = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            contextMenuStripPreview.SuspendLayout();
            panelTile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxStatic).BeginInit();
            flowLayoutPanelButtons.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabelArt, toolStripComboBoxArt, toolStripButtonLand, toolStripButtonLandArt, toolStripButtonStatics, toolStripButtonGrid, toolStripLabelMaxZ, toolStripTextBoxMaxZ, toolStripButtonRefresh, toolStripSeparator1, toolStripDropDownStatics, toolStripSeparator2, toolStripButtonSave, toolStripSeparator3, toolStripLabelHint });
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.TabIndex = 0;
            //
            // toolStripLabelArt
            //
            toolStripLabelArt.Name = "toolStripLabelArt";
            toolStripLabelArt.Text = "Art:";
            //
            // toolStripComboBoxArt
            //
            toolStripComboBoxArt.Items.AddRange(new object[] { "Enhanced (EC art)", "Legacy (classic art)" });
            toolStripComboBoxArt.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBoxArt.Name = "toolStripComboBoxArt";
            toolStripComboBoxArt.Size = new System.Drawing.Size(140, 25);
            toolStripComboBoxArt.SelectedIndexChanged += OnViewOptionChanged;
            //
            // toolStripButtonLand
            //
            toolStripButtonLand.CheckOnClick = true;
            toolStripButtonLand.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonLand.Checked = true;
            toolStripButtonLand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonLand.Name = "toolStripButtonLand";
            toolStripButtonLand.Text = "Land";
            toolStripButtonLand.CheckedChanged += OnViewOptionChanged;
            //
            // toolStripButtonLandArt
            //
            toolStripButtonLandArt.CheckOnClick = true;
            toolStripButtonLandArt.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonLandArt.Checked = true;
            toolStripButtonLandArt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonLandArt.Name = "toolStripButtonLandArt";
            toolStripButtonLandArt.Text = "Land art";
            toolStripButtonLandArt.ToolTipText = "Flat land with the land tiles of the classic client loaded in UoFiddler (else radar colors)";
            toolStripButtonLandArt.CheckedChanged += OnViewOptionChanged;
            //
            // toolStripButtonStatics
            //
            toolStripButtonStatics.CheckOnClick = true;
            toolStripButtonStatics.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripButtonStatics.Checked = true;
            toolStripButtonStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonStatics.Name = "toolStripButtonStatics";
            toolStripButtonStatics.Text = "Statics";
            toolStripButtonStatics.CheckedChanged += OnViewOptionChanged;
            //
            // toolStripButtonGrid
            //
            toolStripButtonGrid.CheckOnClick = true;
            toolStripButtonGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonGrid.Name = "toolStripButtonGrid";
            toolStripButtonGrid.Text = "Grid";
            toolStripButtonGrid.CheckedChanged += OnViewOptionChanged;
            //
            // toolStripLabelMaxZ
            //
            toolStripLabelMaxZ.Name = "toolStripLabelMaxZ";
            toolStripLabelMaxZ.Text = "Max z:";
            //
            // toolStripTextBoxMaxZ
            //
            toolStripTextBoxMaxZ.Name = "toolStripTextBoxMaxZ";
            toolStripTextBoxMaxZ.Size = new System.Drawing.Size(40, 25);
            toolStripTextBoxMaxZ.ToolTipText = "Highest z of the statics that are drawn – Enter";
            toolStripTextBoxMaxZ.KeyDown += OnMaxZKeyDown;
            //
            // toolStripButtonRefresh
            //
            toolStripButtonRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            toolStripButtonRefresh.Text = "Refresh";
            toolStripButtonRefresh.Click += OnClickRefresh;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            //
            // toolStripDropDownStatics
            //
            toolStripDropDownStatics.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { importStaticsToolStripMenuItem, exportStaticsToolStripMenuItem, toolStripSeparatorStatics, clearStaticsToolStripMenuItem, toolStripSeparatorStatics2, exportImageToolStripMenuItem });
            toolStripDropDownStatics.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownStatics.Name = "toolStripDropDownStatics";
            toolStripDropDownStatics.Text = "Statics";
            //
            // importStaticsToolStripMenuItem
            //
            importStaticsToolStripMenuItem.Name = "importStaticsToolStripMenuItem";
            importStaticsToolStripMenuItem.Text = "Import statics...";
            importStaticsToolStripMenuItem.ToolTipText = "Own text / CSV export, or a multi (text, UOA, CSV, WSC)";
            importStaticsToolStripMenuItem.Click += OnClickImport;
            //
            // exportStaticsToolStripMenuItem
            //
            exportStaticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportTextToolStripMenuItem, exportCsvToolStripMenuItem, exportMultiToolStripMenuItem });
            exportStaticsToolStripMenuItem.Name = "exportStaticsToolStripMenuItem";
            exportStaticsToolStripMenuItem.Text = "Export statics of the area";
            //
            // exportTextToolStripMenuItem
            //
            exportTextToolStripMenuItem.Name = "exportTextToolStripMenuItem";
            exportTextToolStripMenuItem.Tag = "Text";
            exportTextToolStripMenuItem.Text = "Text (item x y z hue, relative)...";
            exportTextToolStripMenuItem.Click += OnClickExport;
            //
            // exportCsvToolStripMenuItem
            //
            exportCsvToolStripMenuItem.Name = "exportCsvToolStripMenuItem";
            exportCsvToolStripMenuItem.Tag = "Csv";
            exportCsvToolStripMenuItem.Text = "CSV (map coordinates)...";
            exportCsvToolStripMenuItem.Click += OnClickExport;
            //
            // exportMultiToolStripMenuItem
            //
            exportMultiToolStripMenuItem.Name = "exportMultiToolStripMenuItem";
            exportMultiToolStripMenuItem.Tag = "MultiText";
            exportMultiToolStripMenuItem.Text = "Multi text (for the Multis tab)...";
            exportMultiToolStripMenuItem.Click += OnClickExport;
            //
            // toolStripSeparatorStatics
            //
            toolStripSeparatorStatics.Name = "toolStripSeparatorStatics";
            //
            // clearStaticsToolStripMenuItem
            //
            clearStaticsToolStripMenuItem.Name = "clearStaticsToolStripMenuItem";
            clearStaticsToolStripMenuItem.Text = "Remove all statics of the area...";
            clearStaticsToolStripMenuItem.Click += OnClickClearStatics;
            //
            // toolStripSeparatorStatics2
            //
            toolStripSeparatorStatics2.Name = "toolStripSeparatorStatics2";
            //
            // exportImageToolStripMenuItem
            //
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Text = "Save image...";
            exportImageToolStripMenuItem.Click += OnClickSaveImage;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            //
            // toolStripButtonSave
            //
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Text = "Save map";
            toolStripButtonSave.ToolTipText = "Save the facet file (the old file is kept as .bak)";
            toolStripButtonSave.Click += OnClickSave;
            //
            // toolStripSeparator3
            //
            toolStripSeparator3.Name = "toolStripSeparator3";
            //
            // toolStripLabelHint
            //
            toolStripLabelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            toolStripLabelHint.Name = "toolStripLabelHint";
            toolStripLabelHint.Text = "Ctrl + wheel = zoom, click = select";
            //
            // splitContainer
            //
            splitContainer.Panel1.Controls.Add(previewPanel);
            splitContainer.Panel2.Controls.Add(panelTile);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer.Name = "splitContainer";
            splitContainer.Size = new System.Drawing.Size(1000, 600);
            splitContainer.SplitterDistance = 720;
            splitContainer.TabIndex = 1;
            //
            // previewPanel
            //
            previewPanel.AutoScroll = true;
            previewPanel.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            previewPanel.ContextMenuStrip = contextMenuStripPreview;
            previewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            previewPanel.Name = "previewPanel";
            previewPanel.TabIndex = 0;
            previewPanel.MouseMove += OnPreviewMouseMove;
            previewPanel.MouseClick += OnPreviewMouseClick;
            //
            // contextMenuStripPreview
            //
            contextMenuStripPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addStaticToolStripMenuItem, editStaticToolStripMenuItem, removeStaticToolStripMenuItem, toolStripSeparatorMenu, copyIdToolStripMenuItem, copyPositionToolStripMenuItem });
            contextMenuStripPreview.Name = "contextMenuStripPreview";
            //
            // addStaticToolStripMenuItem
            //
            addStaticToolStripMenuItem.Name = "addStaticToolStripMenuItem";
            addStaticToolStripMenuItem.Text = "Add static here...";
            addStaticToolStripMenuItem.Click += OnClickAddStatic;
            //
            // editStaticToolStripMenuItem
            //
            editStaticToolStripMenuItem.Name = "editStaticToolStripMenuItem";
            editStaticToolStripMenuItem.Text = "Edit selected static...";
            editStaticToolStripMenuItem.Click += OnClickEditStatic;
            //
            // removeStaticToolStripMenuItem
            //
            removeStaticToolStripMenuItem.Name = "removeStaticToolStripMenuItem";
            removeStaticToolStripMenuItem.Text = "Remove selected static";
            removeStaticToolStripMenuItem.Click += OnClickRemoveStatic;
            //
            // toolStripSeparatorMenu
            //
            toolStripSeparatorMenu.Name = "toolStripSeparatorMenu";
            //
            // copyIdToolStripMenuItem
            //
            copyIdToolStripMenuItem.Name = "copyIdToolStripMenuItem";
            copyIdToolStripMenuItem.Text = "Copy item id";
            copyIdToolStripMenuItem.Click += OnClickCopyId;
            //
            // copyPositionToolStripMenuItem
            //
            copyPositionToolStripMenuItem.Name = "copyPositionToolStripMenuItem";
            copyPositionToolStripMenuItem.Text = "Copy position x,y,z";
            copyPositionToolStripMenuItem.Click += OnClickCopyPosition;
            //
            // panelTile
            //
            panelTile.Controls.Add(pictureBoxStatic);
            panelTile.Controls.Add(flowLayoutPanelButtons);
            panelTile.Controls.Add(listViewTileStatics);
            panelTile.Controls.Add(labelTileInfo);
            panelTile.Dock = System.Windows.Forms.DockStyle.Fill;
            panelTile.Name = "panelTile";
            panelTile.TabIndex = 0;
            //
            // pictureBoxStatic
            //
            pictureBoxStatic.BackColor = System.Drawing.Color.White;
            pictureBoxStatic.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxStatic.Name = "pictureBoxStatic";
            pictureBoxStatic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxStatic.TabStop = false;
            //
            // flowLayoutPanelButtons
            //
            flowLayoutPanelButtons.Controls.Add(buttonAdd);
            flowLayoutPanelButtons.Controls.Add(buttonEdit);
            flowLayoutPanelButtons.Controls.Add(buttonRemove);
            flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            flowLayoutPanelButtons.Size = new System.Drawing.Size(276, 32);
            flowLayoutPanelButtons.TabIndex = 2;
            //
            // buttonAdd
            //
            buttonAdd.AutoSize = true;
            buttonAdd.Name = "buttonAdd";
            buttonAdd.TabIndex = 0;
            buttonAdd.Text = "Add...";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += OnClickAddStatic;
            //
            // buttonEdit
            //
            buttonEdit.AutoSize = true;
            buttonEdit.Name = "buttonEdit";
            buttonEdit.TabIndex = 1;
            buttonEdit.Text = "Edit...";
            buttonEdit.UseVisualStyleBackColor = true;
            buttonEdit.Click += OnClickEditStatic;
            //
            // buttonRemove
            //
            buttonRemove.AutoSize = true;
            buttonRemove.Name = "buttonRemove";
            buttonRemove.TabIndex = 2;
            buttonRemove.Text = "Remove";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += OnClickRemoveStatic;
            //
            // listViewTileStatics
            //
            listViewTileStatics.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderItem, columnHeaderName, columnHeaderZ, columnHeaderHue });
            listViewTileStatics.Dock = System.Windows.Forms.DockStyle.Top;
            listViewTileStatics.FullRowSelect = true;
            listViewTileStatics.HideSelection = false;
            listViewTileStatics.MultiSelect = false;
            listViewTileStatics.Name = "listViewTileStatics";
            listViewTileStatics.Size = new System.Drawing.Size(276, 160);
            listViewTileStatics.TabIndex = 1;
            listViewTileStatics.UseCompatibleStateImageBehavior = false;
            listViewTileStatics.View = System.Windows.Forms.View.Details;
            listViewTileStatics.SelectedIndexChanged += OnTileStaticSelected;
            listViewTileStatics.DoubleClick += OnClickEditStatic;
            //
            // columnHeaderItem
            //
            columnHeaderItem.Text = "Item";
            columnHeaderItem.Width = 60;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 110;
            //
            // columnHeaderZ
            //
            columnHeaderZ.Text = "z";
            columnHeaderZ.Width = 35;
            //
            // columnHeaderHue
            //
            columnHeaderHue.Text = "Hue";
            columnHeaderHue.Width = 45;
            //
            // labelTileInfo
            //
            labelTileInfo.Dock = System.Windows.Forms.DockStyle.Top;
            labelTileInfo.Font = new System.Drawing.Font("Consolas", 8.5F);
            labelTileInfo.Name = "labelTileInfo";
            labelTileInfo.Padding = new System.Windows.Forms.Padding(4);
            labelTileInfo.Size = new System.Drawing.Size(276, 52);
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
            toolStripStatusLabel.Text = "";
            toolStripStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // EcRegionViewerForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            Controls.Add(toolStrip);
            Controls.Add(statusStrip);
            Name = "EcRegionViewerForm";
            ClientSize = new System.Drawing.Size(1000, 650);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Region Viewer (Enhanced Client)";
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            contextMenuStripPreview.ResumeLayout(false);
            panelTile.ResumeLayout(false);
            panelTile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxStatic).EndInit();
            flowLayoutPanelButtons.ResumeLayout(false);
            flowLayoutPanelButtons.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabelArt;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxArt;
        private System.Windows.Forms.ToolStripButton toolStripButtonLand;
        private System.Windows.Forms.ToolStripButton toolStripButtonLandArt;
        private System.Windows.Forms.ToolStripButton toolStripButtonStatics;
        private System.Windows.Forms.ToolStripButton toolStripButtonGrid;
        private System.Windows.Forms.ToolStripLabel toolStripLabelMaxZ;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxMaxZ;
        private System.Windows.Forms.ToolStripButton toolStripButtonRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownStatics;
        private System.Windows.Forms.ToolStripMenuItem importStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCsvToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportMultiToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorStatics;
        private System.Windows.Forms.ToolStripMenuItem clearStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorStatics2;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabelHint;
        private System.Windows.Forms.SplitContainer splitContainer;
        private UoFiddler.Plugin.Enhanced.UserControls.EcPreviewPanel previewPanel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPreview;
        private System.Windows.Forms.ToolStripMenuItem addStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeStaticToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorMenu;
        private System.Windows.Forms.ToolStripMenuItem copyIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyPositionToolStripMenuItem;
        private System.Windows.Forms.Panel panelTile;
        private System.Windows.Forms.PictureBox pictureBoxStatic;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.ListView listViewTileStatics;
        private System.Windows.Forms.ColumnHeader columnHeaderItem;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderZ;
        private System.Windows.Forms.ColumnHeader columnHeaderHue;
        private System.Windows.Forms.Label labelTileInfo;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}

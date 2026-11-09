/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareItemControl
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
            if (disposing)
            {
                CleanUp();
                if (components != null)
                {
                    components.Dispose();
                }
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
            listBoxOrg = new System.Windows.Forms.ListBox();
            contextMenuStripOrg = new System.Windows.Forms.ContextMenuStrip(components);
            replaceGraphicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editTileDataOrgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportOrgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportOrgBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportOrgTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportOrgPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorOrg = new System.Windows.Forms.ToolStripSeparator();
            removeOrgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            listBoxSec = new System.Windows.Forms.ListBox();
            contextMenuStripSec = new System.Windows.Forms.ContextMenuStrip(components);
            copyItem2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorSec = new System.Windows.Forms.ToolStripSeparator();
            extractAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportSecBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportSecTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportSecPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportImageVonBisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportRangeBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportRangeTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportRangePngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pictureBoxOrg = new System.Windows.Forms.PictureBox();
            pictureBoxSec = new System.Windows.Forms.PictureBox();
            labelOrgInfo = new System.Windows.Forms.Label();
            labelSecInfo = new System.Windows.Forms.Label();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tableLayoutPanelBottom = new System.Windows.Forms.TableLayoutPanel();
            flowLayoutPanelLoad = new System.Windows.Forms.FlowLayoutPanel();
            labelSecondDir = new System.Windows.Forms.Label();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            buttonLoadSecond = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            comboBoxSaveDir = new System.Windows.Forms.ComboBox();
            BtDataDirectoryisSettings = new System.Windows.Forms.Button();
            flowLayoutPanelFilter = new System.Windows.Forms.FlowLayoutPanel();
            labelFilter = new System.Windows.Forms.Label();
            comboBoxFilter = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            searchTextBox = new System.Windows.Forms.TextBox();
            comboBoxSearchMode = new System.Windows.Forms.ComboBox();
            OnClickSearch = new System.Windows.Forms.Button();
            progressBarScan = new System.Windows.Forms.ProgressBar();
            buttonCancelScan = new System.Windows.Forms.Button();
            labelStatus = new System.Windows.Forms.Label();
            flowLayoutPanelActions = new System.Windows.Forms.FlowLayoutPanel();
            btLeftMoveItem = new System.Windows.Forms.Button();
            btLeftMoveItemMore = new System.Windows.Forms.Button();
            btremoveitemfromindex = new System.Windows.Forms.Button();
            buttonCopyTo = new System.Windows.Forms.Button();
            buttonCopyAllDiff = new System.Windows.Forms.Button();
            checkBoxCopyTileData = new System.Windows.Forms.CheckBox();
            buttonEditTileData = new System.Windows.Forms.Button();
            buttonSaveArt = new System.Windows.Forms.Button();
            buttonSaveTileData = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            contextMenuStripOrg.SuspendLayout();
            contextMenuStripSec.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanelBottom.SuspendLayout();
            flowLayoutPanelLoad.SuspendLayout();
            flowLayoutPanelFilter.SuspendLayout();
            flowLayoutPanelActions.SuspendLayout();
            SuspendLayout();
            //
            // listBoxOrg
            //
            listBoxOrg.ContextMenuStrip = contextMenuStripOrg;
            listBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxOrg.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxOrg.FormattingEnabled = true;
            listBoxOrg.IntegralHeight = false;
            listBoxOrg.ItemHeight = 15;
            listBoxOrg.Location = new System.Drawing.Point(4, 3);
            listBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxOrg.Name = "listBoxOrg";
            listBoxOrg.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBoxOrg.Size = new System.Drawing.Size(220, 330);
            listBoxOrg.TabIndex = 0;
            listBoxOrg.DrawItem += DrawItemOrg;
            listBoxOrg.SelectedIndexChanged += OnIndexChangedOrg;
            listBoxOrg.KeyDown += ListBoxOrg_KeyDown;
            //
            // contextMenuStripOrg
            //
            contextMenuStripOrg.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { replaceGraphicToolStripMenuItem, editTileDataOrgToolStripMenuItem, exportOrgToolStripMenuItem, nextFreeSlotToolStripMenuItem, toolStripSeparatorOrg, removeOrgToolStripMenuItem });
            contextMenuStripOrg.Name = "contextMenuStripOrg";
            contextMenuStripOrg.Size = new System.Drawing.Size(230, 120);
            contextMenuStripOrg.Opening += ContextMenuStripOrg_Opening;
            //
            // replaceGraphicToolStripMenuItem
            //
            replaceGraphicToolStripMenuItem.Name = "replaceGraphicToolStripMenuItem";
            replaceGraphicToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            replaceGraphicToolStripMenuItem.Text = "Replace graphic with";
            replaceGraphicToolStripMenuItem.ToolTipText = "Replaces the selected original item with one of the items selected in the second list.";
            //
            // editTileDataOrgToolStripMenuItem
            //
            editTileDataOrgToolStripMenuItem.Name = "editTileDataOrgToolStripMenuItem";
            editTileDataOrgToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            editTileDataOrgToolStripMenuItem.Text = "Edit TileData...";
            editTileDataOrgToolStripMenuItem.Click += OnClickEditTileData;
            //
            // exportOrgToolStripMenuItem
            //
            exportOrgToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportOrgBmpToolStripMenuItem, exportOrgTiffToolStripMenuItem, exportOrgPngToolStripMenuItem });
            exportOrgToolStripMenuItem.Name = "exportOrgToolStripMenuItem";
            exportOrgToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            exportOrgToolStripMenuItem.Text = "Export selected";
            //
            // exportOrgBmpToolStripMenuItem
            //
            exportOrgBmpToolStripMenuItem.Name = "exportOrgBmpToolStripMenuItem";
            exportOrgBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportOrgBmpToolStripMenuItem.Tag = "bmp";
            exportOrgBmpToolStripMenuItem.Text = "As Bmp";
            exportOrgBmpToolStripMenuItem.Click += OnClickExportOrg;
            //
            // exportOrgTiffToolStripMenuItem
            //
            exportOrgTiffToolStripMenuItem.Name = "exportOrgTiffToolStripMenuItem";
            exportOrgTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportOrgTiffToolStripMenuItem.Tag = "tiff";
            exportOrgTiffToolStripMenuItem.Text = "As Tiff";
            exportOrgTiffToolStripMenuItem.Click += OnClickExportOrg;
            //
            // exportOrgPngToolStripMenuItem
            //
            exportOrgPngToolStripMenuItem.Name = "exportOrgPngToolStripMenuItem";
            exportOrgPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportOrgPngToolStripMenuItem.Tag = "png";
            exportOrgPngToolStripMenuItem.Text = "As Png";
            exportOrgPngToolStripMenuItem.Click += OnClickExportOrg;
            //
            // nextFreeSlotToolStripMenuItem
            //
            nextFreeSlotToolStripMenuItem.Name = "nextFreeSlotToolStripMenuItem";
            nextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            nextFreeSlotToolStripMenuItem.Text = "Go to next free slot";
            nextFreeSlotToolStripMenuItem.Click += OnClickNextFreeSlot;
            //
            // toolStripSeparatorOrg
            //
            toolStripSeparatorOrg.Name = "toolStripSeparatorOrg";
            toolStripSeparatorOrg.Size = new System.Drawing.Size(226, 6);
            //
            // removeOrgToolStripMenuItem
            //
            removeOrgToolStripMenuItem.Name = "removeOrgToolStripMenuItem";
            removeOrgToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            removeOrgToolStripMenuItem.Text = "Remove selected from original";
            removeOrgToolStripMenuItem.Click += Btremoveitemfromindex_Click;
            //
            // listBoxSec
            //
            listBoxSec.ContextMenuStrip = contextMenuStripSec;
            listBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxSec.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBoxSec.FormattingEnabled = true;
            listBoxSec.IntegralHeight = false;
            listBoxSec.ItemHeight = 15;
            listBoxSec.Location = new System.Drawing.Point(612, 3);
            listBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBoxSec.Name = "listBoxSec";
            listBoxSec.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            listBoxSec.Size = new System.Drawing.Size(221, 330);
            listBoxSec.TabIndex = 1;
            listBoxSec.DrawItem += DrawItemSec;
            listBoxSec.SelectedIndexChanged += OnIndexChangedSec;
            listBoxSec.KeyDown += ListBoxSec_KeyDown;
            //
            // contextMenuStripSec
            //
            contextMenuStripSec.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyItem2To1ToolStripMenuItem, copyToToolStripMenuItem, copyToFreeSlotsToolStripMenuItem, toolStripSeparatorSec, extractAsToolStripMenuItem, exportImageVonBisToolStripMenuItem });
            contextMenuStripSec.Name = "contextMenuStripSec";
            contextMenuStripSec.Size = new System.Drawing.Size(250, 120);
            //
            // copyItem2To1ToolStripMenuItem
            //
            copyItem2To1ToolStripMenuItem.Name = "copyItem2To1ToolStripMenuItem";
            copyItem2To1ToolStripMenuItem.ShortcutKeyDisplayString = "Left";
            copyItem2To1ToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            copyItem2To1ToolStripMenuItem.Text = "Copy to same position";
            copyItem2To1ToolStripMenuItem.Click += BtLeftMoveItemMore_Click;
            //
            // copyToToolStripMenuItem
            //
            copyToToolStripMenuItem.Name = "copyToToolStripMenuItem";
            copyToToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            copyToToolStripMenuItem.Text = "Copy to position...";
            copyToToolStripMenuItem.Click += OnClickCopyTo;
            //
            // copyToFreeSlotsToolStripMenuItem
            //
            copyToFreeSlotsToolStripMenuItem.Name = "copyToFreeSlotsToolStripMenuItem";
            copyToFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            copyToFreeSlotsToolStripMenuItem.Text = "Copy to free slots...";
            copyToFreeSlotsToolStripMenuItem.Click += OnClickCopyToFree;
            //
            // toolStripSeparatorSec
            //
            toolStripSeparatorSec.Name = "toolStripSeparatorSec";
            toolStripSeparatorSec.Size = new System.Drawing.Size(246, 6);
            //
            // extractAsToolStripMenuItem
            //
            extractAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportSecBmpToolStripMenuItem, exportSecTiffToolStripMenuItem, exportSecPngToolStripMenuItem });
            extractAsToolStripMenuItem.Name = "extractAsToolStripMenuItem";
            extractAsToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            extractAsToolStripMenuItem.Text = "Export selected";
            //
            // exportSecBmpToolStripMenuItem
            //
            exportSecBmpToolStripMenuItem.Name = "exportSecBmpToolStripMenuItem";
            exportSecBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportSecBmpToolStripMenuItem.Tag = "bmp";
            exportSecBmpToolStripMenuItem.Text = "As Bmp";
            exportSecBmpToolStripMenuItem.Click += OnClickExportSec;
            //
            // exportSecTiffToolStripMenuItem
            //
            exportSecTiffToolStripMenuItem.Name = "exportSecTiffToolStripMenuItem";
            exportSecTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportSecTiffToolStripMenuItem.Tag = "tiff";
            exportSecTiffToolStripMenuItem.Text = "As Tiff";
            exportSecTiffToolStripMenuItem.Click += OnClickExportSec;
            //
            // exportSecPngToolStripMenuItem
            //
            exportSecPngToolStripMenuItem.Name = "exportSecPngToolStripMenuItem";
            exportSecPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportSecPngToolStripMenuItem.Tag = "png";
            exportSecPngToolStripMenuItem.Text = "As Png";
            exportSecPngToolStripMenuItem.Click += OnClickExportSec;
            //
            // exportImageVonBisToolStripMenuItem
            //
            exportImageVonBisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportRangeBmpToolStripMenuItem, exportRangeTiffToolStripMenuItem, exportRangePngToolStripMenuItem });
            exportImageVonBisToolStripMenuItem.Name = "exportImageVonBisToolStripMenuItem";
            exportImageVonBisToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            exportImageVonBisToolStripMenuItem.Text = "Export range from to";
            exportImageVonBisToolStripMenuItem.ToolTipText = "Exports every item from the lowest to the highest selected id.";
            //
            // exportRangeBmpToolStripMenuItem
            //
            exportRangeBmpToolStripMenuItem.Name = "exportRangeBmpToolStripMenuItem";
            exportRangeBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportRangeBmpToolStripMenuItem.Tag = "bmp";
            exportRangeBmpToolStripMenuItem.Text = "As Bmp";
            exportRangeBmpToolStripMenuItem.Click += OnClickExportRange;
            //
            // exportRangeTiffToolStripMenuItem
            //
            exportRangeTiffToolStripMenuItem.Name = "exportRangeTiffToolStripMenuItem";
            exportRangeTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportRangeTiffToolStripMenuItem.Tag = "tiff";
            exportRangeTiffToolStripMenuItem.Text = "As Tiff";
            exportRangeTiffToolStripMenuItem.Click += OnClickExportRange;
            //
            // exportRangePngToolStripMenuItem
            //
            exportRangePngToolStripMenuItem.Name = "exportRangePngToolStripMenuItem";
            exportRangePngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            exportRangePngToolStripMenuItem.Tag = "png";
            exportRangePngToolStripMenuItem.Text = "As Png";
            exportRangePngToolStripMenuItem.Click += OnClickExportRange;
            //
            // labelOrgInfo
            //
            labelOrgInfo.AutoEllipsis = true;
            labelOrgInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            labelOrgInfo.Location = new System.Drawing.Point(4, 1);
            labelOrgInfo.Name = "labelOrgInfo";
            labelOrgInfo.Size = new System.Drawing.Size(364, 20);
            labelOrgInfo.TabIndex = 4;
            labelOrgInfo.Text = "Original";
            labelOrgInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pictureBoxOrg
            //
            pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxOrg.Location = new System.Drawing.Point(5, 25);
            pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxOrg.Name = "pictureBoxOrg";
            pictureBoxOrg.Size = new System.Drawing.Size(362, 140);
            pictureBoxOrg.TabIndex = 2;
            pictureBoxOrg.TabStop = false;
            pictureBoxOrg.SizeChanged += OnPreviewSizeChanged;
            //
            // labelSecInfo
            //
            labelSecInfo.AutoEllipsis = true;
            labelSecInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            labelSecInfo.Location = new System.Drawing.Point(4, 170);
            labelSecInfo.Name = "labelSecInfo";
            labelSecInfo.Size = new System.Drawing.Size(364, 20);
            labelSecInfo.TabIndex = 5;
            labelSecInfo.Text = "Second";
            labelSecInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // pictureBoxSec
            //
            pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxSec.Location = new System.Drawing.Point(5, 194);
            pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxSec.Name = "pictureBoxSec";
            pictureBoxSec.Size = new System.Drawing.Size(362, 140);
            pictureBoxSec.TabIndex = 3;
            pictureBoxSec.TabStop = false;
            pictureBoxSec.SizeChanged += OnPreviewSizeChanged;
            //
            // tableLayoutPanel1
            //
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(labelOrgInfo, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBoxOrg, 0, 1);
            tableLayoutPanel1.Controls.Add(labelSecInfo, 0, 2);
            tableLayoutPanel1.Controls.Add(pictureBoxSec, 0, 3);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(232, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(372, 330);
            tableLayoutPanel1.TabIndex = 7;
            //
            // tableLayoutPanel2
            //
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.45454F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.Controls.Add(listBoxOrg, 0, 0);
            tableLayoutPanel2.Controls.Add(listBoxSec, 2, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 1, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new System.Drawing.Size(940, 336);
            tableLayoutPanel2.TabIndex = 8;
            //
            // splitContainer1
            //
            splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer1.Panel1
            //
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel2);
            //
            // splitContainer1.Panel2
            //
            splitContainer1.Panel2.Controls.Add(tableLayoutPanelBottom);
            splitContainer1.Size = new System.Drawing.Size(940, 480);
            splitContainer1.SplitterDistance = 336;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 9;
            //
            // tableLayoutPanelBottom
            //
            tableLayoutPanelBottom.ColumnCount = 1;
            tableLayoutPanelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanelLoad, 0, 0);
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanelFilter, 0, 1);
            tableLayoutPanelBottom.Controls.Add(flowLayoutPanelActions, 0, 2);
            tableLayoutPanelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanelBottom.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanelBottom.Name = "tableLayoutPanelBottom";
            tableLayoutPanelBottom.RowCount = 3;
            tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableLayoutPanelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanelBottom.Size = new System.Drawing.Size(940, 139);
            tableLayoutPanelBottom.TabIndex = 0;
            //
            // flowLayoutPanelLoad
            //
            flowLayoutPanelLoad.AutoSize = true;
            flowLayoutPanelLoad.Controls.Add(labelSecondDir);
            flowLayoutPanelLoad.Controls.Add(textBoxSecondDir);
            flowLayoutPanelLoad.Controls.Add(buttonBrowse);
            flowLayoutPanelLoad.Controls.Add(buttonLoadSecond);
            flowLayoutPanelLoad.Controls.Add(label1);
            flowLayoutPanelLoad.Controls.Add(comboBoxSaveDir);
            flowLayoutPanelLoad.Controls.Add(BtDataDirectoryisSettings);
            flowLayoutPanelLoad.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelLoad.Location = new System.Drawing.Point(3, 3);
            flowLayoutPanelLoad.Name = "flowLayoutPanelLoad";
            flowLayoutPanelLoad.Size = new System.Drawing.Size(934, 31);
            flowLayoutPanelLoad.TabIndex = 0;
            //
            // labelSecondDir
            //
            labelSecondDir.AutoSize = true;
            labelSecondDir.Location = new System.Drawing.Point(3, 7);
            labelSecondDir.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelSecondDir.Name = "labelSecondDir";
            labelSecondDir.Size = new System.Drawing.Size(70, 15);
            labelSecondDir.TabIndex = 0;
            labelSecondDir.Text = "Second dir :";
            //
            // textBoxSecondDir
            //
            textBoxSecondDir.AllowDrop = true;
            textBoxSecondDir.Location = new System.Drawing.Point(79, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(300, 23);
            textBoxSecondDir.TabIndex = 1;
            toolTip1.SetToolTip(textBoxSecondDir, "Directory with art.mul + artidx.mul or artLegacyMUL.uop (optional tiledata.mul).\r\nPress Enter to load, or drop a folder here.");
            textBoxSecondDir.DragDrop += TextBoxSecondDir_DragDrop;
            textBoxSecondDir.DragEnter += TextBoxSecondDir_DragEnter;
            textBoxSecondDir.KeyDown += TextBoxSecondDir_KeyDown;
            //
            // buttonBrowse
            //
            buttonBrowse.Location = new System.Drawing.Point(385, 3);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(30, 23);
            buttonBrowse.TabIndex = 2;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += OnClickBrowse;
            //
            // buttonLoadSecond
            //
            buttonLoadSecond.AutoSize = true;
            buttonLoadSecond.Location = new System.Drawing.Point(421, 3);
            buttonLoadSecond.Name = "buttonLoadSecond";
            buttonLoadSecond.Size = new System.Drawing.Size(90, 25);
            buttonLoadSecond.TabIndex = 3;
            buttonLoadSecond.Text = "Load Second";
            buttonLoadSecond.UseVisualStyleBackColor = true;
            buttonLoadSecond.Click += OnClickLoadSecond;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(517, 7);
            label1.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 15);
            label1.TabIndex = 4;
            label1.Text = "Recent :";
            //
            // comboBoxSaveDir
            //
            comboBoxSaveDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSaveDir.FormattingEnabled = true;
            comboBoxSaveDir.Location = new System.Drawing.Point(573, 3);
            comboBoxSaveDir.Name = "comboBoxSaveDir";
            comboBoxSaveDir.Size = new System.Drawing.Size(280, 23);
            comboBoxSaveDir.TabIndex = 5;
            toolTip1.SetToolTip(comboBoxSaveDir, "Recently used directories. Selecting one loads it.");
            comboBoxSaveDir.SelectionChangeCommitted += ComboBoxSaveDir_SelectionChangeCommitted;
            //
            // BtDataDirectoryisSettings
            //
            BtDataDirectoryisSettings.Location = new System.Drawing.Point(859, 3);
            BtDataDirectoryisSettings.Name = "BtDataDirectoryisSettings";
            BtDataDirectoryisSettings.Size = new System.Drawing.Size(40, 23);
            BtDataDirectoryisSettings.TabIndex = 6;
            BtDataDirectoryisSettings.Text = "Dir";
            toolTip1.SetToolTip(BtDataDirectoryisSettings, "Opens the directory where the settings of this tab are stored.");
            BtDataDirectoryisSettings.UseVisualStyleBackColor = true;
            BtDataDirectoryisSettings.Click += BtDataDirectoryisSettings_Click;
            //
            // flowLayoutPanelFilter
            //
            flowLayoutPanelFilter.AutoSize = true;
            flowLayoutPanelFilter.Controls.Add(labelFilter);
            flowLayoutPanelFilter.Controls.Add(comboBoxFilter);
            flowLayoutPanelFilter.Controls.Add(label2);
            flowLayoutPanelFilter.Controls.Add(searchTextBox);
            flowLayoutPanelFilter.Controls.Add(comboBoxSearchMode);
            flowLayoutPanelFilter.Controls.Add(OnClickSearch);
            flowLayoutPanelFilter.Controls.Add(progressBarScan);
            flowLayoutPanelFilter.Controls.Add(buttonCancelScan);
            flowLayoutPanelFilter.Controls.Add(labelStatus);
            flowLayoutPanelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelFilter.Location = new System.Drawing.Point(3, 40);
            flowLayoutPanelFilter.Name = "flowLayoutPanelFilter";
            flowLayoutPanelFilter.Size = new System.Drawing.Size(934, 31);
            flowLayoutPanelFilter.TabIndex = 1;
            //
            // labelFilter
            //
            labelFilter.AutoSize = true;
            labelFilter.Location = new System.Drawing.Point(3, 7);
            labelFilter.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelFilter.Name = "labelFilter";
            labelFilter.Size = new System.Drawing.Size(42, 15);
            labelFilter.TabIndex = 0;
            labelFilter.Text = "Show :";
            //
            // comboBoxFilter
            //
            comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFilter.FormattingEnabled = true;
            comboBoxFilter.Items.AddRange(new object[] { "All", "All differences", "Changed (in both, different)", "Only in Original", "Only in Second (new)", "Free in Original" });
            comboBoxFilter.Location = new System.Drawing.Point(51, 3);
            comboBoxFilter.Name = "comboBoxFilter";
            comboBoxFilter.Size = new System.Drawing.Size(180, 23);
            comboBoxFilter.TabIndex = 1;
            comboBoxFilter.SelectedIndexChanged += OnFilterChanged;
            //
            // label2
            //
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(237, 7);
            label2.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(48, 15);
            label2.TabIndex = 2;
            label2.Text = "Search :";
            //
            // searchTextBox
            //
            searchTextBox.Location = new System.Drawing.Point(291, 3);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new System.Drawing.Size(80, 23);
            searchTextBox.TabIndex = 3;
            toolTip1.SetToolTip(searchTextBox, "Item id. \"0x\" prefix is always hex. Press Enter to search.");
            searchTextBox.KeyDown += SearchTextBox_KeyDown;
            //
            // comboBoxSearchMode
            //
            comboBoxSearchMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxSearchMode.FormattingEnabled = true;
            comboBoxSearchMode.Items.AddRange(new object[] { "Hex", "Dec" });
            comboBoxSearchMode.Location = new System.Drawing.Point(377, 3);
            comboBoxSearchMode.Name = "comboBoxSearchMode";
            comboBoxSearchMode.Size = new System.Drawing.Size(55, 23);
            comboBoxSearchMode.TabIndex = 4;
            //
            // OnClickSearch
            //
            OnClickSearch.Location = new System.Drawing.Point(438, 3);
            OnClickSearch.Name = "OnClickSearch";
            OnClickSearch.Size = new System.Drawing.Size(60, 23);
            OnClickSearch.TabIndex = 5;
            OnClickSearch.Text = "Search";
            OnClickSearch.UseVisualStyleBackColor = true;
            OnClickSearch.Click += OnClickSearch_Click;
            //
            // progressBarScan
            //
            progressBarScan.Location = new System.Drawing.Point(504, 5);
            progressBarScan.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            progressBarScan.Name = "progressBarScan";
            progressBarScan.Size = new System.Drawing.Size(140, 18);
            progressBarScan.TabIndex = 6;
            progressBarScan.Visible = false;
            //
            // buttonCancelScan
            //
            buttonCancelScan.AutoSize = true;
            buttonCancelScan.Location = new System.Drawing.Point(650, 3);
            buttonCancelScan.Name = "buttonCancelScan";
            buttonCancelScan.Size = new System.Drawing.Size(55, 25);
            buttonCancelScan.TabIndex = 7;
            buttonCancelScan.Text = "Cancel";
            buttonCancelScan.UseVisualStyleBackColor = true;
            buttonCancelScan.Visible = false;
            buttonCancelScan.Click += ButtonCancelScan_Click;
            //
            // labelStatus
            //
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(711, 7);
            labelStatus.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(126, 15);
            labelStatus.TabIndex = 8;
            labelStatus.Text = "Second file not loaded";
            //
            // flowLayoutPanelActions
            //
            flowLayoutPanelActions.Controls.Add(btLeftMoveItem);
            flowLayoutPanelActions.Controls.Add(btLeftMoveItemMore);
            flowLayoutPanelActions.Controls.Add(btremoveitemfromindex);
            flowLayoutPanelActions.Controls.Add(buttonCopyTo);
            flowLayoutPanelActions.Controls.Add(buttonCopyAllDiff);
            flowLayoutPanelActions.Controls.Add(checkBoxCopyTileData);
            flowLayoutPanelActions.Controls.Add(buttonEditTileData);
            flowLayoutPanelActions.Controls.Add(buttonSaveArt);
            flowLayoutPanelActions.Controls.Add(buttonSaveTileData);
            flowLayoutPanelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelActions.Location = new System.Drawing.Point(3, 77);
            flowLayoutPanelActions.Name = "flowLayoutPanelActions";
            flowLayoutPanelActions.Size = new System.Drawing.Size(934, 59);
            flowLayoutPanelActions.TabIndex = 2;
            //
            // btLeftMoveItem
            //
            btLeftMoveItem.Image = Properties.Resources.left;
            btLeftMoveItem.Location = new System.Drawing.Point(3, 3);
            btLeftMoveItem.Name = "btLeftMoveItem";
            btLeftMoveItem.Size = new System.Drawing.Size(52, 53);
            btLeftMoveItem.TabIndex = 0;
            toolTip1.SetToolTip(btLeftMoveItem, "Copy the focused second item to the same position in the original (key: Left).");
            btLeftMoveItem.UseVisualStyleBackColor = true;
            btLeftMoveItem.Click += BtLeftMoveItem_Click;
            //
            // btLeftMoveItemMore
            //
            btLeftMoveItemMore.Image = Properties.Resources.left2;
            btLeftMoveItemMore.Location = new System.Drawing.Point(61, 3);
            btLeftMoveItemMore.Name = "btLeftMoveItemMore";
            btLeftMoveItemMore.Size = new System.Drawing.Size(52, 53);
            btLeftMoveItemMore.TabIndex = 1;
            toolTip1.SetToolTip(btLeftMoveItemMore, "Copy all selected second items to the same positions in the original.");
            btLeftMoveItemMore.UseVisualStyleBackColor = true;
            btLeftMoveItemMore.Click += BtLeftMoveItemMore_Click;
            //
            // btremoveitemfromindex
            //
            btremoveitemfromindex.Image = Properties.Resources.right;
            btremoveitemfromindex.Location = new System.Drawing.Point(119, 3);
            btremoveitemfromindex.Name = "btremoveitemfromindex";
            btremoveitemfromindex.Size = new System.Drawing.Size(52, 53);
            btremoveitemfromindex.TabIndex = 2;
            toolTip1.SetToolTip(btremoveitemfromindex, "Remove the selected items from the original (with confirmation, key: Delete in the original list).");
            btremoveitemfromindex.UseVisualStyleBackColor = true;
            btremoveitemfromindex.Click += Btremoveitemfromindex_Click;
            //
            // buttonCopyTo
            //
            buttonCopyTo.Location = new System.Drawing.Point(177, 3);
            buttonCopyTo.Name = "buttonCopyTo";
            buttonCopyTo.Size = new System.Drawing.Size(110, 53);
            buttonCopyTo.TabIndex = 3;
            buttonCopyTo.Text = "Copy selected to...";
            toolTip1.SetToolTip(buttonCopyTo, "Copy the selected second items to the same position, a chosen position or free slots.");
            buttonCopyTo.UseVisualStyleBackColor = true;
            buttonCopyTo.Click += OnClickCopyTo;
            //
            // buttonCopyAllDiff
            //
            buttonCopyAllDiff.Location = new System.Drawing.Point(293, 3);
            buttonCopyAllDiff.Name = "buttonCopyAllDiff";
            buttonCopyAllDiff.Size = new System.Drawing.Size(120, 53);
            buttonCopyAllDiff.TabIndex = 4;
            buttonCopyAllDiff.Text = "Copy all differences...";
            toolTip1.SetToolTip(buttonCopyAllDiff, "Copy every new or changed item of the second file to the same position.");
            buttonCopyAllDiff.UseVisualStyleBackColor = true;
            buttonCopyAllDiff.Click += OnClickCopyAllDifferences;
            //
            // checkBoxCopyTileData
            //
            checkBoxCopyTileData.AutoSize = true;
            checkBoxCopyTileData.Location = new System.Drawing.Point(419, 20);
            checkBoxCopyTileData.Margin = new System.Windows.Forms.Padding(3, 20, 3, 3);
            checkBoxCopyTileData.Name = "checkBoxCopyTileData";
            checkBoxCopyTileData.Size = new System.Drawing.Size(102, 19);
            checkBoxCopyTileData.TabIndex = 5;
            checkBoxCopyTileData.Text = "Copy TileData";
            toolTip1.SetToolTip(checkBoxCopyTileData, "Also copy the tiledata entry (name, flags, height, ...) of the second client.");
            checkBoxCopyTileData.UseVisualStyleBackColor = true;
            //
            // buttonEditTileData
            //
            buttonEditTileData.Location = new System.Drawing.Point(527, 3);
            buttonEditTileData.Name = "buttonEditTileData";
            buttonEditTileData.Size = new System.Drawing.Size(100, 53);
            buttonEditTileData.TabIndex = 6;
            buttonEditTileData.Text = "Edit TileData...";
            toolTip1.SetToolTip(buttonEditTileData, "Edit the tiledata of the selected original items.");
            buttonEditTileData.UseVisualStyleBackColor = true;
            buttonEditTileData.Click += OnClickEditTileData;
            //
            // buttonSaveArt
            //
            buttonSaveArt.Location = new System.Drawing.Point(633, 3);
            buttonSaveArt.Name = "buttonSaveArt";
            buttonSaveArt.Size = new System.Drawing.Size(90, 53);
            buttonSaveArt.TabIndex = 7;
            buttonSaveArt.Text = "Save Art";
            toolTip1.SetToolTip(buttonSaveArt, "Save art.mul/artidx.mul of the original to the output directory.");
            buttonSaveArt.UseVisualStyleBackColor = true;
            buttonSaveArt.Click += OnClickSaveArt;
            //
            // buttonSaveTileData
            //
            buttonSaveTileData.Location = new System.Drawing.Point(729, 3);
            buttonSaveTileData.Name = "buttonSaveTileData";
            buttonSaveTileData.Size = new System.Drawing.Size(90, 53);
            buttonSaveTileData.TabIndex = 8;
            buttonSaveTileData.Text = "Save TileData";
            toolTip1.SetToolTip(buttonSaveTileData, "Save tiledata.mul of the original to the output directory.");
            buttonSaveTileData.UseVisualStyleBackColor = true;
            buttonSaveTileData.Click += OnClickSaveTileData;
            //
            // CompareItemControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareItemControl";
            Size = new System.Drawing.Size(940, 480);
            Load += OnLoad;
            contextMenuStripOrg.ResumeLayout(false);
            contextMenuStripSec.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanelBottom.ResumeLayout(false);
            tableLayoutPanelBottom.PerformLayout();
            flowLayoutPanelLoad.ResumeLayout(false);
            flowLayoutPanelLoad.PerformLayout();
            flowLayoutPanelFilter.ResumeLayout(false);
            flowLayoutPanelFilter.PerformLayout();
            flowLayoutPanelActions.ResumeLayout(false);
            flowLayoutPanelActions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListBox listBoxOrg;
        private System.Windows.Forms.ListBox listBoxSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.Label labelOrgInfo;
        private System.Windows.Forms.Label labelSecInfo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelBottom;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelLoad;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelFilter;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelActions;
        private System.Windows.Forms.Label labelSecondDir;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonLoadSecond;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxSaveDir;
        private System.Windows.Forms.Button BtDataDirectoryisSettings;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.ComboBox comboBoxSearchMode;
        private System.Windows.Forms.Button OnClickSearch;
        private System.Windows.Forms.ProgressBar progressBarScan;
        private System.Windows.Forms.Button buttonCancelScan;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btLeftMoveItem;
        private System.Windows.Forms.Button btLeftMoveItemMore;
        private System.Windows.Forms.Button btremoveitemfromindex;
        private System.Windows.Forms.Button buttonCopyTo;
        private System.Windows.Forms.Button buttonCopyAllDiff;
        private System.Windows.Forms.CheckBox checkBoxCopyTileData;
        private System.Windows.Forms.Button buttonEditTileData;
        private System.Windows.Forms.Button buttonSaveArt;
        private System.Windows.Forms.Button buttonSaveTileData;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripOrg;
        private System.Windows.Forms.ToolStripMenuItem replaceGraphicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTileDataOrgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportOrgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportOrgBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportOrgTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportOrgPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorOrg;
        private System.Windows.Forms.ToolStripMenuItem removeOrgToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSec;
        private System.Windows.Forms.ToolStripMenuItem copyItem2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorSec;
        private System.Windows.Forms.ToolStripMenuItem extractAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSecBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSecTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportSecPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportImageVonBisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRangeBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRangeTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportRangePngToolStripMenuItem;
    }
}

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

using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    partial class ItemsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemsControl));
            splitContainer2 = new SplitContainer();
            chkApplyColorChange = new CheckBox();
            DetailPictureBox = new PictureBox();
            DetailPictureBoxContextMenuStrip = new ContextMenuStrip(components);
            changeBackgroundColorToolStripMenuItemDetail = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            particleGraylToolStripMenuItem = new ToolStripMenuItem();
            particleGrayColorToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator10 = new ToolStripSeparator();
            grayscaleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            drawRhombusToolStripMenuItem = new ToolStripMenuItem();
            gridPictureToolStripMenuItem = new ToolStripMenuItem();
            SelectColorToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            copyClipboardToolStripMenuItem = new ToolStripMenuItem();
            DetailTextBox = new RichTextBox();
            splitContainer1 = new SplitContainer();
            ItemsTileView = new TileView.TileViewControl();
            TileViewContextMenuStrip = new ContextMenuStrip(components);
            showFreeSlotsToolStripMenuItem = new ToolStripMenuItem();
            findNextFreeSlotToolStripMenuItem = new ToolStripMenuItem();
            countItemsToolStripMenuItem = new ToolStripMenuItem();
            ChangeBackgroundColorToolStripMenuItem = new ToolStripMenuItem();
            colorsImageToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            selectInTileDataTabToolStripMenuItem = new ToolStripMenuItem();
            selectInRadarColorTabToolStripMenuItem = new ToolStripMenuItem();
            SelectIDToHexToolStripMenuItem = new ToolStripMenuItem();
            selectInGumpsTabMaleToolStripMenuItem = new ToolStripMenuItem();
            selectInGumpsTabFemaleToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            extractToolStripMenuItem = new ToolStripMenuItem();
            bmpToolStripMenuItem = new ToolStripMenuItem();
            tiffToolStripMenuItem = new ToolStripMenuItem();
            asJpgToolStripMenuItem1 = new ToolStripMenuItem();
            asPngToolStripMenuItem1 = new ToolStripMenuItem();
            SaveImageNameAndHexToTempToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator11 = new ToolStripSeparator();
            replaceToolStripMenuItem = new ToolStripMenuItem();
            replaceStartingFromToolStripMenuItem = new ToolStripMenuItem();
            ReplaceStartingFromText = new ToolStripTextBox();
            removeToolStripMenuItem = new ToolStripMenuItem();
            insertAtToolStripMenuItem = new ToolStripMenuItem();
            InsertText = new ToolStripTextBox();
            imageSwapToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            mirrorToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            copyToolStripMenuItem = new ToolStripMenuItem();
            importToolStripclipboardMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            markToolStripMenuItem = new ToolStripMenuItem();
            gotoMarkToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator9 = new ToolStripSeparator();
            saveToolStripMenuItem = new ToolStripMenuItem();
            StatusStrip = new StatusStrip();
            NameLabel = new ToolStripStatusLabel();
            GraphicLabel = new ToolStripStatusLabel();
            toolStripStatusLabelItemHowMuch = new ToolStripStatusLabel();
            PreLoader = new System.ComponentModel.BackgroundWorker();
            ToolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            searchByIdToolStripTextBox = new ToolStripTextBox();
            toolStripLabel2 = new ToolStripLabel();
            searchByNameToolStripTextBox = new ToolStripTextBox();
            searchByNameToolStripButton = new ToolStripButton();
            SearchToolStripButton = new ToolStripButton();
            ReverseSearchToolStripButton = new ToolStripButton();
            ProgressBar = new ToolStripProgressBar();
            PreloadItemsToolStripButton = new ToolStripButton();
            MiscToolStripDropDownButton = new ToolStripDropDownButton();
            ExportAllToolStripMenuItem = new ToolStripMenuItem();
            asBmpToolStripMenuItem = new ToolStripMenuItem();
            asTiffToolStripMenuItem = new ToolStripMenuItem();
            asJpgToolStripMenuItem = new ToolStripMenuItem();
            asPngToolStripMenuItem = new ToolStripMenuItem();
            toolStripButton1 = new ToolStripButton();
            toolStripButtonColorImage = new ToolStripButton();
            colorDialog = new ColorDialog();
            collapsibleSplitter1 = new CollapsibleSplitter();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DetailPictureBox).BeginInit();
            DetailPictureBoxContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            TileViewContextMenuStrip.SuspendLayout();
            StatusStrip.SuspendLayout();
            ToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(chkApplyColorChange);
            splitContainer2.Panel1.Controls.Add(DetailPictureBox);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(DetailTextBox);
            splitContainer2.Size = new System.Drawing.Size(338, 342);
            splitContainer2.SplitterDistance = 192;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // chkApplyColorChange
            // 
            chkApplyColorChange.AutoSize = true;
            chkApplyColorChange.Location = new System.Drawing.Point(3, 3);
            chkApplyColorChange.Name = "chkApplyColorChange";
            chkApplyColorChange.Size = new System.Drawing.Size(98, 19);
            chkApplyColorChange.TabIndex = 1;
            chkApplyColorChange.Text = "Particele Grey";
            chkApplyColorChange.UseVisualStyleBackColor = true;
            chkApplyColorChange.CheckedChanged += ChkApplyColorChange_CheckedChanged;
            // 
            // DetailPictureBox
            // 
            DetailPictureBox.ContextMenuStrip = DetailPictureBoxContextMenuStrip;
            DetailPictureBox.Dock = DockStyle.Fill;
            DetailPictureBox.Location = new System.Drawing.Point(0, 0);
            DetailPictureBox.Margin = new Padding(4, 3, 4, 3);
            DetailPictureBox.Name = "DetailPictureBox";
            DetailPictureBox.Size = new System.Drawing.Size(338, 192);
            DetailPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            DetailPictureBox.TabIndex = 0;
            DetailPictureBox.TabStop = false;
            // 
            // DetailPictureBoxContextMenuStrip
            // 
            DetailPictureBoxContextMenuStrip.Items.AddRange(new ToolStripItem[] { changeBackgroundColorToolStripMenuItemDetail, toolStripSeparator6, particleGraylToolStripMenuItem, particleGrayColorToolStripMenuItem, toolStripSeparator10, grayscaleToolStripMenuItem, toolStripSeparator7, drawRhombusToolStripMenuItem, gridPictureToolStripMenuItem, SelectColorToolStripMenuItem, toolStripSeparator8, copyClipboardToolStripMenuItem });
            DetailPictureBoxContextMenuStrip.Name = "contextMenuStrip2";
            DetailPictureBoxContextMenuStrip.Size = new System.Drawing.Size(213, 204);
            // 
            // changeBackgroundColorToolStripMenuItemDetail
            // 
            changeBackgroundColorToolStripMenuItemDetail.Image = Properties.Resources.colordialog_background;
            changeBackgroundColorToolStripMenuItemDetail.Name = "changeBackgroundColorToolStripMenuItemDetail";
            changeBackgroundColorToolStripMenuItemDetail.Size = new System.Drawing.Size(212, 22);
            changeBackgroundColorToolStripMenuItemDetail.Text = "Change background color";
            changeBackgroundColorToolStripMenuItemDetail.ToolTipText = "Color Dialog for Background Display";
            changeBackgroundColorToolStripMenuItemDetail.Click += ChangeBackgroundColorToolStripMenuItemDetail_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(209, 6);
            // 
            // particleGraylToolStripMenuItem
            // 
            particleGraylToolStripMenuItem.Image = Properties.Resources.particle_gray_hue;
            particleGraylToolStripMenuItem.Name = "particleGraylToolStripMenuItem";
            particleGraylToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            particleGraylToolStripMenuItem.Text = "Particle Grey";
            particleGraylToolStripMenuItem.ToolTipText = "Displays the colors that can be used for in-game coloring.";
            particleGraylToolStripMenuItem.Click += ParticleGraylToolStripMenuItem_Click;
            // 
            // particleGrayColorToolStripMenuItem
            // 
            particleGrayColorToolStripMenuItem.Image = Properties.Resources.colordialog;
            particleGrayColorToolStripMenuItem.Name = "particleGrayColorToolStripMenuItem";
            particleGrayColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            particleGrayColorToolStripMenuItem.Text = "Particle Grey Color";
            particleGrayColorToolStripMenuItem.ToolTipText = "Color Dialog for Particle Gray";
            particleGrayColorToolStripMenuItem.Click += ParticleGrayColorToolStripMenuItem_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(209, 6);
            // 
            // grayscaleToolStripMenuItem
            // 
            grayscaleToolStripMenuItem.Image = Properties.Resources.grayscale_image;
            grayscaleToolStripMenuItem.Name = "grayscaleToolStripMenuItem";
            grayscaleToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            grayscaleToolStripMenuItem.Text = "Grayscale";
            grayscaleToolStripMenuItem.ToolTipText = "Grayscale image";
            grayscaleToolStripMenuItem.Click += grayscaleToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(209, 6);
            // 
            // drawRhombusToolStripMenuItem
            // 
            drawRhombusToolStripMenuItem.Image = Properties.Resources.diamand_;
            drawRhombusToolStripMenuItem.Name = "drawRhombusToolStripMenuItem";
            drawRhombusToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            drawRhombusToolStripMenuItem.Text = "Draw Rhombus";
            drawRhombusToolStripMenuItem.ToolTipText = "Draws a diamond shape on the image.";
            drawRhombusToolStripMenuItem.Click += DrawRhombusToolStripMenuItem_Click;
            // 
            // gridPictureToolStripMenuItem
            // 
            gridPictureToolStripMenuItem.Image = Properties.Resources.draw_rhombus;
            gridPictureToolStripMenuItem.Name = "gridPictureToolStripMenuItem";
            gridPictureToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            gridPictureToolStripMenuItem.Text = "Grid Picture";
            gridPictureToolStripMenuItem.ToolTipText = "Create the grid picture.";
            gridPictureToolStripMenuItem.Click += GridPictureToolStripMenuItem_Click;
            // 
            // SelectColorToolStripMenuItem
            // 
            SelectColorToolStripMenuItem.Image = Properties.Resources.Color;
            SelectColorToolStripMenuItem.Name = "SelectColorToolStripMenuItem";
            SelectColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            SelectColorToolStripMenuItem.Text = "Grid Color";
            SelectColorToolStripMenuItem.ToolTipText = "Change the color of the grid.";
            SelectColorToolStripMenuItem.Click += SelectColorToolStripMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(209, 6);
            // 
            // copyClipboardToolStripMenuItem
            // 
            copyClipboardToolStripMenuItem.Image = Properties.Resources.Copy;
            copyClipboardToolStripMenuItem.Name = "copyClipboardToolStripMenuItem";
            copyClipboardToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            copyClipboardToolStripMenuItem.Text = "Copy Clipboard";
            copyClipboardToolStripMenuItem.ToolTipText = "Copy the image to the clipboard.";
            copyClipboardToolStripMenuItem.Click += CopyClipboardToolStripMenuItem_Click;
            // 
            // DetailTextBox
            // 
            DetailTextBox.Dock = DockStyle.Fill;
            DetailTextBox.Location = new System.Drawing.Point(0, 0);
            DetailTextBox.Margin = new Padding(4, 3, 4, 3);
            DetailTextBox.Name = "DetailTextBox";
            DetailTextBox.Size = new System.Drawing.Size(338, 145);
            DetailTextBox.TabIndex = 0;
            DetailTextBox.Text = "";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 36);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(ItemsTileView);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(1303, 342);
            splitContainer1.SplitterDistance = 960;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 6;
            // 
            // ItemsTileView
            // 
            ItemsTileView.AutoScroll = true;
            ItemsTileView.AutoScrollMinSize = new System.Drawing.Size(0, 102);
            ItemsTileView.BackColor = System.Drawing.SystemColors.Window;
            ItemsTileView.ContextMenuStrip = TileViewContextMenuStrip;
            ItemsTileView.Dock = DockStyle.Fill;
            ItemsTileView.FocusIndex = -1;
            ItemsTileView.Location = new System.Drawing.Point(0, 0);
            ItemsTileView.Margin = new Padding(4, 3, 4, 3);
            ItemsTileView.MultiSelect = true;
            ItemsTileView.Name = "ItemsTileView";
            ItemsTileView.Size = new System.Drawing.Size(960, 342);
            ItemsTileView.TabIndex = 0;
            ItemsTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            ItemsTileView.TileBorderColor = System.Drawing.Color.Gray;
            ItemsTileView.TileBorderWidth = 1F;
            ItemsTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            ItemsTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            ItemsTileView.TileMargin = new Padding(2, 2, 0, 0);
            ItemsTileView.TilePadding = new Padding(1);
            ItemsTileView.TileSize = new System.Drawing.Size(96, 96);
            ItemsTileView.VirtualListSize = 1;
            ItemsTileView.ItemSelectionChanged += ItemsTileView_ItemSelectionChanged;
            ItemsTileView.FocusSelectionChanged += ItemsTileView_FocusSelectionChanged;
            ItemsTileView.DrawItem += ItemsTileView_DrawItem;
            ItemsTileView.KeyDown += ItemsTileView_KeyDown;
            ItemsTileView.KeyUp += ItemsTileView_KeyUp;
            ItemsTileView.MouseDoubleClick += ItemsTileView_MouseDoubleClick;
            // 
            // TileViewContextMenuStrip
            // 
            TileViewContextMenuStrip.Items.AddRange(new ToolStripItem[] { showFreeSlotsToolStripMenuItem, findNextFreeSlotToolStripMenuItem, countItemsToolStripMenuItem, ChangeBackgroundColorToolStripMenuItem, colorsImageToolStripMenuItem, toolStripSeparator3, selectInTileDataTabToolStripMenuItem, selectInRadarColorTabToolStripMenuItem, SelectIDToHexToolStripMenuItem, selectInGumpsTabMaleToolStripMenuItem, selectInGumpsTabFemaleToolStripMenuItem, toolStripSeparator2, extractToolStripMenuItem, SaveImageNameAndHexToTempToolStripMenuItem, toolStripSeparator11, replaceToolStripMenuItem, replaceStartingFromToolStripMenuItem, removeToolStripMenuItem, insertAtToolStripMenuItem, imageSwapToolStripMenuItem, toolStripSeparator5, mirrorToolStripMenuItem, toolStripSeparator1, copyToolStripMenuItem, importToolStripclipboardMenuItem, toolStripSeparator4, markToolStripMenuItem, gotoMarkToolStripMenuItem, toolStripSeparator9, saveToolStripMenuItem });
            TileViewContextMenuStrip.Name = "contextMenuStrip1";
            TileViewContextMenuStrip.Size = new System.Drawing.Size(230, 552);
            TileViewContextMenuStrip.Closing += TileViewContextMenuStrip_Closing;
            TileViewContextMenuStrip.Opening += TileViewContextMenuStrip_Opening;
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            showFreeSlotsToolStripMenuItem.Image = Properties.Resources.show;
            showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            showFreeSlotsToolStripMenuItem.ToolTipText = "Displays all available ID slots.";
            showFreeSlotsToolStripMenuItem.Click += OnClickShowFreeSlots;
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            findNextFreeSlotToolStripMenuItem.Image = Properties.Resources.Search;
            findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            findNextFreeSlotToolStripMenuItem.ToolTipText = "Finds the next ID slot.";
            findNextFreeSlotToolStripMenuItem.Click += OnClickFindFree;
            // 
            // countItemsToolStripMenuItem
            // 
            countItemsToolStripMenuItem.Image = Properties.Resources.reset__2_;
            countItemsToolStripMenuItem.Name = "countItemsToolStripMenuItem";
            countItemsToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            countItemsToolStripMenuItem.Text = "Items Counter";
            countItemsToolStripMenuItem.Click += countItemsToolStripMenuItem_Click;
            // 
            // ChangeBackgroundColorToolStripMenuItem
            // 
            ChangeBackgroundColorToolStripMenuItem.Image = Properties.Resources.Color;
            ChangeBackgroundColorToolStripMenuItem.Name = "ChangeBackgroundColorToolStripMenuItem";
            ChangeBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            ChangeBackgroundColorToolStripMenuItem.Text = "Change background color";
            ChangeBackgroundColorToolStripMenuItem.ToolTipText = "Changes the background color.";
            ChangeBackgroundColorToolStripMenuItem.Click += ChangeBackgroundColorToolStripMenuItem_Click;
            // 
            // colorsImageToolStripMenuItem
            // 
            colorsImageToolStripMenuItem.Image = Properties.Resources.colordialog_background;
            colorsImageToolStripMenuItem.Name = "colorsImageToolStripMenuItem";
            colorsImageToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            colorsImageToolStripMenuItem.Text = "Colors Image";
            colorsImageToolStripMenuItem.ToolTipText = "Show Colors from Image";
            colorsImageToolStripMenuItem.Click += ColorsImageToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(226, 6);
            // 
            // selectInTileDataTabToolStripMenuItem
            // 
            selectInTileDataTabToolStripMenuItem.Image = Properties.Resources.Select;
            selectInTileDataTabToolStripMenuItem.Name = "selectInTileDataTabToolStripMenuItem";
            selectInTileDataTabToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            selectInTileDataTabToolStripMenuItem.Text = "Select in TileData tab";
            selectInTileDataTabToolStripMenuItem.ToolTipText = "Highlights the ID in the Tiledata tab.";
            selectInTileDataTabToolStripMenuItem.Click += OnClickSelectTiledata;
            // 
            // selectInRadarColorTabToolStripMenuItem
            // 
            selectInRadarColorTabToolStripMenuItem.Image = Properties.Resources.Select;
            selectInRadarColorTabToolStripMenuItem.Name = "selectInRadarColorTabToolStripMenuItem";
            selectInRadarColorTabToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            selectInRadarColorTabToolStripMenuItem.Text = "Select in RadarColor tab";
            selectInRadarColorTabToolStripMenuItem.ToolTipText = "Highlights the ID in the RadarColor tab.";
            selectInRadarColorTabToolStripMenuItem.Click += OnClickSelectRadarCol;
            // 
            // SelectIDToHexToolStripMenuItem
            // 
            SelectIDToHexToolStripMenuItem.Image = Properties.Resources.hexdecimal_adresse_to_clipbord;
            SelectIDToHexToolStripMenuItem.Name = "SelectIDToHexToolStripMenuItem";
            SelectIDToHexToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            SelectIDToHexToolStripMenuItem.Text = "Select Hex to Clipboard";
            SelectIDToHexToolStripMenuItem.ToolTipText = "Saved the hex address to the clipboard.";
            SelectIDToHexToolStripMenuItem.Click += SelectIDToHexToolStripMenuItem_Click;
            // 
            // selectInGumpsTabMaleToolStripMenuItem
            // 
            selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
            selectInGumpsTabMaleToolStripMenuItem.Image = Properties.Resources.gumps_men_fantasy;
            selectInGumpsTabMaleToolStripMenuItem.Name = "selectInGumpsTabMaleToolStripMenuItem";
            selectInGumpsTabMaleToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            selectInGumpsTabMaleToolStripMenuItem.Text = "Select in Gumps (M)";
            selectInGumpsTabMaleToolStripMenuItem.ToolTipText = "Highlights the ID in the Gumps tab.";
            selectInGumpsTabMaleToolStripMenuItem.Click += SelectInGumpsTabMaleToolStripMenuItem_Click;
            // 
            // selectInGumpsTabFemaleToolStripMenuItem
            // 
            selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
            selectInGumpsTabFemaleToolStripMenuItem.Image = Properties.Resources.gumps_woman_fantasy;
            selectInGumpsTabFemaleToolStripMenuItem.Name = "selectInGumpsTabFemaleToolStripMenuItem";
            selectInGumpsTabFemaleToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            selectInGumpsTabFemaleToolStripMenuItem.Text = "Select in Gumps (F)";
            selectInGumpsTabFemaleToolStripMenuItem.ToolTipText = "Highlights the ID in the Gumps tab.";
            selectInGumpsTabFemaleToolStripMenuItem.Click += SelectInGumpsTabFemaleToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(226, 6);
            // 
            // extractToolStripMenuItem
            // 
            extractToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { bmpToolStripMenuItem, tiffToolStripMenuItem, asJpgToolStripMenuItem1, asPngToolStripMenuItem1 });
            extractToolStripMenuItem.Image = Properties.Resources.Export;
            extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            extractToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            extractToolStripMenuItem.Text = "Export Image..";
            extractToolStripMenuItem.ToolTipText = "Export the image to.";
            // 
            // bmpToolStripMenuItem
            // 
            bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            bmpToolStripMenuItem.Text = "As Bmp";
            bmpToolStripMenuItem.Click += Extract_Image_ClickBmp;
            // 
            // tiffToolStripMenuItem
            // 
            tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            tiffToolStripMenuItem.Text = "As Tiff";
            tiffToolStripMenuItem.Click += Extract_Image_ClickTiff;
            // 
            // asJpgToolStripMenuItem1
            // 
            asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem1.Text = "As Jpg";
            asJpgToolStripMenuItem1.Click += Extract_Image_ClickJpg;
            // 
            // asPngToolStripMenuItem1
            // 
            asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            asPngToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem1.Text = "As Png";
            asPngToolStripMenuItem1.Click += Extract_Image_ClickPng;
            // 
            // SaveImageNameAndHexToTempToolStripMenuItem
            // 
            SaveImageNameAndHexToTempToolStripMenuItem.Image = Properties.Resources.Image;
            SaveImageNameAndHexToTempToolStripMenuItem.Name = "SaveImageNameAndHexToTempToolStripMenuItem";
            SaveImageNameAndHexToTempToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            SaveImageNameAndHexToTempToolStripMenuItem.Text = "Save Img Hex_Name to Temp";
            SaveImageNameAndHexToTempToolStripMenuItem.ToolTipText = "Save Graphic with Hex Address and Name to Temp";
            SaveImageNameAndHexToTempToolStripMenuItem.Click += SaveImageNameAndHexToTempToolStripMenuItem_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(226, 6);
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Image = Properties.Resources.replace;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            replaceToolStripMenuItem.Text = "Replace...";
            replaceToolStripMenuItem.ToolTipText = "Replace the image.";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ReplaceStartingFromText });
            replaceStartingFromToolStripMenuItem.Image = Properties.Resources.replace2;
            replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            replaceStartingFromToolStripMenuItem.ToolTipText = "Replace the image at position id.";
            // 
            // ReplaceStartingFromText
            // 
            ReplaceStartingFromText.Name = "ReplaceStartingFromText";
            ReplaceStartingFromText.Size = new System.Drawing.Size(100, 23);
            ReplaceStartingFromText.KeyDown += ReplaceStartingFromText_KeyDown;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Image = Properties.Resources.Remove;
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.ToolTipText = "Remove the image.";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // insertAtToolStripMenuItem
            // 
            insertAtToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { InsertText });
            insertAtToolStripMenuItem.Image = Properties.Resources.import;
            insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            insertAtToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            insertAtToolStripMenuItem.Text = "Insert At..";
            insertAtToolStripMenuItem.ToolTipText = "Inserts at position id.";
            // 
            // InsertText
            // 
            InsertText.Name = "InsertText";
            InsertText.Size = new System.Drawing.Size(100, 23);
            InsertText.KeyDown += OnKeyDownInsertText;
            InsertText.TextChanged += OnTextChangedInsert;
            // 
            // imageSwapToolStripMenuItem
            // 
            imageSwapToolStripMenuItem.Image = Properties.Resources.two_image_swap;
            imageSwapToolStripMenuItem.Name = "imageSwapToolStripMenuItem";
            imageSwapToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            imageSwapToolStripMenuItem.Text = "Image Swap";
            imageSwapToolStripMenuItem.ToolTipText = "Swap the graphics with each other";
            imageSwapToolStripMenuItem.Click += ImageSwapToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(226, 6);
            // 
            // mirrorToolStripMenuItem
            // 
            mirrorToolStripMenuItem.Image = Properties.Resources.Mirror;
            mirrorToolStripMenuItem.Name = "mirrorToolStripMenuItem";
            mirrorToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            mirrorToolStripMenuItem.Text = "Mirror";
            mirrorToolStripMenuItem.ToolTipText = "Mirror the image.";
            mirrorToolStripMenuItem.Click += MirrorToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(226, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = Properties.Resources.Copy;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.ToolTipText = "Copy the graphic to the clipboard.";
            copyToolStripMenuItem.Click += CopyToolStripMenuItem_Click;
            // 
            // importToolStripclipboardMenuItem
            // 
            importToolStripclipboardMenuItem.Image = Properties.Resources.import;
            importToolStripclipboardMenuItem.Name = "importToolStripclipboardMenuItem";
            importToolStripclipboardMenuItem.Size = new System.Drawing.Size(229, 22);
            importToolStripclipboardMenuItem.Text = "Import";
            importToolStripclipboardMenuItem.ToolTipText = "Import clipbord image";
            importToolStripclipboardMenuItem.Click += ImportToolStripclipboardMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(226, 6);
            // 
            // markToolStripMenuItem
            // 
            markToolStripMenuItem.Image = Properties.Resources.Mark;
            markToolStripMenuItem.Name = "markToolStripMenuItem";
            markToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            markToolStripMenuItem.Text = "Mark Position";
            markToolStripMenuItem.ToolTipText = "Mark Position";
            markToolStripMenuItem.Click += MarkToolStripMenuItem_Click;
            // 
            // gotoMarkToolStripMenuItem
            // 
            gotoMarkToolStripMenuItem.Image = Properties.Resources.zoom_image_into_tile;
            gotoMarkToolStripMenuItem.Name = "gotoMarkToolStripMenuItem";
            gotoMarkToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            gotoMarkToolStripMenuItem.Text = "Goto Position";
            gotoMarkToolStripMenuItem.ToolTipText = "Strg+J Jump to Last Mark Position";
            gotoMarkToolStripMenuItem.Click += GoToMarkedPositionToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(226, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.Save2;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(229, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Saves the .mul file.";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // StatusStrip
            // 
            StatusStrip.Items.AddRange(new ToolStripItem[] { NameLabel, GraphicLabel, toolStripStatusLabelItemHowMuch });
            StatusStrip.Location = new System.Drawing.Point(0, 378);
            StatusStrip.Name = "StatusStrip";
            StatusStrip.Padding = new Padding(1, 0, 16, 0);
            StatusStrip.Size = new System.Drawing.Size(1303, 22);
            StatusStrip.TabIndex = 5;
            StatusStrip.Text = "statusStrip1";
            // 
            // NameLabel
            // 
            NameLabel.AutoSize = false;
            NameLabel.BorderStyle = Border3DStyle.SunkenInner;
            NameLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            NameLabel.Name = "NameLabel";
            NameLabel.Size = new System.Drawing.Size(200, 17);
            NameLabel.Text = "Name:";
            NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GraphicLabel
            // 
            GraphicLabel.AutoSize = false;
            GraphicLabel.Name = "GraphicLabel";
            GraphicLabel.Size = new System.Drawing.Size(150, 17);
            GraphicLabel.Text = "Graphic:";
            GraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabelItemHowMuch
            // 
            toolStripStatusLabelItemHowMuch.Name = "toolStripStatusLabelItemHowMuch";
            toolStripStatusLabelItemHowMuch.Size = new System.Drawing.Size(34, 17);
            toolStripStatusLabelItemHowMuch.Text = "Item:";
            // 
            // PreLoader
            // 
            PreLoader.WorkerReportsProgress = true;
            PreLoader.DoWork += PreLoaderDoWork;
            PreLoader.ProgressChanged += PreLoaderProgressChanged;
            PreLoader.RunWorkerCompleted += PreLoaderCompleted;
            // 
            // ToolStrip
            // 
            ToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            ToolStrip.Items.AddRange(new ToolStripItem[] { toolStripLabel1, searchByIdToolStripTextBox, toolStripLabel2, searchByNameToolStripTextBox, searchByNameToolStripButton, SearchToolStripButton, ReverseSearchToolStripButton, ProgressBar, PreloadItemsToolStripButton, MiscToolStripDropDownButton, toolStripButtonColorImage, toolStripButton1 });
            ToolStrip.Location = new System.Drawing.Point(0, 0);
            ToolStrip.Name = "ToolStrip";
            ToolStrip.RenderMode = ToolStripRenderMode.System;
            ToolStrip.Size = new System.Drawing.Size(1303, 28);
            ToolStrip.TabIndex = 7;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(39, 25);
            toolStripLabel1.Text = "Index:";
            // 
            // searchByIdToolStripTextBox
            // 
            searchByIdToolStripTextBox.Name = "searchByIdToolStripTextBox";
            searchByIdToolStripTextBox.Size = new System.Drawing.Size(100, 28);
            searchByIdToolStripTextBox.ToolTipText = "Search by ID";
            searchByIdToolStripTextBox.KeyUp += SearchByIdToolStripTextBox_KeyUp;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(42, 25);
            toolStripLabel2.Text = "Name:";
            // 
            // searchByNameToolStripTextBox
            // 
            searchByNameToolStripTextBox.Name = "searchByNameToolStripTextBox";
            searchByNameToolStripTextBox.Size = new System.Drawing.Size(100, 28);
            searchByNameToolStripTextBox.ToolTipText = "Search by Name";
            searchByNameToolStripTextBox.KeyUp += SearchByNameToolStripTextBox_KeyUp;
            // 
            // searchByNameToolStripButton
            // 
            searchByNameToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            searchByNameToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            searchByNameToolStripButton.Name = "searchByNameToolStripButton";
            searchByNameToolStripButton.Size = new System.Drawing.Size(60, 25);
            searchByNameToolStripButton.Text = "Find next";
            searchByNameToolStripButton.Click += SearchByNameToolStripButton_Click;
            // 
            // SearchToolStripButton
            // 
            SearchToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            SearchToolStripButton.Image = Properties.Resources.Search;
            SearchToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SearchToolStripButton.Name = "SearchToolStripButton";
            SearchToolStripButton.Size = new System.Drawing.Size(23, 25);
            SearchToolStripButton.Text = "Search";
            SearchToolStripButton.ToolTipText = "Old Search";
            SearchToolStripButton.Click += OnSearchClick;
            // 
            // ReverseSearchToolStripButton
            // 
            ReverseSearchToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            ReverseSearchToolStripButton.Image = (System.Drawing.Image)resources.GetObject("ReverseSearchToolStripButton.Image");
            ReverseSearchToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ReverseSearchToolStripButton.Name = "ReverseSearchToolStripButton";
            ReverseSearchToolStripButton.Size = new System.Drawing.Size(94, 25);
            ReverseSearchToolStripButton.Text = "Previous Search";
            ReverseSearchToolStripButton.ToolTipText = "Previous Search";
            ReverseSearchToolStripButton.Click += ReverseSearchToolStripButton_Click;
            // 
            // ProgressBar
            // 
            ProgressBar.Alignment = ToolStripItemAlignment.Right;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(117, 25);
            // 
            // PreloadItemsToolStripButton
            // 
            PreloadItemsToolStripButton.Alignment = ToolStripItemAlignment.Right;
            PreloadItemsToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            PreloadItemsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            PreloadItemsToolStripButton.Name = "PreloadItemsToolStripButton";
            PreloadItemsToolStripButton.Size = new System.Drawing.Size(83, 25);
            PreloadItemsToolStripButton.Text = "Preload Items";
            PreloadItemsToolStripButton.Click += OnClickPreLoad;
            // 
            // MiscToolStripDropDownButton
            // 
            MiscToolStripDropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            MiscToolStripDropDownButton.DropDownItems.AddRange(new ToolStripItem[] { ExportAllToolStripMenuItem });
            MiscToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            MiscToolStripDropDownButton.Name = "MiscToolStripDropDownButton";
            MiscToolStripDropDownButton.Size = new System.Drawing.Size(45, 25);
            MiscToolStripDropDownButton.Text = "Misc";
            // 
            // ExportAllToolStripMenuItem
            // 
            ExportAllToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            ExportAllToolStripMenuItem.Name = "ExportAllToolStripMenuItem";
            ExportAllToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            ExportAllToolStripMenuItem.Text = "Export all..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += OnClick_SaveAllBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += OnClick_SaveAllTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClick_SaveAllJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += OnClick_SaveAllPng;
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = Properties.Resources.Save2;
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(23, 25);
            toolStripButton1.Text = "toolStripButtonSave";
            toolStripButton1.ToolTipText = "Save Items.mul file";
            toolStripButton1.Click += OnClickSave;
            // 
            // toolStripButtonColorImage
            // 
            toolStripButtonColorImage.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonColorImage.Image = Properties.Resources.colordialog;
            toolStripButtonColorImage.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonColorImage.Name = "toolStripButtonColorImage";
            toolStripButtonColorImage.Size = new System.Drawing.Size(23, 25);
            toolStripButtonColorImage.Text = "toolStripButton2";
            toolStripButtonColorImage.ToolTipText = "Reads the colors from the item image, displays them in a PictureBox and in a RichTextBox.";
            toolStripButtonColorImage.Click += toolStripButtonColorImage_Click;
            // 
            // collapsibleSplitter1
            // 
            collapsibleSplitter1.AnimationDelay = 20;
            collapsibleSplitter1.AnimationStep = 20;
            collapsibleSplitter1.BorderStyle3D = Border3DStyle.Flat;
            collapsibleSplitter1.ControlToHide = ToolStrip;
            collapsibleSplitter1.Dock = DockStyle.Top;
            collapsibleSplitter1.ExpandParentForm = false;
            collapsibleSplitter1.Location = new System.Drawing.Point(0, 28);
            collapsibleSplitter1.Margin = new Padding(4, 3, 4, 3);
            collapsibleSplitter1.Name = "collapsibleSplitter1";
            collapsibleSplitter1.Size = new System.Drawing.Size(1303, 8);
            collapsibleSplitter1.TabIndex = 8;
            collapsibleSplitter1.TabStop = false;
            collapsibleSplitter1.UseAnimations = false;
            collapsibleSplitter1.VisualStyle = VisualStyles.DoubleDots;
            // 
            // ItemsControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Controls.Add(StatusStrip);
            Controls.Add(collapsibleSplitter1);
            Controls.Add(ToolStrip);
            DoubleBuffered = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "ItemsControl";
            Size = new System.Drawing.Size(1303, 400);
            Load += OnLoad;
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DetailPictureBox).EndInit();
            DetailPictureBoxContextMenuStrip.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            TileViewContextMenuStrip.ResumeLayout(false);
            StatusStrip.ResumeLayout(false);
            StatusStrip.PerformLayout();
            ToolStrip.ResumeLayout(false);
            ToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStripMenuItem asBmpToolStripMenuItem;
        private ToolStripMenuItem asJpgToolStripMenuItem;
        private ToolStripMenuItem asJpgToolStripMenuItem1;
        private ToolStripMenuItem asPngToolStripMenuItem;
        private ToolStripMenuItem asPngToolStripMenuItem1;
        private ToolStripMenuItem asTiffToolStripMenuItem;
        private ToolStripMenuItem bmpToolStripMenuItem;
        private UoFiddler.Controls.UserControls.CollapsibleSplitter collapsibleSplitter1;
        private ContextMenuStrip TileViewContextMenuStrip;
        private PictureBox DetailPictureBox;
        private ToolStripMenuItem ExportAllToolStripMenuItem;
        private ToolStripMenuItem extractToolStripMenuItem;
        private ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private ToolStripStatusLabel GraphicLabel;
        private ToolStripMenuItem insertAtToolStripMenuItem;
        private ToolStripTextBox InsertText;
        private ToolStripStatusLabel NameLabel;
        private System.ComponentModel.BackgroundWorker PreLoader;
        private ToolStripProgressBar ProgressBar;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripMenuItem replaceToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem selectInRadarColorTabToolStripMenuItem;
        private ToolStripMenuItem selectInTileDataTabToolStripMenuItem;
        private ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private StatusStrip StatusStrip;
        private ToolStripMenuItem tiffToolStripMenuItem;
        private ToolStrip ToolStrip;
        private ToolStripButton SearchToolStripButton;
        private ToolStripButton PreloadItemsToolStripButton;
        private ToolStripDropDownButton MiscToolStripDropDownButton;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ContextMenuStrip DetailPictureBoxContextMenuStrip;
        private ToolStripMenuItem changeBackgroundColorToolStripMenuItemDetail;
        private ColorDialog colorDialog;
        private ToolStripMenuItem ChangeBackgroundColorToolStripMenuItem;
        private TileView.TileViewControl ItemsTileView;
        private RichTextBox DetailTextBox;
        private ToolStripMenuItem selectInGumpsTabMaleToolStripMenuItem;
        private ToolStripMenuItem selectInGumpsTabFemaleToolStripMenuItem;
        private ToolStripMenuItem replaceStartingFromToolStripMenuItem;
        private ToolStripTextBox ReplaceStartingFromText;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem importToolStripclipboardMenuItem;
        private ToolStripMenuItem mirrorToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox searchByIdToolStripTextBox;
        private ToolStripLabel toolStripLabel2;
        private ToolStripTextBox searchByNameToolStripTextBox;
        private ToolStripButton searchByNameToolStripButton;
        private ToolStripButton toolStripButton1;
        private ToolStripMenuItem SelectIDToHexToolStripMenuItem;
        private ToolStripMenuItem imageSwapToolStripMenuItem;
        private ToolStripButton ReverseSearchToolStripButton;
        private ToolStripMenuItem particleGraylToolStripMenuItem;
        private CheckBox chkApplyColorChange;
        private ToolStripMenuItem particleGrayColorToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem drawRhombusToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem gridPictureToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem copyClipboardToolStripMenuItem;
        private ToolStripMenuItem SelectColorToolStripMenuItem;
        private ToolStripMenuItem colorsImageToolStripMenuItem;
        private ToolStripMenuItem markToolStripMenuItem;
        private ToolStripMenuItem gotoMarkToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripMenuItem grayscaleToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem SaveImageNameAndHexToTempToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripStatusLabel toolStripStatusLabelItemHowMuch;
        private ToolStripMenuItem countItemsToolStripMenuItem;
        private ToolStripButton toolStripButtonColorImage;
    }
}

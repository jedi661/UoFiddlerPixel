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

namespace UoFiddler.Controls.UserControls
{
    partial class TexturesControl
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
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceStartingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ReplaceStartingFromTb = new System.Windows.Forms.ToolStripTextBox();
            insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            InsertText = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importFromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importByTempToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            um90GradToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            colorsTexturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            copyHexAdressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyDecAdressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip = new System.Windows.Forms.ToolStrip();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            SaveButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            IndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            searchByIdToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            SearchButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            MiscToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsBmp = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsTiff = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsJpeg = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsPng = new System.Windows.Forms.ToolStripMenuItem();
            PlaySoundtoolStripButton1 = new System.Windows.Forms.ToolStripButton();
            toolStripButtonTextureWindows = new System.Windows.Forms.ToolStripButton();
            toolStripButtonPictureFusion = new System.Windows.Forms.ToolStripButton();
            toolStripColor = new System.Windows.Forms.ToolStripButton();
            TextureTileView = new TileView.TileViewControl();
            panel1 = new System.Windows.Forms.Panel();
            statusStrip = new System.Windows.Forms.StatusStrip();
            GraphicLabel = new System.Windows.Forms.ToolStripStatusLabel();
            contextMenuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            panel1.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { showFreeSlotsToolStripMenuItem, toolStripSeparator5, exportImageToolStripMenuItem, toolStripSeparator2, findNextFreeSlotToolStripMenuItem, removeToolStripMenuItem, replaceToolStripMenuItem, replaceStartingFromToolStripMenuItem, insertAtToolStripMenuItem, toolStripSeparator6, copyToolStripMenuItem, importFromClipboardToolStripMenuItem, importByTempToolStripMenuItem, toolStripSeparator7, um90GradToolStripMenuItem, colorsTexturesToolStripMenuItem, toolStripSeparator9, copyHexAdressToolStripMenuItem, copyDecAdressToolStripMenuItem, toolStripSeparator10, saveToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip1";
            contextMenuStrip.Size = new System.Drawing.Size(194, 370);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            showFreeSlotsToolStripMenuItem.Image = Properties.Resources.Search;
            showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            showFreeSlotsToolStripMenuItem.Click += ShowFreeSlotsToolStripMenuItem_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(190, 6);
            // 
            // exportImageToolStripMenuItem
            // 
            exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            exportImageToolStripMenuItem.Image = Properties.Resources.Export;
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += OnClickExportBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += OnClickExportTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClickExportJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += OnClickExportPng;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            findNextFreeSlotToolStripMenuItem.Image = Properties.Resources.Search;
            findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            findNextFreeSlotToolStripMenuItem.Click += OnClickFindNext;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Image = Properties.Resources.Remove;
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Image = Properties.Resources.replace;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            replaceToolStripMenuItem.Text = "Replace";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ReplaceStartingFromTb });
            replaceStartingFromToolStripMenuItem.Image = Properties.Resources.replace2;
            replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            // 
            // ReplaceStartingFromTb
            // 
            ReplaceStartingFromTb.Name = "ReplaceStartingFromTb";
            ReplaceStartingFromTb.Size = new System.Drawing.Size(100, 23);
            ReplaceStartingFromTb.KeyDown += ReplaceStartingFrom_OnInsert;
            // 
            // insertAtToolStripMenuItem
            // 
            insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { InsertText });
            insertAtToolStripMenuItem.Image = Properties.Resources.import;
            insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            insertAtToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            InsertText.Name = "InsertText";
            InsertText.Size = new System.Drawing.Size(100, 23);
            InsertText.KeyDown += OnKeyDownInsert;
            InsertText.TextChanged += OnTextChangedInsert;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(190, 6);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = Properties.Resources.Copy;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.ToolTipText = "Copy the graphic to the clipboard.";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // importFromClipboardToolStripMenuItem
            // 
            importFromClipboardToolStripMenuItem.Image = Properties.Resources.import;
            importFromClipboardToolStripMenuItem.Name = "importFromClipboardToolStripMenuItem";
            importFromClipboardToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            importFromClipboardToolStripMenuItem.Text = "Import";
            importFromClipboardToolStripMenuItem.ToolTipText = "Import from clipboard";
            importFromClipboardToolStripMenuItem.Click += importFromClipboardToolStripMenuItem_Click;
            // 
            // importByTempToolStripMenuItem
            // 
            importByTempToolStripMenuItem.Image = Properties.Resources.import;
            importByTempToolStripMenuItem.Name = "importByTempToolStripMenuItem";
            importByTempToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            importByTempToolStripMenuItem.Text = "Import by Temp";
            importByTempToolStripMenuItem.ToolTipText = "Imports the graphic via Temp directory";
            importByTempToolStripMenuItem.Click += importByTempToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(190, 6);
            // 
            // um90GradToolStripMenuItem
            // 
            um90GradToolStripMenuItem.Image = Properties.Resources.Rotate;
            um90GradToolStripMenuItem.Name = "um90GradToolStripMenuItem";
            um90GradToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            um90GradToolStripMenuItem.Text = "Rotate by 90 degrees.";
            um90GradToolStripMenuItem.ToolTipText = "Rotate by 90 degrees.";
            um90GradToolStripMenuItem.Click += um90GradToolStripMenuItem_Click;
            // 
            // colorsTexturesToolStripMenuItem
            // 
            colorsTexturesToolStripMenuItem.Image = Properties.Resources.colordialog_background;
            colorsTexturesToolStripMenuItem.Name = "colorsTexturesToolStripMenuItem";
            colorsTexturesToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            colorsTexturesToolStripMenuItem.Text = "colors Textures";
            colorsTexturesToolStripMenuItem.ToolTipText = "Colors from Texture";
            colorsTexturesToolStripMenuItem.Click += colorsTexturesToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(190, 6);
            // 
            // copyHexAdressToolStripMenuItem
            // 
            copyHexAdressToolStripMenuItem.Image = Properties.Resources.Clipbord;
            copyHexAdressToolStripMenuItem.Name = "copyHexAdressToolStripMenuItem";
            copyHexAdressToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            copyHexAdressToolStripMenuItem.Text = "Copy Hex Adress";
            copyHexAdressToolStripMenuItem.Click += copyHexAdressToolStripMenuItem_Click;
            // 
            // copyDecAdressToolStripMenuItem
            // 
            copyDecAdressToolStripMenuItem.Image = Properties.Resources.Clipbord;
            copyDecAdressToolStripMenuItem.Name = "copyDecAdressToolStripMenuItem";
            copyDecAdressToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            copyDecAdressToolStripMenuItem.Text = "Copy Dec Adress";
            copyDecAdressToolStripMenuItem.Click += copyDecAdressToolStripMenuItem_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(190, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.Save2;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Save Texture.Mul";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripSeparator3, toolStripSeparator4, SaveButton, toolStripSeparator1, IndexToolStripLabel, searchByIdToolStripTextBox, SearchButton, toolStripSeparator8, MiscToolStripDropDownButton, PlaySoundtoolStripButton1, toolStripButtonTextureWindows, toolStripButtonPictureFusion, toolStripColor });
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip.Size = new System.Drawing.Size(733, 25);
            toolStrip.TabIndex = 4;
            toolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // SaveButton
            // 
            SaveButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            SaveButton.Image = Properties.Resources.Save2;
            SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(23, 22);
            SaveButton.Text = "Save";
            SaveButton.ToolTipText = "Save Texture.mul";
            SaveButton.Click += OnClickSave;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // IndexToolStripLabel
            // 
            IndexToolStripLabel.Name = "IndexToolStripLabel";
            IndexToolStripLabel.Size = new System.Drawing.Size(39, 22);
            IndexToolStripLabel.Text = "Index:";
            // 
            // searchByIdToolStripTextBox
            // 
            searchByIdToolStripTextBox.Name = "searchByIdToolStripTextBox";
            searchByIdToolStripTextBox.Size = new System.Drawing.Size(100, 25);
            searchByIdToolStripTextBox.ToolTipText = "Search";
            searchByIdToolStripTextBox.KeyUp += SearchByIdToolStripTextBox_KeyUp;
            // 
            // SearchButton
            // 
            SearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            SearchButton.Image = Properties.Resources.Search;
            SearchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SearchButton.Name = "SearchButton";
            SearchButton.Size = new System.Drawing.Size(23, 22);
            SearchButton.Text = "Search";
            SearchButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            SearchButton.ToolTipText = "Old Search";
            SearchButton.Click += OnClickSearch;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // MiscToolStripDropDownButton
            // 
            MiscToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            MiscToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportAllToolStripMenuItem });
            MiscToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            MiscToolStripDropDownButton.Margin = new System.Windows.Forms.Padding(0, 1, 20, 2);
            MiscToolStripDropDownButton.Name = "MiscToolStripDropDownButton";
            MiscToolStripDropDownButton.Size = new System.Drawing.Size(45, 22);
            MiscToolStripDropDownButton.Text = "Misc";
            // 
            // exportAllToolStripMenuItem
            // 
            exportAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ExportAllAsBmp, ExportAllAsTiff, ExportAllAsJpeg, ExportAllAsPng });
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            exportAllToolStripMenuItem.Text = "Export All..";
            // 
            // ExportAllAsBmp
            // 
            ExportAllAsBmp.Name = "ExportAllAsBmp";
            ExportAllAsBmp.Size = new System.Drawing.Size(115, 22);
            ExportAllAsBmp.Text = "As Bmp";
            ExportAllAsBmp.Click += ExportAllAsBmp_Click;
            // 
            // ExportAllAsTiff
            // 
            ExportAllAsTiff.Name = "ExportAllAsTiff";
            ExportAllAsTiff.Size = new System.Drawing.Size(115, 22);
            ExportAllAsTiff.Text = "As Tiff";
            ExportAllAsTiff.Click += ExportAllAsTiff_Click;
            // 
            // ExportAllAsJpeg
            // 
            ExportAllAsJpeg.Name = "ExportAllAsJpeg";
            ExportAllAsJpeg.Size = new System.Drawing.Size(115, 22);
            ExportAllAsJpeg.Text = "As Jpg";
            ExportAllAsJpeg.Click += ExportAllAsJpeg_Click;
            // 
            // ExportAllAsPng
            // 
            ExportAllAsPng.Name = "ExportAllAsPng";
            ExportAllAsPng.Size = new System.Drawing.Size(115, 22);
            ExportAllAsPng.Text = "As Png";
            ExportAllAsPng.Click += ExportAllAsPng_Click;
            // 
            // PlaySoundtoolStripButton1
            // 
            PlaySoundtoolStripButton1.CheckOnClick = true;
            PlaySoundtoolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            PlaySoundtoolStripButton1.Image = Properties.Resources.volume;
            PlaySoundtoolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            PlaySoundtoolStripButton1.Name = "PlaySoundtoolStripButton1";
            PlaySoundtoolStripButton1.Size = new System.Drawing.Size(23, 22);
            PlaySoundtoolStripButton1.Text = "Sound";
            PlaySoundtoolStripButton1.ToolTipText = "Deactivate the MessageBox and enable only one sound.";
            PlaySoundtoolStripButton1.Click += PlaySoundtoolStripButton1_Click;
            // 
            // toolStripButtonTextureWindows
            // 
            toolStripButtonTextureWindows.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonTextureWindows.Image = Properties.Resources.texture_form_tiles;
            toolStripButtonTextureWindows.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonTextureWindows.Name = "toolStripButtonTextureWindows";
            toolStripButtonTextureWindows.Size = new System.Drawing.Size(23, 22);
            toolStripButtonTextureWindows.Text = "Texture Windows";
            toolStripButtonTextureWindows.ToolTipText = "Texture form for editing textures and tiles.";
            toolStripButtonTextureWindows.Click += toolStripButtonTextureWindows_Click;
            // 
            // toolStripButtonPictureFusion
            // 
            toolStripButtonPictureFusion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonPictureFusion.Image = Properties.Resources.Image;
            toolStripButtonPictureFusion.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonPictureFusion.Name = "toolStripButtonPictureFusion";
            toolStripButtonPictureFusion.Size = new System.Drawing.Size(23, 22);
            toolStripButtonPictureFusion.Text = "PictureFusion";
            toolStripButtonPictureFusion.ToolTipText = "make one picture out of two pictures";
            toolStripButtonPictureFusion.Click += toolStripButtonPictureFusion_Click;
            // 
            // toolStripColor
            // 
            toolStripColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripColor.Image = Properties.Resources.colordialog;
            toolStripColor.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripColor.Name = "toolStripColor";
            toolStripColor.Size = new System.Drawing.Size(23, 22);
            toolStripColor.Text = "Texture Painter and Color and Pattern ";
            toolStripColor.Click += toolStripColor_Click;
            // 
            // TextureTileView
            // 
            TextureTileView.AutoScroll = true;
            TextureTileView.AutoScrollMinSize = new System.Drawing.Size(0, 134);
            TextureTileView.ContextMenuStrip = contextMenuStrip;
            TextureTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            TextureTileView.FocusIndex = -1;
            TextureTileView.Location = new System.Drawing.Point(0, 0);
            TextureTileView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TextureTileView.MultiSelect = false;
            TextureTileView.Name = "TextureTileView";
            TextureTileView.Size = new System.Drawing.Size(733, 356);
            TextureTileView.TabIndex = 5;
            TextureTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            TextureTileView.TileBorderColor = System.Drawing.Color.Gray;
            TextureTileView.TileBorderWidth = 1F;
            TextureTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            TextureTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            TextureTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            TextureTileView.TilePadding = new System.Windows.Forms.Padding(1);
            TextureTileView.TileSize = new System.Drawing.Size(128, 128);
            TextureTileView.VirtualListSize = 1;
            TextureTileView.ItemSelectionChanged += TextureTileView_ItemSelectionChanged;
            TextureTileView.DrawItem += TextureTileView_DrawItem;
            TextureTileView.KeyDown += TexturesControl_KeyDown;
            // 
            // panel1
            // 
            panel1.Controls.Add(statusStrip);
            panel1.Controls.Add(TextureTileView);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 25);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(733, 356);
            panel1.TabIndex = 1;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { GraphicLabel });
            statusStrip.Location = new System.Drawing.Point(0, 334);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(733, 22);
            statusStrip.TabIndex = 6;
            statusStrip.Text = "statusStrip1";
            // 
            // GraphicLabel
            // 
            GraphicLabel.Name = "GraphicLabel";
            GraphicLabel.Size = new System.Drawing.Size(51, 17);
            GraphicLabel.Text = "Graphic:";
            // 
            // TexturesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel1);
            Controls.Add(toolStrip);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TexturesControl";
            Size = new System.Drawing.Size(733, 381);
            Load += OnLoad;
            contextMenuStrip.ResumeLayout(false);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStripButton SearchButton;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private TileView.TileViewControl TextureTileView;
        private System.Windows.Forms.ToolStripDropDownButton MiscToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsBmp;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsTiff;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsJpeg;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsPng;
        private System.Windows.Forms.ToolStripMenuItem replaceStartingFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ReplaceStartingFromTb;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importByTempToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem um90GradToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel IndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox searchByIdToolStripTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel GraphicLabel;
        private System.Windows.Forms.ToolStripMenuItem copyHexAdressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyDecAdressToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripButton PlaySoundtoolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButtonTextureWindows;
        private System.Windows.Forms.ToolStripButton toolStripButtonPictureFusion;
        private System.Windows.Forms.ToolStripMenuItem colorsTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripColor;
    }
}

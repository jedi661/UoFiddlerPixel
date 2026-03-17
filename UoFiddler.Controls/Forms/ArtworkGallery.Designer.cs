// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class ArtworkGallery
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArtworkGallery));
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            panelLeft = new System.Windows.Forms.Panel();
            groupBoxControl = new System.Windows.Forms.GroupBox();
            ButtonRuler = new System.Windows.Forms.Button();
            ButtonResetOffset = new System.Windows.Forms.Button();
            ButtonRemoveBackground = new System.Windows.Forms.Button();
            ButtonLeft = new System.Windows.Forms.Button();
            ButtonRight = new System.Windows.Forms.Button();
            checkBoxSecondArtwork = new System.Windows.Forms.CheckBox();
            ButtonDrawRhombus = new System.Windows.Forms.Button();
            ButtonRemoveGif = new System.Windows.Forms.Button();
            ButtonRemoveSecondImage = new System.Windows.Forms.Button();
            ButtonClearAll = new System.Windows.Forms.Button();
            ButtonShowHideIndexedImages = new System.Windows.Forms.Button();
            ButtonBackgroundImage = new System.Windows.Forms.Button();
            ButtonResetZoom = new System.Windows.Forms.Button();
            ButtonBatchExport = new System.Windows.Forms.Button();
            buttonToggleFavorite = new System.Windows.Forms.Button();
            listBoxImages = new System.Windows.Forms.ListBox();
            contextMenuStripListbox = new System.Windows.Forms.ContextMenuStrip(components);
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBoxSearchTexbox = new System.Windows.Forms.ToolStripTextBox();
            ButtonLoadImages = new System.Windows.Forms.Button();
            groupBoxOverlay = new System.Windows.Forms.GroupBox();
            labelOpacity = new System.Windows.Forms.Label();
            trackBarOpacity = new System.Windows.Forms.TrackBar();
            labelOpacityValue = new System.Windows.Forms.Label();
            labelScale = new System.Windows.Forms.Label();
            trackBarScale = new System.Windows.Forms.TrackBar();
            labelScaleValue = new System.Windows.Forms.Label();
            groupBoxChroma = new System.Windows.Forms.GroupBox();
            checkBoxUseChromaKey = new System.Windows.Forms.CheckBox();
            labelChroma1 = new System.Windows.Forms.Label();
            buttonPickChromaColor1 = new System.Windows.Forms.Button();
            labelChroma2 = new System.Windows.Forms.Label();
            buttonPickChromaColor2 = new System.Windows.Forms.Button();
            groupBoxCrop = new System.Windows.Forms.GroupBox();
            labelWidth = new System.Windows.Forms.Label();
            textBoxWidth = new System.Windows.Forms.TextBox();
            labelHeight = new System.Windows.Forms.Label();
            textBoxHeight = new System.Windows.Forms.TextBox();
            ButtonCrop = new System.Windows.Forms.Button();
            checkBoxCuttingTemplate = new System.Windows.Forms.CheckBox();
            checkBoxBlack = new System.Windows.Forms.CheckBox();
            checkBoxWhite = new System.Windows.Forms.CheckBox();
            ButtonSaveCuttingTemplate = new System.Windows.Forms.Button();
            ButtonCopyCuttingTemplateToClipboard = new System.Windows.Forms.Button();
            groupBoxFavorites = new System.Windows.Forms.GroupBox();
            listBoxFavorites = new System.Windows.Forms.ListBox();
            labelImageInfo = new System.Windows.Forms.Label();
            labelContentHeight = new System.Windows.Forms.Label();
            labelPixelInfo = new System.Windows.Forms.Label();
            labelZoom = new System.Windows.Forms.Label();
            pictureBoxArtworkGallery = new System.Windows.Forms.PictureBox();
            contextMenuStripArtworkGallery = new System.Windows.Forms.ContextMenuStrip(components);
            loadSecondImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadClipboradToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadGifToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorArtworkGallery = new System.Windows.Forms.ToolStripSeparator();
            centerOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            flipVerticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            FlipHorizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ButtonCrosshair = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            panelLeft.SuspendLayout();
            groupBoxControl.SuspendLayout();
            contextMenuStripListbox.SuspendLayout();
            groupBoxOverlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScale).BeginInit();
            groupBoxChroma.SuspendLayout();
            groupBoxCrop.SuspendLayout();
            groupBoxFavorites.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxArtworkGallery).BeginInit();
            contextMenuStripArtworkGallery.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(panelLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(pictureBoxArtworkGallery);
            splitContainerMain.Size = new System.Drawing.Size(1050, 722);
            splitContainerMain.SplitterDistance = 477;
            splitContainerMain.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.AutoScroll = true;
            panelLeft.Controls.Add(groupBoxControl);
            panelLeft.Controls.Add(groupBoxOverlay);
            panelLeft.Controls.Add(groupBoxChroma);
            panelLeft.Controls.Add(groupBoxCrop);
            panelLeft.Controls.Add(groupBoxFavorites);
            panelLeft.Controls.Add(labelImageInfo);
            panelLeft.Controls.Add(labelContentHeight);
            panelLeft.Controls.Add(labelPixelInfo);
            panelLeft.Controls.Add(labelZoom);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            panelLeft.Location = new System.Drawing.Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Size = new System.Drawing.Size(477, 722);
            panelLeft.TabIndex = 0;
            // 
            // groupBoxControl
            // 
            groupBoxControl.Controls.Add(ButtonCrosshair);
            groupBoxControl.Controls.Add(ButtonRuler);
            groupBoxControl.Controls.Add(ButtonResetOffset);
            groupBoxControl.Controls.Add(ButtonRemoveBackground);
            groupBoxControl.Controls.Add(ButtonLeft);
            groupBoxControl.Controls.Add(ButtonRight);
            groupBoxControl.Controls.Add(checkBoxSecondArtwork);
            groupBoxControl.Controls.Add(ButtonDrawRhombus);
            groupBoxControl.Controls.Add(ButtonRemoveGif);
            groupBoxControl.Controls.Add(ButtonRemoveSecondImage);
            groupBoxControl.Controls.Add(ButtonClearAll);
            groupBoxControl.Controls.Add(ButtonShowHideIndexedImages);
            groupBoxControl.Controls.Add(ButtonBackgroundImage);
            groupBoxControl.Controls.Add(ButtonResetZoom);
            groupBoxControl.Controls.Add(ButtonBatchExport);
            groupBoxControl.Controls.Add(buttonToggleFavorite);
            groupBoxControl.Controls.Add(listBoxImages);
            groupBoxControl.Controls.Add(ButtonLoadImages);
            groupBoxControl.Location = new System.Drawing.Point(8, 8);
            groupBoxControl.Name = "groupBoxControl";
            groupBoxControl.Size = new System.Drawing.Size(458, 240);
            groupBoxControl.TabIndex = 0;
            groupBoxControl.TabStop = false;
            groupBoxControl.Text = "Control";
            // 
            // ButtonRuler
            // 
            ButtonRuler.Location = new System.Drawing.Point(140, 165);
            ButtonRuler.Name = "ButtonRuler";
            ButtonRuler.Size = new System.Drawing.Size(53, 22);
            ButtonRuler.TabIndex = 15;
            ButtonRuler.Text = "Ruler";
            ButtonRuler.UseVisualStyleBackColor = true;
            ButtonRuler.Click += ButtonRuler_Click;
            // 
            // ButtonResetOffset
            // 
            ButtonResetOffset.Location = new System.Drawing.Point(340, 212);
            ButtonResetOffset.Name = "ButtonResetOffset";
            ButtonResetOffset.Size = new System.Drawing.Size(93, 22);
            ButtonResetOffset.TabIndex = 14;
            ButtonResetOffset.Text = "Reset Offset";
            ButtonResetOffset.UseVisualStyleBackColor = true;
            ButtonResetOffset.Click += ButtonResetOffset_Click;
            // 
            // ButtonRemoveBackground
            // 
            ButtonRemoveBackground.Location = new System.Drawing.Point(6, 165);
            ButtonRemoveBackground.Name = "ButtonRemoveBackground";
            ButtonRemoveBackground.Size = new System.Drawing.Size(115, 22);
            ButtonRemoveBackground.TabIndex = 13;
            ButtonRemoveBackground.Text = "Remove BG";
            ButtonRemoveBackground.UseVisualStyleBackColor = true;
            ButtonRemoveBackground.Click += ButtonRemoveBackground_Click;
            // 
            // ButtonLeft
            // 
            ButtonLeft.Location = new System.Drawing.Point(6, 212);
            ButtonLeft.Name = "ButtonLeft";
            ButtonLeft.Size = new System.Drawing.Size(75, 23);
            ButtonLeft.TabIndex = 0;
            ButtonLeft.Text = "◄ Left";
            ButtonLeft.Click += ButtonLeft_Click;
            // 
            // ButtonRight
            // 
            ButtonRight.Location = new System.Drawing.Point(87, 212);
            ButtonRight.Name = "ButtonRight";
            ButtonRight.Size = new System.Drawing.Size(75, 23);
            ButtonRight.TabIndex = 1;
            ButtonRight.Text = "Right ►";
            ButtonRight.Click += ButtonRight_Click;
            // 
            // checkBoxSecondArtwork
            // 
            checkBoxSecondArtwork.Checked = true;
            checkBoxSecondArtwork.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSecondArtwork.Location = new System.Drawing.Point(342, 187);
            checkBoxSecondArtwork.Name = "checkBoxSecondArtwork";
            checkBoxSecondArtwork.Size = new System.Drawing.Size(115, 19);
            checkBoxSecondArtwork.TabIndex = 2;
            checkBoxSecondArtwork.Text = "Second Artwork";
            checkBoxSecondArtwork.CheckedChanged += CheckBoxSecondArtwork_CheckedChanged;
            // 
            // ButtonDrawRhombus
            // 
            ButtonDrawRhombus.Location = new System.Drawing.Point(370, 20);
            ButtonDrawRhombus.Name = "ButtonDrawRhombus";
            ButtonDrawRhombus.Size = new System.Drawing.Size(80, 23);
            ButtonDrawRhombus.TabIndex = 3;
            ButtonDrawRhombus.Text = "Rhombus";
            ButtonDrawRhombus.Click += ButtonDrawRhombus_Click;
            // 
            // ButtonRemoveGif
            // 
            ButtonRemoveGif.Location = new System.Drawing.Point(6, 20);
            ButtonRemoveGif.Name = "ButtonRemoveGif";
            ButtonRemoveGif.Size = new System.Drawing.Size(115, 23);
            ButtonRemoveGif.TabIndex = 4;
            ButtonRemoveGif.Text = "Remove GIF";
            ButtonRemoveGif.Click += ButtonRemoveGif_Click;
            // 
            // ButtonRemoveSecondImage
            // 
            ButtonRemoveSecondImage.Location = new System.Drawing.Point(6, 49);
            ButtonRemoveSecondImage.Name = "ButtonRemoveSecondImage";
            ButtonRemoveSecondImage.Size = new System.Drawing.Size(115, 23);
            ButtonRemoveSecondImage.TabIndex = 5;
            ButtonRemoveSecondImage.Text = "Remove Second";
            ButtonRemoveSecondImage.Click += ButtonRemoveSecondImage_Click;
            // 
            // ButtonClearAll
            // 
            ButtonClearAll.Location = new System.Drawing.Point(6, 78);
            ButtonClearAll.Name = "ButtonClearAll";
            ButtonClearAll.Size = new System.Drawing.Size(115, 23);
            ButtonClearAll.TabIndex = 6;
            ButtonClearAll.Text = "Clear All";
            ButtonClearAll.Click += ButtonClearAllItems_Click;
            // 
            // ButtonShowHideIndexedImages
            // 
            ButtonShowHideIndexedImages.Location = new System.Drawing.Point(6, 107);
            ButtonShowHideIndexedImages.Name = "ButtonShowHideIndexedImages";
            ButtonShowHideIndexedImages.Size = new System.Drawing.Size(115, 23);
            ButtonShowHideIndexedImages.TabIndex = 7;
            ButtonShowHideIndexedImages.Text = "Hide Index";
            ButtonShowHideIndexedImages.Click += ButtonShowHideIndexedImages_Click;
            // 
            // ButtonBackgroundImage
            // 
            ButtonBackgroundImage.Location = new System.Drawing.Point(6, 136);
            ButtonBackgroundImage.Name = "ButtonBackgroundImage";
            ButtonBackgroundImage.Size = new System.Drawing.Size(115, 23);
            ButtonBackgroundImage.TabIndex = 8;
            ButtonBackgroundImage.Text = "Background";
            ButtonBackgroundImage.Click += ButtonBackgroundImage_Click;
            // 
            // ButtonResetZoom
            // 
            ButtonResetZoom.Location = new System.Drawing.Point(254, 212);
            ButtonResetZoom.Name = "ButtonResetZoom";
            ButtonResetZoom.Size = new System.Drawing.Size(80, 22);
            ButtonResetZoom.TabIndex = 9;
            ButtonResetZoom.Text = "Reset Zoom";
            ButtonResetZoom.Click += ButtonResetZoom_Click;
            // 
            // ButtonBatchExport
            // 
            ButtonBatchExport.Location = new System.Drawing.Point(370, 49);
            ButtonBatchExport.Name = "ButtonBatchExport";
            ButtonBatchExport.Size = new System.Drawing.Size(80, 23);
            ButtonBatchExport.TabIndex = 9;
            ButtonBatchExport.Text = "Batch Export";
            ButtonBatchExport.Click += ButtonBatchExport_Click;
            // 
            // buttonToggleFavorite
            // 
            buttonToggleFavorite.Location = new System.Drawing.Point(370, 78);
            buttonToggleFavorite.Name = "buttonToggleFavorite";
            buttonToggleFavorite.Size = new System.Drawing.Size(80, 23);
            buttonToggleFavorite.TabIndex = 10;
            buttonToggleFavorite.Text = "☆ Favorite";
            buttonToggleFavorite.Click += ButtonToggleFavorite_Click;
            // 
            // listBoxImages
            // 
            listBoxImages.ContextMenuStrip = contextMenuStripListbox;
            listBoxImages.ItemHeight = 15;
            listBoxImages.Location = new System.Drawing.Point(140, 20);
            listBoxImages.Name = "listBoxImages";
            listBoxImages.Size = new System.Drawing.Size(218, 139);
            listBoxImages.TabIndex = 11;
            listBoxImages.SelectedIndexChanged += listBoxImages_SelectedIndexChanged;
            // 
            // contextMenuStripListbox
            // 
            contextMenuStripListbox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem });
            contextMenuStripListbox.Name = "contextMenuStripListbox";
            contextMenuStripListbox.Size = new System.Drawing.Size(110, 26);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxSearchTexbox });
            searchToolStripMenuItem.Image = Properties.Resources.zoomminus;
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            searchToolStripMenuItem.Text = "Search";
            // 
            // toolStripTextBoxSearchTexbox
            // 
            toolStripTextBoxSearchTexbox.Name = "toolStripTextBoxSearchTexbox";
            toolStripTextBoxSearchTexbox.Size = new System.Drawing.Size(100, 23);
            toolStripTextBoxSearchTexbox.KeyDown += toolStripTextBoxSearchTexbox_KeyDown;
            toolStripTextBoxSearchTexbox.TextChanged += toolStripTextBoxSearchTexbox_TextChanged;
            // 
            // ButtonLoadImages
            // 
            ButtonLoadImages.Location = new System.Drawing.Point(168, 212);
            ButtonLoadImages.Name = "ButtonLoadImages";
            ButtonLoadImages.Size = new System.Drawing.Size(80, 23);
            ButtonLoadImages.TabIndex = 12;
            ButtonLoadImages.Text = "Directory";
            ButtonLoadImages.Click += ButtonLoadImages_Click;
            // 
            // groupBoxOverlay
            // 
            groupBoxOverlay.Controls.Add(labelOpacity);
            groupBoxOverlay.Controls.Add(trackBarOpacity);
            groupBoxOverlay.Controls.Add(labelOpacityValue);
            groupBoxOverlay.Controls.Add(labelScale);
            groupBoxOverlay.Controls.Add(trackBarScale);
            groupBoxOverlay.Controls.Add(labelScaleValue);
            groupBoxOverlay.Location = new System.Drawing.Point(8, 254);
            groupBoxOverlay.Name = "groupBoxOverlay";
            groupBoxOverlay.Size = new System.Drawing.Size(458, 90);
            groupBoxOverlay.TabIndex = 1;
            groupBoxOverlay.TabStop = false;
            groupBoxOverlay.Text = "Overlay Adjustments";
            // 
            // labelOpacity
            // 
            labelOpacity.AutoSize = true;
            labelOpacity.Location = new System.Drawing.Point(6, 22);
            labelOpacity.Name = "labelOpacity";
            labelOpacity.Size = new System.Drawing.Size(48, 15);
            labelOpacity.TabIndex = 0;
            labelOpacity.Text = "Opacity";
            // 
            // trackBarOpacity
            // 
            trackBarOpacity.Location = new System.Drawing.Point(60, 18);
            trackBarOpacity.Name = "trackBarOpacity";
            trackBarOpacity.Size = new System.Drawing.Size(300, 45);
            trackBarOpacity.TabIndex = 0;
            trackBarOpacity.TickFrequency = 25;
            // 
            // labelOpacityValue
            // 
            labelOpacityValue.AutoSize = true;
            labelOpacityValue.Location = new System.Drawing.Point(368, 22);
            labelOpacityValue.Name = "labelOpacityValue";
            labelOpacityValue.Size = new System.Drawing.Size(35, 15);
            labelOpacityValue.TabIndex = 1;
            labelOpacityValue.Text = "100%";
            // 
            // labelScale
            // 
            labelScale.AutoSize = true;
            labelScale.Location = new System.Drawing.Point(6, 58);
            labelScale.Name = "labelScale";
            labelScale.Size = new System.Drawing.Size(34, 15);
            labelScale.TabIndex = 2;
            labelScale.Text = "Scale";
            // 
            // trackBarScale
            // 
            trackBarScale.Location = new System.Drawing.Point(60, 54);
            trackBarScale.Name = "trackBarScale";
            trackBarScale.Size = new System.Drawing.Size(300, 45);
            trackBarScale.TabIndex = 1;
            trackBarScale.TickFrequency = 5;
            // 
            // labelScaleValue
            // 
            labelScaleValue.AutoSize = true;
            labelScaleValue.Location = new System.Drawing.Point(368, 58);
            labelScaleValue.Name = "labelScaleValue";
            labelScaleValue.Size = new System.Drawing.Size(30, 15);
            labelScaleValue.TabIndex = 3;
            labelScaleValue.Text = "1.0×";
            // 
            // groupBoxChroma
            // 
            groupBoxChroma.Controls.Add(checkBoxUseChromaKey);
            groupBoxChroma.Controls.Add(labelChroma1);
            groupBoxChroma.Controls.Add(buttonPickChromaColor1);
            groupBoxChroma.Controls.Add(labelChroma2);
            groupBoxChroma.Controls.Add(buttonPickChromaColor2);
            groupBoxChroma.Location = new System.Drawing.Point(8, 343);
            groupBoxChroma.Name = "groupBoxChroma";
            groupBoxChroma.Size = new System.Drawing.Size(458, 60);
            groupBoxChroma.TabIndex = 2;
            groupBoxChroma.TabStop = false;
            groupBoxChroma.Text = "Chroma Key (transparency color)";
            // 
            // checkBoxUseChromaKey
            // 
            checkBoxUseChromaKey.AutoSize = true;
            checkBoxUseChromaKey.Checked = true;
            checkBoxUseChromaKey.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxUseChromaKey.Location = new System.Drawing.Point(6, 26);
            checkBoxUseChromaKey.Name = "checkBoxUseChromaKey";
            checkBoxUseChromaKey.Size = new System.Drawing.Size(59, 19);
            checkBoxUseChromaKey.TabIndex = 0;
            checkBoxUseChromaKey.Text = "Active";
            checkBoxUseChromaKey.CheckedChanged += CheckBoxUseChromaKey_CheckedChanged;
            // 
            // labelChroma1
            // 
            labelChroma1.AutoSize = true;
            labelChroma1.Location = new System.Drawing.Point(80, 26);
            labelChroma1.Name = "labelChroma1";
            labelChroma1.Size = new System.Drawing.Size(48, 15);
            labelChroma1.TabIndex = 1;
            labelChroma1.Text = "Color 1:";
            // 
            // buttonPickChromaColor1
            // 
            buttonPickChromaColor1.BackColor = System.Drawing.Color.Black;
            buttonPickChromaColor1.Location = new System.Drawing.Point(138, 22);
            buttonPickChromaColor1.Name = "buttonPickChromaColor1";
            buttonPickChromaColor1.Size = new System.Drawing.Size(55, 22);
            buttonPickChromaColor1.TabIndex = 2;
            buttonPickChromaColor1.UseVisualStyleBackColor = false;
            buttonPickChromaColor1.Click += ButtonPickChromaColor1_Click;
            // 
            // labelChroma2
            // 
            labelChroma2.AutoSize = true;
            labelChroma2.Location = new System.Drawing.Point(210, 26);
            labelChroma2.Name = "labelChroma2";
            labelChroma2.Size = new System.Drawing.Size(48, 15);
            labelChroma2.TabIndex = 3;
            labelChroma2.Text = "Color 2:";
            // 
            // buttonPickChromaColor2
            // 
            buttonPickChromaColor2.BackColor = System.Drawing.Color.White;
            buttonPickChromaColor2.Location = new System.Drawing.Point(268, 22);
            buttonPickChromaColor2.Name = "buttonPickChromaColor2";
            buttonPickChromaColor2.Size = new System.Drawing.Size(55, 22);
            buttonPickChromaColor2.TabIndex = 4;
            buttonPickChromaColor2.UseVisualStyleBackColor = false;
            buttonPickChromaColor2.Click += ButtonPickChromaColor2_Click;
            // 
            // groupBoxCrop
            // 
            groupBoxCrop.Controls.Add(labelWidth);
            groupBoxCrop.Controls.Add(textBoxWidth);
            groupBoxCrop.Controls.Add(labelHeight);
            groupBoxCrop.Controls.Add(textBoxHeight);
            groupBoxCrop.Controls.Add(ButtonCrop);
            groupBoxCrop.Controls.Add(checkBoxCuttingTemplate);
            groupBoxCrop.Controls.Add(checkBoxBlack);
            groupBoxCrop.Controls.Add(checkBoxWhite);
            groupBoxCrop.Controls.Add(ButtonSaveCuttingTemplate);
            groupBoxCrop.Controls.Add(ButtonCopyCuttingTemplateToClipboard);
            groupBoxCrop.Location = new System.Drawing.Point(8, 411);
            groupBoxCrop.Name = "groupBoxCrop";
            groupBoxCrop.Size = new System.Drawing.Size(458, 90);
            groupBoxCrop.TabIndex = 3;
            groupBoxCrop.TabStop = false;
            groupBoxCrop.Text = "Crop / Cutting Template";
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new System.Drawing.Point(6, 24);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new System.Drawing.Size(39, 15);
            labelWidth.TabIndex = 0;
            labelWidth.Text = "Width";
            // 
            // textBoxWidth
            // 
            textBoxWidth.Location = new System.Drawing.Point(6, 42);
            textBoxWidth.Name = "textBoxWidth";
            textBoxWidth.Size = new System.Drawing.Size(60, 23);
            textBoxWidth.TabIndex = 1;
            textBoxWidth.TextChanged += TextBoxCuttingTemplateSizeChanged;
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new System.Drawing.Point(74, 24);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new System.Drawing.Size(43, 15);
            labelHeight.TabIndex = 2;
            labelHeight.Text = "Height";
            // 
            // textBoxHeight
            // 
            textBoxHeight.Location = new System.Drawing.Point(74, 42);
            textBoxHeight.Name = "textBoxHeight";
            textBoxHeight.Size = new System.Drawing.Size(60, 23);
            textBoxHeight.TabIndex = 3;
            textBoxHeight.TextChanged += TextBoxCuttingTemplateSizeChanged;
            // 
            // ButtonCrop
            // 
            ButtonCrop.Location = new System.Drawing.Point(144, 42);
            ButtonCrop.Name = "ButtonCrop";
            ButtonCrop.Size = new System.Drawing.Size(56, 23);
            ButtonCrop.TabIndex = 4;
            ButtonCrop.Text = "Crop";
            ButtonCrop.Click += ButtonCrop_Click;
            // 
            // checkBoxCuttingTemplate
            // 
            checkBoxCuttingTemplate.AutoSize = true;
            checkBoxCuttingTemplate.Location = new System.Drawing.Point(208, 44);
            checkBoxCuttingTemplate.Name = "checkBoxCuttingTemplate";
            checkBoxCuttingTemplate.Size = new System.Drawing.Size(116, 19);
            checkBoxCuttingTemplate.TabIndex = 5;
            checkBoxCuttingTemplate.Text = "Cutting template";
            checkBoxCuttingTemplate.CheckedChanged += CheckBoxCuttingTemplate_CheckedChanged;
            // 
            // checkBoxBlack
            // 
            checkBoxBlack.AutoSize = true;
            checkBoxBlack.Location = new System.Drawing.Point(144, 18);
            checkBoxBlack.Name = "checkBoxBlack";
            checkBoxBlack.Size = new System.Drawing.Size(54, 19);
            checkBoxBlack.TabIndex = 6;
            checkBoxBlack.Text = "Black";
            checkBoxBlack.CheckedChanged += CheckBoxBackgroundColorChanged;
            // 
            // checkBoxWhite
            // 
            checkBoxWhite.AutoSize = true;
            checkBoxWhite.Location = new System.Drawing.Point(208, 18);
            checkBoxWhite.Name = "checkBoxWhite";
            checkBoxWhite.Size = new System.Drawing.Size(57, 19);
            checkBoxWhite.TabIndex = 7;
            checkBoxWhite.Text = "White";
            checkBoxWhite.CheckedChanged += CheckBoxBackgroundColorChanged;
            // 
            // ButtonSaveCuttingTemplate
            // 
            ButtonSaveCuttingTemplate.Location = new System.Drawing.Point(370, 18);
            ButtonSaveCuttingTemplate.Name = "ButtonSaveCuttingTemplate";
            ButtonSaveCuttingTemplate.Size = new System.Drawing.Size(80, 23);
            ButtonSaveCuttingTemplate.TabIndex = 8;
            ButtonSaveCuttingTemplate.Text = "Crop Save";
            ButtonSaveCuttingTemplate.Click += ButtonSaveCuttingTemplate_Click;
            // 
            // ButtonCopyCuttingTemplateToClipboard
            // 
            ButtonCopyCuttingTemplateToClipboard.Location = new System.Drawing.Point(370, 47);
            ButtonCopyCuttingTemplateToClipboard.Name = "ButtonCopyCuttingTemplateToClipboard";
            ButtonCopyCuttingTemplateToClipboard.Size = new System.Drawing.Size(80, 23);
            ButtonCopyCuttingTemplateToClipboard.TabIndex = 9;
            ButtonCopyCuttingTemplateToClipboard.Text = "Clipboard";
            ButtonCopyCuttingTemplateToClipboard.Click += ButtonCopyCuttingTemplateToClipboard_Click;
            // 
            // groupBoxFavorites
            // 
            groupBoxFavorites.Controls.Add(listBoxFavorites);
            groupBoxFavorites.Location = new System.Drawing.Point(8, 509);
            groupBoxFavorites.Name = "groupBoxFavorites";
            groupBoxFavorites.Size = new System.Drawing.Size(458, 100);
            groupBoxFavorites.TabIndex = 4;
            groupBoxFavorites.TabStop = false;
            groupBoxFavorites.Text = "Favorites";
            // 
            // listBoxFavorites
            // 
            listBoxFavorites.ItemHeight = 15;
            listBoxFavorites.Location = new System.Drawing.Point(6, 20);
            listBoxFavorites.Name = "listBoxFavorites";
            listBoxFavorites.Size = new System.Drawing.Size(442, 64);
            listBoxFavorites.TabIndex = 0;
            listBoxFavorites.SelectedIndexChanged += ListBoxFavorites_SelectedIndexChanged;
            // 
            // labelImageInfo
            // 
            labelImageInfo.Location = new System.Drawing.Point(8, 617);
            labelImageInfo.Name = "labelImageInfo";
            labelImageInfo.Size = new System.Drawing.Size(458, 18);
            labelImageInfo.TabIndex = 5;
            labelImageInfo.Text = "Base: – | Overlay: –";
            // 
            // labelContentHeight
            // 
            labelContentHeight.Location = new System.Drawing.Point(8, 637);
            labelContentHeight.Name = "labelContentHeight";
            labelContentHeight.Size = new System.Drawing.Size(458, 18);
            labelContentHeight.TabIndex = 6;
            labelContentHeight.Text = "Content height: –";
            // 
            // labelPixelInfo
            // 
            labelPixelInfo.Font = new System.Drawing.Font("Courier New", 8.25F);
            labelPixelInfo.Location = new System.Drawing.Point(8, 657);
            labelPixelInfo.Name = "labelPixelInfo";
            labelPixelInfo.Size = new System.Drawing.Size(458, 18);
            labelPixelInfo.TabIndex = 7;
            // 
            // labelZoom
            // 
            labelZoom.Location = new System.Drawing.Point(8, 677);
            labelZoom.Name = "labelZoom";
            labelZoom.Size = new System.Drawing.Size(200, 18);
            labelZoom.TabIndex = 8;
            labelZoom.Text = "Zoom: 100%";
            // 
            // pictureBoxArtworkGallery
            // 
            pictureBoxArtworkGallery.ContextMenuStrip = contextMenuStripArtworkGallery;
            pictureBoxArtworkGallery.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxArtworkGallery.Location = new System.Drawing.Point(0, 0);
            pictureBoxArtworkGallery.Name = "pictureBoxArtworkGallery";
            pictureBoxArtworkGallery.Size = new System.Drawing.Size(569, 722);
            pictureBoxArtworkGallery.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxArtworkGallery.TabIndex = 0;
            pictureBoxArtworkGallery.TabStop = false;
            pictureBoxArtworkGallery.Paint += PictureBoxArtworkGallery_Paint;
            // 
            // contextMenuStripArtworkGallery
            // 
            contextMenuStripArtworkGallery.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadSecondImageToolStripMenuItem, loadClipboradToolStripMenuItem, loadGifToolStripMenuItem, toolStripSeparatorArtworkGallery, centerOverlayToolStripMenuItem, flipVerticalToolStripMenuItem, FlipHorizontalToolStripMenuItem });
            contextMenuStripArtworkGallery.Name = "contextMenuStripArtworkGallery";
            contextMenuStripArtworkGallery.Size = new System.Drawing.Size(179, 142);
            // 
            // loadSecondImageToolStripMenuItem
            // 
            loadSecondImageToolStripMenuItem.Image = Properties.Resources.Animation;
            loadSecondImageToolStripMenuItem.Name = "loadSecondImageToolStripMenuItem";
            loadSecondImageToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            loadSecondImageToolStripMenuItem.Text = "Load Second Image";
            loadSecondImageToolStripMenuItem.Click += LoadSecondImageToolStripMenuItem_Click;
            // 
            // loadClipboradToolStripMenuItem
            // 
            loadClipboradToolStripMenuItem.Image = Properties.Resources.Clipbord;
            loadClipboradToolStripMenuItem.Name = "loadClipboradToolStripMenuItem";
            loadClipboradToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            loadClipboradToolStripMenuItem.Text = "Load Clipboard";
            loadClipboradToolStripMenuItem.Click += loadClipboradToolStripMenuItem_Click;
            // 
            // loadGifToolStripMenuItem
            // 
            loadGifToolStripMenuItem.Image = Properties.Resources.Animate;
            loadGifToolStripMenuItem.Name = "loadGifToolStripMenuItem";
            loadGifToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            loadGifToolStripMenuItem.Text = "Load GIF";
            loadGifToolStripMenuItem.Click += loadGifToolStripMenuItem_Click;
            // 
            // toolStripSeparatorArtworkGallery
            // 
            toolStripSeparatorArtworkGallery.Name = "toolStripSeparatorArtworkGallery";
            toolStripSeparatorArtworkGallery.Size = new System.Drawing.Size(175, 6);
            // 
            // centerOverlayToolStripMenuItem
            // 
            centerOverlayToolStripMenuItem.Image = Properties.Resources.Zeichnen;
            centerOverlayToolStripMenuItem.Name = "centerOverlayToolStripMenuItem";
            centerOverlayToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            centerOverlayToolStripMenuItem.Text = "Center Overlay";
            centerOverlayToolStripMenuItem.Click += CenterOverlayToolStripMenuItem_Click;
            // 
            // flipVerticalToolStripMenuItem
            // 
            flipVerticalToolStripMenuItem.Image = Properties.Resources.reset_2_;
            flipVerticalToolStripMenuItem.Name = "flipVerticalToolStripMenuItem";
            flipVerticalToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            flipVerticalToolStripMenuItem.Text = "Flip Vertical";
            flipVerticalToolStripMenuItem.Click += FlipVerticalToolStripMenuItem_Click;
            // 
            // FlipHorizontalToolStripMenuItem
            // 
            FlipHorizontalToolStripMenuItem.Image = Properties.Resources.reset_2_;
            FlipHorizontalToolStripMenuItem.Name = "FlipHorizontalToolStripMenuItem";
            FlipHorizontalToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            FlipHorizontalToolStripMenuItem.Text = "Flip Horizontal";
            FlipHorizontalToolStripMenuItem.Click += FlipHorizontalToolStripMenuItem_Click;
            // 
            // ButtonCrosshair
            // 
            ButtonCrosshair.Location = new System.Drawing.Point(199, 165);
            ButtonCrosshair.Name = "ButtonCrosshair";
            ButtonCrosshair.Size = new System.Drawing.Size(66, 22);
            ButtonCrosshair.TabIndex = 16;
            ButtonCrosshair.Text = "Crosshair";
            ButtonCrosshair.UseVisualStyleBackColor = true;
            ButtonCrosshair.Click += ButtonCrosshair_Click;
            // 
            // ArtworkGallery
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1050, 722);
            Controls.Add(splitContainerMain);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "ArtworkGallery";
            Text = "Artwork Gallery";
            KeyDown += ArtworkGallery_KeyDown;
            KeyUp += ArtworkGallery_KeyUp;
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            groupBoxControl.ResumeLayout(false);
            contextMenuStripListbox.ResumeLayout(false);
            groupBoxOverlay.ResumeLayout(false);
            groupBoxOverlay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarScale).EndInit();
            groupBoxChroma.ResumeLayout(false);
            groupBoxChroma.PerformLayout();
            groupBoxCrop.ResumeLayout(false);
            groupBoxCrop.PerformLayout();
            groupBoxFavorites.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxArtworkGallery).EndInit();
            contextMenuStripArtworkGallery.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel panelLeft;

        private System.Windows.Forms.GroupBox groupBoxControl;
        private System.Windows.Forms.Button ButtonLeft;
        private System.Windows.Forms.Button ButtonRight;
        private System.Windows.Forms.CheckBox checkBoxSecondArtwork;
        private System.Windows.Forms.Button ButtonDrawRhombus;
        private System.Windows.Forms.Button ButtonRemoveGif;
        private System.Windows.Forms.Button ButtonRemoveSecondImage;
        private System.Windows.Forms.Button ButtonClearAll;
        private System.Windows.Forms.Button ButtonShowHideIndexedImages;
        private System.Windows.Forms.Button ButtonBackgroundImage;
        private System.Windows.Forms.Button ButtonBatchExport;
        private System.Windows.Forms.Button buttonToggleFavorite;
        private System.Windows.Forms.ListBox listBoxImages;
        private System.Windows.Forms.Button ButtonLoadImages;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListbox;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearchTexbox;

        private System.Windows.Forms.GroupBox groupBoxOverlay;
        private System.Windows.Forms.Label labelOpacity;
        private System.Windows.Forms.TrackBar trackBarOpacity;
        private System.Windows.Forms.Label labelOpacityValue;
        private System.Windows.Forms.Label labelScale;
        private System.Windows.Forms.TrackBar trackBarScale;
        private System.Windows.Forms.Label labelScaleValue;

        private System.Windows.Forms.GroupBox groupBoxChroma;
        private System.Windows.Forms.CheckBox checkBoxUseChromaKey;
        private System.Windows.Forms.Label labelChroma1;
        private System.Windows.Forms.Button buttonPickChromaColor1;
        private System.Windows.Forms.Label labelChroma2;
        private System.Windows.Forms.Button buttonPickChromaColor2;

        private System.Windows.Forms.GroupBox groupBoxCrop;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.Button ButtonCrop;
        private System.Windows.Forms.CheckBox checkBoxCuttingTemplate;
        private System.Windows.Forms.CheckBox checkBoxBlack;
        private System.Windows.Forms.CheckBox checkBoxWhite;
        private System.Windows.Forms.Button ButtonSaveCuttingTemplate;
        private System.Windows.Forms.Button ButtonCopyCuttingTemplateToClipboard;

        private System.Windows.Forms.GroupBox groupBoxFavorites;
        private System.Windows.Forms.ListBox listBoxFavorites;

        private System.Windows.Forms.Label labelImageInfo;
        private System.Windows.Forms.Label labelContentHeight;
        private System.Windows.Forms.Label labelPixelInfo;
        private System.Windows.Forms.Label labelZoom;
        private System.Windows.Forms.Button ButtonResetZoom;

        private System.Windows.Forms.PictureBox pictureBoxArtworkGallery;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripArtworkGallery;
        private System.Windows.Forms.ToolStripMenuItem loadSecondImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadClipboradToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadGifToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorArtworkGallery;
        private System.Windows.Forms.ToolStripMenuItem centerOverlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flipVerticalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FlipHorizontalToolStripMenuItem;
        private System.Windows.Forms.Button ButtonRemoveBackground;
        private System.Windows.Forms.Button ButtonResetOffset;
        private System.Windows.Forms.Button ButtonRuler;
        private System.Windows.Forms.Button ButtonCrosshair;
    }
}
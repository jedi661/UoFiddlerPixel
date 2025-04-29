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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArtworkGallery));
            splitContainerArtworkGallery = new System.Windows.Forms.SplitContainer();
            groupBoxCrop = new System.Windows.Forms.GroupBox();
            ButtonCopyCuttingTemplateToClipboard = new System.Windows.Forms.Button();
            ButtonSaveCuttingTemplate = new System.Windows.Forms.Button();
            checkBoxBlack = new System.Windows.Forms.CheckBox();
            checkBoxWhite = new System.Windows.Forms.CheckBox();
            checkBoxCuttingTemplate = new System.Windows.Forms.CheckBox();
            ButtonCrop = new System.Windows.Forms.Button();
            labelWidth = new System.Windows.Forms.Label();
            labelHeight = new System.Windows.Forms.Label();
            textBoxHeight = new System.Windows.Forms.TextBox();
            textBoxWidth = new System.Windows.Forms.TextBox();
            label1ImageInfo = new System.Windows.Forms.Label();
            groupBoxArtworkGallery = new System.Windows.Forms.GroupBox();
            ButtonLoadImages = new System.Windows.Forms.Button();
            listBoxImages = new System.Windows.Forms.ListBox();
            contextMenuStripListbox = new System.Windows.Forms.ContextMenuStrip(components);
            searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBoxSearchTexbox = new System.Windows.Forms.ToolStripTextBox();
            ButtonBackgroundImage = new System.Windows.Forms.Button();
            ButtonShowHideIndexedImages = new System.Windows.Forms.Button();
            ButtonClearAll_Click = new System.Windows.Forms.Button();
            ButtonRemoveSecondImage = new System.Windows.Forms.Button();
            ButtonRemoveGif = new System.Windows.Forms.Button();
            ButtonDrawRhombus = new System.Windows.Forms.Button();
            checkBoxSecondArtwork = new System.Windows.Forms.CheckBox();
            ButtonLeft = new System.Windows.Forms.Button();
            ButtonRight = new System.Windows.Forms.Button();
            pictureBoxArtworkGallery = new System.Windows.Forms.PictureBox();
            contextMenuStripArtworkGallery = new System.Windows.Forms.ContextMenuStrip(components);
            loadSecondImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadClipboradToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadGifToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorArtworkGallery = new System.Windows.Forms.ToolStripSeparator();
            centerOverlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            flipVerticalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            FlipHorizontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainerArtworkGallery).BeginInit();
            splitContainerArtworkGallery.Panel1.SuspendLayout();
            splitContainerArtworkGallery.Panel2.SuspendLayout();
            splitContainerArtworkGallery.SuspendLayout();
            groupBoxCrop.SuspendLayout();
            groupBoxArtworkGallery.SuspendLayout();
            contextMenuStripListbox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxArtworkGallery).BeginInit();
            contextMenuStripArtworkGallery.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerArtworkGallery
            // 
            splitContainerArtworkGallery.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerArtworkGallery.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerArtworkGallery.Location = new System.Drawing.Point(0, 0);
            splitContainerArtworkGallery.Name = "splitContainerArtworkGallery";
            // 
            // splitContainerArtworkGallery.Panel1
            // 
            splitContainerArtworkGallery.Panel1.Controls.Add(groupBoxCrop);
            splitContainerArtworkGallery.Panel1.Controls.Add(label1ImageInfo);
            splitContainerArtworkGallery.Panel1.Controls.Add(groupBoxArtworkGallery);
            // 
            // splitContainerArtworkGallery.Panel2
            // 
            splitContainerArtworkGallery.Panel2.Controls.Add(pictureBoxArtworkGallery);
            splitContainerArtworkGallery.Size = new System.Drawing.Size(800, 450);
            splitContainerArtworkGallery.SplitterDistance = 444;
            splitContainerArtworkGallery.TabIndex = 0;
            // 
            // groupBoxCrop
            // 
            groupBoxCrop.Controls.Add(ButtonCopyCuttingTemplateToClipboard);
            groupBoxCrop.Controls.Add(ButtonSaveCuttingTemplate);
            groupBoxCrop.Controls.Add(checkBoxBlack);
            groupBoxCrop.Controls.Add(checkBoxWhite);
            groupBoxCrop.Controls.Add(checkBoxCuttingTemplate);
            groupBoxCrop.Controls.Add(ButtonCrop);
            groupBoxCrop.Controls.Add(labelWidth);
            groupBoxCrop.Controls.Add(labelHeight);
            groupBoxCrop.Controls.Add(textBoxHeight);
            groupBoxCrop.Controls.Add(textBoxWidth);
            groupBoxCrop.Location = new System.Drawing.Point(8, 231);
            groupBoxCrop.Name = "groupBoxCrop";
            groupBoxCrop.Size = new System.Drawing.Size(429, 84);
            groupBoxCrop.TabIndex = 5;
            groupBoxCrop.TabStop = false;
            groupBoxCrop.Text = "Crop Second Image";
            // 
            // ButtonCopyCuttingTemplateToClipboard
            // 
            ButtonCopyCuttingTemplateToClipboard.Location = new System.Drawing.Point(343, 44);
            ButtonCopyCuttingTemplateToClipboard.Name = "ButtonCopyCuttingTemplateToClipboard";
            ButtonCopyCuttingTemplateToClipboard.Size = new System.Drawing.Size(71, 23);
            ButtonCopyCuttingTemplateToClipboard.TabIndex = 9;
            ButtonCopyCuttingTemplateToClipboard.Text = "Clipboard";
            ButtonCopyCuttingTemplateToClipboard.UseVisualStyleBackColor = true;
            ButtonCopyCuttingTemplateToClipboard.Click += ButtonCopyCuttingTemplateToClipboard_Click;
            // 
            // ButtonSaveCuttingTemplate
            // 
            ButtonSaveCuttingTemplate.Location = new System.Drawing.Point(343, 17);
            ButtonSaveCuttingTemplate.Name = "ButtonSaveCuttingTemplate";
            ButtonSaveCuttingTemplate.Size = new System.Drawing.Size(71, 23);
            ButtonSaveCuttingTemplate.TabIndex = 8;
            ButtonSaveCuttingTemplate.Text = "Crop";
            ButtonSaveCuttingTemplate.UseVisualStyleBackColor = true;
            ButtonSaveCuttingTemplate.Click += ButtonSaveCuttingTemplate_Click;
            // 
            // checkBoxBlack
            // 
            checkBoxBlack.AutoSize = true;
            checkBoxBlack.Location = new System.Drawing.Point(290, 21);
            checkBoxBlack.Name = "checkBoxBlack";
            checkBoxBlack.Size = new System.Drawing.Size(54, 19);
            checkBoxBlack.TabIndex = 7;
            checkBoxBlack.Text = "Black";
            checkBoxBlack.UseVisualStyleBackColor = true;
            checkBoxBlack.CheckedChanged += CheckBoxBackgroundColorChanged;
            // 
            // checkBoxWhite
            // 
            checkBoxWhite.AutoSize = true;
            checkBoxWhite.Location = new System.Drawing.Point(227, 21);
            checkBoxWhite.Name = "checkBoxWhite";
            checkBoxWhite.Size = new System.Drawing.Size(57, 19);
            checkBoxWhite.TabIndex = 6;
            checkBoxWhite.Text = "White";
            checkBoxWhite.UseVisualStyleBackColor = true;
            checkBoxWhite.CheckedChanged += CheckBoxBackgroundColorChanged;
            // 
            // checkBoxCuttingTemplate
            // 
            checkBoxCuttingTemplate.AutoSize = true;
            checkBoxCuttingTemplate.Location = new System.Drawing.Point(227, 46);
            checkBoxCuttingTemplate.Name = "checkBoxCuttingTemplate";
            checkBoxCuttingTemplate.Size = new System.Drawing.Size(116, 19);
            checkBoxCuttingTemplate.TabIndex = 5;
            checkBoxCuttingTemplate.Text = "Cutting template";
            checkBoxCuttingTemplate.UseVisualStyleBackColor = true;
            checkBoxCuttingTemplate.CheckedChanged += CheckBoxCuttingTemplate_CheckedChanged;
            // 
            // ButtonCrop
            // 
            ButtonCrop.Location = new System.Drawing.Point(158, 43);
            ButtonCrop.Name = "ButtonCrop";
            ButtonCrop.Size = new System.Drawing.Size(56, 23);
            ButtonCrop.TabIndex = 4;
            ButtonCrop.Text = "Crop";
            ButtonCrop.UseVisualStyleBackColor = true;
            ButtonCrop.Click += ButtonCrop_Click;
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new System.Drawing.Point(9, 26);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new System.Drawing.Size(39, 15);
            labelWidth.TabIndex = 3;
            labelWidth.Text = "Width";
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new System.Drawing.Point(87, 26);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new System.Drawing.Size(43, 15);
            labelHeight.TabIndex = 2;
            labelHeight.Text = "Height";
            // 
            // textBoxHeight
            // 
            textBoxHeight.Location = new System.Drawing.Point(87, 44);
            textBoxHeight.Name = "textBoxHeight";
            textBoxHeight.Size = new System.Drawing.Size(63, 23);
            textBoxHeight.TabIndex = 1;
            textBoxHeight.TextChanged += TextBoxCuttingTemplateSizeChanged;
            // 
            // textBoxWidth
            // 
            textBoxWidth.Location = new System.Drawing.Point(9, 44);
            textBoxWidth.Name = "textBoxWidth";
            textBoxWidth.Size = new System.Drawing.Size(63, 23);
            textBoxWidth.TabIndex = 0;
            textBoxWidth.TextChanged += TextBoxCuttingTemplateSizeChanged;
            // 
            // label1ImageInfo
            // 
            label1ImageInfo.AutoSize = true;
            label1ImageInfo.Location = new System.Drawing.Point(18, 200);
            label1ImageInfo.Name = "label1ImageInfo";
            label1ImageInfo.Size = new System.Drawing.Size(28, 15);
            label1ImageInfo.TabIndex = 4;
            label1ImageInfo.Text = "Info";
            // 
            // groupBoxArtworkGallery
            // 
            groupBoxArtworkGallery.Controls.Add(ButtonLoadImages);
            groupBoxArtworkGallery.Controls.Add(listBoxImages);
            groupBoxArtworkGallery.Controls.Add(ButtonBackgroundImage);
            groupBoxArtworkGallery.Controls.Add(ButtonShowHideIndexedImages);
            groupBoxArtworkGallery.Controls.Add(ButtonClearAll_Click);
            groupBoxArtworkGallery.Controls.Add(ButtonRemoveSecondImage);
            groupBoxArtworkGallery.Controls.Add(ButtonRemoveGif);
            groupBoxArtworkGallery.Controls.Add(ButtonDrawRhombus);
            groupBoxArtworkGallery.Controls.Add(checkBoxSecondArtwork);
            groupBoxArtworkGallery.Controls.Add(ButtonLeft);
            groupBoxArtworkGallery.Controls.Add(ButtonRight);
            groupBoxArtworkGallery.Location = new System.Drawing.Point(8, 12);
            groupBoxArtworkGallery.Name = "groupBoxArtworkGallery";
            groupBoxArtworkGallery.Size = new System.Drawing.Size(429, 174);
            groupBoxArtworkGallery.TabIndex = 2;
            groupBoxArtworkGallery.TabStop = false;
            groupBoxArtworkGallery.Text = "Control";
            // 
            // ButtonLoadImages
            // 
            ButtonLoadImages.Location = new System.Drawing.Point(333, 108);
            ButtonLoadImages.Name = "ButtonLoadImages";
            ButtonLoadImages.Size = new System.Drawing.Size(75, 23);
            ButtonLoadImages.TabIndex = 10;
            ButtonLoadImages.Text = "Directory";
            ButtonLoadImages.UseVisualStyleBackColor = true;
            ButtonLoadImages.Click += ButtonLoadImages_Click;
            // 
            // listBoxImages
            // 
            listBoxImages.ContextMenuStrip = contextMenuStripListbox;
            listBoxImages.FormattingEnabled = true;
            listBoxImages.ItemHeight = 15;
            listBoxImages.Location = new System.Drawing.Point(140, 22);
            listBoxImages.Name = "listBoxImages";
            listBoxImages.Size = new System.Drawing.Size(169, 109);
            listBoxImages.TabIndex = 9;
            listBoxImages.SelectedIndexChanged += listBoxImages_SelectedIndexChanged;
            // 
            // contextMenuStripListbox
            // 
            contextMenuStripListbox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { searchToolStripMenuItem });
            contextMenuStripListbox.Name = "contextMenuStripListbox";
            contextMenuStripListbox.Size = new System.Drawing.Size(181, 48);
            // 
            // searchToolStripMenuItem
            // 
            searchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxSearchTexbox });
            searchToolStripMenuItem.Image = Properties.Resources.zoomminus;
            searchToolStripMenuItem.Name = "searchToolStripMenuItem";
            searchToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            searchToolStripMenuItem.Text = "Search";
            // 
            // toolStripTextBoxSearchTexbox
            // 
            toolStripTextBoxSearchTexbox.Name = "toolStripTextBoxSearchTexbox";
            toolStripTextBoxSearchTexbox.Size = new System.Drawing.Size(100, 23);
            toolStripTextBoxSearchTexbox.KeyDown += toolStripTextBoxSearchTexbox_KeyDown;
            toolStripTextBoxSearchTexbox.TextChanged += toolStripTextBoxSearchTexbox_TextChanged;
            // 
            // ButtonBackgroundImage
            // 
            ButtonBackgroundImage.Location = new System.Drawing.Point(333, 144);
            ButtonBackgroundImage.Name = "ButtonBackgroundImage";
            ButtonBackgroundImage.Size = new System.Drawing.Size(75, 23);
            ButtonBackgroundImage.TabIndex = 8;
            ButtonBackgroundImage.Text = "Image";
            ButtonBackgroundImage.UseVisualStyleBackColor = true;
            ButtonBackgroundImage.Click += ButtonBackgroundImage_Click;
            // 
            // ButtonShowHideIndexedImages
            // 
            ButtonShowHideIndexedImages.Location = new System.Drawing.Point(6, 109);
            ButtonShowHideIndexedImages.Name = "ButtonShowHideIndexedImages";
            ButtonShowHideIndexedImages.Size = new System.Drawing.Size(109, 23);
            ButtonShowHideIndexedImages.TabIndex = 7;
            ButtonShowHideIndexedImages.Text = "Hide Index";
            ButtonShowHideIndexedImages.UseVisualStyleBackColor = true;
            ButtonShowHideIndexedImages.Click += ButtonShowHideIndexedImages_Click;
            // 
            // ButtonClearAll_Click
            // 
            ButtonClearAll_Click.Location = new System.Drawing.Point(6, 80);
            ButtonClearAll_Click.Name = "ButtonClearAll_Click";
            ButtonClearAll_Click.Size = new System.Drawing.Size(109, 23);
            ButtonClearAll_Click.TabIndex = 6;
            ButtonClearAll_Click.Text = "Clear All";
            ButtonClearAll_Click.UseVisualStyleBackColor = true;
            ButtonClearAll_Click.Click += ButtonClearAll_Click_Click;
            // 
            // ButtonRemoveSecondImage
            // 
            ButtonRemoveSecondImage.Location = new System.Drawing.Point(6, 51);
            ButtonRemoveSecondImage.Name = "ButtonRemoveSecondImage";
            ButtonRemoveSecondImage.Size = new System.Drawing.Size(109, 23);
            ButtonRemoveSecondImage.TabIndex = 5;
            ButtonRemoveSecondImage.Text = "Remove Second";
            ButtonRemoveSecondImage.UseVisualStyleBackColor = true;
            ButtonRemoveSecondImage.Click += ButtonRemoveSecondImage_Click;
            // 
            // ButtonRemoveGif
            // 
            ButtonRemoveGif.Location = new System.Drawing.Point(6, 22);
            ButtonRemoveGif.Name = "ButtonRemoveGif";
            ButtonRemoveGif.Size = new System.Drawing.Size(109, 23);
            ButtonRemoveGif.TabIndex = 4;
            ButtonRemoveGif.Text = "Remove Gif";
            ButtonRemoveGif.UseVisualStyleBackColor = true;
            ButtonRemoveGif.Click += ButtonRemoveGif_Click;
            // 
            // ButtonDrawRhombus
            // 
            ButtonDrawRhombus.Location = new System.Drawing.Point(333, 22);
            ButtonDrawRhombus.Name = "ButtonDrawRhombus";
            ButtonDrawRhombus.Size = new System.Drawing.Size(75, 23);
            ButtonDrawRhombus.TabIndex = 3;
            ButtonDrawRhombus.Text = "Rhombus";
            ButtonDrawRhombus.UseVisualStyleBackColor = true;
            ButtonDrawRhombus.Click += ButtonDrawRhombus_Click;
            // 
            // checkBoxSecondArtwork
            // 
            checkBoxSecondArtwork.AutoSize = true;
            checkBoxSecondArtwork.Checked = true;
            checkBoxSecondArtwork.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSecondArtwork.Location = new System.Drawing.Point(168, 148);
            checkBoxSecondArtwork.Name = "checkBoxSecondArtwork";
            checkBoxSecondArtwork.Size = new System.Drawing.Size(110, 19);
            checkBoxSecondArtwork.TabIndex = 2;
            checkBoxSecondArtwork.Text = "Second Artwork";
            checkBoxSecondArtwork.UseVisualStyleBackColor = true;
            checkBoxSecondArtwork.CheckedChanged += CheckBoxSecondArtwork_CheckedChanged;
            // 
            // ButtonLeft
            // 
            ButtonLeft.Location = new System.Drawing.Point(6, 145);
            ButtonLeft.Name = "ButtonLeft";
            ButtonLeft.Size = new System.Drawing.Size(75, 23);
            ButtonLeft.TabIndex = 0;
            ButtonLeft.Text = "Left";
            ButtonLeft.UseVisualStyleBackColor = true;
            ButtonLeft.Click += ButtonLeft_Click;
            // 
            // ButtonRight
            // 
            ButtonRight.Location = new System.Drawing.Point(87, 145);
            ButtonRight.Name = "ButtonRight";
            ButtonRight.Size = new System.Drawing.Size(75, 23);
            ButtonRight.TabIndex = 1;
            ButtonRight.Text = "Right";
            ButtonRight.UseVisualStyleBackColor = true;
            ButtonRight.Click += ButtonRight_Click;
            // 
            // pictureBoxArtworkGallery
            // 
            pictureBoxArtworkGallery.ContextMenuStrip = contextMenuStripArtworkGallery;
            pictureBoxArtworkGallery.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxArtworkGallery.Location = new System.Drawing.Point(0, 0);
            pictureBoxArtworkGallery.Name = "pictureBoxArtworkGallery";
            pictureBoxArtworkGallery.Size = new System.Drawing.Size(352, 450);
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
            loadClipboradToolStripMenuItem.Text = "Load Clipborad";
            loadClipboradToolStripMenuItem.Click += loadClipboradToolStripMenuItem_Click;
            // 
            // loadGifToolStripMenuItem
            // 
            loadGifToolStripMenuItem.Image = Properties.Resources.Animate;
            loadGifToolStripMenuItem.Name = "loadGifToolStripMenuItem";
            loadGifToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            loadGifToolStripMenuItem.Text = "Load Gif";
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
            // ArtworkGallery
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(splitContainerArtworkGallery);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ArtworkGallery";
            Text = "ArtworkGallery";
            KeyDown += ArtworkGallery_KeyDown;
            KeyUp += ArtworkGallery_KeyUp;
            splitContainerArtworkGallery.Panel1.ResumeLayout(false);
            splitContainerArtworkGallery.Panel1.PerformLayout();
            splitContainerArtworkGallery.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerArtworkGallery).EndInit();
            splitContainerArtworkGallery.ResumeLayout(false);
            groupBoxCrop.ResumeLayout(false);
            groupBoxCrop.PerformLayout();
            groupBoxArtworkGallery.ResumeLayout(false);
            groupBoxArtworkGallery.PerformLayout();
            contextMenuStripListbox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxArtworkGallery).EndInit();
            contextMenuStripArtworkGallery.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerArtworkGallery;
        private System.Windows.Forms.PictureBox pictureBoxArtworkGallery;
        private System.Windows.Forms.GroupBox groupBoxArtworkGallery;
        private System.Windows.Forms.Button ButtonLeft;
        private System.Windows.Forms.Button ButtonRight;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripArtworkGallery;
        private System.Windows.Forms.ToolStripMenuItem loadSecondImageToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxSecondArtwork;
        private System.Windows.Forms.Button ButtonDrawRhombus;
        private System.Windows.Forms.ToolStripMenuItem loadClipboradToolStripMenuItem;
        private System.Windows.Forms.Label label1ImageInfo;
        private System.Windows.Forms.ToolStripMenuItem loadGifToolStripMenuItem;
        private System.Windows.Forms.Button ButtonClearAll_Click;
        private System.Windows.Forms.Button ButtonRemoveSecondImage;
        private System.Windows.Forms.Button ButtonRemoveGif;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorArtworkGallery;
        private System.Windows.Forms.GroupBox groupBoxCrop;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.TextBox textBoxHeight;
        private System.Windows.Forms.TextBox textBoxWidth;
        private System.Windows.Forms.Button ButtonCrop;
        private System.Windows.Forms.ToolStripMenuItem centerOverlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flipVerticalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FlipHorizontalToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxCuttingTemplate;
        private System.Windows.Forms.CheckBox checkBoxBlack;
        private System.Windows.Forms.CheckBox checkBoxWhite;
        private System.Windows.Forms.Button ButtonCopyCuttingTemplateToClipboard;
        private System.Windows.Forms.Button ButtonSaveCuttingTemplate;
        private System.Windows.Forms.Button ButtonShowHideIndexedImages;
        private System.Windows.Forms.Button ButtonBackgroundImage;
        private System.Windows.Forms.Button ButtonLoadImages;
        private System.Windows.Forms.ListBox listBoxImages;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripListbox;
        private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxSearchTexbox;
    }
}
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
            ButtonCrop = new System.Windows.Forms.Button();
            labelWidth = new System.Windows.Forms.Label();
            labelHeight = new System.Windows.Forms.Label();
            textBoxHeight = new System.Windows.Forms.TextBox();
            textBoxWidth = new System.Windows.Forms.TextBox();
            label1ImageInfo = new System.Windows.Forms.Label();
            groupBoxArtworkGallery = new System.Windows.Forms.GroupBox();
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
            ((System.ComponentModel.ISupportInitialize)splitContainerArtworkGallery).BeginInit();
            splitContainerArtworkGallery.Panel1.SuspendLayout();
            splitContainerArtworkGallery.Panel2.SuspendLayout();
            splitContainerArtworkGallery.SuspendLayout();
            groupBoxCrop.SuspendLayout();
            groupBoxArtworkGallery.SuspendLayout();
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
            groupBoxCrop.Controls.Add(ButtonCrop);
            groupBoxCrop.Controls.Add(labelWidth);
            groupBoxCrop.Controls.Add(labelHeight);
            groupBoxCrop.Controls.Add(textBoxHeight);
            groupBoxCrop.Controls.Add(textBoxWidth);
            groupBoxCrop.Location = new System.Drawing.Point(12, 231);
            groupBoxCrop.Name = "groupBoxCrop";
            groupBoxCrop.Size = new System.Drawing.Size(414, 84);
            groupBoxCrop.TabIndex = 5;
            groupBoxCrop.TabStop = false;
            groupBoxCrop.Text = "Crop Second Image";
            // 
            // ButtonCrop
            // 
            ButtonCrop.Location = new System.Drawing.Point(165, 43);
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
            // 
            // textBoxWidth
            // 
            textBoxWidth.Location = new System.Drawing.Point(9, 44);
            textBoxWidth.Name = "textBoxWidth";
            textBoxWidth.Size = new System.Drawing.Size(63, 23);
            textBoxWidth.TabIndex = 0;
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
            groupBoxArtworkGallery.Controls.Add(ButtonClearAll_Click);
            groupBoxArtworkGallery.Controls.Add(ButtonRemoveSecondImage);
            groupBoxArtworkGallery.Controls.Add(ButtonRemoveGif);
            groupBoxArtworkGallery.Controls.Add(ButtonDrawRhombus);
            groupBoxArtworkGallery.Controls.Add(checkBoxSecondArtwork);
            groupBoxArtworkGallery.Controls.Add(ButtonLeft);
            groupBoxArtworkGallery.Controls.Add(ButtonRight);
            groupBoxArtworkGallery.Location = new System.Drawing.Point(12, 12);
            groupBoxArtworkGallery.Name = "groupBoxArtworkGallery";
            groupBoxArtworkGallery.Size = new System.Drawing.Size(414, 174);
            groupBoxArtworkGallery.TabIndex = 2;
            groupBoxArtworkGallery.TabStop = false;
            groupBoxArtworkGallery.Text = "Control";
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
            contextMenuStripArtworkGallery.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadSecondImageToolStripMenuItem, loadClipboradToolStripMenuItem, loadGifToolStripMenuItem, toolStripSeparatorArtworkGallery });
            contextMenuStripArtworkGallery.Name = "contextMenuStripArtworkGallery";
            contextMenuStripArtworkGallery.Size = new System.Drawing.Size(179, 76);
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
    }
}
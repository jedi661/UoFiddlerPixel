// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class BildFusionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BildFusionForm));
            pictureBox64x64 = new System.Windows.Forms.PictureBox();
            pictureBox128x128 = new System.Windows.Forms.PictureBox();
            pictureBox256x256 = new System.Windows.Forms.PictureBox();
            comboBoxRubberStamp = new System.Windows.Forms.ComboBox();
            panelPixturebox = new System.Windows.Forms.Panel();
            lb256x256 = new System.Windows.Forms.Label();
            lb128x128 = new System.Windows.Forms.Label();
            lb64x64 = new System.Windows.Forms.Label();
            panelMenu = new System.Windows.Forms.Panel();
            btViewLoadBackground = new System.Windows.Forms.Button();
            lb2to1Load = new System.Windows.Forms.Label();
            lbSingleLoad = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            lbBackgroundImage = new System.Windows.Forms.Label();
            lbForegroundImage = new System.Windows.Forms.Label();
            btBackgroundImageLoad = new System.Windows.Forms.Button();
            btMirror = new System.Windows.Forms.Button();
            lbDir = new System.Windows.Forms.Label();
            tbDirBackgroundImage = new System.Windows.Forms.TextBox();
            comboBoxBackgroundImage = new System.Windows.Forms.ComboBox();
            btViewLoad = new System.Windows.Forms.Button();
            tbFileDir = new System.Windows.Forms.TextBox();
            btLoadRubberStamp = new System.Windows.Forms.Button();
            checkBox256x256 = new System.Windows.Forms.CheckBox();
            checkBox128x128 = new System.Windows.Forms.CheckBox();
            checkBox64x64 = new System.Windows.Forms.CheckBox();
            btLoadSingleForeground = new System.Windows.Forms.Button();
            btLoadSingleBackground = new System.Windows.Forms.Button();
            lbRotate = new System.Windows.Forms.Label();
            lbForeground = new System.Windows.Forms.Label();
            lbbackground = new System.Windows.Forms.Label();
            btLeftOverlayImage = new System.Windows.Forms.Button();
            btRightOverlayImage = new System.Windows.Forms.Button();
            btLeftBackgroundImage = new System.Windows.Forms.Button();
            btRightBackgroundImage = new System.Windows.Forms.Button();
            btSave64x64 = new System.Windows.Forms.Button();
            btLoad = new System.Windows.Forms.Button();
            trackBarFading = new System.Windows.Forms.TrackBar();
            trackBarColor = new System.Windows.Forms.TrackBar();
            trackBarLabel = new System.Windows.Forms.Label();
            lbNr = new System.Windows.Forms.Label();
            lbColors = new System.Windows.Forms.Label();
            lbFading = new System.Windows.Forms.Label();
            btDirSaveOrder = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox64x64).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox128x128).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox256x256).BeginInit();
            panelPixturebox.SuspendLayout();
            panelMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarFading).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarColor).BeginInit();
            SuspendLayout();
            // 
            // pictureBox64x64
            // 
            pictureBox64x64.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox64x64.Location = new System.Drawing.Point(24, 35);
            pictureBox64x64.Name = "pictureBox64x64";
            pictureBox64x64.Size = new System.Drawing.Size(64, 64);
            pictureBox64x64.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox64x64.TabIndex = 0;
            pictureBox64x64.TabStop = false;
            // 
            // pictureBox128x128
            // 
            pictureBox128x128.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox128x128.Location = new System.Drawing.Point(106, 35);
            pictureBox128x128.Name = "pictureBox128x128";
            pictureBox128x128.Size = new System.Drawing.Size(128, 128);
            pictureBox128x128.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox128x128.TabIndex = 1;
            pictureBox128x128.TabStop = false;
            // 
            // pictureBox256x256
            // 
            pictureBox256x256.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBox256x256.Location = new System.Drawing.Point(258, 35);
            pictureBox256x256.Name = "pictureBox256x256";
            pictureBox256x256.Size = new System.Drawing.Size(256, 256);
            pictureBox256x256.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox256x256.TabIndex = 2;
            pictureBox256x256.TabStop = false;
            // 
            // comboBoxRubberStamp
            // 
            comboBoxRubberStamp.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBoxRubberStamp.FormattingEnabled = true;
            comboBoxRubberStamp.Location = new System.Drawing.Point(60, 158);
            comboBoxRubberStamp.Name = "comboBoxRubberStamp";
            comboBoxRubberStamp.Size = new System.Drawing.Size(125, 24);
            comboBoxRubberStamp.TabIndex = 5;
            comboBoxRubberStamp.DrawItem += comboBoxRubberStamp_DrawItem;
            comboBoxRubberStamp.SelectedIndexChanged += comboBoxRubberStamp_SelectedIndexChanged;
            // 
            // panelPixturebox
            // 
            panelPixturebox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelPixturebox.Controls.Add(lb256x256);
            panelPixturebox.Controls.Add(lb128x128);
            panelPixturebox.Controls.Add(lb64x64);
            panelPixturebox.Controls.Add(pictureBox256x256);
            panelPixturebox.Controls.Add(pictureBox64x64);
            panelPixturebox.Controls.Add(pictureBox128x128);
            panelPixturebox.Location = new System.Drawing.Point(236, 79);
            panelPixturebox.Name = "panelPixturebox";
            panelPixturebox.Size = new System.Drawing.Size(552, 343);
            panelPixturebox.TabIndex = 6;
            // 
            // lb256x256
            // 
            lb256x256.AutoSize = true;
            lb256x256.Location = new System.Drawing.Point(258, 17);
            lb256x256.Name = "lb256x256";
            lb256x256.Size = new System.Drawing.Size(49, 15);
            lb256x256.TabIndex = 5;
            lb256x256.Text = "256x256";
            // 
            // lb128x128
            // 
            lb128x128.AutoSize = true;
            lb128x128.Location = new System.Drawing.Point(106, 17);
            lb128x128.Name = "lb128x128";
            lb128x128.Size = new System.Drawing.Size(49, 15);
            lb128x128.TabIndex = 4;
            lb128x128.Text = "128x128";
            // 
            // lb64x64
            // 
            lb64x64.AutoSize = true;
            lb64x64.Location = new System.Drawing.Point(24, 17);
            lb64x64.Name = "lb64x64";
            lb64x64.Size = new System.Drawing.Size(37, 15);
            lb64x64.TabIndex = 3;
            lb64x64.Text = "64x64";
            // 
            // panelMenu
            // 
            panelMenu.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelMenu.Controls.Add(btDirSaveOrder);
            panelMenu.Controls.Add(btViewLoadBackground);
            panelMenu.Controls.Add(lb2to1Load);
            panelMenu.Controls.Add(lbSingleLoad);
            panelMenu.Controls.Add(label1);
            panelMenu.Controls.Add(lbBackgroundImage);
            panelMenu.Controls.Add(lbForegroundImage);
            panelMenu.Controls.Add(btBackgroundImageLoad);
            panelMenu.Controls.Add(btMirror);
            panelMenu.Controls.Add(lbDir);
            panelMenu.Controls.Add(tbDirBackgroundImage);
            panelMenu.Controls.Add(comboBoxBackgroundImage);
            panelMenu.Controls.Add(btViewLoad);
            panelMenu.Controls.Add(tbFileDir);
            panelMenu.Controls.Add(btLoadRubberStamp);
            panelMenu.Controls.Add(checkBox256x256);
            panelMenu.Controls.Add(checkBox128x128);
            panelMenu.Controls.Add(checkBox64x64);
            panelMenu.Controls.Add(btLoadSingleForeground);
            panelMenu.Controls.Add(btLoadSingleBackground);
            panelMenu.Controls.Add(lbRotate);
            panelMenu.Controls.Add(lbForeground);
            panelMenu.Controls.Add(lbbackground);
            panelMenu.Controls.Add(btLeftOverlayImage);
            panelMenu.Controls.Add(btRightOverlayImage);
            panelMenu.Controls.Add(btLeftBackgroundImage);
            panelMenu.Controls.Add(btRightBackgroundImage);
            panelMenu.Controls.Add(btSave64x64);
            panelMenu.Controls.Add(btLoad);
            panelMenu.Controls.Add(comboBoxRubberStamp);
            panelMenu.Location = new System.Drawing.Point(12, 12);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new System.Drawing.Size(218, 410);
            panelMenu.TabIndex = 7;
            // 
            // btViewLoadBackground
            // 
            btViewLoadBackground.Location = new System.Drawing.Point(11, 45);
            btViewLoadBackground.Name = "btViewLoadBackground";
            btViewLoadBackground.Size = new System.Drawing.Size(43, 23);
            btViewLoadBackground.TabIndex = 32;
            btViewLoadBackground.Text = "view";
            btViewLoadBackground.UseVisualStyleBackColor = true;
            btViewLoadBackground.Click += btViewLoadBackground_Click;
            // 
            // lb2to1Load
            // 
            lb2to1Load.AutoSize = true;
            lb2to1Load.Location = new System.Drawing.Point(11, 299);
            lb2to1Load.Name = "lb2to1Load";
            lb2to1Load.Size = new System.Drawing.Size(31, 15);
            lb2to1Load.TabIndex = 31;
            lb2to1Load.Text = "2 x 1";
            // 
            // lbSingleLoad
            // 
            lbSingleLoad.AutoSize = true;
            lbSingleLoad.Location = new System.Drawing.Point(25, 223);
            lbSingleLoad.Name = "lbSingleLoad";
            lbSingleLoad.Size = new System.Drawing.Size(74, 15);
            lbSingleLoad.TabIndex = 30;
            lbSingleLoad.Text = "Single Load :";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 110);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(61, 15);
            label1.TabIndex = 29;
            label1.Text = "Directory :";
            // 
            // lbBackgroundImage
            // 
            lbBackgroundImage.AutoSize = true;
            lbBackgroundImage.Location = new System.Drawing.Point(63, 27);
            lbBackgroundImage.Name = "lbBackgroundImage";
            lbBackgroundImage.Size = new System.Drawing.Size(107, 15);
            lbBackgroundImage.TabIndex = 28;
            lbBackgroundImage.Text = "Background Image";
            // 
            // lbForegroundImage
            // 
            lbForegroundImage.AutoSize = true;
            lbForegroundImage.Location = new System.Drawing.Point(63, 111);
            lbForegroundImage.Name = "lbForegroundImage";
            lbForegroundImage.Size = new System.Drawing.Size(105, 15);
            lbForegroundImage.TabIndex = 27;
            lbForegroundImage.Text = "Foreground image";
            // 
            // btBackgroundImageLoad
            // 
            btBackgroundImageLoad.Location = new System.Drawing.Point(10, 70);
            btBackgroundImageLoad.Name = "btBackgroundImageLoad";
            btBackgroundImageLoad.Size = new System.Drawing.Size(47, 23);
            btBackgroundImageLoad.TabIndex = 26;
            btBackgroundImageLoad.Text = "Dir";
            btBackgroundImageLoad.UseVisualStyleBackColor = true;
            btBackgroundImageLoad.Click += btBackgroundImageLoad_Click;
            // 
            // btMirror
            // 
            btMirror.Location = new System.Drawing.Point(11, 375);
            btMirror.Name = "btMirror";
            btMirror.Size = new System.Drawing.Size(84, 23);
            btMirror.TabIndex = 23;
            btMirror.Text = "Mirror";
            btMirror.UseVisualStyleBackColor = true;
            btMirror.Click += btMirror_Click;
            // 
            // lbDir
            // 
            lbDir.AutoSize = true;
            lbDir.Location = new System.Drawing.Point(3, 27);
            lbDir.Name = "lbDir";
            lbDir.Size = new System.Drawing.Size(61, 15);
            lbDir.TabIndex = 22;
            lbDir.Text = "Directory :";
            // 
            // tbDirBackgroundImage
            // 
            tbDirBackgroundImage.Location = new System.Drawing.Point(63, 45);
            tbDirBackgroundImage.Name = "tbDirBackgroundImage";
            tbDirBackgroundImage.Size = new System.Drawing.Size(125, 23);
            tbDirBackgroundImage.TabIndex = 25;
            // 
            // comboBoxBackgroundImage
            // 
            comboBoxBackgroundImage.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBoxBackgroundImage.FormattingEnabled = true;
            comboBoxBackgroundImage.Location = new System.Drawing.Point(63, 71);
            comboBoxBackgroundImage.Name = "comboBoxBackgroundImage";
            comboBoxBackgroundImage.Size = new System.Drawing.Size(125, 24);
            comboBoxBackgroundImage.TabIndex = 24;
            comboBoxBackgroundImage.DrawItem += comboBoxBackgroundImage_DrawItem;
            comboBoxBackgroundImage.SelectedIndexChanged += comboBoxBackgroundImage_SelectedIndexChanged;
            // 
            // btViewLoad
            // 
            btViewLoad.Location = new System.Drawing.Point(11, 128);
            btViewLoad.Name = "btViewLoad";
            btViewLoad.Size = new System.Drawing.Size(43, 23);
            btViewLoad.TabIndex = 21;
            btViewLoad.Text = "view";
            btViewLoad.UseVisualStyleBackColor = true;
            btViewLoad.Click += btViewLoad_Click;
            // 
            // tbFileDir
            // 
            tbFileDir.Location = new System.Drawing.Point(60, 129);
            tbFileDir.Name = "tbFileDir";
            tbFileDir.Size = new System.Drawing.Size(125, 23);
            tbFileDir.TabIndex = 20;
            // 
            // btLoadRubberStamp
            // 
            btLoadRubberStamp.Location = new System.Drawing.Point(11, 157);
            btLoadRubberStamp.Name = "btLoadRubberStamp";
            btLoadRubberStamp.Size = new System.Drawing.Size(43, 23);
            btLoadRubberStamp.TabIndex = 19;
            btLoadRubberStamp.Text = "Dir";
            btLoadRubberStamp.UseVisualStyleBackColor = true;
            btLoadRubberStamp.Click += btLoadRubberStamp_Click;
            // 
            // checkBox256x256
            // 
            checkBox256x256.AutoSize = true;
            checkBox256x256.Location = new System.Drawing.Point(120, 266);
            checkBox256x256.Name = "checkBox256x256";
            checkBox256x256.Size = new System.Drawing.Size(68, 19);
            checkBox256x256.TabIndex = 18;
            checkBox256x256.Text = "256x256";
            checkBox256x256.UseVisualStyleBackColor = true;
            checkBox256x256.CheckedChanged += checkBox256x256_CheckedChanged;
            // 
            // checkBox128x128
            // 
            checkBox128x128.AutoSize = true;
            checkBox128x128.Location = new System.Drawing.Point(120, 241);
            checkBox128x128.Name = "checkBox128x128";
            checkBox128x128.Size = new System.Drawing.Size(68, 19);
            checkBox128x128.TabIndex = 17;
            checkBox128x128.Text = "128x128";
            checkBox128x128.UseVisualStyleBackColor = true;
            checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;
            // 
            // checkBox64x64
            // 
            checkBox64x64.AutoSize = true;
            checkBox64x64.Checked = true;
            checkBox64x64.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBox64x64.Location = new System.Drawing.Point(120, 215);
            checkBox64x64.Name = "checkBox64x64";
            checkBox64x64.Size = new System.Drawing.Size(56, 19);
            checkBox64x64.TabIndex = 16;
            checkBox64x64.Text = "64x64";
            checkBox64x64.UseVisualStyleBackColor = true;
            checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
            // 
            // btLoadSingleForeground
            // 
            btLoadSingleForeground.Location = new System.Drawing.Point(11, 270);
            btLoadSingleForeground.Name = "btLoadSingleForeground";
            btLoadSingleForeground.Size = new System.Drawing.Size(84, 23);
            btLoadSingleForeground.TabIndex = 15;
            btLoadSingleForeground.Text = "Foreground";
            btLoadSingleForeground.UseVisualStyleBackColor = true;
            btLoadSingleForeground.Click += btLoadSingleForeground_Click;
            // 
            // btLoadSingleBackground
            // 
            btLoadSingleBackground.Location = new System.Drawing.Point(11, 241);
            btLoadSingleBackground.Name = "btLoadSingleBackground";
            btLoadSingleBackground.Size = new System.Drawing.Size(84, 23);
            btLoadSingleBackground.TabIndex = 14;
            btLoadSingleBackground.Text = "Background";
            btLoadSingleBackground.UseVisualStyleBackColor = true;
            btLoadSingleBackground.Click += btLoadSingleBackground_Click;
            // 
            // lbRotate
            // 
            lbRotate.AutoSize = true;
            lbRotate.Location = new System.Drawing.Point(108, 299);
            lbRotate.Name = "lbRotate";
            lbRotate.Size = new System.Drawing.Size(77, 15);
            lbRotate.TabIndex = 13;
            lbRotate.Text = "Rotate Image";
            // 
            // lbForeground
            // 
            lbForeground.AutoSize = true;
            lbForeground.Location = new System.Drawing.Point(28, 350);
            lbForeground.Name = "lbForeground";
            lbForeground.Size = new System.Drawing.Size(75, 15);
            lbForeground.TabIndex = 12;
            lbForeground.Text = "Foreground :";
            // 
            // lbbackground
            // 
            lbbackground.AutoSize = true;
            lbbackground.Location = new System.Drawing.Point(28, 321);
            lbbackground.Name = "lbbackground";
            lbbackground.Size = new System.Drawing.Size(77, 15);
            lbbackground.TabIndex = 11;
            lbbackground.Text = "Background :";
            // 
            // btLeftOverlayImage
            // 
            btLeftOverlayImage.Location = new System.Drawing.Point(108, 346);
            btLeftOverlayImage.Name = "btLeftOverlayImage";
            btLeftOverlayImage.Size = new System.Drawing.Size(46, 23);
            btLeftOverlayImage.TabIndex = 10;
            btLeftOverlayImage.Text = "Left";
            btLeftOverlayImage.UseVisualStyleBackColor = true;
            btLeftOverlayImage.Click += btLeftOverlayImage_Click;
            // 
            // btRightOverlayImage
            // 
            btRightOverlayImage.Location = new System.Drawing.Point(160, 346);
            btRightOverlayImage.Name = "btRightOverlayImage";
            btRightOverlayImage.Size = new System.Drawing.Size(43, 23);
            btRightOverlayImage.TabIndex = 9;
            btRightOverlayImage.Text = "Right";
            btRightOverlayImage.UseVisualStyleBackColor = true;
            btRightOverlayImage.Click += btRightOverlayImage_Click;
            // 
            // btLeftBackgroundImage
            // 
            btLeftBackgroundImage.Location = new System.Drawing.Point(108, 317);
            btLeftBackgroundImage.Name = "btLeftBackgroundImage";
            btLeftBackgroundImage.Size = new System.Drawing.Size(46, 23);
            btLeftBackgroundImage.TabIndex = 8;
            btLeftBackgroundImage.Text = "Left";
            btLeftBackgroundImage.UseVisualStyleBackColor = true;
            btLeftBackgroundImage.Click += btLeftBackgroundImage_Click;
            // 
            // btRightBackgroundImage
            // 
            btRightBackgroundImage.Location = new System.Drawing.Point(160, 317);
            btRightBackgroundImage.Name = "btRightBackgroundImage";
            btRightBackgroundImage.Size = new System.Drawing.Size(43, 23);
            btRightBackgroundImage.TabIndex = 7;
            btRightBackgroundImage.Text = "Right";
            btRightBackgroundImage.UseVisualStyleBackColor = true;
            btRightBackgroundImage.Click += btRightBackgroundImage_Click;
            // 
            // btSave64x64
            // 
            btSave64x64.Location = new System.Drawing.Point(160, 375);
            btSave64x64.Name = "btSave64x64";
            btSave64x64.Size = new System.Drawing.Size(43, 23);
            btSave64x64.TabIndex = 3;
            btSave64x64.Text = "Save";
            btSave64x64.UseVisualStyleBackColor = true;
            btSave64x64.Click += btSave_Click;
            // 
            // btLoad
            // 
            btLoad.Location = new System.Drawing.Point(52, 295);
            btLoad.Name = "btLoad";
            btLoad.Size = new System.Drawing.Size(43, 23);
            btLoad.TabIndex = 6;
            btLoad.Text = "Load";
            btLoad.UseVisualStyleBackColor = true;
            btLoad.Click += btLoad_Click;
            // 
            // trackBarFading
            // 
            trackBarFading.Location = new System.Drawing.Point(236, 17);
            trackBarFading.Maximum = 255;
            trackBarFading.Name = "trackBarFading";
            trackBarFading.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            trackBarFading.Size = new System.Drawing.Size(104, 45);
            trackBarFading.TabIndex = 8;
            trackBarFading.TickStyle = System.Windows.Forms.TickStyle.Both;
            trackBarFading.Scroll += trackBarFading_Scroll;
            // 
            // trackBarColor
            // 
            trackBarColor.Location = new System.Drawing.Point(367, 17);
            trackBarColor.Maximum = 255;
            trackBarColor.Name = "trackBarColor";
            trackBarColor.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            trackBarColor.Size = new System.Drawing.Size(104, 45);
            trackBarColor.TabIndex = 9;
            trackBarColor.TickStyle = System.Windows.Forms.TickStyle.Both;
            trackBarColor.Scroll += trackBarColor_Scroll;
            // 
            // trackBarLabel
            // 
            trackBarLabel.AutoSize = true;
            trackBarLabel.Location = new System.Drawing.Point(367, 60);
            trackBarLabel.Name = "trackBarLabel";
            trackBarLabel.Size = new System.Drawing.Size(39, 15);
            trackBarLabel.TabIndex = 10;
            trackBarLabel.Text = "Color:";
            // 
            // lbNr
            // 
            lbNr.AutoSize = true;
            lbNr.Location = new System.Drawing.Point(337, 30);
            lbNr.Name = "lbNr";
            lbNr.Size = new System.Drawing.Size(26, 15);
            lbNr.TabIndex = 11;
            lbNr.Text = "Pos";
            // 
            // lbColors
            // 
            lbColors.AutoSize = true;
            lbColors.Location = new System.Drawing.Point(367, 4);
            lbColors.Name = "lbColors";
            lbColors.Size = new System.Drawing.Size(47, 15);
            lbColors.TabIndex = 12;
            lbColors.Text = "Colors :";
            // 
            // lbFading
            // 
            lbFading.AutoSize = true;
            lbFading.Location = new System.Drawing.Point(243, 4);
            lbFading.Name = "lbFading";
            lbFading.Size = new System.Drawing.Size(49, 15);
            lbFading.TabIndex = 13;
            lbFading.Text = "Fading :";
            // 
            // btDirSaveOrder
            // 
            btDirSaveOrder.Location = new System.Drawing.Point(108, 376);
            btDirSaveOrder.Name = "btDirSaveOrder";
            btDirSaveOrder.Size = new System.Drawing.Size(46, 23);
            btDirSaveOrder.TabIndex = 33;
            btDirSaveOrder.Text = "Dir";
            btDirSaveOrder.UseVisualStyleBackColor = true;
            btDirSaveOrder.Click += btDirSaveOrder_Click;
            // 
            // BildFusionForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(794, 434);
            Controls.Add(lbFading);
            Controls.Add(lbColors);
            Controls.Add(lbNr);
            Controls.Add(trackBarLabel);
            Controls.Add(trackBarColor);
            Controls.Add(trackBarFading);
            Controls.Add(panelMenu);
            Controls.Add(panelPixturebox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "BildFusionForm";
            Text = "Texture Picture Fusion";
            ((System.ComponentModel.ISupportInitialize)pictureBox64x64).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox128x128).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox256x256).EndInit();
            panelPixturebox.ResumeLayout(false);
            panelPixturebox.PerformLayout();
            panelMenu.ResumeLayout(false);
            panelMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarFading).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarColor).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox64x64;
        private System.Windows.Forms.PictureBox pictureBox128x128;
        private System.Windows.Forms.PictureBox pictureBox256x256;
        private System.Windows.Forms.ComboBox comboBoxRubberStamp;
        private System.Windows.Forms.Panel panelPixturebox;
        private System.Windows.Forms.Panel panelMenu;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.Button btSave64x64;
        private System.Windows.Forms.Button btLeftBackgroundImage;
        private System.Windows.Forms.Button btRightBackgroundImage;
        private System.Windows.Forms.Button btLeftOverlayImage;
        private System.Windows.Forms.Button btRightOverlayImage;
        private System.Windows.Forms.Label lbRotate;
        private System.Windows.Forms.Label lbForeground;
        private System.Windows.Forms.Label lbbackground;
        private System.Windows.Forms.Button btLoadSingleForeground;
        private System.Windows.Forms.Button btLoadSingleBackground;
        private System.Windows.Forms.CheckBox checkBox256x256;
        private System.Windows.Forms.CheckBox checkBox128x128;
        private System.Windows.Forms.CheckBox checkBox64x64;
        private System.Windows.Forms.TextBox tbFileDir;
        private System.Windows.Forms.Button btLoadRubberStamp;
        private System.Windows.Forms.Button btViewLoad;
        private System.Windows.Forms.Label lbDir;
        private System.Windows.Forms.TrackBar trackBarFading;
        private System.Windows.Forms.TrackBar trackBarColor;
        private System.Windows.Forms.Label trackBarLabel;
        private System.Windows.Forms.Label lbNr;
        private System.Windows.Forms.Button btMirror;
        private System.Windows.Forms.Label lb256x256;
        private System.Windows.Forms.Label lb128x128;
        private System.Windows.Forms.Label lb64x64;
        private System.Windows.Forms.Label lbColors;
        private System.Windows.Forms.Label lbFading;
        private System.Windows.Forms.ComboBox comboBoxBackgroundImage;
        private System.Windows.Forms.TextBox tbDirBackgroundImage;
        private System.Windows.Forms.Button btBackgroundImageLoad;
        private System.Windows.Forms.Label lbBackgroundImage;
        private System.Windows.Forms.Label lbForegroundImage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbSingleLoad;
        private System.Windows.Forms.Label lb2to1Load;
        private System.Windows.Forms.Button btViewLoadBackground;
        private System.Windows.Forms.Button btDirSaveOrder;
    }
}
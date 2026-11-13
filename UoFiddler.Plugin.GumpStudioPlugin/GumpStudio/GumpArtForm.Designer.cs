namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpArtForm
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
            if (disposing) { _picture?.Dispose(); _frame?.Dispose(); picturePicture.Image = null; pictureSource.Image = null; pictureFramePreview.Image?.Dispose(); }
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
            tabMain = new System.Windows.Forms.TabControl();
            tabPicture = new System.Windows.Forms.TabPage();
            tabFrame = new System.Windows.Forms.TabPage();
            picturePicture = new System.Windows.Forms.PictureBox();
            flowPicture = new System.Windows.Forms.FlowLayoutPanel();
            btnLoadPicture = new System.Windows.Forms.Button();
            lblPictureId = new System.Windows.Forms.Label();
            numPictureId = new System.Windows.Forms.NumericUpDown();
            btnFreePicture = new System.Windows.Forms.Button();
            chkBlackTransparent = new System.Windows.Forms.CheckBox();
            splitFrame = new System.Windows.Forms.SplitContainer();
            pictureSource = new System.Windows.Forms.PictureBox();
            pictureFramePreview = new System.Windows.Forms.PictureBox();
            flowFrame = new System.Windows.Forms.FlowLayoutPanel();
            btnLoadFrame = new System.Windows.Forms.Button();
            lblLeft = new System.Windows.Forms.Label();
            numLeft = new System.Windows.Forms.NumericUpDown();
            lblTop = new System.Windows.Forms.Label();
            numTop = new System.Windows.Forms.NumericUpDown();
            lblRight = new System.Windows.Forms.Label();
            numRight = new System.Windows.Forms.NumericUpDown();
            lblBottom = new System.Windows.Forms.Label();
            numBottom = new System.Windows.Forms.NumericUpDown();
            lblFrameId = new System.Windows.Forms.Label();
            numFrameId = new System.Windows.Forms.NumericUpDown();
            btnFreeFrame = new System.Windows.Forms.Button();
            lblPreviewSize = new System.Windows.Forms.Label();
            numPreviewWidth = new System.Windows.Forms.NumericUpDown();
            numPreviewHeight = new System.Windows.Forms.NumericUpDown();
            chkFrameBlack = new System.Windows.Forms.CheckBox();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnInsert = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            tabMain.SuspendLayout();
            tabPicture.SuspendLayout();
            tabFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePicture).BeginInit();
            flowPicture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPictureId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitFrame).BeginInit();
            splitFrame.Panel1.SuspendLayout();
            splitFrame.Panel2.SuspendLayout();
            splitFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureFramePreview).BeginInit();
            flowFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numLeft).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTop).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBottom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFrameId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPreviewWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPreviewHeight).BeginInit();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // tabMain
            //
            tabMain.Controls.Add(tabPicture);
            tabMain.Controls.Add(tabFrame);
            tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tabMain.Name = "tabMain";
            tabMain.TabIndex = 0;
            tabMain.SelectedIndexChanged += Option_Changed;
            //
            // tabPicture
            //
            tabPicture.Controls.Add(picturePicture);
            tabPicture.Controls.Add(flowPicture);
            tabPicture.Padding = new System.Windows.Forms.Padding(3);
            tabPicture.Text = "Picture → gump";
            tabPicture.UseVisualStyleBackColor = true;
            tabPicture.Name = "tabPicture";
            tabPicture.TabIndex = 1;
            //
            // tabFrame
            //
            tabFrame.Controls.Add(splitFrame);
            tabFrame.Controls.Add(flowFrame);
            tabFrame.Padding = new System.Windows.Forms.Padding(3);
            tabFrame.Text = "Frame generator (9 parts)";
            tabFrame.UseVisualStyleBackColor = true;
            tabFrame.Name = "tabFrame";
            tabFrame.TabIndex = 2;
            //
            // picturePicture
            //
            picturePicture.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePicture.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePicture.TabStop = false;
            picturePicture.Name = "picturePicture";
            picturePicture.TabIndex = 3;
            //
            // flowPicture
            //
            flowPicture.Controls.Add(btnLoadPicture);
            flowPicture.Controls.Add(lblPictureId);
            flowPicture.Controls.Add(numPictureId);
            flowPicture.Controls.Add(btnFreePicture);
            flowPicture.Controls.Add(chkBlackTransparent);
            flowPicture.AutoSize = true;
            flowPicture.Dock = System.Windows.Forms.DockStyle.Top;
            flowPicture.Padding = new System.Windows.Forms.Padding(4);
            flowPicture.Name = "flowPicture";
            flowPicture.TabIndex = 4;
            //
            // btnLoadPicture
            //
            btnLoadPicture.AutoSize = true;
            btnLoadPicture.Text = "Load picture…";
            btnLoadPicture.UseVisualStyleBackColor = true;
            btnLoadPicture.Name = "btnLoadPicture";
            btnLoadPicture.TabIndex = 5;
            btnLoadPicture.Click += BtnLoadPicture_Click;
            //
            // lblPictureId
            //
            lblPictureId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPictureId.AutoSize = true;
            lblPictureId.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblPictureId.Text = "Gump ID 0x";
            lblPictureId.Name = "lblPictureId";
            lblPictureId.TabIndex = 6;
            //
            // numPictureId
            //
            numPictureId.Maximum = new decimal(new int[] { 1048575, 0, 0, 0 });
            numPictureId.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numPictureId.Size = new System.Drawing.Size(80, 23);
            numPictureId.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numPictureId.Hexadecimal = true;
            numPictureId.Name = "numPictureId";
            numPictureId.TabIndex = 7;
            numPictureId.ValueChanged += Option_Changed;
            //
            // btnFreePicture
            //
            btnFreePicture.AutoSize = true;
            btnFreePicture.Text = "Next free";
            btnFreePicture.UseVisualStyleBackColor = true;
            btnFreePicture.Name = "btnFreePicture";
            btnFreePicture.TabIndex = 8;
            toolTip1.SetToolTip(btnFreePicture, "Finds the next gump ID that is not used\n\nDE: Sucht die nächste freie Gump-ID");
            btnFreePicture.Click += BtnFreePicture_Click;
            //
            // chkBlackTransparent
            //
            chkBlackTransparent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkBlackTransparent.AutoSize = true;
            chkBlackTransparent.Text = "Black = transparent";
            chkBlackTransparent.Checked = true;
            chkBlackTransparent.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBlackTransparent.Name = "chkBlackTransparent";
            chkBlackTransparent.TabIndex = 9;
            toolTip1.SetToolTip(chkBlackTransparent, "Pure black pixels become transparent (in UO gumps color 0 is see-through)\n\nDE: Rein schwarze Pixel werden durchsichtig (in UO-Gumps ist Farbe 0 durchsichtig)");
            chkBlackTransparent.CheckedChanged += Option_Changed;
            //
            // splitFrame
            //
            splitFrame.Panel1.Controls.Add(pictureSource);
            splitFrame.Panel2.Controls.Add(pictureFramePreview);
            splitFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            splitFrame.Size = new System.Drawing.Size(880, 400);
            splitFrame.SplitterDistance = 430;
            splitFrame.Name = "splitFrame";
            splitFrame.TabIndex = 10;
            //
            // pictureSource
            //
            pictureSource.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            pictureSource.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureSource.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureSource.TabStop = false;
            pictureSource.Name = "pictureSource";
            pictureSource.TabIndex = 11;
            pictureSource.Paint += PictureSource_Paint;
            //
            // pictureFramePreview
            //
            pictureFramePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            pictureFramePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureFramePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureFramePreview.TabStop = false;
            pictureFramePreview.Name = "pictureFramePreview";
            pictureFramePreview.TabIndex = 12;
            //
            // flowFrame
            //
            flowFrame.Controls.Add(btnLoadFrame);
            flowFrame.Controls.Add(lblLeft);
            flowFrame.Controls.Add(numLeft);
            flowFrame.Controls.Add(lblTop);
            flowFrame.Controls.Add(numTop);
            flowFrame.Controls.Add(lblRight);
            flowFrame.Controls.Add(numRight);
            flowFrame.Controls.Add(lblBottom);
            flowFrame.Controls.Add(numBottom);
            flowFrame.Controls.Add(lblFrameId);
            flowFrame.Controls.Add(numFrameId);
            flowFrame.Controls.Add(btnFreeFrame);
            flowFrame.Controls.Add(lblPreviewSize);
            flowFrame.Controls.Add(numPreviewWidth);
            flowFrame.Controls.Add(numPreviewHeight);
            flowFrame.Controls.Add(chkFrameBlack);
            flowFrame.AutoSize = true;
            flowFrame.Dock = System.Windows.Forms.DockStyle.Top;
            flowFrame.Padding = new System.Windows.Forms.Padding(4);
            flowFrame.Name = "flowFrame";
            flowFrame.TabIndex = 13;
            //
            // btnLoadFrame
            //
            btnLoadFrame.AutoSize = true;
            btnLoadFrame.Text = "Load picture…";
            btnLoadFrame.UseVisualStyleBackColor = true;
            btnLoadFrame.Name = "btnLoadFrame";
            btnLoadFrame.TabIndex = 14;
            toolTip1.SetToolTip(btnLoadFrame, "A picture of a whole window / box; the 4 margins cut it into 9 parts\n\nDE: Ein Bild eines ganzen Fensters / Kastens; die 4 Ränder schneiden es in 9 Teile");
            btnLoadFrame.Click += BtnLoadFrame_Click;
            //
            // lblLeft
            //
            lblLeft.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblLeft.AutoSize = true;
            lblLeft.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblLeft.Text = "left:";
            lblLeft.Name = "lblLeft";
            lblLeft.TabIndex = 15;
            //
            // numLeft
            //
            numLeft.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numLeft.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numLeft.Size = new System.Drawing.Size(55, 23);
            numLeft.Value = new decimal(new int[] { 16, 0, 0, 0 });
            numLeft.Name = "numLeft";
            numLeft.TabIndex = 16;
            numLeft.ValueChanged += Option_Changed;
            //
            // lblTop
            //
            lblTop.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblTop.AutoSize = true;
            lblTop.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblTop.Text = "top:";
            lblTop.Name = "lblTop";
            lblTop.TabIndex = 17;
            //
            // numTop
            //
            numTop.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numTop.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTop.Size = new System.Drawing.Size(55, 23);
            numTop.Value = new decimal(new int[] { 16, 0, 0, 0 });
            numTop.Name = "numTop";
            numTop.TabIndex = 18;
            numTop.ValueChanged += Option_Changed;
            //
            // lblRight
            //
            lblRight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblRight.AutoSize = true;
            lblRight.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblRight.Text = "right:";
            lblRight.Name = "lblRight";
            lblRight.TabIndex = 19;
            //
            // numRight
            //
            numRight.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numRight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRight.Size = new System.Drawing.Size(55, 23);
            numRight.Value = new decimal(new int[] { 16, 0, 0, 0 });
            numRight.Name = "numRight";
            numRight.TabIndex = 20;
            numRight.ValueChanged += Option_Changed;
            //
            // lblBottom
            //
            lblBottom.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblBottom.AutoSize = true;
            lblBottom.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblBottom.Text = "bottom:";
            lblBottom.Name = "lblBottom";
            lblBottom.TabIndex = 21;
            //
            // numBottom
            //
            numBottom.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numBottom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numBottom.Size = new System.Drawing.Size(55, 23);
            numBottom.Value = new decimal(new int[] { 16, 0, 0, 0 });
            numBottom.Name = "numBottom";
            numBottom.TabIndex = 22;
            numBottom.ValueChanged += Option_Changed;
            //
            // lblFrameId
            //
            lblFrameId.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFrameId.AutoSize = true;
            lblFrameId.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblFrameId.Text = "first gump ID 0x";
            lblFrameId.Name = "lblFrameId";
            lblFrameId.TabIndex = 23;
            //
            // numFrameId
            //
            numFrameId.Maximum = new decimal(new int[] { 1048575, 0, 0, 0 });
            numFrameId.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numFrameId.Size = new System.Drawing.Size(80, 23);
            numFrameId.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numFrameId.Hexadecimal = true;
            numFrameId.Name = "numFrameId";
            numFrameId.TabIndex = 24;
            toolTip1.SetToolTip(numFrameId, "The frame needs 9 free IDs in a row (ID … ID+8)\n\nDE: Der Rahmen braucht 9 freie IDs am Stück (ID … ID+8)");
            numFrameId.ValueChanged += Option_Changed;
            //
            // btnFreeFrame
            //
            btnFreeFrame.AutoSize = true;
            btnFreeFrame.Text = "Next free";
            btnFreeFrame.UseVisualStyleBackColor = true;
            btnFreeFrame.Name = "btnFreeFrame";
            btnFreeFrame.TabIndex = 25;
            btnFreeFrame.Click += BtnFreeFrame_Click;
            //
            // lblPreviewSize
            //
            lblPreviewSize.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPreviewSize.AutoSize = true;
            lblPreviewSize.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblPreviewSize.Text = "preview size:";
            lblPreviewSize.Name = "lblPreviewSize";
            lblPreviewSize.TabIndex = 26;
            //
            // numPreviewWidth
            //
            numPreviewWidth.Maximum = new decimal(new int[] { 1200, 0, 0, 0 });
            numPreviewWidth.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numPreviewWidth.Size = new System.Drawing.Size(60, 23);
            numPreviewWidth.Value = new decimal(new int[] { 320, 0, 0, 0 });
            numPreviewWidth.Name = "numPreviewWidth";
            numPreviewWidth.TabIndex = 27;
            numPreviewWidth.ValueChanged += Option_Changed;
            //
            // numPreviewHeight
            //
            numPreviewHeight.Maximum = new decimal(new int[] { 1200, 0, 0, 0 });
            numPreviewHeight.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            numPreviewHeight.Size = new System.Drawing.Size(60, 23);
            numPreviewHeight.Value = new decimal(new int[] { 200, 0, 0, 0 });
            numPreviewHeight.Name = "numPreviewHeight";
            numPreviewHeight.TabIndex = 28;
            numPreviewHeight.ValueChanged += Option_Changed;
            //
            // chkFrameBlack
            //
            chkFrameBlack.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkFrameBlack.AutoSize = true;
            chkFrameBlack.Text = "Black = transparent";
            chkFrameBlack.Checked = true;
            chkFrameBlack.CheckState = System.Windows.Forms.CheckState.Checked;
            chkFrameBlack.Name = "chkFrameBlack";
            chkFrameBlack.TabIndex = 29;
            chkFrameBlack.CheckedChanged += Option_Changed;
            //
            // flowButtons
            //
            flowButtons.Controls.Add(btnClose);
            flowButtons.Controls.Add(btnInsert);
            flowButtons.Controls.Add(lblStatus);
            flowButtons.AutoSize = true;
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.Padding = new System.Windows.Forms.Padding(4);
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Name = "flowButtons";
            flowButtons.TabIndex = 30;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 31;
            //
            // btnInsert
            //
            btnInsert.AutoSize = true;
            btnInsert.Text = "Write into gumpart and use it";
            btnInsert.UseVisualStyleBackColor = true;
            btnInsert.Name = "btnInsert";
            btnInsert.TabIndex = 32;
            toolTip1.SetToolTip(btnInsert, "Puts the picture(s) into the gump file in memory – save it in UoFiddler (Gumps tab) to keep it\n\nDE: Setzt das Bild / die Bilder in die Gump-Datei im Speicher – in UoFiddler (Reiter Gumps) speichern, um es zu behalten");
            btnInsert.Click += BtnInsert_Click;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 30, 0);
            lblStatus.Text = "";
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 33;
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            //
            // GumpArtForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(900, 560);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(700, 450);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Own gump graphics / Eigene Gump-Grafiken";
            Controls.Add(tabMain);
            Controls.Add(flowButtons);
            Name = "GumpArtForm";
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPreviewHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPreviewWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFrameId).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBottom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTop).EndInit();
            ((System.ComponentModel.ISupportInitialize)numLeft).EndInit();
            flowFrame.ResumeLayout(false);
            flowFrame.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureFramePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureSource).EndInit();
            splitFrame.Panel1.ResumeLayout(false);
            splitFrame.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitFrame).EndInit();
            splitFrame.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numPictureId).EndInit();
            flowPicture.ResumeLayout(false);
            flowPicture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePicture).EndInit();
            tabFrame.ResumeLayout(false);
            tabFrame.PerformLayout();
            tabPicture.ResumeLayout(false);
            tabPicture.PerformLayout();
            tabMain.ResumeLayout(false);
            tabMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabPicture;
        private System.Windows.Forms.TabPage tabFrame;
        private System.Windows.Forms.PictureBox picturePicture;
        private System.Windows.Forms.FlowLayoutPanel flowPicture;
        private System.Windows.Forms.Button btnLoadPicture;
        private System.Windows.Forms.Label lblPictureId;
        private System.Windows.Forms.NumericUpDown numPictureId;
        private System.Windows.Forms.Button btnFreePicture;
        private System.Windows.Forms.CheckBox chkBlackTransparent;
        private System.Windows.Forms.SplitContainer splitFrame;
        private System.Windows.Forms.PictureBox pictureSource;
        private System.Windows.Forms.PictureBox pictureFramePreview;
        private System.Windows.Forms.FlowLayoutPanel flowFrame;
        private System.Windows.Forms.Button btnLoadFrame;
        private System.Windows.Forms.Label lblLeft;
        private System.Windows.Forms.NumericUpDown numLeft;
        private System.Windows.Forms.Label lblTop;
        private System.Windows.Forms.NumericUpDown numTop;
        private System.Windows.Forms.Label lblRight;
        private System.Windows.Forms.NumericUpDown numRight;
        private System.Windows.Forms.Label lblBottom;
        private System.Windows.Forms.NumericUpDown numBottom;
        private System.Windows.Forms.Label lblFrameId;
        private System.Windows.Forms.NumericUpDown numFrameId;
        private System.Windows.Forms.Button btnFreeFrame;
        private System.Windows.Forms.Label lblPreviewSize;
        private System.Windows.Forms.NumericUpDown numPreviewWidth;
        private System.Windows.Forms.NumericUpDown numPreviewHeight;
        private System.Windows.Forms.CheckBox chkFrameBlack;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnInsert;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

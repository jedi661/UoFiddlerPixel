// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class IsoTiloSlicer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                picImagePreview.Image?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(IsoTiloSlicer));

            picImagePreview = new System.Windows.Forms.PictureBox();
            contextMenuStripPicture = new System.Windows.Forms.ContextMenuStrip(components);
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sep1 = new System.Windows.Forms.ToolStripSeparator();
            mirrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sep2 = new System.Windows.Forms.ToolStripSeparator();
            runClipbordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pnlLeft = new System.Windows.Forms.Panel();
            grpImage = new System.Windows.Forms.GroupBox();
            txtImagePath = new System.Windows.Forms.TextBox();
            btnSelectImage = new System.Windows.Forms.Button();
            grpSettings = new System.Windows.Forms.GroupBox();
            lblTileWidth = new System.Windows.Forms.Label();
            nudTileWidth = new System.Windows.Forms.NumericUpDown();
            lblTileHeight = new System.Windows.Forms.Label();
            nudTileHeight = new System.Windows.Forms.NumericUpDown();
            lblOffset = new System.Windows.Forms.Label();
            nudOffset = new System.Windows.Forms.NumericUpDown();
            lblStartNumber = new System.Windows.Forms.Label();
            nudStartNumber = new System.Windows.Forms.NumericUpDown();
            lblFilenameFormat = new System.Windows.Forms.Label();
            txtFilenameFormat = new System.Windows.Forms.TextBox();
            grpActions = new System.Windows.Forms.GroupBox();
            BtnRun = new System.Windows.Forms.Button();
            BtnRun2 = new System.Windows.Forms.Button();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            BtnDeleteTempFiles = new System.Windows.Forms.Button();
            btnHelp = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            lbImageSize = new System.Windows.Forms.Label();

            ((System.ComponentModel.ISupportInitialize)picImagePreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTileWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudTileHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOffset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStartNumber).BeginInit();
            contextMenuStripPicture.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpImage.SuspendLayout();
            grpSettings.SuspendLayout();
            grpActions.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // ── contextMenuStripPicture ──────────────────────────────────────
            contextMenuStripPicture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                loadToolStripMenuItem, importToolStripMenuItem, sep1,
                mirrorToolStripMenuItem, sep2, runClipbordToolStripMenuItem });
            contextMenuStripPicture.Name = "contextMenuStripPicture";
            contextMenuStripPicture.Size = new System.Drawing.Size(170, 110);

            loadToolStripMenuItem.Text = "Load from file...";
            loadToolStripMenuItem.Image = Properties.Resources.Load;
            loadToolStripMenuItem.Click += LoadToolStripMenuItem_Click;

            importToolStripMenuItem.Text = "Import from Clipboard";
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Click += ImportToolStripMenuItem_Click;

            sep1.Name = "sep1";

            mirrorToolStripMenuItem.Text = "Mirror horizontal";
            mirrorToolStripMenuItem.Image = Properties.Resources.mirror_single_image;
            mirrorToolStripMenuItem.Click += MirrorToolStripMenuItem_Click;

            sep2.Name = "sep2";

            runClipbordToolStripMenuItem.Text = "Run (Straight view)";
            runClipbordToolStripMenuItem.Image = Properties.Resources.right;
            runClipbordToolStripMenuItem.ToolTipText = "Process current preview with Straight view";
            runClipbordToolStripMenuItem.Click += RunClipbordToolStripMenuItem_Click;

            // ── picImagePreview ──────────────────────────────────────────────
            picImagePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picImagePreview.ContextMenuStrip = contextMenuStripPicture;
            picImagePreview.Location = new System.Drawing.Point(248, 28);
            picImagePreview.Name = "picImagePreview";
            picImagePreview.Size = new System.Drawing.Size(440, 440);
            picImagePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picImagePreview.TabStop = false;

            // ── lbImageSize ──────────────────────────────────────────────────
            lbImageSize.AutoSize = true;
            lbImageSize.Location = new System.Drawing.Point(248, 8);
            lbImageSize.Name = "lbImageSize";
            lbImageSize.Text = "Size: -";

            // ── grpImage ─────────────────────────────────────────────────────
            grpImage.Text = "Image";
            grpImage.Location = new System.Drawing.Point(6, 6);
            grpImage.Size = new System.Drawing.Size(230, 70);
            grpImage.TabStop = false;

            txtImagePath.Location = new System.Drawing.Point(8, 20);
            txtImagePath.Size = new System.Drawing.Size(140, 23);
            txtImagePath.ReadOnly = true;
            txtImagePath.TabIndex = 0;
            txtImagePath.Name = "txtImagePath";

            btnSelectImage.Text = "Browse...";
            btnSelectImage.Location = new System.Drawing.Point(154, 19);
            btnSelectImage.Size = new System.Drawing.Size(68, 23);
            btnSelectImage.TabIndex = 1;
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Click += BtnSelectImage_Click;

            grpImage.Controls.Add(txtImagePath);
            grpImage.Controls.Add(btnSelectImage);

            // ── grpSettings ──────────────────────────────────────────────────
            grpSettings.Text = "Tile Settings";
            grpSettings.Location = new System.Drawing.Point(6, 86);
            grpSettings.Size = new System.Drawing.Size(230, 195);
            grpSettings.TabStop = false;

            lblTileWidth.Text = "Tile Width:";
            lblTileWidth.Location = new System.Drawing.Point(8, 24);
            lblTileWidth.AutoSize = true;
            lblTileWidth.Name = "lblTileWidth";

            nudTileWidth.Location = new System.Drawing.Point(130, 20);
            nudTileWidth.Size = new System.Drawing.Size(80, 23);
            nudTileWidth.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudTileWidth.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            nudTileWidth.Value = new decimal(new int[] { 44, 0, 0, 0 });
            nudTileWidth.TabIndex = 2;
            nudTileWidth.Name = "nudTileWidth";

            lblTileHeight.Text = "Tile Height:";
            lblTileHeight.Location = new System.Drawing.Point(8, 58);
            lblTileHeight.AutoSize = true;
            lblTileHeight.Name = "lblTileHeight";

            nudTileHeight.Location = new System.Drawing.Point(130, 54);
            nudTileHeight.Size = new System.Drawing.Size(80, 23);
            nudTileHeight.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudTileHeight.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            nudTileHeight.Value = new decimal(new int[] { 44, 0, 0, 0 });
            nudTileHeight.TabIndex = 3;
            nudTileHeight.Name = "nudTileHeight";

            lblOffset.Text = "Offset:";
            lblOffset.Location = new System.Drawing.Point(8, 92);
            lblOffset.AutoSize = true;
            lblOffset.Name = "lblOffset";

            nudOffset.Location = new System.Drawing.Point(130, 88);
            nudOffset.Size = new System.Drawing.Size(80, 23);
            nudOffset.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            nudOffset.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            nudOffset.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudOffset.TabIndex = 4;
            nudOffset.Name = "nudOffset";

            lblStartNumber.Text = "Start number:";
            lblStartNumber.Location = new System.Drawing.Point(8, 126);
            lblStartNumber.AutoSize = true;
            lblStartNumber.Name = "lblStartNumber";

            nudStartNumber.Location = new System.Drawing.Point(130, 122);
            nudStartNumber.Size = new System.Drawing.Size(80, 23);
            nudStartNumber.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            nudStartNumber.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            nudStartNumber.Value = new decimal(new int[] { 0, 0, 0, 0 });
            nudStartNumber.TabIndex = 5;
            nudStartNumber.Name = "nudStartNumber";

            lblFilenameFormat.Text = "Filename format:";
            lblFilenameFormat.Location = new System.Drawing.Point(8, 160);
            lblFilenameFormat.AutoSize = true;
            lblFilenameFormat.Name = "lblFilenameFormat";

            txtFilenameFormat.Location = new System.Drawing.Point(130, 156);
            txtFilenameFormat.Size = new System.Drawing.Size(80, 23);
            txtFilenameFormat.Text = "{0}";
            txtFilenameFormat.TabIndex = 6;
            txtFilenameFormat.Name = "txtFilenameFormat";

            grpSettings.Controls.AddRange(new System.Windows.Forms.Control[] {
                lblTileWidth,  nudTileWidth,
                lblTileHeight, nudTileHeight,
                lblOffset,     nudOffset,
                lblStartNumber, nudStartNumber,
                lblFilenameFormat, txtFilenameFormat });

            // ── grpActions ───────────────────────────────────────────────────
            grpActions.Text = "Actions";
            grpActions.Location = new System.Drawing.Point(6, 291);
            grpActions.Size = new System.Drawing.Size(230, 170);
            grpActions.TabStop = false;

            BtnRun.Text = "Straight view";
            BtnRun.Location = new System.Drawing.Point(8, 22);
            BtnRun.Size = new System.Drawing.Size(212, 28);
            BtnRun.TabIndex = 10;
            BtnRun.Name = "BtnRun";
            BtnRun.Click += BtnRun_Click;

            BtnRun2.Text = "Grid view";
            BtnRun2.Location = new System.Drawing.Point(8, 56);
            BtnRun2.Size = new System.Drawing.Size(212, 28);
            BtnRun2.TabIndex = 11;
            BtnRun2.Name = "BtnRun2";
            BtnRun2.Click += BtnRun2_Click;

            buttonOpenTempGrafic.Text = "Open output folder";
            buttonOpenTempGrafic.Location = new System.Drawing.Point(8, 92);
            buttonOpenTempGrafic.Size = new System.Drawing.Size(106, 28);
            buttonOpenTempGrafic.TabIndex = 12;
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Click += ButtonOpenTempGrafic_Click;

            BtnDeleteTempFiles.Text = "Delete temp";
            BtnDeleteTempFiles.Location = new System.Drawing.Point(118, 92);
            BtnDeleteTempFiles.Size = new System.Drawing.Size(106, 28);
            BtnDeleteTempFiles.TabIndex = 13;
            BtnDeleteTempFiles.Name = "BtnDeleteTempFiles";
            BtnDeleteTempFiles.Click += BtnDeleteTempFiles_Click;

            btnHelp.Text = "? Help";
            btnHelp.Location = new System.Drawing.Point(8, 128);
            btnHelp.Size = new System.Drawing.Size(212, 28);
            btnHelp.TabIndex = 14;
            btnHelp.Name = "btnHelp";
            btnHelp.Click += BtnHelp_Click;

            grpActions.Controls.AddRange(new System.Windows.Forms.Control[] {
                BtnRun, BtnRun2, buttonOpenTempGrafic, BtnDeleteTempFiles, btnHelp });

            // ── pnlLeft ──────────────────────────────────────────────────────
            pnlLeft.Location = new System.Drawing.Point(4, 4);
            pnlLeft.Size = new System.Drawing.Size(242, 460);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Controls.AddRange(new System.Windows.Forms.Control[] {
                grpImage, grpSettings, grpActions });

            // ── statusStrip ──────────────────────────────────────────────────
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Ready.";
            statusLabel.Spring = true;
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            statusStrip.Items.Add(statusLabel);
            statusStrip.Name = "statusStrip";
            statusStrip.SizingGrip = false;

            // ── Form ─────────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(700, 510);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "IsoTiloSlicer";
            Text = "IsoTiloSlicer";

            Controls.Add(pnlLeft);
            Controls.Add(picImagePreview);
            Controls.Add(lbImageSize);
            Controls.Add(statusStrip);

            ((System.ComponentModel.ISupportInitialize)picImagePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTileWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudTileHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOffset).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStartNumber).EndInit();
            contextMenuStripPicture.ResumeLayout(false);
            pnlLeft.ResumeLayout(false);
            grpImage.ResumeLayout(false);
            grpImage.PerformLayout();
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            grpActions.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox picImagePreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPicture;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sep1;
        private System.Windows.Forms.ToolStripMenuItem mirrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sep2;
        private System.Windows.Forms.ToolStripMenuItem runClipbordToolStripMenuItem;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpImage;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.GroupBox grpSettings;
        private System.Windows.Forms.Label lblTileWidth;
        private System.Windows.Forms.NumericUpDown nudTileWidth;
        private System.Windows.Forms.Label lblTileHeight;
        private System.Windows.Forms.NumericUpDown nudTileHeight;
        private System.Windows.Forms.Label lblOffset;
        private System.Windows.Forms.NumericUpDown nudOffset;
        private System.Windows.Forms.Label lblStartNumber;
        private System.Windows.Forms.NumericUpDown nudStartNumber;
        private System.Windows.Forms.Label lblFilenameFormat;
        private System.Windows.Forms.TextBox txtFilenameFormat;
        private System.Windows.Forms.GroupBox grpActions;
        private System.Windows.Forms.Button BtnRun;
        private System.Windows.Forms.Button BtnRun2;
        private System.Windows.Forms.Button buttonOpenTempGrafic;
        private System.Windows.Forms.Button BtnDeleteTempFiles;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label lbImageSize;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
    }
}
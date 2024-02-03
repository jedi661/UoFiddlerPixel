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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class IsoTiloSlicer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IsoTiloSlicer));
            btnSelectImage = new System.Windows.Forms.Button();
            txtImagePath = new System.Windows.Forms.TextBox();
            picImagePreview = new System.Windows.Forms.PictureBox();
            contextMenuStripPixtureBox = new System.Windows.Forms.ContextMenuStrip(components);
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runClipbordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            mirrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cmbCommands = new System.Windows.Forms.ComboBox();
            BtnRun = new System.Windows.Forms.Button();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            lbImageSize = new System.Windows.Forms.Label();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)picImagePreview).BeginInit();
            contextMenuStripPixtureBox.SuspendLayout();
            SuspendLayout();
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new System.Drawing.Point(174, 44);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new System.Drawing.Size(49, 23);
            btnSelectImage.TabIndex = 0;
            btnSelectImage.Text = "Load Image";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += BtnSelectImage_Click;
            // 
            // txtImagePath
            // 
            txtImagePath.Location = new System.Drawing.Point(12, 44);
            txtImagePath.Name = "txtImagePath";
            txtImagePath.Size = new System.Drawing.Size(156, 23);
            txtImagePath.TabIndex = 1;
            // 
            // picImagePreview
            // 
            picImagePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picImagePreview.ContextMenuStrip = contextMenuStripPixtureBox;
            picImagePreview.Location = new System.Drawing.Point(240, 44);
            picImagePreview.Name = "picImagePreview";
            picImagePreview.Size = new System.Drawing.Size(421, 421);
            picImagePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            picImagePreview.TabIndex = 2;
            picImagePreview.TabStop = false;
            // 
            // contextMenuStripPixtureBox
            // 
            contextMenuStripPixtureBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, importToolStripMenuItem, toolStripSeparator1, mirrorToolStripMenuItem, toolStripSeparator2, runClipbordToolStripMenuItem });
            contextMenuStripPixtureBox.Name = "contextMenuStripPixtureBox";
            contextMenuStripPixtureBox.Size = new System.Drawing.Size(181, 126);
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Image = Properties.Resources.Load;
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.ToolTipText = "Load Image";
            loadToolStripMenuItem.Click += LoadToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            importToolStripMenuItem.Text = "Import Clipboard";
            importToolStripMenuItem.ToolTipText = "Copy from Clipboard";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // runClipbordToolStripMenuItem
            // 
            runClipbordToolStripMenuItem.Image = Properties.Resources.right;
            runClipbordToolStripMenuItem.Name = "runClipbordToolStripMenuItem";
            runClipbordToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            runClipbordToolStripMenuItem.Text = "Run Clipbord";
            runClipbordToolStripMenuItem.ToolTipText = "Start Conversion";
            runClipbordToolStripMenuItem.Click += runClipbordToolStripMenuItem_Click;
            // 
            // mirrorToolStripMenuItem
            // 
            mirrorToolStripMenuItem.Image = Properties.Resources.mirror_single_image;
            mirrorToolStripMenuItem.Name = "mirrorToolStripMenuItem";
            mirrorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            mirrorToolStripMenuItem.Text = "Mirror";
            mirrorToolStripMenuItem.Click += mirrorToolStripMenuItem_Click;
            // 
            // cmbCommands
            // 
            cmbCommands.FormattingEnabled = true;
            cmbCommands.Items.AddRange(new object[] { "--image path", "--tilesize 44", "--offset 1", "--output out", "--filename {0}", "--startingnumber 0", "" });
            cmbCommands.Location = new System.Drawing.Point(12, 12);
            cmbCommands.Name = "cmbCommands";
            cmbCommands.Size = new System.Drawing.Size(156, 23);
            cmbCommands.TabIndex = 3;
            // 
            // BtnRun
            // 
            BtnRun.Location = new System.Drawing.Point(174, 73);
            BtnRun.Name = "BtnRun";
            BtnRun.Size = new System.Drawing.Size(49, 23);
            BtnRun.TabIndex = 4;
            BtnRun.Text = "Run";
            BtnRun.UseVisualStyleBackColor = true;
            BtnRun.Click += BtnRun_Click;
            // 
            // buttonOpenTempGrafic
            // 
            buttonOpenTempGrafic.Location = new System.Drawing.Point(174, 102);
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Size = new System.Drawing.Size(49, 23);
            buttonOpenTempGrafic.TabIndex = 5;
            buttonOpenTempGrafic.Text = "Temp";
            buttonOpenTempGrafic.UseVisualStyleBackColor = true;
            buttonOpenTempGrafic.Click += buttonOpenTempGrafic_Click;
            // 
            // lbImageSize
            // 
            lbImageSize.AutoSize = true;
            lbImageSize.Location = new System.Drawing.Point(240, 15);
            lbImageSize.Name = "lbImageSize";
            lbImageSize.Size = new System.Drawing.Size(27, 15);
            lbImageSize.TabIndex = 6;
            lbImageSize.Text = "Size";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // IsoTiloSlicer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(673, 477);
            Controls.Add(lbImageSize);
            Controls.Add(buttonOpenTempGrafic);
            Controls.Add(BtnRun);
            Controls.Add(cmbCommands);
            Controls.Add(picImagePreview);
            Controls.Add(txtImagePath);
            Controls.Add(btnSelectImage);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "IsoTiloSlicer";
            Text = "IsoTiloSlicer";
            ((System.ComponentModel.ISupportInitialize)picImagePreview).EndInit();
            contextMenuStripPixtureBox.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.PictureBox picImagePreview;
        private System.Windows.Forms.ComboBox cmbCommands;
        private System.Windows.Forms.Button BtnRun;
        private System.Windows.Forms.Button buttonOpenTempGrafic;
        private System.Windows.Forms.Label lbImageSize;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPixtureBox;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runClipbordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}
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
    partial class ParticleGrayForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParticleGrayForm));
            splitContainerParticleGray = new System.Windows.Forms.SplitContainer();
            groupBoxEdit = new System.Windows.Forms.GroupBox();
            ButtonZoomOut = new System.Windows.Forms.Button();
            ButtonZoomIn = new System.Windows.Forms.Button();
            ButtonConvertMask = new System.Windows.Forms.Button();
            ButtonPixel = new System.Windows.Forms.Button();
            pictureBoxParticleGray = new System.Windows.Forms.PictureBox();
            contextMenuStripParticleGray = new System.Windows.Forms.ContextMenuStrip(components);
            loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clipboadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorParticleGray = new System.Windows.Forms.ToolStripSeparator();
            ConvertParticleGrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparatorParticeGray1 = new System.Windows.Forms.ToolStripSeparator();
            CopyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            SaveImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ButtonResetImage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainerParticleGray).BeginInit();
            splitContainerParticleGray.Panel1.SuspendLayout();
            splitContainerParticleGray.Panel2.SuspendLayout();
            splitContainerParticleGray.SuspendLayout();
            groupBoxEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxParticleGray).BeginInit();
            contextMenuStripParticleGray.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerParticleGray
            // 
            splitContainerParticleGray.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerParticleGray.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerParticleGray.Location = new System.Drawing.Point(0, 0);
            splitContainerParticleGray.Name = "splitContainerParticleGray";
            // 
            // splitContainerParticleGray.Panel1
            // 
            splitContainerParticleGray.Panel1.Controls.Add(groupBoxEdit);
            // 
            // splitContainerParticleGray.Panel2
            // 
            splitContainerParticleGray.Panel2.Controls.Add(pictureBoxParticleGray);
            splitContainerParticleGray.Size = new System.Drawing.Size(800, 450);
            splitContainerParticleGray.SplitterDistance = 388;
            splitContainerParticleGray.TabIndex = 0;
            // 
            // groupBoxEdit
            // 
            groupBoxEdit.Controls.Add(ButtonResetImage);
            groupBoxEdit.Controls.Add(ButtonZoomOut);
            groupBoxEdit.Controls.Add(ButtonZoomIn);
            groupBoxEdit.Controls.Add(ButtonConvertMask);
            groupBoxEdit.Controls.Add(ButtonPixel);
            groupBoxEdit.Location = new System.Drawing.Point(12, 12);
            groupBoxEdit.Name = "groupBoxEdit";
            groupBoxEdit.Size = new System.Drawing.Size(361, 140);
            groupBoxEdit.TabIndex = 0;
            groupBoxEdit.TabStop = false;
            groupBoxEdit.Text = "Edit";
            // 
            // ButtonZoomOut
            // 
            ButtonZoomOut.Location = new System.Drawing.Point(266, 101);
            ButtonZoomOut.Name = "ButtonZoomOut";
            ButtonZoomOut.Size = new System.Drawing.Size(75, 23);
            ButtonZoomOut.TabIndex = 3;
            ButtonZoomOut.Text = "Zoom Out";
            ButtonZoomOut.UseVisualStyleBackColor = true;
            ButtonZoomOut.Click += ButtonZoomOut_Click;
            // 
            // ButtonZoomIn
            // 
            ButtonZoomIn.Location = new System.Drawing.Point(266, 72);
            ButtonZoomIn.Name = "ButtonZoomIn";
            ButtonZoomIn.Size = new System.Drawing.Size(75, 23);
            ButtonZoomIn.TabIndex = 2;
            ButtonZoomIn.Text = "Zoom In";
            ButtonZoomIn.UseVisualStyleBackColor = true;
            ButtonZoomIn.Click += ButtonZoomIn_Click;
            // 
            // ButtonConvertMask
            // 
            ButtonConvertMask.Location = new System.Drawing.Point(257, 22);
            ButtonConvertMask.Name = "ButtonConvertMask";
            ButtonConvertMask.Size = new System.Drawing.Size(84, 23);
            ButtonConvertMask.TabIndex = 1;
            ButtonConvertMask.Text = "Convert Gray";
            ButtonConvertMask.UseVisualStyleBackColor = true;
            ButtonConvertMask.Click += ButtonConvertMask_Click;
            // 
            // ButtonPixel
            // 
            ButtonPixel.Location = new System.Drawing.Point(6, 22);
            ButtonPixel.Name = "ButtonPixel";
            ButtonPixel.Size = new System.Drawing.Size(87, 23);
            ButtonPixel.TabIndex = 0;
            ButtonPixel.Text = "Pixel Analysis";
            ButtonPixel.UseVisualStyleBackColor = true;
            ButtonPixel.Click += ButtonPixel_Click;
            // 
            // pictureBoxParticleGray
            // 
            pictureBoxParticleGray.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxParticleGray.ContextMenuStrip = contextMenuStripParticleGray;
            pictureBoxParticleGray.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxParticleGray.Location = new System.Drawing.Point(0, 0);
            pictureBoxParticleGray.Name = "pictureBoxParticleGray";
            pictureBoxParticleGray.Size = new System.Drawing.Size(408, 450);
            pictureBoxParticleGray.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxParticleGray.TabIndex = 0;
            pictureBoxParticleGray.TabStop = false;
            pictureBoxParticleGray.Paint += pictureBoxParticleGray_Paint;
            pictureBoxParticleGray.MouseDown += pictureBoxParticleGray_MouseDown;
            pictureBoxParticleGray.MouseMove += pictureBoxParticleGray_MouseMove;
            pictureBoxParticleGray.MouseUp += pictureBoxParticleGray_MouseUp;
            // 
            // contextMenuStripParticleGray
            // 
            contextMenuStripParticleGray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { loadImageToolStripMenuItem, clipboadImageToolStripMenuItem, toolStripSeparatorParticleGray, ConvertParticleGrayToolStripMenuItem, toolStripSeparatorParticeGray1, CopyToClipboardToolStripMenuItem, SaveImageToolStripMenuItem });
            contextMenuStripParticleGray.Name = "contextMenuStripParticleGray";
            contextMenuStripParticleGray.Size = new System.Drawing.Size(186, 126);
            // 
            // loadImageToolStripMenuItem
            // 
            loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            loadImageToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            loadImageToolStripMenuItem.Text = "Load Image";
            loadImageToolStripMenuItem.Click += LoadImageToolStripMenuItem_Click;
            // 
            // clipboadImageToolStripMenuItem
            // 
            clipboadImageToolStripMenuItem.Name = "clipboadImageToolStripMenuItem";
            clipboadImageToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            clipboadImageToolStripMenuItem.Text = "Clipboard Image";
            clipboadImageToolStripMenuItem.Click += ClipboardImageToolStripMenuItem_Click;
            // 
            // toolStripSeparatorParticleGray
            // 
            toolStripSeparatorParticleGray.Name = "toolStripSeparatorParticleGray";
            toolStripSeparatorParticleGray.Size = new System.Drawing.Size(182, 6);
            // 
            // ConvertParticleGrayToolStripMenuItem
            // 
            ConvertParticleGrayToolStripMenuItem.Name = "ConvertParticleGrayToolStripMenuItem";
            ConvertParticleGrayToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            ConvertParticleGrayToolStripMenuItem.Text = "Convert Particle Gray";
            ConvertParticleGrayToolStripMenuItem.Click += ConvertParticleGrayToolStripMenuItem_Click;
            // 
            // toolStripSeparatorParticeGray1
            // 
            toolStripSeparatorParticeGray1.Name = "toolStripSeparatorParticeGray1";
            toolStripSeparatorParticeGray1.Size = new System.Drawing.Size(182, 6);
            // 
            // CopyToClipboardToolStripMenuItem
            // 
            CopyToClipboardToolStripMenuItem.Name = "CopyToClipboardToolStripMenuItem";
            CopyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            CopyToClipboardToolStripMenuItem.Text = "Copy to Clipboard";
            CopyToClipboardToolStripMenuItem.Click += CopyToClipboardToolStripMenuItem_Click;
            // 
            // SaveImageToolStripMenuItem
            // 
            SaveImageToolStripMenuItem.Name = "SaveImageToolStripMenuItem";
            SaveImageToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            SaveImageToolStripMenuItem.Text = "Save Image";
            SaveImageToolStripMenuItem.Click += SaveImageToolStripMenuItem_Click;
            // 
            // ButtonResetImage
            // 
            ButtonResetImage.Location = new System.Drawing.Point(195, 22);
            ButtonResetImage.Name = "ButtonResetImage";
            ButtonResetImage.Size = new System.Drawing.Size(56, 23);
            ButtonResetImage.TabIndex = 4;
            ButtonResetImage.Text = "Reset";
            ButtonResetImage.UseVisualStyleBackColor = true;
            ButtonResetImage.Click += ButtonResetImage_Click;
            // 
            // ParticleGrayForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(splitContainerParticleGray);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ParticleGrayForm";
            Text = "ParticleGray ";
            splitContainerParticleGray.Panel1.ResumeLayout(false);
            splitContainerParticleGray.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerParticleGray).EndInit();
            splitContainerParticleGray.ResumeLayout(false);
            groupBoxEdit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxParticleGray).EndInit();
            contextMenuStripParticleGray.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerParticleGray;
        private System.Windows.Forms.PictureBox pictureBoxParticleGray;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripParticleGray;
        private System.Windows.Forms.ToolStripMenuItem loadImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clipboadImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorParticleGray;
        private System.Windows.Forms.ToolStripMenuItem ConvertParticleGrayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorParticeGray1;
        private System.Windows.Forms.ToolStripMenuItem CopyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveImageToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxEdit;
        private System.Windows.Forms.Button ButtonPixel;
        private System.Windows.Forms.Button ButtonConvertMask;
        private System.Windows.Forms.Button ButtonZoomOut;
        private System.Windows.Forms.Button ButtonZoomIn;
        private System.Windows.Forms.Button ButtonResetImage;
    }
}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IsoTiloSlicer));
            btnSelectImage = new System.Windows.Forms.Button();
            txtImagePath = new System.Windows.Forms.TextBox();
            picImagePreview = new System.Windows.Forms.PictureBox();
            cmbCommands = new System.Windows.Forms.ComboBox();
            BtnRun = new System.Windows.Forms.Button();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            lbImageSize = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)picImagePreview).BeginInit();
            SuspendLayout();
            // 
            // btnSelectImage
            // 
            btnSelectImage.Location = new System.Drawing.Point(204, 78);
            btnSelectImage.Name = "btnSelectImage";
            btnSelectImage.Size = new System.Drawing.Size(75, 23);
            btnSelectImage.TabIndex = 0;
            btnSelectImage.Text = "Load Image";
            btnSelectImage.UseVisualStyleBackColor = true;
            btnSelectImage.Click += BtnSelectImage_Click;
            // 
            // txtImagePath
            // 
            txtImagePath.Location = new System.Drawing.Point(42, 79);
            txtImagePath.Name = "txtImagePath";
            txtImagePath.Size = new System.Drawing.Size(156, 23);
            txtImagePath.TabIndex = 1;
            // 
            // picImagePreview
            // 
            picImagePreview.Location = new System.Drawing.Point(298, 78);
            picImagePreview.Name = "picImagePreview";
            picImagePreview.Size = new System.Drawing.Size(361, 350);
            picImagePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            picImagePreview.TabIndex = 2;
            picImagePreview.TabStop = false;
            // 
            // cmbCommands
            // 
            cmbCommands.FormattingEnabled = true;
            cmbCommands.Items.AddRange(new object[] { "--image path", "--tilesize 44", "--offset 1", "--output out", "--filename {0}", "--startingnumber 0", "" });
            cmbCommands.Location = new System.Drawing.Point(42, 36);
            cmbCommands.Name = "cmbCommands";
            cmbCommands.Size = new System.Drawing.Size(156, 23);
            cmbCommands.TabIndex = 3;
            // 
            // BtnRun
            // 
            BtnRun.Location = new System.Drawing.Point(204, 107);
            BtnRun.Name = "BtnRun";
            BtnRun.Size = new System.Drawing.Size(75, 23);
            BtnRun.TabIndex = 4;
            BtnRun.Text = "Run";
            BtnRun.UseVisualStyleBackColor = true;
            BtnRun.Click += BtnRun_Click;
            // 
            // buttonOpenTempGrafic
            // 
            buttonOpenTempGrafic.Location = new System.Drawing.Point(204, 145);
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Size = new System.Drawing.Size(75, 23);
            buttonOpenTempGrafic.TabIndex = 5;
            buttonOpenTempGrafic.Text = "Temp";
            buttonOpenTempGrafic.UseVisualStyleBackColor = true;
            buttonOpenTempGrafic.Click += buttonOpenTempGrafic_Click;
            // 
            // lbImageSize
            // 
            lbImageSize.AutoSize = true;
            lbImageSize.Location = new System.Drawing.Point(298, 44);
            lbImageSize.Name = "lbImageSize";
            lbImageSize.Size = new System.Drawing.Size(27, 15);
            lbImageSize.TabIndex = 6;
            lbImageSize.Text = "Size";
            // 
            // IsoTiloSlicer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(770, 477);
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
    }
}
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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class AnimationVDForm
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
            btLoadVD = new System.Windows.Forms.Button();
            listBox1 = new System.Windows.Forms.ListBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            btCopyToClipboard = new System.Windows.Forms.Button();
            btExtractImages = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // btLoadVD
            // 
            btLoadVD.Location = new System.Drawing.Point(49, 395);
            btLoadVD.Name = "btLoadVD";
            btLoadVD.Size = new System.Drawing.Size(75, 23);
            btLoadVD.TabIndex = 0;
            btLoadVD.Text = "button1";
            btLoadVD.UseVisualStyleBackColor = true;
            btLoadVD.Click += btLoadVD_Click;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new System.Drawing.Point(42, 35);
            listBox1.Name = "listBox1";
            listBox1.Size = new System.Drawing.Size(295, 259);
            listBox1.TabIndex = 1;
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new System.Drawing.Point(411, 35);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(306, 334);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // btCopyToClipboard
            // 
            btCopyToClipboard.Location = new System.Drawing.Point(121, 338);
            btCopyToClipboard.Name = "btCopyToClipboard";
            btCopyToClipboard.Size = new System.Drawing.Size(75, 23);
            btCopyToClipboard.TabIndex = 3;
            btCopyToClipboard.Text = "button1";
            btCopyToClipboard.UseVisualStyleBackColor = true;
            btCopyToClipboard.Click += btCopyToClipboard_Click;
            // 
            // btExtractImages
            // 
            btExtractImages.Location = new System.Drawing.Point(252, 391);
            btExtractImages.Name = "btExtractImages";
            btExtractImages.Size = new System.Drawing.Size(75, 23);
            btExtractImages.TabIndex = 4;
            btExtractImages.Text = "button1";
            btExtractImages.UseVisualStyleBackColor = true;
            btExtractImages.Click += btExtractImages_Click;
            // 
            // AnimationVDForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(btExtractImages);
            Controls.Add(btCopyToClipboard);
            Controls.Add(pictureBox1);
            Controls.Add(listBox1);
            Controls.Add(btLoadVD);
            Name = "AnimationVDForm";
            Text = "AnimationVDForm";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btLoadVD;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btCopyToClipboard;
        private System.Windows.Forms.Button btExtractImages;
    }
}
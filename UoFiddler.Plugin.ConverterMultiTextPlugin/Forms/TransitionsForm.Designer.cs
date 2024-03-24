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
    partial class TransitionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransitionsForm));
            btnSelectTexture1 = new System.Windows.Forms.Button();
            btnSelectTexture2 = new System.Windows.Forms.Button();
            btnSelectAlpha = new System.Windows.Forms.Button();
            lbPreview = new System.Windows.Forms.Label();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            pictureBoxLandtile = new System.Windows.Forms.PictureBox();
            lbTransitionBlur = new System.Windows.Forms.Label();
            trackBarContrast = new System.Windows.Forms.TrackBar();
            btnGenerateTransition = new System.Windows.Forms.Button();
            trackBarFlou = new System.Windows.Forms.TrackBar();
            pictureBoxAlpha1 = new System.Windows.Forms.PictureBox();
            pictureBoxTexture2 = new System.Windows.Forms.PictureBox();
            pictureBoxTexture1 = new System.Windows.Forms.PictureBox();
            flowLayoutPanelTextures2 = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanelTextures1 = new System.Windows.Forms.FlowLayoutPanel();
            flowLayoutPanelAlphaImages = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLandtile).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarContrast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarFlou).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAlpha1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture1).BeginInit();
            SuspendLayout();
            // 
            // btnSelectTexture1
            // 
            btnSelectTexture1.Location = new System.Drawing.Point(12, 445);
            btnSelectTexture1.Name = "btnSelectTexture1";
            btnSelectTexture1.Size = new System.Drawing.Size(233, 23);
            btnSelectTexture1.TabIndex = 1;
            btnSelectTexture1.Text = "Texture A";
            btnSelectTexture1.UseVisualStyleBackColor = true;
            btnSelectTexture1.Click += btnSelectTexture1_Click;
            // 
            // btnSelectTexture2
            // 
            btnSelectTexture2.Location = new System.Drawing.Point(251, 445);
            btnSelectTexture2.Name = "btnSelectTexture2";
            btnSelectTexture2.Size = new System.Drawing.Size(233, 23);
            btnSelectTexture2.TabIndex = 3;
            btnSelectTexture2.Text = "Texture B";
            btnSelectTexture2.UseVisualStyleBackColor = true;
            btnSelectTexture2.Click += btnSelectTexture2_Click;
            // 
            // btnSelectAlpha
            // 
            btnSelectAlpha.Location = new System.Drawing.Point(490, 445);
            btnSelectAlpha.Name = "btnSelectAlpha";
            btnSelectAlpha.Size = new System.Drawing.Size(233, 23);
            btnSelectAlpha.TabIndex = 5;
            btnSelectAlpha.Text = "Selecting alpha (grayscale)";
            btnSelectAlpha.UseVisualStyleBackColor = true;
            btnSelectAlpha.Click += btnSelectAlpha_Click;
            // 
            // lbPreview
            // 
            lbPreview.AutoSize = true;
            lbPreview.Location = new System.Drawing.Point(729, 12);
            lbPreview.Name = "lbPreview";
            lbPreview.Size = new System.Drawing.Size(54, 15);
            lbPreview.TabIndex = 6;
            lbPreview.Text = "Preview :";
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxPreview.Location = new System.Drawing.Point(729, 30);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(256, 256);
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBoxPreview.TabIndex = 7;
            pictureBoxPreview.TabStop = false;
            // 
            // pictureBoxLandtile
            // 
            pictureBoxLandtile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxLandtile.Location = new System.Drawing.Point(729, 292);
            pictureBoxLandtile.Name = "pictureBoxLandtile";
            pictureBoxLandtile.Size = new System.Drawing.Size(256, 256);
            pictureBoxLandtile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBoxLandtile.TabIndex = 8;
            pictureBoxLandtile.TabStop = false;
            // 
            // lbTransitionBlur
            // 
            lbTransitionBlur.AutoSize = true;
            lbTransitionBlur.Location = new System.Drawing.Point(729, 551);
            lbTransitionBlur.Name = "lbTransitionBlur";
            lbTransitionBlur.Size = new System.Drawing.Size(88, 15);
            lbTransitionBlur.TabIndex = 9;
            lbTransitionBlur.Text = "Transition blur :";
            // 
            // trackBarContrast
            // 
            trackBarContrast.Location = new System.Drawing.Point(729, 569);
            trackBarContrast.Maximum = 100;
            trackBarContrast.Minimum = 1;
            trackBarContrast.Name = "trackBarContrast";
            trackBarContrast.Size = new System.Drawing.Size(256, 45);
            trackBarContrast.TabIndex = 10;
            trackBarContrast.TickFrequency = 10;
            trackBarContrast.Value = 1;
            trackBarContrast.Scroll += trackBarContrast_Scroll;
            // 
            // btnGenerateTransition
            // 
            btnGenerateTransition.Location = new System.Drawing.Point(729, 656);
            btnGenerateTransition.Name = "btnGenerateTransition";
            btnGenerateTransition.Size = new System.Drawing.Size(256, 23);
            btnGenerateTransition.TabIndex = 11;
            btnGenerateTransition.Text = "Generate transitions";
            btnGenerateTransition.UseVisualStyleBackColor = true;
            btnGenerateTransition.Click += btnGenerateTransition_Click;
            // 
            // trackBarFlou
            // 
            trackBarFlou.Location = new System.Drawing.Point(729, 605);
            trackBarFlou.Maximum = 100;
            trackBarFlou.Minimum = 1;
            trackBarFlou.Name = "trackBarFlou";
            trackBarFlou.Size = new System.Drawing.Size(256, 45);
            trackBarFlou.TabIndex = 12;
            trackBarFlou.TickFrequency = 10;
            trackBarFlou.Value = 1;
            trackBarFlou.Scroll += trackBarFlou_Scroll;
            // 
            // pictureBoxAlpha1
            // 
            pictureBoxAlpha1.Location = new System.Drawing.Point(490, 13);
            pictureBoxAlpha1.Name = "pictureBoxAlpha1";
            pictureBoxAlpha1.Size = new System.Drawing.Size(233, 425);
            pictureBoxAlpha1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBoxAlpha1.TabIndex = 13;
            pictureBoxAlpha1.TabStop = false;
            // 
            // pictureBoxTexture2
            // 
            pictureBoxTexture2.Location = new System.Drawing.Point(251, 13);
            pictureBoxTexture2.Name = "pictureBoxTexture2";
            pictureBoxTexture2.Size = new System.Drawing.Size(233, 425);
            pictureBoxTexture2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBoxTexture2.TabIndex = 14;
            pictureBoxTexture2.TabStop = false;
            // 
            // pictureBoxTexture1
            // 
            pictureBoxTexture1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxTexture1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            pictureBoxTexture1.Location = new System.Drawing.Point(13, 13);
            pictureBoxTexture1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxTexture1.Name = "pictureBoxTexture1";
            pictureBoxTexture1.Size = new System.Drawing.Size(233, 427);
            pictureBoxTexture1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBoxTexture1.TabIndex = 15;
            pictureBoxTexture1.TabStop = false;
            // 
            // flowLayoutPanelTextures2
            // 
            flowLayoutPanelTextures2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flowLayoutPanelTextures2.Location = new System.Drawing.Point(251, 13);
            flowLayoutPanelTextures2.Name = "flowLayoutPanelTextures2";
            flowLayoutPanelTextures2.Size = new System.Drawing.Size(233, 427);
            flowLayoutPanelTextures2.TabIndex = 16;
            // 
            // flowLayoutPanelTextures1
            // 
            flowLayoutPanelTextures1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flowLayoutPanelTextures1.Location = new System.Drawing.Point(13, 13);
            flowLayoutPanelTextures1.Name = "flowLayoutPanelTextures1";
            flowLayoutPanelTextures1.Size = new System.Drawing.Size(233, 427);
            flowLayoutPanelTextures1.TabIndex = 17;
            // 
            // flowLayoutPanelAlphaImages
            // 
            flowLayoutPanelAlphaImages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flowLayoutPanelAlphaImages.Location = new System.Drawing.Point(490, 13);
            flowLayoutPanelAlphaImages.Name = "flowLayoutPanelAlphaImages";
            flowLayoutPanelAlphaImages.Size = new System.Drawing.Size(233, 427);
            flowLayoutPanelAlphaImages.TabIndex = 18;
            // 
            // TransitionsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1005, 691);
            Controls.Add(flowLayoutPanelAlphaImages);
            Controls.Add(flowLayoutPanelTextures1);
            Controls.Add(flowLayoutPanelTextures2);
            Controls.Add(trackBarFlou);
            Controls.Add(btnGenerateTransition);
            Controls.Add(trackBarContrast);
            Controls.Add(lbTransitionBlur);
            Controls.Add(pictureBoxLandtile);
            Controls.Add(pictureBoxPreview);
            Controls.Add(lbPreview);
            Controls.Add(btnSelectAlpha);
            Controls.Add(btnSelectTexture2);
            Controls.Add(btnSelectTexture1);
            Controls.Add(pictureBoxAlpha1);
            Controls.Add(pictureBoxTexture2);
            Controls.Add(pictureBoxTexture1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TransitionsForm";
            Text = "UO Texture Transition";
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLandtile).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarContrast).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarFlou).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAlpha1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnSelectTexture1;
        private System.Windows.Forms.Button btnSelectTexture2;
        private System.Windows.Forms.Button btnSelectAlpha;
        private System.Windows.Forms.Label lbPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.PictureBox pictureBoxLandtile;
        private System.Windows.Forms.Label lbTransitionBlur;
        private System.Windows.Forms.TrackBar trackBarContrast;
        private System.Windows.Forms.Button btnGenerateTransition;
        private System.Windows.Forms.TrackBar trackBarFlou;
        private System.Windows.Forms.PictureBox pictureBoxAlpha1;
        private System.Windows.Forms.PictureBox pictureBoxTexture2;
        private System.Windows.Forms.PictureBox pictureBoxTexture1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTextures2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTextures1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelAlphaImages;
    }
}
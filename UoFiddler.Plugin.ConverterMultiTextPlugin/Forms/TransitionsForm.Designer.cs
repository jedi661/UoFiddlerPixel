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
            textBoxNameTextureA = new System.Windows.Forms.TextBox();
            lbName = new System.Windows.Forms.Label();
            lbName2 = new System.Windows.Forms.Label();
            textBoxNameTextureB = new System.Windows.Forms.TextBox();
            lbBrushID = new System.Windows.Forms.Label();
            textBoxBrushNumberA = new System.Windows.Forms.TextBox();
            lbBrushID2 = new System.Windows.Forms.Label();
            textBoxBrushNumberB = new System.Windows.Forms.TextBox();
            lblAlphaName = new System.Windows.Forms.Label();
            Compteur = new System.Windows.Forms.Label();
            btnPrevious = new System.Windows.Forms.Button();
            btnNext = new System.Windows.Forms.Button();
            lbStartHexadecimal = new System.Windows.Forms.Label();
            tbStartHexDec = new System.Windows.Forms.TextBox();
            checkBoxZoom = new System.Windows.Forms.CheckBox();
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
            pictureBoxPreview.TabIndex = 7;
            pictureBoxPreview.TabStop = false;
            // 
            // pictureBoxLandtile
            // 
            pictureBoxLandtile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxLandtile.Location = new System.Drawing.Point(729, 292);
            pictureBoxLandtile.Name = "pictureBoxLandtile";
            pictureBoxLandtile.Size = new System.Drawing.Size(256, 256);
            pictureBoxLandtile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
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
            trackBarContrast.LargeChange = 1;
            trackBarContrast.Location = new System.Drawing.Point(729, 569);
            trackBarContrast.Maximum = 20;
            trackBarContrast.Minimum = 1;
            trackBarContrast.Name = "trackBarContrast";
            trackBarContrast.Size = new System.Drawing.Size(256, 45);
            trackBarContrast.TabIndex = 7;
            trackBarContrast.TickFrequency = 2;
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
            flowLayoutPanelAlphaImages.AutoScroll = true;
            flowLayoutPanelAlphaImages.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            flowLayoutPanelAlphaImages.Location = new System.Drawing.Point(490, 13);
            flowLayoutPanelAlphaImages.Name = "flowLayoutPanelAlphaImages";
            flowLayoutPanelAlphaImages.Size = new System.Drawing.Size(233, 427);
            flowLayoutPanelAlphaImages.TabIndex = 18;
            // 
            // textBoxNameTextureA
            // 
            textBoxNameTextureA.Location = new System.Drawing.Point(75, 501);
            textBoxNameTextureA.Name = "textBoxNameTextureA";
            textBoxNameTextureA.Size = new System.Drawing.Size(100, 23);
            textBoxNameTextureA.TabIndex = 19;
            textBoxNameTextureA.TextChanged += textBoxNameTextureA_TextChanged;
            // 
            // lbName
            // 
            lbName.AutoSize = true;
            lbName.Location = new System.Drawing.Point(104, 483);
            lbName.Name = "lbName";
            lbName.Size = new System.Drawing.Size(45, 15);
            lbName.TabIndex = 20;
            lbName.Text = "Name :";
            // 
            // lbName2
            // 
            lbName2.AutoSize = true;
            lbName2.Location = new System.Drawing.Point(343, 483);
            lbName2.Name = "lbName2";
            lbName2.Size = new System.Drawing.Size(45, 15);
            lbName2.TabIndex = 22;
            lbName2.Text = "Name :";
            // 
            // textBoxNameTextureB
            // 
            textBoxNameTextureB.Location = new System.Drawing.Point(314, 501);
            textBoxNameTextureB.Name = "textBoxNameTextureB";
            textBoxNameTextureB.Size = new System.Drawing.Size(100, 23);
            textBoxNameTextureB.TabIndex = 21;
            textBoxNameTextureB.TextChanged += textBoxNameTextureB_TextChanged;
            // 
            // lbBrushID
            // 
            lbBrushID.AutoSize = true;
            lbBrushID.Location = new System.Drawing.Point(104, 533);
            lbBrushID.Name = "lbBrushID";
            lbBrushID.Size = new System.Drawing.Size(48, 15);
            lbBrushID.TabIndex = 24;
            lbBrushID.Text = "BrushID";
            // 
            // textBoxBrushNumberA
            // 
            textBoxBrushNumberA.Location = new System.Drawing.Point(75, 551);
            textBoxBrushNumberA.Name = "textBoxBrushNumberA";
            textBoxBrushNumberA.Size = new System.Drawing.Size(100, 23);
            textBoxBrushNumberA.TabIndex = 23;
            textBoxBrushNumberA.TextChanged += textBoxBrushNumberA_TextChanged;
            // 
            // lbBrushID2
            // 
            lbBrushID2.AutoSize = true;
            lbBrushID2.Location = new System.Drawing.Point(343, 533);
            lbBrushID2.Name = "lbBrushID2";
            lbBrushID2.Size = new System.Drawing.Size(48, 15);
            lbBrushID2.TabIndex = 26;
            lbBrushID2.Text = "BrushID";
            // 
            // textBoxBrushNumberB
            // 
            textBoxBrushNumberB.Location = new System.Drawing.Point(314, 551);
            textBoxBrushNumberB.Name = "textBoxBrushNumberB";
            textBoxBrushNumberB.Size = new System.Drawing.Size(100, 23);
            textBoxBrushNumberB.TabIndex = 25;
            textBoxBrushNumberB.TextChanged += textBoxBrushNumberB_TextChanged;
            // 
            // lblAlphaName
            // 
            lblAlphaName.AutoSize = true;
            lblAlphaName.Location = new System.Drawing.Point(1005, 30);
            lblAlphaName.Name = "lblAlphaName";
            lblAlphaName.Size = new System.Drawing.Size(79, 15);
            lblAlphaName.TabIndex = 27;
            lblAlphaName.Text = "Alpha Name :";
            // 
            // Compteur
            // 
            Compteur.AutoSize = true;
            Compteur.Location = new System.Drawing.Point(1005, 76);
            Compteur.Name = "Compteur";
            Compteur.Size = new System.Drawing.Size(56, 15);
            Compteur.TabIndex = 28;
            Compteur.Text = "Counter :";
            // 
            // btnPrevious
            // 
            btnPrevious.Location = new System.Drawing.Point(1005, 119);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new System.Drawing.Size(75, 23);
            btnPrevious.TabIndex = 29;
            btnPrevious.Text = "Previous";
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.Location = new System.Drawing.Point(1005, 157);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(75, 23);
            btnNext.TabIndex = 30;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // lbStartHexadecimal
            // 
            lbStartHexadecimal.AutoSize = true;
            lbStartHexadecimal.Location = new System.Drawing.Point(1005, 226);
            lbStartHexadecimal.Name = "lbStartHexadecimal";
            lbStartHexadecimal.Size = new System.Drawing.Size(133, 15);
            lbStartHexadecimal.TabIndex = 31;
            lbStartHexadecimal.Text = "Start Hexadecimal Ox....";
            // 
            // tbStartHexDec
            // 
            tbStartHexDec.Location = new System.Drawing.Point(1005, 253);
            tbStartHexDec.Name = "tbStartHexDec";
            tbStartHexDec.Size = new System.Drawing.Size(100, 23);
            tbStartHexDec.TabIndex = 32;
            tbStartHexDec.TextChanged += StartHexDec_TextChanged;
            // 
            // checkBoxZoom
            // 
            checkBoxZoom.AutoSize = true;
            checkBoxZoom.Checked = true;
            checkBoxZoom.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxZoom.Location = new System.Drawing.Point(1005, 195);
            checkBoxZoom.Name = "checkBoxZoom";
            checkBoxZoom.Size = new System.Drawing.Size(137, 19);
            checkBoxZoom.TabIndex = 33;
            checkBoxZoom.Text = "Zoom / Centerimage";
            checkBoxZoom.UseVisualStyleBackColor = true;
            checkBoxZoom.CheckedChanged += checkBoxZoom_CheckedChanged;
            // 
            // TransitionsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1178, 691);
            Controls.Add(checkBoxZoom);
            Controls.Add(tbStartHexDec);
            Controls.Add(lbStartHexadecimal);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(Compteur);
            Controls.Add(lblAlphaName);
            Controls.Add(lbBrushID2);
            Controls.Add(textBoxBrushNumberB);
            Controls.Add(lbBrushID);
            Controls.Add(textBoxBrushNumberA);
            Controls.Add(lbName2);
            Controls.Add(textBoxNameTextureB);
            Controls.Add(lbName);
            Controls.Add(textBoxNameTextureA);
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
        private System.Windows.Forms.TextBox textBoxNameTextureA;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Label lbName2;
        private System.Windows.Forms.TextBox textBoxNameTextureB;
        private System.Windows.Forms.Label lbBrushID;
        private System.Windows.Forms.TextBox textBoxBrushNumberA;
        private System.Windows.Forms.Label lbBrushID2;
        private System.Windows.Forms.TextBox textBoxBrushNumberB;
        private System.Windows.Forms.Label lblAlphaName;
        private System.Windows.Forms.Label Compteur;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Label lbStartHexadecimal;
        private System.Windows.Forms.TextBox tbStartHexDec;
        private System.Windows.Forms.CheckBox checkBoxZoom;
    }
}
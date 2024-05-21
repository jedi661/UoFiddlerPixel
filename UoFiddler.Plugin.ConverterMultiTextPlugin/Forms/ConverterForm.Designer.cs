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
    partial class ConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConverterForm));
            BtConverterWhite = new System.Windows.Forms.Button();
            BtConverterBlack = new System.Windows.Forms.Button();
            panelColor = new System.Windows.Forms.Panel();
            BtnOpenColorDialog = new System.Windows.Forms.Button();
            BtConverterCustom = new System.Windows.Forms.Button();
            lbBlackWhite = new System.Windows.Forms.Label();
            BtMirrorImages = new System.Windows.Forms.Button();
            panelFunctions = new System.Windows.Forms.Panel();
            BtConvert = new System.Windows.Forms.Button();
            comboBoxFileType = new System.Windows.Forms.ComboBox();
            BtRotateImages = new System.Windows.Forms.Button();
            BtConverterTransparent = new System.Windows.Forms.Button();
            lbFunction = new System.Windows.Forms.Label();
            panelColor.SuspendLayout();
            panelFunctions.SuspendLayout();
            SuspendLayout();
            // 
            // BtConverterWhite
            // 
            BtConverterWhite.Location = new System.Drawing.Point(14, 29);
            BtConverterWhite.Name = "BtConverterWhite";
            BtConverterWhite.Size = new System.Drawing.Size(54, 23);
            BtConverterWhite.TabIndex = 0;
            BtConverterWhite.Text = "White";
            BtConverterWhite.UseVisualStyleBackColor = true;
            BtConverterWhite.Click += BtConverterWhite_Click;
            // 
            // BtConverterBlack
            // 
            BtConverterBlack.Location = new System.Drawing.Point(14, 58);
            BtConverterBlack.Name = "BtConverterBlack";
            BtConverterBlack.Size = new System.Drawing.Size(54, 23);
            BtConverterBlack.TabIndex = 1;
            BtConverterBlack.Text = "Black";
            BtConverterBlack.UseVisualStyleBackColor = true;
            BtConverterBlack.Click += BtConverterBlack_Click;
            // 
            // panelColor
            // 
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Controls.Add(BtnOpenColorDialog);
            panelColor.Controls.Add(BtConverterCustom);
            panelColor.Controls.Add(lbBlackWhite);
            panelColor.Controls.Add(BtConverterBlack);
            panelColor.Controls.Add(BtConverterWhite);
            panelColor.Location = new System.Drawing.Point(12, 12);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(200, 100);
            panelColor.TabIndex = 2;
            // 
            // BtnOpenColorDialog
            // 
            BtnOpenColorDialog.Location = new System.Drawing.Point(89, 58);
            BtnOpenColorDialog.Name = "BtnOpenColorDialog";
            BtnOpenColorDialog.Size = new System.Drawing.Size(100, 23);
            BtnOpenColorDialog.TabIndex = 3;
            BtnOpenColorDialog.Text = "Colors Save";
            BtnOpenColorDialog.UseVisualStyleBackColor = true;
            BtnOpenColorDialog.Click += BtnOpenColorDialog_Click;
            // 
            // BtConverterCustom
            // 
            BtConverterCustom.Location = new System.Drawing.Point(89, 29);
            BtConverterCustom.Name = "BtConverterCustom";
            BtConverterCustom.Size = new System.Drawing.Size(100, 23);
            BtConverterCustom.TabIndex = 4;
            BtConverterCustom.Text = "Custom Color";
            BtConverterCustom.UseVisualStyleBackColor = true;
            BtConverterCustom.Click += BtConverterCustom_Click;
            // 
            // lbBlackWhite
            // 
            lbBlackWhite.AutoSize = true;
            lbBlackWhite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lbBlackWhite.Location = new System.Drawing.Point(14, 8);
            lbBlackWhite.Name = "lbBlackWhite";
            lbBlackWhite.Size = new System.Drawing.Size(181, 15);
            lbBlackWhite.TabIndex = 3;
            lbBlackWhite.Text = "Convert Image Black or White :";
            // 
            // BtMirrorImages
            // 
            BtMirrorImages.Location = new System.Drawing.Point(13, 30);
            BtMirrorImages.Name = "BtMirrorImages";
            BtMirrorImages.Size = new System.Drawing.Size(51, 23);
            BtMirrorImages.TabIndex = 3;
            BtMirrorImages.Text = "Mirror";
            BtMirrorImages.UseVisualStyleBackColor = true;
            BtMirrorImages.Click += BtMirrorImages_Click;
            // 
            // panelFunctions
            // 
            panelFunctions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelFunctions.Controls.Add(BtConvert);
            panelFunctions.Controls.Add(comboBoxFileType);
            panelFunctions.Controls.Add(BtRotateImages);
            panelFunctions.Controls.Add(BtConverterTransparent);
            panelFunctions.Controls.Add(lbFunction);
            panelFunctions.Controls.Add(BtMirrorImages);
            panelFunctions.Location = new System.Drawing.Point(218, 12);
            panelFunctions.Name = "panelFunctions";
            panelFunctions.Size = new System.Drawing.Size(200, 100);
            panelFunctions.TabIndex = 4;
            // 
            // BtConvert
            // 
            BtConvert.Location = new System.Drawing.Point(126, 59);
            BtConvert.Name = "BtConvert";
            BtConvert.Size = new System.Drawing.Size(53, 22);
            BtConvert.TabIndex = 8;
            BtConvert.Text = "FileTyp";
            BtConvert.UseVisualStyleBackColor = true;
            BtConvert.Click += BtConvert_Click;
            // 
            // comboBoxFileType
            // 
            comboBoxFileType.FormattingEnabled = true;
            comboBoxFileType.Items.AddRange(new object[] { "bmp", "png", "jpg", "tiff" });
            comboBoxFileType.Location = new System.Drawing.Point(70, 59);
            comboBoxFileType.Name = "comboBoxFileType";
            comboBoxFileType.Size = new System.Drawing.Size(55, 23);
            comboBoxFileType.TabIndex = 7;
            // 
            // BtRotateImages
            // 
            BtRotateImages.Location = new System.Drawing.Point(13, 59);
            BtRotateImages.Name = "BtRotateImages";
            BtRotateImages.Size = new System.Drawing.Size(51, 23);
            BtRotateImages.TabIndex = 6;
            BtRotateImages.Text = "Rotate";
            BtRotateImages.UseVisualStyleBackColor = true;
            BtRotateImages.Click += BtRotateImages_Click;
            // 
            // BtConverterTransparent
            // 
            BtConverterTransparent.Location = new System.Drawing.Point(70, 30);
            BtConverterTransparent.Name = "BtConverterTransparent";
            BtConverterTransparent.Size = new System.Drawing.Size(79, 23);
            BtConverterTransparent.TabIndex = 5;
            BtConverterTransparent.Text = "Transparent";
            BtConverterTransparent.UseVisualStyleBackColor = true;
            BtConverterTransparent.Click += BtConverterTransparent_Click;
            // 
            // lbFunction
            // 
            lbFunction.AutoSize = true;
            lbFunction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lbFunction.Location = new System.Drawing.Point(10, 9);
            lbFunction.Name = "lbFunction";
            lbFunction.Size = new System.Drawing.Size(66, 15);
            lbFunction.TabIndex = 4;
            lbFunction.Text = "Functions :";
            // 
            // ConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(425, 116);
            Controls.Add(panelFunctions);
            Controls.Add(panelColor);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ConverterForm";
            Text = "Converter all Imges or other";
            panelColor.ResumeLayout(false);
            panelColor.PerformLayout();
            panelFunctions.ResumeLayout(false);
            panelFunctions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button BtConverterWhite;
        private System.Windows.Forms.Button BtConverterBlack;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lbBlackWhite;
        private System.Windows.Forms.Button BtConverterCustom;
        private System.Windows.Forms.Button BtnOpenColorDialog;
        private System.Windows.Forms.Button BtMirrorImages;
        private System.Windows.Forms.Panel panelFunctions;
        private System.Windows.Forms.Label lbFunction;
        private System.Windows.Forms.Button BtConverterTransparent;
        private System.Windows.Forms.Button BtRotateImages;
        private System.Windows.Forms.ComboBox comboBoxFileType;
        private System.Windows.Forms.Button BtConvert;
    }
}
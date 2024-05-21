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
            btConverterWhite = new System.Windows.Forms.Button();
            btConverterBlack = new System.Windows.Forms.Button();
            panelColor = new System.Windows.Forms.Panel();
            btnOpenColorDialog = new System.Windows.Forms.Button();
            btConverterCustom = new System.Windows.Forms.Button();
            lbBlackWhite = new System.Windows.Forms.Label();
            btMirrorImages = new System.Windows.Forms.Button();
            panelFunctions = new System.Windows.Forms.Panel();
            lbFunction = new System.Windows.Forms.Label();
            panelColor.SuspendLayout();
            panelFunctions.SuspendLayout();
            SuspendLayout();
            // 
            // btConverterWhite
            // 
            btConverterWhite.Location = new System.Drawing.Point(14, 29);
            btConverterWhite.Name = "btConverterWhite";
            btConverterWhite.Size = new System.Drawing.Size(54, 23);
            btConverterWhite.TabIndex = 0;
            btConverterWhite.Text = "White";
            btConverterWhite.UseVisualStyleBackColor = true;
            btConverterWhite.Click += btConverterWhite_Click;
            // 
            // btConverterBlack
            // 
            btConverterBlack.Location = new System.Drawing.Point(14, 58);
            btConverterBlack.Name = "btConverterBlack";
            btConverterBlack.Size = new System.Drawing.Size(54, 23);
            btConverterBlack.TabIndex = 1;
            btConverterBlack.Text = "Black";
            btConverterBlack.UseVisualStyleBackColor = true;
            btConverterBlack.Click += btConverterBlack_Click;
            // 
            // panelColor
            // 
            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Controls.Add(btnOpenColorDialog);
            panelColor.Controls.Add(btConverterCustom);
            panelColor.Controls.Add(lbBlackWhite);
            panelColor.Controls.Add(btConverterBlack);
            panelColor.Controls.Add(btConverterWhite);
            panelColor.Location = new System.Drawing.Point(12, 12);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(200, 100);
            panelColor.TabIndex = 2;
            // 
            // btnOpenColorDialog
            // 
            btnOpenColorDialog.Location = new System.Drawing.Point(89, 58);
            btnOpenColorDialog.Name = "btnOpenColorDialog";
            btnOpenColorDialog.Size = new System.Drawing.Size(100, 23);
            btnOpenColorDialog.TabIndex = 3;
            btnOpenColorDialog.Text = "Colors Save";
            btnOpenColorDialog.UseVisualStyleBackColor = true;
            btnOpenColorDialog.Click += btnOpenColorDialog_Click;
            // 
            // btConverterCustom
            // 
            btConverterCustom.Location = new System.Drawing.Point(89, 29);
            btConverterCustom.Name = "btConverterCustom";
            btConverterCustom.Size = new System.Drawing.Size(100, 23);
            btConverterCustom.TabIndex = 4;
            btConverterCustom.Text = "Custom Color";
            btConverterCustom.UseVisualStyleBackColor = true;
            btConverterCustom.Click += btConverterCustom_Click;
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
            // btMirrorImages
            // 
            btMirrorImages.Location = new System.Drawing.Point(13, 30);
            btMirrorImages.Name = "btMirrorImages";
            btMirrorImages.Size = new System.Drawing.Size(51, 23);
            btMirrorImages.TabIndex = 3;
            btMirrorImages.Text = "Mirror";
            btMirrorImages.UseVisualStyleBackColor = true;
            btMirrorImages.Click += btMirrorImages_Click;
            // 
            // panelFunctions
            // 
            panelFunctions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelFunctions.Controls.Add(lbFunction);
            panelFunctions.Controls.Add(btMirrorImages);
            panelFunctions.Location = new System.Drawing.Point(218, 12);
            panelFunctions.Name = "panelFunctions";
            panelFunctions.Size = new System.Drawing.Size(200, 100);
            panelFunctions.TabIndex = 4;
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
            ClientSize = new System.Drawing.Size(556, 335);
            Controls.Add(panelFunctions);
            Controls.Add(panelColor);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ConverterForm";
            Text = "Converter Imges or other";
            panelColor.ResumeLayout(false);
            panelColor.PerformLayout();
            panelFunctions.ResumeLayout(false);
            panelFunctions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btConverterWhite;
        private System.Windows.Forms.Button btConverterBlack;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lbBlackWhite;
        private System.Windows.Forms.Button btConverterCustom;
        private System.Windows.Forms.Button btnOpenColorDialog;
        private System.Windows.Forms.Button btMirrorImages;
        private System.Windows.Forms.Panel panelFunctions;
        private System.Windows.Forms.Label lbFunction;
    }
}
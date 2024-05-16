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

namespace UoFiddler.Controls.Forms
{
    partial class TTFunicodeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TTFunicodeForm));
            panel1 = new System.Windows.Forms.Panel();
            btOpenDir = new System.Windows.Forms.Button();
            lbformat = new System.Windows.Forms.Label();
            lbsize = new System.Windows.Forms.Label();
            comboBoxFontStyle = new System.Windows.Forms.ComboBox();
            comboBoxFontSize = new System.Windows.Forms.ComboBox();
            buttonExport = new System.Windows.Forms.Button();
            buttonImportTTF = new System.Windows.Forms.Button();
            pictureBoxGlyphPreview = new System.Windows.Forms.PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGlyphPreview).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(btOpenDir);
            panel1.Controls.Add(lbformat);
            panel1.Controls.Add(lbsize);
            panel1.Controls.Add(comboBoxFontStyle);
            panel1.Controls.Add(comboBoxFontSize);
            panel1.Controls.Add(buttonExport);
            panel1.Controls.Add(buttonImportTTF);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(200, 186);
            panel1.TabIndex = 0;
            // 
            // btOpenDir
            // 
            btOpenDir.Location = new System.Drawing.Point(93, 9);
            btOpenDir.Name = "btOpenDir";
            btOpenDir.Size = new System.Drawing.Size(65, 23);
            btOpenDir.TabIndex = 8;
            btOpenDir.Text = "Open Dir";
            btOpenDir.UseVisualStyleBackColor = true;
            btOpenDir.Click += btOpenDir_Click;
            // 
            // lbformat
            // 
            lbformat.AutoSize = true;
            lbformat.Location = new System.Drawing.Point(12, 72);
            lbformat.Name = "lbformat";
            lbformat.Size = new System.Drawing.Size(43, 15);
            lbformat.TabIndex = 7;
            lbformat.Text = "format";
            // 
            // lbsize
            // 
            lbsize.AutoSize = true;
            lbsize.Location = new System.Drawing.Point(143, 72);
            lbsize.Name = "lbsize";
            lbsize.Size = new System.Drawing.Size(27, 15);
            lbsize.TabIndex = 6;
            lbsize.Text = "Size";
            // 
            // comboBoxFontStyle
            // 
            comboBoxFontStyle.FormattingEnabled = true;
            comboBoxFontStyle.Location = new System.Drawing.Point(12, 90);
            comboBoxFontStyle.Name = "comboBoxFontStyle";
            comboBoxFontStyle.Size = new System.Drawing.Size(121, 23);
            comboBoxFontStyle.TabIndex = 5;
            // 
            // comboBoxFontSize
            // 
            comboBoxFontSize.FormattingEnabled = true;
            comboBoxFontSize.Items.AddRange(new object[] { "2", "4", "6", "8", "10", "12", "14", "16", "18", "20", "22", "24", "26", "28", "30" });
            comboBoxFontSize.Location = new System.Drawing.Point(143, 90);
            comboBoxFontSize.Name = "comboBoxFontSize";
            comboBoxFontSize.Size = new System.Drawing.Size(37, 23);
            comboBoxFontSize.TabIndex = 4;
            // 
            // buttonExport
            // 
            buttonExport.Location = new System.Drawing.Point(12, 38);
            buttonExport.Name = "buttonExport";
            buttonExport.Size = new System.Drawing.Size(75, 23);
            buttonExport.TabIndex = 3;
            buttonExport.Text = "Export";
            buttonExport.UseVisualStyleBackColor = true;
            buttonExport.Click += ButtonExport_Click;
            // 
            // buttonImportTTF
            // 
            buttonImportTTF.Location = new System.Drawing.Point(12, 9);
            buttonImportTTF.Name = "buttonImportTTF";
            buttonImportTTF.Size = new System.Drawing.Size(75, 23);
            buttonImportTTF.TabIndex = 2;
            buttonImportTTF.Text = "Import TTF";
            buttonImportTTF.UseVisualStyleBackColor = true;
            buttonImportTTF.Click += ButtonImportTTF_Click;
            // 
            // pictureBoxGlyphPreview
            // 
            pictureBoxGlyphPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxGlyphPreview.Location = new System.Drawing.Point(218, 12);
            pictureBoxGlyphPreview.Name = "pictureBoxGlyphPreview";
            pictureBoxGlyphPreview.Size = new System.Drawing.Size(958, 186);
            pictureBoxGlyphPreview.TabIndex = 1;
            pictureBoxGlyphPreview.TabStop = false;
            // 
            // TTFunicodeForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1184, 208);
            Controls.Add(pictureBoxGlyphPreview);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TTFunicodeForm";
            Text = "TTF - Unicode Export";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGlyphPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBoxGlyphPreview;
        private System.Windows.Forms.ComboBox comboBoxFontStyle;
        private System.Windows.Forms.ComboBox comboBoxFontSize;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonImportTTF;
        private System.Windows.Forms.Button btOpenDir;
        private System.Windows.Forms.Label lbformat;
        private System.Windows.Forms.Label lbsize;
    }
}
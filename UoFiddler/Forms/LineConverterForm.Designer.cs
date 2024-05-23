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

namespace UoFiddler.Forms
{
    partial class LineConverterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LineConverterForm));
            TextBoxInputOutput = new System.Windows.Forms.TextBox();
            BtnConvert = new System.Windows.Forms.Button();
            BtnConvert2 = new System.Windows.Forms.Button();
            lbCounter = new System.Windows.Forms.Label();
            BtnClear = new System.Windows.Forms.Button();
            BtnCopy = new System.Windows.Forms.Button();
            BtnRestore = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            chkAddSpaces = new System.Windows.Forms.CheckBox();
            BtnConvertParagraphsToLines2WithoutComments = new System.Windows.Forms.Button();
            BtnConvertWithBlocks = new System.Windows.Forms.Button();
            chkBlockSize4000 = new System.Windows.Forms.CheckBox();
            lblBlockCount = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // TextBoxInputOutput
            // 
            TextBoxInputOutput.Location = new System.Drawing.Point(12, 89);
            TextBoxInputOutput.MaxLength = 2000000;
            TextBoxInputOutput.Multiline = true;
            TextBoxInputOutput.Name = "TextBoxInputOutput";
            TextBoxInputOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            TextBoxInputOutput.Size = new System.Drawing.Size(993, 318);
            TextBoxInputOutput.TabIndex = 0;
            // 
            // BtnConvert
            // 
            BtnConvert.Location = new System.Drawing.Point(12, 422);
            BtnConvert.Name = "BtnConvert";
            BtnConvert.Size = new System.Drawing.Size(75, 23);
            BtnConvert.TabIndex = 1;
            BtnConvert.Text = "Convert";
            BtnConvert.UseVisualStyleBackColor = true;
            BtnConvert.Click += BtnConvert_Click;
            // 
            // BtnConvert2
            // 
            BtnConvert2.Location = new System.Drawing.Point(12, 451);
            BtnConvert2.Name = "BtnConvert2";
            BtnConvert2.Size = new System.Drawing.Size(75, 23);
            BtnConvert2.TabIndex = 2;
            BtnConvert2.Text = "Convert";
            BtnConvert2.UseVisualStyleBackColor = true;
            BtnConvert2.Click += BtnConvert2_Click;
            // 
            // lbCounter
            // 
            lbCounter.AutoSize = true;
            lbCounter.Location = new System.Drawing.Point(425, 426);
            lbCounter.Name = "lbCounter";
            lbCounter.Size = new System.Drawing.Size(40, 15);
            lbCounter.TabIndex = 3;
            lbCounter.Text = "Count";
            // 
            // BtnClear
            // 
            BtnClear.Location = new System.Drawing.Point(182, 422);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(75, 23);
            BtnClear.TabIndex = 4;
            BtnClear.Text = "Clear";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
            // 
            // BtnCopy
            // 
            BtnCopy.Location = new System.Drawing.Point(263, 422);
            BtnCopy.Name = "BtnCopy";
            BtnCopy.Size = new System.Drawing.Size(75, 23);
            BtnCopy.TabIndex = 5;
            BtnCopy.Text = "Copy";
            BtnCopy.UseVisualStyleBackColor = true;
            BtnCopy.Click += BtnCopy_Click;
            // 
            // BtnRestore
            // 
            BtnRestore.Location = new System.Drawing.Point(344, 422);
            BtnRestore.Name = "BtnRestore";
            BtnRestore.Size = new System.Drawing.Size(75, 23);
            BtnRestore.TabIndex = 6;
            BtnRestore.Text = "Restore";
            BtnRestore.UseVisualStyleBackColor = true;
            BtnRestore.Click += BtnRestore_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(994, 75);
            label1.TabIndex = 7;
            label1.Text = resources.GetString("label1.Text");
            // 
            // chkAddSpaces
            // 
            chkAddSpaces.AutoSize = true;
            chkAddSpaces.Location = new System.Drawing.Point(93, 425);
            chkAddSpaces.Name = "chkAddSpaces";
            chkAddSpaces.Size = new System.Drawing.Size(87, 19);
            chkAddSpaces.TabIndex = 8;
            chkAddSpaces.Text = "Add Spaces";
            chkAddSpaces.UseVisualStyleBackColor = true;
            // 
            // BtnConvertParagraphsToLines2WithoutComments
            // 
            BtnConvertParagraphsToLines2WithoutComments.Location = new System.Drawing.Point(12, 480);
            BtnConvertParagraphsToLines2WithoutComments.Name = "BtnConvertParagraphsToLines2WithoutComments";
            BtnConvertParagraphsToLines2WithoutComments.Size = new System.Drawing.Size(75, 23);
            BtnConvertParagraphsToLines2WithoutComments.TabIndex = 9;
            BtnConvertParagraphsToLines2WithoutComments.Text = "Convert";
            BtnConvertParagraphsToLines2WithoutComments.UseVisualStyleBackColor = true;
            BtnConvertParagraphsToLines2WithoutComments.Click += BtnConvertParagraphsToLines2WithoutComments_Click;
            // 
            // BtnConvertWithBlocks
            // 
            BtnConvertWithBlocks.Location = new System.Drawing.Point(93, 480);
            BtnConvertWithBlocks.Name = "BtnConvertWithBlocks";
            BtnConvertWithBlocks.Size = new System.Drawing.Size(150, 23);
            BtnConvertWithBlocks.TabIndex = 10;
            BtnConvertWithBlocks.Text = "Convert Block Size 8000";
            BtnConvertWithBlocks.UseVisualStyleBackColor = true;
            BtnConvertWithBlocks.Click += BtnConvertWithBlocks_Click;
            // 
            // chkBlockSize4000
            // 
            chkBlockSize4000.AutoSize = true;
            chkBlockSize4000.Location = new System.Drawing.Point(249, 483);
            chkBlockSize4000.Name = "chkBlockSize4000";
            chkBlockSize4000.Size = new System.Drawing.Size(105, 19);
            chkBlockSize4000.TabIndex = 11;
            chkBlockSize4000.Text = "Block Size 4000";
            chkBlockSize4000.UseVisualStyleBackColor = true;
            // 
            // lblBlockCount
            // 
            lblBlockCount.AutoSize = true;
            lblBlockCount.Location = new System.Drawing.Point(425, 484);
            lblBlockCount.Name = "lblBlockCount";
            lblBlockCount.Size = new System.Drawing.Size(72, 15);
            lblBlockCount.TabIndex = 12;
            lblBlockCount.Text = "Block Count";
            // 
            // LineConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1017, 508);
            Controls.Add(lblBlockCount);
            Controls.Add(chkBlockSize4000);
            Controls.Add(BtnConvertWithBlocks);
            Controls.Add(BtnConvertParagraphsToLines2WithoutComments);
            Controls.Add(chkAddSpaces);
            Controls.Add(label1);
            Controls.Add(BtnRestore);
            Controls.Add(BtnCopy);
            Controls.Add(BtnClear);
            Controls.Add(lbCounter);
            Controls.Add(BtnConvert2);
            Controls.Add(BtnConvert);
            Controls.Add(TextBoxInputOutput);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "LineConverterForm";
            Text = "Paragraph-to-Line Converter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxInputOutput;
        private System.Windows.Forms.Button BtnConvert;
        private System.Windows.Forms.Button BtnConvert2;
        private System.Windows.Forms.Label lbCounter;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button BtnCopy;
        private System.Windows.Forms.Button BtnRestore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkAddSpaces;
        private System.Windows.Forms.Button BtnConvertParagraphsToLines2WithoutComments;
        private System.Windows.Forms.Button BtnConvertWithBlocks;
        private System.Windows.Forms.CheckBox chkBlockSize4000;
        private System.Windows.Forms.Label lblBlockCount;
    }
}
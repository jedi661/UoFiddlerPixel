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
            SuspendLayout();
            // 
            // TextBoxInputOutput
            // 
            TextBoxInputOutput.Location = new System.Drawing.Point(12, 89);
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
            lbCounter.Location = new System.Drawing.Point(336, 426);
            lbCounter.Name = "lbCounter";
            lbCounter.Size = new System.Drawing.Size(40, 15);
            lbCounter.TabIndex = 3;
            lbCounter.Text = "Count";
            // 
            // BtnClear
            // 
            BtnClear.Location = new System.Drawing.Point(93, 422);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(75, 23);
            BtnClear.TabIndex = 4;
            BtnClear.Text = "Clear";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
            // 
            // BtnCopy
            // 
            BtnCopy.Location = new System.Drawing.Point(174, 422);
            BtnCopy.Name = "BtnCopy";
            BtnCopy.Size = new System.Drawing.Size(75, 23);
            BtnCopy.TabIndex = 5;
            BtnCopy.Text = "Copy";
            BtnCopy.UseVisualStyleBackColor = true;
            BtnCopy.Click += BtnCopy_Click;
            // 
            // BtnRestore
            // 
            BtnRestore.Location = new System.Drawing.Point(255, 422);
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
            // LineConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1017, 482);
            Controls.Add(label1);
            Controls.Add(BtnRestore);
            Controls.Add(BtnCopy);
            Controls.Add(BtnClear);
            Controls.Add(lbCounter);
            Controls.Add(BtnConvert2);
            Controls.Add(BtnConvert);
            Controls.Add(TextBoxInputOutput);
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
    }
}
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
    partial class MultiScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiScript));
            tbNameScript = new System.Windows.Forms.TextBox();
            tbIndexImage = new System.Windows.Forms.TextBox();
            btCreateScript = new System.Windows.Forms.Button();
            tbscriptFinish = new System.Windows.Forms.TextBox();
            btnSaveScript = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // tbNameScript
            // 
            tbNameScript.Location = new System.Drawing.Point(303, 55);
            tbNameScript.Name = "tbNameScript";
            tbNameScript.Size = new System.Drawing.Size(100, 23);
            tbNameScript.TabIndex = 0;
            // 
            // tbIndexImage
            // 
            tbIndexImage.Location = new System.Drawing.Point(14, 34);
            tbIndexImage.Multiline = true;
            tbIndexImage.Name = "tbIndexImage";
            tbIndexImage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbIndexImage.Size = new System.Drawing.Size(275, 391);
            tbIndexImage.TabIndex = 1;
            tbIndexImage.TextChanged += tbIndexImage_TextChanged;
            // 
            // btCreateScript
            // 
            btCreateScript.Location = new System.Drawing.Point(303, 84);
            btCreateScript.Name = "btCreateScript";
            btCreateScript.Size = new System.Drawing.Size(100, 23);
            btCreateScript.TabIndex = 2;
            btCreateScript.Text = "Create Script";
            btCreateScript.UseVisualStyleBackColor = true;
            btCreateScript.Click += btCreateScript_Click;
            // 
            // tbscriptFinish
            // 
            tbscriptFinish.Location = new System.Drawing.Point(447, 34);
            tbscriptFinish.Multiline = true;
            tbscriptFinish.Name = "tbscriptFinish";
            tbscriptFinish.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tbscriptFinish.Size = new System.Drawing.Size(275, 391);
            tbscriptFinish.TabIndex = 3;
            // 
            // btnSaveScript
            // 
            btnSaveScript.Location = new System.Drawing.Point(303, 113);
            btnSaveScript.Name = "btnSaveScript";
            btnSaveScript.Size = new System.Drawing.Size(100, 23);
            btnSaveScript.TabIndex = 4;
            btnSaveScript.Text = "Save Script";
            btnSaveScript.UseVisualStyleBackColor = true;
            btnSaveScript.Click += btnSaveScript_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(303, 37);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(45, 15);
            label1.TabIndex = 5;
            label1.Text = "Name :";
            // 
            // MultiScript
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(738, 450);
            Controls.Add(label1);
            Controls.Add(btnSaveScript);
            Controls.Add(tbscriptFinish);
            Controls.Add(btCreateScript);
            Controls.Add(tbIndexImage);
            Controls.Add(tbNameScript);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "MultiScript";
            Text = "Multi Scripter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox tbNameScript;
        private System.Windows.Forms.TextBox tbIndexImage;
        private System.Windows.Forms.Button btCreateScript;
        private System.Windows.Forms.TextBox tbscriptFinish;
        private System.Windows.Forms.Button btnSaveScript;
        private System.Windows.Forms.Label label1;
    }
}
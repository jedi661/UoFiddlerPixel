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
    partial class EditUoBodyconvMobtypes
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
            btLoadBodyconv = new System.Windows.Forms.Button();
            textBoxEdit = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // btLoadBodyconv
            // 
            btLoadBodyconv.Location = new System.Drawing.Point(12, 29);
            btLoadBodyconv.Name = "btLoadBodyconv";
            btLoadBodyconv.Size = new System.Drawing.Size(75, 23);
            btLoadBodyconv.TabIndex = 0;
            btLoadBodyconv.Text = "button1";
            btLoadBodyconv.UseVisualStyleBackColor = true;
            btLoadBodyconv.Click += btLoadBodyconv_Click;
            // 
            // textBoxEdit
            // 
            textBoxEdit.Location = new System.Drawing.Point(268, 29);
            textBoxEdit.Multiline = true;
            textBoxEdit.Name = "textBoxEdit";
            textBoxEdit.Size = new System.Drawing.Size(505, 367);
            textBoxEdit.TabIndex = 1;
            // 
            // EditUoBodyconvMobtypes
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(textBoxEdit);
            Controls.Add(btLoadBodyconv);
            Name = "EditUoBodyconvMobtypes";
            Text = "EditUoBodyconvMobtypes";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btLoadBodyconv;
        private System.Windows.Forms.TextBox textBoxEdit;
    }
}
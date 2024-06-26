﻿// /***************************************************************************
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
    partial class DecriptClientForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecriptClientForm));
            btDecriptClientForm = new System.Windows.Forms.Button();
            LAB_StatusIS = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            lbMessagePos = new System.Windows.Forms.Label();
            lbMessagePosOld = new System.Windows.Forms.Label();
            lbMessagePos09 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // btDecriptClientForm
            // 
            btDecriptClientForm.Location = new System.Drawing.Point(78, 294);
            btDecriptClientForm.Name = "btDecriptClientForm";
            btDecriptClientForm.Size = new System.Drawing.Size(100, 23);
            btDecriptClientForm.TabIndex = 0;
            btDecriptClientForm.Text = "Decrypt Client.";
            btDecriptClientForm.UseVisualStyleBackColor = true;
            btDecriptClientForm.Click += btDecriptClientForm_Click_1;
            // 
            // LAB_StatusIS
            // 
            LAB_StatusIS.AutoSize = true;
            LAB_StatusIS.Location = new System.Drawing.Point(3, 272);
            LAB_StatusIS.Name = "LAB_StatusIS";
            LAB_StatusIS.Size = new System.Drawing.Size(42, 15);
            LAB_StatusIS.TabIndex = 1;
            LAB_StatusIS.Text = "Status:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(3, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(261, 150);
            label1.TabIndex = 2;
            label1.Text = resources.GetString("label1.Text");
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // lbMessagePos
            // 
            lbMessagePos.AutoSize = true;
            lbMessagePos.Location = new System.Drawing.Point(7, 182);
            lbMessagePos.Name = "lbMessagePos";
            lbMessagePos.Size = new System.Drawing.Size(72, 15);
            lbMessagePos.TabIndex = 3;
            lbMessagePos.Text = "MessagePos";
            // 
            // lbMessagePosOld
            // 
            lbMessagePosOld.AutoSize = true;
            lbMessagePosOld.Location = new System.Drawing.Point(7, 209);
            lbMessagePosOld.Name = "lbMessagePosOld";
            lbMessagePosOld.Size = new System.Drawing.Size(91, 15);
            lbMessagePosOld.TabIndex = 4;
            lbMessagePosOld.Text = "MessagePosOld";
            // 
            // lbMessagePos09
            // 
            lbMessagePos09.AutoSize = true;
            lbMessagePos09.Location = new System.Drawing.Point(7, 241);
            lbMessagePos09.Name = "lbMessagePos09";
            lbMessagePos09.Size = new System.Drawing.Size(84, 15);
            lbMessagePos09.TabIndex = 5;
            lbMessagePos09.Text = "MessagePos09";
            // 
            // DecriptClientForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(268, 329);
            Controls.Add(lbMessagePos09);
            Controls.Add(lbMessagePosOld);
            Controls.Add(lbMessagePos);
            Controls.Add(label1);
            Controls.Add(LAB_StatusIS);
            Controls.Add(btDecriptClientForm);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "DecriptClientForm";
            Text = "Decript Client";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btDecriptClientForm;
        private System.Windows.Forms.Label LAB_StatusIS;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lbMessagePos;
        private System.Windows.Forms.Label lbMessagePosOld;
        private System.Windows.Forms.Label lbMessagePos09;
    }
}
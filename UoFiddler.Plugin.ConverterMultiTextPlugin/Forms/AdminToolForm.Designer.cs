/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class AdminToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdminToolForm));
            BtnPing = new System.Windows.Forms.Button();
            labelIP = new System.Windows.Forms.Label();
            textBoxAdress = new System.Windows.Forms.TextBox();
            textBoxPingAusgabe = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            BtnTracert = new System.Windows.Forms.Button();
            BtnCopyIP = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // BtnPing
            // 
            BtnPing.Location = new System.Drawing.Point(19, 17);
            BtnPing.Name = "BtnPing";
            BtnPing.Size = new System.Drawing.Size(75, 23);
            BtnPing.TabIndex = 0;
            BtnPing.Text = "Ping";
            BtnPing.UseVisualStyleBackColor = true;
            BtnPing.Click += BtnPing_Click;
            // 
            // labelIP
            // 
            labelIP.AutoSize = true;
            labelIP.Location = new System.Drawing.Point(188, 261);
            labelIP.Name = "labelIP";
            labelIP.Size = new System.Drawing.Size(17, 15);
            labelIP.TabIndex = 1;
            labelIP.Text = "IP";
            // 
            // textBoxAdress
            // 
            textBoxAdress.Location = new System.Drawing.Point(110, 17);
            textBoxAdress.Name = "textBoxAdress";
            textBoxAdress.Size = new System.Drawing.Size(100, 23);
            textBoxAdress.TabIndex = 2;
            textBoxAdress.KeyDown += TextBoxAdress_KeyDown;
            // 
            // textBoxPingAusgabe
            // 
            textBoxPingAusgabe.Location = new System.Drawing.Point(110, 52);
            textBoxPingAusgabe.Multiline = true;
            textBoxPingAusgabe.Name = "textBoxPingAusgabe";
            textBoxPingAusgabe.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxPingAusgabe.Size = new System.Drawing.Size(262, 200);
            textBoxPingAusgabe.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(110, 261);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(72, 15);
            label2.TabIndex = 4;
            label2.Text = "Last Adress :";
            // 
            // BtnTracert
            // 
            BtnTracert.Location = new System.Drawing.Point(19, 52);
            BtnTracert.Name = "BtnTracert";
            BtnTracert.Size = new System.Drawing.Size(75, 23);
            BtnTracert.TabIndex = 5;
            BtnTracert.Text = "Tracert";
            BtnTracert.UseVisualStyleBackColor = true;
            BtnTracert.Click += BtnTracert_Click;
            // 
            // BtnCopyIP
            // 
            BtnCopyIP.Location = new System.Drawing.Point(12, 257);
            BtnCopyIP.Name = "BtnCopyIP";
            BtnCopyIP.Size = new System.Drawing.Size(75, 23);
            BtnCopyIP.TabIndex = 6;
            BtnCopyIP.Text = "Clippbord";
            BtnCopyIP.UseVisualStyleBackColor = true;
            BtnCopyIP.Click += BtnCopyIP_Click;
            // 
            // AdminToolForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(384, 285);
            Controls.Add(BtnCopyIP);
            Controls.Add(BtnTracert);
            Controls.Add(label2);
            Controls.Add(textBoxPingAusgabe);
            Controls.Add(textBoxAdress);
            Controls.Add(labelIP);
            Controls.Add(BtnPing);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AdminToolForm";
            Text = "AdminTool";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button BtnPing;
        private System.Windows.Forms.Label labelIP;
        private System.Windows.Forms.TextBox textBoxAdress;
        private System.Windows.Forms.TextBox textBoxPingAusgabe;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnTracert;
        private System.Windows.Forms.Button BtnCopyIP;
    }
}
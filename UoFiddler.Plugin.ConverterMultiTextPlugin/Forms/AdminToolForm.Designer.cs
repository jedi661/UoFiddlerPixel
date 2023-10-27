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
            btnPing = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            textBoxAdress = new System.Windows.Forms.TextBox();
            textBoxPingAusgabe = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            btnTracert = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnPing
            // 
            btnPing.Location = new System.Drawing.Point(19, 17);
            btnPing.Name = "btnPing";
            btnPing.Size = new System.Drawing.Size(75, 23);
            btnPing.TabIndex = 0;
            btnPing.Text = "Ping";
            btnPing.UseVisualStyleBackColor = true;
            btnPing.Click += btnPing_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(188, 261);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 15);
            label1.TabIndex = 1;
            label1.Text = "label1";
            // 
            // textBoxAdress
            // 
            textBoxAdress.Location = new System.Drawing.Point(110, 17);
            textBoxAdress.Name = "textBoxAdress";
            textBoxAdress.Size = new System.Drawing.Size(100, 23);
            textBoxAdress.TabIndex = 2;
            textBoxAdress.KeyDown += textBoxAdress_KeyDown;
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
            // btnTracert
            // 
            btnTracert.Location = new System.Drawing.Point(19, 52);
            btnTracert.Name = "btnTracert";
            btnTracert.Size = new System.Drawing.Size(75, 23);
            btnTracert.TabIndex = 5;
            btnTracert.Text = "Tracert";
            btnTracert.UseVisualStyleBackColor = true;
            btnTracert.Click += btnTracert_Click;
            // 
            // AdminToolForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(384, 285);
            Controls.Add(btnTracert);
            Controls.Add(label2);
            Controls.Add(textBoxPingAusgabe);
            Controls.Add(textBoxAdress);
            Controls.Add(label1);
            Controls.Add(btnPing);
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

        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxAdress;
        private System.Windows.Forms.TextBox textBoxPingAusgabe;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTracert;
    }
}
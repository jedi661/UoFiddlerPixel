// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Helper
{
    partial class RStaticZoom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RStaticZoom));
            Panel2 = new System.Windows.Forms.Panel();
            VScrollBar1 = new System.Windows.Forms.VScrollBar();
            Panel2.SuspendLayout();
            SuspendLayout();
            // 
            // Panel2
            // 
            Panel2.Controls.Add(VScrollBar1);
            Panel2.Location = new System.Drawing.Point(1, 0);
            Panel2.Name = "Panel2";
            Panel2.Size = new System.Drawing.Size(789, 607);
            Panel2.TabIndex = 0;
            Panel2.Paint += Panel2_Paint;
            Panel2.MouseDown += Panel2_MouseDown;
            // 
            // VScrollBar1
            // 
            VScrollBar1.Location = new System.Drawing.Point(761, 0);
            VScrollBar1.Maximum = 65500;
            VScrollBar1.Name = "VScrollBar1";
            VScrollBar1.Size = new System.Drawing.Size(28, 610);
            VScrollBar1.TabIndex = 1;
            VScrollBar1.Scroll += VScrollBar1_Scroll;
            // 
            // RStaticZoom
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(789, 604);
            Controls.Add(Panel2);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "RStaticZoom";
            Text = "Select Static Item";
            Panel2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel Panel2;
        private System.Windows.Forms.VScrollBar VScrollBar1;
    }
}
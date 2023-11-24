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
    partial class StaticZoom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StaticZoom));
            Panel2 = new System.Windows.Forms.Panel();
            VScrollBar1 = new System.Windows.Forms.VScrollBar();
            PictureBox1 = new System.Windows.Forms.PictureBox();
            Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureBox1).BeginInit();
            SuspendLayout();
            // 
            // Panel2
            // 
            Panel2.Controls.Add(VScrollBar1);
            Panel2.Location = new System.Drawing.Point(5, 5);
            Panel2.Name = "Panel2";
            Panel2.Size = new System.Drawing.Size(789, 607);
            Panel2.TabIndex = 2;
            Panel2.Paint += Panel2_Paint;
            Panel2.MouseDown += Panel2_MouseDown;
            // 
            // VScrollBar1
            // 
            VScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            VScrollBar1.Location = new System.Drawing.Point(772, 0);
            VScrollBar1.Maximum = 65500;
            VScrollBar1.Name = "VScrollBar1";
            VScrollBar1.Size = new System.Drawing.Size(17, 607);
            VScrollBar1.TabIndex = 0;
            VScrollBar1.Scroll += VScrollBar1_Scroll;
            // 
            // PictureBox1
            // 
            PictureBox1.Location = new System.Drawing.Point(800, 5);
            PictureBox1.Name = "PictureBox1";
            PictureBox1.Size = new System.Drawing.Size(233, 607);
            PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            PictureBox1.TabIndex = 3;
            PictureBox1.TabStop = false;
            // 
            // StaticZoom
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1036, 616);
            Controls.Add(PictureBox1);
            Controls.Add(Panel2);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "StaticZoom";
            Text = "Static Tiles";
            Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)PictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel Panel2;
        private System.Windows.Forms.VScrollBar VScrollBar1;
        private System.Windows.Forms.PictureBox PictureBox1;
    }
}
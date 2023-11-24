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
    partial class MapZoom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapZoom));
            Panel1 = new System.Windows.Forms.Panel();
            VScrollBar1 = new System.Windows.Forms.VScrollBar();
            Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Panel1
            // 
            Panel1.Controls.Add(VScrollBar1);
            Panel1.Location = new System.Drawing.Point(8, 8);
            Panel1.Name = "Panel1";
            Panel1.Size = new System.Drawing.Size(318, 480);
            Panel1.TabIndex = 0;
            Panel1.Paint += Panel1_Paint;
            Panel1.MouseDown += Panel1_MouseDown;
            // 
            // VScrollBar1
            // 
            VScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
            VScrollBar1.Location = new System.Drawing.Point(301, 0);
            VScrollBar1.Maximum = 16383;
            VScrollBar1.Name = "VScrollBar1";
            VScrollBar1.Size = new System.Drawing.Size(17, 480);
            VScrollBar1.TabIndex = 0;
            VScrollBar1.Value = 16383;
            VScrollBar1.Scroll += VScrollBar1_Scroll;
            // 
            // MapZoom
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(332, 498);
            Controls.Add(Panel1);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "MapZoom";
            Text = "Map Tiles";
            Panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel Panel1;
        private System.Windows.Forms.VScrollBar VScrollBar1;
    }
}
namespace Logger
{
    partial class LoggerForm
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
            this.TextLog = new System.Windows.Forms.TextBox();
            this.MainMenu1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem2 = new System.Windows.Forms.ToolStripButton();
            this.MenuItem3 = new System.Windows.Forms.ToolStripButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.MainMenu1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // TextLog
            // 
            this.TextLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.TextLog.Location = new System.Drawing.Point(0, 31);
            this.TextLog.Multiline = true;
            this.TextLog.Name = "TextLog";
            this.TextLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TextLog.Size = new System.Drawing.Size(404, 256);
            this.TextLog.TabIndex = 0;
            // 
            // MainMenu1
            // 
            this.MainMenu1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.MainMenu1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.MainMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.MainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.MenuItem2,
            this.MenuItem3});
            this.MainMenu1.Location = new System.Drawing.Point(0, 0);
            this.MainMenu1.Name = "MainMenu1";
            this.MainMenu1.Size = new System.Drawing.Size(404, 27);
            this.MainMenu1.TabIndex = 2;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // MenuItem2
            // 
            this.MenuItem2.Image = global::Logger.Properties.Resources.uomc30;
            this.MenuItem2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuItem2.Name = "MenuItem2";
            this.MenuItem2.Size = new System.Drawing.Size(34, 24);
            this.MenuItem2.Text = " ";
            this.MenuItem2.Click += new System.EventHandler(this.MenuItem2_Click);
            // 
            // MenuItem3
            // 
            this.MenuItem3.Image = global::Logger.Properties.Resources.uomc31;
            this.MenuItem3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuItem3.Name = "MenuItem3";
            this.MenuItem3.Size = new System.Drawing.Size(34, 24);
            this.MenuItem3.Text = " ";
            this.MenuItem3.Click += new System.EventHandler(this.MenuItem3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Logger.Properties.Resources.uomc32;
            this.pictureBox1.Location = new System.Drawing.Point(364, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(20, 20);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // LoggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 287);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.MainMenu1);
            this.Controls.Add(this.TextLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LoggerForm";
            this.Text = "Ultima Online™ Map Creator Log ";
            this.MainMenu1.ResumeLayout(false);
            this.MainMenu1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextLog;
        private System.Windows.Forms.ToolStrip MainMenu1;
        private System.Windows.Forms.ToolStripButton MenuItem3;
        private System.Windows.Forms.ToolStripButton MenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
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

namespace UoFiddler.Plugin.DesignStudio.Forms
{
    partial class DesignStudioForm
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
            this.gumpStudio = new UoFiddler.Plugin.DesignStudio.GumpStudio.GumpStudioControl();
            this.SuspendLayout();
            // 
            // gumpStudio
            // 
            this.gumpStudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gumpStudio.Location = new System.Drawing.Point(0, 0);
            this.gumpStudio.Name = "gumpStudio";
            this.gumpStudio.Size = new System.Drawing.Size(1300, 820);
            this.gumpStudio.TabIndex = 0;
            // 
            // DesignStudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1300, 820);
            this.Controls.Add(this.gumpStudio);
            this.MinimumSize = new System.Drawing.Size(1000, 650);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DesignStudioForm";
            this.Text = "Gump Studio";
            this.ResumeLayout(false);

        }

        #endregion

        private UoFiddler.Plugin.DesignStudio.GumpStudio.GumpStudioControl gumpStudio;
    }
}
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

namespace UoFiddler.Plugin.DesignStudio.UserControls
{
    partial class DesignStudioControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gumpStudio = new UoFiddler.Plugin.DesignStudio.GumpStudio.GumpStudioControl();
            SuspendLayout();
            // 
            // gumpStudio
            // 
            gumpStudio.Dock = System.Windows.Forms.DockStyle.Fill;
            gumpStudio.Location = new System.Drawing.Point(0, 0);
            gumpStudio.Name = "gumpStudio";
            gumpStudio.Size = new System.Drawing.Size(854, 675);
            gumpStudio.TabIndex = 0;
            // 
            // DesignStudioControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Controls.Add(gumpStudio);
            Name = "DesignStudioControl";
            Size = new System.Drawing.Size(854, 675);
            ResumeLayout(false);
        }

        #endregion

        private UoFiddler.Plugin.DesignStudio.GumpStudio.GumpStudioControl gumpStudio;
    }
}

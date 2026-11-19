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

namespace UoFiddler.Controls.UserControls
{
    partial class RadarColorControl
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
            if (disposing)
            {
                components?.Dispose();
                DisposeOwnedImages();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// The layout is built in code (BuildLayout in RadarColorControl.cs): a virtual list instead of tree views,
        /// so the tab opens instantly even with 1,000,000+ items.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "RadarColorControl";
            Size = new System.Drawing.Size(900, 560);
            Load += OnLoad;
            ResumeLayout(false);
        }

        #endregion
    }
}

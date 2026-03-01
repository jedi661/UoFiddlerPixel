// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class IsoTiloSlicerHelp
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            rtbHelp = new System.Windows.Forms.RichTextBox();
            btnClose = new System.Windows.Forms.Button();
            SuspendLayout();

            // rtbHelp
            rtbHelp.Location = new System.Drawing.Point(12, 12);
            rtbHelp.Size = new System.Drawing.Size(560, 460);
            rtbHelp.Name = "rtbHelp";
            rtbHelp.ReadOnly = true;
            rtbHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbHelp.BackColor = System.Drawing.SystemColors.Window;
            rtbHelp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            rtbHelp.TabStop = false;

            // btnClose
            btnClose.Text = "Close";
            btnClose.Location = new System.Drawing.Point(472, 484);
            btnClose.Size = new System.Drawing.Size(100, 28);
            btnClose.TabIndex = 0;
            btnClose.Name = "btnClose";
            btnClose.Click += BtnClose_Click;

            // Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 524);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "IsoTiloSlicerHelp";
            Text = "IsoTiloSlicer - Help";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;

            Controls.Add(rtbHelp);
            Controls.Add(btnClose);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbHelp;
        private System.Windows.Forms.Button btnClose;
    }
}
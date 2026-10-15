// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    partial class TerrainHelpForm
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
            richTextBoxHelp = new System.Windows.Forms.RichTextBox();
            panelBottom = new System.Windows.Forms.Panel();
            buttonClose = new System.Windows.Forms.Button();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // richTextBoxHelp
            //
            richTextBoxHelp.BackColor = System.Drawing.SystemColors.Window;
            richTextBoxHelp.BorderStyle = System.Windows.Forms.BorderStyle.None;
            richTextBoxHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxHelp.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            richTextBoxHelp.Location = new System.Drawing.Point(10, 10);
            richTextBoxHelp.Name = "richTextBoxHelp";
            richTextBoxHelp.ReadOnly = true;
            richTextBoxHelp.Size = new System.Drawing.Size(744, 541);
            richTextBoxHelp.TabIndex = 0;
            richTextBoxHelp.Text = "";
            //
            // panelBottom
            //
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Location = new System.Drawing.Point(10, 551);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new System.Drawing.Size(744, 40);
            panelBottom.TabIndex = 1;
            //
            // buttonClose
            //
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Location = new System.Drawing.Point(659, 8);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(85, 27);
            buttonClose.TabIndex = 0;
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // TerrainHelpForm
            //
            AcceptButton = buttonClose;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Window;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(764, 601);
            Controls.Add(richTextBoxHelp);
            Controls.Add(panelBottom);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(500, 350);
            Name = "TerrainHelpForm";
            Padding = new System.Windows.Forms.Padding(10);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Hilfe - Terrain-Bibliothek (LandTiles und Texturen)";
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxHelp;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonClose;
    }
}

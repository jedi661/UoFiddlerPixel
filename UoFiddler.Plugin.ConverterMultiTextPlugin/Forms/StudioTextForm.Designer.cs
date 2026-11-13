/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class StudioTextForm
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
            components = new System.ComponentModel.Container();
            txtContent = new System.Windows.Forms.TextBox();
            pnlButtons = new System.Windows.Forms.Panel();
            btnCopy = new System.Windows.Forms.Button();
            btnSave = new System.Windows.Forms.Button();
            btnAppend = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // txtContent
            // 
            txtContent.Dock = System.Windows.Forms.DockStyle.Fill;
            txtContent.Font = new System.Drawing.Font("Consolas", 9.75F);
            txtContent.Location = new System.Drawing.Point(0, 0);
            txtContent.Multiline = true;
            txtContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtContent.Size = new System.Drawing.Size(684, 421);
            txtContent.WordWrap = false;
            txtContent.Name = "txtContent";
            txtContent.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCopy);
            pnlButtons.Controls.Add(btnSave);
            pnlButtons.Controls.Add(btnAppend);
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.Location = new System.Drawing.Point(0, 421);
            pnlButtons.Size = new System.Drawing.Size(684, 40);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.TabIndex = 1;
            // 
            // btnCopy
            // 
            btnCopy.Location = new System.Drawing.Point(8, 7);
            btnCopy.Size = new System.Drawing.Size(110, 26);
            btnCopy.Text = "Kopieren";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Name = "btnCopy";
            btnCopy.TabIndex = 2;
            btnCopy.Click += BtnCopy_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new System.Drawing.Point(124, 7);
            btnSave.Size = new System.Drawing.Size(130, 26);
            btnSave.Text = "Speichern unter...";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Name = "btnSave";
            btnSave.TabIndex = 3;
            btnSave.Click += BtnSave_Click;
            // 
            // btnAppend
            // 
            btnAppend.Location = new System.Drawing.Point(260, 7);
            btnAppend.Size = new System.Drawing.Size(140, 26);
            btnAppend.Text = "An Datei anhängen...";
            btnAppend.UseVisualStyleBackColor = true;
            btnAppend.Name = "btnAppend";
            btnAppend.TabIndex = 4;
            btnAppend.Click += BtnAppend_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(574, 7);
            btnClose.Size = new System.Drawing.Size(100, 26);
            btnClose.Text = "Schließen";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 5;
            btnClose.Click += BtnClose_Click;
            // 
            // StudioTextForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(684, 461);
            Controls.Add(txtContent);
            Controls.Add(pnlButtons);
            MinimumSize = new System.Drawing.Size(400, 300);
            Name = "StudioTextForm";
            Text = "Text";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            pnlButtons.ResumeLayout(false);
            pnlButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAppend;
        private System.Windows.Forms.Button btnClose;
    }
}

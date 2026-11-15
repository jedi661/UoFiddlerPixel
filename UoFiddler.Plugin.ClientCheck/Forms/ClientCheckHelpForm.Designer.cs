namespace UoFiddler.Plugin.ClientCheck.Forms
{
    partial class ClientCheckHelpForm
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
            this.hostPanel = new System.Windows.Forms.Panel();
            this._text = new System.Windows.Forms.RichTextBox();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnFolder = new System.Windows.Forms.Button();
            this._cmbLang = new System.Windows.Forms.ComboBox();
            this.hostPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // hostPanel
            // 
            this.hostPanel.BackColor = System.Drawing.Color.White;
            this.hostPanel.Controls.Add(this._text);
            this.hostPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostPanel.Location = new System.Drawing.Point(0, 0);
            this.hostPanel.Name = "hostPanel";
            this.hostPanel.Padding = new System.Windows.Forms.Padding(14, 10, 8, 4);
            this.hostPanel.Size = new System.Drawing.Size(804, 640);
            this.hostPanel.TabIndex = 0;
            // 
            // _text
            // 
            this._text.BackColor = System.Drawing.Color.White;
            this._text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._text.DetectUrls = false;
            this._text.Dock = System.Windows.Forms.DockStyle.Fill;
            this._text.Location = new System.Drawing.Point(14, 10);
            this._text.Name = "_text";
            this._text.ReadOnly = true;
            this._text.Size = new System.Drawing.Size(782, 626);
            this._text.TabIndex = 0;
            this._text.Text = "";
            // 
            // buttonPanel
            // 
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.Controls.Add(this._btnClose);
            this.buttonPanel.Controls.Add(this._btnFolder);
            this.buttonPanel.Controls.Add(this._cmbLang);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Location = new System.Drawing.Point(0, 640);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Padding = new System.Windows.Forms.Padding(6);
            this.buttonPanel.Size = new System.Drawing.Size(804, 41);
            this.buttonPanel.TabIndex = 1;
            // 
            // _btnClose
            // 
            this._btnClose.AutoSize = true;
            this._btnClose.Location = new System.Drawing.Point(714, 9);
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(75, 25);
            this._btnClose.TabIndex = 0;
            this._btnClose.Text = "OK";
            this._btnClose.UseVisualStyleBackColor = true;
            this._btnClose.Click += new System.EventHandler(this.OnClickClose);
            // 
            // _btnFolder
            // 
            this._btnFolder.AutoSize = true;
            this._btnFolder.Location = new System.Drawing.Point(633, 9);
            this._btnFolder.Name = "_btnFolder";
            this._btnFolder.Size = new System.Drawing.Size(75, 25);
            this._btnFolder.TabIndex = 1;
            this._btnFolder.Text = "📁";
            this._btnFolder.UseVisualStyleBackColor = true;
            this._btnFolder.Click += new System.EventHandler(this.OnClickFolder);
            // 
            // _cmbLang
            // 
            this._cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbLang.FormattingEnabled = true;
            this._cmbLang.Items.AddRange(new object[] {
            "Deutsch",
            "English",
            "Français"});
            this._cmbLang.Location = new System.Drawing.Point(517, 9);
            this._cmbLang.Name = "_cmbLang";
            this._cmbLang.Size = new System.Drawing.Size(110, 25);
            this._cmbLang.TabIndex = 2;
            this._cmbLang.SelectedIndexChanged += new System.EventHandler(this.OnLanguageSelected);
            // 
            // ClientCheckHelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 681);
            this.Controls.Add(this.hostPanel);
            this.Controls.Add(this.buttonPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "ClientCheckHelpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Client-Prüfung – Hilfe";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosedHelp);
            this.hostPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel hostPanel;
        private System.Windows.Forms.RichTextBox _text;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnFolder;
        private System.Windows.Forms.ComboBox _cmbLang;
    }
}

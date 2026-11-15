namespace UoFiddler.Controls.Forms
{
    partial class TileDataHelpForm
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
            this.components = new System.ComponentModel.Container();
            this._text = new System.Windows.Forms.RichTextBox();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._cmbLang = new System.Windows.Forms.ComboBox();
            this._lblLang = new System.Windows.Forms.Label();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _text
            // 
            this._text.BackColor = System.Drawing.SystemColors.Window;
            this._text.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._text.Dock = System.Windows.Forms.DockStyle.Fill;
            this._text.Name = "_text";
            this._text.ReadOnly = true;
            this._text.TabIndex = 0;
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnClose);
            this._bottom.Controls.Add(this._cmbLang);
            this._bottom.Controls.Add(this._lblLang);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(6);
            this._bottom.TabIndex = 1;
            // 
            // _btnClose
            // 
            this._btnClose.AutoSize = true;
            this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 27);
            this._btnClose.TabIndex = 2;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            // 
            // _cmbLang
            // 
            this._cmbLang.Items.AddRange(new object[] {
            "Deutsch",
            "English",
            "Français"});
            this._cmbLang.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbLang.Name = "_cmbLang";
            this._cmbLang.Size = new System.Drawing.Size(120, 23);
            this._cmbLang.TabIndex = 3;
            this._cmbLang.SelectedIndexChanged += new System.EventHandler(this.OnLanguageSelected);
            // 
            // _lblLang
            // 
            this._lblLang.AutoSize = true;
            this._lblLang.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._lblLang.Name = "_lblLang";
            this._lblLang.Text = "Language:";
            // 
            // TileDataHelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(800, 640);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(480, 360);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TileData – help";
            this.Controls.Add(this._text);
            this.Controls.Add(this._bottom);
            this.Name = "TileDataHelpForm";
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox _text;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.ComboBox _cmbLang;
        private System.Windows.Forms.Label _lblLang;
    }
}

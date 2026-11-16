namespace UoFiddler.Controls.Forms
{
    partial class LightCheckForm
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
            this._list = new System.Windows.Forms.ListView();
            this._colId = new System.Windows.Forms.ColumnHeader();
            this._colKind = new System.Windows.Forms.ColumnHeader();
            this._colText = new System.Windows.Forms.ColumnHeader();
            this._lblSummary = new System.Windows.Forms.Label();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnShow = new System.Windows.Forms.Button();
            this._btnCopy = new System.Windows.Forms.Button();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _list
            // 
            this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colId,
            this._colKind,
            this._colText});
            this._list.Dock = System.Windows.Forms.DockStyle.Fill;
            this._list.FullRowSelect = true;
            this._list.GridLines = true;
            this._list.HideSelection = false;
            this._list.Name = "_list";
            this._list.TabIndex = 0;
            this._list.UseCompatibleStateImageBehavior = false;
            this._list.View = System.Windows.Forms.View.Details;
            this._list.DoubleClick += new System.EventHandler(this.OnClickShow);
            // 
            // _colId
            // 
            this._colId.Text = "Light";
            this._colId.Width = 120;
            // 
            // _colKind
            // 
            this._colKind.Text = "Kind";
            this._colKind.Width = 110;
            // 
            // _colText
            // 
            this._colText.Text = "Details";
            this._colText.Width = 420;
            // 
            // _lblSummary
            // 
            this._lblSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblSummary.Name = "_lblSummary";
            this._lblSummary.Padding = new System.Windows.Forms.Padding(6);
            this._lblSummary.Size = new System.Drawing.Size(680, 48);
            this._lblSummary.Text = "-";
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnClose);
            this._bottom.Controls.Add(this._btnShow);
            this._bottom.Controls.Add(this._btnCopy);
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
            // _btnShow
            // 
            this._btnShow.AutoSize = true;
            this._btnShow.Name = "_btnShow";
            this._btnShow.Size = new System.Drawing.Size(90, 27);
            this._btnShow.TabIndex = 3;
            this._btnShow.Text = "Show";
            this._btnShow.UseVisualStyleBackColor = true;
            this._btnShow.Click += new System.EventHandler(this.OnClickShow);
            // 
            // _btnCopy
            // 
            this._btnCopy.AutoSize = true;
            this._btnCopy.Name = "_btnCopy";
            this._btnCopy.Size = new System.Drawing.Size(90, 27);
            this._btnCopy.TabIndex = 4;
            this._btnCopy.Text = "Copy list";
            this._btnCopy.UseVisualStyleBackColor = true;
            this._btnCopy.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // LightCheckForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(680, 480);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(480, 300);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Light check";
            this.Controls.Add(this._list);
            this.Controls.Add(this._lblSummary);
            this.Controls.Add(this._bottom);
            this.Name = "LightCheckForm";
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader _colId;
        private System.Windows.Forms.ColumnHeader _colKind;
        private System.Windows.Forms.ColumnHeader _colText;
        private System.Windows.Forms.Label _lblSummary;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnShow;
        private System.Windows.Forms.Button _btnCopy;
    }
}

namespace UoFiddler.Controls.Forms
{
    partial class TileDataRangeForm
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
            this._tbl = new System.Windows.Forms.TableLayoutPanel();
            this._lblInfo = new System.Windows.Forms.Label();
            this._txtRange = new System.Windows.Forms.TextBox();
            this._chkAdd = new System.Windows.Forms.CheckBox();
            this._lblResult = new System.Windows.Forms.Label();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._btnApply = new System.Windows.Forms.Button();
            this._tbl.SuspendLayout();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tbl
            // 
            this._tbl.Controls.Add(this._lblInfo, 0, 0);
            this._tbl.Controls.Add(this._txtRange, 0, 1);
            this._tbl.Controls.Add(this._chkAdd, 0, 2);
            this._tbl.Controls.Add(this._lblResult, 0, 3);
            this._tbl.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this._tbl.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tbl.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tbl.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tbl.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._tbl.ColumnCount = 1;
            this._tbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._tbl.Name = "_tbl";
            this._tbl.Padding = new System.Windows.Forms.Padding(8);
            this._tbl.RowCount = 4;
            this._tbl.TabIndex = 0;
            // 
            // _lblInfo
            // 
            this._lblInfo.AutoSize = true;
            this._lblInfo.Name = "_lblInfo";
            this._lblInfo.Text = "IDs and ranges, decimal or hex, separated by commas:\r\ne.g. 0x0B2C-0x0B40, 0x0C00, 3000..3010";
            // 
            // _txtRange
            // 
            this._txtRange.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtRange.Name = "_txtRange";
            this._txtRange.TabIndex = 1;
            this._txtRange.TextChanged += new System.EventHandler(this.OnRangeChanged);
            // 
            // _chkAdd
            // 
            this._chkAdd.AutoSize = true;
            this._chkAdd.Name = "_chkAdd";
            this._chkAdd.TabIndex = 2;
            this._chkAdd.Text = "add to the current selection";
            this._chkAdd.UseVisualStyleBackColor = true;
            // 
            // _lblResult
            // 
            this._lblResult.AutoSize = true;
            this._lblResult.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblResult.Name = "_lblResult";
            this._lblResult.Text = "-";
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnClose);
            this._bottom.Controls.Add(this._btnApply);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(6);
            this._bottom.TabIndex = 3;
            // 
            // _btnClose
            // 
            this._btnClose.AutoSize = true;
            this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 27);
            this._btnClose.TabIndex = 4;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            // 
            // _btnApply
            // 
            this._btnApply.AutoSize = true;
            this._btnApply.Name = "_btnApply";
            this._btnApply.Size = new System.Drawing.Size(90, 27);
            this._btnApply.TabIndex = 5;
            this._btnApply.Text = "Select";
            this._btnApply.UseVisualStyleBackColor = true;
            this._btnApply.Click += new System.EventHandler(this.OnClickOk);
            // 
            // TileDataRangeForm
            // 
            this.AcceptButton = this._btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(460, 190);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select ID range";
            this.Controls.Add(this._tbl);
            this.Controls.Add(this._bottom);
            this.Name = "TileDataRangeForm";
            this._tbl.ResumeLayout(false);
            this._tbl.PerformLayout();
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _tbl;
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.TextBox _txtRange;
        private System.Windows.Forms.CheckBox _chkAdd;
        private System.Windows.Forms.Label _lblResult;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnClose;
        private System.Windows.Forms.Button _btnApply;
    }
}

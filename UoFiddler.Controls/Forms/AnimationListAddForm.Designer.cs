namespace UoFiddler.Controls.Forms
{
    partial class AnimationListAddForm
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
            this.rightPanel = new System.Windows.Forms.Panel();
            this.previewHost = new System.Windows.Forms.Panel();
            this._preview = new System.Windows.Forms.PictureBox();
            this.grid = new System.Windows.Forms.TableLayoutPanel();
            this.lblBody = new System.Windows.Forms.Label();
            this._numBody = new System.Windows.Forms.NumericUpDown();
            this.btnFree = new System.Windows.Forms.Button();
            this._lblInfo = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this._txtName = new System.Windows.Forms.TextBox();
            this.btnSuggest = new System.Windows.Forms.Button();
            this.lblType = new System.Windows.Forms.Label();
            this._cmbType = new System.Windows.Forms.ComboBox();
            this.lblPreview = new System.Windows.Forms.Label();
            this.actionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._cmbAction = new System.Windows.Forms.ComboBox();
            this.lblDirection = new System.Windows.Forms.Label();
            this._trackDirection = new System.Windows.Forms.TrackBar();
            this.leftPanel = new System.Windows.Forms.Panel();
            this._listCandidates = new System.Windows.Forms.ListBox();
            this.leftTitle = new System.Windows.Forms.Label();
            this._lblStatus = new System.Windows.Forms.Label();
            this.buttonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnServUo = new System.Windows.Forms.Button();
            this._btnAdd = new System.Windows.Forms.Button();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.rightPanel.SuspendLayout();
            this.previewHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._preview)).BeginInit();
            this.grid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numBody)).BeginInit();
            this.actionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackDirection)).BeginInit();
            this.leftPanel.SuspendLayout();
            this.buttonsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.previewHost);
            this.rightPanel.Controls.Add(this.grid);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.TabIndex = 0;
            // 
            // previewHost
            // 
            this.previewHost.Controls.Add(this._preview);
            this.previewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewHost.Name = "previewHost";
            this.previewHost.Padding = new System.Windows.Forms.Padding(8, 0, 8, 4);
            this.previewHost.TabIndex = 1;
            // 
            // _preview
            // 
            this._preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(44)))));
            this._preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._preview.Name = "_preview";
            this._preview.TabIndex = 2;
            this._preview.TabStop = false;
            this._preview.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintPreview);
            // 
            // grid
            // 
            this.grid.Controls.Add(this.lblBody, 0, 0);
            this.grid.Controls.Add(this._numBody, 1, 0);
            this.grid.Controls.Add(this.btnFree, 2, 0);
            this.grid.Controls.Add(this._lblInfo, 1, 1);
            this.grid.Controls.Add(this.lblName, 0, 2);
            this.grid.Controls.Add(this._txtName, 1, 2);
            this.grid.Controls.Add(this.btnSuggest, 2, 2);
            this.grid.Controls.Add(this.lblType, 0, 3);
            this.grid.Controls.Add(this._cmbType, 1, 3);
            this.grid.Controls.Add(this.lblPreview, 0, 4);
            this.grid.Controls.Add(this.actionPanel, 1, 4);
            this.grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.grid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.grid.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.grid.SetColumnSpan(this._lblInfo, 2);
            this.grid.AutoSize = true;
            this.grid.ColumnCount = 3;
            this.grid.Dock = System.Windows.Forms.DockStyle.Top;
            this.grid.Name = "grid";
            this.grid.Padding = new System.Windows.Forms.Padding(8);
            this.grid.RowCount = 5;
            this.grid.Size = new System.Drawing.Size(544, 160);
            this.grid.TabIndex = 3;
            // 
            // lblBody
            // 
            this.lblBody.AutoSize = true;
            this.lblBody.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.lblBody.Name = "lblBody";
            this.lblBody.Text = "Body id:";
            // 
            // _numBody
            // 
            this._numBody.Maximum = new decimal(new int[] {65535, 0, 0, 0});
            this._numBody.Name = "_numBody";
            this._numBody.Size = new System.Drawing.Size(90, 23);
            this._numBody.TabIndex = 4;
            this._numBody.ValueChanged += new System.EventHandler(this.OnBodyValueChanged);
            // 
            // btnFree
            // 
            this.btnFree.AutoSize = true;
            this.btnFree.Name = "btnFree";
            this.btnFree.TabIndex = 5;
            this.btnFree.Text = "Free id";
            this.btnFree.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this.btnFree, "Next body id that is not used by a client animation, Bodyconv.def or the list");
            this.btnFree.Click += new System.EventHandler(this.OnClickFreeId);
            // 
            // _lblInfo
            // 
            this._lblInfo.AutoSize = true;
            this._lblInfo.ForeColor = System.Drawing.Color.DimGray;
            this._lblInfo.MaximumSize = new System.Drawing.Size(480, 0);
            this._lblInfo.Name = "_lblInfo";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.lblName.Name = "lblName";
            this.lblName.Text = "Name:";
            // 
            // _txtName
            // 
            this._txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._txtName.Name = "_txtName";
            this._txtName.TabIndex = 6;
            // 
            // btnSuggest
            // 
            this.btnSuggest.AutoSize = true;
            this.btnSuggest.Name = "btnSuggest";
            this.btnSuggest.TabIndex = 7;
            this.btnSuggest.Text = "Suggest";
            this.btnSuggest.UseVisualStyleBackColor = true;
            this.btnSuggest.Click += new System.EventHandler(this.OnClickSuggest);
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.lblType.Name = "lblType";
            this.lblType.Text = "Type:";
            // 
            // _cmbType
            // 
            this._cmbType.Items.AddRange(new object[] {
            "Monster",
            "Sea",
            "Animal",
            "Human",
            "Equipment (paperdoll)"});
            this._cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbType.FormattingEnabled = true;
            this._cmbType.Name = "_cmbType";
            this._cmbType.Size = new System.Drawing.Size(160, 23);
            this._cmbType.TabIndex = 8;
            this._cmbType.SelectedIndexChanged += new System.EventHandler(this.OnTypeChanged);
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Text = "Preview:";
            // 
            // actionPanel
            // 
            this.actionPanel.Controls.Add(this._cmbAction);
            this.actionPanel.Controls.Add(this.lblDirection);
            this.actionPanel.Controls.Add(this._trackDirection);
            this.actionPanel.AutoSize = true;
            this.actionPanel.Margin = new System.Windows.Forms.Padding(0);
            this.actionPanel.Name = "actionPanel";
            this.actionPanel.TabIndex = 9;
            this.actionPanel.WrapContents = false;
            // 
            // _cmbAction
            // 
            this._cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbAction.FormattingEnabled = true;
            this._cmbAction.Name = "_cmbAction";
            this._cmbAction.Size = new System.Drawing.Size(220, 23);
            this._cmbAction.TabIndex = 10;
            this._cmbAction.SelectedIndexChanged += new System.EventHandler(this.OnPreviewSettingChanged);
            // 
            // lblDirection
            // 
            this.lblDirection.AutoSize = true;
            this.lblDirection.Margin = new System.Windows.Forms.Padding(8, 6, 0, 0);
            this.lblDirection.Name = "lblDirection";
            this.lblDirection.Text = "Direction:";
            // 
            // _trackDirection
            // 
            this._trackDirection.AutoSize = false;
            this._trackDirection.Maximum = 7;
            this._trackDirection.Name = "_trackDirection";
            this._trackDirection.Size = new System.Drawing.Size(140, 26);
            this._trackDirection.TabIndex = 11;
            this._trackDirection.TickStyle = System.Windows.Forms.TickStyle.None;
            this._trackDirection.Value = 1;
            this._trackDirection.ValueChanged += new System.EventHandler(this.OnPreviewSettingChanged);
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this._listCandidates);
            this.leftPanel.Controls.Add(this.leftTitle);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new System.Windows.Forms.Padding(8, 8, 0, 8);
            this.leftPanel.Size = new System.Drawing.Size(260, 441);
            this.leftPanel.TabIndex = 12;
            // 
            // _listCandidates
            // 
            this._listCandidates.Dock = System.Windows.Forms.DockStyle.Fill;
            this._listCandidates.Font = new System.Drawing.Font("Consolas", 9F);
            this._listCandidates.FormattingEnabled = true;
            this._listCandidates.IntegralHeight = false;
            this._listCandidates.Name = "_listCandidates";
            this._listCandidates.TabIndex = 13;
            this._listCandidates.SelectedIndexChanged += new System.EventHandler(this.OnCandidateSelected);
            // 
            // leftTitle
            // 
            this.leftTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.leftTitle.Name = "leftTitle";
            this.leftTitle.Size = new System.Drawing.Size(252, 34);
            this.leftTitle.Text = "Edited animations that are not in the list:";
            // 
            // _lblStatus
            // 
            this._lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._lblStatus.ForeColor = System.Drawing.Color.DarkGreen;
            this._lblStatus.Name = "_lblStatus";
            this._lblStatus.Padding = new System.Windows.Forms.Padding(8, 3, 0, 0);
            this._lblStatus.Size = new System.Drawing.Size(804, 22);
            // 
            // buttonsPanel
            // 
            this.buttonsPanel.Controls.Add(this.btnClose);
            this.buttonsPanel.Controls.Add(this.btnServUo);
            this.buttonsPanel.Controls.Add(this._btnAdd);
            this.buttonsPanel.AutoSize = true;
            this.buttonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonsPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonsPanel.Name = "buttonsPanel";
            this.buttonsPanel.Padding = new System.Windows.Forms.Padding(6);
            this.buttonsPanel.TabIndex = 14;
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.Name = "btnClose";
            this.btnClose.TabIndex = 15;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.OnClickClose);
            // 
            // btnServUo
            // 
            this.btnServUo.AutoSize = true;
            this.btnServUo.Name = "btnServUo";
            this.btnServUo.TabIndex = 16;
            this.btnServUo.Text = "ServUO script && setup…";
            this.btnServUo.UseVisualStyleBackColor = true;
            this.btnServUo.Click += new System.EventHandler(this.OnClickServUo);
            // 
            // _btnAdd
            // 
            this._btnAdd.AutoSize = true;
            this._btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this._btnAdd.Name = "_btnAdd";
            this._btnAdd.TabIndex = 17;
            this._btnAdd.Text = "Add to list";
            this._btnAdd.UseVisualStyleBackColor = true;
            this._btnAdd.Click += new System.EventHandler(this.OnClickAdd);
            // 
            // _timer
            // 
            this._timer.Interval = 150;
            this._timer.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // AnimationListAddForm
            // 
            this.AcceptButton = this._btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 521);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(520, 480);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add animation to the list";
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this._lblStatus);
            this.Controls.Add(this.buttonsPanel);
            this.Name = "AnimationListAddForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosedAdd);
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            this.previewHost.ResumeLayout(false);
            this.previewHost.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._preview)).EndInit();
            this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numBody)).EndInit();
            this.actionPanel.ResumeLayout(false);
            this.actionPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackDirection)).EndInit();
            this.leftPanel.ResumeLayout(false);
            this.leftPanel.PerformLayout();
            this.buttonsPanel.ResumeLayout(false);
            this.buttonsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.Panel previewHost;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.TableLayoutPanel grid;
        private System.Windows.Forms.Label lblBody;
        private System.Windows.Forms.NumericUpDown _numBody;
        private System.Windows.Forms.Button btnFree;
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Button btnSuggest;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox _cmbType;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.FlowLayoutPanel actionPanel;
        private System.Windows.Forms.ComboBox _cmbAction;
        private System.Windows.Forms.Label lblDirection;
        private System.Windows.Forms.TrackBar _trackDirection;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.ListBox _listCandidates;
        private System.Windows.Forms.Label leftTitle;
        private System.Windows.Forms.Label _lblStatus;
        private System.Windows.Forms.FlowLayoutPanel buttonsPanel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnServUo;
        private System.Windows.Forms.Button _btnAdd;
        private System.Windows.Forms.Timer _timer;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

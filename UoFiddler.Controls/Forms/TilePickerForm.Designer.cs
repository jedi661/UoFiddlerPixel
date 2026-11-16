namespace UoFiddler.Controls.Forms
{
    partial class TilePickerForm
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
            this._grid = new UoFiddler.Controls.UserControls.TileGridView();
            this._top = new System.Windows.Forms.FlowLayoutPanel();
            this._lblSearch = new System.Windows.Forms.Label();
            this._txtSearch = new System.Windows.Forms.TextBox();
            this._chkOnlyLight = new System.Windows.Forms.CheckBox();
            this._lblSize = new System.Windows.Forms.Label();
            this._cmbSize = new System.Windows.Forms.ComboBox();
            this._lblCount = new System.Windows.Forms.Label();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._lblSelected = new System.Windows.Forms.Label();
            this._top.SuspendLayout();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _grid
            // 
            this._grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._grid.Name = "_grid";
            this._grid.Size = new System.Drawing.Size(784, 480);
            this._grid.TabIndex = 0;
            this._grid.ItemActivated += new System.EventHandler(this.OnItemActivated);
            this._grid.SelectionChanged += new System.EventHandler(this.OnSelectionChanged);
            // 
            // _top
            // 
            this._top.Controls.Add(this._lblSearch);
            this._top.Controls.Add(this._txtSearch);
            this._top.Controls.Add(this._chkOnlyLight);
            this._top.Controls.Add(this._lblSize);
            this._top.Controls.Add(this._cmbSize);
            this._top.Controls.Add(this._lblCount);
            this._top.AutoSize = true;
            this._top.Dock = System.Windows.Forms.DockStyle.Top;
            this._top.Name = "_top";
            this._top.Padding = new System.Windows.Forms.Padding(6);
            this._top.TabIndex = 1;
            // 
            // _lblSearch
            // 
            this._lblSearch.AutoSize = true;
            this._lblSearch.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this._lblSearch.Name = "_lblSearch";
            this._lblSearch.Text = "Search:";
            // 
            // _txtSearch
            // 
            this._txtSearch.Name = "_txtSearch";
            this._txtSearch.PlaceholderText = "ID (dec / 0x…), range or name";
            this._txtSearch.Size = new System.Drawing.Size(220, 23);
            this._txtSearch.TabIndex = 2;
            this._txtSearch.TextChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // _chkOnlyLight
            // 
            this._chkOnlyLight.AutoSize = true;
            this._chkOnlyLight.Margin = new System.Windows.Forms.Padding(10, 6, 3, 3);
            this._chkOnlyLight.Name = "_chkOnlyLight";
            this._chkOnlyLight.TabIndex = 3;
            this._chkOnlyLight.Text = "only light sources (flag LightSource)";
            this._chkOnlyLight.UseVisualStyleBackColor = true;
            this._chkOnlyLight.CheckedChanged += new System.EventHandler(this.OnFilterChanged);
            // 
            // _lblSize
            // 
            this._lblSize.AutoSize = true;
            this._lblSize.Margin = new System.Windows.Forms.Padding(10, 7, 3, 3);
            this._lblSize.Name = "_lblSize";
            this._lblSize.Text = "Size:";
            // 
            // _cmbSize
            // 
            this._cmbSize.Items.AddRange(new object[] {
            "64",
            "96",
            "128",
            "192"});
            this._cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSize.Name = "_cmbSize";
            this._cmbSize.Size = new System.Drawing.Size(70, 23);
            this._cmbSize.TabIndex = 4;
            this._cmbSize.SelectedIndexChanged += new System.EventHandler(this.OnSizeChanged);
            // 
            // _lblCount
            // 
            this._lblCount.AutoSize = true;
            this._lblCount.ForeColor = System.Drawing.SystemColors.GrayText;
            this._lblCount.Margin = new System.Windows.Forms.Padding(10, 7, 3, 3);
            this._lblCount.Name = "_lblCount";
            this._lblCount.Text = "-";
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnCancel);
            this._bottom.Controls.Add(this._btnOk);
            this._bottom.Controls.Add(this._lblSelected);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(6);
            this._bottom.TabIndex = 5;
            // 
            // _btnCancel
            // 
            this._btnCancel.AutoSize = true;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(90, 27);
            this._btnCancel.TabIndex = 6;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.AutoSize = true;
            this._btnOk.Name = "_btnOk";
            this._btnOk.Size = new System.Drawing.Size(90, 27);
            this._btnOk.TabIndex = 7;
            this._btnOk.Text = "Select";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new System.EventHandler(this.OnClickOk);
            // 
            // _lblSelected
            // 
            this._lblSelected.AutoSize = true;
            this._lblSelected.Margin = new System.Windows.Forms.Padding(3, 8, 20, 3);
            this._lblSelected.Name = "_lblSelected";
            this._lblSelected.Text = "-";
            // 
            // TilePickerForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(784, 600);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose";
            this.Controls.Add(this._grid);
            this.Controls.Add(this._top);
            this.Controls.Add(this._bottom);
            this.Name = "TilePickerForm";
            this._top.ResumeLayout(false);
            this._top.PerformLayout();
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private UoFiddler.Controls.UserControls.TileGridView _grid;
        private System.Windows.Forms.FlowLayoutPanel _top;
        private System.Windows.Forms.Label _lblSearch;
        private System.Windows.Forms.TextBox _txtSearch;
        private System.Windows.Forms.CheckBox _chkOnlyLight;
        private System.Windows.Forms.Label _lblSize;
        private System.Windows.Forms.ComboBox _cmbSize;
        private System.Windows.Forms.Label _lblCount;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.Label _lblSelected;
    }
}

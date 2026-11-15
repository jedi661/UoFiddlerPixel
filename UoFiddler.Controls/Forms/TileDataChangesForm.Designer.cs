namespace UoFiddler.Controls.Forms
{
    partial class TileDataChangesForm
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
            this._colKind = new System.Windows.Forms.ColumnHeader();
            this._colId = new System.Windows.Forms.ColumnHeader();
            this._colName = new System.Windows.Forms.ColumnHeader();
            this._colDiff = new System.Windows.Forms.ColumnHeader();
            this._tool = new System.Windows.Forms.ToolStrip();
            this._btnRefresh = new System.Windows.Forms.ToolStripButton();
            this._btnShow = new System.Windows.Forms.ToolStripButton();
            this._btnRevert = new System.Windows.Forms.ToolStripButton();
            this._btnRevertAll = new System.Windows.Forms.ToolStripButton();
            this._btnCopy = new System.Windows.Forms.ToolStripButton();
            this._lblSummary = new System.Windows.Forms.ToolStripLabel();
            this._bottom = new System.Windows.Forms.FlowLayoutPanel();
            this._btnClose = new System.Windows.Forms.Button();
            this._tool.SuspendLayout();
            this._bottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // _list
            // 
            this._list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._colKind,
            this._colId,
            this._colName,
            this._colDiff});
            this._list.Dock = System.Windows.Forms.DockStyle.Fill;
            this._list.FullRowSelect = true;
            this._list.GridLines = true;
            this._list.HideSelection = false;
            this._list.Name = "_list";
            this._list.TabIndex = 0;
            this._list.UseCompatibleStateImageBehavior = false;
            this._list.View = System.Windows.Forms.View.Details;
            this._list.DoubleClick += new System.EventHandler(this.OnClickShow);
            this._list.SelectedIndexChanged += new System.EventHandler(this.OnSelectionChanged);
            // 
            // _colKind
            // 
            this._colKind.Text = "Type";
            this._colKind.Width = 60;
            // 
            // _colId
            // 
            this._colId.Text = "ID";
            this._colId.Width = 110;
            // 
            // _colName
            // 
            this._colName.Text = "Name";
            this._colName.Width = 150;
            // 
            // _colDiff
            // 
            this._colDiff.Text = "Changes (file → now)";
            this._colDiff.Width = 560;
            // 
            // _tool
            // 
            this._tool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnRefresh,
            this._btnShow,
            this._btnRevert,
            this._btnRevertAll,
            this._btnCopy,
            this._lblSummary});
            this._tool.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this._tool.Name = "_tool";
            this._tool.TabIndex = 1;
            // 
            // _btnRefresh
            // 
            this._btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRefresh.Name = "_btnRefresh";
            this._btnRefresh.Text = "⟳ Refresh";
            this._btnRefresh.ToolTipText = "Compare again";
            this._btnRefresh.Click += new System.EventHandler(this.OnClickRefresh);
            // 
            // _btnShow
            // 
            this._btnShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnShow.Name = "_btnShow";
            this._btnShow.Text = "Show";
            this._btnShow.ToolTipText = "Show the tile in the TileData tab (double click)";
            this._btnShow.Click += new System.EventHandler(this.OnClickShow);
            // 
            // _btnRevert
            // 
            this._btnRevert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRevert.Name = "_btnRevert";
            this._btnRevert.Text = "Revert selected";
            this._btnRevert.ToolTipText = "Set the selected tiles back to the state of the file (can be undone)";
            this._btnRevert.Click += new System.EventHandler(this.OnClickRevertSelected);
            // 
            // _btnRevertAll
            // 
            this._btnRevertAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnRevertAll.Name = "_btnRevertAll";
            this._btnRevertAll.Text = "Revert all";
            this._btnRevertAll.ToolTipText = "Set all changed tiles back to the state of the file (can be undone)";
            this._btnRevertAll.Click += new System.EventHandler(this.OnClickRevertAll);
            // 
            // _btnCopy
            // 
            this._btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._btnCopy.Name = "_btnCopy";
            this._btnCopy.Text = "Copy list";
            this._btnCopy.ToolTipText = "Copy the list as text";
            this._btnCopy.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // _lblSummary
            // 
            this._lblSummary.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._lblSummary.Name = "_lblSummary";
            this._lblSummary.Text = "-";
            // 
            // _bottom
            // 
            this._bottom.Controls.Add(this._btnClose);
            this._bottom.AutoSize = true;
            this._bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._bottom.Name = "_bottom";
            this._bottom.Padding = new System.Windows.Forms.Padding(6);
            this._bottom.TabIndex = 2;
            // 
            // _btnClose
            // 
            this._btnClose.AutoSize = true;
            this._btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnClose.Name = "_btnClose";
            this._btnClose.Size = new System.Drawing.Size(90, 27);
            this._btnClose.TabIndex = 3;
            this._btnClose.Text = "Close";
            this._btnClose.UseVisualStyleBackColor = true;
            // 
            // TileDataChangesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnClose;
            this.ClientSize = new System.Drawing.Size(900, 520);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TileData – changed tiles";
            this.Controls.Add(this._list);
            this.Controls.Add(this._tool);
            this.Controls.Add(this._bottom);
            this.Name = "TileDataChangesForm";
            this._tool.ResumeLayout(false);
            this._tool.PerformLayout();
            this._bottom.ResumeLayout(false);
            this._bottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader _colKind;
        private System.Windows.Forms.ColumnHeader _colId;
        private System.Windows.Forms.ColumnHeader _colName;
        private System.Windows.Forms.ColumnHeader _colDiff;
        private System.Windows.Forms.ToolStrip _tool;
        private System.Windows.Forms.ToolStripButton _btnRefresh;
        private System.Windows.Forms.ToolStripButton _btnShow;
        private System.Windows.Forms.ToolStripButton _btnRevert;
        private System.Windows.Forms.ToolStripButton _btnRevertAll;
        private System.Windows.Forms.ToolStripButton _btnCopy;
        private System.Windows.Forms.ToolStripLabel _lblSummary;
        private System.Windows.Forms.FlowLayoutPanel _bottom;
        private System.Windows.Forms.Button _btnClose;
    }
}

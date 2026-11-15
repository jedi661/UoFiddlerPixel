namespace UoFiddler.Controls.UserControls
{
    partial class HuePreviewPanel
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
                _timer.Stop();
                DisposeHued();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._box = new System.Windows.Forms.PictureBox();
            this.bar = new System.Windows.Forms.FlowLayoutPanel();
            this._cmbSource = new System.Windows.Forms.ComboBox();
            this._txtId = new System.Windows.Forms.TextBox();
            this._chkPartial = new System.Windows.Forms.CheckBox();
            this._chkAnimate = new System.Windows.Forms.CheckBox();
            this.dirLabel = new System.Windows.Forms.Label();
            this._numDir = new System.Windows.Forms.NumericUpDown();
            this._chkDark = new System.Windows.Forms.CheckBox();
            this._lnkOwn = new System.Windows.Forms.LinkLabel();
            this._timer = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._box)).BeginInit();
            this.bar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDir)).BeginInit();
            this.SuspendLayout();
            // 
            // _box
            // 
            this._box.Dock = System.Windows.Forms.DockStyle.Fill;
            this._box.Name = "_box";
            this._box.Size = new System.Drawing.Size(380, 230);
            this._box.TabIndex = 0;
            this._box.TabStop = false;
            this._box.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintBox);
            this._box.Resize += new System.EventHandler(this.OnBoxResize);
            // 
            // bar
            // 
            this.bar.Controls.Add(this._cmbSource);
            this.bar.Controls.Add(this._txtId);
            this.bar.Controls.Add(this._chkPartial);
            this.bar.Controls.Add(this._chkAnimate);
            this.bar.Controls.Add(this.dirLabel);
            this.bar.Controls.Add(this._numDir);
            this.bar.Controls.Add(this._chkDark);
            this.bar.Controls.Add(this._lnkOwn);
            this.bar.AutoSize = true;
            this.bar.Dock = System.Windows.Forms.DockStyle.Top;
            this.bar.Name = "bar";
            this.bar.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.bar.Size = new System.Drawing.Size(380, 33);
            this.bar.TabIndex = 1;
            // 
            // _cmbSource
            // 
            this._cmbSource.Items.AddRange(new object[] {
            "Item (art)",
            "Animation",
            "Gump"});
            this._cmbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbSource.FormattingEnabled = true;
            this._cmbSource.Name = "_cmbSource";
            this._cmbSource.Size = new System.Drawing.Size(90, 23);
            this._cmbSource.TabIndex = 2;
            this._cmbSource.SelectedIndexChanged += new System.EventHandler(this.OnSourceChanged);
            // 
            // _txtId
            // 
            this._txtId.Name = "_txtId";
            this._txtId.Size = new System.Drawing.Size(64, 23);
            this._txtId.TabIndex = 3;
            this.toolTip.SetToolTip(this._txtId, "Id (decimal or 0x..), Enter = show");
            this._txtId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnIdKeyDown);
            this._txtId.Leave += new System.EventHandler(this.OnIdLeave);
            // 
            // _chkPartial
            // 
            this._chkPartial.AutoSize = true;
            this._chkPartial.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this._chkPartial.Name = "_chkPartial";
            this._chkPartial.TabIndex = 4;
            this._chkPartial.Text = "Only grey (partial hue)";
            this._chkPartial.UseVisualStyleBackColor = true;
            this.toolTip.SetToolTip(this._chkPartial, "Only grey pixels are coloured. Items: taken from the tiledata flag \"PartialHue\" (as in the game).");
            this._chkPartial.CheckedChanged += new System.EventHandler(this.OnPartialChanged);
            // 
            // _chkAnimate
            // 
            this._chkAnimate.AutoSize = true;
            this._chkAnimate.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkAnimate.Checked = true;
            this._chkAnimate.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this._chkAnimate.Name = "_chkAnimate";
            this._chkAnimate.TabIndex = 5;
            this._chkAnimate.Text = "Animate";
            this._chkAnimate.UseVisualStyleBackColor = true;
            this._chkAnimate.CheckedChanged += new System.EventHandler(this.OnAnimateChanged);
            // 
            // dirLabel
            // 
            this.dirLabel.AutoSize = true;
            this.dirLabel.Margin = new System.Windows.Forms.Padding(3, 6, 0, 0);
            this.dirLabel.Name = "dirLabel";
            this.dirLabel.Text = "Dir";
            // 
            // _numDir
            // 
            this._numDir.Maximum = new decimal(new int[] {7, 0, 0, 0});
            this._numDir.Name = "_numDir";
            this._numDir.Size = new System.Drawing.Size(38, 23);
            this._numDir.TabIndex = 6;
            this.toolTip.SetToolTip(this._numDir, "Direction of the animation");
            this._numDir.ValueChanged += new System.EventHandler(this.OnDirectionChanged);
            // 
            // _chkDark
            // 
            this._chkDark.AutoSize = true;
            this._chkDark.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkDark.Checked = true;
            this._chkDark.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this._chkDark.Name = "_chkDark";
            this._chkDark.TabIndex = 7;
            this._chkDark.Text = "Dark";
            this._chkDark.UseVisualStyleBackColor = true;
            this._chkDark.CheckedChanged += new System.EventHandler(this.OnDarkChanged);
            // 
            // _lnkOwn
            // 
            this._lnkOwn.AutoSize = true;
            this._lnkOwn.Margin = new System.Windows.Forms.Padding(6, 6, 3, 0);
            this._lnkOwn.Name = "_lnkOwn";
            this._lnkOwn.TabIndex = 8;
            this._lnkOwn.TabStop = true;
            this._lnkOwn.Text = "← own test graphic";
            this._lnkOwn.Visible = false;
            this._lnkOwn.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnOwnGraphicClicked);
            // 
            // _timer
            // 
            this._timer.Interval = 150;
            this._timer.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // HuePreviewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(380, 263);
            this.Controls.Add(this._box);
            this.Controls.Add(this.bar);
            this.Name = "HuePreviewPanel";
            ((System.ComponentModel.ISupportInitialize)(this._box)).EndInit();
            this.bar.ResumeLayout(false);
            this.bar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._numDir)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox _box;
        private System.Windows.Forms.FlowLayoutPanel bar;
        private System.Windows.Forms.ComboBox _cmbSource;
        private System.Windows.Forms.TextBox _txtId;
        private System.Windows.Forms.CheckBox _chkPartial;
        private System.Windows.Forms.CheckBox _chkAnimate;
        private System.Windows.Forms.Label dirLabel;
        private System.Windows.Forms.NumericUpDown _numDir;
        private System.Windows.Forms.CheckBox _chkDark;
        private System.Windows.Forms.LinkLabel _lnkOwn;
        private System.Windows.Forms.Timer _timer;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

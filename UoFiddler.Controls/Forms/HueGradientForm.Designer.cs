namespace UoFiddler.Controls.Forms
{
    partial class HueGradientForm
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
            this.previewHost = new System.Windows.Forms.Panel();
            this._preview = new UoFiddler.Controls.UserControls.HuePreviewPanel();
            this._strip = new System.Windows.Forms.PictureBox();
            this.settingsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnDark = new System.Windows.Forms.Button();
            this._btnMiddle = new System.Windows.Forms.Button();
            this._btnBright = new System.Windows.Forms.Button();
            this._chkMiddle = new System.Windows.Forms.CheckBox();
            this.curveLabel = new System.Windows.Forms.Label();
            this._trackGamma = new System.Windows.Forms.TrackBar();
            this._lblGamma = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._txtName = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.previewHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).BeginInit();
            this.settingsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackGamma)).BeginInit();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // previewHost
            // 
            this.previewHost.Controls.Add(this._preview);
            this.previewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewHost.Name = "previewHost";
            this.previewHost.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.previewHost.TabIndex = 0;
            // 
            // _preview
            // 
            this._preview.Dock = System.Windows.Forms.DockStyle.Fill;
            this._preview.Name = "_preview";
            this._preview.TabIndex = 1;
            // 
            // _strip
            // 
            this._strip.Dock = System.Windows.Forms.DockStyle.Top;
            this._strip.Name = "_strip";
            this._strip.Size = new System.Drawing.Size(624, 34);
            this._strip.TabIndex = 2;
            this._strip.TabStop = false;
            this._strip.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintStrip);
            this._strip.Resize += new System.EventHandler(this.OnStripResize);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this._btnDark);
            this.settingsPanel.Controls.Add(this._btnMiddle);
            this.settingsPanel.Controls.Add(this._btnBright);
            this.settingsPanel.Controls.Add(this._chkMiddle);
            this.settingsPanel.Controls.Add(this.curveLabel);
            this.settingsPanel.Controls.Add(this._trackGamma);
            this.settingsPanel.Controls.Add(this._lblGamma);
            this.settingsPanel.AutoSize = true;
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Padding = new System.Windows.Forms.Padding(8);
            this.settingsPanel.TabIndex = 3;
            // 
            // _btnDark
            // 
            this._btnDark.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnDark.Name = "_btnDark";
            this._btnDark.Size = new System.Drawing.Size(90, 28);
            this._btnDark.TabIndex = 4;
            this._btnDark.Text = "Dark";
            this._btnDark.UseVisualStyleBackColor = false;
            this._btnDark.Click += new System.EventHandler(this.OnClickColorButton);
            // 
            // _btnMiddle
            // 
            this._btnMiddle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnMiddle.Name = "_btnMiddle";
            this._btnMiddle.Size = new System.Drawing.Size(90, 28);
            this._btnMiddle.TabIndex = 5;
            this._btnMiddle.Text = "Middle";
            this._btnMiddle.UseVisualStyleBackColor = false;
            this._btnMiddle.Click += new System.EventHandler(this.OnClickColorButton);
            // 
            // _btnBright
            // 
            this._btnBright.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this._btnBright.Name = "_btnBright";
            this._btnBright.Size = new System.Drawing.Size(90, 28);
            this._btnBright.TabIndex = 6;
            this._btnBright.Text = "Bright";
            this._btnBright.UseVisualStyleBackColor = false;
            this._btnBright.Click += new System.EventHandler(this.OnClickColorButton);
            // 
            // _chkMiddle
            // 
            this._chkMiddle.AutoSize = true;
            this._chkMiddle.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkMiddle.Checked = true;
            this._chkMiddle.Margin = new System.Windows.Forms.Padding(6, 8, 3, 3);
            this._chkMiddle.Name = "_chkMiddle";
            this._chkMiddle.TabIndex = 7;
            this._chkMiddle.Text = "use middle colour";
            this._chkMiddle.UseVisualStyleBackColor = true;
            this._chkMiddle.CheckedChanged += new System.EventHandler(this.OnMiddleChanged);
            // 
            // curveLabel
            // 
            this.curveLabel.AutoSize = true;
            this.curveLabel.Margin = new System.Windows.Forms.Padding(10, 8, 0, 3);
            this.curveLabel.Name = "curveLabel";
            this.curveLabel.Text = "Curve:";
            // 
            // _trackGamma
            // 
            this._trackGamma.AutoSize = false;
            this._trackGamma.Maximum = 25;
            this._trackGamma.Minimum = 4;
            this._trackGamma.Name = "_trackGamma";
            this._trackGamma.Size = new System.Drawing.Size(150, 28);
            this._trackGamma.TabIndex = 8;
            this._trackGamma.TickStyle = System.Windows.Forms.TickStyle.None;
            this._trackGamma.Value = 10;
            this._trackGamma.ValueChanged += new System.EventHandler(this.OnGammaChanged);
            // 
            // _lblGamma
            // 
            this._lblGamma.AutoSize = true;
            this._lblGamma.Margin = new System.Windows.Forms.Padding(0, 8, 3, 3);
            this._lblGamma.Name = "_lblGamma";
            this._lblGamma.Text = "linear";
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this._btnCancel);
            this.bottomPanel.Controls.Add(this._btnOk);
            this.bottomPanel.Controls.Add(this._txtName);
            this.bottomPanel.Controls.Add(this.nameLabel);
            this.bottomPanel.AutoSize = true;
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(6);
            this.bottomPanel.TabIndex = 9;
            // 
            // _btnCancel
            // 
            this._btnCancel.AutoSize = true;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.TabIndex = 10;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.AutoSize = true;
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Name = "_btnOk";
            this._btnOk.TabIndex = 11;
            this._btnOk.Text = "Apply";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _txtName
            // 
            this._txtName.MaxLength = 20;
            this._txtName.Name = "_txtName";
            this._txtName.Size = new System.Drawing.Size(180, 23);
            this._txtName.TabIndex = 12;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Text = "Name:";
            // 
            // HueGradientForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 581);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(520, 480);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New hue from a gradient";
            this.Controls.Add(this.previewHost);
            this.Controls.Add(this._strip);
            this.Controls.Add(this.settingsPanel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "HueGradientForm";
            this.previewHost.ResumeLayout(false);
            this.previewHost.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).EndInit();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackGamma)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel previewHost;
        private UoFiddler.Controls.UserControls.HuePreviewPanel _preview;
        private System.Windows.Forms.PictureBox _strip;
        private System.Windows.Forms.FlowLayoutPanel settingsPanel;
        private System.Windows.Forms.Button _btnDark;
        private System.Windows.Forms.Button _btnMiddle;
        private System.Windows.Forms.Button _btnBright;
        private System.Windows.Forms.CheckBox _chkMiddle;
        private System.Windows.Forms.Label curveLabel;
        private System.Windows.Forms.TrackBar _trackGamma;
        private System.Windows.Forms.Label _lblGamma;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Label nameLabel;
    }
}

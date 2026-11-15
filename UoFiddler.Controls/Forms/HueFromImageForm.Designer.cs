namespace UoFiddler.Controls.Forms
{
    partial class HueFromImageForm
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
                _bitmap?.Dispose();
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
            this._image = new System.Windows.Forms.PictureBox();
            this.settingsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnWhole = new System.Windows.Forms.Button();
            this._chkIgnoreDark = new System.Windows.Forms.CheckBox();
            this.smoothLabel = new System.Windows.Forms.Label();
            this._trackSmooth = new System.Windows.Forms.TrackBar();
            this.hintLabel = new System.Windows.Forms.Label();
            this.bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnCancel = new System.Windows.Forms.Button();
            this._btnOk = new System.Windows.Forms.Button();
            this._txtName = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.previewHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
            this.settingsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackSmooth)).BeginInit();
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
            // _image
            // 
            this._image.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(44)))));
            this._image.Cursor = System.Windows.Forms.Cursors.Cross;
            this._image.Dock = System.Windows.Forms.DockStyle.Top;
            this._image.Name = "_image";
            this._image.Size = new System.Drawing.Size(744, 260);
            this._image.TabIndex = 3;
            this._image.TabStop = false;
            this._image.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintImage);
            this._image.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnImageMouseDown);
            this._image.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnImageMouseMove);
            this._image.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnImageMouseUp);
            this._image.Resize += new System.EventHandler(this.OnImageResize);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Controls.Add(this.btnOpen);
            this.settingsPanel.Controls.Add(this.btnPaste);
            this.settingsPanel.Controls.Add(this.btnWhole);
            this.settingsPanel.Controls.Add(this._chkIgnoreDark);
            this.settingsPanel.Controls.Add(this.smoothLabel);
            this.settingsPanel.Controls.Add(this._trackSmooth);
            this.settingsPanel.AutoSize = true;
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.settingsPanel.TabIndex = 4;
            // 
            // btnOpen
            // 
            this.btnOpen.AutoSize = true;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.TabIndex = 5;
            this.btnOpen.Text = "Open image…";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.OnClickOpen);
            // 
            // btnPaste
            // 
            this.btnPaste.AutoSize = true;
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.TabIndex = 6;
            this.btnPaste.Text = "Paste from clipboard";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.OnClickPaste);
            // 
            // btnWhole
            // 
            this.btnWhole.AutoSize = true;
            this.btnWhole.Name = "btnWhole";
            this.btnWhole.TabIndex = 7;
            this.btnWhole.Text = "Whole image";
            this.btnWhole.UseVisualStyleBackColor = true;
            this.btnWhole.Click += new System.EventHandler(this.OnClickWhole);
            // 
            // _chkIgnoreDark
            // 
            this._chkIgnoreDark.AutoSize = true;
            this._chkIgnoreDark.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkIgnoreDark.Checked = true;
            this._chkIgnoreDark.Margin = new System.Windows.Forms.Padding(6, 8, 3, 3);
            this._chkIgnoreDark.Name = "_chkIgnoreDark";
            this._chkIgnoreDark.TabIndex = 8;
            this._chkIgnoreDark.Text = "ignore black/transparent";
            this._chkIgnoreDark.UseVisualStyleBackColor = true;
            this._chkIgnoreDark.CheckedChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // smoothLabel
            // 
            this.smoothLabel.AutoSize = true;
            this.smoothLabel.Margin = new System.Windows.Forms.Padding(8, 8, 0, 3);
            this.smoothLabel.Name = "smoothLabel";
            this.smoothLabel.Text = "Smooth:";
            // 
            // _trackSmooth
            // 
            this._trackSmooth.AutoSize = false;
            this._trackSmooth.Maximum = 4;
            this._trackSmooth.Name = "_trackSmooth";
            this._trackSmooth.Size = new System.Drawing.Size(100, 28);
            this._trackSmooth.TabIndex = 9;
            this._trackSmooth.TickStyle = System.Windows.Forms.TickStyle.None;
            this._trackSmooth.Value = 1;
            this._trackSmooth.ValueChanged += new System.EventHandler(this.OnSettingChanged);
            // 
            // hintLabel
            // 
            this.hintLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Padding = new System.Windows.Forms.Padding(8, 4, 8, 0);
            this.hintLabel.Size = new System.Drawing.Size(744, 34);
            this.hintLabel.Text = "Mark an area in the image with the mouse (e.g. a piece of cloth). Its colours are sorted from dark to bright.";
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
            this.bottomPanel.TabIndex = 10;
            // 
            // _btnCancel
            // 
            this._btnCancel.AutoSize = true;
            this._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._btnCancel.Name = "_btnCancel";
            this._btnCancel.TabIndex = 11;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;
            // 
            // _btnOk
            // 
            this._btnOk.AutoSize = true;
            this._btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._btnOk.Name = "_btnOk";
            this._btnOk.TabIndex = 12;
            this._btnOk.Text = "Apply";
            this._btnOk.UseVisualStyleBackColor = true;
            // 
            // _txtName
            // 
            this._txtName.MaxLength = 20;
            this._txtName.Name = "_txtName";
            this._txtName.Size = new System.Drawing.Size(180, 23);
            this._txtName.TabIndex = 13;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Text = "Name:";
            // 
            // HueFromImageForm
            // 
            this.AcceptButton = this._btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(744, 721);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(520, 480);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New hue from an image";
            this.Controls.Add(this.previewHost);
            this.Controls.Add(this._strip);
            this.Controls.Add(this._image);
            this.Controls.Add(this.settingsPanel);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.bottomPanel);
            this.Name = "HueFromImageForm";
            this.previewHost.ResumeLayout(false);
            this.previewHost.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._strip)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
            this.settingsPanel.ResumeLayout(false);
            this.settingsPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._trackSmooth)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            this.bottomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel previewHost;
        private UoFiddler.Controls.UserControls.HuePreviewPanel _preview;
        private System.Windows.Forms.PictureBox _strip;
        private System.Windows.Forms.PictureBox _image;
        private System.Windows.Forms.FlowLayoutPanel settingsPanel;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnWhole;
        private System.Windows.Forms.CheckBox _chkIgnoreDark;
        private System.Windows.Forms.Label smoothLabel;
        private System.Windows.Forms.TrackBar _trackSmooth;
        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.FlowLayoutPanel bottomPanel;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Button _btnOk;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Label nameLabel;
    }
}

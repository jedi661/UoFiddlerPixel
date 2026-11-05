namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
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
            if (disposing && (components != null))
            {
                components.Dispose();
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
            grid = new UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.TileGridView();
            panelTop = new System.Windows.Forms.Panel();
            lblCount = new System.Windows.Forms.Label();
            cmbSize = new System.Windows.Forms.ComboBox();
            txtFilter = new System.Windows.Forms.TextBox();
            lblFilter = new System.Windows.Forms.Label();
            cmbMode = new System.Windows.Forms.ComboBox();
            lblMode = new System.Windows.Forms.Label();
            panelBottom = new System.Windows.Forms.Panel();
            picPreview = new System.Windows.Forms.PictureBox();
            lblInfo = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            //
            // grid
            //
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Location = new System.Drawing.Point(0, 38);
            grid.Name = "grid";
            grid.Size = new System.Drawing.Size(884, 455);
            grid.TabIndex = 0;
            grid.SelectedIdChanged += Grid_SelectedIdChanged;
            grid.ItemActivated += Grid_ItemActivated;
            //
            // panelTop
            //
            panelTop.Controls.Add(lblCount);
            panelTop.Controls.Add(cmbSize);
            panelTop.Controls.Add(txtFilter);
            panelTop.Controls.Add(lblFilter);
            panelTop.Controls.Add(cmbMode);
            panelTop.Controls.Add(lblMode);
            panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            panelTop.Location = new System.Drawing.Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new System.Drawing.Size(884, 38);
            panelTop.TabIndex = 1;
            //
            // lblMode
            //
            lblMode.AutoSize = true;
            lblMode.Location = new System.Drawing.Point(8, 11);
            lblMode.Name = "lblMode";
            lblMode.Size = new System.Drawing.Size(42, 15);
            lblMode.TabIndex = 0;
            lblMode.Text = "Zeigen:";
            //
            // cmbMode
            //
            cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbMode.Location = new System.Drawing.Point(60, 7);
            cmbMode.Name = "cmbMode";
            cmbMode.Size = new System.Drawing.Size(200, 23);
            cmbMode.TabIndex = 1;
            cmbMode.SelectedIndexChanged += CmbMode_SelectedIndexChanged;
            //
            // lblFilter
            //
            lblFilter.AutoSize = true;
            lblFilter.Location = new System.Drawing.Point(274, 11);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new System.Drawing.Size(42, 15);
            lblFilter.TabIndex = 2;
            lblFilter.Text = "Suche:";
            //
            // txtFilter
            //
            txtFilter.Location = new System.Drawing.Point(322, 7);
            txtFilter.Name = "txtFilter";
            txtFilter.PlaceholderText = "Name oder ID (hex)";
            txtFilter.Size = new System.Drawing.Size(200, 23);
            txtFilter.TabIndex = 3;
            txtFilter.TextChanged += TxtFilter_TextChanged;
            //
            // cmbSize
            //
            cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSize.Location = new System.Drawing.Point(536, 7);
            cmbSize.Name = "cmbSize";
            cmbSize.Size = new System.Drawing.Size(100, 23);
            cmbSize.TabIndex = 4;
            cmbSize.SelectedIndexChanged += CmbSize_SelectedIndexChanged;
            //
            // lblCount
            //
            lblCount.AutoSize = true;
            lblCount.Location = new System.Drawing.Point(650, 11);
            lblCount.Name = "lblCount";
            lblCount.Size = new System.Drawing.Size(12, 15);
            lblCount.TabIndex = 5;
            lblCount.Text = "-";
            //
            // panelBottom
            //
            panelBottom.Controls.Add(picPreview);
            panelBottom.Controls.Add(lblInfo);
            panelBottom.Controls.Add(btnOk);
            panelBottom.Controls.Add(btnCancel);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Location = new System.Drawing.Point(0, 493);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new System.Drawing.Size(884, 108);
            panelBottom.TabIndex = 2;
            //
            // picPreview
            //
            picPreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picPreview.Location = new System.Drawing.Point(8, 6);
            picPreview.Name = "picPreview";
            picPreview.Size = new System.Drawing.Size(96, 96);
            picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            //
            // lblInfo
            //
            lblInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lblInfo.Location = new System.Drawing.Point(112, 8);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new System.Drawing.Size(578, 92);
            lblInfo.TabIndex = 1;
            lblInfo.Text = "-";
            //
            // btnOk
            //
            btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(698, 73);
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(85, 27);
            btnOk.TabIndex = 2;
            btnOk.Text = "Übernehmen";
            btnOk.UseVisualStyleBackColor = true;
            //
            // btnCancel
            //
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(789, 73);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(85, 27);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // TilePickerForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(884, 601);
            Controls.Add(grid);
            Controls.Add(panelTop);
            Controls.Add(panelBottom);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(520, 360);
            Name = "TilePickerForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Katalog – Land-Tiles, Texturen, Items";
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls.TileGridView grid;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.ComboBox cmbSize;
        private System.Windows.Forms.TextBox txtFilter;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ComboBox cmbMode;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}

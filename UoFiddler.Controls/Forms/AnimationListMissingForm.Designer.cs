/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class AnimationListMissingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationListMissingForm));
            _previewBox = new System.Windows.Forms.PictureBox();
            _lblInfo = new System.Windows.Forms.Label();
            _lblProgress = new System.Windows.Forms.Label();
            _lblFacing = new System.Windows.Forms.Label();
            _trackFacing = new System.Windows.Forms.TrackBar();
            _lblName = new System.Windows.Forms.Label();
            _txtName = new System.Windows.Forms.TextBox();
            _lblType = new System.Windows.Forms.Label();
            _cmbType = new System.Windows.Forms.ComboBox();
            _separator = new System.Windows.Forms.Panel();
            _btnAccept = new System.Windows.Forms.Button();
            _btnSkip = new System.Windows.Forms.Button();
            _btnAcceptAll = new System.Windows.Forms.Button();
            _btnCancel = new System.Windows.Forms.Button();
            _animTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)_previewBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_trackFacing).BeginInit();
            SuspendLayout();
            // 
            // _previewBox
            // 
            _previewBox.BackColor = System.Drawing.SystemColors.Control;
            _previewBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _previewBox.Location = new System.Drawing.Point(12, 12);
            _previewBox.Name = "_previewBox";
            _previewBox.Size = new System.Drawing.Size(400, 300);
            _previewBox.TabIndex = 0;
            _previewBox.TabStop = false;
            _previewBox.Paint += PreviewBox_Paint;
            // 
            // _lblInfo
            // 
            _lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            _lblInfo.ForeColor = System.Drawing.Color.Navy;
            _lblInfo.Location = new System.Drawing.Point(420, 12);
            _lblInfo.Name = "_lblInfo";
            _lblInfo.Size = new System.Drawing.Size(220, 56);
            _lblInfo.TabIndex = 0;
            // 
            // _lblProgress
            // 
            _lblProgress.ForeColor = System.Drawing.Color.DimGray;
            _lblProgress.Location = new System.Drawing.Point(420, 75);
            _lblProgress.Name = "_lblProgress";
            _lblProgress.Size = new System.Drawing.Size(220, 20);
            _lblProgress.TabIndex = 1;
            // 
            // _lblFacing
            // 
            _lblFacing.Location = new System.Drawing.Point(12, 325);
            _lblFacing.Name = "_lblFacing";
            _lblFacing.Size = new System.Drawing.Size(60, 18);
            _lblFacing.TabIndex = 2;
            _lblFacing.Text = "Direction:";
            // 
            // _trackFacing
            // 
            _trackFacing.Location = new System.Drawing.Point(76, 314);
            _trackFacing.Maximum = 7;
            _trackFacing.Name = "_trackFacing";
            _trackFacing.Size = new System.Drawing.Size(200, 45);
            _trackFacing.TabIndex = 3;
            _trackFacing.TickStyle = System.Windows.Forms.TickStyle.Both;
            _trackFacing.Value = 1;
            _trackFacing.ValueChanged += TrackFacing_ValueChanged;
            // 
            // _lblName
            // 
            _lblName.Location = new System.Drawing.Point(12, 372);
            _lblName.Name = "_lblName";
            _lblName.Size = new System.Drawing.Size(50, 20);
            _lblName.TabIndex = 4;
            _lblName.Text = "Name:";
            // 
            // _txtName
            // 
            _txtName.Location = new System.Drawing.Point(65, 369);
            _txtName.Name = "_txtName";
            _txtName.Size = new System.Drawing.Size(347, 23);
            _txtName.TabIndex = 5;
            // 
            // _lblType
            // 
            _lblType.Location = new System.Drawing.Point(12, 408);
            _lblType.Name = "_lblType";
            _lblType.Size = new System.Drawing.Size(50, 20);
            _lblType.TabIndex = 6;
            _lblType.Text = "Typ:";
            // 
            // _cmbType
            // 
            _cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _cmbType.FormattingEnabled = true;
            _cmbType.Items.AddRange(new object[] { "0 – Monster", "1 – See / Sea", "2 – Tier / Animal", "3 – Human / Equipment" });
            _cmbType.Location = new System.Drawing.Point(65, 405);
            _cmbType.Name = "_cmbType";
            _cmbType.Size = new System.Drawing.Size(250, 23);
            _cmbType.TabIndex = 7;
            // 
            // _separator
            // 
            _separator.BackColor = System.Drawing.Color.LightGray;
            _separator.Location = new System.Drawing.Point(0, 438);
            _separator.Name = "_separator";
            _separator.Size = new System.Drawing.Size(660, 2);
            _separator.TabIndex = 8;
            // 
            // _btnAccept
            // 
            _btnAccept.BackColor = System.Drawing.Color.MediumSeaGreen;
            _btnAccept.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnAccept.ForeColor = System.Drawing.Color.White;
            _btnAccept.Location = new System.Drawing.Point(12, 450);
            _btnAccept.Name = "_btnAccept";
            _btnAccept.Size = new System.Drawing.Size(130, 34);
            _btnAccept.TabIndex = 8;
            _btnAccept.Text = "✔ Take over";
            _btnAccept.UseVisualStyleBackColor = false;
            _btnAccept.Click += OnAccept;
            // 
            // _btnSkip
            // 
            _btnSkip.Location = new System.Drawing.Point(155, 450);
            _btnSkip.Name = "_btnSkip";
            _btnSkip.Size = new System.Drawing.Size(75, 34);
            _btnSkip.TabIndex = 9;
            _btnSkip.Text = "→ Skip";
            _btnSkip.UseVisualStyleBackColor = true;
            _btnSkip.Click += OnSkip;
            // 
            // _btnAcceptAll
            // 
            _btnAcceptAll.BackColor = System.Drawing.Color.SteelBlue;
            _btnAcceptAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnAcceptAll.ForeColor = System.Drawing.Color.White;
            _btnAcceptAll.Location = new System.Drawing.Point(245, 450);
            _btnAcceptAll.Name = "_btnAcceptAll";
            _btnAcceptAll.Size = new System.Drawing.Size(140, 34);
            _btnAcceptAll.TabIndex = 10;
            _btnAcceptAll.Text = "⚡ Everyone takes over";
            _btnAcceptAll.UseVisualStyleBackColor = false;
            _btnAcceptAll.Click += OnAcceptAll;
            // 
            // _btnCancel
            // 
            _btnCancel.BackColor = System.Drawing.Color.IndianRed;
            _btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            _btnCancel.ForeColor = System.Drawing.Color.White;
            _btnCancel.Location = new System.Drawing.Point(391, 450);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new System.Drawing.Size(100, 34);
            _btnCancel.TabIndex = 11;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = false;
            _btnCancel.Click += OnCancel;
            // 
            // _animTimer
            // 
            _animTimer.Interval = 150;
            _animTimer.Tick += OnAnimTick;
            // 
            // AnimationListMissingForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(721, 496);
            Controls.Add(_previewBox);
            Controls.Add(_lblInfo);
            Controls.Add(_lblProgress);
            Controls.Add(_lblFacing);
            Controls.Add(_trackFacing);
            Controls.Add(_lblName);
            Controls.Add(_txtName);
            Controls.Add(_lblType);
            Controls.Add(_cmbType);
            Controls.Add(_separator);
            Controls.Add(_btnAccept);
            Controls.Add(_btnSkip);
            Controls.Add(_btnAcceptAll);
            Controls.Add(_btnCancel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(672, 535);
            Name = "AnimationListMissingForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Missing animations – step by step";
            ((System.ComponentModel.ISupportInitialize)_previewBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)_trackFacing).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // ── Designer-Felder ──────────────────────────────────────────────────
        private System.Windows.Forms.PictureBox _previewBox;
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.Label _lblProgress;
        private System.Windows.Forms.Label _lblFacing;
        private System.Windows.Forms.TrackBar _trackFacing;
        private System.Windows.Forms.Label _lblName;
        private System.Windows.Forms.TextBox _txtName;
        private System.Windows.Forms.Label _lblType;
        private System.Windows.Forms.ComboBox _cmbType;
        private System.Windows.Forms.Panel _separator;
        private System.Windows.Forms.Button _btnAccept;
        private System.Windows.Forms.Button _btnSkip;
        private System.Windows.Forms.Button _btnAcceptAll;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Timer _animTimer;
    }
}
/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class EffectParticleForm
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
            components = new System.ComponentModel.Container();
            viewPreview = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            pnlSettings = new System.Windows.Forms.Panel();
            lblType = new System.Windows.Forms.Label();
            cbType = new System.Windows.Forms.ComboBox();
            lblFrames = new System.Windows.Forms.Label();
            numFrames = new System.Windows.Forms.NumericUpDown();
            lblSize = new System.Windows.Forms.Label();
            numSize = new System.Windows.Forms.NumericUpDown();
            lblCount = new System.Windows.Forms.Label();
            numCount = new System.Windows.Forms.NumericUpDown();
            lblColor = new System.Windows.Forms.Label();
            pnlColor = new System.Windows.Forms.Panel();
            lblSeed = new System.Windows.Forms.Label();
            numSeed = new System.Windows.Forms.NumericUpDown();
            btnNewSeed = new System.Windows.Forms.Button();
            chkGrayscale = new System.Windows.Forms.CheckBox();
            lblPreviewMs = new System.Windows.Forms.Label();
            numPreviewMs = new System.Windows.Forms.NumericUpDown();
            lblHint = new System.Windows.Forms.Label();
            pnlButtons = new System.Windows.Forms.Panel();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            timerPreview = new System.Windows.Forms.Timer(components);
            pnlSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFrames).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCount).BeginInit();
            pnlColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSeed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPreviewMs).BeginInit();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // viewPreview
            // 
            viewPreview.DarkBackground = true;
            viewPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            viewPreview.Location = new System.Drawing.Point(270, 0);
            viewPreview.Zoom = 2;
            viewPreview.Size = new System.Drawing.Size(474, 421);
            viewPreview.Name = "viewPreview";
            viewPreview.TabIndex = 0;
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(lblType);
            pnlSettings.Controls.Add(cbType);
            pnlSettings.Controls.Add(lblFrames);
            pnlSettings.Controls.Add(numFrames);
            pnlSettings.Controls.Add(lblSize);
            pnlSettings.Controls.Add(numSize);
            pnlSettings.Controls.Add(lblCount);
            pnlSettings.Controls.Add(numCount);
            pnlSettings.Controls.Add(lblColor);
            pnlSettings.Controls.Add(pnlColor);
            pnlSettings.Controls.Add(lblSeed);
            pnlSettings.Controls.Add(numSeed);
            pnlSettings.Controls.Add(btnNewSeed);
            pnlSettings.Controls.Add(chkGrayscale);
            pnlSettings.Controls.Add(lblPreviewMs);
            pnlSettings.Controls.Add(numPreviewMs);
            pnlSettings.Controls.Add(lblHint);
            pnlSettings.Dock = System.Windows.Forms.DockStyle.Left;
            pnlSettings.Location = new System.Drawing.Point(0, 0);
            pnlSettings.Size = new System.Drawing.Size(270, 421);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.TabIndex = 1;
            // 
            // lblType
            // 
            lblType.AutoSize = true;
            lblType.Location = new System.Drawing.Point(8, 13);
            lblType.Text = "Art:";
            lblType.Name = "lblType";
            // 
            // cbType
            // 
            cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbType.FormattingEnabled = true;
            cbType.Items.AddRange(new object[] { "Funken", "Feuer", "Rauch", "Blitz", "Magie-Ring", "Explosion", "Heilung (aufsteigend)" });
            cbType.Location = new System.Drawing.Point(90, 10);
            cbType.Size = new System.Drawing.Size(170, 23);
            cbType.Name = "cbType";
            cbType.TabIndex = 2;
            cbType.SelectedIndexChanged += CbType_SelectedIndexChanged;
            // 
            // lblFrames
            // 
            lblFrames.AutoSize = true;
            lblFrames.Location = new System.Drawing.Point(8, 45);
            lblFrames.Text = "Frames:";
            lblFrames.Name = "lblFrames";
            // 
            // numFrames
            // 
            numFrames.Location = new System.Drawing.Point(90, 42);
            numFrames.Size = new System.Drawing.Size(60, 23);
            numFrames.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            numFrames.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numFrames.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numFrames.Name = "numFrames";
            numFrames.TabIndex = 3;
            numFrames.ValueChanged += Settings_Changed;
            // 
            // lblSize
            // 
            lblSize.AutoSize = true;
            lblSize.Location = new System.Drawing.Point(8, 75);
            lblSize.Text = "Größe px:";
            lblSize.Name = "lblSize";
            // 
            // numSize
            // 
            numSize.Location = new System.Drawing.Point(90, 72);
            numSize.Size = new System.Drawing.Size(60, 23);
            numSize.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numSize.Minimum = new decimal(new int[] { 16, 0, 0, 0 });
            numSize.Value = new decimal(new int[] { 96, 0, 0, 0 });
            numSize.Name = "numSize";
            numSize.TabIndex = 4;
            numSize.ValueChanged += Settings_Changed;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.Location = new System.Drawing.Point(8, 105);
            lblCount.Text = "Partikel:";
            lblCount.Name = "lblCount";
            // 
            // numCount
            // 
            numCount.Location = new System.Drawing.Point(90, 102);
            numCount.Size = new System.Drawing.Size(60, 23);
            numCount.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Value = new decimal(new int[] { 60, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.TabIndex = 5;
            numCount.ValueChanged += Settings_Changed;
            // 
            // lblColor
            // 
            lblColor.AutoSize = true;
            lblColor.Location = new System.Drawing.Point(8, 135);
            lblColor.Text = "Farbe:";
            lblColor.Name = "lblColor";
            // 
            // pnlColor
            // 
            pnlColor.BackColor = System.Drawing.Color.Orange;
            pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlColor.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlColor.Location = new System.Drawing.Point(90, 132);
            pnlColor.Size = new System.Drawing.Size(30, 22);
            pnlColor.Name = "pnlColor";
            pnlColor.TabIndex = 6;
            pnlColor.Click += PnlColor_Click;
            // 
            // lblSeed
            // 
            lblSeed.AutoSize = true;
            lblSeed.Location = new System.Drawing.Point(8, 165);
            lblSeed.Text = "Zufall (Seed):";
            lblSeed.Name = "lblSeed";
            // 
            // numSeed
            // 
            numSeed.Location = new System.Drawing.Point(90, 162);
            numSeed.Size = new System.Drawing.Size(70, 23);
            numSeed.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            numSeed.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numSeed.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numSeed.Name = "numSeed";
            numSeed.TabIndex = 7;
            numSeed.ValueChanged += Settings_Changed;
            // 
            // btnNewSeed
            // 
            btnNewSeed.Location = new System.Drawing.Point(166, 160);
            btnNewSeed.Size = new System.Drawing.Size(94, 26);
            btnNewSeed.Text = "Neu würfeln";
            btnNewSeed.UseVisualStyleBackColor = true;
            btnNewSeed.Name = "btnNewSeed";
            btnNewSeed.TabIndex = 8;
            btnNewSeed.Click += BtnNewSeed_Click;
            // 
            // chkGrayscale
            // 
            chkGrayscale.AutoSize = true;
            chkGrayscale.Location = new System.Drawing.Point(8, 194);
            chkGrayscale.Text = "Graustufen (später per Hue färben)";
            chkGrayscale.UseVisualStyleBackColor = true;
            chkGrayscale.Name = "chkGrayscale";
            chkGrayscale.TabIndex = 9;
            chkGrayscale.CheckedChanged += Settings_Changed;
            // 
            // lblPreviewMs
            // 
            lblPreviewMs.AutoSize = true;
            lblPreviewMs.Location = new System.Drawing.Point(8, 225);
            lblPreviewMs.Text = "Vorschau ms:";
            lblPreviewMs.Name = "lblPreviewMs";
            // 
            // numPreviewMs
            // 
            numPreviewMs.Location = new System.Drawing.Point(90, 222);
            numPreviewMs.Size = new System.Drawing.Size(60, 23);
            numPreviewMs.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numPreviewMs.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numPreviewMs.Value = new decimal(new int[] { 90, 0, 0, 0 });
            numPreviewMs.Name = "numPreviewMs";
            numPreviewMs.TabIndex = 10;
            numPreviewMs.ValueChanged += NumPreviewMs_ValueChanged;
            // 
            // lblHint
            // 
            lblHint.AutoSize = true;
            lblHint.Location = new System.Drawing.Point(8, 259);
            lblHint.Text = "Die Frames werden auf Schwarz erzeugt und\r\nim Editor additiv (Schwarz = transparent)\r\nverwendet. Feuer, Rauch und Heilung\r\nlaufen als nahtlose Schleife.";
            lblHint.Name = "lblHint";
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnOk);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlButtons.Location = new System.Drawing.Point(0, 421);
            pnlButtons.Size = new System.Drawing.Size(744, 40);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.TabIndex = 11;
            // 
            // btnOk
            // 
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(540, 7);
            btnOk.Size = new System.Drawing.Size(110, 26);
            btnOk.Text = "Übernehmen";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 12;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(656, 7);
            btnCancel.Size = new System.Drawing.Size(80, 26);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 13;
            // 
            // timerPreview
            // 
            timerPreview.Interval = 90;
            timerPreview.Tick += TimerPreview_Tick;
            // 
            // EffectParticleForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(744, 461);
            Controls.Add(viewPreview);
            Controls.Add(pnlSettings);
            Controls.Add(pnlButtons);
            MinimumSize = new System.Drawing.Size(600, 400);
            Name = "EffectParticleForm";
            Text = "Partikel-Generator";
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            pnlButtons.ResumeLayout(false);
            pnlButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPreviewMs).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSeed).EndInit();
            pnlColor.ResumeLayout(false);
            pnlColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFrames).EndInit();
            pnlSettings.ResumeLayout(false);
            pnlSettings.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewPreview;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.ComboBox cbType;
        private System.Windows.Forms.Label lblFrames;
        private System.Windows.Forms.NumericUpDown numFrames;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.NumericUpDown numCount;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label lblSeed;
        private System.Windows.Forms.NumericUpDown numSeed;
        private System.Windows.Forms.Button btnNewSeed;
        private System.Windows.Forms.CheckBox chkGrayscale;
        private System.Windows.Forms.Label lblPreviewMs;
        private System.Windows.Forms.NumericUpDown numPreviewMs;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Timer timerPreview;
    }
}

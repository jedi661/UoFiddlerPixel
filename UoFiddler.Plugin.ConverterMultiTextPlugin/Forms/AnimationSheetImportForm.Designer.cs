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
    partial class AnimationSheetImportForm
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
            viewSheet = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            pnlTop = new System.Windows.Forms.Panel();
            lblColumns = new System.Windows.Forms.Label();
            numColumns = new System.Windows.Forms.NumericUpDown();
            lblRows = new System.Windows.Forms.Label();
            numRows = new System.Windows.Forms.NumericUpDown();
            chkCropUnion = new System.Windows.Forms.CheckBox();
            lblMapping = new System.Windows.Forms.Label();
            cbMapping = new System.Windows.Forms.ComboBox();
            chkRemoveBackground = new System.Windows.Forms.CheckBox();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numColumns).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRows).BeginInit();
            SuspendLayout();
            // 
            // viewSheet
            // 
            viewSheet.Dock = System.Windows.Forms.DockStyle.Fill;
            viewSheet.Location = new System.Drawing.Point(0, 92);
            viewSheet.Size = new System.Drawing.Size(804, 469);
            viewSheet.Name = "viewSheet";
            viewSheet.TabIndex = 0;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblColumns);
            pnlTop.Controls.Add(numColumns);
            pnlTop.Controls.Add(lblRows);
            pnlTop.Controls.Add(numRows);
            pnlTop.Controls.Add(chkCropUnion);
            pnlTop.Controls.Add(lblMapping);
            pnlTop.Controls.Add(cbMapping);
            pnlTop.Controls.Add(chkRemoveBackground);
            pnlTop.Controls.Add(btnOk);
            pnlTop.Controls.Add(btnCancel);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Size = new System.Drawing.Size(804, 92);
            pnlTop.Name = "pnlTop";
            pnlTop.TabIndex = 1;
            // 
            // lblColumns
            // 
            lblColumns.AutoSize = true;
            lblColumns.Location = new System.Drawing.Point(8, 11);
            lblColumns.Text = "Spalten (Frames):";
            lblColumns.Name = "lblColumns";
            // 
            // numColumns
            // 
            numColumns.Location = new System.Drawing.Point(120, 8);
            numColumns.Size = new System.Drawing.Size(60, 23);
            numColumns.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numColumns.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numColumns.Value = new decimal(new int[] { 8, 0, 0, 0 });
            numColumns.Name = "numColumns";
            numColumns.TabIndex = 2;
            numColumns.ValueChanged += Grid_Changed;
            // 
            // lblRows
            // 
            lblRows.AutoSize = true;
            lblRows.Location = new System.Drawing.Point(190, 11);
            lblRows.Text = "Reihen:";
            lblRows.Name = "lblRows";
            // 
            // numRows
            // 
            numRows.Location = new System.Drawing.Point(240, 8);
            numRows.Size = new System.Drawing.Size(60, 23);
            numRows.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
            numRows.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRows.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numRows.Name = "numRows";
            numRows.TabIndex = 3;
            numRows.ValueChanged += Grid_Changed;
            // 
            // chkCropUnion
            // 
            chkCropUnion.Checked = true;
            chkCropUnion.CheckState = System.Windows.Forms.CheckState.Checked;
            chkCropUnion.AutoSize = true;
            chkCropUnion.Location = new System.Drawing.Point(320, 10);
            chkCropUnion.Text = "Gemeinsam zuschneiden";
            chkCropUnion.UseVisualStyleBackColor = true;
            chkCropUnion.Name = "chkCropUnion";
            chkCropUnion.TabIndex = 4;
            // 
            // lblMapping
            // 
            lblMapping.AutoSize = true;
            lblMapping.Location = new System.Drawing.Point(8, 41);
            lblMapping.Text = "Reihenfolge:";
            lblMapping.Name = "lblMapping";
            // 
            // cbMapping
            // 
            cbMapping.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbMapping.FormattingEnabled = true;
            cbMapping.Items.AddRange(new object[] { "5 Reihen in UO-Reihenfolge (↓, ↙, ←, ↖, ↑)", "5 Reihen rechte Seite (↑, ↗, →, ↘, ↓) – wird automatisch gespiegelt", "8 Reihen Spielrichtung (↗ N, → NO, ↘ O, ↓ SO, ↙ S, ← SW, ↖ W, ↑ NW)", "8 Reihen ab unten im Uhrzeigersinn (↓, ↙, ←, ↖, ↑, ↗, →, ↘)", "Alle Zellen → aktuell gewählte Richtung" });
            cbMapping.Location = new System.Drawing.Point(120, 38);
            cbMapping.Size = new System.Drawing.Size(460, 23);
            cbMapping.Name = "cbMapping";
            cbMapping.TabIndex = 5;
            cbMapping.SelectedIndexChanged += CbMapping_SelectedIndexChanged;
            // 
            // chkRemoveBackground
            // 
            chkRemoveBackground.AutoSize = true;
            chkRemoveBackground.Location = new System.Drawing.Point(8, 66);
            chkRemoveBackground.Text = "Hintergrund (Farbe oben links) entfernen";
            chkRemoveBackground.UseVisualStyleBackColor = true;
            chkRemoveBackground.Name = "chkRemoveBackground";
            chkRemoveBackground.TabIndex = 6;
            // 
            // btnOk
            // 
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(600, 36);
            btnOk.Size = new System.Drawing.Size(100, 26);
            btnOk.Text = "Importieren";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 7;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(706, 36);
            btnCancel.Size = new System.Drawing.Size(90, 26);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 8;
            // 
            // AnimationSheetImportForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(804, 561);
            Controls.Add(viewSheet);
            Controls.Add(pnlTop);
            Name = "AnimationSheetImportForm";
            Text = "Sprite-Sheet aufteilen";
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            MinimizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)numRows).EndInit();
            ((System.ComponentModel.ISupportInitialize)numColumns).EndInit();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewSheet;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.NumericUpDown numColumns;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.NumericUpDown numRows;
        private System.Windows.Forms.CheckBox chkCropUnion;
        private System.Windows.Forms.Label lblMapping;
        private System.Windows.Forms.ComboBox cbMapping;
        private System.Windows.Forms.CheckBox chkRemoveBackground;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}

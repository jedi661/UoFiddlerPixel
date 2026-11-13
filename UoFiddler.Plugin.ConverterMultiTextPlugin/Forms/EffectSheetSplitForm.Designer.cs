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
    partial class EffectSheetSplitForm
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
            lblColumns = new System.Windows.Forms.Label();
            numColumns = new System.Windows.Forms.NumericUpDown();
            lblRows = new System.Windows.Forms.Label();
            numRows = new System.Windows.Forms.NumericUpDown();
            lblSheetSize = new System.Windows.Forms.Label();
            btnOk = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numColumns).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRows).BeginInit();
            SuspendLayout();
            // 
            // lblColumns
            // 
            lblColumns.AutoSize = true;
            lblColumns.Location = new System.Drawing.Point(12, 15);
            lblColumns.Text = "Spalten:";
            lblColumns.Name = "lblColumns";
            // 
            // numColumns
            // 
            numColumns.Location = new System.Drawing.Point(120, 12);
            numColumns.Size = new System.Drawing.Size(70, 23);
            numColumns.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numColumns.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numColumns.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numColumns.Name = "numColumns";
            numColumns.TabIndex = 0;
            // 
            // lblRows
            // 
            lblRows.AutoSize = true;
            lblRows.Location = new System.Drawing.Point(12, 45);
            lblRows.Text = "Reihen:";
            lblRows.Name = "lblRows";
            // 
            // numRows
            // 
            numRows.Location = new System.Drawing.Point(120, 42);
            numRows.Size = new System.Drawing.Size(70, 23);
            numRows.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numRows.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRows.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numRows.Name = "numRows";
            numRows.TabIndex = 1;
            // 
            // lblSheetSize
            // 
            lblSheetSize.AutoSize = true;
            lblSheetSize.Location = new System.Drawing.Point(200, 15);
            lblSheetSize.Text = "Sheet:";
            lblSheetSize.Name = "lblSheetSize";
            // 
            // btnOk
            // 
            btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOk.Location = new System.Drawing.Point(120, 84);
            btnOk.Size = new System.Drawing.Size(80, 26);
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(206, 84);
            btnCancel.Size = new System.Drawing.Size(86, 26);
            btnCancel.Text = "Abbrechen";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 3;
            // 
            // EffectSheetSplitForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(304, 131);
            Controls.Add(lblColumns);
            Controls.Add(numColumns);
            Controls.Add(lblRows);
            Controls.Add(numRows);
            Controls.Add(lblSheetSize);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);
            Name = "EffectSheetSplitForm";
            Text = "Sprite-Sheet aufteilen";
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)numRows).EndInit();
            ((System.ComponentModel.ISupportInitialize)numColumns).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.NumericUpDown numColumns;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.NumericUpDown numRows;
        private System.Windows.Forms.Label lblSheetSize;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}

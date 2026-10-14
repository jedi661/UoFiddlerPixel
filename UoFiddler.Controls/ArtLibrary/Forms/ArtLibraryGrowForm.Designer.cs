// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    partial class ArtLibraryGrowForm
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
            labelCurrent = new System.Windows.Forms.Label();
            labelNew = new System.Windows.Forms.Label();
            numericUpDownCapacity = new System.Windows.Forms.NumericUpDown();
            labelInfo = new System.Windows.Forms.Label();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCapacity).BeginInit();
            SuspendLayout();
            //
            // labelCurrent
            //
            labelCurrent.AutoSize = true;
            labelCurrent.Location = new System.Drawing.Point(12, 15);
            labelCurrent.Name = "labelCurrent";
            labelCurrent.Size = new System.Drawing.Size(50, 15);
            labelCurrent.TabIndex = 0;
            labelCurrent.Text = "Aktuell:";
            //
            // labelNew
            //
            labelNew.AutoSize = true;
            labelNew.Location = new System.Drawing.Point(12, 46);
            labelNew.Name = "labelNew";
            labelNew.Size = new System.Drawing.Size(95, 15);
            labelNew.TabIndex = 1;
            labelNew.Text = "Neue Anzahl IDs:";
            //
            // numericUpDownCapacity
            //
            numericUpDownCapacity.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDownCapacity.Location = new System.Drawing.Point(120, 44);
            numericUpDownCapacity.Name = "numericUpDownCapacity";
            numericUpDownCapacity.Size = new System.Drawing.Size(150, 23);
            numericUpDownCapacity.TabIndex = 2;
            numericUpDownCapacity.ThousandsSeparator = true;
            numericUpDownCapacity.ValueChanged += NumericUpDownCapacity_ValueChanged;
            //
            // labelInfo
            //
            labelInfo.AutoSize = true;
            labelInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelInfo.Location = new System.Drawing.Point(120, 72);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(0, 15);
            labelInfo.TabIndex = 3;
            //
            // buttonOk
            //
            buttonOk.Location = new System.Drawing.Point(226, 104);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(85, 27);
            buttonOk.TabIndex = 4;
            buttonOk.Text = "Erweitern";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += ButtonOk_Click;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(317, 104);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(85, 27);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // ArtLibraryGrowForm
            //
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(414, 143);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(labelInfo);
            Controls.Add(numericUpDownCapacity);
            Controls.Add(labelNew);
            Controls.Add(labelCurrent);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ArtLibraryGrowForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Bibliothek erweitern";
            ((System.ComponentModel.ISupportInitialize)numericUpDownCapacity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelCurrent;
        private System.Windows.Forms.Label labelNew;
        private System.Windows.Forms.NumericUpDown numericUpDownCapacity;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}

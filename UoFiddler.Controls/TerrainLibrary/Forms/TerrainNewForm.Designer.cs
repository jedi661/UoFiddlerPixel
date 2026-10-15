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

namespace UoFiddler.Controls.TerrainLibrary.Forms
{
    partial class TerrainNewForm
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
            labelName = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            labelDirectory = new System.Windows.Forms.Label();
            textBoxDirectory = new System.Windows.Forms.TextBox();
            buttonBrowse = new System.Windows.Forms.Button();
            labelCapacity = new System.Windows.Forms.Label();
            numericUpDownCapacity = new System.Windows.Forms.NumericUpDown();
            labelSizeInfo = new System.Windows.Forms.Label();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)numericUpDownCapacity).BeginInit();
            SuspendLayout();
            //
            // labelName
            //
            labelName.AutoSize = true;
            labelName.Location = new System.Drawing.Point(12, 18);
            labelName.Name = "labelName";
            labelName.Size = new System.Drawing.Size(42, 15);
            labelName.TabIndex = 0;
            labelName.Text = "Name:";
            //
            // textBoxName
            //
            textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxName.Location = new System.Drawing.Point(110, 15);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new System.Drawing.Size(352, 23);
            textBoxName.TabIndex = 1;
            //
            // labelDirectory
            //
            labelDirectory.AutoSize = true;
            labelDirectory.Location = new System.Drawing.Point(12, 50);
            labelDirectory.Name = "labelDirectory";
            labelDirectory.Size = new System.Drawing.Size(46, 15);
            labelDirectory.TabIndex = 2;
            labelDirectory.Text = "Ordner:";
            //
            // textBoxDirectory
            //
            textBoxDirectory.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxDirectory.Location = new System.Drawing.Point(110, 47);
            textBoxDirectory.Name = "textBoxDirectory";
            textBoxDirectory.Size = new System.Drawing.Size(316, 23);
            textBoxDirectory.TabIndex = 3;
            //
            // buttonBrowse
            //
            buttonBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonBrowse.Location = new System.Drawing.Point(432, 46);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(30, 25);
            buttonBrowse.TabIndex = 4;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += ButtonBrowse_Click;
            //
            // labelCapacity
            //
            labelCapacity.AutoSize = true;
            labelCapacity.Location = new System.Drawing.Point(12, 82);
            labelCapacity.Name = "labelCapacity";
            labelCapacity.Size = new System.Drawing.Size(84, 15);
            labelCapacity.TabIndex = 5;
            labelCapacity.Text = "Anzahl Slots:";
            //
            // numericUpDownCapacity
            //
            numericUpDownCapacity.Increment = new decimal(new int[] { 100000, 0, 0, 0 });
            numericUpDownCapacity.Location = new System.Drawing.Point(110, 80);
            numericUpDownCapacity.Name = "numericUpDownCapacity";
            numericUpDownCapacity.Size = new System.Drawing.Size(140, 23);
            numericUpDownCapacity.TabIndex = 6;
            numericUpDownCapacity.ThousandsSeparator = true;
            numericUpDownCapacity.ValueChanged += NumericUpDownCapacity_ValueChanged;
            //
            // labelSizeInfo
            //
            labelSizeInfo.AutoSize = true;
            labelSizeInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            labelSizeInfo.Location = new System.Drawing.Point(110, 108);
            labelSizeInfo.Name = "labelSizeInfo";
            labelSizeInfo.Size = new System.Drawing.Size(0, 15);
            labelSizeInfo.TabIndex = 7;
            //
            // buttonOk
            //
            buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOk.Location = new System.Drawing.Point(306, 140);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(75, 27);
            buttonOk.TabIndex = 8;
            buttonOk.Text = "Anlegen";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += ButtonOk_Click;
            //
            // buttonCancel
            //
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(387, 140);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(75, 27);
            buttonCancel.TabIndex = 9;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // TerrainNewForm
            //
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(474, 179);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOk);
            Controls.Add(labelSizeInfo);
            Controls.Add(numericUpDownCapacity);
            Controls.Add(labelCapacity);
            Controls.Add(buttonBrowse);
            Controls.Add(textBoxDirectory);
            Controls.Add(labelDirectory);
            Controls.Add(textBoxName);
            Controls.Add(labelName);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TerrainNewForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Neue Terrain-Bibliothek (LandTiles + Texturen)";
            ((System.ComponentModel.ISupportInitialize)numericUpDownCapacity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelDirectory;
        private System.Windows.Forms.TextBox textBoxDirectory;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelCapacity;
        private System.Windows.Forms.NumericUpDown numericUpDownCapacity;
        private System.Windows.Forms.Label labelSizeInfo;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}

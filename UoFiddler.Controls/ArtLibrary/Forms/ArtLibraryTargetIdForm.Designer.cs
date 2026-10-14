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
    partial class ArtLibraryTargetIdForm
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
            tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            labelInfo = new System.Windows.Forms.Label();
            labelSource = new System.Windows.Forms.Label();
            textBoxSource = new System.Windows.Forms.TextBox();
            labelTarget = new System.Windows.Forms.Label();
            textBoxTarget = new System.Windows.Forms.TextBox();
            radioButtonSequential = new System.Windows.Forms.RadioButton();
            radioButtonKeepIds = new System.Windows.Forms.RadioButton();
            checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            groupBoxTransparency = new System.Windows.Forms.GroupBox();
            checkBoxWhite = new System.Windows.Forms.CheckBox();
            checkBoxBlack = new System.Windows.Forms.CheckBox();
            flowLayoutPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
            buttonCancel = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            tableLayoutPanelMain.SuspendLayout();
            groupBoxTransparency.SuspendLayout();
            flowLayoutPanelButtons.SuspendLayout();
            SuspendLayout();
            //
            // tableLayoutPanelMain
            //
            tableLayoutPanelMain.AutoSize = true;
            tableLayoutPanelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            tableLayoutPanelMain.ColumnCount = 2;
            tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 340F));
            tableLayoutPanelMain.Controls.Add(labelInfo, 0, 0);
            tableLayoutPanelMain.Controls.Add(labelSource, 0, 1);
            tableLayoutPanelMain.Controls.Add(textBoxSource, 1, 1);
            tableLayoutPanelMain.Controls.Add(labelTarget, 0, 2);
            tableLayoutPanelMain.Controls.Add(textBoxTarget, 1, 2);
            tableLayoutPanelMain.Controls.Add(radioButtonSequential, 0, 3);
            tableLayoutPanelMain.Controls.Add(radioButtonKeepIds, 0, 4);
            tableLayoutPanelMain.Controls.Add(checkBoxOverwrite, 0, 5);
            tableLayoutPanelMain.Controls.Add(groupBoxTransparency, 0, 6);
            tableLayoutPanelMain.Controls.Add(flowLayoutPanelButtons, 0, 7);
            tableLayoutPanelMain.Location = new System.Drawing.Point(9, 9);
            tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            tableLayoutPanelMain.RowCount = 8;
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanelMain.Size = new System.Drawing.Size(430, 260);
            tableLayoutPanelMain.TabIndex = 0;
            //
            // labelInfo
            //
            labelInfo.AutoSize = true;
            tableLayoutPanelMain.SetColumnSpan(labelInfo, 2);
            labelInfo.Location = new System.Drawing.Point(3, 0);
            labelInfo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            labelInfo.MaximumSize = new System.Drawing.Size(420, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new System.Drawing.Size(0, 15);
            labelInfo.TabIndex = 0;
            //
            // labelSource
            //
            labelSource.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelSource.AutoSize = true;
            labelSource.Name = "labelSource";
            labelSource.Size = new System.Drawing.Size(59, 15);
            labelSource.TabIndex = 1;
            labelSource.Text = "Quell-IDs:";
            //
            // textBoxSource
            //
            textBoxSource.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxSource.Name = "textBoxSource";
            textBoxSource.Size = new System.Drawing.Size(334, 23);
            textBoxSource.TabIndex = 2;
            //
            // labelTarget
            //
            labelTarget.Anchor = System.Windows.Forms.AnchorStyles.Left;
            labelTarget.AutoSize = true;
            labelTarget.Name = "labelTarget";
            labelTarget.Size = new System.Drawing.Size(48, 15);
            labelTarget.TabIndex = 3;
            labelTarget.Text = "Ziel-ID:";
            //
            // textBoxTarget
            //
            textBoxTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            textBoxTarget.Name = "textBoxTarget";
            textBoxTarget.Size = new System.Drawing.Size(334, 23);
            textBoxTarget.TabIndex = 4;
            //
            // radioButtonSequential
            //
            radioButtonSequential.AutoSize = true;
            radioButtonSequential.Checked = true;
            tableLayoutPanelMain.SetColumnSpan(radioButtonSequential, 2);
            radioButtonSequential.Name = "radioButtonSequential";
            radioButtonSequential.Size = new System.Drawing.Size(220, 19);
            radioButtonSequential.TabIndex = 5;
            radioButtonSequential.TabStop = true;
            radioButtonSequential.Text = "Fortlaufend ab der Ziel-ID einfügen";
            radioButtonSequential.UseVisualStyleBackColor = true;
            //
            // radioButtonKeepIds
            //
            radioButtonKeepIds.AutoSize = true;
            tableLayoutPanelMain.SetColumnSpan(radioButtonKeepIds, 2);
            radioButtonKeepIds.Name = "radioButtonKeepIds";
            radioButtonKeepIds.Size = new System.Drawing.Size(220, 19);
            radioButtonKeepIds.TabIndex = 6;
            radioButtonKeepIds.Text = "Gleiche IDs verwenden wie die Quelle";
            radioButtonKeepIds.UseVisualStyleBackColor = true;
            radioButtonKeepIds.CheckedChanged += RadioButtonKeepIds_CheckedChanged;
            //
            // checkBoxOverwrite
            //
            checkBoxOverwrite.AutoSize = true;
            tableLayoutPanelMain.SetColumnSpan(checkBoxOverwrite, 2);
            checkBoxOverwrite.Name = "checkBoxOverwrite";
            checkBoxOverwrite.Size = new System.Drawing.Size(330, 19);
            checkBoxOverwrite.TabIndex = 7;
            checkBoxOverwrite.Text = "Belegte IDs überschreiben (sonst werden sie übersprungen)";
            checkBoxOverwrite.UseVisualStyleBackColor = true;
            //
            // groupBoxTransparency
            //
            tableLayoutPanelMain.SetColumnSpan(groupBoxTransparency, 2);
            groupBoxTransparency.Controls.Add(checkBoxWhite);
            groupBoxTransparency.Controls.Add(checkBoxBlack);
            groupBoxTransparency.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxTransparency.Name = "groupBoxTransparency";
            groupBoxTransparency.Size = new System.Drawing.Size(424, 52);
            groupBoxTransparency.TabIndex = 8;
            groupBoxTransparency.TabStop = false;
            groupBoxTransparency.Text = "Als transparent behandeln";
            //
            // checkBoxWhite
            //
            checkBoxWhite.AutoSize = true;
            checkBoxWhite.Checked = true;
            checkBoxWhite.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxWhite.Location = new System.Drawing.Point(140, 22);
            checkBoxWhite.Name = "checkBoxWhite";
            checkBoxWhite.Size = new System.Drawing.Size(180, 19);
            checkBoxWhite.TabIndex = 1;
            checkBoxWhite.Text = "Weiß (und Hellgrau #D3D3D3)";
            checkBoxWhite.UseVisualStyleBackColor = true;
            //
            // checkBoxBlack
            //
            checkBoxBlack.AutoSize = true;
            checkBoxBlack.Checked = true;
            checkBoxBlack.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBlack.Location = new System.Drawing.Point(12, 22);
            checkBoxBlack.Name = "checkBoxBlack";
            checkBoxBlack.Size = new System.Drawing.Size(70, 19);
            checkBoxBlack.TabIndex = 0;
            checkBoxBlack.Text = "Schwarz";
            checkBoxBlack.UseVisualStyleBackColor = true;
            //
            // flowLayoutPanelButtons
            //
            flowLayoutPanelButtons.AutoSize = true;
            tableLayoutPanelMain.SetColumnSpan(flowLayoutPanelButtons, 2);
            flowLayoutPanelButtons.Controls.Add(buttonCancel);
            flowLayoutPanelButtons.Controls.Add(buttonOk);
            flowLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanelButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanelButtons.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            flowLayoutPanelButtons.Name = "flowLayoutPanelButtons";
            flowLayoutPanelButtons.Size = new System.Drawing.Size(424, 33);
            flowLayoutPanelButtons.TabIndex = 9;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(85, 27);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Abbrechen";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // buttonOk
            //
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(85, 27);
            buttonOk.TabIndex = 0;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += ButtonOk_Click;
            //
            // ArtLibraryTargetIdForm
            //
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(450, 280);
            Controls.Add(tableLayoutPanelMain);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ArtLibraryTargetIdForm";
            Padding = new System.Windows.Forms.Padding(9);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Ziel-ID";
            Shown += ArtLibraryTargetIdForm_Shown;
            tableLayoutPanelMain.ResumeLayout(false);
            tableLayoutPanelMain.PerformLayout();
            groupBoxTransparency.ResumeLayout(false);
            groupBoxTransparency.PerformLayout();
            flowLayoutPanelButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelSource;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.Label labelTarget;
        private System.Windows.Forms.TextBox textBoxTarget;
        private System.Windows.Forms.RadioButton radioButtonSequential;
        private System.Windows.Forms.RadioButton radioButtonKeepIds;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.GroupBox groupBoxTransparency;
        private System.Windows.Forms.CheckBox checkBoxWhite;
        private System.Windows.Forms.CheckBox checkBoxBlack;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelButtons;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public class UopFileSelectionForm : Form
    {
        private ComboBox _cboFiles;
        private Button _btnOk;
        private Button _btnCancel;
        private Label _lblInstruction;
        private CheckBox _chkSwapHeader;

        public string SelectedPath { get; private set; }
        public bool SwapHeader { get; private set; }

        public UopFileSelectionForm(List<string> filePaths)
        {
            InitializeComponent();
            PopulateFiles(filePaths);
        }

        private void InitializeComponent()
        {
            this._lblInstruction = new Label();
            this._cboFiles = new ComboBox();
            this._btnOk = new Button();
            this._btnCancel = new Button();
            this._chkSwapHeader = new CheckBox();
            this.SuspendLayout();

            // Label
            this._lblInstruction.AutoSize = true;
            this._lblInstruction.Location = new System.Drawing.Point(12, 15);
            this._lblInstruction.Name = "lblInstruction";
            this._lblInstruction.Size = new System.Drawing.Size(120, 13);
            this._lblInstruction.TabIndex = 0;
            this._lblInstruction.Text = "Select Target UOP File:";

            // ComboBox
            this._cboFiles.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cboFiles.FormattingEnabled = true;
            this._cboFiles.Location = new System.Drawing.Point(15, 35);
            this._cboFiles.Name = "cboFiles";
            this._cboFiles.Size = new System.Drawing.Size(300, 21);
            this._cboFiles.TabIndex = 1;

            // CheckBox Swap Header
            this._chkSwapHeader.AutoSize = true;
            this._chkSwapHeader.Location = new System.Drawing.Point(15, 65);
            this._chkSwapHeader.Name = "chkSwapHeader";
            this._chkSwapHeader.Size = new System.Drawing.Size(250, 17);
            this._chkSwapHeader.TabIndex = 4;
            this._chkSwapHeader.Text = "Swap Header (Fix Invisible/Cropped)";
            this._chkSwapHeader.UseVisualStyleBackColor = true;

            // OK Button
            this._btnOk.Location = new System.Drawing.Point(159, 90);
            this._btnOk.Name = "btnOk";
            this._btnOk.Size = new System.Drawing.Size(75, 23);
            this._btnOk.TabIndex = 2;
            this._btnOk.Text = "OK";
            this._btnOk.UseVisualStyleBackColor = true;
            this._btnOk.Click += new EventHandler(this.BtnOk_Click);

            // Cancel Button
            this._btnCancel.DialogResult = DialogResult.Cancel;
            this._btnCancel.Location = new System.Drawing.Point(240, 90);
            this._btnCancel.Name = "btnCancel";
            this._btnCancel.Size = new System.Drawing.Size(75, 23);
            this._btnCancel.TabIndex = 3;
            this._btnCancel.Text = "Cancel";
            this._btnCancel.UseVisualStyleBackColor = true;

            // Form
            this.AcceptButton = this._btnOk;
            this.CancelButton = this._btnCancel;
            this.ClientSize = new System.Drawing.Size(334, 130);
            this.Controls.Add(this._chkSwapHeader);
            this.Controls.Add(this._btnCancel);
            this.Controls.Add(this._btnOk);
            this.Controls.Add(this._cboFiles);
            this.Controls.Add(this._lblInstruction);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UopFileSelectionForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Select UOP File";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void PopulateFiles(List<string> files)
        {
            foreach (var file in files)
            {
                _cboFiles.Items.Add(Path.GetFileName(file)); // Display only filename
            }
            if (_cboFiles.Items.Count > 0)
            {
                _cboFiles.SelectedIndex = 0;
            }
            _cboFiles.Tag = files;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (_cboFiles.SelectedIndex >= 0)
            {
                var files = (List<string>)_cboFiles.Tag;
                SelectedPath = files[_cboFiles.SelectedIndex];
                SwapHeader = _chkSwapHeader.Checked;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please select a file.");
            }
        }
    }
}

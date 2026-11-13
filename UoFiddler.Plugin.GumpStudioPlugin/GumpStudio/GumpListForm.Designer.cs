namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpListForm
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
            if (disposing) { picturePreview.Image?.Dispose(); }
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
            splitMain = new System.Windows.Forms.SplitContainer();
            txtRows = new System.Windows.Forms.TextBox();
            lblRows = new System.Windows.Forms.Label();
            txtColumns = new System.Windows.Forms.TextBox();
            lblColumns = new System.Windows.Forms.Label();
            picturePreview = new System.Windows.Forms.PictureBox();
            flowOptions = new System.Windows.Forms.FlowLayoutPanel();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblPerPage = new System.Windows.Forms.Label();
            numPerPage = new System.Windows.Forms.NumericUpDown();
            lblRowHeight = new System.Windows.Forms.Label();
            numRowHeight = new System.Windows.Forms.NumericUpDown();
            lblX = new System.Windows.Forms.Label();
            numX = new System.Windows.Forms.NumericUpDown();
            lblY = new System.Windows.Forms.Label();
            numY = new System.Windows.Forms.NumericUpDown();
            lblFirstPage = new System.Windows.Forms.Label();
            numFirstPage = new System.Windows.Forms.NumericUpDown();
            chkSelectButton = new System.Windows.Forms.CheckBox();
            lblButtonBase = new System.Windows.Forms.Label();
            numButtonBase = new System.Windows.Forms.NumericUpDown();
            lblNavY = new System.Windows.Forms.Label();
            numNavY = new System.Windows.Forms.NumericUpDown();
            lblNavBack = new System.Windows.Forms.Label();
            numNavBack = new System.Windows.Forms.NumericUpDown();
            lblNavNext = new System.Windows.Forms.Label();
            numNavNext = new System.Windows.Forms.NumericUpDown();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            btnPaste = new System.Windows.Forms.Button();
            btnSample = new System.Windows.Forms.Button();
            lblHint = new System.Windows.Forms.Label();
            previewTimer = new System.Windows.Forms.Timer(components);
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPerPage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRowHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFirstPage).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numButtonBase).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNavY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNavBack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numNavNext).BeginInit();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // splitMain
            //
            splitMain.Panel1.Controls.Add(txtRows);
            splitMain.Panel1.Controls.Add(lblRows);
            splitMain.Panel1.Controls.Add(txtColumns);
            splitMain.Panel1.Controls.Add(lblColumns);
            splitMain.Panel2.Controls.Add(picturePreview);
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Size = new System.Drawing.Size(960, 420);
            splitMain.SplitterDistance = 470;
            splitMain.Name = "splitMain";
            splitMain.TabIndex = 0;
            //
            // txtRows
            //
            txtRows.AcceptsReturn = true;
            txtRows.AcceptsTab = true;
            txtRows.Dock = System.Windows.Forms.DockStyle.Fill;
            txtRows.Font = new System.Drawing.Font("Consolas", 9.75F);
            txtRows.Multiline = true;
            txtRows.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtRows.WordWrap = false;
            txtRows.Name = "txtRows";
            txtRows.TabIndex = 1;
            toolTip1.SetToolTip(txtRows, "One row per line, columns separated by ; or Tab (paste from Excel works)\n\nDE: Eine Zeile pro Eintrag, Spalten mit ; oder Tab getrennt (Einfügen aus Excel geht)");
            txtRows.TextChanged += Option_Changed;
            //
            // lblRows
            //
            lblRows.Dock = System.Windows.Forms.DockStyle.Top;
            lblRows.Padding = new System.Windows.Forms.Padding(3, 6, 0, 0);
            lblRows.Size = new System.Drawing.Size(470, 24);
            lblRows.Text = "Rows – one per line, columns with ; or Tab / Zeilen – eine pro Zeile:";
            lblRows.Name = "lblRows";
            lblRows.TabIndex = 2;
            //
            // txtColumns
            //
            txtColumns.Dock = System.Windows.Forms.DockStyle.Top;
            txtColumns.Font = new System.Drawing.Font("Consolas", 9.75F);
            txtColumns.Multiline = true;
            txtColumns.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtColumns.Size = new System.Drawing.Size(470, 70);
            txtColumns.Name = "txtColumns";
            txtColumns.TabIndex = 3;
            toolTip1.SetToolTip(txtColumns, "One column per line: Header|width|hue, e.g. Name|140|1152\n\nDE: Eine Spalte pro Zeile: Überschrift|Breite|Farbe");
            txtColumns.TextChanged += Option_Changed;
            //
            // lblColumns
            //
            lblColumns.Dock = System.Windows.Forms.DockStyle.Top;
            lblColumns.Padding = new System.Windows.Forms.Padding(3, 3, 0, 0);
            lblColumns.Size = new System.Drawing.Size(470, 20);
            lblColumns.Text = "Columns – Header|width|hue / Spalten – Überschrift|Breite|Farbe:";
            lblColumns.Name = "lblColumns";
            lblColumns.TabIndex = 4;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabStop = false;
            picturePreview.Name = "picturePreview";
            picturePreview.TabIndex = 5;
            //
            // flowOptions
            //
            flowOptions.Controls.Add(lblName);
            flowOptions.Controls.Add(txtName);
            flowOptions.Controls.Add(lblPerPage);
            flowOptions.Controls.Add(numPerPage);
            flowOptions.Controls.Add(lblRowHeight);
            flowOptions.Controls.Add(numRowHeight);
            flowOptions.Controls.Add(lblX);
            flowOptions.Controls.Add(numX);
            flowOptions.Controls.Add(lblY);
            flowOptions.Controls.Add(numY);
            flowOptions.Controls.Add(lblFirstPage);
            flowOptions.Controls.Add(numFirstPage);
            flowOptions.Controls.Add(chkSelectButton);
            flowOptions.Controls.Add(lblButtonBase);
            flowOptions.Controls.Add(numButtonBase);
            flowOptions.Controls.Add(lblNavY);
            flowOptions.Controls.Add(numNavY);
            flowOptions.Controls.Add(lblNavBack);
            flowOptions.Controls.Add(numNavBack);
            flowOptions.Controls.Add(lblNavNext);
            flowOptions.Controls.Add(numNavNext);
            flowOptions.AutoSize = true;
            flowOptions.Dock = System.Windows.Forms.DockStyle.Top;
            flowOptions.Padding = new System.Windows.Forms.Padding(4);
            flowOptions.Name = "flowOptions";
            flowOptions.TabIndex = 6;
            //
            // lblName
            //
            lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblName.AutoSize = true;
            lblName.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblName.Text = "Name:";
            lblName.Name = "lblName";
            lblName.TabIndex = 7;
            //
            // txtName
            //
            txtName.Size = new System.Drawing.Size(120, 23);
            txtName.Text = "Players";
            txtName.Name = "txtName";
            txtName.TabIndex = 8;
            txtName.TextChanged += Option_Changed;
            //
            // lblPerPage
            //
            lblPerPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPerPage.AutoSize = true;
            lblPerPage.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblPerPage.Text = "Rows per page:";
            lblPerPage.Name = "lblPerPage";
            lblPerPage.TabIndex = 9;
            //
            // numPerPage
            //
            numPerPage.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numPerPage.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numPerPage.Size = new System.Drawing.Size(55, 23);
            numPerPage.Value = new decimal(new int[] { 8, 0, 0, 0 });
            numPerPage.Name = "numPerPage";
            numPerPage.TabIndex = 10;
            toolTip1.SetToolTip(numPerPage, "0 = all rows on one page\n\nDE: 0 = alle Zeilen auf einer Seite");
            numPerPage.ValueChanged += Option_Changed;
            //
            // lblRowHeight
            //
            lblRowHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblRowHeight.AutoSize = true;
            lblRowHeight.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblRowHeight.Text = "Row height:";
            lblRowHeight.Name = "lblRowHeight";
            lblRowHeight.TabIndex = 11;
            //
            // numRowHeight
            //
            numRowHeight.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numRowHeight.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numRowHeight.Size = new System.Drawing.Size(55, 23);
            numRowHeight.Value = new decimal(new int[] { 25, 0, 0, 0 });
            numRowHeight.Name = "numRowHeight";
            numRowHeight.TabIndex = 12;
            numRowHeight.ValueChanged += Option_Changed;
            //
            // lblX
            //
            lblX.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblX.AutoSize = true;
            lblX.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblX.Text = "X:";
            lblX.Name = "lblX";
            lblX.TabIndex = 13;
            //
            // numX
            //
            numX.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numX.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numX.Size = new System.Drawing.Size(60, 23);
            numX.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numX.Name = "numX";
            numX.TabIndex = 14;
            numX.ValueChanged += Option_Changed;
            //
            // lblY
            //
            lblY.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblY.AutoSize = true;
            lblY.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblY.Text = "Y:";
            lblY.Name = "lblY";
            lblY.TabIndex = 15;
            //
            // numY
            //
            numY.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numY.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numY.Size = new System.Drawing.Size(60, 23);
            numY.Value = new decimal(new int[] { 60, 0, 0, 0 });
            numY.Name = "numY";
            numY.TabIndex = 16;
            numY.ValueChanged += Option_Changed;
            //
            // lblFirstPage
            //
            lblFirstPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblFirstPage.AutoSize = true;
            lblFirstPage.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblFirstPage.Text = "First page:";
            lblFirstPage.Name = "lblFirstPage";
            lblFirstPage.TabIndex = 17;
            //
            // numFirstPage
            //
            numFirstPage.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numFirstPage.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numFirstPage.Size = new System.Drawing.Size(50, 23);
            numFirstPage.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numFirstPage.Name = "numFirstPage";
            numFirstPage.TabIndex = 18;
            toolTip1.SetToolTip(numFirstPage, "Page of the first rows (0 = every page, only without paging)\n\nDE: Seite der ersten Zeilen (0 = jede Seite, nur ohne Blättern)");
            numFirstPage.ValueChanged += Option_Changed;
            //
            // chkSelectButton
            //
            chkSelectButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkSelectButton.AutoSize = true;
            chkSelectButton.Text = "Button per row";
            chkSelectButton.Checked = true;
            chkSelectButton.CheckState = System.Windows.Forms.CheckState.Checked;
            chkSelectButton.Name = "chkSelectButton";
            chkSelectButton.TabIndex = 19;
            toolTip1.SetToolTip(chkSelectButton, "A reply button in front of every row (ID = base + row)\n\nDE: Ein Antwort-Button vor jeder Zeile (ID = Basis + Zeile)");
            chkSelectButton.CheckedChanged += Option_Changed;
            //
            // lblButtonBase
            //
            lblButtonBase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblButtonBase.AutoSize = true;
            lblButtonBase.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblButtonBase.Text = "Button IDs from:";
            lblButtonBase.Name = "lblButtonBase";
            lblButtonBase.TabIndex = 20;
            //
            // numButtonBase
            //
            numButtonBase.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numButtonBase.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numButtonBase.Size = new System.Drawing.Size(70, 23);
            numButtonBase.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numButtonBase.Name = "numButtonBase";
            numButtonBase.TabIndex = 21;
            numButtonBase.ValueChanged += Option_Changed;
            //
            // lblNavY
            //
            lblNavY.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblNavY.AutoSize = true;
            lblNavY.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblNavY.Text = "Page buttons Y:";
            lblNavY.Name = "lblNavY";
            lblNavY.TabIndex = 22;
            //
            // numNavY
            //
            numNavY.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numNavY.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numNavY.Size = new System.Drawing.Size(60, 23);
            numNavY.Value = new decimal(new int[] { 300, 0, 0, 0 });
            numNavY.Name = "numNavY";
            numNavY.TabIndex = 23;
            numNavY.ValueChanged += Option_Changed;
            //
            // lblNavBack
            //
            lblNavBack.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblNavBack.AutoSize = true;
            lblNavBack.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblNavBack.Text = "back X:";
            lblNavBack.Name = "lblNavBack";
            lblNavBack.TabIndex = 24;
            //
            // numNavBack
            //
            numNavBack.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numNavBack.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numNavBack.Size = new System.Drawing.Size(60, 23);
            numNavBack.Value = new decimal(new int[] { 20, 0, 0, 0 });
            numNavBack.Name = "numNavBack";
            numNavBack.TabIndex = 25;
            numNavBack.ValueChanged += Option_Changed;
            //
            // lblNavNext
            //
            lblNavNext.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblNavNext.AutoSize = true;
            lblNavNext.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblNavNext.Text = "next X:";
            lblNavNext.Name = "lblNavNext";
            lblNavNext.TabIndex = 26;
            //
            // numNavNext
            //
            numNavNext.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numNavNext.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numNavNext.Size = new System.Drawing.Size(60, 23);
            numNavNext.Value = new decimal(new int[] { 280, 0, 0, 0 });
            numNavNext.Name = "numNavNext";
            numNavNext.TabIndex = 27;
            numNavNext.ValueChanged += Option_Changed;
            //
            // flowButtons
            //
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Controls.Add(btnOk);
            flowButtons.Controls.Add(btnPaste);
            flowButtons.Controls.Add(btnSample);
            flowButtons.Controls.Add(lblHint);
            flowButtons.AutoSize = true;
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.Padding = new System.Windows.Forms.Padding(4);
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Name = "flowButtons";
            flowButtons.TabIndex = 28;
            //
            // btnCancel
            //
            btnCancel.AutoSize = true;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.TabIndex = 29;
            //
            // btnOk
            //
            btnOk.AutoSize = true;
            btnOk.Text = "Create list";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Name = "btnOk";
            btnOk.TabIndex = 30;
            btnOk.Click += BtnOk_Click;
            //
            // btnPaste
            //
            btnPaste.AutoSize = true;
            btnPaste.Text = "Paste rows";
            btnPaste.UseVisualStyleBackColor = true;
            btnPaste.Name = "btnPaste";
            btnPaste.TabIndex = 31;
            toolTip1.SetToolTip(btnPaste, "Rows from the clipboard (e.g. copied from Excel)\n\nDE: Zeilen aus der Zwischenablage (z. B. aus Excel kopiert)");
            btnPaste.Click += BtnPaste_Click;
            //
            // btnSample
            //
            btnSample.AutoSize = true;
            btnSample.Text = "Sample rows";
            btnSample.UseVisualStyleBackColor = true;
            btnSample.Name = "btnSample";
            btnSample.TabIndex = 32;
            btnSample.Click += BtnSample_Click;
            //
            // lblHint
            //
            lblHint.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblHint.AutoSize = true;
            lblHint.Margin = new System.Windows.Forms.Padding(3, 0, 30, 0);
            lblHint.Text = "";
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 33;
            //
            // previewTimer
            //
            previewTimer.Interval = 300;
            previewTimer.Tick += PreviewTimer_Tick;
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            //
            // GumpListForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(960, 560);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 450);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "List / table generator / Listen- und Tabellen-Generator";
            Controls.Add(splitMain);
            Controls.Add(flowOptions);
            Controls.Add(flowButtons);
            Name = "GumpListForm";
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numNavNext).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNavBack).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNavY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numButtonBase).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFirstPage).EndInit();
            ((System.ComponentModel.ISupportInitialize)numY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numX).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRowHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPerPage).EndInit();
            flowOptions.ResumeLayout(false);
            flowOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TextBox txtRows;
        private System.Windows.Forms.Label lblRows;
        private System.Windows.Forms.TextBox txtColumns;
        private System.Windows.Forms.Label lblColumns;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.FlowLayoutPanel flowOptions;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblPerPage;
        private System.Windows.Forms.NumericUpDown numPerPage;
        private System.Windows.Forms.Label lblRowHeight;
        private System.Windows.Forms.NumericUpDown numRowHeight;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.Label lblFirstPage;
        private System.Windows.Forms.NumericUpDown numFirstPage;
        private System.Windows.Forms.CheckBox chkSelectButton;
        private System.Windows.Forms.Label lblButtonBase;
        private System.Windows.Forms.NumericUpDown numButtonBase;
        private System.Windows.Forms.Label lblNavY;
        private System.Windows.Forms.NumericUpDown numNavY;
        private System.Windows.Forms.Label lblNavBack;
        private System.Windows.Forms.NumericUpDown numNavBack;
        private System.Windows.Forms.Label lblNavNext;
        private System.Windows.Forms.NumericUpDown numNavNext;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.Button btnSample;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Timer previewTimer;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

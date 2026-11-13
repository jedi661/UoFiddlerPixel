namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpThemeForm
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
            listThemes = new System.Windows.Forms.ListBox();
            flowThemeButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnFromGump = new System.Windows.Forms.Button();
            btnSaveTheme = new System.Windows.Forms.Button();
            btnDeleteTheme = new System.Windows.Forms.Button();
            propertyTheme = new System.Windows.Forms.PropertyGrid();
            picturePreview = new System.Windows.Forms.PictureBox();
            flowApply = new System.Windows.Forms.FlowLayoutPanel();
            lblApply = new System.Windows.Forms.Label();
            chkFrames = new System.Windows.Forms.CheckBox();
            chkButtons = new System.Windows.Forms.CheckBox();
            chkSwitches = new System.Windows.Forms.CheckBox();
            chkTexts = new System.Windows.Forms.CheckBox();
            lblPreview = new System.Windows.Forms.Label();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnClose = new System.Windows.Forms.Button();
            btnApply = new System.Windows.Forms.Button();
            lblStatus = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            flowThemeButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowApply.SuspendLayout();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // splitMain
            //
            splitMain.Panel1.Controls.Add(listThemes);
            splitMain.Panel1.Controls.Add(flowThemeButtons);
            splitMain.Panel1.Controls.Add(propertyTheme);
            splitMain.Panel2.Controls.Add(picturePreview);
            splitMain.Panel2.Controls.Add(flowApply);
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.Size = new System.Drawing.Size(980, 520);
            splitMain.SplitterDistance = 330;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Name = "splitMain";
            splitMain.TabIndex = 0;
            //
            // listThemes
            //
            listThemes.Dock = System.Windows.Forms.DockStyle.Fill;
            listThemes.IntegralHeight = false;
            listThemes.Name = "listThemes";
            listThemes.TabIndex = 1;
            listThemes.SelectedIndexChanged += ListThemes_SelectedIndexChanged;
            //
            // flowThemeButtons
            //
            flowThemeButtons.Controls.Add(btnFromGump);
            flowThemeButtons.Controls.Add(btnSaveTheme);
            flowThemeButtons.Controls.Add(btnDeleteTheme);
            flowThemeButtons.AutoSize = true;
            flowThemeButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowThemeButtons.Padding = new System.Windows.Forms.Padding(4);
            flowThemeButtons.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            flowThemeButtons.Name = "flowThemeButtons";
            flowThemeButtons.TabIndex = 2;
            //
            // btnFromGump
            //
            btnFromGump.AutoSize = true;
            btnFromGump.Text = "From gump";
            btnFromGump.UseVisualStyleBackColor = true;
            btnFromGump.Name = "btnFromGump";
            btnFromGump.TabIndex = 3;
            toolTip1.SetToolTip(btnFromGump, "New theme with the frames, buttons and colors of your gump\n\nDE: Neues Thema mit Rahmen, Buttons und Farben deines Gumps");
            btnFromGump.Click += BtnFromGump_Click;
            //
            // btnSaveTheme
            //
            btnSaveTheme.AutoSize = true;
            btnSaveTheme.Text = "Save";
            btnSaveTheme.UseVisualStyleBackColor = true;
            btnSaveTheme.Name = "btnSaveTheme";
            btnSaveTheme.TabIndex = 4;
            toolTip1.SetToolTip(btnSaveTheme, "Saves the theme (built-in ones as a copy) to GumpThemes\n\nDE: Speichert das Thema (mitgelieferte als Kopie) nach GumpThemes");
            btnSaveTheme.Click += BtnSaveTheme_Click;
            //
            // btnDeleteTheme
            //
            btnDeleteTheme.AutoSize = true;
            btnDeleteTheme.Text = "Delete";
            btnDeleteTheme.UseVisualStyleBackColor = true;
            btnDeleteTheme.Name = "btnDeleteTheme";
            btnDeleteTheme.TabIndex = 5;
            btnDeleteTheme.Click += BtnDeleteTheme_Click;
            //
            // propertyTheme
            //
            propertyTheme.Dock = System.Windows.Forms.DockStyle.Bottom;
            propertyTheme.Size = new System.Drawing.Size(330, 300);
            propertyTheme.ToolbarVisible = false;
            propertyTheme.Name = "propertyTheme";
            propertyTheme.TabIndex = 6;
            propertyTheme.PropertyValueChanged += PropertyTheme_PropertyValueChanged;
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabStop = false;
            picturePreview.Name = "picturePreview";
            picturePreview.TabIndex = 7;
            //
            // flowApply
            //
            flowApply.Controls.Add(lblApply);
            flowApply.Controls.Add(chkFrames);
            flowApply.Controls.Add(chkButtons);
            flowApply.Controls.Add(chkSwitches);
            flowApply.Controls.Add(chkTexts);
            flowApply.Controls.Add(lblPreview);
            flowApply.AutoSize = true;
            flowApply.Dock = System.Windows.Forms.DockStyle.Top;
            flowApply.Padding = new System.Windows.Forms.Padding(4);
            flowApply.Name = "flowApply";
            flowApply.TabIndex = 8;
            //
            // lblApply
            //
            lblApply.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblApply.AutoSize = true;
            lblApply.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblApply.Text = "Change:";
            lblApply.Name = "lblApply";
            lblApply.TabIndex = 9;
            //
            // chkFrames
            //
            chkFrames.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkFrames.AutoSize = true;
            chkFrames.Text = "Frames";
            chkFrames.Checked = true;
            chkFrames.CheckState = System.Windows.Forms.CheckState.Checked;
            chkFrames.Name = "chkFrames";
            chkFrames.TabIndex = 10;
            chkFrames.CheckedChanged += Option_Changed;
            //
            // chkButtons
            //
            chkButtons.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkButtons.AutoSize = true;
            chkButtons.Text = "Buttons";
            chkButtons.Checked = true;
            chkButtons.CheckState = System.Windows.Forms.CheckState.Checked;
            chkButtons.Name = "chkButtons";
            chkButtons.TabIndex = 11;
            chkButtons.CheckedChanged += Option_Changed;
            //
            // chkSwitches
            //
            chkSwitches.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkSwitches.AutoSize = true;
            chkSwitches.Text = "Check boxes / radios";
            chkSwitches.Checked = true;
            chkSwitches.CheckState = System.Windows.Forms.CheckState.Checked;
            chkSwitches.Name = "chkSwitches";
            chkSwitches.TabIndex = 12;
            chkSwitches.CheckedChanged += Option_Changed;
            //
            // chkTexts
            //
            chkTexts.Anchor = System.Windows.Forms.AnchorStyles.Left;
            chkTexts.AutoSize = true;
            chkTexts.Text = "Text colors";
            chkTexts.Checked = true;
            chkTexts.CheckState = System.Windows.Forms.CheckState.Checked;
            chkTexts.Name = "chkTexts";
            chkTexts.TabIndex = 13;
            chkTexts.CheckedChanged += Option_Changed;
            //
            // lblPreview
            //
            lblPreview.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblPreview.AutoSize = true;
            lblPreview.Margin = new System.Windows.Forms.Padding(6, 0, 2, 0);
            lblPreview.Text = "Preview: your gump with the theme";
            lblPreview.ForeColor = System.Drawing.SystemColors.GrayText;
            lblPreview.Name = "lblPreview";
            lblPreview.TabIndex = 14;
            //
            // flowButtons
            //
            flowButtons.Controls.Add(btnClose);
            flowButtons.Controls.Add(btnApply);
            flowButtons.Controls.Add(lblStatus);
            flowButtons.AutoSize = true;
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.Padding = new System.Windows.Forms.Padding(4);
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Name = "flowButtons";
            flowButtons.TabIndex = 15;
            //
            // btnClose
            //
            btnClose.AutoSize = true;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnClose.Name = "btnClose";
            btnClose.TabIndex = 16;
            //
            // btnApply
            //
            btnApply.AutoSize = true;
            btnApply.Text = "Apply to the gump";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Name = "btnApply";
            btnApply.TabIndex = 17;
            btnApply.Click += BtnApply_Click;
            //
            // lblStatus
            //
            lblStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Margin = new System.Windows.Forms.Padding(3, 0, 30, 0);
            lblStatus.Text = "";
            lblStatus.Name = "lblStatus";
            lblStatus.TabIndex = 18;
            //
            // toolTip1
            //
            toolTip1.AutoPopDelay = 15000;
            //
            // GumpThemeForm
            //
            AcceptButton = btnApply;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new System.Drawing.Size(980, 600);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(760, 480);
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Themes / Themen";
            Controls.Add(splitMain);
            Controls.Add(flowButtons);
            Name = "GumpThemeForm";
            flowButtons.ResumeLayout(false);
            flowButtons.PerformLayout();
            flowApply.ResumeLayout(false);
            flowApply.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            flowThemeButtons.ResumeLayout(false);
            flowThemeButtons.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ListBox listThemes;
        private System.Windows.Forms.FlowLayoutPanel flowThemeButtons;
        private System.Windows.Forms.Button btnFromGump;
        private System.Windows.Forms.Button btnSaveTheme;
        private System.Windows.Forms.Button btnDeleteTheme;
        private System.Windows.Forms.PropertyGrid propertyTheme;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.FlowLayoutPanel flowApply;
        private System.Windows.Forms.Label lblApply;
        private System.Windows.Forms.CheckBox chkFrames;
        private System.Windows.Forms.CheckBox chkButtons;
        private System.Windows.Forms.CheckBox chkSwitches;
        private System.Windows.Forms.CheckBox chkTexts;
        private System.Windows.Forms.Label lblPreview;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

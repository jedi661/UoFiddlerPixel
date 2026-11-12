namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    partial class GumpTemplateSaveForm
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
            if (disposing)
            {
                picturePreview.Image?.Dispose();
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
            tableMain = new System.Windows.Forms.TableLayoutPanel();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblCategory = new System.Windows.Forms.Label();
            comboCategory = new System.Windows.Forms.ComboBox();
            lblContent = new System.Windows.Forms.Label();
            flowContent = new System.Windows.Forms.FlowLayoutPanel();
            radioWhole = new System.Windows.Forms.RadioButton();
            radioSelection = new System.Windows.Forms.RadioButton();
            lblDescription = new System.Windows.Forms.Label();
            txtDescription = new System.Windows.Forms.TextBox();
            lblHint = new System.Windows.Forms.Label();
            picturePreview = new System.Windows.Forms.PictureBox();
            flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnCancel = new System.Windows.Forms.Button();
            btnOk = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            tableMain.SuspendLayout();
            flowContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
            flowButtons.SuspendLayout();
            SuspendLayout();
            //
            // tableMain
            //
            tableMain.ColumnCount = 3;
            tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 95F));
            tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            tableMain.Controls.Add(lblName, 0, 0);
            tableMain.Controls.Add(txtName, 1, 0);
            tableMain.Controls.Add(lblCategory, 0, 1);
            tableMain.Controls.Add(comboCategory, 1, 1);
            tableMain.Controls.Add(lblContent, 0, 2);
            tableMain.Controls.Add(flowContent, 1, 2);
            tableMain.Controls.Add(lblDescription, 0, 3);
            tableMain.Controls.Add(txtDescription, 1, 3);
            tableMain.Controls.Add(lblHint, 0, 4);
            tableMain.Controls.Add(picturePreview, 2, 0);
            tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tableMain.Location = new System.Drawing.Point(0, 0);
            tableMain.Name = "tableMain";
            tableMain.Padding = new System.Windows.Forms.Padding(8);
            tableMain.RowCount = 5;
            tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableMain.SetColumnSpan(lblHint, 2);
            tableMain.SetRowSpan(picturePreview, 5);
            tableMain.Size = new System.Drawing.Size(624, 321);
            tableMain.TabIndex = 0;
            //
            // lblName
            //
            lblName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblName.AutoSize = true;
            lblName.Name = "lblName";
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            //
            // txtName
            //
            txtName.Dock = System.Windows.Forms.DockStyle.Fill;
            txtName.Name = "txtName";
            txtName.TabIndex = 1;
            toolTip1.SetToolTip(txtName, "Name of the template (becomes the file name)\n\nDE: Name der Vorlage (wird der Dateiname)");
            txtName.TextChanged += TxtName_TextChanged;
            //
            // lblCategory
            //
            lblCategory.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblCategory.AutoSize = true;
            lblCategory.Name = "lblCategory";
            lblCategory.TabIndex = 2;
            lblCategory.Text = "Category:";
            //
            // comboCategory
            //
            comboCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboCategory.Name = "comboCategory";
            comboCategory.TabIndex = 3;
            toolTip1.SetToolTip(comboCategory, "Pick a category or type a new one; \"/\" makes sub categories (e.g. Shops/Vendors)\n\nDE: Kategorie wählen oder neue eintippen; \"/\" macht Unterkategorien (z. B. Shops/Händler)");
            comboCategory.TextChanged += TxtName_TextChanged;
            //
            // lblContent
            //
            lblContent.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lblContent.AutoSize = true;
            lblContent.Name = "lblContent";
            lblContent.TabIndex = 4;
            lblContent.Text = "Save:";
            //
            // flowContent
            //
            flowContent.AutoSize = true;
            flowContent.Controls.Add(radioWhole);
            flowContent.Controls.Add(radioSelection);
            flowContent.Dock = System.Windows.Forms.DockStyle.Fill;
            flowContent.Name = "flowContent";
            flowContent.TabIndex = 5;
            //
            // radioWhole
            //
            radioWhole.AutoSize = true;
            radioWhole.Checked = true;
            radioWhole.Name = "radioWhole";
            radioWhole.TabIndex = 0;
            radioWhole.TabStop = true;
            radioWhole.Text = "Whole gump";
            toolTip1.SetToolTip(radioWhole, "All elements and the gump settings (class name, command …)\n\nDE: Alle Elemente und die Gump-Einstellungen (Klassenname, Befehl …)");
            radioWhole.CheckedChanged += RadioContent_CheckedChanged;
            //
            // radioSelection
            //
            radioSelection.AutoSize = true;
            radioSelection.Name = "radioSelection";
            radioSelection.TabIndex = 1;
            radioSelection.Text = "Only the selected elements (building block)";
            toolTip1.SetToolTip(radioSelection, "A reusable part, e.g. a title bar or an input field with frame – insert it later into any gump\n\nDE: Ein wiederverwendbarer Baustein, z. B. Titelleiste oder Eingabefeld mit Rahmen – später in jedes Gump einfügen");
            radioSelection.CheckedChanged += RadioContent_CheckedChanged;
            //
            // lblDescription
            //
            lblDescription.AutoSize = true;
            lblDescription.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            lblDescription.Name = "lblDescription";
            lblDescription.TabIndex = 6;
            lblDescription.Text = "Note:";
            //
            // txtDescription
            //
            txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtDescription.TabIndex = 7;
            toolTip1.SetToolTip(txtDescription, "What the template is for (shown in the library)\n\nDE: Wofür die Vorlage ist (wird in der Bibliothek gezeigt)");
            //
            // lblHint
            //
            lblHint.AutoSize = true;
            lblHint.ForeColor = System.Drawing.SystemColors.GrayText;
            lblHint.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
            lblHint.Name = "lblHint";
            lblHint.TabIndex = 8;
            lblHint.Text = "Saved to GumpTemplates\\<Category>\\<Name>.gump.json next to UoFiddler.";
            //
            // picturePreview
            //
            picturePreview.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
            picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picturePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            picturePreview.Name = "picturePreview";
            picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            picturePreview.TabIndex = 9;
            picturePreview.TabStop = false;
            //
            // flowButtons
            //
            flowButtons.AutoSize = true;
            flowButtons.Controls.Add(btnCancel);
            flowButtons.Controls.Add(btnOk);
            flowButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowButtons.Location = new System.Drawing.Point(0, 321);
            flowButtons.Name = "flowButtons";
            flowButtons.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            flowButtons.Size = new System.Drawing.Size(624, 43);
            flowButtons.TabIndex = 1;
            //
            // btnCancel
            //
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 27);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            //
            // btnOk
            //
            btnOk.Name = "btnOk";
            btnOk.Size = new System.Drawing.Size(90, 27);
            btnOk.TabIndex = 0;
            btnOk.Text = "Save";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            //
            // GumpTemplateSaveForm
            //
            AcceptButton = btnOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(624, 364);
            Controls.Add(tableMain);
            Controls.Add(flowButtons);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 320);
            Name = "GumpTemplateSaveForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Save as template / Als Vorlage speichern";
            tableMain.ResumeLayout(false);
            tableMain.PerformLayout();
            flowContent.ResumeLayout(false);
            flowContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
            flowButtons.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableMain;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox comboCategory;
        private System.Windows.Forms.Label lblContent;
        private System.Windows.Forms.FlowLayoutPanel flowContent;
        private System.Windows.Forms.RadioButton radioWhole;
        private System.Windows.Forms.RadioButton radioSelection;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}

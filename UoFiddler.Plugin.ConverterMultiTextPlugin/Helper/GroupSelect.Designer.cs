namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class GroupSelect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupSelect));
            label1 = new System.Windows.Forms.Label();
            SelectGroup = new System.Windows.Forms.ComboBox();
            PropertyGrid1 = new System.Windows.Forms.PropertyGrid();
            Button1 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(35, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(84, 15);
            label1.TabIndex = 0;
            label1.Text = "Select Group X";
            // 
            // SelectGroup
            // 
            SelectGroup.FormattingEnabled = true;
            SelectGroup.Location = new System.Drawing.Point(8, 24);
            SelectGroup.Name = "SelectGroup";
            SelectGroup.Size = new System.Drawing.Size(205, 23);
            SelectGroup.TabIndex = 1;
            SelectGroup.SelectedIndexChanged += SelectGroup_SelectedIndexChanged;
            // 
            // PropertyGrid1
            // 
            PropertyGrid1.CommandsVisibleIfAvailable = false;
            PropertyGrid1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            PropertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
            PropertyGrid1.Location = new System.Drawing.Point(8, 53);
            PropertyGrid1.Name = "PropertyGrid1";
            PropertyGrid1.Size = new System.Drawing.Size(205, 324);
            PropertyGrid1.TabIndex = 0;
            // 
            // Button1
            // 
            Button1.Location = new System.Drawing.Point(8, 383);
            Button1.Name = "Button1";
            Button1.Size = new System.Drawing.Size(75, 23);
            Button1.TabIndex = 2;
            Button1.Text = "Select";
            Button1.UseVisualStyleBackColor = true;
            // 
            // GroupSelect
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(215, 418);
            Controls.Add(Button1);
            Controls.Add(PropertyGrid1);
            Controls.Add(SelectGroup);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "GroupSelect";
            Text = "GroupSelect";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox SelectGroup;
        private System.Windows.Forms.PropertyGrid PropertyGrid1;
        private System.Windows.Forms.Button Button1;
    }
}
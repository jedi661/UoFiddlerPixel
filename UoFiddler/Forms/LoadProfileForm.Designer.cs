/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Forms
{
    partial class LoadProfileForm
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
            comboBoxLoad = new System.Windows.Forms.ComboBox();
            button1 = new System.Windows.Forms.Button();
            textBoxCreate = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            bt_Delete_List = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            comboBoxBasedOn = new System.Windows.Forms.ComboBox();
            LBStrgAltP = new System.Windows.Forms.Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxLoad
            // 
            comboBoxLoad.FormattingEnabled = true;
            comboBoxLoad.Location = new System.Drawing.Point(24, 36);
            comboBoxLoad.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxLoad.Name = "comboBoxLoad";
            comboBoxLoad.Size = new System.Drawing.Size(140, 23);
            comboBoxLoad.TabIndex = 0;
            comboBoxLoad.SelectedIndexChanged += ComboBoxLoad_SelectedIndexChanged;
            comboBoxLoad.KeyDown += ComboBoxLoad_KeyDown;
            comboBoxLoad.KeyUp += ComboBoxLoad_KeyUp;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(192, 33);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(99, 27);
            button1.TabIndex = 1;
            button1.Text = "Load Profile";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickLoad;
            // 
            // textBoxCreate
            // 
            textBoxCreate.Location = new System.Drawing.Point(10, 22);
            textBoxCreate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxCreate.Name = "textBoxCreate";
            textBoxCreate.Size = new System.Drawing.Size(140, 23);
            textBoxCreate.TabIndex = 2;
            // 
            // button2
            // 
            button2.DialogResult = System.Windows.Forms.DialogResult.OK;
            button2.Location = new System.Drawing.Point(178, 20);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(99, 27);
            button2.TabIndex = 3;
            button2.Text = "Create Profile";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickCreate;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(LBStrgAltP);
            groupBox1.Location = new System.Drawing.Point(14, 14);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(295, 66);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Load";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(bt_Delete_List);
            groupBox2.Controls.Add(label1);
            groupBox2.Controls.Add(comboBoxBasedOn);
            groupBox2.Controls.Add(button2);
            groupBox2.Controls.Add(textBoxCreate);
            groupBox2.Location = new System.Drawing.Point(14, 87);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(295, 96);
            groupBox2.TabIndex = 5;
            groupBox2.TabStop = false;
            groupBox2.Text = "Create";
            // 
            // bt_Delete_List
            // 
            bt_Delete_List.Location = new System.Drawing.Point(224, 53);
            bt_Delete_List.Name = "bt_Delete_List";
            bt_Delete_List.Size = new System.Drawing.Size(53, 23);
            bt_Delete_List.TabIndex = 6;
            bt_Delete_List.Text = "Delete";
            bt_Delete_List.UseVisualStyleBackColor = true;
            bt_Delete_List.Click += bt_Delete_List_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 57);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(57, 15);
            label1.TabIndex = 5;
            label1.Text = "Based On";
            // 
            // comboBoxBasedOn
            // 
            comboBoxBasedOn.FormattingEnabled = true;
            comboBoxBasedOn.Location = new System.Drawing.Point(77, 53);
            comboBoxBasedOn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxBasedOn.Name = "comboBoxBasedOn";
            comboBoxBasedOn.Size = new System.Drawing.Size(140, 23);
            comboBoxBasedOn.TabIndex = 4;
            // 
            // LBStrgAltP
            // 
            LBStrgAltP.AutoSize = true;
            LBStrgAltP.Location = new System.Drawing.Point(10, 48);
            LBStrgAltP.Name = "LBStrgAltP";
            LBStrgAltP.Size = new System.Drawing.Size(78, 15);
            LBStrgAltP.TabIndex = 6;
            LBStrgAltP.Text = "Strg + Alt + P";
            // 
            // LoadProfileForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(324, 201);
            Controls.Add(button1);
            Controls.Add(comboBoxLoad);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoadProfileForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Choose Profile";
            FormClosed += LoadProfile_FormClosed;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboBoxBasedOn;
        private System.Windows.Forms.ComboBox comboBoxLoad;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCreate;
        private System.Windows.Forms.Button bt_Delete_List;
        private System.Windows.Forms.Label LBStrgAltP;
    }
}
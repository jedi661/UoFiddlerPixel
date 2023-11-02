// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin
{
    partial class AltitudeToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AltitudeToolForm));
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            comboBoxCommand = new System.Windows.Forms.ComboBox();
            textBoxOutput = new System.Windows.Forms.TextBox();
            labelDir = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            textBoxManuel = new System.Windows.Forms.TextBox();
            btCopyMul = new System.Windows.Forms.Button();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            lbTxt = new System.Windows.Forms.Label();
            checkBoxCopyText = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // comboBoxCommand
            // 
            comboBoxCommand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxCommand.FormattingEnabled = true;
            comboBoxCommand.Items.AddRange(new object[] { "extract map0.mul 512 map0.bmp", "modify map0.mul 512 map0.bmp" });
            comboBoxCommand.Location = new System.Drawing.Point(25, 14);
            comboBoxCommand.Name = "comboBoxCommand";
            comboBoxCommand.Size = new System.Drawing.Size(294, 23);
            comboBoxCommand.TabIndex = 0;
            // 
            // textBoxOutput
            // 
            textBoxOutput.Location = new System.Drawing.Point(25, 139);
            textBoxOutput.Multiline = true;
            textBoxOutput.Name = "textBoxOutput";
            textBoxOutput.Size = new System.Drawing.Size(505, 265);
            textBoxOutput.TabIndex = 1;
            // 
            // labelDir
            // 
            labelDir.AutoSize = true;
            labelDir.Location = new System.Drawing.Point(25, 108);
            labelDir.Name = "labelDir";
            labelDir.Size = new System.Drawing.Size(22, 15);
            labelDir.TabIndex = 2;
            labelDir.Text = "Dir";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(456, 14);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(74, 23);
            button1.TabIndex = 3;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += BtStartCommand_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(325, 14);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(103, 23);
            button2.TabIndex = 4;
            button2.Text = "Dir Altitude Tool";
            button2.UseVisualStyleBackColor = true;
            button2.Click += BtSelectDirectory_Click;
            // 
            // textBoxManuel
            // 
            textBoxManuel.Location = new System.Drawing.Point(25, 72);
            textBoxManuel.Name = "textBoxManuel";
            textBoxManuel.Size = new System.Drawing.Size(294, 23);
            textBoxManuel.TabIndex = 5;
            // 
            // btCopyMul
            // 
            btCopyMul.Location = new System.Drawing.Point(456, 46);
            btCopyMul.Name = "btCopyMul";
            btCopyMul.Size = new System.Drawing.Size(74, 23);
            btCopyMul.TabIndex = 6;
            btCopyMul.Text = "Copy File";
            btCopyMul.UseVisualStyleBackColor = true;
            btCopyMul.Click += btCopyMul_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // lbTxt
            // 
            lbTxt.AutoSize = true;
            lbTxt.Location = new System.Drawing.Point(25, 54);
            lbTxt.Name = "lbTxt";
            lbTxt.Size = new System.Drawing.Size(160, 15);
            lbTxt.TabIndex = 7;
            lbTxt.Text = "Manual entry of commands :";
            // 
            // checkBoxCopyText
            // 
            checkBoxCopyText.AutoSize = true;
            checkBoxCopyText.Location = new System.Drawing.Point(325, 74);
            checkBoxCopyText.Name = "checkBoxCopyText";
            checkBoxCopyText.Size = new System.Drawing.Size(78, 19);
            checkBoxCopyText.TabIndex = 8;
            checkBoxCopyText.Text = "Copy Text";
            checkBoxCopyText.UseVisualStyleBackColor = true;
            checkBoxCopyText.CheckedChanged += checkBoxCopyText_CheckedChanged;
            // 
            // AltitudeToolForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(594, 450);
            Controls.Add(checkBoxCopyText);
            Controls.Add(lbTxt);
            Controls.Add(btCopyMul);
            Controls.Add(textBoxManuel);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(labelDir);
            Controls.Add(textBoxOutput);
            Controls.Add(comboBoxCommand);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "AltitudeToolForm";
            Text = "Altitude Tool";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.ComboBox comboBoxCommand;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label labelDir;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBoxManuel;
        private System.Windows.Forms.Button btCopyMul;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lbTxt;
        private System.Windows.Forms.CheckBox checkBoxCopyText;
    }
}
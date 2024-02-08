// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class UOMap
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UOMap));
            BtnLoad = new System.Windows.Forms.Button();
            textBoxLoad = new System.Windows.Forms.TextBox();
            BtnExecute = new System.Windows.Forms.Button();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            progressBarMap = new System.Windows.Forms.ProgressBar();
            panel1 = new System.Windows.Forms.Panel();
            saveAsBmpCheckBox = new System.Windows.Forms.CheckBox();
            comboBoxMaps = new System.Windows.Forms.ComboBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // BtnLoad
            // 
            BtnLoad.Location = new System.Drawing.Point(32, 22);
            BtnLoad.Name = "BtnLoad";
            BtnLoad.Size = new System.Drawing.Size(75, 23);
            BtnLoad.TabIndex = 0;
            BtnLoad.Text = "Load";
            BtnLoad.UseVisualStyleBackColor = true;
            BtnLoad.Click += BtnLoad_Click;
            // 
            // textBoxLoad
            // 
            textBoxLoad.Location = new System.Drawing.Point(113, 23);
            textBoxLoad.Multiline = true;
            textBoxLoad.Name = "textBoxLoad";
            textBoxLoad.Size = new System.Drawing.Size(216, 82);
            textBoxLoad.TabIndex = 1;
            // 
            // BtnExecute
            // 
            BtnExecute.Location = new System.Drawing.Point(32, 53);
            BtnExecute.Name = "BtnExecute";
            BtnExecute.Size = new System.Drawing.Size(75, 23);
            BtnExecute.TabIndex = 2;
            BtnExecute.Text = "Execute";
            BtnExecute.UseVisualStyleBackColor = true;
            BtnExecute.Click += BtnExecute_Click;
            // 
            // buttonOpenTempGrafic
            // 
            buttonOpenTempGrafic.Location = new System.Drawing.Point(32, 82);
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Size = new System.Drawing.Size(75, 23);
            buttonOpenTempGrafic.TabIndex = 3;
            buttonOpenTempGrafic.Text = "Temp";
            buttonOpenTempGrafic.UseVisualStyleBackColor = true;
            buttonOpenTempGrafic.Click += buttonOpenTempGrafic_Click;
            // 
            // progressBarMap
            // 
            progressBarMap.Location = new System.Drawing.Point(3, 209);
            progressBarMap.Name = "progressBarMap";
            progressBarMap.Size = new System.Drawing.Size(430, 23);
            progressBarMap.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(saveAsBmpCheckBox);
            panel1.Controls.Add(comboBoxMaps);
            panel1.Controls.Add(progressBarMap);
            panel1.Controls.Add(BtnLoad);
            panel1.Controls.Add(buttonOpenTempGrafic);
            panel1.Controls.Add(textBoxLoad);
            panel1.Controls.Add(BtnExecute);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(436, 235);
            panel1.TabIndex = 5;
            // 
            // saveAsBmpCheckBox
            // 
            saveAsBmpCheckBox.AutoSize = true;
            saveAsBmpCheckBox.Checked = true;
            saveAsBmpCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            saveAsBmpCheckBox.Location = new System.Drawing.Point(336, 56);
            saveAsBmpCheckBox.Name = "saveAsBmpCheckBox";
            saveAsBmpCheckBox.Size = new System.Drawing.Size(95, 19);
            saveAsBmpCheckBox.TabIndex = 6;
            saveAsBmpCheckBox.Text = ".bmp or .png";
            saveAsBmpCheckBox.UseVisualStyleBackColor = true;
            // 
            // comboBoxMaps
            // 
            comboBoxMaps.FormattingEnabled = true;
            comboBoxMaps.Location = new System.Drawing.Point(113, 111);
            comboBoxMaps.Name = "comboBoxMaps";
            comboBoxMaps.Size = new System.Drawing.Size(216, 23);
            comboBoxMaps.TabIndex = 5;
            comboBoxMaps.SelectedIndexChanged += comboBoxMaps_SelectedIndexChanged;
            // 
            // UOMap
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(460, 259);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "UOMap";
            Text = "UOMap";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button BtnLoad;
        private System.Windows.Forms.TextBox textBoxLoad;
        private System.Windows.Forms.Button BtnExecute;
        private System.Windows.Forms.Button buttonOpenTempGrafic;
        private System.Windows.Forms.ProgressBar progressBarMap;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox comboBoxMaps;
        private System.Windows.Forms.CheckBox saveAsBmpCheckBox;
    }
}
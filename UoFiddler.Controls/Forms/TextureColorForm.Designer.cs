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

namespace UoFiddler.Controls.Forms
{
    partial class TextureColorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureColorForm));
            PictureBoxImageColor = new System.Windows.Forms.PictureBox();
            panel1 = new System.Windows.Forms.Panel();
            buttonPrevious = new System.Windows.Forms.Button();
            buttonNext = new System.Windows.Forms.Button();
            trackBarColor = new System.Windows.Forms.TrackBar();
            labelColorShift = new System.Windows.Forms.Label();
            labelColorValues = new System.Windows.Forms.Label();
            buttonSavePosition = new System.Windows.Forms.Button();
            buttonLoadPosition = new System.Windows.Forms.Button();
            buttonCopyToClipboard = new System.Windows.Forms.Button();
            buttonSaveToFile = new System.Windows.Forms.Button();
            buttonChangeCursorColor = new System.Windows.Forms.Button();
            SavePatternButton = new System.Windows.Forms.Button();
            RestorePatternButton = new System.Windows.Forms.Button();
            btnClear = new System.Windows.Forms.Button();
            panelColor = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarColor).BeginInit();
            SuspendLayout();
            // 
            // PictureBoxImageColor
            // 
            PictureBoxImageColor.Location = new System.Drawing.Point(9, 10);
            PictureBoxImageColor.Name = "PictureBoxImageColor";
            PictureBoxImageColor.Size = new System.Drawing.Size(256, 256);
            PictureBoxImageColor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            PictureBoxImageColor.TabIndex = 0;
            PictureBoxImageColor.TabStop = false;
            PictureBoxImageColor.Paint += PictureBoxImageColor_Paint;
            PictureBoxImageColor.MouseDown += PictureBoxImageColor_MouseDown;
            PictureBoxImageColor.MouseEnter += PictureBoxImageColor_MouseEnter;
            PictureBoxImageColor.MouseLeave += PictureBoxImageColor_MouseLeave;
            PictureBoxImageColor.MouseMove += PictureBoxImageColor_MouseMove;
            PictureBoxImageColor.MouseUp += PictureBoxImageColor_MouseUp;
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(PictureBoxImageColor);
            panel1.Location = new System.Drawing.Point(12, 43);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(277, 278);
            panel1.TabIndex = 1;
            // 
            // buttonPrevious
            // 
            buttonPrevious.Location = new System.Drawing.Point(40, 354);
            buttonPrevious.Name = "buttonPrevious";
            buttonPrevious.Size = new System.Drawing.Size(75, 23);
            buttonPrevious.TabIndex = 2;
            buttonPrevious.Text = "previous";
            buttonPrevious.UseVisualStyleBackColor = true;
            buttonPrevious.Click += buttonPrevious_Click;
            // 
            // buttonNext
            // 
            buttonNext.Location = new System.Drawing.Point(186, 354);
            buttonNext.Name = "buttonNext";
            buttonNext.Size = new System.Drawing.Size(75, 23);
            buttonNext.TabIndex = 3;
            buttonNext.Text = "Next";
            buttonNext.UseVisualStyleBackColor = true;
            buttonNext.Click += buttonNext_Click;
            // 
            // trackBarColor
            // 
            trackBarColor.Location = new System.Drawing.Point(335, 67);
            trackBarColor.Maximum = 256;
            trackBarColor.Minimum = -256;
            trackBarColor.Name = "trackBarColor";
            trackBarColor.RightToLeft = System.Windows.Forms.RightToLeft.No;
            trackBarColor.Size = new System.Drawing.Size(227, 45);
            trackBarColor.TabIndex = 4;
            trackBarColor.Scroll += trackBarColor_Scroll;
            trackBarColor.KeyUp += trackBarColor_KeyUp;
            trackBarColor.MouseUp += trackBarColor_MouseUp;
            // 
            // labelColorShift
            // 
            labelColorShift.AutoSize = true;
            labelColorShift.Location = new System.Drawing.Point(568, 82);
            labelColorShift.Name = "labelColorShift";
            labelColorShift.Size = new System.Drawing.Size(65, 15);
            labelColorShift.TabIndex = 5;
            labelColorShift.Text = "Color shift:";
            // 
            // labelColorValues
            // 
            labelColorValues.AutoSize = true;
            labelColorValues.Location = new System.Drawing.Point(345, 115);
            labelColorValues.Name = "labelColorValues";
            labelColorValues.Size = new System.Drawing.Size(73, 15);
            labelColorValues.TabIndex = 6;
            labelColorValues.Text = "color values:";
            // 
            // buttonSavePosition
            // 
            buttonSavePosition.Location = new System.Drawing.Point(345, 143);
            buttonSavePosition.Name = "buttonSavePosition";
            buttonSavePosition.Size = new System.Drawing.Size(46, 23);
            buttonSavePosition.TabIndex = 7;
            buttonSavePosition.Text = "Mark";
            buttonSavePosition.UseVisualStyleBackColor = true;
            buttonSavePosition.Click += buttonSavePosition_Click;
            // 
            // buttonLoadPosition
            // 
            buttonLoadPosition.Location = new System.Drawing.Point(397, 143);
            buttonLoadPosition.Name = "buttonLoadPosition";
            buttonLoadPosition.Size = new System.Drawing.Size(64, 23);
            buttonLoadPosition.TabIndex = 8;
            buttonLoadPosition.Text = "Position";
            buttonLoadPosition.UseVisualStyleBackColor = true;
            buttonLoadPosition.Click += buttonLoadPosition_Click;
            // 
            // buttonCopyToClipboard
            // 
            buttonCopyToClipboard.Location = new System.Drawing.Point(487, 143);
            buttonCopyToClipboard.Name = "buttonCopyToClipboard";
            buttonCopyToClipboard.Size = new System.Drawing.Size(75, 23);
            buttonCopyToClipboard.TabIndex = 9;
            buttonCopyToClipboard.Text = "Clipboard";
            buttonCopyToClipboard.UseVisualStyleBackColor = true;
            buttonCopyToClipboard.Click += buttonCopyToClipboard_Click;
            // 
            // buttonSaveToFile
            // 
            buttonSaveToFile.Location = new System.Drawing.Point(487, 172);
            buttonSaveToFile.Name = "buttonSaveToFile";
            buttonSaveToFile.Size = new System.Drawing.Size(75, 23);
            buttonSaveToFile.TabIndex = 10;
            buttonSaveToFile.Text = "Save to";
            buttonSaveToFile.UseVisualStyleBackColor = true;
            buttonSaveToFile.Click += buttonSaveToFile_Click;
            // 
            // buttonChangeCursorColor
            // 
            buttonChangeCursorColor.Location = new System.Drawing.Point(345, 298);
            buttonChangeCursorColor.Name = "buttonChangeCursorColor";
            buttonChangeCursorColor.Size = new System.Drawing.Size(83, 23);
            buttonChangeCursorColor.TabIndex = 11;
            buttonChangeCursorColor.Text = "Color Cursor";
            buttonChangeCursorColor.UseVisualStyleBackColor = true;
            buttonChangeCursorColor.Click += buttonChangeCursorColor_Click;
            // 
            // SavePatternButton
            // 
            SavePatternButton.Location = new System.Drawing.Point(345, 269);
            SavePatternButton.Name = "SavePatternButton";
            SavePatternButton.Size = new System.Drawing.Size(83, 23);
            SavePatternButton.TabIndex = 12;
            SavePatternButton.Text = "Save Pattern ";
            SavePatternButton.UseVisualStyleBackColor = true;
            SavePatternButton.Click += SavePatternButton_Click;
            // 
            // RestorePatternButton
            // 
            RestorePatternButton.Location = new System.Drawing.Point(434, 269);
            RestorePatternButton.Name = "RestorePatternButton";
            RestorePatternButton.Size = new System.Drawing.Size(95, 23);
            RestorePatternButton.TabIndex = 13;
            RestorePatternButton.Text = "Restore Pattern";
            RestorePatternButton.UseVisualStyleBackColor = true;
            RestorePatternButton.Click += RestorePatternButton_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(434, 298);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(95, 23);
            btnClear.TabIndex = 14;
            btnClear.Text = "Clear Pattern";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // panelColor
            // 
            panelColor.Location = new System.Drawing.Point(523, 115);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(39, 19);
            panelColor.TabIndex = 15;
            // 
            // TextureColorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(694, 399);
            Controls.Add(panelColor);
            Controls.Add(btnClear);
            Controls.Add(RestorePatternButton);
            Controls.Add(SavePatternButton);
            Controls.Add(buttonChangeCursorColor);
            Controls.Add(buttonSaveToFile);
            Controls.Add(buttonCopyToClipboard);
            Controls.Add(buttonLoadPosition);
            Controls.Add(buttonSavePosition);
            Controls.Add(labelColorValues);
            Controls.Add(labelColorShift);
            Controls.Add(trackBarColor);
            Controls.Add(buttonNext);
            Controls.Add(buttonPrevious);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureColorForm";
            Text = "Texture Color";
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trackBarColor).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBoxImageColor;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.TrackBar trackBarColor;
        private System.Windows.Forms.Label labelColorShift;
        private System.Windows.Forms.Label labelColorValues;
        private System.Windows.Forms.Button buttonSavePosition;
        private System.Windows.Forms.Button buttonLoadPosition;
        private System.Windows.Forms.Button buttonCopyToClipboard;
        private System.Windows.Forms.Button buttonSaveToFile;
        private System.Windows.Forms.Button buttonChangeCursorColor;
        private System.Windows.Forms.Button SavePatternButton;
        private System.Windows.Forms.Button RestorePatternButton;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Panel panelColor;
    }
}
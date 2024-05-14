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
            lbIDNumber = new System.Windows.Forms.Label();
            buttonGeneratePattern = new System.Windows.Forms.Button();
            buttonGenerateSquare = new System.Windows.Forms.Panel();
            checkBoxKeepPreviousPattern = new System.Windows.Forms.CheckBox();
            LoadButton = new System.Windows.Forms.Button();
            SaveButton = new System.Windows.Forms.Button();
            labelSize = new System.Windows.Forms.Label();
            labelCount = new System.Windows.Forms.Label();
            checkBoxRandomSize = new System.Windows.Forms.CheckBox();
            trackBarSize = new System.Windows.Forms.TrackBar();
            trackBarCount = new System.Windows.Forms.TrackBar();
            buttonGenerateSquares = new System.Windows.Forms.Button();
            buttonGenerateCircles = new System.Windows.Forms.Button();
            btnImportClipbord = new System.Windows.Forms.Button();
            btnLoadImage = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarColor).BeginInit();
            buttonGenerateSquare.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarCount).BeginInit();
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
            labelColorValues.Location = new System.Drawing.Point(339, 115);
            labelColorValues.Name = "labelColorValues";
            labelColorValues.Size = new System.Drawing.Size(73, 15);
            labelColorValues.TabIndex = 6;
            labelColorValues.Text = "color values:";
            // 
            // buttonSavePosition
            // 
            buttonSavePosition.Location = new System.Drawing.Point(339, 143);
            buttonSavePosition.Name = "buttonSavePosition";
            buttonSavePosition.Size = new System.Drawing.Size(46, 23);
            buttonSavePosition.TabIndex = 7;
            buttonSavePosition.Text = "Mark";
            buttonSavePosition.UseVisualStyleBackColor = true;
            buttonSavePosition.Click += buttonSavePosition_Click;
            // 
            // buttonLoadPosition
            // 
            buttonLoadPosition.Location = new System.Drawing.Point(391, 143);
            buttonLoadPosition.Name = "buttonLoadPosition";
            buttonLoadPosition.Size = new System.Drawing.Size(64, 23);
            buttonLoadPosition.TabIndex = 8;
            buttonLoadPosition.Text = "Position";
            buttonLoadPosition.UseVisualStyleBackColor = true;
            buttonLoadPosition.Click += buttonLoadPosition_Click;
            // 
            // buttonCopyToClipboard
            // 
            buttonCopyToClipboard.Location = new System.Drawing.Point(481, 143);
            buttonCopyToClipboard.Name = "buttonCopyToClipboard";
            buttonCopyToClipboard.Size = new System.Drawing.Size(75, 23);
            buttonCopyToClipboard.TabIndex = 9;
            buttonCopyToClipboard.Text = "Clipboard";
            buttonCopyToClipboard.UseVisualStyleBackColor = true;
            buttonCopyToClipboard.Click += buttonCopyToClipboard_Click;
            // 
            // buttonSaveToFile
            // 
            buttonSaveToFile.Location = new System.Drawing.Point(481, 172);
            buttonSaveToFile.Name = "buttonSaveToFile";
            buttonSaveToFile.Size = new System.Drawing.Size(75, 23);
            buttonSaveToFile.TabIndex = 10;
            buttonSaveToFile.Text = "Save to";
            buttonSaveToFile.UseVisualStyleBackColor = true;
            buttonSaveToFile.Click += buttonSaveToFile_Click;
            // 
            // buttonChangeCursorColor
            // 
            buttonChangeCursorColor.Location = new System.Drawing.Point(339, 275);
            buttonChangeCursorColor.Name = "buttonChangeCursorColor";
            buttonChangeCursorColor.Size = new System.Drawing.Size(83, 23);
            buttonChangeCursorColor.TabIndex = 11;
            buttonChangeCursorColor.Text = "Color Cursor";
            buttonChangeCursorColor.UseVisualStyleBackColor = true;
            buttonChangeCursorColor.Click += buttonChangeCursorColor_Click;
            // 
            // SavePatternButton
            // 
            SavePatternButton.Location = new System.Drawing.Point(339, 246);
            SavePatternButton.Name = "SavePatternButton";
            SavePatternButton.Size = new System.Drawing.Size(83, 23);
            SavePatternButton.TabIndex = 12;
            SavePatternButton.Text = "Save Pattern ";
            SavePatternButton.UseVisualStyleBackColor = true;
            SavePatternButton.Click += SavePatternButton_Click;
            // 
            // RestorePatternButton
            // 
            RestorePatternButton.Location = new System.Drawing.Point(428, 246);
            RestorePatternButton.Name = "RestorePatternButton";
            RestorePatternButton.Size = new System.Drawing.Size(95, 23);
            RestorePatternButton.TabIndex = 13;
            RestorePatternButton.Text = "Restore Pattern";
            RestorePatternButton.UseVisualStyleBackColor = true;
            RestorePatternButton.Click += RestorePatternButton_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(428, 275);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(95, 23);
            btnClear.TabIndex = 14;
            btnClear.Text = "Clear Pattern";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // panelColor
            // 
            panelColor.Location = new System.Drawing.Point(517, 115);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(39, 19);
            panelColor.TabIndex = 15;
            // 
            // lbIDNumber
            // 
            lbIDNumber.AutoSize = true;
            lbIDNumber.Location = new System.Drawing.Point(12, 331);
            lbIDNumber.Name = "lbIDNumber";
            lbIDNumber.Size = new System.Drawing.Size(31, 15);
            lbIDNumber.TabIndex = 16;
            lbIDNumber.Text = "Info:";
            // 
            // buttonGeneratePattern
            // 
            buttonGeneratePattern.Location = new System.Drawing.Point(3, 4);
            buttonGeneratePattern.Name = "buttonGeneratePattern";
            buttonGeneratePattern.Size = new System.Drawing.Size(113, 23);
            buttonGeneratePattern.TabIndex = 17;
            buttonGeneratePattern.Text = "Generate Pattern";
            buttonGeneratePattern.UseVisualStyleBackColor = true;
            buttonGeneratePattern.Click += buttonGeneratePattern_Click;
            // 
            // buttonGenerateSquare
            // 
            buttonGenerateSquare.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            buttonGenerateSquare.Controls.Add(checkBoxKeepPreviousPattern);
            buttonGenerateSquare.Controls.Add(LoadButton);
            buttonGenerateSquare.Controls.Add(SaveButton);
            buttonGenerateSquare.Controls.Add(labelSize);
            buttonGenerateSquare.Controls.Add(labelCount);
            buttonGenerateSquare.Controls.Add(checkBoxRandomSize);
            buttonGenerateSquare.Controls.Add(trackBarSize);
            buttonGenerateSquare.Controls.Add(trackBarCount);
            buttonGenerateSquare.Controls.Add(buttonGenerateSquares);
            buttonGenerateSquare.Controls.Add(buttonGenerateCircles);
            buttonGenerateSquare.Controls.Add(buttonGeneratePattern);
            buttonGenerateSquare.Location = new System.Drawing.Point(339, 304);
            buttonGenerateSquare.Name = "buttonGenerateSquare";
            buttonGenerateSquare.Size = new System.Drawing.Size(343, 131);
            buttonGenerateSquare.TabIndex = 18;
            // 
            // checkBoxKeepPreviousPattern
            // 
            checkBoxKeepPreviousPattern.AutoSize = true;
            checkBoxKeepPreviousPattern.Location = new System.Drawing.Point(150, 110);
            checkBoxKeepPreviousPattern.Name = "checkBoxKeepPreviousPattern";
            checkBoxKeepPreviousPattern.Size = new System.Drawing.Size(124, 19);
            checkBoxKeepPreviousPattern.TabIndex = 27;
            checkBoxKeepPreviousPattern.Text = "More Pattern Load";
            checkBoxKeepPreviousPattern.UseVisualStyleBackColor = true;
            // 
            // LoadButton
            // 
            LoadButton.Location = new System.Drawing.Point(3, 101);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new System.Drawing.Size(113, 23);
            LoadButton.TabIndex = 26;
            LoadButton.Text = "Load Pattern File";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.Location = new System.Drawing.Point(3, 77);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(113, 23);
            SaveButton.TabIndex = 25;
            SaveButton.Text = "Save Pattern File";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // labelSize
            // 
            labelSize.AutoSize = true;
            labelSize.Location = new System.Drawing.Point(250, 60);
            labelSize.Name = "labelSize";
            labelSize.Size = new System.Drawing.Size(30, 15);
            labelSize.TabIndex = 24;
            labelSize.Text = "Size:";
            // 
            // labelCount
            // 
            labelCount.AutoSize = true;
            labelCount.Location = new System.Drawing.Point(250, 12);
            labelCount.Name = "labelCount";
            labelCount.Size = new System.Drawing.Size(43, 15);
            labelCount.TabIndex = 23;
            labelCount.Text = "Count:";
            // 
            // checkBoxRandomSize
            // 
            checkBoxRandomSize.AutoSize = true;
            checkBoxRandomSize.Location = new System.Drawing.Point(150, 85);
            checkBoxRandomSize.Name = "checkBoxRandomSize";
            checkBoxRandomSize.Size = new System.Drawing.Size(94, 19);
            checkBoxRandomSize.TabIndex = 22;
            checkBoxRandomSize.Text = "Random Size";
            checkBoxRandomSize.UseVisualStyleBackColor = true;
            // 
            // trackBarSize
            // 
            trackBarSize.Location = new System.Drawing.Point(140, 48);
            trackBarSize.Maximum = 41;
            trackBarSize.Minimum = 4;
            trackBarSize.Name = "trackBarSize";
            trackBarSize.Size = new System.Drawing.Size(104, 45);
            trackBarSize.TabIndex = 21;
            trackBarSize.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            trackBarSize.Value = 5;
            trackBarSize.Scroll += trackBarSize_Scroll;
            // 
            // trackBarCount
            // 
            trackBarCount.Location = new System.Drawing.Point(140, 2);
            trackBarCount.Maximum = 50;
            trackBarCount.Minimum = 1;
            trackBarCount.Name = "trackBarCount";
            trackBarCount.Size = new System.Drawing.Size(104, 45);
            trackBarCount.TabIndex = 20;
            trackBarCount.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            trackBarCount.Value = 5;
            trackBarCount.Scroll += trackBarCount_Scroll;
            // 
            // buttonGenerateSquares
            // 
            buttonGenerateSquares.Location = new System.Drawing.Point(3, 52);
            buttonGenerateSquares.Name = "buttonGenerateSquares";
            buttonGenerateSquares.Size = new System.Drawing.Size(113, 23);
            buttonGenerateSquares.TabIndex = 19;
            buttonGenerateSquares.Text = "Generate Squares";
            buttonGenerateSquares.UseVisualStyleBackColor = true;
            buttonGenerateSquares.Click += buttonGenerateSquares_Click;
            // 
            // buttonGenerateCircles
            // 
            buttonGenerateCircles.Location = new System.Drawing.Point(3, 28);
            buttonGenerateCircles.Name = "buttonGenerateCircles";
            buttonGenerateCircles.Size = new System.Drawing.Size(113, 23);
            buttonGenerateCircles.TabIndex = 18;
            buttonGenerateCircles.Text = "Generate Circles";
            buttonGenerateCircles.UseVisualStyleBackColor = true;
            buttonGenerateCircles.Click += buttonGenerateCircles_Click;
            // 
            // btnImportClipbord
            // 
            btnImportClipbord.Location = new System.Drawing.Point(562, 143);
            btnImportClipbord.Name = "btnImportClipbord";
            btnImportClipbord.Size = new System.Drawing.Size(59, 23);
            btnImportClipbord.TabIndex = 19;
            btnImportClipbord.Text = "Import";
            btnImportClipbord.UseVisualStyleBackColor = true;
            btnImportClipbord.Click += btnImportClipbord_Click;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new System.Drawing.Point(562, 172);
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.Size = new System.Drawing.Size(59, 23);
            btnLoadImage.TabIndex = 20;
            btnLoadImage.Text = "Load";
            btnLoadImage.UseVisualStyleBackColor = true;
            btnLoadImage.Click += btnLoadImage_Click;
            // 
            // TextureColorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(694, 443);
            Controls.Add(btnLoadImage);
            Controls.Add(btnImportClipbord);
            Controls.Add(buttonGenerateSquare);
            Controls.Add(lbIDNumber);
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
            Text = "Texture Painter and Color and Pattern ";
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trackBarColor).EndInit();
            buttonGenerateSquare.ResumeLayout(false);
            buttonGenerateSquare.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarCount).EndInit();
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
        private System.Windows.Forms.Label lbIDNumber;
        private System.Windows.Forms.Button buttonGeneratePattern;
        private System.Windows.Forms.Panel buttonGenerateSquare;
        private System.Windows.Forms.Button buttonGenerateCircles;
        private System.Windows.Forms.Button buttonGenerateSquares;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.TrackBar trackBarCount;
        private System.Windows.Forms.CheckBox checkBoxRandomSize;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button btnImportClipbord;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.CheckBox checkBoxKeepPreviousPattern;
    }
}
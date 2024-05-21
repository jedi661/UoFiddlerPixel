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
            ButtonPrevious = new System.Windows.Forms.Button();
            ButtonNext = new System.Windows.Forms.Button();
            TrackBarColor = new System.Windows.Forms.TrackBar();
            labelColorShift = new System.Windows.Forms.Label();
            labelColorValues = new System.Windows.Forms.Label();
            ButtonSavePosition = new System.Windows.Forms.Button();
            ButtonLoadPosition = new System.Windows.Forms.Button();
            ButtonCopyToClipboard = new System.Windows.Forms.Button();
            ButtonSaveToFile = new System.Windows.Forms.Button();
            ButtonChangeCursorColor = new System.Windows.Forms.Button();
            SavePatternButton = new System.Windows.Forms.Button();
            RestorePatternButton = new System.Windows.Forms.Button();
            BtnClear = new System.Windows.Forms.Button();
            panelColor = new System.Windows.Forms.Panel();
            lbIDNumber = new System.Windows.Forms.Label();
            ButtonGeneratePattern = new System.Windows.Forms.Button();
            buttonGenerateSquare = new System.Windows.Forms.Panel();
            checkBoxKeepPreviousPattern = new System.Windows.Forms.CheckBox();
            LoadButton = new System.Windows.Forms.Button();
            SaveButton = new System.Windows.Forms.Button();
            labelSize = new System.Windows.Forms.Label();
            labelCount = new System.Windows.Forms.Label();
            checkBoxRandomSize = new System.Windows.Forms.CheckBox();
            trackBarSize = new System.Windows.Forms.TrackBar();
            trackBarCount = new System.Windows.Forms.TrackBar();
            ButtonGenerateSquares = new System.Windows.Forms.Button();
            ButtonGenerateCircles = new System.Windows.Forms.Button();
            BtnImportClipbord = new System.Windows.Forms.Button();
            BtnLoadImage = new System.Windows.Forms.Button();
            tbColorCode = new System.Windows.Forms.TextBox();
            BtColorPincers = new System.Windows.Forms.Button();
            BtExchangeSelectiveColors = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarColor).BeginInit();
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
            PictureBoxImageColor.MouseClick += PictureBoxImageColor_MouseClick;
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
            // ButtonPrevious
            // 
            ButtonPrevious.Location = new System.Drawing.Point(40, 354);
            ButtonPrevious.Name = "ButtonPrevious";
            ButtonPrevious.Size = new System.Drawing.Size(75, 23);
            ButtonPrevious.TabIndex = 2;
            ButtonPrevious.Text = "previous";
            ButtonPrevious.UseVisualStyleBackColor = true;
            ButtonPrevious.Click += ButtonPrevious_Click;
            // 
            // ButtonNext
            // 
            ButtonNext.Location = new System.Drawing.Point(186, 354);
            ButtonNext.Name = "ButtonNext";
            ButtonNext.Size = new System.Drawing.Size(75, 23);
            ButtonNext.TabIndex = 3;
            ButtonNext.Text = "Next";
            ButtonNext.UseVisualStyleBackColor = true;
            ButtonNext.Click += ButtonNext_Click;
            // 
            // TrackBarColor
            // 
            TrackBarColor.Location = new System.Drawing.Point(335, 67);
            TrackBarColor.Maximum = 256;
            TrackBarColor.Minimum = -256;
            TrackBarColor.Name = "TrackBarColor";
            TrackBarColor.RightToLeft = System.Windows.Forms.RightToLeft.No;
            TrackBarColor.Size = new System.Drawing.Size(227, 45);
            TrackBarColor.TabIndex = 4;
            TrackBarColor.Scroll += TrackBarColor_Scroll;
            TrackBarColor.KeyUp += TrackBarColor_KeyUp;
            TrackBarColor.MouseUp += TrackBarColor_MouseUp;
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
            // ButtonSavePosition
            // 
            ButtonSavePosition.Location = new System.Drawing.Point(339, 143);
            ButtonSavePosition.Name = "ButtonSavePosition";
            ButtonSavePosition.Size = new System.Drawing.Size(46, 23);
            ButtonSavePosition.TabIndex = 7;
            ButtonSavePosition.Text = "Mark";
            ButtonSavePosition.UseVisualStyleBackColor = true;
            ButtonSavePosition.Click += ButtonSavePosition_Click;
            // 
            // ButtonLoadPosition
            // 
            ButtonLoadPosition.Location = new System.Drawing.Point(391, 143);
            ButtonLoadPosition.Name = "ButtonLoadPosition";
            ButtonLoadPosition.Size = new System.Drawing.Size(64, 23);
            ButtonLoadPosition.TabIndex = 8;
            ButtonLoadPosition.Text = "Position";
            ButtonLoadPosition.UseVisualStyleBackColor = true;
            ButtonLoadPosition.Click += ButtonLoadPosition_Click;
            // 
            // ButtonCopyToClipboard
            // 
            ButtonCopyToClipboard.Location = new System.Drawing.Point(481, 143);
            ButtonCopyToClipboard.Name = "ButtonCopyToClipboard";
            ButtonCopyToClipboard.Size = new System.Drawing.Size(75, 23);
            ButtonCopyToClipboard.TabIndex = 9;
            ButtonCopyToClipboard.Text = "Clipboard";
            ButtonCopyToClipboard.UseVisualStyleBackColor = true;
            ButtonCopyToClipboard.Click += ButtonCopyToClipboard_Click;
            // 
            // ButtonSaveToFile
            // 
            ButtonSaveToFile.Location = new System.Drawing.Point(481, 172);
            ButtonSaveToFile.Name = "ButtonSaveToFile";
            ButtonSaveToFile.Size = new System.Drawing.Size(75, 23);
            ButtonSaveToFile.TabIndex = 10;
            ButtonSaveToFile.Text = "Save to";
            ButtonSaveToFile.UseVisualStyleBackColor = true;
            ButtonSaveToFile.Click += ButtonSaveToFile_Click;
            // 
            // ButtonChangeCursorColor
            // 
            ButtonChangeCursorColor.Location = new System.Drawing.Point(339, 275);
            ButtonChangeCursorColor.Name = "ButtonChangeCursorColor";
            ButtonChangeCursorColor.Size = new System.Drawing.Size(83, 23);
            ButtonChangeCursorColor.TabIndex = 11;
            ButtonChangeCursorColor.Text = "Color Cursor";
            ButtonChangeCursorColor.UseVisualStyleBackColor = true;
            ButtonChangeCursorColor.Click += ButtonChangeCursorColor_Click;
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
            RestorePatternButton.Location = new System.Drawing.Point(422, 246);
            RestorePatternButton.Name = "RestorePatternButton";
            RestorePatternButton.Size = new System.Drawing.Size(95, 23);
            RestorePatternButton.TabIndex = 13;
            RestorePatternButton.Text = "Restore Pattern";
            RestorePatternButton.UseVisualStyleBackColor = true;
            RestorePatternButton.Click += RestorePatternButton_Click;
            // 
            // BtnClear
            // 
            BtnClear.Location = new System.Drawing.Point(422, 275);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(95, 23);
            BtnClear.TabIndex = 14;
            BtnClear.Text = "Clear Pattern";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;
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
            // ButtonGeneratePattern
            // 
            ButtonGeneratePattern.Location = new System.Drawing.Point(3, 4);
            ButtonGeneratePattern.Name = "ButtonGeneratePattern";
            ButtonGeneratePattern.Size = new System.Drawing.Size(113, 23);
            ButtonGeneratePattern.TabIndex = 17;
            ButtonGeneratePattern.Text = "Generate Pattern";
            ButtonGeneratePattern.UseVisualStyleBackColor = true;
            ButtonGeneratePattern.Click += ButtonGeneratePattern_Click;
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
            buttonGenerateSquare.Controls.Add(ButtonGenerateSquares);
            buttonGenerateSquare.Controls.Add(ButtonGenerateCircles);
            buttonGenerateSquare.Controls.Add(ButtonGeneratePattern);
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
            trackBarSize.Scroll += TrackBarSize_Scroll;
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
            trackBarCount.Scroll += TrackBarCount_Scroll;
            // 
            // ButtonGenerateSquares
            // 
            ButtonGenerateSquares.Location = new System.Drawing.Point(3, 52);
            ButtonGenerateSquares.Name = "ButtonGenerateSquares";
            ButtonGenerateSquares.Size = new System.Drawing.Size(113, 23);
            ButtonGenerateSquares.TabIndex = 19;
            ButtonGenerateSquares.Text = "Generate Squares";
            ButtonGenerateSquares.UseVisualStyleBackColor = true;
            ButtonGenerateSquares.Click += ButtonGenerateSquares_Click;
            // 
            // ButtonGenerateCircles
            // 
            ButtonGenerateCircles.Location = new System.Drawing.Point(3, 28);
            ButtonGenerateCircles.Name = "ButtonGenerateCircles";
            ButtonGenerateCircles.Size = new System.Drawing.Size(113, 23);
            ButtonGenerateCircles.TabIndex = 18;
            ButtonGenerateCircles.Text = "Generate Circles";
            ButtonGenerateCircles.UseVisualStyleBackColor = true;
            ButtonGenerateCircles.Click += ButtonGenerateCircles_Click;
            // 
            // BtnImportClipbord
            // 
            BtnImportClipbord.Location = new System.Drawing.Point(562, 143);
            BtnImportClipbord.Name = "BtnImportClipbord";
            BtnImportClipbord.Size = new System.Drawing.Size(59, 23);
            BtnImportClipbord.TabIndex = 19;
            BtnImportClipbord.Text = "Import";
            BtnImportClipbord.UseVisualStyleBackColor = true;
            BtnImportClipbord.Click += BtnImportClipbord_Click;
            // 
            // BtnLoadImage
            // 
            BtnLoadImage.Location = new System.Drawing.Point(562, 172);
            BtnLoadImage.Name = "BtnLoadImage";
            BtnLoadImage.Size = new System.Drawing.Size(59, 23);
            BtnLoadImage.TabIndex = 20;
            BtnLoadImage.Text = "Load";
            BtnLoadImage.UseVisualStyleBackColor = true;
            BtnLoadImage.Click += BtnLoadImage_Click;
            // 
            // tbColorCode
            // 
            tbColorCode.Location = new System.Drawing.Point(518, 275);
            tbColorCode.Name = "tbColorCode";
            tbColorCode.Size = new System.Drawing.Size(92, 23);
            tbColorCode.TabIndex = 21;
            // 
            // BtColorPincers
            // 
            BtColorPincers.Location = new System.Drawing.Point(518, 246);
            BtColorPincers.Name = "BtColorPincers";
            BtColorPincers.Size = new System.Drawing.Size(56, 23);
            BtColorPincers.TabIndex = 22;
            BtColorPincers.Text = "Pipette";
            BtColorPincers.UseVisualStyleBackColor = true;
            BtColorPincers.Click += BtColorPincers_Click;
            // 
            // BtExchangeSelectiveColors
            // 
            BtExchangeSelectiveColors.Location = new System.Drawing.Point(575, 246);
            BtExchangeSelectiveColors.Name = "BtExchangeSelectiveColors";
            BtExchangeSelectiveColors.Size = new System.Drawing.Size(70, 23);
            BtExchangeSelectiveColors.TabIndex = 23;
            BtExchangeSelectiveColors.Text = "Exchange";
            BtExchangeSelectiveColors.UseVisualStyleBackColor = true;
            BtExchangeSelectiveColors.Click += BtExchangeSelectiveColors_Click;
            // 
            // TextureColorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(694, 443);
            Controls.Add(BtExchangeSelectiveColors);
            Controls.Add(BtColorPincers);
            Controls.Add(tbColorCode);
            Controls.Add(BtnLoadImage);
            Controls.Add(BtnImportClipbord);
            Controls.Add(buttonGenerateSquare);
            Controls.Add(lbIDNumber);
            Controls.Add(panelColor);
            Controls.Add(BtnClear);
            Controls.Add(RestorePatternButton);
            Controls.Add(SavePatternButton);
            Controls.Add(ButtonChangeCursorColor);
            Controls.Add(ButtonSaveToFile);
            Controls.Add(ButtonCopyToClipboard);
            Controls.Add(ButtonLoadPosition);
            Controls.Add(ButtonSavePosition);
            Controls.Add(labelColorValues);
            Controls.Add(labelColorShift);
            Controls.Add(TrackBarColor);
            Controls.Add(ButtonNext);
            Controls.Add(ButtonPrevious);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureColorForm";
            Text = "Texture Painter and Color and Pattern ";
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).EndInit();
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)TrackBarColor).EndInit();
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
        private System.Windows.Forms.Button ButtonPrevious;
        private System.Windows.Forms.Button ButtonNext;
        private System.Windows.Forms.TrackBar TrackBarColor;
        private System.Windows.Forms.Label labelColorShift;
        private System.Windows.Forms.Label labelColorValues;
        private System.Windows.Forms.Button ButtonSavePosition;
        private System.Windows.Forms.Button ButtonLoadPosition;
        private System.Windows.Forms.Button ButtonCopyToClipboard;
        private System.Windows.Forms.Button ButtonSaveToFile;
        private System.Windows.Forms.Button ButtonChangeCursorColor;
        private System.Windows.Forms.Button SavePatternButton;
        private System.Windows.Forms.Button RestorePatternButton;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label lbIDNumber;
        private System.Windows.Forms.Button ButtonGeneratePattern;
        private System.Windows.Forms.Panel buttonGenerateSquare;
        private System.Windows.Forms.Button ButtonGenerateCircles;
        private System.Windows.Forms.Button ButtonGenerateSquares;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.TrackBar trackBarCount;
        private System.Windows.Forms.CheckBox checkBoxRandomSize;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button BtnImportClipbord;
        private System.Windows.Forms.Button BtnLoadImage;
        private System.Windows.Forms.CheckBox checkBoxKeepPreviousPattern;
        private System.Windows.Forms.TextBox tbColorCode;
        private System.Windows.Forms.Button BtColorPincers;
        private System.Windows.Forms.Button BtExchangeSelectiveColors;
    }
}
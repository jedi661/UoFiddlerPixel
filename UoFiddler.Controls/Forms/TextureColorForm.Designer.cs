// /***************************************************************************
//  *
//  * $Author: Nikodemus (original), refactored & extended
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class TextureColorForm
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            _customCursor?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureColorForm));
            PictureBoxImageColor = new System.Windows.Forms.PictureBox();
            panelPictureBoxHost = new System.Windows.Forms.Panel();
            lbIDNumber = new System.Windows.Forms.Label();
            ButtonPrevious = new System.Windows.Forms.Button();
            ButtonNext = new System.Windows.Forms.Button();
            grpHueShift = new System.Windows.Forms.GroupBox();
            TrackBarColor = new System.Windows.Forms.TrackBar();
            labelColorShift = new System.Windows.Forms.Label();
            ButtonSavePosition = new System.Windows.Forms.Button();
            ButtonLoadPosition = new System.Windows.Forms.Button();
            grpAdjust = new System.Windows.Forms.GroupBox();
            labelSaturation = new System.Windows.Forms.Label();
            trackBarSaturation = new System.Windows.Forms.TrackBar();
            labelBrightness = new System.Windows.Forms.Label();
            trackBarBrightness = new System.Windows.Forms.TrackBar();
            labelContrast = new System.Windows.Forms.Label();
            trackBarContrast = new System.Windows.Forms.TrackBar();
            grpColorInfo = new System.Windows.Forms.GroupBox();
            panelColor = new System.Windows.Forms.Panel();
            labelColorValues = new System.Windows.Forms.Label();
            tbColorCode = new System.Windows.Forms.TextBox();
            BtColorPincers = new System.Windows.Forms.Button();
            BtExchangeSelectiveColors = new System.Windows.Forms.Button();
            numericTolerance = new System.Windows.Forms.NumericUpDown();
            labelTolerance = new System.Windows.Forms.Label();
            grpEffects = new System.Windows.Forms.GroupBox();
            BtnGrayscale = new System.Windows.Forms.Button();
            BtnSepia = new System.Windows.Forms.Button();
            BtnInvert = new System.Windows.Forms.Button();
            BtnFlipH = new System.Windows.Forms.Button();
            BtnFlipV = new System.Windows.Forms.Button();
            ButtonGenerateCircles = new System.Windows.Forms.Button();
            ButtonGeneratePattern = new System.Windows.Forms.Button();
            BtnRotate90 = new System.Windows.Forms.Button();
            BtnResetImage = new System.Windows.Forms.Button();
            BtnUndo = new System.Windows.Forms.Button();
            grpIO = new System.Windows.Forms.GroupBox();
            ButtonCopyToClipboard = new System.Windows.Forms.Button();
            BtnImportClipboard = new System.Windows.Forms.Button();
            ButtonSaveToFile = new System.Windows.Forms.Button();
            BtnLoadImage = new System.Windows.Forms.Button();
            grpSelection = new System.Windows.Forms.GroupBox();
            SavePatternButton = new System.Windows.Forms.Button();
            RestorePatternButton = new System.Windows.Forms.Button();
            BtnClear = new System.Windows.Forms.Button();
            SaveButton = new System.Windows.Forms.Button();
            LoadButton = new System.Windows.Forms.Button();
            checkBoxKeepPreviousPattern = new System.Windows.Forms.CheckBox();
            ButtonChangeCursorColor = new System.Windows.Forms.Button();
            grpShapes = new System.Windows.Forms.GroupBox();
            trackBarCount = new System.Windows.Forms.TrackBar();
            labelCount = new System.Windows.Forms.Label();
            trackBarSize = new System.Windows.Forms.TrackBar();
            labelSize = new System.Windows.Forms.Label();
            checkBoxRandomSize = new System.Windows.Forms.CheckBox();
            ButtonGenerateSquares = new System.Windows.Forms.Button();
            ButtonGenerateTriangles = new System.Windows.Forms.Button();
            ButtonGenerateDiamonds = new System.Windows.Forms.Button();
            ButtonGenerateStars = new System.Windows.Forms.Button();
            ButtonGenerateHexagons = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).BeginInit();
            panelPictureBoxHost.SuspendLayout();
            grpHueShift.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarColor).BeginInit();
            grpAdjust.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSaturation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarBrightness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarContrast).BeginInit();
            grpColorInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).BeginInit();
            grpEffects.SuspendLayout();
            grpIO.SuspendLayout();
            grpSelection.SuspendLayout();
            grpShapes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSize).BeginInit();
            SuspendLayout();

            // ──────────────────────────────────────────────────────────────────────
            // PictureBoxImageColor
            // ──────────────────────────────────────────────────────────────────────
            PictureBoxImageColor.Location = new System.Drawing.Point(9, 9);
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

            // ──────────────────────────────────────────────────────────────────────
            // panelPictureBoxHost
            // ──────────────────────────────────────────────────────────────────────
            panelPictureBoxHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelPictureBoxHost.Controls.Add(PictureBoxImageColor);
            panelPictureBoxHost.Location = new System.Drawing.Point(12, 38);
            panelPictureBoxHost.Name = "panelPictureBoxHost";
            panelPictureBoxHost.Size = new System.Drawing.Size(276, 276);
            panelPictureBoxHost.TabIndex = 1;

            // ──────────────────────────────────────────────────────────────────────
            // lbIDNumber
            // ──────────────────────────────────────────────────────────────────────
            lbIDNumber.AutoSize = true;
            lbIDNumber.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            lbIDNumber.Location = new System.Drawing.Point(12, 13);
            lbIDNumber.Name = "lbIDNumber";
            lbIDNumber.Size = new System.Drawing.Size(31, 13);
            lbIDNumber.TabIndex = 2;
            lbIDNumber.Text = "Info:";

            // ──────────────────────────────────────────────────────────────────────
            // ButtonPrevious / ButtonNext
            // ──────────────────────────────────────────────────────────────────────
            ButtonPrevious.Location = new System.Drawing.Point(12, 322);
            ButtonPrevious.Name = "ButtonPrevious";
            ButtonPrevious.Size = new System.Drawing.Size(90, 26);
            ButtonPrevious.TabIndex = 3;
            ButtonPrevious.Text = "◄  Previous";
            ButtonPrevious.UseVisualStyleBackColor = true;
            ButtonPrevious.Click += ButtonPrevious_Click;

            ButtonNext.Location = new System.Drawing.Point(198, 322);
            ButtonNext.Name = "ButtonNext";
            ButtonNext.Size = new System.Drawing.Size(90, 26);
            ButtonNext.TabIndex = 4;
            ButtonNext.Text = "Next  ►";
            ButtonNext.UseVisualStyleBackColor = true;
            ButtonNext.Click += ButtonNext_Click;

            // ──────────────────────────────────────────────────────────────────────
            // grpHueShift
            // ──────────────────────────────────────────────────────────────────────
            grpHueShift.Controls.Add(TrackBarColor);
            grpHueShift.Controls.Add(labelColorShift);
            grpHueShift.Controls.Add(ButtonSavePosition);
            grpHueShift.Controls.Add(ButtonLoadPosition);
            grpHueShift.Location = new System.Drawing.Point(300, 8);
            grpHueShift.Name = "grpHueShift";
            grpHueShift.Size = new System.Drawing.Size(370, 80);
            grpHueShift.TabIndex = 10;
            grpHueShift.TabStop = false;
            grpHueShift.Text = "Hue Shift";

            TrackBarColor.Location = new System.Drawing.Point(6, 20);
            TrackBarColor.Maximum = 180;
            TrackBarColor.Minimum = -180;
            TrackBarColor.Name = "TrackBarColor";
            TrackBarColor.Size = new System.Drawing.Size(230, 45);
            TrackBarColor.TabIndex = 0;
            TrackBarColor.Scroll += TrackBarColor_Scroll;
            TrackBarColor.KeyUp += TrackBarColor_KeyUp;
            TrackBarColor.MouseUp += TrackBarColor_MouseUp;

            labelColorShift.AutoSize = true;
            labelColorShift.Location = new System.Drawing.Point(240, 50);
            labelColorShift.Name = "labelColorShift";
            labelColorShift.Size = new System.Drawing.Size(73, 15);
            labelColorShift.TabIndex = 1;
            labelColorShift.Text = "Hue Shift: 0°";

            ButtonSavePosition.Location = new System.Drawing.Point(240, 20);
            ButtonSavePosition.Name = "ButtonSavePosition";
            ButtonSavePosition.Size = new System.Drawing.Size(55, 22);
            ButtonSavePosition.TabIndex = 1;
            ButtonSavePosition.Text = "Mark";
            ButtonSavePosition.UseVisualStyleBackColor = true;
            ButtonSavePosition.Click += ButtonSavePosition_Click;

            ButtonLoadPosition.Location = new System.Drawing.Point(302, 20);
            ButtonLoadPosition.Name = "ButtonLoadPosition";
            ButtonLoadPosition.Size = new System.Drawing.Size(60, 22);
            ButtonLoadPosition.TabIndex = 2;
            ButtonLoadPosition.Text = "Recall";
            ButtonLoadPosition.UseVisualStyleBackColor = true;
            ButtonLoadPosition.Click += ButtonLoadPosition_Click;

            // ──────────────────────────────────────────────────────────────────────
            // grpAdjust
            // ──────────────────────────────────────────────────────────────────────
            grpAdjust.Controls.Add(labelSaturation);
            grpAdjust.Controls.Add(trackBarSaturation);
            grpAdjust.Controls.Add(labelBrightness);
            grpAdjust.Controls.Add(trackBarBrightness);
            grpAdjust.Controls.Add(labelContrast);
            grpAdjust.Controls.Add(trackBarContrast);
            grpAdjust.Location = new System.Drawing.Point(300, 95);
            grpAdjust.Name = "grpAdjust";
            grpAdjust.Size = new System.Drawing.Size(370, 133);
            grpAdjust.TabIndex = 11;
            grpAdjust.TabStop = false;
            grpAdjust.Text = "Adjustments";

            labelSaturation.AutoSize = true;
            labelSaturation.Location = new System.Drawing.Point(6, 20);
            labelSaturation.Name = "labelSaturation";
            labelSaturation.Size = new System.Drawing.Size(73, 15);
            labelSaturation.TabIndex = 0;
            labelSaturation.Text = "Saturation: 0";

            trackBarSaturation.Location = new System.Drawing.Point(90, 11);
            trackBarSaturation.Maximum = 100;
            trackBarSaturation.Minimum = -100;
            trackBarSaturation.Name = "trackBarSaturation";
            trackBarSaturation.Size = new System.Drawing.Size(270, 45);
            trackBarSaturation.TabIndex = 0;
            trackBarSaturation.Scroll += TrackBarSaturation_Scroll;
            trackBarSaturation.MouseUp += TrackBarAdjust_MouseUp;   // FIX: only one registration (removed duplicate from constructor)

            labelBrightness.AutoSize = true;
            labelBrightness.Location = new System.Drawing.Point(6, 55);
            labelBrightness.Name = "labelBrightness";
            labelBrightness.Size = new System.Drawing.Size(74, 15);
            labelBrightness.TabIndex = 1;
            labelBrightness.Text = "Brightness: 0";

            trackBarBrightness.Location = new System.Drawing.Point(90, 50);
            trackBarBrightness.Maximum = 100;
            trackBarBrightness.Minimum = -100;
            trackBarBrightness.Name = "trackBarBrightness";
            trackBarBrightness.Size = new System.Drawing.Size(270, 45);
            trackBarBrightness.TabIndex = 1;
            trackBarBrightness.Scroll += TrackBarBrightness_Scroll;
            trackBarBrightness.MouseUp += TrackBarAdjust_MouseUp;   // FIX: only one registration

            labelContrast.AutoSize = true;
            labelContrast.Location = new System.Drawing.Point(6, 90);
            labelContrast.Name = "labelContrast";
            labelContrast.Size = new System.Drawing.Size(70, 15);
            labelContrast.TabIndex = 2;
            labelContrast.Text = "Contrast:   0";

            trackBarContrast.Location = new System.Drawing.Point(90, 93);
            trackBarContrast.Maximum = 100;
            trackBarContrast.Minimum = -100;
            trackBarContrast.Name = "trackBarContrast";
            trackBarContrast.Size = new System.Drawing.Size(270, 45);
            trackBarContrast.TabIndex = 2;
            trackBarContrast.Scroll += TrackBarContrast_Scroll;
            trackBarContrast.MouseUp += TrackBarAdjust_MouseUp;     // FIX: only one registration

            // ──────────────────────────────────────────────────────────────────────
            // grpColorInfo
            // ──────────────────────────────────────────────────────────────────────
            grpColorInfo.Controls.Add(panelColor);
            grpColorInfo.Controls.Add(labelColorValues);
            grpColorInfo.Controls.Add(tbColorCode);
            grpColorInfo.Controls.Add(BtColorPincers);
            grpColorInfo.Controls.Add(BtExchangeSelectiveColors);
            grpColorInfo.Controls.Add(numericTolerance);
            grpColorInfo.Controls.Add(labelTolerance);
            grpColorInfo.Location = new System.Drawing.Point(300, 234);
            grpColorInfo.Name = "grpColorInfo";
            grpColorInfo.Size = new System.Drawing.Size(370, 100);
            grpColorInfo.TabIndex = 12;
            grpColorInfo.TabStop = false;
            grpColorInfo.Text = "Color Info & Exchange";

            panelColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelColor.Location = new System.Drawing.Point(6, 20);
            panelColor.Name = "panelColor";
            panelColor.Size = new System.Drawing.Size(40, 20);
            panelColor.TabIndex = 0;

            labelColorValues.AutoSize = true;
            labelColorValues.Location = new System.Drawing.Point(54, 22);
            labelColorValues.Name = "labelColorValues";
            labelColorValues.Size = new System.Drawing.Size(119, 15);
            labelColorValues.TabIndex = 1;
            labelColorValues.Text = "R: --  G: --  B: --  A: --";

            tbColorCode.Location = new System.Drawing.Point(6, 48);
            tbColorCode.Name = "tbColorCode";
            tbColorCode.Size = new System.Drawing.Size(80, 23);
            tbColorCode.TabIndex = 1;

            BtColorPincers.Location = new System.Drawing.Point(94, 48);
            BtColorPincers.Name = "BtColorPincers";
            BtColorPincers.Size = new System.Drawing.Size(70, 23);
            BtColorPincers.TabIndex = 2;
            BtColorPincers.Text = "Pipette";
            BtColorPincers.UseVisualStyleBackColor = true;
            BtColorPincers.Click += BtColorPincers_Click;

            BtExchangeSelectiveColors.Location = new System.Drawing.Point(172, 48);
            BtExchangeSelectiveColors.Name = "BtExchangeSelectiveColors";
            BtExchangeSelectiveColors.Size = new System.Drawing.Size(80, 23);
            BtExchangeSelectiveColors.TabIndex = 3;
            BtExchangeSelectiveColors.Text = "Exchange";
            BtExchangeSelectiveColors.UseVisualStyleBackColor = true;
            BtExchangeSelectiveColors.Click += BtExchangeSelectiveColors_Click;

            numericTolerance.Location = new System.Drawing.Point(286, 48);
            numericTolerance.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
            numericTolerance.Name = "numericTolerance";
            numericTolerance.Size = new System.Drawing.Size(55, 23);
            numericTolerance.TabIndex = 4;

            labelTolerance.AutoSize = true;
            labelTolerance.Location = new System.Drawing.Point(260, 52);
            labelTolerance.Name = "labelTolerance";
            labelTolerance.Size = new System.Drawing.Size(25, 15);
            labelTolerance.TabIndex = 5;
            labelTolerance.Text = "Tol:";

            // ──────────────────────────────────────────────────────────────────────
            // grpEffects
            // ──────────────────────────────────────────────────────────────────────
            grpEffects.Controls.Add(BtnGrayscale);
            grpEffects.Controls.Add(BtnSepia);
            grpEffects.Controls.Add(BtnInvert);
            grpEffects.Controls.Add(BtnFlipH);
            grpEffects.Controls.Add(BtnFlipV);
            grpEffects.Controls.Add(BtnRotate90);
            grpEffects.Controls.Add(BtnResetImage);
            grpEffects.Controls.Add(BtnUndo);
            grpEffects.Location = new System.Drawing.Point(300, 334);
            grpEffects.Name = "grpEffects";
            grpEffects.Size = new System.Drawing.Size(370, 115);
            grpEffects.TabIndex = 13;
            grpEffects.TabStop = false;
            grpEffects.Text = "Image Effects";

            BtnGrayscale.Location = new System.Drawing.Point(3, 50);
            BtnGrayscale.Name = "BtnGrayscale";
            BtnGrayscale.Size = new System.Drawing.Size(73, 22);
            BtnGrayscale.TabIndex = 0;
            BtnGrayscale.Text = "Grayscale";
            BtnGrayscale.UseVisualStyleBackColor = true;
            BtnGrayscale.Click += BtnGrayscale_Click;

            BtnSepia.Location = new System.Drawing.Point(4, 78);
            BtnSepia.Name = "BtnSepia";
            BtnSepia.Size = new System.Drawing.Size(62, 22);
            BtnSepia.TabIndex = 1;
            BtnSepia.Text = "Sepia";
            BtnSepia.UseVisualStyleBackColor = true;
            BtnSepia.Click += BtnSepia_Click;

            BtnInvert.Location = new System.Drawing.Point(72, 78);
            BtnInvert.Name = "BtnInvert";
            BtnInvert.Size = new System.Drawing.Size(62, 22);
            BtnInvert.TabIndex = 2;
            BtnInvert.Text = "Invert";
            BtnInvert.UseVisualStyleBackColor = true;
            BtnInvert.Click += BtnInvert_Click;

            BtnFlipH.Location = new System.Drawing.Point(140, 78);
            BtnFlipH.Name = "BtnFlipH";
            BtnFlipH.Size = new System.Drawing.Size(43, 22);
            BtnFlipH.TabIndex = 3;
            BtnFlipH.Text = "FlipH";
            BtnFlipH.UseVisualStyleBackColor = true;
            BtnFlipH.Click += BtnFlipH_Click;

            BtnFlipV.Location = new System.Drawing.Point(187, 78);
            BtnFlipV.Name = "BtnFlipV";
            BtnFlipV.Size = new System.Drawing.Size(45, 22);
            BtnFlipV.TabIndex = 4;
            BtnFlipV.Text = "FlipV";
            BtnFlipV.UseVisualStyleBackColor = true;
            BtnFlipV.Click += BtnFlipV_Click;

            BtnRotate90.Location = new System.Drawing.Point(3, 22);
            BtnRotate90.Name = "BtnRotate90";
            BtnRotate90.Size = new System.Drawing.Size(74, 22);
            BtnRotate90.TabIndex = 5;
            BtnRotate90.Text = "Rotate 90";
            BtnRotate90.UseVisualStyleBackColor = true;
            BtnRotate90.Click += BtnRotate90_Click;

            BtnResetImage.Location = new System.Drawing.Point(238, 78);
            BtnResetImage.Name = "BtnResetImage";
            BtnResetImage.Size = new System.Drawing.Size(88, 22);
            BtnResetImage.TabIndex = 6;
            BtnResetImage.Text = "Reset Image";
            BtnResetImage.UseVisualStyleBackColor = true;
            BtnResetImage.Click += BtnResetImage_Click;

            BtnUndo.Location = new System.Drawing.Point(301, 22);
            BtnUndo.Name = "BtnUndo";
            BtnUndo.Size = new System.Drawing.Size(50, 22);
            BtnUndo.TabIndex = 7;
            BtnUndo.Text = "Undo";
            BtnUndo.UseVisualStyleBackColor = true;
            BtnUndo.Click += BtnUndo_Click;

            // ──────────────────────────────────────────────────────────────────────
            // grpIO
            // ──────────────────────────────────────────────────────────────────────
            grpIO.Controls.Add(ButtonCopyToClipboard);
            grpIO.Controls.Add(BtnImportClipboard);
            grpIO.Controls.Add(ButtonSaveToFile);
            grpIO.Controls.Add(BtnLoadImage);
            grpIO.Location = new System.Drawing.Point(300, 451);
            grpIO.Name = "grpIO";
            grpIO.Size = new System.Drawing.Size(370, 56);
            grpIO.TabIndex = 14;
            grpIO.TabStop = false;
            grpIO.Text = "Clipboard & File";

            ButtonCopyToClipboard.Location = new System.Drawing.Point(3, 22);
            ButtonCopyToClipboard.Name = "ButtonCopyToClipboard";
            ButtonCopyToClipboard.Size = new System.Drawing.Size(86, 23);
            ButtonCopyToClipboard.TabIndex = 0;
            ButtonCopyToClipboard.Text = "Copy Image";
            ButtonCopyToClipboard.UseVisualStyleBackColor = true;
            ButtonCopyToClipboard.Click += ButtonCopyToClipboard_Click;

            BtnImportClipboard.Location = new System.Drawing.Point(95, 22);
            BtnImportClipboard.Name = "BtnImportClipboard";
            BtnImportClipboard.Size = new System.Drawing.Size(86, 23);
            BtnImportClipboard.TabIndex = 1;
            BtnImportClipboard.Text = "Paste Image";
            BtnImportClipboard.UseVisualStyleBackColor = true;
            BtnImportClipboard.Click += BtnImportClipboard_Click;

            ButtonSaveToFile.Location = new System.Drawing.Point(187, 22);
            ButtonSaveToFile.Name = "ButtonSaveToFile";
            ButtonSaveToFile.Size = new System.Drawing.Size(86, 23);
            ButtonSaveToFile.TabIndex = 2;
            ButtonSaveToFile.Text = "Export…";
            ButtonSaveToFile.UseVisualStyleBackColor = true;
            ButtonSaveToFile.Click += ButtonSaveToFile_Click;

            BtnLoadImage.Location = new System.Drawing.Point(279, 22);
            BtnLoadImage.Name = "BtnLoadImage";
            BtnLoadImage.Size = new System.Drawing.Size(86, 23);
            BtnLoadImage.TabIndex = 3;
            BtnLoadImage.Text = "Load File…";
            BtnLoadImage.UseVisualStyleBackColor = true;
            BtnLoadImage.Click += BtnLoadImage_Click;

            // ──────────────────────────────────────────────────────────────────────
            // grpSelection
            // ──────────────────────────────────────────────────────────────────────
            grpSelection.Controls.Add(SavePatternButton);
            grpSelection.Controls.Add(RestorePatternButton);
            grpSelection.Controls.Add(BtnClear);
            grpSelection.Controls.Add(SaveButton);
            grpSelection.Controls.Add(LoadButton);
            grpSelection.Controls.Add(checkBoxKeepPreviousPattern);
            grpSelection.Controls.Add(ButtonChangeCursorColor);
            grpSelection.Location = new System.Drawing.Point(300, 511);
            grpSelection.Name = "grpSelection";
            grpSelection.Size = new System.Drawing.Size(370, 82);
            grpSelection.TabIndex = 15;
            grpSelection.TabStop = false;
            grpSelection.Text = "Selection & Pattern";

            SavePatternButton.Location = new System.Drawing.Point(3, 20);
            SavePatternButton.Name = "SavePatternButton";
            SavePatternButton.Size = new System.Drawing.Size(84, 23);
            SavePatternButton.TabIndex = 0;
            SavePatternButton.Text = "Save Pattern";
            SavePatternButton.UseVisualStyleBackColor = true;
            SavePatternButton.Click += SavePatternButton_Click;

            RestorePatternButton.Location = new System.Drawing.Point(93, 20);
            RestorePatternButton.Name = "RestorePatternButton";
            RestorePatternButton.Size = new System.Drawing.Size(90, 23);
            RestorePatternButton.TabIndex = 1;
            RestorePatternButton.Text = "Restore Pattern";
            RestorePatternButton.UseVisualStyleBackColor = true;
            RestorePatternButton.Click += RestorePatternButton_Click;

            BtnClear.Location = new System.Drawing.Point(189, 20);
            BtnClear.Name = "BtnClear";
            BtnClear.Size = new System.Drawing.Size(80, 23);
            BtnClear.TabIndex = 2;
            BtnClear.Text = "Clear All";
            BtnClear.UseVisualStyleBackColor = true;
            BtnClear.Click += BtnClear_Click;

            SaveButton.Location = new System.Drawing.Point(3, 50);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(84, 23);
            SaveButton.TabIndex = 3;
            SaveButton.Text = "Save to File";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;

            LoadButton.Location = new System.Drawing.Point(93, 50);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new System.Drawing.Size(84, 23);
            LoadButton.TabIndex = 4;
            LoadButton.Text = "Load from File";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;

            checkBoxKeepPreviousPattern.AutoSize = true;
            checkBoxKeepPreviousPattern.Location = new System.Drawing.Point(184, 53);
            checkBoxKeepPreviousPattern.Name = "checkBoxKeepPreviousPattern";
            checkBoxKeepPreviousPattern.Size = new System.Drawing.Size(114, 19);
            checkBoxKeepPreviousPattern.TabIndex = 5;
            checkBoxKeepPreviousPattern.Text = "Append on Load";

            ButtonChangeCursorColor.Location = new System.Drawing.Point(280, 20);
            ButtonChangeCursorColor.Name = "ButtonChangeCursorColor";
            ButtonChangeCursorColor.Size = new System.Drawing.Size(84, 23);
            ButtonChangeCursorColor.TabIndex = 6;
            ButtonChangeCursorColor.Text = "Cursor Color";
            ButtonChangeCursorColor.UseVisualStyleBackColor = true;
            ButtonChangeCursorColor.Click += ButtonChangeCursorColor_Click;

            // ──────────────────────────────────────────────────────────────────────
            // grpShapes
            // ──────────────────────────────────────────────────────────────────────
            grpShapes.Controls.Add(trackBarCount);
            grpShapes.Controls.Add(labelCount);
            grpShapes.Controls.Add(trackBarSize);
            grpShapes.Controls.Add(labelSize);
            grpShapes.Controls.Add(checkBoxRandomSize);
            grpShapes.Controls.Add(ButtonGeneratePattern);
            grpShapes.Controls.Add(ButtonGenerateCircles);
            grpShapes.Controls.Add(ButtonGenerateSquares);
            grpShapes.Controls.Add(ButtonGenerateTriangles);
            grpShapes.Controls.Add(ButtonGenerateDiamonds);
            grpShapes.Controls.Add(ButtonGenerateStars);
            grpShapes.Controls.Add(ButtonGenerateHexagons);
            grpShapes.Location = new System.Drawing.Point(300, 594);
            grpShapes.Name = "grpShapes";
            grpShapes.Size = new System.Drawing.Size(370, 146);
            grpShapes.TabIndex = 16;
            grpShapes.TabStop = false;
            grpShapes.Text = "Shape Generation";

            // FIX: trackBarCount – Scroll + MouseUp registered
            trackBarCount.Location = new System.Drawing.Point(60, 15);
            trackBarCount.Maximum = 50;
            trackBarCount.Minimum = 1;
            trackBarCount.Name = "trackBarCount";
            trackBarCount.Size = new System.Drawing.Size(140, 45);
            trackBarCount.TabIndex = 0;
            trackBarCount.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            trackBarCount.Value = 5;
            trackBarCount.Scroll += TrackBarCount_Scroll;
            trackBarCount.MouseUp += TrackBarCount_MouseUp;   // FIX: added

            labelCount.AutoSize = true;
            labelCount.Location = new System.Drawing.Point(6, 20);
            labelCount.Name = "labelCount";
            labelCount.Size = new System.Drawing.Size(52, 15);
            labelCount.TabIndex = 1;
            labelCount.Text = "Count: 5";

            // FIX: trackBarSize – Scroll + MouseUp registered
            trackBarSize.Location = new System.Drawing.Point(60, 48);
            trackBarSize.Maximum = 60;
            trackBarSize.Minimum = 4;
            trackBarSize.Name = "trackBarSize";
            trackBarSize.Size = new System.Drawing.Size(140, 45);
            trackBarSize.TabIndex = 1;
            trackBarSize.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            trackBarSize.Value = 15;
            trackBarSize.Scroll += TrackBarSize_Scroll;
            trackBarSize.MouseUp += TrackBarSize_MouseUp;     // FIX: added

            labelSize.AutoSize = true;
            labelSize.Location = new System.Drawing.Point(6, 53);
            labelSize.Name = "labelSize";
            labelSize.Size = new System.Drawing.Size(45, 15);
            labelSize.TabIndex = 2;
            labelSize.Text = "Size: 15";

            // FIX: checkBoxRandomSize – CheckedChanged registered
            checkBoxRandomSize.AutoSize = true;
            checkBoxRandomSize.Location = new System.Drawing.Point(210, 18);
            checkBoxRandomSize.Name = "checkBoxRandomSize";
            checkBoxRandomSize.Size = new System.Drawing.Size(94, 19);
            checkBoxRandomSize.TabIndex = 2;
            checkBoxRandomSize.Text = "Random Size";
            checkBoxRandomSize.CheckedChanged += CheckBoxRandomSize_CheckedChanged;   // FIX: added

            ButtonGeneratePattern.Location = new System.Drawing.Point(210, 70);
            ButtonGeneratePattern.Name = "ButtonGeneratePattern";
            ButtonGeneratePattern.Size = new System.Drawing.Size(108, 23);
            ButtonGeneratePattern.TabIndex = 3;
            ButtonGeneratePattern.Text = "Generate Pattern";
            ButtonGeneratePattern.UseVisualStyleBackColor = true;
            ButtonGeneratePattern.Click += ButtonGeneratePattern_Click;

            ButtonGenerateCircles.Location = new System.Drawing.Point(210, 45);
            ButtonGenerateCircles.Name = "ButtonGenerateCircles";
            ButtonGenerateCircles.Size = new System.Drawing.Size(108, 23);
            ButtonGenerateCircles.TabIndex = 4;
            ButtonGenerateCircles.Text = "Generate Circles";
            ButtonGenerateCircles.UseVisualStyleBackColor = true;
            ButtonGenerateCircles.Click += ButtonGenerateCircles_Click;

            ButtonGenerateSquares.Location = new System.Drawing.Point(4, 99);
            ButtonGenerateSquares.Name = "ButtonGenerateSquares";
            ButtonGenerateSquares.Size = new System.Drawing.Size(75, 23);
            ButtonGenerateSquares.TabIndex = 5;
            ButtonGenerateSquares.Text = "Squares";
            ButtonGenerateSquares.UseVisualStyleBackColor = true;
            ButtonGenerateSquares.Click += ButtonGenerateSquares_Click;

            ButtonGenerateTriangles.Location = new System.Drawing.Point(80, 99);
            ButtonGenerateTriangles.Name = "ButtonGenerateTriangles";
            ButtonGenerateTriangles.Size = new System.Drawing.Size(75, 23);
            ButtonGenerateTriangles.TabIndex = 6;
            ButtonGenerateTriangles.Text = "Triangles";
            ButtonGenerateTriangles.UseVisualStyleBackColor = true;
            ButtonGenerateTriangles.Click += ButtonGenerateTriangles_Click;

            ButtonGenerateDiamonds.Location = new System.Drawing.Point(156, 99);
            ButtonGenerateDiamonds.Name = "ButtonGenerateDiamonds";
            ButtonGenerateDiamonds.Size = new System.Drawing.Size(75, 23);
            ButtonGenerateDiamonds.TabIndex = 7;
            ButtonGenerateDiamonds.Text = "Diamonds";
            ButtonGenerateDiamonds.UseVisualStyleBackColor = true;
            ButtonGenerateDiamonds.Click += ButtonGenerateDiamonds_Click;

            ButtonGenerateStars.Location = new System.Drawing.Point(232, 99);
            ButtonGenerateStars.Name = "ButtonGenerateStars";
            ButtonGenerateStars.Size = new System.Drawing.Size(41, 23);
            ButtonGenerateStars.TabIndex = 8;
            ButtonGenerateStars.Text = "Stars";
            ButtonGenerateStars.UseVisualStyleBackColor = true;
            ButtonGenerateStars.Click += ButtonGenerateStars_Click;

            ButtonGenerateHexagons.Location = new System.Drawing.Point(279, 99);
            ButtonGenerateHexagons.Name = "ButtonGenerateHexagons";
            ButtonGenerateHexagons.Size = new System.Drawing.Size(75, 23);
            ButtonGenerateHexagons.TabIndex = 9;
            ButtonGenerateHexagons.Text = "Hexagons";
            ButtonGenerateHexagons.UseVisualStyleBackColor = true;
            ButtonGenerateHexagons.Click += ButtonGenerateHexagons_Click;

            // ──────────────────────────────────────────────────────────────────────
            // TextureColorForm
            // ──────────────────────────────────────────────────────────────────────
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(686, 749);
            Controls.Add(lbIDNumber);
            Controls.Add(panelPictureBoxHost);
            Controls.Add(ButtonPrevious);
            Controls.Add(ButtonNext);
            Controls.Add(grpHueShift);
            Controls.Add(grpAdjust);
            Controls.Add(grpColorInfo);
            Controls.Add(grpEffects);
            Controls.Add(grpIO);
            Controls.Add(grpSelection);
            Controls.Add(grpShapes);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureColorForm";
            Text = "UO Texture Painter — Color & Pattern Editor";
            ((System.ComponentModel.ISupportInitialize)PictureBoxImageColor).EndInit();
            panelPictureBoxHost.ResumeLayout(false);
            grpHueShift.ResumeLayout(false);
            grpHueShift.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TrackBarColor).EndInit();
            grpAdjust.ResumeLayout(false);
            grpAdjust.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSaturation).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarBrightness).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarContrast).EndInit();
            grpColorInfo.ResumeLayout(false);
            grpColorInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericTolerance).EndInit();
            grpEffects.ResumeLayout(false);
            grpIO.ResumeLayout(false);
            grpSelection.ResumeLayout(false);
            grpSelection.PerformLayout();
            grpShapes.ResumeLayout(false);
            grpShapes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarSize).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        #region Control Declarations

        private System.Windows.Forms.PictureBox PictureBoxImageColor;
        private System.Windows.Forms.Panel panelPictureBoxHost;
        private System.Windows.Forms.Label lbIDNumber;

        private System.Windows.Forms.Button ButtonPrevious;
        private System.Windows.Forms.Button ButtonNext;

        private System.Windows.Forms.GroupBox grpHueShift;
        private System.Windows.Forms.TrackBar TrackBarColor;
        private System.Windows.Forms.Label labelColorShift;
        private System.Windows.Forms.Button ButtonSavePosition;
        private System.Windows.Forms.Button ButtonLoadPosition;

        private System.Windows.Forms.GroupBox grpAdjust;
        private System.Windows.Forms.TrackBar trackBarSaturation;
        private System.Windows.Forms.Label labelSaturation;
        private System.Windows.Forms.TrackBar trackBarBrightness;
        private System.Windows.Forms.Label labelBrightness;
        private System.Windows.Forms.TrackBar trackBarContrast;
        private System.Windows.Forms.Label labelContrast;

        private System.Windows.Forms.GroupBox grpColorInfo;
        private System.Windows.Forms.Panel panelColor;
        private System.Windows.Forms.Label labelColorValues;
        private System.Windows.Forms.TextBox tbColorCode;
        private System.Windows.Forms.Button BtColorPincers;
        private System.Windows.Forms.Button BtExchangeSelectiveColors;
        private System.Windows.Forms.NumericUpDown numericTolerance;
        private System.Windows.Forms.Label labelTolerance;

        private System.Windows.Forms.GroupBox grpEffects;
        private System.Windows.Forms.Button BtnGrayscale;
        private System.Windows.Forms.Button BtnSepia;
        private System.Windows.Forms.Button BtnInvert;
        private System.Windows.Forms.Button BtnFlipH;
        private System.Windows.Forms.Button BtnFlipV;
        private System.Windows.Forms.Button BtnRotate90;
        private System.Windows.Forms.Button BtnResetImage;
        private System.Windows.Forms.Button BtnUndo;

        private System.Windows.Forms.GroupBox grpIO;
        private System.Windows.Forms.Button ButtonCopyToClipboard;
        private System.Windows.Forms.Button BtnImportClipboard;
        private System.Windows.Forms.Button ButtonSaveToFile;
        private System.Windows.Forms.Button BtnLoadImage;

        private System.Windows.Forms.GroupBox grpSelection;
        private System.Windows.Forms.Button SavePatternButton;
        private System.Windows.Forms.Button RestorePatternButton;
        private System.Windows.Forms.Button BtnClear;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button LoadButton;
        private System.Windows.Forms.CheckBox checkBoxKeepPreviousPattern;
        private System.Windows.Forms.Button ButtonChangeCursorColor;

        private System.Windows.Forms.GroupBox grpShapes;
        private System.Windows.Forms.TrackBar trackBarCount;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.TrackBar trackBarSize;
        private System.Windows.Forms.Label labelSize;
        private System.Windows.Forms.CheckBox checkBoxRandomSize;
        private System.Windows.Forms.Button ButtonGeneratePattern;
        private System.Windows.Forms.Button ButtonGenerateCircles;
        private System.Windows.Forms.Button ButtonGenerateSquares;
        private System.Windows.Forms.Button ButtonGenerateTriangles;
        private System.Windows.Forms.Button ButtonGenerateDiamonds;
        private System.Windows.Forms.Button ButtonGenerateStars;
        private System.Windows.Forms.Button ButtonGenerateHexagons;

        #endregion
    }
}
/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class AnimationStudioForm
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
            components = new System.ComponentModel.Container();
            imageListThumbs = new System.Windows.Forms.ImageList(components);
            tableCompass = new System.Windows.Forms.TableLayoutPanel();
            viewUpLeft = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewUp = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewUpRight = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewLeft = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewSelected = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewRight = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewDownLeft = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewDown = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewDownRight = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            pnlLeft = new System.Windows.Forms.Panel();
            lblDirections = new System.Windows.Forms.Label();
            lbDirections = new System.Windows.Forms.ListBox();
            btnAddImages = new System.Windows.Forms.Button();
            btnImportSheet = new System.Windows.Forms.Button();
            btnLoadMul = new System.Windows.Forms.Button();
            btnMirror = new System.Windows.Forms.Button();
            btnFillEmpty = new System.Windows.Forms.Button();
            btnFrameLeft = new System.Windows.Forms.Button();
            btnFrameRight = new System.Windows.Forms.Button();
            btnRemoveFrames = new System.Windows.Forms.Button();
            btnClearDirection = new System.Windows.Forms.Button();
            lvFrames = new System.Windows.Forms.ListView();
            grpPreview = new System.Windows.Forms.GroupBox();
            lblDelay = new System.Windows.Forms.Label();
            numDelay = new System.Windows.Forms.NumericUpDown();
            lblZoom = new System.Windows.Forms.Label();
            numZoom = new System.Windows.Forms.NumericUpDown();
            chkPlay = new System.Windows.Forms.CheckBox();
            chkCrosshair = new System.Windows.Forms.CheckBox();
            grpCenter = new System.Windows.Forms.GroupBox();
            lblCenterX = new System.Windows.Forms.Label();
            numCenterX = new System.Windows.Forms.NumericUpDown();
            lblCenterY = new System.Windows.Forms.Label();
            numCenterY = new System.Windows.Forms.NumericUpDown();
            lblCenterHint = new System.Windows.Forms.Label();
            grpExport = new System.Windows.Forms.GroupBox();
            lblFileType = new System.Windows.Forms.Label();
            cbFileType = new System.Windows.Forms.ComboBox();
            lblColors = new System.Windows.Forms.Label();
            numColors = new System.Windows.Forms.NumericUpDown();
            lblBody = new System.Windows.Forms.Label();
            numBody = new System.Windows.Forms.NumericUpDown();
            lblAction = new System.Windows.Forms.Label();
            numAction = new System.Windows.Forms.NumericUpDown();
            lblActionHint = new System.Windows.Forms.Label();
            btnExportMul = new System.Windows.Forms.Button();
            btnSaveMul = new System.Windows.Forms.Button();
            btnExportPng = new System.Windows.Forms.Button();
            btnExportSheet = new System.Windows.Forms.Button();
            lblBodyHint = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            timerPlay = new System.Windows.Forms.Timer(components);
            tableCompass.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZoom).BeginInit();
            grpCenter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCenterX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCenterY).BeginInit();
            grpExport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numColors).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBody).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numAction).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // imageListThumbs
            // 
            imageListThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageListThumbs.ImageSize = new System.Drawing.Size(48, 48);
            imageListThumbs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // tableCompass
            // 
            tableCompass.Controls.Add(viewUpLeft, 0, 0);
            tableCompass.Controls.Add(viewUp, 1, 0);
            tableCompass.Controls.Add(viewUpRight, 2, 0);
            tableCompass.Controls.Add(viewLeft, 0, 1);
            tableCompass.Controls.Add(viewSelected, 1, 1);
            tableCompass.Controls.Add(viewRight, 2, 1);
            tableCompass.Controls.Add(viewDownLeft, 0, 2);
            tableCompass.Controls.Add(viewDown, 1, 2);
            tableCompass.Controls.Add(viewDownRight, 2, 2);
            tableCompass.ColumnCount = 3;
            tableCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            tableCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            tableCompass.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            tableCompass.Dock = System.Windows.Forms.DockStyle.Fill;
            tableCompass.Location = new System.Drawing.Point(380, 0);
            tableCompass.RowCount = 3;
            tableCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            tableCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            tableCompass.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            tableCompass.Size = new System.Drawing.Size(844, 739);
            tableCompass.Name = "tableCompass";
            tableCompass.TabIndex = 0;
            // 
            // viewUpLeft
            // 
            viewUpLeft.Caption = "↖ 3";
            viewUpLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            viewUpLeft.Margin = new System.Windows.Forms.Padding(2);
            viewUpLeft.Zoom = 1;
            viewUpLeft.Size = new System.Drawing.Size(277, 242);
            viewUpLeft.Name = "viewUpLeft";
            viewUpLeft.TabIndex = 1;
            viewUpLeft.Click += CompassView_Click;
            // 
            // viewUp
            // 
            viewUp.Caption = "↑ 4";
            viewUp.Dock = System.Windows.Forms.DockStyle.Fill;
            viewUp.Margin = new System.Windows.Forms.Padding(2);
            viewUp.Zoom = 1;
            viewUp.Size = new System.Drawing.Size(277, 242);
            viewUp.Name = "viewUp";
            viewUp.TabIndex = 2;
            viewUp.Click += CompassView_Click;
            // 
            // viewUpRight
            // 
            viewUpRight.Caption = "↗ 3 gespiegelt";
            viewUpRight.Dock = System.Windows.Forms.DockStyle.Fill;
            viewUpRight.Margin = new System.Windows.Forms.Padding(2);
            viewUpRight.Zoom = 1;
            viewUpRight.Size = new System.Drawing.Size(277, 242);
            viewUpRight.Name = "viewUpRight";
            viewUpRight.TabIndex = 3;
            viewUpRight.Click += CompassView_Click;
            // 
            // viewLeft
            // 
            viewLeft.Caption = "← 2";
            viewLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            viewLeft.Margin = new System.Windows.Forms.Padding(2);
            viewLeft.Zoom = 1;
            viewLeft.Size = new System.Drawing.Size(277, 242);
            viewLeft.Name = "viewLeft";
            viewLeft.TabIndex = 4;
            viewLeft.Click += CompassView_Click;
            // 
            // viewSelected
            // 
            viewSelected.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            viewSelected.Caption = "Auswahl";
            viewSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            viewSelected.Margin = new System.Windows.Forms.Padding(2);
            viewSelected.Zoom = 1;
            viewSelected.Size = new System.Drawing.Size(277, 242);
            viewSelected.Name = "viewSelected";
            viewSelected.TabIndex = 5;
            // 
            // viewRight
            // 
            viewRight.Caption = "→ 2 gespiegelt";
            viewRight.Dock = System.Windows.Forms.DockStyle.Fill;
            viewRight.Margin = new System.Windows.Forms.Padding(2);
            viewRight.Zoom = 1;
            viewRight.Size = new System.Drawing.Size(277, 242);
            viewRight.Name = "viewRight";
            viewRight.TabIndex = 6;
            viewRight.Click += CompassView_Click;
            // 
            // viewDownLeft
            // 
            viewDownLeft.Caption = "↙ 1";
            viewDownLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            viewDownLeft.Margin = new System.Windows.Forms.Padding(2);
            viewDownLeft.Zoom = 1;
            viewDownLeft.Size = new System.Drawing.Size(277, 242);
            viewDownLeft.Name = "viewDownLeft";
            viewDownLeft.TabIndex = 7;
            viewDownLeft.Click += CompassView_Click;
            // 
            // viewDown
            // 
            viewDown.Caption = "↓ 0";
            viewDown.Dock = System.Windows.Forms.DockStyle.Fill;
            viewDown.Margin = new System.Windows.Forms.Padding(2);
            viewDown.Zoom = 1;
            viewDown.Size = new System.Drawing.Size(277, 242);
            viewDown.Name = "viewDown";
            viewDown.TabIndex = 8;
            viewDown.Click += CompassView_Click;
            // 
            // viewDownRight
            // 
            viewDownRight.Caption = "↘ 1 gespiegelt";
            viewDownRight.Dock = System.Windows.Forms.DockStyle.Fill;
            viewDownRight.Margin = new System.Windows.Forms.Padding(2);
            viewDownRight.Zoom = 1;
            viewDownRight.Size = new System.Drawing.Size(277, 242);
            viewDownRight.Name = "viewDownRight";
            viewDownRight.TabIndex = 9;
            viewDownRight.Click += CompassView_Click;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(lblDirections);
            pnlLeft.Controls.Add(lbDirections);
            pnlLeft.Controls.Add(btnAddImages);
            pnlLeft.Controls.Add(btnImportSheet);
            pnlLeft.Controls.Add(btnLoadMul);
            pnlLeft.Controls.Add(btnMirror);
            pnlLeft.Controls.Add(btnFillEmpty);
            pnlLeft.Controls.Add(btnFrameLeft);
            pnlLeft.Controls.Add(btnFrameRight);
            pnlLeft.Controls.Add(btnRemoveFrames);
            pnlLeft.Controls.Add(btnClearDirection);
            pnlLeft.Controls.Add(lvFrames);
            pnlLeft.Controls.Add(grpPreview);
            pnlLeft.Controls.Add(grpCenter);
            pnlLeft.Controls.Add(grpExport);
            pnlLeft.AutoScroll = true;
            pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            pnlLeft.Location = new System.Drawing.Point(0, 0);
            pnlLeft.Size = new System.Drawing.Size(380, 739);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.TabIndex = 10;
            // 
            // lblDirections
            // 
            lblDirections.AutoSize = true;
            lblDirections.Location = new System.Drawing.Point(6, 9);
            lblDirections.Text = "Richtungen (so speichert UO sie):";
            lblDirections.Name = "lblDirections";
            // 
            // lbDirections
            // 
            lbDirections.FormattingEnabled = true;
            lbDirections.IntegralHeight = false;
            lbDirections.ItemHeight = 15;
            lbDirections.Location = new System.Drawing.Point(6, 28);
            lbDirections.Size = new System.Drawing.Size(360, 84);
            lbDirections.Name = "lbDirections";
            lbDirections.TabIndex = 11;
            lbDirections.SelectedIndexChanged += LbDirections_SelectedIndexChanged;
            // 
            // btnAddImages
            // 
            btnAddImages.Location = new System.Drawing.Point(6, 118);
            btnAddImages.Size = new System.Drawing.Size(176, 26);
            btnAddImages.Text = "Bilder hinzufügen...";
            btnAddImages.UseVisualStyleBackColor = true;
            btnAddImages.Name = "btnAddImages";
            btnAddImages.TabIndex = 12;
            btnAddImages.Click += BtnAddImages_Click;
            // 
            // btnImportSheet
            // 
            btnImportSheet.Location = new System.Drawing.Point(188, 118);
            btnImportSheet.Size = new System.Drawing.Size(178, 26);
            btnImportSheet.Text = "Sprite-Sheet importieren...";
            btnImportSheet.UseVisualStyleBackColor = true;
            btnImportSheet.Name = "btnImportSheet";
            btnImportSheet.TabIndex = 13;
            btnImportSheet.Click += BtnImportSheet_Click;
            // 
            // btnLoadMul
            // 
            btnLoadMul.Location = new System.Drawing.Point(6, 148);
            btnLoadMul.Size = new System.Drawing.Size(116, 26);
            btnLoadMul.Text = "Aus MUL laden";
            btnLoadMul.UseVisualStyleBackColor = true;
            btnLoadMul.Name = "btnLoadMul";
            btnLoadMul.TabIndex = 14;
            btnLoadMul.Click += BtnLoadMul_Click;
            // 
            // btnMirror
            // 
            btnMirror.Location = new System.Drawing.Point(126, 148);
            btnMirror.Size = new System.Drawing.Size(118, 26);
            btnMirror.Text = "Richtung spiegeln";
            btnMirror.UseVisualStyleBackColor = true;
            btnMirror.Name = "btnMirror";
            btnMirror.TabIndex = 15;
            btnMirror.Click += BtnMirror_Click;
            // 
            // btnFillEmpty
            // 
            btnFillEmpty.Location = new System.Drawing.Point(248, 148);
            btnFillEmpty.Size = new System.Drawing.Size(118, 26);
            btnFillEmpty.Text = "Leere auffüllen";
            btnFillEmpty.UseVisualStyleBackColor = true;
            btnFillEmpty.Name = "btnFillEmpty";
            btnFillEmpty.TabIndex = 16;
            btnFillEmpty.Click += BtnFillEmpty_Click;
            // 
            // btnFrameLeft
            // 
            btnFrameLeft.Location = new System.Drawing.Point(6, 178);
            btnFrameLeft.Size = new System.Drawing.Size(70, 26);
            btnFrameLeft.Text = "◀ Frame";
            btnFrameLeft.UseVisualStyleBackColor = true;
            btnFrameLeft.Name = "btnFrameLeft";
            btnFrameLeft.TabIndex = 17;
            btnFrameLeft.Click += BtnFrameLeft_Click;
            // 
            // btnFrameRight
            // 
            btnFrameRight.Location = new System.Drawing.Point(80, 178);
            btnFrameRight.Size = new System.Drawing.Size(70, 26);
            btnFrameRight.Text = "Frame ▶";
            btnFrameRight.UseVisualStyleBackColor = true;
            btnFrameRight.Name = "btnFrameRight";
            btnFrameRight.TabIndex = 18;
            btnFrameRight.Click += BtnFrameRight_Click;
            // 
            // btnRemoveFrames
            // 
            btnRemoveFrames.Location = new System.Drawing.Point(154, 178);
            btnRemoveFrames.Size = new System.Drawing.Size(80, 26);
            btnRemoveFrames.Text = "Entfernen";
            btnRemoveFrames.UseVisualStyleBackColor = true;
            btnRemoveFrames.Name = "btnRemoveFrames";
            btnRemoveFrames.TabIndex = 19;
            btnRemoveFrames.Click += BtnRemoveFrames_Click;
            // 
            // btnClearDirection
            // 
            btnClearDirection.Location = new System.Drawing.Point(238, 178);
            btnClearDirection.Size = new System.Drawing.Size(128, 26);
            btnClearDirection.Text = "Richtung leeren";
            btnClearDirection.UseVisualStyleBackColor = true;
            btnClearDirection.Name = "btnClearDirection";
            btnClearDirection.TabIndex = 20;
            btnClearDirection.Click += BtnClearDirection_Click;
            // 
            // lvFrames
            // 
            lvFrames.HideSelection = false;
            lvFrames.LargeImageList = imageListThumbs;
            lvFrames.Location = new System.Drawing.Point(6, 210);
            lvFrames.Size = new System.Drawing.Size(360, 150);
            lvFrames.UseCompatibleStateImageBehavior = false;
            lvFrames.Name = "lvFrames";
            lvFrames.TabIndex = 21;
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(lblDelay);
            grpPreview.Controls.Add(numDelay);
            grpPreview.Controls.Add(lblZoom);
            grpPreview.Controls.Add(numZoom);
            grpPreview.Controls.Add(chkPlay);
            grpPreview.Controls.Add(chkCrosshair);
            grpPreview.Location = new System.Drawing.Point(4, 366);
            grpPreview.Size = new System.Drawing.Size(362, 76);
            grpPreview.TabStop = false;
            grpPreview.Text = "Vorschau";
            grpPreview.Name = "grpPreview";
            grpPreview.TabIndex = 22;
            // 
            // lblDelay
            // 
            lblDelay.AutoSize = true;
            lblDelay.Location = new System.Drawing.Point(8, 23);
            lblDelay.Text = "ms/Frame:";
            lblDelay.Name = "lblDelay";
            // 
            // numDelay
            // 
            numDelay.Location = new System.Drawing.Point(76, 20);
            numDelay.Size = new System.Drawing.Size(60, 23);
            numDelay.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numDelay.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numDelay.Value = new decimal(new int[] { 120, 0, 0, 0 });
            numDelay.Name = "numDelay";
            numDelay.TabIndex = 23;
            numDelay.ValueChanged += NumDelay_ValueChanged;
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = true;
            lblZoom.Location = new System.Drawing.Point(144, 23);
            lblZoom.Text = "Zoom:";
            lblZoom.Name = "lblZoom";
            // 
            // numZoom
            // 
            numZoom.Location = new System.Drawing.Point(186, 20);
            numZoom.Size = new System.Drawing.Size(50, 23);
            numZoom.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numZoom.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numZoom.Name = "numZoom";
            numZoom.TabIndex = 24;
            numZoom.ValueChanged += NumZoom_ValueChanged;
            // 
            // chkPlay
            // 
            chkPlay.Checked = true;
            chkPlay.CheckState = System.Windows.Forms.CheckState.Checked;
            chkPlay.AutoSize = true;
            chkPlay.Location = new System.Drawing.Point(248, 22);
            chkPlay.Text = "Abspielen";
            chkPlay.UseVisualStyleBackColor = true;
            chkPlay.Name = "chkPlay";
            chkPlay.TabIndex = 25;
            chkPlay.CheckedChanged += ChkPlay_CheckedChanged;
            // 
            // chkCrosshair
            // 
            chkCrosshair.Checked = true;
            chkCrosshair.CheckState = System.Windows.Forms.CheckState.Checked;
            chkCrosshair.AutoSize = true;
            chkCrosshair.Location = new System.Drawing.Point(8, 48);
            chkCrosshair.Text = "Fußpunkt anzeigen";
            chkCrosshair.UseVisualStyleBackColor = true;
            chkCrosshair.Name = "chkCrosshair";
            chkCrosshair.TabIndex = 26;
            chkCrosshair.CheckedChanged += PreviewSetting_Changed;
            // 
            // grpCenter
            // 
            grpCenter.Controls.Add(lblCenterX);
            grpCenter.Controls.Add(numCenterX);
            grpCenter.Controls.Add(lblCenterY);
            grpCenter.Controls.Add(numCenterY);
            grpCenter.Controls.Add(lblCenterHint);
            grpCenter.Location = new System.Drawing.Point(4, 448);
            grpCenter.Size = new System.Drawing.Size(362, 52);
            grpCenter.TabStop = false;
            grpCenter.Text = "Fußpunkt (Center) relativ zu unten-Mitte";
            grpCenter.Name = "grpCenter";
            grpCenter.TabIndex = 27;
            // 
            // lblCenterX
            // 
            lblCenterX.AutoSize = true;
            lblCenterX.Location = new System.Drawing.Point(8, 23);
            lblCenterX.Text = "X:";
            lblCenterX.Name = "lblCenterX";
            // 
            // numCenterX
            // 
            numCenterX.Location = new System.Drawing.Point(28, 20);
            numCenterX.Size = new System.Drawing.Size(60, 23);
            numCenterX.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numCenterX.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648 });
            numCenterX.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numCenterX.Name = "numCenterX";
            numCenterX.TabIndex = 28;
            numCenterX.ValueChanged += PreviewSetting_Changed;
            // 
            // lblCenterY
            // 
            lblCenterY.AutoSize = true;
            lblCenterY.Location = new System.Drawing.Point(100, 23);
            lblCenterY.Text = "Y:";
            lblCenterY.Name = "lblCenterY";
            // 
            // numCenterY
            // 
            numCenterY.Location = new System.Drawing.Point(120, 20);
            numCenterY.Size = new System.Drawing.Size(60, 23);
            numCenterY.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numCenterY.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648 });
            numCenterY.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numCenterY.Name = "numCenterY";
            numCenterY.TabIndex = 29;
            numCenterY.ValueChanged += PreviewSetting_Changed;
            // 
            // lblCenterHint
            // 
            lblCenterHint.AutoSize = true;
            lblCenterHint.Location = new System.Drawing.Point(190, 23);
            lblCenterHint.Text = "(Y positiv = höher)";
            lblCenterHint.Name = "lblCenterHint";
            // 
            // grpExport
            // 
            grpExport.Controls.Add(lblFileType);
            grpExport.Controls.Add(cbFileType);
            grpExport.Controls.Add(lblColors);
            grpExport.Controls.Add(numColors);
            grpExport.Controls.Add(lblBody);
            grpExport.Controls.Add(numBody);
            grpExport.Controls.Add(lblAction);
            grpExport.Controls.Add(numAction);
            grpExport.Controls.Add(lblActionHint);
            grpExport.Controls.Add(btnExportMul);
            grpExport.Controls.Add(btnSaveMul);
            grpExport.Controls.Add(btnExportPng);
            grpExport.Controls.Add(btnExportSheet);
            grpExport.Controls.Add(lblBodyHint);
            grpExport.Location = new System.Drawing.Point(4, 506);
            grpExport.Size = new System.Drawing.Size(362, 170);
            grpExport.TabStop = false;
            grpExport.Text = "Export in Anim-MUL";
            grpExport.Name = "grpExport";
            grpExport.TabIndex = 30;
            // 
            // lblFileType
            // 
            lblFileType.AutoSize = true;
            lblFileType.Location = new System.Drawing.Point(8, 25);
            lblFileType.Text = "Datei:";
            lblFileType.Name = "lblFileType";
            // 
            // cbFileType
            // 
            cbFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbFileType.FormattingEnabled = true;
            cbFileType.Items.AddRange(new object[] { "anim.mul", "anim2.mul", "anim3.mul", "anim4.mul", "anim5.mul" });
            cbFileType.Location = new System.Drawing.Point(60, 22);
            cbFileType.Size = new System.Drawing.Size(110, 23);
            cbFileType.Name = "cbFileType";
            cbFileType.TabIndex = 31;
            // 
            // lblColors
            // 
            lblColors.AutoSize = true;
            lblColors.Location = new System.Drawing.Point(180, 25);
            lblColors.Text = "Farben:";
            lblColors.Name = "lblColors";
            // 
            // numColors
            // 
            numColors.Location = new System.Drawing.Point(240, 22);
            numColors.Size = new System.Drawing.Size(60, 23);
            numColors.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numColors.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numColors.Value = new decimal(new int[] { 256, 0, 0, 0 });
            numColors.Name = "numColors";
            numColors.TabIndex = 32;
            // 
            // lblBody
            // 
            lblBody.AutoSize = true;
            lblBody.Location = new System.Drawing.Point(8, 55);
            lblBody.Text = "Body:";
            lblBody.Name = "lblBody";
            // 
            // numBody
            // 
            numBody.Location = new System.Drawing.Point(60, 52);
            numBody.Size = new System.Drawing.Size(70, 23);
            numBody.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            numBody.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numBody.Value = new decimal(new int[] { 400, 0, 0, 0 });
            numBody.Name = "numBody";
            numBody.TabIndex = 33;
            // 
            // lblAction
            // 
            lblAction.AutoSize = true;
            lblAction.Location = new System.Drawing.Point(140, 55);
            lblAction.Text = "Action:";
            lblAction.Name = "lblAction";
            // 
            // numAction
            // 
            numAction.Location = new System.Drawing.Point(190, 52);
            numAction.Size = new System.Drawing.Size(50, 23);
            numAction.Maximum = new decimal(new int[] { 80, 0, 0, 0 });
            numAction.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numAction.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numAction.Name = "numAction";
            numAction.TabIndex = 34;
            // 
            // lblActionHint
            // 
            lblActionHint.AutoSize = true;
            lblActionHint.Location = new System.Drawing.Point(244, 55);
            lblActionHint.Text = "0 Walk, 1 Run/Stand, 2 Idle…";
            lblActionHint.Name = "lblActionHint";
            // 
            // btnExportMul
            // 
            btnExportMul.Location = new System.Drawing.Point(8, 82);
            btnExportMul.Size = new System.Drawing.Size(170, 26);
            btnExportMul.Text = "In Animation schreiben";
            btnExportMul.UseVisualStyleBackColor = true;
            btnExportMul.Name = "btnExportMul";
            btnExportMul.TabIndex = 35;
            btnExportMul.Click += BtnExportMul_Click;
            // 
            // btnSaveMul
            // 
            btnSaveMul.Location = new System.Drawing.Point(184, 82);
            btnSaveMul.Size = new System.Drawing.Size(170, 26);
            btnSaveMul.Text = "Anim-MUL speichern";
            btnSaveMul.UseVisualStyleBackColor = true;
            btnSaveMul.Name = "btnSaveMul";
            btnSaveMul.TabIndex = 36;
            btnSaveMul.Click += BtnSaveMul_Click;
            // 
            // btnExportPng
            // 
            btnExportPng.Location = new System.Drawing.Point(8, 112);
            btnExportPng.Size = new System.Drawing.Size(170, 26);
            btnExportPng.Text = "Frames als PNG...";
            btnExportPng.UseVisualStyleBackColor = true;
            btnExportPng.Name = "btnExportPng";
            btnExportPng.TabIndex = 37;
            btnExportPng.Click += BtnExportPng_Click;
            // 
            // btnExportSheet
            // 
            btnExportSheet.Location = new System.Drawing.Point(184, 112);
            btnExportSheet.Size = new System.Drawing.Size(170, 26);
            btnExportSheet.Text = "Sprite-Sheet als PNG...";
            btnExportSheet.UseVisualStyleBackColor = true;
            btnExportSheet.Name = "btnExportSheet";
            btnExportSheet.TabIndex = 38;
            btnExportSheet.Click += BtnExportSheet_Click;
            // 
            // lblBodyHint
            // 
            lblBodyHint.AutoSize = true;
            lblBodyHint.Location = new System.Drawing.Point(8, 145);
            lblBodyHint.Text = "anim.mul: Body <200 Monster (22 Actions), <400 Tiere (13), ab 400 Menschen (35)";
            lblBodyHint.Name = "lblBodyHint";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 39;
            // 
            // lblStatus
            // 
            lblStatus.Text = "Bilder oder ein Sprite-Sheet importieren.";
            lblStatus.Name = "lblStatus";
            // 
            // timerPlay
            // 
            timerPlay.Interval = 120;
            timerPlay.Tick += TimerPlay_Tick;
            // 
            // AnimationStudioForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1224, 761);
            Controls.Add(tableCompass);
            Controls.Add(pnlLeft);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(1000, 650);
            Name = "AnimationStudioForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Animations-Studio (Mobile-Animationen, 5 Richtungen + Spiegeln)";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numAction).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBody).EndInit();
            ((System.ComponentModel.ISupportInitialize)numColors).EndInit();
            grpExport.ResumeLayout(false);
            grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCenterY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCenterX).EndInit();
            grpCenter.ResumeLayout(false);
            grpCenter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numZoom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDelay).EndInit();
            grpPreview.ResumeLayout(false);
            grpPreview.PerformLayout();
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            tableCompass.ResumeLayout(false);
            tableCompass.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ImageList imageListThumbs;
        private System.Windows.Forms.TableLayoutPanel tableCompass;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewUpLeft;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewUp;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewUpRight;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewLeft;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewSelected;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewRight;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewDownLeft;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewDown;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewDownRight;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Label lblDirections;
        private System.Windows.Forms.ListBox lbDirections;
        private System.Windows.Forms.Button btnAddImages;
        private System.Windows.Forms.Button btnImportSheet;
        private System.Windows.Forms.Button btnLoadMul;
        private System.Windows.Forms.Button btnMirror;
        private System.Windows.Forms.Button btnFillEmpty;
        private System.Windows.Forms.Button btnFrameLeft;
        private System.Windows.Forms.Button btnFrameRight;
        private System.Windows.Forms.Button btnRemoveFrames;
        private System.Windows.Forms.Button btnClearDirection;
        private System.Windows.Forms.ListView lvFrames;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown numDelay;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.NumericUpDown numZoom;
        private System.Windows.Forms.CheckBox chkPlay;
        private System.Windows.Forms.CheckBox chkCrosshair;
        private System.Windows.Forms.GroupBox grpCenter;
        private System.Windows.Forms.Label lblCenterX;
        private System.Windows.Forms.NumericUpDown numCenterX;
        private System.Windows.Forms.Label lblCenterY;
        private System.Windows.Forms.NumericUpDown numCenterY;
        private System.Windows.Forms.Label lblCenterHint;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.ComboBox cbFileType;
        private System.Windows.Forms.Label lblColors;
        private System.Windows.Forms.NumericUpDown numColors;
        private System.Windows.Forms.Label lblBody;
        private System.Windows.Forms.NumericUpDown numBody;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.NumericUpDown numAction;
        private System.Windows.Forms.Label lblActionHint;
        private System.Windows.Forms.Button btnExportMul;
        private System.Windows.Forms.Button btnSaveMul;
        private System.Windows.Forms.Button btnExportPng;
        private System.Windows.Forms.Button btnExportSheet;
        private System.Windows.Forms.Label lblBodyHint;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer timerPlay;
    }
}

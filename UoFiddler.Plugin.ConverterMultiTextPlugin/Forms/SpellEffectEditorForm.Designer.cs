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
    partial class SpellEffectEditorForm
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
            pnlRight = new System.Windows.Forms.Panel();
            viewEffect = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            lvFrames = new System.Windows.Forms.ListView();
            pnlLeft = new System.Windows.Forms.Panel();
            grpImport = new System.Windows.Forms.GroupBox();
            btnAddImages = new System.Windows.Forms.Button();
            btnImportSheet = new System.Windows.Forms.Button();
            btnLoadArt = new System.Windows.Forms.Button();
            btnRemove = new System.Windows.Forms.Button();
            btnClear = new System.Windows.Forms.Button();
            grpProcess = new System.Windows.Forms.GroupBox();
            chkBlack = new System.Windows.Forms.CheckBox();
            numBlack = new System.Windows.Forms.NumericUpDown();
            chkGray = new System.Windows.Forms.CheckBox();
            lblBrightness = new System.Windows.Forms.Label();
            numBrightness = new System.Windows.Forms.NumericUpDown();
            lblScale = new System.Windows.Forms.Label();
            numScale = new System.Windows.Forms.NumericUpDown();
            lblMaxSize = new System.Windows.Forms.Label();
            numMaxSize = new System.Windows.Forms.NumericUpDown();
            chkCrop = new System.Windows.Forms.CheckBox();
            chkPingPong = new System.Windows.Forms.CheckBox();
            grpPreview = new System.Windows.Forms.GroupBox();
            lblMs = new System.Windows.Forms.Label();
            numMs = new System.Windows.Forms.NumericUpDown();
            chkPlay = new System.Windows.Forms.CheckBox();
            lblZoom = new System.Windows.Forms.Label();
            numZoom = new System.Windows.Forms.NumericUpDown();
            chkAdditive = new System.Windows.Forms.CheckBox();
            chkGround = new System.Windows.Forms.CheckBox();
            numGroundId = new System.Windows.Forms.NumericUpDown();
            lblHue = new System.Windows.Forms.Label();
            numHue = new System.Windows.Forms.NumericUpDown();
            chkPartialHue = new System.Windows.Forms.CheckBox();
            picHueStrip = new System.Windows.Forms.PictureBox();
            grpExport = new System.Windows.Forms.GroupBox();
            lblStartId = new System.Windows.Forms.Label();
            txtStartId = new System.Windows.Forms.TextBox();
            btnFreeBlock = new System.Windows.Forms.Button();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            chkTranslucent = new System.Windows.Forms.CheckBox();
            btnExport = new System.Windows.Forms.Button();
            btnSaveArt = new System.Windows.Forms.Button();
            btnSaveTileData = new System.Windows.Forms.Button();
            btnExportPng = new System.Windows.Forms.Button();
            btnCopyScript = new System.Windows.Forms.Button();
            lblExportHint = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            timerPlay = new System.Windows.Forms.Timer(components);
            pnlRight.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpImport.SuspendLayout();
            grpProcess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBlack).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numBrightness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxSize).BeginInit();
            grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMs).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZoom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGroundId).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picHueStrip).BeginInit();
            grpExport.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // imageListThumbs
            // 
            imageListThumbs.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageListThumbs.ImageSize = new System.Drawing.Size(48, 48);
            imageListThumbs.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(viewEffect);
            pnlRight.Controls.Add(lvFrames);
            pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlRight.Location = new System.Drawing.Point(340, 0);
            pnlRight.Size = new System.Drawing.Size(824, 719);
            pnlRight.Name = "pnlRight";
            pnlRight.TabIndex = 0;
            // 
            // viewEffect
            // 
            viewEffect.DarkBackground = true;
            viewEffect.Dock = System.Windows.Forms.DockStyle.Fill;
            viewEffect.Location = new System.Drawing.Point(0, 0);
            viewEffect.Size = new System.Drawing.Size(824, 609);
            viewEffect.Name = "viewEffect";
            viewEffect.TabIndex = 1;
            // 
            // lvFrames
            // 
            lvFrames.Dock = System.Windows.Forms.DockStyle.Bottom;
            lvFrames.HideSelection = false;
            lvFrames.LargeImageList = imageListThumbs;
            lvFrames.Location = new System.Drawing.Point(0, 609);
            lvFrames.Size = new System.Drawing.Size(824, 110);
            lvFrames.UseCompatibleStateImageBehavior = false;
            lvFrames.Name = "lvFrames";
            lvFrames.TabIndex = 2;
            lvFrames.SelectedIndexChanged += LvFrames_SelectedIndexChanged;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(grpImport);
            pnlLeft.Controls.Add(grpProcess);
            pnlLeft.Controls.Add(grpPreview);
            pnlLeft.Controls.Add(grpExport);
            pnlLeft.AutoScroll = true;
            pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            pnlLeft.Location = new System.Drawing.Point(0, 0);
            pnlLeft.Size = new System.Drawing.Size(340, 719);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.TabIndex = 3;
            // 
            // grpImport
            // 
            grpImport.Controls.Add(btnAddImages);
            grpImport.Controls.Add(btnImportSheet);
            grpImport.Controls.Add(btnLoadArt);
            grpImport.Controls.Add(btnRemove);
            grpImport.Controls.Add(btnClear);
            grpImport.Location = new System.Drawing.Point(4, 6);
            grpImport.Size = new System.Drawing.Size(316, 84);
            grpImport.TabStop = false;
            grpImport.Text = "Import";
            grpImport.Name = "grpImport";
            grpImport.TabIndex = 4;
            // 
            // btnAddImages
            // 
            btnAddImages.Location = new System.Drawing.Point(8, 20);
            btnAddImages.Size = new System.Drawing.Size(150, 26);
            btnAddImages.Text = "Bilder / GIF hinzufügen...";
            btnAddImages.UseVisualStyleBackColor = true;
            btnAddImages.Name = "btnAddImages";
            btnAddImages.TabIndex = 5;
            btnAddImages.Click += BtnAddImages_Click;
            // 
            // btnImportSheet
            // 
            btnImportSheet.Location = new System.Drawing.Point(162, 20);
            btnImportSheet.Size = new System.Drawing.Size(146, 26);
            btnImportSheet.Text = "Sprite-Sheet...";
            btnImportSheet.UseVisualStyleBackColor = true;
            btnImportSheet.Name = "btnImportSheet";
            btnImportSheet.TabIndex = 6;
            btnImportSheet.Click += BtnImportSheet_Click;
            // 
            // btnLoadArt
            // 
            btnLoadArt.Location = new System.Drawing.Point(8, 50);
            btnLoadArt.Size = new System.Drawing.Size(98, 26);
            btnLoadArt.Text = "Aus Art laden";
            btnLoadArt.UseVisualStyleBackColor = true;
            btnLoadArt.Name = "btnLoadArt";
            btnLoadArt.TabIndex = 7;
            btnLoadArt.Click += BtnLoadArt_Click;
            // 
            // btnRemove
            // 
            btnRemove.Location = new System.Drawing.Point(110, 50);
            btnRemove.Size = new System.Drawing.Size(98, 26);
            btnRemove.Text = "Entfernen";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Name = "btnRemove";
            btnRemove.TabIndex = 8;
            btnRemove.Click += BtnRemove_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new System.Drawing.Point(212, 50);
            btnClear.Size = new System.Drawing.Size(96, 26);
            btnClear.Text = "Alle leeren";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Name = "btnClear";
            btnClear.TabIndex = 9;
            btnClear.Click += BtnClear_Click;
            // 
            // grpProcess
            // 
            grpProcess.Controls.Add(chkBlack);
            grpProcess.Controls.Add(numBlack);
            grpProcess.Controls.Add(chkGray);
            grpProcess.Controls.Add(lblBrightness);
            grpProcess.Controls.Add(numBrightness);
            grpProcess.Controls.Add(lblScale);
            grpProcess.Controls.Add(numScale);
            grpProcess.Controls.Add(lblMaxSize);
            grpProcess.Controls.Add(numMaxSize);
            grpProcess.Controls.Add(chkCrop);
            grpProcess.Controls.Add(chkPingPong);
            grpProcess.Location = new System.Drawing.Point(4, 96);
            grpProcess.Size = new System.Drawing.Size(316, 170);
            grpProcess.TabStop = false;
            grpProcess.Text = "Bearbeitung";
            grpProcess.Name = "grpProcess";
            grpProcess.TabIndex = 10;
            // 
            // chkBlack
            // 
            chkBlack.Checked = true;
            chkBlack.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBlack.AutoSize = true;
            chkBlack.Location = new System.Drawing.Point(8, 22);
            chkBlack.Text = "Schwarz → transparent, Schwelle:";
            chkBlack.UseVisualStyleBackColor = true;
            chkBlack.Name = "chkBlack";
            chkBlack.TabIndex = 11;
            chkBlack.CheckedChanged += Process_Changed;
            // 
            // numBlack
            // 
            numBlack.Location = new System.Drawing.Point(240, 20);
            numBlack.Size = new System.Drawing.Size(60, 23);
            numBlack.Maximum = new decimal(new int[] { 254, 0, 0, 0 });
            numBlack.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numBlack.Value = new decimal(new int[] { 16, 0, 0, 0 });
            numBlack.Name = "numBlack";
            numBlack.TabIndex = 12;
            numBlack.ValueChanged += Process_Changed;
            // 
            // chkGray
            // 
            chkGray.AutoSize = true;
            chkGray.Location = new System.Drawing.Point(8, 48);
            chkGray.Text = "Graustufen (voll färbbar per Hue)";
            chkGray.UseVisualStyleBackColor = true;
            chkGray.Name = "chkGray";
            chkGray.TabIndex = 13;
            chkGray.CheckedChanged += Process_Changed;
            // 
            // lblBrightness
            // 
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new System.Drawing.Point(8, 75);
            lblBrightness.Text = "Helligkeit:";
            lblBrightness.Name = "lblBrightness";
            // 
            // numBrightness
            // 
            numBrightness.Location = new System.Drawing.Point(80, 72);
            numBrightness.Size = new System.Drawing.Size(60, 23);
            numBrightness.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numBrightness.Minimum = new decimal(new int[] { 100, 0, 0, -2147483648 });
            numBrightness.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numBrightness.Name = "numBrightness";
            numBrightness.TabIndex = 14;
            numBrightness.ValueChanged += Process_Changed;
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(150, 75);
            lblScale.Text = "Größe %:";
            lblScale.Name = "lblScale";
            // 
            // numScale
            // 
            numScale.Location = new System.Drawing.Point(220, 72);
            numScale.Size = new System.Drawing.Size(60, 23);
            numScale.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            numScale.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            numScale.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numScale.Name = "numScale";
            numScale.TabIndex = 15;
            numScale.ValueChanged += Process_Changed;
            // 
            // lblMaxSize
            // 
            lblMaxSize.AutoSize = true;
            lblMaxSize.Location = new System.Drawing.Point(8, 103);
            lblMaxSize.Text = "Max. Kante px:";
            lblMaxSize.Name = "lblMaxSize";
            // 
            // numMaxSize
            // 
            numMaxSize.Location = new System.Drawing.Point(100, 100);
            numMaxSize.Size = new System.Drawing.Size(60, 23);
            numMaxSize.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numMaxSize.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numMaxSize.Value = new decimal(new int[] { 160, 0, 0, 0 });
            numMaxSize.Name = "numMaxSize";
            numMaxSize.TabIndex = 16;
            numMaxSize.ValueChanged += Process_Changed;
            // 
            // chkCrop
            // 
            chkCrop.Checked = true;
            chkCrop.CheckState = System.Windows.Forms.CheckState.Checked;
            chkCrop.AutoSize = true;
            chkCrop.Location = new System.Drawing.Point(170, 102);
            chkCrop.Text = "Gemeinsam zuschneiden";
            chkCrop.UseVisualStyleBackColor = true;
            chkCrop.Name = "chkCrop";
            chkCrop.TabIndex = 17;
            chkCrop.CheckedChanged += Process_Changed;
            // 
            // chkPingPong
            // 
            chkPingPong.AutoSize = true;
            chkPingPong.Location = new System.Drawing.Point(8, 130);
            chkPingPong.Text = "Ping-Pong (vor und zurück)";
            chkPingPong.UseVisualStyleBackColor = true;
            chkPingPong.Name = "chkPingPong";
            chkPingPong.TabIndex = 18;
            chkPingPong.CheckedChanged += Process_Changed;
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(lblMs);
            grpPreview.Controls.Add(numMs);
            grpPreview.Controls.Add(chkPlay);
            grpPreview.Controls.Add(lblZoom);
            grpPreview.Controls.Add(numZoom);
            grpPreview.Controls.Add(chkAdditive);
            grpPreview.Controls.Add(chkGround);
            grpPreview.Controls.Add(numGroundId);
            grpPreview.Controls.Add(lblHue);
            grpPreview.Controls.Add(numHue);
            grpPreview.Controls.Add(chkPartialHue);
            grpPreview.Controls.Add(picHueStrip);
            grpPreview.Location = new System.Drawing.Point(4, 272);
            grpPreview.Size = new System.Drawing.Size(316, 170);
            grpPreview.TabStop = false;
            grpPreview.Text = "Vorschau";
            grpPreview.Name = "grpPreview";
            grpPreview.TabIndex = 19;
            // 
            // lblMs
            // 
            lblMs.AutoSize = true;
            lblMs.Location = new System.Drawing.Point(8, 23);
            lblMs.Text = "ms/Frame:";
            lblMs.Name = "lblMs";
            // 
            // numMs
            // 
            numMs.Location = new System.Drawing.Point(80, 20);
            numMs.Size = new System.Drawing.Size(60, 23);
            numMs.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numMs.Minimum = new decimal(new int[] { 20, 0, 0, 0 });
            numMs.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numMs.Name = "numMs";
            numMs.TabIndex = 20;
            numMs.ValueChanged += NumMs_ValueChanged;
            // 
            // chkPlay
            // 
            chkPlay.Checked = true;
            chkPlay.CheckState = System.Windows.Forms.CheckState.Checked;
            chkPlay.AutoSize = true;
            chkPlay.Location = new System.Drawing.Point(150, 22);
            chkPlay.Text = "Abspielen";
            chkPlay.UseVisualStyleBackColor = true;
            chkPlay.Name = "chkPlay";
            chkPlay.TabIndex = 21;
            chkPlay.CheckedChanged += ChkPlay_CheckedChanged;
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = true;
            lblZoom.Location = new System.Drawing.Point(230, 23);
            lblZoom.Text = "Zoom:";
            lblZoom.Name = "lblZoom";
            // 
            // numZoom
            // 
            numZoom.Location = new System.Drawing.Point(272, 20);
            numZoom.Size = new System.Drawing.Size(36, 23);
            numZoom.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            numZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numZoom.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numZoom.Name = "numZoom";
            numZoom.TabIndex = 22;
            numZoom.ValueChanged += NumZoom_ValueChanged;
            // 
            // chkAdditive
            // 
            chkAdditive.Checked = true;
            chkAdditive.CheckState = System.Windows.Forms.CheckState.Checked;
            chkAdditive.AutoSize = true;
            chkAdditive.Location = new System.Drawing.Point(8, 48);
            chkAdditive.Text = "Additiv (wie leuchtende Client-Effekte)";
            chkAdditive.UseVisualStyleBackColor = true;
            chkAdditive.Name = "chkAdditive";
            chkAdditive.TabIndex = 23;
            chkAdditive.CheckedChanged += Preview_Changed;
            // 
            // chkGround
            // 
            chkGround.AutoSize = true;
            chkGround.Location = new System.Drawing.Point(8, 74);
            chkGround.Text = "Auf Bodentile, Land-ID:";
            chkGround.UseVisualStyleBackColor = true;
            chkGround.Name = "chkGround";
            chkGround.TabIndex = 24;
            chkGround.CheckedChanged += Ground_Changed;
            // 
            // numGroundId
            // 
            numGroundId.Location = new System.Drawing.Point(170, 72);
            numGroundId.Size = new System.Drawing.Size(70, 23);
            numGroundId.Maximum = new decimal(new int[] { 16383, 0, 0, 0 });
            numGroundId.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numGroundId.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numGroundId.Name = "numGroundId";
            numGroundId.TabIndex = 25;
            numGroundId.ValueChanged += Ground_Changed;
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new System.Drawing.Point(8, 103);
            lblHue.Text = "Hue:";
            lblHue.Name = "lblHue";
            // 
            // numHue
            // 
            numHue.Location = new System.Drawing.Point(50, 100);
            numHue.Size = new System.Drawing.Size(70, 23);
            numHue.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numHue.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Name = "numHue";
            numHue.TabIndex = 26;
            numHue.ValueChanged += NumHue_ValueChanged;
            // 
            // chkPartialHue
            // 
            chkPartialHue.AutoSize = true;
            chkPartialHue.Location = new System.Drawing.Point(128, 102);
            chkPartialHue.Text = "nur Grautöne (PartialHue)";
            chkPartialHue.UseVisualStyleBackColor = true;
            chkPartialHue.Name = "chkPartialHue";
            chkPartialHue.TabIndex = 27;
            chkPartialHue.CheckedChanged += Preview_Changed;
            // 
            // picHueStrip
            // 
            picHueStrip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picHueStrip.Location = new System.Drawing.Point(8, 132);
            picHueStrip.Size = new System.Drawing.Size(300, 14);
            picHueStrip.TabStop = false;
            picHueStrip.Name = "picHueStrip";
            picHueStrip.TabIndex = 28;
            // 
            // grpExport
            // 
            grpExport.Controls.Add(lblStartId);
            grpExport.Controls.Add(txtStartId);
            grpExport.Controls.Add(btnFreeBlock);
            grpExport.Controls.Add(lblName);
            grpExport.Controls.Add(txtName);
            grpExport.Controls.Add(chkTranslucent);
            grpExport.Controls.Add(btnExport);
            grpExport.Controls.Add(btnSaveArt);
            grpExport.Controls.Add(btnSaveTileData);
            grpExport.Controls.Add(btnExportPng);
            grpExport.Controls.Add(btnCopyScript);
            grpExport.Controls.Add(lblExportHint);
            grpExport.Location = new System.Drawing.Point(4, 448);
            grpExport.Size = new System.Drawing.Size(316, 228);
            grpExport.TabStop = false;
            grpExport.Text = "Export (Art + Tiledata + Animdata)";
            grpExport.Name = "grpExport";
            grpExport.TabIndex = 29;
            // 
            // lblStartId
            // 
            lblStartId.AutoSize = true;
            lblStartId.Location = new System.Drawing.Point(8, 23);
            lblStartId.Text = "Start-ID:";
            lblStartId.Name = "lblStartId";
            // 
            // txtStartId
            // 
            txtStartId.Location = new System.Drawing.Point(80, 20);
            txtStartId.Size = new System.Drawing.Size(80, 23);
            txtStartId.Text = "0x3E00";
            txtStartId.Name = "txtStartId";
            txtStartId.TabIndex = 30;
            // 
            // btnFreeBlock
            // 
            btnFreeBlock.Location = new System.Drawing.Point(166, 18);
            btnFreeBlock.Size = new System.Drawing.Size(140, 26);
            btnFreeBlock.Text = "Freien Block";
            btnFreeBlock.UseVisualStyleBackColor = true;
            btnFreeBlock.Name = "btnFreeBlock";
            btnFreeBlock.TabIndex = 31;
            btnFreeBlock.Click += BtnFreeBlock_Click;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(8, 51);
            lblName.Text = "Name:";
            lblName.Name = "lblName";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(80, 48);
            txtName.Size = new System.Drawing.Size(226, 23);
            txtName.Text = "magic effect";
            txtName.MaxLength = 20;
            txtName.Name = "txtName";
            txtName.TabIndex = 32;
            // 
            // chkTranslucent
            // 
            chkTranslucent.AutoSize = true;
            chkTranslucent.Location = new System.Drawing.Point(8, 76);
            chkTranslucent.Text = "Translucent-Flag";
            chkTranslucent.UseVisualStyleBackColor = true;
            chkTranslucent.Name = "chkTranslucent";
            chkTranslucent.TabIndex = 33;
            // 
            // btnExport
            // 
            btnExport.Location = new System.Drawing.Point(8, 102);
            btnExport.Size = new System.Drawing.Size(298, 26);
            btnExport.Text = "In Art / Tiledata / Animdata schreiben";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Name = "btnExport";
            btnExport.TabIndex = 34;
            btnExport.Click += BtnExport_Click;
            // 
            // btnSaveArt
            // 
            btnSaveArt.Location = new System.Drawing.Point(8, 132);
            btnSaveArt.Size = new System.Drawing.Size(146, 26);
            btnSaveArt.Text = "Art-MUL speichern";
            btnSaveArt.UseVisualStyleBackColor = true;
            btnSaveArt.Name = "btnSaveArt";
            btnSaveArt.TabIndex = 35;
            btnSaveArt.Click += BtnSaveArt_Click;
            // 
            // btnSaveTileData
            // 
            btnSaveTileData.Location = new System.Drawing.Point(160, 132);
            btnSaveTileData.Size = new System.Drawing.Size(146, 26);
            btnSaveTileData.Text = "tiledata + animdata";
            btnSaveTileData.UseVisualStyleBackColor = true;
            btnSaveTileData.Name = "btnSaveTileData";
            btnSaveTileData.TabIndex = 36;
            btnSaveTileData.Click += BtnSaveTileData_Click;
            // 
            // btnExportPng
            // 
            btnExportPng.Location = new System.Drawing.Point(8, 162);
            btnExportPng.Size = new System.Drawing.Size(146, 26);
            btnExportPng.Text = "Frames als PNG...";
            btnExportPng.UseVisualStyleBackColor = true;
            btnExportPng.Name = "btnExportPng";
            btnExportPng.TabIndex = 37;
            btnExportPng.Click += BtnExportPng_Click;
            // 
            // btnCopyScript
            // 
            btnCopyScript.Location = new System.Drawing.Point(160, 162);
            btnCopyScript.Size = new System.Drawing.Size(146, 26);
            btnCopyScript.Text = "Script-Snippet kopieren";
            btnCopyScript.UseVisualStyleBackColor = true;
            btnCopyScript.Name = "btnCopyScript";
            btnCopyScript.TabIndex = 38;
            btnCopyScript.Click += BtnCopyScript_Click;
            // 
            // lblExportHint
            // 
            lblExportHint.AutoSize = true;
            lblExportHint.Location = new System.Drawing.Point(8, 197);
            lblExportHint.Text = "Frame 0 = Basis-ID (wird im Spiel animiert)";
            lblExportHint.Name = "lblExportHint";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 39;
            // 
            // lblStatus
            // 
            lblStatus.Text = "Bildfolge (PNG/BMP/GIF) oder Sprite-Sheet importieren.";
            lblStatus.Name = "lblStatus";
            // 
            // timerPlay
            // 
            timerPlay.Interval = 100;
            timerPlay.Tick += TimerPlay_Tick;
            // 
            // SpellEffectEditorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1164, 741);
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(950, 620);
            Name = "SpellEffectEditorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Spell- & Effekt-Editor (Bildfolge → UO-Effekt)";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            grpExport.ResumeLayout(false);
            grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picHueStrip).EndInit();
            ((System.ComponentModel.ISupportInitialize)numHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGroundId).EndInit();
            ((System.ComponentModel.ISupportInitialize)numZoom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMs).EndInit();
            grpPreview.ResumeLayout(false);
            grpPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBrightness).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBlack).EndInit();
            grpProcess.ResumeLayout(false);
            grpProcess.PerformLayout();
            grpImport.ResumeLayout(false);
            grpImport.PerformLayout();
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            pnlRight.ResumeLayout(false);
            pnlRight.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ImageList imageListThumbs;
        private System.Windows.Forms.Panel pnlRight;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewEffect;
        private System.Windows.Forms.ListView lvFrames;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpImport;
        private System.Windows.Forms.Button btnAddImages;
        private System.Windows.Forms.Button btnImportSheet;
        private System.Windows.Forms.Button btnLoadArt;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.GroupBox grpProcess;
        private System.Windows.Forms.CheckBox chkBlack;
        private System.Windows.Forms.NumericUpDown numBlack;
        private System.Windows.Forms.CheckBox chkGray;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.NumericUpDown numBrightness;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Label lblMaxSize;
        private System.Windows.Forms.NumericUpDown numMaxSize;
        private System.Windows.Forms.CheckBox chkCrop;
        private System.Windows.Forms.CheckBox chkPingPong;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Label lblMs;
        private System.Windows.Forms.NumericUpDown numMs;
        private System.Windows.Forms.CheckBox chkPlay;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.NumericUpDown numZoom;
        private System.Windows.Forms.CheckBox chkAdditive;
        private System.Windows.Forms.CheckBox chkGround;
        private System.Windows.Forms.NumericUpDown numGroundId;
        private System.Windows.Forms.Label lblHue;
        private System.Windows.Forms.NumericUpDown numHue;
        private System.Windows.Forms.CheckBox chkPartialHue;
        private System.Windows.Forms.PictureBox picHueStrip;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Label lblStartId;
        private System.Windows.Forms.TextBox txtStartId;
        private System.Windows.Forms.Button btnFreeBlock;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.CheckBox chkTranslucent;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnSaveArt;
        private System.Windows.Forms.Button btnSaveTileData;
        private System.Windows.Forms.Button btnExportPng;
        private System.Windows.Forms.Button btnCopyScript;
        private System.Windows.Forms.Label lblExportHint;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer timerPlay;
    }
}

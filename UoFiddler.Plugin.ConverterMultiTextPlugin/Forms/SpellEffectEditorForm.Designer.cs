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
            btnParticles = new System.Windows.Forms.Button();
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
            lblGlow = new System.Windows.Forms.Label();
            numGlow = new System.Windows.Forms.NumericUpDown();
            lblGlowStrength = new System.Windows.Forms.Label();
            numGlowStrength = new System.Windows.Forms.NumericUpDown();
            chkPingPong = new System.Windows.Forms.CheckBox();
            grpFrame = new System.Windows.Forms.GroupBox();
            lblFrameOffset = new System.Windows.Forms.Label();
            numFrameDx = new System.Windows.Forms.NumericUpDown();
            numFrameDy = new System.Windows.Forms.NumericUpDown();
            lblRepeat = new System.Windows.Forms.Label();
            numRepeat = new System.Windows.Forms.NumericUpDown();
            lblSequence = new System.Windows.Forms.Label();
            grpPreview = new System.Windows.Forms.GroupBox();
            lblMs = new System.Windows.Forms.Label();
            numMs = new System.Windows.Forms.NumericUpDown();
            chkPlay = new System.Windows.Forms.CheckBox();
            lblZoom = new System.Windows.Forms.Label();
            numZoom = new System.Windows.Forms.NumericUpDown();
            chkAdditive = new System.Windows.Forms.CheckBox();
            lblPreviewMode = new System.Windows.Forms.Label();
            cbPreviewMode = new System.Windows.Forms.ComboBox();
            lblGround = new System.Windows.Forms.Label();
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
            ((System.ComponentModel.ISupportInitialize)numGlow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGlowStrength).BeginInit();
            grpFrame.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFrameDx).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFrameDy).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRepeat).BeginInit();
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
            pnlLeft.Controls.Add(grpFrame);
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
            grpImport.Controls.Add(btnParticles);
            grpImport.Location = new System.Drawing.Point(4, 6);
            grpImport.Size = new System.Drawing.Size(316, 112);
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
            // btnParticles
            // 
            btnParticles.Location = new System.Drawing.Point(8, 80);
            btnParticles.Size = new System.Drawing.Size(300, 26);
            btnParticles.Text = "Partikel-Generator (Funken, Feuer, Blitz ...)...";
            btnParticles.UseVisualStyleBackColor = true;
            btnParticles.Name = "btnParticles";
            btnParticles.TabIndex = 10;
            btnParticles.Click += BtnParticles_Click;
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
            grpProcess.Controls.Add(lblGlow);
            grpProcess.Controls.Add(numGlow);
            grpProcess.Controls.Add(lblGlowStrength);
            grpProcess.Controls.Add(numGlowStrength);
            grpProcess.Controls.Add(chkPingPong);
            grpProcess.Location = new System.Drawing.Point(4, 124);
            grpProcess.Size = new System.Drawing.Size(316, 190);
            grpProcess.TabStop = false;
            grpProcess.Text = "Bearbeitung";
            grpProcess.Name = "grpProcess";
            grpProcess.TabIndex = 11;
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
            chkBlack.TabIndex = 12;
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
            numBlack.TabIndex = 13;
            numBlack.ValueChanged += Process_Changed;
            // 
            // chkGray
            // 
            chkGray.AutoSize = true;
            chkGray.Location = new System.Drawing.Point(8, 48);
            chkGray.Text = "Graustufen (voll färbbar per Hue)";
            chkGray.UseVisualStyleBackColor = true;
            chkGray.Name = "chkGray";
            chkGray.TabIndex = 14;
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
            numBrightness.TabIndex = 15;
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
            numScale.TabIndex = 16;
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
            numMaxSize.TabIndex = 17;
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
            chkCrop.TabIndex = 18;
            chkCrop.CheckedChanged += Process_Changed;
            // 
            // lblGlow
            // 
            lblGlow.AutoSize = true;
            lblGlow.Location = new System.Drawing.Point(8, 131);
            lblGlow.Text = "Glow px:";
            lblGlow.Name = "lblGlow";
            // 
            // numGlow
            // 
            numGlow.Location = new System.Drawing.Point(80, 128);
            numGlow.Size = new System.Drawing.Size(50, 23);
            numGlow.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numGlow.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numGlow.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numGlow.Name = "numGlow";
            numGlow.TabIndex = 19;
            numGlow.ValueChanged += Process_Changed;
            // 
            // lblGlowStrength
            // 
            lblGlowStrength.AutoSize = true;
            lblGlowStrength.Location = new System.Drawing.Point(140, 131);
            lblGlowStrength.Text = "Stärke:";
            lblGlowStrength.Name = "lblGlowStrength";
            // 
            // numGlowStrength
            // 
            numGlowStrength.Location = new System.Drawing.Point(190, 128);
            numGlowStrength.Size = new System.Drawing.Size(56, 23);
            numGlowStrength.DecimalPlaces = 1;
            numGlowStrength.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numGlowStrength.Maximum = new decimal(new int[] { 3, 0, 0, 0 });
            numGlowStrength.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numGlowStrength.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numGlowStrength.Name = "numGlowStrength";
            numGlowStrength.TabIndex = 20;
            numGlowStrength.ValueChanged += Process_Changed;
            // 
            // chkPingPong
            // 
            chkPingPong.AutoSize = true;
            chkPingPong.Location = new System.Drawing.Point(8, 160);
            chkPingPong.Text = "Ping-Pong (vor und zurück, ohne extra Art)";
            chkPingPong.UseVisualStyleBackColor = true;
            chkPingPong.Name = "chkPingPong";
            chkPingPong.TabIndex = 21;
            chkPingPong.CheckedChanged += Process_Changed;
            // 
            // grpFrame
            // 
            grpFrame.Controls.Add(lblFrameOffset);
            grpFrame.Controls.Add(numFrameDx);
            grpFrame.Controls.Add(numFrameDy);
            grpFrame.Controls.Add(lblRepeat);
            grpFrame.Controls.Add(numRepeat);
            grpFrame.Controls.Add(lblSequence);
            grpFrame.Location = new System.Drawing.Point(4, 320);
            grpFrame.Size = new System.Drawing.Size(316, 80);
            grpFrame.TabStop = false;
            grpFrame.Text = "Ausgewählte Frames";
            grpFrame.Name = "grpFrame";
            grpFrame.TabIndex = 22;
            // 
            // lblFrameOffset
            // 
            lblFrameOffset.AutoSize = true;
            lblFrameOffset.Location = new System.Drawing.Point(8, 23);
            lblFrameOffset.Text = "Versatz X/Y:";
            lblFrameOffset.Name = "lblFrameOffset";
            // 
            // numFrameDx
            // 
            numFrameDx.Location = new System.Drawing.Point(90, 20);
            numFrameDx.Size = new System.Drawing.Size(56, 23);
            numFrameDx.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numFrameDx.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648 });
            numFrameDx.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numFrameDx.Name = "numFrameDx";
            numFrameDx.TabIndex = 23;
            numFrameDx.ValueChanged += FrameSetting_Changed;
            // 
            // numFrameDy
            // 
            numFrameDy.Location = new System.Drawing.Point(150, 20);
            numFrameDy.Size = new System.Drawing.Size(56, 23);
            numFrameDy.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numFrameDy.Minimum = new decimal(new int[] { 200, 0, 0, -2147483648 });
            numFrameDy.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numFrameDy.Name = "numFrameDy";
            numFrameDy.TabIndex = 24;
            numFrameDy.ValueChanged += FrameSetting_Changed;
            // 
            // lblRepeat
            // 
            lblRepeat.AutoSize = true;
            lblRepeat.Location = new System.Drawing.Point(212, 23);
            lblRepeat.Text = "Dauer x:";
            lblRepeat.Name = "lblRepeat";
            // 
            // numRepeat
            // 
            numRepeat.Location = new System.Drawing.Point(264, 20);
            numRepeat.Size = new System.Drawing.Size(44, 23);
            numRepeat.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numRepeat.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numRepeat.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numRepeat.Name = "numRepeat";
            numRepeat.TabIndex = 25;
            numRepeat.ValueChanged += FrameSetting_Changed;
            // 
            // lblSequence
            // 
            lblSequence.AutoSize = true;
            lblSequence.Location = new System.Drawing.Point(8, 55);
            lblSequence.Text = "Sequenz: 0 / 64 Einträge";
            lblSequence.Name = "lblSequence";
            // 
            // grpPreview
            // 
            grpPreview.Controls.Add(lblMs);
            grpPreview.Controls.Add(numMs);
            grpPreview.Controls.Add(chkPlay);
            grpPreview.Controls.Add(lblZoom);
            grpPreview.Controls.Add(numZoom);
            grpPreview.Controls.Add(chkAdditive);
            grpPreview.Controls.Add(lblPreviewMode);
            grpPreview.Controls.Add(cbPreviewMode);
            grpPreview.Controls.Add(lblGround);
            grpPreview.Controls.Add(numGroundId);
            grpPreview.Controls.Add(lblHue);
            grpPreview.Controls.Add(numHue);
            grpPreview.Controls.Add(chkPartialHue);
            grpPreview.Controls.Add(picHueStrip);
            grpPreview.Location = new System.Drawing.Point(4, 406);
            grpPreview.Size = new System.Drawing.Size(316, 170);
            grpPreview.TabStop = false;
            grpPreview.Text = "Vorschau";
            grpPreview.Name = "grpPreview";
            grpPreview.TabIndex = 26;
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
            numMs.TabIndex = 27;
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
            chkPlay.TabIndex = 28;
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
            numZoom.TabIndex = 29;
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
            chkAdditive.TabIndex = 30;
            chkAdditive.CheckedChanged += Preview_Changed;
            // 
            // lblPreviewMode
            // 
            lblPreviewMode.AutoSize = true;
            lblPreviewMode.Location = new System.Drawing.Point(8, 77);
            lblPreviewMode.Text = "Ansicht:";
            lblPreviewMode.Name = "lblPreviewMode";
            // 
            // cbPreviewMode
            // 
            cbPreviewMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbPreviewMode.FormattingEnabled = true;
            cbPreviewMode.Items.AddRange(new object[] { "Freistehend", "Auf Bodentile", "Auf Figur", "Projektil (A → B)" });
            cbPreviewMode.Location = new System.Drawing.Point(70, 74);
            cbPreviewMode.Size = new System.Drawing.Size(140, 23);
            cbPreviewMode.Name = "cbPreviewMode";
            cbPreviewMode.TabIndex = 31;
            cbPreviewMode.SelectedIndexChanged += PreviewMode_Changed;
            // 
            // lblGround
            // 
            lblGround.AutoSize = true;
            lblGround.Location = new System.Drawing.Point(214, 77);
            lblGround.Text = "Land:";
            lblGround.Name = "lblGround";
            // 
            // numGroundId
            // 
            numGroundId.Location = new System.Drawing.Point(250, 74);
            numGroundId.Size = new System.Drawing.Size(58, 23);
            numGroundId.Maximum = new decimal(new int[] { 16383, 0, 0, 0 });
            numGroundId.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numGroundId.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numGroundId.Name = "numGroundId";
            numGroundId.TabIndex = 32;
            numGroundId.ValueChanged += PreviewMode_Changed;
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new System.Drawing.Point(8, 105);
            lblHue.Text = "Hue:";
            lblHue.Name = "lblHue";
            // 
            // numHue
            // 
            numHue.Location = new System.Drawing.Point(50, 102);
            numHue.Size = new System.Drawing.Size(70, 23);
            numHue.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numHue.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Name = "numHue";
            numHue.TabIndex = 33;
            numHue.ValueChanged += NumHue_ValueChanged;
            // 
            // chkPartialHue
            // 
            chkPartialHue.AutoSize = true;
            chkPartialHue.Location = new System.Drawing.Point(128, 104);
            chkPartialHue.Text = "nur Grautöne (PartialHue)";
            chkPartialHue.UseVisualStyleBackColor = true;
            chkPartialHue.Name = "chkPartialHue";
            chkPartialHue.TabIndex = 34;
            chkPartialHue.CheckedChanged += Preview_Changed;
            // 
            // picHueStrip
            // 
            picHueStrip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            picHueStrip.Location = new System.Drawing.Point(8, 134);
            picHueStrip.Size = new System.Drawing.Size(300, 14);
            picHueStrip.TabStop = false;
            picHueStrip.Name = "picHueStrip";
            picHueStrip.TabIndex = 35;
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
            grpExport.Location = new System.Drawing.Point(4, 582);
            grpExport.Size = new System.Drawing.Size(316, 228);
            grpExport.TabStop = false;
            grpExport.Text = "Export (Art + Tiledata + Animdata)";
            grpExport.Name = "grpExport";
            grpExport.TabIndex = 36;
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
            txtStartId.TabIndex = 37;
            // 
            // btnFreeBlock
            // 
            btnFreeBlock.Location = new System.Drawing.Point(166, 18);
            btnFreeBlock.Size = new System.Drawing.Size(140, 26);
            btnFreeBlock.Text = "Freien Block";
            btnFreeBlock.UseVisualStyleBackColor = true;
            btnFreeBlock.Name = "btnFreeBlock";
            btnFreeBlock.TabIndex = 38;
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
            txtName.TabIndex = 39;
            // 
            // chkTranslucent
            // 
            chkTranslucent.AutoSize = true;
            chkTranslucent.Location = new System.Drawing.Point(8, 76);
            chkTranslucent.Text = "Translucent-Flag";
            chkTranslucent.UseVisualStyleBackColor = true;
            chkTranslucent.Name = "chkTranslucent";
            chkTranslucent.TabIndex = 40;
            // 
            // btnExport
            // 
            btnExport.Location = new System.Drawing.Point(8, 102);
            btnExport.Size = new System.Drawing.Size(298, 26);
            btnExport.Text = "In Art / Tiledata / Animdata schreiben";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Name = "btnExport";
            btnExport.TabIndex = 41;
            btnExport.Click += BtnExport_Click;
            // 
            // btnSaveArt
            // 
            btnSaveArt.Location = new System.Drawing.Point(8, 132);
            btnSaveArt.Size = new System.Drawing.Size(146, 26);
            btnSaveArt.Text = "Art-MUL speichern";
            btnSaveArt.UseVisualStyleBackColor = true;
            btnSaveArt.Name = "btnSaveArt";
            btnSaveArt.TabIndex = 42;
            btnSaveArt.Click += BtnSaveArt_Click;
            // 
            // btnSaveTileData
            // 
            btnSaveTileData.Location = new System.Drawing.Point(160, 132);
            btnSaveTileData.Size = new System.Drawing.Size(146, 26);
            btnSaveTileData.Text = "tiledata + animdata";
            btnSaveTileData.UseVisualStyleBackColor = true;
            btnSaveTileData.Name = "btnSaveTileData";
            btnSaveTileData.TabIndex = 43;
            btnSaveTileData.Click += BtnSaveTileData_Click;
            // 
            // btnExportPng
            // 
            btnExportPng.Location = new System.Drawing.Point(8, 162);
            btnExportPng.Size = new System.Drawing.Size(146, 26);
            btnExportPng.Text = "Frames als PNG...";
            btnExportPng.UseVisualStyleBackColor = true;
            btnExportPng.Name = "btnExportPng";
            btnExportPng.TabIndex = 44;
            btnExportPng.Click += BtnExportPng_Click;
            // 
            // btnCopyScript
            // 
            btnCopyScript.Location = new System.Drawing.Point(160, 162);
            btnCopyScript.Size = new System.Drawing.Size(146, 26);
            btnCopyScript.Text = "Script-Snippet kopieren";
            btnCopyScript.UseVisualStyleBackColor = true;
            btnCopyScript.Name = "btnCopyScript";
            btnCopyScript.TabIndex = 45;
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
            statusStrip.TabIndex = 46;
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
            ((System.ComponentModel.ISupportInitialize)numRepeat).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFrameDy).EndInit();
            ((System.ComponentModel.ISupportInitialize)numFrameDx).EndInit();
            grpFrame.ResumeLayout(false);
            grpFrame.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGlowStrength).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGlow).EndInit();
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
        private System.Windows.Forms.Button btnParticles;
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
        private System.Windows.Forms.Label lblGlow;
        private System.Windows.Forms.NumericUpDown numGlow;
        private System.Windows.Forms.Label lblGlowStrength;
        private System.Windows.Forms.NumericUpDown numGlowStrength;
        private System.Windows.Forms.CheckBox chkPingPong;
        private System.Windows.Forms.GroupBox grpFrame;
        private System.Windows.Forms.Label lblFrameOffset;
        private System.Windows.Forms.NumericUpDown numFrameDx;
        private System.Windows.Forms.NumericUpDown numFrameDy;
        private System.Windows.Forms.Label lblRepeat;
        private System.Windows.Forms.NumericUpDown numRepeat;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.Label lblMs;
        private System.Windows.Forms.NumericUpDown numMs;
        private System.Windows.Forms.CheckBox chkPlay;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.NumericUpDown numZoom;
        private System.Windows.Forms.CheckBox chkAdditive;
        private System.Windows.Forms.Label lblPreviewMode;
        private System.Windows.Forms.ComboBox cbPreviewMode;
        private System.Windows.Forms.Label lblGround;
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

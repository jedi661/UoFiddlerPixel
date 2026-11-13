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
    partial class SoundStudioForm
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
            waveform = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.WaveformView();
            levelMeter = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.LevelMeter();
            pnlTop = new System.Windows.Forms.Panel();
            grpFile = new System.Windows.Forms.GroupBox();
            btnLoad = new System.Windows.Forms.Button();
            lblSoundId = new System.Windows.Forms.Label();
            numSoundId = new System.Windows.Forms.NumericUpDown();
            btnLoadUo = new System.Windows.Forms.Button();
            btnFreeId = new System.Windows.Forms.Button();
            btnUndo = new System.Windows.Forms.Button();
            grpEdit = new System.Windows.Forms.GroupBox();
            btnCrop = new System.Windows.Forms.Button();
            btnDeleteSel = new System.Windows.Forms.Button();
            btnTrimSilence = new System.Windows.Forms.Button();
            lblSilence = new System.Windows.Forms.Label();
            numSilence = new System.Windows.Forms.NumericUpDown();
            btnFadeIn = new System.Windows.Forms.Button();
            btnFadeOut = new System.Windows.Forms.Button();
            lblFadeMs = new System.Windows.Forms.Label();
            numFadeMs = new System.Windows.Forms.NumericUpDown();
            btnSelectAll = new System.Windows.Forms.Button();
            btnReverse = new System.Windows.Forms.Button();
            grpLevel = new System.Windows.Forms.GroupBox();
            lblNormalize = new System.Windows.Forms.Label();
            numNormalize = new System.Windows.Forms.NumericUpDown();
            btnNormalize = new System.Windows.Forms.Button();
            lblGain = new System.Windows.Forms.Label();
            numGain = new System.Windows.Forms.NumericUpDown();
            btnGain = new System.Windows.Forms.Button();
            btnDc = new System.Windows.Forms.Button();
            grpPlay = new System.Windows.Forms.GroupBox();
            btnPlay = new System.Windows.Forms.Button();
            btnStop = new System.Windows.Forms.Button();
            lblPreviewVol = new System.Windows.Forms.Label();
            numPreviewVol = new System.Windows.Forms.NumericUpDown();
            chkLoop = new System.Windows.Forms.CheckBox();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            btnSetUo = new System.Windows.Forms.Button();
            btnSaveMul = new System.Windows.Forms.Button();
            btnSaveWav = new System.Windows.Forms.Button();
            grpView = new System.Windows.Forms.GroupBox();
            btnZoomIn = new System.Windows.Forms.Button();
            btnZoomOut = new System.Windows.Forms.Button();
            btnZoomFit = new System.Windows.Forms.Button();
            btnZoomSel = new System.Windows.Forms.Button();
            lblViewHint = new System.Windows.Forms.Label();
            grpEffects = new System.Windows.Forms.GroupBox();
            lblRoom = new System.Windows.Forms.Label();
            numRoom = new System.Windows.Forms.NumericUpDown();
            lblWet = new System.Windows.Forms.Label();
            numWet = new System.Windows.Forms.NumericUpDown();
            btnReverb = new System.Windows.Forms.Button();
            lblGate = new System.Windows.Forms.Label();
            numGate = new System.Windows.Forms.NumericUpDown();
            btnGate = new System.Windows.Forms.Button();
            lblTempo = new System.Windows.Forms.Label();
            numTempo = new System.Windows.Forms.NumericUpDown();
            btnTempo = new System.Windows.Forms.Button();
            lblPitch = new System.Windows.Forms.Label();
            numPitch = new System.Windows.Forms.NumericUpDown();
            btnPitch = new System.Windows.Forms.Button();
            btnSpeed = new System.Windows.Forms.Button();
            lblEffectsHint = new System.Windows.Forms.Label();
            grpBatch = new System.Windows.Forms.GroupBox();
            lblTargetRms = new System.Windows.Forms.Label();
            numTargetRms = new System.Windows.Forms.NumericUpDown();
            btnBatch = new System.Windows.Forms.Button();
            btnDefScript = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            lblInfo = new System.Windows.Forms.ToolStripStatusLabel();
            timerPlay = new System.Windows.Forms.Timer(components);
            pnlTop.SuspendLayout();
            grpFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSoundId).BeginInit();
            grpEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numSilence).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numFadeMs).BeginInit();
            grpLevel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numNormalize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGain).BeginInit();
            grpPlay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numPreviewVol).BeginInit();
            grpView.SuspendLayout();
            grpEffects.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numRoom).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWet).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTempo).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPitch).BeginInit();
            grpBatch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTargetRms).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // waveform
            // 
            waveform.Dock = System.Windows.Forms.DockStyle.Fill;
            waveform.Location = new System.Drawing.Point(0, 290);
            waveform.Size = new System.Drawing.Size(1014, 389);
            waveform.Name = "waveform";
            waveform.TabIndex = 0;
            waveform.SelectionChanged += Waveform_SelectionChanged;
            // 
            // levelMeter
            // 
            levelMeter.Dock = System.Windows.Forms.DockStyle.Right;
            levelMeter.Location = new System.Drawing.Point(1014, 290);
            levelMeter.Size = new System.Drawing.Size(70, 389);
            levelMeter.Name = "levelMeter";
            levelMeter.TabIndex = 1;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(grpFile);
            pnlTop.Controls.Add(grpEdit);
            pnlTop.Controls.Add(grpLevel);
            pnlTop.Controls.Add(grpPlay);
            pnlTop.Controls.Add(grpView);
            pnlTop.Controls.Add(grpEffects);
            pnlTop.Controls.Add(grpBatch);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Size = new System.Drawing.Size(1084, 290);
            pnlTop.Name = "pnlTop";
            pnlTop.TabIndex = 2;
            // 
            // grpFile
            // 
            grpFile.Controls.Add(btnLoad);
            grpFile.Controls.Add(lblSoundId);
            grpFile.Controls.Add(numSoundId);
            grpFile.Controls.Add(btnLoadUo);
            grpFile.Controls.Add(btnFreeId);
            grpFile.Controls.Add(btnUndo);
            grpFile.Location = new System.Drawing.Point(6, 4);
            grpFile.Size = new System.Drawing.Size(220, 168);
            grpFile.TabStop = false;
            grpFile.Text = "Datei";
            grpFile.Name = "grpFile";
            grpFile.TabIndex = 3;
            // 
            // btnLoad
            // 
            btnLoad.Location = new System.Drawing.Point(8, 20);
            btnLoad.Size = new System.Drawing.Size(200, 26);
            btnLoad.Text = "WAV / MP3 laden...";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Name = "btnLoad";
            btnLoad.TabIndex = 4;
            btnLoad.Click += BtnLoad_Click;
            // 
            // lblSoundId
            // 
            lblSoundId.AutoSize = true;
            lblSoundId.Location = new System.Drawing.Point(8, 55);
            lblSoundId.Text = "UO-Sound ID:";
            lblSoundId.Name = "lblSoundId";
            // 
            // numSoundId
            // 
            numSoundId.Location = new System.Drawing.Point(96, 52);
            numSoundId.Size = new System.Drawing.Size(70, 23);
            numSoundId.Maximum = new decimal(new int[] { 4094, 0, 0, 0 });
            numSoundId.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numSoundId.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numSoundId.Name = "numSoundId";
            numSoundId.TabIndex = 5;
            // 
            // btnLoadUo
            // 
            btnLoadUo.Location = new System.Drawing.Point(8, 80);
            btnLoadUo.Size = new System.Drawing.Size(98, 26);
            btnLoadUo.Text = "Aus UO laden";
            btnLoadUo.UseVisualStyleBackColor = true;
            btnLoadUo.Name = "btnLoadUo";
            btnLoadUo.TabIndex = 6;
            btnLoadUo.Click += BtnLoadUo_Click;
            // 
            // btnFreeId
            // 
            btnFreeId.Location = new System.Drawing.Point(110, 80);
            btnFreeId.Size = new System.Drawing.Size(98, 26);
            btnFreeId.Text = "Freie ID";
            btnFreeId.UseVisualStyleBackColor = true;
            btnFreeId.Name = "btnFreeId";
            btnFreeId.TabIndex = 7;
            btnFreeId.Click += BtnFreeId_Click;
            // 
            // btnUndo
            // 
            btnUndo.Location = new System.Drawing.Point(8, 112);
            btnUndo.Size = new System.Drawing.Size(200, 26);
            btnUndo.Text = "Rückgängig (Strg+Z)";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Name = "btnUndo";
            btnUndo.TabIndex = 8;
            btnUndo.Click += BtnUndo_Click;
            // 
            // grpEdit
            // 
            grpEdit.Controls.Add(btnCrop);
            grpEdit.Controls.Add(btnDeleteSel);
            grpEdit.Controls.Add(btnTrimSilence);
            grpEdit.Controls.Add(lblSilence);
            grpEdit.Controls.Add(numSilence);
            grpEdit.Controls.Add(btnFadeIn);
            grpEdit.Controls.Add(btnFadeOut);
            grpEdit.Controls.Add(lblFadeMs);
            grpEdit.Controls.Add(numFadeMs);
            grpEdit.Controls.Add(btnSelectAll);
            grpEdit.Controls.Add(btnReverse);
            grpEdit.Location = new System.Drawing.Point(232, 4);
            grpEdit.Size = new System.Drawing.Size(300, 168);
            grpEdit.TabStop = false;
            grpEdit.Text = "Schneiden (Auswahl mit der Maus ziehen)";
            grpEdit.Name = "grpEdit";
            grpEdit.TabIndex = 9;
            // 
            // btnCrop
            // 
            btnCrop.Location = new System.Drawing.Point(8, 20);
            btnCrop.Size = new System.Drawing.Size(140, 26);
            btnCrop.Text = "Auf Auswahl zuschneiden";
            btnCrop.UseVisualStyleBackColor = true;
            btnCrop.Name = "btnCrop";
            btnCrop.TabIndex = 10;
            btnCrop.Click += BtnCrop_Click;
            // 
            // btnDeleteSel
            // 
            btnDeleteSel.Location = new System.Drawing.Point(152, 20);
            btnDeleteSel.Size = new System.Drawing.Size(140, 26);
            btnDeleteSel.Text = "Auswahl löschen (Entf)";
            btnDeleteSel.UseVisualStyleBackColor = true;
            btnDeleteSel.Name = "btnDeleteSel";
            btnDeleteSel.TabIndex = 11;
            btnDeleteSel.Click += BtnDeleteSel_Click;
            // 
            // btnTrimSilence
            // 
            btnTrimSilence.Location = new System.Drawing.Point(8, 52);
            btnTrimSilence.Size = new System.Drawing.Size(140, 26);
            btnTrimSilence.Text = "Stille entfernen";
            btnTrimSilence.UseVisualStyleBackColor = true;
            btnTrimSilence.Name = "btnTrimSilence";
            btnTrimSilence.TabIndex = 12;
            btnTrimSilence.Click += BtnTrimSilence_Click;
            // 
            // lblSilence
            // 
            lblSilence.AutoSize = true;
            lblSilence.Location = new System.Drawing.Point(152, 56);
            lblSilence.Text = "Schwelle %:";
            lblSilence.Name = "lblSilence";
            // 
            // numSilence
            // 
            numSilence.Location = new System.Drawing.Point(226, 52);
            numSilence.Size = new System.Drawing.Size(60, 23);
            numSilence.DecimalPlaces = 1;
            numSilence.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numSilence.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            numSilence.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numSilence.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numSilence.Name = "numSilence";
            numSilence.TabIndex = 13;
            // 
            // btnFadeIn
            // 
            btnFadeIn.Location = new System.Drawing.Point(8, 84);
            btnFadeIn.Size = new System.Drawing.Size(90, 26);
            btnFadeIn.Text = "Einblenden";
            btnFadeIn.UseVisualStyleBackColor = true;
            btnFadeIn.Name = "btnFadeIn";
            btnFadeIn.TabIndex = 14;
            btnFadeIn.Click += BtnFadeIn_Click;
            // 
            // btnFadeOut
            // 
            btnFadeOut.Location = new System.Drawing.Point(102, 84);
            btnFadeOut.Size = new System.Drawing.Size(90, 26);
            btnFadeOut.Text = "Ausblenden";
            btnFadeOut.UseVisualStyleBackColor = true;
            btnFadeOut.Name = "btnFadeOut";
            btnFadeOut.TabIndex = 15;
            btnFadeOut.Click += BtnFadeOut_Click;
            // 
            // lblFadeMs
            // 
            lblFadeMs.AutoSize = true;
            lblFadeMs.Location = new System.Drawing.Point(198, 88);
            lblFadeMs.Text = "ms:";
            lblFadeMs.Name = "lblFadeMs";
            // 
            // numFadeMs
            // 
            numFadeMs.Location = new System.Drawing.Point(226, 84);
            numFadeMs.Size = new System.Drawing.Size(60, 23);
            numFadeMs.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numFadeMs.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numFadeMs.Value = new decimal(new int[] { 80, 0, 0, 0 });
            numFadeMs.Name = "numFadeMs";
            numFadeMs.TabIndex = 16;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Location = new System.Drawing.Point(8, 116);
            btnSelectAll.Size = new System.Drawing.Size(140, 26);
            btnSelectAll.Text = "Alles auswählen";
            btnSelectAll.UseVisualStyleBackColor = true;
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.TabIndex = 17;
            btnSelectAll.Click += BtnSelectAll_Click;
            // 
            // btnReverse
            // 
            btnReverse.Location = new System.Drawing.Point(152, 116);
            btnReverse.Size = new System.Drawing.Size(140, 26);
            btnReverse.Text = "Rückwärts";
            btnReverse.UseVisualStyleBackColor = true;
            btnReverse.Name = "btnReverse";
            btnReverse.TabIndex = 18;
            btnReverse.Click += BtnReverse_Click;
            // 
            // grpLevel
            // 
            grpLevel.Controls.Add(lblNormalize);
            grpLevel.Controls.Add(numNormalize);
            grpLevel.Controls.Add(btnNormalize);
            grpLevel.Controls.Add(lblGain);
            grpLevel.Controls.Add(numGain);
            grpLevel.Controls.Add(btnGain);
            grpLevel.Controls.Add(btnDc);
            grpLevel.Location = new System.Drawing.Point(538, 4);
            grpLevel.Size = new System.Drawing.Size(250, 168);
            grpLevel.TabStop = false;
            grpLevel.Text = "Lautstärke";
            grpLevel.Name = "grpLevel";
            grpLevel.TabIndex = 19;
            // 
            // lblNormalize
            // 
            lblNormalize.AutoSize = true;
            lblNormalize.Location = new System.Drawing.Point(8, 23);
            lblNormalize.Text = "Normalisieren auf %:";
            lblNormalize.Name = "lblNormalize";
            // 
            // numNormalize
            // 
            numNormalize.Location = new System.Drawing.Point(140, 20);
            numNormalize.Size = new System.Drawing.Size(60, 23);
            numNormalize.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numNormalize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numNormalize.Value = new decimal(new int[] { 95, 0, 0, 0 });
            numNormalize.Name = "numNormalize";
            numNormalize.TabIndex = 20;
            // 
            // btnNormalize
            // 
            btnNormalize.Location = new System.Drawing.Point(8, 48);
            btnNormalize.Size = new System.Drawing.Size(230, 26);
            btnNormalize.Text = "Normalisieren";
            btnNormalize.UseVisualStyleBackColor = true;
            btnNormalize.Name = "btnNormalize";
            btnNormalize.TabIndex = 21;
            btnNormalize.Click += BtnNormalize_Click;
            // 
            // lblGain
            // 
            lblGain.AutoSize = true;
            lblGain.Location = new System.Drawing.Point(8, 85);
            lblGain.Text = "Verstärkung dB:";
            lblGain.Name = "lblGain";
            // 
            // numGain
            // 
            numGain.Location = new System.Drawing.Point(140, 82);
            numGain.Size = new System.Drawing.Size(60, 23);
            numGain.DecimalPlaces = 1;
            numGain.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numGain.Maximum = new decimal(new int[] { 40, 0, 0, 0 });
            numGain.Minimum = new decimal(new int[] { 40, 0, 0, -2147483648 });
            numGain.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numGain.Name = "numGain";
            numGain.TabIndex = 22;
            // 
            // btnGain
            // 
            btnGain.Location = new System.Drawing.Point(8, 110);
            btnGain.Size = new System.Drawing.Size(110, 26);
            btnGain.Text = "Anwenden";
            btnGain.UseVisualStyleBackColor = true;
            btnGain.Name = "btnGain";
            btnGain.TabIndex = 23;
            btnGain.Click += BtnGain_Click;
            // 
            // btnDc
            // 
            btnDc.Location = new System.Drawing.Point(128, 110);
            btnDc.Size = new System.Drawing.Size(110, 26);
            btnDc.Text = "DC entfernen";
            btnDc.UseVisualStyleBackColor = true;
            btnDc.Name = "btnDc";
            btnDc.TabIndex = 24;
            btnDc.Click += BtnDc_Click;
            // 
            // grpPlay
            // 
            grpPlay.Controls.Add(btnPlay);
            grpPlay.Controls.Add(btnStop);
            grpPlay.Controls.Add(lblPreviewVol);
            grpPlay.Controls.Add(numPreviewVol);
            grpPlay.Controls.Add(chkLoop);
            grpPlay.Controls.Add(lblName);
            grpPlay.Controls.Add(txtName);
            grpPlay.Controls.Add(btnSetUo);
            grpPlay.Controls.Add(btnSaveMul);
            grpPlay.Controls.Add(btnSaveWav);
            grpPlay.Location = new System.Drawing.Point(794, 4);
            grpPlay.Size = new System.Drawing.Size(280, 168);
            grpPlay.TabStop = false;
            grpPlay.Text = "Vorschau & Export";
            grpPlay.Name = "grpPlay";
            grpPlay.TabIndex = 25;
            // 
            // btnPlay
            // 
            btnPlay.Location = new System.Drawing.Point(8, 20);
            btnPlay.Size = new System.Drawing.Size(170, 26);
            btnPlay.Text = "▶ Abspielen (Leertaste)";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Name = "btnPlay";
            btnPlay.TabIndex = 26;
            btnPlay.Click += BtnPlay_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new System.Drawing.Point(182, 20);
            btnStop.Size = new System.Drawing.Size(88, 26);
            btnStop.Text = "■ Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Name = "btnStop";
            btnStop.TabIndex = 27;
            btnStop.Click += BtnStop_Click;
            // 
            // lblPreviewVol
            // 
            lblPreviewVol.AutoSize = true;
            lblPreviewVol.Location = new System.Drawing.Point(8, 55);
            lblPreviewVol.Text = "Vorschau-Lautst. %:";
            lblPreviewVol.Name = "lblPreviewVol";
            // 
            // numPreviewVol
            // 
            numPreviewVol.Location = new System.Drawing.Point(124, 52);
            numPreviewVol.Size = new System.Drawing.Size(56, 23);
            numPreviewVol.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            numPreviewVol.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numPreviewVol.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numPreviewVol.Name = "numPreviewVol";
            numPreviewVol.TabIndex = 28;
            // 
            // chkLoop
            // 
            chkLoop.AutoSize = true;
            chkLoop.Location = new System.Drawing.Point(190, 54);
            chkLoop.Text = "Schleife";
            chkLoop.UseVisualStyleBackColor = true;
            chkLoop.Name = "chkLoop";
            chkLoop.TabIndex = 29;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new System.Drawing.Point(8, 85);
            lblName.Text = "Name:";
            lblName.Name = "lblName";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(56, 82);
            txtName.Size = new System.Drawing.Size(214, 23);
            txtName.Text = "new_sound.wav";
            txtName.MaxLength = 31;
            txtName.Name = "txtName";
            txtName.TabIndex = 30;
            // 
            // btnSetUo
            // 
            btnSetUo.Location = new System.Drawing.Point(8, 110);
            btnSetUo.Size = new System.Drawing.Size(130, 26);
            btnSetUo.Text = "Als UO-Sound setzen";
            btnSetUo.UseVisualStyleBackColor = true;
            btnSetUo.Name = "btnSetUo";
            btnSetUo.TabIndex = 31;
            btnSetUo.Click += BtnSetUo_Click;
            // 
            // btnSaveMul
            // 
            btnSaveMul.Location = new System.Drawing.Point(142, 110);
            btnSaveMul.Size = new System.Drawing.Size(128, 26);
            btnSaveMul.Text = "sound.mul speichern";
            btnSaveMul.UseVisualStyleBackColor = true;
            btnSaveMul.Name = "btnSaveMul";
            btnSaveMul.TabIndex = 32;
            btnSaveMul.Click += BtnSaveMul_Click;
            // 
            // btnSaveWav
            // 
            btnSaveWav.Location = new System.Drawing.Point(8, 136);
            btnSaveWav.Size = new System.Drawing.Size(262, 26);
            btnSaveWav.Text = "Als WAV (UO-Format) speichern...";
            btnSaveWav.UseVisualStyleBackColor = true;
            btnSaveWav.Name = "btnSaveWav";
            btnSaveWav.TabIndex = 33;
            btnSaveWav.Click += BtnSaveWav_Click;
            // 
            // grpView
            // 
            grpView.Controls.Add(btnZoomIn);
            grpView.Controls.Add(btnZoomOut);
            grpView.Controls.Add(btnZoomFit);
            grpView.Controls.Add(btnZoomSel);
            grpView.Controls.Add(lblViewHint);
            grpView.Location = new System.Drawing.Point(6, 176);
            grpView.Size = new System.Drawing.Size(220, 108);
            grpView.TabStop = false;
            grpView.Text = "Ansicht";
            grpView.Name = "grpView";
            grpView.TabIndex = 34;
            // 
            // btnZoomIn
            // 
            btnZoomIn.Location = new System.Drawing.Point(8, 20);
            btnZoomIn.Size = new System.Drawing.Size(98, 26);
            btnZoomIn.Text = "Zoom +";
            btnZoomIn.UseVisualStyleBackColor = true;
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.TabIndex = 35;
            btnZoomIn.Click += BtnZoomIn_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.Location = new System.Drawing.Point(110, 20);
            btnZoomOut.Size = new System.Drawing.Size(98, 26);
            btnZoomOut.Text = "Zoom −";
            btnZoomOut.UseVisualStyleBackColor = true;
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.TabIndex = 36;
            btnZoomOut.Click += BtnZoomOut_Click;
            // 
            // btnZoomFit
            // 
            btnZoomFit.Location = new System.Drawing.Point(8, 50);
            btnZoomFit.Size = new System.Drawing.Size(98, 26);
            btnZoomFit.Text = "Alles";
            btnZoomFit.UseVisualStyleBackColor = true;
            btnZoomFit.Name = "btnZoomFit";
            btnZoomFit.TabIndex = 37;
            btnZoomFit.Click += BtnZoomFit_Click;
            // 
            // btnZoomSel
            // 
            btnZoomSel.Location = new System.Drawing.Point(110, 50);
            btnZoomSel.Size = new System.Drawing.Size(98, 26);
            btnZoomSel.Text = "Auswahl";
            btnZoomSel.UseVisualStyleBackColor = true;
            btnZoomSel.Name = "btnZoomSel";
            btnZoomSel.TabIndex = 38;
            btnZoomSel.Click += BtnZoomSel_Click;
            // 
            // lblViewHint
            // 
            lblViewHint.AutoSize = true;
            lblViewHint.Location = new System.Drawing.Point(8, 83);
            lblViewHint.Text = "Rad = Zoom, Shift+Rad = Scrollen";
            lblViewHint.Name = "lblViewHint";
            // 
            // grpEffects
            // 
            grpEffects.Controls.Add(lblRoom);
            grpEffects.Controls.Add(numRoom);
            grpEffects.Controls.Add(lblWet);
            grpEffects.Controls.Add(numWet);
            grpEffects.Controls.Add(btnReverb);
            grpEffects.Controls.Add(lblGate);
            grpEffects.Controls.Add(numGate);
            grpEffects.Controls.Add(btnGate);
            grpEffects.Controls.Add(lblTempo);
            grpEffects.Controls.Add(numTempo);
            grpEffects.Controls.Add(btnTempo);
            grpEffects.Controls.Add(lblPitch);
            grpEffects.Controls.Add(numPitch);
            grpEffects.Controls.Add(btnPitch);
            grpEffects.Controls.Add(btnSpeed);
            grpEffects.Controls.Add(lblEffectsHint);
            grpEffects.Location = new System.Drawing.Point(232, 176);
            grpEffects.Size = new System.Drawing.Size(556, 108);
            grpEffects.TabStop = false;
            grpEffects.Text = "Effekte (auf Auswahl bzw. ganzen Sound)";
            grpEffects.Name = "grpEffects";
            grpEffects.TabIndex = 39;
            // 
            // lblRoom
            // 
            lblRoom.AutoSize = true;
            lblRoom.Location = new System.Drawing.Point(8, 23);
            lblRoom.Text = "Hall Raum %:";
            lblRoom.Name = "lblRoom";
            // 
            // numRoom
            // 
            numRoom.Location = new System.Drawing.Point(92, 20);
            numRoom.Size = new System.Drawing.Size(50, 23);
            numRoom.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numRoom.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numRoom.Value = new decimal(new int[] { 50, 0, 0, 0 });
            numRoom.Name = "numRoom";
            numRoom.TabIndex = 40;
            // 
            // lblWet
            // 
            lblWet.AutoSize = true;
            lblWet.Location = new System.Drawing.Point(148, 23);
            lblWet.Text = "Anteil %:";
            lblWet.Name = "lblWet";
            // 
            // numWet
            // 
            numWet.Location = new System.Drawing.Point(206, 20);
            numWet.Size = new System.Drawing.Size(50, 23);
            numWet.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numWet.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numWet.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numWet.Name = "numWet";
            numWet.TabIndex = 41;
            // 
            // btnReverb
            // 
            btnReverb.Location = new System.Drawing.Point(262, 18);
            btnReverb.Size = new System.Drawing.Size(70, 26);
            btnReverb.Text = "Hall";
            btnReverb.UseVisualStyleBackColor = true;
            btnReverb.Name = "btnReverb";
            btnReverb.TabIndex = 42;
            btnReverb.Click += BtnReverb_Click;
            // 
            // lblGate
            // 
            lblGate.AutoSize = true;
            lblGate.Location = new System.Drawing.Point(342, 23);
            lblGate.Text = "Gate dB:";
            lblGate.Name = "lblGate";
            // 
            // numGate
            // 
            numGate.Location = new System.Drawing.Point(400, 20);
            numGate.Size = new System.Drawing.Size(56, 23);
            numGate.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
            numGate.Minimum = new decimal(new int[] { 80, 0, 0, -2147483648 });
            numGate.Value = new decimal(new int[] { 45, 0, 0, -2147483648 });
            numGate.Name = "numGate";
            numGate.TabIndex = 43;
            // 
            // btnGate
            // 
            btnGate.Location = new System.Drawing.Point(462, 18);
            btnGate.Size = new System.Drawing.Size(86, 26);
            btnGate.Text = "Rausch-Gate";
            btnGate.UseVisualStyleBackColor = true;
            btnGate.Name = "btnGate";
            btnGate.TabIndex = 44;
            btnGate.Click += BtnGate_Click;
            // 
            // lblTempo
            // 
            lblTempo.AutoSize = true;
            lblTempo.Location = new System.Drawing.Point(8, 55);
            lblTempo.Text = "Tempo %:";
            lblTempo.Name = "lblTempo";
            // 
            // numTempo
            // 
            numTempo.Location = new System.Drawing.Point(92, 52);
            numTempo.Size = new System.Drawing.Size(50, 23);
            numTempo.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            numTempo.Minimum = new decimal(new int[] { 25, 0, 0, 0 });
            numTempo.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numTempo.Name = "numTempo";
            numTempo.TabIndex = 45;
            // 
            // btnTempo
            // 
            btnTempo.Location = new System.Drawing.Point(148, 50);
            btnTempo.Size = new System.Drawing.Size(70, 26);
            btnTempo.Text = "Tempo";
            btnTempo.UseVisualStyleBackColor = true;
            btnTempo.Name = "btnTempo";
            btnTempo.TabIndex = 46;
            btnTempo.Click += BtnTempo_Click;
            // 
            // lblPitch
            // 
            lblPitch.AutoSize = true;
            lblPitch.Location = new System.Drawing.Point(226, 55);
            lblPitch.Text = "Tonhöhe HT:";
            lblPitch.Name = "lblPitch";
            // 
            // numPitch
            // 
            numPitch.Location = new System.Drawing.Point(304, 52);
            numPitch.Size = new System.Drawing.Size(50, 23);
            numPitch.Maximum = new decimal(new int[] { 24, 0, 0, 0 });
            numPitch.Minimum = new decimal(new int[] { 24, 0, 0, -2147483648 });
            numPitch.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numPitch.Name = "numPitch";
            numPitch.TabIndex = 47;
            // 
            // btnPitch
            // 
            btnPitch.Location = new System.Drawing.Point(360, 50);
            btnPitch.Size = new System.Drawing.Size(80, 26);
            btnPitch.Text = "Tonhöhe";
            btnPitch.UseVisualStyleBackColor = true;
            btnPitch.Name = "btnPitch";
            btnPitch.TabIndex = 48;
            btnPitch.Click += BtnPitch_Click;
            // 
            // btnSpeed
            // 
            btnSpeed.Location = new System.Drawing.Point(446, 50);
            btnSpeed.Size = new System.Drawing.Size(102, 26);
            btnSpeed.Text = "Beides (Geschw.)";
            btnSpeed.UseVisualStyleBackColor = true;
            btnSpeed.Name = "btnSpeed";
            btnSpeed.TabIndex = 49;
            btnSpeed.Click += BtnSpeed_Click;
            // 
            // lblEffectsHint
            // 
            lblEffectsHint.AutoSize = true;
            lblEffectsHint.Location = new System.Drawing.Point(8, 83);
            lblEffectsHint.Text = "Tempo: Länge ohne Tonhöhe · Tonhöhe: ohne Länge · Beides: wie schneller abspielen";
            lblEffectsHint.Name = "lblEffectsHint";
            // 
            // grpBatch
            // 
            grpBatch.Controls.Add(lblTargetRms);
            grpBatch.Controls.Add(numTargetRms);
            grpBatch.Controls.Add(btnBatch);
            grpBatch.Controls.Add(btnDefScript);
            grpBatch.Location = new System.Drawing.Point(794, 176);
            grpBatch.Size = new System.Drawing.Size(280, 108);
            grpBatch.TabStop = false;
            grpBatch.Text = "Stapel & Einträge";
            grpBatch.Name = "grpBatch";
            grpBatch.TabIndex = 50;
            // 
            // lblTargetRms
            // 
            lblTargetRms.AutoSize = true;
            lblTargetRms.Location = new System.Drawing.Point(8, 23);
            lblTargetRms.Text = "Ziel-Lautheit RMS dB:";
            lblTargetRms.Name = "lblTargetRms";
            // 
            // numTargetRms
            // 
            numTargetRms.Location = new System.Drawing.Point(150, 20);
            numTargetRms.Size = new System.Drawing.Size(60, 23);
            numTargetRms.Maximum = new decimal(new int[] { 3, 0, 0, -2147483648 });
            numTargetRms.Minimum = new decimal(new int[] { 40, 0, 0, -2147483648 });
            numTargetRms.Value = new decimal(new int[] { 18, 0, 0, -2147483648 });
            numTargetRms.Name = "numTargetRms";
            numTargetRms.TabIndex = 51;
            // 
            // btnBatch
            // 
            btnBatch.Location = new System.Drawing.Point(8, 48);
            btnBatch.Size = new System.Drawing.Size(262, 26);
            btnBatch.Text = "Stapel: Lautheit angleichen...";
            btnBatch.UseVisualStyleBackColor = true;
            btnBatch.Name = "btnBatch";
            btnBatch.TabIndex = 52;
            btnBatch.Click += BtnBatch_Click;
            // 
            // btnDefScript
            // 
            btnDefScript.Location = new System.Drawing.Point(8, 76);
            btnDefScript.Size = new System.Drawing.Size(262, 26);
            btnDefScript.Text = "Sound.def / Script-Einträge...";
            btnDefScript.UseVisualStyleBackColor = true;
            btnDefScript.Name = "btnDefScript";
            btnDefScript.TabIndex = 53;
            btnDefScript.Click += BtnDefScript_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus, lblInfo });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 54;
            // 
            // lblStatus
            // 
            lblStatus.Text = "WAV oder MP3 laden.";
            lblStatus.Name = "lblStatus";
            // 
            // lblInfo
            // 
            lblInfo.Spring = true;
            lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblInfo.Name = "lblInfo";
            // 
            // timerPlay
            // 
            timerPlay.Interval = 30;
            timerPlay.Tick += TimerPlay_Tick;
            // 
            // SoundStudioForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1084, 701);
            Controls.Add(waveform);
            Controls.Add(levelMeter);
            Controls.Add(pnlTop);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(1000, 600);
            Name = "SoundStudioForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sound-Studio (WAV / MP3 → UO-Sound)";
            KeyPreview = true;
            KeyDown += SoundStudioForm_KeyDown;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTargetRms).EndInit();
            grpBatch.ResumeLayout(false);
            grpBatch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPitch).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTempo).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWet).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRoom).EndInit();
            grpEffects.ResumeLayout(false);
            grpEffects.PerformLayout();
            grpView.ResumeLayout(false);
            grpView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numPreviewVol).EndInit();
            grpPlay.ResumeLayout(false);
            grpPlay.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGain).EndInit();
            ((System.ComponentModel.ISupportInitialize)numNormalize).EndInit();
            grpLevel.ResumeLayout(false);
            grpLevel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numFadeMs).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSilence).EndInit();
            grpEdit.ResumeLayout(false);
            grpEdit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSoundId).EndInit();
            grpFile.ResumeLayout(false);
            grpFile.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.WaveformView waveform;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.LevelMeter levelMeter;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.GroupBox grpFile;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblSoundId;
        private System.Windows.Forms.NumericUpDown numSoundId;
        private System.Windows.Forms.Button btnLoadUo;
        private System.Windows.Forms.Button btnFreeId;
        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.GroupBox grpEdit;
        private System.Windows.Forms.Button btnCrop;
        private System.Windows.Forms.Button btnDeleteSel;
        private System.Windows.Forms.Button btnTrimSilence;
        private System.Windows.Forms.Label lblSilence;
        private System.Windows.Forms.NumericUpDown numSilence;
        private System.Windows.Forms.Button btnFadeIn;
        private System.Windows.Forms.Button btnFadeOut;
        private System.Windows.Forms.Label lblFadeMs;
        private System.Windows.Forms.NumericUpDown numFadeMs;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnReverse;
        private System.Windows.Forms.GroupBox grpLevel;
        private System.Windows.Forms.Label lblNormalize;
        private System.Windows.Forms.NumericUpDown numNormalize;
        private System.Windows.Forms.Button btnNormalize;
        private System.Windows.Forms.Label lblGain;
        private System.Windows.Forms.NumericUpDown numGain;
        private System.Windows.Forms.Button btnGain;
        private System.Windows.Forms.Button btnDc;
        private System.Windows.Forms.GroupBox grpPlay;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblPreviewVol;
        private System.Windows.Forms.NumericUpDown numPreviewVol;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Button btnSetUo;
        private System.Windows.Forms.Button btnSaveMul;
        private System.Windows.Forms.Button btnSaveWav;
        private System.Windows.Forms.GroupBox grpView;
        private System.Windows.Forms.Button btnZoomIn;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btnZoomFit;
        private System.Windows.Forms.Button btnZoomSel;
        private System.Windows.Forms.Label lblViewHint;
        private System.Windows.Forms.GroupBox grpEffects;
        private System.Windows.Forms.Label lblRoom;
        private System.Windows.Forms.NumericUpDown numRoom;
        private System.Windows.Forms.Label lblWet;
        private System.Windows.Forms.NumericUpDown numWet;
        private System.Windows.Forms.Button btnReverb;
        private System.Windows.Forms.Label lblGate;
        private System.Windows.Forms.NumericUpDown numGate;
        private System.Windows.Forms.Button btnGate;
        private System.Windows.Forms.Label lblTempo;
        private System.Windows.Forms.NumericUpDown numTempo;
        private System.Windows.Forms.Button btnTempo;
        private System.Windows.Forms.Label lblPitch;
        private System.Windows.Forms.NumericUpDown numPitch;
        private System.Windows.Forms.Button btnPitch;
        private System.Windows.Forms.Button btnSpeed;
        private System.Windows.Forms.Label lblEffectsHint;
        private System.Windows.Forms.GroupBox grpBatch;
        private System.Windows.Forms.Label lblTargetRms;
        private System.Windows.Forms.NumericUpDown numTargetRms;
        private System.Windows.Forms.Button btnBatch;
        private System.Windows.Forms.Button btnDefScript;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblInfo;
        private System.Windows.Forms.Timer timerPlay;
    }
}

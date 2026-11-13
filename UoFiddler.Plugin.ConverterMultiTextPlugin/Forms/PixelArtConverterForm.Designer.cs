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
    partial class PixelArtConverterForm
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
            splitViews = new System.Windows.Forms.SplitContainer();
            viewSource = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewResult = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            pnlSettings = new System.Windows.Forms.Panel();
            btnLoad = new System.Windows.Forms.Button();
            btnClipboard = new System.Windows.Forms.Button();
            lblPreset = new System.Windows.Forms.Label();
            cbPreset = new System.Windows.Forms.ComboBox();
            grpBackground = new System.Windows.Forms.GroupBox();
            cbBackground = new System.Windows.Forms.ComboBox();
            pnlKeyColor = new System.Windows.Forms.Panel();
            lblTolerance = new System.Windows.Forms.Label();
            numTolerance = new System.Windows.Forms.NumericUpDown();
            lblToleranceHint = new System.Windows.Forms.Label();
            grpColors = new System.Windows.Forms.GroupBox();
            lblBrightness = new System.Windows.Forms.Label();
            numBrightness = new System.Windows.Forms.NumericUpDown();
            lblContrast = new System.Windows.Forms.Label();
            numContrast = new System.Windows.Forms.NumericUpDown();
            lblSaturation = new System.Windows.Forms.Label();
            numSaturation = new System.Windows.Forms.NumericUpDown();
            chkGray = new System.Windows.Forms.CheckBox();
            lblHuePreview = new System.Windows.Forms.Label();
            numHuePreview = new System.Windows.Forms.NumericUpDown();
            lblHueOff = new System.Windows.Forms.Label();
            grpSize = new System.Windows.Forms.GroupBox();
            lblMaxW = new System.Windows.Forms.Label();
            numMaxW = new System.Windows.Forms.NumericUpDown();
            lblMaxH = new System.Windows.Forms.Label();
            numMaxH = new System.Windows.Forms.NumericUpDown();
            lblScale = new System.Windows.Forms.Label();
            cbScale = new System.Windows.Forms.ComboBox();
            chkUpscale = new System.Windows.Forms.CheckBox();
            lblIso = new System.Windows.Forms.Label();
            cbIso = new System.Windows.Forms.ComboBox();
            lblIsoWidth = new System.Windows.Forms.Label();
            numIsoWidth = new System.Windows.Forms.NumericUpDown();
            chkAutoCrop = new System.Windows.Forms.CheckBox();
            grpPalette = new System.Windows.Forms.GroupBox();
            lblColors = new System.Windows.Forms.Label();
            numColors = new System.Windows.Forms.NumericUpDown();
            chkDither = new System.Windows.Forms.CheckBox();
            lblSharpen = new System.Windows.Forms.Label();
            numSharpen = new System.Windows.Forms.NumericUpDown();
            lblOutline = new System.Windows.Forms.Label();
            cbOutline = new System.Windows.Forms.ComboBox();
            pnlOutlineColor = new System.Windows.Forms.Panel();
            chkOutlineDiagonal = new System.Windows.Forms.CheckBox();
            grpExtra = new System.Windows.Forms.GroupBox();
            lblPaletteSource = new System.Windows.Forms.Label();
            cbPaletteSource = new System.Windows.Forms.ComboBox();
            txtPaletteId = new System.Windows.Forms.TextBox();
            btnPaletteFile = new System.Windows.Forms.Button();
            lblPaletteInfo = new System.Windows.Forms.Label();
            chkShadow = new System.Windows.Forms.CheckBox();
            lblShadow = new System.Windows.Forms.Label();
            numShadowX = new System.Windows.Forms.NumericUpDown();
            numShadowY = new System.Windows.Forms.NumericUpDown();
            chkShadowDither = new System.Windows.Forms.CheckBox();
            chkCompare = new System.Windows.Forms.CheckBox();
            trackCompare = new System.Windows.Forms.TrackBar();
            grpExport = new System.Windows.Forms.GroupBox();
            lblId = new System.Windows.Forms.Label();
            txtId = new System.Windows.Forms.TextBox();
            btnFreeId = new System.Windows.Forms.Button();
            btnSetArt = new System.Windows.Forms.Button();
            btnSetLand = new System.Windows.Forms.Button();
            btnSetGump = new System.Windows.Forms.Button();
            btnSavePng = new System.Windows.Forms.Button();
            btnSaveArt = new System.Windows.Forms.Button();
            btnSaveGump = new System.Windows.Forms.Button();
            btnCopyResult = new System.Windows.Forms.Button();
            btnBatch = new System.Windows.Forms.Button();
            lblZoom = new System.Windows.Forms.Label();
            numZoom = new System.Windows.Forms.NumericUpDown();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            timerDebounce = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)splitViews).BeginInit();
            splitViews.Panel1.SuspendLayout();
            splitViews.Panel2.SuspendLayout();
            pnlSettings.SuspendLayout();
            grpBackground.SuspendLayout();
            pnlKeyColor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTolerance).BeginInit();
            grpColors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBrightness).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numContrast).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSaturation).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHuePreview).BeginInit();
            grpSize.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numMaxW).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxH).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numIsoWidth).BeginInit();
            grpPalette.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numColors).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSharpen).BeginInit();
            pnlOutlineColor.SuspendLayout();
            grpExtra.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numShadowX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numShadowY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackCompare).BeginInit();
            grpExport.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numZoom).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitViews
            // 
            splitViews.Panel1.Controls.Add(viewSource);
            splitViews.Panel2.Controls.Add(viewResult);
            splitViews.Dock = System.Windows.Forms.DockStyle.Fill;
            splitViews.Location = new System.Drawing.Point(330, 0);
            splitViews.Size = new System.Drawing.Size(834, 699);
            splitViews.SplitterDistance = 400;
            splitViews.Name = "splitViews";
            splitViews.TabIndex = 0;
            // 
            // viewSource
            // 
            viewSource.Caption = "Original";
            viewSource.Dock = System.Windows.Forms.DockStyle.Fill;
            viewSource.Zoom = 1;
            viewSource.Location = new System.Drawing.Point(0, 0);
            viewSource.Size = new System.Drawing.Size(400, 699);
            viewSource.Name = "viewSource";
            viewSource.TabIndex = 1;
            viewSource.MouseClick += ViewSource_MouseClick;
            // 
            // viewResult
            // 
            viewResult.Caption = "Ergebnis (UO)";
            viewResult.Dock = System.Windows.Forms.DockStyle.Fill;
            viewResult.Zoom = 3;
            viewResult.Location = new System.Drawing.Point(0, 0);
            viewResult.Size = new System.Drawing.Size(430, 699);
            viewResult.Name = "viewResult";
            viewResult.TabIndex = 2;
            // 
            // pnlSettings
            // 
            pnlSettings.Controls.Add(btnLoad);
            pnlSettings.Controls.Add(btnClipboard);
            pnlSettings.Controls.Add(lblPreset);
            pnlSettings.Controls.Add(cbPreset);
            pnlSettings.Controls.Add(grpBackground);
            pnlSettings.Controls.Add(grpColors);
            pnlSettings.Controls.Add(grpSize);
            pnlSettings.Controls.Add(grpPalette);
            pnlSettings.Controls.Add(grpExtra);
            pnlSettings.Controls.Add(grpExport);
            pnlSettings.Controls.Add(lblZoom);
            pnlSettings.Controls.Add(numZoom);
            pnlSettings.AutoScroll = true;
            pnlSettings.Dock = System.Windows.Forms.DockStyle.Left;
            pnlSettings.Location = new System.Drawing.Point(0, 0);
            pnlSettings.Size = new System.Drawing.Size(330, 699);
            pnlSettings.Name = "pnlSettings";
            pnlSettings.TabIndex = 3;
            // 
            // btnLoad
            // 
            btnLoad.Location = new System.Drawing.Point(6, 6);
            btnLoad.Size = new System.Drawing.Size(150, 26);
            btnLoad.Text = "Bild laden...";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Name = "btnLoad";
            btnLoad.TabIndex = 4;
            btnLoad.Click += BtnLoad_Click;
            // 
            // btnClipboard
            // 
            btnClipboard.Location = new System.Drawing.Point(160, 6);
            btnClipboard.Size = new System.Drawing.Size(150, 26);
            btnClipboard.Text = "Aus Zwischenablage";
            btnClipboard.UseVisualStyleBackColor = true;
            btnClipboard.Name = "btnClipboard";
            btnClipboard.TabIndex = 5;
            btnClipboard.Click += BtnClipboard_Click;
            // 
            // lblPreset
            // 
            lblPreset.AutoSize = true;
            lblPreset.Location = new System.Drawing.Point(6, 41);
            lblPreset.Text = "Ziel / Vorlage:";
            lblPreset.Name = "lblPreset";
            // 
            // cbPreset
            // 
            cbPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbPreset.FormattingEnabled = true;
            cbPreset.Items.AddRange(new object[] { "Item (Art, max. 44x80)", "Großes Item (Art, max. 88x140)", "Bodentile (Land 44x44)", "Wand links (\\)", "Wand rechts (/)", "Gump Icon (max. 64x64)", "Gump frei (Originalgröße)" });
            cbPreset.Location = new System.Drawing.Point(110, 38);
            cbPreset.Size = new System.Drawing.Size(200, 23);
            cbPreset.Name = "cbPreset";
            cbPreset.TabIndex = 6;
            cbPreset.SelectedIndexChanged += CbPreset_SelectedIndexChanged;
            // 
            // grpBackground
            // 
            grpBackground.Controls.Add(cbBackground);
            grpBackground.Controls.Add(pnlKeyColor);
            grpBackground.Controls.Add(lblTolerance);
            grpBackground.Controls.Add(numTolerance);
            grpBackground.Controls.Add(lblToleranceHint);
            grpBackground.Location = new System.Drawing.Point(4, 70);
            grpBackground.Size = new System.Drawing.Size(306, 78);
            grpBackground.TabStop = false;
            grpBackground.Text = "Hintergrund";
            grpBackground.Name = "grpBackground";
            grpBackground.TabIndex = 7;
            // 
            // cbBackground
            // 
            cbBackground.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbBackground.FormattingEnabled = true;
            cbBackground.Items.AddRange(new object[] { "nicht entfernen", "vom Rand aus entfernen", "Farbe überall entfernen" });
            cbBackground.Location = new System.Drawing.Point(8, 20);
            cbBackground.Size = new System.Drawing.Size(170, 23);
            cbBackground.Name = "cbBackground";
            cbBackground.TabIndex = 8;
            cbBackground.SelectedIndexChanged += Settings_Changed;
            // 
            // pnlKeyColor
            // 
            pnlKeyColor.BackColor = System.Drawing.Color.White;
            pnlKeyColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlKeyColor.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlKeyColor.Location = new System.Drawing.Point(186, 20);
            pnlKeyColor.Size = new System.Drawing.Size(30, 22);
            pnlKeyColor.Name = "pnlKeyColor";
            pnlKeyColor.TabIndex = 9;
            pnlKeyColor.Click += PnlColor_Click;
            // 
            // lblTolerance
            // 
            lblTolerance.AutoSize = true;
            lblTolerance.Location = new System.Drawing.Point(8, 51);
            lblTolerance.Text = "Toleranz:";
            lblTolerance.Name = "lblTolerance";
            // 
            // numTolerance
            // 
            numTolerance.Location = new System.Drawing.Point(80, 48);
            numTolerance.Size = new System.Drawing.Size(60, 23);
            numTolerance.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numTolerance.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numTolerance.Value = new decimal(new int[] { 30, 0, 0, 0 });
            numTolerance.Name = "numTolerance";
            numTolerance.TabIndex = 10;
            numTolerance.ValueChanged += Settings_Changed;
            // 
            // lblToleranceHint
            // 
            lblToleranceHint.AutoSize = true;
            lblToleranceHint.Location = new System.Drawing.Point(146, 51);
            lblToleranceHint.Text = "(Farbe: Klick ins Original)";
            lblToleranceHint.Name = "lblToleranceHint";
            // 
            // grpColors
            // 
            grpColors.Controls.Add(lblBrightness);
            grpColors.Controls.Add(numBrightness);
            grpColors.Controls.Add(lblContrast);
            grpColors.Controls.Add(numContrast);
            grpColors.Controls.Add(lblSaturation);
            grpColors.Controls.Add(numSaturation);
            grpColors.Controls.Add(chkGray);
            grpColors.Controls.Add(lblHuePreview);
            grpColors.Controls.Add(numHuePreview);
            grpColors.Controls.Add(lblHueOff);
            grpColors.Location = new System.Drawing.Point(4, 154);
            grpColors.Size = new System.Drawing.Size(306, 104);
            grpColors.TabStop = false;
            grpColors.Text = "Farben";
            grpColors.Name = "grpColors";
            grpColors.TabIndex = 11;
            // 
            // lblBrightness
            // 
            lblBrightness.AutoSize = true;
            lblBrightness.Location = new System.Drawing.Point(8, 23);
            lblBrightness.Text = "Helligkeit:";
            lblBrightness.Name = "lblBrightness";
            // 
            // numBrightness
            // 
            numBrightness.Location = new System.Drawing.Point(90, 20);
            numBrightness.Size = new System.Drawing.Size(60, 23);
            numBrightness.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numBrightness.Minimum = new decimal(new int[] { 100, 0, 0, -2147483648 });
            numBrightness.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numBrightness.Name = "numBrightness";
            numBrightness.TabIndex = 12;
            numBrightness.ValueChanged += Settings_Changed;
            // 
            // lblContrast
            // 
            lblContrast.AutoSize = true;
            lblContrast.Location = new System.Drawing.Point(156, 23);
            lblContrast.Text = "Kontrast:";
            lblContrast.Name = "lblContrast";
            // 
            // numContrast
            // 
            numContrast.Location = new System.Drawing.Point(230, 20);
            numContrast.Size = new System.Drawing.Size(60, 23);
            numContrast.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            numContrast.Minimum = new decimal(new int[] { 100, 0, 0, -2147483648 });
            numContrast.Value = new decimal(new int[] { 10, 0, 0, 0 });
            numContrast.Name = "numContrast";
            numContrast.TabIndex = 13;
            numContrast.ValueChanged += Settings_Changed;
            // 
            // lblSaturation
            // 
            lblSaturation.AutoSize = true;
            lblSaturation.Location = new System.Drawing.Point(8, 51);
            lblSaturation.Text = "Sättigung %:";
            lblSaturation.Name = "lblSaturation";
            // 
            // numSaturation
            // 
            numSaturation.Location = new System.Drawing.Point(90, 48);
            numSaturation.Size = new System.Drawing.Size(60, 23);
            numSaturation.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            numSaturation.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numSaturation.Value = new decimal(new int[] { 110, 0, 0, 0 });
            numSaturation.Name = "numSaturation";
            numSaturation.TabIndex = 14;
            numSaturation.ValueChanged += Settings_Changed;
            // 
            // chkGray
            // 
            chkGray.AutoSize = true;
            chkGray.Location = new System.Drawing.Point(156, 50);
            chkGray.Text = "Graustufen (für Hues)";
            chkGray.UseVisualStyleBackColor = true;
            chkGray.Name = "chkGray";
            chkGray.TabIndex = 15;
            chkGray.CheckedChanged += Settings_Changed;
            // 
            // lblHuePreview
            // 
            lblHuePreview.AutoSize = true;
            lblHuePreview.Location = new System.Drawing.Point(8, 79);
            lblHuePreview.Text = "Hue-Vorschau:";
            lblHuePreview.Name = "lblHuePreview";
            // 
            // numHuePreview
            // 
            numHuePreview.Location = new System.Drawing.Point(90, 76);
            numHuePreview.Size = new System.Drawing.Size(60, 23);
            numHuePreview.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numHuePreview.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHuePreview.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numHuePreview.Name = "numHuePreview";
            numHuePreview.TabIndex = 16;
            numHuePreview.ValueChanged += Settings_Changed;
            // 
            // lblHueOff
            // 
            lblHueOff.AutoSize = true;
            lblHueOff.Location = new System.Drawing.Point(156, 79);
            lblHueOff.Text = "(0 = aus)";
            lblHueOff.Name = "lblHueOff";
            // 
            // grpSize
            // 
            grpSize.Controls.Add(lblMaxW);
            grpSize.Controls.Add(numMaxW);
            grpSize.Controls.Add(lblMaxH);
            grpSize.Controls.Add(numMaxH);
            grpSize.Controls.Add(lblScale);
            grpSize.Controls.Add(cbScale);
            grpSize.Controls.Add(chkUpscale);
            grpSize.Controls.Add(lblIso);
            grpSize.Controls.Add(cbIso);
            grpSize.Controls.Add(lblIsoWidth);
            grpSize.Controls.Add(numIsoWidth);
            grpSize.Controls.Add(chkAutoCrop);
            grpSize.Location = new System.Drawing.Point(4, 264);
            grpSize.Size = new System.Drawing.Size(306, 132);
            grpSize.TabStop = false;
            grpSize.Text = "Größe & Isometrie";
            grpSize.Name = "grpSize";
            grpSize.TabIndex = 17;
            // 
            // lblMaxW
            // 
            lblMaxW.AutoSize = true;
            lblMaxW.Location = new System.Drawing.Point(8, 23);
            lblMaxW.Text = "Max. Breite:";
            lblMaxW.Name = "lblMaxW";
            // 
            // numMaxW
            // 
            numMaxW.Location = new System.Drawing.Point(90, 20);
            numMaxW.Size = new System.Drawing.Size(60, 23);
            numMaxW.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numMaxW.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMaxW.Value = new decimal(new int[] { 44, 0, 0, 0 });
            numMaxW.Name = "numMaxW";
            numMaxW.TabIndex = 18;
            numMaxW.ValueChanged += Settings_Changed;
            // 
            // lblMaxH
            // 
            lblMaxH.AutoSize = true;
            lblMaxH.Location = new System.Drawing.Point(156, 23);
            lblMaxH.Text = "Höhe:";
            lblMaxH.Name = "lblMaxH";
            // 
            // numMaxH
            // 
            numMaxH.Location = new System.Drawing.Point(200, 20);
            numMaxH.Size = new System.Drawing.Size(60, 23);
            numMaxH.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numMaxH.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numMaxH.Value = new decimal(new int[] { 80, 0, 0, 0 });
            numMaxH.Name = "numMaxH";
            numMaxH.TabIndex = 19;
            numMaxH.ValueChanged += Settings_Changed;
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(8, 51);
            lblScale.Text = "Skalierung:";
            lblScale.Name = "lblScale";
            // 
            // cbScale
            // 
            cbScale.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbScale.FormattingEnabled = true;
            cbScale.Items.AddRange(new object[] { "Weich (Bikubisch)", "Pixel (Nearest)", "Bilinear" });
            cbScale.Location = new System.Drawing.Point(90, 48);
            cbScale.Size = new System.Drawing.Size(120, 23);
            cbScale.Name = "cbScale";
            cbScale.TabIndex = 20;
            cbScale.SelectedIndexChanged += Settings_Changed;
            // 
            // chkUpscale
            // 
            chkUpscale.AutoSize = true;
            chkUpscale.Location = new System.Drawing.Point(216, 50);
            chkUpscale.Text = "vergrößern";
            chkUpscale.UseVisualStyleBackColor = true;
            chkUpscale.Name = "chkUpscale";
            chkUpscale.TabIndex = 21;
            chkUpscale.CheckedChanged += Settings_Changed;
            // 
            // lblIso
            // 
            lblIso.AutoSize = true;
            lblIso.Location = new System.Drawing.Point(8, 79);
            lblIso.Text = "Isometrie:";
            lblIso.Name = "lblIso";
            // 
            // cbIso
            // 
            cbIso.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbIso.FormattingEnabled = true;
            cbIso.Items.AddRange(new object[] { "keine", "Boden (Diamant)", "Wand links (\\)", "Wand rechts (/)" });
            cbIso.Location = new System.Drawing.Point(90, 76);
            cbIso.Size = new System.Drawing.Size(120, 23);
            cbIso.Name = "cbIso";
            cbIso.TabIndex = 22;
            cbIso.SelectedIndexChanged += Settings_Changed;
            // 
            // lblIsoWidth
            // 
            lblIsoWidth.AutoSize = true;
            lblIsoWidth.Location = new System.Drawing.Point(214, 79);
            lblIsoWidth.Text = "Breite:";
            lblIsoWidth.Name = "lblIsoWidth";
            // 
            // numIsoWidth
            // 
            numIsoWidth.Location = new System.Drawing.Point(256, 76);
            numIsoWidth.Size = new System.Drawing.Size(44, 23);
            numIsoWidth.Maximum = new decimal(new int[] { 440, 0, 0, 0 });
            numIsoWidth.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            numIsoWidth.Value = new decimal(new int[] { 44, 0, 0, 0 });
            numIsoWidth.Name = "numIsoWidth";
            numIsoWidth.TabIndex = 23;
            numIsoWidth.ValueChanged += Settings_Changed;
            // 
            // chkAutoCrop
            // 
            chkAutoCrop.Checked = true;
            chkAutoCrop.CheckState = System.Windows.Forms.CheckState.Checked;
            chkAutoCrop.AutoSize = true;
            chkAutoCrop.Location = new System.Drawing.Point(8, 104);
            chkAutoCrop.Text = "Leeren Rand abschneiden";
            chkAutoCrop.UseVisualStyleBackColor = true;
            chkAutoCrop.Name = "chkAutoCrop";
            chkAutoCrop.TabIndex = 24;
            chkAutoCrop.CheckedChanged += Settings_Changed;
            // 
            // grpPalette
            // 
            grpPalette.Controls.Add(lblColors);
            grpPalette.Controls.Add(numColors);
            grpPalette.Controls.Add(chkDither);
            grpPalette.Controls.Add(lblSharpen);
            grpPalette.Controls.Add(numSharpen);
            grpPalette.Controls.Add(lblOutline);
            grpPalette.Controls.Add(cbOutline);
            grpPalette.Controls.Add(pnlOutlineColor);
            grpPalette.Controls.Add(chkOutlineDiagonal);
            grpPalette.Location = new System.Drawing.Point(4, 402);
            grpPalette.Size = new System.Drawing.Size(306, 132);
            grpPalette.TabStop = false;
            grpPalette.Text = "Palette & Kanten";
            grpPalette.Name = "grpPalette";
            grpPalette.TabIndex = 25;
            // 
            // lblColors
            // 
            lblColors.AutoSize = true;
            lblColors.Location = new System.Drawing.Point(8, 23);
            lblColors.Text = "Farbanzahl:";
            lblColors.Name = "lblColors";
            // 
            // numColors
            // 
            numColors.Location = new System.Drawing.Point(90, 20);
            numColors.Size = new System.Drawing.Size(60, 23);
            numColors.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            numColors.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            numColors.Value = new decimal(new int[] { 32, 0, 0, 0 });
            numColors.Name = "numColors";
            numColors.TabIndex = 26;
            numColors.ValueChanged += Settings_Changed;
            // 
            // chkDither
            // 
            chkDither.AutoSize = true;
            chkDither.Location = new System.Drawing.Point(160, 22);
            chkDither.Text = "Dithering";
            chkDither.UseVisualStyleBackColor = true;
            chkDither.Name = "chkDither";
            chkDither.TabIndex = 27;
            chkDither.CheckedChanged += Settings_Changed;
            // 
            // lblSharpen
            // 
            lblSharpen.AutoSize = true;
            lblSharpen.Location = new System.Drawing.Point(8, 51);
            lblSharpen.Text = "Schärfen:";
            lblSharpen.Name = "lblSharpen";
            // 
            // numSharpen
            // 
            numSharpen.Location = new System.Drawing.Point(90, 48);
            numSharpen.Size = new System.Drawing.Size(60, 23);
            numSharpen.DecimalPlaces = 1;
            numSharpen.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numSharpen.Maximum = new decimal(new int[] { 2, 0, 0, 0 });
            numSharpen.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numSharpen.Value = new decimal(new int[] { 3, 0, 0, 65536 });
            numSharpen.Name = "numSharpen";
            numSharpen.TabIndex = 28;
            numSharpen.ValueChanged += Settings_Changed;
            // 
            // lblOutline
            // 
            lblOutline.AutoSize = true;
            lblOutline.Location = new System.Drawing.Point(8, 79);
            lblOutline.Text = "Outline:";
            lblOutline.Name = "lblOutline";
            // 
            // cbOutline
            // 
            cbOutline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbOutline.FormattingEnabled = true;
            cbOutline.Items.AddRange(new object[] { "keine", "außen (1px)", "innen (Kante)" });
            cbOutline.Location = new System.Drawing.Point(90, 76);
            cbOutline.Size = new System.Drawing.Size(120, 23);
            cbOutline.Name = "cbOutline";
            cbOutline.TabIndex = 29;
            cbOutline.SelectedIndexChanged += Settings_Changed;
            // 
            // pnlOutlineColor
            // 
            pnlOutlineColor.BackColor = System.Drawing.Color.FromArgb(24, 16, 8);
            pnlOutlineColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pnlOutlineColor.Cursor = System.Windows.Forms.Cursors.Hand;
            pnlOutlineColor.Location = new System.Drawing.Point(216, 76);
            pnlOutlineColor.Size = new System.Drawing.Size(30, 22);
            pnlOutlineColor.Name = "pnlOutlineColor";
            pnlOutlineColor.TabIndex = 30;
            pnlOutlineColor.Click += PnlColor_Click;
            // 
            // chkOutlineDiagonal
            // 
            chkOutlineDiagonal.AutoSize = true;
            chkOutlineDiagonal.Location = new System.Drawing.Point(8, 104);
            chkOutlineDiagonal.Text = "Diagonal";
            chkOutlineDiagonal.UseVisualStyleBackColor = true;
            chkOutlineDiagonal.Name = "chkOutlineDiagonal";
            chkOutlineDiagonal.TabIndex = 31;
            chkOutlineDiagonal.CheckedChanged += Settings_Changed;
            // 
            // grpExtra
            // 
            grpExtra.Controls.Add(lblPaletteSource);
            grpExtra.Controls.Add(cbPaletteSource);
            grpExtra.Controls.Add(txtPaletteId);
            grpExtra.Controls.Add(btnPaletteFile);
            grpExtra.Controls.Add(lblPaletteInfo);
            grpExtra.Controls.Add(chkShadow);
            grpExtra.Controls.Add(lblShadow);
            grpExtra.Controls.Add(numShadowX);
            grpExtra.Controls.Add(numShadowY);
            grpExtra.Controls.Add(chkShadowDither);
            grpExtra.Controls.Add(chkCompare);
            grpExtra.Controls.Add(trackCompare);
            grpExtra.Location = new System.Drawing.Point(4, 540);
            grpExtra.Size = new System.Drawing.Size(306, 140);
            grpExtra.TabStop = false;
            grpExtra.Text = "Palette-Quelle, Schatten & Vergleich";
            grpExtra.Name = "grpExtra";
            grpExtra.TabIndex = 32;
            // 
            // lblPaletteSource
            // 
            lblPaletteSource.AutoSize = true;
            lblPaletteSource.Location = new System.Drawing.Point(8, 23);
            lblPaletteSource.Text = "Palette:";
            lblPaletteSource.Name = "lblPaletteSource";
            // 
            // cbPaletteSource
            // 
            cbPaletteSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbPaletteSource.FormattingEnabled = true;
            cbPaletteSource.Items.AddRange(new object[] { "Optimiert (aus Bild)", "Aus Art-Item (ID)", "Aus Bilddatei" });
            cbPaletteSource.Location = new System.Drawing.Point(70, 20);
            cbPaletteSource.Size = new System.Drawing.Size(140, 23);
            cbPaletteSource.Name = "cbPaletteSource";
            cbPaletteSource.TabIndex = 33;
            cbPaletteSource.SelectedIndexChanged += Settings_Changed;
            // 
            // txtPaletteId
            // 
            txtPaletteId.Location = new System.Drawing.Point(216, 20);
            txtPaletteId.Size = new System.Drawing.Size(80, 23);
            txtPaletteId.Text = "0x0EED";
            txtPaletteId.Name = "txtPaletteId";
            txtPaletteId.TabIndex = 34;
            // 
            // btnPaletteFile
            // 
            btnPaletteFile.Location = new System.Drawing.Point(8, 48);
            btnPaletteFile.Size = new System.Drawing.Size(110, 26);
            btnPaletteFile.Text = "Palettenbild...";
            btnPaletteFile.UseVisualStyleBackColor = true;
            btnPaletteFile.Name = "btnPaletteFile";
            btnPaletteFile.TabIndex = 35;
            btnPaletteFile.Click += BtnPaletteFile_Click;
            // 
            // lblPaletteInfo
            // 
            lblPaletteInfo.AutoSize = true;
            lblPaletteInfo.Location = new System.Drawing.Point(124, 53);
            lblPaletteInfo.Text = "";
            lblPaletteInfo.Name = "lblPaletteInfo";
            // 
            // chkShadow
            // 
            chkShadow.AutoSize = true;
            chkShadow.Location = new System.Drawing.Point(8, 80);
            chkShadow.Text = "Schatten";
            chkShadow.UseVisualStyleBackColor = true;
            chkShadow.Name = "chkShadow";
            chkShadow.TabIndex = 36;
            chkShadow.CheckedChanged += Settings_Changed;
            // 
            // lblShadow
            // 
            lblShadow.AutoSize = true;
            lblShadow.Location = new System.Drawing.Point(88, 81);
            lblShadow.Text = "X/Y:";
            lblShadow.Name = "lblShadow";
            // 
            // numShadowX
            // 
            numShadowX.Location = new System.Drawing.Point(120, 78);
            numShadowX.Size = new System.Drawing.Size(44, 23);
            numShadowX.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numShadowX.Minimum = new decimal(new int[] { 20, 0, 0, -2147483648 });
            numShadowX.Value = new decimal(new int[] { 4, 0, 0, 0 });
            numShadowX.Name = "numShadowX";
            numShadowX.TabIndex = 37;
            numShadowX.ValueChanged += Settings_Changed;
            // 
            // numShadowY
            // 
            numShadowY.Location = new System.Drawing.Point(168, 78);
            numShadowY.Size = new System.Drawing.Size(44, 23);
            numShadowY.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            numShadowY.Minimum = new decimal(new int[] { 20, 0, 0, -2147483648 });
            numShadowY.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numShadowY.Name = "numShadowY";
            numShadowY.TabIndex = 38;
            numShadowY.ValueChanged += Settings_Changed;
            // 
            // chkShadowDither
            // 
            chkShadowDither.Checked = true;
            chkShadowDither.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShadowDither.AutoSize = true;
            chkShadowDither.Location = new System.Drawing.Point(220, 80);
            chkShadowDither.Text = "gerastert";
            chkShadowDither.UseVisualStyleBackColor = true;
            chkShadowDither.Name = "chkShadowDither";
            chkShadowDither.TabIndex = 39;
            chkShadowDither.CheckedChanged += Settings_Changed;
            // 
            // chkCompare
            // 
            chkCompare.AutoSize = true;
            chkCompare.Location = new System.Drawing.Point(8, 110);
            chkCompare.Text = "Vorher/Nachher";
            chkCompare.UseVisualStyleBackColor = true;
            chkCompare.Name = "chkCompare";
            chkCompare.TabIndex = 40;
            chkCompare.CheckedChanged += Compare_Changed;
            // 
            // trackCompare
            // 
            trackCompare.AutoSize = false;
            trackCompare.Location = new System.Drawing.Point(120, 106);
            trackCompare.Maximum = 100;
            trackCompare.Size = new System.Drawing.Size(180, 28);
            trackCompare.TickFrequency = 10;
            trackCompare.Value = 50;
            trackCompare.Name = "trackCompare";
            trackCompare.TabIndex = 41;
            trackCompare.Scroll += Compare_Changed;
            // 
            // grpExport
            // 
            grpExport.Controls.Add(lblId);
            grpExport.Controls.Add(txtId);
            grpExport.Controls.Add(btnFreeId);
            grpExport.Controls.Add(btnSetArt);
            grpExport.Controls.Add(btnSetLand);
            grpExport.Controls.Add(btnSetGump);
            grpExport.Controls.Add(btnSavePng);
            grpExport.Controls.Add(btnSaveArt);
            grpExport.Controls.Add(btnSaveGump);
            grpExport.Controls.Add(btnCopyResult);
            grpExport.Controls.Add(btnBatch);
            grpExport.Location = new System.Drawing.Point(4, 686);
            grpExport.Size = new System.Drawing.Size(306, 170);
            grpExport.TabStop = false;
            grpExport.Text = "Export";
            grpExport.Name = "grpExport";
            grpExport.TabIndex = 42;
            // 
            // lblId
            // 
            lblId.AutoSize = true;
            lblId.Location = new System.Drawing.Point(8, 25);
            lblId.Text = "ID (dez/0x):";
            lblId.Name = "lblId";
            // 
            // txtId
            // 
            txtId.Location = new System.Drawing.Point(90, 22);
            txtId.Size = new System.Drawing.Size(90, 23);
            txtId.Text = "0x0";
            txtId.Name = "txtId";
            txtId.TabIndex = 43;
            // 
            // btnFreeId
            // 
            btnFreeId.Location = new System.Drawing.Point(186, 20);
            btnFreeId.Size = new System.Drawing.Size(110, 26);
            btnFreeId.Text = "Freie ID";
            btnFreeId.UseVisualStyleBackColor = true;
            btnFreeId.Name = "btnFreeId";
            btnFreeId.TabIndex = 44;
            btnFreeId.Click += BtnFreeId_Click;
            // 
            // btnSetArt
            // 
            btnSetArt.Location = new System.Drawing.Point(8, 50);
            btnSetArt.Size = new System.Drawing.Size(140, 26);
            btnSetArt.Text = "Als Art-Item setzen";
            btnSetArt.UseVisualStyleBackColor = true;
            btnSetArt.Name = "btnSetArt";
            btnSetArt.TabIndex = 45;
            btnSetArt.Click += BtnSetArt_Click;
            // 
            // btnSetLand
            // 
            btnSetLand.Location = new System.Drawing.Point(152, 50);
            btnSetLand.Size = new System.Drawing.Size(144, 26);
            btnSetLand.Text = "Als Bodentile setzen";
            btnSetLand.UseVisualStyleBackColor = true;
            btnSetLand.Name = "btnSetLand";
            btnSetLand.TabIndex = 46;
            btnSetLand.Click += BtnSetLand_Click;
            // 
            // btnSetGump
            // 
            btnSetGump.Location = new System.Drawing.Point(8, 80);
            btnSetGump.Size = new System.Drawing.Size(140, 26);
            btnSetGump.Text = "Als Gump setzen";
            btnSetGump.UseVisualStyleBackColor = true;
            btnSetGump.Name = "btnSetGump";
            btnSetGump.TabIndex = 47;
            btnSetGump.Click += BtnSetGump_Click;
            // 
            // btnSavePng
            // 
            btnSavePng.Location = new System.Drawing.Point(152, 80);
            btnSavePng.Size = new System.Drawing.Size(144, 26);
            btnSavePng.Text = "PNG speichern...";
            btnSavePng.UseVisualStyleBackColor = true;
            btnSavePng.Name = "btnSavePng";
            btnSavePng.TabIndex = 48;
            btnSavePng.Click += BtnSavePng_Click;
            // 
            // btnSaveArt
            // 
            btnSaveArt.Location = new System.Drawing.Point(8, 110);
            btnSaveArt.Size = new System.Drawing.Size(140, 26);
            btnSaveArt.Text = "Art-MUL speichern";
            btnSaveArt.UseVisualStyleBackColor = true;
            btnSaveArt.Name = "btnSaveArt";
            btnSaveArt.TabIndex = 49;
            btnSaveArt.Click += BtnSaveArt_Click;
            // 
            // btnSaveGump
            // 
            btnSaveGump.Location = new System.Drawing.Point(152, 110);
            btnSaveGump.Size = new System.Drawing.Size(144, 26);
            btnSaveGump.Text = "Gump-MUL speichern";
            btnSaveGump.UseVisualStyleBackColor = true;
            btnSaveGump.Name = "btnSaveGump";
            btnSaveGump.TabIndex = 50;
            btnSaveGump.Click += BtnSaveGump_Click;
            // 
            // btnCopyResult
            // 
            btnCopyResult.Location = new System.Drawing.Point(8, 140);
            btnCopyResult.Size = new System.Drawing.Size(140, 26);
            btnCopyResult.Text = "Ergebnis kopieren";
            btnCopyResult.UseVisualStyleBackColor = true;
            btnCopyResult.Name = "btnCopyResult";
            btnCopyResult.TabIndex = 51;
            btnCopyResult.Click += BtnCopyResult_Click;
            // 
            // btnBatch
            // 
            btnBatch.Location = new System.Drawing.Point(152, 140);
            btnBatch.Size = new System.Drawing.Size(144, 26);
            btnBatch.Text = "Stapel umwandeln...";
            btnBatch.UseVisualStyleBackColor = true;
            btnBatch.Name = "btnBatch";
            btnBatch.TabIndex = 52;
            btnBatch.Click += BtnBatch_Click;
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = true;
            lblZoom.Location = new System.Drawing.Point(6, 865);
            lblZoom.Text = "Zoom Ergebnis:";
            lblZoom.Name = "lblZoom";
            // 
            // numZoom
            // 
            numZoom.Location = new System.Drawing.Point(110, 862);
            numZoom.Size = new System.Drawing.Size(60, 23);
            numZoom.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
            numZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numZoom.Value = new decimal(new int[] { 3, 0, 0, 0 });
            numZoom.Name = "numZoom";
            numZoom.TabIndex = 53;
            numZoom.ValueChanged += NumZoom_ValueChanged;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 54;
            // 
            // lblStatus
            // 
            lblStatus.Text = "Bild laden, um zu beginnen.";
            lblStatus.Name = "lblStatus";
            // 
            // timerDebounce
            // 
            timerDebounce.Interval = 250;
            timerDebounce.Tick += TimerDebounce_Tick;
            // 
            // PixelArtConverterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1164, 721);
            Controls.Add(splitViews);
            Controls.Add(pnlSettings);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(900, 600);
            Name = "PixelArtConverterForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Pixel-Art-Konverter (Bild → UO Item / Gump)";
            AllowDrop = true;
            KeyPreview = true;
            DragDrop += PixelArtConverterForm_DragDrop;
            DragEnter += PixelArtConverterForm_DragEnter;
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numZoom).EndInit();
            grpExport.ResumeLayout(false);
            grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackCompare).EndInit();
            ((System.ComponentModel.ISupportInitialize)numShadowY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numShadowX).EndInit();
            grpExtra.ResumeLayout(false);
            grpExtra.PerformLayout();
            pnlOutlineColor.ResumeLayout(false);
            pnlOutlineColor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numSharpen).EndInit();
            ((System.ComponentModel.ISupportInitialize)numColors).EndInit();
            grpPalette.ResumeLayout(false);
            grpPalette.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numIsoWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxH).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxW).EndInit();
            grpSize.ResumeLayout(false);
            grpSize.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHuePreview).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSaturation).EndInit();
            ((System.ComponentModel.ISupportInitialize)numContrast).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBrightness).EndInit();
            grpColors.ResumeLayout(false);
            grpColors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTolerance).EndInit();
            pnlKeyColor.ResumeLayout(false);
            pnlKeyColor.PerformLayout();
            grpBackground.ResumeLayout(false);
            grpBackground.PerformLayout();
            pnlSettings.ResumeLayout(false);
            pnlSettings.PerformLayout();
            splitViews.Panel1.ResumeLayout(false);
            splitViews.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitViews).EndInit();
            splitViews.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitViews;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewSource;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewResult;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnClipboard;
        private System.Windows.Forms.Label lblPreset;
        private System.Windows.Forms.ComboBox cbPreset;
        private System.Windows.Forms.GroupBox grpBackground;
        private System.Windows.Forms.ComboBox cbBackground;
        private System.Windows.Forms.Panel pnlKeyColor;
        private System.Windows.Forms.Label lblTolerance;
        private System.Windows.Forms.NumericUpDown numTolerance;
        private System.Windows.Forms.Label lblToleranceHint;
        private System.Windows.Forms.GroupBox grpColors;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.NumericUpDown numBrightness;
        private System.Windows.Forms.Label lblContrast;
        private System.Windows.Forms.NumericUpDown numContrast;
        private System.Windows.Forms.Label lblSaturation;
        private System.Windows.Forms.NumericUpDown numSaturation;
        private System.Windows.Forms.CheckBox chkGray;
        private System.Windows.Forms.Label lblHuePreview;
        private System.Windows.Forms.NumericUpDown numHuePreview;
        private System.Windows.Forms.Label lblHueOff;
        private System.Windows.Forms.GroupBox grpSize;
        private System.Windows.Forms.Label lblMaxW;
        private System.Windows.Forms.NumericUpDown numMaxW;
        private System.Windows.Forms.Label lblMaxH;
        private System.Windows.Forms.NumericUpDown numMaxH;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.ComboBox cbScale;
        private System.Windows.Forms.CheckBox chkUpscale;
        private System.Windows.Forms.Label lblIso;
        private System.Windows.Forms.ComboBox cbIso;
        private System.Windows.Forms.Label lblIsoWidth;
        private System.Windows.Forms.NumericUpDown numIsoWidth;
        private System.Windows.Forms.CheckBox chkAutoCrop;
        private System.Windows.Forms.GroupBox grpPalette;
        private System.Windows.Forms.Label lblColors;
        private System.Windows.Forms.NumericUpDown numColors;
        private System.Windows.Forms.CheckBox chkDither;
        private System.Windows.Forms.Label lblSharpen;
        private System.Windows.Forms.NumericUpDown numSharpen;
        private System.Windows.Forms.Label lblOutline;
        private System.Windows.Forms.ComboBox cbOutline;
        private System.Windows.Forms.Panel pnlOutlineColor;
        private System.Windows.Forms.CheckBox chkOutlineDiagonal;
        private System.Windows.Forms.GroupBox grpExtra;
        private System.Windows.Forms.Label lblPaletteSource;
        private System.Windows.Forms.ComboBox cbPaletteSource;
        private System.Windows.Forms.TextBox txtPaletteId;
        private System.Windows.Forms.Button btnPaletteFile;
        private System.Windows.Forms.Label lblPaletteInfo;
        private System.Windows.Forms.CheckBox chkShadow;
        private System.Windows.Forms.Label lblShadow;
        private System.Windows.Forms.NumericUpDown numShadowX;
        private System.Windows.Forms.NumericUpDown numShadowY;
        private System.Windows.Forms.CheckBox chkShadowDither;
        private System.Windows.Forms.CheckBox chkCompare;
        private System.Windows.Forms.TrackBar trackCompare;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.TextBox txtId;
        private System.Windows.Forms.Button btnFreeId;
        private System.Windows.Forms.Button btnSetArt;
        private System.Windows.Forms.Button btnSetLand;
        private System.Windows.Forms.Button btnSetGump;
        private System.Windows.Forms.Button btnSavePng;
        private System.Windows.Forms.Button btnSaveArt;
        private System.Windows.Forms.Button btnSaveGump;
        private System.Windows.Forms.Button btnCopyResult;
        private System.Windows.Forms.Button btnBatch;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.NumericUpDown numZoom;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer timerDebounce;
    }
}

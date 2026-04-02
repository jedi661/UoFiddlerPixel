// ============================================================
//  TextureWindowForm_Designer.cs  –  UoFiddler Texture / Tile Converter
//  Neu: RadioButtons für Tile-Hintergrundfarbe (Schwarz / Weiß)
//       im groupBoxMakeTile-Bereich ergänzt.
// ============================================================

namespace UoFiddler.Controls.Forms
{
    partial class TextureWindowForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureWindowForm));
            pictureBoxTexture = new System.Windows.Forms.PictureBox();
            contextMenuStripTexturen = new System.Windows.Forms.ContextMenuStrip(components);
            clipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            importToPrewiewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            mirrorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            triangleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btBackward = new System.Windows.Forms.Button();
            btForward = new System.Windows.Forms.Button();
            btMakeTile = new System.Windows.Forms.Button();
            btImageLeft = new System.Windows.Forms.Button();
            btImageRight = new System.Windows.Forms.Button();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            BtCreateTexture = new System.Windows.Forms.Button();
            lbTextureSize = new System.Windows.Forms.Label();
            lbIDNr = new System.Windows.Forms.Label();
            labelContrastValue = new System.Windows.Forms.Label();
            lblRotation = new System.Windows.Forms.Label();
            lblTextureSize = new System.Windows.Forms.Label();
            checkBoxLeft = new System.Windows.Forms.CheckBox();
            checkBoxRight = new System.Windows.Forms.CheckBox();
            checkBoxAntiAliasing = new System.Windows.Forms.CheckBox();
            checkBox64x64 = new System.Windows.Forms.CheckBox();
            checkBox128x128 = new System.Windows.Forms.CheckBox();
            trackBarColor = new System.Windows.Forms.TrackBar();
            panelTexture = new System.Windows.Forms.Panel();
            groupBoxMakeTile = new System.Windows.Forms.GroupBox();
            lblTileBg = new System.Windows.Forms.Label();
            radioButtonTileBgBlack = new System.Windows.Forms.RadioButton();
            radioButtonTileBgWhite = new System.Windows.Forms.RadioButton();
            groupBoxRotate = new System.Windows.Forms.GroupBox();
            groupBoxResize = new System.Windows.Forms.GroupBox();
            toolStripTexture = new System.Windows.Forms.ToolStrip();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            toolStripButtonImageLoad = new System.Windows.Forms.ToolStripButton();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            panelPreview = new System.Windows.Forms.Panel();
            IgPreviewClicked = new System.Windows.Forms.Button();
            previousButton = new System.Windows.Forms.Button();
            NextButton = new System.Windows.Forms.Button();
            btBackground = new System.Windows.Forms.Button();
            tBoxInfoColor = new System.Windows.Forms.TextBox();
            tbColorSet = new System.Windows.Forms.TextBox();
            btColorHex = new System.Windows.Forms.Button();
            btReplaceColor = new System.Windows.Forms.Button();
            btColorDialog = new System.Windows.Forms.Button();
            btCopyColorCode = new System.Windows.Forms.Button();
            rtBoxInfo = new System.Windows.Forms.RichTextBox();
            lblOldColor = new System.Windows.Forms.Label();
            lblNewColor = new System.Windows.Forms.Label();
            groupBoxColors = new System.Windows.Forms.GroupBox();
            lblColorHint = new System.Windows.Forms.Label();
            btToggleGrid = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).BeginInit();
            contextMenuStripTexturen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarColor).BeginInit();
            panelTexture.SuspendLayout();
            groupBoxMakeTile.SuspendLayout();
            groupBoxRotate.SuspendLayout();
            groupBoxResize.SuspendLayout();
            toolStripTexture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            panelPreview.SuspendLayout();
            groupBoxColors.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBoxTexture
            // 
            pictureBoxTexture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxTexture.ContextMenuStrip = contextMenuStripTexturen;
            pictureBoxTexture.Location = new System.Drawing.Point(8, 18);
            pictureBoxTexture.Name = "pictureBoxTexture";
            pictureBoxTexture.Size = new System.Drawing.Size(200, 160);
            pictureBoxTexture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxTexture.TabIndex = 0;
            pictureBoxTexture.TabStop = false;
            // 
            // contextMenuStripTexturen
            // 
            contextMenuStripTexturen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { clipboardToolStripMenuItem, importToolStripMenuItem, toolStripSeparator1, importToPrewiewToolStripMenuItem, toolStripSeparator2, mirrorToolStripMenuItem, triangleToolStripMenuItem, toolStripSeparator3, saveToolStripMenuItem });
            contextMenuStripTexturen.Name = "contextMenuStripTexturen";
            contextMenuStripTexturen.Size = new System.Drawing.Size(187, 154);
            // 
            // clipboardToolStripMenuItem
            // 
            clipboardToolStripMenuItem.Image = Properties.Resources.Clipbord;
            clipboardToolStripMenuItem.Name = "clipboardToolStripMenuItem";
            clipboardToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            clipboardToolStripMenuItem.Text = "Copy to Clipboard";
            clipboardToolStripMenuItem.ToolTipText = "Aktuelles Bild in die Zwischenablage kopieren.";
            clipboardToolStripMenuItem.Click += ClipboardToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            importToolStripMenuItem.Text = "Paste from Clipboard";
            importToolStripMenuItem.ToolTipText = "Bild aus Zwischenablage einfügen.";
            importToolStripMenuItem.Click += ImportToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // importToPrewiewToolStripMenuItem
            // 
            importToPrewiewToolStripMenuItem.Image = Properties.Resources.iishenar_map;
            importToPrewiewToolStripMenuItem.Name = "importToPrewiewToolStripMenuItem";
            importToPrewiewToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            importToPrewiewToolStripMenuItem.Text = "Paste to Preview Grid";
            importToPrewiewToolStripMenuItem.ToolTipText = "Zwischenablage-Bild als gekacheltes Rautenmuster im Vorschau-Panel anzeigen.";
            importToPrewiewToolStripMenuItem.Click += importToPrewiewToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
            // 
            // mirrorToolStripMenuItem
            // 
            mirrorToolStripMenuItem.Image = Properties.Resources.Mirror;
            mirrorToolStripMenuItem.Name = "mirrorToolStripMenuItem";
            mirrorToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            mirrorToolStripMenuItem.Text = "Mirror Horizontally";
            mirrorToolStripMenuItem.ToolTipText = "Bild horizontal spiegeln.";
            mirrorToolStripMenuItem.Click += mirrorToolStripMenuItem_Click;
            // 
            // triangleToolStripMenuItem
            // 
            triangleToolStripMenuItem.Image = Properties.Resources.triangle;
            triangleToolStripMenuItem.Name = "triangleToolStripMenuItem";
            triangleToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            triangleToolStripMenuItem.Text = "Apply Triangle Mask";
            triangleToolStripMenuItem.ToolTipText = "Untere Hälfte des Bildes mit Schwarz füllen.";
            triangleToolStripMenuItem.Click += triangleToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(183, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            saveToolStripMenuItem.Text = "Save As…";
            saveToolStripMenuItem.ToolTipText = "Bild als BMP, PNG, TIFF oder JPEG speichern.";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // btBackward
            // 
            btBackward.Location = new System.Drawing.Point(8, 225);
            btBackward.Name = "btBackward";
            btBackward.Size = new System.Drawing.Size(80, 26);
            btBackward.TabIndex = 2;
            btBackward.Text = "◄ Backward";
            btBackward.UseVisualStyleBackColor = true;
            btBackward.Click += BtBackward_Click;
            // 
            // btForward
            // 
            btForward.Location = new System.Drawing.Point(94, 225);
            btForward.Name = "btForward";
            btForward.Size = new System.Drawing.Size(80, 26);
            btForward.TabIndex = 3;
            btForward.Text = "Forward ►";
            btForward.UseVisualStyleBackColor = true;
            btForward.Click += BtForward_Click;
            // 
            // btMakeTile
            // 
            btMakeTile.Location = new System.Drawing.Point(6, 20);
            btMakeTile.Name = "btMakeTile";
            btMakeTile.Size = new System.Drawing.Size(80, 26);
            btMakeTile.TabIndex = 0;
            btMakeTile.Text = "Make Tile";
            btMakeTile.UseVisualStyleBackColor = true;
            btMakeTile.Click += BtMakeTile_Click;
            // 
            // btImageLeft
            // 
            btImageLeft.Location = new System.Drawing.Point(6, 20);
            btImageLeft.Name = "btImageLeft";
            btImageLeft.Size = new System.Drawing.Size(90, 26);
            btImageLeft.TabIndex = 0;
            btImageLeft.Text = "◄ Rotate 90°";
            btImageLeft.UseVisualStyleBackColor = true;
            btImageLeft.Click += btImageLeft_Click;
            // 
            // btImageRight
            // 
            btImageRight.Location = new System.Drawing.Point(102, 20);
            btImageRight.Name = "btImageRight";
            btImageRight.Size = new System.Drawing.Size(90, 26);
            btImageRight.TabIndex = 1;
            btImageRight.Text = "Rotate 90° ►";
            btImageRight.UseVisualStyleBackColor = true;
            btImageRight.Click += BtImageRight_Click;
            // 
            // buttonOpenTempGrafic
            // 
            buttonOpenTempGrafic.Location = new System.Drawing.Point(240, 462);
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Size = new System.Drawing.Size(90, 26);
            buttonOpenTempGrafic.TabIndex = 8;
            buttonOpenTempGrafic.Text = "Open Folder";
            buttonOpenTempGrafic.UseVisualStyleBackColor = true;
            buttonOpenTempGrafic.Click += ButtonOpenTempGrafic_Click;
            // 
            // BtCreateTexture
            // 
            BtCreateTexture.Location = new System.Drawing.Point(6, 20);
            BtCreateTexture.Name = "BtCreateTexture";
            BtCreateTexture.Size = new System.Drawing.Size(108, 26);
            BtCreateTexture.TabIndex = 0;
            BtCreateTexture.Text = "Create Texture";
            BtCreateTexture.UseVisualStyleBackColor = true;
            BtCreateTexture.Click += BtCreateTexture_Click;
            // 
            // lbTextureSize
            // 
            lbTextureSize.AutoSize = true;
            lbTextureSize.Location = new System.Drawing.Point(8, 185);
            lbTextureSize.Name = "lbTextureSize";
            lbTextureSize.Size = new System.Drawing.Size(39, 15);
            lbTextureSize.TabIndex = 1;
            lbTextureSize.Text = "Size: –";
            // 
            // lbIDNr
            // 
            lbIDNr.AutoSize = true;
            lbIDNr.Location = new System.Drawing.Point(8, 203);
            lbIDNr.Name = "lbIDNr";
            lbIDNr.Size = new System.Drawing.Size(30, 15);
            lbIDNr.TabIndex = 2;
            lbIDNr.Text = "ID: –";
            // 
            // labelContrastValue
            // 
            labelContrastValue.AutoSize = true;
            labelContrastValue.Location = new System.Drawing.Point(8, 443);
            labelContrastValue.Name = "labelContrastValue";
            labelContrastValue.Size = new System.Drawing.Size(64, 15);
            labelContrastValue.TabIndex = 8;
            labelContrastValue.Text = "Contrast: 0";
            // 
            // lblRotation
            // 
            lblRotation.Location = new System.Drawing.Point(0, 0);
            lblRotation.Name = "lblRotation";
            lblRotation.Size = new System.Drawing.Size(100, 23);
            lblRotation.TabIndex = 0;
            // 
            // lblTextureSize
            // 
            lblTextureSize.Location = new System.Drawing.Point(0, 0);
            lblTextureSize.Name = "lblTextureSize";
            lblTextureSize.Size = new System.Drawing.Size(100, 23);
            lblTextureSize.TabIndex = 0;
            // 
            // checkBoxLeft
            // 
            checkBoxLeft.AutoSize = true;
            checkBoxLeft.Checked = true;
            checkBoxLeft.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLeft.Location = new System.Drawing.Point(94, 23);
            checkBoxLeft.Name = "checkBoxLeft";
            checkBoxLeft.Size = new System.Drawing.Size(80, 19);
            checkBoxLeft.TabIndex = 1;
            checkBoxLeft.Text = "Left (–45°)";
            checkBoxLeft.UseVisualStyleBackColor = true;
            checkBoxLeft.CheckedChanged += CheckBoxLeft_CheckedChanged;
            // 
            // checkBoxRight
            // 
            checkBoxRight.AutoSize = true;
            checkBoxRight.Location = new System.Drawing.Point(182, 23);
            checkBoxRight.Name = "checkBoxRight";
            checkBoxRight.Size = new System.Drawing.Size(90, 19);
            checkBoxRight.TabIndex = 2;
            checkBoxRight.Text = "Right (+45°)";
            checkBoxRight.UseVisualStyleBackColor = true;
            checkBoxRight.CheckedChanged += CheckBoxRight_CheckedChanged;
            // 
            // checkBoxAntiAliasing
            // 
            checkBoxAntiAliasing.AutoSize = true;
            checkBoxAntiAliasing.Location = new System.Drawing.Point(207, 50);
            checkBoxAntiAliasing.Name = "checkBoxAntiAliasing";
            checkBoxAntiAliasing.Size = new System.Drawing.Size(78, 19);
            checkBoxAntiAliasing.TabIndex = 3;
            checkBoxAntiAliasing.Text = "Anti-Alias";
            checkBoxAntiAliasing.UseVisualStyleBackColor = true;
            // 
            // checkBox64x64
            // 
            checkBox64x64.AutoSize = true;
            checkBox64x64.Location = new System.Drawing.Point(122, 23);
            checkBox64x64.Name = "checkBox64x64";
            checkBox64x64.Size = new System.Drawing.Size(58, 19);
            checkBox64x64.TabIndex = 1;
            checkBox64x64.Text = "64×64";
            checkBox64x64.UseVisualStyleBackColor = true;
            checkBox64x64.CheckedChanged += checkBox64x64_CheckedChanged;
            // 
            // checkBox128x128
            // 
            checkBox128x128.AutoSize = true;
            checkBox128x128.Location = new System.Drawing.Point(184, 23);
            checkBox128x128.Name = "checkBox128x128";
            checkBox128x128.Size = new System.Drawing.Size(70, 19);
            checkBox128x128.TabIndex = 2;
            checkBox128x128.Text = "128×128";
            checkBox128x128.UseVisualStyleBackColor = true;
            checkBox128x128.CheckedChanged += checkBox128x128_CheckedChanged;
            // 
            // trackBarColor
            // 
            trackBarColor.Location = new System.Drawing.Point(8, 460);
            trackBarColor.Maximum = 128;
            trackBarColor.Minimum = -128;
            trackBarColor.Name = "trackBarColor";
            trackBarColor.Size = new System.Drawing.Size(220, 45);
            trackBarColor.TabIndex = 7;
            trackBarColor.TickFrequency = 16;
            trackBarColor.ValueChanged += trackBarContrast_ValueChanged;
            // 
            // panelTexture
            // 
            panelTexture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelTexture.Controls.Add(pictureBoxTexture);
            panelTexture.Controls.Add(lbTextureSize);
            panelTexture.Controls.Add(lbIDNr);
            panelTexture.Controls.Add(btBackward);
            panelTexture.Controls.Add(btForward);
            panelTexture.Controls.Add(groupBoxMakeTile);
            panelTexture.Controls.Add(groupBoxRotate);
            panelTexture.Controls.Add(groupBoxResize);
            panelTexture.Controls.Add(trackBarColor);
            panelTexture.Controls.Add(labelContrastValue);
            panelTexture.Controls.Add(buttonOpenTempGrafic);
            panelTexture.Location = new System.Drawing.Point(12, 30);
            panelTexture.Name = "panelTexture";
            panelTexture.Size = new System.Drawing.Size(340, 500);
            panelTexture.TabIndex = 1;
            // 
            // groupBoxMakeTile
            // 
            groupBoxMakeTile.Controls.Add(btMakeTile);
            groupBoxMakeTile.Controls.Add(checkBoxLeft);
            groupBoxMakeTile.Controls.Add(checkBoxRight);
            groupBoxMakeTile.Controls.Add(checkBoxAntiAliasing);
            groupBoxMakeTile.Controls.Add(lblTileBg);
            groupBoxMakeTile.Controls.Add(radioButtonTileBgBlack);
            groupBoxMakeTile.Controls.Add(radioButtonTileBgWhite);
            groupBoxMakeTile.Location = new System.Drawing.Point(8, 258);
            groupBoxMakeTile.Name = "groupBoxMakeTile";
            groupBoxMakeTile.Size = new System.Drawing.Size(322, 78);
            groupBoxMakeTile.TabIndex = 4;
            groupBoxMakeTile.TabStop = false;
            groupBoxMakeTile.Text = "Make Tile  (44×44 UO Land Tile)";
            // 
            // lblTileBg
            // 
            lblTileBg.AutoSize = true;
            lblTileBg.Location = new System.Drawing.Point(6, 52);
            lblTileBg.Name = "lblTileBg";
            lblTileBg.Size = new System.Drawing.Size(79, 15);
            lblTileBg.TabIndex = 10;
            lblTileBg.Text = "Außen-Farbe:";
            // 
            // radioButtonTileBgBlack
            // 
            radioButtonTileBgBlack.AutoSize = true;
            radioButtonTileBgBlack.Checked = true;
            radioButtonTileBgBlack.Location = new System.Drawing.Point(82, 50);
            radioButtonTileBgBlack.Name = "radioButtonTileBgBlack";
            radioButtonTileBgBlack.Size = new System.Drawing.Size(68, 19);
            radioButtonTileBgBlack.TabIndex = 11;
            radioButtonTileBgBlack.TabStop = true;
            radioButtonTileBgBlack.Text = "Schwarz";
            radioButtonTileBgBlack.UseVisualStyleBackColor = true;
            radioButtonTileBgBlack.CheckedChanged += RadioButtonTileBgBlack_CheckedChanged;
            // 
            // radioButtonTileBgWhite
            // 
            radioButtonTileBgWhite.AutoSize = true;
            radioButtonTileBgWhite.Location = new System.Drawing.Point(158, 50);
            radioButtonTileBgWhite.Name = "radioButtonTileBgWhite";
            radioButtonTileBgWhite.Size = new System.Drawing.Size(52, 19);
            radioButtonTileBgWhite.TabIndex = 12;
            radioButtonTileBgWhite.Text = "Weiß";
            radioButtonTileBgWhite.UseVisualStyleBackColor = true;
            radioButtonTileBgWhite.CheckedChanged += RadioButtonTileBgWhite_CheckedChanged;
            // 
            // groupBoxRotate
            // 
            groupBoxRotate.Controls.Add(btImageLeft);
            groupBoxRotate.Controls.Add(btImageRight);
            groupBoxRotate.Location = new System.Drawing.Point(8, 342);
            groupBoxRotate.Name = "groupBoxRotate";
            groupBoxRotate.Size = new System.Drawing.Size(322, 56);
            groupBoxRotate.TabIndex = 5;
            groupBoxRotate.TabStop = false;
            groupBoxRotate.Text = "Pre-Rotate Source";
            // 
            // groupBoxResize
            // 
            groupBoxResize.Controls.Add(BtCreateTexture);
            groupBoxResize.Controls.Add(checkBox64x64);
            groupBoxResize.Controls.Add(checkBox128x128);
            groupBoxResize.Location = new System.Drawing.Point(8, 404);
            groupBoxResize.Name = "groupBoxResize";
            groupBoxResize.Size = new System.Drawing.Size(322, 56);
            groupBoxResize.TabIndex = 6;
            groupBoxResize.TabStop = false;
            groupBoxResize.Text = "UO Texture Size  (64×64 / 128×128)";
            // 
            // toolStripTexture
            // 
            toolStripTexture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonSave, toolStripButtonImageLoad });
            toolStripTexture.Location = new System.Drawing.Point(0, 0);
            toolStripTexture.Name = "toolStripTexture";
            toolStripTexture.Size = new System.Drawing.Size(900, 25);
            toolStripTexture.TabIndex = 0;
            toolStripTexture.Text = "toolStripTexture";
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonSave.Image = Properties.Resources.save;
            toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            toolStripButtonSave.Text = "Quick Save";
            toolStripButtonSave.ToolTipText = "Bild als timestamped BMP in den Ordner tempGrafic speichern.";
            toolStripButtonSave.Click += toolStripButtonSave_Click;
            // 
            // toolStripButtonImageLoad
            // 
            toolStripButtonImageLoad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonImageLoad.Image = Properties.Resources.disk_load_image;
            toolStripButtonImageLoad.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonImageLoad.Name = "toolStripButtonImageLoad";
            toolStripButtonImageLoad.Size = new System.Drawing.Size(23, 22);
            toolStripButtonImageLoad.Text = "Load Image";
            toolStripButtonImageLoad.ToolTipText = "Bild von der Festplatte laden.";
            toolStripButtonImageLoad.Click += toolStripButtonImageLoad_Click;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.Location = new System.Drawing.Point(5, 5);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(508, 455);
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            // 
            // panelPreview
            // 
            panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelPreview.Controls.Add(btToggleGrid);
            panelPreview.Controls.Add(pictureBoxPreview);
            panelPreview.Controls.Add(IgPreviewClicked);
            panelPreview.Controls.Add(previousButton);
            panelPreview.Controls.Add(NextButton);
            panelPreview.Controls.Add(btBackground);
            panelPreview.Location = new System.Drawing.Point(360, 30);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new System.Drawing.Size(520, 500);
            panelPreview.TabIndex = 2;
            // 
            // IgPreviewClicked
            // 
            IgPreviewClicked.Location = new System.Drawing.Point(5, 465);
            IgPreviewClicked.Name = "IgPreviewClicked";
            IgPreviewClicked.Size = new System.Drawing.Size(72, 26);
            IgPreviewClicked.TabIndex = 1;
            IgPreviewClicked.Text = "Preview";
            IgPreviewClicked.UseVisualStyleBackColor = true;
            IgPreviewClicked.Click += IgPreviewClicked_Click;
            // 
            // previousButton
            // 
            previousButton.Location = new System.Drawing.Point(160, 465);
            previousButton.Name = "previousButton";
            previousButton.Size = new System.Drawing.Size(72, 26);
            previousButton.TabIndex = 2;
            previousButton.Text = "◄ Prev";
            previousButton.UseVisualStyleBackColor = true;
            previousButton.Click += previousButton_Click;
            // 
            // NextButton
            // 
            NextButton.Location = new System.Drawing.Point(238, 465);
            NextButton.Name = "NextButton";
            NextButton.Size = new System.Drawing.Size(72, 26);
            NextButton.TabIndex = 3;
            NextButton.Text = "Next ►";
            NextButton.UseVisualStyleBackColor = true;
            NextButton.Click += NextButton_Click;
            // 
            // btBackground
            // 
            btBackground.Location = new System.Drawing.Point(320, 465);
            btBackground.Name = "btBackground";
            btBackground.Size = new System.Drawing.Size(81, 26);
            btBackground.TabIndex = 4;
            btBackground.Text = "Background";
            btBackground.UseVisualStyleBackColor = true;
            btBackground.Click += BtBackground_Click;
            // 
            // tBoxInfoColor
            // 
            tBoxInfoColor.Location = new System.Drawing.Point(113, 22);
            tBoxInfoColor.Name = "tBoxInfoColor";
            tBoxInfoColor.PlaceholderText = "RRGGBB";
            tBoxInfoColor.Size = new System.Drawing.Size(90, 23);
            tBoxInfoColor.TabIndex = 1;
            // 
            // tbColorSet
            // 
            tbColorSet.Location = new System.Drawing.Point(243, 22);
            tbColorSet.Name = "tbColorSet";
            tbColorSet.PlaceholderText = "RRGGBB";
            tbColorSet.Size = new System.Drawing.Size(90, 23);
            tbColorSet.TabIndex = 2;
            // 
            // btColorHex
            // 
            btColorHex.Location = new System.Drawing.Point(728, 18);
            btColorHex.Name = "btColorHex";
            btColorHex.Size = new System.Drawing.Size(64, 26);
            btColorHex.TabIndex = 5;
            btColorHex.Text = "Analyze";
            btColorHex.UseVisualStyleBackColor = true;
            btColorHex.Click += BtColorHex_Click;
            // 
            // btReplaceColor
            // 
            btReplaceColor.Location = new System.Drawing.Point(336, 22);
            btReplaceColor.Name = "btReplaceColor";
            btReplaceColor.Size = new System.Drawing.Size(110, 23);
            btReplaceColor.TabIndex = 3;
            btReplaceColor.Text = "Replace Color";
            btReplaceColor.UseVisualStyleBackColor = true;
            btReplaceColor.Click += BtReplaceColor_Click;
            // 
            // btColorDialog
            // 
            btColorDialog.Location = new System.Drawing.Point(8, 22);
            btColorDialog.Name = "btColorDialog";
            btColorDialog.Size = new System.Drawing.Size(72, 23);
            btColorDialog.TabIndex = 0;
            btColorDialog.Text = "Pick Color";
            btColorDialog.UseVisualStyleBackColor = true;
            btColorDialog.Click += BtColorDialog_Click;
            // 
            // btCopyColorCode
            // 
            btCopyColorCode.Location = new System.Drawing.Point(728, 52);
            btCopyColorCode.Name = "btCopyColorCode";
            btCopyColorCode.Size = new System.Drawing.Size(64, 26);
            btCopyColorCode.TabIndex = 6;
            btCopyColorCode.Text = "Copy All";
            btCopyColorCode.UseVisualStyleBackColor = true;
            btCopyColorCode.Click += BtCopyColorCode_Click;
            // 
            // rtBoxInfo
            // 
            rtBoxInfo.Location = new System.Drawing.Point(460, 18);
            rtBoxInfo.Name = "rtBoxInfo";
            rtBoxInfo.ReadOnly = true;
            rtBoxInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            rtBoxInfo.Size = new System.Drawing.Size(260, 90);
            rtBoxInfo.TabIndex = 4;
            rtBoxInfo.Text = "";
            // 
            // lblOldColor
            // 
            lblOldColor.AutoSize = true;
            lblOldColor.Location = new System.Drawing.Point(81, 26);
            lblOldColor.Name = "lblOldColor";
            lblOldColor.Size = new System.Drawing.Size(29, 15);
            lblOldColor.TabIndex = 1;
            lblOldColor.Text = "Old:";
            // 
            // lblNewColor
            // 
            lblNewColor.AutoSize = true;
            lblNewColor.Location = new System.Drawing.Point(205, 26);
            lblNewColor.Name = "lblNewColor";
            lblNewColor.Size = new System.Drawing.Size(34, 15);
            lblNewColor.TabIndex = 2;
            lblNewColor.Text = "New:";
            // 
            // groupBoxColors
            // 
            groupBoxColors.Controls.Add(btColorDialog);
            groupBoxColors.Controls.Add(lblOldColor);
            groupBoxColors.Controls.Add(tBoxInfoColor);
            groupBoxColors.Controls.Add(lblNewColor);
            groupBoxColors.Controls.Add(tbColorSet);
            groupBoxColors.Controls.Add(btReplaceColor);
            groupBoxColors.Controls.Add(rtBoxInfo);
            groupBoxColors.Controls.Add(btColorHex);
            groupBoxColors.Controls.Add(btCopyColorCode);
            groupBoxColors.Controls.Add(lblColorHint);
            groupBoxColors.Location = new System.Drawing.Point(12, 548);
            groupBoxColors.Name = "groupBoxColors";
            groupBoxColors.Size = new System.Drawing.Size(868, 120);
            groupBoxColors.TabIndex = 3;
            groupBoxColors.TabStop = false;
            groupBoxColors.Text = "Color Analysis & Replacement";
            // 
            // lblColorHint
            // 
            lblColorHint.AutoSize = true;
            lblColorHint.ForeColor = System.Drawing.Color.Gray;
            lblColorHint.Location = new System.Drawing.Point(8, 56);
            lblColorHint.Name = "lblColorHint";
            lblColorHint.Size = new System.Drawing.Size(365, 15);
            lblColorHint.TabIndex = 7;
            lblColorHint.Text = "Click on the color field = select the hex code, then replace the color.";
            // 
            // btToggleGrid
            // 
            btToggleGrid.Location = new System.Drawing.Point(407, 465);
            btToggleGrid.Name = "btToggleGrid";
            btToggleGrid.Size = new System.Drawing.Size(105, 26);
            btToggleGrid.TabIndex = 9;
            btToggleGrid.Text = "Grid einblenden";
            btToggleGrid.UseVisualStyleBackColor = true;
            btToggleGrid.Click += BtToggleGrid_Click;
            // 
            // TextureWindowForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 680);
            Controls.Add(toolStripTexture);
            Controls.Add(panelTexture);
            Controls.Add(panelPreview);
            Controls.Add(groupBoxColors);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureWindowForm";
            Text = "Texture  /  Tile  Converter  –  Ultima Online 44×44";
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).EndInit();
            contextMenuStripTexturen.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trackBarColor).EndInit();
            panelTexture.ResumeLayout(false);
            panelTexture.PerformLayout();
            groupBoxMakeTile.ResumeLayout(false);
            groupBoxMakeTile.PerformLayout();
            groupBoxRotate.ResumeLayout(false);
            groupBoxResize.ResumeLayout(false);
            groupBoxResize.PerformLayout();
            toolStripTexture.ResumeLayout(false);
            toolStripTexture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            panelPreview.ResumeLayout(false);
            groupBoxColors.ResumeLayout(false);
            groupBoxColors.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        // ── Field declarations ─────────────────────────────────────────────
        private System.Windows.Forms.PictureBox pictureBoxTexture;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTexturen;
        private System.Windows.Forms.ToolStripMenuItem clipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToPrewiewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mirrorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem triangleToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.Button btBackward;
        private System.Windows.Forms.Button btForward;
        private System.Windows.Forms.Button btMakeTile;
        private System.Windows.Forms.Button btImageLeft;
        private System.Windows.Forms.Button btImageRight;
        private System.Windows.Forms.Button buttonOpenTempGrafic;
        private System.Windows.Forms.Button BtCreateTexture;
        private System.Windows.Forms.Label lbTextureSize;
        private System.Windows.Forms.Label lbIDNr;
        private System.Windows.Forms.Label labelContrastValue;
        private System.Windows.Forms.Label lblRotation;
        private System.Windows.Forms.Label lblTextureSize;
        private System.Windows.Forms.CheckBox checkBoxLeft;
        private System.Windows.Forms.CheckBox checkBoxRight;
        private System.Windows.Forms.CheckBox checkBoxAntiAliasing;
        private System.Windows.Forms.CheckBox checkBox64x64;
        private System.Windows.Forms.CheckBox checkBox128x128;
        private System.Windows.Forms.TrackBar trackBarColor;
        private System.Windows.Forms.Panel panelTexture;
        private System.Windows.Forms.GroupBox groupBoxMakeTile;
        private System.Windows.Forms.GroupBox groupBoxRotate;
        private System.Windows.Forms.GroupBox groupBoxResize;
        private System.Windows.Forms.ToolStrip toolStripTexture;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonImageLoad;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.Button IgPreviewClicked;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button btBackground;
        private System.Windows.Forms.TextBox tBoxInfoColor;
        private System.Windows.Forms.TextBox tbColorSet;
        private System.Windows.Forms.Button btColorHex;
        private System.Windows.Forms.Button btReplaceColor;
        private System.Windows.Forms.Button btColorDialog;
        private System.Windows.Forms.Button btCopyColorCode;
        private System.Windows.Forms.RichTextBox rtBoxInfo;
        private System.Windows.Forms.Label lblOldColor;
        private System.Windows.Forms.Label lblNewColor;
        private System.Windows.Forms.GroupBox groupBoxColors;
        private System.Windows.Forms.Label lblColorHint;

        // NEU
        private System.Windows.Forms.RadioButton radioButtonTileBgBlack;
        private System.Windows.Forms.RadioButton radioButtonTileBgWhite;
        private System.Windows.Forms.Label lblTileBg;
        private System.Windows.Forms.Button btToggleGrid;
    }
}
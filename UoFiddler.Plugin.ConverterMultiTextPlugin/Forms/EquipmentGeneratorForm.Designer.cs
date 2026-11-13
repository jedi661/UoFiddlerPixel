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
    partial class EquipmentGeneratorForm
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
            viewPaperdoll = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            viewGump = new UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView();
            pnlLeft = new System.Windows.Forms.Panel();
            grpSource = new System.Windows.Forms.GroupBox();
            btnLoadImage = new System.Windows.Forms.Button();
            lblArtSource = new System.Windows.Forms.Label();
            txtArtSource = new System.Windows.Forms.TextBox();
            btnLoadArt = new System.Windows.Forms.Button();
            btnLoadGump = new System.Windows.Forms.Button();
            grpFigure = new System.Windows.Forms.GroupBox();
            cbGender = new System.Windows.Forms.ComboBox();
            lblBodyGump = new System.Windows.Forms.Label();
            numBodyGump = new System.Windows.Forms.NumericUpDown();
            chkShowBody = new System.Windows.Forms.CheckBox();
            lblZoom = new System.Windows.Forms.Label();
            numZoom = new System.Windows.Forms.NumericUpDown();
            grpTransform = new System.Windows.Forms.GroupBox();
            lblOffX = new System.Windows.Forms.Label();
            numOffX = new System.Windows.Forms.NumericUpDown();
            lblOffY = new System.Windows.Forms.Label();
            numOffY = new System.Windows.Forms.NumericUpDown();
            lblScale = new System.Windows.Forms.Label();
            numScale = new System.Windows.Forms.NumericUpDown();
            lblRotate = new System.Windows.Forms.Label();
            numRotate = new System.Windows.Forms.NumericUpDown();
            chkFlip = new System.Windows.Forms.CheckBox();
            chkGray = new System.Windows.Forms.CheckBox();
            lblHue = new System.Windows.Forms.Label();
            numHue = new System.Windows.Forms.NumericUpDown();
            btnAutoPlace = new System.Windows.Forms.Button();
            chkAutoPlace = new System.Windows.Forms.CheckBox();
            grpTileData = new System.Windows.Forms.GroupBox();
            lblItemId = new System.Windows.Forms.Label();
            txtItemId = new System.Windows.Forms.TextBox();
            btnReadTileData = new System.Windows.Forms.Button();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblLayer = new System.Windows.Forms.Label();
            cbLayer = new System.Windows.Forms.ComboBox();
            lblAnim = new System.Windows.Forms.Label();
            numAnim = new System.Windows.Forms.NumericUpDown();
            btnFreeAnim = new System.Windows.Forms.Button();
            lblGumpIds = new System.Windows.Forms.Label();
            lblWeight = new System.Windows.Forms.Label();
            numWeight = new System.Windows.Forms.NumericUpDown();
            lblTileHue = new System.Windows.Forms.Label();
            numTileHue = new System.Windows.Forms.NumericUpDown();
            lblHeight = new System.Windows.Forms.Label();
            numHeight = new System.Windows.Forms.NumericUpDown();
            chkPartialHue = new System.Windows.Forms.CheckBox();
            chkFemale = new System.Windows.Forms.CheckBox();
            grpExport = new System.Windows.Forms.GroupBox();
            btnApplyAll = new System.Windows.Forms.Button();
            btnApplyGumps = new System.Windows.Forms.Button();
            btnApplyTileData = new System.Windows.Forms.Button();
            btnSaveGumps = new System.Windows.Forms.Button();
            btnSaveTileData = new System.Windows.Forms.Button();
            btnSaveGumpPng = new System.Windows.Forms.Button();
            btnCopyScript = new System.Windows.Forms.Button();
            lblHelp = new System.Windows.Forms.Label();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitViews).BeginInit();
            splitViews.Panel1.SuspendLayout();
            splitViews.Panel2.SuspendLayout();
            pnlLeft.SuspendLayout();
            grpSource.SuspendLayout();
            grpFigure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numBodyGump).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numZoom).BeginInit();
            grpTransform.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numOffX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numOffY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numRotate).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHue).BeginInit();
            grpTileData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numAnim).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numWeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTileHue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numHeight).BeginInit();
            grpExport.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitViews
            // 
            splitViews.Panel1.Controls.Add(viewPaperdoll);
            splitViews.Panel2.Controls.Add(viewGump);
            splitViews.Dock = System.Windows.Forms.DockStyle.Fill;
            splitViews.Location = new System.Drawing.Point(350, 0);
            splitViews.Size = new System.Drawing.Size(814, 739);
            splitViews.SplitterDistance = 560;
            splitViews.Name = "splitViews";
            splitViews.TabIndex = 0;
            // 
            // viewPaperdoll
            // 
            viewPaperdoll.Caption = "Paperdoll-Vorschau (Item mit der Maus verschieben)";
            viewPaperdoll.Dock = System.Windows.Forms.DockStyle.Fill;
            viewPaperdoll.Location = new System.Drawing.Point(0, 0);
            viewPaperdoll.Size = new System.Drawing.Size(560, 739);
            viewPaperdoll.Name = "viewPaperdoll";
            viewPaperdoll.TabIndex = 1;
            viewPaperdoll.MouseDown += ViewPaperdoll_MouseDown;
            viewPaperdoll.MouseMove += ViewPaperdoll_MouseMove;
            viewPaperdoll.MouseUp += ViewPaperdoll_MouseUp;
            // 
            // viewGump
            // 
            viewGump.Caption = "Item / Gump";
            viewGump.Dock = System.Windows.Forms.DockStyle.Fill;
            viewGump.Location = new System.Drawing.Point(0, 0);
            viewGump.Size = new System.Drawing.Size(250, 739);
            viewGump.Name = "viewGump";
            viewGump.TabIndex = 2;
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(grpSource);
            pnlLeft.Controls.Add(grpFigure);
            pnlLeft.Controls.Add(grpTransform);
            pnlLeft.Controls.Add(grpTileData);
            pnlLeft.Controls.Add(grpExport);
            pnlLeft.Controls.Add(lblHelp);
            pnlLeft.AutoScroll = true;
            pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            pnlLeft.Location = new System.Drawing.Point(0, 0);
            pnlLeft.Size = new System.Drawing.Size(350, 739);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.TabIndex = 3;
            // 
            // grpSource
            // 
            grpSource.Controls.Add(btnLoadImage);
            grpSource.Controls.Add(lblArtSource);
            grpSource.Controls.Add(txtArtSource);
            grpSource.Controls.Add(btnLoadArt);
            grpSource.Controls.Add(btnLoadGump);
            grpSource.Location = new System.Drawing.Point(4, 6);
            grpSource.Size = new System.Drawing.Size(326, 84);
            grpSource.TabStop = false;
            grpSource.Text = "Quelle";
            grpSource.Name = "grpSource";
            grpSource.TabIndex = 4;
            // 
            // btnLoadImage
            // 
            btnLoadImage.Location = new System.Drawing.Point(8, 20);
            btnLoadImage.Size = new System.Drawing.Size(100, 26);
            btnLoadImage.Text = "Bild laden...";
            btnLoadImage.UseVisualStyleBackColor = true;
            btnLoadImage.Name = "btnLoadImage";
            btnLoadImage.TabIndex = 5;
            btnLoadImage.Click += BtnLoadImage_Click;
            // 
            // lblArtSource
            // 
            lblArtSource.AutoSize = true;
            lblArtSource.Location = new System.Drawing.Point(116, 25);
            lblArtSource.Text = "Art-ID:";
            lblArtSource.Name = "lblArtSource";
            // 
            // txtArtSource
            // 
            txtArtSource.Location = new System.Drawing.Point(164, 22);
            txtArtSource.Size = new System.Drawing.Size(70, 23);
            txtArtSource.Text = "0x1415";
            txtArtSource.Name = "txtArtSource";
            txtArtSource.TabIndex = 6;
            // 
            // btnLoadArt
            // 
            btnLoadArt.Location = new System.Drawing.Point(240, 20);
            btnLoadArt.Size = new System.Drawing.Size(76, 26);
            btnLoadArt.Text = "aus Art";
            btnLoadArt.UseVisualStyleBackColor = true;
            btnLoadArt.Name = "btnLoadArt";
            btnLoadArt.TabIndex = 7;
            btnLoadArt.Click += BtnLoadArt_Click;
            // 
            // btnLoadGump
            // 
            btnLoadGump.Location = new System.Drawing.Point(8, 50);
            btnLoadGump.Size = new System.Drawing.Size(308, 26);
            btnLoadGump.Text = "Vorhandenes Gump laden (Anim-ID)";
            btnLoadGump.UseVisualStyleBackColor = true;
            btnLoadGump.Name = "btnLoadGump";
            btnLoadGump.TabIndex = 8;
            btnLoadGump.Click += BtnLoadGump_Click;
            // 
            // grpFigure
            // 
            grpFigure.Controls.Add(cbGender);
            grpFigure.Controls.Add(lblBodyGump);
            grpFigure.Controls.Add(numBodyGump);
            grpFigure.Controls.Add(chkShowBody);
            grpFigure.Controls.Add(lblZoom);
            grpFigure.Controls.Add(numZoom);
            grpFigure.Location = new System.Drawing.Point(4, 96);
            grpFigure.Size = new System.Drawing.Size(326, 80);
            grpFigure.TabStop = false;
            grpFigure.Text = "Figur";
            grpFigure.Name = "grpFigure";
            grpFigure.TabIndex = 9;
            // 
            // cbGender
            // 
            cbGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbGender.FormattingEnabled = true;
            cbGender.Items.AddRange(new object[] { "Männlich (0x0C)", "Weiblich (0x0D)", "Eigenes Gump" });
            cbGender.Location = new System.Drawing.Point(8, 20);
            cbGender.Size = new System.Drawing.Size(120, 23);
            cbGender.Name = "cbGender";
            cbGender.TabIndex = 10;
            cbGender.SelectedIndexChanged += CbGender_SelectedIndexChanged;
            // 
            // lblBodyGump
            // 
            lblBodyGump.AutoSize = true;
            lblBodyGump.Location = new System.Drawing.Point(136, 23);
            lblBodyGump.Text = "Body-Gump:";
            lblBodyGump.Name = "lblBodyGump";
            // 
            // numBodyGump
            // 
            numBodyGump.Location = new System.Drawing.Point(214, 20);
            numBodyGump.Size = new System.Drawing.Size(70, 23);
            numBodyGump.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numBodyGump.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numBodyGump.Value = new decimal(new int[] { 12, 0, 0, 0 });
            numBodyGump.Name = "numBodyGump";
            numBodyGump.TabIndex = 11;
            numBodyGump.ValueChanged += NumBodyGump_ValueChanged;
            // 
            // chkShowBody
            // 
            chkShowBody.Checked = true;
            chkShowBody.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowBody.AutoSize = true;
            chkShowBody.Location = new System.Drawing.Point(8, 52);
            chkShowBody.Text = "Figur anzeigen";
            chkShowBody.UseVisualStyleBackColor = true;
            chkShowBody.Name = "chkShowBody";
            chkShowBody.TabIndex = 12;
            chkShowBody.CheckedChanged += Compose_Changed;
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = true;
            lblZoom.Location = new System.Drawing.Point(136, 55);
            lblZoom.Text = "Zoom:";
            lblZoom.Name = "lblZoom";
            // 
            // numZoom
            // 
            numZoom.Location = new System.Drawing.Point(214, 50);
            numZoom.Size = new System.Drawing.Size(50, 23);
            numZoom.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
            numZoom.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numZoom.Value = new decimal(new int[] { 2, 0, 0, 0 });
            numZoom.Name = "numZoom";
            numZoom.TabIndex = 13;
            numZoom.ValueChanged += NumZoom_ValueChanged;
            // 
            // grpTransform
            // 
            grpTransform.Controls.Add(lblOffX);
            grpTransform.Controls.Add(numOffX);
            grpTransform.Controls.Add(lblOffY);
            grpTransform.Controls.Add(numOffY);
            grpTransform.Controls.Add(lblScale);
            grpTransform.Controls.Add(numScale);
            grpTransform.Controls.Add(lblRotate);
            grpTransform.Controls.Add(numRotate);
            grpTransform.Controls.Add(chkFlip);
            grpTransform.Controls.Add(chkGray);
            grpTransform.Controls.Add(lblHue);
            grpTransform.Controls.Add(numHue);
            grpTransform.Controls.Add(btnAutoPlace);
            grpTransform.Controls.Add(chkAutoPlace);
            grpTransform.Location = new System.Drawing.Point(4, 182);
            grpTransform.Size = new System.Drawing.Size(326, 170);
            grpTransform.TabStop = false;
            grpTransform.Text = "Position & Form";
            grpTransform.Name = "grpTransform";
            grpTransform.TabIndex = 14;
            // 
            // lblOffX
            // 
            lblOffX.AutoSize = true;
            lblOffX.Location = new System.Drawing.Point(8, 23);
            lblOffX.Text = "X:";
            lblOffX.Name = "lblOffX";
            // 
            // numOffX
            // 
            numOffX.Location = new System.Drawing.Point(30, 20);
            numOffX.Size = new System.Drawing.Size(60, 23);
            numOffX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numOffX.Minimum = new decimal(new int[] { 500, 0, 0, -2147483648 });
            numOffX.Value = new decimal(new int[] { 60, 0, 0, 0 });
            numOffX.Name = "numOffX";
            numOffX.TabIndex = 15;
            numOffX.ValueChanged += Compose_Changed;
            // 
            // lblOffY
            // 
            lblOffY.AutoSize = true;
            lblOffY.Location = new System.Drawing.Point(100, 23);
            lblOffY.Text = "Y:";
            lblOffY.Name = "lblOffY";
            // 
            // numOffY
            // 
            numOffY.Location = new System.Drawing.Point(120, 20);
            numOffY.Size = new System.Drawing.Size(60, 23);
            numOffY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numOffY.Minimum = new decimal(new int[] { 500, 0, 0, -2147483648 });
            numOffY.Value = new decimal(new int[] { 90, 0, 0, 0 });
            numOffY.Name = "numOffY";
            numOffY.TabIndex = 16;
            numOffY.ValueChanged += Compose_Changed;
            // 
            // lblScale
            // 
            lblScale.AutoSize = true;
            lblScale.Location = new System.Drawing.Point(8, 53);
            lblScale.Text = "Größe %:";
            lblScale.Name = "lblScale";
            // 
            // numScale
            // 
            numScale.Location = new System.Drawing.Point(70, 50);
            numScale.Size = new System.Drawing.Size(60, 23);
            numScale.Maximum = new decimal(new int[] { 800, 0, 0, 0 });
            numScale.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numScale.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numScale.Name = "numScale";
            numScale.TabIndex = 17;
            numScale.ValueChanged += Compose_Changed;
            // 
            // lblRotate
            // 
            lblRotate.AutoSize = true;
            lblRotate.Location = new System.Drawing.Point(140, 53);
            lblRotate.Text = "Drehen °:";
            lblRotate.Name = "lblRotate";
            // 
            // numRotate
            // 
            numRotate.Location = new System.Drawing.Point(204, 50);
            numRotate.Size = new System.Drawing.Size(60, 23);
            numRotate.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            numRotate.Minimum = new decimal(new int[] { 180, 0, 0, -2147483648 });
            numRotate.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numRotate.Name = "numRotate";
            numRotate.TabIndex = 18;
            numRotate.ValueChanged += Compose_Changed;
            // 
            // chkFlip
            // 
            chkFlip.AutoSize = true;
            chkFlip.Location = new System.Drawing.Point(8, 80);
            chkFlip.Text = "Spiegeln";
            chkFlip.UseVisualStyleBackColor = true;
            chkFlip.Name = "chkFlip";
            chkFlip.TabIndex = 19;
            chkFlip.CheckedChanged += Compose_Changed;
            // 
            // chkGray
            // 
            chkGray.AutoSize = true;
            chkGray.Location = new System.Drawing.Point(100, 80);
            chkGray.Text = "Graustufen (färbbar)";
            chkGray.UseVisualStyleBackColor = true;
            chkGray.Name = "chkGray";
            chkGray.TabIndex = 20;
            chkGray.CheckedChanged += Compose_Changed;
            // 
            // lblHue
            // 
            lblHue.AutoSize = true;
            lblHue.Location = new System.Drawing.Point(8, 111);
            lblHue.Text = "Hue-Vorschau:";
            lblHue.Name = "lblHue";
            // 
            // numHue
            // 
            numHue.Location = new System.Drawing.Point(100, 108);
            numHue.Size = new System.Drawing.Size(70, 23);
            numHue.Maximum = new decimal(new int[] { 3000, 0, 0, 0 });
            numHue.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numHue.Name = "numHue";
            numHue.TabIndex = 21;
            numHue.ValueChanged += Compose_Changed;
            // 
            // btnAutoPlace
            // 
            btnAutoPlace.Location = new System.Drawing.Point(8, 136);
            btnAutoPlace.Size = new System.Drawing.Size(190, 26);
            btnAutoPlace.Text = "Auto-Platzieren (nach Layer)";
            btnAutoPlace.UseVisualStyleBackColor = true;
            btnAutoPlace.Name = "btnAutoPlace";
            btnAutoPlace.TabIndex = 22;
            btnAutoPlace.Click += BtnAutoPlace_Click;
            // 
            // chkAutoPlace
            // 
            chkAutoPlace.Checked = true;
            chkAutoPlace.CheckState = System.Windows.Forms.CheckState.Checked;
            chkAutoPlace.AutoSize = true;
            chkAutoPlace.Location = new System.Drawing.Point(206, 140);
            chkAutoPlace.Text = "automatisch";
            chkAutoPlace.UseVisualStyleBackColor = true;
            chkAutoPlace.Name = "chkAutoPlace";
            chkAutoPlace.TabIndex = 23;
            // 
            // grpTileData
            // 
            grpTileData.Controls.Add(lblItemId);
            grpTileData.Controls.Add(txtItemId);
            grpTileData.Controls.Add(btnReadTileData);
            grpTileData.Controls.Add(lblName);
            grpTileData.Controls.Add(txtName);
            grpTileData.Controls.Add(lblLayer);
            grpTileData.Controls.Add(cbLayer);
            grpTileData.Controls.Add(lblAnim);
            grpTileData.Controls.Add(numAnim);
            grpTileData.Controls.Add(btnFreeAnim);
            grpTileData.Controls.Add(lblGumpIds);
            grpTileData.Controls.Add(lblWeight);
            grpTileData.Controls.Add(numWeight);
            grpTileData.Controls.Add(lblTileHue);
            grpTileData.Controls.Add(numTileHue);
            grpTileData.Controls.Add(lblHeight);
            grpTileData.Controls.Add(numHeight);
            grpTileData.Controls.Add(chkPartialHue);
            grpTileData.Controls.Add(chkFemale);
            grpTileData.Location = new System.Drawing.Point(4, 358);
            grpTileData.Size = new System.Drawing.Size(326, 222);
            grpTileData.TabStop = false;
            grpTileData.Text = "Tiledata-Eintrag";
            grpTileData.Name = "grpTileData";
            grpTileData.TabIndex = 24;
            // 
            // lblItemId
            // 
            lblItemId.AutoSize = true;
            lblItemId.Location = new System.Drawing.Point(8, 23);
            lblItemId.Text = "Item-ID:";
            lblItemId.Name = "lblItemId";
            // 
            // txtItemId
            // 
            txtItemId.Location = new System.Drawing.Point(90, 20);
            txtItemId.Size = new System.Drawing.Size(80, 23);
            txtItemId.Text = "0x1415";
            txtItemId.Name = "txtItemId";
            txtItemId.TabIndex = 25;
            // 
            // btnReadTileData
            // 
            btnReadTileData.Location = new System.Drawing.Point(176, 18);
            btnReadTileData.Size = new System.Drawing.Size(66, 26);
            btnReadTileData.Text = "Lesen";
            btnReadTileData.UseVisualStyleBackColor = true;
            btnReadTileData.Name = "btnReadTileData";
            btnReadTileData.TabIndex = 26;
            btnReadTileData.Click += BtnReadTileData_Click;
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
            txtName.Location = new System.Drawing.Point(90, 48);
            txtName.Size = new System.Drawing.Size(226, 23);
            txtName.Text = "new armor";
            txtName.MaxLength = 20;
            txtName.Name = "txtName";
            txtName.TabIndex = 27;
            // 
            // lblLayer
            // 
            lblLayer.AutoSize = true;
            lblLayer.Location = new System.Drawing.Point(8, 79);
            lblLayer.Text = "Layer:";
            lblLayer.Name = "lblLayer";
            // 
            // cbLayer
            // 
            cbLayer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbLayer.FormattingEnabled = true;
            cbLayer.Items.AddRange(new object[] { "0 - Ungültig", "1 - Einhand-Waffe", "2 - Zweihand / Schild", "3 - Schuhe", "4 - Hose", "5 - Hemd", "6 - Helm / Hut", "7 - Handschuhe", "8 - Ring", "9 - Talisman", "10 - Halskette", "11 - Haare", "12 - Gürtel / Schürze", "13 - Innerer Torso (Brust)", "14 - Armband", "15 - Gesicht", "16 - Bart", "17 - Mittlerer Torso (Tunika)", "18 - Ohrringe", "19 - Arme", "20 - Umhang", "21 - Rucksack", "22 - Äußerer Torso (Robe)", "23 - Äußere Beine (Rock)", "24 - Innere Beine", "25 - Reittier" });
            cbLayer.Location = new System.Drawing.Point(90, 76);
            cbLayer.Size = new System.Drawing.Size(226, 23);
            cbLayer.Name = "cbLayer";
            cbLayer.TabIndex = 28;
            cbLayer.SelectedIndexChanged += CbLayer_SelectedIndexChanged;
            // 
            // lblAnim
            // 
            lblAnim.AutoSize = true;
            lblAnim.Location = new System.Drawing.Point(8, 107);
            lblAnim.Text = "Animation:";
            lblAnim.Name = "lblAnim";
            // 
            // numAnim
            // 
            numAnim.Location = new System.Drawing.Point(90, 104);
            numAnim.Size = new System.Drawing.Size(70, 23);
            numAnim.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numAnim.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numAnim.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            numAnim.Name = "numAnim";
            numAnim.TabIndex = 29;
            numAnim.ValueChanged += NumAnim_ValueChanged;
            // 
            // btnFreeAnim
            // 
            btnFreeAnim.Location = new System.Drawing.Point(168, 102);
            btnFreeAnim.Size = new System.Drawing.Size(100, 26);
            btnFreeAnim.Text = "Freie suchen";
            btnFreeAnim.UseVisualStyleBackColor = true;
            btnFreeAnim.Name = "btnFreeAnim";
            btnFreeAnim.TabIndex = 30;
            btnFreeAnim.Click += BtnFreeAnim_Click;
            // 
            // lblGumpIds
            // 
            lblGumpIds.AutoSize = true;
            lblGumpIds.Location = new System.Drawing.Point(8, 133);
            lblGumpIds.Text = "Gump männlich: 51000   weiblich: 61000";
            lblGumpIds.Name = "lblGumpIds";
            // 
            // lblWeight
            // 
            lblWeight.AutoSize = true;
            lblWeight.Location = new System.Drawing.Point(8, 161);
            lblWeight.Text = "Gewicht:";
            lblWeight.Name = "lblWeight";
            // 
            // numWeight
            // 
            numWeight.Location = new System.Drawing.Point(70, 158);
            numWeight.Size = new System.Drawing.Size(50, 23);
            numWeight.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numWeight.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numWeight.Value = new decimal(new int[] { 5, 0, 0, 0 });
            numWeight.Name = "numWeight";
            numWeight.TabIndex = 31;
            // 
            // lblTileHue
            // 
            lblTileHue.AutoSize = true;
            lblTileHue.Location = new System.Drawing.Point(128, 161);
            lblTileHue.Text = "Hue:";
            lblTileHue.Name = "lblTileHue";
            // 
            // numTileHue
            // 
            numTileHue.Location = new System.Drawing.Point(160, 158);
            numTileHue.Size = new System.Drawing.Size(50, 23);
            numTileHue.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numTileHue.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numTileHue.Value = new decimal(new int[] { 0, 0, 0, 0 });
            numTileHue.Name = "numTileHue";
            numTileHue.TabIndex = 32;
            // 
            // lblHeight
            // 
            lblHeight.AutoSize = true;
            lblHeight.Location = new System.Drawing.Point(216, 161);
            lblHeight.Text = "Höhe:";
            lblHeight.Name = "lblHeight";
            // 
            // numHeight
            // 
            numHeight.Location = new System.Drawing.Point(256, 158);
            numHeight.Size = new System.Drawing.Size(50, 23);
            numHeight.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numHeight.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            numHeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numHeight.Name = "numHeight";
            numHeight.TabIndex = 33;
            // 
            // chkPartialHue
            // 
            chkPartialHue.Checked = true;
            chkPartialHue.CheckState = System.Windows.Forms.CheckState.Checked;
            chkPartialHue.AutoSize = true;
            chkPartialHue.Location = new System.Drawing.Point(8, 188);
            chkPartialHue.Text = "PartialHue";
            chkPartialHue.UseVisualStyleBackColor = true;
            chkPartialHue.Name = "chkPartialHue";
            chkPartialHue.TabIndex = 34;
            chkPartialHue.CheckedChanged += Compose_Changed;
            // 
            // chkFemale
            // 
            chkFemale.Checked = true;
            chkFemale.CheckState = System.Windows.Forms.CheckState.Checked;
            chkFemale.AutoSize = true;
            chkFemale.Location = new System.Drawing.Point(120, 188);
            chkFemale.Text = "auch weibliches Gump";
            chkFemale.UseVisualStyleBackColor = true;
            chkFemale.Name = "chkFemale";
            chkFemale.TabIndex = 35;
            // 
            // grpExport
            // 
            grpExport.Controls.Add(btnApplyAll);
            grpExport.Controls.Add(btnApplyGumps);
            grpExport.Controls.Add(btnApplyTileData);
            grpExport.Controls.Add(btnSaveGumps);
            grpExport.Controls.Add(btnSaveTileData);
            grpExport.Controls.Add(btnSaveGumpPng);
            grpExport.Controls.Add(btnCopyScript);
            grpExport.Location = new System.Drawing.Point(4, 586);
            grpExport.Size = new System.Drawing.Size(326, 150);
            grpExport.TabStop = false;
            grpExport.Text = "Export";
            grpExport.Name = "grpExport";
            grpExport.TabIndex = 36;
            // 
            // btnApplyAll
            // 
            btnApplyAll.Location = new System.Drawing.Point(8, 20);
            btnApplyAll.Size = new System.Drawing.Size(308, 26);
            btnApplyAll.Text = "Alles setzen (Gump + Tiledata)";
            btnApplyAll.UseVisualStyleBackColor = true;
            btnApplyAll.Name = "btnApplyAll";
            btnApplyAll.TabIndex = 37;
            btnApplyAll.Click += BtnApplyAll_Click;
            // 
            // btnApplyGumps
            // 
            btnApplyGumps.Location = new System.Drawing.Point(8, 50);
            btnApplyGumps.Size = new System.Drawing.Size(150, 26);
            btnApplyGumps.Text = "Nur Gump(s) setzen";
            btnApplyGumps.UseVisualStyleBackColor = true;
            btnApplyGumps.Name = "btnApplyGumps";
            btnApplyGumps.TabIndex = 38;
            btnApplyGumps.Click += BtnApplyGumps_Click;
            // 
            // btnApplyTileData
            // 
            btnApplyTileData.Location = new System.Drawing.Point(166, 50);
            btnApplyTileData.Size = new System.Drawing.Size(150, 26);
            btnApplyTileData.Text = "Nur Tiledata setzen";
            btnApplyTileData.UseVisualStyleBackColor = true;
            btnApplyTileData.Name = "btnApplyTileData";
            btnApplyTileData.TabIndex = 39;
            btnApplyTileData.Click += BtnApplyTileData_Click;
            // 
            // btnSaveGumps
            // 
            btnSaveGumps.Location = new System.Drawing.Point(8, 80);
            btnSaveGumps.Size = new System.Drawing.Size(150, 26);
            btnSaveGumps.Text = "Gump-MUL speichern";
            btnSaveGumps.UseVisualStyleBackColor = true;
            btnSaveGumps.Name = "btnSaveGumps";
            btnSaveGumps.TabIndex = 40;
            btnSaveGumps.Click += BtnSaveGumps_Click;
            // 
            // btnSaveTileData
            // 
            btnSaveTileData.Location = new System.Drawing.Point(166, 80);
            btnSaveTileData.Size = new System.Drawing.Size(150, 26);
            btnSaveTileData.Text = "tiledata.mul speichern";
            btnSaveTileData.UseVisualStyleBackColor = true;
            btnSaveTileData.Name = "btnSaveTileData";
            btnSaveTileData.TabIndex = 41;
            btnSaveTileData.Click += BtnSaveTileData_Click;
            // 
            // btnSaveGumpPng
            // 
            btnSaveGumpPng.Location = new System.Drawing.Point(8, 110);
            btnSaveGumpPng.Size = new System.Drawing.Size(150, 26);
            btnSaveGumpPng.Text = "Gump als PNG...";
            btnSaveGumpPng.UseVisualStyleBackColor = true;
            btnSaveGumpPng.Name = "btnSaveGumpPng";
            btnSaveGumpPng.TabIndex = 42;
            btnSaveGumpPng.Click += BtnSaveGumpPng_Click;
            // 
            // btnCopyScript
            // 
            btnCopyScript.Location = new System.Drawing.Point(166, 110);
            btnCopyScript.Size = new System.Drawing.Size(150, 26);
            btnCopyScript.Text = "Script-Snippet kopieren";
            btnCopyScript.UseVisualStyleBackColor = true;
            btnCopyScript.Name = "btnCopyScript";
            btnCopyScript.TabIndex = 43;
            btnCopyScript.Click += BtnCopyScript_Click;
            // 
            // lblHelp
            // 
            lblHelp.AutoSize = true;
            lblHelp.Location = new System.Drawing.Point(6, 744);
            lblHelp.MaximumSize = new System.Drawing.Size(320, 0);
            lblHelp.Text = "So funktioniert es:\r\n1. Item-Bild laden (Datei oder Art-ID).\r\n2. Layer wählen – das Bild wird automatisch an die passende Stelle der Figur gesetzt und skaliert.\r\n3. Mit Maus / X / Y / Größe / Drehen feinjustieren.\r\n4. Item-ID, Name und Animation eintragen (\u201eFreie suchen\u201c).\r\n5. \u201eAlles setzen\u201c erzeugt das Paperdoll-Gump (50000/60000 + Animation) und den Tiledata-Eintrag.\r\n6. Gump-MUL und tiledata.mul speichern.";
            lblHelp.Name = "lblHelp";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 44;
            // 
            // lblStatus
            // 
            lblStatus.Text = "Item-Bild laden (Datei oder aus Art).";
            lblStatus.Name = "lblStatus";
            // 
            // EquipmentGeneratorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1164, 761);
            Controls.Add(splitViews);
            Controls.Add(pnlLeft);
            Controls.Add(statusStrip);
            MinimumSize = new System.Drawing.Size(950, 650);
            Name = "EquipmentGeneratorForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Ausrüstungs-Generator (Item → Paperdoll-Gump + Tiledata)";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            grpExport.ResumeLayout(false);
            grpExport.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTileHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numWeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numAnim).EndInit();
            grpTileData.ResumeLayout(false);
            grpTileData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numHue).EndInit();
            ((System.ComponentModel.ISupportInitialize)numRotate).EndInit();
            ((System.ComponentModel.ISupportInitialize)numScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOffY).EndInit();
            ((System.ComponentModel.ISupportInitialize)numOffX).EndInit();
            grpTransform.ResumeLayout(false);
            grpTransform.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numZoom).EndInit();
            ((System.ComponentModel.ISupportInitialize)numBodyGump).EndInit();
            grpFigure.ResumeLayout(false);
            grpFigure.PerformLayout();
            grpSource.ResumeLayout(false);
            grpSource.PerformLayout();
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            splitViews.Panel1.ResumeLayout(false);
            splitViews.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitViews).EndInit();
            splitViews.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitViews;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewPaperdoll;
        private UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio.PixelView viewGump;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.GroupBox grpSource;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.Label lblArtSource;
        private System.Windows.Forms.TextBox txtArtSource;
        private System.Windows.Forms.Button btnLoadArt;
        private System.Windows.Forms.Button btnLoadGump;
        private System.Windows.Forms.GroupBox grpFigure;
        private System.Windows.Forms.ComboBox cbGender;
        private System.Windows.Forms.Label lblBodyGump;
        private System.Windows.Forms.NumericUpDown numBodyGump;
        private System.Windows.Forms.CheckBox chkShowBody;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.NumericUpDown numZoom;
        private System.Windows.Forms.GroupBox grpTransform;
        private System.Windows.Forms.Label lblOffX;
        private System.Windows.Forms.NumericUpDown numOffX;
        private System.Windows.Forms.Label lblOffY;
        private System.Windows.Forms.NumericUpDown numOffY;
        private System.Windows.Forms.Label lblScale;
        private System.Windows.Forms.NumericUpDown numScale;
        private System.Windows.Forms.Label lblRotate;
        private System.Windows.Forms.NumericUpDown numRotate;
        private System.Windows.Forms.CheckBox chkFlip;
        private System.Windows.Forms.CheckBox chkGray;
        private System.Windows.Forms.Label lblHue;
        private System.Windows.Forms.NumericUpDown numHue;
        private System.Windows.Forms.Button btnAutoPlace;
        private System.Windows.Forms.CheckBox chkAutoPlace;
        private System.Windows.Forms.GroupBox grpTileData;
        private System.Windows.Forms.Label lblItemId;
        private System.Windows.Forms.TextBox txtItemId;
        private System.Windows.Forms.Button btnReadTileData;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblLayer;
        private System.Windows.Forms.ComboBox cbLayer;
        private System.Windows.Forms.Label lblAnim;
        private System.Windows.Forms.NumericUpDown numAnim;
        private System.Windows.Forms.Button btnFreeAnim;
        private System.Windows.Forms.Label lblGumpIds;
        private System.Windows.Forms.Label lblWeight;
        private System.Windows.Forms.NumericUpDown numWeight;
        private System.Windows.Forms.Label lblTileHue;
        private System.Windows.Forms.NumericUpDown numTileHue;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.CheckBox chkPartialHue;
        private System.Windows.Forms.CheckBox chkFemale;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.Button btnApplyAll;
        private System.Windows.Forms.Button btnApplyGumps;
        private System.Windows.Forms.Button btnApplyTileData;
        private System.Windows.Forms.Button btnSaveGumps;
        private System.Windows.Forms.Button btnSaveTileData;
        private System.Windows.Forms.Button btnSaveGumpPng;
        private System.Windows.Forms.Button btnCopyScript;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}

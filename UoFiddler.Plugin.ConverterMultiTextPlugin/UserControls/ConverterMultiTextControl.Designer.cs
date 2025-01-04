/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UserControls
{
    partial class ConverterMultiTextControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            BtnMultiOpen = new System.Windows.Forms.Button();
            BtnSpeichernTxt = new System.Windows.Forms.Button();
            BtnUmwandeln = new System.Windows.Forms.Button();
            btnCopyTBox2 = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            textBox2 = new System.Windows.Forms.TextBox();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            label3 = new System.Windows.Forms.Label();
            TabControl1 = new System.Windows.Forms.TabControl();
            tabPageMain = new System.Windows.Forms.TabPage();
            lbIsoTiloSlicer = new System.Windows.Forms.Label();
            BtIsoTiloSlicer = new System.Windows.Forms.Button();
            lbAltitudeTool = new System.Windows.Forms.Label();
            BtAltitudeTool = new System.Windows.Forms.Button();
            tabPageAnimation = new System.Windows.Forms.TabPage();
            lbAnimationVD = new System.Windows.Forms.Label();
            lbAnimationEdit = new System.Windows.Forms.Label();
            BtAnimationVDForm = new System.Windows.Forms.Button();
            BtAnimationEditFormButton = new System.Windows.Forms.Button();
            tabPageGraphic = new System.Windows.Forms.TabPage();
            lbConverter = new System.Windows.Forms.Label();
            BtConverter = new System.Windows.Forms.Button();
            lbTransitions = new System.Windows.Forms.Label();
            BtTransitions = new System.Windows.Forms.Button();
            lbTileArtForm = new System.Windows.Forms.Label();
            BtTileArtForm = new System.Windows.Forms.Button();
            label5 = new System.Windows.Forms.Label();
            BtGumpIDRechner = new System.Windows.Forms.Button();
            lbUoArtMerge = new System.Windows.Forms.Label();
            BtUOArtMerge = new System.Windows.Forms.Button();
            lbGumpsEdit = new System.Windows.Forms.Label();
            lbTextureCutter = new System.Windows.Forms.Label();
            lbGraficCutter = new System.Windows.Forms.Label();
            BtGumpsEdit = new System.Windows.Forms.Button();
            ButtonGraficCutterForm = new System.Windows.Forms.Button();
            TextureCutter = new System.Windows.Forms.Button();
            tabPageMap = new System.Windows.Forms.TabPage();
            lbUoMap = new System.Windows.Forms.Label();
            UOMap = new System.Windows.Forms.Button();
            lbCopyMapReplace = new System.Windows.Forms.Label();
            BtMapReplace = new System.Windows.Forms.Button();
            lbMapMaker = new System.Windows.Forms.Label();
            BtMapMaker = new System.Windows.Forms.Button();
            tabPageClient = new System.Windows.Forms.TabPage();
            label4 = new System.Windows.Forms.Label();
            BtArtMul = new System.Windows.Forms.Button();
            lbDecriptClient = new System.Windows.Forms.Label();
            BtDecriptClient = new System.Windows.Forms.Button();
            tabPageScript = new System.Windows.Forms.TabPage();
            lbScriptCreator = new System.Windows.Forms.Label();
            btScriptCreator = new System.Windows.Forms.Button();
            tabPageTextureConverter = new System.Windows.Forms.TabPage();
            PanelMinMax = new System.Windows.Forms.Panel();
            TBMaximum = new System.Windows.Forms.TextBox();
            LBMaximum = new System.Windows.Forms.Label();
            TBMinimum = new System.Windows.Forms.TextBox();
            LBMinimum = new System.Windows.Forms.Label();
            LBResult = new System.Windows.Forms.Label();
            BtnGenerate = new System.Windows.Forms.Button();
            BtTest = new System.Windows.Forms.Button();
            Btclear = new System.Windows.Forms.Button();
            BtMorseCode = new System.Windows.Forms.Button();
            checkBoxASCII = new System.Windows.Forms.CheckBox();
            BtBinaryCode = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            timer1 = new System.Windows.Forms.Timer(components);
            contextMenuStrip1.SuspendLayout();
            TabControl1.SuspendLayout();
            tabPageMain.SuspendLayout();
            tabPageAnimation.SuspendLayout();
            tabPageGraphic.SuspendLayout();
            tabPageMap.SuspendLayout();
            tabPageClient.SuspendLayout();
            tabPageScript.SuspendLayout();
            tabPageTextureConverter.SuspendLayout();
            PanelMinMax.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // BtnMultiOpen
            // 
            BtnMultiOpen.Location = new System.Drawing.Point(338, 203);
            BtnMultiOpen.Name = "BtnMultiOpen";
            BtnMultiOpen.Size = new System.Drawing.Size(92, 23);
            BtnMultiOpen.TabIndex = 0;
            BtnMultiOpen.Text = "Open";
            BtnMultiOpen.UseVisualStyleBackColor = true;
            BtnMultiOpen.Click += BtnMultiOpen_Click;
            // 
            // BtnSpeichernTxt
            // 
            BtnSpeichernTxt.Location = new System.Drawing.Point(338, 174);
            BtnSpeichernTxt.Name = "BtnSpeichernTxt";
            BtnSpeichernTxt.Size = new System.Drawing.Size(92, 23);
            BtnSpeichernTxt.TabIndex = 1;
            BtnSpeichernTxt.Text = "Save";
            BtnSpeichernTxt.UseVisualStyleBackColor = true;
            BtnSpeichernTxt.Click += BtnSpeichernTxt_Click;
            // 
            // BtnUmwandeln
            // 
            BtnUmwandeln.Location = new System.Drawing.Point(338, 26);
            BtnUmwandeln.Name = "BtnUmwandeln";
            BtnUmwandeln.Size = new System.Drawing.Size(92, 23);
            BtnUmwandeln.TabIndex = 2;
            BtnUmwandeln.Text = "Convert";
            BtnUmwandeln.UseVisualStyleBackColor = true;
            BtnUmwandeln.Click += BtnUmwandeln_Click;
            // 
            // btnCopyTBox2
            // 
            btnCopyTBox2.Location = new System.Drawing.Point(716, 290);
            btnCopyTBox2.Name = "btnCopyTBox2";
            btnCopyTBox2.Size = new System.Drawing.Size(43, 23);
            btnCopyTBox2.TabIndex = 3;
            btnCopyTBox2.Text = "Copy";
            btnCopyTBox2.UseVisualStyleBackColor = true;
            btnCopyTBox2.Click += BtnCopyTBox2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 294);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(38, 15);
            label1.TabIndex = 4;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(441, 294);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(38, 15);
            label2.TabIndex = 5;
            label2.Text = "label2";
            // 
            // textBox1
            // 
            textBox1.ContextMenuStrip = contextMenuStrip1;
            textBox1.Location = new System.Drawing.Point(6, 6);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox1.Size = new System.Drawing.Size(318, 267);
            textBox1.TabIndex = 6;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToolStripMenuItem, clearToolStripMenuItem, importClipboardToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(166, 70);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += BtnCopyTBox2_Click;
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += Btclear_Click;
            // 
            // importClipboardToolStripMenuItem
            // 
            importClipboardToolStripMenuItem.Name = "importClipboardToolStripMenuItem";
            importClipboardToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            importClipboardToolStripMenuItem.Text = "Import Clipboard";
            importClipboardToolStripMenuItem.Click += ImportClipboardToolStripMenuItem_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(441, 6);
            textBox2.Multiline = true;
            textBox2.Name = "textBox2";
            textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox2.Size = new System.Drawing.Size(318, 267);
            textBox2.TabIndex = 7;
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(346, 6);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(76, 15);
            label3.TabIndex = 8;
            label3.Text = "to HexCode :";
            // 
            // TabControl1
            // 
            TabControl1.Controls.Add(tabPageMain);
            TabControl1.Controls.Add(tabPageAnimation);
            TabControl1.Controls.Add(tabPageGraphic);
            TabControl1.Controls.Add(tabPageMap);
            TabControl1.Controls.Add(tabPageClient);
            TabControl1.Controls.Add(tabPageScript);
            TabControl1.Controls.Add(tabPageTextureConverter);
            TabControl1.Location = new System.Drawing.Point(3, 27);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new System.Drawing.Size(968, 365);
            TabControl1.TabIndex = 15;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(lbIsoTiloSlicer);
            tabPageMain.Controls.Add(BtIsoTiloSlicer);
            tabPageMain.Controls.Add(lbAltitudeTool);
            tabPageMain.Controls.Add(BtAltitudeTool);
            tabPageMain.Location = new System.Drawing.Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            tabPageMain.Size = new System.Drawing.Size(960, 337);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Main";
            tabPageMain.UseVisualStyleBackColor = true;
            // 
            // lbIsoTiloSlicer
            // 
            lbIsoTiloSlicer.AutoSize = true;
            lbIsoTiloSlicer.Location = new System.Drawing.Point(3, 226);
            lbIsoTiloSlicer.Name = "lbIsoTiloSlicer";
            lbIsoTiloSlicer.Size = new System.Drawing.Size(69, 15);
            lbIsoTiloSlicer.TabIndex = 3;
            lbIsoTiloSlicer.Text = "IsoTiloSlicer";
            // 
            // BtIsoTiloSlicer
            // 
            BtIsoTiloSlicer.Image = Properties.Resources.grafic_cutter;
            BtIsoTiloSlicer.Location = new System.Drawing.Point(3, 168);
            BtIsoTiloSlicer.Name = "BtIsoTiloSlicer";
            BtIsoTiloSlicer.Size = new System.Drawing.Size(55, 55);
            BtIsoTiloSlicer.TabIndex = 2;
            BtIsoTiloSlicer.UseVisualStyleBackColor = true;
            BtIsoTiloSlicer.Click += BtIsoTiloSlicer_Click;
            // 
            // lbAltitudeTool
            // 
            lbAltitudeTool.AutoSize = true;
            lbAltitudeTool.Location = new System.Drawing.Point(3, 301);
            lbAltitudeTool.Name = "lbAltitudeTool";
            lbAltitudeTool.Size = new System.Drawing.Size(71, 15);
            lbAltitudeTool.TabIndex = 1;
            lbAltitudeTool.Text = "AltitudeTool";
            // 
            // BtAltitudeTool
            // 
            BtAltitudeTool.Image = Properties.Resources.altitude_tool;
            BtAltitudeTool.Location = new System.Drawing.Point(3, 243);
            BtAltitudeTool.Name = "BtAltitudeTool";
            BtAltitudeTool.Size = new System.Drawing.Size(55, 55);
            BtAltitudeTool.TabIndex = 0;
            BtAltitudeTool.UseVisualStyleBackColor = true;
            BtAltitudeTool.Click += BtAltitudeTool_Click;
            // 
            // tabPageAnimation
            // 
            tabPageAnimation.Controls.Add(lbAnimationVD);
            tabPageAnimation.Controls.Add(lbAnimationEdit);
            tabPageAnimation.Controls.Add(BtAnimationVDForm);
            tabPageAnimation.Controls.Add(BtAnimationEditFormButton);
            tabPageAnimation.Location = new System.Drawing.Point(4, 24);
            tabPageAnimation.Name = "tabPageAnimation";
            tabPageAnimation.Size = new System.Drawing.Size(960, 337);
            tabPageAnimation.TabIndex = 3;
            tabPageAnimation.Text = "Animation";
            tabPageAnimation.UseVisualStyleBackColor = true;
            // 
            // lbAnimationVD
            // 
            lbAnimationVD.AutoSize = true;
            lbAnimationVD.Location = new System.Drawing.Point(3, 153);
            lbAnimationVD.Name = "lbAnimationVD";
            lbAnimationVD.Size = new System.Drawing.Size(81, 15);
            lbAnimationVD.TabIndex = 18;
            lbAnimationVD.Text = "Animation VD";
            // 
            // lbAnimationEdit
            // 
            lbAnimationEdit.AutoSize = true;
            lbAnimationEdit.Location = new System.Drawing.Point(3, 71);
            lbAnimationEdit.Name = "lbAnimationEdit";
            lbAnimationEdit.Size = new System.Drawing.Size(86, 15);
            lbAnimationEdit.TabIndex = 17;
            lbAnimationEdit.Text = "Animation Edit";
            // 
            // BtAnimationVDForm
            // 
            BtAnimationVDForm.Image = Properties.Resources.animation_edit_3_;
            BtAnimationVDForm.Location = new System.Drawing.Point(3, 94);
            BtAnimationVDForm.Name = "BtAnimationVDForm";
            BtAnimationVDForm.Size = new System.Drawing.Size(59, 56);
            BtAnimationVDForm.TabIndex = 16;
            BtAnimationVDForm.UseVisualStyleBackColor = true;
            BtAnimationVDForm.Click += BtAnimationVDForm_Click;
            // 
            // BtAnimationEditFormButton
            // 
            BtAnimationEditFormButton.Image = Properties.Resources.animation_edit;
            BtAnimationEditFormButton.Location = new System.Drawing.Point(3, 12);
            BtAnimationEditFormButton.Name = "BtAnimationEditFormButton";
            BtAnimationEditFormButton.Size = new System.Drawing.Size(59, 56);
            BtAnimationEditFormButton.TabIndex = 15;
            BtAnimationEditFormButton.UseVisualStyleBackColor = true;
            BtAnimationEditFormButton.Click += BtAnimationEditFormButton_Click;
            // 
            // tabPageGraphic
            // 
            tabPageGraphic.Controls.Add(lbConverter);
            tabPageGraphic.Controls.Add(BtConverter);
            tabPageGraphic.Controls.Add(lbTransitions);
            tabPageGraphic.Controls.Add(BtTransitions);
            tabPageGraphic.Controls.Add(lbTileArtForm);
            tabPageGraphic.Controls.Add(BtTileArtForm);
            tabPageGraphic.Controls.Add(label5);
            tabPageGraphic.Controls.Add(BtGumpIDRechner);
            tabPageGraphic.Controls.Add(lbUoArtMerge);
            tabPageGraphic.Controls.Add(BtUOArtMerge);
            tabPageGraphic.Controls.Add(lbGumpsEdit);
            tabPageGraphic.Controls.Add(lbTextureCutter);
            tabPageGraphic.Controls.Add(lbGraficCutter);
            tabPageGraphic.Controls.Add(BtGumpsEdit);
            tabPageGraphic.Controls.Add(ButtonGraficCutterForm);
            tabPageGraphic.Controls.Add(TextureCutter);
            tabPageGraphic.Location = new System.Drawing.Point(4, 24);
            tabPageGraphic.Name = "tabPageGraphic";
            tabPageGraphic.Size = new System.Drawing.Size(960, 337);
            tabPageGraphic.TabIndex = 2;
            tabPageGraphic.Text = "Graphic";
            tabPageGraphic.UseVisualStyleBackColor = true;
            // 
            // lbConverter
            // 
            lbConverter.AutoSize = true;
            lbConverter.Location = new System.Drawing.Point(209, 260);
            lbConverter.Name = "lbConverter";
            lbConverter.Size = new System.Drawing.Size(59, 15);
            lbConverter.TabIndex = 26;
            lbConverter.Text = "Converter";
            // 
            // BtConverter
            // 
            BtConverter.Image = Properties.Resources.convert_image_1_;
            BtConverter.Location = new System.Drawing.Point(209, 193);
            BtConverter.Name = "BtConverter";
            BtConverter.Size = new System.Drawing.Size(56, 64);
            BtConverter.TabIndex = 25;
            BtConverter.UseVisualStyleBackColor = true;
            BtConverter.Click += BtConverter_Click;
            // 
            // lbTransitions
            // 
            lbTransitions.AutoSize = true;
            lbTransitions.Location = new System.Drawing.Point(106, 260);
            lbTransitions.Name = "lbTransitions";
            lbTransitions.Size = new System.Drawing.Size(63, 15);
            lbTransitions.TabIndex = 24;
            lbTransitions.Text = "Transitions";
            // 
            // BtTransitions
            // 
            BtTransitions.Image = Properties.Resources.transitions;
            BtTransitions.Location = new System.Drawing.Point(106, 193);
            BtTransitions.Name = "BtTransitions";
            BtTransitions.Size = new System.Drawing.Size(56, 64);
            BtTransitions.TabIndex = 23;
            BtTransitions.UseVisualStyleBackColor = true;
            BtTransitions.Click += BtTransitions_Click;
            // 
            // lbTileArtForm
            // 
            lbTileArtForm.AutoSize = true;
            lbTileArtForm.Location = new System.Drawing.Point(106, 173);
            lbTileArtForm.Name = "lbTileArtForm";
            lbTileArtForm.Size = new System.Drawing.Size(73, 15);
            lbTileArtForm.TabIndex = 22;
            lbTileArtForm.Text = "Land Tile Art";
            // 
            // BtTileArtForm
            // 
            BtTileArtForm.Image = Properties.Resources.tile_art_form;
            BtTileArtForm.Location = new System.Drawing.Point(106, 104);
            BtTileArtForm.Name = "BtTileArtForm";
            BtTileArtForm.Size = new System.Drawing.Size(56, 66);
            BtTileArtForm.TabIndex = 21;
            BtTileArtForm.UseVisualStyleBackColor = true;
            BtTileArtForm.Click += BtTileArtForm_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(209, 86);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(80, 15);
            label5.TabIndex = 20;
            label5.Text = "Gump ID Calc";
            // 
            // BtGumpIDRechner
            // 
            BtGumpIDRechner.Image = Properties.Resources.GumpID;
            BtGumpIDRechner.Location = new System.Drawing.Point(209, 19);
            BtGumpIDRechner.Name = "BtGumpIDRechner";
            BtGumpIDRechner.Size = new System.Drawing.Size(56, 64);
            BtGumpIDRechner.TabIndex = 19;
            BtGumpIDRechner.UseVisualStyleBackColor = true;
            BtGumpIDRechner.Click += BtGumpIDRechner_Click;
            // 
            // lbUoArtMerge
            // 
            lbUoArtMerge.AutoSize = true;
            lbUoArtMerge.Location = new System.Drawing.Point(106, 86);
            lbUoArtMerge.Name = "lbUoArtMerge";
            lbUoArtMerge.Size = new System.Drawing.Size(78, 15);
            lbUoArtMerge.TabIndex = 18;
            lbUoArtMerge.Text = "Uo Art Merge";
            // 
            // BtUOArtMerge
            // 
            BtUOArtMerge.Image = Properties.Resources.art_merge;
            BtUOArtMerge.Location = new System.Drawing.Point(106, 19);
            BtUOArtMerge.Name = "BtUOArtMerge";
            BtUOArtMerge.Size = new System.Drawing.Size(56, 64);
            BtUOArtMerge.TabIndex = 17;
            BtUOArtMerge.UseVisualStyleBackColor = true;
            BtUOArtMerge.Click += BtUOArtMerge_Click;
            // 
            // lbGumpsEdit
            // 
            lbGumpsEdit.AutoSize = true;
            lbGumpsEdit.Location = new System.Drawing.Point(3, 260);
            lbGumpsEdit.Name = "lbGumpsEdit";
            lbGumpsEdit.Size = new System.Drawing.Size(68, 15);
            lbGumpsEdit.TabIndex = 16;
            lbGumpsEdit.Text = "Gumps Edit";
            // 
            // lbTextureCutter
            // 
            lbTextureCutter.AutoSize = true;
            lbTextureCutter.Location = new System.Drawing.Point(3, 173);
            lbTextureCutter.Name = "lbTextureCutter";
            lbTextureCutter.Size = new System.Drawing.Size(81, 15);
            lbTextureCutter.TabIndex = 15;
            lbTextureCutter.Text = "Texture Cutter";
            // 
            // lbGraficCutter
            // 
            lbGraficCutter.AutoSize = true;
            lbGraficCutter.Location = new System.Drawing.Point(3, 86);
            lbGraficCutter.Name = "lbGraficCutter";
            lbGraficCutter.Size = new System.Drawing.Size(74, 15);
            lbGraficCutter.TabIndex = 14;
            lbGraficCutter.Text = "Grafic Cutter";
            // 
            // BtGumpsEdit
            // 
            BtGumpsEdit.Image = Properties.Resources.gump_edit_1_;
            BtGumpsEdit.Location = new System.Drawing.Point(3, 193);
            BtGumpsEdit.Name = "BtGumpsEdit";
            BtGumpsEdit.Size = new System.Drawing.Size(56, 64);
            BtGumpsEdit.TabIndex = 13;
            BtGumpsEdit.UseVisualStyleBackColor = true;
            BtGumpsEdit.Click += BtGumpsEdit_Click;
            // 
            // ButtonGraficCutterForm
            // 
            ButtonGraficCutterForm.Image = Properties.Resources.grafic_cutter_1_;
            ButtonGraficCutterForm.Location = new System.Drawing.Point(3, 19);
            ButtonGraficCutterForm.Name = "ButtonGraficCutterForm";
            ButtonGraficCutterForm.Size = new System.Drawing.Size(56, 64);
            ButtonGraficCutterForm.TabIndex = 11;
            ButtonGraficCutterForm.UseVisualStyleBackColor = true;
            ButtonGraficCutterForm.Click += ButtonGraficCutterForm_Click;
            // 
            // TextureCutter
            // 
            TextureCutter.Image = Properties.Resources.texture_cutter;
            TextureCutter.Location = new System.Drawing.Point(3, 106);
            TextureCutter.Name = "TextureCutter";
            TextureCutter.Size = new System.Drawing.Size(56, 64);
            TextureCutter.TabIndex = 12;
            TextureCutter.UseVisualStyleBackColor = true;
            TextureCutter.Click += TextureCutter_Click;
            // 
            // tabPageMap
            // 
            tabPageMap.Controls.Add(lbUoMap);
            tabPageMap.Controls.Add(UOMap);
            tabPageMap.Controls.Add(lbCopyMapReplace);
            tabPageMap.Controls.Add(BtMapReplace);
            tabPageMap.Controls.Add(lbMapMaker);
            tabPageMap.Controls.Add(BtMapMaker);
            tabPageMap.Location = new System.Drawing.Point(4, 24);
            tabPageMap.Name = "tabPageMap";
            tabPageMap.Size = new System.Drawing.Size(960, 337);
            tabPageMap.TabIndex = 4;
            tabPageMap.Text = "Map";
            tabPageMap.UseVisualStyleBackColor = true;
            // 
            // lbUoMap
            // 
            lbUoMap.AutoSize = true;
            lbUoMap.Location = new System.Drawing.Point(6, 234);
            lbUoMap.Name = "lbUoMap";
            lbUoMap.Size = new System.Drawing.Size(49, 15);
            lbUoMap.TabIndex = 18;
            lbUoMap.Text = "Uo Map";
            // 
            // UOMap
            // 
            UOMap.Image = Properties.Resources.ultima_online_map;
            UOMap.Location = new System.Drawing.Point(6, 172);
            UOMap.Name = "UOMap";
            UOMap.Size = new System.Drawing.Size(59, 57);
            UOMap.TabIndex = 17;
            UOMap.UseVisualStyleBackColor = true;
            UOMap.Click += UOMap_Click;
            // 
            // lbCopyMapReplace
            // 
            lbCopyMapReplace.AutoSize = true;
            lbCopyMapReplace.Location = new System.Drawing.Point(6, 151);
            lbCopyMapReplace.Name = "lbCopyMapReplace";
            lbCopyMapReplace.Size = new System.Drawing.Size(106, 15);
            lbCopyMapReplace.TabIndex = 16;
            lbCopyMapReplace.Text = "Copy Map Replace";
            // 
            // BtMapReplace
            // 
            BtMapReplace.Image = Properties.Resources.copy_a_map_and_replace;
            BtMapReplace.Location = new System.Drawing.Point(6, 91);
            BtMapReplace.Name = "BtMapReplace";
            BtMapReplace.Size = new System.Drawing.Size(59, 57);
            BtMapReplace.TabIndex = 15;
            BtMapReplace.UseVisualStyleBackColor = true;
            BtMapReplace.Click += BtMapReplace_Click;
            // 
            // lbMapMaker
            // 
            lbMapMaker.AutoSize = true;
            lbMapMaker.Location = new System.Drawing.Point(6, 70);
            lbMapMaker.Name = "lbMapMaker";
            lbMapMaker.Size = new System.Drawing.Size(67, 15);
            lbMapMaker.TabIndex = 14;
            lbMapMaker.Text = "Map Maker";
            // 
            // BtMapMaker
            // 
            BtMapMaker.Image = Properties.Resources.map_maker;
            BtMapMaker.Location = new System.Drawing.Point(6, 10);
            BtMapMaker.Name = "BtMapMaker";
            BtMapMaker.Size = new System.Drawing.Size(59, 57);
            BtMapMaker.TabIndex = 13;
            BtMapMaker.UseVisualStyleBackColor = true;
            BtMapMaker.Click += BtMapMaker_Click;
            // 
            // tabPageClient
            // 
            tabPageClient.Controls.Add(label4);
            tabPageClient.Controls.Add(BtArtMul);
            tabPageClient.Controls.Add(lbDecriptClient);
            tabPageClient.Controls.Add(BtDecriptClient);
            tabPageClient.Location = new System.Drawing.Point(4, 24);
            tabPageClient.Name = "tabPageClient";
            tabPageClient.Size = new System.Drawing.Size(960, 337);
            tabPageClient.TabIndex = 5;
            tabPageClient.Text = "Client";
            tabPageClient.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(92, 75);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(65, 15);
            label4.TabIndex = 15;
            label4.Text = "Create Mul";
            // 
            // BtArtMul
            // 
            BtArtMul.Image = Properties.Resources.create_art_abd_mul_file2;
            BtArtMul.Location = new System.Drawing.Point(92, 13);
            BtArtMul.Name = "BtArtMul";
            BtArtMul.Size = new System.Drawing.Size(56, 59);
            BtArtMul.TabIndex = 14;
            BtArtMul.UseVisualStyleBackColor = true;
            BtArtMul.Click += BtArtMul_Click;
            // 
            // lbDecriptClient
            // 
            lbDecriptClient.AutoSize = true;
            lbDecriptClient.Location = new System.Drawing.Point(3, 75);
            lbDecriptClient.Name = "lbDecriptClient";
            lbDecriptClient.Size = new System.Drawing.Size(76, 15);
            lbDecriptClient.TabIndex = 13;
            lbDecriptClient.Text = "DecriptClient";
            // 
            // BtDecriptClient
            // 
            BtDecriptClient.Image = Properties.Resources.decript;
            BtDecriptClient.Location = new System.Drawing.Point(3, 13);
            BtDecriptClient.Name = "BtDecriptClient";
            BtDecriptClient.Size = new System.Drawing.Size(56, 59);
            BtDecriptClient.TabIndex = 12;
            BtDecriptClient.UseVisualStyleBackColor = true;
            BtDecriptClient.Click += BtDecriptClient_Click;
            // 
            // tabPageScript
            // 
            tabPageScript.Controls.Add(lbScriptCreator);
            tabPageScript.Controls.Add(btScriptCreator);
            tabPageScript.Location = new System.Drawing.Point(4, 24);
            tabPageScript.Name = "tabPageScript";
            tabPageScript.Padding = new System.Windows.Forms.Padding(3);
            tabPageScript.Size = new System.Drawing.Size(960, 337);
            tabPageScript.TabIndex = 6;
            tabPageScript.Text = "Scripts";
            tabPageScript.UseVisualStyleBackColor = true;
            // 
            // lbScriptCreator
            // 
            lbScriptCreator.AutoSize = true;
            lbScriptCreator.Location = new System.Drawing.Point(6, 75);
            lbScriptCreator.Name = "lbScriptCreator";
            lbScriptCreator.Size = new System.Drawing.Size(79, 15);
            lbScriptCreator.TabIndex = 1;
            lbScriptCreator.Text = "Script Creator";
            // 
            // btScriptCreator
            // 
            btScriptCreator.BackColor = System.Drawing.Color.Transparent;
            btScriptCreator.Image = Properties.Resources.script_creator;
            btScriptCreator.Location = new System.Drawing.Point(6, 6);
            btScriptCreator.Name = "btScriptCreator";
            btScriptCreator.Size = new System.Drawing.Size(55, 66);
            btScriptCreator.TabIndex = 0;
            btScriptCreator.UseVisualStyleBackColor = false;
            btScriptCreator.Click += BtScriptCreator_Click;
            // 
            // tabPageTextureConverter
            // 
            tabPageTextureConverter.Controls.Add(PanelMinMax);
            tabPageTextureConverter.Controls.Add(BtnGenerate);
            tabPageTextureConverter.Controls.Add(BtTest);
            tabPageTextureConverter.Controls.Add(Btclear);
            tabPageTextureConverter.Controls.Add(BtMorseCode);
            tabPageTextureConverter.Controls.Add(checkBoxASCII);
            tabPageTextureConverter.Controls.Add(BtBinaryCode);
            tabPageTextureConverter.Controls.Add(textBox1);
            tabPageTextureConverter.Controls.Add(BtnMultiOpen);
            tabPageTextureConverter.Controls.Add(BtnSpeichernTxt);
            tabPageTextureConverter.Controls.Add(BtnUmwandeln);
            tabPageTextureConverter.Controls.Add(btnCopyTBox2);
            tabPageTextureConverter.Controls.Add(label1);
            tabPageTextureConverter.Controls.Add(label2);
            tabPageTextureConverter.Controls.Add(label3);
            tabPageTextureConverter.Controls.Add(textBox2);
            tabPageTextureConverter.Location = new System.Drawing.Point(4, 24);
            tabPageTextureConverter.Name = "tabPageTextureConverter";
            tabPageTextureConverter.Padding = new System.Windows.Forms.Padding(3);
            tabPageTextureConverter.Size = new System.Drawing.Size(960, 337);
            tabPageTextureConverter.TabIndex = 1;
            tabPageTextureConverter.Text = "Text Converter";
            tabPageTextureConverter.UseVisualStyleBackColor = true;
            // 
            // PanelMinMax
            // 
            PanelMinMax.Controls.Add(TBMaximum);
            PanelMinMax.Controls.Add(LBMaximum);
            PanelMinMax.Controls.Add(TBMinimum);
            PanelMinMax.Controls.Add(LBMinimum);
            PanelMinMax.Controls.Add(LBResult);
            PanelMinMax.Location = new System.Drawing.Point(767, 6);
            PanelMinMax.Name = "PanelMinMax";
            PanelMinMax.Size = new System.Drawing.Size(154, 267);
            PanelMinMax.TabIndex = 20;
            // 
            // TBMaximum
            // 
            TBMaximum.Location = new System.Drawing.Point(14, 139);
            TBMaximum.Name = "TBMaximum";
            TBMaximum.Size = new System.Drawing.Size(100, 23);
            TBMaximum.TabIndex = 15;
            TBMaximum.Text = "100";
            TBMaximum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            TBMaximum.KeyPress += TB_KeyPress;
            // 
            // LBMaximum
            // 
            LBMaximum.AutoSize = true;
            LBMaximum.Location = new System.Drawing.Point(14, 165);
            LBMaximum.Name = "LBMaximum";
            LBMaximum.Size = new System.Drawing.Size(62, 15);
            LBMaximum.TabIndex = 19;
            LBMaximum.Text = "Maximum";
            // 
            // TBMinimum
            // 
            TBMinimum.Location = new System.Drawing.Point(14, 78);
            TBMinimum.Name = "TBMinimum";
            TBMinimum.Size = new System.Drawing.Size(100, 23);
            TBMinimum.TabIndex = 14;
            TBMinimum.Text = "1";
            TBMinimum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            TBMinimum.KeyPress += TB_KeyPress;
            // 
            // LBMinimum
            // 
            LBMinimum.AutoSize = true;
            LBMinimum.Location = new System.Drawing.Point(14, 104);
            LBMinimum.Name = "LBMinimum";
            LBMinimum.Size = new System.Drawing.Size(60, 15);
            LBMinimum.TabIndex = 18;
            LBMinimum.Text = "Minimum";
            // 
            // LBResult
            // 
            LBResult.AutoSize = true;
            LBResult.Location = new System.Drawing.Point(14, 28);
            LBResult.Name = "LBResult";
            LBResult.Size = new System.Drawing.Size(39, 15);
            LBResult.TabIndex = 16;
            LBResult.Text = "Result";
            // 
            // BtnGenerate
            // 
            BtnGenerate.Location = new System.Drawing.Point(794, 290);
            BtnGenerate.Name = "BtnGenerate";
            BtnGenerate.Size = new System.Drawing.Size(75, 23);
            BtnGenerate.TabIndex = 17;
            BtnGenerate.Text = "Generate";
            BtnGenerate.UseVisualStyleBackColor = true;
            BtnGenerate.Click += BtnGenerate_Click;
            // 
            // BtTest
            // 
            BtTest.Location = new System.Drawing.Point(338, 232);
            BtTest.Name = "BtTest";
            BtTest.Size = new System.Drawing.Size(92, 23);
            BtTest.TabIndex = 13;
            BtTest.Text = "Test";
            BtTest.UseVisualStyleBackColor = true;
            BtTest.Click += BtTest_Click;
            // 
            // Btclear
            // 
            Btclear.Location = new System.Drawing.Point(284, 290);
            Btclear.Name = "Btclear";
            Btclear.Size = new System.Drawing.Size(40, 23);
            Btclear.TabIndex = 12;
            Btclear.Text = "clear";
            Btclear.UseVisualStyleBackColor = true;
            Btclear.Click += Btclear_Click;
            // 
            // BtMorseCode
            // 
            BtMorseCode.Location = new System.Drawing.Point(338, 84);
            BtMorseCode.Name = "BtMorseCode";
            BtMorseCode.Size = new System.Drawing.Size(92, 23);
            BtMorseCode.TabIndex = 11;
            BtMorseCode.Text = "Morse code";
            BtMorseCode.UseVisualStyleBackColor = true;
            BtMorseCode.Click += BtMorseCode_Click;
            // 
            // checkBoxASCII
            // 
            checkBoxASCII.AutoSize = true;
            checkBoxASCII.Location = new System.Drawing.Point(338, 149);
            checkBoxASCII.Name = "checkBoxASCII";
            checkBoxASCII.Size = new System.Drawing.Size(94, 19);
            checkBoxASCII.TabIndex = 10;
            checkBoxASCII.Text = "back original";
            checkBoxASCII.UseVisualStyleBackColor = true;
            // 
            // BtBinaryCode
            // 
            BtBinaryCode.Location = new System.Drawing.Point(338, 55);
            BtBinaryCode.Name = "BtBinaryCode";
            BtBinaryCode.Size = new System.Drawing.Size(92, 23);
            BtBinaryCode.TabIndex = 9;
            BtBinaryCode.Text = "Binary Code";
            BtBinaryCode.UseVisualStyleBackColor = true;
            BtBinaryCode.Click += BtBinaryCode_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(974, 24);
            menuStrip1.TabIndex = 16;
            menuStrip1.Text = "menuStrip1";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabelTime });
            statusStrip1.Location = new System.Drawing.Point(0, 395);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(974, 22);
            statusStrip1.TabIndex = 17;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabelTime
            // 
            toolStripStatusLabelTime.Name = "toolStripStatusLabelTime";
            toolStripStatusLabelTime.Size = new System.Drawing.Size(33, 17);
            toolStripStatusLabelTime.Text = "Time";
            // 
            // ConverterMultiTextControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(statusStrip1);
            Controls.Add(TabControl1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ConverterMultiTextControl";
            Size = new System.Drawing.Size(974, 417);
            contextMenuStrip1.ResumeLayout(false);
            TabControl1.ResumeLayout(false);
            tabPageMain.ResumeLayout(false);
            tabPageMain.PerformLayout();
            tabPageAnimation.ResumeLayout(false);
            tabPageAnimation.PerformLayout();
            tabPageGraphic.ResumeLayout(false);
            tabPageGraphic.PerformLayout();
            tabPageMap.ResumeLayout(false);
            tabPageMap.PerformLayout();
            tabPageClient.ResumeLayout(false);
            tabPageClient.PerformLayout();
            tabPageScript.ResumeLayout(false);
            tabPageScript.PerformLayout();
            tabPageTextureConverter.ResumeLayout(false);
            tabPageTextureConverter.PerformLayout();
            PanelMinMax.ResumeLayout(false);
            PanelMinMax.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button BtnMultiOpen;
        private System.Windows.Forms.Button BtnSpeichernTxt;
        private System.Windows.Forms.Button BtnUmwandeln;
        private System.Windows.Forms.Button btnCopyTBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl TabControl1;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageTextureConverter;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TabPage tabPageGraphic;
        private System.Windows.Forms.Button ButtonGraficCutterForm;
        private System.Windows.Forms.Button TextureCutter;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.Button BtAnimationVDForm;
        private System.Windows.Forms.Button BtAnimationEditFormButton;
        private System.Windows.Forms.TabPage tabPageMap;
        private System.Windows.Forms.Button BtMapMaker;
        private System.Windows.Forms.TabPage tabPageClient;
        private System.Windows.Forms.Button BtDecriptClient;
        private System.Windows.Forms.Button BtGumpsEdit;
        private System.Windows.Forms.Button BtAltitudeTool;
        private System.Windows.Forms.Label lbGraficCutter;
        private System.Windows.Forms.Label lbMapMaker;
        private System.Windows.Forms.Label lbDecriptClient;
        private System.Windows.Forms.Label lbAltitudeTool;
        private System.Windows.Forms.Label lbAnimationVD;
        private System.Windows.Forms.Label lbAnimationEdit;
        private System.Windows.Forms.Label lbGumpsEdit;
        private System.Windows.Forms.Label lbTextureCutter;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button BtBinaryCode;
        private System.Windows.Forms.CheckBox checkBoxASCII;
        private System.Windows.Forms.Button BtMorseCode;
        private System.Windows.Forms.Button Btclear;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importClipboardToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPageScript;
        private System.Windows.Forms.Button BtMapReplace;
        private System.Windows.Forms.Label lbCopyMapReplace;
        private System.Windows.Forms.Button BtArtMul;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btScriptCreator;
        private System.Windows.Forms.Label lbScriptCreator;
        private System.Windows.Forms.Button BtUOArtMerge;
        private System.Windows.Forms.Label lbUoArtMerge;
        private System.Windows.Forms.Button BtGumpIDRechner;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BtIsoTiloSlicer;
        private System.Windows.Forms.Label lbIsoTiloSlicer;
        private System.Windows.Forms.Button UOMap;
        private System.Windows.Forms.Label lbUoMap;
        private System.Windows.Forms.Button BtTileArtForm;
        private System.Windows.Forms.Label lbTileArtForm;
        private System.Windows.Forms.Label lbTransitions;
        private System.Windows.Forms.Button BtTransitions;
        private System.Windows.Forms.Button BtTest;
        private System.Windows.Forms.Label lbConverter;
        private System.Windows.Forms.Button BtConverter;
        private System.Windows.Forms.Button BtnGenerate;
        private System.Windows.Forms.Label LBResult;
        private System.Windows.Forms.TextBox TBMaximum;
        private System.Windows.Forms.TextBox TBMinimum;
        private System.Windows.Forms.Label LBMinimum;
        private System.Windows.Forms.Label LBMaximum;
        private System.Windows.Forms.Panel PanelMinMax;       
    }
}

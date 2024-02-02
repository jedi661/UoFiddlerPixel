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
            btnSpeichernTxt = new System.Windows.Forms.Button();
            btnUmwandeln = new System.Windows.Forms.Button();
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
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPageMain = new System.Windows.Forms.TabPage();
            btIsoTiloSlicer = new System.Windows.Forms.Button();
            lbAltitudeTool = new System.Windows.Forms.Label();
            btAltitudeTool = new System.Windows.Forms.Button();
            tabPageAnimation = new System.Windows.Forms.TabPage();
            lbAnimationVD = new System.Windows.Forms.Label();
            lbAnimationEdit = new System.Windows.Forms.Label();
            btAnimationVDForm = new System.Windows.Forms.Button();
            btAnimationEditFormButton = new System.Windows.Forms.Button();
            tabPageGraphic = new System.Windows.Forms.TabPage();
            label5 = new System.Windows.Forms.Label();
            btGumpIDRechner = new System.Windows.Forms.Button();
            lbUoArtMerge = new System.Windows.Forms.Label();
            btUOArtMerge = new System.Windows.Forms.Button();
            lbGumpsEdit = new System.Windows.Forms.Label();
            lbTextureCutter = new System.Windows.Forms.Label();
            lbGraficCutter = new System.Windows.Forms.Label();
            btGumpsEdit = new System.Windows.Forms.Button();
            buttonGraficCutterForm = new System.Windows.Forms.Button();
            TextureCutter = new System.Windows.Forms.Button();
            tabPageMap = new System.Windows.Forms.TabPage();
            lbCopyMapReplace = new System.Windows.Forms.Label();
            btMapReplace = new System.Windows.Forms.Button();
            lbMapMaker = new System.Windows.Forms.Label();
            btMapMaker = new System.Windows.Forms.Button();
            tabPageClient = new System.Windows.Forms.TabPage();
            label4 = new System.Windows.Forms.Label();
            btArtMul = new System.Windows.Forms.Button();
            lbDecriptClient = new System.Windows.Forms.Label();
            btDecriptClient = new System.Windows.Forms.Button();
            tabPageScript = new System.Windows.Forms.TabPage();
            lbScriptCreator = new System.Windows.Forms.Label();
            btScriptCreator = new System.Windows.Forms.Button();
            tabPageTextureConverter = new System.Windows.Forms.TabPage();
            btclear = new System.Windows.Forms.Button();
            btMorseCode = new System.Windows.Forms.Button();
            checkBoxASCII = new System.Windows.Forms.CheckBox();
            btBinaryCode = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabelTime = new System.Windows.Forms.ToolStripStatusLabel();
            timer1 = new System.Windows.Forms.Timer(components);
            lbIsoTiloSlicer = new System.Windows.Forms.Label();
            contextMenuStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageMain.SuspendLayout();
            tabPageAnimation.SuspendLayout();
            tabPageGraphic.SuspendLayout();
            tabPageMap.SuspendLayout();
            tabPageClient.SuspendLayout();
            tabPageScript.SuspendLayout();
            tabPageTextureConverter.SuspendLayout();
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
            // btnSpeichernTxt
            // 
            btnSpeichernTxt.Location = new System.Drawing.Point(338, 174);
            btnSpeichernTxt.Name = "btnSpeichernTxt";
            btnSpeichernTxt.Size = new System.Drawing.Size(92, 23);
            btnSpeichernTxt.TabIndex = 1;
            btnSpeichernTxt.Text = "Save";
            btnSpeichernTxt.UseVisualStyleBackColor = true;
            btnSpeichernTxt.Click += btnSpeichernTxt_Click;
            // 
            // btnUmwandeln
            // 
            btnUmwandeln.Location = new System.Drawing.Point(338, 26);
            btnUmwandeln.Name = "btnUmwandeln";
            btnUmwandeln.Size = new System.Drawing.Size(92, 23);
            btnUmwandeln.TabIndex = 2;
            btnUmwandeln.Text = "Convert";
            btnUmwandeln.UseVisualStyleBackColor = true;
            btnUmwandeln.Click += btnUmwandeln_Click;
            // 
            // btnCopyTBox2
            // 
            btnCopyTBox2.Location = new System.Drawing.Point(716, 290);
            btnCopyTBox2.Name = "btnCopyTBox2";
            btnCopyTBox2.Size = new System.Drawing.Size(43, 23);
            btnCopyTBox2.TabIndex = 3;
            btnCopyTBox2.Text = "Copy";
            btnCopyTBox2.UseVisualStyleBackColor = true;
            btnCopyTBox2.Click += btnCopyTBox2_Click;
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
            copyToolStripMenuItem.Click += btnCopyTBox2_Click;
            // 
            // clearToolStripMenuItem
            // 
            clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            clearToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            clearToolStripMenuItem.Text = "Clear";
            clearToolStripMenuItem.Click += btclear_Click;
            // 
            // importClipboardToolStripMenuItem
            // 
            importClipboardToolStripMenuItem.Name = "importClipboardToolStripMenuItem";
            importClipboardToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            importClipboardToolStripMenuItem.Text = "Import Clipboard";
            importClipboardToolStripMenuItem.Click += importClipboardToolStripMenuItem_Click;
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
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageMain);
            tabControl1.Controls.Add(tabPageAnimation);
            tabControl1.Controls.Add(tabPageGraphic);
            tabControl1.Controls.Add(tabPageMap);
            tabControl1.Controls.Add(tabPageClient);
            tabControl1.Controls.Add(tabPageScript);
            tabControl1.Controls.Add(tabPageTextureConverter);
            tabControl1.Location = new System.Drawing.Point(3, 27);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(968, 365);
            tabControl1.TabIndex = 15;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(lbIsoTiloSlicer);
            tabPageMain.Controls.Add(btIsoTiloSlicer);
            tabPageMain.Controls.Add(lbAltitudeTool);
            tabPageMain.Controls.Add(btAltitudeTool);
            tabPageMain.Location = new System.Drawing.Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            tabPageMain.Size = new System.Drawing.Size(960, 337);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Main";
            tabPageMain.UseVisualStyleBackColor = true;
            // 
            // btIsoTiloSlicer
            // 
            btIsoTiloSlicer.Image = Properties.Resources.grafic_cutter;
            btIsoTiloSlicer.Location = new System.Drawing.Point(3, 168);
            btIsoTiloSlicer.Name = "btIsoTiloSlicer";
            btIsoTiloSlicer.Size = new System.Drawing.Size(55, 55);
            btIsoTiloSlicer.TabIndex = 2;
            btIsoTiloSlicer.UseVisualStyleBackColor = true;
            btIsoTiloSlicer.Click += btIsoTiloSlicer_Click;
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
            // btAltitudeTool
            // 
            btAltitudeTool.Image = Properties.Resources.altitude_tool;
            btAltitudeTool.Location = new System.Drawing.Point(3, 243);
            btAltitudeTool.Name = "btAltitudeTool";
            btAltitudeTool.Size = new System.Drawing.Size(55, 55);
            btAltitudeTool.TabIndex = 0;
            btAltitudeTool.UseVisualStyleBackColor = true;
            btAltitudeTool.Click += btAltitudeTool_Click;
            // 
            // tabPageAnimation
            // 
            tabPageAnimation.Controls.Add(lbAnimationVD);
            tabPageAnimation.Controls.Add(lbAnimationEdit);
            tabPageAnimation.Controls.Add(btAnimationVDForm);
            tabPageAnimation.Controls.Add(btAnimationEditFormButton);
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
            // btAnimationVDForm
            // 
            btAnimationVDForm.Image = Properties.Resources.animation_edit_3_;
            btAnimationVDForm.Location = new System.Drawing.Point(3, 94);
            btAnimationVDForm.Name = "btAnimationVDForm";
            btAnimationVDForm.Size = new System.Drawing.Size(59, 56);
            btAnimationVDForm.TabIndex = 16;
            btAnimationVDForm.UseVisualStyleBackColor = true;
            btAnimationVDForm.Click += btAnimationVDForm_Click;
            // 
            // btAnimationEditFormButton
            // 
            btAnimationEditFormButton.Image = Properties.Resources.animation_edit;
            btAnimationEditFormButton.Location = new System.Drawing.Point(3, 12);
            btAnimationEditFormButton.Name = "btAnimationEditFormButton";
            btAnimationEditFormButton.Size = new System.Drawing.Size(59, 56);
            btAnimationEditFormButton.TabIndex = 15;
            btAnimationEditFormButton.UseVisualStyleBackColor = true;
            btAnimationEditFormButton.Click += btAnimationEditFormButton_Click;
            // 
            // tabPageGraphic
            // 
            tabPageGraphic.Controls.Add(label5);
            tabPageGraphic.Controls.Add(btGumpIDRechner);
            tabPageGraphic.Controls.Add(lbUoArtMerge);
            tabPageGraphic.Controls.Add(btUOArtMerge);
            tabPageGraphic.Controls.Add(lbGumpsEdit);
            tabPageGraphic.Controls.Add(lbTextureCutter);
            tabPageGraphic.Controls.Add(lbGraficCutter);
            tabPageGraphic.Controls.Add(btGumpsEdit);
            tabPageGraphic.Controls.Add(buttonGraficCutterForm);
            tabPageGraphic.Controls.Add(TextureCutter);
            tabPageGraphic.Location = new System.Drawing.Point(4, 24);
            tabPageGraphic.Name = "tabPageGraphic";
            tabPageGraphic.Size = new System.Drawing.Size(960, 337);
            tabPageGraphic.TabIndex = 2;
            tabPageGraphic.Text = "Graphic";
            tabPageGraphic.UseVisualStyleBackColor = true;
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
            // btGumpIDRechner
            // 
            btGumpIDRechner.Image = Properties.Resources.GumpID;
            btGumpIDRechner.Location = new System.Drawing.Point(209, 19);
            btGumpIDRechner.Name = "btGumpIDRechner";
            btGumpIDRechner.Size = new System.Drawing.Size(56, 64);
            btGumpIDRechner.TabIndex = 19;
            btGumpIDRechner.UseVisualStyleBackColor = true;
            btGumpIDRechner.Click += btGumpIDRechner_Click;
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
            // btUOArtMerge
            // 
            btUOArtMerge.Image = Properties.Resources.art_merge;
            btUOArtMerge.Location = new System.Drawing.Point(106, 19);
            btUOArtMerge.Name = "btUOArtMerge";
            btUOArtMerge.Size = new System.Drawing.Size(56, 64);
            btUOArtMerge.TabIndex = 17;
            btUOArtMerge.UseVisualStyleBackColor = true;
            btUOArtMerge.Click += btUOArtMerge_Click;
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
            // btGumpsEdit
            // 
            btGumpsEdit.Image = Properties.Resources.gump_edit_1_;
            btGumpsEdit.Location = new System.Drawing.Point(3, 193);
            btGumpsEdit.Name = "btGumpsEdit";
            btGumpsEdit.Size = new System.Drawing.Size(56, 64);
            btGumpsEdit.TabIndex = 13;
            btGumpsEdit.UseVisualStyleBackColor = true;
            btGumpsEdit.Click += btGumpsEdit_Click;
            // 
            // buttonGraficCutterForm
            // 
            buttonGraficCutterForm.Image = Properties.Resources.grafic_cutter_1_;
            buttonGraficCutterForm.Location = new System.Drawing.Point(3, 19);
            buttonGraficCutterForm.Name = "buttonGraficCutterForm";
            buttonGraficCutterForm.Size = new System.Drawing.Size(56, 64);
            buttonGraficCutterForm.TabIndex = 11;
            buttonGraficCutterForm.UseVisualStyleBackColor = true;
            buttonGraficCutterForm.Click += buttonGraficCutterForm_Click;
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
            tabPageMap.Controls.Add(lbCopyMapReplace);
            tabPageMap.Controls.Add(btMapReplace);
            tabPageMap.Controls.Add(lbMapMaker);
            tabPageMap.Controls.Add(btMapMaker);
            tabPageMap.Location = new System.Drawing.Point(4, 24);
            tabPageMap.Name = "tabPageMap";
            tabPageMap.Size = new System.Drawing.Size(960, 337);
            tabPageMap.TabIndex = 4;
            tabPageMap.Text = "Map";
            tabPageMap.UseVisualStyleBackColor = true;
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
            // btMapReplace
            // 
            btMapReplace.Image = Properties.Resources.copy_a_map_and_replace;
            btMapReplace.Location = new System.Drawing.Point(6, 91);
            btMapReplace.Name = "btMapReplace";
            btMapReplace.Size = new System.Drawing.Size(59, 57);
            btMapReplace.TabIndex = 15;
            btMapReplace.UseVisualStyleBackColor = true;
            btMapReplace.Click += btMapReplace_Click;
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
            // btMapMaker
            // 
            btMapMaker.Image = Properties.Resources.map_maker;
            btMapMaker.Location = new System.Drawing.Point(6, 10);
            btMapMaker.Name = "btMapMaker";
            btMapMaker.Size = new System.Drawing.Size(59, 57);
            btMapMaker.TabIndex = 13;
            btMapMaker.UseVisualStyleBackColor = true;
            btMapMaker.Click += btMapMaker_Click;
            // 
            // tabPageClient
            // 
            tabPageClient.Controls.Add(label4);
            tabPageClient.Controls.Add(btArtMul);
            tabPageClient.Controls.Add(lbDecriptClient);
            tabPageClient.Controls.Add(btDecriptClient);
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
            // btArtMul
            // 
            btArtMul.Image = Properties.Resources.create_art_abd_mul_file2;
            btArtMul.Location = new System.Drawing.Point(92, 13);
            btArtMul.Name = "btArtMul";
            btArtMul.Size = new System.Drawing.Size(56, 59);
            btArtMul.TabIndex = 14;
            btArtMul.UseVisualStyleBackColor = true;
            btArtMul.Click += btArtMul_Click;
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
            // btDecriptClient
            // 
            btDecriptClient.Image = Properties.Resources.decript;
            btDecriptClient.Location = new System.Drawing.Point(3, 13);
            btDecriptClient.Name = "btDecriptClient";
            btDecriptClient.Size = new System.Drawing.Size(56, 59);
            btDecriptClient.TabIndex = 12;
            btDecriptClient.UseVisualStyleBackColor = true;
            btDecriptClient.Click += btDecriptClient_Click;
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
            btScriptCreator.Click += btScriptCreator_Click;
            // 
            // tabPageTextureConverter
            // 
            tabPageTextureConverter.Controls.Add(btclear);
            tabPageTextureConverter.Controls.Add(btMorseCode);
            tabPageTextureConverter.Controls.Add(checkBoxASCII);
            tabPageTextureConverter.Controls.Add(btBinaryCode);
            tabPageTextureConverter.Controls.Add(textBox1);
            tabPageTextureConverter.Controls.Add(BtnMultiOpen);
            tabPageTextureConverter.Controls.Add(btnSpeichernTxt);
            tabPageTextureConverter.Controls.Add(btnUmwandeln);
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
            // btclear
            // 
            btclear.Location = new System.Drawing.Point(284, 290);
            btclear.Name = "btclear";
            btclear.Size = new System.Drawing.Size(40, 23);
            btclear.TabIndex = 12;
            btclear.Text = "clear";
            btclear.UseVisualStyleBackColor = true;
            btclear.Click += btclear_Click;
            // 
            // btMorseCode
            // 
            btMorseCode.Location = new System.Drawing.Point(338, 84);
            btMorseCode.Name = "btMorseCode";
            btMorseCode.Size = new System.Drawing.Size(92, 23);
            btMorseCode.TabIndex = 11;
            btMorseCode.Text = "Morse code";
            btMorseCode.UseVisualStyleBackColor = true;
            btMorseCode.Click += btMorseCode_Click;
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
            // btBinaryCode
            // 
            btBinaryCode.Location = new System.Drawing.Point(338, 55);
            btBinaryCode.Name = "btBinaryCode";
            btBinaryCode.Size = new System.Drawing.Size(92, 23);
            btBinaryCode.TabIndex = 9;
            btBinaryCode.Text = "Binary Code";
            btBinaryCode.UseVisualStyleBackColor = true;
            btBinaryCode.Click += btBinaryCode_Click;
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
            // lbIsoTiloSlicer
            // 
            lbIsoTiloSlicer.AutoSize = true;
            lbIsoTiloSlicer.Location = new System.Drawing.Point(3, 226);
            lbIsoTiloSlicer.Name = "lbIsoTiloSlicer";
            lbIsoTiloSlicer.Size = new System.Drawing.Size(69, 15);
            lbIsoTiloSlicer.TabIndex = 3;
            lbIsoTiloSlicer.Text = "IsoTiloSlicer";
            // 
            // ConverterMultiTextControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(statusStrip1);
            Controls.Add(tabControl1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ConverterMultiTextControl";
            Size = new System.Drawing.Size(974, 417);
            contextMenuStrip1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
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
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button BtnMultiOpen;
        private System.Windows.Forms.Button btnSpeichernTxt;
        private System.Windows.Forms.Button btnUmwandeln;
        private System.Windows.Forms.Button btnCopyTBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.TabPage tabPageTextureConverter;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TabPage tabPageGraphic;
        private System.Windows.Forms.Button buttonGraficCutterForm;
        private System.Windows.Forms.Button TextureCutter;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.Button btAnimationVDForm;
        private System.Windows.Forms.Button btAnimationEditFormButton;
        private System.Windows.Forms.TabPage tabPageMap;
        private System.Windows.Forms.Button btMapMaker;
        private System.Windows.Forms.TabPage tabPageClient;
        private System.Windows.Forms.Button btDecriptClient;
        private System.Windows.Forms.Button btGumpsEdit;
        private System.Windows.Forms.Button btAltitudeTool;
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
        private System.Windows.Forms.Button btBinaryCode;
        private System.Windows.Forms.CheckBox checkBoxASCII;
        private System.Windows.Forms.Button btMorseCode;
        private System.Windows.Forms.Button btclear;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importClipboardToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPageScript;
        private System.Windows.Forms.Button btMapReplace;
        private System.Windows.Forms.Label lbCopyMapReplace;
        private System.Windows.Forms.Button btArtMul;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btScriptCreator;
        private System.Windows.Forms.Label lbScriptCreator;
        private System.Windows.Forms.Button btUOArtMerge;
        private System.Windows.Forms.Label lbUoArtMerge;
        private System.Windows.Forms.Button btGumpIDRechner;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btIsoTiloSlicer;
        private System.Windows.Forms.Label lbIsoTiloSlicer;
    }
}

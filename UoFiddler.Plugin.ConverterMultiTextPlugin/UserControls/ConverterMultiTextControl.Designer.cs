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
            textBox2 = new System.Windows.Forms.TextBox();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            label3 = new System.Windows.Forms.Label();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPageMain = new System.Windows.Forms.TabPage();
            btAltitudeTool = new System.Windows.Forms.Button();
            tabPageAnimation = new System.Windows.Forms.TabPage();
            btAnimationVDForm = new System.Windows.Forms.Button();
            btAnimationEditFormButton = new System.Windows.Forms.Button();
            tabPageGraphic = new System.Windows.Forms.TabPage();
            btGumpsEdit = new System.Windows.Forms.Button();
            buttonGraficCutterForm = new System.Windows.Forms.Button();
            TextureCutter = new System.Windows.Forms.Button();
            tabPageMap = new System.Windows.Forms.TabPage();
            btMapMaker = new System.Windows.Forms.Button();
            tabPageClient = new System.Windows.Forms.TabPage();
            btDecriptClient = new System.Windows.Forms.Button();
            tabPageTextureConverter = new System.Windows.Forms.TabPage();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            lbDecriptClient = new System.Windows.Forms.Label();
            lbMapMaker = new System.Windows.Forms.Label();
            lbGraficCutter = new System.Windows.Forms.Label();
            lbTextureCutter = new System.Windows.Forms.Label();
            lbGumpsEdit = new System.Windows.Forms.Label();
            lbAnimationEdit = new System.Windows.Forms.Label();
            lbAnimationVD = new System.Windows.Forms.Label();
            lbAltitudeTool = new System.Windows.Forms.Label();
            tabControl1.SuspendLayout();
            tabPageMain.SuspendLayout();
            tabPageAnimation.SuspendLayout();
            tabPageGraphic.SuspendLayout();
            tabPageMap.SuspendLayout();
            tabPageClient.SuspendLayout();
            tabPageTextureConverter.SuspendLayout();
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
            textBox1.Location = new System.Drawing.Point(6, 6);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBox1.Size = new System.Drawing.Size(318, 267);
            textBox1.TabIndex = 6;
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
            tabControl1.Controls.Add(tabPageTextureConverter);
            tabControl1.Location = new System.Drawing.Point(3, 27);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(968, 358);
            tabControl1.TabIndex = 15;
            // 
            // tabPageMain
            // 
            tabPageMain.Controls.Add(lbAltitudeTool);
            tabPageMain.Controls.Add(btAltitudeTool);
            tabPageMain.Location = new System.Drawing.Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            tabPageMain.Size = new System.Drawing.Size(960, 330);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Main";
            tabPageMain.UseVisualStyleBackColor = true;
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
            tabPageAnimation.Size = new System.Drawing.Size(960, 330);
            tabPageAnimation.TabIndex = 3;
            tabPageAnimation.Text = "Animation";
            tabPageAnimation.UseVisualStyleBackColor = true;
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
            tabPageGraphic.Controls.Add(lbGumpsEdit);
            tabPageGraphic.Controls.Add(lbTextureCutter);
            tabPageGraphic.Controls.Add(lbGraficCutter);
            tabPageGraphic.Controls.Add(btGumpsEdit);
            tabPageGraphic.Controls.Add(buttonGraficCutterForm);
            tabPageGraphic.Controls.Add(TextureCutter);
            tabPageGraphic.Location = new System.Drawing.Point(4, 24);
            tabPageGraphic.Name = "tabPageGraphic";
            tabPageGraphic.Size = new System.Drawing.Size(960, 330);
            tabPageGraphic.TabIndex = 2;
            tabPageGraphic.Text = "Graphic";
            tabPageGraphic.UseVisualStyleBackColor = true;
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
            tabPageMap.Controls.Add(lbMapMaker);
            tabPageMap.Controls.Add(btMapMaker);
            tabPageMap.Location = new System.Drawing.Point(4, 24);
            tabPageMap.Name = "tabPageMap";
            tabPageMap.Size = new System.Drawing.Size(960, 330);
            tabPageMap.TabIndex = 4;
            tabPageMap.Text = "Map";
            tabPageMap.UseVisualStyleBackColor = true;
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
            tabPageClient.Controls.Add(lbDecriptClient);
            tabPageClient.Controls.Add(btDecriptClient);
            tabPageClient.Location = new System.Drawing.Point(4, 24);
            tabPageClient.Name = "tabPageClient";
            tabPageClient.Size = new System.Drawing.Size(960, 330);
            tabPageClient.TabIndex = 5;
            tabPageClient.Text = "Client";
            tabPageClient.UseVisualStyleBackColor = true;
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
            // tabPageTextureConverter
            // 
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
            tabPageTextureConverter.Size = new System.Drawing.Size(960, 330);
            tabPageTextureConverter.TabIndex = 1;
            tabPageTextureConverter.Text = "Text Converter";
            tabPageTextureConverter.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(974, 24);
            menuStrip1.TabIndex = 16;
            menuStrip1.Text = "menuStrip1";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
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
            // lbMapMaker
            // 
            lbMapMaker.AutoSize = true;
            lbMapMaker.Location = new System.Drawing.Point(6, 70);
            lbMapMaker.Name = "lbMapMaker";
            lbMapMaker.Size = new System.Drawing.Size(67, 15);
            lbMapMaker.TabIndex = 14;
            lbMapMaker.Text = "Map Maker";
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
            // lbTextureCutter
            // 
            lbTextureCutter.AutoSize = true;
            lbTextureCutter.Location = new System.Drawing.Point(3, 173);
            lbTextureCutter.Name = "lbTextureCutter";
            lbTextureCutter.Size = new System.Drawing.Size(81, 15);
            lbTextureCutter.TabIndex = 15;
            lbTextureCutter.Text = "Texture Cutter";
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
            // lbAnimationEdit
            // 
            lbAnimationEdit.AutoSize = true;
            lbAnimationEdit.Location = new System.Drawing.Point(3, 71);
            lbAnimationEdit.Name = "lbAnimationEdit";
            lbAnimationEdit.Size = new System.Drawing.Size(86, 15);
            lbAnimationEdit.TabIndex = 17;
            lbAnimationEdit.Text = "Animation Edit";
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
            // lbAltitudeTool
            // 
            lbAltitudeTool.AutoSize = true;
            lbAltitudeTool.Location = new System.Drawing.Point(3, 301);
            lbAltitudeTool.Name = "lbAltitudeTool";
            lbAltitudeTool.Size = new System.Drawing.Size(71, 15);
            lbAltitudeTool.TabIndex = 1;
            lbAltitudeTool.Text = "AltitudeTool";
            // 
            // ConverterMultiTextControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tabControl1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ConverterMultiTextControl";
            Size = new System.Drawing.Size(974, 417);
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
            tabPageTextureConverter.ResumeLayout(false);
            tabPageTextureConverter.PerformLayout();
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
    }
}

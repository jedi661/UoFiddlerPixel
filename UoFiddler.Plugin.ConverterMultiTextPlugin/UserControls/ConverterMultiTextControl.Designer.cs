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
            tabControl1.SuspendLayout();
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
            tabPageMain.Location = new System.Drawing.Point(4, 24);
            tabPageMain.Name = "tabPageMain";
            tabPageMain.Padding = new System.Windows.Forms.Padding(3);
            tabPageMain.Size = new System.Drawing.Size(960, 330);
            tabPageMain.TabIndex = 0;
            tabPageMain.Text = "Main";
            tabPageMain.UseVisualStyleBackColor = true;
            // 
            // tabPageAnimation
            // 
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
            btAnimationVDForm.Location = new System.Drawing.Point(3, 194);
            btAnimationVDForm.Name = "btAnimationVDForm";
            btAnimationVDForm.Size = new System.Drawing.Size(92, 23);
            btAnimationVDForm.TabIndex = 16;
            btAnimationVDForm.Text = "VD Edit";
            btAnimationVDForm.UseVisualStyleBackColor = true;
            btAnimationVDForm.Click += btAnimationVDForm_Click;
            // 
            // btAnimationEditFormButton
            // 
            btAnimationEditFormButton.Location = new System.Drawing.Point(3, 165);
            btAnimationEditFormButton.Name = "btAnimationEditFormButton";
            btAnimationEditFormButton.Size = new System.Drawing.Size(92, 23);
            btAnimationEditFormButton.TabIndex = 15;
            btAnimationEditFormButton.Text = "Amin Edit";
            btAnimationEditFormButton.UseVisualStyleBackColor = true;
            btAnimationEditFormButton.Click += btAnimationEditFormButton_Click;
            // 
            // tabPageGraphic
            // 
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
            btGumpsEdit.Location = new System.Drawing.Point(3, 70);
            btGumpsEdit.Name = "btGumpsEdit";
            btGumpsEdit.Size = new System.Drawing.Size(92, 23);
            btGumpsEdit.TabIndex = 13;
            btGumpsEdit.Text = "Gump";
            btGumpsEdit.UseVisualStyleBackColor = true;
            btGumpsEdit.Click += btGumpsEdit_Click;
            // 
            // buttonGraficCutterForm
            // 
            buttonGraficCutterForm.Location = new System.Drawing.Point(3, 14);
            buttonGraficCutterForm.Name = "buttonGraficCutterForm";
            buttonGraficCutterForm.Size = new System.Drawing.Size(92, 23);
            buttonGraficCutterForm.TabIndex = 11;
            buttonGraficCutterForm.Text = "Grafik Cutter";
            buttonGraficCutterForm.UseVisualStyleBackColor = true;
            buttonGraficCutterForm.Click += buttonGraficCutterForm_Click;
            // 
            // TextureCutter
            // 
            TextureCutter.Location = new System.Drawing.Point(3, 41);
            TextureCutter.Name = "TextureCutter";
            TextureCutter.Size = new System.Drawing.Size(92, 23);
            TextureCutter.TabIndex = 12;
            TextureCutter.Text = "Texture Cutter";
            TextureCutter.UseVisualStyleBackColor = true;
            TextureCutter.Click += TextureCutter_Click;
            // 
            // tabPageMap
            // 
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
            btMapMaker.Location = new System.Drawing.Point(3, 122);
            btMapMaker.Name = "btMapMaker";
            btMapMaker.Size = new System.Drawing.Size(92, 23);
            btMapMaker.TabIndex = 13;
            btMapMaker.Text = "MapMaker";
            btMapMaker.UseVisualStyleBackColor = true;
            btMapMaker.Click += btMapMaker_Click;
            // 
            // tabPageClient
            // 
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
            btDecriptClient.Location = new System.Drawing.Point(3, 132);
            btDecriptClient.Name = "btDecriptClient";
            btDecriptClient.Size = new System.Drawing.Size(92, 23);
            btDecriptClient.TabIndex = 12;
            btDecriptClient.Text = "Decript";
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
            tabPageAnimation.ResumeLayout(false);
            tabPageGraphic.ResumeLayout(false);
            tabPageMap.ResumeLayout(false);
            tabPageClient.ResumeLayout(false);
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
    }
}

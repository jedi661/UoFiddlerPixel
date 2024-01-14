// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class TextureWindowForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureWindowForm));
            pictureBoxTexture = new System.Windows.Forms.PictureBox();
            contextMenuStripTexturen = new System.Windows.Forms.ContextMenuStrip(components);
            clipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            importToPrewiewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btBackward = new System.Windows.Forms.Button();
            btForward = new System.Windows.Forms.Button();
            lbTextureSize = new System.Windows.Forms.Label();
            lbIDNr = new System.Windows.Forms.Label();
            btMakeTile = new System.Windows.Forms.Button();
            checkBoxLeft = new System.Windows.Forms.CheckBox();
            checkBoxRight = new System.Windows.Forms.CheckBox();
            checkBoxAntiAliasing = new System.Windows.Forms.CheckBox();
            panelTexture = new System.Windows.Forms.Panel();
            buttonOpenTempGrafic = new System.Windows.Forms.Button();
            toolStripTexture = new System.Windows.Forms.ToolStrip();
            toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            IgPreviewClicked = new System.Windows.Forms.Button();
            previousButton = new System.Windows.Forms.Button();
            NextButton = new System.Windows.Forms.Button();
            panelPreview = new System.Windows.Forms.Panel();
            tBoxInfoColor = new System.Windows.Forms.TextBox();
            btColorHex = new System.Windows.Forms.Button();
            rtBoxInfo = new System.Windows.Forms.RichTextBox();
            btReplaceColor = new System.Windows.Forms.Button();
            tbColorSet = new System.Windows.Forms.TextBox();
            btColorDialog = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).BeginInit();
            contextMenuStripTexturen.SuspendLayout();
            panelTexture.SuspendLayout();
            toolStripTexture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            panelPreview.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBoxTexture
            // 
            pictureBoxTexture.ContextMenuStrip = contextMenuStripTexturen;
            pictureBoxTexture.Location = new System.Drawing.Point(29, 36);
            pictureBoxTexture.Name = "pictureBoxTexture";
            pictureBoxTexture.Size = new System.Drawing.Size(206, 160);
            pictureBoxTexture.TabIndex = 0;
            pictureBoxTexture.TabStop = false;
            // 
            // contextMenuStripTexturen
            // 
            contextMenuStripTexturen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { clipboardToolStripMenuItem, importToolStripMenuItem, toolStripSeparator1, importToPrewiewToolStripMenuItem, toolStripSeparator2, saveToolStripMenuItem });
            contextMenuStripTexturen.Name = "contextMenuStripTexturen";
            contextMenuStripTexturen.Size = new System.Drawing.Size(172, 104);
            // 
            // clipboardToolStripMenuItem
            // 
            clipboardToolStripMenuItem.Image = Properties.Resources.Clipbord;
            clipboardToolStripMenuItem.Name = "clipboardToolStripMenuItem";
            clipboardToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            clipboardToolStripMenuItem.Text = "Clipboard";
            clipboardToolStripMenuItem.ToolTipText = "Copy the image from the PictureBox to the clipboard.";
            clipboardToolStripMenuItem.Click += clipboardToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            importToolStripMenuItem.Text = "Import";
            importToolStripMenuItem.ToolTipText = "Import the image from the clipboard into the PictureBox.";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // importToPrewiewToolStripMenuItem
            // 
            importToPrewiewToolStripMenuItem.Image = Properties.Resources.iishenar_map;
            importToPrewiewToolStripMenuItem.Name = "importToPrewiewToolStripMenuItem";
            importToPrewiewToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            importToPrewiewToolStripMenuItem.Text = "Import to Prewiew";
            importToPrewiewToolStripMenuItem.ToolTipText = "Imports the graphic from the clipboard into the grid of the Picture Preview.";
            importToPrewiewToolStripMenuItem.Click += importToPrewiewToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(168, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.ToolTipText = "Save the image to the target directory.";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // btBackward
            // 
            btBackward.Location = new System.Drawing.Point(29, 224);
            btBackward.Name = "btBackward";
            btBackward.Size = new System.Drawing.Size(67, 23);
            btBackward.TabIndex = 1;
            btBackward.Text = "Backward";
            btBackward.UseVisualStyleBackColor = true;
            btBackward.Click += btBackward_Click;
            // 
            // btForward
            // 
            btForward.Location = new System.Drawing.Point(102, 224);
            btForward.Name = "btForward";
            btForward.Size = new System.Drawing.Size(58, 23);
            btForward.TabIndex = 2;
            btForward.Text = "Forward";
            btForward.UseVisualStyleBackColor = true;
            btForward.Click += btForward_Click;
            // 
            // lbTextureSize
            // 
            lbTextureSize.AutoSize = true;
            lbTextureSize.Location = new System.Drawing.Point(29, 18);
            lbTextureSize.Name = "lbTextureSize";
            lbTextureSize.Size = new System.Drawing.Size(30, 15);
            lbTextureSize.TabIndex = 3;
            lbTextureSize.Text = "Size:";
            // 
            // lbIDNr
            // 
            lbIDNr.AutoSize = true;
            lbIDNr.Location = new System.Drawing.Point(29, 199);
            lbIDNr.Name = "lbIDNr";
            lbIDNr.Size = new System.Drawing.Size(21, 15);
            lbIDNr.TabIndex = 4;
            lbIDNr.Text = "ID:";
            // 
            // btMakeTile
            // 
            btMakeTile.Location = new System.Drawing.Point(167, 224);
            btMakeTile.Name = "btMakeTile";
            btMakeTile.Size = new System.Drawing.Size(68, 23);
            btMakeTile.TabIndex = 5;
            btMakeTile.Text = "Make Tile";
            btMakeTile.UseVisualStyleBackColor = true;
            btMakeTile.Click += btMakeTile_Click;
            // 
            // checkBoxLeft
            // 
            checkBoxLeft.AutoSize = true;
            checkBoxLeft.Location = new System.Drawing.Point(241, 36);
            checkBoxLeft.Name = "checkBoxLeft";
            checkBoxLeft.Size = new System.Drawing.Size(46, 19);
            checkBoxLeft.TabIndex = 6;
            checkBoxLeft.Text = "Left";
            checkBoxLeft.UseVisualStyleBackColor = true;
            checkBoxLeft.CheckedChanged += checkBoxLeft_CheckedChanged;
            // 
            // checkBoxRight
            // 
            checkBoxRight.AutoSize = true;
            checkBoxRight.Location = new System.Drawing.Point(241, 61);
            checkBoxRight.Name = "checkBoxRight";
            checkBoxRight.Size = new System.Drawing.Size(54, 19);
            checkBoxRight.TabIndex = 7;
            checkBoxRight.Text = "Right";
            checkBoxRight.UseVisualStyleBackColor = true;
            checkBoxRight.CheckedChanged += checkBoxRight_CheckedChanged;
            // 
            // checkBoxAntiAliasing
            // 
            checkBoxAntiAliasing.AutoSize = true;
            checkBoxAntiAliasing.Location = new System.Drawing.Point(241, 86);
            checkBoxAntiAliasing.Name = "checkBoxAntiAliasing";
            checkBoxAntiAliasing.Size = new System.Drawing.Size(95, 19);
            checkBoxAntiAliasing.TabIndex = 8;
            checkBoxAntiAliasing.Text = "Anti-Aliasing";
            checkBoxAntiAliasing.UseVisualStyleBackColor = true;
            // 
            // panelTexture
            // 
            panelTexture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelTexture.Controls.Add(buttonOpenTempGrafic);
            panelTexture.Controls.Add(pictureBoxTexture);
            panelTexture.Controls.Add(btBackward);
            panelTexture.Controls.Add(checkBoxAntiAliasing);
            panelTexture.Controls.Add(btForward);
            panelTexture.Controls.Add(checkBoxRight);
            panelTexture.Controls.Add(lbTextureSize);
            panelTexture.Controls.Add(checkBoxLeft);
            panelTexture.Controls.Add(lbIDNr);
            panelTexture.Controls.Add(btMakeTile);
            panelTexture.Location = new System.Drawing.Point(12, 37);
            panelTexture.Name = "panelTexture";
            panelTexture.Size = new System.Drawing.Size(340, 282);
            panelTexture.TabIndex = 9;
            // 
            // buttonOpenTempGrafic
            // 
            buttonOpenTempGrafic.Location = new System.Drawing.Point(241, 224);
            buttonOpenTempGrafic.Name = "buttonOpenTempGrafic";
            buttonOpenTempGrafic.Size = new System.Drawing.Size(46, 23);
            buttonOpenTempGrafic.TabIndex = 9;
            buttonOpenTempGrafic.Text = "Dir";
            buttonOpenTempGrafic.UseVisualStyleBackColor = true;
            buttonOpenTempGrafic.Click += buttonOpenTempGrafic_Click;
            // 
            // toolStripTexture
            // 
            toolStripTexture.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButtonSave });
            toolStripTexture.Location = new System.Drawing.Point(0, 0);
            toolStripTexture.Name = "toolStripTexture";
            toolStripTexture.Size = new System.Drawing.Size(868, 25);
            toolStripTexture.TabIndex = 10;
            toolStripTexture.Text = "toolStrip1";
            // 
            // toolStripButtonSave
            // 
            toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            toolStripButtonSave.Image = Properties.Resources.save;
            toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButtonSave.Name = "toolStripButtonSave";
            toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            toolStripButtonSave.Text = "Save";
            toolStripButtonSave.ToolTipText = "Save the image to the temporary directory as a BMP file with a single click";
            toolStripButtonSave.Click += toolStripButtonSave_Click;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.Location = new System.Drawing.Point(5, 12);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(473, 376);
            pictureBoxPreview.TabIndex = 11;
            pictureBoxPreview.TabStop = false;
            // 
            // IgPreviewClicked
            // 
            IgPreviewClicked.Location = new System.Drawing.Point(5, 394);
            IgPreviewClicked.Name = "IgPreviewClicked";
            IgPreviewClicked.Size = new System.Drawing.Size(56, 23);
            IgPreviewClicked.TabIndex = 12;
            IgPreviewClicked.Text = "Preview";
            IgPreviewClicked.UseVisualStyleBackColor = true;
            IgPreviewClicked.Click += IgPreviewClicked_Click;
            // 
            // previousButton
            // 
            previousButton.Location = new System.Drawing.Point(160, 394);
            previousButton.Name = "previousButton";
            previousButton.Size = new System.Drawing.Size(61, 23);
            previousButton.TabIndex = 13;
            previousButton.Text = "previous";
            previousButton.UseVisualStyleBackColor = true;
            previousButton.Click += previousButton_Click;
            // 
            // NextButton
            // 
            NextButton.Location = new System.Drawing.Point(227, 394);
            NextButton.Name = "NextButton";
            NextButton.Size = new System.Drawing.Size(56, 23);
            NextButton.TabIndex = 14;
            NextButton.Text = "Next";
            NextButton.UseVisualStyleBackColor = true;
            NextButton.Click += NextButton_Click;
            // 
            // panelPreview
            // 
            panelPreview.Controls.Add(pictureBoxPreview);
            panelPreview.Controls.Add(NextButton);
            panelPreview.Controls.Add(IgPreviewClicked);
            panelPreview.Controls.Add(previousButton);
            panelPreview.Location = new System.Drawing.Point(368, 37);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new System.Drawing.Size(488, 428);
            panelPreview.TabIndex = 15;
            // 
            // tBoxInfoColor
            // 
            tBoxInfoColor.Location = new System.Drawing.Point(76, 330);
            tBoxInfoColor.Multiline = true;
            tBoxInfoColor.Name = "tBoxInfoColor";
            tBoxInfoColor.Size = new System.Drawing.Size(97, 23);
            tBoxInfoColor.TabIndex = 16;
            // 
            // btColorHex
            // 
            btColorHex.Location = new System.Drawing.Point(304, 431);
            btColorHex.Name = "btColorHex";
            btColorHex.Size = new System.Drawing.Size(48, 23);
            btColorHex.TabIndex = 17;
            btColorHex.Text = "Color";
            btColorHex.UseVisualStyleBackColor = true;
            btColorHex.Click += btColorHex_Click;
            // 
            // rtBoxInfo
            // 
            rtBoxInfo.Location = new System.Drawing.Point(183, 325);
            rtBoxInfo.Name = "rtBoxInfo";
            rtBoxInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            rtBoxInfo.Size = new System.Drawing.Size(169, 100);
            rtBoxInfo.TabIndex = 18;
            rtBoxInfo.Text = "";
            // 
            // btReplaceColor
            // 
            btReplaceColor.Location = new System.Drawing.Point(76, 402);
            btReplaceColor.Name = "btReplaceColor";
            btReplaceColor.Size = new System.Drawing.Size(97, 23);
            btReplaceColor.TabIndex = 19;
            btReplaceColor.Text = "Replace Color";
            btReplaceColor.UseVisualStyleBackColor = true;
            btReplaceColor.Click += btReplaceColor_Click;
            // 
            // tbColorSet
            // 
            tbColorSet.Location = new System.Drawing.Point(76, 373);
            tbColorSet.Multiline = true;
            tbColorSet.Name = "tbColorSet";
            tbColorSet.Size = new System.Drawing.Size(97, 23);
            tbColorSet.TabIndex = 20;
            // 
            // btColorDialog
            // 
            btColorDialog.Location = new System.Drawing.Point(24, 373);
            btColorDialog.Name = "btColorDialog";
            btColorDialog.Size = new System.Drawing.Size(46, 23);
            btColorDialog.TabIndex = 21;
            btColorDialog.Text = "Colors";
            btColorDialog.UseVisualStyleBackColor = true;
            btColorDialog.Click += btColorDialog_Click;
            // 
            // TextureWindowForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(868, 473);
            Controls.Add(btColorDialog);
            Controls.Add(tbColorSet);
            Controls.Add(btReplaceColor);
            Controls.Add(rtBoxInfo);
            Controls.Add(btColorHex);
            Controls.Add(tBoxInfoColor);
            Controls.Add(panelPreview);
            Controls.Add(toolStripTexture);
            Controls.Add(panelTexture);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureWindowForm";
            Text = "Texture ./. Tile  Converter";
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).EndInit();
            contextMenuStripTexturen.ResumeLayout(false);
            panelTexture.ResumeLayout(false);
            panelTexture.PerformLayout();
            toolStripTexture.ResumeLayout(false);
            toolStripTexture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            panelPreview.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxTexture;
        private System.Windows.Forms.Button btBackward;
        private System.Windows.Forms.Button btForward;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripTexturen;
        private System.Windows.Forms.ToolStripMenuItem clipboardToolStripMenuItem;
        private System.Windows.Forms.Label lbTextureSize;
        private System.Windows.Forms.Label lbIDNr;
        private System.Windows.Forms.Button btMakeTile;
        private System.Windows.Forms.CheckBox checkBoxLeft;
        private System.Windows.Forms.CheckBox checkBoxRight;
        private System.Windows.Forms.CheckBox checkBoxAntiAliasing;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.Panel panelTexture;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripTexture;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.Button buttonOpenTempGrafic;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Button IgPreviewClicked;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.ToolStripMenuItem importToPrewiewToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.TextBox tBoxInfoColor;
        private System.Windows.Forms.Button btColorHex;
        private System.Windows.Forms.RichTextBox rtBoxInfo;
        private System.Windows.Forms.Button btReplaceColor;
        private System.Windows.Forms.TextBox tbColorSet;
        private System.Windows.Forms.Button btColorDialog;
    }
}
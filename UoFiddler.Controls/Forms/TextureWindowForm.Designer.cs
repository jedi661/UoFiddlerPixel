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
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).BeginInit();
            contextMenuStripTexturen.SuspendLayout();
            panelTexture.SuspendLayout();
            toolStripTexture.SuspendLayout();
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
            contextMenuStripTexturen.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { clipboardToolStripMenuItem, importToolStripMenuItem, saveToolStripMenuItem });
            contextMenuStripTexturen.Name = "contextMenuStripTexturen";
            contextMenuStripTexturen.Size = new System.Drawing.Size(127, 70);
            // 
            // clipboardToolStripMenuItem
            // 
            clipboardToolStripMenuItem.Image = Properties.Resources.Clipbord;
            clipboardToolStripMenuItem.Name = "clipboardToolStripMenuItem";
            clipboardToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            clipboardToolStripMenuItem.Text = "Clipboard";
            clipboardToolStripMenuItem.ToolTipText = "Copy the image from the PictureBox to the clipboard.";
            clipboardToolStripMenuItem.Click += clipboardToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Image = Properties.Resources.import;
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            importToolStripMenuItem.Text = "Import";
            importToolStripMenuItem.ToolTipText = "Import the image from the clipboard into the PictureBox.";
            importToolStripMenuItem.Click += importToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Image = Properties.Resources.save;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
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
            toolStripTexture.Size = new System.Drawing.Size(498, 25);
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
            // TextureWindowForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(498, 425);
            Controls.Add(toolStripTexture);
            Controls.Add(panelTexture);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "TextureWindowForm";
            Text = "Texture Window ";
            ((System.ComponentModel.ISupportInitialize)pictureBoxTexture).EndInit();
            contextMenuStripTexturen.ResumeLayout(false);
            panelTexture.ResumeLayout(false);
            panelTexture.PerformLayout();
            toolStripTexture.ResumeLayout(false);
            toolStripTexture.PerformLayout();
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
    }
}
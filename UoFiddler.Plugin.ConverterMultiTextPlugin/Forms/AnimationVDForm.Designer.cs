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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class AnimationVDForm
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
            pictureBoxAminImage = new System.Windows.Forms.PictureBox();
            contextMenuStripPictureBox = new System.Windows.Forms.ContextMenuStrip(components);
            importImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            playToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panel1 = new System.Windows.Forms.Panel();
            labelSpeed = new System.Windows.Forms.Label();
            btStopAminID = new System.Windows.Forms.Button();
            checkBoxLoop = new System.Windows.Forms.CheckBox();
            trackBarSpeedAmin = new System.Windows.Forms.TrackBar();
            btPlayAminID = new System.Windows.Forms.Button();
            btLoadAminID = new System.Windows.Forms.Button();
            checkedListBoxAminID = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxAminImage).BeginInit();
            contextMenuStripPictureBox.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSpeedAmin).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxAminImage
            // 
            pictureBoxAminImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxAminImage.ContextMenuStrip = contextMenuStripPictureBox;
            pictureBoxAminImage.Location = new System.Drawing.Point(20, 15);
            pictureBoxAminImage.Name = "pictureBoxAminImage";
            pictureBoxAminImage.Size = new System.Drawing.Size(148, 316);
            pictureBoxAminImage.TabIndex = 0;
            pictureBoxAminImage.TabStop = false;
            // 
            // contextMenuStripPictureBox
            // 
            contextMenuStripPictureBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { importImageToolStripMenuItem, loadImageToolStripMenuItem, toolStripSeparator1, playToolStripMenuItem, stopToolStripMenuItem });
            contextMenuStripPictureBox.Name = "contextMenuStripPictureBox";
            contextMenuStripPictureBox.Size = new System.Drawing.Size(202, 98);
            // 
            // importImageToolStripMenuItem
            // 
            importImageToolStripMenuItem.Image = Properties.Resources.import;
            importImageToolStripMenuItem.Name = "importImageToolStripMenuItem";
            importImageToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            importImageToolStripMenuItem.Text = "Import Image Clipboard";
            importImageToolStripMenuItem.Click += importImageToolStripMenuItem_Click;
            // 
            // loadImageToolStripMenuItem
            // 
            loadImageToolStripMenuItem.Image = Properties.Resources.Load;
            loadImageToolStripMenuItem.Name = "loadImageToolStripMenuItem";
            loadImageToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            loadImageToolStripMenuItem.Text = "Load Image";
            loadImageToolStripMenuItem.Click += btLoadAminID_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(198, 6);
            // 
            // playToolStripMenuItem
            // 
            playToolStripMenuItem.Image = Properties.Resources.right;
            playToolStripMenuItem.Name = "playToolStripMenuItem";
            playToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            playToolStripMenuItem.Text = "Play";
            playToolStripMenuItem.Click += btPlayAminID_Click;
            // 
            // stopToolStripMenuItem
            // 
            stopToolStripMenuItem.Image = Properties.Resources.uomc02;
            stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            stopToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            stopToolStripMenuItem.Text = "Stop";
            stopToolStripMenuItem.Click += btStopAminID_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(labelSpeed);
            panel1.Controls.Add(btStopAminID);
            panel1.Controls.Add(checkBoxLoop);
            panel1.Controls.Add(trackBarSpeedAmin);
            panel1.Controls.Add(btPlayAminID);
            panel1.Controls.Add(btLoadAminID);
            panel1.Controls.Add(checkedListBoxAminID);
            panel1.Controls.Add(pictureBoxAminImage);
            panel1.Location = new System.Drawing.Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(342, 346);
            panel1.TabIndex = 1;
            // 
            // labelSpeed
            // 
            labelSpeed.AutoSize = true;
            labelSpeed.Location = new System.Drawing.Point(188, 316);
            labelSpeed.Name = "labelSpeed";
            labelSpeed.Size = new System.Drawing.Size(39, 15);
            labelSpeed.TabIndex = 7;
            labelSpeed.Text = "Speed";
            // 
            // btStopAminID
            // 
            btStopAminID.Location = new System.Drawing.Point(246, 63);
            btStopAminID.Name = "btStopAminID";
            btStopAminID.Size = new System.Drawing.Size(75, 23);
            btStopAminID.TabIndex = 6;
            btStopAminID.Text = "Stop";
            btStopAminID.UseVisualStyleBackColor = true;
            btStopAminID.Click += btStopAminID_Click;
            // 
            // checkBoxLoop
            // 
            checkBoxLoop.AutoSize = true;
            checkBoxLoop.Checked = true;
            checkBoxLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxLoop.Location = new System.Drawing.Point(188, 261);
            checkBoxLoop.Name = "checkBoxLoop";
            checkBoxLoop.Size = new System.Drawing.Size(53, 19);
            checkBoxLoop.TabIndex = 5;
            checkBoxLoop.Text = "Loop";
            checkBoxLoop.UseVisualStyleBackColor = true;
            // 
            // trackBarSpeedAmin
            // 
            trackBarSpeedAmin.Location = new System.Drawing.Point(188, 286);
            trackBarSpeedAmin.Maximum = 5;
            trackBarSpeedAmin.Minimum = 1;
            trackBarSpeedAmin.Name = "trackBarSpeedAmin";
            trackBarSpeedAmin.Size = new System.Drawing.Size(104, 45);
            trackBarSpeedAmin.TabIndex = 4;
            trackBarSpeedAmin.Value = 3;
            // 
            // btPlayAminID
            // 
            btPlayAminID.Location = new System.Drawing.Point(246, 39);
            btPlayAminID.Name = "btPlayAminID";
            btPlayAminID.Size = new System.Drawing.Size(75, 23);
            btPlayAminID.TabIndex = 3;
            btPlayAminID.Text = "Play";
            btPlayAminID.UseVisualStyleBackColor = true;
            btPlayAminID.Click += btPlayAminID_Click;
            // 
            // btLoadAminID
            // 
            btLoadAminID.Location = new System.Drawing.Point(246, 15);
            btLoadAminID.Name = "btLoadAminID";
            btLoadAminID.Size = new System.Drawing.Size(75, 23);
            btLoadAminID.TabIndex = 2;
            btLoadAminID.Text = "Load";
            btLoadAminID.UseVisualStyleBackColor = true;
            btLoadAminID.Click += btLoadAminID_Click;
            // 
            // checkedListBoxAminID
            // 
            checkedListBoxAminID.FormattingEnabled = true;
            checkedListBoxAminID.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" });
            checkedListBoxAminID.Location = new System.Drawing.Point(188, 15);
            checkedListBoxAminID.Name = "checkedListBoxAminID";
            checkedListBoxAminID.Size = new System.Drawing.Size(39, 184);
            checkedListBoxAminID.TabIndex = 1;
            checkedListBoxAminID.ItemCheck += CheckedListBoxAminID_ItemCheck;
            // 
            // AnimationVDForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(376, 372);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Name = "AnimationVDForm";
            Text = "Animation Playing";
            FormClosing += AnimationVDForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)pictureBoxAminImage).EndInit();
            contextMenuStripPictureBox.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarSpeedAmin).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxAminImage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btLoadAminID;
        private System.Windows.Forms.CheckedListBox checkedListBoxAminID;
        private System.Windows.Forms.Button btPlayAminID;
        private System.Windows.Forms.CheckBox checkBoxLoop;
        private System.Windows.Forms.TrackBar trackBarSpeedAmin;
        private System.Windows.Forms.Button btStopAminID;
        private System.Windows.Forms.Label labelSpeed;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripPictureBox;
        private System.Windows.Forms.ToolStripMenuItem importImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem playToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadImageToolStripMenuItem;
    }
}
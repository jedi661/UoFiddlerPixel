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

namespace UoFiddler.Controls.Forms
{
    partial class MapReplaceTilesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapReplaceTilesForm));
            label5 = new System.Windows.Forms.Label();
            browseButton = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            richTextBox1 = new System.Windows.Forms.RichTextBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            pictureBox3 = new System.Windows.Forms.PictureBox();
            pictureBox4 = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            btToupdateRichbox1 = new System.Windows.Forms.Button();
            btStaticForward = new System.Windows.Forms.Button();
            btStaticBackward = new System.Windows.Forms.Button();
            btLandForward = new System.Windows.Forms.Button();
            btLandBackward = new System.Windows.Forms.Button();
            label7 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            btOpenDir = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(13, 15);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(79, 15);
            label5.TabIndex = 17;
            label5.Text = "Replace From";
            // 
            // browseButton
            // 
            browseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            browseButton.Location = new System.Drawing.Point(394, 9);
            browseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            browseButton.Name = "browseButton";
            browseButton.Size = new System.Drawing.Size(55, 27);
            browseButton.TabIndex = 16;
            browseButton.Text = "...";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += OnBrowse;
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(100, 12);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(278, 23);
            textBox1.TabIndex = 15;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(291, 12);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(66, 27);
            button2.TabIndex = 18;
            button2.Text = "Replace";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnReplace;
            // 
            // richTextBox1
            // 
            richTextBox1.DetectUrls = false;
            richTextBox1.Location = new System.Drawing.Point(13, 42);
            richTextBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new System.Drawing.Size(365, 278);
            richTextBox1.TabIndex = 19;
            richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // pictureBox1
            // 
            pictureBox1.Location = new System.Drawing.Point(394, 65);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(65, 50);
            pictureBox1.TabIndex = 20;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Location = new System.Drawing.Point(511, 65);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(65, 50);
            pictureBox2.TabIndex = 21;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Location = new System.Drawing.Point(391, 228);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new System.Drawing.Size(100, 92);
            pictureBox3.TabIndex = 22;
            pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Location = new System.Drawing.Point(511, 228);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new System.Drawing.Size(100, 92);
            pictureBox4.TabIndex = 23;
            pictureBox4.TabStop = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(394, 45);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(55, 15);
            label1.TabIndex = 24;
            label1.Text = "Tiles Old:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(511, 45);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(60, 15);
            label2.TabIndex = 25;
            label2.Text = "Tiles New:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(394, 193);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(61, 15);
            label3.TabIndex = 26;
            label3.Text = "Static Old:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(511, 193);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(66, 15);
            label4.TabIndex = 27;
            label4.Text = "Static New:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(473, 45);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(18, 15);
            label6.TabIndex = 28;
            label6.Text = "to";
            // 
            // btToupdateRichbox1
            // 
            btToupdateRichbox1.Location = new System.Drawing.Point(3, 12);
            btToupdateRichbox1.Name = "btToupdateRichbox1";
            btToupdateRichbox1.Size = new System.Drawing.Size(74, 27);
            btToupdateRichbox1.TabIndex = 29;
            btToupdateRichbox1.Text = "to Update";
            btToupdateRichbox1.UseVisualStyleBackColor = true;
            btToupdateRichbox1.Click += btToupdateRichbox1_Click;
            // 
            // btStaticForward
            // 
            btStaticForward.Image = Properties.Resources.left_arrow;
            btStaticForward.Location = new System.Drawing.Point(404, 326);
            btStaticForward.Name = "btStaticForward";
            btStaticForward.Size = new System.Drawing.Size(55, 55);
            btStaticForward.TabIndex = 30;
            btStaticForward.UseVisualStyleBackColor = true;
            btStaticForward.Click += btStaticForward_Click;
            // 
            // btStaticBackward
            // 
            btStaticBackward.Image = Properties.Resources.right_arrow;
            btStaticBackward.Location = new System.Drawing.Point(511, 326);
            btStaticBackward.Name = "btStaticBackward";
            btStaticBackward.Size = new System.Drawing.Size(54, 55);
            btStaticBackward.TabIndex = 31;
            btStaticBackward.UseVisualStyleBackColor = true;
            btStaticBackward.Click += btStaticBackward_Click;
            // 
            // btLandForward
            // 
            btLandForward.Image = Properties.Resources.left_arrow;
            btLandForward.Location = new System.Drawing.Point(404, 121);
            btLandForward.Name = "btLandForward";
            btLandForward.Size = new System.Drawing.Size(55, 51);
            btLandForward.TabIndex = 32;
            btLandForward.UseVisualStyleBackColor = true;
            btLandForward.Click += btLandForward_Click;
            // 
            // btLandBackward
            // 
            btLandBackward.Image = Properties.Resources.right_arrow;
            btLandBackward.Location = new System.Drawing.Point(511, 121);
            btLandBackward.Name = "btLandBackward";
            btLandBackward.Size = new System.Drawing.Size(54, 51);
            btLandBackward.TabIndex = 33;
            btLandBackward.UseVisualStyleBackColor = true;
            btLandBackward.Click += btLandBackward_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(473, 193);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(18, 15);
            label7.TabIndex = 34;
            label7.Text = "to";
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            panel1.Controls.Add(btOpenDir);
            panel1.Controls.Add(btToupdateRichbox1);
            panel1.Controls.Add(button2);
            panel1.Location = new System.Drawing.Point(13, 326);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(365, 55);
            panel1.TabIndex = 35;
            // 
            // btOpenDir
            // 
            btOpenDir.Location = new System.Drawing.Point(85, 12);
            btOpenDir.Name = "btOpenDir";
            btOpenDir.Size = new System.Drawing.Size(64, 27);
            btOpenDir.TabIndex = 30;
            btOpenDir.Text = "Open Dir";
            btOpenDir.UseVisualStyleBackColor = true;
            btOpenDir.Click += btOpenDir_Click;
            // 
            // MapReplaceTilesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(615, 387);
            Controls.Add(panel1);
            Controls.Add(label7);
            Controls.Add(btLandBackward);
            Controls.Add(btLandForward);
            Controls.Add(btStaticBackward);
            Controls.Add(btStaticForward);
            Controls.Add(label6);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox4);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(richTextBox1);
            Controls.Add(label5);
            Controls.Add(browseButton);
            Controls.Add(textBox1);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "MapReplaceTilesForm";
            Text = "Map - Replace Tiles";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btToupdateRichbox1;
        private System.Windows.Forms.Button btStaticForward;
        private System.Windows.Forms.Button btStaticBackward;
        private System.Windows.Forms.Button btLandForward;
        private System.Windows.Forms.Button btLandBackward;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btOpenDir;
    }
}
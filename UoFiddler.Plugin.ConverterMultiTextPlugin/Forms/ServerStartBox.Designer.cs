// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  * 
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    partial class ServerStartBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerStartBox));
            btSetDirectoryServuo = new System.Windows.Forms.Button();
            panelServUo = new System.Windows.Forms.Panel();
            btOtherServer = new System.Windows.Forms.Button();
            lbServer = new System.Windows.Forms.Label();
            btSetExe = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            btSetServUOsln = new System.Windows.Forms.Button();
            btServUOsln = new System.Windows.Forms.Button();
            tbServuoDir = new System.Windows.Forms.TextBox();
            btOpenServUoDir = new System.Windows.Forms.Button();
            btStartServuo = new System.Windows.Forms.Button();
            lbServuo = new System.Windows.Forms.Label();
            panelServUo.SuspendLayout();
            SuspendLayout();
            // 
            // btSetDirectoryServuo
            // 
            btSetDirectoryServuo.Location = new System.Drawing.Point(3, 3);
            btSetDirectoryServuo.Name = "btSetDirectoryServuo";
            btSetDirectoryServuo.Size = new System.Drawing.Size(82, 23);
            btSetDirectoryServuo.TabIndex = 0;
            btSetDirectoryServuo.Text = "Set Directory";
            btSetDirectoryServuo.UseVisualStyleBackColor = true;
            btSetDirectoryServuo.Click += btSetDirectoryServuo_Click;
            // 
            // panelServUo
            // 
            panelServUo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelServUo.Controls.Add(btOtherServer);
            panelServUo.Controls.Add(lbServer);
            panelServUo.Controls.Add(btSetExe);
            panelServUo.Controls.Add(label1);
            panelServUo.Controls.Add(btSetServUOsln);
            panelServUo.Controls.Add(btServUOsln);
            panelServUo.Controls.Add(tbServuoDir);
            panelServUo.Controls.Add(btOpenServUoDir);
            panelServUo.Controls.Add(btStartServuo);
            panelServUo.Controls.Add(lbServuo);
            panelServUo.Controls.Add(btSetDirectoryServuo);
            panelServUo.Location = new System.Drawing.Point(12, 12);
            panelServUo.Name = "panelServUo";
            panelServUo.Size = new System.Drawing.Size(286, 130);
            panelServUo.TabIndex = 1;
            panelServUo.Tag = "";
            // 
            // btOtherServer
            // 
            btOtherServer.Location = new System.Drawing.Point(178, 61);
            btOtherServer.Name = "btOtherServer";
            btOtherServer.Size = new System.Drawing.Size(103, 23);
            btOtherServer.TabIndex = 10;
            btOtherServer.Text = "Start Server";
            btOtherServer.UseVisualStyleBackColor = true;
            btOtherServer.Click += btOtherServer_Click;
            // 
            // lbServer
            // 
            lbServer.AutoSize = true;
            lbServer.Location = new System.Drawing.Point(91, 65);
            lbServer.Name = "lbServer";
            lbServer.Size = new System.Drawing.Size(76, 15);
            lbServer.TabIndex = 9;
            lbServer.Text = "other Server :";
            // 
            // btSetExe
            // 
            btSetExe.Location = new System.Drawing.Point(3, 61);
            btSetExe.Name = "btSetExe";
            btSetExe.Size = new System.Drawing.Size(82, 23);
            btSetExe.TabIndex = 8;
            btSetExe.Text = "Set Directory";
            btSetExe.UseVisualStyleBackColor = true;
            btSetExe.Click += btSetExe_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(91, 96);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(81, 15);
            label1.TabIndex = 7;
            label1.Text = "Visual Studio :";
            // 
            // btSetServUOsln
            // 
            btSetServUOsln.Location = new System.Drawing.Point(3, 92);
            btSetServUOsln.Name = "btSetServUOsln";
            btSetServUOsln.Size = new System.Drawing.Size(82, 23);
            btSetServUOsln.TabIndex = 6;
            btSetServUOsln.Text = "Set Directory";
            btSetServUOsln.UseVisualStyleBackColor = true;
            btSetServUOsln.Click += btSetServUOsln_Click;
            // 
            // btServUOsln
            // 
            btServUOsln.Location = new System.Drawing.Point(178, 92);
            btServUOsln.Name = "btServUOsln";
            btServUOsln.Size = new System.Drawing.Size(103, 23);
            btServUOsln.TabIndex = 5;
            btServUOsln.Text = "Start Projekt .sln";
            btServUOsln.UseVisualStyleBackColor = true;
            btServUOsln.Click += btServUOsln_Click;
            // 
            // tbServuoDir
            // 
            tbServuoDir.Location = new System.Drawing.Point(3, 32);
            tbServuoDir.Name = "tbServuoDir";
            tbServuoDir.Size = new System.Drawing.Size(278, 23);
            tbServuoDir.TabIndex = 4;
            // 
            // btOpenServUoDir
            // 
            btOpenServUoDir.Location = new System.Drawing.Point(161, 3);
            btOpenServUoDir.Name = "btOpenServUoDir";
            btOpenServUoDir.Size = new System.Drawing.Size(63, 23);
            btOpenServUoDir.TabIndex = 3;
            btOpenServUoDir.Text = "Open Dir";
            btOpenServUoDir.UseVisualStyleBackColor = true;
            btOpenServUoDir.Click += btOpenServUoDir_Click;
            // 
            // btStartServuo
            // 
            btStartServuo.Location = new System.Drawing.Point(230, 3);
            btStartServuo.Name = "btStartServuo";
            btStartServuo.Size = new System.Drawing.Size(51, 23);
            btStartServuo.TabIndex = 2;
            btStartServuo.Text = "Start";
            btStartServuo.UseVisualStyleBackColor = true;
            btStartServuo.Click += btStartServuo_Click;
            // 
            // lbServuo
            // 
            lbServuo.AutoSize = true;
            lbServuo.Location = new System.Drawing.Point(91, 7);
            lbServuo.Name = "lbServuo";
            lbServuo.Size = new System.Drawing.Size(50, 15);
            lbServuo.TabIndex = 1;
            lbServuo.Text = "ServUo :";
            // 
            // ServerStartBox
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(306, 149);
            Controls.Add(panelServUo);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ServerStartBox";
            Text = "Server StartBox";
            panelServUo.ResumeLayout(false);
            panelServUo.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btSetDirectoryServuo;
        private System.Windows.Forms.Panel panelServUo;
        private System.Windows.Forms.Button btOpenServUoDir;
        private System.Windows.Forms.Button btStartServuo;
        private System.Windows.Forms.Label lbServuo;
        private System.Windows.Forms.TextBox tbServuoDir;
        private System.Windows.Forms.Button btServUOsln;
        private System.Windows.Forms.Button btSetServUOsln;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btOtherServer;
        private System.Windows.Forms.Label lbServer;
        private System.Windows.Forms.Button btSetExe;
    }
}
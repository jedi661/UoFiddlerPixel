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
    partial class ARTMulIDXCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ARTMulIDXCreator));
            textBox1 = new System.Windows.Forms.TextBox();
            btCreateARTIDXMul = new System.Windows.Forms.Button();
            btFileOrder = new System.Windows.Forms.Button();
            textBox2 = new System.Windows.Forms.TextBox();
            lblEntryCount = new System.Windows.Forms.Label();
            btnCountEntries = new System.Windows.Forms.Button();
            btnShowInfo = new System.Windows.Forms.Button();
            textBoxInfo = new System.Windows.Forms.TextBox();
            btnReadArtIdx = new System.Windows.Forms.Button();
            textBoxIndex = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(22, 24);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(181, 23);
            textBox1.TabIndex = 0;
            // 
            // btCreateARTIDXMul
            // 
            btCreateARTIDXMul.Location = new System.Drawing.Point(128, 54);
            btCreateARTIDXMul.Name = "btCreateARTIDXMul";
            btCreateARTIDXMul.Size = new System.Drawing.Size(75, 23);
            btCreateARTIDXMul.TabIndex = 1;
            btCreateARTIDXMul.Text = "Create";
            btCreateARTIDXMul.UseVisualStyleBackColor = true;
            btCreateARTIDXMul.Click += btCreateARTIDXMul_Click;
            // 
            // btFileOrder
            // 
            btFileOrder.Location = new System.Drawing.Point(22, 54);
            btFileOrder.Name = "btFileOrder";
            btFileOrder.Size = new System.Drawing.Size(100, 23);
            btFileOrder.TabIndex = 2;
            btFileOrder.Text = "Select file folder";
            btFileOrder.UseVisualStyleBackColor = true;
            btFileOrder.Click += btFileOrder_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(209, 24);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(66, 23);
            textBox2.TabIndex = 3;
            textBox2.Text = "81884";
            // 
            // lblEntryCount
            // 
            lblEntryCount.AutoSize = true;
            lblEntryCount.Location = new System.Drawing.Point(298, 27);
            lblEntryCount.Name = "lblEntryCount";
            lblEntryCount.Size = new System.Drawing.Size(72, 15);
            lblEntryCount.TabIndex = 4;
            lblEntryCount.Text = "Index Count";
            // 
            // btnCountEntries
            // 
            btnCountEntries.Location = new System.Drawing.Point(298, 54);
            btnCountEntries.Name = "btnCountEntries";
            btnCountEntries.Size = new System.Drawing.Size(115, 23);
            btnCountEntries.TabIndex = 5;
            btnCountEntries.Text = "Count ArtIDX Read";
            btnCountEntries.UseVisualStyleBackColor = true;
            btnCountEntries.Click += btnCountEntries_Click;
            // 
            // btnShowInfo
            // 
            btnShowInfo.Location = new System.Drawing.Point(22, 419);
            btnShowInfo.Name = "btnShowInfo";
            btnShowInfo.Size = new System.Drawing.Size(75, 23);
            btnShowInfo.TabIndex = 9;
            btnShowInfo.Text = "Index All";
            btnShowInfo.UseVisualStyleBackColor = true;
            btnShowInfo.Click += btnShowInfo_Click;
            // 
            // textBoxInfo
            // 
            textBoxInfo.Location = new System.Drawing.Point(22, 158);
            textBoxInfo.Multiline = true;
            textBoxInfo.Name = "textBoxInfo";
            textBoxInfo.Size = new System.Drawing.Size(253, 234);
            textBoxInfo.TabIndex = 10;
            // 
            // btnReadArtIdx
            // 
            btnReadArtIdx.Location = new System.Drawing.Point(103, 419);
            btnReadArtIdx.Name = "btnReadArtIdx";
            btnReadArtIdx.Size = new System.Drawing.Size(120, 23);
            btnReadArtIdx.TabIndex = 11;
            btnReadArtIdx.Text = "Selected index";
            btnReadArtIdx.UseVisualStyleBackColor = true;
            btnReadArtIdx.Click += btnReadArtIdx_Click;
            // 
            // textBoxIndex
            // 
            textBoxIndex.Location = new System.Drawing.Point(232, 420);
            textBoxIndex.Name = "textBoxIndex";
            textBoxIndex.Size = new System.Drawing.Size(43, 23);
            textBoxIndex.TabIndex = 13;
            textBoxIndex.Text = "1";
            textBoxIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(22, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(25, 15);
            label1.TabIndex = 14;
            label1.Text = "Dir:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(209, 6);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(43, 15);
            label2.TabIndex = 15;
            label2.Text = "Count:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(22, 401);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(108, 15);
            label3.TabIndex = 16;
            label3.Text = "Reading the ArtIdx:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(232, 401);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(39, 15);
            label4.TabIndex = 17;
            label4.Text = "Index:";
            // 
            // ARTMulIDXCreator
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(447, 450);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBoxIndex);
            Controls.Add(btnReadArtIdx);
            Controls.Add(textBoxInfo);
            Controls.Add(btnShowInfo);
            Controls.Add(btnCountEntries);
            Controls.Add(lblEntryCount);
            Controls.Add(textBox2);
            Controls.Add(btFileOrder);
            Controls.Add(btCreateARTIDXMul);
            Controls.Add(textBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ARTMulIDXCreator";
            Text = "ARTMulIDXCreator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btCreateARTIDXMul;
        private System.Windows.Forms.Button btFileOrder;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label lblEntryCount;
        private System.Windows.Forms.Button btnCountEntries;
        private System.Windows.Forms.Button btnShowInfo;
        private System.Windows.Forms.TextBox textBoxInfo;
        private System.Windows.Forms.Button btnReadArtIdx;
        private System.Windows.Forms.TextBox textBoxIndex;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}
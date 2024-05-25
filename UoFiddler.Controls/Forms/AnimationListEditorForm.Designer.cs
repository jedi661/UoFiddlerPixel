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

namespace UoFiddler.Controls.Forms
{
    partial class AnimationListEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationListEditorForm));
            richTextBox = new System.Windows.Forms.RichTextBox();
            SearchButton = new System.Windows.Forms.Button();
            searchTextBox = new System.Windows.Forms.TextBox();
            SaveButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // richTextBox
            // 
            richTextBox.Location = new System.Drawing.Point(2, 46);
            richTextBox.Name = "richTextBox";
            richTextBox.Size = new System.Drawing.Size(796, 435);
            richTextBox.TabIndex = 0;
            richTextBox.Text = "";
            // 
            // SearchButton
            // 
            SearchButton.Location = new System.Drawing.Point(2, 5);
            SearchButton.Name = "SearchButton";
            SearchButton.Size = new System.Drawing.Size(85, 35);
            SearchButton.TabIndex = 1;
            SearchButton.Text = "Search";
            SearchButton.UseVisualStyleBackColor = true;
            SearchButton.Click += SearchButton_Click;
            // 
            // searchTextBox
            // 
            searchTextBox.Location = new System.Drawing.Point(93, 12);
            searchTextBox.Name = "searchTextBox";
            searchTextBox.Size = new System.Drawing.Size(705, 23);
            searchTextBox.TabIndex = 2;
            // 
            // SaveButton
            // 
            SaveButton.Location = new System.Drawing.Point(2, 487);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(796, 23);
            SaveButton.TabIndex = 3;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // AnimationListEditorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 511);
            Controls.Add(SaveButton);
            Controls.Add(searchTextBox);
            Controls.Add(SearchButton);
            Controls.Add(richTextBox);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "AnimationListEditorForm";
            Text = "AnimationList XML Editor";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button SaveButton;
    }
}
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
    partial class ItemSearchForm
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
            ButtonSearchNextName = new System.Windows.Forms.Button();
            textBoxGraphic = new System.Windows.Forms.TextBox();
            textBoxItemName = new System.Windows.Forms.TextBox();
            SearchButtonID = new System.Windows.Forms.Button();
            SearchButtonName = new System.Windows.Forms.Button();
            ListBoxSearch = new System.Windows.Forms.ListBox();
            pictureBoxGraphic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGraphic).BeginInit();
            SuspendLayout();
            // 
            // ButtonSearchNextName
            // 
            ButtonSearchNextName.Location = new System.Drawing.Point(392, 12);
            ButtonSearchNextName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ButtonSearchNextName.Name = "ButtonSearchNextName";
            ButtonSearchNextName.Size = new System.Drawing.Size(88, 52);
            ButtonSearchNextName.TabIndex = 5;
            ButtonSearchNextName.Text = "Search Next";
            ButtonSearchNextName.UseVisualStyleBackColor = true;
            ButtonSearchNextName.Click += SearchNextName;
            // 
            // textBoxGraphic
            // 
            textBoxGraphic.Location = new System.Drawing.Point(8, 12);
            textBoxGraphic.Name = "textBoxGraphic";
            textBoxGraphic.Size = new System.Drawing.Size(298, 23);
            textBoxGraphic.TabIndex = 6;
            textBoxGraphic.KeyDown += OnKeyDownSearch;
            // 
            // textBoxItemName
            // 
            textBoxItemName.Location = new System.Drawing.Point(8, 41);
            textBoxItemName.Name = "textBoxItemName";
            textBoxItemName.Size = new System.Drawing.Size(298, 23);
            textBoxItemName.TabIndex = 7;
            textBoxItemName.KeyDown += OnKeyDownSearch;
            // 
            // SearchButtonID
            // 
            SearchButtonID.Location = new System.Drawing.Point(312, 12);
            SearchButtonID.Name = "SearchButtonID";
            SearchButtonID.Size = new System.Drawing.Size(75, 23);
            SearchButtonID.TabIndex = 8;
            SearchButtonID.Text = "Search ID";
            SearchButtonID.UseVisualStyleBackColor = true;
            SearchButtonID.Click += Search_Graphic;
            // 
            // SearchButtonName
            // 
            SearchButtonName.Location = new System.Drawing.Point(312, 41);
            SearchButtonName.Name = "SearchButtonName";
            SearchButtonName.Size = new System.Drawing.Size(75, 23);
            SearchButtonName.TabIndex = 9;
            SearchButtonName.Text = "Name";
            SearchButtonName.UseVisualStyleBackColor = true;
            SearchButtonName.Click += Search_ItemName;
            // 
            // ListBoxSearch
            // 
            ListBoxSearch.FormattingEnabled = true;
            ListBoxSearch.ItemHeight = 15;
            ListBoxSearch.Location = new System.Drawing.Point(312, 70);
            ListBoxSearch.Name = "ListBoxSearch";
            ListBoxSearch.Size = new System.Drawing.Size(168, 229);
            ListBoxSearch.TabIndex = 10;
            ListBoxSearch.SelectedIndexChanged += ListBoxSearch_SelectedIndexChanged;
            // 
            // pictureBoxGraphic
            // 
            pictureBoxGraphic.Location = new System.Drawing.Point(8, 70);
            pictureBoxGraphic.Name = "pictureBoxGraphic";
            pictureBoxGraphic.Size = new System.Drawing.Size(298, 229);
            pictureBoxGraphic.TabIndex = 11;
            pictureBoxGraphic.TabStop = false;
            // 
            // ItemSearchForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(485, 306);
            Controls.Add(pictureBoxGraphic);
            Controls.Add(ListBoxSearch);
            Controls.Add(SearchButtonName);
            Controls.Add(SearchButtonID);
            Controls.Add(textBoxItemName);
            Controls.Add(textBoxGraphic);
            Controls.Add(ButtonSearchNextName);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "ItemSearchForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Item Search";
            ((System.ComponentModel.ISupportInitialize)pictureBoxGraphic).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button ButtonSearchNextName;
        private System.Windows.Forms.TextBox textBoxGraphic;
        private System.Windows.Forms.TextBox textBoxItemName;
        private System.Windows.Forms.Button SearchButtonID;
        private System.Windows.Forms.Button SearchButtonName;
        private System.Windows.Forms.ListBox ListBoxSearch;
        private System.Windows.Forms.PictureBox pictureBoxGraphic;
    }
}
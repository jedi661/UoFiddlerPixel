// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.ArtLibrary.Forms
{
    partial class ArtLibraryCategoryForm
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
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            groupBoxCategories = new System.Windows.Forms.GroupBox();
            treeViewCategories = new System.Windows.Forms.TreeView();
            panelCategoryButtons = new System.Windows.Forms.Panel();
            textBoxCategory = new System.Windows.Forms.TextBox();
            buttonAddCategory = new System.Windows.Forms.Button();
            buttonAddSubCategory = new System.Windows.Forms.Button();
            buttonRenameCategory = new System.Windows.Forms.Button();
            buttonDeleteCategory = new System.Windows.Forms.Button();
            groupBoxTags = new System.Windows.Forms.GroupBox();
            listBoxTags = new System.Windows.Forms.ListBox();
            panelTagButtons = new System.Windows.Forms.Panel();
            textBoxTag = new System.Windows.Forms.TextBox();
            buttonAddTag = new System.Windows.Forms.Button();
            buttonRenameTag = new System.Windows.Forms.Button();
            buttonDeleteTag = new System.Windows.Forms.Button();
            panelBottom = new System.Windows.Forms.Panel();
            labelHint = new System.Windows.Forms.Label();
            buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            groupBoxCategories.SuspendLayout();
            panelCategoryButtons.SuspendLayout();
            groupBoxTags.SuspendLayout();
            panelTagButtons.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Name = "splitContainerMain";
            //
            // splitContainerMain.Panel1
            //
            splitContainerMain.Panel1.Controls.Add(groupBoxCategories);
            splitContainerMain.Panel1.Padding = new System.Windows.Forms.Padding(6);
            //
            // splitContainerMain.Panel2
            //
            splitContainerMain.Panel2.Controls.Add(groupBoxTags);
            splitContainerMain.Panel2.Padding = new System.Windows.Forms.Padding(6);
            splitContainerMain.Size = new System.Drawing.Size(704, 421);
            splitContainerMain.SplitterDistance = 400;
            splitContainerMain.TabIndex = 0;
            //
            // groupBoxCategories
            //
            groupBoxCategories.Controls.Add(treeViewCategories);
            groupBoxCategories.Controls.Add(panelCategoryButtons);
            groupBoxCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxCategories.Name = "groupBoxCategories";
            groupBoxCategories.Size = new System.Drawing.Size(388, 409);
            groupBoxCategories.TabIndex = 0;
            groupBoxCategories.TabStop = false;
            groupBoxCategories.Text = "Kategorien (Ziehen = in andere Kategorie verschieben)";
            //
            // treeViewCategories
            //
            treeViewCategories.AllowDrop = true;
            treeViewCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewCategories.HideSelection = false;
            treeViewCategories.Name = "treeViewCategories";
            treeViewCategories.Size = new System.Drawing.Size(382, 303);
            treeViewCategories.TabIndex = 1;
            treeViewCategories.AfterSelect += TreeViewCategories_AfterSelect;
            treeViewCategories.ItemDrag += TreeViewCategories_ItemDrag;
            treeViewCategories.DragDrop += TreeViewCategories_DragDrop;
            treeViewCategories.DragOver += TreeViewCategories_DragOver;
            //
            // panelCategoryButtons
            //
            panelCategoryButtons.Controls.Add(textBoxCategory);
            panelCategoryButtons.Controls.Add(buttonAddCategory);
            panelCategoryButtons.Controls.Add(buttonAddSubCategory);
            panelCategoryButtons.Controls.Add(buttonRenameCategory);
            panelCategoryButtons.Controls.Add(buttonDeleteCategory);
            panelCategoryButtons.Dock = System.Windows.Forms.DockStyle.Top;
            panelCategoryButtons.Name = "panelCategoryButtons";
            panelCategoryButtons.Size = new System.Drawing.Size(382, 66);
            panelCategoryButtons.TabIndex = 0;
            //
            // textBoxCategory
            //
            textBoxCategory.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxCategory.Location = new System.Drawing.Point(4, 4);
            textBoxCategory.Name = "textBoxCategory";
            textBoxCategory.PlaceholderText = "Name der Kategorie";
            textBoxCategory.Size = new System.Drawing.Size(374, 23);
            textBoxCategory.TabIndex = 0;
            //
            // buttonAddCategory
            //
            buttonAddCategory.Location = new System.Drawing.Point(4, 33);
            buttonAddCategory.Name = "buttonAddCategory";
            buttonAddCategory.Size = new System.Drawing.Size(70, 27);
            buttonAddCategory.TabIndex = 1;
            buttonAddCategory.Text = "Neu";
            buttonAddCategory.UseVisualStyleBackColor = true;
            buttonAddCategory.Click += ButtonAddCategory_Click;
            //
            // buttonAddSubCategory
            //
            buttonAddSubCategory.Location = new System.Drawing.Point(80, 33);
            buttonAddSubCategory.Name = "buttonAddSubCategory";
            buttonAddSubCategory.Size = new System.Drawing.Size(120, 27);
            buttonAddSubCategory.TabIndex = 2;
            buttonAddSubCategory.Text = "Unterkategorie";
            buttonAddSubCategory.UseVisualStyleBackColor = true;
            buttonAddSubCategory.Click += ButtonAddSubCategory_Click;
            //
            // buttonRenameCategory
            //
            buttonRenameCategory.Location = new System.Drawing.Point(206, 33);
            buttonRenameCategory.Name = "buttonRenameCategory";
            buttonRenameCategory.Size = new System.Drawing.Size(90, 27);
            buttonRenameCategory.TabIndex = 3;
            buttonRenameCategory.Text = "Umbenennen";
            buttonRenameCategory.UseVisualStyleBackColor = true;
            buttonRenameCategory.Click += ButtonRenameCategory_Click;
            //
            // buttonDeleteCategory
            //
            buttonDeleteCategory.Location = new System.Drawing.Point(302, 33);
            buttonDeleteCategory.Name = "buttonDeleteCategory";
            buttonDeleteCategory.Size = new System.Drawing.Size(76, 27);
            buttonDeleteCategory.TabIndex = 4;
            buttonDeleteCategory.Text = "Löschen";
            buttonDeleteCategory.UseVisualStyleBackColor = true;
            buttonDeleteCategory.Click += ButtonDeleteCategory_Click;
            //
            // groupBoxTags
            //
            groupBoxTags.Controls.Add(listBoxTags);
            groupBoxTags.Controls.Add(panelTagButtons);
            groupBoxTags.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBoxTags.Name = "groupBoxTags";
            groupBoxTags.Size = new System.Drawing.Size(288, 409);
            groupBoxTags.TabIndex = 0;
            groupBoxTags.TabStop = false;
            groupBoxTags.Text = "Tags";
            //
            // listBoxTags
            //
            listBoxTags.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxTags.IntegralHeight = false;
            listBoxTags.ItemHeight = 15;
            listBoxTags.Name = "listBoxTags";
            listBoxTags.Size = new System.Drawing.Size(282, 303);
            listBoxTags.TabIndex = 1;
            listBoxTags.SelectedIndexChanged += ListBoxTags_SelectedIndexChanged;
            //
            // panelTagButtons
            //
            panelTagButtons.Controls.Add(textBoxTag);
            panelTagButtons.Controls.Add(buttonAddTag);
            panelTagButtons.Controls.Add(buttonRenameTag);
            panelTagButtons.Controls.Add(buttonDeleteTag);
            panelTagButtons.Dock = System.Windows.Forms.DockStyle.Top;
            panelTagButtons.Name = "panelTagButtons";
            panelTagButtons.Size = new System.Drawing.Size(282, 66);
            panelTagButtons.TabIndex = 0;
            //
            // textBoxTag
            //
            textBoxTag.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxTag.Location = new System.Drawing.Point(4, 4);
            textBoxTag.Name = "textBoxTag";
            textBoxTag.PlaceholderText = "Tag";
            textBoxTag.Size = new System.Drawing.Size(274, 23);
            textBoxTag.TabIndex = 0;
            //
            // buttonAddTag
            //
            buttonAddTag.Location = new System.Drawing.Point(4, 33);
            buttonAddTag.Name = "buttonAddTag";
            buttonAddTag.Size = new System.Drawing.Size(80, 27);
            buttonAddTag.TabIndex = 1;
            buttonAddTag.Text = "Hinzufügen";
            buttonAddTag.UseVisualStyleBackColor = true;
            buttonAddTag.Click += ButtonAddTag_Click;
            //
            // buttonRenameTag
            //
            buttonRenameTag.Location = new System.Drawing.Point(90, 33);
            buttonRenameTag.Name = "buttonRenameTag";
            buttonRenameTag.Size = new System.Drawing.Size(90, 27);
            buttonRenameTag.TabIndex = 2;
            buttonRenameTag.Text = "Umbenennen";
            buttonRenameTag.UseVisualStyleBackColor = true;
            buttonRenameTag.Click += ButtonRenameTag_Click;
            //
            // buttonDeleteTag
            //
            buttonDeleteTag.Location = new System.Drawing.Point(186, 33);
            buttonDeleteTag.Name = "buttonDeleteTag";
            buttonDeleteTag.Size = new System.Drawing.Size(76, 27);
            buttonDeleteTag.TabIndex = 3;
            buttonDeleteTag.Text = "Löschen";
            buttonDeleteTag.UseVisualStyleBackColor = true;
            buttonDeleteTag.Click += ButtonDeleteTag_Click;
            //
            // panelBottom
            //
            panelBottom.Controls.Add(labelHint);
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Location = new System.Drawing.Point(0, 421);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new System.Drawing.Size(704, 40);
            panelBottom.TabIndex = 1;
            //
            // labelHint
            //
            labelHint.AutoSize = true;
            labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelHint.Location = new System.Drawing.Point(9, 13);
            labelHint.Name = "labelHint";
            labelHint.Size = new System.Drawing.Size(380, 15);
            labelHint.TabIndex = 0;
            labelHint.Text = "Name eingeben, dann Neu / Unterkategorie / Umbenennen klicken.";
            //
            // buttonClose
            //
            buttonClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Location = new System.Drawing.Point(607, 7);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(85, 27);
            buttonClose.TabIndex = 1;
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // ArtLibraryCategoryForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(704, 461);
            Controls.Add(splitContainerMain);
            Controls.Add(panelBottom);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 360);
            Name = "ArtLibraryCategoryForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Kategorien und Tags verwalten";
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            groupBoxCategories.ResumeLayout(false);
            panelCategoryButtons.ResumeLayout(false);
            panelCategoryButtons.PerformLayout();
            groupBoxTags.ResumeLayout(false);
            panelTagButtons.ResumeLayout(false);
            panelTagButtons.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.GroupBox groupBoxCategories;
        private System.Windows.Forms.TreeView treeViewCategories;
        private System.Windows.Forms.Panel panelCategoryButtons;
        private System.Windows.Forms.TextBox textBoxCategory;
        private System.Windows.Forms.Button buttonAddCategory;
        private System.Windows.Forms.Button buttonAddSubCategory;
        private System.Windows.Forms.Button buttonRenameCategory;
        private System.Windows.Forms.Button buttonDeleteCategory;
        private System.Windows.Forms.GroupBox groupBoxTags;
        private System.Windows.Forms.ListBox listBoxTags;
        private System.Windows.Forms.Panel panelTagButtons;
        private System.Windows.Forms.TextBox textBoxTag;
        private System.Windows.Forms.Button buttonAddTag;
        private System.Windows.Forms.Button buttonRenameTag;
        private System.Windows.Forms.Button buttonDeleteTag;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Button buttonClose;
    }
}

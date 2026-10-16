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

namespace UoFiddler.Controls.GumpLibrary.Forms
{
    partial class GumpLibraryCategoryForm
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
            treeViewCategories = new System.Windows.Forms.TreeView();
            buttonAddCategory = new System.Windows.Forms.Button();
            buttonAddSubCategory = new System.Windows.Forms.Button();
            buttonDeleteCategory = new System.Windows.Forms.Button();
            buttonCategoryUp = new System.Windows.Forms.Button();
            buttonCategoryDown = new System.Windows.Forms.Button();
            buttonCategoryToTop = new System.Windows.Forms.Button();
            labelCatName = new System.Windows.Forms.Label();
            textBoxCategoryName = new System.Windows.Forms.TextBox();
            labelCatDescription = new System.Windows.Forms.Label();
            textBoxCategoryDescription = new System.Windows.Forms.TextBox();
            buttonCategoryColor = new System.Windows.Forms.Button();
            buttonCategoryNoColor = new System.Windows.Forms.Button();
            buttonCategoryApply = new System.Windows.Forms.Button();
            flowLayoutPanelCategoryButtons = new System.Windows.Forms.FlowLayoutPanel();
            labelCategoryHint = new System.Windows.Forms.Label();
            tabPageCategories = new System.Windows.Forms.TabPage();
            listBoxTags = new System.Windows.Forms.ListBox();
            buttonAddTag = new System.Windows.Forms.Button();
            buttonRenameTag = new System.Windows.Forms.Button();
            buttonDeleteTag = new System.Windows.Forms.Button();
            buttonCleanTags = new System.Windows.Forms.Button();
            flowLayoutPanelTagButtons = new System.Windows.Forms.FlowLayoutPanel();
            tabPageTags = new System.Windows.Forms.TabPage();
            tabControl = new System.Windows.Forms.TabControl();
            buttonClose = new System.Windows.Forms.Button();
            panelBottom = new System.Windows.Forms.Panel();
            colorDialog = new System.Windows.Forms.ColorDialog();
            flowLayoutPanelCategoryButtons.SuspendLayout();
            tabPageCategories.SuspendLayout();
            flowLayoutPanelTagButtons.SuspendLayout();
            tabPageTags.SuspendLayout();
            tabControl.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // treeViewCategories
            // 
            treeViewCategories.AllowDrop = true;
            treeViewCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewCategories.HideSelection = false;
            treeViewCategories.Name = "treeViewCategories";
            treeViewCategories.TabIndex = 0;
            treeViewCategories.AfterSelect += TreeViewCategories_AfterSelect;
            treeViewCategories.ItemDrag += TreeViewCategories_ItemDrag;
            treeViewCategories.DragOver += TreeViewCategories_DragOver;
            treeViewCategories.DragDrop += TreeViewCategories_DragDrop;
            // 
            // buttonAddCategory
            // 
            buttonAddCategory.Name = "buttonAddCategory";
            buttonAddCategory.Size = new System.Drawing.Size(150, 27);
            buttonAddCategory.Text = "Neue Kategorie";
            buttonAddCategory.UseVisualStyleBackColor = true;
            buttonAddCategory.Click += ButtonAddCategory_Click;
            // 
            // buttonAddSubCategory
            // 
            buttonAddSubCategory.Name = "buttonAddSubCategory";
            buttonAddSubCategory.Size = new System.Drawing.Size(150, 27);
            buttonAddSubCategory.Text = "Neue Unterkategorie";
            buttonAddSubCategory.UseVisualStyleBackColor = true;
            buttonAddSubCategory.Click += ButtonAddSubCategory_Click;
            // 
            // buttonDeleteCategory
            // 
            buttonDeleteCategory.Name = "buttonDeleteCategory";
            buttonDeleteCategory.Size = new System.Drawing.Size(150, 27);
            buttonDeleteCategory.Text = "Löschen";
            buttonDeleteCategory.UseVisualStyleBackColor = true;
            buttonDeleteCategory.Click += ButtonDeleteCategory_Click;
            // 
            // buttonCategoryUp
            // 
            buttonCategoryUp.Name = "buttonCategoryUp";
            buttonCategoryUp.Size = new System.Drawing.Size(150, 27);
            buttonCategoryUp.Text = "Nach oben";
            buttonCategoryUp.UseVisualStyleBackColor = true;
            buttonCategoryUp.Click += ButtonCategoryUp_Click;
            // 
            // buttonCategoryDown
            // 
            buttonCategoryDown.Name = "buttonCategoryDown";
            buttonCategoryDown.Size = new System.Drawing.Size(150, 27);
            buttonCategoryDown.Text = "Nach unten";
            buttonCategoryDown.UseVisualStyleBackColor = true;
            buttonCategoryDown.Click += ButtonCategoryDown_Click;
            // 
            // buttonCategoryToTop
            // 
            buttonCategoryToTop.Name = "buttonCategoryToTop";
            buttonCategoryToTop.Size = new System.Drawing.Size(150, 27);
            buttonCategoryToTop.Text = "Eine Ebene höher";
            buttonCategoryToTop.UseVisualStyleBackColor = true;
            buttonCategoryToTop.Click += ButtonCategoryToTop_Click;
            // 
            // labelCatName
            // 
            labelCatName.AutoSize = true;
            labelCatName.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            labelCatName.Name = "labelCatName";
            labelCatName.Text = "Name:";
            // 
            // textBoxCategoryName
            // 
            textBoxCategoryName.Name = "textBoxCategoryName";
            textBoxCategoryName.Size = new System.Drawing.Size(150, 23);
            // 
            // labelCatDescription
            // 
            labelCatDescription.AutoSize = true;
            labelCatDescription.Name = "labelCatDescription";
            labelCatDescription.Text = "Beschreibung:";
            // 
            // textBoxCategoryDescription
            // 
            textBoxCategoryDescription.Multiline = true;
            textBoxCategoryDescription.Name = "textBoxCategoryDescription";
            textBoxCategoryDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxCategoryDescription.Size = new System.Drawing.Size(150, 60);
            // 
            // buttonCategoryColor
            // 
            buttonCategoryColor.Name = "buttonCategoryColor";
            buttonCategoryColor.Size = new System.Drawing.Size(72, 27);
            buttonCategoryColor.Text = "Farbe...";
            buttonCategoryColor.UseVisualStyleBackColor = true;
            buttonCategoryColor.Click += ButtonCategoryColor_Click;
            // 
            // buttonCategoryNoColor
            // 
            buttonCategoryNoColor.Name = "buttonCategoryNoColor";
            buttonCategoryNoColor.Size = new System.Drawing.Size(72, 27);
            buttonCategoryNoColor.Text = "Keine";
            buttonCategoryNoColor.UseVisualStyleBackColor = true;
            buttonCategoryNoColor.Click += ButtonCategoryNoColor_Click;
            // 
            // buttonCategoryApply
            // 
            buttonCategoryApply.Name = "buttonCategoryApply";
            buttonCategoryApply.Size = new System.Drawing.Size(150, 27);
            buttonCategoryApply.Text = "Übernehmen";
            buttonCategoryApply.UseVisualStyleBackColor = true;
            buttonCategoryApply.Click += ButtonCategoryApply_Click;
            // 
            // flowLayoutPanelCategoryButtons
            // 
            flowLayoutPanelCategoryButtons.Controls.Add(buttonAddCategory);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonAddSubCategory);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonDeleteCategory);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryUp);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryDown);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryToTop);
            flowLayoutPanelCategoryButtons.Controls.Add(labelCatName);
            flowLayoutPanelCategoryButtons.Controls.Add(textBoxCategoryName);
            flowLayoutPanelCategoryButtons.Controls.Add(labelCatDescription);
            flowLayoutPanelCategoryButtons.Controls.Add(textBoxCategoryDescription);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryColor);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryNoColor);
            flowLayoutPanelCategoryButtons.Controls.Add(buttonCategoryApply);
            flowLayoutPanelCategoryButtons.Dock = System.Windows.Forms.DockStyle.Right;
            flowLayoutPanelCategoryButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelCategoryButtons.Name = "flowLayoutPanelCategoryButtons";
            flowLayoutPanelCategoryButtons.Size = new System.Drawing.Size(162, 400);
            // 
            // labelCategoryHint
            // 
            labelCategoryHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            labelCategoryHint.ForeColor = System.Drawing.SystemColors.GrayText;
            labelCategoryHint.Name = "labelCategoryHint";
            labelCategoryHint.Size = new System.Drawing.Size(300, 34);
            labelCategoryHint.Text = "Ziehen mit der Maus = in eine andere Kategorie einsortieren. Ein Gump kann in beliebig vielen Kategorien sein. Löschen entfernt nur die Kategorie, nicht die Gumps.";
            // 
            // tabPageCategories
            // 
            tabPageCategories.Controls.Add(treeViewCategories);
            tabPageCategories.Controls.Add(labelCategoryHint);
            tabPageCategories.Controls.Add(flowLayoutPanelCategoryButtons);
            tabPageCategories.Name = "tabPageCategories";
            tabPageCategories.Padding = new System.Windows.Forms.Padding(3);
            tabPageCategories.Text = "Kategorien";
            tabPageCategories.UseVisualStyleBackColor = true;
            // 
            // listBoxTags
            // 
            listBoxTags.Dock = System.Windows.Forms.DockStyle.Fill;
            listBoxTags.IntegralHeight = false;
            listBoxTags.Name = "listBoxTags";
            listBoxTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            // 
            // buttonAddTag
            // 
            buttonAddTag.Name = "buttonAddTag";
            buttonAddTag.Size = new System.Drawing.Size(150, 27);
            buttonAddTag.Text = "Neuer Tag";
            buttonAddTag.UseVisualStyleBackColor = true;
            buttonAddTag.Click += ButtonAddTag_Click;
            // 
            // buttonRenameTag
            // 
            buttonRenameTag.Name = "buttonRenameTag";
            buttonRenameTag.Size = new System.Drawing.Size(150, 27);
            buttonRenameTag.Text = "Umbenennen";
            buttonRenameTag.UseVisualStyleBackColor = true;
            buttonRenameTag.Click += ButtonRenameTag_Click;
            // 
            // buttonDeleteTag
            // 
            buttonDeleteTag.Name = "buttonDeleteTag";
            buttonDeleteTag.Size = new System.Drawing.Size(150, 27);
            buttonDeleteTag.Text = "Löschen";
            buttonDeleteTag.UseVisualStyleBackColor = true;
            buttonDeleteTag.Click += ButtonDeleteTag_Click;
            // 
            // buttonCleanTags
            // 
            buttonCleanTags.Name = "buttonCleanTags";
            buttonCleanTags.Size = new System.Drawing.Size(150, 27);
            buttonCleanTags.Text = "Unbenutzte entfernen";
            buttonCleanTags.UseVisualStyleBackColor = true;
            buttonCleanTags.Click += ButtonCleanTags_Click;
            // 
            // flowLayoutPanelTagButtons
            // 
            flowLayoutPanelTagButtons.Controls.Add(buttonAddTag);
            flowLayoutPanelTagButtons.Controls.Add(buttonRenameTag);
            flowLayoutPanelTagButtons.Controls.Add(buttonDeleteTag);
            flowLayoutPanelTagButtons.Controls.Add(buttonCleanTags);
            flowLayoutPanelTagButtons.Dock = System.Windows.Forms.DockStyle.Right;
            flowLayoutPanelTagButtons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanelTagButtons.Name = "flowLayoutPanelTagButtons";
            flowLayoutPanelTagButtons.Size = new System.Drawing.Size(162, 400);
            // 
            // tabPageTags
            // 
            tabPageTags.Controls.Add(listBoxTags);
            tabPageTags.Controls.Add(flowLayoutPanelTagButtons);
            tabPageTags.Name = "tabPageTags";
            tabPageTags.Padding = new System.Windows.Forms.Padding(3);
            tabPageTags.Text = "Tags (Suchbegriffe)";
            tabPageTags.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageCategories);
            tabControl.Controls.Add(tabPageTags);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            // 
            // buttonClose
            // 
            buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonClose.Dock = System.Windows.Forms.DockStyle.Right;
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(100, 27);
            buttonClose.Text = "Schließen";
            buttonClose.UseVisualStyleBackColor = true;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonClose);
            panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new System.Windows.Forms.Padding(4);
            panelBottom.Size = new System.Drawing.Size(560, 35);
            // 
            // GumpLibraryCategoryForm
            // 
            AcceptButton = buttonClose;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new System.Drawing.Size(600, 520);
            Controls.Add(tabControl);
            Controls.Add(panelBottom);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(480, 420);
            Name = "GumpLibraryCategoryForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Kategorien und Tags verwalten";
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            tabControl.ResumeLayout(false);
            tabControl.PerformLayout();
            tabPageTags.ResumeLayout(false);
            tabPageTags.PerformLayout();
            flowLayoutPanelTagButtons.ResumeLayout(false);
            flowLayoutPanelTagButtons.PerformLayout();
            tabPageCategories.ResumeLayout(false);
            tabPageCategories.PerformLayout();
            flowLayoutPanelCategoryButtons.ResumeLayout(false);
            flowLayoutPanelCategoryButtons.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TreeView treeViewCategories;
        private System.Windows.Forms.Button buttonAddCategory;
        private System.Windows.Forms.Button buttonAddSubCategory;
        private System.Windows.Forms.Button buttonDeleteCategory;
        private System.Windows.Forms.Button buttonCategoryUp;
        private System.Windows.Forms.Button buttonCategoryDown;
        private System.Windows.Forms.Button buttonCategoryToTop;
        private System.Windows.Forms.Label labelCatName;
        private System.Windows.Forms.TextBox textBoxCategoryName;
        private System.Windows.Forms.Label labelCatDescription;
        private System.Windows.Forms.TextBox textBoxCategoryDescription;
        private System.Windows.Forms.Button buttonCategoryColor;
        private System.Windows.Forms.Button buttonCategoryNoColor;
        private System.Windows.Forms.Button buttonCategoryApply;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelCategoryButtons;
        private System.Windows.Forms.Label labelCategoryHint;
        private System.Windows.Forms.TabPage tabPageCategories;
        private System.Windows.Forms.ListBox listBoxTags;
        private System.Windows.Forms.Button buttonAddTag;
        private System.Windows.Forms.Button buttonRenameTag;
        private System.Windows.Forms.Button buttonDeleteTag;
        private System.Windows.Forms.Button buttonCleanTags;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTagButtons;
        private System.Windows.Forms.TabPage tabPageTags;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}

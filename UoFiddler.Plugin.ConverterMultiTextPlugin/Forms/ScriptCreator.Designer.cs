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
    partial class ScriptCreator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptCreator));
            panelBaseScript = new System.Windows.Forms.Panel();
            tabControlItems = new System.Windows.Forms.TabControl();
            tabPageBook = new System.Windows.Forms.TabPage();
            btSaveBookScript = new System.Windows.Forms.Button();
            btClipBoard = new System.Windows.Forms.Button();
            btColorSet = new System.Windows.Forms.Button();
            lbColorHueBook = new System.Windows.Forms.Label();
            buttonTest = new System.Windows.Forms.Button();
            lbIDAdressBook = new System.Windows.Forms.Label();
            lbFileName = new System.Windows.Forms.Label();
            lbBookAutor = new System.Windows.Forms.Label();
            lbBookTile = new System.Windows.Forms.Label();
            textBoxScriptResult = new System.Windows.Forms.TextBox();
            buttonCreateBookScript = new System.Windows.Forms.Button();
            textBoxBookContent = new System.Windows.Forms.TextBox();
            comboBoxEnableAutoformat = new System.Windows.Forms.ComboBox();
            textBoxGrafikID = new System.Windows.Forms.TextBox();
            comboBoxBookGrafic = new System.Windows.Forms.ComboBox();
            textBoxCoverHue = new System.Windows.Forms.TextBox();
            textBoxBookFilename = new System.Windows.Forms.TextBox();
            textBoxBookAuthor = new System.Windows.Forms.TextBox();
            textBoxBookTitle = new System.Windows.Forms.TextBox();
            tabPageAnimation = new System.Windows.Forms.TabPage();
            tabPageItem = new System.Windows.Forms.TabPage();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            panelBaseScript.SuspendLayout();
            tabControlItems.SuspendLayout();
            tabPageBook.SuspendLayout();
            SuspendLayout();
            // 
            // panelBaseScript
            // 
            panelBaseScript.Controls.Add(tabControlItems);
            panelBaseScript.Location = new System.Drawing.Point(12, 43);
            panelBaseScript.Name = "panelBaseScript";
            panelBaseScript.Size = new System.Drawing.Size(776, 612);
            panelBaseScript.TabIndex = 0;
            // 
            // tabControlItems
            // 
            tabControlItems.Controls.Add(tabPageBook);
            tabControlItems.Controls.Add(tabPageAnimation);
            tabControlItems.Controls.Add(tabPageItem);
            tabControlItems.Location = new System.Drawing.Point(3, 3);
            tabControlItems.Name = "tabControlItems";
            tabControlItems.SelectedIndex = 0;
            tabControlItems.Size = new System.Drawing.Size(770, 606);
            tabControlItems.TabIndex = 0;
            // 
            // tabPageBook
            // 
            tabPageBook.Controls.Add(btSaveBookScript);
            tabPageBook.Controls.Add(btClipBoard);
            tabPageBook.Controls.Add(btColorSet);
            tabPageBook.Controls.Add(lbColorHueBook);
            tabPageBook.Controls.Add(buttonTest);
            tabPageBook.Controls.Add(lbIDAdressBook);
            tabPageBook.Controls.Add(lbFileName);
            tabPageBook.Controls.Add(lbBookAutor);
            tabPageBook.Controls.Add(lbBookTile);
            tabPageBook.Controls.Add(textBoxScriptResult);
            tabPageBook.Controls.Add(buttonCreateBookScript);
            tabPageBook.Controls.Add(textBoxBookContent);
            tabPageBook.Controls.Add(comboBoxEnableAutoformat);
            tabPageBook.Controls.Add(textBoxGrafikID);
            tabPageBook.Controls.Add(comboBoxBookGrafic);
            tabPageBook.Controls.Add(textBoxCoverHue);
            tabPageBook.Controls.Add(textBoxBookFilename);
            tabPageBook.Controls.Add(textBoxBookAuthor);
            tabPageBook.Controls.Add(textBoxBookTitle);
            tabPageBook.Location = new System.Drawing.Point(4, 24);
            tabPageBook.Name = "tabPageBook";
            tabPageBook.Padding = new System.Windows.Forms.Padding(3);
            tabPageBook.Size = new System.Drawing.Size(762, 578);
            tabPageBook.TabIndex = 0;
            tabPageBook.Text = "Book";
            tabPageBook.UseVisualStyleBackColor = true;
            // 
            // btSaveBookScript
            // 
            btSaveBookScript.Location = new System.Drawing.Point(396, 279);
            btSaveBookScript.Name = "btSaveBookScript";
            btSaveBookScript.Size = new System.Drawing.Size(40, 23);
            btSaveBookScript.TabIndex = 18;
            btSaveBookScript.Text = "Save";
            btSaveBookScript.UseVisualStyleBackColor = true;
            btSaveBookScript.Click += btSaveBookScript_Click;
            // 
            // btClipBoard
            // 
            btClipBoard.Location = new System.Drawing.Point(329, 279);
            btClipBoard.Name = "btClipBoard";
            btClipBoard.Size = new System.Drawing.Size(67, 23);
            btClipBoard.TabIndex = 17;
            btClipBoard.Text = "Clipboard";
            btClipBoard.UseVisualStyleBackColor = true;
            btClipBoard.Click += btClipBoard_Click;
            // 
            // btColorSet
            // 
            btColorSet.Location = new System.Drawing.Point(157, 279);
            btColorSet.Name = "btColorSet";
            btColorSet.Size = new System.Drawing.Size(46, 23);
            btColorSet.TabIndex = 16;
            btColorSet.Text = "Color";
            btColorSet.UseVisualStyleBackColor = true;
            btColorSet.Click += btColorSet_Click;
            // 
            // lbColorHueBook
            // 
            lbColorHueBook.AutoSize = true;
            lbColorHueBook.Location = new System.Drawing.Point(6, 261);
            lbColorHueBook.Name = "lbColorHueBook";
            lbColorHueBook.Size = new System.Drawing.Size(91, 15);
            lbColorHueBook.TabIndex = 15;
            lbColorHueBook.Text = "Color Hue Book";
            // 
            // buttonTest
            // 
            buttonTest.Location = new System.Drawing.Point(292, 279);
            buttonTest.Name = "buttonTest";
            buttonTest.Size = new System.Drawing.Size(37, 23);
            buttonTest.TabIndex = 14;
            buttonTest.Text = "Test";
            buttonTest.UseVisualStyleBackColor = true;
            buttonTest.Click += buttonTest_Click;
            // 
            // lbIDAdressBook
            // 
            lbIDAdressBook.AutoSize = true;
            lbIDAdressBook.Location = new System.Drawing.Point(6, 158);
            lbIDAdressBook.Name = "lbIDAdressBook";
            lbIDAdressBook.Size = new System.Drawing.Size(92, 15);
            lbIDAdressBook.TabIndex = 13;
            lbIDAdressBook.Text = "ID Adresse Book";
            // 
            // lbFileName
            // 
            lbFileName.AutoSize = true;
            lbFileName.Location = new System.Drawing.Point(6, 212);
            lbFileName.Name = "lbFileName";
            lbFileName.Size = new System.Drawing.Size(60, 15);
            lbFileName.TabIndex = 12;
            lbFileName.Text = "File Name";
            // 
            // lbBookAutor
            // 
            lbBookAutor.AutoSize = true;
            lbBookAutor.Location = new System.Drawing.Point(6, 70);
            lbBookAutor.Name = "lbBookAutor";
            lbBookAutor.Size = new System.Drawing.Size(67, 15);
            lbBookAutor.TabIndex = 11;
            lbBookAutor.Text = "Book Autor";
            // 
            // lbBookTile
            // 
            lbBookTile.AutoSize = true;
            lbBookTile.Location = new System.Drawing.Point(6, 20);
            lbBookTile.Name = "lbBookTile";
            lbBookTile.Size = new System.Drawing.Size(59, 15);
            lbBookTile.TabIndex = 10;
            lbBookTile.Text = "Book Title";
            // 
            // textBoxScriptResult
            // 
            textBoxScriptResult.Location = new System.Drawing.Point(205, 308);
            textBoxScriptResult.Multiline = true;
            textBoxScriptResult.Name = "textBoxScriptResult";
            textBoxScriptResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxScriptResult.Size = new System.Drawing.Size(514, 254);
            textBoxScriptResult.TabIndex = 9;
            // 
            // buttonCreateBookScript
            // 
            buttonCreateBookScript.Location = new System.Drawing.Point(205, 279);
            buttonCreateBookScript.Name = "buttonCreateBookScript";
            buttonCreateBookScript.Size = new System.Drawing.Size(86, 23);
            buttonCreateBookScript.TabIndex = 8;
            buttonCreateBookScript.Text = "Create script";
            buttonCreateBookScript.UseVisualStyleBackColor = true;
            buttonCreateBookScript.Click += buttonCreateBookScript_Click;
            // 
            // textBoxBookContent
            // 
            textBoxBookContent.Location = new System.Drawing.Point(205, 36);
            textBoxBookContent.Multiline = true;
            textBoxBookContent.Name = "textBoxBookContent";
            textBoxBookContent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textBoxBookContent.Size = new System.Drawing.Size(514, 237);
            textBoxBookContent.TabIndex = 7;
            // 
            // comboBoxEnableAutoformat
            // 
            comboBoxEnableAutoformat.FormattingEnabled = true;
            comboBoxEnableAutoformat.Location = new System.Drawing.Point(6, 324);
            comboBoxEnableAutoformat.Name = "comboBoxEnableAutoformat";
            comboBoxEnableAutoformat.Size = new System.Drawing.Size(149, 23);
            comboBoxEnableAutoformat.TabIndex = 6;
            // 
            // textBoxGrafikID
            // 
            textBoxGrafikID.Location = new System.Drawing.Point(6, 176);
            textBoxGrafikID.Name = "textBoxGrafikID";
            textBoxGrafikID.Size = new System.Drawing.Size(149, 23);
            textBoxGrafikID.TabIndex = 5;
            // 
            // comboBoxBookGrafic
            // 
            comboBoxBookGrafic.FormattingEnabled = true;
            comboBoxBookGrafic.Location = new System.Drawing.Point(6, 127);
            comboBoxBookGrafic.Name = "comboBoxBookGrafic";
            comboBoxBookGrafic.Size = new System.Drawing.Size(149, 23);
            comboBoxBookGrafic.TabIndex = 4;
            // 
            // textBoxCoverHue
            // 
            textBoxCoverHue.Location = new System.Drawing.Point(6, 280);
            textBoxCoverHue.Name = "textBoxCoverHue";
            textBoxCoverHue.Size = new System.Drawing.Size(149, 23);
            textBoxCoverHue.TabIndex = 3;
            textBoxCoverHue.TextChanged += textBoxCoverHue_TextChanged;
            // 
            // textBoxBookFilename
            // 
            textBoxBookFilename.Location = new System.Drawing.Point(6, 230);
            textBoxBookFilename.Name = "textBoxBookFilename";
            textBoxBookFilename.Size = new System.Drawing.Size(149, 23);
            textBoxBookFilename.TabIndex = 2;
            // 
            // textBoxBookAuthor
            // 
            textBoxBookAuthor.Location = new System.Drawing.Point(6, 88);
            textBoxBookAuthor.Name = "textBoxBookAuthor";
            textBoxBookAuthor.Size = new System.Drawing.Size(149, 23);
            textBoxBookAuthor.TabIndex = 1;
            textBoxBookAuthor.TextChanged += textBoxBookAuthor_TextChanged;
            // 
            // textBoxBookTitle
            // 
            textBoxBookTitle.Location = new System.Drawing.Point(6, 38);
            textBoxBookTitle.Name = "textBoxBookTitle";
            textBoxBookTitle.Size = new System.Drawing.Size(149, 23);
            textBoxBookTitle.TabIndex = 0;
            textBoxBookTitle.TextChanged += textBoxBookTitle_TextChanged;
            // 
            // tabPageAnimation
            // 
            tabPageAnimation.Location = new System.Drawing.Point(4, 24);
            tabPageAnimation.Name = "tabPageAnimation";
            tabPageAnimation.Padding = new System.Windows.Forms.Padding(3);
            tabPageAnimation.Size = new System.Drawing.Size(762, 578);
            tabPageAnimation.TabIndex = 1;
            tabPageAnimation.Text = "Animation";
            tabPageAnimation.UseVisualStyleBackColor = true;
            // 
            // tabPageItem
            // 
            tabPageItem.Location = new System.Drawing.Point(4, 24);
            tabPageItem.Name = "tabPageItem";
            tabPageItem.Size = new System.Drawing.Size(762, 578);
            tabPageItem.TabIndex = 2;
            tabPageItem.Text = "Items";
            tabPageItem.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(800, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // ScriptCreator
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 667);
            Controls.Add(toolStrip1);
            Controls.Add(panelBaseScript);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ScriptCreator";
            Text = "ScriptCreator";
            panelBaseScript.ResumeLayout(false);
            tabControlItems.ResumeLayout(false);
            tabPageBook.ResumeLayout(false);
            tabPageBook.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel panelBaseScript;
        private System.Windows.Forms.TabControl tabControlItems;
        private System.Windows.Forms.TabPage tabPageBook;
        private System.Windows.Forms.TabPage tabPageAnimation;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPageItem;
        private System.Windows.Forms.TextBox textBoxScriptResult;
        private System.Windows.Forms.Button buttonCreateBookScript;
        private System.Windows.Forms.TextBox textBoxBookContent;
        private System.Windows.Forms.ComboBox comboBoxEnableAutoformat;
        private System.Windows.Forms.TextBox textBoxGrafikID;
        private System.Windows.Forms.ComboBox comboBoxBookGrafic;
        private System.Windows.Forms.TextBox textBoxCoverHue;
        private System.Windows.Forms.TextBox textBoxBookFilename;
        private System.Windows.Forms.TextBox textBoxBookAuthor;
        private System.Windows.Forms.TextBox textBoxBookTitle;
        private System.Windows.Forms.Label lbBookTile;
        private System.Windows.Forms.Label lbFileName;
        private System.Windows.Forms.Label lbBookAutor;
        private System.Windows.Forms.Label lbIDAdressBook;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label lbColorHueBook;
        private System.Windows.Forms.Button btColorSet;
        private System.Windows.Forms.Button btClipBoard;
        private System.Windows.Forms.Button btSaveBookScript;
    }
}
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
    partial class EditorXML
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorXML));
            menuStripXMLEdit = new System.Windows.Forms.MenuStrip();
            dateiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            designToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addNewLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            keyDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            richTextBoxXmlContent = new System.Windows.Forms.RichTextBox();
            contextMenuStripXMLEdit = new System.Windows.Forms.ContextMenuStrip(components);
            findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBoxFind = new System.Windows.Forms.ToolStripTextBox();
            resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            searchAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            copyClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBoxEditXML = new System.Windows.Forms.GroupBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxOutputPath = new System.Windows.Forms.CheckBox();
            checkBoxAutoSave = new System.Windows.Forms.CheckBox();
            checkBoxXMLFormatted = new System.Windows.Forms.CheckBox();
            statusStripXMLEditor = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusWord = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelTextStatistics = new System.Windows.Forms.ToolStripStatusLabel();
            menuStripXMLEdit.SuspendLayout();
            contextMenuStripXMLEdit.SuspendLayout();
            groupBoxEditXML.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            statusStripXMLEditor.SuspendLayout();
            SuspendLayout();
            // 
            // menuStripXMLEdit
            // 
            menuStripXMLEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { dateiToolStripMenuItem, designToolStripMenuItem, addToolStripMenuItem, helpToolStripMenuItem });
            menuStripXMLEdit.Location = new System.Drawing.Point(0, 0);
            menuStripXMLEdit.Name = "menuStripXMLEdit";
            menuStripXMLEdit.Size = new System.Drawing.Size(849, 24);
            menuStripXMLEdit.TabIndex = 0;
            menuStripXMLEdit.Text = "menuStrip1";
            // 
            // dateiToolStripMenuItem
            // 
            dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, toolStripSeparator1, printToolStripMenuItem });
            dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
            dateiToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            dateiToolStripMenuItem.Text = "Datei";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            loadToolStripMenuItem.Text = "Load";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(97, 6);
            // 
            // printToolStripMenuItem
            // 
            printToolStripMenuItem.Name = "printToolStripMenuItem";
            printToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            printToolStripMenuItem.Text = "Print";
            printToolStripMenuItem.Click += printToolStripMenuItem_Click;
            // 
            // designToolStripMenuItem
            // 
            designToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { darkModeToolStripMenuItem });
            designToolStripMenuItem.Name = "designToolStripMenuItem";
            designToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            designToolStripMenuItem.Text = "Design";
            // 
            // darkModeToolStripMenuItem
            // 
            darkModeToolStripMenuItem.Name = "darkModeToolStripMenuItem";
            darkModeToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            darkModeToolStripMenuItem.Text = "Dark mode";
            darkModeToolStripMenuItem.Click += designToolStripMenuItem_Click;
            // 
            // addToolStripMenuItem
            // 
            addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { addNewLineToolStripMenuItem });
            addToolStripMenuItem.Name = "addToolStripMenuItem";
            addToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            addToolStripMenuItem.Text = "Add";
            // 
            // addNewLineToolStripMenuItem
            // 
            addNewLineToolStripMenuItem.Name = "addNewLineToolStripMenuItem";
            addNewLineToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            addNewLineToolStripMenuItem.Text = "Add new line";
            addNewLineToolStripMenuItem.Click += addNewLineToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { keyDownToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // keyDownToolStripMenuItem
            // 
            keyDownToolStripMenuItem.Name = "keyDownToolStripMenuItem";
            keyDownToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            keyDownToolStripMenuItem.Text = "KeyDown";
            keyDownToolStripMenuItem.Click += keyDownToolStripMenuItem_Click;
            // 
            // richTextBoxXmlContent
            // 
            richTextBoxXmlContent.ContextMenuStrip = contextMenuStripXMLEdit;
            richTextBoxXmlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxXmlContent.Location = new System.Drawing.Point(3, 19);
            richTextBoxXmlContent.Name = "richTextBoxXmlContent";
            richTextBoxXmlContent.Size = new System.Drawing.Size(694, 508);
            richTextBoxXmlContent.TabIndex = 1;
            richTextBoxXmlContent.Text = "";
            richTextBoxXmlContent.TextChanged += richTextBoxXmlContent_TextChanged;
            // 
            // contextMenuStripXMLEdit
            // 
            contextMenuStripXMLEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { findToolStripMenuItem, resetToolStripMenuItem, toolStripSeparator2, undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator4, searchAndReplaceToolStripMenuItem, toolStripSeparator3, copyClipboardToolStripMenuItem });
            contextMenuStripXMLEdit.Name = "contextMenuStripXMLEdit";
            contextMenuStripXMLEdit.Size = new System.Drawing.Size(177, 154);
            // 
            // findToolStripMenuItem
            // 
            findToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxFind });
            findToolStripMenuItem.Image = Properties.Resources.Mark;
            findToolStripMenuItem.Name = "findToolStripMenuItem";
            findToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            findToolStripMenuItem.Text = "Find";
            findToolStripMenuItem.Click += findToolStripMenuItem_Click;
            // 
            // toolStripTextBoxFind
            // 
            toolStripTextBoxFind.Name = "toolStripTextBoxFind";
            toolStripTextBoxFind.Size = new System.Drawing.Size(100, 23);
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Image = Properties.Resources.Remove;
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            resetToolStripMenuItem.Text = "Reset";
            resetToolStripMenuItem.Click += resetToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Image = Properties.Resources.left_arrow;
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            undoToolStripMenuItem.Text = "Undo";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Image = Properties.Resources.Export;
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            redoToolStripMenuItem.Text = "Redo";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(173, 6);
            // 
            // searchAndReplaceToolStripMenuItem
            // 
            searchAndReplaceToolStripMenuItem.Image = Properties.Resources.reload;
            searchAndReplaceToolStripMenuItem.Name = "searchAndReplaceToolStripMenuItem";
            searchAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            searchAndReplaceToolStripMenuItem.Text = "Search and Replace";
            searchAndReplaceToolStripMenuItem.Click += searchAndReplaceToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(173, 6);
            // 
            // copyClipboardToolStripMenuItem
            // 
            copyClipboardToolStripMenuItem.Image = Properties.Resources.Text;
            copyClipboardToolStripMenuItem.Name = "copyClipboardToolStripMenuItem";
            copyClipboardToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            copyClipboardToolStripMenuItem.Text = "Copy Clipboard";
            copyClipboardToolStripMenuItem.Click += copyClipboardToolStripMenuItem_Click;
            // 
            // groupBoxEditXML
            // 
            groupBoxEditXML.Controls.Add(richTextBoxXmlContent);
            groupBoxEditXML.Location = new System.Drawing.Point(12, 27);
            groupBoxEditXML.Name = "groupBoxEditXML";
            groupBoxEditXML.Size = new System.Drawing.Size(700, 530);
            groupBoxEditXML.TabIndex = 2;
            groupBoxEditXML.TabStop = false;
            groupBoxEditXML.Text = "Edit";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(checkBoxOutputPath);
            flowLayoutPanel1.Controls.Add(checkBoxAutoSave);
            flowLayoutPanel1.Controls.Add(checkBoxXMLFormatted);
            flowLayoutPanel1.Location = new System.Drawing.Point(718, 46);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(119, 498);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // checkBoxOutputPath
            // 
            checkBoxOutputPath.AutoSize = true;
            checkBoxOutputPath.Location = new System.Drawing.Point(3, 3);
            checkBoxOutputPath.Name = "checkBoxOutputPath";
            checkBoxOutputPath.Size = new System.Drawing.Size(91, 19);
            checkBoxOutputPath.TabIndex = 0;
            checkBoxOutputPath.Text = "Output Path";
            checkBoxOutputPath.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoSave
            // 
            checkBoxAutoSave.AutoSize = true;
            checkBoxAutoSave.Checked = true;
            checkBoxAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAutoSave.Location = new System.Drawing.Point(3, 28);
            checkBoxAutoSave.Name = "checkBoxAutoSave";
            checkBoxAutoSave.Size = new System.Drawing.Size(75, 19);
            checkBoxAutoSave.TabIndex = 1;
            checkBoxAutoSave.Text = "Autosave";
            checkBoxAutoSave.UseVisualStyleBackColor = true;
            checkBoxAutoSave.CheckedChanged += checkBoxAutoSave_CheckedChanged;
            // 
            // checkBoxXMLFormatted
            // 
            checkBoxXMLFormatted.AutoSize = true;
            checkBoxXMLFormatted.Location = new System.Drawing.Point(3, 53);
            checkBoxXMLFormatted.Name = "checkBoxXMLFormatted";
            checkBoxXMLFormatted.Size = new System.Drawing.Size(108, 19);
            checkBoxXMLFormatted.TabIndex = 2;
            checkBoxXMLFormatted.Text = "XML Formatted";
            checkBoxXMLFormatted.UseVisualStyleBackColor = true;
            // 
            // statusStripXMLEditor
            // 
            statusStripXMLEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabelInfo, toolStripStatusWord, toolStripStatusLabelTextStatistics });
            statusStripXMLEditor.Location = new System.Drawing.Point(0, 547);
            statusStripXMLEditor.Name = "statusStripXMLEditor";
            statusStripXMLEditor.Size = new System.Drawing.Size(849, 22);
            statusStripXMLEditor.TabIndex = 4;
            statusStripXMLEditor.Text = "statusStrip1";
            // 
            // toolStripStatusLabelInfo
            // 
            toolStripStatusLabelInfo.Name = "toolStripStatusLabelInfo";
            toolStripStatusLabelInfo.Size = new System.Drawing.Size(28, 17);
            toolStripStatusLabelInfo.Text = "Info";
            // 
            // toolStripStatusWord
            // 
            toolStripStatusWord.Name = "toolStripStatusWord";
            toolStripStatusWord.Size = new System.Drawing.Size(43, 17);
            toolStripStatusWord.Text = "Count:";
            // 
            // toolStripStatusLabelTextStatistics
            // 
            toolStripStatusLabelTextStatistics.Name = "toolStripStatusLabelTextStatistics";
            toolStripStatusLabelTextStatistics.Size = new System.Drawing.Size(76, 17);
            toolStripStatusLabelTextStatistics.Text = "Text statistics";
            // 
            // EditorXML
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(849, 569);
            Controls.Add(statusStripXMLEditor);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(groupBoxEditXML);
            Controls.Add(menuStripXMLEdit);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripXMLEdit;
            Name = "EditorXML";
            Text = "XML Editor";
            DragDrop += EditorXML_DragDrop;
            DragEnter += EditorXML_DragEnter;
            KeyDown += EditorXML_KeyDown;
            menuStripXMLEdit.ResumeLayout(false);
            menuStripXMLEdit.PerformLayout();
            contextMenuStripXMLEdit.ResumeLayout(false);
            groupBoxEditXML.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            statusStripXMLEditor.ResumeLayout(false);
            statusStripXMLEditor.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStripXMLEdit;
        private System.Windows.Forms.RichTextBox richTextBoxXmlContent;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripXMLEdit;
        private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBoxEditXML;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxOutputPath;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFind;
        private System.Windows.Forms.StatusStrip statusStripXMLEditor;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelInfo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusWord;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTextStatistics;
        private System.Windows.Forms.ToolStripMenuItem designToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBoxAutoSave;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.CheckBox checkBoxXMLFormatted;
        private System.Windows.Forms.ToolStripMenuItem copyClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyDownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchAndReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}
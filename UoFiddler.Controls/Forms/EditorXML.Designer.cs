// /***************************************************************************
//  *
//  * $Author: Turley
//  * Advanced Nikodemus
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class EditorXML
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            _undoDebounceTimer?.Dispose();
            _autoSaveTimer?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorXML));
            menuStripXMLEdit = new System.Windows.Forms.MenuStrip();
            dateiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            designToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            darkModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            xmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            validateXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            prettyPrintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            minifyXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addNewLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toggleBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nextBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            prevBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            clearBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            keyDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            lineNumberPanel = new System.Windows.Forms.Panel();
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
            goToLineToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            copyClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupBoxEditXML = new System.Windows.Forms.GroupBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            checkBoxOutputPath = new System.Windows.Forms.CheckBox();
            checkBoxAutoSave = new System.Windows.Forms.CheckBox();
            checkBoxXMLFormatted = new System.Windows.Forms.CheckBox();
            checkBoxSyntaxHighlight = new System.Windows.Forms.CheckBox();
            statusStripXMLEditor = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusWord = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabelLine = new System.Windows.Forms.ToolStripStatusLabel();
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
            menuStripXMLEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { dateiToolStripMenuItem, designToolStripMenuItem, xmlToolStripMenuItem, addToolStripMenuItem, bookmarkToolStripMenuItem, helpToolStripMenuItem });
            menuStripXMLEdit.Location = new System.Drawing.Point(0, 0);
            menuStripXMLEdit.Name = "menuStripXMLEdit";
            menuStripXMLEdit.Size = new System.Drawing.Size(930, 24);
            menuStripXMLEdit.TabIndex = 4;
            menuStripXMLEdit.Text = "menuStrip1";
            // 
            // dateiToolStripMenuItem
            // 
            dateiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator1, printToolStripMenuItem });
            dateiToolStripMenuItem.Name = "dateiToolStripMenuItem";
            dateiToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            dateiToolStripMenuItem.Text = "Datei";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            loadToolStripMenuItem.Text = "Load…";
            loadToolStripMenuItem.Click += loadToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            saveToolStripMenuItem.Text = "Save (Ctrl+S)";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            saveAsToolStripMenuItem.Text = "Save as…";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // printToolStripMenuItem
            // 
            printToolStripMenuItem.Name = "printToolStripMenuItem";
            printToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
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
            darkModeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            darkModeToolStripMenuItem.Text = "Dark Mode";
            darkModeToolStripMenuItem.Click += designToolStripMenuItem_Click;
            // 
            // xmlToolStripMenuItem
            // 
            xmlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { validateXmlToolStripMenuItem, prettyPrintToolStripMenuItem, minifyXmlToolStripMenuItem });
            xmlToolStripMenuItem.Name = "xmlToolStripMenuItem";
            xmlToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            xmlToolStripMenuItem.Text = "XML";
            // 
            // validateXmlToolStripMenuItem
            // 
            validateXmlToolStripMenuItem.Name = "validateXmlToolStripMenuItem";
            validateXmlToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            validateXmlToolStripMenuItem.Text = "Validieren";
            validateXmlToolStripMenuItem.Click += validateXmlToolStripMenuItem_Click;
            // 
            // prettyPrintToolStripMenuItem
            // 
            prettyPrintToolStripMenuItem.Name = "prettyPrintToolStripMenuItem";
            prettyPrintToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            prettyPrintToolStripMenuItem.Text = "Pretty Print (formatieren)";
            prettyPrintToolStripMenuItem.Click += prettyPrintToolStripMenuItem_Click;
            // 
            // minifyXmlToolStripMenuItem
            // 
            minifyXmlToolStripMenuItem.Name = "minifyXmlToolStripMenuItem";
            minifyXmlToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            minifyXmlToolStripMenuItem.Text = "Minify (komprimieren)";
            minifyXmlToolStripMenuItem.Click += minifyXmlToolStripMenuItem_Click;
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
            addNewLineToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            addNewLineToolStripMenuItem.Text = "New mob / equipment…";
            addNewLineToolStripMenuItem.Click += addNewLineToolStripMenuItem_Click;
            // 
            // bookmarkToolStripMenuItem
            // 
            bookmarkToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toggleBookmarkToolStripMenuItem, nextBookmarkToolStripMenuItem, prevBookmarkToolStripMenuItem, clearBookmarksToolStripMenuItem });
            bookmarkToolStripMenuItem.Name = "bookmarkToolStripMenuItem";
            bookmarkToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            bookmarkToolStripMenuItem.Text = "Bookmark";
            // 
            // toggleBookmarkToolStripMenuItem
            // 
            toggleBookmarkToolStripMenuItem.Name = "toggleBookmarkToolStripMenuItem";
            toggleBookmarkToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            toggleBookmarkToolStripMenuItem.Text = "Set / Remove (Ctrl+B)";
            toggleBookmarkToolStripMenuItem.Click += toggleBookmarkToolStripMenuItem_Click;
            // 
            // nextBookmarkToolStripMenuItem
            // 
            nextBookmarkToolStripMenuItem.Name = "nextBookmarkToolStripMenuItem";
            nextBookmarkToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            nextBookmarkToolStripMenuItem.Text = "Next  (F2)";
            nextBookmarkToolStripMenuItem.Click += nextBookmarkToolStripMenuItem_Click;
            // 
            // prevBookmarkToolStripMenuItem
            // 
            prevBookmarkToolStripMenuItem.Name = "prevBookmarkToolStripMenuItem";
            prevBookmarkToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            prevBookmarkToolStripMenuItem.Text = "Previous";
            prevBookmarkToolStripMenuItem.Click += prevBookmarkToolStripMenuItem_Click;
            // 
            // clearBookmarksToolStripMenuItem
            // 
            clearBookmarksToolStripMenuItem.Name = "clearBookmarksToolStripMenuItem";
            clearBookmarksToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            clearBookmarksToolStripMenuItem.Text = "Delete all";
            clearBookmarksToolStripMenuItem.Click += clearBookmarksToolStripMenuItem_Click;
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
            keyDownToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            keyDownToolStripMenuItem.Text = "Tastatur-Shortcuts";
            keyDownToolStripMenuItem.Click += keyDownToolStripMenuItem_Click;
            // 
            // lineNumberPanel
            // 
            lineNumberPanel.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            lineNumberPanel.Dock = System.Windows.Forms.DockStyle.Left;
            lineNumberPanel.Location = new System.Drawing.Point(3, 19);
            lineNumberPanel.Name = "lineNumberPanel";
            lineNumberPanel.Size = new System.Drawing.Size(42, 518);
            lineNumberPanel.TabIndex = 1;
            lineNumberPanel.Paint += lineNumberPanel_Paint;
            lineNumberPanel.MouseClick += lineNumberPanel_MouseClick;
            // 
            // richTextBoxXmlContent
            // 
            richTextBoxXmlContent.ContextMenuStrip = contextMenuStripXMLEdit;
            richTextBoxXmlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxXmlContent.Font = new System.Drawing.Font("Consolas", 10F);
            richTextBoxXmlContent.Location = new System.Drawing.Point(45, 19);
            richTextBoxXmlContent.Name = "richTextBoxXmlContent";
            richTextBoxXmlContent.Size = new System.Drawing.Size(722, 518);
            richTextBoxXmlContent.TabIndex = 0;
            richTextBoxXmlContent.Text = "";
            richTextBoxXmlContent.WordWrap = false;
            richTextBoxXmlContent.TextChanged += richTextBoxXmlContent_TextChanged;
            // 
            // contextMenuStripXMLEdit
            // 
            contextMenuStripXMLEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { findToolStripMenuItem, resetToolStripMenuItem, toolStripSeparator2, undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator4, searchAndReplaceToolStripMenuItem, goToLineToolStripMenuItem2, toolStripSeparator3, copyClipboardToolStripMenuItem });
            contextMenuStripXMLEdit.Name = "contextMenuStripXMLEdit";
            contextMenuStripXMLEdit.Size = new System.Drawing.Size(222, 176);
            // 
            // findToolStripMenuItem
            // 
            findToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripTextBoxFind });
            findToolStripMenuItem.Image = Properties.Resources.Mark;
            findToolStripMenuItem.Name = "findToolStripMenuItem";
            findToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            findToolStripMenuItem.Text = "Suchen  (F3)";
            findToolStripMenuItem.Click += findToolStripMenuItem_Click;
            // 
            // toolStripTextBoxFind
            // 
            toolStripTextBoxFind.Name = "toolStripTextBoxFind";
            toolStripTextBoxFind.Size = new System.Drawing.Size(120, 23);
            toolStripTextBoxFind.TextChanged += toolStripTextBoxFind_TextChanged;
            // 
            // resetToolStripMenuItem
            // 
            resetToolStripMenuItem.Image = Properties.Resources.Remove;
            resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            resetToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            resetToolStripMenuItem.Text = "Suche zurücksetzen  (F5)";
            resetToolStripMenuItem.Click += resetToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(218, 6);
            // 
            // undoToolStripMenuItem
            // 
            undoToolStripMenuItem.Image = Properties.Resources.left_arrow;
            undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            undoToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            undoToolStripMenuItem.Text = "Rückgängig  (Ctrl+Z)";
            undoToolStripMenuItem.Click += undoToolStripMenuItem_Click;
            // 
            // redoToolStripMenuItem
            // 
            redoToolStripMenuItem.Image = Properties.Resources.Export;
            redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            redoToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            redoToolStripMenuItem.Text = "Wiederherstellen  (Ctrl+Y)";
            redoToolStripMenuItem.Click += redoToolStripMenuItem_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(218, 6);
            // 
            // searchAndReplaceToolStripMenuItem
            // 
            searchAndReplaceToolStripMenuItem.Image = Properties.Resources.reload;
            searchAndReplaceToolStripMenuItem.Name = "searchAndReplaceToolStripMenuItem";
            searchAndReplaceToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            searchAndReplaceToolStripMenuItem.Text = "Suchen & Ersetzen";
            searchAndReplaceToolStripMenuItem.Click += searchAndReplaceToolStripMenuItem_Click;
            // 
            // goToLineToolStripMenuItem2
            // 
            goToLineToolStripMenuItem2.Name = "goToLineToolStripMenuItem2";
            goToLineToolStripMenuItem2.Size = new System.Drawing.Size(221, 22);
            goToLineToolStripMenuItem2.Text = "Gehe zu Zeile…  (Ctrl+G)";
            goToLineToolStripMenuItem2.Click += goToLineToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(218, 6);
            // 
            // copyClipboardToolStripMenuItem
            // 
            copyClipboardToolStripMenuItem.Image = Properties.Resources.Text;
            copyClipboardToolStripMenuItem.Name = "copyClipboardToolStripMenuItem";
            copyClipboardToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            copyClipboardToolStripMenuItem.Text = "In Zwischenablage kopieren";
            copyClipboardToolStripMenuItem.Click += copyClipboardToolStripMenuItem_Click;
            // 
            // groupBoxEditXML
            // 
            groupBoxEditXML.Controls.Add(richTextBoxXmlContent);
            groupBoxEditXML.Controls.Add(lineNumberPanel);
            groupBoxEditXML.Location = new System.Drawing.Point(12, 27);
            groupBoxEditXML.Name = "groupBoxEditXML";
            groupBoxEditXML.Size = new System.Drawing.Size(770, 540);
            groupBoxEditXML.TabIndex = 2;
            groupBoxEditXML.TabStop = false;
            groupBoxEditXML.Text = "Editor";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(checkBoxOutputPath);
            flowLayoutPanel1.Controls.Add(checkBoxAutoSave);
            flowLayoutPanel1.Controls.Add(checkBoxXMLFormatted);
            flowLayoutPanel1.Controls.Add(checkBoxSyntaxHighlight);
            flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flowLayoutPanel1.Location = new System.Drawing.Point(790, 46);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(130, 498);
            flowLayoutPanel1.TabIndex = 3;
            // 
            // checkBoxOutputPath
            // 
            checkBoxOutputPath.AutoSize = true;
            checkBoxOutputPath.Location = new System.Drawing.Point(3, 3);
            checkBoxOutputPath.Name = "checkBoxOutputPath";
            checkBoxOutputPath.Size = new System.Drawing.Size(91, 19);
            checkBoxOutputPath.TabIndex = 0;
            checkBoxOutputPath.Text = "Output path";
            // 
            // checkBoxAutoSave
            // 
            checkBoxAutoSave.AutoSize = true;
            checkBoxAutoSave.Checked = true;
            checkBoxAutoSave.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAutoSave.Location = new System.Drawing.Point(3, 28);
            checkBoxAutoSave.Name = "checkBoxAutoSave";
            checkBoxAutoSave.Size = new System.Drawing.Size(81, 19);
            checkBoxAutoSave.TabIndex = 1;
            checkBoxAutoSave.Text = "Auto-Save";
            checkBoxAutoSave.CheckedChanged += checkBoxAutoSave_CheckedChanged;
            // 
            // checkBoxXMLFormatted
            // 
            checkBoxXMLFormatted.AutoSize = true;
            checkBoxXMLFormatted.Location = new System.Drawing.Point(3, 53);
            checkBoxXMLFormatted.Name = "checkBoxXMLFormatted";
            checkBoxXMLFormatted.Size = new System.Drawing.Size(79, 19);
            checkBoxXMLFormatted.TabIndex = 2;
            checkBoxXMLFormatted.Text = "RTF colors";
            // 
            // checkBoxSyntaxHighlight
            // 
            checkBoxSyntaxHighlight.AutoSize = true;
            checkBoxSyntaxHighlight.Checked = true;
            checkBoxSyntaxHighlight.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxSyntaxHighlight.Location = new System.Drawing.Point(3, 78);
            checkBoxSyntaxHighlight.Name = "checkBoxSyntaxHighlight";
            checkBoxSyntaxHighlight.Size = new System.Drawing.Size(116, 19);
            checkBoxSyntaxHighlight.TabIndex = 3;
            checkBoxSyntaxHighlight.Text = "Syntax-Highlight";
            checkBoxSyntaxHighlight.CheckedChanged += checkBoxSyntaxHighlight_CheckedChanged;
            // 
            // statusStripXMLEditor
            // 
            statusStripXMLEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabelInfo, toolStripStatusWord, toolStripStatusLabelLine, toolStripStatusLabelTextStatistics });
            statusStripXMLEditor.Location = new System.Drawing.Point(0, 573);
            statusStripXMLEditor.Name = "statusStripXMLEditor";
            statusStripXMLEditor.Size = new System.Drawing.Size(930, 24);
            statusStripXMLEditor.TabIndex = 1;
            // 
            // toolStripStatusLabelInfo
            // 
            toolStripStatusLabelInfo.Name = "toolStripStatusLabelInfo";
            toolStripStatusLabelInfo.Size = new System.Drawing.Size(629, 19);
            toolStripStatusLabelInfo.Spring = true;
            toolStripStatusLabelInfo.Text = "Ready.";
            toolStripStatusLabelInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusWord
            // 
            toolStripStatusWord.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            toolStripStatusWord.Name = "toolStripStatusWord";
            toolStripStatusWord.Size = new System.Drawing.Size(4, 19);
            // 
            // toolStripStatusLabelLine
            // 
            toolStripStatusLabelLine.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            toolStripStatusLabelLine.Name = "toolStripStatusLabelLine";
            toolStripStatusLabelLine.Size = new System.Drawing.Size(48, 19);
            toolStripStatusLabelLine.Text = "Zeile: 1";
            // 
            // toolStripStatusLabelTextStatistics
            // 
            toolStripStatusLabelTextStatistics.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            toolStripStatusLabelTextStatistics.Name = "toolStripStatusLabelTextStatistics";
            toolStripStatusLabelTextStatistics.Size = new System.Drawing.Size(234, 19);
            toolStripStatusLabelTextStatistics.Text = "Words: 0 Characters: 0 Lines: 0 XML tags: 0";
            // 
            // EditorXML
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(930, 597);
            Controls.Add(statusStripXMLEditor);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(groupBoxEditXML);
            Controls.Add(menuStripXMLEdit);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStripXMLEdit;
            Name = "EditorXML";
            Text = "XML Editor – Ultima Online";
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

        // ── Felddeklarationen ────────────────────────────────────────────────
        private System.Windows.Forms.MenuStrip menuStripXMLEdit;
        private System.Windows.Forms.ToolStripMenuItem dateiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem designToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem darkModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validateXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem prettyPrintToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minifyXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toggleBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem prevBookmarkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearBookmarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyDownToolStripMenuItem;

        private System.Windows.Forms.Panel lineNumberPanel;
        private System.Windows.Forms.RichTextBox richTextBoxXmlContent;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripXMLEdit;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxFind;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem searchAndReplaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem goToLineToolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copyClipboardToolStripMenuItem;

        private System.Windows.Forms.GroupBox groupBoxEditXML;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxOutputPath;
        private System.Windows.Forms.CheckBox checkBoxAutoSave;
        private System.Windows.Forms.CheckBox checkBoxXMLFormatted;
        private System.Windows.Forms.CheckBox checkBoxSyntaxHighlight;

        private System.Windows.Forms.StatusStrip statusStripXMLEditor;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelInfo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusWord;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelLine;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelTextStatistics;
    }
}
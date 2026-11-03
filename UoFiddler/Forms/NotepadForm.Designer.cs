/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/


namespace UoFiddler.Forms
{
    partial class NotepadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotepadForm));
            menuStrip = new System.Windows.Forms.MenuStrip();
            toolStripFormat = new System.Windows.Forms.ToolStrip();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblPosition = new System.Windows.Forms.ToolStripStatusLabel();
            lblStats = new System.Windows.Forms.ToolStripStatusLabel();
            lblZoom = new System.Windows.Forms.ToolStripStatusLabel();
            lblSaved = new System.Windows.Forms.ToolStripStatusLabel();
            lblNoteCount = new System.Windows.Forms.ToolStripStatusLabel();
            splitMain = new System.Windows.Forms.SplitContainer();
            tableList = new System.Windows.Forms.TableLayoutPanel();
            textSearch = new System.Windows.Forms.TextBox();
            tableFilters = new System.Windows.Forms.TableLayoutPanel();
            comboCategory = new System.Windows.Forms.ComboBox();
            comboSort = new System.Windows.Forms.ComboBox();
            listNotes = new System.Windows.Forms.ListBox();
            flowListButtons = new System.Windows.Forms.FlowLayoutPanel();
            btnNewNote = new System.Windows.Forms.Button();
            btnTemplate = new System.Windows.Forms.Button();
            btnDeleteNote = new System.Windows.Forms.Button();
            btnSticky = new System.Windows.Forms.Button();
            tabNotes = new System.Windows.Forms.TabControl();
            panelFind = new System.Windows.Forms.FlowLayoutPanel();
            lblFind = new System.Windows.Forms.Label();
            txtFind = new System.Windows.Forms.TextBox();
            btnFindPrev = new System.Windows.Forms.Button();
            btnFindNext = new System.Windows.Forms.Button();
            chkMatchCase = new System.Windows.Forms.CheckBox();
            chkWholeWord = new System.Windows.Forms.CheckBox();
            lblFindCount = new System.Windows.Forms.Label();
            lblReplace = new System.Windows.Forms.Label();
            txtReplace = new System.Windows.Forms.TextBox();
            btnReplace = new System.Windows.Forms.Button();
            btnReplaceAll = new System.Windows.Forms.Button();
            btnFindClose = new System.Windows.Forms.Button();
            tableProps = new System.Windows.Forms.TableLayoutPanel();
            txtTitle = new System.Windows.Forms.TextBox();
            flowProps = new System.Windows.Forms.FlowLayoutPanel();
            lblCategory = new System.Windows.Forms.Label();
            comboNoteCategory = new System.Windows.Forms.ComboBox();
            lblTags = new System.Windows.Forms.Label();
            txtTags = new System.Windows.Forms.TextBox();
            btnNoteColor = new System.Windows.Forms.Button();
            chkPinned = new System.Windows.Forms.CheckBox();
            chkCode = new System.Windows.Forms.CheckBox();
            lblReminder = new System.Windows.Forms.Label();
            dtReminder = new System.Windows.Forms.DateTimePicker();
            timerAutosave = new System.Windows.Forms.Timer(components);
            timerSearch = new System.Windows.Forms.Timer(components);
            toolTip = new System.Windows.Forms.ToolTip(components);
            statusStrip.SuspendLayout();
            tableList.SuspendLayout();
            tableFilters.SuspendLayout();
            flowListButtons.SuspendLayout();
            panelFind.SuspendLayout();
            tableProps.SuspendLayout();
            flowProps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Name = "menuStrip";
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.TabIndex = 0;
            // 
            // toolStripFormat
            // 
            toolStripFormat.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStripFormat.Name = "toolStripFormat";
            toolStripFormat.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            toolStripFormat.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStripFormat.TabIndex = 1;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblPosition, lblStats, lblZoom, lblSaved, lblNoteCount });
            statusStrip.Name = "statusStrip";
            statusStrip.TabIndex = 3;
            // 
            // lblPosition
            // 
            lblPosition.Name = "lblPosition";
            lblPosition.Text = "Ln 1, Col 1";
            lblPosition.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            // 
            // lblStats
            // 
            lblStats.Name = "lblStats";
            lblStats.Text = "0 words";
            lblStats.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            // 
            // lblZoom
            // 
            lblZoom.Name = "lblZoom";
            lblZoom.Text = "100 %";
            lblZoom.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            // 
            // lblSaved
            // 
            lblSaved.Name = "lblSaved";
            lblSaved.Text = "";
            lblSaved.Spring = true;
            lblSaved.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblNoteCount
            // 
            lblNoteCount.Name = "lblNoteCount";
            lblNoteCount.Text = "";
            lblNoteCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            lblZoom.ToolTipText = "Click = 100 %, Ctrl + mouse wheel = zoom";
            lblZoom.Click += LblZoom_Click;
            // 
            // splitMain
            // 
            splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitMain.Name = "splitMain";
            splitMain.Panel1.Controls.Add(tableList);
            splitMain.Panel1MinSize = 200;
            splitMain.Panel2.Controls.Add(tabNotes);
            splitMain.Panel2.Controls.Add(panelFind);
            splitMain.Panel2.Controls.Add(tableProps);
            splitMain.Panel2MinSize = 300;
            splitMain.Size = new System.Drawing.Size(1100, 640);
            splitMain.SplitterDistance = 290;
            splitMain.SplitterWidth = 5;
            splitMain.TabIndex = 2;
            // 
            // tableList
            // 
            tableList.BackColor = System.Drawing.Color.FromArgb(248, 249, 251);
            tableList.ColumnCount = 1;
            tableList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableList.Controls.Add(textSearch, 0, 0);
            tableList.Controls.Add(tableFilters, 0, 1);
            tableList.Controls.Add(listNotes, 0, 2);
            tableList.Controls.Add(flowListButtons, 0, 3);
            tableList.Dock = System.Windows.Forms.DockStyle.Fill;
            tableList.Name = "tableList";
            tableList.Padding = new System.Windows.Forms.Padding(6);
            tableList.RowCount = 4;
            tableList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableList.TabIndex = 0;
            // 
            // textSearch
            // 
            textSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            textSearch.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            textSearch.Name = "textSearch";
            textSearch.PlaceholderText = "Search all notes …  (Ctrl+Shift+F)";
            textSearch.TabIndex = 0;
            textSearch.TextChanged += TextSearch_TextChanged;
            textSearch.KeyDown += TextSearch_KeyDown;
            // 
            // tableFilters
            // 
            tableFilters.AutoSize = true;
            tableFilters.ColumnCount = 2;
            tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            tableFilters.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            tableFilters.Controls.Add(comboCategory, 0, 0);
            tableFilters.Controls.Add(comboSort, 1, 0);
            tableFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            tableFilters.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            tableFilters.Name = "tableFilters";
            tableFilters.RowCount = 1;
            tableFilters.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableFilters.TabIndex = 1;
            // 
            // comboCategory
            // 
            comboCategory.Dock = System.Windows.Forms.DockStyle.Fill;
            comboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboCategory.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            comboCategory.Name = "comboCategory";
            toolTip.SetToolTip(comboCategory, "Filter by category");
            comboCategory.SelectedIndexChanged += ComboCategory_SelectedIndexChanged;
            // 
            // comboSort
            // 
            comboSort.Dock = System.Windows.Forms.DockStyle.Fill;
            comboSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboSort.Margin = new System.Windows.Forms.Padding(0);
            comboSort.Name = "comboSort";
            toolTip.SetToolTip(comboSort, "Sort order (pinned notes always first)");
            comboSort.SelectedIndexChanged += ComboSort_SelectedIndexChanged;
            comboSort.Items.AddRange(new object[] { "Modified", "Created", "Title", "Category" });
            // 
            // listNotes
            // 
            listNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            listNotes.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listNotes.IntegralHeight = false;
            listNotes.ItemHeight = 60;
            listNotes.Margin = new System.Windows.Forms.Padding(0);
            listNotes.Name = "listNotes";
            listNotes.TabIndex = 2;
            listNotes.DrawItem += ListNotes_DrawItem;
            listNotes.SelectedIndexChanged += ListNotes_SelectedIndexChanged;
            listNotes.KeyDown += ListNotes_KeyDown;
            listNotes.MouseDown += ListNotes_MouseDown;
            listNotes.DoubleClick += ListNotes_DoubleClick;
            // 
            // flowListButtons
            // 
            flowListButtons.AutoSize = true;
            flowListButtons.Controls.Add(btnNewNote);
            flowListButtons.Controls.Add(btnTemplate);
            flowListButtons.Controls.Add(btnDeleteNote);
            flowListButtons.Controls.Add(btnSticky);
            flowListButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            flowListButtons.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            flowListButtons.Name = "flowListButtons";
            flowListButtons.TabIndex = 3;
            // 
            // btnNewNote
            // 
            btnNewNote.AutoSize = true;
            btnNewNote.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            btnNewNote.Name = "btnNewNote";
            btnNewNote.TabIndex = 0;
            btnNewNote.Text = "+ New";
            toolTip.SetToolTip(btnNewNote, "New note (Ctrl+N)");
            btnNewNote.UseVisualStyleBackColor = true;
            btnNewNote.Click += BtnNewNote_Click;
            // 
            // btnTemplate
            // 
            btnTemplate.AutoSize = true;
            btnTemplate.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            btnTemplate.Name = "btnTemplate";
            btnTemplate.TabIndex = 1;
            btnTemplate.Text = "Template ▾";
            toolTip.SetToolTip(btnTemplate, "New note from a template");
            btnTemplate.UseVisualStyleBackColor = true;
            btnTemplate.Click += BtnTemplate_Click;
            // 
            // btnDeleteNote
            // 
            btnDeleteNote.AutoSize = true;
            btnDeleteNote.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            btnDeleteNote.Name = "btnDeleteNote";
            btnDeleteNote.TabIndex = 2;
            btnDeleteNote.Text = "Delete";
            toolTip.SetToolTip(btnDeleteNote, "Delete the selected note");
            btnDeleteNote.UseVisualStyleBackColor = true;
            btnDeleteNote.Click += BtnDeleteNote_Click;
            // 
            // btnSticky
            // 
            btnSticky.AutoSize = true;
            btnSticky.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            btnSticky.Name = "btnSticky";
            btnSticky.TabIndex = 3;
            btnSticky.Text = "Sticky";
            toolTip.SetToolTip(btnSticky, "Show the note as a small always-on-top sticky note");
            btnSticky.UseVisualStyleBackColor = true;
            btnSticky.Click += BtnSticky_Click;
            btnNewNote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            // 
            // tabNotes
            // 
            tabNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            tabNotes.Name = "tabNotes";
            tabNotes.ShowToolTips = true;
            tabNotes.TabIndex = 2;
            tabNotes.SelectedIndexChanged += TabNotes_SelectedIndexChanged;
            tabNotes.MouseUp += TabNotes_MouseUp;
            // 
            // panelFind
            // 
            panelFind.AutoSize = true;
            panelFind.BackColor = System.Drawing.Color.FromArgb(255, 250, 225);
            panelFind.Controls.Add(lblFind);
            panelFind.Controls.Add(txtFind);
            panelFind.Controls.Add(btnFindPrev);
            panelFind.Controls.Add(btnFindNext);
            panelFind.Controls.Add(chkMatchCase);
            panelFind.Controls.Add(chkWholeWord);
            panelFind.Controls.Add(lblFindCount);
            panelFind.Controls.Add(lblReplace);
            panelFind.Controls.Add(txtReplace);
            panelFind.Controls.Add(btnReplace);
            panelFind.Controls.Add(btnReplaceAll);
            panelFind.Controls.Add(btnFindClose);
            panelFind.Dock = System.Windows.Forms.DockStyle.Top;
            panelFind.Name = "panelFind";
            panelFind.Padding = new System.Windows.Forms.Padding(4, 3, 4, 2);
            panelFind.TabIndex = 1;
            panelFind.Visible = false;
            // 
            // lblFind
            // 
            lblFind.AutoSize = true;
            lblFind.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblFind.Name = "lblFind";
            lblFind.Text = "Find:";
            // 
            // txtFind
            // 
            txtFind.Name = "txtFind";
            txtFind.PlaceholderText = "Text to find";
            txtFind.Size = new System.Drawing.Size(180, 23);
            txtFind.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            txtFind.TextChanged += FindOptions_Changed;
            txtFind.KeyDown += TxtFind_KeyDown;
            // 
            // btnFindPrev
            // 
            btnFindPrev.Name = "btnFindPrev";
            btnFindPrev.Size = new System.Drawing.Size(30, 26);
            btnFindPrev.Text = "▲";
            btnFindPrev.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            btnFindPrev.UseVisualStyleBackColor = true;
            btnFindPrev.Click += BtnFindPrev_Click;
            toolTip.SetToolTip(btnFindPrev, "Previous (Shift+F3)");
            // 
            // btnFindNext
            // 
            btnFindNext.Name = "btnFindNext";
            btnFindNext.Size = new System.Drawing.Size(30, 26);
            btnFindNext.Text = "▼";
            btnFindNext.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            btnFindNext.UseVisualStyleBackColor = true;
            btnFindNext.Click += BtnFindNext_Click;
            toolTip.SetToolTip(btnFindNext, "Next (F3 / Enter)");
            // 
            // chkMatchCase
            // 
            chkMatchCase.AutoSize = true;
            chkMatchCase.Margin = new System.Windows.Forms.Padding(6, 6, 2, 0);
            chkMatchCase.Name = "chkMatchCase";
            chkMatchCase.Text = "Aa";
            toolTip.SetToolTip(chkMatchCase, "Match case");
            chkMatchCase.CheckedChanged += FindOptions_Changed;
            // 
            // chkWholeWord
            // 
            chkWholeWord.AutoSize = true;
            chkWholeWord.Margin = new System.Windows.Forms.Padding(6, 6, 2, 0);
            chkWholeWord.Name = "chkWholeWord";
            chkWholeWord.Text = "Whole word";
            toolTip.SetToolTip(chkWholeWord, "Whole words only");
            chkWholeWord.CheckedChanged += FindOptions_Changed;
            // 
            // lblFindCount
            // 
            lblFindCount.AutoSize = true;
            lblFindCount.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblFindCount.Name = "lblFindCount";
            lblFindCount.Text = "";
            lblFindCount.ForeColor = System.Drawing.Color.DimGray;
            lblFindCount.MinimumSize = new System.Drawing.Size(70, 0);
            // 
            // lblReplace
            // 
            lblReplace.AutoSize = true;
            lblReplace.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblReplace.Name = "lblReplace";
            lblReplace.Text = "Replace:";
            // 
            // txtReplace
            // 
            txtReplace.Name = "txtReplace";
            txtReplace.PlaceholderText = "Replace with";
            txtReplace.Size = new System.Drawing.Size(160, 23);
            txtReplace.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            txtReplace.KeyDown += TxtFind_KeyDown;
            // 
            // btnReplace
            // 
            btnReplace.Name = "btnReplace";
            btnReplace.Size = new System.Drawing.Size(70, 26);
            btnReplace.Text = "Replace";
            btnReplace.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            btnReplace.UseVisualStyleBackColor = true;
            btnReplace.Click += BtnReplace_Click;
            toolTip.SetToolTip(btnReplace, "Replace the current match");
            // 
            // btnReplaceAll
            // 
            btnReplaceAll.Name = "btnReplaceAll";
            btnReplaceAll.Size = new System.Drawing.Size(50, 26);
            btnReplaceAll.Text = "All";
            btnReplaceAll.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            btnReplaceAll.UseVisualStyleBackColor = true;
            btnReplaceAll.Click += BtnReplaceAll_Click;
            toolTip.SetToolTip(btnReplaceAll, "Replace all matches");
            // 
            // btnFindClose
            // 
            btnFindClose.Name = "btnFindClose";
            btnFindClose.Size = new System.Drawing.Size(30, 26);
            btnFindClose.Text = "✕";
            btnFindClose.Margin = new System.Windows.Forms.Padding(2, 2, 2, 0);
            btnFindClose.UseVisualStyleBackColor = true;
            btnFindClose.Click += BtnFindClose_Click;
            toolTip.SetToolTip(btnFindClose, "Close (Esc)");
            // 
            // tableProps
            // 
            tableProps.AutoSize = true;
            tableProps.BackColor = System.Drawing.SystemColors.Window;
            tableProps.ColumnCount = 1;
            tableProps.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableProps.Controls.Add(txtTitle, 0, 0);
            tableProps.Controls.Add(flowProps, 0, 1);
            tableProps.Dock = System.Windows.Forms.DockStyle.Top;
            tableProps.Name = "tableProps";
            tableProps.Padding = new System.Windows.Forms.Padding(8, 6, 8, 2);
            tableProps.RowCount = 2;
            tableProps.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableProps.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            tableProps.TabIndex = 0;
            // 
            // txtTitle
            // 
            txtTitle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            txtTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            txtTitle.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121);
            txtTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            txtTitle.Name = "txtTitle";
            txtTitle.PlaceholderText = "Title";
            txtTitle.TabIndex = 0;
            txtTitle.TextChanged += NoteProps_Changed;
            // 
            // flowProps
            // 
            flowProps.AutoSize = true;
            flowProps.Controls.Add(lblCategory);
            flowProps.Controls.Add(comboNoteCategory);
            flowProps.Controls.Add(lblTags);
            flowProps.Controls.Add(txtTags);
            flowProps.Controls.Add(btnNoteColor);
            flowProps.Controls.Add(chkPinned);
            flowProps.Controls.Add(chkCode);
            flowProps.Controls.Add(lblReminder);
            flowProps.Controls.Add(dtReminder);
            flowProps.Dock = System.Windows.Forms.DockStyle.Fill;
            flowProps.Margin = new System.Windows.Forms.Padding(0);
            flowProps.Name = "flowProps";
            flowProps.TabIndex = 1;
            // 
            // lblCategory
            // 
            lblCategory.AutoSize = true;
            lblCategory.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblCategory.Name = "lblCategory";
            lblCategory.Text = "Category:";
            // 
            // comboNoteCategory
            // 
            comboNoteCategory.Name = "comboNoteCategory";
            comboNoteCategory.Size = new System.Drawing.Size(120, 23);
            comboNoteCategory.Margin = new System.Windows.Forms.Padding(2, 3, 8, 0);
            comboNoteCategory.TextChanged += NoteProps_Changed;
            // 
            // lblTags
            // 
            lblTags.AutoSize = true;
            lblTags.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblTags.Name = "lblTags";
            lblTags.Text = "Tags:";
            // 
            // txtTags
            // 
            txtTags.Name = "txtTags";
            txtTags.PlaceholderText = "comma separated";
            txtTags.Size = new System.Drawing.Size(170, 23);
            txtTags.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            txtTags.Margin = new System.Windows.Forms.Padding(2, 3, 8, 0);
            txtTags.TextChanged += NoteProps_Changed;
            // 
            // btnNoteColor
            // 
            btnNoteColor.Name = "btnNoteColor";
            btnNoteColor.Size = new System.Drawing.Size(64, 26);
            btnNoteColor.Text = "Color";
            btnNoteColor.Margin = new System.Windows.Forms.Padding(2, 2, 6, 0);
            btnNoteColor.UseVisualStyleBackColor = true;
            btnNoteColor.Click += BtnNoteColor_Click;
            toolTip.SetToolTip(btnNoteColor, "Color mark of the note in the list");
            // 
            // chkPinned
            // 
            chkPinned.AutoSize = true;
            chkPinned.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            chkPinned.Name = "chkPinned";
            chkPinned.Text = "📌 Pinned";
            toolTip.SetToolTip(chkPinned, "Pinned notes stay at the top of the list");
            chkPinned.CheckedChanged += NoteProps_Changed;
            // 
            // chkCode
            // 
            chkCode.AutoSize = true;
            chkCode.Margin = new System.Windows.Forms.Padding(4, 6, 4, 0);
            chkCode.Name = "chkCode";
            chkCode.Text = "Code";
            toolTip.SetToolTip(chkCode, "Code note: monospace font and C# coloring for scripts");
            chkCode.CheckedChanged += NoteProps_Changed;
            // 
            // lblReminder
            // 
            lblReminder.AutoSize = true;
            lblReminder.Margin = new System.Windows.Forms.Padding(4, 7, 2, 0);
            lblReminder.Name = "lblReminder";
            lblReminder.Text = "⏰ Reminder:";
            // 
            // dtReminder
            // 
            dtReminder.CustomFormat = "dd.MM.yyyy  HH:mm";
            dtReminder.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dtReminder.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            dtReminder.Name = "dtReminder";
            dtReminder.ShowCheckBox = true;
            dtReminder.Size = new System.Drawing.Size(160, 23);
            dtReminder.Checked = false;
            toolTip.SetToolTip(dtReminder, "Tick to get a reminder popup at this time (also added to the calendar)");
            dtReminder.ValueChanged += NoteProps_Changed;
            // 
            // timerAutosave
            // 
            timerAutosave.Interval = 1500;
            timerAutosave.Tick += TimerAutosave_Tick;
            // 
            // timerSearch
            // 
            timerSearch.Interval = 250;
            timerSearch.Tick += TimerSearch_Tick;
            // 
            // NotepadForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 720);
            Controls.Add(splitMain);
            Controls.Add(toolStripFormat);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip;
            MinimumSize = new System.Drawing.Size(760, 480);
            Name = "NotepadForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Notepad";
            flowProps.ResumeLayout(false);
            flowProps.PerformLayout();
            tableProps.ResumeLayout(false);
            tableProps.PerformLayout();
            panelFind.ResumeLayout(false);
            panelFind.PerformLayout();
            flowListButtons.ResumeLayout(false);
            flowListButtons.PerformLayout();
            tableFilters.ResumeLayout(false);
            tableFilters.PerformLayout();
            tableList.ResumeLayout(false);
            tableList.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            splitMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStrip toolStripFormat;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblPosition;
        private System.Windows.Forms.ToolStripStatusLabel lblStats;
        private System.Windows.Forms.ToolStripStatusLabel lblZoom;
        private System.Windows.Forms.ToolStripStatusLabel lblSaved;
        private System.Windows.Forms.ToolStripStatusLabel lblNoteCount;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.TableLayoutPanel tableList;
        private System.Windows.Forms.TextBox textSearch;
        private System.Windows.Forms.TableLayoutPanel tableFilters;
        private System.Windows.Forms.ComboBox comboCategory;
        private System.Windows.Forms.ComboBox comboSort;
        private System.Windows.Forms.ListBox listNotes;
        private System.Windows.Forms.FlowLayoutPanel flowListButtons;
        private System.Windows.Forms.Button btnNewNote;
        private System.Windows.Forms.Button btnTemplate;
        private System.Windows.Forms.Button btnDeleteNote;
        private System.Windows.Forms.Button btnSticky;
        private System.Windows.Forms.TabControl tabNotes;
        private System.Windows.Forms.FlowLayoutPanel panelFind;
        private System.Windows.Forms.Label lblFind;
        private System.Windows.Forms.TextBox txtFind;
        private System.Windows.Forms.Button btnFindPrev;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.CheckBox chkWholeWord;
        private System.Windows.Forms.Label lblFindCount;
        private System.Windows.Forms.Label lblReplace;
        private System.Windows.Forms.TextBox txtReplace;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.Button btnFindClose;
        private System.Windows.Forms.TableLayoutPanel tableProps;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.FlowLayoutPanel flowProps;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox comboNoteCategory;
        private System.Windows.Forms.Label lblTags;
        private System.Windows.Forms.TextBox txtTags;
        private System.Windows.Forms.Button btnNoteColor;
        private System.Windows.Forms.CheckBox chkPinned;
        private System.Windows.Forms.CheckBox chkCode;
        private System.Windows.Forms.Label lblReminder;
        private System.Windows.Forms.DateTimePicker dtReminder;
        private System.Windows.Forms.Timer timerAutosave;
        private System.Windows.Forms.Timer timerSearch;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

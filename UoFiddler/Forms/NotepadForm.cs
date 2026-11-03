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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Forms.Notepad;

namespace UoFiddler.Forms
{
    /// <summary>
    /// Notepad: notes with categories, tags, colors, pins and reminders; rich text editor with
    /// headings, lists, checklists, images and UO graphics; search over all notes; find / replace;
    /// print, PDF, HTML, Markdown export; sticky notes; code notes for ServUO scripts.
    /// </summary>
    public partial class NotepadForm : Form
    {
        #region Hooks from the main window

        /// <summary>Shows an item id in UoFiddler's Items tab.</summary>
        public Action<int> ShowItemInFiddler { get; set; }

        /// <summary>Item id selected in UoFiddler's Items tab, or -1.</summary>
        public Func<int> SelectedItemProvider { get; set; }

        /// <summary>Gump id selected in UoFiddler's Gumps tab, or -1.</summary>
        public Func<int> SelectedGumpProvider { get; set; }

        #endregion

        private static readonly Color _headingColor = Color.FromArgb(31, 78, 121);

        private static readonly (string Name, Color Color)[] _textColors =
        {
            ("Black", Color.Black), ("Dark gray", Color.FromArgb(90, 90, 90)), ("Red", Color.FromArgb(200, 30, 30)),
            ("Orange", Color.FromArgb(220, 110, 0)), ("Green", Color.FromArgb(20, 135, 60)), ("Blue", Color.FromArgb(31, 78, 200)),
            ("Purple", Color.FromArgb(130, 50, 170)), ("Brown", Color.FromArgb(140, 80, 30))
        };

        private static readonly (string Name, Color Color)[] _highlightColors =
        {
            ("Yellow", Color.FromArgb(255, 240, 120)), ("Green", Color.FromArgb(190, 240, 180)), ("Blue", Color.FromArgb(190, 225, 255)),
            ("Pink", Color.FromArgb(255, 200, 225)), ("Orange", Color.FromArgb(255, 215, 160)), ("Gray", Color.FromArgb(225, 225, 225))
        };

        private static readonly (string Name, Color Color)[] _noteColors =
        {
            ("Red", Color.FromArgb(230, 80, 80)), ("Orange", Color.FromArgb(240, 150, 50)), ("Yellow", Color.FromArgb(235, 200, 40)),
            ("Green", Color.FromArgb(80, 180, 90)), ("Blue", Color.FromArgb(60, 130, 220)), ("Purple", Color.FromArgb(150, 90, 200)),
            ("Gray", Color.FromArgb(150, 150, 150))
        };

        private readonly NotepadData _data = NoteStore.Data;
        private readonly Dictionary<string, TabPage> _tabs = new();
        private readonly Timer _statsTimer = new() { Interval = 400 };
        private readonly IdPreviewPopup _idPopup = new();
        private readonly ContextMenuStrip _editorMenu = new();
        private readonly ContextMenuStrip _listMenu = new();
        private readonly ContextMenuStrip _tabMenu = new();
        private readonly ContextMenuStrip _templateMenu = new();
        private readonly Font _listTitleFont = new("Segoe UI", 9.75F, FontStyle.Bold);
        private readonly Font _listSmallFont = new("Segoe UI", 8.25F);

        private PageSettings _pageSettings;
        private bool _updatingProps;
        private bool _updatingToolbar;
        private bool _updatingList;
        private bool _metaDirty;
        private bool _reminderChanged;
        private int _contextIndex = -1;
        private TabPage _menuTab;

        private ToolStripComboBox _fontCombo;
        private ToolStripComboBox _sizeCombo;
        private ToolStripComboBox _headingCombo;
        private ToolStripButton _boldBtn, _italicBtn, _underlineBtn, _strikeBtn;
        private ToolStripButton _bulletBtn, _numberBtn, _checkBtn, _alignLeftBtn, _alignCenterBtn, _alignRightBtn;
        private ToolStripMenuItem _wordWrapItem, _lineNumbersItem, _topMostItem, _showListItem, _printHeaderItem;

        private NotepadSettings Settings => _data.Settings;

        #region Construction

        public NotepadForm()
        {
            InitializeComponent();

            BuildMenus();
            BuildToolbar();
            BuildContextMenus();

            _updatingList = true;
            int sort = comboSort.Items.IndexOf(Settings.SortBy);
            comboSort.SelectedIndex = sort < 0 ? 0 : sort;
            _updatingList = false;

            TopMost = Settings.TopMost;
            splitMain.Panel1Collapsed = !Settings.ShowList;

            RefreshCategories();
            RefreshList();

            foreach (string id in Settings.OpenNoteIds.ToList())
            {
                Note n = NoteStore.Find(id);
                if (n != null)
                {
                    OpenNote(n, false);
                }
            }

            Note active = NoteStore.Find(Settings.ActiveNoteId);
            if (active != null)
            {
                OpenNote(active);
            }
            else if (tabNotes.TabCount == 0)
            {
                Note first = _data.Notes.OrderByDescending(n => n.Pinned).ThenByDescending(n => n.Modified).FirstOrDefault();
                if (first != null)
                {
                    OpenNote(first);
                }
                else
                {
                    NewNote(null);
                }
            }

            UpdateEditorDependentUi();

            NoteStore.NoteSaved += OnNoteSavedElsewhere;
            _statsTimer.Tick += (_, _) =>
            {
                _statsTimer.Stop();
                UpdateStats();
            };

            Load += (_, _) =>
            {
                try
                {
                    splitMain.SplitterDistance = Math.Clamp(Settings.SplitterDistance, splitMain.Panel1MinSize,
                        Math.Max(splitMain.Panel1MinSize, splitMain.Width - splitMain.Panel2MinSize));
                }
                catch (InvalidOperationException)
                {
                    // window too small
                }

                if (NoteStore.LastMigrationInfo.Length > 0)
                {
                    lblSaved.Text = NoteStore.LastMigrationInfo;
                }

                CurrentEditor?.Box.Focus();
            };
            Deactivate += (_, _) => _idPopup.Hide();
            KeyDown += NotepadForm_KeyDown;
            FormClosing += NotepadForm_FormClosing;
            FormClosed += (_, _) =>
            {
                NoteStore.NoteSaved -= OnNoteSavedElsewhere;
                _statsTimer.Dispose();
                _idPopup.Dispose();
                _editorMenu.Dispose();
                _listMenu.Dispose();
                _tabMenu.Dispose();
                _templateMenu.Dispose();
                _listTitleFont.Dispose();
                _listSmallFont.Dispose();
            };
        }

        private void NotepadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveAll();
            Settings.SplitterDistance = splitMain.SplitterDistance;
            Settings.OpenNoteIds = tabNotes.TabPages.Cast<TabPage>().Select(p => ((NoteEditor)p.Tag).Note.Id).ToList();
            Settings.ActiveNoteId = CurrentNote?.Id ?? "";
            Settings.SortBy = comboSort.SelectedItem?.ToString() ?? "Modified";
            try
            {
                NoteStore.Save();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(this, "The notes could not be saved:\n" + ex.Message + "\n\nClose anyway?", "Notepad",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Opens a note by id (used by reminders).
        /// </summary>
        public void OpenNoteById(string id)
        {
            Note n = NoteStore.Find(id);
            if (n != null)
            {
                OpenNote(n);
            }

            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            Activate();
        }

        #endregion

        #region Menus, toolbar, context menus

        private static ToolStripMenuItem Item(string text, EventHandler click, Keys keys = Keys.None, string display = null)
        {
            var item = new ToolStripMenuItem(text, null, click);
            if (keys != Keys.None)
            {
                item.ShortcutKeys = keys;
            }

            if (display != null)
            {
                item.ShortcutKeyDisplayString = display;
            }

            return item;
        }

        private void BuildMenus()
        {
            // File
            var file = new ToolStripMenuItem("&File");
            var templates = new ToolStripMenuItem("New from template");
            foreach (string t in NoteTemplates.Names)
            {
                string name = t;
                templates.DropDownItems.Add(Item(name, (_, _) => NewFromTemplate(name)));
            }

            var export = new ToolStripMenuItem("Export note");
            export.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("PDF (.pdf) …", (_, _) => Export("pdf")),
                Item("Web page (.html) …", (_, _) => Export("html")),
                Item("Markdown (.md) …", (_, _) => Export("md")),
                Item("Rich text (.rtf) …", (_, _) => Export("rtf")),
                Item("Text (.txt) …", (_, _) => Export("txt")),
                new ToolStripSeparator(),
                Item("Backup of all notes (.json) …", (_, _) => ExportBackup())
            });
            _printHeaderItem = Item("Print title and page numbers", (_, _) =>
            {
                Settings.PrintHeader = !Settings.PrintHeader;
                _printHeaderItem.Checked = Settings.PrintHeader;
            });
            _printHeaderItem.Checked = Settings.PrintHeader;

            file.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("New note", (_, _) => NewNote(null), Keys.Control | Keys.N),
                templates,
                new ToolStripSeparator(),
                Item("Import files …", (_, _) => ImportFiles(), Keys.Control | Keys.O),
                export,
                new ToolStripSeparator(),
                Item("Print …", (_, _) => Print(), Keys.Control | Keys.P),
                Item("Print preview …", (_, _) => PrintPreview()),
                Item("Page setup …", (_, _) => PageSetup()),
                _printHeaderItem,
                new ToolStripSeparator(),
                Item("Save now", (_, _) => SaveAll(), Keys.Control | Keys.S),
                Item("Open data folder", (_, _) => OpenFolder(NoteStore.Folder)),
                new ToolStripSeparator(),
                Item("Close tab", (_, _) => CloseTab(tabNotes.SelectedTab), Keys.Control | Keys.W),
                Item("Close window", (_, _) => Close())
            });

            // Edit (Ctrl+C/V/X/Z/A are handled by the text boxes themselves)
            var edit = new ToolStripMenuItem("&Edit");
            edit.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Undo", (_, _) => CurrentBox?.Undo(), Keys.None, "Ctrl+Z"),
                Item("Redo", (_, _) => CurrentBox?.Redo(), Keys.None, "Ctrl+Y"),
                new ToolStripSeparator(),
                Item("Cut", (_, _) => CurrentBox?.Cut(), Keys.None, "Ctrl+X"),
                Item("Copy", (_, _) => CurrentBox?.Copy(), Keys.None, "Ctrl+C"),
                Item("Paste", (_, _) => CurrentBox?.Paste(), Keys.None, "Ctrl+V"),
                Item("Paste as plain text", (_, _) => CurrentBox?.PastePlainText(), Keys.None, "Ctrl+Shift+V"),
                Item("Select all", (_, _) => CurrentBox?.SelectAll(), Keys.None, "Ctrl+A"),
                new ToolStripSeparator(),
                Item("Find …", (_, _) => ShowFind(false), Keys.Control | Keys.F),
                Item("Replace …", (_, _) => ShowFind(true), Keys.Control | Keys.H),
                Item("Find next", (_, _) => FindNext(true), Keys.None, "F3"),
                Item("Search all notes", (_, _) => FocusGlobalSearch(), Keys.Control | Keys.Shift | Keys.F)
            });

            // Format
            var format = new ToolStripMenuItem("F&ormat");
            var heading = new ToolStripMenuItem("Style");
            heading.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Normal", (_, _) => ApplyStyle(0), Keys.None, "Ctrl+0"),
                Item("Heading 1", (_, _) => ApplyStyle(1), Keys.None, "Ctrl+1"),
                Item("Heading 2", (_, _) => ApplyStyle(2), Keys.None, "Ctrl+2"),
                Item("Heading 3", (_, _) => ApplyStyle(3), Keys.None, "Ctrl+3"),
                Item("Quote", (_, _) => ApplyStyle(4)),
                Item("Code", (_, _) => ApplyStyle(5))
            });
            var spacing = new ToolStripMenuItem("Line spacing");
            spacing.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("1.0", (_, _) => WithBox(b => RichEditNative.SetLineSpacing(b, 0))),
                Item("1.5", (_, _) => WithBox(b => RichEditNative.SetLineSpacing(b, 1))),
                Item("2.0", (_, _) => WithBox(b => RichEditNative.SetLineSpacing(b, 2)))
            });
            format.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Bold", (_, _) => ToggleEffect(RichEditNative.CFM_BOLD), Keys.None, "Ctrl+B"),
                Item("Italic", (_, _) => ToggleEffect(RichEditNative.CFM_ITALIC), Keys.None, "Ctrl+I"),
                Item("Underline", (_, _) => ToggleEffect(RichEditNative.CFM_UNDERLINE), Keys.None, "Ctrl+U"),
                Item("Strikethrough", (_, _) => ToggleEffect(RichEditNative.CFM_STRIKEOUT), Keys.None, "Ctrl+Shift+X"),
                heading,
                new ToolStripSeparator(),
                Item("Text color …", (_, _) => PickColor(false)),
                Item("Highlight color …", (_, _) => PickColor(true)),
                Item("Font …", (_, _) => ChooseFont()),
                new ToolStripSeparator(),
                Item("Bullets", (_, _) => ToggleNumbering(RichEditNative.PFN_BULLET)),
                Item("Numbering", (_, _) => ToggleNumbering(RichEditNative.PFN_ARABIC)),
                Item("Checklist", (_, _) => ToggleChecklist(), Keys.None, "Ctrl+Shift+L"),
                Item("Increase indent", (_, _) => Indent(24)),
                Item("Decrease indent", (_, _) => Indent(-24)),
                spacing,
                new ToolStripSeparator(),
                Item("Clear formatting", (_, _) => ClearFormatting(), Keys.None, "Ctrl+Space")
            });

            // Insert
            var insert = new ToolStripMenuItem("&Insert");
            insert.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Image …", (_, _) => InsertImageFile()),
                Item("UO graphic (item / gump) …", (_, _) => InsertUoGraphic(-1)),
                Item("Checkbox ☐", (_, _) => WithBox(b => b.SelectedText = NoteRichTextBox.Unchecked + " ")),
                Item("Horizontal line", (_, _) => InsertLine()),
                new ToolStripSeparator(),
                Item("Date", (_, _) => InsertText(DateTime.Now.ToString("d", CultureInfo.CurrentCulture))),
                Item("Time", (_, _) => InsertText(DateTime.Now.ToString("t", CultureInfo.CurrentCulture))),
                Item("Date and time", (_, _) => InsertText(DateTime.Now.ToString("g", CultureInfo.CurrentCulture)), Keys.F5),
                new ToolStripSeparator(),
                Item("Hex ⇄ Dec converter …", (_, _) => ShowHexDec()),
                Item("Convert selection hex ⇄ dec", (_, _) => ConvertSelectionHexDec(), Keys.Control | Keys.Shift | Keys.H)
            });

            // View
            var view = new ToolStripMenuItem("&View");
            _showListItem = Item("Note list", (_, _) => ToggleList(), Keys.F9);
            _showListItem.Checked = Settings.ShowList;
            _wordWrapItem = Item("Word wrap", (_, _) => ToggleWordWrap());
            _wordWrapItem.Checked = Settings.WordWrap;
            _lineNumbersItem = Item("Line numbers", (_, _) => ToggleLineNumbers());
            _lineNumbersItem.Checked = Settings.LineNumbers;
            _topMostItem = Item("Always on top", (_, _) =>
            {
                Settings.TopMost = !Settings.TopMost;
                TopMost = Settings.TopMost;
                _topMostItem.Checked = Settings.TopMost;
            });
            _topMostItem.Checked = Settings.TopMost;
            view.DropDownItems.AddRange(new ToolStripItem[]
            {
                _showListItem, _wordWrapItem, _lineNumbersItem,
                new ToolStripSeparator(),
                Item("Zoom in", (_, _) => Zoom(0.1f), Keys.Control | Keys.Oemplus, "Ctrl++"),
                Item("Zoom out", (_, _) => Zoom(-0.1f), Keys.Control | Keys.OemMinus, "Ctrl+-"),
                Item("Zoom 100 %", (_, _) => Zoom(0)),
                new ToolStripSeparator(),
                _topMostItem
            });

            // Note
            var note = new ToolStripMenuItem("&Note");
            var color = new ToolStripMenuItem("Color mark");
            foreach ((string name, Color c) in _noteColors)
            {
                Color col = c;
                color.DropDownItems.Add(new ToolStripMenuItem(name, ColorSwatch(col), (_, _) => SetNoteColor(col.ToArgb())));
            }

            color.DropDownItems.Add(Item("None", (_, _) => SetNoteColor(0)));
            note.DropDownItems.AddRange(new ToolStripItem[]
            {
                Item("Pin / unpin", (_, _) => chkPinned.Checked = !chkPinned.Checked),
                color,
                Item("Duplicate", (_, _) => DuplicateNote(CurrentNote)),
                Item("Show as sticky note", (_, _) => ShowSticky(CurrentNote)),
                new ToolStripSeparator(),
                Item("Code note (C#)", (_, _) => chkCode.Checked = !chkCode.Checked),
                Item("Color the code now", (_, _) => CurrentEditor?.Highlight()),
                Item("Set reminder …", (_, _) =>
                {
                    dtReminder.Checked = true;
                    dtReminder.Focus();
                }),
                new ToolStripSeparator(),
                Item("Delete note", (_, _) => DeleteNote(CurrentNote))
            });

            var help = new ToolStripMenuItem("&Help");
            help.DropDownItems.Add(Item("Shortcuts and tips", (_, _) => ShowHelp(), Keys.F1));

            menuStrip.Items.AddRange(new ToolStripItem[] { file, edit, format, insert, view, note, help });
        }

        private static Bitmap ColorSwatch(Color c)
        {
            var bmp = new Bitmap(16, 16);
            using Graphics g = Graphics.FromImage(bmp);
            using var brush = new SolidBrush(c);
            g.FillRectangle(brush, 1, 1, 14, 14);
            g.DrawRectangle(Pens.Gray, 1, 1, 13, 13);
            return bmp;
        }

        private ToolStripButton ToolButton(string text, string tip, EventHandler click, FontStyle style = FontStyle.Regular)
        {
            var b = new ToolStripButton(text) { ToolTipText = tip, DisplayStyle = ToolStripItemDisplayStyle.Text, AutoSize = true };
            if (style != FontStyle.Regular)
            {
                b.Font = new Font("Segoe UI", 9.75F, style);
            }

            b.Click += click;
            return b;
        }

        private void BuildToolbar()
        {
            _fontCombo = new ToolStripComboBox { Size = new Size(150, 23), ToolTipText = "Font", FlatStyle = FlatStyle.System };
            _fontCombo.Items.AddRange(FontFamily.Families.Select(f => (object)f.Name).ToArray());
            _fontCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _fontCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            _fontCombo.SelectedIndexChanged += (_, _) => ApplyFontFromCombo();
            _fontCombo.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ApplyFontFromCombo();
                    e.SuppressKeyPress = true;
                }
            };

            _sizeCombo = new ToolStripComboBox { Size = new Size(50, 23), ToolTipText = "Font size", FlatStyle = FlatStyle.System };
            _sizeCombo.Items.AddRange(new object[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "36", "48", "72" });
            _sizeCombo.SelectedIndexChanged += (_, _) => ApplySizeFromCombo();
            _sizeCombo.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ApplySizeFromCombo();
                    e.SuppressKeyPress = true;
                }
            };

            _boldBtn = ToolButton("B", "Bold (Ctrl+B)", (_, _) => ToggleEffect(RichEditNative.CFM_BOLD), FontStyle.Bold);
            _italicBtn = ToolButton("I", "Italic (Ctrl+I)", (_, _) => ToggleEffect(RichEditNative.CFM_ITALIC), FontStyle.Italic);
            _underlineBtn = ToolButton("U", "Underline (Ctrl+U)", (_, _) => ToggleEffect(RichEditNative.CFM_UNDERLINE), FontStyle.Underline);
            _strikeBtn = ToolButton("S", "Strikethrough (Ctrl+Shift+X)", (_, _) => ToggleEffect(RichEditNative.CFM_STRIKEOUT), FontStyle.Strikeout);

            var textColor = new ToolStripDropDownButton("A") { ToolTipText = "Text color", ForeColor = Color.FromArgb(200, 30, 30), Font = new Font("Segoe UI", 9.75F, FontStyle.Bold) };
            foreach ((string name, Color c) in _textColors)
            {
                Color col = c;
                textColor.DropDownItems.Add(new ToolStripMenuItem(name, ColorSwatch(col), (_, _) => WithBox(b => RichEditNative.SetTextColor(b, col))));
            }

            textColor.DropDownItems.Add(new ToolStripSeparator());
            textColor.DropDownItems.Add(Item("Automatic", (_, _) => WithBox(b => RichEditNative.SetTextColor(b, null))));
            textColor.DropDownItems.Add(Item("More colors …", (_, _) => PickColor(false)));

            var highlight = new ToolStripDropDownButton("Mark") { ToolTipText = "Highlight color", BackColor = Color.FromArgb(255, 240, 120) };
            foreach ((string name, Color c) in _highlightColors)
            {
                Color col = c;
                highlight.DropDownItems.Add(new ToolStripMenuItem(name, ColorSwatch(col), (_, _) => WithBox(b => RichEditNative.SetBackColor(b, col))));
            }

            highlight.DropDownItems.Add(new ToolStripSeparator());
            highlight.DropDownItems.Add(Item("No highlight", (_, _) => WithBox(b => RichEditNative.SetBackColor(b, null))));
            highlight.DropDownItems.Add(Item("More colors …", (_, _) => PickColor(true)));

            _headingCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(95, 23), ToolTipText = "Paragraph style (Ctrl+0 … Ctrl+3)", FlatStyle = FlatStyle.System };
            _headingCombo.Items.AddRange(new object[] { "Normal", "Heading 1", "Heading 2", "Heading 3", "Quote", "Code" });
            _headingCombo.SelectedIndexChanged += (_, _) =>
            {
                if (!_updatingToolbar && _headingCombo.SelectedIndex >= 0)
                {
                    ApplyStyle(_headingCombo.SelectedIndex);
                }
            };

            _alignLeftBtn = ToolButton("⯇≡", "Align left", (_, _) => Align(HorizontalAlignment.Left));
            _alignLeftBtn.Text = "Left";
            _alignCenterBtn = ToolButton("Center", "Center", (_, _) => Align(HorizontalAlignment.Center));
            _alignRightBtn = ToolButton("Right", "Align right", (_, _) => Align(HorizontalAlignment.Right));
            _bulletBtn = ToolButton("•  List", "Bullets", (_, _) => ToggleNumbering(RichEditNative.PFN_BULLET));
            _numberBtn = ToolButton("1.  List", "Numbering", (_, _) => ToggleNumbering(RichEditNative.PFN_ARABIC));
            _checkBtn = ToolButton("☑ Todo", "Checklist (Ctrl+Shift+L) – click a box to tick it off", (_, _) => ToggleChecklist());

            toolStripFormat.Items.AddRange(new ToolStripItem[]
            {
                _fontCombo, _sizeCombo, new ToolStripSeparator(),
                _boldBtn, _italicBtn, _underlineBtn, _strikeBtn, textColor, highlight, new ToolStripSeparator(),
                _headingCombo, new ToolStripSeparator(),
                _alignLeftBtn, _alignCenterBtn, _alignRightBtn, new ToolStripSeparator(),
                _bulletBtn, _numberBtn, _checkBtn,
                ToolButton("«", "Decrease indent", (_, _) => Indent(-24)),
                ToolButton("»", "Increase indent", (_, _) => Indent(24)),
                new ToolStripSeparator(),
                ToolButton("Image", "Insert an image file (or paste / drop one)", (_, _) => InsertImageFile()),
                ToolButton("UO", "Insert an item or gump graphic", (_, _) => InsertUoGraphic(-1)),
                ToolButton("—", "Horizontal line", (_, _) => InsertLine()),
                ToolButton("Hex⇄Dec", "Convert the selected number (Ctrl+Shift+H)", (_, _) => ConvertSelectionHexDec()),
                new ToolStripSeparator(),
                ToolButton("Clear", "Clear formatting (Ctrl+Space)", (_, _) => ClearFormatting()),
                ToolButton("Print", "Print (Ctrl+P)", (_, _) => Print()),
                ToolButton("PDF", "Save as PDF", (_, _) => Export("pdf"))
            });
        }

        private void BuildContextMenus()
        {
            // editor
            _editorMenu.Opening += EditorMenu_Opening;

            // note list
            _listMenu.Items.AddRange(new ToolStripItem[]
            {
                Item("Open", (_, _) => OpenSelectedListNote()),
                Item("Show as sticky note", (_, _) => ShowSticky(SelectedListNote)),
                Item("Pin / unpin", (_, _) => TogglePin(SelectedListNote)),
                Item("Duplicate", (_, _) => DuplicateNote(SelectedListNote)),
                new ToolStripSeparator(),
                Item("Delete", (_, _) => DeleteNote(SelectedListNote))
            });
            listNotes.ContextMenuStrip = _listMenu;

            // tabs
            _tabMenu.Items.AddRange(new ToolStripItem[]
            {
                Item("Close", (_, _) => CloseTab(_menuTab)),
                Item("Close other tabs", (_, _) =>
                {
                    foreach (TabPage p in tabNotes.TabPages.Cast<TabPage>().Where(p => p != _menuTab).ToList())
                    {
                        CloseTab(p);
                    }
                }),
                Item("Show as sticky note", (_, _) => ShowSticky((_menuTab?.Tag as NoteEditor)?.Note))
            });

            // templates button
            foreach (string t in NoteTemplates.Names)
            {
                string name = t;
                _templateMenu.Items.Add(Item(name, (_, _) => NewFromTemplate(name)));
            }
        }

        #endregion

        #region Current editor helpers

        private NoteEditor CurrentEditor => tabNotes.SelectedTab?.Tag as NoteEditor;

        private NoteRichTextBox CurrentBox => CurrentEditor?.Box;

        private Note CurrentNote => CurrentEditor?.Note;

        private IEnumerable<NoteEditor> Editors => tabNotes.TabPages.Cast<TabPage>().Select(p => p.Tag).OfType<NoteEditor>();

        private void WithBox(Action<NoteRichTextBox> action)
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                return;
            }

            action(box);
            box.Focus();
            UpdateToolbarState();
        }

        #endregion

        #region Note list

        private Note SelectedListNote => listNotes.SelectedItem as Note;

        private void RefreshCategories()
        {
            string filter = comboCategory.SelectedIndex > 0 ? comboCategory.SelectedItem?.ToString() : null;
            List<string> categories = _data.Notes.Select(n => n.Category).Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase).ToList();

            bool keep = _updatingList;
            _updatingList = true;
            comboCategory.BeginUpdate();
            comboCategory.Items.Clear();
            comboCategory.Items.Add("All categories");
            comboCategory.Items.AddRange(categories.Cast<object>().ToArray());
            comboCategory.EndUpdate();
            int idx = filter == null ? 0 : comboCategory.Items.IndexOf(filter);
            comboCategory.SelectedIndex = idx < 0 ? 0 : idx;

            string current = comboNoteCategory.Text;
            bool keepProps = _updatingProps;
            _updatingProps = true;
            comboNoteCategory.Items.Clear();
            comboNoteCategory.Items.AddRange(categories.Concat(new[] { "Todo", "Bugs", "Scripts", "Building", "Shard" })
                .Distinct(StringComparer.OrdinalIgnoreCase).Cast<object>().ToArray());
            comboNoteCategory.Text = current;
            _updatingProps = keepProps;
            _updatingList = keep;
        }

        private string[] SearchTerms => textSearch.Text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        private void RefreshList()
        {
            string[] terms = SearchTerms;
            string category = comboCategory.SelectedIndex > 0 ? comboCategory.SelectedItem?.ToString() : null;

            IEnumerable<Note> query = _data.Notes
                .Where(n => category == null || string.Equals(n.Category, category, StringComparison.OrdinalIgnoreCase))
                .Where(n => n.Matches(terms));

            IOrderedEnumerable<Note> sorted = query.OrderByDescending(n => n.Pinned);
            sorted = (comboSort.SelectedItem?.ToString()) switch
            {
                "Created" => sorted.ThenByDescending(n => n.Created),
                "Title" => sorted.ThenBy(n => n.Title, StringComparer.CurrentCultureIgnoreCase),
                "Category" => sorted.ThenBy(n => n.Category, StringComparer.CurrentCultureIgnoreCase).ThenByDescending(n => n.Modified),
                _ => sorted.ThenByDescending(n => n.Modified)
            };

            List<Note> notes = sorted.ToList();
            int top = listNotes.TopIndex;

            _updatingList = true;
            listNotes.BeginUpdate();
            listNotes.Items.Clear();
            listNotes.Items.AddRange(notes.Cast<object>().ToArray());
            Note current = CurrentNote;
            if (current != null && notes.Contains(current))
            {
                listNotes.SelectedItem = current;
            }

            if (top < listNotes.Items.Count)
            {
                listNotes.TopIndex = top;
            }

            listNotes.EndUpdate();
            _updatingList = false;

            lblNoteCount.Text = notes.Count == _data.Notes.Count
                ? $"{_data.Notes.Count} notes"
                : $"{notes.Count} of {_data.Notes.Count} notes";
        }

        private void ListNotes_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= listNotes.Items.Count || listNotes.Items[e.Index] is not Note n)
            {
                return;
            }

            Graphics g = e.Graphics;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Rectangle r = e.Bounds;
            using (var back = new SolidBrush(selected ? Color.FromArgb(218, 232, 252) : Color.White))
            {
                g.FillRectangle(back, r);
            }

            if (n.Color != 0)
            {
                using var stripe = new SolidBrush(Color.FromArgb(n.Color));
                g.FillRectangle(stripe, r.X, r.Y + 4, 4, r.Height - 8);
            }

            bool dirty = _tabs.TryGetValue(n.Id, out TabPage page) && page.Tag is NoteEditor { Dirty: true };
            string title = (n.Pinned ? "📌 " : "") + (dirty ? "● " : "") + (string.IsNullOrWhiteSpace(n.Title) ? "(no title)" : n.Title);
            int x = r.X + 10, w = r.Width - 14;
            TextRenderer.DrawText(g, title, _listTitleFont, new Rectangle(x, r.Y + 4, w, 18), Color.FromArgb(30, 35, 45),
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            var meta = new StringBuilder(n.Modified.ToString("g", CultureInfo.CurrentCulture));
            if (n.Category.Length > 0)
            {
                meta.Append("  ·  ").Append(n.Category);
            }

            if (n.ReminderAt.HasValue && !n.ReminderDone)
            {
                meta.Append("  ·  ⏰ ").Append(n.ReminderAt.Value.ToString("g", CultureInfo.CurrentCulture));
            }

            if (n.Tags.Count > 0)
            {
                meta.Append("  ·  #").Append(string.Join(" #", n.Tags));
            }

            TextRenderer.DrawText(g, meta.ToString(), _listSmallFont, new Rectangle(x, r.Y + 23, w, 15), Color.FromArgb(31, 78, 121),
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, n.Snippet(), _listSmallFont, new Rectangle(x, r.Y + 39, w, 15), Color.Gray,
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            using var line = new Pen(Color.FromArgb(232, 234, 238));
            g.DrawLine(line, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
        }

        private void ListNotes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingList)
            {
                OpenSelectedListNote();
            }
        }

        private void OpenSelectedListNote()
        {
            if (SelectedListNote is Note n)
            {
                OpenNote(n);
                HighlightSearchInEditor();
            }
        }

        private void ListNotes_DoubleClick(object sender, EventArgs e) => CurrentBox?.Focus();

        private void ListNotes_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listNotes.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    _updatingList = true;
                    listNotes.SelectedIndex = index;
                    _updatingList = false;
                }
            }
        }

        private void ListNotes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteNote(SelectedListNote);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                CurrentBox?.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private void ComboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingList)
            {
                RefreshList();
            }
        }

        private void ComboSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updatingList)
            {
                Settings.SortBy = comboSort.SelectedItem?.ToString() ?? "Modified";
                RefreshList();
            }
        }

        private void TextSearch_TextChanged(object sender, EventArgs e)
        {
            timerSearch.Stop();
            timerSearch.Start();
        }

        private void TimerSearch_Tick(object sender, EventArgs e)
        {
            timerSearch.Stop();
            RefreshList();
            HighlightSearchInEditor();
        }

        private void TextSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                timerSearch.Stop();
                RefreshList();
                if (listNotes.Items.Count > 0)
                {
                    listNotes.SelectedIndex = 0;
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Down && listNotes.Items.Count > 0)
            {
                listNotes.Focus();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                textSearch.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void FocusGlobalSearch()
        {
            if (splitMain.Panel1Collapsed)
            {
                ToggleList();
            }

            textSearch.Focus();
            textSearch.SelectAll();
        }

        /// <summary>
        /// Selects the first match of the global search in the open note.
        /// </summary>
        private void HighlightSearchInEditor()
        {
            NoteRichTextBox box = CurrentBox;
            string[] terms = SearchTerms;
            if (box == null || terms.Length == 0)
            {
                return;
            }

            int index = box.Text.IndexOf(terms[0], StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                box.Select(index, terms[0].Length);
                box.ScrollToCaret();
            }
        }

        #endregion

        #region Open / close / new / delete notes

        private NoteEditor OpenNote(Note n, bool select = true)
        {
            if (n == null)
            {
                return null;
            }

            if (_tabs.TryGetValue(n.Id, out TabPage existing))
            {
                if (select)
                {
                    tabNotes.SelectedTab = existing;
                }

                return (NoteEditor)existing.Tag;
            }

            var editor = new NoteEditor(n, Settings) { Dock = DockStyle.Fill };
            var page = new TabPage { Tag = editor, ToolTipText = n.Title, UseVisualStyleBackColor = true };
            page.Text = TabTitle(editor);
            page.Controls.Add(editor);

            NoteRichTextBox box = editor.Box;
            box.ContextMenuStrip = _editorMenu;
            editor.DirtyChanged += (_, _) =>
            {
                page.Text = TabTitle(editor);
                listNotes.Invalidate();
                if (editor.Dirty)
                {
                    RestartAutosave();
                }
            };
            box.TextChanged += (_, _) =>
            {
                _statsTimer.Stop();
                _statsTimer.Start();
            };
            box.SelectionChanged += (_, _) =>
            {
                if (editor == CurrentEditor)
                {
                    UpdatePosition();
                    UpdateToolbarState();
                }
            };
            box.HoverIdChanged += OnHoverId;
            box.MouseDown += (_, e) => _contextIndex = box.GetCharIndexFromPosition(e.Location);
            box.MouseClick += (_, e) => OnEditorClick(box, e);
            box.LinkClicked += (_, e) => StickyNoteForm.OpenLink(e.LinkText);
            box.DragEnter += Editor_DragEnter;
            box.DragDrop += Editor_DragDrop;
            box.MouseWheel += (_, _) => UpdateZoomLabel();

            _tabs[n.Id] = page;
            tabNotes.TabPages.Add(page);
            if (select)
            {
                tabNotes.SelectedTab = page;
                UpdateEditorDependentUi();
            }

            return editor;
        }

        private static string TabTitle(NoteEditor editor)
        {
            string title = string.IsNullOrWhiteSpace(editor.Note.Title) ? "(no title)" : editor.Note.Title;
            if (title.Length > 26)
            {
                title = title.Substring(0, 25) + "…";
            }

            return (editor.Dirty ? "● " : "") + title + "  ";
        }

        private void CloseTab(TabPage page)
        {
            if (page?.Tag is not NoteEditor editor)
            {
                return;
            }

            SaveAll();
            _tabs.Remove(editor.Note.Id);
            tabNotes.TabPages.Remove(page);
            page.Dispose();
            UpdateEditorDependentUi();
        }

        private void TabNotes_SelectedIndexChanged(object sender, EventArgs e) => UpdateEditorDependentUi();

        private void TabNotes_MouseUp(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabNotes.TabCount; i++)
            {
                if (!tabNotes.GetTabRect(i).Contains(e.Location))
                {
                    continue;
                }

                if (e.Button == MouseButtons.Middle)
                {
                    CloseTab(tabNotes.TabPages[i]);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    _menuTab = tabNotes.TabPages[i];
                    _tabMenu.Show(tabNotes, e.Location);
                }

                return;
            }
        }

        private void UpdateEditorDependentUi()
        {
            bool has = CurrentEditor != null;
            tableProps.Enabled = has;
            toolStripFormat.Enabled = has;
            LoadProps(CurrentNote);
            UpdatePosition();
            UpdateStats();
            UpdateToolbarState();
            UpdateZoomLabel();
            Text = has ? $"Notepad – {CurrentNote.Title}" : "Notepad";

            if (has && listNotes.SelectedItem != CurrentNote && listNotes.Items.Contains(CurrentNote))
            {
                _updatingList = true;
                listNotes.SelectedItem = CurrentNote;
                _updatingList = false;
            }
        }

        private void BtnNewNote_Click(object sender, EventArgs e) => NewNote(null);

        private void NewNote(Note template)
        {
            Note n = template ?? new Note
            {
                Category = comboCategory.SelectedIndex > 0 ? comboCategory.SelectedItem?.ToString() ?? "" : ""
            };
            _data.Notes.Add(n);
            TrySave();
            RefreshCategories();
            RefreshList();
            OpenNote(n);
            if (template == null)
            {
                txtTitle.Focus();
                txtTitle.SelectAll();
            }
            else
            {
                CurrentBox?.Focus();
            }
        }

        private void NewFromTemplate(string name) => NewNote(NoteTemplates.Create(name, Settings));

        private void BtnTemplate_Click(object sender, EventArgs e) => _templateMenu.Show(btnTemplate, new Point(0, btnTemplate.Height));

        private void BtnDeleteNote_Click(object sender, EventArgs e) => DeleteNote(SelectedListNote ?? CurrentNote);

        private void DeleteNote(Note n)
        {
            if (n == null)
            {
                return;
            }

            if (MessageBox.Show(this, $"Delete the note \"{n.Title}\"?\n\n(A daily backup is kept in the data folder.)", "Notepad",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (_tabs.TryGetValue(n.Id, out TabPage page))
            {
                _tabs.Remove(n.Id);
                tabNotes.TabPages.Remove(page);
                page.Dispose();
            }

            foreach (StickyNoteForm sticky in Application.OpenForms.OfType<StickyNoteForm>().Where(s => s.NoteId == n.Id).ToList())
            {
                sticky.Close();
            }

            _data.Notes.Remove(n);
            TrySave();
            RefreshCategories();
            RefreshList();
            if (tabNotes.TabCount == 0 && listNotes.Items.Count > 0)
            {
                OpenNote((Note)listNotes.Items[0]);
            }

            UpdateEditorDependentUi();
        }

        private void DuplicateNote(Note n)
        {
            if (n == null)
            {
                return;
            }

            SaveAll();
            NewNote(new Note
            {
                Title = n.Title + " (copy)",
                Category = n.Category,
                Tags = n.Tags.ToList(),
                Color = n.Color,
                Rtf = n.Rtf,
                PlainText = n.PlainText,
                CodeMode = n.CodeMode
            });
        }

        private void TogglePin(Note n)
        {
            if (n == null)
            {
                return;
            }

            n.Pinned = !n.Pinned;
            if (n == CurrentNote)
            {
                LoadProps(n);
            }

            TrySave();
            RefreshList();
        }

        private void BtnSticky_Click(object sender, EventArgs e) => ShowSticky(SelectedListNote ?? CurrentNote);

        private void ShowSticky(Note n)
        {
            if (n == null)
            {
                return;
            }

            SaveAll();
            StickyNoteForm existing = Application.OpenForms.OfType<StickyNoteForm>().FirstOrDefault(s => s.NoteId == n.Id);
            if (existing != null)
            {
                existing.Activate();
                return;
            }

            new StickyNoteForm(n, note =>
            {
                Show();
                OpenNoteById(note.Id);
            }).Show();
        }

        #endregion

        #region Note properties

        private void LoadProps(Note n)
        {
            _updatingProps = true;
            try
            {
                txtTitle.Text = n?.Title ?? "";
                comboNoteCategory.Text = n?.Category ?? "";
                txtTags.Text = n?.TagText ?? "";
                chkPinned.Checked = n?.Pinned ?? false;
                chkCode.Checked = n?.CodeMode ?? false;
                btnNoteColor.BackColor = n != null && n.Color != 0 ? Color.FromArgb(n.Color) : SystemColors.Control;
                btnNoteColor.UseVisualStyleBackColor = n == null || n.Color == 0;
                if (n?.ReminderAt != null)
                {
                    dtReminder.Value = n.ReminderAt.Value;
                    dtReminder.Checked = !n.ReminderDone;
                }
                else
                {
                    dtReminder.Value = DateTime.Now.AddHours(1);
                    dtReminder.Checked = false;
                }
            }
            finally
            {
                _updatingProps = false;
            }
        }

        private void NoteProps_Changed(object sender, EventArgs e)
        {
            Note n = CurrentNote;
            if (_updatingProps || n == null)
            {
                return;
            }

            if (sender == txtTitle)
            {
                n.Title = txtTitle.Text;
                if (_tabs.TryGetValue(n.Id, out TabPage page))
                {
                    page.Text = TabTitle((NoteEditor)page.Tag);
                    page.ToolTipText = n.Title;
                }

                Text = $"Notepad – {n.Title}";
            }
            else if (sender == comboNoteCategory)
            {
                n.Category = comboNoteCategory.Text.Trim();
            }
            else if (sender == txtTags)
            {
                n.Tags = txtTags.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().TrimStart('#')).Where(t => t.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
            else if (sender == chkPinned)
            {
                n.Pinned = chkPinned.Checked;
            }
            else if (sender == chkCode)
            {
                n.CodeMode = chkCode.Checked;
                NoteEditor editor = CurrentEditor;
                if (n.CodeMode)
                {
                    editor.ApplyCodeMode();
                }
                else
                {
                    editor.Box.WordWrap = Settings.WordWrap;
                    editor.Dirty = true;
                }
            }
            else if (sender == dtReminder)
            {
                n.ReminderAt = dtReminder.Checked ? dtReminder.Value : null;
                n.ReminderDone = false;
                _reminderChanged = true;
            }

            n.Modified = DateTime.Now;
            _metaDirty = true;
            listNotes.Invalidate();
            RestartAutosave();
        }

        private void BtnNoteColor_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            foreach ((string name, Color c) in _noteColors)
            {
                Color col = c;
                menu.Items.Add(new ToolStripMenuItem(name, ColorSwatch(col), (_, _) => SetNoteColor(col.ToArgb())));
            }

            menu.Items.Add(Item("None", (_, _) => SetNoteColor(0)));
            menu.Closed += (_, _) => BeginInvoke(new Action(menu.Dispose));
            menu.Show(btnNoteColor, new Point(0, btnNoteColor.Height));
        }

        private void SetNoteColor(int argb)
        {
            Note n = CurrentNote;
            if (n == null)
            {
                return;
            }

            n.Color = argb;
            LoadProps(n);
            _metaDirty = true;
            listNotes.Invalidate();
            RestartAutosave();
        }

        #endregion

        #region Saving

        private void RestartAutosave()
        {
            timerAutosave.Stop();
            timerAutosave.Start();
        }

        private void TimerAutosave_Tick(object sender, EventArgs e) => SaveAll();

        /// <summary>
        /// Saves every changed note to disk.
        /// </summary>
        private void SaveAll()
        {
            timerAutosave.Stop();
            var changed = new List<Note>();
            foreach (NoteEditor editor in Editors)
            {
                if (editor.SaveToNote())
                {
                    changed.Add(editor.Note);
                }
            }

            if (_metaDirty && CurrentNote != null)
            {
                changed.Add(CurrentNote);
            }

            if (changed.Count == 0)
            {
                return;
            }

            if (_reminderChanged)
            {
                foreach (Note n in changed.Where(n => n.ReminderAt.HasValue))
                {
                    NoteReminderService.WriteToCalendar(n);
                }
            }

            if (TrySave())
            {
                foreach (Note n in changed.Distinct())
                {
                    NoteStore.NotifySaved(n, this);
                }

                lblSaved.Text = $"Saved {DateTime.Now:T}";
            }

            _metaDirty = false;
            _reminderChanged = false;
            RefreshCategories();
            RefreshList();
        }

        private bool TrySave()
        {
            try
            {
                NoteStore.Save();
                return true;
            }
            catch (Exception ex)
            {
                lblSaved.Text = "NOT SAVED: " + ex.Message;
                return false;
            }
        }

        private void OnNoteSavedElsewhere(Note n, object source)
        {
            if (source == this || IsDisposed)
            {
                return;
            }

            if (_tabs.TryGetValue(n.Id, out TabPage page) && page.Tag is NoteEditor { Dirty: false } editor)
            {
                editor.LoadFromNote();
                if (editor == CurrentEditor)
                {
                    LoadProps(n);
                }
            }

            RefreshList();
        }

        #endregion

        #region Formatting

        private void ToggleEffect(uint flag) => WithBox(b => RichEditNative.ToggleEffect(b, flag));

        private void ApplyFontFromCombo()
        {
            if (_updatingToolbar || string.IsNullOrWhiteSpace(_fontCombo.Text))
            {
                return;
            }

            string face = _fontCombo.Text.Trim();
            if (FontFamily.Families.Any(f => string.Equals(f.Name, face, StringComparison.OrdinalIgnoreCase)))
            {
                WithBox(b => RichEditNative.SetFontFace(b, face));
            }
        }

        private void ApplySizeFromCombo()
        {
            if (!_updatingToolbar && float.TryParse(_sizeCombo.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float size) && size is >= 4 and <= 400)
            {
                WithBox(b => RichEditNative.SetFontSize(b, size));
            }
        }

        private void PickColor(bool background)
        {
            using var dialog = new ColorDialog { FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                WithBox(b =>
                {
                    if (background)
                    {
                        RichEditNative.SetBackColor(b, dialog.Color);
                    }
                    else
                    {
                        RichEditNative.SetTextColor(b, dialog.Color);
                    }
                });
            }
        }

        private void ChooseFont()
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                return;
            }

            using var dialog = new FontDialog { Font = box.SelectionFont ?? box.Font, ShowColor = true, Color = box.SelectionColor };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                box.SelectionFont = dialog.Font;
                box.SelectionColor = dialog.Color;
                box.Focus();
            }
        }

        /// <summary>
        /// Start and end of the paragraphs touched by the selection.
        /// </summary>
        private static (int Start, int End) ParagraphRange(RichTextBox box)
        {
            string t = box.Text;
            int s = Math.Min(box.SelectionStart, t.Length);
            int e = Math.Min(s + box.SelectionLength, t.Length);
            int ps = s > 0 ? t.LastIndexOf('\n', s - 1) + 1 : 0;
            if (e > s && t[e - 1] == '\n')
            {
                e--;
            }

            int pe = t.IndexOf('\n', Math.Max(e, ps));
            return (ps, pe < 0 ? t.Length : pe);
        }

        /// <summary>
        /// 0 normal, 1-3 headings, 4 quote, 5 code.
        /// </summary>
        private void ApplyStyle(int style)
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                return;
            }

            int keepStart = box.SelectionStart, keepLength = box.SelectionLength;
            (int ps, int pe) = ParagraphRange(box);
            box.Select(ps, pe - ps);

            RichEditNative.SetFontFace(box, Settings.DefaultFont);
            RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD | RichEditNative.CFM_ITALIC, false);
            RichEditNative.SetBackColor(box, null);
            RichEditNative.SetTextColor(box, null);
            box.SelectionIndent = 0;

            switch (style)
            {
                case 1:
                    RichEditNative.SetFontSize(box, RtfConverter.H1Size);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD, true);
                    RichEditNative.SetTextColor(box, _headingColor);
                    break;
                case 2:
                    RichEditNative.SetFontSize(box, RtfConverter.H2Size);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD, true);
                    RichEditNative.SetTextColor(box, _headingColor);
                    break;
                case 3:
                    RichEditNative.SetFontSize(box, RtfConverter.H3Size);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD, true);
                    break;
                case 4:
                    RichEditNative.SetFontSize(box, Settings.DefaultSize);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_ITALIC, true);
                    RichEditNative.SetTextColor(box, Color.FromArgb(100, 100, 100));
                    box.SelectionIndent = 24;
                    break;
                case 5:
                    RichEditNative.SetFontFace(box, "Consolas");
                    RichEditNative.SetFontSize(box, 10f);
                    RichEditNative.SetBackColor(box, Color.FromArgb(242, 242, 242));
                    break;
                default:
                    RichEditNative.SetFontSize(box, Settings.DefaultSize);
                    break;
            }

            box.Select(keepStart, keepLength);
            box.Focus();
            UpdateToolbarState();
        }

        private void Align(HorizontalAlignment alignment) => WithBox(b => b.SelectionAlignment = alignment);

        private void ToggleNumbering(short kind) =>
            WithBox(b => RichEditNative.SetNumbering(b, RichEditNative.GetNumbering(b) == kind ? (short)0 : kind));

        private void Indent(int delta) => WithBox(b => b.SelectionIndent = Math.Max(0, b.SelectionIndent + delta));

        private void ToggleChecklist()
        {
            WithBox(box =>
            {
                (int ps, int pe) = ParagraphRange(box);
                string t = box.Text;
                var starts = new List<int> { ps };
                for (int i = ps; i < pe; i++)
                {
                    if (t[i] == '\n')
                    {
                        starts.Add(i + 1);
                    }
                }

                static bool IsBox(char c) => c == NoteRichTextBox.Unchecked || c == NoteRichTextBox.Checked;
                bool allBoxes = starts.All(i => i < t.Length && IsBox(t[i]));

                foreach (int i in Enumerable.Reverse(starts))
                {
                    if (allBoxes)
                    {
                        box.Select(i, i + 1 < t.Length && t[i + 1] == ' ' ? 2 : 1);
                        box.SelectedText = "";
                    }
                    else if (i >= t.Length || !IsBox(t[i]))
                    {
                        box.Select(i, 0);
                        box.SelectedText = NoteRichTextBox.Unchecked + " ";
                    }
                }
            });
        }

        private void ClearFormatting()
        {
            WithBox(box =>
            {
                if (box.SelectionLength == 0)
                {
                    (int ps, int pe) = ParagraphRange(box);
                    box.Select(ps, pe - ps);
                }

                RichEditNative.SetFontFace(box, Settings.DefaultFont);
                RichEditNative.SetFontSize(box, Settings.DefaultSize);
                RichEditNative.SetEffect(box, RichEditNative.CFM_BOLD | RichEditNative.CFM_ITALIC | RichEditNative.CFM_UNDERLINE | RichEditNative.CFM_STRIKEOUT, false);
                RichEditNative.SetTextColor(box, null);
                RichEditNative.SetBackColor(box, null);
                RichEditNative.SetNumbering(box, 0);
                RichEditNative.SetLineSpacing(box, 0);
                box.SelectionAlignment = HorizontalAlignment.Left;
                box.SelectionIndent = 0;
            });
        }

        private void UpdateToolbarState()
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null || !box.IsHandleCreated)
            {
                return;
            }

            _updatingToolbar = true;
            try
            {
                RichEditNative.CHARFORMAT2 cf = RichEditNative.GetCharFormat(box);
                bool Has(uint f) => (cf.dwMask & f) != 0 && (cf.dwEffects & f) != 0;
                _boldBtn.Checked = Has(RichEditNative.CFM_BOLD);
                _italicBtn.Checked = Has(RichEditNative.CFM_ITALIC);
                _underlineBtn.Checked = Has(RichEditNative.CFM_UNDERLINE);
                _strikeBtn.Checked = Has(RichEditNative.CFM_STRIKEOUT);

                (string face, float size) = RichEditNative.GetFont(box);
                if (!_fontCombo.Focused)
                {
                    _fontCombo.Text = face ?? "";
                }

                if (!_sizeCombo.Focused)
                {
                    _sizeCombo.Text = size > 0 ? size.ToString("0.#", CultureInfo.InvariantCulture) : "";
                }

                short numbering = RichEditNative.GetNumbering(box);
                _bulletBtn.Checked = numbering == RichEditNative.PFN_BULLET;
                _numberBtn.Checked = numbering == RichEditNative.PFN_ARABIC;
                HorizontalAlignment align = box.SelectionAlignment;
                _alignLeftBtn.Checked = align == HorizontalAlignment.Left;
                _alignCenterBtn.Checked = align == HorizontalAlignment.Center;
                _alignRightBtn.Checked = align == HorizontalAlignment.Right;

                int style = size >= RtfConverter.H1Size - 0.5f ? 1
                    : size >= RtfConverter.H2Size - 0.5f ? 2
                    : size >= RtfConverter.H3Size - 0.5f && _boldBtn.Checked ? 3
                    : face == "Consolas" && !(CurrentNote?.CodeMode ?? false) ? 5
                    : 0;
                _headingCombo.SelectedIndex = style;
            }
            finally
            {
                _updatingToolbar = false;
            }
        }

        #endregion

        #region Insert

        private void InsertText(string text) => WithBox(b => b.SelectedText = text);

        private void InsertLine()
        {
            WithBox(box =>
            {
                string line = new string('─', 48);
                int start = box.SelectionStart;
                bool needBreak = start > 0 && box.Text[start - 1] != '\n';
                box.SelectedText = (needBreak ? "\n" : "") + line + "\n";
                int lineStart = start + (needBreak ? 1 : 0);
                box.Select(lineStart, line.Length);
                RichEditNative.SetTextColor(box, Color.Silver);
                box.Select(lineStart + line.Length + 1, 0);
                RichEditNative.SetTextColor(box, null);
            });
        }

        private void InsertImageFile()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files (*.*)|*.*",
                Multiselect = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            foreach (string file in dialog.FileNames)
            {
                InsertImageFromFile(file);
            }
        }

        private void InsertImageFromFile(string file)
        {
            try
            {
                using var img = Image.FromFile(file);
                InsertImage(img);
            }
            catch (Exception ex)
            {
                lblSaved.Text = "Image could not be loaded: " + ex.Message;
            }
        }

        /// <summary>
        /// Inserts an image at the caret (large images are scaled down). The clipboard is restored afterwards.
        /// </summary>
        private void InsertImage(Image image)
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null || image == null)
            {
                return;
            }

            const int maxWidth = 900;
            using var bmp = image.Width > maxWidth
                ? new Bitmap(image, maxWidth, Math.Max(1, image.Height * maxWidth / image.Width))
                : new Bitmap(image);

            DataObject backup = null;
            try
            {
                IDataObject current = Clipboard.GetDataObject();
                if (current != null)
                {
                    backup = new DataObject();
                    foreach (string format in current.GetFormats(false))
                    {
                        try
                        {
                            object data = current.GetData(format, false);
                            if (data != null)
                            {
                                backup.SetData(format, data);
                            }
                        }
                        catch
                        {
                            // some formats can not be copied
                        }
                    }
                }
            }
            catch
            {
                backup = null;
            }

            try
            {
                Clipboard.SetImage(bmp);
                box.Paste(DataFormats.GetFormat(DataFormats.Bitmap));
            }
            finally
            {
                try
                {
                    if (backup != null && backup.GetFormats().Length > 0)
                    {
                        Clipboard.SetDataObject(backup, true);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            box.Focus();
        }

        private void InsertUoGraphic(int id)
        {
            using var dialog = new UoGraphicDialog(SelectedItemProvider, SelectedGumpProvider);
            if (id >= 0)
            {
                dialog.SetId(id);
            }

            if (dialog.ShowDialog(this) != DialogResult.OK || dialog.ResultImage == null)
            {
                return;
            }

            using (dialog.ResultImage)
            {
                InsertImage(dialog.ResultImage);
            }

            if (dialog.ResultCaption.Length > 0)
            {
                WithBox(box =>
                {
                    int start = box.SelectionStart;
                    box.SelectedText = "\n" + dialog.ResultCaption + "\n";
                    box.Select(start + 1, dialog.ResultCaption.Length);
                    RichEditNative.SetTextColor(box, Color.Gray);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_ITALIC, true);
                    box.Select(start + dialog.ResultCaption.Length + 2, 0);
                    RichEditNative.SetTextColor(box, null);
                    RichEditNative.SetEffect(box, RichEditNative.CFM_ITALIC, false);
                });
            }
        }

        private void ShowHexDec()
        {
            using var dialog = new HexDecDialog(CurrentBox?.SelectedText.Trim());
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.Result != null)
            {
                InsertText(dialog.Result);
            }
        }

        private void ConvertSelectionHexDec()
        {
            WithBox(box =>
            {
                if (box.SelectionLength == 0)
                {
                    if (box.IdAt(box.SelectionStart, out int s, out int len) < 0 &&
                        box.IdAt(Math.Max(0, box.SelectionStart - 1), out s, out len) < 0)
                    {
                        // select the number at the caret
                        Match m = Regex.Matches(box.Text, @"\d+").FirstOrDefault(x => x.Index <= box.SelectionStart && x.Index + x.Length >= box.SelectionStart);
                        if (m == null)
                        {
                            return;
                        }

                        s = m.Index;
                        len = m.Length;
                    }

                    box.Select(s, len);
                }

                string text = box.SelectedText.Trim();
                if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                    long.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long hex))
                {
                    box.SelectedText = hex.ToString(CultureInfo.InvariantCulture);
                }
                else if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long dec) && dec >= 0)
                {
                    box.SelectedText = "0x" + dec.ToString("X4", CultureInfo.InvariantCulture);
                }
            });
        }

        #endregion

        #region Editor events: ids, context menu, drag & drop

        private void OnHoverId(int id, Point screen)
        {
            if (id < 0 || !ContainsFocus)
            {
                _idPopup.Hide();
                return;
            }

            _idPopup.ShowId(id, screen);
        }

        private void OnEditorClick(NoteRichTextBox box, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || (ModifierKeys & Keys.Control) == 0)
            {
                return;
            }

            int id = box.IdAt(box.GetCharIndexFromPosition(e.Location), out _, out _);
            if (id < 0)
            {
                return;
            }

            if (ShowItemInFiddler != null)
            {
                ShowItemInFiddler(id);
                lblSaved.Text = $"0x{id:X4} shown in the Items tab";
            }
            else
            {
                Clipboard.SetText($"0x{id:X4}");
                lblSaved.Text = $"0x{id:X4} copied";
            }
        }

        private void EditorMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NoteRichTextBox box = CurrentBox;
            _editorMenu.Items.Clear();
            if (box == null)
            {
                e.Cancel = true;
                return;
            }

            int index = box.SelectionLength > 0 ? box.SelectionStart : _contextIndex;
            int id = box.IdAt(index, out int idStart, out int idLength);
            if (id >= 0)
            {
                string name = UoGraphics.ItemName(id);
                _editorMenu.Items.Add(new ToolStripMenuItem($"0x{id:X4} ({id})" + (name.Length > 0 ? " – " + name : "")) { Enabled = false });
                if (ShowItemInFiddler != null)
                {
                    _editorMenu.Items.Add(Item("Show in UoFiddler Items tab", (_, _) => ShowItemInFiddler(id), Keys.None, "Ctrl+Click"));
                }

                _editorMenu.Items.Add(Item("Insert image of this item …", (_, _) => InsertUoGraphic(id)));
                _editorMenu.Items.Add(Item("Copy id", (_, _) => Clipboard.SetText($"0x{id:X4}")));
                _editorMenu.Items.Add(Item("Convert to decimal", (_, _) =>
                {
                    box.Select(idStart, idLength);
                    box.SelectedText = id.ToString(CultureInfo.InvariantCulture);
                }));
                _editorMenu.Items.Add(new ToolStripSeparator());
            }

            bool hasSel = box.SelectionLength > 0;
            _editorMenu.Items.Add(new ToolStripMenuItem("Cut", null, (_, _) => box.Cut()) { Enabled = hasSel, ShortcutKeyDisplayString = "Ctrl+X" });
            _editorMenu.Items.Add(new ToolStripMenuItem("Copy", null, (_, _) => box.Copy()) { Enabled = hasSel, ShortcutKeyDisplayString = "Ctrl+C" });
            _editorMenu.Items.Add(Item("Paste", (_, _) => box.Paste(), Keys.None, "Ctrl+V"));
            _editorMenu.Items.Add(Item("Paste as plain text", (_, _) => box.PastePlainText(), Keys.None, "Ctrl+Shift+V"));
            _editorMenu.Items.Add(new ToolStripSeparator());
            _editorMenu.Items.Add(Item("Bold", (_, _) => ToggleEffect(RichEditNative.CFM_BOLD), Keys.None, "Ctrl+B"));
            _editorMenu.Items.Add(Item("Highlight", (_, _) => WithBox(b => RichEditNative.SetBackColor(b, _highlightColors[0].Color))));
            _editorMenu.Items.Add(Item("Checklist", (_, _) => ToggleChecklist(), Keys.None, "Ctrl+Shift+L"));
            _editorMenu.Items.Add(Item("Clear formatting", (_, _) => ClearFormatting(), Keys.None, "Ctrl+Space"));
            _editorMenu.Items.Add(new ToolStripSeparator());
            _editorMenu.Items.Add(Item("Insert date and time", (_, _) => InsertText(DateTime.Now.ToString("g", CultureInfo.CurrentCulture)), Keys.None, "F5"));
            _editorMenu.Items.Add(Item("Select all", (_, _) => box.SelectAll(), Keys.None, "Ctrl+A"));
        }

        private static readonly string[] _imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
        private static readonly string[] _importExtensions = { ".txt", ".rtf", ".md", ".markdown", ".htm", ".html", ".cs", ".xml", ".json", ".log", ".cfg", ".ini" };

        private void Editor_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data?.GetDataPresent(DataFormats.FileDrop) == true || e.Data?.GetDataPresent(DataFormats.UnicodeText) == true
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void Editor_DragDrop(object sender, DragEventArgs e)
        {
            var box = (NoteRichTextBox)sender;
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                var imports = new List<string>();
                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    if (_imageExtensions.Contains(ext))
                    {
                        InsertImageFromFile(file);
                    }
                    else if (_importExtensions.Contains(ext))
                    {
                        imports.Add(file);
                    }
                }

                if (imports.Count > 0)
                {
                    ImportFiles(imports);
                }

                return;
            }

            if (e.Data?.GetData(DataFormats.UnicodeText) is string text)
            {
                box.Select(box.GetCharIndexFromPosition(box.PointToClient(new Point(e.X, e.Y))), 0);
                box.SelectedText = text;
            }
        }

        #endregion

        #region Find / replace

        private RichTextBoxFinds FindOptions =>
            (chkMatchCase.Checked ? RichTextBoxFinds.MatchCase : RichTextBoxFinds.None) |
            (chkWholeWord.Checked ? RichTextBoxFinds.WholeWord : RichTextBoxFinds.None);

        private void ShowFind(bool replace)
        {
            NoteRichTextBox box = CurrentBox;
            panelFind.Visible = true;
            if (box != null && box.SelectionLength is > 0 and < 100 && !box.SelectedText.Contains('\n'))
            {
                txtFind.Text = box.SelectedText;
            }

            if (replace && txtFind.TextLength > 0)
            {
                txtReplace.Focus();
                txtReplace.SelectAll();
            }
            else
            {
                txtFind.Focus();
                txtFind.SelectAll();
            }

            UpdateFindCount();
        }

        private void BtnFindClose_Click(object sender, EventArgs e)
        {
            panelFind.Visible = false;
            CurrentBox?.Focus();
        }

        private void BtnFindNext_Click(object sender, EventArgs e) => FindNext(true);

        private void BtnFindPrev_Click(object sender, EventArgs e) => FindNext(false);

        private void FindOptions_Changed(object sender, EventArgs e) => UpdateFindCount();

        private void TxtFind_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender == txtReplace)
                {
                    BtnReplace_Click(sender, e);
                }
                else
                {
                    FindNext(!e.Shift);
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                BtnFindClose_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private bool FindNext(bool forward)
        {
            NoteRichTextBox box = CurrentBox;
            string text = txtFind.Text;
            if (box == null || text.Length == 0)
            {
                if (!panelFind.Visible)
                {
                    ShowFind(false);
                }

                return false;
            }

            int index;
            if (forward)
            {
                int start = Math.Min(box.SelectionStart + box.SelectionLength, box.TextLength);
                index = start < box.TextLength ? box.Find(text, start, -1, FindOptions) : -1;
                if (index < 0)
                {
                    index = box.Find(text, 0, -1, FindOptions);
                }
            }
            else
            {
                int end = box.SelectionStart;
                index = end > 0 ? box.Find(text, 0, end, FindOptions | RichTextBoxFinds.Reverse) : -1;
                if (index < 0)
                {
                    index = box.Find(text, 0, -1, FindOptions | RichTextBoxFinds.Reverse);
                }
            }

            if (index >= 0)
            {
                box.ScrollToCaret();
            }
            else
            {
                SystemSounds.Beep.Play();
            }

            UpdateFindCount();
            return index >= 0;
        }

        /// <summary>
        /// All match positions of the find text in the current note.
        /// </summary>
        private List<int> FindAll()
        {
            var result = new List<int>();
            NoteRichTextBox box = CurrentBox;
            string find = txtFind.Text;
            if (box == null || find.Length == 0)
            {
                return result;
            }

            string text = box.Text;
            StringComparison cmp = chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int i = 0;
            while ((i = text.IndexOf(find, i, cmp)) >= 0)
            {
                bool ok = !chkWholeWord.Checked ||
                          ((i == 0 || !char.IsLetterOrDigit(text[i - 1])) &&
                           (i + find.Length >= text.Length || !char.IsLetterOrDigit(text[i + find.Length])));
                if (ok)
                {
                    result.Add(i);
                }

                i += Math.Max(1, find.Length);
            }

            return result;
        }

        private void UpdateFindCount()
        {
            if (!panelFind.Visible)
            {
                return;
            }

            List<int> all = FindAll();
            if (txtFind.TextLength == 0)
            {
                lblFindCount.Text = "";
                txtFind.BackColor = SystemColors.Window;
                return;
            }

            int current = CurrentBox == null ? -1 : all.IndexOf(CurrentBox.SelectionStart);
            lblFindCount.Text = all.Count == 0 ? "No matches" : current >= 0 ? $"{current + 1} of {all.Count}" : $"{all.Count} matches";
            txtFind.BackColor = all.Count == 0 ? Color.FromArgb(255, 220, 220) : SystemColors.Window;
        }

        private void BtnReplace_Click(object sender, EventArgs e)
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null || txtFind.TextLength == 0)
            {
                return;
            }

            StringComparison cmp = chkMatchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (box.SelectionLength > 0 && string.Equals(box.SelectedText, txtFind.Text, cmp))
            {
                box.SelectedText = txtReplace.Text;
            }

            FindNext(true);
        }

        private void BtnReplaceAll_Click(object sender, EventArgs e)
        {
            NoteRichTextBox box = CurrentBox;
            string find = txtFind.Text;
            if (box == null || find.Length == 0)
            {
                return;
            }

            List<int> all = FindAll();
            if (all.Count == 0)
            {
                SystemSounds.Beep.Play();
                return;
            }

            // replace from the end so the positions stay valid
            foreach (int i in Enumerable.Reverse(all))
            {
                box.Select(i, find.Length);
                box.SelectedText = txtReplace.Text;
            }

            lblFindCount.Text = $"{all.Count} replaced";
            lblSaved.Text = $"{all.Count} replacements";
        }

        #endregion

        #region View

        private void ToggleList()
        {
            Settings.ShowList = !Settings.ShowList;
            splitMain.Panel1Collapsed = !Settings.ShowList;
            _showListItem.Checked = Settings.ShowList;
        }

        private void ToggleWordWrap()
        {
            Settings.WordWrap = !Settings.WordWrap;
            _wordWrapItem.Checked = Settings.WordWrap;
            foreach (NoteEditor editor in Editors.Where(ed => !ed.Note.CodeMode))
            {
                editor.Box.WordWrap = Settings.WordWrap;
            }
        }

        private void ToggleLineNumbers()
        {
            Settings.LineNumbers = !Settings.LineNumbers;
            _lineNumbersItem.Checked = Settings.LineNumbers;
            foreach (NoteEditor editor in Editors)
            {
                editor.Gutter.Visible = Settings.LineNumbers;
            }
        }

        private void Zoom(float delta)
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                return;
            }

            float zoom = delta == 0 ? 1f : Math.Clamp((float)Math.Round(box.ZoomFactor + delta, 1), 0.5f, 4f);
            box.ZoomFactor = zoom;
            Settings.Zoom = zoom;
            UpdateZoomLabel();
        }

        private void LblZoom_Click(object sender, EventArgs e) => Zoom(0);

        private void UpdateZoomLabel()
        {
            NoteRichTextBox box = CurrentBox;
            lblZoom.Text = box == null ? "" : $"{Math.Round(box.ZoomFactor * 100)} %";
        }

        private void UpdatePosition()
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                lblPosition.Text = "";
                return;
            }

            string text = box.Text;
            int index = Math.Min(box.SelectionStart, text.Length);
            int line = 1;
            for (int i = 0; i < index; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                }
            }

            int lineStart = index > 0 ? text.LastIndexOf('\n', index - 1) + 1 : 0;
            lblPosition.Text = $"Ln {line}, Col {index - lineStart + 1}" + (box.SelectionLength > 0 ? $"  ({box.SelectionLength} selected)" : "");
            if (panelFind.Visible)
            {
                UpdateFindCount();
            }
        }

        private void UpdateStats()
        {
            NoteRichTextBox box = CurrentBox;
            if (box == null)
            {
                lblStats.Text = "";
                return;
            }

            string text = box.Text;
            int words = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
            int lines = text.Count(c => c == '\n') + 1;
            lblStats.Text = $"{words} words  ·  {text.Length} chars  ·  {lines} lines";
            UpdateZoomLabel();
        }

        #endregion

        #region Keyboard

        private void NotepadForm_KeyDown(object sender, KeyEventArgs e)
        {
            NoteRichTextBox box = CurrentBox;
            bool inEditor = box != null && box.Focused;

            if (e.KeyCode == Keys.F3)
            {
                FindNext(!e.Shift);
                e.Handled = e.SuppressKeyPress = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                if (panelFind.Visible)
                {
                    BtnFindClose_Click(sender, e);
                    e.Handled = e.SuppressKeyPress = true;
                }

                _idPopup.Hide();
                return;
            }

            if (!inEditor || !e.Control)
            {
                return;
            }

            bool handled = true;
            switch (e.KeyCode)
            {
                case Keys.B when !e.Shift: ToggleEffect(RichEditNative.CFM_BOLD); break;
                case Keys.I when !e.Shift: ToggleEffect(RichEditNative.CFM_ITALIC); break;
                case Keys.U when !e.Shift: ToggleEffect(RichEditNative.CFM_UNDERLINE); break;
                case Keys.X when e.Shift: ToggleEffect(RichEditNative.CFM_STRIKEOUT); break;
                case Keys.L when e.Shift: ToggleChecklist(); break;
                case Keys.D0 or Keys.NumPad0: ApplyStyle(0); break;
                case Keys.D1 or Keys.NumPad1: ApplyStyle(1); break;
                case Keys.D2 or Keys.NumPad2: ApplyStyle(2); break;
                case Keys.D3 or Keys.NumPad3: ApplyStyle(3); break;
                case Keys.Space: ClearFormatting(); break;
                default: handled = false; break;
            }

            if (handled)
            {
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region Import / export / print

        private static string SafeFileName(string title)
        {
            string name = string.IsNullOrWhiteSpace(title) ? "Note" : title.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Length > 80 ? name.Substring(0, 80) : name;
        }

        private void ImportFiles()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Notes (*.txt;*.rtf;*.md;*.html)|*.txt;*.rtf;*.md;*.markdown;*.htm;*.html|Scripts and text (*.cs;*.xml;*.json;*.log;*.cfg;*.ini)|*.cs;*.xml;*.json;*.log;*.cfg;*.ini|All files (*.*)|*.*",
                Multiselect = true,
                Title = "Import files as notes"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                ImportFiles(dialog.FileNames);
            }
        }

        private void ImportFiles(IEnumerable<string> files)
        {
            Note last = null;
            int count = 0;
            using var helper = new RichTextBox();
            foreach (string file in files)
            {
                try
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();
                    string content = File.ReadAllText(file);
                    var note = new Note { Title = Path.GetFileNameWithoutExtension(file), Category = "Imported" };

                    switch (ext)
                    {
                        case ".rtf":
                            helper.Rtf = content;
                            note.Rtf = content;
                            note.PlainText = helper.Text;
                            break;
                        case ".md":
                        case ".markdown":
                            note.Rtf = RtfConverter.MarkdownToRtf(content, Settings.DefaultFont, Settings.DefaultSize);
                            helper.Rtf = note.Rtf;
                            note.PlainText = helper.Text;
                            break;
                        case ".htm":
                        case ".html":
                            string plain = Regex.Replace(content, @"<(script|style)[\s\S]*?</\1>", "", RegexOptions.IgnoreCase);
                            plain = Regex.Replace(plain, @"<br\s*/?>|</p>|</div>|</li>|</h\d>", "\n", RegexOptions.IgnoreCase);
                            plain = System.Net.WebUtility.HtmlDecode(Regex.Replace(plain, "<[^>]+>", ""));
                            note.PlainText = Regex.Replace(plain, @"\n{3,}", "\n\n").Trim();
                            break;
                        case ".cs":
                            note.PlainText = content.Replace("\r\n", "\n");
                            note.CodeMode = true;
                            note.Category = "Scripts";
                            break;
                        default:
                            note.PlainText = content.Replace("\r\n", "\n");
                            break;
                    }

                    _data.Notes.Add(note);
                    last = note;
                    count++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Could not import {file}:\n{ex.Message}", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            if (count == 0)
            {
                return;
            }

            TrySave();
            RefreshCategories();
            RefreshList();
            OpenNote(last);
            lblSaved.Text = $"{count} file(s) imported";
        }

        private void Export(string kind)
        {
            NoteEditor editor = CurrentEditor;
            if (editor == null)
            {
                return;
            }

            SaveAll();
            Note note = editor.Note;
            string rtf = editor.Box.Rtf;

            (string filter, string ext) = kind switch
            {
                "pdf" => ("PDF (*.pdf)|*.pdf", ".pdf"),
                "html" => ("Web page (*.html)|*.html", ".html"),
                "md" => ("Markdown (*.md)|*.md", ".md"),
                "rtf" => ("Rich text (*.rtf)|*.rtf", ".rtf"),
                _ => ("Text (*.txt)|*.txt", ".txt")
            };

            using var dialog = new SaveFileDialog { Filter = filter, FileName = SafeFileName(note.Title) + ext, Title = "Export note" };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string path = dialog.FileName;
            try
            {
                UseWaitCursor = true;
                switch (kind)
                {
                    case "pdf":
                        ExportPdf(note, rtf, path);
                        return;
                    case "html":
                        File.WriteAllText(path, RtfConverter.ToHtml(new Note
                        {
                            Title = note.Title, Category = note.Category, Tags = note.Tags, Modified = note.Modified,
                            Rtf = rtf, PlainText = editor.Box.Text, CodeMode = note.CodeMode
                        }), Encoding.UTF8);
                        break;
                    case "md":
                        File.WriteAllText(path, RtfConverter.ToMarkdown(note), Encoding.UTF8);
                        break;
                    case "rtf":
                        File.WriteAllText(path, rtf, Encoding.ASCII);
                        break;
                    default:
                        File.WriteAllText(path, editor.Box.Text.Replace("\n", Environment.NewLine), Encoding.UTF8);
                        break;
                }

                lblSaved.Text = "Exported: " + path;
                if (kind == "html" && AskOpen(path))
                {
                    OpenFolder(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Export failed: " + ex.Message, "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private bool AskOpen(string path) =>
            MessageBox.Show(this, $"Saved:\n{path}\n\nOpen it now?", "Notepad", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;

        private async void ExportPdf(Note note, string rtf, string path)
        {
            bool selectableText = false;
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                if (NotePrinter.HasPdfPrinter())
                {
                    try
                    {
                        using var printer = new NotePrinter(note, rtf, Settings.PrintHeader);
                        printer.ExportPdfWithPrinter(path, _pageSettings);
                        selectableText = true;
                    }
                    catch
                    {
                        selectableText = false;
                    }
                }

                if (!selectableText)
                {
                    using var printer = new NotePrinter(note, rtf, Settings.PrintHeader);
                    printer.ExportPdfAsImages(path, _pageSettings?.Landscape ?? false, _pageSettings?.Margins);
                }

                // the Windows PDF printer writes the file in the background
                for (int i = 0; i < 50 && (!File.Exists(path) || new FileInfo(path).Length == 0); i++)
                {
                    await Task.Delay(200);
                }

                UseWaitCursor = false;
                lblSaved.Text = $"PDF saved ({(selectableText ? "selectable text" : "image PDF")}): {path}";
                if (File.Exists(path) && AskOpen(path))
                {
                    OpenFolder(path);
                }
            }
            catch (Exception ex)
            {
                UseWaitCursor = false;
                MessageBox.Show(this, "PDF export failed: " + ex.Message, "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportBackup()
        {
            SaveAll();
            using var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = $"notes_backup_{DateTime.Now:yyyyMMdd}.json" };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.Copy(NoteStore.FilePath, dialog.FileName, true);
                    lblSaved.Text = "Backup saved: " + dialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Backup failed: " + ex.Message, "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ApplyPageSettings(PrintDocument doc)
        {
            if (_pageSettings != null)
            {
                doc.DefaultPageSettings = (PageSettings)_pageSettings.Clone();
            }
        }

        private NotePrinter CreatePrinter()
        {
            NoteEditor editor = CurrentEditor;
            if (editor == null)
            {
                return null;
            }

            SaveAll();
            var printer = new NotePrinter(editor.Note, editor.Box.Rtf, Settings.PrintHeader);
            ApplyPageSettings(printer.Document);
            return printer;
        }

        private void Print()
        {
            try
            {
                using NotePrinter printer = CreatePrinter();
                if (printer == null)
                {
                    return;
                }

                NoteRichTextBox box = CurrentBox;
                using var dialog = new PrintDialog { Document = printer.Document, AllowSelection = box.SelectionLength > 0, UseEXDialog = true };
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (dialog.PrinterSettings.PrintRange == PrintRange.Selection)
                {
                    printer.SetRange(box.SelectionStart, box.SelectionLength);
                }

                printer.Document.Print();
                lblSaved.Text = "Sent to the printer";
            }
            catch (InvalidPrinterException)
            {
                MessageBox.Show(this, "No printer is installed. Use File > Export note > PDF instead.", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Printing failed: " + ex.Message, "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintPreview()
        {
            try
            {
                using NotePrinter printer = CreatePrinter();
                if (printer == null)
                {
                    return;
                }

                using var preview = new PrintPreviewDialog
                {
                    Document = printer.Document,
                    Width = 1000,
                    Height = 800,
                    UseAntiAlias = true,
                    StartPosition = FormStartPosition.CenterParent
                };
                preview.ShowDialog(this);
            }
            catch (InvalidPrinterException)
            {
                MessageBox.Show(this, "No printer is installed. Use File > Export note > PDF instead.", "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Print preview failed: " + ex.Message, "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PageSetup()
        {
            try
            {
                _pageSettings ??= new PageSettings();
                using var dialog = new PageSetupDialog { PageSettings = _pageSettings, EnableMetric = true };
                dialog.ShowDialog(this);
            }
            catch (InvalidPrinterException)
            {
                MessageBox.Show(this, "No printer is installed, page setup is not available. The PDF export uses A4.", "Notepad",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void OpenFolder(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch
            {
                // ignored
            }
        }

        private void ShowHelp()
        {
            MessageBox.Show(this,
                "NOTES\r\n" +
                "Ctrl+N new note · Ctrl+W close tab · Ctrl+S save (autosave is on) · F9 show/hide list\r\n" +
                "Search all notes: Ctrl+Shift+F – several words = all must match (title, text, tags, category).\r\n" +
                "Right click a note: sticky note, pin, duplicate, delete. Middle click a tab closes it.\r\n\r\n" +
                "TEXT\r\n" +
                "Ctrl+B / I / U bold, italic, underline · Ctrl+Shift+X strikethrough\r\n" +
                "Ctrl+1 / 2 / 3 headings · Ctrl+0 normal · Ctrl+Space clear formatting\r\n" +
                "Ctrl+Shift+L checklist (click a box to tick it) · Ctrl+Shift+V paste as plain text\r\n" +
                "Ctrl+F find · Ctrl+H replace · F3 / Shift+F3 next / previous · F5 date and time\r\n" +
                "Ctrl + mouse wheel zoom · images: paste, drop or Insert > Image\r\n\r\n" +
                "UO\r\n" +
                "Hover an id like 0x1A2B to see the item. Ctrl+Click shows it in the Items tab.\r\n" +
                "Insert > UO graphic adds an item or gump picture. Ctrl+Shift+H converts hex ⇄ dec.\r\n" +
                "Code notes color C# (ServUO scripts).\r\n\r\n" +
                "Data: " + NoteStore.FilePath,
                "Notepad – Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion
    }
}

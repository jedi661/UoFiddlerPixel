/***************************************************************************
 *
 * $Author: Turley
 * Advanced: Nikodemus
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using System.Drawing;


namespace UoFiddler.Controls.UserControls
{
    public partial class ClilocControl : UserControl
    {
        public ClilocControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _source = new BindingSource();
            FindEntry.TextBox.PreviewKeyDown += FindEntry_PreviewKeyDown;

            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
        }

        private const string _searchNumberPlaceholder = "Enter Number";
        private const string _searchTextPlaceholder = "Enter Text";

        private static StringList _cliloc;
        private static BindingSource _source;
        private int _lang;
        private SortOrder _sortOrder;
        private int _sortColumn;
        private bool _loaded;

        #region int Lang
        /// <summary>
        /// Sets Language and loads cliloc
        /// </summary>
        private int Lang
        {
            get => _lang;
            set
            {
                _lang = value;
                switch (value)
                {
                    case 0:
                        _cliloc = new StringList("enu", Options.NewClilocFormat);
                        break;
                    case 1:
                        _cliloc = new StringList("deu", Options.NewClilocFormat);
                        break;
                    case 2:
                        TestCustomLang("cliloc.custom1");
                        _cliloc = new StringList("custom1", Options.NewClilocFormat);
                        break;
                    case 3:
                        TestCustomLang("cliloc.custom2");
                        _cliloc = new StringList("custom2", Options.NewClilocFormat);
                        break;
                }
            }
        }
        #endregion

        #region Reload
        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            OnLoad(this, EventArgs.Empty);
        }
        #endregion

        #region  OnLoad
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _sortOrder = SortOrder.Ascending;
            _sortColumn = 0;
            LangComboBox.SelectedIndex = 0;
            Lang = 0;
            _cliloc.Entries.Sort(new StringList.NumberComparer(false));
            _source.DataSource = _cliloc.Entries;
            dataGridView1.DataSource = _source;
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].Width = 60;
                dataGridView1.Columns[2].ReadOnly = true;
            }
            dataGridView1.Invalidate();
            LangComboBox.Items[2] = Files.GetFilePath("cliloc.custom1") != null
                ? $"Custom 1 ({Path.GetExtension(Files.GetFilePath("cliloc.custom1"))})"
                : "Custom 1";

            LangComboBox.Items[3] = Files.GetFilePath("cliloc.custom2") != null
                ? $"Custom 2 ({Path.GetExtension(Files.GetFilePath("cliloc.custom2"))})"
                : "Custom 2";

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region TestCustomLang
        private void TestCustomLang(string what)
        {
            if (Files.GetFilePath(what) != null)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose Cliloc file to open";
                dialog.CheckFileExists = true;
                dialog.Filter = "cliloc files (cliloc.*)|cliloc.*";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Files.SetMulPath(dialog.FileName, what);
                LangComboBox.BeginUpdate();
                if (what == "cliloc.custom1")
                {
                    LangComboBox.Items[2] = $"Custom 1 ({Path.GetExtension(dialog.FileName)})";
                }
                else
                {
                    LangComboBox.Items[3] = $"Custom 2 ({Path.GetExtension(dialog.FileName)})";
                }

                LangComboBox.EndUpdate();
            }
        }
        #endregion

        #region OnLangChange
        private void OnLangChange(object sender, EventArgs e)
        {
            if (LangComboBox.SelectedIndex == Lang)
            {
                return;
            }

            Lang = LangComboBox.SelectedIndex;
            _sortOrder = SortOrder.Ascending;
            _sortColumn = 0;
            _cliloc.Entries.Sort(new StringList.NumberComparer(false));
            _source.DataSource = _cliloc.Entries;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].Width = 60;
                dataGridView1.Columns[2].ReadOnly = true;
            }

            dataGridView1.Invalidate();
        }
        #endregion

        #region GotoNr
        private void GotoNr(object sender, EventArgs e)
        {
            // Reset the current search word to remove highlighting
            currentSearchWord = string.Empty;
            dataGridView1.Refresh();

            if (int.TryParse(GotoEntry.Text, NumberStyles.Integer, null, out int nr))
            {
                for (int i = 0; i < dataGridView1.Rows.Count; ++i)
                {
                    if ((int)dataGridView1.Rows[i].Cells[0].Value != nr)
                    {
                        continue;
                    }

                    // Set the current search word to the value you are looking for
                    currentSearchWord = nr.ToString();
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    dataGridView1.Refresh();
                    return;
                }
            }

            MessageBox.Show(
                "Number not found.",
                "Goto",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region  FindEntryClick        

        private string currentSearchWord;

        /*private void FindEntryClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FindEntry.Text) || FindEntry.Text == _searchTextPlaceholder)
            {
                MessageBox.Show("Please provide search text", "Find Entry", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                return;
            }

            var searchMethod = SearchHelper.GetSearchMethod(RegexToolStripButton.Checked);

            bool hasErrors = false;

            // Enter the current search word
            currentSearchWord = FindEntry.Text.ToLower();

            for (int i = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected) + 1; i < dataGridView1.Rows.Count; ++i)
            {
                var cellText = dataGridView1.Rows[i].Cells[1].Value.ToString().ToLower();

                var searchResult = searchMethod(currentSearchWord, cellText);
                if (searchResult.HasErrors)
                {
                    hasErrors = true;
                    break;
                }

                if (searchResult.EntryFound)
                {
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }

            // Refresh the DataGridView to update the highlighting
            dataGridView1.Refresh();

            MessageBox.Show(hasErrors ? "Invalid regular expression." : "Entry not found.", "Find Entry",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }*/

        private void FindEntryClick(object sender, EventArgs e)
        {
            // Eingabevalidierung
            if (string.IsNullOrEmpty(FindEntry.Text) || FindEntry.Text == _searchTextPlaceholder)
            {
                MessageBox.Show("Please provide search text", "Find Entry", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
                return;
            }

            // Den aktuellen Suchtext speichern
            currentSearchWord = FindEntry.Text.ToLower(); // Kleinbuchstaben wie in der ersten Methode

            // W�hle die Suchmethode aus
            var searchMethod = SearchHelper.GetSearchMethod(RegexToolStripButton.Checked);
            bool hasErrors = false;

            // Schleife �ber die Datens�tze
            for (int i = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected) + 1; i < dataGridView1.Rows.Count; ++i)
            {
                var cellText = dataGridView1.Rows[i].Cells[1].Value.ToString().ToLower(); // Kleinbuchstabenvergleich
                var searchResult = searchMethod(currentSearchWord, cellText);

                if (searchResult.HasErrors)
                {
                    hasErrors = true;
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue; // Schleife fortsetzen
                }

                // Eintrag hervorheben und scrollen
                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                return;
            }

            // Ansicht aktualisieren (falls n�tig) und Feedback geben
            dataGridView1.Refresh(); // Option aus der ersten Methode
            MessageBox.Show(hasErrors ? "Invalid regular expression." : "Entry not found.", "Find Entry",
                MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        #endregion

        #region dataGridView1_CellFormatting
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var cell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.Value is string cellText && !string.IsNullOrEmpty(currentSearchWord) && cellText.ToLower().Contains(currentSearchWord))
                {
                    e.CellStyle.BackColor = Color.Yellow;
                }
                else
                {
                    e.CellStyle.BackColor = Color.White;
                }
            }
        }
        #endregion

        #region OnClickSave
        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();

            string path = Options.OutputPath;
            string fileName;

            if (_cliloc.Language == "custom1")
            {
                fileName = Path.Combine(path, $"Cliloc{Path.GetExtension(Files.GetFilePath("cliloc.custom1"))}");
            }
            else
            {
                fileName = _cliloc.Language == "custom2"
                    ? Path.Combine(path, $"Cliloc{Path.GetExtension(Files.GetFilePath("cliloc.custom2"))}")
                    : Path.Combine(path, $"Cliloc.{_cliloc.Language}");
            }

            _cliloc.SaveStringList(fileName);
            dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            _sortColumn = 0;
            _sortOrder = SortOrder.Ascending;
            dataGridView1.Invalidate();
            MessageBox.Show(
                $"CliLoc saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["CliLoc"] = false;
        }
        #endregion

        #region OnCell_dbClick
        private void OnCell_dbClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int cellNr = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            string cellText = (string)dataGridView1.Rows[e.RowIndex].Cells[1].Value;

            new ClilocDetailForm(cellNr, cellText, SaveEntry).Show();
        }
        #endregion

        #region OnClick_AddEntry
        private void OnClick_AddEntry(object sender, EventArgs e)
        {
            new ClilocAddForm(IsNumberFree, AddEntry).Show();
        }
        #endregion

        #region OnClick_DeleteEntry
        private void OnClick_DeleteEntry(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count <= 0)
            {
                return;
            }

            _cliloc.Entries.RemoveAt(dataGridView1.SelectedCells[0].OwningRow.Index);
            dataGridView1.Invalidate();
            Options.ChangedUltimaClass["CliLoc"] = true;
        }
        #endregion

        #region OnHeaderClicked
        private void OnHeaderClicked(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (_sortColumn == e.ColumnIndex)
            {
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
                dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = _sortOrder;
            _sortColumn = e.ColumnIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    _cliloc.Entries.Sort(new StringList.NumberComparer(_sortOrder == SortOrder.Descending));
                    break;
                case 1:
                    _cliloc.Entries.Sort(new StringList.TextComparer(_sortOrder == SortOrder.Descending));
                    break;
                default:
                    _cliloc.Entries.Sort(new StringList.FlagComparer(_sortOrder == SortOrder.Descending));
                    break;
            }

            dataGridView1.Invalidate();
        }
        #endregion

        #region OnCLick_CopyClilocNumber
        private void OnCLick_CopyClilocNumber(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                Clipboard.SetDataObject(
                    ((int)dataGridView1.SelectedCells[0].OwningRow.Cells[0].Value).ToString(), true);
            }
        }
        #endregion

        #region OnCLick_CopyClilocText
        private void OnCLick_CopyClilocText(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                Clipboard.SetDataObject(
                    (string)dataGridView1.SelectedCells[0].OwningRow.Cells[1].Value, true);
            }
        }
        #endregion

        #region SaveEntry
        public void SaveEntry(int number, string text)
        {
            for (int i = 0; i < _cliloc.Entries.Count; ++i)
            {
                if (_cliloc.Entries[i].Number != number)
                {
                    continue;
                }

                _cliloc.Entries[i].Text = text;
                _cliloc.Entries[i].Flag = StringEntry.CliLocFlag.Modified;

                dataGridView1.Invalidate();
                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;

                Options.ChangedUltimaClass["CliLoc"] = true;

                return;
            }
        }
        #endregion

        #region IsNumberFree
        public bool IsNumberFree(int number)
        {
            foreach (StringEntry entry in _cliloc.Entries)
            {
                if (entry.Number == number)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region AddEntry
        public void AddEntry(int number)
        {
            int index = 0;

            foreach (StringEntry entry in _cliloc.Entries)
            {
                if (entry.Number > number)
                {
                    _cliloc.Entries.Insert(index, new StringEntry(number, "", StringEntry.CliLocFlag.Custom));

                    dataGridView1.Invalidate();
                    dataGridView1.Rows[index].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = index;

                    Options.ChangedUltimaClass["CliLoc"] = true;

                    return;
                }

                ++index;
            }
        }
        #endregion

        #region FindEntry_PreviewKeyDown
        private static void FindEntry_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyData == Keys.Control) || (e.Ke Keys.Alt | Keys.Tab | Keys.a))
            e.IsInputKey = true;
        }
        #endregion

        #region OnClickExportCSV
        private void OnClickExportCSV(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "CliLoc.csv");

            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)))
            {
                tex.WriteLine("Number;Text;Flag");

                foreach (StringEntry entry in _cliloc.Entries)
                {
                    tex.WriteLine("{0};{1};{2}", entry.Number, entry.Text, entry.Flag);
                }
            }

            MessageBox.Show($"CliLoc saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickImportCSV
        private void OnClickImportCSV(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    int count = 0;

                    while (sr.ReadLine() is { } line)
                    {
                        if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        if (line.StartsWith("Number;"))
                        {
                            continue;
                        }

                        try
                        {
                            string[] split = line.Split(';');
                            if (split.Length < 3)
                            {
                                continue;
                            }

                            int id = int.Parse(split[0].Trim());
                            string text = split[1].Trim();

                            int index = 0;
                            foreach (StringEntry entry in _cliloc.Entries)
                            {
                                if (entry.Number == id)
                                {
                                    if (entry.Text != text)
                                    {
                                        entry.Text = text;
                                        entry.Flag = StringEntry.CliLocFlag.Modified;
                                        count++;
                                    }
                                    break;
                                }

                                if (entry.Number > id)
                                {
                                    _cliloc.Entries.Insert(index, new StringEntry(id, text, StringEntry.CliLocFlag.Custom));
                                    count++;
                                    break;
                                }
                                ++index;
                            }

                            dataGridView1.Invalidate();
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (count > 0)
                    {
                        Options.ChangedUltimaClass["CliLoc"] = true;
                        MessageBox.Show(this, $"{count} entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "No entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            dialog.Dispose();
        }
        #endregion

        #region TileDataToolStripMenuItem
        private void TileDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 0;
            for (int index = 0; index < TileData.ItemTable.Length; index++)
            {
                ItemData itemData = TileData.ItemTable[index];
                int baseClilocId = GetCliLocBaseId(index);
                int id = index + baseClilocId;

                if (string.IsNullOrWhiteSpace(itemData.Name))
                {
                    int i = _cliloc.Entries.FindIndex(x => x.Number == id);

                    if (i >= 0)
                    {
                        _cliloc.Entries.RemoveAt(i);
                        count++;
                    }
                }
                else
                {
                    int entryIndex = 0;
                    foreach (StringEntry entry in _cliloc.Entries)
                    {
                        if (entry.Number == id)
                        {
                            if (entry.Text != itemData.Name)
                            {
                                entry.Text = itemData.Name;
                                entry.Flag = StringEntry.CliLocFlag.Modified;
                                count++;
                            }

                            break;
                        }

                        if (entry.Number > id)
                        {
                            _cliloc.Entries.Insert(entryIndex, new StringEntry(id, itemData.Name, StringEntry.CliLocFlag.Modified));
                            count++;
                            break;
                        }

                        entryIndex++;
                    }
                }
            }

            if (count > 0)
            {
                Options.ChangedUltimaClass["CliLoc"] = true;
                MessageBox.Show(this, $"{count} entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "No entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region GetCliLocBaseId
        private static int GetCliLocBaseId(int tileId)
        {
            if (tileId >= 0x4000u)
            {
                if (tileId >= 0x8000u)
                {
                    if (tileId < 0x10000)
                    {
                        return 1084024;
                    }
                }
                else
                {
                    return 1078872;
                }
            }
            else
            {
                return 1020000;
            }

            throw new ArgumentException("Tile id out of range.", nameof(tileId));
        }
        #endregion

        #region GotoEntry_Enter
        private void GotoEntry_Enter(object sender, EventArgs e)
        {
            if (GotoEntry.Text == _searchNumberPlaceholder)
            {
                GotoEntry.Text = "";
            }
        }
        #endregion

        #region FindEntry_Enter
        private void FindEntry_Enter(object sender, EventArgs e)
        {
            if (FindEntry.Text == _searchTextPlaceholder)
            {
                FindEntry.Text = "";
            }
        }
        #endregion

        #region GotoEntry_KeyDown
        private void GotoEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            GotoNr(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
        #endregion

        #region FindEntry_KeyDown
        private void FindEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            FindEntryClick(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
        #endregion

        #region copyToolStripMenuItem
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                // Access the first selected cell
                DataGridViewCell cell = dataGridView1.SelectedCells[0];

                // Copy the cell's value to the clipboard
                Clipboard.SetDataObject(cell.Value.ToString(), true);
            }
        }
        #endregion

        #region addHtmlLocalizedToolStripMenuItem 
        private void addHtmlLocalizedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            TextBox[] textBoxes = new TextBox[7];
            Label[] labels = new Label[7]
            {
                new Label { Text = "X - coordinate" },
                new Label { Text = "Y - coordinate" },
                new Label { Text = "Width" },
                new Label { Text = "Height" },
                new Label { Text = "Color" },
                new Label { Text = "Arguments" },
                new Label { Text = "comment" }
            };
            Button copyButton = new Button();

            for (int i = 0; i < textBoxes.Length; i++)
            {
                labels[i].Top = i * 30;
                labels[i].Left = 10;
                form.Controls.Add(labels[i]);

                textBoxes[i] = new TextBox { Top = i * 30, Left = labels[i].Right + 10 };
                form.Controls.Add(textBoxes[i]);
            }

            // Set the default value for the arguments text box
            textBoxes[5].Text = "string.Empty";

            // Load comment text from Cells[1] when the form is opened
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedRow = dataGridView1.SelectedCells[0].RowIndex;
                string text = dataGridView1.Rows[selectedRow].Cells[1].Value.ToString();
                textBoxes[6].Text = text;
            }

            copyButton.Text = "Copy";
            copyButton.Top = textBoxes.Length * 30;
            copyButton.Left = 10;
            copyButton.Click += (sender, e) =>
            {
                if (dataGridView1.SelectedCells.Count > 0)
                {
                    int selectedRow = dataGridView1.SelectedCells[0].RowIndex;
                    string number = dataGridView1.Rows[selectedRow].Cells[0].Value.ToString();

                    // Use the comment text from the text box
                    string comment = textBoxes[6].Text;

                    string arguments = textBoxes[5].Text == "string.Empty" ? "string.Empty" : $"\"{textBoxes[5].Text}\"";

                    string result = $"AddHtmlLocalized({textBoxes[0].Text}, {textBoxes[1].Text}, {textBoxes[2].Text}, {textBoxes[3].Text}, {number}, {arguments}, {textBoxes[4].Text}, false, false); // {comment}";

                    Clipboard.SetText(result);
                }
            };

            form.Controls.Add(copyButton);
            form.Show();
        }
        #endregion
    }
}

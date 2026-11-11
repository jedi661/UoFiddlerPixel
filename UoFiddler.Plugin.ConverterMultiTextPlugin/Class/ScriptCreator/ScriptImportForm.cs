using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>
    /// Turns an existing script (.cs or Sphere .scp) into a template: every value found can become a field
    /// (tick it, rename it, choose its type); the result is saved as own template.
    /// </summary>
    internal sealed class ScriptImportForm : Form
    {
        private readonly TextBox _name, _group, _description;
        private readonly DataGridView _grid;
        private readonly RichTextBox _preview;
        private readonly Label _info;
        private ScriptImporter.Result _result;

        public ScriptTemplate Template { get; private set; }

        public ScriptImportForm(string source, string fileName)
        {
            Text = "Script → template / Skript → Vorlage";
            Size = new Size(1150, 760);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowIcon = false;

            _name = new TextBox { Width = 230, Text = Path.GetFileNameWithoutExtension(fileName ?? "Imported script") };
            _group = new TextBox { Width = 180, Text = "My templates" };
            _description = new TextBox { Width = 420, Text = "Imported from " + Path.GetFileName(fileName ?? "clipboard") };
            _info = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 0) };
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4) };
            top.Controls.AddRange(new Control[]
            {
                new Label { Text = "Name:", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, _name,
                new Label { Text = "Group:", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, _group,
                new Label { Text = "Description:", AutoSize = true, Margin = new Padding(3, 7, 3, 0) }, _description
            });

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "Field", FillWeight = 12 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Key", FillWeight = 40 });
            var typeCol = new DataGridViewComboBoxColumn { HeaderText = "Type", FillWeight = 30, FlatStyle = FlatStyle.Flat };
            typeCol.Items.AddRange(ScriptImporter.Types.Cast<object>().ToArray());
            _grid.Columns.Add(typeCol);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Label", FillWeight = 40 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Default", FillWeight = 35 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Found in", FillWeight = 90, ReadOnly = true });
            _grid.DataError += (s, e) => e.ThrowException = false;
            _grid.CellValueChanged += (s, e) => UpdatePreview();
            _grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (_grid.IsCurrentCellDirty && (_grid.CurrentCell is DataGridViewCheckBoxCell || _grid.CurrentCell is DataGridViewComboBoxCell))
                {
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };

            _preview = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9.5f), WordWrap = false, BackColor = SystemColors.Window };
            var split = new SplitContainer { Dock = DockStyle.Fill };
            Shown += (s, e) => { if (split.Width > 600) split.SplitterDistance = Math.Min(560, split.Width - 200); };
            split.Panel1.Controls.Add(_grid);
            split.Panel2.Controls.Add(_preview);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var save = new Button { Text = "Save as template", AutoSize = true, Height = 28 };
            save.Click += (s, e) => SaveTemplate();
            var cancel = new Button { Text = "Cancel", AutoSize = true, Height = 28, DialogResult = DialogResult.Cancel };
            var all = new Button { Text = "All", AutoSize = true, Height = 28 };
            all.Click += (s, e) => SetAll(true);
            var none = new Button { Text = "None", AutoSize = true, Height = 28 };
            none.Click += (s, e) => SetAll(false);
            buttons.Controls.AddRange(new Control[] { cancel, save, none, all, _info });
            CancelButton = cancel;

            var hint = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.DimGray,
                Padding = new Padding(6, 2, 6, 0),
                Text = "Tick the values that you want to change later – they become fields with {{placeholders}}. The class name becomes {{ClassName}} automatically.\n" +
                       "Werte ankreuzen, die du später ändern willst – sie werden Felder mit {{Platzhaltern}}. Der Klassenname wird automatisch {{ClassName}}."
            };

            Controls.Add(split);
            Controls.Add(hint);
            Controls.Add(top);
            Controls.Add(buttons);

            _result = ScriptImporter.Analyze(source);
            foreach (ImportCandidate c in _result.Candidates)
            {
                string type = ScriptImporter.Types.Contains(c.Type) ? c.Type : null;
                if (type == null && !typeCol.Items.Contains(c.Type))
                {
                    typeCol.Items.Add(c.Type);
                }
                int row = _grid.Rows.Add(c.Use, c.Key, c.Type, c.Label, c.Default, c.Context);
                _grid.Rows[row].Tag = c;
            }
            _info.Text = (_result.ClassName != null ? $"class {_result.ClassName}" + (_result.BaseClass != null ? $" : {_result.BaseClass}" : string.Empty) + " · " : string.Empty)
                         + $"{_result.Candidates.Count} values found";
            UpdatePreview();
        }

        private void SetAll(bool use)
        {
            foreach (DataGridViewRow r in _grid.Rows)
            {
                r.Cells[0].Value = use;
            }
            UpdatePreview();
        }

        private void ReadGrid()
        {
            foreach (DataGridViewRow r in _grid.Rows)
            {
                var c = (ImportCandidate)r.Tag;
                c.Use = r.Cells[0].Value is bool b && b;
                c.Key = Convert.ToString(r.Cells[1].Value)?.Trim();
                c.Type = Convert.ToString(r.Cells[2].Value)?.Trim();
                c.Label = Convert.ToString(r.Cells[3].Value)?.Trim();
                c.Default = Convert.ToString(r.Cells[4].Value) ?? string.Empty;
            }
        }

        private ScriptTemplate BuildTemplate()
        {
            ReadGrid();
            return ScriptImporter.Build(_result, _name.Text.Trim(), _group.Text.Trim(), _description.Text.Trim());
        }

        private void UpdatePreview()
        {
            try
            {
                CodeHighlighter.Show(_preview, BuildTemplate().ToText(), "tpl");
            }
            catch (Exception ex)
            {
                _preview.Text = ex.Message;
            }
        }

        private void SaveTemplate()
        {
            ReadGrid();
            var dupes = _result.Candidates.Where(c => c.Use).GroupBy(c => c.Key, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (dupes.Count > 0 || _result.Candidates.Any(c => c.Use && string.IsNullOrWhiteSpace(c.Key)))
            {
                MessageBox.Show(this, "Every ticked value needs its own key.\n" + string.Join(", ", dupes), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_name.Text))
            {
                MessageBox.Show(this, "Give the template a name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                ScriptTemplate t = BuildTemplate();
                ScriptTemplate.Parse(t.ToText()); // check
                string file = Path.Combine(ScriptTemplateLibrary.UserFolder, new string(t.Name.Select(ch => Path.GetInvalidFileNameChars().Contains(ch) || ch == ' ' ? '_' : ch).ToArray()) + ScriptTemplateLibrary.Extension);
                if (File.Exists(file) && MessageBox.Show(this, $"\"{Path.GetFileName(file)}\" exists. Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
                t.SourceFile = file;
                ScriptTemplateLibrary.Save(t);
                Template = t;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ReadText(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            try
            {
                return new UTF8Encoding(false, true).GetString(bytes).TrimStart('﻿');
            }
            catch (DecoderFallbackException)
            {
                return Encoding.Latin1.GetString(bytes);
            }
        }
    }
}

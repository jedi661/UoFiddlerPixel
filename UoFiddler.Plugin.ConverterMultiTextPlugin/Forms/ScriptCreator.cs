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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Script generator based on templates: pick a template, fill the fields (with lists, item / hue / body previews and checks),
    /// see the script live and save it. Own templates live in ScriptTemplates (*.uotpl); existing scripts can be imported as templates.
    /// Third tab: editor for the client text files (walls.txt, doors.txt, bodyconv.def …).
    /// </summary>
    public partial class ScriptCreator : Form
    {
        private static string SettingsFile => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptCreator.settings.xml");

        private List<ScriptTemplate> _templates = new List<ScriptTemplate>();
        private ScriptTemplate _current;
        private readonly List<FieldEditor> _editors = new List<FieldEditor>();
        private readonly Dictionary<ScriptTemplate, Dictionary<string, string>> _values = new Dictionary<ScriptTemplate, Dictionary<string, string>>();
        private string _scriptsFolder = string.Empty;
        private string _lastTemplate;
        private string _lastOutput = string.Empty;

        public ScriptCreator()
        {
            InitializeComponent();
            LoadSettings();
            BuildToolbar();
            BuildGeneratorTab();
            BuildTemplatesTab();
            BuildClientFilesTab();
            ReloadTemplates(_lastTemplate);
            SetMessage("Choose a template on the left, fill the fields, save the script. Own scripts: \"Import script\". F1 = help.");
        }

        #region Helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private Button Btn(string text, EventHandler click, string tip = null)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            b.Click += click;
            if (tip != null)
            {
                toolTip1.SetToolTip(b, tip);
            }
            return b;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private ToolStripButton TsButton(string text, string tip, EventHandler click, Image image = null)
        {
            var b = new ToolStripButton(text, image) { ToolTipText = tip, DisplayStyle = image != null ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Text };
            b.Click += click;
            return b;
        }

        private static void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // no explorer
            }
        }

        private string Prompt(string question, string value)
        {
            using (var f = new Form { Text = Text, FormBorderStyle = FormBorderStyle.FixedDialog, MinimizeBox = false, MaximizeBox = false, ShowIcon = false,
                       StartPosition = FormStartPosition.CenterParent, ClientSize = new Size(420, 110), TopMost = TopMost })
            {
                var label = new Label { Text = question, Left = 10, Top = 10, Width = 400 };
                var box = new TextBox { Text = value, Left = 10, Top = 34, Width = 400 };
                var ok = new Button { Text = "OK", Left = 250, Top = 72, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 335, Top = 72, Width = 75, DialogResult = DialogResult.Cancel };
                f.Controls.AddRange(new Control[] { label, box, ok, cancel });
                f.AcceptButton = ok;
                f.CancelButton = cancel;
                return f.ShowDialog(this) == DialogResult.OK ? box.Text : null;
            }
        }

        private void Copy(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            try
            {
                Clipboard.SetText(text);
                SetMessage("Copied to the clipboard.");
            }
            catch (Exception ex)
            {
                SetMessage("Copy failed: " + ex.Message);
            }
        }

        #endregion

        #region Settings

        private string _clientFolder = string.Empty;

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    XElement root = XDocument.Load(SettingsFile).Root;
                    _scriptsFolder = (string)root?.Element("ScriptsFolder") ?? string.Empty;
                    _clientFolder = (string)root?.Element("ClientFolder") ?? string.Empty;
                    _lastTemplate = (string)root?.Element("LastTemplate");
                }
            }
            catch (Exception)
            {
                // broken settings are ignored
            }
            if (string.IsNullOrEmpty(_clientFolder))
            {
                try
                {
                    _clientFolder = Ultima.Files.RootDir ?? string.Empty;
                }
                catch (Exception)
                {
                    _clientFolder = string.Empty;
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                new XDocument(new XElement("ScriptCreator",
                    new XElement("ScriptsFolder", _scriptsFolder ?? string.Empty),
                    new XElement("ClientFolder", _clientFolder ?? string.Empty),
                    new XElement("LastTemplate", _current?.Name ?? string.Empty))).Save(SettingsFile);
            }
            catch (Exception)
            {
                // read-only folder
            }
        }

        #endregion

        #region Toolbar

        private void BuildToolbar()
        {
            toolStrip1.Items.AddRange(new ToolStripItem[]
            {
                TsButton("Import script…", Tip("Turn an existing script (.cs / .scp) into an own template: its values become fields.",
                    "Ein vorhandenes Skript (.cs / .scp) in eine eigene Vorlage umwandeln: seine Werte werden Felder."), (s, e) => ImportScriptFile(), Properties.Resources.Load),
                TsButton("Import from clipboard", Tip("Same with a script copied to the clipboard.", "Dasselbe mit einem Skript aus der Zwischenablage."), (s, e) => ImportClipboard()),
                new ToolStripSeparator(),
                TsButton("Reload templates", Tip("Reads the templates folder again (after editing files by hand).", "Liest den Vorlagenordner neu (nach Bearbeitung von Hand)."), (s, e) => ReloadTemplates(_current?.Name)),
                TsButton("Templates folder", Tip("Opens ScriptTemplates next to UoFiddler.", "Öffnet ScriptTemplates neben UoFiddler."), (s, e) => OpenFolder(ScriptTemplateLibrary.UserFolder)),
                new ToolStripSeparator(),
                TsButton("?", Tip("Help (F1)", "Hilfe (F1)"), (s, e) => ShowHelp())
            });
        }

        #endregion

        #region Generator tab

        private TreeView _tree;
        private TextBox _search;
        private Panel _fieldsHost;
        private TableLayoutPanel _fieldsTable;
        private Label _title, _description, _problems;
        private RichTextBox _output;
        private TextBox _txtScriptsFolder;
        private Panel _bigPreview;
        private Label _fileLabel;

        private void BuildGeneratorTab()
        {
            _search = new TextBox { Dock = DockStyle.Top, PlaceholderText = "Search / Suchen" };
            _search.TextChanged += (s, e) => FillTree(_current?.Name);
            _tree = new TreeView { Dock = DockStyle.Fill, HideSelection = false, FullRowSelect = true };
            _tree.AfterSelect += (s, e) =>
            {
                if (e.Node?.Tag is ScriptTemplate t)
                {
                    SelectTemplate(t);
                }
            };
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(_tree);
            left.Controls.Add(_search);

            _title = new Label { AutoSize = true, Font = new Font(Font.FontFamily, 11f, FontStyle.Bold), Margin = new Padding(3, 3, 3, 0) };
            _description = new Label { AutoSize = true, MaximumSize = new Size(420, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 2, 3, 6) };
            _bigPreview = new Panel { Width = 150, Height = 150, Margin = new Padding(3) };
            _bigPreview.Paint += BigPreview_Paint;
            _fieldsTable = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(0) };
            _fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            _fieldsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var fieldsFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            buttons.Controls.Add(Btn("Reset values", (s, e) => ResetValues(), Tip("Back to the defaults of the template.", "Zurück zu den Vorgaben der Vorlage.")));
            buttons.Controls.Add(Btn("Edit template", (s, e) => EditCurrentTemplate(), Tip("Opens the template in the Templates tab.", "Öffnet die Vorlage im Reiter Templates.")));
            buttons.Controls.Add(Btn("Values as defaults", (s, e) => SaveValuesAsDefaults(), Tip(
                "Saves the current values as defaults of an own copy of the template.", "Speichert die aktuellen Werte als Vorgaben einer eigenen Kopie der Vorlage.")));
            fieldsFlow.Controls.AddRange(new Control[] { _title, _description, _bigPreview, _fieldsTable, buttons });
            _fieldsHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(4) };
            _fieldsHost.Controls.Add(fieldsFlow);

            _problems = new Label { Dock = DockStyle.Top, AutoSize = true, MaximumSize = new Size(700, 0), ForeColor = Color.Firebrick, Padding = new Padding(4) };
            _output = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, WordWrap = false, Font = new Font("Consolas", 10f), BackColor = SystemColors.Window, DetectUrls = false };
            _txtScriptsFolder = new TextBox { Width = 300, Text = _scriptsFolder, Margin = new Padding(3, 4, 3, 0) };
            _txtScriptsFolder.TextChanged += (s, e) => _scriptsFolder = _txtScriptsFolder.Text.Trim();
            _fileLabel = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 0) };
            var outBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
            outBar.Controls.AddRange(new Control[]
            {
                Btn("Copy", (s, e) => Copy(_lastOutput), Tip("Copy the script (Ctrl+C when nothing is selected)", "Skript kopieren")),
                Btn("Save", (s, e) => SaveScript(false), Tip("Save into the scripts folder (Ctrl+S)", "In den Scripts-Ordner speichern (Strg+S)")),
                Btn("Save as…", (s, e) => SaveScript(true)),
                Lbl("Scripts folder:"), _txtScriptsFolder,
                Btn("…", (s, e) => BrowseScriptsFolder()),
                Btn("Open", (s, e) => { if (Directory.Exists(_scriptsFolder)) OpenFolder(_scriptsFolder); }),
                _fileLabel
            });
            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(_output);
            right.Controls.Add(_problems);
            right.Controls.Add(outBar);

            var inner = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 500, FixedPanel = FixedPanel.Panel1 };
            inner.Panel1.Controls.Add(_fieldsHost);
            inner.Panel2.Controls.Add(right);
            var outer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 250, FixedPanel = FixedPanel.Panel1 };
            outer.Panel1.Controls.Add(left);
            outer.Panel2.Controls.Add(inner);
            tabGenerator.Controls.Add(outer);
        }

        private void ReloadTemplates(string select)
        {
            var errors = new List<string>();
            ScriptLists.Reload();
            Dictionary<string, Dictionary<string, string>> old = _values.ToDictionary(kv => kv.Key.Name, kv => kv.Value);
            _templates = ScriptTemplateLibrary.LoadAll(errors);
            _values.Clear();
            foreach (ScriptTemplate t in _templates)
            {
                if (old.TryGetValue(t.Name, out Dictionary<string, string> v))
                {
                    _values[t] = v;
                }
            }
            _current = null;
            FillTree(select);
            FillTemplateList(select);
            if (errors.Count > 0)
            {
                MessageBox.Show(this, "Some templates could not be read:\n\n" + string.Join("\n", errors), Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            SetMessage($"{_templates.Count} templates ({_templates.Count(t => !t.IsBuiltIn)} own).");
        }

        private void FillTree(string select)
        {
            string f = _search.Text.Trim();
            _tree.BeginUpdate();
            _tree.Nodes.Clear();
            TreeNode toSelect = null;
            foreach (IGrouping<string, ScriptTemplate> g in _templates
                         .Where(t => f.Length == 0 || t.Name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 || t.Group.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0
                                     || t.Description.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0)
                         .GroupBy(t => t.Group).OrderBy(g => g.Key.StartsWith("ServUO", StringComparison.Ordinal) ? 0 : g.Key.StartsWith("Sphere", StringComparison.Ordinal) ? 1 : 2).ThenBy(g => g.Key))
            {
                var node = new TreeNode(g.Key);
                foreach (ScriptTemplate t in g.OrderBy(t => t.Name))
                {
                    var n = new TreeNode(t.IsBuiltIn ? t.Name : "★ " + t.Name) { Tag = t, ToolTipText = t.Description };
                    node.Nodes.Add(n);
                    if (select != null && t.Name == select)
                    {
                        toSelect = n;
                    }
                }
                _tree.Nodes.Add(node);
            }
            _tree.ExpandAll();
            _tree.EndUpdate();
            _tree.ShowNodeToolTips = true;
            if (toSelect == null && f.Length == 0 && _tree.Nodes.Count > 0 && _tree.Nodes[0].Nodes.Count > 0)
            {
                toSelect = _tree.Nodes[0].Nodes[0];
            }
            if (toSelect != null)
            {
                _tree.SelectedNode = toSelect;
                toSelect.EnsureVisible();
            }
        }

        private void SelectTemplate(ScriptTemplate t)
        {
            StoreValues();
            _current = t;
            if (!_values.TryGetValue(t, out Dictionary<string, string> values))
            {
                values = t.DefaultValues();
                _values[t] = values;
            }
            _title.Text = t.Name + (t.IsBuiltIn ? string.Empty : "   (own)");
            _description.Text = t.Description;
            _fieldsTable.SuspendLayout();
            _fieldsTable.Controls.Clear();
            _fieldsTable.RowStyles.Clear();
            _editors.Clear();
            _fieldsTable.RowCount = t.Fields.Count;
            for (int i = 0; i < t.Fields.Count; i++)
            {
                TemplateField f = t.Fields[i];
                values.TryGetValue(f.Key, out string v);
                var ed = new FieldEditor(f, v ?? f.Default, toolTip1);
                ed.Changed += (s, e) => SchedulePreview();
                _editors.Add(ed);
                var label = new Label { Text = f.Label, AutoSize = false, Width = 146, Height = 22, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(3, 3, 3, 0) };
                toolTip1.SetToolTip(label, $"{{{{{f.Key}}}}}  ({f.Type})" + (string.IsNullOrEmpty(f.Tip) ? string.Empty : "\n" + f.Tip));
                _fieldsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                _fieldsTable.Controls.Add(label, 0, i);
                _fieldsTable.Controls.Add(ed.Control, 1, i);
            }
            _fieldsTable.ResumeLayout();
            _bigPreview.Visible = t.Fields.Any(f => f.BaseType == "itemid" || f.BaseType == "body");
            UpdatePreview();
        }

        private void StoreValues()
        {
            if (_current == null)
            {
                return;
            }
            _values[_current] = CurrentValues();
        }

        private Dictionary<string, string> CurrentValues()
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldEditor ed in _editors)
            {
                d[ed.Field.Key] = ed.Value;
            }
            return d;
        }

        private void ResetValues()
        {
            if (_current == null)
            {
                return;
            }
            foreach (FieldEditor ed in _editors)
            {
                ed.Value = ed.Field.Default;
            }
            UpdatePreview();
        }

        private void SchedulePreview()
        {
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            previewTimer.Stop();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_current == null)
            {
                return;
            }
            Dictionary<string, string> v = CurrentValues();
            List<string> problems = _current.Validate(v);
            try
            {
                _lastOutput = _current.Render(v).Replace("\r\n", "\n").Replace("\n", "\r\n");
                CodeHighlighter.Show(_output, _lastOutput, _current.Output);
                _fileLabel.Text = "→ " + _current.RenderFileName(v);
            }
            catch (Exception ex)
            {
                _lastOutput = string.Empty;
                _output.Text = "Template error: " + ex.Message;
                problems.Insert(0, "The template has an error – fix it in the Templates tab.");
            }
            _problems.Text = problems.Count == 0 ? string.Empty : "⚠ " + string.Join("\n⚠ ", problems);
            _bigPreview.Invalidate();
        }

        private void BigPreview_Paint(object sender, PaintEventArgs e)
        {
            if (_current == null)
            {
                return;
            }
            FieldEditor id = _editors.FirstOrDefault(x => x.Field.BaseType == "itemid") ?? _editors.FirstOrDefault(x => x.Field.BaseType == "body");
            FieldEditor hue = _editors.FirstOrDefault(x => x.Field.BaseType == "hue");
            int hueValue = hue != null && ScriptTemplate.TryParseInt(hue.Value, out long hv) ? (int)hv : 0;
            Bitmap img = null;
            string caption = "–";
            if (id != null && ScriptTemplate.TryParseInt(id.Value, out long v) && v >= 0 && v <= 0xFFFF)
            {
                img = id.Field.BaseType == "body" ? UoPreview.Body((int)v, hueValue) : UoPreview.Item((int)v, hueValue);
                caption = img == null ? "not in the client files" : $"{id.Field.Label} 0x{v:X4}";
            }
            UoPreview.Draw(e.Graphics, _bigPreview.ClientRectangle, img, caption, Font);
            img?.Dispose();
        }

        private void BrowseScriptsFolder()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = Directory.Exists(_scriptsFolder) ? _scriptsFolder : string.Empty, Description = "Scripts folder (e.g. ServUO\\Scripts\\Custom)" })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _txtScriptsFolder.Text = d.SelectedPath;
                    SaveSettings();
                }
            }
        }

        private void SaveScript(bool ask)
        {
            if (_current == null || string.IsNullOrEmpty(_lastOutput))
            {
                return;
            }
            Dictionary<string, string> v = CurrentValues();
            List<string> problems = _current.Validate(v);
            if (problems.Count > 0 && MessageBox.Show(this, "The values have problems:\n\n" + string.Join("\n", problems) + "\n\nSave anyway?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            string fileName = _current.RenderFileName(v);
            string target;
            if (ask || !Directory.Exists(_scriptsFolder))
            {
                using (var d = new SaveFileDialog { FileName = fileName, Filter = $"{_current.Output} files|*.{_current.Output}|All files|*.*" })
                {
                    if (Directory.Exists(_scriptsFolder))
                    {
                        d.InitialDirectory = _scriptsFolder;
                    }
                    if (d.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    target = d.FileName;
                    if (!Directory.Exists(_scriptsFolder))
                    {
                        _txtScriptsFolder.Text = Path.GetDirectoryName(target);
                    }
                }
            }
            else
            {
                target = Path.Combine(_scriptsFolder, fileName);
                if (File.Exists(target) && MessageBox.Show(this, $"{fileName} exists. Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }
            try
            {
                File.WriteAllText(target, _lastOutput, new UTF8Encoding(false));
                SaveSettings();
                SetMessage("Saved: " + target);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveValuesAsDefaults()
        {
            if (_current == null)
            {
                return;
            }
            ScriptTemplate copy = ScriptTemplate.Parse(_current.ToText());
            Dictionary<string, string> v = CurrentValues();
            foreach (TemplateField f in copy.Fields)
            {
                if (v.TryGetValue(f.Key, out string value))
                {
                    f.Default = value;
                }
            }
            if (_current.IsBuiltIn)
            {
                string name = Prompt("Name of the own template / Name der eigenen Vorlage:", _current.Name + " (" + (v.TryGetValue("ClassName", out string cn) ? cn : "copy") + ")");
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }
                copy.Name = name.Trim();
                copy.Group = "My templates";
                copy.SourceFile = null;
            }
            else
            {
                copy.SourceFile = _current.SourceFile;
            }
            try
            {
                string file = ScriptTemplateLibrary.Save(copy);
                ReloadTemplates(copy.Name);
                SetMessage("Template saved: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Import

        private void ImportScriptFile()
        {
            using (var d = new OpenFileDialog { Filter = "Scripts (*.cs;*.scp)|*.cs;*.scp|All files|*.*", Title = "Script to turn into a template" })
            {
                if (Directory.Exists(_scriptsFolder))
                {
                    d.InitialDirectory = _scriptsFolder;
                }
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    Import(ScriptImportForm.ReadText(d.FileName), d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Reading failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportClipboard()
        {
            string text = null;
            try
            {
                text = Clipboard.ContainsText() ? Clipboard.GetText() : null;
            }
            catch (Exception)
            {
                text = null;
            }
            if (string.IsNullOrWhiteSpace(text))
            {
                SetMessage("The clipboard contains no text.");
                return;
            }
            Import(text, "Clipboard script");
        }

        private void Import(string source, string name)
        {
            using (var f = new ScriptImportForm(source, name) { TopMost = TopMost })
            {
                if (f.ShowDialog(this) == DialogResult.OK && f.Template != null)
                {
                    ReloadTemplates(f.Template.Name);
                    tabControl1.SelectedTab = tabGenerator;
                    SetMessage($"Template \"{f.Template.Name}\" created with {f.Template.Fields.Count} fields: {f.Template.SourceFile}");
                }
            }
        }

        #endregion

        #region Templates tab

        private ListBox _templateList;
        private RichTextBox _templateText;
        private RichTextBox _templateTest;
        private Label _templateInfo;
        private ScriptTemplate _editing;
        private bool _templateDirty;

        private void BuildTemplatesTab()
        {
            _templateList = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            _templateList.SelectedIndexChanged += (s, e) =>
            {
                if (_templateList.SelectedItem is TemplateItem ti && ti.Template != _editing)
                {
                    if (!ConfirmTemplateDiscard())
                    {
                        return;
                    }
                    LoadTemplateText(ti.Template);
                }
            };
            var listButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            listButtons.Controls.AddRange(new Control[]
            {
                Btn("New", (s, e) => NewTemplate()),
                Btn("Delete", (s, e) => DeleteTemplate(), Tip("Deletes an own template (built-in ones cannot be deleted).", "Löscht eine eigene Vorlage (eingebaute nicht).")),
                Btn("Folder", (s, e) => OpenFolder(ScriptTemplateLibrary.UserFolder))
            });
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(_templateList);
            left.Controls.Add(listButtons);

            _templateText = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f), WordWrap = false, AcceptsTab = true, DetectUrls = false };
            _templateText.TextChanged += (s, e) => _templateDirty = true;
            _templateInfo = new Label { Dock = DockStyle.Top, AutoSize = true, ForeColor = Color.DimGray, Padding = new Padding(4) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            bar.Controls.AddRange(new Control[]
            {
                Btn("Test", (s, e) => TestTemplate(), Tip("Builds the script with the default values (F5).", "Baut das Skript mit den Vorgaben (F5).")),
                Btn("Colors", (s, e) => ColorTemplate()),
                Btn("Save", (s, e) => SaveTemplate(), Tip("Saves as own template (Ctrl+S). A built-in template is saved as own copy that replaces it.",
                    "Speichert als eigene Vorlage (Strg+S). Eine eingebaute wird als eigene Kopie gespeichert, die sie ersetzt.")),
                Btn("Use in generator", (s, e) => { if (_editing != null) { tabControl1.SelectedTab = tabGenerator; FillTree(_editing.Name); } })
            });
            var syntax = new Label
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                ForeColor = Color.DimGray,
                Padding = new Padding(4),
                Text = "@field Key | type | Label | default | tip   types: class string text int:min:max double range bool itemid hue body sound choice:a,b combo:@List list:@List pages:lines:chars\n" +
                       "{{Key}}  {{Key|cs}} (escape)  |lower |upper |hex |dec |scp   {{#if Key}}…{{else}}…{{/if}}  {{#if !Key}}   {{#each List}}{{key}} = {{value}} {{value1}} {{value2}} {{sep}}{{/each}}   {{lb}} = \"{{\""
            };
            var editor = new Panel { Dock = DockStyle.Fill };
            editor.Controls.Add(_templateText);
            editor.Controls.Add(syntax);
            editor.Controls.Add(bar);
            editor.Controls.Add(_templateInfo);

            _templateTest = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9.5f), WordWrap = false, BackColor = SystemColors.Window };
            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 430 };
            right.Panel1.Controls.Add(editor);
            right.Panel2.Controls.Add(_templateTest);
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 250, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(right);
            tabTemplates.Controls.Add(split);
        }

        private sealed class TemplateItem
        {
            public ScriptTemplate Template;
            public override string ToString() => (Template.IsBuiltIn ? string.Empty : "★ ") + Template.Group + " / " + Template.Name;
        }

        private void FillTemplateList(string select)
        {
            _templateList.BeginUpdate();
            _templateList.Items.Clear();
            foreach (ScriptTemplate t in _templates.OrderBy(t => t.Group).ThenBy(t => t.Name))
            {
                _templateList.Items.Add(new TemplateItem { Template = t });
            }
            _templateList.EndUpdate();
            _editing = null;
            int i = _templateList.Items.Cast<TemplateItem>().ToList().FindIndex(x => x.Template.Name == select);
            if (i >= 0)
            {
                _templateList.SelectedIndex = i;
            }
        }

        private bool ConfirmTemplateDiscard()
        {
            return !_templateDirty || _editing == null ||
                   MessageBox.Show(this, $"\"{_editing.Name}\" has unsaved changes. Discard them?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void LoadTemplateText(ScriptTemplate t)
        {
            _editing = t;
            CodeHighlighter.Show(_templateText, t.ToText().Replace("\n", "\r\n"), "tpl");
            _templateDirty = false;
            _templateInfo.Text = t.IsBuiltIn ? "Built-in template – \"Save\" creates an own copy with the same name that replaces it."
                : "Own template: " + t.SourceFile;
            _templateTest.Clear();
        }

        private void ColorTemplate()
        {
            bool dirty = _templateDirty;
            CodeHighlighter.Show(_templateText, _templateText.Text, "tpl");
            _templateDirty = dirty;
        }

        private ScriptTemplate ParseEditor()
        {
            try
            {
                return ScriptTemplate.Parse(_templateText.Text);
            }
            catch (Exception ex)
            {
                _templateTest.Text = "Header error: " + ex.Message;
                return null;
            }
        }

        private void TestTemplate()
        {
            ScriptTemplate t = ParseEditor();
            if (t == null)
            {
                return;
            }
            Dictionary<string, string> v = t.DefaultValues();
            try
            {
                string result = t.Render(v);
                List<string> problems = t.Validate(v);
                string head = $"// {t.Fields.Count} fields · file {t.RenderFileName(v)}" + (problems.Count > 0 ? "\n// ⚠ " + string.Join("\n// ⚠ ", problems) : string.Empty) + "\n\n";
                CodeHighlighter.Show(_templateTest, (head + result).Replace("\r\n", "\n").Replace("\n", "\r\n"), t.Output);
                SetMessage(result.Contains("{{?") ? "Test: unknown placeholders are shown as {{?Name}}." : "Test OK.");
            }
            catch (Exception ex)
            {
                _templateTest.Text = "Template error: " + ex.Message;
            }
        }

        private void SaveTemplate()
        {
            ScriptTemplate t = ParseEditor();
            if (t == null)
            {
                return;
            }
            try
            {
                t.Render(t.DefaultValues());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The template has an error and was not saved:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_editing != null && !_editing.IsBuiltIn && string.Equals(_editing.Name, t.Name, StringComparison.Ordinal))
            {
                t.SourceFile = _editing.SourceFile;
            }
            try
            {
                string file = ScriptTemplateLibrary.Save(t);
                _templateDirty = false;
                ReloadTemplates(t.Name);
                SetMessage("Template saved: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewTemplate()
        {
            if (!ConfirmTemplateDiscard())
            {
                return;
            }
            var t = ScriptTemplate.Parse("""
                @name My new template
                @group My templates
                @output cs
                @file {{ClassName}}.cs
                @description What this template creates.
                @field ClassName | class | Class name | MyItem
                @field ItemID | itemid | Item ID | 0x0EED
                @field Name | string | Name | my item
                @field Hue | hue | Hue | 0
                @template
                using System;

                namespace Server.Items
                {
                    public class {{ClassName}} : Item
                    {
                        [Constructable]
                        public {{ClassName}}()
                            : base({{ItemID}})
                        {
                            Name = "{{Name|cs}}";
                            Hue = {{Hue}};
                        }

                        public {{ClassName}}(Serial serial)
                            : base(serial)
                        {
                        }

                        public override void Serialize(GenericWriter writer)
                        {
                            base.Serialize(writer);
                            writer.Write((int)0);
                        }

                        public override void Deserialize(GenericReader reader)
                        {
                            base.Deserialize(reader);
                            int version = reader.ReadInt();
                        }
                    }
                }

                """);
            _templateList.ClearSelected();
            LoadTemplateText(t);
            _editing = null;
            _templateDirty = true;
            _templateInfo.Text = "New template – change @name and save.";
        }

        private void EditCurrentTemplate()
        {
            if (_current == null)
            {
                return;
            }
            tabControl1.SelectedTab = tabTemplates;
            int i = _templateList.Items.Cast<TemplateItem>().ToList().FindIndex(x => x.Template == _current);
            if (i >= 0)
            {
                _templateList.SelectedIndex = i;
            }
        }

        private void DeleteTemplate()
        {
            if (!(_templateList.SelectedItem is TemplateItem ti))
            {
                return;
            }
            if (ti.Template.IsBuiltIn)
            {
                SetMessage("Built-in templates cannot be deleted (an own copy with the same name replaces them).");
                return;
            }
            if (MessageBox.Show(this, $"Delete \"{ti.Template.Name}\"?\n{ti.Template.SourceFile}", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                File.Delete(ti.Template.SourceFile);
                _templateDirty = false;
                ReloadTemplates(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Client text files tab

        private TextBox _txtClientFolder, _find, _replace;
        private ListBox _clientFiles;
        private RichTextBox _clientText;
        private CheckBox _matchCase;
        private Label _clientInfo;
        private string _clientFile;
        private bool _clientDirty, _clientLoading;

        private void BuildClientFilesTab()
        {
            _txtClientFolder = new TextBox { Dock = DockStyle.Fill, Text = _clientFolder };
            var folderBar = new TableLayoutPanel { Dock = DockStyle.Top, Height = 30, ColumnCount = 3 };
            folderBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            folderBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderBar.Controls.Add(_txtClientFolder, 0, 0);
            folderBar.Controls.Add(Btn("…", (s, e) => BrowseClientFolder()), 1, 0);
            folderBar.Controls.Add(Btn("List", (s, e) => ListClientFiles()), 2, 0);
            _clientFiles = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            _clientFiles.SelectedIndexChanged += (s, e) =>
            {
                if (_clientFiles.SelectedItem is string f && !_clientLoading)
                {
                    OpenClientFile(Path.Combine(_txtClientFolder.Text.Trim(), f));
                }
            };
            toolTip1.SetToolTip(_clientFiles, Tip("Text files of the client: walls, doors, floors, roofs, stairs, misc (house designer), bodyconv.def, mobtypes.txt …",
                "Textdateien des Clients: walls, doors, floors, roof, stairs, misc (Hausbau), bodyconv.def, mobtypes.txt …"));
            var left = new Panel { Dock = DockStyle.Fill };
            left.Controls.Add(_clientFiles);
            left.Controls.Add(folderBar);

            _clientText = new RichTextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 10f), WordWrap = false, AcceptsTab = true, DetectUrls = false };
            _clientText.TextChanged += (s, e) =>
            {
                if (!_clientLoading)
                {
                    _clientDirty = true;
                    UpdateClientInfo();
                }
            };
            _clientText.SelectionChanged += (s, e) => UpdateClientInfo();
            _find = new TextBox { Width = 170, Margin = new Padding(3, 4, 3, 0) };
            _find.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; FindNext(e.Shift ? -1 : 1); } };
            _replace = new TextBox { Width = 140, Margin = new Padding(3, 4, 3, 0) };
            _matchCase = new CheckBox { Text = "Aa", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            toolTip1.SetToolTip(_matchCase, Tip("Match case", "Groß-/Kleinschreibung beachten"));
            _clientInfo = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 0) };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            bar.Controls.AddRange(new Control[]
            {
                Btn("Save", (s, e) => SaveClientFile(), Tip("Saves the file; the first time a .bak copy is made (Ctrl+S).", "Speichert die Datei; beim ersten Mal entsteht eine .bak-Kopie (Strg+S).")),
                Btn("Reload", (s, e) => { if (_clientFile != null && ConfirmClientDiscard()) OpenClientFile(_clientFile, true); }),
                Lbl("Find:"), _find, Btn("▲", (s, e) => FindNext(-1)), Btn("▼", (s, e) => FindNext(1)), _matchCase,
                Lbl("Replace:"), _replace, Btn("Replace", (s, e) => ReplaceOne()), Btn("All", (s, e) => ReplaceAll()),
                _clientInfo
            });
            var right = new Panel { Dock = DockStyle.Fill };
            right.Controls.Add(_clientText);
            right.Controls.Add(bar);
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 260, FixedPanel = FixedPanel.Panel1 };
            split.Panel1.Controls.Add(left);
            split.Panel2.Controls.Add(right);
            tabClientFiles.Controls.Add(split);
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabClientFiles && _clientFiles.Items.Count == 0)
                {
                    ListClientFiles();
                }
            };
        }

        private void BrowseClientFolder()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = Directory.Exists(_txtClientFolder.Text) ? _txtClientFolder.Text : string.Empty, Description = "Ultima Online client folder" })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _txtClientFolder.Text = d.SelectedPath;
                    ListClientFiles();
                }
            }
        }

        private void ListClientFiles()
        {
            string folder = _txtClientFolder.Text.Trim();
            _clientLoading = true;
            _clientFiles.Items.Clear();
            try
            {
                if (Directory.Exists(folder))
                {
                    _clientFolder = folder;
                    SaveSettings();
                    foreach (string f in Directory.GetFiles(folder)
                                 .Where(f => new[] { ".txt", ".def", ".cfg", ".csv", ".xml" }.Contains(Path.GetExtension(f).ToLowerInvariant()))
                                 .OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        _clientFiles.Items.Add(Path.GetFileName(f));
                    }
                    SetMessage($"{_clientFiles.Items.Count} text files in {folder}.");
                }
                else
                {
                    SetMessage("Choose the client folder.");
                }
            }
            finally
            {
                _clientLoading = false;
            }
        }

        private bool ConfirmClientDiscard()
        {
            return !_clientDirty || MessageBox.Show(this, $"{Path.GetFileName(_clientFile)} has unsaved changes. Discard them?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void OpenClientFile(string file, bool force = false)
        {
            if (!force && string.Equals(file, _clientFile, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (!force && !ConfirmClientDiscard())
            {
                return;
            }
            try
            {
                _clientLoading = true;
                // Latin-1 keeps every byte as it is
                _clientText.Text = File.ReadAllText(file, Encoding.Latin1);
                _clientFile = file;
                _clientDirty = false;
                UpdateClientInfo();
                SetMessage("Opened " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Opening failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _clientLoading = false;
            }
        }

        private void SaveClientFile()
        {
            if (_clientFile == null)
            {
                SetMessage("No file is open.");
                return;
            }
            try
            {
                string bak = _clientFile + ".bak";
                if (!File.Exists(bak) && File.Exists(_clientFile))
                {
                    File.Copy(_clientFile, bak);
                }
                File.WriteAllText(_clientFile, _clientText.Text.Replace("\r\n", "\n").Replace("\n", "\r\n"), Encoding.Latin1);
                _clientDirty = false;
                UpdateClientInfo();
                SetMessage("Saved " + _clientFile + (File.Exists(bak) ? " (backup: " + Path.GetFileName(bak) + ")" : string.Empty));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateClientInfo()
        {
            if (_clientFile == null)
            {
                _clientInfo.Text = string.Empty;
                return;
            }
            int line = _clientText.GetLineFromCharIndex(_clientText.SelectionStart);
            int col = _clientText.SelectionStart - _clientText.GetFirstCharIndexFromLine(line);
            _clientInfo.Text = $"{Path.GetFileName(_clientFile)}{(_clientDirty ? " *" : string.Empty)}   line {line + 1}, col {col + 1}   {_clientText.Lines.Length} lines";
        }

        private StringComparison Comparison => _matchCase.Checked ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        /// <summary>Plain text search (no regex – brackets and + are safe).</summary>
        private void FindNext(int dir)
        {
            string f = _find.Text;
            if (f.Length == 0)
            {
                return;
            }
            string t = _clientText.Text;
            int count = CountMatches(t, f);
            int index;
            if (dir > 0)
            {
                int start = Math.Min(t.Length, _clientText.SelectionStart + _clientText.SelectionLength);
                index = t.IndexOf(f, start, Comparison);
                if (index < 0)
                {
                    index = t.IndexOf(f, 0, Comparison);
                }
            }
            else
            {
                int start = _clientText.SelectionStart - 1;
                index = start >= 0 ? t.LastIndexOf(f, start, Comparison) : -1;
                if (index < 0)
                {
                    index = t.LastIndexOf(f, Comparison);
                }
            }
            if (index < 0)
            {
                SetMessage($"\"{f}\" not found.");
                return;
            }
            _clientText.Select(index, f.Length);
            _clientText.ScrollToCaret();
            _clientText.Focus();
            SetMessage($"{count} matches.");
        }

        private int CountMatches(string text, string f)
        {
            int n = 0;
            for (int i = text.IndexOf(f, Comparison); i >= 0; i = text.IndexOf(f, i + f.Length, Comparison))
            {
                n++;
            }
            return n;
        }

        private void ReplaceOne()
        {
            if (_find.Text.Length > 0 && string.Equals(_clientText.SelectedText, _find.Text, Comparison))
            {
                _clientText.SelectedText = _replace.Text;
            }
            FindNext(1);
        }

        private void ReplaceAll()
        {
            string f = _find.Text;
            if (f.Length == 0)
            {
                return;
            }
            int n = CountMatches(_clientText.Text, f);
            if (n == 0)
            {
                SetMessage($"\"{f}\" not found.");
                return;
            }
            var sb = new StringBuilder();
            string t = _clientText.Text;
            int pos = 0;
            for (int i = t.IndexOf(f, Comparison); i >= 0; i = t.IndexOf(f, i + f.Length, Comparison))
            {
                sb.Append(t, pos, i - pos).Append(_replace.Text);
                pos = i + f.Length;
            }
            sb.Append(t, pos, t.Length - pos);
            _clientText.Text = sb.ToString();
            SetMessage($"{n} replaced.");
        }

        #endregion

        #region Keyboard, help, closing

        private void ShowHelp() => ScriptCreatorHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ShowHelp();
                return true;
            }
            if (keyData == Keys.F5 && tabControl1.SelectedTab == tabTemplates)
            {
                TestTemplate();
                return true;
            }
            if (keyData == (Keys.Control | Keys.S))
            {
                if (tabControl1.SelectedTab == tabGenerator)
                {
                    SaveScript(false);
                }
                else if (tabControl1.SelectedTab == tabTemplates)
                {
                    SaveTemplate();
                }
                else
                {
                    SaveClientFile();
                }
                return true;
            }
            if (keyData == (Keys.Control | Keys.F) && tabControl1.SelectedTab == tabClientFiles)
            {
                _find.Focus();
                _find.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_templateDirty && _editing != null && !ConfirmTemplateDiscard())
            {
                e.Cancel = true;
                return;
            }
            if (_clientDirty && !ConfirmClientDiscard())
            {
                e.Cancel = true;
                return;
            }
            StoreValues();
            SaveSettings();
            base.OnFormClosing(e);
        }

        #endregion
    }
}

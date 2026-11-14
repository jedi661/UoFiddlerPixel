/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * ServUO script & setup for an animation:
 *  - script templates (monster, mage, animal, mount, sea creature, human npc, equipment item)
 *  - check of the client files (anim data, Bodyconv.def, mobtypes.txt, body.def, tiledata, gumps)
 *    and of ServUO (Data/bodyTable.cfg) with lines that can be written (backup *.bak)
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public sealed class ServUOSetupForm : Form
    {
        private const string ServUoPathFile = "ServUOPath.txt";

        private AnimationCandidate _source;
        private bool _filling;
        private List<SetupCheck> _checks = new List<SetupCheck>();

        private readonly ToolTip _tip = new ToolTip();

        private ComboBox _cmbTemplate;
        private TextBox _txtClass;
        private TextBox _txtName;
        private NumericUpDown _numBody;
        private ComboBox _cmbType;
        private TextBox _txtHue;
        private TextBox _txtSound;
        private NumericUpDown _numStrMin, _numStrMax, _numDexMin, _numDexMax, _numIntMin, _numIntMax;
        private NumericUpDown _numHitsMin, _numHitsMax, _numDamMin, _numDamMax;
        private NumericUpDown _numFame, _numKarma, _numArmor, _numSlots, _numTame;
        private CheckBox _chkTamable;
        private TextBox _txtMountItem;
        private TextBox _txtItemId;
        private ComboBox _cmbLayer;
        private ComboBox _cmbLoot;
        private TextBox _txtServUo;

        private TextBox _txtScript;
        private ListView _listChecks;
        private TextBox _txtLine;
        private Button _btnWriteLine;
        private Label _lblStatus;

        /// <param name="type">animation list type 0..3, 4 = equipment</param>
        /// <param name="source">file/index the animation was made in (or null)</param>
        public ServUOSetupForm(int body, int type, string name, AnimationCandidate source)
        {
            BuildUi();
            SetAnimation(body, type, name, source);
        }

        public void SetAnimation(int body, int type, string name, AnimationCandidate source)
        {
            _source = source;
            _filling = true;
            try
            {
                string clean = AnimationListStore.CleanName(name);
                // "dragons_ice_dragon (1234)" -> "ice dragon"
                string display = System.Text.RegularExpressions.Regex.Replace(clean, @"\s*\(\d+\)$", string.Empty);
                int underscore = display.IndexOf('_');
                if (underscore > 0 && underscore < display.Length - 1)
                {
                    display = display.Substring(underscore + 1);
                }

                display = display.Replace('_', ' ').Trim();
                if (display.Length == 0 || display.StartsWith("anim", StringComparison.OrdinalIgnoreCase))
                {
                    display = $"creature {body}";
                }

                _numBody.Value = Math.Max(0, Math.Min(65535, body));
                _cmbType.SelectedIndex = Math.Max(0, Math.Min(4, type));
                _txtName.Text = type == 4 ? display : (display.StartsWith("a ") || display.StartsWith("an ") ? display : "a " + display);
                _txtClass.Text = ServUOAnimationSetup.ToClassName(display);
                _cmbTemplate.SelectedIndex = (int)ServUOAnimationSetup.SuggestTemplate(type);
                _chkTamable.Checked = type == 2;
                if (type == 4 && TileData.ItemTable != null)
                {
                    // item whose tiledata uses this animation
                    int item = Array.FindIndex(TileData.ItemTable, d => d.Animation == body && d.Wearable);
                    if (item > 0)
                    {
                        _txtItemId.Text = $"0x{item:X4}";
                        int layer = TileData.ItemTable[item].Quality;
                        if (layer > 0 && layer < ServUOAnimationSetup.Layers.Length)
                        {
                            _cmbLayer.SelectedItem = ServUOAnimationSetup.Layers[layer];
                        }
                    }
                }
            }
            finally
            {
                _filling = false;
            }

            Text = $"ServUO script & setup – body {body} (0x{body:X})" +
                   (source != null && source.FileType > 0 ? $"  [{source.FileLabel} #{source.Index}]" : string.Empty);
            UpdateTemplateFields();
            Regenerate();
        }

        #region UI

        private void BuildUi()
        {
            Text = "ServUO script & setup";
            Icon = Options.GetFiddlerIcon();
            Size = new Size(1180, 760);
            MinimumSize = new Size(900, 560);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);

            // ── settings (left) ───────────────────────────────────────
            var grid = new TableLayoutPanel { AutoSize = true, ColumnCount = 3, Dock = DockStyle.Top, Padding = new Padding(6) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _cmbTemplate = Combo(new[] { "Monster (melee)", "Monster (mage)", "Animal (tamable)", "Mount", "Sea creature", "Human / NPC", "Equipment item" });
            _cmbTemplate.SelectedIndexChanged += (s, e) => { UpdateTemplateFields(); Regenerate(); };
            Row(grid, "Template:", _cmbTemplate);

            _txtClass = Text1();
            Row(grid, "Class name:", _txtClass);
            _txtName = Text1();
            Row(grid, "Name in game:", _txtName);

            _numBody = Num(0, 65535, 0);
            Row(grid, "Body id:", _numBody);
            _cmbType = Combo(AnimationListStore.TypeNames);
            _tip.SetToolTip(_cmbType, "Animation type: mobtypes.txt (client) and Data/bodyTable.cfg (ServUO)");
            Row(grid, "Animation type:", _cmbType);

            _txtHue = Text1("0");
            Row(grid, "Hue:", _txtHue);
            _txtSound = Text1("-1");
            _tip.SetToolTip(_txtSound, "BaseSoundID (decimal or 0x..), -1 = none");
            Row(grid, "Sound id:", _txtSound);

            _numStrMin = Num(1, 10000, 100); _numStrMax = Num(1, 10000, 150);
            Row(grid, "Str:", _numStrMin, _numStrMax);
            _numDexMin = Num(1, 10000, 50); _numDexMax = Num(1, 10000, 70);
            Row(grid, "Dex:", _numDexMin, _numDexMax);
            _numIntMin = Num(1, 10000, 30); _numIntMax = Num(1, 10000, 50);
            Row(grid, "Int:", _numIntMin, _numIntMax);
            _numHitsMin = Num(1, 100000, 80); _numHitsMax = Num(1, 100000, 120);
            Row(grid, "Hits:", _numHitsMin, _numHitsMax);
            _numDamMin = Num(0, 1000, 8); _numDamMax = Num(0, 1000, 14);
            Row(grid, "Damage:", _numDamMin, _numDamMax);
            _numFame = Num(-50000, 50000, 3000);
            _numKarma = Num(-50000, 50000, -3000);
            Row(grid, "Fame / Karma:", _numFame, _numKarma);
            _numArmor = Num(0, 1000, 30);
            Row(grid, "Virtual armor:", _numArmor);
            _cmbLoot = Combo(ServUOAnimationSetup.LootPacks);
            _cmbLoot.SelectedItem = "Average";
            Row(grid, "Loot pack:", _cmbLoot);

            _chkTamable = new CheckBox { Text = "Tamable", AutoSize = true, Margin = new Padding(3, 6, 3, 3) };
            _numSlots = Num(1, 5, 1);
            Row(grid, string.Empty, _chkTamable);
            _numTame = new NumericUpDown { Minimum = 0, Maximum = 120, DecimalPlaces = 1, Value = 50, Width = 80 };
            Row(grid, "Slots / min. taming:", _numSlots, _numTame);

            _txtMountItem = Text1("0x3EA0");
            _tip.SetToolTip(_txtMountItem, "Item id of the mount item (layer Mount). Its tiledata field \"Animation\" must be the body id.");
            Row(grid, "Mount item id:", _txtMountItem);

            _txtItemId = Text1("0x0000");
            Row(grid, "Item id:", _txtItemId);
            _cmbLayer = Combo(ServUOAnimationSetup.Layers.Skip(1).ToArray());
            _cmbLayer.SelectedItem = "OuterTorso";
            Row(grid, "Layer:", _cmbLayer);

            _txtServUo = Text1(LoadServUoPath());
            var btnBrowse = new Button { Text = "…", Width = 30 };
            btnBrowse.Click += (s, e) => BrowseServUo();
            Row(grid, "ServUO folder:", _txtServUo, btnBrowse);

            foreach (Control c in grid.Controls)
            {
                switch (c)
                {
                    case TextBox t: t.TextChanged += (s, e) => Regenerate(); break;
                    case NumericUpDown n: n.ValueChanged += (s, e) => Regenerate(); break;
                    case ComboBox cb when cb != _cmbTemplate: cb.SelectedIndexChanged += (s, e) => Regenerate(); break;
                    case CheckBox ch: ch.CheckedChanged += (s, e) => Regenerate(); break;
                }
            }

            var left = new Panel { Dock = DockStyle.Left, Width = 380, AutoScroll = true };
            left.Controls.Add(grid);

            // ── script tab ────────────────────────────────────────────
            _txtScript = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.5f),
                AcceptsTab = true
            };
            var scriptButtons = Buttons(
                Btn("Copy", (s, e) => CopyText(_txtScript.Text)),
                Btn("Save as…", (s, e) => SaveScriptAs()),
                Btn("Save into ServUO (Scripts/Custom/Animations)", (s, e) => SaveScriptIntoServUo()),
                Btn("Regenerate", (s, e) => Regenerate(true)));
            var tabScript = new TabPage("Script");
            tabScript.Controls.Add(_txtScript);
            tabScript.Controls.Add(scriptButtons);

            // ── setup tab ─────────────────────────────────────────────
            _listChecks = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false
            };
            _listChecks.Columns.Add("Status", 80);
            _listChecks.Columns.Add("Where", 60);
            _listChecks.Columns.Add("File / entry", 200);
            _listChecks.Columns.Add("Details", 700);
            _listChecks.SelectedIndexChanged += (s, e) => OnCheckSelected();
            _listChecks.ShowItemToolTips = true;

            _txtLine = new TextBox { Dock = DockStyle.Fill, Font = new Font("Consolas", 9.5f) };
            _btnWriteLine = Btn("Write line into the file…", (s, e) => WriteSelectedLine());
            _btnWriteLine.Enabled = false;
            var linePanel = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 34, ColumnCount = 3, Padding = new Padding(4) };
            linePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            linePanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linePanel.Controls.Add(new Label { Text = "Line:", AutoSize = true, Margin = new Padding(3, 6, 3, 3) }, 0, 0);
            linePanel.Controls.Add(_txtLine, 1, 0);
            linePanel.Controls.Add(_btnWriteLine, 2, 0);

            var checkButtons = Buttons(
                Btn("Check again", (s, e) => RunCheck()),
                Btn("Copy all lines", (s, e) => CopyAllLines()),
                Btn("Copy report", (s, e) => CopyText(Report())));

            var tabSetup = new TabPage("Setup check (client + server)");
            tabSetup.Controls.Add(_listChecks);
            tabSetup.Controls.Add(linePanel);
            tabSetup.Controls.Add(checkButtons);

            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(tabScript);
            tabs.TabPages.Add(tabSetup);
            tabs.SelectedIndexChanged += (s, e) => { if (tabs.SelectedTab == tabSetup) { RunCheck(); } };

            _lblStatus = new Label { Dock = DockStyle.Bottom, Height = 22, Padding = new Padding(6, 3, 0, 0), ForeColor = Color.DarkGreen };

            Controls.Add(tabs);
            Controls.Add(left);
            Controls.Add(_lblStatus);
        }

        private static void Row(TableLayoutPanel grid, string label, Control a, Control b = null)
        {
            int row = grid.RowCount;
            grid.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, row);
            grid.Controls.Add(a, 1, row);
            if (b != null)
            {
                grid.Controls.Add(b, 2, row);
            }

            grid.RowCount++;
        }

        private static ComboBox Combo(string[] items)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
            c.Items.AddRange(items.Cast<object>().ToArray());
            c.SelectedIndex = 0;
            return c;
        }

        private static TextBox Text1(string text = "") => new TextBox { Width = 170, Text = text };

        private static NumericUpDown Num(int min, int max, int value) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = 80 };

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static FlowLayoutPanel Buttons(params Button[] buttons)
        {
            var p = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(4) };
            p.Controls.AddRange(buttons.Cast<Control>().ToArray());
            return p;
        }

        private void UpdateTemplateFields()
        {
            var t = (ServUOTemplate)Math.Max(0, _cmbTemplate.SelectedIndex);
            bool item = t == ServUOTemplate.EquipmentItem;
            bool creatureOnly = !item;
            foreach (Control c in new Control[] { _numStrMin, _numStrMax, _numDexMin, _numDexMax, _numIntMin, _numIntMax, _numHitsMin, _numHitsMax,
                         _numDamMin, _numDamMax, _numFame, _numKarma, _numArmor, _cmbLoot, _txtSound, _chkTamable, _numSlots, _numTame })
            {
                c.Enabled = creatureOnly;
            }

            _txtMountItem.Enabled = t == ServUOTemplate.Mount;
            _txtItemId.Enabled = item;
            _cmbLayer.Enabled = item;
            if (!_filling && (t == ServUOTemplate.Animal || t == ServUOTemplate.Mount))
            {
                _chkTamable.Checked = true;
            }
        }

        #endregion

        #region Script

        private static int ParseInt(string text, int fallback)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex) ? hex : fallback;
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        }

        private ServUOScriptSettings Settings()
        {
            string cls = ServUOAnimationSetup.ToClassName(_txtClass.Text);
            return new ServUOScriptSettings
            {
                Template = (ServUOTemplate)Math.Max(0, _cmbTemplate.SelectedIndex),
                ClassName = cls,
                DisplayName = _txtName.Text.Trim().Length == 0 ? cls : _txtName.Text.Trim(),
                Body = (int)_numBody.Value,
                Hue = ParseInt(_txtHue.Text, 0),
                SoundId = ParseInt(_txtSound.Text, -1),
                StrMin = (int)Math.Min(_numStrMin.Value, _numStrMax.Value),
                StrMax = (int)Math.Max(_numStrMin.Value, _numStrMax.Value),
                DexMin = (int)Math.Min(_numDexMin.Value, _numDexMax.Value),
                DexMax = (int)Math.Max(_numDexMin.Value, _numDexMax.Value),
                IntMin = (int)Math.Min(_numIntMin.Value, _numIntMax.Value),
                IntMax = (int)Math.Max(_numIntMin.Value, _numIntMax.Value),
                HitsMin = (int)Math.Min(_numHitsMin.Value, _numHitsMax.Value),
                HitsMax = (int)Math.Max(_numHitsMin.Value, _numHitsMax.Value),
                DamageMin = (int)Math.Min(_numDamMin.Value, _numDamMax.Value),
                DamageMax = (int)Math.Max(_numDamMin.Value, _numDamMax.Value),
                Fame = (int)_numFame.Value,
                Karma = (int)_numKarma.Value,
                VirtualArmor = (int)_numArmor.Value,
                Tamable = _chkTamable.Checked,
                ControlSlots = (int)_numSlots.Value,
                MinTameSkill = (double)_numTame.Value,
                MountItemId = ParseInt(_txtMountItem.Text, 0x3EA0),
                ItemId = ParseInt(_txtItemId.Text, 0),
                Layer = _cmbLayer.SelectedItem as string ?? "OuterTorso",
                LootPack = _cmbLoot.SelectedItem as string ?? "Average"
            };
        }

        private bool _scriptEdited;

        private void Regenerate(bool force = false)
        {
            if (_filling || _txtScript == null)
            {
                return;
            }

            if (_scriptEdited && !force)
            {
                _lblStatus.Text = "The script was edited by hand – \"Regenerate\" replaces it with the template.";
                return;
            }

            _txtScript.TextChanged -= OnScriptEdited;
            _txtScript.Text = ServUOAnimationSetup.BuildScript(Settings()).Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
            _txtScript.TextChanged += OnScriptEdited;
            _scriptEdited = false;
        }

        private void OnScriptEdited(object sender, EventArgs e) => _scriptEdited = true;

        private void SaveScriptAs()
        {
            using (var dlg = new SaveFileDialog { Filter = "C# script (*.cs)|*.cs", FileName = ServUOAnimationSetup.ToClassName(_txtClass.Text) + ".cs" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                File.WriteAllText(dlg.FileName, _txtScript.Text, Encoding.UTF8);
                _lblStatus.Text = $"Script saved: {dlg.FileName}";
            }
        }

        private void SaveScriptIntoServUo()
        {
            string root = _txtServUo.Text.Trim();
            if (!Directory.Exists(Path.Combine(root, "Scripts")))
            {
                MessageBox.Show(this, "Please choose the ServUO folder (it contains the folder \"Scripts\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                BrowseServUo();
                root = _txtServUo.Text.Trim();
                if (!Directory.Exists(Path.Combine(root, "Scripts")))
                {
                    return;
                }
            }

            string dir = Path.Combine(root, "Scripts", "Custom", "Animations");
            string file = Path.Combine(dir, ServUOAnimationSetup.ToClassName(_txtClass.Text) + ".cs");
            if (File.Exists(file) && MessageBox.Show(this, $"{file}\nalready exists. Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(dir);
                if (File.Exists(file))
                {
                    File.Copy(file, file + ".bak", true);
                }

                File.WriteAllText(file, _txtScript.Text, Encoding.UTF8);
                _lblStatus.Text = $"Script saved: {file}  – restart the server, then [add {ServUOAnimationSetup.ToClassName(_txtClass.Text)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Setup check

        private void RunCheck()
        {
            int body = (int)_numBody.Value;
            int sourceType = _source?.FileType ?? 0;
            int sourceIndex = _source?.Index ?? -1;
            try
            {
                _checks = ServUOAnimationSetup.Check(body, _cmbType.SelectedIndex, sourceType, sourceIndex, Settings(), _txtServUo.Text.Trim());
            }
            catch (Exception ex)
            {
                _checks = new List<SetupCheck>
                {
                    new SetupCheck { Group = "-", Title = "Error", Status = SetupStatus.Warning, Detail = ex.Message }
                };
            }

            _listChecks.BeginUpdate();
            _listChecks.Items.Clear();
            foreach (SetupCheck c in _checks)
            {
                var item = new ListViewItem(StatusText(c.Status))
                {
                    Tag = c,
                    ForeColor = c.Status switch
                    {
                        SetupStatus.Ok => Color.DarkGreen,
                        SetupStatus.Missing => Color.Firebrick,
                        SetupStatus.Warning => Color.DarkOrange,
                        _ => Color.SteelBlue
                    },
                    ToolTipText = c.Detail
                };
                item.SubItems.Add(c.Group);
                item.SubItems.Add(c.Title);
                item.SubItems.Add(c.Detail);
                _listChecks.Items.Add(item);
            }

            _listChecks.EndUpdate();
            int open = _checks.Count(c => c.Status == SetupStatus.Missing || c.Status == SetupStatus.Warning);
            _lblStatus.Text = open == 0 ? "All checked entries are present." : $"{open} entr{(open == 1 ? "y needs" : "ies need")} attention.";
            _txtLine.Text = string.Empty;
            _btnWriteLine.Enabled = false;
        }

        private static string StatusText(SetupStatus s) => s switch
        {
            SetupStatus.Ok => "✓ OK",
            SetupStatus.Missing => "✗ missing",
            SetupStatus.Warning => "! check",
            _ => "i info"
        };

        private SetupCheck SelectedCheck => _listChecks.SelectedItems.Count > 0 ? _listChecks.SelectedItems[0].Tag as SetupCheck : null;

        private void OnCheckSelected()
        {
            SetupCheck c = SelectedCheck;
            _txtLine.Text = c?.SuggestedLine ?? string.Empty;
            _btnWriteLine.Enabled = c != null && c.File != SetupFile.None && !string.IsNullOrEmpty(c.SuggestedLine);
        }

        private void WriteSelectedLine()
        {
            SetupCheck c = SelectedCheck;
            if (c == null || c.File == SetupFile.None)
            {
                return;
            }

            string line = _txtLine.Text.Trim();
            int body = (int)_numBody.Value;
            string first = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!int.TryParse(first, out int id))
            {
                MessageBox.Show(this, "The line has to start with the body id.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string target = c.File switch
            {
                SetupFile.Bodyconv => ChooseClientFile("Bodyconv.def"),
                SetupFile.Mobtypes => ChooseClientFile("mobtypes.txt"),
                SetupFile.BodyTable => ServUoBodyTable(),
                _ => null
            };

            if (target == null)
            {
                return;
            }

            if (c.File == SetupFile.BodyTable)
            {
                line = line.Replace(' ', '\t'); // ServUO splits only on TAB
            }

            Dictionary<int, string> existing = ServUOAnimationSetup.ReadIdLines(target);
            if (existing.TryGetValue(id, out string old) &&
                MessageBox.Show(this, $"{Path.GetFileName(target)} already has a line for {id}:\n\n{old}\n\nReplace it with\n\n{line}", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                ServUOAnimationSetup.WriteIdLine(target, id, line);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Writing failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (c.File == SetupFile.Bodyconv && string.Equals(Path.GetFullPath(target), Path.GetFullPath(Files.GetFilePath("Bodyconv.def") ?? string.Empty), StringComparison.OrdinalIgnoreCase))
            {
                BodyConverter.Initialize(); // the client file was changed: use the new mapping at once
            }

            _lblStatus.Text = $"Written into {target} (backup: {Path.GetFileName(target)}.bak)";
            RunCheck();
            if (body != id)
            {
                _lblStatus.Text += $"  – note: the line is for id {id}, not for body {body}.";
            }
        }

        /// <summary>The client file, or a copy in the output folder (asks)</summary>
        private string ChooseClientFile(string name)
        {
            string client = Files.GetFilePath(name);
            string output = string.IsNullOrEmpty(Options.OutputPath) ? null : Path.Combine(Options.OutputPath, name);

            if (client == null && output == null)
            {
                MessageBox.Show(this, $"{name} was not found and no output folder is set.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (client != null && output != null)
            {
                DialogResult r = MessageBox.Show(this,
                    $"Where should the line be written?\n\nYes = client file\n{client}\n\nNo = copy in the output folder\n{output}",
                    name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    return null;
                }

                if (r == DialogResult.Yes)
                {
                    return client;
                }
            }
            else if (client != null)
            {
                return client;
            }

            // output folder: start with a copy of the client file
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            if (!File.Exists(output) && client != null)
            {
                File.Copy(client, output);
            }

            return output;
        }

        private string ServUoBodyTable()
        {
            string root = _txtServUo.Text.Trim();
            string path = string.IsNullOrEmpty(root) ? null : Path.Combine(root, "Data", "bodyTable.cfg");
            if (path == null || !Directory.Exists(Path.Combine(root, "Data")))
            {
                MessageBox.Show(this, "Please choose the ServUO folder (it contains the folder \"Data\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                BrowseServUo();
                root = _txtServUo.Text.Trim();
                path = Path.Combine(root, "Data", "bodyTable.cfg");
                if (!Directory.Exists(Path.Combine(root, "Data")))
                {
                    return null;
                }
            }

            return path;
        }

        private void CopyAllLines()
        {
            var sb = new StringBuilder();
            foreach (IGrouping<SetupFile, SetupCheck> group in _checks.Where(c => !string.IsNullOrEmpty(c.SuggestedLine)).GroupBy(c => c.File))
            {
                sb.AppendLine(group.Key switch
                {
                    SetupFile.Bodyconv => "# Bodyconv.def (client)",
                    SetupFile.Mobtypes => "# mobtypes.txt (client)",
                    SetupFile.BodyTable => "# Data/bodyTable.cfg (ServUO, TAB separated)",
                    _ => "#"
                });
                foreach (SetupCheck c in group)
                {
                    sb.AppendLine(c.SuggestedLine);
                }

                sb.AppendLine();
            }

            CopyText(sb.Length == 0 ? "(nothing to add)" : sb.ToString());
        }

        private string Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Body {_numBody.Value} – {_txtName.Text} ({_cmbType.SelectedItem})");
            foreach (SetupCheck c in _checks)
            {
                sb.AppendLine($"[{StatusText(c.Status)}] {c.Group} / {c.Title}: {c.Detail}");
                if (!string.IsNullOrEmpty(c.SuggestedLine))
                {
                    sb.AppendLine($"      line: {c.SuggestedLine}");
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Helpers

        private void CopyText(string text)
        {
            try
            {
                Clipboard.SetText(string.IsNullOrEmpty(text) ? " " : text);
                _lblStatus.Text = "Copied to the clipboard.";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Copy failed: {ex.Message}";
            }
        }

        private void BrowseServUo()
        {
            using (var dlg = new FolderBrowserDialog { Description = "ServUO folder (contains Scripts and Data)" })
            {
                if (Directory.Exists(_txtServUo.Text))
                {
                    dlg.SelectedPath = _txtServUo.Text;
                }

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _txtServUo.Text = dlg.SelectedPath;
                    SaveServUoPath(dlg.SelectedPath);
                }
            }
        }

        private static string LoadServUoPath()
        {
            try
            {
                string file = Path.Combine(Options.AppDataPath, ServUoPathFile);
                return File.Exists(file) ? File.ReadAllText(file).Trim() : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static void SaveServUoPath(string path)
        {
            try
            {
                File.WriteAllText(Path.Combine(Options.AppDataPath, ServUoPathFile), path ?? string.Empty);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tip.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}

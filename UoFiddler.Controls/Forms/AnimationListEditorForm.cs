using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationListEditorForm : Form
    {
        // ── Farben / Design ─────────────────────────────────────────────────
        private static readonly Color BgDark = Color.FromArgb(22, 26, 34);
        private static readonly Color BgPanel = Color.FromArgb(30, 36, 46);
        private static readonly Color BgCard = Color.FromArgb(38, 46, 60);
        private static readonly Color AccentBlue = Color.FromArgb(64, 156, 255);
        private static readonly Color AccentGreen = Color.FromArgb(80, 210, 130);
        private static readonly Color AccentRed = Color.FromArgb(230, 80, 80);
        private static readonly Color AccentOrange = Color.FromArgb(255, 165, 60);
        private static readonly Color TextLight = Color.FromArgb(220, 228, 240);
        private static readonly Color TextDim = Color.FromArgb(130, 145, 165);

        // ── Zustand ─────────────────────────────────────────────────────────
        private string _fileName;
        private bool _modified = false;
        private int _browseId = 0;
        private AnimationFrame[] _previewFrames;
        private int _previewFrameIdx = 0;
        private int _previewFacing = 1;
        private int _previewAction = 0;
        private Timer _animTimer;

        // ── Action-Namen ─────────────────────────────────────────────────────
        private static readonly string[][] ActionNames =
        {
            new[] { "Walk","Idle","Die1","Die2","Attack1","Attack2","Attack3",
                    "AttackBow","AttackCrossBow","AttackThrow","GetHit","Pillage",
                    "Stomp","Cast2","Cast3","BlockRight","BlockLeft","Idle",
                    "Fidget","Fly","TakeOff","GetHitInAir" },
            new[] { "Walk","Run","Idle","Idle","Fidget","Attack1","Attack2","GetHit","Die1" },
            new[] { "Walk","Run","Idle","Eat","Alert","Attack1","Attack2","GetHit",
                    "Die1","Idle","Fidget","LieDown","Die2" },
            new[] { "Walk_01","WalkStaff_01","Run_01","RunStaff_01","Idle_01","Idle_01",
                    "Fidget_Yawn_Stretch_01","CombatIdle1H_01","CombatIdle1H_01",
                    "AttackSlash1H_01","AttackPierce1H_01","AttackBash1H_01",
                    "AttackBash2H_01","AttackSlash2H_01","AttackPierce2H_01",
                    "CombatAdvance_1H_01","Spell1","Spell2","AttackBow_01",
                    "AttackCrossbow_01","GetHit_Fr_Hi_01","Die_Hard_Fwd_01",
                    "Die_Hard_Back_01","Horse_Walk_01","Horse_Run_01","Horse_Idle_01",
                    "Horse_Attack1_HSlashRight_01","Horse_AttackBow_01",
                    "Horse_AttackCrossbow_01","Horse_Attack2_HSlashRight_01",
                    "Block_Shield_Hard_01","Punch_Punch_Jab_01","Bow_Lesser_01",
                    "Salute_Armed1h_01","Ingest_Eat_01" }
        };

        public AnimationListEditorForm(string fileName)
        {
            _fileName = fileName;
            InitializeComponent();
            ApplyTheme();
            SetupAnimTimer();
            if (!string.IsNullOrEmpty(_fileName) && File.Exists(_fileName))
                LoadXml(_fileName);
            UpdateTitle();
        }

        // ════════════════════════════════════════════════════════════════════
        //  Theme
        // ════════════════════════════════════════════════════════════════════

        private void ApplyTheme()
        {
            BackColor = BgDark;
            ForeColor = TextLight;

            toolStrip.BackColor = BgPanel;
            toolStrip.ForeColor = TextLight;
            toolStrip.Renderer = new DarkToolStripRenderer();

            richTextBox.BackColor = Color.FromArgb(18, 22, 30);
            richTextBox.ForeColor = Color.FromArgb(200, 215, 235);
            richTextBox.Font = new Font("Cascadia Code", 9.5f);
            richTextBox.BorderStyle = BorderStyle.None;

            statusStrip.BackColor = BgPanel;
            statusStrip.ForeColor = TextDim;

            pnlPreview.BackColor = BgPanel;
            picPreview.BackColor = Color.Black;

            pnlBrowse.BackColor = BgPanel;
            pnlEdit.BackColor = BgCard;
        }

        // ════════════════════════════════════════════════════════════════════
        //  Load/save XML
        // ════════════════════════════════════════════════════════════════════

        private void LoadXml(string path)
        {
            try
            {
                richTextBox.Text = FormatXml(File.ReadAllText(path));
                ApplySyntaxHighlight();
                _fileName = path;
                _modified = false;
                UpdateStatus($"Loaded: {path}");
                UpdateTitle();
                PopulateEntryList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatXml(string xml)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    Encoding = Encoding.UTF8
                };
                using (XmlWriter writer = XmlWriter.Create(sb, settings))
                    doc.Save(writer);
                return sb.ToString().TrimStart('\uFEFF');
            }
            catch
            {
                return xml;
            }
        }

        private void ApplySyntaxHighlight()
        {
            richTextBox.SuspendLayout();
            int selStart = richTextBox.SelectionStart;
            int selLen = richTextBox.SelectionLength;

            richTextBox.SelectAll();
            richTextBox.SelectionColor = Color.FromArgb(200, 215, 235);

            HighlightPattern(@"<[^>]+>", Color.FromArgb(86, 182, 255));
            HighlightPattern(@"\b(name|body|type)\b(?=\s*=)", AccentOrange);
            HighlightPattern(@"""[^""]*""", AccentGreen);
            HighlightPattern(@"<!--.*?-->", Color.FromArgb(100, 115, 130));
            HighlightPattern(@"<\?xml[^?]*\?>", Color.FromArgb(180, 130, 255));

            richTextBox.Select(selStart, selLen);
            richTextBox.ResumeLayout();
        }

        private void HighlightPattern(string pattern, Color color)
        {
            foreach (Match m in Regex.Matches(richTextBox.Text, pattern, RegexOptions.Singleline))
            {
                richTextBox.Select(m.Index, m.Length);
                richTextBox.SelectionColor = color;
            }
        }

        private void SaveXml(string path)
        {
            try
            {
                File.WriteAllText(path, richTextBox.Text, Encoding.UTF8);
                _modified = false;
                UpdateStatus($"Saved: {path}");
                UpdateTitle();
                MessageBox.Show("XML Saved.", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  Entry-Liste
        // ════════════════════════════════════════════════════════════════════

        private void PopulateEntryList()
        {
            listEntries.Items.Clear();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(richTextBox.Text);
                XmlElement root = doc["Graphics"];
                if (root == null) return;

                foreach (XmlElement el in root.SelectNodes("Mob|Equip"))
                {
                    string name = el.GetAttribute("name");
                    string body = el.GetAttribute("body");
                    string type = el.GetAttribute("type");
                    string tag = el.Name;
                    
                    var item = new ListViewItem(new[] { body, tag, type, name })
                    {
                        ForeColor = tag == "Equip" ? AccentOrange : TextLight
                    };
                    listEntries.Items.Add(item);
                }
                lblEntryCount.Text = $"{listEntries.Items.Count} Einträge";
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════════════
        //  MUL-Scanner / Browser
        // ════════════════════════════════════════════════════════════════════

        private void BrowseToId(int id)
        {
            _browseId = Math.Max(0, Math.Min(0xFFFF, id));
            numBrowseId.Value = _browseId;

            string fileName = Animations.GetFileName(_browseId) ?? "–";
            lblBrowseFile.Text = $"Datei: {fileName}";

            var hasAction = new List<string>();
            for (int type = 0; type <= 3; type++)
            {
                int cnt = ActionNames[type].Length;
                for (int a = 0; a < cnt; a++)
                {
                    if (Animations.IsActionDefined(_browseId, a, 0))
                        hasAction.Add($"Typ{type}/{ActionNames[type][a]}");
                }
                if (hasAction.Count > 0) break;
            }

            bool hasAny = hasAction.Count > 0;
            lblBrowseHasAnim.Text = hasAny ? "✔ Animation available" : "✘ No animation";
            lblBrowseHasAnim.ForeColor = hasAny ? AccentGreen : AccentRed;

            bool inXml = IsBodyInXml(_browseId);
            lblBrowseInXml.Text = inXml ? "✔ Present in XML" : "✘ Not in XML";
            lblBrowseInXml.ForeColor = inXml ? AccentGreen : AccentOrange;

            LoadPreview(_browseId, _previewAction, _previewFacing);
        }

        private bool IsBodyInXml(int bodyId)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(richTextBox.Text);
                XmlElement root = doc["Graphics"];
                if (root == null) return false;
                foreach (XmlElement el in root.SelectNodes("Mob|Equip"))
                    if (int.TryParse(el.GetAttribute("body"), out int b) && b == bodyId)
                        return true;
            }
            catch { }
            return false;
        }

        // ════════════════════════════════════════════════════════════════════
        //  Animation preview
        // ════════════════════════════════════════════════════════════════════

        private void SetupAnimTimer()
        {
            _animTimer = new Timer { Interval = 150 };
            _animTimer.Tick += OnAnimTick;
            _animTimer.Start();
        }

        private void LoadPreview(int bodyId, int action, int facing)
        {
            _previewFrames = null;
            _previewFrameIdx = 0;
            int hue = 0;
            try
            {
                _previewFrames = Animations.GetAnimation(bodyId, action, facing,
                                     ref hue, false, false);
            }
            catch { }
            DrawPreviewFrame();
            lblPreviewInfo.Text =
                $"Body: {bodyId}  Action: {action}  Facing: {facing}" +
                (_previewFrames != null ? $"  Frames: {_previewFrames.Length}" : "  [no frames]");
        }

        private void DrawPreviewFrame()
        {
            if (_previewFrames == null || _previewFrames.Length == 0)
            {
                picPreview.Image = null;
                return;
            }
            var frame = _previewFrames[_previewFrameIdx % _previewFrames.Length];
            if (frame?.Bitmap == null) { picPreview.Image = null; return; }
            var old = picPreview.Image;
            picPreview.Image = new Bitmap(frame.Bitmap);
            old?.Dispose();
        }

        private void OnAnimTick(object sender, EventArgs e)
        {
            if (_previewFrames == null || _previewFrames.Length == 0) return;
            _previewFrameIdx = (_previewFrameIdx + 1) % _previewFrames.Length;
            DrawPreviewFrame();
        }

        // ════════════════════════════════════════════════════════════════════
        //  Auto-Cleanup
        // ════════════════════════════════════════════════════════════════════

        private void RunCleanup(bool stepByStep)
        {
            XmlDocument doc = new XmlDocument();
            try { doc.LoadXml(richTextBox.Text); }
            catch (XmlException ex)
            {
                MessageBox.Show($"XML-Error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            XmlElement root = doc["Graphics"];
            if (root == null) return;

            var toRemove = new List<XmlElement>();
            foreach (XmlElement el in root.SelectNodes("Mob|Equip"))
            {
                if (!int.TryParse(el.GetAttribute("body"), out int bodyId)) continue;
                bool hasAny = false;
                for (int t = 0; t <= 3 && !hasAny; t++)
                    for (int a = 0; a < ActionNames[t].Length && !hasAny; a++)
                        if (Animations.IsActionDefined(bodyId, a, 0))
                            hasAny = true;
                if (!hasAny)
                    toRemove.Add(el);
            }

            if (toRemove.Count == 0)
            {
                MessageBox.Show("No empty entries found.", "Cleanup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (stepByStep)
            {
                using var dlg = new CleanupStepForm(toRemove, doc, root);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    richTextBox.Text = FormatXml(GetXmlString(doc));
                    ApplySyntaxHighlight();
                    _modified = true;
                    PopulateEntryList();
                    UpdateStatus($"Cleanup: {dlg.RemovedCount} Entries removed");
                }
            }
            else
            {
                DialogResult confirm = MessageBox.Show(
                    $"{toRemove.Count} Entries without animation found.\nRemove all?",
                    "Auto-Cleanup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;

                foreach (var el in toRemove)
                    root.RemoveChild(el);

                richTextBox.Text = FormatXml(GetXmlString(doc));
                ApplySyntaxHighlight();
                _modified = true;
                PopulateEntryList();
                UpdateStatus($"Cleanup: {toRemove.Count} Entries removed");
                MessageBox.Show($"{toRemove.Count} Entries removed.", "Cleanup",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string GetXmlString(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings s = new XmlWriterSettings
            { Indent = true, IndentChars = "  ", NewLineChars = "\r\n" };
            using (XmlWriter w = XmlWriter.Create(sb, s))
                doc.Save(w);
            return sb.ToString().TrimStart('\uFEFF');
        }

        // ════════════════════════════════════════════════════════════════════
        //  Edit entry
        // ════════════════════════════════════════════════════════════════════

        private void EditSelectedEntry()
        {
            if (listEntries.SelectedItems.Count == 0) return;
            var item = listEntries.SelectedItems[0];
            int bodyId = int.Parse(item.SubItems[0].Text);

            using var editDlg = new EntryEditDialog(
                bodyId,
                item.SubItems[3].Text,
                int.Parse(item.SubItems[2].Text),
                item.SubItems[1].Text == "Equip");

            if (editDlg.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(richTextBox.Text);
                XmlElement root = doc["Graphics"];
                                
                if (root == null) return;

                foreach (XmlElement el in root.SelectNodes("Mob|Equip"))
                {
                    if (!int.TryParse(el.GetAttribute("body"), out int b) || b != bodyId) continue;
                    el.SetAttribute("name", editDlg.EntryName);
                    el.SetAttribute("type", editDlg.EntryType.ToString());
                    if ((editDlg.IsEquip && el.Name == "Mob") ||
                        (!editDlg.IsEquip && el.Name == "Equip"))
                    {
                        XmlElement newEl = doc.CreateElement(editDlg.IsEquip ? "Equip" : "Mob");
                        foreach (XmlAttribute attr in el.Attributes)
                            newEl.SetAttribute(attr.Name, attr.Value);
                        root.ReplaceChild(newEl, el);
                    }
                    break;
                }
                richTextBox.Text = FormatXml(GetXmlString(doc));
                ApplySyntaxHighlight();
                _modified = true;
                PopulateEntryList();
                UpdateStatus($"Entry Body {bodyId} updated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  Find & Replace
        // ════════════════════════════════════════════════════════════════════

        private int _lastSearchIdx = -1;

        private void DoSearch(bool searchForward = true)
        {
            string term = txtSearch.Text;
            if (string.IsNullOrEmpty(term)) return;
            string text = richTextBox.Text;
            int start = searchForward
                ? (_lastSearchIdx >= 0 ? _lastSearchIdx + 1 : 0)
                : (_lastSearchIdx > 0 ? _lastSearchIdx - 1 : text.Length - 1);

            int idx = searchForward
                ? text.IndexOf(term, start, StringComparison.OrdinalIgnoreCase)
                : text.LastIndexOf(term, start, StringComparison.OrdinalIgnoreCase);

            if (idx < 0)
            {
                idx = searchForward
                    ? text.IndexOf(term, 0, StringComparison.OrdinalIgnoreCase)
                    : text.LastIndexOf(term, StringComparison.OrdinalIgnoreCase);
            }

            if (idx >= 0)
            {
                _lastSearchIdx = idx;
                richTextBox.Select(idx, term.Length);
                richTextBox.ScrollToCaret();
                richTextBox.Focus();
                UpdateStatus($"Found at position {idx}");
            }
            else
            {
                UpdateStatus("Not found.");
            }
        }

        private void DoReplace()
        {
            if (string.IsNullOrEmpty(txtSearch.Text)) return;
            richTextBox.Text = Regex.Replace(
                richTextBox.Text,
                Regex.Escape(txtSearch.Text),
                txtReplace.Text,
                RegexOptions.IgnoreCase);
            ApplySyntaxHighlight();
            _modified = true;
            UpdateStatus("Replace completed.");
        }

        // ════════════════════════════════════════════════════════════════════
        //  Auxiliary methods
        // ════════════════════════════════════════════════════════════════════

        private void UpdateTitle()
        {
            string name = string.IsNullOrEmpty(_fileName)
                ? "Keine Datei"
                : Path.GetFileName(_fileName);
            Text = $"AnimationList XML Editor  –  {name}{(_modified ? " *" : "")}";
        }

        private void UpdateStatus(string msg) => lblStatus.Text = msg;

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_modified)
            {
                var r = MessageBox.Show(
                    "Unsaved changes exist.\nSave now?",
                    "Close", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                    SaveXml(_fileName);
                else if (r == DialogResult.Cancel)
                    e.Cancel = true;
            }
            _animTimer?.Stop();
            _animTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  DarkToolStripRenderer
    // ══════════════════════════════════════════════════════════════════════════

    internal class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        public DarkToolStripRenderer() : base(new DarkColorTable()) { }
    }

    internal class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => Color.FromArgb(30, 36, 46);
        public override Color ToolStripGradientMiddle => Color.FromArgb(30, 36, 46);
        public override Color ToolStripGradientEnd => Color.FromArgb(30, 36, 46);
        public override Color MenuItemSelected => Color.FromArgb(50, 60, 78);
        public override Color MenuItemBorder => Color.FromArgb(64, 156, 255);
        public override Color ButtonSelectedHighlight => Color.FromArgb(50, 60, 78);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  EntryEditDialog
    // ══════════════════════════════════════════════════════════════════════════

    internal class EntryEditDialog : Form
    {
        public string EntryName { get; private set; }
        public int EntryType { get; private set; }
        public bool IsEquip { get; private set; }

        public EntryEditDialog(int bodyId, string name, int type, bool isEquip)
        {
            Text = $"Edit entry – Body {bodyId}";
            Size = new Size(420, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(30, 36, 46);
            ForeColor = Color.FromArgb(220, 228, 240);
            
            var lblName = new Label { Text = "Name:", Location = new Point(12, 18), Size = new Size(60, 20) };
            var txtName = new TextBox
            {
                Text = name,
                Location = new Point(80, 15),
                Size = new Size(310, 22),
                BackColor = Color.FromArgb(22, 26, 34),
                ForeColor = Color.FromArgb(200, 215, 235)
            };

            var lblType = new Label { Text = "Typ:", Location = new Point(12, 55), Size = new Size(60, 20) };
            var cmbType = new ComboBox
            {
                Location = new Point(80, 52),
                Size = new Size(200, 22),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(22, 26, 34),
                ForeColor = Color.FromArgb(200, 215, 235)
            };
            cmbType.Items.AddRange(new object[] { "0 – Monster", "1 – See", "2 – Tier", "3 – Human/Equipment" });
            cmbType.SelectedIndex = Math.Min(type, 3);

            var chkEquip = new CheckBox
            {
                Text = "As <Equip> save",
                Location = new Point(80, 90),
                Size = new Size(200, 22),
                Checked = isEquip,
                ForeColor = Color.FromArgb(220, 228, 240)
            };

            var btnOk = new Button
            {
                Text = "OK",
                Location = new Point(200, 140),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(64, 156, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(300, 140),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(38, 46, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            btnOk.Click += (s, e) =>
            {
                EntryName = txtName.Text.Trim();
                EntryType = cmbType.SelectedIndex;
                IsEquip = chkEquip.Checked;
            };

            Controls.AddRange(new Control[] { lblName, txtName, lblType, cmbType, chkEquip, btnOk, btnCancel });
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CleanupStepForm
    // ══════════════════════════════════════════════════════════════════════════

    internal class CleanupStepForm : Form
    {
        public int RemovedCount { get; private set; } = 0;

        private readonly List<XmlElement> _items;
        private readonly XmlDocument _doc;
        private readonly XmlElement _root;
        private int _idx = 0;

        private Label _lblInfo;
        private Button _btnRemove, _btnKeep, _btnRemoveAll, _btnDone;

        public CleanupStepForm(List<XmlElement> items, XmlDocument doc, XmlElement root)
        {
            _items = items; _doc = doc; _root = root;
            Text = "Cleanup – Step by Step";
            Size = new Size(480, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(22, 26, 34);
            ForeColor = Color.FromArgb(220, 228, 240);

            _lblInfo = new Label
            {
                Location = new Point(12, 12),
                Size = new Size(450, 80),
                ForeColor = Color.FromArgb(220, 228, 240)
            };

            _btnRemove = MakeBtn("✘ Remove", new Point(12, 140), Color.FromArgb(180, 60, 60));
            _btnKeep = MakeBtn("→ Keep", new Point(120, 140), Color.FromArgb(50, 60, 78));
            _btnRemoveAll = MakeBtn("⚡ Remove all", new Point(228, 140), Color.FromArgb(64, 100, 180));
            _btnDone = MakeBtn("✔ Ready", new Point(360, 140), Color.FromArgb(64, 156, 255));

            _btnRemove.Click += (s, e) => { _root.RemoveChild(_items[_idx]); RemovedCount++; Next(); };
            _btnKeep.Click += (s, e) => Next();
            _btnRemoveAll.Click += (s, e) =>
            {
                for (int i = _idx; i < _items.Count; i++)
                { _root.RemoveChild(_items[i]); RemovedCount++; }
                DialogResult = DialogResult.OK;
                Close();
            };
            _btnDone.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };

            Controls.AddRange(new Control[] { _lblInfo, _btnRemove, _btnKeep, _btnRemoveAll, _btnDone });
            ShowCurrent();
        }

        private Button MakeBtn(string txt, Point loc, Color bg) =>
            new Button
            {
                Text = txt,
                Location = loc,
                Size = new Size(100, 30),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

        private void ShowCurrent()
        {
            if (_idx >= _items.Count) { DialogResult = DialogResult.OK; Close(); return; }
            var el = _items[_idx];
            _lblInfo.Text =
                $"Eintrag {_idx + 1} von {_items.Count}\n\n" +
                $"  <{el.Name}  name=\"{el.GetAttribute("name")}\"\n" +
                $"           body=\"{el.GetAttribute("body")}\"  type=\"{el.GetAttribute("type")}\" />\n\n" +
                $"  → No animation found in the .mul files.";
        }

        private void Next() { _idx++; ShowCurrent(); }
    }
}
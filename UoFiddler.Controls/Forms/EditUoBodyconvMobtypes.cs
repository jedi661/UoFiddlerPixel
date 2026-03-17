using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;
using System.Media;

namespace UoFiddler.Controls.Forms
{
    public partial class EditUoBodyconvMobtypes : Form
    {
        // -------------------------------------------------------------------------
        // Fields
        // -------------------------------------------------------------------------
        private int searchStartIndex = 0;
        private string currentFilePath;
        private int newIdCount = 0;

        // Constants for animation creature types
        const int cHighDetail = 110;
        const int cLowDetail = 65;
        const int cHuman = 175;
        const int cHighDetailOLd = 1;
        const int cLowDetailOld = 2;
        const int cHumanOld = 3;

        // =========================================================================
        // Constructor
        // =========================================================================
        public EditUoBodyconvMobtypes()
        {
            InitializeComponent();
            textBoxPfad.Text = Properties.Settings.Default.LastPath;

            // Initialize backup checkbox state — stored in a small local file
            // so no custom Setting entry is required in Properties/Settings.settings
            chkBackup.Checked = LoadBackupEnabled();
        }

        // ── Backup-flag persistence (avoids needing a custom Setting entry) ──────
        private static string BackupFlagFile =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings", "BackupEnabled.flag");

        private static bool LoadBackupEnabled()
        {
            try { return File.Exists(BackupFlagFile) && File.ReadAllText(BackupFlagFile).Trim() == "1"; }
            catch { return false; }
        }

        private static void SaveBackupEnabled(bool value)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BackupFlagFile));
                File.WriteAllText(BackupFlagFile, value ? "1" : "0");
            }
            catch { }
        }

        // =========================================================================
        // HELPER — plays the click sound safely (no memory leak, no crash)
        // =========================================================================
        #region PlayClickSound
        private void PlayClickSound()
        {
            try
            {
                string soundFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sound.wav");
                if (!File.Exists(soundFile)) return;

                using (SoundPlayer player = new SoundPlayer(soundFile))
                {
                    player.Play();
                }
            }
            catch
            {
                // Sound is optional — never crash because of a missing .wav
            }
        }
        #endregion

        // =========================================================================
        // HELPER — creates a backup of a file if the backup checkbox is enabled
        // Returns true if backup succeeded (or was not needed), false on error.
        // =========================================================================
        #region CreateBackup
        private bool CreateBackup(string filePath)
        {
            if (!chkBackup.Checked) return true;
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return true;

            try
            {
                string backupDir = Path.Combine(Path.GetDirectoryName(filePath), "Backup");
                Directory.CreateDirectory(backupDir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupName = $"{Path.GetFileNameWithoutExtension(filePath)}_{timestamp}{Path.GetExtension(filePath)}.bak";
                string backupPath = Path.Combine(backupDir, backupName);

                File.Copy(filePath, backupPath, overwrite: true);
                lbStatusStrip.Text = $"Backup created: {backupName}";
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed:\n{ex.Message}\n\nSave anyway?",
                    "Backup Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                return false; // caller decides whether to abort
            }
        }
        #endregion

        // =========================================================================
        // HELPER — sets status bar text
        // =========================================================================
        #region SetStatus
        private void SetStatus(string text)
        {
            lbStatusStrip.Text = text;
        }
        #endregion

        // =========================================================================
        // PANEL EDIT — Load / Save files
        // =========================================================================

        #region btLoadBodyconv_Click
        private void btLoadBodyconv_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "Bodyconv.def");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;
                lbFileName.Text = Path.GetFileName(path);
                SetStatus($"Loaded: {path}");
                searchStartIndex = 0;
            }
            else
            {
                MessageBox.Show($"File not found:\n{path}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            PlayClickSound();
        }
        #endregion

        #region btLoadPfad_Click
        private void btLoadPfad_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select your Ultima Online data directory";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    textBoxPfad.Text = dlg.SelectedPath;
                    Properties.Settings.Default.LastPath = dlg.SelectedPath;
                    Properties.Settings.Default.Save();
                    SetStatus($"Path set: {dlg.SelectedPath}");
                }
            }
            PlayClickSound();
        }
        #endregion

        #region btmobtypes_Click
        private void btmobtypes_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "mobtypes.txt");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;
                lbFileName.Text = Path.GetFileName(path);
                SetStatus($"Loaded: {path}");
                searchStartIndex = 0;
            }
            else
            {
                MessageBox.Show($"File not found:\n{path}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            PlayClickSound();
        }
        #endregion

        #region btSaveFile_Click
        private void btSaveFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("No file loaded to save.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create backup before overwriting (if enabled)
            if (!CreateBackup(currentFilePath)) return;

            try
            {
                File.WriteAllText(currentFilePath, richTextBoxEdit.Text, Encoding.UTF8);
                SetStatus($"Saved: {currentFilePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            PlayClickSound();
        }
        #endregion

        #region btBackwardText_Click
        private void btBackwardText_Click(object sender, EventArgs e)
        {
            if (richTextBoxEdit.CanUndo)
                richTextBoxEdit.Undo();
            else
                MessageBox.Show("Nothing to undo.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region btUOOpenDirectory_Click
        private void btUOOpenDirectory_Click(object sender, EventArgs e)
        {
            string path = textBoxPfad.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("No directory path set.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Directory does not exist.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe", path);
            PlayClickSound();
        }
        #endregion

        // =========================================================================
        // SEARCH
        // =========================================================================

        #region searchToolStripMenuItem_Click
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxSearch.Text;
            if (string.IsNullOrEmpty(searchText)) return;

            int count = Regex.Matches(richTextBoxEdit.Text, Regex.Escape(searchText),
                            RegexOptions.IgnoreCase).Count;
            lbSearchCount.Text = $"Matches: {count}";

            int index = richTextBoxEdit.Find(searchText, searchStartIndex,
                            RichTextBoxFinds.None);

            if (index != -1)
            {
                richTextBoxEdit.Select(index, searchText.Length);
                richTextBoxEdit.ScrollToCaret();
                richTextBoxEdit.Focus();
                searchStartIndex = index + searchText.Length;
            }
            else
            {
                if (searchStartIndex > 0)
                {
                    searchStartIndex = 0;
                    MessageBox.Show("End of file reached. Restarting from top.", "Search",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No matches found.", "Search",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        #endregion

        #region richTextBoxEdit_KeyDown
        private void richTextBoxEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                searchToolStripMenuItem_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
            // Ctrl+S = quick save
            if (e.Control && e.KeyCode == Keys.S)
            {
                btSaveFile_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
            // Ctrl+Z already handled by RichTextBox natively
        }
        #endregion

        // =========================================================================
        // CHECK FREE NUMBERS
        // =========================================================================

        #region btCheckNumbers_Click — skips comment lines
        private void btCheckNumbers_Click(object sender, EventArgs e)
        {
            // Only read non-comment lines (lines not starting with # or //)
            var lines = richTextBoxEdit.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(l => !l.TrimStart().StartsWith("#") && !l.TrimStart().StartsWith("//"));

            string nonCommentText = string.Join("\n", lines);

            var matches = Regex.Matches(nonCommentText, @"\b\d{2,4}\b");
            var numbers = new HashSet<int>(
                matches.Cast<Match>().Select(m => int.Parse(m.Value)));

            int missingNumber = Enumerable.Range(10, 9990)
                .FirstOrDefault(i => !numbers.Contains(i));

            MessageBox.Show($"First free Body-ID: {missingNumber}  (0x{missingNumber:X})",
                "Free ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // =========================================================================
        // CLIPBOARD
        // =========================================================================

        #region btClipboard_Click
        private void btClipboard_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(richTextBoxEdit.Text))
            {
                Clipboard.SetText(richTextBoxEdit.Text);
                SetStatus("Copied to clipboard.");
            }
            PlayClickSound();
        }
        #endregion

        // =========================================================================
        // CONSISTENCY CHECK — Bodyconv.def vs mobtypes.txt
        // =========================================================================

        #region btConsistencyCheck_Click
        private void btConsistencyCheck_Click(object sender, EventArgs e)
        {
            string uoDir = textBoxPfad.Text;
            string bodyconvPath = Path.Combine(uoDir, "Bodyconv.def");
            string mobtypesPath = Path.Combine(uoDir, "mobtypes.txt");

            if (!File.Exists(bodyconvPath) || !File.Exists(mobtypesPath))
            {
                MessageBox.Show("Both Bodyconv.def and mobtypes.txt must exist in the UO directory.",
                    "Check failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var bodyconvIDs = ParseBodyIDs(File.ReadAllLines(bodyconvPath));
            var mobtypeIDs = ParseBodyIDs(File.ReadAllLines(mobtypesPath));

            var onlyInBodyconv = bodyconvIDs.Except(mobtypeIDs).OrderBy(x => x).ToList();
            var onlyInMobtypes = mobtypeIDs.Except(bodyconvIDs).OrderBy(x => x).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"=== Consistency Check ===");
            sb.AppendLine($"Bodyconv.def entries : {bodyconvIDs.Count}");
            sb.AppendLine($"mobtypes.txt entries : {mobtypeIDs.Count}");
            sb.AppendLine();

            if (onlyInBodyconv.Count == 0 && onlyInMobtypes.Count == 0)
            {
                sb.AppendLine("No inconsistencies found.");
            }
            else
            {
                if (onlyInBodyconv.Count > 0)
                {
                    sb.AppendLine($"--- In Bodyconv.def but NOT in mobtypes.txt ({onlyInBodyconv.Count}) ---");
                    foreach (int id in onlyInBodyconv)
                        sb.AppendLine($"  Body-ID: {id}  (0x{id:X})");
                    sb.AppendLine();
                }
                if (onlyInMobtypes.Count > 0)
                {
                    sb.AppendLine($"--- In mobtypes.txt but NOT in Bodyconv.def ({onlyInMobtypes.Count}) ---");
                    foreach (int id in onlyInMobtypes)
                        sb.AppendLine($"  Body-ID: {id}  (0x{id:X})");
                }
            }

            richTextBoxEdit.Text = sb.ToString();
            currentFilePath = null;
            lbFileName.Text = "Consistency Check Result";
            SetStatus("Consistency check complete.");
            PlayClickSound();
        }

        private HashSet<int> ParseBodyIDs(string[] lines)
        {
            var ids = new HashSet<int>();
            foreach (string line in lines)
            {
                string trimmed = line.TrimStart();
                if (trimmed.StartsWith("#") || trimmed.StartsWith("//") || string.IsNullOrWhiteSpace(trimmed))
                    continue;

                // First token on each line is the Body-ID (decimal or hex)
                string firstToken = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (int.TryParse(firstToken, out int decId))
                    ids.Add(decId);
                else if (firstToken.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                         && int.TryParse(firstToken.Substring(2),
                             System.Globalization.NumberStyles.HexNumber, null, out int hexId))
                    ids.Add(hexId);
            }
            return ids;
        }
        #endregion

        // =========================================================================
        // FIND FIRST FREE ID ACROSS BOTH FILES
        // =========================================================================

        #region btFindFreeIDBoth_Click
        private void btFindFreeIDBoth_Click(object sender, EventArgs e)
        {
            string uoDir = textBoxPfad.Text;
            string bodyconvPath = Path.Combine(uoDir, "Bodyconv.def");
            string mobtypesPath = Path.Combine(uoDir, "mobtypes.txt");

            var allIDs = new HashSet<int>();

            if (File.Exists(bodyconvPath))
                foreach (int id in ParseBodyIDs(File.ReadAllLines(bodyconvPath)))
                    allIDs.Add(id);

            if (File.Exists(mobtypesPath))
                foreach (int id in ParseBodyIDs(File.ReadAllLines(mobtypesPath)))
                    allIDs.Add(id);

            if (allIDs.Count == 0)
            {
                MessageBox.Show("No IDs found. Make sure the UO path is set correctly.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int freeID = Enumerable.Range(10, 9990).FirstOrDefault(i => !allIDs.Contains(i));
            MessageBox.Show(
                $"First free ID (not in Bodyconv.def or mobtypes.txt):\n\n" +
                $"Decimal: {freeID}\nHex:     0x{freeID:X}",
                "Free ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // =========================================================================
        // SPHERE ID CHECKBOX — safe conversion
        // =========================================================================

        #region checkBoxSphereID_CheckedChanged
        private void checkBoxSphereID_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBoxSphereID.Checked) return;

            if (!int.TryParse(textBoxBody.Text, out int decValue))
            {
                MessageBox.Show("Body-ID must be a valid integer.", "Conversion Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                checkBoxSphereID.Checked = false;
                return;
            }
            tbCHARDEF.Text = decValue.ToString("X3"); // zero-padded 3-digit hex
        }
        #endregion

        // =========================================================================
        // PUBLIC PROPERTY SETTERS (called from external forms)
        // =========================================================================

        #region TextBoxID / TextBoxBody
        public string TextBoxID { set { textBoxID.Text = value; } }
        public string TextBoxBody { set { textBoxBody.Text = value; } }
        #endregion

        // =========================================================================
        // RUNUO SCRIPT CREATOR
        // =========================================================================

        #region btCreateScript_Click — BUG FIX: Damage now reads from correct fields
        private void btCreateScript_Click(object sender, EventArgs e)
        {
            string scriptName = tbScriptName.Text;
            if (string.IsNullOrWhiteSpace(scriptName))
            {
                MessageBox.Show("Please enter a Script Name.", "Missing Input",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string bodyValue = textBoxBody.Text;
            string ItemID = tbItemID.Text;
            string ItemID2 = tbItemID2.Text;
            string ItemID3 = tbItemID3.Text;
            string BaseSoundID1 = tBBaseSoundID1.Text;
            string BaseSoundID2 = tBBaseSoundID2.Text;

            string StrNR1 = tbStr1.Text;
            string StrNR2 = tbStr2.Text;
            string DexNR1 = tbDex1.Text;
            string DexNR2 = tbDex2.Text;
            string IntN1 = tbInt1.Text;
            string IntN2 = tbInt2.Text;
            string Hitp1 = tbSetHits1.Text;
            string Hitp2 = tbSetHits2.Text;

            // BUG FIX: was reading from tbSetHits — now correctly reads from tbSetDamage
            string SetDam1 = tbSetDamage1.Text;
            string SetDam2 = tbSetDamage2.Text;

            string SetFame = tbFame.Text;
            string SetKarma = tbKarma.Text;
            string SetArmor = tbVirtualArmor.Text;

            string SetPhysical = tbPhysical.Text;
            string SetFire = tbFire.Text;
            string SetEnergy = tbEnergy.Text;

            string SetRPhysical1 = tbRPhysical1.Text;
            string SetRPhysical2 = tbRPhysical2.Text;
            string SetRFire1 = tbRFire1.Text;
            string SetRFire2 = tbRFire2.Text;
            string SetRCold1 = tbRCold1.Text;
            string SetRCold2 = tbRCold2.Text;
            string SetRPoison1 = tbRPoison1.Text;
            string SetRPoison2 = tbRPoison2.Text;
            string SetREnergy1 = tbREnergy1.Text;
            string SetREnergy2 = tbREnergy2.Text;

            string SetEvalInt1 = tbSEvalInt1.Text;
            string SetEvalInt2 = tbSEvalInt2.Text;
            string SetMagery1 = tbSetMagery1.Text;
            string SetMagery2 = tbSetMagery2.Text;
            string SMagicResist1 = tbSMagicResist1.Text;
            string SMagicResist2 = tbSMagicResist2.Text;
            string STactics1 = tbSTactics1.Text;
            string STactics2 = tbSTactics2.Text;
            string SWrestling1 = tbSWrestling1.Text;
            string SWrestling2 = tbSWrestling2.Text;

            string STamable = tbTamable.Text;
            string SControlSlots = tbSControlSlots.Text;
            string SMinTameSkill = tbMinTameSkill.Text;

            string script = $@"using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Mobiles
{{
    [CorpseName(""a {scriptName} corpse"")]
    public class {scriptName} : BaseMount
    {{
        [Constructable]
        public {scriptName}() : this(""a {scriptName}"")
        {{
        }}

        [Constructable]
        public {scriptName}(string name) : base(name, 0x74, 0x3EBB, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {{
            BaseSoundID = {BaseSoundID1};

            SetStr({StrNR1}, {StrNR2});
            SetDex({DexNR1}, {DexNR2});
            SetInt({IntN1}, {IntN2});

            SetHits({Hitp1}, {Hitp2});
            SetDamage({SetDam1}, {SetDam2});

            SetDamageType(ResistanceType.Physical, {SetPhysical});
            SetDamageType(ResistanceType.Fire,     {SetFire});
            SetDamageType(ResistanceType.Energy,   {SetEnergy});

            SetResistance(ResistanceType.Physical, {SetRPhysical1}, {SetRPhysical2});
            SetResistance(ResistanceType.Fire,     {SetRFire1},     {SetRFire2});
            SetResistance(ResistanceType.Cold,     {SetRCold1},     {SetRCold2});
            SetResistance(ResistanceType.Poison,   {SetRPoison1},   {SetRPoison2});
            SetResistance(ResistanceType.Energy,   {SetREnergy1},   {SetREnergy2});

            SetSkill(SkillName.EvalInt,     {SetEvalInt1},   {SetEvalInt2});
            SetSkill(SkillName.Magery,      {SetMagery1},    {SetMagery2});
            SetSkill(SkillName.MagicResist, {SMagicResist1}, {SMagicResist2});
            SetSkill(SkillName.Tactics,     {STactics1},     {STactics2});
            SetSkill(SkillName.Wrestling,   {SWrestling1},   {SWrestling2});

            Fame          = {SetFame};
            Karma         = {SetKarma};
            VirtualArmor  = {SetArmor};
            Tamable       = {STamable};
            ControlSlots  = {SControlSlots};
            MinTameSkill  = {SMinTameSkill};

            switch (Utility.Random(3))
            {{
                case 0:
                    BodyValue = {bodyValue};
                    ItemID    = {ItemID};
                    break;
                case 1:
                    BodyValue = {bodyValue};
                    ItemID    = {ItemID2};
                    break;
                case 2:
                    BodyValue = {bodyValue};
                    ItemID    = {ItemID3};
                    break;
            }}

            PackItem(new SulfurousAsh(Utility.RandomMinMax(3, 5)));
        }}

        public override void GenerateLoot()
        {{
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Potions);
        }}

        public override int Meat         {{ get {{ return 5; }} }}
        public override int Hides        {{ get {{ return 10; }} }}
        public override HideType HideType {{ get {{ return HideType.Barbed; }} }}
        public override FoodType FavoriteFood {{ get {{ return FoodType.Meat; }} }}

        public {scriptName}(Serial serial) : base(serial)
        {{
        }}

        public override void Serialize(GenericWriter writer)
        {{
            base.Serialize(writer);
            writer.Write((int)0);
        }}

        public override void Deserialize(GenericReader reader)
        {{
            base.Deserialize(reader);
            int version = reader.ReadInt();
            if (BaseSoundID == {BaseSoundID2})
                BaseSoundID = {BaseSoundID1};
        }}
    }}
}}";

            richTextBoxEdit.Text = script;
            currentFilePath = null;
            lbFileName.Text = $"{scriptName}.cs (unsaved)";
            SetStatus("RunUO script generated.");
            PlayClickSound();
        }
        #endregion

        #region btSaveScript_Click
        private void btSaveScript_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "C# Files|*.cs";
                dlg.Title = "Save RunUO Script";
                dlg.FileName = tbScriptName.Text;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, richTextBoxEdit.Text, Encoding.UTF8);
                    currentFilePath = dlg.FileName;
                    lbFileName.Text = Path.GetFileName(dlg.FileName);
                    SetStatus($"Script saved: {dlg.FileName}");
                }
            }
        }
        #endregion

        // =========================================================================
        // SETTINGS SAVE / LOAD — with bounds check
        // =========================================================================

        #region btnSaveSettings_Click
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            var values = new List<string>
            {
                tbScriptName.Text, tbItemID.Text, tbItemID2.Text, tbItemID3.Text,
                tbStr1.Text, tbStr2.Text, tbDex1.Text, tbDex2.Text,
                tbInt1.Text, tbInt2.Text, tbSetHits1.Text, tbSetHits2.Text,
                tBBaseSoundID1.Text, tBBaseSoundID2.Text,
                tbSetDamage1.Text, tbSetDamage2.Text,           // fixed index 14/15
                tbPhysical.Text, tbFire.Text, tbEnergy.Text,
                tbFame.Text, tbKarma.Text, tbVirtualArmor.Text,
                tbRPhysical1.Text, tbRPhysical2.Text,
                tbRFire1.Text, tbRFire2.Text,
                tbRCold1.Text, tbRCold2.Text,
                tbRPoison1.Text, tbRPoison2.Text,
                tbREnergy1.Text, tbREnergy2.Text,
                tbSEvalInt1.Text, tbSEvalInt2.Text,
                tbSetMagery1.Text, tbSetMagery2.Text,
                tbSMagicResist1.Text, tbSMagicResist2.Text,
                tbSTactics1.Text, tbSTactics2.Text,
                tbSWrestling1.Text, tbSWrestling2.Text,
                tbSControlSlots.Text, tbTamable.Text, tbMinTameSkill.Text
            };

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings");
            Directory.CreateDirectory(dir);
            File.WriteAllLines(Path.Combine(dir, "ScriptSettingsAnimationen.txt"), values);

            SetStatus("Settings saved.");
            PlayClickSound();
        }
        #endregion

        #region btnLoadSettings_Click — BUG FIX: bounds check added
        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Data", "SavesSettings", "ScriptSettingsAnimationen.txt");

            if (!File.Exists(path))
            {
                MessageBox.Show("Settings file not found.", "Load Settings",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> values = File.ReadAllLines(path).ToList();

            // BUG FIX: guard against corrupted / outdated settings files
            if (values.Count < 45)
            {
                MessageBox.Show(
                    $"Settings file has only {values.Count} entries (expected 45).\n" +
                    "The file may be outdated. Partial load attempted.",
                    "Load Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            string V(int i) => i < values.Count ? values[i] : string.Empty;

            tbScriptName.Text = V(0); tbItemID.Text = V(1);
            tbItemID2.Text = V(2); tbItemID3.Text = V(3);
            tbStr1.Text = V(4); tbStr2.Text = V(5);
            tbDex1.Text = V(6); tbDex2.Text = V(7);
            tbInt1.Text = V(8); tbInt2.Text = V(9);
            tbSetHits1.Text = V(10); tbSetHits2.Text = V(11);
            tBBaseSoundID1.Text = V(12); tBBaseSoundID2.Text = V(13);
            tbSetDamage1.Text = V(14); tbSetDamage2.Text = V(15);
            tbPhysical.Text = V(16); tbFire.Text = V(17);
            tbEnergy.Text = V(18); tbFame.Text = V(19);
            tbKarma.Text = V(20); tbVirtualArmor.Text = V(21);
            tbRPhysical1.Text = V(22); tbRPhysical2.Text = V(23);
            tbRFire1.Text = V(24); tbRFire2.Text = V(25);
            tbRCold1.Text = V(26); tbRCold2.Text = V(27);
            tbRPoison1.Text = V(28); tbRPoison2.Text = V(29);
            tbREnergy1.Text = V(30); tbREnergy2.Text = V(31);
            tbSEvalInt1.Text = V(32); tbSEvalInt2.Text = V(33);
            tbSetMagery1.Text = V(34); tbSetMagery2.Text = V(35);
            tbSMagicResist1.Text = V(36); tbSMagicResist2.Text = V(37);
            tbSTactics1.Text = V(38); tbSTactics2.Text = V(39);
            tbSWrestling1.Text = V(40); tbSWrestling2.Text = V(41);
            tbSControlSlots.Text = V(42); tbTamable.Text = V(43);
            tbMinTameSkill.Text = V(44);

            SetStatus("Settings loaded.");
            PlayClickSound();
        }
        #endregion

        // =========================================================================
        // SPHERE SCRIPT CREATOR — BUG FIX: RESENERGY now has proper braces
        // =========================================================================

        #region btCreateSphereScript_Click
        private void btCreateSphereScript_Click(object sender, EventArgs e)
        {
            string SetCHARDEF = tbCHARDEF.Text;
            string SetDEFNAME = tbDEFNAME.Text;
            string SetNAME = tbName.Text;
            string SetICON = tbICON.Text;
            string SetSOUND = tbSOUND.Text;
            string SetCAN = tbCAN.Text;
            string SetDAM = tbDAM.Text;
            string SetArmor = tbAmor.Text;
            string SetDESIRES = tbDESIRES.Text;
            string SetAVERSIONS = tbAVERSIONS.Text;
            string SetFOODTYPE = tbFOODTYPE.Text;
            string SetMAXFOOD = tbMAXFOOD.Text;
            string SetRESOURCES = tbRESOURCES.Text;
            string SetCATEGORY = tbCATEGORY.Text;
            string SetSUBSECTION = tbSUBSECTION.Text;
            string SetDESCRIPTION = tbDESCRIPTION.Text;
            string SetNPC = tbNPC.Text;
            string SetSFame = tbSFAME.Text;
            string SetSKarma = tbSKARMA.Text;
            string SetSTR = tbSTR.Text;
            string SetDEX = tbDEX.Text;
            string SetINT = tbINT.Text;
            string SetEVALUATINGINTEL = tbEVALUATINGINTEL.Text;
            string SetMAGERY = tbMAGERY.Text;
            string SetMAGICRESISTANCE = tbMAGICRESISTANCE.Text;
            string SetMEDITATION = tbMEDITATION.Text;
            string SetPARRYING = tbPARRYING.Text;
            string SetTACTICS = tbTACTICS.Text;
            string SetWRESTLING = tbWRESTLING.Text;
            string SetRESPHYSICAL = tbRESPHYSICAL.Text;
            string SetRESCOLD = tbRESCOLD.Text;
            string SetRESENERGY = tbRESENERGY.Text;
            string SetRESFIRE = tbRESFIRE.Text;
            string SetRESPOISON = tbRESPOISON.Text;

            // BUG FIX: RESENERGY was missing {{}} in the original — now consistent with all other fields
            string script = $@"
[CHARDEF {SetCHARDEF}]
DEFNAME={SetDEFNAME}
NAME={SetNAME}
ICON={SetICON}
SOUND={SetSOUND}
CAN={SetCAN}
DAM={SetDAM}
armor={SetArmor}
DESIRES={SetDESIRES}
AVERSIONS={SetAVERSIONS}
FOODTYPE={SetFOODTYPE}
MAXFOOD={SetMAXFOOD}
RESOURCES={SetRESOURCES}
TAG.FORCEMONSTRE=3
TAG.Barding.Diff=72.5
TAG.SlayerGroup=GARGOYLE,DEMON
TEVENTS=e_carnivores2
CATEGORY={SetCATEGORY}
SUBSECTION={SetSUBSECTION}
DESCRIPTION={SetDESCRIPTION}
ON=@Create
    NPC={SetNPC}
    FAME={{{SetSFame}}}
    KARMA={{{SetSKarma}}}
    STR={{{SetSTR}}}
    DEX={{{SetDEX}}}
    INT={{{SetINT}}}
    EVALUATINGINTEL={{{SetEVALUATINGINTEL}}}
    MAGERY={{{SetMAGERY}}}
    MAGICRESISTANCE={{{SetMAGICRESISTANCE}}}
    MEDITATION={{{SetMEDITATION}}}
    PARRYING={{{SetPARRYING}}}
    TACTICS={{{SetTACTICS}}}
    WRESTLING={{{SetWRESTLING}}}
    RESPHYSICAL={{{SetRESPHYSICAL}}}
    RESCOLD={{{SetRESCOLD}}}
    RESENERGY={{{SetRESENERGY}}}
    RESFIRE={{{SetRESFIRE}}}
    RESPOISON={{{SetRESPOISON}}}

    ITEMNEWBIE=i_spellbook
    ADDSPELL=s_magic_arrow
    ADDSPELL=s_clumsy
    ADDSPELL=s_weaken
    ADDSPELL=s_feeblemind
    ADDSPELL=s_curse
    ADDSPELL=s_harm
    ADDSPELL=s_fireball
    ADDSPELL=s_poison
    ADDSPELL=s_lightning
    ADDSPELL=s_mana_drain
    ADDSPELL=s_mind_blast
    ADDSPELL=s_paralyze
    ADDSPELL=s_energy_bolt
    ADDSPELL=s_explosion
    ADDSPELL=s_mass_curse
    ADDSPELL=s_chain_lightning
    ADDSPELL=s_flamestrike
    ADDSPELL=s_mana_vampire

ON=@CreateLoot
    ITEM=loot_gargoyle
    ITEM=i_pierre_depecage_gargoyle
    ITEM=loot_gold_2";

            richTextBoxEdit.Text = script;
            currentFilePath = null;
            lbFileName.Text = $"{SetNAME}.scp (unsaved)";
            SetStatus("Sphere script generated.");
            PlayClickSound();
        }
        #endregion

        #region btSaveSphereScript_Click
        private void btSaveSphereScript_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "SCP Files|*.scp";
                dlg.Title = "Save Sphere Script";
                dlg.FileName = tbName.Text;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dlg.FileName, richTextBoxEdit.Text, Encoding.UTF8);
                    currentFilePath = dlg.FileName;
                    lbFileName.Text = Path.GetFileName(dlg.FileName);
                    SetStatus($"Sphere script saved: {dlg.FileName}");
                }
            }
        }
        #endregion

        // =========================================================================
        // ANIMATIONLIST
        // =========================================================================

        #region btAnimationlistLoad_Click
        private void btAnimationlistLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"UoFiddler\Animationlist.xml");

            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;
                lbFileName.Text = "Animationlist.xml";
                SetStatus($"Loaded: {path}");
            }
            else
            {
                MessageBox.Show($"File not found:\n{path}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            PlayClickSound();
        }
        #endregion

        #region btSaveAnimationlist_Click
        private void btSaveAnimationlist_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"UoFiddler\Animationlist.xml");

            if (!CreateBackup(path)) return;

            File.WriteAllText(path, richTextBoxEdit.Text, Encoding.UTF8);
            SetStatus($"Animationlist saved: {path}");
            PlayClickSound();
        }
        #endregion

        // =========================================================================
        // APP DATA / DIRECTORY BUTTONS
        // =========================================================================

        #region btAppData_Click
        private void btAppData_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "UoFiddler");

            if (Directory.Exists(path))
                System.Diagnostics.Process.Start("explorer.exe", path);
            else
                MessageBox.Show($"Directory not found:\n{path}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            PlayClickSound();
        }
        #endregion

        // =========================================================================
        // MOBTYPES INFO
        // =========================================================================

        #region btInfoMobtypes_Click
        private void btInfoMobtypes_Click(object sender, EventArgs e)
        {
            string info =
                "MOBTYPES FLAG REFERENCE\n" +
                "═══════════════════════════════════════════════════\n\n" +
                "1000  — Spell casting: actions 12, 13, 14\n" +
                "1004  — Actions 7 & 8; action 12 = summon animation\n" +
                "1008  — Flying (actions 19/20/21) + cast actions 12 & 13\n" +
                "1009  — Flying (actions 19/20/21) + cast action 12 only\n" +
                "A00   — Limited actions; attack actions 4/5/6 replace 12/13/14\n\n" +
                "Format:  BodyID  MONSTER|ANIMAL|EQUIPMENT  Flags\n" +
                "Example: 747  MONSTER  1008";

            MessageBox.Show(info, "mobtypes.txt Reference",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        // =========================================================================
        // ANIM.MUL — Create empty file
        // =========================================================================

        #region btnSingleEmptyAnimMul_Click
        private void btnSingleEmptyAnimMul_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select output directory for empty anim.mul";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.Combine(dlg.SelectedPath, "anim.mul");
                    File.WriteAllBytes(path, Array.Empty<byte>());
                    SetStatus($"Created empty anim.mul at: {path}");
                }
            }
        }
        #endregion

        // =========================================================================
        // ANIM IDX PROCESSOR
        // =========================================================================

        #region btnBrowseClick
        private void btnBrowseClick(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Index Files|*.idx|All Files|*.*";
                dlg.Title = "Select anim.idx";
                if (dlg.ShowDialog() == DialogResult.OK)
                    tbfilename.Text = dlg.FileName;
            }
        }
        #endregion

        #region btnSetOutputDirectoryClick
        private void btnSetOutputDirectoryClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select output directory";
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtOutputDirectory.Text = dlg.SelectedPath;
            }
        }
        #endregion

        #region btnProcessClick
        private void btnProcessClick(object sender, EventArgs e)
        {
            tbProcessAminidx.Clear();
            newIdCount = 0;
            lblNewIdCount.Text = "Count: 0";

            try
            {
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("ERROR: Could not open anim.idx\n");
                    return;
                }

                if (!int.TryParse(txtOrigCreatureID.Text,
                        System.Globalization.NumberStyles.HexNumber, null, out int creatureID))
                {
                    tbProcessAminidx.AppendText("ERROR: Enter a valid hex Animation ID (e.g. 4B)\n");
                    return;
                }

                int copyCount;
                if (chkHighDetail.Checked) copyCount = cHighDetail;
                else if (chkLowDetail.Checked) copyCount = cLowDetail;
                else if (chkHuman.Checked) copyCount = cHuman;
                else if (!int.TryParse(txtNewCreatureID.Text, out copyCount))
                {
                    tbProcessAminidx.AppendText("ERROR: Enter a valid copy count or select a checkbox\n");
                    return;
                }

                string outputFilename;
                if (string.IsNullOrEmpty(txtOutputFilename.Text))
                    outputFilename = Path.Combine(txtOutputDirectory.Text, "anim.idx");
                else
                    outputFilename = Path.Combine(txtOutputDirectory.Text,
                        "anim" + txtOutputFilename.Text + ".idx");

                File.Copy(filename, outputFilename, overwrite: true);
                tbProcessAminidx.AppendText($"Copied source to: {outputFilename}\n");

                CopyAnimationData(outputFilename, creatureID, copyCount);
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"ERROR: {ex.Message}\n");
            }
        }

        private void CopyAnimationData(string filename, int creatureID, int copyCount)
        {
            tbProcessAminidx.AppendText("Analyzing source animation...\n");

            DetermineCreatureProperties(creatureID,
                out int indexOffset, out int readLength, out string creatureType);

            tbProcessAminidx.AppendText($"Type: {creatureType}\n");
            tbProcessAminidx.AppendText($"Index offset: {indexOffset}  |  Block size: {readLength} bytes\n");

            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                long seekPos = (long)indexOffset * 12;
                if (seekPos + readLength > stream.Length)
                {
                    tbProcessAminidx.AppendText(
                        $"ERROR: Seek position {seekPos} + {readLength} bytes exceeds file size {stream.Length}.\n" +
                        "Check the Animation ID.\n");
                    return;
                }

                stream.Seek(seekPos, SeekOrigin.Begin);
                byte[] buffer = new byte[readLength];
                int bytesRead = stream.Read(buffer, 0, readLength);
                tbProcessAminidx.AppendText($"Read {bytesRead} bytes from cID {creatureID}\n");

                for (int i = 0; i < copyCount; i++)
                {
                    stream.Seek(0, SeekOrigin.End);
                    stream.Write(buffer, 0, readLength);
                    newIdCount++;
                }
            }

            lblNewIdCount.Text = $"IDs created: {newIdCount}";
            tbProcessAminidx.AppendText($"Done. Wrote {copyCount} copies ({readLength} bytes each).\n");
            SetStatus($"anim.idx written: {newIdCount} new entries.");
        }
        #endregion

        #region DetermineCreatureProperties
        private void DetermineCreatureProperties(int creatureID,
            out int indexOffset, out int readLength, out string creatureType)
        {
            if (creatureID <= 199)
            {
                indexOffset = creatureID * cHighDetail;
                readLength = cHighDetail * 12;
                creatureType = "High Detail Critter (ID 0-199)";
            }
            else if (creatureID <= 399)
            {
                indexOffset = (creatureID - 200) * cLowDetail + 22000;
                readLength = cLowDetail * 12;
                creatureType = "Low Detail Critter (ID 200-399)";
            }
            else
            {
                indexOffset = (creatureID - 400) * cHuman + 35000;
                readLength = cHuman * 12;
                creatureType = "Human / Accessory (ID 400+)";
            }
        }
        #endregion

        #region btnProcessClickOldVersion
        private void btnProcessClickOldVersion(object sender, EventArgs e)
        {
            tbProcessAminidx.Clear();

            try
            {
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("ERROR: Could not open anim.idx\n");
                    return;
                }

                if (!int.TryParse(txtOrigCreatureID.Text,
                        System.Globalization.NumberStyles.HexNumber, null, out int creatureID))
                {
                    tbProcessAminidx.AppendText("ERROR: Enter a valid hex Animation ID\n");
                    return;
                }

                if (!int.TryParse(txtNewCreatureID.Text, out int copyCount))
                {
                    tbProcessAminidx.AppendText("ERROR: Enter a valid copy count\n");
                    return;
                }

                int indexOffset, readLength;
                byte cAnimType;

                if (creatureID <= 199)
                {
                    indexOffset = creatureID * 110;
                    readLength = 110 * 12;
                    cAnimType = cHighDetailOLd;
                    tbProcessAminidx.AppendText("High Detail Critter\n");
                }
                else if (creatureID <= 399)
                {
                    indexOffset = (creatureID - 200) * 65 + 22000;
                    readLength = 65 * 12;
                    cAnimType = cLowDetailOld;
                    tbProcessAminidx.AppendText("Low Detail Critter\n");
                }
                else
                {
                    indexOffset = (creatureID - 400) * 175 + 35000;
                    readLength = 175 * 12;
                    cAnimType = cHumanOld;
                    tbProcessAminidx.AppendText("Human / Accessory\n");
                }

                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Seek((long)indexOffset * 12, SeekOrigin.Begin);
                    byte[] buffer = new byte[readLength];
                    stream.Read(buffer, 0, readLength);
                    tbProcessAminidx.AppendText($"Read {readLength} bytes\n");

                    for (int i = 0; i < copyCount; i++)
                    {
                        stream.Seek(0, SeekOrigin.End);
                        stream.Write(buffer, 0, readLength);
                    }
                    tbProcessAminidx.AppendText($"Wrote {copyCount} copies (Old Version mode).\n");
                }
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"ERROR: {ex.Message}\n");
            }
        }
        #endregion

        // =========================================================================
        // BACKUP CHECKBOX — persists setting
        // =========================================================================

        #region chkBackup_CheckedChanged
        private void chkBackup_CheckedChanged(object sender, EventArgs e)
        {
            SaveBackupEnabled(chkBackup.Checked);
            SetStatus(chkBackup.Checked
                ? "Auto-backup ON — files will be backed up before saving."
                : "Auto-backup OFF.");
        }
        #endregion

        // =========================================================================
        // FORM CLOSING — save settings
        // =========================================================================

        #region EditUoBodyconvMobtypes_FormClosing
        private void EditUoBodyconvMobtypes_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}
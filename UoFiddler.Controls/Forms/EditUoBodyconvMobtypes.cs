﻿// /***************************************************************************
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
        private int searchStartIndex = 0;
        private string currentFilePath;  // Saves the path of the last loaded file
        public EditUoBodyconvMobtypes()
        {
            InitializeComponent();

            textBoxPfad.Text = Properties.Settings.Default.LastPath; // Save Last Path
        }

        #region btLoadBodyconv_Click
        private void btLoadBodyconv_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "Bodyconv.def");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;  // Updates the path of the last loaded file
                lbFileName.Text = Path.GetFileName(path);  // Displays the file name in lbFileName
            }
            else
            {
                MessageBox.Show("The Bodyconv.def file could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btLoadPfad_Click
        private void btLoadPfad_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxPfad.Text = folderBrowserDialog.SelectedPath;
                // Save the selected path in Settings
                Properties.Settings.Default.LastPath = folderBrowserDialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region btmobtypes_Click
        private void btmobtypes_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfad.Text, "mobtypes.txt");
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
                currentFilePath = path;  // Updates the path of the last loaded file
                lbFileName.Text = Path.GetFileName(path);  // Displays the file name in lbFileName
            }
            else
            {
                MessageBox.Show("The file mobtypes.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region searchToolStripMenuItem
        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string searchText = toolStripTextBoxSearch.Text;
            if (!string.IsNullOrEmpty(searchText))
            {
                // Counts the number of matches and displays them in lbSearchCount
                int count = Regex.Matches(richTextBoxEdit.Text, searchText).Count;
                lbSearchCount.Text = $"Number of matches: {count}";

                int index = richTextBoxEdit.Find(searchText, searchStartIndex, RichTextBoxFinds.None);
                if (index != -1)
                {
                    richTextBoxEdit.Select(index, searchText.Length);
                    richTextBoxEdit.ScrollToCaret();  // Scrolls to the cursor position
                    richTextBoxEdit.Focus();  // Sets the focus on the RichTextBox
                    searchStartIndex = index + searchText.Length;
                }
                else
                {
                    MessageBox.Show("No further matches found.");
                    searchStartIndex = 0;
                }
            }
        }
        #endregion

        #region btSaveFile_Click
        private void btSaveFile_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath))
            {
                File.WriteAllText(currentFilePath, richTextBoxEdit.Text);
            }
            else
            {
                MessageBox.Show("No file was selected to save.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btBackwardText_Click
        private void btBackwardText_Click(object sender, EventArgs e)
        {
            if (richTextBoxEdit.CanUndo)
            {
                richTextBoxEdit.Undo();
            }
            else
            {
                MessageBox.Show("No further reversals possible.");
            }
        }
        #endregion

        #region btCheckNumbers_Click
        private void btCheckNumbers_Click(object sender, EventArgs e)
        {
            // Extract all numbers from the text
            var matches = Regex.Matches(richTextBoxEdit.Text, @"\b\d{2,4}\b");
            var numbers = new HashSet<int>(matches.Cast<Match>().Select(m => int.Parse(m.Value)));

            // Find the first missing number
            int missingNumber = Enumerable.Range(10, 9990).FirstOrDefault(i => !numbers.Contains(i));

            // Zeigen Sie die fehlende Zahl in einer MessageBox an
            MessageBox.Show($"The first free number found is at {missingNumber}");
        }
        #endregion

        #region string TextBoxID
        public string TextBoxID
        {
            set { textBoxID.Text = value; }
        }
        #endregion

        #region string TextBoxBody
        public string TextBoxBody
        {
            set { textBoxBody.Text = value; }
        }
        #endregion

        #region btCreateScript_Click
        private void btCreateScript_Click(object sender, EventArgs e)
        {
            string scriptName = tbScriptName.Text;
            string bodyValue = textBoxBody.Text;
            string ItemID = tbItemID.Text;
            string ItemID2 = tbItemID2.Text;
            string ItemID3 = tbItemID3.Text;
            string BaseSoundID1 = tBBaseSoundID1.Text;
            string BaseSoundID2 = tBBaseSoundID2.Text;


            //strength
            string StrNR1 = tbStr1.Text; // strength
            string StrNR2 = tbStr2.Text; // strength
            // Dex
            string DexNR1 = tbDex1.Text; // Dex
            string DexNR2 = tbDex2.Text; // Dex
            // Int
            string IntN1 = tbInt1.Text; // Int
            string IntN2 = tbInt2.Text; // Int
            //Hitpoints
            string Hitp1 = tbSetHits1.Text; // Hitpoints
            string Hitp2 = tbSetHits2.Text; // Hitpoints
            // Damage
            string SetDam1 = tbSetHits1.Text; // Damage
            string SetDam2 = tbSetHits2.Text; // Damage
            // Fame
            string SetFame = tbFame.Text; // Fame
            // Karma
            string SetKarma = tbKarma.Text; // Karma
            // Armor
            string SetArmor = tbVirtualArmor.Text; // Armor
            // Damage Resistance
            string SetPhysical = tbPhysical.Text; // Damage Resistance Physical
            string SetFire = tbFire.Text; // Damage Resistance Fire
            string SetEnergy = tbEnergy.Text; // Damage Resistance Energy
            // Resistance
            string SetRPhysical1 = tbRPhysical1.Text; // Resistance Physical
            string SetRPhysical2 = tbRPhysical2.Text; // Resistance Physical
            string SetRFire1 = tbRFire1.Text; // Resistance Fire
            string SetRFire2 = tbRFire2.Text; // Resistance Fire
            string SetRCold1 = tbRCold1.Text; // Resistance Cold
            string SetRCold2 = tbRCold2.Text; // Resistance Cold
            string SetRPoison1 = tbRPoison1.Text; // Resistance Poison
            string SetRPoison2 = tbRPoison2.Text; // Resistance Poison
            string SetREnergy1 = tbREnergy1.Text; // Resistance Energy
            string SetREnergy2 = tbREnergy2.Text; // Resistance Energy
            // Set Skills
            string SetEvalInt1 = tbSEvalInt1.Text; // Set Skill EvalInt
            string SetEvalInt2 = tbSEvalInt2.Text; // Set Skill EvalInt
            string SetMagery1 = tbSetMagery1.Text; // Set Skill Magery
            string SetMagery2 = tbSetMagery2.Text; // Set Skill Magery
            string SMagicResist1 = tbSMagicResist1.Text; // Set Skill Magery Resist
            string SMagicResist2 = tbSMagicResist2.Text; // Set Skill Magery Resist
            string STactics1 = tbSTactics1.Text; // Set Skill Tactics
            string STactics2 = tbSTactics2.Text; // Set Skill Tactics
            string SWrestling1 = tbSWrestling1.Text; // Set Skill Wrestling
            string SWrestling2 = tbSWrestling2.Text; // Set Skill Wrestling
            // Animal
            string STamable = tbTamable.Text; // Set True or False
            string SControlSlots = tbSControlSlots.Text; // Set Controll Slots you need from 1 to 6
            string SMinTameSkill = tbMinTameSkill.Text; // Set Tame Skill you Need

            string script = $@"
            using System;
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
                BaseSoundID = {BaseSoundID1};       // Sound ID 1
                SetStr({StrNR1}, {StrNR2});         // strength
                SetDex({DexNR1}, {DexNR2});         // Dex
                SetInt({IntN1}, {IntN2});           // Int

                SetHits({Hitp1}, {Hitp2});          // Hitpoints
                SetDamage({SetDam1}, {SetDam2});    // Damage

                // Damage Resistance
                SetDamageType(ResistanceType.Physical, {SetPhysical});
                SetDamageType(ResistanceType.Fire, {SetFire});
                SetDamageType(ResistanceType.Energy, {SetEnergy});

                // Set Resistance
                SetResistance(ResistanceType.Physical, {SetRPhysical1}, {SetRPhysical2});
                SetResistance(ResistanceType.Fire, {SetRFire1}, {SetRFire2});
                SetResistance(ResistanceType.Cold, {SetRCold1}, {SetRCold2});
                SetResistance(ResistanceType.Poison, {SetRPoison1}, {SetRPoison2});
                SetResistance(ResistanceType.Energy, {SetREnergy1}, {SetREnergy2});

                // Set Skills
                SetSkill(SkillName.EvalInt, {SetEvalInt1}, {SetEvalInt2});
                SetSkill(SkillName.Magery, {SetMagery1}, {SetMagery2});
                SetSkill(SkillName.MagicResist, {SMagicResist1}, {SMagicResist2});
                SetSkill(SkillName.Tactics, {STactics1}, {STactics2});
                SetSkill(SkillName.Wrestling, {SWrestling1}, {SWrestling2});

                Fame = {SetFame}; // Fame
                Karma = {SetKarma}; // Karma

                VirtualArmor = {SetArmor}; // Armor
                Tamable = {STamable}; // Tame false or true
                ControlSlots = {SControlSlots}; // Control slots you need
                MinTameSkill = {SMinTameSkill};  //Skill required to tame

                switch (Utility.Random(3))
                {{
                    case 0:
                        {{
                            BodyValue = {bodyValue}; //Animation BodyID
                            ItemID = {ItemID}; // Item Slot Id 1
                            break;
                        }}
                    case 1:
                        {{
                            BodyValue = {bodyValue}; //Animation BodyID
                            ItemID = {ItemID2}; // Item Slot Id 2
                            break;
                        }}
                    case 2:
                        {{
                            BodyValue = {bodyValue}; //Animation BodyID
                            ItemID = {ItemID3}; // Item Slot Id 3
                            break;
                        }}
                }}

                PackItem(new SulfurousAsh(Utility.RandomMinMax(3, 5)));
            }}

            public override void GenerateLoot()
            {{
                AddLoot(LootPack.FilthyRich); //Loot Packs
                AddLoot(LootPack.LowScrolls); //Loot Packs
                AddLoot(LootPack.Potions); //Loot Packs
            }}

            public override int Meat {{ get {{ return 5; }} }}
            public override int Hides {{ get {{ return 10; }} }}
            public override HideType HideType {{ get {{ return HideType.Barbed; }} }}
            public override FoodType FavoriteFood {{ get {{ return FoodType.Meat; }} }}

            public {scriptName}(Serial serial) : base(serial)
            {{
            }}

            public override void Serialize(GenericWriter writer)
            {{
                base.Serialize(writer);
                writer.Write((int)0); // version
            }}

            public override void Deserialize(GenericReader reader)
            {{
                base.Deserialize(reader);
                int version = reader.ReadInt();
                if (BaseSoundID == {BaseSoundID2}) // Sound ID 2
                    BaseSoundID = {BaseSoundID1}; //Sound ID 1
            }}

         }}
    }}";
            richTextBoxEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSaveScript_Click
        private void btSaveScript_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C# Files|*.cs";
            saveFileDialog.Title = "Save a C# File";
            saveFileDialog.FileName = tbScriptName.Text; // Sets the filename to the contents of tbScriptName

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.OpenFile()))
                {
                    writer.Write(richTextBoxEdit.Text); // Writes the contents of richTextBoxEdit to the file
                }
            }
        }
        #endregion

        #region btClipboard
        private void btClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBoxEdit.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // Erstellen Sie eine neue Liste, um die Werte zu speichern
            List<string> values = new List<string>();

            // Fügen Sie die Werte der Textboxen zur Liste hinzu
            values.Add(tbScriptName.Text);
            values.Add(tbItemID.Text);
            values.Add(tbItemID2.Text);
            values.Add(tbItemID3.Text);
            values.Add(tbStr1.Text);
            values.Add(tbStr2.Text);
            values.Add(tbDex1.Text);
            values.Add(tbDex2.Text);
            values.Add(tbInt1.Text);
            values.Add(tbInt2.Text);
            values.Add(tbSetHits1.Text);
            values.Add(tbSetHits2.Text);
            values.Add(tBBaseSoundID1.Text);
            values.Add(tBBaseSoundID2.Text);
            values.Add(tbSetDamage1.Text);
            values.Add(tbSetDamage2.Text);
            values.Add(tbPhysical.Text);
            values.Add(tbFire.Text);
            values.Add(tbEnergy.Text);
            values.Add(tbFame.Text);
            values.Add(tbKarma.Text);
            values.Add(tbVirtualArmor.Text);
            values.Add(tbRPhysical1.Text);
            values.Add(tbRPhysical2.Text);
            values.Add(tbRFire1.Text);
            values.Add(tbRFire2.Text);
            values.Add(tbRCold1.Text);
            values.Add(tbRCold2.Text);
            values.Add(tbRPoison1.Text);
            values.Add(tbRPoison2.Text);
            values.Add(tbREnergy1.Text);
            values.Add(tbREnergy2.Text);
            values.Add(tbSEvalInt1.Text);
            values.Add(tbSEvalInt2.Text);
            values.Add(tbSetMagery1.Text);
            values.Add(tbSetMagery2.Text);
            values.Add(tbSMagicResist1.Text);
            values.Add(tbSMagicResist2.Text);
            values.Add(tbSTactics1.Text);
            values.Add(tbSTactics2.Text);
            values.Add(tbSWrestling1.Text);
            values.Add(tbSWrestling2.Text);
            values.Add(tbSControlSlots.Text);
            values.Add(tbTamable.Text);
            values.Add(tbMinTameSkill.Text);

            // Fügen Sie weitere Textboxen hinzu, falls vorhanden...

            // Bestimmen Sie den Pfad zum Speicherort
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings");

            // Erstellen Sie den Ordner, wenn er noch nicht existiert
            Directory.CreateDirectory(path);

            // Speichern Sie die Liste in einer Datei
            File.WriteAllLines(Path.Combine(path, "ScriptSettingsAnimationen.txt"), values);
        }

        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            // Bestimmen Sie den Pfad zum Speicherort
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings", "ScriptSettingsAnimationen.txt");

            // Überprüfen Sie, ob die Datei existiert
            if (File.Exists(path))
            {
                // Lesen Sie die Werte aus der Datei
                List<string> values = File.ReadAllLines(path).ToList();

                // Setzen Sie die Werte der Textboxen
                tbScriptName.Text = values[0];
                tbItemID.Text = values[1];
                tbItemID2.Text = values[2];
                tbItemID3.Text = values[3];
                tbStr1.Text = values[4];
                tbStr2.Text = values[5];
                tbDex1.Text = values[6];
                tbDex2.Text = values[7];
                tbInt1.Text = values[8];
                tbInt2.Text = values[9];
                tbSetHits1.Text = values[10];
                tbSetHits2.Text = values[11];
                tBBaseSoundID1.Text = values[12];
                tBBaseSoundID2.Text = values[13];
                tbSetDamage1.Text = values[14];
                tbSetDamage2.Text = values[15];
                tbPhysical.Text = values[16];
                tbFire.Text = values[17];
                tbEnergy.Text = values[18];
                tbFame.Text = values[19];
                tbKarma.Text = values[20];
                tbVirtualArmor.Text = values[21];
                tbRPhysical1.Text = values[22];
                tbRPhysical2.Text = values[23];
                tbRFire1.Text = values[24];
                tbRFire2.Text = values[25];
                tbRCold1.Text = values[26];
                tbRCold2.Text = values[27];
                tbRPoison1.Text = values[28];
                tbRPoison2.Text = values[29];
                tbREnergy1.Text = values[30];
                tbREnergy2.Text = values[31];
                tbSEvalInt1.Text = values[31];
                tbSEvalInt2.Text = values[32];
                tbSetMagery1.Text = values[33];
                tbSetMagery2.Text = values[34];
                tbSMagicResist1.Text = values[35];
                tbSMagicResist2.Text = values[36];
                tbSTactics1.Text = values[37];
                tbSTactics2.Text = values[38];
                tbSWrestling1.Text = values[39];
                tbSWrestling2.Text = values[40];
                tbSControlSlots.Text = values[41];
                tbTamable.Text = values[42];
                tbMinTameSkill.Text = values[43];

                // Setzen Sie weitere Textboxen, falls vorhanden...
            }
            else
            {
                MessageBox.Show("Die Einstellungsdatei konnte nicht gefunden werden.");
            }
        }
    }
}
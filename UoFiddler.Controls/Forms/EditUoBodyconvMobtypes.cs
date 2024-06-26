﻿// /***************************************************************************
//  *
//  * $Author: Nikodemus
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

        #region btnSaveSettings
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // Create a new list to store the values
            List<string> values = new List<string>();

            // Add the text box values ​​to the list
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

            // Add more text boxes if any...

            // Bestimmen Sie den Pfad zum Speicherort
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings");

            // Create the folder if it doesn't already exist
            Directory.CreateDirectory(path);

            // Save the list to a file
            File.WriteAllLines(Path.Combine(path, "ScriptSettingsAnimationen.txt"), values);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btnLoadSettings_Click
        private void btnLoadSettings_Click(object sender, EventArgs e)
        {
            // Specify the path to the storage location
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SavesSettings", "ScriptSettingsAnimationen.txt");

            // Check if the file exists
            if (File.Exists(path))
            {
                // Read the values ​​from the file
                List<string> values = File.ReadAllLines(path).ToList();

                // Set the values ​​of the text boxes
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
                tbSEvalInt1.Text = values[32];
                tbSEvalInt2.Text = values[33];
                tbSetMagery1.Text = values[34];
                tbSetMagery2.Text = values[35];
                tbSMagicResist1.Text = values[36];
                tbSMagicResist2.Text = values[37];
                tbSTactics1.Text = values[38];
                tbSTactics2.Text = values[39];
                tbSWrestling1.Text = values[40];
                tbSWrestling2.Text = values[41];
                tbSControlSlots.Text = values[42];
                tbTamable.Text = values[43];
                tbMinTameSkill.Text = values[44];

                // Put additional text boxes if there are any...
            }
            else
            {
                MessageBox.Show("The settings file could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateSphereScript_Click
        private void btCreateSphereScript_Click(object sender, EventArgs e)
        {
            string SetCHARDEF = tbCHARDEF.Text; // CHARDEF
            string SetDEFNAME = tbDEFNAME.Text; // DEFNAME
            string SetNAME = tbName.Text; // NAME
            string SetICON = tbICON.Text; // ICON
            string SetSOUND = tbSOUND.Text; // SOUND
            string SetCAN = tbCAN.Text; // CAN
            string SetDAM = tbDAM.Text; // DAM
            string SetArmor = tbAmor.Text; // armor
            string SetDESIRES = tbDESIRES.Text; // DESIRES
            string SetAVERSIONS = tbAVERSIONS.Text; // AVERSIONS
            string SetFOODTYPE = tbFOODTYPE.Text; // FOODTYPE
            string SetMAXFOOD = tbMAXFOOD.Text; // MAXFOOD
            string SetRESOURCES = tbRESOURCES.Text; // RESOURCES
            string SetCATEGORY = tbCATEGORY.Text; // CATEGORY
            string SetSUBSECTION = tbSUBSECTION.Text; // SUBSECTION
            string SetDESCRIPTION = tbDESCRIPTION.Text; // DESCRIPTION
            string SetNPC = tbNPC.Text; // NPC
            string SetSFame = tbSFAME.Text; // FAME
            string SetSKarma = tbSKARMA.Text; // KARMA
            string SetSTR = tbSTR.Text; // STR
            string SetDEX = tbDEX.Text; // DEX
            string SetINT = tbINT.Text; // INT
            string SetEVALUATINGINTEL = tbEVALUATINGINTEL.Text; // EVALUATINGINTEL
            string SetMAGERY = tbMAGERY.Text; // MAGERY
            string SetMAGICRESISTANCE = tbMAGICRESISTANCE.Text; // MAGICRESISTANCE
            string SetMEDITATION = tbMEDITATION.Text; // MEDITATION
            string SetPARRYING = tbPARRYING.Text; // PARRYING
            string SetTACTICS = tbTACTICS.Text; // TACTICS
            string SetWRESTLING = tbWRESTLING.Text; // WRESTLING
            string SetRESPHYSICAL = tbRESPHYSICAL.Text; // RESPHYSICAL
            string SetRESCOLD = tbRESCOLD.Text; // RESCOLD
            string SetRESENERGY = tbRESENERGY.Text; // RESENERGY
            string SetRESFIRE = tbRESFIRE.Text; // RESFIRE
            string SetRESPOISON = tbRESPOISON.Text; // RESPOISON

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
    RESENERGY={SetRESENERGY}
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

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSaveSphereScript_Click
        private void btSaveSphereScript_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SCP Files|*.scp";
            saveFileDialog.Title = "Save a SCP File";
            saveFileDialog.FileName = tbName.Text; // Setzt den Dateinamen auf den Inhalt von tbName

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.OpenFile()))
                {
                    writer.Write(richTextBoxEdit.Text); // Schreibt den Inhalt von richTextBoxEdit in die Datei
                }
            }
        }
        #endregion

        #region checkBoxSphereID_CheckedChanged
        private void checkBoxSphereID_CheckedChanged(Object sender, EventArgs e)
        {
            if (checkBoxSphereID.Checked)
            {
                int decValue = int.Parse(textBoxBody.Text);
                string hexValue = decValue.ToString("X");
                tbCHARDEF.Text = decValue < 16 ? "0" + hexValue : "0" + hexValue;
            }
        }
        #endregion

        #region btAnimationlistLoad
        private void btAnimationlistLoad_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UoFiddler\Animationlist.xml";
            if (File.Exists(path))
            {
                richTextBoxEdit.Text = File.ReadAllText(path);
            }
            else
            {
                MessageBox.Show("file does not exist: " + path);
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSaveAnimationlist
        private void btSaveAnimationlist_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UoFiddler\Animationlist.xml";
            File.WriteAllText(path, richTextBoxEdit.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btAppData_Click
        private void btAppData_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\UoFiddler";
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show("Directory does not exist: " + path);
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btUOOpenDirectory_Clic
        private void btUOOpenDirectory_Click(object sender, EventArgs e)
        {
            string path = textBoxPfad.Text;

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Please provide a directory.", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!Directory.Exists(path))
            {
                MessageBox.Show("The specified directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btInfoMobtypes
        private void btInfoMobtypes_Click(object sender, EventArgs e)
        {
            string infoText = "ENGLISH:\n" +
                      "mobtypes MONSTER 1000 indicates actions 12, 13, and 14 are for casting spells. mobtypes MONSTER 1008 signifies flying actions 19, 20, 21 are supported, along with casting actions 12 and 13. mobtypes MONSTER 1009 also supports flying actions 19, 20, 21, but only casting action 12. For mobtypes MONSTER A00, the uniqueness lies in the limited actions available, with attack actions 4, 5, and 6 serving as replacements for actions 12, 13, and 14. Lastly, mobtypes MONSTER 1004 includes additional actions 7 and 8, with action 12 being utilized for the monster's summoning animation.\n\n" +
                      "DEUTSCH:\n" +
                      "mobtypes MONSTER 1000 zeigt an, dass Aktionen 12, 13 und 14 zum Zaubern verwendet werden. mobtypes MONSTER 1008 deutet darauf hin, dass Flugaktionen 19, 20, 21 unterstützt werden, zusammen mit Zaubereiaktionen 12 und 13. mobtypes MONSTER 1009 unterstützt ebenfalls Flugaktionen 19, 20, 21, aber nur Zaubereiaktion 12. Bei mobtypes MONSTER A00 liegt die Besonderheit in den begrenzten verfügbaren Aktionen, wobei Angriffsaktionen 4, 5 und 6 als Ersatz für Aktionen 12, 13 und 14 dienen. Schließlich beinhalten mobtypes MONSTER 1004 zusätzliche Aktionen 7 und 8, wobei Aktion 12 für die Beschwörungsanimation des Monsters verwendet wird.";

            MessageBox.Show(infoText, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region richTextBoxEdit_KeyDown
        private void richTextBoxEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                searchToolStripMenuItem_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;  //Prevents the event from being forwarded
            }
        }
        #endregion 

        #region btnSingleEmptyAnimMul_Click
        private void btnSingleEmptyAnimMul_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select a directory to save the files";

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;

                    // Create the file anim.mul
                    using (FileStream fs = File.Create(Path.Combine(path, "anim.mul")))
                    {
                        // No data is written to the anim.mul file because it is supposed to be empty
                    }
                }
            }
        }
        #endregion

        #region Constants
        // Constants for the sizes of different creature types
        const int cHighDetail = 110;
        const int cLowDetail = 65;
        const int cHuman = 175;

        const int cHighDetailOLd = 1; // Old Version
        const int cLowDetailOld = 2; // Old Version
        const int cHumanOld = 3; // Old Version

        private int newIdCount = 0;
        #endregion

        #region btnBrowseClick
        private void btnBrowseClick(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                tbfilename.Text = openFileDialog.FileName;
            }
        }
        #endregion

        #region btnSetOutputDirectoryClick
        private void btnSetOutputDirectoryClick(object sender, EventArgs e)
        {
            // Open a FolderBrowserDialog to select the output directory
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Set the text of the output directory textbox to the selected path
                txtOutputDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }
        #endregion

        #region btnProcessClick
        private void btnProcessClick(object sender, EventArgs e)
        {
            // Flush tbProcessAminidx with every new process
            tbProcessAminidx.Clear();

            try
            {
                // Check if the file exists
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("Could not open anim.idx!\n");
                    return;
                }

                // Parse the creature ID from the text box
                int creatureID;
                if (!int.TryParse(txtOrigCreatureID.Text, System.Globalization.NumberStyles.HexNumber, null, out creatureID))
                {
                    tbProcessAminidx.AppendText("Enter a valid Animation ID\n");
                    return;
                }

                // Determine the number of copies based on the selected checkboxes
                int copyCount;
                if (chkHighDetail.Checked)
                {
                    copyCount = cHighDetail;
                }
                else if (chkLowDetail.Checked)
                {
                    copyCount = cLowDetail;
                }
                else if (chkHuman.Checked)
                {
                    copyCount = cHuman;
                }
                else
                {
                    // If no checkbox is selected, parse the number of copies from the text box
                    if (!int.TryParse(txtNewCreatureID.Text, out copyCount))
                    {
                        tbProcessAminidx.AppendText("Enter a valid copy count\n");
                        return;
                    }
                }

                // Determine the output filename
                string outputFilename = txtOutputFilename.Text;
                if (string.IsNullOrEmpty(outputFilename))
                {
                    outputFilename = Path.Combine(txtOutputDirectory.Text, "anim.idx"); // Use "anim.idx" in the output directory if no output file is specified
                }
                else
                {
                    // Append "amin", the outputFilename, and ".idx" to the output directory
                    outputFilename = Path.Combine(txtOutputDirectory.Text, "amin" + outputFilename + ".idx");
                }

                // Copy the original file to the output file
                File.Copy(filename, outputFilename, true);

                // Execute the copy process
                // CopyAnimationData(filename, creatureID, copyCount);

                // Execute the copy process on the new file
                CopyAnimationData(outputFilename, creatureID, copyCount);
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"An error has occurred: {ex.Message}\n");
            }
        }

        private void CopyAnimationData(string filename, int creatureID, int copyCount)
        {
            tbProcessAminidx.AppendText("Checking if new Animation ID is in use\n");

            using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
            {
                // Determine the index offset and length of data to read based on the creature ID
                int indexOffset, readLength;
                string creatureType;
                DetermineCreatureProperties(creatureID, out indexOffset, out readLength, out creatureType);

                tbProcessAminidx.AppendText($"Creature is a {creatureType}\n");

                // Read the animation index data into a byte array
                stream.Seek(indexOffset * 12, SeekOrigin.Begin);
                byte[] buffer = new byte[readLength];
                stream.Read(buffer, 0, readLength);
                tbProcessAminidx.AppendText($"Read {readLength} bytes of index-data for cID {creatureID}\n");

                // Copy the data the specified number of times
                for (int i = 0; i < copyCount; i++)
                {
                    // Find the end of the file
                    stream.Seek(0, SeekOrigin.End);

                    // Write the data directly to the stream
                    stream.Write(buffer, 0, readLength);

                    tbProcessAminidx.AppendText($"Wrote {readLength} bytes of index-data to cID {creatureID}\n");

                    // Increment the counter for each ID created
                    newIdCount++;
                }
                // Update the label with the number of IDs created
                lblNewIdCount.Text = $"Number of IDs created: {newIdCount}";
            }
        }
        #endregion

        #region DetermineCreatureProperties
        private void DetermineCreatureProperties(int creatureID, out int indexOffset, out int readLength, out string creatureType)
        {
            if (creatureID <= 199)
            {
                indexOffset = creatureID * cHighDetail;
                readLength = cHighDetail * 12;
                creatureType = "High Detail Critter";
            }
            else if (creatureID > 199 && creatureID <= 399)
            {
                indexOffset = (creatureID - 200) * cLowDetail + 22000;
                readLength = cLowDetail * 12;
                creatureType = "Low Detail Critter";
            }
            else
            {
                indexOffset = (creatureID - 400) * cHuman + 35000;
                readLength = cHuman * 12;
                creatureType = "Human or an Accessoire";
            }
        }
        #endregion

        #region btnProcessClickOldVersion
        private void btnProcessClickOldVersion(object sender, EventArgs e)
        {
            // Flush tbProcessAminidx with every new process
            tbProcessAminidx.Clear();

            try
            {
                int readLength = 0;
                int wroteLength = 0;
                string filename = tbfilename.Text;
                if (!File.Exists(filename))
                {
                    tbProcessAminidx.AppendText("Could not open anim.idx!\n");
                    return;
                }

                int creatureID;
                if (!int.TryParse(txtOrigCreatureID.Text, System.Globalization.NumberStyles.HexNumber, null, out creatureID))
                {
                    tbProcessAminidx.AppendText("Enter a valid Animation ID\n");
                    return;
                }

                int copyCount;
                if (!int.TryParse(txtNewCreatureID.Text, out copyCount))
                {
                    tbProcessAminidx.AppendText("Enter a valid copy count\n");
                    return;
                }

                tbProcessAminidx.AppendText("Checking if new Animation ID is in use\n");

                using (FileStream stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite))
                {
                    int indexOffset;
                    byte cAnimType;

                    if (creatureID <= 199)
                    {
                        indexOffset = creatureID * 110;
                        readLength = 110 * 12;
                        cAnimType = cHighDetailOLd;
                        tbProcessAminidx.AppendText("Creature is a High Detail Critter\n");
                    }
                    else if (creatureID > 199 && creatureID <= 399)
                    {
                        indexOffset = (creatureID - 200) * 65 + 22000;
                        readLength = 65 * 12;
                        cAnimType = cLowDetailOld;
                        tbProcessAminidx.AppendText("Creature is a Low Detail Critter\n");
                    }
                    else
                    {
                        indexOffset = (creatureID - 400) * 175 + 35000;
                        readLength = 175 * 12;
                        cAnimType = cHumanOld;
                        tbProcessAminidx.AppendText("Creature is a Human or an Accessoire\n");
                    }

                    stream.Seek(indexOffset * 12, SeekOrigin.Begin);
                    byte[] buffer = new byte[readLength];
                    stream.Read(buffer, 0, readLength);
                    tbProcessAminidx.AppendText($"Read {readLength} bytes of index-data for cID {creatureID}\n");

                    // Copy the data the number of times specified in copyCount
                    for (int i = 0; i < copyCount; i++)
                    {
                        // Find the end of the file
                        stream.Seek(0, SeekOrigin.End);

                        // Write the data directly to the stream
                        stream.Write(buffer, 0, readLength);

                        // Update wroteLength to the value of readLength since all data has been written
                        wroteLength = readLength;
                        tbProcessAminidx.AppendText($"Wrote {wroteLength} bytes of index-data to cID {creatureID}\n");

                        // Perform different actions based on the creature type
                        switch (cAnimType)
                        {
                            case cHighDetailOLd:
                                // Perform some action for high detail creatures
                                break;
                            case cLowDetailOld:
                                // Perform some action for low detail creatures
                                break;
                            case cHumanOld:
                                // Perform some action for human creatures
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tbProcessAminidx.AppendText($"An error has occurred: {ex.Message}\n");
            }
        }
        #endregion      
    }
}
// /***************************************************************************
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

                // Resistance
                SetDamageType(ResistanceType.Physical, 40);
                SetDamageType(ResistanceType.Fire, 40);
                SetDamageType(ResistanceType.Energy, 20);

                SetResistance(ResistanceType.Physical, 55, 65);
                SetResistance(ResistanceType.Fire, 30, 40);
                SetResistance(ResistanceType.Cold, 30, 40);
                SetResistance(ResistanceType.Poison, 30, 40);
                SetResistance(ResistanceType.Energy, 20, 30);

                // Skills
                SetSkill(SkillName.EvalInt, 10.4, 50.0);
                SetSkill(SkillName.Magery, 10.4, 50.0);
                SetSkill(SkillName.MagicResist, 85.3, 100.0);
                SetSkill(SkillName.Tactics, 97.6, 100.0);
                SetSkill(SkillName.Wrestling, 80.5, 92.5);

                Fame = {SetFame}; // Fame
                Karma = {SetKarma}; // Karma

                VirtualArmor = {SetArmor}; // Armor
                Tamable = true; // Tame false or true
                ControlSlots = 2; // Control slots you need
                MinTameSkill = 95.1;  //Skill required to tame

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
    }
}
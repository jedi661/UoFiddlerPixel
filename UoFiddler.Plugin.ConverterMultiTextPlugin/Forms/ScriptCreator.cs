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
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ScriptCreator : Form
    {
        private string currentFilePathWall;  // Saves the path of the last loaded file
        private string currentFilePathChair;  // Saves the path of the last loaded file
        private int searchStartIndex = 0;
        public ScriptCreator()
        {
            InitializeComponent();

            this.textBoxBookTitle.TextChanged += new System.EventHandler(this.textBoxBookTitle_TextChanged);
            this.textBoxBookAuthor.TextChanged += new System.EventHandler(this.textBoxBookAuthor_TextChanged);

            // Create an ImageList and add your images
            ImageList imageList = new ImageList();
            imageList.Images.Add("0x1C11", Properties.Resources._0x1C11); // 1 Picture
            imageList.Images.Add("0x1C13", Properties.Resources._0x1C13); // 2 ...
            imageList.Images.Add("0x1E20", Properties.Resources._0x1E20); // 3 ..
            imageList.Images.Add("0x2D9D", Properties.Resources._0x2D9D); // 4
            imageList.Images.Add("0x22C5", Properties.Resources._0x22C5); // 5
            imageList.Images.Add("0x23A0", Properties.Resources._0x23A0); // 6
            imageList.Images.Add("0x225A", Properties.Resources._0x225A); // 7
            imageList.Images.Add("0x238C", Properties.Resources._0x238C); // 8
            imageList.Images.Add("0x2252", Properties.Resources._0x2252); // 9
            imageList.Images.Add("0x2253", Properties.Resources._0x2253); // 10
            imageList.Images.Add("0x2254", Properties.Resources._0x2254); // 11
            imageList.Images.Add("0x2259", Properties.Resources._0x2259); // 12
            imageList.Images.Add("0xE3B", Properties.Resources._0xE3B); // 13
            imageList.Images.Add("0xFBE", Properties.Resources._0xFBE); // 14
            imageList.Images.Add("0xFEF", Properties.Resources._0xFEF); // 15
            imageList.Images.Add("0xFF0", Properties.Resources._0xFF0); // 16
            imageList.Images.Add("0xFF1", Properties.Resources._0xFF1); // 17
            imageList.Images.Add("0xFF2", Properties.Resources._0xFF2); // 18
            imageList.Images.Add("0xFF3", Properties.Resources._0xFF3); // 19
            imageList.Images.Add("0xFF4", Properties.Resources._0xFF4); // 20


            // Add more pictures here...

            // Fill the comboBoxBookGrafic with the images
            foreach (string key in imageList.Images.Keys)
            {
                comboBoxBookGrafic.Items.Add(key);
            }

            // Set the DrawItem event
            comboBoxBookGrafic.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxBookGrafic.DrawItem += (sender, e) =>
            {
                e.DrawBackground();
                e.Graphics.DrawImage(imageList.Images[e.Index], e.Bounds.Left, e.Bounds.Top);
                e.Graphics.DrawString(comboBoxBookGrafic.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + imageList.ImageSize.Width, e.Bounds.Top);
                e.DrawFocusRectangle();
            };

            // Set the SelectedIndexChanged event
            this.comboBoxBookGrafic.SelectedIndexChanged += new System.EventHandler(this.comboBoxBookGrafic_SelectedIndexChanged);

            // Fill the comboBoxEnableAutoformat with the options
            comboBoxEnableAutoformat.Items.Add("Enable Autoformat");
            comboBoxEnableAutoformat.Items.Add("Disable Autoformat");

            // Select “Disable Autoformat” by default
            comboBoxEnableAutoformat.SelectedItem = "Disable Autoformat";

            // Set the SelectedIndexChanged event
            this.comboBoxEnableAutoformat.SelectedIndexChanged += new System.EventHandler(this.comboBoxEnableAutoformat_SelectedIndexChanged);

            try
            {
                (currentFilePathWall, currentFilePathChair) = LoadPaths();
                textBoxPfadWall.Text = currentFilePathWall;
                textBoxPfadChair.Text = currentFilePathChair;
            }
            catch (System.IO.FileNotFoundException)
            {
                // The XML file doesn't exist yet, so we don't do anything.
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                // The directory doesn't exist yet, so we don't do anything.
            }

        }

        #region buttonCreateBookScript_Click
        private void buttonCreateBookScript_Click(object sender, EventArgs e)
        {
            string bookTitle = textBoxBookTitle.Text;
            string bookAuthor = textBoxBookAuthor.Text;
            string bookFilename = textBoxBookFilename.Text.Replace(" ", ""); // Removes spaces from the class name
            string grafikID = textBoxGrafikID.Text;
            string coverHue = textBoxCoverHue.Text;
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem != null ? comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat" : false;
            string bookContent = textBoxBookContent.Text;

            // Check that all required fields are filled out
            if (string.IsNullOrEmpty(textBoxBookTitle.Text) ||
                string.IsNullOrEmpty(textBoxBookAuthor.Text) ||
                string.IsNullOrEmpty(textBoxBookFilename.Text) ||
                string.IsNullOrEmpty(textBoxGrafikID.Text) ||
                string.IsNullOrEmpty(textBoxCoverHue.Text) ||
                comboBoxEnableAutoformat.SelectedItem == null)
            {
                MessageBox.Show("Please fill out all fields before creating the script.");
                return;
            }


            // Autoformat content if enabled
            if (enableAutoFormat)
            {
                // Replace all line breaks with a space
                bookContent = bookContent.Replace("\n", " ").Replace("\r", " ");

                // Perform a recursive scan to remove multiple spaces
                while (bookContent.Contains("  "))
                {
                    bookContent = bookContent.Replace("  ", " ");
                }

                // Remove newlines and spaces at the beginning
                bookContent = bookContent.TrimStart();
            }

            // Division of content into pages
            List<string> pageContents = SplitContentIntoPages(bookContent, 8); // Say 8 lines per page

            // Splitting content into pages based on characters
            pageContents = SplitContentIntoPagesByCharacters(bookContent, 90); // Assume 90 characters per page


            // Creation of the BookPageInfo objects for each page
            StringBuilder bookPageInfos = new StringBuilder();
            foreach (string pageContent in pageContents)
            {
                List<string> lines = SplitContentIntoLines(pageContent, 30); // Assume 30 characters per line
                string formattedPageContent = FormatPageContentForBookPageInfo(string.Join(",\n", lines));
                bookPageInfos.AppendLine(formattedPageContent);
            }


            // Generation of the script with the BookPageInfo objects
            string script = GenerateBookScript(bookTitle, bookAuthor, bookFilename, grafikID, coverHue, bookPageInfos.ToString());
            textBoxScriptResult.Text = script;
        }
        #endregion

        #region SplitContentIntoPages
        private List<string> SplitContentIntoPages(string content, int maxLinesPerPage)
        {
            string[] lines = content.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<string> pages = new List<string>();
            StringBuilder currentPage = new StringBuilder();
            int lineCount = 0;

            foreach (string line in lines)
            {
                currentPage.AppendLine(line);
                lineCount++;

                if (lineCount == maxLinesPerPage)
                {
                    pages.Add(currentPage.ToString());
                    currentPage.Clear();
                    lineCount = 0;
                }
            }

            if (currentPage.Length > 0) // Add the last page if there is one
            {
                pages.Add(currentPage.ToString());
            }

            return pages;
        }
        #endregion

        #region SplitContentIntoPagesByCharacters
        private List<string> SplitContentIntoPagesByCharacters(string content, int maxCharactersPerPage)
        {
            List<string> pages = new List<string>();
            for (int i = 0; i < content.Length; i += maxCharactersPerPage)
            {
                int length = Math.Min(maxCharactersPerPage, content.Length - i);
                pages.Add(content.Substring(i, length));
            }
            return pages;
        }
        #endregion

        #region SplitContentIntoLines
        private List<string> SplitContentIntoLines(string content, int maxCharactersPerLine)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < content.Length; i += maxCharactersPerLine)
            {
                int length = Math.Min(maxCharactersPerLine, content.Length - i);
                lines.Add(content.Substring(i, length));
            }
            return lines;
        }
        #endregion

        #region FormatPageContentForBookPageInfo
        private string FormatPageContentForBookPageInfo(string pageContent)
        {
            string[] lines = pageContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string formattedLines = string.Join(",\n                ", lines.Select(line => $"\"{line.Trim()}\""));
            return $@"new BookPageInfo
            (
                {formattedLines}
            ),";
        }
        #endregion

        #region GenerateBookScript
        private string GenerateBookScript(string title, string author, string filename, string graphicID, string coverHue, string pageInfos)
        {
            return $@"
            using System;
            using Server;            

            namespace Server.Items
            {{
                public class {filename} : BaseBook
            {{
                public static readonly BookContent Content = new BookContent
                (
                    ""{title}"", ""{author}"",
                    {pageInfos.TrimEnd(',', '\n', '\r')}
                );

                public override BookContent DefaultContent{{ get{{ return Content; }} }}

                [Constructable]
                public {filename}() : base( {graphicID}, false )
                {{
                    Hue = {coverHue};
                }}

                public {filename}(Serial serial) : base(serial)
                {{
                }}

                public override void Serialize(GenericWriter writer)
                {{
                    base.Serialize(writer);
                    writer.WriteEncodedInt((int)0);
                }}

                public override void Deserialize(GenericReader reader)
                {{
                    base.Deserialize(reader);
                    int version = reader.ReadEncodedInt();
                }}
            }}
        }}".Trim();
        }
        #endregion

        #region textBoxBookTitle_TextChanged
        private void textBoxBookTitle_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }
        #endregion

        #region textBoxBookAuthor_TextChanged
        private void textBoxBookAuthor_TextChanged(object sender, EventArgs e)
        {
            UpdateBookFilename();
        }
        #endregion

        #region UpdateBookFilename
        private void UpdateBookFilename()
        {
            string bookTitle = textBoxBookTitle.Text.Replace(" ", "_"); // Replaces spaces with underscores
            string bookAuthor = textBoxBookAuthor.Text.Replace(" ", "_"); // Replaces spaces with underscores

            // Create the class name based on the book title and author
            string className = "CBook" + bookTitle + bookAuthor;

            // Update the textBoxBookFilename
            textBoxBookFilename.Text = className;
        }
        #endregion

        #region comboBoxBookGrafic_SelectedIndexChanged
        private void comboBoxBookGrafic_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set the contents of the textBoxGrafikID to the selected hex address
            textBoxGrafikID.Text = comboBoxBookGrafic.SelectedItem.ToString();
        }
        #endregion

        #region comboBoxEnableAutoformat_SelectedIndexChanged
        private void comboBoxEnableAutoformat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if Autoformat is enabled
            bool enableAutoFormat = comboBoxEnableAutoformat.SelectedItem.ToString() == "Enable Autoformat" ? true : false;

            if (enableAutoFormat)
            {
                // Replace all line breaks with a space
                textBoxBookContent.Text = textBoxBookContent.Text.Replace("\n", " ").Replace("\r", " ");

                // Perform a recursive scan to remove multiple spaces
                while (textBoxBookContent.Text.Contains("  "))
                {
                    textBoxBookContent.Text = textBoxBookContent.Text.Replace("  ", " ");
                }

                // Remove newlines and spaces at the beginning
                textBoxBookContent.Text = textBoxBookContent.Text.TrimStart();
            }
        }
        #endregion

        #region buttonTest_Click
        private void buttonTest_Click(object sender, EventArgs e)
        {
            // Fill the TextBoxes and ComboBoxes with test data
            textBoxBookTitle.Text = "TestTitle";
            textBoxBookAuthor.Text = "TestAuthor";
            textBoxBookFilename.Text = "CBookTestTitleTestAuthor";
            comboBoxBookGrafic.SelectedIndex = 0; // Select the first item in the ComboBox
            textBoxGrafikID.Text = "0x1C11";
            textBoxCoverHue.Text = "684";
            comboBoxEnableAutoformat.SelectedIndex = 0; // Select the first item in the ComboBox
            textBoxBookContent.Text = "This is test content.";

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region DataGridViewColorCell
        public class DataGridViewColorCell : DataGridViewTextBoxCell
        {
            protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
            {
                Color color = Color.White;
                if (value is Color)
                {
                    color = (Color)value;
                }

                using (Brush brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, cellBounds);
                }
            }
        }
        #endregion

        #region btColorSet_Click        
        private void btColorSet_Click(object sender, EventArgs e)
        {
            // Create a new form
            Form colorForm = new Form();
            colorForm.Text = "Choose a color";
            colorForm.Size = new Size(1000, 600);  // Set the size of the form
            colorForm.FormBorderStyle = FormBorderStyle.FixedDialog;  // Disable form resizing

            // Create a new DataGridView
            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;  // Add this line
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;  // Adjust the column width to fill the available space
            colorForm.Controls.Add(dgv);

            // Add columns for the Hue values, colors, and Photoshop color codes
            dgv.Columns.Add("Hue", "Hue");
            dgv.Columns.Add("Color", "Color");
            dgv.Columns.Add("PSColor", "PS Color");

            // Add a color preview column
            DataGridViewColumn colorPreviewColumn = new DataGridViewColumn(new DataGridViewColorCell());
            colorPreviewColumn.Name = "ColorPreview";
            dgv.Columns.Add(colorPreviewColumn);

            // Fill the DataGridView with all colors in the 16-bit 555 color scheme
            for (int hue = 1; hue <= 32768; hue++)
            {
                Color color = HueToRGB555(hue);
                string psColor = RGBToPhotoshopColorCode(color);
                dgv.Rows.Add(hue, color.Name, psColor, color);
            }

            // When the user clicks on a row, copy the Hue value to the textBoxCoverHue
            dgv.CellClick += (s, args) =>
            {
                if (args.RowIndex >= 0)
                {
                    textBoxCoverHue.Text = dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString();
                    int hue = int.Parse(dgv.Rows[args.RowIndex].Cells["Hue"].Value.ToString());
                    textBoxCoverHue.BackColor = HueToRGB555(hue);
                }
            };

            // View the form
            //colorForm.ShowDialog();

            // View the form and bring it to the front
            colorForm.Show();
            colorForm.BringToFront();
        }

        // Convert a .NET Color to a Photoshop color code
        private string RGBToPhotoshopColorCode(Color color)
        {
            // Photoshop uses the hexadecimal RGB color code
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        #endregion

        #region Color HueToRGB555
        private Color HueToRGB555(int hue)
        {
            // Divide the Hue value into three parts
            int r = (hue >> 10) & 0x1F;
            int g = (hue >> 5) & 0x1F;
            int b = hue & 0x1F;

            // Scale the values to the range 0 to 255
            r = (r * 255) / 31;
            g = (g * 255) / 31;
            b = (b * 255) / 31;

            return Color.FromArgb(r, g, b);
        }

        #endregion

        #region textBoxCoverHue_TextChanged
        private void textBoxCoverHue_TextChanged(object sender, EventArgs e)
        {
            // Check whether textBoxCoverHue.Text contains a value
            if (!string.IsNullOrEmpty(textBoxCoverHue.Text))
            {
                // Try parsing the value
                int hue;
                if (int.TryParse(textBoxCoverHue.Text, out hue))
                {
                    // Convert the Hue value to a color
                    Color color = HueToRGB555(hue);

                    // Color the background of the textBoxCoverHue accordingly
                    textBoxCoverHue.BackColor = color;
                }
                else
                {
                    // The value could not be parsed
                    MessageBox.Show("Please enter a valid number.");
                }
            }
            else
            {
                // textBoxCoverHue.Text is empty
                textBoxCoverHue.BackColor = SystemColors.Window; // Reset the background to the default color
            }
        }
        #endregion

        #region btClipBoard_Click
        private void btClipBoard_Click(object sender, EventArgs e)
        {
            // Check if textBoxScriptResult.Text has a value
            if (!string.IsNullOrEmpty(textBoxScriptResult.Text))
            {
                // Copy the contents of textBoxScriptResult to the clipboard
                Clipboard.SetText(textBoxScriptResult.Text);
            }
            else
            {
                // Show a message if textBoxScriptResult.Text is empty
                MessageBox.Show("There is no text to copy to the clipboard.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSaveBookScript_Click
        private void btSaveBookScript_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C# Dateien (*.cs)|*.cs";
            saveFileDialog.FileName = textBoxBookFilename.Text;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(saveFileDialog.FileName, textBoxScriptResult.Text);
            }
        }
        #endregion

        #region btCreateScriptCrossbow
        private void btCreateScriptCrossbow_Click(object sender, EventArgs e)
        {
            // Name Script
            string SetNameCrossbow = tbNameCrossbow.Text.Replace(" ", "_");
            string SetCrossBowItem1 = tbCrossBowItem1.Text; // Item ID
            string SetCrossBowItem2 = tbCrossBowItem2.Text; // Item ID
            string SetWeight = tbWeight.Text; // Weight
            string SetLayer = tbTwoHanded.Text; // Layer
            string SetBaseRanged = tbBaseRanged.Text; // BaseRanged
            string SetEffectID = tbEffectID.Text; // EffectID
            string SetAmmoType = tbAmmoType.Text; // AmmoType
            string SetWeaponAbility1 = tbWeaponAbility1.Text; // WeaponAbility1
            string SetWeaponAbility2 = tbWeaponAbility2.Text; // WeaponAbility2
            string SetAosStrengthReq = tbAosStrengthReq.Text; // AosStrengthReq
            string SetAosMinDamage = tbAosMinDamage.Text; // AosMinDamage
            string SetAosMaxDamage1 = tbAosMaxDamage1.Text; // AosMaxDamage
            string SetAosMaxDamage2 = tbAosMaxDamage2.Text; // AosMaxDamage
            string SetAosSpeed = tbAosSpeed.Text; // AosSpeed
            string SetMlSpeed = tbMlSpeed.Text; // MlSpeed
            string SetOldStrengthReq = tbOldStrengthReq.Text; // OldStrengthReq
            string SetOldMinDamage = tbOldMinDamage.Text; // OldMinDamage
            string SetOldMaxDamage = tbOldMaxDamage.Text; // OldMaxDamage
            string SetOldSpeed = tbOldSpeed.Text; // OldSpeed
            string SetDefMaxRange = tbDefMaxRange.Text; // DefMaxRange
            string SetInitMinHits = tbInitMinHits.Text; // InitMinHits
            string SetInitMaxHits = tbInitMaxHits.Text; // InitMaxHits

            string script = $@"
    using System;
    using Server;
    using Ultima

    namespace Server.Items
    {{
        [FlipableAttribute({SetCrossBowItem1}, {SetCrossBowItem2})]
        public class {SetNameCrossbow} : {SetBaseRanged}
        {{
            [Constructable]
            public {SetNameCrossbow}()
                : base({SetCrossBowItem1})
            {{
                this.Weight = {SetWeight};
                this.Layer = Layer.{SetLayer};
            }}

            public {SetNameCrossbow}(Serial serial)
                : base(serial)
            {{
            }}

            public override int EffectID
            {{
                get
                {{
                    return {SetEffectID};
                }}
            }}
            public override Type AmmoType
            {{
                get
                {{
                    return typeof({SetAmmoType});
                }}
            }}
            public override Item Ammo
            {{
                get
                {{
                    return new {SetAmmoType}();
                }}
            }}
            public override WeaponAbility PrimaryAbility
            {{
                get
                {{
                    return WeaponAbility.{SetWeaponAbility1};
                }}
            }}
            public override WeaponAbility SecondaryAbility
            {{
                get
                {{
                    return WeaponAbility.{SetWeaponAbility2};
                }}
            }}
            public override int AosStrengthReq
            {{
                get
                {{
                    return {SetAosStrengthReq};
                }}
            }}
            public override int AosMinDamage
            {{
                get
                {{
                    return {SetAosMinDamage};
                }}
            }}
            public override int AosMaxDamage
            {{
                get
                {{
                    return Core.ML ? {SetAosMaxDamage1} : {SetAosMaxDamage2};
                }}
            }}
            public override int AosSpeed
            {{
                get
                {{
                    return {SetAosSpeed};
                }}
            }}
            public override float MlSpeed
            {{
                get
                {{
                    return {SetMlSpeed};
                }}
            }}
            public override int OldStrengthReq
            {{
                get
                {{
                    return {SetOldStrengthReq};
                }}
            }}
            public override int OldMinDamage
            {{
                get
                {{
                    return {SetOldMinDamage};
                }}
            }}
            public override int OldMaxDamage
            {{
                get
                {{
                    return {SetOldMaxDamage};
                }}
            }}
            public override int OldSpeed
            {{
                get
                {{
                    return {SetOldSpeed};
                }}
            }}
            public override int DefMaxRange
            {{
                get
                {{
                    return {SetDefMaxRange};
                }}
            }}
            public override int InitMinHits
            {{
                get
                {{
                    return {SetInitMinHits};
                }}
            }}
            public override int InitMaxHits
            {{
                get
                {{
                    return {SetInitMaxHits};
                }}
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
            }}
        }}
    }}";

            richTextBoxScriptEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCopyToClipboard
        private void btCopyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBoxScriptEdit.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCopyToClipboardAmin
        private void btCopyToClipboardAmin_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextAminBoxEdit.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCopyToClipboardWall
        private void btCopyToClipboardWall_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBoxWallScript.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCopyToClipboardChair
        private void btCopyToClipboardChair_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBoxScriptMiscellaneous.Text);

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateScriptBow_Click
        private void btCreateScriptBow_Click(object sender, EventArgs e)
        {
            string SetNameCrossbow = tbBowName.Text.Replace(" ", "_");
            string SetBow = tbBow.Text; // Bow Class or Other
            string SetItemID = tbItemID.Text; // ItemID
            string SetName = tbName.Text; // Name
            string SetHue = tbHue.Text; // Hue
            string SetStrRequirement = tbStrRequirement.Text; // StrRequirement
            string SetAttributes1 = tbAttributes1.Text; // Attributes
            string SetSpellChanneling = tbSpellChanneling.Text; // SpellChanneling
            string SetWeaponAttributes1 = tbWeaponAttributes1.Text; // WeaponAttributes 1
            string SetHitLightning = tbHitLightning.Text; // tbHitLightning
            string SetMinDamage = tbMinDamage.Text; // MinDamage
            string SetMaxDamage = tbMaxDamage.Text; // MaxDamage
            string SetWeaponAttributes2 = tbWeaponAttributes2.Text; // DurabilityBonus
            string SetValue1 = tbValue1.Text; // 1000
            string SetLootType = tbLootType.Text; // LootType
            string SetWeaponAttributes3 = tbWeaponAttributes3.Text; // WeaponSpeed
            string SetValue2 = tbValue2.Text; // 20
            string SetWeaponAttributes4 = tbWeaponAttributes4.Text; // HitLeechHits
            string SetValue3 = tbValue3.Text; // 5
            string SetWeaponAttributes5 = tbWeaponAttributes5.Text; // HitHarm
            string SetValue4 = tbValue4.Text; // 10
            string SetWeaponAttributes6 = tbWeaponAttributes6.Text; // HitFireball
            string SetValue5 = tbValue5.Text; // 15


            string script = $@"
using System;
using Server;
using Ultima;

namespace Server.Items
{{
    public class {SetNameCrossbow} : {SetBow}
    {{
        [Constructable]
        public {SetNameCrossbow}()
        {{
            this.ItemID = {SetItemID};
            Name = ""{SetName}"";
            Hue = {SetHue};
            StrRequirement = {SetStrRequirement};
            Attributes.{SetAttributes1} = {SetSpellChanneling};
            WeaponAttributes.{SetWeaponAttributes1} = {SetHitLightning};
            MinDamage = {SetMinDamage};
            MaxDamage = {SetMaxDamage};
            WeaponAttributes.{SetWeaponAttributes2} = {SetValue1};
            LootType = LootType.{SetLootType};

            // Hinzugefügte Attribute
            Attributes.{SetWeaponAttributes3} = {SetValue2};
            WeaponAttributes.{SetWeaponAttributes4} = {SetValue3};
            WeaponAttributes.{SetWeaponAttributes5} = {SetValue4};
            WeaponAttributes.{SetWeaponAttributes6} = {SetValue5};
        }}

        public override void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {{
            phys = fire = pois = cold = chaos = direct = 0;
            nrgy = 100;
        }}

        public {SetNameCrossbow}(Serial serial) : base(serial)
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
        }}
    }}
}}";
            richTextBoxScriptEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateSwordScript_Click
        private void btCreateSwordScript_Click(object sender, EventArgs e)
        {
            string SetScriptName = tbScriptName.Text.Replace(" ", "_");
            string Settypeof1 = tbStypeof1.Text; // DefBlacksmithy
            string Settypeof2 = tbStypeof2.Text; // DreadSword
            string SetBaseSword = tbBaseSword.Text; // BaseSword
            string SetSGraficID1 = tbSGraficID1.Text; // Grafik ID
            string SetSGraficID2 = tbSGraficID2.Text; // Grafik ID
            string SetSWeight = tbSWeight.Text; // Weight
            string SetSWeaponAbility1 = tbSWeaponAbility1.Text; // WeaponAbility
            string SetSWeaponAbility2 = tbSWeaponAbility2.Text; // WeaponAbility
            string SetSWeaponAbility3 = tbSWeaponAbility3.Text; // WeaponAbility
            string SetSWeaponAbility4 = tbSWeaponAbility4.Text; // WeaponAbility
            string SetSAosStrengthReq = tbSAosStrengthReq.Text; // AosStrengthReq 30
            string SetSAosMinDamage = tbSAosMinDamage.Text; // AosMinDamage 18
            string SetSAosMaxDamage = tbSAosMaxDamage.Text; // AosMaxDamage 17
            string SetSAosSpeed = tbSAosSpeed.Text; // AosSpeed 33
            string SetSMlSpeed = tbSMlSpeed.Text; // MlSpeed 3.25f
            string SetSOldStrengthReq = tbSOldStrengthReq.Text; // OldStrengthReq 30
            string SetSOldMinDamage = tbSOldMinDamage.Text; // OldMinDamage 5
            string SetSOldMaxDamage = tbSOldMaxDamage.Text; // OldMaxDamage 29
            string SetSOldSpeed = tbSOldSpeed.Text; // OldSpeed 45
            string SetSDefHitSound = tbSDefHitSound.Text; // DefHitSound 0x237
            string SetSDefMissSound = tbSDefMissSound.Text; // DefMissSound 0x23A
            string SetSInitMinHits = tbSInitMinHits.Text; // InitMinHits 31
            string SetSInitMaxHits = tbSInitMaxHits.Text; // InitMaxHits 100

            string script = $@"
using System;
using Server.Engines.Craft;
using Server;
using Ultima

namespace Server.Items
{{
    [Alterable(typeof({Settypeof1}), typeof({Settypeof2}))]
    [FlipableAttribute({SetSGraficID1}, {SetSGraficID2})]
    public class {SetScriptName} : {SetBaseSword}
    {{
        [Constructable]
        public {SetScriptName}()
            : base({SetSGraficID1})
        {{
            this.Weight = {SetSWeight};
        }}

        public {SetScriptName}(Serial serial)
            : base(serial)
        {{
        }}

        public override WeaponAbility {SetSWeaponAbility1}
        {{
            get
            {{
                return WeaponAbility.{SetSWeaponAbility2};
            }}
        }}
        public override WeaponAbility {SetSWeaponAbility3}
        {{
            get
            {{
                return WeaponAbility.{SetSWeaponAbility4};
            }}
        }}
        public override int AosStrengthReq
        {{
            get
            {{
                return {SetSAosStrengthReq};
            }}
        }}
        public override int AosMinDamage
        {{
            get
            {{
                return {SetSAosMinDamage};
            }}
        }}
        public override int AosMaxDamage
        {{
            get
            {{
                return {SetSAosMaxDamage};
            }}
        }}
        public override int AosSpeed
        {{
            get
            {{
                return {SetSAosSpeed};
            }}
        }}
        public override float MlSpeed
        {{
            get
            {{
                return {SetSMlSpeed};
            }}
        }}
        public override int OldStrengthReq
        {{
            get
            {{
                return {SetSOldStrengthReq};
            }}
        }}
        public override int OldMinDamage
        {{
            get
            {{
                return {SetSOldMinDamage};
            }}
        }}
        public override int OldMaxDamage
        {{
            get
            {{
                return {SetSOldMaxDamage};
            }}
        }}
        public override int OldSpeed
        {{
            get
            {{
                return {SetSOldSpeed};
            }}
        }}
        public override int DefHitSound
        {{
            get
            {{
                return {SetSDefHitSound};
            }}
        }}
        public override int DefMissSound
        {{
            get
            {{
                return {SetSDefMissSound};
            }}
        }}
        public override int InitMinHits
        {{
            get
            {{
                return {SetSInitMinHits};
            }}
        }}
        public override int InitMaxHits
        {{
            get
            {{
                return {SetSInitMaxHits};
            }}
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
        }}
    }}
}}";
            richTextBoxScriptEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }

        #endregion

        #region btStaffStript_Click
        private void btStaffStript_Click(object sender, EventArgs e)
        {
            string SetStaffscriptName = tbStaffName.Text.Replace(" ", "_");
            string SetClassNameStaff = tbStaffClassName.Text; // BlackStaff
            string SetStaffItemID = tbStaffItemID.Text; // StaffItemID 0x0DF1
            string SetStaffItemName = tbStaffItemName.Text; // Staff Name
            string SetStaffHue1 = tbStaffHue1.Text; // 0x4F2
            string SetStaffHue2 = tbStaffHue2.Text; // 0x4EF
            string SetStaffStrRequirement = tbStaffStrRequirement.Text; // StrRequirement 12
            string SetStaffAttributes1 = tbStaffAttributes1.Text; // SpellChanneling
            string SetStaffAttributes2 = tbStaffAttributes2.Text; // 1
            string SetMageWeapon1 = tbMageWeapon1.Text; // 15
            string SetSpellDamage1 = tbSpellDamage1.Text; // 5
            string SetStaffCastRecovery = tbStaffCastRecovery.Text; // 2
            string SetStaffLowerManaCost = tbStaffLowerManaCost.Text; // 5
            string SetStaffInitMinHits = tbStaffInitMinHits.Text; // 255
            string SetStaffInitMaxHits = tbStaffInitMaxHits.Text; // 255
            string SetStaffLootType = tbStaffLootType.Text; // Blessed true or false
            string SetStaffWeaponAttributes1 = tbStaffWeaponAttributes1.Text; // HitFireball
            string SetStaffValue1 = tbStaffValue1.Text; // 20
            string SetStaffWeaponAttributes2 = tbStaffWeaponAttributes2.Text; // HitHarm
            string SetStaffValue2 = tbStaffValue2.Text; // 10
            string SetDurabilityBonus = tbStaffDurabilityBonus.Text; // 1000

            string script = $@"
using System;
using Server;
using Ultima

namespace Server.Items
{{
    public class {SetStaffscriptName} : {SetClassNameStaff}
    {{
        public override bool IsArtifact {{ get {{ return true; }} }}
        [Constructable]
        public {SetStaffscriptName}()
        {{
            this.ItemID = {SetStaffItemID};
            Name = ""{SetStaffItemName}"";

            Hue = Utility.RandomBool() ? {SetStaffHue1} : {SetStaffHue2};
            StrRequirement = {SetStaffStrRequirement};
            WeaponAttributes.MageWeapon = {SetMageWeapon1};
            Attributes.{SetStaffAttributes1} = {SetStaffAttributes2};
            Attributes.SpellDamage = {SetSpellDamage1};
            Attributes.CastRecovery = {SetStaffCastRecovery};
            Attributes.LowerManaCost = {SetStaffLowerManaCost};
            LootType = LootType.{SetStaffLootType};
            WeaponAttributes.{SetStaffWeaponAttributes1} = {SetStaffValue1};
            WeaponAttributes.{SetStaffWeaponAttributes2} = {SetStaffValue2};
            WeaponAttributes.DurabilityBonus = {SetDurabilityBonus};

        }}

        public {SetStaffscriptName}(Serial serial)
            : base(serial)
        {{
        }}

        public override int LabelNumber
        {{
            get
            {{
                return 1070692;
            }}
        }}
        public override int InitMinHits
        {{
            get
            {{
                return {SetStaffInitMinHits};
            }}
        }}
        public override int InitMaxHits
        {{
            get
            {{
                return {SetStaffInitMaxHits};
            }}
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
        }}
    }}
}}";
            richTextBoxScriptEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateAxeSxript_Click
        private void btCreateAxeSxript_Click(object sender, EventArgs e)
        {
            string SetAxeScriptName = tbAxeName.Text.Replace(" ", "_");
            string SetAxeClassName = tbAxeClassName.Text; // BattleAxe
            string SetAxeIdName1 = tbAxeIdName1.Text; // 0xF47
            string SetAxeIdName2 = tbAxeIdName2.Text; // 0xF47
            string SetAxeItemName = tbAxeItemName.Text; // AxeOfAbandon
            string SetAxeLabelNumber = tbAxeLabelNumber.Text; // 1113863
            string SetIsArtifact = tbAxeIsArtifact.Text; // true or false
            string SetAxeHue = tbAxeHue.Text; // 556
            string SetAxeWeaponAttributes1 = tbAxeWeaponAttributes1.Text; // HitLowerDefend
            string SetAxeWeaponAttributes2 = tbAxeWeaponAttributes2.Text; // BattleLust
            string SetAxeWeaponAttributes3 = tbAxeWeaponAttributes3.Text; // HitFireball
            string setAxeValue1 = tbAxeValue1.Text; // 40
            string setAxeValue2 = tbAxeValue2.Text; // 1
            string setAxeValue3 = tbAxeValue3.Text; // 15
            string SetAttackChance = tbAttackChance.Text; // 15 
            string SetAxeLootType = tbAxeLootType.Text; // true of false
            string SetDefendChance = tbAxeDefendChance.Text; //10
            string SetAxeCastSpeed = tbAxeCastSpeed.Text; // 1
            string SetAxeInitMinHits = tbAxeInitMinHits.Text; // 255
            string SetAxeInitMaxHits = tbAxeInitMaxHits.Text; // 255
            string SetAxeWeaponSpeed = tbAxeWeaponSpeed.Text; // 30
            string SetAxeWeaponDamage = tbAxeWeaponDamage.Text; // 50
            string SetAxeStrRequirement = tbAxeStrRequirement.Text; // 35

            string script = $@"
using System;
using Server;
using Ultima

namespace Server.Items
{{
    [FlipableAttribute({SetAxeIdName1}, {SetAxeIdName2})]
    public class {SetAxeScriptName} : {SetAxeClassName}
    {{
        public override bool IsArtifact {{ get {{ return {SetIsArtifact}; }} }}
        public override int LabelNumber {{ get {{ return {SetAxeLabelNumber}; }} }} // Axe of Abandon 1113863
        
        [Constructable]
        public {SetAxeScriptName}()
        {{	
            Name = ""{SetAxeItemName}"";
            Hue = {SetAxeHue};		
            WeaponAttributes.{SetAxeWeaponAttributes1} = {setAxeValue1};
            WeaponAttributes.{SetAxeWeaponAttributes2} = {setAxeValue2};
            WeaponAttributes.{SetAxeWeaponAttributes3} = {setAxeValue3};
            Attributes.AttackChance = {SetAttackChance};
            Attributes.DefendChance = {SetDefendChance};	
            Attributes.CastSpeed = {SetAxeCastSpeed};	
            Attributes.WeaponSpeed = {SetAxeWeaponSpeed};
            Attributes.WeaponDamage = {SetAxeWeaponDamage};
            StrRequirement = {SetAxeStrRequirement};
            LootType = LootType.{SetAxeLootType};
        }}

        public {SetAxeScriptName}(Serial serial)
            : base(serial)
        {{
        }}

        public override int InitMinHits
        {{
            get
            {{
                return {SetAxeInitMinHits};
            }}
        }}
        public override int InitMaxHits
        {{
            get
            {{
                return {SetAxeInitMaxHits};
            }}
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
        }}
    }}
}}";
            richTextBoxScriptEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateScriptAmin
        private void btCreateScriptAmin_Click(object sender, EventArgs e)
        {
            string SetAminScriptName = textBoxBodyAmin.Text.Replace(" ", "_");
            string SetAminIDbodyValue = tbAminIDbodyValue.Text;
            string SetAminItemID = tbAminItemID1.Text;
            string SetAminItemID2 = tbAminItemID2.Text;
            string SetAminItemID3 = tbAminItemID3.Text;
            string SetAminBaseSoundID1 = tBBaseSoundID1.Text;
            string SetAminBaseSoundID2 = tBBaseSoundID2.Text;


            //strength
            string SetAminStrNR1 = tbStr1.Text; // strength
            string SetAminStrNR2 = tbStr2.Text; // strength
            // Dex
            string SetAminDexNR1 = tbDex1.Text; // Dex
            string SetAminDexNR2 = tbDex2.Text; // Dex
            // Int
            string SetAminIntN1 = tbInt1.Text; // Int
            string SetAminIntN2 = tbInt2.Text; // Int
            //Hitpoints
            string SetAminHitp1 = tbSetHits1.Text; // Hitpoints
            string SetAminHitp2 = tbSetHits2.Text; // Hitpoints
            // Damage
            string SetAminDam1 = tbSetHits1.Text; // Damage
            string SetAminDam2 = tbSetHits2.Text; // Damage
            // Fame
            string SetAminFame = tbFame.Text; // Fame
            // Karma
            string SetAminKarma = tbKarma.Text; // Karma
            // Armor
            string SetAminArmor = tbVirtualArmor.Text; // Armor
            // Damage Resistance
            string SetAminPhysical = tbPhysical.Text; // Damage Resistance Physical
            string SetAminFire = tbFire.Text; // Damage Resistance Fire
            string SetAminEnergy = tbEnergy.Text; // Damage Resistance Energy
            // Resistance
            string SetAminRPhysical1 = tbRPhysical1.Text; // Resistance Physical
            string SetAminRPhysical2 = tbRPhysical2.Text; // Resistance Physical
            string SetAminRFire1 = tbRFire1.Text; // Resistance Fire
            string SetAminRFire2 = tbRFire2.Text; // Resistance Fire
            string SetAminRCold1 = tbRCold1.Text; // Resistance Cold
            string SetAminRCold2 = tbRCold2.Text; // Resistance Cold
            string SetAminRPoison1 = tbRPoison1.Text; // Resistance Poison
            string SetAminRPoison2 = tbRPoison2.Text; // Resistance Poison
            string SetAminREnergy1 = tbREnergy1.Text; // Resistance Energy
            string SetAminREnergy2 = tbREnergy2.Text; // Resistance Energy
            // Set Skills
            string SetAminEvalInt1 = tbSEvalInt1.Text; // Set Skill EvalInt
            string SetAminEvalInt2 = tbSEvalInt2.Text; // Set Skill EvalInt
            string SetAminMagery1 = tbSetMagery1.Text; // Set Skill Magery
            string SetAminMagery2 = tbSetMagery2.Text; // Set Skill Magery
            string SetAminMagicResist1 = tbSMagicResist1.Text; // Set Skill Magery Resist
            string SetAminMagicResist2 = tbSMagicResist2.Text; // Set Skill Magery Resist
            string SetAminTactics1 = tbSTactics1.Text; // Set Skill Tactics
            string SetAminTactics2 = tbSTactics2.Text; // Set Skill Tactics
            string SetAminWrestling1 = tbSWrestling1.Text; // Set Skill Wrestling
            string SetAminWrestling2 = tbSWrestling2.Text; // Set Skill Wrestling
            // Animal
            string SetAminTamable = tbTamable.Text; // Set True or False
            string SetAminControlSlots = tbSControlSlots.Text; // Set Controll Slots you need from 1 to 6
            string SetAminMinTameSkill = tbMinTameSkill.Text; // Set Tame Skill you Need

            string script = $@"
            using System;
            using Server;
            using Server.Items;
            using Server.Mobiles;

            namespace Server.Mobiles
            {{
            [CorpseName(""a {SetAminScriptName} corpse"")]
            public class {SetAminScriptName} : BaseMount
            {{
            
                [Constructable]
                public {SetAminScriptName}() : this(""a {SetAminScriptName}"")                
                {{
                }}
            
            [Constructable]
            public {SetAminScriptName}(string name) : base(name, 0x74, 0x3EBB, AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
            {{
                BaseSoundID = {SetAminBaseSoundID1};       // Sound ID 1
                SetStr({SetAminStrNR1}, {SetAminStrNR2});         // strength
                SetDex({SetAminDexNR1}, {SetAminDexNR2});         // Dex
                SetInt({SetAminIntN1}, {SetAminIntN2});           // Int

                SetHits({SetAminHitp1}, {SetAminHitp2});          // Hitpoints
                SetDamage({SetAminDam1}, {SetAminDam2});    // Damage

                // Damage Resistance
                SetDamageType(ResistanceType.Physical, {SetAminPhysical});
                SetDamageType(ResistanceType.Fire, {SetAminFire});
                SetDamageType(ResistanceType.Energy, {SetAminEnergy});

                // Set Resistance
                SetResistance(ResistanceType.Physical, {SetAminRPhysical1}, {SetAminRPhysical2});
                SetResistance(ResistanceType.Fire, {SetAminRFire1}, {SetAminRFire2});
                SetResistance(ResistanceType.Cold, {SetAminRCold1}, {SetAminRCold2});
                SetResistance(ResistanceType.Poison, {SetAminRPoison1}, {SetAminRPoison2});
                SetResistance(ResistanceType.Energy, {SetAminREnergy1}, {SetAminREnergy2});

                // Set Skills
                SetSkill(SkillName.EvalInt, {SetAminEvalInt1}, {SetAminEvalInt2});
                SetSkill(SkillName.Magery, {SetAminMagery1}, {SetAminMagery2});
                SetSkill(SkillName.MagicResist, {SetAminMagicResist1}, {SetAminMagicResist2});
                SetSkill(SkillName.Tactics, {SetAminTactics1}, {SetAminTactics2});
                SetSkill(SkillName.Wrestling, {SetAminWrestling1}, {SetAminWrestling2});

                Fame = {SetAminFame}; // Fame
                Karma = {SetAminKarma}; // Karma

                VirtualArmor = {SetAminArmor}; // Armor
                Tamable = {SetAminTamable}; // Tame false or true
                ControlSlots = {SetAminControlSlots}; // Control slots you need
                MinTameSkill = {SetAminMinTameSkill};  //Skill required to tame

                switch (Utility.Random(3))
                {{
                    case 0:
                        {{
                            BodyValue = {SetAminIDbodyValue}; //Animation BodyID
                            ItemID = {SetAminItemID}; // Item Slot Id 1
                            break;
                        }}
                    case 1:
                        {{
                            BodyValue = {SetAminIDbodyValue}; //Animation BodyID
                            ItemID = {SetAminItemID2}; // Item Slot Id 2
                            break;
                        }}
                    case 2:
                        {{
                            BodyValue = {SetAminIDbodyValue}; //Animation BodyID
                            ItemID = {SetAminItemID3}; // Item Slot Id 3
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

            public {SetAminScriptName}(Serial serial) : base(serial)
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
                if (BaseSoundID == {SetAminBaseSoundID2}) // Sound ID 2
                    BaseSoundID = {SetAminBaseSoundID1}; //Sound ID 1
            }}

         }}
    }}";
            richTextAminBoxEdit.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSandWallScript_Click
        private void btSandWallScript_Click(object sender, EventArgs e)
        {
            string scriptName = tbSandWallNane1.Text.Replace(" ", "_");
            string SetDamageableItem1 = tbDamageableItem1.Text; // DamageableItem
            string SetIDItems1 = tbIDItems1.Text; // 969
            string SetIDItems2 = tbIDItems2.Text; // 631
            string SetIDName1 = tbIDName1.Text; // Stone Wall
            string SetIDHue1 = tbIDHue1.Text; // hue
            string SetVeryEasy1 = tbItemLevel1.Text; // VeryEasy
            string SetMovable1 = tbMovable1.Text; // Movable

            string scriptName2 = tbSandWallNane2.Text.Replace(" ", "_");
            string SetDamageableItem2 = tbDamageableItem2.Text; // DamageableItem
            string SetIDItems3 = tbIDItems3.Text; // 968
            string SetIDItems4 = tbIDItems4.Text; // 636
            string SetIDName2 = tbIDName2.Text; // Stone Wall
            string SetIDHue2 = tbIDHue2.Text; // hue
            string SetVeryEasy2 = tbItemLevel2.Text; // VeryEasy
            string SetMovable2 = tbMovable2.Text; // Movable

            string script = $@"
using System;

namespace Server.Items
{{
    public class {scriptName} : {SetDamageableItem1}
    {{
        [Constructable]
        public {scriptName}()
            : base({SetIDItems1}, {SetIDItems2})
        {{
            this.Name = ""{SetIDName1}"";
            this.Hue = {SetIDHue1};

            this.Level = ItemLevel.{SetVeryEasy1};
            this.Movable = {SetMovable1};
        }}

        public {scriptName}(Serial serial)
            : base(serial)
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
        }}
    }}

    public class {scriptName2} : {SetDamageableItem2}
    {{
        [Constructable]
        public {scriptName2}()
            : base({SetIDItems3}, {SetIDItems4})
        {{
            this.Name = ""{SetIDName2}"";
            this.Hue = {SetIDHue2};

            this.Level = ItemLevel.{SetVeryEasy2};
            this.Movable = {SetMovable2};
        }}

        public {scriptName2}(Serial serial)
            : base(serial)
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
        }}
    }}
}}";
            richTextBoxWallScript.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }

        #endregion

        #region  btCreateScriptChair_Click
        private void btCreateScriptChair_Click(object sender, EventArgs e)
        {
            string SetScriptName1 = tbClassNameChair1.Text.Replace(" ", "_"); // FancyWoodenChairCushion
            string SetScriptName2 = tbClassNameChair2.Text; // CraftableFurniture
            string SetItemChair1 = tbItemChair1.Text; // 0xB4F
            string SetItemChair2 = tbItemChair2.Text; // 0xB4E
            string SetItemChair3 = tbItemChair3.Text; // 0xB50
            string SetItemChair4 = tbItemChair4.Text; // 0xB51
            string SetWeightChair = tbWeightChair.Text; // 20.0

            string script = $@"
using System;

namespace Server.Items
{{
    [Furniture]
    [Flipable({SetItemChair1}, {SetItemChair2}, {SetItemChair3}, {SetItemChair4})]
    public class {SetScriptName1} : {SetScriptName2}
    {{
        [Constructable]
        public {SetScriptName1}()
            : base({SetItemChair1})
        {{
            this.Weight = {SetWeightChair};
        }}

        public {SetScriptName1}(Serial serial)
            : base(serial)
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

            if (this.Weight == 6.0)
                this.Weight = {SetWeightChair};
        }}
    }}
}}";
            richTextBoxScriptMiscellaneous.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region  ShowFolderBrowserDialog
        private string ShowFolderBrowserDialog()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    return folderBrowserDialog.SelectedPath;
                }
            }
            return null;
        }
        #endregion

        #region SavePaths
        // Save the paths in an XML file
        public void SavePaths(string pathWall, string pathChair)
        {
            string directoryPath = "DirectoryisSettings";
            if (!System.IO.Directory.Exists(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }

            XDocument doc = new XDocument(
                new XElement("Paths",
                    new XElement("PathWall", pathWall),
                    new XElement("PathChair", pathChair)
                )
            );
            doc.Save(System.IO.Path.Combine(directoryPath, "LoadDirXML.xml"));
        }
        #endregion

        #region LoadPaths
        // Loading paths from an XML file
        public (string, string) LoadPaths()
        {
            string directoryPath = "DirectoryisSettings";
            XDocument doc = XDocument.Load(System.IO.Path.Combine(directoryPath, "LoadDirXML.xml"));
            string pathWall = doc.Root.Element("PathWall").Value;
            string pathChair = doc.Root.Element("PathChair").Value;
            return (pathWall, pathChair);
        }
        #endregion

        #region btLoadPfadWall_Click
        private void btLoadPfadWall_Click(object sender, EventArgs e)
        {
            string selectedPath = ShowFolderBrowserDialog();
            if (selectedPath != null)
            {
                textBoxPfadWall.Text = selectedPath;
                SavePaths(selectedPath, textBoxPfadChair.Text);
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btLoadPfadChair
        private void btLoadPfadChair_Click(object sender, EventArgs e)
        {
            string selectedPath = ShowFolderBrowserDialog();
            if (selectedPath != null)
            {
                textBoxPfadChair.Text = selectedPath;
                SavePaths(textBoxPfadWall.Text, selectedPath);
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSaveXML_Click
        private void btSaveXML_Click(object sender, EventArgs e)
        {
            SavePaths(textBoxPfadWall.Text, textBoxPfadChair.Text);
        }
        #endregion

        #region btWallTXTLoad_Click
        private void btWallTXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "walls.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript.Text = Path.GetFileName(path);  // Displays the file name in lbFileName1
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btMiscTXTLoad_Click
        private void btMiscTXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "misc.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btProfTXTLoad
        private void btProfTXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Prof.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btroofTXTLoad
        private void btroofXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "roof.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btStairsTXTLoad
        private void btstairsXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "stairs.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btDoorsTXTLoad
        private void btDroorsXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "doors.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btFloorsTXTLoad
        private void btFloorsXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "floors.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btKeynamesTXTLoad
        private void btKeynamesXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Keynames.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btMacrosXTLoad
        private void btMacrosXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Macros.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btMacros2DXTLoad
        private void btMacros2DXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "macros2D.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btMetricsXTLoad
        private void btMetricsXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "metrics.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btMobtypesXTLoad
        private void btMobtypesXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "mobtypes.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btSuppinfoXTLoad_Click
        private void btSuppinfoXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "suppinfo.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btteleprtsXTLoad_Click
        private void btteleprtsTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "teleprts.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btTile256XTLoad_Click
        private void btTile256XTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Tile256.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btTile1024XTLoad_Click
        private void btTile1024XTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Tile1024.txt");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btBodyconvXTLoad_Click
        private void btBodyconvXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Bodyconv.def");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCorpseXTLoad_Click
        private void btCorpseXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "Corpse.def");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
            }

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btArtXTLoad
        private void btArtXTLoad_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBoxPfadWall.Text, "art.def");
            if (File.Exists(path))
            {
                richTextBoxScriptMiscellaneous.Text = File.ReadAllText(path);
                currentFilePathWall = path;  // Updates the path of the last loaded file
                lbFileNameScript2.Text = Path.GetFileName(path);  // Displays the file name in lbFileName2
            }
            else
            {
                MessageBox.Show("The file walls.txt could not be found.");
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
                int count = Regex.Matches(richTextBoxScriptMiscellaneous.Text, searchText).Count;
                lbSearchCount.Text = $"Number of matches: {count}";

                int index = richTextBoxScriptMiscellaneous.Find(searchText, searchStartIndex, RichTextBoxFinds.None);
                if (index != -1)
                {
                    richTextBoxScriptMiscellaneous.Select(index, searchText.Length);
                    richTextBoxScriptMiscellaneous.ScrollToCaret();  // Scrolls to the cursor position
                    richTextBoxScriptMiscellaneous.Focus();  // Sets the focus on the RichTextBox
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
            // Überprüfen, ob eine Datei geladen wurde
            if (!string.IsNullOrEmpty(currentFilePathWall))
            {
                // Speichern des Inhalts von richTextBoxScriptMiscellaneous in die Datei
                File.WriteAllText(currentFilePathWall, richTextBoxScriptMiscellaneous.Text);

                // Erstellen eines neuen SoundPlayers
                SoundPlayer player = new SoundPlayer();

                // Laden der Sounddatei
                player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

                // Abspielen des Sounds
                player.Play();
            }
            else
            {
                MessageBox.Show("No file was loaded.");
            }
        }
        #endregion

        #region btCreateGiftBoxScript
        private void btCreateGiftBoxScript_Click(object sender, EventArgs e)
        {
            string SetclassNameGift = tbGiftBoxClassScriptName.Text.Replace(" ", "_"); // Class Name GiftBox
            string SetGiftBaseContainer = tbGiftBaseContainer.Text; // BaseContainer
            string SetGiftLabelNumber = tbGiftLabelNumber.Text; // LabelNumber 1156382
            string SetGiftFlipable1 = tbGiftFlipable1.Text; // 0x232A
            string SetGiftFlipable2 = tbGiftFlipable2.Text; // 0x232B
            string SetGiftWeight = tbGiftWeight.Text; // 2.0
            string SetGiftBox = tbGiftGiftBox.Text;  // RandomDyedHue
            string SetGiftHue = tbGiftHue.Text; // hue

            string script = $@"
            using System;

            namespace Server.Items
            {{
                [Furniture]
                [Flipable({SetGiftFlipable1}, {SetGiftFlipable2})]
                public class {SetclassNameGift} : {SetGiftBaseContainer}
                {{
                    public override int LabelNumber {{ get {{ return {SetGiftLabelNumber}; }} }} // Holiday Giftbox

                    [Constructable]
                    public {SetclassNameGift}()
                        : this(Utility.{SetGiftBox}())
                    {{
                    }}

                    [Constructable]
                    public {SetclassNameGift}(int hue)
                        : base(Utility.Random({SetGiftFlipable1}, 2))
                    {{
                        this.Weight = {SetGiftWeight};
                        this.Hue = {SetGiftHue};
                    }}

                    public {SetclassNameGift}(Serial serial)
                        : base(serial)
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
                    }}
                }}
            }}";

            richTextBoxScriptMiscellaneous.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateClosedBarrelScript
        private void btCreateClosedBarrelScript_Click(object sender, EventArgs e)
        {
            string SetclassNameClosedBarrel = tbClassnameClosedBarrel.Text.Replace(" ", "_"); // ClosedBarrel
            string SetTrapableContainer = tbTrapableContainer.Text; // TrapableContainer
            string SetClosedBarrelItemsID = tbClosedBarrelItemsID.Text; // 0x0FAE
            string SetDefaultGumpID = tbDefaultGumpID.Text; // 0x3e

            string script = $@"
using System;

namespace Server.Items
{{
    class {SetclassNameClosedBarrel} : {SetTrapableContainer}
    {{ 
        [Constructable]
        public {SetclassNameClosedBarrel}()
            : base({SetClosedBarrelItemsID})
        {{
        }}

        public {SetclassNameClosedBarrel}(Serial serial)
            : base(serial)
        {{
        }}

        public override int DefaultGumpID
        {{
            get
            {{
                return {SetDefaultGumpID};
            }}
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
        }}
    }}
}}";
            richTextBoxScriptMiscellaneous.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        #region btCreateCommodityDeedBoxScript
        private void btCreateCommodityDeedBoxScript_Click(object sender, EventArgs e)
        {
            string SetclassNameCommodityDeedBoxScript = tbClassnameCommodityDeedBox.Text.Replace(" ", "_");
            string SetBaseContainerCommodityDeedBox = tbBaseContainerCommodityDeedBox.Text; // BaseContainer
            string SetFlipableID1 = tbFlipableID1.Text; // 0x9AA
            string SetFlipableID2 = tbFlipableID2.Text; // 0xE7D
            string SetCommodityDeedBoxHue = tbCommodityDeedBoxHue.Text; // 0x47
            string SetCommodityDeedBoxWeight = tbCommodityDeedBoxWeight.Text; // 4.0
            string SetCommodityDeedBoxLabelNumber = tbCommodityDeedBoxLabelNumber.Text; // 1080523
            string SetCommodityDeedBoxGump = tbCommodityDeedBoxGump.Text; // 0x43
            string SetCommodityDeedBoxRewardItem = tbCommodityDeedBoxRewardItem.Text; // 1076217

            string script = $@"
using System;
using Server.Engines.VeteranRewards;

namespace Server.Items
{{ 
    [Furniture]
    [Flipable({SetFlipableID1}, {SetFlipableID2})]
    public class {SetclassNameCommodityDeedBoxScript} : {SetBaseContainerCommodityDeedBox}, IRewardItem
    {{
        private bool m_IsRewardItem;
        [Constructable]
        public {SetclassNameCommodityDeedBoxScript}()
            : base({SetFlipableID1})
        {{
            this.Hue = {SetCommodityDeedBoxHue};
            this.Weight = {SetCommodityDeedBoxWeight};
        }}

        public {SetclassNameCommodityDeedBoxScript}(Serial serial)
            : base(serial)
        {{
        }}

        public override int LabelNumber
        {{
            get
            {{
                return {SetCommodityDeedBoxLabelNumber};
            }}
        }}// Commodity Deed Box
        public override int DefaultGumpID
        {{
            get
            {{
                return {SetCommodityDeedBoxGump};
            }}
        }}
        [CommandProperty(AccessLevel.GameMaster)]
        public bool IsRewardItem
        {{
            get
            {{
                return this.m_IsRewardItem;
            }}
            set
            {{
                this.m_IsRewardItem = value;
                this.InvalidateProperties();
            }}
        }}
        public static {SetclassNameCommodityDeedBoxScript} Find(Item deed)
        {{
            Item parent = deed;

            while (parent != null && !(parent is {SetclassNameCommodityDeedBoxScript}))
                parent = parent.Parent as Item;

            return parent as {SetclassNameCommodityDeedBoxScript};
        }}

        public override void GetProperties(ObjectPropertyList list)
        {{
            base.GetProperties(list);
			
            if (this.m_IsRewardItem)
                list.Add({SetCommodityDeedBoxRewardItem}); // 1st Year Veteran Reward		
        }}

        public override void Serialize(GenericWriter writer)
        {{
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write((bool)this.m_IsRewardItem);
        }}

        public override void Deserialize(GenericReader reader)
        {{
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            this.m_IsRewardItem = reader.ReadBool();
        }}
    }}
}}";
            richTextBoxScriptMiscellaneous.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
        #endregion

        private void btCreateBagOfReagentsScript_Click(object sender, EventArgs e)
        {
            string SetclassNameCreateBagOfReagent = tbCreateBagOfReagentsScript.Text.Replace(" ", "_");
            string SetCreateBagOfReagentBag = tbBagOfReagentsBag.Text; // Bag
            string SetBagOfReagentsBagMuch = tbBagOfReagentsBagMuch.Text; // 50
            string SetBlackPearl = tbBlackPearl.Text; // BlackPearl
            string SetBloodmoss = tbBloodmoss.Text; // Bloodmoss
            string SetGarlic = tbGarlic.Text; // Garlic
            string SetGinseng = tbGinseng.Text; // Ginseng
            string SetMandrakeRoot = tbMandrakeRoot.Text; // MandrakeRoot
            string SetNightshade = tbNightshade.Text; // Nightshade
            string SetSulfurousAsh = tbSulfurousAsh.Text; // SulfurousAsh
            string SetSpidersSilk = tbSpidersSilk.Text; // SpidersSilk

            string script = $@"
using System;

namespace Server.Items
{{
    public class {SetclassNameCreateBagOfReagent} : {SetCreateBagOfReagentBag}
    {{
        [Constructable]
        public {SetclassNameCreateBagOfReagent}()
            : this({SetBagOfReagentsBagMuch})
        {{
        }}

        [Constructable]
        public {SetclassNameCreateBagOfReagent}(int amount)
        {{
            this.DropItem(new {SetBlackPearl}(amount));
            this.DropItem(new {SetBloodmoss}(amount));
            this.DropItem(new {SetGarlic}(amount));
            this.DropItem(new {SetGinseng}(amount));
            this.DropItem(new {SetMandrakeRoot}(amount));
            this.DropItem(new {SetNightshade}(amount));
            this.DropItem(new {SetSulfurousAsh}(amount));
            this.DropItem(new {SetSpidersSilk}(amount));
        }}

        public {SetclassNameCreateBagOfReagent}(Serial serial)
            : base(serial)
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
        }}
    }}
}}";
            richTextBoxScriptMiscellaneous.Text = script;

            // Create a new SoundPlayer
            SoundPlayer player = new SoundPlayer();

            // Load the sound file
            player.SoundLocation = AppDomain.CurrentDomain.BaseDirectory + "\\Sound.wav";

            // Play the sound
            player.Play();
        }
    }
}

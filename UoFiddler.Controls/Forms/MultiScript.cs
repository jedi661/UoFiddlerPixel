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
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class MultiScript : Form
    {
        public MultiScript()
        {
            InitializeComponent();
        }

        #region btCreateScript
        private void btCreateScript_Click(object sender, EventArgs e)
        {
            // Verify that the necessary information has been entered
            if (string.IsNullOrEmpty(tbIndexImage.Text) || string.IsNullOrEmpty(tbNameScript.Text))
            {
                MessageBox.Show("Please enter the necessary information.");
                return;
            }

            // Create a list of Component objects based on the information entered
            List<Component> components = new List<Component>();
            string[] lines = tbIndexImage.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5)
                {
                    MessageBox.Show("Ungültiges Format in tbIndexImage.");
                    return;
                }

                Component component = new Component
                {
                    ItemId = Convert.ToInt32(parts[0], 16), // Convert hexadecimal to decimal
                    X = int.Parse(parts[1]),
                    Y = int.Parse(parts[2]),
                    Z = int.Parse(parts[3]),
                    Hue = int.Parse(parts[4])
                };
                components.Add(component);
            }

            // Create the script based on the information entered
            string script = GenerateScript(tbNameScript.Text, components);

            // Display the created script in the tbscriptFinish text box
            tbscriptFinish.Text = script;
        }
        #endregion

        #region GenerateScript
        public string GenerateScript(string className, List<Component> components)
        {
            StringBuilder sb = new StringBuilder();

            // Beginn des Skripts
            sb.AppendLine("using System;");
            sb.AppendLine("using Server;");
            sb.AppendLine("using Server.Items;");
            sb.AppendLine("namespace Server.Items");
            sb.AppendLine("{");
            sb.AppendLine($"public class {className} : BaseAddon");
            sb.AppendLine("{");

            // Komponenten hinzufügen
            sb.AppendLine("private static int[,] m_AddOnSimpleComponents = new int[,]");
            sb.AppendLine("{");
            foreach (var component in components)
            {
                sb.AppendLine($"{{0x{component.ItemId.ToString("X")}, {component.X}, {component.Y}, {component.Z}, {component.Hue}}},");
            }
            sb.AppendLine("};");

            // Rest des Skripts
            sb.AppendLine("public override BaseAddonDeed Deed { get { return new " + className + "Deed(); } }");
            sb.AppendLine("[Constructable]");
            sb.AppendLine("public " + className + "()");
            sb.AppendLine("{");
            sb.AppendLine("for (int i = 0; i < m_AddOnSimpleComponents.Length / 5; i++)");
            sb.AppendLine("AddComponent(new AddonComponent(m_AddOnSimpleComponents[i, 0]), m_AddOnSimpleComponents[i, 1], m_AddOnSimpleComponents[i, 2], m_AddOnSimpleComponents[i, 3]);");
            sb.AppendLine("}");
            sb.AppendLine("public " + className + "( Serial serial ) : base( serial ) { }");
            sb.AppendLine("public override void Serialize( GenericWriter writer )");
            sb.AppendLine("{");
            sb.AppendLine("base.Serialize( writer );");
            sb.AppendLine("writer.Write( 0 ); // Version");
            sb.AppendLine("}");
            sb.AppendLine("public override void Deserialize( GenericReader reader )");
            sb.AppendLine("{");
            sb.AppendLine("base.Deserialize( reader );");
            sb.AppendLine("int version = reader.ReadInt();");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("public class " + className + "Deed : BaseAddonDeed");
            sb.AppendLine("{");
            sb.AppendLine("public override BaseAddon Addon { get { return new " + className + "(); } }");
            sb.AppendLine("[Constructable]");
            sb.AppendLine("public " + className + "Deed()");
            sb.AppendLine("{");
            sb.AppendLine("Name = \"" + className + "\";");
            sb.AppendLine("}");
            sb.AppendLine("public " + className + "Deed( Serial serial ) : base( serial ) { }");
            sb.AppendLine("public override void Serialize( GenericWriter writer )");
            sb.AppendLine("{");
            sb.AppendLine("base.Serialize( writer );");
            sb.AppendLine("writer.Write( 0 ); // Version");
            sb.AppendLine("}");
            sb.AppendLine("public override void Deserialize( GenericReader reader )");
            sb.AppendLine("{");
            sb.AppendLine("base.Deserialize( reader );");
            sb.AppendLine("int version = reader.ReadInt();");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");

            return sb.ToString();
        }
        #endregion

        #region Component
        public class Component
        {
            public int ItemId { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
            public int Hue { get; set; }
        }
        #endregion

        #region tbIndexImage_TextChanged
        private bool updatingText = false;
                
        private void tbIndexImage_TextChanged(object sender, EventArgs e)
        {
            if (updatingText) return;

            updatingText = true;

            // Divide the text in the text box into lines
            string[] lines = tbIndexImage.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                // Divide each line into words
                string[] words = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Check if the first word is a hex address
                if (words.Length > 0 && words[0].StartsWith("0x"))
                {
                    // Insert a line break before the hex address, except for the first line
                    if (i != 0)
                    {
                        lines[i] = "\r\n" + lines[i];
                    }
                }
            }

            // Put the text in the text box on the formatted lines
            tbIndexImage.Text = string.Join(" ", lines);

            updatingText = false;
        }
        #endregion

        #region btnSaveScript
        private void btnSaveScript_Click(object sender, EventArgs e)
        {
            // Überprüfen, ob ein Skriptname angegeben wurde
            if (string.IsNullOrEmpty(tbNameScript.Text))
            {
                MessageBox.Show("Please enter a name for the script.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "C# Dateien (*.cs)|*.cs";
            saveFileDialog.FileName = tbNameScript.Text;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Speichern Sie das Skript in der ausgewählten .cs-Datei
                System.IO.File.WriteAllText(saveFileDialog.FileName, tbscriptFinish.Text);

                MessageBox.Show("The script was saved successfully!");
            }
        }
        #endregion

        #region IndexImageText
        public string IndexImageText
        {
            get { return tbIndexImage.Text; }
            set { tbIndexImage.Text = value; }
        }
        #endregion
    }
}

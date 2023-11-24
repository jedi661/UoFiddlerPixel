using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Terrain;

using Microsoft.VisualBasic.CompilerServices;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Helper;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class GroupSelect : Form
    {
        private PropertyGrid _PropertyGrid1;
        private ClsTerrainTable iTerrain;
        internal object SelectGroupName;

        public GroupSelect()
        {
            InitializeComponent();

            this.Load += new EventHandler(this.GroupSelect_Load);
            this.iTerrain = new ClsTerrainTable();
        }
        private void GroupSelect_Load(object sender, EventArgs e)
        {
            this.iTerrain.Load();
            this.iTerrain.Display(this.SelectGroup);
        }

        private void SelectGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PropertyGrid1.SelectedObject = RuntimeHelpers.GetObjectValue(this.SelectGroup.SelectedItem);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // Erstellen Sie eine neue Instanz von GroupSelect
            GroupSelect groupSelect = new GroupSelect();

            // Konvertieren Sie groupSelect.SelectGroupName sicher in ein Label
            Label selectGroupNameLabel = groupSelect.SelectGroupName as Label;

            // Überprüfen Sie, ob die Konvertierung erfolgreich war
            if (selectGroupNameLabel != null)
            {
                // Setzen Sie den Text des Labels
                string text = selectGroupNameLabel.Text;

                // Erstellen Sie eine neue Instanz von CreateTransitions und setzen Sie das Tag der groupSelect
                CreateTransitions tedit = (CreateTransitions)this.Tag;

                // Überprüfen Sie den Text des Labels und führen Sie entsprechende Aktionen durch
                if (StringType.StrCmp(text, "Select Group A", false) == 0)
                {
                    tedit.Selected_Terrain_A = (ClsTerrain)this.SelectGroup.SelectedItem;
                    tedit.MenuTerrainA.Text = string.Format("Select Terrain A - {0}", RuntimeHelpers.GetObjectValue(LateBinding.LateGet(this.SelectGroup.SelectedItem, (Type)null, "Name", new object[0], (string[])null, (bool[])null)));
                }
                else if (StringType.StrCmp(text, "Select Group B", false) == 0)
                {
                    tedit.Selected_Terrain_B = (ClsTerrain)this.SelectGroup.SelectedItem;
                    tedit.MenuTerrainB.Text = string.Format("Select Terrain B - {0}", RuntimeHelpers.GetObjectValue(LateBinding.LateGet(this.SelectGroup.SelectedItem, (Type)null, "Name", new object[0], (string[])null, (bool[])null)));
                }
                else if (StringType.StrCmp(text, "Select Group C", false) == 0)
                {
                    tedit.Selected_Terrain_C = (ClsTerrain)this.SelectGroup.SelectedItem;
                    tedit.MenuTerrainC.Text = string.Format("Select Terrain C - {0}", RuntimeHelpers.GetObjectValue(LateBinding.LateGet(this.SelectGroup.SelectedItem, (Type)null, "Name", new object[0], (string[])null, (bool[])null)));
                }
                else if (StringType.StrCmp(text, "Clone Group A", false) == 0)
                {
                    tedit.Selected_Terrain_A = (ClsTerrain)this.SelectGroup.SelectedItem;
                    tedit.Menu_CloneGroupA.Text = string.Format("Select Terrain A - {0}", RuntimeHelpers.GetObjectValue(LateBinding.LateGet(this.SelectGroup.SelectedItem, (Type)null, "Name", new object[0], (string[])null, (bool[])null)));
                }
                else if (StringType.StrCmp(text, "Clone Group B", false) == 0)
                {
                    tedit.Selected_Terrain_B = (ClsTerrain)this.SelectGroup.SelectedItem;
                    tedit.Menu_CloneGroupB.Text = string.Format("Select Terrain B - {0}", RuntimeHelpers.GetObjectValue(LateBinding.LateGet(this.SelectGroup.SelectedItem, (Type)null, "Name", new object[0], (string[])null, (bool[])null)));
                }
            }

            // Zeigen Sie das groupSelect Formular an
            groupSelect.Show();

            // Schließen Sie das aktuelle Formular
            this.Close();
        }

    }
}

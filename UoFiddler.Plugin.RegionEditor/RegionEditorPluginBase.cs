/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Plugin "Region editor": regions of ServUO/RunUO, Sphere and POL on the maps.
 *
 ***************************************************************************/

using System.Windows.Forms;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.RegionEditor.Classes;
using UoFiddler.Plugin.RegionEditor.Forms;
using UoFiddler.Plugin.RegionEditor.UserControls;

namespace UoFiddler.Plugin.RegionEditor
{
    public class RegionEditorPluginBase : PluginBase
    {
        public override string Name { get; } = "Region Editor";

        public override string Description => Loc.T("plugin.description");

        public override string Author { get; } = "UoFiddlerPixel";

        public override string Version { get; } = "1.0.0";

        public override IPluginHost Host { get; set; }

        public override void Initialize()
        {
        }

        public override void Unload()
        {
        }

        public override void ModifyTabPages(TabControl tabControl)
        {
            var page = new TabPage
            {
                Tag = tabControl.TabCount + 1, // order for undock/dock
                Text = Loc.T("tab.title")
            };
            page.Controls.Add(new RegionEditorControl { Dock = DockStyle.Fill });
            tabControl.TabPages.Add(page);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
            var item = new ToolStripMenuItem { Text = $"{Loc.T("tab.title")} – {Loc.T("btn.help").TrimStart('?', ' ')}" };
            item.Click += OnClickHelp;
            toolStrip.DropDownItems.Add(item);
        }

        private static void OnClickHelp(object sender, System.EventArgs e) => RegionHelpForm.ShowHelp(null);
    }
}

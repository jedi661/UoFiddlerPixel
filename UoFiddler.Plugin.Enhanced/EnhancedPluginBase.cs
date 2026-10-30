/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Windows.Forms;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.Enhanced.UserControls;

namespace UoFiddler.Plugin.Enhanced
{
    /// <summary>
    /// Plugin for the files of the Ultima Online Enhanced Client (UOP packages: Texture.uop, tileart.uop, string_dictionary.uop, ...).
    /// </summary>
    public class EnhancedPluginBase : PluginBase
    {
        public override string Name { get; } = "Enhanced Client";

        public override string Description { get; } = "View and edit the files of the Ultima Online Enhanced Client:\r\n" +
                                                      "art, tiledata, gumps, hues, animations, animdata, multis, maps, sounds, cliloc, interface and every UOP file.";

        public override string Author { get; } = "Nikodemus (UoFiddlerPixel)";

        public override string Version { get; } = "0.9.0";

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
                Tag = tabControl.TabCount + 1, // used by the undock/dock feature to keep the order
                Text = "Enhanced Client"
            };
            page.Controls.Add(new EnhancedClientControl());
            tabControl.TabPages.Add(page);
        }
    }
}

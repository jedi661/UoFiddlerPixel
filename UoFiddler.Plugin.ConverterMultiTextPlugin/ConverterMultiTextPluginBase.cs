﻿/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.UserControls;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin
{
    public class ConverterMultiTextPluginBase : PluginBase
    {
        private const string _itemDescFileName = "itemdesc.cfg";

        public ConverterMultiTextPluginBase()
        {
            PluginEvents.ModifyItemsControlContextMenuEvent += EventsModifyItemsControlContextMenuEvent;
        }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "Tool Box";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } = "This is Tool Box plugin and AdminTool, Imagecutter and Texturecutter, Converter, Alarm Clock, and more.";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Nikodemus";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.0.5";

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get; set; }

        public override void Initialize()
        {
            // make something useful
            _ = Files.RootDir;
        }

        public override void Unload()
        {
        }

        public override void ModifyTabPages(TabControl tabControl)
        {
            TabPage page = new TabPage
            {
                Tag = tabControl.TabCount + 1, // at end used for undock/dock feature to define the order
                Text = "Tool Box"
            };
            page.Controls.Add(new ConverterMultiTextControl());
            tabControl.TabPages.Add(page);
        }

        #region ModifyPluginToolStrip ServerStartBox
        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "AdminTool" //ConverterMultiText
            };
            item.Click += ItemClick;
            toolStrip.DropDownItems.Add(item);

            // Add a menu item for ServerStartBox
            ToolStripMenuItem serverStartBoxItem = new ToolStripMenuItem
            {
                Text = "ServerStartBox"
            };
            serverStartBoxItem.Click += ShowServerStartBox;
            toolStrip.DropDownItems.Add(serverStartBoxItem);
        }
        #endregion

        #region ItemClick Admintool
        private static void ItemClick(object sender, EventArgs e)
        {
            //new AdminToolForm().Show();
            AdminToolForm form = AdminToolForm.GetInstance();
            form.Show();
        }
        #endregion

        #region ShowServerStartBox
        private static void ShowServerStartBox(object sender, EventArgs e)
        {
            // Create a new instance of ServerStartBox and display it
            new ServerStartBox().Show();
        }
        #endregion

        private void EventsModifyItemsControlContextMenuEvent(ContextMenuStrip strip)
        {
            strip.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exportItemDescItem = new ToolStripMenuItem
            {
                Text = "Export selected to itemdesc.cfg"
            };
            exportItemDescItem.Click += ExportToItemDescClicked;
            strip.Items.Add(exportItemDescItem);

            strip.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exportOffsetItem = new ToolStripMenuItem
            {
                Text = "Export all items to offset.cfg"
            };
            exportOffsetItem.Click += ExportToOffsetClicked;
            strip.Items.Add(exportOffsetItem);
        }

        private void ExportToOffsetClicked(object sender, EventArgs e)
        {
            List<int> itemIds = new List<int>();
            itemIds.AddRange(Host.GetItemsControl().ItemList);

            string fileName = Path.Combine(Options.OutputPath, "offset.cfg");

            string inputMessage = "Do you want to export all items to offset.cfg?\r\n"
                                  + "It may take some time (around 10-20 seconds).\r\n\r\n"
                                  + "Export will replace existing file located at: "
                                  + fileName
                                  + "\r\n\r\nContinue?\r\n";

            if (MessageBox.Show(inputMessage, "Export all items to offset.cfg?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (int itemId in itemIds)
            {
                if (itemId <= -1 || !Art.IsValidStatic(itemId))
                {
                    continue;
                }

                Art.Measure(Art.GetStatic(itemId), out int xMin, out int yMin, out int xMax, out int yMax);

                sb.AppendFormat("Item 0x{0:X4}", itemId).AppendLine();
                sb.AppendLine("{");
                sb.AppendFormat("   xMin    {0}", xMin).AppendLine();
                sb.AppendFormat("   yMin    {0}", yMin).AppendLine();
                sb.AppendFormat("   xMax    {0}", xMax).AppendLine();
                sb.AppendFormat("   yMax    {0}", yMax).AppendLine();
                sb.AppendLine("}").AppendLine();
            }

            File.WriteAllText(fileName, sb.ToString());

            MessageBox.Show("Done!");
        }

        private void ExportToItemDescClicked(object sender, EventArgs e)
        {
            var selectedArtIds = new List<int>();
            var itemsControl = Host.GetItemsControl();
            var itemsControlTileView = Host.GetItemsControlTileView();

            foreach (var item in itemsControlTileView.SelectedIndices)
            {
                var graphic = itemsControl.ItemList[item];
                if (Art.IsValidStatic(graphic))
                {
                    selectedArtIds.Add(graphic);
                }
            }

            ExportAllItems(selectedArtIds);
        }

        private static void ExportAllItems(ICollection<int> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, _itemDescFileName);

            using (StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write), Encoding.GetEncoding(1252)))
            {
                foreach (var item in items)
                {
                    streamWriter.WriteLine(GetItemDescEntry(item));
                }
            }
        }

        private static string GetItemDescEntry(int itemId)
        {
            ItemData itemData = TileData.ItemTable[itemId];

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Item 0x{0:X4}", itemId).AppendLine();
            sb.AppendLine("{");
            sb.AppendFormat("   Name    {0}", itemData.Name).AppendLine();
            sb.AppendFormat("   Graphic 0x{0:X4}", itemId).AppendLine();
            sb.AppendFormat("   Weight  {0}", itemData.Weight).AppendLine();
            sb.AppendLine("}").AppendLine();

            return sb.ToString();
        }
    }
}

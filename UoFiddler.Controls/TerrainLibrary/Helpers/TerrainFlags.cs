// /***************************************************************************
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
using System.Linq;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>
    /// Shows tiledata flags (Ultima.TileFlag) as a CheckedListBox. Unused/unknown flags are left out.
    /// </summary>
    public static class TerrainFlags
    {
        private sealed class FlagItem
        {
            public FlagItem(TileFlag flag)
            {
                Flag = flag;
            }

            public TileFlag Flag { get; }

            public override string ToString() => Flag.ToString();
        }

        public static void Fill(CheckedListBox list)
        {
            list.Items.Clear();
            foreach (TileFlag flag in Enum.GetValues(typeof(TileFlag)).Cast<TileFlag>())
            {
                string name = flag.ToString();
                if (flag == TileFlag.None || name.StartsWith("Unused", StringComparison.Ordinal) || name.StartsWith("Unknown", StringComparison.Ordinal))
                {
                    continue;
                }

                list.Items.Add(new FlagItem(flag));
            }
        }

        public static ulong Get(CheckedListBox list)
        {
            ulong value = 0;
            foreach (object item in list.CheckedItems)
            {
                value |= (ulong)((FlagItem)item).Flag;
            }

            return value;
        }

        public static void Set(CheckedListBox list, ulong value)
        {
            for (int i = 0; i < list.Items.Count; ++i)
            {
                ulong flag = (ulong)((FlagItem)list.Items[i]).Flag;
                list.SetItemChecked(i, (value & flag) != 0);
            }
        }
    }
}

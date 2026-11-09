using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class ListBoxHelper
    {
        private const int _lbGetCaretIndex = 0x019F;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Index of the selected item that has the focus (the one last clicked) in a multi select ListBox,
        /// -1 when nothing is selected. ListBox.SelectedIndex only returns "any" selected item.
        /// </summary>
        public static int GetFocusedSelectedIndex(ListBox listBox)
        {
            if (listBox.SelectedIndices.Count == 0)
            {
                return -1;
            }

            if (listBox.IsHandleCreated)
            {
                int caret = (int)SendMessage(listBox.Handle, _lbGetCaretIndex, IntPtr.Zero, IntPtr.Zero);
                if (caret >= 0 && caret < listBox.Items.Count && listBox.GetSelected(caret))
                {
                    return caret;
                }
            }

            int index = listBox.SelectedIndex;
            return index >= 0 && index < listBox.Items.Count ? index : listBox.SelectedIndices[0];
        }
    }
}

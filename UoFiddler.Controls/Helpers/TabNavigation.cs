using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    /// <summary>Brings the tab page (or undocked window) that contains a control to the front.</summary>
    public static class TabNavigation
    {
        public static void BringToFront(Control control)
        {
            if (control == null)
            {
                return;
            }

            Control child = control;
            for (Control parent = control.Parent; parent != null; child = parent, parent = parent.Parent)
            {
                if (child is TabPage page && parent is TabControl tabs && tabs.SelectedTab != page)
                {
                    tabs.SelectedTab = page;
                }
            }

            Form form = control.FindForm();
            if (form != null && !form.Focused && form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            form?.Activate();
        }
    }
}

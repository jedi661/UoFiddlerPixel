/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Light check: duplicate, empty, very dark, very big lights and lights with
 * inverted ("negative") values; summary with used / free entries.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class LightCheckForm : Form
    {
        private readonly Action<int> _show;

        public LightCheckForm()
            : this(null)
        {
        }

        /// <param name="show">selects a light in the light tab</param>
        public LightCheckForm(Action<int> show)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _show = show;
            Icon = Options.GetFiddlerIcon();
            Run();
        }

        private void Run()
        {
            var lights = new List<(int, LightImage)>();
            for (int i = 0; i < Light.Count; i++)
            {
                if (!Light.TestLight(i))
                {
                    continue;
                }

                byte[] data = Light.GetRawLight(i, out int w, out int h);
                if (data != null)
                {
                    lights.Add((i, new LightImage(w, h, data)));
                }
            }

            List<LightProblem> problems = LightCheck.Run(lights);
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (LightProblem p in problems)
            {
                _list.Items.Add(new ListViewItem(new[] { LightNames.Display(p.Id), p.Kind, p.Text }) { Tag = p.Id });
            }

            _list.EndUpdate();

            int free = Light.Count - lights.Count;
            int firstFree = Enumerable.Range(0, Light.Count).FirstOrDefault(i => !Light.TestLight(i));
            _lblSummary.Text = $"{lights.Count} lights in {Light.Count} entries, {free} free" +
                               (free > 0 ? $" (first free: {firstFree} / 0x{firstFree:X2})" : string.Empty) +
                               $"\n{(problems.Count == 0 ? "✔ no problems found" : $"{problems.Count} note(s) – double click shows the light")}";
        }

        private void OnClickShow(object sender, EventArgs e)
        {
            if (_list.SelectedItems.Count > 0 && _list.SelectedItems[0].Tag is int id)
            {
                _show?.Invoke(id);
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            var sb = new StringBuilder(_lblSummary.Text.Replace("\n", Environment.NewLine)).AppendLine();
            foreach (ListViewItem item in _list.Items)
            {
                sb.AppendLine(string.Join("\t", item.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(s => s.Text)));
            }

            Clipboard.SetText(sb.ToString());
        }
    }
}

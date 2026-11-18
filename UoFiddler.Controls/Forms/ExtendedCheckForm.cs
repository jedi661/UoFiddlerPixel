/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Shows what is really saved: extended items in memory compared with the files on disk
    /// (loaded folder, Ultima Online folder, output folder). Double-click an id to jump to it in the Items tab.
    /// </summary>
    public sealed class ExtendedCheckForm : Form
    {
        private readonly Action<int> _goTo;
        private readonly TextBox _report;
        private readonly ListView _list;

        public ExtendedCheckForm(Action<int> goTo)
        {
            _goTo = goTo;

            Text = "Extended format - check what is saved";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1150, 680);
            MinimumSize = new Size(800, 450);

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 680 };
            Controls.Add(split);

            _report = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Dock = DockStyle.Fill,
                Font = new Font(FontFamily.GenericMonospace, 9f)
            };
            split.Panel1.Controls.Add(_report);

            _list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            _list.DoubleClick += (s, e) =>
            {
                if (_list.SelectedItems.Count > 0 && _list.SelectedItems[0].Tag is int id)
                {
                    _goTo?.Invoke(id);
                }
            };
            split.Panel2.Controls.Add(_list);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            var refresh = new Button { Text = "Check again", AutoSize = true };
            refresh.Click += (s, e) => RunCheck();
            var copy = new Button { Text = "Copy report", AutoSize = true };
            copy.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(_report.Text))
                {
                    Clipboard.SetText(_report.Text);
                }
            };
            buttons.Controls.Add(refresh);
            buttons.Controls.Add(copy);
            buttons.Controls.Add(new Label
            {
                AutoSize = true,
                Margin = new Padding(10, 8, 3, 3),
                ForeColor = Color.DimGray,
                Text = "Double-click an id in the list to show it in the Items tab."
            });
            Controls.Add(buttons);

            Shown += (s, e) => RunCheck();
        }

        private void RunCheck()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string text = ExtendedCheck.BuildReport(out List<ExtendedCheck.Row> rows);
                _report.Text = text.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

                _list.BeginUpdate();
                _list.Items.Clear();
                _list.Columns.Clear();
                _list.Columns.Add("Item id", 90);
                _list.Columns.Add("Memory", 80);
                List<string> folders = rows.SelectMany(r => r.OnDisk.Keys).Distinct().ToList();
                foreach (string f in folders)
                {
                    _list.Columns.Add(f, 120);
                }

                foreach (ExtendedCheck.Row row in rows)
                {
                    var item = new ListViewItem($"0x{row.Id:X}") { Tag = row.Id };
                    item.SubItems.Add(row.InMemory ? (row.Unsaved ? "yes (unsaved)" : "yes") : "-");
                    bool problem = false;
                    foreach (string f in folders)
                    {
                        bool on = row.OnDisk.TryGetValue(f, out bool v) && v;
                        item.SubItems.Add(on ? "saved" : "-");
                        problem |= row.InMemory && !on && f.Contains("loaded");
                    }

                    if (row.Unsaved || problem)
                    {
                        item.ForeColor = Color.DarkOrange;
                    }

                    _list.Items.Add(item);
                }

                _list.EndUpdate();
            }
            catch (Exception ex)
            {
                _report.Text = $"Check failed: {ex}";
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }
}

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
        private readonly Action _reloadItems;
        private readonly TextBox _report;
        private readonly ListView _list;

        public ExtendedCheckForm(Action<int> goTo, Action reloadItems = null)
        {
            _goTo = goTo;
            _reloadItems = reloadItems;

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
            var loadOutput = new Button { Text = "Load from output folder", AutoSize = true };
            loadOutput.Click += (s, e) => LoadFromOutput();
            var copyToUo = new Button { Text = "Copy output -> Ultima Online folder (with backup)", AutoSize = true };
            copyToUo.Click += (s, e) => CopyOutputToClient();

            buttons.Controls.Add(refresh);
            buttons.Controls.Add(copy);
            buttons.Controls.Add(loadOutput);
            buttons.Controls.Add(copyToUo);
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

        private bool ConfirmDiscardUnsaved()
        {
            int unsaved = Ultima.Art.HasExtension ? Ultima.Art.GetUnsavedExtendedIds().Count : 0;
            return unsaved == 0 || MessageBox.Show(this,
                $"{unsaved} extended item(s) are not saved yet and will be discarded. Continue?",
                "Extended files", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void LoadFromOutput()
        {
            if (!ConfirmDiscardUnsaved())
            {
                return;
            }

            List<string> found = ExtendedCheck.LoadFrom(Options.OutputPath);
            if (found.Count == 0)
            {
                MessageBox.Show(this, $"No extended files in the output folder{Environment.NewLine}{Options.OutputPath}", "Load",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _reloadItems?.Invoke();
            RunCheck();
            MessageBox.Show(this,
                $"Loaded from the output folder:{Environment.NewLine}{Options.OutputPath}{Environment.NewLine}{Environment.NewLine}" +
                "UOFiddler now loads AND saves the extension there (the path is remembered)." + Environment.NewLine +
                "Copy the files into the Ultima Online folder when you want them there.",
                "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyOutputToClient()
        {
            if (MessageBox.Show(this,
                    $"Copy {string.Join(", ", ExtendedCheck.FileNames)}{Environment.NewLine}from  {Options.OutputPath}{Environment.NewLine}" +
                    $"to    {Ultima.Files.RootDir}{Environment.NewLine}{Environment.NewLine}" +
                    "The files there are backed up first (output folder\\UO_Backup\\<date>). art.mul / tiledata.mul are NOT copied. Continue?",
                    "Copy to Ultima Online folder", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            if (!ConfirmDiscardUnsaved())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string backup = ExtendedCheck.CopyToClientFolder(Options.OutputPath, out List<string> copied);
                _reloadItems?.Invoke();
                RunCheck();
                MessageBox.Show(this,
                    $"Copied: {string.Join(", ", copied)}{Environment.NewLine}Backup: {backup}{Environment.NewLine}{Environment.NewLine}" +
                    "The extension is now loaded from the Ultima Online folder.",
                    "Copy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Copy failed: {ex.Message}", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
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

/***************************************************************************
 *
 *  * Dragon Map Studio – learn rules / scripts from a CGI area or a stamp:
 *  * what to learn (land, statics, both), the learned files as a list to tick,
 *  * the content of the selected file, then "Übernehmen"
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class LearnRulesDialog : Form
    {
        private readonly Func<LearnOptions, List<LearnedItem>> _learn;
        private readonly DragonRuleSet _rules;
        private readonly ComboBox _preset;
        private readonly CheckBox _map;
        private readonly CheckBox _items;
        private readonly CheckBox _statics;
        private readonly NumericUpDown _min;
        private readonly TextBox _name;
        private readonly CheckedListBox _list;
        private readonly TextBox _details;
        private List<LearnedItem> _result = new List<LearnedItem>();

        public LearnRulesDialog(string source, string name, DragonRuleSet rules, Func<LearnOptions, List<LearnedItem>> learn)
        {
            _learn = learn;
            _rules = rules;
            Text = L.T("Regeln und Skripte lernen – ") + source;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 680);
            Font = new Font("Segoe UI", 9f);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 74, Padding = new Padding(6), WrapContents = true };
            top.Controls.Add(new Label { Text = L.T("Lernen:"), AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
            _preset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
            _preset.Items.AddRange(new object[] { L.T("Beides (Land + Statics)"), L.T("Nur Land (Map-Übergänge)"), L.T("Nur Statics (Items + Static-Übergänge)"), L.T("Eigene Auswahl") });
            top.Controls.Add(_preset);
            _map = new CheckBox { Text = L.T("Map-Übergänge"), Checked = true, AutoSize = true, Margin = new Padding(12, 6, 3, 3) };
            _items = new CheckBox { Text = L.T("Items (Statics auf Flächen)"), Checked = true, AutoSize = true, Margin = new Padding(6, 6, 3, 3) };
            _statics = new CheckBox { Text = L.T("Static-Übergänge (Statics an Rändern)"), Checked = true, AutoSize = true, Margin = new Padding(6, 6, 3, 3) };
            top.Controls.AddRange(new Control[] { _map, _items, _statics });
            top.SetFlowBreak(_statics, true);
            top.Controls.Add(new Label { Text = L.T("Mindestens Beispiele je Muster:"), AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
            _min = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 2, Width = 60 };
            top.Controls.Add(_min);
            top.Controls.Add(new Label { Text = L.T("Name der Dateien:"), AutoSize = true, Margin = new Padding(12, 8, 3, 3) });
            _name = new TextBox { Width = 180, Text = name };
            top.Controls.Add(_name);
            var run = new Button { Text = L.T("Lernen"), AutoSize = true, BackColor = Color.FromArgb(22, 128, 61), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            run.Click += (s, e) => Run();
            top.Controls.Add(run);
            _preset.SelectedIndexChanged += (s, e) =>
            {
                if (_preset.SelectedIndex == 0) { _map.Checked = _items.Checked = _statics.Checked = true; }
                if (_preset.SelectedIndex == 1) { _map.Checked = true; _items.Checked = _statics.Checked = false; }
                if (_preset.SelectedIndex == 2) { _map.Checked = false; _items.Checked = _statics.Checked = true; }
            };
            _preset.SelectedIndex = 0;

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 260 };
            _list = new CheckedListBox { Dock = DockStyle.Fill, CheckOnClick = true, IntegralHeight = false, HorizontalScrollbar = true };
            _list.SelectedIndexChanged += (s, e) => ShowDetails();
            _details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, ReadOnly = true, Font = new Font("Consolas", 9f) };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(_details);

            var info = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(8, 2, 8, 0),
                ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Map-Übergang: welches Tile an jedem Gruppen-Rand liegt (8 Nachbarn A/B) → map/lernen_*.txt + Regel. Items: Statics auf Flächen mit Häufigkeit und Dichte → items/lernen_*.txt + items.txt. " +
                           "Static-Übergang: Statics an Rändern → statics/lernen_*.txt + Regel. Vorhandene Regeln behalten Vorrang, gelernte füllen nur fehlende Muster.")
            };
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 42, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            var ok = new Button { Text = L.T("Angehakte übernehmen"), DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = L.T("Abbrechen"), DialogResult = DialogResult.Cancel, AutoSize = true };
            var none = new Button { Text = L.T("Keine"), AutoSize = true };
            none.Click += (s, e) => SetAll(false);
            var all = new Button { Text = L.T("Alle"), AutoSize = true };
            all.Click += (s, e) => SetAll(true);
            bottom.Controls.AddRange(new Control[] { ok, cancel, none, all });
            Controls.Add(split);
            Controls.Add(info);
            Controls.Add(bottom);
            Controls.Add(top);
            AcceptButton = ok;
            CancelButton = cancel;
            Shown += (s, e) => Run();
        }

        public List<LearnedItem> Result => _result;

        private void Run()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _result = _learn(new LearnOptions
                {
                    MapTransitions = _map.Checked,
                    Items = _items.Checked,
                    StaticTransitions = _statics.Checked,
                    MinCount = (int)_min.Value,
                    Name = _name.Text.Trim()
                }) ?? new List<LearnedItem>();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (LearnedItem it in _result)
            {
                _list.Items.Add(it, it.Accept);
            }
            _list.EndUpdate();
            _details.Text = _result.Count == 0 ? L.T("Nichts gelernt – Bereich größer wählen, 'Mindestens Beispiele' kleiner oder andere Auswahl.") : string.Empty;
            if (_result.Count > 0)
            {
                _list.SelectedIndex = 0;
            }
        }

        private void ShowDetails()
        {
            if (_list.SelectedItem is LearnedItem it)
            {
                _details.Text = RuleLearner.Report(new[] { it }, _rules).Replace("\n", "\r\n");
            }
        }

        private void SetAll(bool on)
        {
            for (int i = 0; i < _list.Items.Count; i++)
            {
                _list.SetItemChecked(i, on);
            }
        }

        /// <summary>Writes the ticks into the items.</summary>
        public void ApplyChecks()
        {
            for (int i = 0; i < _list.Items.Count && i < _result.Count; i++)
            {
                _result[i].Accept = _list.GetItemChecked(i);
            }
        }
    }
}

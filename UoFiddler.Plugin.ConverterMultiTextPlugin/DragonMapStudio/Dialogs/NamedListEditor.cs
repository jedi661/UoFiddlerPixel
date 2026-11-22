/***************************************************************************
 *
 *  * Dragon Map Studio – small editor for lists of named settings (variant sets, roads, course kinds, wall sets)
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class NamedListEditor : Form
    {
        private readonly IList _items;
        private readonly Func<object> _create;
        private readonly Func<object, object> _copy;
        private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        private readonly PropertyGrid _grid = new PropertyGrid { Dock = DockStyle.Fill, ToolbarVisible = false, HelpVisible = true, PropertySort = PropertySort.Categorized };
        private readonly FlowLayoutPanel _extra = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(4) };
        private readonly Label _note = new Label { Dock = DockStyle.Bottom, Height = 40, Padding = new Padding(4), ForeColor = Color.FromArgb(71, 85, 105) };

        public NamedListEditor(string title, IList items, Func<object> create, Func<object, object> copy, string note = null)
        {
            _items = items;
            _create = create;
            _copy = copy;
            Text = title;
            Size = new Size(720, 520);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            Font = new Font("Segoe UI", 9f);
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 220 };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(2) };
            Button B(string text, Action a)
            {
                var b = new Button { Text = L.T(text), AutoSize = true, FlatStyle = FlatStyle.System };
                b.Click += (s, e) => a();
                buttons.Controls.Add(b);
                return b;
            }
            B("Neu", () => Add(_create()));
            B("Kopie", () =>
            {
                if (_list.SelectedItem != null)
                {
                    Add(_copy(_list.SelectedItem));
                }
            });
            B("Löschen", () =>
            {
                if (_list.SelectedItem != null)
                {
                    _items.Remove(_list.SelectedItem);
                    Fill(null);
                }
            });
            split.Panel1.Controls.Add(_list);
            split.Panel1.Controls.Add(buttons);
            split.Panel2.Controls.Add(_grid);
            split.Panel2.Controls.Add(_extra);
            _note.Text = note ?? string.Empty;
            _note.Visible = !string.IsNullOrEmpty(note);
            Controls.Add(split);
            Controls.Add(_note);
            var close = new Button { Text = L.T("Fertig"), DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 30 };
            Controls.Add(close);
            AcceptButton = close;
            _list.SelectedIndexChanged += (s, e) => _grid.SelectedObject = _list.SelectedItem;
            _grid.PropertyValueChanged += (s, e) =>
            {
                object sel = _list.SelectedItem;
                Fill(sel);
            };
            Fill(null);
        }

        public object Selected => _list.SelectedItem;

        /// <summary>Extra button under the settings: works on the selected entry.</summary>
        public void AddAction(string text, Action<object> action)
        {
            var b = new Button { Text = L.T(text), AutoSize = true, FlatStyle = FlatStyle.System };
            b.Click += (s, e) =>
            {
                if (_list.SelectedItem != null)
                {
                    action(_list.SelectedItem);
                    _grid.Refresh();
                    Fill(_list.SelectedItem);
                }
            };
            _extra.Controls.Add(b);
        }

        public void Select(Func<object, bool> which)
        {
            foreach (object o in _items)
            {
                if (which(o))
                {
                    _list.SelectedItem = o;
                    return;
                }
            }
        }

        private void Add(object o)
        {
            if (o == null)
            {
                return;
            }
            _items.Add(o);
            Fill(o);
        }

        private void Fill(object select)
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (object o in _items)
            {
                _list.Items.Add(o);
            }
            _list.EndUpdate();
            if (select != null && _list.Items.Contains(select))
            {
                _list.SelectedItem = select;
            }
            else if (_list.Items.Count > 0 && _list.SelectedIndex < 0)
            {
                _list.SelectedIndex = 0;
            }
        }
    }
}

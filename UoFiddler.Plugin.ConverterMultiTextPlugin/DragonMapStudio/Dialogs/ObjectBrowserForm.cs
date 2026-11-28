/***************************************************************************
 *
 *  * Dragon Map Studio – object list ("Objekte & Texturen"): every item and every land tile / texture of the client as
 *  *  a clear list (ID, name, height, kind) with search by name or ID (also ranges "08e0-08ff"), quick filters for
 *  *  caves, rocks, stalagmites, walls, floors and the walkable NoDraw pieces, a big picture of the chosen one and
 *  *  "Benutzen" (item → Static setzen, land → Land-Pinsel / Höhlen-Boden).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class ObjectBrowserHost
    {
        /// <summary>An item was chosen to place (Static setzen).</summary>
        public Action<int> UseItem;

        /// <summary>A land tile was chosen for the land brush.</summary>
        public Action<int> UseLand;

        /// <summary>A land tile as floor of the tool "Höhle" (add = mix in).</summary>
        public Action<int, bool> UseCaveFloor;

        public Action<string> Status;
    }

    public sealed class ObjectBrowserForm : Form
    {
        private static ObjectBrowserForm _open;
        private static List<int> _allItems;
        private static List<int> _allLand;

        private readonly ObjectBrowserHost _host;
        private readonly ListView _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, VirtualMode = true, HideSelection = false, MultiSelect = false };
        private readonly ToolStripTextBox _search = new ToolStripTextBox { AutoSize = false, Width = 220 };
        private readonly PictureBox _pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(30, 41, 59) };
        private readonly Label _info = new Label { Dock = DockStyle.Bottom, Height = 110, Padding = new Padding(6), Font = new Font("Consolas", 9f) };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly Timer _delay = new Timer { Interval = 250 };
        private readonly ToolStripButton _items = new ToolStripButton();
        private readonly ToolStripButton _land = new ToolStripButton();
        private readonly ToolStripButton _texture = new ToolStripButton();
        private List<int> _shown = new List<int>();
        private string _filter = "Alle";
        private bool _isLand;

        public static void ShowShared(IWin32Window owner, ObjectBrowserHost host, bool land = false, string filter = null)
        {
            if (_open == null || _open.IsDisposed)
            {
                _open = new ObjectBrowserForm(host);
                if (owner is Form f)
                {
                    _open.Owner = f;
                }
                _open.FormClosed += (s, e) => _open = null;
                _open.Show(owner);
            }
            _open.Activate();
            _open.SetKind(land);
            if (filter != null)
            {
                _open.SetFilter(filter);
            }
        }

        private ObjectBrowserForm(ObjectBrowserHost host)
        {
            _host = host;
            Text = L.T("Objekte & Texturen") + " – Dragon Map Studio";
            Size = new Size(1100, 760);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            ShowInTaskbar = false;
            Font = new Font("Segoe UI", 9f);
            BuildUi();
        }

        private void Say(string t) => _status.Text = "  " + t;

        private void BuildUi()
        {
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _items.Text = L.T("Items (Statics)");
            _items.Click += (s, e) => SetKind(false);
            _land.Text = L.T("Land / Texturen");
            _land.Click += (s, e) => SetKind(true);
            _texture.Text = L.T("als Textur");
            _texture.CheckOnClick = true;
            _texture.ToolTipText = L.T("Land als Textur (texmaps) zeigen statt als Land-Bild");
            _texture.Click += (s, e) => ShowSelected();
            bar.Items.AddRange(new ToolStripItem[] { _items, _land, _texture, new ToolStripSeparator(), new ToolStripLabel(L.T("Suche:")), _search });
            _search.ToolTipText = L.T("Name (z. B. stalag, rock, cave, wall) oder ID (08e3, 0x8e3) oder Bereich (08e0-08ff)");
            _search.TextChanged += (s, e) =>
            {
                _delay.Stop();
                _delay.Start();
            };
            _search.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    _delay.Stop();
                    Refill();
                    e.SuppressKeyPress = true;
                }
            };
            _delay.Tick += (s, e) =>
            {
                _delay.Stop();
                Refill();
            };
            var filters = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            filters.Items.Add(new ToolStripLabel(L.T("Schnell:")));
            foreach (string f in new[] { "Alle", "Höhle", "Felsen", "Stalagmiten", "Wände", "Boden", "NoDraw / begehbar" })
            {
                string name = f;
                var b = new ToolStripButton(L.T(f));
                b.Click += (s, e) => SetFilter(name);
                filters.Items.Add(b);
            }

            _list.Columns.Add("ID", 70);
            _list.Columns.Add(L.T("Name"), 230);
            _list.Columns.Add(L.T("Höhe"), 50);
            _list.Columns.Add(L.T("Art / Flags"), 260);
            _list.RetrieveVirtualItem += (s, e) =>
            {
                int id = _shown[e.ItemIndex];
                string name = _isLand ? UoArtCache.LandName(id) : UoArtCache.ItemName(id);
                string h = _isLand ? string.Empty : UoArtCache.ItemHeight(id).ToString(CultureInfo.InvariantCulture);
                string kind = _isLand ? UoArtCache.LandFlagsText(id) : UoArtCache.ItemKind(id) + " · " + UoArtCache.ItemFlagsText(id);
                e.Item = new ListViewItem(new[] { $"0x{id:X4}", name, h, kind });
            };
            _list.SelectedIndexChanged += (s, e) => ShowSelected();
            _list.DoubleClick += (s, e) => Use(false);
            _list.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    Use(false);
                }
            };

            var right = new Panel { Dock = DockStyle.Fill };
            var use = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(4) };
            Button B(string t, Action a, int w)
            {
                var b = new Button { Text = L.T(t), Width = w, Height = 28, FlatStyle = FlatStyle.System };
                b.Click += (s, e) => a();
                use.Controls.Add(b);
                return b;
            }
            B("✔ Benutzen", () => Use(false), 120);
            B("Als Höhlen-Boden", () => Use(true), 140);
            B("Höhlen-Boden dazu mischen", () => UseCaveMix(), 180);
            right.Controls.Add(_pic);
            right.Controls.Add(_info);
            right.Controls.Add(use);
            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 650 };
            split.Panel1.Controls.Add(_list);
            split.Panel2.Controls.Add(right);
            Controls.Add(split);
            Controls.Add(filters);
            Controls.Add(bar);
            Controls.Add(_status);
            Shown += (s, e) => split.SplitterDistance = Math.Max(400, ClientSize.Width - 400);
        }

        private void SetKind(bool land)
        {
            _isLand = land;
            _items.Checked = !land;
            _land.Checked = land;
            _texture.Enabled = land;
            Refill();
        }

        private void SetFilter(string name)
        {
            _filter = name;
            if (name == "NoDraw / begehbar" || name == "Boden")
            {
                // floors and walkable pieces: land first
            }
            Refill();
        }

        private static List<int> AllItems()
        {
            if (_allItems == null)
            {
                int max = Math.Min(UoArtCache.MaxItemId, 0xFFFF);
                _allItems = Enumerable.Range(0, max + 1).Where(UoArtCache.IsValidStatic).ToList();
            }
            return _allItems;
        }

        private static List<int> AllLand()
        {
            _allLand ??= Enumerable.Range(0, UoArtCache.MaxLandId + 1).Where(UoArtCache.IsValidLand).ToList();
            return _allLand;
        }

        private static bool Has(string name, params string[] words) => words.Any(w => name.Contains(w));

        private bool Matches(int id)
        {
            string name = (_isLand ? UoArtCache.LandName(id) : UoArtCache.ItemName(id)).ToLowerInvariant();
            switch (_filter)
            {
                case "Höhle":
                    if (!Has(name, "cave", "cavern", "dungeon", "stalag", "rock", "dirt"))
                    {
                        return false;
                    }
                    break;
                case "Felsen":
                    if (!Has(name, "rock", "boulder", "rubble", "stone", "pebble"))
                    {
                        return false;
                    }
                    break;
                case "Stalagmiten":
                    if (!Has(name, "stalag", "stalac"))
                    {
                        return false;
                    }
                    break;
                case "Wände":
                    if (_isLand ? !Has(name, "wall", "rock", "mountain") : UoArtCache.ItemKind(id) != StaticKind.Wall && !Has(name, "wall"))
                    {
                        return false;
                    }
                    break;
                case "Boden":
                    if (_isLand ? Has(name, "wall", "void") : UoArtCache.ItemKind(id) != StaticKind.Floor)
                    {
                        return false;
                    }
                    break;
                case "NoDraw / begehbar":
                    if (_isLand ? !CaveEntranceBuilder.IsNoDraw((ushort)id) && id != 0x01DB && !Has(name, "nodraw", "no draw", "void")
                        : !CaveEntranceBuilder.IsNoDrawStatic((ushort)id, UoArtCache.ItemName(id)))
                    {
                        return false;
                    }
                    break;
            }
            string q = _search.Text.Trim().ToLowerInvariant();
            if (q.Length == 0)
            {
                return true;
            }
            int dash = q.IndexOf('-');
            if (dash > 0 && TryHex(q.Substring(0, dash), out int lo) && TryHex(q.Substring(dash + 1), out int hi))
            {
                return id >= Math.Min(lo, hi) && id <= Math.Max(lo, hi);
            }
            if (TryHex(q, out int one) && (q.StartsWith("0x") || q.All(Uri.IsHexDigit)) && q.Length <= 6)
            {
                if (id == one)
                {
                    return true;
                }
                if (q.StartsWith("0x"))
                {
                    return false;
                }
            }
            return name.Contains(q) || $"{id:x4}".Contains(q.Replace("0x", string.Empty));
        }

        private static bool TryHex(string s, out int v)
        {
            s = s.Trim();
            if (s.StartsWith("0x"))
            {
                s = s.Substring(2);
            }
            return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v);
        }

        private void Refill()
        {
            UseWaitCursor = true;
            try
            {
                List<int> source = _isLand ? AllLand() : AllItems();
                _shown = source.Where(Matches).ToList();
                _list.VirtualListSize = 0;
                _list.VirtualListSize = _shown.Count;
                Say($"{_shown.Count:N0} " + L.T(_isLand ? "Land-Tiles" : "Items") + $" ({L.T(_filter)}" + (_search.Text.Length > 0 ? $", „{_search.Text}“" : string.Empty) + ") – " +
                    L.T("Doppelklick / Enter = benutzen"));
                if (_shown.Count > 0)
                {
                    _list.SelectedIndices.Clear();
                    _list.SelectedIndices.Add(0);
                    _list.EnsureVisible(0);
                }
                else
                {
                    _pic.Image = null;
                    _info.Text = string.Empty;
                }
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private int Selected => _list.SelectedIndices.Count > 0 && _list.SelectedIndices[0] < _shown.Count ? _shown[_list.SelectedIndices[0]] : -1;

        private void ShowSelected()
        {
            int id = Selected;
            if (id < 0)
            {
                return;
            }
            try
            {
                _pic.Image = _isLand ? (_texture.Checked ? UoArtCache.Texture(id) : null) ?? UoArtCache.Land(id) : UoArtCache.Static(id, 0);
            }
            catch (Exception)
            {
                _pic.Image = null;
            }
            _info.Text = _isLand
                ? $"Land 0x{id:X4}  {UoArtCache.LandName(id)}\nTextur {UoArtCache.LandTextureId(id)}\n{UoArtCache.LandFlagsText(id)}" +
                  (CaveEntranceBuilder.IsNoDraw((ushort)id) ? "\n" + L.T("NoDraw: unsichtbar, man kann hindurch gehen (Höhlen-Durchgang)") : string.Empty)
                : $"Item 0x{id:X4}  {UoArtCache.ItemName(id)}\n" + L.T("Höhe") + $" {UoArtCache.ItemHeight(id)} (" + L.T("stapelt") + $" {UoArtCache.StackHeight(id)})\n{UoArtCache.ItemKind(id)}\n{UoArtCache.ItemFlagsText(id)}";
        }

        private void Use(bool asCaveFloor)
        {
            int id = Selected;
            if (id < 0)
            {
                return;
            }
            if (_isLand)
            {
                if (asCaveFloor)
                {
                    _host.UseCaveFloor?.Invoke(id, false);
                }
                else
                {
                    _host.UseLand?.Invoke(id);
                }
            }
            else
            {
                if (asCaveFloor)
                {
                    Say(L.T("Höhlen-Boden geht nur mit Land-Tiles (oben 'Land / Texturen')."));
                    return;
                }
                _host.UseItem?.Invoke(id);
            }
            Say(L.T("Benutzt") + $": 0x{id:X4}");
        }

        private void UseCaveMix()
        {
            int id = Selected;
            if (id >= 0 && _isLand)
            {
                _host.UseCaveFloor?.Invoke(id, true);
                Say(L.T("Zum Höhlen-Boden gemischt") + $": 0x{id:X4}");
            }
            else
            {
                Say(L.T("Höhlen-Boden geht nur mit Land-Tiles (oben 'Land / Texturen')."));
            }
        }
    }
}

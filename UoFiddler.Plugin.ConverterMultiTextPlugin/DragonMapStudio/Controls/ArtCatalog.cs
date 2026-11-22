/***************************************************************************
 *
 *  * Dragon Map Studio – art catalog: statics, land tiles and multis with pictures
 *  *
 *  *  Search (name or hex), built-in categories (walls, floors, stairs, bridge, …) and own categories
 *  *  ("+ Kategorie", right click "In Kategorie …"). A click picks the piece for the tool of the view
 *  *  (static setzen, land setzen, multi setzen).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls
{
    public sealed class ArtPickedEventArgs : EventArgs
    {
        public ArtPickedEventArgs(ArtKind kind, int id, bool activated)
        {
            Kind = kind;
            Id = id;
            Activated = activated;
        }

        public ArtKind Kind { get; }
        public int Id { get; }

        /// <summary>Double click / Enter.</summary>
        public bool Activated { get; }
    }

    public sealed class ArtCatalog : Panel
    {
        private const string All = "– alle –";
        private const string AutoPrefix = "◆ ";

        private static List<int> _statics;
        private static List<int> _land;
        private static List<int> _multis;

        private readonly ComboBox _kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        private readonly ComboBox _cat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
        private readonly TextBox _search = new TextBox { Width = 120 };
        private readonly Label _count = new Label { AutoSize = true, ForeColor = Color.FromArgb(100, 116, 139), Margin = new Padding(3, 7, 3, 0) };
        private readonly TileGridView _grid = new TileGridView { Dock = DockStyle.Fill, CellSize = 56 };
        private readonly Timer _delay = new Timer { Interval = 300 };
        private readonly Dictionary<string, List<int>> _autoCache = new Dictionary<string, List<int>>();

        public ArtCatalog()
        {
            Dock = DockStyle.Fill;
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = true, Padding = new Padding(2) };
            _kind.Items.AddRange(new object[] { L.T("Statics"), L.T("Land / Textur"), L.T("Multis") });
            _kind.SelectedIndex = 0;
            _kind.SelectedIndexChanged += (s, e) =>
            {
                FillCategories(null);
                Refill();
            };
            _cat.SelectedIndexChanged += (s, e) => Refill();
            _search.TextChanged += (s, e) =>
            {
                _delay.Stop();
                _delay.Start();
            };
            _delay.Tick += (s, e) =>
            {
                _delay.Stop();
                Refill();
            };
            var tip = new ToolTip();
            tip.SetToolTip(_search, L.T("Suche: Name (z. B. wall, stair, bridge) oder Hex-ID (z. B. 0241)"));
            top.Controls.Add(_kind);
            top.Controls.Add(new Label { Text = "🔍", AutoSize = true, Margin = new Padding(3, 7, 0, 0) });
            top.Controls.Add(_search);
            top.Controls.Add(_cat);
            var add = new Button { Text = L.T("+ Kategorie"), AutoSize = true, Height = 24, FlatStyle = FlatStyle.System };
            add.Click += (s, e) => NewCategory();
            top.Controls.Add(add);
            var size = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 55 };
            size.Items.AddRange(new object[] { "40", "56", "80", "120" });
            size.SelectedIndex = 1;
            size.SelectedIndexChanged += (s, e) => _grid.CellSize = int.Parse((string)size.SelectedItem);
            top.Controls.Add(size);
            top.Controls.Add(_count);
            _grid.ImageOf = ImageOf;
            _grid.NameOf = NameOf;
            _grid.SelectedIdChanged += (s, e) =>
            {
                if (_grid.SelectedId >= 0)
                {
                    Picked?.Invoke(this, new ArtPickedEventArgs(Kind, _grid.SelectedId, false));
                }
            };
            _grid.ItemActivated += (s, e) =>
            {
                if (_grid.SelectedId >= 0)
                {
                    Picked?.Invoke(this, new ArtPickedEventArgs(Kind, _grid.SelectedId, true));
                }
            };
            _grid.ContextMenuStrip = BuildMenu();
            Controls.Add(_grid);
            Controls.Add(top);
            FillCategories(null);
        }

        /// <summary>A piece was clicked (or double clicked).</summary>
        public event EventHandler<ArtPickedEventArgs> Picked;

        public ArtKind Kind
        {
            get => (ArtKind)Math.Max(0, _kind.SelectedIndex);
            set => _kind.SelectedIndex = (int)value;
        }

        public int SelectedId => _grid.SelectedId;

        /// <summary>Opens a category (own or built-in) of the current kind, e.g. "Dungeon (map0)".</summary>
        public void ShowCategory(ArtKind kind, string name)
        {
            Kind = kind;
            FillCategories(name);
            Refill();
        }

        private string _customName;
        private List<int> _customIds;

        /// <summary>Shows a given list (e.g. the bridge pieces found in the tiledata) as an extra category "▸ title".</summary>
        public void ShowIds(ArtKind kind, List<int> ids, string title)
        {
            _customName = "▸ " + title;
            _customIds = ids.ToList();
            _search.Text = string.Empty;
            Kind = kind;
            FillCategories(_customName);
            Refill();
        }

        /// <summary>Search text from outside (e.g. "stair").</summary>
        public void Search(string text)
        {
            _search.Text = text;
            Refill();
        }

        // ------------------------------------------------------------------ lists

        private static List<int> AllIds(ArtKind kind)
        {
            switch (kind)
            {
                case ArtKind.Land:
                    return _land ??= Enumerable.Range(0, UoArtCache.MaxLandId + 1).Where(UoArtCache.IsValidLand).ToList();
                case ArtKind.Multi:
                    if (_multis == null)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        _multis = Enumerable.Range(0, 0x2000).Where(i => UoArtCache.Multi(i).Count > 0).ToList();
                    }
                    return _multis;
                default:
                    return _statics ??= Enumerable.Range(1, Math.Max(1, UoArtCache.MaxItemId)).Where(UoArtCache.IsValidStatic).ToList();
            }
        }

        private void FillCategories(string select)
        {
            _cat.BeginUpdate();
            _cat.Items.Clear();
            _cat.Items.Add(L.T(All));
            if (_customName != null)
            {
                _cat.Items.Add(_customName);
            }
            foreach (ArtCategory c in ArtCategories.Own.Where(c => c.Kind == Kind).OrderBy(c => c.Name))
            {
                _cat.Items.Add(c.Name);
            }
            foreach (string a in Kind == ArtKind.Static ? ArtCategories.StaticAuto : Kind == ArtKind.Land ? ArtCategories.LandAuto : Array.Empty<string>())
            {
                _cat.Items.Add(AutoPrefix + a);
            }
            int i = select == null ? 0 : Math.Max(0, _cat.Items.IndexOf(select) >= 0 ? _cat.Items.IndexOf(select) : _cat.Items.IndexOf(AutoPrefix + select));
            _cat.SelectedIndex = i;
            _cat.EndUpdate();
        }

        private string CategoryName => _cat.SelectedIndex <= 0 ? null : (string)_cat.SelectedItem;

        private bool OwnCategory => CategoryName != null && !CategoryName.StartsWith(AutoPrefix) && CategoryName != _customName;

        private void Refill()
        {
            if (_kind == null || _cat.SelectedIndex < 0)
            {
                return;
            }
            IEnumerable<int> ids;
            string cat = CategoryName;
            if (cat == null)
            {
                ids = AllIds(Kind);
            }
            else if (cat == _customName)
            {
                ids = _customIds ?? new List<int>();
            }
            else if (cat.StartsWith(AutoPrefix))
            {
                string key = Kind + ":" + cat;
                if (!_autoCache.TryGetValue(key, out List<int> list))
                {
                    string name = cat.Substring(AutoPrefix.Length);
                    Cursor.Current = Cursors.WaitCursor;
                    list = AllIds(Kind).Where(id => Kind == ArtKind.Land ? ArtCategories.LandFits(id, name) : ArtCategories.StaticFits(id, name)).ToList();
                    _autoCache[key] = list;
                }
                ids = list;
            }
            else
            {
                ids = ArtCategories.Get(Kind, cat)?.IdList() ?? new List<int>();
            }
            string q = _search.Text.Trim().ToLowerInvariant();
            if (q.Length > 0)
            {
                ushort[] hex = q.Length <= 4 && q.All(Uri.IsHexDigit) ? HexList.Parse(q) : Array.Empty<ushort>();
                string[] words = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                ids = ids.Where(id => (hex.Length > 0 && id == hex[0]) || words.All(w => NameOf(id).ToLowerInvariant().Contains(w)));
            }
            List<int> shown = ids.ToList();
            _grid.Ids = shown;
            _count.Text = $"{shown.Count:N0}";
        }

        private Image ImageOf(int id)
        {
            switch (Kind)
            {
                case ArtKind.Land:
                    return UoArtCache.Land(id);
                case ArtKind.Multi:
                    return UoArtCache.MultiImage(id);
                default:
                    return UoArtCache.Static(id, 0);
            }
        }

        private string NameOf(int id)
        {
            switch (Kind)
            {
                case ArtKind.Land:
                    return $"{id:x4} {UoArtCache.LandName(id)}";
                case ArtKind.Multi:
                    return $"Multi {id:x4} ({UoArtCache.Multi(id).Count} " + L.T("Teile") + ")";
                default:
                    return $"{id:x4} {UoArtCache.ItemName(id)} · h {UoArtCache.ItemHeight(id)}";
            }
        }

        // ------------------------------------------------------------------ own categories

        private ContextMenuStrip BuildMenu()
        {
            var m = new ContextMenuStrip();
            var into = new ToolStripMenuItem(L.T("In Kategorie …"));
            m.Opening += (s, e) =>
            {
                into.DropDownItems.Clear();
                foreach (ArtCategory c in ArtCategories.Own.Where(c => c.Kind == Kind).OrderBy(c => c.Name))
                {
                    string name = c.Name;
                    into.DropDownItems.Add(name, null, (s2, e2) =>
                    {
                        if (_grid.SelectedId >= 0)
                        {
                            ArtCategories.AddTo(Kind, name, new[] { _grid.SelectedId });
                        }
                    });
                }
                into.DropDownItems.Add(new ToolStripSeparator());
                into.DropDownItems.Add(L.T("Neue Kategorie …"), null, (s2, e2) => NewCategory(true));
            };
            m.Items.Add(into);
            m.Items.Add(L.T("Aus dieser Kategorie entfernen"), null, (s, e) =>
            {
                if (OwnCategory && _grid.SelectedId >= 0)
                {
                    ArtCategories.RemoveFrom(Kind, CategoryName, new[] { _grid.SelectedId });
                    Refill();
                }
            });
            m.Items.Add(L.T("Alle gezeigten in Kategorie …"), null, (s, e) =>
            {
                string name = AskName(L.T("Alle gezeigten Teile in die Kategorie (neu oder vorhanden):"));
                if (name != null)
                {
                    ArtCategories.AddTo(Kind, name, _grid.Ids);
                    FillCategories(name);
                    Refill();
                }
            });
            m.Items.Add(new ToolStripSeparator());
            m.Items.Add(L.T("Kategorie löschen"), null, (s, e) =>
            {
                if (OwnCategory && MessageBox.Show(this, L.T("Kategorie löschen?") + " " + CategoryName, L.T("Katalog"), MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ArtCategories.Delete(Kind, CategoryName);
                    FillCategories(null);
                    Refill();
                }
            });
            m.Items.Add(L.T("ID kopieren"), null, (s, e) =>
            {
                if (_grid.SelectedId >= 0)
                {
                    Clipboard.SetText(_grid.SelectedId.ToString("x4"));
                }
            });
            return m;
        }

        private string AskName(string prompt)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox(prompt, L.T("Katalog"), OwnCategory ? CategoryName : L.T("Meine Teile"));
            return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        }

        private void NewCategory(bool withSelected = false)
        {
            string name = AskName(L.T("Name der neuen Kategorie (z. B. Dungeon Wände, Brücke Holz):"));
            if (name == null)
            {
                return;
            }
            ArtCategories.AddTo(Kind, name, withSelected && _grid.SelectedId >= 0 ? new[] { _grid.SelectedId } : Array.Empty<int>());
            FillCategories(name);
            Refill();
        }
    }
}

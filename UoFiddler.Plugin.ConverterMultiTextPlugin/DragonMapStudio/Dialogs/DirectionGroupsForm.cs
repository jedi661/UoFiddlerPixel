/***************************************************************************
 *
 *  * Dragon Map Studio – "Richtungen festlegen": put the graphic for north, east, south and west of a piece into four
 *  *  fields (items, land tiles – land also as texture). The table wins over the automatic recognition when stairs
 *  *  are built, pieces are turned (R, 90°), motifs are turned and land is turned.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class DirectionGroupsForm : Form
    {
        private static DirectionGroupsForm _open;
        private static readonly Compass[] Dirs = { Compass.North, Compass.East, Compass.South, Compass.West };

        private readonly List<DirectionGroup> _groups;
        private readonly ListBox _list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
        private readonly TextBox _name = new TextBox { Width = 220 };
        private readonly ComboBox _kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
        private readonly CheckBox _stair = new CheckBox { AutoSize = true };
        private readonly CheckBox _texture = new CheckBox { AutoSize = true };
        private readonly TextBox _note = new TextBox { Width = 420 };
        private readonly ArtCatalog _catalog = new ArtCatalog { Dock = DockStyle.Fill };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly Dictionary<Compass, PictureBox> _pics = new Dictionary<Compass, PictureBox>();
        private readonly Dictionary<Compass, TextBox> _ids = new Dictionary<Compass, TextBox>();
        private readonly Dictionary<Compass, Panel> _frames = new Dictionary<Compass, Panel>();
        private Compass _active = Compass.North;
        private bool _sync;
        private bool _dirty;

        /// <summary>Opens the window (only once; a second call brings it to the front).</summary>
        public static void ShowShared(IWin32Window owner, string selectName = null)
        {
            if (_open != null && !_open.IsDisposed)
            {
                _open.Activate();
                _open.Select(selectName);
                return;
            }
            _open = new DirectionGroupsForm();
            _open.FormClosed += (s, e) => _open = null;
            if (owner is Form f)
            {
                _open.Owner = f;
            }
            _open.Show(owner);
            _open.Select(selectName);
        }

        private DirectionGroupsForm()
        {
            Text = L.T("Richtungen festlegen") + " – Dragon Map Studio";
            Size = new Size(1180, 820);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            _groups = DirectionGroups.All.Select(Clone).ToList();
            BuildUi();
            FillList(0);
            Say(L.T("Gruppe wählen oder 'Neu'. Feld anklicken (Nord, Ost, Süd, West), dann im Katalog die Grafik anklicken – das nächste Feld wird gewählt. Speichern nicht vergessen."));
            FormClosing += (s, e) =>
            {
                if (_dirty && MessageBox.Show(this, L.T("Änderungen an den Richtungen speichern?"), Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SaveAll();
                }
            };
        }

        private static DirectionGroup Clone(DirectionGroup g) => new DirectionGroup
        {
            Name = g.Name, Kind = g.Kind, Stair = g.Stair, Note = g.Note, Ids = (int[])(g.Ids ?? new int[4]).Clone(), Shipped = g.Shipped
        };

        private void Say(string t) => _status.Text = "  " + t;

        private DirectionGroup Current => _list.SelectedItem as DirectionGroup;

        private void Select(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            int i = _groups.FindIndex(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
            if (i >= 0)
            {
                _list.SelectedIndex = i;
            }
        }

        private void FillList(int select)
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (DirectionGroup g in _groups)
            {
                _list.Items.Add(g);
            }
            _list.EndUpdate();
            if (_list.Items.Count > 0)
            {
                _list.SelectedIndex = Math.Clamp(select, 0, _list.Items.Count - 1);
            }
            else
            {
                ShowGroup();
            }
        }

        private void Changed()
        {
            DirectionGroup g = Current;
            if (g == null || _sync)
            {
                return;
            }
            g.Shipped = false; // a changed project group is saved as an own one
            _dirty = true;
            int i = _list.SelectedIndex;
            _sync = true;
            _list.Items[i] = g;
            _sync = false;
        }

        private void ShowGroup()
        {
            DirectionGroup g = Current;
            _sync = true;
            _name.Text = g?.Name ?? string.Empty;
            _kind.SelectedIndex = g == null ? 0 : (int)g.Kind;
            _stair.Checked = g?.Stair ?? false;
            _note.Text = g?.Note ?? string.Empty;
            foreach (Compass c in Dirs)
            {
                int id = g?[c] ?? 0;
                _ids[c].Text = id == 0 ? string.Empty : $"0x{id:X4}";
                _pics[c].Image = Picture(g?.Kind ?? DirectionKind.Item, id);
            }
            _sync = false;
            if (g != null)
            {
                _catalog.Kind = g.Kind == DirectionKind.Land ? ArtKind.Land : ArtKind.Static;
            }
            SetActive(_active);
        }

        private Image Picture(DirectionKind kind, int id)
        {
            if (id <= 0)
            {
                return null;
            }
            try
            {
                if (kind == DirectionKind.Land)
                {
                    return (_texture.Checked ? UoArtCache.Texture(id) : null) ?? UoArtCache.Land(id);
                }
                return UoArtCache.Static(id, 0);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void SetActive(Compass c)
        {
            _active = c;
            foreach (KeyValuePair<Compass, Panel> kv in _frames)
            {
                kv.Value.BackColor = kv.Key == c ? Color.FromArgb(245, 158, 11) : Color.FromArgb(203, 213, 225);
            }
        }

        private void SetId(Compass c, int id)
        {
            DirectionGroup g = Current;
            if (g == null)
            {
                return;
            }
            g[c] = id;
            _sync = true;
            _ids[c].Text = id == 0 ? string.Empty : $"0x{id:X4}";
            _pics[c].Image = Picture(g.Kind, id);
            _sync = false;
            Changed();
        }

        private static bool TryHex(string s, out int id)
        {
            s = (s ?? string.Empty).Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out id);
            }
            if (s.Length == 0)
            {
                id = 0;
                return true;
            }
            return int.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out id);
        }

        /// <summary>Fills empty fields from the filled ones with the automatic recognition (stair slope / wall outline).</summary>
        private void Suggest()
        {
            DirectionGroup g = Current;
            if (g == null || g.Kind != DirectionKind.Item)
            {
                Say(L.T("Vorschlagen geht nur für Items."));
                return;
            }
            int seed = Dirs.Select(c => g[c]).FirstOrDefault(i => i != 0);
            if (seed == 0)
            {
                Say(L.T("Erst ein Feld mit einer Grafik füllen."));
                return;
            }
            int found = 0;
            string name = UoArtCache.ItemName(seed).Trim().ToLowerInvariant();
            if (g.Stair)
            {
                // neighbours with the same name, sorted by their slope in the art
                for (int d = -24; d <= 24; d++)
                {
                    int id = seed + d;
                    if (id <= 0 || id > UoArtCache.MaxItemId || UoArtCache.ItemName(id).Trim().ToLowerInvariant() != name)
                    {
                        continue;
                    }
                    Compass? up = null;
                    try
                    {
                        up = BridgeKit.StairUp(id, out _);
                    }
                    catch (Exception)
                    {
                        // no art
                    }
                    if (up != null && g[up.Value] == 0)
                    {
                        SetId(up.Value, id);
                        found++;
                    }
                }
            }
            else
            {
                int o = BridgeKit.Orientation(seed);
                int other = BridgeKit.Turned((ushort)seed);
                if (o != 0)
                {
                    // +1 = runs west – east: east / west slots, the turned one north / south
                    Compass[] mine = o > 0 ? new[] { Compass.East, Compass.West } : new[] { Compass.North, Compass.South };
                    Compass[] theirs = o > 0 ? new[] { Compass.North, Compass.South } : new[] { Compass.East, Compass.West };
                    foreach (Compass c in mine.Where(c => g[c] == 0))
                    {
                        SetId(c, seed);
                        found++;
                    }
                    if (other != seed)
                    {
                        foreach (Compass c in theirs.Where(c => g[c] == 0))
                        {
                            SetId(c, other);
                            found++;
                        }
                    }
                }
            }
            Say(found > 0 ? found + " " + L.T("Felder vorgeschlagen – bitte ansehen und falsche von Hand ändern.") : L.T("Nichts gefunden – bitte von Hand legen."));
        }

        private void SaveAll()
        {
            DirectionGroups.Save(_groups);
            _dirty = false;
            Say(L.T("Gespeichert") + ": " + DirectionGroups.FilePath + " – " + L.T("Treppen, Drehen und Muster benutzen die Tabelle jetzt."));
        }

        private void BuildUi()
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 330 };
            // ---- left: groups
            var leftBar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 96, Padding = new Padding(2) };
            Button B(string t, Action a, int w = 0)
            {
                var b = new Button { Text = L.T(t), AutoSize = w == 0, Width = w == 0 ? 75 : w, Height = 26, Margin = new Padding(2), FlatStyle = FlatStyle.System };
                b.Click += (s, e) => a();
                return b;
            }
            leftBar.Controls.Add(B("Neu", () =>
            {
                _groups.Add(new DirectionGroup { Name = L.T("Neue Gruppe") + " " + (_groups.Count + 1), Kind = _catalog.Kind == ArtKind.Land ? DirectionKind.Land : DirectionKind.Item });
                _dirty = true;
                FillList(_groups.Count - 1);
                SetActive(Compass.North);
                _name.Focus();
                _name.SelectAll();
            }, 60));
            leftBar.Controls.Add(B("Kopieren", () =>
            {
                if (Current == null)
                {
                    return;
                }
                DirectionGroup c = Clone(Current);
                c.Name += " (2)";
                c.Shipped = false;
                _groups.Add(c);
                _dirty = true;
                FillList(_groups.Count - 1);
            }, 70));
            leftBar.Controls.Add(B("Löschen", () =>
            {
                if (Current == null)
                {
                    return;
                }
                if (Current.Shipped)
                {
                    Say(L.T("Diese Gruppe kommt aus dem Projekt (DragonData\\Richtungen) – dort löschen, oder hier ändern (wird dann eigene)."));
                    return;
                }
                int i = _list.SelectedIndex;
                _groups.RemoveAt(i);
                _dirty = true;
                FillList(i);
            }, 70));
            leftBar.Controls.Add(B("💾 Speichern", SaveAll, 100));
            leftBar.Controls.Add(B("Exportieren …", () =>
            {
                using var d = new SaveFileDialog { Filter = "JSON|*.json", FileName = "richtungen.json", Title = L.T("Richtungen exportieren (z. B. ins Projekt: DragonData\\Richtungen)") };
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    DirectionGroups.Export(_groups, d.FileName);
                    Say(L.T("Exportiert") + ": " + d.FileName);
                }
            }, 100));
            leftBar.Controls.Add(B("Importieren …", () =>
            {
                using var d = new OpenFileDialog { Filter = "JSON|*.json" };
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                int n = 0;
                foreach (DirectionGroup g in DirectionGroups.ImportFile(d.FileName))
                {
                    g.Shipped = false;
                    _groups.RemoveAll(x => string.Equals(x.Name, g.Name, StringComparison.OrdinalIgnoreCase));
                    _groups.Add(g);
                    n++;
                }
                _dirty = true;
                FillList(_groups.Count - 1);
                Say(n + " " + L.T("Gruppen importiert – speichern nicht vergessen."));
            }, 100));
            split.Panel1.Controls.Add(_list);
            split.Panel1.Controls.Add(leftBar);
            split.Panel1.Controls.Add(new Label { Dock = DockStyle.Top, Height = 20, Text = "  " + L.T("GRUPPEN (Name: Nord Ost Süd West)"), BackColor = Color.FromArgb(68, 64, 60), ForeColor = Color.White });
            _list.SelectedIndexChanged += (s, e) =>
            {
                if (!_sync)
                {
                    ShowGroup();
                }
            };

            // ---- right: editor + catalog
            var right = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 330 };
            var top = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(6) };
            var row1 = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            row1.Controls.Add(new Label { Text = L.T("Name:"), AutoSize = true, Margin = new Padding(0, 6, 2, 0) });
            row1.Controls.Add(_name);
            row1.Controls.Add(new Label { Text = L.T("Art:"), AutoSize = true, Margin = new Padding(10, 6, 2, 0) });
            _kind.Items.AddRange(new object[] { L.T("Items"), L.T("Land-Tiles") });
            row1.Controls.Add(_kind);
            _stair.Text = L.T("Treppe (Richtung = hinauf nach)");
            _stair.Margin = new Padding(10, 5, 0, 0);
            row1.Controls.Add(_stair);
            _texture.Text = L.T("Land als Textur zeigen");
            _texture.Margin = new Padding(10, 5, 0, 0);
            row1.Controls.Add(_texture);
            top.Controls.Add(row1);
            var row2 = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            row2.Controls.Add(new Label { Text = L.T("Notiz:"), AutoSize = true, Margin = new Padding(0, 6, 2, 0) });
            row2.Controls.Add(_note);
            top.Controls.Add(row2);

            var slots = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 8, 0, 0) };
            foreach (Compass c in Dirs)
            {
                var frame = new Panel { Width = 150, Height = 200, Padding = new Padding(3), Margin = new Padding(4), Cursor = Cursors.Hand };
                var inner = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
                var pic = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White, Cursor = Cursors.Hand };
                string arrow = c switch { Compass.North => "↗", Compass.East => "↘", Compass.South => "↙", _ => "↖" };
                var title = new Label { Dock = DockStyle.Top, Height = 20, Text = $"{arrow} {L.T(Motif.DirName(c))}", TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
                var idBox = new TextBox { Dock = DockStyle.Bottom };
                var clear = new Button { Dock = DockStyle.Bottom, Height = 24, Text = "✖ " + L.T("leeren"), FlatStyle = FlatStyle.System };
                Compass cc = c;
                void Activate(object s, EventArgs e) => SetActive(cc);
                frame.Click += Activate;
                pic.Click += Activate;
                title.Click += Activate;
                idBox.Enter += Activate;
                idBox.TextChanged += (s, e) =>
                {
                    if (_sync || Current == null)
                    {
                        return;
                    }
                    if (TryHex(idBox.Text, out int id))
                    {
                        Current[cc] = id;
                        pic.Image = Picture(Current.Kind, id);
                        Changed();
                    }
                };
                clear.Click += (s, e) => SetId(cc, 0);
                inner.Controls.Add(pic);
                inner.Controls.Add(title);
                inner.Controls.Add(idBox);
                inner.Controls.Add(clear);
                frame.Controls.Add(inner);
                _pics[c] = pic;
                _ids[c] = idBox;
                _frames[c] = frame;
                slots.Controls.Add(frame);
            }
            top.Controls.Add(slots);
            var row3 = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
            row3.Controls.Add(B("Aus einem Feld vorschlagen", Suggest, 190));
            row3.Controls.Add(B("Hilfe", () => MessageBox.Show(this, L.T(
                "Richtungen festlegen:\n\n" +
                "• Eine Gruppe = ein Teil in vier Richtungen (Nord ↗, Ost ↘, Süd ↙, West ↖ – so wie es im Client zu sehen ist).\n" +
                "• Treppe: die Richtung bedeutet 'geht hinauf nach'. Dann baut das Werkzeug Treppe und der Treppen-Designer genau diese Stufen ein.\n" +
                "• Wand / Zaun: Ost und West = das Teil, das West → Ost läuft, Nord und Süd = das Teil Nord → Süd. Ein leeres Feld nimmt das gegenüberliegende.\n" +
                "• Land-Tiles: beim Drehen eines Bereichs (90°) werden die Tiles nach der Tabelle getauscht.\n" +
                "• Was in der Tabelle steht, gilt vor der automatischen Erkennung (Treppen, Drehen, Muster).\n" +
                "• Exportieren ins Projekt: DragonData\\Richtungen\\*.json – wird dann mit ausgeliefert."), Text), 70));
            top.Controls.Add(row3);
            top.Controls.Add(new Label
            {
                AutoSize = false, Width = 640, Height = 36, ForeColor = Color.FromArgb(71, 85, 105),
                Text = L.T("Feld anklicken (gelb = aktiv), dann im Katalog unten die Grafik anklicken. Die Nummer kann auch direkt eingetippt werden (hex).")
            });
            right.Panel1.Controls.Add(top);
            right.Panel2.Controls.Add(_catalog);
            right.Panel2.Controls.Add(new Label
            {
                Dock = DockStyle.Top, Height = 18, Text = "  " + L.T("KATALOG – Items / Land oben umschalten · Klick = ins gelbe Feld"),
                BackColor = Color.FromArgb(180, 83, 9), ForeColor = Color.White
            });
            split.Panel2.Controls.Add(right);
            Controls.Add(split);
            Controls.Add(_status);

            _name.TextChanged += (s, e) =>
            {
                if (!_sync && Current != null)
                {
                    Current.Name = _name.Text;
                    Changed();
                }
            };
            _note.TextChanged += (s, e) =>
            {
                if (!_sync && Current != null)
                {
                    Current.Note = _note.Text;
                    Changed();
                }
            };
            _stair.CheckedChanged += (s, e) =>
            {
                if (!_sync && Current != null)
                {
                    Current.Stair = _stair.Checked;
                    Changed();
                }
            };
            _kind.SelectedIndexChanged += (s, e) =>
            {
                if (!_sync && Current != null)
                {
                    Current.Kind = (DirectionKind)_kind.SelectedIndex;
                    _catalog.Kind = Current.Kind == DirectionKind.Land ? ArtKind.Land : ArtKind.Static;
                    Changed();
                    ShowGroup();
                }
            };
            _texture.CheckedChanged += (s, e) => ShowGroup();
            _catalog.Picked += (s, e) =>
            {
                DirectionGroup g = Current;
                if (g == null || e.Kind == ArtKind.Multi)
                {
                    return;
                }
                var kind = e.Kind == ArtKind.Land ? DirectionKind.Land : DirectionKind.Item;
                if (kind != g.Kind)
                {
                    if (Dirs.Any(c => g[c] != 0))
                    {
                        Say(L.T("Diese Gruppe ist für") + " " + (g.Kind == DirectionKind.Land ? L.T("Land-Tiles") : L.T("Items")) + " – " + L.T("im Katalog oben die passende Art wählen."));
                        return;
                    }
                    g.Kind = kind;
                    ShowGroup();
                }
                SetId(_active, e.Id);
                Say(L.T(Motif.DirName(_active)) + $" = 0x{e.Id:X4}");
                Compass next = Dirs.FirstOrDefault(c => g[c] == 0);
                SetActive(Dirs.Any(c => g[c] == 0) ? next : (Compass)(((int)_active + 1) % 4));
            };
        }
    }
}

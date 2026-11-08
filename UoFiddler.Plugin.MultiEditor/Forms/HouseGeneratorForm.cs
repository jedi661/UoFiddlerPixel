/***************************************************************************
 *
 * MultiEditor – house generator window
 *   tab "House":    size, floors, roof, options, learning
 *   tab "Learned":  the learned template (editable) + parts of the style
 *   tab "Parts":    building parts placed into the house
 *   tab "Roles":    learned role of every tile id (correctable)
 *   right side:     example house (what was learned, colored) and result
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public sealed class HouseGeneratorForm : Form
    {
        private readonly Func<List<ImportTile>> _getCurrentTiles;
        private readonly Func<int> _getDrawTile;
        private readonly Action<List<ImportTile>, string> _createNew;
        private readonly Action<List<ImportTile>> _insert;
        private readonly Action<HouseStyle> _restyle;

        private readonly ComboBox _styleCombo;
        private readonly NumericUpDown _width;
        private readonly NumericUpDown _depth;
        private readonly NumericUpDown _floors;
        private readonly NumericUpDown _floorHeight;
        private readonly NumericUpDown _baseZ;
        private readonly NumericUpDown _roofStep;
        private readonly NumericUpDown _windowSpacing;
        private readonly ComboBox _roof;
        private readonly CheckBox _door;
        private readonly CheckBox _windows;
        private readonly CheckBox _stairs;
        private readonly CheckBox _innerWall;
        private readonly DataGridView _grid;
        private readonly PropertyGrid _templateGrid;
        private readonly DataGridView _partsGrid;
        private readonly DataGridView _rolesGrid;
        private readonly CheckBox _rolesOnlyStyle;
        private readonly PictureBox _preview;
        private readonly PictureBox _example;
        private readonly Label _exampleLabel;
        private readonly Label _info;
        private readonly Label _templateInfo;
        private readonly Random _rnd = new Random();

        private List<ImportTile> _lastTiles = new List<ImportTile>();
        private bool _loadingStyle;

        private static readonly TileRole[] _editableRoles =
        {
            TileRole.WallX, TileRole.WallY, TileRole.Corner, TileRole.Post, TileRole.Floor,
            TileRole.WindowX, TileRole.WindowY, TileRole.RoofNorth, TileRole.RoofSouth,
            TileRole.RoofWest, TileRole.RoofEast, TileRole.RoofRidge, TileRole.Stair
        };

        public HouseGeneratorForm(Func<List<ImportTile>> getCurrentTiles, Func<int> getDrawTile,
            Action<List<ImportTile>, string> createNew, Action<List<ImportTile>> insert, Action<HouseStyle> restyle)
        {
            _getCurrentTiles = getCurrentTiles;
            _getDrawTile = getDrawTile;
            _createNew = createNew;
            _insert = insert;
            _restyle = restyle;

            Text = L.T("Hausbau-Generator", "House generator");
            Size = new Size(1280, 820);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var tabHouse = new TabPage(L.T("Haus", "House")) { AutoScroll = true };
            var tabLearned = new TabPage(L.T("Gelernt", "Learned"));
            var tabParts = new TabPage(L.T("Bauteile", "Parts"));
            var tabRoles = new TabPage(L.T("Rollen", "Roles"));
            tabs.TabPages.AddRange(new[] { tabHouse, tabLearned, tabParts, tabRoles });

            // ---------- tab house ----------
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4, Padding = new Padding(4) };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            _styleCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
            _width = N(1, 60, 7);
            _depth = N(1, 60, 7);
            _floors = N(1, 6, 1);
            _floorHeight = N(5, 40, 20);
            _baseZ = N(-60, 100, 0);
            _roofStep = N(1, 10, 3);
            _windowSpacing = N(2, 10, 3);
            _roof = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _roof.Items.AddRange(new object[]
            {
                L.T("Kein Dach", "No roof"), L.T("Flachdach", "Flat roof"),
                L.T("Satteldach (First X)", "Gable roof (ridge X)"), L.T("Satteldach (First Y)", "Gable roof (ridge Y)"),
                L.T("Wie gelernt", "As learned")
            });
            _roof.SelectedIndex = 4;
            _door = new CheckBox { Text = L.T("Türöffnung (Süd)", "Door gap (south)"), Checked = true, AutoSize = true };
            _windows = new CheckBox { Text = L.T("Fenster (gelerntes Muster)", "Windows (learned pattern)"), Checked = true, AutoSize = true };
            _stairs = new CheckBox { Text = L.T("Treppen", "Stairs"), Checked = true, AutoSize = true };
            _innerWall = new CheckBox { Text = L.T("Innenwand", "Inner wall"), AutoSize = true };

            int r = 0;
            void Row(string l1, Control c1, string l2, Control c2)
            {
                table.Controls.Add(new Label { Text = l1, AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, r);
                table.Controls.Add(c1, 1, r);
                if (l2 != null)
                {
                    table.Controls.Add(new Label { Text = l2, AutoSize = true, Margin = new Padding(8, 7, 3, 3) }, 2, r);
                    table.Controls.Add(c2, 3, r);
                }

                ++r;
            }

            table.Controls.Add(new Label { Text = L.T("Stil:", "Style:"), AutoSize = true, Margin = new Padding(3, 7, 3, 3) }, 0, r);
            table.Controls.Add(_styleCombo, 1, r);
            table.SetColumnSpan(_styleCombo, 3);
            ++r;
            Row(L.T("Boden Breite (X):", "Floor width (X):"), _width, L.T("Boden Tiefe (Y):", "Floor depth (Y):"), _depth);
            Row(L.T("Etagen:", "Floors:"), _floors, L.T("Basis-Z:", "Base Z:"), _baseZ);
            Row(L.T("Etagenhöhe*:", "Floor height*:"), _floorHeight, L.T("Dachstufe Z*:", "Roof step Z*:"), _roofStep);
            Row(L.T("Dach:", "Roof:"), _roof, L.T("Fensterabst.*:", "Window gap*:"), _windowSpacing);
            var checks = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            checks.Controls.AddRange(new Control[] { _door, _windows, _stairs, _innerWall });
            table.Controls.Add(checks, 0, r);
            table.SetColumnSpan(checks, 4);
            ++r;
            var star = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(420, 0),
                ForeColor = SystemColors.GrayText,
                Text = L.T("* nur für Stile ohne gelernte Vorlage – mit Vorlage gelten die gelernten Werte (Reiter „Gelernt“).",
                    "* only for styles without learned template – with a template the learned values apply (tab \"Learned\").")
            };
            table.Controls.Add(star, 0, r);
            table.SetColumnSpan(star, 4);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(2) };
            actions.Controls.Add(Btn(L.T("Vorschau", "Preview"), (_, _) => DoPreview()));
            actions.Controls.Add(Btn(L.T("Zufall", "Random"), (_, _) => DoRandom()));
            actions.Controls.Add(Btn(L.T("Als neues Multi", "Create as new multi"), (_, _) => DoCreate()));
            actions.Controls.Add(Btn(L.T("In aktuelles Multi einfügen", "Insert into current multi"), (_, _) => DoInsert()));
            actions.Controls.Add(Btn(L.T("Aktuelles Multi umstylen", "Restyle current multi"), (_, _) => DoRestyle()));

            var learn = new GroupBox { Text = L.T("Lernen aus Beispielen (gute, fertige Häuser)", "Learn from examples (good, finished houses)"), Dock = DockStyle.Top, Height = 96 };
            var lf = new FlowLayoutPanel { Dock = DockStyle.Fill };
            lf.Controls.Add(Btn(L.T("Aktuelles Multi", "Current multi"), (_, _) => LearnCurrent()));
            lf.Controls.Add(Btn(L.T("Multi.mul Bereich…", "Multi.mul range…"), (_, _) => LearnMulRange()));
            lf.Controls.Add(Btn(L.T("Dateien…", "Files…"), (_, _) => LearnFiles()));
            lf.Controls.Add(Btn(L.T("Wissen zurücksetzen", "Reset knowledge"), (_, _) => ResetKnowledge()));
            learn.Controls.Add(lf);

            var styleButtons = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            styleButtons.Controls.Add(Btn(L.T("Neuer Stil", "New style"), (_, _) => NewStyle()));
            styleButtons.Controls.Add(Btn(L.T("Stil löschen", "Delete style"), (_, _) => DeleteStyle()));
            styleButtons.Controls.Add(Btn(L.T("Stil umbenennen", "Rename style"), (_, _) => RenameStyle()));

            _info = new Label { Dock = DockStyle.Top, Height = 60, ForeColor = SystemColors.GrayText, Padding = new Padding(4) };

            tabHouse.Controls.Add(_info);
            tabHouse.Controls.Add(styleButtons);
            tabHouse.Controls.Add(learn);
            tabHouse.Controls.Add(actions);
            tabHouse.Controls.Add(table);

            // ---------- tab learned ----------
            _templateInfo = new Label { Dock = DockStyle.Top, Height = 70, Padding = new Padding(4) };
            _templateGrid = new PropertyGrid { Dock = DockStyle.Fill, ToolbarVisible = false, PropertySort = PropertySort.Categorized, HelpVisible = true };
            _templateGrid.PropertyValueChanged += (_, _) =>
            {
                StyleKnowledge.Instance.Save();
                ShowExample();
                DoPreview();
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 250,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Teil (ohne Vorlage)", "Part (without template)"), ReadOnly = true, FillWeight = 45 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 25 });
            _grid.Columns.Add(new DataGridViewImageColumn { HeaderText = "Art", FillWeight = 30, ImageLayout = DataGridViewImageCellLayout.Zoom });
            _grid.RowTemplate.Height = 34;
            foreach (TileRole role in _editableRoles)
            {
                _grid.Rows.Add(RoleName(role), string.Empty, null);
            }

            _grid.CellEndEdit += (_, e) => GridEdited(e.RowIndex);

            var learnedButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            learnedButtons.Controls.Add(Btn(L.T("Zeichen-Tile → Teil", "Draw tile → part"), (_, _) => UseDrawTileForRow()));
            learnedButtons.Controls.Add(Btn(L.T("Vorlage aus aktuellem Multi neu lernen", "Relearn template from current multi"), (_, _) => RelearnTemplate()));
            learnedButtons.Controls.Add(Btn(L.T("Vorlage entfernen", "Remove template"), (_, _) =>
            {
                if (CurrentStyle != null)
                {
                    CurrentStyle.Template = null;
                    StyleKnowledge.Instance.Save();
                    LoadStyleToGrid();
                }
            }));

            tabLearned.Controls.Add(_templateGrid);
            tabLearned.Controls.Add(_templateInfo);
            tabLearned.Controls.Add(learnedButtons);
            tabLearned.Controls.Add(_grid);

            // ---------- tab parts ----------
            _partsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            var prefabCol = new DataGridViewComboBoxColumn { HeaderText = L.T("Bauteil", "Part"), FillWeight = 50, FlatStyle = FlatStyle.Flat };
            foreach (Prefab p in StructureLibrary.Instance.Prefabs)
            {
                if (!prefabCol.Items.Contains(p.Name))
                {
                    prefabCol.Items.Add(p.Name);
                }
            }

            _partsGrid.Columns.Add(prefabCol);
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "X", FillWeight = 12 });
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Y", FillWeight = 12 });
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Etage", "Floor"), FillWeight = 12 });
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Drehung", "Rotation"), FillWeight = 14 });
            _partsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Z+", FillWeight = 10 });
            _partsGrid.CellValueChanged += (_, _) => DoPreview();
            _partsGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_partsGrid.IsCurrentCellDirty)
                {
                    _partsGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            _partsGrid.DataError += (_, e) => e.ThrowException = false;

            var partButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
            partButtons.Controls.Add(Btn(L.T("+ Bauteil", "+ Part"), (_, _) =>
            {
                string first = StructureLibrary.Instance.Prefabs.FirstOrDefault()?.Name;
                if (first == null)
                {
                    MessageBox.Show(this, L.T("Die Bauteile-Bibliothek ist leer. Im Editor eine Auswahl als Bauteil speichern (Reiter „Bauteile“).",
                        "The parts library is empty. Save a selection as building part in the editor (tab \"Parts\")."), Text);
                    return;
                }

                _partsGrid.Rows.Add(first, "1", "1", "0", "0", "0");
                DoPreview();
            }));
            partButtons.Controls.Add(Btn(L.T("Entfernen", "Remove"), (_, _) =>
            {
                if (_partsGrid.CurrentRow != null)
                {
                    _partsGrid.Rows.Remove(_partsGrid.CurrentRow);
                    DoPreview();
                }
            }));
            partButtons.Controls.Add(Btn(L.T("Drehen", "Rotate"), (_, _) =>
            {
                if (_partsGrid.CurrentRow != null)
                {
                    int.TryParse(Convert.ToString(_partsGrid.CurrentRow.Cells[4].Value), out int rot);
                    _partsGrid.CurrentRow.Cells[4].Value = ((rot + 1) % 4).ToString(CultureInfo.InvariantCulture);
                }
            }));
            var partsHelp = new Label
            {
                Dock = DockStyle.Top,
                Height = 54,
                Padding = new Padding(4),
                ForeColor = SystemColors.GrayText,
                Text = L.T("Fertige Bauteile (Treppen, Türrahmen, Zinnen, Möbel …) ins Haus setzen. X/Y relativ zur Nordwest-Ecke des Bodens, Etage 0 = Erdgeschoss, Drehung 0-3 (×90°).",
                    "Place ready parts (stairs, door frames, battlements, furniture …) into the house. X/Y relative to the north-west corner of the floor, floor 0 = ground floor, rotation 0-3 (×90°).")
            };
            tabParts.Controls.Add(_partsGrid);
            tabParts.Controls.Add(partsHelp);
            tabParts.Controls.Add(partButtons);

            // ---------- tab roles ----------
            _rolesOnlyStyle = new CheckBox { Text = L.T("Nur Teile des gewählten Stils", "Only parts of the selected style"), Checked = true, AutoSize = true, Dock = DockStyle.Top };
            _rolesOnlyStyle.CheckedChanged += (_, _) => FillRoles();
            _rolesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _rolesGrid.RowTemplate.Height = 40;
            _rolesGrid.Columns.Add(new DataGridViewImageColumn { HeaderText = "Art", FillWeight = 18, ImageLayout = DataGridViewImageCellLayout.Zoom });
            _rolesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", ReadOnly = true, FillWeight = 16 });
            _rolesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", ReadOnly = true, FillWeight = 26 });
            var roleCol = new DataGridViewComboBoxColumn { HeaderText = L.T("Rolle", "Role"), FillWeight = 26, FlatStyle = FlatStyle.Flat };
            foreach (TileRole role in Enum.GetValues(typeof(TileRole)))
            {
                roleCol.Items.Add(role.ToString());
            }

            _rolesGrid.Columns.Add(roleCol);
            _rolesGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = L.T("Stimmen", "Votes"), ReadOnly = true, FillWeight = 14 });
            _rolesGrid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_rolesGrid.IsCurrentCellDirty)
                {
                    _rolesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            _rolesGrid.CellValueChanged += RolesGrid_CellValueChanged;
            _rolesGrid.DataError += (_, e) => e.ThrowException = false;
            var rolesHelp = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(4),
                ForeColor = SystemColors.GrayText,
                Text = L.T("Rolle ändern = korrigieren. Wird für Drehen/Spiegeln, Symmetrie und Umstylen benutzt.", "Change a role = correct it. Used for rotate/mirror, symmetry and restyle.")
            };
            tabRoles.Controls.Add(_rolesGrid);
            tabRoles.Controls.Add(rolesHelp);
            tabRoles.Controls.Add(_rolesOnlyStyle);

            // ---------- right: example + result ----------
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 330 };
            _exampleLabel = new Label { Dock = DockStyle.Top, Height = 34, Padding = new Padding(4), ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 55) };
            _example = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.FromArgb(35, 45, 60) };
            split.Panel1.Controls.Add(_example);
            split.Panel1.Controls.Add(_exampleLabel);
            var resultLabel = new Label { Dock = DockStyle.Top, Height = 22, Padding = new Padding(4), ForeColor = Color.White, BackColor = Color.FromArgb(50, 50, 55), Text = L.T("Ergebnis", "Result") };
            _preview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.CenterImage, BackColor = Color.FromArgb(40, 60, 40) };
            split.Panel2.Controls.Add(_preview);
            split.Panel2.Controls.Add(resultLabel);

            var left = new Panel { Dock = DockStyle.Left, Width = 470 };
            left.Controls.Add(tabs);
            Controls.Add(split);
            Controls.Add(new Splitter { Dock = DockStyle.Left });
            Controls.Add(left);

            _styleCombo.SelectedIndexChanged += (_, _) => LoadStyleToGrid();
            foreach (NumericUpDown n in new[] { _width, _depth, _floors, _floorHeight, _baseZ, _roofStep, _windowSpacing })
            {
                n.ValueChanged += (_, _) => DoPreview();
            }

            _roof.SelectedIndexChanged += (_, _) => DoPreview();
            foreach (CheckBox c in new[] { _door, _windows, _stairs, _innerWall })
            {
                c.CheckedChanged += (_, _) => DoPreview();
            }

            split.Panel1.Resize += (_, _) => ShowExample();

            EnsureDefaultStyle();
            FillStyles();
        }

        private static NumericUpDown N(int min, int max, int value)
        {
            return new NumericUpDown { Minimum = min, Maximum = max, Value = value, Width = 70 };
        }

        private static Button Btn(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private static string RoleName(TileRole role)
        {
            return role switch
            {
                TileRole.WallX => L.T("Wand entlang X", "Wall along X"),
                TileRole.WallY => L.T("Wand entlang Y", "Wall along Y"),
                TileRole.Corner => L.T("Ecke", "Corner"),
                TileRole.Post => L.T("Pfosten", "Post"),
                TileRole.Floor => L.T("Boden", "Floor"),
                TileRole.WindowX => L.T("Fenster X", "Window X"),
                TileRole.WindowY => L.T("Fenster Y", "Window Y"),
                TileRole.RoofNorth => L.T("Dach Nordseite", "Roof north side"),
                TileRole.RoofSouth => L.T("Dach Südseite", "Roof south side"),
                TileRole.RoofWest => L.T("Dach Westseite", "Roof west side"),
                TileRole.RoofEast => L.T("Dach Ostseite", "Roof east side"),
                TileRole.RoofRidge => L.T("Dachfirst", "Roof ridge"),
                TileRole.Stair => L.T("Treppe", "Stair"),
                _ => role.ToString()
            };
        }

        private StyleKnowledge K => StyleKnowledge.Instance;

        private HouseStyle CurrentStyle => _styleCombo.SelectedItem as HouseStyle;

        private void EnsureDefaultStyle()
        {
            // styles learned with an older version: learn the template from their multi.mul source now
            bool changed = false;
            foreach (HouseStyle st in K.Styles.Where(x => x.Template == null && x.Source != null && x.Source.StartsWith("Multi 0x", StringComparison.OrdinalIgnoreCase)))
            {
                if (!int.TryParse(st.Source.Substring(8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int mid))
                {
                    continue;
                }

                try
                {
                    MultiComponentList m = Multis.GetComponents(mid);
                    if (m != MultiComponentList.Empty)
                    {
                        st.Template = HouseTemplate.Learn(MultiFormats.FromSdk(m));
                        changed |= st.Template != null;
                    }
                }
                catch
                {
                    // ignore
                }
            }

            if (changed)
            {
                K.Save();
            }

            if (K.Styles.Count > 0)
            {
                return;
            }

            // first start: learn from the classic houses of the loaded multi.mul (0x64 - 0x7F)
            try
            {
                for (int id = 0x64; id <= 0x7F; ++id)
                {
                    MultiComponentList m = Multis.GetComponents(id);
                    if (m != MultiComponentList.Empty)
                    {
                        K.Learn(MultiFormats.FromSdk(m), $"Multi 0x{id:X}");
                    }
                }
            }
            catch
            {
                // multi.mul not available
            }

            if (K.Styles.Count == 0)
            {
                K.Styles.Add(new HouseStyle { Name = L.T("Eigener Stil", "Custom style"), Source = "manual" });
            }

            K.Save();
        }

        private void FillStyles()
        {
            object sel = _styleCombo.SelectedItem;
            _styleCombo.Items.Clear();
            foreach (HouseStyle s in K.Styles)
            {
                _styleCombo.Items.Add(s);
            }

            if (sel != null && _styleCombo.Items.Contains(sel))
            {
                _styleCombo.SelectedItem = sel;
            }
            else if (_styleCombo.Items.Count > 0)
            {
                int withTemplate = K.Styles.FindIndex(s => s.Template != null);
                _styleCombo.SelectedIndex = Math.Max(0, withTemplate);
            }

            _info.Text = L.T(
                $"Stile: {K.Styles.Count} (davon mit Vorlage: {K.Styles.Count(s => s.Template != null)})   gelernte Beispiele: {K.LearnedExamples}   bekannte Kacheln: {K.RoleVotes.Count}",
                $"Styles: {K.Styles.Count} (with template: {K.Styles.Count(s => s.Template != null)})   learned examples: {K.LearnedExamples}   known tiles: {K.RoleVotes.Count}");
        }

        private void LoadStyleToGrid()
        {
            HouseStyle s = CurrentStyle;
            if (s == null)
            {
                return;
            }

            _loadingStyle = true;
            for (int i = 0; i < _editableRoles.Length; ++i)
            {
                int id = s.Get(_editableRoles[i]);
                _grid.Rows[i].Cells[1].Value = id > 0 ? $"0x{id:X4}" : string.Empty;
                _grid.Rows[i].Cells[2].Value = id > 0 ? Art.GetStatic(id) : null;
            }

            _templateGrid.SelectedObject = s.Template != null ? new TemplateView(s.Template) : null;
            if (s.Template != null)
            {
                HouseTemplate t = s.Template;
                _templateInfo.Text = L.T(
                    $"Vorlage aus „{s.Source}“: {t.Levels.Count} Etage(n), Etagenhöhe {t.FloorHeight}, Dach: {t.Roof}. Wände: N {t.OffsetNorth:+0;-0;0}, S {t.OffsetSouth:+0;-0;0}, W {t.OffsetWest:+0;-0;0}, O {t.OffsetEast:+0;-0;0} zur Bodenkante.\nWerte unten ändern = Vorlage korrigieren (wird gespeichert).",
                    $"Template from \"{s.Source}\": {t.Levels.Count} level(s), floor height {t.FloorHeight}, roof: {t.Roof}. Walls: N {t.OffsetNorth:+0;-0;0}, S {t.OffsetSouth:+0;-0;0}, W {t.OffsetWest:+0;-0;0}, E {t.OffsetEast:+0;-0;0} to the floor edge.\nChange values below = correct the template (is saved).");
                if (_width.Value < 3)
                {
                    _width.Value = 7;
                }
            }
            else
            {
                _templateInfo.Text = L.T("Dieser Stil hat keine gelernte Vorlage – er baut nach einfachen Regeln (Tabelle unten). Für richtige Häuser ein gutes Beispielhaus lernen.",
                    "This style has no learned template – it builds with simple rules (table below). For proper houses learn a good example house.");
            }

            _loadingStyle = false;
            ShowExample();
            FillRoles();
            DoPreview();
        }

        private void ShowExample()
        {
            HouseStyle s = CurrentStyle;
            Image old = _example.Image;
            _example.Image = null;
            old?.Dispose();
            if (s?.Template == null || s.Template.Example.Count == 0)
            {
                _exampleLabel.Text = L.T("Beispiel: (keins – Stil ohne Vorlage)", "Example: (none – style without template)");
                return;
            }

            HouseTemplate t = s.Template;
            _exampleLabel.Text = L.T($"Beispiel „{s.Source}“ – rot = Ecken, blau = Nord/Süd, grün = West/Ost, orange = Dach",
                $"Example \"{s.Source}\" – red = corners, blue = north/south, green = west/east, orange = roof");
            var tiles = t.Example.Select(e => new ImportTile { Id = (ushort)e[0], X = e[1], Y = e[2], Z = e[3] }).ToList();

            Color? Tint(ImportTile e)
            {
                if (e.Id < TileData.ItemTable.Length && (TileData.ItemTable[e.Id].Flags & TileFlag.Roof) != 0)
                {
                    return Color.Orange;
                }

                if (HouseTemplate.IsFloorTile(e.Id))
                {
                    return null;
                }

                bool onX = e.X == t.ExampleWest || e.X == t.ExampleEast;
                bool onY = e.Y == t.ExampleNorth || e.Y == t.ExampleSouth;
                bool insideX = e.X >= t.ExampleWest && e.X <= t.ExampleEast;
                bool insideY = e.Y >= t.ExampleNorth && e.Y <= t.ExampleSouth;
                if (onX && onY)
                {
                    return Color.Red;
                }

                if (onY && insideX)
                {
                    return Color.DodgerBlue;
                }

                if (onX && insideY)
                {
                    return Color.LimeGreen;
                }

                return null;
            }

            _example.Image = MultiPreview.Render(tiles, Math.Max(80, _example.Width - 10), Math.Max(80, _example.Height - 10), Tint);
        }

        private void FillRoles()
        {
            if (_rolesGrid == null)
            {
                return;
            }

            _rolesGrid.CellValueChanged -= RolesGrid_CellValueChanged;
            _rolesGrid.Rows.Clear();
            IEnumerable<int> ids;
            HouseStyle s = CurrentStyle;
            if (_rolesOnlyStyle.Checked && s != null)
            {
                var set = new HashSet<int>(_editableRoles.Select(s.Get).Where(i => i > 0));
                if (s.Template != null)
                {
                    foreach (int id in s.Template.WallIds())
                    {
                        set.Add(id);
                    }

                    foreach (int id in s.Template.RoofRowsA.Concat(s.Template.RoofRowsB).Append(s.Template.RoofRidge).Append(s.Template.FloorId).Append(s.Template.UpperFloorId))
                    {
                        if (id > 0)
                        {
                            set.Add(id);
                        }
                    }
                }

                ids = set.OrderBy(i => i);
            }
            else
            {
                ids = K.RoleVotes.Keys.Concat(K.RoleOverrides.Keys).Distinct().OrderBy(i => i).Take(2000);
            }

            foreach (int id in ids)
            {
                string votes = K.RoleVotes.TryGetValue(id, out Dictionary<TileRole, int> v) ? string.Join(", ", v.OrderByDescending(x => x.Value).Take(3).Select(x => $"{x.Key} {x.Value}")) : string.Empty;
                string name = id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : string.Empty;
                int row = _rolesGrid.Rows.Add(Art.GetStatic(id), $"0x{id:X4}", name, K.GetRole(id).ToString(), votes);
                _rolesGrid.Rows[row].Tag = id;
                if (K.RoleOverrides.ContainsKey(id))
                {
                    _rolesGrid.Rows[row].Cells[3].Style.BackColor = Color.LightYellow;
                }
            }

            _rolesGrid.CellValueChanged += RolesGrid_CellValueChanged;
        }

        private void RolesGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 3 || _rolesGrid.Rows[e.RowIndex].Tag is not int id)
            {
                return;
            }

            if (Enum.TryParse(Convert.ToString(_rolesGrid.Rows[e.RowIndex].Cells[3].Value), out TileRole role))
            {
                K.RoleOverrides[id] = role;
                K.Save();
                _rolesGrid.Rows[e.RowIndex].Cells[3].Style.BackColor = Color.LightYellow;
            }
        }

        private void GridEdited(int row)
        {
            if (_loadingStyle || row < 0 || CurrentStyle == null)
            {
                return;
            }

            string text = Convert.ToString(_grid.Rows[row].Cells[1].Value);
            int id = 0;
            if (!string.IsNullOrWhiteSpace(text) && !SimpleDialog.TryParseId(text, out id))
            {
                return;
            }

            CurrentStyle.Set(_editableRoles[row], Math.Max(0, id));
            K.Save();
            LoadStyleToGrid();
        }

        private void UseDrawTileForRow()
        {
            if (CurrentStyle == null || _grid.CurrentRow == null)
            {
                return;
            }

            int id = _getDrawTile();
            if (id < 0 || id == 0xFFFF)
            {
                return;
            }

            CurrentStyle.Set(_editableRoles[_grid.CurrentRow.Index], id);
            K.Save();
            LoadStyleToGrid();
        }

        private void RelearnTemplate()
        {
            HouseStyle s = CurrentStyle;
            List<ImportTile> tiles = _getCurrentTiles();
            if (s == null || tiles == null || tiles.Count == 0)
            {
                return;
            }

            HouseTemplate t = HouseTemplate.Learn(tiles);
            if (t == null)
            {
                MessageBox.Show(this, L.T("Im aktuellen Multi wurde kein Haus erkannt (Boden + vier Wandseiten nötig).", "No house found in the current multi (floor + four wall sides needed)."), Text);
                return;
            }

            s.Template = t;
            s.Source = L.T("aktuelles Multi", "current multi");
            K.Save();
            LoadStyleToGrid();
        }

        private void NewStyle()
        {
            string name = SimpleDialog.AskText(this, L.T("Neuer Stil", "New style"), "Name:", "Style " + (K.Styles.Count + 1));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            HouseStyle s = CurrentStyle?.Clone() ?? new HouseStyle();
            s.Name = name;
            s.Source = "manual";
            K.Styles.Add(s);
            K.Save();
            FillStyles();
            _styleCombo.SelectedItem = s;
        }

        private void DeleteStyle()
        {
            if (CurrentStyle == null)
            {
                return;
            }

            K.Styles.Remove(CurrentStyle);
            K.Save();
            FillStyles();
        }

        private void RenameStyle()
        {
            if (CurrentStyle == null)
            {
                return;
            }

            string name = SimpleDialog.AskText(this, L.T("Umbenennen", "Rename"), "Name:", CurrentStyle.Name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            CurrentStyle.Name = name;
            K.Save();
            FillStyles();
        }

        private void LearnFrom(IList<ImportTile> tiles, string name, List<string> report)
        {
            HouseStyle s = K.Learn(tiles, name);
            if (s.Template != null)
            {
                HouseTemplate t = s.Template;
                report.Add(L.T($"{name}: Vorlage gelernt ({t.Levels.Count} Etage(n), Höhe {t.FloorHeight}, Dach {t.Roof})",
                    $"{name}: template learned ({t.Levels.Count} level(s), height {t.FloorHeight}, roof {t.Roof})"));
            }
            else
            {
                report.Add(s.IsUsable
                    ? L.T($"{name}: nur einfacher Stil (kein klares Haus erkannt)", $"{name}: simple style only (no clear house found)")
                    : L.T($"{name}: nur Kachel-Rollen gelernt", $"{name}: only tile roles learned"));
            }
        }

        private void FinishLearning(List<string> report)
        {
            K.Save();
            FillStyles();
            string text = string.Join(Environment.NewLine, report.Take(30));
            if (report.Count > 30)
            {
                text += Environment.NewLine + "…";
            }

            MessageBox.Show(this, text, L.T("Gelernt", "Learned"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LearnCurrent()
        {
            List<ImportTile> tiles = _getCurrentTiles();
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            string name = SimpleDialog.AskText(this, L.T("Stil-Name", "Style name"), "Name:", L.T("Haus ", "House ") + DateTime.Now.ToString("HHmmss"));
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var report = new List<string>();
            LearnFrom(tiles, name, report);
            FinishLearning(report);
            HouseStyle learned = K.Styles.LastOrDefault(s => s.Name == name);
            if (learned != null)
            {
                _styleCombo.SelectedItem = learned;
            }
        }

        private void LearnMulRange()
        {
            var d = new SimpleDialog(L.T("Aus Multi.mul lernen", "Learn from Multi.mul"))
                .AddText("from", L.T("Von ID:", "From id:"), "0x64")
                .AddText("to", L.T("Bis ID:", "To id:"), "0x7F")
                .AddInfo(L.T("Klassische Häuser liegen z.B. bei 0x64 - 0x7F.", "Classic houses are e.g. at 0x64 - 0x7F."));
            if (!d.ShowOk(this) || !SimpleDialog.TryParseId(d.GetText("from"), out int from) || !SimpleDialog.TryParseId(d.GetText("to"), out int to))
            {
                return;
            }

            var report = new List<string>();
            Cursor = Cursors.WaitCursor;
            try
            {
                for (int id = Math.Max(0, from); id <= Math.Min(Multis.MaximumMultiIndex - 1, to); ++id)
                {
                    MultiComponentList m = Multis.GetComponents(id);
                    if (m == MultiComponentList.Empty)
                    {
                        continue;
                    }

                    LearnFrom(MultiFormats.FromSdk(m), $"Multi 0x{id:X}", report);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            FinishLearning(report);
        }

        private void LearnFiles()
        {
            using var dlg = new OpenFileDialog { Multiselect = true, Filter = MultiFormats.ImportFilter };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var report = new List<string>();
            foreach (string f in dlg.FileNames)
            {
                try
                {
                    ImportResult res = MultiFormats.Load(f);
                    LearnFrom(res.Tiles, res.Name, report);
                }
                catch (Exception ex)
                {
                    report.Add($"{f}: {ex.Message}");
                }
            }

            FinishLearning(report);
        }

        private void ResetKnowledge()
        {
            if (MessageBox.Show(this, L.T("Alle gelernten Stile, Vorlagen und Rollen löschen?", "Delete all learned styles, templates and roles?"),
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            K.Reset();
            EnsureDefaultStyle();
            K.Save();
            FillStyles();
        }

        private List<PrefabPlacement> GetParts()
        {
            var list = new List<PrefabPlacement>();
            foreach (DataGridViewRow row in _partsGrid.Rows)
            {
                string name = Convert.ToString(row.Cells[0].Value);
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                int I(int c) => int.TryParse(Convert.ToString(row.Cells[c].Value), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
                list.Add(new PrefabPlacement { Prefab = name, X = I(1), Y = I(2), Floor = I(3), Rotation = I(4), Dz = I(5) });
            }

            return list;
        }

        private HouseGeneratorOptions GetOptions()
        {
            return new HouseGeneratorOptions
            {
                Width = (int)_width.Value,
                Depth = (int)_depth.Value,
                Floors = (int)_floors.Value,
                FloorHeight = (int)_floorHeight.Value,
                BaseZ = (int)_baseZ.Value,
                RoofStep = (int)_roofStep.Value,
                WindowSpacing = (int)_windowSpacing.Value,
                Roof = (RoofType)_roof.SelectedIndex,
                DoorGap = _door.Checked,
                Windows = _windows.Checked,
                Stairs = _stairs.Checked,
                InnerWall = _innerWall.Checked,
                Style = CurrentStyle,
                Parts = GetParts()
            };
        }

        private void DoPreview()
        {
            if (_loadingStyle)
            {
                return;
            }

            HouseGeneratorOptions o = GetOptions();
            _lastTiles = HouseGenerator.Generate(o);
            Image old = _preview.Image;
            _preview.Image = MultiPreview.Render(_lastTiles, Math.Max(100, _preview.Width - 10), Math.Max(100, _preview.Height - 10));
            old?.Dispose();
            if (o.Style == null || !o.Style.IsUsable)
            {
                _info.Text = L.T("Stil braucht eine gelernte Vorlage oder Wand X, Wand Y und Boden.", "Style needs a learned template or wall X, wall Y and floor.");
            }
            else if (_lastTiles.Count > 0)
            {
                _info.Text = L.T($"{_lastTiles.Count} Kacheln · Multi-Größe {_lastTiles.Max(t => t.X) + 1} x {_lastTiles.Max(t => t.Y) + 1}" + (o.Style.Template != null ? " · nach gelernter Vorlage" : " · einfache Regeln"),
                    $"{_lastTiles.Count} tiles · multi size {_lastTiles.Max(t => t.X) + 1} x {_lastTiles.Max(t => t.Y) + 1}" + (o.Style.Template != null ? " · learned template" : " · simple rules"));
            }
        }

        private void DoRandom()
        {
            HouseGeneratorOptions o = HouseGenerator.Randomize(_rnd, K);
            _width.Value = Math.Max(_width.Minimum, Math.Min(_width.Maximum, o.Width));
            _depth.Value = Math.Max(_depth.Minimum, Math.Min(_depth.Maximum, o.Depth));
            _floors.Value = o.Floors;
            _windows.Checked = o.Windows;
            _innerWall.Checked = o.InnerWall;
            _roof.SelectedIndex = 4;
            if (o.Style != null)
            {
                _styleCombo.SelectedItem = o.Style;
            }

            DoPreview();
        }

        private void DoCreate()
        {
            DoPreview();
            if (_lastTiles.Count == 0)
            {
                return;
            }

            _createNew(_lastTiles.Select(t => t.Clone()).ToList(), CurrentStyle?.Name ?? "House");
        }

        private void DoInsert()
        {
            DoPreview();
            if (_lastTiles.Count == 0)
            {
                return;
            }

            _insert(_lastTiles.Select(t => t.Clone()).ToList());
        }

        private void DoRestyle()
        {
            if (CurrentStyle == null)
            {
                return;
            }

            _restyle(CurrentStyle);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ShowExample();
            DoPreview();
        }

        /// <summary>
        /// Editable view of a template for the property grid
        /// </summary>
        private sealed class TemplateView
        {
            private readonly HouseTemplate _t;

            public TemplateView(HouseTemplate t)
            {
                _t = t;
            }

            private static string Id(int id) => id > 0 ? $"0x{id:X4}" : string.Empty;

            private static int ParseId(string s) => TileStack.TryId(s ?? string.Empty, out int v) ? v : 0;

            private static string Ids(List<int> ids) => string.Join(", ", ids.Select(i => $"0x{i:X4}"));

            private static List<int> ParseIds(string s) => (s ?? string.Empty).Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(ParseId).Where(i => i > 0).ToList();

            private LevelTemplate Lv => _t.Levels[Math.Max(0, Math.Min(_t.Levels.Count - 1, Level))];

            [Category("1 Boden / Floor"), DisplayName("Boden-ID / floor id")]
            public string FloorId { get => Id(_t.FloorId); set => _t.FloorId = ParseId(value); }

            [Category("1 Boden / Floor"), DisplayName("Obergeschoss-Boden / upper floor id")]
            public string UpperFloorId { get => Id(_t.UpperFloorId); set => _t.UpperFloorId = ParseId(value); }

            [Category("1 Boden / Floor"), DisplayName("Etagenhöhe / floor height")]
            public int FloorHeight { get => _t.FloorHeight; set => _t.FloorHeight = Math.Max(1, value); }

            [Category("2 Wandlinien / wall lines"), DisplayName("Nord (zur Bodenkante)"), Description("Wand-Linie minus Bodenkante. -1 = eine Zelle außerhalb des Bodens, 0 = auf der Bodenkante.")]
            public int OffsetNorth { get => _t.OffsetNorth; set => _t.OffsetNorth = value; }

            [Category("2 Wandlinien / wall lines"), DisplayName("Süd (zur Bodenkante)")]
            public int OffsetSouth { get => _t.OffsetSouth; set => _t.OffsetSouth = value; }

            [Category("2 Wandlinien / wall lines"), DisplayName("West (zur Bodenkante)")]
            public int OffsetWest { get => _t.OffsetWest; set => _t.OffsetWest = value; }

            [Category("2 Wandlinien / wall lines"), DisplayName("Ost (zur Bodenkante)")]
            public int OffsetEast { get => _t.OffsetEast; set => _t.OffsetEast = value; }

            [Category("3 Etage / level"), DisplayName("Angezeigte Etage / shown level"), Description("0 = Erdgeschoss. Die Werte darunter gehören zu dieser Etage. Format: 0xID@dz+0xID@dz, Muster mit | getrennt.")]
            public int Level { get; set; }

            [Category("3 Etage / level"), DisplayName("Ecke NW")]
            public string CornerNW { get => Lv.CornerNW.ToString(); set => Lv.CornerNW = TileStack.Parse(value); }

            [Category("3 Etage / level"), DisplayName("Ecke NO / NE")]
            public string CornerNE { get => Lv.CornerNE.ToString(); set => Lv.CornerNE = TileStack.Parse(value); }

            [Category("3 Etage / level"), DisplayName("Ecke SW")]
            public string CornerSW { get => Lv.CornerSW.ToString(); set => Lv.CornerSW = TileStack.Parse(value); }

            [Category("3 Etage / level"), DisplayName("Ecke SO / SE")]
            public string CornerSE { get => Lv.CornerSE.ToString(); set => Lv.CornerSE = TileStack.Parse(value); }

            [Category("4 Seiten / sides"), DisplayName("Nord: Muster")]
            public string NorthPattern { get => Lv.North.PatternText; set => Lv.North.PatternText = value; }

            [Category("4 Seiten / sides"), DisplayName("Nord: normale Wand")]
            public string NorthPlain { get => Lv.North.Plain.ToString(); set => Lv.North.Plain = TileStack.Parse(value); }

            [Category("4 Seiten / sides"), DisplayName("Nord: erstes / letztes Feld")]
            public string NorthEnds { get => $"{Lv.North.First} | {Lv.North.Last}"; set => SetEnds(Lv.North, value); }

            [Category("4 Seiten / sides"), DisplayName("Süd: Muster")]
            public string SouthPattern { get => Lv.South.PatternText; set => Lv.South.PatternText = value; }

            [Category("4 Seiten / sides"), DisplayName("Süd: normale Wand")]
            public string SouthPlain { get => Lv.South.Plain.ToString(); set => Lv.South.Plain = TileStack.Parse(value); }

            [Category("4 Seiten / sides"), DisplayName("Süd: erstes / letztes Feld")]
            public string SouthEnds { get => $"{Lv.South.First} | {Lv.South.Last}"; set => SetEnds(Lv.South, value); }

            [Category("4 Seiten / sides"), DisplayName("West: Muster")]
            public string WestPattern { get => Lv.West.PatternText; set => Lv.West.PatternText = value; }

            [Category("4 Seiten / sides"), DisplayName("West: normale Wand")]
            public string WestPlain { get => Lv.West.Plain.ToString(); set => Lv.West.Plain = TileStack.Parse(value); }

            [Category("4 Seiten / sides"), DisplayName("West: erstes / letztes Feld")]
            public string WestEnds { get => $"{Lv.West.First} | {Lv.West.Last}"; set => SetEnds(Lv.West, value); }

            [Category("4 Seiten / sides"), DisplayName("Ost: Muster")]
            public string EastPattern { get => Lv.East.PatternText; set => Lv.East.PatternText = value; }

            [Category("4 Seiten / sides"), DisplayName("Ost: normale Wand")]
            public string EastPlain { get => Lv.East.Plain.ToString(); set => Lv.East.Plain = TileStack.Parse(value); }

            [Category("4 Seiten / sides"), DisplayName("Ost: erstes / letztes Feld")]
            public string EastEnds { get => $"{Lv.East.First} | {Lv.East.Last}"; set => SetEnds(Lv.East, value); }

            [Category("5 Dach / roof"), DisplayName("Dachart / roof type")]
            public RoofType Roof { get => _t.Roof; set => _t.Roof = value == RoofType.FromTemplate ? _t.Roof : value; }

            [Category("5 Dach / roof"), DisplayName("Start-Z über letzter Etage")]
            public int RoofDz { get => _t.RoofDz; set => _t.RoofDz = value; }

            [Category("5 Dach / roof"), DisplayName("Stufe Z pro Reihe")]
            public int RoofStep { get => _t.RoofStep; set => _t.RoofStep = value; }

            [Category("5 Dach / roof"), DisplayName("Überstand Nord"), Description("Dachkante minus Wandlinie. -1 = eine Zelle über die Wand hinaus.")]
            public int RoofOverNorth { get => _t.RoofOverNorth; set => _t.RoofOverNorth = value; }

            [Category("5 Dach / roof"), DisplayName("Überstand Süd")]
            public int RoofOverSouth { get => _t.RoofOverSouth; set => _t.RoofOverSouth = value; }

            [Category("5 Dach / roof"), DisplayName("Überstand West")]
            public int RoofOverWest { get => _t.RoofOverWest; set => _t.RoofOverWest = value; }

            [Category("5 Dach / roof"), DisplayName("Überstand Ost")]
            public int RoofOverEast { get => _t.RoofOverEast; set => _t.RoofOverEast = value; }

            [Category("5 Dach / roof"), DisplayName("Reihen erste Seite (zum First)")]
            public string RoofRowsA { get => Ids(_t.RoofRowsA); set => _t.RoofRowsA = ParseIds(value); }

            [Category("5 Dach / roof"), DisplayName("Reihen zweite Seite (zum First)")]
            public string RoofRowsB { get => Ids(_t.RoofRowsB); set => _t.RoofRowsB = ParseIds(value); }

            [Category("5 Dach / roof"), DisplayName("First / ridge")]
            public string RoofRidge { get => Id(_t.RoofRidge); set => _t.RoofRidge = ParseId(value); }

            [Category("5 Dach / roof"), DisplayName("Flachdach-ID")]
            public string FlatRoofId { get => Id(_t.FlatRoofId); set => _t.FlatRoofId = ParseId(value); }

            private static void SetEnds(SideTemplate side, string value)
            {
                string[] p = (value ?? string.Empty).Split('|');
                side.First = TileStack.Parse(p.Length > 0 ? p[0] : string.Empty);
                side.Last = TileStack.Parse(p.Length > 1 ? p[1] : string.Empty);
            }
        }
    }
}

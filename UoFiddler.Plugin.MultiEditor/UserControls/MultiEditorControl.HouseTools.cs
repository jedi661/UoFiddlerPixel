/***************************************************************************
 *
 * MultiEditor – ServUO house script (markers), walkability check,
 * symmetry mode, hue picker, material list, multi browser.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Forms;

namespace UoFiddler.Plugin.MultiEditor.UserControls
{
    public partial class MultiEditorControl
    {
        private enum SymmetryMode
        {
            Off,
            X,
            Y,
            Both
        }

        private readonly List<HouseMarker> _houseMarkers = new List<HouseMarker>();
        private List<WalkIssue> _walkIssues = new List<WalkIssue>();
        private SymmetryMode _symmetry = SymmetryMode.Off;
        private ToolStripDropDownButton _tbSymmetry;
        private CheckResultForm _checkForm;
        private MultiBrowserForm _browserForm;

        #region Toolbar / menu entries

        /// <summary>
        /// Adds the buttons to the second toolbar (called after BuildStructureToolBar)
        /// </summary>
        private void AddHouseToolButtons()
        {
            if (_toolBar2 == null)
            {
                return;
            }

            _tbSymmetry = new ToolStripDropDownButton { ToolTipText = L.T("Symmetrie-Modus: beim Bauen wird an der Multi-Mitte gespiegelt", "Symmetry mode: building is mirrored at the multi center") };
            void SymItem(string text, SymmetryMode mode)
            {
                var i = new ToolStripMenuItem(text);
                i.Click += (_, _) => SetSymmetry(mode);
                _tbSymmetry.DropDownItems.Add(i);
            }

            SymItem(L.T("Symmetrie aus", "Symmetry off"), SymmetryMode.Off);
            SymItem(L.T("Spiegeln an X-Mitte (links/rechts)", "Mirror at X center"), SymmetryMode.X);
            SymItem(L.T("Spiegeln an Y-Mitte (vorne/hinten)", "Mirror at Y center"), SymmetryMode.Y);
            SymItem(L.T("4-fach (X und Y)", "4-way (X and Y)"), SymmetryMode.Both);
            UpdateSymmetryButton();

            var hue = new ToolStripButton(L.T("Farbe…", "Color…")) { ToolTipText = L.T("Hue mit Farbvorschau wählen", "Choose hue with color preview") };
            hue.Click += (_, _) => PickDrawHue();

            var check = new ToolStripButton(L.T("Prüfen", "Check")) { ToolTipText = L.T("Begehbarkeit prüfen (Stufen, Löcher, Deckenhöhe, Erreichbarkeit)", "Check walkability (steps, holes, ceiling, reachability)") };
            check.Click += (_, _) => RunWalkCheck();

            _toolBar2.Items.AddRange(new ToolStripItem[] { new ToolStripSeparator(), _tbSymmetry, hue, check });
        }

        private void AddHouseToolMenus()
        {
            ToolStripMenuItem file = (ToolStripMenuItem)_menu.Items[0];
            int idx = file.DropDownItems.IndexOf(file.DropDownItems.Cast<ToolStripItem>().First(i => i is ToolStripSeparator)) + 1;
            file.DropDownItems.Insert(idx, Item(L.T("Multi-Browser (Vorschaubilder)…", "Multi browser (thumbnails)…"), (_, _) => OpenMultiBrowser()));
            file.DropDownItems.Insert(idx + 1, new ToolStripSeparator());

            int exportIdx = file.DropDownItems.Cast<ToolStripItem>().ToList().FindIndex(i => i.Text.Contains("ServUO"));
            file.DropDownItems.Insert(exportIdx + 1, Item(L.T("ServUO Haus-Skript (BaseHouse) exportieren…", "Export ServUO house script (BaseHouse)…"), (_, _) => ExportHouseScript()));

            var extras = new ToolStripMenuItem(L.T("E&xtras", "E&xtras"));
            extras.DropDownItems.Add(Item(L.T("Begehbarkeit prüfen", "Check walkability"), (_, _) => RunWalkCheck()));
            extras.DropDownItems.Add(Item(L.T("Prüf-Markierungen entfernen", "Remove check marks"), (_, _) =>
            {
                _walkIssues.Clear();
                pictureBoxMulti.Invalidate();
            }));
            extras.DropDownItems.Add(new ToolStripSeparator());
            extras.DropDownItems.Add(Item(L.T("Material- und Kachelliste…", "Material and tile list…"), (_, _) => OpenMaterialList()));
            extras.DropDownItems.Add(Item(L.T("Multi-Browser…", "Multi browser…"), (_, _) => OpenMultiBrowser()));
            extras.DropDownItems.Add(Item(L.T("Hue wählen…", "Choose hue…"), (_, _) => PickDrawHue()));
            extras.DropDownItems.Add(new ToolStripSeparator());
            var sym = new ToolStripMenuItem(L.T("Symmetrie", "Symmetry"));
            sym.DropDownItems.Add(Item(L.T("Aus", "Off"), (_, _) => SetSymmetry(SymmetryMode.Off)));
            sym.DropDownItems.Add(Item("X", (_, _) => SetSymmetry(SymmetryMode.X)));
            sym.DropDownItems.Add(Item("Y", (_, _) => SetSymmetry(SymmetryMode.Y)));
            sym.DropDownItems.Add(Item(L.T("4-fach", "4-way"), (_, _) => SetSymmetry(SymmetryMode.Both)));
            extras.DropDownItems.Add(sym);
            extras.DropDownItems.Add(new ToolStripSeparator());
            extras.DropDownItems.Add(Item(L.T("Haus-Markierungen löschen", "Delete house markers"), (_, _) =>
            {
                _houseMarkers.Clear();
                pictureBoxMulti.Invalidate();
            }));
            extras.DropDownItems.Add(Item(L.T("ServUO Haus-Skript…", "ServUO house script…"), (_, _) => ExportHouseScript()));

            _menu.Items.Insert(Math.Min(5, _menu.Items.Count), extras);
        }

        #endregion

        #region House markers / script

        private void SetMarker(HouseMarkerKind kind, int x, int y, int z)
        {
            if (kind is HouseMarkerKind.Sign or HouseMarkerKind.BanLocation)
            {
                _houseMarkers.RemoveAll(m => m.Kind == kind);
            }
            else
            {
                _houseMarkers.RemoveAll(m => m.X == x && m.Y == y && m.Kind is HouseMarkerKind.DoorSouth or HouseMarkerKind.DoorEast);
            }

            _houseMarkers.Add(new HouseMarker { Kind = kind, X = x, Y = y, Z = z });
            pictureBoxMulti.Invalidate();
        }

        private void AddMarkerContextItems()
        {
            if (_compList == null)
            {
                return;
            }

            int x = _contextCell.X;
            int y = _contextCell.Y;
            int z = _contextZ;
            var m = new ToolStripMenuItem(L.T("Haus (ServUO)", "House (ServUO)"));
            m.DropDownItems.Add(Item(L.T($"Tür Süd hier (Z {z})", $"South door here (Z {z})"), (_, _) => SetMarker(HouseMarkerKind.DoorSouth, x, y, z)));
            m.DropDownItems.Add(Item(L.T($"Tür Ost hier (Z {z})", $"East door here (Z {z})"), (_, _) => SetMarker(HouseMarkerKind.DoorEast, x, y, z)));
            m.DropDownItems.Add(Item(L.T("Hausschild hier", "House sign here"), (_, _) => SetMarker(HouseMarkerKind.Sign, x, y, z + 5)));
            m.DropDownItems.Add(Item(L.T("Ban-Location hier", "Ban location here"), (_, _) => SetMarker(HouseMarkerKind.BanLocation, x, y, z)));
            List<HouseMarker> here = _houseMarkers.Where(h => h.X == x && h.Y == y).ToList();
            if (here.Count > 0)
            {
                m.DropDownItems.Add(Item(L.T("Markierung hier löschen", "Delete marker here"), (_, _) =>
                {
                    _houseMarkers.RemoveAll(h => h.X == x && h.Y == y);
                    pictureBoxMulti.Invalidate();
                }));
            }

            m.DropDownItems.Add(new ToolStripSeparator());
            m.DropDownItems.Add(Item(L.T("Haus-Skript exportieren…", "Export house script…"), (_, _) => ExportHouseScript()));
            _multiContext.Items.Add(m);
        }

        private void PaintHouseMarkers(Graphics g)
        {
            if (_compList == null || _houseMarkers.Count == 0)
            {
                return;
            }

            using var font = new Font("Arial", 8f, FontStyle.Bold);
            foreach (HouseMarker m in _houseMarkers)
            {
                Point p = BackgroundLayer.CellToScreen(_compList, m.X, m.Y, m.Z, hScrollBar.Value, vScrollBar.Value);
                Color c = m.Kind switch
                {
                    HouseMarkerKind.DoorSouth or HouseMarkerKind.DoorEast => Color.Lime,
                    HouseMarkerKind.Sign => Color.Gold,
                    _ => Color.DeepPink
                };
                Point[] d =
                {
                    new Point(p.X + 22, p.Y + 6), new Point(p.X + 38, p.Y + 22),
                    new Point(p.X + 22, p.Y + 38), new Point(p.X + 6, p.Y + 22)
                };
                using (var fill = new SolidBrush(Color.FromArgb(110, c)))
                {
                    g.FillPolygon(fill, d);
                }

                using (var pen = new Pen(c, 2))
                {
                    g.DrawPolygon(pen, d);
                }

                SizeF s = g.MeasureString(m.Letter, font);
                g.DrawString(m.Letter, font, Brushes.Black, p.X + 22 - (s.Width / 2) + 1, p.Y + 22 - (s.Height / 2) + 1);
                g.DrawString(m.Letter, font, Brushes.White, p.X + 22 - (s.Width / 2), p.Y + 22 - (s.Height / 2));
            }
        }

        /// <summary>
        /// Markers in the original orientation, relative to the origin
        /// </summary>
        private List<HouseMarker> GetCanonicalMarkers()
        {
            var list = _houseMarkers.Select(m => new HouseMarker { Kind = m.Kind, X = m.X, Y = m.Y, Z = m.Z }).ToList();
            int w = _compList.Width;
            int h = _compList.Height;
            for (int i = 0; i < (4 - _compList.ViewRotation) % 4; ++i)
            {
                RotateMarkersCw(list, h);
                (w, h) = (h, w);
            }

            _compList.GetCanonicalCells(out _, out _, out int ox, out int oy);
            foreach (HouseMarker m in list)
            {
                m.X -= ox;
                m.Y -= oy;
            }

            return list;
        }

        private static void RotateMarkersCw(List<HouseMarker> markers, int height)
        {
            foreach (HouseMarker m in markers)
            {
                int nx = height - 1 - m.Y;
                m.Y = m.X;
                m.X = nx;
                m.Kind = m.Kind switch
                {
                    HouseMarkerKind.DoorSouth => HouseMarkerKind.DoorEast,
                    HouseMarkerKind.DoorEast => HouseMarkerKind.DoorSouth,
                    _ => m.Kind
                };
            }
        }

        /// <summary>
        /// Called by RotateView before the data is rotated
        /// </summary>
        private void RotateMarkersWithView(int oldHeight)
        {
            RotateMarkersCw(_houseMarkers, oldHeight);
            _walkIssues.Clear();
        }

        private void ExportHouseScript()
        {
            if (_compList == null)
            {
                return;
            }

            Utils.ConvertStringToInt(textBox_SaveToID.Text, out int multiId, 0, Multis.MaximumMultiIndex - 1);
            bool hasDoor = _houseMarkers.Any(m => m.Kind is HouseMarkerKind.DoorSouth or HouseMarkerKind.DoorEast);
            var d = new SimpleDialog(L.T("ServUO Haus-Skript", "ServUO house script"))
                .AddText("name", L.T("Klassenname:", "Class name:"), MultiFormats.SanitizeClassName(DesignName) + "House")
                .AddText("deed", L.T("Deed-Name:", "Deed name:"), L.T("eine Urkunde für ", "a deed for ") + DesignName)
                .AddText("ns", "Namespace:", "Server.Multis.Deeds")
                .AddText("id", L.T("Multi-ID:", "Multi id:"), $"0x{multiId:X4}")
                .AddNumber("price", L.T("Preis:", "Price:"), 50000, 0, 100000000)
                .AddNumber("lock", L.T("Max. Lockdowns:", "Max lockdowns:"), 425, 0, 10000)
                .AddNumber("secure", L.T("Max. Secures:", "Max secures:"), 3, 0, 100)
                .AddCheck("wood", L.T("Holztüren (sonst Metall)", "Wooden doors (otherwise metal)"), true)
                .AddInfo((hasDoor ? string.Empty : L.T("Keine Tür markiert! Rechtsklick → Haus (ServUO) → Tür Süd/Ost hier.\n", "No door marked! Right click → House (ServUO) → south/east door here.\n")) +
                         L.T("Das Multi muss unter der ID in der multi.mul von Server und Client stehen (Datei → In Multi.mul schreiben). Einrichtung zählt nicht zur Fläche.",
                             "The multi must be stored under the id in the multi.mul of server and client (File → Write to Multi.mul). Furniture does not count for the area."));
            if (!d.ShowOk(this) || !SimpleDialog.TryParseId(d.GetText("id"), out int id))
            {
                return;
            }

            var o = new HouseScriptOptions
            {
                Name = d.GetText("name"),
                DeedName = d.GetText("deed"),
                Namespace = string.IsNullOrWhiteSpace(d.GetText("ns")) ? "Server.Multis.Deeds" : d.GetText("ns").Trim(),
                MultiId = id,
                Price = d.GetInt("price"),
                MaxLockDowns = d.GetInt("lock"),
                MaxSecures = d.GetInt("secure"),
                WoodDoors = d.GetCheck("wood")
            };

            string cls = MultiFormats.SanitizeClassName(o.Name);
            string file = AskSaveFile("C# script (*.cs)|*.cs", cls + ".cs");
            if (file == null)
            {
                return;
            }

            File.WriteAllText(file, HouseScriptExporter.Create(_compList.ToImportTiles(), GetCanonicalMarkers(), o), new UTF8Encoding(false));
            MessageBox.Show(this, L.T($"Haus-Skript gespeichert:\n{file}\n\nIm Spiel: [add {cls}{(cls.EndsWith("House") ? string.Empty : "House")}Deed",
                    $"House script saved:\n{file}\n\nIn game: [add {cls}{(cls.EndsWith("House") ? string.Empty : "House")}Deed"),
                "ServUO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Walkability check

        private void RunWalkCheck()
        {
            if (_compList == null)
            {
                return;
            }

            var tiles = _compList.RealTiles.Select(t => new ImportTile { Id = t.Id, X = t.X, Y = t.Y, Z = t.Z, Deco = t.IsDeco }).ToList();
            var starts = _houseMarkers.Where(m => m.Kind != HouseMarkerKind.Sign).Select(m => (m.X, m.Y, m.Z)).ToList();
            var checker = new WalkabilityChecker();
            checker.Run(tiles, starts);
            _walkIssues = checker.Issues;
            pictureBoxMulti.Invalidate();

            string summary = L.T(
                $"{checker.WalkableSurfaces} begehbare Flächen · {_walkIssues.Count} Hinweise" + (starts.Count > 0 ? $" · {checker.ReachableSurfaces} von Tür/Ban erreichbar" : " · für Erreichbarkeit Tür oder Ban-Location markieren"),
                $"{checker.WalkableSurfaces} walkable surfaces · {_walkIssues.Count} hints" + (starts.Count > 0 ? $" · {checker.ReachableSurfaces} reachable from door/ban" : " · mark a door or ban location for reachability"));

            _checkForm?.Close();
            _checkForm = new CheckResultForm(_walkIssues, summary, CenterOnIssue, RunWalkCheck);
            _checkForm.Show(this);
            ShowStatus(summary);
        }

        private WalkIssue _focusIssue;

        private void CenterOnIssue(WalkIssue issue)
        {
            _focusIssue = issue;
            if (_compList == null)
            {
                return;
            }

            Point p = BackgroundLayer.CellToScreen(_compList, issue.X, issue.Y, issue.Z, 0, 0);
            int viewW = (int)(pictureBoxMulti.Width / _zoom);
            int viewH = (int)(pictureBoxMulti.Height / _zoom);
            if (hScrollBar.Enabled)
            {
                hScrollBar.Value = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum, p.X - (viewW / 2)));
            }

            if (vScrollBar.Enabled)
            {
                vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, p.Y - (viewH / 2)));
            }

            pictureBoxMulti.Invalidate();
        }

        private void PaintWalkIssues(Graphics g)
        {
            if (_compList == null || _walkIssues.Count == 0)
            {
                return;
            }

            foreach (WalkIssue i in _walkIssues)
            {
                Point p = BackgroundLayer.CellToScreen(_compList, i.X, i.Y, i.Z, hScrollBar.Value, vScrollBar.Value);
                Color c = CheckResultForm.IssueColor(i.Kind);
                bool focus = i == _focusIssue;
                using var pen = new Pen(c, focus ? 4 : 2);
                if (i.Kind == WalkIssueKind.Unreachable)
                {
                    pen.DashStyle = DashStyle.Dot;
                }

                g.DrawPolygon(pen, new[]
                {
                    new Point(p.X + 22, p.Y + 2), new Point(p.X + 42, p.Y + 22),
                    new Point(p.X + 22, p.Y + 42), new Point(p.X + 2, p.Y + 22)
                });
                if (i.Kind != WalkIssueKind.Unreachable)
                {
                    g.DrawString("!", _heightFont, Brushes.White, p.X + 19, p.Y + 15);
                }
            }
        }

        #endregion

        #region Symmetry

        private void SetSymmetry(SymmetryMode mode)
        {
            _symmetry = mode;
            UpdateSymmetryButton();
            pictureBoxMulti.Invalidate();
        }

        private void UpdateSymmetryButton()
        {
            if (_tbSymmetry == null)
            {
                return;
            }

            _tbSymmetry.Text = _symmetry switch
            {
                SymmetryMode.X => L.T("Symmetrie: X", "Symmetry: X"),
                SymmetryMode.Y => L.T("Symmetrie: Y", "Symmetry: Y"),
                SymmetryMode.Both => L.T("Symmetrie: 4-fach", "Symmetry: 4-way"),
                _ => L.T("Symmetrie", "Symmetry")
            };
            _tbSymmetry.ForeColor = _symmetry == SymmetryMode.Off ? SystemColors.ControlText : Color.DarkMagenta;
        }

        private static Dictionary<ushort, ushort> RoofSwap(bool alongX)
        {
            var map = new Dictionary<ushort, ushort>();
            foreach (HouseStyle s in StyleKnowledge.Instance.Styles)
            {
                int a = alongX ? s.RoofWest : s.RoofNorth;
                int b = alongX ? s.RoofEast : s.RoofSouth;
                if (a > 0 && b > 0 && a != b)
                {
                    map.TryAdd((ushort)a, (ushort)b);
                    map.TryAdd((ushort)b, (ushort)a);
                }
            }

            return map;
        }

        /// <summary>
        /// Mirrors one tile. Walls lie on the cell border, so they are moved by one more cell.
        /// </summary>
        private ImportTile MirrorTile(ImportTile t, bool alongX, Dictionary<ushort, ushort> roofSwap)
        {
            ImportTile c = t.Clone();
            TileRole role = StyleKnowledge.Instance.GetRole(t.Id);
            if (alongX)
            {
                bool edge = role is TileRole.WallY or TileRole.WindowY or TileRole.Corner or TileRole.Post;
                c.X = edge ? _compList.Width - t.X : _compList.Width - 1 - t.X;
            }
            else
            {
                bool edge = role is TileRole.WallX or TileRole.WindowX or TileRole.Corner or TileRole.Post;
                c.Y = edge ? _compList.Height - t.Y : _compList.Height - 1 - t.Y;
            }

            if (roofSwap.TryGetValue(c.Id, out ushort n))
            {
                c.Id = n;
            }

            return c;
        }

        /// <summary>
        /// Adds the mirrored copies (cell coordinates) when the symmetry mode is on
        /// </summary>
        private List<ImportTile> WithSymmetry(List<ImportTile> tiles)
        {
            if (_symmetry == SymmetryMode.Off || _compList == null || tiles.Count == 0)
            {
                return tiles;
            }

            var result = new List<ImportTile>(tiles);
            if (_symmetry is SymmetryMode.X or SymmetryMode.Both)
            {
                Dictionary<ushort, ushort> swap = RoofSwap(true);
                result.AddRange(result.ToList().Select(t => MirrorTile(t, true, swap)));
            }

            if (_symmetry is SymmetryMode.Y or SymmetryMode.Both)
            {
                Dictionary<ushort, ushort> swap = RoofSwap(false);
                result.AddRange(result.ToList().Select(t => MirrorTile(t, false, swap)));
            }

            // no doubles on the axis
            return result.GroupBy(t => (t.Id, t.X, t.Y, t.Z)).Select(g => g.First()).ToList();
        }

        /// <summary>
        /// Remove tool with symmetry: removes the mirrored tiles too
        /// </summary>
        private void RemoveWithSymmetry(MultiTile tile)
        {
            if (_symmetry == SymmetryMode.Off)
            {
                _compList.TileRemove(tile);
                return;
            }

            var probe = new ImportTile { Id = tile.Id, X = tile.X, Y = tile.Y, Z = tile.Z };
            List<ImportTile> targets = WithSymmetry(new List<ImportTile> { probe });
            var remove = new List<MultiTile>();
            foreach (ImportTile t in targets)
            {
                MultiTile m = _compList.GetMultiTileLitAtCoordinate(t.X, t.Y).FirstOrDefault(o => o.Z == t.Z && (o.Id == t.Id || o.Id == tile.Id));
                if (m != null && !remove.Contains(m))
                {
                    remove.Add(m);
                }
            }

            _compList.TilesRemove(remove, "Remove (symmetry)");
        }

        private void PaintSymmetryAxes(Graphics g)
        {
            if (_symmetry == SymmetryMode.Off || _compList == null)
            {
                return;
            }

            int z = BTN_Floor.Checked ? DrawFloorZ : 0;

            PointF Corner(double cx, double cy)
            {
                float px = (float)((cx - cy) * 22) + MultiEditorComponentList.GapXMod - _compList.XMin - hScrollBar.Value;
                float py = (float)((cx + cy) * 22) - (z * 4) - 44 + MultiEditorComponentList.GapYMod - _compList.YMin - vScrollBar.Value;
                return new PointF(px, py);
            }

            using var pen = new Pen(Color.FromArgb(200, 255, 0, 255), 2) { DashStyle = DashStyle.DashDot };
            if (_symmetry is SymmetryMode.X or SymmetryMode.Both)
            {
                double cx = _compList.Width / 2.0;
                g.DrawLine(pen, Corner(cx, 0), Corner(cx, _compList.Height));
            }

            if (_symmetry is SymmetryMode.Y or SymmetryMode.Both)
            {
                double cy = _compList.Height / 2.0;
                g.DrawLine(pen, Corner(0, cy), Corner(_compList.Width, cy));
            }
        }

        #endregion

        #region Hue picker / material list / browser

        private void PickDrawHue()
        {
            using var f = new HuePickerForm(_drawHue, _drawTile.Id);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                _hueBox.Value = Math.Max(_hueBox.Minimum, Math.Min(_hueBox.Maximum, f.SelectedHue));
            }
        }

        /// <summary>
        /// Hue picker for tiles (context menu)
        /// </summary>
        private int? PickHue(int current, ushort tileId)
        {
            using var f = new HuePickerForm(current, tileId);
            return f.ShowDialog(this) == DialogResult.OK ? f.SelectedHue : (int?)null;
        }

        private void OpenMaterialList()
        {
            if (_compList == null)
            {
                return;
            }

            var f = new MaterialListForm(_compList.ToImportTiles(), DesignName, id =>
            {
                _selection.Clear();
                foreach (MultiTile t in _compList.RealTiles.Where(t => t.Id == id))
                {
                    _selection.Add(t);
                }

                UpdateInfo();
                pictureBoxMulti.Invalidate();
            });
            f.Show(this);
        }

        private Dictionary<int, string> GetMultiNames()
        {
            var names = new Dictionary<int, string>();
            if (treeViewMultiList.Nodes.Count == 0)
            {
                return names;
            }

            foreach (TreeNode n in treeViewMultiList.Nodes[0].Nodes)
            {
                if (n.Tag is int id)
                {
                    int p = n.Text.IndexOf(')');
                    names[id] = p >= 0 && p + 1 < n.Text.Length ? n.Text.Substring(p + 1).Trim() : string.Empty;
                }
            }

            return names;
        }

        private void OpenMultiBrowser()
        {
            if (_browserForm == null || _browserForm.IsDisposed)
            {
                _browserForm = new MultiBrowserForm(GetMultiNames(), OpenMultiFromMul);
            }

            _browserForm.Show(this);
            _browserForm.BringToFront();
        }

        private void OpenMultiFromMul(int id)
        {
            MultiComponentList list = Multis.GetComponents(id);
            if (list == MultiComponentList.Empty)
            {
                return;
            }

            if (_compList != null && _compList.CanUndo &&
                MessageBox.Show(this, L.T($"Multi 0x{id:X} öffnen? Ungespeicherte Änderungen gehen verloren.", $"Open multi 0x{id:X}? Unsaved changes are lost."),
                    "Multi", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _compList = new MultiEditorComponentList(list, this);
            GetMultiNames().TryGetValue(id, out string name);
            AfterMultiLoaded(string.IsNullOrEmpty(name) ? $"Multi_{id:X}" : name, id);
        }

        #endregion
    }
}

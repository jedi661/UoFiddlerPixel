/***************************************************************************
 *
 *  * Dragon Map Studio – cave building helpers: quick options with a short guide for the tools "Höhle" and "Eingang",
 *  *  a picker for cave floors (land) and cave items (statics: stalagmites, rocks …), and the view switches
 *  *  "Oberfläche aus" (see and build below the map), height scale, tile info and coordinate lines.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private Form _cavePicker;
        private ToolStripMenuItem _viewSurface;

        private static readonly string CaveGuide =
            "HÖHLE (Werkzeug 'Höhle')\n" +
            "1. Am Fuß des Berges anfangen – dort, wo der Gang beginnen soll – und in den Berg hineinziehen.\n" +
            "2. Jeder Tile unter dem Pinsel wird Höhlen-Boden (die Boden-Tiles unten, zufällig gemischt) und bekommt die Höhe des ERSTEN Klicks " +
            "(oder 'Boden auf Höhe =' mit der Zahl daneben).\n" +
            "3. 'Rand zu Wänden' an: Berg-Tiles am Rand des Pinsels werden auf Boden + 'Wand-Höhe' angehoben – der Client zeichnet dort die Fels-Textur als Wand.\n" +
            "4. Pinselgröße = Breite des Ganges. Ein Zug = ein Rückgängig-Schritt. Mehrere Züge = verzweigte Höhle.\n" +
            "Tipp: zuerst mit 'Eingang' oder dem 'Höhlen-Stempel' den Eingang machen, dann mit 'Höhle' dahinter weitergraben.\n\n" +
            "EINGANG (Werkzeug 'Eingang')\n" +
            "1. Auf den Boden direkt am Fuß des Berges klicken (die Höhe dieses Klicks ist der Boden vor dem Eingang) und 2–3 Tiles in den Hang ziehen (mindestens 2×2).\n" +
            "2. Die überstrichenen Tiles werden der 'Mund': 01DB (schwarz) oder 0002 (unsichtbar), 'Mund-Höhe' über dem Boden, als steile Wand in den Hang.\n" +
            "3. Der Rand zum Berg bekommt automatisch die Eingangs-Tiles 01D3–01DA (wie Dragon cave-enti2mountain.txt).\n" +
            "4. Sichtbar ist ein Eingang im Client nur, wenn er nach Süden oder Osten zeigt (zur Kamera hin).\n" +
            "Der 'Höhlen-Stempel' macht dasselbe mit einem Klick fertig (Bogen, Vorderkante, Weg davor) – deshalb ist er am einfachsten.\n\n" +
            "UNTER DER KARTE\n" +
            "'⬇ Unter der Karte' + 'Oberfläche aus' (Ansicht): das Land wird nur als Umriss gezeigt, Statics auf dem Boden ausgeblendet – so sieht man " +
            "Keller, Kanalisation, Höhlen darunter und baut dort. Die Höhen-Skala rechts zeigt Boden, Bau-Ebene und Keller-Ebene.";

        /// <summary>The view helpers (menu "Ansicht" in the CGI bar) – once, from BuildToolOptions.</summary>
        private void InitViewExtras()
        {
            var view = new ToolStripDropDownButton("👁 " + L.T("Ansicht")) { Alignment = ToolStripItemAlignment.Right, ToolTipText = L.T("Hilfen der Ansicht: Oberfläche aus (unter der Karte), Höhen-Skala, Tile-Info, Linien") };
            _viewSurface = new ToolStripMenuItem(L.T("Oberfläche aus (Land nur als Umriss, Boden-Statics weg)")) { CheckOnClick = true };
            _viewSurface.Click += (s, e) => SetSurfaceHidden(_viewSurface.Checked);
            var scale = new ToolStripMenuItem(L.T("Höhen-Skala rechts")) { CheckOnClick = true, Checked = _settings.ViewHeightScale };
            scale.Click += (s, e) =>
            {
                _settings.ViewHeightScale = isoView.HeightScale = scale.Checked;
                isoView.Invalidate();
            };
            var info = new ToolStripMenuItem(L.T("Tile-Info am Mauszeiger")) { CheckOnClick = true, Checked = _settings.ViewTileInfo };
            info.Click += (s, e) =>
            {
                _settings.ViewTileInfo = isoView.TileInfo = info.Checked;
                isoView.Invalidate();
            };
            var lines = new ToolStripMenuItem(L.T("Koordinaten-Linien"));
            foreach (int step in new[] { 0, 1, 4, 8, 16, 32, 64 })
            {
                int st = step;
                var item = new ToolStripMenuItem(step == 0 ? L.T("aus") : $"{step} Tiles") { Checked = _settings.ViewCoordLines == step };
                item.Click += (s, e) =>
                {
                    _settings.ViewCoordLines = isoView.CoordLines = st;
                    foreach (ToolStripMenuItem other in lines.DropDownItems)
                    {
                        other.Checked = other == item;
                    }
                    isoView.Invalidate();
                };
                lines.DropDownItems.Add(item);
            }
            var picker = new ToolStripMenuItem(L.T("Höhlen-Boden und -Items …"));
            picker.Click += (s, e) => OpenCavePicker();
            var guide = new ToolStripMenuItem(L.T("Anleitung Höhle / Eingang …"));
            guide.Click += (s, e) => ShowCaveGuide();
            view.DropDownItems.AddRange(new ToolStripItem[] { _viewSurface, scale, info, lines, new ToolStripSeparator(), picker, guide });
            toolStripCgi.Items.Add(view);
            isoView.HeightScale = _settings.ViewHeightScale;
            isoView.TileInfo = _settings.ViewTileInfo;
            isoView.CoordLines = _settings.ViewCoordLines;
            InitCellarBar();
            isoView.CaveEntrancePresets = () => CaveEntrancePresets.All;
        }

        private ToolStripButton _cellarTextures;
        private ToolStripButton _cellarItems;

        private ObjectBrowserHost ObjectHost() => new ObjectBrowserHost
        {
            Status = SetStatus,
            UseItem = id =>
            {
                isoView.PlaceId = (ushort)id;
                SelectCgiTool(IsoTool.Place);
                SetStatus(L.T("Static setzen") + $": 0x{id:X4} {UoArtCache.ItemName(id)}" + (isoView.LayerMode ? " – " + L.T("auf der Keller-Ebene") + $" z {isoView.LayerZ}" : string.Empty));
            },
            UseLand = id =>
            {
                txtCgiLandId.Text = id.ToString("x4");
                SelectCgiTool(IsoTool.LandSet);
                SetStatus(L.T("Land-Pinsel") + $": 0x{id:X4} {UoArtCache.LandName(id)}");
            },
            UseCaveFloor = (id, add) =>
            {
                if (_caveForm != null && !_caveForm.IsDisposed)
                {
                    _caveForm.AddTile(id);
                    return;
                }
                IEnumerable<ushort> ids = add ? isoView.CaveFloorIds.Append((ushort)id).Distinct() : new[] { (ushort)id };
                txtCgiCaveFloor.Text = string.Join(" ", ids.Select(i => i.ToString("x4")));
                SetStatus(L.T("Höhlen-Boden") + ": " + txtCgiCaveFloor.Text);
            }
        };

        /// <summary>
        /// Own bar for building below the map (above the view, also in the region window): cellar level on / off with
        /// its height, textures and surface items on / off, grid of the level, object list, walkable NoDraw passage.
        /// </summary>
        private void InitCellarBar()
        {
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.FromArgb(237, 233, 254) };
            var layer = new ToolStripButton("⬇ " + L.T("Keller-Ebene")) { CheckOnClick = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ToolTipText = L.T("Flache Bau-Ebene unter der Karte ein / aus: die Maus trifft die Felder DIESER Ebene, Static setzen, Wand, Treppe, Multi und Muster bauen auf ihr, alles unter dem Boden wird gezeigt") };
            var z = new NumericUpDown { Minimum = -128, Maximum = 127, Value = Math.Clamp(_settings.ViewLayerZ, -128, 127), Width = 55, Increment = 5 };
            var zHost = new ToolStripControlHost(z) { ToolTipText = L.T("Höhe der Keller-Ebene (absolutes Z)") };
            var fromGround = new ToolStripButton(L.T("= Boden − 20")) { ToolTipText = L.T("Ebene 20 unter den Boden an der letzten Mausposition legen (eine Etage tiefer)") };
            var down = new ToolStripButton("▼ 20") { ToolTipText = L.T("Eine Etage (20) tiefer") };
            var up = new ToolStripButton("▲ 20") { ToolTipText = L.T("Eine Etage (20) höher") };
            var grid = new ToolStripButton(L.T("Gitter der Ebene")) { CheckOnClick = true, Checked = true };
            _cellarTextures = new ToolStripButton("▦ " + L.T("Texturen")) { CheckOnClick = true, Checked = !isoView.LandHidden, ToolTipText = L.T("Boden-Texturen der Karte ein / aus (aus = nur Umriss – die Höhle darunter ist zu sehen)") };
            _cellarItems = new ToolStripButton("🏠 " + L.T("Oberflächen-Items")) { CheckOnClick = true, Checked = !isoView.SurfaceStaticsHidden, ToolTipText = L.T("Statics auf / über dem Boden (Häuser, Bäume über der Höhle) ein / aus") };
            var objects = new ToolStripButton("🔎 " + L.T("Objekte & Texturen …")) { ToolTipText = L.T("Liste aller Items und Land-Tiles mit Suche nach Name und ID, Schnellfilter Höhle / Felsen / Stalagmiten / NoDraw") };
            var caveItems = new ToolStripButton("🪨 " + L.T("Höhlen-Items")) { ToolTipText = L.T("Objekt-Liste gleich mit dem Filter Höhle") };
            var passage = new ToolStripButton(L.T("Durchgang (NoDraw 0002)")) { ToolTipText = L.T("Land-Pinsel mit 0002: unsichtbar und begehbar – Loch / Durchgang in der Karte zur Höhle darunter") };
            var guide = new ToolStripButton("❓");
            guide.ToolTipText = L.T("Anleitung Höhle / Eingang / Keller");

            void ShowLayerItems()
            {
                zHost.Visible = fromGround.Visible = down.Visible = up.Visible = grid.Visible = layer.Checked;
            }
            layer.Click += (s, e) =>
            {
                isoView.LayerZ = (int)z.Value;
                isoView.LayerMode = layer.Checked;
                ShowLayerItems();
                if (layer.Checked && isoView.Tool != IsoTool.Place && isoView.Tool != IsoTool.Wall && isoView.Tool != IsoTool.Stairs && isoView.Tool != IsoTool.Motif &&
                    isoView.Tool != IsoTool.Multi && isoView.Tool != IsoTool.Select && isoView.Tool != IsoTool.Eraser && isoView.Tool != IsoTool.Delete)
                {
                    SelectCgiTool(IsoTool.Place);
                }
                SetStatus(layer.Checked
                    ? L.T("Keller-Ebene an") + $" (z {z.Value}): " + L.T("die Maus trifft die Felder der Ebene – Static setzen, Wand, Treppe, Muster, Multi bauen dort. '▦ Texturen' aus = die Karte darüber nur als Umriss.")
                    : L.T("Keller-Ebene aus – wieder auf der Karte."));
            };
            z.ValueChanged += (s, e) =>
            {
                isoView.LayerZ = (int)z.Value;
                _settings.ViewLayerZ = (int)z.Value;
                SaveLayoutSoon();
            };
            fromGround.Click += (s, e) =>
            {
                Point t = isoView.LastTile;
                if (_cgiMap != null && _cgiMap.InBounds(t.X, t.Y))
                {
                    z.Value = Math.Clamp(_cgiMap.LandZ(t.X, t.Y) - 20, -128, 127);
                }
            };
            down.Click += (s, e) => z.Value = Math.Max(z.Minimum, z.Value - 20);
            up.Click += (s, e) => z.Value = Math.Min(z.Maximum, z.Value + 20);
            grid.Click += (s, e) =>
            {
                isoView.LayerGrid = grid.Checked;
                isoView.Invalidate();
            };
            _cellarTextures.Click += (s, e) =>
            {
                isoView.LandHidden = !_cellarTextures.Checked;
                SyncSurfaceItems();
            };
            _cellarItems.Click += (s, e) =>
            {
                isoView.SurfaceStaticsHidden = !_cellarItems.Checked;
                SyncSurfaceItems();
            };
            objects.Click += (s, e) => ObjectBrowserForm.ShowShared(this, ObjectHost());
            caveItems.Click += (s, e) => ObjectBrowserForm.ShowShared(this, ObjectHost(), false, "Höhle");
            passage.Click += (s, e) =>
            {
                txtCgiLandId.Text = "0002";
                SelectCgiTool(IsoTool.LandSet);
                SetStatus(L.T("Durchgang: Land 0002 (NoDraw) malen – unsichtbar und begehbar. Darunter auf der Keller-Ebene die Höhle bauen; für einen sichtbaren Eingang lieber den Höhlen-Stempel."));
            };
            guide.Click += (s, e) => ShowCaveGuide();
            bar.Items.AddRange(new ToolStripItem[] { layer, zHost, fromGround, down, up, grid, new ToolStripSeparator(), _cellarTextures, _cellarItems, new ToolStripSeparator(), objects, caveItems, passage, guide });
            ShowLayerItems();
            tabCgi.Controls.Add(bar);
            tabCgi.Controls.SetChildIndex(bar, tabCgi.Controls.GetChildIndex(toolStripCgiLand));
        }

        /// <summary>The buttons / menu show the state of the view.</summary>
        private void SyncSurfaceItems()
        {
            if (_cellarTextures != null)
            {
                _cellarTextures.Checked = !isoView.LandHidden;
                _cellarItems.Checked = !isoView.SurfaceStaticsHidden;
            }
            if (_viewSurface != null)
            {
                _viewSurface.Checked = isoView.SurfaceHidden;
            }
        }

        private void SetSurfaceHidden(bool on)
        {
            isoView.SurfaceHidden = on;
            SyncSurfaceItems();
            SetStatus(on
                ? L.T("Oberfläche aus: das Land ist nur als Umriss zu sehen, Statics auf dem Boden sind ausgeblendet – darunter bauen (mit '⬇ Unter der Karte'). Die Karte selbst bleibt unverändert.")
                : L.T("Oberfläche wieder an."));
        }

        private void ShowCaveGuide() => MessageBox.Show(this, L.T(CaveGuide), L.T("Anleitung Höhle / Eingang"), MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>A number field in the quick bar that drives a field of the side panel (both stay the same).</summary>
        private void QuickMirror(string label, NumericUpDown source, string tip)
        {
            AddQuick(new ToolStripLabel(L.T(label)));
            var n = new NumericUpDown { Minimum = source.Minimum, Maximum = source.Maximum, Value = source.Value, Width = 55, Increment = source.Increment };
            n.ValueChanged += (s, e) => source.Value = n.Value;
            AddQuick(new ToolStripControlHost(n) { ToolTipText = L.T(tip) });
        }

        private void QuickMirror(string label, CheckBox source, string tip)
        {
            var b = new ToolStripButton(L.T(label)) { CheckOnClick = true, Checked = source.Checked, ToolTipText = L.T(tip) };
            b.CheckedChanged += (s, e) => source.Checked = b.Checked;
            AddQuick(b);
        }

        /// <summary>Quick options of the tools "Höhle" and "Eingang" (in UpdateQuickOptions).</summary>
        private CaveToolForm _caveForm;
        private IsoTool? _caveAutoFor;

        /// <summary>The window of the cave tools (texture with pictures, saved entrances) – opens with Höhle / Eingang.</summary>
        private void OpenCaveWindow(IsoTool t)
        {
            if (_caveForm != null && !_caveForm.IsDisposed)
            {
                _caveForm.ShowTool(t);
                if (!_caveForm.Visible)
                {
                    _caveForm.Show(this);
                }
                return;
            }
            var host = new CaveToolHost
            {
                View = isoView,
                Settings = _settings,
                SaveSoon = SaveLayoutSoon,
                SetFloorIds = ids => txtCgiCaveFloor.Text = ids,
                WallHeight = numCgiCaveWall,
                RaiseWalls = chkCgiCaveWalls,
                UseFixedHeight = chkCgiCaveTarget,
                FixedHeight = numCgiTarget,
                OpenObjects = () => ObjectBrowserForm.ShowShared(this, ObjectHost(), true, "Höhle"),
                SelectTool = tool =>
                {
                    if (isoView.Tool != tool)
                    {
                        SelectCgiTool(tool);
                    }
                },
                ShowSettings = ShowSettingsWindow,
                Status = SetStatus,
                Guide = ShowCaveGuide
            };
            _caveForm = new CaveToolForm(host) { Owner = this };
            _caveForm.FormClosed += (s, e) => _caveForm = null;
            _caveForm.ShowTool(t);
            _caveForm.Show(this);
        }

        private void CaveQuickItems(IsoTool t)
        {
            AddQuick(new ToolStripSeparator());
            Cmd("🪟 Höhlen-Fenster", "Fenster mit den Boden-Texturen (Bilder, fest / zufällig, eigene dazu) und den Eingängen (Standard, gespeicherte, Vorschau)", () => OpenCaveWindow(t));
            if (_settings.CaveWindowAuto && _caveAutoFor != t)
            {
                _caveAutoFor = t;
                BeginInvoke(new Action(() => OpenCaveWindow(t)));
            }
            if (t == IsoTool.CaveEntrance)
            {
                var click = new ToolStripButton(L.T("Klick = ganzer Eingang")) { CheckOnClick = true, Checked = isoView.CaveEntranceClick, ToolTipText = L.T("An = ein Klick setzt den gewählten Eingang (Mausrad = anderer, Umschalt+Mausrad = Höhe). Aus = Mund von Hand malen.") };
                click.CheckedChanged += (s, e) => isoView.CaveEntranceClick = click.Checked;
                AddQuick(click);
            }
            if (t == IsoTool.Cave)
            {
                var floor = new ToolStripButton(L.T("Boden") + ": " + string.Join(" ", isoView.CaveFloorIds.Take(3).Select(i => i.ToString("x4"))) + (isoView.CaveFloorIds.Length > 3 ? " …" : string.Empty))
                {
                    ToolTipText = L.T("Höhlen-Boden wählen (mit Bild) – Klick = dieser Boden, Strg+Klick = dazu mischen")
                };
                floor.Click += (s, e) => OpenCavePicker();
                AddQuick(floor);
                QuickMirror("Wand-Höhe", numCgiCaveWall, "So hoch über dem Höhlen-Boden wird der Rand (Berg) angehoben – das ist die Fels-Wand");
                QuickMirror("Rand → Wand", chkCgiCaveWalls, "Berg-Tiles am Rand des Pinsels werden zur Wand angehoben");
                QuickMirror("Boden auf Höhe =", chkCgiCaveTarget, "Höhlen-Boden auf die Zahl daneben statt auf die Höhe des ersten Klicks");
                QuickMirror(string.Empty, numCgiTarget, "Höhe des Höhlen-Bodens (wenn 'Boden auf Höhe =' an ist)");
            }
            else
            {
                QuickMirror("Mund-Höhe", numCgiCaveMouth, "So hoch über dem Boden vor dem Eingang liegt der Mund (die schwarze / unsichtbare Fläche im Hang)");
                QuickMirror("unsichtbar (0002)", chkCgiCaveInvisible, "Mund unsichtbar (0002) statt schwarz (01DB)");
                QuickMirror("Vorderkante unsichtbar", chkCgiCaveFront, "Die vorderste Reihe des Mundes unsichtbar (0002), wie im Original");
            }
            Cmd("Oberfläche aus / an", "Land nur als Umriss, Boden-Statics ausblenden – sehen, was darunter liegt", () => SetSurfaceHidden(!isoView.SurfaceHidden));
            Cmd("❓ Anleitung", "Wie 'Höhle' und 'Eingang' genau funktionieren", ShowCaveGuide);
        }

        // ================================================================== cave floor / item picker

        private static List<int> _caveLand;
        private static List<int> _caveItems;

        private static List<int> CaveLand()
        {
            if (_caveLand == null)
            {
                var list = new List<int> { 0x0244, 0x0245, 0x0246, 0x0247, 0x0248, 0x0249 };
                for (int id = 0; id <= UoArtCache.MaxLandId; id++)
                {
                    string n = UoArtCache.LandName(id).ToLowerInvariant();
                    if (n.Contains("cave") || n.Contains("dungeon") || n.Contains("cavern"))
                    {
                        list.Add(id);
                    }
                }
                _caveLand = list.Distinct().Where(UoArtCache.IsValidLand).ToList();
            }
            return _caveLand;
        }

        private static List<int> CaveItems()
        {
            if (_caveItems == null)
            {
                var list = new List<int>();
                int max = Math.Min(UoArtCache.MaxItemId, 0xFFFF);
                for (int id = 1; id <= max; id++)
                {
                    string n = UoArtCache.ItemName(id).ToLowerInvariant();
                    if (n.Contains("stalag") || n.Contains("cave") || n.Contains("rock") || n.Contains("stalactite") || n.Contains("dungeon") || n.Contains("rubble") ||
                        n.Contains("boulder"))
                    {
                        list.Add(id);
                    }
                    if (list.Count > 3000)
                    {
                        break;
                    }
                }
                _caveItems = list;
            }
            return _caveItems;
        }

        /// <summary>
        /// Picker window: cave floors (land, Klick = floor of the tool "Höhle", Strg+Klick = mix in) and cave items
        /// (statics, Klick = place with "Static setzen").
        /// </summary>
        private void OpenCavePicker()
        {
            if (_cavePicker != null && !_cavePicker.IsDisposed)
            {
                _cavePicker.Activate();
                return;
            }
            var f = new Form
            {
                Text = L.T("Höhlen-Boden und -Items"), Owner = this, ShowInTaskbar = false, FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent, Size = new Size(760, 640), Font = new Font("Segoe UI", 9f)
            };
            var catalog = new ArtCatalog { Dock = DockStyle.Fill };
            var floorLabel = new Label { Dock = DockStyle.Top, Height = 40, Padding = new Padding(6), BackColor = Color.FromArgb(241, 245, 249) };
            void ShowFloor() => floorLabel.Text = L.T("Höhlen-Boden jetzt") + ": " + string.Join(" ", isoView.CaveFloorIds.Select(i => i.ToString("x4"))) + "   ·   " +
                                                 L.T("Land: Klick = dieser Boden, Strg+Klick = dazu mischen · Items: Klick = mit 'Static setzen' platzieren");
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            var land = new ToolStripButton(L.T("Höhlen-Boden (Land)"));
            land.Click += (s, e) => catalog.ShowIds(ArtKind.Land, CaveLand(), L.T("Höhlen-Boden"));
            var items = new ToolStripButton(L.T("Höhlen-Items (Statics)"));
            items.Click += (s, e) =>
            {
                UseWaitCursor = true;
                try
                {
                    catalog.ShowIds(ArtKind.Static, CaveItems(), L.T("Höhlen-Items"));
                }
                finally
                {
                    UseWaitCursor = false;
                }
            };
            var reset = new ToolStripButton(L.T("Boden zurücksetzen (0245–0249)"));
            reset.Click += (s, e) =>
            {
                txtCgiCaveFloor.Text = "0245 0246 0247 0248 0249";
                ShowFloor();
            };
            var guide = new ToolStripButton("❓ " + L.T("Anleitung"));
            guide.Click += (s, e) => ShowCaveGuide();
            bar.Items.AddRange(new ToolStripItem[] { land, items, new ToolStripSeparator(), reset, guide });
            catalog.Picked += (s, e) =>
            {
                if (e.Kind == ArtKind.Land)
                {
                    bool add = (ModifierKeys & (Keys.Control | Keys.Shift)) != 0;
                    IEnumerable<ushort> ids = add ? isoView.CaveFloorIds.Append((ushort)e.Id).Distinct() : new[] { (ushort)e.Id };
                    txtCgiCaveFloor.Text = string.Join(" ", ids.Select(i => i.ToString("x4")));
                    ShowFloor();
                    if (isoView.Tool != IsoTool.Cave && isoView.Tool != IsoTool.Dungeon)
                    {
                        SelectCgiTool(IsoTool.Cave);
                    }
                    SetStatus(L.T("Höhlen-Boden") + ": " + txtCgiCaveFloor.Text + " – " + L.T("mit 'Höhle' in den Berg ziehen."));
                }
                else if (e.Kind == ArtKind.Static)
                {
                    isoView.PlaceId = (ushort)e.Id;
                    SelectCgiTool(IsoTool.Place);
                    SetStatus(L.T("Höhlen-Item") + $" 0x{e.Id:X4} {UoArtCache.ItemName(e.Id)} – " + L.T("mit 'Static setzen' klicken (unter der Karte: '⬇ Unter der Karte' an)."));
                }
            };
            f.Controls.Add(catalog);
            f.Controls.Add(floorLabel);
            f.Controls.Add(bar);
            f.FormClosed += (s, e) => _cavePicker = null;
            _cavePicker = f;
            ShowFloor();
            f.Show(this);
            catalog.ShowIds(ArtKind.Land, CaveLand(), L.T("Höhlen-Boden"));
        }
    }
}

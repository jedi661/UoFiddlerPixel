/***************************************************************************
 *
 *  * Dragon Map Studio – another map (e.g. the original map0 / Felucca) in its own CGI window
 *  *
 *  *  Open map#.mul / .uop (+ statics), jump to coordinates, mark a dungeon with "Bereich" and copy it into the
 *  *  workshop (or save it as a piece by category). The other map is only read, never changed.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Controls;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Dialogs
{
    public sealed class ForeignMapForm : Form
    {
        private static MapData _cache;
        private static string _cachePath;

        private readonly IsoMapView _view = new IsoMapView { Dock = DockStyle.Fill, Tool = IsoTool.Area };
        private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White };
        private readonly NumericUpDown _x = new NumericUpDown { Maximum = 8000, Width = 65 };
        private readonly NumericUpDown _y = new NumericUpDown { Maximum = 8000, Width = 65 };
        private MapData _map;

        public ForeignMapForm(Action<IsoMapView> configure)
        {
            Text = L.T("Fremde Karte") + " – " + L.T("Bereich wählen und in die Werkstatt kopieren");
            Size = new Size(1200, 820);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            configure?.Invoke(_view);
            _view.BrushSettings = new CgiBrushSettings();
            _view.UnderView = false;
            _view.MaxZ = 127;
            _view.Tool = IsoTool.Area;
            _view.Message += (s, t) => Say(t);
            var bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            var open = new ToolStripButton(L.T("Karte öffnen …"));
            open.Click += (s, e) => OpenMap();
            var jump = new ToolStripButton(L.T("Springen"));
            jump.Click += (s, e) => _view.CenterOn((double)_x.Value, (double)_y.Value);
            var area = new ToolStripButton(L.T("Bereich"));
            area.Click += (s, e) => _view.Tool = IsoTool.Area;
            var sel = new ToolStripButton(L.T("Auswahl"));
            sel.Click += (s, e) => _view.Tool = IsoTool.Select;
            var zin = new ToolStripButton("Zoom +");
            zin.Click += (s, e) => _view.ZoomStep(1);
            var zout = new ToolStripButton("Zoom −");
            zout.Click += (s, e) => _view.ZoomStep(-1);
            var grid = new ToolStripButton(L.T("Gitter")) { CheckOnClick = true };
            grid.Click += (s, e) =>
            {
                _view.ShowGrid = grid.Checked;
                _view.Refresh(true);
            };
            var statics = new ToolStripButton(L.T("Statics")) { CheckOnClick = true, Checked = true };
            statics.Click += (s, e) =>
            {
                _view.ShowStatics = statics.Checked;
                _view.Refresh(true);
            };
            var take = new ToolStripButton("✔ " + L.T("Bereich in die Werkstatt")) { Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            take.Click += (s, e) => Take(false);
            var save = new ToolStripButton(L.T("Bereich als Vorlage speichern …"));
            save.Click += (s, e) => Take(true);
            bar.Items.AddRange(new ToolStripItem[]
            {
                open, new ToolStripSeparator(), new ToolStripLabel("X"), new ToolStripControlHost(_x), new ToolStripLabel("Y"), new ToolStripControlHost(_y), jump,
                new ToolStripSeparator(), area, sel, zin, zout, grid, statics, new ToolStripSeparator(), take, save
            });
            Controls.Add(_view);
            Controls.Add(bar);
            Controls.Add(_status);
            Say(L.T("'Karte öffnen' wählt map#.mul oder map#LegacyMUL.uop (statics#.mul daneben). Dann zum Dungeon springen und mit 'Bereich' markieren."));
            if (_cache != null)
            {
                Show(_cache);
            }
        }

        /// <summary>The marked area was taken (copy into the workshop).</summary>
        public event EventHandler<WorkshopPiece> Taken;

        private void Say(string text) => _status.Text = "  " + text;

        private new void Show(MapData map)
        {
            _map = map;
            _view.SetMap(map);
            _x.Maximum = map.Width - 1;
            _y.Maximum = map.Height - 1;
            Text = L.T("Fremde Karte") + $": {Path.GetFileName(map.Source)} ({map.Width}×{map.Height})";
            Say(L.T("Geladen") + $": {map.Source} – {map.StaticCount:N0} Statics. " + L.T("Zum Dungeon springen (X / Y), mit 'Bereich' markieren, dann '✔ Bereich in die Werkstatt'."));
        }

        private async void OpenMap()
        {
            using var dlg = new OpenFileDialog { Filter = "UO map (map*.mul;map*.uop)|map*.mul;map*.uop|*.*|*.*", Title = L.T("Fremde Karte öffnen") };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string path = dlg.FileName;
            if (_cache != null && _cachePath == path)
            {
                Show(_cache);
                return;
            }
            int num = MulMapIO.GuessMapNumber(path);
            MapSizePreset size = MulMapIO.GuessSize(path, 0, 0, num);
            if (size == null)
            {
                MessageBox.Show(this, L.T("Die Größe der Karte ist unbekannt."), Text);
                return;
            }
            Say(L.T("Lade") + $" {Path.GetFileName(path)} ({size.Width}×{size.Height}) …");
            UseWaitCursor = true;
            try
            {
                MapData m = await Task.Run(() => MapData.LoadFiles(path, size.Width, size.Height, num, out _));
                _cache = m;
                _cachePath = path;
                Show(m);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, L.T("Laden fehlgeschlagen:") + Environment.NewLine + ex.Message, Text);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void Take(bool save)
        {
            if (_map == null || _view.Area.IsEmpty)
            {
                Say(L.T("Erst die Karte öffnen und einen Bereich markieren."));
                return;
            }
            Rectangle r = _view.Area;
            if (r.Width * r.Height > 512 * 512)
            {
                Say(L.T("Der Bereich ist zu groß (höchstens 512×512)."));
                return;
            }
            WorkshopPiece p = WorkshopPiece.From(_map, r, $"{Path.GetFileNameWithoutExtension(_map.Source)} {r.X},{r.Y}", "Fremde Karten");
            if (save)
            {
                string cat = Microsoft.VisualBasic.Interaction.InputBox(L.T("Kategorie (Ordner):"), Text, "Dungeon");
                if (string.IsNullOrWhiteSpace(cat))
                {
                    return;
                }
                string name = Microsoft.VisualBasic.Interaction.InputBox(L.T("Name:"), Text, p.Name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    return;
                }
                p.Category = cat.Trim();
                p.Name = name.Trim();
                Say(L.T("Gespeichert") + $": {p.Save()}");
                return;
            }
            Taken?.Invoke(this, p);
            Say(L.T("In die Werkstatt kopiert") + $": {r.Width}×{r.Height}, {p.Statics.Count} Statics.");
        }
    }
}

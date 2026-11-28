/***************************************************************************
 *
 *  * Dragon Map Studio – "Bereich bearbeiten (eigenes Fenster)": the marked area of the CGI map is copied (with 8
 *  *  tiles border), the whole CGI work surface (view, palette, side panel, tool bars) moves into the region window
 *  *  and works on the copy. "Fertig" writes the changes back to the same place (MapData.WriteBack = one undo step
 *  *  of the big map), "Abbrechen" throws the copy away; the work surface moves back into the tab.
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
        /// <summary>Largest side of a region (the copy also gets an 8 tiles border).</summary>
        private const int RegionMaxSide = 1024;

        private RegionEditorForm _region;
        private MapData _regionMain;
        private Point _regionOrigin;
        private Rectangle _regionArea;
        private Panel _regionPlaceholder;
        private List<Control> _regionMoved;
        private (int lines, bool info) _regionViewBefore;

        /// <summary>True (with a hint) while a region window is open – loading / saving the CGI map waits for it.</summary>
        private bool RegionBlocks()
        {
            if (_region == null)
            {
                return false;
            }
            SetStatus(L.T("Erst das Bereichs-Fenster mit '✔ Fertig' oder '✖ Abbrechen' schließen."));
            _region.Activate();
            return true;
        }

        private void OpenRegion()
        {
            if (_region != null)
            {
                _region.Activate();
                return;
            }
            if (_cgiMap == null || isoView.Area.IsEmpty)
            {
                SetStatus(L.T("Erst mit 'Bereich' ein Gebiet markieren (Umschalt = erweitern, Strg + Klick = zwei Ecken, 'Koordinaten …'), dann 'In eigenem Fenster bearbeiten'."));
                return;
            }
            Rectangle a = isoView.Area;
            if (a.Width > RegionMaxSide || a.Height > RegionMaxSide)
            {
                SetStatus(L.T("Der Bereich ist zu groß") + $" ({a.Width}×{a.Height}, " + L.T("höchstens") + $" {RegionMaxSide}×{RegionMaxSide}).");
                return;
            }
            MapData main = _cgiMap;
            MapData copy;
            Point origin;
            UseWaitCursor = true;
            try
            {
                copy = main.CopyRegion(Rectangle.Inflate(a, 8, 8), out origin);
            }
            finally
            {
                UseWaitCursor = false;
            }
            _regionMain = main;
            _regionOrigin = origin;
            _regionArea = a;
            _regionViewBefore = (isoView.CoordLines, isoView.TileInfo);

            var host = new RegionHost
            {
                MainView = isoView,
                Map = () => _cgiMap,
                Origin = origin,
                Configure = ConfigureLikeMain,
                SelectTool = SelectCgiTool,
                SetUnder = SetUnderView,
                Finish = EndRegion,
                LevelGround = LevelGroundDialog,
                SaveTemplate = t => SaveTemplate(t, t.HasLand, t.Name, L.T("Gebäude")),
                Status = SetStatus,
                Help = ShowHelp,
                TogglePalette = ToggleCgiPalette,
                ToggleSide = ToggleSidePanel,
                UndockSide = UndockSidePanel,
                ToggleOptions = ToggleOptionWindow
            };
            _region = new RegionEditorForm(host) { Owner = this };
            _region.Text = L.T("Bereich bearbeiten") + $" – ({a.X}, {a.Y}) {a.Width}×{a.Height} – Dragon Map Studio";

            // the whole CGI work surface moves into the window (same order = same docking)
            _regionMoved = tabCgi.Controls.Cast<Control>().ToList();
            tabCgi.SuspendLayout();
            foreach (Control c in _regionMoved)
            {
                tabCgi.Controls.Remove(c);
                _region.WorkspaceHost.Controls.Add(c);
            }
            _regionPlaceholder = RegionPlaceholder(a);
            tabCgi.Controls.Add(_regionPlaceholder);
            tabCgi.ResumeLayout();

            SetCgiMap(copy, a.X - origin.X + a.Width / 2.0, a.Y - origin.Y + a.Height / 2.0);
            isoView.CoordOffset = origin;
            isoView.CoordLines = 8;
            isoView.TileInfo = true;
            SelectCgiTool(IsoTool.Select);
            isoView.SetAreaAndShow(new Rectangle(a.X - origin.X, a.Y - origin.Y, a.Width, a.Height));
            _region.Show(this);
            SetStatus(L.T("Bereich im eigenen Fenster") + $": ({a.X}, {a.Y}) {a.Width}×{a.Height} – " + L.T("alle Werkzeuge wirken auf die Kopie; '✔ Fertig' schreibt sie zurück."));
        }

        private Panel RegionPlaceholder(Rectangle a)
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(241, 245, 249) };
            var text = new Label
            {
                Left = 40, Top = 40, Width = 720, Height = 60, Font = new Font("Segoe UI", 11f),
                Text = L.T("Der Bereich") + $" ({a.X}, {a.Y}) {a.Width}×{a.Height} " + L.T("wird gerade im eigenen Fenster bearbeitet. Die CGI-Arbeitsfläche ist dort.")
            };
            var show = new Button { Left = 40, Top = 110, Width = 200, Height = 32, Text = L.T("⧉ Bereichs-Fenster zeigen") };
            show.Click += (s, e) => _region?.Activate();
            var done = new Button { Left = 250, Top = 110, Width = 200, Height = 32, Text = L.T("✔ Fertig – zurückschreiben") };
            done.Click += (s, e) => EndRegion(true);
            var cancel = new Button { Left = 460, Top = 110, Width = 140, Height = 32, Text = L.T("✖ Abbrechen") };
            cancel.Click += (s, e) => _region?.Close();
            p.Controls.AddRange(new Control[] { text, show, done, cancel });
            return p;
        }

        /// <summary>Ends the region window: <paramref name="apply"/> = write the copy back (one undo step), else throw it away.</summary>
        private void EndRegion(bool apply)
        {
            if (_region == null)
            {
                return;
            }
            MapData copy = _cgiMap;
            RegionEditorForm form = _region;
            _region = null;

            // work surface back into the tab first (closing the window would dispose it)
            tabCgi.SuspendLayout();
            tabCgi.Controls.Remove(_regionPlaceholder);
            _regionPlaceholder.Dispose();
            _regionPlaceholder = null;
            foreach (Control c in _regionMoved)
            {
                form.WorkspaceHost.Controls.Remove(c);
                tabCgi.Controls.Add(c);
            }
            tabCgi.ResumeLayout();
            _regionMoved = null;

            int n = 0;
            bool changed = copy != null && copy.UndoCount > 0;
            if (apply && changed)
            {
                UseWaitCursor = true;
                try
                {
                    n = _regionMain.WriteBack(copy, _regionOrigin);
                    _regionMain.NameLastStep(L.T("Bereichs-Fenster"));
                }
                finally
                {
                    UseWaitCursor = false;
                }
            }
            isoView.CoordOffset = Point.Empty;
            isoView.CoordLines = _regionViewBefore.lines;
            isoView.TileInfo = _regionViewBefore.info;
            Rectangle a = _regionArea;
            SetCgiMap(_regionMain, a.X + a.Width / 2.0, a.Y + a.Height / 2.0);
            isoView.SetAreaAndShow(a, false);
            _regionMain = null;
            form.CloseQuiet();
            SetStatus(apply
                ? (changed ? L.T("Bereich zurückgeschrieben") + $": {n} " + L.T("Änderungen an") + $" ({a.X}, {a.Y}) – Strg+Z " + L.T("nimmt alles zurück") + "." : L.T("Im Bereich wurde nichts geändert."))
                : L.T("Bereichs-Fenster abgebrochen – die Karte ist unverändert."));
        }
    }
}

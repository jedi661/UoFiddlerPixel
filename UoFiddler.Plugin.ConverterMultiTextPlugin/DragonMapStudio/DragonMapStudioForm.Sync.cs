/***************************************************************************
 *
 *  * Dragon Map Studio – map editor and CGI view stay in step: both sides remember the area they changed (colour map:
 *  *  MapHistory.Edited, CGI map: MapData.Edited). When the tab is changed, the other side is brought up to date –
 *  *  colour map → CGI: the area is built again; CGI → colour map: the area is converted into colours (+ statics
 *  *  layer). Changed on both sides at the same place: the studio asks which one wins. Saving / closing with changes
 *  *  that the other side does not have yet gives a warning (can be ignored). Mode: automatically / ask / only warn.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        /// <summary>Areas changed on one side that the other side does not have yet.</summary>
        private Rectangle _editorPending = Rectangle.Empty;
        private Rectangle _cgiPending = Rectangle.Empty;

        /// <summary>The maps the pending areas belong to (another map opened = forget them).</summary>
        private object _editorPendingMap;
        private MapData _cgiPendingMap;

        /// <summary>True while the studio itself writes the other side (these changes are not "pending").</summary>
        private bool _viewSyncing;

        private MapData _syncWatched;
        private TabPage _syncLastTab;
        private ToolStripDropDownButton _syncCgiButton;
        private ToolStripDropDownButton _syncEditorButton;

        /// <summary>Colour map and CGI map belong together (same size, no region window open).</summary>
        private bool SyncPaired => _map != null && _cgiMap != null && _region == null && _cgiFromEditor && _cgiMap.Width == _map.Width && _cgiMap.Height == _map.Height;

        private static Rectangle Grow(Rectangle a, Rectangle b) => a.IsEmpty ? b : Rectangle.Union(a, b);

        /// <summary>Once, from BuildToolOptions.</summary>
        private void InitSync()
        {
            _history.Edited += r =>
            {
                if (_viewSyncing || !SyncPaired)
                {
                    return;
                }
                if (_editorPendingMap != _map)
                {
                    _editorPending = Rectangle.Empty;
                    _editorPendingMap = _map;
                }
                _editorPending = Grow(_editorPending, r);
                UpdateSyncMarks();
            };
            _syncCgiButton = SyncButton();
            toolStripCgi.Items.Add(_syncCgiButton);
            _syncEditorButton = SyncButton();
            toolStripEditor.Items.Add(_syncEditorButton);
            _syncLastTab = tabMain.SelectedTab;
            WatchCgiMap();
            UpdateSyncMarks();
        }

        /// <summary>Follows the changes of the current CGI map (called whenever the CGI map is set).</summary>
        private void WatchCgiMap()
        {
            if (_syncWatched == _cgiMap)
            {
                return;
            }
            if (_syncWatched != null)
            {
                _syncWatched.Edited -= CgiMapEdited;
            }
            _syncWatched = _cgiMap;
            if (_syncWatched != null)
            {
                _syncWatched.Edited += CgiMapEdited;
            }
        }

        private void CgiMapEdited(Rectangle r)
        {
            if (_viewSyncing || !SyncPaired)
            {
                return;
            }
            NoteCgiEdited(r);
        }

        /// <summary>The CGI map changed in this area (also from the region window "Fertig").</summary>
        private void NoteCgiEdited(Rectangle r)
        {
            if (_cgiPendingMap != _cgiMap)
            {
                _cgiPending = Rectangle.Empty;
                _cgiPendingMap = _cgiMap;
            }
            _cgiPending = Grow(_cgiPending, r);
            UpdateSyncMarks();
        }

        /// <summary>A fresh CGI map from the colour map (built / loaded): nothing is pending any more.</summary>
        private void ForgetPending()
        {
            _editorPending = _cgiPending = Rectangle.Empty;
            UpdateSyncMarks();
        }

        private ToolStripDropDownButton SyncButton()
        {
            var dd = new ToolStripDropDownButton("⟳ " + L.T("Abgleich")) { Alignment = ToolStripItemAlignment.Right };
            var now = new ToolStripMenuItem(L.T("Jetzt abgleichen"));
            now.Click += (s, e) => SyncNow(true);
            string[] modes = { "Beim Tab-Wechsel automatisch", "Beim Tab-Wechsel erst fragen", "Nur hinweisen (nichts von selbst)" };
            var items = new List<ToolStripMenuItem>();
            for (int i = 0; i < modes.Length; i++)
            {
                int mode = i;
                var it = new ToolStripMenuItem(L.T(modes[i])) { Checked = _settings.SyncMode == i };
                it.Click += (s, e) =>
                {
                    _settings.SyncMode = mode;
                    SaveLayoutSoon();
                    UpdateSyncMarks();
                };
                items.Add(it);
            }
            var forget = new ToolStripMenuItem(L.T("Offene Änderungen vergessen (ignorieren)"));
            forget.Click += (s, e) =>
            {
                ForgetPending();
                SetStatus(L.T("Abgleich: offene Änderungen werden ignoriert."));
            };
            dd.DropDownOpening += (s, e) =>
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Checked = _settings.SyncMode == i;
                }
                now.Enabled = SyncPaired && (!_editorPending.IsEmpty || !_cgiPending.IsEmpty);
                forget.Enabled = now.Enabled;
            };
            dd.DropDownItems.Add(now);
            dd.DropDownItems.Add(new ToolStripSeparator());
            dd.DropDownItems.AddRange(items.ToArray());
            dd.DropDownItems.Add(new ToolStripSeparator());
            dd.DropDownItems.Add(forget);
            return dd;
        }

        /// <summary>Marks on the tabs / buttons: ⟳ = the other side does not have these changes yet.</summary>
        private void UpdateSyncMarks()
        {
            bool ed = SyncPaired && !_editorPending.IsEmpty, cg = SyncPaired && !_cgiPending.IsEmpty;
            string Strip(string t) => t.Replace(" ⟳", string.Empty);
            tabEditor.Text = Strip(tabEditor.Text) + (cg ? " ⟳" : string.Empty);
            tabCgi.Text = Strip(tabCgi.Text) + (ed ? " ⟳" : string.Empty);
            string tip = !ed && !cg ? L.T("Karten-Editor und CGI-Ansicht sind gleich.")
                : (ed ? L.T("Im Karten-Editor geändert") + $": {_editorPending.Width}×{_editorPending.Height} " + L.T("ab") + $" ({_editorPending.X}, {_editorPending.Y}) – " + L.T("fehlt noch in der CGI-Ansicht") + ". " : string.Empty) +
                  (cg ? L.T("In der CGI-Ansicht geändert") + $": {_cgiPending.Width}×{_cgiPending.Height} " + L.T("ab") + $" ({_cgiPending.X}, {_cgiPending.Y}) – " + L.T("fehlt noch im Karten-Editor") + "." : string.Empty);
            foreach (ToolStripDropDownButton b in new[] { _syncCgiButton, _syncEditorButton })
            {
                if (b == null)
                {
                    continue;
                }
                b.ToolTipText = tip;
                b.ForeColor = ed || cg ? Color.FromArgb(194, 65, 12) : SystemColors.ControlText;
                b.Text = "⟳ " + L.T("Abgleich") + (ed || cg ? " !" : string.Empty);
            }
        }

        /// <summary>Called when the tab changes: bring the side that is shown now up to date.</summary>
        private void SyncOnTabChange()
        {
            TabPage now = tabMain.SelectedTab;
            TabPage before = _syncLastTab;
            _syncLastTab = now;
            if (!SyncPaired || now == before || _viewSyncing)
            {
                return;
            }
            if (now == tabCgi && !_editorPending.IsEmpty)
            {
                SyncDirection(true, false);
            }
            else if (now == tabEditor && !_cgiPending.IsEmpty)
            {
                SyncDirection(false, false);
            }
        }

        /// <summary>"Jetzt abgleichen": the side that is shown gets the changes of the other one (both, if both changed).</summary>
        private void SyncNow(bool user)
        {
            if (!SyncPaired)
            {
                SetStatus(L.T("Abgleich geht nur, wenn Farbkarte und CGI-Karte gleich groß sind (und kein Bereichs-Fenster offen ist)."));
                return;
            }
            if (!_cgiPending.IsEmpty)
            {
                SyncDirection(false, user);
            }
            if (!_editorPending.IsEmpty)
            {
                SyncDirection(true, user);
            }
        }

        /// <summary>
        /// <paramref name="toCgi"/>: colour map → CGI view (else CGI → colour map). Asks when both sides changed at the same
        /// place, or in the mode "erst fragen"; in the mode "nur hinweisen" only a hint (unless <paramref name="user"/>).
        /// </summary>
        private void SyncDirection(bool toCgi, bool user)
        {
            Rectangle area = toCgi ? _editorPending : _cgiPending;
            Rectangle other = toCgi ? _cgiPending : _editorPending;
            if (area.IsEmpty)
            {
                return;
            }
            string from = L.T(toCgi ? "Karten-Editor" : "CGI-Ansicht"), to = L.T(toCgi ? "CGI-Ansicht" : "Karten-Editor");
            string where = $"{area.Width}×{area.Height} " + L.T("ab") + $" ({area.X}, {area.Y})";
            if (!user && _settings.SyncMode == 2)
            {
                SetStatus("⟳ " + L.T("Im") + $" {from} " + L.T("geändert") + $" ({where}) – " + L.T("fehlt noch in der") + $" {to}. " + L.T("'⟳ Abgleich → Jetzt abgleichen' übernimmt es."));
                return;
            }
            bool conflict = !other.IsEmpty && Rectangle.Inflate(other, 2, 2).IntersectsWith(area);
            bool big = area.Width > 1024 || area.Height > 1024;
            if (conflict || big || (!user && _settings.SyncMode == 1))
            {
                string text = conflict
                    ? L.T("Karten-Editor UND CGI-Ansicht wurden an derselben Stelle geändert.") + "\n\n" +
                      L.T("Ja") + $" = {from} → {to} (" + L.T("die Änderungen dort in der") + $" {to} " + L.T("werden überschrieben") + ")\n" +
                      L.T("Nein = ignorieren (später abgleichen)") + "\n" + L.T("Abbrechen = vergessen (beide Seiten bleiben, wie sie sind)")
                    : $"{from} " + L.T("hat Änderungen") + $" ({where}), " + L.T("die in der") + $" {to} " + L.T("noch fehlen.") +
                      (big ? "\n" + L.T("Der Bereich ist groß – das kann einen Moment dauern.") : string.Empty) + "\n\n" +
                      L.T("Ja = jetzt übernehmen") + "\n" + L.T("Nein = später (bleibt markiert ⟳)") + "\n" + L.T("Abbrechen = ignorieren (vergessen)");
                DialogResult r = MessageBox.Show(this, text, L.T("Abgleich Karten-Editor ↔ CGI"), MessageBoxButtons.YesNoCancel, conflict ? MessageBoxIcon.Warning : MessageBoxIcon.Question);
                if (r == DialogResult.No)
                {
                    return;
                }
                if (r == DialogResult.Cancel)
                {
                    if (toCgi)
                    {
                        _editorPending = Rectangle.Empty;
                    }
                    else
                    {
                        _cgiPending = Rectangle.Empty;
                    }
                    UpdateSyncMarks();
                    return;
                }
            }
            if (toCgi)
            {
                _editorPending = Rectangle.Empty;
                if (conflict)
                {
                    _cgiPending = Rectangle.Empty;
                }
                UpdateSyncMarks();
                RebuildCgiArea(Rectangle.Inflate(area, 1, 1), area.X + area.Width / 2.0, area.Y + area.Height / 2.0, false,
                    () => SetStatus("⟳ " + L.T("CGI-Ansicht abgeglichen") + $": {where}"));
            }
            else
            {
                _cgiPending = Rectangle.Empty;
                if (conflict)
                {
                    _editorPending = Rectangle.Empty;
                }
                UpdateSyncMarks();
                CgiIntoEditorQuiet(area);
            }
        }

        /// <summary>CGI area → colour map (+ statics layer) without dialogs; missing colours get the nearest one.</summary>
        private void CgiIntoEditorQuiet(Rectangle area)
        {
            area.Intersect(new Rectangle(0, 0, _cgiMap.Width, _cgiMap.Height));
            if (area.IsEmpty)
            {
                return;
            }
            UseWaitCursor = true;
            try
            {
                LandTransitionEngine engine = TransitionEngine();
                CgiConversion conv = CgiToColor.Analyze(_cgiMap, area, _rules, engine, true, CompilerTileGroups());
                var log = new List<string>();
                if (!ResolveMissingColours(conv, MissingColorMode.Suggest, log))
                {
                    _cgiPending = Grow(_cgiPending, area);
                    UpdateSyncMarks();
                    return;
                }
                CgiToColor.Finish(conv, _cgiMap, _rules, engine);
                _viewSyncing = true;
                try
                {
                    WriteConversionIntoEditor(conv, _cgiMap, StaticsMode.Layer, false);
                }
                finally
                {
                    _viewSyncing = false;
                }
                mapCanvas.InvalidateMap();
                SetStatus("⟳ " + L.T("Karten-Editor abgeglichen") + $": {area.Width}×{area.Height} " + L.T("ab") + $" ({area.X}, {area.Y}) – Strg+Z " + L.T("im Karten-Editor nimmt es zurück") + ".");
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        /// <summary>Before saving / closing: changes the other side does not have yet. False = cancel.</summary>
        private bool WarnPending(bool savingCgi, bool closing)
        {
            if (!SyncPaired)
            {
                return true;
            }
            bool ed = !_editorPending.IsEmpty, cg = !_cgiPending.IsEmpty;
            if (closing ? !(ed || cg) : savingCgi ? !ed : !cg)
            {
                return true;
            }
            string what = (ed ? "• " + L.T("Karten-Editor-Änderungen fehlen noch in der CGI-Ansicht") + $" ({_editorPending.Width}×{_editorPending.Height})\n" : string.Empty) +
                          (cg ? "• " + L.T("CGI-Änderungen fehlen noch im Karten-Editor") + $" ({_cgiPending.Width}×{_cgiPending.Height})\n" : string.Empty);
            DialogResult r = MessageBox.Show(this, L.T("Nicht abgeglichen") + ":\n" + what + "\n" + L.T("Ja = jetzt abgleichen") + "\n" + L.T("Nein = ignorieren und weiter") + "\n" + L.T("Abbrechen = zurück"),
                L.T("Abgleich Karten-Editor ↔ CGI"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (r == DialogResult.Cancel)
            {
                return false;
            }
            if (r == DialogResult.Yes)
            {
                if (cg)
                {
                    SyncDirection(false, true);
                }
                if (ed)
                {
                    SyncDirection(true, true);
                    if (savingCgi || closing)
                    {
                        // the CGI area is built in the background – save afterwards
                        SetStatus(L.T("Die CGI-Ansicht wird abgeglichen – danach bitte noch einmal speichern."));
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

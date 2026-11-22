/***************************************************************************
 *
 *  * Dragon Map Studio – map editor: height layer with the normal colour tools
 *  *
 *  *  "Höhe behalten": brush, eraser, spray, line, shapes, fill and "fill selection" keep the height of the ground –
 *  *  the colour of the same group at that height, and when the group has no colour there (wood floor only Z 0) the
 *  *  height goes into the height layer of the map (.statics.bin, undo / redo with the pixels).
 *  *  Without "Höhe behalten" a painted cell loses an old entry of the height layer (the colour's own height counts).
 *  *  "Höhen-Ebene zeigen" draws the cells of the layer (cyan = active, grey = painted over), hover shows the height.
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
        private bool _heightBrushHooked;

        private void InitHeightBrush()
        {
            if (_heightBrushHooked)
            {
                return;
            }
            _heightBrushHooked = true;
            mapCanvas.HeightLayer = () => _staticsLayer?.Heights;
            mapCanvas.CellColour = CellColourFor;
            mapCanvas.ShowHeightLayer = _settings.ShowHeightLayer;
            // the collected heights belong into the same undo step as the pixels
            mapCanvas.StepClosing += (s, e) => ApplyPendingHeights();
            mapCanvas.Painting += (s, e) => ApplyPendingHeights();
            mapCanvas.CursorMoved += (s, e) => ShowHeightAtCursor(e.X, e.Y);
        }

        /// <summary>Height of a cell while painting: a height noted in this step, the height layer, else the colour's altitude.</summary>
        private int EffectiveAltitude(int x, int y, ushort colour)
        {
            if (_pendingHeights.TryGetValue((x, y), out (sbyte z, ushort colour)? pending))
            {
                if (pending.HasValue && pending.Value.colour == colour)
                {
                    return pending.Value.z;
                }
                return colour < _rules.Palette.Count ? _rules.Palette[colour].Altitude : 0;
            }
            if (_staticsLayer.Heights.TryGetValue((x, y), out (sbyte z, ushort colour) h) && h.colour == colour)
            {
                return h.z;
            }
            return colour < _rules.Palette.Count ? _rules.Palette[colour].Altitude : 0;
        }

        /// <summary>Colour tools of the canvas: what really goes into a cell (see "Höhe behalten").</summary>
        private ushort CellColourFor(int x, int y, ushort current, ushort chosen)
        {
            if (!_settings.BrushKeepHeight)
            {
                // a newly painted cell has the height of its colour: an old entry of the height layer goes (undo brings it back)
                if (_staticsLayer.Heights.Count > 0 && _staticsLayer.Heights.ContainsKey((x, y)))
                {
                    _pendingHeights[(x, y)] = null;
                }
                return chosen;
            }
            if (chosen >= _rules.Palette.Count)
            {
                return chosen;
            }
            int want = current < _rules.Palette.Count ? EffectiveAltitude(x, y, current) : _rules.Palette[chosen].Altitude;
            int result = chosen;
            int g = _rules.Palette[chosen].Group;
            if (g >= 0)
            {
                if (!_groupEntries.TryGetValue(g, out List<DragonPaletteEntry> entries))
                {
                    _groupEntries[g] = entries = _rules.EntriesOfGroup(g).Where(e => e.Defined).ToList();
                }
                if (entries.Count > 0)
                {
                    result = entries.OrderBy(e => Math.Abs(e.Altitude - want)).ThenBy(e => e.Index == chosen ? 0 : 1).First().Index;
                }
            }
            if (Math.Abs(_rules.Palette[result].Altitude - want) > 1)
            {
                _pendingHeights[(x, y)] = ((sbyte)Math.Clamp(want, -128, 127), (ushort)result);
            }
            else if (_pendingHeights.ContainsKey((x, y)) || _staticsLayer.Heights.ContainsKey((x, y)))
            {
                _pendingHeights[(x, y)] = null; // only touch cells that have an entry (big fills stay small)
            }
            return (ushort)result;
        }

        private void ToggleKeepHeight(bool on)
        {
            _settings.BrushKeepHeight = on;
            _groupEntries.Clear();
            UpdateEditorOptions();
            SetStatus(on
                ? L.T("Höhe behalten: an – Pinsel, Linie, Formen und Füllen nehmen die Farbe der Gruppe auf der Höhe des Bodens; fehlt sie (z. B. Holzboden nur Z 0), merkt sich die Höhen-Ebene die Höhe.")
                : L.T("Höhe behalten: aus – jede Farbe hat ihre eigene Höhe."));
        }

        private void ToggleHeightLayerView(bool on)
        {
            _settings.ShowHeightLayer = on;
            mapCanvas.ShowHeightLayer = on;
            mapCanvas.Invalidate();
            UpdateEditorOptions();
            int n = _staticsLayer?.Heights.Count ?? 0;
            SetStatus(on ? L.T("Höhen-Ebene") + $": {n:N0} " + L.T("Zellen – türkis = Höhe gilt, grau = Farbe wurde übermalt (zählt nicht). Ab Zoom 12 mit Höhe.") : L.T("Höhen-Ebene ausgeblendet."));
        }

        /// <summary>Removes the entries of the height layer inside the selection (or the whole map) – one undo step.</summary>
        private void ClearHeightLayer(Rectangle area)
        {
            if (_map == null)
            {
                return;
            }
            List<(int x, int y)> keys = _staticsLayer.Heights.Keys.Where(k => area.Contains(k.x, k.y)).ToList();
            if (keys.Count == 0)
            {
                SetStatus(L.T("Hier gibt es keine Einträge der Höhen-Ebene."));
                return;
            }
            mapCanvas.EditRect(area, L.T("Höhen-Ebene löschen"), m =>
            {
                foreach ((int x, int y) k in keys)
                {
                    _pendingHeights[k] = null;
                }
            });
            mapCanvas.Invalidate();
            SetStatus(L.T("Höhen-Ebene") + $": {keys.Count:N0} " + L.T("Einträge entfernt (Strg+Z holt sie zurück)."));
        }

        /// <summary>Status line: the height under the mouse, marked when it comes from the height layer.</summary>
        private void ShowHeightAtCursor(int x, int y)
        {
            if (_map == null || !_map.InBounds(x, y))
            {
                return;
            }
            ushort c = _map[x, y];
            bool layer = _staticsLayer.Heights.TryGetValue((x, y), out (sbyte z, ushort colour) h);
            string text;
            if (layer && h.colour == c)
            {
                text = $"Z {h.z} (" + L.T("Höhen-Ebene") + ")";
            }
            else
            {
                text = "Z " + (c < _rules.Palette.Count ? _rules.Palette[c].Altitude : 0) + (layer ? " · " + L.T("alter Eintrag") + $" Z {h.z}" : string.Empty);
            }
            lblPosition.Text = lblPosition.Text + "  |  " + text;
        }
    }
}

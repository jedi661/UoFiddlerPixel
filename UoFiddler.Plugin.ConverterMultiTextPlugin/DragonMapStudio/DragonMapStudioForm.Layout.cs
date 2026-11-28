/***************************************************************************
 *
 *  * Dragon Map Studio – the layout is remembered: palettes (own window or docked, switched off), the side panel (own
 *  *  window, width, switched off), the tool options window, the region window with its panes. Every change is written
 *  *  to the settings file a moment later (not only when the studio closes), so it is there at the next start.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio
{
    public partial class DragonMapStudioForm
    {
        private Timer _layoutSave;

        /// <summary>Writes the settings a moment after the last layout change (moving a window fires many events).</summary>
        private void SaveLayoutSoon()
        {
            if (_layoutSave == null)
            {
                _layoutSave = new Timer { Interval = 1500 };
                _layoutSave.Tick += (s, e) =>
                {
                    _layoutSave.Stop();
                    try
                    {
                        _settings.Save();
                    }
                    catch (Exception)
                    {
                        // read-only folder – kept until the studio closes
                    }
                };
            }
            _layoutSave.Stop();
            _layoutSave.Start();
        }

        /// <summary>Applies the remembered layout and saves every change of it (once, from BuildToolOptions).</summary>
        private void InitLayoutMemory()
        {
            if (_settings.CgiPaletteHidden && _cgiPalette != null)
            {
                _cgiPalette.Visible = false;
            }
            if (_settings.EditorPaletteHidden && _editorPalette != null)
            {
                _editorPalette.Visible = false;
            }
            if (_settings.CgiSideCollapsed && !_settings.SideWindowOpen)
            {
                splitCgi.Panel2Collapsed = true;
            }
            void ApplyWidth(object s, EventArgs e)
            {
                Shown -= ApplyWidth;
                if (_settings.CgiSideWidth > 200 && splitCgi.Width > _settings.CgiSideWidth + 300)
                {
                    try
                    {
                        splitCgi.SplitterDistance = splitCgi.Width - _settings.CgiSideWidth;
                    }
                    catch (Exception)
                    {
                        // too small – the default stays
                    }
                }
                splitCgi.SplitterMoved += (s2, e2) =>
                {
                    if (!splitCgi.Panel2Collapsed)
                    {
                        _settings.CgiSideWidth = splitCgi.Width - splitCgi.SplitterDistance;
                        SaveLayoutSoon();
                    }
                };
            }
            if (Visible)
            {
                ApplyWidth(this, EventArgs.Empty);
            }
            else
            {
                Shown += ApplyWidth;
            }
            _cgiDock.Changed += (s, e) => SaveLayoutSoon();
            _editorDock.Changed += (s, e) => SaveLayoutSoon();
        }
    }
}

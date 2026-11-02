/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Globalization;
using UoFiddler.Plugin.Enhanced.EcUltima.Analysis;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>An editor that can show an id or a map position (used by the search).</summary>
    public interface IEcNavigable
    {
        void Navigate(EcNavigation target);
    }

    // The implementations are kept here so the editors themselves stay unchanged.

    public sealed partial class EcTileArtControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            toolStripComboBoxRange.SelectedIndex = 0;
            toolStripTextBoxFilter.Text = target.Id.ToString(CultureInfo.InvariantCulture);
            ApplyFilter();
        }
    }

    public sealed partial class EcMultisControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            SelectId(target.Id);
        }
    }

    public sealed partial class EcMapControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            if (target.File != null && toolStripComboBoxFacet.Items.Contains(target.File))
            {
                toolStripComboBoxFacet.SelectedItem = target.File;
            }
            toolStripTextBoxGoTo.Text = string.Format(CultureInfo.InvariantCulture, "{0},{1}", target.X, target.Y);
            GoTo();
        }
    }

    public sealed partial class EcHuesControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            tabControl.SelectedTab = tabPageEditor;
            toolStripTextBoxSearch.Text = string.Empty;
            ApplyFilter();
            SelectHue(target.Id);
        }
    }

    public sealed partial class EcAnimDefinitionControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            toolStripComboBoxFilter.SelectedIndex = 0;
            toolStripTextBoxSearch.Text = target.Id.ToString(CultureInfo.InvariantCulture);
            ApplyFilter();
        }
    }

    public sealed partial class EcMobileAnimationControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            toolStripTextBoxSearch.Text = target.Id.ToString(CultureInfo.InvariantCulture);
            ApplyFilter();
        }
    }

    public sealed partial class EcArtConverterControl : IEcNavigable
    {
        public void Navigate(EcNavigation target)
        {
            OnActivated();
            bool gump = target.Target == EcTarget.Gump;
            tabControl.SelectedTab = gump ? tabPageGumps : tabPageItems;
            string id = string.Format(CultureInfo.InvariantCulture, "0x{0:X4}", target.Id);
            if (gump)
            {
                toolStripTextBoxSearchGumps.Text = id;
                if (_gumps.Count == 0)
                {
                    ScanGumpsAsync();
                }
                else
                {
                    ApplyGumpFilter();
                }
            }
            else
            {
                toolStripTextBoxSearchItems.Text = id;
                if (_items.Count == 0)
                {
                    ScanItemsAsync();
                }
                else
                {
                    ApplyItemFilter();
                }
            }
        }
    }
}

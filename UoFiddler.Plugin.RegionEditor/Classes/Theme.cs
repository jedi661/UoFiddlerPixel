/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Light / dark design of the region editor. The designer sets the light
 * colours; Apply() walks through all controls and sets the colours of the
 * chosen theme at runtime.
 *
 ***************************************************************************/

using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public sealed class ThemePalette
    {
        public Color Back { get; set; }
        public Color Panel { get; set; }
        public Color Input { get; set; }
        public Color Text { get; set; }
        public Color SubText { get; set; }
        public Color Accent { get; set; }
        public Color Border { get; set; }
        public Color Selection { get; set; }
        public Color ToolBar { get; set; }
        public Color MapBack { get; set; }
        public Color Error { get; set; }
        public Color Warning { get; set; }

        public static readonly ThemePalette Light = new ThemePalette
        {
            Back = Color.FromArgb(246, 247, 249),
            Panel = Color.White,
            Input = Color.White,
            Text = Color.FromArgb(30, 30, 34),
            SubText = Color.FromArgb(105, 110, 120),
            Accent = Color.FromArgb(0, 120, 212),
            Border = Color.FromArgb(214, 218, 224),
            Selection = Color.FromArgb(204, 228, 247),
            ToolBar = Color.FromArgb(240, 242, 245),
            MapBack = Color.FromArgb(40, 44, 52),
            Error = Color.FromArgb(200, 40, 40),
            Warning = Color.FromArgb(180, 120, 0)
        };

        public static readonly ThemePalette Dark = new ThemePalette
        {
            Back = Color.FromArgb(30, 31, 34),
            Panel = Color.FromArgb(37, 39, 43),
            Input = Color.FromArgb(48, 50, 56),
            Text = Color.FromArgb(225, 227, 230),
            SubText = Color.FromArgb(150, 155, 165),
            Accent = Color.FromArgb(76, 160, 255),
            Border = Color.FromArgb(62, 65, 72),
            Selection = Color.FromArgb(38, 79, 120),
            ToolBar = Color.FromArgb(43, 45, 50),
            MapBack = Color.FromArgb(18, 19, 22),
            Error = Color.FromArgb(255, 110, 110),
            Warning = Color.FromArgb(240, 190, 80)
        };
    }

    public static class ThemeManager
    {
        public static ThemePalette Palette => RegionEditorSettings.Current.Theme == EditorTheme.Dark ? ThemePalette.Dark : ThemePalette.Light;

        public static void Apply(Control root)
        {
            ThemePalette p = Palette;
            ApplyTo(root, p);
        }

        private static void ApplyTo(Control c, ThemePalette p)
        {
            switch (c)
            {
                case ToolStrip ts:
                    ts.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(p)) { RoundedEdges = false };
                    ts.BackColor = p.ToolBar;
                    ts.ForeColor = p.Text;
                    foreach (ToolStripItem item in ts.Items)
                    {
                        ApplyItem(item, p);
                    }

                    break;
                case TextBoxBase _:
                case ComboBox _:
                case NumericUpDown _:
                case ListBox _:
                    c.BackColor = p.Input;
                    c.ForeColor = p.Text;
                    break;
                case TreeView tv:
                    tv.BackColor = p.Panel;
                    tv.ForeColor = p.Text;
                    tv.LineColor = p.SubText;
                    break;
                case ListView lv:
                    lv.BackColor = p.Panel;
                    lv.ForeColor = p.Text;
                    break;
                case DataGridView grid:
                    grid.BackgroundColor = p.Panel;
                    grid.GridColor = p.Border;
                    grid.DefaultCellStyle.BackColor = p.Input;
                    grid.DefaultCellStyle.ForeColor = p.Text;
                    grid.DefaultCellStyle.SelectionBackColor = p.Selection;
                    grid.DefaultCellStyle.SelectionForeColor = p.Text;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = p.ToolBar;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = p.Text;
                    grid.EnableHeadersVisualStyles = false;
                    break;
                case Button b:
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = p.Border;
                    b.BackColor = p.ToolBar;
                    b.ForeColor = p.Text;
                    break;
                case TabPage tp:
                    tp.BackColor = p.Panel;
                    tp.ForeColor = p.Text;
                    break;
                case SplitContainer sc:
                    sc.BackColor = p.Border;
                    sc.Panel1.BackColor = p.Back;
                    sc.Panel2.BackColor = p.Back;
                    break;
                case IThemed themed:
                    themed.ApplyTheme(p);
                    break;
                default:
                    if (c is Label || c is CheckBox || c is RadioButton || c is GroupBox || c is Panel || c is TableLayoutPanel || c is UserControl || c is Form || c is TabControl)
                    {
                        c.BackColor = c is Form || c is UserControl ? p.Back : (c is Label || c is CheckBox || c is RadioButton ? Color.Transparent : p.Panel);
                        c.ForeColor = p.Text;
                    }

                    break;
            }

            if (c.ContextMenuStrip != null)
            {
                ApplyTo(c.ContextMenuStrip, p);
            }

            foreach (Control child in c.Controls)
            {
                ApplyTo(child, p);
            }
        }

        private static void ApplyItem(ToolStripItem item, ThemePalette p)
        {
            item.ForeColor = p.Text;
            if (item is ToolStripComboBox combo)
            {
                combo.BackColor = p.Input;
                combo.ForeColor = p.Text;
            }
            else if (item is ToolStripTextBox text)
            {
                text.BackColor = p.Input;
                text.ForeColor = p.Text;
            }

            if (item is ToolStripDropDownItem dd)
            {
                dd.DropDown.BackColor = p.Panel;
                dd.DropDown.ForeColor = p.Text;
                if (dd.DropDown is ToolStripDropDownMenu menu)
                {
                    menu.Renderer = new ToolStripProfessionalRenderer(new ThemeColorTable(p)) { RoundedEdges = false };
                }

                foreach (ToolStripItem child in dd.DropDownItems)
                {
                    ApplyItem(child, p);
                }
            }
        }
    }

    /// <summary>controls that colour themselves (e.g. the map view)</summary>
    public interface IThemed
    {
        void ApplyTheme(ThemePalette palette);
    }

    internal sealed class ThemeColorTable : ProfessionalColorTable
    {
        private readonly ThemePalette _p;

        public ThemeColorTable(ThemePalette p)
        {
            _p = p;
            UseSystemColors = false;
        }

        public override Color ToolStripGradientBegin => _p.ToolBar;
        public override Color ToolStripGradientMiddle => _p.ToolBar;
        public override Color ToolStripGradientEnd => _p.ToolBar;
        public override Color ToolStripBorder => _p.Border;
        public override Color ToolStripDropDownBackground => _p.Panel;
        public override Color ImageMarginGradientBegin => _p.Panel;
        public override Color ImageMarginGradientMiddle => _p.Panel;
        public override Color ImageMarginGradientEnd => _p.Panel;
        public override Color MenuBorder => _p.Border;
        public override Color MenuItemBorder => _p.Accent;
        public override Color MenuItemSelected => _p.Selection;
        public override Color MenuItemSelectedGradientBegin => _p.Selection;
        public override Color MenuItemSelectedGradientEnd => _p.Selection;
        public override Color MenuItemPressedGradientBegin => _p.Selection;
        public override Color MenuItemPressedGradientEnd => _p.Selection;
        public override Color ButtonSelectedHighlight => _p.Selection;
        public override Color ButtonSelectedGradientBegin => _p.Selection;
        public override Color ButtonSelectedGradientMiddle => _p.Selection;
        public override Color ButtonSelectedGradientEnd => _p.Selection;
        public override Color ButtonSelectedBorder => _p.Accent;
        public override Color ButtonPressedGradientBegin => _p.Accent;
        public override Color ButtonPressedGradientMiddle => _p.Accent;
        public override Color ButtonPressedGradientEnd => _p.Accent;
        public override Color ButtonCheckedGradientBegin => _p.Selection;
        public override Color ButtonCheckedGradientMiddle => _p.Selection;
        public override Color ButtonCheckedGradientEnd => _p.Selection;
        public override Color CheckBackground => _p.Selection;
        public override Color CheckSelectedBackground => _p.Selection;
        public override Color SeparatorDark => _p.Border;
        public override Color SeparatorLight => _p.Border;
        public override Color StatusStripGradientBegin => _p.ToolBar;
        public override Color StatusStripGradientEnd => _p.ToolBar;
        public override Color GripDark => _p.Border;
        public override Color GripLight => _p.ToolBar;
    }
}

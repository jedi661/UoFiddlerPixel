// /***************************************************************************
//  *
//  * Dragon Map Studio – user interface language (Deutsch / English / Français)
//  *
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.DragonMapStudio.Core
{
    public enum UiLanguage
    {
        Deutsch,
        English,
        Francais
    }

    /// <summary>
    /// Translations of the German texts. The tables are text files in DragonData/Lang (en.txt, fr.txt):
    /// one line per text, "German&lt;TAB&gt;translation", "\n" for a line break – easy to extend.
    /// </summary>
    public static class L
    {
        private static Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.Ordinal);

        public static UiLanguage Current { get; private set; } = UiLanguage.Deutsch;

        public static event EventHandler Changed;

        public static string FileName(UiLanguage lang) => lang == UiLanguage.English ? "en.txt" : lang == UiLanguage.Francais ? "fr.txt" : null;

        /// <summary>Loads the table of the language (German = no table).</summary>
        public static void Load(UiLanguage lang)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            string file = FileName(lang);
            string folder = StudioPaths.DataFolder == null ? null : Path.Combine(StudioPaths.DataFolder, "Lang");
            if (file != null && folder != null && File.Exists(Path.Combine(folder, file)))
            {
                foreach (string line in File.ReadLines(Path.Combine(folder, file)))
                {
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    int tab = line.IndexOf('\t');
                    if (tab <= 0)
                    {
                        continue;
                    }
                    string de = Unescape(line.Substring(0, tab)), tr = Unescape(line.Substring(tab + 1));
                    if (tr.Length > 0)
                    {
                        map[de] = tr;
                    }
                }
            }
            _map = map;
            Current = lang;
            Changed?.Invoke(null, EventArgs.Empty);
        }

        private static string Unescape(string s) => s.Replace("\\n", "\n").Replace("\\t", "\t");

        /// <summary>Translation of a German text; unknown texts stay German. Multi-line texts are also translated line by line.</summary>
        public static string T(string de)
        {
            if (string.IsNullOrEmpty(de) || Current == UiLanguage.Deutsch)
            {
                return de;
            }
            if (_map.TryGetValue(de, out string tr))
            {
                return tr;
            }
            string norm = de.Replace("\r\n", "\n");
            if (norm.Contains('\n'))
            {
                return string.Join(Environment.NewLine, norm.Split('\n').Select(T));
            }
            return de;
        }

        /// <summary>German / English / French text chosen directly (for texts with values inside).</summary>
        public static string Pick(string de, string en, string fr) => Current == UiLanguage.English ? en : Current == UiLanguage.Francais ? fr : de;
    }

    /// <summary>
    /// Translates the texts of a form at run time: controls, menus, tool bars, tool tips, columns.
    /// The German original is remembered, so switching back works.
    /// </summary>
    public static class UiTranslator
    {
        private static readonly ConditionalWeakTable<object, Dictionary<string, (string de, string shown)>> Originals =
            new ConditionalWeakTable<object, Dictionary<string, (string de, string shown)>>();

        /// <summary>
        /// Translation of a property; the German original is remembered. When the program changed the text in the
        /// meantime (it is not the text set last time), the new text is the original.
        /// </summary>
        private static string Tr(object owner, string key, string current)
        {
            Dictionary<string, (string de, string shown)> d = Originals.GetOrCreateValue(owner);
            string de = d.TryGetValue(key, out var e) && e.shown == current ? e.de : current;
            string shown = L.T(de);
            d[key] = (de, shown);
            return shown;
        }

        public static void Apply(Control root, ToolTip tip = null)
        {
            if (root == null)
            {
                return;
            }
            Translate(root, tip);
        }

        private static void Translate(Control c, ToolTip tip)
        {
            // user input and data must not be translated
            bool skipText = c is TextBoxBase || c is ComboBox || c is NumericUpDown || c is ListBox || c is DataGridView || c is PropertyGrid;
            if (!skipText && !string.IsNullOrEmpty(c.Text))
            {
                c.Text = Tr(c, "Text", c.Text);
            }
            if (c is TextBox tb && !string.IsNullOrEmpty(tb.PlaceholderText))
            {
                tb.PlaceholderText = Tr(c, "Placeholder", tb.PlaceholderText);
            }
            if (tip != null)
            {
                string t = tip.GetToolTip(c);
                if (!string.IsNullOrEmpty(t))
                {
                    tip.SetToolTip(c, Tr(c, "Tip", t));
                }
            }
            if (c is ToolStrip ts)
            {
                foreach (ToolStripItem item in ts.Items)
                {
                    TranslateItem(item);
                }
            }
            if (c is ListView lv)
            {
                foreach (ColumnHeader h in lv.Columns)
                {
                    h.Text = Tr(h, "Text", h.Text);
                }
            }
            if (c is DataGridView grid)
            {
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    col.HeaderText = Tr(col, "Header", col.HeaderText);
                }
            }
            if (c.ContextMenuStrip != null)
            {
                foreach (ToolStripItem item in c.ContextMenuStrip.Items)
                {
                    TranslateItem(item);
                }
            }
            if (c is TabControl tc)
            {
                foreach (TabPage p in tc.TabPages)
                {
                    Translate(p, tip);
                }
            }
            foreach (Control child in c.Controls)
            {
                if (c is TabControl)
                {
                    continue; // done above
                }
                Translate(child, tip);
            }
        }

        private static void TranslateItem(ToolStripItem item)
        {
            bool input = item is ToolStripComboBox || item is ToolStripTextBox;
            if (!input && !string.IsNullOrEmpty(item.Text))
            {
                item.Text = Tr(item, "Text", item.Text);
            }
            if (!string.IsNullOrEmpty(item.ToolTipText))
            {
                item.ToolTipText = Tr(item, "Tip", item.ToolTipText);
            }
            if (item is ToolStripDropDownItem dd)
            {
                foreach (ToolStripItem sub in dd.DropDownItems)
                {
                    TranslateItem(sub);
                }
            }
        }
    }

    /// <summary>
    /// Translates DisplayName, Description and Category of the settings classes (property grids, grid columns).
    /// </summary>
    public sealed class LocalizedTypeProvider : TypeDescriptionProvider
    {
        private static readonly HashSet<Type> Registered = new HashSet<Type>();

        public LocalizedTypeProvider(Type type)
            : base(TypeDescriptor.GetProvider(type))
        {
        }

        /// <summary>Registers all classes of the studio that have German display names.</summary>
        public static void RegisterAll()
        {
            foreach (Type t in typeof(L).Assembly.GetTypes().Where(t => t.IsClass && t.Namespace != null && t.Namespace.StartsWith(typeof(L).Namespace ?? string.Empty, StringComparison.Ordinal)))
            {
                bool hasNames = t.GetProperties().Any(p => p.GetCustomAttributes(typeof(DisplayNameAttribute), true).Length > 0 || p.GetCustomAttributes(typeof(DescriptionAttribute), true).Length > 0);
                lock (Registered)
                {
                    if (hasNames && Registered.Add(t))
                    {
                        TypeDescriptor.AddProvider(new LocalizedTypeProvider(t), t);
                    }
                }
            }
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new Descriptor(base.GetTypeDescriptor(objectType, instance));
        }

        private sealed class Descriptor : CustomTypeDescriptor
        {
            public Descriptor(ICustomTypeDescriptor parent)
                : base(parent)
            {
            }

            public override PropertyDescriptorCollection GetProperties() => Wrap(base.GetProperties());

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) => Wrap(base.GetProperties(attributes));

            private static PropertyDescriptorCollection Wrap(PropertyDescriptorCollection props)
            {
                return new PropertyDescriptorCollection(props.Cast<PropertyDescriptor>().Select(p => (PropertyDescriptor)new LocalizedProperty(p)).ToArray(), true);
            }
        }

        private sealed class LocalizedProperty : PropertyDescriptor
        {
            private readonly PropertyDescriptor _inner;

            public LocalizedProperty(PropertyDescriptor inner)
                : base(inner)
            {
                _inner = inner;
            }

            public override string DisplayName => L.T(_inner.DisplayName);
            public override string Description => L.T(_inner.Description);
            public override string Category => L.T(_inner.Category);
            public override Type ComponentType => _inner.ComponentType;
            public override bool IsReadOnly => _inner.IsReadOnly;
            public override Type PropertyType => _inner.PropertyType;
            public override TypeConverter Converter => _inner.Converter;
            public override bool CanResetValue(object component) => _inner.CanResetValue(component);
            public override object GetValue(object component) => _inner.GetValue(component);
            public override void ResetValue(object component) => _inner.ResetValue(component);
            public override void SetValue(object component, object value) => _inner.SetValue(component, value);
            public override bool ShouldSerializeValue(object component) => _inner.ShouldSerializeValue(component);
            public override object GetEditor(Type editorBaseType) => _inner.GetEditor(editorBaseType);
        }
    }
}

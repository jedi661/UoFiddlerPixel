using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.ScriptCreator
{
    /// <summary>Simple syntax colors for C#, Sphere scripts and templates in a RichTextBox.</summary>
    internal static class CodeHighlighter
    {
        private static readonly Regex CsKeywords = new Regex(@"\b(using|namespace|public|private|protected|internal|static|readonly|override|virtual|abstract|sealed|class|struct|enum|interface|new|return|get|set|if|else|while|for|foreach|switch|case|break|base|this|void|int|bool|double|string|true|false|null|out|ref|params|var)\b", RegexOptions.Compiled);
        private static readonly Regex Strings = new Regex(@"""(?:[^""\\\r\n]|\\.)*""", RegexOptions.Compiled);
        private static readonly Regex Comments = new Regex(@"//[^\r\n]*", RegexOptions.Compiled);
        private static readonly Regex Numbers = new Regex(@"\b(0x[0-9A-Fa-f]+|\d+(\.\d+)?)\b", RegexOptions.Compiled);
        private static readonly Regex Attributes = new Regex(@"^\s*\[[^\]\r\n]+\]", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex Placeholders = new Regex(@"\{\{[^}]*\}\}", RegexOptions.Compiled);
        private static readonly Regex SphereKeys = new Regex(@"^\s*[A-Za-z_][\w\.]*(?==)", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex Header = new Regex(@"^@\w+", RegexOptions.Compiled | RegexOptions.Multiline);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WmSetRedraw = 0x0B;

        /// <summary>Sets the text and colors it. language: cs, scp or tpl (template).</summary>
        public static void Show(RichTextBox box, string text, string language)
        {
            int scroll = box.GetPositionFromCharIndex(0).Y;
            int caret = box.SelectionStart;
            SendMessage(box.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
            try
            {
                box.Text = text ?? string.Empty;
                box.SelectAll();
                box.SelectionColor = box.ForeColor;
                string t = box.Text;
                if (t.Length < 200000)
                {
                    if (language == "scp")
                    {
                        Color(box, Attributes.Matches(t), System.Drawing.Color.FromArgb(0, 102, 153), true);
                        Color(box, SphereKeys.Matches(t), System.Drawing.Color.FromArgb(128, 0, 128), false);
                    }
                    else
                    {
                        Color(box, CsKeywords.Matches(t), System.Drawing.Color.Blue, false);
                        Color(box, Numbers.Matches(t), System.Drawing.Color.FromArgb(9, 134, 88), false);
                        Color(box, Attributes.Matches(t), System.Drawing.Color.FromArgb(43, 145, 175), false);
                        Color(box, Strings.Matches(t), System.Drawing.Color.FromArgb(163, 21, 21), false);
                    }
                    Color(box, Comments.Matches(t), System.Drawing.Color.Green, false);
                    if (language == "tpl")
                    {
                        Color(box, Header.Matches(t), System.Drawing.Color.FromArgb(128, 0, 128), true);
                        Color(box, Placeholders.Matches(t), System.Drawing.Color.FromArgb(200, 90, 0), true);
                    }
                }
                box.Select(Math.Min(caret, box.TextLength), 0);
            }
            finally
            {
                SendMessage(box.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                box.Invalidate();
            }
        }

        private static void Color(RichTextBox box, MatchCollection matches, Color color, bool bold)
        {
            foreach (Match m in matches)
            {
                box.Select(m.Index, m.Length);
                box.SelectionColor = color;
                if (bold)
                {
                    box.SelectionFont = new Font(box.Font, FontStyle.Bold);
                }
            }
        }
    }

    /// <summary>Pictures from the loaded client files (all optional – null when not loaded).</summary>
    internal static class UoPreview
    {
        public static Bitmap Item(int id, int hue)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    if (!Ultima.Art.IsValidStatic(id))
                    {
                        return null;
                    }
                    Bitmap art = Ultima.Art.GetStatic(id);
                    return art == null ? null : Hued(art, hue, true);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Bitmap Body(int body, int hue)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    int h = 0;
                    Ultima.AnimationFrame[] frames = Ultima.Animations.GetAnimation(body, 0, 1, ref h, false, true);
                    if (frames == null || frames.Length == 0 || frames[0]?.Bitmap == null)
                    {
                        return null;
                    }
                    return Hued(frames[0].Bitmap, hue, false);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>The 32 colors of a hue (1-based number, as in scripts) or null.</summary>
        public static Color[] HueColors(int number, out string name)
        {
            name = null;
            try
            {
                Ultima.Hue[] list = Ultima.Hues.List;
                int n = number & 0x3FFF;
                if (list == null || n < 1 || n > list.Length)
                {
                    return null;
                }
                Ultima.Hue hue = list[n - 1];
                name = hue.Name?.Trim();
                return hue.Colors.Select(c => System.Drawing.Color.FromArgb(((c >> 10) & 0x1F) * 255 / 31, ((c >> 5) & 0x1F) * 255 / 31, (c & 0x1F) * 255 / 31)).ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Bitmap Hued(Bitmap src, int hue, bool onlyGray)
        {
            Bitmap copy = src.Clone(new Rectangle(0, 0, src.Width, src.Height), PixelFormat.Format16bppArgb1555);
            int n = hue & 0x3FFF;
            try
            {
                Ultima.Hue[] list = Ultima.Hues.List;
                if (n >= 1 && list != null && n <= list.Length)
                {
                    list[n - 1].ApplyTo(copy, onlyGray || (hue & 0x8000) != 0);
                }
            }
            catch (Exception)
            {
                // no hues
            }
            return copy;
        }

        public static void Draw(Graphics g, Rectangle area, Image img, string caption, Font font)
        {
            g.Clear(System.Drawing.Color.FromArgb(45, 45, 48));
            Rectangle box = new Rectangle(area.X + 3, area.Y + 3, area.Width - 6, area.Height - 20);
            if (img != null)
            {
                double scale = Math.Min(2, Math.Min(box.Width / (double)img.Width, box.Height / (double)img.Height));
                int w = Math.Max(1, (int)(img.Width * scale)), h = Math.Max(1, (int)(img.Height * scale));
                g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(img, new Rectangle(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h));
            }
            TextRenderer.DrawText(g, caption, font, new Rectangle(area.X, area.Bottom - 18, area.Width, 16), System.Drawing.Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Builds an input control for a template field and reads its value.</summary>
    internal sealed class FieldEditor
    {
        public TemplateField Field { get; }
        public Control Control { get; }
        private readonly Func<string> _get;
        private readonly Action<string> _set;

        public event EventHandler Changed;

        public string Value
        {
            get => _get();
            set => _set(value ?? string.Empty);
        }

        private void Raise() => Changed?.Invoke(this, EventArgs.Empty);

        public FieldEditor(TemplateField field, string value, ToolTip tip)
        {
            Field = field;
            string type = field.BaseType;
            switch (type)
            {
                case "bool":
                {
                    var c = new CheckBox { AutoSize = true, Checked = bool.TryParse(value, out bool b) && b, Margin = new Padding(3, 5, 3, 0) };
                    c.CheckedChanged += (s, e) => Raise();
                    Control = c;
                    _get = () => c.Checked ? "true" : "false";
                    _set = v => c.Checked = bool.TryParse(v, out bool x) && x;
                    break;
                }
                case "choice":
                case "combo":
                {
                    var c = new ComboBox
                    {
                        Width = 220,
                        DropDownStyle = type == "choice" ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown,
                        AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                        AutoCompleteSource = AutoCompleteSource.ListItems,
                        Margin = new Padding(3, 2, 3, 0)
                    };
                    c.Items.AddRange(ScriptLists.Resolve(field.Options).Cast<object>().ToArray());
                    void SetC(string v)
                    {
                        int i = c.FindStringExact(v);
                        if (i >= 0)
                        {
                            c.SelectedIndex = i;
                        }
                        else if (type == "combo")
                        {
                            c.Text = v;
                        }
                        else if (c.Items.Count > 0 && c.SelectedIndex < 0)
                        {
                            c.SelectedIndex = 0;
                        }
                    }
                    SetC(value);
                    c.TextChanged += (s, e) => Raise();
                    c.SelectedIndexChanged += (s, e) => Raise();
                    Control = c;
                    _get = () => c.Text.Trim();
                    _set = SetC;
                    break;
                }
                case "list":
                    Control = BuildList(field, value, out _get, out _set);
                    break;
                case "text":
                case "pages":
                {
                    var t = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Width = 320, Height = type == "pages" ? 160 : 70, Text = value.Replace("\r\n", "\n").Replace("\n", "\r\n"), AcceptsReturn = true, Margin = new Padding(3, 2, 3, 0) };
                    t.TextChanged += (s, e) => Raise();
                    Control = t;
                    _get = () => t.Text.Replace("\r\n", "\n");
                    _set = v => t.Text = v.Replace("\r\n", "\n").Replace("\n", "\r\n");
                    break;
                }
                case "itemid":
                case "hue":
                case "body":
                {
                    var t = new TextBox { Width = 90, Text = value, Margin = new Padding(3, 2, 3, 0) };
                    var pic = new Panel { Width = type == "hue" ? 128 : 44, Height = type == "hue" ? 16 : 44, Margin = new Padding(3, 2, 3, 0) };
                    pic.Paint += (s, e) => PaintSmall(e.Graphics, pic.ClientRectangle, type, t.Text);
                    t.TextChanged += (s, e) => { pic.Invalidate(); Raise(); };
                    var flow = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
                    flow.Controls.Add(t);
                    flow.Controls.Add(pic);
                    Control = flow;
                    _get = () => t.Text.Trim();
                    _set = v => t.Text = v;
                    break;
                }
                default:
                {
                    var t = new TextBox { Width = type == "class" || type == "string" ? 220 : 110, Text = value, Margin = new Padding(3, 2, 3, 0) };
                    t.TextChanged += (s, e) => Raise();
                    Control = t;
                    _get = () => t.Text.Trim();
                    _set = v => t.Text = v;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(field.Tip))
            {
                tip.SetToolTip(Control, field.Tip);
            }
        }

        private static void PaintSmall(Graphics g, Rectangle r, string type, string text)
        {
            g.Clear(Color.FromArgb(45, 45, 48));
            if (!ScriptTemplate.TryParseInt(text, out long v) || v < 0 || v > 0xFFFF)
            {
                return;
            }
            if (type == "hue")
            {
                Color[] colors = UoPreview.HueColors((int)v, out _);
                if (colors == null)
                {
                    return;
                }
                float w = r.Width / 32f;
                for (int i = 0; i < 32; i++)
                {
                    using (var b = new SolidBrush(colors[i]))
                    {
                        g.FillRectangle(b, i * w, 0, w + 1, r.Height);
                    }
                }
                return;
            }
            using (Bitmap img = type == "body" ? UoPreview.Body((int)v, 0) : UoPreview.Item((int)v, 0))
            {
                if (img == null)
                {
                    return;
                }
                double s = Math.Min(1, Math.Min(r.Width / (double)img.Width, r.Height / (double)img.Height));
                int w = (int)(img.Width * s), h = (int)(img.Height * s);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, (r.Width - w) / 2, (r.Height - h) / 2, w, h);
            }
        }

        /// <summary>Grid with name (from the list) and value; rows can be added and removed.</summary>
        private Control BuildList(TemplateField field, string value, out Func<string> get, out Action<string> set)
        {
            var grid = new DataGridView
            {
                Width = 330,
                Height = 130,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                RowHeadersWidth = 24,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                Margin = new Padding(3, 2, 3, 0),
                EditMode = DataGridViewEditMode.EditOnEnter
            };
            string[] names = ScriptLists.Resolve(field.Options);
            DataGridViewColumn nameCol;
            if (names.Length > 0)
            {
                var combo = new DataGridViewComboBoxColumn { HeaderText = "Name", FillWeight = 60, FlatStyle = FlatStyle.Flat, DisplayStyleForCurrentCellOnly = true };
                combo.Items.AddRange(names.Cast<object>().ToArray());
                nameCol = combo;
            }
            else
            {
                nameCol = new DataGridViewTextBoxColumn { HeaderText = "Name", FillWeight = 60 };
            }
            grid.Columns.Add(nameCol);
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Value", FillWeight = 40 });
            grid.DataError += (s, e) => e.ThrowException = false;

            bool loading = false;
            void Fill(string v)
            {
                loading = true;
                grid.Rows.Clear();
                foreach (KeyValuePair<string, string> kv in ScriptTemplate.ParseList(v))
                {
                    if (nameCol is DataGridViewComboBoxColumn cc && !cc.Items.Contains(kv.Key))
                    {
                        cc.Items.Add(kv.Key);
                    }
                    grid.Rows.Add(kv.Key, kv.Value);
                }
                loading = false;
            }
            Fill(value);
            get = () => ScriptTemplate.JoinList(grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow)
                .Select(r => new KeyValuePair<string, string>(Convert.ToString(r.Cells[0].Value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                    Convert.ToString(r.Cells[1].Value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty)));
            set = Fill;
            grid.CellValueChanged += (s, e) => { if (!loading) Raise(); };
            grid.RowsRemoved += (s, e) => { if (!loading) Raise(); };
            grid.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            return grid;
        }
    }
}

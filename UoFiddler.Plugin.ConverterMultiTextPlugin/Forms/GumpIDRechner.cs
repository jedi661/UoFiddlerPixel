// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GumpCalc;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// UO calculator: paperdoll gump IDs (male = AnimID + 50000, female = AnimID + 60000) with preview, wearables list,
    /// body / animation index, hues and colors, tiledata flags, map blocks and sextant, layers / art index and a number
    /// converter with formulas. Every result can be copied and goes into the history.
    /// </summary>
    public partial class GumpIDRechner : Form
    {
        private static readonly Color PreviewBack = Color.FromArgb(45, 45, 48);

        public GumpIDRechner()
        {
            InitializeComponent();
            BuildHistory();
            BuildGumpTab();
            BuildWearablesTab();
            BuildBodyTab();
            BuildHueTab();
            BuildFlagsTab();
            BuildMapTab();
            BuildLayerTab();
            BuildConvertTab();
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabWearables && _wearables == null)
                {
                    LoadWearables();
                }
            };
            SetMessage("Type an item ID, AnimID or gump ID in the Gump ID tab. Double click any result to copy it. F1 = help.");
        }

        #region Helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text, int width = 0) => width > 0
            ? new Label { Text = text, Width = width, Margin = new Padding(3, 7, 3, 0) }
            : new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private Button Btn(string text, EventHandler click, string tip = null, int width = 0)
        {
            var b = new Button { Text = text, AutoSize = width == 0, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            if (width > 0)
            {
                b.Width = width;
            }
            b.Click += click;
            if (tip != null)
            {
                toolTip1.SetToolTip(b, tip);
            }
            return b;
        }

        private static NumericUpDown Num(decimal min, decimal max, decimal value, int width = 70, bool hex = false) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = width, Hexadecimal = hex, Margin = new Padding(3, 3, 3, 0) };

        private static CheckBox Chk(string text, bool value = false) => new CheckBox { Text = text, AutoSize = true, Checked = value, Margin = new Padding(3, 5, 3, 0) };

        private static TextBox In(int width = 110, string text = "") => new TextBox { Width = width, Text = text, Margin = new Padding(3, 4, 3, 0) };

        private static ComboBox Combo(int width, params object[] items)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = width, Margin = new Padding(3, 4, 3, 0) };
            c.Items.AddRange(items);
            if (items.Length > 0)
            {
                c.SelectedIndex = 0;
            }
            return c;
        }

        /// <summary>Read-only result box: double click copies the value.</summary>
        private TextBox Out(int width = 110)
        {
            var t = new TextBox { Width = width, ReadOnly = true, BackColor = SystemColors.Info, Margin = new Padding(3, 4, 3, 0), Cursor = Cursors.Hand };
            t.DoubleClick += (s, e) => Copy(t.Text);
            toolTip1.SetToolTip(t, Tip("Double click = copy", "Doppelklick = kopieren"));
            return t;
        }

        private static GroupBox Group(string title, params Control[] rows)
        {
            var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 4, 6, 6), Margin = new Padding(3, 3, 3, 6) };
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Fill };
            flow.Controls.AddRange(rows);
            box.Controls.Add(flow);
            return box;
        }

        /// <summary>Two columns of groups that scroll together.</summary>
        private static void FillTab(TabPage page, Control[] left, Control[] right = null)
        {
            var outer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, AutoScroll = true };
            var col1 = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
            col1.Controls.AddRange(left);
            outer.Controls.Add(col1);
            if (right != null)
            {
                var col2 = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true };
                col2.Controls.AddRange(right);
                outer.Controls.Add(col2);
            }
            page.Controls.Add(outer);
        }

        private static Panel Preview(int width, int height, PaintEventHandler paint)
        {
            var p = new DoubleBufferedPanel { Width = width, Height = height, BackColor = PreviewBack, Margin = new Padding(3) };
            p.Paint += paint;
            return p;
        }

        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        /// <summary>Draws an image centered and scaled down (never up more than maxScale) with a caption.</summary>
        private void DrawPreview(Graphics g, Rectangle area, Image img, string caption, Color captionColor, int maxScale = 2)
        {
            Rectangle box = new Rectangle(area.X + 4, area.Y + 4, area.Width - 8, area.Height - 26);
            if (img != null && img.Width > 0 && img.Height > 0)
            {
                double scale = Math.Min(maxScale, Math.Min(box.Width / (double)img.Width, box.Height / (double)img.Height));
                int w = Math.Max(1, (int)(img.Width * scale)), h = Math.Max(1, (int)(img.Height * scale));
                g.InterpolationMode = scale >= 1 ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(img, new Rectangle(box.X + (box.Width - w) / 2, box.Y + (box.Height - h) / 2, w, h));
            }
            TextRenderer.DrawText(g, caption, Font, new Rectangle(area.X, area.Bottom - 22, area.Width, 20), captionColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
        }

        private void Copy(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            try
            {
                Clipboard.SetText(text);
                SetMessage("Copied: " + (text.Length > 80 ? text.Substring(0, 80) + "…" : text.Replace("\r\n", " | ")));
            }
            catch (Exception ex)
            {
                SetMessage("Copy failed: " + ex.Message);
            }
        }

        private static ItemData? Item(int id)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    return table != null && id >= 0 && id < table.Length ? table[id] : (ItemData?)null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool GumpExists(int id)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return id >= 0 && Gumps.IsValidIndex(id);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Bitmap GetGump(int id)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return id >= 0 && Gumps.IsValidIndex(id) ? Gumps.GetGump(id) : null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int GumpCount()
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return Gumps.GetCount();
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static Bitmap GetItemArt(int id)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return Art.IsValidStatic(id) ? Art.GetStatic(id) : null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Hue GetHueEntry(int number)
        {
            try
            {
                Hue[] list = Ultima.Hues.List;
                return list != null && number >= 1 && number <= list.Length ? list[number - 1] : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>Copy of the image with a hue applied (number 0 = no hue, the original is returned).</summary>
        private static Bitmap Hued(Bitmap src, int hueNumber, bool onlyGray, out bool owned)
        {
            owned = false;
            Hue hue = GetHueEntry(hueNumber);
            if (src == null || hue == null)
            {
                return src;
            }
            Bitmap copy;
            lock (SdkSync.Lock)
            {
                copy = src.Clone(new Rectangle(0, 0, src.Width, src.Height), PixelFormat.Format16bppArgb1555);
            }
            hue.ApplyTo(copy, onlyGray);
            owned = true;
            return copy;
        }

        private static void SetCombo(ComboBox c, int index)
        {
            if (index >= 0 && index < c.Items.Count)
            {
                c.SelectedIndex = index;
            }
        }

        #endregion

        #region History

        private ListBox _history;

        private void BuildHistory()
        {
            var title = new Label { Text = "History / Verlauf", Dock = DockStyle.Top, Height = 22, Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            _history = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, HorizontalScrollbar = true, Font = new Font("Consolas", 9F) };
            _history.DoubleClick += (s, e) => Copy(_history.SelectedItem as string);
            toolTip1.SetToolTip(_history, Tip("Every calculation lands here. Double click = copy.", "Jede Berechnung landet hier. Doppelklick = kopieren."));
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 64, WrapContents = true };
            buttons.Controls.Add(Btn("Copy", (s, e) => Copy(_history.SelectedItem as string)));
            buttons.Controls.Add(Btn("Copy all", (s, e) => Copy(string.Join("\r\n", _history.Items.Cast<string>()))));
            buttons.Controls.Add(Btn("Save...", (s, e) => SaveHistory()));
            buttons.Controls.Add(Btn("Clear", (s, e) => _history.Items.Clear()));
            buttons.Controls.Add(Btn("?", (s, e) => ShowHelp(), Tip("Help (F1)", "Hilfe (F1)")));
            splitContainer1.Panel2.Controls.Add(_history);
            splitContainer1.Panel2.Controls.Add(buttons);
            splitContainer1.Panel2.Controls.Add(title);
        }

        private void AddHistory(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }
            if (_history.Items.Count > 0 && (string)_history.Items[0] == line)
            {
                return;
            }
            _history.Items.Insert(0, line);
            while (_history.Items.Count > 300)
            {
                _history.Items.RemoveAt(_history.Items.Count - 1);
            }
        }

        private void SaveHistory()
        {
            if (_history.Items.Count == 0)
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "Text (*.txt)|*.txt", FileName = "UO_calculations.txt" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    File.WriteAllLines(d.FileName, _history.Items.Cast<string>().Reverse());
                    SetMessage("History saved: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Gump tab

        private ComboBox _gumpKind, _gumpStyle, _gumpItems, _scriptKind;
        private TextBox _gumpInput, _gItemHex, _gItemDec, _gArtIndex, _gName, _gLayer, _gAnimHex, _gAnimDec, _gMaleHex, _gMaleDec, _gFemaleHex, _gFemaleDec, _script;
        private Label _gumpRead, _gumpState;
        private CheckBox _chkFemaleOnly, _chkOnBody, _chkGumpPartial;
        private NumericUpDown _numGumpHue, _numFreeFrom;
        private Panel _pvItem, _pvMale, _pvFemale;
        private int _gItem = -1, _gAnim = -1, _gLayerValue;
        private string _gNameValue = string.Empty;
        private bool _gumpUpdating;

        private void BuildGumpTab()
        {
            _gumpKind = Combo(150, "Item ID", "AnimID (tiledata)", "Gump ID (male / female)");
            _gumpStyle = Combo(70, "Auto", "Hex", "Decimal");
            _gumpInput = In(110);
            toolTip1.SetToolTip(_gumpInput, Tip("Number: 0x1517, 1517h or 5399. \"Auto\": letters A-F or 0x = hex, otherwise decimal.",
                "Zahl: 0x1517, 1517h oder 5399. \"Auto\": Buchstaben A-F oder 0x = Hex, sonst Dezimal."));
            toolTip1.SetToolTip(_gumpStyle, Tip("How a plain number without 0x is read. The old tool always read hex.", "Wie eine Zahl ohne 0x gelesen wird. Das alte Tool las immer Hex."));
            _gumpRead = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 0) };
            _gumpInput.TextChanged += (s, e) => CalcGump();
            _gumpKind.SelectedIndexChanged += (s, e) => CalcGump();
            _gumpStyle.SelectedIndexChanged += (s, e) => CalcGump();
            _gumpInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AddGumpHistory();
                }
            };
            _chkFemaleOnly = Chk("Female-only item (female gump at +50000)");
            toolTip1.SetToolTip(_chkFemaleOnly, Tip("Items that only exist for women (e.g. female leather) often use the male offset +50000.",
                "Items, die es nur für Frauen gibt (z. B. Damen-Leder), benutzen oft den Männer-Offset +50000."));
            _chkFemaleOnly.CheckedChanged += (s, e) => CalcGump();

            _gItemHex = Out(80); _gItemDec = Out(70); _gArtIndex = Out(80); _gName = Out(230); _gLayer = Out(160);
            _gAnimHex = Out(80); _gAnimDec = Out(70); _gMaleHex = Out(80); _gMaleDec = Out(70); _gFemaleHex = Out(80); _gFemaleDec = Out(70);
            _gumpItems = Combo(230);
            _gumpItems.SelectedIndexChanged += (s, e) =>
            {
                if (!_gumpUpdating && _gumpItems.SelectedItem is ItemChoice c)
                {
                    ShowGump(c.Id, _gAnim);
                }
            };
            toolTip1.SetToolTip(_gumpItems, Tip("All wearable items that use this AnimID.", "Alle tragbaren Items, die diese AnimID benutzen."));
            _gumpState = new Label { AutoSize = true, MaximumSize = new Size(420, 0), Margin = new Padding(3, 6, 3, 0) };

            GroupBox input = Group("Input",
                Row(_gumpKind, _gumpInput, _gumpStyle, Btn("→ History", (s, e) => AddGumpHistory(), Tip("Store the result (Enter)", "Ergebnis merken (Enter)"))),
                _gumpRead, _chkFemaleOnly);
            GroupBox results = Group("Result (male = AnimID + 50000, female = AnimID + 60000)",
                Row(Lbl("Item ID:", 85), _gItemHex, _gItemDec, Lbl("Art index:"), _gArtIndex),
                Row(Lbl("Name:", 85), _gName),
                Row(Lbl("Layer:", 85), _gLayer),
                Row(Lbl("Items:", 85), _gumpItems),
                Row(Lbl("AnimID:", 85), _gAnimHex, _gAnimDec),
                Row(Lbl("Gump male:", 85), _gMaleHex, _gMaleDec),
                Row(Lbl("Gump female:", 85), _gFemaleHex, _gFemaleDec),
                _gumpState,
                Row(Btn("Copy all", (s, e) => Copy(UoCalc.MakeScript(4, Math.Max(0, _gItem), _gNameValue, _gLayerValue, Math.Max(0, _gAnim), 0)))));

            _pvItem = Preview(130, 190, (s, e) => PaintGumpPreview(e.Graphics, 0));
            _pvMale = Preview(200, 260, (s, e) => PaintGumpPreview(e.Graphics, 1));
            _pvFemale = Preview(200, 260, (s, e) => PaintGumpPreview(e.Graphics, 2));
            _chkOnBody = Chk("On body (gump 0x0C / 0x0D)", true);
            _chkOnBody.CheckedChanged += (s, e) => InvalidateGumpPreviews();
            _numGumpHue = Num(0, 3000, 0, 60);
            _numGumpHue.ValueChanged += (s, e) => InvalidateGumpPreviews();
            _chkGumpPartial = Chk("only gray (partial hue)", true);
            _chkGumpPartial.CheckedChanged += (s, e) => InvalidateGumpPreviews();
            GroupBox preview = Group("Preview: item · male · female",
                Row(_pvItem, _pvMale, _pvFemale),
                Row(_chkOnBody, Lbl("Hue:"), _numGumpHue, _chkGumpPartial));

            _numFreeFrom = Num(0, 0xFFFF, 0x200, 70, true);
            GroupBox free = Group("New clothing: free AnimID",
                Row(Lbl("Search from AnimID 0x"), _numFreeFrom, Btn("Find free", (s, e) => FindFreeAnim(), Tip(
                    "Finds the next AnimID where no item uses it and both gumps (50000+ and 60000+) are empty.",
                    "Sucht die nächste AnimID, die kein Item benutzt und deren beide Gumps (50000+ und 60000+) leer sind."))));

            _scriptKind = Combo(170, UoCalc.ScriptKinds);
            _scriptKind.SelectedIndexChanged += (s, e) => UpdateScript();
            _script = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Width = 420, Height = 150, Font = new Font("Consolas", 9F), BackColor = SystemColors.Window };
            GroupBox script = Group("Script output",
                Row(_scriptKind, Btn("Copy", (s, e) => Copy(_script.Text))),
                _script);

            FillTab(tabGump, new Control[] { input, results, free }, new Control[] { preview, script });
            CalcGump();
        }

        private sealed class ItemChoice
        {
            public ItemChoice(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; }
            public string Name { get; }
            public override string ToString() => $"0x{Id:X4} {Name}";
        }

        private NumberStyle GumpStyle => (NumberStyle)Math.Max(0, _gumpStyle.SelectedIndex);

        private void CalcGump()
        {
            if (_gumpInput == null)
            {
                return;
            }
            string text = _gumpInput.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                _gumpRead.Text = "Example: item 0x1517 (shirt), AnimID 435, gump 0xC4B3";
                ShowGump(-1, -1);
                return;
            }
            if (!UoCalc.TryParse(text, GumpStyle, out long value) || value < 0 || value > 0xFFFFF)
            {
                _gumpRead.Text = "⚠ not a valid number";
                _gumpRead.ForeColor = Color.Firebrick;
                ShowGump(-1, -1);
                return;
            }
            int v = (int)value;
            _gumpRead.ForeColor = Color.DimGray;
            _gumpRead.Text = $"read as {UoCalc.Hex(v)} = {v}";
            switch (_gumpKind.SelectedIndex)
            {
                case 0:
                {
                    ItemData? item = Item(v);
                    if (item == null)
                    {
                        _gumpRead.Text += "   (tiledata not loaded or ID too large – AnimID unknown)";
                        ShowGump(v, -1);
                        return;
                    }
                    ShowGump(v, item.Value.Animation);
                    break;
                }
                case 1:
                    ShowGump(-1, v);
                    break;
                default:
                {
                    int gender = UoCalc.GumpGender(v, out int anim);
                    if (gender == 0)
                    {
                        _gumpRead.Text += "   – below 50000: not a paperdoll gump (no AnimID)";
                        ShowGump(-1, -1);
                        return;
                    }
                    _gumpRead.Text += gender == 1 ? "   – male gump" : "   – female gump";
                    ShowGump(-1, anim);
                    break;
                }
            }
        }

        private void ShowGump(int itemId, int anim)
        {
            _gumpUpdating = true;
            _gAnim = anim;
            List<ItemChoice> users = anim >= 0 ? ItemsUsingAnim(anim) : new List<ItemChoice>();
            if (itemId < 0 && users.Count > 0)
            {
                itemId = users[0].Id;
            }
            _gItem = itemId;
            _gumpItems.Items.Clear();
            foreach (ItemChoice c in users)
            {
                _gumpItems.Items.Add(c);
            }
            int sel = users.FindIndex(c => c.Id == itemId);
            if (sel >= 0)
            {
                _gumpItems.SelectedIndex = sel;
            }
            _gumpUpdating = false;

            ItemData? item = itemId >= 0 ? Item(itemId) : null;
            _gNameValue = item?.Name?.Trim() ?? string.Empty;
            _gLayerValue = item?.Quality ?? 0;
            _gItemHex.Text = itemId >= 0 ? UoCalc.Hex(itemId) : string.Empty;
            _gItemDec.Text = itemId >= 0 ? itemId.ToString() : string.Empty;
            _gArtIndex.Text = itemId >= 0 ? UoCalc.Hex(UoCalc.ItemToArtIndex(itemId)) : string.Empty;
            _gName.Text = _gNameValue;
            _gLayer.Text = item != null ? $"{_gLayerValue} (0x{_gLayerValue:X2}) {UoCalc.LayerName(_gLayerValue)}" : string.Empty;
            _gAnimHex.Text = anim >= 0 ? UoCalc.Hex(anim) : string.Empty;
            _gAnimDec.Text = anim >= 0 ? anim.ToString() : string.Empty;
            int male = anim >= 0 ? UoCalc.MaleGump(anim) : -1;
            int female = anim >= 0 ? (_chkFemaleOnly.Checked ? male : UoCalc.FemaleGump(anim)) : -1;
            _gMaleHex.Text = male >= 0 ? UoCalc.Hex(male) : string.Empty;
            _gMaleDec.Text = male >= 0 ? male.ToString() : string.Empty;
            _gFemaleHex.Text = female >= 0 ? UoCalc.Hex(female) : string.Empty;
            _gFemaleDec.Text = female >= 0 ? female.ToString() : string.Empty;

            var state = new StringBuilder();
            Color stateColor = Color.DarkGreen;
            if (anim >= 0)
            {
                bool wearable = item != null && (item.Value.Flags & TileFlag.Wearable) != 0;
                if (item != null && !wearable)
                {
                    state.AppendLine("⚠ The item has no \"Wearable\" flag – it has no paperdoll gump.");
                    stateColor = Color.DarkOrange;
                }
                if (anim == 0 && item != null)
                {
                    state.AppendLine("⚠ AnimID is 0 – set \"Animation\" in tiledata first.");
                    stateColor = Color.DarkOrange;
                }
                int count = GumpCount();
                bool maleOk = GumpExists(male), femaleOk = GumpExists(female);
                state.AppendLine(maleOk ? "✓ male gump exists" : "✗ male gump missing");
                if (_chkFemaleOnly.Checked)
                {
                    state.AppendLine("female-only item: female gump = male gump (+50000)");
                }
                else
                {
                    state.AppendLine(femaleOk ? "✓ female gump exists" : maleOk ? "female gump missing → the client shows the male gump" : "✗ female gump missing");
                }
                if (!maleOk)
                {
                    stateColor = Color.Firebrick;
                }
                if (count > 0 && Math.Max(male, female) >= count)
                {
                    state.AppendLine($"⚠ gump {Math.Max(male, female)} is beyond the end of gumpidx ({count} entries) – the file must be extended.");
                    stateColor = Color.Firebrick;
                }
                if (Math.Max(male, female) > 0xFFFF)
                {
                    state.AppendLine("⚠ above 0xFFFF: only clients with extended gump files show it.");
                    stateColor = Color.Firebrick;
                }
                if (users.Count > 1)
                {
                    state.AppendLine($"{users.Count} wearable items share this AnimID.");
                }
            }
            _gumpState.Text = state.ToString().TrimEnd();
            _gumpState.ForeColor = stateColor;
            InvalidateGumpPreviews();
            UpdateScript();
        }

        private void AddGumpHistory()
        {
            if (_gAnim < 0)
            {
                return;
            }
            string item = _gItem >= 0 ? $"item {UoCalc.Hex(_gItem)} {_gNameValue} → " : string.Empty;
            AddHistory($"Gump: {item}AnimID {_gAnim} ({UoCalc.Hex(_gAnim)}) → male {_gMaleDec.Text} ({_gMaleHex.Text}), female {_gFemaleDec.Text} ({_gFemaleHex.Text})");
        }

        private static List<ItemChoice> ItemsUsingAnim(int anim)
        {
            var list = new List<ItemChoice>();
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    if (table == null)
                    {
                        return list;
                    }
                    for (int i = 0; i < table.Length; i++)
                    {
                        if (table[i].Animation == anim && (table[i].Flags & TileFlag.Wearable) != 0)
                        {
                            list.Add(new ItemChoice(i, table[i].Name?.Trim()));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // tiledata missing
            }
            return list;
        }

        private void InvalidateGumpPreviews()
        {
            _pvItem?.Invalidate();
            _pvMale?.Invalidate();
            _pvFemale?.Invalidate();
        }

        private void PaintGumpPreview(Graphics g, int which)
        {
            Panel panel = which == 0 ? _pvItem : which == 1 ? _pvMale : _pvFemale;
            Rectangle area = panel.ClientRectangle;
            int hue = (int)_numGumpHue.Value;
            bool gray = _chkGumpPartial.Checked;
            if (which == 0)
            {
                Bitmap art = _gItem >= 0 ? GetItemArt(_gItem) : null;
                Bitmap shown = Hued(art, hue, gray, out bool owned);
                DrawPreview(g, area, shown, art == null ? "no item" : "item " + UoCalc.Hex(_gItem), Color.Gainsboro);
                if (owned)
                {
                    shown.Dispose();
                }
                return;
            }
            if (_gAnim < 0)
            {
                DrawPreview(g, area, null, which == 1 ? "male" : "female", Color.Gray);
                return;
            }
            int male = UoCalc.MaleGump(_gAnim);
            int id = which == 1 || _chkFemaleOnly.Checked ? male : UoCalc.FemaleGump(_gAnim);
            Bitmap gump = GetGump(id);
            string caption = $"{(which == 1 ? "male" : "female")} {id}";
            Color color = Color.LightGreen;
            if (gump == null && which == 2)
            {
                gump = GetGump(male);
                caption = gump != null ? $"female missing → male {male}" : $"female {id} missing";
                color = gump != null ? Color.Orange : Color.IndianRed;
            }
            else if (gump == null)
            {
                caption += " missing";
                color = Color.IndianRed;
            }
            Bitmap item = Hued(gump, hue, gray, out bool ownedItem);
            Bitmap body = _chkOnBody.Checked ? GetGump(which == 1 ? 0x0C : 0x0D) : null;
            if (body != null && item != null)
            {
                using (var composite = new Bitmap(Math.Max(body.Width, item.Width), Math.Max(body.Height, item.Height)))
                {
                    using (Graphics cg = Graphics.FromImage(composite))
                    {
                        cg.DrawImage(body, 0, 0, body.Width, body.Height);
                        cg.DrawImage(item, 0, 0, item.Width, item.Height);
                    }
                    DrawPreview(g, area, composite, caption, color, 1);
                }
            }
            else
            {
                DrawPreview(g, area, item, caption, color, 1);
            }
            if (ownedItem)
            {
                item.Dispose();
            }
        }

        private void UpdateScript()
        {
            if (_script == null)
            {
                return;
            }
            if (_gAnim < 0)
            {
                _script.Text = string.Empty;
                return;
            }
            string name = string.IsNullOrEmpty(_gNameValue) ? "new item" : _gNameValue;
            _script.Text = UoCalc.MakeScript(_scriptKind.SelectedIndex, Math.Max(0, _gItem), name, _gLayerValue, _gAnim, (int)_numGumpHue.Value)
                .Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        private void FindFreeAnim()
        {
            var used = new HashSet<int>();
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    if (table != null)
                    {
                        foreach (ItemData d in table)
                        {
                            if ((d.Flags & TileFlag.Wearable) != 0)
                            {
                                used.Add(d.Animation);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // tiledata missing
            }
            if (GumpCount() == 0)
            {
                SetMessage("Gumps are not loaded – cannot check free places.");
                return;
            }
            for (int a = Math.Max(1, (int)_numFreeFrom.Value); a <= 0xFFFF - UoCalc.FemaleGumpOffset; a++)
            {
                if (!used.Contains(a) && !GumpExists(UoCalc.MaleGump(a)) && !GumpExists(UoCalc.FemaleGump(a)))
                {
                    _numFreeFrom.Value = a + 1;
                    SetCombo(_gumpKind, 1);
                    _gumpInput.Text = "0x" + a.ToString("X");
                    AddHistory($"Free AnimID {a} (0x{a:X}) → male gump {UoCalc.MaleGump(a)}, female gump {UoCalc.FemaleGump(a)}");
                    SetMessage($"Free: AnimID {a} (0x{a:X}). Put the gumps into {UoCalc.MaleGump(a)} and {UoCalc.FemaleGump(a)} and set Animation = {a} in tiledata.");
                    return;
                }
            }
            SetMessage("No free AnimID found up to 5535 (female gump would be above 65535).");
        }

        /// <summary>Opens an item in the Gump ID tab (used by other tabs).</summary>
        private void OpenInGumpTab(int itemId)
        {
            tabControl1.SelectedTab = tabGump;
            SetCombo(_gumpKind, 0);
            SetCombo(_gumpStyle, 0);
            _gumpInput.Text = UoCalc.Hex(itemId);
        }

        #endregion

        #region Wearables tab

        private sealed class Wearable
        {
            public int Id;
            public string Name;
            public int Layer;
            public int Anim;
            public bool Male;
            public bool Female;
        }

        private List<Wearable> _wearables;
        private ListView _listWear;
        private TextBox _wearFilter;
        private ComboBox _wearLayer, _wearState;
        private Label _wearCount;

        private void BuildWearablesTab()
        {
            _wearFilter = In(150);
            _wearFilter.TextChanged += (s, e) => FillWearables();
            toolTip1.SetToolTip(_wearFilter, Tip("Part of the name, or an ID (item, AnimID or gump).", "Teil des Namens oder eine ID (Item, AnimID oder Gump)."));
            _wearLayer = Combo(150, new object[] { "All layers" }.Concat(UoCalc.Layers.Skip(1).Select(l => (object)$"{l.Id} {l.Name}")).ToArray());
            _wearLayer.SelectedIndexChanged += (s, e) => FillWearables();
            _wearState = Combo(170, "All", "Male gump missing", "Female gump missing", "Both gumps exist", "Both missing");
            _wearState.SelectedIndexChanged += (s, e) => FillWearables();
            _wearCount = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
            _listWear = new ListView { View = View.Details, FullRowSelect = true, HideSelection = false, Dock = DockStyle.Fill, GridLines = true };
            _listWear.Columns.Add("Item ID", 70);
            _listWear.Columns.Add("Name", 170);
            _listWear.Columns.Add("Layer", 120);
            _listWear.Columns.Add("AnimID", 90);
            _listWear.Columns.Add("Gump male", 100);
            _listWear.Columns.Add("Gump female", 100);
            _listWear.Columns.Add("State", 170);
            _listWear.DoubleClick += (s, e) =>
            {
                if (_listWear.SelectedItems.Count > 0 && _listWear.SelectedItems[0].Tag is Wearable w)
                {
                    OpenInGumpTab(w.Id);
                }
            };
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
            top.Controls.AddRange(new Control[]
            {
                Lbl("Filter:"), _wearFilter, _wearLayer, _wearState,
                Btn("Reload", (s, e) => LoadWearables()), Btn("Export CSV...", (s, e) => ExportWearables()), _wearCount
            });
            var hint = new Label { Dock = DockStyle.Bottom, Height = 20, ForeColor = Color.DimGray, Text = "Double click = open in the Gump ID tab. / Doppelklick = im Reiter Gump ID öffnen." };
            tabWearables.Controls.Add(_listWear);
            tabWearables.Controls.Add(top);
            tabWearables.Controls.Add(hint);
        }

        private void LoadWearables()
        {
            Cursor = Cursors.WaitCursor;
            var list = new List<Wearable>();
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    if (table != null)
                    {
                        for (int i = 0; i < table.Length; i++)
                        {
                            if ((table[i].Flags & TileFlag.Wearable) == 0)
                            {
                                continue;
                            }
                            int anim = table[i].Animation;
                            list.Add(new Wearable
                            {
                                Id = i,
                                Name = table[i].Name?.Trim(),
                                Layer = table[i].Quality,
                                Anim = anim,
                                Male = Gumps.IsValidIndex(UoCalc.MaleGump(anim)),
                                Female = Gumps.IsValidIndex(UoCalc.FemaleGump(anim))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetMessage("Wearables could not be read: " + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            _wearables = list;
            FillWearables();
        }

        private void FillWearables()
        {
            if (_wearables == null)
            {
                return;
            }
            string f = _wearFilter.Text.Trim();
            bool isNumber = UoCalc.TryParseInt(f, out int number);
            int layer = _wearLayer.SelectedIndex;
            int state = _wearState.SelectedIndex;
            _listWear.BeginUpdate();
            _listWear.Items.Clear();
            foreach (Wearable w in _wearables)
            {
                if (layer > 0 && w.Layer != layer)
                {
                    continue;
                }
                if ((state == 1 && w.Male) || (state == 2 && w.Female) || (state == 3 && !(w.Male && w.Female)) || (state == 4 && (w.Male || w.Female)))
                {
                    continue;
                }
                if (f.Length > 0)
                {
                    bool match = (w.Name ?? string.Empty).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!match && isNumber)
                    {
                        match = w.Id == number || w.Anim == number || UoCalc.MaleGump(w.Anim) == number || UoCalc.FemaleGump(w.Anim) == number;
                    }
                    if (!match)
                    {
                        continue;
                    }
                }
                string st = w.Male && w.Female ? "both" : w.Male ? "female → uses male" : w.Female ? "only female" : "no gump";
                var item = new ListViewItem(new[]
                {
                    UoCalc.Hex(w.Id), w.Name, $"{w.Layer} {UoCalc.LayerName(w.Layer)}", $"{w.Anim} (0x{w.Anim:X})",
                    UoCalc.MaleGump(w.Anim).ToString(), UoCalc.FemaleGump(w.Anim).ToString(), st
                }) { Tag = w };
                if (!w.Male && !w.Female)
                {
                    item.ForeColor = Color.Firebrick;
                }
                else if (!w.Female)
                {
                    item.ForeColor = Color.DarkOrange;
                }
                _listWear.Items.Add(item);
            }
            _listWear.EndUpdate();
            _wearCount.Text = $"{_listWear.Items.Count} of {_wearables.Count} wearables";
        }

        private void ExportWearables()
        {
            if (_listWear.Items.Count == 0)
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "wearables.csv" })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                var sb = new StringBuilder("ItemID;Name;Layer;AnimID;GumpMale;GumpFemale;MaleExists;FemaleExists\r\n");
                foreach (ListViewItem li in _listWear.Items)
                {
                    var w = (Wearable)li.Tag;
                    sb.Append($"0x{w.Id:X4};{w.Name};{w.Layer};{w.Anim};{UoCalc.MaleGump(w.Anim)};{UoCalc.FemaleGump(w.Anim)};{w.Male};{w.Female}\r\n");
                }
                try
                {
                    File.WriteAllText(d.FileName, sb.ToString(), Encoding.UTF8);
                    SetMessage("Exported: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Body tab

        private TextBox _bodyInput, _bFile, _bConverted, _bGroup, _bIndexHex, _bIndexDec, _bActions, _bIdxReverse, _bReverseOut;
        private NumericUpDown _bodyAction, _bodyHue;
        private ComboBox _bodyDir, _bReverseFile;
        private ListBox _bodyMounts;
        private Panel _pvBody;
        private int _bodyValue = -1, _bodyFileType = 1, _bodyConvertedValue = -1;

        private void BuildBodyTab()
        {
            _bodyInput = In(90);
            _bodyInput.TextChanged += (s, e) => CalcBody();
            toolTip1.SetToolTip(_bodyInput, Tip("Body ID (e.g. 400 = human male, 401 = human female, 0xC8 = horse). Auto: 0x / A-F = hex.",
                "Body-ID (z. B. 400 = Mensch männlich, 401 = weiblich, 0xC8 = Pferd). Auto: 0x / A-F = Hex."));
            _bodyAction = Num(0, 34, 0, 55);
            _bodyAction.ValueChanged += (s, e) => CalcBody();
            _bodyDir = Combo(150, UoCalc.DirectionNames);
            _bodyDir.SelectedIndexChanged += (s, e) => CalcBody();
            _bodyHue = Num(0, 3000, 0, 60);
            _bodyHue.ValueChanged += (s, e) => _pvBody.Invalidate();
            _bFile = Out(190); _bConverted = Out(90); _bGroup = Out(250); _bIndexHex = Out(90); _bIndexDec = Out(80); _bActions = Out(50);
            _bodyMounts = new ListBox { Width = 330, Height = 80, IntegralHeight = false };
            _bodyMounts.DoubleClick += (s, e) =>
            {
                if (_bodyMounts.SelectedItem is ItemChoice c)
                {
                    OpenInGumpTab(c.Id);
                }
            };
            toolTip1.SetToolTip(_bodyMounts, Tip("Items on layer 25 (Mount) whose tiledata Animation is this body – the item a rider \"wears\".",
                "Items auf Layer 25 (Reittier), deren tiledata-Animation dieser Body ist – das Item, das ein Reiter \"trägt\"."));
            GroupBox body = Group("Body → animation file",
                Row(Lbl("Body:", 80), _bodyInput, Lbl("Action:"), _bodyAction, Lbl("Direction:"), _bodyDir),
                Row(Lbl("File:", 80), _bFile, Lbl("Body in file:"), _bConverted),
                Row(Lbl("Type:", 80), _bGroup, Lbl("Actions:"), _bActions),
                Row(Lbl("idx index:", 80), _bIndexHex, _bIndexDec, Btn("→ History", (s, e) => AddBodyHistory())),
                Lbl("Mount items using this body (double click = Gump ID tab):"),
                _bodyMounts);

            _bReverseFile = Combo(170, "anim.mul", "anim2.mul", "anim3.mul", "anim4.mul", "anim5.mul");
            _bIdxReverse = In(90);
            _bReverseOut = Out(330);
            _bIdxReverse.TextChanged += (s, e) => ReverseAnimIndex();
            _bReverseFile.SelectedIndexChanged += (s, e) => ReverseAnimIndex();
            GroupBox reverse = Group("idx index → body / action / direction",
                Row(_bReverseFile, Lbl("Index:"), _bIdxReverse),
                _bReverseOut);

            _pvBody = Preview(260, 260, (s, e) => PaintBody(e.Graphics));
            GroupBox preview = Group("Preview (first frame)", _pvBody, Row(Lbl("Hue:"), _bodyHue));
            FillTab(tabBody, new Control[] { body, reverse }, new Control[] { preview });
            _bodyInput.Text = "400";
        }

        private void CalcBody()
        {
            _bodyValue = -1;
            if (!UoCalc.TryParseInt(_bodyInput.Text, out int body) || body < 0 || body > 0xFFFF)
            {
                foreach (TextBox t in new[] { _bFile, _bConverted, _bGroup, _bIndexHex, _bIndexDec, _bActions })
                {
                    t.Text = string.Empty;
                }
                _bodyMounts.Items.Clear();
                _pvBody?.Invalidate();
                return;
            }
            _bodyValue = body;
            int converted = body;
            int fileType = 1;
            try
            {
                lock (SdkSync.Lock)
                {
                    fileType = BodyConverter.Convert(ref converted);
                }
            }
            catch (Exception)
            {
                converted = body;
                fileType = 1;
            }
            _bodyFileType = fileType;
            _bodyConvertedValue = converted;
            int group = UoCalc.AnimGroup(converted, fileType);
            int actions = UoCalc.ActionCount(group);
            _bodyAction.Maximum = actions - 1;
            int index = UoCalc.AnimIndex(converted, (int)_bodyAction.Value, Math.Max(0, _bodyDir.SelectedIndex), fileType);
            _bFile.Text = fileType == 1 ? "anim.mul (no bodyconv entry)" : $"{UoCalc.AnimFileName(fileType)} (bodyconv.def)";
            _bConverted.Text = converted.ToString();
            _bGroup.Text = UoCalc.AnimGroupNames[group];
            _bActions.Text = actions.ToString();
            _bIndexHex.Text = UoCalc.Hex(index);
            _bIndexDec.Text = index.ToString();
            _bodyMounts.Items.Clear();
            try
            {
                lock (SdkSync.Lock)
                {
                    ItemData[] table = TileData.ItemTable;
                    if (table != null)
                    {
                        for (int i = 0; i < table.Length; i++)
                        {
                            if (table[i].Quality == 0x19 && table[i].Animation == body)
                            {
                                _bodyMounts.Items.Add(new ItemChoice(i, table[i].Name?.Trim()));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // tiledata missing
            }
            _pvBody.Invalidate();
        }

        private void AddBodyHistory()
        {
            if (_bodyValue >= 0)
            {
                AddHistory($"Body {_bodyValue} (0x{_bodyValue:X}) → {UoCalc.AnimFileName(_bodyFileType)} body {_bodyConvertedValue}, action {_bodyAction.Value}, dir {_bodyDir.SelectedIndex} → idx {_bIndexDec.Text} ({_bIndexHex.Text})");
            }
        }

        private void ReverseAnimIndex()
        {
            if (!UoCalc.TryParseInt(_bIdxReverse.Text, out int idx) || idx < 0)
            {
                _bReverseOut.Text = string.Empty;
                return;
            }
            int fileType = _bReverseFile.SelectedIndex + 1;
            for (int body = 0; body < 4096; body++)
            {
                int group = UoCalc.AnimGroup(body, fileType);
                int start = UoCalc.AnimIndex(body, 0, 0, fileType);
                int count = UoCalc.ActionCount(group) * 5;
                if (idx >= start && idx < start + count)
                {
                    int rest = idx - start;
                    int slot = rest % 5;
                    string dirs = slot == 0 ? "0" : $"{slot} (or mirrored {8 - slot})";
                    int trueBody = body;
                    try
                    {
                        lock (SdkSync.Lock)
                        {
                            trueBody = BodyConverter.GetTrueBody(fileType, body);
                        }
                    }
                    catch (Exception)
                    {
                        trueBody = body;
                    }
                    _bReverseOut.Text = $"body {body}" + (trueBody != body && trueBody >= 0 ? $" (game body {trueBody})" : string.Empty) + $", action {rest / 5}, direction {dirs}";
                    return;
                }
            }
            _bReverseOut.Text = "outside of the file";
        }

        private void PaintBody(Graphics g)
        {
            Rectangle area = _pvBody.ClientRectangle;
            if (_bodyValue < 0)
            {
                DrawPreview(g, area, null, "no body", Color.Gray);
                return;
            }
            Bitmap frame = null;
            try
            {
                lock (SdkSync.Lock)
                {
                    int hue = 0;
                    AnimationFrame[] frames = Animations.GetAnimation(_bodyValue, (int)_bodyAction.Value, Math.Max(0, _bodyDir.SelectedIndex), ref hue, false, true);
                    if (frames != null && frames.Length > 0 && frames[0]?.Bitmap != null)
                    {
                        frame = new Bitmap(frames[0].Bitmap);
                    }
                }
            }
            catch (Exception)
            {
                frame = null;
            }
            if (frame == null)
            {
                DrawPreview(g, area, null, "no animation", Color.IndianRed);
                return;
            }
            int dir = Math.Max(0, _bodyDir.SelectedIndex);
            if (dir > 4)
            {
                frame.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            Bitmap shown = Hued(frame, (int)_bodyHue.Value, false, out bool owned);
            DrawPreview(g, area, shown, $"body {_bodyValue}, action {_bodyAction.Value}", Color.Gainsboro, 3);
            if (owned)
            {
                shown.Dispose();
            }
            frame.Dispose();
        }

        #endregion

        #region Hue tab

        private TextBox _hueInput, _hNumber, _hHex, _hEntry, _hName, _hPartial;
        private Panel _hueStrip, _pvHue, _colorSwatch;
        private TextBox _hueItem;
        private CheckBox _chkHueGray;
        private NumericUpDown _numR, _numG, _numB;
        private TextBox _colHtml, _col555, _col555Dec, _colBack, _colInput;
        private ListBox _nearest;
        private int _hueValue = -1;
        private bool _colorUpdating;

        private void BuildHueTab()
        {
            _hueInput = In(90, "1");
            _hueInput.TextChanged += (s, e) => CalcHue();
            toolTip1.SetToolTip(_hueInput, Tip("Hue as in scripts / packets, e.g. 1153, 0x481 or 0x8481 (bit 0x8000 = only gray pixels).",
                "Hue wie in Skripten / Packets, z. B. 1153, 0x481 oder 0x8481 (Bit 0x8000 = nur graue Pixel)."));
            _hNumber = Out(80); _hHex = Out(80); _hEntry = Out(80); _hName = Out(200); _hPartial = Out(270);
            _hueStrip = Preview(330, 26, (s, e) => PaintHueStrip(e.Graphics));
            toolTip1.SetToolTip(_hueStrip, Tip("The 32 colors: gray level 0 (left) … 31 (right).", "Die 32 Farben: Graustufe 0 (links) … 31 (rechts)."));
            _hueItem = In(80, "0x1517");
            _hueItem.TextChanged += (s, e) => _pvHue.Invalidate();
            _chkHueGray = Chk("only gray pixels", true);
            _chkHueGray.CheckedChanged += (s, e) => _pvHue.Invalidate();
            _pvHue = Preview(200, 200, (s, e) => PaintHuePreview(e.Graphics));
            GroupBox hue = Group("Hue (hues.mul)",
                Row(Lbl("Value:", 70), _hueInput, Btn("◀", (s, e) => StepHue(-1)), Btn("▶", (s, e) => StepHue(1)), Btn("→ History", (s, e) => AddHueHistory())),
                Row(Lbl("Hue no.:", 70), _hNumber, _hHex, Lbl("hues.mul entry:"), _hEntry),
                Row(Lbl("Name:", 70), _hName),
                Row(Lbl("Flags:", 70), _hPartial),
                _hueStrip,
                Row(Lbl("Item:", 70), _hueItem, _chkHueGray),
                _pvHue);

            _numR = Num(0, 255, 200, 55); _numG = Num(0, 255, 120, 55); _numB = Num(0, 255, 40, 55);
            foreach (NumericUpDown n in new[] { _numR, _numG, _numB })
            {
                n.ValueChanged += (s, e) => { if (!_colorUpdating) SetColor(Color.FromArgb((int)_numR.Value, (int)_numG.Value, (int)_numB.Value)); };
            }
            _colInput = In(110);
            toolTip1.SetToolTip(_colInput, Tip("#RRGGBB (web), 0x7FFF / 32767 (15 bit UO color) or \"r,g,b\".",
                "#RRGGBB (Web), 0x7FFF / 32767 (15-Bit-UO-Farbe) oder \"r,g,b\"."));
            _colInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ParseColorInput(); } };
            _colHtml = Out(80); _col555 = Out(80); _col555Dec = Out(70); _colBack = Out(80);
            _colorSwatch = new Panel { Width = 60, Height = 40, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(3) };
            _nearest = new ListBox { Width = 330, Height = 130, IntegralHeight = false, Font = new Font("Consolas", 9F) };
            _nearest.DoubleClick += (s, e) =>
            {
                if (_nearest.SelectedItem is HueMatch m)
                {
                    _hueInput.Text = m.Number.ToString();
                }
            };
            GroupBox color = Group("Color: RGB ↔ 15 bit UO color",
                Row(Lbl("R"), _numR, Lbl("G"), _numG, Lbl("B"), _numB, Btn("Pick...", (s, e) => PickColor()), _colorSwatch),
                Row(Lbl("Input:", 70), _colInput, Btn("Read", (s, e) => ParseColorInput())),
                Row(Lbl("Web:", 70), _colHtml, Lbl("UO 555:"), _col555, _col555Dec),
                Row(Lbl("In client:", 70), _colBack, Lbl("(after the 15 bit round trip)")),
                Row(Btn("Find nearest hues", (s, e) => FindNearestHues(), Tip("Searches all hues for the color (any of the 32 levels).", "Sucht in allen Hues nach der Farbe (in allen 32 Stufen)."))),
                _nearest);
            FillTab(tabHue, new Control[] { hue }, new Control[] { color });
            CalcHue();
            SetColor(Color.FromArgb(200, 120, 40));
        }

        private void CalcHue()
        {
            _hueValue = -1;
            if (!UoCalc.TryParseInt(_hueInput.Text, out int value) || value < 0 || value > 0xFFFF)
            {
                foreach (TextBox t in new[] { _hNumber, _hHex, _hEntry, _hName, _hPartial })
                {
                    t.Text = string.Empty;
                }
                _hueStrip.Invalidate();
                _pvHue.Invalidate();
                return;
            }
            int number = UoCalc.HueNumber(value, out bool partial);
            _hueValue = number;
            _hNumber.Text = number.ToString();
            _hHex.Text = UoCalc.Hex(number);
            _hEntry.Text = number == 0 ? "-" : (number - 1).ToString();
            Hue hue = GetHueEntry(number);
            _hName.Text = number == 0 ? "0 = no hue (original colors)" : hue == null ? "not in hues.mul" : hue.Name?.Trim();
            var flags = new List<string>();
            if (partial)
            {
                flags.Add("0x8000: only gray pixels");
            }
            if ((value & 0x4000) != 0)
            {
                flags.Add("0x4000 set");
            }
            _hPartial.Text = flags.Count == 0 ? "none" : string.Join(", ", flags);
            if (_chkHueGray != null && partial)
            {
                _chkHueGray.Checked = true;
            }
            _hueStrip.Invalidate();
            _pvHue.Invalidate();
        }

        private void StepHue(int delta)
        {
            int n = Math.Max(1, Math.Min(3000, (_hueValue < 0 ? 0 : _hueValue) + delta));
            _hueInput.Text = n.ToString();
        }

        private void AddHueHistory()
        {
            if (_hueValue >= 0)
            {
                AddHistory($"Hue {_hueValue} ({UoCalc.Hex(_hueValue)}) \"{_hName.Text}\", hues.mul entry {_hEntry.Text}, with 0x8000 = {UoCalc.Hex(_hueValue | UoCalc.HuePartialFlag)}");
            }
        }

        private void PaintHueStrip(Graphics g)
        {
            Hue hue = GetHueEntry(_hueValue);
            if (hue == null)
            {
                TextRenderer.DrawText(g, _hueValue == 0 ? "no hue" : "–", Font, _hueStrip.ClientRectangle, Color.Gray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            float w = _hueStrip.Width / 32f;
            for (int i = 0; i < 32; i++)
            {
                using (var b = new SolidBrush(UoCalc.Color555ToRgb(hue.Colors[i])))
                {
                    g.FillRectangle(b, i * w, 0, w + 1, _hueStrip.Height);
                }
            }
        }

        private void PaintHuePreview(Graphics g)
        {
            Bitmap art = UoCalc.TryParseInt(_hueItem.Text, out int id) ? GetItemArt(id) : null;
            Bitmap shown = Hued(art, Math.Max(0, _hueValue), _chkHueGray.Checked, out bool owned);
            DrawPreview(g, _pvHue.ClientRectangle, shown, art == null ? "no item" : $"item {UoCalc.Hex(id)}", Color.Gainsboro, 3);
            if (owned)
            {
                shown.Dispose();
            }
        }

        private void SetColor(Color c)
        {
            _colorUpdating = true;
            _numR.Value = c.R;
            _numG.Value = c.G;
            _numB.Value = c.B;
            _colorUpdating = false;
            int v = UoCalc.RgbTo555(c);
            Color back = UoCalc.Color555ToRgb(v);
            _colHtml.Text = UoCalc.HtmlHex(c);
            _col555.Text = UoCalc.Hex(v);
            _col555Dec.Text = v.ToString();
            _colBack.Text = UoCalc.HtmlHex(back);
            _colorSwatch.BackColor = back;
        }

        private void PickColor()
        {
            using (var d = new ColorDialog { Color = Color.FromArgb((int)_numR.Value, (int)_numG.Value, (int)_numB.Value), FullOpen = true })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    SetColor(d.Color);
                }
            }
        }

        private void ParseColorInput()
        {
            string t = _colInput.Text.Trim();
            string[] parts = t.Split(',', ';', ' ').Where(p => p.Length > 0).ToArray();
            if (parts.Length == 3 && parts.All(p => int.TryParse(p, out int x) && x >= 0 && x <= 255))
            {
                SetColor(Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])));
            }
            else if (t.StartsWith("#", StringComparison.Ordinal) && t.Length == 7 && int.TryParse(t.Substring(1), NumberStyles.HexNumber, null, out int rgb))
            {
                SetColor(Color.FromArgb(255, Color.FromArgb(rgb)));
            }
            else if (UoCalc.TryParseInt(t, out int v555) && v555 >= 0 && v555 <= 0xFFFF)
            {
                SetColor(UoCalc.Color555ToRgb(v555 & 0x7FFF));
            }
            else
            {
                SetMessage("Color not understood: use #RRGGBB, 0x7FFF or r,g,b.");
                return;
            }
            AddHistory($"Color {_colHtml.Text} = UO {_col555.Text} ({_col555Dec.Text})");
        }

        private sealed class HueMatch
        {
            public int Number;
            public int Level;
            public int Distance;
            public string Name;
            public override string ToString() => $"{Number,5}  0x{Number:X4}  level {Level,2}  Δ{(int)Math.Sqrt(Distance / 9.0),3}  {Name}";
        }

        private void FindNearestHues()
        {
            Hue[] list;
            try
            {
                list = Ultima.Hues.List;
            }
            catch (Exception)
            {
                list = null;
            }
            if (list == null)
            {
                SetMessage("hues.mul is not loaded.");
                return;
            }
            Color target = Color.FromArgb((int)_numR.Value, (int)_numG.Value, (int)_numB.Value);
            var best = new List<HueMatch>();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i]?.Colors == null)
                {
                    continue;
                }
                int bestDist = int.MaxValue, bestLevel = 0;
                for (int l = 0; l < 32; l++)
                {
                    int d = UoCalc.ColorDistance(target, UoCalc.Color555ToRgb(list[i].Colors[l]));
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestLevel = l;
                    }
                }
                best.Add(new HueMatch { Number = i + 1, Level = bestLevel, Distance = bestDist, Name = list[i].Name?.Trim() });
            }
            _nearest.Items.Clear();
            foreach (HueMatch m in best.OrderBy(m => m.Distance).Take(25))
            {
                _nearest.Items.Add(m);
            }
            SetMessage("Nearest hues found – double click to show one.");
        }

        #endregion

        #region Flags tab

        private CheckedListBox _flagList;
        private TextBox _flagHex, _flagHex32, _flagDec, _flagInput, _flagId;
        private Label _flagInfo;
        private RadioButton _flagItem;
        private bool _flagUpdating;

        private static readonly Dictionary<string, string> FlagInfo = new Dictionary<string, string>
        {
            ["Background"] = "Background tile, no shadow / Hintergrund, kein Schatten",
            ["Weapon"] = "Weapon / Waffe",
            ["Transparent"] = "Transparent / durchsichtig",
            ["Translucent"] = "Drawn half transparent / halbtransparent gezeichnet",
            ["Wall"] = "Wall / Wand",
            ["Damaging"] = "Hurts when walked on (lava…) / verletzt (Lava…)",
            ["Impassable"] = "Blocks walking / nicht begehbar",
            ["Wet"] = "Water / Wasser",
            ["Surface"] = "Can stand / place items on it / Oberfläche, begehbar",
            ["Bridge"] = "Bridge, stairs / Brücke, Treppe",
            ["Generic"] = "Stackable / stapelbar",
            ["Window"] = "Window / Fenster",
            ["NoShoot"] = "Blocks line of sight / blockiert Sichtlinie",
            ["ArticleA"] = "Name with \"a\" / Name mit \"a\"",
            ["ArticleAn"] = "Name with \"an\" / Name mit \"an\"",
            ["Foliage"] = "Foliage, hidden near player (circle of transparency) / Laub",
            ["PartialHue"] = "Only gray pixels are hued / nur graue Pixel werden eingefärbt",
            ["Map"] = "Shown on the radar map / auf der Radarkarte",
            ["Container"] = "Container / Behälter",
            ["Wearable"] = "Can be worn – needs Animation + layer / tragbar – braucht Animation + Layer",
            ["LightSource"] = "Gives light / Lichtquelle",
            ["Animation"] = "Animated item (animdata) / animiertes Item",
            ["HoverOver"] = "Flying monsters can pass / fliegende Monster können darüber",
            ["NoDiagonal"] = "No diagonal movement / keine Diagonalbewegung",
            ["Armor"] = "Armor / Rüstung",
            ["Roof"] = "Roof / Dach",
            ["Door"] = "Door / Tür",
            ["StairBack"] = "Stairs facing back / Treppe nach hinten",
            ["StairRight"] = "Stairs facing right / Treppe nach rechts",
            ["AlphaBlend"] = "Alpha blending (HS) / Alpha-Überblendung",
            ["NoShadow"] = "No shadow (HS) / kein Schatten",
            ["PixelBleed"] = "Pixel bleed (HS)",
            ["PlayAnimOnce"] = "Animation plays once (HS) / Animation nur einmal"
        };

        private void BuildFlagsTab()
        {
            _flagList = new CheckedListBox { Width = 330, Height = 470, CheckOnClick = true, IntegralHeight = false };
            foreach (TileFlag f in Enum.GetValues(typeof(TileFlag)))
            {
                string name = f.ToString();
                if (f == TileFlag.None || name.StartsWith("Unused", StringComparison.Ordinal))
                {
                    continue;
                }
                _flagList.Items.Add(new FlagItem(f));
            }
            _flagList.ItemCheck += (s, e) => BeginInvoke((Action)UpdateFlagValue);
            _flagList.SelectedIndexChanged += (s, e) =>
            {
                if (_flagList.SelectedItem is FlagItem fi)
                {
                    _flagInfo.Text = FlagInfo.TryGetValue(fi.Flag.ToString(), out string info) ? info : fi.Flag.ToString();
                }
            };
            _flagHex = Out(170); _flagHex32 = Out(110); _flagDec = Out(170);
            _flagInput = In(170);
            _flagInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ReadFlagInput(); } };
            toolTip1.SetToolTip(_flagInput, Tip("A flag value (hex with 0x or decimal) → the boxes are checked.", "Ein Flag-Wert (Hex mit 0x oder Dezimal) → die Häkchen werden gesetzt."));
            _flagInfo = new Label { Width = 330, Height = 40, ForeColor = Color.DimGray };
            _flagItem = new RadioButton { Text = "Item", Checked = true, AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            var land = new RadioButton { Text = "Land", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            _flagId = In(80);
            GroupBox list = Group("Flags", _flagList, _flagInfo);
            GroupBox value = Group("Value",
                Row(Lbl("Hex (64 bit):", 95), _flagHex),
                Row(Lbl("Hex (32 bit):", 95), _flagHex32),
                Row(Lbl("Decimal:", 95), _flagDec),
                Row(Lbl("Input:", 95), _flagInput, Btn("Read", (s, e) => ReadFlagInput())),
                Row(Btn("Clear", (s, e) => SetFlags(0)), Btn("→ History", (s, e) => AddHistory($"Flags {_flagHex.Text} = {FlagNames(CurrentFlags())}"))));
            GroupBox tiledata = Group("tiledata",
                Row(_flagItem, land, Lbl("ID:"), _flagId),
                Row(Btn("Load flags", (s, e) => LoadFlagsFromTile()), Btn("Write flags...", (s, e) => WriteFlagsToTile(), Tip(
                    "Writes the flags into tiledata (memory). Save tiledata.mul in UoFiddler afterwards.",
                    "Schreibt die Flags in tiledata (Speicher). Danach tiledata.mul in UoFiddler speichern."))));
            FillTab(tabFlags, new Control[] { list }, new Control[] { value, tiledata });
            UpdateFlagValue();
        }

        private sealed class FlagItem
        {
            public FlagItem(TileFlag flag) => Flag = flag;

            public TileFlag Flag { get; }
            public override string ToString() => $"{Flag}  (0x{(ulong)Flag:X})";
        }

        private ulong CurrentFlags()
        {
            ulong v = 0;
            foreach (object o in _flagList.CheckedItems)
            {
                v |= (ulong)((FlagItem)o).Flag;
            }
            return v;
        }

        private static string FlagNames(ulong v) => v == 0 ? "None" : ((TileFlag)v).ToString();

        private void UpdateFlagValue()
        {
            if (_flagUpdating)
            {
                return;
            }
            ulong v = CurrentFlags();
            _flagHex.Text = "0x" + v.ToString("X16");
            _flagHex32.Text = "0x" + (v & 0xFFFFFFFF).ToString("X8");
            _flagDec.Text = v.ToString();
        }

        private void SetFlags(ulong v)
        {
            _flagUpdating = true;
            for (int i = 0; i < _flagList.Items.Count; i++)
            {
                _flagList.SetItemChecked(i, (v & (ulong)((FlagItem)_flagList.Items[i]).Flag) != 0);
            }
            _flagUpdating = false;
            UpdateFlagValue();
        }

        private void ReadFlagInput()
        {
            string t = _flagInput.Text.Trim();
            bool hex = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            string digits = hex ? t.Substring(2) : t;
            if (!ulong.TryParse(digits, hex || digits.Any(c => char.IsLetter(c)) ? NumberStyles.HexNumber : NumberStyles.None, CultureInfo.InvariantCulture, out ulong v))
            {
                SetMessage("Not a flag value.");
                return;
            }
            SetFlags(v);
            SetMessage("Flags: " + FlagNames(v));
        }

        private void LoadFlagsFromTile()
        {
            if (!UoCalc.TryParseInt(_flagId.Text, out int id) || id < 0)
            {
                SetMessage("Invalid ID.");
                return;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    if (_flagItem.Checked)
                    {
                        SetFlags((ulong)TileData.ItemTable[id].Flags);
                    }
                    else
                    {
                        SetFlags((ulong)TileData.LandTable[id].Flags);
                    }
                }
                SetMessage($"Flags of {(_flagItem.Checked ? "item" : "land")} {UoCalc.Hex(id)} loaded.");
            }
            catch (Exception)
            {
                SetMessage("tiledata is not loaded or the ID is too large.");
            }
        }

        private void WriteFlagsToTile()
        {
            if (!UoCalc.TryParseInt(_flagId.Text, out int id) || id < 0)
            {
                SetMessage("Invalid ID.");
                return;
            }
            ulong v = CurrentFlags();
            string what = _flagItem.Checked ? "item" : "land";
            if (MessageBox.Show(this, $"Set the flags of {what} {UoCalc.Hex(id)} to\n{FlagNames(v)}?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    if (_flagItem.Checked)
                    {
                        TileData.ItemTable[id].Flags = (TileFlag)v;
                    }
                    else
                    {
                        TileData.LandTable[id].Flags = (TileFlag)v;
                    }
                }
                UoFiddler.Controls.Classes.Options.ChangedUltimaClass["TileData"] = true;
                UoFiddler.Controls.Classes.ControlEvents.FireTileDataChangeEvent(this, _flagItem.Checked ? id + 0x4000 : id);
                AddHistory($"Wrote flags {_flagHex.Text} to {what} {UoCalc.Hex(id)}");
                SetMessage("Flags written – save tiledata.mul in UoFiddler.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Map tab

        private ComboBox _mapCombo;
        private NumericUpDown _mapW, _mapH, _mapX, _mapY, _mapX2, _mapY2, _sLatDeg, _sLatMin, _sLonDeg, _sLonMin;
        private TextBox _mBlockXY, _mBlock, _mCell, _mMapOffset, _mStaIdx, _mBlocks, _mFileSize, _mSextant, _mDistance, _mSextantXY;
        private ComboBox _sNS, _sEW;
        private CheckBox _sDungeon;
        private List<UoCalc.MapInfo> _maps;

        private void BuildMapTab()
        {
            _maps = UoCalc.DefaultMaps();
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    Ultima.Map m = Ultima.Map.GetBuiltIn(i);
                    if (m != null && m.Width > 0 && m.Height > 0)
                    {
                        _maps[i].Width = m.Width;
                        _maps[i].Height = m.Height;
                    }
                }
                catch (Exception)
                {
                    // map not loaded
                }
            }
            _mapCombo = Combo(230, _maps.Cast<object>().ToArray());
            _mapW = Num(8, 65536, 7168, 70);
            _mapH = Num(8, 65536, 4096, 70);
            _mapX = Num(0, 65535, 1323, 70);
            _mapY = Num(0, 65535, 1624, 70);
            _mapX2 = Num(0, 65535, 1434, 70);
            _mapY2 = Num(0, 65535, 1699, 70);
            _mapCombo.SelectedIndexChanged += (s, e) =>
            {
                UoCalc.MapInfo m = _maps[_mapCombo.SelectedIndex];
                _mapW.Value = m.Width;
                _mapH.Value = m.Height;
                CalcMap();
            };
            foreach (NumericUpDown n in new[] { _mapW, _mapH, _mapX, _mapY, _mapX2, _mapY2 })
            {
                n.ValueChanged += (s, e) => CalcMap();
            }
            _mBlockXY = Out(110); _mBlock = Out(150); _mCell = Out(70); _mMapOffset = Out(150); _mStaIdx = Out(150);
            _mBlocks = Out(150); _mFileSize = Out(200); _mSextant = Out(220); _mDistance = Out(300);
            GroupBox map = Group("Map position → map*.mul / staidx*.mul",
                Row(Lbl("Map:", 70), _mapCombo, Lbl("W"), _mapW, Lbl("H"), _mapH),
                Row(Lbl("X:", 70), _mapX, Lbl("Y:"), _mapY, Btn("→ History", (s, e) => AddMapHistory())),
                Row(Lbl("Block X/Y:", 110), _mBlockXY, Lbl("Cell:"), _mCell),
                Row(Lbl("Block index:", 110), _mBlock),
                Row(Lbl("map.mul offset:", 110), _mMapOffset, Lbl("(block, 196 bytes)")),
                Row(Lbl("staidx offset:", 110), _mStaIdx, Lbl("(12 bytes per block)")),
                Row(Lbl("Blocks total:", 110), _mBlocks),
                Row(Lbl("map.mul size:", 110), _mFileSize),
                Row(Lbl("Sextant:", 110), _mSextant));
            GroupBox distance = Group("Distance to X2 / Y2",
                Row(Lbl("X2:", 70), _mapX2, Lbl("Y2:"), _mapY2),
                _mDistance);

            _sLatDeg = Num(0, 180, 0, 55); _sLatMin = Num(0, 59, 0, 50); _sLonDeg = Num(0, 180, 0, 55); _sLonMin = Num(0, 59, 0, 50);
            _sNS = Combo(45, "N", "S");
            _sEW = Combo(45, "W", "E");
            _sDungeon = Chk("Felucca / Trammel dungeon area (x ≥ 5120)");
            _mSextantXY = Out(200);
            GroupBox sextant = Group("Sextant → X / Y",
                Row(_sLatDeg, Lbl("°"), _sLatMin, Lbl("'"), _sNS),
                Row(_sLonDeg, Lbl("°"), _sLonMin, Lbl("'"), _sEW),
                _sDungeon,
                Row(Btn("Calculate", (s, e) => SextantToXY()), _mSextantXY));
            FillTab(tabMap, new Control[] { map, distance }, new Control[] { sextant });
            CalcMap();
        }

        private void CalcMap()
        {
            if (_mDistance == null)
            {
                return;
            }
            int w = (int)_mapW.Value, h = (int)_mapH.Value, x = (int)_mapX.Value, y = (int)_mapY.Value;
            bool inside = x < w && y < h;
            int block = UoCalc.BlockIndex(x, y, h);
            int cell = UoCalc.CellInBlock(x, y);
            long mapOffset = (long)block * UoCalc.MapBlockBytes;
            int blocks = (w >> 3) * (h >> 3);
            _mBlockXY.Text = $"{x >> 3} / {y >> 3}";
            _mCell.Text = $"{cell}";
            _mBlock.Text = inside ? UoCalc.Both(block, 1) : "outside the map";
            _mMapOffset.Text = inside ? UoCalc.Hex(mapOffset, 1) + $" (+{4 + cell * 3} cell)" : string.Empty;
            _mStaIdx.Text = inside ? UoCalc.Hex((long)block * UoCalc.StaIdxBytes, 1) : string.Empty;
            _mBlocks.Text = blocks.ToString("N0");
            long size = (long)blocks * UoCalc.MapBlockBytes;
            _mFileSize.Text = $"{size:N0} bytes ({size / 1048576.0:0.0} MB)";
            int mapIndex = _mapCombo.SelectedIndex;
            _mSextant.Text = UoCalc.ToSextant(mapIndex, x, y, w, h, out string text, out _, out _, out _, out _, out _, out _) ? text : "no sextant here";
            int dx = Math.Abs((int)_mapX2.Value - x), dy = Math.Abs((int)_mapY2.Value - y);
            _mDistance.Text = $"UO range {Math.Max(dx, dy)} tiles · straight {Math.Sqrt(dx * dx + dy * dy):0.0} · dx {dx} dy {dy}";
        }

        private void AddMapHistory() =>
            AddHistory($"Map {_maps[_mapCombo.SelectedIndex].Name} X {_mapX.Value} Y {_mapY.Value} → block {_mBlock.Text}, cell {_mCell.Text}, map offset {_mMapOffset.Text}, staidx {_mStaIdx.Text}, sextant {_mSextant.Text}");

        private void SextantToXY()
        {
            Point p = UoCalc.FromSextant((int)_sLatDeg.Value, (int)_sLatMin.Value, _sNS.SelectedIndex == 1, (int)_sLonDeg.Value, (int)_sLonMin.Value, _sEW.SelectedIndex == 1,
                _sDungeon.Checked, (int)_mapW.Value, (int)_mapH.Value);
            _mSextantXY.Text = $"X {p.X}  Y {p.Y}  (± 7 tiles)";
            string s = $"{_sLatDeg.Value}° {_sLatMin.Value}'{_sNS.Text}, {_sLonDeg.Value}° {_sLonMin.Value}'{_sEW.Text}";
            AddHistory($"Sextant {s} → X {p.X} Y {p.Y}");
            if (p.X <= _mapX.Maximum && p.Y <= _mapY.Maximum)
            {
                _mapX.Value = p.X;
                _mapY.Value = p.Y;
            }
        }

        #endregion

        #region Layer tab

        private TextBox _artInput, _aItem, _aArt, _aLand, _aMulti, _aMax;

        private void BuildLayerTab()
        {
            var list = new ListView { View = View.Details, FullRowSelect = true, GridLines = true, Width = 470, Height = 520 };
            list.Columns.Add("Dec", 45);
            list.Columns.Add("Hex", 50);
            list.Columns.Add("Name (scripts)", 110);
            list.Columns.Add("Deutsch", 150);
            list.Columns.Add("Example", 110);
            foreach (UoCalc.LayerInfo l in UoCalc.Layers)
            {
                list.Items.Add(new ListViewItem(new[] { l.Id.ToString(), $"0x{l.Id:X2}", l.Name, l.German, l.Example }) { Tag = l });
            }
            list.DoubleClick += (s, e) =>
            {
                if (list.SelectedItems.Count > 0 && list.SelectedItems[0].Tag is UoCalc.LayerInfo l)
                {
                    Copy($"{l.Id} (0x{l.Id:X2}) {l.Name}");
                    AddHistory($"Layer {l.Id} (0x{l.Id:X2}) = {l.Name} / {l.German}");
                }
            };
            toolTip1.SetToolTip(list, Tip("Layer = \"Quality\" field of a wearable in tiledata. Double click = copy.", "Layer = Feld \"Quality\" eines tragbaren Items in tiledata. Doppelklick = kopieren."));
            GroupBox layers = Group("Layers (tiledata Quality of wearables)", list);

            _artInput = In(90);
            _artInput.TextChanged += (s, e) => CalcArtIndex();
            _aItem = Out(150); _aArt = Out(150); _aLand = Out(150); _aMulti = Out(150); _aMax = Out(230);
            GroupBox art = Group("Item ID ↔ art index",
                Row(Lbl("ID or index:", 90), _artInput, Btn("→ History", (s, e) => AddHistory($"Art: item {_aItem.Text}, art index {_aArt.Text}, multi {_aMulti.Text}"))),
                Row(Lbl("as item ID:", 90), _aItem),
                Row(Lbl("art index:", 90), _aArt, Lbl("(item + 0x4000)")),
                Row(Lbl("if art index:", 90), _aLand),
                Row(Lbl("multi:", 90), _aMulti),
                Row(Lbl("Client:", 90), _aMax));
            FillTab(tabLayer, new Control[] { layers }, new Control[] { art });
            try
            {
                _aMax.Text = $"max item ID 0x{Art.GetMaxItemId():X4}" + (Art.IsUOAHS() ? " (High Seas)" : string.Empty);
            }
            catch (Exception)
            {
                _aMax.Text = "art.mul not loaded";
            }
        }

        private void CalcArtIndex()
        {
            if (!UoCalc.TryParseInt(_artInput.Text, out int v) || v < 0)
            {
                _aItem.Text = _aArt.Text = _aLand.Text = _aMulti.Text = string.Empty;
                return;
            }
            _aItem.Text = UoCalc.Both(v);
            _aArt.Text = UoCalc.Both(UoCalc.ItemToArtIndex(v));
            _aLand.Text = v < UoCalc.StaticArtOffset ? $"land tile {UoCalc.Hex(v)}" : $"item {UoCalc.Both(v - UoCalc.StaticArtOffset)}";
            _aMulti.Text = $"multi {v} → packet / Sphere {UoCalc.Hex(v | UoCalc.StaticArtOffset)}";
        }

        #endregion

        #region Converter tab

        private TextBox _convInput, _convOut, _formula, _formulaOut, _textIn, _textOut;
        private ComboBox _convStyle;
        private NumericUpDown _convBase;

        private void BuildConvertTab()
        {
            _convInput = In(200);
            _convStyle = Combo(110, "Auto", "Hex", "Decimal", "Binary", "Octal", "Base N");
            _convBase = Num(2, 36, 36, 50);
            _convOut = new TextBox { Multiline = true, ReadOnly = true, Width = 470, Height = 230, Font = new Font("Consolas", 9.75F), ScrollBars = ScrollBars.Vertical, BackColor = SystemColors.Info };
            _convOut.DoubleClick += (s, e) => Copy(_convOut.SelectedText.Length > 0 ? _convOut.SelectedText : _convOut.Text);
            _convInput.TextChanged += (s, e) => ConvertNumber();
            _convStyle.SelectedIndexChanged += (s, e) => ConvertNumber();
            _convBase.ValueChanged += (s, e) => ConvertNumber();
            GroupBox numbers = Group("Number converter",
                Row(Lbl("Input:"), _convInput, _convStyle, Lbl("N:"), _convBase, Btn("→ History", (s, e) => AddHistory("Number " + _convOut.Lines.FirstOrDefault()))),
                _convOut);

            _formula = In(330);
            _formulaOut = Out(330);
            _formula.TextChanged += (s, e) => EvaluateFormula(false);
            _formula.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; EvaluateFormula(true); } };
            toolTip1.SetToolTip(_formula, Tip("Example: 0xC350 + 435   ·   (0x1517 - 0x1500) * 2   ·   0x481 | 0x8000. Hex needs 0x or h. Enter = history.",
                "Beispiel: 0xC350 + 435   ·   (0x1517 - 0x1500) * 2   ·   0x481 | 0x8000. Hex braucht 0x oder h. Enter = Verlauf."));
            GroupBox formula = Group("Formula ( + - * / % ( ) << >> & | ^ ~ )",
                _formula, _formulaOut);

            _textIn = new TextBox { Multiline = true, Width = 470, Height = 70, ScrollBars = ScrollBars.Vertical };
            _textOut = new TextBox { Multiline = true, ReadOnly = true, Width = 470, Height = 70, ScrollBars = ScrollBars.Vertical, BackColor = SystemColors.Info };
            _textOut.DoubleClick += (s, e) => Copy(_textOut.Text);
            GroupBox text = Group("Text",
                _textIn,
                Row(Btn("→ char codes", (s, e) => _textOut.Text = string.Join(" ", _textIn.Text.Select(c => ((int)c).ToString()))),
                    Btn("Codes → text", (s, e) => CodesToText()),
                    Btn("→ hex bytes (UTF-8)", (s, e) => _textOut.Text = BitConverter.ToString(Encoding.UTF8.GetBytes(_textIn.Text)).Replace("-", " ")),
                    Btn("→ UO Unicode (BE)", (s, e) => _textOut.Text = BitConverter.ToString(Encoding.BigEndianUnicode.GetBytes(_textIn.Text)).Replace("-", " "))),
                Row(Btn("UPPER", (s, e) => _textOut.Text = _textIn.Text.ToUpperInvariant()),
                    Btn("lower", (s, e) => _textOut.Text = _textIn.Text.ToLowerInvariant()),
                    Btn("Title Case", (s, e) => _textOut.Text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(_textIn.Text.ToLowerInvariant()))),
                _textOut);
            FillTab(tabConvert, new Control[] { numbers, formula }, new Control[] { text });
        }

        private void ConvertNumber()
        {
            string t = _convInput.Text;
            if (string.IsNullOrWhiteSpace(t))
            {
                _convOut.Text = string.Empty;
                return;
            }
            long v;
            bool ok;
            switch (_convStyle.SelectedIndex)
            {
                case 1: ok = UoCalc.TryParse(t, NumberStyle.Hex, out v); break;
                case 2: ok = UoCalc.TryParse(t, NumberStyle.Decimal, out v); break;
                case 3: ok = UoCalc.TryFromBase(t.StartsWith("0b", StringComparison.OrdinalIgnoreCase) ? t.Substring(2) : t, 2, out v); break;
                case 4: ok = UoCalc.TryFromBase(t, 8, out v); break;
                case 5: ok = UoCalc.TryFromBase(t, (int)_convBase.Value, out v); break;
                default: ok = UoCalc.TryParse(t, NumberStyle.Auto, out v); break;
            }
            if (!ok)
            {
                _convOut.Text = "⚠ not a valid number for this input type";
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Decimal   : {v}");
            sb.AppendLine($"Hex       : {UoCalc.Hex(v, 1)}   ({UoCalc.Hex(v, 4)})");
            sb.AppendLine($"Binary    : {UoCalc.Binary(v)}");
            sb.AppendLine($"Octal     : {UoCalc.ToBase(v, 8)}");
            sb.AppendLine($"Base {(int)_convBase.Value,-5}: {UoCalc.ToBase(v, (int)_convBase.Value)}");
            if (v >= short.MinValue && v <= ushort.MaxValue)
            {
                sb.AppendLine($"16 bit    : signed {(short)(ushort)v}, unsigned {(ushort)v}");
                sb.AppendLine($"Bytes 16  : LE {(byte)v:X2} {(byte)(v >> 8):X2}   BE {(byte)(v >> 8):X2} {(byte)v:X2}");
            }
            if (v >= int.MinValue && v <= uint.MaxValue)
            {
                byte[] le = BitConverter.GetBytes((uint)v);
                sb.AppendLine($"Bytes 32  : LE {BitConverter.ToString(le).Replace("-", " ")}   BE {BitConverter.ToString(le.Reverse().ToArray()).Replace("-", " ")}");
            }
            if (v >= 32 && v < 0x10FFFF && v != 127)
            {
                try
                {
                    sb.AppendLine($"Character : {char.ConvertFromUtf32((int)v)}");
                }
                catch (ArgumentException)
                {
                    // surrogate range
                }
            }
            if (v >= 0 && v <= 0xFFFF)
            {
                sb.AppendLine($"As gump   : {(UoCalc.GumpGender((int)v, out int anim) switch { 1 => $"male paperdoll, AnimID {anim}", 2 => $"female paperdoll, AnimID {anim}", _ => "no paperdoll gump" })}");
                sb.AppendLine($"As AnimID : male gump {UoCalc.MaleGump((int)v)}, female gump {UoCalc.FemaleGump((int)v)}");
            }
            _convOut.Text = sb.ToString().TrimEnd();
        }

        private void EvaluateFormula(bool toHistory)
        {
            if (string.IsNullOrWhiteSpace(_formula.Text))
            {
                _formulaOut.Text = string.Empty;
                return;
            }
            try
            {
                long v = UoCalc.Evaluate(_formula.Text);
                _formulaOut.ForeColor = SystemColors.WindowText;
                _formulaOut.Text = $"= {v}   = {UoCalc.Hex(v, 1)}";
                if (toHistory)
                {
                    AddHistory($"{_formula.Text.Trim()} = {v} ({UoCalc.Hex(v, 1)})");
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is DivideByZeroException || ex is OverflowException)
            {
                _formulaOut.ForeColor = Color.Firebrick;
                _formulaOut.Text = "⚠ " + ex.Message;
            }
        }

        private void CodesToText()
        {
            var sb = new StringBuilder();
            foreach (string part in _textIn.Text.Split(new[] { ' ', ',', ';', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!UoCalc.TryParseInt(part, out int code) || code < 0 || code > 0x10FFFF || (code >= 0xD800 && code <= 0xDFFF))
                {
                    _textOut.Text = $"⚠ \"{part}\" is not a character code";
                    return;
                }
                sb.Append(char.ConvertFromUtf32(code));
            }
            _textOut.Text = sb.ToString();
        }

        #endregion

        #region Keyboard, help

        private void ShowHelp() => GumpCalcHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ShowHelp();
                return true;
            }
            if ((keyData & Keys.Control) == Keys.Control && (keyData & Keys.Alt) == 0)
            {
                Keys key = keyData & Keys.KeyCode;
                if (key >= Keys.D1 && key <= Keys.D8)
                {
                    tabControl1.SelectedIndex = key - Keys.D1;
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextConverter
{
    /// <summary>
    /// Content of the tab "Text Converter": multi formats (old Sphere / UOA → UoFiddler import, addon scripts, MULTIDEF …),
    /// number columns, Sphere hex, coordinates and small text tools. Input left, output right, options on top.
    /// </summary>
    internal sealed class TextConverterPanel : UserControl
    {
        private enum Mode
        {
            Multi, ColumnsToHex, ColumnsToDecimal, SphereToCSharp, CSharpToSphere, Coordinates,
            ToBinary, FromBinary, ToMorse, FromMorse, ToBase64, FromBase64, ToHexBytes, FromHexBytes, Random
        }

        private static readonly string[] ModeNames =
        {
            "Multi: convert formats (Sphere / UOA → UoFiddler import, addon script …)",
            "Numbers in columns: decimal → hex",
            "Numbers in columns: hex → decimal",
            "Sphere hex (0abc) → C# hex (0xABC)",
            "C# hex (0xABC) → Sphere hex (0abc)",
            "Coordinates → Point3D / Sphere P= / [go",
            "Text → binary",
            "Binary → text",
            "Text → Morse code",
            "Morse code → text",
            "Text → Base64",
            "Base64 → text",
            "Text → hex bytes",
            "Hex bytes → text",
            "Random numbers"
        };

        private readonly ToolTip _tip = new ToolTip { AutoPopDelay = 15000 };
        private readonly Timer _liveTimer = new Timer { Interval = 400 };
        private readonly Random _random = new Random();
        private ComboBox _mode;
        private CheckBox _live;
        private TextBox _input, _output;
        private Label _status, _inputLabel, _outputLabel;
        private FlowLayoutPanel _options;
        private readonly Dictionary<Mode, Control[]> _optionSets = new Dictionary<Mode, Control[]>();
        private List<string> _skipped = new List<string>();
        private List<MultiPart> _lastParts = new List<MultiPart>();
        private string _lastFolder;
        private string _inputName = "converted";

        public TextConverterPanel()
        {
            Dock = DockStyle.Fill;
            BuildUi();
            _liveTimer.Tick += (s, e) =>
            {
                _liveTimer.Stop();
                Run(false);
            };
            ShowOptions();
        }

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private Mode CurrentMode => (Mode)Math.Max(0, _mode.SelectedIndex);

        #region UI

        private Button Btn(string text, EventHandler click, string tip = null)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            b.Click += click;
            if (tip != null)
            {
                _tip.SetToolTip(b, tip);
            }
            return b;
        }

        private static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(6, 7, 2, 0) };

        private static NumericUpDown Num(int min, int max, int value, int width = 60) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = width, Margin = new Padding(2, 3, 2, 0) };

        private static ComboBox Combo(int width, params string[] items)
        {
            var c = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = width, Margin = new Padding(2, 3, 2, 0) };
            c.Items.AddRange(items.Cast<object>().ToArray());
            c.SelectedIndex = 0;
            return c;
        }

        // multi options
        private ComboBox _inFormat, _idStyle, _outFormat, _origin, _flags;
        private NumericUpDown _ox, _oy, _oz, _dx, _dy, _dz, _bx, _by, _bz, _multiId;
        private CheckBox _dupes, _sort;
        private TextBox _replace, _className, _name;
        // other options
        private TextBox _columns;
        private NumericUpDown _digits, _sphereMap, _rndCount, _rndMin, _rndMax;
        private ComboBox _coordFormat, _mapName;
        private CheckBox _rndUnique;

        private void BuildUi()
        {
            _mode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 420, Margin = new Padding(2, 3, 2, 0) };
            _mode.Items.AddRange(ModeNames.Cast<object>().ToArray());
            _mode.SelectedIndex = 0;
            _mode.SelectedIndexChanged += (s, e) => { ShowOptions(); Run(false); };
            _live = new CheckBox { Text = "Live", Checked = true, AutoSize = true, Margin = new Padding(6, 6, 2, 0) };
            _tip.SetToolTip(_live, Tip("Convert while typing / changing options.", "Beim Tippen / Ändern sofort umwandeln."));

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(2) };
            top.Controls.AddRange(new Control[]
            {
                _mode,
                Btn("Convert (F5)", (s, e) => Run(true)), _live,
                Btn("Open…", (s, e) => OpenInput(), Tip("Open a text file into the input.", "Textdatei in die Eingabe laden.")),
                Btn("Paste", (s, e) => PasteInput()),
                Btn("Save output…", (s, e) => SaveOutput()),
                Btn("Copy output", (s, e) => CopyOutput()),
                Btn("⇦ Output → input", (s, e) => { _input.Text = _output.Text; }, Tip("Use the result as new input (e.g. for a second step).", "Ergebnis als neue Eingabe (z. B. für einen zweiten Schritt).")),
                Btn("Clear", (s, e) => { _input.Clear(); _output.Clear(); SetStatus(string.Empty); }),
                Btn("?", (s, e) => TextConverterHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de"), "Help / Hilfe (F1)")
            });

            // ---- multi options
            _inFormat = Combo(250, MultiFormats.InputNames);
            _idStyle = Combo(80, "IDs: auto", "IDs: decimal", "IDs: hex");
            _tip.SetToolTip(_idStyle, Tip("How IDs without 0x are read. Auto: letters A-F = hex, otherwise decimal (old Sphere / UOA files are decimal).",
                "Wie IDs ohne 0x gelesen werden. Auto: Buchstaben A-F = Hex, sonst Dezimal (alte Sphere- / UOA-Dateien sind dezimal)."));
            _outFormat = Combo(290, MultiFormats.OutputNames);
            _origin = Combo(150, "Origin: keep", "Origin: center", "Origin: min corner", "Origin: manual X / Y / Z");
            _tip.SetToolTip(_origin, Tip("Moves the parts so that this point becomes 0 / 0 / 0 – needed for world coordinates.",
                "Verschiebt die Teile, sodass dieser Punkt 0 / 0 / 0 wird – nötig bei Weltkoordinaten."));
            _ox = Num(-65535, 65535, 0, 65); _oy = Num(-65535, 65535, 0, 65); _oz = Num(-128, 127, 0, 50);
            _dx = Num(-1000, 1000, 0, 55); _dy = Num(-1000, 1000, 0, 55); _dz = Num(-127, 127, 0, 50);
            _flags = Combo(110, "Flags: keep", "Flags: all 0", "Flags: all 1");
            _tip.SetToolTip(_flags, Tip("Flags in multi.mul: 1 = visible part (most tiles), 0 = only for the house designer.",
                "Flags in multi.mul: 1 = sichtbares Teil (die meisten), 0 = nur für den Hausbau."));
            _dupes = new CheckBox { Text = "Remove duplicates", AutoSize = true, Margin = new Padding(6, 6, 2, 0) };
            _sort = new CheckBox { Text = "Sort (draw order)", AutoSize = true, Margin = new Padding(6, 6, 2, 0) };
            _replace = new TextBox { Width = 170, Margin = new Padding(2, 3, 2, 0), PlaceholderText = "replace: 0x6A5=0x6A6; 99=100" };
            _tip.SetToolTip(_replace, Tip("Replace item IDs: old=new, separated by ;", "Item-IDs ersetzen: alt=neu, getrennt mit ;"));
            _className = new TextBox { Width = 120, Text = "AlchemistAddon", Margin = new Padding(2, 3, 2, 0) };
            _name = new TextBox { Width = 120, Text = "alchemist shop", Margin = new Padding(2, 3, 2, 0) };
            _bx = Num(0, 65535, 1405, 65); _by = Num(0, 65535, 1709, 65); _bz = Num(-128, 127, 0, 50);
            _multiId = Num(0, 0x21FF, 0x13EC, 70);
            _multiId.Hexadecimal = true;
            _tip.SetToolTip(_multiId, Tip("Multi ID (hex) in multi.mul", "Multi-ID (hex) in der multi.mul"));
            _optionSets[Mode.Multi] = new Control[]
            {
                Lbl("In:"), _inFormat, _idStyle, Lbl("Out:"), _outFormat, Lbl(""),
                _origin, Lbl("X"), _ox, Lbl("Y"), _oy, Lbl("Z"), _oz,
                Lbl("Shift X"), _dx, Lbl("Y"), _dy, Lbl("Z"), _dz, _flags, _dupes, _sort, _replace,
                Lbl("Class / defname:"), _className, Lbl("Name:"), _name, Lbl("Decoration base X"), _bx, Lbl("Y"), _by, Lbl("Z"), _bz,
                Btn("Preview", (s, e) => ShowPreview(), Tip("Shows the multi as picture (needs the client files).", "Zeigt das Multi als Bild (braucht die Client-Dateien).")),
                Lbl("multi.mul ID 0x"), _multiId,
                Btn("→ multi.mul", (s, e) => WriteMulti(), Tip("Puts the parts directly into multi.mul at this ID (memory) – no text import needed.",
                    "Setzt die Teile direkt in die multi.mul an diese ID (Speicher) – kein Text-Import nötig.")),
                Btn("Save multi.mul", (s, e) => SaveMultiMul()),
                Btn("Batch files…", (s, e) => Batch(), Tip("Convert many files at once (e.g. a folder of old Sphere multis) with these settings.",
                    "Viele Dateien auf einmal umwandeln (z. B. einen Ordner alter Sphere-Multis) mit diesen Einstellungen."))
            };

            // ---- columns
            _columns = new TextBox { Width = 80, Text = "1", Margin = new Padding(2, 3, 2, 0) };
            _tip.SetToolTip(_columns, Tip("Which columns: 1 = first number of each line, \"1,3\" or \"all\".", "Welche Spalten: 1 = erste Zahl jeder Zeile, \"1,3\" oder \"all\"."));
            _digits = Num(1, 8, 4, 45);
            _optionSets[Mode.ColumnsToHex] = new Control[] { Lbl("Columns:"), _columns, Lbl("Hex digits:"), _digits };
            _optionSets[Mode.ColumnsToDecimal] = new Control[] { Lbl("Columns:"), _columns };

            // ---- coordinates
            _coordFormat = Combo(270, TextTools.CoordinateOutputs);
            _mapName = Combo(100, "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur");
            _sphereMap = Num(0, 255, 0, 45);
            _optionSets[Mode.Coordinates] = new Control[] { Lbl("Format:"), _coordFormat, Lbl("ServUO map:"), _mapName, Lbl("Sphere map:"), _sphereMap };

            // ---- random
            _rndCount = Num(1, 100000, 1, 70); _rndMin = Num(int.MinValue, int.MaxValue, 1, 90); _rndMax = Num(int.MinValue, int.MaxValue, 100, 90);
            _rndUnique = new CheckBox { Text = "no repeats", AutoSize = true, Margin = new Padding(6, 6, 2, 0) };
            _optionSets[Mode.Random] = new Control[] { Lbl("Count:"), _rndCount, Lbl("Min:"), _rndMin, Lbl("Max:"), _rndMax, _rndUnique };

            foreach (Control c in _optionSets.Values.SelectMany(x => x))
            {
                switch (c)
                {
                    case ComboBox cb: cb.SelectedIndexChanged += (s, e) => { UpdateEnabled(); ScheduleLive(); }; break;
                    case NumericUpDown n when n != _multiId: n.ValueChanged += (s, e) => ScheduleLive(); break;
                    case CheckBox ch: ch.CheckedChanged += (s, e) => ScheduleLive(); break;
                    case TextBox t: t.TextChanged += (s, e) => ScheduleLive(); break;
                }
            }

            _options = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(2), BackColor = Color.FromArgb(240, 243, 248) };

            _input = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9.75f), AcceptsTab = true, MaxLength = 0 };
            _input.TextChanged += (s, e) => ScheduleLive();
            _output = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9.75f), MaxLength = 0, BackColor = Color.FromArgb(250, 250, 240) };
            _inputLabel = new Label { Dock = DockStyle.Top, Height = 18, Text = "Input", Font = new Font(Font, FontStyle.Bold) };
            _outputLabel = new Label { Dock = DockStyle.Top, Height = 18, Text = "Output", Font = new Font(Font, FontStyle.Bold) };
            var split = new SplitContainer { Dock = DockStyle.Fill };
            split.Panel1.Controls.Add(_input);
            split.Panel1.Controls.Add(_inputLabel);
            split.Panel2.Controls.Add(_output);
            split.Panel2.Controls.Add(_outputLabel);
            bool splitSet = false;
            split.SizeChanged += (s, e) =>
            {
                // once, when the tab has its real size (setting it earlier gets clamped)
                if (!splitSet && split.Width > 200)
                {
                    split.SplitterDistance = split.Width / 2;
                    splitSet = true;
                }
            };

            _status = new Label { Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand };
            _status.Click += (s, e) => ShowSkipped();
            _tip.SetToolTip(_status, Tip("Click: list of the lines that could not be read", "Klick: Liste der Zeilen, die nicht gelesen werden konnten"));

            Controls.Add(split);
            Controls.Add(_options);
            Controls.Add(top);
            Controls.Add(_status);
        }

        private void ShowOptions()
        {
            _options.SuspendLayout();
            _options.Controls.Clear();
            if (_optionSets.TryGetValue(CurrentMode, out Control[] set))
            {
                _options.Controls.AddRange(set);
                _options.Visible = true;
            }
            else
            {
                _options.Visible = false;
            }
            _options.ResumeLayout();
            UpdateEnabled();
            _inputLabel.Text = CurrentMode switch
            {
                Mode.Multi => "Input – multi text (any format, e.g. old Sphere / UOA file)",
                Mode.Coordinates => "Input – coordinates (x y z [map] per line, any separators)",
                Mode.Random => "Input – (not used)",
                _ => "Input"
            };
        }

        private void UpdateEnabled()
        {
            bool manual = _origin.SelectedIndex == 3;
            _ox.Enabled = _oy.Enabled = _oz.Enabled = manual;
            var o = (MultiOutput)_outFormat.SelectedIndex;
            _className.Enabled = _name.Enabled = o == MultiOutput.ServUOAddon || o == MultiOutput.SphereMultiDef;
            _bx.Enabled = _by.Enabled = _bz.Enabled = o == MultiOutput.Decoration;
        }

        private void ScheduleLive()
        {
            if (!_live.Checked || _input.TextLength > 2_000_000)
            {
                return;
            }
            _liveTimer.Stop();
            _liveTimer.Start();
        }

        private void SetStatus(string text, bool warning = false)
        {
            _status.Text = text;
            _status.ForeColor = warning ? Color.Firebrick : SystemColors.ControlText;
        }

        #endregion

        #region Convert

        private MultiOptions ReadMultiOptions() => new MultiOptions
        {
            Input = (MultiInput)_inFormat.SelectedIndex,
            Ids = (IdStyle)_idStyle.SelectedIndex,
            Origin = (OriginMode)_origin.SelectedIndex,
            OriginX = (int)_ox.Value, OriginY = (int)_oy.Value, OriginZ = (int)_oz.Value,
            ShiftX = (int)_dx.Value, ShiftY = (int)_dy.Value, ShiftZ = (int)_dz.Value,
            FlagsMode = _flags.SelectedIndex,
            RemoveDuplicates = _dupes.Checked,
            Sort = _sort.Checked,
            Replace = _replace.Text,
            ClassName = _className.Text.Trim(),
            Name = _name.Text.Trim(),
            BaseX = (int)_bx.Value, BaseY = (int)_by.Value, BaseZ = (int)_bz.Value
        };

        /// <summary>Converts input → output. explicit = the user pressed Convert (errors are shown louder).</summary>
        private void Run(bool explicitRun)
        {
            _skipped = new List<string>();
            string input = _input.Text;
            if (input.Length == 0 && CurrentMode != Mode.Random)
            {
                _output.Clear();
                SetStatus(explicitRun ? "The input is empty – open a file or paste text. / Eingabe ist leer." : string.Empty);
                return;
            }
            try
            {
                int n;
                switch (CurrentMode)
                {
                    case Mode.Multi:
                    {
                        MultiOptions o = ReadMultiOptions();
                        List<MultiPart> parts = MultiFormats.Parse(input, o, out MultiInput used, _skipped);
                        var notes = new List<string>();
                        parts = MultiFormats.Transform(parts, o, notes);
                        _lastParts = parts;
                        _output.Text = MultiFormats.Write(parts, (MultiOutput)_outFormat.SelectedIndex, o);
                        string read = o.Input == MultiInput.Auto ? $"recognized: {MultiFormats.InputNames[(int)used]} · " : string.Empty;
                        SetStatus(read + MultiFormats.Describe(parts) + (notes.Count > 0 ? " · " + string.Join(" · ", notes) : string.Empty)
                                  + (_skipped.Count > 0 ? $" · ⚠ {_skipped.Count} lines skipped (click)" : string.Empty), _skipped.Count > 0 || notes.Any(x => x.StartsWith("⚠")));
                        break;
                    }
                    case Mode.ColumnsToHex:
                        _output.Text = TextTools.ConvertColumns(input, _columns.Text, true, (int)_digits.Value, out n);
                        SetStatus($"{n} numbers converted to hex.");
                        break;
                    case Mode.ColumnsToDecimal:
                        _output.Text = TextTools.ConvertColumns(input, _columns.Text, false, 0, out n);
                        SetStatus($"{n} hex numbers converted to decimal.");
                        break;
                    case Mode.SphereToCSharp:
                        _output.Text = TextTools.SphereHex(input, true, out n);
                        SetStatus($"{n} Sphere hex numbers changed.");
                        break;
                    case Mode.CSharpToSphere:
                        _output.Text = TextTools.SphereHex(input, false, out n);
                        SetStatus($"{n} hex numbers changed.");
                        break;
                    case Mode.Coordinates:
                        _output.Text = TextTools.Coordinates(input, _coordFormat.SelectedIndex, (string)_mapName.SelectedItem, (int)_sphereMap.Value, _skipped);
                        SetStatus(_skipped.Count > 0 ? $"⚠ {_skipped.Count} lines without coordinates (click)" : "Done.", _skipped.Count > 0);
                        break;
                    case Mode.ToBinary:
                        _output.Text = TextTools.ToBinary(input);
                        SetStatus("Text → binary (UTF-8).");
                        break;
                    case Mode.FromBinary:
                        _output.Text = TextTools.FromBinary(input);
                        SetStatus("Binary → text.");
                        break;
                    case Mode.ToMorse:
                        _output.Text = TextTools.ToMorse(input, out n);
                        SetStatus(n > 0 ? $"⚠ {n} characters have no Morse code (shown as ?)" : "Text → Morse.", n > 0);
                        break;
                    case Mode.FromMorse:
                        _output.Text = TextTools.FromMorse(input, out n);
                        SetStatus(n > 0 ? $"⚠ {n} codes unknown (shown as ?)" : "Morse → text.", n > 0);
                        break;
                    case Mode.ToBase64:
                        _output.Text = TextTools.ToBase64(input);
                        SetStatus("Text → Base64.");
                        break;
                    case Mode.FromBase64:
                        _output.Text = TextTools.FromBase64(input);
                        SetStatus("Base64 → text.");
                        break;
                    case Mode.ToHexBytes:
                        _output.Text = TextTools.ToHexBytes(input);
                        SetStatus("Text → hex bytes (UTF-8).");
                        break;
                    case Mode.FromHexBytes:
                        _output.Text = TextTools.FromHexBytes(input);
                        SetStatus("Hex bytes → text.");
                        break;
                    case Mode.Random:
                        if (!explicitRun)
                        {
                            return;
                        }
                        _output.Text = TextTools.RandomNumbers((int)_rndCount.Value, (int)_rndMin.Value, (int)_rndMax.Value, _rndUnique.Checked, _random);
                        SetStatus($"{_rndCount.Value} random numbers.");
                        break;
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is OverflowException)
            {
                SetStatus("⚠ " + ex.Message, true);
                if (explicitRun)
                {
                    _output.Clear();
                }
            }
        }

        private void ShowSkipped()
        {
            if (_skipped.Count == 0)
            {
                return;
            }
            MessageBox.Show(this, "Not read / nicht gelesen:\n\n" + string.Join("\n", _skipped.Take(40)) + (_skipped.Count > 40 ? $"\n… {_skipped.Count - 40} more" : string.Empty),
                "Text Converter", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Files

        public static string ReadText(string file)
        {
            byte[] bytes = File.ReadAllBytes(file);
            try
            {
                return new UTF8Encoding(false, true).GetString(bytes).TrimStart('﻿');
            }
            catch (DecoderFallbackException)
            {
                return Encoding.Latin1.GetString(bytes);
            }
        }

        private void OpenInput()
        {
            using (var d = new OpenFileDialog { Filter = "Text files|*.txt;*.uoa;*.csv;*.wsc;*.xml;*.scp;*.cs;*.cfg|All files|*.*" })
            {
                if (_lastFolder != null && Directory.Exists(_lastFolder))
                {
                    d.InitialDirectory = _lastFolder;
                }
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    _lastFolder = Path.GetDirectoryName(d.FileName);
                    _inputName = Path.GetFileNameWithoutExtension(d.FileName);
                    _input.Text = ReadText(d.FileName);
                    if (CurrentMode == Mode.Multi && _className.Text == "AlchemistAddon")
                    {
                        string cls = new string(_inputName.Where(char.IsLetterOrDigit).ToArray());
                        if (cls.Length > 0)
                        {
                            _className.Text = char.ToUpperInvariant(cls[0]) + cls.Substring(1) + "Addon";
                            _name.Text = _inputName;
                        }
                    }
                    Run(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Opening failed:\n" + ex.Message, "Text Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PasteInput()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    _input.Text = Clipboard.GetText();
                    Run(true);
                }
                else
                {
                    SetStatus("The clipboard contains no text.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Clipboard: " + ex.Message, true);
            }
        }

        private string OutputExtension()
        {
            if (CurrentMode != Mode.Multi)
            {
                return "txt";
            }
            switch ((MultiOutput)_outFormat.SelectedIndex)
            {
                case MultiOutput.Csv: return "csv";
                case MultiOutput.Wsc: return "wsc";
                case MultiOutput.Xml: return "xml";
                case MultiOutput.ServUOAddon:
                case MultiOutput.CSharpArray: return "cs";
                case MultiOutput.SphereMultiDef: return "scp";
                case MultiOutput.Decoration: return "cfg";
                case MultiOutput.Uoa: return "uoa.txt";
                default: return "txt";
            }
        }

        private void SaveOutput()
        {
            if (_output.TextLength == 0)
            {
                SetStatus("There is no output to save. / Keine Ausgabe zum Speichern.", true);
                return;
            }
            string ext = OutputExtension();
            string name = CurrentMode == Mode.Multi && (MultiOutput)_outFormat.SelectedIndex == MultiOutput.ServUOAddon ? _className.Text.Trim() : _inputName;
            using (var d = new SaveFileDialog { FileName = name + "." + ext, Filter = $"*.{ext}|*.{ext}|All files|*.*" })
            {
                if (_lastFolder != null && Directory.Exists(_lastFolder))
                {
                    d.InitialDirectory = _lastFolder;
                }
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    // without BOM: UoFiddler's TXT import would read the BOM as part of the first ID
                    File.WriteAllText(d.FileName, _output.Text, new UTF8Encoding(false));
                    _lastFolder = Path.GetDirectoryName(d.FileName);
                    SetStatus("Saved: " + d.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Text Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopyOutput()
        {
            if (_output.TextLength == 0)
            {
                SetStatus("There is no output to copy.", true);
                return;
            }
            try
            {
                Clipboard.SetText(_output.Text);
                SetStatus("Output copied.");
            }
            catch (Exception ex)
            {
                SetStatus("Clipboard: " + ex.Message, true);
            }
        }

        private void Batch()
        {
            using (var d = new OpenFileDialog { Multiselect = true, Filter = "Text files|*.txt;*.uoa;*.csv;*.wsc;*.xml;*.scp;*.cs|All files|*.*", Title = "Files to convert" })
            {
                if (_lastFolder != null && Directory.Exists(_lastFolder))
                {
                    d.InitialDirectory = _lastFolder;
                }
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                using (var f = new FolderBrowserDialog { Description = "Output folder / Ausgabeordner", SelectedPath = Path.GetDirectoryName(d.FileNames[0]) })
                {
                    if (f.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    MultiOptions o = ReadMultiOptions();
                    var output = (MultiOutput)_outFormat.SelectedIndex;
                    string ext = OutputExtension();
                    var report = new StringBuilder();
                    int ok = 0;
                    foreach (string file in d.FileNames)
                    {
                        try
                        {
                            var skipped = new List<string>();
                            List<MultiPart> parts = MultiFormats.Transform(MultiFormats.Parse(ReadText(file), o, out _, skipped), o, null);
                            string baseName = Path.GetFileNameWithoutExtension(file);
                            if (output == MultiOutput.ServUOAddon || output == MultiOutput.SphereMultiDef)
                            {
                                string cls = new string(baseName.Where(char.IsLetterOrDigit).ToArray());
                                o.ClassName = (cls.Length > 0 ? char.ToUpperInvariant(cls[0]) + cls.Substring(1) : "Multi") + "Addon";
                                o.Name = baseName;
                            }
                            string target = Path.Combine(f.SelectedPath, baseName + "." + ext);
                            if (string.Equals(Path.GetFullPath(target), Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase))
                            {
                                target = Path.Combine(f.SelectedPath, baseName + "_converted." + ext);
                            }
                            File.WriteAllText(target, MultiFormats.Write(parts, output, o), new UTF8Encoding(false));
                            report.AppendLine($"✓ {Path.GetFileName(file)} → {Path.GetFileName(target)}: {parts.Count} parts" + (skipped.Count > 0 ? $", {skipped.Count} lines skipped" : string.Empty));
                            ok++;
                        }
                        catch (Exception ex)
                        {
                            report.AppendLine($"✗ {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                    _output.Text = report.ToString();
                    SetStatus($"Batch: {ok} of {d.FileNames.Length} files converted into {f.SelectedPath}.", ok < d.FileNames.Length);
                }
            }
        }

        #endregion

        #region multi.mul

        private bool EnsureParts()
        {
            if (CurrentMode != Mode.Multi)
            {
                return false;
            }
            Run(true);
            if (_lastParts.Count == 0)
            {
                SetStatus("No multi parts – convert a multi first.", true);
                return false;
            }
            return true;
        }

        private MultiComponentList BuildList()
        {
            var entries = _lastParts.Select(p => new MultiComponentList.MultiTileEntry
            {
                ItemId = (ushort)Math.Max(0, Math.Min(0xFFFF, p.Id)),
                OffsetX = (short)p.X,
                OffsetY = (short)p.Y,
                OffsetZ = (short)p.Z,
                Flags = p.Flags,
                Unk1 = 0
            }).ToList();
            lock (SdkSync.Lock)
            {
                return new MultiComponentList(entries);
            }
        }

        private void ShowPreview()
        {
            if (!EnsureParts())
            {
                return;
            }
            Bitmap img;
            try
            {
                lock (SdkSync.Lock)
                {
                    img = BuildList().GetImage(600);
                }
            }
            catch (Exception ex)
            {
                SetStatus("Preview failed (client files loaded?): " + ex.Message, true);
                return;
            }
            if (img == null)
            {
                SetStatus("Nothing to show.", true);
                return;
            }
            var form = new Form { Text = $"Multi preview – {_lastParts.Count} parts", Size = new Size(Math.Min(1200, img.Width + 40), Math.Min(900, img.Height + 60)), StartPosition = FormStartPosition.CenterParent, ShowIcon = false };
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(40, 40, 40) };
            panel.Controls.Add(new PictureBox { Image = img, SizeMode = PictureBoxSizeMode.AutoSize });
            form.Controls.Add(panel);
            form.FormClosed += (s, e) => img.Dispose();
            form.Show(FindForm());
        }

        private void WriteMulti()
        {
            if (!EnsureParts())
            {
                return;
            }
            int id = (int)_multiId.Value;
            bool used = false;
            try
            {
                lock (SdkSync.Lock)
                {
                    MultiComponentList existing = Multis.GetComponents(id);
                    used = existing != null && existing != MultiComponentList.Empty && existing.SortedTiles != null && existing.SortedTiles.Length > 0;
                }
            }
            catch (Exception)
            {
                used = false;
            }
            string question = $"Put {_lastParts.Count} parts into multi.mul at 0x{id:X4} ({id})?" + (used ? "\n\n⚠ This ID already holds a multi – it will be replaced." : string.Empty);
            if (MessageBox.Show(this, question, "Text Converter", MessageBoxButtons.YesNo, used ? MessageBoxIcon.Warning : MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                MultiComponentList list = BuildList();
                lock (SdkSync.Lock)
                {
                    Multis.Add(id, list);
                }
                UoFiddler.Controls.Classes.Options.ChangedUltimaClass["Multis"] = true;
                UoFiddler.Controls.Classes.ControlEvents.FireMultiChangeEvent(this, id);
                SetStatus($"Multi 0x{id:X4} replaced in memory – \"Save multi.mul\" (here or in UoFiddler) writes the file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Writing failed:\n" + ex.Message, "Text Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMultiMul()
        {
            string path = UoFiddler.Controls.Classes.Options.OutputPath;
            if (MessageBox.Show(this, $"Save multi.idx / multi.mul into\n{path}?", "Text Converter", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    Multis.Save(path);
                }
                UoFiddler.Controls.Classes.Options.ChangedUltimaClass["Multis"] = false;
                SetStatus("multi.mul saved in " + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Text Converter", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5)
            {
                Run(true);
                return true;
            }
            if (keyData == Keys.F1)
            {
                TextConverterHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _liveTimer.Dispose();
                _tip.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

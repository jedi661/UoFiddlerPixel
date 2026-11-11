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
    public partial class TextConverterPanel : UserControl
    {
        private enum Mode
        {
            Multi, ColumnsToHex, ColumnsToDecimal, SphereToCSharp, CSharpToSphere, Coordinates,
            ToBinary, FromBinary, ToMorse, FromMorse, ToBase64, FromBase64, ToHexBytes, FromHexBytes, Random
        }

        private readonly Random _random = new Random();
        private List<string> _skipped = new List<string>();
        private List<MultiPart> _lastParts = new List<MultiPart>();
        private string _lastFolder;
        private string _inputName = "converted";

        public TextConverterPanel()
        {
            InitializeComponent();
            // the designer does not store SelectedIndex – start with the first entry
            foreach (ComboBox c in new[] { comboMode, comboInFormat, comboIdStyle, comboOutFormat, comboOrigin, comboFlags, comboCoordFormat, comboMapName })
            {
                if (c.Items.Count > 0 && c.SelectedIndex < 0)
                {
                    c.SelectedIndex = 0;
                }
            }
            ShowOptions();
        }

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private Mode CurrentMode => (Mode)Math.Max(0, comboMode.SelectedIndex);

        #region UI events

        private void ComboMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowOptions();
            Run(false);
        }

        /// <summary>Every option field: update the enabled fields and convert again (Live).</summary>
        private void Option_Changed(object sender, EventArgs e)
        {
            UpdateEnabled();
            ScheduleLive();
        }

        private void LiveTimer_Tick(object sender, EventArgs e)
        {
            liveTimer.Stop();
            Run(false);
        }

        private void BtnConvert_Click(object sender, EventArgs e) => Run(true);

        private void BtnOpen_Click(object sender, EventArgs e) => OpenInput();

        private void BtnPaste_Click(object sender, EventArgs e) => PasteInput();

        private void BtnSaveOutput_Click(object sender, EventArgs e) => SaveOutput();

        private void BtnCopyOutput_Click(object sender, EventArgs e) => CopyOutput();

        private void BtnOutputToInput_Click(object sender, EventArgs e) => txtInput.Text = txtOutput.Text;

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            txtOutput.Clear();
            SetStatus(string.Empty);
        }

        private void BtnHelp_Click(object sender, EventArgs e) => TextConverterHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        private void BtnPreview_Click(object sender, EventArgs e) => ShowPreview();

        private void BtnWriteMulti_Click(object sender, EventArgs e) => WriteMulti();

        private void BtnSaveMulti_Click(object sender, EventArgs e) => SaveMultiMul();

        private void BtnBatch_Click(object sender, EventArgs e) => Batch();

        private void LblStatus_Click(object sender, EventArgs e) => ShowSkipped();

        /// <summary>Shows the option row of the chosen conversion (the rows are separate panels in the designer).</summary>
        private void ShowOptions()
        {
            Mode m = CurrentMode;
            flowMulti.Visible = m == Mode.Multi;
            flowColumns.Visible = m == Mode.ColumnsToHex || m == Mode.ColumnsToDecimal;
            lblDigits.Visible = numDigits.Visible = m == Mode.ColumnsToHex;
            flowCoordinates.Visible = m == Mode.Coordinates;
            flowRandom.Visible = m == Mode.Random;
            UpdateEnabled();
            lblInput.Text = m switch
            {
                Mode.Multi => "Input – multi text (any format, e.g. old Sphere / UOA file)",
                Mode.Coordinates => "Input – coordinates (x y z [map] per line, any separators)",
                Mode.Random => "Input – (not used)",
                _ => "Input"
            };
        }

        private void UpdateEnabled()
        {
            bool manual = comboOrigin.SelectedIndex == 3;
            numOriginX.Enabled = numOriginY.Enabled = numOriginZ.Enabled = manual;
            var o = (MultiOutput)Math.Max(0, comboOutFormat.SelectedIndex);
            txtClassName.Enabled = txtName.Enabled = o == MultiOutput.ServUOAddon || o == MultiOutput.SphereMultiDef;
            numBaseX.Enabled = numBaseY.Enabled = numBaseZ.Enabled = o == MultiOutput.Decoration;
        }

        private void ScheduleLive()
        {
            if (!chkLive.Checked || txtInput.TextLength > 2_000_000)
            {
                return;
            }
            liveTimer.Stop();
            liveTimer.Start();
        }

        private void SetStatus(string text, bool warning = false)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = warning ? Color.Firebrick : SystemColors.ControlText;
        }

        #endregion

        #region Convert

        private MultiOptions ReadMultiOptions() => new MultiOptions
        {
            Input = (MultiInput)comboInFormat.SelectedIndex,
            Ids = (IdStyle)comboIdStyle.SelectedIndex,
            Origin = (OriginMode)comboOrigin.SelectedIndex,
            OriginX = (int)numOriginX.Value, OriginY = (int)numOriginY.Value, OriginZ = (int)numOriginZ.Value,
            ShiftX = (int)numShiftX.Value, ShiftY = (int)numShiftY.Value, ShiftZ = (int)numShiftZ.Value,
            FlagsMode = comboFlags.SelectedIndex,
            RemoveDuplicates = chkDuplicates.Checked,
            Sort = chkSort.Checked,
            Replace = txtReplace.Text,
            ClassName = txtClassName.Text.Trim(),
            Name = txtName.Text.Trim(),
            BaseX = (int)numBaseX.Value, BaseY = (int)numBaseY.Value, BaseZ = (int)numBaseZ.Value
        };

        /// <summary>Converts input → output. explicit = the user pressed Convert (errors are shown louder).</summary>
        private void Run(bool explicitRun)
        {
            _skipped = new List<string>();
            string input = txtInput.Text;
            if (input.Length == 0 && CurrentMode != Mode.Random)
            {
                txtOutput.Clear();
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
                        txtOutput.Text = MultiFormats.Write(parts, (MultiOutput)comboOutFormat.SelectedIndex, o);
                        string read = o.Input == MultiInput.Auto ? $"recognized: {MultiFormats.InputNames[(int)used]} · " : string.Empty;
                        SetStatus(read + MultiFormats.Describe(parts) + (notes.Count > 0 ? " · " + string.Join(" · ", notes) : string.Empty)
                                  + (_skipped.Count > 0 ? $" · ⚠ {_skipped.Count} lines skipped (click)" : string.Empty), _skipped.Count > 0 || notes.Any(x => x.StartsWith("⚠")));
                        break;
                    }
                    case Mode.ColumnsToHex:
                        txtOutput.Text = TextTools.ConvertColumns(input, txtColumns.Text, true, (int)numDigits.Value, out n);
                        SetStatus($"{n} numbers converted to hex.");
                        break;
                    case Mode.ColumnsToDecimal:
                        txtOutput.Text = TextTools.ConvertColumns(input, txtColumns.Text, false, 0, out n);
                        SetStatus($"{n} hex numbers converted to decimal.");
                        break;
                    case Mode.SphereToCSharp:
                        txtOutput.Text = TextTools.SphereHex(input, true, out n);
                        SetStatus($"{n} Sphere hex numbers changed.");
                        break;
                    case Mode.CSharpToSphere:
                        txtOutput.Text = TextTools.SphereHex(input, false, out n);
                        SetStatus($"{n} hex numbers changed.");
                        break;
                    case Mode.Coordinates:
                        txtOutput.Text = TextTools.Coordinates(input, comboCoordFormat.SelectedIndex, (string)comboMapName.SelectedItem, (int)numSphereMap.Value, _skipped);
                        SetStatus(_skipped.Count > 0 ? $"⚠ {_skipped.Count} lines without coordinates (click)" : "Done.", _skipped.Count > 0);
                        break;
                    case Mode.ToBinary:
                        txtOutput.Text = TextTools.ToBinary(input);
                        SetStatus("Text → binary (UTF-8).");
                        break;
                    case Mode.FromBinary:
                        txtOutput.Text = TextTools.FromBinary(input);
                        SetStatus("Binary → text.");
                        break;
                    case Mode.ToMorse:
                        txtOutput.Text = TextTools.ToMorse(input, out n);
                        SetStatus(n > 0 ? $"⚠ {n} characters have no Morse code (shown as ?)" : "Text → Morse.", n > 0);
                        break;
                    case Mode.FromMorse:
                        txtOutput.Text = TextTools.FromMorse(input, out n);
                        SetStatus(n > 0 ? $"⚠ {n} codes unknown (shown as ?)" : "Morse → text.", n > 0);
                        break;
                    case Mode.ToBase64:
                        txtOutput.Text = TextTools.ToBase64(input);
                        SetStatus("Text → Base64.");
                        break;
                    case Mode.FromBase64:
                        txtOutput.Text = TextTools.FromBase64(input);
                        SetStatus("Base64 → text.");
                        break;
                    case Mode.ToHexBytes:
                        txtOutput.Text = TextTools.ToHexBytes(input);
                        SetStatus("Text → hex bytes (UTF-8).");
                        break;
                    case Mode.FromHexBytes:
                        txtOutput.Text = TextTools.FromHexBytes(input);
                        SetStatus("Hex bytes → text.");
                        break;
                    case Mode.Random:
                        if (!explicitRun)
                        {
                            return;
                        }
                        txtOutput.Text = TextTools.RandomNumbers((int)numRandomCount.Value, (int)numRandomMin.Value, (int)numRandomMax.Value, chkRandomUnique.Checked, _random);
                        SetStatus($"{numRandomCount.Value} random numbers.");
                        break;
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is OverflowException)
            {
                SetStatus("⚠ " + ex.Message, true);
                if (explicitRun)
                {
                    txtOutput.Clear();
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
                    txtInput.Text = ReadText(d.FileName);
                    if (CurrentMode == Mode.Multi && txtClassName.Text == "AlchemistAddon")
                    {
                        string cls = new string(_inputName.Where(char.IsLetterOrDigit).ToArray());
                        if (cls.Length > 0)
                        {
                            txtClassName.Text = char.ToUpperInvariant(cls[0]) + cls.Substring(1) + "Addon";
                            txtName.Text = _inputName;
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
                    txtInput.Text = Clipboard.GetText();
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
            switch ((MultiOutput)comboOutFormat.SelectedIndex)
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
            if (txtOutput.TextLength == 0)
            {
                SetStatus("There is no output to save. / Keine Ausgabe zum Speichern.", true);
                return;
            }
            string ext = OutputExtension();
            string name = CurrentMode == Mode.Multi && (MultiOutput)comboOutFormat.SelectedIndex == MultiOutput.ServUOAddon ? txtClassName.Text.Trim() : _inputName;
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
                    File.WriteAllText(d.FileName, txtOutput.Text, new UTF8Encoding(false));
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
            if (txtOutput.TextLength == 0)
            {
                SetStatus("There is no output to copy.", true);
                return;
            }
            try
            {
                Clipboard.SetText(txtOutput.Text);
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
                    var output = (MultiOutput)comboOutFormat.SelectedIndex;
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
                    txtOutput.Text = report.ToString();
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
            int id = (int)numMultiId.Value;
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
    }
}

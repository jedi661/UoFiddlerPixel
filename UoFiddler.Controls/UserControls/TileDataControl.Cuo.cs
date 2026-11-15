/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * TileDataControl – editor of the ClassicUO text files (Data\Client\*.txt):
 *  - finds the folder (ClassicUO folder, Data or Data\Client)
 *  - description of every file, check (format, unknown graphics, duplicates)
 *  - name of the tile of the line under the cursor
 *  - adds the tiles selected in the Items / Land tab
 *  - backup (.bak) before the first save, question before losing changes
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileDataControl
    {
        private CuoFileKind? _cuoKind;
        private string _cuoFile;
        private bool _cuoDirty;
        private bool _cuoLoading;
        private int _cuoIndex = -1;
        private readonly HashSet<string> _cuoBackedUp = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private string CuoFolder => CuoDataFiles.ResolveFolder(tbClassicUOPfad.Text);

        private void UpdateCuoStatus(string extra = null)
        {
            string folder = CuoFolder;
            string state = folder == null ? "No folder set." : _cuoFile == null ? $"Folder: {folder}" : $"{Path.GetFileName(_cuoFile)}{(_cuoDirty ? " – changed" : string.Empty)}";
            _lblCuoStatus.Text = extra == null ? state : state + "\n" + extra;
            if (_cuoKind.HasValue)
            {
                _btnCuoAddSelected.Enabled = CuoDataFiles.AcceptsItems(_cuoKind.Value);
            }
        }

        /// <returns>false = the user cancelled</returns>
        private bool AskSaveCuo()
        {
            if (!_cuoDirty || _cuoFile == null)
            {
                return true;
            }

            DialogResult r = MessageBox.Show($"{Path.GetFileName(_cuoFile)} was changed. Save it first?", "ClassicUO files",
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                return false;
            }

            return r == DialogResult.No || SaveCuoFile();
        }

        private void LoadCuoFile()
        {
            if (_cuoLoading || comboBoxLoadText.SelectedItem == null)
            {
                return;
            }

            if (comboBoxLoadText.SelectedIndex != _cuoIndex && !AskSaveCuo())
            {
                _cuoLoading = true;
                comboBoxLoadText.SelectedIndex = _cuoIndex;
                _cuoLoading = false;
                return;
            }

            _cuoIndex = comboBoxLoadText.SelectedIndex;
            CuoFileKind? kind = CuoDataFiles.KindOf(comboBoxLoadText.SelectedItem.ToString());
            string folder = CuoFolder;
            if (kind == null)
            {
                return;
            }

            richTextInfoCuo.Text = CuoDataFiles.Description(kind.Value);
            if (folder == null)
            {
                MessageBox.Show("Please set the ClassicUO folder first (button \"Load\").", "ClassicUO files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string file = Path.Combine(folder, CuoDataFiles.FileName(kind.Value));
            string text = string.Empty;
            if (File.Exists(file))
            {
                text = File.ReadAllText(file);
            }
            else if (MessageBox.Show($"{file}\n\ndoes not exist. ClassicUO creates it with default values on the next start.\n\nCreate an empty file now?",
                         "ClassicUO files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _cuoLoading = true;
            richTextBoxEdit.Text = text;
            ClearCuoMarks();
            _cuoLoading = false;
            _cuoKind = kind;
            _cuoFile = file;
            _cuoDirty = !File.Exists(file);
            UpdateCuoStatus();
        }

        private bool SaveCuoFile()
        {
            if (_cuoFile == null || _cuoKind == null)
            {
                MessageBox.Show("Choose a file first.", "ClassicUO files");
                return false;
            }

            List<CuoProblem> problems = CheckCuo();
            if (problems.Any(p => !p.Message.StartsWith("duplicate", StringComparison.Ordinal) && !p.Message.Contains("has no art")) &&
                MessageBox.Show($"The file has {problems.Count} problem(s), e.g.\n{problems[0]}\n\nSave anyway?", "ClassicUO files",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return false;
            }

            try
            {
                // one backup per file and session: the state before the first save
                if (File.Exists(_cuoFile) && _cuoBackedUp.Add(_cuoFile))
                {
                    File.Copy(_cuoFile, _cuoFile + ".bak", true);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(_cuoFile));
                File.WriteAllText(_cuoFile, richTextBoxEdit.Text.Replace("\r\n", "\n").Replace("\n", "\r\n"), new UTF8Encoding(false));
                _cuoDirty = false;
                UpdateCuoStatus("saved (backup: .bak)");
                if (_playCustomSound)
                {
                    using (var player = new System.Media.SoundPlayer { SoundLocation = "sound.wav" })
                    {
                        try
                        {
                            player.Play();
                        }
                        catch (Exception)
                        {
                            // no sound file
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ClassicUO files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void OnCuoTextChanged(object sender, EventArgs e)
        {
            if (_cuoLoading || _cuoFile == null)
            {
                return;
            }

            if (!_cuoDirty)
            {
                _cuoDirty = true;
                UpdateCuoStatus();
            }
        }

        /// <summary>shows what the line under the cursor refers to</summary>
        private void OnCuoSelectionChanged(object sender, EventArgs e)
        {
            if (_cuoLoading || _cuoKind == null)
            {
                return;
            }

            int lineIndex = richTextBoxEdit.GetLineFromCharIndex(richTextBoxEdit.SelectionStart);
            if (lineIndex < 0 || lineIndex >= richTextBoxEdit.Lines.Length)
            {
                return;
            }

            CuoLineInfo info = CuoDataFiles.ParseLine(_cuoKind.Value, richTextBoxEdit.Lines[lineIndex]);
            var parts = new List<string>();
            foreach (int g in info.Items)
            {
                string name = TileData.ItemTable != null && g < TileData.ItemTable.Length ? TileData.ItemTable[g].Name : "?";
                parts.Add($"item 0x{g:X4} \"{name}\"");
            }

            foreach (int g in info.Lands)
            {
                string name = TileData.LandTable != null && g < TileData.LandTable.Length ? TileData.LandTable[g].Name : "?";
                parts.Add($"land 0x{g:X4} \"{name}\"");
            }

            if (info.Gump.HasValue)
            {
                parts.Add($"gump 0x{info.Gump:X4}");
            }

            string text = $"Line {lineIndex + 1}: " + (info.IsComment ? "comment" : info.Error ?? string.Join(", ", parts));
            UpdateCuoStatus(text);
        }

        private List<CuoProblem> CheckCuo()
        {
            if (_cuoKind == null)
            {
                return new List<CuoProblem>();
            }

            bool artLoaded = TileData.ItemTable != null;
            return CuoDataFiles.Check(_cuoKind.Value, richTextBoxEdit.Text,
                artLoaded ? (Func<int, bool>)Art.IsValidStatic : null,
                artLoaded ? (Func<int, bool>)Art.IsValidLand : null,
                Gumps.IsValidIndex);
        }

        private void ClearCuoMarks()
        {
            bool loading = _cuoLoading;
            _cuoLoading = true;
            int start = richTextBoxEdit.SelectionStart, length = richTextBoxEdit.SelectionLength;
            richTextBoxEdit.SelectAll();
            richTextBoxEdit.SelectionBackColor = richTextBoxEdit.BackColor;
            richTextBoxEdit.Select(start, length);
            _cuoLoading = loading;
        }

        /// <summary>"Check": marks the lines with problems and lists them</summary>
        private void OnClickCuoCheck(object sender, EventArgs e)
        {
            if (_cuoKind == null)
            {
                return;
            }

            List<CuoProblem> problems = CheckCuo();
            _cuoLoading = true;
            ClearCuoMarks();
            foreach (int line in problems.Select(p => p.Line - 1).Distinct())
            {
                if (line < 0 || line >= richTextBoxEdit.Lines.Length)
                {
                    continue;
                }

                int start = richTextBoxEdit.GetFirstCharIndexFromLine(line);
                richTextBoxEdit.Select(start, richTextBoxEdit.Lines[line].Length);
                richTextBoxEdit.SelectionBackColor = Color.FromArgb(255, 220, 220);
            }

            richTextBoxEdit.Select(0, 0);
            _cuoLoading = false;

            if (problems.Count == 0)
            {
                UpdateCuoStatus("✔ no problems found");
                return;
            }

            UpdateCuoStatus($"{problems.Count} problem(s) – marked red");
            string list = string.Join("\n", problems.Take(25).Select(p => p.ToString())) + (problems.Count > 25 ? $"\n… and {problems.Count - 25} more" : string.Empty);
            MessageBox.Show(list, $"{Path.GetFileName(_cuoFile)} – {problems.Count} problem(s)", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            int first = problems[0].Line - 1;
            if (first >= 0 && first < richTextBoxEdit.Lines.Length)
            {
                richTextBoxEdit.Select(richTextBoxEdit.GetFirstCharIndexFromLine(first), 0);
                richTextBoxEdit.ScrollToCaret();
                richTextBoxEdit.Focus();
            }
        }

        private void OnClickCuoReload(object sender, EventArgs e)
        {
            if (_cuoFile == null)
            {
                return;
            }

            if (_cuoDirty && MessageBox.Show("Discard the changes and load the file again?", "ClassicUO files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            _cuoDirty = false;
            _cuoIndex = -1;
            LoadCuoFile();
        }

        private void OnClickCuoOpenFolder(object sender, EventArgs e)
        {
            string folder = CuoFolder;
            if (folder == null || !Directory.Exists(folder))
            {
                MessageBox.Show("The folder does not exist.", "ClassicUO files");
                return;
            }

            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }

        /// <summary>"Add selected tiles": the selected items (seasons.txt also land tiles) as new lines</summary>
        private void OnClickCuoAddSelected(object sender, EventArgs e)
        {
            if (_cuoKind == null)
            {
                MessageBox.Show("Choose a file first.", "ClassicUO files");
                return;
            }

            bool land = _cuoKind == CuoFileKind.Seasons && LandTargets().Count > 0 && ItemTargets().Count == 0;
            AddTilesToCuo(_cuoKind.Value, land ? LandTargets() : ItemTargets(), land);
        }

        /// <summary>context menu of the lists: "Add to ClassicUO file" → chair.txt …</summary>
        private void OnClickAddToCuoFile(object sender, EventArgs e)
        {
            CuoFileKind kind = sender == _miCuoChair ? CuoFileKind.Chair
                : sender == _miCuoCave ? CuoFileKind.Cave
                : sender == _miCuoVegetation ? CuoFileKind.Vegetation
                : sender == _miCuoTree ? CuoFileKind.Tree
                : sender == _miCuoLights ? CuoFileKind.Lights
                : CuoFileKind.Seasons;
            bool land = sender == _miLandAddSeasons;
            List<int> tiles = land ? LandTargets() : ItemTargets();
            if (tiles.Count == 0)
            {
                return;
            }

            if (CuoFolder == null || !Directory.Exists(CuoFolder))
            {
                tabcontrol.SelectedTab = tabPageCouEdit;
                MessageBox.Show("Please set the ClassicUO folder first (button \"Load\" in the ClassicUO tab).", "ClassicUO files");
                return;
            }

            // open the file in the editor (asks to save another changed file first)
            int index = comboBoxLoadText.Items.IndexOf(kind.ToString().ToLowerInvariant());
            if (index >= 0 && comboBoxLoadText.SelectedIndex != index)
            {
                comboBoxLoadText.SelectedIndex = index;
                if (_cuoKind != kind)
                {
                    return;
                }
            }
            else if (_cuoKind != kind)
            {
                LoadCuoFile();
            }

            tabcontrol.SelectedTab = tabPageCouEdit;
            AddTilesToCuo(kind, tiles, land);
        }

        private void AddTilesToCuo(CuoFileKind kind, List<int> tiles, bool land)
        {
            if (!CuoDataFiles.AcceptsItems(kind))
            {
                return;
            }

            HashSet<int> existing = CuoDataFiles.Keys(kind, richTextBoxEdit.Text);
            var sb = new StringBuilder();
            int added = 0, skipped = 0;
            foreach (int id in tiles.OrderBy(i => i))
            {
                if (kind != CuoFileKind.Seasons && existing.Contains(id))
                {
                    skipped++;
                    continue;
                }

                string name = land ? TileData.LandTable[id].Name : TileData.ItemTable[id].Name;
                sb.Append($"# 0x{id:X4} {name}\n");
                sb.Append(CuoDataFiles.NewLine(kind, id, land)).Append('\n');
                added++;
            }

            if (added > 0)
            {
                string text = richTextBoxEdit.Text;
                if (text.Length > 0 && !text.EndsWith("\n", StringComparison.Ordinal))
                {
                    richTextBoxEdit.AppendText("\n");
                }

                richTextBoxEdit.AppendText(sb.ToString());
                richTextBoxEdit.SelectionStart = richTextBoxEdit.TextLength;
                richTextBoxEdit.ScrollToCaret();
            }

            string hint = kind == CuoFileKind.Chair ? " – set the directions (0/2/4/6, -1) and offsets"
                : kind == CuoFileKind.Seasons ? " – set the season and the replacement graphic"
                : kind == CuoFileKind.Lights ? " – set the shader id or H<hue>" : string.Empty;
            UpdateCuoStatus($"{added} line(s) added{(skipped > 0 ? $", {skipped} already in the file" : string.Empty)}{hint}");
        }
    }
}

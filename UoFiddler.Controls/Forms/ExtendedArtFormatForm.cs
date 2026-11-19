/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Creates / resizes / saves / loads the side-car extensions (see <see cref="ExtendedFormat"/>):
    /// items (1M+), land tiles, textures and hues. Radar colours follow the item / land tile extensions automatically.
    /// </summary>
    public sealed class ExtendedArtFormatForm : Form
    {
        private readonly Action _reloadItems; // kept for compatibility, the tabs listen to UoExtensions.Changed
        private readonly Action<int> _goTo;
        private readonly ComboBox _kind;
        private readonly Label _status;
        private readonly NumericUpDown _slots;
        private readonly Label _slotsInfo;
        private readonly CheckBox _autoBackup;
        private readonly Label _backupLabel;
        private readonly TextBox _info;

        public ExtendedArtFormatForm(Action reloadItems, Action<int> goTo = null, UoExtensionKind kind = UoExtensionKind.Items)
        {
            _reloadItems = reloadItems;
            _goTo = goTo;

            Text = "Extended formats (items, land tiles, textures, hues)";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(900, 720);
            MinimumSize = new Size(640, 520);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(8) };
            for (int i = 0; i < 5; ++i)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            // --- which extension
            var kindRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            kindRow.Controls.Add(new Label { Text = "Extension:", AutoSize = true, Margin = new Padding(3, 7, 3, 3), Font = new Font(Font, FontStyle.Bold) });
            _kind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
            foreach (UoExtensionKind k in Enum.GetValues(typeof(UoExtensionKind)))
            {
                _kind.Items.Add(UoExtensions.Title(k));
            }

            kindRow.Controls.Add(_kind);
            kindRow.Controls.Add(MakeButton("Save ALL extensions to output path", (s, e) => SaveAll()));
            kindRow.Controls.Add(MakeButton("Check what is saved...", (s, e) =>
                new ExtendedCheckForm(_goTo, () => { AfterReload(null); }).Show(this)));
            kindRow.Controls.Add(MakeButton("Load extensions from folder...", (s, e) => LoadFromFolder()));
            layout.Controls.Add(kindRow, 0, 0);

            _status = new Label { AutoSize = true, Font = new Font(FontFamily.GenericMonospace, 9f), Margin = new Padding(3, 6, 3, 8) };
            layout.Controls.Add(_status, 0, 1);

            // --- size of the selected extension
            var row = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            row.Controls.Add(new Label { Text = "Extended slots:", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            _slots = new NumericUpDown { Minimum = 1, Maximum = ExtendedFormat.MaxSlots, ThousandsSeparator = true, Width = 120 };
            _slots.ValueChanged += (s, e) => UpdateStatus();
            row.Controls.Add(_slots);
            row.Controls.Add(MakeButton("Max", (s, e) => _slots.Value = _slots.Maximum));
            row.Controls.Add(MakeButton("Create / resize", (s, e) => CreateOrResize()));
            row.Controls.Add(MakeButton("Save this extension to output path", (s, e) => SaveKind()));
            _slotsInfo = new Label { AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 7, 3, 3) };
            row.Controls.Add(_slotsInfo);
            layout.Controls.Add(row, 0, 2);

            var hint = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "Tip: 'Save' in the tabs (Items, LandTiles, Textures, Hues, TileData, RadarColor) writes the matching extension files automatically, too."
            };
            layout.Controls.Add(hint, 0, 3);

            // --- backup of the original client files + folder buttons
            var backupGroup = new GroupBox { Text = "Backup / folders", Dock = DockStyle.Fill, AutoSize = true };
            var backupLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            _autoBackup = new CheckBox
            {
                Text = "Automatic backup before create / save",
                AutoSize = true,
                Checked = true,
                Margin = new Padding(3, 7, 10, 3)
            };
            backupLayout.Controls.Add(_autoBackup);
            backupLayout.Controls.Add(MakeButton("Backup now", (s, e) => DoBackup(true)));
            backupLayout.Controls.Add(MakeButton("Open backup folder", (s, e) =>
                OpenFolder(UoFileBackup.LastBackupFolder ?? UoFileBackup.GetBackupRoot(Options.OutputPath))));
            backupLayout.Controls.Add(MakeButton("Open Ultima Online folder", (s, e) => OpenFolder(Files.RootDir)));
            backupLayout.Controls.Add(MakeButton("Open output folder", (s, e) => OpenFolder(Options.OutputPath)));
            _backupLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(3, 6, 3, 3),
                Text = $"Backups go to {UoFileBackup.GetBackupRoot(Options.OutputPath ?? string.Empty)}\\<date_time>\\ - your client files are only copied, never changed."
            };
            backupLayout.Controls.Add(_backupLabel);
            backupLayout.SetFlowBreak(backupLayout.Controls[backupLayout.Controls.Count - 2], true); // label on its own line
            backupGroup.Controls.Add(backupLayout);
            layout.Controls.Add(backupGroup, 0, 4);

            _info = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
            layout.Controls.Add(_info, 0, 5);

            _kind.SelectedIndexChanged += (s, e) => OnKindChanged();
            _kind.SelectedIndex = (int)kind;
        }

        private static ExtendedArtFormatForm _shared;

        /// <summary>Opens the (single) window with the given extension selected - used by every tab.</summary>
        public static void ShowFor(UoExtensionKind kind, Action<int> goTo = null)
        {
            if (_shared == null || _shared.IsDisposed)
            {
                _shared = new ExtendedArtFormatForm(null, goTo, kind);
                _shared.Show();
            }
            else
            {
                _shared.SelectKind(kind);
                if (_shared.WindowState == FormWindowState.Minimized)
                {
                    _shared.WindowState = FormWindowState.Normal;
                }

                _shared.Activate();
            }
        }

        /// <summary>Shows the given extension (e.g. when opened from the LandTiles tab).</summary>
        public void SelectKind(UoExtensionKind kind)
        {
            _kind.SelectedIndex = (int)kind;
        }

        private UoExtensionKind Kind => (UoExtensionKind)Math.Max(0, _kind.SelectedIndex);

        private void OnKindChanged()
        {
            UoExtensionKind kind = Kind;
            _slots.Maximum = UoExtensions.MaxSlots(kind);
            _slots.Increment = UoExtensions.Increment(kind);
            int current = UoExtensions.CurrentSlots(kind);
            _slots.Value = Math.Clamp(current > 0 ? current : UoExtensions.DefaultSlots(kind), 1, (int)_slots.Maximum);
            _info.Text = InfoText(kind);
            UpdateStatus();
        }

        private static Button MakeButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true };
            b.Click += click;
            return b;
        }

        private void OpenFolder(string path)
        {
            try
            {
                UoFileBackup.OpenFolder(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Can not open the folder:{Environment.NewLine}{path}{Environment.NewLine}{ex.Message}",
                    "Open folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>One automatic backup per session before the first create/save (if the checkbox is on).</summary>
        private bool AutoBackup()
        {
            if (!_autoBackup.Checked || UoFileBackup.LastBackupFolder != null)
            {
                return true;
            }

            return DoBackup(false);
        }

        /// <returns>false if the backup failed and the user does not want to continue.</returns>
        private bool DoBackup(bool showResult)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string folder = UoFileBackup.CreateBackup(Options.OutputPath, out var copied);
                _backupLabel.Text = $"Last backup: {folder} ({copied.Count} file(s))";
                if (showResult)
                {
                    MessageBox.Show(this,
                        $"Backup created:{Environment.NewLine}{folder}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, copied)}",
                        "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                return true;
            }
            catch (Exception ex)
            {
                return MessageBox.Show(this,
                    $"The backup failed:{Environment.NewLine}{ex.Message}{Environment.NewLine}{Environment.NewLine}Continue without backup?",
                    "Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>After a create/load: refresh the Items tab (old callback) and tell every tab.</summary>
        private void AfterReload(UoExtensionKind? kind)
        {
            // every tab (Items, LandTiles, Textures, Hues, TileData, RadarColor) listens to this event
            UoExtensions.RaiseChanged(kind);
            UpdateStatus();
        }

        /// <summary>
        /// Loads all side-car files from any folder (e.g. the output folder after saving). The chosen paths are stored
        /// in the path settings, so they are found again after a restart.
        /// </summary>
        private void LoadFromFolder()
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Folder with the extension files (artext*, artlandext*, tiledata*ext, texmapsext, huesext, radarcol*ext)",
                SelectedPath = Art.ExtendedDataPath != null ? Path.GetDirectoryName(Art.ExtendedDataPath) : Options.OutputPath
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string dir = dlg.SelectedPath;
            var found = new List<string>();
            foreach (string name in ExtendedFormat.AllFileNames)
            {
                if (File.Exists(Path.Combine(dir, name)))
                {
                    found.Add(name);
                }
            }

            if (found.Count == 0)
            {
                MessageBox.Show(this, $"No extension files found in{Environment.NewLine}{dir}", "Load extensions",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(this, "Unsaved changes in the extended ranges (items, land tiles, textures, hues, radar colours) will be discarded. Continue?",
                    "Load extensions", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (string name in found)
            {
                Files.SetMulPath(Path.Combine(dir, name), name);
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                UoExtensions.ReloadAll();
                AfterReload(null);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            OnKindChanged();
            MessageBox.Show(this,
                $"Loaded from{Environment.NewLine}{dir}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, found)}" +
                $"{Environment.NewLine}{Environment.NewLine}{UoExtensions.MemorySummary()}{Environment.NewLine}The paths are remembered in the path settings.",
                "Load extensions", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateStatus()
        {
            UoExtensionKind kind = Kind;
            int slots = (int)_slots.Value;
            int b = UoExtensions.BaseId(kind);
            int current = UoExtensions.CurrentSlots(kind);

            string classic = kind switch
            {
                UoExtensionKind.Items => $"art.mul / tiledata.mul: ids 0x0000 - 0x{Art.GetMaxItemId():X4} ({(Art.GetIdxLength() >= 0x13FDC ? "High Seas" : Art.GetIdxLength() == 0xC000 ? "Stygian Abyss" : "ML or older")})",
                UoExtensionKind.LandTiles => "art.mul / tiledata.mul: land ids 0x0000 - 0x3FFF",
                UoExtensionKind.Textures => $"texmaps.mul: ids 0x0000 - 0x3FFF ({Textures.GetIdxLength():N0} index entries)",
                _ => "hues.mul: hue numbers 1 - 3000 (375 blocks of 8)"
            };

            string loaded = kind switch
            {
                UoExtensionKind.Items => Art.ExtendedDataPath,
                UoExtensionKind.LandTiles => Art.LandExtension.DataPath,
                UoExtensionKind.Textures => Textures.Extension.DataPath,
                _ => Hues.ExtensionPath
            };

            string state = current > 0
                ? (loaded != null ? $"loaded ({loaded})" : "created in memory - not saved yet")
                : $"none - searched in the Ultima Online folder: {Files.RootDir}";

            _status.Text =
                $"Classic        : {classic}" + Environment.NewLine +
                $"Extension      : {state}" + Environment.NewLine +
                $"Slots now      : {current:N0}   used: {(current > 0 ? UoExtensions.CountUsed(kind) : 0):N0}   " +
                (current > 0 ? UoExtensions.RangeText(kind, b, b + current - 1) : "-") + Environment.NewLine +
                $"With {slots:N0} slots: {UoExtensions.RangeText(kind, b, b + slots - 1)}" + Environment.NewLine +
                $"Files          : {string.Join(", ", UoExtensions.FileNames(kind))}";

            _slotsInfo.Text = $"max {UoExtensions.MaxSlots(kind):N0}";
        }

        private void CreateOrResize()
        {
            UoExtensionKind kind = Kind;
            int slots = (int)_slots.Value;
            int current = UoExtensions.CurrentSlots(kind);
            if (current > 0 && slots < current)
            {
                int lost = UoExtensions.CountUsedAbove(kind, slots);
                if (lost > 0 && MessageBox.Show(this,
                        $"Shrinking removes {lost:N0} used entr(y/ies) above {UoExtensions.RangeText(kind, UoExtensions.BaseId(kind) + slots, UoExtensions.BaseId(kind) + current - 1)}. Continue?",
                        "Resize", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (!AutoBackup())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                UoExtensions.CreateOrResize(kind, slots);
                AfterReload(kind);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error: {ex.Message}", "Extended format", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            int b = UoExtensions.BaseId(kind);
            MessageBox.Show(this,
                $"{UoExtensions.Title(kind)}: extended range ready, {UoExtensions.RangeText(kind, b, b + slots - 1)}." + Environment.NewLine +
                NextStepText(kind),
                "Extended format", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string NextStepText(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => "Insert items there (Items tab: Insert at / Replace / Directions / sprite sheets) and save Art and TileData.",
            UoExtensionKind.LandTiles => "Insert land tiles there (LandTiles tab: Insert at / Replace) and save Art and TileData. Radar colours: RadarColor tab.",
            UoExtensionKind.Textures => "Insert textures there (Textures tab: Insert at / Replace) and save the Textures. Land tiles point to them with the texture id in the TileData tab.",
            _ => "Edit the new hues in the Hues tab and save the Hues. They are available in every hue picker (items, animations, gumps ...)."
        };

        private void SaveKind()
        {
            UoExtensionKind kind = Kind;
            if (!UoExtensions.Exists(kind))
            {
                MessageBox.Show(this, "This extension does not exist yet - create it first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Save(new[] { kind });
        }

        private void SaveAll()
        {
            var kinds = new List<UoExtensionKind>();
            foreach (UoExtensionKind k in Enum.GetValues(typeof(UoExtensionKind)))
            {
                if (UoExtensions.Exists(k))
                {
                    kinds.Add(k);
                }
            }

            if (kinds.Count == 0)
            {
                MessageBox.Show(this, "There is no extension yet - create one first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Save(kinds);
        }

        private void Save(IReadOnlyList<UoExtensionKind> kinds)
        {
            if (!AutoBackup())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var written = new List<string>();
                foreach (UoExtensionKind kind in kinds)
                {
                    UoExtensions.Save(kind, Options.OutputPath);
                    foreach (string name in UoExtensions.FileNames(kind))
                    {
                        if (File.Exists(Path.Combine(Options.OutputPath, name)))
                        {
                            written.Add(name);
                        }
                    }
                }

                // the "unsaved changes" hint stays only if the classic files really have unsaved changes
                if (!Art.LegacyModified)
                {
                    Options.ChangedUltimaClass["Art"] = false;
                }

                if (TileData.IsLegacyUnchanged())
                {
                    Options.ChangedUltimaClass["TileData"] = false;
                }

                string checkText = string.Empty;
                bool ok = true;
                if (kinds.Contains(UoExtensionKind.Items))
                {
                    ok = ExtendedCheck.VerifySaved(Options.OutputPath, out checkText);
                }

                MessageBox.Show(this,
                    $"Saved to {Options.OutputPath}:{Environment.NewLine}{string.Join(Environment.NewLine, written)}" +
                    Environment.NewLine + Environment.NewLine + checkText + Environment.NewLine + Environment.NewLine +
                    UoExtensions.DiskSummary(Options.OutputPath),
                    "Save", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error: {ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateStatus();
        }

        private static string InfoText(UoExtensionKind kind)
        {
            string nl = Environment.NewLine;
            string common =
                nl + nl + "BACKUP / SAFETY" + nl +
                "Your client files are never changed by creating an extension. Saving writes into the output folder; " +
                "use the check window to copy the side-car files into the Ultima Online folder (with a dated backup)." + nl +
                "All side-car files: 32-byte header (magic, version, base id, count, record size) - see Docs/ExtendedFormat_Client_Spezifikation.md.";

            switch (kind)
            {
                case UoExtensionKind.Items:
                    return "ITEMS (art + tiledata + radar colours)" + nl +
                           "art.mul / artidx.mul / tiledata.mul (or artLegacyMUL.uop) keep the exact client layout for the ids 0x0000-0xFFFF." + nl +
                           "All ids from 0x10000 upwards are stored in:" + nl +
                           $"  {ExtendedFormat.ArtIndexFileName}   index, 64-bit offsets (no 2 GB limit)" + nl +
                           $"  {ExtendedFormat.ArtDataFileName}      images, same encoding as art.mul" + nl +
                           $"  {ExtendedFormat.TileDataFileName} tiledata records (High Seas layout, 41 bytes)" + nl +
                           $"  {ExtendedFormat.RadarItemFileName} radar colours of these items (follows the size automatically)" + nl + nl +
                           "Up to 16,777,216 slots. Maps, statics and multis store 16-bit ids - extended items can not be placed there " +
                           "until a client/server supports it." + common;
                case UoExtensionKind.LandTiles:
                    return "LAND TILES (art + tiledata + radar colours)" + nl +
                           "art.mul keeps its 0x4000 land tiles (0x0000-0x3FFF) and tiledata.mul its 512 land groups - unchanged for the client." + nl +
                           "Land ids from 0x4000 upwards are stored in:" + nl +
                           $"  {ExtendedFormat.LandIndexFileName} index (16-byte entries)" + nl +
                           $"  {ExtendedFormat.LandDataFileName}     44x44 land tiles, same encoding as art.mul" + nl +
                           $"  {ExtendedFormat.LandTileDataFileName} land tiledata (32 bytes: uint64 flags, int32 texture id, char[20] name)" + nl +
                           $"  {ExtendedFormat.RadarLandFileName} radar colours of these land tiles" + nl + nl +
                           "LIMIT: map*.mul stores the land id as 16 bit, so land tiles end at 0xFFFF (49,152 extra slots). " +
                           "A client must read the side-car files to show land ids 0x4000+." + common;
                case UoExtensionKind.Textures:
                    return "TEXTURES" + nl +
                           "texidx.mul / texmaps.mul keep the ids 0x0000-0x3FFF unchanged." + nl +
                           "Texture ids from 0x4000 upwards are stored in:" + nl +
                           $"  {ExtendedFormat.TextureIndexFileName}   index (16-byte entries, extra 0 = 64x64, 1 = 128x128)" + nl +
                           $"  {ExtendedFormat.TextureDataFileName}  textures, same encoding as texmaps.mul" + nl + nl +
                           "LIMIT: the texture id in the land tiledata is 16 bit, so textures end at 0xFFFF (49,152 extra slots)." + common;
                default:
                    return "HUES" + nl +
                           "hues.mul keeps its 375 blocks = hue numbers 1-3000 unchanged." + nl +
                           "Hue numbers from 3001 upwards are stored in:" + nl +
                           $"  {ExtendedFormat.HueFileName}  88-byte records (32 colours, table start/end, char[20] name), no block headers" + nl + nl +
                           "LIMIT: the client uses (hue & 0x3FFF), so hue numbers end at 0x3FFF = 16,383 (13,383 extra hues). " +
                           "The new hues appear in every hue picker of the tool (items, land, animations, gumps, dress ...). " +
                           "A client must read huesext.mul to show them." + common;
            }
        }
    }
}

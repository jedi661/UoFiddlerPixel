/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Creates / resizes / saves the extended side-car storage (artextidx.mul, artext.mul, tiledataext.mul)
    /// for item ids from 0x10000 up to more than 1,000,000 - see <see cref="ExtendedFormat"/>.
    /// </summary>
    public sealed class ExtendedArtFormatForm : Form
    {
        private readonly Action _reloadItems;
        private readonly Label _status;
        private readonly NumericUpDown _slots;
        private readonly CheckBox _autoBackup;
        private readonly Label _backupLabel;

        public ExtendedArtFormatForm(Action reloadItems)
        {
            _reloadItems = reloadItems;

            Text = "Extended Art Format (1M+ items)";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(820, 640);
            MinimumSize = new Size(600, 480);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            _status = new Label { AutoSize = true, Font = new Font(FontFamily.GenericMonospace, 9f), Margin = new Padding(3, 3, 3, 10) };
            layout.Controls.Add(_status, 0, 0);

            var row = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            row.Controls.Add(new Label { Text = "Extended slots:", AutoSize = true, Margin = new Padding(3, 7, 3, 3) });
            _slots = new NumericUpDown
            {
                Minimum = 1,
                Maximum = ExtendedFormat.MaxSlots,
                Value = Art.HasExtension ? Art.ExtendedCount : ExtendedFormat.DefaultSlots,
                Increment = 0x10000,
                ThousandsSeparator = true,
                Width = 120
            };
            _slots.ValueChanged += (s, e) => UpdateStatus();
            row.Controls.Add(_slots);

            var create = new Button { Text = "Create / resize", AutoSize = true };
            create.Click += (s, e) => CreateOrResize();
            row.Controls.Add(create);

            var save = new Button { Text = "Save extension files to output path", AutoSize = true };
            save.Click += (s, e) => SaveExtension();
            row.Controls.Add(save);

            var load = new Button { Text = "Load extension from folder...", AutoSize = true };
            load.Click += (s, e) => LoadFromFolder();
            row.Controls.Add(load);
            layout.Controls.Add(row, 0, 1);

            var hint = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "Tip: 'Save' in the Items tab (and in the TileData tab) writes the extension files automatically, too."
            };
            layout.Controls.Add(hint, 0, 2);

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
            layout.Controls.Add(backupGroup, 0, 3);

            var info = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Text =
                    "HOW IT WORKS" + Environment.NewLine +
                    "art.mul / artidx.mul / tiledata.mul (or artLegacyMUL.uop) keep the exact client layout for the ids 0x0000-0xFFFF." + Environment.NewLine +
                    "Your current Ultima Online client keeps working with them - nothing about them changes." + Environment.NewLine + Environment.NewLine +
                    "All ids from 0x10000 upwards are stored in three additional files next to them:" + Environment.NewLine +
                    $"  {ExtendedFormat.ArtIndexFileName}   index, 64-bit offsets (no 2 GB limit)" + Environment.NewLine +
                    $"  {ExtendedFormat.ArtDataFileName}      images, same encoding as art.mul" + Environment.NewLine +
                    $"  {ExtendedFormat.TileDataFileName} tiledata records (High Seas layout, 41 bytes)" + Environment.NewLine + Environment.NewLine +
                    "Why not one bigger art.mul? artidx.mul uses 32-bit offsets (2 GB), the art/tiledata version is detected from the file size, " +
                    "and the client protocol only has 16-bit item ids. A future client only has to read the extra files." + Environment.NewLine + Environment.NewLine +
                    "Your existing files are loaded unchanged; the extension is loaded automatically when the files are in the client directory " +
                    "(or set their path in the path settings). Items above 0xFFFF are shown in the Items tab after the normal ones and " +
                    "in the TileData tab in the group 'Extended (0x10000+)'." + Environment.NewLine + Environment.NewLine +
                    "Note: maps, statics and multis store 16-bit ids - extended items can not be placed there until a client/server supports it."
            };
            layout.Controls.Add(info, 0, 4);

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

        /// <summary>
        /// Loads artextidx.mul / artext.mul / tiledataext.mul from any folder (e.g. the output folder after saving).
        /// The chosen paths are stored in the path settings, so they are found again after a restart.
        /// </summary>
        private void LoadFromFolder()
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Folder with artextidx.mul / artext.mul / tiledataext.mul",
                SelectedPath = Art.ExtendedDataPath != null ? System.IO.Path.GetDirectoryName(Art.ExtendedDataPath) : Options.OutputPath
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string dir = dlg.SelectedPath;
            string[] names = { ExtendedFormat.ArtIndexFileName, ExtendedFormat.ArtDataFileName, ExtendedFormat.TileDataFileName };
            var found = new System.Collections.Generic.List<string>();
            foreach (string name in names)
            {
                if (System.IO.File.Exists(System.IO.Path.Combine(dir, name)))
                {
                    found.Add(name);
                }
            }

            bool artOk = found.Contains(ExtendedFormat.ArtIndexFileName) && found.Contains(ExtendedFormat.ArtDataFileName);
            if (!artOk && !found.Contains(ExtendedFormat.TileDataFileName))
            {
                MessageBox.Show(this, $"No extended files found in{Environment.NewLine}{dir}", "Load extension",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Art.HasExtension && Options.ChangedUltimaClass.TryGetValue("Art", out bool changed) && changed &&
                MessageBox.Show(this, "Unsaved changes in the extended range will be discarded. Continue?", "Load extension",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            foreach (string name in found)
            {
                Files.SetMulPath(System.IO.Path.Combine(dir, name), name);
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Art.ReloadExtension();
                TileData.ReloadExtension();
                _reloadItems?.Invoke();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            if (Art.HasExtension)
            {
                _slots.Value = Math.Clamp(Art.ExtendedCount, (int)_slots.Minimum, (int)_slots.Maximum);
            }

            UpdateStatus();
            MessageBox.Show(this,
                Art.HasExtension || TileData.HasExtension
                    ? $"Extension loaded from{Environment.NewLine}{dir}{Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, found)}{Environment.NewLine}{Environment.NewLine}The path is remembered in the path settings."
                    : "The files were found but could not be read (damaged or unknown format).",
                "Load extension", MessageBoxButtons.OK, Art.HasExtension || TileData.HasExtension ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void UpdateStatus()
        {
            int legacyMax = Art.GetMaxItemId();
            string legacyName = Art.GetIdxLength() >= 0x13FDC ? "High Seas" : Art.GetIdxLength() == 0xC000 ? "Stygian Abyss" : "ML or older";
            string extState = Art.HasExtension
                ? (Art.ExtendedDataPath != null ? $"loaded ({Art.ExtendedDataPath})" : "created in memory - not saved yet")
                : $"none - searched in the Ultima Online folder: {Files.RootDir}";
            int slots = (int)_slots.Value;

            _status.Text =
                $"Legacy art     : {legacyName}, ids 0x0000 - 0x{legacyMax:X4} ({legacyMax + 1:N0})" + Environment.NewLine +
                $"Extension      : {extState}" + Environment.NewLine +
                $"Ext. slots     : {Art.ExtendedCount:N0}  used: {(Art.HasExtension ? Art.CountUsedExtended() : 0):N0}" + Environment.NewLine +
                $"Ext. tiledata  : {TileData.ExtendedCount:N0} slots" + Environment.NewLine +
                $"Highest id now : 0x{Art.GetMaxExtendedItemId():X} ({Art.GetMaxExtendedItemId() + 1:N0} items)" + Environment.NewLine +
                $"With {slots:N0} slots: ids up to 0x{ExtendedFormat.GetMaxId(slots):X} ({ExtendedFormat.GetMaxId(slots) + 1:N0} items)";
        }

        private void CreateOrResize()
        {
            int slots = (int)_slots.Value;
            if (Art.HasExtension && slots < Art.ExtendedCount)
            {
                int lost = 0;
                for (int id = ExtendedFormat.BaseId + slots; id <= Art.GetMaxExtendedItemId(); ++id)
                {
                    if (Art.IsValidStatic(id))
                    {
                        ++lost;
                    }
                }

                if (lost > 0 && MessageBox.Show(this,
                        $"Shrinking removes {lost:N0} used extended item(s) above 0x{ExtendedFormat.GetMaxId(slots):X}. Continue?",
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
                Art.CreateOrResizeExtension(slots);
                TileData.EnsureExtendedCapacity(slots);
                Options.ChangedUltimaClass["Art"] = true;
                Options.ChangedUltimaClass["TileData"] = true;
                _reloadItems?.Invoke();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            UpdateStatus();
            MessageBox.Show(this,
                $"Extended range ready: 0x{ExtendedFormat.BaseId:X} - 0x{Art.GetMaxExtendedItemId():X}." + Environment.NewLine +
                "Insert items there (Insert at / Replace / Directions) and save the Art and the TileData.",
                "Extended format", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveExtension()
        {
            if (!Art.HasExtension && !TileData.HasExtension)
            {
                MessageBox.Show(this, "There is no extension yet - create it first.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!AutoBackup())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Art.SaveExtension(Options.OutputPath);
                TileData.SaveExtension(Options.OutputPath);

                // the extension is saved now: the "unsaved changes" hint stays only if the classic files
                // (art.mul / tiledata.mul) really have unsaved changes
                if (!Art.LegacyModified)
                {
                    Options.ChangedUltimaClass["Art"] = false;
                }

                if (TileData.IsLegacyUnchanged())
                {
                    Options.ChangedUltimaClass["TileData"] = false;
                }
                MessageBox.Show(this,
                    $"Saved {ExtendedFormat.ArtIndexFileName}, {ExtendedFormat.ArtDataFileName} and {ExtendedFormat.TileDataFileName} to {Options.OutputPath}",
                    "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}

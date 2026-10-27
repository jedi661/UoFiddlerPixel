/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Lists the backups created when map files were saved and restores them
    /// </summary>
    public partial class MapBackupForm : Form
    {
        /// <summary>Only for the designer</summary>
        public MapBackupForm()
        {
            InitializeComponent();
        }

        public MapBackupForm(string mapName, IEnumerable<string> directories)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            Text = $"Map backups - {mapName}";

            foreach (string dir in directories.Where(d => !string.IsNullOrWhiteSpace(d)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                comboBoxDirectory.Items.Add(dir);
            }

            // first directory that has backups
            int index = 0;
            for (int i = 0; i < comboBoxDirectory.Items.Count; i++)
            {
                if (MapSaveService.ListBackups((string)comboBoxDirectory.Items[i]).Count > 0)
                {
                    index = i;
                    break;
                }
            }

            if (comboBoxDirectory.Items.Count > 0)
            {
                comboBoxDirectory.SelectedIndex = index;
            }
        }

        /// <summary>true if a backup was restored</summary>
        public bool Restored { get; private set; }

        private MapBackupInfo Selected => listViewBackups.SelectedItems.Count == 0 ? null : (MapBackupInfo)listViewBackups.SelectedItems[0].Tag;

        private void OnDirectoryChanged(object sender, EventArgs e)
        {
            FillList();
        }

        private void FillList()
        {
            listViewBackups.BeginUpdate();
            try
            {
                listViewBackups.Items.Clear();
                foreach (MapBackupInfo backup in MapSaveService.ListBackups(comboBoxDirectory.Text))
                {
                    var item = new ListViewItem(backup.Created.ToString("yyyy-MM-dd HH:mm:ss")) { Tag = backup };
                    item.SubItems.Add($"{backup.MapName} (map{backup.FileIndex})");
                    item.SubItems.Add(string.Join(", ", backup.Files));
                    item.SubItems.Add(backup.Comment);
                    listViewBackups.Items.Add(item);
                }
            }
            finally
            {
                listViewBackups.EndUpdate();
            }

            OnSelectionChanged(this, EventArgs.Empty);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            buttonRestore.Enabled = buttonDelete.Enabled = buttonOpenFolder.Enabled = Selected != null;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { Description = "Directory with the map files", SelectedPath = comboBoxDirectory.Text })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    comboBoxDirectory.Text = dialog.SelectedPath;
                    FillList();
                }
            }
        }

        private void OnClickRestore(object sender, EventArgs e)
        {
            MapBackupInfo backup = Selected;
            if (backup == null)
            {
                return;
            }

            if (MessageBox.Show(
                    $"Restore the backup from {backup.Created:yyyy-MM-dd HH:mm:ss}?\n\n{string.Join("\n", backup.Files)}\n\ninto {backup.TargetDirectory}\n\n" +
                    "Unsaved changes of this map in memory are lost. Close the client and the server first.",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                MapSaveService.Restore(backup, checkBoxBackupCurrent.Checked);
                Restored = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Restoring failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            MessageBox.Show("The backup was restored.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            FillList();
        }

        private void OnClickDelete(object sender, EventArgs e)
        {
            MapBackupInfo backup = Selected;
            if (backup == null ||
                MessageBox.Show($"Delete the backup from {backup.Created:yyyy-MM-dd HH:mm:ss}?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Directory.Delete(backup.Directory, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            FillList();
        }

        private void OnClickOpenFolder(object sender, EventArgs e)
        {
            MapBackupInfo backup = Selected;
            if (backup != null && Directory.Exists(backup.Directory))
            {
                Process.Start(new ProcessStartInfo { FileName = backup.Directory, UseShellExecute = true });
            }
        }
    }
}

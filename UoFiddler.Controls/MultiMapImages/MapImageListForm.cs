/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>Manages the images of the "Load.." list: show, rename, remove, open folder</summary>
    public class MapImageListForm : Form
    {
        private readonly ListView _list = new ListView();
        private readonly PictureBox _preview = new PictureBox();

        public MapImageListForm()
        {
            Text = "MultiMap image list";
            Icon = Options.GetFiddlerIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(860, 460);
            MinimumSize = new Size(600, 300);
            ShowInTaskbar = false;

            _list.View = View.Details;
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.MultiSelect = false;
            _list.Columns.Add("Name", 200);
            _list.Columns.Add("Stored", 80);
            _list.Columns.Add("Size", 90);
            _list.Columns.Add("File", 360);
            _list.Dock = DockStyle.Fill;
            _list.SelectedIndexChanged += (s, e) => UpdatePreview();
            _list.DoubleClick += (s, e) => OnShow();

            _preview.Dock = DockStyle.Right;
            _preview.Width = 240;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.BackColor = Color.White;
            _preview.BorderStyle = BorderStyle.FixedSingle;

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 40, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6) };
            buttons.Controls.Add(MakeButton("Show", (s, e) => OnShow()));
            buttons.Controls.Add(MakeButton("Rename...", (s, e) => OnRename()));
            buttons.Controls.Add(MakeButton("Remove...", (s, e) => OnRemove()));
            buttons.Controls.Add(MakeButton("Open folder", (s, e) => OnOpenFolder()));
            buttons.Controls.Add(MakeButton("Image folder of the app", (s, e) => OpenExplorer(MapImageLibrary.Folder, null), 170));
            var close = MakeButton("Close", (s, e) => Close());
            buttons.Controls.Add(close);
            CancelButton = close;

            Controls.Add(_list);
            Controls.Add(_preview);
            Controls.Add(buttons);

            FillList();
        }

        /// <summary>Entry the user wants to see in the MultiMap tab</summary>
        public MapImageEntry EntryToShow { get; private set; }

        private static Button MakeButton(string text, EventHandler click, int width = 100)
        {
            var b = new Button { Text = text, Width = width, Height = 27 };
            b.Click += click;
            return b;
        }

        private MapImageEntry Selected => _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as MapImageEntry : null;

        private void FillList()
        {
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (MapImageEntry entry in MapImageLibrary.Entries)
            {
                string size = string.Empty;
                if (entry.Exists)
                {
                    size = $"{new FileInfo(entry.FilePath).Length / 1024.0:0} KB";
                }

                var item = new ListViewItem(new[]
                {
                    entry.Name, entry.IsInternal ? "in app" : "external", entry.Exists ? size : "missing", entry.FilePath
                })
                { Tag = entry };
                if (!entry.Exists)
                {
                    item.ForeColor = Color.Gray;
                }

                _list.Items.Add(item);
            }

            _list.EndUpdate();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();

            MapImageEntry entry = Selected;
            if (entry == null || !entry.Exists)
            {
                return;
            }

            try
            {
                // small thumbnail, so big maps do not stay in memory
                using (Bitmap full = MapImageLibrary.LoadBitmap(entry.FilePath))
                {
                    float f = Math.Min(1f, Math.Min(480f / full.Width, 480f / full.Height));
                    var thumb = new Bitmap(Math.Max(1, (int)(full.Width * f)), Math.Max(1, (int)(full.Height * f)));
                    using (Graphics g = Graphics.FromImage(thumb))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                        g.DrawImage(full, 0, 0, thumb.Width, thumb.Height);
                    }

                    _preview.Image = thumb;
                }
            }
            catch
            {
                // no preview
            }
        }

        private void OnShow()
        {
            MapImageEntry entry = Selected;
            if (entry == null || !entry.Exists)
            {
                return;
            }

            EntryToShow = entry;
            Close();
        }

        private void OnRename()
        {
            MapImageEntry entry = Selected;
            if (entry == null)
            {
                return;
            }

            using (var form = new Form())
            {
                form.Text = "Rename";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new Size(360, 80);
                var text = new TextBox { Left = 12, Top = 12, Width = 336, Text = entry.Name };
                var ok = new Button { Text = "OK", Left = 188, Top = 44, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 273, Top = 44, Width = 75, DialogResult = DialogResult.Cancel };
                form.Controls.AddRange(new Control[] { text, ok, cancel });
                form.AcceptButton = ok;
                form.CancelButton = cancel;
                if (form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(text.Text))
                {
                    MapImageLibrary.Rename(entry, text.Text);
                    FillList();
                }
            }
        }

        private void OnRemove()
        {
            MapImageEntry entry = Selected;
            if (entry == null)
            {
                return;
            }

            if (entry.IsInternal)
            {
                DialogResult r = MessageBox.Show(this,
                    $"Remove \"{entry.Name}\" from the list?\n\nYes = remove and delete the file\nNo = remove only from the list (the file stays in the folder\n      and comes back at the next start)",
                    Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel)
                {
                    return;
                }

                ReleasePreview();
                MapImageLibrary.Remove(entry, r == DialogResult.Yes);
            }
            else
            {
                if (MessageBox.Show(this, $"Remove \"{entry.Name}\" from the list?\n(The file is not deleted.)", Text,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                MapImageLibrary.Remove(entry, false);
            }

            FillList();
        }

        private void ReleasePreview()
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();
        }

        private void OnOpenFolder()
        {
            MapImageEntry entry = Selected;
            if (entry == null)
            {
                OpenExplorer(MapImageLibrary.Folder, null);
                return;
            }

            OpenExplorer(Path.GetDirectoryName(entry.FilePath), entry.Exists ? entry.FilePath : null);
        }

        private static void OpenExplorer(string folder, string file)
        {
            try
            {
                if (file != null)
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{file}\"") { UseShellExecute = true });
                }
                else if (Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleasePreview();
            }

            base.Dispose(disposing);
        }
    }
}

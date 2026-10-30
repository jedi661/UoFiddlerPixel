/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using UoFiddler.Plugin.Enhanced.Classes;
using UoFiddler.Plugin.Enhanced.EcUltima;
using UoFiddler.Plugin.Enhanced.EcUltima.Sounds;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.UserControls
{
    /// <summary>
    /// Sounds and music of the Enhanced Client (Audio.uop with audio_sounds.csv / audio_music.csv).
    /// </summary>
    public sealed class EcSoundsControl : UserControl, IEcEditor
    {
        public const string FileName = "Audio.uop";

        private sealed class Row
        {
            public EcSound Sound;
            public EcUopEntry Unlisted;
        }

        private readonly ToolStripComboBox _kindBox;
        private readonly ToolStripTextBox _searchBox;
        private readonly ToolStripButton _saveButton;
        private readonly ListView _list;
        private readonly ToolStripStatusLabel _statusLabel;
        private readonly SoundPlayer _player = new SoundPlayer();

        private EcClient _client;
        private EcSounds _sounds;
        private bool _loaded;
        private List<Row> _view = new List<Row>();

        public EcSoundsControl()
        {
            Dock = DockStyle.Fill;

            var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _kindBox = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };
            _kindBox.Items.AddRange(new object[] { "Sounds", "Music", "Files in no list" });
            _kindBox.SelectedIndex = 0;
            _kindBox.SelectedIndexChanged += (s, e) => ApplyFilter();
            _searchBox = new ToolStripTextBox { Width = 160, ToolTipText = "Id or part of the path – Enter" };
            _searchBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; ApplyFilter(); } };
            _saveButton = new ToolStripButton("Save", null, (s, e) => Save(false)) { Enabled = false };

            toolStrip.Items.Add(_kindBox);
            toolStrip.Items.Add(new ToolStripLabel("Search:"));
            toolStrip.Items.Add(_searchBox);
            toolStrip.Items.Add(new ToolStripButton("Apply", null, (s, e) => ApplyFilter()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Play", null, (s, e) => Play()) { ToolTipText = "wav: here, mp3/ogg: with the Windows player" });
            toolStrip.Items.Add(new ToolStripButton("Stop", null, (s, e) => _player.Stop()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("Export...", null, (s, e) => Export()));
            toolStrip.Items.Add(new ToolStripButton("Replace...", null, (s, e) => Replace()));
            toolStrip.Items.Add(new ToolStripButton("Add...", null, (s, e) => Add()));
            toolStrip.Items.Add(new ToolStripButton("Change path...", null, (s, e) => ChangePath()));
            toolStrip.Items.Add(new ToolStripButton("Remove", null, (s, e) => Remove()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(_saveButton);
            toolStrip.Items.Add(new ToolStripButton("Save as...", null, (s, e) => Save(true)));

            _list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false, VirtualMode = true, MultiSelect = false };
            _list.Columns.Add("Id", 70, HorizontalAlignment.Right);
            _list.Columns.Add("File", 420);
            _list.Columns.Add("Size", 80, HorizontalAlignment.Right);
            _list.Columns.Add("More", 80);
            _list.RetrieveVirtualItem += RetrieveItem;
            _list.DoubleClick += (s, e) => Play();

            var status = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Choose the Enhanced Client folder on top.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            status.Items.Add(_statusLabel);

            Controls.Add(_list);
            Controls.Add(toolStrip);
            Controls.Add(status);
        }

        public void SetClient(EcClient client)
        {
            _player.Stop();
            _client = client;
            _loaded = false;
            _sounds = null;
            _view = new List<Row>();
            _list.VirtualListSize = 0;
            _saveButton.Enabled = false;
            if (Visible && client != null)
            {
                OnActivated();
            }
        }

        public void OnActivated()
        {
            if (_loaded || _client == null)
            {
                return;
            }
            _loaded = true;

            EcUopPackage package = _client.GetPackage(FileName);
            if (package == null)
            {
                _statusLabel.Text = $"{FileName} was not found.";
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                _sounds = new EcSounds(package);
                package.Changed += (s, e) => _saveButton.Enabled = package.IsDirty;
                ApplyFilter();
                _statusLabel.Text = $"{_sounds.Sounds.Count(s => !s.IsMusic)} sounds, {_sounds.Sounds.Count(s => s.IsMusic)} music titles, {_sounds.Unlisted.Count} files in no list.";
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "Audio.uop can't be read.", ex);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ApplyFilter()
        {
            if (_sounds == null)
            {
                return;
            }
            string filter = _searchBox.Text.Trim();
            IEnumerable<Row> rows = _kindBox.SelectedIndex switch
            {
                1 => _sounds.Sounds.Where(s => s.IsMusic).OrderBy(s => s.Id).Select(s => new Row { Sound = s }),
                2 => _sounds.Unlisted.OrderBy(e => e.Name ?? "~").Select(e => new Row { Unlisted = e }),
                _ => _sounds.Sounds.Where(s => !s.IsMusic).OrderBy(s => s.Id).Select(s => new Row { Sound = s })
            };

            if (filter.Length > 0)
            {
                bool number = int.TryParse(filter, out int id);
                rows = rows.Where(r => (r.Sound != null && ((number && r.Sound.Id == id) || r.Sound.Path.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)) ||
                                       (r.Unlisted != null && r.Unlisted.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0));
            }
            _view = rows.ToList();
            _list.VirtualListSize = _view.Count;
            _list.Invalidate();
        }

        private void RetrieveItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < 0 || e.ItemIndex >= _view.Count)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }
            Row row = _view[e.ItemIndex];
            ListViewItem item;
            if (row.Sound != null)
            {
                item = new ListViewItem(row.Sound.Id.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(row.Sound.Path);
                item.SubItems.Add(row.Sound.Entry == null ? "missing" : EcImageHelper.FormatSize(row.Sound.Entry.DecompressedSize));
                item.SubItems.Add(row.Sound.IsMusic ? $"loop {row.Sound.Extra}" : row.Sound.Extra ?? string.Empty);
                if (row.Sound.Entry == null)
                {
                    item.ForeColor = Color.DarkRed;
                }
                else if (row.Sound.Entry.IsModified)
                {
                    item.ForeColor = Color.DarkBlue;
                }
            }
            else
            {
                item = new ListViewItem("-");
                item.SubItems.Add(row.Unlisted.DisplayName);
                item.SubItems.Add(EcImageHelper.FormatSize(row.Unlisted.DecompressedSize));
                item.SubItems.Add(string.Empty);
            }
            e.Item = item;
        }

        private Row Selected => _list.SelectedIndices.Count == 1 && _list.SelectedIndices[0] < _view.Count ? _view[_list.SelectedIndices[0]] : null;

        private byte[] SelectedData(out string name)
        {
            Row row = Selected;
            name = null;
            if (row == null)
            {
                return null;
            }
            EcUopEntry entry = row.Sound?.Entry ?? row.Unlisted;
            name = row.Sound?.PackedName ?? row.Unlisted?.Name ?? $"{row.Unlisted?.Hash:X16}.mp3";
            return entry == null ? null : _sounds.Package.ReadData(entry);
        }

        private void Play()
        {
            byte[] data = SelectedData(out string name);
            if (data == null)
            {
                return;
            }
            try
            {
                if (data.Length > 4 && data[0] == 'R' && data[1] == 'I' && data[2] == 'F' && data[3] == 'F')
                {
                    _player.Stop();
                    _player.Stream = new MemoryStream(data);
                    _player.Play();
                }
                else
                {
                    string folder = Path.Combine(Path.GetTempPath(), "UoFiddlerEnhanced");
                    Directory.CreateDirectory(folder);
                    string file = Path.Combine(folder, Path.GetFileName(name));
                    File.WriteAllBytes(file, data);
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                EcDialogs.ShowError(this, "The sound can't be played.", ex);
            }
        }

        private void Export()
        {
            byte[] data = SelectedData(out string name);
            if (data == null)
            {
                return;
            }
            using (var dialog = new SaveFileDialog { FileName = Path.GetFileName(name), Filter = "Audio|*.mp3;*.wav;*.ogg|All files (*.*)|*.*" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllBytes(dialog.FileName, data);
                }
            }
        }

        private static string ChooseAudio(IWin32Window owner)
        {
            using (var dialog = new OpenFileDialog { Filter = "Audio (*.mp3;*.wav;*.ogg)|*.mp3;*.wav;*.ogg|All files (*.*)|*.*" })
            {
                return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.FileName : null;
            }
        }

        private void Replace()
        {
            Row row = Selected;
            if (row == null)
            {
                return;
            }
            string file = ChooseAudio(this);
            if (file == null)
            {
                return;
            }
            byte[] data = File.ReadAllBytes(file);
            if (row.Sound != null)
            {
                int shared = _sounds.Sounds.Count(s => s.Entry != null && s.Entry == row.Sound.Entry);
                if (shared > 1 && !EcDialogs.Confirm(this, $"{shared} ids use this file. Replace it for all of them?"))
                {
                    return;
                }
                _sounds.SetData(row.Sound, data);
            }
            else
            {
                _sounds.Package.SetData(row.Unlisted, data);
            }
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        private void Add()
        {
            if (_sounds == null)
            {
                return;
            }
            string file = ChooseAudio(this);
            if (file == null)
            {
                return;
            }
            bool music = _kindBox.SelectedIndex == 1;
            List<EcSound> list = _sounds.Sounds.Where(s => s.IsMusic == music).ToList();
            int next = list.Count == 0 ? 0 : list.Max(s => s.Id) + 1;
            string[] values = EcDialogs.Prompt(this, music ? "Add music" : "Add sound", new[] { "Id:", "Path (below data/audio/):" },
                new[] { next.ToString(CultureInfo.InvariantCulture), (music ? "Music/" : "Sounds/custom/") + Path.GetFileName(file) });
            if (values == null || !int.TryParse(values[0], out int id) || string.IsNullOrWhiteSpace(values[1]))
            {
                return;
            }
            _sounds.Add(id, music, values[1].Trim().Replace('\\', '/'), File.ReadAllBytes(file));
            _saveButton.Enabled = true;
            ApplyFilter();
        }

        private void ChangePath()
        {
            Row row = Selected;
            if (row?.Sound == null)
            {
                return;
            }
            string path = EcDialogs.Prompt(this, "Change path", "Path (below data/audio/):", row.Sound.Path);
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            _sounds.ChangePath(row.Sound, path.Trim().Replace('\\', '/'));
            _saveButton.Enabled = true;
            _list.Invalidate();
        }

        private void Remove()
        {
            Row row = Selected;
            if (row == null || !EcDialogs.Confirm(this, "Remove the selected entry?"))
            {
                return;
            }
            if (row.Sound != null)
            {
                _sounds.Remove(row.Sound);
            }
            else
            {
                _sounds.Package.Remove(row.Unlisted);
                _sounds.Unlisted.Remove(row.Unlisted);
            }
            _saveButton.Enabled = true;
            ApplyFilter();
        }

        private void Save(bool saveAs)
        {
            if (_sounds == null)
            {
                return;
            }
            _sounds.Apply();
            if (EcPackageSaver.Save(this, new[] { _sounds.Package }, saveAs))
            {
                _saveButton.Enabled = false;
                _statusLabel.Text = "Saved.";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _player.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

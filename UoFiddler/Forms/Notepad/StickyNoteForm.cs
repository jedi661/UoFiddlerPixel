/***************************************************************************
 *
 * $Author: Nikodemus
 * Coder: Nikodemus
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

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Small always-on-top note window (e.g. next to the game client). Changes are saved automatically.
    /// </summary>
    internal sealed class StickyNoteForm : Form
    {
        private readonly Note _note;
        private readonly RichTextBox _box = new() { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, DetectUrls = true };
        private readonly Timer _saveTimer = new() { Interval = 1000 };
        private readonly Action<Note> _openInNotepad;
        private bool _loading;

        private static readonly Color _paper = Color.FromArgb(255, 249, 196);

        public StickyNoteForm(Note note, Action<Note> openInNotepad)
        {
            _note = note;
            _openInNotepad = openInNotepad;
            Text = note.Title;
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            Size = new Size(300, 320);
            BackColor = _paper;
            Padding = new Padding(6);
            _box.BackColor = note.Color != 0 ? Blend(Color.FromArgb(note.Color)) : _paper;
            BackColor = _box.BackColor;

            Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 1024);
            Location = new Point(area.Right - Width - 20, area.Top + 40);

            var menu = new ContextMenuStrip();
            menu.Items.Add("Open in Notepad", null, (_, _) => _openInNotepad?.Invoke(_note));
            menu.Items.Add(new ToolStripSeparator());
            foreach (int opacity in new[] { 100, 85, 70, 55 })
            {
                int o = opacity;
                menu.Items.Add($"Opacity {o} %", null, (_, _) => Opacity = o / 100.0);
            }

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Zoom +", null, (_, _) => _box.ZoomFactor = Math.Min(4f, _box.ZoomFactor + 0.1f));
            menu.Items.Add("Zoom −", null, (_, _) => _box.ZoomFactor = Math.Max(0.5f, _box.ZoomFactor - 0.1f));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Close", null, (_, _) => Close());
            _box.ContextMenuStrip = menu;

            Controls.Add(_box);
            Reload();

            _box.TextChanged += (_, _) =>
            {
                if (!_loading)
                {
                    _saveTimer.Stop();
                    _saveTimer.Start();
                }
            };
            _box.LinkClicked += (_, e) => OpenLink(e.LinkText);
            _saveTimer.Tick += (_, _) => SaveNow();
            NoteStore.NoteSaved += OnNoteSaved;
            FormClosing += (_, _) => SaveNow();
            FormClosed += (_, _) =>
            {
                NoteStore.NoteSaved -= OnNoteSaved;
                _saveTimer.Dispose();
            };
        }

        private static Color Blend(Color c) => Color.FromArgb((c.R + 255 * 2) / 3, (c.G + 255 * 2) / 3, (c.B + 255 * 2) / 3);

        public string NoteId => _note.Id;

        private void Reload()
        {
            _loading = true;
            try
            {
                if (!string.IsNullOrEmpty(_note.Rtf))
                {
                    try
                    {
                        _box.Rtf = _note.Rtf;
                    }
                    catch (ArgumentException)
                    {
                        _box.Text = _note.PlainText;
                    }
                }
                else
                {
                    _box.Text = _note.PlainText ?? "";
                }

                Text = _note.Title;
            }
            finally
            {
                _loading = false;
            }
        }

        private void OnNoteSaved(Note note, object source)
        {
            if (source != this && note.Id == _note.Id && !_box.Focused && !IsDisposed)
            {
                Reload();
            }
        }

        private void SaveNow()
        {
            _saveTimer.Stop();
            if (_loading)
            {
                return;
            }

            string rtf = _box.Rtf;
            if (rtf == _note.Rtf)
            {
                return;
            }

            _note.Rtf = rtf;
            _note.PlainText = _box.Text;
            _note.Modified = DateTime.Now;
            try
            {
                NoteStore.Save();
                NoteStore.NotifySaved(_note, this);
            }
            catch (Exception ex)
            {
                Text = "Not saved: " + ex.Message;
            }
        }

        internal static void OpenLink(string link)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(link) { UseShellExecute = true });
            }
            catch
            {
                // ignored
            }
        }
    }
}

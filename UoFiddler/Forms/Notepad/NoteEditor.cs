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
    /// Editor for one note (one tab): rich text box with line numbers, dirty tracking and code mode.
    /// </summary>
    public class NoteEditor : UserControl
    {
        private readonly Timer _highlightTimer = new() { Interval = 700 };
        private bool _loading;
        private bool _dirty;

        public Note Note { get; }
        public NoteRichTextBox Box { get; } = new() { Dock = DockStyle.Fill };
        public LineNumberGutter Gutter { get; }

        public event EventHandler DirtyChanged;

        public NoteEditor(Note note, NotepadSettings settings)
        {
            Note = note;
            Gutter = new LineNumberGutter(Box) { Dock = DockStyle.Left };

            var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 6, 4, 4), BackColor = SystemColors.Window };
            host.Controls.Add(Box);
            Controls.Add(host);
            Controls.Add(Gutter);

            Box.Font = new Font(settings.DefaultFont, settings.DefaultSize);
            Box.WordWrap = settings.WordWrap;
            Gutter.Visible = settings.LineNumbers;

            Box.TextChanged += (_, _) =>
            {
                if (_loading)
                {
                    return;
                }

                Dirty = true;
                if (Note.CodeMode)
                {
                    _highlightTimer.Stop();
                    _highlightTimer.Start();
                }
            };
            _highlightTimer.Tick += (_, _) =>
            {
                _highlightTimer.Stop();
                Highlight();
            };

            LoadFromNote();
            HandleCreated += (_, _) => Box.ZoomFactor = Math.Clamp(settings.Zoom, 0.5f, 4f);
        }

        public bool Dirty
        {
            get => _dirty;
            set
            {
                if (_dirty != value)
                {
                    _dirty = value;
                    DirtyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void LoadFromNote()
        {
            _loading = true;
            try
            {
                if (!string.IsNullOrEmpty(Note.Rtf))
                {
                    try
                    {
                        Box.Rtf = Note.Rtf;
                    }
                    catch (ArgumentException)
                    {
                        Box.Text = Note.PlainText ?? "";
                    }
                }
                else
                {
                    Box.Text = Note.PlainText ?? "";
                }

                ApplyCodeMode(false);
                Box.ClearUndo();
                Box.Select(0, 0);
            }
            finally
            {
                _loading = false;
            }

            Dirty = false;
        }

        /// <summary>
        /// Copies the editor content into the note. Returns true if something changed.
        /// </summary>
        public bool SaveToNote()
        {
            if (!Dirty)
            {
                return false;
            }

            Note.Rtf = Box.Rtf;
            Note.PlainText = Box.Text;
            Note.Modified = DateTime.Now;
            Dirty = false;
            return true;
        }

        public void ApplyCodeMode(bool markDirty = true)
        {
            bool wasLoading = _loading;
            _loading = true;
            try
            {
                if (Note.CodeMode)
                {
                    int s = Box.SelectionStart, l = Box.SelectionLength;
                    Box.SelectAll();
                    RichEditNative.SetFontFace(Box, "Consolas");
                    RichEditNative.SetFontSize(Box, 10f);
                    Box.Select(s, l);
                    Box.WordWrap = false;
                    Box.SelectionFont = new Font("Consolas", 10f);
                    CSharpHighlighter.Highlight(Box);
                }
            }
            finally
            {
                _loading = wasLoading;
            }

            if (markDirty)
            {
                Dirty = true;
            }
        }

        public void Highlight()
        {
            if (!Note.CodeMode)
            {
                return;
            }

            bool wasLoading = _loading;
            _loading = true;
            try
            {
                CSharpHighlighter.Highlight(Box);
            }
            finally
            {
                _loading = wasLoading;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _highlightTimer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

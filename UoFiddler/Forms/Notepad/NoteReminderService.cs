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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace UoFiddler.Forms.Notepad
{
    /// <summary>
    /// Checks note reminders while UoFiddler runs and shows a popup when one is due.
    /// Reminders are also written into the calendar (Data/Calendar/CalendarDateNotes.xml).
    /// </summary>
    public static class NoteReminderService
    {
        private static Timer _timer;
        private static Action<string> _openNote;
        private static readonly HashSet<string> _showing = new();

        public static void Start(Action<string> openNote)
        {
            _openNote = openNote;
            if (_timer != null)
            {
                return;
            }

            _timer = new Timer { Interval = 30_000 };
            _timer.Tick += (_, _) => Check();
            _timer.Start();
            Check();
        }

        public static void Check()
        {
            List<Note> due;
            try
            {
                due = NoteStore.Data.Notes
                    .Where(n => n.ReminderAt.HasValue && !n.ReminderDone && n.ReminderAt.Value <= DateTime.Now && !_showing.Contains(n.Id))
                    .ToList();
            }
            catch
            {
                return;
            }

            foreach (Note note in due)
            {
                _showing.Add(note.Id);
                var popup = new ReminderPopup(note, _openNote);
                popup.FormClosed += (_, _) => _showing.Remove(note.Id);
                popup.Show();
            }
        }

        public static string CalendarFile =>
            Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory,
                "Data", "Calendar", "CalendarDateNotes.xml");

        /// <summary>
        /// Adds the reminder to the calendar day so it shows up in the CalendarForm.
        /// </summary>
        public static void WriteToCalendar(Note note)
        {
            if (!note.ReminderAt.HasValue)
            {
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CalendarFile)!);
                XDocument doc = File.Exists(CalendarFile) ? XDocument.Load(CalendarFile) : new XDocument(new XElement("notes"));
                string date = note.ReminderAt.Value.ToShortDateString();
                string line = $"⏰ {note.ReminderAt.Value:t}  Note: {note.Title}";

                XElement day = doc.Root!.Elements("note").FirstOrDefault(n => (string)n.Element("date") == date);
                if (day == null)
                {
                    doc.Root.Add(new XElement("note", new XElement("date", date), new XElement("text", line)));
                }
                else
                {
                    XElement text = day.Element("text") ?? new XElement("text", "");
                    if (day.Element("text") == null)
                    {
                        day.Add(text);
                    }

                    if (!text.Value.Contains(line))
                    {
                        text.Value = string.IsNullOrEmpty(text.Value) ? line : text.Value + Environment.NewLine + line;
                    }
                }

                doc.Save(CalendarFile);
            }
            catch
            {
                // calendar is optional
            }
        }
    }

    internal sealed class ReminderPopup : Form
    {
        public ReminderPopup(Note note, Action<string> openNote)
        {
            Text = "Reminder";
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = true;
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(340, 150);
            BackColor = Color.FromArgb(255, 249, 196);

            Rectangle area = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 1024);
            Location = new Point(area.Right - Width - 12, area.Bottom - Height - 12);

            Controls.Add(new Label
            {
                Text = "⏰ " + note.Title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoEllipsis = true,
                Bounds = new Rectangle(12, 10, 316, 24)
            });
            Controls.Add(new Label
            {
                Text = $"{note.ReminderAt:g}\n{note.Snippet(120)}",
                ForeColor = Color.DimGray,
                Bounds = new Rectangle(12, 38, 316, 60)
            });

            var open = new Button { Text = "Open", Bounds = new Rectangle(12, 108, 90, 30) };
            var snooze = new Button { Text = "In 10 min", Bounds = new Rectangle(108, 108, 100, 30) };
            var done = new Button { Text = "Done", Bounds = new Rectangle(238, 108, 90, 30) };
            open.Click += (_, _) =>
            {
                MarkDone(note);
                openNote?.Invoke(note.Id);
                Close();
            };
            snooze.Click += (_, _) =>
            {
                note.ReminderAt = DateTime.Now.AddMinutes(10);
                SaveQuietly();
                Close();
            };
            done.Click += (_, _) =>
            {
                MarkDone(note);
                Close();
            };
            Controls.AddRange(new Control[] { open, snooze, done });
            System.Media.SystemSounds.Asterisk.Play();
        }

        private static void MarkDone(Note note)
        {
            note.ReminderDone = true;
            SaveQuietly();
            NoteStore.NotifySaved(note, null);
        }

        private static void SaveQuietly()
        {
            try
            {
                NoteStore.Save();
            }
            catch
            {
                // ignored
            }
        }
    }
}

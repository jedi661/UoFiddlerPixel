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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using System.Xml;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms.Notepad
{
    public class Note
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "New note";
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public int Color { get; set; }
        public bool Pinned { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Modified { get; set; } = DateTime.Now;
        public string Rtf { get; set; } = "";
        public string PlainText { get; set; } = "";
        public bool CodeMode { get; set; }
        public DateTime? ReminderAt { get; set; }
        public bool ReminderDone { get; set; }

        public string TagText => string.Join(", ", Tags ?? new List<string>());

        /// <summary>
        /// All words must appear in title, category, tags or text.
        /// </summary>
        public bool Matches(string[] terms)
        {
            if (terms == null || terms.Length == 0)
            {
                return true;
            }

            string all = string.Join("\n", Title, Category, TagText, PlainText);
            return terms.All(t => all.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public string Snippet(int length = 90)
        {
            string text = (PlainText ?? "").Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('￼', ' ');
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            text = text.Trim();
            return text.Length > length ? text.Substring(0, length) + "…" : text;
        }
    }

    public class NotepadSettings
    {
        public bool WordWrap { get; set; } = true;
        public bool LineNumbers { get; set; } = true;
        public bool TopMost { get; set; }
        public bool ShowList { get; set; } = true;
        public int SplitterDistance { get; set; } = 290;
        public string SortBy { get; set; } = "Modified";
        public float Zoom { get; set; } = 1f;
        public string DefaultFont { get; set; } = "Segoe UI";
        public float DefaultSize { get; set; } = 11f;
        public bool PrintHeader { get; set; } = true;
        public List<string> OpenNoteIds { get; set; } = new();
        public string ActiveNoteId { get; set; } = "";
    }

    public class NotepadData
    {
        public int Version { get; set; } = 1;
        public List<Note> Notes { get; set; } = new();
        public NotepadSettings Settings { get; set; } = new();
    }

    /// <summary>
    /// Loads and saves all notes as JSON in the UoFiddler app data folder (with daily backups).
    /// </summary>
    public static class NoteStore
    {
        private static readonly JsonSerializerOptions _json = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
        private static NotepadData _data;

        /// <summary>
        /// Raised after a note was saved. The second argument is the object that saved it.
        /// </summary>
        public static event Action<Note, object> NoteSaved;

        public static string Folder =>
            Path.Combine(string.IsNullOrEmpty(Options.AppDataPath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UoFiddler")
                : Options.AppDataPath, "Notepad");

        public static string FilePath => Path.Combine(Folder, "notes.json");

        public static string BackupFolder => Path.Combine(Folder, "Backup");

        public static NotepadData Data => _data ??= Load();

        public static string LastMigrationInfo { get; private set; } = "";

        private static NotepadData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    NotepadData data = JsonSerializer.Deserialize<NotepadData>(File.ReadAllText(FilePath), _json);
                    if (data != null)
                    {
                        data.Notes ??= new List<Note>();
                        data.Settings ??= new NotepadSettings();
                        data.Settings.OpenNoteIds ??= new List<string>();
                        foreach (Note n in data.Notes)
                        {
                            n.Tags ??= new List<string>();
                        }

                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.Copy(FilePath, FilePath + ".broken", true);
                }
                catch
                {
                    // ignored
                }

                MessageBox.Show("The notes file could not be read and was copied to notes.json.broken.\n\n" + ex.Message,
                    "Notepad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            var fresh = new NotepadData();
            MigrateOldNotes(fresh);
            return fresh;
        }

        /// <summary>
        /// Takes over the notes of the old notepad (NotepadMessage.xml) once.
        /// </summary>
        private static void MigrateOldNotes(NotepadData data)
        {
            var candidates = new[]
            {
                Path.Combine(Environment.CurrentDirectory, "NotepadMessage.xml"),
                Path.Combine(AppContext.BaseDirectory, "NotepadMessage.xml")
            }.Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (string file in candidates)
            {
                if (!File.Exists(file))
                {
                    continue;
                }

                try
                {
                    var doc = new XmlDocument();
                    doc.Load(file);
                    using var rtb = new RichTextBox();
                    int count = 0;
                    foreach (XmlElement e in doc.GetElementsByTagName("Note"))
                    {
                        string rtf = e.GetAttribute("rtfText");
                        string plain = "";
                        try
                        {
                            rtb.Rtf = rtf;
                            plain = rtb.Text;
                        }
                        catch (ArgumentException)
                        {
                            plain = rtf;
                            rtf = "";
                        }

                        string title = e.GetAttribute("headline");
                        data.Notes.Add(new Note
                        {
                            Title = string.IsNullOrWhiteSpace(title) ? "Note " + e.GetAttribute("id") : title,
                            Rtf = rtf,
                            PlainText = plain,
                            Category = "Imported"
                        });
                        count++;
                    }

                    if (count > 0)
                    {
                        LastMigrationInfo = $"{count} note(s) taken over from {file}";
                        Save(data);
                    }

                    return;
                }
                catch
                {
                    // old file unreadable – start empty
                }
            }
        }

        public static void Save(NotepadData data = null)
        {
            data ??= Data;
            Directory.CreateDirectory(Folder);
            BackupOncePerDay();
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, _json), Encoding.UTF8);
            File.Move(tmp, FilePath, true);
        }

        public static void NotifySaved(Note note, object source) => NoteSaved?.Invoke(note, source);

        private static void BackupOncePerDay()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    return;
                }

                Directory.CreateDirectory(BackupFolder);
                string target = Path.Combine(BackupFolder, $"notes_{DateTime.Now:yyyyMMdd}.json");
                if (File.Exists(target))
                {
                    return;
                }

                File.Copy(FilePath, target);

                // keep the last 14 backups
                foreach (string old in Directory.GetFiles(BackupFolder, "notes_*.json").OrderByDescending(f => f).Skip(14))
                {
                    File.Delete(old);
                }
            }
            catch
            {
                // a failed backup must never block saving
            }
        }

        public static Note Find(string id) => Data.Notes.FirstOrDefault(n => n.Id == id);
    }
}

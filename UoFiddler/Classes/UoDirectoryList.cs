using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Classes
{
    /// <summary>One Ultima Online folder of the quick list.</summary>
    public sealed class UoDirectoryEntry
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Favourite { get; set; }
        public DateTime LastUsed { get; set; }

        public override string ToString()
        {
            return Favourite
                ? $"* {(string.IsNullOrEmpty(Name) ? System.IO.Path.GetFileName(Path.TrimEnd('\\', '/')) : Name)}  -  {Path}"
                : $"{Path}   (last used {LastUsed:yyyy-MM-dd HH:mm})";
        }
    }

    /// <summary>
    /// Quick list of Ultima Online folders: favourites (saved with a name) and the last used folders.
    /// Stored in &lt;AppData&gt;\UoFiddler\UoDirectories.json (independent of the profile).
    /// </summary>
    public static class UoDirectoryList
    {
        private const int MaxRecent = 15;

        private static List<UoDirectoryEntry> _entries;

        public static string FilePath => System.IO.Path.Combine(Options.AppDataPath ?? AppContext.BaseDirectory, "UoDirectories.json");

        /// <summary>Favourites first (by name), then the recent folders (newest first).</summary>
        public static IReadOnlyList<UoDirectoryEntry> Entries
        {
            get
            {
                Load();
                return _entries.Where(e => e.Favourite).OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                    .Concat(_entries.Where(e => !e.Favourite).OrderByDescending(e => e.LastUsed)).ToList();
            }
        }

        private static void Load()
        {
            if (_entries != null)
            {
                return;
            }

            _entries = new List<UoDirectoryEntry>();
            try
            {
                if (File.Exists(FilePath))
                {
                    _entries = JsonSerializer.Deserialize<List<UoDirectoryEntry>>(File.ReadAllText(FilePath)) ?? new List<UoDirectoryEntry>();
                }
            }
            catch (Exception)
            {
                _entries = new List<UoDirectoryEntry>(); // damaged file: start with an empty list
            }

            _entries.RemoveAll(e => string.IsNullOrWhiteSpace(e?.Path));
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath));
                File.WriteAllText(FilePath, JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception)
            {
                // not important enough to bother the user
            }
        }

        private static string Normalize(string path)
        {
            try
            {
                return System.IO.Path.GetFullPath(path).TrimEnd('\\', '/');
            }
            catch (Exception)
            {
                return path?.Trim().TrimEnd('\\', '/');
            }
        }

        private static UoDirectoryEntry Find(string path)
        {
            Load();
            string n = Normalize(path);
            return _entries.FirstOrDefault(e => string.Equals(Normalize(e.Path), n, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Remembers a used folder (recent list, max. 15 that are not favourites).</summary>
        public static void MarkUsed(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            UoDirectoryEntry e = Find(path);
            if (e == null)
            {
                e = new UoDirectoryEntry { Path = Normalize(path) };
                _entries.Add(e);
            }

            e.LastUsed = DateTime.Now;
            foreach (UoDirectoryEntry old in _entries.Where(x => !x.Favourite).OrderByDescending(x => x.LastUsed).Skip(MaxRecent).ToList())
            {
                _entries.Remove(old);
            }

            Save();
        }

        /// <summary>Saves a folder with a name in the quick list.</summary>
        public static void SaveFavourite(string path, string name)
        {
            UoDirectoryEntry e = Find(path);
            if (e == null)
            {
                e = new UoDirectoryEntry { Path = Normalize(path), LastUsed = DateTime.Now };
                _entries.Add(e);
            }

            e.Favourite = true;
            e.Name = string.IsNullOrWhiteSpace(name) ? System.IO.Path.GetFileName(Normalize(path)) : name.Trim();
            Save();
        }

        public static bool Remove(string path)
        {
            UoDirectoryEntry e = Find(path);
            if (e == null)
            {
                return false;
            }

            _entries.Remove(e);
            Save();
            return true;
        }

        public static UoDirectoryEntry Get(string path) => Find(path);
    }
}

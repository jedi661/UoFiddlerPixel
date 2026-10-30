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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UoFiddler.Plugin.Enhanced.EcUltima.Uop;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Sounds
{
    public sealed class EcSound
    {
        /// <summary>Sound id (music: the music id).</summary>
        public int Id { get; set; }
        public bool IsMusic { get; set; }

        /// <summary>Path as written in the list (e.g. "Music/turfin.mp3").</summary>
        public string Path { get; set; }

        /// <summary>Further columns of the list line (e.g. the loop flag of the music).</summary>
        public string Extra { get; set; }

        /// <summary>Packed name ("data/audio/" + path in lower case).</summary>
        public string PackedName => "data/audio/" + Path.Replace('\\', '/').ToLowerInvariant();

        public EcUopEntry Entry { get; internal set; }
    }

    /// <summary>
    /// Audio.uop: mp3 / wav files ("data/audio/...") listed in "data/audio/audio_sounds.csv" (id, path) and
    /// "data/audio/audio_music.csv" (id, path, loop). Files that are in no list are shown as well.
    /// </summary>
    public sealed class EcSounds
    {
        public const string SoundListName = "data/audio/audio_sounds.csv";
        public const string MusicListName = "data/audio/audio_music.csv";

        private readonly List<EcSound> _sounds = new List<EcSound>();
        private readonly Dictionary<string, List<string>> _listHeaders = new Dictionary<string, List<string>>();
        private bool _listsChanged;
        private string _newLine = "\r\n";

        public EcUopPackage Package { get; }
        public IReadOnlyList<EcSound> Sounds => _sounds;

        /// <summary>Audio files that are not in one of the lists.</summary>
        public List<EcUopEntry> Unlisted { get; } = new List<EcUopEntry>();

        public EcSounds(EcUopPackage package)
        {
            Package = package;
            package.ResolveName(SoundListName);
            package.ResolveName(MusicListName);

            ReadList(SoundListName, false);
            ReadList(MusicListName, true);

            var listed = new HashSet<EcUopEntry>(_sounds.Where(s => s.Entry != null).Select(s => s.Entry));
            foreach (EcUopEntry e in package.Entries)
            {
                if (!listed.Contains(e) && e.Name != SoundListName && e.Name != MusicListName && IsAudioName(e.Name))
                {
                    Unlisted.Add(e);
                }
            }
        }

        private static bool IsAudioName(string name)
        {
            if (name == null)
            {
                return true; // unknown names are shown as unlisted
            }
            string ext = System.IO.Path.GetExtension(name).ToLowerInvariant();
            return ext == ".mp3" || ext == ".wav" || ext == ".ogg";
        }

        private void ReadList(string listName, bool music)
        {
            EcUopEntry entry = Package.GetEntry(listName);
            var headers = new List<string>();
            _listHeaders[listName] = headers;
            if (entry == null)
            {
                return;
            }

            string text = Encoding.UTF8.GetString(Package.ReadData(entry));
            _newLine = text.Contains("\r\n") ? "\r\n" : "\n";
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (line.Length == 0)
                {
                    continue;
                }
                if (line.StartsWith("#", StringComparison.Ordinal) || line.StartsWith(";", StringComparison.Ordinal))
                {
                    headers.Add(line);
                    continue;
                }

                string[] parts = line.Split(new[] { ',' }, 3);
                if (parts.Length < 2 || !int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    headers.Add(line);
                    continue;
                }

                var sound = new EcSound { Id = id, IsMusic = music, Path = parts[1].Trim(), Extra = parts.Length > 2 ? parts[2] : null };
                sound.Entry = Package.GetEntry(sound.PackedName);
                if (sound.Entry != null)
                {
                    sound.Entry.Name = sound.PackedName;
                }
                _sounds.Add(sound);
            }
        }

        public byte[] ReadData(EcSound sound) => sound.Entry == null ? null : Package.ReadData(sound.Entry);

        /// <summary>Replaces the audio data of a sound (if several ids share the file, all of them change).</summary>
        public void SetData(EcSound sound, byte[] data)
        {
            if (sound.Entry != null)
            {
                Package.SetData(sound.Entry, data);
            }
            else
            {
                sound.Entry = Package.AddOrReplace(sound.PackedName, data, EcUopCompression.None);
            }
        }

        /// <summary>Adds a sound to a list with its own file.</summary>
        public EcSound Add(int id, bool music, string path, byte[] data)
        {
            var sound = new EcSound { Id = id, IsMusic = music, Path = path, Extra = music ? "0" : null };
            sound.Entry = Package.AddOrReplace(sound.PackedName, data, EcUopCompression.None);
            _sounds.RemoveAll(s => s.IsMusic == music && s.Id == id);
            _sounds.Add(sound);
            _listsChanged = true;
            return sound;
        }

        /// <summary>Removes the sound from its list; the file is removed when no other id uses it.</summary>
        public void Remove(EcSound sound)
        {
            _sounds.Remove(sound);
            if (sound.Entry != null && !_sounds.Any(s => s.Entry == sound.Entry))
            {
                Package.Remove(sound.Entry);
            }
            _listsChanged = true;
        }

        public void ChangePath(EcSound sound, string path)
        {
            sound.Path = path;
            sound.Entry = Package.GetEntry(sound.PackedName);
            _listsChanged = true;
        }

        /// <summary>Writes the changed lists into the package.</summary>
        public void Apply()
        {
            if (!_listsChanged)
            {
                return;
            }
            WriteList(SoundListName, false);
            WriteList(MusicListName, true);
            _listsChanged = false;
        }

        private void WriteList(string listName, bool music)
        {
            var sb = new StringBuilder();
            foreach (string header in _listHeaders.TryGetValue(listName, out List<string> h) ? h : new List<string>())
            {
                sb.Append(header).Append(_newLine);
            }
            foreach (EcSound s in _sounds.Where(x => x.IsMusic == music).OrderBy(x => x.Id))
            {
                sb.Append(s.Id.ToString(CultureInfo.InvariantCulture)).Append(',').Append(s.Path);
                if (s.Extra != null)
                {
                    sb.Append(',').Append(s.Extra);
                }
                sb.Append(_newLine);
            }
            Package.AddOrReplace(listName, Encoding.UTF8.GetBytes(sb.ToString()), Package.PreferredCompression);
        }
    }
}

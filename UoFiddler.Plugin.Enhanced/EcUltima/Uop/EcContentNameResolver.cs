/***************************************************************************
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
using System.Text.RegularExpressions;
using System.Threading;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Uop
{
    /// <summary>
    /// Finds packed file names from the contents of the package: many Enhanced Client files (Interface.uop, Audio.uop, ...)
    /// store real paths ("data/interface/default/Source/ContextMenu.lua"), and the xml / mod / lua / csv files inside
    /// mention the other files relative to their own folder. Starting with a few known names, every text entry is read and
    /// every path in it is tried, until no new name is found.
    /// </summary>
    public static class EcContentNameResolver
    {
        private sealed class Settings
        {
            public string[] Seeds = Array.Empty<string>();
            public string[] Roots = Array.Empty<string>();
        }

        private static readonly Dictionary<string, Settings> _files = new Dictionary<string, Settings>(StringComparer.OrdinalIgnoreCase)
        {
            ["Interface.uop"] = new Settings
            {
                Seeds = new[]
                {
                    "data/interface/DefaultModules.txt", "data/interface/Interface.xsd", "data/interface/default/Interface.xml",
                    "data/interface/default/default.mod", "data/interface/Default.mod"
                },
                Roots = new[] { "data/interface/", "data/interface/default/", "data/" }
            },
            ["Audio.uop"] = new Settings
            {
                Seeds = new[] { "data/audio/audio_sounds.csv", "data/audio/audio_music.csv" },
                Roots = new[] { "data/audio/", "data/audio/sounds/", "data/audio/music/", "data/" }
            },
            ["GameData.uop"] = new Settings { Roots = new[] { "data/gamedata/", "data/", "gamedata/" } },
            ["Hues.uop"] = new Settings { Roots = new[] { "data/hues/", "data/", "hues/" } },
            ["MainMisc.uop"] = new Settings { Roots = new[] { "data/", "data/mainmisc/" } },
            ["Shaders.uop"] = new Settings { Roots = new[] { "data/shaders/", "data/", "shaders/" } },
            ["SystemTextures.uop"] = new Settings { Roots = new[] { "data/systemtextures/", "data/textures/", "data/", "systemtextures/" } },
            ["LegacyTerrain.uop"] = new Settings { Roots = new[] { "data/legacyterrain/", "data/terrain/", "data/" } },
            ["LocalizedStrings.uop"] = new Settings { Roots = new[] { "data/localizedstrings/", "data/", "localizedstrings/" } },
            ["waypoint.uop"] = new Settings { Roots = new[] { "data/waypoint/", "data/" } },
        };

        private static readonly Regex _pathRegex = new Regex(
            @"[A-Za-z0-9_\-\\/\.\$ ]{1,160}\.(?:xml|lua|mod|xsd|dds|tga|png|jpg|bmp|ttf|fnt|txt|csv|mp3|wav|ogg|nif|ems|kf|fx|psh|vsh|bin|dat)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly string[] _imageOrAudio = { ".dds", ".tga", ".png", ".jpg", ".bmp", ".mp3", ".wav", ".ogg", ".nif", ".ttf" };

        /// <summary>
        /// True when names of this file are worth searching in its contents.
        /// </summary>
        public static bool Supports(string fileName) => _files.ContainsKey(fileName);

        /// <summary>
        /// Resolves names; returns the number of entries that got a name.
        /// </summary>
        public static int Resolve(EcUopPackage package, string fileName, CancellationToken cancel = default, int maxRounds = 12)
        {
            if (package.Entries.All(e => e.Name != null))
            {
                return 0;
            }

            string baseName = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
            Settings settings = _files.TryGetValue(fileName, out Settings s)
                ? s
                : new Settings { Roots = new[] { "data/", $"data/{baseName}/", $"build/{baseName}/" } };

            int before = package.Entries.Count(e => e.Name != null);
            foreach (string seed in settings.Seeds)
            {
                package.ResolveName(seed);
            }

            var processed = new HashSet<ulong>();
            var textCache = new Dictionary<ulong, List<string>>();

            for (int round = 0; round < maxRounds; round++)
            {
                cancel.ThrowIfCancellationRequested();
                int namedAtStart = package.Entries.Count(e => e.Name != null);

                // all folders known so far are bases for bare file names
                var knownFolders = new HashSet<string>(package.Entries.Where(e => e.Name != null).Select(e => Folder(e.Name)), StringComparer.OrdinalIgnoreCase);
                foreach (string root in settings.Roots)
                {
                    knownFolders.Add(root);
                }

                foreach (EcUopEntry entry in package.Entries.ToList())
                {
                    cancel.ThrowIfCancellationRequested();

                    // named entries are read once with their name (relative paths), unnamed ones once without
                    ulong key = entry.Hash ^ (entry.Name != null ? 0x5A5A5A5AUL : 0);
                    if (!processed.Add(key))
                    {
                        continue;
                    }

                    List<string> paths = GetPaths(package, entry, textCache);
                    if (paths == null || paths.Count == 0)
                    {
                        continue;
                    }

                    string ownFolder = entry.Name != null ? Folder(entry.Name) : null;
                    foreach (string path in paths)
                    {
                        TryPath(package, path, ownFolder, knownFolders);
                    }
                }

                if (package.Entries.Count(e => e.Name != null) == namedAtStart)
                {
                    break;
                }
            }

            return package.Entries.Count(e => e.Name != null) - before;
        }

        private static List<string> GetPaths(EcUopPackage package, EcUopEntry entry, Dictionary<ulong, List<string>> cache)
        {
            if (cache.TryGetValue(entry.Hash, out List<string> cached))
            {
                return cached;
            }

            List<string> result = null;
            try
            {
                bool knownNonText = entry.Name != null && _imageOrAudio.Contains(Path.GetExtension(entry.Name).ToLowerInvariant());
                if (!knownNonText && entry.IsSupported && entry.DecompressedSize <= 8 * 1024 * 1024)
                {
                    byte[] head = package.ReadDataHead(entry, 512);
                    if (LooksLikeText(head))
                    {
                        string text = Decode(package.ReadData(entry));
                        result = _pathRegex.Matches(text).Select(m => m.Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    }
                }
            }
            catch (Exception)
            {
                result = null;
            }

            cache[entry.Hash] = result;
            return result;
        }

        private static void TryPath(EcUopPackage package, string raw, string ownFolder, HashSet<string> knownFolders)
        {
            string path = raw.Trim().Trim('"', '\'').Replace('\\', '/');
            while (path.StartsWith("./", StringComparison.Ordinal))
            {
                path = path.Substring(2);
            }
            path = path.TrimStart('/');
            if (path.Length < 3)
            {
                return;
            }

            // names written from the root ("Data/Interface/...")
            package.ResolveName(path);

            if (ownFolder != null)
            {
                string relative = Combine(ownFolder, path);
                if (relative != null)
                {
                    package.ResolveName(relative);
                }

                // also the parent folders (mod files often refer to the module root)
                string parent = ownFolder;
                for (int i = 0; i < 4 && parent.Length > 0; i++)
                {
                    parent = Folder(parent.TrimEnd('/'));
                    string candidate = Combine(parent, path);
                    if (candidate != null)
                    {
                        package.ResolveName(candidate);
                    }
                }
            }

            // bare file names: every known folder
            if (!path.Contains('/'))
            {
                foreach (string folder in knownFolders)
                {
                    package.ResolveName(folder + path);
                }
            }
            else
            {
                foreach (string folder in knownFolders.Where(f => f.Count(c => c == '/') <= 2))
                {
                    string candidate = Combine(folder, path);
                    if (candidate != null)
                    {
                        package.ResolveName(candidate);
                    }
                }
            }
        }

        /// <summary>Folder part including the trailing slash ("a/b/c.xml" -> "a/b/").</summary>
        private static string Folder(string name)
        {
            int slash = name.LastIndexOf('/');
            return slash >= 0 ? name.Substring(0, slash + 1) : string.Empty;
        }

        /// <summary>Joins folder and relative path and resolves "../".</summary>
        private static string Combine(string folder, string path)
        {
            var parts = new List<string>((folder + path).Split('/'));
            var result = new List<string>();
            foreach (string part in parts)
            {
                if (part == "..")
                {
                    if (result.Count == 0)
                    {
                        return null;
                    }
                    result.RemoveAt(result.Count - 1);
                }
                else if (part.Length > 0 && part != ".")
                {
                    result.Add(part);
                }
            }
            return string.Join("/", result);
        }

        private static bool LooksLikeText(byte[] head)
        {
            if (head.Length == 0)
            {
                return false;
            }
            if (head.Length >= 2 && ((head[0] == 0xFF && head[1] == 0xFE) || (head[0] == 0xFE && head[1] == 0xFF)))
            {
                return true; // UTF-16
            }
            int printable = 0;
            int start = head.Length >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF ? 3 : 0;
            for (int i = start; i < head.Length; i++)
            {
                byte b = head[i];
                if (b == 0)
                {
                    return false;
                }
                if (b == 9 || b == 10 || b == 13 || (b >= 32 && b < 127) || b >= 128)
                {
                    printable++;
                }
            }
            return printable >= (head.Length - start) * 0.95;
        }

        private static string Decode(byte[] data)
        {
            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(data, 2, data.Length - 2);
            }
            if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2);
            }
            return Encoding.UTF8.GetString(data);
        }
    }
}

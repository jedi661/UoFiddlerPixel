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
using System.IO;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// Plugin settings stored as "key=value" lines in the UoFiddler app data folder.
    /// </summary>
    public static class EcSettings
    {
        private const string FileName = "EnhancedClientPlugin.cfg";
        private static Dictionary<string, string> _values;

        private static string FilePath
        {
            get
            {
                string dir = string.IsNullOrEmpty(Options.AppDataPath) ? AppContext.BaseDirectory : Options.AppDataPath;
                return Path.Combine(dir, FileName);
            }
        }

        public static string Get(string key, string defaultValue = null)
        {
            Load();
            return _values.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public static void Set(string key, string value)
        {
            Load();
            _values[key] = value ?? string.Empty;
            try
            {
                var lines = new List<string>();
                foreach (KeyValuePair<string, string> kv in _values)
                {
                    lines.Add($"{kv.Key}={kv.Value}");
                }
                File.WriteAllLines(FilePath, lines);
            }
            catch (Exception)
            {
                // settings are only a convenience
            }
        }

        private static void Load()
        {
            if (_values != null)
            {
                return;
            }

            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(FilePath))
                {
                    foreach (string line in File.ReadAllLines(FilePath))
                    {
                        int p = line.IndexOf('=');
                        if (p > 0)
                        {
                            _values[line.Substring(0, p)] = line.Substring(p + 1);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore broken settings
            }
        }
    }
}

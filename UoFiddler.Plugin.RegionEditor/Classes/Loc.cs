/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Texts of the region editor in German, English and French.
 * The chosen language is stored in the settings (<AppData>\RegionEditor.json),
 * the texts themselves are in Loc.Texts.cs.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace UoFiddler.Plugin.RegionEditor.Classes
{
    public static partial class Loc
    {
        public static readonly string[] Languages = { "de", "en", "fr" };
        public static readonly string[] LanguageNames = { "Deutsch", "English", "Français" };

        private static string _language;

        /// <summary>Raised after the language was changed</summary>
        public static event EventHandler LanguageChanged;

        public static string Language
        {
            get
            {
                if (_language == null)
                {
                    string stored = RegionEditorSettings.Current.Language;
                    if (Array.IndexOf(Languages, stored) >= 0)
                    {
                        _language = stored;
                    }
                    else
                    {
                        // first start: language of Windows (German, French), otherwise English
                        string ui = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                        _language = ui == "de" || ui == "fr" ? ui : "en";
                    }
                }

                return _language;
            }
            set
            {
                if (Array.IndexOf(Languages, value) < 0 || value == _language)
                {
                    return;
                }

                _language = value;
                RegionEditorSettings.Current.Language = value;
                RegionEditorSettings.Current.Save();
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        /// <summary>Text of a key in the current language (the key itself when unknown)</summary>
        public static string T(string key)
        {
            if (!_texts.TryGetValue(key, out string[] values))
            {
                return key;
            }

            int i = Array.IndexOf(Languages, Language);
            return values[i < 0 ? 1 : i];
        }

        /// <summary>Formatted text of a key</summary>
        public static string F(string key, params object[] args)
        {
            string format = T(key);
            try
            {
                return string.Format(CultureInfo.InvariantCulture, format, args ?? Array.Empty<object>());
            }
            catch (FormatException)
            {
                return format;
            }
        }

        public static bool Has(string key) => _texts.ContainsKey(key);

        /// <summary>all keys (tests: every key must have three texts)</summary>
        public static IEnumerable<KeyValuePair<string, string[]>> All => _texts;
    }
}

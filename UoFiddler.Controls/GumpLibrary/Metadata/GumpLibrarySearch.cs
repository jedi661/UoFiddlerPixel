// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  *
//  * \"THE BEER-WINE-WARE LICENSE\"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UoFiddler.Controls.ArtLibrary.Helpers;

namespace UoFiddler.Controls.GumpLibrary.Metadata
{
    /// <summary>How the gump list is sorted.</summary>
    public enum GumpSortMode
    {
        Id = 0,
        Custom = 1,
        Name = 2,
        SourceId = 3,
        Category = 4,
        Added = 5,
        Modified = 6
    }

    /// <summary>
    /// Full text search over id, original id, name, description, tags and categories.
    /// <code>
    /// hemd rot          both words must be found (anywhere)
    /// "rotes hemd"      exact phrase
    /// -leder            must not contain "leder"
    /// tag:rot  cat:kleidung  name:hemd  desc:alt  src:0xC350
    /// id:0x10-0x20      id or id range (decimal or hex)
    /// is:fav  is:preview  is:nocat  is:notag  is:noname
    /// </code>
    /// </summary>
    public sealed class GumpLibrarySearch
    {
        private readonly List<Term> _terms = new List<Term>();

        private GumpLibrarySearch(string text)
        {
            Text = text ?? string.Empty;
            foreach (string token in Tokenize(Text))
            {
                string value = token;
                bool negate = false;
                if (value.Length > 1 && value[0] == '-')
                {
                    negate = true;
                    value = value.Substring(1);
                }

                string field = string.Empty;
                int colon = value.IndexOf(':');
                if (colon > 0 && colon < value.Length - 1)
                {
                    string prefix = value.Substring(0, colon).ToLowerInvariant();
                    if (IsField(prefix))
                    {
                        field = Normalize(prefix);
                        value = value.Substring(colon + 1);
                    }
                }

                var term = new Term { Field = field, Value = value, Negate = negate };
                // plain words like "face" are text, not hex: without a prefix only numbers and 0x.. count as id
                if (field == "id" || field == "src" || (field.Length == 0 && LooksLikeId(value)))
                {
                    if (ArtIdParser.TryParseList(value, out List<int> ids, 100_000))
                    {
                        term.Ids = new HashSet<int>(ids);
                    }
                }

                _terms.Add(term);
            }
        }

        public string Text { get; }

        public bool IsEmpty => _terms.Count == 0;

        public static GumpLibrarySearch Parse(string text) => new GumpLibrarySearch(text);

        /// <param name="hasPreview">Tells whether the id has an own preview image (for is:preview).</param>
        public bool Matches(int id, GumpLibraryMetadata metadata, Func<int, bool> hasPreview = null)
        {
            if (_terms.Count == 0)
            {
                return true;
            }

            GumpLibraryItemInfo info = metadata?.Get(id);
            foreach (Term term in _terms)
            {
                if (MatchTerm(term, id, info, metadata, hasPreview) == term.Negate)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchTerm(Term term, int id, GumpLibraryItemInfo info, GumpLibraryMetadata metadata, Func<int, bool> hasPreview)
        {
            switch (term.Field)
            {
                case "id":
                    return term.Ids != null ? term.Ids.Contains(id) : Contains(ArtIdParser.ToHex(id), term.Value);
                case "src":
                    return info?.SourceId != null && (term.Ids != null ? term.Ids.Contains(info.SourceId.Value) : Contains($"0x{info.SourceId.Value:X}", term.Value));
                case "name":
                    return Contains(info?.Name, term.Value);
                case "desc":
                    return Contains(info?.Description, term.Value);
                case "tag":
                    return info != null && info.Tags.Any(t => Contains(t, term.Value));
                case "cat":
                    return info != null && info.CategoryIds.Any(c => Contains(metadata.GetCategoryPath(c), term.Value));
                case "is":
                    switch (term.Value.ToLowerInvariant())
                    {
                        case "fav":
                        case "favorit":
                        case "favorite":
                            return info?.Favorite == true;
                        case "preview":
                        case "vorschau":
                            return hasPreview?.Invoke(id) == true;
                        case "nocat":
                        case "ohnekategorie":
                            return info == null || info.CategoryIds.Count == 0;
                        case "notag":
                            return info == null || info.Tags.Count == 0;
                        case "noname":
                            return info == null || string.IsNullOrWhiteSpace(info.Name);
                        default:
                            return false;
                    }
            }

            if (term.Ids != null && (term.Ids.Contains(id) || (info?.SourceId != null && term.Ids.Contains(info.SourceId.Value))))
            {
                return true;
            }

            if (info == null)
            {
                return false;
            }

            return Contains(info.Name, term.Value)
                   || Contains(info.Description, term.Value)
                   || info.Tags.Any(t => Contains(t, term.Value))
                   || info.CategoryIds.Any(c => Contains(metadata.GetCategoryPath(c), term.Value));
        }

        private static bool LooksLikeId(string value)
        {
            foreach (string part in value.Split('-'))
            {
                string p = part.Trim();
                bool hex = p.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && p.Length > 2 &&
                           p.Substring(2).All(Uri.IsHexDigit);
                if (!hex && (p.Length == 0 || !p.All(char.IsDigit)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsField(string prefix)
        {
            switch (prefix)
            {
                case "id":
                case "src":
                case "orig":
                case "name":
                case "desc":
                case "beschreibung":
                case "tag":
                case "cat":
                case "kat":
                case "kategorie":
                case "is":
                    return true;
                default:
                    return false;
            }
        }

        private static string Normalize(string prefix)
        {
            switch (prefix)
            {
                case "orig":
                    return "src";
                case "beschreibung":
                    return "desc";
                case "kat":
                case "kategorie":
                    return "cat";
                default:
                    return prefix;
            }
        }

        /// <summary>Splits on spaces, keeps "quoted phrases" together.</summary>
        public static List<string> Tokenize(string text)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool quoted = false;
            foreach (char c in text ?? string.Empty)
            {
                if (c == '"')
                {
                    quoted = !quoted;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !quoted)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }

                    continue;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result;
        }

        private static bool Contains(string value, string text)
        {
            return !string.IsNullOrEmpty(value) &&
                   CultureInfo.CurrentCulture.CompareInfo.IndexOf(value, text, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        /// <summary>Sorts the ids for the list.</summary>
        public static List<int> Sort(IEnumerable<int> ids, GumpLibraryMetadata metadata, GumpSortMode mode, bool descending)
        {
            Func<int, GumpLibraryItemInfo> get = metadata.Get;
            IOrderedEnumerable<int> ordered;
            StringComparer text = StringComparer.CurrentCultureIgnoreCase;
            switch (mode)
            {
                case GumpSortMode.Custom:
                    ordered = ids.OrderBy(id => get(id)?.SortOrder ?? int.MaxValue).ThenBy(id => id);
                    break;
                case GumpSortMode.Name:
                    ordered = ids.OrderBy(id => string.IsNullOrEmpty(get(id)?.Name) ? 1 : 0).ThenBy(id => get(id)?.Name ?? string.Empty, text).ThenBy(id => id);
                    break;
                case GumpSortMode.SourceId:
                    ordered = ids.OrderBy(id => get(id)?.SourceId ?? int.MaxValue).ThenBy(id => id);
                    break;
                case GumpSortMode.Category:
                    ordered = ids.OrderBy(id => get(id)?.CategoryIds.Count > 0 ? 0 : 1)
                        .ThenBy(id => FirstCategory(metadata, get(id)), text)
                        .ThenBy(id => get(id)?.SortOrder ?? int.MaxValue)
                        .ThenBy(id => id);
                    break;
                case GumpSortMode.Added:
                    ordered = ids.OrderBy(id => get(id)?.Added ?? DateTime.MaxValue).ThenBy(id => id);
                    break;
                case GumpSortMode.Modified:
                    ordered = ids.OrderBy(id => get(id)?.Modified ?? DateTime.MaxValue).ThenBy(id => id);
                    break;
                default:
                    ordered = ids.OrderBy(id => id);
                    break;
            }

            List<int> list = ordered.ToList();
            if (descending)
            {
                list.Reverse();
            }

            return list;
        }

        private static string FirstCategory(GumpLibraryMetadata metadata, GumpLibraryItemInfo info)
        {
            if (info == null || info.CategoryIds.Count == 0)
            {
                return string.Empty;
            }

            return info.CategoryIds.Select(metadata.GetCategoryPath).OrderBy(p => p, StringComparer.CurrentCultureIgnoreCase).First();
        }

        public static string GetSortModeText(GumpSortMode mode)
        {
            switch (mode)
            {
                case GumpSortMode.Custom: return "Eigene Reihenfolge";
                case GumpSortMode.Name: return "Name";
                case GumpSortMode.SourceId: return "Original-ID";
                case GumpSortMode.Category: return "Kategorie";
                case GumpSortMode.Added: return "Hinzugefügt";
                case GumpSortMode.Modified: return "Geändert";
                default: return "ID";
            }
        }

        private sealed class Term
        {
            public string Field;
            public string Value;
            public bool Negate;
            public HashSet<int> Ids;
        }
    }
}

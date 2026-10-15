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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UoFiddler.Controls.ArtLibrary.Metadata
{
    /// <summary>
    /// Names, descriptions, categories and tags of a library, stored as JSON next to the library
    /// (Name.artlib.json). Only ids with metadata are stored.
    /// </summary>
    public sealed class ArtLibraryMetadata
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string _path;

        public int Version { get; set; } = 1;

        public List<ArtLibraryCategory> Categories { get; set; } = new List<ArtLibraryCategory>();

        /// <summary>All known tags (also tags not used by any graphic yet).</summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Metadata per graphic id.</summary>
        public Dictionary<int, ArtLibraryItemInfo> Items { get; set; } = new Dictionary<int, ArtLibraryItemInfo>();

        /// <summary>True when there are unsaved changes.</summary>
        [JsonIgnore]
        public bool IsDirty { get; private set; }

        #region [ Load / Save ]

        public static ArtLibraryMetadata Load(string path)
        {
            ArtLibraryMetadata metadata = null;
            if (File.Exists(path))
            {
                metadata = JsonSerializer.Deserialize<ArtLibraryMetadata>(File.ReadAllText(path), _jsonOptions);
            }

            metadata ??= new ArtLibraryMetadata();
            metadata._path = path;
            metadata.Categories ??= new List<ArtLibraryCategory>();
            metadata.Tags ??= new List<string>();
            metadata.Items ??= new Dictionary<int, ArtLibraryItemInfo>();
            foreach (ArtLibraryItemInfo info in metadata.Items.Values)
            {
                info.Tags ??= new List<string>();
            }

            metadata.IsDirty = false;
            return metadata;
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(_path))
            {
                throw new InvalidOperationException("Metadata has no file path.");
            }

            foreach (int id in Items.Where(pair => pair.Value == null || pair.Value.IsEmpty).Select(pair => pair.Key).ToList())
            {
                Items.Remove(id);
            }

            // write to a temp file first so a crash never leaves a half written json
            string temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, _jsonOptions));
            File.Move(temp, _path, true);
            IsDirty = false;
        }

        public void SaveIfDirty()
        {
            if (IsDirty)
            {
                Save();
            }
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Marks the metadata as saved. Used when it is stored as part of another file
        /// (e.g. the terrain library keeps its categories and tags in an ArtLibraryMetadata).
        /// </summary>
        public void AcceptChanges()
        {
            IsDirty = false;
        }

        #endregion

        #region [ Items ]

        /// <summary>Metadata of <paramref name="id"/> or null.</summary>
        public ArtLibraryItemInfo Get(int id)
        {
            return Items.TryGetValue(id, out ArtLibraryItemInfo info) ? info : null;
        }

        public ArtLibraryItemInfo GetOrCreate(int id)
        {
            if (!Items.TryGetValue(id, out ArtLibraryItemInfo info) || info == null)
            {
                info = new ArtLibraryItemInfo();
                Items[id] = info;
                IsDirty = true;
            }

            return info;
        }

        public void Set(int id, ArtLibraryItemInfo info)
        {
            if (info == null || info.IsEmpty)
            {
                Remove(id);
                return;
            }

            info.Tags = NormalizeTags(info.Tags);
            foreach (string tag in info.Tags)
            {
                AddTag(tag);
            }

            Items[id] = info;
            IsDirty = true;
        }

        public void Remove(int id)
        {
            if (Items.Remove(id))
            {
                IsDirty = true;
            }
        }

        /// <summary>Copies metadata (graphic copied inside the library).</summary>
        public void Copy(int sourceId, int targetId)
        {
            ArtLibraryItemInfo info = Get(sourceId);
            if (info == null)
            {
                Remove(targetId);
                return;
            }

            Items[targetId] = info.Clone();
            IsDirty = true;
        }

        /// <summary>Moves metadata (graphic moved inside the library).</summary>
        public void Move(int sourceId, int targetId)
        {
            if (sourceId == targetId)
            {
                return;
            }

            Copy(sourceId, targetId);
            Remove(sourceId);
        }

        public void Swap(int firstId, int secondId)
        {
            ArtLibraryItemInfo first = Get(firstId);
            ArtLibraryItemInfo second = Get(secondId);
            Items.Remove(firstId);
            Items.Remove(secondId);
            if (second != null)
            {
                Items[firstId] = second;
            }

            if (first != null)
            {
                Items[secondId] = first;
            }

            IsDirty = true;
        }

        public string GetName(int id) => Get(id)?.Name ?? string.Empty;

        #endregion

        #region [ Categories ]

        public ArtLibraryCategory GetCategory(int? id)
        {
            return id == null ? null : Categories.FirstOrDefault(c => c.Id == id.Value);
        }

        public ArtLibraryCategory AddCategory(string name, int? parentId = null)
        {
            var category = new ArtLibraryCategory
            {
                Id = Categories.Count == 0 ? 1 : Categories.Max(c => c.Id) + 1,
                Name = (name ?? string.Empty).Trim(),
                ParentId = GetCategory(parentId)?.Id
            };

            Categories.Add(category);
            IsDirty = true;
            return category;
        }

        public void RenameCategory(int id, string name)
        {
            ArtLibraryCategory category = GetCategory(id);
            if (category == null)
            {
                return;
            }

            category.Name = (name ?? string.Empty).Trim();
            IsDirty = true;
        }

        /// <summary>
        /// Deletes a category. Its sub categories move to its parent, its graphics lose the category.
        /// </summary>
        public void DeleteCategory(int id)
        {
            ArtLibraryCategory category = GetCategory(id);
            if (category == null)
            {
                return;
            }

            foreach (ArtLibraryCategory child in Categories.Where(c => c.ParentId == id))
            {
                child.ParentId = category.ParentId;
            }

            foreach (ArtLibraryItemInfo info in Items.Values.Where(i => i.CategoryId == id))
            {
                info.CategoryId = null;
            }

            Categories.Remove(category);
            IsDirty = true;
        }

        /// <summary>Full path of a category, e.g. "Möbel / Tische".</summary>
        public string GetCategoryPath(int? id)
        {
            var names = new List<string>();
            var visited = new HashSet<int>();
            ArtLibraryCategory category = GetCategory(id);
            while (category != null && visited.Add(category.Id))
            {
                names.Insert(0, category.Name);
                category = GetCategory(category.ParentId);
            }

            return string.Join(" / ", names);
        }

        /// <summary>The category and all of its sub categories (recursive).</summary>
        public HashSet<int> GetCategoryWithChildren(int id)
        {
            var result = new HashSet<int> { id };
            bool added = true;
            while (added)
            {
                added = false;
                foreach (ArtLibraryCategory category in Categories)
                {
                    if (category.ParentId != null && result.Contains(category.ParentId.Value) && result.Add(category.Id))
                    {
                        added = true;
                    }
                }
            }

            return result;
        }

        /// <summary>Categories sorted by their full path (for combo boxes).</summary>
        public List<ArtLibraryCategory> GetCategoriesSorted()
        {
            return Categories.OrderBy(c => GetCategoryPath(c.Id), StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>
        /// Can <paramref name="categoryId"/> get <paramref name="newParentId"/> as parent without creating a loop?
        /// </summary>
        public bool CanSetParent(int categoryId, int? newParentId)
        {
            return newParentId == null || !GetCategoryWithChildren(categoryId).Contains(newParentId.Value);
        }

        public void SetCategoryParent(int categoryId, int? parentId)
        {
            ArtLibraryCategory category = GetCategory(categoryId);
            if (category == null || !CanSetParent(categoryId, parentId))
            {
                return;
            }

            category.ParentId = parentId;
            IsDirty = true;
        }

        #endregion

        #region [ Tags ]

        public static List<string> NormalizeTags(IEnumerable<string> tags)
        {
            return (tags ?? Enumerable.Empty<string>())
                .Select(t => (t ?? string.Empty).Trim())
                .Where(t => t.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Splits "holz, tisch; mittelalter" into tags.</summary>
        public static List<string> ParseTags(string text)
        {
            return NormalizeTags((text ?? string.Empty).Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public void AddTag(string tag)
        {
            tag = (tag ?? string.Empty).Trim();
            if (tag.Length == 0 || Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            Tags.Add(tag);
            Tags.Sort(StringComparer.CurrentCultureIgnoreCase);
            IsDirty = true;
        }

        public void RenameTag(string oldTag, string newTag)
        {
            newTag = (newTag ?? string.Empty).Trim();
            if (newTag.Length == 0)
            {
                return;
            }

            Tags.RemoveAll(t => string.Equals(t, oldTag, StringComparison.OrdinalIgnoreCase));
            AddTag(newTag);

            foreach (ArtLibraryItemInfo info in Items.Values)
            {
                int index = info.Tags.FindIndex(t => string.Equals(t, oldTag, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    info.Tags[index] = newTag;
                    info.Tags = NormalizeTags(info.Tags);
                }
            }

            IsDirty = true;
        }

        /// <summary>Removes the tag from the tag list and from all graphics.</summary>
        public void DeleteTag(string tag)
        {
            Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            foreach (ArtLibraryItemInfo info in Items.Values)
            {
                info.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            }

            IsDirty = true;
        }

        #endregion

        #region [ Search ]

        /// <summary>
        /// True when <paramref name="id"/> matches the search text: the id itself (decimal or hex)
        /// or the text contained in name, description, tags or category.
        /// </summary>
        public bool Matches(int id, string text, int? parsedId)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            if (parsedId != null && parsedId.Value == id)
            {
                return true;
            }

            ArtLibraryItemInfo info = Get(id);
            if (info == null)
            {
                return false;
            }

            return Contains(info.Name, text)
                   || Contains(info.Description, text)
                   || info.Tags.Any(t => Contains(t, text))
                   || Contains(GetCategoryPath(info.CategoryId), text);
        }

        private static bool Contains(string value, string text)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        #endregion
    }
}

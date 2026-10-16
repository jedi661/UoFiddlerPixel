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

namespace UoFiddler.Controls.GumpLibrary.Metadata
{
    /// <summary>
    /// Names, descriptions, categories, tags and sort order of a gump library (Name.gumplib.json).
    /// </summary>
    public sealed class GumpLibraryMetadata
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string _path;

        public int Version { get; set; } = 1;

        public List<GumpLibraryCategory> Categories { get; set; } = new List<GumpLibraryCategory>();

        /// <summary>All known tags (also tags not used by any gump yet).</summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>Metadata per library id.</summary>
        public Dictionary<int, GumpLibraryItemInfo> Items { get; set; } = new Dictionary<int, GumpLibraryItemInfo>();

        [JsonIgnore]
        public bool IsDirty { get; private set; }

        #region [ Load / Save ]

        public static GumpLibraryMetadata Load(string path)
        {
            GumpLibraryMetadata metadata = null;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                metadata = JsonSerializer.Deserialize<GumpLibraryMetadata>(File.ReadAllText(path), _jsonOptions);
            }

            metadata ??= new GumpLibraryMetadata();
            metadata._path = path;
            metadata.Categories ??= new List<GumpLibraryCategory>();
            metadata.Tags ??= new List<string>();
            metadata.Items ??= new Dictionary<int, GumpLibraryItemInfo>();
            foreach (int id in metadata.Items.Where(p => p.Value == null).Select(p => p.Key).ToList())
            {
                metadata.Items.Remove(id);
            }

            HashSet<int> known = metadata.Categories.Select(c => c.Id).ToHashSet();
            foreach (GumpLibraryItemInfo info in metadata.Items.Values)
            {
                info.Tags = NormalizeTags(info.Tags);
                info.CategoryIds = (info.CategoryIds ?? new List<int>()).Where(known.Contains).Distinct().ToList();
                info.Name ??= string.Empty;
                info.Description ??= string.Empty;
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

        public void MarkDirty() => IsDirty = true;

        /// <summary>Forgets unsaved changes (e.g. before reloading from disk).</summary>
        public void AcceptChanges() => IsDirty = false;

        #endregion

        #region [ Items ]

        public GumpLibraryItemInfo Get(int id) => Items.TryGetValue(id, out GumpLibraryItemInfo info) ? info : null;

        public GumpLibraryItemInfo GetOrCreate(int id)
        {
            if (!Items.TryGetValue(id, out GumpLibraryItemInfo info))
            {
                info = new GumpLibraryItemInfo { SortOrder = NextSortOrder() };
                Items[id] = info;
                IsDirty = true;
            }

            return info;
        }

        public void Set(int id, GumpLibraryItemInfo info)
        {
            if (info == null)
            {
                Remove(id);
                return;
            }

            info.Modified = DateTime.Now;
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

        public void Copy(int sourceId, int targetId)
        {
            GumpLibraryItemInfo info = Get(sourceId);
            if (info == null)
            {
                Remove(targetId);
                return;
            }

            GumpLibraryItemInfo clone = info.Clone();
            clone.Added = DateTime.Now;
            clone.Modified = DateTime.Now;
            clone.SortOrder = NextSortOrder();
            Items[targetId] = clone;
            IsDirty = true;
        }

        public void Move(int sourceId, int targetId)
        {
            GumpLibraryItemInfo info = Get(sourceId);
            Items.Remove(sourceId);
            if (info == null)
            {
                Items.Remove(targetId);
            }
            else
            {
                Items[targetId] = info;
            }

            IsDirty = true;
        }

        public void Swap(int firstId, int secondId)
        {
            GumpLibraryItemInfo first = Get(firstId);
            GumpLibraryItemInfo second = Get(secondId);
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

        #region [ Sort order ]

        public int NextSortOrder() => Items.Count == 0 ? 10 : Items.Values.Max(i => i.SortOrder) + 10;

        /// <summary>
        /// Gives the ids the sort orders 10, 20, 30 ... in the given sequence (used after moving
        /// entries in the mode "Eigene Reihenfolge").
        /// </summary>
        public void ApplyOrder(IEnumerable<int> orderedIds)
        {
            int order = 10;
            foreach (int id in orderedIds)
            {
                GetOrCreate(id).SortOrder = order;
                order += 10;
            }

            IsDirty = true;
        }

        #endregion

        #region [ Categories ]

        public GumpLibraryCategory GetCategory(int? id) => id == null ? null : Categories.FirstOrDefault(c => c.Id == id.Value);

        public GumpLibraryCategory AddCategory(string name, int? parentId = null)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0)
            {
                throw new ArgumentException("Der Name der Kategorie darf nicht leer sein.", nameof(name));
            }

            if (parentId != null && GetCategory(parentId) == null)
            {
                parentId = null;
            }

            var category = new GumpLibraryCategory
            {
                Id = Categories.Count == 0 ? 1 : Categories.Max(c => c.Id) + 1,
                Name = name,
                ParentId = parentId,
                SortOrder = Categories.Where(c => c.ParentId == parentId).Select(c => c.SortOrder).DefaultIfEmpty(0).Max() + 10
            };
            Categories.Add(category);
            IsDirty = true;
            return category;
        }

        public void RenameCategory(int id, string name)
        {
            GumpLibraryCategory category = GetCategory(id);
            name = (name ?? string.Empty).Trim();
            if (category != null && name.Length > 0)
            {
                category.Name = name;
                IsDirty = true;
            }
        }

        /// <summary>Deletes the category; sub categories move to its parent, gumps lose the assignment.</summary>
        public void DeleteCategory(int id)
        {
            GumpLibraryCategory category = GetCategory(id);
            if (category == null)
            {
                return;
            }

            foreach (GumpLibraryCategory child in Categories.Where(c => c.ParentId == id))
            {
                child.ParentId = category.ParentId;
            }

            Categories.Remove(category);
            foreach (GumpLibraryItemInfo info in Items.Values)
            {
                info.CategoryIds.Remove(id);
            }

            IsDirty = true;
        }

        public bool CanSetParent(int categoryId, int? newParentId)
        {
            return newParentId == null || (newParentId.Value != categoryId && !GetCategoryWithChildren(categoryId).Contains(newParentId.Value));
        }

        public void SetCategoryParent(int categoryId, int? parentId)
        {
            GumpLibraryCategory category = GetCategory(categoryId);
            if (category == null || !CanSetParent(categoryId, parentId))
            {
                return;
            }

            category.ParentId = parentId;
            IsDirty = true;
        }

        /// <summary>Moves a category one place up or down among its siblings.</summary>
        public void MoveCategory(int id, int direction)
        {
            GumpLibraryCategory category = GetCategory(id);
            if (category == null)
            {
                return;
            }

            List<GumpLibraryCategory> siblings = GetChildren(category.ParentId);
            int index = siblings.IndexOf(category);
            int target = index + Math.Sign(direction);
            if (index < 0 || target < 0 || target >= siblings.Count)
            {
                return;
            }

            siblings.RemoveAt(index);
            siblings.Insert(target, category);
            for (int i = 0; i < siblings.Count; ++i)
            {
                siblings[i].SortOrder = (i + 1) * 10;
            }

            IsDirty = true;
        }

        /// <summary>"Kleidung/Hemden".</summary>
        public string GetCategoryPath(int id)
        {
            var names = new List<string>();
            var seen = new HashSet<int>();
            GumpLibraryCategory category = GetCategory(id);
            while (category != null && seen.Add(category.Id))
            {
                names.Insert(0, category.Name);
                category = GetCategory(category.ParentId);
            }

            return string.Join("/", names);
        }

        public HashSet<int> GetCategoryWithChildren(int id)
        {
            var result = new HashSet<int> { id };
            bool added = true;
            while (added)
            {
                added = false;
                foreach (GumpLibraryCategory category in Categories)
                {
                    if (category.ParentId != null && result.Contains(category.ParentId.Value) && result.Add(category.Id))
                    {
                        added = true;
                    }
                }
            }

            return result;
        }

        public List<GumpLibraryCategory> GetChildren(int? parentId)
        {
            return Categories.Where(c => c.ParentId == parentId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>All categories in tree order with their depth (for lists and menus).</summary>
        public List<(GumpLibraryCategory Category, int Depth)> GetCategoryTree()
        {
            var result = new List<(GumpLibraryCategory, int)>();
            var seen = new HashSet<int>();

            void Add(int? parentId, int depth)
            {
                foreach (GumpLibraryCategory category in GetChildren(parentId))
                {
                    if (seen.Add(category.Id))
                    {
                        result.Add((category, depth));
                        Add(category.Id, depth + 1);
                    }
                }
            }

            Add(null, 0);
            // categories with a broken parent chain
            foreach (GumpLibraryCategory category in Categories.Where(c => !seen.Contains(c.Id)))
            {
                result.Add((category, 0));
            }

            return result;
        }

        /// <summary>Number of gumps per category (without sub categories).</summary>
        public Dictionary<int, int> CountPerCategory(IEnumerable<int> occupiedIds)
        {
            var counts = Categories.ToDictionary(c => c.Id, _ => 0);
            foreach (int id in occupiedIds)
            {
                GumpLibraryItemInfo info = Get(id);
                if (info == null)
                {
                    continue;
                }

                foreach (int categoryId in info.CategoryIds)
                {
                    if (counts.ContainsKey(categoryId))
                    {
                        counts[categoryId]++;
                    }
                }
            }

            return counts;
        }

        public void AssignCategory(IEnumerable<int> ids, int categoryId, bool assign)
        {
            if (GetCategory(categoryId) == null)
            {
                return;
            }

            foreach (int id in ids)
            {
                GumpLibraryItemInfo info = GetOrCreate(id);
                if (assign && !info.CategoryIds.Contains(categoryId))
                {
                    info.CategoryIds.Add(categoryId);
                    info.Modified = DateTime.Now;
                }
                else if (!assign && info.CategoryIds.Remove(categoryId))
                {
                    info.Modified = DateTime.Now;
                }
            }

            IsDirty = true;
        }

        public string GetCategoryText(GumpLibraryItemInfo info)
        {
            if (info == null || info.CategoryIds.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", info.CategoryIds.Select(GetCategoryPath).Where(p => p.Length > 0));
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

        /// <summary>Splits "hemd, rot; leinen" into tags.</summary>
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
            foreach (GumpLibraryItemInfo info in Items.Values)
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

        public void DeleteTag(string tag)
        {
            Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            foreach (GumpLibraryItemInfo info in Items.Values)
            {
                info.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
            }

            IsDirty = true;
        }

        /// <summary>Tags of the tag list plus every tag used by a gump, sorted.</summary>
        public List<string> GetAllTags()
        {
            return NormalizeTags(Tags.Concat(Items.Values.SelectMany(i => i.Tags)))
                .OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public void SetTags(int id, IEnumerable<string> tags)
        {
            GumpLibraryItemInfo info = GetOrCreate(id);
            info.Tags = NormalizeTags(tags);
            foreach (string tag in info.Tags)
            {
                AddTag(tag);
            }

            info.Modified = DateTime.Now;
            IsDirty = true;
        }

        #endregion
    }
}

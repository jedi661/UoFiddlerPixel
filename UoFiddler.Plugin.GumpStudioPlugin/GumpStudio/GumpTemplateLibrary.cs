using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.DesignStudio.GumpStudio
{
    /// <summary>One template: a built-in one or a *.gump.json file in a category folder.</summary>
    public sealed class GumpTemplateEntry
    {
        public string Name { get; set; }

        public string Category { get; set; }

        /// <summary>Full path of the file; null for built-in templates.</summary>
        public string File { get; set; }

        public bool BuiltIn => File == null;

        public Func<GumpDesign> Factory { get; set; }

        public string Description { get; set; } = string.Empty;

        public int ElementCount { get; set; }

        public int Pages { get; set; } = 1;

        public DateTime Changed { get; set; }

        public GumpDesign Load() => BuiltIn ? Factory() : GumpDesign.FromJson(System.IO.File.ReadAllText(File));

        public override string ToString() => Category + " / " + Name;
    }

    /// <summary>
    /// Your own gump templates, sorted in categories:
    /// GumpTemplates\&lt;Category&gt;\&lt;Name&gt;.gump.json next to UoFiddler. Any Gump Studio project file can be copied there.
    /// </summary>
    public static class GumpTemplateLibrary
    {
        public const string BuiltInCategory = "Built-in";
        public const string DefaultCategory = "General";
        public const string Extension = ".gump.json";

        public static string Folder { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GumpTemplates");

        public static IEnumerable<GumpTemplateEntry> BuiltIn()
        {
            foreach ((string name, Func<GumpDesign> create) in GumpTemplates.All)
            {
                GumpDesign d = create();
                yield return new GumpTemplateEntry
                {
                    Name = name, Category = BuiltInCategory, Factory = create, ElementCount = d.Elements.Count, Pages = d.Pages,
                    Description = "Built-in template. / Mitgelieferte Vorlage."
                };
            }
        }

        /// <summary>All user templates (built-in ones not included). Broken files are reported in errors.</summary>
        public static List<GumpTemplateEntry> LoadUser(List<string> errors)
        {
            var list = new List<GumpTemplateEntry>();
            if (!Directory.Exists(Folder))
            {
                return list;
            }
            foreach (string file in Directory.EnumerateFiles(Folder, "*" + Extension, SearchOption.AllDirectories))
            {
                try
                {
                    GumpDesign d = GumpDesign.FromJson(System.IO.File.ReadAllText(file));
                    list.Add(new GumpTemplateEntry
                    {
                        Name = NameOf(file),
                        Category = CategoryOf(file),
                        File = file,
                        Description = d.Description ?? string.Empty,
                        ElementCount = d.Elements.Count,
                        Pages = d.Pages,
                        Changed = System.IO.File.GetLastWriteTime(file)
                    });
                }
                catch (Exception ex)
                {
                    errors?.Add(Path.GetFileName(file) + ": " + ex.Message);
                }
            }
            return list.OrderBy(e => e.Category, StringComparer.CurrentCultureIgnoreCase).ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        /// <summary>Category folders (also empty ones), sorted.</summary>
        public static List<string> Categories()
        {
            if (!Directory.Exists(Folder))
            {
                return new List<string>();
            }
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string dir in Directory.EnumerateDirectories(Folder, "*", SearchOption.AllDirectories))
            {
                set.Add(Relative(dir));
            }
            if (Directory.EnumerateFiles(Folder, "*" + Extension).Any())
            {
                set.Add(DefaultCategory);
            }
            return set.OrderBy(c => c, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        private static string Relative(string dir) => dir.Substring(Folder.Length).Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');

        private static string NameOf(string file)
        {
            string n = Path.GetFileName(file);
            return n.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ? n.Substring(0, n.Length - Extension.Length) : Path.GetFileNameWithoutExtension(n);
        }

        private static string CategoryOf(string file)
        {
            string rel = Relative(Path.GetDirectoryName(file));
            return rel.Length == 0 ? DefaultCategory : rel;
        }

        /// <summary>Removes characters that are not allowed in file names; "/" in categories makes sub folders.</summary>
        public static string Safe(string text, bool allowSlash = false)
        {
            var sb = new StringBuilder();
            foreach (char c in (text ?? string.Empty).Trim())
            {
                if (allowSlash && (c == '/' || c == '\\'))
                {
                    sb.Append('/');
                }
                else if (Path.GetInvalidFileNameChars().Contains(c) || ":*?\"<>|".IndexOf(c) >= 0)
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }
            string s = string.Join("/", sb.ToString().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().Trim('.')).Where(p => p.Length > 0));
            return s;
        }

        public static string CategoryFolder(string category)
        {
            category = Safe(category, true);
            if (category.Length == 0)
            {
                category = DefaultCategory;
            }
            return Path.Combine(Folder, category.Replace('/', Path.DirectorySeparatorChar));
        }

        public static string PathFor(string name, string category) => Path.Combine(CategoryFolder(category), Safe(name) + Extension);

        public static bool Exists(string name, string category) => System.IO.File.Exists(PathFor(name, category));

        /// <summary>Saves the gump as template (overwrites a template with the same name in the category).</summary>
        public static string Save(GumpDesign design, string name, string category, string description)
        {
            if (Safe(name).Length == 0)
            {
                throw new ArgumentException("The template needs a name.");
            }
            GumpDesign copy = design.Clone();
            copy.Description = description ?? string.Empty;
            string file = PathFor(name, category);
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            System.IO.File.WriteAllText(file, copy.ToJson(), new UTF8Encoding(false));
            return file;
        }

        public static void CreateCategory(string category) => Directory.CreateDirectory(CategoryFolder(category));

        /// <summary>Deletes an empty category folder; returns false if it still holds templates.</summary>
        public static bool DeleteCategory(string category)
        {
            string dir = CategoryFolder(category);
            if (!Directory.Exists(dir))
            {
                return true;
            }
            if (Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Any())
            {
                return false;
            }
            Directory.Delete(dir, true);
            return true;
        }

        public static void Delete(GumpTemplateEntry entry)
        {
            if (!entry.BuiltIn && System.IO.File.Exists(entry.File))
            {
                System.IO.File.Delete(entry.File);
            }
        }

        /// <summary>Renames and / or moves a template; returns the new path.</summary>
        public static string Move(GumpTemplateEntry entry, string newName, string newCategory)
        {
            if (entry.BuiltIn)
            {
                throw new InvalidOperationException("Built-in templates cannot be changed – save a copy instead.");
            }
            string target = PathFor(newName, newCategory);
            if (string.Equals(Path.GetFullPath(target), Path.GetFullPath(entry.File), StringComparison.OrdinalIgnoreCase))
            {
                if (target != entry.File)
                {
                    // only upper / lower case changed
                    string tmp = entry.File + ".tmp";
                    System.IO.File.Move(entry.File, tmp);
                    System.IO.File.Move(tmp, target);
                }
                return target;
            }
            if (System.IO.File.Exists(target))
            {
                throw new IOException($"\"{newName}\" already exists in {newCategory}.");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            System.IO.File.Move(entry.File, target);
            return target;
        }

        /// <summary>Changes the note of a template file.</summary>
        public static void SetDescription(GumpTemplateEntry entry, string description)
        {
            GumpDesign d = entry.Load();
            d.Description = description ?? string.Empty;
            System.IO.File.WriteAllText(entry.File, d.ToJson(), new UTF8Encoding(false));
            entry.Description = d.Description;
        }

        /// <summary>
        /// Copies gump files into a category: Gump Studio projects (*.gump.json) directly, ServUO scripts (*.cs) are imported first.
        /// Returns the number of templates added.
        /// </summary>
        public static int ImportFiles(IEnumerable<string> files, string category, List<string> messages)
        {
            int n = 0;
            foreach (string f in files)
            {
                try
                {
                    GumpDesign d;
                    string name;
                    if (f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        string code = System.IO.File.ReadAllText(f);
                        List<string> classes = GumpImporter.FindGumpClasses(code);
                        if (classes.Count == 0)
                        {
                            classes.Add(null);
                        }
                        foreach (string cls in classes)
                        {
                            d = GumpImporter.Import(code, null, cls, GumpImporter.FolderResolver(f));
                            if (d.Elements.Count == 0)
                            {
                                continue;
                            }
                            name = cls ?? NameOf(f);
                            d.Description = "Imported from " + Path.GetFileName(f);
                            Save(d, Unique(name, category), category, d.Description);
                            n++;
                        }
                        continue;
                    }
                    d = GumpDesign.FromJson(System.IO.File.ReadAllText(f));
                    name = NameOf(f);
                    Save(d, Unique(name, category), category, d.Description);
                    n++;
                }
                catch (Exception ex)
                {
                    messages?.Add(Path.GetFileName(f) + ": " + ex.Message);
                }
            }
            return n;
        }

        /// <summary>Name that is not used yet in the category (Name, Name (2), Name (3) …).</summary>
        public static string Unique(string name, string category)
        {
            string n = Safe(name);
            if (n.Length == 0)
            {
                n = "Template";
            }
            if (!Exists(n, category))
            {
                return n;
            }
            for (int i = 2; ; i++)
            {
                string c = $"{n} ({i})";
                if (!Exists(c, category))
                {
                    return c;
                }
            }
        }

        /// <summary>
        /// Elements of a template prepared for inserting into another gump: moved to (x, y) and page,
        /// with button / switch / entry IDs moved behind the ones already used.
        /// </summary>
        public static List<GumpElement> ForInsert(GumpDesign template, GumpDesign target, int x, int y, int page)
        {
            List<GumpElement> els = template.Clone().Elements;
            if (els.Count == 0)
            {
                return els;
            }
            int minX = els.Min(e => e.X), minY = els.Min(e => e.Y);
            int maxButton = target.Elements.Where(e => (e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply).Select(e => e.ButtonId).DefaultIfEmpty(0).Max();
            int maxSwitch = target.Elements.Where(e => e.Type == ElementType.Check || e.Type == ElementType.Radio).Select(e => e.SwitchId).DefaultIfEmpty(0).Max();
            int maxEntry = target.Elements.Where(e => e.Type == ElementType.TextEntry).Select(e => e.EntryId).DefaultIfEmpty(0).Max();
            bool targetHasRadios = target.Elements.Any(e => e.Type == ElementType.Radio);
            int maxGroup = target.Elements.Where(e => e.Type == ElementType.Radio).Select(e => e.Group).DefaultIfEmpty(0).Max();
            bool multiPage = els.Any(e => e.Page > 1);
            foreach (GumpElement e in els)
            {
                e.X += x - minX;
                e.Y += y - minY;
                if (!multiPage)
                {
                    e.Page = e.Page == 0 && page == 0 ? 0 : page;
                }
                if ((e.Type == ElementType.Button || e.Type == ElementType.ItemButton) && e.ButtonType == ButtonKind.Reply && e.ButtonId > 0)
                {
                    e.ButtonId += maxButton;
                }
                if (e.Type == ElementType.Check || e.Type == ElementType.Radio)
                {
                    e.SwitchId += maxSwitch;
                }
                if (e.Type == ElementType.Radio && targetHasRadios)
                {
                    e.Group += maxGroup + 1;
                }
                if (e.Type == ElementType.TextEntry)
                {
                    e.EntryId += maxEntry;
                }
                e.Locked = false;
                e.Hidden = false;
            }
            return els;
        }
    }
}

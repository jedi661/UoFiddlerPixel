/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Xml;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// One image in the "Load.." list of the MultiMap tab.
    /// </summary>
    public sealed class MapImageEntry
    {
        public string Name { get; set; }

        /// <summary>Full path of the image file</summary>
        public string FilePath { get; set; }

        /// <summary>True if the file lies in the library folder of the app (false = file in an external folder)</summary>
        public bool IsInternal { get; set; }

        public DateTime Added { get; set; } = DateTime.Now;

        public bool Exists => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Library of the generated / added map images.
    /// Internal images are stored in <c>AppData\UoFiddler\MultiMapImages</c>, external images are only referenced.
    /// The list is saved in <c>MultiMapImages.xml</c> in the same folder.
    /// </summary>
    public static class MapImageLibrary
    {
        public const string FolderName = "MultiMapImages";
        private const string IndexFileName = "MultiMapImages.xml";

        private static readonly List<MapImageEntry> _entries = new List<MapImageEntry>();
        private static bool _loaded;

        /// <summary>Raised after an entry was added, removed or changed</summary>
        public static event EventHandler Changed;

        /// <summary>Folder of the images stored inside the app</summary>
        public static string Folder
        {
            get
            {
                string folder = Path.Combine(Options.AppDataPath ?? AppDomain.CurrentDomain.BaseDirectory, FolderName);
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        private static string IndexFile => Path.Combine(Folder, IndexFileName);

        private static string _lastExternalFolder;

        /// <summary>Last folder used for saving outside the app (stored in the list file)</summary>
        public static string LastExternalFolder
        {
            get
            {
                EnsureLoaded();
                return string.IsNullOrEmpty(_lastExternalFolder) ? Options.OutputPath : _lastExternalFolder;
            }
            set
            {
                EnsureLoaded();
                _lastExternalFolder = value;
                Save();
            }
        }

        public static IReadOnlyList<MapImageEntry> Entries
        {
            get
            {
                EnsureLoaded();
                return _entries;
            }
        }

        #region Formats

        /// <summary>File filter for open dialogs (all image formats GDI+ can read)</summary>
        public const string OpenFilter =
            "All images|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.ico;*.emf;*.wmf;*.exif|" +
            "PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|Bitmap (*.bmp)|*.bmp|GIF (*.gif)|*.gif|" +
            "TIFF (*.tif;*.tiff)|*.tif;*.tiff|Icon (*.ico)|*.ico|Metafile (*.emf;*.wmf)|*.emf;*.wmf|All files (*.*)|*.*";

        /// <summary>File filter for save dialogs. The index matches <see cref="FormatFromFilterIndex"/>.</summary>
        public const string SaveFilter =
            "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff|Icon (*.ico)|*.ico";

        public static ImageFormat FormatFromFilterIndex(int filterIndex)
        {
            switch (filterIndex)
            {
                case 2: return ImageFormat.Jpeg;
                case 3: return ImageFormat.Bmp;
                case 4: return ImageFormat.Gif;
                case 5: return ImageFormat.Tiff;
                case 6: return ImageFormat.Icon;
                default: return ImageFormat.Png;
            }
        }

        /// <summary>Image format from the file extension (png if unknown)</summary>
        public static ImageFormat FormatFromExtension(string fileName)
        {
            switch (Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".gif":
                    return ImageFormat.Gif;
                case ".tif":
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".ico":
                    return ImageFormat.Icon;
                default:
                    return ImageFormat.Png;
            }
        }

        public static string ExtensionFor(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Jpeg))
            {
                return "jpg";
            }

            if (format.Equals(ImageFormat.Bmp))
            {
                return "bmp";
            }

            if (format.Equals(ImageFormat.Gif))
            {
                return "gif";
            }

            if (format.Equals(ImageFormat.Tiff))
            {
                return "tiff";
            }

            if (format.Equals(ImageFormat.Icon))
            {
                return "ico";
            }

            return "png";
        }

        /// <summary>
        /// Loads an image into memory without keeping the file locked (the file can be overwritten later).
        /// Always returns a 32bpp bitmap that can be drawn on.
        /// </summary>
        public static Bitmap LoadBitmap(string fileName)
        {
            byte[] data = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream(data))
            using (Image img = Image.FromStream(ms))
            {
                var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                bmp.SetResolution(96, 96);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
                }

                return bmp;
            }
        }

        /// <summary>
        /// Saves an image in the format of the file extension. JPEG with the given quality,
        /// icons are scaled to 256x256 (largest icon size).
        /// </summary>
        public static void SaveImage(Image image, string fileName, ImageFormat format = null, long jpegQuality = 95)
        {
            format ??= FormatFromExtension(fileName);
            string dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (format.Equals(ImageFormat.Jpeg))
            {
                ImageCodecInfo codec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                if (codec != null)
                {
                    using (var parameters = new EncoderParameters(1))
                    {
                        parameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
                        image.Save(fileName, codec, parameters);
                    }

                    return;
                }
            }

            if (format.Equals(ImageFormat.Icon))
            {
                SaveIcon(image, fileName);
                return;
            }

            image.Save(fileName, format);
        }

        /// <summary>Writes a .ico file with one 256x256 PNG compressed image</summary>
        private static void SaveIcon(Image image, string fileName)
        {
            const int size = 256;
            byte[] png;
            using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    float scale = Math.Min((float)size / image.Width, (float)size / image.Height);
                    float w = image.Width * scale, h = image.Height * scale;
                    g.DrawImage(image, (size - w) / 2f, (size - h) / 2f, w, h);
                }

                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    png = ms.ToArray();
                }
            }

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var w = new BinaryWriter(fs))
            {
                w.Write((short)0); // reserved
                w.Write((short)1); // type icon
                w.Write((short)1); // count
                w.Write((byte)0); // width 256
                w.Write((byte)0); // height 256
                w.Write((byte)0); // colors
                w.Write((byte)0); // reserved
                w.Write((short)1); // planes
                w.Write((short)32); // bit count
                w.Write(png.Length);
                w.Write(6 + 16); // offset
                w.Write(png);
            }
        }

        #endregion

        #region List

        private static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            _entries.Clear();
            try
            {
                if (!File.Exists(IndexFile))
                {
                    ImportUnlistedFiles();
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(IndexFile);
                _lastExternalFolder = doc.DocumentElement?.GetAttribute("lastFolder");
                foreach (XmlElement e in doc.SelectNodes("/MultiMapImages/Image").OfType<XmlElement>())
                {
                    var entry = new MapImageEntry
                    {
                        Name = e.GetAttribute("name"),
                        FilePath = e.GetAttribute("path"),
                        IsInternal = e.GetAttribute("internal") == "1"
                    };

                    if (entry.IsInternal && !Path.IsPathRooted(entry.FilePath))
                    {
                        entry.FilePath = Path.Combine(Folder, entry.FilePath);
                    }

                    if (DateTime.TryParse(e.GetAttribute("added"), System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out DateTime added))
                    {
                        entry.Added = added;
                    }

                    if (!string.IsNullOrWhiteSpace(entry.FilePath))
                    {
                        _entries.Add(entry);
                    }
                }

                ImportUnlistedFiles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MapImageLibrary: {ex.Message}");
            }
        }

        /// <summary>Images that were copied into the folder by hand are added to the list too</summary>
        private static void ImportUnlistedFiles()
        {
            string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff" };
            foreach (string file in Directory.GetFiles(Folder))
            {
                if (!extensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                {
                    continue;
                }

                if (_entries.Any(e => SamePath(e.FilePath, file)))
                {
                    continue;
                }

                _entries.Add(new MapImageEntry
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    FilePath = file,
                    IsInternal = true,
                    Added = File.GetCreationTime(file)
                });
            }
        }

        public static void Save()
        {
            EnsureLoaded();
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = doc.CreateElement("MultiMapImages");
            if (!string.IsNullOrEmpty(_lastExternalFolder))
            {
                root.SetAttribute("lastFolder", _lastExternalFolder);
            }

            doc.AppendChild(root);
            foreach (MapImageEntry entry in _entries)
            {
                XmlElement e = doc.CreateElement("Image");
                e.SetAttribute("name", entry.Name ?? string.Empty);
                e.SetAttribute("path", entry.IsInternal && IsInFolder(entry.FilePath) ? Path.GetFileName(entry.FilePath) : entry.FilePath);
                e.SetAttribute("internal", entry.IsInternal ? "1" : "0");
                e.SetAttribute("added", entry.Added.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
                root.AppendChild(e);
            }

            doc.Save(IndexFile);
        }

        private static bool IsInFolder(string file)
        {
            return !string.IsNullOrEmpty(file) &&
                   SamePath(Path.GetDirectoryName(Path.GetFullPath(file)), Folder);
        }

        private static bool SamePath(string a, string b)
        {
            try
            {
                return string.Equals(Path.GetFullPath(a).TrimEnd('\\', '/'), Path.GetFullPath(b).TrimEnd('\\', '/'),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static MapImageEntry FindByPath(string file)
        {
            return Entries.FirstOrDefault(e => SamePath(e.FilePath, file));
        }

        /// <summary>Removes characters that are not allowed in file names</summary>
        public static string SafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "MapImage";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }

        /// <summary>A file name in the library folder that is not used yet</summary>
        public static string UniqueInternalPath(string name, string extension)
        {
            string baseName = SafeFileName(name);
            string file = Path.Combine(Folder, $"{baseName}.{extension}");
            int i = 2;
            while (File.Exists(file))
            {
                file = Path.Combine(Folder, $"{baseName}_{i++}.{extension}");
            }

            return file;
        }

        /// <summary>
        /// Stores the image inside the app (library folder) and adds it to the list.
        /// </summary>
        public static MapImageEntry AddInternal(Image image, string name, ImageFormat format = null)
        {
            EnsureLoaded();
            format ??= ImageFormat.Png;
            string file = UniqueInternalPath(name, ExtensionFor(format));
            SaveImage(image, file, format);
            var entry = new MapImageEntry { Name = UniqueName(name), FilePath = file, IsInternal = true };
            _entries.Add(entry);
            Save();
            OnChanged();
            return entry;
        }

        /// <summary>
        /// Adds an image file of any folder to the list (the file is only referenced).
        /// When <paramref name="copyToLibrary"/> is true, the file is copied into the app.
        /// </summary>
        public static MapImageEntry AddFile(string fileName, string name = null, bool copyToLibrary = false)
        {
            EnsureLoaded();
            MapImageEntry existing = FindByPath(fileName);
            if (existing != null)
            {
                return existing;
            }

            name = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(fileName) : name;
            if (copyToLibrary)
            {
                string ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
                if (ext == "ico" || ext == "emf" || ext == "wmf" || string.IsNullOrEmpty(ext))
                {
                    using (Bitmap bmp = LoadBitmap(fileName))
                    {
                        return AddInternal(bmp, name);
                    }
                }

                string target = UniqueInternalPath(name, ext);
                File.Copy(fileName, target);
                var copied = new MapImageEntry { Name = UniqueName(name), FilePath = target, IsInternal = true };
                _entries.Add(copied);
                Save();
                OnChanged();
                return copied;
            }

            var entry = new MapImageEntry { Name = UniqueName(name), FilePath = fileName, IsInternal = false };
            _entries.Add(entry);
            Save();
            OnChanged();
            return entry;
        }

        /// <summary>Overwrites the file of an entry with the edited image</summary>
        public static void Update(MapImageEntry entry, Image image)
        {
            SaveImage(image, entry.FilePath);
            OnChanged();
        }

        public static void Rename(MapImageEntry entry, string newName)
        {
            if (entry == null || string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            entry.Name = newName.Trim();
            Save();
            OnChanged();
        }

        /// <summary>Removes the entry. Internal files are deleted when <paramref name="deleteFile"/> is true.</summary>
        public static void Remove(MapImageEntry entry, bool deleteFile)
        {
            EnsureLoaded();
            if (entry == null || !_entries.Remove(entry))
            {
                return;
            }

            if (deleteFile && entry.IsInternal && entry.Exists)
            {
                try
                {
                    File.Delete(entry.FilePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MapImageLibrary: {ex.Message}");
                }
            }

            Save();
            OnChanged();
        }

        private static string UniqueName(string name)
        {
            string result = name;
            int i = 2;
            while (_entries.Any(e => string.Equals(e.Name, result, StringComparison.OrdinalIgnoreCase)))
            {
                result = $"{name} ({i++})";
            }

            return result;
        }

        public static void OnChanged()
        {
            Changed?.Invoke(null, EventArgs.Empty);
        }

        #endregion
    }
}

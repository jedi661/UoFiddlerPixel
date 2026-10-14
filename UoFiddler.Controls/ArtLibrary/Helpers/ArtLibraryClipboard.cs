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
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Core;

namespace UoFiddler.Controls.ArtLibrary.Helpers
{
    /// <summary>
    /// One graphic on the clipboard: encoded data (art.mul item format) plus where it came from.
    /// </summary>
    public sealed class ArtClipboardItem
    {
        public int SourceId { get; set; }

        /// <summary>Encoded graphic (ArtLibraryCodec format) - lossless.</summary>
        public byte[] Data { get; set; }

        public string Name { get; set; } = string.Empty;

        public Bitmap ToBitmap() => ArtLibraryCodec.DecodeBitmap(Data);
    }

    /// <summary>
    /// Clipboard exchange. Copy puts two things on the clipboard:
    /// <list type="bullet">
    /// <item>a private format with all selected graphics (lossless, several at once, with their ids)</item>
    /// <item>a normal image of the first graphic (black background) - the items tab and other programs paste this</item>
    /// </list>
    /// Paste understands the private format, normal images (from the items tab, image editors, browsers)
    /// and copied image files from the Windows Explorer.
    /// </summary>
    public static class ArtLibraryClipboard
    {
        public const string FormatName = "UoFiddler.ArtLibrary.Items.v1";
        private const int Magic = 0x4C414655; // "UFAL"

        public static void Copy(IReadOnlyList<ArtClipboardItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetData(FormatName, false, Serialize(items));

            using (Bitmap first = items[0].ToBitmap())
            {
                if (first != null)
                {
                    // same as the items tab copy: transparent becomes black, which the items tab import turns back into transparent
                    data.SetData(DataFormats.Bitmap, true, ArtLibraryImageConverter.ToExportBitmap(first));
                }
            }

            Clipboard.SetDataObject(data, true);
        }

        /// <summary>True when the clipboard holds something that can be pasted as art.</summary>
        public static bool ContainsArt()
        {
            try
            {
                return Clipboard.ContainsData(FormatName)
                       || Clipboard.ContainsImage()
                       || Clipboard.ContainsData("PNG")
                       || GetImageFiles().Count > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Graphics in the private format, or an empty list.</summary>
        public static List<ArtClipboardItem> GetItems()
        {
            try
            {
                if (Clipboard.GetData(FormatName) is MemoryStream stream)
                {
                    return Deserialize(stream);
                }
            }
            catch
            {
                // foreign or damaged data
            }

            return new List<ArtClipboardItem>();
        }

        /// <summary>
        /// A normal clipboard image prepared for the art format, or null.
        /// PNG data (keeps real transparency) is preferred over the plain bitmap.
        /// </summary>
        public static Bitmap GetImage(ArtImportOptions options)
        {
            try
            {
                if (Clipboard.GetData("PNG") is MemoryStream png)
                {
                    using (Image image = Image.FromStream(png))
                    {
                        return ArtLibraryImageConverter.PrepareForArt(image, options);
                    }
                }

                if (Clipboard.ContainsImage())
                {
                    using (Image image = Clipboard.GetImage())
                    {
                        return image == null ? null : ArtLibraryImageConverter.PrepareForArt(image, options);
                    }
                }
            }
            catch
            {
                // unsupported image data
            }

            return null;
        }

        /// <summary>Image files copied in the Windows Explorer.</summary>
        public static List<string> GetImageFiles()
        {
            var result = new List<string>();
            if (!Clipboard.ContainsFileDropList())
            {
                return result;
            }

            StringCollection files = Clipboard.GetFileDropList();
            foreach (string file in files)
            {
                if (File.Exists(file) && ArtLibraryImageConverter.IsSupportedFile(file))
                {
                    result.Add(file);
                }
            }

            return result;
        }

        private static MemoryStream Serialize(IReadOnlyList<ArtClipboardItem> items)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write(items.Count);
                foreach (ArtClipboardItem item in items)
                {
                    writer.Write(item.SourceId);
                    writer.Write(item.Name ?? string.Empty);
                    writer.Write(item.Data.Length);
                    writer.Write(item.Data);
                }
            }

            stream.Position = 0;
            return stream;
        }

        private static List<ArtClipboardItem> Deserialize(Stream stream)
        {
            var result = new List<ArtClipboardItem>();
            stream.Position = 0;
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                if (reader.ReadInt32() != Magic)
                {
                    return result;
                }

                int count = reader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var item = new ArtClipboardItem
                    {
                        SourceId = reader.ReadInt32(),
                        Name = reader.ReadString()
                    };

                    int length = reader.ReadInt32();
                    item.Data = reader.ReadBytes(length);
                    if (item.Data.Length == length && ArtLibraryCodec.IsValid(item.Data))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        /// <summary>Clipboard item from a bitmap (encodes it).</summary>
        public static ArtClipboardItem CreateItem(int sourceId, Bitmap bitmap, string name)
        {
            return new ArtClipboardItem
            {
                SourceId = sourceId,
                Data = ArtLibraryCodec.Encode(bitmap),
                Name = name ?? string.Empty
            };
        }

        /// <summary>Distinct, ordered ids helper.</summary>
        public static List<int> Ordered(IEnumerable<int> ids) => ids.Distinct().OrderBy(id => id).ToList();
    }
}

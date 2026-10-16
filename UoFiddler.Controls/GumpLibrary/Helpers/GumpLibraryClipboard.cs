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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.GumpLibrary.Core;
using UoFiddler.Controls.GumpLibrary.Metadata;

namespace UoFiddler.Controls.GumpLibrary.Helpers
{
    /// <summary>One gump on the clipboard: graphic, own preview and metadata (lossless).</summary>
    public sealed class GumpClipboardItem
    {
        public int SourceId { get; set; }

        /// <summary>Gump in <see cref="GumpLibraryCodec"/> format.</summary>
        public byte[] Data { get; set; }

        /// <summary>Own preview in <see cref="GumpLibraryCodec"/> format or null.</summary>
        public byte[] Preview { get; set; }

        /// <summary>Metadata or null.</summary>
        public GumpLibraryItemInfo Info { get; set; }

        public Bitmap ToBitmap() => GumpLibraryCodec.DecodeBitmap(Data);

        public Bitmap PreviewToBitmap() => Preview == null ? null : GumpLibraryCodec.DecodeBitmap(Preview);
    }

    /// <summary>
    /// Clipboard exchange of the gump library. Copy puts a private format (all selected gumps with previews
    /// and metadata) and a normal image of the first gump on the clipboard. The image is made like the
    /// Gumps tab "Copy": transparent / #D3D3D3 become black, 24 bit - so the Gumps tab "Import" and image
    /// editors understand it. Paste understands the private format, normal images and copied image files.
    /// </summary>
    public static class GumpLibraryClipboard
    {
        public const string FormatName = "UoFiddler.GumpLibrary.Gumps.v1";
        private const int Magic = 0x4C424755; // "UGBL"

        public static void Copy(IReadOnlyList<GumpClipboardItem> items)
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
                    data.SetData(DataFormats.Bitmap, true, ToClipboardImage(first));
                }
            }

            Clipboard.SetDataObject(data, true);
        }

        /// <summary>Puts only an image (e.g. the preview) on the clipboard, Gumps tab style.</summary>
        public static void CopyImage(Bitmap bitmap)
        {
            if (bitmap != null)
            {
                Clipboard.SetImage(ToClipboardImage(bitmap));
            }
        }

        /// <summary>Same conversion as the Gumps tab copy: black background, 24 bit.</summary>
        public static Bitmap ToClipboardImage(Bitmap gump)
        {
            var result = new Bitmap(gump.Width, gump.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.Clear(Color.Black);
                g.DrawImageUnscaled(gump, 0, 0);
            }

            return result;
        }

        public static bool ContainsGumps()
        {
            try
            {
                return Clipboard.ContainsData(FormatName)
                       || Clipboard.ContainsImage()
                       || Clipboard.ContainsData("PNG")
                       || ArtLibraryClipboard.GetImageFiles().Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static List<GumpClipboardItem> GetItems()
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

            return new List<GumpClipboardItem>();
        }

        /// <summary>A normal clipboard image with the background colors made transparent, or null.</summary>
        public static Bitmap GetImage(ArtImportOptions options) => ArtLibraryClipboard.GetImage(options);

        public static List<string> GetImageFiles() => ArtLibraryClipboard.GetImageFiles();

        public static GumpClipboardItem CreateItem(GumpLibraryFile library, int id)
        {
            Bitmap gump = library.GetGump(id);
            if (gump == null)
            {
                return null;
            }

            Bitmap preview = library.GetPreview(id);
            return new GumpClipboardItem
            {
                SourceId = id,
                Data = GumpLibraryCodec.Encode(gump),
                Preview = preview == null ? null : GumpLibraryCodec.Encode(preview),
                Info = library.Metadata.Get(id)?.Clone()
            };
        }

        public static GumpClipboardItem CreateItem(int sourceId, Bitmap bitmap, string name)
        {
            return new GumpClipboardItem
            {
                SourceId = sourceId,
                Data = GumpLibraryCodec.Encode(bitmap),
                Info = string.IsNullOrEmpty(name) ? null : new GumpLibraryItemInfo { Name = name }
            };
        }

        private static MemoryStream Serialize(IReadOnlyList<GumpClipboardItem> items)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write(items.Count);
                foreach (GumpClipboardItem item in items)
                {
                    writer.Write(item.SourceId);
                    writer.Write(item.Data.Length);
                    writer.Write(item.Data);
                    writer.Write(item.Preview?.Length ?? 0);
                    if (item.Preview != null)
                    {
                        writer.Write(item.Preview);
                    }

                    writer.Write(item.Info == null ? string.Empty : JsonSerializer.Serialize(item.Info));
                }
            }

            stream.Position = 0;
            return stream;
        }

        private static List<GumpClipboardItem> Deserialize(Stream stream)
        {
            var result = new List<GumpClipboardItem>();
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
                    var item = new GumpClipboardItem { SourceId = reader.ReadInt32() };
                    item.Data = reader.ReadBytes(reader.ReadInt32());
                    int previewLength = reader.ReadInt32();
                    item.Preview = previewLength > 0 ? reader.ReadBytes(previewLength) : null;
                    string json = reader.ReadString();
                    if (json.Length > 0)
                    {
                        try
                        {
                            item.Info = JsonSerializer.Deserialize<GumpLibraryItemInfo>(json);
                        }
                        catch
                        {
                            item.Info = null;
                        }
                    }

                    if (GumpLibraryCodec.IsValid(item.Data))
                    {
                        if (item.Preview != null && !GumpLibraryCodec.IsValid(item.Preview))
                        {
                            item.Preview = null;
                        }

                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}

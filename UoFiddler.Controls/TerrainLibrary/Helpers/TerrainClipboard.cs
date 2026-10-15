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
using System.IO;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.ArtLibrary.Helpers;
using UoFiddler.Controls.TerrainLibrary.Core;

namespace UoFiddler.Controls.TerrainLibrary.Helpers
{
    /// <summary>One land tile or texture on the clipboard (lossless, with its slot id).</summary>
    public sealed class TerrainClipboardItem
    {
        public int SourceId { get; set; }

        public byte[] Data { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Clipboard exchange like the art library: a private format per layer (several graphics, lossless)
    /// plus a normal image of the first one, so the Land Tiles / Textures tab and other programs can paste it.
    /// Land tiles and textures are opaque, so no colors are made transparent when pasting.
    /// </summary>
    public static class TerrainClipboard
    {
        private const int Magic = 0x4C525455; // "UTRL"

        public static string FormatName(TerrainLayer layer) =>
            layer == TerrainLayer.LandTile ? "UoFiddler.TerrainLibrary.Land.v1" : "UoFiddler.TerrainLibrary.Texture.v1";

        public static void Copy(TerrainLayer layer, IReadOnlyList<TerrainClipboardItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            var data = new DataObject();
            data.SetData(FormatName(layer), false, Serialize(items));

            using (Bitmap first = TerrainCodec.Decode(layer, items[0].Data))
            {
                if (first != null)
                {
                    data.SetData(DataFormats.Bitmap, true, ArtLibraryImageConverter.ToExportBitmap(first));
                }
            }

            Clipboard.SetDataObject(data, true);
        }

        public static bool ContainsGraphics(TerrainLayer layer)
        {
            try
            {
                return Clipboard.ContainsData(FormatName(layer))
                       || Clipboard.ContainsImage()
                       || Clipboard.ContainsData("PNG")
                       || ArtLibraryClipboard.GetImageFiles().Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static List<TerrainClipboardItem> GetItems(TerrainLayer layer)
        {
            try
            {
                if (Clipboard.GetData(FormatName(layer)) is MemoryStream stream)
                {
                    return Deserialize(layer, stream);
                }
            }
            catch
            {
                // foreign or damaged data
            }

            return new List<TerrainClipboardItem>();
        }

        /// <summary>A normal clipboard image (not converted), or null.</summary>
        public static Bitmap GetImage()
        {
            try
            {
                if (Clipboard.GetData("PNG") is MemoryStream png)
                {
                    using (Image image = Image.FromStream(png))
                    {
                        return new Bitmap(image);
                    }
                }

                if (Clipboard.ContainsImage())
                {
                    using (Image image = Clipboard.GetImage())
                    {
                        return image == null ? null : new Bitmap(image);
                    }
                }
            }
            catch
            {
                // unsupported image data
            }

            return null;
        }

        private static MemoryStream Serialize(IReadOnlyList<TerrainClipboardItem> items)
        {
            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(Magic);
                writer.Write(items.Count);
                foreach (TerrainClipboardItem item in items)
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

        private static List<TerrainClipboardItem> Deserialize(TerrainLayer layer, Stream stream)
        {
            var result = new List<TerrainClipboardItem>();
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
                    var item = new TerrainClipboardItem { SourceId = reader.ReadInt32(), Name = reader.ReadString() };
                    int length = reader.ReadInt32();
                    item.Data = reader.ReadBytes(length);
                    if (item.Data.Length == length && TerrainCodec.IsValid(layer, item.Data))
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }
    }
}

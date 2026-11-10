using System;
using System.Collections.Generic;
using System.Drawing;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter
{
    /// <summary>
    /// Access to the texmaps.mul loaded in UoFiddler. Writing works like the Textures tab:
    /// the change is in memory until texmaps.mul is saved.
    /// </summary>
    internal static class TextureMul
    {
        public const int MaxId = 0x3FFF;

        public static bool IsValidSize(Size size) => size.Width == size.Height && (size.Width == 64 || size.Width == 128);

        public static bool IsAvailable
        {
            get
            {
                try
                {
                    lock (SdkSync.Lock)
                    {
                        return Textures.GetIdxLength() > 0;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsValid(int id)
        {
            if (id < 0 || id > MaxId)
            {
                return false;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    return Textures.TestTexture(id);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>The cached texture. Do not dispose or change it.</summary>
        public static Bitmap GetShared(int id)
        {
            if (!IsValid(id))
            {
                return null;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    return Textures.GetTexture(id);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>A 32bpp copy that belongs to the caller.</summary>
        public static Bitmap GetCopy(int id)
        {
            Bitmap shared = GetShared(id);
            if (shared == null)
            {
                return null;
            }
            lock (SdkSync.Lock)
            {
                return CutterImaging.To32Bpp(shared);
            }
        }

        public static bool[] ScanUsed()
        {
            var used = new bool[MaxId + 1];
            if (!IsAvailable)
            {
                return used;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    for (int i = 0; i <= MaxId; i++)
                    {
                        used[i] = Textures.TestTexture(i);
                    }
                }
            }
            catch (Exception)
            {
                // Files not loaded.
            }
            return used;
        }

        public static int FindFree(int start)
        {
            for (int i = Math.Max(0, start); i <= MaxId; i++)
            {
                if (!IsValid(i))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int FindFreeBlock(int start, int count)
        {
            int run = 0;
            for (int i = Math.Max(0, start); i <= MaxId; i++)
            {
                run = IsValid(i) ? 0 : run + 1;
                if (run >= count)
                {
                    return i - count + 1;
                }
            }
            return -1;
        }

        /// <summary>Writes a copy of the bitmap (64 × 64 or 128 × 128) and tells UoFiddler about it.</summary>
        public static void Write(int id, Bitmap bitmap, object sender)
        {
            if (id < 0 || id > MaxId)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, $"The texture id must be between 0 and 0x{MaxId:X4}.");
            }
            if (!IsValidSize(bitmap.Size))
            {
                throw new ArgumentException("Textures must be 64 × 64 or 128 × 128 pixels.");
            }
            Bitmap copy = CutterImaging.To32Bpp(bitmap);
            lock (SdkSync.Lock)
            {
                Textures.Replace(id, copy);
            }
            ControlEvents.FireTextureChangeEvent(sender, id);
            Options.ChangedUltimaClass["Texture"] = true;
        }

        public static List<int> WriteMany(int start, IReadOnlyList<Bitmap> images, bool skipUsed, object sender)
        {
            var written = new List<int>();
            int id = start;
            foreach (Bitmap image in images)
            {
                if (skipUsed)
                {
                    while (id <= MaxId && IsValid(id))
                    {
                        id++;
                    }
                }
                if (id > MaxId)
                {
                    break;
                }
                Write(id, image, sender);
                written.Add(id);
                id++;
            }
            return written;
        }

        public static void Remove(int id, object sender)
        {
            lock (SdkSync.Lock)
            {
                Textures.Remove(id);
            }
            ControlEvents.FireTextureChangeEvent(sender, id);
            Options.ChangedUltimaClass["Texture"] = true;
        }

        /// <summary>Saves texidx.mul / texmaps.mul into the UoFiddler output folder. Returns the folder.</summary>
        public static string Save()
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("No output folder is set in UoFiddler.");
            }
            lock (SdkSync.Lock)
            {
                Textures.Save(path);
            }
            Options.ChangedUltimaClass["Texture"] = false;
            return path;
        }

        /// <summary>Land tiles (art.mul) that use this texture according to tiledata.mul.</summary>
        public static List<int> LandTilesUsing(int textureId)
        {
            var list = new List<int>();
            try
            {
                lock (SdkSync.Lock)
                {
                    LandData[] table = TileData.LandTable;
                    for (int i = 0; i < table.Length; i++)
                    {
                        if (table[i].TextureId == textureId)
                        {
                            list.Add(i);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // tiledata not loaded
            }
            return list;
        }
    }
}

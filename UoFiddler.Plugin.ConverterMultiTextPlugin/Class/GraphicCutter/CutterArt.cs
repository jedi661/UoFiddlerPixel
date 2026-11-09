using System;
using System.Collections.Generic;
using System.Drawing;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter
{
    internal enum ArtKind { Item, Land }

    /// <summary>
    /// Access to the art.mul loaded in UoFiddler (items and land tiles).
    /// Writing works like the Items / Land tiles tab: the change is in memory until art.mul is saved.
    /// </summary>
    internal static class CutterArt
    {
        public const int LandSize = 44;

        /// <summary>Thrown by the access methods when no client files are loaded.</summary>
        public static bool IsAvailable
        {
            get
            {
                try
                {
                    lock (SdkSync.Lock)
                    {
                        return Art.GetIdxLength() > 0;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static int MaxId(ArtKind kind)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return kind == ArtKind.Item ? Art.GetMaxItemId() : Art.LandCount - 1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool IsValid(ArtKind kind, int id)
        {
            if (id < 0 || id > MaxId(kind))
            {
                return false;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    return kind == ArtKind.Item ? Art.IsValidStatic(id) : Art.IsValidLand(id);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>The cached art bitmap. Do not dispose or change it.</summary>
        public static Bitmap GetShared(ArtKind kind, int id)
        {
            if (id < 0 || id > MaxId(kind))
            {
                return null;
            }
            try
            {
                lock (SdkSync.Lock)
                {
                    if (kind == ArtKind.Item)
                    {
                        return Art.IsValidStatic(id) ? Art.GetStatic(id) : null;
                    }
                    return Art.IsValidLand(id) ? Art.GetLand(id) : null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>A 32bpp copy of the art that belongs to the caller.</summary>
        public static Bitmap GetCopy(ArtKind kind, int id)
        {
            Bitmap shared = GetShared(kind, id);
            if (shared == null)
            {
                return null;
            }
            lock (SdkSync.Lock)
            {
                return CutterImaging.To32Bpp(shared);
            }
        }

        /// <summary>All used (valid) ids of a kind, ascending.</summary>
        public static bool[] ScanUsed(ArtKind kind)
        {
            int max = MaxId(kind);
            var used = new bool[Math.Max(0, max + 1)];
            try
            {
                lock (SdkSync.Lock)
                {
                    for (int i = 0; i <= max; i++)
                    {
                        used[i] = kind == ArtKind.Item ? Art.IsValidStatic(i) : Art.IsValidLand(i);
                    }
                }
            }
            catch (Exception)
            {
                // Files not loaded – report nothing as used.
            }
            return used;
        }

        /// <summary>First free id at or after start, or -1.</summary>
        public static int FindFree(ArtKind kind, int start)
        {
            int max = MaxId(kind);
            for (int i = Math.Max(0, start); i <= max; i++)
            {
                if (!IsValid(kind, i))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>First id at or after start where count ids in a row are free, or -1.</summary>
        public static int FindFreeBlock(ArtKind kind, int start, int count)
        {
            int max = MaxId(kind);
            int run = 0;
            for (int i = Math.Max(0, start); i <= max; i++)
            {
                run = IsValid(kind, i) ? 0 : run + 1;
                if (run >= count)
                {
                    return i - count + 1;
                }
            }
            return -1;
        }

        /// <summary>Writes a copy of the bitmap into art.mul (in memory) and tells UoFiddler about it.</summary>
        public static void Write(ArtKind kind, int id, Bitmap bitmap, object sender)
        {
            if (id < 0 || id > MaxId(kind))
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, $"The id must be between 0 and 0x{MaxId(kind):X4}.");
            }
            if (kind == ArtKind.Land && (bitmap.Width != LandSize || bitmap.Height != LandSize))
            {
                throw new ArgumentException("Land tiles must be exactly 44 × 44 pixels.");
            }

            Bitmap copy = CutterImaging.To32Bpp(bitmap);
            lock (SdkSync.Lock)
            {
                if (kind == ArtKind.Item)
                {
                    Art.ReplaceStatic(id, copy);
                }
                else
                {
                    Art.ReplaceLand(id, copy);
                }
            }

            if (kind == ArtKind.Item)
            {
                ControlEvents.FireItemChangeEvent(sender, id);
            }
            else
            {
                ControlEvents.FireLandTileChangeEvent(sender, id);
            }
            Options.ChangedUltimaClass["Art"] = true;
        }

        /// <summary>Writes several images to ids from start on. Used ids are skipped when skipUsed is set.</summary>
        public static List<int> WriteMany(ArtKind kind, int start, IReadOnlyList<Bitmap> images, bool skipUsed, object sender)
        {
            var written = new List<int>();
            int id = start;
            int max = MaxId(kind);
            foreach (Bitmap image in images)
            {
                if (skipUsed)
                {
                    while (id <= max && IsValid(kind, id))
                    {
                        id++;
                    }
                }
                if (id > max)
                {
                    break;
                }
                Write(kind, id, image, sender);
                written.Add(id);
                id++;
            }
            return written;
        }

        /// <summary>Saves artidx.mul / art.mul into the UoFiddler output folder. Returns the folder.</summary>
        public static string Save()
        {
            string path = Options.OutputPath;
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("No output folder is set in UoFiddler.");
            }
            lock (SdkSync.Lock)
            {
                Art.Save(path);
            }
            Options.ChangedUltimaClass["Art"] = false;
            return path;
        }
    }
}

/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Reflection;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>
    /// Safe access to Art / Gump images of the Ultima SDK.
    /// <para>
    /// The SDK returns the bitmap that lives in its internal cache – it must never be disposed.
    /// These helpers always return an independent 32bpp copy (the caller owns it). If the cached
    /// bitmap was disposed somewhere (GDI+ then throws "Parameter is not valid"), the broken cache
    /// entry is removed and the image is read again from the client files.
    /// </para>
    /// </summary>
    public static class UltimaImages
    {
        /// <summary>Copy of an item (static) image or null.</summary>
        public static Bitmap CopyStatic(int id)
        {
            if (!Art.IsValidStatic(id))
            {
                return null;
            }
            return SafeCopy(() => Art.GetStatic(id), typeof(Art));
        }

        /// <summary>Copy of a land tile image or null.</summary>
        public static Bitmap CopyLand(int id)
        {
            if (!Art.IsValidLand(id))
            {
                return null;
            }
            return SafeCopy(() => Art.GetLand(id), typeof(Art));
        }

        /// <summary>Copy of a gump image or null.</summary>
        public static Bitmap CopyGump(int id)
        {
            if (id < 0 || id >= Gumps.GetCount() || !Gumps.IsValidIndex(id))
            {
                return null;
            }
            return SafeCopy(() => Gumps.GetGump(id), typeof(Gumps));
        }

        private static Bitmap SafeCopy(Func<Bitmap> get, Type cacheOwner)
        {
            Bitmap cached = get();
            if (cached == null)
            {
                return null;
            }

            try
            {
                return StudioImaging.To32(cached);
            }
            catch (ArgumentException)
            {
                // the cached bitmap was disposed -> drop it from the cache and load it again
                if (!RemoveFromCache(cacheOwner, cached))
                {
                    throw new InvalidOperationException(
                        "Das Bild im UoFiddler-Cache ist beschädigt. Bitte UoFiddler neu starten.");
                }
                Bitmap reloaded = get();
                return reloaded == null ? null : StudioImaging.To32(reloaded);
            }
        }

        /// <summary>Sets the cache slot that holds the broken bitmap back to null.</summary>
        private static bool RemoveFromCache(Type owner, Bitmap broken)
        {
            FieldInfo field = owner.GetField("_cache", BindingFlags.NonPublic | BindingFlags.Static);
            if (field?.GetValue(null) is not Bitmap[] cache)
            {
                return false;
            }
            int index = Array.IndexOf(cache, broken);
            if (index < 0)
            {
                return false;
            }
            cache[index] = null;
            return true;
        }
    }
}

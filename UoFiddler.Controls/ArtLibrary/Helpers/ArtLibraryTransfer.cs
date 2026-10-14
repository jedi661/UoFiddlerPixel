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
using System.Text.RegularExpressions;
using Ultima;
using UoFiddler.Controls.ArtLibrary.Core;
using UoFiddler.Controls.ArtLibrary.Metadata;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.ArtLibrary.Helpers
{
    /// <summary>
    /// Everything that touches the original art.mul (static Ultima.Art) plus file name helpers.
    /// This is the only place where the art library talks to the original classes; it only uses their
    /// public methods (Art.GetStatic/ReplaceStatic/RemoveStatic, ControlEvents.FireItemChangeEvent).
    /// Changes to the original are made in memory like in the items tab and must be saved there.
    /// </summary>
    public static class ArtLibraryTransfer
    {
        #region [ Original art.mul ]

        /// <summary>True when an art.mul is loaded in UoFiddler.</summary>
        public static bool IsOriginalAvailable
        {
            get
            {
                try
                {
                    return Art.GetIdxLength() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>Highest item id of the loaded art.mul.</summary>
        public static int OriginalMaxId => Art.GetMaxItemId();

        public static bool IsValidOriginalId(int id) => id >= 0 && id <= OriginalMaxId;

        public static bool IsOriginalOccupied(int id) => IsValidOriginalId(id) && Art.IsValidStatic(id);

        /// <summary>Graphic of the original art.mul (including unsaved changes) or null.</summary>
        public static Bitmap GetOriginal(int id)
        {
            return IsValidOriginalId(id) ? Art.GetStatic(id) : null;
        }

        /// <summary>Item name from tiledata.mul, or an empty string.</summary>
        public static string GetOriginalName(int id)
        {
            try
            {
                ItemData[] table = TileData.ItemTable;
                if (table != null && id >= 0 && id < table.Length)
                {
                    return table[id].Name?.Trim() ?? string.Empty;
                }
            }
            catch
            {
                // tiledata not loaded
            }

            return string.Empty;
        }

        /// <summary>
        /// Writes a graphic into the original art.mul (in memory, like the items tab) and notifies the items tab.
        /// </summary>
        public static void WriteToOriginal(int originalId, Bitmap bitmap, object sender)
        {
            if (!IsValidOriginalId(originalId))
            {
                throw new ArgumentOutOfRangeException(nameof(originalId), originalId,
                    $"Die ID liegt außerhalb der originalen art.mul (0 bis {ArtIdParser.ToHex(OriginalMaxId)}).");
            }

            // independent copy: the library bitmap is cached and shared
            Bitmap copy = ArtLibraryCodec.ToBitmap(ArtLibraryCodec.ToPixels(bitmap), bitmap.Width, bitmap.Height);

            Art.ReplaceStatic(originalId, copy);
            ControlEvents.FireItemChangeEvent(sender, originalId);
            Options.ChangedUltimaClass["Art"] = true;
        }

        public static void RemoveFromOriginal(int originalId, object sender)
        {
            if (!IsValidOriginalId(originalId))
            {
                return;
            }

            Art.RemoveStatic(originalId);
            ControlEvents.FireItemChangeEvent(sender, originalId);
            Options.ChangedUltimaClass["Art"] = true;
        }

        /// <summary>
        /// Copies a graphic from the original art.mul into the library and remembers where it came from.
        /// </summary>
        public static bool ImportFromOriginal(ArtLibraryFile library, ArtLibraryMetadata metadata, int originalId, int libraryId)
        {
            Bitmap bitmap = GetOriginal(originalId);
            if (bitmap == null)
            {
                return false;
            }

            library.SetBitmap(libraryId, bitmap);

            if (metadata != null)
            {
                ArtLibraryItemInfo info = metadata.GetOrCreate(libraryId);
                info.SourceId = originalId;
                info.Added = DateTime.Now;
                if (string.IsNullOrWhiteSpace(info.Name))
                {
                    info.Name = GetOriginalName(originalId);
                }

                metadata.MarkDirty();
            }

            return true;
        }

        #endregion

        #region [ Target ids ]

        /// <summary>
        /// Calculates the target id for every source id.
        /// <para><paramref name="keepIds"/>: target = source id. Otherwise ids are assigned one after
        /// another starting at <paramref name="startId"/>.</para>
        /// <para>Without <paramref name="overwrite"/> occupied ids are skipped (sequential mode) or the
        /// item is left out (keep ids mode). Left out items get -1.</para>
        /// </summary>
        public static int[] AssignTargetIds(IReadOnlyList<int> sourceIds, int startId, bool keepIds, bool overwrite,
            Func<int, bool> isOccupied, int maxId)
        {
            var result = new int[sourceIds.Count];
            int next = startId;
            var used = new HashSet<int>();

            for (int i = 0; i < sourceIds.Count; ++i)
            {
                int target;
                if (keepIds)
                {
                    target = sourceIds[i];
                    if (target < 0 || target > maxId || (!overwrite && isOccupied(target)) || !used.Add(target))
                    {
                        target = -1;
                    }
                }
                else
                {
                    while (next <= maxId && !overwrite && isOccupied(next))
                    {
                        ++next;
                    }

                    target = next <= maxId ? next++ : -1;
                }

                result[i] = target;
            }

            return result;
        }

        #endregion

        #region [ File names ]

        private static readonly Regex _hexPrefix = new Regex(@"0x([0-9a-fA-F]+)", RegexOptions.Compiled);
        private static readonly Regex _hexAdress = new Regex(@"Hex_Adress_([0-9a-fA-F]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _decimal = new Regex(@"(?<![0-9A-Za-z])([0-9]+)(?![0-9A-Za-z])", RegexOptions.Compiled);

        /// <summary>
        /// File name used for exports, same as the items tab ("Item 0x04D2.bmp"), so files can be
        /// imported into both the library and the original.
        /// </summary>
        public static string GetExportFileName(int id, string extension)
        {
            return $"Item 0x{id:X4}.{extension.TrimStart('.')}";
        }

        /// <summary>
        /// Reads an id from a file name: "Item 0x04D2.bmp", "Art_Hex_Adress_4D2.bmp", "4D2h.png",
        /// "0x4D2.png" or a plain decimal number ("1234.bmp", "tisch_1234.png"). Returns -1 if none is found.
        /// </summary>
        public static int ParseIdFromFileName(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path ?? string.Empty);
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            Match match = _hexPrefix.Match(name);
            if (match.Success && ArtIdParser.TryParse("0x" + match.Groups[1].Value, out int id))
            {
                return id;
            }

            match = _hexAdress.Match(name);
            if (match.Success && ArtIdParser.TryParse("0x" + match.Groups[1].Value, out id))
            {
                return id;
            }

            if (ArtIdParser.TryParse(name, out id))
            {
                return id;
            }

            MatchCollection numbers = _decimal.Matches(name);
            if (numbers.Count > 0 && ArtIdParser.TryParse(numbers[numbers.Count - 1].Groups[1].Value, out id))
            {
                return id;
            }

            return -1;
        }

        #endregion
    }
}

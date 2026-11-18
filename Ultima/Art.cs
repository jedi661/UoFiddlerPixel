using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima.Helpers;

namespace Ultima
{
    public static class Art
    {
        // ===================================================================================================
        // RANGE DEFINITION — the single place that defines how many Land tiles and Static/item art entries
        // this class reserves. Static art is stored right after Land art in the same array, offset by
        // LandCount (mirrors the legacy Artidx.mul/Art.mul layout, where static index 0 sits at slot 0x4000).
        //
        // The defaults below reproduce the legacy client layout exactly (16,384 Land + 65,536 Static =
        // 81,920 = 0x14000 total), so every existing artidx.mul/art.mul (ML, Stygian Abyss, High Seas) keeps
        // loading and saving exactly as before — nothing about legacy behavior changes.
        //
        // RECOMMENDED WAY TO GET MORE THAN 1,000,000 ITEMS: the side-car extension (ExtendedFormat.cs,
        // CreateOrResizeExtension/SaveExtension below). It keeps art.mul/artidx.mul 100% client compatible and stores
        // ids >= 0x10000 in artextidx.mul/artext.mul (64-bit offsets). The constants below are the older,
        // single-file alternative and are left at the legacy sizes.
        //
        // TO EXTEND BEYOND 1,000,000 ENTRIES (this tool's own "Extended" format — not read by a regular
        // client, only by tools/servers that understand a bigger artidx.mul/art.mul), just raise the two
        // constants, e.g. for ~1,000,000 total split evenly between Land and Static:
        //
        //      public const int LandCount = 500_000;
        //      public const int StaticCount = 500_000;
        //
        // Rules when changing them:
        //   * Never set LandCount below 0x4000 or StaticCount below 0x10000 — that would truncate legacy
        //     data on load/save (enforced by ValidateRangeConstants() below, called from the static ctor).
        //   * These constants only size the in-memory capacity and a freshly-saved artidx.mul/art.mul.
        //     Loading an existing, smaller legacy file always works — unused extra slots simply stay empty.
        //   * GetMaxItemId()/IsExtended() decide, purely from the *loaded* file's actual entry count
        //     (GetIdxLength()), whether legacy (High Seas/Stygian Abyss/ML) or the Extended item-id range
        //     applies — see GetMaxItemId() below.
        //   * Static art above 0xFFFF (65535) can only be reached through GetLegalStaticId()/GetStatic()/
        //     ReplaceStatic()/RemoveStatic()/IsValidStatic()/GetRawStatic() — NOT through GetLegalItemId(),
        //     which stays ushort-limited on purpose (see comment there).
        // ===================================================================================================
        public const int LandCount = 0x4000;   // 16,384 — legacy land-tile range
        public const int StaticCount = 0x10000; // 65,536 — legacy static/item range
        public const int TotalCount = LandCount + StaticCount;

        private static FileIndex _fileIndex = new FileIndex(
        "Artidx.mul", "Art.mul", "artLegacyMUL.uop", TotalCount, 4, ".tga", 0x13FDC, false);
        private static Bitmap[] _cache;
        private static bool[] _removed;
        private static readonly Dictionary<int, bool> _patched = new Dictionary<int, bool>();
        public static bool Modified;

        /// <summary>
        /// True when the classic part (land/static art in art.mul, ids below 0x10000) was changed since loading or the
        /// last full <see cref="Save(string)"/>. Changes in the extended range do not set it, so after saving only
        /// the extension files the tool knows whether art.mul still has unsaved changes.
        /// </summary>
        public static bool LegacyModified { get; private set; }

        private static byte[] _streamBuffer;
        private static readonly byte[] _validBuffer = new byte[4];

        private struct ImageData
        {
            public byte[] Data;
            public int Position;
            public int Length;
        }

        // sha256 (hex) -> already written image, so identical images are stored once (O(1) instead of O(n^2))
        private static Dictionary<string, ImageData> _landImageData;
        private static Dictionary<string, ImageData> _staticImageData;

        // ===================================================================================================
        // EXTENDED SIDE-CAR STORAGE (see ExtendedFormat.cs): static items with id >= 0x10000 live in
        // artextidx.mul / artext.mul. art.mul / artidx.mul keep their exact legacy layout, so the regular client
        // keeps working. Null _extIndex = no extension loaded/created (pure legacy behaviour).
        // ===================================================================================================
        private static ExtendedFormat.ArtIndexEntry[] _extIndex;
        private static Bitmap[] _extCache;
        private static bool[] _extRemoved;
        private static string _extDataPath;
        private static FileStream _extStream;

        static Art()
        {
            ValidateRangeConstants();
            _cache = new Bitmap[TotalCount];
            _removed = new bool[TotalCount];
            LoadExtension();
        }

        #region Extended side-car storage
        /// <summary>True if an extended art storage (ids >= 0x10000) is loaded or was created in this session.</summary>
        public static bool HasExtension => _extIndex != null;

        /// <summary>Number of extended slots (ids 0x10000 .. 0x10000 + ExtendedCount - 1).</summary>
        public static int ExtendedCount => _extIndex?.Length ?? 0;

        /// <summary>
        /// Highest static id including the extension. Without extension identical to <see cref="GetMaxItemId"/>.
        /// Use this only where ids above 0xFFFF are supported (Items tab, TileData) - everything that writes
        /// map/statics/multi files must keep using <see cref="GetMaxItemId"/>.
        /// </summary>
        public static int GetMaxExtendedItemId()
        {
            return HasExtension ? ExtendedFormat.GetMaxId(ExtendedCount) : GetMaxItemId();
        }

        /// <summary>Path of the loaded artext.mul (null if the extension only exists in memory).</summary>
        public static string ExtendedDataPath => _extDataPath;

        private static void LoadExtension()
        {
            CloseExtensionStream();
            _extIndex = null;
            _extCache = null;
            _extRemoved = null;
            _extDataPath = null;

            try
            {
                string idx = Files.GetFilePath(ExtendedFormat.ArtIndexFileName);
                string mul = Files.GetFilePath(ExtendedFormat.ArtDataFileName);
                if (idx == null || mul == null)
                {
                    return;
                }

                ExtendedFormat.ArtIndexEntry[] entries = ExtendedFormat.ReadArtIndex(idx);
                _extIndex = entries;
                _extCache = new Bitmap[entries.Length];
                _extRemoved = new bool[entries.Length];
                _extDataPath = mul;
            }
            catch (Exception)
            {
                // corrupt or foreign file: behave like pure legacy, never break loading of art.mul
                _extIndex = null;
                _extCache = null;
                _extRemoved = null;
                _extDataPath = null;
            }
        }

        private static void CloseExtensionStream()
        {
            _extStream?.Dispose();
            _extStream = null;
        }

        /// <summary>
        /// Creates the extended storage in memory or resizes it (files are written on the next <see cref="Save(string)"/>).
        /// Growing keeps every existing item. Shrinking drops all items above the new size.
        /// </summary>
        public static void CreateOrResizeExtension(int slots)
        {
            if (slots < 1 || slots > ExtendedFormat.MaxSlots)
            {
                throw new ArgumentOutOfRangeException(nameof(slots), slots, $"Must be between 1 and {ExtendedFormat.MaxSlots}.");
            }

            int old = ExtendedCount;
            if (_extIndex == null)
            {
                _extIndex = new ExtendedFormat.ArtIndexEntry[slots];
                _extCache = new Bitmap[slots];
                _extRemoved = new bool[slots];
                for (int i = 0; i < slots; ++i)
                {
                    _extIndex[i] = ExtendedFormat.ArtIndexEntry.Empty;
                }
            }
            else
            {
                Array.Resize(ref _extIndex, slots);
                Array.Resize(ref _extCache, slots);
                Array.Resize(ref _extRemoved, slots);
                for (int i = old; i < slots; ++i)
                {
                    _extIndex[i] = ExtendedFormat.ArtIndexEntry.Empty;
                }
            }

            Modified = true;
        }

        /// <summary>Number of used extended slots (cheap, uses the in-memory index only).</summary>
        public static int CountUsedExtended()
        {
            int n = 0;
            for (int k = 0; k < ExtendedCount; ++k)
            {
                if (IsValidExtended(k))
                {
                    ++n;
                }
            }

            return n;
        }

        private static bool IsValidExtended(int k)
        {
            if (_extRemoved[k])
            {
                return false;
            }

            return _extCache[k] != null || _extIndex[k].IsValid;
        }

        private static byte[] ReadExtendedRaw(int k)
        {
            ExtendedFormat.ArtIndexEntry e = _extIndex[k];
            if (!e.IsValid || _extDataPath == null)
            {
                return null;
            }

            if (_extStream == null || !_extStream.CanRead)
            {
                if (!File.Exists(_extDataPath))
                {
                    return null;
                }

                _extStream = new FileStream(_extDataPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (e.Offset + e.Length > _extStream.Length)
            {
                return null;
            }

            var buffer = new byte[e.Length];
            _extStream.Seek(e.Offset, SeekOrigin.Begin);
            _extStream.ReadExactly(buffer, 0, e.Length);
            return buffer;
        }

        private static Bitmap GetExtended(int k)
        {
            if (_extRemoved[k])
            {
                return null;
            }

            if (_extCache[k] != null)
            {
                return _extCache[k];
            }

            byte[] raw = ReadExtendedRaw(k);
            if (raw == null)
            {
                return null;
            }

            Bitmap bmp = LoadStatic(new MemoryStream(raw), raw.Length);
            if (Files.CacheData)
            {
                _extCache[k] = bmp;
            }

            return bmp;
        }
        #endregion

        // Guards against LandCount/StaticCount being edited below the legacy minimums, which would silently
        // truncate legacy artidx.mul/art.mul data. Runs once from the static ctor and from Reload().
        private static void ValidateRangeConstants()
        {
            if (LandCount < 0x4000 || StaticCount < 0x10000)
            {
                throw new InvalidOperationException(
                    "Art.LandCount/Art.StaticCount must stay at least at the legacy sizes " +
                    "(0x4000 Land / 0x10000 Static) so existing artidx.mul/art.mul files keep loading correctly.");
            }
        }

        /// <summary>
        /// Highest legal Static item id for the format that is currently loaded, based on the actual
        /// entry count found in artidx.mul (GetIdxLength()) — NOT on the LandCount/StaticCount capacity
        /// defined above. A newly loaded legacy file (ML/Stygian Abyss/High Seas) always resolves to the
        /// exact same value it always did.
        /// </summary>
        public static int GetMaxItemId()
        {
            // Extended (this tool's own >1,000,000-capable format — see RANGE DEFINITION above)
            if (GetIdxLength() > 0x13FDC)
            {
                return StaticCount - 1;
            }

            // High Seas
            if (GetIdxLength() >= 0x13FDC)
            {
                return 0xFFDC;
            }

            // Stygian Abyss
            if (GetIdxLength() == 0xC000)
            {
                return 0x7FFF;
            }

            // ML and older
            return 0x3FFF;
        }

        public static bool IsUOAHS()
        {
            return GetIdxLength() >= 0x13FDC;
        }

        /// <summary>
        /// True once a loaded artidx.mul has more entries than any regular client format ever produces —
        /// i.e. it was saved by this tool with LandCount/StaticCount raised above the legacy sizes.
        /// </summary>
        public static bool IsExtended()
        {
            return GetIdxLength() > 0x13FDC;
        }

        // Kept ushort on purpose: this method also legalizes ids for the ushort-based map/statics-placement
        // mul formats (TileMatrix, MultiComponentList, map/statics editors, ...), whose file format itself
        // stores a UInt16 per tile — those callers cannot use ids above 65535 regardless of what art.mul
        // supports. For direct art.mul static-art access above 0xFFFF, use GetLegalStaticId() instead.
        public static ushort GetLegalItemId(int itemId, bool checkMaxId = true)
        {
            if (itemId < 0)
            {
                return 0;
            }

            if (!checkMaxId)
            {
                return (ushort)itemId;
            }

            int max = GetMaxItemId();
            if (itemId > max)
            {
                return 0;
            }

            return (ushort)itemId;
        }

        /// <summary>
        /// Extended (int-based) counterpart of GetLegalItemId(), used internally by GetStatic/ReplaceStatic/
        /// RemoveStatic/IsValidStatic/GetRawStatic. Supports the full StaticCount range, so static art
        /// indices above 0xFFFF work once StaticCount is raised above the legacy 0x10000 (see RANGE
        /// DEFINITION above).
        /// </summary>
        public static int GetLegalStaticId(int itemId, bool checkMaxId = true)
        {
            if (itemId < 0)
            {
                return 0;
            }

            // extended ids (>= 0x10000) are only legal inside the loaded/created extension
            if (itemId >= ExtendedFormat.BaseId)
            {
                return HasExtension && itemId <= GetMaxExtendedItemId() ? itemId : 0;
            }

            if (!checkMaxId)
            {
                return itemId;
            }

            int max = GetMaxItemId();
            return itemId > max ? 0 : itemId;
        }

        // Normalizes a Land index into [0, LandCount). Legacy code used "index &= 0x3FFF", which only works
        // because 0x4000 is a power of two; LandCount may not be one once customized (e.g. 500_000), so this
        // uses a real modulo instead.
        private static int NormalizeLandIndex(int index)
        {
            index %= LandCount;
            if (index < 0)
            {
                index += LandCount;
            }

            return index;
        }

        public static int GetIdxLength()
        {
            return (int)(_fileIndex.IdxLength / 12);
        }

        /// <summary>
        /// ReReads Art.mul
        /// </summary>
        public static void Reload()
        {
            ValidateRangeConstants();
            _fileIndex = new FileIndex(
                "Artidx.mul", "Art.mul", "artLegacyMUL.uop", TotalCount, 4, ".tga", 0x13FDC, false);
            _cache = new Bitmap[TotalCount];
            _removed = new bool[TotalCount];
            _patched.Clear();
            LoadExtension();
            Modified = false;
            LegacyModified = false;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_cache"/> of Static
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceStatic(int index, Bitmap bmp)
        {
            index = GetLegalStaticId(index);
            if (index >= ExtendedFormat.BaseId)
            {
                int k = index - ExtendedFormat.BaseId;
                _extCache[k] = bmp;
                _extRemoved[k] = bmp == null;
                Modified = true;
                return;
            }

            index += LandCount;
            LegacyModified = true;

            _cache[index] = bmp;
            _removed[index] = false;

            if (_patched.ContainsKey(index))
            {
                _patched.Remove(index);
            }

            Modified = true;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_cache"/> of Land
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceLand(int index, Bitmap bmp)
        {
            index = NormalizeLandIndex(index);
            LegacyModified = true;
            _cache[index] = bmp;
            _removed[index] = false;

            if (_patched.ContainsKey(index))
            {
                _patched.Remove(index);
            }

            Modified = true;
        }

        /// <summary>
        /// Removes Static index <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveStatic(int index)
        {
            index = GetLegalStaticId(index);
            if (index >= ExtendedFormat.BaseId)
            {
                _extRemoved[index - ExtendedFormat.BaseId] = true;
                Modified = true;
                return;
            }

            index += LandCount;

            _removed[index] = true;
            Modified = true;
            LegacyModified = true;
        }

        /// <summary>
        /// Removes Land index <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveLand(int index)
        {
            index = NormalizeLandIndex(index);
            _removed[index] = true;
            Modified = true;
            LegacyModified = true;
        }

        /// <summary>
        /// Tests if Static is defined (width and height check)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidStatic(int index)
        {
            index = GetLegalStaticId(index);
            if (index >= ExtendedFormat.BaseId)
            {
                return IsValidExtended(index - ExtendedFormat.BaseId);
            }

            index += LandCount;

            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            Stream stream = _fileIndex.Seek(index, out int _, out int _, out bool _);

            if (stream == null)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Current);
            stream.Read(_validBuffer, 0, 4);

            short width = (short)(_validBuffer[0] | (_validBuffer[1] << 8));
            short height = (short)(_validBuffer[2] | (_validBuffer[3] << 8));

            return width > 0 && height > 0;
        }

        /// <summary>
        /// Tests if LandTile is defined
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidLand(int index)
        {
            index = NormalizeLandIndex(index);
            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            return _fileIndex.Valid(index, out int _, out int _, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of LandTile (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index)
        {
            return GetLand(index, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of LandTile (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index, out bool patched)
        {
            index = NormalizeLandIndex(index);
            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out patched);
            if (stream == null)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            if (Files.CacheData)
            {
                return _cache[index] = LoadLand(stream, length);
            }

            return LoadLand(stream, length);
        }

        // ReSharper disable once UnusedMember.Global
        public static byte[] GetRawLand(int index)
        {
            index = NormalizeLandIndex(index);

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of Static (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="checkMaxId"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, bool checkMaxId = true)
        {
            return GetStatic(index, out bool _, checkMaxId);
        }

        /// <summary>
        /// Returns Bitmap of Static (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <param name="checkMaxId"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, out bool patched, bool checkMaxId = true)
        {
            index = GetLegalStaticId(index, checkMaxId);
            if (index >= ExtendedFormat.BaseId)
            {
                patched = false;
                return GetExtended(index - ExtendedFormat.BaseId);
            }

            index += LandCount;

            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out patched);
            if (stream == null)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            if (Files.CacheData)
            {
                return _cache[index] = LoadStatic(stream, length);
            }

            return LoadStatic(stream, length);
        }

        // ReSharper disable once UnusedMember.Global
        public static byte[] GetRawStatic(int index)
        {
            index = GetLegalStaticId(index);
            if (index >= ExtendedFormat.BaseId)
            {
                return ReadExtendedRaw(index - ExtendedFormat.BaseId);
            }

            index += LandCount;

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        public static unsafe void Measure(Bitmap bmp, out int xMin, out int yMin, out int xMax, out int yMax)
        {
            xMin = yMin = 0;
            xMax = yMax = -1;

            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
            {
                return;
            }

            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);

            int delta = (bd.Stride >> 1) - bd.Width;
            int lineDelta = bd.Stride >> 1;

            var pBuffer = (ushort*)bd.Scan0;
            ushort* pLineEnd = pBuffer + bd.Width;
            ushort* pEnd = pBuffer + (bd.Height * lineDelta);

            bool foundPixel = false;

            int x = 0, y = 0;

            while (pBuffer < pEnd)
            {
                while (pBuffer < pLineEnd)
                {
                    ushort c = *pBuffer++;

                    if ((c & 0x8000) != 0)
                    {
                        if (!foundPixel)
                        {
                            foundPixel = true;
                            xMin = xMax = x;
                            yMin = yMax = y;
                        }
                        else
                        {
                            if (x < xMin)
                            {
                                xMin = x;
                            }

                            if (y < yMin)
                            {
                                yMin = y;
                            }

                            if (x > xMax)
                            {
                                xMax = x;
                            }

                            if (y > yMax)
                            {
                                yMax = y;
                            }
                        }
                    }
                    ++x;
                }

                pBuffer += delta;
                pLineEnd += lineDelta;
                ++y;
                x = 0;
            }

            bmp.UnlockBits(bd);
        }

        private static unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            stream.Read(_streamBuffer, 0, length);
            stream.Close();

            Bitmap bmp;
            fixed (byte* data = _streamBuffer)
            {
                var binData = (ushort*)data;
                int count = 2;
                int width = binData[count++];
                int height = binData[count++];

                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                var lookups = new int[height];

                int start = height + 4;

                for (int i = 0; i < height; ++i)
                {
                    lookups[i] = start + binData[count++];
                }

                bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = lookups[y];

                    ushort* cur = line;
                    int xOffset, xRun;

                    while ((xOffset = binData[count++]) + (xRun = binData[count++]) != 0)
                    {
                        if (xOffset > delta)
                        {
                            break;
                        }

                        cur += xOffset;
                        if (xOffset + xRun > delta)
                        {
                            break;
                        }

                        ushort* end = cur + xRun;
                        while (cur < end)
                        {
                            *cur++ = (ushort)(binData[count++] ^ 0x8000);
                        }
                    }
                }

                bmp.UnlockBits(bd);
            }

            return bmp;
        }

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            var bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            stream.Read(_streamBuffer, 0, length);
            stream.Close();
            fixed (byte* binData = _streamBuffer)
            {
                var bdata = (ushort*)binData;
                int xOffset = 21;
                int xRun = 2;

                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                    }
                }

                xOffset = 0;
                xRun = 44;

                for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                    }
                }
            }

            bmp.UnlockBits(bd);

            return bmp;
        }

        /// <summary>
        /// Saves artidx.mul/art.mul, writing exactly as many entries as are currently loaded
        /// (GetIdxLength()) — identical to the historical behavior. To force-write the full Extended
        /// capacity (e.g. when creating a brand new file that uses all of LandCount/StaticCount), use the
        /// <see cref="Save(string, int)"/> overload with entryCount: TotalCount.
        /// </summary>
        /// <param name="path"></param>
        public static void Save(string path)
        {
            Save(path, GetIdxLength());
        }

        /// <summary>
        /// Saves artidx.mul/art.mul, writing exactly <paramref name="entryCount"/> index slots. Pass
        /// Art.TotalCount (or any custom count up to it, e.g. LandCount + 500_000) to write beyond the
        /// legacy 0x14000 entries — see RANGE DEFINITION at the top of this class for how to raise
        /// LandCount/StaticCount first.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="entryCount"></param>
        public static unsafe void Save(string path, int entryCount)
        {
            if (entryCount < 0 || entryCount > TotalCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(entryCount),
                    entryCount,
                    $"Must be between 0 and TotalCount ({TotalCount}). Raise LandCount/StaticCount first if you need to save more entries.");
            }

            _landImageData = new Dictionary<string, ImageData>();
            _staticImageData = new Dictionary<string, ImageData>();

            string idx = Path.Combine(path, "artidx.mul");
            string mul = Path.Combine(path, "art.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memidx = new MemoryStream();
                var memmul = new MemoryStream();

                using (var binidx = new BinaryWriter(memidx))
                using (var binmul = new BinaryWriter(memmul))
                {
                    for (int index = 0; index < entryCount; index++)
                    {
                        Files.FireFileSaveEvent();
                        if (_cache[index] == null)
                        {
                            if (index < LandCount)
                            {
                                _cache[index] = GetLand(index);
                            }
                            else
                            {
                                _cache[index] = GetStatic(index - LandCount, false);
                            }
                        }

                        Bitmap bmp = _cache[index];
                        if (bmp == null || _removed[index])
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0);  // Length
                            binidx.Write(-1); // extra
                        }
                        else if (index < LandCount)
                        {
                            byte[] imageData = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            if (CompareSaveImagesLand(imageData, out ImageData resultImageData))
                            {
                                binidx.Write(resultImageData.Position); // lookup
                                binidx.Write(resultImageData.Length);
                                binidx.Write(0);

                                continue;
                            }

                            // land
                            BitmapData bd = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); // lookup
                            var length = (int)binmul.BaseStream.Position;
                            int x = 22;
                            int y = 0; // TODO: y is never used? 
                            int lineWidth = 2;
                            for (int m = 0; m < 22; ++m, ++y, line += delta, lineWidth += 2)
                            {
                                --x;
                                ushort* cur = line;
                                for (int n = 0; n < lineWidth; ++n)
                                {
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                                }
                            }

                            x = 0;
                            lineWidth = 44;
                            y = 22;
                            line = (ushort*)bd.Scan0;
                            line += delta * 22;
                            for (int m = 0; m < 22; m++, y++, line += delta, ++x, lineWidth -= 2)
                            {
                                ushort* cur = line;
                                for (int n = 0; n < lineWidth; n++)
                                {
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                                }
                            }

                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);

                            _landImageData[Convert.ToHexString(imageData)] = new ImageData
                            {
                                Position = start,
                                Length = length,
                                Data = imageData
                            };
                        }
                        else
                        {
                            byte[] imageData = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            if (CompareSaveImagesStatic(imageData, out ImageData resultImageData))
                            {
                                binidx.Write(resultImageData.Position); // lookup
                                binidx.Write(resultImageData.Length);
                                binidx.Write(0);

                                continue;
                            }

                            // art
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); // lookup
                            var length = (int)binmul.BaseStream.Position;
                            binmul.Write(1234); // header //TODO: check what to write to header? Maybe different value will be better?
                            binmul.Write((short)bmp.Width);
                            binmul.Write((short)bmp.Height);
                            var lookup = (int)binmul.BaseStream.Position;
                            int streamLoc = lookup + (bmp.Height * 2);
                            int width = 0;
                            for (int i = 0; i < bmp.Height; ++i) // fill lookup
                            {
                                binmul.Write(width);
                            }

                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                width = (int)(binmul.BaseStream.Position - streamLoc) / 2;
                                binmul.BaseStream.Seek(lookup + (y * 2), SeekOrigin.Begin);
                                binmul.Write(width);
                                binmul.BaseStream.Seek(streamLoc + (width * 2), SeekOrigin.Begin);
                                int i = 0;
                                int x = 0;
                                while (i < bmp.Width)
                                {
                                    for (i = x; i <= bmp.Width; ++i)
                                    {
                                        // first pixel set
                                        if (i >= bmp.Width)
                                        {
                                            continue;
                                        }

                                        if (cur[i] != 0)
                                        {
                                            break;
                                        }
                                    }

                                    if (i >= bmp.Width)
                                    {
                                        continue;
                                    }

                                    int j;
                                    for (j = i + 1; j < bmp.Width; ++j)
                                    {
                                        // next non set pixel
                                        if (cur[j] == 0)
                                        {
                                            break;
                                        }
                                    }

                                    binmul.Write((short)(i - x)); // xOffset
                                    binmul.Write((short)(j - i)); // run

                                    for (int p = i; p < j; ++p)
                                    {
                                        binmul.Write((ushort)(cur[p] ^ 0x8000));
                                    }

                                    x = j;
                                }

                                binmul.Write((short)0); // xOffset
                                binmul.Write((short)0); // Run
                            }

                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);

                            _staticImageData[Convert.ToHexString(imageData)] = new ImageData
                            {
                                Position = start,
                                Length = length,
                                Data = imageData
                            };
                        }
                    }

                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }

            LegacyModified = false;

            // ids >= 0x10000 go to the side-car files, art.mul/artidx.mul stay client compatible
            SaveExtension(path);
        }

        private static bool CompareSaveImagesLand(byte[] newChecksum, out ImageData sum)
        {
            return _landImageData.TryGetValue(Convert.ToHexString(newChecksum), out sum);
        }

        private static bool CompareSaveImagesStatic(byte[] imageData, out ImageData resultImageData)
        {
            return _staticImageData.TryGetValue(Convert.ToHexString(imageData), out resultImageData);
        }

        #region Extended save
        /// <summary>
        /// Encodes a bitmap into the static art.mul format (int32 header, int16 width, int16 height,
        /// uint16 row lookups, runs of [xOffset, run, pixels...] terminated by 0,0).
        /// </summary>
        public static unsafe byte[] EncodeStatic(Bitmap bmp)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            int width = bmp.Width;
            int height = bmp.Height;
            bw.Write(1234); // header (unused by readers)
            bw.Write((short)width);
            bw.Write((short)height);

            long lookupPos = ms.Position;
            for (int i = 0; i < height; ++i)
            {
                bw.Write((ushort)0);
            }

            long dataStart = ms.Position;
            var lookups = new ushort[height];

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    long rel = (ms.Position - dataStart) / 2;
                    if (rel > ushort.MaxValue)
                    {
                        throw new InvalidOperationException("Image too large for the static art format.");
                    }

                    lookups[y] = (ushort)rel;

                    int x = 0;
                    while (x < width)
                    {
                        int i = x;
                        while (i < width && line[i] == 0)
                        {
                            ++i;
                        }

                        if (i >= width)
                        {
                            break;
                        }

                        int j = i + 1;
                        while (j < width && line[j] != 0)
                        {
                            ++j;
                        }

                        bw.Write((short)(i - x));
                        bw.Write((short)(j - i));
                        for (int p = i; p < j; ++p)
                        {
                            bw.Write((ushort)(line[p] ^ 0x8000));
                        }

                        x = j;
                    }

                    bw.Write((short)0);
                    bw.Write((short)0);
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            ms.Position = lookupPos;
            foreach (ushort l in lookups)
            {
                bw.Write(l);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Writes artextidx.mul / artext.mul into <paramref name="path"/>. Unchanged images are copied raw
        /// (no decode/encode), identical images are stored once.
        /// </summary>
        public static void SaveExtension(string path)
        {
            if (!HasExtension)
            {
                return;
            }

            string idxFinal = Path.Combine(path, ExtendedFormat.ArtIndexFileName);
            string mulFinal = Path.Combine(path, ExtendedFormat.ArtDataFileName);
            string idxTmp = idxFinal + ".tmp";
            string mulTmp = mulFinal + ".tmp";

            int count = ExtendedCount;
            var entries = new ExtendedFormat.ArtIndexEntry[count];
            var written = new Dictionary<string, ExtendedFormat.ArtIndexEntry>();

            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var fsMul = new FileStream(mulTmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                for (int k = 0; k < count; ++k)
                {
                    Files.FireFileSaveEvent();
                    entries[k] = ExtendedFormat.ArtIndexEntry.Empty;

                    if (_extRemoved[k])
                    {
                        continue;
                    }

                    byte[] data;
                    if (_extCache[k] != null)
                    {
                        data = EncodeStatic(_extCache[k]);
                    }
                    else
                    {
                        data = ReadExtendedRaw(k);
                    }

                    if (data == null || data.Length == 0)
                    {
                        continue;
                    }

                    string key = Convert.ToHexString(sha.ComputeHash(data));
                    if (written.TryGetValue(key, out ExtendedFormat.ArtIndexEntry existing))
                    {
                        entries[k] = existing;
                        continue;
                    }

                    var entry = new ExtendedFormat.ArtIndexEntry { Offset = fsMul.Position, Length = data.Length, Extra = 0 };
                    fsMul.Write(data, 0, data.Length);
                    entries[k] = entry;
                    written[key] = entry;
                }
            }

            using (var fsIdx = new FileStream(idxTmp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                ExtendedFormat.WriteArtIndex(fsIdx, entries);
            }

            bool sameAsSource = _extDataPath != null &&
                string.Equals(Path.GetFullPath(_extDataPath), Path.GetFullPath(mulFinal), StringComparison.OrdinalIgnoreCase);

            if (sameAsSource)
            {
                // the source file gets replaced; the new index (entries) points into the new file afterwards
                CloseExtensionStream();
            }

            File.Move(mulTmp, mulFinal, true);
            File.Move(idxTmp, idxFinal, true);

            if (sameAsSource || _extDataPath == null)
            {
                _extIndex = entries;
                _extDataPath = mulFinal;
                Array.Clear(_extRemoved);
            }
        }
        #endregion
    }
}
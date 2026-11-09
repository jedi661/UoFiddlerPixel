using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Static access to a second art file. Used by CompareLandControl.
    /// The item compare uses its own <see cref="SecondArtFile"/> instance so both tabs do not overwrite each other.
    /// </summary>
    internal static class SecondArt
    {
        private static SecondArtFile _file;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            _file?.Dispose();
            _file = new SecondArtFile(new SecondFileIndex(idxPath, mulPath, SecondArtFile.TotalCount));
        }

        public static int GetMaxItemId() => _file?.GetMaxItemId() ?? 0x3FFF;

        public static bool IsValidStatic(int index) => _file?.IsValidStatic(index) == true;

        public static Bitmap GetStatic(int index) => _file?.GetStatic(index);

        public static bool IsValidLand(int index) => _file?.IsValidLand(index) == true;

        public static Bitmap GetLand(int index) => _file?.GetLand(index);
    }

    /// <summary>
    /// A second art file (art.mul/artidx.mul or artLegacyMUL.uop) loaded from any directory.
    /// </summary>
    internal sealed class SecondArtFile : IDisposable
    {
        public const int LandCount = 0x4000;
        public const int TotalCount = 0x14000;

        private readonly SecondFileIndex _fileIndex;
        private readonly Bitmap[] _cache = new Bitmap[TotalCount];
        private byte[] _streamBuffer;
        private readonly byte[] _validBuffer = new byte[4];

        public SecondArtFile(SecondFileIndex fileIndex)
        {
            _fileIndex = fileIndex;
        }

        /// <summary>
        /// Directory the file was loaded from.
        /// </summary>
        public string Directory { get; private set; }

        /// <summary>
        /// Short description of the loaded format, e.g. "MUL" or "UOP".
        /// </summary>
        public string SourceDescription { get; private set; }

        /// <summary>
        /// Loads the art files of a directory. MUL files are preferred, artLegacyMUL.uop is used as fallback.
        /// File names are searched case-insensitive.
        /// </summary>
        /// <exception cref="FileNotFoundException">No usable art files found.</exception>
        public static SecondArtFile Load(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !System.IO.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory not found:\n{directory}");
            }

            string mulFile = FindFile(directory, "art.mul");
            string idxFile = FindFile(directory, "artidx.mul");
            string uopFile = FindFile(directory, "artLegacyMUL.uop");

            if (mulFile != null && idxFile != null)
            {
                return new SecondArtFile(new SecondFileIndex(idxFile, mulFile, TotalCount))
                {
                    Directory = directory,
                    SourceDescription = $"MUL ({Path.GetFileName(mulFile)} + {Path.GetFileName(idxFile)})"
                };
            }

            if (uopFile != null)
            {
                return new SecondArtFile(SecondFileIndex.FromUop(uopFile, TotalCount, ".tga", 0x13FDC, false))
                {
                    Directory = directory,
                    SourceDescription = $"UOP ({Path.GetFileName(uopFile)})"
                };
            }

            List<string> missing = new List<string>();
            if (mulFile == null)
            {
                missing.Add("art.mul");
            }
            if (idxFile == null)
            {
                missing.Add("artidx.mul");
            }

            throw new FileNotFoundException(
                $"No art files found in:\n{directory}\n\nMissing: {string.Join(", ", missing)}\n" +
                "Alternatively artLegacyMUL.uop can be used.");
        }

        /// <summary>
        /// Finds a file in a directory ignoring the case of the file name.
        /// </summary>
        public static string FindFile(string directory, string fileName)
        {
            string direct = Path.Combine(directory, fileName);
            if (File.Exists(direct))
            {
                return direct;
            }

            try
            {
                return System.IO.Directory.EnumerateFiles(directory)
                    .FirstOrDefault(f => string.Equals(Path.GetFileName(f), fileName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public int GetMaxItemId()
        {
            long idxLength = _fileIndex.IdxLength / 12;

            // High Seas (or bigger)
            if (idxLength >= 0x13FDC)
            {
                return 0xFFDC;
            }

            // Stygian Abyss
            if (idxLength == 0xC000)
            {
                return 0x7FFF;
            }

            // ML and older
            return 0x3FFF;
        }

        /// <summary>
        /// Returns true when the id lies inside the range of this file.
        /// Ids outside the range are NOT mapped to item 0 (that was a bug in the old implementation).
        /// </summary>
        public bool IsInRange(int itemId)
        {
            return itemId >= 0 && itemId <= GetMaxItemId();
        }

        public bool IsValidStatic(int itemId)
        {
            if (!IsInRange(itemId))
            {
                return false;
            }

            int index = itemId + LandCount;

            if (_cache[index] != null)
            {
                return true;
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _);
            if (stream == null || length < 8)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Current);
            if (stream.Read(_validBuffer, 0, 4) != 4)
            {
                return false;
            }

            short width = (short)(_validBuffer[0] | (_validBuffer[1] << 8));
            short height = (short)(_validBuffer[2] | (_validBuffer[3] << 8));

            return width > 0 && height > 0;
        }

        public Bitmap GetStatic(int itemId)
        {
            if (!IsInRange(itemId))
            {
                return null;
            }

            int index = itemId + LandCount;

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _);
            if (stream == null)
            {
                return null;
            }

            Bitmap bmp = LoadStatic(stream, length);
            if (Files.CacheData)
            {
                _cache[index] = bmp;
            }
            return bmp;
        }

        private bool ReadToBuffer(Stream stream, int length)
        {
            if (length <= 0)
            {
                return false;
            }

            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            int read = 0;
            while (read < length)
            {
                int r = stream.Read(_streamBuffer, read, length - read);
                if (r <= 0)
                {
                    return false;
                }
                read += r;
            }
            return true;
        }

        private unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            if (!ReadToBuffer(stream, length) || length < 8)
            {
                return null;
            }

            int maxCount = length / 2; // number of ushorts in the buffer

            fixed (byte* data = _streamBuffer)
            {
                ushort* binData = (ushort*)data;
                int count = 2;
                int width = binData[count++];
                int height = binData[count++];

                if (width <= 0 || height <= 0 || 4 + height > maxCount)
                {
                    return null;
                }

                int[] lookups = new int[height];
                int start = height + 4;

                for (int i = 0; i < height; ++i)
                {
                    lookups[i] = start + binData[count++];
                }

                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                try
                {
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;

                    for (int y = 0; y < height; ++y, line += delta)
                    {
                        count = lookups[y];

                        ushort* cur = line;
                        ushort* lineEnd = line + width;

                        while (count + 1 < maxCount)
                        {
                            int xOffset = binData[count++];
                            int xRun = binData[count++];
                            if (xOffset + xRun == 0)
                            {
                                break;
                            }

                            cur += xOffset;
                            if (cur + xRun > lineEnd || count + xRun > maxCount)
                            {
                                break; // corrupt data
                            }

                            ushort* end = cur + xRun;
                            while (cur < end)
                            {
                                *cur++ = (ushort)(binData[count++] ^ 0x8000);
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(bd);
                }
                return bmp;
            }
        }

        public bool IsValidLand(int index)
        {
            index &= 0x3FFF;
            return _cache[index] != null || _fileIndex.Valid(index, out _, out _);
        }

        public Bitmap GetLand(int index)
        {
            index &= 0x3FFF;

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _);
            if (stream == null)
            {
                return null;
            }

            Bitmap bmp = LoadLand(stream, length);
            if (Files.CacheData)
            {
                _cache[index] = bmp;
            }
            return bmp;
        }

        private unsafe Bitmap LoadLand(Stream stream, int length)
        {
            // A land tile always contains 1012 pixels (2024 bytes).
            if (!ReadToBuffer(stream, length) || length < 2024)
            {
                return null;
            }

            Bitmap bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                fixed (byte* binData = _streamBuffer)
                {
                    ushort* bdata = (ushort*)binData;
                    int xOffset = 21;
                    int xRun = 2;

                    ushort* line = (ushort*)bd.Scan0;
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
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        public void Dispose()
        {
            _fileIndex?.Dispose();
            Array.Clear(_cache, 0, _cache.Length);
        }
    }
}

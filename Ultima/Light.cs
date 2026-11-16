/***************************************************************************
 *
 * light.mul / lightidx.mul
 *
 *  lightidx.mul: 12 bytes per light – lookup (int), length (int), extra (int)
 *                extra: width = extra & 0xFFFF, height = (extra >> 16) & 0xFFFF
 *  light.mul:    width * height bytes, row by row. One byte per pixel:
 *                0 = no light, 1..31 = brightness (5 bit).
 *                Values above 31 are "negative" lights stored bit-inverted;
 *                ClassicUO reads them as (~value & 0x1F).
 *
 * The number of lights is taken from the length of lightidx.mul (the classic
 * file has 100 entries). ClassicUO reads all entries of the idx file, so the
 * file can be extended with <see cref="SetCount"/>.
 *
 * Changed lights are kept as raw bytes, so saving does not change values
 * (the old bitmap round trip lost one brightness step).
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class Light
    {
        /// <summary>number of entries of the classic light files</summary>
        public const int DefaultCount = 100;

        /// <summary>highest number of entries the editor allows (light ids are 16 bit in the tiledata of ClassicUO)</summary>
        public const int MaxCount = 0x10000;

        public const int MaxValue = 31;

        private static FileIndex _fileIndex;
        private static int _count;
        private static Bitmap[] _cache;
        private static byte[][] _raw;
        private static Size[] _rawSize;
        private static bool[] _removed;

        static Light()
        {
            Reload();
        }

        /// <summary>
        /// ReReads light.mul
        /// </summary>
        public static void Reload()
        {
            int fileCount = GetFileCount();
            _count = Math.Max(DefaultCount, fileCount);
            _fileIndex = new FileIndex("lightidx.mul", "light.mul", _count, -1);
            _cache = new Bitmap[_count];
            _raw = new byte[_count][];
            _rawSize = new Size[_count];
            _removed = new bool[_count];
        }

        /// <summary>number of entries (at least 100, more when the idx file is longer or was extended)</summary>
        public static int Count => _count;

        /// <summary>number of entries of lightidx.mul on disk (0 = not found)</summary>
        public static int GetFileCount()
        {
            string idxPath = Files.GetFilePath("lightidx.mul");
            if (idxPath == null || !File.Exists(idxPath))
            {
                return 0;
            }

            return (int)(new FileInfo(idxPath).Length / 12);
        }

        /// <summary>
        /// Gets count of defined lights (kept for older callers: the number of entries)
        /// </summary>
        public static int GetCount() => _count;

        /// <summary>
        /// Changes the number of entries. More entries are empty until a light is inserted;
        /// fewer entries drop the lights at the end. Written with <see cref="Save"/>.
        /// </summary>
        public static void SetCount(int count)
        {
            count = Math.Max(1, Math.Min(MaxCount, count));
            if (count == _count)
            {
                return;
            }

            // lights that are only in the file are read before the arrays change
            for (int i = 0; i < Math.Min(count, _count); i++)
            {
                if (_raw[i] == null && !_removed[i])
                {
                    byte[] data = ReadFromFile(i, out int w, out int h);
                    if (data != null)
                    {
                        _raw[i] = data;
                        _rawSize[i] = new Size(w, h);
                    }
                }
            }

            Array.Resize(ref _cache, count);
            Array.Resize(ref _raw, count);
            Array.Resize(ref _rawSize, count);
            Array.Resize(ref _removed, count);

            // entries behind the old end are not in the file
            for (int i = _count; i < count; i++)
            {
                _removed[i] = true;
            }

            _count = count;
        }

        private static bool InRange(int index) => index >= 0 && index < _count;

        /// <summary>
        /// Tests if given index is valid
        /// </summary>
        public static bool TestLight(int index)
        {
            if (!InRange(index) || _removed[index])
            {
                return false;
            }

            if (_raw[index] != null)
            {
                return _rawSize[index].Width > 0 && _rawSize[index].Height > 0;
            }

            Stream stream = _fileIndex.Seek(index, out int _, out int extra, out bool _);
            if (stream == null)
            {
                return false;
            }

            stream.Close();
            int width = extra & 0xFFFF;
            int height = (extra >> 16) & 0xFFFF;
            return width > 0 && height > 0;
        }

        /// <summary>
        /// Removes Light
        /// </summary>
        public static void Remove(int index)
        {
            if (!InRange(index))
            {
                return;
            }

            _removed[index] = true;
            _raw[index] = null;
            _cache[index] = null;
        }

        /// <summary>
        /// Replaces Light with a bitmap: the brightness of every pixel becomes the light value 0..31
        /// (black = no light). The entry count grows when the index is behind the end.
        /// </summary>
        public static void Replace(int index, Bitmap bmp)
        {
            if (bmp == null)
            {
                return;
            }

            ReplaceRaw(index, FromBitmap(bmp), bmp.Width, bmp.Height);
        }

        /// <summary>Replaces Light with raw values (one byte per pixel, 0..31)</summary>
        public static void ReplaceRaw(int index, byte[] data, int width, int height)
        {
            if (index < 0 || index >= MaxCount || data == null || width <= 0 || height <= 0 || data.Length < width * height)
            {
                throw new ArgumentException("Invalid light data");
            }

            if (index >= _count)
            {
                SetCount(index + 1);
            }

            _raw[index] = (byte[])data.Clone();
            _rawSize[index] = new Size(width, height);
            _removed[index] = false;
            _cache[index] = null;
        }

        private static byte[] ReadFromFile(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            Stream stream = _fileIndex.Seek(index, out int length, out int extra, out bool _);
            if (stream == null)
            {
                return null;
            }

            width = extra & 0xFFFF;
            height = (extra >> 16) & 0xFFFF;
            var buffer = new byte[Math.Max(0, length)];
            int read = 0;
            while (read < buffer.Length)
            {
                int n = stream.Read(buffer, read, buffer.Length - read);
                if (n <= 0)
                {
                    break;
                }

                read += n;
            }

            stream.Close();
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            if (buffer.Length < width * height)
            {
                Array.Resize(ref buffer, width * height);
            }

            return buffer;
        }

        /// <summary>raw bytes of a light (one byte per pixel, row by row), null = no light</summary>
        public static byte[] GetRawLight(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (!InRange(index) || _removed[index])
            {
                return null;
            }

            if (_raw[index] != null)
            {
                width = _rawSize[index].Width;
                height = _rawSize[index].Height;
                return _raw[index];
            }

            return ReadFromFile(index, out width, out height);
        }

        /// <summary>light value 0..31 of a stored byte (like ClassicUO: values above 31 are bit-inverted)</summary>
        public static int DecodeValue(byte stored) => stored > 0x1F ? ~stored & 0x1F : stored;

        /// <summary>
        /// Returns Bitmap of given index: grey scale, black = no light, white = full light
        /// </summary>
        public static Bitmap GetLight(int index)
        {
            if (!InRange(index) || _removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            byte[] data = GetRawLight(index, out int width, out int height);
            if (data == null)
            {
                return null;
            }

            Bitmap bmp = ToBitmap(data, width, height);
            if (Files.CacheData)
            {
                _cache[index] = bmp;
            }

            return bmp;
        }

        /// <summary>raw values → grey scale bitmap (32 bit)</summary>
        public static unsafe Bitmap ToBitmap(byte[] data, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    uint* line = (uint*)((byte*)bd.Scan0 + (y * bd.Stride));
                    for (int x = 0; x < width; x++)
                    {
                        int i = (y * width) + x;
                        int v = i < data.Length ? DecodeValue(data[i]) : 0;
                        uint g = (uint)((v * 255) / MaxValue);
                        line[x] = 0xFF000000 | (g << 16) | (g << 8) | g;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            return bmp;
        }

        /// <summary>bitmap → raw values: brightness 0..255 becomes 0..31, transparent pixels are 0</summary>
        public static unsafe byte[] FromBitmap(Bitmap bmp)
        {
            int width = bmp.Width, height = bmp.Height;
            var data = new byte[width * height];
            using (var copy = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(copy))
                {
                    g.DrawImage(bmp, new Rectangle(0, 0, width, height));
                }

                BitmapData bd = copy.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    for (int y = 0; y < height; y++)
                    {
                        uint* line = (uint*)((byte*)bd.Scan0 + (y * bd.Stride));
                        for (int x = 0; x < width; x++)
                        {
                            uint c = line[x];
                            int a = (int)(c >> 24);
                            int r = (int)((c >> 16) & 0xFF), gr = (int)((c >> 8) & 0xFF), b = (int)(c & 0xFF);
                            int lum = (Math.Max(r, Math.Max(gr, b)) * a) / 255;
                            data[(y * width) + x] = (byte)Math.Min(MaxValue, (lum * MaxValue + 127) / 255);
                        }
                    }
                }
                finally
                {
                    copy.UnlockBits(bd);
                }
            }

            return data;
        }

        /// <summary>
        /// Writes light.mul / lightidx.mul with <see cref="Count"/> entries
        /// </summary>
        public static void Save(string path)
        {
            string idx = Path.Combine(path, "lightidx.mul");
            string mul = Path.Combine(path, "light.mul");

            // all data is read before the files are written (the files may be the source files)
            var entries = new byte[_count][];
            var sizes = new Size[_count];
            for (int index = 0; index < _count; index++)
            {
                byte[] data = GetRawLight(index, out int w, out int h);
                if (data != null && w > 0 && h > 0)
                {
                    entries[index] = data;
                    sizes[index] = new Size(w, h);
                }
            }

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                for (int index = 0; index < _count; index++)
                {
                    byte[] data = entries[index];
                    if (data == null)
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(0); // length
                        binidx.Write(0); // extra
                        continue;
                    }

                    int length = sizes[index].Width * sizes[index].Height;
                    binidx.Write((int)fsmul.Position); // lookup
                    binidx.Write(length);
                    binidx.Write((sizes[index].Height << 16) | (sizes[index].Width & 0xFFFF)); // extra: height high, width low
                    binmul.Write(data, 0, length);
                }
            }
        }
    }
}

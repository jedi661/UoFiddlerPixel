using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace Ultima
{
    public sealed class Textures
    {
        private static FileIndex _fileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
        private static Bitmap[] _cache = new Bitmap[0x4000];
        private static bool[] _removed = new bool[0x4000];
        private static readonly Dictionary<int, bool> _patched = new Dictionary<int, bool>();

        // ===================================================================================================
        // TEXTURE EXTENSION (ids 0x4000 .. up to 0x1003FFF): texextidx.mul / texmapsext.mul, same raw encoding as
        // texmaps.mul (64x64 or 128x128 pixels, colour ^ 0x8000, extra 0 = 64, 1 = 128). texidx.mul / texmaps.mul
        // keep their client layout.
        // ===================================================================================================
        public static readonly ExtendedBlobStore Extension = new ExtendedBlobStore(
            ExtendedFormat.TextureIndexFileName, ExtendedFormat.TextureDataFileName, ExtendedFormat.TextureIndexMagic,
            ExtendedFormat.TextureBaseId, ExtendedFormat.TextureMaxSlots);

        private static readonly Dictionary<int, Bitmap> _extCache = new Dictionary<int, Bitmap>();

        public static bool HasExtension => Extension.Exists;

        /// <summary>Number of classic texture slots (0x4000).</summary>
        public const int LegacyCount = 0x4000;

        /// <summary>Highest texture id incl. the extension (0x3FFF without).</summary>
        public static int GetMaxTextureId() => Extension.Exists ? Extension.MaxId : LegacyCount - 1;

        public static bool IsLegalTextureId(int id) => (id >= 0 && id < LegacyCount) || Extension.Contains(id);

        public static void CreateOrResizeExtension(int slots)
        {
            Extension.CreateOrResize(slots);
            foreach (int id in new List<int>(_extCache.Keys))
            {
                if (!Extension.Contains(id))
                {
                    _extCache.Remove(id);
                }
            }
        }

        public static void ReloadExtension()
        {
            _extCache.Clear();
            Extension.Load();
        }

        public static void SaveExtension(string path)
        {
            Extension.Save(path);
        }

        /// <summary>Encodes a 64x64 or 128x128 texture (other sizes are drawn into 64x64 / 128x128).</summary>
        public static unsafe byte[] EncodeTexture(Bitmap source, out int extra)
        {
            int size = source.Width > 64 || source.Height > 64 ? 128 : 64;
            extra = size == 128 ? 1 : 0;
            Bitmap bmp = source;
            bool own = false;
            if (bmp.Width != size || bmp.Height != size)
            {
                bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImageUnscaled(source, 0, 0);
                }

                own = true;
            }

            try
            {
                var result = new byte[size * size * 2];
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                try
                {
                    var line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    int o = 0;
                    for (int y = 0; y < size; ++y, line += delta)
                    {
                        for (int x = 0; x < size; ++x)
                        {
                            ushort v = (ushort)(line[x] ^ 0x8000);
                            result[o++] = (byte)v;
                            result[o++] = (byte)(v >> 8);
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(bd);
                }

                return result;
            }
            finally
            {
                if (own)
                {
                    bmp.Dispose();
                }
            }
        }

        private static unsafe Bitmap DecodeTexture(byte[] data, int extra)
        {
            int size = extra == 0 && data.Length < 0x8000 ? 64 : 128;
            if (data.Length < size * size * 2)
            {
                return null;
            }

            var bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            fixed (byte* p = data)
            {
                var src = (ushort*)p;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    for (int x = 0; x < size; ++x)
                    {
                        line[x] = (ushort)(*src++ ^ 0x8000);
                    }
                }
            }

            bmp.UnlockBits(bd);
            return bmp;
        }

        private static Bitmap GetExtended(int id)
        {
            if (_extCache.TryGetValue(id, out Bitmap cached))
            {
                return cached;
            }

            byte[] raw = Extension.Read(id, out int extra);
            if (raw == null)
            {
                return null;
            }

            Bitmap bmp = DecodeTexture(raw, extra);
            if (bmp != null && Files.CacheData)
            {
                _extCache[id] = bmp;
            }

            return bmp;
        }

        private struct Checksums
        {
            public byte[] Checksum;
            public int Position;
            public int Length;
            public int Extra;
        }

        /// <summary>
        /// ReReads texmaps
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
            _cache = new Bitmap[0x4000];
            _removed = new bool[0x4000];
            _patched.Clear();
            ReloadExtension();
        }

        static Textures()
        {
            ReloadExtension();
        }

        public static int GetIdxLength()
        {
            return (int)(_fileIndex.IdxLength / 12);
        }

        /// <summary>
        /// Removes Texture <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void Remove(int index)
        {
            if (Extension.Contains(index))
            {
                Extension.Remove(index);
                _extCache.Remove(index);
                return;
            }

            _removed[index] = true;
        }

        /// <summary>
        /// Replaces Texture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void Replace(int index, Bitmap bmp)
        {
            if (Extension.Contains(index))
            {
                if (bmp == null)
                {
                    Remove(index);
                    return;
                }

                Extension.Set(index, EncodeTexture(bmp, out int extra), extra);
                _extCache[index] = bmp;
                return;
            }

            _cache[index] = bmp;
            _removed[index] = false;
            if (_patched.ContainsKey(index))
            {
                _patched.Remove(index);
            }
        }

        /// <summary>
        /// Tests if index is valid Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TestTexture(int index)
        {
            if (Extension.Contains(index))
            {
                return Extension.IsValid(index);
            }

            index &= 0x3FFF;

            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            bool valid = _fileIndex.Valid(index, out int length, out int _, out bool _);

            return valid && (length != 0);
        }

        /// <summary>
        /// Returns Bitmap of Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetTexture(int index)
        {
            return GetTexture(index, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of Texture with verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static unsafe Bitmap GetTexture(int index, out bool patched)
        {
            if (Extension.Contains(index))
            {
                patched = false;
                return GetExtended(index);
            }

            index &= 0x3FFF;
            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int extra, out patched);
            if (stream == null)
            {
                return null;
            }

            if (length == 0)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            int size = extra == 0 ? 64 : 128;

            var bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int max = size * size * 2;

            byte[] streamBuffer = new byte[max];

            stream.Read(streamBuffer, 0, max);

            fixed (byte* data = streamBuffer)
            {
                var binData = (ushort*)data;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + size;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*binData++ ^ 0x8000);
                    }
                }
            }

            bmp.UnlockBits(bd);

            stream.Close();

            if (!Files.CacheData)
            {
                return _cache[index] = bmp;
            }

            return bmp;
        }

        public static unsafe void Save(string path)
        {
            string idx = Path.Combine(path, "texidx.mul");
            string mul = Path.Combine(path, "texmaps.mul");

            List<Checksums> checksumList = new List<Checksums>();

            var memIdx = new MemoryStream();
            var memMul = new MemoryStream();

            using (var binIdx = new BinaryWriter(memIdx))
            using (var binMul = new BinaryWriter(memMul))
            {
                for (int index = 0; index < GetIdxLength(); ++index)
                {
                    if (_cache[index] == null)
                    {
                        _cache[index] = GetTexture(index);
                    }

                    Bitmap bmp = _cache[index];
                    if ((bmp == null) || (_removed[index]))
                    {
                        binIdx.Write(0); // lookup
                        binIdx.Write(0); // length
                        binIdx.Write(0); // extra
                    }
                    else
                    {
                        byte[] newChecksum;
                        using (var sha = SHA256.Create())
                        using (var ms = new MemoryStream())
                        {
                            bmp.Save(ms, ImageFormat.Bmp);
                            newChecksum = sha.ComputeHash(ms.ToArray());
                        }

                        if (CompareSaveImages(checksumList, newChecksum, out Checksums sum))
                        {
                            binIdx.Write(sum.Position); // lookup
                            binIdx.Write(sum.Length); // length
                            binIdx.Write(sum.Extra); // extra

                            continue;
                        }

                        BitmapData bd = bmp.LockBits(
                            new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                            PixelFormat.Format16bppArgb1555);
                        var line = (ushort*)bd.Scan0;
                        int delta = bd.Stride >> 1;

                        binIdx.Write((int)binMul.BaseStream.Position); // lookup
                        var length = (int)binMul.BaseStream.Position;

                        for (int y = 0; y < bmp.Height; ++y, line += delta)
                        {
                            ushort* cur = line;
                            for (int x = 0; x < bmp.Width; ++x)
                            {
                                binMul.Write((ushort)(cur[x] ^ 0x8000));
                            }
                        }

                        int start = length;
                        length = (int)binMul.BaseStream.Position - length;
                        binIdx.Write(length);
                        var extra = GetExtraFlag(length);
                        binIdx.Write(extra);
                        bmp.UnlockBits(bd);

                        checksumList.Add(new Checksums
                        {
                            Position = start,
                            Length = length,
                            Checksum = newChecksum,
                            Extra = extra
                        });
                    }
                }

                using (var fileIdx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var fileMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    memIdx.WriteTo(fileIdx);
                    memMul.WriteTo(fileMul);
                }
            }

            memIdx.Dispose();

            // ids >= 0x4000 go to the side-car files, texidx.mul/texmaps.mul stay client compatible
            SaveExtension(path);
        }

        private static int GetExtraFlag(int length)
        {
            // length of 0x8000 == width 128x128 else 64x64
            return length == 0x8000 ? 1 : 0;
        }

        private static bool CompareSaveImages(IReadOnlyList<Checksums> checksumList, IReadOnlyList<byte> newChecksum, out Checksums sum)
        {
            sum = new Checksums();
            for (int i = 0; i < checksumList.Count; ++i)
            {
                byte[] cmp = checksumList[i].Checksum;
                if ((cmp == null) || (newChecksum == null) || (cmp.Length != newChecksum.Count))
                {
                    return false;
                }

                bool valid = true;

                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] == newChecksum[j])
                    {
                        continue;
                    }

                    valid = false;
                    break;
                }

                if (!valid)
                {
                    continue;
                }

                sum = checksumList[i];
                return true;
            }

            return false;
        }
    }
}
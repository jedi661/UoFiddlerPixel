using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Index of a "second" (foreign) mul/uop file, independent of the files loaded by Ultima.Files.
    /// </summary>
    public sealed class SecondFileIndex : IDisposable
    {
        private readonly string _mulPath;
        private Entry3D[] Index { get; }
        private Stream Stream { get; set; }

        public long IdxLength { get; private set; }

        /// <summary>
        /// True when the data comes from a .uop container.
        /// </summary>
        public bool IsUop { get; private set; }

        /// <summary>
        /// Path of the data file (mul or uop) or null when nothing could be loaded.
        /// </summary>
        public string DataPath => _mulPath;

        public Stream Seek(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return null;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0 || e.Length < 0)
            {
                length = extra = 0;
                return null;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if (Stream?.CanRead != true || !Stream.CanSeek)
            {
                Stream = _mulPath == null
                    ? null
                    : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream == null || e.Lookup >= Stream.Length)
            {
                length = extra = 0;
                return null;
            }

            Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return Stream;
        }

        public bool Valid(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return false;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0 || e.Length < 0)
            {
                length = extra = 0;
                return false;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if (_mulPath == null || !File.Exists(_mulPath))
            {
                length = extra = 0;
                return false;
            }

            if (Stream?.CanRead != true || !Stream.CanSeek)
            {
                Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream.Length >= e.Lookup)
            {
                return true;
            }

            length = extra = 0;
            return false;
        }

        /// <summary>
        /// Loads a classic idx/mul pair.
        /// </summary>
        public SecondFileIndex(string idxFile, string mulFile, int length)
        {
            Index = new Entry3D[length];

            _mulPath = mulFile;
            if (!File.Exists(idxFile))
            {
                idxFile = null;
            }

            if (!File.Exists(_mulPath))
            {
                _mulPath = null;
            }

            if (idxFile != null && _mulPath != null)
            {
                using (FileStream index = new FileStream(idxFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    int count = (int)(index.Length / 12);
                    IdxLength = index.Length;

                    // Only copy as many entries as fit into the index (a bigger idx file must not overflow the buffer).
                    int bytesToCopy = (int)Math.Min(IdxLength, (long)length * 12);
                    byte[] buffer = new byte[bytesToCopy];
                    int read = 0;
                    while (read < bytesToCopy)
                    {
                        int r = index.Read(buffer, read, bytesToCopy - read);
                        if (r <= 0)
                        {
                            break;
                        }
                        read += r;
                    }

                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    try
                    {
                        Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), read);
                    }
                    finally
                    {
                        gc.Free();
                    }

                    for (int i = Math.Min(count, length); i < length; ++i)
                    {
                        Index[i].Lookup = -1;
                        Index[i].Length = -1;
                        Index[i].Extra = -1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    Index[i].Lookup = -1;
                    Index[i].Length = -1;
                    Index[i].Extra = -1;
                }
                Stream = null;
            }
        }

        private SecondFileIndex(string uopFile, Entry3D[] index, long idxLength)
        {
            _mulPath = uopFile;
            Index = index;
            IdxLength = idxLength;
            IsUop = true;
        }

        /// <summary>
        /// Loads a legacy uop container (e.g. artLegacyMUL.uop) by using the uop reader of the Ultima library.
        /// Compressed entries are not supported and are treated as missing.
        /// </summary>
        public static SecondFileIndex FromUop(string uopFile, int length, string entryExtension, int idxLength, bool hasExtra)
        {
            Ultima.UopFileAccessor accessor = new Ultima.UopFileAccessor(uopFile, entryExtension, length, idxLength, hasExtra);
            try
            {
                Entry3D[] index = new Entry3D[length];
                for (int i = 0; i < length; ++i)
                {
                    Ultima.IEntry e = accessor.GetEntry(i);
                    bool usable = e != null && e.Lookup >= 0 && e.Length >= 0 && e.Flag == Ultima.CompressionFlag.None;
                    index[i].Lookup = usable ? e.Lookup : -1;
                    index[i].Length = usable ? e.Length : -1;
                    index[i].Extra = usable ? e.Extra : -1;
                }

                return new SecondFileIndex(uopFile, index, accessor.IdxLength);
            }
            finally
            {
                accessor.Stream?.Dispose();
            }
        }

        public void Dispose()
        {
            Stream?.Dispose();
            Stream = null;
        }
    }

    public struct Entry3D
    {
        public int Lookup;
        public int Length;
        public int Extra;
    }
}

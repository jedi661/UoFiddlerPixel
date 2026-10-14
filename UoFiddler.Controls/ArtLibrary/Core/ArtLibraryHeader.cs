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
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace UoFiddler.Controls.ArtLibrary.Core
{
    /// <summary>
    /// Fixed 64 byte header at the start of every *.artlib.idx file (little endian):
    /// <code>
    /// 0  char[8]  Magic      "UOFARTLB"
    /// 8  int32    Version    1
    /// 12 int32    EntrySize  16
    /// 16 int32    Capacity   number of ids (index entries following the header)
    /// 20 int32    Kind       ArtLibraryKind
    /// 24 int64    Created    unix time in milliseconds (UTC)
    /// 32 char[32] Name       display name, UTF-8, zero padded
    /// </code>
    /// </summary>
    public sealed class ArtLibraryHeader
    {
        public const int Size = 64;
        public const int CurrentVersion = 1;
        public const int NameByteLength = 32;

        private const int CapacityOffset = 16;
        private const int NameOffset = 32;

        private static readonly byte[] _magic = Encoding.ASCII.GetBytes("UOFARTLB");

        public int Version { get; set; } = CurrentVersion;
        public int EntrySize { get; set; } = ArtLibraryEntry.Size;
        public int Capacity { get; set; }
        public ArtLibraryKind Kind { get; set; } = ArtLibraryKind.ItemArt;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// File offset of the capacity field, used to update it in place when the library grows.
        /// </summary>
        public static long CapacityFieldOffset => CapacityOffset;

        /// <summary>
        /// File offset of the name field, used to rename a library in place.
        /// </summary>
        public static long NameFieldOffset => NameOffset;

        public static ArtLibraryHeader Read(Stream stream)
        {
            var buffer = new byte[Size];
            stream.Seek(0, SeekOrigin.Begin);
            stream.ReadExactly(buffer, 0, Size);

            ReadOnlySpan<byte> span = buffer;
            if (!span.Slice(0, _magic.Length).SequenceEqual(_magic.AsSpan()))
            {
                throw new InvalidDataException("The file is not an art library (*.artlib.idx): wrong magic.");
            }

            var header = new ArtLibraryHeader
            {
                Version = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(8)),
                EntrySize = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(12)),
                Capacity = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(CapacityOffset)),
                Kind = (ArtLibraryKind)BinaryPrimitives.ReadInt32LittleEndian(span.Slice(20)),
                Created = DateTimeOffset.FromUnixTimeMilliseconds(BinaryPrimitives.ReadInt64LittleEndian(span.Slice(24))).UtcDateTime,
                Name = DecodeName(span.Slice(NameOffset, NameByteLength))
            };

            if (header.Version > CurrentVersion)
            {
                throw new InvalidDataException($"Art library version {header.Version} is newer than supported version {CurrentVersion}.");
            }

            if (header.EntrySize != ArtLibraryEntry.Size)
            {
                throw new InvalidDataException($"Unsupported entry size {header.EntrySize} (expected {ArtLibraryEntry.Size}).");
            }

            if (header.Capacity < 0)
            {
                throw new InvalidDataException("Invalid capacity in art library header.");
            }

            return header;
        }

        public void Write(Stream stream)
        {
            var buffer = new byte[Size];
            Span<byte> span = buffer;

            _magic.CopyTo(span);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(8), Version);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(12), EntrySize);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(CapacityOffset), Capacity);
            BinaryPrimitives.WriteInt32LittleEndian(span.Slice(20), (int)Kind);
            BinaryPrimitives.WriteInt64LittleEndian(span.Slice(24), new DateTimeOffset(Created.ToUniversalTime()).ToUnixTimeMilliseconds());
            EncodeName(Name).CopyTo(span.Slice(NameOffset, NameByteLength));

            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(buffer, 0, Size);
        }

        /// <summary>
        /// Encodes a name to exactly <see cref="NameByteLength"/> bytes (UTF-8, zero padded),
        /// truncated without splitting a multi-byte character.
        /// </summary>
        public static byte[] EncodeName(string name)
        {
            var result = new byte[NameByteLength];
            if (string.IsNullOrEmpty(name))
            {
                return result;
            }

            int length = 0;
            foreach (var rune in name.EnumerateRunes())
            {
                int runeLength = rune.Utf8SequenceLength;
                if (length + runeLength > NameByteLength)
                {
                    break;
                }

                rune.EncodeToUtf8(result.AsSpan(length));
                length += runeLength;
            }

            return result;
        }

        private static string DecodeName(ReadOnlySpan<byte> bytes)
        {
            int end = bytes.IndexOf((byte)0);
            if (end < 0)
            {
                end = bytes.Length;
            }

            return Encoding.UTF8.GetString(bytes.Slice(0, end));
        }
    }
}

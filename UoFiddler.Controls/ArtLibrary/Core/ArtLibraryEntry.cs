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

namespace UoFiddler.Controls.ArtLibrary.Core
{
    /// <summary>
    /// One index entry of a *.artlib.idx file (16 bytes, little endian):
    /// <code>
    /// int64 Offset   position of the graphic in *.artlib.mul, -1 = empty
    /// int32 Length   length of the graphic data in bytes
    /// int32 Flags    reserved (see ArtLibraryEntryFlags)
    /// </code>
    /// Entry number i is graphic id i.
    /// </summary>
    public struct ArtLibraryEntry
    {
        public const int Size = 16;

        public long Offset;
        public int Length;
        public int Flags;

        public static ArtLibraryEntry Empty => new ArtLibraryEntry { Offset = -1, Length = 0, Flags = 0 };

        public bool IsEmpty => Offset < 0 || Length <= 0;

        public static ArtLibraryEntry Read(ReadOnlySpan<byte> buffer)
        {
            return new ArtLibraryEntry
            {
                Offset = BinaryPrimitives.ReadInt64LittleEndian(buffer),
                Length = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(8)),
                Flags = BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(12))
            };
        }

        public void Write(Span<byte> buffer)
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer, Offset);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(8), Length);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(12), Flags);
        }
    }

    /// <summary>
    /// Bit flags stored in <see cref="ArtLibraryEntry.Flags"/>.
    /// </summary>
    [Flags]
    public enum ArtLibraryEntryFlags
    {
        None = 0,
        Favorite = 1
    }
}

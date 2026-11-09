using System;
using System.IO;
using System.Runtime.InteropServices;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Read-only tiledata.mul of a second client directory.
    /// The format (old = pre High Seas, new = High Seas and later) is detected from the file size.
    /// </summary>
    internal sealed class SecondTileData
    {
        private const int LandBlocks = 512;

        public string FilePath { get; private set; }
        public bool IsNewFormat { get; private set; }
        public LandData[] LandTable { get; private set; }
        public ItemData[] ItemTable { get; private set; }

        public string FormatDescription => IsNewFormat ? "High Seas (64 bit flags)" : "pre High Seas (32 bit flags)";

        public bool HasItem(int id) => ItemTable != null && id >= 0 && id < ItemTable.Length;

        /// <summary>
        /// Loads tiledata.mul from the directory. Returns null when there is no tiledata.mul.
        /// </summary>
        /// <param name="directory">Directory with the client files.</param>
        /// <param name="preferNewFormat">Used only when the file size fits both formats.</param>
        public static SecondTileData Load(string directory, bool preferNewFormat)
        {
            string path = SecondArtFile.FindFile(directory, "tiledata.mul");
            if (path == null)
            {
                return null;
            }

            byte[] buffer = File.ReadAllBytes(path);
            bool isNew = DetectFormat(buffer.Length, preferNewFormat);

            SecondTileData result = new SecondTileData
            {
                FilePath = path,
                IsNewFormat = isNew
            };
            result.Parse(buffer);
            return result;
        }

        private static unsafe bool DetectFormat(long length, bool preferNew)
        {
            int newLand = LandBlocks * (4 + (32 * sizeof(NewLandTileDataMul)));
            int oldLand = LandBlocks * (4 + (32 * sizeof(OldLandTileDataMul)));
            int newItemBlock = 4 + (32 * sizeof(NewItemTileDataMul));
            int oldItemBlock = 4 + (32 * sizeof(OldItemTileDataMul));

            bool fitsNew = length > newLand && (length - newLand) % newItemBlock == 0;
            bool fitsOld = length > oldLand && (length - oldLand) % oldItemBlock == 0;

            if (fitsNew && !fitsOld)
            {
                return true;
            }

            if (fitsOld && !fitsNew)
            {
                return false;
            }

            return preferNew;
        }

        private unsafe void Parse(byte[] buffer)
        {
            LandTable = new LandData[0x4000];
            GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr basePtr = gc.AddrOfPinnedObject();
                long pos = 0;
                int landSize = IsNewFormat ? sizeof(NewLandTileDataMul) : sizeof(OldLandTileDataMul);

                for (int i = 0; i < 0x4000; i += 32)
                {
                    pos += 4; // block header
                    for (int c = 0; c < 32; ++c)
                    {
                        if (pos + landSize > buffer.Length)
                        {
                            break;
                        }

                        IntPtr ptr = basePtr + (int)pos;
                        LandTable[i + c] = IsNewFormat
                            ? new LandData(Marshal.PtrToStructure<NewLandTileDataMul>(ptr))
                            : new LandData(Marshal.PtrToStructure<OldLandTileDataMul>(ptr));
                        pos += landSize;
                    }
                }

                int itemSize = IsNewFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);
                long remaining = Math.Max(0, buffer.Length - pos);
                int blocks = (int)(remaining / ((itemSize * 32) + 4));
                ItemTable = new ItemData[blocks * 32];

                for (int i = 0; i < ItemTable.Length; i += 32)
                {
                    pos += 4; // block header
                    for (int c = 0; c < 32; ++c)
                    {
                        IntPtr ptr = basePtr + (int)pos;
                        ItemTable[i + c] = IsNewFormat
                            ? new ItemData(Marshal.PtrToStructure<NewItemTileDataMul>(ptr))
                            : new ItemData(Marshal.PtrToStructure<OldItemTileDataMul>(ptr));
                        pos += itemSize;
                    }
                }
            }
            finally
            {
                gc.Free();
            }
        }
    }
}

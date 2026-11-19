using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    /// <summary>
    /// radarcol.mul: one ushort colour per land tile (0x0000..0x3FFF) followed by one per item (index + 0x4000).
    ///
    /// Extensions (same idea as for art/tiledata, the classic file keeps its client layout):
    ///   radarcollandext.mul  colours of the extended land tiles (ids 0x4000+, see <see cref="Art.LandExtension"/>)
    ///   radarcolext.mul      colours of the extended items      (ids 0x10000+, see <see cref="Art.ExtendedCount"/>)
    /// Both: 32-byte header (magic UORL / UORI, base id, count, record size 2) + count ushort colours.
    /// Their size follows the art extensions automatically.
    /// </summary>
    public sealed class RadarCol
    {
        public const int LandCount = 0x4000;

        /// <summary>Items reachable in radarcol.mul (ids 0x0000..0xFFFF).</summary>
        public const int LegacyItemCount = 0x10000;

        private static ushort[] _landExt = Array.Empty<ushort>();
        private static ushort[] _itemExt = Array.Empty<ushort>();

        static RadarCol()
        {
            Initialize();
        }

        /// <summary>The classic colours (radarcol.mul): land 0..0x3FFF, items at index + 0x4000.</summary>
        public static ushort[] Colors { get; private set; }

        /// <summary>Number of extended land colours (land ids 0x4000+).</summary>
        public static int LandExtendedCount => _landExt.Length;

        /// <summary>Number of extended item colours (item ids 0x10000+).</summary>
        public static int ItemExtendedCount => _itemExt.Length;

        public static bool HasExtension => _landExt.Length > 0 || _itemExt.Length > 0;

        /// <summary>Number of item colours in radarcol.mul (may be less than 0x10000 for old files).</summary>
        public static int LegacyItemColors => Math.Max(0, Colors.Length - LandCount);

        public static ushort GetItemColor(int index)
        {
            if (index < 0)
            {
                return 0;
            }

            if (index >= LegacyItemCount)
            {
                int k = index - LegacyItemCount;
                return k < _itemExt.Length ? _itemExt[k] : (ushort)0;
            }

            return index + LandCount < Colors.Length ? Colors[index + LandCount] : (ushort)0;
        }

        public static ushort GetLandColor(int index)
        {
            if (index < 0)
            {
                return 0;
            }

            if (index >= LandCount)
            {
                int k = index - LandCount;
                return k < _landExt.Length ? _landExt[k] : (ushort)0;
            }

            return index < Colors.Length ? Colors[index] : (ushort)0;
        }

        /// <summary>
        /// Sets an item colour. Ids up to 0xFFFF grow radarcol.mul if it is shorter (old clients), ids above go into
        /// the extension, which grows up to the size of the art extension.
        /// </summary>
        public static void SetItemColor(int index, ushort value)
        {
            if (index < 0)
            {
                return;
            }

            if (index >= LegacyItemCount)
            {
                int k = index - LegacyItemCount;
                if (k >= _itemExt.Length)
                {
                    if (value == 0)
                    {
                        return;
                    }

                    ushort[] a = _itemExt;
                    Array.Resize(ref a, Math.Max(k + 1, Art.ExtendedCount));
                    _itemExt = a;
                }

                _itemExt[k] = value;
                return;
            }

            int i = index + LandCount;
            if (i >= Colors.Length)
            {
                if (value == 0)
                {
                    return;
                }

                ushort[] c = Colors;
                Array.Resize(ref c, LandCount + LegacyItemCount);
                Colors = c;
            }

            Colors[i] = value;
        }

        public static void SetLandColor(int index, ushort value)
        {
            if (index < 0)
            {
                return;
            }

            if (index >= LandCount)
            {
                int k = index - LandCount;
                if (k >= _landExt.Length)
                {
                    if (value == 0)
                    {
                        return;
                    }

                    ushort[] a = _landExt;
                    Array.Resize(ref a, Math.Max(k + 1, Art.LandExtension.Count));
                    _landExt = a;
                }

                _landExt[k] = value;
                return;
            }

            if (index >= Colors.Length)
            {
                ushort[] c = Colors;
                Array.Resize(ref c, LandCount + LegacyItemCount);
                Colors = c;
            }

            Colors[index] = value;
        }

        /// <summary>Resizes the extensions to the art extensions (keeps every colour inside the new size).</summary>
        public static void SyncExtensionSizes()
        {
            ushort[] land = _landExt;
            Array.Resize(ref land, Art.LandExtension.Count);
            _landExt = land;

            ushort[] items = _itemExt;
            Array.Resize(ref items, Art.ExtendedCount);
            _itemExt = items;
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("radarcol.mul");
            if (path != null)
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Colors = new ushort[fs.Length / 2];
                    GCHandle gc = GCHandle.Alloc(Colors, GCHandleType.Pinned);
                    try
                    {
                        var buffer = new byte[Colors.Length * 2];
                        fs.ReadExactly(buffer, 0, buffer.Length);
                        Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), buffer.Length);
                    }
                    finally
                    {
                        gc.Free();
                    }
                }
            }
            else
            {
                Colors = new ushort[0x8000];
            }

            if (Colors.Length < LandCount)
            {
                ushort[] c = Colors;
                Array.Resize(ref c, LandCount);
                Colors = c;
            }

            LoadExtension();
        }

        /// <summary>Loads radarcollandext.mul / radarcolext.mul again (unsaved extended colours are discarded).</summary>
        public static void LoadExtension()
        {
            _landExt = ReadExt(ExtendedFormat.RadarLandFileName, ExtendedFormat.RadarLandMagic, ExtendedFormat.LandBaseId);
            _itemExt = ReadExt(ExtendedFormat.RadarItemFileName, ExtendedFormat.RadarItemMagic, ExtendedFormat.BaseId);

            // the extensions always cover the art extensions (missing colours are 0 = black)
            if (_landExt.Length < Art.LandExtension.Count || _itemExt.Length < Art.ExtendedCount)
            {
                ushort[] land = _landExt;
                Array.Resize(ref land, Math.Max(land.Length, Art.LandExtension.Count));
                _landExt = land;
                ushort[] items = _itemExt;
                Array.Resize(ref items, Math.Max(items.Length, Art.ExtendedCount));
                _itemExt = items;
            }
        }

        private static ushort[] ReadExt(string name, string magic, int baseId)
        {
            try
            {
                string path = Files.GetFilePathWithRootFallback(name);
                if (path == null)
                {
                    return Array.Empty<ushort>();
                }

                byte[] data = ExtendedFormat.ReadRecords(path, magic, baseId, ExtendedFormat.RadarRecordSize, out int count);
                var result = new ushort[count];
                Buffer.BlockCopy(data, 0, result, 0, count * 2);
                return result;
            }
            catch (Exception)
            {
                return Array.Empty<ushort>(); // corrupt/foreign file: radarcol.mul stays usable
            }
        }

        /// <summary>Writes radarcol.mul and - if there are extended colours - the extension files next to it.</summary>
        public static void Save(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                foreach (var colorValue in Colors)
                {
                    bin.Write(colorValue);
                }
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            if (!string.IsNullOrEmpty(directory))
            {
                SaveExtension(directory);
            }
        }

        /// <summary>Writes radarcollandext.mul / radarcolext.mul (only the ones that have entries).</summary>
        public static void SaveExtension(string directory)
        {
            SyncExtensionSizes();
            if (_landExt.Length > 0)
            {
                ExtendedFormat.WriteRecords(Path.Combine(directory, ExtendedFormat.RadarLandFileName), ExtendedFormat.RadarLandMagic,
                    ExtendedFormat.LandBaseId, ExtendedFormat.RadarRecordSize, _landExt.Length, (bw, k) => bw.Write(_landExt[k]));
            }

            if (_itemExt.Length > 0)
            {
                ExtendedFormat.WriteRecords(Path.Combine(directory, ExtendedFormat.RadarItemFileName), ExtendedFormat.RadarItemMagic,
                    ExtendedFormat.BaseId, ExtendedFormat.RadarRecordSize, _itemExt.Length, (bw, k) => bw.Write(_itemExt[k]));
            }
        }

        /// <summary>
        /// CSV: "ID;Color" for radarcol.mul (index as in the file: land 0..0x3FFF, item = 0x4000 + id) plus
        /// "L:0x4000;Color" (extended land) and "I:0x10000;Color" (extended items).
        /// </summary>
        public static void ExportToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("ID;Color");

                for (int i = 0; i < Colors.Length; ++i)
                {
                    tex.WriteLine("0x{0:X4};{1}", i, Colors[i]);
                }

                for (int k = 0; k < _landExt.Length; ++k)
                {
                    tex.WriteLine("L:0x{0:X4};{1}", LandCount + k, _landExt[k]);
                }

                for (int k = 0; k < _itemExt.Length; ++k)
                {
                    tex.WriteLine("I:0x{0:X5};{1}", LegacyItemCount + k, _itemExt[k]);
                }
            }
        }

        /// <summary>Reads a CSV written by <see cref="ExportToCSV"/>. Lines that are not in the file keep their colour.</summary>
        public static void ImportFromCSV(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                while (sr.ReadLine() is { } line)
                {
                    line = line.Trim();

                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    string[] split = line.Split(';');
                    if (split.Length < 2)
                    {
                        continue;
                    }

                    string key = split[0].Trim();
                    int color = ConvertStringToInt(split[1].Trim());
                    if (color < 0 || color > ushort.MaxValue)
                    {
                        continue;
                    }

                    if (key.StartsWith("L:", StringComparison.OrdinalIgnoreCase))
                    {
                        SetLandColor(ConvertStringToInt(key.Substring(2)), (ushort)color);
                    }
                    else if (key.StartsWith("I:", StringComparison.OrdinalIgnoreCase))
                    {
                        SetItemColor(ConvertStringToInt(key.Substring(2)), (ushort)color);
                    }
                    else
                    {
                        int id = ConvertStringToInt(key);
                        if (id < 0)
                        {
                            continue;
                        }

                        if (id < LandCount)
                        {
                            SetLandColor(id, (ushort)color);
                        }
                        else if (id < LandCount + LegacyItemCount)
                        {
                            SetItemColor(id - LandCount, (ushort)color);
                        }
                    }
                }
            }
        }

        private static int ConvertStringToInt(string text)
        {
            int result;
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(text.Substring(2), NumberStyles.HexNumber, null, out result))
                {
                    return -1;
                }
            }
            else if (!int.TryParse(text, NumberStyles.Integer, null, out result))
            {
                return -1;
            }

            return result;
        }
    }
}

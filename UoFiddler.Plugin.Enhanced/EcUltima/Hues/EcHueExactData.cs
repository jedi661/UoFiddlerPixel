/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Hues
{
    /// <summary>
    /// The exact classic hues (colors, table values, name bytes, group headers) stored in Hues.uop as
    /// "build/hues/classichues.bin", together with the drawing settings. The client doesn't read it;
    /// it makes the way back to hues.mul lossless. A hue is only used while its bitmap is still the one drawn from it.
    /// <para>Format: "UFHX", ushort version, byte scale, byte bmp blur, byte dds blur, byte styles (1 = bmp blurred, 2 = dds blurred), int hue count,
    /// bit list (1 = hue stored), then the hues as hues.mul groups.</para>
    /// </summary>
    public sealed class EcHueExactData
    {
        public const string EntryName = "build/hues/classichues.bin";
        private const uint Magic = 0x58484655; // "UFHX"
        private const ushort FormatVersion = 1;

        private ClassicHueFile _file = ClassicHueFile.Create(0);
        private readonly HashSet<int> _stored = new HashSet<int>();

        public HueRenderOptions Options { get; set; } = new HueRenderOptions();

        public int Count => _stored.Count;

        public ClassicHue Get(int number) => _stored.Contains(number) ? _file.Get(number) : null;

        public void Set(ClassicHue hue)
        {
            _file.EnsureCount(hue.Number);
            ClassicHue target = _file.Get(hue.Number);
            target.SetColors(hue.Colors);
            target.TableStart = hue.TableStart;
            target.TableEnd = hue.TableEnd;
            target.SetRawName((byte[])hue.RawName.Clone());
            _stored.Add(hue.Number);
        }

        public bool Remove(int number) => _stored.Remove(number);

        /// <summary>Group headers of the classic file (kept for a byte exact hues.mul).</summary>
        public void SetGroupHeaders(ClassicHueFile source)
        {
            _file.EnsureCount(source.Count);
            for (int n = 1; n <= source.Count; n += ClassicHueFile.HuesPerGroup)
            {
                _file.SetGroupHeader(n, source.GroupHeader(n));
            }
        }

        public int GroupHeader(int number) => number <= _file.Count ? _file.GroupHeader(number) : 0;

        public byte[] ToBytes()
        {
            int count = _stored.Count == 0 ? 0 : _stored.Max();
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write(Magic);
                w.Write(FormatVersion);
                w.Write((byte)Options.Scale);
                w.Write((byte)Options.BmpBlur);
                w.Write((byte)Options.DdsBlur);
                w.Write((byte)((Options.BmpStyle == HueRampStyle.Blurred ? 1 : 0) | (Options.DdsStyle == HueRampStyle.Blurred ? 2 : 0)));
                w.Write(count);
                var bits = new byte[(count + 7) / 8];
                foreach (int n in _stored)
                {
                    bits[(n - 1) / 8] |= (byte)(1 << ((n - 1) % 8));
                }
                w.Write(bits);
                ClassicHueFile copy = _file.Clone();
                copy.EnsureCount(count);
                w.Write(copy.ToBytes(), 0, (count + ClassicHueFile.HuesPerGroup - 1) / ClassicHueFile.HuesPerGroup * ClassicHueFile.GroupSize);
                w.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>Null when the data is not in this format.</summary>
        public static EcHueExactData FromBytes(byte[] data)
        {
            if (data == null || data.Length < 16 || BitConverter.ToUInt32(data, 0) != Magic || BitConverter.ToUInt16(data, 4) > FormatVersion)
            {
                return null;
            }
            var result = new EcHueExactData
            {
                Options = new HueRenderOptions
                {
                    Scale = (HueColorScale)data[6],
                    BmpBlur = data[7],
                    DdsBlur = data[8],
                    BmpStyle = (data[9] & 1) != 0 ? HueRampStyle.Blurred : HueRampStyle.Linear,
                    DdsStyle = (data[9] & 2) != 0 ? HueRampStyle.Blurred : HueRampStyle.Linear
                }
            };
            int count = BitConverter.ToInt32(data, 10);
            int bitsLength = (count + 7) / 8;
            int pos = 14 + bitsLength;
            if (count < 0 || pos > data.Length)
            {
                return null;
            }
            result._file = ClassicHueFile.FromBytes(data.Skip(pos).ToArray());
            for (int n = 1; n <= count && n <= result._file.Count; n++)
            {
                if ((data[14 + ((n - 1) / 8)] & (1 << ((n - 1) % 8))) != 0)
                {
                    result._stored.Add(n);
                }
            }
            return result;
        }
    }
}

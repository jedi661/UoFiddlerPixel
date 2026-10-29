/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Text;

namespace UoFiddler.Plugin.Enhanced.Core.Uop
{
    /// <summary>
    /// Hash functions used by Mythic UOP packages.
    /// File names inside a package are stored only as a 64 bit hash (Bob Jenkins' hashlittle2).
    /// The data block hash in the table of contents is an Adler-32 checksum of the entry's metadata header.
    /// </summary>
    public static class EcUopHash
    {
        public static ulong HashFileName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            byte[] s = Encoding.ASCII.GetBytes(name.ToLowerInvariant());
            int length = s.Length;
            uint a, b, c;
            a = b = c = 0xDEADBEEF + (uint)length;

            int k = 0;
            while (length > 12)
            {
                a += BitConverter.ToUInt32(s, k);
                b += BitConverter.ToUInt32(s, k + 4);
                c += BitConverter.ToUInt32(s, k + 8);

                a -= c; a ^= Rot(c, 4); c += b;
                b -= a; b ^= Rot(a, 6); a += c;
                c -= b; c ^= Rot(b, 8); b += a;
                a -= c; a ^= Rot(c, 16); c += b;
                b -= a; b ^= Rot(a, 19); a += c;
                c -= b; c ^= Rot(b, 4); b += a;

                length -= 12;
                k += 12;
            }

            if (length == 0)
            {
                return ((ulong)b << 32) | c;
            }

            byte[] tail = new byte[12];
            Buffer.BlockCopy(s, k, tail, 0, length);
            a += BitConverter.ToUInt32(tail, 0);
            b += BitConverter.ToUInt32(tail, 4);
            c += BitConverter.ToUInt32(tail, 8);

            c ^= b; c -= Rot(b, 14);
            a ^= c; a -= Rot(c, 11);
            b ^= a; b -= Rot(a, 25);
            c ^= b; c -= Rot(b, 16);
            a ^= c; a -= Rot(c, 4);
            b ^= a; b -= Rot(a, 14);
            c ^= b; c -= Rot(b, 24);

            return ((ulong)b << 32) | c;
        }

        public static uint Adler32(byte[] data, int offset, int count)
        {
            const uint mod = 65521;
            uint a = 1, b = 0;
            for (int i = 0; i < count; i++)
            {
                a = (a + data[offset + i]) % mod;
                b = (b + a) % mod;
            }
            return (b << 16) | a;
        }

        private static uint Rot(uint x, int k) => (x << k) | (x >> (32 - k));
    }
}

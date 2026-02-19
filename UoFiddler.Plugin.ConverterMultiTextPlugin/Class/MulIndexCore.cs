// =============================================================================
//  MulIndexCore.cs  –  Kern-Datenmodell für alle UO MUL/IDX-Dateiformate
//  Namespace: UoFiddler.Plugin.ConverterMultiTextPlugin.MulCore
//
//  VERSION 2.0  –  Alle Fixes aus v1.0 enthalten +  NEU:
//    • HuesFile        – hues.mul (3000 × 88 Byte Farbpaletten)
//    • MapFile         – map*.mul  (Terrain-Tiles, beliebige Größe)
//    • StaticsFile     – statics*.mul + staidx*.mul
//    • MultiFile       – multi.mul / multi.idx (Häuser, Boote)
//    • SkillsFile      – skills.mul / skills.idx
//    • MulValidator    – Prüft IDX↔MUL-Konsistenz, findet kaputte Einträge
//    • IdxPatcher      – Schreibt einzelne IDX-Einträge gezielt zurück
//    • BatchSetup      – Erstellt alle Standard-Dateien für einen leeren Shard
//    • HexViewHelper   – Byte-genaues Lesen beliebiger MUL-Dateien
//    • FileSizeCompare – Vergleicht zwei Shard-Verzeichnisse
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.MulCore
{
    // =========================================================================
    //  ENUMS
    // =========================================================================

    public enum IndexFormat : byte { Legacy = 0, Extended = 1, Compressed = 2 }
    public enum TileDataVersion { Legacy, HighSeas }
    public enum CreatureType { HighDetail = 110, LowDetail = 65, Human = 175 }

    // =========================================================================
    //  MulIndexEntry
    // =========================================================================

    public sealed class MulIndexEntry
    {
        public const int LegacySize = 12;
        public const int ExtendedSize = 20;
        public const uint LegacyEmpty = 0xFFFFFFFF;
        public const long ExtendedEmpty = -1L;

        public long Lookup { get; set; }
        public long Size { get; set; }
        public uint Unknown { get; set; }
        public uint ExtFlags { get; set; }
        public IndexFormat Format { get; set; }

        public bool IsEmpty => Format == IndexFormat.Legacy ? (uint)Lookup == LegacyEmpty : Lookup == ExtendedEmpty;
        public bool IsCompressed => (ExtFlags & 0x01) != 0;
        public int ByteSize => Format == IndexFormat.Legacy ? LegacySize : ExtendedSize;

        public MulIndexEntry()
        { Format = IndexFormat.Legacy; Lookup = LegacyEmpty; Size = 0; Unknown = 0; ExtFlags = 0; }

        public MulIndexEntry(uint lookup, uint size, uint unknown)
        { Format = IndexFormat.Legacy; Lookup = lookup; Size = size; Unknown = unknown; ExtFlags = 0; }

        public MulIndexEntry(long lookup, long size, uint unknown, uint extFlags = 0)
        { Format = IndexFormat.Extended; Lookup = lookup; Size = size; Unknown = unknown; ExtFlags = extFlags; }

        public void WriteTo(BinaryWriter w)
        {
            if (Format == IndexFormat.Legacy)
            {
                w.Write(IsEmpty ? LegacyEmpty : (uint)Math.Min(Lookup, uint.MaxValue));
                w.Write((uint)Math.Min(Size, uint.MaxValue));
                w.Write(Unknown);
            }
            else
            {
                w.Write(IsEmpty ? ExtendedEmpty : Lookup);
                w.Write(Size);
                w.Write(Unknown);
                w.Write(ExtFlags);
            }
        }

        public static MulIndexEntry ReadFrom(BinaryReader r, IndexFormat fmt)
        {
            var e = new MulIndexEntry { Format = fmt };
            if (fmt == IndexFormat.Legacy)
            { e.Lookup = r.ReadUInt32(); e.Size = r.ReadUInt32(); e.Unknown = r.ReadUInt32(); }
            else
            { e.Lookup = r.ReadInt64(); e.Size = r.ReadInt64(); e.Unknown = r.ReadUInt32(); e.ExtFlags = r.ReadUInt32(); }
            return e;
        }

        public MulIndexEntry ToExtended() =>
            new MulIndexEntry(IsEmpty ? ExtendedEmpty : Lookup, Size, Unknown, 0) { Format = IndexFormat.Extended };

        public MulIndexEntry ToLegacy()
        {
            if (IsEmpty || Lookup > uint.MaxValue || Size > uint.MaxValue) return new MulIndexEntry();
            return new MulIndexEntry((uint)Lookup, (uint)Size, Unknown);
        }

        public override string ToString() =>
            IsEmpty ? $"[{Format}] EMPTY"
            : Format == IndexFormat.Legacy
                ? $"[Legacy]   Lookup=0x{Lookup:X8}  Size={Size,10}  Unknown=0x{Unknown:X8}"
                : $"[Extended] Lookup=0x{Lookup:X16}  Size={Size,12}  Unknown=0x{Unknown:X8}  ExtFlags=0x{ExtFlags:X8}";
    }

    // =========================================================================
    //  MulIndexFile
    // =========================================================================

    public sealed class MulIndexFile
    {
        private const uint MagicExtended = 0x4E445845;
        private const uint MagicCompressed = 0x444E455A;
        private const uint HeaderVersion = 1;

        private readonly List<MulIndexEntry> _entries = new List<MulIndexEntry>();

        public IndexFormat Format { get; private set; } = IndexFormat.Legacy;
        public int Count => _entries.Count;
        public int DefinedCount { get { int n = 0; foreach (var e in _entries) if (!e.IsEmpty) n++; return n; } }

        public MulIndexEntry this[int i]
        { get => _entries[i]; set => _entries[i] = value ?? throw new ArgumentNullException(nameof(value)); }

        public IReadOnlyList<MulIndexEntry> Entries => _entries.AsReadOnly();

        public static MulIndexFile CreateEmpty(long count, IndexFormat fmt = IndexFormat.Legacy)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            var f = new MulIndexFile { Format = fmt };
            for (long i = 0; i < count; i++) f.AddEmpty();
            return f;
        }

        public MulIndexEntry GetEntry(int i) => (i >= 0 && i < _entries.Count) ? _entries[i] : null;
        public void Add(MulIndexEntry e) => _entries.Add(e ?? throw new ArgumentNullException(nameof(e)));

        public void AddEmpty()
        {
            var e = new MulIndexEntry { Format = Format };
            if (Format != IndexFormat.Legacy) e.Lookup = MulIndexEntry.ExtendedEmpty;
            _entries.Add(e);
        }

        public void AddEmptyRange(long count) { for (long i = 0; i < count; i++) AddEmpty(); }
        public void RemoveAt(int i) => _entries.RemoveAt(i);

        public void ClearEntry(int i)
        {
            var e = _entries[i];
            e.Lookup = Format == IndexFormat.Legacy ? MulIndexEntry.LegacyEmpty : MulIndexEntry.ExtendedEmpty;
            e.Size = 0; e.Unknown = 0; e.ExtFlags = 0;
        }

        public void ClearAll() { for (int i = 0; i < _entries.Count; i++) ClearEntry(i); }

        public void UpgradeToExtended()
        {
            if (Format != IndexFormat.Legacy) return;
            for (int i = 0; i < _entries.Count; i++) _entries[i] = _entries[i].ToExtended();
            Format = IndexFormat.Extended;
        }

        public void DowngradeToLegacy()
        {
            if (Format == IndexFormat.Legacy) return;
            for (int i = 0; i < _entries.Count; i++) _entries[i] = _entries[i].ToLegacy();
            Format = IndexFormat.Legacy;
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("IDX nicht gefunden.", path);
            _entries.Clear();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var r = new BinaryReader(fs, Encoding.Latin1, false);
            if (fs.Length >= 8)
            {
                uint magic = r.ReadUInt32();
                if (magic == MagicExtended || magic == MagicCompressed)
                {
                    Format = magic == MagicCompressed ? IndexFormat.Compressed : IndexFormat.Extended;
                    r.ReadUInt32();
                    ReadEntries(r, IndexFormat.Extended);
                    return;
                }
                fs.Seek(0, SeekOrigin.Begin);
            }
            Format = IndexFormat.Legacy;
            ReadEntries(r, IndexFormat.Legacy);
        }

        private void ReadEntries(BinaryReader r, IndexFormat fmt)
        {
            int sz = fmt == IndexFormat.Legacy ? MulIndexEntry.LegacySize : MulIndexEntry.ExtendedSize;
            long cnt = (r.BaseStream.Length - r.BaseStream.Position) / sz;
            for (long i = 0; i < cnt; i++) _entries.Add(MulIndexEntry.ReadFrom(r, fmt));
        }

        public void SaveToFile(string path)
        {
            EnsureDir(path);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var w = new BinaryWriter(fs, Encoding.Latin1, false);
            if (Format == IndexFormat.Extended || Format == IndexFormat.Compressed)
            { w.Write(Format == IndexFormat.Compressed ? MagicCompressed : MagicExtended); w.Write(HeaderVersion); }
            foreach (var e in _entries)
            { e.Format = Format == IndexFormat.Legacy ? IndexFormat.Legacy : IndexFormat.Extended; e.WriteTo(w); }
        }

        public void SaveWithEmptyMul(string idxPath, string mulPath)
        { SaveToFile(idxPath); using var _ = File.Create(mulPath); }

        public string GetSummary() =>
            $"Format: {Format}  |  Einträge: {Count:N0} gesamt, {DefinedCount:N0} definiert, {Count - DefinedCount:N0} leer";

        public string GetDetailedInfo(int maxEntries = 500)
        {
            var sb = new StringBuilder();
            int limit = (maxEntries > 0 && maxEntries < _entries.Count) ? maxEntries : _entries.Count;
            sb.AppendLine(GetSummary());
            for (int i = 0; i < limit; i++) sb.AppendLine($"  [{i,6}] {_entries[i]}");
            if (limit < _entries.Count) sb.AppendLine($"  ... ({_entries.Count - limit} weitere Einträge)");
            return sb.ToString();
        }

        private static void EnsureDir(string path)
        {
            string d = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d);
        }
    }

    // =========================================================================
    //  MulFileHelper
    // =========================================================================

    public static class MulFileHelper
    {
        public static string CreateIndexAndMul(string directory, string idxName, string mulName,
            long entryCount, IndexFormat format = IndexFormat.Legacy)
        {
            if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Kein Verzeichnis.");
            if (entryCount < 0) throw new ArgumentOutOfRangeException(nameof(entryCount));
            string idxPath = Path.Combine(directory, idxName);
            string mulPath = Path.Combine(directory, mulName);
            MulIndexFile.CreateEmpty(entryCount, format).SaveWithEmptyMul(idxPath, mulPath);
            return $"Erstellt ({format}, {entryCount:N0} Einträge):\n  {idxPath}\n  {mulPath}";
        }

        public static (MulIndexFile Index, string Summary) LoadAndSummarize(string idxPath)
        {
            var idx = new MulIndexFile(); idx.LoadFromFile(idxPath); return (idx, idx.GetSummary());
        }

        public static string ReadSingleEntry(string idxPath, int i)
        {
            var idx = new MulIndexFile(); idx.LoadFromFile(idxPath);
            if (i < 0 || i >= idx.Count) return $"Index {i} außerhalb Bereich (0..{idx.Count - 1}).";
            return $"Eintrag {i}: {idx[i]}";
        }

        public static string ExtendIndex(string idxPath, long additional)
        {
            var idx = new MulIndexFile();
            if (File.Exists(idxPath)) idx.LoadFromFile(idxPath);
            long before = idx.Count;
            idx.AddEmptyRange(additional);
            idx.SaveToFile(idxPath);
            return $"Index erweitert: {before:N0} → {idx.Count:N0}\n  {idxPath}";
        }

        public static string UpgradeToExtended(string srcPath, string dstPath = null)
        {
            var idx = new MulIndexFile(); idx.LoadFromFile(srcPath);
            if (idx.Format != IndexFormat.Legacy) return $"Bereits {idx.Format}-Format.";
            idx.UpgradeToExtended();
            string outPath = dstPath ?? srcPath;
            idx.SaveToFile(outPath);
            return $"Legacy → Extended ({idx.Count:N0} Einträge)\n  {outPath}";
        }

        public static long ParseEntryCount(string input, long defaultVal = 65500, long min = 1, long max = 0)
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultVal;
            string t = input.Trim(); long result;
            bool ok = t.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? long.TryParse(t.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out result)
                : long.TryParse(t, out result);
            if (!ok) return defaultVal;
            if (result < min) return min;
            if (max > 0 && result > max) return max;
            return result;
        }
    }

    // =========================================================================
    //  TileData
    // =========================================================================

    public sealed class LandTileEntry
    {
        public ulong Flags { get; set; }
        public ushort TextureID { get; set; }
        public string Name { get; set; } = string.Empty;

        public const int LegacySize = 26;
        public const int HighSeasSize = 30;

        public void WriteTo(BinaryWriter w, TileDataVersion ver)
        {
            if (ver == TileDataVersion.Legacy) w.Write((uint)Flags); else w.Write(Flags);
            w.Write(TextureID); WriteFixed(w, Name, 20);
        }

        public static LandTileEntry ReadFrom(BinaryReader r, TileDataVersion ver)
        {
            var e = new LandTileEntry();
            e.Flags = ver == TileDataVersion.Legacy ? r.ReadUInt32() : r.ReadUInt64();
            e.TextureID = r.ReadUInt16(); e.Name = ReadFixed(r, 20); return e;
        }

        public override string ToString() => $"Name={Name,-20}  TexID={TextureID,5}  Flags=0x{Flags:X16}";

        static void WriteFixed(BinaryWriter w, string s, int len)
        {
            byte[] b = new byte[len];
            if (!string.IsNullOrEmpty(s)) { byte[] src = Encoding.ASCII.GetBytes(s); Array.Copy(src, b, Math.Min(src.Length, len)); }
            w.Write(b);
        }
        static string ReadFixed(BinaryReader r, int len) => Encoding.ASCII.GetString(r.ReadBytes(len)).TrimEnd('\0');
    }

    public sealed class StaticTileEntry
    {
        public uint Unknown0 { get; set; }
        public ulong Flags { get; set; }
        public byte Weight { get; set; }
        public byte Quality { get; set; }
        public ushort MiscData { get; set; }
        public byte Unk1 { get; set; }
        public byte Quantity { get; set; }
        public ushort AnimID { get; set; }
        public byte Unk2 { get; set; }
        public byte Hue { get; set; }
        public byte StackingOff { get; set; }
        public byte Value { get; set; }
        public byte Height { get; set; }
        public string Name { get; set; } = string.Empty;

        public const int LegacySize = 41;
        public const int HighSeasSize = 45;

        public void WriteTo(BinaryWriter w, TileDataVersion ver)
        {
            w.Write(Unknown0);
            if (ver == TileDataVersion.Legacy) w.Write((uint)Flags); else w.Write(Flags);
            w.Write(Weight); w.Write(Quality); w.Write(MiscData); w.Write(Unk1);
            w.Write(Quantity); w.Write(AnimID); w.Write(Unk2); w.Write(Hue);
            w.Write(StackingOff); w.Write(Value); w.Write(Height);
            WriteFixed(w, Name, 20);
        }

        public static StaticTileEntry ReadFrom(BinaryReader r, TileDataVersion ver)
        {
            var e = new StaticTileEntry();
            e.Unknown0 = r.ReadUInt32();
            e.Flags = ver == TileDataVersion.Legacy ? r.ReadUInt32() : r.ReadUInt64();
            e.Weight = r.ReadByte(); e.Quality = r.ReadByte(); e.MiscData = r.ReadUInt16();
            e.Unk1 = r.ReadByte(); e.Quantity = r.ReadByte(); e.AnimID = r.ReadUInt16();
            e.Unk2 = r.ReadByte(); e.Hue = r.ReadByte(); e.StackingOff = r.ReadByte();
            e.Value = r.ReadByte(); e.Height = r.ReadByte(); e.Name = ReadFixed(r, 20); return e;
        }

        public override string ToString() =>
            $"Name={Name,-20}  AnimID={AnimID,5}  H={Height,3}  Flags=0x{Flags:X16}  Unk0=0x{Unknown0:X8}";

        static void WriteFixed(BinaryWriter w, string s, int len)
        {
            byte[] b = new byte[len];
            if (!string.IsNullOrEmpty(s)) { byte[] src = Encoding.ASCII.GetBytes(s); Array.Copy(src, b, Math.Min(src.Length, len)); }
            w.Write(b);
        }
        static string ReadFixed(BinaryReader r, int len) => Encoding.ASCII.GetString(r.ReadBytes(len)).TrimEnd('\0');
    }

    public sealed class TileDataFile
    {
        public TileDataVersion Version { get; private set; }
        public List<LandTileEntry> LandTiles { get; } = new List<LandTileEntry>();
        public List<StaticTileEntry> StaticTiles { get; } = new List<StaticTileEntry>();

        public int LandGroupCount => (LandTiles.Count + 31) / 32;
        public int StaticGroupCount => (StaticTiles.Count + 31) / 32;

        private const int LandGroupLegacy = 4 + 32 * LandTileEntry.LegacySize;
        private const int StaticGroupLegacy = 4 + 32 * StaticTileEntry.LegacySize;
        private const int LandGroupHS = 4 + 32 * LandTileEntry.HighSeasSize;
        private const int StaticGroupHS = 4 + 32 * StaticTileEntry.HighSeasSize;

        public const int DefaultLandGroups = 512;
        public const int DefaultStaticGroups = 2048;

        public static TileDataFile CreateEmpty(int land = DefaultLandGroups, int stat = DefaultStaticGroups,
            TileDataVersion ver = TileDataVersion.Legacy)
        {
            var f = new TileDataFile { Version = ver };
            for (int i = 0; i < land * 32; i++) f.LandTiles.Add(new LandTileEntry());
            for (int i = 0; i < stat * 32; i++) f.StaticTiles.Add(new StaticTileEntry());
            return f;
        }

        public void SaveToFile(string path)
        {
            EnsureDir(path);
            using var w = new BinaryWriter(File.Open(path, FileMode.Create));
            for (int g = 0; g < LandGroupCount; g++)
            {
                w.Write(0u);
                for (int i = 0; i < 32; i++) { int idx = g * 32 + i; (idx < LandTiles.Count ? LandTiles[idx] : new LandTileEntry()).WriteTo(w, Version); }
            }
            for (int g = 0; g < StaticGroupCount; g++)
            {
                w.Write(0u);
                for (int i = 0; i < 32; i++) { int idx = g * 32 + i; (idx < StaticTiles.Count ? StaticTiles[idx] : new StaticTileEntry()).WriteTo(w, Version); }
            }
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("TileData nicht gefunden.", path);
            LandTiles.Clear(); StaticTiles.Clear();
            Version = DetectVersion(new FileInfo(path).Length);
            int lgs = Version == TileDataVersion.Legacy ? LandGroupLegacy : LandGroupHS;
            int sgs = Version == TileDataVersion.Legacy ? StaticGroupLegacy : StaticGroupHS;
            using var r = new BinaryReader(File.OpenRead(path));
            for (int g = 0; g < DefaultLandGroups && r.BaseStream.Position + lgs <= r.BaseStream.Length; g++)
            { r.ReadUInt32(); for (int i = 0; i < 32; i++) LandTiles.Add(LandTileEntry.ReadFrom(r, Version)); }
            while (r.BaseStream.Position + sgs <= r.BaseStream.Length)
            { r.ReadUInt32(); for (int i = 0; i < 32; i++) StaticTiles.Add(StaticTileEntry.ReadFrom(r, Version)); }
        }

        private static TileDataVersion DetectVersion(long sz)
        {
            long llb = (long)DefaultLandGroups * LandGroupLegacy;
            long lhb = (long)DefaultLandGroups * LandGroupHS;
            long rl = sz - llb; if (rl >= 0 && (rl % StaticGroupLegacy) == 0) return TileDataVersion.Legacy;
            long rh = sz - lhb; if (rh >= 0 && (rh % StaticGroupHS) == 0) return TileDataVersion.HighSeas;
            long ls = llb + (long)DefaultStaticGroups * StaticGroupLegacy;
            return Math.Abs(sz - ls) < sz / 10 ? TileDataVersion.Legacy : TileDataVersion.HighSeas;
        }

        public string GetSummary() =>
            $"Version: {Version}  |  Land: {LandTiles.Count:N0}  |  Static: {StaticTiles.Count:N0}";

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    public sealed class TileDataFlags
    {
        private static readonly Dictionary<string, ulong> FlagMasks =
            new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase)
        {
            {"background",0x1},{"weapon",0x2},{"transparent",0x4},{"translucent",0x8},
            {"wall",0x10},{"damaging",0x20},{"impassable",0x40},{"wet",0x80},
            {"surface",0x200},{"climbable",0x400},{"stackable",0x800},{"window",0x1000},
            {"noShoot",0x2000},{"articleA",0x4000},{"articleAn",0x8000},{"internal",0x10000},
            {"foliage",0x20000},{"partialHue",0x40000},{"map",0x100000},{"container",0x200000},
            {"wearable",0x400000},{"lightSource",0x800000},{"animation",0x1000000},{"noDiagonal",0x2000000},
            {"armor",0x8000000},{"roof",0x10000000},{"door",0x20000000},{"stairBack",0x40000000},
            {"stairRight",0x80000000},{"noShadow",0x100000000},{"pixelBleed",0x200000000},
            {"playAnimOnce",0x400000000},{"multiMovable",0x800000000},{"fullBright",0x2000000000},
            {"hoverOver",0x80000000000}
        };

        private ulong _value;
        public ulong Value { get => _value; set => _value = value; }
        public TileDataFlags() { }
        public TileDataFlags(ulong v) { _value = v; }
        public TileDataFlags(string names)
        {
            foreach (var n in names.Split(','))
            { string t = n.Trim(); if (FlagMasks.TryGetValue(t, out ulong m)) _value |= m; else throw new ArgumentException($"Unbekannter Flag: {t}"); }
        }
        public bool HasFlag(string n) => FlagMasks.TryGetValue(n, out ulong m) && (_value & m) != 0;
        public void SetFlag(string n, bool s) { if (!FlagMasks.TryGetValue(n, out ulong m)) throw new ArgumentException($"Unbekannter Flag: {n}"); if (s) _value |= m; else _value &= ~m; }
        public void SetBit(int b, bool v) { ulong m = (ulong)1 << b; if (v) _value |= m; else _value &= ~m; }
        public bool GetBit(int b) => (_value & ((ulong)1 << b)) != 0;
        public static IReadOnlyDictionary<string, ulong> KnownFlags => FlagMasks;
        public override string ToString() => $"0x{_value:X16}";
    }

    // =========================================================================
    //  Animation
    // =========================================================================

    public sealed class AnimFrame
    {
        public ushort CenterX { get; set; }
        public ushort CenterY { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte[] PixelData { get; set; }
    }

    public sealed class AnimationGroup
    {
        public ushort[] Palette { get; set; } = new ushort[256];
        public uint FrameCount { get; set; }
        public uint[] FrameLookup { get; set; } = Array.Empty<uint>();
        public AnimFrame[] Frames { get; set; } = Array.Empty<AnimFrame>();
        public override string ToString() => $"AnimationGroup: {FrameCount} Frames";
    }

    public sealed class AnimationFile
    {
        public static AnimationGroup Load(string mulPath, string idxPath)
        {
            if (!File.Exists(idxPath)) throw new FileNotFoundException("anim.idx nicht gefunden.", idxPath);
            if (!File.Exists(mulPath)) throw new FileNotFoundException("anim.mul nicht gefunden.", mulPath);
            var anim = new AnimationGroup();
            using (var idxR = new BinaryReader(File.OpenRead(idxPath)))
            {
                long n = idxR.BaseStream.Length / 12;
                anim.FrameCount = (uint)n;
                anim.FrameLookup = new uint[n];
                for (long i = 0; i < n; i++)
                {
                    if (idxR.BaseStream.Position + 12 > idxR.BaseStream.Length) break;
                    uint lookup = idxR.ReadUInt32(); uint size = idxR.ReadUInt32(); idxR.ReadUInt32();
                    if (lookup != 0xFFFFFFFF && size > 0) anim.FrameLookup[i] = lookup;
                }
            }
            anim.Frames = new AnimFrame[anim.FrameCount];
            using (var mulR = new BinaryReader(File.OpenRead(mulPath)))
            {
                long len = mulR.BaseStream.Length;
                for (int i = 0; i < (int)anim.FrameCount; i++)
                {
                    uint off = anim.FrameLookup[i];
                    if (off == 0 || off >= (ulong)len || (long)off + 8 > len) continue;
                    mulR.BaseStream.Seek(off, SeekOrigin.Begin);
                    var fr = new AnimFrame { CenterX = mulR.ReadUInt16(), CenterY = mulR.ReadUInt16(), Width = mulR.ReadUInt16(), Height = mulR.ReadUInt16() };
                    int nfs = int.MaxValue;
                    for (int j = i + 1; j < (int)anim.FrameCount; j++) if (anim.FrameLookup[j] > off) { nfs = (int)anim.FrameLookup[j]; break; }
                    int plen = nfs == int.MaxValue ? (int)(len - (long)off - 8) : nfs - (int)off - 8;
                    if (plen > 0 && (long)off + 8 + plen <= len) fr.PixelData = mulR.ReadBytes(plen);
                    anim.Frames[i] = fr;
                }
            }
            return anim;
        }

        public static (long IndexOffset, int ReadLength, CreatureType Type) GetCreatureIdxInfo(int id)
        {
            if (id <= 199) return ((long)id * (int)CreatureType.HighDetail, (int)CreatureType.HighDetail * 12, CreatureType.HighDetail);
            if (id <= 399) return ((long)(id - 200) * (int)CreatureType.LowDetail + 22000L, (int)CreatureType.LowDetail * 12, CreatureType.LowDetail);
            return ((long)(id - 400) * (int)CreatureType.Human + 35000L, (int)CreatureType.Human * 12, CreatureType.Human);
        }

        public static string CopyCreatureIdx(string idxPath, int creatureID, int copyCount, Action<string> log = null)
        {
            if (!File.Exists(idxPath)) return $"Datei nicht gefunden: {idxPath}";
            var (offset, length, type) = GetCreatureIdxInfo(creatureID);
            log?.Invoke($"Kreatur-Typ: {type}");
            using var stream = File.Open(idxPath, FileMode.Open, FileAccess.ReadWrite);
            long seekPos = offset * 12;
            if (seekPos + length > stream.Length) return $"Creature-ID {creatureID}: Offset {seekPos} außerhalb der Datei.";
            stream.Seek(seekPos, SeekOrigin.Begin);
            byte[] buf = new byte[length]; int read = stream.Read(buf, 0, length);
            log?.Invoke($"{read} Bytes gelesen.");
            for (int i = 0; i < copyCount; i++) { stream.Seek(0, SeekOrigin.End); stream.Write(buf, 0, length); log?.Invoke($"Kopie {i + 1}/{copyCount}"); }
            return $"Fertig: {copyCount}× Creature-ID {creatureID} ({type}) kopiert.";
        }
    }

    // =========================================================================
    //  Sound
    // =========================================================================

    public sealed class SoundIndexEntry
    {
        public int StartPos { get; set; }
        public int Length { get; set; }
        public ushort Index { get; set; }
        public ushort Reserved { get; set; }
        public const int ByteSize = 12;
        public void WriteTo(BinaryWriter w) { w.Write(StartPos); w.Write(Length); w.Write(Index); w.Write(Reserved); }
        public static SoundIndexEntry ReadFrom(BinaryReader r) =>
            new SoundIndexEntry { StartPos = r.ReadInt32(), Length = r.ReadInt32(), Index = r.ReadUInt16(), Reserved = r.ReadUInt16() };
    }

    public sealed class SoundFile
    {
        private readonly List<SoundIndexEntry> _index = new List<SoundIndexEntry>();
        public IReadOnlyList<SoundIndexEntry> Index => _index.AsReadOnly();

        public static string CreateEmpty(string dir, int count)
        {
            string ip = Path.Combine(dir, "SoundIdx.mul"), sp = Path.Combine(dir, "Sound.mul");
            using var iw = new BinaryWriter(File.Open(ip, FileMode.Create));
            using var sw = new BinaryWriter(File.Open(sp, FileMode.Create));
            byte[] ph = new byte[1024]; int pos = 0;
            for (int i = 0; i < count; i++)
            { sw.Write(ph); new SoundIndexEntry { StartPos = pos, Length = ph.Length, Index = (ushort)i }.WriteTo(iw); pos += ph.Length; }
            return $"Sound erstellt ({count} Einträge):\n  {ip}\n  {sp}";
        }

        public int LoadIndex(string p)
        {
            _index.Clear();
            using var r = new BinaryReader(File.OpenRead(p));
            while (r.BaseStream.Position + SoundIndexEntry.ByteSize <= r.BaseStream.Length) _index.Add(SoundIndexEntry.ReadFrom(r));
            return _index.Count;
        }
    }

    // =========================================================================
    //  Gump
    // =========================================================================

    public struct GumpRun { public ushort Value; public ushort Run; }
    public struct GumpRow { public ushort[] Pixels; }

    public sealed class GumpFile
    {
        public const int IndexEntrySize = 12;

        public static int CountEntries(string dir)
        {
            string p = Path.Combine(dir, "GUMPIDX.MUL");
            return File.Exists(p) ? (int)(new FileInfo(p).Length / IndexEntrySize) : 0;
        }

        public static List<GumpRow> ReadGump(string dir, int idx)
        {
            using var ir = new BinaryReader(File.OpenRead(Path.Combine(dir, "GUMPIDX.MUL")));
            using var dr = new BinaryReader(File.OpenRead(Path.Combine(dir, "GUMPART.MUL")));
            ir.BaseStream.Seek((long)idx * IndexEntrySize, SeekOrigin.Begin);
            int lk = ir.ReadInt32(); ir.ReadInt32(); ushort h = ir.ReadUInt16(); ushort w = ir.ReadUInt16();
            var rows = new List<GumpRow>(); dr.BaseStream.Seek(lk, SeekOrigin.Begin);
            for (int y = 0; y < h; y++) rows.Add(DecodeRow(dr, w));
            return rows;
        }

        public static void CreateEmpty(string dir, int count, int width = 64, int height = 64)
        {
            using var iw = new BinaryWriter(File.Open(Path.Combine(dir, "GUMPIDX.MUL"), FileMode.Create));
            using var dw = new BinaryWriter(File.Open(Path.Combine(dir, "GUMPART.MUL"), FileMode.Create));
            for (int i = 0; i < count; i++)
            { iw.Write(unchecked((uint)0xFFFFFFFF)); iw.Write(0); iw.Write((ushort)height); iw.Write((ushort)width); dw.Write(0); }
        }

        private static GumpRow DecodeRow(BinaryReader r, int w)
        {
            var row = new GumpRow { Pixels = new ushort[w] }; int i = 0;
            while (i < w) { ushort v = r.ReadUInt16(); ushort run = r.ReadUInt16(); for (int j = 0; j < run && i < w; j++) row.Pixels[i++] = v; }
            return row;
        }

        public static List<GumpRun> EncodeRow(GumpRow row)
        {
            var runs = new List<GumpRun>(); if (row.Pixels == null || row.Pixels.Length == 0) return runs;
            ushort cur = row.Pixels[0]; int len = 1;
            for (int i = 1; i < row.Pixels.Length; i++) { if (row.Pixels[i] == cur) len++; else { runs.Add(new GumpRun { Value = cur, Run = (ushort)len }); cur = row.Pixels[i]; len = 1; } }
            runs.Add(new GumpRun { Value = cur, Run = (ushort)len }); return runs;
        }
    }

    // =========================================================================
    //  Palette
    // =========================================================================

    public sealed class PaletteFile
    {
        public List<(byte R, byte G, byte B)> Colors { get; } = new List<(byte, byte, byte)>();
        public void Add(System.Drawing.Color c) => Colors.Add((c.R, c.G, c.B));
        public void CreateGrayscale() { Colors.Clear(); for (int i = 0; i < 256; i++) Colors.Add(((byte)i, (byte)i, (byte)i)); }
        public void Save(string p) { using var w = new BinaryWriter(File.Open(p, FileMode.Create)); foreach (var (r, g, b) in Colors) { w.Write(r); w.Write(g); w.Write(b); } }
        public void Load(string p) { Colors.Clear(); using var r = new BinaryReader(File.OpenRead(p)); while (r.BaseStream.Position + 3 <= r.BaseStream.Length) Colors.Add((r.ReadByte(), r.ReadByte(), r.ReadByte())); }
        public void Draw(System.Drawing.Graphics g, int tw, int th)
        {
            if (Colors.Count == 0) return; int cols = Math.Max(1, tw / 10);
            for (int i = 0; i < Colors.Count; i++) { var (r, gr, b) = Colors[i]; using var br = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(r, gr, b)); g.FillRectangle(br, (i % cols) * 10, (i / cols) * 10, 10, 10); }
        }
    }

    // =========================================================================
    //  NEU: HuesFile  –  hues.mul
    //  Aufbau: 3000 Gruppen × 88 Byte
    //    jede Gruppe: ushort[32] ColorTable + ushort TableStart + ushort TableEnd + char[20] Name
    //    Gesamt: 3000 × 88 = 264.000 Byte
    // =========================================================================

    public sealed class HueEntry
    {
        public ushort[] ColorTable { get; set; } = new ushort[32];
        public ushort TableStart { get; set; }
        public ushort TableEnd { get; set; }
        public string Name { get; set; } = string.Empty;

        public const int ByteSize = 88; // 32×2 + 2 + 2 + 20

        public void WriteTo(BinaryWriter w)
        {
            foreach (var c in ColorTable) w.Write(c);
            w.Write(TableStart); w.Write(TableEnd);
            byte[] nb = new byte[20];
            if (!string.IsNullOrEmpty(Name)) { byte[] s = Encoding.ASCII.GetBytes(Name); Array.Copy(s, nb, Math.Min(s.Length, 20)); }
            w.Write(nb);
        }

        public static HueEntry ReadFrom(BinaryReader r)
        {
            var e = new HueEntry();
            for (int i = 0; i < 32; i++) e.ColorTable[i] = r.ReadUInt16();
            e.TableStart = r.ReadUInt16(); e.TableEnd = r.ReadUInt16();
            e.Name = Encoding.ASCII.GetString(r.ReadBytes(20)).TrimEnd('\0');
            return e;
        }

        public override string ToString() => $"Name={Name,-20}  Start={TableStart}  End={TableEnd}";
    }

    public sealed class HuesFile
    {
        public const int GroupHeaderSize = 4;   // int header vor je 8 Einträgen
        public const int EntriesPerGroup = 8;
        public const int TotalGroups = 375; // 375 × 8 = 3000 Einträge
        public const int TotalEntries = 3000;

        private readonly List<HueEntry> _entries = new List<HueEntry>();
        public IReadOnlyList<HueEntry> Entries => _entries.AsReadOnly();
        public int Count => _entries.Count;

        /// <summary>Erstellt leere hues.mul mit 3000 Einträgen.</summary>
        public static HuesFile CreateEmpty()
        {
            var f = new HuesFile();
            for (int i = 0; i < TotalEntries; i++) f._entries.Add(new HueEntry());
            return f;
        }

        public void SaveToFile(string path)
        {
            EnsureDir(path);
            using var w = new BinaryWriter(File.Open(path, FileMode.Create));
            // 375 Gruppen, je 4-Byte-Header + 8 Einträge
            for (int g = 0; g < TotalGroups; g++)
            {
                w.Write(0); // Gruppen-Header
                for (int i = 0; i < EntriesPerGroup; i++)
                {
                    int idx = g * EntriesPerGroup + i;
                    (idx < _entries.Count ? _entries[idx] : new HueEntry()).WriteTo(w);
                }
            }
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("hues.mul nicht gefunden.", path);
            _entries.Clear();
            using var r = new BinaryReader(File.OpenRead(path));
            while (r.BaseStream.Position + GroupHeaderSize + EntriesPerGroup * HueEntry.ByteSize <= r.BaseStream.Length)
            {
                r.ReadInt32(); // Header
                for (int i = 0; i < EntriesPerGroup; i++) _entries.Add(HueEntry.ReadFrom(r));
            }
        }

        public string GetSummary() => $"hues.mul: {Count:N0} Hue-Einträge  (Dateigröße soll: {TotalGroups * (GroupHeaderSize + EntriesPerGroup * HueEntry.ByteSize):N0} Byte)";

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    // =========================================================================
    //  NEU: MapFile  –  map*.mul
    //  Terrain-Tiles: 3 Byte je Cell (ushort TileID + sbyte Z)
    //  Block: 196 Byte = 4-Byte-Header + 64 Cells × 3 Byte
    //  Standard Felucca: 7168 × 4096 Tiles = 448 × 256 Blöcke
    // =========================================================================

    public sealed class MapCell
    {
        public ushort TileID { get; set; }
        public sbyte Z { get; set; }

        public void WriteTo(BinaryWriter w) { w.Write(TileID); w.Write(Z); }
        public static MapCell ReadFrom(BinaryReader r) => new MapCell { TileID = r.ReadUInt16(), Z = r.ReadSByte() };
    }

    public sealed class MapBlock
    {
        public uint Header { get; set; }
        public MapCell[] Cells { get; set; } = new MapCell[64];

        public const int ByteSize = 4 + 64 * 3; // 196 Byte

        public void WriteTo(BinaryWriter w)
        {
            w.Write(Header);
            foreach (var c in Cells) (c ?? new MapCell()).WriteTo(w);
        }

        public static MapBlock ReadFrom(BinaryReader r)
        {
            var b = new MapBlock { Header = r.ReadUInt32() };
            for (int i = 0; i < 64; i++) b.Cells[i] = MapCell.ReadFrom(r);
            return b;
        }
    }

    public sealed class MapFile
    {
        // Bekannte Standard-Kartengrößen in Tiles
        public static readonly (string Name, int Width, int Height)[] KnownSizes =
        {
            ("Felucca / Trammel", 7168, 4096),
            ("Ilshenar",          2304, 1600),
            ("Malas",             2560, 2048),
            ("Tokuno",            1448, 1448),
            ("Ter Mur",           1280, 4096),
            ("Custom 512×512",     512,  512),
            ("Custom 1024×1024", 1024, 1024),
            ("Custom 2048×2048", 2048, 2048),
        };

        public int Width { get; private set; } // in Tiles
        public int Height { get; private set; }

        public int BlocksX => Width / 8;
        public int BlocksY => Height / 8;
        public int TotalBlocks => BlocksX * BlocksY;
        public long ExpectedFileSize => (long)TotalBlocks * MapBlock.ByteSize;

        /// <summary>
        /// Erstellt eine leere map*.mul mit wählbarer Größe.
        /// Alle Tiles = ID 0x0002 (Dirt), Z = 0.
        /// </summary>
        public static string CreateEmpty(string directory, string filename, int widthTiles, int heightTiles)
        {
            if (widthTiles % 8 != 0 || heightTiles % 8 != 0)
                throw new ArgumentException("Breite und Höhe müssen durch 8 teilbar sein.");

            string path = Path.Combine(directory, filename);
            EnsureDir(path);

            int bx = widthTiles / 8, by = heightTiles / 8;
            using var w = new BinaryWriter(File.Open(path, FileMode.Create));

            var block = new MapBlock { Header = 0 };
            for (int i = 0; i < 64; i++) block.Cells[i] = new MapCell { TileID = 0x0002, Z = 0 };

            for (int x = 0; x < bx; x++)
                for (int y = 0; y < by; y++)
                    block.WriteTo(w);

            long size = new FileInfo(path).Length;
            return $"map erstellt: {widthTiles}×{heightTiles} Tiles  ({bx}×{by} Blöcke)\n  {path}\n  Größe: {size:N0} Byte";
        }

        public static long CalculateFileSize(int widthTiles, int heightTiles) =>
            (long)(widthTiles / 8) * (heightTiles / 8) * MapBlock.ByteSize;

        /// <summary>Gibt Info über Blockanzahl und Dateigröße zurück, ohne Datei zu laden.</summary>
        public static string GetSizeInfo(int widthTiles, int heightTiles)
        {
            long sz = CalculateFileSize(widthTiles, heightTiles);
            return $"{widthTiles}×{heightTiles} Tiles  →  {widthTiles / 8}×{heightTiles / 8} Blöcke  →  Dateigröße: {sz:N0} Byte  ({sz / 1024.0 / 1024.0:F1} MB)";
        }

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    // =========================================================================
    //  NEU: StaticsFile  –  statics*.mul + staidx*.mul
    //  IDX: int Lookup | int Length | int Unknown  (12 Byte je Block)
    //  MUL: Für jeden Block eine Liste von Static-Objekten (7 Byte je Objekt)
    //       ushort TileID | byte X | byte Y | sbyte Z | ushort Hue
    // =========================================================================

    public sealed class StaticObject
    {
        public ushort TileID { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public sbyte Z { get; set; }
        public ushort Hue { get; set; }

        public const int ByteSize = 7;

        public void WriteTo(BinaryWriter w) { w.Write(TileID); w.Write(X); w.Write(Y); w.Write(Z); w.Write(Hue); }
        public static StaticObject ReadFrom(BinaryReader r) =>
            new StaticObject { TileID = r.ReadUInt16(), X = r.ReadByte(), Y = r.ReadByte(), Z = r.ReadSByte(), Hue = r.ReadUInt16() };

        public override string ToString() => $"TileID=0x{TileID:X4}  X={X}  Y={Y}  Z={Z}  Hue={Hue}";
    }

    public sealed class StaticsFile
    {
        /// <summary>
        /// Erstellt leere statics*.mul + staidx*.mul für eine Karte der angegebenen Größe.
        /// Alle IDX-Einträge haben Lookup=0xFFFFFFFF (leer).
        /// </summary>
        public static string CreateEmpty(string directory, string staticName, string idxName,
            int mapWidthTiles, int mapHeightTiles)
        {
            string mulPath = Path.Combine(directory, staticName);
            string idxPath = Path.Combine(directory, idxName);
            EnsureDir(mulPath);

            int totalBlocks = (mapWidthTiles / 8) * (mapHeightTiles / 8);

            using var iw = new BinaryWriter(File.Open(idxPath, FileMode.Create));
            for (int i = 0; i < totalBlocks; i++)
            { iw.Write(0xFFFFFFFF); iw.Write(0); iw.Write(0); }

            using var _ = File.Create(mulPath); // Leere MUL

            return $"Statics erstellt ({totalBlocks:N0} Blöcke):\n  {idxPath}\n  {mulPath}";
        }

        public static List<StaticObject> ReadBlock(string mulPath, string idxPath, int blockIndex)
        {
            var result = new List<StaticObject>();
            using var ir = new BinaryReader(File.OpenRead(idxPath));
            ir.BaseStream.Seek((long)blockIndex * 12, SeekOrigin.Begin);
            uint lookup = ir.ReadUInt32(); int length = ir.ReadInt32();
            if (lookup == 0xFFFFFFFF || length <= 0) return result;
            using var mr = new BinaryReader(File.OpenRead(mulPath));
            mr.BaseStream.Seek(lookup, SeekOrigin.Begin);
            int count = length / StaticObject.ByteSize;
            for (int i = 0; i < count; i++) result.Add(StaticObject.ReadFrom(mr));
            return result;
        }

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    // =========================================================================
    //  NEU: MultiFile  –  multi.mul + multi.idx
    //  IDX: 12 Byte (Lookup, Size, Unknown) wie üblich
    //  MUL: Für jede Multi-Definition eine Liste von Einträgen
    //       Legacy:  short X | short Y | short Z | ushort TileID | uint Flags  (12 Byte)
    //       HighSeas: + uint Extra  (16 Byte)
    // =========================================================================

    public sealed class MultiTileEntry
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public ushort TileID { get; set; }
        public uint Flags { get; set; }
        public uint Extra { get; set; } // nur HighSeas

        public const int LegacySize = 12;
        public const int HighSeasSize = 16;

        public void WriteTo(BinaryWriter w, bool highSeas = false)
        { w.Write(X); w.Write(Y); w.Write(Z); w.Write(TileID); w.Write(Flags); if (highSeas) w.Write(Extra); }

        public static MultiTileEntry ReadFrom(BinaryReader r, bool highSeas = false)
        {
            var e = new MultiTileEntry { X = r.ReadInt16(), Y = r.ReadInt16(), Z = r.ReadInt16(), TileID = r.ReadUInt16(), Flags = r.ReadUInt32() };
            if (highSeas) e.Extra = r.ReadUInt32();
            return e;
        }

        public override string ToString() => $"TileID=0x{TileID:X4}  X={X,4}  Y={Y,4}  Z={Z,4}  Flags=0x{Flags:X8}";
    }

    public sealed class MultiFile
    {
        /// <summary>
        /// Erstellt leere multi.mul + multi.idx mit <paramref name="count"/> Einträgen.
        /// Jeder Eintrag enthält ein einzelnes "unsichtbares" Tile als Platzhalter.
        /// </summary>
        public static string CreateEmpty(string directory, int count)
        {
            string mulPath = Path.Combine(directory, "multi.mul");
            string idxPath = Path.Combine(directory, "multi.idx");
            EnsureDir(mulPath);

            using var mw = new BinaryWriter(File.Open(mulPath, FileMode.Create));
            using var iw = new BinaryWriter(File.Open(idxPath, FileMode.Create));

            // Ein Platzhalter-Tile pro Multi
            var placeholder = new MultiTileEntry { X = 0, Y = 0, Z = 0, TileID = 0x0001, Flags = 0 };
            long pos = 0;

            for (int i = 0; i < count; i++)
            {
                iw.Write((uint)pos); iw.Write(MultiTileEntry.LegacySize); iw.Write(0u);
                placeholder.WriteTo(mw);
                pos += MultiTileEntry.LegacySize;
            }

            return $"Multi erstellt ({count:N0} Einträge):\n  {idxPath}\n  {mulPath}";
        }

        public static List<MultiTileEntry> ReadMulti(string directory, int index, bool highSeas = false)
        {
            string mulPath = Path.Combine(directory, "multi.mul");
            string idxPath = Path.Combine(directory, "multi.idx");
            var result = new List<MultiTileEntry>();
            using var ir = new BinaryReader(File.OpenRead(idxPath));
            ir.BaseStream.Seek((long)index * 12, SeekOrigin.Begin);
            uint lookup = ir.ReadUInt32(); int size = ir.ReadInt32();
            if (lookup == 0xFFFFFFFF || size <= 0) return result;
            using var mr = new BinaryReader(File.OpenRead(mulPath));
            mr.BaseStream.Seek(lookup, SeekOrigin.Begin);
            int stride = highSeas ? MultiTileEntry.HighSeasSize : MultiTileEntry.LegacySize;
            int count = size / stride;
            for (int i = 0; i < count; i++) result.Add(MultiTileEntry.ReadFrom(mr, highSeas));
            return result;
        }

        public static int CountEntries(string directory)
        {
            string p = Path.Combine(directory, "multi.idx");
            return File.Exists(p) ? (int)(new FileInfo(p).Length / 12) : 0;
        }

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    // =========================================================================
    //  NEU: SkillsFile  –  skills.mul + skills.idx
    //  IDX: 12 Byte (Lookup, Size, Unknown) – Lookup zeigt auf 1 Byte in MUL
    //  MUL: byte UseType + float[?] Titel-String (variabel)
    //       Layout je Eintrag: byte UseType + null-terminierter Titel-String
    // =========================================================================

    public sealed class SkillEntry
    {
        public byte UseType { get; set; }  // 0 = passiv, 1 = aktiv
        public string Name { get; set; } = string.Empty;

        public override string ToString() => $"[{(UseType == 1 ? "aktiv" : "passiv")}]  {Name}";
    }

    public sealed class SkillsFile
    {
        private readonly List<SkillEntry> _skills = new List<SkillEntry>();
        public IReadOnlyList<SkillEntry> Skills => _skills.AsReadOnly();
        public int Count => _skills.Count;

        /// <summary>Standard-UO-Skills (58 Skills für Classic-Server).</summary>
        public static readonly string[] DefaultSkillNames =
        {
            "Alchemy","Anatomy","Animal Lore","Item Identification","Arms Lore","Parrying",
            "Begging","Blacksmithy","Bowcraft / Fletching","Peacemaking","Camping","Carpentry",
            "Cartography","Cooking","Detecting Hidden","Discordance","Evaluating Intelligence",
            "Healing","Fishing","Forensic Evaluation","Herding","Hiding","Provocation",
            "Inscription","Lockpicking","Magery","Magic Resist","Tactics","Snooping",
            "Musicianship","Poisoning","Archery","Spirit Speak","Stealing","Tailoring",
            "Animal Taming","Taste Identification","Tinkering","Tracking","Veterinary","Swordsmanship",
            "Mace Fighting","Fencing","Wrestling","Lumberjacking","Mining","Meditation",
            "Stealth","Remove Trap","Necromancy","Focus","Chivalry","Bushido","Ninjitsu",
            "Spellweaving","Mysticism","Imbuing","Throwing"
        };

        public static SkillsFile CreateDefault()
        {
            var f = new SkillsFile();
            foreach (var n in DefaultSkillNames) f._skills.Add(new SkillEntry { UseType = 1, Name = n });
            return f;
        }

        public static SkillsFile CreateEmpty(int count)
        {
            var f = new SkillsFile();
            for (int i = 0; i < count; i++) f._skills.Add(new SkillEntry { UseType = 0, Name = $"Skill_{i}" });
            return f;
        }

        public void SaveToFile(string directory)
        {
            string mulPath = Path.Combine(directory, "skills.mul");
            string idxPath = Path.Combine(directory, "skills.idx");
            EnsureDir(mulPath);

            using var mw = new BinaryWriter(File.Open(mulPath, FileMode.Create));
            using var iw = new BinaryWriter(File.Open(idxPath, FileMode.Create));

            long pos = 0;
            foreach (var s in _skills)
            {
                byte[] nameBytes = Encoding.ASCII.GetBytes(s.Name + "\0");
                int sz = 1 + nameBytes.Length;
                iw.Write((uint)pos); iw.Write(sz); iw.Write(0u);
                mw.Write(s.UseType); mw.Write(nameBytes);
                pos += sz;
            }
            // Abschluss-Eintrag
            iw.Write((uint)pos); iw.Write(0); iw.Write(0u);
        }

        public void LoadFromFile(string directory)
        {
            string mulPath = Path.Combine(directory, "skills.mul");
            string idxPath = Path.Combine(directory, "skills.idx");
            if (!File.Exists(idxPath) || !File.Exists(mulPath)) throw new FileNotFoundException("skills.mul / skills.idx nicht gefunden.");
            _skills.Clear();
            using var ir = new BinaryReader(File.OpenRead(idxPath));
            using var mr = new BinaryReader(File.OpenRead(mulPath));
            while (ir.BaseStream.Position + 12 <= ir.BaseStream.Length)
            {
                uint lk = ir.ReadUInt32(); int sz = ir.ReadInt32(); ir.ReadUInt32();
                if (lk == 0xFFFFFFFF || sz <= 0) break;
                mr.BaseStream.Seek(lk, SeekOrigin.Begin);
                byte use = mr.ReadByte();
                var sb = new StringBuilder();
                while (mr.BaseStream.Position < mr.BaseStream.Length) { byte b = mr.ReadByte(); if (b == 0) break; sb.Append((char)b); }
                _skills.Add(new SkillEntry { UseType = use, Name = sb.ToString() });
            }
        }

        public string GetSummary()
        {
            var sb = new StringBuilder($"Skills: {Count}\n");
            for (int i = 0; i < _skills.Count; i++) sb.AppendLine($"  [{i,3}] {_skills[i]}");
            return sb.ToString();
        }

        static void EnsureDir(string p) { string d = Path.GetDirectoryName(p); if (!string.IsNullOrEmpty(d) && !Directory.Exists(d)) Directory.CreateDirectory(d); }
    }

    // =========================================================================
    //  NEU: MulValidator  –  prüft IDX ↔ MUL Konsistenz
    // =========================================================================

    public sealed class ValidationResult
    {
        public int TotalEntries { get; set; }
        public int EmptyEntries { get; set; }
        public int ValidEntries { get; set; }
        public int BrokenEntries { get; set; }
        public long MulFileSize { get; set; }
        public List<string> Issues { get; } = new List<string>();

        public bool IsHealthy => BrokenEntries == 0;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== Validierungs-Ergebnis ===");
            sb.AppendLine($"  Gesamt:   {TotalEntries:N0}");
            sb.AppendLine($"  Leer:     {EmptyEntries:N0}");
            sb.AppendLine($"  Gültig:   {ValidEntries:N0}");
            sb.AppendLine($"  Defekt:   {BrokenEntries:N0}");
            sb.AppendLine($"  MUL-Größe:{MulFileSize:N0} Byte");
            sb.AppendLine(IsHealthy ? "  ✓ Keine Fehler gefunden." : "  ✗ Fehler gefunden!");
            if (Issues.Count > 0)
            {
                sb.AppendLine($"\nProbleme ({Issues.Count}):");
                foreach (var iss in Issues.Take(200)) sb.AppendLine("  " + iss);
                if (Issues.Count > 200) sb.AppendLine($"  ... ({Issues.Count - 200} weitere)");
            }
            return sb.ToString();
        }
    }

    public static class MulValidator
    {
        /// <summary>
        /// Prüft IDX ↔ MUL-Konsistenz.
        /// Findet: Lookup außerhalb MUL, Size=0 mit gültigem Lookup, Lookup+Size > MUL-Ende.
        /// </summary>
        public static ValidationResult Validate(string idxPath, string mulPath)
        {
            var res = new ValidationResult();
            if (!File.Exists(idxPath)) { res.Issues.Add($"IDX nicht gefunden: {idxPath}"); return res; }

            bool hasMul = File.Exists(mulPath);
            res.MulFileSize = hasMul ? new FileInfo(mulPath).Length : -1;

            var idx = new MulIndexFile();
            idx.LoadFromFile(idxPath);
            res.TotalEntries = idx.Count;

            for (int i = 0; i < idx.Count; i++)
            {
                var e = idx[i];
                if (e.IsEmpty) { res.EmptyEntries++; continue; }

                bool ok = true;
                if (!hasMul)
                { res.Issues.Add($"[{i}] MUL fehlt, Eintrag hat Lookup={e.Lookup}"); ok = false; }
                else
                {
                    if (e.Lookup < 0 || e.Lookup >= res.MulFileSize)
                    { res.Issues.Add($"[{i}] Lookup=0x{e.Lookup:X} außerhalb MUL (Größe={res.MulFileSize})"); ok = false; }
                    else if (e.Size <= 0)
                    { res.Issues.Add($"[{i}] Lookup gültig aber Size=0"); ok = false; }
                    else if (e.Lookup + e.Size > res.MulFileSize)
                    { res.Issues.Add($"[{i}] Lookup+Size={e.Lookup + e.Size} überschreitet MUL-Ende ({res.MulFileSize})"); ok = false; }
                }

                if (ok) res.ValidEntries++; else res.BrokenEntries++;
            }
            return res;
        }

        /// <summary>Vergleicht zwei Shard-Verzeichnisse auf Dateigrößen-Konsistenz.</summary>
        public static string CompareDirectories(string dirA, string dirB)
        {
            if (!Directory.Exists(dirA)) return $"Verzeichnis nicht gefunden: {dirA}";
            if (!Directory.Exists(dirB)) return $"Verzeichnis nicht gefunden: {dirB}";

            var filesA = Directory.GetFiles(dirA, "*.mul", SearchOption.TopDirectoryOnly)
                                  .Select(f => Path.GetFileName(f).ToLowerInvariant()).ToHashSet();
            var filesB = Directory.GetFiles(dirB, "*.mul", SearchOption.TopDirectoryOnly)
                                  .Select(f => Path.GetFileName(f).ToLowerInvariant()).ToHashSet();

            var sb = new StringBuilder($"Verzeichnis-Vergleich:\n  A: {dirA}\n  B: {dirB}\n\n");

            var allFiles = filesA.Union(filesB).OrderBy(f => f);
            foreach (var fn in allFiles)
            {
                bool inA = filesA.Contains(fn), inB = filesB.Contains(fn);
                if (!inA) { sb.AppendLine($"  ✗ Nur in B: {fn}"); continue; }
                if (!inB) { sb.AppendLine($"  ✗ Nur in A: {fn}"); continue; }

                long sA = new FileInfo(Path.Combine(dirA, fn)).Length;
                long sB = new FileInfo(Path.Combine(dirB, fn)).Length;
                string mark = sA == sB ? "✓" : "≠";
                sb.AppendLine($"  {mark} {fn,-30} A={sA,12:N0}  B={sB,12:N0}  Diff={sB - sA:+#;-#;0}");
            }
            return sb.ToString();
        }
    }

    // =========================================================================
    //  NEU: IdxPatcher  –  schreibt einzelne IDX-Einträge gezielt zurück
    // =========================================================================

    public static class IdxPatcher
    {
        /// <summary>Überschreibt einen einzelnen Eintrag in einer bestehenden IDX-Datei.</summary>
        public static string PatchEntry(string idxPath, int entryIndex,
            long lookup, long size, uint unknown = 0)
        {
            if (!File.Exists(idxPath)) return $"IDX nicht gefunden: {idxPath}";

            // Format auto-erkennen
            var idx = new MulIndexFile();
            idx.LoadFromFile(idxPath);
            if (entryIndex < 0 || entryIndex >= idx.Count)
                return $"Index {entryIndex} außerhalb Bereich (0..{idx.Count - 1}).";

            var e = idx[entryIndex];
            e.Lookup = lookup;
            e.Size = size;
            e.Unknown = unknown;
            idx.SaveToFile(idxPath);
            return $"Eintrag [{entryIndex}] gepatcht:\n  Lookup=0x{lookup:X}  Size={size}  Unknown=0x{unknown:X}";
        }

        /// <summary>Löscht (leert) einen einzelnen Eintrag.</summary>
        public static string ClearEntry(string idxPath, int entryIndex)
        {
            if (!File.Exists(idxPath)) return $"IDX nicht gefunden: {idxPath}";
            var idx = new MulIndexFile(); idx.LoadFromFile(idxPath);
            if (entryIndex < 0 || entryIndex >= idx.Count)
                return $"Index {entryIndex} außerhalb Bereich (0..{idx.Count - 1}).";
            idx.ClearEntry(entryIndex);
            idx.SaveToFile(idxPath);
            return $"Eintrag [{entryIndex}] geleert.";
        }

        /// <summary>Gibt alle Einträge eines Bereichs als formatierten String zurück.</summary>
        public static string ReadRange(string idxPath, int from, int count)
        {
            var idx = new MulIndexFile(); idx.LoadFromFile(idxPath);
            int to = Math.Min(from + count, idx.Count);
            var sb = new StringBuilder($"Einträge [{from}..{to - 1}] aus {idx.Count:N0}:\n");
            for (int i = from; i < to; i++) sb.AppendLine($"  [{i,6}] {idx[i]}");
            return sb.ToString();
        }
    }

    // =========================================================================
    //  NEU: BatchSetup  –  alle Standard-Dateien für einen leeren Shard
    // =========================================================================

    public sealed class BatchSetupOptions
    {
        public int MapWidth { get; set; } = 7168;
        public int MapHeight { get; set; } = 4096;
        public int MapIndex { get; set; } = 0;    // 0 = map0.mul
        public int ArtEntries { get; set; } = 81884;
        public int HueCount { get; set; } = HuesFile.TotalEntries;
        public int SoundCount { get; set; } = 4095;
        public int GumpCount { get; set; } = 65535;
        public int MultiCount { get; set; } = 5000;
        public int TileDataLandGroups { get; set; } = 512;
        public int TileDataStaticGroups { get; set; } = 2048;
        public bool CreateSkills { get; set; } = true;
        public bool CreateDefaultSkills { get; set; } = true;
        public int SkillCount { get; set; } = 58;
    }

    public static class BatchSetup
    {
        /// <summary>
        /// Erstellt alle Standard-Dateien für einen leeren UO-Custom-Shard.
        /// Gibt Fortschritts-Log zurück.
        /// </summary>
        public static string CreateAll(string directory, BatchSetupOptions opt = null,
            Action<string> progress = null)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            opt = opt ?? new BatchSetupOptions();
            var log = new StringBuilder();

            void Step(string name, Func<string> action)
            {
                try { string r = action(); log.AppendLine($"✓ {name}: {r}"); progress?.Invoke($"✓ {name}"); }
                catch (Exception ex) { log.AppendLine($"✗ {name}: {ex.Message}"); progress?.Invoke($"✗ {name}: {ex.Message}"); }
            }

            // Art
            Step("artidx.mul + art.mul", () =>
                MulFileHelper.CreateIndexAndMul(directory, "artidx.mul", "art.mul", opt.ArtEntries));

            // TileData
            Step("tiledata.mul", () =>
            { TileDataFile.CreateEmpty(opt.TileDataLandGroups, opt.TileDataStaticGroups).SaveToFile(Path.Combine(directory, "tiledata.mul")); return $"{opt.TileDataLandGroups * 32:N0} Land + {opt.TileDataStaticGroups * 32:N0} Static Tiles"; });

            // Map + Statics
            Step($"map{opt.MapIndex}.mul", () =>
                MapFile.CreateEmpty(directory, $"map{opt.MapIndex}.mul", opt.MapWidth, opt.MapHeight));

            Step($"statics{opt.MapIndex}.mul + staidx{opt.MapIndex}.mul", () =>
                StaticsFile.CreateEmpty(directory, $"statics{opt.MapIndex}.mul", $"staidx{opt.MapIndex}.mul", opt.MapWidth, opt.MapHeight));

            // Gump
            Step("GUMPIDX.MUL + GUMPART.MUL", () =>
            { GumpFile.CreateEmpty(directory, opt.GumpCount); return $"{opt.GumpCount:N0} Einträge"; });

            // Sound
            Step("SoundIdx.mul + Sound.mul", () => SoundFile.CreateEmpty(directory, opt.SoundCount));

            // Hues
            Step("hues.mul", () =>
            { HuesFile.CreateEmpty().SaveToFile(Path.Combine(directory, "hues.mul")); return $"{opt.HueCount:N0} Einträge"; });

            // Multi
            Step("multi.mul + multi.idx", () => MultiFile.CreateEmpty(directory, opt.MultiCount));

            // Skills
            if (opt.CreateSkills)
                Step("skills.mul + skills.idx", () =>
                {
                    var sf = opt.CreateDefaultSkills ? SkillsFile.CreateDefault() : SkillsFile.CreateEmpty(opt.SkillCount);
                    sf.SaveToFile(directory);
                    return $"{sf.Count} Skills";
                });

            // RadarColor
            Step("radarcol.mul", () =>
            {
                string p = Path.Combine(directory, "radarcol.mul");
                using var w = new BinaryWriter(File.Open(p, FileMode.Create));
                for (int i = 0; i < 0x14000; i++) w.Write((short)0);
                return "0x14000 Einträge";
            });

            // TexMaps
            Step("texmaps.mul + texidx.mul", () =>
            { MulFileHelper.CreateIndexAndMul(directory, "texidx.mul", "texmaps.mul", 16383); return "16383 Einträge"; });

            log.Insert(0, $"=== Batch-Setup abgeschlossen: {directory} ===\n");
            return log.ToString();
        }
    }

    // =========================================================================
    //  NEU: HexViewHelper  –  byte-genaues Lesen beliebiger MUL-Dateien
    // =========================================================================

    public static class HexViewHelper
    {
        /// <summary>
        /// Liest <paramref name="length"/> Bytes ab <paramref name="offset"/> aus einer Datei
        /// und gibt formatierte Hex+ASCII-Ausgabe zurück.
        /// </summary>
        public static string ReadHex(string filePath, long offset, int length, int bytesPerLine = 16)
        {
            if (!File.Exists(filePath)) return $"Datei nicht gefunden: {filePath}";
            length = Math.Max(1, Math.Min(length, 65536)); // max 64 KB

            using var fs = File.OpenRead(filePath);
            if (offset < 0 || offset >= fs.Length) return $"Offset 0x{offset:X} außerhalb der Datei ({fs.Length:N0} Byte).";

            fs.Seek(offset, SeekOrigin.Begin);
            int toRead = (int)Math.Min(length, fs.Length - offset);
            byte[] buf = new byte[toRead]; fs.Read(buf, 0, toRead);

            var sb = new StringBuilder();
            sb.AppendLine($"Datei:  {Path.GetFileName(filePath)}");
            sb.AppendLine($"Offset: 0x{offset:X8}  ({offset:N0})");
            sb.AppendLine($"Länge:  {toRead:N0} Byte");
            sb.AppendLine(new string('─', 74));

            for (int row = 0; row < toRead; row += bytesPerLine)
            {
                sb.Append($"  {offset + row:X8}  ");
                int cols = Math.Min(bytesPerLine, toRead - row);
                for (int c = 0; c < bytesPerLine; c++)
                {
                    if (c < cols) sb.Append($"{buf[row + c]:X2} "); else sb.Append("   ");
                    if (c == bytesPerLine / 2 - 1) sb.Append(' ');
                }
                sb.Append(" |");
                for (int c = 0; c < cols; c++) { byte b = buf[row + c]; sb.Append(b >= 32 && b < 127 ? (char)b : '.'); }
                sb.AppendLine("|");
            }
            return sb.ToString();
        }

        /// <summary>Gibt Datei-Infos zurück (Größe, Datum, mögliche IDX-Eintragsanzahl).</summary>
        public static string GetFileInfo(string filePath)
        {
            if (!File.Exists(filePath)) return $"Datei nicht gefunden: {filePath}";
            var fi = new FileInfo(filePath);
            var sb = new StringBuilder();
            sb.AppendLine($"Datei:       {fi.Name}");
            sb.AppendLine($"Größe:       {fi.Length:N0} Byte  ({fi.Length / 1024.0 / 1024.0:F2} MB)");
            sb.AppendLine($"Geändert:    {fi.LastWriteTime:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"Als IDX 12B: {fi.Length / 12:N0} Einträge");
            sb.AppendLine($"Als IDX 20B: {fi.Length / 20:N0} Einträge");
            sb.AppendLine($"Als map-Blk: {fi.Length / MapBlock.ByteSize:N0} Blöcke ({(int)Math.Sqrt(fi.Length / MapBlock.ByteSize)}² möglich)");
            return sb.ToString();
        }

        /// <summary>Durchsucht eine Datei nach einem Byte-Muster.</summary>
        public static string SearchPattern(string filePath, byte[] pattern, int maxResults = 50)
        {
            if (!File.Exists(filePath)) return $"Datei nicht gefunden: {filePath}";
            if (pattern == null || pattern.Length == 0) return "Leeres Suchmuster.";

            using var fs = File.OpenRead(filePath);
            var buf = new byte[65536];
            var hits = new List<long>();
            long globalOffset = 0;

            while (globalOffset < fs.Length && hits.Count < maxResults)
            {
                int read = fs.Read(buf, 0, buf.Length);
                for (int i = 0; i <= read - pattern.Length; i++)
                {
                    bool match = true;
                    for (int j = 0; j < pattern.Length; j++) if (buf[i + j] != pattern[j]) { match = false; break; }
                    if (match) hits.Add(globalOffset + i);
                    if (hits.Count >= maxResults) break;
                }
                globalOffset += read - pattern.Length + 1;
                if (globalOffset > 0) fs.Seek(globalOffset, SeekOrigin.Begin);
            }

            var sb = new StringBuilder($"Pattern: {BitConverter.ToString(pattern)}\nTreffer: {hits.Count}{(hits.Count >= maxResults ? " (Limit)" : "")}\n");
            foreach (var h in hits) sb.AppendLine($"  0x{h:X8}  ({h:N0})");
            return sb.ToString();
        }
    }
}
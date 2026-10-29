/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using UoFiddler.Plugin.Enhanced.Core.StringDictionary;
using UoFiddler.Plugin.Enhanced.Core.TileArt;

namespace UoFiddler.Plugin.Enhanced.Classes
{
    /// <summary>
    /// PropertyGrid wrapper around a <see cref="TileArtRecord"/>. Every change marks the record as modified.
    /// </summary>
    public sealed class TileArtEntryView
    {
        private readonly TileArtRecord _record;
        private readonly TileArtFile _file;
        private readonly EcStringDictionary _strings;
        private readonly Action _changed;

        public TileArtEntryView(TileArtRecord record, TileArtFile file, EcStringDictionary strings, Action changed)
        {
            _record = record;
            _file = file;
            _strings = strings;
            _changed = changed;
        }

        private TileArtEntry E => _record.Entry;

        private void Changed()
        {
            _file.MarkModified(_record);
            _changed?.Invoke();
        }

        // ------------------------------------------------------------- general

        [Category("1 General"), DisplayName("Id"), Description("Id of the packed file (build/tileart/<id>.bin). Use 'Duplicate' to create an entry with another id.")]
        public uint Id => _record.Id;

        [Category("1 General"), DisplayName("Id (hex)")]
        public string IdHex => $"0x{_record.Id:X}";

        [Category("1 General"), DisplayName("Id in data"), Description("Id stored inside the entry. Normally the same as the file id.")]
        public uint TileId
        {
            get => E.TileId;
            set { E.TileId = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Name"), Description("Name from string_dictionary.uop. A new text is added to the dictionary when it doesn't exist yet.")]
        public string Name
        {
            get => _strings?.Get(E.NameIndex) ?? (E.NameIndex == 0 ? string.Empty : $"#{E.NameIndex}");
            set
            {
                if (_strings == null)
                {
                    throw new InvalidOperationException("string_dictionary.uop is not loaded.");
                }
                E.NameIndex = string.IsNullOrEmpty(value) ? 0 : _strings.GetOrAdd(value);
                Changed();
            }
        }

        [Category("1 General"), DisplayName("Name index"), Description("1 based index into string_dictionary.uop.")]
        public uint NameIndex
        {
            get => E.NameIndex;
            set { E.NameIndex = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Old id (legacy)"), Description("Legacy id; 16777215 (0xFFFFFF) = none.")]
        public int OldId
        {
            get => E.OldId;
            set { E.OldId = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Type")]
        public int Type
        {
            get => E.Type;
            set { E.Type = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Facing")]
        public int Facing
        {
            get => E.Facing;
            set { E.Facing = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Flags (hex)"), Description("Flag field 1 – easier to edit in the flag list.")]
        public string FlagsHex
        {
            get => $"0x{(ulong)E.Flags:X}";
            set { E.Flags = (TileArtFlags)ParseULong(value); Changed(); }
        }

        [Category("1 General"), DisplayName("Flags 2 (hex)"), Description("Second flag field (meaning unknown).")]
        public string Flags2Hex
        {
            get => $"0x{E.Flags2:X}";
            set { E.Flags2 = ParseULong(value); Changed(); }
        }

        [Category("1 General"), DisplayName("Radar color")]
        public Color RadarColor
        {
            get => Color.FromArgb(E.RadarA, E.RadarR, E.RadarG, E.RadarB);
            set
            {
                E.RadarR = value.R;
                E.RadarG = value.G;
                E.RadarB = value.B;
                E.RadarA = value.A;
                Changed();
            }
        }

        [Category("1 General"), DisplayName("Light 1")]
        public float Light1
        {
            get => E.Light1;
            set { E.Light1 = value; Changed(); }
        }

        [Category("1 General"), DisplayName("Light 2")]
        public float Light2
        {
            get => E.Light2;
            set { E.Light2 = value; Changed(); }
        }

        // ------------------------------------------------------------- properties

        private string GetProp(TileArtPropertyId id)
        {
            TileArtProperty p = E.Properties.FirstOrDefault(x => x.Id == id);
            return p == null ? string.Empty : p.Value.ToString(CultureInfo.InvariantCulture);
        }

        private void SetProp(TileArtPropertyId id, string value)
        {
            TileArtProperty p = E.Properties.FirstOrDefault(x => x.Id == id);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (p != null)
                {
                    E.Properties.Remove(p);
                }
            }
            else
            {
                uint v = (uint)ParseULong(value);
                if (p == null)
                {
                    E.Properties.Add(new TileArtProperty { Id = id, Value = v });
                }
                else
                {
                    p.Value = v;
                }
            }
            Changed();
        }

        private const string PropHint = "Empty = property not present.";

        [Category("2 Properties"), Description(PropHint)] public string Weight { get => GetProp(TileArtPropertyId.Weight); set => SetProp(TileArtPropertyId.Weight, value); }
        [Category("2 Properties"), Description(PropHint)] public string Quality { get => GetProp(TileArtPropertyId.Quality); set => SetProp(TileArtPropertyId.Quality, value); }
        [Category("2 Properties"), Description(PropHint)] public string Quantity { get => GetProp(TileArtPropertyId.Quantity); set => SetProp(TileArtPropertyId.Quantity, value); }
        [Category("2 Properties"), Description(PropHint)] public string Height { get => GetProp(TileArtPropertyId.Height); set => SetProp(TileArtPropertyId.Height, value); }
        [Category("2 Properties"), Description(PropHint)] public string Value { get => GetProp(TileArtPropertyId.Value); set => SetProp(TileArtPropertyId.Value, value); }
        [Category("2 Properties"), DisplayName("AC / VC"), Description(PropHint)] public string AcVc { get => GetProp(TileArtPropertyId.AcVc); set => SetProp(TileArtPropertyId.AcVc, value); }
        [Category("2 Properties"), Description(PropHint)] public string Slot { get => GetProp(TileArtPropertyId.Slot); set => SetProp(TileArtPropertyId.Slot, value); }
        [Category("2 Properties"), DisplayName("Off C8"), Description(PropHint)] public string OffC8 { get => GetProp(TileArtPropertyId.Off_C8); set => SetProp(TileArtPropertyId.Off_C8, value); }
        [Category("2 Properties"), Description(PropHint)] public string Appearance { get => GetProp(TileArtPropertyId.Appearance); set => SetProp(TileArtPropertyId.Appearance, value); }
        [Category("2 Properties"), Description(PropHint)] public string Race { get => GetProp(TileArtPropertyId.Race); set => SetProp(TileArtPropertyId.Race, value); }
        [Category("2 Properties"), Description(PropHint)] public string Gender { get => GetProp(TileArtPropertyId.Gender); set => SetProp(TileArtPropertyId.Gender, value); }
        [Category("2 Properties"), Description(PropHint)] public string Paperdoll { get => GetProp(TileArtPropertyId.Paperdoll); set => SetProp(TileArtPropertyId.Paperdoll, value); }

        [Category("2 Properties"), DisplayName("Properties 2"), Description("Second property list as 'Id=Value; Id=Value' (ids: Weight, Quality, Quantity, Height, Value, AcVc, Slot, Off_C8, Appearance, Race, Gender, Paperdoll or numbers).")]
        public string Properties2
        {
            get => string.Join("; ", E.Properties2.Select(p => $"{p.Id}={p.Value}"));
            set
            {
                E.Properties2.Clear();
                foreach (string part in (value ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kv = part.Split('=');
                    if (kv.Length != 2)
                    {
                        throw new FormatException("Use 'Id=Value; Id=Value'.");
                    }
                    TileArtPropertyId id = byte.TryParse(kv[0].Trim(), out byte b) ? (TileArtPropertyId)b : (TileArtPropertyId)Enum.Parse(typeof(TileArtPropertyId), kv[0].Trim(), true);
                    E.Properties2.Add(new TileArtProperty { Id = id, Value = (uint)ParseULong(kv[1]) });
                }
                Changed();
            }
        }

        [Category("2 Properties"), DisplayName("Stack aliases"), Description("Graphic per amount, e.g. gold: 'Amount:ItemId; Amount:ItemId'.")]
        public string StackAliases
        {
            get => string.Join("; ", E.StackAliases.Select(a => $"{a.Amount}:{a.ItemId}"));
            set
            {
                E.StackAliases.Clear();
                foreach (string part in (value ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kv = part.Split(':');
                    if (kv.Length != 2)
                    {
                        throw new FormatException("Use 'Amount:ItemId; Amount:ItemId'.");
                    }
                    E.StackAliases.Add(new TileArtStackAlias { Amount = (uint)ParseULong(kv[0]), ItemId = (uint)ParseULong(kv[1]) });
                }
                Changed();
            }
        }

        // ------------------------------------------------------------- image offsets

        private static string OffsetText(TileArtImageOffset o) => $"{o.XStart}, {o.YStart}, {o.XEnd}, {o.YEnd}, {o.XOffset}, {o.YOffset}";

        private static TileArtImageOffset ParseOffset(string text)
        {
            int[] v = (text ?? string.Empty).Split(',').Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture)).ToArray();
            if (v.Length != 6)
            {
                throw new FormatException("Use 6 numbers: xStart, yStart, xEnd, yEnd, xOffset, yOffset");
            }
            return new TileArtImageOffset { XStart = v[0], YStart = v[1], XEnd = v[2], YEnd = v[3], XOffset = v[4], YOffset = v[5] };
        }

        [Category("3 Image offsets"), DisplayName("EC image"), Description("xStart, yStart, xEnd, yEnd, xOffset, yOffset of the Enhanced Client image.")]
        public string EcImageOffset
        {
            get => OffsetText(E.EcImageOffset);
            set { E.EcImageOffset = ParseOffset(value); Changed(); }
        }

        [Category("3 Image offsets"), DisplayName("CC image"), Description("xStart, yStart, xEnd, yEnd, xOffset, yOffset of the Classic Client image.")]
        public string CcImageOffset
        {
            get => OffsetText(E.CcImageOffset);
            set { E.CcImageOffset = ParseOffset(value); Changed(); }
        }

        // ------------------------------------------------------------- unknown

        [Category("4 Unknown"), DisplayName("Version")] public ushort Version { get => E.Version; set { E.Version = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 1 (byte)")] public byte Unknown1 { get => E.Unknown1; set { E.Unknown1 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 2 (byte)")] public byte Unknown2 { get => E.Unknown2; set { E.Unknown2 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 3 (float)")] public float Unknown3 { get => E.Unknown3; set { E.Unknown3 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 4 (float)")] public float Unknown4 { get => E.Unknown4; set { E.Unknown4 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Fixed zero")] public int FixedZero { get => E.FixedZero; set { E.FixedZero = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 5 (float)")] public float Unknown5 { get => E.Unknown5; set { E.Unknown5 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 6 (byte)")] public byte Unknown6 { get => E.Unknown6; set { E.Unknown6 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 7")] public int Unknown7 { get => E.Unknown7; set { E.Unknown7 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 8")] public int Unknown8 { get => E.Unknown8; set { E.Unknown8 = value; Changed(); } }
        [Category("4 Unknown"), DisplayName("Unknown 9")] public uint Unknown9 { get => E.Unknown9; set { E.Unknown9 = value; Changed(); } }

        [Category("4 Unknown"), DisplayName("Sitting"), Description("4 values when the item is a seat; empty = no sitting data.")]
        public string Sitting
        {
            get => E.HasSitting ? string.Join(", ", E.Sitting) : string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    E.HasSitting = false;
                }
                else
                {
                    uint[] v = value.Split(',').Select(s => (uint)ParseULong(s)).ToArray();
                    if (v.Length != 4)
                    {
                        throw new FormatException("Use 4 numbers.");
                    }
                    E.HasSitting = true;
                    Array.Copy(v, E.Sitting, 4);
                }
                Changed();
            }
        }

        [Category("4 Unknown"), DisplayName("Appearance data"), Description("Raw animation appearance block (read only).")]
        public string AppearanceData => BitConverter.ToString(E.AppearanceData);

        [Category("4 Unknown"), DisplayName("Remaining data size"), Description("Bytes after the radar color (textures and more), kept unchanged.")]
        public int TailSize => E.Tail.Length;

        private static ulong ParseULong(string text)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ulong.Parse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return ulong.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }
    }
}

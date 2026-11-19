using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>The kinds of side-car extensions the tool can create (see <see cref="ExtendedFormat"/>).</summary>
    public enum UoExtensionKind
    {
        Items = 0,
        LandTiles = 1,
        Textures = 2,
        Hues = 3
    }

    /// <summary>
    /// One place for everything that concerns ALL extensions at once: sizes, create/resize, reload, close, save,
    /// summary for the check window. The single classes (Art, TileData, Textures, Hues, RadarCol) do the work.
    /// </summary>
    public static class UoExtensions
    {
        /// <summary>
        /// Raised after an extension was created, resized, loaded or reloaded - the tabs (Items, LandTiles, Textures,
        /// Hues, TileData, RadarColor) refresh their lists. Argument null = all kinds.
        /// </summary>
        public static event Action<UoExtensionKind?> Changed;

        public static void RaiseChanged(UoExtensionKind? kind)
        {
            Changed?.Invoke(kind);
        }

        public static string Title(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => "Items (art + tiledata)",
            UoExtensionKind.LandTiles => "Land tiles (art + tiledata)",
            UoExtensionKind.Textures => "Textures",
            UoExtensionKind.Hues => "Hues",
            _ => kind.ToString()
        };

        /// <summary>First id of the extension (hues: index, hue number = index + 1).</summary>
        public static int BaseId(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => ExtendedFormat.BaseId,
            UoExtensionKind.LandTiles => ExtendedFormat.LandBaseId,
            UoExtensionKind.Textures => ExtendedFormat.TextureBaseId,
            _ => ExtendedFormat.HueBaseId
        };

        public static int MaxSlots(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => ExtendedFormat.MaxSlots,
            UoExtensionKind.LandTiles => ExtendedFormat.LandMaxSlots,
            UoExtensionKind.Textures => ExtendedFormat.TextureMaxSlots,
            _ => ExtendedFormat.HueMaxSlots
        };

        /// <summary>Suggested size when nothing exists yet.</summary>
        public static int DefaultSlots(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => ExtendedFormat.DefaultSlots,
            UoExtensionKind.Hues => ClassicLimitSlots(kind),
            _ => ClassicLimitSlots(kind)
        };

        /// <summary>
        /// Slots that still fit the classic 16-bit / 14-bit fields (land ids in maps, texture ids in tiledata.mul,
        /// hue numbers in the classic client). Everything above is for the new client - the tool supports up to
        /// <see cref="MaxSlots"/> (16,777,216) like for the items.
        /// </summary>
        public static int ClassicLimitSlots(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => 0, // item ids above 0xFFFF never fit the classic protocol
            UoExtensionKind.LandTiles => ExtendedFormat.LandClassicLimit - ExtendedFormat.LandBaseId + 1,
            UoExtensionKind.Textures => ExtendedFormat.TextureClassicLimit - ExtendedFormat.TextureBaseId + 1,
            _ => ExtendedFormat.HueClassicLimit - ExtendedFormat.HueBaseId
        };

        /// <summary>Rough memory need per slot while loaded (index, caches, tiledata / hue objects, radar colour).</summary>
        public static int EstimatedBytesPerSlot(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => 16 + 8 + 2 + 48 + 8 + 2,
            UoExtensionKind.LandTiles => 16 + 8 + 4 + 2 + 32 + 2,
            UoExtensionKind.Textures => 16 + 8 + 4 + 2,
            _ => 8 + 24 + 88 + 40 + 40
        };

        public static int Increment(UoExtensionKind kind) => kind == UoExtensionKind.Items ? 0x10000 : kind == UoExtensionKind.Hues ? 1000 : 0x1000;

        public static int CurrentSlots(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => Art.ExtendedCount,
            UoExtensionKind.LandTiles => Art.LandExtension.Count,
            UoExtensionKind.Textures => Textures.Extension.Count,
            _ => Hues.ExtendedCount
        };

        public static bool Exists(UoExtensionKind kind) => CurrentSlots(kind) > 0;

        /// <summary>Number of used entries (graphic present / hue not empty).</summary>
        public static int CountUsed(UoExtensionKind kind)
        {
            switch (kind)
            {
                case UoExtensionKind.Items:
                    return Art.HasExtension ? Art.CountUsedExtended() : 0;
                case UoExtensionKind.LandTiles:
                    return Art.LandExtension.CountUsed();
                case UoExtensionKind.Textures:
                    return Textures.Extension.CountUsed();
                default:
                    int n = 0;
                    for (int i = Hues.LegacyCount; i < Hues.List.Length; ++i)
                    {
                        if (!IsEmptyHue(Hues.List[i]))
                        {
                            ++n;
                        }
                    }

                    return n;
            }
        }

        /// <summary>Used entries above the new size (they are lost when shrinking).</summary>
        public static int CountUsedAbove(UoExtensionKind kind, int slots)
        {
            int first = BaseId(kind) + slots;
            int last = BaseId(kind) + CurrentSlots(kind) - 1;
            int n = 0;
            for (int id = first; id <= last; ++id)
            {
                bool used = kind switch
                {
                    UoExtensionKind.Items => Art.IsValidStatic(id),
                    UoExtensionKind.LandTiles => Art.LandExtension.IsValid(id),
                    UoExtensionKind.Textures => Textures.Extension.IsValid(id),
                    _ => !IsEmptyHue(Hues.List[id])
                };
                if (used)
                {
                    ++n;
                }
            }

            return n;
        }

        public static bool IsEmptyHue(Hue hue)
        {
            if (hue == null)
            {
                return true;
            }

            foreach (ushort c in hue.Colors)
            {
                if (c != 0)
                {
                    return false;
                }
            }

            return string.IsNullOrEmpty(hue.Name) || hue.Name == "Null";
        }

        /// <summary>Creates or resizes one extension in memory incl. the parts that belong to it (tiledata, radar colours).</summary>
        public static void CreateOrResize(UoExtensionKind kind, int slots)
        {
            switch (kind)
            {
                case UoExtensionKind.Items:
                    Art.CreateOrResizeExtension(slots);
                    TileData.EnsureExtendedCapacity(slots);
                    RadarCol.SyncExtensionSizes();
                    Options.ChangedUltimaClass["Art"] = true;
                    Options.ChangedUltimaClass["TileData"] = true;
                    Options.ChangedUltimaClass["RadarColor"] = true;
                    break;
                case UoExtensionKind.LandTiles:
                    Art.CreateOrResizeLandExtension(slots);
                    TileData.EnsureLandExtendedCapacity(slots);
                    RadarCol.SyncExtensionSizes();
                    Options.ChangedUltimaClass["Art"] = true;
                    Options.ChangedUltimaClass["TileData"] = true;
                    Options.ChangedUltimaClass["RadarColor"] = true;
                    break;
                case UoExtensionKind.Textures:
                    Textures.CreateOrResizeExtension(slots);
                    Options.ChangedUltimaClass["Texture"] = true;
                    break;
                default:
                    Hues.CreateOrResizeExtension(slots);
                    Options.ChangedUltimaClass["Hues"] = true;
                    break;
            }
        }

        /// <summary>Writes the side-car files of one extension (not the classic files) into <paramref name="folder"/>.</summary>
        public static void Save(UoExtensionKind kind, string folder)
        {
            switch (kind)
            {
                case UoExtensionKind.Items:
                    Art.SaveExtension(folder);
                    TileData.SaveExtension(folder);
                    RadarCol.SaveExtension(folder);
                    break;
                case UoExtensionKind.LandTiles:
                    Art.SaveLandExtension(folder);
                    TileData.SaveLandExtension(folder);
                    RadarCol.SaveExtension(folder);
                    break;
                case UoExtensionKind.Textures:
                    Textures.SaveExtension(folder);
                    break;
                default:
                    Hues.SaveExtension(folder);
                    break;
            }
        }

        public static void SaveAll(string folder)
        {
            foreach (UoExtensionKind kind in Enum.GetValues(typeof(UoExtensionKind)))
            {
                if (Exists(kind))
                {
                    Save(kind, folder);
                }
            }
        }

        /// <summary>Releases all open extension data files (before they are replaced by copies).</summary>
        public static void CloseAll()
        {
            Art.CloseExtensionFiles();
            Art.LandExtension.Close();
            Textures.Extension.Close();
        }

        /// <summary>Loads every extension again from the current paths (unsaved extended changes are discarded).</summary>
        public static void ReloadAll()
        {
            Art.ReloadExtension();
            Art.ReloadLandExtension();
            TileData.ReloadExtension();
            TileData.ReloadLandExtension();
            Textures.ReloadExtension();
            Hues.ReloadExtension();
            RadarCol.LoadExtension();
        }

        /// <summary>The side-car files that belong to one kind.</summary>
        public static string[] FileNames(UoExtensionKind kind) => kind switch
        {
            UoExtensionKind.Items => new[] { ExtendedFormat.ArtIndexFileName, ExtendedFormat.ArtDataFileName, ExtendedFormat.TileDataFileName, ExtendedFormat.RadarItemFileName },
            UoExtensionKind.LandTiles => new[] { ExtendedFormat.LandIndexFileName, ExtendedFormat.LandDataFileName, ExtendedFormat.LandTileDataFileName, ExtendedFormat.RadarLandFileName },
            UoExtensionKind.Textures => new[] { ExtendedFormat.TextureIndexFileName, ExtendedFormat.TextureDataFileName },
            _ => new[] { ExtendedFormat.HueFileName }
        };

        /// <summary>Short summary of everything in memory (one line per kind).</summary>
        public static string MemorySummary()
        {
            var sb = new StringBuilder();
            foreach (UoExtensionKind kind in Enum.GetValues(typeof(UoExtensionKind)))
            {
                int slots = CurrentSlots(kind);
                int b = BaseId(kind);
                string range = slots > 0 ? RangeText(kind, b, b + slots - 1) : "-";
                sb.AppendLine($"{Title(kind),-28} slots {slots,9:N0}   used {(slots > 0 ? CountUsed(kind) : 0),9:N0}   {range}");
            }

            sb.AppendLine($"{"Radar colours",-28} land ext {RadarCol.LandExtendedCount:N0}, item ext {RadarCol.ItemExtendedCount:N0}");
            sb.AppendLine($"{"Land tiledata",-28} ext {TileData.LandExtendedCount:N0}");
            return sb.ToString();
        }

        public static string RangeText(UoExtensionKind kind, int first, int last)
        {
            return kind == UoExtensionKind.Hues
                ? $"hue numbers {first + 1} - {last + 1} (0x{first + 1:X} - 0x{last + 1:X})"
                : $"ids 0x{first:X} - 0x{last:X}";
        }

        /// <summary>One line per side-car file in <paramref name="folder"/> (exists, size, slots, header problems).</summary>
        public static string DiskSummary(string folder)
        {
            var sb = new StringBuilder();
            foreach (string name in ExtendedFormat.AllFileNames)
            {
                string path = Path.Combine(folder, name);
                var fi = new FileInfo(path);
                if (!fi.Exists)
                {
                    sb.AppendLine($"  {name,-20} missing");
                    continue;
                }

                string state = ExtendedFormat.TryReadHeader(path, out string magic, out int baseId, out int count, out int _, out string error)
                    ? $"{magic}, base 0x{baseId:X}, {count:N0} slots" + (error != null ? $"  WARNING: {error}" : string.Empty)
                    : $"UNREADABLE: {error}";
                sb.AppendLine($"  {name,-20} {fi.Length,14:N0} bytes  {fi.LastWriteTime:yyyy-MM-dd HH:mm}  {state}");
            }

            return sb.ToString();
        }
    }
}

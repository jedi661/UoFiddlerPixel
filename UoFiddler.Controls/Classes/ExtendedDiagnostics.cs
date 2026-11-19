using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Checks what is LOADED (memory) for all extensions and repairs what can be repaired automatically.
    /// The checks of the files on disk are in <see cref="ExtendedValidator"/> (Ultima, no GDI).
    /// </summary>
    public static class ExtendedDiagnostics
    {
        private const int MaxListed = 200;

        public static List<ExtIssue> CheckMemory()
        {
            var issues = new List<ExtIssue>();
            void Add(ExtIssueSeverity s, ExtIssueArea a, string msg, int id = -1, ExtIssueFix fix = ExtIssueFix.None, string file = "(memory)")
                => issues.Add(new ExtIssue { Severity = s, Area = a, Message = msg, Id = id, Fix = fix, File = file });

            bool any = false;

            // ---- sizes that belong together
            if (Art.HasExtension)
            {
                any = true;
                if (TileData.ExtendedCount != Art.ExtendedCount || RadarCol.ItemExtendedCount != Art.ExtendedCount)
                {
                    Add(ExtIssueSeverity.Warning, ExtIssueArea.Items,
                        $"Sizes differ: art {Art.ExtendedCount:N0}, tiledata {TileData.ExtendedCount:N0}, radar colours {RadarCol.ItemExtendedCount:N0} slots.",
                        fix: ExtIssueFix.SyncSizes);
                }
            }

            if (Art.HasLandExtension)
            {
                any = true;
                int n = Art.LandExtension.Count;
                if (TileData.LandExtendedCount != n || RadarCol.LandExtendedCount != n)
                {
                    Add(ExtIssueSeverity.Warning, ExtIssueArea.LandTiles,
                        $"Sizes differ: land art {n:N0}, land tiledata {TileData.LandExtendedCount:N0}, radar colours {RadarCol.LandExtendedCount:N0} slots.",
                        fix: ExtIssueFix.SyncSizes);
                }
            }

            any |= Textures.HasExtension || Hues.HasExtension;
            if (!any)
            {
                Add(ExtIssueSeverity.Info, ExtIssueArea.General, "No extension loaded or created.");
                return issues;
            }

            // ---- created in memory but never saved / unsaved changes
            Unsaved(issues, ExtIssueArea.Items, Art.HasExtension, Art.ExtendedDataPath, Art.HasExtension ? Art.GetUnsavedExtendedIds().Count : 0);
            Unsaved(issues, ExtIssueArea.LandTiles, Art.HasLandExtension, Art.LandExtension.DataPath, Art.LandExtension.Exists ? Art.LandExtension.GetUnsavedIds().Count : 0);
            Unsaved(issues, ExtIssueArea.Textures, Textures.HasExtension, Textures.Extension.DataPath, Textures.Extension.Exists ? Textures.Extension.GetUnsavedIds().Count : 0);
            if (Hues.HasExtension && Hues.ExtensionPath == null)
            {
                Add(ExtIssueSeverity.Warning, ExtIssueArea.Hues, $"{Hues.ExtendedCount:N0} extended hues exist only in memory - not saved yet.", fix: ExtIssueFix.Save);
            }

            // ---- loaded from another folder than the Ultima Online folder
            Folder(issues, ExtIssueArea.Items, Art.ExtendedDataPath);
            Folder(issues, ExtIssueArea.LandTiles, Art.LandExtension.DataPath);
            Folder(issues, ExtIssueArea.Textures, Textures.Extension.DataPath);
            Folder(issues, ExtIssueArea.Hues, Hues.ExtensionPath);

            // ---- items: graphic without name / name without graphic / black radar colour
            if (Art.HasExtension && TileData.ItemTable != null)
            {
                int noName = 0, noGraphic = 0, black = 0;
                int last = Math.Min(Art.GetMaxExtendedItemId(), TileData.ItemTable.Length - 1);
                for (int id = ExtendedFormat.BaseId; id <= last; ++id)
                {
                    bool graphic = Art.IsValidStatic(id);
                    bool named = !string.IsNullOrEmpty(TileData.ItemTable[id].Name);
                    if (graphic && !named && ++noName <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Info, ExtIssueArea.Items, "graphic without tiledata name", id);
                    }

                    if (!graphic && named && ++noGraphic <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Info, ExtIssueArea.Items, $"tiledata '{TileData.ItemTable[id].Name}' but no graphic", id);
                    }

                    if (graphic && RadarCol.GetItemColor(id) == 0 && ++black <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Info, ExtIssueArea.RadarColors, "item has a graphic but no radar colour (black on the radar map)", id, ExtIssueFix.RadarFromGraphic);
                    }
                }

                Summary(issues, ExtIssueArea.Items, noName, "graphics without name");
                Summary(issues, ExtIssueArea.Items, noGraphic, "names without graphic");
                Summary(issues, ExtIssueArea.RadarColors, black, "items without radar colour");
            }

            // ---- land tiles: missing textures, black radar colour
            if (TileData.LandTable != null)
            {
                int badTex = 0, black = 0, tooBig = 0;
                for (int id = 0; id < TileData.LandTable.Length; ++id)
                {
                    int tex = TileData.LandTable[id].TextureId;
                    if (id < TileData.LegacyLandCount && tex > ushort.MaxValue && ++tooBig <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Error, ExtIssueArea.LandTiles,
                            $"classic land tile points to texture 0x{tex:X} - tiledata.mul only stores 16 bit (cut to 0xFFFF on save)", id);
                    }

                    if (tex != 0 && (id >= TileData.LegacyLandCount || Textures.HasExtension) && !Textures.TestTexture(tex) && ++badTex <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Warning, ExtIssueArea.LandTiles, $"texture id 0x{tex:X} does not exist", id);
                    }

                    if (id >= TileData.LegacyLandCount && Art.IsValidLand(id) && RadarCol.GetLandColor(id) == 0 && ++black <= MaxListed)
                    {
                        Add(ExtIssueSeverity.Info, ExtIssueArea.RadarColors, "land tile has a graphic but no radar colour", id, ExtIssueFix.RadarFromGraphic);
                    }
                }

                Summary(issues, ExtIssueArea.LandTiles, badTex, "missing textures");
                Summary(issues, ExtIssueArea.RadarColors, black, "land tiles without radar colour");
            }

            if (Hues.HasExtension)
            {
                int empty = 0;
                for (int i = Hues.LegacyCount; i < Hues.List.Length; ++i)
                {
                    if (UoExtensions.IsEmptyHue(Hues.List[i]))
                    {
                        ++empty;
                    }
                }

                Add(ExtIssueSeverity.Info, ExtIssueArea.Hues, $"{Hues.ExtendedCount:N0} extended hues, {empty:N0} of them still empty.");
            }

            return issues;
        }

        private static void Unsaved(List<ExtIssue> issues, ExtIssueArea area, bool exists, string dataPath, int unsaved)
        {
            if (!exists)
            {
                return;
            }

            if (dataPath == null)
            {
                issues.Add(new ExtIssue { Severity = ExtIssueSeverity.Warning, Area = area, File = "(memory)", Message = "exists only in memory - not saved yet.", Fix = ExtIssueFix.Save });
            }
            else if (unsaved > 0)
            {
                issues.Add(new ExtIssue { Severity = ExtIssueSeverity.Warning, Area = area, File = "(memory)", Message = $"{unsaved:N0} changed entr(y/ies) not saved yet.", Fix = ExtIssueFix.Save });
            }
        }

        private static void Folder(List<ExtIssue> issues, ExtIssueArea area, string dataPath)
        {
            if (dataPath == null || string.IsNullOrEmpty(Files.RootDir))
            {
                return;
            }

            string folder = Path.GetDirectoryName(Path.GetFullPath(dataPath))?.TrimEnd('\\', '/');
            string root = Path.GetFullPath(Files.RootDir).TrimEnd('\\', '/');
            if (!string.Equals(folder, root, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new ExtIssue
                {
                    Severity = ExtIssueSeverity.Info,
                    Area = area,
                    File = Path.GetFileName(dataPath),
                    Message = $"loaded from {folder} (not from the Ultima Online folder) - copy the files there for the client."
                });
            }
        }

        private static void Summary(List<ExtIssue> issues, ExtIssueArea area, int count, string what)
        {
            if (count > MaxListed)
            {
                issues.Add(new ExtIssue { Severity = ExtIssueSeverity.Info, Area = area, File = "(memory)", Message = $"... {count - MaxListed:N0} more {what}." });
            }
        }

        public static bool CanFix(ExtIssue issue) => issue != null && issue.Fix != ExtIssueFix.None;

        /// <summary>Repairs one issue in memory (and saves where the fix is "save"). Returns a short result text.</summary>
        public static string ApplyFix(ExtIssue issue, string outputFolder)
        {
            switch (issue.Fix)
            {
                case ExtIssueFix.SyncSizes:
                    if (issue.Area == ExtIssueArea.LandTiles && Art.HasLandExtension)
                    {
                        UoExtensions.CreateOrResize(UoExtensionKind.LandTiles, Art.LandExtension.Count);
                        return "land tile sizes synced (save to write the files)";
                    }

                    if (Art.HasExtension)
                    {
                        UoExtensions.CreateOrResize(UoExtensionKind.Items, Art.ExtendedCount);
                        return "item sizes synced (save to write the files)";
                    }

                    return "nothing loaded to sync - load the folder first";

                case ExtIssueFix.RemoveEntry:
                    if (issue.Id < 0)
                    {
                        return "no id";
                    }

                    switch (issue.Area)
                    {
                        case ExtIssueArea.Items when Art.IsLegalStaticId(issue.Id):
                            Art.RemoveStatic(issue.Id);
                            Options.ChangedUltimaClass["Art"] = true;
                            return $"0x{issue.Id:X} removed in memory (save to write the files)";
                        case ExtIssueArea.LandTiles when Art.LandExtension.Contains(issue.Id):
                            Art.RemoveLand(issue.Id);
                            Options.ChangedUltimaClass["Art"] = true;
                            return $"land 0x{issue.Id:X} removed in memory (save to write the files)";
                        case ExtIssueArea.Textures when Textures.Extension.Contains(issue.Id):
                            Textures.Remove(issue.Id);
                            Options.ChangedUltimaClass["Texture"] = true;
                            return $"texture 0x{issue.Id:X} removed in memory (save to write the files)";
                        default:
                            return "the id is not in the loaded extension (load this folder first)";
                    }

                case ExtIssueFix.Save:
                    UoExtensionKind kind = issue.Area switch
                    {
                        ExtIssueArea.LandTiles => UoExtensionKind.LandTiles,
                        ExtIssueArea.Textures => UoExtensionKind.Textures,
                        ExtIssueArea.Hues => UoExtensionKind.Hues,
                        _ => UoExtensionKind.Items
                    };
                    UoExtensions.Save(kind, outputFolder);
                    return $"{UoExtensions.Title(kind)} saved to {outputFolder}";

                case ExtIssueFix.RadarFromGraphic:
                    // land and item ids overlap (land ext 0x4000+, items 0x10000+): the message tells which one
                    bool land = issue.Message.StartsWith("land", StringComparison.Ordinal);
                    Bitmap bmp = land ? Art.GetLand(issue.Id) : Art.GetStatic(issue.Id);
                    if (!TryAverage(bmp, out ushort color))
                    {
                        return "no graphic";
                    }

                    if (land)
                    {
                        RadarCol.SetLandColor(issue.Id, color);
                    }
                    else
                    {
                        RadarCol.SetItemColor(issue.Id, color);
                    }

                    Options.ChangedUltimaClass["RadarColor"] = true;
                    return $"radar colour 0x{color:X4} set (save the RadarColor tab / extensions)";

                default:
                    return "no automatic fix";
            }
        }

        /// <summary>Average colour of the visible pixels as 15-bit radar colour.</summary>
        public static unsafe bool TryAverage(Bitmap image, out ushort color)
        {
            color = 0;
            if (image == null)
            {
                return false;
            }

            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            long r = 0, g = 0, b = 0, count = 0;
            try
            {
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < image.Height; ++y, line += delta)
                {
                    for (int x = 0; x < image.Width; ++x)
                    {
                        ushort p = line[x];
                        if ((p & 0x7FFF) == 0)
                        {
                            continue;
                        }

                        r += HueHelpers.HueToColorR(p);
                        g += HueHelpers.HueToColorG(p);
                        b += HueHelpers.HueToColorB(p);
                        ++count;
                    }
                }
            }
            finally
            {
                image.UnlockBits(bd);
            }

            if (count == 0)
            {
                return false;
            }

            color = HueHelpers.ColorToHue(Color.FromArgb((int)(r / count), (int)(g / count), (int)(b / count)));
            return true;
        }

        /// <summary>Counts per severity, e.g. "2 errors, 5 warnings, 10 infos".</summary>
        public static string CountText(IEnumerable<ExtIssue> issues)
        {
            List<ExtIssue> list = issues.ToList();
            return $"{list.Count(i => i.Severity == ExtIssueSeverity.Error):N0} error(s), " +
                   $"{list.Count(i => i.Severity == ExtIssueSeverity.Warning):N0} warning(s), " +
                   $"{list.Count(i => i.Severity == ExtIssueSeverity.Info):N0} info(s)";
        }
    }
}

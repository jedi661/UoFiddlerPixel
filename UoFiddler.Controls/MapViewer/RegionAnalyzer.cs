/***************************************************************************
 *
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using Ultima;

namespace UoFiddler.Controls.MapViewer
{
    /// <summary>
    /// Options of the region check
    /// </summary>
    public sealed class RegionAnalyzerOptions
    {
        public bool CheckInvalidIds { get; set; } = true;
        public bool CheckDuplicates { get; set; } = true;
        public bool CheckBelowLand { get; set; } = true;

        /// <summary>Statics high above land/other statics (many false hits on roofs and upper floors, off by default)</summary>
        public bool CheckFloating { get; set; }

        /// <summary>Minimum gap in z between a static and the thing below it</summary>
        public int FloatingThreshold { get; set; } = 10;

        public bool CheckSteepLand { get; set; } = true;

        /// <summary>Maximum z difference between neighbour land tiles</summary>
        public int SteepThreshold { get; set; } = 30;
    }

    /// <summary>
    /// Searches a <see cref="RegionData"/> for typical map errors.
    /// </summary>
    public static class RegionAnalyzer
    {
        public static List<RegionIssue> Analyze(RegionData data, RegionAnalyzerOptions options)
        {
            var issues = new List<RegionIssue>();

            foreach (RegionCell cell in data.Cells)
            {
                if (options.CheckInvalidIds && !Art.IsValidLand(cell.LandId))
                {
                    issues.Add(new RegionIssue
                    {
                        Type = RegionIssueType.InvalidLandId,
                        Severity = RegionIssueSeverity.Error,
                        X = cell.X,
                        Y = cell.Y,
                        Z = cell.LandZ,
                        TileId = cell.LandId,
                        Description = $"Land id 0x{cell.LandId:X4} has no art",
                        CanAutoFix = true,
                        FixByDefault = true
                    });
                }

                if (options.CheckSteepLand)
                {
                    int diff = Math.Max(Math.Abs(cell.LandZ - cell.ZRight), Math.Abs(cell.LandZ - cell.ZLeft));
                    if (diff > options.SteepThreshold)
                    {
                        issues.Add(new RegionIssue
                        {
                            Type = RegionIssueType.SteepLand,
                            Severity = RegionIssueSeverity.Warning,
                            X = cell.X,
                            Y = cell.Y,
                            Z = cell.LandZ,
                            TileId = cell.LandId,
                            Description = $"Height jump of {diff} to the neighbour (z {cell.LandZ}, right {cell.ZRight}, below {cell.ZLeft})",
                            CanAutoFix = true,
                            FixByDefault = false
                        });
                    }
                }

                CheckStatics(cell, options, issues);
            }

            return issues;
        }

        private static void CheckStatics(RegionCell cell, RegionAnalyzerOptions options, List<RegionIssue> issues)
        {
            var seen = new HashSet<long>();
            int landTop = Math.Max(Math.Max(cell.LandZ, cell.ZRight), Math.Max(cell.ZBottom, cell.ZLeft));
            int landLow = cell.MinCornerZ;

            foreach (RegionStatic s in cell.Statics)
            {
                if (options.CheckInvalidIds && !Art.IsValidStatic(s.Id))
                {
                    issues.Add(new RegionIssue
                    {
                        Type = RegionIssueType.InvalidStaticId,
                        Severity = RegionIssueSeverity.Error,
                        X = cell.X,
                        Y = cell.Y,
                        Z = s.Z,
                        TileId = s.Id,
                        Hue = s.Hue,
                        Description = $"Static 0x{s.Id:X4} has no art",
                        CanAutoFix = true,
                        FixByDefault = true
                    });
                    continue;
                }

                if (options.CheckDuplicates)
                {
                    long key = ((long)s.Id << 32) | ((long)(byte)s.Z << 16) | (ushort)s.Hue;
                    if (!seen.Add(key))
                    {
                        issues.Add(new RegionIssue
                        {
                            Type = RegionIssueType.DuplicateStatic,
                            Severity = RegionIssueSeverity.Error,
                            X = cell.X,
                            Y = cell.Y,
                            Z = s.Z,
                            TileId = s.Id,
                            Hue = s.Hue,
                            Description = $"Static 0x{s.Id:X4} ({s.Name}) exists more than once at the same z and hue",
                            CanAutoFix = true,
                            FixByDefault = true
                        });
                        continue;
                    }
                }

                // same rule as "Report Statics below Map": completely below the lowest land corner
                if (options.CheckBelowLand && s.Z < landLow && s.Z + s.Height < landLow)
                {
                    issues.Add(new RegionIssue
                    {
                        Type = RegionIssueType.StaticBelowLand,
                        Severity = RegionIssueSeverity.Error,
                        X = cell.X,
                        Y = cell.Y,
                        Z = s.Z,
                        TileId = s.Id,
                        Hue = s.Hue,
                        Description = $"Static 0x{s.Id:X4} ({s.Name}) z {s.Z}+{s.Height} is below the land (z {landLow}) and invisible",
                        CanAutoFix = true,
                        FixByDefault = true
                    });
                    continue;
                }

                if (options.CheckFloating)
                {
                    TileFlag flags = s.Flags;
                    if ((flags & (TileFlag.Roof | TileFlag.Foliage | TileFlag.LightSource | TileFlag.Window)) != 0)
                    {
                        continue;
                    }

                    int support = GetSupport(cell, s, landTop);
                    if (s.Z - support > options.FloatingThreshold)
                    {
                        issues.Add(new RegionIssue
                        {
                            Type = RegionIssueType.FloatingStatic,
                            Severity = RegionIssueSeverity.Info,
                            X = cell.X,
                            Y = cell.Y,
                            Z = s.Z,
                            TileId = s.Id,
                            Hue = s.Hue,
                            Description = $"Static 0x{s.Id:X4} ({s.Name}) floats {s.Z - support} above the ground (z {support})",
                            CanAutoFix = true,
                            FixByDefault = false
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Highest top (land or static) below the given static
        /// </summary>
        public static int GetSupport(RegionCell cell, RegionStatic target, int landTop)
        {
            int support = landTop > target.Z ? cell.MinCornerZ : landTop;
            foreach (RegionStatic other in cell.Statics)
            {
                if (other.Id == target.Id && other.Z == target.Z)
                {
                    continue;
                }

                int top = other.Z + other.Height;
                if (other.Z <= target.Z && top <= target.Z && top > support)
                {
                    support = top;
                }
            }

            return support;
        }
    }
}

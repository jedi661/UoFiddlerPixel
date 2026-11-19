using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>One patched slot of the patch test.</summary>
    public sealed class PatchTestEntry
    {
        public UoExtensionKind Kind;
        public int SourceId;
        public int TargetId;
        public bool Patched;
        public bool MemoryOk;
        public bool DiskOk;
        public bool ReloadOk;
        public string Result = string.Empty;

        public bool Ok => Patched && MemoryOk && DiskOk && ReloadOk;

        // state before the patch (for "remove the test entries again")
        internal ItemData OldItem;
        internal LandData OldLand;
        internal ushort OldRadar;
        internal ushort[] OldHueColors;
        internal ushort OldHueStart, OldHueEnd;
        internal string OldHueName;
        internal byte[] Expected; // encoded data / record that must be on disk
    }

    public sealed class PatchTestResult
    {
        public readonly List<PatchTestEntry> Entries = new List<PatchTestEntry>();
        public string Folder;
        public string LogPath;
        public string Log;
        public bool Undone;

        public bool AllOk => Entries.Count > 0 && Entries.All(e => e.Ok);
    }

    /// <summary>
    /// Patch test for every extension: copies a real classic entry (item graphic + tiledata + radar colour, land tile +
    /// land tiledata + radar colour, texture, hue) into a free slot of the extension (first free slot and - optional -
    /// the last slot), checks it in memory, writes a test copy of the side-car files into a separate folder, reads the
    /// files back from disk and compares byte by byte. Everything is logged (to send it in case of errors).
    /// The loaded files are not changed; the test entries can be removed again (<see cref="Undo"/>).
    /// </summary>
    public static class ExtendedPatchTest
    {
        public const string NamePrefix = "PatchTest";

        private static StringBuilder _log;

        private static void Log(string text)
        {
            _log.AppendLine($"{DateTime.Now:HH:mm:ss.fff}  {text}");
        }

        public static PatchTestResult Run(IReadOnlyCollection<UoExtensionKind> kinds, bool lastSlotToo, string outputFolder, Action<string> progress)
        {
            _log = new StringBuilder();
            var result = new PatchTestResult
            {
                Folder = Path.Combine(outputFolder, "PatchTest", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"))
            };

            Log("UOFiddler extension patch test");
            Log($"Ultima Online folder : {Files.RootDir}");
            Log($"Test folder          : {result.Folder}");
            Log($"Kinds                : {string.Join(", ", kinds)}   last slot too: {lastSlotToo}");
            Log("Loaded now:");
            foreach (string line in UoExtensions.MemorySummary().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                Log("  " + line);
            }

            try
            {
                Directory.CreateDirectory(result.Folder);

                // 1. patch in memory
                foreach (UoExtensionKind kind in kinds)
                {
                    progress?.Invoke($"Patch {UoExtensions.Title(kind)} ...");
                    Guard($"patch {kind}", () => Patch(kind, lastSlotToo, result));
                }

                // 2. write a test copy (the loaded files stay as they are)
                progress?.Invoke("Write test copy ...");
                foreach (UoExtensionKind kind in kinds.Where(k => result.Entries.Any(e => e.Kind == k && e.Patched)))
                {
                    Guard($"save {kind}", () => SaveCopy(kind, result.Folder));
                }

                // 3. read back from disk and compare
                progress?.Invoke("Read back and compare ...");
                foreach (PatchTestEntry e in result.Entries.Where(x => x.Patched))
                {
                    Guard($"verify {e.Kind} 0x{e.TargetId:X}", () => VerifyDisk(e, result.Folder));
                }

                // 4. validator over the whole test folder
                progress?.Invoke("Check the test folder ...");
                List<ExtIssue> issues = ExtendedValidator.ValidateFolder(result.Folder);
                Log($"Validator on the test folder: {ExtendedDiagnostics.CountText(issues)}");
                foreach (ExtIssue i in issues.Where(i => i.Severity != ExtIssueSeverity.Info))
                {
                    Log("  " + i);
                }
            }
            catch (Exception ex)
            {
                Log("FATAL " + ex);
            }

            Log(string.Empty);
            Log("RESULT");
            foreach (PatchTestEntry e in result.Entries)
            {
                Log($"  {(e.Ok ? "OK  " : "FAIL")} {e.Kind,-10} source 0x{e.SourceId:X} -> 0x{e.TargetId:X}  memory {Mark(e.MemoryOk)} disk {Mark(e.DiskOk)} reload {Mark(e.ReloadOk)}  {e.Result}");
            }

            Log(result.Entries.Count == 0 ? "Nothing tested (create the extensions first)." : result.AllOk ? "ALL OK" : "ERRORS - please send this log.");

            result.Log = _log.ToString();
            try
            {
                result.LogPath = Path.Combine(result.Folder, "patchtest.log");
                Directory.CreateDirectory(result.Folder);
                File.WriteAllText(result.LogPath, result.Log);
            }
            catch (Exception ex)
            {
                result.Log += Environment.NewLine + "Log could not be written: " + ex.Message;
            }

            return result;
        }

        private static string Mark(bool ok) => ok ? "ok" : "--";

        private static void Guard(string step, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Log($"ERROR in {step}: {ex}");
            }
        }

        #region Patch

        private static void Patch(UoExtensionKind kind, bool lastSlotToo, PatchTestResult result)
        {
            if (!UoExtensions.Exists(kind))
            {
                Log($"{kind}: SKIPPED - no extension (create it first).");
                return;
            }

            int baseId = UoExtensions.BaseId(kind);
            int last = baseId + UoExtensions.CurrentSlots(kind) - 1;
            int source = FindSource(kind);
            if (source < 0)
            {
                Log($"{kind}: SKIPPED - no classic entry with data found to copy.");
                return;
            }

            var targets = new List<int>();
            int free = FirstFree(kind, baseId, last);
            if (free >= 0)
            {
                targets.Add(free);
            }
            else
            {
                Log($"{kind}: no free slot found.");
            }

            if (lastSlotToo && last != free)
            {
                if (IsUsed(kind, last))
                {
                    Log($"{kind}: last slot 0x{last:X} is used - not overwritten, skipped.");
                }
                else
                {
                    targets.Add(last);
                }
            }

            foreach (int target in targets)
            {
                var e = new PatchTestEntry { Kind = kind, SourceId = source, TargetId = target };
                result.Entries.Add(e);
                Log($"{kind}: patch 0x{source:X} -> 0x{target:X}");
                try
                {
                    PatchOne(e);
                    e.Patched = true;
                    e.MemoryOk = VerifyMemory(e);
                    Log($"  memory check: {(e.MemoryOk ? "ok" : "FAILED")}");
                }
                catch (Exception ex)
                {
                    e.Result = ex.Message;
                    Log($"  ERROR {ex}");
                }
            }
        }

        /// <summary>A classic entry with real data (the same ones every time, so results are comparable).</summary>
        private static int FindSource(UoExtensionKind kind)
        {
            switch (kind)
            {
                case UoExtensionKind.Items:
                    foreach (int id in new[] { 0x0E75, 0x0EED, 0x1F14 }.Concat(Enumerable.Range(1, 0x3FFF)))
                    {
                        if (Art.IsValidStatic(id))
                        {
                            return id;
                        }
                    }

                    return -1;
                case UoExtensionKind.LandTiles:
                    foreach (int id in new[] { 0x0003 }.Concat(Enumerable.Range(1, 0x3FFE)))
                    {
                        if (Art.IsValidLand(id))
                        {
                            return id;
                        }
                    }

                    return -1;
                case UoExtensionKind.Textures:
                    for (int id = 1; id < Textures.LegacyCount; ++id)
                    {
                        if (Textures.TestTexture(id))
                        {
                            return id;
                        }
                    }

                    return -1;
                default:
                    for (int i = 1; i < Hues.LegacyCount; ++i)
                    {
                        if (!UoExtensions.IsEmptyHue(Hues.List[i]))
                        {
                            return i;
                        }
                    }

                    return -1;
            }
        }

        private static bool IsUsed(UoExtensionKind kind, int id) => kind switch
        {
            UoExtensionKind.Items => Art.IsValidStatic(id),
            UoExtensionKind.LandTiles => Art.IsValidLand(id),
            UoExtensionKind.Textures => Textures.TestTexture(id),
            _ => !UoExtensions.IsEmptyHue(Hues.List[id])
        };

        private static int FirstFree(UoExtensionKind kind, int first, int last)
        {
            for (int id = first; id <= last; ++id)
            {
                if (!IsUsed(kind, id))
                {
                    return id;
                }
            }

            return -1;
        }

        private static Bitmap Copy1555(Bitmap src)
        {
            return src.Clone(new Rectangle(0, 0, src.Width, src.Height), PixelFormat.Format16bppArgb1555);
        }

        private static string TestName(int id) => $"{NamePrefix} {id:X}";

        private static void PatchOne(PatchTestEntry e)
        {
            int s = e.SourceId, t = e.TargetId;
            switch (e.Kind)
            {
                case UoExtensionKind.Items:
                {
                    if (TileData.ItemTable == null || t >= TileData.ItemTable.Length)
                    {
                        throw new InvalidOperationException($"tiledata has no entry 0x{t:X} (sizes differ - run the error search / sync sizes)");
                    }

                    Bitmap copy = Copy1555(Art.GetStatic(s));
                    e.Expected = Art.EncodeStatic(copy);
                    e.OldItem = TileData.ItemTable[t];
                    e.OldRadar = RadarCol.GetItemColor(t);
                    Art.ReplaceStatic(t, copy);
                    ItemData d = TileData.ItemTable[s];
                    d.Name = TestName(t);
                    TileData.ItemTable[t] = d;
                    ushort color = RadarCol.GetItemColor(s);
                    RadarCol.SetItemColor(t, color != 0 ? color : (ushort)0x7C00);
                    break;
                }

                case UoExtensionKind.LandTiles:
                {
                    if (TileData.LandTable == null || t >= TileData.LandTable.Length)
                    {
                        throw new InvalidOperationException($"land tiledata has no entry 0x{t:X} (sizes differ - run the error search / sync sizes)");
                    }

                    Bitmap copy = Copy1555(Art.GetLand(s));
                    e.Expected = Art.EncodeLand(copy);
                    e.OldLand = TileData.LandTable[t];
                    e.OldRadar = RadarCol.GetLandColor(t);
                    Art.ReplaceLand(t, copy);
                    LandData d = TileData.LandTable[s];
                    d.Name = TestName(t);
                    TileData.LandTable[t] = d;
                    ushort color = RadarCol.GetLandColor(s);
                    RadarCol.SetLandColor(t, color != 0 ? color : (ushort)0x03E0);
                    break;
                }

                case UoExtensionKind.Textures:
                {
                    Bitmap copy = Copy1555(Textures.GetTexture(s));
                    e.Expected = Textures.EncodeTexture(copy, out _);
                    Textures.Replace(t, copy);
                    break;
                }

                default:
                {
                    Hue src = Hues.List[s];
                    Hue dst = Hues.List[t];
                    e.OldHueColors = (ushort[])dst.Colors.Clone();
                    e.OldHueStart = dst.TableStart;
                    e.OldHueEnd = dst.TableEnd;
                    e.OldHueName = dst.Name;
                    Array.Copy(src.Colors, dst.Colors, 32);
                    dst.TableStart = src.TableStart;
                    dst.TableEnd = src.TableEnd;
                    dst.Name = TestName(t + 1);
                    e.Expected = HueRecord(dst);
                    break;
                }
            }
        }

        private static byte[] HueRecord(Hue h)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            foreach (ushort c in h.Colors)
            {
                bw.Write(c);
            }

            bw.Write(h.TableStart);
            bw.Write(h.TableEnd);
            ExtendedFormat.WriteName(bw, h.Name);
            bw.Flush();
            return ms.ToArray();
        }

        #endregion

        #region Verify

        private static bool VerifyMemory(PatchTestEntry e)
        {
            int t = e.TargetId;
            switch (e.Kind)
            {
                case UoExtensionKind.Items:
                    return Art.IsValidStatic(t) && Same(Art.EncodeStatic(Copy1555(Art.GetStatic(t))), e.Expected) &&
                           TileData.ItemTable[t].Name == TestName(t) && RadarCol.GetItemColor(t) != 0;
                case UoExtensionKind.LandTiles:
                    return Art.IsValidLand(t) && Same(Art.GetRawLand(t), e.Expected) &&
                           TileData.LandTable[t].Name == TestName(t) && RadarCol.GetLandColor(t) != 0;
                case UoExtensionKind.Textures:
                    return Textures.TestTexture(t) && Same(Textures.EncodeTexture(Copy1555(Textures.GetTexture(t)), out _), e.Expected);
                default:
                    return Hues.GetHue(t).Name == TestName(t + 1);
            }
        }

        private static void SaveCopy(UoExtensionKind kind, string folder)
        {
            switch (kind)
            {
                case UoExtensionKind.Items:
                    Art.SaveExtension(folder, true);
                    TileData.SaveExtension(folder);
                    RadarCol.SaveExtension(folder);
                    break;
                case UoExtensionKind.LandTiles:
                    Art.LandExtension.SaveCopy(folder);
                    TileData.SaveLandExtension(folder);
                    RadarCol.SaveExtension(folder);
                    break;
                case UoExtensionKind.Textures:
                    Textures.Extension.SaveCopy(folder);
                    break;
                default:
                    Hues.SaveExtension(folder, false);
                    break;
            }

            Log($"{kind}: test copy written ({string.Join(", ", UoExtensions.FileNames(kind).Where(n => File.Exists(Path.Combine(folder, n))))})");
        }

        /// <summary>Reads the written files with fresh readers (nothing from memory) and compares.</summary>
        private static void VerifyDisk(PatchTestEntry e, string folder)
        {
            int t = e.TargetId;
            var problems = new List<string>();
            switch (e.Kind)
            {
                case UoExtensionKind.Items:
                {
                    ExtendedFormat.ArtIndexEntry[] idx = ExtendedFormat.ReadArtIndex(Path.Combine(folder, ExtendedFormat.ArtIndexFileName));
                    byte[] raw = ReadEntry(idx, t - ExtendedFormat.BaseId, Path.Combine(folder, ExtendedFormat.ArtDataFileName));
                    e.DiskOk = Same(raw, e.Expected);
                    if (!e.DiskOk)
                    {
                        problems.Add(raw == null ? "graphic missing in artext.mul" : "graphic bytes differ");
                    }

                    ExtendedFormat.TileRecord[] tiles = ExtendedFormat.ReadTileData(Path.Combine(folder, ExtendedFormat.TileDataFileName));
                    int k = t - ExtendedFormat.BaseId;
                    bool tileOk = k < tiles.Length && tiles[k].Name == TestName(t);
                    ushort radar = RadarAt(folder, ExtendedFormat.RadarItemFileName, ExtendedFormat.RadarItemMagic, ExtendedFormat.BaseId, t);
                    e.ReloadOk = tileOk && radar != 0;
                    if (!tileOk)
                    {
                        problems.Add("tiledataext.mul: name not found");
                    }

                    if (radar == 0)
                    {
                        problems.Add("radarcolext.mul: colour missing");
                    }

                    break;
                }

                case UoExtensionKind.LandTiles:
                {
                    var store = new ExtendedBlobStore(ExtendedFormat.LandIndexFileName, ExtendedFormat.LandDataFileName, ExtendedFormat.LandIndexMagic,
                        ExtendedFormat.LandBaseId, ExtendedFormat.LandMaxSlots);
                    store.LoadFrom(Path.Combine(folder, ExtendedFormat.LandIndexFileName), Path.Combine(folder, ExtendedFormat.LandDataFileName));
                    byte[] raw = store.Read(t, out _);
                    store.Close();
                    e.DiskOk = Same(raw, e.Expected);
                    if (!e.DiskOk)
                    {
                        problems.Add(raw == null ? "land tile missing in artlandext.mul" : "land tile bytes differ");
                    }

                    byte[] rec = ExtendedFormat.ReadRecords(Path.Combine(folder, ExtendedFormat.LandTileDataFileName), ExtendedFormat.LandTileMagic,
                        ExtendedFormat.LandBaseId, ExtendedFormat.LandTileRecordSize, out int count);
                    int k = t - ExtendedFormat.LandBaseId;
                    bool tileOk = k < count && ExtendedFormat.ReadName(rec, (k * ExtendedFormat.LandTileRecordSize) + 12) == TestName(t);
                    ushort radar = RadarAt(folder, ExtendedFormat.RadarLandFileName, ExtendedFormat.RadarLandMagic, ExtendedFormat.LandBaseId, t);
                    e.ReloadOk = tileOk && radar != 0;
                    if (!tileOk)
                    {
                        problems.Add("tiledatalandext.mul: name not found");
                    }

                    if (radar == 0)
                    {
                        problems.Add("radarcollandext.mul: colour missing");
                    }

                    break;
                }

                case UoExtensionKind.Textures:
                {
                    var store = new ExtendedBlobStore(ExtendedFormat.TextureIndexFileName, ExtendedFormat.TextureDataFileName, ExtendedFormat.TextureIndexMagic,
                        ExtendedFormat.TextureBaseId, ExtendedFormat.TextureMaxSlots);
                    store.LoadFrom(Path.Combine(folder, ExtendedFormat.TextureIndexFileName), Path.Combine(folder, ExtendedFormat.TextureDataFileName));
                    byte[] raw = store.Read(t, out int extra);
                    store.Close();
                    e.DiskOk = Same(raw, e.Expected);
                    e.ReloadOk = raw != null && (raw.Length == 0x8000) == (extra == 1);
                    if (!e.DiskOk)
                    {
                        problems.Add(raw == null ? "texture missing in texmapsext.mul" : "texture bytes differ");
                    }

                    if (!e.ReloadOk)
                    {
                        problems.Add("size flag (extra) does not fit");
                    }

                    break;
                }

                default:
                {
                    byte[] rec = ExtendedFormat.ReadRecords(Path.Combine(folder, ExtendedFormat.HueFileName), ExtendedFormat.HueMagic,
                        ExtendedFormat.HueBaseId, ExtendedFormat.HueRecordSize, out int count);
                    int k = t - ExtendedFormat.HueBaseId;
                    byte[] got = k < count ? rec.Skip(k * ExtendedFormat.HueRecordSize).Take(ExtendedFormat.HueRecordSize).ToArray() : null;
                    e.DiskOk = Same(got, e.Expected);
                    e.ReloadOk = e.DiskOk;
                    if (!e.DiskOk)
                    {
                        problems.Add(got == null ? "hue missing in huesext.mul" : "hue bytes differ");
                    }

                    break;
                }
            }

            e.Result = problems.Count == 0 ? "ok" : string.Join("; ", problems);
            Log($"{e.Kind} 0x{t:X}: disk {(e.DiskOk ? "ok" : "FAILED")}, reload {(e.ReloadOk ? "ok" : "FAILED")} {(problems.Count > 0 ? "- " + e.Result : string.Empty)}");
        }

        private static ushort RadarAt(string folder, string name, string magic, int baseId, int id)
        {
            string path = Path.Combine(folder, name);
            if (!File.Exists(path))
            {
                return 0;
            }

            byte[] data = ExtendedFormat.ReadRecords(path, magic, baseId, ExtendedFormat.RadarRecordSize, out int count);
            int k = id - baseId;
            return k >= 0 && k < count ? BitConverter.ToUInt16(data, k * 2) : (ushort)0;
        }

        private static byte[] ReadEntry(ExtendedFormat.ArtIndexEntry[] idx, int k, string dataPath)
        {
            if (k < 0 || k >= idx.Length || !idx[k].IsValid)
            {
                return null;
            }

            using var fs = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (idx[k].Offset + idx[k].Length > fs.Length)
            {
                return null;
            }

            var buffer = new byte[idx[k].Length];
            fs.Seek(idx[k].Offset, SeekOrigin.Begin);
            fs.ReadExactly(buffer, 0, buffer.Length);
            return buffer;
        }

        private static bool Same(byte[] a, byte[] b) => a != null && b != null && a.AsSpan().SequenceEqual(b);

        #endregion

        /// <summary>Removes the test entries from memory again (the state before the test).</summary>
        public static int Undo(PatchTestResult result)
        {
            int n = 0;
            foreach (PatchTestEntry e in result.Entries.Where(x => x.Patched))
            {
                int t = e.TargetId;
                switch (e.Kind)
                {
                    case UoExtensionKind.Items:
                        Art.RemoveStatic(t);
                        TileData.ItemTable[t] = e.OldItem;
                        RadarCol.SetItemColor(t, e.OldRadar);
                        break;
                    case UoExtensionKind.LandTiles:
                        Art.RemoveLand(t);
                        TileData.LandTable[t] = e.OldLand;
                        RadarCol.SetLandColor(t, e.OldRadar);
                        break;
                    case UoExtensionKind.Textures:
                        Textures.Remove(t);
                        break;
                    default:
                        Hue h = Hues.List[t];
                        Array.Copy(e.OldHueColors, h.Colors, 32);
                        h.TableStart = e.OldHueStart;
                        h.TableEnd = e.OldHueEnd;
                        h.Name = e.OldHueName;
                        break;
                }

                e.Patched = false;
                ++n;
            }

            result.Undone = true;
            return n;
        }
    }
}

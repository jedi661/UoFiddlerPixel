using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Models.Uop.Imaging;

namespace UoFiddler.Controls.Uop
{
    public static class VdImportHelper
    {
        private static void ValidateAndDumpFrameIfMismatch(Bitmap bmp, UopFrameHeader header, Rectangle reportedBounds, int animId, int action, int dir, int frameIndex)
        {
            try
            {
                int width = bmp.Width;
                int height = bmp.Height;
                int minX = width, minY = height, maxX = -1, maxY = -1;

                var bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                unsafe
                {
                    uint* pixels = (uint*)bd.Scan0.ToPointer();
                    int stride = bd.Stride / 4;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if ((pixels[y * stride + x] >> 24) > 0)
                            {
                                if (x < minX) minX = x;
                                if (x > maxX) maxX = x;
                                if (y < minY) minY = y;
                                if (y > maxY) maxY = y;
                            }
                        }
                    }
                }
                bmp.UnlockBits(bd);

                Rectangle actual = (maxX == -1) ? Rectangle.Empty : new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

                bool mismatch = false;
                if (actual == Rectangle.Empty && (reportedBounds.Width != width || reportedBounds.Height != height))
                    mismatch = true;
                else if (actual != Rectangle.Empty && (actual.X != reportedBounds.X || actual.Y != reportedBounds.Y || actual.Width != reportedBounds.Width || actual.Height != reportedBounds.Height))
                    mismatch = true;

                if (mismatch)
                {
                    string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uop_debug_frames");
                    Directory.CreateDirectory(baseDir);
                    string name = $"anim{animId}_act{action}_dir{dir}_f{frameIndex}";
                    string pngPath = Path.Combine(baseDir, $"{name}.png");
                    bmp.Save(pngPath, ImageFormat.Png);

                    var sb = new StringBuilder();
                    sb.AppendLine($"DEBUG FRAME MISMATCH: {name}");
                    sb.AppendLine($" Header: CenterX={header.CenterX}, CenterY={header.CenterY}, W={header.Width}, H={header.Height}");
                    sb.AppendLine($" ReportedBounds: {reportedBounds}");
                    sb.AppendLine($" ActualVisibleBounds: {actual}");
                    sb.AppendLine($" Saved PNG: {pngPath}");
                    System.Diagnostics.Debug.WriteLine(sb.ToString());
                    try { File.AppendAllText(Path.Combine(baseDir, "uop_debug_frames.log"), sb.ToString()); } catch { }
                }
            }
            catch (Exception ex)
            {
                try { System.Diagnostics.Debug.WriteLine($"ValidateAndDumpFrameIfMismatch exception: {ex.Message}"); } catch { }
            }
        }
        private const string VersionMarker = "HYBRID_HASH_V3_FINAL";

        // Track modified AnimIDs during VD import
        private static HashSet<int> _modifiedAnimIds = new HashSet<int>();


        public static void MarkAnimIdModified(int animId)
        {
            try
            {
                _modifiedAnimIds.Add(animId);
                System.Diagnostics.Debug.WriteLine($"🔖 MarkAnimIdModified: {animId}");
            }
            catch { }
        }

        public static HashSet<int> GetModifiedAnimIds() => new HashSet<int>(_modifiedAnimIds);
        public static void ClearModifiedAnimIds() => _modifiedAnimIds.Clear();

        private static void Log(string msg)
        {
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uop_debug.txt");
                File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            }
            catch { }
        }

        public static bool ImportCreaturesVdToUop(string vdFilePath, UopAnimationDataManager manager, int targetAnimId, string targetUopPath)
        {
            if (!File.Exists(vdFilePath)) return false;

            UopFileReader targetUopFile = null;
            if (!string.IsNullOrEmpty(targetUopPath)) targetUopFile = manager.GetReaderByPath(targetUopPath);
            if (targetUopFile == null) targetUopFile = GetTargetUopFileForAnimId(manager, targetAnimId);

            if (targetUopFile == null) return false;

            using (FileStream fs = new FileStream(vdFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                reader.ReadInt32(); // skip magic
                var offsetTable = new (int offset, int size, int reserved)[32, 5];
                for (int action = 0; action < 32; action++)
                    for (int dir = 0; dir < 5; dir++)
                        offsetTable[action, dir] = (reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                var indexDataAnimation = GetOrCreateIndexDataAnimation(manager, targetAnimId);
                for (int action = 0; action < 32; action++)
                {
                    var allDirectionsFrames = new Dictionary<int, List<DecodedUopFrame>>();
                    for (int dir = 0; dir < 5; dir++)
                    {
                        var entry = offsetTable[action, dir];
                        if (entry.offset <= 0) continue;
                        fs.Seek(entry.offset, SeekOrigin.Begin);
                        var frames = ReadVdFramesForDirection(reader, entry.size > 0 ? entry.offset + entry.size : fs.Length, entry.reserved == 1);
                        if (frames.Count > 0) allDirectionsFrames[dir] = frames;
                    }

                    if (allDirectionsFrames.Count > 0)
                    {
                        byte[] encodedData = EncodeActionToAmouBin(targetAnimId, action, allDirectionsFrames);
                        var uopGroup = indexDataAnimation.GetUopGroup(action, true);

                        ulong hash = UopFileReader.CreateHash($"build/animationlegacyframe/{targetAnimId:D6}/{action:D2}.bin");
                        var sharedFileInfo = new IndexDataFileInfo(targetUopFile, new UopDataHeader(0, 2, 0, (uint)encodedData.Length, hash, 1));

                        for (int dir = 0; dir < 5; dir++)
                        {
                            uopGroup.m_Direction[dir] = sharedFileInfo;
                            sharedFileInfo.SetModifiedData(dir, encodedData);

                            var uopAnim = new UopAnimIdx(targetAnimId, action, dir);
                            if (allDirectionsFrames.TryGetValue(dir, out var frms)) foreach (var f in frms) uopAnim.Frames.Add(f);
                            manager._animCache[$"{targetAnimId}_{action}_{dir}"] = uopAnim;
                        }
                    }
                }

                // Track modified AnimIDs
                _modifiedAnimIds.Add(targetAnimId);

                return true;
            }
        }


        private static IndexDataAnimation GetOrCreateIndexDataAnimation(UopAnimationDataManager manager, int targetAnimId)
        {
            var dataIndexField = typeof(UopAnimationDataManager).GetField("_dataIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dataIndex = (IndexDataAnimation[])dataIndexField.GetValue(manager);
            if (dataIndex[targetAnimId] == null) dataIndex[targetAnimId] = new IndexDataAnimation();
            return dataIndex[targetAnimId];
        }

        private static UopFileReader GetTargetUopFileForAnimId(UopAnimationDataManager manager, int targetAnimId)
        {
            var dataIndexField = typeof(UopAnimationDataManager).GetField("_dataIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var dataIndex = (IndexDataAnimation[])dataIndexField.GetValue(manager);
            if (targetAnimId >= 0 && targetAnimId < dataIndex.Length && dataIndex[targetAnimId] != null)
                for (int group = 0; group < 100; group++)
                {
                    var g = dataIndex[targetAnimId].GetUopGroup(group, false);
                    if (g != null && g.m_Direction[0] != null) return g.m_Direction[0].File;
                }
            var readersField = typeof(UopAnimationDataManager).GetField("_animationFrameReaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var readers = (List<UopFileReader>)readersField.GetValue(manager);
            return readers?.FirstOrDefault(r => r != null && r.IsLoaded);
        }

        private static List<DecodedUopFrame> ReadVdFramesForDirection(BinaryReader reader, long endOffset, bool hasPerFramePalette = false)
        {
            var frames = new List<DecodedUopFrame>();
            try
            {
                long dataStart = reader.BaseStream.Position;
                List<Color> globalPalette = null;
                if (!hasPerFramePalette)
                {
                    globalPalette = new List<Color>(256);
                    for (int i = 0; i < 256; i++)
                    {
                        ushort c555 = reader.ReadUInt16(); c555 ^= 0x8000;
                        if ((c555 & 0x8000) == 0 || i == 0) globalPalette.Add(Color.Transparent);
                        else globalPalette.Add(Color.FromArgb(255, ((c555 >> 10) & 0x1F) << 3, ((c555 >> 5) & 0x1F) << 3, (c555 & 0x1F) << 3));
                    }
                }
                int frameCount = reader.ReadInt32();
                if (frameCount <= 0) return frames;
                var frameOffsets = new int[frameCount];
                for (int i = 0; i < frameCount; i++) frameOffsets[i] = reader.ReadInt32();
                long baseOffsetForFrames = hasPerFramePalette ? dataStart : dataStart + 512;
                for (int frameNum = 0; frameNum < frameCount; frameNum++)
                {
                    if (frameOffsets[frameNum] == 0) continue;
                    reader.BaseStream.Seek(baseOffsetForFrames + frameOffsets[frameNum], SeekOrigin.Begin);
                    List<Color> currentPalette = globalPalette;
                    if (hasPerFramePalette)
                    {
                        currentPalette = new List<Color>(256);
                        for (int i = 0; i < 256; i++)
                        {
                            ushort c555 = reader.ReadUInt16(); c555 ^= 0x8000;
                            if ((c555 & 0x8000) == 0 || i == 0) currentPalette.Add(Color.Transparent);
                            else currentPalette.Add(Color.FromArgb(255, ((c555 >> 10) & 0x1F) << 3, ((c555 >> 5) & 0x1F) << 3, (c555 & 0x1F) << 3));
                        }
                    }
                    short centerX = reader.ReadInt16(), centerY = reader.ReadInt16();
                    ushort width = reader.ReadUInt16(), height = reader.ReadUInt16();

                    // Validate sizes to avoid absurd values
                    if (width == 0 || height == 0 || width > 4096 || height > 4096)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️ ReadVdFramesForDirection: Skipping frame with invalid dimensions W={width} H={height}");
                        continue;
                    }

                    var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var bd = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    unsafe
                    {
                        uint* pixels = (uint*)bd.Scan0.ToPointer(); int stride = bd.Stride / 4;
                        while (reader.BaseStream.Position < endOffset)
                        {
                            if (reader.BaseStream.Position + 4 > endOffset) break;
                            uint rleHeader = reader.ReadUInt32(); if (rleHeader == 0x7FFF7FFF) break;
                            int runLength = (int)(rleHeader & 0x0FFF), y = (int)((rleHeader >> 12) & 0x03FF), x = (int)((rleHeader >> 22) & 0x03FF);
                            if ((x & 0x200) != 0) x = -(0x400 - x); if ((y & 0x200) != 0) y = -(0x400 - y);
                            if (runLength == 0) continue;
                            if (reader.BaseStream.Position + runLength > endOffset) { reader.BaseStream.Seek(endOffset, SeekOrigin.Begin); break; }
                            byte[] pixelBytes = reader.ReadBytes(runLength);
                            for (int i = 0; i < runLength; i++)
                            {
                                int pixelX = centerX + x + i, pixelY = height - 1 - (-y - centerY);
                                if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                                {
                                    Color c = currentPalette[pixelBytes[i]];
                                    pixels[pixelY * stride + pixelX] = (uint)((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B);
                                }
                            }
                        }
                    }
                    bitmap.UnlockBits(bd);

                    frames.Add(new DecodedUopFrame { Header = new UopFrameHeader { CenterX = centerX, CenterY = centerY, Width = width, Height = height }, Palette = currentPalette, Image = bitmap });
                }
            }
            catch (Exception ex)
            {
                try { System.Diagnostics.Debug.WriteLine($"⚠️ ReadVdFramesForDirection exception: {ex.Message}"); } catch { }
            }
            return frames;
        }

        internal static void EncodeRlePixels(BinaryWriter writer, Bitmap image, List<Color> palette, UopFrameHeader header)
        {
            const int MAX_RUN = 0x0FFF;      // 12 bits
            const int COORD_LIMIT = 0x200;   // 10 bits -> signed range -512..511

            var bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var colorLookup = new Dictionary<uint, byte>();
            unsafe
            {
                uint* pixels = (uint*)bd.Scan0.ToPointer();
                int stride = bd.Stride / 4;

                for (int y = 0; y < image.Height; y++)
                {
                    int x = 0;
                    while (x < image.Width)
                    {
                        // skip transparent pixels
                        while (x < image.Width && (pixels[y * stride + x] >> 24) == 0) x++;
                        if (x >= image.Width) break;

                        int runStart = x;
                        var runPixels = new List<byte>();

                        // collect a run of opaque pixels
                        while (x < image.Width && (pixels[y * stride + x] >> 24) > 0)
                        {
                            uint pixelValue = pixels[y * stride + x];
                            if (!colorLookup.TryGetValue(pixelValue, out byte idx))
                            {
                                Color c = Color.FromArgb((int)pixelValue);
                                idx = FindClosestPaletteIndex(c, palette);
                                colorLookup[pixelValue] = idx;
                            }
                            runPixels.Add(idx);
                            x++;
                        }

                        int remaining = runPixels.Count;
                        int localRunOffset = 0;

                        // Write the run in chunks that fit the header fields
                        while (remaining > 0)
                        {
                            int chunkLen = Math.Min(remaining, MAX_RUN);

                            // compute rleX/rleY for current chunk start
                            int currentRunStart = runStart + localRunOffset;
                            int rleX = currentRunStart - header.CenterX;
                            int rleY = y - image.Height + 1 - header.CenterY;

                            // If rleY is outside signed 10-bit range, it's unrecoverable for this line:
                            if (rleY < -COORD_LIMIT || rleY > COORD_LIMIT - 1)
                            {
                                // Log and abort this chunk (skip writing) to avoid corrupting stream
                                try { System.Diagnostics.Debug.WriteLine($"⚠️ RLE Y out of range: y={y}, height={image.Height}, centerY={header.CenterY}, rleY={rleY}"); } catch { }
                                break;
                            }

                            // Adjust chunkLen so that rleX for chunk stays within range [-512..511]
                            int allowedMinStart = header.CenterX - (COORD_LIMIT);
                            int allowedMaxStart = header.CenterX + (COORD_LIMIT - 1);
                            if (currentRunStart < allowedMinStart)
                            {
                                // advance start until inside allowed range
                                int skip = allowedMinStart - currentRunStart;
                                localRunOffset += skip;
                                remaining -= skip;
                                if (remaining <= 0) break;
                                currentRunStart = runStart + localRunOffset;
                                rleX = currentRunStart - header.CenterX;
                            }

                            if (currentRunStart > allowedMaxStart)
                            {
                                // cannot write this portion, skip to avoid corruption
                                try { System.Diagnostics.Debug.WriteLine($"⚠️ RLE X start out of range and cannot be fixed: runStart={currentRunStart}, centerX={header.CenterX}"); } catch { }
                                break;
                            }

                            // ensure chunk does not exceed the allowed X boundary
                            int maxAllowedLen = allowedMaxStart - currentRunStart + 1;
                            if (chunkLen > maxAllowedLen) chunkLen = maxAllowedLen;
                            if (chunkLen <= 0) break;

                            // Build header and write chunk
                            uint rleHeader = (uint)((chunkLen & 0x0FFF) | ((rleY & 0x03FF) << 12) | ((rleX & 0x03FF) << 22));
                            writer.Write(rleHeader);

                            // write only the slice of bytes for this chunk
                            var slice = new byte[chunkLen];
                            for (int s = 0; s < chunkLen; s++)
                            {
                                slice[s] = runPixels[localRunOffset + s];
                            }
                            writer.Write(slice);

                            // advance
                            remaining -= chunkLen;
                            localRunOffset += chunkLen;
                        }
                    }
                }
            }
            image.UnlockBits(bd);
            writer.Write((uint)0x7FFF7FFF);
        }

        private static byte FindClosestPaletteIndex(Color target, List<Color> palette)
        {
            int minDistance = int.MaxValue; byte closest = 0;
            for (int i = 0; i < palette.Count; i++)
            {
                int d = Math.Abs(target.R - palette[i].R) + Math.Abs(target.G - palette[i].G) + Math.Abs(target.B - palette[i].B);
                if (d < minDistance) { minDistance = d; closest = (byte)i; }
            }
            return closest;
        }

        private static Rectangle GetVisibleBounds(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            int minX = width, minY = height, maxX = -1, maxY = -1;

            var bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            unsafe
            {
                uint* pixels = (uint*)bd.Scan0.ToPointer();
                int stride = bd.Stride / 4;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if ((pixels[y * stride + x] >> 24) > 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            }
            bmp.UnlockBits(bd);

            // IMPORTANT: return Rectangle.Empty if no visible pixel found.
            if (maxX == -1) return Rectangle.Empty;
            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static byte[] EncodeActionToAmouBin(int animId, int action, Dictionary<int, List<DecodedUopFrame>> allDirectionsFrames)
        {
            if (allDirectionsFrames == null || allDirectionsFrames.Count == 0) return null;

            // nombre maximal de frames par direction
            int maxFrames = 0;
            List<Color> globalPal = new List<Color>();

            // 1) Trouver maxFrames et rassembler palette de référence
            foreach (var frames in allDirectionsFrames.Values)
            {
                if (frames.Count > maxFrames) maxFrames = frames.Count;
                foreach (var f in frames)
                {
                    if (globalPal.Count == 0 && f.Palette != null) globalPal.AddRange(f.Palette);
                }
            }

            // Si aucune frame trouvée -> abort
            if (maxFrames == 0)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ EncodeActionToAmouBin: no frames for Anim={animId} Action={action} -> aborting AMOU creation");
                return null;
            }

            // 2) Recalculer correctement les bornes min/max depuis les headers de frames
            int minL = int.MaxValue, minT = int.MaxValue, maxR = int.MinValue, maxB = int.MinValue;
            foreach (var frames in allDirectionsFrames.Values)
            {
                foreach (var f in frames)
                {
                    // Use visible bounds for accurate global bounding box
                    var bounds = GetVisibleBounds(f.Image);
                    int left = bounds.X - f.Header.CenterX;
                    int top = bounds.Y - f.Header.CenterY - f.Header.Height;
                    int right = (bounds.X + bounds.Width) - f.Header.CenterX;
                    int bottom = (bounds.Y + bounds.Height) - f.Header.CenterY - f.Header.Height;

                    if (left < minL) minL = left;
                    if (top < minT) minT = top;
                    if (right > maxR) maxR = right;
                    if (bottom > maxB) maxB = bottom;
                }
            }

            if (minL == int.MaxValue) minL = 0;
            if (minT == int.MaxValue) minT = 0;
            if (maxR == int.MinValue) maxR = 0;
            if (maxB == int.MinValue) maxB = 0;

            // Clamp aux bornes d'un Int16 avant écriture
            short sMinL = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, minL));
            short sMinT = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, minT));
            short sMaxR = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, maxR));
            short sMaxB = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, maxB));

            // 3) Pad palette si besoin
            while (globalPal.Count < 256) globalPal.Add(Color.Transparent);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Header minimal
                writer.Write(0x554F4D41u);
                writer.Write(1u);
                writer.Write(0u); // TotalSize -> patché après
                writer.Write((uint)animId);

                // Écrire les bounding boxes calculees
                writer.Write(sMinL);
                writer.Write(sMinT);
                writer.Write(sMaxR);
                writer.Write(sMaxB);

                // Palette count / header size / frame count / frame index offset
                writer.Write(256u);
                writer.Write(40u);
                writer.Write((uint)(maxFrames * 5));
                // CORRECTION: palette entries are written as 256 ushorts (512 bytes)
                writer.Write((uint)(40 + 512));

                // Palette (555 encoded ushorts) - global palette
                for (int k = 0; k < 256; k++)
                {
                    Color c = (k < globalPal.Count) ? globalPal[k] : Color.Transparent;
                    ushort c555 = (ushort)(((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3));
                    if (c.A == 0) c555 |= 0x8000;
                    writer.Write(c555);
                }

                // Table d'index (maxFrames * 5 * 16 bytes)
                long indexStart = ms.Position;
                writer.Write(new byte[maxFrames * 5 * 16]);

                var indices = new List<UopFrameIndex>();

                // Écrire les frames (par direction et frame)
                for (int dir = 0; dir < 5; dir++)
                {
                    var frames = allDirectionsFrames.ContainsKey(dir) ? allDirectionsFrames[dir] : new List<DecodedUopFrame>();
                    for (int i = 0; i < maxFrames; i++)
                    {
                        if (i >= frames.Count)
                        {
                            // pas de frame -> entrée vide
                            indices.Add(new UopFrameIndex { Direction = (ushort)dir, FrameNumber = (ushort)i, FrameDataOffset = 0, Left = 0, Top = 0, Right = 0, Bottom = 0 });
                            continue;
                        }

                        var f = frames[i];
                        var bounds = GetVisibleBounds(f.Image);

                        // frame complètement vide -> entrée vide
                        if (bounds == Rectangle.Empty)
                        {
                            indices.Add(new UopFrameIndex { Direction = (ushort)dir, FrameNumber = (ushort)i, FrameDataOffset = 0, Left = 0, Top = 0, Right = 0, Bottom = 0 });
                            continue;
                        }

                        // calculer relOff maintenant que l'on sait qu'on va écrire des données
                        long dataPos = ms.Position;
                        uint relOff = (uint)(dataPos - (indexStart + (dir * maxFrames + i) * 16));

                        // utiliser les dimensions réelles de la Bitmap (défensive)
                        int actualWidth = (f.Image != null) ? f.Image.Width : f.Header.Width;
                        int actualHeight = (f.Image != null) ? f.Image.Height : f.Header.Height;

                        if (f.Header.Width != actualWidth || f.Header.Height != actualHeight)
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠️ EncodeActionToAmouBin: header size mismatch Anim={animId} Act={action} Dir={dir} Frame={i} - Header={f.Header.Width}x{f.Header.Height} Bitmap={actualWidth}x{actualHeight}. Using bitmap dimensions.");
                            f.Header.Width = (ushort)actualWidth;
                            f.Header.Height = (ushort)actualHeight;
                        }

                        // Palette locale (on écrit la palette globale par frame pour compatibilité as ushorts)
                        for (int k = 0; k < 256; k++)
                        {
                            Color c = (k < f.Palette.Count) ? f.Palette[k] : Color.Transparent;
                            ushort c555 = (ushort)(((c.R >> 3) << 10) | ((c.G >> 3) << 5) | (c.B >> 3));
                            if (c.A == 0) c555 |= 0x8000;
                            writer.Write(c555);
                        }

                        // Frame header (center + dimensions réelles)
                        writer.Write(f.Header.CenterX);
                        writer.Write(f.Header.CenterY);
                        writer.Write((ushort)actualWidth);
                        writer.Write((ushort)actualHeight);

                        // RLE
                        EncodeRlePixels(writer, f.Image, f.Palette, f.Header);

                        indices.Add(new UopFrameIndex
                        {
                            Direction = (ushort)dir,
                            FrameNumber = (ushort)i,
                            FrameDataOffset = relOff,
                            Left = (short)(bounds.X - f.Header.CenterX),
                            Top = (short)(bounds.Y - f.Header.CenterY - actualHeight + 1),
                            Right = (short)((bounds.X + bounds.Width - 1) - f.Header.CenterX),
                            Bottom = (short)((bounds.Y + bounds.Height - 1) - f.Header.CenterY - actualHeight + 1)
                        });
                    }
                }

                // Réécrire la table d'index
                ms.Seek(indexStart, SeekOrigin.Begin);
                ushort globId = 1;
                foreach (var idx in indices)
                {
                    writer.Write((ushort)action);
                    writer.Write(globId++);
                    writer.Write(idx.Left);
                    writer.Write(idx.Top);
                    writer.Write(idx.Right);
                    writer.Write(idx.Bottom);
                    writer.Write(idx.FrameDataOffset);
                }

                // patch TotalSize
                ms.Seek(8, SeekOrigin.Begin);
                writer.Write((uint)ms.Length);

                var result = ms.ToArray();

                // Validation rapide
                if (!ValidateAmouBin(result, animId, action))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ EncodeActionToAmouBin produced invalid AMOU for Anim={animId} Action={action}. Aborting this action export.");
                    return null;
                }

                return result;
            }
        }



        /// <summary>
        /// Sauvegarde les animations UOP en mode HYBRIDE (Jenkins + Numérique)
        /// </summary>
        public static bool SaveModifiedAnimationsToUopHybrid(
    UopAnimationDataManager manager,
    int animId,
    string outputUopPath)
        {
            try
            {
                // 1) Rassembler la liste complète des AnimIDs modifiés
                var animIdsToSave = new HashSet<int>();

                // a) animId passé en paramètre (compatibilité)
                if (animId >= 0) animIdsToSave.Add(animId);

                

                // c) IDs présents dans le cache et marqués IsModified
                try
                {
                    var cacheField = typeof(UopAnimationDataManager).GetField("_animCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                      ?? typeof(UopAnimationDataManager).GetField("_animCache", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (cacheField != null)
                    {
                        var cache = cacheField.GetValue(manager) as System.Collections.IDictionary;
                        if (cache != null)
                        {
                            foreach (System.Collections.DictionaryEntry kv in cache)
                            {
                                string key = kv.Key as string;
                                if (string.IsNullOrEmpty(key)) continue;
                                var parts = key.Split('_');
                                if (parts.Length >= 3 && int.TryParse(parts[0], out int kAnimId))
                                {
                                    var uopAnim = kv.Value as UopAnimIdx;
                                    if (uopAnim != null)
                                    {
                                        var isModProp = uopAnim.GetType().GetProperty("IsModified");
                                        if (isModProp != null && (bool)(isModProp.GetValue(uopAnim) ?? false))
                                        {
                                            animIdsToSave.Add(kAnimId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch { /* best-effort */ }

                if (animIdsToSave.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("SaveModifiedAnimationsToUopHybrid: nothing to save.");
                    return false;
                }

                // 2) Préparer dossier de sortie
                string outputDir = null;
                try
                {
                    outputDir = Path.GetDirectoryName(outputUopPath);
                }
                catch { outputDir = null; }
                if (string.IsNullOrEmpty(outputDir)) outputDir = AppDomain.CurrentDomain.BaseDirectory;

                bool anyFileSaved = false;

                // 3) Grouper les AnimIDs par fichier source (ou fallback -> destination dir)
                var fileMap = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                foreach (int id in animIdsToSave)
                {
                    string sourcePath = null;
                    try
                    {
                        var fi = manager.GetAnimationData(id, 0, 0);
                        if (fi != null && fi.File != null && !string.IsNullOrEmpty(fi.File.FilePath))
                            sourcePath = fi.File.FilePath;
                    }
                    catch { sourcePath = null; }

                    string destFile;
                    if (!string.IsNullOrEmpty(sourcePath))
                    {
                        destFile = Path.Combine(outputDir, Path.GetFileName(sourcePath));
                    }
                    else
                    {
                        // fallback: use provided outputUopPath filename (use once) or default name per id group
                        destFile = Path.Combine(outputDir, Path.GetFileName(outputUopPath) ?? $"AnimationFrame_{id}.uop");
                    }

                    if (!fileMap.TryGetValue(destFile, out var list)) { list = new List<int>(); fileMap[destFile] = list; }
                    if (!list.Contains(id)) list.Add(id);
                }

                // 4) Pour chaque fichier destination, reconstruire le .uop en intégrant toutes les AnimIDs du groupe
                foreach (var kv in fileMap)
                {
                    string destPath = kv.Key;
                    var groupAnimIds = new HashSet<int>(kv.Value);

                    // Charger le reader source : essayer d'ouvrir le fichier dest (s'il existe) pour reprendre les autres entrées
                    UopFileReader sourceReader = null;
                    try
                    {
                        if (File.Exists(destPath))
                        {
                            var r = new UopFileReader(destPath);
                            if (r.Load()) sourceReader = r;
                        }
                    }
                    catch { sourceReader = null; }

                    // Si aucune source locale, tenter pour chaque anim du groupe de trouver son reader et prendre le premier
                    if (sourceReader == null)
                    {
                        foreach (int id in groupAnimIds)
                        {
                            try
                            {
                                var fi = manager.GetAnimationData(id, 0, 0);
                                if (fi != null && fi.File != null && !string.IsNullOrEmpty(fi.File.FilePath))
                                {
                                    var r = new UopFileReader(fi.File.FilePath);
                                    if (r.Load())
                                    {
                                        sourceReader = r;
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    var finalFileDataList = new List<UopFileData>();

                    // 4.1 Conserver toutes les entrées existantes sauf celles relatives aux AnimIDs du groupe
                    if (sourceReader != null)
                    {
                        var allEntries = sourceReader.GetAllEntries();
                        foreach (var entry in allEntries)
                        {
                            ulong hash = entry.Key;
                            bool belongsToGroup = false;

                            foreach (int id in groupAnimIds)
                            {
                                // Jenkins hashes for this id
                                for (int action = 0; action < 32; action++)
                                {
                                    string jenkinsPath = $"build/animationlegacyframe/{id:D6}/{action:D2}.bin";
                                    if (hash == UopFileReader.CreateHash(jenkinsPath)) { belongsToGroup = true; break; }
                                }
                                if (belongsToGroup) break;

                                // Numeric hash
                                if (((hash & 0xFFFF000000000000UL) == 0x000C000000000000UL) && ((hash & 0x0000FFFFUL) == (ulong)id))
                                {
                                    belongsToGroup = true;
                                    break;
                                }
                            }

                            if (belongsToGroup)
                            {
                                System.Diagnostics.Debug.WriteLine($"🗑️ Removing old entry for group (file={Path.GetFileName(destPath)}) hash={hash:X16}");
                                continue;
                            }

                            byte[] existingData = null;
                            try { existingData = sourceReader.ReadData(entry.Value); } catch { existingData = null; }
                            if (existingData != null && existingData.Length > 0)
                            {
                                finalFileDataList.Add(new UopFileData
                                {
                                    Hash = entry.Key,
                                    Data = existingData,
                                    DecompressedSize = (uint)existingData.Length,
                                    IsCompressed = true,
                                    IsEmpty = false
                                });
                            }
                        }
                    }

                    // 4.2 Ajouter les nouvelles entrées pour chaque animId du groupe
                    foreach (int id in groupAnimIds)
                    {
                        for (int action = 0; action < 32; action++)
                        {
                            var allDirectionsFrames = new Dictionary<int, List<DecodedUopFrame>>();
                            bool hasData = false;
                            for (int dir = 0; dir < 5; dir++)
                            {
                                var uopAnim = manager.GetUopAnimation(id, action, dir);
                                if (uopAnim != null && uopAnim.Frames != null && uopAnim.Frames.Count > 0)
                                {
                                    allDirectionsFrames[dir] = uopAnim.Frames;
                                    hasData = true;
                                }
                            }
                            if (!hasData) continue;

                            byte[] amouData = EncodeActionToAmouBin(id, action, allDirectionsFrames);
                            if (amouData == null || amouData.Length == 0) continue;

                            string jenkinsPath = $"build/animationlegacyframe/{id:D6}/{action:D2}.bin";
                            ulong jenkinsHash = UopFileReader.CreateHash(jenkinsPath);
                            finalFileDataList.Add(new UopFileData
                            {
                                Hash = jenkinsHash,
                                Data = amouData,
                                DecompressedSize = (uint)amouData.Length,
                                IsCompressed = true,
                                IsEmpty = false
                            });

                            if (action == 0)
                            {
                                ulong numericHash = 0x000C000000000000UL | (ulong)id;
                                finalFileDataList.Add(new UopFileData
                                {
                                    Hash = numericHash,
                                    Data = amouData,
                                    DecompressedSize = (uint)amouData.Length,
                                    IsCompressed = true,
                                    IsEmpty = false
                                });
                            }
                        }
                    }

                    if (finalFileDataList.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ No data to write for file {destPath}");
                        continue;
                    }

                    finalFileDataList = finalFileDataList.OrderBy(e => e.Hash).ToList();

                    // Écrire le fichier UOP
                    UopFileWriter.WriteUopFile(destPath, finalFileDataList, 1000);
                    System.Diagnostics.Debug.WriteLine($"✅ Hybrid UOP file written: {destPath}");
                    anyFileSaved = true;
                }

                

                return anyFileSaved;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error saving hybrid UOP set: {ex.Message}");
                return false;
            }
        }

        // Ajout à VdImportHelper : validation simple du AMOU généré
        private static bool ValidateAmouBin(byte[] binData, int animId, int action)
        {
            try
            {
                // Vérifier header minimal
                var header = UopAnimationDataManager.ReadAmouHeader(binData);
                if (header == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ValidateAmouBin: header null for Anim={animId} Act={action}");
                    return false;
                }

                if (header.FrameCount == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ValidateAmouBin: FrameCount==0 for Anim={animId} Act={action}");
                    return false;
                }

                // Essayer de charger la première frame (direction 0, frame 0) via le loader existant
                var test = UopAnimationDataManager.LoadFromUopBin(binData, 0, 0);
                if (test == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ValidateAmouBin: failed to decode first frame for Anim={animId} Act={action}");
                    return false;
                }

                // Validate basic geometry consistency
                if (test.Header.Width == 0 || test.Header.Height == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ValidateAmouBin: invalid dims W/H for Anim={animId} Act={action} => {test.Header.Width}x{test.Header.Height}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ValidateAmouBin exception Anim={animId} Act={action}: {ex.Message}");
                return false;
            }
        }
    }
}
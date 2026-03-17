using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UoFiddler.Controls.Models.Uop;
using UoFiddler.Controls.Models.Uop.Imaging;

namespace UoFiddler.Controls.Uop
{
    public static class VdExportHelper
    {
        public static void WriteVDHeader(BinaryWriter writer, short animType)
        {
            // NOTE:
            // Historically AnimationEdit.ExportToVD writes a 2-byte value = 6 as first short,
            // then the animType short. Older UoFiddler releases expect that format.
            // Align here so exported .vd files match the "Misc -> Export All Valid to .vd" output.
            writer.Write((short)6);
            writer.Write(animType);
        }

        public static int GetVdLength(short animType)
        {
            return animType switch
            {
                0 => 22,  // Animals (MUL)
                1 => 13,  // Monsters (MUL)
                2 => 35,  // Sea Monsters (MUL)
                4 => 32,  // ✅ CORRECTION : Creatures (UOP) → 32 actions !
                _ => 22
            };
        }

        public static int GetDirectionCount(short animType) => 5;

        // ✅ MÉTHODE GÉNÉRIQUE pour MUL (Animals, Monsters, Sea Monsters)
        public static void WriteVDAnimations(BinaryWriter writer, UOAnimation?[] exportedAnimations, short animType, double scale = 1.0)
        {
            int vdLength = GetVdLength(animType);
            int dirCount = GetDirectionCount(animType);

            long headerPos = writer.BaseStream.Position;
            long animPos = headerPos + (vdLength * dirCount * 12);

            for (int i = 0; i < vdLength; i++)
            {
                for (int dir = 0; dir < dirCount; dir++)
                {
                    if (i < exportedAnimations.Length && exportedAnimations[i]?.Frames?.Count > 0)
                    {
                        writer.Write((int)animPos);
                        writer.Write(0);
                        writer.Write(0);
                    }
                    else
                    {
                        writer.Write(0);
                        writer.Write(0);
                        writer.Write(0);
                    }
                }
            }

            for (int i = 0; i < vdLength && i < exportedAnimations.Length; i++)
            {
                var anim = exportedAnimations[i];
                if (anim != null)
                {
                    if (scale != 1.0) anim.Resize(scale);
                    for (int dir = 0; dir < dirCount; dir++)
                    {
                        anim.ExportAnimationToVD(writer, dir, ref headerPos, ref animPos);
                    }
                }
                else
                {
                    headerPos += (dirCount * 12);
                }
            }
        }

        // ✅ NOUVELLE MÉTHODE : Export spécifique pour Creatures (UOP) avec 32 actions
        public static void WriteVDCreaturesUop(BinaryWriter writer, UopAnimationDataManager uopManager, int sourceAnimId, Dictionary<int, int> remapping = null, double scale = 1.0)
        {
            const int actionCount = 32;
            const int directionCount = 5;

            // ✅ 1. ÉCRIRE LE HEADER VD
            WriteVDHeader(writer, 4); // animType = 4 (Creatures UOP)

            // ✅ 2. RÉSERVER LA TABLE DES OFFSETS (32 actions × 5 directions × 12 bytes)
            long offsetTablePos = writer.BaseStream.Position;
            var offsetTable = new (long offset, int size)[actionCount, directionCount];

            // Écrire des zéros temporaires
            for (int action = 0; action < actionCount; action++)
            {
                for (int dir = 0; dir < directionCount; dir++)
                {
                    writer.Write((int)0); // offset
                    writer.Write((int)0); // size
                    writer.Write((int)0); // reserved
                }
            }

            // ✅ 3. ÉCRIRE LES DONNÉES DE CHAQUE ACTION/DIRECTION
            // On boucle sur les actions CIBLES (0 à 31)
            for (int targetAction = 0; targetAction < actionCount; targetAction++)
            {
                // Déterminer l'action SOURCE
                int sourceAction = targetAction;

                if (remapping != null)
                {
                    // IMPORTANT : si un dictionnaire de remappage est fourni, n'exporter
                    // que les actions explicitement mappées. L'absence d'entrée = NONE.
                    if (!remapping.TryGetValue(targetAction, out int mappedSource))
                    {
                        // Pas de mapping -> considérer la cible comme NONE et sauter
                        continue;
                    }
                    sourceAction = mappedSource;
                }
                // si remapping == null => comportement par défaut (1:1)

                if (sourceAction < 0) continue;

                // ✅ Vérifier si l'action existe dans le cache UOP
                bool hasValidData = false;
                for (int dir = 0; dir < directionCount; dir++)
                {
                    var uopAnim = uopManager.GetUopAnimation(sourceAnimId, sourceAction, dir);
                    if (uopAnim != null && uopAnim.Frames.Count > 0)
                    {
                        hasValidData = true;
                        break;
                    }
                }

                if (!hasValidData)
                {
                    // System.Diagnostics.Debug.WriteLine($"⚠️ Target Action {targetAction} (Source {sourceAction}) skipped (no data)");
                    continue;
                }

                for (int dir = 0; dir < directionCount; dir++)
                {
                    var uopAnim = uopManager.GetUopAnimation(sourceAnimId, sourceAction, dir);
                    if (uopAnim == null || uopAnim.Frames.Count == 0)
                    {
                        continue;
                    }

                    long startOffset = writer.BaseStream.Position;

                    // ✅ LE FORMAT "PER-FRAME PALETTE" (Reserved=1) NE COMMENCE PAS PAR UNE PALETTE GLOBALE
                    // Il commence directement par FrameCount.
                    
                    // ✅ ÉCRIRE LE FRAMECOUNT
                    writer.Write((int)uopAnim.Frames.Count);

                    // ✅ RÉSERVER LA TABLE DES OFFSETS DE FRAMES
                    long frameTableStart = writer.BaseStream.Position;
                    for (int i = 0; i < uopAnim.Frames.Count; i++)
                    {
                        writer.Write((int)0); // Offset relatif (sera mis à jour)
                    }

                    // ✅ ÉCRIRE LES FRAMES (AVEC PALETTE LOCALE)
                    var frameOffsets = new int[uopAnim.Frames.Count];
                    for (int frameNum = 0; frameNum < uopAnim.Frames.Count; frameNum++)
                    {
                        var frame = uopAnim.Frames[frameNum];

                        // ✅ Calculer l'offset RELATIF depuis le DÉBUT DU BLOC DE DONNÉES (startOffset)
                        // car dans le lecteur modifié, si hasPerFramePalette=true, baseOffset = dataStart.
                        long frameDataStart = writer.BaseStream.Position;
                        frameOffsets[frameNum] = (int)(frameDataStart - startOffset);

                        // ✅ ÉCRIRE LA PALETTE LOCALE (512 bytes)
                        var palette = frame.Palette ?? new List<Color>();
                        int writtenColors = 0;
                        foreach (var color in palette)
                        {
                            if (writtenColors >= 256) break;
                            ushort color555 = (ushort)(
                                0x8000 | // MSB = 1 (opaque)
                                ((color.R >> 3) << 10) |
                                ((color.G >> 3) << 5) |
                                (color.B >> 3)
                            );
                            writer.Write((ushort)(color555 ^ 0x8000));
                            writtenColors++;
                        }
                        // Padding si moins de 256 couleurs
                        while (writtenColors < 256)
                        {
                            writer.Write((ushort)0);
                            writtenColors++;
                        }

                        // ✅ Frame Header & Resize logic
                        short finalCenterX = (short)(frame.Header.CenterX * scale);
                        short finalCenterY = (short)(frame.Header.CenterY * scale);
                        Bitmap finalImage = frame.Image;

                        if (scale != 1.0)
                        {
                            int newWidth = (int)Math.Max(1, frame.Header.Width * scale);
                            int newHeight = (int)Math.Max(1, frame.Header.Height * scale);
                            Bitmap resized = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(resized))
                            {
                                // Use NearestNeighbor to preserve palette colors
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                                g.DrawImage(frame.Image, 0, 0, newWidth, newHeight);
                            }
                            finalImage = resized;
                        }

                        writer.Write(finalCenterX);
                        writer.Write(finalCenterY);
                        writer.Write((ushort)finalImage.Width);
                        writer.Write((ushort)finalImage.Height);

                        // ✅ Pixels RLE
                        var tempHeader = new UoFiddler.Controls.Uop.UopFrameHeader { 
                            CenterX = finalCenterX, 
                            CenterY = finalCenterY, 
                            Width = (ushort)finalImage.Width, 
                            Height = (ushort)finalImage.Height 
                        };
                        VdImportHelper.EncodeRlePixels(writer, finalImage, frame.Palette, tempHeader);

                        if (scale != 1.0) finalImage.Dispose();
                    }

                    long endOffset = writer.BaseStream.Position;
                    int dataSize = (int)(endOffset - startOffset);

                    // ✅ Mettre à jour la table des offsets de frames
                    writer.BaseStream.Seek(frameTableStart, SeekOrigin.Begin);
                    foreach (var offset in frameOffsets)
                    {
                        writer.Write(offset);
                    }
                    writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);

                    // ✅ Stocker l'offset/size pour la table principale (Target Action !)
                    // ET SET RESERVED = 1 POUR INDIQUER LE FORMAT AVEC PALETTE PAR FRAME
                    offsetTable[targetAction, dir] = (startOffset, dataSize);

                    System.Diagnostics.Debug.WriteLine($"✅ Exported [Target {targetAction}, Source {sourceAction}, Dir {dir}]: offset={startOffset}, size={dataSize}, frames={uopAnim.Frames.Count}");
                }
            }

            // ✅ 4. METTRE À JOUR LA TABLE DES OFFSETS PRINCIPALE
            writer.BaseStream.Seek(offsetTablePos, SeekOrigin.Begin);
            for (int action = 0; action < actionCount; action++)
            {
                for (int dir = 0; dir < directionCount; dir++)
                {
                    writer.Write((int)offsetTable[action, dir].offset);
                    writer.Write((int)offsetTable[action, dir].size);
                    writer.Write((int)(offsetTable[action, dir].size > 0 ? 1 : 0)); // ✅ Reserved = 1 if data exists (Per-Frame Palette)
                }
            }

            writer.Flush();
            System.Diagnostics.Debug.WriteLine($"🎉 VD export completed for animId {sourceAnimId}");
        }
        
        public static List<ColourEntry> GenerateProperPaletteFromImage(DirectBitmap frameImage, List<ColourEntry> existingPalette = null)
        {
            if (existingPalette != null && existingPalette.Count > 0)
            {
                int colorCount = existingPalette.Count(c => c.R > 0 || c.G > 0 || c.B > 0);
                if (colorCount > 0)
                {
                    if (existingPalette.Count < 256)
                    {
                        var padded = new List<ColourEntry>(existingPalette);
                        padded.AddRange(Enumerable.Repeat(new ColourEntry(0, 0, 0, 0), 256 - padded.Count));
                        return padded;
                    }
                    return existingPalette.Take(256).ToList();
                }
            }

            HashSet<Color> uniqueColors = new HashSet<Color>();
            for (int y = 0; y < frameImage.Height; y++)
            {
                for (int x = 0; x < frameImage.Width; x++)
                {
                    Color pixel = frameImage.GetPixel(x, y);
                    if (pixel.A > 0)
                    {
                        uniqueColors.Add(pixel);
                    }
                }
            }
            
            List<ColourEntry> fallbackPalette = new List<ColourEntry>();
            // ✅ Reserve Index 0 for Transparency
            fallbackPalette.Add(new ColourEntry(0, 0, 0, 0));

            foreach (var color in uniqueColors.Take(255))
            {
                fallbackPalette.Add(new ColourEntry(color.R, color.G, color.B, color.A));
            }

            while (fallbackPalette.Count < 256)
            {
                fallbackPalette.Add(new ColourEntry(0, 0, 0, 0));
            }

            return fallbackPalette;
        }
    }
}

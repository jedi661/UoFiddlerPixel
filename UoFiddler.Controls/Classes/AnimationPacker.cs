using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Classes
{
    public static class AnimationPacker
    {
        public class FrameInfo
        {
            public Bitmap Image { get; set; }
            public Point Center { get; set; }
            public int Direction { get; set; }
            public int Index { get; set; }
        }

        #region Spritesheet Export

        public static void ExportToSpritesheet(List<FrameInfo> frames, string outputDir, string baseName, int maxWidth, int spacing, bool oneRowPerDir)
        {
            if (frames == null || frames.Count == 0) return;

            // Packing Logic
            int currentX = spacing;
            int currentY = spacing;
            int rowHeight = 0;
            int canvasWidth = 0;
            int canvasHeight = 0;

            var packedFrames = new List<PackedFrameEntry>();

            // Group by direction if oneRowPerDir
            if (oneRowPerDir)
            {
                var grouped = frames.GroupBy(f => f.Direction).OrderBy(g => g.Key);
                foreach (var group in grouped)
                {
                    currentX = spacing;
                    rowHeight = 0;
                    
                    foreach (var frame in group)
                    {
                        // Add to current row
                        int w = frame.Image.Width;
                        int h = frame.Image.Height;

                        // Just accumulate in X
                        var entry = new PackedFrameEntry
                        {
                            Direction = frame.Direction,
                            Index = frame.Index,
                            Frame = new Rect { X = currentX, Y = currentY, W = w, H = h },
                            Center = new PointStruct { X = frame.Center.X, Y = frame.Center.Y }
                        };
                        packedFrames.Add(entry);

                        currentX += w + spacing;
                        rowHeight = Math.Max(rowHeight, h);
                    }
                    canvasWidth = Math.Max(canvasWidth, currentX);
                    currentY += rowHeight + spacing;
                    canvasHeight = currentY;
                }
            }
            else
            {
                // Standard packing (Shelf)
                foreach (var frame in frames)
                {
                    int w = frame.Image.Width;
                    int h = frame.Image.Height;

                    if (currentX + w > maxWidth)
                    {
                        currentY += rowHeight + spacing;
                        currentX = spacing;
                        rowHeight = 0;
                    }

                    if (currentX == spacing) rowHeight = h;
                    else rowHeight = Math.Max(rowHeight, h);

                    var entry = new PackedFrameEntry
                    {
                        Direction = frame.Direction,
                        Index = frame.Index,
                        Frame = new Rect { X = currentX, Y = currentY, W = w, H = h },
                        Center = new PointStruct { X = frame.Center.X, Y = frame.Center.Y }
                    };
                    packedFrames.Add(entry);

                    canvasWidth = Math.Max(canvasWidth, currentX + w + spacing);
                    canvasHeight = Math.Max(canvasHeight, currentY + rowHeight + spacing);
                    currentX += w + spacing;
                }
            }

            // Create Bitmap
            using (var sprite = new Bitmap(Math.Max(1, canvasWidth), Math.Max(1, canvasHeight)))
            using (var g = Graphics.FromImage(sprite))
            {
                g.Clear(Color.Transparent);

                foreach (var entry in packedFrames)
                {
                    var frameInfo = frames.FirstOrDefault(f => f.Direction == entry.Direction && f.Index == entry.Index);
                    if (frameInfo != null)
                    {
                        g.DrawImage(frameInfo.Image, entry.Frame.X, entry.Frame.Y, entry.Frame.W, entry.Frame.H);
                    }
                }

                string imageFile = Path.Combine(outputDir, baseName + ".png");
                sprite.Save(imageFile, ImageFormat.Png);

                // JSON
                var outObj = new PackedOutput
                {
                    Meta = new PackedMeta
                    {
                        Image = Path.GetFileName(imageFile),
                        Size = new SizeStruct { W = sprite.Width, H = sprite.Height },
                        Format = "RGBA8888"
                    },
                    Frames = packedFrames
                };

                string jsonFile = Path.Combine(outputDir, baseName + ".json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(outObj, options);
                File.WriteAllText(jsonFile, json);
            }
        }

        #endregion

        #region Frames + XML Export

        public static void ExportToFramesXml(List<FrameInfo> frames, string outputDir, string baseName)
        {
            if (frames == null || frames.Count == 0) return;

            string framesDir = Path.Combine(outputDir, baseName + "_frames");
            if (!Directory.Exists(framesDir)) Directory.CreateDirectory(framesDir);

            var doc = new XDocument(new XElement("Animation"));
            var root = doc.Root;

            foreach (var frame in frames)
            {
                string fileName = $"{frame.Direction}_{frame.Index:D2}.png";
                string fullPath = Path.Combine(framesDir, fileName);
                frame.Image.Save(fullPath, ImageFormat.Png);

                root.Add(new XElement("Frame",
                    new XAttribute("dir", frame.Direction),
                    new XAttribute("index", frame.Index),
                    new XAttribute("centerX", frame.Center.X),
                    new XAttribute("centerY", frame.Center.Y),
                    new XAttribute("file", fileName)
                ));
            }

            doc.Save(Path.Combine(outputDir, baseName + ".xml"));
        }

        #endregion

        #region Import Logic

        public static List<FrameInfo> ImportFromSpritesheet(string jsonFile)
        {
            if (!File.Exists(jsonFile)) throw new FileNotFoundException("JSON file not found", jsonFile);

            string json = File.ReadAllText(jsonFile);
            var packedData = JsonSerializer.Deserialize<PackedOutput>(json);
            if (packedData == null) throw new Exception("Invalid JSON format");

            string imagePath = Path.Combine(Path.GetDirectoryName(jsonFile), packedData.Meta.Image);
            if (!File.Exists(imagePath)) throw new FileNotFoundException("Sprite image not found", imagePath);

            var frames = new List<FrameInfo>();

            using (var sprite = new Bitmap(imagePath))
            {
                foreach (var entry in packedData.Frames)
                {
                    var rect = new Rectangle(entry.Frame.X, entry.Frame.Y, entry.Frame.W, entry.Frame.H);
                    var bmp = sprite.Clone(rect, PixelFormat.Format32bppArgb);

                    frames.Add(new FrameInfo
                    {
                        Image = bmp,
                        Center = new Point(entry.Center.X, entry.Center.Y),
                        Direction = entry.Direction,
                        Index = entry.Index
                    });
                }
            }

            return frames;
        }

        public static List<FrameInfo> ImportFromFramesXml(string xmlFile)
        {
            if (!File.Exists(xmlFile)) throw new FileNotFoundException("XML file not found", xmlFile);

            var doc = XDocument.Load(xmlFile);
            string framesDir = Path.Combine(Path.GetDirectoryName(xmlFile), Path.GetFileNameWithoutExtension(xmlFile) + "_frames");
            // Or assume images are relative to xml
            
            var frames = new List<FrameInfo>();

            foreach (var elem in doc.Root.Elements("Frame"))
            {
                int dir = int.Parse(elem.Attribute("dir").Value);
                int index = int.Parse(elem.Attribute("index").Value);
                int cx = int.Parse(elem.Attribute("centerX").Value);
                int cy = int.Parse(elem.Attribute("centerY").Value);
                string fileName = elem.Attribute("file").Value;

                string fullPath = Path.Combine(Path.GetDirectoryName(xmlFile), fileName);
                // Fallback check if it was in subdirectory
                if (!File.Exists(fullPath))
                {
                    fullPath = Path.Combine(framesDir, fileName);
                }

                if (File.Exists(fullPath))
                {
                    var bmp = new Bitmap(fullPath);
                    frames.Add(new FrameInfo
                    {
                        Image = bmp,
                        Center = new Point(cx, cy),
                        Direction = dir,
                        Index = index
                    });
                }
            }

            return frames;
        }

        #endregion
    }
}

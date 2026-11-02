/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * Nikodemus (UoFiddlerPixel) wrote this file.
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UoFiddler.Plugin.Enhanced.EcUltima.Art;

namespace UoFiddler.Plugin.Enhanced.EcUltima.Animations
{
    /// <summary>One classic frame: BGRA image and its center (the foot point, like UOFiddler's AnimationFrame.Center).</summary>
    public sealed class ClassicAnimFrame
    {
        public byte[] Bgra;
        public int Width;
        public int Height;
        public int CenterX;
        public int CenterY;
    }

    /// <summary>Layout of a sprite sheet: one row per stored direction, one column per frame.</summary>
    public sealed class SpriteSheetInfo
    {
        public int Columns;
        public int CellWidth;
        public int CellHeight;

        /// <summary>Position of the tile center (foot point) inside each cell.</summary>
        public int AnchorX;
        public int AnchorY;

        public uint Body;
        public int Action;

        public string ToText() => string.Format(CultureInfo.InvariantCulture,
            "AMOU sprite sheet\r\ncolumns={0}\r\ncell={1}x{2}\r\nanchor={3},{4}\r\nbody={5}\r\naction={6}\r\nrows=5 stored directions (0 = down, 1 = down left, 2 = left, 3 = up left, 4 = up)\r\n",
            Columns, CellWidth, CellHeight, AnchorX, AnchorY, Body, Action);

        public static SpriteSheetInfo FromText(string text)
        {
            var info = new SpriteSheetInfo();
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                int eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }
                string key = line.Substring(0, eq).Trim(), value = line.Substring(eq + 1).Trim();
                string[] two = value.Split('x', ',');
                switch (key)
                {
                    case "columns": info.Columns = int.Parse(value, CultureInfo.InvariantCulture); break;
                    case "cell": info.CellWidth = int.Parse(two[0], CultureInfo.InvariantCulture); info.CellHeight = int.Parse(two[1], CultureInfo.InvariantCulture); break;
                    case "anchor": info.AnchorX = int.Parse(two[0], CultureInfo.InvariantCulture); info.AnchorY = int.Parse(two[1], CultureInfo.InvariantCulture); break;
                    case "body": info.Body = uint.Parse(value, CultureInfo.InvariantCulture); break;
                    case "action": info.Action = int.Parse(value, CultureInfo.InvariantCulture); break;
                }
            }
            if (info.Columns <= 0 || info.CellWidth <= 0 || info.CellHeight <= 0)
            {
                throw new InvalidDataException("The sheet description is incomplete (columns, cell, anchor).");
            }
            return info;
        }
    }

    /// <summary>
    /// Classic animations → EC (AMOU) and sprite sheets of EC animations.
    /// The 5 stored directions are the same in both clients (0 = ↓, 1 = ↙, 2 = ←, 3 = ↖, 4 = ↑; the others mirrored).
    /// Frame numbers fill a grid: number = direction × frames per direction + frame + 1.
    /// </summary>
    public static class EcAnimationTools
    {
        public const int Directions = 5;

        /// <summary>
        /// Builds an EC animation from classic frames (index = stored direction). <paramref name="scale"/> null keeps the
        /// classic size, otherwise the frames are enlarged 1.5 times like the EC art.
        /// </summary>
        public static EcAnimation FromClassic(IList<List<ClassicAnimFrame>> directions, uint body, ushort action, ArtScaleMode? scale)
        {
            int perDirection = directions.Max(d => d?.Count ?? 0);
            if (perDirection == 0)
            {
                throw new InvalidOperationException("The classic animation has no frames.");
            }
            double factor = scale.HasValue ? EcArtScaler.EcScale : 1.0;
            var prepared = new List<(int Dir, int Index, byte[] Bgra, int W, int H, short X1, short Y1)>();
            for (int dir = 0; dir < Math.Min(Directions, directions.Count); dir++)
            {
                List<ClassicAnimFrame> frames = directions[dir];
                if (frames == null)
                {
                    continue;
                }
                for (int i = 0; i < frames.Count; i++)
                {
                    ClassicAnimFrame f = frames[i];
                    if (f == null || f.Width <= 0 || f.Height <= 0)
                    {
                        continue;
                    }
                    byte[] px = f.Bgra;
                    int w = f.Width, h = f.Height;
                    if (scale.HasValue)
                    {
                        px = EcArtScaler.Scale(px, w, h, scale.Value, out w, out h);
                    }
                    // classic: drawn at foot - center.X, foot - center.Y - height
                    short x1 = (short)Math.Round(-f.CenterX * factor);
                    short y1 = (short)Math.Round((-f.CenterY - f.Height) * factor);
                    prepared.Add((dir, i, px, w, h, x1, y1));
                }
            }

            var animation = new EcAnimation { BodyId = body };
            animation.BuildPalette(prepared.Select(p => p.Bgra));
            foreach (var p in prepared.OrderBy(p => p.Dir).ThenBy(p => p.Index))
            {
                var frame = new EcAnimationFrame { ActionId = action, Number = (ushort)((p.Dir * perDirection) + p.Index + 1) };
                animation.SetFrameImage(frame, p.Bgra, p.W, p.H, p.X1, p.Y1);
                animation.Frames.Add(frame);
            }
            animation.UpdateBounds();
            return animation;
        }

        /// <summary>A sprite sheet (BGRA) of an EC animation: rows = stored directions, columns = frames.</summary>
        public static byte[] ToSheet(EcAnimation animation, out int width, out int height, out SpriteSheetInfo info)
        {
            int perDirection = animation.SuggestedFramesPerDirection();
            List<EcAnimationFrame> frames = animation.Frames.Where(f => !f.IsEmpty).ToList();
            int minX = frames.Count == 0 ? 0 : frames.Min(f => f.X1), minY = frames.Count == 0 ? 0 : frames.Min(f => f.Y1);
            int maxX = frames.Count == 0 ? 1 : frames.Max(f => f.X2), maxY = frames.Count == 0 ? 1 : frames.Max(f => f.Y2);
            info = new SpriteSheetInfo
            {
                Columns = perDirection,
                CellWidth = maxX - minX + 2,
                CellHeight = maxY - minY + 2,
                AnchorX = -minX + 1,
                AnchorY = -minY + 1,
                Body = animation.BodyId,
                Action = animation.ActionId
            };
            width = info.CellWidth * perDirection;
            height = info.CellHeight * Directions;
            var sheet = new byte[width * height * 4];
            foreach (EcAnimationFrame f in frames)
            {
                int dir = (f.Number - 1) / perDirection, col = (f.Number - 1) % perDirection;
                if (dir >= Directions)
                {
                    continue;
                }
                byte[] px = animation.ToBgra(f);
                int ox = (col * info.CellWidth) + info.AnchorX + f.X1, oy = (dir * info.CellHeight) + info.AnchorY + f.Y1;
                for (int y = 0; y < f.Height; y++)
                {
                    Buffer.BlockCopy(px, y * f.Width * 4, sheet, (((oy + y) * width) + ox) * 4, f.Width * 4);
                }
            }
            return sheet;
        }

        /// <summary>
        /// Reads a sprite sheet back into an animation (new palette); empty cells give no frame.
        /// </summary>
        public static EcAnimation FromSheet(byte[] sheet, int width, int height, SpriteSheetInfo info, uint body, ushort action)
        {
            var cells = new List<(int Number, byte[] Bgra, int W, int H, short X1, short Y1)>();
            for (int dir = 0; dir < Directions; dir++)
            {
                for (int col = 0; col < info.Columns; col++)
                {
                    int cx = col * info.CellWidth, cy = dir * info.CellHeight;
                    if (cx + info.CellWidth > width || cy + info.CellHeight > height)
                    {
                        continue;
                    }
                    var cell = new byte[info.CellWidth * info.CellHeight * 4];
                    for (int y = 0; y < info.CellHeight; y++)
                    {
                        Buffer.BlockCopy(sheet, (((cy + y) * width) + cx) * 4, cell, y * info.CellWidth * 4, info.CellWidth * 4);
                    }
                    (int ux, int uy, int uw, int uh) = EcArtScaler.UsedArea(cell, info.CellWidth, info.CellHeight);
                    if (uw == 0)
                    {
                        continue;
                    }
                    var px = new byte[uw * uh * 4];
                    for (int y = 0; y < uh; y++)
                    {
                        Buffer.BlockCopy(cell, (((uy + y) * info.CellWidth) + ux) * 4, px, y * uw * 4, uw * 4);
                    }
                    cells.Add(((dir * info.Columns) + col + 1, px, uw, uh, (short)(ux - info.AnchorX), (short)(uy - info.AnchorY)));
                }
            }
            if (cells.Count == 0)
            {
                throw new InvalidOperationException("The sheet has no frames.");
            }
            var animation = new EcAnimation { BodyId = body };
            animation.BuildPalette(cells.Select(c => c.Bgra));
            foreach (var c in cells)
            {
                var frame = new EcAnimationFrame { ActionId = action, Number = (ushort)c.Number };
                animation.SetFrameImage(frame, c.Bgra, c.W, c.H, c.X1, c.Y1);
                animation.Frames.Add(frame);
            }
            animation.UpdateBounds();
            return animation;
        }
    }
}

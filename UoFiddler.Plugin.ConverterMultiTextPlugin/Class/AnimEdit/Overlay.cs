using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit
{
    internal enum OverlayTint
    {
        Original,
        Gray,
        Color
    }

    /// <summary>
    /// A floating image above the frames (e.g. a cage around a bird). It is NOT burned into the frames:
    /// it can be moved, scaled, tinted, copied and shown on all / some frames until it is "baked".
    /// Pure black and white stay inside the overlay image and are shown transparent (like the UO client)
    /// when "B/W transparent" is on. A GIF gives an animated overlay: editor frame i uses image i mod n.
    /// </summary>
    internal sealed class Overlay
    {
        public string Name = "Overlay";

        /// <summary>Source images. Shared between clones (undo history) and never changed.</summary>
        public List<Bitmap> Sources = new List<Bitmap>();

        public Point Position;
        public int ScalePercent = 100;
        public OverlayTint Tint = OverlayTint.Original;
        public Color TintColor = Color.FromArgb(128, 128, 128);
        public bool Visible = true;
        public float Opacity = 1f;

        /// <summary>Frames the overlay is shown on. null = all frames.</summary>
        public HashSet<int> Frames;

        private readonly Dictionary<int, Bitmap> cache = new Dictionary<int, Bitmap>();

        public Overlay Clone()
        {
            return new Overlay
            {
                Name = Name,
                Sources = Sources,
                Position = Position,
                ScalePercent = ScalePercent,
                Tint = Tint,
                TintColor = TintColor,
                Visible = Visible,
                Opacity = Opacity,
                Frames = Frames != null ? new HashSet<int>(Frames) : null
            };
        }

        public bool AppliesTo(int frame) => Visible && Sources.Count > 0 && (Frames == null || Frames.Contains(frame));

        private int SourceIndex(int frame) => Sources.Count == 0 ? 0 : frame % Sources.Count;

        public Size SizeFor(int frame)
        {
            if (Sources.Count == 0) return Size.Empty;
            Bitmap s = Sources[SourceIndex(frame)];
            return new Size(Math.Max(1, s.Width * ScalePercent / 100), Math.Max(1, s.Height * ScalePercent / 100));
        }

        public Rectangle BoundsFor(int frame) => new Rectangle(Position, SizeFor(frame));

        /// <summary>Scaled (nearest neighbour) and tinted image for a frame. Cached; do not dispose.</summary>
        public Bitmap RenderedFor(int frame)
        {
            int idx = SourceIndex(frame);
            if (cache.TryGetValue(idx, out Bitmap b)) return b;

            Size size = SizeFor(frame);
            Bitmap scaled = ImageUtil.ScaleNearest(Sources[idx], size.Width, size.Height);
            if (Tint != OverlayTint.Original)
                OverlayUtil.ApplyTint(scaled, Tint == OverlayTint.Gray ? (Color?)null : TintColor);
            cache[idx] = scaled;
            return scaled;
        }

        /// <summary>Call after changing scale or tint.</summary>
        public void Invalidate()
        {
            foreach (Bitmap b in cache.Values) b.Dispose();
            cache.Clear();
        }

        public string Describe(int frame)
        {
            Size s = SizeFor(frame);
            string scope = Frames == null ? "all" : Frames.Count == 1 ? "F" + (Frames.First() + 1) : Frames.Count + " frames";
            string anim = Sources.Count > 1 ? $", {Sources.Count} imgs" : "";
            return $"{(Visible ? "" : "(hidden) ")}{Name}  {s.Width}x{s.Height}  [{scope}{anim}]";
        }
    }

    internal static class OverlayUtil
    {
        private static bool IsKey(int argb)
        {
            int rgb = argb & 0xFFFFFF;
            return rgb == 0x000000 || rgb == 0xFFFFFF || rgb == 0xD3D3D3;
        }

        /// <summary>
        /// Tints an image. color null = gray (like UO "particle gray"), otherwise a hue-like ramp
        /// dark → color → light based on brightness. Transparent pixels and the key colors
        /// (pure black, white, #D3D3D3) stay unchanged, so they remain transparent in UO.
        /// </summary>
        public static void ApplyTint(Bitmap bmp, Color? color)
        {
            var pb = new PixelBuffer(bmp);
            for (int i = 0; i < pb.Pixels.Length; i++)
            {
                int p = pb.Pixels[i];
                int a = (int)((uint)p >> 24);
                if (a == 0 || IsKey(p)) continue;

                int r = (p >> 16) & 255, g = (p >> 8) & 255, b = p & 255;
                int lum = (r * 299 + g * 587 + b * 114) / 1000;
                int nr, ng, nb;
                if (color == null)
                {
                    nr = ng = nb = lum;
                }
                else
                {
                    Color c = color.Value;
                    if (lum < 128)
                    {
                        nr = c.R * lum / 128; ng = c.G * lum / 128; nb = c.B * lum / 128;
                    }
                    else
                    {
                        int t = lum - 128;
                        nr = c.R + (255 - c.R) * t / 127; ng = c.G + (255 - c.G) * t / 127; nb = c.B + (255 - c.B) * t / 127;
                    }
                }
                // never produce a key color by accident (it would become transparent)
                int rgb = (nr << 16) | (ng << 8) | nb;
                if (rgb == 0x000000) nb = 1;
                else if (rgb == 0xFFFFFF) nb = 254;
                else if (rgb == 0xD3D3D3) nb = 0xD2;
                pb.Pixels[i] = (a << 24) | (nr << 16) | (ng << 8) | nb;
            }
            pb.WriteTo(bmp);
        }

        /// <summary>
        /// Cuts away the border made of transparent, pure black, pure white or #D3D3D3 pixels
        /// (the background of most UO art exports). Returns a new bitmap (copy if nothing to cut).
        /// </summary>
        public static Bitmap AutoCrop(Bitmap src)
        {
            var pb = new PixelBuffer(src);
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            for (int y = 0; y < pb.Height; y++)
            {
                for (int x = 0; x < pb.Width; x++)
                {
                    int p = pb.Pixels[y * pb.Width + x];
                    if (((uint)p >> 24) == 0 || IsKey(p)) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            if (maxX < 0) return ImageUtil.To32(src);
            using (Bitmap full = ImageUtil.To32(src))
                return full.Clone(Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
    }
}

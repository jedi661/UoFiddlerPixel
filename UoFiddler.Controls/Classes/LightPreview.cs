/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Preview of a light like in the game: land tiles as ground, the light
 * source item in the middle, the scene darkened, the light brightens it.
 * Optional a guide image (e.g. a window) on top.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    public sealed class LightPreviewOptions
    {
        public int LandId { get; set; } = 0x3;
        public int ItemId { get; set; } = 0x0B20;

        /// <summary>darkness of the scene without light, 0..100 %</summary>
        public int Darkness { get; set; } = 70;

        /// <summary>position of the light relative to the middle of the item</summary>
        public int LightOffsetX { get; set; }
        public int LightOffsetY { get; set; } = 30;

        /// <summary>guide image (not part of the light), drawn on top</summary>
        public Bitmap Guide { get; set; }
        public int GuideX { get; set; }
        public int GuideY { get; set; }
        public int GuideScale { get; set; } = 100;
        public int GuideOpacity { get; set; } = 60;
    }

    public static class LightPreview
    {
        /// <summary>the light alone: grey scale on black, optional with the guide image</summary>
        public static Bitmap RenderLight(LightImage light, LightPreviewOptions options)
        {
            Bitmap bmp = Light.ToBitmap(light.Data, light.Width, light.Height);
            if (options?.Guide != null)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    DrawGuide(g, options, bmp.Width / 2, bmp.Height / 2);
                }
            }

            return bmp;
        }

        /// <summary>the light in a scene like in the game</summary>
        public static unsafe Bitmap RenderScene(Size size, LightImage light, LightPreviewOptions options)
        {
            int width = Math.Max(1, size.Width), height = Math.Max(1, size.Height);
            var bit = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            int itemCenterX = width / 2, itemCenterY = height / 2;
            using (Graphics g = Graphics.FromImage(bit))
            {
                g.Clear(Color.Black);
                Bitmap background = Art.IsValidLand(options.LandId) ? Art.GetLand(options.LandId) : null;
                if (background != null)
                {
                    // diamond pattern of land tiles (44 x 44, rows every 22 pixels)
                    int row = 0;
                    for (int y = -22; y <= height; y += 22, row++)
                    {
                        for (int x = row % 2 == 0 ? 0 : -22; x <= width; x += 44)
                        {
                            g.DrawImage(background, x, y);
                        }
                    }
                }

                Bitmap item = Art.IsValidStatic(options.ItemId) ? Art.GetStatic(options.ItemId) : null;
                if (item != null)
                {
                    g.DrawImage(item, itemCenterX - (item.Width / 2), itemCenterY - (item.Height / 2));
                }
            }

            double ambient = 1 - (Math.Max(0, Math.Min(100, options.Darkness)) / 100.0);
            int lightX = itemCenterX + options.LightOffsetX - (light?.Width ?? 0) / 2;
            int lightY = itemCenterY + options.LightOffsetY - (light?.Height ?? 0) / 2;

            BitmapData bd = bit.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    uint* line = (uint*)((byte*)bd.Scan0 + (y * bd.Stride));
                    for (int x = 0; x < width; x++)
                    {
                        double v = 0;
                        int lx = x - lightX, ly = y - lightY;
                        if (light != null && lx >= 0 && ly >= 0 && lx < light.Width && ly < light.Height)
                        {
                            v = Light.DecodeValue(light[lx, ly]) / 31.0;
                        }

                        // the light brightens the darkened scene (a bit more than the original, like the client)
                        double factor = ambient + ((1 - ambient) * 1.35 * v);
                        uint c = line[x];
                        int r = Math.Min(255, (int)(((c >> 16) & 0xFF) * factor));
                        int gr = Math.Min(255, (int)(((c >> 8) & 0xFF) * factor));
                        int b = Math.Min(255, (int)((c & 0xFF) * factor));
                        line[x] = 0xFF000000 | ((uint)r << 16) | ((uint)gr << 8) | (uint)b;
                    }
                }
            }
            finally
            {
                bit.UnlockBits(bd);
            }

            if (options.Guide != null)
            {
                using (Graphics g = Graphics.FromImage(bit))
                {
                    DrawGuide(g, options, lightX + ((light?.Width ?? 0) / 2), lightY + ((light?.Height ?? 0) / 2));
                }
            }

            return bit;
        }

        /// <summary>guide image centered at (cx, cy) + its offset, semi transparent</summary>
        public static void DrawGuide(Graphics g, LightPreviewOptions o, int cx, int cy)
        {
            Bitmap guide = o.Guide;
            int w = Math.Max(1, guide.Width * o.GuideScale / 100), h = Math.Max(1, guide.Height * o.GuideScale / 100);
            var dest = new Rectangle(cx + o.GuideX - (w / 2), cy + o.GuideY - (h / 2), w, h);
            using (var attributes = new ImageAttributes())
            {
                var matrix = new ColorMatrix { Matrix33 = Math.Max(0, Math.Min(100, o.GuideOpacity)) / 100f };
                attributes.SetColorMatrix(matrix);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(guide, dest, 0, 0, guide.Width, guide.Height, GraphicsUnit.Pixel, attributes);
            }

            using (var pen = new Pen(Color.FromArgb(120, Color.Orange)) { DashStyle = DashStyle.Dot })
            {
                g.DrawRectangle(pen, dest);
            }
        }
    }
}

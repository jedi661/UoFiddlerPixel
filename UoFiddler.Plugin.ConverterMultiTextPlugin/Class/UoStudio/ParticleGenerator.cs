/***************************************************************************
 *
 * $Author: Nikodemus
 *
 * "THE WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>Kinds of generated particle effects.</summary>
    public enum ParticleType
    {
        Sparks = 0,
        Fire = 1,
        Smoke = 2,
        Lightning = 3,
        MagicRing = 4,
        Explosion = 5,
        Healing = 6
    }

    /// <summary>
    /// Generates effect frame sequences (on black background, for additive UO effects):
    /// sparks, fire, smoke, lightning, magic ring, explosion and healing sparkles.
    /// </summary>
    public static class ParticleGenerator
    {
        public static readonly string[] TypeNames =
        {
            "Funken", "Feuer", "Rauch", "Blitz", "Magie-Ring", "Explosion", "Heilung (aufsteigend)"
        };

        /// <summary>Typical colour per type (used as default in the UI).</summary>
        public static Color DefaultColor(ParticleType type) => type switch
        {
            ParticleType.Sparks => Color.FromArgb(255, 200, 80),
            ParticleType.Fire => Color.FromArgb(255, 110, 20),
            ParticleType.Smoke => Color.FromArgb(150, 150, 150),
            ParticleType.Lightning => Color.FromArgb(120, 170, 255),
            ParticleType.MagicRing => Color.FromArgb(170, 90, 255),
            ParticleType.Explosion => Color.FromArgb(255, 140, 40),
            ParticleType.Healing => Color.FromArgb(90, 255, 140),
            _ => Color.White
        };

        private sealed class Particle
        {
            public double X, Y, Vx, Vy, Life, MaxLife, Size, Seed;
        }

        /// <summary>Generates the frames (size x size pixels each).</summary>
        public static List<Bitmap> Generate(ParticleType type, int frames, int size, int count, Color color, int seed)
        {
            var rnd = new Random(seed);
            var result = new List<Bitmap>();
            frames = Math.Max(1, frames);
            size = Math.Max(8, size);
            count = Math.Max(1, count);
            List<Particle> particles = CreateParticles(type, count, size, rnd);

            for (int f = 0; f < frames; f++)
            {
                double t = frames == 1 ? 0.5 : f / (double)(frames - 1); // 0..1 progress
                var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    switch (type)
                    {
                        case ParticleType.Lightning:
                            DrawLightning(g, size, color, new Random(seed + (f * 7919)), t);
                            break;
                        case ParticleType.MagicRing:
                            DrawRing(g, particles, size, color, t);
                            break;
                        case ParticleType.Explosion:
                            DrawExplosion(g, particles, size, color, t);
                            break;
                        default:
                            DrawParticles(g, type, particles, size, color, t, frames);
                            break;
                    }
                }
                result.Add(bmp);
            }
            return result;
        }

        private static List<Particle> CreateParticles(ParticleType type, int count, int size, Random rnd)
        {
            var list = new List<Particle>();
            double c = size / 2.0;
            for (int i = 0; i < count; i++)
            {
                var p = new Particle { Seed = rnd.NextDouble() };
                double ang = rnd.NextDouble() * Math.PI * 2;
                switch (type)
                {
                    case ParticleType.Sparks:
                        double sp = (0.25 + (rnd.NextDouble() * 0.75)) * size * 0.55;
                        p.X = c;
                        p.Y = c;
                        p.Vx = Math.Cos(ang) * sp;
                        p.Vy = (Math.Sin(ang) * sp * 0.6) - (size * 0.15);
                        p.Size = 1 + (rnd.NextDouble() * 1.5);
                        p.Life = rnd.NextDouble() * 0.3;           // start delay
                        p.MaxLife = 0.5 + (rnd.NextDouble() * 0.5);
                        break;
                    case ParticleType.Fire:
                        p.X = c + ((rnd.NextDouble() - 0.5) * size * 0.35);
                        p.Y = size * 0.92;
                        p.Vx = (rnd.NextDouble() - 0.5) * size * 0.1;
                        p.Vy = -(0.4 + (rnd.NextDouble() * 0.6)) * size * 0.8;
                        p.Size = size * (0.06 + (rnd.NextDouble() * 0.08));
                        p.Life = rnd.NextDouble();                 // phase (loops)
                        p.MaxLife = 0.6 + (rnd.NextDouble() * 0.4);
                        break;
                    case ParticleType.Smoke:
                        p.X = c + ((rnd.NextDouble() - 0.5) * size * 0.3);
                        p.Y = size * 0.9;
                        p.Vx = (rnd.NextDouble() - 0.5) * size * 0.15;
                        p.Vy = -(0.3 + (rnd.NextDouble() * 0.4)) * size * 0.7;
                        p.Size = size * (0.08 + (rnd.NextDouble() * 0.08));
                        p.Life = rnd.NextDouble();
                        p.MaxLife = 1;
                        break;
                    case ParticleType.MagicRing:
                        p.X = ang;                                 // angle on the ring
                        p.Y = 0;
                        p.Size = 1.5 + (rnd.NextDouble() * 2);
                        p.Life = rnd.NextDouble() * 0.15;
                        break;
                    case ParticleType.Explosion:
                        double es = (0.3 + (rnd.NextDouble() * 0.7)) * size * 0.5;
                        p.X = c;
                        p.Y = c;
                        p.Vx = Math.Cos(ang) * es;
                        p.Vy = Math.Sin(ang) * es * 0.7;
                        p.Size = size * (0.03 + (rnd.NextDouble() * 0.06));
                        break;
                    case ParticleType.Healing:
                        p.X = c + ((rnd.NextDouble() - 0.5) * size * 0.5);
                        p.Y = size * (0.6 + (rnd.NextDouble() * 0.35));
                        p.Vx = (rnd.NextDouble() - 0.5) * size * 0.04;
                        p.Vy = -(0.3 + (rnd.NextDouble() * 0.4)) * size * 0.5;
                        p.Size = 1 + (rnd.NextDouble() * 2.5);
                        p.Life = rnd.NextDouble();
                        p.MaxLife = 1;
                        break;
                }
                list.Add(p);
            }
            return list;
        }

        private static Color Mix(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            return Color.FromArgb((int)(a.R + ((b.R - a.R) * t)), (int)(a.G + ((b.G - a.G) * t)), (int)(a.B + ((b.B - a.B) * t)));
        }

        private static Color Fade(Color c, double brightness)
        {
            brightness = Math.Max(0, Math.Min(1, brightness));
            return Color.FromArgb((int)(c.R * brightness), (int)(c.G * brightness), (int)(c.B * brightness));
        }

        /// <summary>Soft round blob (radial gradient from colour to black = transparent later).</summary>
        private static void Blob(Graphics g, double x, double y, double r, Color c)
        {
            if (r < 0.6)
            {
                return;
            }
            using var path = new GraphicsPath();
            path.AddEllipse((float)(x - r), (float)(y - r), (float)(r * 2), (float)(r * 2));
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = c,
                SurroundColors = new[] { Color.FromArgb(0, c) }
            };
            g.FillPath(brush, path);
        }

        private static void DrawParticles(Graphics g, ParticleType type, List<Particle> particles, int size, Color color, double t, int frames)
        {
            foreach (Particle p in particles)
            {
                switch (type)
                {
                    case ParticleType.Sparks:
                    {
                        double age = (t - p.Life) / p.MaxLife;
                        if (age < 0 || age > 1)
                        {
                            continue;
                        }
                        double x = p.X + (p.Vx * age);
                        double y = p.Y + (p.Vy * age) + (0.5 * size * 0.9 * age * age); // gravity
                        double tx = x - (p.Vx * 0.06), ty = y - ((p.Vy + (size * 0.9 * age)) * 0.06);
                        Color c = Fade(Mix(Color.White, color, age * 1.5), 1 - (age * 0.8));
                        using var pen = new Pen(c, (float)p.Size) { StartCap = LineCap.Round, EndCap = LineCap.Round };
                        g.DrawLine(pen, (float)tx, (float)ty, (float)x, (float)y);
                        break;
                    }
                    case ParticleType.Fire:
                    {
                        // looping: every particle has its own phase -> seamless loop over the frames
                        double age = (p.Life + t) % 1.0;
                        double x = p.X + (p.Vx * age) + (Math.Sin((age * 9) + (p.Seed * 6)) * size * 0.02);
                        double y = p.Y + (p.Vy * age);
                        double r = p.Size * (1 - (age * 0.7));
                        Color c = age < 0.25 ? Mix(Color.FromArgb(255, 250, 200), Color.FromArgb(255, 220, 80), age * 4)
                                             : Mix(Color.FromArgb(255, 220, 80), color, (age - 0.25) * 1.6);
                        Blob(g, x, y, r, Fade(c, 1 - (age * 0.9)));
                        break;
                    }
                    case ParticleType.Smoke:
                    {
                        double age = (p.Life + t) % 1.0;
                        double x = p.X + (p.Vx * age);
                        double y = p.Y + (p.Vy * age);
                        double r = p.Size * (1 + (age * 1.5));
                        Blob(g, x, y, r, Fade(color, 0.55 * Math.Sin(age * Math.PI)));
                        break;
                    }
                    case ParticleType.Healing:
                    {
                        double age = (p.Life + t) % 1.0;
                        double x = p.X + (p.Vx * age) + (Math.Sin((age * 12) + (p.Seed * 10)) * 2);
                        double y = p.Y + (p.Vy * age);
                        double twinkle = 0.5 + (0.5 * Math.Sin((age * 25) + (p.Seed * 20)));
                        Color c = Fade(Mix(color, Color.White, twinkle * 0.6), Math.Sin(age * Math.PI));
                        Blob(g, x, y, p.Size * 2.2, Fade(c, 0.6));
                        using var b = new SolidBrush(c);
                        g.FillEllipse(b, (float)(x - (p.Size / 2)), (float)(y - (p.Size / 2)), (float)p.Size, (float)p.Size);
                        break;
                    }
                }
            }
        }

        private static void DrawRing(Graphics g, List<Particle> particles, int size, Color color, double t)
        {
            double c = size / 2.0;
            double radius = size * 0.08 + (t * size * 0.4);
            double brightness = t < 0.2 ? t / 0.2 : 1 - ((t - 0.2) / 0.8);
            // glowing ring (flattened like the UO floor)
            using (var pen = new Pen(Fade(color, brightness * 0.8), Math.Max(1f, size / 40f)))
            {
                g.DrawEllipse(pen, (float)(c - radius), (float)(c - (radius * 0.5)), (float)(radius * 2), (float)radius);
            }
            foreach (Particle p in particles)
            {
                double a = p.X + (t * 1.5);
                double r = radius * (0.9 + (p.Seed * 0.2));
                double x = c + (Math.Cos(a) * r);
                double y = c + (Math.Sin(a) * r * 0.5) - (t * size * 0.15 * p.Seed);
                Blob(g, x, y, p.Size * 2, Fade(Mix(color, Color.White, 0.4), brightness));
            }
        }

        private static void DrawExplosion(Graphics g, List<Particle> particles, int size, Color color, double t)
        {
            double c = size / 2.0;
            // flash
            if (t < 0.35)
            {
                double f = 1 - (t / 0.35);
                Blob(g, c, c, size * (0.15 + (t * 0.8)), Fade(Color.FromArgb(255, 250, 220), f));
            }
            // fire ball
            Blob(g, c, c, size * (0.12 + (t * 0.25)), Fade(color, Math.Max(0, 1 - (t * 1.4))));
            // debris / sparks
            foreach (Particle p in particles)
            {
                double ease = 1 - Math.Pow(1 - t, 2);
                double x = p.X + (p.Vx * ease);
                double y = p.Y + (p.Vy * ease) + (size * 0.2 * t * t);
                Color col = Fade(Mix(Color.FromArgb(255, 240, 180), color, t * 1.3), 1 - t);
                Blob(g, x, y, p.Size * (1 - (t * 0.5)), col);
            }
            // smoke at the end
            if (t > 0.4)
            {
                double s = (t - 0.4) / 0.6;
                Blob(g, c, c - (s * size * 0.1), size * (0.2 + (s * 0.2)), Fade(Color.FromArgb(90, 90, 90), 0.5 * Math.Sin(s * Math.PI)));
            }
        }

        private static void DrawLightning(Graphics g, int size, Color color, Random rnd, double t)
        {
            // flicker: strong at start, weaker later
            double strength = 0.6 + (0.4 * rnd.NextDouble());
            var points = new List<PointF>();
            double x = (size / 2.0) + ((rnd.NextDouble() - 0.5) * size * 0.2);
            int steps = 12;
            for (int i = 0; i <= steps; i++)
            {
                double y = i * (size - 4) / (double)steps + 2;
                points.Add(new PointF((float)x, (float)y));
                x += (rnd.NextDouble() - 0.5) * size * 0.18;
                x = Math.Max(4, Math.Min(size - 4, x));
            }
            DrawBolt(g, points.ToArray(), color, size, strength);

            // branches
            int branches = 1 + rnd.Next(3);
            for (int b = 0; b < branches; b++)
            {
                int start = 2 + rnd.Next(steps - 4);
                var bp = new List<PointF> { points[start] };
                double bx = points[start].X, by = points[start].Y;
                double dir = rnd.NextDouble() < 0.5 ? -1 : 1;
                for (int i = 0; i < 4; i++)
                {
                    bx += dir * size * (0.04 + (rnd.NextDouble() * 0.06));
                    by += size * (0.04 + (rnd.NextDouble() * 0.05));
                    bp.Add(new PointF((float)bx, (float)by));
                }
                DrawBolt(g, bp.ToArray(), color, size * 0.6, strength * 0.7);
            }
        }

        private static void DrawBolt(Graphics g, PointF[] pts, Color color, double size, double strength)
        {
            using (var glow = new Pen(Fade(color, 0.35 * strength), (float)Math.Max(3, size / 14)) { LineJoin = LineJoin.Round })
            {
                g.DrawLines(glow, pts);
            }
            using (var mid = new Pen(Fade(Mix(color, Color.White, 0.4), 0.8 * strength), (float)Math.Max(1.5, size / 40)) { LineJoin = LineJoin.Round })
            {
                g.DrawLines(mid, pts);
            }
            using var core = new Pen(Fade(Color.White, strength), 1f) { LineJoin = LineJoin.Round };
            g.DrawLines(core, pts);
        }
    }
}

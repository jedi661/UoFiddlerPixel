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
using System.IO;
using System.Text;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio
{
    /// <summary>Mono audio clip as float samples (-1..1).</summary>
    public sealed class AudioClip
    {
        public AudioClip(float[] samples, int sampleRate)
        {
            Samples = samples ?? Array.Empty<float>();
            SampleRate = sampleRate;
        }

        public float[] Samples { get; }
        public int SampleRate { get; }
        public double Duration => SampleRate == 0 ? 0 : Samples.Length / (double)SampleRate;
    }

    /// <summary>
    /// Audio helpers for the Sound-Studio: WAV reader (PCM 8/16/24/32, float), MP3 reader (NLayer),
    /// resampling, normalising, fades and the UO sound format (22050 Hz, 16 bit, mono).
    /// </summary>
    public static class StudioAudio
    {
        public const int UoSampleRate = 22050;

        #region Reading

        public static AudioClip Load(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (ext == ".mp3")
            {
                return LoadMp3(fileName);
            }
            return ReadWav(File.ReadAllBytes(fileName));
        }

        public static AudioClip LoadMp3(string fileName)
        {
            using var mp3 = new NLayer.MpegFile(fileName);
            int channels = Math.Max(1, mp3.Channels);
            var all = new List<float>();
            var buffer = new float[4096 * channels];
            int read;
            while ((read = mp3.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i + channels - 1 < read; i += channels)
                {
                    float sum = 0;
                    for (int c = 0; c < channels; c++)
                    {
                        sum += buffer[i + c];
                    }
                    all.Add(sum / channels);
                }
            }
            return new AudioClip(all.ToArray(), mp3.SampleRate);
        }

        /// <summary>Parses a RIFF/WAVE file (any PCM bit depth or 32 bit float), mixes down to mono.</summary>
        public static AudioClip ReadWav(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "RIFF")
            {
                throw new InvalidDataException("Keine WAV-Datei (RIFF fehlt).");
            }
            br.ReadInt32();
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != "WAVE")
            {
                throw new InvalidDataException("Keine WAV-Datei (WAVE fehlt).");
            }

            int format = 1, channels = 1, rate = UoSampleRate, bits = 16;
            byte[] pcm = null;
            while (ms.Position + 8 <= ms.Length)
            {
                string id = Encoding.ASCII.GetString(br.ReadBytes(4));
                int len = br.ReadInt32();
                long next = ms.Position + len + (len & 1);
                if (id == "fmt ")
                {
                    format = br.ReadInt16();
                    channels = br.ReadInt16();
                    rate = br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt16();
                    bits = br.ReadInt16();
                    if (format == unchecked((short)0xFFFE) || format == -2)
                    {
                        // WAVE_FORMAT_EXTENSIBLE: sub format in the first 2 bytes of the GUID
                        if (len >= 26)
                        {
                            br.ReadInt16();
                            br.ReadInt16();
                            br.ReadInt32();
                            format = br.ReadInt16();
                        }
                    }
                }
                else if (id == "data")
                {
                    len = (int)Math.Min(len, ms.Length - ms.Position);
                    pcm = br.ReadBytes(len);
                }
                if (next > ms.Length)
                {
                    break;
                }
                ms.Position = next;
            }

            if (pcm == null)
            {
                throw new InvalidDataException("WAV-Datei enthält keine Audiodaten.");
            }
            if (format != 1 && format != 3)
            {
                throw new InvalidDataException($"WAV-Format {format} wird nicht unterstützt (nur PCM oder Float).");
            }

            int bytesPer = bits / 8;
            int frameSize = bytesPer * channels;
            int frames = frameSize == 0 ? 0 : pcm.Length / frameSize;
            var samples = new float[frames];
            for (int f = 0; f < frames; f++)
            {
                float sum = 0;
                for (int c = 0; c < channels; c++)
                {
                    int o = (f * frameSize) + (c * bytesPer);
                    sum += ReadSample(pcm, o, bits, format == 3);
                }
                samples[f] = sum / channels;
            }
            return new AudioClip(samples, rate);
        }

        private static float ReadSample(byte[] d, int o, int bits, bool isFloat)
        {
            switch (bits)
            {
                case 8:
                    return (d[o] - 128) / 128f;
                case 16:
                    return BitConverter.ToInt16(d, o) / 32768f;
                case 24:
                    int v = d[o] | (d[o + 1] << 8) | ((sbyte)d[o + 2] << 16);
                    return v / 8388608f;
                case 32:
                    return isFloat ? BitConverter.ToSingle(d, o) : BitConverter.ToInt32(d, o) / 2147483648f;
                default:
                    return 0;
            }
        }

        #endregion

        #region Writing

        /// <summary>Writes a 16 bit mono PCM WAV.</summary>
        public static byte[] ToWav16(float[] samples, int sampleRate, float volume = 1f)
        {
            int dataLen = samples.Length * 2;
            using var ms = new MemoryStream(44 + dataLen);
            using var bw = new BinaryWriter(ms);
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataLen);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(sampleRate);
            bw.Write(sampleRate * 2);
            bw.Write((short)2);
            bw.Write((short)16);
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(dataLen);
            foreach (float s in samples)
            {
                float v = Math.Max(-1f, Math.Min(1f, s * volume));
                bw.Write((short)Math.Round(v * 32767f));
            }
            bw.Flush();
            return ms.ToArray();
        }

        /// <summary>Converts the clip into the UO sound format (22050 Hz, 16 bit, mono).</summary>
        public static byte[] ToUoWav(AudioClip clip)
        {
            float[] s = Resample(clip.Samples, clip.SampleRate, UoSampleRate);
            return ToWav16(s, UoSampleRate);
        }

        #endregion

        #region Effects

        /// <summary>Changes speed and pitch together (factor 2 = one octave higher and half as long).</summary>
        public static float[] ChangeSpeed(float[] s, double factor)
        {
            if (s.Length == 0 || factor <= 0)
            {
                return (float[])s.Clone();
            }
            int len = Math.Max(1, (int)(s.Length / factor));
            var d = new float[len];
            for (int i = 0; i < len; i++)
            {
                double pos = i * factor;
                int p0 = Math.Min(s.Length - 1, (int)pos);
                int p1 = Math.Min(s.Length - 1, p0 + 1);
                double t = pos - p0;
                d[i] = (float)((s[p0] * (1 - t)) + (s[p1] * t));
            }
            return d;
        }

        /// <summary>
        /// Changes the tempo without changing the pitch (overlap-add with a small similarity search, WSOLA).
        /// tempo 2 = twice as fast (half as long).
        /// </summary>
        public static float[] TimeStretch(float[] s, int rate, double tempo)
        {
            if (s.Length == 0 || tempo <= 0 || Math.Abs(tempo - 1) < 0.001)
            {
                return (float[])s.Clone();
            }
            int frame = Math.Max(64, rate * 40 / 1000);      // 40 ms
            int hop = frame / 2;                              // synthesis hop
            int search = Math.Max(16, rate * 10 / 1000);      // +-10 ms
            int outLen = Math.Max(1, (int)(s.Length / tempo));
            var output = new double[outLen + frame];
            var norm = new double[outLen + frame];
            var window = new double[frame];
            for (int i = 0; i < frame; i++)
            {
                window[i] = 0.5 - (0.5 * Math.Cos(2 * Math.PI * i / (frame - 1)));
            }

            int prevIn = 0;
            for (int outPos = 0; outPos < outLen; outPos += hop)
            {
                int nominal = (int)(outPos * tempo);
                int best = nominal;
                if (outPos > 0)
                {
                    // find the input position that continues the previous frame best
                    double bestCorr = double.MinValue;
                    int natural = prevIn + hop;
                    for (int off = -search; off <= search; off += 2)
                    {
                        int cand = nominal + off;
                        if (cand < 0 || cand + frame >= s.Length)
                        {
                            continue;
                        }
                        double corr = 0;
                        for (int k = 0; k < hop; k += 4)
                        {
                            int a = natural + k, b = cand + k;
                            if (a < s.Length && b < s.Length)
                            {
                                corr += s[a] * s[b];
                            }
                        }
                        if (corr > bestCorr)
                        {
                            bestCorr = corr;
                            best = cand;
                        }
                    }
                }
                best = Math.Max(0, Math.Min(Math.Max(0, s.Length - 1), best));
                for (int k = 0; k < frame; k++)
                {
                    int src = best + k;
                    if (src >= s.Length || outPos + k >= output.Length)
                    {
                        break;
                    }
                    output[outPos + k] += s[src] * window[k];
                    norm[outPos + k] += window[k];
                }
                prevIn = best;
            }

            var d = new float[outLen];
            for (int i = 0; i < outLen; i++)
            {
                d[i] = norm[i] > 0.0001 ? (float)(output[i] / norm[i]) : 0f;
            }
            return d;
        }

        /// <summary>Pitch shift in semitones, length stays the same.</summary>
        public static float[] PitchShift(float[] s, int rate, double semitones)
        {
            if (Math.Abs(semitones) < 0.01)
            {
                return (float[])s.Clone();
            }
            double factor = Math.Pow(2, semitones / 12.0);
            // stretch first (longer for higher pitch), then resample back to the original length
            float[] stretched = TimeStretch(s, rate, 1.0 / factor);
            float[] result = ChangeSpeed(stretched, factor);
            if (result.Length != s.Length)
            {
                Array.Resize(ref result, s.Length);
            }
            return result;
        }

        /// <summary>
        /// Simple Schroeder reverb (4 comb + 2 allpass filters). room 0..1 (size / decay), wet 0..1.
        /// withTail = the sound gets longer by the reverb tail.
        /// </summary>
        public static float[] Reverb(float[] s, int rate, double room, double wet, bool withTail)
        {
            int tail = withTail ? (int)(rate * (0.3 + (room * 1.7))) : 0;
            var input = new float[s.Length + tail];
            Array.Copy(s, input, s.Length);

            double scale = rate / 44100.0;
            int[] combDelays = { 1116, 1188, 1277, 1356 };
            int[] allDelays = { 556, 441 };
            double feedback = 0.70 + (room * 0.28);
            double damp = 0.2;

            var wetSig = new double[input.Length];
            foreach (int cd in combDelays)
            {
                int delay = Math.Max(1, (int)(cd * scale));
                var buf = new double[delay];
                int idx = 0;
                double filter = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    double outv = buf[idx];
                    filter = (outv * (1 - damp)) + (filter * damp);
                    buf[idx] = input[i] + (filter * feedback);
                    idx = (idx + 1) % delay;
                    wetSig[i] += outv;
                }
            }
            foreach (int ad in allDelays)
            {
                int delay = Math.Max(1, (int)(ad * scale));
                var buf = new double[delay];
                int idx = 0;
                for (int i = 0; i < wetSig.Length; i++)
                {
                    double bufout = buf[idx];
                    double outv = -wetSig[i] + bufout;
                    buf[idx] = wetSig[i] + (bufout * 0.5);
                    idx = (idx + 1) % delay;
                    wetSig[i] = outv;
                }
            }

            var d = new float[input.Length];
            for (int i = 0; i < d.Length; i++)
            {
                double v = (input[i] * (1 - (wet * 0.5))) + (wetSig[i] * wet * 0.25);
                d[i] = (float)Math.Max(-1, Math.Min(1, v));
            }
            return d;
        }

        /// <summary>Noise gate: everything below the threshold (dBFS) is faded out smoothly.</summary>
        public static void NoiseGate(float[] s, int start, int end, int rate, double thresholdDb)
        {
            start = Math.Max(0, start);
            end = Math.Min(s.Length, end);
            double thr = Math.Pow(10, thresholdDb / 20.0);
            double attack = Math.Exp(-1.0 / (rate * 0.002));   // 2 ms
            double release = Math.Exp(-1.0 / (rate * 0.080));  // 80 ms
            double env = 0, gain = 0;
            for (int i = start; i < end; i++)
            {
                double a = Math.Abs(s[i]);
                env = a > env ? a : (env * 0.999) + (a * 0.001);
                double target = env >= thr ? 1 : 0;
                gain = target > gain ? (gain * attack) + (target * (1 - attack)) : (gain * release) + (target * (1 - release));
                s[i] = (float)(s[i] * gain);
            }
        }

        /// <summary>Brings the clip to the target loudness (RMS in dBFS) without exceeding peakLimit.</summary>
        public static void NormalizeRms(float[] s, double targetDb, float peakLimit)
        {
            double rms = Rms(s, 0, s.Length);
            if (rms < 0.00001)
            {
                return;
            }
            double factor = Math.Pow(10, targetDb / 20.0) / rms;
            float peak = Peak(s, 0, s.Length);
            if (peak * factor > peakLimit)
            {
                factor = peakLimit / peak;
            }
            Gain(s, 0, s.Length, (float)factor);
        }

        #endregion

        #region Processing

        /// <summary>Linear interpolation resampler with a simple low pass when down sampling.</summary>
        public static float[] Resample(float[] src, int fromRate, int toRate)
        {
            if (fromRate == toRate || src.Length == 0)
            {
                return (float[])src.Clone();
            }

            float[] input = src;
            if (toRate < fromRate)
            {
                // moving average low pass against aliasing
                int width = Math.Max(1, (int)Math.Round(fromRate / (double)toRate));
                if (width > 1)
                {
                    input = new float[src.Length];
                    double acc = 0;
                    for (int i = 0; i < src.Length; i++)
                    {
                        acc += src[i];
                        if (i >= width)
                        {
                            acc -= src[i - width];
                        }
                        input[i] = (float)(acc / Math.Min(i + 1, width));
                    }
                }
            }

            long len = (long)Math.Round(src.Length * (double)toRate / fromRate);
            var dst = new float[Math.Max(1, len)];
            double step = fromRate / (double)toRate;
            for (long i = 0; i < dst.Length; i++)
            {
                double pos = i * step;
                int p0 = (int)pos;
                int p1 = Math.Min(input.Length - 1, p0 + 1);
                if (p0 >= input.Length)
                {
                    p0 = input.Length - 1;
                }
                double t = pos - p0;
                dst[i] = (float)((input[p0] * (1 - t)) + (input[p1] * t));
            }
            return dst;
        }

        public static float Peak(float[] s, int start, int end)
        {
            float p = 0;
            for (int i = Math.Max(0, start); i < Math.Min(s.Length, end); i++)
            {
                p = Math.Max(p, Math.Abs(s[i]));
            }
            return p;
        }

        public static double Rms(float[] s, int start, int end)
        {
            start = Math.Max(0, start);
            end = Math.Min(s.Length, end);
            if (end <= start)
            {
                return 0;
            }
            double sum = 0;
            for (int i = start; i < end; i++)
            {
                sum += s[i] * (double)s[i];
            }
            return Math.Sqrt(sum / (end - start));
        }

        public static double ToDb(double v) => v <= 0.000001 ? -120 : 20 * Math.Log10(v);

        /// <summary>Scales the range so that the peak reaches targetPeak (0..1).</summary>
        public static void Normalize(float[] s, int start, int end, float targetPeak)
        {
            float peak = Peak(s, start, end);
            if (peak < 0.00001f)
            {
                return;
            }
            Gain(s, start, end, targetPeak / peak);
        }

        public static void Gain(float[] s, int start, int end, float factor)
        {
            for (int i = Math.Max(0, start); i < Math.Min(s.Length, end); i++)
            {
                s[i] = Math.Max(-1f, Math.Min(1f, s[i] * factor));
            }
        }

        public static void Fade(float[] s, int start, int end, bool fadeIn)
        {
            start = Math.Max(0, start);
            end = Math.Min(s.Length, end);
            int n = end - start;
            if (n <= 1)
            {
                return;
            }
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                s[start + i] *= fadeIn ? t : 1 - t;
            }
        }

        /// <summary>Removes the DC offset (average) of the whole clip.</summary>
        public static void RemoveDc(float[] s)
        {
            if (s.Length == 0)
            {
                return;
            }
            double avg = 0;
            foreach (float v in s)
            {
                avg += v;
            }
            avg /= s.Length;
            for (int i = 0; i < s.Length; i++)
            {
                s[i] = (float)(s[i] - avg);
            }
        }

        /// <summary>Finds the first / last sample above the threshold (silence trimming).</summary>
        public static (int Start, int End) FindSound(float[] s, float threshold)
        {
            int a = 0, b = s.Length;
            while (a < s.Length && Math.Abs(s[a]) < threshold)
            {
                a++;
            }
            while (b > a && Math.Abs(s[b - 1]) < threshold)
            {
                b--;
            }
            return (a, b);
        }

        #endregion
    }
}

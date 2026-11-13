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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Sound-Studio: WAV / MP3 importieren, schneiden, normalisieren, Lautstaerke anpassen,
    /// Ein-/Ausblenden und als UO-Sound (22050 Hz, 16 Bit, Mono) speichern, mit Pegel-Anzeige
    /// und Abspiel-Vorschau.
    /// </summary>
    public class SoundStudioForm : Form
    {
        private float[] _samples = Array.Empty<float>();
        private int _rate = StudioAudio.UoSampleRate;
        private readonly Stack<(float[] Samples, int Rate)> _undo = new Stack<(float[], int)>();
        private string _fileName = "new_sound";

        private readonly WaveformView _wave = new WaveformView { Dock = DockStyle.Fill };
        private readonly LevelMeter _meter = new LevelMeter { Dock = DockStyle.Right, Width = 70 };
        private readonly ToolStripStatusLabel _status = new ToolStripStatusLabel("WAV oder MP3 laden.");
        private readonly ToolStripStatusLabel _info = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleRight };

        private SoundPlayer _player;
        private MemoryStream _playStream;
        private readonly Stopwatch _playWatch = new Stopwatch();
        private readonly Timer _playTimer = new Timer { Interval = 30 };
        private int _playStart, _playEnd;

        private NumericUpDown _numNormalize, _numGain, _numPreviewVol, _numSilence, _numSoundId, _numFadeMs;
        private TextBox _txtName;
        private CheckBox _chkLoop;

        public SoundStudioForm()
        {
            Text = "Sound-Studio (WAV / MP3 → UO-Sound)";
            Size = new Size(1100, 640);
            MinimumSize = new Size(900, 520);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BuildUi();

            _playTimer.Tick += PlayTimer_Tick;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Space)
                {
                    Play(true);
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.Z)
                {
                    Undo();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    DeleteSelection();
                    e.Handled = true;
                }
            };
        }

        #region UI

        private void BuildUi()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 170 };

            var grpFile = new GroupBox { Text = "Datei", Left = 6, Top = 4, Width = 220, Height = 160 };
            grpFile.Controls.Add(StudioUi.Button("WAV / MP3 laden...", 8, 20, 200, (s, e) => LoadFile()));
            grpFile.Controls.Add(StudioUi.Label("UO-Sound ID:", 8, 54));
            _numSoundId = StudioUi.Number(96, 52, 70, 0, 0xFFE, 0);
            grpFile.Controls.Add(_numSoundId);
            grpFile.Controls.Add(StudioUi.Button("Aus UO laden", 8, 80, 98, (s, e) => LoadFromUo()));
            grpFile.Controls.Add(StudioUi.Button("Freie ID", 110, 80, 98, (s, e) => FindFreeId()));
            grpFile.Controls.Add(StudioUi.Button("Rückgängig (Strg+Z)", 8, 112, 200, (s, e) => Undo()));
            top.Controls.Add(grpFile);

            var grpEdit = new GroupBox { Text = "Schneiden (Auswahl mit der Maus ziehen)", Left = 232, Top = 4, Width = 300, Height = 160 };
            grpEdit.Controls.Add(StudioUi.Button("Auf Auswahl zuschneiden", 8, 20, 140, (s, e) => CropToSelection()));
            grpEdit.Controls.Add(StudioUi.Button("Auswahl löschen (Entf)", 152, 20, 140, (s, e) => DeleteSelection()));
            grpEdit.Controls.Add(StudioUi.Button("Stille entfernen", 8, 52, 140, (s, e) => TrimSilence()));
            grpEdit.Controls.Add(StudioUi.Label("Schwelle %:", 152, 56));
            _numSilence = StudioUi.Number(226, 52, 60, 0, 50, 2, 1);
            grpEdit.Controls.Add(_numSilence);
            grpEdit.Controls.Add(StudioUi.Button("Einblenden", 8, 84, 90, (s, e) => DoFade(true)));
            grpEdit.Controls.Add(StudioUi.Button("Ausblenden", 102, 84, 90, (s, e) => DoFade(false)));
            grpEdit.Controls.Add(StudioUi.Label("ms:", 198, 88));
            _numFadeMs = StudioUi.Number(226, 84, 60, 1, 10000, 80);
            grpEdit.Controls.Add(_numFadeMs);
            grpEdit.Controls.Add(StudioUi.Button("Alles auswählen", 8, 116, 140, (s, e) => { _wave.SelectAll(); UpdateInfo(); }));
            grpEdit.Controls.Add(StudioUi.Button("Rückwärts", 152, 116, 140, (s, e) => Reverse()));
            top.Controls.Add(grpEdit);

            var grpLevel = new GroupBox { Text = "Lautstärke", Left = 538, Top = 4, Width = 250, Height = 160 };
            grpLevel.Controls.Add(StudioUi.Label("Normalisieren auf %:", 8, 22));
            _numNormalize = StudioUi.Number(140, 20, 60, 1, 100, 95);
            grpLevel.Controls.Add(_numNormalize);
            grpLevel.Controls.Add(StudioUi.Button("Normalisieren", 8, 48, 230, (s, e) => DoNormalize()));
            grpLevel.Controls.Add(StudioUi.Label("Verstärkung dB:", 8, 84));
            _numGain = StudioUi.Number(140, 82, 60, -40, 40, 3, 1);
            grpLevel.Controls.Add(_numGain);
            grpLevel.Controls.Add(StudioUi.Button("Anwenden", 8, 110, 110, (s, e) => DoGain()));
            grpLevel.Controls.Add(StudioUi.Button("DC entfernen", 128, 110, 110, (s, e) => DoDc()));
            top.Controls.Add(grpLevel);

            var grpPlay = new GroupBox { Text = "Vorschau & Export", Left = 794, Top = 4, Width = 280, Height = 160 };
            grpPlay.Controls.Add(StudioUi.Button("▶ Abspielen (Leertaste)", 8, 20, 170, (s, e) => Play(true)));
            grpPlay.Controls.Add(StudioUi.Button("■ Stop", 182, 20, 88, (s, e) => StopPlay()));
            grpPlay.Controls.Add(StudioUi.Label("Vorschau-Lautst. %:", 8, 54));
            _numPreviewVol = StudioUi.Number(124, 52, 56, 0, 200, 100);
            grpPlay.Controls.Add(_numPreviewVol);
            _chkLoop = StudioUi.Check("Schleife", 190, 54, false);
            grpPlay.Controls.Add(_chkLoop);
            grpPlay.Controls.Add(StudioUi.Label("Name:", 8, 84));
            _txtName = new TextBox { Left = 56, Top = 82, Width = 214, Text = "new_sound.wav", MaxLength = 31 };
            grpPlay.Controls.Add(_txtName);
            grpPlay.Controls.Add(StudioUi.Button("Als UO-Sound setzen", 8, 110, 130, (s, e) => SetUoSound()));
            grpPlay.Controls.Add(StudioUi.Button("sound.mul speichern", 142, 110, 128, (s, e) => SaveSoundMul()));
            grpPlay.Controls.Add(StudioUi.Button("Als WAV (UO-Format) speichern...", 8, 136, 262, (s, e) => SaveWav()));
            grpPlay.Height = 168;
            top.Height = 178;
            top.Controls.Add(grpPlay);

            _wave.SelectionChanged += UpdateInfo;

            var status = new StatusStrip();
            status.Items.Add(_status);
            status.Items.Add(_info);

            Controls.Add(_wave);
            Controls.Add(_meter);
            Controls.Add(top);
            Controls.Add(status);
        }

        #endregion

        #region Data helpers

        private void SetData(float[] samples, int rate, string message)
        {
            _samples = samples;
            _rate = rate;
            _wave.SetSamples(_samples, _rate);
            _status.Text = message;
            UpdateInfo();
        }

        private void PushUndo()
        {
            _undo.Push(((float[])_samples.Clone(), _rate));
            while (_undo.Count > 30)
            {
                // keep the stack small: rebuild without the oldest entry
                var items = _undo.ToArray();
                _undo.Clear();
                for (int i = items.Length - 2; i >= 0; i--)
                {
                    _undo.Push(items[i]);
                }
            }
        }

        private void Undo()
        {
            if (_undo.Count == 0)
            {
                return;
            }
            var (s, r) = _undo.Pop();
            SetData(s, r, "Rückgängig gemacht.");
        }

        private (int Start, int End) Range()
        {
            if (_wave.HasSelection)
            {
                return (_wave.SelStart, _wave.SelEnd);
            }
            return (0, _samples.Length);
        }

        private void UpdateInfo()
        {
            var (a, b) = Range();
            float peak = StudioAudio.Peak(_samples, a, b);
            double rms = StudioAudio.Rms(_samples, a, b);
            _meter.SetLevels(peak, rms);
            double dur = _samples.Length / (double)Math.Max(1, _rate);
            double uoBytes = _samples.Length * (StudioAudio.UoSampleRate / (double)Math.Max(1, _rate)) * 2;
            string sel = _wave.HasSelection ? $"  Auswahl: {(a / (double)_rate):0.000}s – {(b / (double)_rate):0.000}s ({(b - a) / (double)_rate:0.000}s)" : string.Empty;
            _info.Text = $"{dur:0.000}s, {_rate} Hz → UO 22050 Hz/16 Bit/Mono ({uoBytes / 1024:0} KB)  Peak {StudioAudio.ToDb(peak):0.0} dBFS  RMS {StudioAudio.ToDb(rms):0.0} dBFS{sel}";
        }

        private bool HasAudio()
        {
            if (_samples.Length > 0)
            {
                return true;
            }
            StudioUi.Info(this, "Kein Sound geladen.");
            return false;
        }

        #endregion

        #region Loading

        private void LoadFile()
        {
            using var dlg = new OpenFileDialog { Filter = "Audio|*.wav;*.mp3|WAV|*.wav|MP3|*.mp3|Alle Dateien|*.*" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Cursor = Cursors.WaitCursor;
                AudioClip clip = StudioAudio.Load(dlg.FileName);
                PushUndo();
                _fileName = Path.GetFileNameWithoutExtension(dlg.FileName);
                _txtName.Text = Truncate(_fileName + ".wav", 31);
                SetData(clip.Samples, clip.SampleRate, $"Geladen: {Path.GetFileName(dlg.FileName)} ({clip.SampleRate} Hz, {clip.Duration:0.00}s, nach Mono gemischt)");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Datei konnte nicht gelesen werden:\n" + ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void LoadFromUo()
        {
            int id = (int)_numSoundId.Value;
            try
            {
                UoSound sound = Sounds.GetSound(id);
                if (sound?.Buffer == null)
                {
                    StudioUi.Info(this, $"Sound {id} (0x{id:X3}) existiert nicht.");
                    return;
                }
                AudioClip clip = StudioAudio.ReadWav(sound.Buffer);
                PushUndo();
                _txtName.Text = Truncate(string.IsNullOrEmpty(sound.Name) ? $"sound_{id}.wav" : sound.Name, 31);
                SetData(clip.Samples, clip.SampleRate, $"UO-Sound {id} (0x{id:X3}) \"{sound.Name}\" geladen.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void FindFreeId()
        {
            try
            {
                for (int i = 1; i < 0xFFF; i++)
                {
                    if (!Sounds.IsValidSound(i, out _, out _))
                    {
                        _numSoundId.Value = i;
                        _status.Text = $"Freie Sound-ID: {i} (0x{i:X3})";
                        return;
                    }
                }
                StudioUi.Info(this, "Keine freie Sound-ID gefunden.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max);

        #endregion

        #region Editing

        private void CropToSelection()
        {
            if (!HasAudio() || !_wave.HasSelection)
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            var n = new float[b - a];
            Array.Copy(_samples, a, n, 0, n.Length);
            SetData(n, _rate, "Auf Auswahl zugeschnitten.");
        }

        private void DeleteSelection()
        {
            if (!HasAudio() || !_wave.HasSelection)
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            var n = new float[_samples.Length - (b - a)];
            Array.Copy(_samples, 0, n, 0, a);
            Array.Copy(_samples, b, n, a, _samples.Length - b);
            SetData(n, _rate, "Auswahl gelöscht.");
        }

        private void TrimSilence()
        {
            if (!HasAudio())
            {
                return;
            }
            var (a, b) = StudioAudio.FindSound(_samples, (float)_numSilence.Value / 100f);
            if (b <= a)
            {
                StudioUi.Info(this, "Alles ist unter der Schwelle – nichts geändert.");
                return;
            }
            PushUndo();
            var n = new float[b - a];
            Array.Copy(_samples, a, n, 0, n.Length);
            SetData(n, _rate, $"Stille entfernt ({a / (double)_rate:0.000}s vorne, {(_samples.Length - b) / (double)_rate:0.000}s hinten).");
        }

        private void DoFade(bool fadeIn)
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            int a, b;
            if (_wave.HasSelection)
            {
                (a, b) = Range();
            }
            else
            {
                int len = (int)(_rate * (double)_numFadeMs.Value / 1000.0);
                (a, b) = fadeIn ? (0, len) : (_samples.Length - len, _samples.Length);
            }
            StudioAudio.Fade(_samples, a, b, fadeIn);
            SetData(_samples, _rate, fadeIn ? "Eingeblendet." : "Ausgeblendet.");
        }

        private void DoNormalize()
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            StudioAudio.Normalize(_samples, a, b, (float)_numNormalize.Value / 100f);
            SetData(_samples, _rate, $"Normalisiert auf {_numNormalize.Value}% ({StudioAudio.ToDb((double)_numNormalize.Value / 100):0.0} dBFS).");
        }

        private void DoGain()
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            float f = (float)Math.Pow(10, (double)_numGain.Value / 20.0);
            StudioAudio.Gain(_samples, a, b, f);
            SetData(_samples, _rate, $"Verstärkung {_numGain.Value} dB angewendet.");
        }

        private void DoDc()
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            StudioAudio.RemoveDc(_samples);
            SetData(_samples, _rate, "DC-Versatz entfernt.");
        }

        private void Reverse()
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            Array.Reverse(_samples, a, b - a);
            SetData(_samples, _rate, "Umgekehrt.");
        }

        #endregion

        #region Playback

        private void Play(bool toggle)
        {
            if (!HasAudio())
            {
                return;
            }
            if (toggle && _playTimer.Enabled)
            {
                StopPlay();
                return;
            }
            StopPlay();

            var (a, b) = Range();
            _playStart = a;
            _playEnd = b;
            var part = new float[b - a];
            Array.Copy(_samples, a, part, 0, part.Length);

            // preview exactly like UO will play it: resampled to 22050 Hz, 16 bit
            float[] uo = StudioAudio.Resample(part, _rate, StudioAudio.UoSampleRate);
            byte[] wav = StudioAudio.ToWav16(uo, StudioAudio.UoSampleRate, (float)_numPreviewVol.Value / 100f);

            try
            {
                _playStream = new MemoryStream(wav);
                _player = new SoundPlayer(_playStream);
                if (_chkLoop.Checked)
                {
                    _player.PlayLooping();
                }
                else
                {
                    _player.Play();
                }
                _playWatch.Restart();
                _playTimer.Start();
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Wiedergabe nicht möglich:\n" + ex.Message);
            }
        }

        private void StopPlay()
        {
            _playTimer.Stop();
            _playWatch.Stop();
            _player?.Stop();
            _player?.Dispose();
            _player = null;
            _playStream?.Dispose();
            _playStream = null;
            _wave.PlayPosition = -1;
            UpdateInfo();
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            double sec = _playWatch.Elapsed.TotalSeconds;
            int len = _playEnd - _playStart;
            int pos = (int)(sec * _rate);
            if (pos >= len)
            {
                if (!_chkLoop.Checked)
                {
                    StopPlay();
                    return;
                }
                pos = len == 0 ? 0 : pos % len;
            }
            int abs = _playStart + pos;
            _wave.PlayPosition = abs;

            // live level meter (50 ms window) scaled by the preview volume
            int win = Math.Max(1, _rate / 20);
            float vol = (float)_numPreviewVol.Value / 100f;
            _meter.SetLevels(Math.Min(1f, StudioAudio.Peak(_samples, abs, abs + win) * vol), StudioAudio.Rms(_samples, abs, abs + win) * vol);
        }

        #endregion

        #region Export

        private void SaveWav()
        {
            if (!HasAudio())
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "WAV|*.wav", FileName = _txtName.Text, InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            File.WriteAllBytes(dlg.FileName, StudioAudio.ToUoWav(new AudioClip(_samples, _rate)));
            _status.Text = "Gespeichert (22050 Hz, 16 Bit, Mono): " + dlg.FileName;
        }

        private void SetUoSound()
        {
            if (!HasAudio())
            {
                return;
            }
            int id = (int)_numSoundId.Value;
            try
            {
                if (Sounds.IsValidSound(id, out string oldName, out bool translated) && !translated &&
                    !StudioUi.Ask(this, $"Sound {id} (0x{id:X3}) \"{oldName}\" existiert bereits. Überschreiben?"))
                {
                    return;
                }
                string tmp = Path.Combine(Path.GetTempPath(), $"uostudio_sound_{id}.wav");
                File.WriteAllBytes(tmp, StudioAudio.ToUoWav(new AudioClip(_samples, _rate)));
                string name = string.IsNullOrWhiteSpace(_txtName.Text) ? $"sound_{id}.wav" : _txtName.Text.Trim();
                Sounds.Add(id, name, tmp);
                try
                {
                    File.Delete(tmp);
                }
                catch
                {
                    // ignore
                }
                StudioUi.MarkChanged("Sound");
                _status.Text = $"Sound {id} (0x{id:X3}) \"{name}\" gesetzt. Mit \"sound.mul speichern\" sichern.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void SaveSoundMul()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Sounds.Save(StudioUi.OutputPath);
                StudioUi.ResetChanged("Sound");
                StudioUi.Info(this, $"sound.mul / soundidx.mul gespeichert in:\n{StudioUi.OutputPath}");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            StopPlay();
            _playTimer.Dispose();
            base.OnFormClosed(e);
        }

        #region Controls

        /// <summary>Waveform display with mouse selection and play cursor.</summary>
        private sealed class WaveformView : Control
        {
            private float[] _s = Array.Empty<float>();
            private int _rate = 22050;
            private int _selA = -1, _selB = -1;
            private int _play = -1;
            private bool _mouse;

            public WaveformView()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = Color.FromArgb(24, 26, 32);
                Cursor = Cursors.IBeam;
            }

            public event Action SelectionChanged;

            public bool HasSelection => _selA >= 0 && _selB >= 0 && Math.Abs(_selB - _selA) > 1;
            public int SelStart => Math.Max(0, Math.Min(_selA, _selB));
            public int SelEnd => Math.Min(_s.Length, Math.Max(_selA, _selB));

            public int PlayPosition
            {
                get => _play;
                set
                {
                    _play = value;
                    Invalidate();
                }
            }

            public void SetSamples(float[] s, int rate)
            {
                _s = s ?? Array.Empty<float>();
                _rate = rate;
                if (_selA > _s.Length || _selB > _s.Length)
                {
                    _selA = _selB = -1;
                }
                Invalidate();
            }

            public void SelectAll()
            {
                _selA = 0;
                _selB = _s.Length;
                Invalidate();
            }

            private int ToSample(int x) => _s.Length == 0 ? 0 : (int)Math.Max(0, Math.Min(_s.Length, (long)x * _s.Length / Math.Max(1, Width)));

            private int ToX(int sample) => _s.Length == 0 ? 0 : (int)((long)sample * Width / _s.Length);

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button == MouseButtons.Left)
                {
                    _mouse = true;
                    _selA = _selB = ToSample(e.X);
                    Invalidate();
                }
                else if (e.Button == MouseButtons.Right)
                {
                    _selA = _selB = -1;
                    Invalidate();
                    SelectionChanged?.Invoke();
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (_mouse)
                {
                    _selB = ToSample(e.X);
                    Invalidate();
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (_mouse)
                {
                    _mouse = false;
                    SelectionChanged?.Invoke();
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                int w = Width, h = Height, mid = h / 2;

                if (HasSelection)
                {
                    using var sb = new SolidBrush(Color.FromArgb(70, 80, 140, 255));
                    int x1 = ToX(SelStart), x2 = ToX(SelEnd);
                    g.FillRectangle(sb, x1, 0, Math.Max(1, x2 - x1), h);
                }

                using (var grid = new Pen(Color.FromArgb(50, 255, 255, 255)))
                {
                    g.DrawLine(grid, 0, mid, w, mid);
                    // -6 dB lines
                    g.DrawLine(grid, 0, mid - (h / 4), w, mid - (h / 4));
                    g.DrawLine(grid, 0, mid + (h / 4), w, mid + (h / 4));
                    // second ticks
                    if (_rate > 0 && _s.Length > 0)
                    {
                        double secs = _s.Length / (double)_rate;
                        double step = secs > 20 ? 5 : secs > 5 ? 1 : secs > 1 ? 0.25 : 0.1;
                        for (double t = step; t < secs; t += step)
                        {
                            int x = ToX((int)(t * _rate));
                            g.DrawLine(grid, x, h - 12, x, h);
                            g.DrawString($"{t:0.##}s", Font, Brushes.Gray, x + 2, h - 14);
                        }
                    }
                }

                if (_s.Length == 0)
                {
                    g.DrawString("Kein Sound geladen – WAV / MP3 laden oder UO-Sound öffnen.", Font, Brushes.Gray, 10, 10);
                    return;
                }

                using var pen = new Pen(Color.FromArgb(90, 210, 140));
                using var clip = new Pen(Color.FromArgb(255, 80, 80));
                for (int x = 0; x < w; x++)
                {
                    int a = (int)((long)x * _s.Length / w);
                    int b = Math.Max(a + 1, (int)((long)(x + 1) * _s.Length / w));
                    float min = 0, max = 0;
                    for (int i = a; i < b && i < _s.Length; i++)
                    {
                        if (_s[i] < min) { min = _s[i]; }
                        if (_s[i] > max) { max = _s[i]; }
                    }
                    bool clipped = max >= 0.999f || min <= -0.999f;
                    g.DrawLine(clipped ? clip : pen, x, mid - (int)(max * (mid - 2)), x, mid - (int)(min * (mid - 2)));
                }

                if (_play >= 0)
                {
                    int px = ToX(_play);
                    g.DrawLine(Pens.Yellow, px, 0, px, h);
                }
            }
        }

        /// <summary>Vertical peak / RMS meter in dBFS.</summary>
        private sealed class LevelMeter : Control
        {
            private float _peak;
            private double _rms;

            public LevelMeter()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = Color.FromArgb(30, 30, 34);
            }

            public void SetLevels(float peak, double rms)
            {
                _peak = peak;
                _rms = rms;
                Invalidate();
            }

            private int Y(double db) => (int)((Height - 20) * Math.Min(1, Math.Max(0, -db / 60.0))) + 4;

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                int bottom = Height - 16;
                double pdb = StudioAudio.ToDb(_peak), rdb = StudioAudio.ToDb(_rms);

                using (var green = new SolidBrush(Color.FromArgb(60, 180, 90)))
                using (var yellow = new SolidBrush(Color.FromArgb(220, 200, 60)))
                using (var red = new SolidBrush(Color.FromArgb(220, 60, 60)))
                {
                    int top = Y(pdb);
                    for (int y = top; y < bottom; y += 3)
                    {
                        double db = -60.0 * (y - 4) / (Height - 20);
                        Brush b = db > -3 ? red : db > -12 ? yellow : green;
                        g.FillRectangle(b, 8, y, 22, 2);
                    }
                    int rt = Y(rdb);
                    using var rb = new SolidBrush(Color.FromArgb(120, 160, 255));
                    g.FillRectangle(rb, 36, rt, 10, Math.Max(0, bottom - rt));
                }

                using var f = new Font(Font.FontFamily, 7f);
                foreach (int db in new[] { 0, -6, -12, -24, -48 })
                {
                    int y = Y(db);
                    g.DrawLine(Pens.Gray, 4, y, 50, y);
                    g.DrawString(db.ToString(), f, Brushes.Silver, 48, y - 6);
                }
                g.DrawString($"{pdb:0}", f, Brushes.White, 6, bottom + 1);
            }
        }

        #endregion
    }
}

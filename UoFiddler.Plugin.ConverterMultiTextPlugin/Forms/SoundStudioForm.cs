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
using System.IO;
using System.Media;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Sound-Studio: WAV / MP3 importieren, schneiden, normalisieren, Lautstaerke anpassen,
    /// Ein-/Ausblenden und als UO-Sound (22050 Hz, 16 Bit, Mono) speichern, mit Pegel-Anzeige
    /// und Abspiel-Vorschau.
    /// </summary>
    public partial class SoundStudioForm : Form
    {
        private float[] _samples = Array.Empty<float>();
        private int _rate = StudioAudio.UoSampleRate;
        private readonly Stack<(float[] Samples, int Rate)> _undo = new Stack<(float[], int)>();
        private string _fileName = "new_sound";

        private SoundPlayer _player;
        private MemoryStream _playStream;
        private readonly Stopwatch _playWatch = new Stopwatch();
        private int _playStart, _playEnd;

        public SoundStudioForm()
        {
            InitializeComponent();
        }

        #region Events

        private void SoundStudioForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && !(ActiveControl is TextBox) && !(ActiveControl is NumericUpDown))
            {
                Play(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && !(ActiveControl is TextBox))
            {
                DeleteSelection();
                e.Handled = true;
            }
        }

        private void Waveform_SelectionChanged(object sender, EventArgs e) => UpdateInfo();

        private void BtnLoad_Click(object sender, EventArgs e) => LoadFile();

        private void BtnLoadUo_Click(object sender, EventArgs e) => LoadFromUo();

        private void BtnFreeId_Click(object sender, EventArgs e) => FindFreeId();

        private void BtnUndo_Click(object sender, EventArgs e) => Undo();

        private void BtnCrop_Click(object sender, EventArgs e) => CropToSelection();

        private void BtnDeleteSel_Click(object sender, EventArgs e) => DeleteSelection();

        private void BtnTrimSilence_Click(object sender, EventArgs e) => TrimSilence();

        private void BtnFadeIn_Click(object sender, EventArgs e) => DoFade(true);

        private void BtnFadeOut_Click(object sender, EventArgs e) => DoFade(false);

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            waveform.SelectAll();
            UpdateInfo();
        }

        private void BtnReverse_Click(object sender, EventArgs e) => Reverse();

        private void BtnNormalize_Click(object sender, EventArgs e) => DoNormalize();

        private void BtnGain_Click(object sender, EventArgs e) => DoGain();

        private void BtnDc_Click(object sender, EventArgs e) => DoDc();

        private void BtnPlay_Click(object sender, EventArgs e) => Play(true);

        private void BtnStop_Click(object sender, EventArgs e) => StopPlay();

        private void BtnSetUo_Click(object sender, EventArgs e) => SetUoSound();

        private void BtnSaveMul_Click(object sender, EventArgs e) => SaveSoundMul();

        private void BtnSaveWav_Click(object sender, EventArgs e) => SaveWav();

        private void BtnZoomIn_Click(object sender, EventArgs e) => waveform.ZoomIn();

        private void BtnZoomOut_Click(object sender, EventArgs e) => waveform.ZoomOut();

        private void BtnZoomFit_Click(object sender, EventArgs e) => waveform.ZoomFit();

        private void BtnZoomSel_Click(object sender, EventArgs e) => waveform.ZoomSelection();

        private void BtnReverb_Click(object sender, EventArgs e)
        {
            ApplyEffect("Hall", (part, isWhole) => StudioAudio.Reverb(part, _rate, (double)numRoom.Value / 100.0, (double)numWet.Value / 100.0, isWhole));
        }

        private void BtnGate_Click(object sender, EventArgs e)
        {
            ApplyEffect("Rausch-Gate", (part, _) =>
            {
                float[] copy = (float[])part.Clone();
                StudioAudio.NoiseGate(copy, 0, copy.Length, _rate, (double)numGate.Value);
                return copy;
            });
        }

        private void BtnTempo_Click(object sender, EventArgs e)
        {
            double tempo = (double)numTempo.Value / 100.0;
            ApplyEffect($"Tempo {numTempo.Value}%", (part, _) => StudioAudio.TimeStretch(part, _rate, tempo));
        }

        private void BtnPitch_Click(object sender, EventArgs e)
        {
            double st = (double)numPitch.Value;
            ApplyEffect($"Tonhöhe {st:+0;-0;0} Halbtöne", (part, _) => StudioAudio.PitchShift(part, _rate, st));
        }

        private void BtnSpeed_Click(object sender, EventArgs e)
        {
            double factor = (double)numTempo.Value / 100.0;
            ApplyEffect($"Geschwindigkeit {numTempo.Value}%", (part, _) => StudioAudio.ChangeSpeed(part, factor));
        }

        /// <summary>Runs an effect on the selection (or the whole sound); the result may change the length.</summary>
        private void ApplyEffect(string name, Func<float[], bool, float[]> effect)
        {
            if (!HasAudio())
            {
                return;
            }
            Cursor = Cursors.WaitCursor;
            try
            {
                PushUndo();
                var (a, b) = Range();
                bool whole = a == 0 && b == _samples.Length;
                var part = new float[b - a];
                Array.Copy(_samples, a, part, 0, part.Length);
                float[] processed = effect(part, whole);
                var result = new float[_samples.Length - part.Length + processed.Length];
                Array.Copy(_samples, 0, result, 0, a);
                Array.Copy(processed, 0, result, a, processed.Length);
                Array.Copy(_samples, b, result, a + processed.Length, _samples.Length - b);
                SetData(result, _rate, $"{name} angewendet ({(whole ? "ganzer Sound" : "Auswahl")}).");
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

        /// <summary>Loads several files, brings them to the same loudness and saves them / sets them as UO sounds.</summary>
        private void BtnBatch_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Audio|*.wav;*.mp3|Alle Dateien|*.*", Multiselect = true, Title = "Sounds für die Stapel-Verarbeitung" };
            if (ofd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string[] files = ofd.FileNames;
            Array.Sort(files, new NaturalStringComparer());
            int startId = (int)numSoundId.Value;
            DialogResult answer = MessageBox.Show(this,
                $"{files.Length} Dateien auf {numTargetRms.Value} dB RMS angleichen und ins UO-Format (22050 Hz, 16 Bit, Mono) wandeln.\n\n" +
                $"Ja = als UO-Sounds ab ID {startId} (0x{startId:X3}) setzen\nNein = nur als WAV in einen Ordner speichern",
                "Stapel: Lautheit angleichen", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel)
            {
                return;
            }
            string folder = null;
            if (answer == DialogResult.No)
            {
                using var fbd = new FolderBrowserDialog { Description = "Zielordner für die WAV-Dateien", SelectedPath = StudioUi.OutputPath };
                if (fbd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                folder = fbd.SelectedPath;
            }

            int done = 0;
            var errors = new List<string>();
            Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        AudioClip clip = StudioAudio.Load(files[i]);
                        float[] s = StudioAudio.Resample(clip.Samples, clip.SampleRate, StudioAudio.UoSampleRate);
                        StudioAudio.RemoveDc(s);
                        StudioAudio.NormalizeRms(s, (double)numTargetRms.Value, 0.98f);
                        byte[] wav = StudioAudio.ToWav16(s, StudioAudio.UoSampleRate);
                        string baseName = Path.GetFileNameWithoutExtension(files[i]);
                        if (folder != null)
                        {
                            File.WriteAllBytes(Path.Combine(folder, baseName + ".wav"), wav);
                        }
                        else
                        {
                            int id = startId + i;
                            if (id >= 0xFFF)
                            {
                                errors.Add($"{baseName}: ID {id} zu groß");
                                continue;
                            }
                            string tmp = Path.Combine(Path.GetTempPath(), $"uostudio_batch_{id}.wav");
                            File.WriteAllBytes(tmp, wav);
                            Sounds.Add(id, Truncate(baseName + ".wav", 31), tmp);
                            File.Delete(tmp);
                        }
                        done++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{Path.GetFileName(files[i])}: {ex.Message}");
                    }
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
            if (folder == null && done > 0)
            {
                StudioUi.MarkChanged("Sound");
            }
            lblStatus.Text = $"Stapel: {done} von {files.Length} verarbeitet." + (folder == null ? " Mit \"sound.mul speichern\" sichern." : $" Ordner: {folder}");
            StudioUi.Info(this, lblStatus.Text + (errors.Count > 0 ? "\n\nFehler:\n" + string.Join("\n", errors) : string.Empty));
        }

        private void BtnDefScript_Click(object sender, EventArgs e)
        {
            int id = (int)numSoundId.Value;
            string name = txtName.Text;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"# Sound {id} (0x{id:X3}) \"{name}\"");
            sb.AppendLine();
            sb.AppendLine("# ---- Sound.def (optional) --------------------------------------");
            sb.AppendLine("# Leitet eine (leere) Sound-ID auf einen vorhandenen Sound um:");
            sb.AppendLine("# NEUE_ID {VORHANDENE_ID} 0");
            sb.AppendLine($"# 4000 {{{id}}} 0");
            sb.AppendLine();
            sb.AppendLine("// ---- ServUO / RunUO --------------------------------------------");
            sb.AppendLine($"from.PlaySound(0x{id:X});");
            sb.AppendLine($"Effects.PlaySound(from.Location, from.Map, 0x{id:X});");
            sb.AppendLine($"// in einer BaseCreature:  public override int GetIdleSound() => 0x{id:X};");
            sb.AppendLine($"// in einem Spell:         public override int CastSound => 0x{id:X};");
            StudioTextForm.ShowText(this, $"Sound-Einträge für {id}", sb.ToString(), "Sound.def");
        }

        #endregion

        #region Data helpers

        private void SetData(float[] samples, int rate, string message)
        {
            _samples = samples;
            _rate = rate;
            waveform.SetSamples(_samples, _rate);
            lblStatus.Text = message;
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
            if (waveform.HasSelection)
            {
                return (waveform.SelStart, waveform.SelEnd);
            }
            return (0, _samples.Length);
        }

        private void UpdateInfo()
        {
            var (a, b) = Range();
            float peak = StudioAudio.Peak(_samples, a, b);
            double rms = StudioAudio.Rms(_samples, a, b);
            levelMeter.SetLevels(peak, rms);
            double dur = _samples.Length / (double)Math.Max(1, _rate);
            double uoBytes = _samples.Length * (StudioAudio.UoSampleRate / (double)Math.Max(1, _rate)) * 2;
            string sel = waveform.HasSelection ? $"  Auswahl: {(a / (double)_rate):0.000}s – {(b / (double)_rate):0.000}s ({(b - a) / (double)_rate:0.000}s)" : string.Empty;
            lblInfo.Text = $"{dur:0.000}s, {_rate} Hz → UO 22050 Hz/16 Bit/Mono ({uoBytes / 1024:0} KB)  Peak {StudioAudio.ToDb(peak):0.0} dBFS  RMS {StudioAudio.ToDb(rms):0.0} dBFS{sel}";
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
                txtName.Text = Truncate(_fileName + ".wav", 31);
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
            int id = (int)numSoundId.Value;
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
                txtName.Text = Truncate(string.IsNullOrEmpty(sound.Name) ? $"sound_{id}.wav" : sound.Name, 31);
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
                        numSoundId.Value = i;
                        lblStatus.Text = $"Freie Sound-ID: {i} (0x{i:X3})";
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
            if (!HasAudio() || !waveform.HasSelection)
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
            if (!HasAudio() || !waveform.HasSelection)
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
            var (a, b) = StudioAudio.FindSound(_samples, (float)numSilence.Value / 100f);
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
            if (waveform.HasSelection)
            {
                (a, b) = Range();
            }
            else
            {
                int len = (int)(_rate * (double)numFadeMs.Value / 1000.0);
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
            StudioAudio.Normalize(_samples, a, b, (float)numNormalize.Value / 100f);
            SetData(_samples, _rate, $"Normalisiert auf {numNormalize.Value}% ({StudioAudio.ToDb((double)numNormalize.Value / 100):0.0} dBFS).");
        }

        private void DoGain()
        {
            if (!HasAudio())
            {
                return;
            }
            PushUndo();
            var (a, b) = Range();
            float f = (float)Math.Pow(10, (double)numGain.Value / 20.0);
            StudioAudio.Gain(_samples, a, b, f);
            SetData(_samples, _rate, $"Verstärkung {numGain.Value} dB angewendet.");
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
            if (toggle && timerPlay.Enabled)
            {
                StopPlay();
                return;
            }
            StopPlay();

            var (a, b) = Range();
            if (!waveform.HasSelection && waveform.CursorPosition >= 0 && waveform.CursorPosition < _samples.Length)
            {
                // no selection: play from the cursor to the end
                a = waveform.CursorPosition;
            }
            _playStart = a;
            _playEnd = b;
            var part = new float[b - a];
            Array.Copy(_samples, a, part, 0, part.Length);

            // preview exactly like UO will play it: resampled to 22050 Hz, 16 bit
            float[] uo = StudioAudio.Resample(part, _rate, StudioAudio.UoSampleRate);
            byte[] wav = StudioAudio.ToWav16(uo, StudioAudio.UoSampleRate, (float)numPreviewVol.Value / 100f);

            try
            {
                _playStream = new MemoryStream(wav);
                _player = new SoundPlayer(_playStream);
                if (chkLoop.Checked)
                {
                    _player.PlayLooping();
                }
                else
                {
                    _player.Play();
                }
                _playWatch.Restart();
                timerPlay.Start();
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Wiedergabe nicht möglich:\n" + ex.Message);
            }
        }

        private void StopPlay()
        {
            timerPlay.Stop();
            _playWatch.Stop();
            _player?.Stop();
            _player?.Dispose();
            _player = null;
            _playStream?.Dispose();
            _playStream = null;
            waveform.PlayPosition = -1;
            UpdateInfo();
        }

        private void TimerPlay_Tick(object sender, EventArgs e)
        {
            double sec = _playWatch.Elapsed.TotalSeconds;
            int len = _playEnd - _playStart;
            int pos = (int)(sec * _rate);
            if (pos >= len)
            {
                if (!chkLoop.Checked)
                {
                    StopPlay();
                    return;
                }
                pos = len == 0 ? 0 : pos % len;
            }
            int abs = _playStart + pos;
            waveform.PlayPosition = abs;

            // live level meter (50 ms window) scaled by the preview volume
            int win = Math.Max(1, _rate / 20);
            float vol = (float)numPreviewVol.Value / 100f;
            levelMeter.SetLevels(Math.Min(1f, StudioAudio.Peak(_samples, abs, abs + win) * vol), StudioAudio.Rms(_samples, abs, abs + win) * vol);
        }

        #endregion

        #region Export

        private void SaveWav()
        {
            if (!HasAudio())
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "WAV|*.wav", FileName = txtName.Text, InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            File.WriteAllBytes(dlg.FileName, StudioAudio.ToUoWav(new AudioClip(_samples, _rate)));
            lblStatus.Text = "Gespeichert (22050 Hz, 16 Bit, Mono): " + dlg.FileName;
        }

        private void SetUoSound()
        {
            if (!HasAudio())
            {
                return;
            }
            int id = (int)numSoundId.Value;
            try
            {
                if (Sounds.IsValidSound(id, out string oldName, out bool translated) && !translated &&
                    !StudioUi.Ask(this, $"Sound {id} (0x{id:X3}) \"{oldName}\" existiert bereits. Überschreiben?"))
                {
                    return;
                }
                string tmp = Path.Combine(Path.GetTempPath(), $"uostudio_sound_{id}.wav");
                File.WriteAllBytes(tmp, StudioAudio.ToUoWav(new AudioClip(_samples, _rate)));
                string name = string.IsNullOrWhiteSpace(txtName.Text) ? $"sound_{id}.wav" : txtName.Text.Trim();
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
                lblStatus.Text = $"Sound {id} (0x{id:X3}) \"{name}\" gesetzt. Mit \"sound.mul speichern\" sichern.";
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
            base.OnFormClosed(e);
        }
    }
}

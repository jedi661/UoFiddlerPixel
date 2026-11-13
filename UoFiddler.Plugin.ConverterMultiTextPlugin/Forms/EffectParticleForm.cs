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
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Partikel-Generator of the Spell- und Effekt-Editor: creates frame sequences
    /// (sparks, fire, smoke, lightning, magic ring, explosion, healing) with animated preview.
    /// </summary>
    public partial class EffectParticleForm : Form
    {
        private List<Bitmap> _frames = new List<Bitmap>();
        private int _index;
        private bool _ready;

        public EffectParticleForm()
        {
            InitializeComponent();
            cbType.SelectedIndex = 0;
            _ready = true;
            Generate();
            timerPreview.Start();
        }

        /// <summary>The generated frames. The caller takes ownership (they are not disposed by the form).</summary>
        public List<Bitmap> TakeFrames()
        {
            List<Bitmap> result = _frames;
            _frames = new List<Bitmap>();
            return result;
        }

        private void Settings_Changed(object sender, EventArgs e)
        {
            if (_ready)
            {
                Generate();
            }
        }

        private void CbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (ParticleType)Math.Max(0, cbType.SelectedIndex);
            pnlColor.BackColor = ParticleGenerator.DefaultColor(type);
            Settings_Changed(sender, e);
        }

        private void PnlColor_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog { Color = pnlColor.BackColor, FullOpen = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                pnlColor.BackColor = dlg.Color;
                Settings_Changed(sender, e);
            }
        }

        private void BtnNewSeed_Click(object sender, EventArgs e)
        {
            numSeed.Value = new Random().Next(0, (int)numSeed.Maximum);
        }

        private void NumPreviewMs_ValueChanged(object sender, EventArgs e)
        {
            timerPreview.Interval = (int)numPreviewMs.Value;
        }

        private void TimerPreview_Tick(object sender, EventArgs e)
        {
            if (_frames.Count == 0)
            {
                return;
            }
            _index = (_index + 1) % _frames.Count;
            viewPreview.Image = _frames[_index];
            viewPreview.Caption = $"Frame {_index + 1}/{_frames.Count}";
        }

        private void Generate()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                viewPreview.Image = null;
                foreach (Bitmap b in _frames)
                {
                    b.Dispose();
                }
                var type = (ParticleType)Math.Max(0, cbType.SelectedIndex);
                _frames = ParticleGenerator.Generate(type, (int)numFrames.Value, (int)numSize.Value, (int)numCount.Value,
                                                     pnlColor.BackColor, (int)numSeed.Value);
                if (chkGrayscale.Checked)
                {
                    for (int i = 0; i < _frames.Count; i++)
                    {
                        Bitmap g = StudioImaging.ToGray(_frames[i]);
                        _frames[i].Dispose();
                        _frames[i] = g;
                    }
                }
                _index = 0;
                if (_frames.Count > 0)
                {
                    viewPreview.Image = _frames[0];
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timerPreview.Stop();
            viewPreview.Image = null;
            // with OK the caller takes the frames via TakeFrames() (FormClosed is raised before ShowDialog returns)
            if (DialogResult != DialogResult.OK)
            {
                foreach (Bitmap b in _frames)
                {
                    b.Dispose();
                }
                _frames.Clear();
            }
            base.OnFormClosed(e);
        }
    }
}

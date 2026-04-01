/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WINE-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer and Wine in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationListMissingForm : Form
    {        
        private readonly List<MissingEntry> _missing;
        private readonly XmlDocument _dom;
        private readonly XmlElement _root;
        private readonly string _sourcePath;
        private int _index = 0;
        private AnimationFrame[] _frames;
        private int _frameIndex = 0;
        private int _facing = 1;        

        public AnimationListMissingForm(List<MissingEntry> missing, XmlDocument dom,
                                        XmlElement root, string sourcePath)
        {
            _missing = missing;
            _dom = dom;
            _root = root;
            _sourcePath = sourcePath;

            InitializeComponent();
                        
            // Prevents flickering when the timer is invalid.
            typeof(Control)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.SetValue(_previewBox, true);

            _animTimer.Start();

            FormClosed += (s, e) =>
            {
                _animTimer.Stop();
                _animTimer.Dispose();
            };

            ShowEntry(_index);
        }        

        private void PreviewBox_Paint(object sender, PaintEventArgs e)
        {
            // Gray Windows default background
            e.Graphics.Clear(SystemColors.Control);

            if (_frames == null || _frames.Length == 0) return;

            Bitmap bmp = _frames[_frameIndex % _frames.Length]?.Bitmap;
            if (bmp == null) return;

            Rectangle box = _previewBox.ClientRectangle;

            int drawW = bmp.Width;
            int drawH = bmp.Height;

            bool needsScale = drawW > box.Width || drawH > box.Height;

            if (needsScale)
            {                
                float scale = Math.Min((float)box.Width / drawW,
                                       (float)box.Height / drawH);
                drawW = (int)(drawW * scale);
                drawH = (int)(drawH * scale);

                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.SmoothingMode = SmoothingMode.None;
            }
            else
            {                
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.SmoothingMode = SmoothingMode.None;
            }

            // Centering
            int x = box.X + (box.Width - drawW) / 2;
            int y = box.Y + (box.Height - drawH) / 2;
            
            e.Graphics.DrawImage(
                bmp,
                new Rectangle(x, y, drawW, drawH),
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                GraphicsUnit.Pixel);
        }

        // ════════════════════════════════════════════════════════════════════
        //  Show entry
        // ════════════════════════════════════════════════════════════════════

        private void ShowEntry(int index)
        {
            if (index >= _missing.Count)
            {
                FinishAndSave();
                return;
            }

            MissingEntry entry = _missing[index];

            _lblInfo.Text =
                $"Body-ID:  {entry.GraphicId}  (0x{entry.GraphicId:X})\n" +
                $"Datei:    {Animations.GetFileName(entry.GraphicId) ?? "–"}";

            _lblProgress.Text = $"entry {index + 1} from {_missing.Count}";

            string suggested = Animations.GetFileName(entry.GraphicId) ?? $"anim_{entry.GraphicId}";
            _txtName.Text = $"{suggested} ({entry.GraphicId})";

            _cmbType.SelectedIndex = Math.Min(entry.SuggestedType, 3);

            LoadFrames(entry.GraphicId);
        }

        private void LoadFrames(int graphicId)
        {
            _frameIndex = 0;
            int hue = 0;
            try
            {
                _frames = Animations.GetAnimation(graphicId, 0, _facing, ref hue, false, false);
            }
            catch
            {
                _frames = null;
            }
            _previewBox.Invalidate();
        }

        // ════════════════════════════════════════════════════════════════════
        //  Event handler
        // ════════════════════════════════════════════════════════════════════

        private void OnAnimTick(object sender, EventArgs e)
        {
            if (_frames == null || _frames.Length == 0) return;
            _frameIndex = (_frameIndex + 1) % _frames.Length;
            _previewBox.Invalidate();
        }

        private void TrackFacing_ValueChanged(object sender, EventArgs e)
        {
            _facing = _trackFacing.Value;
            LoadFrames(_missing[_index].GraphicId);
        }

        private void OnAccept(object sender, EventArgs e)
        {
            WriteEntry(_index);
            _index++;
            ShowEntry(_index);
        }

        private void OnSkip(object sender, EventArgs e)
        {
            _index++;
            ShowEntry(_index);
        }

        private void OnAcceptAll(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                $"All remaining {_missing.Count - _index} Entries automatically\n" +
                "Accept with suggested name and type?",
                "Everyone takes over",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            for (int i = _index; i < _missing.Count; i++)
            {
                MissingEntry entry = _missing[i];
                string autoName = Animations.GetFileName(entry.GraphicId) ?? $"anim_{entry.GraphicId}";
                WriteEntryDirect(entry.GraphicId, entry.SuggestedType, $"{autoName} ({entry.GraphicId})");
            }

            FinishAndSave();
        }

        private void OnCancel(object sender, EventArgs e) => Close();

        #region [ WriteEntry ] // Reads the UI elements for the current entry and writes them to the DOM.
        private void WriteEntry(int index)
        {
            MissingEntry entry = _missing[index];
            int type = _cmbType.SelectedIndex;
            string name = _txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) name = $"anim_{entry.GraphicId}";
            WriteEntryDirect(entry.GraphicId, type, name);
        }
        #endregion

        #region [ Write directly to the DOM without reading UI elements] ───────────────
        private void WriteEntryDirect(int graphicId, int type, string name)
        {
            string tag = type == 3 ? "Equip" : "Mob";
            XmlElement elem = _dom.CreateElement(tag);
            elem.SetAttribute("name", name);
            elem.SetAttribute("body", graphicId.ToString());
            elem.SetAttribute("type", type.ToString());
            _root.AppendChild(elem);
        }
        #endregion

        #region [ Save ]
        private void FinishAndSave()
        {
            _animTimer.Stop();

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Title = "Animationlist.xml Save";
                dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                dlg.FileName = "Animationlist.xml";
                dlg.InitialDirectory = Options.OutputPath ?? Options.AppDataPath;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _dom.Save(dlg.FileName);
                        MessageBox.Show(
                            $"Saved under:\n{dlg.FileName}",
                            "Saved",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving:\n{ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            Close();
        }
        #endregion

        #region [ Helper class for missing entries] ───────────────────────────────
        public class MissingEntry
        {
            public int GraphicId { get; set; }
            public int SuggestedType { get; set; }
        }
        #endregion
    }
}
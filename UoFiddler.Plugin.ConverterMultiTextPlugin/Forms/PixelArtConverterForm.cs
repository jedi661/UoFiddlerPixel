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
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.UoStudio;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Pixel-Art-Konverter: wandelt ein beliebiges Bild / Foto in ein UO-taugliches Item (Art),
    /// Bodentile, Wandteil oder Gump um (Hintergrund entfernen, Farben, Groesse, Isometrie,
    /// Palette reduzieren, Kanten / Outline, UO 15-Bit Farben).
    /// </summary>
    public partial class PixelArtConverterForm : Form
    {
        private Bitmap _source;
        private Bitmap _result;
        private Bitmap _paletteImage;   // image for the palette source "Aus Bilddatei"
        private bool _loadingPreset;

        public PixelArtConverterForm()
        {
            InitializeComponent();

            // ComboBox start values (the designer does not store SelectedIndex)
            _loadingPreset = true;
            cbBackground.SelectedIndex = 1;
            cbScale.SelectedIndex = 0;
            cbIso.SelectedIndex = 0;
            cbOutline.SelectedIndex = 2;
            cbPaletteSource.SelectedIndex = 0;
            cbPreset.SelectedIndex = 0;
            _loadingPreset = false;
            ApplyPreset();
        }

        #region Settings / events

        private void Settings_Changed(object sender, EventArgs e)
        {
            if (_loadingPreset)
            {
                return;
            }
            // re-process with a small delay
            timerDebounce.Stop();
            timerDebounce.Start();
        }

        private void TimerDebounce_Tick(object sender, EventArgs e)
        {
            timerDebounce.Stop();
            Process();
        }

        private void CbPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loadingPreset)
            {
                ApplyPreset();
            }
        }

        private void NumZoom_ValueChanged(object sender, EventArgs e)
        {
            viewResult.Zoom = (int)numZoom.Value;
        }

        private void PnlColor_Click(object sender, EventArgs e)
        {
            if (sender is not Panel target)
            {
                return;
            }
            using var dlg = new ColorDialog { Color = target.BackColor, FullOpen = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                target.BackColor = dlg.Color;
                Settings_Changed(sender, e);
            }
        }

        private void ViewSource_MouseClick(object sender, MouseEventArgs e)
        {
            if (_source == null)
            {
                return;
            }
            Rectangle r = viewSource.ImageRectangle;
            if (!r.Contains(e.Location))
            {
                return;
            }
            int x = (e.X - r.X) / viewSource.Zoom;
            int y = (e.Y - r.Y) / viewSource.Zoom;
            if (x < _source.Width && y < _source.Height)
            {
                pnlKeyColor.BackColor = Color.FromArgb(255, _source.GetPixel(x, y));
                if (cbBackground.SelectedIndex == 0)
                {
                    cbBackground.SelectedIndex = 1;
                }
                Settings_Changed(sender, e);
            }
        }

        private void PixelArtConverterForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void PixelArtConverterForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadImage(files[0]);
            }
        }

        private void ApplyPreset()
        {
            _loadingPreset = true;
            try
            {
                switch (cbPreset.SelectedIndex)
                {
                    case 0: SetSize(44, 80, 0, 32, 2); break;
                    case 1: SetSize(88, 140, 0, 48, 2); break;
                    case 2: SetSize(44, 44, 1, 48, 0); numIsoWidth.Value = 44; break;
                    case 3: SetSize(200, 200, 2, 32, 2); numIsoWidth.Value = 44; break;
                    case 4: SetSize(200, 200, 3, 32, 2); numIsoWidth.Value = 44; break;
                    case 5: SetSize(64, 64, 0, 64, 1); break;
                    case 6: SetSize(2000, 2000, 0, 256, 0); break;
                }
                chkAutoCrop.Checked = cbPreset.SelectedIndex != 2;
            }
            finally
            {
                _loadingPreset = false;
            }
            Settings_Changed(this, EventArgs.Empty);
        }

        private void SetSize(int w, int h, int iso, int colors, int outline)
        {
            numMaxW.Value = w;
            numMaxH.Value = h;
            cbIso.SelectedIndex = iso;
            numColors.Value = colors;
            cbOutline.SelectedIndex = outline;
        }

        #endregion

        #region Load

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter, Title = "Bild oder Foto laden" };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                LoadImage(dlg.FileName);
            }
        }

        private void LoadImage(string file)
        {
            try
            {
                SetSource(StudioImaging.LoadImage(file));
                lblStatus.Text = $"Geladen: {Path.GetFileName(file)} ({_source.Width}x{_source.Height})";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Bild konnte nicht geladen werden:\n" + ex.Message);
            }
        }

        private void BtnClipboard_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                StudioUi.Info(this, "Die Zwischenablage enthält kein Bild.");
                return;
            }
            using Image img = Clipboard.GetImage();
            SetSource(StudioImaging.To32(img));
            lblStatus.Text = $"Aus Zwischenablage ({_source.Width}x{_source.Height})";
        }

        private void SetSource(Bitmap bmp)
        {
            // detach the view first, then free the old image
            viewSource.Image = null;
            _source?.Dispose();
            _source = bmp;
            // background colour guess: upper left pixel
            Color c = _source.GetPixel(0, 0);
            pnlKeyColor.BackColor = c.A == 0 ? Color.White : Color.FromArgb(255, c);
            viewSource.Zoom = Math.Max(1, Math.Min(4, 400 / Math.Max(_source.Width, _source.Height)));
            viewSource.Image = _source;
            Process();
        }

        #endregion

        #region Processing

        private void Process()
        {
            if (_source == null)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                Bitmap img = Convert(_source, true);
                Bitmap oldResult = _result;
                _result = img;
                UpdateDisplay();
                oldResult?.Dispose();
                int hueId = (int)numHuePreview.Value;
                lblStatus.Text = $"Ergebnis: {_result.Width}x{_result.Height} Pixel, {StudioImaging.CountColors(_result)} Farben" +
                                 (hueId > 0 ? $" (Hue {hueId} nur Vorschau)" : string.Empty);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Fehler: " + ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>Shows the result, or the before/after comparison.</summary>
        private void UpdateDisplay()
        {
            Image old = viewResult.Image;
            if (_result == null)
            {
                viewResult.Image = null;
            }
            else if (chkCompare.Checked && _source != null)
            {
                viewResult.Image = StudioImaging.CompareSplit(_source, _result, trackCompare.Value / 100.0);
            }
            else
            {
                viewResult.Image = _result;
            }
            if (old != null && !ReferenceEquals(old, _result) && !ReferenceEquals(old, viewResult.Image))
            {
                old.Dispose();
            }
        }

        private void Compare_Changed(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        /// <summary>
        /// Runs the whole conversion with the current settings on any source image
        /// (used for the preview and for the batch conversion).
        /// </summary>
        private Bitmap Convert(Bitmap source, bool withHuePreview)
        {
            Bitmap img = new Bitmap(source);

            Color key = pnlKeyColor.BackColor;
            img = Step(img, b => cbBackground.SelectedIndex switch
            {
                1 => StudioImaging.RemoveBackgroundFromEdges(b, key, (int)numTolerance.Value),
                2 => StudioImaging.RemoveBackground(b, key, (int)numTolerance.Value),
                _ => new Bitmap(b)
            });

            if (chkAutoCrop.Checked)
            {
                img = Step(img, b => StudioImaging.AutoCrop(b));
            }

            img = Step(img, b => StudioImaging.AdjustColors(b, (int)numBrightness.Value, (int)numContrast.Value, (int)numSaturation.Value));

            if (chkGray.Checked)
            {
                img = Step(img, StudioImaging.ToGray);
            }

            var iso = (IsoMode)Math.Max(0, cbIso.SelectedIndex);
            InterpolationMode mode = cbScale.SelectedIndex switch
            {
                1 => InterpolationMode.NearestNeighbor,
                2 => InterpolationMode.HighQualityBilinear,
                _ => InterpolationMode.HighQualityBicubic
            };

            if (iso == IsoMode.None)
            {
                img = Step(img, b => StudioImaging.ResizeFit(b, (int)numMaxW.Value, (int)numMaxH.Value, mode, chkUpscale.Checked));
            }
            else
            {
                int iw = (int)numIsoWidth.Value;
                if (iso == IsoMode.Floor)
                {
                    img = Step(img, b => StudioImaging.Resize(b, iw, iw, mode));
                }
                img = Step(img, b => StudioImaging.IsoProject(b, iso, iw));
            }

            if (numSharpen.Value > 0)
            {
                img = Step(img, b => StudioImaging.Sharpen(b, (double)numSharpen.Value));
            }

            img = Step(img, b => StudioImaging.HardAlpha(b, 128));

            List<Color> palette = BuildPalette(img);
            img = Step(img, b => StudioImaging.ApplyPalette(b, palette, chkDither.Checked));

            if (cbOutline.SelectedIndex > 0)
            {
                img = Step(img, b => StudioImaging.Outline(b, pnlOutlineColor.BackColor, cbOutline.SelectedIndex == 1, chkOutlineDiagonal.Checked));
            }

            if (chkShadow.Checked && iso != IsoMode.Floor)
            {
                img = Step(img, b => StudioImaging.AddShadow(b, (int)numShadowX.Value, (int)numShadowY.Value, Color.FromArgb(24, 22, 20), chkShadowDither.Checked));
            }

            if (iso == IsoMode.Floor && cbPreset.SelectedIndex == 2 && (img.Width != 44 || img.Height != 44))
            {
                img = Step(img, b => StudioImaging.Resize(b, 44, 44, InterpolationMode.NearestNeighbor));
            }

            img = Step(img, StudioImaging.ToUoColors);

            int hueId = (int)numHuePreview.Value;
            if (withHuePreview && hueId > 0)
            {
                Hue hue = StudioUi.GetHue(hueId);
                img = Step(img, b => StudioImaging.ApplyHue(b, hue, !chkGray.Checked));
            }
            return img;
        }

        /// <summary>Palette: optimised from the image, or taken from an art item / a palette image.</summary>
        private List<Color> BuildPalette(Bitmap img)
        {
            int colors = Math.Max(2, (int)numColors.Value - (cbOutline.SelectedIndex > 0 ? 1 : 0));
            switch (cbPaletteSource.SelectedIndex)
            {
                case 1:
                    if (StudioUi.TryParseId(txtPaletteId.Text, out int artId))
                    {
                        using Bitmap art = UltimaImages.CopyStatic(artId);
                        if (art != null)
                        {
                            lblPaletteInfo.Text = $"Art 0x{artId:X4}";
                            return StudioImaging.BuildPalette(new[] { art }, colors);
                        }
                    }
                    lblPaletteInfo.Text = "Art-ID ungültig";
                    break;
                case 2:
                    if (_paletteImage != null)
                    {
                        return StudioImaging.BuildPalette(new[] { _paletteImage }, colors);
                    }
                    lblPaletteInfo.Text = "kein Palettenbild";
                    break;
            }
            return StudioImaging.BuildPalette(new[] { img }, colors);
        }

        private void BtnPaletteFile_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = StudioUi.ImageFilter, Title = "Bild, dessen Farben als Palette dienen" };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            try
            {
                _paletteImage?.Dispose();
                _paletteImage = StudioImaging.LoadImage(dlg.FileName);
                lblPaletteInfo.Text = Path.GetFileName(dlg.FileName);
                cbPaletteSource.SelectedIndex = 2;
                Settings_Changed(sender, e);
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void BtnBatch_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = StudioUi.ImageFilter, Multiselect = true, Title = "Bilder für die Stapel-Umwandlung" };
            if (ofd.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            string[] files = ofd.FileNames;
            Array.Sort(files, new NaturalStringComparer());

            bool gump = cbPreset.SelectedIndex >= 5;
            bool land = cbPreset.SelectedIndex == 2;
            string kind = gump ? "Gumps" : land ? "Bodentiles" : "Art-Items";
            DialogResult answer = MessageBox.Show(this,
                $"{files.Length} Bilder mit den aktuellen Einstellungen umwandeln.\n\n" +
                $"Ja = als {kind} an fortlaufende IDs ab {txtId.Text} setzen\n" +
                "Nein = nur als PNG in einen Ordner speichern",
                "Stapel umwandeln", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (answer == DialogResult.Cancel)
            {
                return;
            }

            string folder = null;
            int startId = 0;
            if (answer == DialogResult.No)
            {
                using var fbd = new FolderBrowserDialog { Description = "Zielordner für die PNG-Dateien", SelectedPath = StudioUi.OutputPath };
                if (fbd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                folder = fbd.SelectedPath;
            }
            else if (!GetId(out startId))
            {
                return;
            }

            int done = 0, skipped = 0;
            Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    using Bitmap src = StudioImaging.LoadImage(files[i]);
                    using Bitmap result = Convert(src, false);
                    if (folder != null)
                    {
                        result.Save(Path.Combine(folder, Path.GetFileNameWithoutExtension(files[i]) + "_uo.png"), ImageFormat.Png);
                        done++;
                        continue;
                    }

                    int id = startId + i;
                    if (gump)
                    {
                        if (id >= Gumps.GetCount())
                        {
                            skipped++;
                            continue;
                        }
                        Gumps.ReplaceGump(id, StudioImaging.To1555(result));
                    }
                    else if (land)
                    {
                        if (result.Width != 44 || result.Height != 44)
                        {
                            skipped++;
                            continue;
                        }
                        Art.ReplaceLand(id, StudioImaging.To1555(result));
                    }
                    else
                    {
                        if (id > Art.GetMaxItemId())
                        {
                            skipped++;
                            continue;
                        }
                        Art.ReplaceStatic(id, StudioImaging.To1555(result));
                    }
                    done++;
                }
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }

            if (folder == null && done > 0)
            {
                StudioUi.MarkChanged(gump ? "Gumps" : "Art");
            }
            string where = folder ?? $"{kind} 0x{startId:X4} – 0x{startId + files.Length - 1:X4}";
            lblStatus.Text = $"Stapel: {done} umgewandelt, {skipped} übersprungen → {where}";
            StudioUi.Info(this, lblStatus.Text + (folder == null ? "\nZum Sichern noch die MUL speichern." : string.Empty));
        }

        private static Bitmap Step(Bitmap input, Func<Bitmap, Bitmap> f)
        {
            Bitmap output = f(input);
            if (!ReferenceEquals(output, input))
            {
                input.Dispose();
            }
            return output;
        }

        /// <summary>The result without the hue preview (hue preview is not exported).</summary>
        private Bitmap ExportBitmap()
        {
            if (_result == null || _source == null)
            {
                StudioUi.Info(this, "Noch kein Ergebnis vorhanden. Bitte zuerst ein Bild laden.");
                return null;
            }
            return numHuePreview.Value > 0 ? Convert(_source, false) : new Bitmap(_result);
        }

        #endregion

        #region Export

        private bool GetId(out int id)
        {
            if (!StudioUi.TryParseId(txtId.Text, out id) || id < 0)
            {
                StudioUi.Error(this, "Ungültige ID.");
                return false;
            }
            return true;
        }

        private void BtnFreeId_Click(object sender, EventArgs e)
        {
            try
            {
                bool gump = cbPreset.SelectedIndex >= 5;
                bool land = cbPreset.SelectedIndex == 2;
                int max = gump ? Gumps.GetCount() : land ? 0x4000 : Art.GetMaxItemId() + 1;
                for (int i = 1; i < max; i++)
                {
                    bool used = gump ? Gumps.IsValidIndex(i) : land ? Art.IsValidLand(i) : Art.IsValidStatic(i);
                    if (!used)
                    {
                        txtId.Text = $"0x{i:X4}";
                        lblStatus.Text = $"Freie {(gump ? "Gump" : land ? "Land" : "Item")}-ID: 0x{i:X4} ({i})";
                        return;
                    }
                }
                StudioUi.Info(this, "Keine freie ID gefunden.");
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Client-Dateien nicht geladen?\n" + ex.Message);
            }
        }

        private void BtnSetArt_Click(object sender, EventArgs e)
        {
            ExportArt(false);
        }

        private void BtnSetLand_Click(object sender, EventArgs e)
        {
            ExportArt(true);
        }

        private void ExportArt(bool land)
        {
            if (!GetId(out int id))
            {
                return;
            }
            using Bitmap bmp = ExportBitmap();
            if (bmp == null)
            {
                return;
            }

            try
            {
                if (land)
                {
                    if (bmp.Width != 44 || bmp.Height != 44)
                    {
                        StudioUi.Error(this, "Bodentiles müssen genau 44x44 Pixel groß sein. Bitte Vorlage \"Bodentile\" verwenden.");
                        return;
                    }
                    if (Art.IsValidLand(id) && !StudioUi.Ask(this, $"Land-ID 0x{id:X4} ist belegt. Überschreiben?"))
                    {
                        return;
                    }
                    Art.ReplaceLand(id, StudioImaging.To1555(bmp));
                }
                else
                {
                    if (Art.IsValidStatic(id) && !StudioUi.Ask(this, $"Item-ID 0x{id:X4} ist belegt. Überschreiben?"))
                    {
                        return;
                    }
                    Art.ReplaceStatic(id, StudioImaging.To1555(bmp));
                }
                StudioUi.MarkChanged("Art");
                lblStatus.Text = $"{(land ? "Land" : "Item")} 0x{id:X4} gesetzt. Mit \"Art-MUL speichern\" in {StudioUi.OutputPath} schreiben.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void BtnSetGump_Click(object sender, EventArgs e)
        {
            if (!GetId(out int id))
            {
                return;
            }
            using Bitmap bmp = ExportBitmap();
            if (bmp == null)
            {
                return;
            }
            try
            {
                if (id >= Gumps.GetCount())
                {
                    StudioUi.Error(this, $"Gump-ID {id} liegt außerhalb der Gump-Datei (max. {Gumps.GetCount() - 1}).");
                    return;
                }
                if (Gumps.IsValidIndex(id) && !StudioUi.Ask(this, $"Gump-ID 0x{id:X4} ist belegt. Überschreiben?"))
                {
                    return;
                }
                Gumps.ReplaceGump(id, StudioImaging.To1555(bmp));
                StudioUi.MarkChanged("Gumps");
                lblStatus.Text = $"Gump 0x{id:X4} gesetzt. Mit \"Gump-MUL speichern\" in {StudioUi.OutputPath} schreiben.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void BtnSavePng_Click(object sender, EventArgs e)
        {
            using Bitmap bmp = ExportBitmap();
            if (bmp == null)
            {
                return;
            }
            using var dlg = new SaveFileDialog { Filter = "PNG|*.png|Bitmap|*.bmp", FileName = "uo_pixelart.png", InitialDirectory = StudioUi.OutputPath };
            if (dlg.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            bmp.Save(dlg.FileName, dlg.FilterIndex == 2 ? ImageFormat.Bmp : ImageFormat.Png);
            lblStatus.Text = "Gespeichert: " + dlg.FileName;
        }

        private void BtnCopyResult_Click(object sender, EventArgs e)
        {
            if (_result != null)
            {
                Clipboard.SetImage(_result);
            }
        }

        private void BtnSaveArt_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Art.Save(StudioUi.OutputPath);
                StudioUi.ResetChanged("Art");
                StudioUi.Info(this, $"art.mul / artidx.mul gespeichert in:\n{StudioUi.OutputPath}");
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

        private void BtnSaveGump_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Gumps.Save(StudioUi.OutputPath);
                StudioUi.ResetChanged("Gumps");
                StudioUi.Info(this, $"gumpart.mul / gumpidx.mul gespeichert in:\n{StudioUi.OutputPath}");
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
            timerDebounce.Stop();
            Image shown = viewResult.Image;
            viewSource.Image = null;
            viewResult.Image = null;
            if (shown != null && !ReferenceEquals(shown, _result))
            {
                shown.Dispose();
            }
            _source?.Dispose();
            _result?.Dispose();
            _paletteImage?.Dispose();
            base.OnFormClosed(e);
        }
    }
}

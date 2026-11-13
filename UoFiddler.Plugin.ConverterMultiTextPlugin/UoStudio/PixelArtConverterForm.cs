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

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.UoStudio
{
    /// <summary>
    /// Pixel-Art-Konverter: wandelt ein beliebiges Bild / Foto in ein UO-taugliches Item (Art),
    /// Bodentile, Wandteil oder Gump um (Hintergrund entfernen, Farben, Groesse, Isometrie,
    /// Palette reduzieren, Kanten / Outline, UO 15-Bit Farben).
    /// </summary>
    public class PixelArtConverterForm : Form
    {
        private Bitmap _source;
        private Bitmap _result;
        private readonly Timer _debounce = new Timer { Interval = 250 };

        private readonly PixelView _viewSource = new PixelView { Dock = DockStyle.Fill, Caption = "Original", Zoom = 1 };
        private readonly PixelView _viewResult = new PixelView { Dock = DockStyle.Fill, Caption = "Ergebnis (UO)", Zoom = 3 };
        private readonly ToolStripStatusLabel _status = new ToolStripStatusLabel("Bild laden, um zu beginnen.");

        private ComboBox _cbPreset;
        private ComboBox _cbBackground;
        private Panel _pnlKeyColor;
        private NumericUpDown _numTolerance;
        private NumericUpDown _numBrightness, _numContrast, _numSaturation;
        private NumericUpDown _numMaxW, _numMaxH;
        private ComboBox _cbScale;
        private CheckBox _chkUpscale;
        private ComboBox _cbIso;
        private NumericUpDown _numIsoWidth;
        private NumericUpDown _numSharpen;
        private NumericUpDown _numColors;
        private CheckBox _chkDither;
        private ComboBox _cbOutline;
        private Panel _pnlOutlineColor;
        private CheckBox _chkOutlineDiagonal;
        private CheckBox _chkAutoCrop;
        private CheckBox _chkGray;
        private NumericUpDown _numHuePreview;
        private TextBox _txtId;
        private NumericUpDown _numZoom;

        private bool _loadingPreset;

        public PixelArtConverterForm()
        {
            Text = "Pixel-Art-Konverter (Bild → UO Item / Gump)";
            Size = new Size(1180, 760);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            _debounce.Tick += (s, e) =>
            {
                _debounce.Stop();
                Process();
            };

            BuildUi();
            ApplyPreset();
        }

        #region UI

        private void BuildUi()
        {
            var settings = new Panel { Dock = DockStyle.Left, Width = 330, AutoScroll = true, Padding = new Padding(4) };
            int y = 6;

            settings.Controls.Add(StudioUi.Button("Bild laden...", 6, y, 150, (s, e) => LoadImage()));
            settings.Controls.Add(StudioUi.Button("Aus Zwischenablage", 160, y, 150, (s, e) => LoadFromClipboard()));
            y += 32;

            settings.Controls.Add(StudioUi.Label("Ziel / Vorlage:", 6, y));
            _cbPreset = StudioUi.Combo(110, y, 200,
                "Item (Art, max. 44x80)",
                "Großes Item (Art, max. 88x140)",
                "Bodentile (Land 44x44)",
                "Wand links (\\)",
                "Wand rechts (/)",
                "Gump Icon (max. 64x64)",
                "Gump frei (Originalgröße)");
            _cbPreset.SelectedIndexChanged += (s, e) => ApplyPreset();
            settings.Controls.Add(_cbPreset);
            y += 32;

            var grpBg = new GroupBox { Text = "Hintergrund", Left = 4, Top = y, Width = 306, Height = 78 };
            _cbBackground = StudioUi.Combo(8, 20, 170, "nicht entfernen", "vom Rand aus entfernen", "Farbe überall entfernen");
            _cbBackground.SelectedIndex = 1;
            _pnlKeyColor = new Panel { Left = 186, Top = 20, Width = 30, Height = 22, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _pnlKeyColor.Click += (s, e) => PickColor(_pnlKeyColor);
            grpBg.Controls.Add(_cbBackground);
            grpBg.Controls.Add(_pnlKeyColor);
            grpBg.Controls.Add(StudioUi.Label("Toleranz:", 8, 48));
            _numTolerance = StudioUi.Number(80, 48, 60, 0, 255, 30);
            grpBg.Controls.Add(_numTolerance);
            grpBg.Controls.Add(StudioUi.Label("(Farbe: Klick ins Original)", 146, 48));
            settings.Controls.Add(grpBg);
            y += 84;

            var grpCol = new GroupBox { Text = "Farben", Left = 4, Top = y, Width = 306, Height = 104 };
            grpCol.Controls.Add(StudioUi.Label("Helligkeit:", 8, 20));
            _numBrightness = StudioUi.Number(90, 20, 60, -100, 100, 0);
            grpCol.Controls.Add(_numBrightness);
            grpCol.Controls.Add(StudioUi.Label("Kontrast:", 156, 20));
            _numContrast = StudioUi.Number(230, 20, 60, -100, 100, 10);
            grpCol.Controls.Add(_numContrast);
            grpCol.Controls.Add(StudioUi.Label("Sättigung %:", 8, 48));
            _numSaturation = StudioUi.Number(90, 48, 60, 0, 300, 110);
            grpCol.Controls.Add(_numSaturation);
            _chkGray = StudioUi.Check("Graustufen (für Hues)", 156, 48, false);
            grpCol.Controls.Add(_chkGray);
            grpCol.Controls.Add(StudioUi.Label("Hue-Vorschau:", 8, 76));
            _numHuePreview = StudioUi.Number(90, 76, 60, 0, 3000, 0);
            grpCol.Controls.Add(_numHuePreview);
            grpCol.Controls.Add(StudioUi.Label("(0 = aus)", 156, 76));
            settings.Controls.Add(grpCol);
            y += 110;

            var grpSize = new GroupBox { Text = "Größe & Isometrie", Left = 4, Top = y, Width = 306, Height = 132 };
            grpSize.Controls.Add(StudioUi.Label("Max. Breite:", 8, 20));
            _numMaxW = StudioUi.Number(90, 20, 60, 1, 2000, 44);
            grpSize.Controls.Add(_numMaxW);
            grpSize.Controls.Add(StudioUi.Label("Höhe:", 156, 20));
            _numMaxH = StudioUi.Number(200, 20, 60, 1, 2000, 80);
            grpSize.Controls.Add(_numMaxH);
            grpSize.Controls.Add(StudioUi.Label("Skalierung:", 8, 48));
            _cbScale = StudioUi.Combo(90, 48, 120, "Weich (Bikubisch)", "Pixel (Nearest)", "Bilinear");
            grpSize.Controls.Add(_cbScale);
            _chkUpscale = StudioUi.Check("vergrößern", 216, 50, false);
            grpSize.Controls.Add(_chkUpscale);
            grpSize.Controls.Add(StudioUi.Label("Isometrie:", 8, 76));
            _cbIso = StudioUi.Combo(90, 76, 120, "keine", "Boden (Diamant)", "Wand links (\\)", "Wand rechts (/)");
            grpSize.Controls.Add(_cbIso);
            grpSize.Controls.Add(StudioUi.Label("Breite:", 214, 76));
            _numIsoWidth = StudioUi.Number(256, 76, 44, 8, 440, 44);
            grpSize.Controls.Add(_numIsoWidth);
            _chkAutoCrop = StudioUi.Check("Leeren Rand abschneiden", 8, 104, true);
            grpSize.Controls.Add(_chkAutoCrop);
            settings.Controls.Add(grpSize);
            y += 138;

            var grpPal = new GroupBox { Text = "Palette & Kanten", Left = 4, Top = y, Width = 306, Height = 132 };
            grpPal.Controls.Add(StudioUi.Label("Farbanzahl:", 8, 20));
            _numColors = StudioUi.Number(90, 20, 60, 2, 256, 32);
            grpPal.Controls.Add(_numColors);
            _chkDither = StudioUi.Check("Dithering", 160, 22, false);
            grpPal.Controls.Add(_chkDither);
            grpPal.Controls.Add(StudioUi.Label("Schärfen:", 8, 48));
            _numSharpen = StudioUi.Number(90, 48, 60, 0, 2, 0.3m, 1);
            grpPal.Controls.Add(_numSharpen);
            grpPal.Controls.Add(StudioUi.Label("Outline:", 8, 76));
            _cbOutline = StudioUi.Combo(90, 76, 120, "keine", "außen (1px)", "innen (Kante)");
            _cbOutline.SelectedIndex = 2;
            grpPal.Controls.Add(_cbOutline);
            _pnlOutlineColor = new Panel { Left = 216, Top = 76, Width = 30, Height = 22, BackColor = Color.FromArgb(24, 16, 8), BorderStyle = BorderStyle.FixedSingle, Cursor = Cursors.Hand };
            _pnlOutlineColor.Click += (s, e) => PickColor(_pnlOutlineColor);
            grpPal.Controls.Add(_pnlOutlineColor);
            _chkOutlineDiagonal = StudioUi.Check("Diagonal", 8, 104, false);
            grpPal.Controls.Add(_chkOutlineDiagonal);
            settings.Controls.Add(grpPal);
            y += 138;

            var grpOut = new GroupBox { Text = "Export", Left = 4, Top = y, Width = 306, Height = 170 };
            grpOut.Controls.Add(StudioUi.Label("ID (dez/0x):", 8, 22));
            _txtId = new TextBox { Left = 90, Top = 22, Width = 90, Text = "0x0" };
            grpOut.Controls.Add(_txtId);
            grpOut.Controls.Add(StudioUi.Button("Freie ID", 186, 20, 110, (s, e) => FindFreeId()));
            grpOut.Controls.Add(StudioUi.Button("Als Art-Item setzen", 8, 50, 140, (s, e) => ExportArt(false)));
            grpOut.Controls.Add(StudioUi.Button("Als Bodentile setzen", 152, 50, 144, (s, e) => ExportArt(true)));
            grpOut.Controls.Add(StudioUi.Button("Als Gump setzen", 8, 80, 140, (s, e) => ExportGump()));
            grpOut.Controls.Add(StudioUi.Button("PNG speichern...", 152, 80, 144, (s, e) => SavePng()));
            grpOut.Controls.Add(StudioUi.Button("Art-MUL speichern", 8, 110, 140, (s, e) => SaveArtFiles()));
            grpOut.Controls.Add(StudioUi.Button("Gump-MUL speichern", 152, 110, 144, (s, e) => SaveGumpFiles()));
            grpOut.Controls.Add(StudioUi.Button("Ergebnis kopieren", 8, 140, 140, (s, e) => { if (_result != null) { Clipboard.SetImage(_result); } }));
            settings.Controls.Add(grpOut);
            y += 176;

            settings.Controls.Add(StudioUi.Label("Zoom Ergebnis:", 6, y));
            _numZoom = StudioUi.Number(110, y, 60, 1, 16, 3);
            _numZoom.ValueChanged += (s, e) => _viewResult.Zoom = (int)_numZoom.Value;
            settings.Controls.Add(_numZoom);

            // all value changes re-process with a small delay
            foreach (Control c in AllControls(settings))
            {
                switch (c)
                {
                    case NumericUpDown n when n != _numZoom:
                        n.ValueChanged += (s, e) => Schedule();
                        break;
                    case CheckBox cb:
                        cb.CheckedChanged += (s, e) => Schedule();
                        break;
                    case ComboBox cmb when cmb != _cbPreset:
                        cmb.SelectedIndexChanged += (s, e) => Schedule();
                        break;
                }
            }

            var split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 400 };
            split.Panel1.Controls.Add(_viewSource);
            split.Panel2.Controls.Add(_viewResult);
            _viewSource.MouseClick += ViewSource_MouseClick;

            var statusStrip = new StatusStrip();
            statusStrip.Items.Add(_status);

            Controls.Add(split);
            Controls.Add(settings);
            Controls.Add(statusStrip);

            AllowDrop = true;
            DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            DragDrop += (s, e) =>
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
                {
                    LoadImage(files[0]);
                }
            };
        }

        private static IEnumerable<Control> AllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control cc in AllControls(c))
                {
                    yield return cc;
                }
            }
        }

        private void Schedule()
        {
            if (_loadingPreset)
            {
                return;
            }
            _debounce.Stop();
            _debounce.Start();
        }

        private void PickColor(Panel target)
        {
            using var dlg = new ColorDialog { Color = target.BackColor, FullOpen = true };
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                target.BackColor = dlg.Color;
                Schedule();
            }
        }

        private void ViewSource_MouseClick(object sender, MouseEventArgs e)
        {
            if (_source == null)
            {
                return;
            }
            Rectangle r = _viewSource.ImageRectangle;
            if (!r.Contains(e.Location))
            {
                return;
            }
            int x = (e.X - r.X) / _viewSource.Zoom;
            int y = (e.Y - r.Y) / _viewSource.Zoom;
            if (x < _source.Width && y < _source.Height)
            {
                _pnlKeyColor.BackColor = Color.FromArgb(255, _source.GetPixel(x, y));
                if (_cbBackground.SelectedIndex == 0)
                {
                    _cbBackground.SelectedIndex = 1;
                }
                Schedule();
            }
        }

        private void ApplyPreset()
        {
            _loadingPreset = true;
            try
            {
                switch (_cbPreset.SelectedIndex)
                {
                    case 0: SetSize(44, 80, 0, 32, 2); break;
                    case 1: SetSize(88, 140, 0, 48, 2); break;
                    case 2: SetSize(44, 44, 1, 48, 0); _numIsoWidth.Value = 44; break;
                    case 3: SetSize(200, 200, 2, 32, 2); _numIsoWidth.Value = 44; break;
                    case 4: SetSize(200, 200, 3, 32, 2); _numIsoWidth.Value = 44; break;
                    case 5: SetSize(64, 64, 0, 64, 1); break;
                    case 6: SetSize(2000, 2000, 0, 256, 0); break;
                }
                _chkAutoCrop.Checked = _cbPreset.SelectedIndex != 2;
            }
            finally
            {
                _loadingPreset = false;
            }
            Schedule();
        }

        private void SetSize(int w, int h, int iso, int colors, int outline)
        {
            _numMaxW.Value = w;
            _numMaxH.Value = h;
            _cbIso.SelectedIndex = iso;
            _numColors.Value = colors;
            _cbOutline.SelectedIndex = outline;
        }

        #endregion

        #region Load

        private void LoadImage()
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
                _status.Text = $"Geladen: {Path.GetFileName(file)} ({_source.Width}x{_source.Height})";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, "Bild konnte nicht geladen werden:\n" + ex.Message);
            }
        }

        private void LoadFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                StudioUi.Info(this, "Die Zwischenablage enthält kein Bild.");
                return;
            }
            using Image img = Clipboard.GetImage();
            SetSource(StudioImaging.To32(img));
            _status.Text = $"Aus Zwischenablage ({_source.Width}x{_source.Height})";
        }

        private void SetSource(Bitmap bmp)
        {
            _source?.Dispose();
            _source = bmp;
            // background colour guess: upper left pixel
            Color c = _source.GetPixel(0, 0);
            _pnlKeyColor.BackColor = c.A == 0 ? Color.White : Color.FromArgb(255, c);
            _viewSource.Zoom = Math.Max(1, Math.Min(4, 400 / Math.Max(_source.Width, _source.Height)));
            _viewSource.Image = _source;
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
                Bitmap img = new Bitmap(_source);

                img = Step(img, b => _cbBackground.SelectedIndex switch
                {
                    1 => StudioImaging.RemoveBackgroundFromEdges(b, _pnlKeyColor.BackColor, (int)_numTolerance.Value),
                    2 => StudioImaging.RemoveBackground(b, _pnlKeyColor.BackColor, (int)_numTolerance.Value),
                    _ => new Bitmap(b)
                });

                if (_chkAutoCrop.Checked)
                {
                    img = Step(img, b => StudioImaging.AutoCrop(b));
                }

                img = Step(img, b => StudioImaging.AdjustColors(b, (int)_numBrightness.Value, (int)_numContrast.Value, (int)_numSaturation.Value));

                if (_chkGray.Checked)
                {
                    img = Step(img, StudioImaging.ToGray);
                }

                var iso = (IsoMode)_cbIso.SelectedIndex;
                InterpolationMode mode = _cbScale.SelectedIndex switch
                {
                    1 => InterpolationMode.NearestNeighbor,
                    2 => InterpolationMode.HighQualityBilinear,
                    _ => InterpolationMode.HighQualityBicubic
                };

                if (iso == IsoMode.None)
                {
                    img = Step(img, b => StudioImaging.ResizeFit(b, (int)_numMaxW.Value, (int)_numMaxH.Value, mode, _chkUpscale.Checked));
                }
                else
                {
                    // make the texture square-ish before projecting
                    int iw = (int)_numIsoWidth.Value;
                    if (iso == IsoMode.Floor)
                    {
                        img = Step(img, b => StudioImaging.Resize(b, iw, iw, mode));
                    }
                    img = Step(img, b => StudioImaging.IsoProject(b, iso, iw));
                }

                if (_numSharpen.Value > 0)
                {
                    img = Step(img, b => StudioImaging.Sharpen(b, (double)_numSharpen.Value));
                }

                img = Step(img, b => StudioImaging.HardAlpha(b, 128));

                List<Color> palette = StudioImaging.BuildPalette(new[] { img }, Math.Max(2, (int)_numColors.Value - (_cbOutline.SelectedIndex > 0 ? 1 : 0)));
                img = Step(img, b => StudioImaging.ApplyPalette(b, palette, _chkDither.Checked));

                if (_cbOutline.SelectedIndex > 0)
                {
                    img = Step(img, b => StudioImaging.Outline(b, _pnlOutlineColor.BackColor, _cbOutline.SelectedIndex == 1, _chkOutlineDiagonal.Checked));
                }

                if (iso == IsoMode.Floor && _cbPreset.SelectedIndex == 2 && (img.Width != 44 || img.Height != 44))
                {
                    img = Step(img, b => StudioImaging.Resize(b, 44, 44, InterpolationMode.NearestNeighbor));
                }

                img = Step(img, StudioImaging.ToUoColors);

                int hueId = (int)_numHuePreview.Value;
                if (hueId > 0)
                {
                    Hue hue = StudioUi.GetHue(hueId);
                    img = Step(img, b => StudioImaging.ApplyHue(b, hue, !_chkGray.Checked));
                }

                _result?.Dispose();
                _result = img;
                _viewResult.Image = _result;
                _status.Text = $"Ergebnis: {_result.Width}x{_result.Height} Pixel, {StudioImaging.CountColors(_result)} Farben" +
                               (hueId > 0 ? $" (Hue {hueId} nur Vorschau)" : string.Empty);
            }
            catch (Exception ex)
            {
                _status.Text = "Fehler: " + ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
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
            if (_result == null)
            {
                StudioUi.Info(this, "Noch kein Ergebnis vorhanden. Bitte zuerst ein Bild laden.");
                return null;
            }
            if (_numHuePreview.Value > 0)
            {
                decimal old = _numHuePreview.Value;
                _numHuePreview.Value = 0;
                Process();
                Bitmap b = new Bitmap(_result);
                _numHuePreview.Value = old;
                Process();
                return b;
            }
            return new Bitmap(_result);
        }

        #endregion

        #region Export

        private bool GetId(out int id)
        {
            if (!StudioUi.TryParseId(_txtId.Text, out id) || id < 0)
            {
                StudioUi.Error(this, "Ungültige ID.");
                return false;
            }
            return true;
        }

        private void FindFreeId()
        {
            try
            {
                bool gump = _cbPreset.SelectedIndex >= 5;
                bool land = _cbPreset.SelectedIndex == 2;
                int max = gump ? Gumps.GetCount() : land ? 0x4000 : Art.GetMaxItemId() + 1;
                for (int i = 1; i < max; i++)
                {
                    bool used = gump ? Gumps.IsValidIndex(i) : land ? Art.IsValidLand(i) : Art.IsValidStatic(i);
                    if (!used)
                    {
                        _txtId.Text = $"0x{i:X4}";
                        _status.Text = $"Freie {(gump ? "Gump" : land ? "Land" : "Item")}-ID: 0x{i:X4} ({i})";
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
                _status.Text = $"{(land ? "Land" : "Item")} 0x{id:X4} gesetzt. Mit \"Art-MUL speichern\" in {StudioUi.OutputPath} schreiben.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void ExportGump()
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
                _status.Text = $"Gump 0x{id:X4} gesetzt. Mit \"Gump-MUL speichern\" in {StudioUi.OutputPath} schreiben.";
            }
            catch (Exception ex)
            {
                StudioUi.Error(this, ex.Message);
            }
        }

        private void SavePng()
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
            _status.Text = "Gespeichert: " + dlg.FileName;
        }

        private void SaveArtFiles()
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

        private void SaveGumpFiles()
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
            _debounce.Dispose();
            _source?.Dispose();
            _result?.Dispose();
            base.OnFormClosed(e);
        }
    }
}

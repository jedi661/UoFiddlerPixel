/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 * Light designer: new lights from templates and parameters (radius,
 * direction, opening angle, length …), mirror / rotate, a guide image
 * (e.g. a window item) to fit the light to, test like in the game, then
 * into light.mul, as PNG or as .uolight file. Existing lights can be
 * edited (size, rotation, brightness, position).
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class LightDesignerForm : Form
    {
        /// <summary>clipboard of designed lights – stays while UoFiddler runs</summary>
        private static readonly List<LightLibraryEntry> _store = new List<LightLibraryEntry>();

        private readonly Action<int> _onSaved;
        private LightImage _base;
        private LightImage _result;
        private Bitmap _guide;
        private string _guideName;
        private int _testLand = 0x3;
        private int _testItem = 0x0B20;
        private bool _loading;
        private bool _lastFlipH;
        private bool _lastFlipV;
        private int _lastRotate;
        private string _designName = "New light";

        // undo / redo: design + edited image
        private sealed class State
        {
            public LightDesign Design { get; set; }
            public LightImage Base { get; set; }
            public string Name { get; set; }
        }

        private readonly List<State> _undo = new List<State>();
        private readonly List<State> _redo = new List<State>();
        private State _current;
        private DateTime _lastPush;
        private bool _restoring;

        // "before": the light as it was when the designer was opened
        private LightImage _original;

        private Color _lightColor = Color.White;
        private double _flicker = 1;
        private readonly Random _random = new Random();

        // dragging on the preview
        private Point _dragStart;
        private MouseButtons _dragButton;
        private int _dragOffsetX, _dragOffsetY, _dragGuideX, _dragGuideY;
        private LightImage _dragBase;

        public LightDesignerForm()
            : this(null, 0, null)
        {
        }

        /// <param name="baseImage">light to edit (null = new light)</param>
        /// <param name="targetId">id in light.mul for "Into light.mul"</param>
        /// <param name="onSaved">called after a light was put into light.mul</param>
        public LightDesignerForm(LightImage baseImage, int targetId, Action<int> onSaved)
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _onSaved = onSaved;
            Icon = Options.GetFiddlerIcon();
            _loading = true;
            foreach (LightDesign preset in LightGenerator.Presets)
            {
                _lstPresets.Items.Add(preset.Name);
            }

            _cmbView.SelectedIndex = 0;
            _cmbZoom.SelectedIndex = 1;
            _cmbRotate.SelectedIndex = 0;
            _numTargetId.Maximum = Light.MaxCount - 1;
            _numTargetId.Value = Math.Max(0, Math.Min(Light.MaxCount - 1, targetId));
            FillStore();
            ShowDesign(LightGenerator.Presets[1]);
            _loading = false;

            if (baseImage != null)
            {
                _base = baseImage.Clone();
                _designName = $"Light {targetId}";
                Text = $"Light designer – light {targetId} (0x{targetId:X2})";
            }

            UpdateResult();
            _original = _result?.Clone();
            OnTargetChanged(this, EventArgs.Empty);
        }

        public void SetLightColor(Color color)
        {
            _lightColor = color;
            Render();
        }

        public void SetTestTiles(int land, int item)
        {
            _testLand = land;
            _testItem = item;
            UpdateResult();
        }

        #region Design <-> controls

        private static decimal Clamp(NumericUpDown n, int v) => Math.Max(n.Minimum, Math.Min(n.Maximum, v));

        private void ShowDesign(LightDesign d)
        {
            bool loading = _loading;
            _loading = true;
            _designName = d.Name;
            _cmbShape.SelectedIndex = (int)d.Shape;
            _numWidth.Value = Clamp(_numWidth, d.Width);
            _numHeight.Value = Clamp(_numHeight, d.Height);
            _numOffsetX.Value = Clamp(_numOffsetX, d.OffsetX);
            _numOffsetY.Value = Clamp(_numOffsetY, d.OffsetY);
            _numRadius.Value = Clamp(_numRadius, d.Radius);
            _numRadius2.Value = Clamp(_numRadius2, d.Radius2);
            _numAngle.Value = Clamp(_numAngle, d.Angle);
            _numSpread.Value = Clamp(_numSpread, d.Spread);
            _numLength.Value = Clamp(_numLength, d.Length);
            _numSourceWidth.Value = Clamp(_numSourceWidth, d.SourceWidth);
            _numIntensity.Value = Clamp(_numIntensity, d.Intensity);
            _numFalloff.Value = Clamp(_numFalloff, d.Falloff);
            _numSoftness.Value = Clamp(_numSoftness, d.Softness);
            _numNoise.Value = Clamp(_numNoise, d.Noise);
            _numSeed.Value = Clamp(_numSeed, d.Seed);
            _chkFlipH.Checked = _lastFlipH = d.FlipH;
            _chkFlipV.Checked = _lastFlipV = d.FlipV;
            _cmbRotate.SelectedIndex = (((d.Rotate % 360) + 360) % 360) / 90;
            _lastRotate = _cmbRotate.SelectedIndex;
            _loading = loading;
            UpdateShapeFields();
        }

        private LightDesign ReadDesign() => new LightDesign
        {
            Name = _designName,
            Shape = (LightShape)Math.Max(0, _cmbShape.SelectedIndex),
            Width = (int)_numWidth.Value,
            Height = (int)_numHeight.Value,
            OffsetX = (int)_numOffsetX.Value,
            OffsetY = (int)_numOffsetY.Value,
            Radius = (int)_numRadius.Value,
            Radius2 = (int)_numRadius2.Value,
            Angle = (int)_numAngle.Value,
            Spread = (int)_numSpread.Value,
            Length = (int)_numLength.Value,
            SourceWidth = (int)_numSourceWidth.Value,
            Intensity = (int)_numIntensity.Value,
            Falloff = (int)_numFalloff.Value,
            Softness = (int)_numSoftness.Value,
            Noise = (int)_numNoise.Value,
            Seed = (int)_numSeed.Value,
            FlipH = _chkFlipH.Checked,
            FlipV = _chkFlipV.Checked,
            Rotate = Math.Max(0, _cmbRotate.SelectedIndex) * 90
        };

        /// <summary>only the fields the shape uses are enabled</summary>
        private void UpdateShapeFields()
        {
            var shape = (LightShape)Math.Max(0, _cmbShape.SelectedIndex);
            bool radial = shape == LightShape.Radial || shape == LightShape.Ellipse || shape == LightShape.Ring;
            _numRadius.Enabled = radial;
            _numRadius2.Enabled = shape == LightShape.Ellipse || shape == LightShape.Ring;
            _numAngle.Enabled = !radial;
            _numSpread.Enabled = shape == LightShape.Cone || shape == LightShape.Window;
            _numLength.Enabled = !radial;
            _numSourceWidth.Enabled = shape == LightShape.Window || shape == LightShape.Beam;
            bool design = _base == null;
            _grpShape.Enabled = _grpLight.Enabled = design;
        }

        private void OnDesignChanged(object sender, EventArgs e)
        {
            if (_loading)
            {
                return;
            }

            // an edited image: mirror / rotate work on the image itself
            if (_base != null)
            {
                if (_chkFlipH.Checked != _lastFlipH)
                {
                    _base = LightGenerator.FlipHorizontal(_base);
                }

                if (_chkFlipV.Checked != _lastFlipV)
                {
                    _base = LightGenerator.FlipVertical(_base);
                }

                int steps = ((_cmbRotate.SelectedIndex - _lastRotate) + 4) % 4;
                for (int i = 0; i < steps; i++)
                {
                    _base = LightGenerator.Rotate90(_base);
                }
            }

            _lastFlipH = _chkFlipH.Checked;
            _lastFlipV = _chkFlipV.Checked;
            _lastRotate = Math.Max(0, _cmbRotate.SelectedIndex);
            UpdateShapeFields();
            UpdateResult();
        }

        private void OnPresetActivated(object sender, EventArgs e)
        {
            if (_lstPresets.SelectedIndex < 0)
            {
                return;
            }

            _base = null;
            ShowDesign(LightGenerator.Presets[_lstPresets.SelectedIndex].Clone());
            UpdateResult();
        }

        #endregion

        #region Result and preview

        private void UpdateResult()
        {
            if (_loading)
            {
                return;
            }

            LightDesign design = ReadDesign();
            _result = _base?.Clone() ?? LightGenerator.Generate(design);
            RememberState(design);
            UpdateShapeFields();
            Render();
        }

        #region Undo / redo

        private static bool SameState(State a, State b)
        {
            if (a == null || b == null)
            {
                return false;
            }

            string da = System.Text.Json.JsonSerializer.Serialize(a.Design), db = System.Text.Json.JsonSerializer.Serialize(b.Design);
            bool baseSame = (a.Base == null && b.Base == null) ||
                            (a.Base != null && b.Base != null && a.Base.Width == b.Base.Width && a.Base.Height == b.Base.Height && a.Base.Data.SequenceEqual(b.Base.Data));
            return da == db && baseSame;
        }

        /// <summary>a change becomes an undo step (several small changes within a second are one step)</summary>
        private void RememberState(LightDesign design)
        {
            var state = new State { Design = design.Clone(), Base = _base?.Clone(), Name = _designName };
            if (_restoring || SameState(state, _current))
            {
                _current = _restoring ? state : _current;
                UpdateUndoButtons();
                return;
            }

            if (_current != null)
            {
                // typing in a field or dragging with the mouse: one step
                bool dragging = _dragButton != MouseButtons.None;
                bool merge = (DateTime.Now - _lastPush).TotalSeconds < 1 && _undo.Count > 0 &&
                             ((state.Base == null && _current.Base == null) || dragging);
                if (!merge)
                {
                    _undo.Add(_current);
                    if (_undo.Count > 100)
                    {
                        _undo.RemoveAt(0);
                    }
                }

                _redo.Clear();
            }

            _lastPush = DateTime.Now;
            _current = state;
            UpdateUndoButtons();
        }

        private void UpdateUndoButtons()
        {
            _btnUndo.Enabled = _undo.Count > 0;
            _btnRedo.Enabled = _redo.Count > 0;
        }

        private void Restore(State state)
        {
            _restoring = true;
            try
            {
                _base = state.Base?.Clone();
                ShowDesign(state.Design);
                _designName = state.Name;
                UpdateResult();
            }
            finally
            {
                _restoring = false;
            }
        }

        private void OnClickUndo(object sender, EventArgs e)
        {
            if (_undo.Count == 0)
            {
                return;
            }

            _redo.Add(_current);
            State state = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Restore(state);
        }

        private void OnClickRedo(object sender, EventArgs e)
        {
            if (_redo.Count == 0)
            {
                return;
            }

            _undo.Add(_current);
            State state = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            Restore(state);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!(ActiveControl is TextBoxBase) && !(ActiveControl is NumericUpDown))
            {
                if (keyData == (Keys.Control | Keys.Z))
                {
                    OnClickUndo(this, EventArgs.Empty);
                    return true;
                }

                if (keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    OnClickRedo(this, EventArgs.Empty);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Before / after, colour, flicker

        private void OnCompareDown(object sender, MouseEventArgs e)
        {
            if (_original == null)
            {
                return;
            }

            LightImage now = _result;
            _result = _original;
            Render();
            _result = now;
        }

        private void OnCompareUp(object sender, MouseEventArgs e) => Render();

        private void OnClickColor(object sender, EventArgs e)
        {
            _colorDialog.Color = _lightColor;
            if (_colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                _lightColor = _colorDialog.Color;
                _cmbView.SelectedIndex = 1;
                Render();
            }
        }

        private void OnFlickerChanged(object sender, EventArgs e)
        {
            if (_chkFlicker.Checked)
            {
                _cmbView.SelectedIndex = 1;
                _flickerTimer.Start();
            }
            else
            {
                _flickerTimer.Stop();
                _flicker = 1;
                Render();
            }
        }

        private void OnFlickerTick(object sender, EventArgs e)
        {
            if (_cmbView.SelectedIndex != 1)
            {
                return;
            }

            _flicker = Math.Max(0.7, Math.Min(1.1, _flicker + ((_random.NextDouble() - 0.5) * 0.18) + ((0.9 - _flicker) * 0.2)));
            Render();
        }

        #endregion

        #region Dragging on the preview

        /// <summary>left button: moves the light source (edited image: the image), right button: the guide image</summary>
        private void OnPreviewMouseDown(object sender, MouseEventArgs e)
        {
            _dragStart = e.Location;
            _dragButton = e.Button;
            _dragOffsetX = (int)_numOffsetX.Value;
            _dragOffsetY = (int)_numOffsetY.Value;
            _dragGuideX = (int)_numGX.Value;
            _dragGuideY = (int)_numGY.Value;
            _dragBase = _base?.Clone();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragButton == MouseButtons.None || (e.Button & _dragButton) == 0)
            {
                return;
            }

            int dx = (e.X - _dragStart.X) / Zoom, dy = (e.Y - _dragStart.Y) / Zoom;
            if (_dragButton == MouseButtons.Right)
            {
                _loading = true;
                _numGX.Value = Clamp(_numGX, _dragGuideX + dx);
                _numGY.Value = Clamp(_numGY, _dragGuideY + dy);
                _loading = false;
                Render();
                return;
            }

            if (_dragBase != null)
            {
                _base = LightGenerator.Shift(_dragBase, dx, dy);
                UpdateResult();
                return;
            }

            _loading = true;
            _numOffsetX.Value = Clamp(_numOffsetX, _dragOffsetX + dx);
            _numOffsetY.Value = Clamp(_numOffsetY, _dragOffsetY + dy);
            _loading = false;
            UpdateResult();
        }

        private void OnPreviewMouseUp(object sender, MouseEventArgs e)
        {
            _dragButton = MouseButtons.None;
            _dragBase = null;
        }

        #endregion

        private LightPreviewOptions PreviewOptions() => new LightPreviewOptions
        {
            LandId = _testLand,
            ItemId = _testItem,
            Darkness = (int)_numTDark.Value,
            LightOffsetX = (int)_numTLightX.Value,
            LightOffsetY = (int)_numTLightY.Value,
            Guide = _chkShowGuide.Checked ? _guide : null,
            LightColor = _lightColor,
            Flicker = _flickerTimer.Enabled ? _flicker : 1,
            GuideX = (int)_numGX.Value,
            GuideY = (int)_numGY.Value,
            GuideScale = (int)_numGScale.Value,
            GuideOpacity = (int)_numGOpacity.Value
        };

        private int Zoom => Math.Max(1, _cmbZoom.SelectedIndex + 1);

        private void Render()
        {
            if (_result == null)
            {
                return;
            }

            Bitmap image;
            try
            {
                if (_cmbView.SelectedIndex == 1)
                {
                    var size = new Size(Math.Max(400, _result.Width + 160), Math.Max(400, _result.Height + 160));
                    image = LightPreview.RenderScene(size, _result, PreviewOptions());
                }
                else
                {
                    image = LightPreview.RenderLight(_result, PreviewOptions());
                }
            }
            catch (Exception ex)
            {
                _lblInfo.Text = ex.Message;
                return;
            }

            int zoom = Zoom;
            if (zoom > 1)
            {
                var scaled = new Bitmap(image.Width * zoom, image.Height * zoom);
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
                }

                image.Dispose();
                image = scaled;
            }

            Image old = _preview.Image;
            _preview.Image = image;
            old?.Dispose();

            // centered in the panel
            _preview.Left = Math.Max(0, (_pnlCenter.ClientSize.Width - image.Width) / 2);
            _preview.Top = Math.Max(0, (_pnlCenter.ClientSize.Height - image.Height) / 2);

            _lblInfo.Text = $"{_result.Width} × {_result.Height}   ·   {_result.LitPixels:N0} lit pixels   ·   brightest {_result.MaxValue}/31" +
                            (_base != null ? "   ·   edited image (design fields are off)" : string.Empty);
        }

        private void OnViewChanged(object sender, EventArgs e)
        {
            if (!_loading)
            {
                Render();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_preview?.Image != null)
            {
                _preview.Left = Math.Max(0, (_pnlCenter.ClientSize.Width - _preview.Image.Width) / 2);
                _preview.Top = Math.Max(0, (_pnlCenter.ClientSize.Height - _preview.Image.Height) / 2);
            }
        }

        #endregion

        #region Edit finished image

        private void OnClickEditApply(object sender, EventArgs e)
        {
            LightImage img = (_base ?? _result).Clone();
            if (sender == _btnERotateFree)
            {
                int deg = (int)_numERotateFree.Value;
                if (deg % 90 == 0)
                {
                    for (int r = ((deg % 360) + 360) % 360; r >= 90; r -= 90)
                    {
                        img = LightGenerator.Rotate90(img);
                    }
                }
                else
                {
                    img = LightGenerator.RotateFree(img, deg);
                }
            }
            else if (sender == _btnEScale)
            {
                int p = (int)_numEScale.Value;
                img = LightGenerator.Resize(img, img.Width * p / 100, img.Height * p / 100);
            }
            else if (sender == _btnEBright)
            {
                img = LightGenerator.Brightness(img, (int)_numEBright.Value);
            }
            else if (sender == _btnEShiftX || sender == _btnEShiftY)
            {
                img = LightGenerator.Shift(img, (int)_numEShiftX.Value, (int)_numEShiftY.Value);
            }

            _base = img;
            UpdateResult();
        }

        private void OnClickBackToDesign(object sender, EventArgs e)
        {
            if (_base == null)
            {
                return;
            }

            if (MessageBox.Show(this, "Forget the edited image and create the light from the design fields again?", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _base = null;
                UpdateResult();
            }
        }

        #endregion

        #region Guide image and test tiles

        private int Pick(TilePickerMode mode, int current, bool onlyLight)
        {
            using (var picker = new TilePickerForm(mode, current, onlyLight))
            {
                return picker.ShowDialog(this) == DialogResult.OK ? picker.SelectedId : -1;
            }
        }

        private void SetGuide(Bitmap bmp, string name)
        {
            _guide?.Dispose();
            _guide = bmp == null ? null : new Bitmap(bmp);
            _guideName = name;
            _lblGuide.Text = bmp == null ? "no guide image" : $"{name} ({bmp.Width} × {bmp.Height})";
            _chkShowGuide.Checked = bmp != null;
            Render();
        }

        private void OnClickGuideStatic(object sender, EventArgs e)
        {
            int id = Pick(TilePickerMode.Item, -1, false);
            if (id >= 0)
            {
                SetGuide(Art.GetStatic(id), $"item 0x{id:X4}");
            }
        }

        private void OnClickGuideLand(object sender, EventArgs e)
        {
            int id = Pick(TilePickerMode.Land, -1, false);
            if (id >= 0)
            {
                SetGuide(Art.GetLand(id), $"land 0x{id:X4}");
            }
        }

        private void OnClickGuideFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Images (*.png;*.bmp;*.tif;*.tiff;*.jpg;*.gif)|*.png;*.bmp;*.tif;*.tiff;*.jpg;*.gif" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (var bmp = new Bitmap(dialog.FileName))
                    {
                        SetGuide(bmp, Path.GetFileName(dialog.FileName));
                    }
                }
            }
        }

        private void OnClickGuideNone(object sender, EventArgs e) => SetGuide(null, null);

        private void OnClickTestLand(object sender, EventArgs e)
        {
            int id = Pick(TilePickerMode.Land, _testLand, false);
            if (id >= 0)
            {
                _testLand = id;
                _cmbView.SelectedIndex = 1;
                Render();
            }
        }

        private void OnClickTestItem(object sender, EventArgs e)
        {
            int id = Pick(TilePickerMode.Item, _testItem, true);
            if (id >= 0)
            {
                _testItem = id;
                _cmbView.SelectedIndex = 1;
                Render();
            }
        }

        #endregion

        #region light.mul, files

        private void OnTargetChanged(object sender, EventArgs e)
        {
            int id = (int)_numTargetId.Value;
            _lblTargetState.Text = id >= Light.Count ? $"new entry (the file grows to {id + 1})"
                : Light.TestLight(id) ? "used – will be replaced" : "free";
        }

        private void OnClickSaveMul(object sender, EventArgs e)
        {
            if (_result == null || _result.LitPixels == 0)
            {
                MessageBox.Show(this, "The light is empty.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = (int)_numTargetId.Value;
            if (Light.TestLight(id) &&
                MessageBox.Show(this, $"Light {id} (0x{id:X2}) exists. Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            if (id >= 255 &&
                MessageBox.Show(this, $"Light ids above 254 only work with clients/servers that read the light id as 16 bit value (ClassicUO). Continue?",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            Light.ReplaceRaw(id, _result.Data, _result.Width, _result.Height);
            Options.ChangedUltimaClass["Light"] = true;
            if (string.IsNullOrEmpty(LightNames.Get(id)) && !string.IsNullOrWhiteSpace(_designName))
            {
                LightNames.Set(id, _designName);
            }

            _onSaved?.Invoke(id);
            OnTargetChanged(this, EventArgs.Empty);
            MessageBox.Show(this, $"Light {id} (0x{id:X2}) is in light.mul now (in memory).\nWrite the files with \"Save light.mul\".", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "PNG (*.png)|*.png", FileName = LightLibrary.SafeFileName(_designName) + ".png" })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bmp = Light.ToBitmap(_result.Data, _result.Width, _result.Height))
                    {
                        bmp.Save(dialog.FileName, ImageFormat.Png);
                    }
                }
            }
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            Directory.CreateDirectory(LightLibrary.DefaultFolder);
            using (var dialog = new SaveFileDialog
            {
                Filter = $"Light (*{LightLibrary.Extension})|*{LightLibrary.Extension}",
                InitialDirectory = LightLibrary.DefaultFolder,
                FileName = LightLibrary.SafeFileName(_designName) + LightLibrary.Extension
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string name = Path.GetFileNameWithoutExtension(dialog.FileName);
                    LightLibrary.Save(LightLibraryEntry.From(name, _base == null ? ReadDesign() : null, _result), dialog.FileName);
                }
            }
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = $"Light or image (*{LightLibrary.Extension};*.png;*.bmp;*.tif;*.tiff)|*{LightLibrary.Extension};*.png;*.bmp;*.tif;*.tiff",
                InitialDirectory = Directory.Exists(LightLibrary.DefaultFolder) ? LightLibrary.DefaultFolder : string.Empty
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    if (dialog.FileName.EndsWith(LightLibrary.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        LoadEntry(LightLibrary.Load(dialog.FileName));
                    }
                    else
                    {
                        using (var bmp = new Bitmap(dialog.FileName))
                        {
                            _base = new LightImage(bmp.Width, bmp.Height, Light.FromBitmap(bmp));
                        }

                        _designName = Path.GetFileNameWithoutExtension(dialog.FileName);
                        UpdateResult();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>a stored light: with an unchanged design the design fields are used, otherwise the image</summary>
        private void LoadEntry(LightLibraryEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            LightImage image = entry.ToImage();
            if (entry.Design != null)
            {
                ShowDesign(entry.Design);
                LightImage generated = LightGenerator.Generate(entry.Design);
                bool same = image == null || (generated.Width == image.Width && generated.Height == image.Height && generated.Data.SequenceEqual(image.Data));
                _base = same ? null : image;
            }
            else
            {
                _base = image;
            }

            _designName = entry.Name;
            UpdateResult();
        }

        #endregion

        #region Clipboard / library

        private void FillStore()
        {
            _lstStore.Items.Clear();
            foreach (LightLibraryEntry e in _store)
            {
                _lstStore.Items.Add(e);
            }
        }

        private void OnClickStoreAdd(object sender, EventArgs e)
        {
            if (_result == null)
            {
                return;
            }

            string name = $"{_designName} {DateTime.Now:HH:mm:ss}";
            _store.Add(LightLibraryEntry.From(name, _base == null ? ReadDesign() : null, _result));
            FillStore();
            _lstStore.SelectedIndex = _lstStore.Items.Count - 1;
        }

        private void OnClickStoreLoad(object sender, EventArgs e)
        {
            if (_lstStore.SelectedItem is LightLibraryEntry entry)
            {
                LoadEntry(entry);
            }
        }

        private void OnClickStoreRemove(object sender, EventArgs e)
        {
            if (_lstStore.SelectedItem is LightLibraryEntry entry)
            {
                _store.Remove(entry);
                FillStore();
            }
        }

        private void OnClickStoreSaveAll(object sender, EventArgs e)
        {
            if (_store.Count == 0)
            {
                return;
            }

            using (var dialog = new FolderBrowserDialog { SelectedPath = LightLibrary.DefaultFolder, Description = "Folder for the lights (.uolight)" })
            {
                Directory.CreateDirectory(LightLibrary.DefaultFolder);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                foreach (LightLibraryEntry entry in _store)
                {
                    string file = Path.Combine(dialog.SelectedPath, LightLibrary.SafeFileName(entry.Name.Replace(':', '-')) + LightLibrary.Extension);
                    LightLibrary.Save(entry, file);
                }

                if (MessageBox.Show(this, $"{_store.Count} light(s) saved. Open the folder?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(dialog.SelectedPath) { UseShellExecute = true });
                }
            }
        }

        private void OnClickLibLoad(object sender, EventArgs e)
        {
            Directory.CreateDirectory(LightLibrary.DefaultFolder);
            using (var dialog = new FolderBrowserDialog { SelectedPath = LightLibrary.DefaultFolder, Description = "Folder with .uolight files" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                List<LightLibraryEntry> loaded = LightLibrary.LoadFolder(dialog.SelectedPath);
                _store.AddRange(loaded);
                FillStore();
                if (loaded.Count == 0)
                {
                    MessageBox.Show(this, "No .uolight files in this folder.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        private void OnClickClose(object sender, EventArgs e) => Close();

        private void OnDesignerClosing(object sender, FormClosingEventArgs e)
        {
            _flickerTimer.Stop();
            _preview.Image?.Dispose();
            _preview.Image = null;
            _guide?.Dispose();
        }
    }
}

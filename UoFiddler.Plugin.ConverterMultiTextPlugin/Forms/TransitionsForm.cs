﻿/***************************************************************************
 *
 * $Author: Prapilk
 * Built-in : Nikodemus
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.AnimEdit;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.TextureCutter;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.Transitions;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Transitions between two textures (e.g. meadow → road): 12 pieces made from masks.
    /// Masks are generated, loaded, or learned from existing UO transitions; the result is saved
    /// as files, into texmaps.mul / art.mul (+ tiledata) and as TilesBrush.xml for CentrED+.
    /// </summary>
    public partial class TransitionsForm : Form
    {
        private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");
        private const int SlotCount = 12;

        private Bitmap _texA, _texB, _learnT;
        private readonly Bitmap[] _masks = new Bitmap[SlotCount];
        private readonly Bitmap[] _results = new Bitmap[SlotCount];
        private readonly Bitmap[] _tiles = new Bitmap[SlotCount];
        private MaskSettings _settings = new MaskSettings();
        private bool _updating;
        private int[] _landIds = Array.Empty<int>();
        private List<TransitionMasks.LibraryEntry> _library = new List<TransitionMasks.LibraryEntry>();
        private string _lastXml;

        // views
        private SlotGrid _grid;
        private ComboBox _comboView;
        private Panel _detail;
        private Label _lblDetail;
        private TerrainView _assembled;
        private Panel _assembledPanel;
        private ComboBox _comboPattern, _comboPatternSize;
        private RadioButton _radioIso;
        private CheckBox _chkAsmGrid;
        private int _patternSeed = 1;

        // textures tab
        private PictureBox _picA, _picB;
        private TextBox _txtNameA, _txtNameB, _txtBrushA, _txtBrushB, _txtLandsA, _txtLandsB, _txtTexIdA, _txtTexIdB, _txtLandIdA, _txtLandIdB;
        private ComboBox _comboSize;

        // masks tab
        private NumericUpDown _numSoft, _numRough, _numSeed, _numShift, _numCorner, _numBlur, _numContrast;
        private CheckBox _chkLive;
        private PictureBox _picMask;

        // learn tab
        private TextBox _txtFilter, _txtTexT;
        private CheckBox _chkWithTexture;
        private ArtTileGrid _gridLand;
        private Panel _panelLand;
        private Label _lblLand;
        private PictureBox _picT;
        private RadioButton _radioAverage;
        private NumericUpDown _numDetail;
        private string[] _landNames;
        private int[] _landTextures;

        // library tab
        private TextBox _txtLibCategory, _txtLibName;
        private ComboBox _comboLibFilter;
        private ArtTileGrid _gridLib;

        // output tab
        private TextBox _txtFolder, _txtTexStart, _txtArtStart;
        private ComboBox _comboFormat, _comboRoot;
        private CheckBox _chkFiles, _chkTex, _chkArt, _chkTileData, _chkSkipUsed;

        public TransitionsForm()
        {
            InitializeComponent();
            BuildViews();
            BuildTexturesTab();
            BuildMasksTab();
            BuildLearnTab();
            BuildLibraryTab();
            BuildOutputTab();
            tabControl1.SelectedIndexChanged += (s, e) =>
            {
                if (tabControl1.SelectedTab == tabLearn && _landNames == null)
                {
                    ReloadLand();
                }
                if (tabControl1.SelectedTab == tabLibrary)
                {
                    ReloadLibrary();
                }
            };

            GenerateMasks(false);
            SetMessage("Load texture A and B (Textures tab) – the 12 transitions appear at once. F1 = help.");
        }

        #region Helpers

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            row.Controls.AddRange(controls);
            return row;
        }

        private static Label Lbl(string text, int width = 0) => width > 0
            ? new Label { Text = text, Width = width, Margin = new Padding(3, 7, 3, 0) }
            : new Label { Text = text, AutoSize = true, Margin = new Padding(3, 7, 3, 0) };

        private Button Btn(string text, EventHandler click, string tip = null)
        {
            var b = new Button { Text = text, AutoSize = true, Height = 26, UseVisualStyleBackColor = true, Margin = new Padding(2) };
            b.Click += click;
            if (tip != null)
            {
                toolTip1.SetToolTip(b, tip);
            }
            return b;
        }

        private static NumericUpDown Num(int min, int max, int value, int width = 55) =>
            new NumericUpDown { Minimum = min, Maximum = max, Value = Math.Max(min, Math.Min(max, value)), Width = width, Margin = new Padding(3, 3, 3, 0) };

        private static TextBox Txt(int width, string text = "") => new TextBox { Width = width, Text = text, Margin = new Padding(3, 4, 3, 0) };

        private static CheckBox Chk(string text, bool value = false) => new CheckBox { Text = text, AutoSize = true, Checked = value, Margin = new Padding(3, 5, 3, 0) };

        private static GroupBox Group(string title, params Control[] rows)
        {
            var box = new GroupBox { Text = title, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(6, 4, 6, 6), Margin = new Padding(3, 3, 3, 6) };
            var flow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Fill };
            flow.Controls.AddRange(rows);
            box.Controls.Add(flow);
            return box;
        }

        private static void FillTab(TabPage page, params Control[] groups)
        {
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            flow.Controls.AddRange(groups);
            page.Controls.Add(flow);
        }

        private static void Replace(ref Bitmap field, Bitmap value)
        {
            Bitmap old = field;
            field = value;
            if (old != null && !ReferenceEquals(old, value))
            {
                old.Dispose();
            }
        }

        private static void ShowThumb(PictureBox box, Bitmap bmp)
        {
            Image old = box.Image;
            box.Image = bmp == null ? null : CutterImaging.To32Bpp(bmp);
            old?.Dispose();
        }

        private int OutputSize => _comboSize?.SelectedIndex == 1 ? 128 : 64;

        private int Selected => _grid.Selected;

        private Bitmap ReadClipboard()
        {
            try
            {
                Bitmap bmp = CutterImaging.GetClipboardImage();
                if (bmp == null)
                {
                    SetMessage("The clipboard contains no image.");
                }
                return bmp;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The clipboard could not be read:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private Bitmap ReadFile(string title)
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = title })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }
                try
                {
                    return CutterImaging.LoadBitmap(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private static Bitmap TextureFromLand(int landId, out int textureId)
        {
            textureId = -1;
            try
            {
                lock (SdkSync.Lock)
                {
                    int tex = TileData.LandTable[landId].TextureId;
                    if (tex > 0 && Textures.TestTexture(tex))
                    {
                        textureId = tex;
                        return CutterImaging.To32Bpp(Textures.GetTexture(tex));
                    }
                }
            }
            catch (Exception)
            {
                // tiledata / texmaps missing
            }
            Bitmap art = CutterArt.GetCopy(ArtKind.Land, landId);
            if (art == null)
            {
                return null;
            }
            using (art)
            {
                return TextureImaging.LandTileToTexture(art, 64);
            }
        }

        private static string LandName(int landId)
        {
            try
            {
                lock (SdkSync.Lock)
                {
                    return TileData.LandTable[landId].Name?.Trim() ?? string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion

        #region Views (pieces, detail, assembled preview)

        private void BuildViews()
        {
            _grid = new SlotGrid { Dock = DockStyle.Fill };
            _grid.ImageProvider = SlotImage;
            _grid.SelectedChanged += (s, e) => { UpdateDetail(); ShowMaskThumb(); };
            _comboView = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _comboView.Items.AddRange(new object[] { "Textures", "Land tiles (diamond)", "Masks" });
            _comboView.SelectedIndex = 0;
            _comboView.SelectedIndexChanged += (s, e) => { _grid.Checker = _comboView.SelectedIndex == 1; _grid.Invalidate(); };
            var gridTop = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            gridTop.Controls.AddRange(new Control[]
            {
                Lbl("Show:"), _comboView,
                Btn("? Help", (s, e) => TransitionsHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de")),
                Lbl("A_ = brush A (B comes in), B_ = brush B (A comes in). Arrow keys select.")
            });
            splitTop.Panel1.Controls.Add(_grid);
            splitTop.Panel1.Controls.Add(gridTop);

            _detail = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 48) };
            _detail.Paint += Detail_Paint;
            _detail.Resize += (s, e) => _detail.Invalidate();
            _lblDetail = new Label { Dock = DockStyle.Top, Height = 36, ForeColor = Color.Gainsboro, BackColor = Color.FromArgb(45, 45, 48), Padding = new Padding(4) };
            splitTop.Panel2.Controls.Add(_detail);
            splitTop.Panel2.Controls.Add(_lblDetail);

            _comboPattern = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
            _comboPattern.Items.AddRange(TransitionMasks.PatternNames);
            _comboPattern.SelectedIndex = 0;
            _comboPatternSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50 };
            _comboPatternSize.Items.AddRange(new object[] { "8", "12", "16", "24" });
            _comboPatternSize.SelectedIndex = 1;
            _radioIso = new RadioButton { Text = "3D", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            var top = new RadioButton { Text = "2D", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            _chkAsmGrid = Chk("Grid");
            _comboPattern.SelectedIndexChanged += (s, e) => RenderAssembled();
            _comboPatternSize.SelectedIndexChanged += (s, e) => RenderAssembled();
            _radioIso.CheckedChanged += (s, e) => RenderAssembled();
            _chkAsmGrid.CheckedChanged += (s, e) => RenderAssembled();
            var asmTop = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32 };
            asmTop.Controls.AddRange(new Control[]
            {
                Lbl("Assembled:"), _comboPattern, _comboPatternSize, _radioIso, top, _chkAsmGrid,
                Btn("New random", (s, e) => { _patternSeed++; RenderAssembled(); }),
                Btn("Save PNG...", (s, e) => SaveAssembled())
            });
            toolTip1.SetToolTip(_comboPattern, Tip("The 12 pieces are put together automatically like CentrED+ does – every seam becomes visible.",
                "Die 12 Teile werden automatisch zusammengesetzt wie in CentrED+ – jede Naht wird sichtbar."));
            _assembled = new TerrainView();
            _assembledPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(30, 30, 30) };
            _assembledPanel.Controls.Add(_assembled);
            _assembledPanel.Resize += (s, e) => CenterAssembled();
            splitRight.Panel2.Controls.Add(_assembledPanel);
            splitRight.Panel2.Controls.Add(asmTop);
        }

        private Image SlotImage(int i)
        {
            switch (_comboView.SelectedIndex)
            {
                case 1: return _tiles[i];
                case 2: return _masks[i];
                default: return _results[i];
            }
        }

        private void UpdateDetail()
        {
            TransitionSlot slot = TransitionMasks.Slots[Selected];
            string ids = _landIds.Length == SlotCount && _landIds[Selected] >= 0 ? $"   land 0x{_landIds[Selected]:X4}" : string.Empty;
            _lblDetail.Text = $"{slot.Name} – {slot.Description}{ids}\nTexture · land tile · mask";
            _detail.Invalidate();
        }

        private void Detail_Paint(object sender, PaintEventArgs e)
        {
            int i = Selected;
            Rectangle area = Rectangle.Inflate(_detail.ClientRectangle, -6, -6);
            int s = Math.Max(20, Math.Min(area.Width, (area.Height - 12) / 3));
            var boxes = new[]
            {
                new Rectangle(area.X + (area.Width - s) / 2, area.Y, s, s),
                new Rectangle(area.X + (area.Width - s) / 2, area.Y + s + 6, s, s),
                new Rectangle(area.X + (area.Width - s) / 2, area.Y + 2 * (s + 6), s, s)
            };
            Image[] images = { _results[i], _tiles[i], _masks[i] };
            for (int k = 0; k < 3; k++)
            {
                CutterImaging.DrawChecker(e.Graphics, boxes[k]);
                if (images[k] != null)
                {
                    CutterImaging.DrawFitted(e.Graphics, images[k], boxes[k], true);
                }
            }
        }

        private void CenterAssembled()
        {
            Size c = _assembledPanel.ClientSize;
            Point sc = _assembledPanel.AutoScrollPosition;
            _assembled.Location = new Point(Math.Max(0, (c.Width - _assembled.Width) / 2) + sc.X, Math.Max(0, (c.Height - _assembled.Height) / 2) + sc.Y);
        }

        /// <summary>Puts the pieces together on a small map (lake, road, ...) the way CentrED+ chooses them.</summary>
        private void RenderAssembled()
        {
            if (_texA == null || _texB == null || _results.Any(r => r == null))
            {
                _assembled.Image?.Dispose();
                _assembled.Image = null;
                return;
            }
            int n = int.Parse((string)_comboPatternSize.SelectedItem);
            bool[,] isB = TransitionMasks.MakePattern(_comboPattern.SelectedIndex, n, _patternSeed);
            var kinds = new List<TerrainKind>();
            try
            {
                TerrainKind Kind(Bitmap tex, string name)
                {
                    var k = new TerrainKind { Name = name };
                    k.SetImage(TextureImaging.Resize(tex, OutputSize, OutputSize, TextureImaging.ResizeQuality.Smooth));
                    return k;
                }
                kinds.Add(Kind(_texA, "A"));
                kinds.Add(Kind(_texB, "B"));
                for (int i = 0; i < SlotCount; i++)
                {
                    kinds.Add(Kind(_results[i], TransitionMasks.Slots[i].Name));
                }
                var scene = new TerrainScene(n);
                for (int y = 0; y < n; y++)
                {
                    for (int x = 0; x < n; x++)
                    {
                        int piece = TransitionMasks.PieceFor(isB, x, y);
                        scene.Kind[y * n + x] = piece == -1 ? 0 : piece == -2 ? 1 : piece + 2;
                    }
                }
                var options = new TerrainRenderOptions { Isometric = _radioIso.Checked, Grid = _chkAsmGrid.Checked, Light = false, CellSize2D = n <= 12 ? 40 : 28 };
                Bitmap bmp = TerrainRenderer.Render(scene, kinds, options);
                Bitmap old = _assembled.Image;
                _assembled.Image = bmp;
                old?.Dispose();
                CenterAssembled();
            }
            finally
            {
                kinds.ForEach(k => k.Dispose());
            }
        }

        private void SaveAssembled()
        {
            if (_assembled.Image == null)
            {
                return;
            }
            using (var d = new SaveFileDialog { Filter = "PNG|*.png", FileName = "TransitionPreview_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") })
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _assembled.Image.Save(d.FileName, ImageFormat.Png);
                }
            }
        }

        #endregion

        #region Results

        private void ScheduleUpdate()
        {
            updateTimer.Stop();
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            updateTimer.Stop();
            UpdateResults();
        }

        /// <summary>Mixes A and B with every mask.</summary>
        private void UpdateResults()
        {
            int size = OutputSize;
            for (int i = 0; i < SlotCount; i++)
            {
                Bitmap result = null, tile = null;
                if (_texA != null && _texB != null && _masks[i] != null)
                {
                    result = TransitionMasks.Blend(_texA, _texB, _masks[i], size, (int)_numBlur.Value, (int)_numContrast.Value);
                    tile = TextureImaging.TextureToLandTile(result, false);
                }
                Replace(ref _results[i], result);
                Replace(ref _tiles[i], tile);
            }
            _grid.Invalidate();
            UpdateDetail();
            RenderAssembled();
        }

        #endregion

        #region Textures tab

        private void BuildTexturesTab()
        {
            GroupBox Side(string title, bool a)
            {
                var pic = new PictureBox { Width = 96, Height = 96, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(3) };
                TextBox name = Txt(120, a ? "Grass" : "Sand"), brush = Txt(60, a ? "0001" : "0002"), lands = Txt(170), texId = Txt(70), landId = Txt(70);
                toolTip1.SetToolTip(lands, Tip("Land tile IDs of the pure texture for the XML, e.g. 0x0003, 0x0004", "Landkachel-IDs der reinen Textur für die XML, z. B. 0x0003, 0x0004"));
                toolTip1.SetToolTip(brush, Tip("Brush ID in CentrED+ (4 digits).", "Brush-ID in CentrED+ (4 Stellen)."));
                Action<Bitmap, string> set = (bmp, n) => SetTexture(a, bmp, n);
                var buttons = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
                buttons.Controls.Add(Btn("File...", (s, e) => set(ReadFile("Texture " + (a ? "A" : "B")), null)));
                buttons.Controls.Add(Btn("Clipboard", (s, e) => set(ReadClipboard(), null)));
                if (a)
                {
                    _picA = pic; _txtNameA = name; _txtBrushA = brush; _txtLandsA = lands; _txtTexIdA = texId; _txtLandIdA = landId;
                }
                else
                {
                    _picB = pic; _txtNameB = name; _txtBrushB = brush; _txtLandsB = lands; _txtTexIdB = texId; _txtLandIdB = landId;
                }
                return Group(title,
                    Row(pic, buttons),
                    Row(Lbl("texmaps ID:", 75), texId, Btn("Load", (s, e) => LoadTextureId(a))),
                    Row(Lbl("land ID:", 75), landId, Btn("Load", (s, e) => LoadLandId(a), Tip("Takes the texture of the land tile (tiledata) or turns the land tile into a texture.",
                        "Nimmt die Textur der Landkachel (tiledata) oder macht aus der Landkachel eine Textur."))),
                    Row(Lbl("Name:", 75), name, Lbl("Brush:"), brush),
                    Row(Lbl("Land IDs:", 75), lands));
            }

            _comboSize = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboSize.Items.AddRange(new object[] { "64", "128" });
            _comboSize.SelectedIndex = 0;
            _comboSize.SelectedIndexChanged += (s, e) => { GenerateMasksIfLive(); ScheduleUpdate(); };
            GroupBox size = Group("Output",
                Row(Lbl("Texture size:"), _comboSize, Btn("⇄ Swap A / B", (s, e) => SwapAB())));
            FillTab(tabTextures, Side("Texture A (e.g. meadow)", true), Side("Texture B (e.g. road)", false), size);
        }

        private void SetTexture(bool a, Bitmap bmp, string name)
        {
            if (bmp == null)
            {
                return;
            }
            if (a)
            {
                Replace(ref _texA, bmp);
                ShowThumb(_picA, bmp);
                if (!string.IsNullOrEmpty(name))
                {
                    _txtNameA.Text = name;
                }
            }
            else
            {
                Replace(ref _texB, bmp);
                ShowThumb(_picB, bmp);
                if (!string.IsNullOrEmpty(name))
                {
                    _txtNameB.Text = name;
                }
            }
            ScheduleUpdate();
            SetMessage($"Texture {(a ? "A" : "B")} set ({bmp.Width}×{bmp.Height}).");
        }

        private void LoadTextureId(bool a)
        {
            if (!CutterImaging.TryParseId((a ? _txtTexIdA : _txtTexIdB).Text, out int id))
            {
                SetMessage("Enter a texture ID (hex 0x1A2B or decimal).");
                return;
            }
            Bitmap bmp = TextureMul.GetCopy(id);
            if (bmp == null)
            {
                SetMessage($"Texture 0x{id:X4} is empty or texmaps.mul is not loaded.");
                return;
            }
            List<int> lands = TextureMul.LandTilesUsing(id);
            if (lands.Count > 0)
            {
                (a ? _txtLandsA : _txtLandsB).Text = string.Join(", ", lands.Take(8).Select(l => $"0x{l:X4}"));
            }
            SetTexture(a, bmp, lands.Count > 0 ? LandName(lands[0]) : null);
        }

        private void LoadLandId(bool a)
        {
            if (!CutterImaging.TryParseId((a ? _txtLandIdA : _txtLandIdB).Text, out int id) || id < 0 || id > 0x3FFF)
            {
                SetMessage("Enter a land tile ID (0 – 0x3FFF).");
                return;
            }
            Bitmap bmp = TextureFromLand(id, out int texId);
            if (bmp == null)
            {
                SetMessage($"Land tile 0x{id:X4} is empty or art.mul is not loaded.");
                return;
            }
            (a ? _txtLandsA : _txtLandsB).Text = $"0x{id:X4}";
            if (texId >= 0)
            {
                (a ? _txtTexIdA : _txtTexIdB).Text = $"0x{texId:X4}";
            }
            SetTexture(a, bmp, LandName(id));
        }

        private void SwapAB()
        {
            (_texA, _texB) = (_texB, _texA);
            ShowThumb(_picA, _texA);
            ShowThumb(_picB, _texB);
            foreach ((TextBox x, TextBox y) in new[] { (_txtNameA, _txtNameB), (_txtBrushA, _txtBrushB), (_txtLandsA, _txtLandsB), (_txtTexIdA, _txtTexIdB), (_txtLandIdA, _txtLandIdB) })
            {
                (x.Text, y.Text) = (y.Text, x.Text);
            }
            ScheduleUpdate();
        }

        #endregion

        #region Masks tab

        private void BuildMasksTab()
        {
            _numSoft = Num(0, 100, _settings.Softness);
            _numRough = Num(0, 100, _settings.Roughness);
            _numSeed = Num(0, 99999, _settings.Seed, 70);
            _numShift = Num(-40, 40, _settings.EdgeShift);
            _numCorner = Num(20, 100, _settings.CornerSize);
            _numBlur = Num(0, 8, 0);
            _numContrast = Num(-100, 100, 0);
            _chkLive = Chk("Live: generate all masks when a value changes", true);
            foreach (NumericUpDown n in new[] { _numSoft, _numRough, _numSeed, _numShift, _numCorner })
            {
                n.ValueChanged += (s, e) => { if (!_updating) GenerateMasksIfLive(); };
            }
            _numBlur.ValueChanged += (s, e) => ScheduleUpdate();
            _numContrast.ValueChanged += (s, e) => ScheduleUpdate();
            toolTip1.SetToolTip(_numSoft, Tip("Width of the soft border (0 = hard edge).", "Breite des weichen Übergangs (0 = harte Kante)."));
            toolTip1.SetToolTip(_numRough, Tip("How much the border wanders – natural, irregular edges. The pattern repeats with the tile, so pieces fit together.",
                "Wie stark die Kante wandert – natürlich, unregelmäßig. Das Muster wiederholt sich mit der Kachel, die Teile passen zusammen."));
            toolTip1.SetToolTip(_numShift, Tip("Moves the border: negative = more A, positive = more B.", "Verschiebt die Kante: negativ = mehr A, positiv = mehr B."));
            toolTip1.SetToolTip(_numCorner, Tip("Shape of the corners: 20–25 = cut off straight, 50 = round, 100 = almost square. Corners always meet the edges in the middle, so the pieces fit.",
                "Form der Ecken: 20–25 = gerade abgeschnitten, 50 = rund, 100 = fast eckig. Ecken treffen die Kanten immer in der Mitte, die Teile passen zusammen."));
            toolTip1.SetToolTip(_numBlur, Tip("Blurs the masks when mixing (old \"Transition blur\").", "Weichzeichnen der Masken beim Mischen (früher \"Transition blur\")."));
            toolTip1.SetToolTip(_numContrast, Tip("Harder (+) or softer (−) border when mixing (old \"Contrast\").", "Härtere (+) oder weichere (−) Kante beim Mischen (früher \"Contrast\")."));

            GroupBox gen = Group("Generator",
                Row(Lbl("Softness:", 70), _numSoft, Lbl("Rough:"), _numRough),
                Row(Lbl("Pattern:", 70), _numSeed, Btn("New", (s, e) => _numSeed.Value = new Random().Next(0, 99999))),
                Row(Lbl("Border:", 70), _numShift, Lbl("Corners:"), _numCorner),
                _chkLive,
                Row(Btn("Generate all 12", (s, e) => GenerateMasks(true)), Btn("Generate selected", (s, e) => GenerateSelected())));

            GroupBox mix = Group("Mixing (all pieces)", Row(Lbl("Blur px:", 70), _numBlur, Lbl("Contrast:"), _numContrast));

            _picMask = new PictureBox { Width = 96, Height = 96, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(3), BackColor = Color.Black };
            GroupBox selected = Group("Selected mask (white = A, black = B)",
                Row(_picMask, new FlowLayoutPanel
                {
                    AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                    Controls =
                    {
                        Btn("Load file...", (s, e) => SetMask(Selected, ReadFile("Mask " + TransitionMasks.Slots[Selected].Name), true)),
                        Btn("Paste", (s, e) => SetMask(Selected, ReadClipboard(), true)),
                        Btn("Copy", (s, e) => { if (_masks[Selected] != null) CutterImaging.CopyToClipboard(_masks[Selected]); })
                    }
                }),
                Row(Btn("Invert", (s, e) => ModifyMask(TransitionMasks.Invert)), Btn("Smooth", (s, e) => ModifyMask(m => TransitionMasks.SmoothMask(m, 1))),
                    Btn("Mirror", (s, e) => ModifyMask(m => Flip(m, RotateFlipType.RotateNoneFlipX))), Btn("Rotate", (s, e) => ModifyMask(m => Flip(m, RotateFlipType.Rotate90FlipNone)))));

            GroupBox files = Group("Mask files (old \"Selecting alpha\")",
                Row(Btn("Load folder (A_DR ... B_UL)...", (s, e) => LoadMaskFolder(), Tip("Images whose names start with A_DR, A_DL, A_UU, A_LL, A_UR, A_UL, B_... – like the old tool.",
                    "Bilder, deren Namen mit A_DR, A_DL, A_UU, A_LL, A_UR, A_UL, B_... beginnen – wie im alten Tool."))),
                Row(Btn("Save all masks to folder...", (s, e) => SaveMaskFolder())));

            FillTab(tabMasks, gen, mix, selected, files);
        }

        private static Bitmap Flip(Bitmap m, RotateFlipType type)
        {
            Bitmap copy = CutterImaging.To32Bpp(m);
            copy.RotateFlip(type);
            return copy;
        }

        private MaskSettings ReadSettings() => new MaskSettings
        {
            Softness = (int)_numSoft.Value,
            Roughness = (int)_numRough.Value,
            Seed = (int)_numSeed.Value,
            EdgeShift = (int)_numShift.Value,
            CornerSize = (int)_numCorner.Value,
            Blur = (int)_numBlur.Value,
            Contrast = (int)_numContrast.Value
        };

        private void ApplySettings(MaskSettings s)
        {
            _updating = true;
            _numSoft.Value = s.Softness;
            _numRough.Value = s.Roughness;
            _numSeed.Value = Math.Max(0, Math.Min(99999, s.Seed));
            _numShift.Value = Math.Max(-40, Math.Min(40, s.EdgeShift));
            _numCorner.Value = Math.Max(20, Math.Min(100, s.CornerSize));
            _numBlur.Value = Math.Max(0, Math.Min(8, s.Blur));
            _numContrast.Value = Math.Max(-100, Math.Min(100, s.Contrast));
            _updating = false;
        }

        private void GenerateMasksIfLive()
        {
            if (_chkLive != null && _chkLive.Checked)
            {
                GenerateMasks(false);
            }
        }

        private void GenerateMasks(bool message)
        {
            _settings = ReadSettings();
            for (int i = 0; i < SlotCount; i++)
            {
                Replace(ref _masks[i], TransitionMasks.Generate(TransitionMasks.Slots[i], _settings, OutputSize));
            }
            ShowMaskThumb();
            ScheduleUpdate();
            if (message)
            {
                SetMessage("12 masks generated.");
            }
        }

        private void GenerateSelected()
        {
            _settings = ReadSettings();
            SetMask(Selected, TransitionMasks.Generate(TransitionMasks.Slots[Selected], _settings, OutputSize), false);
        }

        /// <summary>Puts a mask into a slot. fromOutside: loaded / learned masks switch the live generator off.</summary>
        private void SetMask(int slot, Bitmap mask, bool fromOutside)
        {
            if (mask == null)
            {
                return;
            }
            if (fromOutside && _chkLive.Checked)
            {
                _chkLive.Checked = false;
            }
            Replace(ref _masks[slot], mask);
            ShowMaskThumb();
            ScheduleUpdate();
        }

        private void ModifyMask(Func<Bitmap, Bitmap> f)
        {
            if (_masks[Selected] == null)
            {
                return;
            }
            SetMask(Selected, f(_masks[Selected]), true);
        }

        private void ShowMaskThumb()
        {
            if (_picMask != null)
            {
                ShowThumb(_picMask, _masks[Selected]);
            }
        }

        private void LoadMaskFolder()
        {
            using (var d = new FolderBrowserDialog { Description = "Folder with the masks A_DR ... B_UL", UseDescriptionForTitle = true })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                LoadMasks(d.SelectedPath);
            }
        }

        private void LoadMasks(string folder)
        {
            Bitmap[] masks = TransitionMasks.LoadMasksFromFolder(folder, out int found);
            if (found == 0)
            {
                MessageBox.Show(this, "No masks found. The file names must start with A_DR, A_DL, A_UU, A_LL, A_UR, A_UL or B_...", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _chkLive.Checked = false;
            for (int i = 0; i < SlotCount; i++)
            {
                if (masks[i] != null)
                {
                    Replace(ref _masks[i], masks[i]);
                }
            }
            ShowMaskThumb();
            ScheduleUpdate();
            SetMessage($"{found} of 12 masks loaded from {folder}" + (found < SlotCount ? " – the others stay as they were." : "."));
        }

        private void SaveMaskFolder()
        {
            using (var d = new FolderBrowserDialog { Description = "Folder for the 12 masks", UseDescriptionForTitle = true })
            {
                if (d.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                for (int i = 0; i < SlotCount; i++)
                {
                    _masks[i]?.Save(Path.Combine(d.SelectedPath, TransitionMasks.Slots[i].Name + ".png"), ImageFormat.Png);
                }
                SetMessage("Masks saved in " + d.SelectedPath);
            }
        }

        #endregion

        #region Learn tab

        private void BuildLearnTab()
        {
            _txtFilter = Txt(120);
            _chkWithTexture = Chk("Only with texture", true);
            _txtFilter.TextChanged += (s, e) => RebuildLandList();
            _chkWithTexture.CheckedChanged += (s, e) => RebuildLandList();
            toolTip1.SetToolTip(_txtFilter, Tip("Part of the tiledata name, e.g. grass, sand, dirt, cobble. Or an ID.", "Teil des tiledata-Namens, z. B. grass, sand, dirt, cobble. Oder eine ID."));

            _gridLand = new ArtTileGrid { Width = 360, Height = 260, Text = "art.mul / tiledata not loaded." };
            _gridLand.ImageProvider = id => CutterArt.GetShared(ArtKind.Land, id);
            _gridLand.LabelProvider = id => _landNames != null ? $"{id:X4} {_landNames[id]}" : $"0x{id:X4}";
            _gridLand.SelectionChanged += (s, e) => { UpdateLandInfo(); _panelLand.Invalidate(); };

            _panelLand = new Panel { Width = 360, Height = 100, BackColor = Color.FromArgb(45, 45, 48) };
            _panelLand.Paint += PanelLand_Paint;
            _lblLand = new Label { Width = 360, Height = 32 };

            GroupBox browser = Group("UO land tiles and their textures (tiledata)",
                Row(Lbl("Filter:"), _txtFilter, _chkWithTexture, Btn("⟳", (s, e) => ReloadLand())),
                _gridLand, _lblLand, _panelLand,
                Row(Btn("→ A", (s, e) => TakeLand(0)), Btn("→ B", (s, e) => TakeLand(1)), Btn("→ T (analyse)", (s, e) => TakeLand(2),
                    Tip("Use this tile as the transition to learn from.", "Diese Kachel als Übergang nehmen, von dem gelernt wird."))));

            _picT = new PictureBox { Width = 72, Height = 72, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(3) };
            _txtTexT = Txt(70);
            _radioAverage = new RadioButton { Text = "Average colors", AutoSize = true, Checked = true, Margin = new Padding(3, 6, 3, 0) };
            var exact = new RadioButton { Text = "Pixel exact", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            toolTip1.SetToolTip(_radioAverage, Tip("Compares T with the average colors of A and B. Best when T was made from other pixels.",
                "Vergleicht T mit den Durchschnittsfarben von A und B. Am besten, wenn T aus anderen Pixeln gemacht wurde."));
            toolTip1.SetToolTip(exact, Tip("Compares pixel by pixel. Best when T was made exactly from A and B.", "Vergleicht Pixel für Pixel. Am besten, wenn T genau aus A und B gemacht wurde."));
            _numDetail = Num(0, 6, 2);
            toolTip1.SetToolTip(_numDetail, Tip("Blur before comparing: higher = calmer mask, lower = more detail.", "Weichzeichnen vor dem Vergleich: höher = ruhigere Maske, niedriger = mehr Details."));
            GroupBox learn = Group("Learn the mask of a transition T",
                Row(_picT, new FlowLayoutPanel
                {
                    AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false,
                    Controls =
                    {
                        Row(Lbl("texmaps ID:"), _txtTexT, Btn("Load", (s, e) => TakeTextureT())),
                        Row(Btn("File...", (s, e) => SetT(ReadFile("Transition T"))), Btn("Clipboard", (s, e) => SetT(ReadClipboard())))
                    }
                }),
                Row(_radioAverage, exact, Lbl("Detail:"), _numDetail),
                Row(Btn("Extract → selected piece", (s, e) => ExtractMask(false)),
                    Btn("Extract → best fitting piece", (s, e) => ExtractMask(true), Tip("Compares the learned mask with the 12 piece shapes and puts it where it fits best.",
                        "Vergleicht die gelernte Maske mit den 12 Teilformen und legt sie dort ab, wo sie am besten passt."))));

            FillTab(tabLearn, browser, learn);
        }

        private void ReloadLand()
        {
            if (!CutterArt.IsAvailable)
            {
                _landNames = null;
                _gridLand.Items = Array.Empty<int>();
                _lblLand.Text = "art.mul is not loaded.";
                return;
            }
            Cursor.Current = Cursors.WaitCursor;
            _landNames = new string[0x4000];
            _landTextures = new int[0x4000];
            bool[] used = CutterArt.ScanUsed(ArtKind.Land);
            try
            {
                lock (SdkSync.Lock)
                {
                    for (int i = 0; i < 0x4000; i++)
                    {
                        _landNames[i] = TileData.LandTable[i].Name?.Trim() ?? string.Empty;
                        int tex = TileData.LandTable[i].TextureId;
                        _landTextures[i] = tex > 0 && Textures.TestTexture(tex) ? tex : -1;
                    }
                }
            }
            catch (Exception)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    _landNames[i] = _landNames[i] ?? string.Empty;
                }
            }
            for (int i = 0; i < 0x4000; i++)
            {
                if (i >= used.Length || !used[i])
                {
                    _landNames[i] = null;
                }
            }
            Cursor.Current = Cursors.Default;
            RebuildLandList();
        }

        private void RebuildLandList()
        {
            if (_landNames == null)
            {
                return;
            }
            string f = _txtFilter.Text.Trim();
            bool idFilter = CutterImaging.TryParseId(f, out int fid) && f.Length > 0 && (f.StartsWith("0x", StringComparison.OrdinalIgnoreCase) || f.All(char.IsDigit));
            var list = new List<int>();
            for (int i = 0; i < _landNames.Length; i++)
            {
                if (_landNames[i] == null || (_chkWithTexture.Checked && _landTextures[i] < 0))
                {
                    continue;
                }
                if (f.Length > 0 && !(idFilter ? i == fid : _landNames[i].IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    continue;
                }
                list.Add(i);
            }
            _gridLand.Items = list;
            UpdateLandInfo();
        }

        private void UpdateLandInfo()
        {
            int id = _gridLand.SelectedItem;
            if (id < 0 || _landNames == null)
            {
                _lblLand.Text = _landNames == null ? string.Empty : $"{_gridLand.Items.Count} land tiles";
                return;
            }
            int tex = _landTextures[id];
            _lblLand.Text = $"Land 0x{id:X4} \"{_landNames[id]}\"" + (tex >= 0 ? $" – texture 0x{tex:X4}" : " – no texture") + $"\n{_gridLand.Items.Count} land tiles in the list";
        }

        private void PanelLand_Paint(object sender, PaintEventArgs e)
        {
            int id = _gridLand.SelectedItem;
            if (id < 0)
            {
                return;
            }
            try
            {
                Bitmap art = CutterArt.GetShared(ArtKind.Land, id);
                var left = new Rectangle(8, 6, 88, 88);
                if (art != null)
                {
                    CutterImaging.DrawFitted(e.Graphics, art, left, true);
                }
                if (_landTextures != null && _landTextures[id] >= 0)
                {
                    Bitmap tex = TextureMul.GetShared(_landTextures[id]);
                    if (tex != null)
                    {
                        CutterImaging.DrawFitted(e.Graphics, tex, new Rectangle(110, 6, 88, 88), true);
                    }
                }
                TextRenderer.DrawText(e.Graphics, "land tile · texture", Font, new Rectangle(210, 40, 140, 20), Color.Gainsboro);
            }
            catch (Exception)
            {
                // in use
            }
        }

        private void TakeLand(int target)
        {
            int id = _gridLand.SelectedItem;
            if (id < 0)
            {
                SetMessage("Select a land tile first.");
                return;
            }
            Bitmap bmp = TextureFromLand(id, out int texId);
            if (bmp == null)
            {
                SetMessage($"Land tile 0x{id:X4} has no graphic.");
                return;
            }
            switch (target)
            {
                case 0:
                    _txtLandIdA.Text = $"0x{id:X4}";
                    _txtLandsA.Text = $"0x{id:X4}";
                    _txtTexIdA.Text = texId >= 0 ? $"0x{texId:X4}" : string.Empty;
                    SetTexture(true, bmp, _landNames?[id]);
                    break;
                case 1:
                    _txtLandIdB.Text = $"0x{id:X4}";
                    _txtLandsB.Text = $"0x{id:X4}";
                    _txtTexIdB.Text = texId >= 0 ? $"0x{texId:X4}" : string.Empty;
                    SetTexture(false, bmp, _landNames?[id]);
                    break;
                default:
                    _txtTexT.Text = texId >= 0 ? $"0x{texId:X4}" : string.Empty;
                    SetT(bmp);
                    break;
            }
        }

        private void TakeTextureT()
        {
            if (!CutterImaging.TryParseId(_txtTexT.Text, out int id) || TextureMul.GetCopy(id) is not Bitmap bmp)
            {
                SetMessage("Texture not found.");
                return;
            }
            SetT(bmp);
        }

        private void SetT(Bitmap bmp)
        {
            if (bmp == null)
            {
                return;
            }
            Replace(ref _learnT, bmp);
            ShowThumb(_picT, bmp);
            SetMessage($"Transition T set ({bmp.Width}×{bmp.Height}). Now \"Extract\" – A and B must be the two pure textures.");
        }

        private void ExtractMask(bool bestSlot)
        {
            if (_texA == null || _texB == null || _learnT == null)
            {
                MessageBox.Show(this, "Set the pure textures A and B and the transition T first.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Bitmap learned;
            double reliability;
            using (Bitmap raw = TransitionMasks.Extract(_texA, _texB, _learnT, _radioAverage.Checked, (int)_numDetail.Value, out reliability))
            {
                learned = TextureImaging.Resize(raw, OutputSize, OutputSize, TextureImaging.ResizeQuality.Smooth);
            }
            int slot = Selected;
            if (bestSlot)
            {
                double best = double.MaxValue;
                var plain = new MaskSettings { Roughness = 0, Softness = 40 };
                int[] lp = CutterImaging.GetPixels(learned);
                for (int i = 0; i < SlotCount; i++)
                {
                    using (Bitmap shape = TransitionMasks.Generate(TransitionMasks.Slots[i], plain, OutputSize))
                    {
                        int[] sp = CutterImaging.GetPixels(shape);
                        double diff = 0;
                        for (int k = 0; k < lp.Length; k++)
                        {
                            int d = (lp[k] & 0xFF) - (sp[k] & 0xFF);
                            diff += d * d;
                        }
                        if (diff < best)
                        {
                            best = diff;
                            slot = i;
                        }
                    }
                }
                _grid.Selected = slot;
            }
            SetMask(slot, learned, true);
            string quality = reliability > 0.6 ? "good" : reliability > 0.3 ? "medium – A and B are similar in color" : "weak – A and B have nearly the same colors";
            SetMessage($"Mask learned into {TransitionMasks.Slots[slot].Name}. Reliability: {quality}. Tip: save the set in the Library.");
        }

        #endregion

        #region Library tab

        private void BuildLibraryTab()
        {
            _comboLibFilter = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
            _comboLibFilter.SelectedIndexChanged += (s, e) => RebuildLibraryList();
            _gridLib = new ArtTileGrid { Width = 360, Height = 330, CellSize = new Size(116, 70), Text = "No patterns yet.\nSave the current masks below.\n\nNoch keine Muster.\nUnten die aktuellen Masken speichern." };
            _gridLib.ImageProvider = i => i >= 0 && i < _library.Count ? _library[i].Preview : null;
            _gridLib.LabelProvider = i => i >= 0 && i < _library.Count ? _library[i].Name : string.Empty;
            _gridLib.ItemActivated += (s, e) => LoadLibraryEntry();
            _txtLibCategory = Txt(150, "Roads");
            _txtLibName = Txt(150, "Soft edge");
            toolTip1.SetToolTip(_txtLibCategory, Tip("Category, e.g. Roads, Meadow, Cobblestone, Coast.", "Kategorie, z. B. Straßen, Wiese, Pflaster, Küste."));

            GroupBox list = Group("Patterns (mask sets)",
                Row(Lbl("Category:"), _comboLibFilter, Btn("⟳", (s, e) => ReloadLibrary())),
                _gridLib,
                Row(Btn("Load", (s, e) => LoadLibraryEntry()), Btn("Delete", (s, e) => DeleteLibraryEntry()), Btn("Open folder", (s, e) => OpenFolder(TransitionMasks.LibraryRoot))));
            GroupBox save = Group("Save the current masks as pattern",
                Row(Lbl("Category:", 65), _txtLibCategory),
                Row(Lbl("Name:", 65), _txtLibName),
                Row(Btn("Save pattern", (s, e) => SaveLibraryEntry(), Tip("Saves the 12 masks, the generator values and a preview.", "Speichert die 12 Masken, die Generator-Werte und eine Vorschau."))));
            FillTab(tabLibrary, list, save);
        }

        private void ReloadLibrary()
        {
            foreach (TransitionMasks.LibraryEntry e in _library)
            {
                e.Preview?.Dispose();
            }
            _library = TransitionMasks.ListPatterns();
            string keep = _comboLibFilter.SelectedItem as string;
            _comboLibFilter.Items.Clear();
            _comboLibFilter.Items.Add("(all)");
            foreach (string c in _library.Select(l => l.Category).Distinct())
            {
                _comboLibFilter.Items.Add(c);
            }
            _comboLibFilter.SelectedItem = keep != null && _comboLibFilter.Items.Contains(keep) ? keep : "(all)";
            RebuildLibraryList();
        }

        private void RebuildLibraryList()
        {
            string cat = _comboLibFilter.SelectedItem as string;
            _gridLib.Items = Enumerable.Range(0, _library.Count).Where(i => cat == null || cat == "(all)" || _library[i].Category == cat).ToList();
        }

        private Bitmap MakeLibraryPreview()
        {
            const int cell = 40;
            var bmp = new Bitmap(6 * cell, 2 * cell, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                for (int i = 0; i < SlotCount; i++)
                {
                    Bitmap img = _results[i] ?? _masks[i];
                    if (img != null)
                    {
                        g.DrawImage(img, new Rectangle((i % 6) * cell, (i / 6) * cell, cell - 1, cell - 1));
                    }
                }
            }
            return bmp;
        }

        private void SaveLibraryEntry()
        {
            if (_masks.Any(m => m == null))
            {
                SetMessage("Not all 12 masks exist.");
                return;
            }
            string folder = Path.Combine(TransitionMasks.LibraryRoot, _txtLibCategory.Text.Trim(), _txtLibName.Text.Trim());
            if (Directory.Exists(folder) &&
                MessageBox.Show(this, $"The pattern \"{_txtLibName.Text}\" already exists in \"{_txtLibCategory.Text}\". Replace it?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                using (Bitmap preview = MakeLibraryPreview())
                {
                    string saved = TransitionMasks.SavePattern(_txtLibCategory.Text, _txtLibName.Text, _masks, ReadSettings(), preview);
                    SetMessage("Pattern saved: " + saved);
                }
                ReloadLibrary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private TransitionMasks.LibraryEntry SelectedLibraryEntry()
        {
            int i = _gridLib.SelectedItem;
            if (i >= 0 && i < _library.Count)
            {
                return _library[i];
            }
            SetMessage("Select a pattern first.");
            return null;
        }

        private void LoadLibraryEntry()
        {
            TransitionMasks.LibraryEntry entry = SelectedLibraryEntry();
            if (entry == null)
            {
                return;
            }
            MaskSettings s = TransitionMasks.LoadSettings(entry.Folder);
            if (s != null)
            {
                ApplySettings(s);
            }
            LoadMasks(entry.Folder);
            _txtLibCategory.Text = entry.Category;
            _txtLibName.Text = entry.Name;
            SetMessage($"Pattern \"{entry.Name}\" ({entry.Category}) loaded.");
        }

        private void DeleteLibraryEntry()
        {
            TransitionMasks.LibraryEntry entry = SelectedLibraryEntry();
            if (entry == null || MessageBox.Show(this, $"Delete the pattern \"{entry.Name}\"?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                entry.Preview?.Dispose();
                entry.Preview = null;
                Directory.Delete(entry.Folder, true);
                ReloadLibrary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Deleting failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Output tab

        private void BuildOutputTab()
        {
            _txtFolder = Txt(250);
            _comboFormat = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
            _comboFormat.Items.AddRange(new object[] { "BMP", "PNG", "TIFF" });
            _comboFormat.SelectedIndex = 0;
            _chkFiles = Chk("Save files: textures, land tiles, XML", true);
            _chkTex = Chk("Write textures into texmaps.mul from ID");
            _chkArt = Chk("Write land tiles into art.mul from ID");
            _chkTileData = Chk("Link land tile → texture in tiledata (name + texture ID)", true);
            _chkSkipUsed = Chk("Skip used IDs", true);
            _txtTexStart = Txt(80);
            _txtArtStart = Txt(80);
            _comboRoot = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 110 };
            _comboRoot.Items.AddRange(new object[] { "TilesBrush", "transition" });
            _comboRoot.SelectedIndex = 0;
            toolTip1.SetToolTip(_txtFolder, Tip("Empty = a new folder in tempGrafic.", "Leer = ein neuer Ordner in tempGrafic."));
            toolTip1.SetToolTip(_txtArtStart, Tip("Also used for the land IDs in the XML when nothing is written into art.mul.", "Wird auch für die Land-IDs in der XML benutzt, wenn nichts in art.mul geschrieben wird."));
            toolTip1.SetToolTip(_comboRoot, Tip("Root element of the XML. CentrED+ uses TilesBrush; \"transition\" is the format of the old tool.",
                "Wurzel-Element der XML. CentrED+ benutzt TilesBrush; \"transition\" ist das Format des alten Tools."));
            toolTip1.SetToolTip(_chkTileData, Tip("Only when textures and land tiles are both written: the new land tiles point to their textures, like UO does.",
                "Nur wenn Texturen und Landkacheln beide geschrieben werden: die neuen Landkacheln zeigen auf ihre Texturen, wie in UO."));

            GroupBox files = Group("Files",
                _chkFiles,
                Row(Lbl("Folder:"), _txtFolder, Btn("...", (s, e) => ChooseFolder())),
                Row(Lbl("Format:"), _comboFormat, Btn("Open tempGrafic", (s, e) => OpenFolder(OutputDirectory))));
            GroupBox mul = Group("Into the client files (in memory – save in UoFiddler afterwards)",
                Row(_chkTex, _txtTexStart, Btn("Free", (s, e) => FindFree(true))),
                Row(_chkArt, _txtArtStart, Btn("Free", (s, e) => FindFree(false))),
                _chkSkipUsed, _chkTileData,
                Row(Btn("Save texmaps.mul", (s, e) => SaveMul(true)), Btn("Save art.mul", (s, e) => SaveMul(false))));
            GroupBox xml = Group("CentrED+ XML",
                Row(Lbl("Root:"), _comboRoot, Btn("Show / edit XML", (s, e) => ShowXml())));
            var go = new Button { Text = "Create transitions", Width = 340, Height = 36, Font = new Font(Font, FontStyle.Bold), UseVisualStyleBackColor = true };
            go.Click += (s, e) => CreateTransitions();
            FillTab(tabOutput, files, mul, xml, go);
        }

        private void ChooseFolder()
        {
            using (var d = new FolderBrowserDialog())
            {
                if (d.ShowDialog(this) == DialogResult.OK)
                {
                    _txtFolder.Text = d.SelectedPath;
                }
            }
        }

        private static void OpenFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);
                Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // explorer missing
            }
        }

        private void FindFree(bool texture)
        {
            int count = SlotCount;
            if (texture)
            {
                int id = TextureMul.FindFreeBlock(1, count);
                _txtTexStart.Text = id >= 0 ? $"0x{id:X4}" : string.Empty;
                SetMessage(id >= 0 ? $"Free texture IDs 0x{id:X4} – 0x{id + count - 1:X4}" : "No 12 free texture IDs in a row (texmaps loaded?).");
            }
            else
            {
                int id = CutterArt.FindFreeBlock(ArtKind.Land, 1, count);
                _txtArtStart.Text = id >= 0 ? $"0x{id:X4}" : string.Empty;
                SetMessage(id >= 0 ? $"Free land IDs 0x{id:X4} – 0x{id + count - 1:X4}" : "No 12 free land IDs in a row (art.mul loaded?).");
            }
        }

        private static List<int> ParseIds(string text)
        {
            var list = new List<int>();
            foreach (string part in (text ?? string.Empty).Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (CutterImaging.TryParseId(part, out int id))
                {
                    list.Add(id);
                }
            }
            return list;
        }

        private string BuildXml(IReadOnlyList<int> landIds) => TransitionMasks.BuildXml((string)_comboRoot.SelectedItem,
            _txtBrushA.Text.Trim(), _txtNameA.Text.Trim(), ParseIds(_txtLandsA.Text),
            _txtBrushB.Text.Trim(), _txtNameB.Text.Trim(), ParseIds(_txtLandsB.Text), landIds);

        /// <summary>Planned land IDs: from the art.mul start ID on (or 0 when empty).</summary>
        private int[] PlannedLandIds()
        {
            CutterImaging.TryParseId(_txtArtStart.Text, out int start);
            return Enumerable.Range(start, SlotCount).ToArray();
        }

        private void CreateTransitions()
        {
            if (_results.Any(r => r == null))
            {
                MessageBox.Show(this, "Load texture A and B first – then the 12 transitions exist.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!_chkFiles.Checked && !_chkTex.Checked && !_chkArt.Checked)
            {
                MessageBox.Show(this, "Choose at least one target.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int texStart = 0, artStart = 0;
            if (_chkTex.Checked && (!TextureMul.IsAvailable || !CutterImaging.TryParseId(_txtTexStart.Text, out texStart)))
            {
                MessageBox.Show(this, "texmaps.mul must be loaded and a start ID entered (or \"Free\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_chkArt.Checked && (!CutterArt.IsAvailable || !CutterImaging.TryParseId(_txtArtStart.Text, out artStart)))
            {
                MessageBox.Show(this, "art.mul must be loaded and a start ID entered (or \"Free\").", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if ((_chkTex.Checked || _chkArt.Checked) &&
                MessageBox.Show(this, "The 12 pieces are written into the loaded client files (in memory). Continue?", Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            var report = new List<string>();
            int[] texIds = Enumerable.Repeat(-1, SlotCount).ToArray();
            int[] landIds = PlannedLandIds();
            var tiles = new List<Bitmap>();
            try
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    tiles.Add(TextureImaging.TextureToLandTile(_results[i], true));
                }

                if (_chkTex.Checked)
                {
                    List<int> ids = TextureMul.WriteMany(texStart, _results, _chkSkipUsed.Checked, this);
                    for (int i = 0; i < ids.Count; i++)
                    {
                        texIds[i] = ids[i];
                    }
                    report.Add($"{ids.Count} textures → texmaps.mul" + (ids.Count > 0 ? $" (0x{ids[0]:X4} – 0x{ids[ids.Count - 1]:X4})" : string.Empty));
                }
                if (_chkArt.Checked)
                {
                    List<int> ids = CutterArt.WriteMany(ArtKind.Land, artStart, tiles, _chkSkipUsed.Checked, this);
                    landIds = Enumerable.Repeat(-1, SlotCount).ToArray();
                    for (int i = 0; i < ids.Count; i++)
                    {
                        landIds[i] = ids[i];
                    }
                    report.Add($"{ids.Count} land tiles → art.mul" + (ids.Count > 0 ? $" (0x{ids[0]:X4} – 0x{ids[ids.Count - 1]:X4})" : string.Empty));
                }
                if (_chkTex.Checked && _chkArt.Checked && _chkTileData.Checked)
                {
                    int linked = 0;
                    lock (SdkSync.Lock)
                    {
                        for (int i = 0; i < SlotCount; i++)
                        {
                            if (landIds[i] >= 0 && texIds[i] >= 0)
                            {
                                TileData.LandTable[landIds[i]].TextureId = (ushort)texIds[i];
                                string name = (TransitionMasks.Slots[i].IsA ? _txtNameA.Text : _txtNameB.Text).Trim();
                                TileData.LandTable[landIds[i]].Name = name.Length > 20 ? name.Substring(0, 20) : name;
                                linked++;
                            }
                        }
                    }
                    UoFiddler.Controls.Classes.Options.ChangedUltimaClass["TileData"] = true;
                    report.Add($"{linked} land tiles linked to their textures in tiledata (save tiledata.mul)");
                }

                _landIds = landIds;
                _lastXml = BuildXml(landIds);

                if (_chkFiles.Checked)
                {
                    string folder = string.IsNullOrWhiteSpace(_txtFolder.Text)
                        ? Path.Combine(OutputDirectory, $"Transitions_{Safe(_txtNameA.Text)}_{Safe(_txtNameB.Text)}_{DateTime.Now:yyyyMMdd_HHmmss}")
                        : _txtFolder.Text.Trim();
                    Directory.CreateDirectory(folder);
                    string ext = _comboFormat.SelectedIndex == 1 ? ".png" : _comboFormat.SelectedIndex == 2 ? ".tiff" : ".bmp";
                    for (int i = 0; i < SlotCount; i++)
                    {
                        string id = landIds[i] >= 0 ? $"_0x{landIds[i]:X4}" : string.Empty;
                        CutterImaging.SaveImage(_results[i], Path.Combine(folder, $"{TransitionMasks.Slots[i].Name}{id}{ext}"), TransparentFill.Keep);
                        CutterImaging.SaveImage(tiles[i], Path.Combine(folder, $"{TransitionMasks.Slots[i].Name}{id}_land{ext}"), TransparentFill.Keep);
                        _masks[i]?.Save(Path.Combine(folder, $"mask_{TransitionMasks.Slots[i].Name}.png"), ImageFormat.Png);
                    }
                    File.WriteAllText(Path.Combine(folder, "transition.xml"), _lastXml);
                    _txtFolder.Tag = folder;
                    report.Add("Files and transition.xml saved in " + folder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Creating the transitions failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                tiles.ForEach(t => t.Dispose());
            }
            UpdateDetail();
            SetMessage(string.Join(" · ", report));
            MessageBox.Show(this, string.Join("\n", report), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string Safe(string text)
        {
            string s = string.IsNullOrWhiteSpace(text) ? "X" : text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }
            return s;
        }

        private void SaveMul(bool texmaps)
        {
            try
            {
                string path = texmaps ? TextureMul.Save() : CutterArt.Save();
                SetMessage((texmaps ? "texmaps.mul" : "art.mul") + " saved in " + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>The XML in an editor window (old "XML Edit"): change, save, copy.</summary>
        private void ShowXml()
        {
            string xml = _lastXml ?? BuildXml(_landIds.Length == SlotCount ? _landIds : PlannedLandIds());
            var form = new Form { Text = "transition.xml (CentrED+)", Size = new Size(820, 640), StartPosition = FormStartPosition.CenterParent, ShowIcon = false, TopMost = TopMost };
            var box = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, Dock = DockStyle.Fill, Font = new Font("Consolas", 10f), Text = xml.Replace("\r\n", "\n").Replace("\n", "\r\n") };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36 };
            var save = new Button { Text = "Save as...", AutoSize = true };
            save.Click += (s, e) =>
            {
                using (var d = new SaveFileDialog { Filter = "XML|*.xml", FileName = "transition.xml" })
                {
                    if (_txtFolder.Tag is string folder && Directory.Exists(folder))
                    {
                        d.InitialDirectory = folder;
                    }
                    if (d.ShowDialog(form) == DialogResult.OK)
                    {
                        File.WriteAllText(d.FileName, box.Text);
                    }
                }
            };
            var copy = new Button { Text = "Copy", AutoSize = true };
            copy.Click += (s, e) => Clipboard.SetText(box.Text);
            var load = new Button { Text = "Open file...", AutoSize = true };
            load.Click += (s, e) =>
            {
                using (var d = new OpenFileDialog { Filter = "XML|*.xml|All files|*.*" })
                {
                    if (d.ShowDialog(form) == DialogResult.OK)
                    {
                        box.Text = File.ReadAllText(d.FileName).Replace("\r\n", "\n").Replace("\n", "\r\n");
                    }
                }
            };
            bar.Controls.AddRange(new Control[] { save, copy, load });
            form.Controls.Add(box);
            form.Controls.Add(bar);
            form.Show(this);
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                TransitionsHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            updateTimer.Stop();
            _texA?.Dispose();
            _texB?.Dispose();
            _learnT?.Dispose();
            foreach (Bitmap[] list in new[] { _masks, _results, _tiles })
            {
                foreach (Bitmap b in list)
                {
                    b?.Dispose();
                }
            }
            foreach (TransitionMasks.LibraryEntry entry in _library)
            {
                entry.Preview?.Dispose();
            }
            _assembled.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationEditFormButton : Form
    {
        #region Fields

        // --- Frame Editor ---
        private int currentIndex = 0;
        private CheckBox[] checks;
        private bool pipetteMode = false;
        private Label line = new Label();
        private Label vLine = new Label();

        // --- Zoom levels per PictureBox ---
        private int zoomLevel1 = 0, zoomLevel2 = 0, zoomLevel3 = 0, zoomLevel4 = 0,
                    zoomLevel5 = 0, zoomLevel6 = 0, zoomLevel7 = 0, zoomLevel8 = 0,
                    zoomLevel9 = 0, zoomLevel10 = 0;

        private SelectablePictureBox[] pictureBoxes;
        public static SelectablePictureBox[] boxes = new SelectablePictureBox[10];

        // --- Clipboard Animation ---
        private Dictionary<string, Image> originalImages = new Dictionary<string, Image>();
        private Dictionary<string, Image> images = new Dictionary<string, Image>();
        private int imageCounter = 1;
        private Timer playTimer;
        private int currentImageIndex = 0;

        // --- UO Animation Browser ---
        private int selectedAnimBody = 0;
        private int selectedAnimAction = 0;
        private int selectedAnimDir = 0;
        private int selectedAnimHue = 0;
        private int selectedAnimFileIndex = 1;
        private List<Ultima.AnimationFrame[]> loadedAnimFrames = new List<Ultima.AnimationFrame[]>();
        private List<Bitmap> loadedCustomBitmaps = new List<Bitmap>();
        private int animPreviewCurrentFrame = 0;
        private Timer animBrowserTimer;

        // --- Art Browser ---
        private int artSearchIndex = 0;
        private bool artShowStatic = true;
        private List<int> artSearchResults = new List<int>();

        // FIX: Guard flag to prevent CheckedChanged during initialization
        private bool _artBrowserInitializing = true;

        #endregion

        #region Constructor

        public AnimationEditFormButton()
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            this.KeyPreview = true;

            // --- Setup boxes and checks arrays ---
            boxes = new SelectablePictureBox[]
            {
                selectablePictureBox1, selectablePictureBox2, selectablePictureBox3,
                selectablePictureBox4, selectablePictureBox5, selectablePictureBox6,
                selectablePictureBox7, selectablePictureBox8, selectablePictureBox9,
                selectablePictureBox10
            };

            checks = new CheckBox[]
            {
                checkBox1, checkBox2, checkBox3, checkBox4, checkBox5,
                checkBox6, checkBox7, checkBox8, checkBox9, checkBox10
            };

            // --- Timer Setup ---
            timer.Interval = 1000;
            timer.Tick += new EventHandler(timer_Tick);

            numericUpDownFrameDelay.ValueChanged += new EventHandler(numericUpDownFrameDelay_ValueChanged);
            btColordialog.Click += (sender, e) => SelectColor();
            tboxColorCode.TextChanged += tboxColorCode_TextChanged;

            // --- Zoom Tag Setup ---
            SetupZoomTags();

            // --- Overlay Lines Setup ---
            SetupOverlayLines();

            // --- Play Timer for Clipboard Anim Tab ---
            playTimer = new Timer();
            playTimer.Tick += new EventHandler(PlayTimer_Tick);

            // --- Anim Browser Timer ---
            animBrowserTimer = new Timer();
            animBrowserTimer.Tick += new EventHandler(AnimBrowserTimer_Tick);
            animBrowserTimer.Interval = 150;

            // --- Initialize Anim Browser Tab (bind events only) ---
            InitAnimBrowserTab();

            // --- Initialize Art Browser Tab (bind events only) ---
            InitArtBrowserTab();

            // FIX: Allow CheckedChanged to work after full init
            _artBrowserInitializing = false;
        }

        #endregion

        #region Setup Helpers

        private void SetupZoomTags()
        {
            zoomInButton1.Tag = selectablePictureBox1; zoomOutButton1.Tag = selectablePictureBox1;
            zoomInButton2.Tag = selectablePictureBox2; zoomOutButton2.Tag = selectablePictureBox2;
            zoomInButton3.Tag = selectablePictureBox3; zoomOutButton3.Tag = selectablePictureBox3;
            zoomInButton4.Tag = selectablePictureBox4; zoomOutButton4.Tag = selectablePictureBox4;
            zoomInButton5.Tag = selectablePictureBox5; zoomOutButton5.Tag = selectablePictureBox5;
            zoomInButton6.Tag = selectablePictureBox6; zoomOutButton6.Tag = selectablePictureBox6;
            zoomInButton7.Tag = selectablePictureBox7; zoomOutButton7.Tag = selectablePictureBox7;
            zoomInButton8.Tag = selectablePictureBox8; zoomOutButton8.Tag = selectablePictureBox8;
            zoomInButton9.Tag = selectablePictureBox9; zoomOutButton9.Tag = selectablePictureBox9;
            zoomInButton10.Tag = selectablePictureBox10; zoomOutButton10.Tag = selectablePictureBox10;
        }

        private void SetupOverlayLines()
        {
            line.BackColor = Color.Red;
            line.Width = AnimationPictureBox.Width;
            line.Visible = false;

            vLine.BackColor = Color.Red;
            vLine.Height = AnimationPictureBox.Height;
            vLine.Visible = false;

            panelAnimationPictureBox.Controls.Add(line);
            panelAnimationPictureBox.Controls.Add(vLine);
            panelAnimationPictureBox.BackColor = Color.Transparent;

            numericUpDownHighAnimationPictureBox.ValueChanged += (s, e) => UpdateLine();
            numericUpDownSizeLineAnimationPictureBox.ValueChanged += (s, e) => UpdateLine();
            numericUpDownWidthAnimationPictureBox.ValueChanged += (s, e) => UpdateVLine();
            numericUpDownSizeLine2AnimationPictureBox.ValueChanged += (s, e) => UpdateVLine();
            numericUpDownColor.ValueChanged += (s, e) => { UpdateLine(); UpdateVLine(); UpdateOverlayColor(); };
        }

        #endregion

        #region Overlay Line Methods

        private void UpdateLine()
        {
            int y = (int)numericUpDownHighAnimationPictureBox.Value;
            int thickness = (int)numericUpDownSizeLineAnimationPictureBox.Value;
            line.Top = y;
            line.Height = thickness;
            line.BackColor = GetOverlayColor();
            line.Visible = thickness > 0;
            line.BringToFront();
        }

        private void UpdateVLine()
        {
            int x = (int)numericUpDownWidthAnimationPictureBox.Value;
            int thickness = (int)numericUpDownSizeLine2AnimationPictureBox.Value;
            vLine.Left = x;
            vLine.Width = thickness;
            vLine.BackColor = GetOverlayColor();
            vLine.Visible = thickness > 0;
            vLine.BringToFront();
        }

        private void UpdateOverlayColor()
        {
            Color c = GetOverlayColor();
            line.BackColor = c;
            vLine.BackColor = c;
            line.Invalidate();
            vLine.Invalidate();
        }

        private Color GetOverlayColor()
        {
            switch ((int)numericUpDownColor.Value)
            {
                case 0: return Color.Red;
                case 1: return Color.Blue;
                case 2: return Color.Green;
                case 3: return Color.Yellow;
                case 4: return Color.Black;
                case 5: return Color.White;
                case 6: return Color.Orange;
                case 7: return Color.Pink;
                case 8: return Color.Turquoise;
                case 9: return Color.Gray;
                case 10: return Color.Gold;
                default: return Color.Red;
            }
        }

        #endregion

        #region Timer_Tick (Frame Animation)

        private void timer_Tick(object sender, EventArgs e)
        {
            currentIndex++;
            if (currentIndex >= boxes.Length)
                currentIndex = (int)numericUpDownStartDelay.Value - 1;

            Bitmap imageBitmap = null;
            Bitmap flippedImageBitmap = null;

            if (boxes[currentIndex].Image != null && checks[currentIndex].Checked)
            {
                imageBitmap = new Bitmap(boxes[currentIndex].Image);

                Color[] colorsToMakeTransparent = new Color[]
                {
                    Color.FromArgb(0, 0, 0),
                    Color.FromArgb(255, 255, 255),
                    Color.FromArgb(211, 211, 211)
                };

                foreach (Color color in colorsToMakeTransparent)
                    imageBitmap = MakeTransparent(imageBitmap, color);

                flippedImageBitmap = new Bitmap(imageBitmap);
                flippedImageBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            if (checkBoxShowFrame.Checked || imageBitmap == null)
            {
                AnimationPictureBox.Image = boxes[currentIndex].Image;
                if (boxes[currentIndex].Image != null)
                {
                    Bitmap originalFlippedImage = new Bitmap(boxes[currentIndex].Image);
                    originalFlippedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    AnimationPictureBox2.Image = originalFlippedImage;
                }
                else
                {
                    AnimationPictureBox2.Image = null;
                }
            }
            else
            {
                AnimationPictureBox.Image = imageBitmap;
                AnimationPictureBox2.Image = flippedImageBitmap;
            }

            frameLabel.Text = (currentIndex + 1).ToString();
        }

        #endregion

        #region Form Events

        private async void OnLoad(object sender, EventArgs e)
        {
            await Task.Yield();

            // FIX: Run PopulateAnimBodyList in background too — Animations.IsActionDefined
            // can cause deep call stacks (stack overflow) when called 2048 times on UI thread
            await Task.Run(() =>
            {
                try
                {
                    var bodyItems = new List<int>();
                    for (int body = 0; body < 2048; body++)
                    {
                        try
                        {
                            if (Animations.IsActionDefined(body, 0, 0))
                                bodyItems.Add(body);
                        }
                        catch { }
                    }

                    this.Invoke((Action)(() =>
                    {
                        if (listBoxAnimBodies == null || listBoxAnimBodies.IsDisposed) return;
                        listBoxAnimBodies.BeginUpdate();
                        listBoxAnimBodies.Items.Clear();
                        foreach (var b in bodyItems)
                            listBoxAnimBodies.Items.Add(b);
                        listBoxAnimBodies.EndUpdate();

                        // Populate directions and file combo (must be on UI thread)
                        if (listBoxAnimDirections != null)
                        {
                            listBoxAnimDirections.Items.Clear();
                            string[] dirNames = { "East", "NE", "North", "NW", "West", "SW", "South", "SE" };
                            for (int i = 0; i < 8; i++)
                                listBoxAnimDirections.Items.Add($"Dir {i}: {dirNames[i]}");
                            if (listBoxAnimDirections.Items.Count > 0)
                                listBoxAnimDirections.SelectedIndex = 0;
                        }

                        if (comboBoxAnimFile != null)
                        {
                            comboBoxAnimFile.Items.Clear();
                            comboBoxAnimFile.Items.AddRange(new object[] {
                        "anim.mul (1)", "anim2.mul (2)", "anim3.mul (3)",
                        "anim4.mul (4)", "anim5.mul (5)",
                        "AnimationFrame1.uop", "AnimationFrame2.uop", "AnimationFrame3.uop"
                    });
                            comboBoxAnimFile.SelectedIndex = 0;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke((Action)(() =>
                    {
                        if (labelAnimFrameInfo != null)
                            labelAnimFrameInfo.Text = $"Body load error: {ex.Message}";
                    }));
                }
            });

            // Art list background load (unchanged)
            await Task.Run(() =>
            {
                try
                {
                    var results = new List<(int id, string label)>();
                    bool isStatic = true;

                    for (int i = 0; i < 0x4000; i++)
                    {
                        try
                        {
                            Bitmap bmp = isStatic ? Art.GetStatic(i) : Art.GetLand(i);
                            if (bmp != null)
                                results.Add((i, $"0x{i:X4} ({i})"));
                        }
                        catch { }
                    }

                    this.Invoke((Action)(() =>
                    {
                        if (listBoxArtItems == null || listBoxArtItems.IsDisposed) return;
                        listBoxArtItems.BeginUpdate();
                        listBoxArtItems.Items.Clear();
                        artSearchResults.Clear();
                        foreach (var (id, label) in results)
                        {
                            artSearchResults.Add(id);
                            listBoxArtItems.Items.Add(label);
                        }
                        listBoxArtItems.EndUpdate();
                        if (labelArtInfo != null)
                            labelArtInfo.Text = $"Loaded: {artSearchResults.Count} items";
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke((Action)(() =>
                    {
                        if (labelArtInfo != null)
                            labelArtInfo.Text = $"Art load error: {ex.Message}";
                    }));
                }
            });
        }

        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            animBrowserTimer?.Stop();
            playTimer?.Stop();
            timer?.Stop();
        }

        #endregion

        #region Load Toolbar / Menu

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadImageToCheckedBoxes(false, false);
        }

        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LoadImageToCheckedBoxes(false, false);
        }

        private void loadToolStripMenuItemAllSingle_Click(object sender, EventArgs e)
        {
            LoadSingleImagesPerBox(false);
        }

        private void loadToolStripMenuItemAllSingleMirror_Click(object sender, EventArgs e)
        {
            LoadSingleImagesPerBox(true);
        }

        private void loadOneImageAllMirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadImageToCheckedBoxes(true, true);
        }

        #endregion

        #region Load Image Helpers

        private void LoadImageToCheckedBoxes(bool mirror, bool mirrorAll)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
                dlg.Title = "Please select an image file.";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                Image image = Image.FromFile(dlg.FileName);

                for (int i = 0; i < boxes.Length; i++)
                {
                    boxes[i].Image = null;
                    boxes[i].ClearImage();
                }

                for (int i = 0; i < boxes.Length; i++)
                {
                    if (checks[i].Checked)
                    {
                        boxes[i].Image = image;
                        boxes[i].OriginalImage = new Bitmap(image);
                        boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(image.Width, image.Height);
                        if (mirror) boxes[i].MirrorImage();
                        if (mirrorAll) boxes[i].MirrorAllImages();
                    }
                }
            }
        }

        private void LoadSingleImagesPerBox(bool mirror)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
                dlg.Title = "Please select an image file.";

                for (int i = 0; i < boxes.Length; i++)
                {
                    if (!checks[i].Checked) continue;
                    if (dlg.ShowDialog() != DialogResult.OK) break;

                    Image image = Image.FromFile(dlg.FileName);
                    boxes[i].ClearImage();
                    boxes[i].Image = image;
                    boxes[i].OriginalImage = new Bitmap(image);
                    boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(image.Width, image.Height);
                    if (mirror) boxes[i].MirrorImage();
                }
            }
        }

        #endregion

        #region KeyDown / PreviewKeyDown

        private void pictureBox_KeyDown(object sender, KeyEventArgs e) { }

        private void pictureBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                SelectablePictureBox currentBox = sender as SelectablePictureBox;
                if (Clipboard.ContainsImage())
                    currentBox.Image = Clipboard.GetImage();
                else
                    MessageBox.Show("The clipboard does not contain an image.");
            }
        }

        #endregion

        #region NumericUpDownFrameDelay

        private void numericUpDownFrameDelay_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownFrameDelay.Value > 0)
            {
                timer.Interval = 1000 / (int)numericUpDownFrameDelay.Value;
                delayLabel.Text = $"Processing speed: {timer.Interval} ms";
                if (timer.Enabled) { timer.Stop(); timer.Start(); }
            }
            else
            {
                MessageBox.Show("Frame delay must be greater than zero.");
            }
        }

        #endregion

        #region Start Animation

        private void startAnimationButton_Click(object sender, EventArgs e)
        {
            ToggleFrameAnimation();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleFrameAnimation();
        }

        private void ToggleFrameAnimation()
        {
            if (timer.Enabled)
            {
                timer.Stop();
                startAnimationButton.Text = "Start";
                startAnimationButton.BackColor = SystemColors.Control;
            }
            else
            {
                if (numericUpDownFrameDelay.Value > 0)
                {
                    timer.Interval = 1000 / (int)numericUpDownFrameDelay.Value;
                    currentIndex = (int)numericUpDownStartDelay.Value;
                    timer.Start();
                    startAnimationButton.Text = "Stop";
                    startAnimationButton.BackColor = Color.OrangeRed;
                }
                else
                {
                    MessageBox.Show("Frame delay must be greater than zero.");
                }
            }
        }

        #endregion

        #region MakeTransparent

        private Bitmap MakeTransparent(Bitmap original, Color color)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            for (int i = 0; i < original.Width; i++)
                for (int j = 0; j < original.Height; j++)
                {
                    Color originalColor = original.GetPixel(i, j);
                    newBitmap.SetPixel(i, j, originalColor == color ? Color.Transparent : originalColor);
                }
            return newBitmap;
        }

        private Image MakeColorTransparent(Image image, Color color)
        {
            Bitmap bitmap = new Bitmap(image);
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.ToArgb() == color.ToArgb())
                        bitmap.SetPixel(i, j, Color.Transparent);
                }
            return bitmap;
        }

        #endregion

        #region SelectColor / UpdateColor

        private void SelectColor()
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Color color = colorDialog.Color;
                    tboxColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    foreach (SelectablePictureBox box in boxes)
                        box.SetDrawingColor(color);
                }
            }
        }

        private void UpdateColor()
        {
            string code = tboxColorCode.Text;
            if (code.StartsWith("#")) code = code.Substring(1);
            if (code.Length == 6 && int.TryParse(code, NumberStyles.HexNumber, null, out int number))
            {
                Color color = Color.FromArgb(number >> 16, (number >> 8) & 255, number & 255);
                foreach (SelectablePictureBox box in boxes)
                    box.SetDrawingColor(color);
            }
        }

        #endregion

        #region Draw All

        private void btDrawAllSelectableBox_Click(object sender, EventArgs e)
        {
            bool isActive = false;
            foreach (SelectablePictureBox box in boxes)
            {
                box.ToggleDrawing();
                if (box.IsDrawing()) isActive = true;
            }

            if (isActive)
            {
                btDrawAllSelectableBox.BackColor = Color.Green;
                labelDrawAll.Text = "Active";
            }
            else
            {
                btDrawAllSelectableBox.BackColor = SystemColors.Control;
                labelDrawAll.Text = "Not active";
            }
        }

        #endregion

        #region BrushSize

        private void numericUpDownBrushSize_ValueChanged(object sender, EventArgs e)
        {
            int size = (int)numericUpDownBrushSize.Value;
            foreach (SelectablePictureBox box in boxes)
                box.SetBrushSize(size);
        }

        #endregion

        #region ComboBox Background

        private void comboBoxImageBackgrund_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxImageBackgrund.SelectedIndex < 0) return;
            string selected = comboBoxImageBackgrund.SelectedItem.ToString();

            Image bg = null;
            switch (selected)
            {
                case "Green": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Green2; break;
                case "Water": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.water2; break;
                case "Sand": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Sand; break;
                case "Street": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Street; break;
                case "Forest": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Forest; break;
                case "Dirt": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dirt; break;
                case "Dungeon": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dungeon; break;
                case "Cave": bg = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.cave; break;
                case "Clear": bg = null; break;
            }

            AnimationPictureBox.BackgroundImage = bg;
            AnimationPictureBox2.BackgroundImage = bg;
        }

        #endregion

        #region NumericUpDownImageShow

        private void numericUpDownImageShow_ValueChanged(object sender, EventArgs e)
        {
            int index = (int)numericUpDownImageShow.Value - 1;
            if (index >= 0 && index < boxes.Length && boxes[index].Image != null)
            {
                Bitmap imageBitmap = new Bitmap(boxes[index].Image);
                Color[] colorsToMakeTransparent = {
                    Color.FromArgb(0, 0, 0),
                    Color.FromArgb(255, 255, 255),
                    Color.FromArgb(211, 211, 211)
                };
                foreach (Color color in colorsToMakeTransparent)
                    imageBitmap = MakeTransparent(imageBitmap, color);

                AnimationPictureBox.Image = imageBitmap;
                Bitmap flippedImageBitmap = new Bitmap(imageBitmap);
                flippedImageBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                AnimationPictureBox2.Image = flippedImageBitmap;
            }
            else
            {
                AnimationPictureBox.Image = null;
                AnimationPictureBox2.Image = null;
            }
        }

        #endregion

        #region Pipette

        private void selectablePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is SelectablePictureBox box)
            {
                if (pipetteMode)
                {
                    Color color = box.GetPixelColor(e.Location);
                    tboxColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    pipetteMode = false;
                    btPipette.BackColor = SystemColors.Control;
                }
            }
        }

        private void btPipette_Click(object sender, EventArgs e)
        {
            pipetteMode = !pipetteMode;
            btPipette.BackColor = pipetteMode ? Color.Green : SystemColors.Control;
        }

        private void selectablePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is SelectablePictureBox box)
            {
                Color color = box.GetPixelColor(e.Location);
                panelFarbcode.BackColor = color;
                lbColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }

        #endregion

        #region TextBox ColorCode

        private void tboxColorCode_TextChanged(object sender, EventArgs e)
        {
            string colorCode = tboxColorCode.Text;
            if (!colorCode.StartsWith("#")) colorCode = "#" + colorCode;
            if (Regex.IsMatch(colorCode, "^#[0-9A-Fa-f]{6}$"))
            {
                Color color = ColorTranslator.FromHtml(colorCode);
                panelFarbCodeTB.BackColor = color;
            }
        }

        #endregion

        #region Zoom

        private void ZoomIn(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is SelectablePictureBox pictureBox)
                ApplyZoom(pictureBox, true);
        }

        private void ZoomOut(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is SelectablePictureBox pictureBox)
                ApplyZoom(pictureBox, false);
        }

        private void ApplyZoom(SelectablePictureBox pictureBox, bool zoomIn)
        {
            int index = Array.IndexOf(boxes, pictureBox);
            if (index < 0) return;

            int[] zoomLevels = { zoomLevel1, zoomLevel2, zoomLevel3, zoomLevel4, zoomLevel5,
                                  zoomLevel6, zoomLevel7, zoomLevel8, zoomLevel9, zoomLevel10 };
            int zl = zoomLevels[index];
            ChangeZoomLevel(pictureBox, ref zl, zoomIn);
            zoomLevels[index] = zl;

            zoomLevel1 = zoomLevels[0]; zoomLevel2 = zoomLevels[1];
            zoomLevel3 = zoomLevels[2]; zoomLevel4 = zoomLevels[3];
            zoomLevel5 = zoomLevels[4]; zoomLevel6 = zoomLevels[5];
            zoomLevel7 = zoomLevels[6]; zoomLevel8 = zoomLevels[7];
            zoomLevel9 = zoomLevels[8]; zoomLevel10 = zoomLevels[9];
        }

        private void ChangeZoomLevel(SelectablePictureBox pictureBox, ref int zoomLevel, bool zoomIn)
        {
            if (pictureBox.Image == null) return;
            if (zoomIn && zoomLevel < 2)
            {
                pictureBox.Image = new Bitmap(pictureBox.Image, new Size(pictureBox.Image.Width * 2, pictureBox.Image.Height * 2));
                zoomLevel++;
            }
            else if (!zoomIn && zoomLevel > 0)
            {
                pictureBox.Image = new Bitmap(pictureBox.Image, new Size(pictureBox.Image.Width / 2, pictureBox.Image.Height / 2));
                zoomLevel--;
            }
        }

        #endregion

        #region Background Image Loader

        private void imageFadeinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
                dlg.Title = "Please select an image file.";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Image image = new Bitmap(Image.FromFile(dlg.FileName), new Size(176, 238));
                    AnimationPictureBox.BackgroundImage = image;
                    if (ShowAnimationPictureBox2.Checked)
                        AnimationPictureBox2.BackgroundImage = image;
                }
            }
        }

        private void ShowAnimationPictureBox2_CheckedChanged(object sender, EventArgs e)
        {
            AnimationPictureBox2.BackgroundImage = ShowAnimationPictureBox2.Checked
                ? AnimationPictureBox.BackgroundImage
                : null;
        }

        #endregion

        #region Clipboard Animation Tab

        private void clipbordAllSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage()) { MessageBox.Show("The cache does not contain an image."); return; }
            if (imageCounter > 30) { MessageBox.Show("Maximum number of images reached."); return; }

            Image image = Clipboard.GetImage();
            string imageName = "Item" + imageCounter.ToString("00");
            originalImages[imageName] = (Image)image.Clone();

            if (CheckBoxTransparent.Checked)
            {
                image = MakeColorTransparent(image, Color.FromArgb(0, 0, 0));
                image = MakeColorTransparent(image, Color.FromArgb(255, 255, 255));
            }

            images[imageName] = image;
            PictureBoxAll.Image = image;
            PictureBoxAll.Refresh();
            ListBoxAll.Items.Add(imageName);
            imageCounter++;
        }

        private void listBoxAll_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem == null) return;
            string selectedItem = ListBoxAll.SelectedItem.ToString();
            if (images.ContainsKey(selectedItem))
            {
                PictureBoxAll.Image = images[selectedItem];
                PictureBoxAll.Refresh();
            }
        }

        private void CheckBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var key in images.Keys.ToList())
            {
                Image image;
                if (CheckBoxTransparent.Checked)
                {
                    image = MakeColorTransparent(originalImages[key], Color.FromArgb(0, 0, 0));
                    image = MakeColorTransparent(image, Color.FromArgb(255, 255, 255));
                }
                else
                {
                    image = originalImages[key];
                }
                images[key] = image;
            }

            if (ListBoxAll.SelectedItem != null)
            {
                string selectedItem = ListBoxAll.SelectedItem.ToString();
                if (images.ContainsKey(selectedItem))
                {
                    PictureBoxAll.Image = images[selectedItem];
                    PictureBoxAll.Refresh();
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem == null) { MessageBox.Show("No image selected for deletion."); return; }
            string selectedItem = ListBoxAll.SelectedItem.ToString();
            originalImages.Remove(selectedItem);
            images.Remove(selectedItem);
            ListBoxAll.Items.Remove(selectedItem);
            PictureBoxAll.Image = null;
        }

        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            originalImages.Clear();
            images.Clear();
            ListBoxAll.Items.Clear();
            PictureBoxAll.Image = null;
        }

        private void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListBoxAll.Items.Count > 0)
            {
                currentImageIndex = 0;
                int frameDelay = (int)numericUpDownFrameDelay.Value;
                playTimer.Interval = frameDelay > 0 ? 1000 / frameDelay : 1000;
                playTimer.Start();
            }
            else
            {
                MessageBox.Show("No images available to play.");
            }
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (currentImageIndex >= ListBoxAll.Items.Count)
                currentImageIndex = 0;

            if (ListBoxAll.Items.Count > 0 && currentImageIndex < ListBoxAll.Items.Count)
            {
                string selectedItem = ListBoxAll.Items[currentImageIndex].ToString();
                if (images.ContainsKey(selectedItem))
                {
                    PictureBoxAll.Image = images[selectedItem];
                    PictureBoxAll.Refresh();
                }
                currentImageIndex++;
            }
            else
            {
                playTimer.Stop();
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playTimer.Stop();
        }

        private void exchangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem != null && Clipboard.ContainsImage())
            {
                string selectedItem = ListBoxAll.SelectedItem.ToString();
                Image newImage = Clipboard.GetImage();
                originalImages[selectedItem] = (Image)newImage.Clone();

                if (CheckBoxTransparent.Checked)
                {
                    newImage = MakeColorTransparent(newImage, Color.FromArgb(0, 0, 0));
                    newImage = MakeColorTransparent(newImage, Color.FromArgb(255, 255, 255));
                }

                images[selectedItem] = newImage;
                PictureBoxAll.Image = newImage;
                PictureBoxAll.Refresh();
            }
            else
            {
                MessageBox.Show("No image selected or no image in buffer.");
            }
        }

        #endregion

        // =====================================================================
        // === UO ANIMATION BROWSER TAB ========================================
        // =====================================================================

        #region Anim Browser Tab - Init

        private void InitAnimBrowserTab()
        {
            if (listBoxAnimBodies == null) return;

            listBoxAnimBodies.SelectedIndexChanged += listBoxAnimBodies_SelectedIndexChanged;
            listBoxAnimActions.SelectedIndexChanged += listBoxAnimActions_SelectedIndexChanged;
            listBoxAnimDirections.SelectedIndexChanged += listBoxAnimDirections_SelectedIndexChanged;
            numericAnimHue.ValueChanged += numericAnimHue_ValueChanged;
            btAnimPlay.Click += btAnimPlay_Click;
            btAnimStop.Click += btAnimStop_Click;
            btAnimCopyFramesToEditor.Click += btAnimCopyFramesToEditor_Click;
            btAnimLoadFromFile.Click += btAnimLoadFromFile_Click;
            btAnimExportFrames.Click += btAnimExportFrames_Click;
            btAnimCopyFrameClipboard.Click += btAnimCopyFrameClipboard_Click;
            comboBoxAnimFile.SelectedIndexChanged += comboBoxAnimFile_SelectedIndexChanged;
            numericAnimZoom.ValueChanged += numericAnimZoom_ValueChanged;
            btAnimSearch.Click += btAnimSearch_Click;
            tboxAnimBodySearch.KeyDown += tboxAnimBodySearch_KeyDown;
        }
        #endregion

        #region Anim Browser Tab - Body Selection

        private void listBoxAnimBodies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAnimBodies.SelectedItem == null) return;
            selectedAnimBody = (int)listBoxAnimBodies.SelectedItem;
            PopulateAnimActions();
        }

        private void PopulateAnimActions()
        {
            if (listBoxAnimActions == null) return;
            listBoxAnimActions.BeginUpdate();
            listBoxAnimActions.Items.Clear();

            try
            {
                string[] actionNames = {
                    "Walk", "Run", "Stand", "Fight", "Attack 1", "Attack 2", "Attack 3",
                    "Spell 1", "Spell 2", "Die 1", "Die 2", "Fidget 1", "Fidget 2",
                    "Eat", "Looting", "Mounted", "Block", "Unknown"
                };
                for (int action = 0; action < 35; action++)
                {
                    try
                    {
                        if (Animations.IsActionDefined(selectedAnimBody, action, 0))
                        {
                            string name = action < actionNames.Length ? actionNames[action] : $"Action {action}";
                            listBoxAnimActions.Items.Add($"{action}: {name}");
                        }
                    }
                    catch { }
                }

                if (listBoxAnimActions.Items.Count == 0)
                    for (int a = 0; a < 18; a++)
                        listBoxAnimActions.Items.Add($"{a}: Action {a}");
            }
            catch
            {
                for (int a = 0; a < 18; a++)
                    listBoxAnimActions.Items.Add($"{a}: Action {a}");
            }

            listBoxAnimActions.EndUpdate();
            if (listBoxAnimActions.Items.Count > 0)
                listBoxAnimActions.SelectedIndex = 0;
        }

        private void listBoxAnimActions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAnimActions.SelectedItem == null) return;
            string item = listBoxAnimActions.SelectedItem.ToString();
            selectedAnimAction = int.Parse(item.Split(':')[0]);
            LoadAnimFrames();
        }

        private void listBoxAnimDirections_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedAnimDir = listBoxAnimDirections.SelectedIndex;
            LoadAnimFrames();
        }

        private void numericAnimHue_ValueChanged(object sender, EventArgs e)
        {
            selectedAnimHue = (int)numericAnimHue.Value;
            LoadAnimFrames();
        }

        private void comboBoxAnimFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedAnimFileIndex = comboBoxAnimFile.SelectedIndex + 1;
            // Re-use the background loader instead of calling PopulateAnimBodyList directly
            Task.Run(() =>
            {
                try
                {
                    var bodyItems = new List<int>();
                    for (int body = 0; body < 2048; body++)
                    {
                        try
                        {
                            if (Animations.IsActionDefined(body, 0, 0))
                                bodyItems.Add(body);
                        }
                        catch { }
                    }
                    this.Invoke((Action)(() =>
                    {
                        if (listBoxAnimBodies == null || listBoxAnimBodies.IsDisposed) return;
                        listBoxAnimBodies.BeginUpdate();
                        listBoxAnimBodies.Items.Clear();
                        foreach (var b in bodyItems)
                            listBoxAnimBodies.Items.Add(b);
                        listBoxAnimBodies.EndUpdate();
                    }));
                }
                catch { }
            });
        }

        #endregion

        #region Anim Browser Tab - Load & Display Frames

        private void LoadAnimFrames()
        {
            if (listBoxAnimFrames == null) return;
            loadedAnimFrames.Clear();
            loadedCustomBitmaps.Clear();
            listBoxAnimFrames.Items.Clear();
            animBrowserTimer.Stop();

            try
            {
                int hue = selectedAnimHue;
                bool preserveHue = false;

                Ultima.AnimationFrame[] frames = Animations.GetAnimation(
                    selectedAnimBody, selectedAnimAction, selectedAnimDir,
                    ref hue, preserveHue, false);

                if (frames == null || frames.Length == 0)
                {
                    labelAnimFrameInfo.Text = "No frames found.";
                    picBoxAnimPreview.Image = null;
                    return;
                }

                for (int i = 0; i < frames.Length; i++)
                    listBoxAnimFrames.Items.Add($"Frame {i + 1} ({frames[i].Bitmap?.Width ?? 0}x{frames[i].Bitmap?.Height ?? 0})");

                labelAnimFrameInfo.Text = $"Body: {selectedAnimBody} | Action: {selectedAnimAction} | Dir: {selectedAnimDir} | Frames: {frames.Length}";
                loadedAnimFrames.Add(frames);

                if (frames.Length > 0 && frames[0].Bitmap != null)
                    picBoxAnimPreview.Image = frames[0].Bitmap;

                if (listBoxAnimFrames.Items.Count > 0)
                    listBoxAnimFrames.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                labelAnimFrameInfo.Text = $"Error: {ex.Message}";
            }
        }

        private void listBoxAnimFrames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAnimFrames.SelectedIndex < 0) return;
            int frameIdx = listBoxAnimFrames.SelectedIndex;
            int zoom = (int)numericAnimZoom.Value;

            if (loadedAnimFrames.Count == 0 && loadedCustomBitmaps.Count > 0)
            {
                if (frameIdx >= loadedCustomBitmaps.Count) return;
                Bitmap bmp = loadedCustomBitmaps[frameIdx];
                if (zoom > 1) bmp = new Bitmap(bmp, new Size(bmp.Width * zoom, bmp.Height * zoom));
                picBoxAnimPreview.Image = bmp;
                labelAnimFrameInfo.Text = $"Custom Frame: {frameIdx + 1}/{loadedCustomBitmaps.Count} | Size: {loadedCustomBitmaps[frameIdx].Width}x{loadedCustomBitmaps[frameIdx].Height}";
                return;
            }

            if (loadedAnimFrames.Count == 0) return;
            Ultima.AnimationFrame[] frames = loadedAnimFrames[0];
            if (frameIdx < frames.Length && frames[frameIdx].Bitmap != null)
            {
                Bitmap bmp = frames[frameIdx].Bitmap;
                if (zoom > 1) bmp = new Bitmap(bmp, new Size(bmp.Width * zoom, bmp.Height * zoom));
                picBoxAnimPreview.Image = bmp;
                labelAnimFrameInfo.Text = $"Body: {selectedAnimBody} | Action: {selectedAnimAction} | Dir: {selectedAnimDir} | Frame: {frameIdx + 1}/{frames.Length} | Size: {frames[frameIdx].Bitmap.Width}x{frames[frameIdx].Bitmap.Height}";
            }
        }

        private void numericAnimZoom_ValueChanged(object sender, EventArgs e)
        {
            if (listBoxAnimFrames.SelectedIndex >= 0)
                listBoxAnimFrames_SelectedIndexChanged(sender, e);
        }

        #endregion

        #region Anim Browser Tab - Play / Stop Preview

        private void btAnimPlay_Click(object sender, EventArgs e)
        {
            if (loadedAnimFrames.Count == 0 || loadedAnimFrames[0].Length == 0) return;
            animPreviewCurrentFrame = 0;
            animBrowserTimer.Interval = Math.Max(50, (int)numericAnimPreviewSpeed.Value);
            animBrowserTimer.Start();
            btAnimPlay.BackColor = Color.Green;
        }

        private void btAnimStop_Click(object sender, EventArgs e)
        {
            animBrowserTimer.Stop();
            btAnimPlay.BackColor = SystemColors.Control;
        }

        private void AnimBrowserTimer_Tick(object sender, EventArgs e)
        {
            int zoom = (int)numericAnimZoom.Value;

            if (loadedAnimFrames.Count == 0 && loadedCustomBitmaps.Count > 0)
            {
                if (animPreviewCurrentFrame >= loadedCustomBitmaps.Count)
                    animPreviewCurrentFrame = 0;
                Bitmap bmp = loadedCustomBitmaps[animPreviewCurrentFrame];
                if (zoom > 1) bmp = new Bitmap(bmp, new Size(bmp.Width * zoom, bmp.Height * zoom));
                picBoxAnimPreview.Image = bmp;
                animPreviewCurrentFrame++;
                return;
            }

            if (loadedAnimFrames.Count == 0) return;
            Ultima.AnimationFrame[] frames = loadedAnimFrames[0];
            if (frames.Length == 0) return;

            if (animPreviewCurrentFrame >= frames.Length)
                animPreviewCurrentFrame = 0;

            if (frames[animPreviewCurrentFrame].Bitmap != null)
            {
                Bitmap bmp = frames[animPreviewCurrentFrame].Bitmap;
                if (zoom > 1) bmp = new Bitmap(bmp, new Size(bmp.Width * zoom, bmp.Height * zoom));
                picBoxAnimPreview.Image = bmp;
            }

            animPreviewCurrentFrame++;
        }

        #endregion

        #region Anim Browser Tab - Copy / Export

        private void btAnimCopyFramesToEditor_Click(object sender, EventArgs e)
        {
            if (loadedAnimFrames.Count == 0 && loadedCustomBitmaps.Count > 0)
            {
                int copyCount = Math.Min(loadedCustomBitmaps.Count, boxes.Length);
                for (int i = 0; i < copyCount; i++)
                {
                    if (checks[i].Checked)
                    {
                        boxes[i].Image = new Bitmap(loadedCustomBitmaps[i]);
                        boxes[i].OriginalImage = new Bitmap(loadedCustomBitmaps[i]);
                        boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(loadedCustomBitmaps[i].Width, loadedCustomBitmaps[i].Height);
                    }
                }
                tabControl1.SelectedTab = tabPageFrame;
                MessageBox.Show($"Copied {copyCount} custom frames to the Frame Editor.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (loadedAnimFrames.Count == 0) return;
            Ultima.AnimationFrame[] frames = loadedAnimFrames[0];

            int frameCount = Math.Min(frames.Length, boxes.Length);
            for (int i = 0; i < frameCount; i++)
            {
                if (frames[i].Bitmap != null && checks[i].Checked)
                {
                    boxes[i].Image = new Bitmap(frames[i].Bitmap);
                    boxes[i].OriginalImage = new Bitmap(frames[i].Bitmap);
                    boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(frames[i].Bitmap.Width, frames[i].Bitmap.Height);
                }
            }

            tabControl1.SelectedTab = tabPageFrame;
            MessageBox.Show($"Copied {frameCount} frames to the Frame Editor.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btAnimCopyFrameClipboard_Click(object sender, EventArgs e)
        {
            if (listBoxAnimFrames.SelectedIndex < 0) return;
            int idx = listBoxAnimFrames.SelectedIndex;

            if (loadedAnimFrames.Count == 0 && loadedCustomBitmaps.Count > 0)
            {
                if (idx < loadedCustomBitmaps.Count)
                {
                    Clipboard.SetImage(loadedCustomBitmaps[idx]);
                    MessageBox.Show("Frame copied to clipboard.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            if (loadedAnimFrames.Count == 0) return;
            Ultima.AnimationFrame[] frames = loadedAnimFrames[0];
            if (idx < frames.Length && frames[idx].Bitmap != null)
            {
                Clipboard.SetImage(frames[idx].Bitmap);
                MessageBox.Show("Frame copied to clipboard.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btAnimExportFrames_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select export folder for animation frames";
                if (fbd.ShowDialog() != DialogResult.OK) return;

                int exported = 0;

                if (loadedAnimFrames.Count == 0 && loadedCustomBitmaps.Count > 0)
                {
                    for (int i = 0; i < loadedCustomBitmaps.Count; i++)
                    {
                        string path = Path.Combine(fbd.SelectedPath, $"Custom_Frame{i:D3}.png");
                        loadedCustomBitmaps[i].Save(path, ImageFormat.Png);
                        exported++;
                    }
                    MessageBox.Show($"Exported {exported} frames to:\n{fbd.SelectedPath}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (loadedAnimFrames.Count == 0) { MessageBox.Show("No frames loaded."); return; }
                Ultima.AnimationFrame[] frames = loadedAnimFrames[0];

                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i].Bitmap != null)
                    {
                        string path = Path.Combine(fbd.SelectedPath, $"Body{selectedAnimBody}_Action{selectedAnimAction}_Dir{selectedAnimDir}_Frame{i:D3}.png");
                        frames[i].Bitmap.Save(path, ImageFormat.Png);
                        exported++;
                    }
                }

                MessageBox.Show($"Exported {exported} frames to:\n{fbd.SelectedPath}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btAnimLoadFromFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png|All files (*.*)|*.*";
                dlg.Title = "Load animation frame from file";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() != DialogResult.OK) return;

                loadedAnimFrames.Clear();
                listBoxAnimFrames.Items.Clear();

                var customBitmaps = new List<Bitmap>();
                foreach (string file in dlg.FileNames)
                {
                    try { customBitmaps.Add(new Bitmap(file)); }
                    catch { }
                }

                if (customBitmaps.Count > 0)
                {
                    loadedCustomBitmaps = customBitmaps;
                    loadedAnimFrames.Clear();

                    listBoxAnimFrames.Items.Clear();
                    for (int i = 0; i < customBitmaps.Count; i++)
                        listBoxAnimFrames.Items.Add($"Frame {i + 1} ({customBitmaps[i].Width}x{customBitmaps[i].Height})");

                    picBoxAnimPreview.Image = customBitmaps[0];
                    labelAnimFrameInfo.Text = $"Custom | Frames: {customBitmaps.Count}";

                    if (listBoxAnimFrames.Items.Count > 0)
                        listBoxAnimFrames.SelectedIndex = 0;
                }
            }
        }

        private void btAnimSearch_Click(object sender, EventArgs e) => SearchAnimBodies();

        private void tboxAnimBodySearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) SearchAnimBodies();
        }

        private void SearchAnimBodies()
        {
            if (string.IsNullOrWhiteSpace(tboxAnimBodySearch.Text)) return;
            if (int.TryParse(tboxAnimBodySearch.Text, out int bodyId))
            {
                for (int i = 0; i < listBoxAnimBodies.Items.Count; i++)
                {
                    if ((int)listBoxAnimBodies.Items[i] == bodyId)
                    {
                        listBoxAnimBodies.SelectedIndex = i;
                        listBoxAnimBodies.TopIndex = i;
                        return;
                    }
                }
                MessageBox.Show($"Body ID {bodyId} not found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        // =====================================================================
        // === ART BROWSER TAB =================================================
        // =====================================================================

        #region Art Browser Tab - Init

        private void InitArtBrowserTab()
        {
            if (listBoxArtItems == null) return;
            listBoxArtItems.SelectedIndexChanged += listBoxArtItems_SelectedIndexChanged;
            btArtSearch.Click += btArtSearch_Click;
            btArtCopyClipboard.Click += btArtCopyClipboard_Click;
            btArtCopyToFrameEditor.Click += btArtCopyToFrameEditor_Click;
            btArtExport.Click += btArtExport_Click;

            // FIX: Do NOT bind CheckedChanged here — already bound via Designer would double-fire.
            // rbArtStatic.CheckedChanged and rbArtLand.CheckedChanged are bound in Designer.
            // We only need to guard against them firing during init (see _artBrowserInitializing).

            tboxArtSearch.KeyDown += tboxArtSearch_KeyDown;
            numericArtZoom.ValueChanged += numericArtZoom_ValueChanged;
            btArtLoadPrevious.Click += btArtLoadPrevious_Click;
            btArtLoadNext.Click += btArtLoadNext_Click;
        }

        // FIX: PopulateArtList is now called from OnLoad (async), NOT from rbArtType_CheckedChanged during init.
        private void PopulateArtList()
        {
            if (listBoxArtItems == null) return;
            listBoxArtItems.BeginUpdate();
            listBoxArtItems.Items.Clear();
            artSearchResults.Clear();

            try
            {
                bool isStatic = rbArtStatic == null || rbArtStatic.Checked;

                for (int i = 0; i < 0x4000; i++)
                {
                    try
                    {
                        Bitmap bmp = isStatic ? Art.GetStatic(i) : Art.GetLand(i);
                        if (bmp != null)
                        {
                            artSearchResults.Add(i);
                            listBoxArtItems.Items.Add($"0x{i:X4} ({i})");
                        }
                    }
                    catch { }
                }
            }
            catch
            {
                for (int i = 0; i < 100; i++)
                {
                    artSearchResults.Add(i);
                    listBoxArtItems.Items.Add($"0x{i:X4} ({i})");
                }
            }

            listBoxArtItems.EndUpdate();
            if (labelArtInfo != null)
                labelArtInfo.Text = $"Loaded: {artSearchResults.Count} items";
        }

        #endregion

        #region Art Browser Tab - Events

        private void listBoxArtItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxArtItems.SelectedIndex < 0 || listBoxArtItems.SelectedIndex >= artSearchResults.Count) return;
            int artIdx = artSearchResults[listBoxArtItems.SelectedIndex];
            DisplayArtImage(artIdx);
        }

        // FIX: Guard against firing during initialization
        private void rbArtType_CheckedChanged(object sender, EventArgs e)
        {
            if (_artBrowserInitializing) return;
            PopulateArtList();
        }

        private void tboxArtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btArtSearch_Click(sender, e);
        }

        private void numericArtZoom_ValueChanged(object sender, EventArgs e)
        {
            if (listBoxArtItems.SelectedIndex >= 0)
                listBoxArtItems_SelectedIndexChanged(sender, e);
        }

        private void btArtLoadPrevious_Click(object sender, EventArgs e)
        {
            if (listBoxArtItems.SelectedIndex > 0)
                listBoxArtItems.SelectedIndex--;
        }

        private void btArtLoadNext_Click(object sender, EventArgs e)
        {
            if (listBoxArtItems.SelectedIndex < listBoxArtItems.Items.Count - 1)
                listBoxArtItems.SelectedIndex++;
        }

        #endregion

        #region Art Browser Tab - Display

        private void DisplayArtImage(int artIdx)
        {
            try
            {
                bool isStatic = rbArtStatic == null || rbArtStatic.Checked;
                Bitmap bmp = isStatic ? Art.GetStatic(artIdx) : Art.GetLand(artIdx);

                if (bmp != null)
                {
                    int zoom = numericArtZoom != null ? (int)numericArtZoom.Value : 1;
                    if (zoom > 1)
                        bmp = new Bitmap(bmp, new Size(bmp.Width * zoom, bmp.Height * zoom));

                    picBoxArtPreview.Image = bmp;
                    labelArtInfo.Text = $"ID: 0x{artIdx:X4} ({artIdx}) | Size: {bmp.Width}x{bmp.Height} | Type: {(isStatic ? "Static" : "Land")}";
                }
                else
                {
                    picBoxArtPreview.Image = null;
                    labelArtInfo.Text = $"ID: 0x{artIdx:X4} - No image found";
                }
            }
            catch (Exception ex)
            {
                labelArtInfo.Text = $"Error loading art: {ex.Message}";
            }
        }

        #endregion

        #region Art Browser Tab - Search / Copy / Export

        private void btArtSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tboxArtSearch.Text)) return;

            string query = tboxArtSearch.Text.Trim();
            int searchId = -1;

            if (query.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                int.TryParse(query.Substring(2), NumberStyles.HexNumber, null, out searchId);
            else
                int.TryParse(query, out searchId);

            if (searchId >= 0)
            {
                for (int i = 0; i < artSearchResults.Count; i++)
                {
                    if (artSearchResults[i] == searchId)
                    {
                        listBoxArtItems.SelectedIndex = i;
                        listBoxArtItems.TopIndex = i;
                        return;
                    }
                }
                MessageBox.Show($"Art ID {query} not found.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btArtCopyClipboard_Click(object sender, EventArgs e)
        {
            if (picBoxArtPreview.Image == null) return;
            Clipboard.SetImage(picBoxArtPreview.Image);
            MessageBox.Show("Art image copied to clipboard.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btArtCopyToFrameEditor_Click(object sender, EventArgs e)
        {
            if (picBoxArtPreview.Image == null) return;

            for (int i = 0; i < boxes.Length; i++)
            {
                if (checks[i].Checked)
                {
                    Bitmap bmp = new Bitmap(picBoxArtPreview.Image);
                    boxes[i].Image = bmp;
                    boxes[i].OriginalImage = new Bitmap(bmp);
                    boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(bmp.Width, bmp.Height);

                    tabControl1.SelectedTab = tabPageFrame;
                    MessageBox.Show($"Art copied to Frame {i + 1} in Frame Editor.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            MessageBox.Show("Please check at least one frame checkbox.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btArtExport_Click(object sender, EventArgs e)
        {
            if (picBoxArtPreview.Image == null) return;
            if (listBoxArtItems.SelectedIndex < 0) return;

            int artIdx = artSearchResults[listBoxArtItems.SelectedIndex];

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image (*.png)|*.png|BMP Image (*.bmp)|*.bmp";
                sfd.FileName = $"Art_0x{artIdx:X4}";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat fmt = sfd.FileName.EndsWith(".bmp") ? ImageFormat.Bmp : ImageFormat.Png;
                    picBoxArtPreview.Image.Save(sfd.FileName, fmt);
                    MessageBox.Show("Art exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        #endregion

        // =====================================================================
        // === SAVE / EXPORT ANIMATION =========================================
        // =====================================================================

        #region Save / Export Animation

        private void btSaveAnimationGif_Click(object sender, EventArgs e)
        {
            List<Bitmap> frames = new List<Bitmap>();
            for (int i = 0; i < boxes.Length; i++)
            {
                if (checks[i].Checked && boxes[i].Image != null)
                    frames.Add(new Bitmap(boxes[i].Image));
            }

            if (frames.Count == 0) { MessageBox.Show("No frames to save."); return; }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Spritesheet (*.png)|*.png|BMP Files (*.bmp)|*.bmp";
                sfd.Title = "Export animation frames";
                sfd.FileName = "animation_export";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                int totalWidth = frames.Sum(f => f.Width);
                int maxHeight = frames.Max(f => f.Height);

                using (Bitmap spritesheet = new Bitmap(totalWidth, maxHeight))
                using (Graphics g = Graphics.FromImage(spritesheet))
                {
                    g.Clear(Color.Transparent);
                    int x = 0;
                    foreach (Bitmap frame in frames)
                    {
                        g.DrawImage(frame, x, 0);
                        x += frame.Width;
                    }

                    ImageFormat fmt = sfd.FileName.EndsWith(".bmp") ? ImageFormat.Bmp : ImageFormat.Png;
                    spritesheet.Save(sfd.FileName, fmt);
                }

                MessageBox.Show($"Saved {frames.Count} frames as spritesheet to:\n{sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btSaveAllFramesSeparate_Click(object sender, EventArgs e)
        {
            List<(int index, Bitmap bmp)> frames = new List<(int, Bitmap)>();
            for (int i = 0; i < boxes.Length; i++)
            {
                if (checks[i].Checked && boxes[i].Image != null)
                    frames.Add((i + 1, new Bitmap(boxes[i].Image)));
            }

            if (frames.Count == 0) { MessageBox.Show("No frames to save."); return; }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select folder to save frames";
                if (fbd.ShowDialog() != DialogResult.OK) return;

                foreach (var (index, bmp) in frames)
                {
                    string path = Path.Combine(fbd.SelectedPath, $"Frame_{index:D2}.png");
                    bmp.Save(path, ImageFormat.Png);
                }

                MessageBox.Show($"Saved {frames.Count} frames to:\n{fbd.SelectedPath}", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Clear All Frames

        private void btClearAllFrames_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear all frame images?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].Image = null;
                boxes[i].ClearImage();
            }

            AnimationPictureBox.Image = null;
            AnimationPictureBox2.Image = null;
            frameLabel.Text = "0";
        }

        #endregion
    }
}
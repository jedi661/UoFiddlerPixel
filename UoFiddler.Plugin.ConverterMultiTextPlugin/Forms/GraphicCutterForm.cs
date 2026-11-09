using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class.GraphicCutter;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    /// <summary>
    /// Graphic Cutter: place an image on a 352 x 352 stage with an isometric grid,
    /// cut an area out of it (e.g. 44 px wide art pieces) and edit / save / copy / collect the result
    /// or write it into art.mul.
    /// </summary>
    public partial class GraphicCutterForm : Form, ICutterHost
    {
        private const int StageSize = 352;

        /// <summary>Distance between two diamond corners of the grid (32 * 250/256 * √2 / 2 ≈ 22.1 px).</summary>
        private static readonly double GridStep = 32 * (250.0 / 256) * Math.Sqrt(2) / 2;

        private const int GridCenterX = StageSize / 2;  // x of the top corner of the diamond
        private const int GridTopY = StageSize / 2 - 178; // y of the top corner of the diamond (-2)
        private const int MaxUndo = 60;
        private const int SidePanelWidth = 320;

        private static readonly string OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tempGrafic");

        private enum SelectionShape { Rectangle, Circle, Freehand }

        // Stage (left side)
        private Bitmap _source;   // image as loaded, never changed
        private Bitmap _work;     // _source after transparency / trim / mirror, drawn on the stage
        private Point _imagePos;  // top left corner of _work on the stage
        private bool _stageMirrored;
        private bool _autoTrim;
        private bool _bakeStage;  // grid + background become part of the cut
        private bool _showGrid = true;
        private bool _showBorder = true;
        private bool _showCutFrame = true;
        private bool _showRuler;
        private Color _gridColor = Color.Red;
        private Image _stageBackImage;
        private Bitmap _landTile;
        private int _landTileId = 0x0003;
        private readonly List<Color> _pickedColors = new List<Color>();
        private bool _pipette;

        // Result (middle)
        private Bitmap _result;
        private bool _resultMirrored;
        private int _resultRotation; // quarter turns to the left
        private bool _showGridOnResult;
        private bool _showMask;
        private Bitmap _maskOverlay;
        private int _zoom = 1;

        // Selection on the result, in image pixels
        private SelectionShape _selectionShape = SelectionShape.Rectangle;
        private Rectangle _selection;
        private readonly List<Point> _freehand = new List<Point>();
        private bool _selecting;
        private Point _selectionStart;

        // Mouse dragging on the stage
        private enum StageDrag { None, Image, Cut }
        private StageDrag _stageDrag = StageDrag.None;
        private Point _dragStartMouse;
        private Point _dragStartImage;
        private Rectangle _dragStartCut;

        // Hold-to-repeat arrow buttons
        private Point _repeatDelta;
        private bool _repeatMovesCut;
        private int _repeatTicks;

        // Undo / redo
        private sealed class UndoState : IDisposable
        {
            public Point ImagePos;
            public Rectangle Cut;
            public bool StageMirrored;
            public Bitmap Result;
            public bool ResultMirrored;
            public int ResultRotation;

            public void Dispose() => Result?.Dispose();
        }

        private readonly List<UndoState> _undo = new List<UndoState>();
        private readonly List<UndoState> _redo = new List<UndoState>();
        private string _lastUndoKind;
        private DateTime _lastUndoTime;
        private bool _restoring;
        private Rectangle _shownCut; // cut area the current result belongs to (fields may already show a new value)

        private bool _updatingFields;
        private bool _playSound = true;
        private SoundPlayer _soundPlayer;

        public GraphicCutterForm()
        {
            InitializeComponent();

            SetupRepeatButton(btnUp, 0, -1, false);
            SetupRepeatButton(btnDown, 0, 1, false);
            SetupRepeatButton(btnLeft, -1, 0, false);
            SetupRepeatButton(btnRight, 1, 0, false);
            SetupRepeatButton(btnCutUp, 0, -1, true);
            SetupRepeatButton(btnCutDown, 0, 1, true);
            SetupRepeatButton(btnCutLeft, -1, 0, true);
            SetupRepeatButton(btnCutRight, 1, 0, true);

            panelResult.MouseWheel += Result_MouseWheel;
            pictureBox2.MouseWheel += Result_MouseWheel;

            sidePanel.Host = this;

            tsFormat.SelectedIndex = 0;
            tsFillMode.SelectedIndex = 0;
            tsBackground.SelectedIndex = 0;
            comboPreset.SelectedIndex = 0;
            _shownCut = CutRect;

            InitTooltips();
            SyncUi();
            UpdatePickedLabel();
            UpdateResultBox();
            UpdateStatus();
            SetMessage("Ready – Ctrl+O open, Ctrl+V paste, arrows move the image, Ctrl+arrows move the cut area, F1 help.");
        }

        #region ICutterHost

        Bitmap ICutterHost.CurrentResult => _result;

        TransparentFill ICutterHost.SaveFill => SaveFill;

        string ICutterHost.SaveExtension => SaveExtension;

        void ICutterHost.LoadIntoStage(Bitmap bitmap, string name) => LoadStageImage(bitmap, name);

        void ICutterHost.LoadIntoResult(Bitmap bitmap, string name)
        {
            PushUndo(null);
            ClearSelection();
            SetResult(bitmap);
            SetMessage($"Loaded {name} into the result ({bitmap.Width}×{bitmap.Height}). Moving the cut area replaces it again.");
        }

        void ICutterHost.UseLandBackground(int landId) => SetLandBackground(landId);

        void ICutterHost.ShowMessage(string text) => SetMessage(text);

        private TransparentFill SaveFill => (TransparentFill)Math.Max(0, tsFillMode.SelectedIndex);

        private string SaveExtension
        {
            get
            {
                switch (tsFormat.SelectedItem as string)
                {
                    case "TIFF": return ".tiff";
                    case "PNG": return ".png";
                    case "JPEG": return ".jpeg";
                    default: return ".bmp";
                }
            }
        }

        #endregion

        #region Helpers – UI

        private static string Tip(string en, string de) => en + "\n\nDE: " + de;

        private void SetMessage(string text) => statusMessage.Text = text;

        private static void ReplaceBitmap(ref Bitmap field, Bitmap value)
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }
            Bitmap old = field;
            field = value;
            old?.Dispose();
        }

        private Rectangle CutRect => new Rectangle((int)numericX.Value, (int)numericY.Value, (int)numericWidth.Value, (int)numericHeight.Value);

        private static decimal Clamp(NumericUpDown n, int value) => Math.Max(n.Minimum, Math.Min(n.Maximum, value));

        /// <summary>Sets the four number fields without cutting again.</summary>
        private void SetCutFields(Rectangle r)
        {
            _updatingFields = true;
            numericX.Value = Clamp(numericX, r.X);
            numericY.Value = Clamp(numericY, r.Y);
            numericWidth.Value = Clamp(numericWidth, r.Width);
            numericHeight.Value = Clamp(numericHeight, r.Height);
            _updatingFields = false;
        }

        private void SetCut(Rectangle r)
        {
            SetCutFields(r);
            UpdateResult();
            _shownCut = CutRect;
            pictureBox1.Invalidate();
            UpdateStatus();
        }

        private bool IsTextInputFocused()
        {
            if (bgHexTextBox.Focused || tsFormat.Focused || tsBackground.Focused || tsFillMode.Focused)
            {
                return true;
            }
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }
            return c is TextBoxBase || c is UpDownBase || c?.Parent is UpDownBase || c is ComboBox || c is ArtTileGrid;
        }

        private void SyncUi()
        {
            gridMenuItem.Checked = tsGrid.Checked = _showGrid;
            gridResultMenuItem.Checked = ctxGrid.Checked = _showGridOnResult;
            borderMenuItem.Checked = tsBorder.Checked = _showBorder;
            cutFrameMenuItem.Checked = _showCutFrame;
            rulerMenuItem.Checked = tsRuler.Checked = _showRuler;
            mirrorStageMenuItem.Checked = _stageMirrored;
            autoTrimMenuItem.Checked = _autoTrim;
            bakeMenuItem.Checked = _bakeStage;
            transparentMenuItem.Checked = checkBoxTransparent.Checked;
            showMaskMenuItem.Checked = ctxMask.Checked = tsMask.Checked = _showMask;
            soundMenuItem.Checked = _playSound;
            topMostMenuItem.Checked = TopMost;
            snapMenuItem.Checked = checkBoxSnap.Checked;
            pipetteMenuItem.Checked = tsPipette.Checked = _pipette;
            sidePanelMenuItem.Checked = tsPanel.Checked = sidePanel.Visible;
            labelZoom.Text = _zoom + "x";

            if (checkBoxBake.Checked != _bakeStage)
            {
                checkBoxBake.Checked = _bakeStage;
            }

            bgImageNoneMenuItem.Checked = tsBackground.SelectedIndex == 0;
            bgImageGreenMenuItem.Checked = tsBackground.SelectedIndex == 1;
            bgImageWaterMenuItem.Checked = tsBackground.SelectedIndex == 2;
            bgLandMenuItem.Checked = tsBackground.SelectedIndex == 3;
            bgLandMenuItem.Text = $"Land tile 0x{_landTileId:X4} (art.mul)";

            undoMenuItem.Enabled = tsUndo.Enabled = _undo.Count > 0;
            redoMenuItem.Enabled = tsRedo.Enabled = _redo.Count > 0;
        }

        private void UpdateStatus()
        {
            string image = _work == null ? "no image" : $"image {_work.Width}×{_work.Height} at {_imagePos.X},{_imagePos.Y}";
            Rectangle cut = CutRect;
            string result = _result == null ? "no result" : $"result {_result.Width}×{_result.Height}";
            string collection = sidePanel.CollectionCount > 0 ? $"   |   collection {sidePanel.CollectionCount}" : string.Empty;
            statusInfo.Text = $"{image}   |   cut {cut.Width}×{cut.Height} at {cut.X},{cut.Y}   |   {result}   |   zoom {_zoom}x{collection}";
        }

        private void InitTooltips()
        {
            toolTip1.SetToolTip(pictureBox1, Tip(
                "Stage (352 × 352) with the isometric 8 × 8 tile grid.\nLeft drag = move the image, right drag or Ctrl + left drag = move the cut area.",
                "Arbeitsfläche (352 × 352) mit isometrischem 8 × 8 Kachelraster.\nLinks ziehen = Bild verschieben, rechts ziehen oder Strg + links ziehen = Ausschnitt verschieben."));
            toolTip1.SetToolTip(pictureBox2, Tip(
                "Result of the cut. Drag to select (rectangle, circle or freehand). Right click = menu. Ctrl + wheel = zoom.",
                "Ergebnis des Ausschnitts. Ziehen = Auswahl (Rechteck, Kreis oder Freihand). Rechtsklick = Menü. Strg + Mausrad = Zoom."));

            const string moveEn = "Hold the button to keep moving. Keyboard: arrow keys (Shift = 10 px).";
            const string moveDe = "Gedrückt halten = weiter verschieben. Tastatur: Pfeiltasten (Umschalt = 10 px).";
            foreach (Control b in new Control[] { btnUp, btnDown, btnLeft, btnRight })
            {
                toolTip1.SetToolTip(b, Tip("Move the image on the stage. " + moveEn, "Bild auf der Arbeitsfläche verschieben. " + moveDe));
            }
            foreach (Control b in new Control[] { btnCutUp, btnCutDown, btnCutLeft, btnCutRight })
            {
                toolTip1.SetToolTip(b, Tip("Move the cut area. Keyboard: Ctrl + arrow keys (Shift = 10 px).",
                    "Ausschnitt verschieben. Tastatur: Strg + Pfeiltasten (Umschalt = 10 px)."));
            }

            toolTip1.SetToolTip(numericWidth, Tip("Width of the cut area in pixels.", "Breite des Ausschnitts in Pixeln."));
            toolTip1.SetToolTip(numericHeight, Tip("Height of the cut area in pixels.", "Höhe des Ausschnitts in Pixeln."));
            toolTip1.SetToolTip(numericX, Tip("Left edge of the cut area on the stage.", "Linke Kante des Ausschnitts auf der Arbeitsfläche."));
            toolTip1.SetToolTip(numericY, Tip("Top edge of the cut area on the stage.", "Obere Kante des Ausschnitts auf der Arbeitsfläche."));
            toolTip1.SetToolTip(comboPreset, Tip("Common sizes. 44 × 123 matches the alignment mask.", "Übliche Größen. 44 × 123 passt zur Ausrichtungsmaske."));
            toolTip1.SetToolTip(checkBoxFollow, Tip("When the image is moved, the cut area moves with it.", "Wird das Bild verschoben, wandert der Ausschnitt mit."));
            toolTip1.SetToolTip(checkBoxSnap, Tip("The cut area jumps to the corners of the diamond tiles (left and bottom edge).",
                "Der Ausschnitt springt auf die Ecken der Rautenkacheln (linke Kante und Unterkante)."));
            toolTip1.SetToolTip(buttonQuickSave, Tip("Save the result into the tempGrafic folder (format from the toolbar).",
                "Ergebnis im Ordner tempGrafic speichern (Format aus der Werkzeugleiste)."));
            toolTip1.SetToolTip(checkBoxTransparent, Tip("Show black and white of the image as transparent.", "Schwarz und Weiß des Bildes transparent darstellen."));
            toolTip1.SetToolTip(numericTolerance, Tip("0 = only the exact color. Higher values also remove similar colors (also for picked colors).",
                "0 = nur die genaue Farbe. Höhere Werte entfernen auch ähnliche Farben (auch für Pipetten-Farben)."));
            toolTip1.SetToolTip(radioRectangle, Tip("Select a rectangle in the result.", "Rechteck im Ergebnis auswählen."));
            toolTip1.SetToolTip(radioCircle, Tip("Select an ellipse / circle in the result.", "Ellipse / Kreis im Ergebnis auswählen."));
            toolTip1.SetToolTip(radioFreehand, Tip("Draw a free shape in the result. It is closed automatically.", "Freie Form im Ergebnis zeichnen. Sie wird automatisch geschlossen."));
            toolTip1.SetToolTip(zoomButton, Tip("Zoom in the result (Ctrl + wheel).", "Ergebnis vergrößern (Strg + Mausrad)."));
            toolTip1.SetToolTip(zoomOutButton, Tip("Zoom out the result.", "Ergebnis verkleinern."));
            toolTip1.SetToolTip(resetButton, Tip("Zoom 1x and remove the selection.", "Zoom 1x und Auswahl entfernen."));
            toolTip1.SetToolTip(checkBoxBake, Tip("Background color, background image and grid become part of the cut (old \"Grafic Paint\").",
                "Hintergrundfarbe, Hintergrundbild und Raster werden mit ausgeschnitten (früher \"Grafic Paint\")."));
            toolTip1.SetToolTip(buttonClearPicked, Tip("Remove all colors picked with the pipette.", "Alle mit der Pipette gewählten Farben entfernen."));

            tsOpen.ToolTipText = Tip("Open image (Ctrl+O)", "Bild öffnen (Strg+O)");
            tsImport.ToolTipText = Tip("Paste image from clipboard (Ctrl+V)", "Bild aus der Zwischenablage einfügen (Strg+V)");
            tsSave.ToolTipText = Tip("Save result into tempGrafic (Ctrl+S)", "Ergebnis in tempGrafic speichern (Strg+S)");
            tsFormat.ToolTipText = Tip("File format for quick save", "Dateiformat für schnelles Speichern");
            tsFillMode.ToolTipText = Tip("What transparent pixels become in saved files", "Was aus transparenten Pixeln in gespeicherten Dateien wird");
            tsUndo.ToolTipText = Tip("Undo (Ctrl+Z)", "Rückgängig (Strg+Z)");
            tsRedo.ToolTipText = Tip("Redo (Ctrl+Y)", "Wiederholen (Strg+Y)");
            tsCopy.ToolTipText = Tip("Copy result to clipboard (Ctrl+C)", "Ergebnis in die Zwischenablage kopieren (Strg+C)");
            tsAddCollection.ToolTipText = Tip("Add the result to the collection (Ctrl+Enter)", "Ergebnis zur Sammlung hinzufügen (Strg+Enter)");
            tsMirror.ToolTipText = Tip("Mirror the result", "Ergebnis spiegeln");
            tsRotate.ToolTipText = Tip("Rotate the result 90° to the left", "Ergebnis um 90° nach links drehen");
            tsCutStrips.ToolTipText = Tip("Split the result into art pieces (44 px strips)", "Ergebnis in Art-Teile zerlegen (44-px-Streifen)");
            tsFill.ToolTipText = Tip("Fill the selection with a texture", "Auswahl mit einer Textur füllen");
            tsPipette.ToolTipText = Tip("Pipette: click a color on the stage or in the result – it becomes transparent",
                "Pipette: Farbe auf der Arbeitsfläche oder im Ergebnis anklicken – sie wird transparent");
            tsGrid.ToolTipText = Tip("Grid on the stage (Ctrl+G)", "Raster auf der Arbeitsfläche (Strg+G)");
            tsBorder.ToolTipText = Tip("Red frame around the image", "Roter Rahmen um das Bild");
            tsRuler.ToolTipText = Tip("Ruler around the stage", "Lineal um die Arbeitsfläche");
            tsMask.ToolTipText = Tip("Alignment mask 44 × 123 over the result (mask by Otimpyre). Only a view aid, not saved.",
                "Ausrichtungsmaske 44 × 123 über dem Ergebnis (Maske von Otimpyre). Nur Anzeigehilfe, wird nicht gespeichert.");
            tsBackground.ToolTipText = Tip("Background of the stage. \"land tile\" uses a land tile from art.mul (choose it in the art.mul panel).",
                "Hintergrund der Arbeitsfläche. \"land tile\" nimmt eine Landkachel aus der art.mul (im art.mul-Panel wählen).");
            tsBackColor.ToolTipText = Tip("Switch the stage background between black and white", "Hintergrund der Arbeitsfläche zwischen Schwarz und Weiß umschalten");
            tsPanel.ToolTipText = Tip("Show / hide the art.mul and collection panel (Ctrl+B)", "art.mul- und Sammlungs-Panel ein- / ausblenden (Strg+B)");
            tsHelp.ToolTipText = Tip("Help (F1)", "Hilfe (F1)");
            autoTrimMenuItem.ToolTipText = Tip("Cuts away the transparent border of the image (old \"Change the border\").",
                "Schneidet den transparenten Rand des Bildes weg (früher \"Change the border\").");
            bgHexTextBox.ToolTipText = Tip("Hex color, e.g. FF0000, then press Enter", "Hex-Farbe, z. B. FF0000, dann Enter");
        }

        #endregion

        #region Undo / redo

        private UndoState CaptureState() => new UndoState
        {
            ImagePos = _imagePos,
            Cut = _shownCut,
            StageMirrored = _stageMirrored,
            Result = _result == null ? null : CutterImaging.To32Bpp(_result),
            ResultMirrored = _resultMirrored,
            ResultRotation = _resultRotation
        };

        /// <summary>
        /// Remembers the state before a change. Changes of the same kind within one second
        /// (e.g. holding an arrow key) become one step. kind = null: always a new step.
        /// </summary>
        private void PushUndo(string kind)
        {
            if (_restoring)
            {
                return;
            }
            DateTime now = DateTime.Now;
            if (kind != null && kind == _lastUndoKind && (now - _lastUndoTime).TotalMilliseconds < 1000)
            {
                _lastUndoTime = now;
                return;
            }
            _lastUndoKind = kind;
            _lastUndoTime = now;

            _undo.Add(CaptureState());
            if (_undo.Count > MaxUndo)
            {
                _undo[0].Dispose();
                _undo.RemoveAt(0);
            }
            ClearStates(_redo);
            SyncUi();
        }

        private static void ClearStates(List<UndoState> list)
        {
            foreach (UndoState s in list)
            {
                s.Dispose();
            }
            list.Clear();
        }

        private void ClearHistory()
        {
            ClearStates(_undo);
            ClearStates(_redo);
            _lastUndoKind = null;
            SyncUi();
        }

        private void Restore(UndoState state)
        {
            _restoring = true;
            try
            {
                _imagePos = state.ImagePos;
                if (state.StageMirrored != _stageMirrored && _source != null)
                {
                    _stageMirrored = state.StageMirrored;
                    RebuildWork(keepPosition: true);
                }
                SetCutFields(state.Cut);
                _shownCut = state.Cut;
                _resultMirrored = state.ResultMirrored;
                _resultRotation = state.ResultRotation;
                ClearSelection();
                SetResult(state.Result == null ? null : CutterImaging.To32Bpp(state.Result));
                pictureBox1.Invalidate();
                UpdateStatus();
            }
            finally
            {
                _restoring = false;
                _lastUndoKind = null;
            }
            SyncUi();
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            if (_undo.Count == 0)
            {
                return;
            }
            _redo.Add(CaptureState());
            UndoState state = _undo[_undo.Count - 1];
            _undo.RemoveAt(_undo.Count - 1);
            Restore(state);
            state.Dispose();
            SetMessage($"Undo ({_undo.Count} more).");
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            if (_redo.Count == 0)
            {
                return;
            }
            _undo.Add(CaptureState());
            UndoState state = _redo[_redo.Count - 1];
            _redo.RemoveAt(_redo.Count - 1);
            Restore(state);
            state.Dispose();
            SetMessage($"Redo ({_redo.Count} more).");
        }

        #endregion

        #region Loading and unloading

        private void LoadStageImage(Bitmap bmp, string name)
        {
            ReplaceBitmap(ref _source, bmp);
            _stageMirrored = false;
            _resultMirrored = false;
            _resultRotation = 0;
            ClearSelection();
            ClearHistory();
            RebuildWork(forceRecenter: true);
            SyncUi();
            SetMessage($"Loaded {name} ({bmp.Width}×{bmp.Height}).");
        }

        /// <summary>All colors that are removed from the image.</summary>
        private List<Color> TransparentColors()
        {
            var colors = new List<Color>();
            if (checkBoxTransparent.Checked)
            {
                colors.Add(Color.White);
                colors.Add(Color.Black);
            }
            colors.AddRange(_pickedColors);
            return colors;
        }

        /// <summary>Builds the stage image from the source: transparency, trim, mirror.</summary>
        private void RebuildWork(bool forceRecenter = false, bool keepPosition = false)
        {
            if (_source == null)
            {
                ReplaceBitmap(ref _work, null);
                pictureBox1.Invalidate();
                UpdateStatus();
                return;
            }

            Size oldSize = _work?.Size ?? Size.Empty;
            Bitmap bmp = CutterImaging.To32Bpp(_source);

            CutterImaging.MakeTransparent(bmp, (int)numericTolerance.Value, TransparentColors());

            if (_autoTrim)
            {
                Bitmap trimmed = CutterImaging.TrimTransparent(bmp);
                if (trimmed != null)
                {
                    bmp.Dispose();
                    bmp = trimmed;
                }
            }

            if (_stageMirrored)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            ReplaceBitmap(ref _work, bmp);

            if (!keepPosition && (forceRecenter || oldSize != bmp.Size))
            {
                CenterImage();
            }
            else if (!_restoring)
            {
                UpdateResult();
                pictureBox1.Invalidate();
                UpdateStatus();
            }
        }

        /// <summary>Puts the image bottom centered on the stage and sets the cut area to the image.</summary>
        private void CenterImage()
        {
            if (_work == null)
            {
                return;
            }
            _imagePos = new Point((StageSize - _work.Width) / 2, StageSize - _work.Height);
            SetCut(new Rectangle(_imagePos, _work.Size));
        }

        private void OpenImage_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Open image" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    LoadStageImage(CutterImaging.LoadBitmap(dialog.FileName), Path.GetFileName(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, "Open image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ImportClipboard_Click(object sender, EventArgs e)
        {
            Bitmap bmp;
            try
            {
                bmp = CutterImaging.GetClipboardImage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The clipboard could not be read:\n" + ex.Message, "Paste", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (bmp == null)
            {
                SetMessage("The clipboard contains no image.");
                return;
            }
            LoadStageImage(bmp, "clipboard image");
        }

        private void OpenIntoResult_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Open image into the result" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    Bitmap bmp = CutterImaging.LoadBitmap(dialog.FileName);
                    ((ICutterHost)this).LoadIntoResult(bmp, Path.GetFileName(dialog.FileName));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be loaded:\n" + ex.Message, "Open image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UnloadStage_Click(object sender, EventArgs e)
        {
            if (_source == null)
            {
                SetMessage("No image is loaded on the stage.");
                return;
            }
            ReplaceBitmap(ref _source, null);
            ClearHistory();
            RebuildWork();
            SetMessage("Stage image unloaded.");
        }

        private void UnloadResult_Click(object sender, EventArgs e)
        {
            if (_result == null)
            {
                SetMessage("There is no result.");
                return;
            }
            PushUndo(null);
            ClearSelection();
            SetResult(null);
            SetMessage("Result unloaded.");
        }

        private void Close_Click(object sender, EventArgs e) => Close();

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (sidePanel.CollectionCount > 0 && e.CloseReason == CloseReason.UserClosing &&
                MessageBox.Show(this, $"The collection still contains {sidePanel.CollectionCount} pieces that are lost when closing.\nClose anyway?",
                    "Graphic Cutter", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            moveTimer.Stop();
            ReplaceBitmap(ref _source, null);
            ReplaceBitmap(ref _work, null);
            ReplaceBitmap(ref _result, null);
            ReplaceBitmap(ref _maskOverlay, null);
            ReplaceBitmap(ref _landTile, null);
            ClearStates(_undo);
            ClearStates(_redo);
            _stageBackImage?.Dispose();
            _soundPlayer?.Dispose();
            base.OnFormClosed(e);
        }

        #endregion

        #region Stage – drawing

        private void DrawIsoGrid(Graphics g)
        {
            const int tileWidth = 32; // 32 * scale * sqrt(2) = 44 px on screen, like a UO tile
            const int tiles = 8;
            float scale = 250f / (tiles * tileWidth);

            GraphicsState state = g.Save();
            g.TranslateTransform(GridCenterX, GridTopY);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(45);
            using (var pen = new Pen(_gridColor, 1f))
            {
                for (int i = 0; i <= tiles; i++)
                {
                    g.DrawLine(pen, 0, i * tileWidth, tiles * tileWidth, i * tileWidth);
                    g.DrawLine(pen, i * tileWidth, 0, i * tileWidth, tiles * tileWidth);
                }
            }
            g.Restore(state);
        }

        /// <summary>Fills the 8 × 8 diamond of the grid with the chosen land tile.</summary>
        private void DrawLandTiles(Graphics g)
        {
            if (_landTile == null || tsBackground.SelectedIndex != 3)
            {
                return;
            }
            InterpolationMode old = g.InterpolationMode;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            for (int sum = 0; sum <= 14; sum++)
            {
                for (int i = 0; i < 8; i++)
                {
                    int j = sum - i;
                    if (j < 0 || j > 7)
                    {
                        continue;
                    }
                    int x = (int)Math.Round(GridCenterX + (i - j) * GridStep) - CutterArt.LandSize / 2;
                    int y = (int)Math.Round(GridTopY + (i + j) * GridStep);
                    g.DrawImage(_landTile, new Rectangle(x, y, _landTile.Width, _landTile.Height));
                }
            }
            g.InterpolationMode = old;
        }

        private void DrawStageBackground(Graphics g)
        {
            using (var brush = new SolidBrush(pictureBox1.BackColor))
            {
                g.FillRectangle(brush, 0, 0, StageSize, StageSize);
            }
            if (_stageBackImage != null)
            {
                g.DrawImage(_stageBackImage, new Rectangle(0, 0, StageSize, StageSize));
            }
            DrawLandTiles(g);
        }

        /// <summary>Renders a part of the stage as it is cut: the image, plus grid and background when "bake" is on.</summary>
        private Bitmap RenderRegion(Rectangle region)
        {
            var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.TranslateTransform(-region.X, -region.Y);
                if (_bakeStage)
                {
                    DrawStageBackground(g);
                }
                if (_work != null)
                {
                    g.DrawImage(_work, new Rectangle(_imagePos, _work.Size));
                }
                if (_bakeStage && _showGrid)
                {
                    DrawIsoGrid(g);
                }
            }
            return bmp;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;

            DrawStageBackground(g);

            if (_work != null)
            {
                g.DrawImage(_work, new Rectangle(_imagePos, _work.Size));
            }

            g.PixelOffsetMode = PixelOffsetMode.Default;

            if (_showGrid)
            {
                DrawIsoGrid(g);
            }

            if (_showBorder && _work != null)
            {
                using (var pen = new Pen(Color.Red))
                {
                    g.DrawRectangle(pen, _imagePos.X, _imagePos.Y, _work.Width - 1, _work.Height - 1);
                }
            }

            if (_showCutFrame && _work != null)
            {
                Rectangle cut = CutRect;
                using (var back = new Pen(Color.Black))
                using (var pen = new Pen(Color.Cyan) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(back, cut.X, cut.Y, cut.Width - 1, cut.Height - 1);
                    g.DrawRectangle(pen, cut.X, cut.Y, cut.Width - 1, cut.Height - 1);
                }
            }
        }

        private void PanelStage_Paint(object sender, PaintEventArgs e)
        {
            if (_showRuler)
            {
                DrawRuler(e.Graphics);
            }
        }

        /// <summary>Ruler left of the stage (height, counted from the bottom) and below it (width).</summary>
        private void DrawRuler(Graphics g)
        {
            Rectangle box = pictureBox1.Bounds;
            using (var pen = new Pen(Color.Red))
            using (var font = new Font(Font.FontFamily, 7.5f))
            {
                for (int i = 0; i <= StageSize; i += 10)
                {
                    bool major = i % 50 == 0;
                    int len = major ? 8 : 4;

                    int y = box.Bottom - 1 - i;
                    g.DrawLine(pen, box.Left - 2, y, box.Left - 2 - len, y);

                    int x = box.Left + i;
                    g.DrawLine(pen, x, box.Bottom + 2, x, box.Bottom + 2 + len);

                    if (major)
                    {
                        string text = i.ToString();
                        SizeF size = g.MeasureString(text, font);
                        g.DrawString(text, font, Brushes.Red, box.Left - 12 - size.Width, y - size.Height / 2);
                        g.DrawString(text, font, Brushes.Red, x - size.Width / 2, box.Bottom + 12);
                    }
                }
            }
        }

        #endregion

        #region Stage – moving and snapping

        /// <summary>With "snap to grid": left edge and bottom edge of the cut area go to the nearest diamond corner.</summary>
        private Rectangle SnapCut(Rectangle r)
        {
            if (!checkBoxSnap.Checked)
            {
                return r;
            }
            int x = (int)Math.Round(GridCenterX + Math.Round((r.X - GridCenterX) / GridStep) * GridStep);
            int bottom = (int)Math.Round(GridTopY + Math.Round((r.Bottom - GridTopY) / GridStep) * GridStep);
            return new Rectangle(x, bottom - r.Height, r.Width, r.Height);
        }

        private void MoveImageBy(int dx, int dy)
        {
            if (_work == null)
            {
                return;
            }

            // Keep at least one pixel of the image on the stage.
            int x = Math.Max(-_work.Width + 1, Math.Min(StageSize - 1, _imagePos.X + dx));
            int y = Math.Max(-_work.Height + 1, Math.Min(StageSize - 1, _imagePos.Y + dy));
            int realDx = x - _imagePos.X;
            int realDy = y - _imagePos.Y;
            _imagePos = new Point(x, y);

            if (checkBoxFollow.Checked && (realDx != 0 || realDy != 0))
            {
                Rectangle cut = CutRect;
                cut.Offset(realDx, realDy);
                SetCut(cut);
            }
            else
            {
                UpdateResult();
                pictureBox1.Invalidate();
                UpdateStatus();
            }
        }

        private void MoveCutBy(int dx, int dy)
        {
            Rectangle cut = CutRect;
            if (checkBoxSnap.Checked)
            {
                // One diamond corner per step, four with Shift.
                int steps = Math.Max(Math.Abs(dx), Math.Abs(dy)) >= 10 ? 4 : 1;
                cut.Offset((int)Math.Round(Math.Sign(dx) * GridStep * steps), (int)Math.Round(Math.Sign(dy) * GridStep * steps));
                SetCut(SnapCut(cut));
                return;
            }
            cut.Offset(dx, dy);
            SetCut(cut);
        }

        private void SetupRepeatButton(Button button, int dx, int dy, bool movesCut)
        {
            button.MouseDown += (s, e) =>
            {
                if (e.Button != MouseButtons.Left)
                {
                    return;
                }
                _repeatDelta = new Point(dx, dy);
                _repeatMovesCut = movesCut;
                _repeatTicks = 0;
                PushUndo(movesCut ? "move-cut" : "move-image");
                DoRepeatMove();
                moveTimer.Start();
            };
            button.MouseUp += (s, e) => moveTimer.Stop();
            button.MouseLeave += (s, e) => moveTimer.Stop();
            // Space / Enter on a focused button. Mouse clicks are handled in MouseDown.
            button.Click += (s, e) =>
            {
                if (e is MouseEventArgs)
                {
                    return;
                }
                PushUndo(movesCut ? "move-cut" : "move-image");
                if (movesCut)
                {
                    MoveCutBy(dx, dy);
                }
                else
                {
                    MoveImageBy(dx, dy);
                }
            };
        }

        private void DoRepeatMove()
        {
            int step = (ModifierKeys & Keys.Shift) == Keys.Shift ? 10 : 1;
            if (_repeatMovesCut)
            {
                MoveCutBy(_repeatDelta.X * step, _repeatDelta.Y * step);
            }
            else
            {
                MoveImageBy(_repeatDelta.X * step, _repeatDelta.Y * step);
            }
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            // Short pause before repeating, so a single click moves exactly one step.
            if (++_repeatTicks > 6)
            {
                DoRepeatMove();
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ActiveControl = null; // arrow keys move the image again, not a number field

            if (_pipette && e.Button == MouseButtons.Left)
            {
                PickStageColor(e.Location);
                return;
            }
            if (_work == null || (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right))
            {
                return;
            }
            bool moveCut = e.Button == MouseButtons.Right || (ModifierKeys & Keys.Control) == Keys.Control;
            PushUndo(null);
            _stageDrag = moveCut ? StageDrag.Cut : StageDrag.Image;
            _dragStartMouse = e.Location;
            _dragStartImage = _imagePos;
            _dragStartCut = CutRect;
            pictureBox1.Cursor = Cursors.SizeAll;
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            statusPos.Text = $"X {e.X}  Y {e.Y}";

            if (_stageDrag == StageDrag.None)
            {
                return;
            }
            int dx = e.X - _dragStartMouse.X;
            int dy = e.Y - _dragStartMouse.Y;
            if (_stageDrag == StageDrag.Image)
            {
                MoveImageBy(_dragStartImage.X + dx - _imagePos.X, _dragStartImage.Y + dy - _imagePos.Y);
            }
            else
            {
                Rectangle cut = _dragStartCut;
                cut.Offset(dx, dy);
                cut = SnapCut(cut);
                if (cut != CutRect)
                {
                    SetCut(cut);
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            _stageDrag = StageDrag.None;
            pictureBox1.Cursor = _pipette ? Cursors.Hand : Cursors.Default;
        }

        private void PictureBox1_MouseLeave(object sender, EventArgs e) => statusPos.Text = string.Empty;

        #endregion

        #region Cut area fields

        private void CutField_ValueChanged(object sender, EventArgs e)
        {
            if (_updatingFields)
            {
                return;
            }
            PushUndo("cut-fields");
            UpdateResult();
            _shownCut = CutRect;
            pictureBox1.Invalidate();
            UpdateStatus();
        }

        private void Preset_SelectedIndexChanged(object sender, EventArgs e)
        {
            Rectangle cut = CutRect;
            switch (comboPreset.SelectedIndex)
            {
                case 1: // image size
                    if (_work == null)
                    {
                        comboPreset.SelectedIndex = 0;
                        return;
                    }
                    cut = new Rectangle(_imagePos, _work.Size);
                    break;
                case 2:
                    cut.Size = new Size(44, 44);
                    break;
                case 3:
                    cut.Size = new Size(44, 105);
                    break;
                case 4:
                    cut.Size = new Size(44, 123);
                    break;
                default:
                    return;
            }
            PushUndo(null);
            SetCut(SnapCut(cut));
            // Back to "(choose size)" so the same preset can be chosen again.
            comboPreset.SelectedIndex = 0;
        }

        private void Snap_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxSnap.Checked)
            {
                PushUndo(null);
                SetCut(SnapCut(CutRect));
                SetMessage("Snap to grid: the cut area follows the diamond corners.");
            }
            SyncUi();
        }

        private void SnapMenu_Click(object sender, EventArgs e) => checkBoxSnap.Checked = !checkBoxSnap.Checked;

        #endregion

        #region Result

        /// <summary>Cuts the area out of the stage into the result.</summary>
        private void UpdateResult()
        {
            if (_work == null)
            {
                return;
            }
            Rectangle cut = CutRect;
            if (cut.Width <= 0 || cut.Height <= 0)
            {
                return;
            }

            Bitmap bmp = RenderRegion(cut);
            if (_resultMirrored)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            for (int i = 0; i < _resultRotation; i++)
            {
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
            }
            SetResult(bmp);
        }

        private void SetResult(Bitmap bmp)
        {
            ReplaceBitmap(ref _result, bmp);
            UpdateResultBox();
            UpdateStatus();
        }

        private void UpdateResultBox()
        {
            Size size = _result == null ? new Size(1, 1) : new Size(_result.Width * _zoom, _result.Height * _zoom);
            if (pictureBox2.Size != size)
            {
                pictureBox2.Size = size;
            }
            CenterResultBox();
            pictureBox2.Invalidate();
            panelResult.Invalidate();
        }

        private void CenterResultBox()
        {
            Size client = panelResult.ClientSize;
            Point scroll = panelResult.AutoScrollPosition;
            int x = Math.Max(0, (client.Width - pictureBox2.Width) / 2) + scroll.X;
            int y = Math.Max(0, (client.Height - pictureBox2.Height) / 2) + scroll.Y;
            pictureBox2.Location = new Point(x, y);
        }

        private void PanelResult_Resize(object sender, EventArgs e) => CenterResultBox();

        private void PanelResult_Paint(object sender, PaintEventArgs e)
        {
            if (_result != null)
            {
                return;
            }
            const string hint = "No result yet.\nOpen an image (Ctrl+O), paste one (Ctrl+V) or load art from art.mul.\n\n"
                                + "Noch kein Ergebnis.\nBild öffnen (Strg+O), einfügen (Strg+V) oder Art aus der art.mul laden.";
            TextRenderer.DrawText(e.Graphics, hint, Font, panelResult.ClientRectangle, Color.Gainsboro,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        }

        private Bitmap GetMaskOverlay()
        {
            if (_maskOverlay == null)
            {
                using (Bitmap mask = Properties.Resources.showmask)
                {
                    _maskOverlay = CutterImaging.To32Bpp(mask);
                }
                CutterImaging.MakeTransparent(_maskOverlay, 0, new[] { Color.Black });
            }
            return _maskOverlay;
        }

        private void PictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (_result == null)
            {
                return;
            }
            Graphics g = e.Graphics;
            var area = new Rectangle(0, 0, _result.Width * _zoom, _result.Height * _zoom);
            CutterImaging.DrawChecker(g, area);

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(_result, area);

            if (_showMask)
            {
                Bitmap mask = GetMaskOverlay();
                g.DrawImage(mask, new Rectangle(0, 0, mask.Width * _zoom, mask.Height * _zoom));
            }
            g.PixelOffsetMode = PixelOffsetMode.Default;

            if (_showGridOnResult)
            {
                // Show the stage grid at the place where the result was cut out.
                GraphicsState state = g.Save();
                g.ScaleTransform(_zoom, _zoom);
                if (_work != null)
                {
                    g.TranslateTransform(-_shownCut.X, -_shownCut.Y);
                }
                DrawIsoGrid(g);
                g.Restore(state);
            }

            DrawSelection(g);
        }

        private void DrawSelection(Graphics g)
        {
            using (var back = new Pen(Color.Black))
            using (var pen = new Pen(Color.Yellow) { DashStyle = DashStyle.Dash })
            {
                if (_selectionShape == SelectionShape.Freehand)
                {
                    if (_freehand.Count > 1)
                    {
                        Point[] pts = _freehand.ConvertAll(p => new Point(p.X * _zoom, p.Y * _zoom)).ToArray();
                        g.DrawLines(back, pts);
                        g.DrawLines(pen, pts);
                        if (!_selecting && pts.Length > 2)
                        {
                            g.DrawLine(pen, pts[pts.Length - 1], pts[0]);
                        }
                    }
                    return;
                }

                if (_selection.Width <= 0 || _selection.Height <= 0)
                {
                    return;
                }
                var r = new Rectangle(_selection.X * _zoom, _selection.Y * _zoom, _selection.Width * _zoom - 1, _selection.Height * _zoom - 1);
                if (_selectionShape == SelectionShape.Circle)
                {
                    g.DrawEllipse(back, r);
                    g.DrawEllipse(pen, r);
                }
                else
                {
                    g.DrawRectangle(back, r);
                    g.DrawRectangle(pen, r);
                }
            }
        }

        private Point ToImagePoint(Point p, bool clamp)
        {
            int x = (int)Math.Floor(p.X / (double)_zoom);
            int y = (int)Math.Floor(p.Y / (double)_zoom);
            if (clamp && _result != null)
            {
                x = Math.Max(0, Math.Min(_result.Width, x));
                y = Math.Max(0, Math.Min(_result.Height, y));
            }
            return new Point(x, y);
        }

        private void PictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            ActiveControl = null;
            if (e.Button != MouseButtons.Left || _result == null)
            {
                return;
            }
            if (_pipette)
            {
                PickResultColor(e.Location);
                return;
            }
            _selecting = true;
            Point p = ToImagePoint(e.Location, true);
            if (_selectionShape == SelectionShape.Freehand)
            {
                _freehand.Clear();
                _freehand.Add(p);
            }
            else
            {
                _selectionStart = p;
                _selection = new Rectangle(p, Size.Empty);
            }
            pictureBox2.Invalidate();
        }

        private void PictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = ToImagePoint(e.Location, true);
            int maxX = Math.Max(0, (_result?.Width ?? 1) - 1);
            int maxY = Math.Max(0, (_result?.Height ?? 1) - 1);
            statusPos.Text = $"Result X {Math.Min(p.X, maxX)}  Y {Math.Min(p.Y, maxY)}";

            if (!_selecting)
            {
                return;
            }
            if (_selectionShape == SelectionShape.Freehand)
            {
                if (_freehand.Count == 0 || _freehand[_freehand.Count - 1] != p)
                {
                    _freehand.Add(p);
                }
            }
            else
            {
                _selection = Rectangle.FromLTRB(Math.Min(_selectionStart.X, p.X), Math.Min(_selectionStart.Y, p.Y),
                    Math.Max(_selectionStart.X, p.X), Math.Max(_selectionStart.Y, p.Y));
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    int side = Math.Max(_selection.Width, _selection.Height);
                    _selection = new Rectangle(p.X < _selectionStart.X ? _selectionStart.X - side : _selectionStart.X,
                        p.Y < _selectionStart.Y ? _selectionStart.Y - side : _selectionStart.Y, side, side);
                }
            }
            pictureBox2.Invalidate();
        }

        private void PictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_selecting)
            {
                return;
            }
            _selecting = false;
            pictureBox2.Invalidate();
            if (_selectionShape == SelectionShape.Freehand)
            {
                SetMessage(_freehand.Count > 2 ? "Freehand selection closed." : "Freehand selection is too small.");
            }
            else if (_selection.Width > 0 && _selection.Height > 0)
            {
                SetMessage($"Selection {_selection.Width}×{_selection.Height} at {_selection.X},{_selection.Y}.");
            }
        }

        private void Result_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
            {
                return;
            }
            if (e is HandledMouseEventArgs h)
            {
                h.Handled = true;
            }
            SetZoom(_zoom + (e.Delta > 0 ? 1 : -1));
        }

        private void ClearSelection()
        {
            _selecting = false;
            _selection = Rectangle.Empty;
            _freehand.Clear();
            pictureBox2.Invalidate();
        }

        private void SelectionMode_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && !rb.Checked)
            {
                return;
            }
            _selectionShape = radioCircle.Checked ? SelectionShape.Circle
                : radioFreehand.Checked ? SelectionShape.Freehand
                : SelectionShape.Rectangle;
            ClearSelection();
        }

        /// <summary>The selected shape as a path in image pixels, or null if nothing usable is selected.</summary>
        private GraphicsPath GetSelectionPath(out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            if (_result == null)
            {
                return null;
            }
            var path = new GraphicsPath();
            if (_selectionShape == SelectionShape.Freehand)
            {
                if (_freehand.Count < 3)
                {
                    path.Dispose();
                    return null;
                }
                path.AddPolygon(_freehand.ToArray());
            }
            else
            {
                if (_selection.Width <= 0 || _selection.Height <= 0)
                {
                    path.Dispose();
                    return null;
                }
                if (_selectionShape == SelectionShape.Circle)
                {
                    path.AddEllipse(_selection);
                }
                else
                {
                    path.AddRectangle(_selection);
                }
            }

            bounds = Rectangle.Intersect(Rectangle.Ceiling(path.GetBounds()), new Rectangle(Point.Empty, _result.Size));
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                path.Dispose();
                return null;
            }
            return path;
        }

        private Bitmap ExtractSelection()
        {
            using (GraphicsPath path = GetSelectionPath(out Rectangle bounds))
            {
                if (path == null)
                {
                    return null;
                }
                var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.TranslateTransform(-bounds.X, -bounds.Y);
                    if (_selectionShape != SelectionShape.Rectangle)
                    {
                        g.SetClip(path);
                    }
                    g.DrawImage(_result, new Rectangle(0, 0, _result.Width, _result.Height));
                }
                return bmp;
            }
        }

        private void SetZoom(int zoom)
        {
            _zoom = Math.Max(1, Math.Min(16, zoom));
            labelZoom.Text = _zoom + "x";
            UpdateResultBox();
            UpdateStatus();
        }

        private void ZoomIn_Click(object sender, EventArgs e) => SetZoom(_zoom + 1);

        private void ZoomOut_Click(object sender, EventArgs e) => SetZoom(_zoom - 1);

        private void Reset_Click(object sender, EventArgs e)
        {
            SetZoom(1);
            ClearSelection();
            SetMessage("Zoom reset and selection removed.");
        }

        private void ClearSelection_Click(object sender, EventArgs e) => ClearSelection();

        #endregion

        #region Result – edit

        private bool CheckResult()
        {
            if (_result != null)
            {
                return true;
            }
            MessageBox.Show(this, "There is no result yet. Load an image first.", "Graphic Cutter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private void MirrorResult_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            PushUndo(null);
            _resultMirrored = !_resultMirrored;
            _result.RotateFlip(RotateFlipType.RotateNoneFlipX);
            ClearSelection();
            UpdateResultBox();
        }

        private void RotateResult_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            PushUndo(null);
            _resultRotation = (_resultRotation + 1) % 4;
            _result.RotateFlip(RotateFlipType.Rotate270FlipNone);
            ClearSelection();
            UpdateResultBox();
            UpdateStatus();
        }

        private void FillTexture_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            using (GraphicsPath path = GetSelectionPath(out Rectangle _))
            {
                if (path == null)
                {
                    MessageBox.Show(this, "Select an area in the result first.", "Fill texture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                using (var dialog = new OpenFileDialog { Filter = CutterImaging.ImageFilter, Title = "Choose a texture" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }
                    try
                    {
                        using (Bitmap texture = CutterImaging.LoadBitmap(dialog.FileName))
                        {
                            PushUndo(null);
                            Bitmap bmp = CutterImaging.To32Bpp(_result);
                            using (var g = Graphics.FromImage(bmp))
                            {
                                g.SetClip(path);
                                g.DrawImage(texture, path.GetBounds());
                            }
                            SetResult(bmp);
                            SetMessage("Texture filled into the selection. Moving the cut area replaces the result again (Ctrl+Z goes back).");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "The texture could not be loaded:\n" + ex.Message, "Fill texture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ShowMask_Click(object sender, EventArgs e)
        {
            _showMask = !_showMask;
            SyncUi();
            pictureBox2.Invalidate();
        }

        /// <summary>Split into art pieces: files, art.mul and / or the collection.</summary>
        private void CutStrips_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            StripSettings settings;
            using (var dialog = new StripsDialog(_result.Size))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                settings = dialog.Settings;
            }

            List<Bitmap> pieces = StripsDialog.Split(_result, settings);
            if (pieces.Count == 0)
            {
                MessageBox.Show(this, "All pieces are empty – nothing to do.", "Split into art pieces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var report = new List<string>();
            string folder = null;
            try
            {
                if (settings.SaveFiles)
                {
                    folder = Path.Combine(OutputDirectory, "Strips_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                    Directory.CreateDirectory(folder);
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        CutterImaging.SaveImage(pieces[i], Path.Combine(folder, "art" + (i + 1) + settings.Extension), settings.Fill);
                    }
                    report.Add($"{pieces.Count} files saved in {folder}");
                }

                if (settings.WriteToArt)
                {
                    List<int> ids = CutterArt.WriteMany(ArtKind.Item, settings.StartId, pieces, settings.SkipUsedIds, this);
                    foreach (int id in ids)
                    {
                        sidePanel.MarkUsed(ArtKind.Item, id, true);
                    }
                    if (ids.Count > 0)
                    {
                        report.Add($"{ids.Count} pieces written to art.mul: 0x{ids[0]:X4} – 0x{ids[ids.Count - 1]:X4} (save art.mul to keep them)");
                        sidePanel.SelectArtId(ArtKind.Item, ids[0]);
                    }
                    if (ids.Count < pieces.Count)
                    {
                        report.Add($"{pieces.Count - ids.Count} pieces did not fit before the last ID.");
                    }
                }

                if (settings.AddToCollection)
                {
                    for (int i = 0; i < pieces.Count; i++)
                    {
                        sidePanel.AddToCollection(pieces[i], $"strip {i + 1}");
                    }
                    ShowSidePanel(true);
                    report.Add($"{pieces.Count} pieces added to the collection");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Splitting failed:\n" + ex.Message, "Split into art pieces", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                foreach (Bitmap p in pieces)
                {
                    p.Dispose();
                }
                UpdateStatus();
            }

            SetMessage(string.Join(" · ", report));
            string text = string.Join("\n", report);
            if (folder != null)
            {
                if (MessageBox.Show(this, text + "\n\nOpen the folder?", "Split into art pieces", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    OpenFolder(folder);
                }
            }
            else
            {
                MessageBox.Show(this, text, "Split into art pieces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddToCollection_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            Rectangle cut = _shownCut;
            sidePanel.AddToCollection(_result, _work != null ? $"cut {cut.X},{cut.Y}" : "result");
            ShowSidePanel(true);
            UpdateStatus();
            SetMessage($"Added to the collection ({sidePanel.CollectionCount} pieces).");
        }

        #endregion

        #region Pipette

        private void Pipette_Click(object sender, EventArgs e)
        {
            _pipette = !_pipette;
            pictureBox1.Cursor = _pipette ? Cursors.Hand : Cursors.Default;
            pictureBox2.Cursor = _pipette ? Cursors.Hand : Cursors.Cross;
            SyncUi();
            SetMessage(_pipette ? "Pipette: click a color on the stage or in the result (Esc = cancel)." : "Pipette off.");
        }

        private void AddPickedColor(Color color)
        {
            _pipette = false;
            pictureBox1.Cursor = Cursors.Default;
            pictureBox2.Cursor = Cursors.Cross;
            Color opaque = Color.FromArgb(255, color);
            if (!_pickedColors.Any(c => c.ToArgb() == opaque.ToArgb()))
            {
                _pickedColors.Add(opaque);
            }
            UpdatePickedLabel();
            RebuildWork();
            SyncUi();
            SetMessage($"Color #{opaque.R:X2}{opaque.G:X2}{opaque.B:X2} is now transparent (tolerance {numericTolerance.Value}).");
        }

        private void PickStageColor(Point p)
        {
            if (_work == null)
            {
                return;
            }
            int x = p.X - _imagePos.X;
            int y = p.Y - _imagePos.Y;
            if (x < 0 || y < 0 || x >= _work.Width || y >= _work.Height)
            {
                SetMessage("Click inside the image.");
                return;
            }
            Color c = _work.GetPixel(x, y);
            if (c.A == 0)
            {
                SetMessage("This pixel is already transparent.");
                return;
            }
            AddPickedColor(c);
        }

        private void PickResultColor(Point p)
        {
            Point ip = ToImagePoint(p, false);
            if (ip.X < 0 || ip.Y < 0 || ip.X >= _result.Width || ip.Y >= _result.Height)
            {
                return;
            }
            Color c = _result.GetPixel(ip.X, ip.Y);
            if (c.A == 0)
            {
                SetMessage("This pixel is already transparent.");
                return;
            }
            AddPickedColor(c);
        }

        private void ClearPicked_Click(object sender, EventArgs e)
        {
            if (_pickedColors.Count == 0)
            {
                return;
            }
            _pickedColors.Clear();
            UpdatePickedLabel();
            RebuildWork();
            SetMessage("Picked colors removed.");
        }

        private void UpdatePickedLabel()
        {
            labelPicked.Text = _pickedColors.Count == 0 ? "Picked colors: none" : $"Picked colors: {_pickedColors.Count}";
            toolTip1.SetToolTip(labelPicked, _pickedColors.Count == 0
                ? Tip("Use the pipette in the toolbar to make more colors transparent.", "Mit der Pipette in der Werkzeugleiste weitere Farben transparent machen.")
                : string.Join("  ", _pickedColors.Select(c => $"#{c.R:X2}{c.G:X2}{c.B:X2}")));
            buttonClearPicked.Enabled = _pickedColors.Count > 0;
            labelPicked.Invalidate();
        }

        private void LabelPicked_Paint(object sender, PaintEventArgs e)
        {
            // Small color swatches behind the text.
            int x = TextRenderer.MeasureText(labelPicked.Text, labelPicked.Font).Width + 4;
            foreach (Color c in _pickedColors.Take(6))
            {
                using (var brush = new SolidBrush(c))
                {
                    e.Graphics.FillRectangle(brush, x, 2, 11, 11);
                }
                e.Graphics.DrawRectangle(Pens.Gray, x, 2, 11, 11);
                x += 14;
            }
        }

        #endregion

        #region Copy and save

        private void PlayCopySound()
        {
            if (!_playSound)
            {
                return;
            }
            try
            {
                string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sound.wav");
                if (!File.Exists(file))
                {
                    file = Path.GetFullPath("sound.wav");
                }
                if (!File.Exists(file))
                {
                    return;
                }
                _soundPlayer ??= new SoundPlayer();
                _soundPlayer.SoundLocation = file;
                _soundPlayer.Play();
            }
            catch (Exception)
            {
                // A missing or broken sound file must never stop the copy.
            }
        }

        private void CopyResult_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            try
            {
                CutterImaging.CopyToClipboard(_result);
                PlayCopySound();
                SetMessage("Result copied to the clipboard.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Copy failed:\n" + ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CopySelection_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            using (Bitmap part = ExtractSelection())
            {
                if (part == null)
                {
                    MessageBox.Show(this, "Select an area in the result first.", "Copy selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    CutterImaging.CopyToClipboard(part);
                    PlayCopySound();
                    SetMessage($"Selection {part.Width}×{part.Height} copied to the clipboard.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Copy failed:\n" + ex.Message, "Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            try
            {
                Directory.CreateDirectory(OutputDirectory);
                string file = Path.Combine(OutputDirectory, "ArtItem_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + SaveExtension);
                CutterImaging.SaveImage(_result, file, SaveFill);
                SetMessage("Saved: " + file);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "The image could not be saved:\n" + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAs_Click(object sender, EventArgs e)
        {
            if (!CheckResult())
            {
                return;
            }
            using (var dialog = new SaveFileDialog
            {
                Filter = "PNG|*.png|BMP|*.bmp|TIFF|*.tiff|JPEG|*.jpg",
                FileName = "ArtItem_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                Title = "Save result as"
            })
            {
                if (Directory.Exists(OutputDirectory))
                {
                    dialog.InitialDirectory = OutputDirectory;
                }
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    CutterImaging.SaveImage(_result, dialog.FileName, SaveFill);
                    SetMessage("Saved: " + dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The image could not be saved:\n" + ex.Message, "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                // Explorer not available – nothing to do.
            }
        }

        private void OpenOutputFolder_Click(object sender, EventArgs e) => OpenFolder(OutputDirectory);

        private void SaveArt_Click(object sender, EventArgs e)
        {
            if (!CutterArt.IsAvailable)
            {
                MessageBox.Show(this, "art.mul is not loaded.", "Save art.mul", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, $"Save artidx.mul and art.mul into:\n{UoFiddler.Controls.Classes.Options.OutputPath}", "Save art.mul",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                SetMessage("art.mul saved in " + CutterArt.Save());
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Saving failed:\n" + ex.Message, "Save art.mul", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void Sound_Click(object sender, EventArgs e)
        {
            _playSound = !_playSound;
            SyncUi();
        }

        #endregion

        #region Stage options

        private void MirrorStage_Click(object sender, EventArgs e)
        {
            if (_source == null)
            {
                SetMessage("No image is loaded on the stage.");
                return;
            }
            PushUndo(null);
            _stageMirrored = !_stageMirrored;
            RebuildWork();
            SyncUi();
        }

        private void TransparentCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RebuildWork();
            SyncUi();
        }

        private void Transparent_Click(object sender, EventArgs e) => checkBoxTransparent.Checked = !checkBoxTransparent.Checked;

        private void Tolerance_ValueChanged(object sender, EventArgs e)
        {
            if (checkBoxTransparent.Checked || _pickedColors.Count > 0)
            {
                RebuildWork();
            }
        }

        private void AutoTrim_Click(object sender, EventArgs e)
        {
            _autoTrim = !_autoTrim;
            RebuildWork();
            SyncUi();
            SetMessage(_autoTrim ? "Transparent border removed." : "Transparent border restored.");
        }

        private void Bake_Click(object sender, EventArgs e) => checkBoxBake.Checked = !checkBoxBake.Checked;

        private void Bake_CheckedChanged(object sender, EventArgs e)
        {
            _bakeStage = checkBoxBake.Checked;
            UpdateResult();
            SyncUi();
        }

        private void CenterImage_Click(object sender, EventArgs e)
        {
            PushUndo(null);
            CenterImage();
        }

        private void Grid_Click(object sender, EventArgs e)
        {
            _showGrid = !_showGrid;
            SyncUi();
            pictureBox1.Invalidate();
            if (_bakeStage)
            {
                UpdateResult();
            }
        }

        private void GridResult_Click(object sender, EventArgs e)
        {
            _showGridOnResult = !_showGridOnResult;
            SyncUi();
            pictureBox2.Invalidate();
        }

        private void GridColor_Click(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog
            {
                Color = _gridColor,
                FullOpen = true,
                CustomColors = new[]
                {
                    ColorTranslator.ToOle(Color.Yellow), ColorTranslator.ToOle(Color.Blue), ColorTranslator.ToOle(Color.Orange),
                    ColorTranslator.ToOle(Color.Pink), ColorTranslator.ToOle(Color.White), ColorTranslator.ToOle(Color.DarkBlue),
                    ColorTranslator.ToOle(Color.DarkRed), ColorTranslator.ToOle(Color.Brown)
                }
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                _gridColor = dialog.Color;
                pictureBox1.Invalidate();
                pictureBox2.Invalidate();
                if (_bakeStage)
                {
                    UpdateResult();
                }
            }
        }

        private void Border_Click(object sender, EventArgs e)
        {
            _showBorder = !_showBorder;
            SyncUi();
            pictureBox1.Invalidate();
        }

        private void CutFrame_Click(object sender, EventArgs e)
        {
            _showCutFrame = !_showCutFrame;
            SyncUi();
            pictureBox1.Invalidate();
        }

        private void Ruler_Click(object sender, EventArgs e)
        {
            _showRuler = !_showRuler;
            SyncUi();
            panelStage.Invalidate();
        }

        private void TopMost_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            SyncUi();
        }

        private void ShowSidePanel(bool visible)
        {
            sidePanel.Visible = visible;
            mainLayout.ColumnStyles[2].Width = visible ? SidePanelWidth : 0;
            SyncUi();
        }

        private void SidePanel_Click(object sender, EventArgs e) => ShowSidePanel(!sidePanel.Visible);

        private void Help_Click(object sender, EventArgs e) =>
            GraphicCutterHelp.Show(this, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "de");

        private void HelpEnglish_Click(object sender, EventArgs e) => GraphicCutterHelp.Show(this, false);

        private void HelpGerman_Click(object sender, EventArgs e) => GraphicCutterHelp.Show(this, true);

        #endregion

        #region Background

        private void SetStageBackColor(Color color)
        {
            pictureBox1.BackColor = color;
            pictureBox1.Invalidate();
            if (_bakeStage)
            {
                UpdateResult();
            }
        }

        private void BgBlack_Click(object sender, EventArgs e) => SetStageBackColor(Color.Black);

        private void BgWhite_Click(object sender, EventArgs e) => SetStageBackColor(Color.White);

        private void BackColorToggle_Click(object sender, EventArgs e) =>
            SetStageBackColor(pictureBox1.BackColor.ToArgb() == Color.Black.ToArgb() ? Color.White : Color.Black);

        private void BgColor_Click(object sender, EventArgs e)
        {
            using (var dialog = new ColorDialog { Color = pictureBox1.BackColor, FullOpen = true })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SetStageBackColor(dialog.Color);
                }
            }
        }

        private void BgHex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }
            e.Handled = true;
            e.SuppressKeyPress = true;

            string code = bgHexTextBox.Text.Trim().TrimStart('#');
            if (code.Length == 6 && int.TryParse(code, NumberStyles.HexNumber, null, out int rgb))
            {
                SetStageBackColor(Color.FromArgb(255, Color.FromArgb(rgb)));
                SetMessage("Background color #" + code.ToUpperInvariant());
            }
            else
            {
                MessageBox.Show(this, "Please enter a valid 6-digit color code. Example: FF0000 for red.", "Invalid color code",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Loads a land tile from art.mul and shows it as ground on the stage.</summary>
        private void SetLandBackground(int landId)
        {
            Bitmap tile = CutterArt.GetCopy(ArtKind.Land, landId);
            if (tile == null)
            {
                MessageBox.Show(this, $"Land tile 0x{landId:X4} is not available. Is art.mul loaded?", "Background",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            ReplaceBitmap(ref _landTile, tile);
            _landTileId = landId;
            if (tsBackground.SelectedIndex == 3)
            {
                BackgroundCombo_SelectedIndexChanged(this, EventArgs.Empty);
            }
            else
            {
                tsBackground.SelectedIndex = 3;
            }
            SetMessage($"Stage ground: land tile 0x{landId:X4}.");
        }

        private void BackgroundCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Properties.Resources returns a new bitmap on every access.
            _stageBackImage?.Dispose();
            _stageBackImage = null;
            switch (tsBackground.SelectedIndex)
            {
                case 1:
                    _stageBackImage = Properties.Resources.green;
                    break;
                case 2:
                    _stageBackImage = Properties.Resources.water;
                    break;
                case 3:
                    if (_landTile == null)
                    {
                        _landTile = CutterArt.GetCopy(ArtKind.Land, _landTileId);
                        if (_landTile == null)
                        {
                            SetMessage("art.mul is not loaded – no land tile available.");
                            tsBackground.SelectedIndex = 0;
                            return;
                        }
                    }
                    break;
            }
            SyncUi();
            pictureBox1.Invalidate();
            if (_bakeStage)
            {
                UpdateResult();
            }
        }

        private void BgImageNone_Click(object sender, EventArgs e) => tsBackground.SelectedIndex = 0;

        private void BgImageGreen_Click(object sender, EventArgs e) => tsBackground.SelectedIndex = 1;

        private void BgImageWater_Click(object sender, EventArgs e) => tsBackground.SelectedIndex = 2;

        private void BgLand_Click(object sender, EventArgs e) => tsBackground.SelectedIndex = 3;

        #endregion

        #region Keyboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool shift = (keyData & Keys.Shift) == Keys.Shift;
            bool alt = (keyData & Keys.Alt) == Keys.Alt;
            bool textFocus = IsTextInputFocused();

            if (key == Keys.F1 && !ctrl && !alt)
            {
                Help_Click(this, EventArgs.Empty);
                return true;
            }

            if (!alt)
            {
                // Arrows: image (or cut area with Ctrl). Left alone while a number field, text box or list is used.
                if (!textFocus && (key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right))
                {
                    int step = shift ? 10 : 1;
                    int dx = key == Keys.Left ? -step : key == Keys.Right ? step : 0;
                    int dy = key == Keys.Up ? -step : key == Keys.Down ? step : 0;
                    if (ctrl)
                    {
                        PushUndo("move-cut");
                        MoveCutBy(dx, dy);
                    }
                    else
                    {
                        PushUndo("move-image");
                        MoveImageBy(dx, dy);
                    }
                    return true;
                }

                if (ctrl)
                {
                    switch (key)
                    {
                        case Keys.O:
                            OpenImage_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.S:
                            if (shift)
                            {
                                SaveAs_Click(this, EventArgs.Empty);
                            }
                            else
                            {
                                Save_Click(this, EventArgs.Empty);
                            }
                            return true;
                        case Keys.G:
                            Grid_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.B:
                            SidePanel_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.Enter:
                            AddToCollection_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.Oemplus:
                        case Keys.Add:
                            SetZoom(_zoom + 1);
                            return true;
                        case Keys.OemMinus:
                        case Keys.Subtract:
                            SetZoom(_zoom - 1);
                            return true;
                    }

                    if (!textFocus)
                    {
                        switch (key)
                        {
                            case Keys.V:
                                ImportClipboard_Click(this, EventArgs.Empty);
                                return true;
                            case Keys.Z:
                                if (shift)
                                {
                                    Redo_Click(this, EventArgs.Empty);
                                }
                                else
                                {
                                    Undo_Click(this, EventArgs.Empty);
                                }
                                return true;
                            case Keys.Y:
                                Redo_Click(this, EventArgs.Empty);
                                return true;
                            case Keys.C:
                            case Keys.X: // Ctrl+X was the old shortcut for copying.
                                if (shift)
                                {
                                    CopySelection_Click(this, EventArgs.Empty);
                                }
                                else
                                {
                                    CopyResult_Click(this, EventArgs.Empty);
                                }
                                return true;
                        }
                    }
                }
                else if (key == Keys.Escape && !textFocus)
                {
                    if (_pipette)
                    {
                        Pipette_Click(this, EventArgs.Empty);
                    }
                    else
                    {
                        ClearSelection();
                    }
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion
    }
}

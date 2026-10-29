/***************************************************************************
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
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.MapViewer.Help;

namespace UoFiddler.Controls.MultiMapImages
{
    /// <summary>
    /// Simple paint program for map images: draw, shapes, labels, map symbols, fill, select/crop,
    /// undo/redo, clipboard, print and save (inside the app or in any folder).
    /// </summary>
    public class MapImageEditorForm : Form
    {
        private enum Tool
        {
            Hand,
            Pen,
            Line,
            Arrow,
            Rectangle,
            Ellipse,
            Text,
            Eraser,
            Fill,
            Picker,
            Symbol,
            Select
        }

        private const long UndoBudgetBytes = 600L * 1024 * 1024;

        private static readonly string[] _toolHints =
        {
            "Hand: drag to move the image.",
            "Pen: draw with the left mouse button (right button = paper color).",
            "Line: drag. Shift = 45° steps.",
            "Arrow: drag from the start to the tip. Shift = 45° steps.",
            "Rectangle: drag. Shift = square. \"Filled\" fills it.",
            "Ellipse: drag. Shift = circle. \"Filled\" fills it.",
            "Text: click where the label should be, enter the text, then click to place it.",
            "Eraser: paints with the paper color.",
            "Fill: fills the area with the color (right button = paper color).",
            "Color picker: left = drawing color, right = paper color.",
            "Symbol: click to place the selected symbol (mountain, tree, house, ankh, ...).",
            "Select: drag a rectangle - then copy (Ctrl+C), cut (Ctrl+X), delete (Del) or crop."
        };

        // remembered between openings
        private static Color _color = Color.Black;
        private static Color _paper = Color.White;
        private static float _penWidth = 2f;
        private static bool _filled;
        private static bool _smooth = true;
        private static MapSymbol _symbol = MapSymbol.Mountain;
        private static float _symbolSize = 24f;
        private static int _tolerance = 32;

        private readonly ZoomCanvas _canvas = new ZoomCanvas();
        private readonly Dictionary<Tool, ToolStripButton> _toolButtons = new Dictionary<Tool, ToolStripButton>();
        private readonly LinkedList<Bitmap> _undo = new LinkedList<Bitmap>();
        private readonly Stack<Bitmap> _redo = new Stack<Bitmap>();
        private readonly Random _random = new Random();

        private Bitmap _image;
        private string _name;
        private MapImageEntry _entry;
        private string _externalFile;
        private bool _modified;

        private Tool _tool = Tool.Pen;
        private Tool _toolBeforeFloating = Tool.Pen;
        private bool _drawing;
        private MouseButtons _drawButton;
        private PointF _start;
        private PointF _current;
        private PointF _last;
        private PointF _hover;
        private bool _hoverInside;
        private Rectangle? _selection;
        private Bitmap _floating;
        private bool _spacePan;

        private ToolStripButton _colorButton;
        private ToolStripButton _paperButton;
        private ToolStripComboBox _widthCombo;
        private ToolStripButton _filledButton;
        private ToolStripButton _smoothButton;
        private ToolStripComboBox _symbolCombo;
        private ToolStripComboBox _symbolSizeCombo;
        private ToolStripComboBox _toleranceCombo;
        private ToolStripStatusLabel _statusHint;
        private ToolStripStatusLabel _statusPos;
        private ToolStripStatusLabel _statusSize;
        private ToolStripStatusLabel _statusZoom;
        private ToolStripMenuItem _undoItem;
        private ToolStripMenuItem _redoItem;

        /// <summary>Raised after saving. Entry is null if the file is not in the list.</summary>
        public event Action<MapImageEntry, string> ImageSaved;

        /// <param name="image">Image to edit (a copy is made)</param>
        /// <param name="name">Name for saving and printing</param>
        /// <param name="entry">List entry when the image comes from the "Load.." list (Save overwrites it)</param>
        public MapImageEditorForm(Image image, string name, MapImageEntry entry)
        {
            _name = string.IsNullOrWhiteSpace(name) ? "Map image" : name;
            _entry = entry;
            _image = ToArgb(image);

            Icon = Options.GetFiddlerIcon();
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 800);
            MinimumSize = new Size(700, 450);
            KeyPreview = true;

            BuildLayout();
            _canvas.Image = _image;
            SetTool(Tool.Pen);
            UpdateTitle();
            UpdateStatus();
            Shown += (s, e) =>
            {
                _canvas.ZoomToFit();
                _canvas.Focus();
            };
        }

        private static Bitmap ToArgb(Image image)
        {
            var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            bmp.SetResolution(96, 96);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            }

            return bmp;
        }

        #region Layout

        private void BuildLayout()
        {
            // menu
            var menu = new MenuStrip();
            var file = new ToolStripMenuItem("File");
            file.DropDownItems.Add(MakeItem("Save", Keys.Control | Keys.S, (s, e) => Save()));
            file.DropDownItems.Add(MakeItem("Save as...", Keys.Control | Keys.Shift | Keys.S, (s, e) => SaveAs()));
            file.DropDownItems.Add(MakeItem("Save a copy in the \"Load..\" list", Keys.None, (s, e) => SaveCopyToList()));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(MakeItem("Print...", Keys.Control | Keys.P, (s, e) => MapImagePrinter.Print(this, _image, _name)));
            file.DropDownItems.Add(MakeItem("Print preview...", Keys.None, (s, e) => MapImagePrinter.Preview(this, _image, _name)));
            file.DropDownItems.Add(new ToolStripSeparator());
            file.DropDownItems.Add(MakeItem("Close", Keys.None, (s, e) => Close()));

            var edit = new ToolStripMenuItem("Edit");
            _undoItem = MakeItem("Undo", Keys.Control | Keys.Z, (s, e) => Undo());
            _redoItem = MakeItem("Redo", Keys.Control | Keys.Y, (s, e) => Redo());
            edit.DropDownItems.Add(_undoItem);
            edit.DropDownItems.Add(_redoItem);
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(MakeItem("Copy (selection or whole image)", Keys.Control | Keys.C, (s, e) => CopyToClipboard()));
            edit.DropDownItems.Add(MakeItem("Cut selection", Keys.Control | Keys.X, (s, e) => CutSelection()));
            edit.DropDownItems.Add(MakeItem("Paste (place with a click)", Keys.Control | Keys.V, (s, e) => PasteFloating()));
            edit.DropDownItems.Add(MakeItem("Paste as new image", Keys.Control | Keys.Shift | Keys.V, (s, e) => PasteAsNewImage()));
            edit.DropDownItems.Add(new ToolStripSeparator());
            edit.DropDownItems.Add(MakeItem("Select all", Keys.Control | Keys.A, (s, e) => SelectAll()));
            edit.DropDownItems.Add(MakeItem("Delete selection", Keys.Delete, (s, e) => DeleteSelection()));
            edit.DropDownItems.Add(MakeItem("Crop to selection", Keys.Control | Keys.Shift | Keys.X, (s, e) => CropToSelection()));

            var imageMenu = new ToolStripMenuItem("Image");
            imageMenu.DropDownItems.Add(MakeItem("Rotate 90° right", Keys.None, (s, e) => RotateFlip(RotateFlipType.Rotate90FlipNone)));
            imageMenu.DropDownItems.Add(MakeItem("Rotate 90° left", Keys.None, (s, e) => RotateFlip(RotateFlipType.Rotate270FlipNone)));
            imageMenu.DropDownItems.Add(MakeItem("Flip horizontal", Keys.None, (s, e) => RotateFlip(RotateFlipType.RotateNoneFlipX)));
            imageMenu.DropDownItems.Add(MakeItem("Flip vertical", Keys.None, (s, e) => RotateFlip(RotateFlipType.RotateNoneFlipY)));
            imageMenu.DropDownItems.Add(new ToolStripSeparator());
            imageMenu.DropDownItems.Add(MakeItem("Resize...", Keys.Control | Keys.R, (s, e) => ResizeImage()));
            imageMenu.DropDownItems.Add(new ToolStripSeparator());
            imageMenu.DropDownItems.Add(MakeItem("Grayscale", Keys.None, (s, e) => ApplyPixels(PixelOp.Gray)));
            imageMenu.DropDownItems.Add(MakeItem("Black / white (threshold)", Keys.None, (s, e) => ApplyPixels(PixelOp.BlackWhite)));
            imageMenu.DropDownItems.Add(MakeItem("Invert colors", Keys.None, (s, e) => ApplyPixels(PixelOp.Invert)));
            imageMenu.DropDownItems.Add(MakeItem("Sepia (old map)", Keys.None, (s, e) => ApplyPixels(PixelOp.Sepia)));

            var view = new ToolStripMenuItem("View");
            view.DropDownItems.Add(MakeItem("Zoom in", Keys.Control | Keys.Oemplus, (s, e) => _canvas.ZoomIn()));
            view.DropDownItems.Add(MakeItem("Zoom out", Keys.Control | Keys.OemMinus, (s, e) => _canvas.ZoomOut()));
            view.DropDownItems.Add(MakeItem("Actual size (100 %)", Keys.Control | Keys.D1, (s, e) => _canvas.ZoomActualSize()));
            view.DropDownItems.Add(MakeItem("Fit to window", Keys.Control | Keys.D0, (s, e) => _canvas.ZoomToFit()));

            var help = new ToolStripMenuItem("Help");
            help.DropDownItems.Add(MakeItem("Help", Keys.F1, (s, e) => MapHelpForm.ShowTopic(this, MapHelpContent.MultiMapImages)));

            menu.Items.AddRange(new ToolStripItem[] { file, edit, imageMenu, view, help });
            MainMenuStrip = menu;

            // options bar
            var options = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            _colorButton = new ToolStripButton("Color") { ToolTipText = "Drawing color (left mouse button)" };
            _colorButton.Click += (s, e) => PickColor(true);
            _paperButton = new ToolStripButton("Paper") { ToolTipText = "Paper color: eraser, right mouse button, delete" };
            _paperButton.Click += (s, e) => PickColor(false);
            _widthCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDown, Width = 55, ToolTipText = "Line width (pixels)" };
            _widthCombo.Items.AddRange(new object[] { "1", "2", "3", "4", "6", "8", "12", "16", "24", "32", "48", "64" });
            _widthCombo.Text = _penWidth.ToString("0.#");
            _widthCombo.TextChanged += (s, e) =>
            {
                if (float.TryParse(_widthCombo.Text, out float w) && w > 0 && w <= 500)
                {
                    _penWidth = w;
                    _canvas.Invalidate();
                }
            };
            _filledButton = new ToolStripButton("Filled") { CheckOnClick = true, Checked = _filled, ToolTipText = "Fill rectangles and ellipses" };
            _filledButton.CheckedChanged += (s, e) => _filled = _filledButton.Checked;
            _smoothButton = new ToolStripButton("Smooth") { CheckOnClick = true, Checked = _smooth, ToolTipText = "Anti-aliasing (off = sharp pixels, e.g. for black/white maps)" };
            _smoothButton.CheckedChanged += (s, e) => _smooth = _smoothButton.Checked;

            _symbolCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100, ToolTipText = "Symbol" };
            foreach (MapSymbol sym in Enum.GetValues(typeof(MapSymbol)))
            {
                _symbolCombo.Items.Add(sym);
            }

            _symbolCombo.SelectedItem = _symbol;
            _symbolCombo.SelectedIndexChanged += (s, e) =>
            {
                _symbol = (MapSymbol)_symbolCombo.SelectedItem;
                SetTool(Tool.Symbol);
                _canvas.Focus();
            };
            _symbolSizeCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDown, Width = 50, ToolTipText = "Symbol size (pixels)" };
            _symbolSizeCombo.Items.AddRange(new object[] { "8", "12", "16", "20", "24", "32", "48", "64", "96", "128" });
            _symbolSizeCombo.Text = _symbolSize.ToString("0");
            _symbolSizeCombo.TextChanged += (s, e) =>
            {
                if (float.TryParse(_symbolSizeCombo.Text, out float v) && v >= 4 && v <= 1000)
                {
                    _symbolSize = v;
                    _canvas.Invalidate();
                }
            };
            _toleranceCombo = new ToolStripComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50, ToolTipText = "Fill tolerance" };
            _toleranceCombo.Items.AddRange(new object[] { "0", "16", "32", "64", "96", "128" });
            _toleranceCombo.SelectedItem = _tolerance.ToString();
            if (_toleranceCombo.SelectedIndex < 0)
            {
                _toleranceCombo.SelectedIndex = 2;
            }

            _toleranceCombo.SelectedIndexChanged += (s, e) => _tolerance = int.Parse((string)_toleranceCombo.SelectedItem);

            options.Items.AddRange(new ToolStripItem[]
            {
                _colorButton, _paperButton, new ToolStripSeparator(),
                new ToolStripLabel("Width"), _widthCombo, _filledButton, _smoothButton, new ToolStripSeparator(),
                new ToolStripLabel("Symbol"), _symbolCombo, _symbolSizeCombo, new ToolStripSeparator(),
                new ToolStripLabel("Tolerance"), _toleranceCombo, new ToolStripSeparator(),
                new ToolStripButton("Undo", null, (s, e) => Undo()) { ToolTipText = "Ctrl+Z" },
                new ToolStripButton("Redo", null, (s, e) => Redo()) { ToolTipText = "Ctrl+Y" },
                new ToolStripSeparator(),
                new ToolStripButton("Copy", null, (s, e) => CopyToClipboard()) { ToolTipText = "Ctrl+C - selection or whole image" },
                new ToolStripButton("Print...", null, (s, e) => MapImagePrinter.Print(this, _image, _name)),
                new ToolStripButton("Save", null, (s, e) => Save()) { ToolTipText = "Ctrl+S" },
                new ToolStripSeparator(),
                new ToolStripButton("+", null, (s, e) => _canvas.ZoomIn()) { ToolTipText = "Zoom in (mouse wheel)" },
                new ToolStripButton("-", null, (s, e) => _canvas.ZoomOut()) { ToolTipText = "Zoom out (mouse wheel)" },
                new ToolStripButton("1:1", null, (s, e) => _canvas.ZoomActualSize()),
                new ToolStripButton("Fit", null, (s, e) => _canvas.ZoomToFit())
            });
            UpdateColorButtons();

            // tools
            var tools = new ToolStrip
            {
                Dock = DockStyle.Left,
                GripStyle = ToolStripGripStyle.Hidden,
                LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow,
                AutoSize = false,
                Width = 92
            };
            AddTool(tools, Tool.Hand, "Hand", "H");
            AddTool(tools, Tool.Select, "Select", "M");
            tools.Items.Add(new ToolStripSeparator());
            AddTool(tools, Tool.Pen, "Pen", "P");
            AddTool(tools, Tool.Eraser, "Eraser", "E");
            AddTool(tools, Tool.Line, "Line", "L");
            AddTool(tools, Tool.Arrow, "Arrow", "A");
            AddTool(tools, Tool.Rectangle, "Rectangle", "R");
            AddTool(tools, Tool.Ellipse, "Ellipse", "O");
            tools.Items.Add(new ToolStripSeparator());
            AddTool(tools, Tool.Text, "Text", "T");
            AddTool(tools, Tool.Symbol, "Symbol", "S");
            AddTool(tools, Tool.Fill, "Fill", "F");
            AddTool(tools, Tool.Picker, "Picker", "I");

            // status
            var status = new StatusStrip();
            _statusHint = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _statusPos = new ToolStripStatusLabel { Width = 110, AutoSize = false };
            _statusSize = new ToolStripStatusLabel { Width = 120, AutoSize = false };
            _statusZoom = new ToolStripStatusLabel { Width = 70, AutoSize = false };
            status.Items.AddRange(new ToolStripItem[] { _statusHint, _statusPos, _statusSize, _statusZoom });

            // canvas
            _canvas.Dock = DockStyle.Fill;
            _canvas.MouseDown += OnCanvasMouseDown;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseUp += OnCanvasMouseUp;
            _canvas.MouseLeave += (s, e) =>
            {
                _hoverInside = false;
                _canvas.Invalidate();
            };
            _canvas.PaintOverlay += OnPaintOverlay;
            _canvas.ZoomChanged += (s, e) => UpdateStatus();

            Controls.Add(_canvas);
            Controls.Add(tools);
            Controls.Add(options);
            Controls.Add(status);
            Controls.Add(menu);
        }

        private static ToolStripMenuItem MakeItem(string text, Keys keys, EventHandler click)
        {
            var item = new ToolStripMenuItem(text, null, click);
            if (keys != Keys.None)
            {
                item.ShortcutKeys = keys;
                if (keys == Keys.Delete)
                {
                    item.ShortcutKeyDisplayString = "Del";
                }
            }

            return item;
        }

        private void AddTool(ToolStrip strip, Tool tool, string text, string key)
        {
            var button = new ToolStripButton($"{text}") { ToolTipText = $"{text} ({key})", DisplayStyle = ToolStripItemDisplayStyle.Text, TextAlign = ContentAlignment.MiddleLeft };
            button.Click += (s, e) =>
            {
                SetTool(tool);
                _canvas.Focus();
            };
            _toolButtons[tool] = button;
            strip.Items.Add(button);
        }

        private void SetTool(Tool tool)
        {
            if (_floating != null && tool != _tool)
            {
                DropFloating();
            }

            _tool = tool;
            _drawing = false;
            foreach (KeyValuePair<Tool, ToolStripButton> kv in _toolButtons)
            {
                kv.Value.Checked = kv.Key == tool;
            }

            _canvas.PanWithLeftButton = tool == Tool.Hand || _spacePan;
            Cursor cursor = tool == Tool.Hand ? Cursors.Hand : tool == Tool.Text ? Cursors.IBeam : Cursors.Cross;
            _canvas.Cursor = cursor;
            _canvas.DefaultCursorForTool = cursor;
            _statusHint.Text = _toolHints[(int)tool];
            _canvas.Invalidate();
        }

        private void UpdateColorButtons()
        {
            _colorButton.Image?.Dispose();
            _paperButton.Image?.Dispose();
            _colorButton.Image = Swatch(_color);
            _paperButton.Image = Swatch(_paper);
        }

        private static Bitmap Swatch(Color c)
        {
            var bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            using (var b = new SolidBrush(c))
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(b, 1, 1, 14, 14);
                g.DrawRectangle(Pens.Gray, 0, 0, 15, 15);
            }

            return bmp;
        }

        private void PickColor(bool drawing)
        {
            using (var dialog = new ColorDialog { Color = drawing ? _color : _paper, FullOpen = true })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                if (drawing)
                {
                    _color = dialog.Color;
                }
                else
                {
                    _paper = dialog.Color;
                }

                UpdateColorButtons();
            }
        }

        private void UpdateTitle()
        {
            string where = _entry != null ? (_entry.IsInternal ? "list" : _entry.FilePath) : _externalFile ?? "not saved";
            Text = $"Map image editor - {_name}{(_modified ? " *" : string.Empty)}   [{where}]";
        }

        private void UpdateStatus()
        {
            _statusSize.Text = $"{_image.Width} x {_image.Height}";
            _statusZoom.Text = $"{_canvas.Zoom * 100:0} %";
            _undoItem.Enabled = _undo.Count > 0;
            _redoItem.Enabled = _redo.Count > 0;
        }

        private void SetModified()
        {
            if (!_modified)
            {
                _modified = true;
                UpdateTitle();
            }

            UpdateStatus();
        }

        #endregion

        #region Undo

        private void PushUndo()
        {
            _undo.AddLast(new Bitmap(_image));
            ClearRedo();
            long bytes = 0;
            foreach (Bitmap b in _undo)
            {
                bytes += (long)b.Width * b.Height * 4;
            }

            while (_undo.Count > 3 && bytes > UndoBudgetBytes)
            {
                Bitmap first = _undo.First.Value;
                bytes -= (long)first.Width * first.Height * 4;
                first.Dispose();
                _undo.RemoveFirst();
            }

            SetModified();
        }

        private void ClearRedo()
        {
            while (_redo.Count > 0)
            {
                _redo.Pop().Dispose();
            }
        }

        private void Undo()
        {
            if (_undo.Count == 0)
            {
                return;
            }

            DropFloating();
            _redo.Push(_image);
            Bitmap prev = _undo.Last.Value;
            _undo.RemoveLast();
            SetImage(prev);
            SetModified();
        }

        private void Redo()
        {
            if (_redo.Count == 0)
            {
                return;
            }

            DropFloating();
            _undo.AddLast(_image);
            SetImage(_redo.Pop());
            SetModified();
        }

        /// <summary>Shows another bitmap (the old one is not disposed - it is in the undo/redo list)</summary>
        private void SetImage(Bitmap bmp)
        {
            _image = bmp;
            _canvas.Image = null;
            _canvas.Image = _image;
            if (_selection.HasValue)
            {
                Rectangle r = Rectangle.Intersect(_selection.Value, new Rectangle(0, 0, _image.Width, _image.Height));
                _selection = r.Width > 0 && r.Height > 0 ? r : (Rectangle?)null;
            }

            UpdateStatus();
        }

        /// <summary>Replaces the image after a step that was saved with <see cref="PushUndo"/></summary>
        private void ReplaceImage(Bitmap bmp)
        {
            // the old image is still referenced by the undo list (PushUndo made a copy), so it can go
            Bitmap old = _image;
            SetImage(bmp);
            old.Dispose();
        }

        #endregion

        #region Mouse

        private PointF ImagePoint(Point client)
        {
            return _canvas.ClientToImage(client);
        }

        private Color ButtonColor(MouseButtons b) => b == MouseButtons.Right ? _paper : _color;

        private void OnCanvasMouseDown(object sender, MouseEventArgs e)
        {
            if (_canvas.IsPanning || e.Button == MouseButtons.Middle || _tool == Tool.Hand || _spacePan)
            {
                return;
            }

            PointF p = ImagePoint(e.Location);
            _hover = p;

            if (_floating != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    CommitFloating();
                }
                else
                {
                    DropFloating();
                }

                return;
            }

            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
            {
                return;
            }

            switch (_tool)
            {
                case Tool.Pen:
                case Tool.Eraser:
                    PushUndo();
                    _drawing = true;
                    _drawButton = e.Button;
                    _last = p;
                    DrawStroke(p, p);
                    break;
                case Tool.Line:
                case Tool.Arrow:
                case Tool.Rectangle:
                case Tool.Ellipse:
                case Tool.Select:
                    _drawing = true;
                    _drawButton = e.Button;
                    _start = _current = p;
                    if (_tool == Tool.Select)
                    {
                        _selection = null;
                    }

                    break;
                case Tool.Text:
                    StartText(p);
                    break;
                case Tool.Fill:
                    if (Inside(p))
                    {
                        PushUndo();
                        FloodFill((int)p.X, (int)p.Y, ButtonColor(e.Button));
                        _canvas.Invalidate();
                    }

                    break;
                case Tool.Picker:
                    if (Inside(p))
                    {
                        Color c = _image.GetPixel((int)p.X, (int)p.Y);
                        if (e.Button == MouseButtons.Right)
                        {
                            _paper = c;
                        }
                        else
                        {
                            _color = c;
                        }

                        UpdateColorButtons();
                    }

                    break;
                case Tool.Symbol:
                    PlaceSymbol(p, e.Button);
                    break;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            PointF p = ImagePoint(e.Location);
            PointF oldHover = _hover;
            _hover = p;
            _hoverInside = true;
            _statusPos.Text = Inside(p) ? $"{(int)p.X}, {(int)p.Y}" : string.Empty;

            if (_canvas.IsPanning)
            {
                return;
            }

            if (_drawing)
            {
                switch (_tool)
                {
                    case Tool.Pen:
                    case Tool.Eraser:
                        DrawStroke(_last, p);
                        _last = p;
                        return;
                    default:
                        _current = (ModifierKeys & Keys.Shift) != 0 ? Constrain(_start, p, _tool) : p;
                        _canvas.Invalidate();
                        return;
                }
            }

            if (_floating != null || _tool == Tool.Symbol || _tool == Tool.Pen || _tool == Tool.Eraser)
            {
                // only repaint around the cursor preview
                float r = _floating != null ? Math.Max(_floating.Width, _floating.Height) : Math.Max(_symbolSize * 1.5f, _penWidth) + 4;
                _canvas.InvalidateImageRect(new RectangleF(oldHover.X - r, oldHover.Y - r, 2 * r, 2 * r));
                _canvas.InvalidateImageRect(new RectangleF(p.X - r, p.Y - r, 2 * r, 2 * r));
            }
        }

        private void OnCanvasMouseUp(object sender, MouseEventArgs e)
        {
            if (!_drawing || e.Button != _drawButton)
            {
                return;
            }

            _drawing = false;
            switch (_tool)
            {
                case Tool.Line:
                case Tool.Arrow:
                case Tool.Rectangle:
                case Tool.Ellipse:
                    if (Distance(_start, _current) >= 1)
                    {
                        PushUndo();
                        using (Graphics g = Graphics.FromImage(_image))
                        {
                            g.SmoothingMode = _smooth ? SmoothingMode.AntiAlias : SmoothingMode.None;
                            DrawShape(g, _tool, _start, _current, ButtonColor(_drawButton), _penWidth, _filled);
                        }
                    }

                    _canvas.Invalidate();
                    break;
                case Tool.Select:
                    Rectangle r = Normalize(_start, _current);
                    r.Intersect(new Rectangle(0, 0, _image.Width, _image.Height));
                    _selection = r.Width > 1 && r.Height > 1 ? r : (Rectangle?)null;
                    _canvas.Invalidate();
                    break;
            }
        }

        private bool Inside(PointF p) => p.X >= 0 && p.Y >= 0 && p.X < _image.Width && p.Y < _image.Height;

        private static float Distance(PointF a, PointF b) => (float)Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        private static Rectangle Normalize(PointF a, PointF b)
        {
            int x1 = (int)Math.Floor(Math.Min(a.X, b.X)), y1 = (int)Math.Floor(Math.Min(a.Y, b.Y));
            int x2 = (int)Math.Ceiling(Math.Max(a.X, b.X)), y2 = (int)Math.Ceiling(Math.Max(a.Y, b.Y));
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        /// <summary>Shift: lines in 45° steps, squares and circles</summary>
        private static PointF Constrain(PointF start, PointF p, Tool tool)
        {
            float dx = p.X - start.X, dy = p.Y - start.Y;
            if (tool == Tool.Rectangle || tool == Tool.Ellipse || tool == Tool.Select)
            {
                float d = Math.Max(Math.Abs(dx), Math.Abs(dy));
                return new PointF(start.X + Math.Sign(dx) * d, start.Y + Math.Sign(dy) * d);
            }

            double angle = Math.Atan2(dy, dx);
            double snapped = Math.Round(angle / (Math.PI / 4)) * (Math.PI / 4);
            double len = Math.Sqrt(dx * dx + dy * dy);
            return new PointF(start.X + (float)(Math.Cos(snapped) * len), start.Y + (float)(Math.Sin(snapped) * len));
        }

        #endregion

        #region Drawing

        private void DrawStroke(PointF a, PointF b)
        {
            Color c = _tool == Tool.Eraser ? _paper : ButtonColor(_drawButton);
            using (Graphics g = Graphics.FromImage(_image))
            using (var pen = new Pen(c, _penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                g.SmoothingMode = _smooth && _tool != Tool.Eraser ? SmoothingMode.AntiAlias : SmoothingMode.None;
                if (Distance(a, b) < 0.01f)
                {
                    if (_penWidth <= 1.5f)
                    {
                        using (var brush = new SolidBrush(c))
                        {
                            g.FillRectangle(brush, (int)a.X, (int)a.Y, 1, 1);
                        }
                    }
                    else
                    {
                        using (var brush = new SolidBrush(c))
                        {
                            g.FillEllipse(brush, a.X - _penWidth / 2f, a.Y - _penWidth / 2f, _penWidth, _penWidth);
                        }
                    }
                }
                else
                {
                    g.DrawLine(pen, a, b);
                }
            }

            var rect = RectangleF.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            _canvas.InvalidateImageRect(rect, _penWidth + 2);
        }

        private static void DrawShape(Graphics g, Tool tool, PointF a, PointF b, Color color, float width, bool filled)
        {
            using (var pen = new Pen(color, width) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
            using (var brush = new SolidBrush(color))
            {
                RectangleF r = RectangleF.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                switch (tool)
                {
                    case Tool.Line:
                        g.DrawLine(pen, a, b);
                        break;
                    case Tool.Arrow:
                        using (var cap = new AdjustableArrowCap(Math.Max(3f, 4f), Math.Max(3f, 5f), true))
                        {
                            pen.CustomEndCap = cap;
                            g.DrawLine(pen, a, b);
                        }

                        break;
                    case Tool.Rectangle:
                        if (filled)
                        {
                            g.FillRectangle(brush, r);
                        }
                        else
                        {
                            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                        }

                        break;
                    case Tool.Ellipse:
                        if (filled)
                        {
                            g.FillEllipse(brush, r);
                        }
                        else
                        {
                            g.DrawEllipse(pen, r);
                        }

                        break;
                }
            }
        }

        private void PlaceSymbol(PointF p, MouseButtons button)
        {
            PushUndo();
            using (Graphics g = Graphics.FromImage(_image))
            using (var painter = new MapSymbolPainter(ButtonColor(button), _paper, Math.Max(1f, _symbolSize / 16f)))
            {
                g.SmoothingMode = _smooth ? SmoothingMode.AntiAlias : SmoothingMode.None;
                painter.Draw(g, _symbol, p.X, p.Y, _symbolSize, _random.Next(256));
            }

            _canvas.InvalidateImageRect(MapSymbolPainter.GetBounds(_symbol, p.X, p.Y, _symbolSize), 4);
        }

        private void OnPaintOverlay(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            float px = 1f / _canvas.Zoom; // one screen pixel in image units

            if (_drawing && _tool != Tool.Pen && _tool != Tool.Eraser && _tool != Tool.Select)
            {
                g.SmoothingMode = _smooth ? SmoothingMode.AntiAlias : SmoothingMode.None;
                DrawShape(g, _tool, _start, _current, ButtonColor(_drawButton), _penWidth, _filled);
            }

            Rectangle? sel = _drawing && _tool == Tool.Select ? Normalize(_start, _current) : _selection;
            if (sel.HasValue)
            {
                Rectangle r = sel.Value;
                using (var white = new Pen(Color.White, px))
                using (var dash = new Pen(Color.Black, px) { DashStyle = DashStyle.Dash })
                {
                    g.DrawRectangle(white, r.X, r.Y, r.Width, r.Height);
                    g.DrawRectangle(dash, r.X, r.Y, r.Width, r.Height);
                }
            }

            if (!_hoverInside || _drawing && _tool != Tool.Pen && _tool != Tool.Eraser)
            {
                return;
            }

            if (_floating != null)
            {
                g.DrawImage(_floating, _hover.X - _floating.Width / 2f, _hover.Y - _floating.Height / 2f, _floating.Width, _floating.Height);
                using (var dash = new Pen(Color.Gray, px) { DashStyle = DashStyle.Dot })
                {
                    g.DrawRectangle(dash, _hover.X - _floating.Width / 2f, _hover.Y - _floating.Height / 2f, _floating.Width, _floating.Height);
                }

                return;
            }

            switch (_tool)
            {
                case Tool.Symbol:
                    g.SmoothingMode = _smooth ? SmoothingMode.AntiAlias : SmoothingMode.None;
                    using (var painter = new MapSymbolPainter(Color.FromArgb(160, _color), Color.FromArgb(160, _paper), Math.Max(1f, _symbolSize / 16f)))
                    {
                        painter.Draw(g, _symbol, _hover.X, _hover.Y, _symbolSize, 1);
                    }

                    break;
                case Tool.Pen:
                case Tool.Eraser:
                    if (_penWidth * _canvas.Zoom >= 4)
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        using (var pen = new Pen(Color.Gray, px))
                        {
                            g.DrawEllipse(pen, _hover.X - _penWidth / 2f, _hover.Y - _penWidth / 2f, _penWidth, _penWidth);
                        }
                    }

                    break;
            }
        }

        #endregion

        #region Text / floating

        private void StartText(PointF p)
        {
            using (var dialog = new MapTextDialog(_color, _paper))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = dialog.CreateBitmap();
                if (bmp == null)
                {
                    return;
                }

                StartFloating(bmp);
                _hover = p;
                _hoverInside = true;
                _canvas.Invalidate();
            }
        }

        private void StartFloating(Bitmap bmp)
        {
            _floating?.Dispose();
            _floating = bmp;
            _toolBeforeFloating = _tool;
            _statusHint.Text = "Click to place it (right click / Esc = cancel).";
        }

        private void CommitFloating()
        {
            if (_floating == null)
            {
                return;
            }

            PushUndo();
            using (Graphics g = Graphics.FromImage(_image))
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(_floating, (int)Math.Round(_hover.X - _floating.Width / 2f), (int)Math.Round(_hover.Y - _floating.Height / 2f),
                    _floating.Width, _floating.Height);
            }

            DropFloating();
        }

        private void DropFloating()
        {
            if (_floating == null)
            {
                return;
            }

            _floating.Dispose();
            _floating = null;
            _statusHint.Text = _toolHints[(int)_toolBeforeFloating];
            _canvas.Invalidate();
        }

        #endregion

        #region Edit commands

        private Bitmap SelectionOrImage()
        {
            if (_selection.HasValue)
            {
                return _image.Clone(_selection.Value, PixelFormat.Format32bppArgb);
            }

            return new Bitmap(_image);
        }

        private void CopyToClipboard()
        {
            if (IsTextInputFocused())
            {
                return;
            }

            using (Bitmap bmp = SelectionOrImage())
            {
                Clipboard.SetImage(bmp);
            }

            _statusHint.Text = _selection.HasValue ? "Selection copied." : "Image copied.";
        }

        private void CutSelection()
        {
            if (!_selection.HasValue || IsTextInputFocused())
            {
                return;
            }

            CopyToClipboard();
            DeleteSelection();
        }

        private void PasteFloating()
        {
            if (IsTextInputFocused())
            {
                return;
            }

            if (!Clipboard.ContainsImage())
            {
                _statusHint.Text = "The clipboard has no image.";
                return;
            }

            using (Image img = Clipboard.GetImage())
            {
                if (img == null)
                {
                    return;
                }

                var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
                }

                StartFloating(bmp);
                Point client = _canvas.PointToClient(Cursor.Position);
                _hover = ImagePoint(client);
                _hoverInside = _canvas.ClientRectangle.Contains(client);
                _canvas.Invalidate();
            }
        }

        private void PasteAsNewImage()
        {
            if (!Clipboard.ContainsImage())
            {
                _statusHint.Text = "The clipboard has no image.";
                return;
            }

            using (Image img = Clipboard.GetImage())
            {
                if (img == null)
                {
                    return;
                }

                PushUndo();
                ReplaceImage(ToArgb(img));
                _selection = null;
                _canvas.ZoomToFit();
            }
        }

        private void SelectAll()
        {
            if (IsTextInputFocused())
            {
                return;
            }

            _selection = new Rectangle(0, 0, _image.Width, _image.Height);
            SetTool(Tool.Select);
        }

        private void DeleteSelection()
        {
            if (!_selection.HasValue || IsTextInputFocused())
            {
                return;
            }

            PushUndo();
            using (Graphics g = Graphics.FromImage(_image))
            using (var brush = new SolidBrush(_paper))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(brush, _selection.Value);
            }

            _canvas.Invalidate();
        }

        private void CropToSelection()
        {
            if (!_selection.HasValue)
            {
                _statusHint.Text = "Select an area first (tool \"Select\").";
                return;
            }

            PushUndo();
            Bitmap cropped = _image.Clone(_selection.Value, PixelFormat.Format32bppArgb);
            _selection = null;
            ReplaceImage(cropped);
            _canvas.ZoomToFit();
        }

        private void RotateFlip(RotateFlipType type)
        {
            PushUndo();
            _image.RotateFlip(type);
            _selection = null;
            SetImage(_image);
        }

        private void ResizeImage()
        {
            using (var form = new Form())
            {
                form.Text = "Resize";
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.ShowInTaskbar = false;
                form.StartPosition = FormStartPosition.CenterParent;
                form.ClientSize = new Size(280, 140);
                var w = new NumericUpDown { Left = 90, Top = 12, Width = 90, Minimum = 1, Maximum = 16000, Value = _image.Width };
                var h = new NumericUpDown { Left = 90, Top = 42, Width = 90, Minimum = 1, Maximum = 16000, Value = _image.Height };
                var keep = new CheckBox { Text = "Keep aspect ratio", Left = 12, Top = 72, Width = 200, Checked = true };
                bool busy = false;
                w.ValueChanged += (s, e) =>
                {
                    if (keep.Checked && !busy)
                    {
                        busy = true;
                        h.Value = Math.Max(1, Math.Min(16000, Math.Round(w.Value * _image.Height / _image.Width)));
                        busy = false;
                    }
                };
                h.ValueChanged += (s, e) =>
                {
                    if (keep.Checked && !busy)
                    {
                        busy = true;
                        w.Value = Math.Max(1, Math.Min(16000, Math.Round(h.Value * _image.Width / _image.Height)));
                        busy = false;
                    }
                };
                var ok = new Button { Text = "OK", Left = 108, Top = 104, Width = 75, DialogResult = DialogResult.OK };
                var cancel = new Button { Text = "Cancel", Left = 192, Top = 104, Width = 75, DialogResult = DialogResult.Cancel };
                form.Controls.AddRange(new Control[]
                {
                    new Label { Text = "Width:", Left = 12, Top = 15, Width = 70 }, w,
                    new Label { Text = "Height:", Left = 12, Top = 45, Width = 70 }, h, keep, ok, cancel
                });
                form.AcceptButton = ok;
                form.CancelButton = cancel;
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                int nw = (int)w.Value, nh = (int)h.Value;
                if ((long)nw * nh > 64L * 1024 * 1024)
                {
                    MessageBox.Show(this, "The image would be too big.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                PushUndo();
                var bmp = new Bitmap(nw, nh, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.InterpolationMode = _smooth ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(_image, new Rectangle(0, 0, nw, nh));
                }

                _selection = null;
                ReplaceImage(bmp);
                _canvas.ZoomToFit();
            }
        }

        private enum PixelOp
        {
            Gray,
            BlackWhite,
            Invert,
            Sepia
        }

        private void ApplyPixels(PixelOp op)
        {
            PushUndo();
            Rectangle area = _selection ?? new Rectangle(0, 0, _image.Width, _image.Height);
            int[] px = ReadPixels(_image, area);
            for (int i = 0; i < px.Length; i++)
            {
                int c = px[i];
                int a = (c >> 24) & 0xFF, r = (c >> 16) & 0xFF, g = (c >> 8) & 0xFF, b = c & 0xFF;
                int l = (r * 299 + g * 587 + b * 114) / 1000;
                switch (op)
                {
                    case PixelOp.Gray:
                        r = g = b = l;
                        break;
                    case PixelOp.BlackWhite:
                        r = g = b = l < 128 ? 0 : 255;
                        break;
                    case PixelOp.Invert:
                        r = 255 - r;
                        g = 255 - g;
                        b = 255 - b;
                        break;
                    case PixelOp.Sepia:
                        int sr = (int)(r * 0.393 + g * 0.769 + b * 0.189);
                        int sg = (int)(r * 0.349 + g * 0.686 + b * 0.168);
                        int sb = (int)(r * 0.272 + g * 0.534 + b * 0.131);
                        r = Math.Min(255, sr);
                        g = Math.Min(255, sg);
                        b = Math.Min(255, sb);
                        break;
                }

                px[i] = (a << 24) | (r << 16) | (g << 8) | b;
            }

            WritePixels(_image, area, px);
            _canvas.Invalidate();
        }

        private static int[] ReadPixels(Bitmap bmp, Rectangle area)
        {
            BitmapData data = bmp.LockBits(area, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var px = new int[area.Width * area.Height];
                for (int y = 0; y < area.Height; y++)
                {
                    Marshal.Copy(data.Scan0 + y * data.Stride, px, y * area.Width, area.Width);
                }

                return px;
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        private static void WritePixels(Bitmap bmp, Rectangle area, int[] px)
        {
            BitmapData data = bmp.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < area.Height; y++)
                {
                    Marshal.Copy(px, y * area.Width, data.Scan0 + y * data.Stride, area.Width);
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>Scanline flood fill with color tolerance</summary>
        private void FloodFill(int sx, int sy, Color fill)
        {
            int w = _image.Width, h = _image.Height;
            var area = new Rectangle(0, 0, w, h);
            int[] px = ReadPixels(_image, area);
            int target = px[sy * w + sx];
            int newColor = fill.ToArgb();
            if (target == newColor)
            {
                return;
            }

            int tol = _tolerance;
            int tr = (target >> 16) & 0xFF, tg = (target >> 8) & 0xFF, tb = target & 0xFF, ta = (target >> 24) & 0xFF;

            bool Match(int c)
            {
                return Math.Abs(((c >> 16) & 0xFF) - tr) <= tol &&
                       Math.Abs(((c >> 8) & 0xFF) - tg) <= tol &&
                       Math.Abs((c & 0xFF) - tb) <= tol &&
                       Math.Abs(((c >> 24) & 0xFF) - ta) <= tol;
            }

            var done = new bool[w * h];
            var stack = new Stack<int>();
            stack.Push(sy * w + sx);
            while (stack.Count > 0)
            {
                int idx = stack.Pop();
                int y = idx / w;
                int x = idx % w;
                while (x >= 0 && !done[y * w + x] && Match(px[y * w + x]))
                {
                    x--;
                }

                x++;
                bool up = false, down = false;
                while (x < w && !done[y * w + x] && Match(px[y * w + x]))
                {
                    int i = y * w + x;
                    px[i] = newColor;
                    done[i] = true;
                    if (y > 0)
                    {
                        bool m = !done[i - w] && Match(px[i - w]);
                        if (m && !up)
                        {
                            stack.Push(i - w);
                        }

                        up = m;
                    }

                    if (y < h - 1)
                    {
                        bool m = !done[i + w] && Match(px[i + w]);
                        if (m && !down)
                        {
                            stack.Push(i + w);
                        }

                        down = m;
                    }

                    x++;
                }
            }

            WritePixels(_image, area, px);
        }

        #endregion

        #region Save

        private bool Save()
        {
            DropFloating();
            string target = _entry?.FilePath ?? _externalFile;
            string ext = Path.GetExtension(target ?? string.Empty).ToLowerInvariant();
            if (target == null || ext == ".emf" || ext == ".wmf" || ext == ".exif" || ext.Length == 0)
            {
                return SaveAs();
            }

            try
            {
                if (_entry != null)
                {
                    MapImageLibrary.Update(_entry, _image);
                }
                else
                {
                    MapImageLibrary.SaveImage(_image, target);
                }

                _modified = false;
                UpdateTitle();
                _statusHint.Text = $"Saved: {target}";
                ImageSaved?.Invoke(_entry, target);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool SaveAs()
        {
            DropFloating();
            MapImageEntry entry = MapImageSaveDialog.SaveWithDialog(this, _image, _name, out string file);
            if (file == null)
            {
                return false;
            }

            _entry = entry;
            _externalFile = entry == null ? file : null;
            if (entry != null)
            {
                _name = entry.Name;
            }

            _modified = false;
            UpdateTitle();
            _statusHint.Text = $"Saved: {file}";
            ImageSaved?.Invoke(entry, file);
            return true;
        }

        private void SaveCopyToList()
        {
            DropFloating();
            try
            {
                MapImageEntry entry = MapImageLibrary.AddInternal(_image, _name);
                _statusHint.Text = $"Copy added to the list: {entry.Name}";
                ImageSaved?.Invoke(entry, entry.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Saving failed:\n{ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Keys and closing

        private bool IsTextInputFocused()
        {
            Control c = ActiveControl;
            while (c is ContainerControl cc && cc.ActiveControl != null)
            {
                c = cc.ActiveControl;
            }

            if (c is TextBoxBase)
            {
                return true;
            }

            // combo boxes of the tool strip
            return _widthCombo.Focused || _symbolSizeCombo.Focused;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_floating != null)
                {
                    DropFloating();
                }
                else if (_drawing)
                {
                    _drawing = false;
                    _canvas.Invalidate();
                }
                else if (_selection.HasValue)
                {
                    _selection = null;
                    _canvas.Invalidate();
                }

                e.Handled = true;
                return;
            }

            if (!e.Control && !e.Alt && !IsTextInputFocused())
            {
                Tool? tool = null;
                switch (e.KeyCode)
                {
                    case Keys.H: tool = Tool.Hand; break;
                    case Keys.M: tool = Tool.Select; break;
                    case Keys.P: tool = Tool.Pen; break;
                    case Keys.E: tool = Tool.Eraser; break;
                    case Keys.L: tool = Tool.Line; break;
                    case Keys.A: tool = Tool.Arrow; break;
                    case Keys.R: tool = Tool.Rectangle; break;
                    case Keys.O: tool = Tool.Ellipse; break;
                    case Keys.T: tool = Tool.Text; break;
                    case Keys.S: tool = Tool.Symbol; break;
                    case Keys.F: tool = Tool.Fill; break;
                    case Keys.I: tool = Tool.Picker; break;
                    case Keys.Space:
                        if (!_spacePan)
                        {
                            _spacePan = true;
                            _canvas.PanWithLeftButton = true;
                            _canvas.Cursor = Cursors.Hand;
                        }

                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        return;
                }

                if (tool.HasValue)
                {
                    SetTool(tool.Value);
                    e.Handled = true;
                    return;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && _spacePan)
            {
                _spacePan = false;
                SetTool(_tool);
                e.Handled = true;
                return;
            }

            base.OnKeyUp(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_modified && e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult r = MessageBox.Show(this, $"\"{_name}\" was changed. Save it?", "Map image editor",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (r == DialogResult.Cancel || (r == DialogResult.Yes && !Save()))
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _canvas.Image = null;
                _image?.Dispose();
                _image = null;
                _floating?.Dispose();
                foreach (Bitmap b in _undo)
                {
                    b.Dispose();
                }

                _undo.Clear();
                ClearRedo();
                _colorButton?.Image?.Dispose();
                _paperButton?.Image?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}

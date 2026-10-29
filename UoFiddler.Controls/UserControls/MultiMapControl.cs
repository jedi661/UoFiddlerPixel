/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.MultiMapImages;

namespace UoFiddler.Controls.UserControls
{
    public partial class MultiMapControl : UserControl
    {
        public MultiMapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            multiMapToolStripMenuItem.Tag = -1;
            facet00ToolStripMenuItem.Tag = 0;
            facet01ToolStripMenuItem.Tag = 1;
            facet02ToolStripMenuItem.Tag = 2;
            facet03ToolStripMenuItem.Tag = 3;
            facet04ToolStripMenuItem.Tag = 4;
            facet05ToolStripMenuItem.Tag = 5;
        }

        private bool _loaded;

        /// <summary>Name of the shown image (for export, print, save)</summary>
        private string _currentName = "MultiMap";

        /// <summary>Entry of the "Load.." list when an own image is shown</summary>
        private MapImageEntry _currentEntry;

        /// <summary>Built-in item (MultiMap / Facet) when a client image is shown</summary>
        private ToolStripMenuItem _currentBuiltIn;

        private readonly List<ToolStripMenuItem> _libraryItems = new List<ToolStripMenuItem>();

        private IEnumerable<ToolStripMenuItem> BuiltInItems => new[]
        {
            multiMapToolStripMenuItem, facet00ToolStripMenuItem, facet01ToolStripMenuItem, facet02ToolStripMenuItem,
            facet03ToolStripMenuItem, facet04ToolStripMenuItem, facet05ToolStripMenuItem
        };

        #region Showing images

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            MapImageRenderer.ResetTables();

            if (_currentBuiltIn != null)
            {
                ShowBuiltIn(_currentBuiltIn);
            }
        }

        /// <summary>Shows a new image. The old one is disposed.</summary>
        private void SetImage(Image image, string name, MapImageEntry entry, ToolStripMenuItem builtIn)
        {
            Image old = pictureBox.Image;
            pictureBox.Image = image;
            if (old != null && !ReferenceEquals(old, image))
            {
                old.Dispose();
            }

            _currentName = string.IsNullOrWhiteSpace(name) ? "Image" : name;
            _currentEntry = entry;
            _currentBuiltIn = builtIn;

            foreach (ToolStripMenuItem item in BuiltInItems)
            {
                item.Checked = ReferenceEquals(item, builtIn);
            }

            foreach (ToolStripMenuItem item in _libraryItems)
            {
                item.Checked = entry != null && ReferenceEquals(item.Tag, entry);
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Image img = pictureBox.Image;
            string where = _currentEntry != null
                ? (_currentEntry.IsInternal ? "  (image list)" : $"  ({_currentEntry.FilePath})")
                : string.Empty;
            nameStatusLabel.Text = img == null ? $"{_currentName}: no image" : _currentName + where;
            sizeStatusLabel.Text = img == null ? string.Empty : $"{img.Width} x {img.Height}";
            bool has = img != null;
            editToolStripButton.Enabled = copyToolStripButton.Enabled = printToolStripDropDownButton.Enabled = has;
            toolStripDropDownButton3.Enabled = has;
            multiMapFromCurrentToolStripMenuItem.Enabled = facetFromCurrentToolStripMenuItem.Enabled = has;
            copyContextMenuItem.Enabled = editContextMenuItem.Enabled = saveAsContextMenuItem.Enabled = printContextMenuItem.Enabled = has;
        }

        private void ShowImage(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem strip)
            {
                ShowBuiltIn(strip);
            }
        }

        private void ShowBuiltIn(ToolStripMenuItem strip)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                int id = (int)strip.Tag;
                Image image = id == -1
                    ? Ultima.MultiMap.GetMultiMap()
                    : Ultima.MultiMap.GetFacetImage(id);
                SetImage(image, id == -1 ? "MultiMap" : $"Facet0{id}", null, strip);
            }
            catch (Exception ex)
            {
                SetImage(null, strip.Text, null, strip);
                MessageBox.Show($"The image could not be loaded:\n{ex.Message}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowEntry(MapImageEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (!entry.Exists)
            {
                MessageBox.Show($"The file of \"{entry.Name}\" was not found:\n{entry.FilePath}", "Load", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                SetImage(MapImageLibrary.LoadBitmap(entry.FilePath), entry.Name, entry, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The image could not be loaded:\n{ex.Message}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void ShowFile(string fileName)
        {
            MapImageEntry entry = MapImageLibrary.FindByPath(fileName);
            if (entry != null)
            {
                ShowEntry(entry);
                return;
            }

            try
            {
                SetImage(MapImageLibrary.LoadBitmap(fileName), Path.GetFileNameWithoutExtension(fileName), null, null);
                nameStatusLabel.Text += $"  ({fileName} - not in the list)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The image could not be loaded:\n{ex.Message}", "Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Puts the own images between the two separators of "Load.."</summary>
        private void RebuildLibraryMenu()
        {
            ToolStripItemCollection items = toolStripDropDownButton1.DropDownItems;
            foreach (ToolStripMenuItem item in _libraryItems)
            {
                items.Remove(item);
                item.Dispose();
            }

            _libraryItems.Clear();

            int index = items.IndexOf(toolStripSeparatorLibraryStart) + 1;
            foreach (MapImageEntry entry in MapImageLibrary.Entries)
            {
                var item = new ToolStripMenuItem(entry.Exists ? entry.Name : $"{entry.Name} (file missing)")
                {
                    Tag = entry,
                    ToolTipText = entry.FilePath,
                    Enabled = entry.Exists,
                    Checked = ReferenceEquals(entry, _currentEntry)
                };
                item.Click += (s, e) => ShowEntry((MapImageEntry)((ToolStripMenuItem)s).Tag);
                items.Insert(index++, item);
                _libraryItems.Add(item);
            }

            toolStripSeparatorLibraryEnd.Visible = _libraryItems.Count > 0;

            // the shown entry may have been removed
            if (_currentEntry != null && !_libraryItems.Exists(i => ReferenceEquals(i.Tag, _currentEntry)))
            {
                _currentEntry = null;
                UpdateStatus();
            }
        }

        private void OnLibraryChanged(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(RebuildLibraryMenu));
                return;
            }

            RebuildLibraryMenu();
        }

        #endregion

        #region Load menu

        private void OnClickOpenImageFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Title = "Open image", Filter = MapImageLibrary.OpenFilter })
            {
                if (dialog.ShowDialog(FindForm()) == DialogResult.OK)
                {
                    ShowFile(dialog.FileName);
                }
            }
        }

        private void OnClickAddImageFile(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Title = "Add images to the list", Filter = MapImageLibrary.OpenFilter, Multiselect = true })
            {
                if (dialog.ShowDialog(FindForm()) != DialogResult.OK)
                {
                    return;
                }

                DialogResult copy = MessageBox.Show(
                    "Copy the file(s) into the app?\n\nYes = copy into the image folder of the app\nNo = only link the file(s) in their folder",
                    "Add to list", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (copy == DialogResult.Cancel)
                {
                    return;
                }

                MapImageEntry last = null;
                foreach (string file in dialog.FileNames)
                {
                    try
                    {
                        last = MapImageLibrary.AddFile(file, null, copy == DialogResult.Yes);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{file}\ncould not be added:\n{ex.Message}", "Add to list", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                ShowEntry(last);
            }
        }

        private void OnClickFromClipboard(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("The clipboard has no image.", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    g.Clear(Color.White);
                    g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));
                }

                SetImage(bmp, "Clipboard", null, null);
            }
        }

        private void OnClickManageList(object sender, EventArgs e)
        {
            using (var form = new MapImageListForm())
            {
                form.ShowDialog(FindForm());
                if (form.EntryToShow != null)
                {
                    ShowEntry(form.EntryToShow);
                }
            }
        }

        #endregion

        #region Create, edit, copy, print

        private void OnClickCreateImage(object sender, EventArgs e)
        {
            using (var form = new MapImageGeneratorForm(pictureBox.Image, _currentName))
            {
                form.ShowDialog(FindForm());

                if (form.ImageToEdit != null)
                {
                    using (Bitmap bmp = form.ImageToEdit)
                    {
                        OpenEditor(bmp, form.ResultName, form.AddedEntry);
                    }
                }

                if (form.AddedEntry != null)
                {
                    ShowEntry(form.AddedEntry);
                }
            }
        }

        private void OnClickEdit(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                OpenEditor(pictureBox.Image, _currentName, _currentEntry);
            }
        }

        private void OpenEditor(Image image, string name, MapImageEntry entry)
        {
            var editor = new MapImageEditorForm(image, name, entry);
            editor.ImageSaved += (savedEntry, file) =>
            {
                if (IsDisposed)
                {
                    return;
                }

                if (savedEntry != null)
                {
                    ShowEntry(savedEntry);
                }
                else if (file != null && File.Exists(file))
                {
                    ShowFile(file);
                }
            };
            editor.FormClosed += (s, e) => editor.Dispose();
            editor.Show(FindForm());
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                return;
            }

            using (var bmp = new Bitmap(pictureBox.Image))
            {
                Clipboard.SetImage(bmp);
            }

            nameStatusLabel.Text = $"{_currentName}: copied to the clipboard";
        }

        private void OnClickPrint(object sender, EventArgs e)
        {
            MapImagePrinter.Print(FindForm(), pictureBox.Image, _currentName);
        }

        private void OnClickPrintPreview(object sender, EventArgs e)
        {
            MapImagePrinter.Preview(FindForm(), pictureBox.Image, _currentName);
        }

        #endregion

        #region Zoom and mouse

        private void OnZoomChanged(object sender, EventArgs e)
        {
            zoomToolStripLabel.Text = $"{pictureBox.Zoom * 100:0} %";
        }

        private void OnClickZoomIn(object sender, EventArgs e) => pictureBox.ZoomIn();

        private void OnClickZoomOut(object sender, EventArgs e) => pictureBox.ZoomOut();

        private void OnClickActualSize(object sender, EventArgs e) => pictureBox.ZoomActualSize();

        private void OnClickFit(object sender, EventArgs e) => pictureBox.ZoomToFit();

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Image img = pictureBox.Image;
            if (img == null)
            {
                positionStatusLabel.Text = string.Empty;
                return;
            }

            PointF p = pictureBox.ClientToImage(e.Location);
            positionStatusLabel.Text = p.X >= 0 && p.Y >= 0 && p.X < img.Width && p.Y < img.Height
                ? $"{(int)p.X}, {(int)p.Y}"
                : string.Empty;
        }

        #endregion

        #region Export / save

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Png);
        }

        private void OnClickExportGif(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Gif);
        }

        private void ExportMultiMapImage(ImageFormat imageFormat)
        {
            if (pictureBox.Image == null)
            {
                return;
            }

            string fileExtension = MapImageLibrary.ExtensionFor(imageFormat);
            string name = MapImageLibrary.SafeFileName(_currentName);
            string fileName = Path.Combine(Options.OutputPath, $"{name}.{fileExtension}");

            try
            {
                MapImageLibrary.SaveImage(pictureBox.Image, fileName, imageFormat);
                MessageBox.Show($"{_currentName} saved to {fileName}", "Export", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClickSaveAs(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                return;
            }

            MapImageEntry entry = MapImageSaveDialog.SaveWithDialog(FindForm(), pictureBox.Image, _currentName, out string file);
            if (entry != null)
            {
                ShowEntry(entry);
            }
            else if (file != null)
            {
                nameStatusLabel.Text = $"{_currentName}: saved to {file}";
            }
        }

        private void OnClickSaveToList(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                return;
            }

            try
            {
                ShowEntry(MapImageLibrary.AddInternal(pictureBox.Image, _currentName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Saving failed:\n{ex.Message}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Generate

        private void OnClickGenerateRLE(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                dialog.Filter = MapImageLibrary.OpenFilter;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    using (Bitmap image = new Bitmap(dialog.FileName))
                    {
                        if (image.Height != 2048 || image.Width != 2560)
                        {
                            MessageBox.Show("Invalid image height or width", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }

                        string fileName = WriteMultiMap(image);

                        MessageBox.Show($"MultiMap saved to {fileName}", "Convert",
                            MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Converting failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private static string WriteMultiMap(Bitmap image)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "MultiMap.rle");
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (BinaryWriter bin = new BinaryWriter(fs, Encoding.Unicode))
            {
                Ultima.MultiMap.SaveMultiMap(image, bin);
            }

            return fileName;
        }

        private void OnClickGenerateFacetFromImage(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                dialog.Filter = MapImageLibrary.OpenFilter;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    using (Bitmap image = new Bitmap(dialog.FileName))
                    {
                        string path = Options.OutputPath;
                        string fileName = Path.Combine(path, "facet.mul");
                        Ultima.MultiMap.SaveFacetImage(fileName, image);

                        MessageBox.Show($"Facet saved to {fileName}", "Convert", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Converting failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>Converts the shown image to MultiMap.rle (2560 x 2048, black/white)</summary>
        private void OnClickMultiMapFromCurrent(object sender, EventArgs e)
        {
            Image img = pictureBox.Image;
            if (img == null)
            {
                return;
            }

            if ((img.Width != 2560 || img.Height != 2048) &&
                MessageBox.Show($"The image is {img.Width} x {img.Height}. The MultiMap needs 2560 x 2048 - the image is stretched.\nContinue?",
                    "MultiMap", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                using (Bitmap bw = ToMultiMapBitmap(img))
                {
                    string fileName = WriteMultiMap(bw);
                    MessageBox.Show($"MultiMap saved to {fileName}\n(dark pixels = black, light pixels = white)", "MultiMap",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Converting failed:\n{ex.Message}", "MultiMap", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnClickFacetFromCurrent(object sender, EventArgs e)
        {
            Image img = pictureBox.Image;
            if (img == null)
            {
                return;
            }

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                using (var bmp = new Bitmap(img))
                {
                    string fileName = Path.Combine(Options.OutputPath, "facet.mul");
                    Ultima.MultiMap.SaveFacetImage(fileName, bmp);
                    MessageBox.Show($"Facet saved to {fileName}", "Facet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Converting failed:\n{ex.Message}", "Facet", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        /// <summary>2560 x 2048, only pure white (0xFFFF) and black (0x8000) as the RLE writer expects</summary>
        private static Bitmap ToMultiMapBitmap(Image img)
        {
            const int w = 2560, h = 2048;
            int[] px = new int[w * h];
            using (var scaled = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(scaled))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(img, new Rectangle(0, 0, w, h));
                }

                BitmapData src = scaled.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(src.Scan0 + y * src.Stride, px, y * w, w);
                }

                scaled.UnlockBits(src);
            }

            var result = new Bitmap(w, h, PixelFormat.Format16bppArgb1555);
            BitmapData dst = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            var line = new short[w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int c = px[y * w + x];
                    int l = (((c >> 16) & 0xFF) * 299 + ((c >> 8) & 0xFF) * 587 + (c & 0xFF) * 114) / 1000;
                    line[x] = l < 128 ? unchecked((short)0x8000) : unchecked((short)0xFFFF);
                }

                Marshal.Copy(line, 0, dst.Scan0 + y * dst.Stride, w);
            }

            result.UnlockBits(dst);
            return result;
        }

        #endregion

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_loaded)
            {
                return;
            }

            try
            {
                RebuildLibraryMenu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MultiMapControl: {ex.Message}");
            }

            ShowBuiltIn(multiMapToolStripMenuItem);
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            MapImageLibrary.Changed += OnLibraryChanged;
            _loaded = true;
        }

        /// <summary>Called from Dispose (designer file)</summary>
        private void OnDisposeImages()
        {
            if (_loaded)
            {
                ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
                MapImageLibrary.Changed -= OnLibraryChanged;
            }

            Image img = pictureBox?.Image;
            if (pictureBox != null)
            {
                pictureBox.Image = null;
            }

            img?.Dispose();
        }
    }
}

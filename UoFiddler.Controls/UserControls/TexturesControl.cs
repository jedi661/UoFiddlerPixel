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
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using static UoFiddler.Controls.UserControls.LandTilesControl;

namespace UoFiddler.Controls.UserControls
{
    public partial class TexturesControl : UserControl
    {
        private bool playCustomSound = false;

        private Textures _textures; // TextureWindowForm

        public TexturesControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _textures = new Textures(); // TextureWindowForm
        }

        private List<int> _textureList = new List<int>();
        private bool _showFreeSlots;
        private bool _loaded;
        private int _selectedTextureId = -1;

        #region SelectedTextureId
        public int SelectedTextureId
        {
            get => _selectedTextureId;
            set
            {
                _selectedTextureId = value < 0 ? 0 : value;
                UpdateLabels(_selectedTextureId);
                TextureTileView.FocusIndex = _textureList.IndexOf(_selectedTextureId);
            }
        }
        #endregion

        #region Reload
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _textureList = new List<int>();

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            _selectedTextureId = -1;

            OnLoad(this, EventArgs.Empty);
        }
        #endregion

        #region SearchGraphic
        private bool SearchGraphic(int graphic)
        {
            if (_textureList.All(id => id != graphic))
            {
                return false;
            }

            // we have to invalidate focus so it will scroll to item
            TextureTileView.FocusIndex = -1;
            SelectedTextureId = graphic;

            return true;
        }
        #endregion

        #region OnLoad
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            for (int i = 0; i < Textures.GetIdxLength(); ++i)
            {
                if (Textures.TestTexture(i))
                {
                    _textureList.Add(i);
                }
            }

            TextureTileView.VirtualListSize = _textureList.Count;

            UpdateTileView();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.TextureChangeEvent += OnTextureChangeEvent;
            }

            _loaded = true;

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region OnTextureChangeEvent
        private void OnTextureChangeEvent(object sender, int index)
        {
            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Textures.TestTexture(index))
            {
                bool done = false;

                for (int i = 0; i < _textureList.Count; ++i)
                {
                    if (index < _textureList[i])
                    {
                        _textureList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _textureList[i])
                    {
                        continue;
                    }

                    done = true;

                    break;
                }

                if (!done)
                {
                    _textureList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _textureList.Remove(index);
            }

            TextureTileView.VirtualListSize = _textureList.Count;
            TextureTileView.Invalidate();
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        private TextureSearchForm _showForm;

        #region OnClickSearch
        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new TextureSearchForm(SearchGraphic)
            {
                TopMost = true
            };
            _showForm.Show();
        }
        #endregion

        #region OnClickFindNext
        private void OnClickFindNext(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedTextureId > -1 ? _textureList.IndexOf(_selectedTextureId) + 1 : 0;
                for (; i < _textureList.Count; ++i)
                {
                    if (Textures.TestTexture(_textureList[i]))
                    {
                        continue;
                    }

                    SelectedTextureId = _textureList[i];
                    TextureTileView.Invalidate();
                    break;
                }
            }
            else
            {
                int id, i;
                if (_selectedTextureId > -1)
                {
                    id = _selectedTextureId + 1;
                    i = _textureList.IndexOf(_selectedTextureId) + 1;
                }
                else
                {
                    id = 1;
                    i = 0;
                }

                for (; i < _textureList.Count; ++i, ++id)
                {
                    if (id >= _textureList[i])
                    {
                        continue;
                    }

                    SelectedTextureId = _textureList[i];
                    TextureTileView.Invalidate();

                    break;
                }
            }
        }
        #endregion

        #region OnClickRemove
        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            if (!Textures.TestTexture(_selectedTextureId))
            {
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure to remove 0x{_selectedTextureId:X}", "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Textures.Remove(_selectedTextureId);
            ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);

            if (!_showFreeSlots)
            {
                _textureList.Remove(_selectedTextureId);
                TextureTileView.VirtualListSize = _textureList.Count;
                var moveToIndex = --_selectedTextureId;
                SelectedTextureId = moveToIndex <= 0 ? 0 : _selectedTextureId; // TODO: get last index visible instead just curr -1
            }

            TextureTileView.Invalidate();

            Options.ChangedUltimaClass["Texture"] = true;
        }
        #endregion

        #region OnClickReplace
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // Check if the sound should be played
                if (playCustomSound)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
                else
                {
                    // Play the Windows 'Asterisk' sound
                    System.Media.SystemSounds.Asterisk.Play();

                    // Show the MessageBox
                    MessageBox.Show("Replaced texture Successfully.", "Replaced information.");
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Textures.Replace(_selectedTextureId, bitmap);

                    ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);

                    TextureTileView.Invalidate();

                    Options.ChangedUltimaClass["Texture"] = true;
                }
            }
        }
        #endregion

        #region OnTextChangedInsert
        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Textures.TestTexture(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }
        #endregion

        #region OnKeyDownInsert
        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                return;
            }

            if (Textures.TestTexture(index))
            {
                return;
            }

            contextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (Bitmap bmpTemp = new Bitmap(dialog.FileName))
                {
                    if ((bmpTemp.Width == 64 && bmpTemp.Height == 64) || (bmpTemp.Width == 128 && bmpTemp.Height == 128))
                    {
                        Bitmap bitmap = new Bitmap(bmpTemp);

                        if (dialog.FileName.Contains(".bmp"))
                        {
                            bitmap = Utils.ConvertBmp(bitmap);
                        }

                        Textures.Replace(index, bitmap);

                        ControlEvents.FireTextureChangeEvent(this, index);

                        bool done = false;
                        for (int i = 0; i < _textureList.Count; ++i)
                        {
                            if (index >= _textureList[i])
                            {
                                continue;
                            }

                            _textureList.Insert(i, index);

                            done = true;
                            break;
                        }

                        if (!done)
                        {
                            _textureList.Add(index);
                        }

                        TextureTileView.VirtualListSize = _textureList.Count;
                        TextureTileView.Invalidate();
                        SelectedTextureId = index;

                        Options.ChangedUltimaClass["Texture"] = true;
                    }
                    else
                    {
                        MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }
        #endregion

        #region OnClickSave
        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Textures.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Texture"] = false;
        }
        #endregion

        #region OnClickExportBmp
        private void OnClickExportBmp(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Bmp);
        }
        #endregion

        #region OnClickExportTiff
        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Tiff);
        }
        #endregion

        #region OnClickExportJpg
        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Jpeg);
        }
        #endregion

        #region OnClickExportPng
        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Png);
        }
        #endregion

        #region ExportTextureImage
        private void ExportTextureImage(int index, ImageFormat imageFormat)
        {
            if (!Textures.TestTexture(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Texture 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            // Check if the sound should be played
            if (playCustomSound)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = "sound.wav";
                player.Play();
            }
            else
            {
                MessageBox.Show($"Texture saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region TextureTileView_ItemSelectionChanged
        private void TextureTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (_textureList.Count == 0)
            {
                return;
            }

            SelectedTextureId = e.ItemIndex < 0 || e.ItemIndex > _textureList.Count
                ? _textureList[0]
                : _textureList[e.ItemIndex];
        }
        #endregion

        #region Graphic Label
        private void UpdateLabels(int graphic)
        {
            var width = Textures.TestTexture(graphic) ? Textures.GetTexture(graphic).Width : 0;

            GraphicLabel.Text = $"Graphic Values: Hex Address: 0x{graphic:X4} Decimal Address: ({graphic}) Texture Size: [{width}x{width}]";
        }
        #endregion

        #region TextureTileView_DrawItem
        private void TextureTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + TextureTileView.TilePadding.Left, e.Bounds.Y + TextureTileView.TilePadding.Top);

            const int defaultTileWidth = 128;
            Size defaultTileSize = new Size(defaultTileWidth, defaultTileWidth);
            Rectangle tileRectangle = new Rectangle(itemPoint, defaultTileSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(tileRectangle);

            Bitmap bitmap = Textures.GetTexture(_textureList[e.Index], out bool patched);

            if (bitmap == null)
            {
                e.Graphics.Clip = new Region(tileRectangle);

                tileRectangle.X += 5;
                tileRectangle.Y += 5;

                tileRectangle.Width -= 10;
                tileRectangle.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, tileRectangle);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched)
                {
                    // different background for verdata patched tiles
                    e.Graphics.FillRectangle(Brushes.LightCoral, tileRectangle);
                }

                // center 64x64 instead of drawing int top left corner
                if (bitmap.Width < defaultTileWidth)
                {
                    itemPoint.Offset(bitmap.Width / 2, bitmap.Height / 2);
                }

                Rectangle textureRectangle = new Rectangle(itemPoint, new Size(bitmap.Width, bitmap.Height));
                e.Graphics.DrawImage(bitmap, textureRectangle);

                e.Graphics.Clip = previousClip;
            }
        }
        #endregion

        #region ExportAllAsBmp
        private void ExportAllAsBmp_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Bmp);
        }
        #endregion

        #region ExportAllAsTiff
        private void ExportAllAsTiff_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Tiff);
        }
        #endregion

        #region ExportAllAsJpeg
        private void ExportAllAsJpeg_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Jpeg);
        }
        #endregion

        #region ExportAllAsPng
        private void ExportAllAsPng_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Png);
        }
        #endregion

        #region ExportAllTextures
        private void ExportAllTextures(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var index in _textureList)
                {
                    if (!Textures.TestTexture(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Texture 0x{index:X4}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                MessageBox.Show($"All textures saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
        #endregion

        #region ReplaceStartingFrom
        private void ReplaceStartingFrom_OnInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            const int graphicIdMin = 0;
            const int graphicIdMax = 0x3FFF;

            if (!Utils.ConvertStringToInt(ReplaceStartingFromTb.Text, out int index, graphicIdMin, graphicIdMax))
            {
                return;
            }

            contextMenuStrip.Close();

            // Check if the sound should be played
            if (playCustomSound)
            {
                SoundPlayer player = new SoundPlayer();
                player.SoundLocation = "sound.wav";
                player.Play();
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";


                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    var currentIdx = index + i;

                    if (IsIndexValid(currentIdx))
                    {
                        AddSingleTexture(dialog.FileNames[i], currentIdx);
                    }
                }

                TextureTileView.VirtualListSize = _textureList.Count;
                TextureTileView.Invalidate();
                SelectedTextureId = index;

                Options.ChangedUltimaClass["Texture"] = true;
            }
        }
        #endregion

        #region IsIndexValid
        /// <summary>
        /// Check if it's valid index for texture. Textures has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return index < 0x4000;
        }
        #endregion

        #region AddSingleTexture
        /// <summary>
        /// Adds a single texture.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the texture will be added.</param>
        private void AddSingleTexture(string fileName, int index)
        {
            using (Bitmap bmpTemp = new Bitmap(fileName))
            {
                if ((bmpTemp.Width == 64 && bmpTemp.Height == 64) || (bmpTemp.Width == 128 && bmpTemp.Height == 128))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (fileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Textures.Replace(index, bitmap);

                    ControlEvents.FireTextureChangeEvent(this, index);

                    bool done = false;

                    for (int i = 0; i < _textureList.Count; ++i)
                    {
                        if (index > _textureList[i])
                        {
                            continue;
                        }

                        _textureList[i] = index;

                        done = true;
                        break;
                    }

                    if (!done)
                    {
                        _textureList.Add(index);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Height or Width", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
        #endregion

        #region UpdateTileView
        public void UpdateTileView()
        {
            TextureTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameFocusColor = TextureTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = TextureTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            TextureTileView.TileFocusColor = Options.TileFocusColor;
            TextureTileView.TileHighlightColor = Options.TileSelectionColor;
            TextureTileView.Invalidate();
        }
        #endregion

        #region ShowFreeSlotsToolStripMenuItem
        private void ShowFreeSlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;

            if (_showFreeSlots)
            {
                for (int j = 0; j < Textures.GetIdxLength(); ++j)
                {
                    if (_textureList.Count > j)
                    {
                        if (_textureList[j] != j)
                        {
                            _textureList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _textureList.Insert(j, j);
                    }
                }

                var prevSelected = SelectedTextureId;

                TextureTileView.VirtualListSize = _textureList.Count;

                if (prevSelected >= 0)
                {
                    SelectedTextureId = prevSelected;
                }

                TextureTileView.Invalidate();
            }
            else
            {
                Reload();
            }
        }
        #endregion

        #region Copy clipboard
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if a texture is selected and if it is valid
            if (_selectedTextureId >= 0 && Textures.TestTexture(_selectedTextureId))
            {
                // Get the selected texture as a Bitmap
                using (Bitmap bmp = new Bitmap(Textures.GetTexture(_selectedTextureId)))
                {
                    // Copy the Bitmap to the clipboard
                    Clipboard.SetImage(bmp);

                    // Check if the sound should be played
                    if (playCustomSound)
                    {
                        SoundPlayer player = new SoundPlayer();
                        player.SoundLocation = "sound.wav";
                        player.Play();
                    }
                    else
                    {
                        // Show a MessageBox indicating that the image has been successfully copied to the clipboard
                        MessageBox.Show("The image has been copied to the clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                // If no texture is selected or if it is invalid, show a MessageBox indicating that there is no image to copy
                MessageBox.Show("No image to copy!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Import clipboard Image an Temp

        //Imports graphics from the clipboard
        private void importFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the clipboard contains an image
            if (Clipboard.ContainsImage())
            {
                // Retrieve the image from the clipboard
                using (Bitmap bmp = new Bitmap(Clipboard.GetImage()))
                {
                    // Check if the image has a valid size
                    if ((bmp.Width == 64 && bmp.Height == 64) || (bmp.Width == 128 && bmp.Height == 128) || (bmp.Width == 256 && bmp.Height == 256))
                    {
                        // Get the selected index from the TextureTileView
                        int index = SelectedTextureId;

                        if (index >= 0 && index < Textures.GetIdxLength())
                        {
                            // Create a new bitmap with the same size as the image from the clipboard
                            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height);

                            // Copy the image from the clipboard to the new bitmap
                            using (Graphics g = Graphics.FromImage(newBmp))
                            {
                                g.DrawImage(bmp, 0, 0);
                            }

                            // Replace the image at the specified index
                            Textures.Replace(index, newBmp);
                            ControlEvents.FireTextureChangeEvent(this, index);

                            // Update the _textureList to insert the index only once
                            if (!_textureList.Contains(index))
                            {
                                _textureList.Add(index);
                                _textureList.Sort();
                            }

                            // Update the VirtualListSize of the TextureTileView and invalidate the view
                            TextureTileView.VirtualListSize = _textureList.Count;
                            TextureTileView.Invalidate();
                            SelectedTextureId = index;
                            Options.ChangedUltimaClass["Texture"] = true;

                            // Check if the sound should be played
                            if (playCustomSound)
                            {
                                SoundPlayer player = new SoundPlayer();
                                player.SoundLocation = "sound.wav";
                                player.Play();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid image size. The image must be 64x64, 128x128, or 256x256.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No image in the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //Imports graphics from the clipboard into a temporary directory and the SelectImageFormatForm class is started.
        private void importByTempToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            if (Clipboard.ContainsImage())
            {
                using (Bitmap bmpTemp = new Bitmap(Clipboard.GetImage()))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);
                    // Check the size of the image.
                    if ((bitmap.Width == 64 && bitmap.Height == 64) ||
                        (bitmap.Width == 128 && bitmap.Height == 128) ||
                        (bitmap.Width == 256 && bitmap.Height == 256))
                    {
                        using (SelectImageFormatForm form = new SelectImageFormatForm(bitmap, _selectedTextureId))
                        {
                            if (form.ShowDialog() != DialogResult.OK)
                            {
                                return;
                            }

                            string appDirectory = Application.StartupPath;
                            string tempDirectory = Path.Combine(appDirectory, "tempGrafic");
                            if (!Directory.Exists(tempDirectory))
                            {
                                Directory.CreateDirectory(tempDirectory);
                            }

                            string fileExtension = form.SelectedImageFormat;
                            string fileName = Path.Combine(tempDirectory, $"Texture 0x{_selectedTextureId:X4}.{fileExtension}");

                            bitmap.Save(fileName);

                            Textures.Replace(_selectedTextureId, bitmap);

                            ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);

                            TextureTileView.Invalidate();

                            Options.ChangedUltimaClass["Texture"] = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show("The image must have a size of 64x64, 128x128, or 256x256", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Class SelectImageFormatForm
        public class SelectImageFormatForm : Form
        {
            private ComboBox _comboBox;
            private Button _okButton;
            private Button _cancelButton;
            private Button _clearDirectoryButton;
            private PictureBox _pictureBox;
            private Label _indexLabel;
            private Bitmap _image;

            public string SelectedImageFormat => _comboBox.SelectedItem.ToString();

            public SelectImageFormatForm(Bitmap image, int index)
            {
                // Save image.
                _image = image;

                // Create ComboBox.
                _comboBox = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Location = new Point(10, 10),
                    Width = 200
                };
                _comboBox.Items.Add("bmp");
                _comboBox.Items.Add("tif");
                _comboBox.Items.Add("tiff");
                _comboBox.SelectedIndex = 0;
                Controls.Add(_comboBox);

                // Create PictureBox.
                _pictureBox = new PictureBox
                {
                    Image = image,
                    SizeMode = PictureBoxSizeMode.AutoSize,
                    Location = new Point(10, 40)
                };
                Controls.Add(_pictureBox);

                // Create index label.
                int labelYPosition = 40 + image.Height + 10;
                _indexLabel = new Label
                {
                    Text = $"Index: {index} (0x{index:X})",
                    Location = new Point(10, labelYPosition)
                };
                Controls.Add(_indexLabel);

                // Create OK button.
                int buttonYPosition = labelYPosition + 30;
                _okButton = new Button
                {
                    Text = "Okay",
                    DialogResult = DialogResult.OK,
                    Location = new Point(10, buttonYPosition) // Changed position.
                };
                Controls.Add(_okButton);

                // Create cancel button.
                _cancelButton = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(100, buttonYPosition) // Changed position.
                };
                Controls.Add(_cancelButton);

                // Create clear directory button.
                _clearDirectoryButton = new Button
                {
                    Text = "Clear Directory",
                    Location = new Point(190, buttonYPosition) // Changed position.
                };
                _clearDirectoryButton.Click += ClearDirectoryButton_Click;
                Controls.Add(_clearDirectoryButton);

                // Set form properties.
                AcceptButton = _okButton;
                CancelButton = _cancelButton;
                ClientSize = new Size(300, buttonYPosition + 40); // Resized size.
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                StartPosition = FormStartPosition.CenterParent;
                Text = "Select image format";

                // Check if the directory exists and create it if it doesn't exist
                string directoryName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "tempGrafic");
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                // Save image in temporary directory.
                string fileName = $"Texture 0x{index:X4}.{SelectedImageFormat}";
                string tempFileName = Path.Combine(directoryName, fileName);
                ImageFormat format;

                switch (SelectedImageFormat.ToLower())
                {
                    case "bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case "tif":
                    case "tiff":
                        format = ImageFormat.Tiff;
                        break;
                    default:
                        throw new ArgumentException("Invalid file format.");
                }

                image.Save(tempFileName, format);
            }

            private void ClearDirectoryButton_Click(object sender, EventArgs e)
            {
                string directoryName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "tempGrafic");
                if (Directory.Exists(directoryName))
                {
                    Directory.Delete(directoryName, true);
                }
            }
        }
        // Import und Export Strg+V and Strg+X
        private void TexturesControl_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl+V key combination has been pressed
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Calling the importFromClipboardToolStripMenuItem_Click method to import the graphic from the clipboard.
                importFromClipboardToolStripMenuItem_Click(sender, e);
            }
            // Checking if the Ctrl+X key combination has been pressed
            else if (e.Control && e.KeyCode == Keys.X)
            {
                // Calling the copyToolStripMenuItem_Click method to import the graphic from the clipboard.
                copyToolStripMenuItem_Click(sender, e);
            }
        }

        #endregion

        #region a 90-degree rotation
        private void um90GradToolStripMenuItem_Click(object sender, EventArgs e)
        { // Check if a valid index is selected
            if (_selectedTextureId < 0)
            {
                return;
            }
            // Get the texture at the selected index
            Bitmap originalBmp = Textures.GetTexture(_selectedTextureId);
            if (originalBmp == null)
            {
                return;
            }

            /// Create a new Bitmap with the same size as the original texture
            Bitmap rotatedBmp = new Bitmap(originalBmp.Height, originalBmp.Width);
            // Use the Graphics class to rotate the image 90 degrees to the left
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // Move the origin to the center of the Bitmap
                g.TranslateTransform((float)rotatedBmp.Width / 2, (float)rotatedBmp.Height / 2);
                // Rotate the image 90 degrees to the left
                g.RotateTransform(-90);
                // Move the origin back to the top left corner
                g.TranslateTransform(-(float)originalBmp.Width / 2, -(float)originalBmp.Height / 2);
                // Draw the original image onto the new Bitmap
                g.DrawImage(originalBmp, new Point(0, 0));
            }

            // Replace the texture at the selected index with the rotated image
            Textures.Replace(_selectedTextureId, rotatedBmp);
            ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);

            // Update the view
            TextureTileView.Invalidate();
        }
        #endregion

        #region Search New
        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            var maximumIndex = Textures.GetIdxLength();

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            TextureTileView.FocusIndex = -1;
            SelectedTextureId = indexValue;
        }
        #endregion

        #region Copy Hex and Dec to clipbord
        private void copyDecAdressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy the decimal address to the clipboard
            try
            {
                Clipboard.SetDataObject(_selectedTextureId.ToString(), true, 5, 100);

                // Check if the sound should be played
                if (playCustomSound)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
            catch (ExternalException)
            {
                // Handle the error
                if (!playCustomSound)
                {
                    MessageBox.Show("Failed to copy data to the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void copyHexAdressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy the hex address to the clipboard
            try
            {
                Clipboard.SetText($"0x{_selectedTextureId:X}");

                // Check if the sound should be played
                if (playCustomSound)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
            catch (ExternalException)
            {
                // Handle the error
                if (!playCustomSound)
                {
                    MessageBox.Show("Failed to copy data to the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Sound
        private void PlaySoundtoolStripButton1_Click(object sender, EventArgs e)
        {
            // Toggle the value of the _playInsertSound field           
            playCustomSound = !playCustomSound;
        }
        #endregion

        #region TextureWindows Form
        private TextureWindowForm textureWindowForm;
        private void toolStripButtonTextureWindows_Click(object sender, EventArgs e)
        {
            // Check if the form is already open
            if (textureWindowForm == null || textureWindowForm.IsDisposed)
            {
                // Create a new instance of the shape if it's not already open
                textureWindowForm = new TextureWindowForm(this);
            }

            // Bring the shape to the foreground if it is already open
            textureWindowForm.BringToFront();

            // Open the Form
            textureWindowForm.Show();
        }
        #endregion

        #region // TextureWindowForm GetSelectedTextureId GetTexture GetIdxLengt
        public int GetSelectedTextureId()
        {
            return _selectedTextureId;
        }

        public Bitmap GetTexture(int id)
        {
            return Textures.GetTexture(id);
        }
        public int GetIdxLength()
        {
            return Textures.GetIdxLength();
        }
        #endregion

        #region BildFusion Form
        private BildFusionForm bildFusionForm;
        private void toolStripButtonPictureFusion_Click(object sender, EventArgs e)
        {
            // Check if the form is already open
            if (bildFusionForm == null || bildFusionForm.IsDisposed)
            {
                // Create a new instance of the form if it's not already open
                bildFusionForm = new BildFusionForm();
            }

            // Bring the form to the foreground if it is already open
            bildFusionForm.BringToFront();

            // Open the Form
            bildFusionForm.Show();
        }
        #endregion

        #region colorsTexturesToolStripMenuItem
        private void colorsTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the selected texture ID directly
            int selectedId = _selectedTextureId;

            // Create a new form
            Form colorForm = new Form
            {
                Text = $"Color Values for Texture ID: {selectedId}",
                Width = 500,
                Height = 970
            };

            // Create a new SplitContainer
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            // Add the SplitContainer to the form
            colorForm.Controls.Add(splitContainer);

            // Create a new PixelBox
            PixelBox pixelBox = new PixelBox
            {
                Dock = DockStyle.Fill,
                Cursor = Cursors.Cross // Place the mouse cursor here
            };

            // Create a new RichTextBox
            RichTextBox colorBox = new RichTextBox
            {
                Dock = DockStyle.Fill
            };

            // Add an event handler for the MouseClick event
            colorBox.MouseClick += (sender, e) =>
            {
                // Check if a row is selected
                if (colorBox.GetLineFromCharIndex(colorBox.SelectionStart) == colorBox.GetLineFromCharIndex(colorBox.SelectionStart + colorBox.SelectionLength - 1))
                {
                    // Extract the color code from the selected row
                    string selectedLine = colorBox.Lines[colorBox.GetLineFromCharIndex(colorBox.SelectionStart)];
                    Match match = Regex.Match(selectedLine, @"Hex: (#?[0-9A-Fa-f]{6})");
                    if (match.Success)
                    {
                        // Copy the color code to the clipboard
                        Clipboard.SetText(match.Value);
                    }
                }
            };

            // Add the RichTextBox to the SplitContainer
            splitContainer.Panel2.Controls.Add(colorBox);

            // Add the PixelBox to the SplitContainer
            splitContainer.Panel1.Controls.Add(pixelBox);

            // Get the selected texture
            Bitmap selectedTexture = null;
            if (selectedId >= 0 && selectedId < Textures.GetIdxLength())
            {
                selectedTexture = Textures.GetTexture(selectedId);
            }

            // Set the PixelBox's image to the selected texture
            pixelBox.Image = selectedTexture;

            pixelBox.PixelSelected += (x, y) =>
            {
                // Check that the x and y coordinates are within the boundaries of the texture
                if (x >= 0 && x < selectedTexture.Width && y >= 0 && y < selectedTexture.Height)
                {
                    // Reset the text color for all text in the RichTextBox
                    colorBox.SelectAll();
                    colorBox.SelectionColor = Color.Black;
                    colorBox.DeselectAll();

                    // Find the line in the RichTextBox that corresponds to the selected pixel                   
                    int lineIndex = y * selectedTexture.Width + x;

                    // Get the color of the current pixel                    
                    Color pixelColor = selectedTexture.GetPixel(x, y);

                    // Convert the RGB color values into a hex color code
                    string hexColor = ColorTranslator.ToHtml(pixelColor);

                    // Extract the hex color code from the selected row
                    string selectedLine = colorBox.Lines[lineIndex];
                    Match match = Regex.Match(selectedLine, @"Hex: (#?[0-9A-Fa-f]{6})");
                    if (match.Success)
                    {
                        string selectedHexColor = match.Groups[1].Value;

                        // Compare the hex color codes                      
                        if (hexColor == selectedHexColor)
                        {
                            // The color codes match, highlight the line in the RichTextBox
                            colorBox.Select(colorBox.GetFirstCharIndexFromLine(lineIndex), colorBox.Lines[lineIndex].Length);

                            // Change the text color of the selected line
                            colorBox.SelectionColor = Color.LightGray;

                            // Scroll to the selected line in the RichTextBox
                            colorBox.ScrollToCaret();
                        }
                    }
                }
            };

            // Loop through every pixel in the texture
            for (int y = 0; y < selectedTexture.Height; y++)
            {
                for (int x = 0; x < selectedTexture.Width; x++)
                {
                    // Get the color of the current pixel
                    Color pixelColor = selectedTexture.GetPixel(x, y);

                    // Add the RGB color values and the hex color code to the RichTextBox
                    string hexColor = ColorTranslator.ToHtml(pixelColor);

                    // Add the RGB color values and the Hex color code to the RichTextBox
                    string colorText = $"Pixel ({x}, {y}): Color [R={pixelColor.R}, G={pixelColor.G}, B={pixelColor.B}], Hex: {hexColor}";
                    colorBox.AppendText(colorText);

                    // Add 5 spaces without color
                    colorBox.AppendText("     ");

                    // Add 5 spaces with the background color
                    colorBox.SelectionBackColor = pixelColor;
                    colorBox.AppendText("     ");
                    colorBox.SelectionBackColor = Color.White;

                    // Add a new line
                    colorBox.AppendText("\n");
                }
            }
            // View the form
            colorForm.Show();
        }
        #endregion
    }
}
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
using System.Media;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class FontsControl : UserControl
    {
        private bool playCustomSound = false; //Sound True

        #region FontsControl InitializeComponent
        public FontsControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
            setOffsetsToolStripMenuItem.Visible = false;

            splitContainer2.SplitterDistance = splitContainer2.Height - 40;

            this.KeyDown += Form_KeyDown;
        }
        #endregion

        private bool _loaded;
        private static FontsControl _refMarker;
        private List<int> _fonts = new List<int>();

        #region Reload
        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }
        #endregion

        #region RefreshOnCharChange
        /// <summary>
        /// Refreshes view if Offset of Unicode char is changed
        /// </summary>
        public static void RefreshOnCharChange()
        {
            if ((int)_refMarker.treeView.SelectedNode.Parent.Tag != 1)
            {
                return;
            }

            _refMarker.FontsTileView.Invalidate();

            if (_refMarker.FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            int i = _refMarker.FontsTileView.SelectedIndices[0];
            _refMarker.toolStripStatusLabel1.Text =
                string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}",
                    (char)i, i,
                    UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].XOffset,
                    UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].YOffset);
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
            Options.LoadedUltimaClass["ASCIIFont"] = true;
            Options.LoadedUltimaClass["UnicodeFont"] = true;

            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();

                TreeNode node = new TreeNode("ASCII")
                {
                    Tag = 0
                };
                treeView.Nodes.Add(node);

                for (int i = 0; i < AsciiText.Fonts.Length; ++i)
                {
                    node = new TreeNode(i.ToString())
                    {
                        Tag = i
                    };
                    treeView.Nodes[0].Nodes.Add(node);
                }

                if (LoadUnicodeFontsCheckBox.Checked)
                {
                    node = new TreeNode("Unicode")
                    {
                        Tag = 1
                    };
                    treeView.Nodes.Add(node);

                    for (int i = 0; i < UnicodeFonts.Fonts.Length; ++i)
                    {
                        if (UnicodeFonts.Fonts[i] == null)
                        {
                            continue;
                        }

                        node = new TreeNode(i.ToString())
                        {
                            Tag = i
                        };
                        treeView.Nodes[1].Nodes.Add(node);
                    }
                }

                treeView.ExpandAll();
            }
            finally
            {
                treeView.EndUpdate();
            }

            treeView.SelectedNode = treeView.Nodes[0].Nodes[0];

            UpdateTileView();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region OnSelect
        private void OnSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.SelectedNode = treeView.SelectedNode.Nodes[0];
            }

            int font = (int)treeView.SelectedNode.Tag;

            try
            {
                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    setOffsetsToolStripMenuItem.Visible = true;

                    FontsTileView.VirtualListSize = 0x10000;
                }
                else
                {
                    setOffsetsToolStripMenuItem.Visible = false;

                    if (AsciiText.Fonts[font] == null)
                    {
                        return;
                    }

                    var length = AsciiText.Fonts[font].Characters.Length;
                    FontsTileView.VirtualListSize = length;

                    _fonts = new List<int>(length);
                    for (int i = 0; i < AsciiText.Fonts[font].Characters.Length; ++i)
                    {
                        _fonts.Add(i);
                    }
                }
            }
            finally
            {
                UpdateStatusStrip();
                FontsTileView.Invalidate();
            }
        }
        #endregion

        #region OnClickExport
        private void OnClickExport(object sender, EventArgs e)
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileType = (int)treeView.SelectedNode.Parent.Tag == 1 ? "Unicode" : "ASCII";
            string fileName = (int)treeView.SelectedNode.Parent.Tag == 1
                ? Path.Combine(path, $"{fileType} {(int)treeView.SelectedNode.Tag} 0x{FontsTileView.SelectedIndices[0]:X}.tiff")
                : Path.Combine(path, $"{fileType} {(int)treeView.SelectedNode.Tag} 0x{_fonts[FontsTileView.SelectedIndices[0]] + AsciiFontOffset:X}.tiff");

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                Bitmap bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[FontsTileView.SelectedIndices[0]].GetImage(true)
                             ?? new Bitmap(10, 10);

                bmp.Save(fileName, ImageFormat.Tiff);
            }
            else
            {
                var font = (int)treeView.SelectedNode.Tag;
                Bitmap bmp = AsciiText.Fonts[font].Characters[_fonts[FontsTileView.SelectedIndices[0]]]
                             ?? new Bitmap(10, 10);

                bmp.Save(fileName, ImageFormat.Tiff);
            }

            MessageBox.Show($"Character saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion
        private static int AsciiFontOffset => 32;

        #region OnClickImport
        private void OnClickImport(object sender, EventArgs e)
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose an image file to import";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap import = new Bitmap(dialog.FileName);
                if (import.Height > 255 || import.Width > 255)
                {
                    import.Dispose();

                    MessageBox.Show("Image Height or Width exceeds 255", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

                    return;
                }

                int font = (int)treeView.SelectedNode.Tag;

                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    UnicodeFonts.Fonts[font].Chars[FontsTileView.SelectedIndices[0]].SetBuffer(import);
                    Options.ChangedUltimaClass["UnicodeFont"] = true;
                }
                else
                {
                    AsciiText.Fonts[font].ReplaceCharacter(FontsTileView.SelectedIndices[0], import);
                    Options.ChangedUltimaClass["ASCIIFont"] = true;
                }

                FontsTileView.Invalidate();
            }
        }
        #endregion

        #region OnClickSave
        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                string fileName = UnicodeFonts.Save(path, (int)treeView.SelectedNode.Tag);
                MessageBox.Show($"Unicode saved to {fileName}", "Save", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["UnicodeFont"] = false;
            }
            else
            {
                string fileName = Path.Combine(path, "fonts.mul");
                AsciiText.Save(fileName);
                MessageBox.Show($"Fonts saved to {fileName}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["ASCIIFont"] = false;
            }
        }
        #endregion

        #region OnClickSetOffsets

        private FontOffsetForm _form;
        private void OnClickSetOffsets(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            int font = (int)treeView.SelectedNode.Tag;
            int cha = FontsTileView.SelectedIndices[0];
            if (_form?.IsDisposed == false)
            {
                return;
            }

            _form = new FontOffsetForm(font, cha)
            {
                TopMost = true
            };
            _form.Show();

            RefreshOnCharChange();
        }
        #endregion

        #region OnClickWriteText
        private void OnClickWriteText(object sender, EventArgs e)
        {
            int type = (int)treeView.SelectedNode.Parent.Tag;
            int font = (int)treeView.SelectedNode.Tag;

            new FontTextForm(type, font).Show();
        }
        #endregion

        #region FontsTileView_DrawItem
        private void FontsTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (treeView.Nodes.Count == 0)
            {
                return;
            }

            if (treeView.SelectedNode == null)
            {
                return;
            }

            int i;
            char c;

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                // Unicode fonts
                i = e.Index;
                c = (char)i;

                // draw what should be in tile
                e.Graphics.DrawString(c.ToString(), DefaultFont, Brushes.Gray, e.Bounds.X + (e.Bounds.Width / 2), e.Bounds.Y + (e.Bounds.Height / 2));

                // draw using font from uo if character exists
                var bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].GetImage();
                if (bmp == null)
                {
                    return;
                }

                int width = bmp.Width;
                int height = bmp.Height;

                if (width > e.Bounds.Width)
                {
                    width = e.Bounds.Width - 2;
                }

                if (height > e.Bounds.Height)
                {
                    height = e.Bounds.Height - 2;
                }

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, width, height));

                bmp.Dispose();
            }
            else
            {
                // ASCII Fonts
                i = e.Index;
                c = (char)(i + AsciiFontOffset);

                // draw what should be in tile
                e.Graphics.DrawString(c.ToString(), DefaultFont, Brushes.Gray, e.Bounds.X + (e.Bounds.Width / 2), e.Bounds.Y + (e.Bounds.Height / 2));

                // draw using font from uo if character exists
                var font = (int)treeView.SelectedNode.Tag;
                e.Graphics.DrawImage(AsciiText.Fonts[font].Characters[_fonts[i]], new Point(e.Bounds.X + 2, e.Bounds.Y + 2));
            }
        }
        #endregion

        #region FontsTileView_ItemSelectionChanged
        private void FontsTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (treeView.Nodes.Count == 0)
            {
                return;
            }

            UpdateStatusStrip();
        }
        #endregion

        #region UpdateStatusStrip
        private void UpdateStatusStrip()
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                toolStripStatusLabel1.Text = string.Empty;
                return;
            }

            int i = FontsTileView.SelectedIndices[0];

            toolStripStatusLabel1.Text = (int)treeView.SelectedNode.Parent.Tag == 1
                ? string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}", (char)i, i,
                    UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].XOffset,
                    UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].YOffset)
                : string.Format("'{0}' : {1} (0x{1:X})", (char)(_fonts[i] + AsciiFontOffset), _fonts[i] + AsciiFontOffset);
        }
        #endregion

        #region LoadUnicodeFontsCheckBox_CheckedChanged
        private void LoadUnicodeFontsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            string message = LoadUnicodeFontsCheckBox.Checked
                ? "Would you like to load all fonts including Unicode?"
                : "Load only ASCII fonts?";

            DialogResult result =
                MessageBox.Show(message, "Fonts reload",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Reload();
        }
        #endregion

        #region FontsControl_Resize
        private void FontsControl_Resize(object sender, EventArgs e)
        {
            splitContainer2.SplitterDistance = splitContainer2.Height - 40;
        }
        #endregion

        #region UpdateTileView(
        public void UpdateTileView()
        {
            FontsTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameFocusColor = FontsTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = FontsTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            FontsTileView.TileFocusColor = Options.TileFocusColor;
            FontsTileView.TileHighlightColor = Options.TileSelectionColor;
            FontsTileView.Invalidate();
        }
        #endregion       

        #region importClipbordToolStripMenuItem_Click
        private void importClipbordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Bitmap import = new Bitmap(Clipboard.GetImage());
                if (import.Height > 255 || import.Width > 255)
                {
                    import.Dispose();
                    MessageBox.Show("Image height or width exceeds 255", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                int font = (int)treeView.SelectedNode.Tag;

                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    UnicodeFonts.Fonts[font].Chars[FontsTileView.SelectedIndices[0]].SetBuffer(import);
                    Options.ChangedUltimaClass["UnicodeFont"] = true;
                }
                else
                {
                    AsciiText.Fonts[font].ReplaceCharacter(FontsTileView.SelectedIndices[0], import);
                    Options.ChangedUltimaClass["ASCIIFont"] = true;
                }

                FontsTileView.Invalidate();

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
                MessageBox.Show("Clipboard does not contain an image.");
            }
        }
        #endregion

        #region copyClipbordToolStripMenuItem_Click
        private void copyClipbordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FontsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            int font = (int)treeView.SelectedNode.Tag;
            Bitmap image;

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                image = UnicodeFonts.Fonts[font].Chars[FontsTileView.SelectedIndices[0]].GetImage();
            }
            else
            {
                image = AsciiText.Fonts[font].Characters[FontsTileView.SelectedIndices[0]];
            }

            if (image != null)
            {
                // Make a copy of the image
                Bitmap newImage = new Bitmap(image);
                // Make all pixels with color #d3d3d3 transparent
                newImage.MakeTransparent(Color.FromArgb(0xd3, 0xd3, 0xd3));
                // Create a new bitmap with the background color you want
                Bitmap finalImage = new Bitmap(newImage.Width, newImage.Height);
                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    // Draw a white background
                    g.Clear(Color.FromArgb(0xff, 0xff, 0xff));
                    // Draw the picture on it
                    g.DrawImage(newImage, 0, 0);
                }
                // Copy the final image to the clipboard
                Clipboard.SetImage(finalImage);

                // Check if the sound should be played
                if (playCustomSound)
                {
                    SoundPlayer player = new SoundPlayer();
                    player.SoundLocation = "sound.wav";
                    player.Play();
                }
            }
        }
        #endregion

        #region Form_KeyDown
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.X)
            {
                copyClipbordToolStripMenuItem_Click(sender, e);
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                importClipbordToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion        

        #region Soundbutton
        private void toolStripSplitSound_ButtonClick(object sender, EventArgs e)
        {
            // Toggle the value of the _playInsertSound field           
            playCustomSound = !playCustomSound;

            // Change the background image of the button
            ToolStripSplitButton button = (ToolStripSplitButton)sender;
            if (button.BackgroundImage == null)
            {
                // Create a red background image
                Bitmap newImage = new Bitmap(button.Width, button.Height);
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    // Draw a red background
                    g.Clear(Color.Red);
                }
                button.BackgroundImage = newImage;
            }
            else
            {
                // Reset the background image to zero
                button.BackgroundImage = null;
            }
        }
        #endregion
    }
}

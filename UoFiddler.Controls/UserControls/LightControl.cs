using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class LightControl : UserControl
    {
        public LightControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            LandTileText.Text = _landTile.ToString();
            LightTileText.Text = _lightTile.ToString();
        }

        private bool _loaded;
        private int _landTile = 0x3;
        private int _lightTile = 0x0B20;

        #region Reload
        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
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
            Options.LoadedUltimaClass["Light"] = true;

            treeViewLights.BeginUpdate();
            try
            {
                treeViewLights.Nodes.Clear();
                for (int i = 0; i < Ultima.Light.GetCount(); ++i)
                {
                    if (!Ultima.Light.TestLight(i))
                    {
                        continue;
                    }

                    var treeNode = new TreeNode(i.ToString())
                    {
                        Tag = i
                    };
                    treeViewLights.Nodes.Add(treeNode);
                }
            }
            finally
            {
                treeViewLights.EndUpdate();
            }

            if (treeViewLights.Nodes.Count > 0)
            {
                treeViewLights.SelectedNode = treeViewLights.Nodes[0];
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region  OnFilePathChangeEvent
        private void OnFilePathChangeEvent()
        {
            Reload();
        }
        #endregion

        #region unsafe Bitmap
        private unsafe Bitmap GetImage()
        {
            if (treeViewLights.SelectedNode == null)
            {
                return null;
            }

            if (!iGPreviewToolStripMenuItem.Checked)
            {
                return Ultima.Light.GetLight((int)treeViewLights.SelectedNode.Tag);
            }

            var bit = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
            using (Graphics g = Graphics.FromImage(bit))
            {
                Bitmap background = Ultima.Art.GetLand(_landTile);
                if (background != null)
                {
                    int i = 0;
                    for (int y = -22; y <= bit.Height; y += 22)
                    {
                        int x = i % 2 == 0 ? 0 : -22;
                        for (; x <= bit.Width; x += 44)
                        {
                            g.DrawImage(background, x, y);
                        }
                        ++i;
                    }
                }

                Bitmap lightBit = Ultima.Art.GetStatic(_lightTile);
                if (lightBit != null)
                {
                    g.DrawImage(lightBit, (bit.Width - lightBit.Width) / 2, (bit.Height - lightBit.Height) / 2);
                }
            }

            byte[] light = Ultima.Light.GetRawLight((int)treeViewLights.SelectedNode.Tag, out int lightWidth, out int lightHeight);

            if (light == null)
            {
                return bit;
            }

            BitmapData bd = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* imgPtr = (byte*)bd.Scan0;

            int lightStartX = (bit.Width / 2) - (lightWidth / 2);
            int lightStartY = 30 + (bit.Height / 2) - (lightHeight / 2);

            int lightEndX = lightStartX + lightWidth;
            int lightEndY = lightStartY + lightWidth;

            for (int y = 0; y < bd.Height; ++y)
            {
                for (int x = 0; x < bd.Width; ++x)
                {
                    byte b = *(imgPtr + 0);
                    byte g = *(imgPtr + 1);
                    byte r = *(imgPtr + 2);

                    double lightC = 0;

                    if (x >= lightStartX && x < lightEndX && y >= lightStartY && y < lightEndY)
                    {
                        int offset = ((y - lightStartY) * lightHeight) + (x - lightStartX);
                        if (offset < light.Length)
                        {
                            lightC = light[offset];
                            if (lightC > 31)
                            {
                                lightC = 0;
                            }
                            else
                            {
                                lightC *= 3 / 31D;
                            }
                        }
                    }
                    r /= 3;
                    g /= 3;
                    b /= 3;
                    r += (byte)(r * lightC);
                    g += (byte)(g * lightC);
                    b += (byte)(b * lightC);

                    *imgPtr++ = b;
                    *imgPtr++ = g;
                    *imgPtr++ = r;
                }
                imgPtr += bd.Stride - (bd.Width * 3);
            }
            bit.UnlockBits(bd);

            return bit;
        }
        #endregion

        #region AfterSelect
        private void AfterSelect(object sender, TreeViewEventArgs e)
        {
            pictureBoxPreview.Image = GetImage();
        }
        #endregion

        #region OnClickRemove
        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            int i = (int)treeViewLights.SelectedNode.Tag;
            DialogResult result = MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", i), "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Ultima.Light.Remove(i);
            treeViewLights.Nodes.Remove(treeViewLights.SelectedNode);
            treeViewLights.Invalidate();
            Options.ChangedUltimaClass["Light"] = true;
        }
        #endregion

        #region OnClickReplace
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    var bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    int i = (int)treeViewLights.SelectedNode.Tag;

                    Ultima.Light.Replace(i, bitmap);

                    treeViewLights.Invalidate();
                    AfterSelect(this, null);

                    Options.ChangedUltimaClass["Light"] = true;
                }
            }
        }
        #endregion

        #region OnTextChangedInsert
        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (int.TryParse(InsertText.Text, out int index) && index >= 0 && index <= 99)
            {
                InsertText.ForeColor = Ultima.Light.TestLight(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
                if (index > 99)
                {
                    MessageBox.Show("The ID cannot be greater than 99. Please enter a valid ID.");
                }
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

            if (!int.TryParse(InsertText.Text, out int index) || index < 0 || index > 99)
            {
                MessageBox.Show("Please enter a valid number between 0 and 99.");
                return;
            }

            if (Ultima.Light.TestLight(index))
            {
                var result = MessageBox.Show("A light with this index already exists. Do you want to replace it?", "Confirmation", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Select an image file to add to {index} (0x{index:X}) to insert";
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var bmp = new Bitmap(dialog.FileName);
                    Ultima.Light.Replace(index, bmp);

                    // Find the existing node and remove it
                    TreeNode existingNode = null;
                    foreach (TreeNode node in treeViewLights.Nodes)
                    {
                        if ((int)node.Tag == index)
                        {
                            existingNode = node;
                            break;
                        }
                    }
                    if (existingNode != null)
                    {
                        treeViewLights.Nodes.Remove(existingNode);
                    }

                    // Add the new node
                    var treeNode = new TreeNode(index.ToString()) { Tag = index };
                    treeViewLights.Nodes.Insert(index, treeNode);
                    treeViewLights.SelectedNode = treeNode;

                    Options.ChangedUltimaClass["Light"] = true;
                }
            }
        }
        #endregion

        #region OnClickSave
        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Light.Save(Options.OutputPath);
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Light"] = false;
        }
        #endregion

        #region OnClickExportBmp
        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.bmp");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickExportTiff
        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.tiff");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region OnClickExportJpg
        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.jpg");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Jpeg);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
        #endregion

        #region IgPreviewClicked
        private void IgPreviewClicked(object sender, EventArgs e)
        {
            iGPreviewToolStripMenuItem.Checked = !iGPreviewToolStripMenuItem.Checked;
            pictureBoxPreview.Image = GetImage();
        }
        #endregion

        #region OnPictureSizeChanged(
        private void OnPictureSizeChanged(object sender, EventArgs e)
        {
            pictureBoxPreview.Image = GetImage();
        }
        #endregion

        #region LandTileTextChanged
        private void LandTileTextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            if (Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, 0x3FFF))
            {
                LandTileText.ForeColor = !Ultima.Art.IsValidLand(index) ? Color.Red : Color.Black;
            }
            else
            {
                LandTileText.ForeColor = Color.Red;
            }
        }
        #endregion

        #region ShowPreviewPopu 1-2 and Next 1-2 and prev 1-2 and Update 1-2

        // Instance variable to store the current image index
        private int _currentImageIndex;
        private int _currentImageIndex2;

        // Instance variable to store the preview form
        private Form _previewForm;
        private Form _previewForm2;


        #region ShowPreviewPopup = LandTiles
        //Form 
        // ShowPreviewPopup method
        private void ShowPreviewPopup(Bitmap image)
        {
            // Get the parent ToolStrip control of the LandTileText control
            ToolStrip parentToolStrip = LandTileText.GetCurrentParent();

            // Convert the position of the LandTileText control to screen coordinates
            Point screenLocation = parentToolStrip.PointToScreen(LandTileText.Bounds.Location);

            // Move the screen location below the LandTileText control
            screenLocation.Offset(0, LandTileText.Height);

            // Check if the preview form has already been created
            if (_previewForm == null || _previewForm.IsDisposed)
            {
                // Create a new form to display the image
                _previewForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    StartPosition = FormStartPosition.Manual,
                    Size = new Size(image.Width + 55, image.Height + 120), // Add space for the buttons and form border
                    Location = screenLocation,
                    TopMost = true // Keep the form on top of other windows
                };

                // Create a new picture box to display the image
                PictureBox pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill, // Fill the entire available space
                    Image = image,
                    SizeMode = PictureBoxSizeMode.Zoom, // Adjust the image to fit the PictureBox
                };

                // Create a new FlowLayoutPanel to organize the buttons
                FlowLayoutPanel buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom, // Place it at the bottom of the form
                    FlowDirection = FlowDirection.RightToLeft, // Arrange the buttons from right to left
                    AutoSize = true, // Automatically adjust the size to fit the content
                };

                // Create the buttons and add them to the FlowLayoutPanel
                Button nextButton = new Button { Text = "Next", AutoSize = true };
                Button prevButton = new Button { Text = "Prev", AutoSize = true };
                Button closeButton = new Button { Text = "Close", AutoSize = true };

                buttonPanel.Controls.AddRange(new[] { nextButton, prevButton, closeButton });

                // Add event handlers for the buttons
                nextButton.Click += (s, e) => ShowNextImage();
                prevButton.Click += (s, e) => ShowPrevImage();
                closeButton.Click += (s, e) => _previewForm.Close();

                // Add the PictureBox and the FlowLayoutPanel to the form
                _previewForm.Controls.AddRange(new Control[] { pictureBox, buttonPanel });
            }
            else
            {
                // Update the size of the existing form
                _previewForm.Size = new Size(image.Width + 55, image.Height + 120); // Add space for the buttons and form border

                // Update the image displayed in the picture box
                PictureBox pictureBox = (PictureBox)_previewForm.Controls[0];
                pictureBox.Image = image;
            }

            // Show the preview form
            _previewForm.Show();
        }
        #endregion

        #region ShowPreviewPopup2 = StaticItems
        //Form StaticItems        
        private void ShowPreviewPopup2(Bitmap image)
        {
            // Get the parent ToolStrip control of the LightTileText control
            ToolStrip parentToolStrip = LightTileText.GetCurrentParent();

            // Convert the position of the LightTileText control to screen coordinates
            Point screenLocation = parentToolStrip.PointToScreen(LightTileText.Bounds.Location);

            // Move the screen location below the LightTileText control
            screenLocation.Offset(0, LightTileText.Height);

            // Create a new FlowLayoutPanel to organize the buttons
            FlowLayoutPanel buttonPanel;

            // Check if the preview form has already been created
            if (_previewForm2 == null || _previewForm2.IsDisposed)
            {
                // Create a new form to display the image
                _previewForm2 = new Form
                {
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    StartPosition = FormStartPosition.Manual,
                    Size = new Size(image.Width + 55, image.Height + 120), // Add space for the buttons and form border
                    Location = screenLocation,
                    TopMost = true // Keep the form on top of other windows
                };

                // Create a new picture box to display the image
                PictureBox pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill, // Fill the entire available space
                    Image = image,
                    SizeMode = PictureBoxSizeMode.Zoom, // Adjust the image to fit the PictureBox
                };

                buttonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom, // Place it at the bottom of the form
                    FlowDirection = FlowDirection.RightToLeft, // Arrange the buttons from right to left
                    AutoSize = true, // Automatically adjust the size to fit the content
                };

                // Create the buttons and add them to the FlowLayoutPanel
                Button nextButton = new Button { Text = "Next", AutoSize = true };
                Button prevButton = new Button { Text = "Prev", AutoSize = true };
                Button closeButton = new Button { Text = "Close", AutoSize = true };

                buttonPanel.Controls.AddRange(new[] { nextButton, prevButton, closeButton });

                // Add event handlers for the buttons
                nextButton.Click += (s, e) => ShowNextImage2();
                prevButton.Click += (s, e) => ShowPrevImage2();
                closeButton.Click += (s, e) => _previewForm2.Close();

                // Add the PictureBox and the FlowLayoutPanel to the form
                _previewForm2.Controls.AddRange(new Control[] { pictureBox, buttonPanel });
            }
            else
            {
                // Update the size of the existing form
                buttonPanel = (FlowLayoutPanel)_previewForm2.Controls[1];
                _previewForm2.Size = new Size(image.Width + 55, image.Height + buttonPanel.Height + 30); // Add space for the buttons and form border

                // Update the image displayed in the picture box
                PictureBox pictureBox = (PictureBox)_previewForm2.Controls[0];
                pictureBox.Image = image;
            }

            // Show the preview form
            _previewForm2.Show();

            _previewForm2.KeyPreview = true;
            _previewForm2.KeyUp -= PreviewForm2_KeyUp; // Unregister the event
            _previewForm2.KeyUp += PreviewForm2_KeyUp; // Register the event
        }
        #endregion

        #region PreviewForm2_KeyUp
        private void PreviewForm2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                // If the 'A' key is released, show the previous image
                ShowPrevImage2();
            }
            else if (e.KeyCode == Keys.D)
            {
                // If the 'D' key is released, show the next image
                ShowNextImage2();
            }
        }
        #endregion

        // Method to show the next image

        #region ShowNextImage
        private void ShowNextImage()
        {
            // Increase the current image index.
            do
            {
                _currentImageIndex++;
            } while (!Ultima.Art.IsValidLand(_currentImageIndex) && _currentImageIndex <= Ultima.Art.GetMaxItemId());

            // Check if the index is valid.
            if (_currentImageIndex > Ultima.Art.GetMaxItemId())
            {
                _currentImageIndex = 0;
            }

            // Display the next image.
            ShowPreviewPopup(Ultima.Art.GetLand(_currentImageIndex));

            // Update the display.
            UpdateDisplay();
        }
        #endregion

        #region ShowPrevImage
        // Method to show the previous image
        private void ShowPrevImage()
        {
            // Decrease the current image index.
            do
            {
                _currentImageIndex--;
            } while (!Ultima.Art.IsValidLand(_currentImageIndex) && _currentImageIndex >= 0);


            // Check if the index is valid.
            if (_currentImageIndex < 0)
            {
                //_currentImageIndex = Ultima.Art.GetMaxItemId();
                _currentImageIndex = 0;
                return;
            }

            // Display the previous image.
            ShowPreviewPopup(Ultima.Art.GetLand(_currentImageIndex));

            // Update the display.
            UpdateDisplay();
        }
        #endregion

        #region ShowNextImage2
        // Method to show the next image for items
        private void ShowNextImage2()
        {
            // Increase the current image index for items
            do
            {
                _currentImageIndex2++;
            } while (!Ultima.Art.IsValidStatic(_currentImageIndex2) && _currentImageIndex2 <= Ultima.Art.GetMaxItemId());

            // Check if the index is valid for items
            if (_currentImageIndex2 > Ultima.Art.GetMaxItemId())
            {
                _currentImageIndex2 = 0;
            }

            // Update the display for items
            UpdateDisplay2();
        }
        #endregion

        #region ShowPrevImage2
        // Method to show the previous image for items
        private void ShowPrevImage2()
        {
            // Decrease the current image index for items
            do
            {
                _currentImageIndex2--;
            } while (!Ultima.Art.IsValidStatic(_currentImageIndex2) && _currentImageIndex2 >= 0);

            // Check if the index is valid for items
            if (_currentImageIndex2 < 0)
            {
                _currentImageIndex2 = 0;
                return;
            }

            // Update the display for items
            UpdateDisplay2();
        }
        #endregion

        #region  UpdateDisplay
        // Method to update the display.
        private void UpdateDisplay()
        {
            // Display the image.
            ShowPreviewPopup(Ultima.Art.GetLand(_currentImageIndex));

            // Aktualisiere _landTile
            _landTile = _currentImageIndex;

            pictureBoxPreview.Image = GetImage();
            LandTileText.Text = _currentImageIndex.ToString();
        }
        #endregion

        #region  UpdateDisplay2
        // Method to update the display for items
        private void UpdateDisplay2()
        {
            // Show the image for items
            ShowPreviewPopup2(Ultima.Art.GetStatic(_currentImageIndex2));

            // Update _lightTile and pictureBoxPreview.Image and LightTileText.Text for items
            _lightTile = _currentImageIndex2;
            pictureBoxPreview.Image = GetImage();
            LightTileText.Text = _currentImageIndex2.ToString();
        }
        #endregion

        #region LandTileText_KeyUp
        private void LandTileText_KeyUp(object sender, KeyEventArgs e)
        {
            if (Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, 0x3FFF))
            {
                // Display the entered value in the text field
                LandTileText.Text = index.ToString();

                // Other actions you want to take with the value
                // ...
            }
        }
        #endregion

        #region LandTileText_KeyDown
        private void LandTileText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, 0x3FFF))
            {
                return;
            }

            if (!Ultima.Art.IsValidLand(index))
            {
                return;
            }

            previewContextMenuStrip.Close();
            _landTile = index;
            pictureBoxPreview.Image = GetImage();

            // Show the selected land tile image in the ShowPreviewPopup form
            Bitmap landTileImage = Ultima.Art.GetLand(index);
            if (landTileImage != null)
            {
                ShowPreviewPopup(landTileImage);
            }

            // Update the current image index for land tiles
            _currentImageIndex = index;
        }
        #endregion

        #region LightTileTextChanged
        private void LightTileTextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            // Keep the previewContextMenuStrip menu open
            previewContextMenuStrip.AutoClose = true;

            if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
            {
                LightTileText.ForeColor = !Ultima.Art.IsValidStatic(index) ? Color.Red : Color.Black;

                // Show preview of selected item image
                Bitmap itemImage = Ultima.Art.GetStatic(index);
                if (itemImage != null)
                {
                    ShowPreviewPopup2(itemImage);
                }

                // Set the current image index for items to the entered index
                _currentImageIndex2 = index;
            }
            else
            {
                LightTileText.ForeColor = Color.Red;
            }
        }
        #endregion

        #region LightTileText_KeyDown
        private void LightTileText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                // If the Return key is pressed, trigger the ShowPreviewPopup2 method
                if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
                {
                    Bitmap itemImage = Ultima.Art.GetStatic(index);
                    if (itemImage != null)
                    {
                        ShowPreviewPopup2(itemImage);
                    }

                    _currentImageIndex2 = index;
                }
            }
        }
        #endregion

        #region LightTileText_KeyUp
        private void LightTileText_KeyUp(object sender, KeyEventArgs e)
        {
            if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
            {
                // Show the selected item image in the ShowPreviewPopup2 form
                Bitmap itemImage = Ultima.Art.GetStatic(index);
                if (itemImage != null)
                {
                    ShowPreviewPopup2(itemImage);
                }

                // Update the current image index for items
                _currentImageIndex2 = index;
            }
        }
        #endregion

        #region lightTileToolStripMenuItem click

        private void lightTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
            {
                Bitmap itemImage = Ultima.Art.GetStatic(index);
                if (itemImage != null)
                {
                    ShowPreviewPopup2(itemImage);
                }

                _currentImageIndex2 = index;
            }
        }
        #endregion

        #region backgroundLandTileToolStripMenuItem click
        private void backgroundLandTileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
            {
                Bitmap itemImage = Ultima.Art.GetLand(index);
                if (itemImage != null)
                {
                    ShowPreviewPopup(itemImage);
                }

                _currentImageIndex = index;
            }
        }
        #endregion

        #endregion

        #region Copy Clipbord     
        // Aktuelle
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if an item is selected in the treeViewLights
            if (treeViewLights.SelectedNode == null)
            {
                MessageBox.Show("Please select an item first.");
                return;
            }

            // Get the selected item
            int selectedIndex = (int)treeViewLights.SelectedNode.Tag;
            // Get the bitmap for the selected item
            Bitmap bitmap = Ultima.Light.GetLight(selectedIndex);
            // Check if the bitmap exists
            if (bitmap != null)
            {
                // Change the color #D3D3D3 to #FFFFFF
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color pixelColor = bitmap.GetPixel(x, y);
                        if (pixelColor.R == 211 && pixelColor.G == 211 && pixelColor.B == 211)
                        {
                            bitmap.SetPixel(x, y, Color.FromArgb(255, 255, 255));
                        }
                    }
                }

                // Convert the image to a 16-bit color depth
                Bitmap bmp16bit = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                using (Graphics g = Graphics.FromImage(bmp16bit))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, bmp16bit.Width, bmp16bit.Height));
                }

                // Copy the graphic to the clipboard
                Clipboard.SetImage(bmp16bit);
                MessageBox.Show("The image has been copied to the clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Show a MessageBox to inform the user that the image was successfully copied
                MessageBox.Show("No image to copy!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Import Image Clipboard

        #region importToolStripMenuItem 
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Retrieve the image from the clipboard
            if (Clipboard.ContainsImage())
            {
                Bitmap clipboardImage = new Bitmap(Clipboard.GetImage());

                // Get the index from the toolStripTextBoxInsertImport control or the treeViewLights
                int index;
                if (!string.IsNullOrEmpty(toolStripTextBoxInsertImport.Text) &&
                    Utils.ConvertStringToInt(toolStripTextBoxInsertImport.Text, out index, 0, 99))
                {
                    // Use the index from the toolStripTextBoxInsertImport control
                }
                else if (treeViewLights.SelectedNode != null)
                {
                    // Use the index from the treeViewLights
                    index = (int)treeViewLights.SelectedNode.Tag;
                }
                else
                {
                    // Show a MessageBox to inform the user that no node is selected in the treeViewLights
                    MessageBox.Show("Please first select a node in the treeViewLights or enter an ID in the toolStripTextBoxInsertImport.");
                    return;
                }

                // Check if the index already exists in the treeViewLights
                TreeNode existingNode = null;
                foreach (TreeNode node in treeViewLights.Nodes)
                {
                    if ((int)node.Tag == index)
                    {
                        existingNode = node;
                        break;
                    }
                }

                // If the index does not exist, add a new node
                if (existingNode == null)
                {
                    existingNode = new TreeNode(index.ToString()) { Tag = index };
                    treeViewLights.Nodes.Add(existingNode);
                }

                // Define the colors to replace
                Dictionary<Color, Color> colorsToReplace = new Dictionary<Color, Color>
        {
            { Color.FromArgb(211, 211, 211), Color.FromArgb(255, 255, 255) }, // Replace #D3D3D3 with #ffffff
            { Color.FromArgb(255, 255, 247), Color.FromArgb(255, 255, 255) }  // Replace #fffff7 with #ffffff
        };

                // Iterate through each pixel of the image
                for (int x = 0; x < clipboardImage.Width; x++)
                {
                    for (int y = 0; y < clipboardImage.Height; y++)
                    {
                        // Get the color of the current pixel
                        Color pixelColor = clipboardImage.GetPixel(x, y);

                        // Check if the color of the current pixel is one of the colors to replace
                        if (colorsToReplace.ContainsKey(pixelColor))
                        {
                            // Set the color of the current pixel to the replacement color
                            clipboardImage.SetPixel(x, y, colorsToReplace[pixelColor]);
                        }
                    }
                }

                // Save the image from the clipboard to a temporary file in BMP format
                string tempFile = Path.GetTempFileName();
                clipboardImage.Save(tempFile, ImageFormat.Bmp);

                // Load the image from the temporary file
                Bitmap bmp = new Bitmap(tempFile);

                // Convert the image to 16-bit
                Bitmap newBmp = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format16bppRgb565);

                // Replace the selected image with the image from the clipboard
                Ultima.Light.Replace(index, newBmp);

                // Refresh the image in the pictureBoxPreview
                pictureBoxPreview.Image = GetImage();

                // Show a MessageBox to inform the user that the image was successfully imported
                MessageBox.Show("Image imported successfully!");
            }
        }

        #endregion

        #region toolStripTextBoxInsertImport_KeyDown
        private void toolStripTextBoxInsertImport_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the key pressed is the Enter key
            if (e.KeyCode == Keys.Enter)
            {
                // Call the importToolStripMenuItem_Click method
                importToolStripMenuItem_Click(sender, e);
            }
        }
        #endregion

        #region toolStripTextBoxInsertImport_KeyPress
        private void toolStripTextBoxInsertImport_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the key pressed is not a control key and not a digit
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                // Set the Handled property to true to cancel the key press
                e.Handled = true;
            }
        }
        #endregion

        #region Size Image = sizeToolStripMenuItem
        private void sizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the selected index from the treeViewLights
            if (treeViewLights.SelectedNode != null)
            {
                int index = (int)treeViewLights.SelectedNode.Tag;

                // Get the image
                Bitmap bmp = Ultima.Light.GetLight(index);

                // Create a new Form
                Form form = new Form
                {
                    Width = 300,
                    Height = 250, // Increased height to make room for the OK button
                    Text = "Change image size",
                    FormBorderStyle = FormBorderStyle.FixedDialog, // Prevent the form from being resized
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                // Create Labels for width and height
                Label labelWidth = new Label
                {
                    Text = "Width: " + bmp.Width, // Display the original width
                    Left = 50,
                    Top = 20
                };
                Label labelHeight = new Label
                {
                    Text = "Height: " + bmp.Height, // Display the original height
                    Left = 50,
                    Top = 60
                };

                // Create TrackBars for width and height
                TrackBar trackBarWidth = new TrackBar
                {
                    Minimum = 10,
                    Maximum = 500,
                    TickFrequency = 10,
                    Width = 200,
                    Left = 50,
                    Top = 40,
                    Value = bmp.Width // Set the initial value to the original width
                };

                TrackBar trackBarHeight = new TrackBar
                {
                    Minimum = 10,
                    Maximum = 500,
                    TickFrequency = 10,
                    Width = 200,
                    Left = 50,
                    Top = 80,
                    Value = bmp.Height // Set the initial value to the original height
                };

                // Add Scroll event handlers to update pictureBoxPreview in real-time
                trackBarWidth.Scroll += (s, e) =>
                {
                    labelWidth.Text = "Width: " + trackBarWidth.Value;
                    pictureBoxPreview.Image = new Bitmap(bmp, new Size(trackBarWidth.Value, trackBarHeight.Value));
                };
                trackBarHeight.Scroll += (s, e) =>
                {
                    labelHeight.Text = "Height: " + trackBarHeight.Value;
                    pictureBoxPreview.Image = new Bitmap(bmp, new Size(trackBarWidth.Value, trackBarHeight.Value));
                };

                // Create a Button to confirm the changes
                Button buttonOk = new Button
                {
                    Text = "OK",
                    Left = 100,
                    Top = 150, // Move the OK button further down
                    DialogResult = DialogResult.OK
                };
                form.Controls.Add(buttonOk);

                // Add the Labels, TrackBars and Button to the Form
                form.Controls.Add(labelWidth);
                form.Controls.Add(labelHeight);
                form.Controls.Add(trackBarWidth);
                form.Controls.Add(trackBarHeight);
                form.Controls.Add(buttonOk);

                // Show the Form and get the DialogResult
                DialogResult result = form.ShowDialog();

                // If the user clicked OK, change the size of the image
                if (result == DialogResult.OK)
                {
                    // Resize the image
                    Bitmap newBmp = new Bitmap(bmp, new Size(trackBarWidth.Value, trackBarHeight.Value));

                    // Replace the selected image with the resized image
                    Ultima.Light.Replace(index, newBmp);

                    // Refresh the image in the pictureBoxPreview
                    pictureBoxPreview.Image = GetImage();

                    // Show a MessageBox to inform the user that the image was successfully resized
                    MessageBox.Show("Image size changed successfully!");
                }
            }
        }
        #endregion
        #endregion
    }
}

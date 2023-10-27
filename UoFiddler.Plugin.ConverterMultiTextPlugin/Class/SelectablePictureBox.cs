using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Class
{
    public class SelectablePictureBox : PictureBox
    {
        private bool isDrawing = false;
        private List<Point> points = new List<Point>();
        private Color drawingColor = Color.Black;
        private int brushSize = 1;
        private Bitmap[] drawingBitmaps = new Bitmap[10]; // An array of bitmaps containing 10 images
        private int currentIndex = 0; // Define currentIndex as an instance variable
        private Rectangle selectionRectangle; //Draws the rectangle
        private bool canDraw = false; // Only drawing according to rectangle is allowed

        //public static AnimationEditFormButton AnimationEditForm;


        private Stack<Bitmap> drawingStates = new Stack<Bitmap>(); // Create a stack to store drawing states
        private Bitmap originalImage; // To save the original image

        public SelectablePictureBox()
        {
            this.SetStyle(ControlStyles.Selectable | ControlStyles.UserMouse, true);
            this.TabStop = true;            

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem loadItem = new ToolStripMenuItem("Load Image Drive"); // Loads the image
            ToolStripMenuItem pasteItem = new ToolStripMenuItem("Paste from clipboard"); //Imports the image from clipbord
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripMenuItem saveItem = new ToolStripMenuItem("save Image Drive"); // Save Image Drive
            ToolStripMenuItem copyItem = new ToolStripMenuItem("Copy image Clipbord"); //copy image clipbord
            ToolStripMenuItem selectionRectangleItemClipbord = new ToolStripMenuItem("Copy selection rectangle image"); //Copy rectangle image to clipbord
            ToolStripSeparator separator1 = new ToolStripSeparator();
            ToolStripMenuItem clearItem = new ToolStripMenuItem("Empty Picturebox"); // Clear Pixturebox
            ToolStripMenuItem undoItem = new ToolStripMenuItem("Undo Images"); // undo
            ToolStripMenuItem mirrorItem = new ToolStripMenuItem("Mirror image"); // Mirror Image
            ToolStripMenuItem mirrorItemAll = new ToolStripMenuItem("Mirror image All");
            ToolStripMenuItem selectionRectangleItem = new ToolStripMenuItem("selection rectangle"); //Invite rectangle image
            ToolStripMenuItem rotateImageItem = new ToolStripMenuItem("Rotate image"); //Rotate image 90 degrees counterclockwise
            ToolStripSeparator separator2 = new ToolStripSeparator();
            

            ToolTip toolTip = new ToolTip();
            loadItem.ToolTipText = "Loads the selected image from the directory into the picture box";
            pasteItem.ToolTipText = "Loads the image from clipbord into the picture box";
            saveItem.ToolTipText = "Saves the image from the picture box in a directory";
            copyItem.ToolTipText = "Copies the image to the clipbord";
            selectionRectangleItemClipbord.ToolTipText = "Saves the image marked in rectangle to the clipbord";
            clearItem.ToolTipText = "Empty the picture box";
            undoItem.ToolTipText = "Undo function for inserted images";
            mirrorItem.ToolTipText = "Reflects the imported image";
            mirrorItemAll.ToolTipText = "Mirro all images";
            selectionRectangleItem.ToolTipText = "Use ctrl to draw a rectangle and load the image there";
            rotateImageItem.ToolTipText = "Rotate image 90 degrees counterclockwise";

            // Adding a new option to select the active image
            ToolStripMenuItem selectItem = new ToolStripMenuItem("choose picture");
            for (int i = 0; i < drawingBitmaps.Length; i++)
            {
                var item = new ToolStripMenuItem($"Picture {i + 1}");
                item.Click += (sender, e) => SelectImage(i);
                selectItem.DropDownItems.Add(item);
            }

            contextMenu.Items.Add(loadItem);
            contextMenu.Items.Add(pasteItem);
            contextMenu.Items.Add(separator); //Separator
            contextMenu.Items.Add(saveItem);
            contextMenu.Items.Add(copyItem);
            contextMenu.Items.Add(selectionRectangleItemClipbord);
            contextMenu.Items.Add(separator1);
            contextMenu.Items.Add(clearItem);            
            contextMenu.Items.Add(selectionRectangleItem);
            contextMenu.Items.Add(undoItem);
            contextMenu.Items.Add(mirrorItem);
            contextMenu.Items.Add(mirrorItemAll);
            contextMenu.Items.Add(rotateImageItem);
            contextMenu.Items.Add(separator2);            
            contextMenu.Items.Add(selectItem);
            

            loadItem.Click += (sender, e) => LoadImage();
            pasteItem.Click += (sender, e) => PasteImage();
            saveItem.Click += (sender, e) => SaveImage();
            selectionRectangleItemClipbord.Click += (sender, e) => CopySelectionRectangleToClipboard();
            undoItem.Click += (sender, e) => UndoDrawing(); 
            mirrorItem.Click += (sender, e) => MirrorImage();
            selectionRectangleItem.Click += (sender, e) => DrawSelectionRectangleAndInsertImage();
            copyItem.Click += (sender, e) => CopyImage();
            clearItem.Click += (sender, e) => ClearImage();
            mirrorItemAll.Click += (sender, e) =>
            {
                // Make sure you have an instance of SelectablePictureBox on which to call the method.
                SelectablePictureBox anyInstance = AnimationEditFormButton.boxes.FirstOrDefault(box => box != null);
                if (anyInstance != null)
                {
                    anyInstance.MirrorAllImages();
                }
            };
            rotateImageItem.Click += (sender, e) => RotateImage();

            this.ContextMenuStrip = contextMenu;           

            this.MouseDown += (sender, e) =>
            {
                if (canDraw && Image != null && e.X < Image.Width && e.Y < Image.Height)
                {
                    // Check that drawingBitmaps[currentIndex] is not null before accessing it
                    if (drawingBitmaps[currentIndex] != null)
                    {
                        // Add the current state to the stack before drawing
                        drawingStates.Push((Bitmap)drawingBitmaps[currentIndex].Clone());
                    }

                    if (e.Button == MouseButtons.Left)
                    {
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            // Start drawing the selection rectangle
                            isDrawing = false;
                            points.Clear();
                            points.Add(e.Location);
                            canDraw = false;
                        }
                        else
                        {
                            isDrawing = true;
                            points.Add(e.Location);

                            if (drawingBitmaps[currentIndex] == null)
                            {
                                drawingBitmaps[currentIndex] = new Bitmap(Image);

                                if (drawingBitmaps[currentIndex] == null)
                                {
                                    MessageBox.Show("Error copying image.");
                                }
                            }
                        }
                    }
                }
            };


            this.MouseUp += (sender, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        // Stop drawing the selection rectangle
                        isDrawing = true;
                    }
                    else
                    {
                        isDrawing = false;
                        // Add the current state to the stack after drawing
                        if (drawingBitmaps[currentIndex] != null)
                        {
                            drawingStates.Push((Bitmap)drawingBitmaps[currentIndex].Clone());
                        }
                    }
                }
                points.Clear();
                canDraw = true;
            };

            this.MouseMove += (sender, e) =>
            {
                if (canDraw && isDrawing && Image != null && e.X < Image.Width && e.Y < Image.Height)
                {
                    // Save the current drawing state before the drawing is modified
                    if (drawingBitmaps[currentIndex] != null)  // Check that drawingBitmaps[currentIndex] is not null
                    {
                        drawingStates.Push((Bitmap)drawingBitmaps[currentIndex].Clone());
                    }

                    points.Add(e.Location);

                    if (drawingBitmaps[currentIndex] != null)
                    {
                        using (Graphics g = Graphics.FromImage(drawingBitmaps[currentIndex]))
                        {
                            if (points.Count > 1)
                            {
                                using (Pen pen = new Pen(drawingColor, brushSize))
                                {
                                    g.DrawLines(pen, points.ToArray());
                                }

                                // Creating a copy of the original image
                                Bitmap newImage = new Bitmap(Image);

                                // Draw on the drawn picture on top of the original picture
                                using (Graphics g2 = Graphics.FromImage(newImage))
                                {
                                    g2.DrawImage(drawingBitmaps[currentIndex], 0, 0);
                                }

                                // Set the new image as the image of the PictureBox
                                this.Image = newImage;

                                Refresh();
                            }
                        }
                    }
                }
                else if (!isDrawing && Control.ModifierKeys == Keys.Control)
                {
                    // Update the selection rectangle
                    points.Add(e.Location);
                    selectionRectangle = new Rectangle(points[0], new Size(e.X - points[0].X, e.Y - points[0].Y));
                    this.Invalidate();  // Forces the PictureBox to redraw
                }
            };


            this.Paint += (sender, e) =>
            {
                if (isDrawing && points.Count > 1)
                {
                    using (Pen pen = new Pen(drawingColor, brushSize))
                    {
                        e.Graphics.DrawLines(pen, points.ToArray());
                    }
                }
                else if (!isDrawing && Control.ModifierKeys == Keys.Control)
                {
                    // Draw the selection rectangle
                    using (Pen pen = new Pen(Color.Yellow))
                    {
                        pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        e.Graphics.DrawRectangle(pen, selectionRectangle);
                    }
                }
            };

            this.MouseLeave += (sender, e) =>
            {
                // Empty the points list when the mouse leaves the PictureBox
                points.Clear();
            };
        }

        #region LoadImage
        private void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog.Title = "Please select an image file.";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Image image = Image.FromFile(openFileDialog.FileName);
                this.Image = image;
                originalImage = new Bitmap(image); // Save the original image
                drawingBitmaps[currentIndex] = new Bitmap(image.Width, image.Height);
            }
        }
        #endregion

        #region DrawSelevtRectangleAndInsertImage        

        public void DrawSelectionRectangleAndInsertImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog.Title = "Please select an image file.";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap insertedImage = new Bitmap(Image.FromFile(openFileDialog.FileName));

                // Loop through each pixel in the inserted image
                for (int x = 0; x < insertedImage.Width; x++)
                {
                    for (int y = 0; y < insertedImage.Height; y++)
                    {
                        // Check the color of the pixel
                        Color pixelColor = insertedImage.GetPixel(x, y);
                        if (pixelColor.ToArgb() == Color.Black.ToArgb() || pixelColor.ToArgb() == Color.White.ToArgb())
                        {
                            // Set the pixel to transparent if it is #000000 or #ffffff
                            insertedImage.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }

                if (this.Image != null)
                {
                    Bitmap bitmap = new Bitmap(this.Image);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Paste the edited image into the selection rectangle
                        g.DrawImage(insertedImage, selectionRectangle);
                    }

                    this.Image = bitmap;
                }
            }
        }

        #endregion

        #region OnPictureBoxSelected
        private void OnPictureBoxSelected(int index)
        {
            currentIndex = index; // Set currentIndex to the index of the selected PictureBox
        }
        #endregion

        #region MouseDown
        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();
            base.OnMouseDown(e);
        }
        #endregion

        #region IsInputKey
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down) return true;
            if (keyData == Keys.Left || keyData == Keys.Right) return true;
            return base.IsInputKey(keyData);
        }
        #endregion

        #region OnEnter
        protected override void OnEnter(EventArgs e)
        {
            this.Invalidate();
            base.OnEnter(e);
        }
        #endregion

        #region OnLeave
        protected override void OnLeave(EventArgs e)
        {
            this.Invalidate();
            base.OnLeave(e);
        }
        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (this.Focused)
            {
                var rc = this.ClientRectangle;
                rc.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(pe.Graphics, rc);
            }

            if (drawingBitmaps[currentIndex] != null) // If a drawn image exists
            {
                pe.Graphics.DrawImage(drawingBitmaps[currentIndex], 0, 0); // Draw the drawn picture
            }
            else if (this.Image != null) // If no drawn image exists
            {
                pe.Graphics.DrawImage(this.Image, 0, 0); // Draw the original image
            }

            // Draw the selection rectangle
            if (!isDrawing && Control.ModifierKeys == Keys.Control)
            {
                using (Pen pen = new Pen(Color.Yellow))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    pe.Graphics.DrawRectangle(pen, selectionRectangle);
                }
            }
        }

        #endregion

        #region OnPreviewKeyDown
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Control && e.KeyCode == Keys.X)
            {
                if (isDrawing && drawingBitmaps[currentIndex] != null) // Use the current bitmap
                {
                    Clipboard.SetImage(drawingBitmaps[currentIndex]);
                }
                else if (this.Image != null)
                {
                    Clipboard.SetImage(this.Image);
                }
                else
                {
                    MessageBox.Show("There is no image to copy.");
                }
            }
            else if (e.Control && e.KeyCode == Keys.V)
            {
                if (Clipboard.ContainsImage())
                {
                    Image image = Clipboard.GetImage();
                    this.Image = image;
                    drawingBitmaps[currentIndex] = new Bitmap(image.Width, image.Height);
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image.");
                }
            }
        }
        #endregion

        #region Set Color
        public void SetDrawingColor(Color color)
        {
            drawingColor = color;
        }
        #endregion

        #region Set Brush Size
        public void SetBrushSize(int size)
        {
            brushSize = size;
        }
        #endregion

        #region GetTransparentImage
        public Bitmap GetTransparentImage()
        {
            if (this.Image == null)
            {
                return null;
            }

            // Convert the image to a bitmap
            Bitmap bitmap = new Bitmap(this.Image);

            // Define the colors you want to make transparent
            Color[] colorsToMakeTransparent = new Color[]
            {
                Color.FromArgb(0, 0, 0),      // Black
                Color.FromArgb(255, 255, 255),// White
                Color.FromArgb(211, 211, 211) // Light gray (d3d3d3 in RGB)
            };

            // Make every color in the image transparent
            foreach (Color color in colorsToMakeTransparent)
            {
                bitmap = MakeTransparent(bitmap, color);
            }

            return bitmap;
        }
        #endregion

        #region Toggle Drawing
        public void ToggleDrawing()
        {
            isDrawing = !isDrawing;
        }
        #endregion

        #region SaveImage        

        private void SaveImage()
        {
            if (DrawingBitmaps[CurrentIndex] != null)
            {
                // Make a copy of the original image
                Bitmap newImage = new Bitmap(Image);

                // Draw the drawn image on top of the original image
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    g.DrawImage(DrawingBitmaps[CurrentIndex], 0, 0);
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Bitmap Image|*.bmp|JPEG Image|*.jpg|GIF Image|*.gif|PNG Image|*.png";
                saveFileDialog.Title = "Please select a location to save the image.";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        switch (saveFileDialog.FilterIndex)
                        {
                            case 1:
                                newImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                                break;
                            case 2:
                                newImage.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                                break;
                            case 3:
                                newImage.Save(memory, System.Drawing.Imaging.ImageFormat.Gif);
                                break;
                            case 4:
                                newImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                break;
                        }

                        memory.Position = 0; // Reset the position of the stream

                        using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                        {
                            memory.CopyTo(fs);
                        }
                    }
                }
            }
        }

        #endregion

        #region paste Image
        private void PasteImage()
        {
            if (Clipboard.ContainsImage())
            {
                Image image = Clipboard.GetImage();
                this.Image = image;
                drawingBitmaps[currentIndex] = new Bitmap(image.Width, image.Height);
            }
            else
            {
                MessageBox.Show("The clipboard does not contain an image.");
            }
        }
        #endregion

        #region Copy image
        private void CopyImage()
        {
            if (isDrawing && drawingBitmaps[currentIndex] != null) // Use the current bitmap
            {
                Clipboard.SetImage(drawingBitmaps[currentIndex]);
            }
            else if (this.Image != null)
            {
                Clipboard.SetImage(this.Image);
            }
            else
            {
                MessageBox.Show("There is no image to copy.");
            }
        }
        #endregion
        // A new way to select the active image
        #region SelectImage
        private void SelectImage(int index)
        {
            if (index >= 0 && index < drawingBitmaps.Length)
            {
                currentIndex = index;

                if (drawingBitmaps[currentIndex] != null)
                    this.Image = drawingBitmaps[currentIndex];
            }
            else
            {
                // Handle the error case here
            }
        }
        #endregion
        #region Clear Image
        public void ClearImage()
        {
            this.Image = null;
            if (drawingBitmaps[currentIndex] != null)
            {
                drawingBitmaps[currentIndex].Dispose();
                drawingBitmaps[currentIndex] = null;
            }
        }
        #endregion

        #region Public IsDrawing
        public bool IsDrawing()
        {
            return isDrawing;
        }
        #endregion

        #region Mirror 
        public void MirrorImage()
        {
            if (this.Image != null)
            {
                this.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                this.Invalidate(); // Forces the control to be redrawn
            }
        }
        #endregion

        #region Mirror ALl Images
        public void MirrorAllImages()
        {
            foreach (var pictureBox in AnimationEditFormButton.boxes)
            {
                if (pictureBox != null && pictureBox.Image != null)
                {
                    pictureBox.MirrorImage();
                }
            }
        }
        #endregion

        #region Make Transparent
        private Bitmap MakeTransparent(Bitmap original, Color color)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    Color originalColor = original.GetPixel(i, j);
                    if (originalColor == color)
                    {
                        newBitmap.SetPixel(i, j, Color.Transparent);
                    }
                    else
                    {
                        newBitmap.SetPixel(i, j, originalColor);
                    }
                }
            }
            return newBitmap;
        }
        #endregion

        #region GetPixelColor
        public Color GetPixelColor(Point location)
        {
            if (this.Image != null && location.X >= 0 && location.Y >= 0 && location.X < this.Image.Width && location.Y < this.Image.Height)
            {
                // Create a bitmap from the image and get the color at the specified position
                Bitmap bitmap = new Bitmap(this.Image);
                return bitmap.GetPixel(location.X, location.Y);
            }
            return Color.Transparent;
        }

        #endregion

        //Not active yet
        public void DrawOnImage(Color color, Point location)
        {
            // Check if the color is #000000 or #ffffff
            if (color.ToArgb() != Color.Black.ToArgb() && color.ToArgb() != Color.White.ToArgb())
            {
                // Draw on the image if the color is not #000000 or #ffffff
                using (Graphics g = Graphics.FromImage(this.Image))
                {
                    using (Brush brush = new SolidBrush(color))
                    {
                        g.FillRectangle(brush, new Rectangle(location, new Size(1, 1)));
                    }
                }
            }
        }

        #region Copy Selection Rectangle to Clipbord
        private void CopySelectionRectangleToClipboard()
        {
            if (this.Image != null)
            {
                Bitmap bitmap = new Bitmap(this.Image);
                Bitmap croppedBitmap = bitmap.Clone(selectionRectangle, bitmap.PixelFormat);
                Clipboard.SetImage(croppedBitmap);
            }
            else
            {
                MessageBox.Show("There is no image to copy.");
            }
        }
        #endregion

        #region Undo Drawing
        public void UndoDrawing()
        {
            // Make sure the currentIndex is within the valid range
            if (currentIndex >= 0 && currentIndex < drawingBitmaps.Length)
            {
                // Check if there is a previous character state
                if (drawingStates.Count > 0)
                {
                    // Reset the character state
                    drawingBitmaps[currentIndex] = drawingStates.Pop();
                    if (originalImage != null) // Check that originalImage is not null
                    {
                        Bitmap newImage = new Bitmap(originalImage);
                        using (Graphics g = Graphics.FromImage(newImage))
                        {
                            g.DrawImage(drawingBitmaps[currentIndex], 0, 0);
                        }
                        this.Image = newImage;
                    }
                    else
                    {
                        // Handle the case when originalImage is null
                    }
                }
                else
                {
                    // Handle the case when no previous character state exists
                }
            }
            else
            {
                // Handle the case when the currentIndex is out of range
            }
        }

        // Method to set the index of the SelectablePictureBox
        public void SetCurrentIndex(int index)
        {
            if (index >= 0 && index < drawingBitmaps.Length)
            {
                currentIndex = index;
            }
            else
            {
                // Handle the case when the index passed is out of range
            }
        }
        #endregion

        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                // Release of bitmap objects
                for (int i = 0; i < drawingBitmaps.Length; i++)
                {
                    if (drawingBitmaps[i] != null)
                    {
                        drawingBitmaps[i].Dispose();
                        drawingBitmaps[i] = null;
                    }
                }

                if (originalImage != null)
                {
                    originalImage.Dispose();
                    originalImage = null;
                }

                // Perform further cleanup here if necessary
            }

            // Call the base class to call the base class's Dispose method
            base.Dispose(disposing);
        }
        #endregion

        #region Puplic for clases
        public Bitmap[] DrawingBitmaps
        {
            get { return drawingBitmaps; }
            set { drawingBitmaps = value; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { currentIndex = value; }
        }

        public Bitmap OriginalImage
        {
            get { return originalImage; }
            set { originalImage = value; }
        }
        #endregion

        #region Rotate Image
        private void RotateImage()
        {
            if (this.Image != null)
            {
                this.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                this.Invalidate(); // Forces the control to redraw
            }
        }
        #endregion
    }
}
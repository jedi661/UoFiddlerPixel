using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.ConverterMultiTextPlugin.Class;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationEditFormButton : Form
    {

        int currentIndex = 0; // The index of the current image
        //SelectablePictureBox[] boxes;
        CheckBox[] checks;
        private bool pipetteMode = false;

        Label line = new Label();
        Label vLine = new Label();

        //Zoom
        private int zoomLevel1 = 0;
        private int zoomLevel2 = 0;
        private int zoomLevel3 = 0;
        private int zoomLevel4 = 0;
        private int zoomLevel5 = 0;
        private int zoomLevel6 = 0;
        private int zoomLevel7 = 0;
        private int zoomLevel8 = 0;
        private int zoomLevel9 = 0;
        private int zoomLevel10 = 0;

        private SelectablePictureBox[] pictureBoxes;

        public static SelectablePictureBox[] boxes = new SelectablePictureBox[10];

        private Dictionary<string, Image> originalImages = new Dictionary<string, Image>();
        private Dictionary<string, Image> images = new Dictionary<string, Image>();
        private int imageCounter = 1;
        private Timer playTimer;
        private int currentImageIndex = 0;

        public AnimationEditFormButton()
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            this.KeyPreview = true; // Enable the key preview

            // Initialize the boxes and checks arrays
            boxes = new SelectablePictureBox[]
            {
            selectablePictureBox1,
            selectablePictureBox2,
            selectablePictureBox3,
            selectablePictureBox4,
            selectablePictureBox5,
            selectablePictureBox6,
            selectablePictureBox7,
            selectablePictureBox8,
            selectablePictureBox9,
            selectablePictureBox10
            };

            checks = new CheckBox[]
            {
            checkBox1,
            checkBox2,
            checkBox3,
            checkBox4,
            checkBox5,
            checkBox6,
            checkBox7,
            checkBox8,
            checkBox9,
            checkBox10
            };

            timer.Interval = 1000; // Set the initial interval to 1 second
            timer.Tick += new EventHandler(timer_Tick);

            // Add the ValueChanged event handler to numericUpDownFrameDelay
            numericUpDownFrameDelay.ValueChanged += new EventHandler(numericUpDownFrameDelay_ValueChanged);

            // Add the click event handler to btColordialog
            btColordialog.Click += (sender, e) => SelectColor();

            tboxColorCode.TextChanged += tboxColorCode_TextChanged;

            // Zoom Tags
            zoomInButton1.Tag = selectablePictureBox1;
            zoomOutButton1.Tag = selectablePictureBox1;

            zoomInButton2.Tag = selectablePictureBox2;
            zoomOutButton2.Tag = selectablePictureBox2;

            zoomInButton3.Tag = selectablePictureBox3;
            zoomOutButton3.Tag = selectablePictureBox3;

            zoomInButton4.Tag = selectablePictureBox4;
            zoomOutButton4.Tag = selectablePictureBox4;

            zoomInButton5.Tag = selectablePictureBox5;
            zoomOutButton5.Tag = selectablePictureBox5;

            zoomInButton6.Tag = selectablePictureBox6;
            zoomOutButton6.Tag = selectablePictureBox6;

            zoomInButton7.Tag = selectablePictureBox7;
            zoomOutButton7.Tag = selectablePictureBox7;

            zoomInButton8.Tag = selectablePictureBox8;
            zoomOutButton8.Tag = selectablePictureBox8;

            zoomInButton9.Tag = selectablePictureBox9;
            zoomOutButton9.Tag = selectablePictureBox9;

            zoomInButton10.Tag = selectablePictureBox10;
            zoomOutButton10.Tag = selectablePictureBox10;


            // Create a new Label control for the line
            Label line = new Label();
            Label vLine = new Label();

            // Setzen Sie die Eigenschaften des Labels
            line.BackColor = Color.Red; // The color of the line
            line.Width = AnimationPictureBox.Width; // The height of the line
            line.Visible = false; // The label is initially invisible

            vLine.BackColor = Color.Red; // The color of the line
            vLine.Height = AnimationPictureBox.Height; // The height of the line
            vLine.Visible = false; // The label is initially invisible

            // Add the label to the panelAnimationPictureBox
            panelAnimationPictureBox.Controls.Add(line);
            panelAnimationPictureBox.Controls.Add(vLine);
            panelAnimationPictureBox.BackColor = Color.Transparent; // The panel is transparent

            // A method to update the position and thickness of the line
            void UpdateLine()
            {
                int y = (int)numericUpDownHighAnimationPictureBox.Value;
                int thickness = (int)numericUpDownSizeLineAnimationPictureBox.Value;

                line.Top = y; // The position of the line
                line.Height = thickness; // The thickness of the line

                // Set the color based on the value of the new NumericUpDown controls
                if (numericUpDownColor.Value == 0)
                {
                    line.BackColor = Color.Red; // Red
                }
                else if (numericUpDownColor.Value == 1)
                {
                    line.BackColor = Color.Blue; // Blue
                }
                else if (numericUpDownColor.Value == 2)
                {
                    line.BackColor = Color.Green; // Green
                }
                else if (numericUpDownColor.Value == 3)
                {
                    line.BackColor = Color.Yellow; // Yellow
                }
                else if (numericUpDownColor.Value == 4)
                {
                    line.BackColor = Color.Black; // Black
                }
                else if (numericUpDownColor.Value == 5)
                {
                    line.BackColor = Color.White; // White
                }
                else if (numericUpDownColor.Value == 6)
                {
                    line.BackColor = Color.Orange; // Orange
                }
                else if (numericUpDownColor.Value == 7)
                {
                    line.BackColor = Color.Pink; // Pink
                }
                else if (numericUpDownColor.Value == 8)
                {
                    line.BackColor = Color.Turquoise; // Turquoise
                }
                else if (numericUpDownColor.Value == 9)
                {
                    line.BackColor = Color.Gray; // Gray
                }
                else if (numericUpDownColor.Value == 10)
                {
                    line.BackColor = Color.Gold; // Gold
                }

                line.Visible = thickness > 0; // Make sure the line is only visible when its thickness is greater than 0
                line.BringToFront(); // Make sure the line is in front of the PictureBox
            }

            // A method to update the position and thickness of the line
            void UpdateVLine()
            {
                int x = (int)numericUpDownWidthAnimationPictureBox.Value;
                int thickness = (int)numericUpDownSizeLine2AnimationPictureBox.Value;

                vLine.Left = x; // The position of the line
                vLine.Width = thickness; // The width of the line

                // Set the color based on the value of the new NumericUpDown controls
                if (numericUpDownColor.Value == 0)
                {
                    vLine.BackColor = Color.Red; // Red
                }
                else if (numericUpDownColor.Value == 1)
                {
                    vLine.BackColor = Color.Blue; // Blue
                }
                else if (numericUpDownColor.Value == 2)
                {
                    line.BackColor = Color.Green; // Green
                }
                else if (numericUpDownColor.Value == 3)
                {
                    line.BackColor = Color.Yellow; // Yellow
                }
                else if (numericUpDownColor.Value == 4)
                {
                    line.BackColor = Color.Yellow; // Black
                }
                else if (numericUpDownColor.Value == 5)
                {
                    line.BackColor = Color.White; // White
                }
                else if (numericUpDownColor.Value == 6)
                {
                    line.BackColor = Color.Orange; // Orange
                }
                else if (numericUpDownColor.Value == 7)
                {
                    line.BackColor = Color.Pink; // Pink
                }
                else if (numericUpDownColor.Value == 8)
                {
                    line.BackColor = Color.Turquoise; // Turquoise
                }
                else if (numericUpDownColor.Value == 9)
                {
                    line.BackColor = Color.Gray; // Gray
                }
                else if (numericUpDownColor.Value == 10)
                {
                    line.BackColor = Color.Gold; // Gold
                }

                vLine.Visible = thickness > 0; // Make sure the line is only visible when its thickness is greater than 0
                vLine.BringToFront(); // Make sure the line is in front of the PictureBox
            }

            // Call UpdateLine when the value of the NumericUpDown controls changes
            numericUpDownHighAnimationPictureBox.ValueChanged += (s, e) => UpdateLine();
            numericUpDownSizeLineAnimationPictureBox.ValueChanged += (s, e) => UpdateLine();
            numericUpDownWidthAnimationPictureBox.ValueChanged += (s, e) => UpdateVLine();
            numericUpDownSizeLine2AnimationPictureBox.ValueChanged += (s, e) => UpdateVLine();
            numericUpDownColor.ValueChanged += (s, e) => { UpdateLine(); UpdateVLine(); UpdateColor(); };

            void UpdateColor()
            {
                int colorValue = (int)numericUpDownColor.Value;
                if (colorValue == 0)
                {
                    line.BackColor = Color.Red;
                    vLine.BackColor = Color.Red;
                }
                else if (colorValue == 1)
                {
                    line.BackColor = Color.Blue;
                    vLine.BackColor = Color.Blue;
                }
                else if (colorValue == 2)
                {
                    line.BackColor = Color.Green;
                    vLine.BackColor = Color.Green;
                }
                else if (colorValue == 3)
                {
                    line.BackColor = Color.Yellow;
                    vLine.BackColor = Color.Yellow;
                }
                else if (colorValue == 4)
                {
                    line.BackColor = Color.Black;
                    vLine.BackColor = Color.Black;
                }
                else if (colorValue == 5)
                {
                    line.BackColor = Color.White;
                    vLine.BackColor = Color.White;
                }
                else if (colorValue == 6)
                {
                    line.BackColor = Color.Orange;
                    vLine.BackColor = Color.Orange;
                }
                else if (colorValue == 7)
                {
                    line.BackColor = Color.Pink;
                    vLine.BackColor = Color.Pink;
                }
                else if (colorValue == 8)
                {
                    line.BackColor = Color.Turquoise;
                    vLine.BackColor = Color.Turquoise;
                }
                else if (colorValue == 9)
                {
                    line.BackColor = Color.Gray;
                    vLine.BackColor = Color.Gray;
                }
                else if (colorValue == 10)
                {
                    line.BackColor = Color.Gold;
                    vLine.BackColor = Color.Gold;
                }

                // Update the line and vLine directly
                line.Invalidate();
                vLine.Invalidate();

                // Initialize your pictureBoxes array with the appropriate instances
                pictureBoxes = new SelectablePictureBox[10];

                // Add the PictureBox instances to your form and initialize the indexes
                for (int i = 0; i < pictureBoxes.Length; i++)
                {
                    pictureBoxes[i] = new SelectablePictureBox();
                    Controls.Add(pictureBoxes[i]); // Add the PictureBox to the form.
                    pictureBoxes[i].SetCurrentIndex(i + 1); // Set the index for each PictureBox
                }

                // Call the UndoDrawing method on all PictureBox instances
                SelectablePictureBoxHelper.UndoDrawing(pictureBoxes);
            }

            // Initialisiere den Timer
            playTimer = new Timer();
            playTimer.Tick += new EventHandler(PlayTimer_Tick);
        }
        #region Time_Tick         

        private void timer_Tick(object sender, EventArgs e)
        {
            // Get the next image index
            currentIndex++;
            if (currentIndex >= boxes.Length)
                currentIndex = (int)numericUpDownStartDelay.Value - 1; // Subtrahieren Sie 1 vom Wert von numericUpDownStartDelay

            // Deklarieren Sie die Variablen außerhalb des if-Blocks
            Bitmap imageBitmap = null;
            Bitmap flippedImageBitmap = null;

            // Check if the current PictureBox has an image and is active
            if (boxes[currentIndex].Image != null && checks[currentIndex].Checked)
            {
                // Check if the image exists in the PictureBox
                if (boxes[currentIndex].Image != null)
                {
                    // Get the image from the corresponding selectablePictureBox
                    imageBitmap = new Bitmap(boxes[currentIndex].Image);

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
                        imageBitmap = MakeTransparent(imageBitmap, color);
                    }

                    // Create a copy of the image and flip it
                    flippedImageBitmap = new Bitmap(imageBitmap);
                    flippedImageBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }

            // Check the status of checkBoxShowFrame
            if (checkBoxShowFrame.Checked || imageBitmap == null)
            {
                // If checkBoxShowFrame is enabled or there is no image, restore the original image without transparency
                AnimationPictureBox.Image = boxes[currentIndex].Image;

                // Check whether the image exists before creating a bitmap
                if (boxes[currentIndex].Image != null)
                {
                    // Make a copy of the original image and mirror it
                    Bitmap originalFlippedImage = new Bitmap(boxes[currentIndex].Image);
                    originalFlippedImage.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Display the original mirrored image in your second PictureBox
                    AnimationPictureBox2.Image = originalFlippedImage;
                }
                else
                {
                    AnimationPictureBox2.Image = null;
                }

            }
            else
            {
                // If checkBoxShowFrame is disabled, use the transparent image
                AnimationPictureBox.Image = imageBitmap;
                AnimationPictureBox2.Image = flippedImageBitmap;
            }

            frameLabel.Text = (currentIndex + 1).ToString();
        }


        #endregion

        private void OnLoad(object sender, EventArgs e)
        {

        }

        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        #region LoadToolStripMenuItem
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Array of selectablePictureBox and CheckBoxes
            PictureBox[] boxes = { selectablePictureBox1, selectablePictureBox2, selectablePictureBox3, selectablePictureBox4, selectablePictureBox5, selectablePictureBox6, selectablePictureBox7, selectablePictureBox8, selectablePictureBox9, selectablePictureBox10 };
            CheckBox[] checks = { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10 };

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Displays the dialog and check if the user clicked OK
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Loads the image
                Image image = Image.FromFile(openFileDialog1.FileName);

                // Clear all PictureBoxes before loading the new image
                for (int i = 0; i < boxes.Length; i++)
                {
                    boxes[i].Image = null;
                    if (boxes[i] is SelectablePictureBox)
                    {
                        ((SelectablePictureBox)boxes[i]).ClearImage();
                    }
                }

                // Use a loop to load the image into the PictureBoxes,
                // if the corresponding checkbox is activated
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (checks[i].Checked)
                    {
                        boxes[i].Image = image;
                        if (boxes[i] is SelectablePictureBox)
                        {
                            SelectablePictureBox box = (SelectablePictureBox)boxes[i];
                            box.OriginalImage = new Bitmap(image); // Save the original image
                            box.DrawingBitmaps[box.CurrentIndex] = new Bitmap(image.Width, image.Height);
                        }
                    }
                }
            }
        }
        #endregion

        #region LoadToolStripMenuItem
        private void loadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Array of selectablePictureBox and CheckBoxes
            PictureBox[] boxes = { selectablePictureBox1, selectablePictureBox2, selectablePictureBox3, selectablePictureBox4, selectablePictureBox5, selectablePictureBox6, selectablePictureBox7, selectablePictureBox8, selectablePictureBox9, selectablePictureBox10 };
            CheckBox[] checks = { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10 };

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Displays the dialog and check if the user clicked OK
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Loads the image
                Image image = Image.FromFile(openFileDialog1.FileName);

                // Clear all PictureBoxes before loading the new image
                for (int i = 0; i < boxes.Length; i++)
                {
                    boxes[i].Image = null;
                    if (boxes[i] is SelectablePictureBox)
                    {
                        ((SelectablePictureBox)boxes[i]).ClearImage();
                    }
                }

                // Use a loop to load the image into the PictureBoxes,
                // if the corresponding checkbox is activated
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (checks[i].Checked)
                    {
                        boxes[i].Image = image;
                        if (boxes[i] is SelectablePictureBox)
                        {
                            SelectablePictureBox box = (SelectablePictureBox)boxes[i];
                            box.OriginalImage = new Bitmap(image); // Save the original image
                            box.DrawingBitmaps[box.CurrentIndex] = new Bitmap(image.Width, image.Height);
                        }
                    }
                }
            }
        }

        private void loadToolStripMenuItemAllSingle_Click(object sender, EventArgs e)
        {
            // Array of selectablePictureBox and CheckBoxes
            PictureBox[] boxes = { selectablePictureBox1, selectablePictureBox2, selectablePictureBox3, selectablePictureBox4, selectablePictureBox5, selectablePictureBox6, selectablePictureBox7, selectablePictureBox8, selectablePictureBox9, selectablePictureBox10 };
            CheckBox[] checks = { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10 };

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Use a loop to load a separate image into each PictureBox
            for (int i = 0; i < boxes.Length; i++)
            {
                // Check if the corresponding checkbox is checked
                if (checks[i].Checked)
                {
                    // Displays the dialog and check if the user clicked OK
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        // Loads the image
                        Image image = Image.FromFile(openFileDialog1.FileName);

                        // Clear the PictureBox before loading the new image
                        if (boxes[i] is SelectablePictureBox)
                        {
                            SelectablePictureBox box = (SelectablePictureBox)boxes[i];
                            box.ClearImage();
                        }

                        // Load the image into the PictureBox
                        boxes[i].Image = image;

                        if (boxes[i] is SelectablePictureBox)
                        {
                            SelectablePictureBox box = (SelectablePictureBox)boxes[i];
                            box.OriginalImage = new Bitmap(image); // Save the original image
                            box.DrawingBitmaps[box.CurrentIndex] = new Bitmap(image.Width, image.Height);
                        }
                    }
                    else
                    {
                        // If the user clicked Cancel in the dialog, break the loop
                        break;
                    }
                }
            }

        }

        private void loadToolStripMenuItemAllSingleMirror_Click(object sender, EventArgs e)
        {
            // Array of selectablePictureBox and CheckBoxes
            SelectablePictureBox[] boxes = { selectablePictureBox1, selectablePictureBox2, selectablePictureBox3, selectablePictureBox4, selectablePictureBox5, selectablePictureBox6, selectablePictureBox7, selectablePictureBox8, selectablePictureBox9, selectablePictureBox10 };
            CheckBox[] checks = { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10 };

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Use a loop to load a separate image into each PictureBox
            for (int i = 0; i < boxes.Length; i++)
            {
                // Check if the corresponding checkbox is checked
                if (checks[i].Checked)
                {
                    // Displays the dialog and check if the user clicked OK
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        // Loads the image
                        Image image = Image.FromFile(openFileDialog1.FileName);

                        // Clear the PictureBox before loading the new image
                        boxes[i].ClearImage();

                        // Load the image into the PictureBox
                        boxes[i].Image = image;
                        boxes[i].OriginalImage = new Bitmap(image); // Save the original image
                        boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(image.Width, image.Height);

                        // Mirror the image right after loading it
                        boxes[i].MirrorImage();
                    }
                    else
                    {
                        // If the user clicked Cancel in the dialog, break the loop
                        break;
                    }
                }
            }

        }

        // Load image mirror all
        private void loadOneImageAllMirrorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Array of selectablePictureBox and CheckBoxes
            SelectablePictureBox[] boxes = { selectablePictureBox1, selectablePictureBox2, selectablePictureBox3, selectablePictureBox4, selectablePictureBox5, selectablePictureBox6, selectablePictureBox7, selectablePictureBox8, selectablePictureBox9, selectablePictureBox10 };
            CheckBox[] checks = { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6, checkBox7, checkBox8, checkBox9, checkBox10 };

            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Displays the dialog and check if the user clicked OK
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Loads the image
                Image image = Image.FromFile(openFileDialog1.FileName);

                // Clear all PictureBoxes before loading the new image
                for (int i = 0; i < boxes.Length; i++)
                {
                    boxes[i].Image = null;
                    boxes[i].ClearImage();
                }

                // Use a loop to load the image into the PictureBoxes,
                // if the corresponding checkbox is activated
                for (int i = 0; i < boxes.Length; i++)
                {
                    if (checks[i].Checked)
                    {
                        boxes[i].Image = image;
                        boxes[i].OriginalImage = new Bitmap(image); // Save the original image
                        boxes[i].DrawingBitmaps[boxes[i].CurrentIndex] = new Bitmap(image.Width, image.Height);

                        // Mirror the image right after loading it                        
                        boxes[i].MirrorAllImages();
                    }
                }
            }
        }

        #endregion
        private void pictureBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        #region PictureBox_PrevieKeyDown
        private void pictureBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Cast the sender object into a SelectablePictureBox
                SelectablePictureBox currentBox = sender as SelectablePictureBox;

                if (Clipboard.ContainsImage())
                {
                    currentBox.Image = Clipboard.GetImage();
                }
                else
                {
                    MessageBox.Show("The clipboard does not contain an image.");
                }
            }
        }
        #endregion

        #region NumericUpDownFrameDelay
        private void numericUpDownFrameDelay_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDownFrameDelay.Value > 0)
            {
                timer.Interval = 1000 / (int)numericUpDownFrameDelay.Value;
                delayLabel.Text = $"Processing speed: {timer.Interval} ms"; // Ads in label

                // Restart the timer if it's enabled
                if (timer.Enabled)
                {
                    timer.Stop();
                    timer.Start();
                }
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
            // Start or stop the slideshow
            if (timer.Enabled)
            {
                timer.Stop();
            }
            else
            {
                if (numericUpDownFrameDelay.Value > 0)
                {
                    timer.Interval = 1000 / (int)numericUpDownFrameDelay.Value;
                    // Setzen Sie currentIndex auf den Wert von numericUpDownStartDelay
                    currentIndex = (int)numericUpDownStartDelay.Value;
                    timer.Start();
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

        #region SelectColor

        private void SelectColor()
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Color color = colorDialog.Color;
                tboxColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                foreach (SelectablePictureBox box in boxes)
                {
                    box.SetDrawingColor(color);
                }
            }
        }
        #endregion

        #region UdateColor
        private void UpdateColor()
        {
            string code = tboxColorCode.Text;

            if (code.StartsWith("#"))
                code = code.Substring(1);

            if (code.Length == 6 && int.TryParse(code, System.Globalization.NumberStyles.HexNumber, null, out int number))
            {
                Color color = Color.FromArgb(number >> 16, (number >> 8) & 255, number & 255);
                foreach (SelectablePictureBox box in boxes)
                {
                    box.SetDrawingColor(color);
                }
            }
        }
        #endregion

        #region Draw all
        private void btDrawAllSelectableBox_Click(object sender, EventArgs e)
        {
            bool isActive = false;
            foreach (SelectablePictureBox box in boxes)
            {
                box.ToggleDrawing(); // Toggles the drawing mode for each box
                if (box.IsDrawing()) // Checks whether drawing mode is active
                {
                    isActive = true;
                }
            }

            if (isActive)
            {
                btDrawAllSelectableBox.BackColor = Color.Green; // Sets the background color of the button to green
                labelDrawAll.Text = "Active"; // Sets the text of the label to "Active"
            }
            else
            {
                btDrawAllSelectableBox.BackColor = SystemColors.Control; // Sets the background color of the button to the default color
                labelDrawAll.Text = "Not active"; // Sets the text of the label to "Not active"
            }
        }
        #endregion

        #region BrushSize
        private void numericUpDownBrushSize_ValueChanged(object sender, EventArgs e)
        {
            int size = (int)numericUpDownBrushSize.Value;
            foreach (SelectablePictureBox box in boxes)
            {
                box.SetBrushSize(size);
            }
        }
        #endregion

        #region Combobox Images
        private void comboBoxImageBackgrund_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxImageBackgrund.SelectedIndex != -1)
            {
                string selectedItem = comboBoxImageBackgrund.SelectedItem.ToString();

                if (selectedItem == "Green")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Green2;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Green2;
                }
                else if (selectedItem == "Water")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.water2;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.water2;
                }
                else if (selectedItem == "Sand")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Sand;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Sand;
                }
                else if (selectedItem == "Street")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Street;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Street;
                }
                else if (selectedItem == "Forest")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Forest;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Forest;
                }
                else if (selectedItem == "Dirt")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dirt;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dirt;
                }
                else if (selectedItem == "Dungeon")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dungeon;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.Dungeon;
                }
                else if (selectedItem == "Cave")
                {
                    AnimationPictureBox.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.cave;
                    AnimationPictureBox2.BackgroundImage = UoFiddler.Plugin.ConverterMultiTextPlugin.Properties.Resources.cave;
                }
                else if (selectedItem == "Clear")
                {
                    AnimationPictureBox.BackgroundImage = null;
                    AnimationPictureBox2.BackgroundImage = null;
                }
            }
        }
        #endregion

        #region NumericUpDownImageShow       

        private void numericUpDownImageShow_ValueChanged(object sender, EventArgs e)
        {
            // Get the index of the current image from numericUpDownImageShow
            int index = (int)numericUpDownImageShow.Value - 1;

            // Check whether the index is valid
            if (index >= 0 && index < boxes.Length)
            {
                // Check if the image exists in the PictureBox
                if (boxes[index].Image != null)
                {
                    // Get the image from the corresponding selectablePictureBox
                    Bitmap imageBitmap = new Bitmap(boxes[index].Image);

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
                        imageBitmap = MakeTransparent(imageBitmap, color);
                    }

                    // Set the transparent bitmap as the image for the PictureBox controls
                    AnimationPictureBox.Image = imageBitmap;

                    // Make a copy of the image and flip it
                    Bitmap flippedImageBitmap = new Bitmap(imageBitmap);
                    flippedImageBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    // Set the mirrored bitmap as the image for the PictureBox controls
                    AnimationPictureBox2.Image = flippedImageBitmap;
                }
                else
                {
                    // If there is no image in the PictureBox, set the images in the PictureBox controls to null
                    AnimationPictureBox.Image = null;
                    AnimationPictureBox2.Image = null;
                }
            }
            else
            {
                // If the index is invalid (e.g. 0), delete the images from the PictureBox controls
                AnimationPictureBox.Image = null;
                AnimationPictureBox2.Image = null;
            }
        }

        #endregion

        #region Start Contextmenu
        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Start or stop the slideshow
            if (timer.Enabled)
            {
                timer.Stop();
            }
            else
            {
                if (numericUpDownFrameDelay.Value > 0)
                {
                    timer.Interval = 1000 / (int)numericUpDownFrameDelay.Value;
                    // Set currentIndex to the value of numericUpDownStartDelay
                    currentIndex = (int)numericUpDownStartDelay.Value;
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Frame delay must be greater than zero.");
                }
            }
        }
        #endregion

        #region SelevtabePictureBox_MouseClick 

        private void selectablePictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (sender is SelectablePictureBox box)
            {
                if (pipetteMode)
                {
                    // Get the color at the position where the user clicked
                    Color color = box.GetPixelColor(e.Location);

                    // Put the hexadecimal value of the color in tboxColorCode
                    tboxColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

                    // Exit pipette mode
                    pipetteMode = false;

                    // Reset the color of the button to the default color
                    btPipette.BackColor = SystemColors.Control;
                }
                else
                {
                    // Run your normal MouseClick code here...
                }
            }
        }
        #endregion

        #region Pipette
        private void btPipette_Click(object sender, EventArgs e)
        {
            // Switch to eyedropper mode when the button is clicked
            pipetteMode = !pipetteMode;

            if (pipetteMode)
            {
                // Color the button green when pipette mode is active
                btPipette.BackColor = Color.Green;
            }
            else
            {
                // Reset the button color to the default color when eyedropper mode is finished
                btPipette.BackColor = SystemColors.Control;
            }
        }
        #endregion

        #region SelebtabePictureBoxMouseMove
        private void selectablePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is SelectablePictureBox box)
            {
                // Get the color at the position where the mouse is currently
                Color color = box.GetPixelColor(e.Location);

                // Put the hexadecimal value of the color in panel color code
                panelFarbcode.BackColor = color;

                // Convert the color to a hexadecimal code and display it in the label
                lbColorCode.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }
        #endregion

        #region TextBox Color
        private void tboxColorCode_TextChanged(object sender, EventArgs e)
        {
            string colorCode = tboxColorCode.Text;

            // If the # is missing, add it
            if (!colorCode.StartsWith("#"))
            {
                colorCode = "#" + colorCode;
            }

            // Check whether the text in colorCode is a valid hexadecimal color value
            if (Regex.IsMatch(colorCode, "^#[0-9A-Fa-f]{6}$"))
            {
                // Convert the hexadecimal color value to a Color object
                Color color = ColorTranslator.FromHtml(colorCode);

                // Set the background color of panelColorCodeTB to the selected color
                panelFarbCodeTB.BackColor = color;
            }
        }
        #endregion

        #region Zoom        

        private void ZoomIn(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is SelectablePictureBox pictureBox)
            {
                if (pictureBox == selectablePictureBox1)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel1, true);
                }
                else if (pictureBox == selectablePictureBox2)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel2, true);
                }
                else if (pictureBox == selectablePictureBox3)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel3, true);
                }
                else if (pictureBox == selectablePictureBox4)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel4, true);
                }
                else if (pictureBox == selectablePictureBox5)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel5, true);
                }
                else if (pictureBox == selectablePictureBox6)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel6, true);
                }
                else if (pictureBox == selectablePictureBox7)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel7, true);
                }
                else if (pictureBox == selectablePictureBox8)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel8, true);
                }
                else if (pictureBox == selectablePictureBox9)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel9, true);
                }
                else if (pictureBox == selectablePictureBox10)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel10, true);
                }
            }
        }

        private void ZoomOut(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is SelectablePictureBox pictureBox)
            {
                if (pictureBox == selectablePictureBox1)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel1, false);
                }
                else if (pictureBox == selectablePictureBox2)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel2, false);
                }
                else if (pictureBox == selectablePictureBox3)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel3, false);
                }
                else if (pictureBox == selectablePictureBox4)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel4, false);
                }
                else if (pictureBox == selectablePictureBox5)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel5, false);
                }
                else if (pictureBox == selectablePictureBox6)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel6, false);
                }
                else if (pictureBox == selectablePictureBox7)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel7, false);
                }
                else if (pictureBox == selectablePictureBox8)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel8, false);
                }
                else if (pictureBox == selectablePictureBox9)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel9, false);
                }
                else if (pictureBox == selectablePictureBox10)
                {
                    ChangeZoomLevel(pictureBox, ref zoomLevel10, false);
                }
            }
        }

        private void ChangeZoomLevel(SelectablePictureBox pictureBox, ref int zoomLevel, bool zoomIn)
        {
            if (pictureBox.Image != null)
            {
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
        }
        #endregion

        #region Background Image Loader
        private void imageFadeinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Properties of the OpenFileDialog
            openFileDialog1.Filter = "Image files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            openFileDialog1.Title = "Please select an image file.";

            // Display the dialog and verify that the user clicked OK
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Load the image
                Image image = Image.FromFile(openFileDialog1.FileName);

                // Adjust the size of the image to the size of the PictureBox
                image = new Bitmap(image, new Size(176, 238));

                // Add the image to the AnimationPictureBox as a background image
                AnimationPictureBox.BackgroundImage = image;

                // Check whether the ShowAnimationPictureBox2 checkbox is checked
                if (ShowAnimationPictureBox2.Checked)
                {
                    // If the checkbox is checked, also add the image to the AnimationPictureBox2 as a background image
                    AnimationPictureBox2.BackgroundImage = image;
                }
            }
        }
        #endregion

        #region ShowAnimationPictureBox2 
        private void ShowAnimationPictureBox2_CheckedChanged(object sender, EventArgs e)
        {
            // Check whether the checkbox is activated
            if (ShowAnimationPictureBox2.Checked)
            {
                // If the checkbox is checked, set the background image of AnimationPictureBox2 to the same image as AnimationPictureBox
                AnimationPictureBox2.BackgroundImage = AnimationPictureBox.BackgroundImage;
            }
            else
            {
                // If the checkbox is unchecked, remove the background image of AnimationPictureBox2
                AnimationPictureBox2.BackgroundImage = null;
            }
        }
        #endregion

        #region clipbordAllSingleToolStripMenuItem
        private void clipbordAllSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                // Load the image from the cache
                Image image = Clipboard.GetImage();

                // Generate a unique name for the image
                string imageName = "Item" + imageCounter.ToString("00");

                // Save a copy of the original image
                originalImages[imageName] = (Image)image.Clone();

                // If the CheckBoxTransparent is activated, make the colors #000000 and #ffffff transparent
                if (CheckBoxTransparent.Checked)
                {
                    image = MakeColorTransparent(image, Color.FromArgb(0, 0, 0)); // Black
                    image = MakeColorTransparent(image, Color.FromArgb(255, 255, 255)); // White
                }

                // Add the image to the dictionary
                images[imageName] = image;

                // Add the image to the PictureBox and update it immediately
                PictureBoxAll.Image = image;
                PictureBoxAll.Refresh();

                // Add an entry to the ListBox
                ListBoxAll.Items.Add(imageName);

                // Increase the counter for the next image
                imageCounter++;

                // Make sure not to add more than 30 images
                if (imageCounter > 30)
                {
                    imageCounter = 30;
                    MessageBox.Show("Maximum number of images reached.");
                }
            }
            else
            {
                MessageBox.Show("The cache does not contain an image.");
            }
        }
        #endregion

        #region listBoxAll_SelectedIndexChanged
        private void listBoxAll_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem != null)
            {
                string selectedItem = ListBoxAll.SelectedItem.ToString();
                if (images.ContainsKey(selectedItem))
                {
                    PictureBoxAll.Image = images[selectedItem];
                    PictureBoxAll.Refresh();
                }
                else
                {
                    MessageBox.Show("The selected image does not exist in the dictionary.");
                }
            }
            else
            {
                MessageBox.Show("No image selected.");
            }
        }
        #endregion

        #region CheckBoxTransparent
        private void CheckBoxTransparent_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var key in images.Keys.ToList())
            {
                Image image;

                if (CheckBoxTransparent.Checked)
                {
                    image = MakeColorTransparent(originalImages[key], Color.FromArgb(0, 0, 0)); // Black
                    image = MakeColorTransparent(image, Color.FromArgb(255, 255, 255)); // White
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
        #endregion

        #region MakeColorTransparent
        private Image MakeColorTransparent(Image image, Color color)
        {
            Bitmap bitmap = new Bitmap(image);
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    Color pixelColor = bitmap.GetPixel(i, j);
                    if (pixelColor.ToArgb() == color.ToArgb())
                    {
                        bitmap.SetPixel(i, j, Color.Transparent);
                    }
                }
            }
            return bitmap;
        }
        #endregion

        #region deleteToolStripMenuItem
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem != null)
            {
                string selectedItem = ListBoxAll.SelectedItem.ToString();

                // Remove the entry from the Dictionary objects
                if (originalImages.ContainsKey(selectedItem))
                {
                    originalImages.Remove(selectedItem);
                }
                if (images.ContainsKey(selectedItem))
                {
                    images.Remove(selectedItem);
                }

                // Remove the entry from the ListBox
                ListBoxAll.Items.Remove(selectedItem);

                // Delete the image in the PictureBox if it is displayed
                if (PictureBoxAll.Image != null && images.ContainsKey(selectedItem) && PictureBoxAll.Image == images[selectedItem])
                {
                    PictureBoxAll.Image = null;
                }
            }
            else
            {
                MessageBox.Show("No image selected for deletion.");
            }
        }
        #endregion

        #region deleteAllToolStripMenuItem
        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Remove all entries from the Dictionary objects
            originalImages.Clear();
            images.Clear();

            // Remove all entries from the ListBox
            ListBoxAll.Items.Clear();

            // Delete the picture in the PictureBox
            PictureBoxAll.Image = null;
        }
        #endregion

        #region playToolStripMenuItem
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
        #endregion

        #region PlayTimer_Tick 
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (currentImageIndex >= ListBoxAll.Items.Count)
            {
                currentImageIndex = 0;
            }

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
                MessageBox.Show("No images available to play.");
            }
        }
        #endregion

        #region stopToolStripMenuItem
        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playTimer.Stop();
        }
        #endregion

        #region exchangeToolStripMenuItem
        private void exchangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListBoxAll.SelectedItem != null && Clipboard.ContainsImage())
            {
                string selectedItem = ListBoxAll.SelectedItem.ToString();
                Image newImage = Clipboard.GetImage();

                // Save a copy of the new image
                originalImages[selectedItem] = (Image)newImage.Clone();

                // If the CheckBoxTransparent is activated, make the colors #000000 and #ffffff transparent
                if (CheckBoxTransparent.Checked)
                {
                    newImage = MakeColorTransparent(newImage, Color.FromArgb(0, 0, 0)); // Black
                    newImage = MakeColorTransparent(newImage, Color.FromArgb(255, 255, 255)); // White
                }

                // Replace the picture in the dictionary
                images[selectedItem] = newImage;

                // Refresh the PictureBox if the selected image is displayed
                if (PictureBoxAll.Image != null && images.ContainsKey(selectedItem) && PictureBoxAll.Image == images[selectedItem])
                {
                    PictureBoxAll.Image = newImage;
                    PictureBoxAll.Refresh();
                }
            }
            else
            {
                MessageBox.Show("No image selected or no image in buffer.");
            }
        }
        #endregion
    }
}

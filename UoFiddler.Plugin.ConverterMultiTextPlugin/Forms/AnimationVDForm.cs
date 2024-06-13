// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * "THE WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AnimationVDForm : Form
    {
        private List<Bitmap> frames;
        private int currentFrameIndex;
        private Timer animationTimer;
        private ListView listViewVD;
        private TreeView animationTreeView;
        private PictureBox pictureBox;
        private TextBox textBoxInfo;

        public AnimationVDForm()
        {
            InitializeComponent();
            frames = new List<Bitmap>();
            animationTimer = new Timer();
            animationTimer.Interval = 100; // Animation speed in milliseconds
            animationTimer.Tick += AnimationTimer_Tick;

            // Add a button to load .vd files
            Button loadButton = new Button
            {
                Text = "Load .vd File",
                Location = new Point(10, 10)
            };
            loadButton.Click += LoadButton_Click;
            Controls.Add(loadButton);

            // Add a ListView to display frames
            listViewVD = new ListView
            {
                Name = "listViewVD",
                Location = new Point(10, 50),
                Size = new Size(200, 300),
                View = View.List
            };
            listViewVD.SelectedIndexChanged += ListViewVD_SelectedIndexChanged;
            Controls.Add(listViewVD);

            // Add a TreeView to display animations
            animationTreeView = new TreeView
            {
                Name = "animationTreeView",
                Location = new Point(220, 50),
                Size = new Size(200, 300)
            };
            animationTreeView.AfterSelect += AnimationTreeView_AfterSelect;
            Controls.Add(animationTreeView);

            // Add a PictureBox to display the selected frame
            pictureBox = new PictureBox
            {
                Name = "pictureBox",
                Location = new Point(440, 50),
                Size = new Size(300, 300),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(pictureBox);

            // Add a TextBox to display information
            textBoxInfo = new TextBox
            {
                Name = "textBoxInfo",
                Location = new Point(10, 360),
                Size = new Size(730, 50),
                Multiline = true,
                ReadOnly = true
            };
            Controls.Add(textBoxInfo);

            // Add a button to save frames
            Button saveButton = new Button
            {
                Text = "Save Frames",
                Location = new Point(120, 10)
            };
            saveButton.Click += SaveButton_Click;
            Controls.Add(saveButton);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "VD files (*.vd)|*.vd|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadVDFile(openFileDialog.FileName);
                }
            }
        }

        private void LoadVDFile(string filePath)
        {
            int width = 0;
            int height = 0;
            frames.Clear();
            listViewVD.Items.Clear();
            animationTreeView.Nodes.Clear();
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    // Read the header of the VD file
                    if (reader.BaseStream.Length < sizeof(int))
                    {
                        throw new EndOfStreamException("Unexpected end of file while reading frame count.");
                    }

                    int frameCount = reader.ReadInt32();
                    for (int i = 0; i < frameCount; i++)
                    {
                        if (reader.BaseStream.Length - reader.BaseStream.Position < sizeof(int) * 2)
                        {
                            throw new EndOfStreamException("Unexpected end of file while reading frame dimensions.");
                        }

                        width = reader.ReadInt32();
                        height = reader.ReadInt32();

                        if (reader.BaseStream.Length - reader.BaseStream.Position < width * height)
                        {
                            throw new EndOfStreamException("Unexpected end of file while reading frame data.");
                        }

                        byte[] imageData = reader.ReadBytes(width * height);
                        Bitmap frame = new Bitmap(width, height);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                byte pixelValue = imageData[y * width + x];
                                frame.SetPixel(x, y, Color.FromArgb(pixelValue, pixelValue, pixelValue));
                            }
                        }
                        frames.Add(frame);
                        listViewVD.Items.Add($"Frame {i + 1}");
                        animationTreeView.Nodes.Add($"Frame {i + 1}");
                    }
                    DisplayAnimationInfo(frameCount, width, height);
                }
            }
            catch (EndOfStreamException ex)
            {
                MessageBox.Show($"File format error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading VD file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListViewVD_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView.SelectedIndices.Count > 0 && listView.SelectedIndices[0] < frames.Count)
            {
                pictureBox.Image = frames[listView.SelectedIndices[0]];
            }
        }

        private void AnimationTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            int selectedIndex = e.Node.Index;
            if (selectedIndex != -1 && selectedIndex < frames.Count)
            {
                pictureBox.Image = frames[selectedIndex];
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (frames.Count > 0)
            {
                currentFrameIndex = (currentFrameIndex + 1) % frames.Count;
                pictureBox.Image = frames[currentFrameIndex];
            }
        }

        private void DisplayAnimationInfo(int frameCount, int width, int height)
        {
            textBoxInfo.Text = $"Frames: {frameCount}\nWidth: {width}\nHeight: {height}";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    for (int i = 0; i < frames.Count; i++)
                    {
                        frames[i].Save(Path.Combine(folderPath, $"frame_{i}.png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                    MessageBox.Show("Frames saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}

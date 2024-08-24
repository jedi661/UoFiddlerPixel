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
using System.Threading;
using System.Threading.Tasks;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class AnimationVDForm : Form
    {
        private List<Image> images = new List<Image>(10);
        private int animationSpeed = 500; // Default speed in milliseconds
        private CancellationTokenSource cancellationTokenSource;
        private bool isPlaying = false; // Flag to determine if the animation is active

        public AnimationVDForm()
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                images.Add(null); // Initialize the list with null values
            }

            checkedListBoxAminID.SetItemChecked(0, true); //Activate the first checkbox at startup

            // Set TrackBar to default value (middle)
            trackBarSpeedAmin.Value = 3;

            // Initialize the label at the default speed
            UpdateLabelSpeed();

            // Subscribe to event handler
            trackBarSpeedAmin.Scroll += TrackBarSpeedAmin_Scroll;
            checkedListBoxAminID.ItemCheck += CheckedListBoxAminID_ItemCheck; // Event handler for CheckedListBox          
        }

        #region [ CheckedListBoxAminID_ItemCheck ]
        private void CheckedListBoxAminID_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Only show the image when the animation is not running
            if (!isPlaying && e.NewValue == CheckState.Checked)
            {
                // Uncheck all other checkboxes
                for (int i = 0; i < checkedListBoxAminID.Items.Count; i++)
                {
                    if (i != e.Index)
                    {
                        checkedListBoxAminID.SetItemChecked(i, false);
                    }
                }

                var selectedIndex = e.Index;

                if (selectedIndex >= 0 && selectedIndex < images.Count && images[selectedIndex] != null)
                {
                    pictureBoxAminImage.Image = images[selectedIndex];
                    pictureBoxAminImage.Refresh();
                }
                else
                {
                    pictureBoxAminImage.Image = null;
                }
            }
            else if (isPlaying)
            {
                e.NewValue = e.CurrentValue; // Prevents changes when the animation is running
            }
        }
        #endregion

        #region [ btLoadAminID ]
        private void btLoadAminID_Click(object sender, EventArgs e)
        {
            if (checkedListBoxAminID.SelectedIndex == -1)
            {
                checkedListBoxAminID.SelectedIndex = 0; // Set the SelectedIndex to 0 if none is selected
            }

            if (checkedListBoxAminID.SelectedIndex >= 0 && checkedListBoxAminID.SelectedIndex < 10)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Image img = Image.FromFile(openFileDialog.FileName);
                    images[checkedListBoxAminID.SelectedIndex] = img;

                    // Show the loaded image immediately if the animation is not active
                    if (!isPlaying)
                    {
                        pictureBoxAminImage.Image = img;
                        pictureBoxAminImage.Refresh();
                    }
                }
            }
        }
        #endregion

        #region [ btPlayAminID ]
        private async void btPlayAminID_Click(object sender, EventArgs e)
        {
            isPlaying = true; // Set the flag that the animation is running
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            try
            {
                await Task.Run(async () =>
                {
                    do
                    {
                        foreach (var img in images)
                        {
                            if (img != null)
                            {
                                // Verify that the PictureBox is still valid before attempting to update it
                                if (pictureBoxAminImage != null && !pictureBoxAminImage.IsDisposed && pictureBoxAminImage.IsHandleCreated)
                                {
                                    pictureBoxAminImage.Invoke(new Action(() =>
                                    {
                                        if (!pictureBoxAminImage.IsDisposed) // Check again
                                        {
                                            pictureBoxAminImage.Image = img;
                                            pictureBoxAminImage.Refresh();
                                        }
                                    }));
                                }
                                await Task.Delay(animationSpeed, token); // Waiting time between pictures
                            }
                        }
                    } while (checkBoxLoop.Checked && !token.IsCancellationRequested);
                }, token);
            }
            catch (TaskCanceledException)
            {
                // Task canceled, no action required
            }
            finally
            {
                isPlaying = false; // Animation ended
            }
        }
        #endregion

        #region [ TrackBarSpeedAmin_Scroll ]
        private void TrackBarSpeedAmin_Scroll(object sender, EventArgs e)
        {
            // Calculation of speed based on the position of the TrackBar
            switch (trackBarSpeedAmin.Value)
            {
                case 1:
                    animationSpeed = 2000; // Slowest speed
                    break;
                case 2:
                    animationSpeed = 1000; // Slow
                    break;
                case 3:
                    animationSpeed = 500; // Standard speed
                    break;
                case 4:
                    animationSpeed = 100; // Faster
                    break;
                case 5:
                    animationSpeed = 25; // Fastest speed
                    break;
                default:
                    animationSpeed = 500; // Standard speed
                    break;
            }

            // Update the label with the current speed
            UpdateLabelSpeed();
        }
        #endregion

        #region [ UpdateLabelSpeed ]
        private void UpdateLabelSpeed()
        {
            labelSpeed.Text = $"Speed: {animationSpeed} ms";
        }
        #endregion

        #region [ btStopAminID ]
        private void btStopAminID_Click(object sender, EventArgs e)
        {
            StopAnimation(); // Stops the animation safely
        }
        #endregion

        #region [ AnimationVDForm_FormClosing ]
        private async void AnimationVDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the animation is running, stop it first
            if (isPlaying)
            {
                e.Cancel = true; // Cancel close
                await Task.Run(() => StopAnimation()); // Stop animation
                this.Close(); // Close the mold again
            }
        }
        #endregion

        #region [ StopAnimation ]
        private void StopAnimation()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                isPlaying = false;
            }
        }
        #endregion

        #region [ importImageToolStripMenuItem ]
        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image img = Clipboard.GetImage();

                if (checkedListBoxAminID.SelectedIndex >= 0 && checkedListBoxAminID.SelectedIndex < images.Count)
                {
                    images[checkedListBoxAminID.SelectedIndex] = img;

                    // Show the loaded image immediately if the animation is not active
                    if (!isPlaying)
                    {
                        pictureBoxAminImage.Image = img;
                        pictureBoxAminImage.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a valid checkbox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No image found on clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}

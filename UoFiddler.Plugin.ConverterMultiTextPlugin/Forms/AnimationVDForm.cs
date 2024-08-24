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
        private int animationSpeed = 500; // Standardgeschwindigkeit in Millisekunden
        private CancellationTokenSource cancellationTokenSource;
        private bool isPlaying = false; // Flag, um festzustellen, ob die Animation aktiv ist

        public AnimationVDForm()
        {
            InitializeComponent();
            for (int i = 0; i < 10; i++)
            {
                images.Add(null); // Initialisiere die Liste mit null-Werten
            }

            checkedListBoxAminID.SetItemChecked(0, true); // Erste Checkbox beim Start aktivieren

            // Setze TrackBar auf den Standardwert (Mitte)
            trackBarSpeedAmin.Value = 3;

            // Initialisiere das Label mit der Standardgeschwindigkeit
            UpdateLabelSpeed();

            // Event-Handler abonnieren
            trackBarSpeedAmin.Scroll += TrackBarSpeedAmin_Scroll;
            checkedListBoxAminID.ItemCheck += CheckedListBoxAminID_ItemCheck; // Event-Handler für CheckedListBox            
        }

        private void CheckedListBoxAminID_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Zeige das Bild nur an, wenn die Animation nicht läuft
            if (!isPlaying && e.NewValue == CheckState.Checked)
            {
                // Deaktiviere alle anderen Checkboxen
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
                e.NewValue = e.CurrentValue; // Verhindert Änderungen, wenn die Animation läuft
            }
        }

        private void btLoadAminID_Click(object sender, EventArgs e)
        {
            if (checkedListBoxAminID.SelectedIndex == -1)
            {
                checkedListBoxAminID.SelectedIndex = 0; // Setze den SelectedIndex auf 0, falls keiner ausgewählt ist
            }

            if (checkedListBoxAminID.SelectedIndex >= 0 && checkedListBoxAminID.SelectedIndex < 10)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Image img = Image.FromFile(openFileDialog.FileName);
                    images[checkedListBoxAminID.SelectedIndex] = img;

                    // Zeige das geladene Bild sofort an, wenn die Animation nicht aktiv ist
                    if (!isPlaying)
                    {
                        pictureBoxAminImage.Image = img;
                        pictureBoxAminImage.Refresh();
                    }
                }
            }
        }

        private async void btPlayAminID_Click(object sender, EventArgs e)
        {
            isPlaying = true; // Setze das Flag, dass die Animation läuft
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
                                // Überprüfen, ob die PictureBox noch gültig ist, bevor versucht wird, sie zu aktualisieren
                                if (pictureBoxAminImage != null && !pictureBoxAminImage.IsDisposed && pictureBoxAminImage.IsHandleCreated)
                                {
                                    pictureBoxAminImage.Invoke(new Action(() =>
                                    {
                                        if (!pictureBoxAminImage.IsDisposed) // Nochmalige Prüfung
                                        {
                                            pictureBoxAminImage.Image = img;
                                            pictureBoxAminImage.Refresh();
                                        }
                                    }));
                                }
                                await Task.Delay(animationSpeed, token); // Wartezeit zwischen den Bildern
                            }
                        }
                    } while (checkBoxLoop.Checked && !token.IsCancellationRequested);
                }, token);
            }
            catch (TaskCanceledException)
            {
                // Aufgabe wurde abgebrochen, keine Aktion erforderlich
            }
            finally
            {
                isPlaying = false; // Animation beendet
            }
        }

        private void TrackBarSpeedAmin_Scroll(object sender, EventArgs e)
        {
            // Berechnung der Geschwindigkeit basierend auf der Position der TrackBar
            switch (trackBarSpeedAmin.Value)
            {
                case 1:
                    animationSpeed = 2000; // Langsamste Geschwindigkeit
                    break;
                case 2:
                    animationSpeed = 1000; // Langsam
                    break;
                case 3:
                    animationSpeed = 500; // Standardgeschwindigkeit
                    break;
                case 4:
                    animationSpeed = 100; // Schneller
                    break;
                case 5:
                    animationSpeed = 25; // Schnellste Geschwindigkeit
                    break;
                default:
                    animationSpeed = 500; // Standardgeschwindigkeit
                    break;
            }

            // Aktualisiere das Label mit der aktuellen Geschwindigkeit
            UpdateLabelSpeed();
        }

        private void UpdateLabelSpeed()
        {
            labelSpeed.Text = $"Speed: {animationSpeed} ms";
        }

        private void btStopAminID_Click(object sender, EventArgs e)
        {
            StopAnimation(); // Stoppt die Animation sicher
        }

        private async void AnimationVDForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Wenn die Animation läuft, zuerst stoppen
            if (isPlaying)
            {
                e.Cancel = true; // Schließen abbrechen
                await Task.Run(() => StopAnimation()); // Animation stoppen
                this.Close(); // Form erneut schließen
            }
        }

        private void StopAnimation()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                isPlaying = false;
            }
        }

        private void importImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                Image img = Clipboard.GetImage();

                if (checkedListBoxAminID.SelectedIndex >= 0 && checkedListBoxAminID.SelectedIndex < images.Count)
                {
                    images[checkedListBoxAminID.SelectedIndex] = img;

                    // Zeige das geladene Bild sofort an, wenn die Animation nicht aktiv ist
                    if (!isPlaying)
                    {
                        pictureBoxAminImage.Image = img;
                        pictureBoxAminImage.Refresh();
                    }
                }
                else
                {
                    MessageBox.Show("Bitte wählen Sie eine gültige Checkbox aus.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Kein Bild in der Zwischenablage gefunden.", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

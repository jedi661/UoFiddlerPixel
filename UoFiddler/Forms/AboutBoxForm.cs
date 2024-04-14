/***************************************************************************
 *
 * $Author: Turley
 * Advanced Nikodemus
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using System.Drawing;

namespace UoFiddler.Forms
{
    public partial class AboutBoxForm : Form
    {        
        private Timer animationTimer;
        private Random random;
        private List<Label> labels;
        private List<string> specialWords = new List<string>
        {
            "Code", "Nikodemus", "Matrix", "Ultima", "Turley", "Ares", "AsYlum", "MuadDib", "Nibbio", "Soulblighter", "Andreew", "Online"
        };

        #region AboutBoxForm
        public AboutBoxForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Enable double buffering
            Icon = Options.GetFiddlerIcon();
            checkBoxCheckOnStart.Checked = FiddlerOptions.UpdateCheckOnStart;
            checkBoxFormState.Checked = FiddlerOptions.StoreFormState;
            labels = new List<Label>();
            this.Load += AboutBoxForm_Load;
        }
        #endregion

        #region AboutBoxForm_Load
        private void AboutBoxForm_Load(object sender, EventArgs e)
        {
            InitializeAnimation();
        }
        #endregion

        #region InitializeAnimation()
        private void InitializeAnimation()
        {
            animationTimer = new Timer
            {
                Interval = 50
            };
            
            random = new Random();
            
            for (int i = 0; i < 100; i++)
            {
                var label = new Label
                {
                    ForeColor = Color.Green,
                    Font = new Font("Courier New", 14, FontStyle.Bold),
                    Text = GetRandomCharacter(),
                    Location = new Point(random.Next(animationPanel.Width), random.Next(animationPanel.Height))
                };
                labels.Add(label);
                animationPanel.Controls.Add(label);
            }
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }
        #endregion

        #region GetRandomCharacter()
        private string GetRandomCharacter()
        {
            // Occasionally return a special word
            if (random.Next(200) == 0) // Increase this number to decrease the frequency of the special words
            {
                return specialWords[random.Next(specialWords.Count)];
            }

            // Otherwise return a random character
            return ((char)random.Next(33, 127)).ToString();
        }
        #endregion

        #region AnimationTimer_Tick
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            foreach (var label in labels)
            {
                label.Top += 10;
                
                if (label.Top > animationPanel.Height)
                {
                    label.Top = 0;
                    label.Left = random.Next(animationPanel.Width);
                    label.Text = GetRandomCharacter();
                }
            }
        }
        #endregion

        #region OnChangeCheck
        private void OnChangeCheck(object sender, EventArgs e)
        {
            FiddlerOptions.UpdateCheckOnStart = checkBoxCheckOnStart.Checked;
        }
        #endregion

        #region OnClickUpdate
        private async void OnClickUpdate(object sender, EventArgs e)
        {
            await UpdateRunner.RunAsync(FiddlerOptions.RepositoryOwner, FiddlerOptions.RepositoryName, FiddlerOptions.AppVersion).ConfigureAwait(false);
        }
        #endregion

        #region OnClickLink Old version 4.8
        private void OnClickLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://uofiddler.polserver.com/",
                UseShellExecute = true
            });
        }
        #endregion

        #region OnChangeFormState
        private void OnChangeFormState(object sender, EventArgs e)
        {
            FiddlerOptions.StoreFormState = checkBoxFormState.Checked;
        }
        #endregion

        #region linkLabel2
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/jedi661/UoFiddlerPixel/",
                UseShellExecute = true
            });
        }
        #endregion

        #region ShowRepoInfoButton
        private void ShowRepoInfoButton_Click(object sender, EventArgs e)
        {
            string repoOwner = FiddlerOptions.RepositoryOwner;
            string repoName = FiddlerOptions.RepositoryName;

            MessageBox.Show($"Repository Owner: {repoOwner}\nRepository Name: {repoName}", "Repository Information"); // from FiddlerOptions
        }
        #endregion
    }
}

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
using System.Diagnostics;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class AboutBoxForm : Form
    {
        #region AboutBoxForm
        public AboutBoxForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            checkBoxCheckOnStart.Checked = FiddlerOptions.UpdateCheckOnStart;
            checkBoxFormState.Checked = FiddlerOptions.StoreFormState;
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

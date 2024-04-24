// /***************************************************************************
//  *
//  * $Author: Nikodemus
//  * 
//  * 
//  * "THE BEER-WINE-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer and Wine in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace UoFiddler.Plugin.ConverterMultiTextPlugin.Forms
{
    public partial class ServerStartBox : Form
    {
        public ServerStartBox()
        {
            InitializeComponent();

            this.Load += ServerStartBox_Load;
        }

        #region ServerStartBox_Load
        private void ServerStartBox_Load(object sender, EventArgs e)
        {
            // Load the saved directory when starting the application
            tbServuoDir.Text = Properties.Settings.Default.ServuoDir;
        }
        #endregion

        #region btSetDirectoryServuo
        private void btSetDirectoryServuo_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the selected directory in tbServuoDir and settings
                    tbServuoDir.Text = dialog.SelectedPath;
                    Properties.Settings.Default.ServuoDir = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }
        #endregion

        #region btStartServuo
        private void btStartServuo_Click(object sender, EventArgs e)
        {
            // Make sure the directory exists and that ServUO.exe is present
            string dir = tbServuoDir.Text;
            if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "ServUO.exe")))
            {
                // Start ServUO.exe
                Process.Start(Path.Combine(dir, "ServUO.exe"));
            }
            else
            {
                MessageBox.Show("The directory or ServUO.exe does not exist. Please select a valid directory.");
            }
        }
        #endregion

        #region btSetExe
        private void btSetExe_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Executable files (*.exe)|*.exe";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the selected path in Settings
                    Properties.Settings.Default.ExecutablePath = dialog.FileName;
                    Properties.Settings.Default.Save();
                }
            }
        }
        #endregion

        #region btOtherServer_Click
        private void btOtherServer_Click(object sender, EventArgs e)
        {
            // Make sure the .exe file exists
            string exePath = Properties.Settings.Default.ExecutablePath;
            if (File.Exists(exePath))
            {
                // Start the .exe file
                Process.Start(exePath);
            }
            else
            {
                MessageBox.Show("The .exe file does not exist. Please select a valid file.");
            }
        }
        #endregion

        #region btOpenServUoDir
        private void btOpenServUoDir_Click(object sender, EventArgs e)
        {
            string dir = tbServuoDir.Text;
            if (Directory.Exists(dir))
            {
                // Open the directory with Windows Explorer
                Process.Start("explorer.exe", dir);
            }
            else
            {
                MessageBox.Show("The directory does not exist. Please select a valid directory.");
            }
        }
        #endregion

        #region btServUOsln_Click
        private void btServUOsln_Click(object sender, EventArgs e)
        {
            string dir = tbServuoDir.Text;
            string solutionPath = Path.Combine(dir, "ServUO.sln");

            // Path to your Visual Studio installation
            string visualStudioPath = Path.Combine(Properties.Settings.Default.ServUOsln, "devenv.exe");

            if (File.Exists(solutionPath) && File.Exists(visualStudioPath))
            {
                // Start Visual Studio with the ServUO.sln as an argument
                Process.Start(visualStudioPath, $"\"{solutionPath}\"");
            }
            else
            {
                MessageBox.Show("The ServUo.sln or Visual Studio installation does not exist. Please select valid paths.");
            }
        }
        #endregion

        private void btSetServUOsln_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the selected path to Settingsungen
                    Properties.Settings.Default.ServUOsln = dialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        
    }
}

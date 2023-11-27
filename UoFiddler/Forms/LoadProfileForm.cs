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
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Classes;

namespace UoFiddler.Forms
{
    public partial class LoadProfileForm : Form
    {
        private readonly string[] _profiles;

        public LoadProfileForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _profiles = GetProfiles();
            FiddlerOptions.Logger.Information("Found profiles: {Profiles}", _profiles);
            foreach (string profile in _profiles)
            {
                string name = profile.Substring(8);
                comboBoxLoad.Items.Add(name);
                comboBoxBasedOn.Items.Add(name);
            }
            string lastSelectedProfile = Properties.Settings.Default.LastSelectedProfile; //Profile Load
            int index = Array.IndexOf(_profiles, lastSelectedProfile);
            comboBoxLoad.SelectedIndex = index != -1 ? index : 0;
            comboBoxBasedOn.SelectedIndex = 0;
        }

        private static string[] GetProfiles()
        {
            string[] files = Directory.GetFiles(Options.AppDataPath, "Options_*.xml", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                string[] path = files[i].Split(Path.DirectorySeparatorChar);
                files[i] = path[path.Length - 1];
                files[i] = files[i].Substring(0, files[i].Length - 4);
            }

            return files;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            LoadSelectedProfile();
            Properties.Settings.Default.LastSelectedProfile = _profiles[comboBoxLoad.SelectedIndex];
            Properties.Settings.Default.Save(); //Profile Save Propeties
        }

        private void LoadSelectedProfile()
        {
            if (comboBoxLoad.SelectedIndex == -1)
            {
                return;
            }

            Options.ProfileName = $"{_profiles[comboBoxLoad.SelectedIndex]}.xml";
            FiddlerOptions.Logger.Information("Loading profile: {ProfileName}", Options.ProfileName);
            FiddlerOptions.LoadProfile($"{_profiles[comboBoxLoad.SelectedIndex]}.xml");

            Close();
        }

        private void OnClickCreate(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCreate.Text))
            {
                MessageBox.Show("Profile name is missing", "New Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Options.ProfileName = $"Options_{textBoxCreate.Text}.xml";
            FiddlerOptions.Logger.Information("Creating profile: {ProfileName}", Options.ProfileName);
            FiddlerOptions.LoadProfile($"{_profiles[comboBoxBasedOn.SelectedIndex]}.xml");

            Close();
        }

        private void ComboBoxLoad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadSelectedProfile();
            }
        }

        private void ComboBoxLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }

        private void ComboBoxLoad_KeyUp(object sender, KeyEventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }

        private void LoadProfile_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                Application.Exit();
            }
        }

        #region Delete Entry
        private void bt_Delete_List_Click(object sender, EventArgs e)
        {
            if (comboBoxBasedOn.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an entry to delete..", "Delete Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string profileName = comboBoxBasedOn.SelectedItem.ToString();
                string profilePath = Path.Combine(Options.AppDataPath, $"Options_{profileName}.xml");
                if (File.Exists(profilePath))
                {
                    File.Delete(profilePath);
                }
                comboBoxBasedOn.Items.RemoveAt(comboBoxBasedOn.SelectedIndex);
            }
        }
        #endregion

        public class ProfileManager
{
    private const string LastSelectedProfileKey = "LastSelectedProfile";

    public string LastSelectedProfile
    {
        get
        {
            if (Properties.Settings.Default[LastSelectedProfileKey] is string profile)
            {
                return profile;
            }

            return null;
        }
        set
        {
            Properties.Settings.Default[LastSelectedProfileKey] = value;
            Properties.Settings.Default.Save();
        }
    }
}

    }
}

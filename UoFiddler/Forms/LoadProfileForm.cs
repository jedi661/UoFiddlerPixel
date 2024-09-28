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
using System.Runtime.InteropServices;
using System.Media;

namespace UoFiddler.Forms
{
    public partial class LoadProfileForm : Form
    {
        private readonly string[] _profiles;

        private const int HOTKEY_ID = 1;

        [DllImport("user32.dll")] // Register the hotkey (Ctrl + Alt + P)
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")] // Register the hotkey (Ctrl + Alt + P)
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


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
            
            RegisterHotKey(this.Handle, HOTKEY_ID, 0x0003, (uint)Keys.P); // Register the hotkey (Ctrl + Alt + P)
        }

        #region [ WndProc ]
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                BringToFrontAndCenter();
            }
            base.WndProc(ref m);
        }
        #endregion

        #region [ BringToFrontAndCenter ]
        private void BringToFrontAndCenter()
        {
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BringToFront();
            this.Activate();

            // Play a sound effect after settings have been inserted
            SoundPlayer player = new SoundPlayer();
            player.SoundLocation = "sound.wav";
            player.Play();
        }
        #endregion

        #region [ OnFormClosed ]
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosed(e);
        }
        #endregion

        #region [ GetProfiles ]
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
        #endregion

        #region [ OnClickLoad ]
        private void OnClickLoad(object sender, EventArgs e)
        {
            LoadSelectedProfile();
            Properties.Settings.Default.LastSelectedProfile = _profiles[comboBoxLoad.SelectedIndex];
            Properties.Settings.Default.Save(); //Profile Save Propeties
        }
        #endregion

        #region [ LoadSelectedProfile ]
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
        #endregion

        #region [ OnClickCreate ]
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
        #endregion

        #region [ ComboBoxLoad_KeyDown ]
        private void ComboBoxLoad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadSelectedProfile();
            }
        }
        #endregion

        #region [ ComboBoxLoad_SelectedIndexChanged ]
        private void ComboBoxLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }
        #endregion

        #region [ ComboBoxLoad_KeyUp ]
        private void ComboBoxLoad_KeyUp(object sender, KeyEventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }
        #endregion

        #region [ LoadProfile_FormClosed ]
        private void LoadProfile_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                Application.Exit();
            }
        }
        #endregion

        #region [ Delete Entry ]
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

        #region [ class ProfileManager ]
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
        #endregion
    }
}

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;

namespace UoFiddler.Forms
{
    public partial class MainForm : Form
    {
        //Bin_Dec_Hex_ConverterForm
        private Bin_Dec_Hex_ConverterForm binDecHexConverterForm;

        //AlarmClock
        private AlarmClockForm alarmClockForm;
        public MainForm()
        {
            InitializeComponent();

            //Orginal
            /*if (FiddlerOptions.StoreFormState)
            {
                if (FiddlerOptions.MaximisedForm)
                {
                    StartPosition = FormStartPosition.Manual;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (IsOkFormStateLocation(FiddlerOptions.FormPosition, FiddlerOptions.FormSize))
                    {
                        StartPosition = FormStartPosition.Manual; 
                        WindowState = FormWindowState.Normal;
                        Location = FiddlerOptions.FormPosition;
                        Size = FiddlerOptions.FormSize;
                    }
                }
            }*/

            // Original
            if (FiddlerOptions.StoreFormState)
            {
                if (FiddlerOptions.MaximisedForm)
                {
                    StartPosition = FormStartPosition.Manual;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (IsOkFormStateLocation(FiddlerOptions.FormPosition, FiddlerOptions.FormSize))
                    {
                        StartPosition = FormStartPosition.Manual;
                        WindowState = FormWindowState.Normal;
                        Location = FiddlerOptions.FormPosition;
                        Size = FiddlerOptions.FormSize;
                    }
                }
            }

            // Ohne IsOkFormStateLocation
            /*if (FiddlerOptions.StoreFormState)
            {
                if (FiddlerOptions.MaximisedForm)
                {
                    StartPosition = FormStartPosition.Manual;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    StartPosition = FormStartPosition.Manual;
                    WindowState = FormWindowState.Normal;
                    Location = FiddlerOptions.FormPosition;
                    Size = FiddlerOptions.FormSize;
                }
            }*/

            // Please define the desired order of the tabs.
            string[] tabOrder = new string[] { "StartTab", "ItemsTab", "GumpsTab", "DressTab", "TileDataTab", "LandTilesTab", "TextureTab", "MapTab", "MultiMapTab", "MultisTab", "RadarColTab", "HuesTab", "AnimationTab", "AnimDataTab", "LightTab", "SoundsTab", "SkillsTab", "SkillGrpTab", "SpeechTab", "ClilocTab", "FontsTab", };

            // Create a new list of TabPages.
            List<TabPage> orderedPages = new List<TabPage>();

            // Add the TabPages to the list in the desired order.
            foreach (string tabName in tabOrder)
            {
                TabPage tabPage = TabPanel.TabPages[tabName];
                orderedPages.Add(tabPage);
            }

            // Remove all TabPages from the TabPanel.
            TabPanel.TabPages.Clear();

            // Add the newly ordered TabPages to the TabPanel.
            foreach (TabPage tabPage in orderedPages)
            {
                TabPanel.TabPages.Add(tabPage);
            }

            // Set the DrawMode property of the TabPanel control
            TabPanel.DrawMode = TabDrawMode.OwnerDrawFixed;
            // Register the TabPanel_DrawItem method as an event handler for the DrawItem event of the TabPanel control
            TabPanel.DrawItem += TabPanel_DrawItem;

            // Icon
            Icon = Options.GetFiddlerIcon();
            // Version
            Versionlabel.Text = $"Version {FiddlerOptions.AppVersion.Major}.{FiddlerOptions.AppVersion.Minor}.{FiddlerOptions.AppVersion.Build}";
            Versionlabel.Left = StartTab.Size.Width - Versionlabel.Width - 5;
            // Load Plugins
            LoadExternToolStripMenu();
            GlobalPlugins.Plugins.FindPlugins($@"{Application.StartupPath}\plugins");

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            foreach (AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                if (!plug.Loaded)
                {
                    continue;
                }

                plug.Instance.ModifyPluginToolStrip(toolStripDropDownButtonPlugins);
                plug.Instance.ModifyTabPages(TabPanel);
            }

            foreach (TabPage tab in TabPanel.TabPages)
            {
                if ((int)tab.Tag >= 0 && (int)tab.Tag < Options.ChangedViewState.Count &&
                    !Options.ChangedViewState[(int)tab.Tag])
                {
                    ToggleView(tab);
                }
            }

            // Add the available images to the toolStripComboBoxImage.
            toolStripComboBoxImage.Items.AddRange(new[] { "UOFiddler", "UOFiddler1", "UOFiddler2", "UOFiddler3", "UOFiddler4", "UOFiddler5", "UOFiddler6", "UOFiddler7", "UOFiddler8", "UOFiddler9", "UOFiddler10", "UOFiddler11", "UOFiddler12" });

            // Register the event handler for the SelectedIndexChanged event of the toolStripComboBoxImage.
            toolStripComboBoxImage.SelectedIndexChanged += ImageSwitcher_SelectedIndexChanged;

            // Load the saved user selection and set the background image accordingly.
            var selectedImage = Properties.Settings.Default.SelectedImage;
            switch (selectedImage)
            {
                case "UOFiddler":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler; // Set the background image of the StartTab to UOFiddler.
                    break;
                case "UOFiddler1":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler1; // Set the background image of the StartTab to UOFiddler1
                    break;
                case "UOFiddler2":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler2; // Set the background image of the StartTab to UOFiddler2
                    break;
                case "UOFiddler3":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler3; // Set the background image of the StartTab to UOFiddler3
                    break;
                case "UOFiddler4":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler4; // Set the background image of the StartTab to UOFiddler4
                    break;
                case "UOFiddler5":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler5; // Set the background image of the StartTab to UOFiddler5
                    break;
                case "UOFiddler6":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler6; // Set the background image of the StartTab to UOFiddler6
                    break;
                case "UOFiddler7":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler7; // Set the background image of the StartTab to UOFiddler7
                    break;
                case "UOFiddler8":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler8; // Set the background image of the StartTab to UOFiddler8
                    break;
                case "UOFiddler9":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler9; // Set the background image of the StartTab to UOFiddler9
                    break;
                case "UOFiddler10":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler10; // Set the background image of the StartTab to UOFiddler10
                    break;
                case "UOFiddler11":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler11; // Set the background image of the StartTab to UOFiddler11
                    break;
                case "UOFiddler12":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler12; // Set the background image of the StartTab to UOFiddler12
                    break;
            }


            // AlarmClock Set Pos User.config
            if (Properties.Settings.Default.FormLocationAlarm != Point.Empty)
            {
                this.Location = Properties.Settings.Default.FormLocationAlarm;
            }

            /*
            // MainFormPos
            if (Properties.Settings.Default.MainFormPos != Point.Empty)
            {
                this.Location = Properties.Settings.Default.MainFormPos;
            }

            // MainFormSize
            if (Properties.Settings.Default.MainFormSize != Size.Empty)
            {
                this.Size = Properties.Settings.Default.MainFormSize;
            }

            // MainFormMaximized
            if (Properties.Settings.Default.MainFormMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            */
        }

        #region TabPanel_DrawItem => tab design
        // The TabPanel_DrawItem method is an event handler for the DrawItem event of the TabPanel control
        private void TabPanel_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the Graphics object to draw the tabs
            Graphics g = e.Graphics;
            // Create a brush to draw the text of the tabs
            Brush textBrush = new SolidBrush(TabPanel.ForeColor);
            // Get the current TabPage
            TabPage tabPage = TabPanel.TabPages[e.Index];
            // Get the bounds of the current TabPage
            Rectangle tabBounds = TabPanel.GetTabRect(e.Index);
            // Select the background color based on the name of the tab

            // Check if the current tab is selected.
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Set the background color based on the selection status.
            Color backColor = isSelected ? Color.LightBlue : TabPanel.BackColor;

            // Highlight color for selected tabs.
            Color highlightColor = Color.Yellow;

            // Check if the tab is selected to apply the highlight.
            if (isSelected)
            {
                // Draw a highlight around the tab area.
                using (Pen highlightPen = new Pen(highlightColor, 2))
                {
                    g.DrawRectangle(highlightPen, tabBounds);
                }
            }


            //Color backColor;
            switch (tabPage.Name)
            {
                case "MapTab":
                case "TextureTab":
                case "LandTilesTab":
                case "MultiMapTab":
                    backColor = Color.LightGreen;
                    break;
                case "ItemsTab":
                case "GumpsTab":
                case "DressTab":
                case "TileDataTab":
                    backColor = Color.LightBlue;
                    break;
                case "MultisTab":
                case "RadarColTab":
                case "HuesTab":
                    backColor = Color.Orange;
                    break;
                case "AnimationTab":
                case "AnimDataTab":
                    backColor = Color.LightCoral;
                    break;
                case "SoundsTab":
                case "LightTab":
                case "SkillsTab":
                case "SkillGrpTab":
                case "ClilocTab":
                case "FontsTab":
                case "SpeechTab":
                    backColor = Color.White;
                    break;
                    /*default:
                        backColor = TabPanel.BackColor;
                        //backColor = Color.Red;
                        break;*/
            }
            // Fill the background of the current TabPage with the selected color
            g.FillRectangle(new SolidBrush(backColor), e.Bounds);
            // Create a StringFormat object to center the text in the TabPage
            StringFormat stringFlags = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            // Draw the text of the current TabPage
            g.DrawString(tabPage.Text, TabPanel.Font, textBrush, tabBounds, stringFlags);
            // Dispose of the brush
            textBrush.Dispose();
        }
        #endregion

        #region PathSettingsForm
        private PathSettingsForm _pathSettingsForm = new PathSettingsForm();

        private void Click_path(object sender, EventArgs e)
        {
            if (_pathSettingsForm.IsDisposed)
            {
                _pathSettingsForm = new PathSettingsForm();
            }
            else
            {
                _pathSettingsForm.Focus();
            }

            _pathSettingsForm.TopMost = true;
            _pathSettingsForm.Show();
        }
        #endregion

        #region OnClickAlwaysTop
        private void OnClickAlwaysTop(object sender, EventArgs e)
        {
            TopMost = AlwaysOnTopMenuitem.Checked;
            ControlEvents.FireAlwaysOnTopChangeEvent(TopMost);
        }
        #endregion

        #region Reload Files
        private void ReloadFiles(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Verdata.Initialize();

            if (Options.LoadedUltimaClass["Art"] || Options.LoadedUltimaClass["TileData"])
            {
                // Looks like we have to reload art first to have proper tiledata loading
                // and order here is important
                Art.Reload();
                TileData.Initialize();
            }

            if (Options.LoadedUltimaClass["Hues"])
            {
                Hues.Initialize();
            }

            if (Options.LoadedUltimaClass["ASCIIFont"])
            {
                AsciiText.Initialize();
            }

            if (Options.LoadedUltimaClass["UnicodeFont"])
            {
                UnicodeFonts.Initialize();
            }

            if (Options.LoadedUltimaClass["Animdata"])
            {
                Animdata.Initialize();
            }

            if (Options.LoadedUltimaClass["Light"])
            {
                Light.Reload();
            }

            if (Options.LoadedUltimaClass["Skills"])
            {
                Skills.Reload();
            }

            if (Options.LoadedUltimaClass["Sound"])
            {
                Sounds.Initialize();
            }

            if (Options.LoadedUltimaClass["Texture"])
            {
                Textures.Reload();
            }

            if (Options.LoadedUltimaClass["Gumps"])
            {
                Gumps.Reload();
            }

            if (Options.LoadedUltimaClass["Animations"])
            {
                Animations.Reload();
            }

            if (Options.LoadedUltimaClass["RadarColor"])
            {
                RadarCol.Initialize();
            }

            if (Options.LoadedUltimaClass["Map"])
            {
                MapHelper.CheckForNewMapSize();
                Map.Reload();
            }

            if (Options.LoadedUltimaClass["Multis"])
            {
                Multis.Reload();
            }

            if (Options.LoadedUltimaClass["Speech"])
            {
                SpeechList.Initialize();
            }

            if (Options.LoadedUltimaClass["AnimationEdit"])
            {
                AnimationEdit.Reload();
            }

            ControlEvents.FireFilePathChangeEvent();

            Cursor.Current = Cursors.Default;
        }
        #endregion

        /// <summary>
        /// Reloads the Extern Tools DropDown <see cref="FiddlerOptions.ExternTools"/>
        /// </summary>
        public void LoadExternToolStripMenu()
        {
            ExternToolsDropDown.DropDownItems.Clear();

            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "Manage.."
            };
            item.Click += OnClickToolManage;

            ExternToolsDropDown.DropDownItems.Add(item);
            ExternToolsDropDown.DropDownItems.Add(new ToolStripSeparator());

            if (FiddlerOptions.ExternTools is null)
            {
                return;
            }

            for (int i = 0; i < FiddlerOptions.ExternTools.Count; i++)
            {
                ExternTool tool = FiddlerOptions.ExternTools[i];
                item = new ToolStripMenuItem
                {
                    Text = tool.Name,
                    Tag = i
                };

                item.DropDownItemClicked += ExternTool_ItemClicked;

                ToolStripMenuItem sub = new ToolStripMenuItem
                {
                    Text = "Start",
                    Tag = -1
                };

                item.DropDownItems.Add(sub);
                item.DropDownItems.Add(new ToolStripSeparator());

                for (int j = 0; j < tool.Args.Count; j++)
                {
                    ToolStripMenuItem arg = new ToolStripMenuItem
                    {
                        Text = tool.ArgsName[j],
                        Tag = j
                    };
                    item.DropDownItems.Add(arg);
                }

                ExternToolsDropDown.DropDownItems.Add(item);
            }
        }

        #region ExternTool
        private static void ExternTool_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int argInfo = (int)e.ClickedItem.Tag;
            int toolInfo = (int)e.ClickedItem.OwnerItem.Tag;

            if (toolInfo < 0 || argInfo < -1)
            {
                return;
            }

            using (Process process = new Process())
            {
                ExternTool tool = FiddlerOptions.ExternTools[toolInfo];
                process.StartInfo.FileName = tool.FileName;
                if (argInfo >= 0)
                {
                    process.StartInfo.Arguments = tool.Args[argInfo];
                }

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error starting tool",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }
        #endregion

        #region ManageToolsForm
        private ManageToolsForm _manageForm;

        private void OnClickToolManage(object sender, EventArgs e)
        {
            if (_manageForm?.IsDisposed == false)
            {
                return;
            }

            _manageForm = new ManageToolsForm(LoadExternToolStripMenu)
            {
                TopMost = true
            };
            _manageForm.Show();
        }
        #endregion

        #region OptionsForm
        private OptionsForm _optionsForm;

        private void OnClickOptions(object sender, EventArgs e)
        {
            if (_optionsForm?.IsDisposed == false)
            {
                return;
            }

            _optionsForm = new OptionsForm(
                UpdateAllTileViews,
                UpdateItemsTab,
                UpdateSoundTab,
                UpdateMapTab)
            {
                TopMost = true
            };
            _optionsForm.Show();
        }
        #endregion

        /// <summary>
        /// Updates all tile view tabs
        /// </summary>
        private void UpdateAllTileViews()
        {
            UpdateItemsTab();
            UpdateLandTilesTab();
            UpdateTexturesTab();
            UpdateFontsTab();
        }

        /// <summary>
        /// Updates Item tab
        /// </summary>
        private void UpdateItemsTab()
        {
            itemShowControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Land tiles tab
        /// </summary>
        private void UpdateLandTilesTab()
        {
            landTilesControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Textures tab
        /// </summary>
        private void UpdateTexturesTab()
        {
            textureControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Fonts tab
        /// </summary>
        private void UpdateFontsTab()
        {
            fontsControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Map tab
        /// </summary>
        private void UpdateMapTab()
        {
            if (Options.LoadedUltimaClass["Map"])
            {
                Map.Reload();
            }

            ControlEvents.FireMapSizeChangeEvent();
        }

        /// <summary>
        /// Updates Sounds tab
        /// </summary>
        private void UpdateSoundTab()
        {
            soundControl.Reload();
        }

        #region Dock and Undock
        private void OnClickUnDock(object sender, EventArgs e)
        {
            int tag = (int)TabPanel.SelectedTab.Tag;
            if (tag <= 0)
            {
                return;
            }

            new UnDockedForm(TabPanel.SelectedTab, ReDock).Show();
            TabPanel.TabPages.Remove(TabPanel.SelectedTab);
        }

        /// <summary>
        /// ReDocks closed Form
        /// </summary>
        /// <param name="oldTab"></param>
        public void ReDock(TabPage oldTab)
        {
            bool done = false;
            foreach (TabPage page in TabPanel.TabPages)
            {
                if ((int)page.Tag <= (int)oldTab.Tag)
                {
                    continue;
                }

                TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), oldTab);
                done = true;
                break;
            }

            if (!done)
            {
                TabPanel.TabPages.Add(oldTab);
            }

            TabPanel.SelectedTab = oldTab;
        }
        #endregion

        #region ManagePlugins
        private ManagePluginsForm _pluginsFormForm;

        private void OnClickManagePlugins(object sender, EventArgs e)
        {
            if (_pluginsFormForm?.IsDisposed == false)
            {
                return;
            }

            _pluginsFormForm = new ManagePluginsForm
            {
                TopMost = true
            };
            _pluginsFormForm.Show();
        }
        #endregion

        #region OnClosing
        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            FiddlerOptions.Logger.Information("MainForm - OnClosing - start");
            string files = Options.ChangedUltimaClass
                                    .Where(key => key.Value)
                                    .Aggregate(string.Empty, (current, key) => current + $"- {key.Key} \r\n");

            if (files.Length > 0)
            {
                DialogResult result =
                    MessageBox.Show($"Are you sure you want to quit?\r\n\r\nThere are unsaved files:\r\n{files}",
                        "UnSaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            /*
            // Speichern Sie die Position des MainForm-Fensters
            Properties.Settings.Default.MainFormPos = this.Location;

            //Speichert Size
            Properties.Settings.Default.MainFormSize = this.Size;
            //Speichert das Maximiert
            Properties.Settings.Default.MainFormMaximized = (this.WindowState == FormWindowState.Maximized);
            // Speichern Sie die Einstellungen in die User.Config
            Properties.Settings.Default.Save();
            //ende
            */

            FiddlerOptions.MaximisedForm = WindowState == FormWindowState.Maximized;
            FiddlerOptions.FormPosition = this.Location;
            FiddlerOptions.FormSize = this.Size;

            FiddlerOptions.Logger.Information("MainForm - OnClosing - unloading plugins");
            GlobalPlugins.Plugins.ClosePlugins();

            FiddlerOptions.Logger.Information("MainForm - OnClosing - done");
        }
        #endregion

        #region IsOkFormStateLocation
        // orginal
        /*private static bool IsOkFormStateLocation(Point loc, Size size)
        {
            if (loc.X < 0 || loc.Y < 0)
            {
                return false;
            }

            if (loc.X + size.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                return false;
            }

            return loc.Y + size.Height <= Screen.PrimaryScreen.WorkingArea.Height;
        }*/

        /*private bool IsOkFormStateLocation(Point loc, Size size)
        {
            int maxX = Screen.PrimaryScreen.WorkingArea.Width;
            int maxY = Screen.PrimaryScreen.WorkingArea.Height;

            // Überprüfen Sie die X-Koordinate
            // Check the X coordinate
            if (loc.X < 0 || loc.X + size.Width > maxX)
            {
                loc.X = Math.Max(0, maxX - size.Width);
            }

            // Überprüfen Sie die Y-Koordinate
            // Check the Y coordinate
            if (loc.Y < 0 || loc.Y + size.Height > maxY)
            {
                loc.Y = Math.Max(0, maxY - size.Height);
            }

            // Überprüfen Sie auch die Größe
            // Also check the size
            if (size.Width <= Screen.PrimaryScreen.WorkingArea.Width &&
                size.Height <= Screen.PrimaryScreen.WorkingArea.Height)
            {
                // Die Größe ist in Ordnung
                // The size is okay
            }
            else
            {
                // Die Größe ist zu groß, passen Sie sie an
                // The size is too big, adjust it
                size = new Size(
                    Math.Min(size.Width, Screen.PrimaryScreen.WorkingArea.Width),
                    Math.Min(size.Height, Screen.PrimaryScreen.WorkingArea.Height)
                );
            }

            // Überprüfen Sie, ob die Position und Größe gültig sind und geben Sie das Ergebnis zurück
            // Check if the position and size are valid and return the result
            bool isValid = (loc.X >= 0 &&
                            loc.Y >= 0 &&
                            loc.X + size.Width <= maxX &&
                            loc.Y + size.Height <= maxY);

            if (!isValid)
            {
                throw new Exception("Die Position oder Größe des Fensters ist ungültig.");
                // Throw an exception if the position or size of the window is invalid.
            }

            return isValid;
        }*/

        private bool IsOkFormStateLocation(Point loc, Size size)
        {
            int maxX = Screen.PrimaryScreen.WorkingArea.Width;
            int maxY = Screen.PrimaryScreen.WorkingArea.Height;

            // Adjust the X and Y coordinates
            // Passen Sie die X- und Y-Koordinaten an
            loc = AdjustCoordinates(loc, size, maxX, maxY);

            // Adjust the size
            // Passen Sie die Größe an
            size = AdjustSize(size);

            // Check if the position and size are valid and return the result
            // Überprüfen Sie, ob die Position und Größe gültig sind und geben Sie das Ergebnis zurück
            bool isValid = (loc.X >= 0 &&
                            loc.Y >= 0 &&
                            loc.X + size.Width <= maxX &&
                            loc.Y + size.Height <= maxY);

            if (!isValid)
            {
                throw new Exception("Die Position oder Größe des Fensters ist ungültig.");
                // Throw an exception if the position or size of the window is invalid.
            }

            return isValid;
        }

        private Point AdjustCoordinates(Point loc, Size size, int maxX, int maxY)
        {
            // Check the X coordinate
            // Überprüfen Sie die X-Koordinate
            if (loc.X < 0 || loc.X + size.Width > maxX)
            {
                loc.X = Math.Max(0, maxX - size.Width);
            }

            // Check the Y coordinate
            // Überprüfen Sie die Y-Koordinate
            if (loc.Y < 0 || loc.Y + size.Height > maxY)
            {
                loc.Y = Math.Max(0, maxY - size.Height);
            }

            return loc;
        }

        private Size AdjustSize(Size size)
        {
            // Also check the size
            // Überprüfen Sie auch die Größe
            if (size.Width > Screen.PrimaryScreen.WorkingArea.Width ||
                size.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                // The size is too big, adjust it
                // Die Größe ist zu groß, passen Sie sie an
                size = new Size(
                    Math.Min(size.Width, Screen.PrimaryScreen.WorkingArea.Width),
                    Math.Min(size.Height, Screen.PrimaryScreen.WorkingArea.Height)
                );
            }

            return size;
        }

        #endregion

        #region View Tab List
        private void ToggleView(object sender, EventArgs e)
        {
            ToolStripMenuItem theMenuItem = (ToolStripMenuItem)sender;
            TabPage thePage = TabFromTag((int)theMenuItem.Tag);

            int tag = (int)thePage.Tag;

            if (theMenuItem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thePage))
                {
                    return;
                }

                theMenuItem.Checked = false;
                TabPanel.TabPages.Remove(thePage);
                Options.ChangedViewState[tag] = false;
            }
            else
            {
                theMenuItem.Checked = true;
                bool done = false;
                foreach (TabPage page in TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }

                if (!done)
                {
                    TabPanel.TabPages.Add(thePage);
                }

                Options.ChangedViewState[tag] = true;
            }
        }

        private void ToggleView(TabPage thePage)
        {
            int tag = (int)thePage.Tag;
            ToolStripMenuItem theMenuItem = MenuFromTag(tag);

            if (theMenuItem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thePage))
                {
                    return;
                }

                theMenuItem.Checked = false;
                TabPanel.TabPages.Remove(thePage);
                Options.ChangedViewState[tag] = false;
            }
            else
            {
                theMenuItem.Checked = true;
                bool done = false;
                foreach (TabPage page in TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }

                if (!done)
                {
                    TabPanel.TabPages.Add(thePage);
                }

                Options.ChangedViewState[tag] = true;
            }
        }


        private TabPage TabFromTag(int tag)
        {
            switch (tag)
            {
                case 0: return StartTab;
                case 1: return MultisTab;
                case 2: return AnimationTab;
                case 3: return ItemsTab;
                case 4: return LandTilesTab;
                case 5: return TextureTab;
                case 6: return GumpsTab;
                case 7: return SoundsTab;
                case 8: return HuesTab;
                case 9: return FontsTab;
                case 10: return ClilocTab;
                case 11: return MapTab;
                case 12: return LightTab;
                case 13: return SpeechTab;
                case 14: return SkillsTab;
                case 15: return AnimDataTab;
                case 16: return MultiMapTab;
                case 17: return DressTab;
                case 18: return TileDataTab;
                case 19: return RadarColTab;
                case 20: return SkillGrpTab;
                default: return StartTab;
            }
        }


        private ToolStripMenuItem MenuFromTag(int tag)
        {
            switch (tag)
            {
                case 0: return ToggleViewStart;
                case 1: return ToggleViewMulti;
                case 2: return ToggleViewAnimations;
                case 3: return ToggleViewItems;
                case 4: return ToggleViewLandTiles;
                case 5: return ToggleViewTexture;
                case 6: return ToggleViewGumps;
                case 7: return ToggleViewSounds;
                case 8: return ToggleViewHue;
                case 9: return ToggleViewFonts;
                case 10: return ToggleViewCliloc;
                case 11: return ToggleViewMap;
                case 12: return ToggleViewLight;
                case 13: return ToggleViewSpeech;
                case 14: return ToggleViewSkills;
                case 15: return ToggleViewAnimData;
                case 16: return ToggleViewMultiMap;
                case 17: return ToggleViewDress;
                case 18: return ToggleViewTileData;
                case 19: return ToggleViewRadarColor;
                case 20: return ToggleViewSkillGrp;
                default: return ToggleViewStart;
            }
        }
        #endregion

        #region Polserver Link
        private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
        {
            /*Process.Start(new ProcessStartInfo
            {
                FileName = "http://uofiddler.polserver.com/help.html",
                UseShellExecute = true
            });*/

            using (HelpDokuForm helpDokuForm = new HelpDokuForm())
            {
                helpDokuForm.ShowDialog();
            }
        }
        #endregion

        #region About
        private void ToolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            using (AboutBoxForm aboutBoxForm = new AboutBoxForm())
            {
                aboutBoxForm.ShowDialog(this);
            }
        }
        #endregion

        #region Changelog
        private void changelogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (ChangeLogForm changelogForm = new ChangeLogForm())
            {
                changelogForm.ShowDialog(this);
            }
        }
        #endregion

        #region Delete WebView Cache
        private void HelpDokuForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Get the path to the %LOCALAPPDATA% directory
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            // Create the path to the UoFiddler.exe.WebView2 folder in the %LOCALAPPDATA% directory
            string userDataFolder = Path.Combine(localAppData, "UoFiddler.exe.WebView2");
            // Check if the UoFiddler.exe.WebView2 folder exists
            if (Directory.Exists(userDataFolder))
            {
                // Delete the UoFiddler.exe.WebView2 folder
                Directory.Delete(userDataFolder, true);
            }
        }
        #endregion

        #region Image Switch
        private void toolStripComboBoxImage_Click(object sender, EventArgs e)
        {
            // Add the available images to the toolStripComboBoxImage.
            toolStripComboBoxImage.Items.AddRange(new[] { "UOFiddler", "UOFiddler1", "UOFiddler2", "UOFiddler3", "UOFiddler4", "UOFiddler5", "UOFiddler6", "UOFiddler7", "UOFiddler8", "UOFiddler9", "UOFiddler10", "UOFiddler11", "UOFiddler12" });
        }

        // Event handler for the SelectedIndexChanged event.
        private void ImageSwitcher_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedImage = ((ToolStripComboBox)sender).SelectedItem.ToString();

            // Save the user's selection in the user settings.
            Properties.Settings.Default.SelectedImage = selectedImage;
            Properties.Settings.Default.Save();

            switch (selectedImage)
            {
                case "UOFiddler":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler; // Set the background image of the StartTab to UOFiddler
                    break;
                case "UOFiddler1":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler1; // Set the background image of the StartTab to UOFiddler1
                    break;
                case "UOFiddler2":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler2; // Set the background image of the StartTab to UOFiddler2
                    break;
                case "UOFiddler3":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler3; // Set the background image of the StartTab to UOFiddler3
                    break;
                case "UOFiddler4":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler4; // Set the background image of the StartTab to UOFiddler4
                    break;
                case "UOFiddler5":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler5; // Set the background image of the StartTab to UOFiddler5.
                    break;
                case "UOFiddler6":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler6; // Set the background image of the StartTab to UOFiddler6.
                    break;
                case "UOFiddler7":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler7; // Set the background image of the StartTab to UOFiddler7.
                    break;
                case "UOFiddler8":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler8; // Set the background image of the StartTab to UOFiddler8.
                    break;
                case "UOFiddler9":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler9; // Set the background image of the StartTab to UOFiddler9.
                    break;
                case "UOFiddler10":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler10; // Set the background image of the StartTab to UOFiddler10.
                    break;
                case "UOFiddler11":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler11; // Set the background image of the StartTab to UOFiddler11.
                    break;
                case "UOFiddler12":
                    StartTab.BackgroundImage = Properties.Resources.UOFiddler12; // Set the background image of the StartTab to UOFiddler12.
                    break;
            }
        }
        #endregion

        #region Links
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://uo-freeshards.de",
                UseShellExecute = true
            });
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://www.uo-pixel.de",
                UseShellExecute = true
            });
        }

        private void uodevuofreeshardsdeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://uodev.uo-freeshards.de/",
                UseShellExecute = true
            });
        }

        #endregion

        #region Directory
        private void directoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the output path from the Options class
            string path = Options.OutputPath;
            // Start the Windows Explorer process and pass the path as an argument
            // Enclose the path in quotation marks to handle paths with spaces
            System.Diagnostics.Process.Start("explorer.exe", $"\"{path}\"");
        }
        #endregion

        #region DecimalHexConverter
        private void binaryDecimalHexadecimalConverterToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (binDecHexConverterForm == null || binDecHexConverterForm.IsDisposed)
            {
                binDecHexConverterForm = new Bin_Dec_Hex_ConverterForm()
                {
                    TopMost = true
                };
                binDecHexConverterForm.Show();
            }
            else
            {
                binDecHexConverterForm.Focus();
            }
        }
        #endregion

        #region Links Servuo.com and Discord 
        private void toolStripMenuItemLink3_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://www.servuo.com",
                UseShellExecute = true
            });
        }
        private void ToolStripMenuItemDiscordUoFreeshardsDe_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.com/invite/9zpXy43WWT",
                UseShellExecute = true
            });
        }
        #endregion

        #region F1-F12
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Check if any of the F1-F12 keys were pressed
            //if (keyData >= Keys.F1 && keyData <= Keys.F12)
            if ((keyData >= Keys.F1 && keyData <= Keys.F12) || keyData == Keys.PageUp || keyData == Keys.PageDown)
            {
                // Select the tab based on the pressed key
                switch (keyData)
                {
                    case Keys.F1:
                        TabPanel.SelectedIndex = 1; // Items
                        break;
                    case Keys.F2:
                        TabPanel.SelectedIndex = 4; // Tiledata
                        break;
                    case Keys.F3:
                        TabPanel.SelectedIndex = 5; // LandTiles
                        break;
                    case Keys.F4:
                        TabPanel.SelectedIndex = 6; // Texture
                        break;
                    case Keys.F5:
                        TabPanel.SelectedIndex = 7; // Map
                        break;
                    case Keys.F6:
                        TabPanel.SelectedIndex = 2; // Gumps
                        break;
                    case Keys.F7:
                        TabPanel.SelectedIndex = 9; // Multis
                        break;
                    case Keys.F8:
                        TabPanel.SelectedIndex = 10; // Radarcolor
                        break;
                    case Keys.F9:
                        TabPanel.SelectedIndex = 11; // Hues
                        break;
                    case Keys.F10:
                        TabPanel.SelectedIndex = 12; // Animationen
                        break;
                    case Keys.F11:
                        TabPanel.SelectedIndex = 13; // AminData
                        break;
                    case Keys.F12:
                        TabPanel.SelectedIndex = 14; // Light
                        break;
                    case Keys.PageUp:
                        // Scroll to the previous tab
                        if (TabPanel.SelectedIndex > 0)
                        {
                            TabPanel.SelectedIndex--;
                        }
                        break;
                    case Keys.PageDown:
                        // Scroll to the next tab
                        if (TabPanel.SelectedIndex < TabPanel.TabCount - 1)
                        {
                            TabPanel.SelectedIndex++;
                        }
                        break;
                }

                // Prevent further processing of the key
                return true;
            }

            // Call the base class to continue standard processing
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Open Tempdir
        private void tempDirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Application.StartupPath, "tempGrafic");
            if (Directory.Exists(path))
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            else
            {
                MessageBox.Show("The 'tempGrafic' folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region AlarmClock
        private void alarmClockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (alarmClockForm == null || alarmClockForm.IsDisposed)
            {
                alarmClockForm = new AlarmClockForm()
                {
                    TopMost = true
                };
                alarmClockForm.Show();
            }
            else
            {
                alarmClockForm.Focus();
            }
        }
        #endregion

        #region Notepad Editor
        private NotepadForm notepadForm; // Declare an instance of NotepadForm
        private void notPadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (notepadForm == null || notepadForm.IsDisposed)
            {
                notepadForm = new NotepadForm()
                {
                    TopMost = true
                };
                notepadForm.Show();
            }
            else
            {
                notepadForm.Focus();
            }
        }
        #endregion

        #region Notes
        private void NotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Creating a new form
            Form notesForm = new Form()
            {
                Text = "Notes",
                Size = new System.Drawing.Size(400, 300),
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle,
                MaximizeBox = false
            };

            // Creating a RichTextBox
            RichTextBox rtxtNotes = new RichTextBox()
            {
                ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical,
                Dock = System.Windows.Forms.DockStyle.Fill,
                ReadOnly = true
            };

            // the currentNoteIndex variable
            int currentNoteIndex = 0;

            // A method to load the notes from the XML file
            List<string> LoadNotesFromXml()
            {
                // The path to the XML file
                string xmlFilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "NotepadMessage.xml");

                // Creates a list to store the notes
                List<string> notes = new List<string>();

                try
                {
                    // Loads the XML document
                    System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.Load(xmlFilePath);

                    // Loop through each "note" in the XML document
                    foreach (System.Xml.XmlNode noteNode in xmlDoc.SelectNodes("/Notes/Note"))
                    {
                        // Extracting the RTF text from the "Note"
                        string rtfText = noteNode.Attributes["rtfText"].Value;

                        // Adds the RTF text to the list
                        notes.Add(rtfText);
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    rtxtNotes.Text = "The XML file has not yet been created.";
                }

                // Returns the list of notes
                return notes;
            }

            // Loads the notes and adds the first one to the RichTextBox
            List<string> notes = LoadNotesFromXml();
            if (notes.Count > 0)
            {
                rtxtNotes.Rtf = notes[currentNoteIndex];
            }

            // Creates buttons for scrolling
            Button btnScrollUp = new Button() { Text = "Scroll Up", Dock = System.Windows.Forms.DockStyle.Top };
            Button btnScrollDown = new Button() { Text = "Scroll Down", Dock = System.Windows.Forms.DockStyle.Bottom };

            // Add event handlers for the buttons
            btnScrollUp.Click += (s, ev) =>
            {
                if (currentNoteIndex > 0)
                {
                    currentNoteIndex--;
                    rtxtNotes.Rtf = notes[currentNoteIndex];
                }
            };

            btnScrollDown.Click += (s, ev) =>
            {
                if (currentNoteIndex < notes.Count - 1)
                {
                    currentNoteIndex++;
                    rtxtNotes.Rtf = notes[currentNoteIndex];
                }
            };

            // Adds the controls to the form
            notesForm.Controls.Add(rtxtNotes);
            notesForm.Controls.Add(btnScrollUp);
            notesForm.Controls.Add(btnScrollDown);

            // Displays the form
            notesForm.Show();
        }
        #endregion
    }
}